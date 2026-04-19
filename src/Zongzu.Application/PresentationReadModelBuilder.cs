using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Persistence;

namespace Zongzu.Application;

public sealed class PresentationReadModelBuilder
{
    private readonly SaveCodec _saveCodec = new();

    public PresentationReadModelBundle BuildForM2(GameSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        QueryRegistry queries = BuildQueries(simulation);

        PresentationReadModelBundle bundle = new()
        {
            CurrentDate = simulation.CurrentDate,
            ReplayHash = simulation.ReplayHash,
        };

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.FamilyCore))
        {
            bundle.Clans = queries.GetRequired<IFamilyCoreQueries>().GetClans();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.SocialMemoryAndRelations))
        {
            bundle.ClanNarratives = queries.GetRequired<ISocialMemoryAndRelationsQueries>().GetClanNarratives();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WorldSettlements))
        {
            bundle.Settlements = queries.GetRequired<IWorldSettlementsQueries>().GetSettlements();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PopulationAndHouseholds))
        {
            bundle.PopulationSettlements = queries.GetRequired<IPopulationAndHouseholdsQueries>().GetSettlements();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.EducationAndExams))
        {
            IEducationAndExamsQueries educationQueries = queries.GetRequired<IEducationAndExamsQueries>();
            bundle.EducationCandidates = educationQueries.GetCandidates();
            bundle.Academies = educationQueries.GetAcademies();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry))
        {
            ITradeAndIndustryQueries tradeQueries = queries.GetRequired<ITradeAndIndustryQueries>();
            ClanTradeSnapshot[] clanTrades = tradeQueries.GetClanTrades().ToArray();
            bundle.ClanTrades = clanTrades;
            bundle.Markets = tradeQueries.GetMarkets();
            bundle.TradeRoutes = clanTrades
                .SelectMany(trade => tradeQueries.GetRoutesForClan(trade.ClanId))
                .OrderBy(static route => route.RouteId)
                .ToArray();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PublicLifeAndRumor))
        {
            bundle.PublicLifeSettlements = queries.GetRequired<IPublicLifeAndRumorQueries>().GetSettlementPublicLife();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry))
        {
            bundle.SettlementDisorder = queries.GetRequired<IOrderAndBanditryQueries>().GetSettlementDisorder();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            IOfficeAndCareerQueries officeQueries = queries.GetRequired<IOfficeAndCareerQueries>();
            bundle.OfficeCareers = officeQueries.GetCareers();
            bundle.OfficeJurisdictions = officeQueries.GetJurisdictions();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign))
        {
            IWarfareCampaignQueries warfareQueries = queries.GetRequired<IWarfareCampaignQueries>();
            bundle.Campaigns = warfareQueries.GetCampaigns();
            bundle.CampaignMobilizationSignals = warfareQueries.GetMobilizationSignals();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.NarrativeProjection))
        {
            bundle.Notifications = queries.GetRequired<INarrativeProjectionQueries>().GetNotifications();
        }

        bundle.PlayerCommands = BuildPlayerCommandSurface(bundle);
        bundle.GovernanceSettlements = BuildGovernanceSettlements(bundle);
        bundle.GovernanceFocus = BuildGovernanceFocus(bundle.GovernanceSettlements);
        bundle.GovernanceDocket = BuildGovernanceDocket(
            bundle.GovernanceFocus,
            bundle.GovernanceSettlements,
            bundle.Notifications,
            bundle.PlayerCommands.Receipts);
        bundle.HallDocket = BuildHallDocketStack(bundle);
        bundle.Debug = BuildDebugSnapshot(simulation, bundle.Notifications);
        return bundle;
    }

    private static QueryRegistry BuildQueries(GameSimulation simulation)
    {
        QueryRegistry queries = new();
        foreach (IModuleRunner module in simulation.Modules
                     .OrderBy(static module => module.Phase)
                     .ThenBy(static module => module.ExecutionOrder)
                     .ThenBy(static module => module.ModuleKey, StringComparer.Ordinal))
        {
            if (!simulation.FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!simulation.TryGetModuleState(module.ModuleKey, out object? state) || state is null)
            {
                throw new InvalidOperationException($"Enabled module {module.ModuleKey} has no state for read-model export.");
            }

            module.RegisterQueries(state, queries);
        }

        return queries;
    }

    private static PlayerCommandSurfaceSnapshot BuildPlayerCommandSurface(PresentationReadModelBundle bundle)
    {
        return new PlayerCommandSurfaceSnapshot
        {
            Affordances = BuildPlayerCommandAffordances(bundle),
            Receipts = BuildPlayerCommandReceipts(bundle),
        };
    }

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

    private static IReadOnlyList<PlayerCommandAffordanceSnapshot> BuildPlayerCommandAffordances(PresentationReadModelBundle bundle)
    {
        List<PlayerCommandAffordanceSnapshot> affordances = new();
        Dictionary<int, ClanNarrativeSnapshot> narrativesByClan = bundle.ClanNarratives
            .ToDictionary(static narrative => narrative.ClanId.Value, static narrative => narrative);

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.ClanName, StringComparer.Ordinal))
        {
            ClanNarrativeSnapshot? narrative = narrativesByClan.TryGetValue(clan.Id.Value, out ClanNarrativeSnapshot? snapshot)
                ? snapshot
                : null;
            int grievancePressure = narrative?.GrudgePressure ?? 0;

            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SupportSeniorBranch,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SupportSeniorBranch),
                Summary = $"{clan.ClanName}可在祠堂先定嫡支体面与承祧次序，但旁支怨气会随之浮起。",
                IsEnabled = true,
                AvailabilitySummary = "此令可随时下达，但最易牵动房支偏怨。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.OrderFormalApology,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.OrderFormalApology),
                Summary = $"{clan.ClanName}可先责成赔礼，以压祠堂口角与旧怨。",
                IsEnabled = clan.BranchTension >= 18 || grievancePressure >= 20,
                AvailabilitySummary = clan.BranchTension >= 18 || grievancePressure >= 20
                    ? $"当前房支争势{clan.BranchTension}，适宜先压口角。"
                    : "眼下争声尚浅，赔礼之令未必需要。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.PermitBranchSeparation,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.PermitBranchSeparation),
                Summary = $"{clan.ClanName}可准旁支分房，以拆开同灶积怨与承祧旧账。",
                IsEnabled = clan.SeparationPressure >= 35 || clan.BranchTension >= 55,
                AvailabilitySummary = clan.SeparationPressure >= 35 || clan.BranchTension >= 55
                    ? $"分房之压{clan.SeparationPressure}，已有拆灶立门户之势。"
                    : "分房之议未炽，暂可留待后断。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SuspendClanRelief,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SuspendClanRelief),
                Summary = $"{clan.ClanName}可停其接济，以示宗房威断，但房支怨望会更深。",
                IsEnabled = clan.SupportReserve >= 8,
                AvailabilitySummary = clan.SupportReserve >= 8
                    ? $"宗房余力{clan.SupportReserve}，足可抽去接济。"
                    : "宗房余力浅薄，再停接济只会自伤。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersMediation,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.InviteClanEldersMediation),
                Summary = $"{clan.ClanName}可请族老调停，先让堂议有台阶可下。",
                IsEnabled = clan.BranchTension >= 20 || clan.SeparationPressure >= 20 || grievancePressure >= 20,
                AvailabilitySummary = clan.BranchTension >= 20 || clan.SeparationPressure >= 20 || grievancePressure >= 20
                    ? "争议已起，请族老最能先缓祠堂气口。"
                    : "当前祠堂争议未盛，暂不必惊动族老。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.ArrangeMarriage,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.ArrangeMarriage),
                Summary = $"{clan.ClanName}可先议亲定婚，借姻亲稳一稳香火、人情与房支后计。",
                IsEnabled = clan.MourningLoad < 18 && (clan.MarriageAlliancePressure >= 28 || clan.MarriageAllianceValue < 48),
                AvailabilitySummary = clan.MourningLoad >= 18
                    ? $"门内丧服未除，婚议暂宜后缓；丧服之重{clan.MourningLoad}。"
                    : $"婚议之压{clan.MarriageAlliancePressure}，姻亲可资之势{clan.MarriageAllianceValue}。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SupportNewbornCare,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SupportNewbornCare),
                Summary = $"{clan.ClanName}可先拨粮护婴，把产后调护、乳哺与襁褓衣食稳下来。",
                IsEnabled = clan.InfantCount > 0 && clan.SupportReserve >= 4,
                AvailabilitySummary = clan.InfantCount == 0
                    ? "门内暂无线褓幼儿，眼下无须另拨护婴之费。"
                    : clan.SupportReserve >= 4
                        ? $"门内现有襁褓{clan.InfantCount}口，宗房余力{clan.SupportReserve}。"
                        : $"门内现有襁褓{clan.InfantCount}口，但宗房余力{clan.SupportReserve}，一时难再加拨。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.DesignateHeirPolicy,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.DesignateHeirPolicy),
                Summary = $"{clan.ClanName}可先定承祧次序，把香火名分与后议先写稳。",
                IsEnabled = !clan.HeirPersonId.HasValue || clan.HeirSecurity < 60,
                AvailabilitySummary = !clan.HeirPersonId.HasValue
                    ? "堂上尚未举出承祧之人，宜先定后序。"
                    : $"承祧稳度{clan.HeirSecurity}，名分若虚仍易再起后议。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SetMourningOrder,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SetMourningOrder),
                Summary = $"{clan.ClanName}可先议定丧次与祭次，别让门内一边举哀一边再翻后议。",
                IsEnabled = clan.MourningLoad > 0,
                AvailabilitySummary = clan.MourningLoad > 0
                    ? $"门内丧服之重{clan.MourningLoad}，宜先定服序与支用。"
                    : "门内暂无举哀之事，眼下不必另议丧次。",
                TargetLabel = clan.ClanName,
            });
        }

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            bool canReviewPetitions = jurisdiction.PetitionBacklog > 0 || jurisdiction.PetitionPressure > 0;
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.OfficeAndCareer,
                SurfaceKey = PlayerCommandSurfaceKeys.Office,
                SettlementId = jurisdiction.SettlementId,
                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
                Label = PlayerCommandService.DetermineOfficeCommandLabel(PlayerCommandNames.PetitionViaOfficeChannels),
                Summary = $"{jurisdiction.LeadOfficialName}可在{jurisdiction.LeadOfficeTitle}任上先理词状，缓解积案与乡里怨气。",
                IsEnabled = canReviewPetitions,
                AvailabilitySummary = canReviewPetitions
                    ? $"积案{jurisdiction.PetitionBacklog}，可先批结一轮。"
                    : "本处暂无待批词状。",
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.OfficeAndCareer,
                SurfaceKey = PlayerCommandSurfaceKeys.Office,
                SettlementId = jurisdiction.SettlementId,
                CommandName = PlayerCommandNames.DeployAdministrativeLeverage,
                Label = PlayerCommandService.DetermineOfficeCommandLabel(PlayerCommandNames.DeployAdministrativeLeverage),
                Summary = $"{jurisdiction.LeadOfficialName}可凭官箴与印信发签催办，先压急牍与拖延。",
                IsEnabled = jurisdiction.JurisdictionLeverage >= 12,
                AvailabilitySummary = jurisdiction.JurisdictionLeverage >= 12
                    ? $"乡面杠杆{jurisdiction.JurisdictionLeverage}，足可催动里甲与吏胥。"
                    : "此地官箴未足，不宜强行发签。",
            });
        }

        foreach (CampaignMobilizationSignalSnapshot signal in bundle.CampaignMobilizationSignals.OrderBy(static entry => entry.SettlementId.Value))
        {
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.DraftCampaignPlan,
                "先在案头筹议关津、驿报与前后队次，不急于放大边缘摩擦。",
                true,
                "此令偏向先定方略，不急发众。"));
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.CommitMobilization,
                "发檄点兵，先聚守丁、乡勇与护运之众，再定前压与驻防。",
                !string.Equals(signal.MobilizationWindowLabel, "Closed", StringComparison.Ordinal),
                string.Equals(signal.MobilizationWindowLabel, "Closed", StringComparison.Ordinal)
                    ? "当前动员窗已闭，不宜强起兵众。"
                    : $"当前动员窗为{RenderMobilizationWindow(signal.MobilizationWindowLabel)}。"));
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.ProtectSupplyLine,
                "催督粮道与渡口，优先保住转运、驿报与军前补给。",
                signal.AvailableForceCount > 0,
                signal.AvailableForceCount > 0
                    ? $"可调之众{signal.AvailableForceCount}。"
                    : "当前无可调之众。"));
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.WithdrawToBarracks,
                "暂收行伍归营，整顿伤员、粮册与营中号令。",
                signal.AvailableForceCount > 0,
                signal.AvailableForceCount > 0
                    ? "可行收军归营之令。"
                    : "当前无营伍可收。"));
        }

        affordances.AddRange(BuildPublicLifeAffordances(bundle));
        return affordances;
    }

    private static PlayerCommandAffordanceSnapshot BuildWarfareAffordance(
        CampaignMobilizationSignalSnapshot signal,
        string commandName,
        string summary,
        bool isEnabled,
        string availabilitySummary)
    {
        return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
            SettlementId = signal.SettlementId,
            CommandName = commandName,
            Label = WarfareCampaignDescriptors.DetermineDirectiveLabel(commandName),
            Summary = summary,
            IsEnabled = isEnabled,
            AvailabilitySummary = availabilitySummary,
        };
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildPublicLifeAffordances(PresentationReadModelBundle bundle)
    {
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = bundle.OfficeJurisdictions
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);
        Dictionary<int, SettlementDisorderSnapshot> disorderBySettlement = bundle.SettlementDisorder
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);
        ILookup<int, ClanSnapshot> clansBySettlement = bundle.Clans.ToLookup(static entry => entry.HomeSettlementId.Value);

        foreach (SettlementPublicLifeSnapshot publicLife in bundle.PublicLifeSettlements.OrderBy(static entry => entry.SettlementId.Value))
        {
            jurisdictionsBySettlement.TryGetValue(publicLife.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);

            if (jurisdiction is not null)
            {
                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.PostCountyNotice),
                    Summary = $"{publicLife.NodeLabel}街谈已热，可先借榜示压住众口。",
                    IsEnabled = publicLife.StreetTalkHeat >= 40 || publicLife.PublicLegitimacy < 55,
                    AvailabilitySummary = $"榜示分量{publicLife.DocumentaryWeight}，由{jurisdiction.LeadOfficialName}主其晓谕。",
                    TargetLabel = publicLife.NodeLabel,
                };

                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    CommandName = PlayerCommandNames.DispatchRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.DispatchRoadReport),
                    Summary = $"{publicLife.DominantVenueLabel}消息往来已有迟滞，可先遣吏催报。",
                    IsEnabled = publicLife.RoadReportLag >= 36 || publicLife.CourierRisk >= 35,
                    AvailabilitySummary = $"递报险数{publicLife.CourierRisk}，查验周折{publicLife.VerificationCost}。",
                    TargetLabel = publicLife.DominantVenueLabel,
                };

            }

            if (disorderBySettlement.TryGetValue(publicLife.SettlementId.Value, out SettlementDisorderSnapshot? disorder))
            {
                OrderAdministrativeReachProfile administrativeReach = OrderAdministrativeReachEvaluator.Evaluate(jurisdiction);

                foreach (PlayerCommandAffordanceSnapshot affordance in BuildSupplementalOrderPublicLifeAffordances(publicLife, disorder, administrativeReach))
                {
                    yield return affordance;
                }

                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    CommandName = PlayerCommandNames.EscortRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.EscortRoadReport),
                    Summary = $"{publicLife.DominantVenueLabel}近来路情不稳，可先催护一路，保住津口与路报。",
                    IsEnabled = disorder.RoutePressure >= 28 || publicLife.CourierRisk >= 32,
                    AvailabilitySummary = $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                    ExecutionSummary = administrativeReach.ExecutionSummary,
                    TargetLabel = publicLife.DominantVenueLabel,
                };
            }

            ClanSnapshot? leadClan = clansBySettlement[publicLife.SettlementId.Value]
                .OrderByDescending(static entry => entry.Prestige)
                .ThenBy(static entry => entry.ClanName, StringComparer.Ordinal)
                .FirstOrDefault();
            if (leadClan is not null)
            {
                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    ClanId = leadClan.Id,
                    CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.InviteClanEldersPubliclyBroker),
                    Summary = $"{leadClan.ClanName}可请族老先出面缓口，免得堂内家事扩成街谈公议。",
                    IsEnabled = publicLife.StreetTalkHeat >= 45 || publicLife.MarketRumorFlow >= 45,
                    AvailabilitySummary = $"街谈{publicLife.StreetTalkHeat}，市语流势{publicLife.MarketRumorFlow}。",
                    TargetLabel = leadClan.ClanName,
                };
            }
        }
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildSupplementalOrderPublicLifeAffordances(
        SettlementPublicLifeSnapshot publicLife,
        SettlementDisorderSnapshot disorder,
        OrderAdministrativeReachProfile administrativeReach)
    {
        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.FundLocalWatch,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.FundLocalWatch),
            Summary = $"{publicLife.DominantVenueLabel}近来脚路不稳，可先添雇巡丁，把路口与渡头补起来。",
            IsEnabled = disorder.RoutePressure >= 22 || disorder.DisorderPressure >= 24,
            AvailabilitySummary = $"路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.DominantVenueLabel,
        };

        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.SuppressBanditry,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.SuppressBanditry),
            Summary = $"{publicLife.NodeLabel}已见路匪踪迹，可先严缉，但后手报复也会更重。",
            IsEnabled = disorder.BanditThreat >= 36 || disorder.SuppressionDemand >= 32,
            AvailabilitySummary = $"盗压{disorder.BanditThreat}，镇压之需{disorder.SuppressionDemand}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.NodeLabel,
        };

        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.NegotiateWithOutlaws,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.NegotiateWithOutlaws),
            Summary = $"{publicLife.DominantVenueLabel}若先求一时通路，可遣人议路，换一段缓和。",
            IsEnabled = disorder.BanditThreat >= 24 || disorder.DisorderPressure >= 28,
            AvailabilitySummary = $"盗压{disorder.BanditThreat}，地面不靖{disorder.DisorderPressure}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.DominantVenueLabel,
        };

        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.TolerateDisorder,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.TolerateDisorder),
            Summary = $"{publicLife.NodeLabel}若眼下不宜再逼，也可先缓一缓穷追，把明面风声压住。",
            IsEnabled = disorder.BanditThreat >= 18 || disorder.RoutePressure >= 18 || disorder.DisorderPressure >= 18,
            AvailabilitySummary = $"盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.NodeLabel,
        };
    }

    private static IEnumerable<PlayerCommandReceiptSnapshot> BuildPublicLifeReceipts(PresentationReadModelBundle bundle)
    {
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = bundle.OfficeJurisdictions
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "张榜晓谕", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.PostCountyNotice),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }

            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "遣吏催报", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.DispatchRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.DispatchRoadReport),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            jurisdictionsBySettlement.TryGetValue(disorder.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
            PlayerCommandReceiptSnapshot? receipt = BuildOrderPublicLifeReceipt(disorder, jurisdiction);
            if (receipt is not null)
            {
                yield return receipt;
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (!disorder.LastPressureReason.Contains("催护一路", StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = disorder.SettlementId,
                CommandName = PlayerCommandNames.EscortRoadReport,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.EscortRoadReport),
                Summary = disorder.LastPressureReason,
                OutcomeSummary = $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                TargetLabel = disorder.SettlementId.Value.ToString(),
            };
        }

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.HomeSettlementId.Value))
        {
            if (!string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.InviteClanEldersPubliclyBroker),
                Summary = clan.LastConflictTrace,
                OutcomeSummary = clan.LastConflictOutcome,
                TargetLabel = clan.ClanName,
            };
        }
    }

    private static IEnumerable<PlayerCommandReceiptSnapshot> BuildPublicLifeReceiptsNormalized(PresentationReadModelBundle bundle)
    {
        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "张榜晓谕", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.PostCountyNotice),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }

            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "遣吏催报", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.DispatchRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.DispatchRoadReport),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (!disorder.LastPressureReason.Contains("催护一路", StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = disorder.SettlementId,
                CommandName = PlayerCommandNames.EscortRoadReport,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.EscortRoadReport),
                Summary = disorder.LastPressureReason,
                OutcomeSummary = $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                TargetLabel = disorder.SettlementId.Value.ToString(),
            };
        }

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.HomeSettlementId.Value))
        {
            if (!string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.InviteClanEldersPubliclyBroker),
                Summary = clan.LastConflictTrace,
                OutcomeSummary = clan.LastConflictOutcome,
                TargetLabel = clan.ClanName,
            };
        }
    }

    private static PlayerCommandReceiptSnapshot? BuildOrderPublicLifeReceipt(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (string.IsNullOrWhiteSpace(disorder.LastInterventionCommandCode))
        {
            return null;
        }

        return new PlayerCommandReceiptSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = disorder.SettlementId,
            CommandName = disorder.LastInterventionCommandCode,
            Label = string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
                ? PlayerCommandService.DeterminePublicLifeCommandLabel(disorder.LastInterventionCommandCode)
                : disorder.LastInterventionCommandLabel,
            Summary = disorder.LastInterventionSummary,
            OutcomeSummary = disorder.LastInterventionOutcome,
            ExecutionSummary = BuildOrderAdministrativeAftermathExecutionSummary(disorder, jurisdiction),
            TargetLabel = disorder.SettlementId.Value.ToString(),
        };
    }

    private static string BuildOrderAdministrativeAftermathExecutionSummary(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null
            || string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
            || (string.IsNullOrWhiteSpace(jurisdiction.LastPetitionOutcome)
                && string.IsNullOrWhiteSpace(jurisdiction.LastAdministrativeTrace)))
        {
            return string.Empty;
        }

        bool linkedToOrderAftermath =
            jurisdiction.LastPetitionOutcome.Contains(disorder.LastInterventionCommandLabel, StringComparison.Ordinal)
            || jurisdiction.LastAdministrativeTrace.Contains(disorder.LastInterventionCommandLabel, StringComparison.Ordinal);
        if (!linkedToOrderAftermath)
        {
            return string.Empty;
        }

        string leadLabel = string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
            ? jurisdiction.LeadOfficeTitle
            : jurisdiction.LeadOfficialName;
        return $"{leadLabel}眼下仍在{jurisdiction.CurrentAdministrativeTask}；积案{jurisdiction.PetitionBacklog}，{jurisdiction.LastPetitionOutcome}";
    }

    private static IReadOnlyList<SettlementGovernanceLaneSnapshot> BuildGovernanceSettlements(PresentationReadModelBundle bundle)
    {
        if (bundle.OfficeJurisdictions.Count == 0)
        {
            return [];
        }

        Dictionary<int, SettlementPublicLifeSnapshot> publicLifeBySettlement = bundle.PublicLifeSettlements
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);
        Dictionary<int, SettlementDisorderSnapshot> disorderBySettlement = bundle.SettlementDisorder
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);

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
        return affordances
            .Where(command =>
                command.IsEnabled
                && command.SettlementId == settlementId
                && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal))
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

        string prompt = $"眼下可先以{affordance.Label}应对{affordance.TargetLabel}；{affordance.AvailabilitySummary}";
        if (string.IsNullOrWhiteSpace(affordance.ExecutionSummary))
        {
            return prompt;
        }

        return $"{prompt} {affordance.ExecutionSummary}";
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
        string settlementKey = settlementId.Value.ToString();

        return notifications
            .Where(notification => notification.Traces.Any(trace =>
                string.Equals(trace.EntityKey, settlementKey, StringComparison.Ordinal)))
            .OrderBy(GetGovernanceDocketNotificationPriority)
            .ThenBy(static notification => notification.Tier)
            .ThenByDescending(static notification => notification.CreatedAt.Year)
            .ThenByDescending(static notification => notification.CreatedAt.Month)
            .ThenByDescending(static notification => notification.Id.Value)
            .FirstOrDefault();
    }

    private static PlayerCommandReceiptSnapshot? SelectGovernanceDocketReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        SettlementId settlementId,
        SettlementGovernanceLaneSnapshot? lane)
    {
        return receipts
            .Where(receipt => receipt.SettlementId == settlementId)
            .OrderBy(receipt => GetGovernanceDocketReceiptPriority(receipt, lane))
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

    private static IReadOnlyList<PlayerCommandReceiptSnapshot> BuildPlayerCommandReceipts(PresentationReadModelBundle bundle)
    {
        List<PlayerCommandReceiptSnapshot> receipts = new();

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.ClanName, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(clan.LastConflictTrace)
                && string.IsNullOrWhiteSpace(clan.LastConflictOutcome)
                && string.IsNullOrWhiteSpace(clan.LastLifecycleTrace)
                && string.IsNullOrWhiteSpace(clan.LastLifecycleOutcome))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(clan.LastConflictTrace) || !string.IsNullOrWhiteSpace(clan.LastConflictOutcome))
            {
                receipts.Add(new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = string.IsNullOrWhiteSpace(clan.LastConflictCommandCode)
                    ? PlayerCommandNames.InviteClanEldersMediation
                    : clan.LastConflictCommandCode,
                Label = string.IsNullOrWhiteSpace(clan.LastConflictCommandLabel)
                    ? "祠堂议决"
                    : clan.LastConflictCommandLabel,
                Summary = clan.LastConflictTrace,
                OutcomeSummary = clan.LastConflictOutcome,
                TargetLabel = clan.ClanName,
            });
            }

            if (!string.IsNullOrWhiteSpace(clan.LastLifecycleTrace) || !string.IsNullOrWhiteSpace(clan.LastLifecycleOutcome))
            {
                receipts.Add(new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = clan.HomeSettlementId,
                    ClanId = clan.Id,
                    CommandName = string.IsNullOrWhiteSpace(clan.LastLifecycleCommandCode)
                        ? PlayerCommandNames.ArrangeMarriage
                        : clan.LastLifecycleCommandCode,
                    Label = string.IsNullOrWhiteSpace(clan.LastLifecycleCommandLabel)
                        ? "门内后计"
                        : clan.LastLifecycleCommandLabel,
                    Summary = clan.LastLifecycleTrace,
                    OutcomeSummary = clan.LastLifecycleOutcome,
                    TargetLabel = clan.ClanName,
                });
            }
        }

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (string.IsNullOrWhiteSpace(jurisdiction.LastAdministrativeTrace) && string.IsNullOrWhiteSpace(jurisdiction.LastPetitionOutcome))
            {
                continue;
            }

            string commandName = string.Equals(jurisdiction.PetitionOutcomeCategory, "Granted", StringComparison.Ordinal)
                ? PlayerCommandNames.DeployAdministrativeLeverage
                : PlayerCommandNames.PetitionViaOfficeChannels;

            receipts.Add(new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.OfficeAndCareer,
                SurfaceKey = PlayerCommandSurfaceKeys.Office,
                SettlementId = jurisdiction.SettlementId,
                CommandName = commandName,
                Label = PlayerCommandService.DetermineOfficeCommandLabel(commandName),
                Summary = jurisdiction.LastAdministrativeTrace,
                OutcomeSummary = jurisdiction.LastPetitionOutcome,
            });
        }

        foreach (CampaignFrontSnapshot campaign in bundle.Campaigns.OrderBy(static entry => entry.CampaignId.Value))
        {
            if (string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)
                && string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace))
            {
                continue;
            }

            receipts.Add(new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.WarfareCampaign,
                SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
                SettlementId = campaign.AnchorSettlementId,
                CommandName = campaign.ActiveDirectiveCode,
                Label = campaign.ActiveDirectiveLabel,
                Summary = campaign.LastDirectiveTrace,
                OutcomeSummary = campaign.ActiveDirectiveSummary,
            });
        }

        receipts.AddRange(BuildPublicLifeReceipts(bundle));
        return receipts
            .GroupBy(static receipt => (
                receipt.ModuleKey,
                receipt.SurfaceKey,
                receipt.SettlementId,
                receipt.ClanId,
                receipt.CommandName,
                receipt.TargetLabel))
            .Select(static group => group.First())
            .ToList();
    }

    private static string RenderMobilizationWindow(string windowLabel)
    {
        return windowLabel switch
        {
            "Open" => "可开",
            "Narrow" => "可守",
            "Preparing" => "待整",
            _ => "已闭",
        };
    }

    private PresentationDebugSnapshot BuildDebugSnapshot(
        GameSimulation simulation,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        SaveRoot saveRoot = simulation.ExportSave();
        InteractionPressureMetricsSnapshot interactionPressure = RuntimeObservabilityCollector.CollectInteractionPressure(simulation);
        SettlementPressureDistributionSnapshot pressureDistribution = RuntimeObservabilityCollector.CollectPressureDistribution(simulation);
        RuntimeScaleMetricsSnapshot scaleMetrics = RuntimeObservabilityCollector.CollectScaleMetrics(simulation, saveRoot, notifications.Count);
        IReadOnlyList<SettlementInteractionHotspotSnapshot> currentHotspots = RuntimeObservabilityCollector.CollectTopHotspots(simulation);
        RuntimePayloadSummarySnapshot payloadSummary = RuntimeObservabilityCollector.CollectPayloadSummary(saveRoot);
        IReadOnlyList<ModulePayloadFootprintSnapshot> topPayloadModules = RuntimeObservabilityCollector.CollectTopPayloadModules(saveRoot);
        List<string> warnings = new();
        List<string> invariants = new();

        try
        {
            ModuleBoundaryValidator.Validate(simulation.Modules, simulation.FeatureManifest, saveRoot);
            invariants.Add("Module boundary validation passed.");
        }
        catch (Exception exception)
        {
            warnings.Add($"Module boundary validation failed: {exception.Message}");
        }

        if (simulation.LastMonthResult is null)
        {
            warnings.Add("No monthly diff has been recorded yet.");
        }
        else
        {
            if (simulation.LastMonthResult.Diff.Entries.Count == 0)
            {
                warnings.Add("The latest simulated month produced no diff entries.");
            }

            if (simulation.LastMonthResult.DomainEvents.Count == 0)
            {
                warnings.Add("The latest simulated month produced no domain events.");
            }
        }

        if (notifications.Count >= NarrativeProjectionModule.NotificationRetentionLimit)
        {
            warnings.Add("Notification history is at the retention limit; older notices may already be trimmed.");
        }

        if (notifications.Any(static notification => notification.Tier == NotificationTier.Urgent))
        {
            warnings.Add("Urgent notifications are pending review.");
        }

        if (interactionPressure.ActivatedResponseSettlements > 0)
        {
            warnings.Add($"{interactionPressure.ActivatedResponseSettlements} settlement response postures are currently activated.");
        }

        if (interactionPressure.OrderInterventionCarryoverSettlements > 0)
        {
            warnings.Add($"{interactionPressure.OrderInterventionCarryoverSettlements} settlements still carry recent order follow-through into the current month.");
        }

        if (interactionPressure.OrderAdministrativeAftermathSettlements > 0)
        {
            warnings.Add($"{interactionPressure.OrderAdministrativeAftermathSettlements} settlements still have office cleanup tied to recent order follow-through.");
        }

        if (interactionPressure.ShieldingDominantSettlements > 0)
        {
            warnings.Add($"{interactionPressure.ShieldingDominantSettlements} settlements currently lean toward protected traffic rather than backlash.");
        }

        if (interactionPressure.BacklashDominantSettlements > 0)
        {
            warnings.Add($"{interactionPressure.BacklashDominantSettlements} settlements currently lean toward suppression backlash rather than protected traffic.");
        }

        if (pressureDistribution.CrisisSettlements > 0)
        {
            warnings.Add($"{pressureDistribution.CrisisSettlements} settlements are currently in crisis-pressure range.");
        }

        if (currentHotspots.Count > 0)
        {
            warnings.Add($"Current hotspot focus: {currentHotspots[0].SettlementName} at score {currentHotspots[0].HotspotScore}.");

            if (currentHotspots[0].InterventionCarryoverMonths > 0)
            {
                warnings.Add($"Hotspot {currentHotspots[0].SettlementName} still has {currentHotspots[0].InterventionCarryoverMonths} month of order carryover active.");
            }

            if (!string.IsNullOrWhiteSpace(currentHotspots[0].AdministrativeAftermathSummary))
            {
                warnings.Add($"Hotspot {currentHotspots[0].SettlementName} also carries office follow-through: {currentHotspots[0].AdministrativeAftermathSummary}");
            }
        }

        if (scaleMetrics.NotificationUtilizationPercent >= 90)
        {
            warnings.Add($"Notification retention utilization is high at {scaleMetrics.NotificationUtilizationPercent}%.");
        }

        if (topPayloadModules.Count > 0)
        {
            warnings.Add($"Largest payload module: {topPayloadModules[0].ModuleKey} at {topPayloadModules[0].PayloadBytes} bytes.");
        }

        if (payloadSummary.TotalModulePayloadBytes > 0 && scaleMetrics.SettlementCount > 0)
        {
            warnings.Add($"Runtime payload density is {scaleMetrics.SavePayloadBytesPerSettlement} bytes per settlement.");
        }

        DebugLoadMigrationSnapshot loadMigration = BuildLoadMigrationSnapshot(simulation);
        if (loadMigration.WasMigrationApplied)
        {
            warnings.Add(loadMigration.Summary);
        }
        warnings.AddRange(loadMigration.Warnings);

        return new PresentationDebugSnapshot
        {
            DiagnosticsSchemaVersion = 1,
            InitialSeed = simulation.KernelState.InitialSeed,
            NotificationRetentionLimit = NarrativeProjectionModule.NotificationRetentionLimit,
            RetentionLimitReached = notifications.Count >= NarrativeProjectionModule.NotificationRetentionLimit,
            LatestMetrics = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = simulation.LastMonthResult?.Diff.Entries.Count ?? 0,
                DomainEventCount = simulation.LastMonthResult?.DomainEvents.Count ?? 0,
                NotificationCount = notifications.Count,
                SavePayloadBytes = _saveCodec.Encode(saveRoot).Length,
            },
            CurrentInteractionPressure = interactionPressure,
            CurrentPressureDistribution = pressureDistribution,
            CurrentScale = scaleMetrics,
            CurrentHotspots = currentHotspots,
            CurrentPayloadSummary = payloadSummary,
            TopPayloadModules = topPayloadModules,
            LoadMigration = loadMigration,
            EnabledModules = simulation.FeatureManifest.GetOrderedEntries()
                .Where(static pair => !string.Equals(pair.Value, "off", StringComparison.Ordinal))
                .Select(static pair => new DebugFeatureModeSnapshot
                {
                    ModuleKey = pair.Key,
                    Mode = pair.Value,
                })
                .ToArray(),
            ModuleInspectors = saveRoot.ModuleStates
                .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
                .Select(static pair => new DebugModuleInspectorSnapshot
                {
                    ModuleKey = pair.Key,
                    ModuleSchemaVersion = pair.Value.ModuleSchemaVersion,
                    PayloadBytes = pair.Value.Payload.Length,
                })
                .ToArray(),
            RecentDiffEntries = simulation.LastMonthResult?.Diff.Entries
                .Select(static entry => new DebugDiffTraceSnapshot
                {
                    ModuleKey = entry.ModuleKey,
                    Description = entry.Description,
                    EntityKey = entry.EntityKey,
                })
                .ToArray() ?? [],
            RecentDomainEvents = simulation.LastMonthResult?.DomainEvents
                .Select(static domainEvent => new DebugDomainEventSnapshot
                {
                    ModuleKey = domainEvent.ModuleKey,
                    EventType = domainEvent.EventType,
                    Summary = domainEvent.Summary,
                })
                .ToArray() ?? [],
            Warnings = warnings.ToArray(),
            Invariants = invariants.ToArray(),
        };
    }

    private static DebugLoadMigrationSnapshot BuildLoadMigrationSnapshot(GameSimulation simulation)
    {
        SaveMigrationReport? report = simulation.LoadMigrationReport;
        if (report is null)
        {
            return new DebugLoadMigrationSnapshot
            {
                LoadOriginLabel = "Bootstrap",
                WasMigrationApplied = false,
                StepCount = 0,
                ConsistencyPassed = true,
                Summary = "Simulation was created from bootstrap state.",
                ConsistencySummary = "Bootstrap path did not require save preparation.",
                Steps = [],
                Warnings = [],
            };
        }

        DebugMigrationStepSnapshot[] steps =
        [
            .. report.RootSteps.Select(static step => new DebugMigrationStepSnapshot
            {
                ScopeLabel = "root",
                SourceVersion = step.SourceVersion,
                TargetVersion = step.TargetVersion,
            }),
            .. report.ModuleSteps.Select(static step => new DebugMigrationStepSnapshot
            {
                ScopeLabel = step.ModuleKey,
                SourceVersion = step.SourceVersion,
                TargetVersion = step.TargetVersion,
            }),
        ];

        string summary = report.WasMigrationApplied
            ? $"Loaded save required {report.AppliedStepCount} migration step(s) before simulation resumed."
            : "Loaded save matched current schemas without migration.";
        string consistencySummary =
            $"{report.PreparedEnabledModuleCount}/{report.SourceEnabledModuleCount} enabled modules, " +
            $"{report.PreparedModuleStateCount}/{report.SourceModuleStateCount} module envelopes preserved.";

        return new DebugLoadMigrationSnapshot
        {
            LoadOriginLabel = "SaveLoad",
            WasMigrationApplied = report.WasMigrationApplied,
            StepCount = report.AppliedStepCount,
            ConsistencyPassed = report.ConsistencyPassed,
            Summary = summary,
            ConsistencySummary = consistencySummary,
            Steps = steps,
            Warnings = report.ConsistencyWarnings,
        };
    }
}
