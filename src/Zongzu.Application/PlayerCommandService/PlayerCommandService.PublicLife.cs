using System;
using System.Linq;
using Zongzu.Contracts;
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

        OrderAndBanditryModule? module = simulation.Modules
            .OfType<OrderAndBanditryModule>()
            .FirstOrDefault();
        if (module is null)
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
                Summary = "当前运行模块未注册地方治安与护路。",
            };
        }

        OrderAndBanditryState state = simulation.GetMutableModuleState<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        OrderAdministrativeReachProfile administrativeReach = OrderAdministrativeReachEvaluator.Resolve(simulation, command.SettlementId);
        OrderPublicLifeCommandResult resolution = module.HandlePublicLifeCommand(
            state,
            new OrderPublicLifeCommand
            {
                SettlementId = command.SettlementId,
                CommandName = command.CommandName,
                CommandLabel = DeterminePublicLifeCommandLabel(command.CommandName),
                BenefitShift = administrativeReach.BenefitShift,
                ShieldingShift = administrativeReach.ShieldingShift,
                BacklashShift = administrativeReach.BacklashShift,
                LeakageShift = administrativeReach.LeakageShift,
                ReachSummaryTail = administrativeReach.SummaryTail,
            });

        if (!resolution.Accepted)
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = resolution.Label,
                Summary = resolution.Summary,
            };
        }

        simulation.RefreshReplayHash();
        return new PlayerCommandResult
        {
            Accepted = true,
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = resolution.Label,
            Summary = resolution.Summary,
            TargetLabel = $"据点 {command.SettlementId.Value}",
        };
    }

    private static QueryRegistry BuildQueries(GameSimulation simulation)
    {
        QueryRegistry queries = new();
        foreach (IModuleRunner module in simulation.Modules
                     .OrderBy(static module => module.Phase)
                     .ThenBy(static module => module.ExecutionOrder)
                     .ThenBy(static module => module.ModuleKey, StringComparer.Ordinal))
        {
            if (!simulation.FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!simulation.TryGetModuleState(module.ModuleKey, out object? state) || state is null)
            {
                throw new InvalidOperationException($"Enabled module {module.ModuleKey} has no state for player-command queries.");
            }

            module.RegisterQueries(state, queries);
        }

        return queries;
    }

    internal static string DeterminePublicLifeCommandLabel(string commandName)
    {
        if (string.Equals(commandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal))
        {
            return "添雇巡丁";
        }

        if (string.Equals(commandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal))
        {
            return "严缉路匪";
        }

        if (string.Equals(commandName, PlayerCommandNames.NegotiateWithOutlaws, StringComparison.Ordinal))
        {
            return "遣人议路";
        }

        if (string.Equals(commandName, PlayerCommandNames.TolerateDisorder, StringComparison.Ordinal))
        {
            return "暂缓穷追";
        }

        return commandName switch
        {
            PlayerCommandNames.PostCountyNotice => "张榜晓谕",
            PlayerCommandNames.DispatchRoadReport => "遣吏催报",
            PlayerCommandNames.EscortRoadReport => "催护一路",
            PlayerCommandNames.InviteClanEldersPubliclyBroker => "请族老出面",
            _ => commandName,
        };
    }
}
