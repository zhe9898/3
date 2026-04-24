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
                    targetLabel: jurisdiction.LeadOfficialName);
            }

            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "遣吏催报", StringComparison.Ordinal))
            {
                yield return BuildPlayerCommandReceiptSnapshot(
                    PlayerCommandNames.DispatchRoadReport,
                    jurisdiction.SettlementId,
                    jurisdiction.LastAdministrativeTrace,
                    jurisdiction.LastPetitionOutcome,
                    targetLabel: jurisdiction.LeadOfficialName);
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
            if (!string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal))
            {
                continue;
            }

            yield return BuildPlayerCommandReceiptSnapshot(
                PlayerCommandNames.InviteClanEldersPubliclyBroker,
                clan.HomeSettlementId,
                clan.LastConflictTrace,
                clan.LastConflictOutcome,
                clanId: clan.Id,
                targetLabel: clan.ClanName);
        }
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
                receipts.Add(BuildPlayerCommandReceiptSnapshot(
                    commandName,
                    clan.HomeSettlementId,
                    clan.LastConflictTrace,
                    clan.LastConflictOutcome,
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
                jurisdiction.LastPetitionOutcome));
        }

        foreach (CampaignFrontSnapshot campaign in bundle.Campaigns.OrderBy(static entry => entry.CampaignId.Value))
        {
            if (string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)
                && string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace))
            {
                continue;
            }

            receipts.Add(BuildPlayerCommandReceiptSnapshot(
                campaign.ActiveDirectiveCode,
                campaign.AnchorSettlementId,
                campaign.LastDirectiveTrace,
                campaign.ActiveDirectiveSummary,
                labelOverride: campaign.ActiveDirectiveLabel));
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
}
