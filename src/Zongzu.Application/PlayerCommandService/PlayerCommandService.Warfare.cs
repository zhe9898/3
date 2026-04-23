using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private static PlayerCommandResult IssueWarfareIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign))
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.WarfareCampaign,
                SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = command.CommandName,
                Summary = "当前存档并未启用军务沙盘。",
                TargetLabel = $"据点 {command.SettlementId.Value}",
            };
        }

        return simulation.IssueModuleCommand(KnownModuleKeys.WarfareCampaign, command);
    }
}
