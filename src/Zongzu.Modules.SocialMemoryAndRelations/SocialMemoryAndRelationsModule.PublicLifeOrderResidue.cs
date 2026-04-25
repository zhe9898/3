using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed partial class SocialMemoryAndRelationsModule
{
    private static void ApplyPublicLifeOrderAftermathResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyList<SettlementDisorderSnapshot> settlementDisorder)
    {
        if (clans.Count == 0 || settlementDisorder.Count == 0)
        {
            return;
        }

        ILookup<int, ClanSnapshot> clansBySettlement = clans.ToLookup(static clan => clan.HomeSettlementId.Value);

        foreach (SettlementDisorderSnapshot disorder in settlementDisorder
                     .Where(HasPublicLifeOrderAftermathCarryover)
                     .OrderBy(static disorder => disorder.SettlementId.Value))
        {
            if (!TryBuildPublicLifeOrderResidueProfile(disorder, out PublicLifeOrderResidueProfile profile))
            {
                continue;
            }

            ClanSnapshot? ownerClan = SelectPublicLifeOrderResidueOwner(clansBySettlement[disorder.SettlementId.Value]);
            if (ownerClan is null)
            {
                continue;
            }

            RecordPublicLifeOrderResidue(scope, ownerClan, disorder, profile);
        }
    }

    private static bool HasPublicLifeOrderAftermathCarryover(SettlementDisorderSnapshot disorder)
    {
        return disorder.InterventionCarryoverMonths > 0 || disorder.RefusalCarryoverMonths > 0;
    }

    private static ClanSnapshot? SelectPublicLifeOrderResidueOwner(IEnumerable<ClanSnapshot> localClans)
    {
        return localClans
            .OrderByDescending(static clan => clan.Prestige)
            .ThenByDescending(static clan => clan.SupportReserve)
            .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
            .ThenBy(static clan => clan.Id.Value)
            .FirstOrDefault();
    }

    private static void RecordPublicLifeOrderResidue(
        ModuleExecutionScope<SocialMemoryAndRelationsState> scope,
        ClanSnapshot ownerClan,
        SettlementDisorderSnapshot disorder,
        PublicLifeOrderResidueProfile profile)
    {
        ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, ownerClan.Id);
        ClanEmotionalClimateState climate = GetOrCreateClimate(scope.State, ownerClan.Id);
        int previousPressureBand = climate.LastPressureBand;
        int intensity = ComputePublicLifeOrderResidueIntensity(disorder, profile);

        narrative.FearPressure = ClampPressure(narrative.FearPressure + profile.Fear + (disorder.RetaliationRisk / 24));
        narrative.ShamePressure = ClampPressure(narrative.ShamePressure + profile.Shame + (disorder.ImplementationDrag / 25));
        narrative.GrudgePressure = ClampPressure(narrative.GrudgePressure + profile.Grudge + (disorder.CoercionRisk / 25));
        narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + profile.Favor + (disorder.RouteShielding / 30), -100, 100);
        narrative.PublicNarrative = BuildPublicLifeOrderResidueNarrative(ownerClan, disorder, profile);

        climate.Fear = ClampPressure(climate.Fear + profile.Fear + (disorder.RetaliationRisk / 18));
        climate.Shame = ClampPressure(climate.Shame + profile.Shame + (disorder.ImplementationDrag / 20));
        climate.Anger = ClampPressure(climate.Anger + profile.Anger + (disorder.CoercionRisk / 24));
        climate.Obligation = ClampPressure(climate.Obligation + profile.Obligation + (disorder.RouteShielding / 20));
        climate.Trust = ClampPressure(climate.Trust + profile.Trust);
        climate.Bitterness = ClampPressure(climate.Bitterness + Math.Max(0, profile.Grudge + profile.Anger) / 2);
        climate.Volatility = ClampPressure(climate.Volatility + Math.Max(0, profile.Fear + disorder.RetaliationRisk / 20));

        int pressureScore = ClampPressure(climate.Fear + climate.Shame + climate.Anger - climate.Trust);
        int pressureBand = ResolveBand(pressureScore);
        climate.LastPressureScore = Math.Max(climate.LastPressureScore, pressureScore);
        climate.LastPressureBand = Math.Max(climate.LastPressureBand, pressureBand);
        climate.LastUpdated = scope.Context.CurrentDate;
        climate.LastTrace = $"{profile.CauseKey}: {RenderOrderLabel(disorder)} {RenderOrderOutcomeLabel(disorder)} at settlement {disorder.SettlementId.Value}.";

        AddMemory(
            scope.State,
            ownerClan.Id,
            profile.Kind,
            profile.Type,
            profile.Subtype,
            profile.CauseKey,
            profile.MonthlyDecay,
            intensity,
            isPublic: true,
            BuildPublicLifeOrderResidueSummary(ownerClan, disorder, profile, intensity),
            scope.Context);

        scope.RecordDiff(
            $"{ownerClan.ClanName}承接{RenderOrderLabel(disorder)}{RenderOrderOutcomeLabel(disorder)}后的{profile.ResidueLabel}，强度{intensity}。",
            ownerClan.Id.Value.ToString());

        if (pressureBand > previousPressureBand)
        {
            scope.Emit(
                SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                $"{ownerClan.ClanName}因{RenderOrderLabel(disorder)}后账，门内情压升至{pressureBand}阶。",
                ownerClan.Id.Value.ToString(),
                new Dictionary<string, string>
                {
                    [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseSocialPressure,
                    [DomainEventMetadataKeys.ClanId] = ownerClan.Id.Value.ToString(),
                    [DomainEventMetadataKeys.EmotionalAxis] = profile.Axis.ToString(),
                    [DomainEventMetadataKeys.SocialPressureScore] = pressureScore.ToString(),
                    [DomainEventMetadataKeys.PressureBand] = pressureBand.ToString(),
                    [DomainEventMetadataKeys.SourceEventType] = profile.CauseKey,
                });
        }
    }

    private static int ComputePublicLifeOrderResidueIntensity(
        SettlementDisorderSnapshot disorder,
        PublicLifeOrderResidueProfile profile)
    {
        int pressure =
            profile.BaseIntensity
            + (disorder.RouteShielding / 4)
            + (disorder.RetaliationRisk / 3)
            + (disorder.SuppressionDemand / 5)
            + (disorder.BlackRoutePressure / 6)
            + (disorder.CoercionRisk / 6)
            + (disorder.RefusalCarryoverMonths * 12)
            + ResolveOutcomeIntensityBonus(disorder.LastInterventionOutcomeCode);

        return Math.Clamp(pressure, 10, 100);
    }

    private static int ResolveOutcomeIntensityBonus(string outcomeCode)
    {
        return outcomeCode switch
        {
            OrderInterventionOutcomeCodes.Refused => 15,
            OrderInterventionOutcomeCodes.Partial => 8,
            _ => 0,
        };
    }

    private static string BuildPublicLifeOrderResidueNarrative(
        ClanSnapshot ownerClan,
        SettlementDisorderSnapshot disorder,
        PublicLifeOrderResidueProfile profile)
    {
        string orderLabel = RenderOrderLabel(disorder);
        string outcomeLabel = RenderOrderOutcomeLabel(disorder);
        return $"{ownerClan.ClanName}上月按{orderLabel}出面，{outcomeLabel}；{profile.ResidueLabel}已经落进乡议，路压{disorder.RoutePressure}，报复险{disorder.RetaliationRisk}。";
    }

    private static string BuildPublicLifeOrderResidueSummary(
        ClanSnapshot ownerClan,
        SettlementDisorderSnapshot disorder,
        PublicLifeOrderResidueProfile profile,
        int intensity)
    {
        string orderLabel = RenderOrderLabel(disorder);
        string outcomeLabel = RenderOrderOutcomeLabel(disorder);
        return $"{ownerClan.ClanName}因上月{orderLabel}{outcomeLabel}留下{profile.ResidueLabel}；乡里记得谁担保、谁护路，也记得报复险{disorder.RetaliationRisk}与路压{disorder.RoutePressure}，残留强度{intensity}。";
    }

    private static string RenderOrderLabel(SettlementDisorderSnapshot disorder)
    {
        return string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
            ? disorder.LastInterventionCommandCode
            : disorder.LastInterventionCommandLabel;
    }

    private static string RenderOrderOutcomeLabel(SettlementDisorderSnapshot disorder)
    {
        return disorder.LastInterventionOutcomeCode switch
        {
            OrderInterventionOutcomeCodes.Refused => "被地方拒住",
            OrderInterventionOutcomeCodes.Partial => "只半落地",
            _ => "已经落地",
        };
    }

    private static bool TryBuildPublicLifeOrderResidueProfile(
        SettlementDisorderSnapshot disorder,
        out PublicLifeOrderResidueProfile profile)
    {
        string outcomeCode = string.IsNullOrWhiteSpace(disorder.LastInterventionOutcomeCode)
            ? OrderInterventionOutcomeCodes.Accepted
            : disorder.LastInterventionOutcomeCode;

        if (string.Equals(outcomeCode, OrderInterventionOutcomeCodes.Refused, StringComparison.Ordinal))
        {
            return TryBuildRefusedPublicLifeOrderResidueProfile(
                disorder.LastInterventionCommandCode,
                disorder.LastInterventionRefusalCode,
                out profile);
        }

        if (string.Equals(outcomeCode, OrderInterventionOutcomeCodes.Partial, StringComparison.Ordinal))
        {
            return TryBuildPartialPublicLifeOrderResidueProfile(
                disorder.LastInterventionCommandCode,
                disorder.LastInterventionPartialCode,
                out profile);
        }

        return TryBuildAcceptedPublicLifeOrderResidueProfile(disorder.LastInterventionCommandCode, out profile);
    }

    private static bool TryBuildRefusedPublicLifeOrderResidueProfile(
        string commandName,
        string refusalCode,
        out PublicLifeOrderResidueProfile profile)
    {
        profile = (commandName, refusalCode) switch
        {
            (PlayerCommandNames.FundLocalWatch, OrderInterventionRefusalCodes.WatchmenRefused) => new(
                SocialMemoryKinds.PublicOrderWatchRefusalShame,
                MemoryType.Shame,
                MemorySubtype.PublicShame,
                "order.public_life.fund_local_watch.refused",
                EmotionalPressureAxis.Shame,
                "巡丁不应后的公开担保失败",
                BaseIntensity: 32,
                MonthlyDecay: 2,
                Fear: 5,
                Shame: 9,
                Anger: 3,
                Obligation: 4,
                Trust: -3,
                Favor: -2,
                Grudge: 4),

            (PlayerCommandNames.SuppressBanditry, OrderInterventionRefusalCodes.SuppressionRefused) => new(
                SocialMemoryKinds.PublicOrderSuppressionRefusalFear,
                MemoryType.Fear,
                MemorySubtype.PowerGrudge,
                "order.public_life.suppress_banditry.refused",
                EmotionalPressureAxis.Fear,
                "严缉不成后的恐惧与怨尾",
                BaseIntensity: 36,
                MonthlyDecay: 2,
                Fear: 10,
                Shame: 5,
                Anger: 5,
                Obligation: 1,
                Trust: -4,
                Favor: -3,
                Grudge: 7),

            _ => default,
        };

        return !string.IsNullOrWhiteSpace(profile.Kind);
    }

    private static bool TryBuildPartialPublicLifeOrderResidueProfile(
        string commandName,
        string partialCode,
        out PublicLifeOrderResidueProfile profile)
    {
        profile = (commandName, partialCode) switch
        {
            (PlayerCommandNames.FundLocalWatch, OrderInterventionPartialCodes.CountyDrag) => new(
                SocialMemoryKinds.PublicOrderWatchPartialObligation,
                MemoryType.Debt,
                MemorySubtype.ProtectionFavor,
                "order.public_life.fund_local_watch.partial",
                EmotionalPressureAxis.Obligation,
                "巡丁半落地后的担保债",
                BaseIntensity: 28,
                MonthlyDecay: 3,
                Fear: 4,
                Shame: 4,
                Anger: 2,
                Obligation: 9,
                Trust: -1,
                Favor: 1,
                Grudge: 2),

            (PlayerCommandNames.FundLocalWatch, OrderInterventionPartialCodes.WatchMisread) => new(
                SocialMemoryKinds.PublicOrderWatchPartialObligation,
                MemoryType.Debt,
                MemorySubtype.ProtectionFavor,
                "order.public_life.fund_local_watch.partial",
                EmotionalPressureAxis.Obligation,
                "巡丁半落地后的误读担保债",
                BaseIntensity: 30,
                MonthlyDecay: 3,
                Fear: 5,
                Shame: 5,
                Anger: 3,
                Obligation: 8,
                Trust: -2,
                Favor: 0,
                Grudge: 3),

            (PlayerCommandNames.SuppressBanditry, OrderInterventionPartialCodes.CountyDrag) => new(
                SocialMemoryKinds.PublicOrderSuppressionPartialGrudge,
                MemoryType.Grudge,
                MemorySubtype.PowerGrudge,
                "order.public_life.suppress_banditry.partial",
                EmotionalPressureAxis.Anger,
                "严缉半落地后的拖延怨尾",
                BaseIntensity: 32,
                MonthlyDecay: 2,
                Fear: 6,
                Shame: 4,
                Anger: 6,
                Obligation: 1,
                Trust: -2,
                Favor: -2,
                Grudge: 7),

            (PlayerCommandNames.SuppressBanditry, OrderInterventionPartialCodes.SuppressionBacklash) => new(
                SocialMemoryKinds.PublicOrderSuppressionPartialGrudge,
                MemoryType.Grudge,
                MemorySubtype.PowerGrudge,
                "order.public_life.suppress_banditry.partial",
                EmotionalPressureAxis.Anger,
                "严缉半落地后的反噬怨尾",
                BaseIntensity: 34,
                MonthlyDecay: 2,
                Fear: 8,
                Shame: 4,
                Anger: 8,
                Obligation: 1,
                Trust: -3,
                Favor: -2,
                Grudge: 8),

            _ => default,
        };

        return !string.IsNullOrWhiteSpace(profile.Kind);
    }

    private static bool TryBuildAcceptedPublicLifeOrderResidueProfile(
        string commandName,
        out PublicLifeOrderResidueProfile profile)
    {
        profile = commandName switch
        {
            PlayerCommandNames.EscortRoadReport => new(
                SocialMemoryKinds.PublicOrderEscortObligation,
                MemoryType.Debt,
                MemorySubtype.ProtectionFavor,
                "order.public_life.escort_road_report",
                EmotionalPressureAxis.Obligation,
                "路报担保债",
                BaseIntensity: 18,
                MonthlyDecay: 3,
                Fear: 2,
                Shame: 1,
                Anger: 0,
                Obligation: 6,
                Trust: 2,
                Favor: 2,
                Grudge: 0),

            PlayerCommandNames.FundLocalWatch => new(
                SocialMemoryKinds.PublicOrderWatchObligation,
                MemoryType.Favor,
                MemorySubtype.ProtectionFavor,
                "order.public_life.fund_local_watch",
                EmotionalPressureAxis.Obligation,
                "巡丁担保与护路人情",
                BaseIntensity: 22,
                MonthlyDecay: 3,
                Fear: 2,
                Shame: 2,
                Anger: 1,
                Obligation: 8,
                Trust: 3,
                Favor: 4,
                Grudge: 0),

            PlayerCommandNames.SuppressBanditry => new(
                SocialMemoryKinds.PublicOrderSuppressionFear,
                MemoryType.Fear,
                MemorySubtype.PowerGrudge,
                "order.public_life.suppress_banditry",
                EmotionalPressureAxis.Fear,
                "严缉后的报复恐惧",
                BaseIntensity: 26,
                MonthlyDecay: 2,
                Fear: 8,
                Shame: 2,
                Anger: 4,
                Obligation: 1,
                Trust: 0,
                Favor: -1,
                Grudge: 5),

            PlayerCommandNames.NegotiateWithOutlaws => new(
                SocialMemoryKinds.PublicOrderPublicShame,
                MemoryType.Shame,
                MemorySubtype.PublicShame,
                "order.public_life.negotiate_with_outlaws",
                EmotionalPressureAxis.Shame,
                "议路后的公议羞面",
                BaseIntensity: 20,
                MonthlyDecay: 3,
                Fear: 1,
                Shame: 6,
                Anger: 2,
                Obligation: 5,
                Trust: -2,
                Favor: -1,
                Grudge: 2),

            PlayerCommandNames.TolerateDisorder => new(
                SocialMemoryKinds.PublicOrderPublicShame,
                MemoryType.Shame,
                MemorySubtype.PublicShame,
                "order.public_life.tolerate_disorder",
                EmotionalPressureAxis.Shame,
                "暂缓穷追后的乡面口实",
                BaseIntensity: 18,
                MonthlyDecay: 3,
                Fear: 3,
                Shame: 5,
                Anger: 2,
                Obligation: 0,
                Trust: -3,
                Favor: -2,
                Grudge: 3),

            _ => default,
        };

        return !string.IsNullOrWhiteSpace(profile.Kind);
    }

    private readonly record struct PublicLifeOrderResidueProfile(
        string Kind,
        MemoryType Type,
        MemorySubtype Subtype,
        string CauseKey,
        EmotionalPressureAxis Axis,
        string ResidueLabel,
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
