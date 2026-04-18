using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Modules.WarfareCampaign;

namespace Zongzu.Application;

public sealed class WarfareCampaignCommandService
{
    public WarfareCampaignIntentResult IssueIntent(GameSimulation simulation, WarfareCampaignIntentCommand command)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(command);

        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign))
        {
            return new WarfareCampaignIntentResult
            {
                Accepted = false,
                Summary = "当前存档并未启用军务沙盘。",
            };
        }

        if (!IsKnownCommand(command.CommandName))
        {
            return new WarfareCampaignIntentResult
            {
                Accepted = false,
                Summary = $"未知军令：{command.CommandName}",
            };
        }

        WarfareCampaignState state = simulation.GetMutableModuleState<WarfareCampaignState>(KnownModuleKeys.WarfareCampaign);
        CampaignMobilizationSignalState? signal = state.MobilizationSignals.SingleOrDefault(existing => existing.SettlementId == command.SettlementId);
        if (signal is null)
        {
            return new WarfareCampaignIntentResult
            {
                Accepted = false,
                Summary = $"当前还没有可对据点 {command.SettlementId.Value} 发出的军务信号。",
            };
        }

        string directiveLabel = WarfareCampaignDescriptors.DetermineDirectiveLabel(command.CommandName);
        string directiveSummary = WarfareCampaignDescriptors.BuildDirectiveSummary(command.CommandName, signal.SettlementName);
        signal.ActiveDirectiveCode = command.CommandName;
        signal.ActiveDirectiveLabel = directiveLabel;
        signal.ActiveDirectiveSummary = directiveSummary;

        CampaignFrontState? campaign = state.Campaigns.SingleOrDefault(existing => existing.AnchorSettlementId == command.SettlementId);
        if (campaign is not null)
        {
            campaign.ActiveDirectiveCode = command.CommandName;
            campaign.ActiveDirectiveLabel = directiveLabel;
            campaign.ActiveDirectiveSummary = directiveSummary;
            campaign.LastDirectiveTrace = $"{signal.SettlementName}已收到军令“{directiveLabel}”：{directiveSummary}";
        }

        simulation.RefreshReplayHash();

        return new WarfareCampaignIntentResult
        {
            Accepted = true,
            DirectiveLabel = directiveLabel,
            Summary = $"{signal.SettlementName}已改用军令“{directiveLabel}”。{directiveSummary}",
        };
    }

    private static bool IsKnownCommand(string commandName)
    {
        return string.Equals(commandName, WarfareCampaignCommandNames.DraftCampaignPlan, StringComparison.Ordinal)
            || string.Equals(commandName, WarfareCampaignCommandNames.CommitMobilization, StringComparison.Ordinal)
            || string.Equals(commandName, WarfareCampaignCommandNames.ProtectSupplyLine, StringComparison.Ordinal)
            || string.Equals(commandName, WarfareCampaignCommandNames.WithdrawToBarracks, StringComparison.Ordinal);
    }
}
