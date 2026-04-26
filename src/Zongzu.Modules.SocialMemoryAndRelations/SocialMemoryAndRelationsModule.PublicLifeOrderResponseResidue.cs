using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private const string PublicLifeResponseSourceOrder = "OrderAndBanditry";
    private const string PublicLifeResponseSourceOffice = "OfficeAndCareer";
    private const string PublicLifeResponseSourceFamily = "FamilyCore";

    private static void ApplyPublicLifeOrderResponseAftermathResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyList<SettlementDisorderSnapshot> settlementDisorder,
        IReadOnlyList<JurisdictionAuthoritySnapshot> jurisdictions)
    {
        if (clans.Count == 0)
        {
            return;
        }

        ILookup<int, ClanSnapshot> clansBySettlement = clans.ToLookup(static clan => clan.HomeSettlementId.Value);

        foreach (SettlementDisorderSnapshot disorder in settlementDisorder
                     .Where(HasPublicLifeOrderResponseCarryover)
                     .OrderBy(static disorder => disorder.SettlementId.Value))
        {
            ClanSnapshot? ownerClan = SelectPublicLifeOrderResidueOwner(clansBySettlement[disorder.SettlementId.Value]);
            if (ownerClan is null)
            {
                continue;
            }

            RecordPublicLifeOrderResponseResidue(
                scope,
                ownerClan,
                PublicLifeResponseSourceOrder,
                disorder.SettlementId,
                disorder.LastRefusalResponseCommandCode,
                disorder.LastRefusalResponseCommandLabel,
                disorder.LastRefusalResponseOutcomeCode,
                disorder.LastRefusalResponseTraceCode);
        }

        foreach (JurisdictionAuthoritySnapshot jurisdiction in jurisdictions
                     .Where(HasPublicLifeOrderResponseCarryover)
                     .OrderBy(static jurisdiction => jurisdiction.SettlementId.Value))
        {
            ClanSnapshot? ownerClan = SelectPublicLifeOrderResidueOwner(clansBySettlement[jurisdiction.SettlementId.Value]);
            if (ownerClan is null)
            {
                continue;
            }

            RecordPublicLifeOrderResponseResidue(
                scope,
                ownerClan,
                PublicLifeResponseSourceOffice,
                jurisdiction.SettlementId,
                jurisdiction.LastRefusalResponseCommandCode,
                jurisdiction.LastRefusalResponseCommandLabel,
                jurisdiction.LastRefusalResponseOutcomeCode,
                jurisdiction.LastRefusalResponseTraceCode);
        }

        foreach (ClanSnapshot clan in clans
                     .Where(HasPublicLifeOrderResponseCarryover)
                     .OrderBy(static clan => clan.Id.Value))
        {
            RecordPublicLifeOrderResponseResidue(
                scope,
                clan,
                PublicLifeResponseSourceFamily,
                clan.HomeSettlementId,
                clan.LastRefusalResponseCommandCode,
                clan.LastRefusalResponseCommandLabel,
                clan.LastRefusalResponseOutcomeCode,
                clan.LastRefusalResponseTraceCode);
        }
    }

    private static bool HasPublicLifeOrderResponseCarryover(SettlementDisorderSnapshot disorder)
    {
        return disorder.ResponseCarryoverMonths > 0
            && !string.IsNullOrWhiteSpace(disorder.LastRefusalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(disorder.LastRefusalResponseOutcomeCode)
            && !string.IsNullOrWhiteSpace(disorder.LastRefusalResponseTraceCode);
    }

    private static bool HasPublicLifeOrderResponseCarryover(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return jurisdiction.ResponseCarryoverMonths > 0
            && !string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseOutcomeCode)
            && !string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseTraceCode)
            && !IsCourtPolicyLocalResponseCarryover(jurisdiction);
    }

    private static bool HasPublicLifeOrderResponseCarryover(ClanSnapshot clan)
    {
        return clan.ResponseCarryoverMonths > 0
            && !string.IsNullOrWhiteSpace(clan.LastRefusalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(clan.LastRefusalResponseOutcomeCode)
            && !string.IsNullOrWhiteSpace(clan.LastRefusalResponseTraceCode);
    }

    private static void RecordPublicLifeOrderResponseResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        ClanSnapshot ownerClan,
        string sourceModuleKey,
        SettlementId settlementId,
        string commandCode,
        string commandLabel,
        string outcomeCode,
        string traceCode)
    {
        if (!TryBuildPublicLifeOrderResponseProfile(outcomeCode, out PublicLifeOrderResponseProfile profile))
        {
            return;
        }

        string causeKey = BuildPublicLifeOrderResponseCauseKey(sourceModuleKey, commandCode, outcomeCode, traceCode);
        if (scope.State.Memories.Any(memory =>
                memory.SubjectClanId == ownerClan.Id
                && string.Equals(memory.CauseKey, causeKey, StringComparison.Ordinal)))
        {
            return;
        }

        ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, ownerClan.Id);
        ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, ownerClan.Id);
        int previousPressureBand = climate.LastPressureBand;
        int intensity = ComputePublicLifeOrderResponseIntensity(profile, sourceModuleKey);

        narrative.FearPressure = ClampPressure(narrative.FearPressure + profile.Fear);
        narrative.ShamePressure = ClampPressure(narrative.ShamePressure + profile.Shame);
        narrative.GrudgePressure = ClampPressure(narrative.GrudgePressure + profile.Grudge);
        narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + profile.Favor, -100, 100);
        narrative.PublicNarrative = BuildPublicLifeOrderResponseNarrative(ownerClan, commandCode, commandLabel, outcomeCode, sourceModuleKey);

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
        climate.LastTrace = $"{causeKey}: {RenderPublicLifeOrderResponseOutcome(outcomeCode)} at settlement {settlementId.Value}.";

        AddMemory(
            scope.State,
            ownerClan.Id,
            profile.Kind,
            profile.Type,
            profile.Subtype,
            causeKey,
            profile.MonthlyDecay,
            intensity,
            isPublic: true,
            BuildPublicLifeOrderResponseSummary(ownerClan, commandCode, commandLabel, outcomeCode, sourceModuleKey, intensity),
            scope.Context);

        scope.RecordDiff(
            $"{ownerClan.ClanName}承接{RenderPublicLifeResponseCommandLabel(commandCode, commandLabel)}后的{RenderPublicLifeOrderResponseOutcome(outcomeCode)}，羞面/人情/恐惧/怨尾已重排，强度{intensity}。",
            ownerClan.Id.Value.ToString());

        if (pressureBand > previousPressureBand)
        {
            scope.Emit(
                SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                $"{ownerClan.ClanName}因{RenderPublicLifeResponseCommandLabel(commandCode, commandLabel)}后账回应，门内情压升至{pressureBand}阶。",
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

    private static bool TryBuildPublicLifeOrderResponseProfile(
        string outcomeCode,
        out PublicLifeOrderResponseProfile profile)
    {
        profile = outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => new(
                SocialMemoryKinds.PublicOrderResponseRepaired,
                MemoryType.Favor,
                MemorySubtype.ProtectionFavor,
                EmotionalPressureAxis.Trust,
                BaseIntensity: 22,
                MonthlyDecay: 3,
                Fear: -4,
                Shame: -6,
                Anger: -3,
                Obligation: -2,
                Trust: 6,
                Favor: 5,
                Grudge: -5),

            PublicLifeOrderResponseOutcomeCodes.Contained => new(
                SocialMemoryKinds.PublicOrderResponseContained,
                MemoryType.Debt,
                MemorySubtype.ProtectionFavor,
                EmotionalPressureAxis.Obligation,
                BaseIntensity: 20,
                MonthlyDecay: 3,
                Fear: -2,
                Shame: -2,
                Anger: -1,
                Obligation: 3,
                Trust: 2,
                Favor: 1,
                Grudge: -2),

            PublicLifeOrderResponseOutcomeCodes.Escalated => new(
                SocialMemoryKinds.PublicOrderResponseEscalated,
                MemoryType.Grudge,
                MemorySubtype.PowerGrudge,
                EmotionalPressureAxis.Anger,
                BaseIntensity: 34,
                MonthlyDecay: 2,
                Fear: 8,
                Shame: 5,
                Anger: 7,
                Obligation: 1,
                Trust: -5,
                Favor: -4,
                Grudge: 8),

            PublicLifeOrderResponseOutcomeCodes.Ignored => new(
                SocialMemoryKinds.PublicOrderResponseIgnored,
                MemoryType.Shame,
                MemorySubtype.PublicShame,
                EmotionalPressureAxis.Shame,
                BaseIntensity: 26,
                MonthlyDecay: 2,
                Fear: 3,
                Shame: 6,
                Anger: 4,
                Obligation: 2,
                Trust: -4,
                Favor: -3,
                Grudge: 5),

            _ => default,
        };

        return !string.IsNullOrWhiteSpace(profile.Kind);
    }

    private static int ComputePublicLifeOrderResponseIntensity(
        PublicLifeOrderResponseProfile profile,
        string sourceModuleKey)
    {
        int sourceBonus = sourceModuleKey switch
        {
            PublicLifeResponseSourceOffice => 4,
            PublicLifeResponseSourceFamily => 3,
            _ => 2,
        };

        return Math.Clamp(profile.BaseIntensity + sourceBonus, 10, 100);
    }

    private static string BuildPublicLifeOrderResponseCauseKey(
        string sourceModuleKey,
        string commandCode,
        string outcomeCode,
        string traceCode)
    {
        return $"order.public_life.response.{sourceModuleKey}.{commandCode}.{outcomeCode}.{traceCode}";
    }

    private static string BuildPublicLifeOrderResponseNarrative(
        ClanSnapshot ownerClan,
        string commandCode,
        string commandLabel,
        string outcomeCode,
        string sourceModuleKey)
    {
        return $"{ownerClan.ClanName}已对{RenderPublicLifeResponseCommandLabel(commandCode, commandLabel)}作出回应，{RenderPublicLifeOrderResponseOutcome(outcomeCode)}；{RenderPublicLifeResponseSource(sourceModuleKey)}留下的羞面、人情、恐惧与怨尾开始改口。";
    }

    private static string BuildPublicLifeOrderResponseSummary(
        ClanSnapshot ownerClan,
        string commandCode,
        string commandLabel,
        string outcomeCode,
        string sourceModuleKey,
        int intensity)
    {
        return $"{ownerClan.ClanName}因{RenderPublicLifeResponseCommandLabel(commandCode, commandLabel)}承接前案后账，{RenderPublicLifeOrderResponseOutcome(outcomeCode)}；{RenderPublicLifeResponseSource(sourceModuleKey)}结构化余波转入乡议记忆，残留强度{intensity}。";
    }

    private static string RenderPublicLifeResponseCommandLabel(string commandCode, string commandLabel)
    {
        return string.IsNullOrWhiteSpace(commandLabel) ? commandCode : commandLabel;
    }

    private static string RenderPublicLifeOrderResponseOutcome(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "后账已修复",
            PublicLifeOrderResponseOutcomeCodes.Contained => "后账暂压",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "后账恶化",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "后账放置",
            _ => outcomeCode,
        };
    }

    private static string RenderPublicLifeResponseSource(string sourceModuleKey)
    {
        return sourceModuleKey switch
        {
            PublicLifeResponseSourceOrder => "治安与路面",
            PublicLifeResponseSourceOffice => "县门文移",
            PublicLifeResponseSourceFamily => "宗房解释",
            _ => sourceModuleKey,
        };
    }

    private readonly record struct PublicLifeOrderResponseProfile(
        string Kind,
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
