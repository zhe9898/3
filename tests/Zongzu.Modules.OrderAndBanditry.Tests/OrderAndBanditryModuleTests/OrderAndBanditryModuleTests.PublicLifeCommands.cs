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
        Assert.That(settlement.InterventionCarryoverMonths, Is.EqualTo(1));
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
        Assert.That(unknown.Accepted, Is.False);
        Assert.That(unknown.Summary, Does.Contain("不识此令"));
        Assert.That(settlement.BanditThreat, Is.EqualTo(40));
        Assert.That(settlement.RoutePressure, Is.EqualTo(44));
        Assert.That(settlement.SuppressionDemand, Is.EqualTo(30));
        Assert.That(settlement.DisorderPressure, Is.EqualTo(31));
        Assert.That(settlement.LastInterventionCommandCode, Is.Empty);
        Assert.That(settlement.InterventionCarryoverMonths, Is.EqualTo(0));
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
