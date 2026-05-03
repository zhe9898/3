using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private static void SynchronizeMembershipLivelihoodsAndActivities(PopulationAndHouseholdsState state)
    {
        if (state.Memberships.Count == 0 || state.Households.Count == 0)
        {
            return;
        }

        Dictionary<HouseholdId, PopulationHouseholdState> householdsById = state.Households
            .ToDictionary(static household => household.Id, static household => household);

        foreach (HouseholdMembershipState membership in state.Memberships.OrderBy(static member => member.PersonId.Value))
        {
            if (!householdsById.TryGetValue(membership.HouseholdId, out PopulationHouseholdState? household))
            {
                continue;
            }

            membership.Livelihood = household.Livelihood;
            if (membership.Health >= HealthStatus.Ill)
            {
                membership.Activity = PersonActivity.Convalescing;
                continue;
            }

            if (membership.Activity == PersonActivity.Studying
                && !household.IsMigrating
                && household.MigrationRisk < 80)
            {
                continue;
            }

            membership.Activity = ResolveHouseholdActivity(household);
        }
    }

    private static PersonActivity ResolveHouseholdActivity(PopulationHouseholdState household)
    {
        if (household.IsMigrating || household.MigrationRisk >= 80)
        {
            return PersonActivity.Migrating;
        }

        return household.Livelihood switch
        {
            LivelihoodType.Smallholder => PersonActivity.Farming,
            LivelihoodType.Tenant => PersonActivity.Farming,
            LivelihoodType.HiredLabor => PersonActivity.Laboring,
            LivelihoodType.Artisan => PersonActivity.Laboring,
            LivelihoodType.PettyTrader => PersonActivity.Trading,
            LivelihoodType.Boatman => PersonActivity.Laboring,
            LivelihoodType.DomesticServant => PersonActivity.Serving,
            LivelihoodType.YamenRunner => PersonActivity.Serving,
            LivelihoodType.SeasonalMigrant => PersonActivity.Laboring,
            _ => PersonActivity.Idle,
        };
    }

    private static void PromoteHotHouseholdMembers(
        ModuleExecutionContext context,
        PopulationAndHouseholdsState state,
        IPersonRegistryQueries? personQueries,
        IPersonRegistryCommands? personCommands,
        PopulationHouseholdMobilityRulesData rulesData)
    {
        if (personQueries is null || personCommands is null || state.Memberships.Count == 0)
        {
            return;
        }

        Dictionary<HouseholdId, PopulationHouseholdState> householdsById = state.Households
            .ToDictionary(static household => household.Id, static household => household);
        int focusedMemberPromotionCap = rulesData.GetFocusedMemberPromotionCapOrDefault();

        foreach (IGrouping<HouseholdId, HouseholdMembershipState> group in state.Memberships
            .GroupBy(static membership => membership.HouseholdId)
            .OrderBy(static group => group.Key.Value))
        {
            if (!householdsById.TryGetValue(group.Key, out PopulationHouseholdState? household))
            {
                continue;
            }

            string focusReason = ResolveFocusPromotionReason(household);
            if (string.IsNullOrWhiteSpace(focusReason))
            {
                continue;
            }

            foreach (HouseholdMembershipState membership in group
                .OrderBy(static member => member.PersonId.Value)
                .Where(membership =>
                    personQueries.TryGetPerson(membership.PersonId, out PersonRecord person)
                    && person.IsAlive
                    && person.FidelityRing == FidelityRing.Regional)
                .Take(focusedMemberPromotionCap))
            {
                personCommands.ChangeFidelityRing(
                    context,
                    membership.PersonId,
                    FidelityRing.Local,
                    focusReason);
            }
        }
    }

    private static string ResolveFocusPromotionReason(PopulationHouseholdState household)
    {
        if (household.IsMigrating || household.MigrationRisk >= 80)
        {
            return "迁徙压力触发近处读回";
        }

        if (household.Distress >= 80 && household.DebtPressure >= 85)
        {
            return "生计崩口触发近处读回";
        }

        if (household.LaborCapacity < 25 && household.Distress >= 75)
        {
            return "丁力危面触发近处读回";
        }

        return string.Empty;
    }
}
