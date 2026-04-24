using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public static partial class FamilyCoreCommandResolver
{
    private readonly record struct FamilyCommandSocialModifier(
        bool HasSignal,
        int ReliefBonus,
        int ReliefPenalty,
        int BacklashDelta,
        int SupportCostDelta,
        int HarshCommandPressure,
        int MediationMomentumDelta,
        string Trace)
    {
        public static FamilyCommandSocialModifier Neutral { get; } = new(false, 0, 0, 0, 0, 0, 0, string.Empty);
    }

    private readonly record struct FamilyConflictResolutionProfile(
        int BranchFavorPressureDelta,
        int BranchTensionDelta,
        int InheritancePressureDelta,
        int SeparationPressureDelta,
        int MediationMomentumDelta,
        int SupportReserveDelta,
        int ReliefSanctionPressureDelta,
        string ExecutionSummary);

    private readonly record struct FamilyMarriageResolutionProfile(
        int MarriageAllianceValueLift,
        int MarriageAlliancePressureRelief,
        int ReproductivePressureLift,
        int HeirSecurityLift,
        int BranchTensionRelief,
        int BranchTensionBacklash,
        int SupportCost,
        string ExecutionSummary);

    private readonly record struct FamilyHeirResolutionProfile(
        int HeirSecurityFloor,
        int HeirSecurityLift,
        int InheritancePressureRelief,
        int BranchTensionBacklash,
        int MediationMomentumLift,
        string ExecutionSummary);

    private readonly record struct FamilyLifecycleResolutionProfile(
        int SupportCost,
        int CareLoadRelief,
        int FuneralDebtRelief,
        int MourningLoadRelief,
        int HeirSecurityLift,
        int ReproductivePressureRelief,
        int InheritancePressureRelief,
        int BranchTensionRelief,
        int BranchTensionBacklash,
        int CharityObligationLift,
        int RemedyConfidenceLift,
        int MediationMomentumLift,
        string ExecutionSummary);
}
