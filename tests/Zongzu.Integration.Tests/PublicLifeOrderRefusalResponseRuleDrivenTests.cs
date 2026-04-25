using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
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

    [Test]
    public void PartialWatchResidue_ProjectsOrdinaryHouseholdAfterAccountWithoutPopulationMutation()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260430);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        SettlementId settlementId = SelectSettlementWithDisorder(builder.BuildForM2(simulation));
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);

        HouseholdId ordinaryHouseholdId = new(populationState.Households.Max(static household => household.Id.Value) + 1);
        PopulationHouseholdState ordinaryHousehold = new()
        {
            Id = ordinaryHouseholdId,
            HouseholdName = "Kiln Wu household",
            SettlementId = settlementId,
            SponsorClanId = null,
            Livelihood = LivelihoodType.HiredLabor,
            Distress = 82,
            DebtPressure = 76,
            LaborCapacity = 36,
            MigrationRisk = 58,
            LandHolding = 0,
            GrainStore = 8,
            ToolCondition = 38,
            ShelterQuality = 42,
            DependentCount = 3,
            LaborerCount = 1,
        };
        populationState.Households.Add(ordinaryHousehold);
        PopulationSettlementState? populationSettlement = populationState.Settlements
            .FirstOrDefault(entry => entry.SettlementId == settlementId);
        if (populationSettlement is not null)
        {
            populationSettlement.CommonerDistress = 74;
            populationSettlement.LaborSupply = 48;
            populationSettlement.MigrationPressure = 54;
        }

        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 52;
        settlement.RoutePressure = 64;
        settlement.RetaliationRisk = 16;
        settlement.CoercionRisk = 12;

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

        int distressBeforeProjection = ordinaryHousehold.Distress;
        int debtBeforeProjection = ordinaryHousehold.DebtPressure;
        int migrationBeforeProjection = ordinaryHousehold.MigrationRisk;
        PresentationReadModelBundle monthNPlusOne = builder.BuildForM2(simulation);
        HouseholdSocialPressureSnapshot ordinaryPressure = monthNPlusOne.HouseholdSocialPressures
            .Single(pressure => pressure.HouseholdId == ordinaryHouseholdId);
        HouseholdSocialPressureSignalSnapshot orderResidueSignal = ordinaryPressure.Signals
            .Single(signal => string.Equals(signal.SignalKey, HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue, StringComparison.Ordinal));

        Assert.That(orderResidueSignal.Score, Is.GreaterThan(0));
        Assert.That(ordinaryPressure.PrimaryDriftKey, Is.EqualTo(HouseholdSocialDriftKeys.PublicOrderAftermath));
        Assert.That(ordinaryPressure.PressureSummary, Is.EqualTo(orderResidueSignal.Summary));
        Assert.That(orderResidueSignal.Summary, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(orderResidueSignal.SourceModuleKeys, Does.Contain(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(orderResidueSignal.SourceModuleKeys, Does.Contain(KnownModuleKeys.OrderAndBanditry));
        Assert.That(monthNPlusOne.PlayerCommands.Affordances
            .Any(affordance => affordance.SettlementId == settlementId
                               && affordance.CommandName == PlayerCommandNames.RepairLocalWatchGuarantee),
            Is.True);
        PlayerCommandAffordanceSnapshot repairAffordance = monthNPlusOne.PlayerCommands.Affordances
            .First(affordance => affordance.SettlementId == settlementId
                                 && affordance.CommandName == PlayerCommandNames.RepairLocalWatchGuarantee);
        Assert.That(repairAffordance.TargetLabel, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(repairAffordance.AvailabilitySummary, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(repairAffordance.ExecutionSummary, Does.Contain("命令所有者"));
        Assert.That(repairAffordance.LeverageSummary, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(repairAffordance.CostSummary, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(repairAffordance.ReadbackSummary, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(ordinaryHousehold.Distress, Is.EqualTo(distressBeforeProjection));
        Assert.That(ordinaryHousehold.DebtPressure, Is.EqualTo(debtBeforeProjection));
        Assert.That(ordinaryHousehold.MigrationRisk, Is.EqualTo(migrationBeforeProjection));

        int memoryCountBeforeResponse = socialState.Memories.Count;
        PlayerCommandResult response = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
            });

        Assert.That(response.Accepted, Is.True);
        Assert.That(ordinaryHousehold.Distress, Is.EqualTo(distressBeforeProjection));
        Assert.That(ordinaryHousehold.DebtPressure, Is.EqualTo(debtBeforeProjection));
        Assert.That(ordinaryHousehold.MigrationRisk, Is.EqualTo(migrationBeforeProjection));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeResponse),
            "Same-month response command must not write SocialMemory.");

        PresentationReadModelBundle afterResponse = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot responseReceipt = afterResponse.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == settlementId
                              && receipt.CommandName == PlayerCommandNames.RepairLocalWatchGuarantee);
        Assert.That(responseReceipt.TargetLabel, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(responseReceipt.LeverageSummary, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(responseReceipt.CostSummary, Does.Contain(ordinaryHousehold.HouseholdName));
        Assert.That(responseReceipt.ReadbackSummary, Does.Contain(ordinaryHousehold.HouseholdName));
    }

    [Test]
    public void PartialWatchResidue_ProjectsHomeHouseholdLocalResponse_AndCommandMutatesOnlyPopulation()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260501);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        PopulationHouseholdState anchorHousehold = SelectPlayerAnchorHouseholdForTest(populationState);
        SettlementId settlementId = anchorHousehold.SettlementId;
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);

        anchorHousehold.Distress = 58;
        anchorHousehold.DebtPressure = 48;
        anchorHousehold.LaborCapacity = 42;
        anchorHousehold.MigrationRisk = 72;
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 52;
        settlement.RoutePressure = 62;
        settlement.RetaliationRisk = 14;
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

        SetRelievedLocalResponseHouseholdForTest(anchorHousehold);
        PresentationReadModelBundle monthNPlusOne = builder.BuildForM2(simulation);
        HouseholdSocialPressureSnapshot anchorPressure = monthNPlusOne.HouseholdSocialPressures
            .Single(pressure => pressure.HouseholdId == anchorHousehold.Id);
        HouseholdSocialPressureSignalSnapshot residueSignal = anchorPressure.Signals
            .Single(signal => signal.SignalKey == HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue);
        Assert.That(residueSignal.Score, Is.GreaterThan(0));

        PlayerCommandAffordanceSnapshot[] localAffordances = monthNPlusOne.PlayerCommands.Affordances
            .Where(affordance => affordance.SettlementId == settlementId
                                 && affordance.ModuleKey == KnownModuleKeys.PopulationAndHouseholds)
            .OrderBy(static affordance => affordance.CommandName)
            .ToArray();
        Assert.That(
            localAffordances.Select(static affordance => affordance.CommandName).ToArray(),
            Is.SupersetOf(new[]
            {
                PlayerCommandNames.PoolRunnerCompensation,
                PlayerCommandNames.RestrictNightTravel,
                PlayerCommandNames.SendHouseholdRoadMessage,
            }));
        Assert.That(localAffordances.All(static affordance => affordance.SurfaceKey == PlayerCommandSurfaceKeys.PublicLife), Is.True);
        Assert.That(localAffordances.All(affordance => affordance.TargetLabel.Contains(anchorHousehold.HouseholdName, StringComparison.Ordinal)), Is.True);
        Assert.That(localAffordances.All(static affordance => affordance.ExecutionSummary.Contains(KnownModuleKeys.PopulationAndHouseholds, StringComparison.Ordinal)), Is.True);

        int memoryCountBefore = socialState.Memories.Count;
        int orderRouteBefore = settlement.RoutePressure;
        string orderResponseBefore = settlement.LastRefusalResponseCommandCode;
        string familyResponsesBefore = string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode));
        string officeResponsesBefore = string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode));

        PlayerCommandResult localResponse = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.RestrictNightTravel,
            });

        Assert.That(localResponse.Accepted, Is.True);
        Assert.That(localResponse.ModuleKey, Is.EqualTo(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(anchorHousehold.LastLocalResponseCommandCode, Is.EqualTo(PlayerCommandNames.RestrictNightTravel));
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Relieved));
        Assert.That(anchorHousehold.LastLocalResponseTraceCode, Is.EqualTo(HouseholdLocalResponseTraceCodes.NightTravelRestricted));
        Assert.That(anchorHousehold.MigrationRisk, Is.LessThan(72));
        Assert.That(anchorHousehold.LaborCapacity, Is.LessThan(42));
        Assert.That(settlement.RoutePressure, Is.EqualTo(orderRouteBefore));
        Assert.That(settlement.LastRefusalResponseCommandCode, Is.EqualTo(orderResponseBefore));
        Assert.That(string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode)), Is.EqualTo(familyResponsesBefore));
        Assert.That(string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode)), Is.EqualTo(officeResponsesBefore));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBefore));

        PresentationReadModelBundle afterRelief = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot relievedReceipt = afterRelief.PlayerCommands.Receipts
            .First(receipt => receipt.CommandName == PlayerCommandNames.RestrictNightTravel
                              && receipt.TargetLabel == anchorHousehold.HouseholdName);
        Assert.That(relievedReceipt.OutcomeSummary, Is.EqualTo("本户已缓"));
        Assert.That(relievedReceipt.CostSummary, Does.Contain("迁徙之念"));
        Assert.That(afterRelief.InfluenceFootprint.Reaches
            .Single(reach => reach.ReachKey == InfluenceReachKeys.OwnHousehold)
            .CommandSummary, Does.Contain("低权能回应面"));

        anchorHousehold.Distress = 70;
        anchorHousehold.DebtPressure = 78;
        anchorHousehold.LaborCapacity = 35;
        anchorHousehold.MigrationRisk = 45;
        PlayerCommandResult strainedResponse = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.PoolRunnerCompensation,
            });

        Assert.That(strainedResponse.Accepted, Is.True);
        Assert.That(anchorHousehold.LastLocalResponseCommandCode, Is.EqualTo(PlayerCommandNames.PoolRunnerCompensation));
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Strained));
        Assert.That(anchorHousehold.LastLocalResponseTraceCode, Is.EqualTo(HouseholdLocalResponseTraceCodes.RunnerMisreadSettledLocally));
        Assert.That(anchorHousehold.DebtPressure, Is.GreaterThanOrEqualTo(82));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBefore));

        PresentationReadModelBundle afterStrain = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot strainedReceipt = afterStrain.PlayerCommands.Receipts
            .First(receipt => receipt.CommandName == PlayerCommandNames.PoolRunnerCompensation
                              && receipt.TargetLabel == anchorHousehold.HouseholdName);
        Assert.That(strainedReceipt.OutcomeSummary, Is.EqualTo("本户吃紧"));
        Assert.That(strainedReceipt.ReadbackSummary, Does.Contain("债压"));
    }

    [Test]
    public void HomeHouseholdLocalResponse_MonthNPlusTwoSocialMemoryReadsStructuredAftermath()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260502);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        PopulationHouseholdState anchorHousehold = SelectPlayerAnchorHouseholdForTest(populationState);
        SettlementId settlementId = anchorHousehold.SettlementId;
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);

        anchorHousehold.Distress = 58;
        anchorHousehold.DebtPressure = 48;
        anchorHousehold.LaborCapacity = 42;
        anchorHousehold.MigrationRisk = 72;
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 52;
        settlement.RoutePressure = 62;
        settlement.RetaliationRisk = 14;
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

        SetRelievedLocalResponseHouseholdForTest(anchorHousehold);
        PresentationReadModelBundle monthNPlusOne = builder.BuildForM2(simulation);
        Assert.That(monthNPlusOne.PlayerCommands.Affordances
            .Any(affordance => affordance.SettlementId == settlementId
                               && affordance.CommandName == PlayerCommandNames.RestrictNightTravel
                               && affordance.ModuleKey == KnownModuleKeys.PopulationAndHouseholds),
            Is.True);

        int memoryCountBeforeRelievedResponse = socialState.Memories.Count;
        PlayerCommandResult relievedResponse = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.RestrictNightTravel,
            });

        Assert.That(relievedResponse.Accepted, Is.True);
        Assert.That(relievedResponse.ModuleKey, Is.EqualTo(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Relieved));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeRelievedResponse),
            "Same-month home-household response command must not write SocialMemory.");

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot relievedResidue = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.StartsWith("order.public_life.household_response.", StringComparison.Ordinal)
                              && memory.CauseKey.Contains(anchorHousehold.Id.Value.ToString(), StringComparison.Ordinal)
                              && memory.CauseKey.Contains(PlayerCommandNames.RestrictNightTravel, StringComparison.Ordinal)
                              && memory.CauseKey.Contains(HouseholdLocalResponseOutcomeCodes.Relieved, StringComparison.Ordinal));
        Assert.That(relievedResidue.Type, Is.EqualTo(MemoryType.Favor));
        Assert.That(relievedResidue.Subtype, Is.EqualTo(MemorySubtype.ProtectionFavor));

        PresentationReadModelBundle afterRelievedMemory = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot relievedReceipt = afterRelievedMemory.PlayerCommands.Receipts
            .First(receipt => receipt.CommandName == PlayerCommandNames.RestrictNightTravel
                              && receipt.TargetLabel == anchorHousehold.HouseholdName);
        Assert.That(relievedReceipt.ReadbackSummary, Does.Contain(relievedResidue.Summary));

        anchorHousehold.Distress = 70;
        anchorHousehold.DebtPressure = 78;
        anchorHousehold.LaborCapacity = 35;
        anchorHousehold.MigrationRisk = 45;
        int memoryCountBeforeStrainedResponse = socialState.Memories.Count;
        PlayerCommandResult strainedResponse = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.PoolRunnerCompensation,
            });

        Assert.That(strainedResponse.Accepted, Is.True);
        Assert.That(strainedResponse.ModuleKey, Is.EqualTo(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Strained));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeStrainedResponse),
            "Same-month strained local response must still not write SocialMemory.");

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot strainedResidue = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.StartsWith("order.public_life.household_response.", StringComparison.Ordinal)
                              && memory.CauseKey.Contains(anchorHousehold.Id.Value.ToString(), StringComparison.Ordinal)
                              && memory.CauseKey.Contains(PlayerCommandNames.PoolRunnerCompensation, StringComparison.Ordinal)
                              && memory.CauseKey.Contains(HouseholdLocalResponseOutcomeCodes.Strained, StringComparison.Ordinal));
        Assert.That(strainedResidue.Type, Is.EqualTo(MemoryType.Debt));
        Assert.That(strainedResidue.Subtype, Is.EqualTo(MemorySubtype.MarketDebt));

        PresentationReadModelBundle afterStrainedMemory = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot strainedReceipt = afterStrainedMemory.PlayerCommands.Receipts
            .First(receipt => receipt.CommandName == PlayerCommandNames.PoolRunnerCompensation
                              && receipt.TargetLabel == anchorHousehold.HouseholdName);
        Assert.That(strainedReceipt.ReadbackSummary, Does.Contain(strainedResidue.Summary));
    }

    [Test]
    public void HomeHouseholdLocalResponse_RepeatFrictionReadsSocialMemoryAndMutatesOnlyPopulation()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260503);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        PopulationHouseholdState anchorHousehold = SelectPlayerAnchorHouseholdForTest(populationState);
        SettlementId settlementId = anchorHousehold.SettlementId;
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);

        anchorHousehold.Distress = 58;
        anchorHousehold.DebtPressure = 48;
        anchorHousehold.LaborCapacity = 42;
        anchorHousehold.MigrationRisk = 72;
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 52;
        settlement.RoutePressure = 62;
        settlement.RetaliationRisk = 14;
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

        SetRelievedLocalResponseHouseholdForTest(anchorHousehold);
        PlayerCommandResult firstRelief = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.RestrictNightTravel,
            });
        Assert.That(firstRelief.Accepted, Is.True);
        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot relievedResidue = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.StartsWith("order.public_life.household_response.", StringComparison.Ordinal)
                              && memory.CauseKey.Contains(anchorHousehold.Id.Value.ToString(), StringComparison.Ordinal)
                              && memory.CauseKey.Contains(PlayerCommandNames.RestrictNightTravel, StringComparison.Ordinal)
                              && memory.CauseKey.Contains(HouseholdLocalResponseOutcomeCodes.Relieved, StringComparison.Ordinal));
        Assert.That(relievedResidue.Weight, Is.GreaterThanOrEqualTo(20));

        PresentationReadModelBundle afterRelievedMemory = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot relievedAffordance = afterRelievedMemory.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.RestrictNightTravel
                                 && affordance.SettlementId == settlementId
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);
        Assert.That(relievedAffordance.AvailabilitySummary, Does.Contain("旧账记忆"));
        Assert.That(relievedAffordance.ReadbackSummary, Does.Contain("社会记忆读回"));

        anchorHousehold.Distress = 58;
        anchorHousehold.DebtPressure = 48;
        anchorHousehold.LaborCapacity = 42;
        anchorHousehold.MigrationRisk = 72;
        int memoryCountBeforeRepeatRelief = socialState.Memories.Count;
        int routePressureBeforeRepeatRelief = settlement.RoutePressure;
        string familyResponsesBefore = string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode));
        string officeResponsesBefore = string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode));

        PlayerCommandResult repeatedRelief = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.RestrictNightTravel,
            });

        Assert.That(repeatedRelief.Accepted, Is.True);
        Assert.That(anchorHousehold.MigrationRisk, Is.LessThanOrEqualTo(60),
            "Relieved social-memory residue should provide light local support on the next household response.");
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("旧账记得曾被缓下"));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeRepeatRelief));
        Assert.That(settlement.RoutePressure, Is.EqualTo(routePressureBeforeRepeatRelief));
        Assert.That(string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode)), Is.EqualTo(familyResponsesBefore));
        Assert.That(string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode)), Is.EqualTo(officeResponsesBefore));

        anchorHousehold.Distress = 70;
        anchorHousehold.DebtPressure = 78;
        anchorHousehold.LaborCapacity = 35;
        anchorHousehold.MigrationRisk = 45;
        PlayerCommandResult firstStrain = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.PoolRunnerCompensation,
            });
        Assert.That(firstStrain.Accepted, Is.True);
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Strained));

        simulation.AdvanceOneMonth();

        SocialMemoryEntrySnapshot strainedResidue = simulation.GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .GetMemories()
            .Single(memory => memory.CauseKey.StartsWith("order.public_life.household_response.", StringComparison.Ordinal)
                              && memory.CauseKey.Contains(anchorHousehold.Id.Value.ToString(), StringComparison.Ordinal)
                              && memory.CauseKey.Contains(PlayerCommandNames.PoolRunnerCompensation, StringComparison.Ordinal)
                              && memory.CauseKey.Contains(HouseholdLocalResponseOutcomeCodes.Strained, StringComparison.Ordinal));
        Assert.That(strainedResidue.Weight, Is.GreaterThanOrEqualTo(35));

        anchorHousehold.Distress = 70;
        anchorHousehold.DebtPressure = 78;
        anchorHousehold.LaborCapacity = 35;
        anchorHousehold.MigrationRisk = 45;
        int memoryCountBeforeRepeatStrain = socialState.Memories.Count;
        PlayerCommandResult repeatedStrain = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.PoolRunnerCompensation,
            });

        Assert.That(repeatedStrain.Accepted, Is.True);
        Assert.That(anchorHousehold.DebtPressure, Is.GreaterThanOrEqualTo(90),
            "Strained social-memory residue should add local debt drag rather than repairing order authority.");
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("旧账记忆仍硬"));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeRepeatStrain));
    }

    [Test]
    public void HomeHouseholdLocalResponse_CommonHouseholdTextureShapesAffordanceAndLocalCosts()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260504);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        PopulationHouseholdState anchorHousehold = SelectPlayerAnchorHouseholdForTest(populationState);
        SettlementId settlementId = anchorHousehold.SettlementId;
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);

        anchorHousehold.Distress = 76;
        anchorHousehold.DebtPressure = 82;
        anchorHousehold.LaborCapacity = 33;
        anchorHousehold.MigrationRisk = 62;
        anchorHousehold.DependentCount = 4;
        anchorHousehold.LaborerCount = 1;
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 52;
        settlement.RoutePressure = 62;
        settlement.RetaliationRisk = 14;
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
        PlayerCommandAffordanceSnapshot compensationAffordance = monthNPlusOne.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.PoolRunnerCompensation
                                 && affordance.SettlementId == settlementId
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);
        PlayerCommandAffordanceSnapshot nightTravelAffordance = monthNPlusOne.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.RestrictNightTravel
                                 && affordance.SettlementId == settlementId
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);
        PlayerCommandAffordanceSnapshot roadMessageAffordance = monthNPlusOne.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.SendHouseholdRoadMessage
                                 && affordance.SettlementId == settlementId
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);

        Assert.That(compensationAffordance.AvailabilitySummary, Does.Contain("本户底色"));
        Assert.That(compensationAffordance.AvailabilitySummary, Does.Contain("债压已高"));
        Assert.That(roadMessageAffordance.AvailabilitySummary, Does.Contain("丁力偏薄"));
        Assert.That(nightTravelAffordance.AvailabilitySummary, Does.Contain("迁徙之念"));

        int memoryCountBeforeTextureResponse = socialState.Memories.Count;
        int routePressureBeforeTextureResponse = settlement.RoutePressure;
        string familyResponsesBefore = string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode));
        string officeResponsesBefore = string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode));

        PlayerCommandResult compensation = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.PoolRunnerCompensation,
            });

        Assert.That(compensation.Accepted, Is.True);
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Strained));
        Assert.That(anchorHousehold.DebtPressure, Is.GreaterThanOrEqualTo(95),
            "Debt-heavy household texture should make local runner compensation visibly costly.");
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("本户底色"));
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("债压已高"));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeTextureResponse));
        Assert.That(settlement.RoutePressure, Is.EqualTo(routePressureBeforeTextureResponse));
        Assert.That(string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode)), Is.EqualTo(familyResponsesBefore));
        Assert.That(string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode)), Is.EqualTo(officeResponsesBefore));

        anchorHousehold.Distress = 60;
        anchorHousehold.DebtPressure = 45;
        anchorHousehold.LaborCapacity = 28;
        anchorHousehold.MigrationRisk = 58;
        anchorHousehold.DependentCount = 4;
        anchorHousehold.LaborerCount = 1;

        PlayerCommandResult roadMessage = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.SendHouseholdRoadMessage,
            });

        Assert.That(roadMessage.Accepted, Is.True);
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Strained));
        Assert.That(anchorHousehold.LaborCapacity, Is.LessThanOrEqualTo(21),
            "Labor-thin household texture should make sending a local road message eat scarce household labor.");
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("丁力偏薄"));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeTextureResponse));
        Assert.That(settlement.RoutePressure, Is.EqualTo(routePressureBeforeTextureResponse));
    }

    [Test]
    public void HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260505);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        PopulationHouseholdState anchorHousehold = SelectPlayerAnchorHouseholdForTest(populationState);
        SettlementId settlementId = anchorHousehold.SettlementId;
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(
            KnownModuleKeys.OfficeAndCareer);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);

        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 52;
        settlement.RoutePressure = 62;
        settlement.RetaliationRisk = 14;
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

        anchorHousehold.Distress = 70;
        anchorHousehold.DebtPressure = 96;
        anchorHousehold.LaborCapacity = 16;
        anchorHousehold.MigrationRisk = 82;
        anchorHousehold.DependentCount = 4;
        anchorHousehold.LaborerCount = 1;

        PresentationReadModelBundle monthNPlusOne = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot compensationAffordance = monthNPlusOne.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.PoolRunnerCompensation
                                 && affordance.SettlementId == settlementId
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);
        PlayerCommandAffordanceSnapshot nightTravelAffordance = monthNPlusOne.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.RestrictNightTravel
                                 && affordance.SettlementId == settlementId
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);
        PlayerCommandAffordanceSnapshot roadMessageAffordance = monthNPlusOne.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.SendHouseholdRoadMessage
                                 && affordance.SettlementId == settlementId
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);

        Assert.That(compensationAffordance.IsEnabled, Is.False);
        Assert.That(compensationAffordance.AvailabilitySummary, Does.Contain("回应承受线"));
        Assert.That(compensationAffordance.AvailabilitySummary, Does.Contain("债账已过线"));
        Assert.That(compensationAffordance.CostSummary, Does.Contain("承受线代价"));
        Assert.That(compensationAffordance.ReadbackSummary, Does.Contain("承受线读回"));
        Assert.That(nightTravelAffordance.IsEnabled, Is.True);
        Assert.That(nightTravelAffordance.AvailabilitySummary, Does.Contain("迁徙之念已急"));
        Assert.That(nightTravelAffordance.LeverageSummary, Does.Contain("取舍预判"));
        Assert.That(nightTravelAffordance.LeverageSummary, Does.Contain("预期收益"));
        Assert.That(nightTravelAffordance.CostSummary, Does.Contain("反噬尾巴"));
        Assert.That(nightTravelAffordance.ReadbackSummary, Does.Contain("取舍读回"));
        Assert.That(roadMessageAffordance.IsEnabled, Is.False);
        Assert.That(roadMessageAffordance.AvailabilitySummary, Does.Contain("丁力已贴地"));
        Assert.That(roadMessageAffordance.LeverageSummary, Does.Contain("路情"));
        Assert.That(roadMessageAffordance.CostSummary, Does.Contain("丁力薄"));
        Assert.That(roadMessageAffordance.ReadbackSummary, Does.Contain("外部后账"));
        Assert.That(roadMessageAffordance.ReadbackSummary, Does.Contain("承受线读回"));
        Assert.That(compensationAffordance.LeverageSummary, Does.Contain("脚户误读"));
        Assert.That(compensationAffordance.CostSummary, Does.Contain("新欠账"));

        int memoryCountBeforeResponse = socialState.Memories.Count;
        int routePressureBeforeResponse = settlement.RoutePressure;
        string orderResponseBefore = settlement.LastRefusalResponseCommandCode;
        string familyResponsesBefore = string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode));
        string officeResponsesBefore = string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode));

        PlayerCommandResult roadMessage = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = anchorHousehold.SponsorClanId,
                CommandName = PlayerCommandNames.SendHouseholdRoadMessage,
            });

        Assert.That(roadMessage.Accepted, Is.True);
        Assert.That(roadMessage.ModuleKey, Is.EqualTo(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(anchorHousehold.LastLocalResponseCommandCode, Is.EqualTo(PlayerCommandNames.SendHouseholdRoadMessage));
        Assert.That(anchorHousehold.LastLocalResponseOutcomeCode, Is.EqualTo(HouseholdLocalResponseOutcomeCodes.Strained));
        Assert.That(anchorHousehold.LastLocalResponseTraceCode, Is.EqualTo(HouseholdLocalResponseTraceCodes.HouseholdRoadMessageSent));
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("回应承受线"));
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("丁力已贴地"));
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("取舍预判"));
        Assert.That(anchorHousehold.LastLocalResponseSummary, Does.Contain("外部后账"));
        Assert.That(settlement.RoutePressure, Is.EqualTo(routePressureBeforeResponse));
        Assert.That(settlement.LastRefusalResponseCommandCode, Is.EqualTo(orderResponseBefore));
        Assert.That(string.Join("|", familyState.Clans.Select(static clan => clan.LastRefusalResponseCommandCode)), Is.EqualTo(familyResponsesBefore));
        Assert.That(string.Join("|", officeState.People.Select(static career => career.LastRefusalResponseCommandCode)), Is.EqualTo(officeResponsesBefore));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeResponse),
            "Same-month capacity-shaped local response must not write SocialMemory.");

        PresentationReadModelBundle afterResponse = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot repeatRoadMessageAffordance = afterResponse.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.SendHouseholdRoadMessage
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);
        PlayerCommandAffordanceSnapshot switchedCompensationAffordance = afterResponse.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.PoolRunnerCompensation
                                 && affordance.TargetLabel == anchorHousehold.HouseholdName);
        Assert.That(repeatRoadMessageAffordance.AvailabilitySummary, Does.Contain("续接提示"));
        Assert.That(repeatRoadMessageAffordance.AvailabilitySummary, Does.Contain("已吃紧"));
        Assert.That(repeatRoadMessageAffordance.CostSummary, Does.Contain("冷却提示"));
        Assert.That(repeatRoadMessageAffordance.CostSummary, Does.Contain("本月再压"));
        Assert.That(repeatRoadMessageAffordance.ReadbackSummary, Does.Contain("续接读回"));
        Assert.That(repeatRoadMessageAffordance.ReadbackSummary, Does.Contain("外部后账归位"));
        Assert.That(repeatRoadMessageAffordance.ReadbackSummary, Does.Contain("该走巡丁/路匪 lane"));
        Assert.That(repeatRoadMessageAffordance.ReadbackSummary, Does.Contain("本户不能代修"));
        Assert.That(switchedCompensationAffordance.LeverageSummary, Does.Contain("换招提示"));
        Assert.That(switchedCompensationAffordance.LeverageSummary, Does.Contain("外部后账"));
        Assert.That(switchedCompensationAffordance.LeverageSummary, Does.Contain("外部后账归位"));
        Assert.That(switchedCompensationAffordance.LeverageSummary, Does.Contain("该走县门/文移 lane"));
        Assert.That(switchedCompensationAffordance.LeverageSummary, Does.Contain("该走族老/担保 lane"));
        Assert.That(switchedCompensationAffordance.CostSummary, Does.Contain("换招前"));

        PlayerCommandReceiptSnapshot receipt = afterResponse.PlayerCommands.Receipts
            .First(candidate => candidate.CommandName == PlayerCommandNames.SendHouseholdRoadMessage
                                && candidate.TargetLabel == anchorHousehold.HouseholdName);
        Assert.That(receipt.Summary, Does.Contain("回应承受线"));
        Assert.That(receipt.Summary, Does.Contain("取舍预判"));
        Assert.That(receipt.LeverageSummary, Does.Contain("预期收益"));
        Assert.That(receipt.LeverageSummary, Does.Contain("外部后账"));
        Assert.That(receipt.LeverageSummary, Does.Contain("外部后账归位"));
        Assert.That(receipt.LeverageSummary, Does.Contain("该走巡丁/路匪 lane"));
        Assert.That(receipt.LeverageSummary, Does.Contain("该走县门/文移 lane"));
        Assert.That(receipt.LeverageSummary, Does.Contain("该走族老/担保 lane"));
        Assert.That(receipt.LeverageSummary, Does.Contain("本户不能代修"));
        Assert.That(receipt.LeverageSummary, Does.Contain("短期后果"));
        Assert.That(receipt.LeverageSummary, Does.Contain("缓住项"));
        Assert.That(receipt.LeverageSummary, Does.Contain("路情"));
        Assert.That(receipt.CostSummary, Does.Contain("承受线代价"));
        Assert.That(receipt.CostSummary, Does.Contain("反噬尾巴"));
        Assert.That(receipt.CostSummary, Does.Contain("挤压项"));
        Assert.That(receipt.CostSummary, Does.Contain("少丁出门"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("承受线读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("取舍读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("短期后果"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("仍欠外部后账"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("外部后账归位"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("该走巡丁/路匪 lane"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("该走县门/文移 lane"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("该走族老/担保 lane"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("本户不能代修"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("本户递信不是官署递报"));
        PlayerCommandAffordanceSnapshot orderLaneAffordance = afterResponse.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.FundLocalWatch
                                 && affordance.SettlementId == settlementId);
        Assert.That(orderLaneAffordance.ReadbackSummary, Does.Contain("外部后账归位"));
        Assert.That(orderLaneAffordance.ReadbackSummary, Does.Contain("该走巡丁/路匪 lane"));
        Assert.That(orderLaneAffordance.ReadbackSummary, Does.Contain("本户不能代修"));
        Assert.That(orderLaneAffordance.ReadbackSummary, Does.Contain("承接入口"));
        Assert.That(orderLaneAffordance.ReadbackSummary, Does.Contain("添雇巡丁"));
        Assert.That(orderLaneAffordance.ReadbackSummary, Does.Contain("补保巡丁"));

        PlayerCommandAffordanceSnapshot officeLaneAffordance = afterResponse.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.PetitionViaOfficeChannels
                                 && affordance.SettlementId == settlementId);
        Assert.That(officeLaneAffordance.LeverageSummary, Does.Contain("外部后账归位"));
        Assert.That(officeLaneAffordance.LeverageSummary, Does.Contain("该走县门/文移 lane"));
        Assert.That(officeLaneAffordance.LeverageSummary, Does.Contain("本户不能代修"));
        Assert.That(officeLaneAffordance.LeverageSummary, Does.Contain("承接入口"));
        Assert.That(officeLaneAffordance.LeverageSummary, Does.Contain("押文催县门"));
        Assert.That(officeLaneAffordance.LeverageSummary, Does.Contain("批覆词状"));
        Assert.That(afterResponse.GovernanceDocket.GuidanceSummary, Does.Contain("外部后账归位"));
        Assert.That(afterResponse.GovernanceDocket.GuidanceSummary, Does.Contain("该走县门/文移 lane"));
        Assert.That(afterResponse.GovernanceDocket.GuidanceSummary, Does.Contain("本户不能代修"));
        Assert.That(afterResponse.GovernanceDocket.GuidanceSummary, Does.Contain("承接入口"));
        Assert.That(afterResponse.GovernanceDocket.GuidanceSummary, Does.Contain("押文催县门"));

        Assert.That(anchorHousehold.SponsorClanId.HasValue, Is.True);
        PlayerCommandAffordanceSnapshot familyLaneAffordance = afterResponse.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.InviteClanEldersMediation
                                 && affordance.ClanId == anchorHousehold.SponsorClanId);
        Assert.That(familyLaneAffordance.LeverageSummary, Does.Contain("外部后账归位"));
        Assert.That(familyLaneAffordance.LeverageSummary, Does.Contain("该走族老/担保 lane"));
        Assert.That(familyLaneAffordance.LeverageSummary, Does.Contain("本户不能代修"));
        Assert.That(familyLaneAffordance.LeverageSummary, Does.Contain("承接入口"));
        Assert.That(familyLaneAffordance.LeverageSummary, Does.Contain("请族老解释"));
        Assert.That(familyLaneAffordance.LeverageSummary, Does.Contain("请族老调停"));

        int memoryCountBeforeOwnerLaneResponses = socialState.Memories.Count;
        PlayerCommandResult orderLaneReturn = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
            });
        foreach (OfficeCareerState career in officeState.People.Where(person => person.HasAppointment && person.SettlementId == settlementId))
        {
            career.JurisdictionLeverage = 4;
            career.ClerkDependence = 90;
            career.PetitionBacklog = 70;
        }

        PlayerCommandResult officeLaneReturn = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.PressCountyYamenDocument,
            });
        ClanStateData leadClanForStatus = familyState.Clans
            .Where(clan => clan.HomeSettlementId == settlementId)
            .OrderByDescending(static clan => clan.Prestige)
            .ThenBy(static clan => clan.Id.Value)
            .First();
        PlayerCommandResult familyLaneReturn = commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = leadClanForStatus.Id,
                CommandName = PlayerCommandNames.AskClanEldersExplain,
            });

        Assert.That(orderLaneReturn.Accepted, Is.True);
        Assert.That(orderLaneReturn.ModuleKey, Is.EqualTo(KnownModuleKeys.OrderAndBanditry));
        Assert.That(officeLaneReturn.Accepted, Is.True);
        Assert.That(officeLaneReturn.ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
        Assert.That(familyLaneReturn.Accepted, Is.True);
        Assert.That(familyLaneReturn.ModuleKey, Is.EqualTo(KnownModuleKeys.FamilyCore));
        Assert.That(settlement.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Repaired));
        JurisdictionAuthorityState officeJurisdictionForStatus = officeState.Jurisdictions
            .Single(jurisdiction => jurisdiction.SettlementId == settlementId);
        Assert.That(officeJurisdictionForStatus.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Escalated));
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeOwnerLaneResponses),
            "Owner-lane归口 response commands must still leave durable SocialMemory to the later monthly reader.");

        PresentationReadModelBundle afterOwnerLaneReturn = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot orderStatusAffordance = afterOwnerLaneReturn.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.FundLocalWatch
                                 && affordance.SettlementId == settlementId);
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("归口状态"));
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("已归口到巡丁/路匪 lane"));
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("补保巡丁"));
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("归口不等于修好"));
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("仍看 owner lane 下月读回"));
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("归口后读法"));
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("已修复"));
        Assert.That(orderStatusAffordance.ReadbackSummary, Does.Contain("先停本户加压"));

        PlayerCommandAffordanceSnapshot officeStatusAffordance = afterOwnerLaneReturn.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.PetitionViaOfficeChannels
                                 && affordance.SettlementId == settlementId);
        Assert.That(officeStatusAffordance.LeverageSummary, Does.Contain("归口状态"));
        Assert.That(officeStatusAffordance.LeverageSummary, Does.Contain("已归口到县门/文移 lane"));
        Assert.That(officeStatusAffordance.LeverageSummary, Does.Contain("押文催县门"));
        Assert.That(officeStatusAffordance.LeverageSummary, Does.Contain("归口不等于修好"));
        Assert.That(officeStatusAffordance.LeverageSummary, Does.Contain("归口后读法"));
        Assert.That(officeStatusAffordance.LeverageSummary, Does.Contain("恶化转硬"));
        Assert.That(officeStatusAffordance.LeverageSummary, Does.Contain("别让本户代扛"));
        Assert.That(afterOwnerLaneReturn.GovernanceDocket.GuidanceSummary, Does.Contain("归口状态"));
        Assert.That(afterOwnerLaneReturn.GovernanceDocket.GuidanceSummary, Does.Contain("已归口到县门/文移 lane"));
        Assert.That(afterOwnerLaneReturn.GovernanceDocket.GuidanceSummary, Does.Contain("仍看 owner lane 下月读回"));
        Assert.That(afterOwnerLaneReturn.GovernanceDocket.GuidanceSummary, Does.Contain("归口后读法"));
        Assert.That(afterOwnerLaneReturn.GovernanceDocket.GuidanceSummary, Does.Contain("恶化转硬"));

        PlayerCommandAffordanceSnapshot familyStatusAffordance = afterOwnerLaneReturn.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.InviteClanEldersMediation
                                 && affordance.ClanId == leadClanForStatus.Id);
        Assert.That(familyStatusAffordance.LeverageSummary, Does.Contain("归口状态"));
        Assert.That(familyStatusAffordance.LeverageSummary, Does.Contain("已归口到族老/担保 lane"));
        Assert.That(familyStatusAffordance.LeverageSummary, Does.Contain("请族老解释"));
        Assert.That(familyStatusAffordance.LeverageSummary, Does.Contain("归口不等于修好"));
        Assert.That(familyStatusAffordance.LeverageSummary, Does.Contain("归口后读法"));

        simulation.AdvanceOneMonth();
        Assert.That(socialState.Memories, Has.Count.GreaterThan(memoryCountBeforeOwnerLaneResponses),
            "Later monthly SocialMemory reader should own durable residue after owner-lane responses.");

        PresentationReadModelBundle afterSocialResidueReadback = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot orderResidueAffordance = afterSocialResidueReadback.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.FundLocalWatch
                                 && affordance.SettlementId == settlementId);
        Assert.That(orderResidueAffordance.ReadbackSummary, Does.Contain("社会余味读回"));
        Assert.That(orderResidueAffordance.ReadbackSummary, Does.Contain("后账渐平"));
        Assert.That(orderResidueAffordance.ReadbackSummary, Does.Contain("仍由 SocialMemoryAndRelations 后续沉淀"));
        Assert.That(orderResidueAffordance.ReadbackSummary, Does.Contain("不是本户再修"));

        PlayerCommandAffordanceSnapshot officeResidueAffordance = afterSocialResidueReadback.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.PetitionViaOfficeChannels
                                 && affordance.SettlementId == settlementId);
        Assert.That(officeResidueAffordance.LeverageSummary, Does.Contain("社会余味读回"));
        Assert.That(officeResidueAffordance.LeverageSummary, Does.Contain("后账转硬"));
        Assert.That(officeResidueAffordance.LeverageSummary, Does.Contain("仍由 SocialMemoryAndRelations 后续沉淀"));
        Assert.That(officeResidueAffordance.LeverageSummary, Does.Contain("不是本户再修"));
        Assert.That(afterSocialResidueReadback.GovernanceDocket.GuidanceSummary, Does.Contain("社会余味读回"));
        Assert.That(afterSocialResidueReadback.GovernanceDocket.GuidanceSummary, Does.Contain("后账转硬"));

        PlayerCommandAffordanceSnapshot familyResidueAffordance = afterSocialResidueReadback.PlayerCommands.Affordances
            .First(affordance => affordance.CommandName == PlayerCommandNames.InviteClanEldersMediation
                                 && affordance.ClanId == leadClanForStatus.Id);
        Assert.That(familyResidueAffordance.LeverageSummary, Does.Contain("社会余味读回"));
        Assert.That(familyResidueAffordance.LeverageSummary, Does.Contain("仍由 SocialMemoryAndRelations 后续沉淀"));
        Assert.That(familyResidueAffordance.LeverageSummary, Does.Contain("不是本户再修"));
    }

    private static SettlementId SelectSettlementWithDisorder(PresentationReadModelBundle bundle)
    {
        return bundle.GovernanceSettlements
            .Select(static lane => lane.SettlementId)
            .First(id => bundle.SettlementDisorder.Any(disorder => disorder.SettlementId == id));
    }

    private static void SetRelievedLocalResponseHouseholdForTest(PopulationHouseholdState household)
    {
        household.Distress = 58;
        household.DebtPressure = 48;
        household.LaborCapacity = 42;
        household.MigrationRisk = 72;
        household.DependentCount = 2;
        household.LaborerCount = 3;
    }

    private static PopulationHouseholdState SelectPlayerAnchorHouseholdForTest(PopulationAndHouseholdsState state)
    {
        return state.Households
            .OrderByDescending(static household => household.SponsorClanId.HasValue)
            .ThenBy(static household => household.SettlementId.Value)
            .ThenBy(static household => household.Id.Value)
            .First();
    }
}
