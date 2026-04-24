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
        PlayerCommandAffordanceSnapshot? affordance = SelectPrimaryHallFamilyLifecycleAffordance(
            bundle.PlayerCommands.Affordances,
            bundle.Clans);
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
            affordance.LeverageSummary,
            affordance.CostSummary,
            affordance.ReadbackSummary,
            affordance.ExecutionSummary);
    }

}
