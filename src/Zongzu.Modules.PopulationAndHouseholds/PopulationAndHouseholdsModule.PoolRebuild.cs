using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private static void RebuildSettlementSummaries(
        PopulationAndHouseholdsState state,
        IPersonRegistryQueries? personQueries)
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
        RebuildLaborPools(state);
        RebuildMarriagePools(state, personQueries);
        RebuildMigrationPools(state);
    }

    private static void RebuildLaborPools(PopulationAndHouseholdsState state)
    {
        state.LaborPools = state.Households
            .GroupBy(static household => household.SettlementId)
            .OrderBy(static group => group.Key.Value)
            .Select(static group =>
            {
                PopulationHouseholdState[] households = group.OrderBy(static household => household.Id.Value).ToArray();
                int availableLabor = households.Sum(static household => household.LaborCapacity);
                int laborDemand = households.Sum(ComputeHouseholdLaborDemand);
                int averageMigration = households.Sum(static household => household.MigrationRisk) / households.Length;

                return new LaborPoolEntryState
                {
                    SettlementId = group.Key,
                    AvailableLabor = availableLabor,
                    LaborDemand = laborDemand,
                    SeasonalSurplus = Math.Clamp(availableLabor - laborDemand, -200, 400),
                    WageLevel = Math.Clamp(50 + ((laborDemand - availableLabor) / households.Length) + (averageMigration / 4), 20, 120),
                };
            })
            .ToList();
    }

    private static int ComputeHouseholdLaborDemand(PopulationHouseholdState household)
    {
        int livelihoodDemand = household.Livelihood switch
        {
            LivelihoodType.Smallholder => 55 + household.LandHolding / 2,
            LivelihoodType.Tenant => 45 + household.LandHolding / 3,
            LivelihoodType.HiredLabor => 35,
            LivelihoodType.Artisan => 40,
            LivelihoodType.PettyTrader => 32,
            LivelihoodType.Boatman => 44,
            LivelihoodType.DomesticServant => 28,
            LivelihoodType.YamenRunner => 30,
            LivelihoodType.SeasonalMigrant => 24,
            LivelihoodType.Vagrant => 12,
            _ => 30,
        };

        return Math.Clamp(
            livelihoodDemand
            + (household.DependentCount * 4)
            + Math.Max(0, household.Distress - 55) / 2,
            0,
            140);
    }

    private static void RebuildMarriagePools(
        PopulationAndHouseholdsState state,
        IPersonRegistryQueries? personQueries)
    {
        state.MarriagePools = state.Households
            .GroupBy(static household => household.SettlementId)
            .OrderBy(static group => group.Key.Value)
            .Select(group =>
            {
                HashSet<HouseholdId> householdIds = group.Select(static household => household.Id).ToHashSet();
                HouseholdMembershipState[] memberships = state.Memberships
                    .Where(membership => householdIds.Contains(membership.HouseholdId))
                    .OrderBy(static membership => membership.PersonId.Value)
                    .ToArray();
                int eligibleMales = 0;
                int eligibleFemales = 0;

                foreach (HouseholdMembershipState membership in memberships)
                {
                    if (!IsMarriagePoolEligible(personQueries, membership, out PersonGender gender))
                    {
                        continue;
                    }

                    if (gender == PersonGender.Female)
                    {
                        eligibleFemales += 1;
                    }
                    else
                    {
                        eligibleMales += 1;
                    }
                }

                PopulationHouseholdState[] households = group.ToArray();
                int averageDistress = households.Sum(static household => household.Distress) / households.Length;
                int averageMigration = households.Sum(static household => household.MigrationRisk) / households.Length;
                int mismatch = Math.Abs(eligibleMales - eligibleFemales);

                return new MarriagePoolEntryState
                {
                    SettlementId = group.Key,
                    EligibleMales = eligibleMales,
                    EligibleFemales = eligibleFemales,
                    MatchDifficulty = Math.Clamp((mismatch * 12) + averageDistress / 3 + averageMigration / 4, 0, 100),
                };
            })
            .ToList();
    }

    private static bool IsMarriagePoolEligible(
        IPersonRegistryQueries? personQueries,
        HouseholdMembershipState membership,
        out PersonGender gender)
    {
        gender = membership.PersonId.Value % 2 == 0
            ? PersonGender.Female
            : PersonGender.Male;

        if (personQueries is null)
        {
            return true;
        }

        if (!personQueries.TryGetPerson(membership.PersonId, out PersonRecord person)
            || !person.IsAlive
            || person.LifeStage is not (LifeStage.Youth or LifeStage.Adult))
        {
            return false;
        }

        gender = person.Gender == PersonGender.Unspecified ? gender : person.Gender;
        return true;
    }

    private static void RebuildMigrationPools(PopulationAndHouseholdsState state)
    {
        state.MigrationPools = state.Households
            .GroupBy(static household => household.SettlementId)
            .OrderBy(static group => group.Key.Value)
            .Select(group =>
            {
                PopulationHouseholdState[] households = group.OrderBy(static household => household.Id.Value).ToArray();
                int averageMigration = households.Sum(static household => household.MigrationRisk) / households.Length;
                int averageDistress = households.Sum(static household => household.Distress) / households.Length;
                int averageLabor = households.Sum(static household => household.LaborCapacity) / households.Length;
                int migratingHouseholds = households.Count(static household => household.IsMigrating);
                HashSet<HouseholdId> householdIds = households.Select(static household => household.Id).ToHashSet();
                int migratingMembers = state.Memberships.Count(membership =>
                    householdIds.Contains(membership.HouseholdId)
                    && membership.Activity == PersonActivity.Migrating);
                int seasonalHouseholds = households.Count(static household => household.Livelihood == LivelihoodType.SeasonalMigrant);

                return new MigrationPoolEntryState
                {
                    SettlementId = group.Key,
                    OutflowPressure = Math.Clamp(averageMigration + migratingHouseholds * 8 + migratingMembers * 2, 0, 100),
                    InflowPressure = Math.Clamp(70 - averageDistress + averageLabor / 3, 0, 100),
                    FloatingPopulation = Math.Clamp(migratingHouseholds * 5 + migratingMembers * 2 + seasonalHouseholds * 3, 0, 200),
                };
            })
            .ToList();
    }
}
