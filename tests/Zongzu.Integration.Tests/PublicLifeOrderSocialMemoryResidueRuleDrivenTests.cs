using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
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
}
