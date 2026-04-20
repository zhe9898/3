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
}
