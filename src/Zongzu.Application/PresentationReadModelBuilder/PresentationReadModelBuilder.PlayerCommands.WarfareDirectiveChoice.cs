using System;
using Zongzu.Contracts;
using Zongzu.Modules.WarfareCampaign;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static string BuildWarfareDirectiveChoiceReadback(
        string commandName,
        CampaignMobilizationSignalSnapshot? signal,
        CampaignFrontSnapshot? campaign)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return string.Empty;
        }

        string settlementLabel = campaign?.AnchorSettlementName
            ?? signal?.SettlementName
            ?? "军务据点";
        string directiveLabel = ResolveProjectedWarfareDirectiveLabel(commandName, campaign);
        string choiceLabel = commandName switch
        {
            PlayerCommandNames.DraftCampaignPlan => "案头筹议选择",
            PlayerCommandNames.CommitMobilization => "点兵加压选择",
            PlayerCommandNames.ProtectSupplyLine => "粮道护持选择",
            PlayerCommandNames.WithdrawToBarracks => "归营止损选择",
            _ => "案头观势选择",
        };
        string forceTail = signal is null
            ? string.Empty
            : $"可调之众{signal.AvailableForceCount}，动员窗{RenderMobilizationWindow(signal.MobilizationWindowLabel)}；";
        string campaignTail = campaign is null
            ? string.Empty
            : $"前线{campaign.FrontPressure}，粮道{campaign.SupplyStateLabel}，士气{campaign.MoraleStateLabel}；";

        return $"军令选择读回：{choiceLabel}指向{settlementLabel}“{directiveLabel}”；{forceTail}{campaignTail}WarfareCampaign拥有军令，ConflictAndForce只读力役/战备，Office只读官面协调；军务选择不是县门文移代打，不是普通家户硬扛。";
    }

    private static string ResolveProjectedWarfareDirectiveLabel(string commandName, CampaignFrontSnapshot? campaign)
    {
        if (campaign is not null
            && string.Equals(campaign.ActiveDirectiveCode, commandName, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel))
        {
            return campaign.ActiveDirectiveLabel;
        }

        return WarfareCampaignDescriptors.DetermineDirectiveLabel(commandName);
    }
}
