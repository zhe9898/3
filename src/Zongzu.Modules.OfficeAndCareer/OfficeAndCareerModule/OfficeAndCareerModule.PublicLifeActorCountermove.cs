using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule
{
    private static void ApplyPublicLifeOfficeActorCountermoves(
        ModuleExecutionScope<OfficeAndCareerState> scope,
        IFamilyCoreQueries? familyQueries,
        ISocialMemoryAndRelationsQueries socialQueries)
    {
        if (familyQueries is null)
        {
            return;
        }

        ILookup<int, ClanSnapshot> clansBySettlement = familyQueries.GetClans()
            .ToLookup(static clan => clan.HomeSettlementId.Value);

        foreach (IGrouping<SettlementId, OfficeCareerState> careersBySettlement in scope.State.People
                     .Where(static career => career.HasAppointment)
                     .OrderBy(static career => career.SettlementId.Value)
                     .GroupBy(static career => career.SettlementId))
        {
            OfficeCareerState leadCareer = careersBySettlement
                .OrderByDescending(static career => career.AuthorityTier)
                .ThenByDescending(static career => career.OfficeReputation)
                .ThenBy(static career => career.PersonId.Value)
                .First();
            PublicLifeResponseResidueWeights weights = PublicLifeResponseResidueWeights.FromSocialMemory(
                socialQueries,
                clansBySettlement[careersBySettlement.Key.Value].Select(static clan => clan.Id),
                scope.Context.CurrentDate);
            if (weights.IsNeutral)
            {
                continue;
            }

            if (weights.HardeningDrag >= Math.Max(24, weights.SoftSupport + 10)
                && (leadCareer.ClerkDependence >= 45 || leadCareer.PetitionBacklog >= 30)
                && !string.Equals(
                    leadCareer.LastRefusalResponseTraceCode,
                    PublicLifeOrderResponseTraceCodes.OfficeActorClerkDelayContinued,
                    StringComparison.Ordinal))
            {
                leadCareer.ClerkDependence = Math.Clamp(leadCareer.ClerkDependence + 4, 0, 100);
                leadCareer.PetitionBacklog = Math.Clamp(leadCareer.PetitionBacklog + 3, 0, 100);
                leadCareer.PetitionPressure = Math.Clamp(leadCareer.PetitionPressure + 2, 0, 100);
                leadCareer.AdministrativeTaskLoad = Math.Clamp(leadCareer.AdministrativeTaskLoad + 2, 0, 100);
                leadCareer.CurrentAdministrativeTask = "拖勘前案";
                leadCareer.LastPetitionOutcome =
                    $"胥吏续拖：旧案恶化余重{weights.HardeningDrag}仍在，文移被压回案牍，积案{leadCareer.PetitionBacklog}。";
                leadCareer.LastExplanation = leadCareer.LastPetitionOutcome;
                leadCareer.LastRefusalResponseCommandCode = PlayerCommandNames.PressCountyYamenDocument;
                leadCareer.LastRefusalResponseCommandLabel = "胥吏续拖";
                leadCareer.LastRefusalResponseSummary = leadCareer.LastPetitionOutcome;
                leadCareer.LastRefusalResponseOutcomeCode = PublicLifeOrderResponseOutcomeCodes.Escalated;
                leadCareer.LastRefusalResponseTraceCode = PublicLifeOrderResponseTraceCodes.OfficeActorClerkDelayContinued;
                leadCareer.ResponseCarryoverMonths = 1;
                scope.RecordDiff(
                    $"{leadCareer.DisplayName}任上胥吏续拖前案，积案{leadCareer.PetitionBacklog}，牵制{leadCareer.ClerkDependence}。",
                    careersBySettlement.Key.Value.ToString());
                continue;
            }

            if (weights.SoftSupport >= 18
                && (leadCareer.PetitionBacklog > 0 || leadCareer.ClerkDependence >= 20 || leadCareer.AdministrativeTaskLoad >= 15)
                && !string.Equals(
                    leadCareer.LastRefusalResponseTraceCode,
                    PublicLifeOrderResponseTraceCodes.OfficeActorYamenQuietLanding,
                    StringComparison.Ordinal))
            {
                leadCareer.ClerkDependence = Math.Clamp(leadCareer.ClerkDependence - 2, 0, 100);
                leadCareer.PetitionBacklog = Math.Clamp(leadCareer.PetitionBacklog - 3, 0, 100);
                leadCareer.PetitionPressure = Math.Clamp(leadCareer.PetitionPressure - 1, 0, 100);
                leadCareer.AdministrativeTaskLoad = Math.Clamp(leadCareer.AdministrativeTaskLoad - 2, 0, 100);
                leadCareer.JurisdictionLeverage = Math.Clamp(leadCareer.JurisdictionLeverage + 1, 0, 100);
                leadCareer.CurrentAdministrativeTask = "补落前案";
                leadCareer.LastPetitionOutcome =
                    $"县门自补落地：修复余重{weights.RepairedWeight}、暂压余重{weights.ContainedWeight}给了台阶，文移补入正道，积案{leadCareer.PetitionBacklog}。";
                leadCareer.LastExplanation = leadCareer.LastPetitionOutcome;
                leadCareer.LastRefusalResponseCommandCode = PlayerCommandNames.PressCountyYamenDocument;
                leadCareer.LastRefusalResponseCommandLabel = "县门自补落地";
                leadCareer.LastRefusalResponseSummary = leadCareer.LastPetitionOutcome;
                leadCareer.LastRefusalResponseOutcomeCode = weights.RepairedWeight >= weights.ContainedWeight
                    ? PublicLifeOrderResponseOutcomeCodes.Repaired
                    : PublicLifeOrderResponseOutcomeCodes.Contained;
                leadCareer.LastRefusalResponseTraceCode = PublicLifeOrderResponseTraceCodes.OfficeActorYamenQuietLanding;
                leadCareer.ResponseCarryoverMonths = 1;
                scope.RecordDiff(
                    $"{leadCareer.DisplayName}任上县门补落前案，积案{leadCareer.PetitionBacklog}，牵制{leadCareer.ClerkDependence}。",
                    careersBySettlement.Key.Value.ToString());
            }
        }
    }

    private static IFamilyCoreQueries? TryGetFamilyCoreQueries(ModuleExecutionScope<OfficeAndCareerState> scope)
    {
        try
        {
            return scope.GetRequiredQuery<IFamilyCoreQueries>();
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
            IEnumerable<ClanId> clanIds,
            GameDate currentDate)
        {
            HashSet<ClanId> localClanIds = clanIds.ToHashSet();
            if (localClanIds.Count == 0)
            {
                return default;
            }

            int repaired = 0;
            int contained = 0;
            int escalated = 0;
            int ignored = 0;

            foreach (SocialMemoryEntrySnapshot memory in socialQueries.GetMemories()
                         .Where(static memory => memory.State == MemoryLifecycleState.Active)
                         .Where(static memory => memory.CauseKey.StartsWith("order.public_life.response.", StringComparison.Ordinal))
                         .Where(memory => memory.SourceClanId.HasValue && localClanIds.Contains(memory.SourceClanId.Value))
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
