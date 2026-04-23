using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public static partial class FamilyCoreCommandResolver
{
    private static FamilyCommandSocialModifier BuildSocialModifier(FamilyCoreCommandContext context, ClanStateData clan)
    {
        if (context.SocialMemoryQueries is null)
        {
            return FamilyCommandSocialModifier.Neutral;
        }

        ClanEmotionalClimateSnapshot climate;
        try
        {
            climate = context.SocialMemoryQueries.GetClanEmotionalClimate(clan.Id);
        }
        catch (InvalidOperationException)
        {
            return FamilyCommandSocialModifier.Neutral;
        }

        PersonPressureTemperingSnapshot[] adultTemperings = context.SocialMemoryQueries
            .GetPersonTemperingsByClan(clan.Id)
            .Where(tempering => IsAdultAlive(context, tempering.PersonId))
            .OrderBy(static tempering => tempering.PersonId.Value)
            .ToArray();

        int adultCount = Math.Max(1, adultTemperings.Length);
        int avgRestraint = adultTemperings.Sum(static tempering => tempering.Restraint) / adultCount;
        int avgHardening = adultTemperings.Sum(static tempering => tempering.Hardening) / adultCount;
        int avgBitterness = adultTemperings.Sum(static tempering => tempering.Bitterness) / adultCount;
        int avgVolatility = adultTemperings.Sum(static tempering => tempering.Volatility) / adultCount;
        int avgTrust = adultTemperings.Sum(static tempering => tempering.Trust) / adultCount;

        int volatilityBand = CommandResolutionBands.Score(Math.Max(climate.Volatility, avgVolatility), 45, 65, 82);
        int bitternessBand = CommandResolutionBands.Score(Math.Max(climate.Bitterness, avgBitterness), 45, 65, 82);
        int hardeningBand = CommandResolutionBands.Score(Math.Max(climate.Hardening, avgHardening), 45, 65, 82);
        int trustBand = CommandResolutionBands.Score(Math.Max(climate.Trust, avgTrust), 45, 65, 82);
        int restraintBand = CommandResolutionBands.Score(Math.Max(climate.Restraint, avgRestraint), 45, 65, 82);
        int obligationBand = CommandResolutionBands.Score(climate.Obligation, 45, 65, 82);
        int griefBand = CommandResolutionBands.Score(climate.Grief, 45, 65, 82);
        int shameBand = CommandResolutionBands.Score(climate.Shame, 45, 65, 82);
        int angerBand = CommandResolutionBands.Score(climate.Anger, 45, 65, 82);

        int resistanceBand = Math.Clamp(volatilityBand + bitternessBand + (angerBand / 2) - restraintBand - trustBand, 0, 6);
        int cohesionBand = Math.Clamp(trustBand + restraintBand + obligationBand - bitternessBand, 0, 6);
        int backlashDelta = Math.Clamp(resistanceBand + griefBand - trustBand, 0, 5);
        int reliefBonus = Math.Clamp(cohesionBand - resistanceBand, 0, 4);
        int reliefPenalty = Math.Clamp(resistanceBand - cohesionBand, 0, 4);
        int supportCostDelta = Math.Clamp(griefBand + shameBand + volatilityBand - trustBand - restraintBand, -2, 4);
        int harshCommandPressure = Math.Clamp(hardeningBand + angerBand - trustBand, 0, 4);
        bool hasSignal = volatilityBand + bitternessBand + hardeningBand + trustBand + restraintBand + obligationBand + griefBand + shameBand + angerBand > 0;

        string trace = hasSignal
            ? CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("余怨", bitternessBand),
                new CommandResolutionFactor("克制", restraintBand),
                new CommandResolutionFactor("信义", trustBand),
                new CommandResolutionFactor("躁动", volatilityBand),
                new CommandResolutionFactor("硬气", hardeningBand))
            : string.Empty;

        return new FamilyCommandSocialModifier(
            hasSignal,
            reliefBonus,
            reliefPenalty,
            backlashDelta,
            supportCostDelta,
            harshCommandPressure,
            Math.Clamp(cohesionBand - resistanceBand, -3, 5),
            trace);
    }

    private static bool IsAdultAlive(FamilyCoreCommandContext context, PersonId personId)
    {
        FamilyPersonState? person = context.State.People.FirstOrDefault(person => person.Id == personId);
        return person is not null
            && IsFamilyCommandPersonAlive(person, context.PersonRegistryQueries)
            && GetFamilyCommandAgeMonths(person, context.PersonRegistryQueries, context.CurrentDate) >= FamilyCoreModule.AdultAgeMonths;
    }

    private static FamilyConflictResolutionProfile ApplyConflictSocialModifier(
        string commandName,
        FamilyConflictResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        bool isReconciliation = commandName is PlayerCommandNames.OrderFormalApology
            or PlayerCommandNames.InviteClanEldersMediation
            or PlayerCommandNames.InviteClanEldersPubliclyBroker;
        bool isHarsh = commandName is PlayerCommandNames.SupportSeniorBranch
            or PlayerCommandNames.SuspendClanRelief;

        int reliefShift = isReconciliation ? social.ReliefBonus - social.ReliefPenalty : 0;
        int harshShift = isHarsh ? social.HarshCommandPressure : 0;

        return profile with
        {
            BranchFavorPressureDelta = ApplySignedReliefShift(profile.BranchFavorPressureDelta, reliefShift),
            BranchTensionDelta = ApplySignedReliefShift(profile.BranchTensionDelta, reliefShift) + harshShift + (isReconciliation ? 0 : social.BacklashDelta / 2),
            InheritancePressureDelta = ApplySignedReliefShift(profile.InheritancePressureDelta, reliefShift / 2),
            SeparationPressureDelta = ApplySignedReliefShift(profile.SeparationPressureDelta, reliefShift) + (isHarsh ? social.BacklashDelta / 2 : 0),
            MediationMomentumDelta = profile.MediationMomentumDelta + (isReconciliation ? social.MediationMomentumDelta : -social.ReliefPenalty),
            SupportReserveDelta = profile.SupportReserveDelta - Math.Max(0, social.SupportCostDelta),
            ReliefSanctionPressureDelta = profile.ReliefSanctionPressureDelta + (isHarsh ? social.HarshCommandPressure : 0),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static FamilyMarriageResolutionProfile ApplyMarriageSocialModifier(
        FamilyMarriageResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        return profile with
        {
            MarriageAllianceValueLift = Math.Max(1, profile.MarriageAllianceValueLift + social.ReliefBonus - (social.ReliefPenalty / 2)),
            MarriageAlliancePressureRelief = Math.Max(1, profile.MarriageAlliancePressureRelief + social.ReliefBonus - social.ReliefPenalty),
            HeirSecurityLift = Math.Max(0, profile.HeirSecurityLift + (social.ReliefBonus / 2) - (social.BacklashDelta / 2)),
            BranchTensionRelief = Math.Max(0, profile.BranchTensionRelief + social.ReliefBonus - social.ReliefPenalty),
            BranchTensionBacklash = Math.Max(0, profile.BranchTensionBacklash + social.BacklashDelta - social.ReliefBonus),
            SupportCost = Math.Max(1, profile.SupportCost + Math.Max(0, social.SupportCostDelta)),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static FamilyHeirResolutionProfile ApplyHeirSocialModifier(
        FamilyHeirResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        return profile with
        {
            HeirSecurityFloor = Math.Clamp(profile.HeirSecurityFloor + social.ReliefBonus - social.ReliefPenalty, 25, 78),
            HeirSecurityLift = Math.Max(0, profile.HeirSecurityLift + social.ReliefBonus - (social.ReliefPenalty / 2)),
            InheritancePressureRelief = Math.Max(1, profile.InheritancePressureRelief + social.ReliefBonus - social.ReliefPenalty),
            BranchTensionBacklash = Math.Max(0, profile.BranchTensionBacklash + social.BacklashDelta - social.ReliefBonus),
            MediationMomentumLift = Math.Max(0, profile.MediationMomentumLift + social.MediationMomentumDelta),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static FamilyLifecycleResolutionProfile ApplyLifecycleSocialModifier(
        string commandName,
        FamilyLifecycleResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        bool isCare = commandName == PlayerCommandNames.SupportNewbornCare;
        bool isMourning = commandName == PlayerCommandNames.SetMourningOrder;
        int reliefBonus = isCare || isMourning ? social.ReliefBonus : 0;

        return profile with
        {
            SupportCost = Math.Max(0, profile.SupportCost + social.SupportCostDelta),
            CareLoadRelief = Math.Max(0, profile.CareLoadRelief + (isCare ? reliefBonus - social.ReliefPenalty : 0)),
            FuneralDebtRelief = Math.Max(0, profile.FuneralDebtRelief + (isMourning ? reliefBonus - social.ReliefPenalty : 0)),
            MourningLoadRelief = Math.Max(0, profile.MourningLoadRelief + (isMourning ? reliefBonus - social.ReliefPenalty : 0)),
            HeirSecurityLift = Math.Max(0, profile.HeirSecurityLift + (reliefBonus / 2) - (social.BacklashDelta / 2)),
            ReproductivePressureRelief = Math.Max(0, profile.ReproductivePressureRelief + (isCare ? reliefBonus / 2 : 0)),
            InheritancePressureRelief = Math.Max(0, profile.InheritancePressureRelief + (isMourning ? reliefBonus / 2 : 0)),
            BranchTensionRelief = Math.Max(0, profile.BranchTensionRelief + reliefBonus - social.ReliefPenalty),
            BranchTensionBacklash = Math.Max(0, profile.BranchTensionBacklash + social.BacklashDelta - reliefBonus),
            CharityObligationLift = Math.Max(0, profile.CharityObligationLift + (isCare ? reliefBonus / 2 : 0)),
            RemedyConfidenceLift = Math.Max(0, profile.RemedyConfidenceLift + (isCare ? reliefBonus / 2 : 0)),
            MediationMomentumLift = Math.Max(0, profile.MediationMomentumLift + (isMourning ? social.MediationMomentumDelta : 0)),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static int ApplySignedReliefShift(int delta, int reliefShift)
    {
        if (delta < 0)
        {
            return delta - reliefShift;
        }

        if (delta > 0)
        {
            return Math.Max(0, delta - reliefShift);
        }

        return delta;
    }

    private static string AppendSocialTrace(string executionSummary, FamilyCommandSocialModifier social)
    {
        return social.HasSignal ? $"{executionSummary}、{social.Trace}" : executionSummary;
    }

}
