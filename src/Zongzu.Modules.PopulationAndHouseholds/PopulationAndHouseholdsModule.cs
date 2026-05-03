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

        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        bool anyHouseholdChanged = false;
        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            PopulationHouseholdState[] households = scope.State.Households
                .Where(household => household.SettlementId == bundle.SettlementId)
                .OrderBy(static household => household.Id.Value)
                .ToArray();

            if (households.Length == 0)
            {
                continue;
            }

            int distressDelta = ComputeCampaignDistressDelta(bundle, campaign);
            int debtDelta = ComputeCampaignDebtDelta(bundle, campaign);
            int migrationDelta = ComputeCampaignMigrationDelta(bundle, campaign);
            int laborDrop = ComputeCampaignLaborDrop(bundle, campaign);

            foreach (PopulationHouseholdState household in households)
            {
                household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);
                household.DebtPressure = Math.Clamp(household.DebtPressure + debtDelta, 0, 100);
                household.MigrationRisk = Math.Clamp(household.MigrationRisk + migrationDelta, 0, 100);
                household.LaborCapacity = Math.Clamp(household.LaborCapacity - laborDrop, 0, 100);
                household.IsMigrating = ResolveMigrationStatus(household);
                anyHouseholdChanged = true;
            }

            scope.RecordDiff(
                $"{campaign.AnchorSettlementName}战后余波所及，民困增{distressDelta}，债压增{debtDelta}，迁徙增{migrationDelta}，丁力减{laborDrop}。{campaign.LastAftermathSummary}",
                bundle.SettlementId.Value.ToString());

            if (households.Any(static household => household.MigrationRisk >= 80))
            {
                PopulationHouseholdState migratingHousehold = households
                    .OrderByDescending(static household => household.MigrationRisk)
                    .ThenBy(static household => household.Id.Value)
                    .First();
                scope.Emit(PopulationEventNames.MigrationStarted, $"{migratingHousehold.HouseholdName}受战后余波所逼，已有远徙之意。", bundle.SettlementId.Value.ToString());
            }

            if (households.Any(static household => household.DebtPressure >= 85 && household.Distress >= 80))
            {
                PopulationHouseholdState collapsedHousehold = households
                    .OrderByDescending(static household => household.DebtPressure + household.Distress)
                    .ThenBy(static household => household.Id.Value)
                    .First();
                scope.Emit(PopulationEventNames.LivelihoodCollapsed, $"{collapsedHousehold.HouseholdName}受{campaign.AnchorSettlementName}战后余波牵压，生计顿敝。", bundle.SettlementId.Value.ToString());
            }
        }

        if (anyHouseholdChanged)
        {
            SynchronizeMembershipLivelihoodsAndActivities(scope.State);
            RebuildSettlementSummaries(scope.State, scope.TryGetQuery<IPersonRegistryQueries>());
        }
    }

    private static int ComputeCampaignDistressDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignPressureRaised ? 2 : 0;
        delta += bundle.CampaignSupplyStrained ? 3 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 60) / 20;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignDebtDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignPressureRaised ? 1 : 0;
        delta += bundle.CampaignSupplyStrained ? 2 : 0;
        delta += bundle.CampaignAftermathRegistered ? 1 : 0;
        delta += Math.Max(0, 50 - campaign.SupplyState) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignMigrationDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += bundle.CampaignSupplyStrained ? 1 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignLaborDrop(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int drop = bundle.CampaignMobilized ? 1 : 0;
        drop += bundle.CampaignPressureRaised ? 1 : 0;
        drop += bundle.CampaignSupplyStrained ? 2 : 0;
        drop += Math.Max(0, campaign.MobilizedForceCount - 24) / 24;
        return Math.Max(1, drop);
    }

    private static void AdvanceIllnessAndAdjudicateDeaths(ModuleExecutionScope<PopulationAndHouseholdsState> scope)
    {
        if (scope.State.Memberships.Count == 0)
        {
            return;
        }

        IPersonRegistryCommands registryCommands = scope.GetRequiredQuery<IPersonRegistryCommands>();
        Dictionary<HouseholdId, PopulationHouseholdState> householdsById = scope.State.Households
            .ToDictionary(static household => household.Id, static household => household);

        List<HouseholdMembershipState> deceased = new();

        foreach (HouseholdMembershipState membership in scope.State.Memberships.OrderBy(static member => member.PersonId.Value))
        {
            if (!householdsById.TryGetValue(membership.HouseholdId, out PopulationHouseholdState? household))
            {
                continue;
            }

            int distressPressure = household.Distress >= 70 ? 2 : household.Distress >= 45 ? 1 : 0;
            int resilienceBuffer = membership.HealthResilience >= 70 ? 1 : membership.HealthResilience <= 30 ? -1 : 0;
            int healthScore = (int)membership.Health + distressPressure - resilienceBuffer;

            HealthStatus nextHealth = healthScore switch
            {
                <= 1 => HealthStatus.Healthy,
                2 => HealthStatus.Ailing,
                3 => HealthStatus.Ill,
                4 => HealthStatus.Bedridden,
                _ => HealthStatus.Moribund,
            };

            if (nextHealth >= HealthStatus.Ill)
            {
                membership.IllnessMonths = Math.Min(membership.IllnessMonths + 1, 24);
                membership.Activity = PersonActivity.Convalescing;
            }
            else
            {
                membership.IllnessMonths = Math.Max(membership.IllnessMonths - 1, 0);
                if (membership.Activity == PersonActivity.Convalescing && nextHealth == HealthStatus.Healthy)
                {
                    membership.Activity = PersonActivity.Idle;
                }
            }

            membership.Health = nextHealth;

            if (nextHealth == HealthStatus.Moribund && membership.IllnessMonths >= 3)
            {
                if (registryCommands.MarkDeceased(scope.Context, membership.PersonId))
                {
                    scope.Emit(
                        PopulationEventNames.DeathByIllness,
                        $"{household.HouseholdName}一人病殁。",
                        membership.PersonId.Value.ToString());
                    scope.RecordDiff(
                        $"{household.HouseholdName}一人病殁，宅内添一分丧气。",
                        household.Id.Value.ToString());
                    deceased.Add(membership);
                }
            }
        }

        if (deceased.Count > 0)
        {
            foreach (HouseholdMembershipState member in deceased)
            {
                scope.State.Memberships.Remove(member);
                if (householdsById.TryGetValue(member.HouseholdId, out PopulationHouseholdState? household))
                {
                    household.DependentCount = Math.Max(0, household.DependentCount - 1);
                }
            }
        }
    }

}
