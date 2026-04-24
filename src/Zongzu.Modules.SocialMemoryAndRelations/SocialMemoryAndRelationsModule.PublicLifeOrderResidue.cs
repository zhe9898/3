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
                     .Where(static disorder => disorder.InterventionCarryoverMonths > 0)
                     .OrderBy(static disorder => disorder.SettlementId.Value))
        {
            if (!TryBuildPublicLifeOrderResidueProfile(disorder.LastInterventionCommandCode, out PublicLifeOrderResidueProfile profile))
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
        climate.LastTrace = $"{profile.CauseKey}: {disorder.LastInterventionCommandLabel} left public-order residue at settlement {disorder.SettlementId.Value}.";

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
            $"{ownerClan.ClanName}承接{disorder.LastInterventionCommandLabel}后留下的{profile.ResidueLabel}，强度{intensity}。",
            ownerClan.Id.Value.ToString());

        if (pressureBand > previousPressureBand)
        {
            scope.Emit(
                SocialMemoryAndRelationsEventNames.EmotionalPressureShifted,
                $"{ownerClan.ClanName}因{disorder.LastInterventionCommandLabel}后账，门内情压升至{pressureBand}阶。",
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
            + (disorder.CoercionRisk / 6);

        return Math.Clamp(pressure, 10, 100);
    }

    private static string BuildPublicLifeOrderResidueNarrative(
        ClanSnapshot ownerClan,
        SettlementDisorderSnapshot disorder,
        PublicLifeOrderResidueProfile profile)
    {
        string orderLabel = string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
            ? disorder.LastInterventionCommandCode
            : disorder.LastInterventionCommandLabel;
        return $"{ownerClan.ClanName}上月按{orderLabel}出面，路口稍稳，{profile.ResidueLabel}却已落进乡议；路压{disorder.RoutePressure}，报复险{disorder.RetaliationRisk}。";
    }

    private static string BuildPublicLifeOrderResidueSummary(
        ClanSnapshot ownerClan,
        SettlementDisorderSnapshot disorder,
        PublicLifeOrderResidueProfile profile,
        int intensity)
    {
        string orderLabel = string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
            ? disorder.LastInterventionCommandCode
            : disorder.LastInterventionCommandLabel;
        return $"{ownerClan.ClanName}因上月{orderLabel}留下{profile.ResidueLabel}；乡里记得谁担保、谁得护路，也记得报复险{disorder.RetaliationRisk}与路压{disorder.RoutePressure}，残留强度{intensity}。";
    }

    private static bool TryBuildPublicLifeOrderResidueProfile(
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
