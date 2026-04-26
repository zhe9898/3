using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private static void ApplyOfficePolicyImplementationResidue(
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
                     .Where(HasOfficePolicyImplementationResidue)
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

            RecordOfficePolicyImplementationResidue(scope, ownerClan, jurisdiction);
        }
    }

    private static bool HasOfficePolicyImplementationResidue(JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (!IsOfficeImplementationOutcomeCategory(jurisdiction.PetitionOutcomeCategory))
        {
            return false;
        }

        int pressure =
            jurisdiction.AdministrativeTaskLoad
            + jurisdiction.ClerkDependence
            + jurisdiction.PetitionBacklog
            + (jurisdiction.PetitionPressure / 2);
        return pressure >= 55 || jurisdiction.AuthorityTier >= 3;
    }

    private static bool IsOfficeImplementationOutcomeCategory(string category)
    {
        return string.Equals(category, "Stalled", StringComparison.Ordinal)
            || string.Equals(category, "Delayed", StringComparison.Ordinal)
            || string.Equals(category, "Triaged", StringComparison.Ordinal)
            || string.Equals(category, "Granted", StringComparison.Ordinal);
    }

    private static void RecordOfficePolicyImplementationResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        ClanSnapshot ownerClan,
        JurisdictionAuthoritySnapshot jurisdiction)
    {
        OfficePolicyResidueProfile profile = BuildOfficePolicyResidueProfile(jurisdiction);
        string causeKey = BuildOfficePolicyResidueCauseKey(jurisdiction);
        if (scope.State.Memories.Any(memory =>
                memory.SubjectClanId == ownerClan.Id
                && string.Equals(memory.CauseKey, causeKey, StringComparison.Ordinal)))
        {
            return;
        }

        ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, ownerClan.Id);
        ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, ownerClan.Id);
        int previousPressureBand = climate.LastPressureBand;
        int intensity = ComputeOfficePolicyResidueIntensity(jurisdiction, profile);

        narrative.FearPressure = ClampPressure(narrative.FearPressure + profile.Fear);
        narrative.ShamePressure = ClampPressure(narrative.ShamePressure + profile.Shame);
        narrative.GrudgePressure = ClampPressure(narrative.GrudgePressure + profile.Grudge);
        narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + profile.Favor, -100, 100);
        narrative.PublicNarrative = $"{ownerClan.ClanName}仍在读县门执行余味：{profile.TraceLabel}，不是本户再修。";

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
            SocialMemoryKinds.OfficePolicyImplementationResidue,
            profile.Type,
            profile.Subtype,
            causeKey,
            profile.MonthlyDecay,
            intensity,
            isPublic: true,
            BuildOfficePolicyResidueSummary(ownerClan, jurisdiction, profile, intensity),
            scope.Context);

        scope.RecordDiff(
            $"{ownerClan.ClanName}承接县门执行读回后的{profile.TraceLabel}，社会余味强度{intensity}。",
            ownerClan.Id.Value.ToString());

        if (pressureBand > previousPressureBand)
        {
            scope.Emit(
                SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                $"{ownerClan.ClanName}因县门执行余味，门内情压升至{pressureBand}阶。",
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

    private static OfficePolicyResidueProfile BuildOfficePolicyResidueProfile(
        JurisdictionAuthoritySnapshot jurisdiction)
    {
        return jurisdiction.PetitionOutcomeCategory switch
        {
            "Stalled" => new OfficePolicyResidueProfile(
                "胥吏把持余味",
                MemoryType.Fear,
                MemorySubtype.PowerGrudge,
                EmotionalPressureAxis.Fear,
                2,
                Fear: 6,
                Shame: 4,
                Anger: 5,
                Obligation: 2,
                Trust: -4,
                Favor: -3,
                Grudge: 6),

            "Delayed" => new OfficePolicyResidueProfile(
                "文移拖延余味",
                MemoryType.Grudge,
                MemorySubtype.PowerGrudge,
                EmotionalPressureAxis.Anger,
                2,
                Fear: 3,
                Shame: 3,
                Anger: 4,
                Obligation: 2,
                Trust: -3,
                Favor: -2,
                Grudge: 5),

            "Triaged" => new OfficePolicyResidueProfile(
                "纸面落地余味",
                MemoryType.Debt,
                MemorySubtype.PublicShame,
                EmotionalPressureAxis.Obligation,
                3,
                Fear: 1,
                Shame: 2,
                Anger: 1,
                Obligation: 3,
                Trust: 1,
                Favor: 0,
                Grudge: 1),

            _ => new OfficePolicyResidueProfile(
                "急牍暂缓余味",
                MemoryType.Trust,
                MemorySubtype.ProtectionFavor,
                EmotionalPressureAxis.Trust,
                3,
                Fear: -2,
                Shame: -1,
                Anger: -1,
                Obligation: 0,
                Trust: 3,
                Favor: 2,
                Grudge: -2),
        };
    }

    private static int ComputeOfficePolicyResidueIntensity(
        JurisdictionAuthoritySnapshot jurisdiction,
        OfficePolicyResidueProfile profile)
    {
        int pressure =
            profile.BaseIntensity
            + (jurisdiction.AdministrativeTaskLoad / 4)
            + (jurisdiction.ClerkDependence / 4)
            + (jurisdiction.PetitionBacklog / 5)
            + (jurisdiction.PetitionPressure / 8)
            - (jurisdiction.JurisdictionLeverage / 12);
        return ClampPressure(pressure);
    }

    private static string BuildOfficePolicyResidueCauseKey(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return $"office.policy_implementation.{jurisdiction.SettlementId.Value}.{jurisdiction.PetitionOutcomeCategory}";
    }

    private static string BuildOfficePolicyResidueSummary(
        ClanSnapshot ownerClan,
        JurisdictionAuthoritySnapshot jurisdiction,
        OfficePolicyResidueProfile profile,
        int intensity)
    {
        return $"{ownerClan.ClanName}把县门执行读回沉为社会余味：{profile.TraceLabel}，案牍{jurisdiction.AdministrativeTaskLoad}，胥吏{jurisdiction.ClerkDependence}，积案{jurisdiction.PetitionBacklog}，余重{intensity}；仍由OfficeAndCareer后续读回。";
    }

    private readonly record struct OfficePolicyResidueProfile(
        string TraceLabel,
        MemoryType Type,
        MemorySubtype Subtype,
        EmotionalPressureAxis Axis,
        int MonthlyDecay,
        int Fear,
        int Shame,
        int Anger,
        int Obligation,
        int Trust,
        int Favor,
        int Grudge)
    {
        public int BaseIntensity => Math.Clamp(18 + Fear + Shame + Anger + Obligation + Math.Max(0, Grudge), 10, 100);
    }
}
