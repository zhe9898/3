using System;
using Zongzu.Modules.FamilyCore;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private readonly record struct FamilyConflictResolutionProfile(
        int BranchFavorPressureDelta,
        int BranchTensionDelta,
        int InheritancePressureDelta,
        int SeparationPressureDelta,
        int MediationMomentumDelta,
        int SupportReserveDelta,
        int ReliefSanctionPressureDelta,
        string ExecutionSummary);

    private static FamilyConflictResolutionProfile ResolveSupportSeniorBranchProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int inheritanceBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 8, 25, 45);
        int mediationBand = CommandResolutionBands.Score(clan.MediationMomentum, 10, 30, 55);
        int prestigeBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: Math.Clamp(12 + (inheritanceBand * 2) + prestigeBand, 12, 22),
            BranchTensionDelta: Math.Clamp(5 + branchPressureBand + inheritanceBand - (mediationBand / 2), 3, 14),
            InheritancePressureDelta: Math.Clamp(3 + inheritanceBand + (branchPressureBand / 2), 2, 8),
            SeparationPressureDelta: 0,
            MediationMomentumDelta: -Math.Clamp(2 + (branchPressureBand / 2), 1, 5),
            SupportReserveDelta: -Math.Clamp(1 + (supportBand >= 2 ? 1 : 0), 1, 3),
            ReliefSanctionPressureDelta: 0,
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("后议压力", inheritanceBand),
                new CommandResolutionFactor("宗房余力", supportBand),
                new CommandResolutionFactor("调停余势", mediationBand),
                new CommandResolutionFactor("门第声势", prestigeBand)));
    }

    private static FamilyConflictResolutionProfile ResolveFormalApologyProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int favorBand = CommandResolutionBands.Score(clan.BranchFavorPressure, 15, 35, 60);
        int prestigeBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: -Math.Clamp(2 + favorBand + (prestigeBand / 2), 2, 8),
            BranchTensionDelta: -Math.Clamp(8 + (branchPressureBand * 2) + prestigeBand, 6, 18),
            InheritancePressureDelta: 0,
            SeparationPressureDelta: -Math.Clamp(3 + separationBand + (prestigeBand / 2), 2, 9),
            MediationMomentumDelta: Math.Clamp(5 + prestigeBand + (branchPressureBand / 2), 5, 12),
            SupportReserveDelta: 0,
            ReliefSanctionPressureDelta: 0,
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("偏怨旧账", favorBand),
                new CommandResolutionFactor("门第声势", prestigeBand)));
    }

    private static FamilyConflictResolutionProfile ResolveBranchSeparationProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int inheritanceBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 8, 25, 45);
        int reliefBand = CommandResolutionBands.Score(clan.ReliefSanctionPressure, 15, 35, 60);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: 0,
            BranchTensionDelta: -Math.Clamp(3 + branchPressureBand + (supportBand / 2), 3, 9),
            InheritancePressureDelta: -Math.Clamp(4 + inheritanceBand + separationBand, 3, 12),
            SeparationPressureDelta: -Math.Clamp(12 + (separationBand * 4) + branchPressureBand, 10, 28),
            MediationMomentumDelta: Math.Clamp(2 + (supportBand / 2), 2, 5),
            SupportReserveDelta: -Math.Clamp(4 + separationBand - (supportBand / 2), 4, 10),
            ReliefSanctionPressureDelta: -Math.Clamp(2 + reliefBand, 1, 6),
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("后议压力", inheritanceBand),
                new CommandResolutionFactor("宗房余力", supportBand),
                new CommandResolutionFactor("救济制裁", reliefBand)));
    }

    private static FamilyConflictResolutionProfile ResolveSuspendReliefProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int reliefBand = CommandResolutionBands.Score(clan.ReliefSanctionPressure, 15, 35, 60);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 8, 25, 45);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: 0,
            BranchTensionDelta: Math.Clamp(5 + branchPressureBand + reliefBand, 5, 14),
            InheritancePressureDelta: 0,
            SeparationPressureDelta: Math.Clamp(3 + separationBand + reliefBand, 3, 10),
            MediationMomentumDelta: -Math.Clamp(2 + branchPressureBand, 2, 6),
            SupportReserveDelta: Math.Clamp(2 + reliefBand + (supportBand == 0 ? 1 : 0), 2, 6),
            ReliefSanctionPressureDelta: Math.Clamp(12 + (reliefBand * 2), 12, 22),
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("救济制裁", reliefBand),
                new CommandResolutionFactor("宗房余力", supportBand)));
    }

    private static FamilyConflictResolutionProfile ResolveClanEldersMediationProfile(ClanStateData clan, bool publicBroker)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int favorBand = CommandResolutionBands.Score(clan.BranchFavorPressure, 15, 35, 60);
        int inheritanceBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int prestigeBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);

        int publicDiscount = publicBroker ? 2 : 0;
        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: -Math.Clamp(3 + favorBand + (prestigeBand / 2) - publicDiscount, 2, 8),
            BranchTensionDelta: -Math.Clamp(5 + branchPressureBand + prestigeBand - publicDiscount, 3, 12),
            InheritancePressureDelta: publicBroker ? 0 : -Math.Clamp(1 + (inheritanceBand / 2), 1, 4),
            SeparationPressureDelta: -Math.Clamp(3 + separationBand + (prestigeBand / 2) - publicDiscount, 2, 8),
            MediationMomentumDelta: Math.Clamp((publicBroker ? 8 : 10) + prestigeBand + branchPressureBand + separationBand, publicBroker ? 8 : 10, publicBroker ? 18 : 22),
            SupportReserveDelta: -Math.Clamp(publicBroker ? 2 + (prestigeBand / 2) : 1 + prestigeBand, publicBroker ? 2 : 1, publicBroker ? 5 : 4),
            ReliefSanctionPressureDelta: 0,
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("偏怨旧账", favorBand),
                new CommandResolutionFactor("后议压力", inheritanceBand),
                new CommandResolutionFactor("门第声势", prestigeBand)));
    }

    private static void ApplyFamilyConflictProfile(ClanStateData clan, FamilyConflictResolutionProfile profile)
    {
        clan.BranchFavorPressure = ApplyPressureDelta(clan.BranchFavorPressure, profile.BranchFavorPressureDelta);
        clan.BranchTension = ApplyPressureDelta(clan.BranchTension, profile.BranchTensionDelta);
        clan.InheritancePressure = ApplyPressureDelta(clan.InheritancePressure, profile.InheritancePressureDelta);
        clan.SeparationPressure = ApplyPressureDelta(clan.SeparationPressure, profile.SeparationPressureDelta);
        clan.MediationMomentum = ApplyPressureDelta(clan.MediationMomentum, profile.MediationMomentumDelta);
        clan.SupportReserve = ApplyPressureDelta(clan.SupportReserve, profile.SupportReserveDelta);
        clan.ReliefSanctionPressure = ApplyPressureDelta(clan.ReliefSanctionPressure, profile.ReliefSanctionPressureDelta);
    }

    private static int ApplyPressureDelta(int value, int delta)
    {
        return CommandResolutionMath.Clamp100(value + delta);
    }
}
