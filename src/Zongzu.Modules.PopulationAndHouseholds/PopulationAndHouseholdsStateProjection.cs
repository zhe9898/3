using System;
using System.Collections.Generic;
using Zongzu.Contracts;

namespace Zongzu.Modules.PopulationAndHouseholds;

/// <summary>
/// Phase 3 生计骨骼 schema migration.
/// v1 → v2: add LivelihoodType + land/grain/tool/shelter/dependent/laborer
/// household fields; initialise Memberships / LaborPools / MarriagePools /
/// MigrationPools. See <c>LIVING_WORLD_DESIGN.md §2.3</c>.
/// v2 → v3: add structured home-household local response trace fields.
/// </summary>
public static class PopulationAndHouseholdsStateProjection
{
    public static void UpgradeFromSchemaV1ToV2(PopulationAndHouseholdsState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (PopulationHouseholdState household in state.Households)
        {
            if (household.Livelihood == LivelihoodType.Unknown)
            {
                household.Livelihood = LivelihoodType.Smallholder;
            }

            household.LandHolding = Math.Clamp(household.LandHolding, 0, 100);
            household.GrainStore = Math.Clamp(household.GrainStore, 0, 100);
            household.ToolCondition = Math.Clamp(household.ToolCondition == 0 ? 50 : household.ToolCondition, 0, 100);
            household.ShelterQuality = Math.Clamp(household.ShelterQuality == 0 ? 50 : household.ShelterQuality, 0, 100);
            household.DependentCount = Math.Max(0, household.DependentCount);
            household.LaborerCount = Math.Max(0, household.LaborerCount);
        }

        state.Memberships ??= new List<HouseholdMembershipState>();
        state.LaborPools ??= new List<LaborPoolEntryState>();
        state.MarriagePools ??= new List<MarriagePoolEntryState>();
        state.MigrationPools ??= new List<MigrationPoolEntryState>();

        foreach (HouseholdMembershipState membership in state.Memberships)
        {
            if (membership.Livelihood == LivelihoodType.Unknown)
            {
                membership.Livelihood = LivelihoodType.Smallholder;
            }

            if (membership.Health == HealthStatus.Unknown)
            {
                membership.Health = HealthStatus.Healthy;
            }

            if (membership.Activity == PersonActivity.Unknown)
            {
                membership.Activity = PersonActivity.Idle;
            }

            if (membership.HealthResilience == 0)
            {
                membership.HealthResilience = 50;
            }
        }
    }

    public static void UpgradeFromSchemaV2ToV3(PopulationAndHouseholdsState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (PopulationHouseholdState household in state.Households)
        {
            household.LastLocalResponseCommandCode ??= string.Empty;
            household.LastLocalResponseCommandLabel ??= string.Empty;
            household.LastLocalResponseOutcomeCode ??= string.Empty;
            household.LastLocalResponseTraceCode ??= string.Empty;
            household.LastLocalResponseSummary ??= string.Empty;
            household.LocalResponseCarryoverMonths = Math.Clamp(household.LocalResponseCarryoverMonths, 0, 1);
        }
    }
}
