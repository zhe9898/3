using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed class PopulationAndHouseholdsModule : ModuleRunner<PopulationAndHouseholdsState>
{
    private static readonly string[] CommandNames =
    [
        "HireLabor",
        "AdjustTenancyBurden",
        "ProvideRelief",
    ];

    private static readonly string[] EventNames =
    [
        "HouseholdDebtSpiked",
        "MigrationStarted",
        "LaborShortage",
        "LivelihoodCollapsed",
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.PopulationAndHouseholds;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.PopulationPressure;

    public override int ExecutionOrder => 200;

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

    public override void RunMonth(ModuleExecutionScope<PopulationAndHouseholdsState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();

        foreach (PopulationHouseholdState household in scope.State.Households.OrderBy(static household => household.Id.Value))
        {
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(household.SettlementId);
            int clanSupport = GetClanSupportReserve(familyQueries, household.SponsorClanId);

            int oldDebtPressure = household.DebtPressure;
            int oldLaborCapacity = household.LaborCapacity;
            int oldMigrationRisk = household.MigrationRisk;
            bool wasMigrating = household.IsMigrating;

            int prosperityPressure = settlement.Prosperity < 50 ? 1 : settlement.Prosperity >= 60 ? -1 : 0;
            int securityPressure = settlement.Security < 45 ? 1 : settlement.Security >= 55 ? -1 : 0;
            int relief = clanSupport >= 60 ? 1 : 0;
            int drift = scope.Context.Random.NextInt(-1, 2);

            household.Distress = Math.Clamp(household.Distress + prosperityPressure + securityPressure + drift - relief, 0, 100);
            household.DebtPressure = Math.Clamp(household.DebtPressure + ComputeDebtDelta(household.Distress), 0, 100);
            household.LaborCapacity = Math.Clamp(household.LaborCapacity + ComputeLaborDelta(settlement.Prosperity, household.Distress), 0, 100);
            household.MigrationRisk = Math.Clamp(household.MigrationRisk + ComputeMigrationDelta(settlement.Security, household.Distress), 0, 100);
            household.IsMigrating = household.IsMigrating || household.MigrationRisk >= 80;

            scope.RecordDiff(
                $"{household.HouseholdName}本月民困{household.Distress}，债压{household.DebtPressure}，丁力{household.LaborCapacity}，迁徙之念{household.MigrationRisk}。",
                household.Id.Value.ToString());

            if (oldDebtPressure < 70 && household.DebtPressure >= 70)
            {
                scope.Emit("HouseholdDebtSpiked", $"{household.HouseholdName}债压陡起。");
            }

            if (oldLaborCapacity >= 30 && household.LaborCapacity < 30)
            {
                scope.Emit("LaborShortage", $"{household.HouseholdName}丁力不继。");
            }

            if (oldMigrationRisk < 80 && household.MigrationRisk >= 80 && !wasMigrating)
            {
                scope.Emit("MigrationStarted", $"{household.HouseholdName}已起迁徙之念。");
            }

            if (oldDebtPressure < 85 && household.DebtPressure >= 85 && household.Distress >= 80)
            {
                scope.Emit("LivelihoodCollapsed", $"{household.HouseholdName}生计顿敝。");
            }
        }

        RebuildSettlementSummaries(scope.State);
    }

    public override void HandleEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
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
                household.IsMigrating = household.IsMigrating || household.MigrationRisk >= 80;
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
                scope.Emit("MigrationStarted", $"{migratingHousehold.HouseholdName}受战后余波所逼，已有远徙之意。", bundle.SettlementId.Value.ToString());
            }

            if (households.Any(static household => household.DebtPressure >= 85 && household.Distress >= 80))
            {
                PopulationHouseholdState collapsedHousehold = households
                    .OrderByDescending(static household => household.DebtPressure + household.Distress)
                    .ThenBy(static household => household.Id.Value)
                    .First();
                scope.Emit("LivelihoodCollapsed", $"{collapsedHousehold.HouseholdName}受{campaign.AnchorSettlementName}战后余波牵压，生计顿敝。", bundle.SettlementId.Value.ToString());
            }
        }

        if (anyHouseholdChanged)
        {
            RebuildSettlementSummaries(scope.State);
        }
    }

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

    private static void RebuildSettlementSummaries(PopulationAndHouseholdsState state)
    {
        List<PopulationSettlementState> summaries = state.Households
            .GroupBy(static household => household.SettlementId)
            .OrderBy(static group => group.Key.Value)
            .Select(static group =>
            {
                PopulationHouseholdState[] households = group.ToArray();
                return new PopulationSettlementState
                {
                    SettlementId = group.Key,
                    CommonerDistress = households.Sum(static household => household.Distress) / households.Length,
                    LaborSupply = households.Sum(static household => household.LaborCapacity),
                    MigrationPressure = households.Sum(static household => household.MigrationRisk) / households.Length,
                    MilitiaPotential = households.Sum(static household => Math.Max(0, household.LaborCapacity - (household.Distress / 2))),
                };
            })
            .ToList();

        state.Settlements = summaries;
    }

    private sealed class PopulationQueries : IPopulationAndHouseholdsQueries
    {
        private readonly PopulationAndHouseholdsState _state;

        public PopulationQueries(PopulationAndHouseholdsState state)
        {
            _state = state;
        }

        public HouseholdPressureSnapshot GetRequiredHousehold(HouseholdId householdId)
        {
            PopulationHouseholdState household = _state.Households.Single(household => household.Id == householdId);
            return CloneHousehold(household);
        }

        public PopulationSettlementSnapshot GetRequiredSettlement(SettlementId settlementId)
        {
            PopulationSettlementState settlement = _state.Settlements.Single(settlement => settlement.SettlementId == settlementId);
            return CloneSettlement(settlement);
        }

        public IReadOnlyList<HouseholdPressureSnapshot> GetHouseholds()
        {
            return _state.Households
                .OrderBy(static household => household.Id.Value)
                .Select(CloneHousehold)
                .ToArray();
        }

        public IReadOnlyList<PopulationSettlementSnapshot> GetSettlements()
        {
            return _state.Settlements
                .OrderBy(static settlement => settlement.SettlementId.Value)
                .Select(CloneSettlement)
                .ToArray();
        }

        private static HouseholdPressureSnapshot CloneHousehold(PopulationHouseholdState household)
        {
            return new HouseholdPressureSnapshot
            {
                Id = household.Id,
                HouseholdName = household.HouseholdName,
                SettlementId = household.SettlementId,
                SponsorClanId = household.SponsorClanId,
                Distress = household.Distress,
                DebtPressure = household.DebtPressure,
                LaborCapacity = household.LaborCapacity,
                MigrationRisk = household.MigrationRisk,
                IsMigrating = household.IsMigrating,
            };
        }

        private static PopulationSettlementSnapshot CloneSettlement(PopulationSettlementState settlement)
        {
            return new PopulationSettlementSnapshot
            {
                SettlementId = settlement.SettlementId,
                CommonerDistress = settlement.CommonerDistress,
                LaborSupply = settlement.LaborSupply,
                MigrationPressure = settlement.MigrationPressure,
                MilitiaPotential = settlement.MilitiaPotential,
            };
        }
    }
}
