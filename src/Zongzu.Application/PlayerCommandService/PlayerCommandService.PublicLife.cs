using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private static PlayerCommandResult IssueExpandedOrderIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry))
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = "当前存档未启用地方治安与护路。",
            };
        }

        return simulation.IssueModuleCommand(KnownModuleKeys.OrderAndBanditry, command);
    }

    internal static string DeterminePublicLifeCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.PostCountyNotice
                or PlayerCommandNames.DispatchRoadReport
                => OfficeAndCareerCommandResolver.DeterminePublicLifeOfficeCommandLabel(commandName),
            PlayerCommandNames.InviteClanEldersPubliclyBroker
                => FamilyCoreCommandResolver.DetermineFamilyCommandLabel(commandName),
            _ => OrderAndBanditryCommandResolver.DeterminePublicLifeCommandLabel(commandName),
        };
    }
}
