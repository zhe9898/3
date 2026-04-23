using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.OfficeAndCareer;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private static PlayerCommandResult IssueOfficeIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            return BuildRejectedOfficeResult(command, "当前存档未启用官署权柄。");
        }

        return simulation.IssueModuleCommand(KnownModuleKeys.OfficeAndCareer, command);
    }

    internal static string DetermineOfficeCommandLabel(string commandName)
    {
        return OfficeAndCareerCommandResolver.DetermineOfficeCommandLabel(commandName);
    }

    private static PlayerCommandResult BuildRejectedOfficeResult(PlayerCommandRequest command, string summary)
    {
        bool isPublicLife = command.CommandName is PlayerCommandNames.PostCountyNotice or PlayerCommandNames.DispatchRoadReport;
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            SurfaceKey = isPublicLife ? PlayerCommandSurfaceKeys.PublicLife : PlayerCommandSurfaceKeys.Office,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = isPublicLife
                ? OfficeAndCareerCommandResolver.DeterminePublicLifeOfficeCommandLabel(command.CommandName)
                : DetermineOfficeCommandLabel(command.CommandName),
            Summary = summary,
        };
    }
}
