using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
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