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

    private static void ApplyXunPulseAdjustments(
        SimulationXun currentXun,
        SettlementSnapshot settlement,
        int clanSupport,
        PopulationHouseholdState household)
    {
        int supportBuffer = clanSupport >= 60 ? 1 : 0;

        switch (currentXun)
        {
            case SimulationXun.Shangxun:
            {
                int distressDelta = household.DebtPressure >= 50 ? 1 : 0;
                if (distressDelta > 0 && supportBuffer > 0)
                {
                    distressDelta -= 1;
                }

                household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);

                int debtDelta = household.Distress >= 60 && supportBuffer == 0 ? 1 : 0;
                household.DebtPressure = Math.Clamp(household.DebtPressure + debtDelta, 0, 100);
                break;
            }
            case SimulationXun.Zhongxun:
            {
                int laborDrop = household.Distress >= 60 ? 1 : 0;
                household.LaborCapacity = Math.Clamp(household.LaborCapacity - laborDrop, 0, 100);

                int distressDelta = settlement.Prosperity < 45 && supportBuffer == 0 ? 1 : 0;
                household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);
                break;
            }
            case SimulationXun.Xiaxun:
            {
                int migrationDelta = 0;
                if (settlement.Security < 45)
                {
                    migrationDelta += 1;
                }

                if (household.DebtPressure >= 60)
                {
                    migrationDelta += 1;
                }

                if (migrationDelta > 0 && supportBuffer > 0)
                {
                    migrationDelta -= 1;
                }

                household.MigrationRisk = Math.Clamp(household.MigrationRisk + migrationDelta, 0, 100);
                household.IsMigrating = ResolveMigrationStatus(household);
                break;
            }
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

    private static void DispatchTradeShockEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Step 1b gap 1 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口（Step 2 可吃）：违约方所属聚落的 debt/distress/livelihood；家户 sponsor clan 救济余力；
        // 当地粮价波动；徭役窗口；灾害窗口；季节带。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case TradeAndIndustryEventNames.RouteBusinessBlocked:
                case TradeAndIndustryEventNames.TradeLossOccurred:
                case TradeAndIndustryEventNames.TradeDebtDefaulted:
                case TradeAndIndustryEventNames.TradeProspered:
                    // TODO Step 2: 按维度入口调整相关 household 的 DebtLoad / LivelihoodPressure / MigrationRisk。
                    break;

                case TradeAndIndustryEventNames.GrainPriceSpike:
                    ApplyGrainPriceSubsistencePressure(scope, domainEvent);
                    break;
            }
        }
    }

    private static void ApplyGrainPriceSubsistencePressure(
        ModuleEventHandlingScope<PopulationAndHouseholdsState> scope,
        IDomainEvent domainEvent)
    {
        SettlementId? affectedSettlementId = ResolveSettlementScope(domainEvent);
        GrainPriceShockSignal signal = ResolveGrainPriceShockSignal(domainEvent);
        bool anyHouseholdChanged = false;

        foreach (PopulationHouseholdState household in scope.State.Households.OrderBy(static household => household.Id.Value))
        {
            if (affectedSettlementId.HasValue && household.SettlementId != affectedSettlementId.Value)
            {
                continue;
            }

            int oldDistress = household.Distress;
            SubsistencePressureProfile subsistenceProfile = ComputeSubsistencePressureProfile(household, signal);
            int distressDelta = subsistenceProfile.DistressDelta;
            household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);
            anyHouseholdChanged = true;

            if (oldDistress < 60 && household.Distress >= 60)
            {
                scope.Emit(
                    PopulationEventNames.HouseholdSubsistencePressureChanged,
                    $"{household.HouseholdName}粮价陡起，生计吃紧。",
                    household.Id.Value.ToString(),
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseGrainPriceSpike,
                        [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                        [DomainEventMetadataKeys.SettlementId] = household.SettlementId.Value.ToString(),
                        [DomainEventMetadataKeys.DistressBefore] = oldDistress.ToString(),
                        [DomainEventMetadataKeys.DistressAfter] = household.Distress.ToString(),
                        [DomainEventMetadataKeys.SubsistenceDistressDelta] = distressDelta.ToString(),
                        [DomainEventMetadataKeys.SubsistencePricePressure] = subsistenceProfile.PricePressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceGrainBufferPressure] = subsistenceProfile.GrainBufferPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceMarketDependencyPressure] = subsistenceProfile.MarketDependencyPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceLaborPressure] = subsistenceProfile.LaborPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceFragilityPressure] = subsistenceProfile.FragilityPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceInteractionPressure] = subsistenceProfile.InteractionPressure.ToString(),
                        [DomainEventMetadataKeys.GrainCurrentPrice] = signal.CurrentPrice.ToString(),
                        [DomainEventMetadataKeys.GrainPriceDelta] = signal.PriceDelta.ToString(),
                        [DomainEventMetadataKeys.GrainSupply] = signal.Supply.ToString(),
                        [DomainEventMetadataKeys.GrainDemand] = signal.Demand.ToString(),
                        [DomainEventMetadataKeys.Livelihood] = household.Livelihood.ToString(),
                    });
            }
        }

        if (anyHouseholdChanged)
        {
            SynchronizeMembershipLivelihoodsAndActivities(scope.State);
            RebuildSettlementSummaries(scope.State, scope.TryGetQuery<IPersonRegistryQueries>());
        }
    }

    private static void DispatchWorldPulseEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Step 1b gap 3 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：洪灾 / 徭役窗口落在哪个聚落；家户 sponsor clan 支撑力；家底 ratio；季节带。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case WorldSettlementsEventNames.FloodRiskThresholdBreached:
                case WorldSettlementsEventNames.CorveeWindowChanged:
                    // TODO Step 2: 按维度入口调整 LaborPressure / LivelihoodPressure / MigrationRisk。
                    break;

                case WorldSettlementsEventNames.TaxSeasonOpened:
                    ApplyTaxSeasonPressure(scope, domainEvent);
                    break;
            }
        }
    }

    private static void ApplyTaxSeasonPressure(
        ModuleEventHandlingScope<PopulationAndHouseholdsState> scope,
        IDomainEvent domainEvent)
    {
        SettlementId? scopedSettlementId = ResolveSettlementScope(domainEvent);

        foreach (PopulationHouseholdState household in scope.State.Households.OrderBy(static household => household.Id.Value))
        {
            if (scopedSettlementId is not null && household.SettlementId != scopedSettlementId.Value)
            {
                continue;
            }

            int oldDebt = household.DebtPressure;
            TaxSeasonBurdenProfile taxProfile = ComputeTaxSeasonBurdenProfile(household);
            int taxDebtDelta = taxProfile.DebtDelta;
            household.DebtPressure = Math.Clamp(household.DebtPressure + taxDebtDelta, 0, 100);

            if (oldDebt < 70 && household.DebtPressure >= 70)
            {
                scope.Emit(
                    PopulationEventNames.HouseholdDebtSpiked,
                    $"{household.HouseholdName}税役加急，债压陡起。",
                    household.Id.Value.ToString(),
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseTaxSeason,
                        [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                        [DomainEventMetadataKeys.SettlementId] = household.SettlementId.Value.ToString(),
                        [DomainEventMetadataKeys.DebtBefore] = oldDebt.ToString(),
                        [DomainEventMetadataKeys.DebtAfter] = household.DebtPressure.ToString(),
                        [DomainEventMetadataKeys.TaxDebtDelta] = taxDebtDelta.ToString(),
                        [DomainEventMetadataKeys.TaxVisibilityPressure] = taxProfile.VisibilityPressure.ToString(),
                        [DomainEventMetadataKeys.TaxLiquidityPressure] = taxProfile.LiquidityPressure.ToString(),
                        [DomainEventMetadataKeys.TaxLaborPressure] = taxProfile.LaborPressure.ToString(),
                        [DomainEventMetadataKeys.TaxFragilityPressure] = taxProfile.FragilityPressure.ToString(),
                        [DomainEventMetadataKeys.TaxInteractionPressure] = taxProfile.InteractionPressure.ToString(),
                        [DomainEventMetadataKeys.Livelihood] = household.Livelihood.ToString(),
                    });
            }
        }
    }

    private static void DispatchFamilyBranchEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Step 1b gap 4 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：分房 clan 家底 / 聚落容纳；旧家 sponsor 关系；是否可迁移；新分出支系与原聚落距离；
        // 四季带 / 战后恢复期。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case FamilyCoreEventNames.BranchSeparationApproved:
                case FamilyCoreEventNames.HeirSecurityWeakened:
                    // TODO Step 2: 按维度入口调整 household sponsor / 新增户 / MigrationRisk。
                    break;
            }
        }
    }

    private static void DispatchOfficeSupplyEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Chain 5: official supply requisition becomes household-owned burden.
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != OfficeAndCareerEventNames.OfficialSupplyRequisition)
            {
                continue;
            }

            SettlementId? scopedSettlementId = ResolveSettlementScope(domainEvent);
            if (scopedSettlementId is null)
            {
                continue;
            }

            SettlementId settlementId = scopedSettlementId.Value;
            OfficialSupplySignal signal = ResolveOfficialSupplySignal(domainEvent);
            bool anyHouseholdChanged = false;

            foreach (PopulationHouseholdState household in scope.State.Households
                .Where(h => h.SettlementId == settlementId)
                .OrderBy(static h => h.Id.Value))
            {
                int oldDistress = household.Distress;
                int oldDebt = household.DebtPressure;
                int oldLabor = household.LaborCapacity;
                int oldMigration = household.MigrationRisk;
                OfficialSupplyBurdenProfile burdenProfile = ComputeOfficialSupplyBurdenProfile(household, signal);

                household.Distress = Math.Clamp(household.Distress + burdenProfile.DistressDelta, 0, 100);
                household.DebtPressure = Math.Clamp(household.DebtPressure + burdenProfile.DebtDelta, 0, 100);
                household.LaborCapacity = Math.Clamp(household.LaborCapacity - burdenProfile.LaborDrop, 0, 100);
                household.MigrationRisk = Math.Clamp(household.MigrationRisk + burdenProfile.MigrationDelta, 0, 100);
                household.IsMigrating = ResolveMigrationStatus(household);
                anyHouseholdChanged = anyHouseholdChanged
                    || oldDistress != household.Distress
                    || oldDebt != household.DebtPressure
                    || oldLabor != household.LaborCapacity
                    || oldMigration != household.MigrationRisk;

                if (oldDistress < 80 && household.Distress >= 80)
                {
                    scope.Emit(
                        PopulationEventNames.HouseholdBurdenIncreased,
                        $"{household.HouseholdName}军需催迫，家户负担加重。",
                        household.Id.Value.ToString(),
                        new Dictionary<string, string>
                        {
                            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseOfficialSupply,
                            [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                            [DomainEventMetadataKeys.SettlementId] = settlementId.Value.ToString(),
                            [DomainEventMetadataKeys.DistressBefore] = oldDistress.ToString(),
                            [DomainEventMetadataKeys.DistressAfter] = household.Distress.ToString(),
                            [DomainEventMetadataKeys.DebtBefore] = oldDebt.ToString(),
                            [DomainEventMetadataKeys.DebtAfter] = household.DebtPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyPressure] = signal.SupplyPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = signal.QuotaPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = signal.DocketPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = signal.ClerkDistortionPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = signal.AuthorityBuffer.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyDistressDelta] = burdenProfile.DistressDelta.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyDebtDelta] = burdenProfile.DebtDelta.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLaborDrop] = burdenProfile.LaborDrop.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyMigrationDelta] = burdenProfile.MigrationDelta.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure] = burdenProfile.LivelihoodExposurePressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyResourceBuffer] = burdenProfile.ResourceBuffer.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLaborPressure] = burdenProfile.LaborPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLiquidityPressure] = burdenProfile.LiquidityPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyFragilityPressure] = burdenProfile.FragilityPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyInteractionPressure] = burdenProfile.InteractionPressure.ToString(),
                            [DomainEventMetadataKeys.FrontierPressure] = signal.FrontierPressure.ToString(),
                            [DomainEventMetadataKeys.Livelihood] = household.Livelihood.ToString(),
                        });
                }
            }

            if (anyHouseholdChanged)
            {
                SynchronizeMembershipLivelihoodsAndActivities(scope.State);
                RebuildSettlementSummaries(scope.State, scope.TryGetQuery<IPersonRegistryQueries>());
            }
        }
    }

    private readonly record struct LivelihoodDriftResult(
        LivelihoodType Previous,
        LivelihoodType Current,
        string Reason);

    private static int GetClanSupportReserve(IFamilyCoreQueries familyQueries, ClanId? sponsorClanId)
    {
        return sponsorClanId is null ? 0 : familyQueries.GetRequiredClan(sponsorClanId.Value).SupportReserve;
    }

    private static int ComputeDebtDelta(int distress)
    {
        if (distress >= 75)
        {
            return 3;
        }

        if (distress >= 55)
        {
            return 2;
        }

        if (distress >= 40)
        {
            return 1;
        }

        return -1;
    }

    private static int ComputeLaborDelta(int prosperity, int distress)
    {
        int delta = prosperity >= 58 ? 1 : prosperity <= 42 ? -1 : 0;
        if (distress >= 70)
        {
            delta -= 1;
        }

        return delta;
    }

    private static int ComputeMigrationDelta(int security, int distress)
    {
        int delta = distress >= 70 ? 2 : distress >= 50 ? 1 : -1;
        if (security < 40)
        {
            delta += 1;
        }

        return delta;
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

    private static bool ResolveMigrationStatus(PopulationHouseholdState household)
    {
        return ResolveMigrationStatus(
            household,
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStatusThreshold);
    }

    private static bool ResolveMigrationStatus(PopulationHouseholdState household, int migrationStatusThreshold)
    {
        return household.MigrationRisk >= migrationStatusThreshold;
    }

    private static bool TryApplyMonthlyLivelihoodDrift(
        PopulationHouseholdState household,
        SettlementSnapshot settlement,
        out LivelihoodDriftResult result)
    {
        LivelihoodType previous = household.Livelihood;
        LivelihoodType current = ResolveMonthlyLivelihood(household, settlement);
        if (current == previous)
        {
            result = default;
            return false;
        }

        household.Livelihood = current;
        result = new LivelihoodDriftResult(previous, current, BuildLivelihoodDriftReason(household, settlement, current));
        return true;
    }

    private static LivelihoodType ResolveMonthlyLivelihood(PopulationHouseholdState household, SettlementSnapshot settlement)
    {
        if (household.Distress >= 85 && household.DebtPressure >= 80 && household.LaborCapacity < 35)
        {
            return LivelihoodType.Vagrant;
        }

        if (household.IsMigrating || household.MigrationRisk >= 80)
        {
            return household.Livelihood == LivelihoodType.Vagrant
                ? LivelihoodType.Vagrant
                : LivelihoodType.SeasonalMigrant;
        }

        if (household.Livelihood == LivelihoodType.Smallholder
            && household.LandHolding is > 0 and < 20
            && household.DebtPressure >= 65)
        {
            return LivelihoodType.Tenant;
        }

        if (household.Livelihood == LivelihoodType.Tenant
            && household.Distress >= 70
            && household.LaborCapacity < 45)
        {
            return LivelihoodType.HiredLabor;
        }

        if (household.Livelihood is LivelihoodType.PettyTrader or LivelihoodType.Artisan or LivelihoodType.Boatman
            && household.DebtPressure >= 80
            && household.Distress >= 70)
        {
            return LivelihoodType.HiredLabor;
        }

        if (household.Livelihood is LivelihoodType.HiredLabor or LivelihoodType.SeasonalMigrant
            && settlement.Prosperity >= 58
            && settlement.Security >= 55
            && household.Distress <= 40
            && household.DebtPressure <= 40
            && household.LandHolding >= 25
            && household.GrainStore >= 45)
        {
            return LivelihoodType.Smallholder;
        }

        if (household.Livelihood == LivelihoodType.Vagrant
            && household.Distress <= 45
            && household.DebtPressure <= 50
            && household.LaborCapacity >= 45
            && settlement.Security >= 50)
        {
            return LivelihoodType.HiredLabor;
        }

        return household.Livelihood;
    }

    private static string BuildLivelihoodDriftReason(
        PopulationHouseholdState household,
        SettlementSnapshot settlement,
        LivelihoodType current)
    {
        return current switch
        {
            LivelihoodType.Vagrant => "债压、民困和丁力一齐断裂，先从家计滑成游食",
            LivelihoodType.SeasonalMigrant => "迁徙风险已过阈值，远路成为压力出口",
            LivelihoodType.Tenant => "地少债重，先从自耕滑向佃作",
            LivelihoodType.HiredLabor => "家计不稳，只能把劳力先卖到短工、脚役或佣作处",
            LivelihoodType.Smallholder => $"{settlement.Name}市况和治安稍稳，存粮与薄地足以把家户拉回小农轨道",
            _ => $"在{settlement.Name}的压力面中改换生计",
        };
    }

    private static bool IsLivelihoodCollapseDrift(LivelihoodDriftResult result)
    {
        return result.Current == LivelihoodType.Vagrant
            || (result.Previous is LivelihoodType.Smallholder or LivelihoodType.Tenant
                && result.Current == LivelihoodType.HiredLabor);
    }

    private static string RenderLivelihoodForDiff(LivelihoodType livelihood)
    {
        return livelihood switch
        {
            LivelihoodType.Smallholder => "小农",
            LivelihoodType.Tenant => "佃作",
            LivelihoodType.HiredLabor => "雇工",
            LivelihoodType.Artisan => "手艺",
            LivelihoodType.PettyTrader => "小贩",
            LivelihoodType.Boatman => "船脚",
            LivelihoodType.DomesticServant => "仆役",
            LivelihoodType.YamenRunner => "衙前差使",
            LivelihoodType.SeasonalMigrant => "季节外出",
            LivelihoodType.Vagrant => "游食",
            _ => "生计未明",
        };
    }

    private static int ComputeLivelihoodDistressBaseline(LivelihoodType livelihood)
    {
        return livelihood switch
        {
            LivelihoodType.Vagrant => 3,
            LivelihoodType.SeasonalMigrant => 2,
            LivelihoodType.Tenant => 1,
            LivelihoodType.HiredLabor => 1,
            LivelihoodType.Boatman => 1,
            LivelihoodType.DomesticServant => 0,
            LivelihoodType.YamenRunner => 0,
            LivelihoodType.Smallholder => 0,
            LivelihoodType.Artisan => -1,
            LivelihoodType.PettyTrader => -1,
            _ => 0,
        };
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
