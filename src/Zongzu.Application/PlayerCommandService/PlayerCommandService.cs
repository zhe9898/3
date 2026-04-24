using System;
using Zongzu.Contracts;

namespace Zongzu.Application;

public sealed class PlayerCommandService
{
    public PlayerCommandResult IssueIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(command);

        if (!PlayerCommandCatalog.TryGet(command.CommandName, out PlayerCommandRoute? route))
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = string.Empty,
                SurfaceKey = string.Empty,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = command.CommandName,
                Summary = $"Unknown player command: {command.CommandName}.",
            };
        }

        if (!simulation.FeatureManifest.IsEnabled(route.ModuleKey))
        {
            return BuildDisabledResult(command, route);
        }

        return simulation.IssueModuleCommand(route.ModuleKey, command);
    }

    private static PlayerCommandResult BuildDisabledResult(PlayerCommandRequest command, PlayerCommandRoute route)
    {
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = route.ModuleKey,
            SurfaceKey = route.SurfaceKey,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = route.Label,
            Summary = route.DisabledSummary,
            TargetLabel = route.BuildDisabledTargetLabel(command),
        };
    }
}
