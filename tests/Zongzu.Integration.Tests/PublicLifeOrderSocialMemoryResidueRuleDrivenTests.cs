using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class PublicLifeOrderSocialMemoryResidueRuleDrivenTests
{
    [Test]
    public void MonthNOrderCommand_ProducesMonthNPlusOneSocialMemoryResidueAndReadback()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260425);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle before = builder.BuildForM2(simulation);
        SettlementId settlementId = before.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => before.SettlementDisorder.Any(disorder => disorder.SettlementId == id));

        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState acceptedOrderSettlement = orderState.Settlements.Single(settlement => settlement.SettlementId == settlementId);
        acceptedOrderSettlement.ImplementationDrag = 0;
        acceptedOrderSettlement.CoercionRisk = 0;
        acceptedOrderSettlement.RetaliationRisk = 0;
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        JurisdictionAuthorityState acceptedJurisdiction = officeState.Jurisdictions.Single(jurisdiction => jurisdiction.SettlementId == settlementId);
        acceptedJurisdiction.JurisdictionLeverage = 80;
        acceptedJurisdiction.ClerkDependence = 10;
        acceptedJurisdiction.AuthorityTier = 4;
        acceptedJurisdiction.AdministrativeTaskLoad = 5;
        acceptedJurisdiction.PetitionPressure = 5;
        acceptedJurisdiction.PetitionBacklog = 0;
        int memoryCountBeforeCommand = socialState.Memories.Count;

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });

        Assert.That(result.Accepted, Is.True);
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeCommand),
            "Order command resolution must not directly mutate SocialMemory state in Month N.");

        SettlementDisorderState orderSettlement = orderState.Settlements.Single(settlement => settlement.SettlementId == settlementId);
        Assert.That(orderSettlement.LastInterventionCommandCode, Is.EqualTo(PlayerCommandNames.FundLocalWatch));
        Assert.That(orderSettlement.InterventionCarryoverMonths, Is.EqualTo(1));
        Assert.That(orderSettlement.RouteShielding, Is.GreaterThan(0));

        simulation.AdvanceOneMonth();

        ISocialMemoryAndRelationsQueries socialQueries = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>();
        SocialMemoryEntrySnapshot residue = socialQueries.GetMemories()
            .Single(memory => memory.CauseKey == "order.public_life.fund_local_watch");
        Assert.That(residue.Type, Is.EqualTo(MemoryType.Favor));
        Assert.That(residue.Subtype, Is.EqualTo(MemorySubtype.ProtectionFavor));
        Assert.That(residue.Summary, Does.Contain("添雇巡丁"));

        PresentationReadModelBundle after = builder.BuildForM2(simulation);
        Assert.That(after.SocialMemories.Any(memory => memory.Id == residue.Id), Is.True);

        PlayerCommandReceiptSnapshot receipt = after.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == settlementId
                              && receipt.CommandName == PlayerCommandNames.FundLocalWatch);
        Assert.That(receipt.ReadbackSummary, Does.Contain("社会记忆读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("添雇巡丁"));

        SettlementGovernanceLaneSnapshot lane = after.GovernanceSettlements.Single(lane => lane.SettlementId == settlementId);
        Assert.That(lane.OrderAdministrativeAftermathSummary, Does.Contain("社会记忆读回"));
    }

    [Test]
    public void MonthNPartialWatchCommand_ProducesMonthNPlusOneSocialMemoryResidueAndReadback()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260426);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle before = builder.BuildForM2(simulation);
        SettlementId settlementId = before.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => before.SettlementDisorder.Any(disorder => disorder.SettlementId == id));

        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState orderSettlement = orderState.Settlements.Single(settlement => settlement.SettlementId == settlementId);
        orderSettlement.ImplementationDrag = 50;
        orderSettlement.RetaliationRisk = 12;
        orderSettlement.CoercionRisk = 10;
        int memoryCountBeforeCommand = socialState.Memories.Count;

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });

        Assert.That(result.Accepted, Is.True);
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeCommand),
            "Order command resolution must not directly mutate SocialMemory state in Month N.");
        Assert.That(orderSettlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Partial));
        Assert.That(orderSettlement.LastInterventionPartialCode, Is.EqualTo(OrderInterventionPartialCodes.CountyDrag));
        Assert.That(orderSettlement.InterventionCarryoverMonths, Is.EqualTo(1));
        Assert.That(orderSettlement.RefusalCarryoverMonths, Is.EqualTo(0));

        simulation.AdvanceOneMonth();

        ISocialMemoryAndRelationsQueries socialQueries = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>();
        SocialMemoryEntrySnapshot residue = socialQueries.GetMemories()
            .Single(memory => memory.CauseKey == "order.public_life.fund_local_watch.partial");
        Assert.That(residue.Type, Is.EqualTo(MemoryType.Debt));
        Assert.That(residue.Subtype, Is.EqualTo(MemorySubtype.ProtectionFavor));
        Assert.That(residue.Summary, Does.Contain("添雇巡丁"));

        PresentationReadModelBundle after = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = after.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == settlementId
                              && receipt.CommandName == PlayerCommandNames.FundLocalWatch);
        Assert.That(receipt.ReadbackSummary, Does.Contain("地方拖延"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("后账仍在"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("社会记忆读回"));

        SettlementGovernanceLaneSnapshot lane = after.GovernanceSettlements.Single(lane => lane.SettlementId == settlementId);
        Assert.That(lane.OrderAdministrativeAftermathSummary, Does.Contain("地方拖延"));
        Assert.That(lane.OrderAdministrativeAftermathSummary, Does.Contain("社会记忆读回"));
    }

    [Test]
    public void MonthNRefusedSuppressionCommand_ProducesMonthNPlusOneSocialMemoryResidueAndReadback()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260427);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle before = builder.BuildForM2(simulation);
        SettlementId settlementId = before.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => before.SettlementDisorder.Any(disorder => disorder.SettlementId == id));

        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState orderSettlement = orderState.Settlements.Single(settlement => settlement.SettlementId == settlementId);
        orderSettlement.CoercionRisk = 82;
        orderSettlement.RetaliationRisk = 65;
        orderSettlement.ImplementationDrag = 20;
        int memoryCountBeforeCommand = socialState.Memories.Count;

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.SuppressBanditry,
            });

        Assert.That(result.Accepted, Is.False);
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeCommand),
            "Refused Order resolution must not directly mutate SocialMemory state in Month N.");
        Assert.That(orderSettlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Refused));
        Assert.That(orderSettlement.LastInterventionRefusalCode, Is.EqualTo(OrderInterventionRefusalCodes.SuppressionRefused));
        Assert.That(orderSettlement.InterventionCarryoverMonths, Is.EqualTo(0));
        Assert.That(orderSettlement.RefusalCarryoverMonths, Is.EqualTo(1));

        simulation.AdvanceOneMonth();

        ISocialMemoryAndRelationsQueries socialQueries = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>();
        SocialMemoryEntrySnapshot residue = socialQueries.GetMemories()
            .Single(memory => memory.CauseKey == "order.public_life.suppress_banditry.refused");
        Assert.That(residue.Type, Is.EqualTo(MemoryType.Fear));
        Assert.That(residue.Subtype, Is.EqualTo(MemorySubtype.PowerGrudge));
        Assert.That(residue.Summary, Does.Contain("严缉路匪"));

        PresentationReadModelBundle after = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = after.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == settlementId
                              && receipt.CommandName == PlayerCommandNames.SuppressBanditry);
        Assert.That(receipt.ReadbackSummary, Does.Contain("县门未落地"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("后账仍在"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("社会记忆读回"));

        SettlementGovernanceLaneSnapshot lane = after.GovernanceSettlements.Single(lane => lane.SettlementId == settlementId);
        Assert.That(lane.OrderAdministrativeAftermathSummary, Does.Contain("县门未落地"));
        Assert.That(lane.OrderAdministrativeAftermathSummary, Does.Contain("社会记忆读回"));
    }
}
