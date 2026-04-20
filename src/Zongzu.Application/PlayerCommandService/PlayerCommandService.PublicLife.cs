using System;
using System.Linq;
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

        OrderAndBanditryState state = simulation.GetMutableModuleState<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState? settlement = state.Settlements.SingleOrDefault(entry => entry.SettlementId == command.SettlementId);
        if (settlement is null)
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
                Summary = $"此地暂无可调度的路面与治安节制：{command.SettlementId.Value}。",
            };
        }

        OrderAdministrativeReachProfile administrativeReach = OrderAdministrativeReachEvaluator.Resolve(simulation, command.SettlementId);

        if (!TryApplyOrderPublicLifeCommand(settlement, command.CommandName, administrativeReach))
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
                Summary = $"地方治安不识此令：{command.CommandName}。",
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
            Label = DeterminePublicLifeCommandLabel(command.CommandName),
            Summary = settlement.LastInterventionSummary,
            TargetLabel = $"据点 {command.SettlementId.Value}",
        };
    }

    private static PlayerCommandResult IssueOrderIntentLegacy(GameSimulation simulation, PlayerCommandRequest command)
    {
        return IssueExpandedOrderIntent(simulation, command);

#if false
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
                Summary = "当前存档未启用地方巡缉与护路。",
            };
        }

        OrderAndBanditryState state = simulation.GetMutableModuleState<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState? settlement = state.Settlements.SingleOrDefault(entry => entry.SettlementId == command.SettlementId);
        if (settlement is null)
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
                Summary = $"此地暂无线路可护：{command.SettlementId.Value}。",
            };
        }

        switch (command.CommandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 8);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 5);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.LastPressureReason = "已催护一路，先顾津口驿报与往来行货。";
                break;
            default:
                return new PlayerCommandResult
                {
                    Accepted = false,
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = command.SettlementId,
                    ClanId = command.ClanId,
                    CommandName = command.CommandName,
                    Label = DeterminePublicLifeCommandLabel(command.CommandName),
                    Summary = $"地方巡级不识此令：{command.CommandName}。",
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
            Label = DeterminePublicLifeCommandLabel(command.CommandName),
            Summary = settlement.LastPressureReason,
            TargetLabel = $"据点 {command.SettlementId.Value}",
        };
    }

#endif
    }

    private static OrderAdministrativeReachProfile ResolveOrderAdministrativeReach(GameSimulation simulation, SettlementId settlementId)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        QueryRegistry queries = BuildQueries(simulation);
        IOfficeAndCareerQueries officeQueries;
        try
        {
            officeQueries = queries.GetRequired<IOfficeAndCareerQueries>();
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        JurisdictionAuthoritySnapshot jurisdiction;
        try
        {
            jurisdiction = officeQueries.GetRequiredJurisdiction(settlementId);
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        int supportSignal =
            jurisdiction.JurisdictionLeverage
            + jurisdiction.ClerkDependence
            + (jurisdiction.AuthorityTier * 10);
        int frictionSignal =
            jurisdiction.AdministrativeTaskLoad
            + jurisdiction.PetitionPressure
            + (jurisdiction.PetitionBacklog / 2);
        int netSignal = supportSignal - frictionSignal;

        if (netSignal >= 40)
        {
            return new OrderAdministrativeReachProfile(3, 5, -4, -3, "县署肯出手，文移与差役都跟得上。");
        }

        if (netSignal >= 15)
        {
            return new OrderAdministrativeReachProfile(1, 2, -2, -1, "县署还能接得住，文移差役尚能随令。");
        }

        if (netSignal <= -40)
        {
            return new OrderAdministrativeReachProfile(-3, -5, 4, 3, "县署拥案未解，文移不畅，路上只得勉强敷衍。");
        }

        if (netSignal <= -15)
        {
            return new OrderAdministrativeReachProfile(-1, -2, 2, 1, "县署案牍偏重，差役跟得慢，只能先做半套。");
        }

        return OrderAdministrativeReachProfile.Neutral;
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

    private static bool TryApplyOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        string commandName,
        OrderAdministrativeReachProfile administrativeReach)
    {
        if (administrativeReach.HasModifier)
        {
            return TryApplyOfficeAwareOrderPublicLifeCommand(settlement, commandName, administrativeReach);
        }

        switch (commandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 8);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 5);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + 12, 0, 100);
                settlement.ImplementationDrag = Math.Max(0, settlement.ImplementationDrag - 4);
                settlement.LastPressureReason = "已先护住路报往来，河埠脚路暂得照看。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已催护一路，先把沿途报信与货脚照看起来。",
                    $"路压缓到{settlement.RoutePressure}，镇压之需减到{settlement.SuppressionDemand}，护路得力升到{settlement.RouteShielding}。");
                return true;
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 3);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 10);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 4);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 5);
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + 16, 0, 100);
                settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel + 1, 0, 12);
                settlement.LastPressureReason = "已添雇巡丁，先把路口、渡头与夜巡补起来。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已添雇巡丁，先补路口、渡头与夜巡人手。",
                    $"路压缓到{settlement.RoutePressure}，地面不靖退到{settlement.DisorderPressure}，护路得力升到{settlement.RouteShielding}。");
                return true;
            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 12);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 6);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 8);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 4);
                settlement.CoercionRisk = Math.Clamp(settlement.CoercionRisk + 8, 0, 100);
                settlement.RetaliationRisk = Math.Clamp(settlement.RetaliationRisk + 14, 0, 100);
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + 2, 0, 12);
                settlement.LastPressureReason = "已严缉路匪，先压住明面扰动，但后手报复也会跟着抬头。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已严缉路匪，先压明面匪踪与拦路生事。",
                    $"盗压降到{settlement.BanditThreat}，路压降到{settlement.RoutePressure}，但反噬险已升到{settlement.RetaliationRisk}。");
                return true;
            case PlayerCommandNames.NegotiateWithOutlaws:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 4);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 3);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 10);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + 6, 0, 100);
                settlement.CoercionRisk = Math.Max(0, settlement.CoercionRisk - 6);
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - 12);
                settlement.LastPressureReason = "已遣人议路，先换一路暂安，但私下分流会更容易坐大。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已遣人议路，先换渡头与路口一时安静。",
                    $"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但私路压抬到{settlement.BlackRoutePressure}。");
                return true;
            case PlayerCommandNames.TolerateDisorder:
                settlement.BanditThreat = Math.Clamp(settlement.BanditThreat + 4, 0, 100);
                settlement.RoutePressure = Math.Clamp(settlement.RoutePressure + 6, 0, 100);
                settlement.DisorderPressure = Math.Clamp(settlement.DisorderPressure + 5, 0, 100);
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + 8, 0, 100);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 6);
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - 8);
                settlement.RouteShielding = Math.Max(0, settlement.RouteShielding - 4);
                settlement.LastPressureReason = "已暂缓穷追，先不把地面逼到立刻翻脸，但路面与私下手脚会继续滋长。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已暂缓穷追，先把差役与地面都收一收。",
                    $"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但路压涨到{settlement.RoutePressure}，私路压涨到{settlement.BlackRoutePressure}。");
                return true;
            default:
                return false;
        }
    }

    private static bool TryApplyOfficeAwareOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        string commandName,
        OrderAdministrativeReachProfile administrativeReach)
    {
        switch (commandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(5, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(2, administrativeReach.BenefitShift, 0));
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + AdjustIncrease(12, administrativeReach.ShieldingShift), 0, 100);
                settlement.ImplementationDrag = Math.Max(0, settlement.ImplementationDrag - AdjustReduction(4, Math.Max(0, administrativeReach.BenefitShift)));
                settlement.LastPressureReason = AppendReachSummary("已先护住路报往来，河埠脚路暂得照看。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已催护一路，先把沿途报信与货脚照看起来。", administrativeReach),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，镇压之需减到{settlement.SuppressionDemand}，护路得力升到{settlement.RouteShielding}。", administrativeReach));
                return true;
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(3, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(10, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(5, administrativeReach.BenefitShift));
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + AdjustIncrease(16, administrativeReach.ShieldingShift), 0, 100);
                settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel + 1, 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已添雇巡丁，先把路口、渡头与夜巡补起来。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已添雇巡丁，先补路口、渡头与夜巡人手。", administrativeReach),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，地面不靖退到{settlement.DisorderPressure}，护路得力升到{settlement.RouteShielding}。", administrativeReach));
                return true;
            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(12, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.CoercionRisk = Math.Clamp(settlement.CoercionRisk + AdjustIncrease(8, administrativeReach.BacklashShift), 0, 100);
                settlement.RetaliationRisk = Math.Clamp(settlement.RetaliationRisk + AdjustIncrease(14, administrativeReach.BacklashShift), 0, 100);
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + AdjustIncrease(2, Math.Max(0, administrativeReach.BenefitShift)), 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已严缉路匪，先压住明面扰动，但后手报复也会跟着抬头。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已严缉路匪，先压明面匪踪与拦路生事。", administrativeReach),
                    AppendReachSummary($"盗压降到{settlement.BanditThreat}，路压降到{settlement.RoutePressure}，但反噬险已升到{settlement.RetaliationRisk}。", administrativeReach));
                return true;
            case PlayerCommandNames.NegotiateWithOutlaws:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(3, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(10, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(2, administrativeReach.BenefitShift, 0));
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + AdjustIncrease(6, administrativeReach.LeakageShift), 0, 100);
                settlement.CoercionRisk = Math.Max(0, settlement.CoercionRisk - AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - AdjustReduction(12, administrativeReach.BenefitShift));
                settlement.LastPressureReason = AppendReachSummary("已遣人议路，先换一路暂安，但私下分流会更容易坐大。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已遣人议路，先换渡头与路口一时安静。", administrativeReach),
                    AppendReachSummary($"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但私路压抬到{settlement.BlackRoutePressure}。", administrativeReach));
                return true;
            case PlayerCommandNames.TolerateDisorder:
                settlement.BanditThreat = Math.Clamp(settlement.BanditThreat + AdjustIncrease(4, administrativeReach.LeakageShift), 0, 100);
                settlement.RoutePressure = Math.Clamp(settlement.RoutePressure + AdjustIncrease(6, administrativeReach.LeakageShift), 0, 100);
                settlement.DisorderPressure = Math.Clamp(settlement.DisorderPressure + AdjustIncrease(5, administrativeReach.LeakageShift), 0, 100);
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + AdjustIncrease(8, administrativeReach.LeakageShift), 0, 100);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.RouteShielding = Math.Max(0, settlement.RouteShielding - Math.Max(0, 4 - Math.Max(0, administrativeReach.BenefitShift)));
                settlement.LastPressureReason = AppendReachSummary("已暂缓穷追，先不把地面逼到立刻翻脸，但路面与私下手脚会继续滋长。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已暂缓穷追，先把差役与地面都收一收。", administrativeReach),
                    AppendReachSummary($"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但路压涨到{settlement.RoutePressure}，私路压涨到{settlement.BlackRoutePressure}。", administrativeReach));
                return true;
            default:
                return false;
        }
    }

    private static int AdjustReduction(int baseValue, int shift, int minimum = 1)
    {
        return Math.Max(minimum, baseValue + shift);
    }

    private static int AdjustIncrease(int baseValue, int shift)
    {
        return Math.Max(0, baseValue + shift);
    }

    private static string AppendReachSummary(string text, OrderAdministrativeReachProfile administrativeReach)
    {
        if (string.IsNullOrWhiteSpace(administrativeReach.SummaryTail))
        {
            return text;
        }

        return $"{text}{administrativeReach.SummaryTail}";
    }

    private static void ApplyOrderInterventionReceipt(
        SettlementDisorderState settlement,
        string commandName,
        string summary,
        string outcome)
    {
        settlement.LastInterventionCommandCode = commandName;
        settlement.LastInterventionCommandLabel = DeterminePublicLifeCommandLabel(commandName);
        settlement.LastInterventionSummary = summary;
        settlement.LastInterventionOutcome = outcome;
        settlement.InterventionCarryoverMonths = 1;
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
