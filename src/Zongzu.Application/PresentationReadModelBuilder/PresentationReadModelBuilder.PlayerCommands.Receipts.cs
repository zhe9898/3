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

    private static IEnumerable<PlayerCommandReceiptSnapshot> BuildPublicLifeReceipts(PresentationReadModelBundle bundle)
    {
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = IndexFirstBySettlement(
            bundle.OfficeJurisdictions,
            static entry => entry.SettlementId);
        Dictionary<int, SettlementPublicLifeSnapshot> publicLifeBySettlement = IndexFirstBySettlement(
            bundle.PublicLifeSettlements,
            static entry => entry.SettlementId);
        ILookup<int, ClanSnapshot> clansBySettlement = bundle.Clans.ToLookup(static entry => entry.HomeSettlementId.Value);
        Dictionary<int, ClanNarrativeSnapshot> narrativesByClan = bundle.ClanNarratives
            .ToDictionary(static entry => entry.ClanId.Value, static entry => entry);
        ILookup<int, ClanTradeSnapshot> tradesBySettlement = bundle.ClanTrades
            .ToLookup(static entry => entry.PrimarySettlementId.Value);
        ILookup<int, ClanTradeRouteSnapshot> routesBySettlement = bundle.ClanTradeRoutes
            .ToLookup(static entry => entry.SettlementId.Value);

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "张榜晓谕", StringComparison.Ordinal))
            {
                yield return BuildPlayerCommandReceiptSnapshot(
                    PlayerCommandNames.PostCountyNotice,
                jurisdiction.SettlementId,
                jurisdiction.LastAdministrativeTrace,
                jurisdiction.LastPetitionOutcome,
                readbackSummary: BuildOfficeImplementationAffordanceGuidance(jurisdiction),
                targetLabel: jurisdiction.LeadOfficialName);
            }

            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "遣吏催报", StringComparison.Ordinal))
            {
                yield return BuildPlayerCommandReceiptSnapshot(
                    PlayerCommandNames.DispatchRoadReport,
                jurisdiction.SettlementId,
                jurisdiction.LastAdministrativeTrace,
                jurisdiction.LastPetitionOutcome,
                readbackSummary: BuildOfficeImplementationAffordanceGuidance(jurisdiction),
                targetLabel: jurisdiction.LeadOfficialName);
            }

            if (HasPublicLifeOrderResponseReceipt(jurisdiction))
            {
                ClanSnapshot[] localClans = clansBySettlement[jurisdiction.SettlementId.Value]
                    .OrderByDescending(static entry => entry.Prestige)
                    .ThenBy(static entry => entry.ClanName, StringComparer.Ordinal)
                    .ToArray();
                IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories =
                    SelectLocalPublicLifeOrderSocialMemories(bundle.SocialMemories, localClans);
                yield return BuildPlayerCommandReceiptSnapshot(
                    jurisdiction.LastRefusalResponseCommandCode,
                    jurisdiction.SettlementId,
                    jurisdiction.LastRefusalResponseSummary,
                    RenderPublicLifeResponseOutcome(jurisdiction.LastRefusalResponseOutcomeCode),
                    executionSummary: BuildOfficeResponseAftermathSummary(jurisdiction),
                    readbackSummary: CombinePublicLifeResponseText(
                        BuildOfficeResponseAftermathSummary(jurisdiction),
                        BuildOfficeLaneReceiptClosureSummary(jurisdiction),
                        BuildOwnerLaneFollowUpReceiptClosure(
                            localSocialMemories,
                            OwnerLaneReturnSourceOffice,
                            jurisdiction.LastRefusalResponseCommandCode,
                            jurisdiction.LastRefusalResponseOutcomeCode),
                        BuildOfficeLaneNoLoopGuardSummary(jurisdiction, [], [])),
                    targetLabel: string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
                        ? jurisdiction.LeadOfficeTitle
                        : jurisdiction.LeadOfficialName,
                    labelOverride: jurisdiction.LastRefusalResponseCommandLabel);
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            jurisdictionsBySettlement.TryGetValue(disorder.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
            publicLifeBySettlement.TryGetValue(disorder.SettlementId.Value, out SettlementPublicLifeSnapshot? publicLife);
            ClanSnapshot[] localClans = clansBySettlement[disorder.SettlementId.Value]
                .OrderByDescending(static entry => entry.Prestige)
                .ThenBy(static entry => entry.ClanName, StringComparer.Ordinal)
                .ToArray();
            ClanNarrativeSnapshot[] localNarratives = localClans
                .Where(clan => narrativesByClan.ContainsKey(clan.Id.Value))
                .Select(clan => narrativesByClan[clan.Id.Value])
                .ToArray();
            ClanTradeSnapshot[] localTrades = tradesBySettlement[disorder.SettlementId.Value].ToArray();
            ClanTradeRouteSnapshot[] localRoutes = routesBySettlement[disorder.SettlementId.Value].ToArray();
            IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories =
                SelectLocalPublicLifeOrderSocialMemories(bundle.SocialMemories, localClans);

            PlayerCommandReceiptSnapshot? receipt = BuildOrderPublicLifeReceipt(
                disorder,
                jurisdiction,
                publicLife,
                localClans,
                localNarratives,
                localTrades,
                localRoutes,
                localSocialMemories);
            if (receipt is not null)
            {
                yield return receipt;
            }

            if (HasPublicLifeOrderResponseReceipt(disorder))
            {
                yield return BuildPlayerCommandReceiptSnapshot(
                    disorder.LastRefusalResponseCommandCode,
                    disorder.SettlementId,
                    disorder.LastRefusalResponseSummary,
                    RenderPublicLifeResponseOutcome(disorder.LastRefusalResponseOutcomeCode),
                    executionSummary: BuildOrderResponseAftermathSummary(disorder),
                    readbackSummary: CombinePublicLifeResponseText(
                        BuildOrderResponseAftermathSummary(disorder),
                        BuildOrderSocialMemoryReadbackSummary(localSocialMemories),
                        BuildOwnerLaneFollowUpReceiptClosure(
                            localSocialMemories,
                            OwnerLaneReturnSourceOrder,
                            disorder.LastRefusalResponseCommandCode,
                            disorder.LastRefusalResponseOutcomeCode)),
                    targetLabel: disorder.SettlementId.Value.ToString(),
                    labelOverride: disorder.LastRefusalResponseCommandLabel);
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (!disorder.LastPressureReason.Contains("催护一路", StringComparison.Ordinal))
            {
                continue;
            }

            CommandLeverageProjection escortProjection = BuildOrderPublicLifeLeverageProjection(
                PlayerCommandNames.EscortRoadReport,
                null,
                disorder,
                null,
                [],
                [],
                [],
                [],
                []);

            yield return BuildPlayerCommandReceiptSnapshot(
                PlayerCommandNames.EscortRoadReport,
                disorder.SettlementId,
                disorder.LastPressureReason,
                $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                leverageSummary: escortProjection.LeverageSummary,
                costSummary: escortProjection.CostSummary,
                readbackSummary: escortProjection.ReadbackSummary,
                targetLabel: disorder.SettlementId.Value.ToString());
        }

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.HomeSettlementId.Value))
        {
            if (!string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)
                && !string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.AskClanEldersExplain, StringComparison.Ordinal))
            {
                continue;
            }

            IReadOnlyList<SocialMemoryEntrySnapshot> familySocialMemories =
                SelectLocalPublicLifeOrderSocialMemories(bundle.SocialMemories, [clan]);
            FamilyLaneClosureReadback familyLaneClosure = BuildFamilyLaneClosureReadback(
                SelectRecentLocalResponseHouseholdForClan(bundle.Households, clan),
                clan,
                familySocialMemories);
            yield return BuildPlayerCommandReceiptSnapshot(
                clan.LastConflictCommandCode,
                clan.HomeSettlementId,
                clan.LastConflictTrace,
                clan.LastConflictOutcome,
                readbackSummary: HasPublicLifeOrderResponseReceipt(clan)
                    ? CombinePublicLifeResponseText(
                        BuildFamilyResponseAftermathSummary(clan),
                        BuildFamilyLaneClosureReadbackText(familyLaneClosure),
                        BuildOwnerLaneFollowUpReceiptClosure(
                            familySocialMemories,
                            OwnerLaneReturnSourceFamily,
                            clan.LastRefusalResponseCommandCode,
                            clan.LastRefusalResponseOutcomeCode))
                    : BuildFamilyLaneClosureReadbackText(familyLaneClosure),
                familyLaneEntryReadbackSummary: familyLaneClosure.EntryReadbackSummary,
                familyElderExplanationReadbackSummary: familyLaneClosure.ElderExplanationReadbackSummary,
                familyGuaranteeReadbackSummary: familyLaneClosure.GuaranteeReadbackSummary,
                familyHouseFaceReadbackSummary: familyLaneClosure.HouseFaceReadbackSummary,
                familyLaneReceiptClosureSummary: familyLaneClosure.ReceiptClosureSummary,
                familyLaneResidueFollowUpSummary: familyLaneClosure.ResidueFollowUpSummary,
                familyLaneNoLoopGuardSummary: familyLaneClosure.NoLoopGuardSummary,
                clanId: clan.Id,
                targetLabel: clan.ClanName,
                labelOverride: clan.LastConflictCommandLabel);
        }
    }

    private static bool HasPublicLifeOrderResponseReceipt(SettlementDisorderSnapshot disorder)
    {
        return !string.IsNullOrWhiteSpace(disorder.LastRefusalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(disorder.LastRefusalResponseOutcomeCode);
    }

    private static bool HasPublicLifeOrderResponseReceipt(JurisdictionAuthoritySnapshot jurisdiction)
    {
        return !string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseOutcomeCode);
    }

    private static bool HasPublicLifeOrderResponseReceipt(ClanSnapshot clan)
    {
        return !string.IsNullOrWhiteSpace(clan.LastRefusalResponseCommandCode)
            && !string.IsNullOrWhiteSpace(clan.LastRefusalResponseOutcomeCode);
    }

    private static string RenderPublicLifeResponseOutcome(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "后账已修复",
            PublicLifeOrderResponseOutcomeCodes.Contained => "后账暂压",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "后账恶化",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "后账放置",
            _ => outcomeCode,
        };
    }

    private static string BuildOfficeResponseAftermathSummary(JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (!HasPublicLifeOrderResponseReceipt(jurisdiction))
        {
            return string.Empty;
        }

        string commandLabel = string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseCommandLabel)
            ? jurisdiction.LastRefusalResponseCommandCode
            : jurisdiction.LastRefusalResponseCommandLabel;
        string yamenTail = jurisdiction.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "县门已补落地，文移进入案牍正道。",
            PublicLifeOrderResponseOutcomeCodes.Contained => "县门正道仍滞，递报先把路情暂压。",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "胥吏继续拖延，后账转成新的积案。",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "县门未接住前案，后账仍在。",
            _ => jurisdiction.LastRefusalResponseOutcomeCode,
        };
        return $"{commandLabel}：{yamenTail}积案{jurisdiction.PetitionBacklog}，胥吏牵制{jurisdiction.ClerkDependence}。";
    }

    private static string BuildFamilyResponseAftermathSummary(ClanSnapshot clan)
    {
        if (!HasPublicLifeOrderResponseReceipt(clan))
        {
            return string.Empty;
        }

        string commandLabel = string.IsNullOrWhiteSpace(clan.LastRefusalResponseCommandLabel)
            ? clan.LastRefusalResponseCommandCode
            : clan.LastRefusalResponseCommandLabel;
        string familyTail = clan.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "族老解释缓下羞面，本户担保重新站住。",
            PublicLifeOrderResponseOutcomeCodes.Contained => "族老解释先压住街口议论，本户仍欠人情。",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "族老解释反使议论翻起，怨尾加深。",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "族老未接住前案，担保欠账仍在。",
            _ => clan.LastRefusalResponseOutcomeCode,
        };
        return $"{commandLabel}：{familyTail}门望{clan.Prestige}，调停势{clan.MediationMomentum}，房支争力{clan.BranchTension}。";
    }

    private static PlayerCommandReceiptSnapshot? BuildOrderPublicLifeReceipt(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        SettlementPublicLifeSnapshot? publicLife,
        IReadOnlyList<ClanSnapshot> localClans,
        IReadOnlyList<ClanNarrativeSnapshot> localNarratives,
        IReadOnlyList<ClanTradeSnapshot> localTrades,
        IReadOnlyList<ClanTradeRouteSnapshot> localRoutes,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        if (string.IsNullOrWhiteSpace(disorder.LastInterventionCommandCode)
            || !IsOrderPublicLifeCommand(disorder.LastInterventionCommandCode))
        {
            return null;
        }

        CommandLeverageProjection leverageProjection = BuildOrderPublicLifeLeverageProjection(
            disorder.LastInterventionCommandCode,
            publicLife,
            disorder,
            jurisdiction,
            localClans,
            localNarratives,
            localTrades,
            localRoutes,
            localSocialMemories);

        string socialMemoryReadback = BuildOrderSocialMemoryReadbackSummary(localSocialMemories);
        string readbackSummary = string.IsNullOrWhiteSpace(socialMemoryReadback)
            ? leverageProjection.ReadbackSummary
            : string.Join(" ", new[] { leverageProjection.ReadbackSummary, socialMemoryReadback }
                .Where(static value => !string.IsNullOrWhiteSpace(value)));

        return BuildPlayerCommandReceiptSnapshot(
            disorder.LastInterventionCommandCode,
            disorder.SettlementId,
            disorder.LastInterventionSummary,
            disorder.LastInterventionOutcome,
            executionSummary: BuildOrderAdministrativeAftermathExecutionSummary(disorder, jurisdiction),
            leverageSummary: leverageProjection.LeverageSummary,
            costSummary: leverageProjection.CostSummary,
            readbackSummary: readbackSummary,
            targetLabel: disorder.SettlementId.Value.ToString(),
            labelOverride: disorder.LastInterventionCommandLabel);
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
                string commandName = string.IsNullOrWhiteSpace(clan.LastConflictCommandCode)
                    ? PlayerCommandNames.InviteClanEldersMediation
                    : clan.LastConflictCommandCode;
                IReadOnlyList<SocialMemoryEntrySnapshot> familySocialMemories =
                    SelectLocalPublicLifeOrderSocialMemories(bundle.SocialMemories, [clan]);
                FamilyLaneClosureReadback familyLaneClosure = BuildFamilyLaneClosureReadback(
                    SelectRecentLocalResponseHouseholdForClan(bundle.Households, clan),
                    clan,
                    familySocialMemories);
                string familyReliefChoiceReadback = string.Equals(commandName, PlayerCommandNames.GrantClanRelief, StringComparison.Ordinal)
                    ? BuildFamilyReliefChoiceReadback(clan)
                    : string.Empty;
                receipts.Add(BuildPlayerCommandReceiptSnapshot(
                    commandName,
                    clan.HomeSettlementId,
                    clan.LastConflictTrace,
                    clan.LastConflictOutcome,
                    readbackSummary: HasPublicLifeOrderResponseReceipt(clan)
                        ? CombinePublicLifeResponseText(
                            familyReliefChoiceReadback,
                            BuildFamilyResponseAftermathSummary(clan),
                            BuildFamilyLaneClosureReadbackText(familyLaneClosure),
                            BuildOwnerLaneFollowUpReceiptClosure(
                                familySocialMemories,
                                OwnerLaneReturnSourceFamily,
                                clan.LastRefusalResponseCommandCode,
                                clan.LastRefusalResponseOutcomeCode))
                        : JoinOwnerLaneReturnSurfaceText(
                            familyReliefChoiceReadback,
                            BuildFamilyLaneClosureReadbackText(familyLaneClosure)),
                    familyLaneEntryReadbackSummary: familyLaneClosure.EntryReadbackSummary,
                    familyElderExplanationReadbackSummary: familyLaneClosure.ElderExplanationReadbackSummary,
                    familyGuaranteeReadbackSummary: familyLaneClosure.GuaranteeReadbackSummary,
                    familyHouseFaceReadbackSummary: familyLaneClosure.HouseFaceReadbackSummary,
                    familyLaneReceiptClosureSummary: familyLaneClosure.ReceiptClosureSummary,
                    familyLaneResidueFollowUpSummary: familyLaneClosure.ResidueFollowUpSummary,
                    familyLaneNoLoopGuardSummary: familyLaneClosure.NoLoopGuardSummary,
                    clanId: clan.Id,
                    targetLabel: clan.ClanName,
                    labelOverride: string.IsNullOrWhiteSpace(clan.LastConflictCommandLabel)
                        ? "祠堂议决"
                        : clan.LastConflictCommandLabel));
            }

            if (!string.IsNullOrWhiteSpace(clan.LastLifecycleTrace) || !string.IsNullOrWhiteSpace(clan.LastLifecycleOutcome))
            {
                string commandName = string.IsNullOrWhiteSpace(clan.LastLifecycleCommandCode)
                    ? PlayerCommandNames.ArrangeMarriage
                    : clan.LastLifecycleCommandCode;
                receipts.Add(BuildPlayerCommandReceiptSnapshot(
                    commandName,
                    clan.HomeSettlementId,
                    clan.LastLifecycleTrace,
                    clan.LastLifecycleOutcome,
                    clanId: clan.Id,
                    targetLabel: clan.ClanName,
                    labelOverride: string.IsNullOrWhiteSpace(clan.LastLifecycleCommandLabel)
                        ? "门内后计"
                        : clan.LastLifecycleCommandLabel));
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

            receipts.Add(BuildPlayerCommandReceiptSnapshot(
                commandName,
                jurisdiction.SettlementId,
                jurisdiction.LastAdministrativeTrace,
                jurisdiction.LastPetitionOutcome,
                readbackSummary: BuildOfficeImplementationAffordanceGuidance(jurisdiction)));
        }

        Dictionary<int, CampaignMobilizationSignalSnapshot> signalsBySettlement = IndexFirstBySettlement(
            bundle.CampaignMobilizationSignals,
            static entry => entry.SettlementId);
        Dictionary<int, JurisdictionAuthoritySnapshot> warfareJurisdictionsBySettlement = IndexFirstBySettlement(
            bundle.OfficeJurisdictions,
            static entry => entry.SettlementId);
        ILookup<int, ClanSnapshot> warfareClansBySettlement = bundle.Clans
            .ToLookup(static entry => entry.HomeSettlementId.Value);

        foreach (CampaignFrontSnapshot campaign in bundle.Campaigns.OrderBy(static entry => entry.CampaignId.Value))
        {
            if (string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)
                && string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace))
            {
                continue;
            }

            signalsBySettlement.TryGetValue(campaign.AnchorSettlementId.Value, out CampaignMobilizationSignalSnapshot? signal);
            warfareJurisdictionsBySettlement.TryGetValue(campaign.AnchorSettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
            ClanSnapshot[] localClans = warfareClansBySettlement[campaign.AnchorSettlementId.Value]
                .OrderByDescending(static clan => clan.Prestige)
                .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
                .ToArray();
            IReadOnlyList<SocialMemoryEntrySnapshot> localCampaignSocialMemories =
                SelectLocalCampaignSocialMemories(bundle.SocialMemories, localClans);
            WarfareLaneClosureReadback warfareLaneClosure = BuildWarfareLaneClosureReadback(
                signal,
                campaign,
                jurisdiction,
                localCampaignSocialMemories);

            receipts.Add(BuildPlayerCommandReceiptSnapshot(
                campaign.ActiveDirectiveCode,
                campaign.AnchorSettlementId,
                campaign.LastDirectiveTrace,
                campaign.ActiveDirectiveSummary,
                readbackSummary: BuildWarfareLaneClosureReadbackText(warfareLaneClosure),
                warfareLaneEntryReadbackSummary: warfareLaneClosure.EntryReadbackSummary,
                forceReadinessReadbackSummary: warfareLaneClosure.ForceReadinessReadbackSummary,
                campaignAftermathReadbackSummary: warfareLaneClosure.CampaignAftermathReadbackSummary,
                warfareLaneReceiptClosureSummary: warfareLaneClosure.ReceiptClosureSummary,
                warfareLaneResidueFollowUpSummary: warfareLaneClosure.ResidueFollowUpSummary,
                warfareLaneNoLoopGuardSummary: warfareLaneClosure.NoLoopGuardSummary,
                targetLabel: campaign.CampaignName,
                labelOverride: campaign.ActiveDirectiveLabel));
        }

        receipts.AddRange(BuildPublicLifeReceipts(bundle));
        receipts.AddRange(BuildHomeHouseholdLocalResponseReceipts(bundle));
        IReadOnlyList<PlayerCommandReceiptSnapshot> ordinaryHouseholdResponseReceipts =
            AddOrdinaryHouseholdResponseReceiptSurface(receipts, bundle.HouseholdSocialPressures);
        return ordinaryHouseholdResponseReceipts
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
}
