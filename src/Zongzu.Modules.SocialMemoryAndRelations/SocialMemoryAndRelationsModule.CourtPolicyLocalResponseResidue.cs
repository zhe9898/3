using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private const string OfficePolicyLocalResponsePrefix = "office.policy_local_response";

    private static void ApplyCourtPolicyLocalResponseResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyList<JurisdictionAuthoritySnapshot> jurisdictions)
    {
        if (clans.Count == 0 || jurisdictions.Count == 0)
        {
            return;
        }

        ILookup<int, ClanSnapshot> clansBySettlement = clans.ToLookup(static clan => clan.HomeSettlementId.Value);
        foreach (JurisdictionAuthoritySnapshot jurisdiction in jurisdictions
                     .Where(IsCourtPolicyLocalResponseCarryover)
                     .OrderBy(static jurisdiction => jurisdiction.SettlementId.Value))
        {
            ClanSnapshot? ownerClan = clansBySettlement[jurisdiction.SettlementId.Value]
                .OrderByDescending(static clan => clan.Prestige)
                .ThenBy(static clan => clan.Id.Value)
                .FirstOrDefault();
            if (ownerClan is null)
            {
                continue;
            }

            RecordCourtPolicyLocalResponseResidue(scope, ownerClan, jurisdiction);
        }
    }

    private static bool IsCourtPolicyLocalResponseCarryover(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return jurisdiction.ResponseCarryoverMonths > 0
            && IsCourtPolicyLocalResponseCommand(jurisdiction.LastRefusalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseOutcomeCode)
            && !string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseTraceCode)
            && IsCourtPolicyLocalResponseTrace(jurisdiction.LastRefusalResponseTraceCode)
            && HasCourtPolicyLocalResponsePressure(jurisdiction);
    }

    private static bool IsCourtPolicyLocalResponseCommand(string commandCode)
    {
        return string.Equals(commandCode, PlayerCommandNames.PressCountyYamenDocument, StringComparison.Ordinal)
            || string.Equals(commandCode, PlayerCommandNames.RedirectRoadReport, StringComparison.Ordinal);
    }

    private static bool IsCourtPolicyLocalResponseTrace(string traceCode)
    {
        return string.Equals(traceCode, PublicLifeOrderResponseTraceCodes.OfficeYamenLanded, StringComparison.Ordinal)
            || string.Equals(traceCode, PublicLifeOrderResponseTraceCodes.OfficeReportRerouted, StringComparison.Ordinal);
    }

    private static bool HasCourtPolicyLocalResponsePressure(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return jurisdiction.PetitionPressure >= 35
            || jurisdiction.AdministrativeTaskLoad >= 45
            || jurisdiction.PetitionBacklog >= 6
            || jurisdiction.ClerkDependence >= 45
            || IsOfficeImplementationOutcomeCategory(jurisdiction.PetitionOutcomeCategory);
    }

    private static void RecordCourtPolicyLocalResponseResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        ClanSnapshot ownerClan,
        JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (!TryBuildCourtPolicyLocalResponseProfile(
                jurisdiction.LastRefusalResponseOutcomeCode,
                out CourtPolicyLocalResponseProfile profile))
        {
            return;
        }

        string causeKey = BuildCourtPolicyLocalResponseCauseKey(jurisdiction);
        if (scope.State.Memories.Any(memory =>
                memory.SubjectClanId == ownerClan.Id
                && string.Equals(memory.CauseKey, causeKey, StringComparison.Ordinal)))
        {
            return;
        }

        ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, ownerClan.Id);
        ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, ownerClan.Id);
        int previousPressureBand = climate.LastPressureBand;
        int intensity = ComputeCourtPolicyLocalResponseIntensity(jurisdiction, profile);

        narrative.FearPressure = ClampPressure(narrative.FearPressure + profile.Fear);
        narrative.ShamePressure = ClampPressure(narrative.ShamePressure + profile.Shame);
        narrative.GrudgePressure = ClampPressure(narrative.GrudgePressure + profile.Grudge);
        narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + profile.Favor, -100, 100);
        narrative.PublicNarrative = $"{ownerClan.ClanName}仍在读政策回应余味：{profile.TraceLabel}；不是本户硬扛朝廷后账。";

        climate.Fear = ClampPressure(climate.Fear + profile.Fear);
        climate.Shame = ClampPressure(climate.Shame + profile.Shame);
        climate.Anger = ClampPressure(climate.Anger + profile.Anger);
        climate.Obligation = ClampPressure(climate.Obligation + profile.Obligation);
        climate.Trust = ClampPressure(climate.Trust + profile.Trust);
        climate.Bitterness = ClampPressure(climate.Bitterness + Math.Max(0, profile.Grudge + profile.Anger) / 2);
        climate.Volatility = ClampPressure(climate.Volatility + Math.Max(0, profile.Fear + profile.Anger) / 2);

        int pressureScore = ClampPressure(climate.Fear + climate.Shame + climate.Anger + climate.Obligation - climate.Trust);
        int pressureBand = ResolveBand(pressureScore);
        climate.LastPressureScore = Math.Max(climate.LastPressureScore, pressureScore);
        climate.LastPressureBand = Math.Max(climate.LastPressureBand, pressureBand);
        climate.LastUpdated = scope.Context.CurrentDate;
        climate.LastTrace = $"{causeKey}: {profile.TraceLabel}, settlement {jurisdiction.SettlementId.Value}.";

        AddMemory(
            scope.State,
            ownerClan.Id,
            SocialMemoryKinds.OfficePolicyLocalResponseResidue,
            profile.Type,
            profile.Subtype,
            causeKey,
            profile.MonthlyDecay,
            intensity,
            isPublic: true,
            BuildCourtPolicyLocalResponseSummary(ownerClan, jurisdiction, profile, intensity),
            scope.Context);

        scope.RecordDiff(
            $"{ownerClan.ClanName}承接政策回应后的{profile.TraceLabel}，社会余味强度{intensity}。",
            ownerClan.Id.Value.ToString());

        if (pressureBand > previousPressureBand)
        {
            scope.Emit(
                SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                $"{ownerClan.ClanName}因政策回应余味，门内情压升至{pressureBand}阶。",
                ownerClan.Id.Value.ToString(),
                new Dictionary<string, string>
                {
                    [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseSocialPressure,
                    [DomainEventMetadataKeys.ClanId] = ownerClan.Id.Value.ToString(),
                    [DomainEventMetadataKeys.EmotionalAxis] = profile.Axis.ToString(),
                    [DomainEventMetadataKeys.SocialPressureScore] = pressureScore.ToString(),
                    [DomainEventMetadataKeys.PressureBand] = pressureBand.ToString(),
                    [DomainEventMetadataKeys.SourceEventType] = causeKey,
                });
        }
    }

    private static bool TryBuildCourtPolicyLocalResponseProfile(
        string outcomeCode,
        out CourtPolicyLocalResponseProfile profile)
    {
        profile = outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => new(
                "政策回应转稳余味",
                MemoryType.Trust,
                MemorySubtype.ProtectionFavor,
                EmotionalPressureAxis.Trust,
                BaseIntensity: 22,
                MonthlyDecay: 3,
                Fear: -3,
                Shame: -2,
                Anger: -2,
                Obligation: 1,
                Trust: 5,
                Favor: 3,
                Grudge: -3),

            PublicLifeOrderResponseOutcomeCodes.Contained => new(
                "政策文移暂压余味",
                MemoryType.Debt,
                MemorySubtype.ProtectionFavor,
                EmotionalPressureAxis.Obligation,
                BaseIntensity: 24,
                MonthlyDecay: 3,
                Fear: -1,
                Shame: 1,
                Anger: 0,
                Obligation: 4,
                Trust: 2,
                Favor: 1,
                Grudge: 0),

            PublicLifeOrderResponseOutcomeCodes.Escalated => new(
                "政策回应转硬余味",
                MemoryType.Grudge,
                MemorySubtype.PowerGrudge,
                EmotionalPressureAxis.Anger,
                BaseIntensity: 36,
                MonthlyDecay: 2,
                Fear: 7,
                Shame: 4,
                Anger: 7,
                Obligation: 1,
                Trust: -5,
                Favor: -4,
                Grudge: 7),

            PublicLifeOrderResponseOutcomeCodes.Ignored => new(
                "政策回应放置余味",
                MemoryType.Shame,
                MemorySubtype.PublicShame,
                EmotionalPressureAxis.Shame,
                BaseIntensity: 28,
                MonthlyDecay: 2,
                Fear: 2,
                Shame: 6,
                Anger: 3,
                Obligation: 1,
                Trust: -4,
                Favor: -3,
                Grudge: 4),

            _ => default,
        };

        return !string.IsNullOrWhiteSpace(profile.TraceLabel);
    }

    private static int ComputeCourtPolicyLocalResponseIntensity(
        JurisdictionAuthoritySnapshot jurisdiction,
        CourtPolicyLocalResponseProfile profile)
    {
        int pressure =
            profile.BaseIntensity
            + (jurisdiction.PetitionPressure / 10)
            + (jurisdiction.PetitionBacklog / 6)
            + (jurisdiction.AdministrativeTaskLoad / 10)
            + (jurisdiction.ClerkDependence / 12)
            - (jurisdiction.JurisdictionLeverage / 18);
        return ClampPressure(pressure);
    }

    private static string BuildCourtPolicyLocalResponseCauseKey(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return string.Join(
            ".",
            OfficePolicyLocalResponsePrefix,
            jurisdiction.SettlementId.Value.ToString(),
            jurisdiction.LastRefusalResponseCommandCode,
            jurisdiction.LastRefusalResponseOutcomeCode,
            jurisdiction.LastRefusalResponseTraceCode);
    }

    private static string BuildCourtPolicyLocalResponseSummary(
        ClanSnapshot ownerClan,
        JurisdictionAuthoritySnapshot jurisdiction,
        CourtPolicyLocalResponseProfile profile,
        int intensity)
    {
        string commandLabel = RenderCourtPolicyLocalResponseCommand(jurisdiction.LastRefusalResponseCommandCode);
        return $"{ownerClan.ClanName}把政策回应读回沉为社会余味：{commandLabel}后留下{profile.TraceLabel}，案牍{jurisdiction.AdministrativeTaskLoad}，胥吏{jurisdiction.ClerkDependence}，积案{jurisdiction.PetitionBacklog}，余重{intensity}；仍由OfficeAndCareer/PublicLifeAndRumor后续读回，不是本户硬扛朝廷后账。";
    }

    private static string RenderCourtPolicyLocalResponseCommand(string commandCode)
    {
        return commandCode switch
        {
            PlayerCommandNames.PressCountyYamenDocument => "县门轻催",
            PlayerCommandNames.RedirectRoadReport => "递报改道",
            _ => commandCode,
        };
    }

    private readonly record struct CourtPolicyLocalResponseProfile(
        string TraceLabel,
        MemoryType Type,
        MemorySubtype Subtype,
        EmotionalPressureAxis Axis,
        int BaseIntensity,
        int MonthlyDecay,
        int Fear,
        int Shame,
        int Anger,
        int Obligation,
        int Trust,
        int Favor,
        int Grudge);
}
