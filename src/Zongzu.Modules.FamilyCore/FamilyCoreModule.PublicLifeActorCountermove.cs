using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed partial class FamilyCoreModule
{
    private static bool ApplyPublicLifeFamilyActorCountermove(
        ModuleExecutionScope<FamilyCoreState> scope,
        ClanStateData clan,
        ISocialMemoryAndRelationsQueries? socialQueries)
    {
        if (socialQueries is null)
        {
            return false;
        }

        PublicLifeResponseResidueWeights weights = PublicLifeResponseResidueWeights.FromSocialMemory(
            socialQueries,
            clan.Id,
            scope.Context.CurrentDate);
        if (weights.IsNeutral)
        {
            return false;
        }

        ClanNarrativeSnapshot narrative = socialQueries.GetRequiredClanNarrative(clan.Id);
        if (weights.HardeningDrag >= Math.Max(24, weights.SoftSupport + 10)
            && (narrative.ShamePressure >= 45 || clan.Prestige < 35 || clan.BranchTension >= 55)
            && !string.Equals(
                clan.LastRefusalResponseTraceCode,
                PublicLifeOrderResponseTraceCodes.FamilyActorGuaranteeAvoided,
                StringComparison.Ordinal))
        {
            clan.Prestige = Math.Clamp(clan.Prestige - 1, 0, 100);
            clan.BranchTension = Math.Clamp(clan.BranchTension + 3, 0, 100);
            clan.MediationMomentum = Math.Clamp(clan.MediationMomentum - 1, 0, 100);
            clan.LastConflictCommandCode = PlayerCommandNames.AskClanEldersExplain;
            clan.LastConflictCommandLabel = "族老避羞";
            clan.LastConflictOutcome = "后账放置";
            clan.LastConflictTrace =
                $"旧案恶化余重{weights.HardeningDrag}压在街口，族老避开公开担保，羞面未解，房支争力{clan.BranchTension}。";
            clan.LastRefusalResponseCommandCode = PlayerCommandNames.AskClanEldersExplain;
            clan.LastRefusalResponseCommandLabel = "族老避羞";
            clan.LastRefusalResponseSummary = clan.LastConflictTrace;
            clan.LastRefusalResponseOutcomeCode = PublicLifeOrderResponseOutcomeCodes.Ignored;
            clan.LastRefusalResponseTraceCode = PublicLifeOrderResponseTraceCodes.FamilyActorGuaranteeAvoided;
            clan.ResponseCarryoverMonths = 1;
            scope.RecordDiff(
                $"{clan.ClanName}族老避开前案担保，房支争力{clan.BranchTension}，调停势{clan.MediationMomentum}。",
                clan.Id.Value.ToString());
            return true;
        }

        if (weights.SoftSupport >= 18
            && (narrative.ShamePressure >= 18 || clan.BranchTension >= 20 || clan.MediationMomentum < 65)
            && !string.Equals(
                clan.LastRefusalResponseTraceCode,
                PublicLifeOrderResponseTraceCodes.FamilyActorElderQuietExplanation,
                StringComparison.Ordinal))
        {
            clan.Prestige = Math.Clamp(clan.Prestige + 1, 0, 100);
            clan.BranchTension = Math.Clamp(clan.BranchTension - 2, 0, 100);
            clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + 4, 0, 100);
            clan.LastConflictCommandCode = PlayerCommandNames.AskClanEldersExplain;
            clan.LastConflictCommandLabel = "族老自解释";
            clan.LastConflictOutcome = weights.RepairedWeight >= weights.ContainedWeight ? "后账已修复" : "后账暂压";
            clan.LastConflictTrace =
                $"修复余重{weights.RepairedWeight}、暂压余重{weights.ContainedWeight}给了台阶，族老私下解释前案，本户担保略稳，调停势{clan.MediationMomentum}。";
            clan.LastRefusalResponseCommandCode = PlayerCommandNames.AskClanEldersExplain;
            clan.LastRefusalResponseCommandLabel = "族老自解释";
            clan.LastRefusalResponseSummary = clan.LastConflictTrace;
            clan.LastRefusalResponseOutcomeCode = weights.RepairedWeight >= weights.ContainedWeight
                ? PublicLifeOrderResponseOutcomeCodes.Repaired
                : PublicLifeOrderResponseOutcomeCodes.Contained;
            clan.LastRefusalResponseTraceCode = PublicLifeOrderResponseTraceCodes.FamilyActorElderQuietExplanation;
            clan.ResponseCarryoverMonths = 1;
            scope.RecordDiff(
                $"{clan.ClanName}族老私下解释前案，调停势{clan.MediationMomentum}，房支争力{clan.BranchTension}。",
                clan.Id.Value.ToString());
            return true;
        }

        return false;
    }

    private static ISocialMemoryAndRelationsQueries? TryGetSocialMemoryQueries(ModuleExecutionScope<FamilyCoreState> scope)
    {
        try
        {
            return scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private readonly record struct PublicLifeResponseResidueWeights(
        int RepairedWeight,
        int ContainedWeight,
        int EscalatedWeight,
        int IgnoredWeight)
    {
        public bool IsNeutral => RepairedWeight == 0 && ContainedWeight == 0 && EscalatedWeight == 0 && IgnoredWeight == 0;

        public int SoftSupport => RepairedWeight + (ContainedWeight / 2);

        public int HardeningDrag => EscalatedWeight + (IgnoredWeight / 2);

        public static PublicLifeResponseResidueWeights FromSocialMemory(
            ISocialMemoryAndRelationsQueries socialQueries,
            ClanId clanId,
            GameDate currentDate)
        {
            int repaired = 0;
            int contained = 0;
            int escalated = 0;
            int ignored = 0;

            foreach (SocialMemoryEntrySnapshot memory in socialQueries.GetMemoriesByClan(clanId)
                         .Where(static memory => memory.State == MemoryLifecycleState.Active)
                         .Where(static memory => memory.CauseKey.StartsWith("order.public_life.response.", StringComparison.Ordinal))
                         .Where(memory => !IsCurrentMonth(memory.OriginDate, currentDate)))
            {
                if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Repaired}.", StringComparison.Ordinal))
                {
                    repaired += memory.Weight;
                }
                else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Contained}.", StringComparison.Ordinal))
                {
                    contained += memory.Weight;
                }
                else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Escalated}.", StringComparison.Ordinal))
                {
                    escalated += memory.Weight;
                }
                else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Ignored}.", StringComparison.Ordinal))
                {
                    ignored += memory.Weight;
                }
            }

            return new PublicLifeResponseResidueWeights(
                Math.Clamp(repaired, 0, 200),
                Math.Clamp(contained, 0, 200),
                Math.Clamp(escalated, 0, 200),
                Math.Clamp(ignored, 0, 200));
        }

        private static bool IsCurrentMonth(GameDate date, GameDate currentDate)
        {
            return date.Year == currentDate.Year && date.Month == currentDate.Month;
        }
    }
}
