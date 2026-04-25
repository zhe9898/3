using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class PublicLifeOrderRefusalResponseRuleDrivenTests
{
    [Test]
    public void PartialWatchResidue_ProjectsRepairAffordance_AndMonthNPlusTwoSocialMemoryRepair()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260425);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);

        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 50;
        settlement.RetaliationRisk = 12;
        settlement.CoercionRisk = 10;

        PlayerCommandService commandService = new();
        PlayerCommandResult partial = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });
        Assert.That(partial.Accepted, Is.True);
        Assert.That(settlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Partial));

        simulation.AdvanceOneMonth();

        PresentationReadModelBundle monthNPlusOne = builder.BuildForM2(simulation);
        string[] affordanceCodes = monthNPlusOne.PlayerCommands.Affordances
            .Where(affordance => affordance.SettlementId == settlementId)
            .Select(static affordance => affordance.CommandName)
            .ToArray();
        Assert.That(affordanceCodes, Does.Contain(PlayerCommandNames.RepairLocalWatchGuarantee));
        Assert.That(affordanceCodes, Does.Contain(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(affordanceCodes, Does.Contain(PlayerCommandNames.AskClanEldersExplain));
        PlayerCommandAffordanceSnapshot[] playableResponseAffordances = monthNPlusOne.PlayerCommands.Affordances
            .Where(affordance => affordance.SettlementId == settlementId
                                 && (affordance.CommandName == PlayerCommandNames.RepairLocalWatchGuarantee
                                     || affordance.CommandName == PlayerCommandNames.PressCountyYamenDocument
                                     || affordance.CommandName == PlayerCommandNames.AskClanEldersExplain))
            .ToArray();
        Assert.That(playableResponseAffordances, Has.Length.GreaterThanOrEqualTo(3));
        foreach (PlayerCommandAffordanceSnapshot affordance in playableResponseAffordances)
        {
            Assert.That(affordance.IsEnabled, Is.True);
            Assert.That(affordance.Label, Is.Not.Empty);
            Assert.That(affordance.AvailabilitySummary, Is.Not.Empty);
            Assert.That(affordance.ExecutionSummary, Is.Not.Empty);
            Assert.That(affordance.LeverageSummary, Is.Not.Empty);
            Assert.That(affordance.CostSummary, Is.Not.Empty);
            Assert.That(affordance.ReadbackSummary, Is.Not.Empty);
        }

        int memoryCountBeforeResponse = socialState.Memories.Count;
        PlayerCommandResult response = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
            });

        Assert.That(response.Accepted, Is.True);
        Assert.That(settlement.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.RepairLocalWatchGuarantee));
        Assert.That(settlement.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Repaired));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeResponse),
            "Same-month response command must not write SocialMemory.");
        Assert.That(
            officeState.People.All(static career => string.IsNullOrWhiteSpace(career.LastRefusalResponseCommandCode)),
            Is.True,
            "Order-owned response must not mutate Office response trace at command time.");
        Assert.That(
            familyState.Clans.All(static clan => string.IsNullOrWhiteSpace(clan.LastRefusalResponseCommandCode)),
            Is.True,
            "Order-owned response must not mutate Family response trace at command time.");

        PresentationReadModelBundle afterResponse = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot responseReceipt = afterResponse.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == settlementId
                              && receipt.CommandName == PlayerCommandNames.RepairLocalWatchGuarantee);
        Assert.That(responseReceipt.ReadbackSummary, Does.Contain("后账已修复"));

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot repairedResidue = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.Contains(PublicLifeOrderResponseOutcomeCodes.Repaired)
                              && memory.CauseKey.Contains(PlayerCommandNames.RepairLocalWatchGuarantee));
        Assert.That(repairedResidue.Type, Is.EqualTo(MemoryType.Favor));
        Assert.That(repairedResidue.Subtype, Is.EqualTo(MemorySubtype.ProtectionFavor));

        PresentationReadModelBundle monthNPlusTwo = builder.BuildForM2(simulation);
        SettlementGovernanceLaneSnapshot lane = monthNPlusTwo.GovernanceSettlements.Single(lane => lane.SettlementId == settlementId);
        Assert.That(lane.OrderAdministrativeAftermathSummary, Does.Contain("后账已修复"));
        Assert.That(lane.OrderAdministrativeAftermathSummary, Does.Contain("社会记忆"));
    }

    [Test]
    public void RefusedSuppressionResidue_OfficePressEscalates_AndSocialMemoryReadsStructuredAftermath()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260426);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
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
        }

        PlayerCommandService commandService = new();
        PlayerCommandResult refused = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.SuppressBanditry,
            });
        Assert.That(refused.Accepted, Is.False);
        Assert.That(settlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Refused));

        simulation.AdvanceOneMonth();

        int memoryCountBeforeResponse = socialState.Memories.Count;
        PlayerCommandResult response = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.PressCountyYamenDocument,
            });

        Assert.That(response.Accepted, Is.True);
        OfficeCareerState leadCareer = officeState.People
            .Where(career => career.SettlementId == settlementId && career.HasAppointment)
            .OrderByDescending(static career => career.AuthorityTier)
            .ThenByDescending(static career => career.OfficeReputation)
            .ThenBy(static career => career.PersonId.Value)
            .First();
        Assert.That(leadCareer.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(leadCareer.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Escalated));
        Assert.That(settlement.LastRefusalResponseCommandCode, Is.Empty);
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeResponse),
            "Office command-time resolution must not write SocialMemory.");

        PresentationReadModelBundle afterResponse = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot officeReceipt = afterResponse.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == settlementId
                              && receipt.CommandName == PlayerCommandNames.PressCountyYamenDocument);
        Assert.That(officeReceipt.ReadbackSummary, Does.Contain("胥吏"));

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot escalatedResidue = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.Contains(PublicLifeOrderResponseOutcomeCodes.Escalated)
                              && memory.CauseKey.Contains(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(escalatedResidue.Type, Is.EqualTo(MemoryType.Grudge));
        Assert.That(escalatedResidue.Subtype, Is.EqualTo(MemorySubtype.PowerGrudge));
    }

    [Test]
    public void PartialWatchResidue_FamilyExplanationContainsAftermath_AndSocialMemoryContainedAdjustment()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260427);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);

        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 50;
        settlement.RetaliationRisk = 12;
        settlement.CoercionRisk = 10;
        foreach (ClanStateData clan in familyState.Clans.Where(clan => clan.HomeSettlementId == settlementId))
        {
            clan.Prestige = 1;
            clan.SupportReserve = 1;
            clan.MediationMomentum = 0;
        }

        PlayerCommandService commandService = new();
        PlayerCommandResult partial = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });
        Assert.That(partial.Accepted, Is.True);

        simulation.AdvanceOneMonth();

        ClanStateData leadClan = familyState.Clans
            .Where(clan => clan.HomeSettlementId == settlementId)
            .OrderByDescending(static clan => clan.Prestige)
            .ThenBy(static clan => clan.Id.Value)
            .First();
        int memoryCountBeforeResponse = socialState.Memories.Count;
        PlayerCommandResult response = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = leadClan.Id,
                CommandName = PlayerCommandNames.AskClanEldersExplain,
            });

        Assert.That(response.Accepted, Is.True);
        Assert.That(leadClan.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.AskClanEldersExplain));
        Assert.That(leadClan.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Contained));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeResponse),
            "Family response command must not directly write SocialMemory.");

        PresentationReadModelBundle afterResponse = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot familyReceipt = afterResponse.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == settlementId
                              && receipt.ClanId == leadClan.Id
                              && receipt.CommandName == PlayerCommandNames.AskClanEldersExplain);
        Assert.That(familyReceipt.ReadbackSummary, Does.Contain("族老解释"));
        Assert.That(familyReceipt.ReadbackSummary, Does.Contain("人情"));

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot containedResidue = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.Contains(PublicLifeOrderResponseOutcomeCodes.Contained)
                              && memory.CauseKey.Contains(PlayerCommandNames.AskClanEldersExplain));
        Assert.That(containedResidue.Type, Is.EqualTo(MemoryType.Debt));
        Assert.That(containedResidue.Subtype, Is.EqualTo(MemorySubtype.ProtectionFavor));
    }

    private static SettlementId SelectSettlementWithDisorder(PresentationReadModelBundle bundle)
    {
        return bundle.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => bundle.SettlementDisorder.Any(disorder => disorder.SettlementId == id));
    }
}
