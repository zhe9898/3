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

    private static PlayerCommandAffordanceSnapshot? SelectPrimaryWarfareHallAffordance(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
        SettlementId settlementId)
    {
        return EnumerateAffordancesForSurface(affordances, PlayerCommandSurfaceKeys.Warfare, settlementId)
            .OrderBy(command => GetWarfareHallAffordancePriority(command.CommandName))
            .ThenBy(static command => command.CommandName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static int GetWarfareHallAffordancePriority(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.ProtectSupplyLine => 0,
            PlayerCommandNames.DraftCampaignPlan => 1,
            PlayerCommandNames.CommitMobilization => 2,
            PlayerCommandNames.WithdrawToBarracks => 3,
            _ => 9,
        };
    }

    private static PlayerCommandReceiptSnapshot? SelectWarfareHallDocketReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        CampaignFrontSnapshot campaign)
    {
        return EnumerateReceiptsForSurface(receipts, PlayerCommandSurfaceKeys.Warfare, campaign.AnchorSettlementId)
            .OrderBy(receipt =>
                string.Equals(receipt.CommandName, campaign.ActiveDirectiveCode, StringComparison.Ordinal) ? 0 : 1)
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.OutcomeSummary))
            .ThenBy(static receipt => receipt.CommandName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static NarrativeNotificationSnapshot? SelectWarfareHallDocketNotification(
        IReadOnlyList<NarrativeNotificationSnapshot> notifications,
        SettlementId settlementId)
    {
        return SelectPrimarySettlementNotification(
            notifications,
            settlementId,
            static _ => 0,
            KnownModuleKeys.WarfareCampaign);
    }

    private static int ComputeWarfareHallDocketUrgencyScore(CampaignFrontSnapshot campaign)
    {
        int score =
            campaign.FrontPressure
            + Math.Max(0, 60 - campaign.SupplyState)
            + Math.Max(0, 60 - campaign.MoraleState);

        if (campaign.IsActive)
        {
            score += 25;
        }

        if (!string.IsNullOrWhiteSpace(campaign.LastAftermathSummary))
        {
            score += 20;
        }

        return score;
    }

    private static string BuildWarfareHallDocketHeadline(CampaignFrontSnapshot campaign)
    {
        if (!string.IsNullOrWhiteSpace(campaign.LastAftermathSummary))
        {
            return $"{campaign.AnchorSettlementName}战后后账未清";
        }

        if (campaign.IsActive)
        {
            return $"{campaign.AnchorSettlementName}军务并案在前";
        }

        return $"{campaign.CampaignName}军情在案";
    }

    private static string BuildWarfareHallDocketWhyNowSummary(
        CampaignFrontSnapshot campaign,
        NarrativeNotificationSnapshot? notification)
    {
        return CombineGovernanceDocketText(
            campaign.LastAftermathSummary,
            campaign.SupplyLineSummary,
            campaign.ActiveDirectiveSummary,
            notification?.WhyItHappened ?? string.Empty);
    }

    private static string BuildWarfareHallDocketOrderingSummary(CampaignFrontSnapshot campaign, int urgencyScore)
    {
        if (!string.IsNullOrWhiteSpace(campaign.LastAftermathSummary) && campaign.IsActive)
        {
            return $"{campaign.CampaignName}因前线仍在推进、战后后账未清，以 {urgencyScore} 的军务压力进入堂上案序。";
        }

        if (!string.IsNullOrWhiteSpace(campaign.LastAftermathSummary))
        {
            return $"{campaign.CampaignName}因战后余波未平、收束仍需人力，以 {urgencyScore} 的后续压力进入堂上案序。";
        }

        if (campaign.IsActive)
        {
            return $"{campaign.CampaignName}因前线、粮道与士气一并承压，以 {urgencyScore} 的军务压力进入堂上案序。";
        }

        return $"{campaign.CampaignName}因军情仍在案头待理，以 {urgencyScore} 的军务压力进入堂上案序。";
    }

    private static string DetermineWarfareHallDocketPhaseLabel(
        CampaignFrontSnapshot campaign,
        PlayerCommandReceiptSnapshot? receipt)
    {
        if (!string.IsNullOrWhiteSpace(campaign.LastAftermathSummary))
        {
            return campaign.IsActive ? "军后并案" : "战后收束";
        }

        if (receipt is not null)
        {
            return "已下军令";
        }

        return campaign.IsActive ? "军情在案" : "案头在看";
    }

    private static string BuildWarfareHallDocketPhaseSummary(
        CampaignFrontSnapshot campaign,
        PlayerCommandReceiptSnapshot? receipt,
        string phaseLabel)
    {
        return phaseLabel switch
        {
            "战后收束" => CombineGovernanceDocketText(
                $"{campaign.CampaignName}已转入战后收束。".Trim(),
                campaign.LastAftermathSummary),
            "军后并案" => CombineGovernanceDocketText(
                $"{campaign.CampaignName}一面续行军务，一面并理战后后账。".Trim(),
                campaign.LastAftermathSummary),
            "已下军令" => CombineGovernanceDocketText(
                $"前番已按{receipt?.Label ?? receipt?.CommandName}行军。".Trim(),
                receipt?.OutcomeSummary ?? string.Empty),
            _ => CombineGovernanceDocketText(
                $"{campaign.CampaignName}眼下依{campaign.ActiveDirectiveLabel}行事。".Trim(),
                campaign.ActiveDirectiveSummary),
        };
    }

    private static string BuildWarfareHallDocketHandlingSummary(
        CampaignFrontSnapshot campaign,
        PlayerCommandReceiptSnapshot? receipt)
    {
        if (receipt is not null)
        {
            return CombineGovernanceDocketText(
                $"近已按{receipt.Label}处置。",
                receipt.Summary,
                receipt.OutcomeSummary);
        }

        return CombineGovernanceDocketText(
            campaign.LastDirectiveTrace,
            campaign.ActiveDirectiveSummary);
    }

}
