using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class PublicLifeOrderActorCountermoveRuleDrivenTests
{
    [Test]
    public void RepairedResidue_LaterOrderActorsSelfSettleWithoutPlayerCommand()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260430);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 50;
        settlement.RetaliationRisk = 12;
        settlement.CoercionRisk = 10;
        settlement.RoutePressure = 48;

        PlayerCommandService commandService = new();
        Assert.That(commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            }).Accepted, Is.True);

        simulation.AdvanceOneMonth();

        PlayerCommandResult response = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
            });
        Assert.That(response.Accepted, Is.True);

        simulation.AdvanceOneMonth();
        settlement.RoutePressure = 52;
        settlement.ImplementationDrag = 35;
        settlement.RetaliationRisk = 22;
        int routePressureBeforeCountermove = settlement.RoutePressure;

        simulation.AdvanceOneMonth();

        Assert.That(
            settlement.LastRefusalResponseTraceCode,
            Is.EqualTo(PublicLifeOrderResponseTraceCodes.OrderActorLocalWatchSelfSettled));
        Assert.That(settlement.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.RepairLocalWatchGuarantee));
        Assert.That(settlement.LastRefusalResponseCommandLabel, Is.EqualTo("巡丁自补保"));
        Assert.That(
            settlement.LastRefusalResponseOutcomeCode,
            Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Repaired)
                .Or.EqualTo(PublicLifeOrderResponseOutcomeCodes.Contained));
        Assert.That(settlement.RoutePressure, Is.LessThan(routePressureBeforeCountermove));
        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.OrderActorLocalWatchSelfSettled),
            Is.False,
            "Order actor countermove runs after SocialMemory in the monthly order, so SocialMemory should not record it until a later month.");

        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = bundle.PlayerCommands.Receipts.Single(receipt =>
            receipt.SettlementId == settlementId
            && receipt.CommandName == PlayerCommandNames.RepairLocalWatchGuarantee
            && receipt.Label == "巡丁自补保");
        Assert.That(receipt.ReadbackSummary, Does.Contain("后账"));

        simulation.AdvanceOneMonth();

        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.OrderActorLocalWatchSelfSettled),
            Is.True,
            "The later SocialMemory month may read the owner trace and create durable residue.");
    }

    [Test]
    public void EscalatedResidue_LaterOfficeActorsContinueClerkDelayWithoutPlayerCommand()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260501);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.CoercionRisk = 82;
        settlement.RetaliationRisk = 65;
        settlement.ImplementationDrag = 20;

        foreach (OfficeCareerState career in officeState.People.Where(career => career.SettlementId == settlementId))
        {
            career.JurisdictionLeverage = 4;
            career.ClerkDependence = 90;
            career.PetitionBacklog = 70;
            career.PetitionPressure = 70;
            career.AdministrativeTaskLoad = 70;
        }

        PlayerCommandService commandService = new();
        Assert.That(commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.SuppressBanditry,
            }).Accepted, Is.False);

        simulation.AdvanceOneMonth();

        PlayerCommandResult response = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.PressCountyYamenDocument,
            });
        Assert.That(response.Accepted, Is.True);

        simulation.AdvanceOneMonth();

        OfficeCareerState leadCareer = officeState.People
            .Where(career => career.SettlementId == settlementId && career.HasAppointment)
            .OrderByDescending(static career => career.AuthorityTier)
            .ThenByDescending(static career => career.OfficeReputation)
            .ThenBy(static career => career.PersonId.Value)
            .First();
        leadCareer.ClerkDependence = 85;
        leadCareer.PetitionBacklog = 68;
        int clerkDependenceBeforeCountermove = leadCareer.ClerkDependence;

        simulation.AdvanceOneMonth();

        Assert.That(
            leadCareer.LastRefusalResponseTraceCode,
            Is.EqualTo(PublicLifeOrderResponseTraceCodes.OfficeActorClerkDelayContinued));
        Assert.That(leadCareer.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(leadCareer.LastRefusalResponseCommandLabel, Is.EqualTo("胥吏续拖"));
        Assert.That(leadCareer.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Escalated));
        Assert.That(leadCareer.ClerkDependence, Is.GreaterThan(clerkDependenceBeforeCountermove));
        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.OfficeActorClerkDelayContinued),
            Is.False,
            "Office actor countermove runs after SocialMemory in the monthly order, so durable residue waits for the later pass.");

        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = bundle.PlayerCommands.Receipts.Single(receipt =>
            receipt.SettlementId == settlementId
            && receipt.CommandName == PlayerCommandNames.PressCountyYamenDocument
            && receipt.Label == "胥吏续拖");
        Assert.That(receipt.ReadbackSummary, Does.Contain("后账转成新的积案"));
    }

    private static bool HasActorResponseMemory(GameSimulation simulation, string traceCode)
    {
        return simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Any(memory => memory.CauseKey.Contains($".{traceCode}", System.StringComparison.Ordinal));
    }

    private static SettlementId SelectSettlementWithDisorder(PresentationReadModelBundle bundle)
    {
        return bundle.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => bundle.SettlementDisorder.Any(disorder => disorder.SettlementId == id));
    }
}
