using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule : ModuleRunner<PopulationAndHouseholdsState>
{
    private readonly PopulationHouseholdMobilityRulesData _householdMobilityRulesData;

    public PopulationAndHouseholdsModule()
        : this(PopulationHouseholdMobilityRulesData.Default)
    {
    }

    public PopulationAndHouseholdsModule(PopulationHouseholdMobilityRulesData householdMobilityRulesData)
    {
        _householdMobilityRulesData = householdMobilityRulesData
            ?? throw new ArgumentNullException(nameof(householdMobilityRulesData));
    }

    private static readonly string[] CommandNames =
    [
        PlayerCommandNames.RestrictNightTravel,
        PlayerCommandNames.PoolRunnerCompensation,
        PlayerCommandNames.SendHouseholdRoadMessage,
    ];

    private static readonly string[] EventNames =
    [
        PopulationEventNames.HouseholdDebtSpiked,
        PopulationEventNames.MigrationStarted,
        PopulationEventNames.LaborShortage,
        PopulationEventNames.LivelihoodCollapsed,
        PopulationEventNames.DeathByIllness,
        PopulationEventNames.HouseholdSubsistencePressureChanged,
        PopulationEventNames.HouseholdBurdenIncreased,
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
        // Step 1b gap 1: trade shock → household livelihood pressure (no-op dispatch)
        TradeAndIndustryEventNames.RouteBusinessBlocked,
        TradeAndIndustryEventNames.TradeLossOccurred,
        TradeAndIndustryEventNames.TradeDebtDefaulted,
        TradeAndIndustryEventNames.TradeProspered,
        // Renzong thin chain: grain price spike → household subsistence pressure
        TradeAndIndustryEventNames.GrainPriceSpike,
        // Step 1b gap 3: world pulse → labor / livelihood (no-op dispatch)
        WorldSettlementsEventNames.FloodRiskThresholdBreached,
        WorldSettlementsEventNames.CorveeWindowChanged,
        WorldSettlementsEventNames.TaxSeasonOpened,
        // Step 1b gap 4: family branch / heir pressure → household count & sponsor shift (no-op dispatch)
        FamilyCoreEventNames.BranchSeparationApproved,
        FamilyCoreEventNames.HeirSecurityWeakened,
        // Chain 5 thin slice: frontier strain → official supply requisition → household burden
        OfficeAndCareerEventNames.OfficialSupplyRequisition,
    ];



    public override string ModuleKey => KnownModuleKeys.PopulationAndHouseholds;

    public override int ModuleSchemaVersion => 3;

    public override SimulationPhase Phase => SimulationPhase.PopulationPressure;

    public override int ExecutionOrder => 200;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override PopulationAndHouseholdsState CreateInitialState()
    {
        return new PopulationAndHouseholdsState();
    }

    public override void RegisterQueries(PopulationAndHouseholdsState state, QueryRegistry queries)
    {
        queries.Register<IPopulationAndHouseholdsQueries>(new PopulationQueries(state));
    }

    public override void RunXun(ModuleExecutionScope<PopulationAndHouseholdsState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        IPersonRegistryQueries? personQueries = scope.TryGetQuery<IPersonRegistryQueries>();

        foreach (PopulationHouseholdState household in scope.State.Households.OrderBy(static household => household.Id.Value))
        {
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(household.SettlementId);
            int clanSupport = GetClanSupportReserve(familyQueries, household.SponsorClanId);
            ApplyXunPulseAdjustments(scope.Context.CurrentXun, settlement, clanSupport, household);
        }

        SynchronizeMembershipLivelihoodsAndActivities(scope.State);
        RebuildSettlementSummaries(scope.State, personQueries);
    }

    public override void RunMonth(ModuleExecutionScope<PopulationAndHouseholdsState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        IPersonRegistryQueries? personQueries = scope.TryGetQuery<IPersonRegistryQueries>();
        IPersonRegistryCommands? personCommands = scope.TryGetQuery<IPersonRegistryCommands>();

        foreach (PopulationHouseholdState household in scope.State.Households.OrderBy(static household => household.Id.Value))
        {
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(household.SettlementId);
            int clanSupport = GetClanSupportReserve(familyQueries, household.SponsorClanId);

            int oldDebtPressure = household.DebtPressure;
            int oldLaborCapacity = household.LaborCapacity;
            int oldMigrationRisk = household.MigrationRisk;
            bool wasMigrating = household.IsMigrating;
            bool livelihoodCollapseEmitted = false;

            int prosperityPressure = settlement.Prosperity < 50 ? 1 : settlement.Prosperity >= 60 ? -1 : 0;
            int securityPressure = settlement.Security < 45 ? 1 : settlement.Security >= 55 ? -1 : 0;
            int livelihoodPressure = ComputeLivelihoodDistressBaseline(household.Livelihood);
            int relief = clanSupport >= 60 ? 1 : 0;
            int drift = scope.Context.Random.NextInt(-1, 2);

            household.Distress = Math.Clamp(household.Distress + prosperityPressure + securityPressure + livelihoodPressure + drift - relief, 0, 100);
            household.DebtPressure = Math.Clamp(household.DebtPressure + ComputeDebtDelta(household.Distress), 0, 100);
            household.LaborCapacity = Math.Clamp(household.LaborCapacity + ComputeLaborDelta(settlement.Prosperity, household.Distress), 0, 100);
            household.MigrationRisk = Math.Clamp(household.MigrationRisk + ComputeMigrationDelta(settlement.Security, household.Distress), 0, 100);
            household.IsMigrating = ResolveMigrationStatus(household);

            if (TryApplyMonthlyLivelihoodDrift(household, settlement, out LivelihoodDriftResult livelihoodDrift))
            {
                scope.RecordDiff(
                    $"{household.HouseholdName}生计从{RenderLivelihoodForDiff(livelihoodDrift.Previous)}漂向{RenderLivelihoodForDiff(livelihoodDrift.Current)}；{livelihoodDrift.Reason}。",
                    household.Id.Value.ToString());

                if (IsLivelihoodCollapseDrift(livelihoodDrift))
                {
                    scope.Emit(
                        PopulationEventNames.LivelihoodCollapsed,
                        $"{household.HouseholdName}生计从{RenderLivelihoodForDiff(livelihoodDrift.Previous)}坠作{RenderLivelihoodForDiff(livelihoodDrift.Current)}。",
                        household.Id.Value.ToString(),
                        new Dictionary<string, string>
                        {
                            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseSocialPressure,
                            [DomainEventMetadataKeys.SettlementId] = household.SettlementId.Value.ToString(),
                            [DomainEventMetadataKeys.HouseholdId] = household.Id.Value.ToString(),
                            [DomainEventMetadataKeys.Livelihood] = household.Livelihood.ToString(),
                        });
                    livelihoodCollapseEmitted = true;
                }
            }

            scope.RecordDiff(
                $"{household.HouseholdName}本月民困{household.Distress}，债压{household.DebtPressure}，丁力{household.LaborCapacity}，迁徙之念{household.MigrationRisk}。",
                household.Id.Value.ToString());

            if (oldDebtPressure < 70 && household.DebtPressure >= 70)
            {
                scope.Emit(PopulationEventNames.HouseholdDebtSpiked, $"{household.HouseholdName}债压陡起。");
            }

            if (oldLaborCapacity >= 30 && household.LaborCapacity < 30)
            {
                scope.Emit(PopulationEventNames.LaborShortage, $"{household.HouseholdName}丁力不继。");
            }

            if (oldMigrationRisk < 80 && household.MigrationRisk >= 80 && !wasMigrating)
            {
                scope.Emit(PopulationEventNames.MigrationStarted, $"{household.HouseholdName}已起迁徙之念。");
            }

            if (!livelihoodCollapseEmitted && oldDebtPressure < 85 && household.DebtPressure >= 85 && household.Distress >= 80)
            {
                scope.Emit(PopulationEventNames.LivelihoodCollapsed, $"{household.HouseholdName}生计顿敝。");
            }

            if (household.LocalResponseCarryoverMonths > 0)
            {
                household.LocalResponseCarryoverMonths--;
            }
        }

        SynchronizeMembershipLivelihoodsAndActivities(scope.State);
        PromoteHotHouseholdMembers(
            scope.Context,
            scope.State,
            personQueries,
            personCommands,
            _householdMobilityRulesData);
        AdvanceIllnessAndAdjudicateDeaths(scope);

        RebuildSettlementSummaries(scope.State, personQueries);
        if (ApplyMonthlyHouseholdMobilityRuntimeRule(scope))
        {
            SynchronizeMembershipLivelihoodsAndActivities(scope.State);
            RebuildSettlementSummaries(scope.State, personQueries);
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        DispatchTradeShockEvents(scope);
        DispatchWorldPulseEvents(scope);
        DispatchFamilyBranchEvents(scope);
        DispatchOfficeSupplyEvents(scope);

        ApplyWarfareCampaignAftermathEvents(scope);
    }

}
