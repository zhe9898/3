using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed record OrderPublicLifeCommand
{
    public SettlementId SettlementId { get; init; }

    public string CommandName { get; init; } = string.Empty;

    public string CommandLabel { get; init; } = string.Empty;

    public int BenefitShift { get; init; }

    public int ShieldingShift { get; init; }

    public int BacklashShift { get; init; }

    public int LeakageShift { get; init; }

    public string ReachSummaryTail { get; init; } = string.Empty;
}

public sealed record OrderPublicLifeCommandResult
{
    public bool Accepted { get; init; }

    public SettlementId SettlementId { get; init; }

    public string CommandName { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string OutcomeSummary { get; init; } = string.Empty;
}

public sealed partial class OrderAndBanditryModule
{
    public OrderPublicLifeCommandResult HandlePublicLifeCommand(
        OrderAndBanditryState state,
        OrderPublicLifeCommand command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        SettlementDisorderState? settlement = state.Settlements
            .SingleOrDefault(entry => entry.SettlementId == command.SettlementId);
        string label = ResolveCommandLabel(command);

        if (settlement is null)
        {
            return new OrderPublicLifeCommandResult
            {
                Accepted = false,
                SettlementId = command.SettlementId,
                CommandName = command.CommandName,
                Label = label,
                Summary = $"此地暂无可调度的路面与治安节制：{command.SettlementId.Value}。",
            };
        }

        if (!TryApplyOrderPublicLifeCommand(settlement, command, label))
        {
            return new OrderPublicLifeCommandResult
            {
                Accepted = false,
                SettlementId = command.SettlementId,
                CommandName = command.CommandName,
                Label = label,
                Summary = $"地方治安不识此令：{command.CommandName}。",
            };
        }

        return new OrderPublicLifeCommandResult
        {
            Accepted = true,
            SettlementId = command.SettlementId,
            CommandName = command.CommandName,
            Label = label,
            Summary = settlement.LastInterventionSummary,
            OutcomeSummary = settlement.LastInterventionOutcome,
        };
    }

    private static bool TryApplyOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command,
        string label)
    {
        bool hasModifier = command.BenefitShift != 0
            || command.ShieldingShift != 0
            || command.BacklashShift != 0
            || command.LeakageShift != 0;

        if (hasModifier)
        {
            return TryApplyOfficeAwareOrderPublicLifeCommand(settlement, command, label);
        }

        switch (command.CommandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 8);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 5);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.RouteShielding = Clamp100(settlement.RouteShielding + 12);
                settlement.ImplementationDrag = Math.Max(0, settlement.ImplementationDrag - 4);
                settlement.LastPressureReason = "已先护住路报往来，河埠脚路暂得照看。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    "已催护一路，先把沿途报信与货脚照看起来。",
                    $"路压缓到{settlement.RoutePressure}，镇压之需减到{settlement.SuppressionDemand}，护路得力升到{settlement.RouteShielding}。");
                return true;
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 3);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 10);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 4);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 5);
                settlement.RouteShielding = Clamp100(settlement.RouteShielding + 16);
                settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel + 1, 0, 12);
                settlement.LastPressureReason = "已添雇巡丁，先把路口、渡头与夜巡补起来。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    "已添雇巡丁，先补路口、渡头与夜巡人手。",
                    $"路压缓到{settlement.RoutePressure}，地面不靖退到{settlement.DisorderPressure}，护路得力升到{settlement.RouteShielding}。");
                return true;
            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 12);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 6);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 8);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 4);
                settlement.CoercionRisk = Clamp100(settlement.CoercionRisk + 8);
                settlement.RetaliationRisk = Clamp100(settlement.RetaliationRisk + 14);
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + 2, 0, 12);
                settlement.LastPressureReason = "已严缉路匪，先压住明面扰动，但后手报复也会跟着抬头。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    "已严缉路匪，先压明面匪踪与拦路生事。",
                    $"盗压降到{settlement.BanditThreat}，路压降到{settlement.RoutePressure}，但反噬险已升到{settlement.RetaliationRisk}。");
                return true;
            case PlayerCommandNames.NegotiateWithOutlaws:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 4);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 3);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 10);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.BlackRoutePressure = Clamp100(settlement.BlackRoutePressure + 6);
                settlement.CoercionRisk = Math.Max(0, settlement.CoercionRisk - 6);
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - 12);
                settlement.LastPressureReason = "已遣人议路，先换一路暂安，但私下分流会更容易坐大。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    "已遣人议路，先换渡头与路口一时安静。",
                    $"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但私路压抬到{settlement.BlackRoutePressure}。");
                return true;
            case PlayerCommandNames.TolerateDisorder:
                settlement.BanditThreat = Clamp100(settlement.BanditThreat + 4);
                settlement.RoutePressure = Clamp100(settlement.RoutePressure + 6);
                settlement.DisorderPressure = Clamp100(settlement.DisorderPressure + 5);
                settlement.BlackRoutePressure = Clamp100(settlement.BlackRoutePressure + 8);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 6);
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - 8);
                settlement.RouteShielding = Math.Max(0, settlement.RouteShielding - 4);
                settlement.LastPressureReason = "已暂缓穷追，先不把地面逼到立刻翻脸，但路面与私下手脚会继续滋长。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    "已暂缓穷追，先把差役与地面都收一收。",
                    $"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但路压涨到{settlement.RoutePressure}，私路压涨到{settlement.BlackRoutePressure}。");
                return true;
            default:
                return false;
        }
    }

    private static bool TryApplyOfficeAwareOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command,
        string label)
    {
        switch (command.CommandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(8, command.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(5, command.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(2, command.BenefitShift, 0));
                settlement.RouteShielding = Clamp100(settlement.RouteShielding + AdjustIncrease(12, command.ShieldingShift));
                settlement.ImplementationDrag = Math.Max(0, settlement.ImplementationDrag - AdjustReduction(4, Math.Max(0, command.BenefitShift)));
                settlement.LastPressureReason = AppendReachSummary("已先护住路报往来，河埠脚路暂得照看。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("已催护一路，先把沿途报信与货脚照看起来。", command),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，镇压之需减到{settlement.SuppressionDemand}，护路得力升到{settlement.RouteShielding}。", command));
                return true;
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(3, command.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(10, command.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(4, command.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(5, command.BenefitShift));
                settlement.RouteShielding = Clamp100(settlement.RouteShielding + AdjustIncrease(16, command.ShieldingShift));
                settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel + 1, 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已添雇巡丁，先把路口、渡头与夜巡补起来。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("已添雇巡丁，先补路口、渡头与夜巡人手。", command),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，地面不靖退到{settlement.DisorderPressure}，护路得力升到{settlement.RouteShielding}。", command));
                return true;
            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(12, command.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(6, command.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(8, command.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(4, command.BenefitShift));
                settlement.CoercionRisk = Clamp100(settlement.CoercionRisk + AdjustIncrease(8, command.BacklashShift));
                settlement.RetaliationRisk = Clamp100(settlement.RetaliationRisk + AdjustIncrease(14, command.BacklashShift));
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + AdjustIncrease(2, Math.Max(0, command.BenefitShift)), 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已严缉路匪，先压住明面扰动，但后手报复也会跟着抬头。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("已严缉路匪，先压明面匪踪与拦路生事。", command),
                    AppendReachSummary($"盗压降到{settlement.BanditThreat}，路压降到{settlement.RoutePressure}，但反噬险已升到{settlement.RetaliationRisk}。", command));
                return true;
            case PlayerCommandNames.NegotiateWithOutlaws:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(4, command.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(3, command.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(10, command.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(2, command.BenefitShift, 0));
                settlement.BlackRoutePressure = Clamp100(settlement.BlackRoutePressure + AdjustIncrease(6, command.LeakageShift));
                settlement.CoercionRisk = Math.Max(0, settlement.CoercionRisk - AdjustReduction(6, command.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - AdjustReduction(12, command.BenefitShift));
                settlement.LastPressureReason = AppendReachSummary("已遣人议路，先换一路暂安，但私下分流会更容易坐大。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("已遣人议路，先换渡头与路口一时安静。", command),
                    AppendReachSummary($"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但私路压抬到{settlement.BlackRoutePressure}。", command));
                return true;
            case PlayerCommandNames.TolerateDisorder:
                settlement.BanditThreat = Clamp100(settlement.BanditThreat + AdjustIncrease(4, command.LeakageShift));
                settlement.RoutePressure = Clamp100(settlement.RoutePressure + AdjustIncrease(6, command.LeakageShift));
                settlement.DisorderPressure = Clamp100(settlement.DisorderPressure + AdjustIncrease(5, command.LeakageShift));
                settlement.BlackRoutePressure = Clamp100(settlement.BlackRoutePressure + AdjustIncrease(8, command.LeakageShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(6, command.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - AdjustReduction(8, command.BenefitShift));
                settlement.RouteShielding = Math.Max(0, settlement.RouteShielding - Math.Max(0, 4 - Math.Max(0, command.BenefitShift)));
                settlement.LastPressureReason = AppendReachSummary("已暂缓穷追，先不把地面逼到立刻翻脸，但路面与私下手脚会继续滋长。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("已暂缓穷追，先把差役与地面都收一收。", command),
                    AppendReachSummary($"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但路压涨到{settlement.RoutePressure}，私路压涨到{settlement.BlackRoutePressure}。", command));
                return true;
            default:
                return false;
        }
    }

    private static string ResolveCommandLabel(OrderPublicLifeCommand command)
    {
        return string.IsNullOrWhiteSpace(command.CommandLabel)
            ? command.CommandName
            : command.CommandLabel;
    }

    private static string AppendReachSummary(string text, OrderPublicLifeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ReachSummaryTail))
        {
            return text;
        }

        return $"{text}{command.ReachSummaryTail}";
    }

    private static void ApplyOrderInterventionReceipt(
        SettlementDisorderState settlement,
        string commandName,
        string commandLabel,
        string summary,
        string outcome)
    {
        settlement.LastInterventionCommandCode = commandName;
        settlement.LastInterventionCommandLabel = commandLabel;
        settlement.LastInterventionSummary = summary;
        settlement.LastInterventionOutcome = outcome;
        settlement.InterventionCarryoverMonths = 1;
    }

    private static int Clamp100(int value)
    {
        return Math.Clamp(value, 0, 100);
    }

    private static int AdjustReduction(int baseValue, int shift, int minimum = 1)
    {
        return Math.Max(minimum, baseValue + shift);
    }

    private static int AdjustIncrease(int baseValue, int shift)
    {
        return Math.Max(0, baseValue + shift);
    }
}
