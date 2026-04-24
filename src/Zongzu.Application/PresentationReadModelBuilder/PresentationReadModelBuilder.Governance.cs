using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Persistence;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static IReadOnlyList<SettlementGovernanceLaneSnapshot> BuildGovernanceSettlements(PresentationReadModelBundle bundle)
    {
        if (bundle.OfficeJurisdictions.Count == 0)
        {
            return [];
        }

        Dictionary<int, SettlementPublicLifeSnapshot> publicLifeBySettlement = IndexFirstBySettlement(
            bundle.PublicLifeSettlements,
            static entry => entry.SettlementId);
        Dictionary<int, SettlementDisorderSnapshot> disorderBySettlement = IndexFirstBySettlement(
            bundle.SettlementDisorder,
            static entry => entry.SettlementId);

        return bundle.OfficeJurisdictions
            .OrderBy(static jurisdiction => jurisdiction.SettlementId.Value)
            .Select(jurisdiction =>
            {
                publicLifeBySettlement.TryGetValue(jurisdiction.SettlementId.Value, out SettlementPublicLifeSnapshot? publicLife);
                disorderBySettlement.TryGetValue(jurisdiction.SettlementId.Value, out SettlementDisorderSnapshot? disorder);

                string orderAftermathSummary = disorder is null
                    ? string.Empty
                    : BuildOrderAdministrativeAftermathExecutionSummary(disorder, jurisdiction);
                string focusLabel = ResolveGovernanceFocusLabel(publicLife, jurisdiction);
                string leadLabel = string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
                    ? jurisdiction.LeadOfficeTitle
                    : jurisdiction.LeadOfficialName;
                string recentOrderCommandName = disorder?.LastInterventionCommandCode ?? string.Empty;
                string recentOrderCommandLabel = disorder?.LastInterventionCommandLabel ?? string.Empty;
                bool hasOrderAftermath = !string.IsNullOrWhiteSpace(orderAftermathSummary);
                PlayerCommandAffordanceSnapshot? suggestedAffordance = SelectPrimaryGovernanceAffordance(
                    bundle.PlayerCommands.Affordances,
                    jurisdiction.SettlementId,
                    hasOrderAftermath);

                return new SettlementGovernanceLaneSnapshot
                {
                    SettlementId = jurisdiction.SettlementId,
                    SettlementName = publicLife?.SettlementName ?? $"Settlement {jurisdiction.SettlementId.Value}",
                    NodeLabel = publicLife?.NodeLabel ?? focusLabel,
                    LeadOfficialName = jurisdiction.LeadOfficialName,
                    LeadOfficeTitle = jurisdiction.LeadOfficeTitle,
                    CurrentAdministrativeTask = jurisdiction.CurrentAdministrativeTask,
                    AdministrativeTaskLoad = jurisdiction.AdministrativeTaskLoad,
                    PetitionPressure = jurisdiction.PetitionPressure,
                    PetitionBacklog = jurisdiction.PetitionBacklog,
                    PublicLegitimacy = publicLife?.PublicLegitimacy ?? 0,
                    StreetTalkHeat = publicLife?.StreetTalkHeat ?? 0,
                    RoutePressure = disorder?.RoutePressure ?? 0,
                    SuppressionDemand = disorder?.SuppressionDemand ?? 0,
                    RecentOrderCommandName = recentOrderCommandName,
                    RecentOrderCommandLabel = recentOrderCommandLabel,
                    HasOrderAdministrativeAftermath = hasOrderAftermath,
                    SuggestedCommandName = suggestedAffordance?.CommandName ?? string.Empty,
                    SuggestedCommandLabel = suggestedAffordance?.Label ?? string.Empty,
                    SuggestedCommandPrompt = BuildGovernanceSuggestedCommandPrompt(suggestedAffordance),
                    PublicPressureSummary = BuildGovernancePublicPressureSummary(publicLife, disorder),
                    PublicMomentumSummary = BuildGovernancePublicMomentumSummary(publicLife, jurisdiction, disorder),
                    OrderAdministrativeAftermathSummary = orderAftermathSummary,
                    GovernanceSummary = BuildGovernanceLaneSummary(
                        focusLabel,
                        leadLabel,
                        jurisdiction,
                        recentOrderCommandLabel,
                        orderAftermathSummary),
                };
            })
            .ToArray();
    }

    private static string ResolveGovernanceFocusLabel(
        SettlementPublicLifeSnapshot? publicLife,
        JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (!string.IsNullOrWhiteSpace(publicLife?.NodeLabel))
        {
            return publicLife.NodeLabel;
        }

        return $"Settlement {jurisdiction.SettlementId.Value}";
    }

    private static string BuildGovernancePublicPressureSummary(
        SettlementPublicLifeSnapshot? publicLife,
        SettlementDisorderSnapshot? disorder)
    {
        if (publicLife is null && disorder is null)
        {
            return string.Empty;
        }

        if (publicLife is null)
        {
            return $"路压{disorder!.RoutePressure}，镇压之需{disorder.SuppressionDemand}。";
        }

        if (disorder is null)
        {
            return $"{publicLife.NodeLabel}街谈{publicLife.StreetTalkHeat}，公议{publicLife.PublicLegitimacy}。";
        }

        return $"{publicLife.NodeLabel}街谈{publicLife.StreetTalkHeat}，公议{publicLife.PublicLegitimacy}，路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。";
    }

    private static string BuildGovernancePublicMomentumSummary(
        SettlementPublicLifeSnapshot? publicLife,
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementDisorderSnapshot? disorder)
    {
        int officeHeat = Math.Clamp(
            (jurisdiction.AdministrativeTaskLoad / 2)
            + (jurisdiction.ClerkDependence / 2)
            + (jurisdiction.PetitionBacklog / 6)
            - (jurisdiction.JurisdictionLeverage / 10),
            0,
            100);
        int noticePressure = publicLife?.NoticeVisibility ?? 0;
        int dispatchPressure = publicLife?.PrefectureDispatchPressure ?? jurisdiction.PetitionPressure;
        int reportLag = publicLife?.RoadReportLag ?? 0;
        int routePressure = disorder?.RoutePressure ?? 0;
        string focusLabel = ResolveGovernanceFocusLabel(publicLife, jurisdiction);

        if (officeHeat >= 72 || dispatchPressure >= 68)
        {
            return $"{focusLabel}眼下县门转紧，案牍与催办并压，街前看榜与递报都在发热。";
        }

        if (officeHeat >= 48 || noticePressure >= 52 || reportLag >= 38)
        {
            return $"{focusLabel}眼下县门渐热，榜示渐密，递报略滞，众人多在探听县里先理哪一宗。";
        }

        if (routePressure >= 36)
        {
            return $"{focusLabel}眼下路上牵扰未散，县门尚能缓办，却已开始顺着路报收束轻重。";
        }

        return $"{focusLabel}眼下县门尚可周旋，案牍虽在流转，还未逼到并案催迫。";
    }

    private static string BuildGovernanceLaneSummary(
        string focusLabel,
        string leadLabel,
        JurisdictionAuthoritySnapshot jurisdiction,
        string recentOrderCommandLabel,
        string orderAftermathSummary)
    {
        if (!string.IsNullOrWhiteSpace(orderAftermathSummary))
        {
            return string.IsNullOrWhiteSpace(recentOrderCommandLabel)
                ? $"{focusLabel}眼下由{leadLabel}主理{jurisdiction.CurrentAdministrativeTask}；积案{jurisdiction.PetitionBacklog}，词牍之压{jurisdiction.PetitionPressure}。"
                : $"{focusLabel}眼下由{leadLabel}主理{jurisdiction.CurrentAdministrativeTask}；上月{recentOrderCommandLabel}后，积案{jurisdiction.PetitionBacklog}，词牍之压{jurisdiction.PetitionPressure}。";
        }

        return $"{focusLabel}眼下由{leadLabel}主理{jurisdiction.CurrentAdministrativeTask}；积案{jurisdiction.PetitionBacklog}，词牍之压{jurisdiction.PetitionPressure}。";
    }

    private static PlayerCommandAffordanceSnapshot? SelectPrimaryGovernanceAffordance(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
        SettlementId settlementId,
        bool hasOrderAftermath)
    {
        return EnumerateAffordancesForSurface(affordances, PlayerCommandSurfaceKeys.PublicLife, settlementId)
            .OrderBy(command => GetGovernanceAffordancePriority(command.CommandName, hasOrderAftermath))
            .ThenBy(command => command.CommandName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static int GetGovernanceAffordancePriority(string commandName, bool hasOrderAftermath)
    {
        return commandName switch
        {
            PlayerCommandNames.PostCountyNotice => hasOrderAftermath ? 0 : 1,
            PlayerCommandNames.DispatchRoadReport => 0,
            PlayerCommandNames.EscortRoadReport => 2,
            PlayerCommandNames.FundLocalWatch => 3,
            PlayerCommandNames.InviteClanEldersPubliclyBroker => 4,
            PlayerCommandNames.SuppressBanditry => hasOrderAftermath ? 6 : 5,
            PlayerCommandNames.NegotiateWithOutlaws => 6,
            PlayerCommandNames.TolerateDisorder => 7,
            _ => 10,
        };
    }

    private static string BuildGovernanceSuggestedCommandPrompt(PlayerCommandAffordanceSnapshot? affordance)
    {
        if (affordance is null)
        {
            return string.Empty;
        }

        return CombineGovernanceDocketText(
            $"眼下可先以{affordance.Label}应对{affordance.TargetLabel}；{affordance.AvailabilitySummary}",
            affordance.LeverageSummary,
            affordance.CostSummary,
            affordance.ExecutionSummary);
    }

    private static GovernanceFocusSnapshot BuildGovernanceFocus(
        IReadOnlyList<SettlementGovernanceLaneSnapshot> governanceSettlements)
    {
        SettlementGovernanceLaneSnapshot? lead = governanceSettlements
            .OrderByDescending(ComputeGovernanceUrgencyScore)
            .ThenByDescending(static settlement => settlement.HasOrderAdministrativeAftermath)
            .ThenBy(static settlement => settlement.SettlementName, StringComparer.Ordinal)
            .FirstOrDefault();

        if (lead is null)
        {
            return new GovernanceFocusSnapshot();
        }

        return new GovernanceFocusSnapshot
        {
            SettlementId = lead.SettlementId,
            SettlementName = lead.SettlementName,
            NodeLabel = lead.NodeLabel,
            UrgencyScore = ComputeGovernanceUrgencyScore(lead),
            HasOrderAdministrativeAftermath = lead.HasOrderAdministrativeAftermath,
            LeadSummary = lead.GovernanceSummary,
            PublicPressureSummary = lead.PublicPressureSummary,
            PublicMomentumSummary = lead.PublicMomentumSummary,
            SuggestedCommandName = lead.SuggestedCommandName,
            SuggestedCommandLabel = lead.SuggestedCommandLabel,
            SuggestedCommandPrompt = lead.SuggestedCommandPrompt,
        };
    }

    private static GovernanceDocketSnapshot BuildGovernanceDocket(
        GovernanceFocusSnapshot focus,
        IReadOnlyList<SettlementGovernanceLaneSnapshot> governanceSettlements,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications,
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts)
    {
        if (focus.UrgencyScore <= 0 || focus.SettlementId == default)
        {
            return new GovernanceDocketSnapshot();
        }

        SettlementGovernanceLaneSnapshot? lane = governanceSettlements
            .FirstOrDefault(settlement => settlement.SettlementId == focus.SettlementId);
        NarrativeNotificationSnapshot? relatedNotification = SelectGovernanceDocketNotification(
            notifications,
            focus.SettlementId);
        PlayerCommandReceiptSnapshot? recentReceipt = SelectGovernanceDocketReceipt(
            receipts,
            focus.SettlementId,
            lane);

        string headline = !string.IsNullOrWhiteSpace(relatedNotification?.Title)
            ? relatedNotification.Title
            : focus.LeadSummary;
        if (string.IsNullOrWhiteSpace(headline))
        {
            headline = lane?.GovernanceSummary ?? string.Empty;
        }

        string whyNowSummary = CombineGovernanceDocketText(
            focus.PublicPressureSummary,
            focus.PublicMomentumSummary,
            lane?.OrderAdministrativeAftermathSummary ?? string.Empty,
            relatedNotification?.WhyItHappened ?? string.Empty);
        if (string.IsNullOrWhiteSpace(whyNowSummary))
        {
            whyNowSummary = lane?.GovernanceSummary ?? string.Empty;
        }

        string handlingSummary = BuildGovernanceDocketHandlingSummary(recentReceipt);
        string phaseLabel = DetermineGovernanceDocketPhaseLabel(
            focus,
            lane,
            relatedNotification,
            recentReceipt);
        string phaseSummary = BuildGovernanceDocketPhaseSummary(
            phaseLabel,
            lane,
            relatedNotification,
            recentReceipt);
        string guidanceSummary = CombineGovernanceDocketText(
            handlingSummary,
            focus.SuggestedCommandPrompt,
            relatedNotification?.WhatNext ?? string.Empty);

        return new GovernanceDocketSnapshot
        {
            SettlementId = focus.SettlementId,
            SettlementName = focus.SettlementName,
            NodeLabel = focus.NodeLabel,
            UrgencyScore = focus.UrgencyScore,
            HasOrderAdministrativeAftermath = focus.HasOrderAdministrativeAftermath,
            HasRelatedNotification = relatedNotification is not null,
            RelatedNotificationTier = relatedNotification?.Tier ?? default,
            RelatedNotificationSurface = relatedNotification?.Surface ?? default,
            RelatedNotificationTitle = relatedNotification?.Title ?? string.Empty,
            RelatedNotificationWhyItHappened = relatedNotification?.WhyItHappened ?? string.Empty,
            RelatedNotificationWhatNext = relatedNotification?.WhatNext ?? string.Empty,
            RelatedNotificationSourceModuleKey = relatedNotification?.SourceModuleKey ?? string.Empty,
            LeadOfficialName = lane?.LeadOfficialName ?? string.Empty,
            LeadOfficeTitle = lane?.LeadOfficeTitle ?? string.Empty,
            CurrentAdministrativeTask = lane?.CurrentAdministrativeTask ?? string.Empty,
            HasRecentReceipt = recentReceipt is not null,
            RecentReceiptSurfaceKey = recentReceipt?.SurfaceKey ?? string.Empty,
            RecentReceiptCommandName = recentReceipt?.CommandName ?? string.Empty,
            RecentReceiptLabel = recentReceipt?.Label ?? string.Empty,
            RecentReceiptSummary = recentReceipt?.Summary ?? string.Empty,
            RecentReceiptOutcomeSummary = recentReceipt?.OutcomeSummary ?? string.Empty,
            RecentReceiptExecutionSummary = recentReceipt?.ExecutionSummary ?? string.Empty,
            RecentReceiptLeverageSummary = recentReceipt?.LeverageSummary ?? string.Empty,
            RecentReceiptCostSummary = recentReceipt?.CostSummary ?? string.Empty,
            RecentReceiptReadbackSummary = recentReceipt?.ReadbackSummary ?? string.Empty,
            Headline = headline,
            WhyNowSummary = whyNowSummary,
            PublicMomentumSummary = focus.PublicMomentumSummary,
            PhaseLabel = phaseLabel,
            PhaseSummary = phaseSummary,
            HandlingSummary = handlingSummary,
            GuidanceSummary = guidanceSummary,
            SuggestedCommandName = focus.SuggestedCommandName,
            SuggestedCommandLabel = focus.SuggestedCommandLabel,
            SuggestedCommandPrompt = focus.SuggestedCommandPrompt,
        };
    }

    private static NarrativeNotificationSnapshot? SelectGovernanceDocketNotification(
        IReadOnlyList<NarrativeNotificationSnapshot> notifications,
        SettlementId settlementId)
    {
        return SelectPrimarySettlementNotification(
            notifications,
            settlementId,
            GetGovernanceDocketNotificationPriority);
    }

    private static PlayerCommandReceiptSnapshot? SelectGovernanceDocketReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        SettlementId settlementId,
        SettlementGovernanceLaneSnapshot? lane)
    {
        return EnumerateReceiptsForSurface(receipts, PlayerCommandSurfaceKeys.PublicLife, settlementId)
            .OrderBy(receipt => GetGovernanceDocketReceiptPriority(receipt, lane))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.ReadbackSummary))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.LeverageSummary))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.ExecutionSummary))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.OutcomeSummary))
            .ThenBy(static receipt => receipt.CommandName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static int GetGovernanceDocketReceiptPriority(
        PlayerCommandReceiptSnapshot receipt,
        SettlementGovernanceLaneSnapshot? lane)
    {
        if (!string.IsNullOrWhiteSpace(lane?.RecentOrderCommandName)
            && string.Equals(receipt.CommandName, lane.RecentOrderCommandName, StringComparison.Ordinal))
        {
            return 0;
        }

        return receipt.SurfaceKey switch
        {
            PlayerCommandSurfaceKeys.PublicLife => 1,
            PlayerCommandSurfaceKeys.Office => 2,
            _ => 3,
        };
    }

    private static int GetGovernanceDocketNotificationPriority(NarrativeNotificationSnapshot notification)
    {
        int sourcePriority = notification.SourceModuleKey switch
        {
            KnownModuleKeys.OfficeAndCareer => 0,
            KnownModuleKeys.OrderAndBanditry => 1,
            KnownModuleKeys.PublicLifeAndRumor => 2,
            KnownModuleKeys.ConflictAndForce => 3,
            _ => 4,
        };

        int surfacePriority = notification.Surface switch
        {
            NarrativeSurface.GreatHall => 0,
            NarrativeSurface.DeskSandbox => 1,
            NarrativeSurface.NotificationCenter => 2,
            _ => 3,
        };

        return (sourcePriority * 10) + (surfacePriority * 2) + (notification.IsRead ? 1 : 0);
    }

    private static string CombineGovernanceDocketText(params string[] values)
    {
        return string.Join(
            " ",
            values
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal));
    }

    private static string BuildGovernanceDocketHandlingSummary(PlayerCommandReceiptSnapshot? receipt)
    {
        if (receipt is null)
        {
            return string.Empty;
        }

        string lead = string.IsNullOrWhiteSpace(receipt.Label)
            ? receipt.CommandName
            : receipt.Label;

        return CombineGovernanceDocketText(
            $"近已按{lead}处置。",
            receipt.Summary,
            receipt.OutcomeSummary,
            receipt.LeverageSummary,
            receipt.CostSummary,
            receipt.ReadbackSummary,
            receipt.ExecutionSummary);
    }

    private static string DetermineGovernanceDocketPhaseLabel(
        GovernanceFocusSnapshot focus,
        SettlementGovernanceLaneSnapshot? lane,
        NarrativeNotificationSnapshot? relatedNotification,
        PlayerCommandReceiptSnapshot? recentReceipt)
    {
        if (focus.HasOrderAdministrativeAftermath)
        {
            return "案后收束";
        }

        if (recentReceipt is not null)
        {
            return "已下处置";
        }

        if (relatedNotification?.Tier == NotificationTier.Urgent)
        {
            return "待即刻应对";
        }

        if ((lane?.PetitionBacklog ?? 0) > 0 || (lane?.PetitionPressure ?? 0) > 0 || focus.UrgencyScore >= 100)
        {
            return "待先理";
        }

        return "案头在看";
    }

    private static string BuildGovernanceDocketPhaseSummary(
        string phaseLabel,
        SettlementGovernanceLaneSnapshot? lane,
        NarrativeNotificationSnapshot? relatedNotification,
        PlayerCommandReceiptSnapshot? recentReceipt)
    {
        string leadLabel = string.IsNullOrWhiteSpace(lane?.LeadOfficialName)
            ? lane?.LeadOfficeTitle ?? string.Empty
            : lane.LeadOfficialName;
        string taskLabel = lane?.CurrentAdministrativeTask ?? string.Empty;
        string orderLabel = lane?.RecentOrderCommandLabel ?? string.Empty;

        return phaseLabel switch
        {
            "案后收束" => string.IsNullOrWhiteSpace(orderLabel)
                ? CombineGovernanceDocketText(
                    $"{leadLabel}正按{taskLabel}收束前案。".Trim(),
                    lane?.OrderAdministrativeAftermathSummary ?? string.Empty)
                : CombineGovernanceDocketText(
                    $"{leadLabel}正按{taskLabel}收束上月{orderLabel}之后账。".Trim(),
                    lane?.OrderAdministrativeAftermathSummary ?? string.Empty),
            "已下处置" => CombineGovernanceDocketText(
                $"{leadLabel}前番已按{recentReceipt?.Label ?? recentReceipt?.CommandName}处置，眼下续理{taskLabel}。".Trim(),
                recentReceipt?.OutcomeSummary ?? string.Empty,
                recentReceipt?.ReadbackSummary ?? string.Empty,
                recentReceipt?.ExecutionSummary ?? string.Empty),
            "待即刻应对" => CombineGovernanceDocketText(
                $"{leadLabel}眼下尚未腾出手，仍须先应{relatedNotification?.Title}。".Trim(),
                relatedNotification?.WhyItHappened ?? string.Empty),
            "待先理" => CombineGovernanceDocketText(
                $"{leadLabel}眼下主理{taskLabel}，宜先看{lane?.NodeLabel}案牍。".Trim(),
                lane?.GovernanceSummary ?? string.Empty),
            _ => CombineGovernanceDocketText(
                $"{leadLabel}正看{taskLabel}。".Trim(),
                lane?.GovernanceSummary ?? string.Empty),
        };
    }

    private static int ComputeGovernanceUrgencyScore(SettlementGovernanceLaneSnapshot settlement)
    {
        int score =
            settlement.PetitionBacklog
            + settlement.PetitionPressure
            + settlement.StreetTalkHeat
            + settlement.RoutePressure
            + settlement.SuppressionDemand;

        if (settlement.HasOrderAdministrativeAftermath)
        {
            score += 20;
        }

        return score;
    }

}
