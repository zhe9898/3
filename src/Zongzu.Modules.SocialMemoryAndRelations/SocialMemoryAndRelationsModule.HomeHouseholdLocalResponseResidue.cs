using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private const string HomeHouseholdLocalResponseCausePrefix = "order.public_life.household_response.";

    private static void ApplyHomeHouseholdLocalResponseResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyList<HouseholdPressureSnapshot> households)
    {
        if (clans.Count == 0 || households.Count == 0)
        {
            return;
        }

        Dictionary<ClanId, ClanSnapshot> clansById = clans.ToDictionary(static clan => clan.Id, static clan => clan);
        foreach (HouseholdPressureSnapshot household in households
                     .Where(HasHomeHouseholdLocalResponseAftermath)
                     .Where(static household => household.SponsorClanId.HasValue)
                     .OrderBy(static household => household.SettlementId.Value)
                     .ThenBy(static household => household.SponsorClanId.HasValue ? household.SponsorClanId.Value.Value : int.MaxValue)
                     .ThenBy(static household => household.Id.Value))
        {
            ClanId ownerClanId = household.SponsorClanId!.Value;
            if (!clansById.TryGetValue(ownerClanId, out ClanSnapshot? ownerClan))
            {
                continue;
            }

            RecordHomeHouseholdLocalResponseResidue(scope, ownerClan, household);
        }
    }

    private static bool HasHomeHouseholdLocalResponseAftermath(HouseholdPressureSnapshot household)
    {
        return !string.IsNullOrWhiteSpace(household.LastLocalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(household.LastLocalResponseOutcomeCode)
            && !string.IsNullOrWhiteSpace(household.LastLocalResponseTraceCode);
    }

    private static void RecordHomeHouseholdLocalResponseResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        ClanSnapshot ownerClan,
        HouseholdPressureSnapshot household)
    {
        if (!TryBuildHomeHouseholdLocalResponseProfile(
                household.LastLocalResponseOutcomeCode,
                out HomeHouseholdLocalResponseProfile profile))
        {
            return;
        }

        string causeKey = BuildHomeHouseholdLocalResponseCauseKey(household);
        if (scope.State.Memories.Any(memory =>
                memory.SubjectClanId == ownerClan.Id
                && string.Equals(memory.CauseKey, causeKey, StringComparison.Ordinal)))
        {
            return;
        }

        ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, ownerClan.Id);
        ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, ownerClan.Id);
        int previousPressureBand = climate.LastPressureBand;
        int intensity = ComputeHomeHouseholdLocalResponseIntensity(household, profile);

        narrative.FearPressure = ClampPressure(narrative.FearPressure + profile.Fear);
        narrative.ShamePressure = ClampPressure(narrative.ShamePressure + profile.Shame);
        narrative.GrudgePressure = ClampPressure(narrative.GrudgePressure + profile.Grudge);
        narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + profile.Favor, -100, 100);
        narrative.PublicNarrative = BuildHomeHouseholdLocalResponseNarrative(ownerClan, household, profile, intensity);

        climate.Fear = ClampPressure(climate.Fear + profile.Fear);
        climate.Shame = ClampPressure(climate.Shame + profile.Shame);
        climate.Anger = ClampPressure(climate.Anger + profile.Anger);
        climate.Obligation = ClampPressure(climate.Obligation + profile.Obligation);
        climate.Trust = ClampPressure(climate.Trust + profile.Trust);
        climate.Bitterness = ClampPressure(climate.Bitterness + profile.Bitterness);
        climate.Volatility = ClampPressure(climate.Volatility + profile.Volatility);

        int pressureScore = ClampPressure(climate.Fear + climate.Shame + climate.Anger + climate.Obligation - climate.Trust);
        int pressureBand = ResolveBand(pressureScore);
        climate.LastPressureScore = Math.Max(climate.LastPressureScore, pressureScore);
        climate.LastPressureBand = Math.Max(climate.LastPressureBand, pressureBand);
        climate.LastUpdated = scope.Context.CurrentDate;
        climate.LastTrace = $"{causeKey}: {profile.TraceLabel}, household {household.Id.Value}.";

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
            BuildHomeHouseholdLocalResponseSummary(ownerClan, household, profile, intensity),
            scope.Context);

        scope.RecordDiff(
            $"{ownerClan.ClanName}承接{RenderHomeHouseholdLocalResponseCommandLabel(household)}后的{profile.TraceLabel}，本户羞面/人情/恐惧/怨尾已读回，强度{intensity}。",
            ownerClan.Id.Value.ToString());

        if (profile.EmitsPressureShift && pressureBand > previousPressureBand)
        {
            scope.Emit(
                SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                $"{ownerClan.ClanName}因本户后账回应吃紧，门内情压升至{pressureBand}阶。",
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

    private static bool TryBuildHomeHouseholdLocalResponseProfile(
        string outcomeCode,
        out HomeHouseholdLocalResponseProfile profile)
    {
        profile = outcomeCode switch
        {
            HouseholdLocalResponseOutcomeCodes.Relieved => new(
                SocialMemoryKinds.PublicOrderHouseholdResponseRelieved,
                MemoryType.Favor,
                MemorySubtype.ProtectionFavor,
                EmotionalPressureAxis.Trust,
                "本户后账已缓",
                BaseIntensity: 18,
                MonthlyDecay: 3,
                Fear: -3,
                Shame: -3,
                Anger: -1,
                Obligation: -1,
                Trust: 4,
                Favor: 3,
                Grudge: -2,
                Bitterness: -2,
                Volatility: -1,
                EmitsPressureShift: false),

            HouseholdLocalResponseOutcomeCodes.Contained => new(
                SocialMemoryKinds.PublicOrderHouseholdResponseContained,
                MemoryType.Debt,
                MemorySubtype.ProtectionFavor,
                EmotionalPressureAxis.Obligation,
                "本户后账暂压",
                BaseIntensity: 20,
                MonthlyDecay: 3,
                Fear: -1,
                Shame: 0,
                Anger: 0,
                Obligation: 3,
                Trust: 1,
                Favor: 1,
                Grudge: 0,
                Bitterness: 1,
                Volatility: -1,
                EmitsPressureShift: false),

            HouseholdLocalResponseOutcomeCodes.Strained => new(
                SocialMemoryKinds.PublicOrderHouseholdResponseStrained,
                MemoryType.Debt,
                MemorySubtype.MarketDebt,
                EmotionalPressureAxis.Obligation,
                "本户后账吃紧",
                BaseIntensity: 28,
                MonthlyDecay: 2,
                Fear: 3,
                Shame: 4,
                Anger: 2,
                Obligation: 3,
                Trust: -2,
                Favor: -1,
                Grudge: 3,
                Bitterness: 3,
                Volatility: 2,
                EmitsPressureShift: true),

            HouseholdLocalResponseOutcomeCodes.Ignored => new(
                SocialMemoryKinds.PublicOrderHouseholdResponseIgnored,
                MemoryType.Shame,
                MemorySubtype.PublicShame,
                EmotionalPressureAxis.Shame,
                "本户后账放置",
                BaseIntensity: 24,
                MonthlyDecay: 2,
                Fear: 2,
                Shame: 5,
                Anger: 2,
                Obligation: 1,
                Trust: -3,
                Favor: -2,
                Grudge: 3,
                Bitterness: 2,
                Volatility: 1,
                EmitsPressureShift: true),

            _ => default,
        };

        return !string.IsNullOrWhiteSpace(profile.Kind);
    }

    private static int ComputeHomeHouseholdLocalResponseIntensity(
        HouseholdPressureSnapshot household,
        HomeHouseholdLocalResponseProfile profile)
    {
        int exposure = (household.Distress / 12)
            + (household.DebtPressure / 14)
            + (household.MigrationRisk / 16)
            + (Math.Max(0, 60 - household.LaborCapacity) / 12);
        return ClampPressure(profile.BaseIntensity + exposure);
    }

    private static string BuildHomeHouseholdLocalResponseCauseKey(HouseholdPressureSnapshot household)
    {
        return $"{HomeHouseholdLocalResponseCausePrefix}{household.Id.Value}.{household.LastLocalResponseCommandCode}.{household.LastLocalResponseOutcomeCode}.{household.LastLocalResponseTraceCode}";
    }

    private static string BuildHomeHouseholdLocalResponseNarrative(
        ClanSnapshot ownerClan,
        HouseholdPressureSnapshot household,
        HomeHouseholdLocalResponseProfile profile,
        int intensity)
    {
        return $"{ownerClan.ClanName}因{household.HouseholdName}{RenderHomeHouseholdLocalResponseCommandLabel(household)}留下{profile.TraceLabel}，余重{intensity}。";
    }

    private static string BuildHomeHouseholdLocalResponseSummary(
        ClanSnapshot ownerClan,
        HouseholdPressureSnapshot household,
        HomeHouseholdLocalResponseProfile profile,
        int intensity)
    {
        return $"{ownerClan.ClanName}因{household.HouseholdName}{RenderHomeHouseholdLocalResponseCommandLabel(household)}留下本户后账：{profile.TraceLabel}，民困{household.Distress}，债压{household.DebtPressure}，丁力{household.LaborCapacity}，余重{intensity}。";
    }

    private static string RenderHomeHouseholdLocalResponseCommandLabel(HouseholdPressureSnapshot household)
    {
        if (!string.IsNullOrWhiteSpace(household.LastLocalResponseCommandLabel))
        {
            return household.LastLocalResponseCommandLabel;
        }

        return household.LastLocalResponseCommandCode switch
        {
            PlayerCommandNames.RestrictNightTravel => "暂缩夜行",
            PlayerCommandNames.PoolRunnerCompensation => "凑钱赔脚户",
            PlayerCommandNames.SendHouseholdRoadMessage => "遣少丁递信",
            _ => household.LastLocalResponseCommandCode,
        };
    }

    private readonly record struct HomeHouseholdLocalResponseProfile(
        string Kind,
        MemoryType Type,
        MemorySubtype Subtype,
        EmotionalPressureAxis Axis,
        string TraceLabel,
        int BaseIntensity,
        int MonthlyDecay,
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
