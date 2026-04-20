using System;
using System.Linq;

namespace Zongzu.Modules.FamilyCore;

public static class FamilyCoreStateProjection
{
    public static void UpgradeFromSchemaV1(FamilyCoreState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (ClanStateData clan in state.Clans)
        {
            clan.BranchTension = Math.Clamp(clan.BranchTension, 0, 100);
            clan.InheritancePressure = Math.Clamp(clan.InheritancePressure, 0, 100);
            clan.SeparationPressure = Math.Clamp(clan.SeparationPressure, 0, 100);
            clan.MediationMomentum = Math.Clamp(clan.MediationMomentum, 0, 100);
            clan.BranchFavorPressure = Math.Clamp(clan.BranchFavorPressure, 0, 100);
            clan.ReliefSanctionPressure = Math.Clamp(clan.ReliefSanctionPressure, 0, 100);
            clan.LastConflictCommandCode ??= string.Empty;
            clan.LastConflictCommandLabel ??= string.Empty;
            clan.LastConflictOutcome ??= string.Empty;
            clan.LastConflictTrace ??= string.Empty;
        }
    }

    public static void UpgradeFromSchemaV2ToV3(FamilyCoreState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (ClanStateData clan in state.Clans)
        {
            FamilyPersonState[] livingPeople = state.People
                .Where(person => person.ClanId == clan.Id && person.IsAlive)
                .OrderByDescending(static person => person.AgeMonths)
                .ThenBy(static person => person.Id.Value)
                .ToArray();

            FamilyPersonState? livingHeir = clan.HeirPersonId is null
                ? null
                : livingPeople.SingleOrDefault(person => person.Id == clan.HeirPersonId.Value);

            int adultCount = livingPeople.Count(static person => person.AgeMonths >= 16 * 12 && person.AgeMonths < 55 * 12);
            int childCount = livingPeople.Count(static person => person.AgeMonths < 16 * 12);
            int elderCount = livingPeople.Count(static person => person.AgeMonths >= 55 * 12);

            clan.MarriageAlliancePressure = Math.Clamp(
                adultCount <= 1
                    ? 54
                    : 34,
                0,
                100);
            clan.MarriageAllianceValue = 0;
            clan.HeirSecurity = InferHeirSecurity(livingHeir);
            clan.ReproductivePressure = Math.Clamp(
                childCount == 0
                    ? 42 + Math.Max(0, 1 - adultCount) * 8
                    : 22,
                0,
                100);
            clan.MourningLoad = elderCount > 0 ? 6 : 0;
            clan.LastLifecycleCommandCode ??= string.Empty;
            clan.LastLifecycleCommandLabel ??= string.Empty;
            clan.LastLifecycleOutcome ??= string.Empty;
            clan.LastLifecycleTrace ??= string.Empty;
        }
    }

    private static int InferHeirSecurity(FamilyPersonState? livingHeir)
    {
        if (livingHeir is null)
        {
            return 18;
        }

        if (livingHeir.AgeMonths >= 20 * 12)
        {
            return 68;
        }

        if (livingHeir.AgeMonths >= 12 * 12)
        {
            return 48;
        }

        return 28;
    }
}
