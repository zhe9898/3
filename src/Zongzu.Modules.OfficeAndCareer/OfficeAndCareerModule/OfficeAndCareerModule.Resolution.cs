using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
{
    private static AdministrativeResolution ResolveAdministrativeWork(
        EducationCandidateSnapshot candidate,
        ClanNarrativeSnapshot narrative,
        OfficeCareerState career,
        int authorityTier,
        int reputationSignal,
        int petitionBase)
    {
        AdministrativeTaskPlan taskPlan = DetermineAdministrativeTaskPlan(authorityTier, petitionBase, narrative);
        int taskLoad = Math.Clamp(
            8
            + (petitionBase / 3)
            + (narrative.GrudgePressure / 6)
            + (narrative.FearPressure / 7)
            + (candidate.Stress / 5)
            - (authorityTier * 2),
            0,
            100);
        int petitionBacklog = Math.Clamp(
            (petitionBase / 2)
            + (narrative.FearPressure / 5)
            + (narrative.GrudgePressure / 4)
            + (candidate.Stress / 5)
            - (authorityTier * 3),
            0,
            100);
        int administrativeScore = reputationSignal
            + (career.OfficeReputation / 3)
            + (authorityTier * 8)
            + (narrative.FavorBalance / 2)
            - (narrative.ShamePressure / 5)
            - taskLoad
            - (petitionBacklog / 2)
            - (candidate.Stress / 3);

        if (administrativeScore >= 48)
        {
            return new AdministrativeResolution(
                taskPlan.TaskName,
                taskLoad,
                Math.Max(0, petitionBacklog - 14),
                OfficeAndCareerDescriptors.FormatPetitionOutcome("Cleared", $"今以{taskPlan.TaskName}清理诸状，积案已消。"),
                -10,
                4,
                4,
                -6,
                2);
        }

        if (administrativeScore >= 32)
        {
            return new AdministrativeResolution(
                taskPlan.TaskName,
                taskLoad,
                Math.Max(0, petitionBacklog - 6),
                OfficeAndCareerDescriptors.FormatPetitionOutcome("Triaged", $"正在{taskPlan.TaskName}，诸状分轻重收理。"),
                -4,
                2,
                2,
                -2,
                1);
        }

        if (administrativeScore >= 18)
        {
            return new AdministrativeResolution(
                taskPlan.TaskName,
                taskLoad,
                Math.Clamp(petitionBacklog + 6, 0, 100),
                OfficeAndCareerDescriptors.FormatPetitionOutcome("Delayed", $"正在{taskPlan.TaskName}，其余词牍暂缓。"),
                4,
                0,
                0,
                4,
                0);
        }

        return new AdministrativeResolution(
            taskPlan.TaskName,
            taskLoad,
            Math.Clamp(petitionBacklog + 12, 0, 100),
            OfficeAndCareerDescriptors.FormatPetitionOutcome("Stalled", $"因{taskPlan.TaskName}不济，积案壅滞，怨词渐深。"),
            8,
            -2,
            -2,
            8,
            -1);
    }

    private static AdministrativeTaskPlan DetermineAdministrativeTaskPlan(int authorityTier, int petitionPressure, ClanNarrativeSnapshot narrative)
    {
        if (authorityTier >= 3 && narrative.GrudgePressure >= 45)
        {
            return new AdministrativeTaskPlan("crisis", "勘解乡怨词牍");
        }

        if (authorityTier >= 3 && petitionPressure >= 55)
        {
            return new AdministrativeTaskPlan("crisis", "急牍覆核");
        }

        if (authorityTier >= 2 && narrative.FearPressure >= 45)
        {
            return new AdministrativeTaskPlan("district", "巡丁清点");
        }

        if (authorityTier >= 2 && petitionPressure >= 45)
        {
            return new AdministrativeTaskPlan("district", "勘理词状");
        }

        if (authorityTier >= 2)
        {
            return new AdministrativeTaskPlan("registry", "勾检户籍");
        }

        if (petitionPressure >= 45)
        {
            return new AdministrativeTaskPlan("clerical", "誊录词牍");
        }

        return new AdministrativeTaskPlan("clerical", "誊黄封牍");
    }

    private sealed record AdministrativeResolution(
        string TaskName,
        int TaskLoad,
        int PetitionBacklog,
        string PetitionOutcome,
        int PetitionPressureAdjustment,
        int LeverageAdjustment,
        int PromotionMomentumAdjustment,
        int DemotionPressureAdjustment,
        int OfficeReputationAdjustment);

    private readonly record struct OrderAdministrativeAftermath(
        string CommandLabel,
        string TaskName,
        string PetitionOutcomeCategory,
        string PetitionOutcomeDetail,
        int TaskLoadDelta,
        int PetitionBacklogDelta,
        int PetitionPressureDelta,
        int ClerkDependenceDelta,
        int JurisdictionLeverageDelta,
        int DemotionPressureDelta,
        int PromotionMomentumDelta)
    {
        public bool HasImpact =>
            !string.IsNullOrWhiteSpace(CommandLabel)
            && (TaskLoadDelta != 0
                || PetitionBacklogDelta != 0
                || PetitionPressureDelta != 0
                || ClerkDependenceDelta != 0
                || JurisdictionLeverageDelta != 0
                || DemotionPressureDelta != 0
                || PromotionMomentumDelta != 0
                || !string.IsNullOrWhiteSpace(TaskName));
    }

    private sealed record AdministrativeTaskPlan(string TaskTier, string TaskName);
}
