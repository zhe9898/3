using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryCommandContext
{
    public OrderAndBanditryState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();

    public IOfficeAndCareerQueries? OfficeQueries { get; init; }
}

public static class OrderAndBanditryCommandResolver
{
    public static PlayerCommandResult IssueIntent(OrderAndBanditryCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.State);
        ArgumentNullException.ThrowIfNull(context.Command);

        PlayerCommandRequest command = context.Command;
        SettlementDisorderState? settlement = context.State.Settlements
            .SingleOrDefault(entry => entry.SettlementId == command.SettlementId);
        if (settlement is null)
        {
            return BuildRejectedResult(command, $"此地暂无可调度的路面与治安节制：{command.SettlementId.Value}。");
        }

        OrderAdministrativeReachProfile administrativeReach = ResolveAdministrativeReach(context.OfficeQueries, command.SettlementId);
        if (!TryApplyOrderPublicLifeCommand(settlement, command.CommandName, administrativeReach))
        {
            return BuildRejectedResult(command, $"地方治安不识此令：{command.CommandName}。");
        }

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

    public static string DeterminePublicLifeCommandLabel(string commandName)
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
            PlayerCommandNames.EscortRoadReport => "催护一路",
            _ => commandName,
        };
    }

    public static string DetermineAdministrativeReachExecutionSummary(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        return jurisdiction is null
            ? OrderAdministrativeReachProfile.Neutral.ExecutionSummary
            : EvaluateAdministrativeReach(jurisdiction).ExecutionSummary;
    }

    private static PlayerCommandResult BuildRejectedResult(PlayerCommandRequest command, string summary)
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
            Summary = summary,
        };
    }

    private static OrderAdministrativeReachProfile ResolveAdministrativeReach(
        IOfficeAndCareerQueries? officeQueries,
        SettlementId settlementId)
    {
        if (officeQueries is null)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        try
        {
            return EvaluateAdministrativeReach(officeQueries.GetRequiredJurisdiction(settlementId));
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }
    }

    private static OrderAdministrativeReachProfile EvaluateAdministrativeReach(JurisdictionAuthoritySnapshot jurisdiction)
    {
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
            return new OrderAdministrativeReachProfile(
                3,
                5,
                -4,
                -3,
                "县署肯出手，文移与差役都跟得上。",
                "县署肯出手，此令多半能照行到底。");
        }

        if (netSignal >= 15)
        {
            return new OrderAdministrativeReachProfile(
                1,
                2,
                -2,
                -1,
                "县署还能接得住，文移差役尚能随令。",
                "县署还能接得住，此令大体跟得上。");
        }

        if (netSignal <= -40)
        {
            return new OrderAdministrativeReachProfile(
                -3,
                -5,
                4,
                3,
                "县署拥案未解，文移不畅，路上只得勉强敷衍。",
                "县署拥案未解，此令多半只落在文面，地面跟得慢。");
        }

        if (netSignal <= -15)
        {
            return new OrderAdministrativeReachProfile(
                -1,
                -2,
                2,
                1,
                "县署案牍偏重，差役跟得慢，只能先做半套。",
                "县署案牍偏重，此令常要拖成半套。");
        }

        return OrderAdministrativeReachProfile.Neutral;
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
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - OrderCommandResolutionMath.AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - OrderCommandResolutionMath.AdjustReduction(5, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - OrderCommandResolutionMath.AdjustReduction(2, administrativeReach.BenefitShift, 0));
                settlement.RouteShielding = OrderCommandResolutionMath.Clamp100(settlement.RouteShielding + OrderCommandResolutionMath.AdjustIncrease(12, administrativeReach.ShieldingShift));
                settlement.ImplementationDrag = Math.Max(0, settlement.ImplementationDrag - OrderCommandResolutionMath.AdjustReduction(4, Math.Max(0, administrativeReach.BenefitShift)));
                settlement.LastPressureReason = AppendReachSummary("已先护住路报往来，河埠脚路暂得照看。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已催护一路，先把沿途报信与货脚照看起来。", administrativeReach),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，镇压之需减到{settlement.SuppressionDemand}，护路得力升到{settlement.RouteShielding}。", administrativeReach));
                return true;
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - OrderCommandResolutionMath.AdjustReduction(3, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - OrderCommandResolutionMath.AdjustReduction(10, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - OrderCommandResolutionMath.AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - OrderCommandResolutionMath.AdjustReduction(5, administrativeReach.BenefitShift));
                settlement.RouteShielding = OrderCommandResolutionMath.Clamp100(settlement.RouteShielding + OrderCommandResolutionMath.AdjustIncrease(16, administrativeReach.ShieldingShift));
                settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel + 1, 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已添雇巡丁，先把路口、渡头与夜巡补起来。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已添雇巡丁，先补路口、渡头与夜巡人手。", administrativeReach),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，地面不靖退到{settlement.DisorderPressure}，护路得力升到{settlement.RouteShielding}。", administrativeReach));
                return true;
            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - OrderCommandResolutionMath.AdjustReduction(12, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - OrderCommandResolutionMath.AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - OrderCommandResolutionMath.AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - OrderCommandResolutionMath.AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.CoercionRisk = OrderCommandResolutionMath.Clamp100(settlement.CoercionRisk + OrderCommandResolutionMath.AdjustIncrease(8, administrativeReach.BacklashShift));
                settlement.RetaliationRisk = OrderCommandResolutionMath.Clamp100(settlement.RetaliationRisk + OrderCommandResolutionMath.AdjustIncrease(14, administrativeReach.BacklashShift));
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + OrderCommandResolutionMath.AdjustIncrease(2, Math.Max(0, administrativeReach.BenefitShift)), 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已严缉路匪，先压住明面扰动，但后手报复也会跟着抬头。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已严缉路匪，先压明面匪踪与拦路生事。", administrativeReach),
                    AppendReachSummary($"盗压降到{settlement.BanditThreat}，路压降到{settlement.RoutePressure}，但反噬险已升到{settlement.RetaliationRisk}。", administrativeReach));
                return true;
            case PlayerCommandNames.NegotiateWithOutlaws:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - OrderCommandResolutionMath.AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - OrderCommandResolutionMath.AdjustReduction(3, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - OrderCommandResolutionMath.AdjustReduction(10, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - OrderCommandResolutionMath.AdjustReduction(2, administrativeReach.BenefitShift, 0));
                settlement.BlackRoutePressure = OrderCommandResolutionMath.Clamp100(settlement.BlackRoutePressure + OrderCommandResolutionMath.AdjustIncrease(6, administrativeReach.LeakageShift));
                settlement.CoercionRisk = Math.Max(0, settlement.CoercionRisk - OrderCommandResolutionMath.AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - OrderCommandResolutionMath.AdjustReduction(12, administrativeReach.BenefitShift));
                settlement.LastPressureReason = AppendReachSummary("已遣人议路，先换一路暂安，但私下分流会更容易坐大。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已遣人议路，先换渡头与路口一时安静。", administrativeReach),
                    AppendReachSummary($"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但私路压抬到{settlement.BlackRoutePressure}。", administrativeReach));
                return true;
            case PlayerCommandNames.TolerateDisorder:
                settlement.BanditThreat = OrderCommandResolutionMath.Clamp100(settlement.BanditThreat + OrderCommandResolutionMath.AdjustIncrease(4, administrativeReach.LeakageShift));
                settlement.RoutePressure = OrderCommandResolutionMath.Clamp100(settlement.RoutePressure + OrderCommandResolutionMath.AdjustIncrease(6, administrativeReach.LeakageShift));
                settlement.DisorderPressure = OrderCommandResolutionMath.Clamp100(settlement.DisorderPressure + OrderCommandResolutionMath.AdjustIncrease(5, administrativeReach.LeakageShift));
                settlement.BlackRoutePressure = OrderCommandResolutionMath.Clamp100(settlement.BlackRoutePressure + OrderCommandResolutionMath.AdjustIncrease(8, administrativeReach.LeakageShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - OrderCommandResolutionMath.AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - OrderCommandResolutionMath.AdjustReduction(8, administrativeReach.BenefitShift));
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

    private static string AppendReachSummary(string text, OrderAdministrativeReachProfile administrativeReach)
    {
        return string.IsNullOrWhiteSpace(administrativeReach.SummaryTail)
            ? text
            : $"{text}{administrativeReach.SummaryTail}";
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

    private readonly record struct OrderAdministrativeReachProfile(
        int BenefitShift,
        int ShieldingShift,
        int BacklashShift,
        int LeakageShift,
        string SummaryTail,
        string ExecutionSummary)
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

    private static class OrderCommandResolutionMath
    {
        public static int Clamp100(int value)
        {
            return Math.Clamp(value, 0, 100);
        }

        public static int AdjustReduction(int baseValue, int shift, int minimum = 1)
        {
            return Math.Max(minimum, baseValue + shift);
        }

        public static int AdjustIncrease(int baseValue, int shift)
        {
            return Math.Max(0, baseValue + shift);
        }
    }
}
