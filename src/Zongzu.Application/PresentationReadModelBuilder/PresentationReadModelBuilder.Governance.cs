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
        ILookup<int, ClanTradeRouteSnapshot> routesBySettlement = bundle.ClanTradeRoutes
            .ToLookup(static entry => entry.SettlementId.Value);
        ILookup<int, OfficeCareerSnapshot> officeCareersBySettlement = bundle.OfficeCareers
            .ToLookup(static entry => entry.SettlementId.Value);
        ILookup<int, ClanSnapshot> clansBySettlement = bundle.Clans.ToLookup(static entry => entry.HomeSettlementId.Value);

        return bundle.OfficeJurisdictions
            .OrderBy(static jurisdiction => jurisdiction.SettlementId.Value)
            .Select(jurisdiction =>
            {
                publicLifeBySettlement.TryGetValue(jurisdiction.SettlementId.Value, out SettlementPublicLifeSnapshot? publicLife);
                disorderBySettlement.TryGetValue(jurisdiction.SettlementId.Value, out SettlementDisorderSnapshot? disorder);
                ClanSnapshot[] localClans = clansBySettlement[jurisdiction.SettlementId.Value].ToArray();
                ClanTradeRouteSnapshot[] localRoutes = routesBySettlement[jurisdiction.SettlementId.Value].ToArray();
                OfficeCareerSnapshot[] localOfficeCareers = officeCareersBySettlement[jurisdiction.SettlementId.Value].ToArray();
                IReadOnlyList<SocialMemoryEntrySnapshot> localOrderSocialMemories =
                    SelectLocalPublicLifeOrderSocialMemories(bundle.SocialMemories, localClans);
                IReadOnlyList<SocialMemoryEntrySnapshot> localOfficeSocialMemories =
                    SelectLocalOfficePolicySocialMemories(bundle.SocialMemories, localClans);

                string orderAdministrativeAftermathSummary = disorder is null
                    ? string.Empty
                    : BuildOrderAdministrativeAftermathExecutionSummary(disorder, jurisdiction);
                string orderLandingAftermathSummary = disorder is null
                    ? string.Empty
                    : BuildOrderLandingAftermathSummary(disorder);
                string orderResponseAftermathSummary = disorder is null
                    ? string.Empty
                    : BuildOrderResponseAftermathSummary(disorder);
                string officeResponseAftermathSummary = BuildOfficeResponseAftermathSummary(jurisdiction);
                string socialMemoryAftermathSummary = BuildOrderSocialMemoryReadbackSummary(
                    localOrderSocialMemories);
                string orderAftermathSummary = CombineGovernanceDocketText(
                    orderLandingAftermathSummary,
                    orderResponseAftermathSummary,
                    officeResponseAftermathSummary,
                    orderAdministrativeAftermathSummary,
                    socialMemoryAftermathSummary);
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
                string officeImplementationReadback = BuildOfficeImplementationReadbackSummary(jurisdiction, publicLife);
                string officeNextStepReadback = BuildOfficeImplementationNextStepSummary(jurisdiction);
                string regimeOfficeReadback = BuildRegimeOfficeReadbackSummary(localOfficeCareers, jurisdiction);
                string canalRouteReadback = BuildCanalRouteReadbackSummary(publicLife, disorder, localRoutes);
                string residueHealth = BuildResidueHealthSummary(
                    localOrderSocialMemories,
                    localOfficeSocialMemories,
                    officeImplementationReadback,
                    canalRouteReadback);
                string governanceSummary = CombineGovernanceDocketText(
                    BuildGovernanceLaneSummary(
                        focusLabel,
                        leadLabel,
                        jurisdiction,
                        recentOrderCommandLabel,
                        orderAftermathSummary),
                    officeImplementationReadback,
                    regimeOfficeReadback);

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
                    OfficeImplementationReadbackSummary = officeImplementationReadback,
                    OfficeNextStepReadbackSummary = officeNextStepReadback,
                    RegimeOfficeReadbackSummary = regimeOfficeReadback,
                    CanalRouteReadbackSummary = canalRouteReadback,
                    ResidueHealthSummary = residueHealth,
                    GovernanceSummary = governanceSummary,
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

    private static string BuildOfficeImplementationReadbackSummary(
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        string nodeLabel = publicLife?.NodeLabel ?? $"Settlement {jurisdiction.SettlementId.Value}";
        return jurisdiction.PetitionOutcomeCategory switch
        {
            "Stalled" =>
                $"{nodeLabel}县门执行读回：胥吏把持，案牍{jurisdiction.AdministrativeTaskLoad}，积案{jurisdiction.PetitionBacklog}，该回OfficeAndCareer lane押文催县门；本户不能代修。",
            "Delayed" =>
                $"{nodeLabel}县门执行读回：文移拖在案牍，胥吏牵制{jurisdiction.ClerkDependence}，该回OfficeAndCareer lane催办/递报；本户不能代修。",
            "Triaged" =>
                $"{nodeLabel}县门执行读回：纸面落地，实办仍薄，先看OfficeAndCareer lane下月是否真正落地。",
            "Granted" =>
                $"{nodeLabel}县门执行读回：急牍先过，官署这头暂缓，可先冷却观察；县门/文移后账仍由OfficeAndCareer lane读回，路面或宗房后账仍回各owner lane。",
            _ when jurisdiction.AdministrativeTaskLoad >= 55 || jurisdiction.ClerkDependence >= 55 =>
                $"{nodeLabel}县门执行读回：案牍与胥吏仍紧，后账仍需OfficeAndCareer lane读回；本户不能代修。",
            _ => string.Empty,
        };
    }

    private static string BuildOfficeImplementationNextStepSummary(JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Stalled", StringComparison.Ordinal)
            || jurisdiction.ClerkDependence >= 65)
        {
            return "县门/文移后手：押文催县门，必要时改走递报；不要让本户代办官署。";
        }

        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Delayed", StringComparison.Ordinal)
            || jurisdiction.AdministrativeTaskLoad >= 60)
        {
            return "县门/文移后手：轻催文移或改走递报，先看胥吏拖延是否松动。";
        }

        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Triaged", StringComparison.Ordinal))
        {
            return "县门/文移后手：纸面已接，可先冷却一月，再看实办读回。";
        }

        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Granted", StringComparison.Ordinal))
        {
            return "县门/文移后手：官署暂缓，宜冷却观察，不重复催压。";
        }

        return string.Empty;
    }

    private static string BuildOfficeImplementationAffordanceGuidance(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null)
        {
            return string.Empty;
        }

        return CombineGovernanceDocketText(
            BuildOfficeImplementationReadbackSummary(jurisdiction, null),
            BuildOfficeImplementationNextStepSummary(jurisdiction));
    }

    private static string BuildRegimeOfficeReadbackSummary(
        IReadOnlyList<OfficeCareerSnapshot> localOfficeCareers,
        JurisdictionAuthoritySnapshot jurisdiction)
    {
        OfficeCareerSnapshot? riskCareer = localOfficeCareers
            .OrderByDescending(static career => career.OfficialDefectionRisk)
            .ThenBy(static career => career.PersonId.Value)
            .FirstOrDefault();
        int risk = riskCareer?.OfficialDefectionRisk ?? 0;
        if (risk < 55)
        {
            return string.Empty;
        }

        string name = string.IsNullOrWhiteSpace(riskCareer?.DisplayName)
            ? jurisdiction.LeadOfficialName
            : riskCareer.DisplayName;
        return risk >= 80
            ? $"官员摇摆读回：{name}退避风险{risk}，天命与胥吏压力已压到OfficeAndCareer lane；这不是公共家户可修的后账。"
            : $"官员摇摆读回：{name}退避风险{risk}，需继续观察OfficeAndCareer lane。";
    }

    private static string BuildCanalRouteReadbackSummary(
        SettlementPublicLifeSnapshot? publicLife,
        SettlementDisorderSnapshot? disorder,
        IReadOnlyList<ClanTradeRouteSnapshot> localRoutes)
    {
        int activeRoutes = localRoutes.Count(static route => route.IsActive);
        int peakRouteRisk = localRoutes.Count == 0
            ? 0
            : localRoutes.Max(static route => Math.Max(route.Risk, route.SeizureRisk));
        int blockedShipments = localRoutes.Sum(static route => route.BlockedShipmentCount);
        int routePressure = disorder?.RoutePressure ?? 0;
        int roadLag = publicLife?.RoadReportLag ?? 0;

        if (activeRoutes == 0 && peakRouteRisk < 30 && routePressure < 30 && roadLag < 30)
        {
            return string.Empty;
        }

        return $"运河/脚路读回：可见商路{activeRoutes}条，路险{peakRouteRisk}，阻运{blockedShipments}，路压{routePressure}，递报迟滞{roadLag}；只作Trade/Order/PublicLife投影，不新开运河账本。";
    }

    private static string BuildResidueHealthSummary(
        IReadOnlyList<SocialMemoryEntrySnapshot> localOrderSocialMemories,
        IReadOnlyList<SocialMemoryEntrySnapshot> localOfficeSocialMemories,
        string officeImplementationReadback,
        string canalRouteReadback)
    {
        SocialMemoryEntrySnapshot? strongest = localOfficeSocialMemories
            .Concat(localOrderSocialMemories)
            .OrderByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .FirstOrDefault();
        if (strongest is not null)
        {
            return $"余味健康读回：{RenderSocialMemoryTypeLabel(strongest.Type)}{strongest.Weight}仍在，后续由SocialMemoryAndRelations月推进沉淀，不由UI或本户再修。";
        }

        if (!string.IsNullOrWhiteSpace(officeImplementationReadback) || !string.IsNullOrWhiteSpace(canalRouteReadback))
        {
            return "余味健康读回：本月先显示 owner lane 后账，若后续仍有羞面/恐惧/人情/旧怨，再由SocialMemoryAndRelations沉淀。";
        }

        return string.Empty;
    }

    private static IReadOnlyList<SocialMemoryEntrySnapshot> SelectLocalOfficePolicySocialMemories(
        IReadOnlyList<SocialMemoryEntrySnapshot> socialMemories,
        IReadOnlyList<ClanSnapshot> localClans)
    {
        if (socialMemories.Count == 0 || localClans.Count == 0)
        {
            return [];
        }

        HashSet<ClanId> localClanIds = localClans.Select(static clan => clan.Id).ToHashSet();
        return socialMemories
            .Where(memory =>
                memory.State == MemoryLifecycleState.Active
                && memory.CauseKey.StartsWith("office.policy_implementation", StringComparison.Ordinal)
                && memory.SourceClanId.HasValue
                && localClanIds.Contains(memory.SourceClanId.Value))
            .OrderByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .ToArray();
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
            PlayerCommandNames.PressCountyYamenDocument => hasOrderAftermath ? 0 : 2,
            PlayerCommandNames.RedirectRoadReport => hasOrderAftermath ? 1 : 2,
            PlayerCommandNames.RepairLocalWatchGuarantee => hasOrderAftermath ? 2 : 5,
            PlayerCommandNames.CompensateRunnerMisread => hasOrderAftermath ? 2 : 5,
            PlayerCommandNames.DeferHardPressure => hasOrderAftermath ? 2 : 5,
            PlayerCommandNames.AskClanEldersExplain => hasOrderAftermath ? 3 : 6,
            PlayerCommandNames.PostCountyNotice => hasOrderAftermath ? 0 : 1,
            PlayerCommandNames.DispatchRoadReport => 0,
            PlayerCommandNames.EscortRoadReport => 4,
            PlayerCommandNames.FundLocalWatch => 5,
            PlayerCommandNames.InviteClanEldersPubliclyBroker => 6,
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
            OfficeImplementationReadbackSummary = lead.OfficeImplementationReadbackSummary,
            OfficeNextStepReadbackSummary = lead.OfficeNextStepReadbackSummary,
            RegimeOfficeReadbackSummary = lead.RegimeOfficeReadbackSummary,
            CanalRouteReadbackSummary = lead.CanalRouteReadbackSummary,
            ResidueHealthSummary = lead.ResidueHealthSummary,
            SuggestedCommandName = lead.SuggestedCommandName,
            SuggestedCommandLabel = lead.SuggestedCommandLabel,
            SuggestedCommandPrompt = lead.SuggestedCommandPrompt,
        };
    }

    private static GovernanceDocketSnapshot BuildGovernanceDocket(
        GovernanceFocusSnapshot focus,
        IReadOnlyList<SettlementGovernanceLaneSnapshot> governanceSettlements,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications,
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        IReadOnlyList<HouseholdPressureSnapshot> households,
        IReadOnlyList<JurisdictionAuthoritySnapshot> jurisdictions,
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyList<SocialMemoryEntrySnapshot> socialMemories)
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
        JurisdictionAuthoritySnapshot? jurisdiction = jurisdictions
            .FirstOrDefault(entry => entry.SettlementId == focus.SettlementId);
        HouseholdPressureSnapshot? ownerLaneReturnHousehold =
            SelectRecentLocalResponseHouseholdForSettlement(households, focus.SettlementId);
        ClanSnapshot[] localClans = clans
            .Where(clan => clan.HomeSettlementId == focus.SettlementId)
            .OrderByDescending(static clan => clan.Prestige)
            .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
            .ToArray();
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories =
            SelectLocalPublicLifeOrderSocialMemories(socialMemories, localClans);
        string ownerLaneReturnGuidance = JoinOwnerLaneReturnSurfaceText(
            BuildOfficeOwnerLaneReturnSurfaceGuidance(ownerLaneReturnHousehold),
            BuildOfficeOwnerLaneReturnStatusGuidance(ownerLaneReturnHousehold, jurisdiction, localSocialMemories));

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
            lane?.OfficeImplementationReadbackSummary ?? string.Empty,
            lane?.RegimeOfficeReadbackSummary ?? string.Empty,
            lane?.CanalRouteReadbackSummary ?? string.Empty,
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
            ownerLaneReturnGuidance,
            lane?.OfficeNextStepReadbackSummary ?? string.Empty,
            lane?.ResidueHealthSummary ?? string.Empty,
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
            OfficeImplementationReadbackSummary = lane?.OfficeImplementationReadbackSummary ?? string.Empty,
            OfficeNextStepReadbackSummary = lane?.OfficeNextStepReadbackSummary ?? string.Empty,
            RegimeOfficeReadbackSummary = lane?.RegimeOfficeReadbackSummary ?? string.Empty,
            CanalRouteReadbackSummary = lane?.CanalRouteReadbackSummary ?? string.Empty,
            ResidueHealthSummary = lane?.ResidueHealthSummary ?? string.Empty,
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

        if (!string.IsNullOrWhiteSpace(settlement.OfficeImplementationReadbackSummary))
        {
            score += 16;
        }

        if (!string.IsNullOrWhiteSpace(settlement.RegimeOfficeReadbackSummary))
        {
            score += 12;
        }

        if (!string.IsNullOrWhiteSpace(settlement.ResidueHealthSummary))
        {
            score += 8;
        }

        return score;
    }

}
