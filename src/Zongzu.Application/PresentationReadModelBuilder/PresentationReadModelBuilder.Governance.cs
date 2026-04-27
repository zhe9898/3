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
        Dictionary<int, CampaignFrontSnapshot> campaignsBySettlement = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.IsActive)
            .ThenByDescending(static campaign => campaign.FrontPressure)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .GroupBy(static campaign => campaign.AnchorSettlementId.Value)
            .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<int, AftermathDocketSnapshot> aftermathDocketsByCampaign = bundle.CampaignAftermathDockets
            .OrderBy(static docket => docket.CampaignId.Value)
            .GroupBy(static docket => docket.CampaignId.Value)
            .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<int, CampaignMobilizationSignalSnapshot> campaignSignalsBySettlement = IndexFirstBySettlement(
            bundle.CampaignMobilizationSignals,
            static entry => entry.SettlementId);

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
                IReadOnlyList<SocialMemoryEntrySnapshot> localCampaignSocialMemories =
                    SelectLocalCampaignSocialMemories(bundle.SocialMemories, localClans);
                campaignsBySettlement.TryGetValue(jurisdiction.SettlementId.Value, out CampaignFrontSnapshot? campaign);
                campaignSignalsBySettlement.TryGetValue(jurisdiction.SettlementId.Value, out CampaignMobilizationSignalSnapshot? signal);
                HouseholdPressureSnapshot? familyLaneHousehold =
                    SelectRecentLocalResponseHouseholdForSettlement(bundle.Households, jurisdiction.SettlementId);
                ClanSnapshot? familyLaneClan = SelectFamilyLaneClosureClan(localClans);
                FamilyLaneClosureReadback familyLaneClosure = BuildFamilyLaneClosureReadback(
                    familyLaneHousehold,
                    familyLaneClan,
                    localOrderSocialMemories);
                WarfareLaneClosureReadback warfareLaneClosure = BuildWarfareLaneClosureReadback(
                    signal,
                    campaign,
                    campaign is not null && aftermathDocketsByCampaign.TryGetValue(campaign.CampaignId.Value, out AftermathDocketSnapshot? aftermathDocket)
                        ? aftermathDocket
                        : null,
                    jurisdiction,
                    localCampaignSocialMemories);

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
                string officeImplementationReadback = BuildOfficeImplementationReadbackSummary(jurisdiction, publicLife);
                string officeNextStepReadback = BuildOfficeImplementationNextStepSummary(jurisdiction);
                string regimeOfficeReadback = BuildRegimeOfficeReadbackSummary(localOfficeCareers, jurisdiction);
                string officeLaneEntryReadback = BuildOfficeLaneEntryReadbackSummary(jurisdiction);
                string officeLaneReceiptClosure = BuildOfficeLaneReceiptClosureSummary(jurisdiction);
                string officeLaneResidueFollowUp =
                    BuildOfficeLaneResidueFollowUpSummary(localOfficeSocialMemories, jurisdiction, publicLife);
                string officeLaneNoLoopGuard = BuildOfficeLaneNoLoopGuardSummary(
                    jurisdiction,
                    localOfficeCareers,
                    localOfficeSocialMemories);
                string courtPolicyEntryReadback = BuildCourtPolicyEntryReadbackSummary(jurisdiction, publicLife);
                string courtPolicyDispatchReadback = BuildCourtPolicyDispatchReadbackSummary(jurisdiction, publicLife);
                string courtPolicyPublicReadback = CombineGovernanceDocketText(
                    BuildCourtPolicyPublicReadbackSummary(jurisdiction, publicLife),
                    BuildCourtPolicyPublicReadingEchoGuidance(localOfficeSocialMemories, jurisdiction, publicLife));
                string courtPolicyNoLoopGuard = BuildCourtPolicyNoLoopGuardSummary(jurisdiction, publicLife);
                bool hasCourtPolicyProcess = !string.IsNullOrWhiteSpace(courtPolicyEntryReadback);
                PlayerCommandAffordanceSnapshot? suggestedAffordance = SelectPrimaryGovernanceAffordance(
                    bundle.PlayerCommands.Affordances,
                    jurisdiction.SettlementId,
                    hasOrderAftermath,
                    hasCourtPolicyProcess);
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
                    courtPolicyEntryReadback,
                    courtPolicyDispatchReadback,
                    courtPolicyPublicReadback,
                    courtPolicyNoLoopGuard,
                    officeImplementationReadback,
                    officeLaneReceiptClosure,
                    familyLaneClosure.ReceiptClosureSummary,
                    familyLaneClosure.NoLoopGuardSummary,
                    warfareLaneClosure.CampaignAftermathReadbackSummary,
                    warfareLaneClosure.NoLoopGuardSummary,
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
                    OfficeLaneEntryReadbackSummary = officeLaneEntryReadback,
                    OfficeLaneReceiptClosureSummary = officeLaneReceiptClosure,
                    OfficeLaneResidueFollowUpSummary = officeLaneResidueFollowUp,
                    OfficeLaneNoLoopGuardSummary = officeLaneNoLoopGuard,
                    CourtPolicyEntryReadbackSummary = courtPolicyEntryReadback,
                    CourtPolicyDispatchReadbackSummary = courtPolicyDispatchReadback,
                    CourtPolicyPublicReadbackSummary = courtPolicyPublicReadback,
                    CourtPolicyNoLoopGuardSummary = courtPolicyNoLoopGuard,
                    FamilyLaneEntryReadbackSummary = familyLaneClosure.EntryReadbackSummary,
                    FamilyElderExplanationReadbackSummary = familyLaneClosure.ElderExplanationReadbackSummary,
                    FamilyGuaranteeReadbackSummary = familyLaneClosure.GuaranteeReadbackSummary,
                    FamilyHouseFaceReadbackSummary = familyLaneClosure.HouseFaceReadbackSummary,
                    FamilyLaneReceiptClosureSummary = familyLaneClosure.ReceiptClosureSummary,
                    FamilyLaneResidueFollowUpSummary = familyLaneClosure.ResidueFollowUpSummary,
                    FamilyLaneNoLoopGuardSummary = familyLaneClosure.NoLoopGuardSummary,
                    WarfareLaneEntryReadbackSummary = warfareLaneClosure.EntryReadbackSummary,
                    ForceReadinessReadbackSummary = warfareLaneClosure.ForceReadinessReadbackSummary,
                    CampaignAftermathReadbackSummary = warfareLaneClosure.CampaignAftermathReadbackSummary,
                    WarfareLaneReceiptClosureSummary = warfareLaneClosure.ReceiptClosureSummary,
                    WarfareLaneResidueFollowUpSummary = warfareLaneClosure.ResidueFollowUpSummary,
                    WarfareLaneNoLoopGuardSummary = warfareLaneClosure.NoLoopGuardSummary,
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
            BuildOfficeImplementationNextStepSummary(jurisdiction),
            BuildOfficeLaneEntryReadbackSummary(jurisdiction),
            BuildOfficeLaneReceiptClosureSummary(jurisdiction),
            BuildOfficeLaneNoLoopGuardSummary(jurisdiction, [], []));
    }

    private static string BuildOfficeLaneEntryReadbackSummary(JurisdictionAuthoritySnapshot jurisdiction)
    {
        string leadLabel = ResolveOfficeLeadLabel(jurisdiction);
        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Stalled", StringComparison.Ordinal)
            || jurisdiction.ClerkDependence >= 65)
        {
            return $"Office承接入口：该走县门/文移 lane（OfficeAndCareer）：{leadLabel}眼下先押文催县门；若胥吏续拖，再改走递报。本户不能代修。";
        }

        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Delayed", StringComparison.Ordinal)
            || jurisdiction.AdministrativeTaskLoad >= 60)
        {
            return $"Office承接入口：该走县门/文移 lane（OfficeAndCareer）：{leadLabel}可轻催文移或改走递报，先看县门是否真正落地；本户不能代修。";
        }

        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Triaged", StringComparison.Ordinal))
        {
            return $"Office承接入口：该走县门/文移 lane（OfficeAndCareer）：{leadLabel}纸面已接，先冷却观察一月，看实办读回；本户不能代修。";
        }

        if (string.Equals(jurisdiction.PetitionOutcomeCategory, "Granted", StringComparison.Ordinal))
        {
            return $"Office承接入口：该走县门/文移 lane（OfficeAndCareer）：{leadLabel}官署暂缓，宜冷却观察，不把已缓后账回压本户。";
        }

        if (jurisdiction.PetitionBacklog > 0 || jurisdiction.PetitionPressure > 0)
        {
            return $"Office承接入口：该走县门/文移 lane（OfficeAndCareer）：{leadLabel}仍有积案{jurisdiction.PetitionBacklog}、词牍之压{jurisdiction.PetitionPressure}，先看批阅词状或发签催办；本户不能代修。";
        }

        return string.Empty;
    }

    private static string BuildOfficeLaneReceiptClosureSummary(JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (!HasStructuredOfficeOwnerLaneResponse(jurisdiction))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            jurisdiction.LastRefusalResponseCommandLabel,
            jurisdiction.LastRefusalResponseCommandCode);
        string leadLabel = ResolveOfficeLeadLabel(jurisdiction);
        string tail = $"积案{jurisdiction.PetitionBacklog}，胥吏牵制{jurisdiction.ClerkDependence}。";

        return jurisdiction.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired =>
                $"Office后手收口读回：已收口：{commandLabel}已让{leadLabel}把县门/文移后账接住，先停本户加压；{tail}",
            PublicLifeOrderResponseOutcomeCodes.Contained =>
                $"Office后手收口读回：仍拖：{commandLabel}只把后账暂压在Office lane，仍看县门/文移下月读回；{tail}",
            PublicLifeOrderResponseOutcomeCodes.Escalated =>
                $"Office后手收口读回：转硬：{commandLabel}被胥吏拖成新积案，先换Office-lane办法，不回压本户；{tail}",
            PublicLifeOrderResponseOutcomeCodes.Ignored =>
                $"Office后手收口读回：放置：{commandLabel}未接住县门/文移后账，仍等Office承接口；本户不能代修；{tail}",
            _ => string.Empty,
        };
    }

    private static string BuildOfficeLaneResidueFollowUpSummary(
        IReadOnlyList<SocialMemoryEntrySnapshot> localOfficeSocialMemories,
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        SocialMemoryEntrySnapshot? residue = SelectOfficePolicyResidue(localOfficeSocialMemories, jurisdiction);
        if (residue is null)
        {
            return string.Empty;
        }

        if (TryReadOfficePolicyLocalResponseResidueCause(residue.CauseKey, out OfficePolicyLocalResponseResidueCause localResponseCause))
        {
            string commandLabel = localResponseCause.CommandCode switch
            {
                PlayerCommandNames.PressCountyYamenDocument => "县门轻催",
                PlayerCommandNames.RedirectRoadReport => "递报改道",
                _ => localResponseCause.CommandCode,
            };
            string responseFollowUp = localResponseCause.OutcomeCode switch
            {
                PublicLifeOrderResponseOutcomeCodes.Contained => "续接提示：政策回应余味已暂压，仍看Office/PublicLife下月是否接稳。",
                PublicLifeOrderResponseOutcomeCodes.Escalated => "换招提示：政策回应余味转硬，先换Office-lane办法，不从本户硬补。",
                PublicLifeOrderResponseOutcomeCodes.Ignored => "冷却提示：政策回应余味被放置，先停重复催压。",
                PublicLifeOrderResponseOutcomeCodes.Repaired => "冷却提示：政策回应转稳，先等公议和县门读回。",
                _ => "续接提示：政策回应余味仍看Office/PublicLife后续月读回。",
            };

            string memoryPressureReadback = BuildCourtPolicyMemoryPressureReadbackSummary(
                residue,
                localResponseCause,
                jurisdiction,
                publicLife);
            return CombineGovernanceDocketText(
                $"政策回应余味续接读回：{commandLabel}留下{RenderSocialMemoryTypeLabel(residue.Type)}{residue.Weight}；{responseFollowUp} 仍由SocialMemoryAndRelations后续月沉淀，不是本户硬扛朝廷后账。",
                memoryPressureReadback);
        }

        string category = ReadOfficePolicyResidueCategory(residue.CauseKey);
        string followUp = category switch
        {
            "Stalled" => "换招提示：胥吏把持余味仍硬，先换Office-lane办法或等更强承接口。",
            "Delayed" => "续接提示：文移拖延余味未平，可轻催文移，但别从本户硬补。",
            "Triaged" => "冷却提示：纸面已接，先冷却观察，等实办读回。",
            "Granted" => "冷却提示：官署暂缓，先停重复催压。",
            _ => "续接提示：仍看Office lane后续月读回。",
        };

        return $"Office余味续接读回：{RenderSocialMemoryTypeLabel(residue.Type)}{residue.Weight}仍在；{followUp} 仍由SocialMemoryAndRelations后续月沉淀，不是本户再修。";
    }

    private static string BuildCourtPolicyMemoryPressureReadbackSummary(
        SocialMemoryEntrySnapshot residue,
        OfficePolicyLocalResponseResidueCause localResponseCause,
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        if (!HasCourtPolicyProcessReadback(jurisdiction, publicLife))
        {
            return string.Empty;
        }

        string traceLabel = RenderCourtPolicyLocalResponseTraceLabel(localResponseCause.TraceCode);
        string outcomeLabel = RenderCourtPolicyLocalResponseOutcomeLabel(localResponseCause.OutcomeCode);
        int noticeVisibility = publicLife?.NoticeVisibility ?? 0;
        int streetTalkHeat = publicLife?.StreetTalkHeat ?? 0;
        int publicLegitimacy = publicLife?.PublicLegitimacy ?? 0;
        return $"政策旧账回压读回：旧文移余味（{traceLabel}、{outcomeLabel}、{RenderSocialMemoryTypeLabel(residue.Type)}{residue.Weight}）进入下一次政策窗口读法；公议旧读法续压：榜示{noticeVisibility}、街谈{streetTalkHeat}、公议{publicLegitimacy}；仍由Office/PublicLife/SocialMemory分读，不是本户硬扛朝廷旧账。";
    }

    private static string RenderCourtPolicyLocalResponseTraceLabel(string traceCode)
    {
        return traceCode switch
        {
            PublicLifeOrderResponseTraceCodes.OfficeYamenLanded => "县门承接旧痕",
            PublicLifeOrderResponseTraceCodes.OfficeReportRerouted => "递报改道旧痕",
            _ => "文移旧痕",
        };
    }

    private static string RenderCourtPolicyLocalResponseOutcomeLabel(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "已转稳",
            PublicLifeOrderResponseOutcomeCodes.Contained => "暂压",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "转硬",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "放置",
            _ => "未定",
        };
    }

    private static string BuildOfficeLaneNoLoopGuardSummary(
        JurisdictionAuthoritySnapshot jurisdiction,
        IReadOnlyList<OfficeCareerSnapshot> localOfficeCareers,
        IReadOnlyList<SocialMemoryEntrySnapshot> localOfficeSocialMemories)
    {
        bool hasOfficeResponse = HasStructuredOfficeOwnerLaneResponse(jurisdiction);
        bool hasOfficePolicyResidue = SelectOfficePolicyResidue(localOfficeSocialMemories, jurisdiction) is not null;
        bool hasOfficeImplementation = IsOfficeImplementationCategory(jurisdiction.PetitionOutcomeCategory)
            || jurisdiction.AdministrativeTaskLoad >= 55
            || jurisdiction.ClerkDependence >= 55;
        bool hasOfficialWavering = localOfficeCareers.Any(static career => career.OfficialDefectionRisk >= 55);
        if (!hasOfficeResponse && !hasOfficePolicyResidue && !hasOfficeImplementation && !hasOfficialWavering)
        {
            return string.Empty;
        }

        string waveringTail = hasOfficialWavering
            ? " 官员摇摆、县门脸面与胥吏拖延也留在OfficeAndCareer lane。"
            : string.Empty;
        return $"Office闭环防回压：县门/文移/胥吏后账留在OfficeAndCareer lane；本户不再代修，不把Office后手回压本户。{waveringTail}".Trim();
    }

    private static string BuildCourtPolicyEntryReadbackSummary(
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        if (!HasCourtPolicyProcessReadback(jurisdiction, publicLife))
        {
            return string.Empty;
        }

        string nodeLabel = publicLife?.NodeLabel ?? $"Settlement {jurisdiction.SettlementId.Value}";
        string policyTone = ResolveCourtPolicyToneReadback(jurisdiction);
        return $"朝议压力读回：{nodeLabel}已有政策窗口读回；政策语气读回：{policyTone}，官阶{jurisdiction.AuthorityTier}、词牍压{jurisdiction.PetitionPressure}、乡面杠力{jurisdiction.JurisdictionLeverage}；朝廷后手仍不直写地方，先由OfficeAndCareer开窗承接。";
    }

    private static string BuildCourtPolicyDispatchReadbackSummary(
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        if (!HasCourtPolicyProcessReadback(jurisdiction, publicLife))
        {
            return string.Empty;
        }

        int dispatchPressure = publicLife?.PrefectureDispatchPressure ?? 0;
        int documentaryWeight = publicLife?.DocumentaryWeight ?? 0;
        int verificationCost = publicLife?.VerificationCost ?? 0;
        int courierRisk = publicLife?.CourierRisk ?? 0;
        string documentDirection = ResolveCourtPolicyDocumentDirectionReadback(publicLife, jurisdiction);
        string countyPosture = ResolveCourtPolicyCountyPostureReadback(jurisdiction);
        return $"文移到达读回：文移指向读回：{documentDirection}；县门承接姿态：{countyPosture}；州县催牒压{dispatchPressure}，文书重{documentaryWeight}，验看成本{verificationCost}，递送风险{courierRisk}；县门执行承接读回仍归OfficeAndCareer，公议读法另由PublicLifeAndRumor读。";
    }

    private static string BuildCourtPolicyPublicReadbackSummary(
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        if (!HasCourtPolicyProcessReadback(jurisdiction, publicLife))
        {
            return string.Empty;
        }

        int noticeVisibility = publicLife?.NoticeVisibility ?? 0;
        int streetTalkHeat = publicLife?.StreetTalkHeat ?? 0;
        int publicLegitimacy = publicLife?.PublicLegitimacy ?? 0;
        string publicReading = ResolveCourtPolicyPublicReadingReadback(publicLife);
        return $"公议读法读回：公议承压读法：{publicReading}；榜示{noticeVisibility}，街谈{streetTalkHeat}，公议{publicLegitimacy}；Office/PublicLife分读：县门是否落地看OfficeAndCareer，街面怎么传看PublicLifeAndRumor。";
    }

    private static string BuildCourtPolicyNoLoopGuardSummary(
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        if (!HasCourtPolicyProcessReadback(jurisdiction, publicLife))
        {
            return string.Empty;
        }

        return "Court-policy防回压：朝廷后手仍不直写地方；不是本户硬扛朝廷后账，也不是县门独吞朝廷后账；Office/PublicLife分读，只显示已投影字段。";
    }

    private static string ResolveCourtPolicyToneReadback(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return jurisdiction.PetitionOutcomeCategory switch
        {
            "Stalled" => "催意偏硬，县门实办被胥吏截留",
            "Delayed" => "催意已到县门，文移仍拖在案牍",
            "Triaged" => "榜文先行，纸面先承接实办仍薄",
            "Granted" => "急牍先过，官署暂能接住",
            _ when jurisdiction.PetitionPressure >= 60 => "词牍压力偏硬，县门需先分轻重",
            _ when jurisdiction.AdministrativeTaskLoad >= 60 => "案牍压力偏厚，文移先压执行口",
            _ => "朝议压力转为县门可读的政策窗口",
        };
    }

    private static string ResolveCourtPolicyDocumentDirectionReadback(
        SettlementPublicLifeSnapshot? publicLife,
        JurisdictionAuthoritySnapshot jurisdiction)
    {
        int dispatchPressure = publicLife?.PrefectureDispatchPressure ?? 0;
        int documentaryWeight = publicLife?.DocumentaryWeight ?? 0;

        if (dispatchPressure >= 12 || documentaryWeight >= 35)
        {
            return "州县催牒已压到榜示与递报口";
        }

        if (jurisdiction.PetitionBacklog >= 12 || jurisdiction.AdministrativeTaskLoad >= 60)
        {
            return "文移先指向县门积案和承办次序";
        }

        return "文移先把朝议压力交给县门承接";
    }

    private static string ResolveCourtPolicyCountyPostureReadback(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return jurisdiction.PetitionOutcomeCategory switch
        {
            "Stalled" => "胥吏截留，承接姿态偏硬且实办不清",
            "Delayed" => "县门接牒但拖延，仍需Office后续读回",
            "Triaged" => "纸面接住，实办薄，先冷却再看",
            "Granted" => "急牍先过，官署暂缓",
            _ when jurisdiction.ClerkDependence >= 60 => "胥吏牵制重，县门承接不稳",
            _ when jurisdiction.AdministrativeTaskLoad >= 60 => "案牍偏重，承接先慢后看",
            _ => "县门可承接，但结果仍看OfficeAndCareer",
        };
    }

    private static string ResolveCourtPolicyPublicReadingReadback(SettlementPublicLifeSnapshot? publicLife)
    {
        if (publicLife is null)
        {
            return "PublicLifeAndRumor尚无街面投影，只能等待榜示与街谈读回";
        }

        if (publicLife.PrefectureDispatchPressure >= 14 || publicLife.StreetTalkHeat >= 70)
        {
            return "街面把它读成州县催办已压近县门";
        }

        if (publicLife.PublicLegitimacy < 45 && publicLife.NoticeVisibility >= 35)
        {
            return "榜示可见但公信偏虚，街谈仍会追问实办";
        }

        if (publicLife.NoticeVisibility >= 35)
        {
            return "榜示先被看见，公议仍等县门实办";
        }

        return "公议还在探听，尚未把后账压回本户";
    }

    private static bool HasCourtPolicyProcessReadback(
        JurisdictionAuthoritySnapshot jurisdiction,
        SettlementPublicLifeSnapshot? publicLife)
    {
        return IsOfficeImplementationCategory(jurisdiction.PetitionOutcomeCategory)
            || jurisdiction.PetitionPressure >= 45
            || jurisdiction.AdministrativeTaskLoad >= 55
            || jurisdiction.PetitionBacklog >= 8
            || (publicLife?.PrefectureDispatchPressure ?? 0) > 0
            || (publicLife?.DocumentaryWeight ?? 0) >= 30
            || (publicLife?.VerificationCost ?? 0) >= 30
            || (publicLife?.CourierRisk ?? 0) >= 30;
    }

    private static SocialMemoryEntrySnapshot? SelectOfficePolicyResidue(
        IReadOnlyList<SocialMemoryEntrySnapshot> localOfficeSocialMemories,
        JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (localOfficeSocialMemories.Count == 0)
        {
            return null;
        }

        string settlementMarker = $".{jurisdiction.SettlementId.Value}.";
        bool preferLocalResponseResidue = HasStructuredOfficeOwnerLaneResponse(jurisdiction);
        return localOfficeSocialMemories
            .Where(static memory => memory.State == MemoryLifecycleState.Active)
            .Where(memory =>
                (memory.CauseKey.StartsWith("office.policy_implementation.", StringComparison.Ordinal)
                 || memory.CauseKey.StartsWith("office.policy_local_response.", StringComparison.Ordinal))
                && memory.CauseKey.Contains(settlementMarker, StringComparison.Ordinal))
            .OrderByDescending(memory =>
                preferLocalResponseResidue
                && memory.CauseKey.StartsWith("office.policy_local_response.", StringComparison.Ordinal))
            .ThenByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .FirstOrDefault();
    }

    private static string ReadOfficePolicyResidueCategory(string causeKey)
    {
        string[] parts = causeKey.Split('.');
        return parts.Length >= 4 ? parts[3] : string.Empty;
    }

    private static bool TryReadOfficePolicyLocalResponseResidueCause(
        string causeKey,
        out OfficePolicyLocalResponseResidueCause cause)
    {
        cause = default;
        if (!causeKey.StartsWith("office.policy_local_response.", StringComparison.Ordinal))
        {
            return false;
        }

        string[] parts = causeKey.Split('.');
        if (parts.Length < 6)
        {
            return false;
        }

        cause = new OfficePolicyLocalResponseResidueCause(
            SettlementId: parts[2],
            CommandCode: parts[3],
            OutcomeCode: parts[4],
            TraceCode: string.Join(".", parts.Skip(5)));
        return true;
    }

    private static bool IsOfficeImplementationCategory(string category)
    {
        return string.Equals(category, "Stalled", StringComparison.Ordinal)
            || string.Equals(category, "Delayed", StringComparison.Ordinal)
            || string.Equals(category, "Triaged", StringComparison.Ordinal)
            || string.Equals(category, "Granted", StringComparison.Ordinal);
    }

    private static string ResolveOfficeLeadLabel(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
            ? jurisdiction.LeadOfficeTitle
            : jurisdiction.LeadOfficialName;
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
                && (memory.CauseKey.StartsWith("office.policy_implementation", StringComparison.Ordinal)
                    || memory.CauseKey.StartsWith("office.policy_local_response", StringComparison.Ordinal))
                && memory.SourceClanId.HasValue
                && localClanIds.Contains(memory.SourceClanId.Value))
            .OrderByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .ToArray();
    }

    private readonly record struct OfficePolicyLocalResponseResidueCause(
        string SettlementId,
        string CommandCode,
        string OutcomeCode,
        string TraceCode);

    private static ClanSnapshot? SelectFamilyLaneClosureClan(IReadOnlyList<ClanSnapshot> localClans)
    {
        if (localClans.Count == 0)
        {
            return null;
        }

        return localClans
                .Where(HasStructuredFamilyOwnerLaneResponse)
                .OrderByDescending(static clan => clan.ResponseCarryoverMonths)
                .ThenByDescending(static clan => clan.Prestige)
                .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
                .FirstOrDefault()
            ?? localClans
                .OrderByDescending(static clan => clan.Prestige)
                .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
                .First();
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
        bool hasOrderAftermath,
        bool hasCourtPolicyProcess)
    {
        return EnumerateAffordancesForSurface(affordances, PlayerCommandSurfaceKeys.PublicLife, settlementId)
            .OrderBy(command => GetGovernanceAffordancePriority(command.CommandName, hasOrderAftermath, hasCourtPolicyProcess))
            .ThenBy(command => command.CommandName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static int GetGovernanceAffordancePriority(
        string commandName,
        bool hasOrderAftermath,
        bool hasCourtPolicyProcess)
    {
        return commandName switch
        {
            PlayerCommandNames.PressCountyYamenDocument => hasOrderAftermath || hasCourtPolicyProcess ? 0 : 2,
            PlayerCommandNames.RedirectRoadReport => hasOrderAftermath ? 1 : hasCourtPolicyProcess ? 1 : 2,
            PlayerCommandNames.RepairLocalWatchGuarantee => hasOrderAftermath ? 2 : 5,
            PlayerCommandNames.CompensateRunnerMisread => hasOrderAftermath ? 2 : 5,
            PlayerCommandNames.DeferHardPressure => hasOrderAftermath ? 2 : 5,
            PlayerCommandNames.AskClanEldersExplain => hasOrderAftermath ? 3 : 6,
            PlayerCommandNames.PostCountyNotice => hasOrderAftermath ? 0 : hasCourtPolicyProcess ? 2 : 1,
            PlayerCommandNames.DispatchRoadReport => hasCourtPolicyProcess && !hasOrderAftermath ? 3 : 0,
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
            OfficeLaneEntryReadbackSummary = lead.OfficeLaneEntryReadbackSummary,
            OfficeLaneReceiptClosureSummary = lead.OfficeLaneReceiptClosureSummary,
            OfficeLaneResidueFollowUpSummary = lead.OfficeLaneResidueFollowUpSummary,
            OfficeLaneNoLoopGuardSummary = lead.OfficeLaneNoLoopGuardSummary,
            CourtPolicyEntryReadbackSummary = lead.CourtPolicyEntryReadbackSummary,
            CourtPolicyDispatchReadbackSummary = lead.CourtPolicyDispatchReadbackSummary,
            CourtPolicyPublicReadbackSummary = lead.CourtPolicyPublicReadbackSummary,
            CourtPolicyNoLoopGuardSummary = lead.CourtPolicyNoLoopGuardSummary,
            FamilyLaneEntryReadbackSummary = lead.FamilyLaneEntryReadbackSummary,
            FamilyElderExplanationReadbackSummary = lead.FamilyElderExplanationReadbackSummary,
            FamilyGuaranteeReadbackSummary = lead.FamilyGuaranteeReadbackSummary,
            FamilyHouseFaceReadbackSummary = lead.FamilyHouseFaceReadbackSummary,
            FamilyLaneReceiptClosureSummary = lead.FamilyLaneReceiptClosureSummary,
            FamilyLaneResidueFollowUpSummary = lead.FamilyLaneResidueFollowUpSummary,
            FamilyLaneNoLoopGuardSummary = lead.FamilyLaneNoLoopGuardSummary,
            WarfareLaneEntryReadbackSummary = lead.WarfareLaneEntryReadbackSummary,
            ForceReadinessReadbackSummary = lead.ForceReadinessReadbackSummary,
            CampaignAftermathReadbackSummary = lead.CampaignAftermathReadbackSummary,
            WarfareLaneReceiptClosureSummary = lead.WarfareLaneReceiptClosureSummary,
            WarfareLaneResidueFollowUpSummary = lead.WarfareLaneResidueFollowUpSummary,
            WarfareLaneNoLoopGuardSummary = lead.WarfareLaneNoLoopGuardSummary,
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
            lane?.CourtPolicyEntryReadbackSummary ?? string.Empty,
            lane?.CourtPolicyDispatchReadbackSummary ?? string.Empty,
            lane?.CourtPolicyPublicReadbackSummary ?? string.Empty,
            lane?.OfficeImplementationReadbackSummary ?? string.Empty,
            lane?.OfficeLaneReceiptClosureSummary ?? string.Empty,
            lane?.CampaignAftermathReadbackSummary ?? string.Empty,
            lane?.WarfareLaneNoLoopGuardSummary ?? string.Empty,
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
            lane?.CourtPolicyEntryReadbackSummary ?? string.Empty,
            lane?.CourtPolicyDispatchReadbackSummary ?? string.Empty,
            lane?.CourtPolicyPublicReadbackSummary ?? string.Empty,
            lane?.CourtPolicyNoLoopGuardSummary ?? string.Empty,
            lane?.OfficeLaneEntryReadbackSummary ?? string.Empty,
            lane?.OfficeLaneReceiptClosureSummary ?? string.Empty,
            lane?.OfficeNextStepReadbackSummary ?? string.Empty,
            lane?.OfficeLaneResidueFollowUpSummary ?? string.Empty,
            lane?.OfficeLaneNoLoopGuardSummary ?? string.Empty,
            lane?.FamilyLaneEntryReadbackSummary ?? string.Empty,
            lane?.FamilyElderExplanationReadbackSummary ?? string.Empty,
            lane?.FamilyGuaranteeReadbackSummary ?? string.Empty,
            lane?.FamilyHouseFaceReadbackSummary ?? string.Empty,
            lane?.FamilyLaneReceiptClosureSummary ?? string.Empty,
            lane?.FamilyLaneResidueFollowUpSummary ?? string.Empty,
            lane?.FamilyLaneNoLoopGuardSummary ?? string.Empty,
            lane?.WarfareLaneEntryReadbackSummary ?? string.Empty,
            lane?.ForceReadinessReadbackSummary ?? string.Empty,
            lane?.CampaignAftermathReadbackSummary ?? string.Empty,
            lane?.WarfareLaneReceiptClosureSummary ?? string.Empty,
            lane?.WarfareLaneResidueFollowUpSummary ?? string.Empty,
            lane?.WarfareLaneNoLoopGuardSummary ?? string.Empty,
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
            OfficeLaneEntryReadbackSummary = lane?.OfficeLaneEntryReadbackSummary ?? string.Empty,
            OfficeLaneReceiptClosureSummary = lane?.OfficeLaneReceiptClosureSummary ?? string.Empty,
            OfficeLaneResidueFollowUpSummary = lane?.OfficeLaneResidueFollowUpSummary ?? string.Empty,
            OfficeLaneNoLoopGuardSummary = lane?.OfficeLaneNoLoopGuardSummary ?? string.Empty,
            CourtPolicyEntryReadbackSummary = lane?.CourtPolicyEntryReadbackSummary ?? string.Empty,
            CourtPolicyDispatchReadbackSummary = lane?.CourtPolicyDispatchReadbackSummary ?? string.Empty,
            CourtPolicyPublicReadbackSummary = lane?.CourtPolicyPublicReadbackSummary ?? string.Empty,
            CourtPolicyNoLoopGuardSummary = lane?.CourtPolicyNoLoopGuardSummary ?? string.Empty,
            FamilyLaneEntryReadbackSummary = lane?.FamilyLaneEntryReadbackSummary ?? string.Empty,
            FamilyElderExplanationReadbackSummary = lane?.FamilyElderExplanationReadbackSummary ?? string.Empty,
            FamilyGuaranteeReadbackSummary = lane?.FamilyGuaranteeReadbackSummary ?? string.Empty,
            FamilyHouseFaceReadbackSummary = lane?.FamilyHouseFaceReadbackSummary ?? string.Empty,
            FamilyLaneReceiptClosureSummary = lane?.FamilyLaneReceiptClosureSummary ?? string.Empty,
            FamilyLaneResidueFollowUpSummary = lane?.FamilyLaneResidueFollowUpSummary ?? string.Empty,
            FamilyLaneNoLoopGuardSummary = lane?.FamilyLaneNoLoopGuardSummary ?? string.Empty,
            WarfareLaneEntryReadbackSummary = lane?.WarfareLaneEntryReadbackSummary ?? string.Empty,
            ForceReadinessReadbackSummary = lane?.ForceReadinessReadbackSummary ?? string.Empty,
            CampaignAftermathReadbackSummary = lane?.CampaignAftermathReadbackSummary ?? string.Empty,
            WarfareLaneReceiptClosureSummary = lane?.WarfareLaneReceiptClosureSummary ?? string.Empty,
            WarfareLaneResidueFollowUpSummary = lane?.WarfareLaneResidueFollowUpSummary ?? string.Empty,
            WarfareLaneNoLoopGuardSummary = lane?.WarfareLaneNoLoopGuardSummary ?? string.Empty,
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

        if (!string.IsNullOrWhiteSpace(settlement.CourtPolicyEntryReadbackSummary))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(settlement.CourtPolicyNoLoopGuardSummary))
        {
            score += 4;
        }

        if (!string.IsNullOrWhiteSpace(settlement.RegimeOfficeReadbackSummary))
        {
            score += 12;
        }

        if (!string.IsNullOrWhiteSpace(settlement.OfficeLaneReceiptClosureSummary))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(settlement.OfficeLaneNoLoopGuardSummary))
        {
            score += 6;
        }

        if (!string.IsNullOrWhiteSpace(settlement.CampaignAftermathReadbackSummary))
        {
            score += 12;
        }

        if (!string.IsNullOrWhiteSpace(settlement.WarfareLaneNoLoopGuardSummary))
        {
            score += 6;
        }

        if (!string.IsNullOrWhiteSpace(settlement.ResidueHealthSummary))
        {
            score += 8;
        }

        return score;
    }

}
