using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class PublicLifeOrderResidueDecayFrictionRuleDrivenTests
{
    [Test]
    public void RepairedResponseResidue_SoftensLater_AndOrderRepeatRepairReadsStructuredMemory()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260428);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 50;
        settlement.RetaliationRisk = 12;
        settlement.CoercionRisk = 10;

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

        SocialMemoryEntrySnapshot repairedBeforeDrift = FindResponseMemory(
            simulation,
            PublicLifeOrderResponseOutcomeCodes.Repaired,
            PlayerCommandNames.RepairLocalWatchGuarantee);
        int memoryCountBeforeRepeatCommand = socialState.Memories.Count;

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot repairedAfterDrift = FindResponseMemory(
            simulation,
            PublicLifeOrderResponseOutcomeCodes.Repaired,
            PlayerCommandNames.RepairLocalWatchGuarantee);
        Assert.That(repairedAfterDrift.Weight, Is.LessThan(repairedBeforeDrift.Weight));
        Assert.That(repairedAfterDrift.Summary, Does.Contain("后账渐平"));

        PlayerCommandResult repeatResponse = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
            });

        Assert.That(repeatResponse.Accepted, Is.True);
        Assert.That(repeatResponse.Summary, Does.Contain("社会记忆回读"));
        Assert.That(repeatResponse.Summary, Does.Contain("修复余重"));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeRepeatCommand),
            "Order repeat response may read SocialMemory friction, but must not write SocialMemory at command time.");
    }

    [Test]
    public void EscalatedResponseResidue_HardensLater_AndOfficeRepeatPressCarriesClerkDrag()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260429);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.CoercionRisk = 82;
        settlement.RetaliationRisk = 65;
        settlement.ImplementationDrag = 20;
        foreach (OfficeCareerState career in officeState.People.Where(career => career.SettlementId == settlementId))
        {
            career.JurisdictionLeverage = 4;
            career.ClerkDependence = 90;
            career.PetitionBacklog = 70;
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

        SocialMemoryEntrySnapshot escalatedBeforeDrift = FindResponseMemory(
            simulation,
            PublicLifeOrderResponseOutcomeCodes.Escalated,
            PlayerCommandNames.PressCountyYamenDocument);
        int memoryCountBeforeRepeatCommand = socialState.Memories.Count;
        OfficeCareerState leadCareer = officeState.People
            .Where(career => career.SettlementId == settlementId && career.HasAppointment)
            .OrderByDescending(static career => career.AuthorityTier)
            .ThenByDescending(static career => career.OfficeReputation)
            .ThenBy(static career => career.PersonId.Value)
            .First();
        int clerkDependenceBeforeRepeat = leadCareer.ClerkDependence;

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot escalatedAfterDrift = FindResponseMemory(
            simulation,
            PublicLifeOrderResponseOutcomeCodes.Escalated,
            PlayerCommandNames.PressCountyYamenDocument);
        Assert.That(escalatedAfterDrift.Weight, Is.GreaterThan(escalatedBeforeDrift.Weight));
        Assert.That(escalatedAfterDrift.Summary, Does.Contain("后账转硬"));

        PlayerCommandResult repeatResponse = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.PressCountyYamenDocument,
            });

        Assert.That(repeatResponse.Accepted, Is.True);
        Assert.That(repeatResponse.Summary, Does.Contain("社会记忆回读"));
        Assert.That(repeatResponse.Summary, Does.Contain("恶化余重"));
        Assert.That(leadCareer.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Escalated));
        Assert.That(leadCareer.ClerkDependence, Is.GreaterThanOrEqualTo(clerkDependenceBeforeRepeat));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeRepeatCommand),
            "Office repeat response may read SocialMemory friction, but must not write SocialMemory at command time.");
    }

    private static SocialMemoryEntrySnapshot FindResponseMemory(
        GameSimulation simulation,
        string outcomeCode,
        string commandName)
    {
        return simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.Contains($".{outcomeCode}.")
                              && memory.CauseKey.Contains($".{commandName}."));
    }

    private static SettlementId SelectSettlementWithDisorder(PresentationReadModelBundle bundle)
    {
        return bundle.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => bundle.SettlementDisorder.Any(disorder => disorder.SettlementId == id));
    }
}
