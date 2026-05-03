using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
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

        public IReadOnlyList<HouseholdMembershipSnapshot> GetMemberships()
        {
            return _state.Memberships
                .OrderBy(static member => member.PersonId.Value)
                .Select(CloneMembership)
                .ToArray();
        }

        public IReadOnlyList<HouseholdMembershipSnapshot> GetMembershipsByHousehold(HouseholdId householdId)
        {
            return _state.Memberships
                .Where(member => member.HouseholdId == householdId)
                .OrderBy(static member => member.PersonId.Value)
                .Select(CloneMembership)
                .ToArray();
        }

        public bool TryGetMembership(PersonId personId, out HouseholdMembershipSnapshot membership)
        {
            HouseholdMembershipState? found = _state.Memberships.SingleOrDefault(member => member.PersonId == personId);
            if (found is null)
            {
                membership = new HouseholdMembershipSnapshot();
                return false;
            }

            membership = CloneMembership(found);
            return true;
        }

        public IReadOnlyList<LaborPoolEntrySnapshot> GetLaborPools()
        {
            return _state.LaborPools
                .OrderBy(static entry => entry.SettlementId.Value)
                .Select(static entry => new LaborPoolEntrySnapshot
                {
                    SettlementId = entry.SettlementId,
                    AvailableLabor = entry.AvailableLabor,
                    LaborDemand = entry.LaborDemand,
                    SeasonalSurplus = entry.SeasonalSurplus,
                    WageLevel = entry.WageLevel,
                })
                .ToArray();
        }

        public IReadOnlyList<MarriagePoolEntrySnapshot> GetMarriagePools()
        {
            return _state.MarriagePools
                .OrderBy(static entry => entry.SettlementId.Value)
                .Select(static entry => new MarriagePoolEntrySnapshot
                {
                    SettlementId = entry.SettlementId,
                    EligibleMales = entry.EligibleMales,
                    EligibleFemales = entry.EligibleFemales,
                    MatchDifficulty = entry.MatchDifficulty,
                })
                .ToArray();
        }

        public IReadOnlyList<MigrationPoolEntrySnapshot> GetMigrationPools()
        {
            return _state.MigrationPools
                .OrderBy(static entry => entry.SettlementId.Value)
                .Select(static entry => new MigrationPoolEntrySnapshot
                {
                    SettlementId = entry.SettlementId,
                    OutflowPressure = entry.OutflowPressure,
                    InflowPressure = entry.InflowPressure,
                    FloatingPopulation = entry.FloatingPopulation,
                })
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
                Livelihood = household.Livelihood,
                Distress = household.Distress,
                DebtPressure = household.DebtPressure,
                LaborCapacity = household.LaborCapacity,
                MigrationRisk = household.MigrationRisk,
                IsMigrating = household.IsMigrating,
                LandHolding = household.LandHolding,
                GrainStore = household.GrainStore,
                ToolCondition = household.ToolCondition,
                ShelterQuality = household.ShelterQuality,
                DependentCount = household.DependentCount,
                LaborerCount = household.LaborerCount,
                LastLocalResponseCommandCode = household.LastLocalResponseCommandCode,
                LastLocalResponseCommandLabel = household.LastLocalResponseCommandLabel,
                LastLocalResponseOutcomeCode = household.LastLocalResponseOutcomeCode,
                LastLocalResponseTraceCode = household.LastLocalResponseTraceCode,
                LastLocalResponseSummary = household.LastLocalResponseSummary,
                LocalResponseCarryoverMonths = household.LocalResponseCarryoverMonths,
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

        private static HouseholdMembershipSnapshot CloneMembership(HouseholdMembershipState membership)
        {
            return new HouseholdMembershipSnapshot
            {
                PersonId = membership.PersonId,
                HouseholdId = membership.HouseholdId,
                Livelihood = membership.Livelihood,
                HealthResilience = membership.HealthResilience,
                Health = membership.Health,
                IllnessMonths = membership.IllnessMonths,
                Activity = membership.Activity,
            };
        }
    }
}
