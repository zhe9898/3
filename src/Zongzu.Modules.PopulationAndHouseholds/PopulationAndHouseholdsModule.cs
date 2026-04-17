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

    public override string ModuleKey => KnownModuleKeys.PopulationAndHouseholds;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.PopulationPressure;

    public override int ExecutionOrder => 200;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

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
                $"Household {household.HouseholdName} shifted to distress {household.Distress}, debt {household.DebtPressure}, labor {household.LaborCapacity}, migration {household.MigrationRisk}.",
                household.Id.Value.ToString());

            if (oldDebtPressure < 70 && household.DebtPressure >= 70)
            {
                scope.Emit("HouseholdDebtSpiked", $"Household {household.HouseholdName} debt pressure spiked.");
            }

            if (oldLaborCapacity >= 30 && household.LaborCapacity < 30)
            {
                scope.Emit("LaborShortage", $"Household {household.HouseholdName} entered labor shortage.");
            }

            if (oldMigrationRisk < 80 && household.MigrationRisk >= 80 && !wasMigrating)
            {
                scope.Emit("MigrationStarted", $"Household {household.HouseholdName} started migration.");
            }

            if (oldDebtPressure < 85 && household.DebtPressure >= 85 && household.Distress >= 80)
            {
                scope.Emit("LivelihoodCollapsed", $"Household {household.HouseholdName} livelihood collapsed.");
            }
        }

        RebuildSettlementSummaries(scope.State);
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
