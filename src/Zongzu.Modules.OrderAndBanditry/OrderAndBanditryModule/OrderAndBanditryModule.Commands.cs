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

    public string OutcomeCode { get; init; } = string.Empty;

    public string RefusalCode { get; init; } = string.Empty;

    public string PartialCode { get; init; } = string.Empty;

    public string TraceCode { get; init; } = string.Empty;
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
                OutcomeCode = OrderInterventionOutcomeCodes.Refused,
                RefusalCode = OrderInterventionRefusalCodes.MissingSettlement,
                TraceCode = OrderInterventionTraceCodes.MissingSettlement,
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
                OutcomeCode = OrderInterventionOutcomeCodes.Refused,
                RefusalCode = OrderInterventionRefusalCodes.UnknownCommand,
                TraceCode = OrderInterventionTraceCodes.UnknownCommand,
            };
        }

        bool accepted = !string.Equals(
            settlement.LastInterventionOutcomeCode,
            OrderInterventionOutcomeCodes.Refused,
            StringComparison.Ordinal);
        return new OrderPublicLifeCommandResult
        {
            Accepted = accepted,
            SettlementId = command.SettlementId,
            CommandName = command.CommandName,
            Label = label,
            Summary = settlement.LastInterventionSummary,
            OutcomeSummary = settlement.LastInterventionOutcome,
            OutcomeCode = settlement.LastInterventionOutcomeCode,
            RefusalCode = settlement.LastInterventionRefusalCode,
            PartialCode = settlement.LastInterventionPartialCode,
            TraceCode = settlement.LastInterventionTraceCode,
        };
    }

    private static bool TryApplyOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command,
        string label)
    {
        OrderLandingDecision landingDecision = DetermineLandingDecision(settlement, command);
        if (landingDecision.IsRefused)
        {
            return ApplyRefusedOrderPublicLifeCommand(settlement, command, label, landingDecision);
        }

        if (landingDecision.IsPartial)
        {
            return ApplyPartialOrderPublicLifeCommand(settlement, command, label, landingDecision);
        }

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
        ApplyOrderInterventionReceipt(
            settlement,
            commandName,
            commandLabel,
            summary,
            outcome,
            OrderInterventionOutcomeCodes.Accepted,
            string.Empty,
            string.Empty,
            OrderInterventionTraceCodes.AcceptedFollowThrough,
            interventionCarryoverMonths: 1,
            refusalCarryoverMonths: 0);
    }

    private static void ApplyOrderInterventionReceipt(
        SettlementDisorderState settlement,
        string commandName,
        string commandLabel,
        string summary,
        string outcome,
        string outcomeCode,
        string refusalCode,
        string partialCode,
        string traceCode,
        int interventionCarryoverMonths,
        int refusalCarryoverMonths)
    {
        settlement.LastInterventionCommandCode = commandName;
        settlement.LastInterventionCommandLabel = commandLabel;
        settlement.LastInterventionSummary = summary;
        settlement.LastInterventionOutcome = outcome;
        settlement.LastInterventionOutcomeCode = outcomeCode;
        settlement.LastInterventionRefusalCode = refusalCode;
        settlement.LastInterventionPartialCode = partialCode;
        settlement.LastInterventionTraceCode = traceCode;
        settlement.InterventionCarryoverMonths = Math.Clamp(interventionCarryoverMonths, 0, 1);
        settlement.RefusalCarryoverMonths = Math.Clamp(refusalCarryoverMonths, 0, 1);
    }

    private static OrderLandingDecision DetermineLandingDecision(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command)
    {
        return command.CommandName switch
        {
            PlayerCommandNames.FundLocalWatch => DetermineWatchLandingDecision(settlement, command),
            PlayerCommandNames.SuppressBanditry => DetermineSuppressionLandingDecision(settlement, command),
            _ => OrderLandingDecision.Accepted,
        };
    }

    private static OrderLandingDecision DetermineWatchLandingDecision(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command)
    {
        if ((command.BenefitShift <= -3 && settlement.ImplementationDrag >= 55)
            || (settlement.CoercionRisk >= 72 && settlement.RouteShielding <= 12))
        {
            return new OrderLandingDecision(
                OrderInterventionOutcomeCodes.Refused,
                OrderInterventionRefusalCodes.WatchmenRefused,
                string.Empty,
                OrderInterventionTraceCodes.WatchGroundRefusal);
        }

        if (command.BenefitShift < 0
            || command.ShieldingShift < 0
            || settlement.ImplementationDrag >= 45
            || settlement.RetaliationRisk >= 50
            || settlement.CoercionRisk >= 55)
        {
            string partialCode = settlement.CoercionRisk >= 55 || settlement.RetaliationRisk >= 50
                ? OrderInterventionPartialCodes.WatchMisread
                : OrderInterventionPartialCodes.CountyDrag;
            string traceCode = string.Equals(partialCode, OrderInterventionPartialCodes.WatchMisread, StringComparison.Ordinal)
                ? OrderInterventionTraceCodes.WatchGroundRefusal
                : OrderInterventionTraceCodes.WatchCountyDrag;
            return new OrderLandingDecision(
                OrderInterventionOutcomeCodes.Partial,
                string.Empty,
                partialCode,
                traceCode);
        }

        return OrderLandingDecision.Accepted;
    }

    private static OrderLandingDecision DetermineSuppressionLandingDecision(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command)
    {
        if ((command.BenefitShift <= -3 && (settlement.RetaliationRisk >= 55 || settlement.ImplementationDrag >= 55))
            || (settlement.CoercionRisk >= 80 && settlement.RetaliationRisk >= 60))
        {
            return new OrderLandingDecision(
                OrderInterventionOutcomeCodes.Refused,
                OrderInterventionRefusalCodes.SuppressionRefused,
                string.Empty,
                OrderInterventionTraceCodes.SuppressionGroundRefusal);
        }

        if (command.BenefitShift < 0
            || command.BacklashShift > 0
            || settlement.RetaliationRisk >= 45
            || settlement.CoercionRisk >= 55
            || settlement.ImplementationDrag >= 45)
        {
            string partialCode = settlement.RetaliationRisk >= 45 || settlement.CoercionRisk >= 55 || command.BacklashShift > 0
                ? OrderInterventionPartialCodes.SuppressionBacklash
                : OrderInterventionPartialCodes.CountyDrag;
            string traceCode = string.Equals(partialCode, OrderInterventionPartialCodes.SuppressionBacklash, StringComparison.Ordinal)
                ? OrderInterventionTraceCodes.SuppressionBacklash
                : OrderInterventionTraceCodes.SuppressionCountyDrag;
            return new OrderLandingDecision(
                OrderInterventionOutcomeCodes.Partial,
                string.Empty,
                partialCode,
                traceCode);
        }

        return OrderLandingDecision.Accepted;
    }

    private static bool ApplyPartialOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command,
        string label,
        OrderLandingDecision landingDecision)
    {
        switch (command.CommandName)
        {
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(1, command.BenefitShift, 0));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(4, command.BenefitShift, 0));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(2, command.BenefitShift, 0));
                settlement.DisorderPressure = Clamp100(settlement.DisorderPressure + Math.Max(0, command.LeakageShift) - 1);
                settlement.RouteShielding = Clamp100(settlement.RouteShielding + AdjustIncrease(6, command.ShieldingShift));
                settlement.ImplementationDrag = Clamp100(settlement.ImplementationDrag + 8 + Math.Max(0, -command.BenefitShift));
                settlement.RetaliationRisk = Clamp100(settlement.RetaliationRisk + 4 + Math.Max(0, command.BacklashShift));
                settlement.LastPressureReason = AppendReachSummary("添雇巡丁只落了半套，县门与地面仍在互相推慢。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("添雇巡丁半落地，路口、渡头和夜巡有人应下，却未能全数到位。", command),
                    $"路压暂缓到{settlement.RoutePressure}，护路得力半成，县门拖延与地面误读仍留下后账。",
                    landingDecision.OutcomeCode,
                    landingDecision.RefusalCode,
                    landingDecision.PartialCode,
                    landingDecision.TraceCode,
                    interventionCarryoverMonths: 1,
                    refusalCarryoverMonths: 0);
                return true;

            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(5, command.BenefitShift, 0));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(2, command.BenefitShift, 0));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(3, command.BenefitShift, 0));
                settlement.DisorderPressure = Clamp100(settlement.DisorderPressure + Math.Max(0, command.LeakageShift) - 1);
                settlement.CoercionRisk = Clamp100(settlement.CoercionRisk + AdjustIncrease(10, command.BacklashShift));
                settlement.RetaliationRisk = Clamp100(settlement.RetaliationRisk + AdjustIncrease(12, command.BacklashShift));
                settlement.ImplementationDrag = Clamp100(settlement.ImplementationDrag + 6 + Math.Max(0, -command.BenefitShift));
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + 1, 0, 12);
                settlement.LastPressureReason = AppendReachSummary("严缉路匪只压住明面，胥吏拖延与地面反噬仍在。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("严缉路匪半落地，明面路匪被逼退一截，暗处反噬和误伤脚户仍起。", command),
                    $"盗压暂降到{settlement.BanditThreat}，但反噬险升到{settlement.RetaliationRisk}，胥吏拖延与后账仍在。",
                    landingDecision.OutcomeCode,
                    landingDecision.RefusalCode,
                    landingDecision.PartialCode,
                    landingDecision.TraceCode,
                    interventionCarryoverMonths: 1,
                    refusalCarryoverMonths: 0);
                return true;

            default:
                return false;
        }
    }

    private static bool ApplyRefusedOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        OrderPublicLifeCommand command,
        string label,
        OrderLandingDecision landingDecision)
    {
        switch (command.CommandName)
        {
            case PlayerCommandNames.FundLocalWatch:
                settlement.RoutePressure = Clamp100(settlement.RoutePressure + 2);
                settlement.DisorderPressure = Clamp100(settlement.DisorderPressure + 3);
                settlement.ImplementationDrag = Clamp100(settlement.ImplementationDrag + 8 + Math.Max(0, -command.BenefitShift));
                settlement.RetaliationRisk = Clamp100(settlement.RetaliationRisk + 6 + Math.Max(0, command.BacklashShift));
                settlement.RouteShielding = Math.Max(0, settlement.RouteShielding - 2);
                settlement.LastPressureReason = AppendReachSummary("添雇巡丁未被地方接住，巡丁与脚户都不肯把本户担保当成实令。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("添雇巡丁被拒，县门未落地，脚户与巡丁只把话带成了半截。", command),
                    $"护路未成，路压反升到{settlement.RoutePressure}，县门拖延和本户担保失败留下后账。",
                    landingDecision.OutcomeCode,
                    landingDecision.RefusalCode,
                    landingDecision.PartialCode,
                    landingDecision.TraceCode,
                    interventionCarryoverMonths: 0,
                    refusalCarryoverMonths: 1);
                return true;

            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Clamp100(settlement.BanditThreat + 3);
                settlement.RoutePressure = Clamp100(settlement.RoutePressure + 2);
                settlement.DisorderPressure = Clamp100(settlement.DisorderPressure + 2);
                settlement.CoercionRisk = Clamp100(settlement.CoercionRisk + 8 + Math.Max(0, command.BacklashShift));
                settlement.RetaliationRisk = Clamp100(settlement.RetaliationRisk + 15 + Math.Max(0, command.BacklashShift));
                settlement.ImplementationDrag = Clamp100(settlement.ImplementationDrag + 10 + Math.Max(0, -command.BenefitShift));
                settlement.LastPressureReason = AppendReachSummary("严缉路匪未被地方接住，县门不肯真动，地面反先听成了本户逼迫。", command);
                ApplyOrderInterventionReceipt(
                    settlement,
                    command.CommandName,
                    label,
                    AppendReachSummary("严缉路匪被拒，县门未落地，胥吏拖延，地面先起反噬。", command),
                    $"镇压未成，盗压升到{settlement.BanditThreat}，反噬险升到{settlement.RetaliationRisk}，后账仍在。",
                    landingDecision.OutcomeCode,
                    landingDecision.RefusalCode,
                    landingDecision.PartialCode,
                    landingDecision.TraceCode,
                    interventionCarryoverMonths: 0,
                    refusalCarryoverMonths: 1);
                return true;

            default:
                return false;
        }
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

    private readonly record struct OrderLandingDecision(
        string OutcomeCode,
        string RefusalCode,
        string PartialCode,
        string TraceCode)
    {
        public static readonly OrderLandingDecision Accepted = new(
            OrderInterventionOutcomeCodes.Accepted,
            string.Empty,
            string.Empty,
            OrderInterventionTraceCodes.AcceptedFollowThrough);

        public bool IsRefused => string.Equals(OutcomeCode, OrderInterventionOutcomeCodes.Refused, StringComparison.Ordinal);

        public bool IsPartial => string.Equals(OutcomeCode, OrderInterventionOutcomeCodes.Partial, StringComparison.Ordinal);
    }
}
