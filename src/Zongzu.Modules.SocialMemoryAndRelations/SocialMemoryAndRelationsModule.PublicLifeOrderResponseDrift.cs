using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private const string PublicLifeResponseCausePrefix = "order.public_life.response.";

    private static void ApplyPublicLifeOrderResponseResidueDrift(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        IReadOnlyList<ClanSnapshot> clans)
    {
        if (scope.State.Memories.Count == 0)
        {
            return;
        }

        Dictionary<ClanId, ClanSnapshot> clansById = clans.ToDictionary(static clan => clan.Id, static clan => clan);
        foreach (MemoryRecordState memory in scope.State.Memories
                     .Where(memory => memory.LifecycleState == MemoryLifecycleState.Active)
                     .Where(static memory => memory.CauseKey.StartsWith(PublicLifeResponseCausePrefix, StringComparison.Ordinal))
                     .Where(memory => !WasCreatedThisMonth(memory, scope.Context.CurrentDate))
                     .OrderBy(static memory => memory.SubjectClanId.Value)
                     .ThenBy(static memory => memory.Id.Value))
        {
            if (!TryReadPublicLifeResponseCause(memory.CauseKey, out PublicLifeResponseCause cause)
                || !TryBuildPublicLifeOrderResponseDriftProfile(cause.OutcomeCode, out PublicLifeOrderResponseDriftProfile profile))
            {
                continue;
            }

            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, memory.SubjectClanId);
            ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, memory.SubjectClanId);
            int previousPressureBand = climate.LastPressureBand;
            string clanName = clansById.TryGetValue(memory.SubjectClanId, out ClanSnapshot? clan)
                ? clan.ClanName
                : $"clan {memory.SubjectClanId.Value}";

            memory.Weight = ClampPressure(memory.Weight + profile.WeightDelta);
            if (memory.Weight == 0)
            {
                memory.LifecycleState = MemoryLifecycleState.Dormant;
            }

            narrative.FearPressure = ClampPressure(narrative.FearPressure + profile.Fear);
            narrative.ShamePressure = ClampPressure(narrative.ShamePressure + profile.Shame);
            narrative.GrudgePressure = ClampPressure(narrative.GrudgePressure + profile.Grudge);
            narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + profile.Favor, -100, 100);
            narrative.PublicNarrative = BuildPublicLifeOrderResponseDriftNarrative(clanName, cause, profile, memory.Weight);

            climate.Fear = ClampPressure(climate.Fear + profile.Fear);
            climate.Shame = ClampPressure(climate.Shame + profile.Shame);
            climate.Anger = ClampPressure(climate.Anger + profile.Anger);
            climate.Obligation = ClampPressure(climate.Obligation + profile.Obligation);
            climate.Trust = ClampPressure(climate.Trust + profile.Trust);
            climate.Bitterness = ClampPressure(climate.Bitterness + profile.Bitterness);
            climate.Volatility = ClampPressure(climate.Volatility + profile.Volatility);

            int pressureScore = ClampPressure(climate.Fear + climate.Shame + climate.Anger + climate.Obligation - climate.Trust);
            int pressureBand = ResolveBand(pressureScore);
            climate.LastPressureScore = pressureScore;
            climate.LastPressureBand = pressureBand;
            climate.LastUpdated = scope.Context.CurrentDate;
            climate.LastTrace = $"{memory.CauseKey}: {profile.TraceLabel}, weight {memory.Weight}.";

            memory.Summary = BuildPublicLifeOrderResponseDriftSummary(clanName, cause, profile, memory.Weight);

            scope.RecordDiff(
                $"{clanName}前案回应后账{profile.TraceLabel}，乡议余重{memory.Weight}。",
                memory.SubjectClanId.Value.ToString());

            if (profile.EmitsPressureShift && pressureBand > previousPressureBand)
            {
                scope.Emit(
                    SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                    $"{clanName}前案回应后账转硬，门内情压升至{pressureBand}阶。",
                    memory.SubjectClanId.Value.ToString(),
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseSocialPressure,
                        [DomainEventMetadataKeys.ClanId] = memory.SubjectClanId.Value.ToString(),
                        [DomainEventMetadataKeys.EmotionalAxis] = profile.Axis.ToString(),
                        [DomainEventMetadataKeys.SocialPressureScore] = pressureScore.ToString(),
                        [DomainEventMetadataKeys.PressureBand] = pressureBand.ToString(),
                        [DomainEventMetadataKeys.SourceEventType] = memory.CauseKey,
                    });
            }
        }
    }

    private static bool WasCreatedThisMonth(MemoryRecordState memory, GameDate currentDate)
    {
        return memory.CreatedAt.Year == currentDate.Year && memory.CreatedAt.Month == currentDate.Month;
    }

    private static bool TryReadPublicLifeResponseCause(string causeKey, out PublicLifeResponseCause cause)
    {
        cause = default;
        if (!causeKey.StartsWith(PublicLifeResponseCausePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        string[] parts = causeKey.Split('.');
        if (parts.Length < 6)
        {
            return false;
        }

        cause = new PublicLifeResponseCause(
            SourceModuleKey: parts[3],
            CommandCode: parts[4],
            OutcomeCode: parts[5]);
        return true;
    }

    private static bool TryBuildPublicLifeOrderResponseDriftProfile(
        string outcomeCode,
        out PublicLifeOrderResponseDriftProfile profile)
    {
        profile = outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => new(
                "渐平",
                "后账渐平",
                EmotionalPressureAxis.Trust,
                WeightDelta: -2,
                Fear: -2,
                Shame: -3,
                Anger: -1,
                Obligation: -1,
                Trust: 2,
                Favor: 2,
                Grudge: -2,
                Bitterness: -2,
                Volatility: -1,
                EmitsPressureShift: false),

            PublicLifeOrderResponseOutcomeCodes.Contained => new(
                "暂压留账",
                "后账暂压留账",
                EmotionalPressureAxis.Obligation,
                WeightDelta: 1,
                Fear: -1,
                Shame: -1,
                Anger: 0,
                Obligation: 2,
                Trust: 0,
                Favor: 1,
                Grudge: 0,
                Bitterness: 0,
                Volatility: -1,
                EmitsPressureShift: false),

            PublicLifeOrderResponseOutcomeCodes.Escalated => new(
                "转硬",
                "后账转硬",
                EmotionalPressureAxis.Anger,
                WeightDelta: 5,
                Fear: 3,
                Shame: 2,
                Anger: 3,
                Obligation: 0,
                Trust: -2,
                Favor: -2,
                Grudge: 3,
                Bitterness: 3,
                Volatility: 2,
                EmitsPressureShift: true),

            PublicLifeOrderResponseOutcomeCodes.Ignored => new(
                "放置发酸",
                "后账放置发酸",
                EmotionalPressureAxis.Shame,
                WeightDelta: 4,
                Fear: 1,
                Shame: 3,
                Anger: 2,
                Obligation: 1,
                Trust: -2,
                Favor: -1,
                Grudge: 2,
                Bitterness: 2,
                Volatility: 1,
                EmitsPressureShift: true),

            _ => default,
        };

        return !string.IsNullOrWhiteSpace(profile.TraceLabel);
    }

    private static string BuildPublicLifeOrderResponseDriftNarrative(
        string clanName,
        PublicLifeResponseCause cause,
        PublicLifeOrderResponseDriftProfile profile,
        int weight)
    {
        return $"{clanName}前案{RenderPublicLifeResponseCommandName(cause.CommandCode)}{RenderPublicLifeOrderResponseOutcome(cause.OutcomeCode)}之后，{profile.MemoryLabel}，余重{weight}。";
    }

    private static string BuildPublicLifeOrderResponseDriftSummary(
        string clanName,
        PublicLifeResponseCause cause,
        PublicLifeOrderResponseDriftProfile profile,
        int weight)
    {
        return $"{clanName}的{RenderPublicLifeResponseCommandName(cause.CommandCode)}回应后账{profile.MemoryLabel}：羞面、人情、恐惧与怨尾重新排布，余重{weight}。";
    }

    private static string RenderPublicLifeResponseCommandName(string commandCode)
    {
        return commandCode switch
        {
            PlayerCommandNames.RepairLocalWatchGuarantee => "补保巡丁",
            PlayerCommandNames.CompensateRunnerMisread => "赔脚户误读",
            PlayerCommandNames.DeferHardPressure => "暂缓强压",
            PlayerCommandNames.PressCountyYamenDocument => "押文催县门",
            PlayerCommandNames.RedirectRoadReport => "改走递报",
            PlayerCommandNames.AskClanEldersExplain => "请族老解释",
            _ => commandCode,
        };
    }

    private readonly record struct PublicLifeResponseCause(
        string SourceModuleKey,
        string CommandCode,
        string OutcomeCode);

    private readonly record struct PublicLifeOrderResponseDriftProfile(
        string TraceLabel,
        string MemoryLabel,
        EmotionalPressureAxis Axis,
        int WeightDelta,
        int Fear,
        int Shame,
        int Anger,
        int Obligation,
        int Trust,
        int Favor,
        int Grudge,
        int Bitterness,
        int Volatility,
        bool EmitsPressureShift);
}
