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
    private static HallDocketStackSnapshot BuildHallDocketStack(PresentationReadModelBundle bundle)
    {
        List<HallDocketItemSnapshot> items = new();

        HallDocketItemSnapshot? familyItem = BuildFamilyHallDocketItem(bundle);
        if (familyItem is not null)
        {
            items.Add(familyItem);
        }

        HallDocketItemSnapshot? governanceItem = BuildGovernanceHallDocketItem(bundle.GovernanceDocket);
        if (governanceItem is not null)
        {
            items.Add(governanceItem);
        }

        HallDocketItemSnapshot? warfareItem = BuildWarfareHallDocketItem(bundle);
        if (warfareItem is not null)
        {
            items.Add(warfareItem);
        }

        HallDocketItemSnapshot[] ordered = items
            .Where(static item => !string.IsNullOrWhiteSpace(item.Headline))
            .OrderByDescending(static item => item.UrgencyScore)
            .ThenBy(item => GetHallDocketLanePriority(item.LaneKey))
            .ThenBy(static item => item.TargetLabel, StringComparer.Ordinal)
            .ToArray();

        if (ordered.Length == 0)
        {
            return new HallDocketStackSnapshot();
        }

        return new HallDocketStackSnapshot
        {
            LeadItem = ordered[0],
            SecondaryItems = ordered.Skip(1).ToArray(),
        };
    }

    private static HallDocketItemSnapshot? BuildFamilyHallDocketItem(PresentationReadModelBundle bundle)
    {
        PlayerCommandAffordanceSnapshot? affordance = SelectPrimaryHallFamilyLifecycleAffordance(bundle.PlayerCommands.Affordances);
        ClanSnapshot? clan = affordance?.ClanId is { } clanId
            ? bundle.Clans.FirstOrDefault(candidate => candidate.Id == clanId)
            : bundle.Clans
                .OrderByDescending(ComputeFamilyHallDocketUrgencyScore)
                .ThenBy(static candidate => candidate.ClanName, StringComparer.Ordinal)
                .FirstOrDefault();

        PlayerCommandReceiptSnapshot? receipt = SelectFamilyHallDocketReceipt(bundle.PlayerCommands.Receipts, clan?.Id);
        if (clan is null && receipt?.ClanId is { } receiptClanId)
        {
            clan = bundle.Clans.FirstOrDefault(candidate => candidate.Id == receiptClanId);
        }

        if (clan is null)
        {
            return null;
        }

        SettlementSnapshot? settlement = bundle.Settlements.FirstOrDefault(candidate => candidate.Id == clan.HomeSettlementId);
        string headline = BuildFamilyHallDocketHeadline(clan, affordance, receipt);
        string whyNowSummary = BuildFamilyHallDocketWhyNowSummary(clan, affordance);
        string handlingSummary = BuildFamilyHallDocketHandlingSummary(receipt);
        string phaseLabel = DetermineFamilyHallDocketPhaseLabel(receipt, affordance);
        string phaseSummary = BuildFamilyHallDocketPhaseSummary(clan, affordance, receipt, phaseLabel);
        string guidanceSummary = BuildHallAffordancePrompt(affordance);
        int urgencyScore = ComputeFamilyHallDocketUrgencyScore(clan);

        return new HallDocketItemSnapshot
        {
            LaneKey = HallDocketLaneKeys.Family,
            SettlementId = clan.HomeSettlementId,
            ClanId = clan.Id,
            SettlementName = settlement?.Name ?? string.Empty,
            NodeLabel = settlement?.Name ?? string.Empty,
            TargetLabel = clan.ClanName,
            UrgencyScore = urgencyScore,
            OrderingSummary = BuildFamilyHallDocketOrderingSummary(clan, urgencyScore),
            Headline = headline,
            WhyNowSummary = whyNowSummary,
            PhaseLabel = phaseLabel,
            PhaseSummary = phaseSummary,
            HandlingSummary = handlingSummary,
            GuidanceSummary = guidanceSummary,
            SuggestedCommandName = affordance?.CommandName ?? string.Empty,
            SuggestedCommandLabel = affordance?.Label ?? string.Empty,
            SuggestedCommandPrompt = BuildHallAffordancePrompt(affordance),
            SourceProjectionKeys = DistinctNonEmpty(
                HallDocketSourceProjectionKeys.Clans,
                affordance is null ? string.Empty : HallDocketSourceProjectionKeys.PlayerCommandAffordances,
                receipt is null ? string.Empty : HallDocketSourceProjectionKeys.PlayerCommandReceipts),
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.FamilyCore,
                affordance?.ModuleKey ?? string.Empty,
                receipt?.ModuleKey ?? string.Empty),
        };
    }

    private static HallDocketItemSnapshot? BuildGovernanceHallDocketItem(GovernanceDocketSnapshot docket)
    {
        if (string.IsNullOrWhiteSpace(docket.Headline))
        {
            return null;
        }

        return new HallDocketItemSnapshot
        {
            LaneKey = HallDocketLaneKeys.Governance,
            SettlementId = docket.SettlementId,
            SettlementName = docket.SettlementName,
            NodeLabel = docket.NodeLabel,
            TargetLabel = docket.NodeLabel,
            UrgencyScore = docket.UrgencyScore,
            OrderingSummary = BuildGovernanceHallDocketOrderingSummary(docket),
            Headline = docket.Headline,
            WhyNowSummary = docket.WhyNowSummary,
            PhaseLabel = docket.PhaseLabel,
            PhaseSummary = docket.PhaseSummary,
            HandlingSummary = docket.HandlingSummary,
            GuidanceSummary = docket.GuidanceSummary,
            SuggestedCommandName = docket.SuggestedCommandName,
            SuggestedCommandLabel = docket.SuggestedCommandLabel,
            SuggestedCommandPrompt = docket.SuggestedCommandPrompt,
            SourceProjectionKeys = DistinctNonEmpty(
                HallDocketSourceProjectionKeys.GovernanceDocket,
                docket.HasRelatedNotification ? HallDocketSourceProjectionKeys.Notifications : string.Empty,
                docket.HasRecentReceipt ? HallDocketSourceProjectionKeys.PlayerCommandReceipts : string.Empty),
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.OfficeAndCareer,
                KnownModuleKeys.PublicLifeAndRumor,
                KnownModuleKeys.OrderAndBanditry,
                docket.RelatedNotificationSourceModuleKey,
                docket.HasRecentReceipt ? KnownModuleKeys.OrderAndBanditry : string.Empty),
        };
    }

    private static string BuildGovernanceHallDocketOrderingSummary(GovernanceDocketSnapshot docket)
    {
        if (docket.HasOrderAdministrativeAftermath && docket.HasRecentReceipt)
        {
            return $"{docket.NodeLabel}因县署积案与上月处置后账并压，以 {docket.UrgencyScore} 的治理压力排在堂上前列。";
        }

        if (docket.HasOrderAdministrativeAftermath)
        {
            return $"{docket.NodeLabel}因秩序后账拖住县署案牍，以 {docket.UrgencyScore} 的治理压力排在堂上前列。";
        }

        if (docket.HasRelatedNotification)
        {
            return $"{docket.NodeLabel}因公面压力已外显成报，以 {docket.UrgencyScore} 的治理压力排在堂上前列。";
        }

        return $"{docket.NodeLabel}因积案、街谈与路压并看，以 {docket.UrgencyScore} 的治理压力排在堂上前列。";
    }

    private static HallDocketItemSnapshot? BuildWarfareHallDocketItem(PresentationReadModelBundle bundle)
    {
        CampaignFrontSnapshot? campaign = bundle.Campaigns
            .Where(static entry =>
                entry.IsActive
                || !string.IsNullOrWhiteSpace(entry.LastAftermathSummary)
                || !string.IsNullOrWhiteSpace(entry.ActiveDirectiveLabel))
            .OrderByDescending(ComputeWarfareHallDocketUrgencyScore)
            .ThenBy(static entry => entry.CampaignName, StringComparer.Ordinal)
            .FirstOrDefault();
        if (campaign is null)
        {
            return null;
        }

        PlayerCommandReceiptSnapshot? receipt = SelectWarfareHallDocketReceipt(bundle.PlayerCommands.Receipts, campaign);
        NarrativeNotificationSnapshot? notification = SelectWarfareHallDocketNotification(bundle.Notifications, campaign.AnchorSettlementId);
        PlayerCommandAffordanceSnapshot? affordance = SelectPrimaryWarfareHallAffordance(
            bundle.PlayerCommands.Affordances,
            campaign.AnchorSettlementId);

        string phaseLabel = DetermineWarfareHallDocketPhaseLabel(campaign, receipt);
        int urgencyScore = ComputeWarfareHallDocketUrgencyScore(campaign);

        return new HallDocketItemSnapshot
        {
            LaneKey = HallDocketLaneKeys.Warfare,
            SettlementId = campaign.AnchorSettlementId,
            SettlementName = campaign.AnchorSettlementName,
            NodeLabel = campaign.FrontLabel,
            TargetLabel = campaign.CampaignName,
            UrgencyScore = urgencyScore,
            OrderingSummary = BuildWarfareHallDocketOrderingSummary(campaign, urgencyScore),
            Headline = BuildWarfareHallDocketHeadline(campaign),
            WhyNowSummary = BuildWarfareHallDocketWhyNowSummary(campaign, notification),
            PhaseLabel = phaseLabel,
            PhaseSummary = BuildWarfareHallDocketPhaseSummary(campaign, receipt, phaseLabel),
            HandlingSummary = BuildWarfareHallDocketHandlingSummary(campaign, receipt),
            GuidanceSummary = BuildHallAffordancePrompt(affordance),
            SuggestedCommandName = affordance?.CommandName ?? string.Empty,
            SuggestedCommandLabel = affordance?.Label ?? string.Empty,
            SuggestedCommandPrompt = BuildHallAffordancePrompt(affordance),
            SourceProjectionKeys = DistinctNonEmpty(
                HallDocketSourceProjectionKeys.Campaigns,
                notification is null ? string.Empty : HallDocketSourceProjectionKeys.Notifications,
                affordance is null ? string.Empty : HallDocketSourceProjectionKeys.PlayerCommandAffordances,
                receipt is null ? string.Empty : HallDocketSourceProjectionKeys.PlayerCommandReceipts),
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.WarfareCampaign,
                notification?.SourceModuleKey ?? string.Empty,
                affordance?.ModuleKey ?? string.Empty,
                receipt?.ModuleKey ?? string.Empty),
        };
    }

    private static int GetHallDocketLanePriority(string laneKey)
    {
        return laneKey switch
        {
            HallDocketLaneKeys.Governance => 0,
            HallDocketLaneKeys.Family => 1,
            HallDocketLaneKeys.Warfare => 2,
            _ => 9,
        };
    }

    private static IReadOnlyList<string> DistinctNonEmpty(params string[] values)
    {
        return values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static PlayerCommandAffordanceSnapshot? SelectPrimaryHallFamilyLifecycleAffordance(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
    {
        return affordances
            .Where(static entry =>
                entry.IsEnabled
                && string.Equals(entry.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && IsFamilyLifecycleCommand(entry.CommandName))
            .OrderBy(entry => GetFamilyLifecycleCommandPriority(entry.CommandName))
            .ThenBy(static entry => entry.TargetLabel, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static PlayerCommandReceiptSnapshot? SelectFamilyHallDocketReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        ClanId? clanId)
    {
        if (!clanId.HasValue)
        {
            return null;
        }

        return receipts
            .Where(receipt =>
                receipt.ClanId == clanId
                && string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && IsFamilyLifecycleCommand(receipt.CommandName))
            .OrderBy(receipt => GetFamilyLifecycleCommandPriority(receipt.CommandName))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.OutcomeSummary))
            .FirstOrDefault();
    }

    private static bool IsFamilyLifecycleCommand(string commandName)
    {
        return commandName is PlayerCommandNames.SetMourningOrder
            or PlayerCommandNames.SupportNewbornCare
            or PlayerCommandNames.DesignateHeirPolicy
            or PlayerCommandNames.ArrangeMarriage;
    }

    private static int GetFamilyLifecycleCommandPriority(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.SetMourningOrder => 0,
            PlayerCommandNames.SupportNewbornCare => 1,
            PlayerCommandNames.DesignateHeirPolicy => 2,
            PlayerCommandNames.ArrangeMarriage => 3,
            _ => 9,
        };
    }

    private static int ComputeFamilyHallDocketUrgencyScore(ClanSnapshot clan)
    {
        int score =
            clan.MourningLoad * 2
            + clan.InheritancePressure
            + clan.MarriageAlliancePressure
            + clan.ReproductivePressure
            + (clan.InfantCount * 12)
            + Math.Max(0, 60 - clan.HeirSecurity);

        if (!clan.HeirPersonId.HasValue)
        {
            score += 20;
        }

        return score;
    }

    private static string BuildFamilyHallDocketOrderingSummary(ClanSnapshot clan, int urgencyScore)
    {
        if (clan.MourningLoad > 0)
        {
            return $"{clan.ClanName}因门内举哀与承祧后续并压，以 {urgencyScore} 的家门压力排入堂上前列。";
        }

        if (!clan.HeirPersonId.HasValue || clan.HeirSecurity < 40)
        {
            return $"{clan.ClanName}因承祧未定、继嗣稳度偏低，以 {urgencyScore} 的继嗣压力排入堂上前列。";
        }

        if (clan.InfantCount > 0)
        {
            return $"{clan.ClanName}因新婴照护与宗房余力同看，以 {urgencyScore} 的家内压力排入堂上前列。";
        }

        return $"{clan.ClanName}因婚议与添丁两端并压，以 {urgencyScore} 的家门压力排入堂上前列。";
    }

    private static string BuildFamilyHallDocketHeadline(
        ClanSnapshot clan,
        PlayerCommandAffordanceSnapshot? affordance,
        PlayerCommandReceiptSnapshot? receipt)
    {
        if (clan.MourningLoad > 0)
        {
            return $"{clan.ClanName}门内举哀未毕";
        }

        if (!clan.HeirPersonId.HasValue || clan.HeirSecurity < 40)
        {
            return $"{clan.ClanName}承祧未定";
        }

        if (clan.InfantCount > 0)
        {
            return $"{clan.ClanName}襁褓待护";
        }

        if (clan.MarriageAlliancePressure >= 28 || clan.MarriageAllianceValue < 48)
        {
            return $"{clan.ClanName}婚议待定";
        }

        if (!string.IsNullOrWhiteSpace(affordance?.Label))
        {
            return $"{clan.ClanName}{affordance.Label}";
        }

        if (!string.IsNullOrWhiteSpace(receipt?.Label))
        {
            return $"{clan.ClanName}{receipt.Label}";
        }

        return $"{clan.ClanName}门内后计待理";
    }

    private static string BuildFamilyHallDocketWhyNowSummary(
        ClanSnapshot clan,
        PlayerCommandAffordanceSnapshot? affordance)
    {
        string pressureSummary = clan.MourningLoad > 0
            ? $"门内丧服之重{clan.MourningLoad}，后议之压{clan.InheritancePressure}。"
            : !clan.HeirPersonId.HasValue || clan.HeirSecurity < 40
                ? $"承祧稳度{clan.HeirSecurity}，后议之压{clan.InheritancePressure}。"
                : clan.InfantCount > 0
                    ? $"门内现有襁褓{clan.InfantCount}口，宗房余力{clan.SupportReserve}。"
                    : $"婚议之压{clan.MarriageAlliancePressure}，添丁之望{clan.ReproductivePressure}。";

        return CombineGovernanceDocketText(
            pressureSummary,
            affordance?.AvailabilitySummary ?? string.Empty);
    }

    private static string DetermineFamilyHallDocketPhaseLabel(
        PlayerCommandReceiptSnapshot? receipt,
        PlayerCommandAffordanceSnapshot? affordance)
    {
        if (receipt is not null)
        {
            return "已下处置";
        }

        if (affordance is not null)
        {
            return "待先理";
        }

        return "案头在看";
    }

    private static string BuildFamilyHallDocketPhaseSummary(
        ClanSnapshot clan,
        PlayerCommandAffordanceSnapshot? affordance,
        PlayerCommandReceiptSnapshot? receipt,
        string phaseLabel)
    {
        return phaseLabel switch
        {
            "已下处置" => CombineGovernanceDocketText(
                $"{clan.ClanName}前番已按{receipt?.Label ?? receipt?.CommandName}处置。".Trim(),
                receipt?.OutcomeSummary ?? string.Empty),
            "待先理" => CombineGovernanceDocketText(
                $"{clan.ClanName}眼下宜先{affordance?.Label}。".Trim(),
                affordance?.AvailabilitySummary ?? string.Empty),
            _ => $"{clan.ClanName}门内后计仍在案头。".Trim(),
        };
    }

    private static string BuildFamilyHallDocketHandlingSummary(PlayerCommandReceiptSnapshot? receipt)
    {
        if (receipt is null)
        {
            return string.Empty;
        }

        return CombineGovernanceDocketText(
            $"近已按{receipt.Label}处置。",
            receipt.Summary,
            receipt.OutcomeSummary);
    }

    private static PlayerCommandAffordanceSnapshot? SelectPrimaryWarfareHallAffordance(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
        SettlementId settlementId)
    {
        return affordances
            .Where(command =>
                command.IsEnabled
                && command.SettlementId == settlementId
                && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal))
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
        return receipts
            .Where(receipt =>
                receipt.SettlementId == campaign.AnchorSettlementId
                && string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal))
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
        string settlementKey = settlementId.Value.ToString();

        return notifications
            .Where(notification =>
                string.Equals(notification.SourceModuleKey, KnownModuleKeys.WarfareCampaign, StringComparison.Ordinal)
                && notification.Traces.Any(trace => string.Equals(trace.EntityKey, settlementKey, StringComparison.Ordinal)))
            .OrderBy(static notification => notification.Tier)
            .ThenByDescending(static notification => notification.CreatedAt.Year)
            .ThenByDescending(static notification => notification.CreatedAt.Month)
            .ThenByDescending(static notification => notification.Id.Value)
            .FirstOrDefault();
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

    private static string BuildHallAffordancePrompt(PlayerCommandAffordanceSnapshot? affordance)
    {
        if (affordance is null)
        {
            return string.Empty;
        }

        string targetLabel = string.IsNullOrWhiteSpace(affordance.TargetLabel)
            ? string.Empty
            : affordance.TargetLabel;
        string targetClause = string.IsNullOrWhiteSpace(targetLabel)
            ? affordance.Label
            : $"{affordance.Label}应对{targetLabel}";

        return CombineGovernanceDocketText(
            $"眼下可先{targetClause}。",
            affordance.AvailabilitySummary,
            affordance.ExecutionSummary);
    }

}
