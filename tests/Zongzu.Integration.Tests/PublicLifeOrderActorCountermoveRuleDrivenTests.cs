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
        Assert.That(receipt.ReadbackSummary, Does.Contain("Office后手收口读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("转硬"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Office闭环防回压"));

        SettlementGovernanceLaneSnapshot governance = bundle.GovernanceSettlements.Single(lane => lane.SettlementId == settlementId);
        Assert.That(governance.OfficeLaneReceiptClosureSummary, Does.Contain("Office后手收口读回"));
        Assert.That(governance.OfficeLaneReceiptClosureSummary, Does.Contain("转硬"));
        Assert.That(governance.OfficeLaneNoLoopGuardSummary, Does.Contain("Office闭环防回压"));
        Assert.That(bundle.GovernanceDocket.GuidanceSummary, Does.Contain("Office后手收口读回"));
    }

    [Test]
    public void EscalatedResidue_LaterOrderActorsHardenRunnerMisreadWithoutPlayerCommand()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260502);
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
        settlement.RoutePressure = 45;
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

        settlement.RoutePressure = 54;
        settlement.RetaliationRisk = 45;
        settlement.CoercionRisk = 36;
        simulation.AdvanceOneMonth();

        Assert.That(
            settlement.LastRefusalResponseTraceCode,
            Is.EqualTo(PublicLifeOrderResponseTraceCodes.OrderActorRunnerMisreadHardened));
        Assert.That(settlement.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.CompensateRunnerMisread));
        Assert.That(settlement.LastRefusalResponseCommandLabel, Is.EqualTo("脚户误读反噬"));
        Assert.That(settlement.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Escalated));
        Assert.That(settlement.ResponseCarryoverMonths, Is.EqualTo(1));
        Assert.That(settlement.RetaliationRisk, Is.GreaterThan(0));
        Assert.That(settlement.LastRefusalResponseSummary, Is.Not.Empty);
        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.OrderActorRunnerMisreadHardened),
            Is.False,
            "Order actor countermove runs after SocialMemory, so the new owner trace should be read later.");

        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = bundle.PlayerCommands.Receipts.Single(receipt =>
            receipt.SettlementId == settlementId
            && receipt.CommandName == PlayerCommandNames.CompensateRunnerMisread
            && receipt.Label == "脚户误读反噬");
        Assert.That(receipt.ReadbackSummary, Does.Contain("后账恶化"));

        simulation.AdvanceOneMonth();

        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.OrderActorRunnerMisreadHardened),
            Is.True);
    }

    [Test]
    public void RepairedResidue_LaterOfficeActorsQuietlyLandDocumentWithoutPlayerCommand()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260503);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 50;
        settlement.RetaliationRisk = 12;
        settlement.CoercionRisk = 10;
        settlement.RoutePressure = 42;

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

        OfficeCareerState leadCareer = FindLeadCareer(officeState, settlementId);
        leadCareer.ClerkDependence = 38;
        leadCareer.PetitionBacklog = 35;
        leadCareer.PetitionPressure = 25;
        leadCareer.AdministrativeTaskLoad = 30;
        int backlogBeforeCountermove = leadCareer.PetitionBacklog;

        simulation.AdvanceOneMonth();

        Assert.That(
            leadCareer.LastRefusalResponseTraceCode,
            Is.EqualTo(PublicLifeOrderResponseTraceCodes.OfficeActorYamenQuietLanding));
        Assert.That(leadCareer.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(leadCareer.LastRefusalResponseCommandLabel, Is.EqualTo("县门自补落地"));
        Assert.That(
            leadCareer.LastRefusalResponseOutcomeCode,
            Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Repaired)
                .Or.EqualTo(PublicLifeOrderResponseOutcomeCodes.Contained));
        Assert.That(leadCareer.PetitionBacklog, Is.LessThan(backlogBeforeCountermove));
        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.OfficeActorYamenQuietLanding),
            Is.False,
            "Office actor countermove runs after SocialMemory, so durable residue waits for the later pass.");

        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = bundle.PlayerCommands.Receipts.Single(receipt =>
            receipt.SettlementId == settlementId
            && receipt.CommandName == PlayerCommandNames.PressCountyYamenDocument
            && receipt.Label == "县门自补落地");
        Assert.That(receipt.ReadbackSummary, Does.Contain("县门自补落地"));

        simulation.AdvanceOneMonth();

        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.OfficeActorYamenQuietLanding),
            Is.True);
    }

    [Test]
    public void RepairedResidue_LaterFamilyEldersQuietlyExplainAndSocialMemoryReadsSamePass()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260504);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);
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

        SocialMemoryEntrySnapshot repairedMemory = FindResponseMemory(
            simulation,
            PublicLifeOrderResponseOutcomeCodes.Repaired,
            PlayerCommandNames.RepairLocalWatchGuarantee);
        Assert.That(repairedMemory.SourceClanId.HasValue, Is.True);
        ClanStateData clan = familyState.Clans.Single(clan => clan.Id == repairedMemory.SourceClanId!.Value);
        clan.BranchTension = 42;
        clan.MediationMomentum = 24;
        clan.Prestige = 45;
        int branchTensionBeforeCountermove = clan.BranchTension;
        int mediationBeforeCountermove = clan.MediationMomentum;

        simulation.AdvanceOneMonth();

        Assert.That(
            clan.LastRefusalResponseTraceCode,
            Is.EqualTo(PublicLifeOrderResponseTraceCodes.FamilyActorElderQuietExplanation));
        Assert.That(clan.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.AskClanEldersExplain));
        Assert.That(clan.LastRefusalResponseCommandLabel, Is.EqualTo("族老自解释"));
        Assert.That(
            clan.LastRefusalResponseOutcomeCode,
            Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Repaired)
                .Or.EqualTo(PublicLifeOrderResponseOutcomeCodes.Contained));
        Assert.That(clan.BranchTension, Is.LessThan(branchTensionBeforeCountermove));
        Assert.That(clan.MediationMomentum, Is.GreaterThan(mediationBeforeCountermove));
        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.FamilyActorElderQuietExplanation),
            Is.True,
            "Family actor traces are written before SocialMemory in this monthly pass and keep carryover long enough to be read.");

        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = bundle.PlayerCommands.Receipts.Single(receipt =>
            receipt.ClanId == clan.Id
            && receipt.CommandName == PlayerCommandNames.AskClanEldersExplain
            && receipt.Label == "族老自解释");
        Assert.That(receipt.ReadbackSummary, Does.Contain("族老解释"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Family承接入口"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("族老解释读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("本户担保读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("宗房脸面读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Family后手收口读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Family闭环防回压"));
        Assert.That(receipt.FamilyLaneEntryReadbackSummary, Does.Contain("Family承接入口"));
        Assert.That(receipt.FamilyElderExplanationReadbackSummary, Does.Contain("族老解释读回"));
        Assert.That(receipt.FamilyGuaranteeReadbackSummary, Does.Contain("本户担保读回"));
        Assert.That(receipt.FamilyHouseFaceReadbackSummary, Does.Contain("宗房脸面读回"));
        Assert.That(receipt.FamilyLaneReceiptClosureSummary, Does.Contain("Family后手收口读回"));
        Assert.That(receipt.FamilyLaneNoLoopGuardSummary, Does.Contain("不是普通家户再扛"));

        SettlementGovernanceLaneSnapshot governance = bundle.GovernanceSettlements.Single(lane => lane.SettlementId == settlementId);
        Assert.That(governance.FamilyLaneEntryReadbackSummary, Does.Contain("Family承接入口"));
        Assert.That(governance.FamilyLaneNoLoopGuardSummary, Does.Contain("Family闭环防回压"));

    }

    [Test]
    public void EscalatedResidue_LaterFamilyEldersAvoidGuaranteeAndSocialMemoryReadsSamePass()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260505);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);
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

        SocialMemoryEntrySnapshot escalatedMemory = FindResponseMemory(
            simulation,
            PublicLifeOrderResponseOutcomeCodes.Escalated,
            PlayerCommandNames.PressCountyYamenDocument);
        Assert.That(escalatedMemory.SourceClanId.HasValue, Is.True);
        ClanStateData clan = familyState.Clans.Single(clan => clan.Id == escalatedMemory.SourceClanId!.Value);
        clan.Prestige = 12;
        clan.BranchTension = 62;
        clan.MediationMomentum = 30;
        int branchTensionBeforeCountermove = clan.BranchTension;

        simulation.AdvanceOneMonth();

        Assert.That(
            clan.LastRefusalResponseTraceCode,
            Is.EqualTo(PublicLifeOrderResponseTraceCodes.FamilyActorGuaranteeAvoided));
        Assert.That(clan.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.AskClanEldersExplain));
        Assert.That(clan.LastRefusalResponseCommandLabel, Is.EqualTo("族老避羞"));
        Assert.That(clan.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Ignored));
        Assert.That(clan.BranchTension, Is.GreaterThan(branchTensionBeforeCountermove));
        Assert.That(
            HasActorResponseMemory(simulation, PublicLifeOrderResponseTraceCodes.FamilyActorGuaranteeAvoided),
            Is.True,
            "Family actor traces are written before SocialMemory in this monthly pass and keep carryover long enough to be read.");

        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = bundle.PlayerCommands.Receipts.Single(receipt =>
            receipt.ClanId == clan.Id
            && receipt.CommandName == PlayerCommandNames.AskClanEldersExplain
            && receipt.Label == "族老避羞");
        Assert.That(receipt.ReadbackSummary, Does.Contain("担保欠账仍在"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Family承接入口"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("本户担保读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("宗房脸面读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Family后手收口读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Family闭环防回压"));
        Assert.That(receipt.FamilyLaneEntryReadbackSummary, Does.Contain("Family承接入口"));
        Assert.That(receipt.FamilyGuaranteeReadbackSummary, Does.Contain("本户担保读回"));
        Assert.That(receipt.FamilyHouseFaceReadbackSummary, Does.Contain("宗房脸面读回"));
        Assert.That(receipt.FamilyLaneReceiptClosureSummary, Does.Contain("未接待承口"));
        Assert.That(receipt.FamilyLaneNoLoopGuardSummary, Does.Contain("不是普通家户再扛"));

        SettlementGovernanceLaneSnapshot governance = bundle.GovernanceSettlements.Single(lane => lane.SettlementId == settlementId);
        Assert.That(governance.FamilyLaneEntryReadbackSummary, Does.Contain("Family承接入口"));
        Assert.That(governance.FamilyLaneNoLoopGuardSummary, Does.Contain("Family闭环防回压"));

    }

    private static bool HasActorResponseMemory(GameSimulation simulation, string traceCode)
    {
        return simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Any(memory => memory.CauseKey.Contains($".{traceCode}", System.StringComparison.Ordinal));
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

    private static OfficeCareerState FindLeadCareer(OfficeAndCareerState officeState, SettlementId settlementId)
    {
        return officeState.People
            .Where(career => career.SettlementId == settlementId && career.HasAppointment)
            .OrderByDescending(static career => career.AuthorityTier)
            .ThenByDescending(static career => career.OfficeReputation)
            .ThenBy(static career => career.PersonId.Value)
            .First();
    }

    private static SettlementId SelectSettlementWithDisorder(PresentationReadModelBundle bundle)
    {
        return bundle.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => bundle.SettlementDisorder.Any(disorder => disorder.SettlementId == id));
    }
}
