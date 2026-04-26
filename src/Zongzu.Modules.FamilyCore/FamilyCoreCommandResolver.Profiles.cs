using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public static partial class FamilyCoreCommandResolver
{
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

    private static FamilyReliefResolutionProfile ResolveGrantClanReliefProfile(ClanStateData clan)
    {
        int obligationBand = CommandResolutionBands.Score(clan.CharityObligation, 12, 28, 48);
        int reliefBand = CommandResolutionBands.Score(clan.ReliefSanctionPressure, 10, 25, 45);
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 8, 25, 45);
        int prestigeBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);

        int supportCost = Math.Clamp(
            3 + obligationBand + reliefBand + Math.Max(0, 2 - supportBand),
            3,
            9);
        int charityRelief = Math.Clamp(7 + (obligationBand * 3) + supportBand + (prestigeBand / 2), 5, 22);
        int sanctionRelief = Math.Clamp(4 + (reliefBand * 3) + supportBand, 3, 16);
        int branchRelief = Math.Clamp(2 + branchPressureBand + supportBand + (prestigeBand / 2), 1, 10);
        int branchBacklash = clan.SupportReserve <= supportCost + 3 && branchPressureBand >= 2 ? 2 : 0;
        int favorRelief = clan.BranchFavorPressure > 0
            ? Math.Clamp(1 + reliefBand, 1, 5)
            : 0;
        int mediationLift = Math.Clamp(2 + supportBand + (prestigeBand / 2), 2, 8);

        return new FamilyReliefResolutionProfile(
            CharityObligationDelta: -charityRelief,
            SupportReserveDelta: -supportCost,
            BranchTensionDelta: -branchRelief + branchBacklash,
            BranchFavorPressureDelta: -favorRelief,
            ReliefSanctionPressureDelta: -sanctionRelief,
            MediationMomentumDelta: mediationLift,
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("接济义务", obligationBand),
                new CommandResolutionFactor("救济制裁", reliefBand),
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("宗房余力", supportBand),
                new CommandResolutionFactor("门第声势", prestigeBand)));
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

    private static void ApplyFamilyReliefProfile(ClanStateData clan, FamilyReliefResolutionProfile profile)
    {
        clan.CharityObligation = ApplyPressureDelta(clan.CharityObligation, profile.CharityObligationDelta);
        clan.SupportReserve = ApplyPressureDelta(clan.SupportReserve, profile.SupportReserveDelta);
        clan.BranchTension = ApplyPressureDelta(clan.BranchTension, profile.BranchTensionDelta);
        clan.BranchFavorPressure = ApplyPressureDelta(clan.BranchFavorPressure, profile.BranchFavorPressureDelta);
        clan.ReliefSanctionPressure = ApplyPressureDelta(clan.ReliefSanctionPressure, profile.ReliefSanctionPressureDelta);
        clan.MediationMomentum = ApplyPressureDelta(clan.MediationMomentum, profile.MediationMomentumDelta);
    }

    private static int ApplyPressureDelta(int value, int delta)
    {
        return CommandResolutionMath.Clamp100(value + delta);
    }

    private static FamilyMarriageResolutionProfile ResolveMarriageProfile(ClanStateData clan)
    {
        int alliancePressureBand = CommandResolutionBands.Score(clan.MarriageAlliancePressure, 20, 40, 65);
        int allianceValueBand = CommandResolutionBands.Score(clan.MarriageAllianceValue, 28, 48, 70);
        int affinalNeedBand = Math.Clamp(3 - allianceValueBand, 0, 3);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 12, 30, 50);
        int standingBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);
        int heirFragility = clan.HeirSecurity < 35 ? 2 : clan.HeirSecurity < 60 ? 1 : 0;
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int mourningDrag = clan.MourningLoad >= 24 ? 2 : clan.MourningLoad >= 12 ? 1 : 0;

        int valueLift = Math.Clamp(18 + (alliancePressureBand * 4) + (standingBand * 2) - (mourningDrag * 3), 12, 36);
        int pressureRelief = Math.Clamp(12 + (alliancePressureBand * 4) + (supportBand * 2) - (mourningDrag * 3), 6, 30);
        int reproductiveLift = Math.Clamp(4 + (heirFragility * 2) + alliancePressureBand - mourningDrag, 2, 12);
        int heirSecurityLift = Math.Clamp(3 + (heirFragility * 2) + standingBand - mourningDrag, 1, 10);
        int branchTensionRelief = clan.BranchTension >= 20 ? Math.Clamp(1 + standingBand + (supportBand / 2), 1, 5) : 0;
        int branchBacklash = supportBand == 0 && clan.BranchTension >= 35 ? 2 : mourningDrag > 0 && branchPressureBand >= 2 ? 1 : 0;
        int supportCost = Math.Clamp(2 + affinalNeedBand + (standingBand >= 2 ? 1 : 0) - (supportBand / 2), 2, 7);
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("婚议之压", alliancePressureBand),
            new CommandResolutionFactor("姻亲短处", affinalNeedBand),
            new CommandResolutionFactor("宗房余力", supportBand),
            new CommandResolutionFactor("门第声势", standingBand),
            new CommandResolutionFactor("丧服拖累", mourningDrag),
            new CommandResolutionFactor("承祧虚处", heirFragility),
            new CommandResolutionFactor("房支争势", branchPressureBand));

        return new FamilyMarriageResolutionProfile(
            valueLift,
            pressureRelief,
            reproductiveLift,
            heirSecurityLift,
            branchTensionRelief,
            branchBacklash,
            supportCost,
            executionSummary);
    }

    private static FamilyHeirResolutionProfile ResolveHeirPolicyProfile(ClanStateData clan, int candidateAgeMonths)
    {
        bool isAdultCandidate = candidateAgeMonths >= FamilyCoreModule.AdultAgeMonths;
        int candidateStabilityBand = isAdultCandidate
            ? CommandResolutionBands.Score(candidateAgeMonths, FamilyCoreModule.AdultAgeMonths, 20 * 12, 28 * 12)
            : CommandResolutionBands.Score(candidateAgeMonths, 4 * 12, 8 * 12, 12 * 12);
        int heirFragility = clan.HeirSecurity < 35 ? 2 : clan.HeirSecurity < 60 ? 1 : 0;
        int inheritancePressureBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int mediationBand = CommandResolutionBands.Score(clan.MediationMomentum, 10, 30, 55);

        int floorBase = isAdultCandidate ? 58 : 34;
        int heirSecurityFloor = Math.Clamp(floorBase + (candidateStabilityBand * 3) + (isAdultCandidate ? mediationBand : 0), 30, 72);
        int heirSecurityLift = Math.Clamp(2 + (heirFragility * 3) + candidateStabilityBand + (mediationBand / 2), 1, 12);
        int inheritanceRelief = Math.Clamp(5 + (candidateStabilityBand * 2) + mediationBand + inheritancePressureBand + (isAdultCandidate ? 2 : 0) - (branchPressureBand / 2), 3, 18);
        int branchBacklash = Math.Clamp(1 + branchPressureBand + heirFragility - (mediationBand / 2) - (isAdultCandidate ? 1 : 0), 0, 6);
        int mediationMomentumLift = Math.Clamp(1 + (mediationBand / 2) + (isAdultCandidate ? 1 : 0), 1, 4);
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("候选稳度", candidateStabilityBand),
            new CommandResolutionFactor("承祧虚处", heirFragility),
            new CommandResolutionFactor("后议压力", inheritancePressureBand),
            new CommandResolutionFactor("房支争势", branchPressureBand),
            new CommandResolutionFactor("调停余势", mediationBand));

        return new FamilyHeirResolutionProfile(
            heirSecurityFloor,
            heirSecurityLift,
            inheritanceRelief,
            branchBacklash,
            mediationMomentumLift,
            executionSummary);
    }

    private static FamilyLifecycleResolutionProfile ResolveNewbornCareProfile(ClanStateData clan, int infantCount)
    {
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 18, 36, 58);
        int careBand = CommandResolutionBands.Score(clan.CareLoad, 12, 30, 55);
        int remedyBand = CommandResolutionBands.Score(clan.RemedyConfidence, 25, 50, 72);
        int mourningDrag = clan.MourningLoad >= 24 ? 2 : clan.MourningLoad >= 12 ? 1 : 0;
        int infantBand = Math.Clamp(infantCount, 1, 3);

        int supportCost = Math.Clamp(3 + infantBand + careBand - supportBand, 4, 9);
        int careRelief = Math.Clamp(3 + supportBand + remedyBand + infantBand - mourningDrag, 2, 10);
        int heirSecurityLift = Math.Clamp(2 + infantBand + remedyBand - mourningDrag, 1, 8);
        int reproductivePressureRelief = Math.Clamp(2 + careBand + (remedyBand / 2), 1, 7);
        int branchTensionRelief = clan.BranchTension >= 25 ? Math.Clamp(1 + supportBand, 1, 4) : 0;
        int branchBacklash = clan.SupportReserve <= supportCost + 4 && clan.BranchTension >= 35 ? 2 : 0;
        int charityObligationLift = Math.Clamp(1 + infantBand, 1, 4);
        int remedyConfidenceLift = clan.RemedyConfidence >= 80 ? 0 : supportBand >= 1 ? 2 : 1;
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("宗房余力", supportBand),
            new CommandResolutionFactor("照料负担", careBand),
            new CommandResolutionFactor("药护信心", remedyBand),
            new CommandResolutionFactor("丧服拖累", mourningDrag),
            new CommandResolutionFactor("襁褓口数", infantBand));

        return new FamilyLifecycleResolutionProfile(
            supportCost,
            careRelief,
            0,
            0,
            heirSecurityLift,
            reproductivePressureRelief,
            0,
            branchTensionRelief,
            branchBacklash,
            charityObligationLift,
            remedyConfidenceLift,
            0,
            executionSummary);
    }

    private static FamilyLifecycleResolutionProfile ResolveMourningOrderProfile(ClanStateData clan)
    {
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 12, 28, 48);
        int mourningBand = CommandResolutionBands.Score(clan.MourningLoad, 12, 28, 50);
        int funeralDebtBand = CommandResolutionBands.Score(clan.FuneralDebt, 8, 20, 40);
        int ritualStandingBand = CommandResolutionBands.Score(clan.Prestige + clan.MediationMomentum, 55, 85, 115);
        int heirFragility = clan.HeirSecurity < 35 ? 2 : clan.HeirSecurity < 55 ? 1 : 0;

        int supportCost = Math.Clamp(2 + funeralDebtBand + Math.Max(0, ritualStandingBand - 1), 2, 8);
        int mourningLoadRelief = Math.Clamp(5 + ritualStandingBand + supportBand - heirFragility, 4, 14);
        int funeralDebtRelief = Math.Clamp(2 + supportBand + ritualStandingBand - (clan.SupportReserve < 8 ? 1 : 0), 1, 9);
        int inheritancePressureRelief = Math.Clamp(2 + ritualStandingBand + mourningBand - heirFragility, 0, 9);
        int branchTensionRelief = clan.BranchTension > 0 ? Math.Clamp(1 + ritualStandingBand + (mourningBand / 2), 1, 6) : 0;
        int branchBacklash = heirFragility > 0 && clan.InheritancePressure >= 35 ? heirFragility : 0;
        int mediationMomentumLift = ritualStandingBand > 0 ? 1 : 0;
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("礼法威望", ritualStandingBand),
            new CommandResolutionFactor("宗房余力", supportBand),
            new CommandResolutionFactor("丧服之重", mourningBand),
            new CommandResolutionFactor("丧葬债压", funeralDebtBand),
            new CommandResolutionFactor("承祧虚处", heirFragility));

        return new FamilyLifecycleResolutionProfile(
            supportCost,
            0,
            funeralDebtRelief,
            mourningLoadRelief,
            0,
            0,
            inheritancePressureRelief,
            branchTensionRelief,
            branchBacklash,
            0,
            0,
            mediationMomentumLift,
            executionSummary);
    }

    private static string RenderLifecycleBacklash(FamilyLifecycleResolutionProfile profile)
    {
        return RenderBranchBacklash(profile.BranchTensionBacklash);
    }

    private static string RenderBranchBacklash(int branchTensionBacklash)
    {
        return branchTensionBacklash > 0
            ? $"诸房仍有{branchTensionBacklash}点反弹留在堂内。"
            : string.Empty;
    }

}
