using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Modules.OrderAndBanditry;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryCommandContext
{
    public OrderAndBanditryState State { get; init; } = new();

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
        public bool HasModifier => BenefitShift != 0 || ShieldingShift != 0 || BacklashShift != 0 || LeakageShift != 0;

        public static OrderAdministrativeReachProfile Neutral => new(
            0,
            0,
            0,
            0,
            string.Empty,
            "此地眼下多凭本地人手与地面情势，官面帮衬未显。");
    }
}
