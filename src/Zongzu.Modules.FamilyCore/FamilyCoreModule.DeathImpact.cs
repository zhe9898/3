using System;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Modules.FamilyCore;

public sealed partial class FamilyCoreModule
{
    internal readonly record struct FamilyDeathImpactProfile(
        int MourningLoadDelta,
        int InheritancePressureDelta,
        int SeparationPressureDelta,
        int BranchTensionDelta,
        int FuneralDebtDelta,
        int ReproductivePressureDelta,
        int MarriageAlliancePressureDelta,
        int CareLoadDelta,
        string PressureSummary);

    internal static FamilyDeathImpactProfile ComputeDeathImpactProfile(
        ClanStateData clan,
        FamilyMonthSignals signals,
        FamilyPersonState deathTarget,
        int deathAgeMonths,
        bool wasHeir,
        IPersonRegistryQueries registryQueries)
    {
        bool isChildDeath = deathAgeMonths < AdultAgeMonths;
        bool isElderDeath = deathAgeMonths >= ElderAgeMonths;
        int adultSuccessorCount = CountAdultHeirCandidatesAfterDeath(signals, deathTarget, registryQueries);
        bool hasAdultSuccessor = adultSuccessorCount > 0;
        bool hasLivingSpouse = deathTarget.SpouseId is not null
            && signals.LivingPeople.Any(entry => entry.Person.Id == deathTarget.SpouseId.Value);
        bool hasKnownLivingChild = deathTarget.ChildrenIds.Any(childId =>
            signals.LivingPeople.Any(entry => entry.Person.Id == childId));

        int deathRoleBand = wasHeir ? 3 : isElderDeath ? 2 : isChildDeath ? 1 : 1;
        int successionGapBand = wasHeir
            ? hasAdultSuccessor ? 1 : 3
            : hasAdultSuccessor ? 0 : 1;
        int dependentBand = isChildDeath
            ? 0
            : (hasLivingSpouse ? 1 : 0) + (hasKnownLivingChild ? 1 : 0);
        int ritualBurdenBand = isChildDeath ? 1 : isElderDeath ? 3 : 2;
        int supportFragilityBand = clan.SupportReserve < 20 ? 2 : clan.SupportReserve < 40 ? 1 : 0;

        int mourningDelta = isChildDeath
            ? 18
            : Math.Clamp(18 + (ritualBurdenBand * 2) + (wasHeir ? 2 : 0), 20, 28);
        int funeralDebtDelta = isChildDeath
            ? 6
            : wasHeir
                ? 18 + (hasAdultSuccessor ? 0 : 4)
                : Math.Clamp(10 + ritualBurdenBand + supportFragilityBand, 12, 18);
        int inheritanceDelta = isChildDeath
            ? 6
            : wasHeir
                ? Math.Clamp(12 + (successionGapBand * 4) - (hasKnownLivingChild ? 2 : 0), 14, 28)
                : Math.Clamp(5 + successionGapBand + dependentBand + (isElderDeath ? 2 : 0), 6, 12);
        int separationDelta = isChildDeath
            ? 1
            : wasHeir
                ? Math.Clamp(4 + (successionGapBand * 2) + supportFragilityBand, 5, 13)
                : Math.Clamp(1 + successionGapBand + supportFragilityBand, 2, 6);
        int branchDelta = isChildDeath
            ? 0
            : wasHeir
                ? Math.Clamp(3 + successionGapBand + supportFragilityBand, 4, 8)
                : Math.Clamp(successionGapBand + supportFragilityBand, 0, 4);
        int reproductiveDelta = isChildDeath
            ? 14
            : wasHeir && !hasAdultSuccessor
                ? 8
                : successionGapBand > 0 ? 3 : 0;
        int marriageDelta = isChildDeath
            ? 0
            : wasHeir && !hasAdultSuccessor
                ? 8
                : successionGapBand > 0 ? 3 : 0;
        int careDelta = isChildDeath
            ? -2
            : isElderDeath ? -4 : 0;

        string pressureSummary = string.Join(
            "、",
            $"死者名分{deathRoleBand}阶",
            $"承祧缺口{successionGapBand}阶",
            $"身后牵挂{dependentBand}阶",
            $"丧葬拖累{ritualBurdenBand}阶",
            $"宗房余力短处{supportFragilityBand}阶");

        return new FamilyDeathImpactProfile(
            mourningDelta,
            inheritanceDelta,
            separationDelta,
            branchDelta,
            funeralDebtDelta,
            reproductiveDelta,
            marriageDelta,
            careDelta,
            pressureSummary);
    }

    internal static void ApplyDeathImpactProfile(ClanStateData clan, FamilyDeathImpactProfile profile)
    {
        clan.MourningLoad = Math.Clamp(clan.MourningLoad + profile.MourningLoadDelta, 0, 100);
        clan.InheritancePressure = Math.Clamp(clan.InheritancePressure + profile.InheritancePressureDelta, 0, 100);
        clan.SeparationPressure = Math.Clamp(clan.SeparationPressure + profile.SeparationPressureDelta, 0, 100);
        clan.BranchTension = Math.Clamp(clan.BranchTension + profile.BranchTensionDelta, 0, 100);
        clan.FuneralDebt = Math.Clamp(clan.FuneralDebt + profile.FuneralDebtDelta, 0, 100);
        clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + profile.ReproductivePressureDelta, 0, 100);
        clan.MarriageAlliancePressure = Math.Clamp(clan.MarriageAlliancePressure + profile.MarriageAlliancePressureDelta, 0, 100);
        clan.CareLoad = Math.Clamp(clan.CareLoad + profile.CareLoadDelta, 0, 100);
    }

    private static int CountAdultHeirCandidatesAfterDeath(
        FamilyMonthSignals signals,
        FamilyPersonState deathTarget,
        IPersonRegistryQueries registryQueries)
    {
        return signals.LivingPeople.Count(entry =>
            entry.Person.Id != deathTarget.Id
            && entry.AgeMonths >= AdultAgeMonths
            && IsHeirEligibleGender(entry.Person.Id, registryQueries));
    }
}
