using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WarfareCampaign;

public sealed class WarfareCampaignCommandContext
{
    public WarfareCampaignState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();
}

public static class WarfareCampaignCommandResolver
{
    public static PlayerCommandResult IssueIntent(WarfareCampaignCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.State);
        ArgumentNullException.ThrowIfNull(context.Command);

        PlayerCommandRequest command = context.Command;
        if (!IsKnownCommand(command.CommandName))
        {
            return BuildRejectedResult(command, $"未知军令：{command.CommandName}");
        }

        CampaignMobilizationSignalState? signal = context.State.MobilizationSignals
            .SingleOrDefault(existing => existing.SettlementId == command.SettlementId);
        if (signal is null)
        {
            return BuildRejectedResult(command, $"当前还没有可对据点 {command.SettlementId.Value} 发出的军务信号。");
        }

        string directiveLabel = WarfareCampaignDescriptors.DetermineDirectiveLabel(command.CommandName);
        string directiveSummary = WarfareCampaignDescriptors.BuildDirectiveSummary(command.CommandName, signal.SettlementName);
        signal.ActiveDirectiveCode = command.CommandName;
        signal.ActiveDirectiveLabel = directiveLabel;
        signal.ActiveDirectiveSummary = directiveSummary;

        CampaignFrontState? campaign = context.State.Campaigns
            .SingleOrDefault(existing => existing.AnchorSettlementId == command.SettlementId);
        if (campaign is not null)
        {
            campaign.ActiveDirectiveCode = command.CommandName;
            campaign.ActiveDirectiveLabel = directiveLabel;
            campaign.ActiveDirectiveSummary = directiveSummary;
            campaign.LastDirectiveTrace = $"{signal.SettlementName}已收到军令“{directiveLabel}”：{directiveSummary}";
        }

        return new PlayerCommandResult
        {
            Accepted = true,
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = directiveLabel,
            Summary = $"{signal.SettlementName}已改用军令“{directiveLabel}”。{directiveSummary}",
            TargetLabel = $"据点 {command.SettlementId.Value}",
        };
    }

    private static PlayerCommandResult BuildRejectedResult(PlayerCommandRequest command, string summary)
    {
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = WarfareCampaignDescriptors.DetermineDirectiveLabel(command.CommandName),
            Summary = summary,
            TargetLabel = $"据点 {command.SettlementId.Value}",
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
