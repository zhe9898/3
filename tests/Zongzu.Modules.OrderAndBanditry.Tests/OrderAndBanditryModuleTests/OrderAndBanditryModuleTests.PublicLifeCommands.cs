using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.OrderAndBanditry;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

public sealed partial class OrderAndBanditryModuleTests
{
    [Test]
    public void HandlePublicLifeCommand_FundLocalWatch_AppliesOwnedReceiptAndCarryover()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            BanditThreat = 40,
            RoutePressure = 44,
            SuppressionDemand = 30,
            DisorderPressure = 31,
            RouteShielding = 5,
            ResponseActivationLevel = 2,
        });

        OrderPublicLifeCommandResult result = module.HandlePublicLifeCommand(
            state,
            new OrderPublicLifeCommand
            {
                SettlementId = new SettlementId(1),
                CommandName = PlayerCommandNames.FundLocalWatch,
                CommandLabel = "添雇巡丁",
            });

        SettlementDisorderState settlement = state.Settlements.Single();
        Assert.That(result.Accepted, Is.True);
        Assert.That(result.CommandName, Is.EqualTo(PlayerCommandNames.FundLocalWatch));
        Assert.That(result.Label, Is.EqualTo("添雇巡丁"));
        Assert.That(settlement.BanditThreat, Is.EqualTo(37));
        Assert.That(settlement.RoutePressure, Is.EqualTo(34));
        Assert.That(settlement.SuppressionDemand, Is.EqualTo(26));
        Assert.That(settlement.DisorderPressure, Is.EqualTo(26));
        Assert.That(settlement.RouteShielding, Is.EqualTo(21));
        Assert.That(settlement.ResponseActivationLevel, Is.EqualTo(3));
        Assert.That(settlement.LastInterventionCommandCode, Is.EqualTo(PlayerCommandNames.FundLocalWatch));
        Assert.That(settlement.LastInterventionCommandLabel, Is.EqualTo("添雇巡丁"));
        Assert.That(settlement.LastInterventionSummary, Is.Not.Empty);
        Assert.That(settlement.LastInterventionOutcome, Does.Contain("护路得力"));
        Assert.That(settlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Accepted));
        Assert.That(settlement.LastInterventionRefusalCode, Is.Empty);
        Assert.That(settlement.LastInterventionPartialCode, Is.Empty);
        Assert.That(settlement.LastInterventionTraceCode, Is.EqualTo(OrderInterventionTraceCodes.AcceptedFollowThrough));
        Assert.That(settlement.InterventionCarryoverMonths, Is.EqualTo(1));
        Assert.That(settlement.RefusalCarryoverMonths, Is.EqualTo(0));
        Assert.That(result.OutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Accepted));
    }

    [Test]
    public void HandlePublicLifeCommand_RejectsMissingSettlementAndUnknownCommandWithoutMutation()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            BanditThreat = 40,
            RoutePressure = 44,
            SuppressionDemand = 30,
            DisorderPressure = 31,
        });

        OrderPublicLifeCommandResult missing = module.HandlePublicLifeCommand(
            state,
            new OrderPublicLifeCommand
            {
                SettlementId = new SettlementId(2),
                CommandName = PlayerCommandNames.FundLocalWatch,
                CommandLabel = "添雇巡丁",
            });
        OrderPublicLifeCommandResult unknown = module.HandlePublicLifeCommand(
            state,
            new OrderPublicLifeCommand
            {
                SettlementId = new SettlementId(1),
                CommandName = "OpenGodPanel",
                CommandLabel = "OpenGodPanel",
            });

        SettlementDisorderState settlement = state.Settlements.Single();
        Assert.That(missing.Accepted, Is.False);
        Assert.That(missing.Summary, Does.Contain("暂无可调度"));
        Assert.That(missing.OutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Refused));
        Assert.That(missing.RefusalCode, Is.EqualTo(OrderInterventionRefusalCodes.MissingSettlement));
        Assert.That(missing.TraceCode, Is.EqualTo(OrderInterventionTraceCodes.MissingSettlement));
        Assert.That(unknown.Accepted, Is.False);
        Assert.That(unknown.Summary, Does.Contain("不识此令"));
        Assert.That(unknown.OutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Refused));
        Assert.That(unknown.RefusalCode, Is.EqualTo(OrderInterventionRefusalCodes.UnknownCommand));
        Assert.That(unknown.TraceCode, Is.EqualTo(OrderInterventionTraceCodes.UnknownCommand));
        Assert.That(settlement.BanditThreat, Is.EqualTo(40));
        Assert.That(settlement.RoutePressure, Is.EqualTo(44));
        Assert.That(settlement.SuppressionDemand, Is.EqualTo(30));
        Assert.That(settlement.DisorderPressure, Is.EqualTo(31));
        Assert.That(settlement.LastInterventionCommandCode, Is.Empty);
        Assert.That(settlement.InterventionCarryoverMonths, Is.EqualTo(0));
        Assert.That(settlement.RefusalCarryoverMonths, Is.EqualTo(0));
    }

    [Test]
    public void HandlePublicLifeCommand_FundLocalWatch_PartialLandingWritesStructuredOrderTrace()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            BanditThreat = 40,
            RoutePressure = 44,
            SuppressionDemand = 30,
            DisorderPressure = 31,
            ImplementationDrag = 50,
            RouteShielding = 12,
        });

        OrderPublicLifeCommandResult result = module.HandlePublicLifeCommand(
            state,
            new OrderPublicLifeCommand
            {
                SettlementId = new SettlementId(1),
                CommandName = PlayerCommandNames.FundLocalWatch,
                CommandLabel = "添雇巡丁",
                BenefitShift = -1,
                ShieldingShift = -2,
                ReachSummaryTail = "县门拖延，只肯先记半套。",
            });

        SettlementDisorderState settlement = state.Settlements.Single();
        Assert.That(result.Accepted, Is.True);
        Assert.That(result.OutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Partial));
        Assert.That(result.PartialCode, Is.EqualTo(OrderInterventionPartialCodes.CountyDrag));
        Assert.That(result.TraceCode, Is.EqualTo(OrderInterventionTraceCodes.WatchCountyDrag));
        Assert.That(settlement.LastInterventionCommandCode, Is.EqualTo(PlayerCommandNames.FundLocalWatch));
        Assert.That(settlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Partial));
        Assert.That(settlement.LastInterventionPartialCode, Is.EqualTo(OrderInterventionPartialCodes.CountyDrag));
        Assert.That(settlement.LastInterventionRefusalCode, Is.Empty);
        Assert.That(settlement.InterventionCarryoverMonths, Is.EqualTo(1));
        Assert.That(settlement.RefusalCarryoverMonths, Is.EqualTo(0));
        Assert.That(settlement.ImplementationDrag, Is.GreaterThan(50));
    }

    [Test]
    public void HandlePublicLifeCommand_SuppressBanditry_RefusalWritesStructuredOrderTrace()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = BuildSuppressionState();
        SettlementDisorderState settlement = state.Settlements.Single();
        settlement.ImplementationDrag = 60;
        settlement.RetaliationRisk = 58;

        OrderPublicLifeCommandResult result = module.HandlePublicLifeCommand(
            state,
            new OrderPublicLifeCommand
            {
                SettlementId = new SettlementId(1),
                CommandName = PlayerCommandNames.SuppressBanditry,
                CommandLabel = "严缉路匪",
                BenefitShift = -3,
                BacklashShift = 4,
                ReachSummaryTail = "胥吏拖延，地方不肯真动。",
            });

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.OutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Refused));
        Assert.That(result.RefusalCode, Is.EqualTo(OrderInterventionRefusalCodes.SuppressionRefused));
        Assert.That(result.TraceCode, Is.EqualTo(OrderInterventionTraceCodes.SuppressionGroundRefusal));
        Assert.That(settlement.LastInterventionCommandCode, Is.EqualTo(PlayerCommandNames.SuppressBanditry));
        Assert.That(settlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Refused));
        Assert.That(settlement.LastInterventionRefusalCode, Is.EqualTo(OrderInterventionRefusalCodes.SuppressionRefused));
        Assert.That(settlement.LastInterventionPartialCode, Is.Empty);
        Assert.That(settlement.InterventionCarryoverMonths, Is.EqualTo(0));
        Assert.That(settlement.RefusalCarryoverMonths, Is.EqualTo(1));
        Assert.That(settlement.RetaliationRisk, Is.GreaterThan(58));
    }

    [Test]
    public void HandlePublicLifeCommand_OfficeReachChangesSuppressionBacklashAndReceipt()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState supportedState = BuildSuppressionState();
        OrderAndBanditryState cloggedState = BuildSuppressionState();

        OrderPublicLifeCommandResult supported = module.HandlePublicLifeCommand(
            supportedState,
            new OrderPublicLifeCommand
            {
                SettlementId = new SettlementId(1),
                CommandName = PlayerCommandNames.SuppressBanditry,
                CommandLabel = "严缉路匪",
                BenefitShift = 3,
                ShieldingShift = 5,
                BacklashShift = -4,
                LeakageShift = -3,
                ReachSummaryTail = "县署肯出手，文移与差役都跟得上。",
            });
        OrderPublicLifeCommandResult clogged = module.HandlePublicLifeCommand(
            cloggedState,
            new OrderPublicLifeCommand
            {
                SettlementId = new SettlementId(1),
                CommandName = PlayerCommandNames.SuppressBanditry,
                CommandLabel = "严缉路匪",
                BenefitShift = -3,
                ShieldingShift = -5,
                BacklashShift = 4,
                LeakageShift = 3,
                ReachSummaryTail = "县署拥案未解，路上只得勉强敷衍。",
            });

        SettlementDisorderState supportedSettlement = supportedState.Settlements.Single();
        SettlementDisorderState cloggedSettlement = cloggedState.Settlements.Single();
        Assert.That(supported.Accepted, Is.True);
        Assert.That(clogged.Accepted, Is.True);
        Assert.That(supported.OutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Accepted));
        Assert.That(clogged.OutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Partial));
        Assert.That(supportedSettlement.BanditThreat, Is.LessThan(cloggedSettlement.BanditThreat));
        Assert.That(supportedSettlement.RoutePressure, Is.LessThan(cloggedSettlement.RoutePressure));
        Assert.That(supportedSettlement.RetaliationRisk, Is.LessThan(cloggedSettlement.RetaliationRisk));
        Assert.That(supported.Summary, Does.Contain("县署肯出手"));
        Assert.That(clogged.Summary, Does.Contain("县署拥案未解"));
    }

    private static OrderAndBanditryState BuildSuppressionState()
    {
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            BanditThreat = 48,
            RoutePressure = 42,
            SuppressionDemand = 44,
            DisorderPressure = 36,
            CoercionRisk = 18,
            RetaliationRisk = 12,
        });
        return state;
    }
}
