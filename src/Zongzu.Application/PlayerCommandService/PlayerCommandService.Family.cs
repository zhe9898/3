using System;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private static PlayerCommandResult IssueFamilyIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.FamilyCore))
        {
            return BuildDisabledFamilyResult(command, "当前存档未启用宗房裁断。");
        }

        return simulation.IssueModuleCommand(KnownModuleKeys.FamilyCore, command);
    }

    internal static string DetermineFamilyCommandLabel(string commandName)
    {
        return FamilyCoreCommandResolver.DetermineFamilyCommandLabel(commandName);
    }

    private static PlayerCommandResult BuildDisabledFamilyResult(PlayerCommandRequest command, string summary)
    {
        bool isPublicLife = string.Equals(command.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal);
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.FamilyCore,
            SurfaceKey = isPublicLife ? PlayerCommandSurfaceKeys.PublicLife : PlayerCommandSurfaceKeys.Family,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = isPublicLife ? DeterminePublicLifeCommandLabel(command.CommandName) : DetermineFamilyCommandLabel(command.CommandName),
            Summary = summary,
        };
    }
}
