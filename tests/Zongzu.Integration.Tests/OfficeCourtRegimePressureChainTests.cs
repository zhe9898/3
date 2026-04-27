using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class OfficeCourtRegimePressureChainTests
{
    [Test]
    public void Chain7_RealScheduler_ClerkCaptureDrainsIntoScopedPublicLifeAndDoesNotRepeat()
    {
        FeatureManifest manifest = BuildManifest(includePublicLife: true);
        IReadOnlyList<IModuleRunner> modules = BuildModules(includePublicLife: true);
        Dictionary<string, object> states = BuildStates(modules);
        SeedWorld((WorldSettlementsState)states[KnownModuleKeys.WorldSettlements]);

        OfficeAndCareerState officeState = (OfficeAndCareerState)states[KnownModuleKeys.OfficeAndCareer];
        officeState.People.Add(MakeOfficial(1, 10, authorityTier: 3, clerk: 35, backlog: 20));
        officeState.People.Add(MakeOfficial(2, 20, authorityTier: 3, clerk: 10, backlog: 5));

        PublicLifeAndRumorState publicLifeState = (PublicLifeAndRumorState)states[KnownModuleKeys.PublicLifeAndRumor];
        publicLifeState.Settlements.Add(MakePublicLife(10));
        publicLifeState.Settlements.Add(MakePublicLife(20));

        MonthlyScheduler scheduler = new();
        SimulationMonthResult first = scheduler.AdvanceOneMonth(
            new GameDate(1022, 6),
            manifest,
            new DeterministicRandom(KernelState.Create(789)),
            states,
            modules);

        IDomainEvent[] firstEvents = first.DomainEvents.ToArray();
        Assert.That(
            firstEvents,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.ClerkCaptureDeepened
                     && e.EntityKey == "10"
                     && e.Metadata.ContainsKey(DomainEventMetadataKeys.ClerkCapturePressure)
                     && e.Metadata.ContainsKey(DomainEventMetadataKeys.ClerkCaptureBacklogPressure)
                     && e.Metadata.ContainsKey(DomainEventMetadataKeys.ClerkCaptureAuthorityBuffer)));
        Assert.That(
            firstEvents,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.ClerkCaptureDeepened
                     && e.EntityKey == "20"),
            "Clerk capture must remain settlement-scoped.");

        SettlementPublicLifeState affected = publicLifeState.Settlements.Single(static s => s.SettlementId == new SettlementId(10));
        SettlementPublicLifeState unaffected = publicLifeState.Settlements.Single(static s => s.SettlementId == new SettlementId(20));
        Assert.That(affected.LastPublicTrace, Does.Contain("书吏坐大"));
        Assert.That(unaffected.LastPublicTrace, Does.Not.Contain("书吏坐大"));

        SimulationMonthResult second = scheduler.AdvanceOneMonth(
            new GameDate(1022, 7),
            manifest,
            new DeterministicRandom(KernelState.Create(790)),
            states,
            modules);

        Assert.That(
            second.DomainEvents,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == OfficeAndCareerEventNames.ClerkCaptureDeepened),
            "An active clerk-capture condition must not re-emit until it clears.");
    }

    [Test]
    public void Chain8_RealScheduler_CourtAgendaDrainsIntoOnePolicyImplementation()
    {
        FeatureManifest manifest = BuildManifest(includePublicLife: false);
        IReadOnlyList<IModuleRunner> modules = BuildModules(includePublicLife: false);
        Dictionary<string, object> states = BuildStates(modules);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        SeedWorld(worldState);
        worldState.CurrentSeason.Imperial.MandateConfidence = 30;

        OfficeAndCareerState officeState = (OfficeAndCareerState)states[KnownModuleKeys.OfficeAndCareer];
        officeState.People.Add(MakeOfficial(1, 10, authorityTier: 3, leverage: 60));
        officeState.People.Add(MakeOfficial(2, 20, authorityTier: 3, leverage: 10));

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 6),
            manifest,
            new DeterministicRandom(KernelState.Create(891)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.CourtAgendaPressureAccumulated
                     && e.EntityKey == "court"));
        IDomainEvent[] windows = events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.PolicyWindowOpened)
            .ToArray();
        Assert.That(windows.Length, Is.EqualTo(1),
            "Court agenda pressure must not fan out into every jurisdiction.");
        Assert.That(windows[0].EntityKey, Is.EqualTo("10"));
        Assert.That(windows[0].Metadata[DomainEventMetadataKeys.PolicyWindowPressure], Is.EqualTo("84"));
        Assert.That(windows[0].Metadata[DomainEventMetadataKeys.PolicyWindowMandateDeficit], Is.EqualTo("10"));
        Assert.That(windows[0].Metadata[DomainEventMetadataKeys.PolicyWindowAuthoritySignal], Is.EqualTo("54"));
        Assert.That(windows[0].Metadata[DomainEventMetadataKeys.PolicyWindowLeverageSignal], Is.EqualTo("20"));
        Assert.That(windows[0].Metadata[DomainEventMetadataKeys.PolicyWindowAdministrativeDrag], Is.EqualTo("0"));
        Assert.That(windows[0].Metadata[DomainEventMetadataKeys.PolicyWindowClerkDrag], Is.EqualTo("0"));
        Assert.That(windows[0].Metadata[DomainEventMetadataKeys.PolicyWindowBacklogDrag], Is.EqualTo("0"));

        IDomainEvent[] implementations = events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.PolicyImplemented)
            .ToArray();
        Assert.That(implementations.Length, Is.EqualTo(1),
            "The scheduler's bounded drain must let the policy window resolve into one Office-owned implementation outcome in the same month.");
        Assert.That(implementations[0].EntityKey, Is.EqualTo("10"));
        Assert.That(
            implementations[0].Metadata[DomainEventMetadataKeys.SourceEventType],
            Is.EqualTo(OfficeAndCareerEventNames.PolicyWindowOpened));
        Assert.That(
            implementations[0].Metadata[DomainEventMetadataKeys.PolicyImplementationOutcome],
            Is.EqualTo(DomainEventMetadataValues.PolicyImplementationRapid));
        Assert.That(implementations[0].Metadata[DomainEventMetadataKeys.PolicyImplementationScore], Is.EqualTo("100"));
        Assert.That(implementations[0].Metadata[DomainEventMetadataKeys.PolicyImplementationWindowPressure], Is.EqualTo("84"));
        Assert.That(implementations[0].Metadata[DomainEventMetadataKeys.PolicyImplementationDocketDrag], Is.EqualTo("0"));
        Assert.That(implementations[0].Metadata[DomainEventMetadataKeys.PolicyImplementationClerkCapture], Is.EqualTo("0"));
        Assert.That(implementations[0].Metadata[DomainEventMetadataKeys.PolicyImplementationLocalBuffer], Is.EqualTo("44"));
        Assert.That(implementations[0].Metadata[DomainEventMetadataKeys.PolicyImplementationPaperCompliance], Is.EqualTo("59"));
        Assert.That(
            Array.IndexOf(events, implementations[0]),
            Is.GreaterThan(Array.IndexOf(events, windows[0])),
            "PolicyImplemented must be downstream of PolicyWindowOpened, not a parallel application-layer shortcut.");
    }

    [Test]
    public void Chain8_OfficePolicyImplementationProjectsGovernanceReadbackAndNextMonthSocialResidue()
    {
        FeatureManifest manifest = BuildManifest(includePublicLife: true);
        IReadOnlyList<IModuleRunner> modules = BuildModules(includePublicLife: true);
        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1022, 6),
            KernelState.Create(893),
            manifest,
            modules);

        SeedWorld(simulation.GetModuleStateForTesting<WorldSettlementsState>(KnownModuleKeys.WorldSettlements));
        simulation.GetModuleStateForTesting<WorldSettlementsState>(KnownModuleKeys.WorldSettlements)
            .CurrentSeason.Imperial.MandateConfidence = 30;

        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(KnownModuleKeys.FamilyCore);
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Qinghe Zhang",
            HomeSettlementId = new SettlementId(10),
            Prestige = 55,
            SupportReserve = 44,
            HeirPersonId = new PersonId(1),
        });

        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(KnownModuleKeys.OfficeAndCareer);
        officeState.People.Add(MakeOfficial(1, 10, authorityTier: 3, leverage: 60));
        officeState.People.Add(MakeOfficial(2, 20, authorityTier: 3, leverage: 10));

        PublicLifeAndRumorState publicLifeState =
            simulation.GetModuleStateForTesting<PublicLifeAndRumorState>(KnownModuleKeys.PublicLifeAndRumor);
        publicLifeState.Settlements.Add(MakePublicLife(10));
        publicLifeState.Settlements.Add(MakePublicLife(20));

        SimulationMonthResult first = simulation.AdvanceOneMonth();

        Assert.That(
            first.DomainEvents,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.PolicyImplemented
                     && e.EntityKey == "10"
                     && e.Metadata[DomainEventMetadataKeys.PolicyImplementationOutcome]
                     == DomainEventMetadataValues.PolicyImplementationRapid));
        SettlementPublicLifeState affectedPublicLife =
            publicLifeState.Settlements.Single(static settlement => settlement.SettlementId == new SettlementId(10));
        Assert.That(affectedPublicLife.OfficialNoticeLine, Does.Contain("县门执行读回"));
        Assert.That(affectedPublicLife.OfficialNoticeLine, Does.Contain("OfficeAndCareer"));
        Assert.That(affectedPublicLife.OfficialNoticeLine, Does.Contain("本户不能代修"));
        Assert.That(affectedPublicLife.OfficialNoticeLine, Does.Contain("政策语气读回"));
        Assert.That(affectedPublicLife.OfficialNoticeLine, Does.Contain("县门承接姿态"));
        Assert.That(affectedPublicLife.PrefectureDispatchLine, Does.Contain("文移指向读回"));
        Assert.That(affectedPublicLife.PrefectureDispatchLine, Does.Contain("朝廷后手仍不直写地方"));
        Assert.That(affectedPublicLife.ContentionSummary, Does.Contain("公议承压读法"));
        Assert.That(affectedPublicLife.ChannelSummary, Does.Contain("不是本户硬扛朝廷后账"));

        SocialMemoryAndRelationsState socialState =
            simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(KnownModuleKeys.SocialMemoryAndRelations);
        Assert.That(
            socialState.Memories,
            Has.None.Matches<MemoryRecordState>(
                memory => memory.CauseKey.StartsWith("office.policy_implementation.", StringComparison.Ordinal)),
            "SocialMemory runs earlier in the same month and must not write immediate command-time residue.");

        PresentationReadModelBundle afterFirst = new PresentationReadModelBuilder().BuildForM2(simulation);
        SettlementGovernanceLaneSnapshot governance =
            afterFirst.GovernanceSettlements.Single(static lane => lane.SettlementId == new SettlementId(10));
        Assert.That(governance.OfficeImplementationReadbackSummary, Does.Contain("县门执行读回"));
        Assert.That(governance.OfficeImplementationReadbackSummary, Does.Contain("OfficeAndCareer lane"));
        Assert.That(governance.OfficeNextStepReadbackSummary, Does.Contain("县门/文移后手"));
        Assert.That(governance.OfficeLaneEntryReadbackSummary, Does.Contain("Office承接入口"));
        Assert.That(governance.OfficeLaneEntryReadbackSummary, Does.Contain("该走县门/文移 lane"));
        Assert.That(governance.OfficeLaneNoLoopGuardSummary, Does.Contain("Office闭环防回压"));
        Assert.That(governance.OfficeLaneNoLoopGuardSummary, Does.Contain("本户不再代修"));
        Assert.That(governance.CourtPolicyEntryReadbackSummary, Does.Contain("朝议压力读回"));
        Assert.That(governance.CourtPolicyEntryReadbackSummary, Does.Contain("政策窗口读回"));
        Assert.That(governance.CourtPolicyEntryReadbackSummary, Does.Contain("政策语气读回"));
        Assert.That(governance.CourtPolicyDispatchReadbackSummary, Does.Contain("文移到达读回"));
        Assert.That(governance.CourtPolicyDispatchReadbackSummary, Does.Contain("县门执行承接读回"));
        Assert.That(governance.CourtPolicyDispatchReadbackSummary, Does.Contain("文移指向读回"));
        Assert.That(governance.CourtPolicyDispatchReadbackSummary, Does.Contain("县门承接姿态"));
        Assert.That(governance.CourtPolicyPublicReadbackSummary, Does.Contain("公议读法读回"));
        Assert.That(governance.CourtPolicyPublicReadbackSummary, Does.Contain("Office/PublicLife分读"));
        Assert.That(governance.CourtPolicyPublicReadbackSummary, Does.Contain("公议承压读法"));
        Assert.That(governance.CourtPolicyNoLoopGuardSummary, Does.Contain("Court-policy防回压"));
        Assert.That(governance.CourtPolicyNoLoopGuardSummary, Does.Contain("朝廷后手仍不直写地方"));
        Assert.That(governance.CourtPolicyNoLoopGuardSummary, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(governance.GovernanceSummary, Does.Contain("县门执行读回"));
        Assert.That(governance.GovernanceSummary, Does.Contain("朝议压力读回"));
        Assert.That(governance.GovernanceSummary, Does.Contain("Court-policy防回压"));
        Assert.That(afterFirst.GovernanceDocket.OfficeImplementationReadbackSummary, Does.Contain("县门执行读回"));
        Assert.That(afterFirst.GovernanceDocket.OfficeLaneEntryReadbackSummary, Does.Contain("Office承接入口"));
        Assert.That(afterFirst.GovernanceDocket.OfficeLaneNoLoopGuardSummary, Does.Contain("Office闭环防回压"));
        Assert.That(afterFirst.GovernanceDocket.CourtPolicyEntryReadbackSummary, Does.Contain("朝议压力读回"));
        Assert.That(afterFirst.GovernanceDocket.CourtPolicyDispatchReadbackSummary, Does.Contain("文移到达读回"));
        Assert.That(afterFirst.GovernanceDocket.CourtPolicyPublicReadbackSummary, Does.Contain("公议读法读回"));
        Assert.That(afterFirst.GovernanceDocket.CourtPolicyNoLoopGuardSummary, Does.Contain("Court-policy防回压"));
        Assert.That(afterFirst.GovernanceDocket.WhyNowSummary, Does.Contain("政策窗口读回"));
        Assert.That(afterFirst.GovernanceDocket.GuidanceSummary, Does.Contain("县门/文移后手"));
        Assert.That(afterFirst.GovernanceDocket.GuidanceSummary, Does.Contain("Office承接入口"));
        Assert.That(afterFirst.GovernanceDocket.GuidanceSummary, Does.Contain("Office闭环防回压"));
        Assert.That(afterFirst.GovernanceDocket.GuidanceSummary, Does.Contain("Office/PublicLife分读"));

        simulation.AdvanceOneMonth();

        Assert.That(
            socialState.Memories,
            Has.Some.Matches<MemoryRecordState>(
                memory => memory.CauseKey.StartsWith("office.policy_implementation.10.", StringComparison.Ordinal)
                          && memory.Kind == SocialMemoryKinds.OfficePolicyImplementationResidue),
            "The next monthly SocialMemory pass may read Office's structured jurisdiction aftermath and write durable residue.");
        MemoryRecordState residue = socialState.Memories.Single(
            memory => memory.CauseKey.StartsWith("office.policy_implementation.10.", StringComparison.Ordinal));
        Assert.That(residue.Summary, Does.Contain("县门执行读回"));
        Assert.That(residue.Summary, Does.Contain("OfficeAndCareer"));
        Assert.That(residue.Summary, Does.Not.Contain("LastPetitionOutcome"));
        Assert.That(residue.Summary, Does.Not.Contain("DomainEvent.Summary"));

        PresentationReadModelBundle afterSecond = new PresentationReadModelBuilder().BuildForM2(simulation);
        SettlementGovernanceLaneSnapshot afterSecondGovernance =
            afterSecond.GovernanceSettlements.Single(static lane => lane.SettlementId == new SettlementId(10));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("Office余味续接读回"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("SocialMemoryAndRelations"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("不是本户再修"));
    }

    [Test]
    public void Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue()
    {
        FeatureManifest manifest = BuildManifest(includePublicLife: true);
        IReadOnlyList<IModuleRunner> modules = BuildModules(includePublicLife: true);
        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1022, 6),
            KernelState.Create(894),
            manifest,
            modules);

        SeedWorld(simulation.GetModuleStateForTesting<WorldSettlementsState>(KnownModuleKeys.WorldSettlements));
        simulation.GetModuleStateForTesting<WorldSettlementsState>(KnownModuleKeys.WorldSettlements)
            .CurrentSeason.Imperial.MandateConfidence = 30;

        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(KnownModuleKeys.FamilyCore);
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Qinghe Zhang",
            HomeSettlementId = new SettlementId(10),
            Prestige = 55,
            SupportReserve = 44,
            HeirPersonId = new PersonId(1),
        });

        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(KnownModuleKeys.OfficeAndCareer);
        officeState.People.Add(MakeOfficial(
            1,
            10,
            authorityTier: 3,
            clerk: 35,
            backlog: 40,
            leverage: 20,
            petition: 30,
            reputation: 50));
        officeState.People.Single(static career => career.PersonId == new PersonId(1)).AdministrativeTaskLoad = 50;
        officeState.People.Add(MakeOfficial(2, 20, authorityTier: 3, leverage: 10));

        PublicLifeAndRumorState publicLifeState =
            simulation.GetModuleStateForTesting<PublicLifeAndRumorState>(KnownModuleKeys.PublicLifeAndRumor);
        publicLifeState.Settlements.Add(MakePublicLife(10));
        publicLifeState.Settlements.Add(MakePublicLife(20));

        simulation.AdvanceOneMonth();

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle afterFirst = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot pressAffordance = afterFirst.PlayerCommands.Affordances
            .Single(affordance => affordance.SettlementId == new SettlementId(10)
                                  && affordance.CommandName == PlayerCommandNames.PressCountyYamenDocument);
        Assert.That(pressAffordance.IsEnabled, Is.True);
        Assert.That(pressAffordance.LeverageSummary, Does.Contain("政策回应入口"));
        Assert.That(pressAffordance.LeverageSummary, Does.Contain("文移续接选择"));
        Assert.That(pressAffordance.LeverageSummary, Does.Contain("公议降温只读回"));
        Assert.That(pressAffordance.LeverageSummary, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(pressAffordance.ExecutionSummary, Does.Contain("OfficeAndCareer"));
        Assert.That(pressAffordance.ExecutionSummary, Does.Contain("不计算政策成败"));

        PlayerCommandAffordanceSnapshot redirectAffordance = afterFirst.PlayerCommands.Affordances
            .Single(affordance => affordance.SettlementId == new SettlementId(10)
                                  && affordance.CommandName == PlayerCommandNames.RedirectRoadReport);
        Assert.That(redirectAffordance.LeverageSummary, Does.Contain("政策回应入口"));
        Assert.That(redirectAffordance.ExecutionSummary, Does.Contain("projected fields"));

        SettlementGovernanceLaneSnapshot governance =
            afterFirst.GovernanceSettlements.Single(static lane => lane.SettlementId == new SettlementId(10));
        Assert.That(governance.SuggestedCommandName, Is.EqualTo(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(governance.SuggestedCommandPrompt, Does.Contain("政策回应入口"));
        Assert.That(governance.SuggestedCommandPrompt, Does.Contain("文移续接选择"));

        SocialMemoryAndRelationsState socialState =
            simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(KnownModuleKeys.SocialMemoryAndRelations);
        int memoryCountBeforeCommand = socialState.Memories.Count;
        PlayerCommandResult response = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = new SettlementId(10),
                CommandName = PlayerCommandNames.PressCountyYamenDocument,
            });

        Assert.That(response.Accepted, Is.True);
        Assert.That(response.Summary, Does.Contain("政策文移续接"));
        OfficeCareerState affectedOfficial = officeState.People.Single(static career => career.PersonId == new PersonId(1));
        OfficeCareerState offScopeOfficial = officeState.People.Single(static career => career.PersonId == new PersonId(2));
        Assert.That(affectedOfficial.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(affectedOfficial.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Contained));
        Assert.That(affectedOfficial.LastRefusalResponseSummary, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(offScopeOfficial.LastRefusalResponseCommandCode, Is.Empty);
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeCommand),
            "Same-month policy local response may write Office structured aftermath, but must not write durable SocialMemory residue.");

        PresentationReadModelBundle afterCommand = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = afterCommand.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == new SettlementId(10)
                              && receipt.CommandName == PlayerCommandNames.PressCountyYamenDocument);
        Assert.That(receipt.ReadbackSummary, Does.Contain("政策回应入口"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("Court-policy"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("OfficeAndCareer"));

        simulation.AdvanceOneMonth();

        Assert.That(
            socialState.Memories,
            Has.Some.Matches<MemoryRecordState>(
                memory => memory.CauseKey.StartsWith("office.policy_local_response.10.", StringComparison.Ordinal)
                          && memory.Kind == SocialMemoryKinds.OfficePolicyLocalResponseResidue),
            "The later SocialMemory pass may read Office's structured local-response aftermath and write durable residue.");
        Assert.That(
            socialState.Memories,
            Has.None.Matches<MemoryRecordState>(
                memory => memory.CauseKey.StartsWith("order.public_life.response.OfficeAndCareer", StringComparison.Ordinal)),
            "Court-policy local response residue must not be mislabeled as an Order/PublicLife response debt.");

        MemoryRecordState localResponseResidue = socialState.Memories.Single(
            memory => memory.CauseKey.StartsWith("office.policy_local_response.10.", StringComparison.Ordinal));
        Assert.That(localResponseResidue.Summary, Does.Contain("政策回应读回"));
        Assert.That(localResponseResidue.Summary, Does.Contain("OfficeAndCareer/PublicLifeAndRumor"));
        Assert.That(localResponseResidue.Summary, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(localResponseResidue.Summary, Does.Not.Contain("政策文移续接"));

        PresentationReadModelBundle afterSecond = builder.BuildForM2(simulation);
        SettlementGovernanceLaneSnapshot afterSecondGovernance =
            afterSecond.GovernanceSettlements.Single(static lane => lane.SettlementId == new SettlementId(10));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("政策回应余味续接读回"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("SocialMemoryAndRelations"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("政策旧账回压读回"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("旧文移余味"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("下一次政策窗口读法"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("公议旧读法续压"));
        Assert.That(afterSecondGovernance.OfficeLaneResidueFollowUpSummary, Does.Contain("不是本户硬扛朝廷旧账"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("政策公议旧读回"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("公议旧账回声"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("政策公议后手提示"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("公议轻续提示"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("下一步仍看榜示/递报承口"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("不是冷却账本"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("PublicLife只读街面解释"));
        Assert.That(afterSecondGovernance.CourtPolicyPublicReadbackSummary, Does.Contain("县门承接仍归OfficeAndCareer"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("政策后手案牍防误读"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("公议后手只作案牍提示"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("不是Order后账"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("不是Office成败"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("仍等Office/PublicLife/SocialMemory分读"));
        Assert.That(afterSecond.GovernanceDocket.CourtPolicyNoLoopGuardSummary, Does.Contain("政策后手案牍防误读"));
        Assert.That(afterSecond.GovernanceDocket.GuidanceSummary, Does.Contain("不是冷却账本"));
        Assert.That(afterSecond.GovernanceDocket.GuidanceSummary, Does.Contain("不是Order后账"));
        Assert.That(afterSecond.GovernanceDocket.GuidanceSummary, Does.Contain("不是Office成败"));
        Assert.That(afterSecondGovernance.SuggestedCommandPrompt, Does.Contain("建议动作防误读"));
        Assert.That(afterSecondGovernance.SuggestedCommandPrompt, Does.Contain("只承接已投影的政策公议后手"));
        Assert.That(afterSecondGovernance.SuggestedCommandPrompt, Does.Contain("不是Order后账"));
        Assert.That(afterSecond.GovernanceDocket.SuggestedCommandPrompt, Does.Contain("建议动作防误读"));
        Assert.That(afterSecond.GovernanceDocket.GuidanceSummary, Does.Contain("只承接已投影的政策公议后手"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("回执案牍一致防误读"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("回执只回收已投影的政策公议后手"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("案牍不把回执读成新政策结果"));
        Assert.That(afterSecondGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("仍等Office/PublicLife/SocialMemory分读"));
        Assert.That(afterSecond.GovernanceDocket.CourtPolicyNoLoopGuardSummary, Does.Contain("回执案牍一致防误读"));
        Assert.That(afterSecond.GovernanceDocket.GuidanceSummary, Does.Contain("回执只回收已投影的政策公议后手"));
        Assert.That(afterSecond.GovernanceDocket.GuidanceSummary, Does.Contain("案牍不把回执读成新政策结果"));
        PlayerCommandAffordanceSnapshot noticeAffordance = afterSecond.PlayerCommands.Affordances
            .Single(affordance => affordance.SettlementId == new SettlementId(10)
                                  && affordance.CommandName == PlayerCommandNames.PostCountyNotice);
        Assert.That(noticeAffordance.LeverageSummary, Does.Contain("政策公议旧读回"));
        Assert.That(noticeAffordance.LeverageSummary, Does.Contain("下一次榜示/递报旧读法"));
        Assert.That(noticeAffordance.LeverageSummary, Does.Contain("政策公议后手提示"));
        Assert.That(noticeAffordance.LeverageSummary, Does.Contain("公议轻续提示"));
        Assert.That(noticeAffordance.LeverageSummary, Does.Contain("不是冷却账本"));
        Assert.That(noticeAffordance.ReadbackSummary, Does.Contain("公议旧账回声"));
        Assert.That(noticeAffordance.ReadbackSummary, Does.Contain("下一步仍看榜示/递报承口"));
        PlayerCommandAffordanceSnapshot roadReportAffordance = afterSecond.PlayerCommands.Affordances
            .Single(affordance => affordance.SettlementId == new SettlementId(10)
                                  && affordance.CommandName == PlayerCommandNames.DispatchRoadReport);
        Assert.That(roadReportAffordance.LeverageSummary, Does.Contain("政策公议旧读回"));
        Assert.That(roadReportAffordance.LeverageSummary, Does.Contain("政策公议后手提示"));
        Assert.That(roadReportAffordance.ReadbackSummary, Does.Contain("不是本户硬扛朝廷旧账"));
        SettlementGovernanceLaneSnapshot offScopeGovernance =
            afterSecond.GovernanceSettlements.Single(static lane => lane.SettlementId == new SettlementId(20));
        Assert.That(offScopeGovernance.OfficeLaneResidueFollowUpSummary, Does.Not.Contain("政策旧账回压读回"));
        Assert.That(offScopeGovernance.CourtPolicyPublicReadbackSummary, Does.Not.Contain("政策公议旧读回"));
        Assert.That(offScopeGovernance.CourtPolicyPublicReadbackSummary, Does.Not.Contain("政策公议后手提示"));
        Assert.That(offScopeGovernance.CourtPolicyNoLoopGuardSummary, Does.Not.Contain("政策后手案牍防误读"));
        Assert.That(offScopeGovernance.SuggestedCommandPrompt, Does.Not.Contain("建议动作防误读"));
        Assert.That(offScopeGovernance.CourtPolicyNoLoopGuardSummary, Does.Not.Contain("回执案牍一致防误读"));
        PlayerCommandAffordanceSnapshot offScopeNoticeAffordance = afterSecond.PlayerCommands.Affordances
            .Single(affordance => affordance.SettlementId == new SettlementId(20)
                                  && affordance.CommandName == PlayerCommandNames.PostCountyNotice);
        Assert.That(offScopeNoticeAffordance.LeverageSummary, Does.Not.Contain("政策公议旧读回"));
        Assert.That(offScopeNoticeAffordance.LeverageSummary, Does.Not.Contain("政策公议后手提示"));

        Assert.That(afterSecondGovernance.SuggestedCommandName, Is.Not.Empty);
        int memoryCountBeforeSuggestedReceiptCommand = socialState.Memories.Count;
        PlayerCommandResult suggestedReceiptResponse = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = new SettlementId(10),
                CommandName = afterSecondGovernance.SuggestedCommandName,
            });

        Assert.That(suggestedReceiptResponse.Accepted, Is.True);
        Assert.That(socialState.Memories, Has.Count.EqualTo(memoryCountBeforeSuggestedReceiptCommand),
            "Suggested receipt readback may reuse projected court-policy residue, but same-month command handling must not write durable residue.");
        PresentationReadModelBundle afterSuggestedReceipt = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot suggestedReceipt = afterSuggestedReceipt.PlayerCommands.Receipts
            .First(receipt => receipt.SettlementId == new SettlementId(10)
                              && receipt.CommandName == afterSecondGovernance.SuggestedCommandName);
        Assert.That(suggestedReceipt.ReadbackSummary, Does.Contain("建议回执防误读"));
        Assert.That(suggestedReceipt.ReadbackSummary, Does.Contain("只回收已投影的政策公议后手"));
        Assert.That(suggestedReceipt.ReadbackSummary, Does.Contain("回执不是新政策结果"));
        Assert.That(suggestedReceipt.ReadbackSummary, Does.Contain("不是Order后账"));
        Assert.That(suggestedReceipt.ReadbackSummary, Does.Contain("仍等Office/PublicLife/SocialMemory分读"));
        SettlementGovernanceLaneSnapshot afterSuggestedReceiptGovernance =
            afterSuggestedReceipt.GovernanceSettlements.Single(static lane => lane.SettlementId == new SettlementId(10));
        Assert.That(afterSuggestedReceiptGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("回执案牍一致防误读"));
        Assert.That(afterSuggestedReceiptGovernance.CourtPolicyNoLoopGuardSummary, Does.Contain("案牍不把回执读成新政策结果"));
        Assert.That(afterSuggestedReceipt.GovernanceDocket.GuidanceSummary, Does.Contain("回执只回收已投影的政策公议后手"));
        Assert.That(afterSuggestedReceipt.PlayerCommands.Receipts
            .Where(static receipt => receipt.SettlementId == new SettlementId(20))
            .Any(static receipt => receipt.ReadbackSummary.Contains("建议回执防误读", StringComparison.Ordinal)), Is.False);
    }

    [Test]
    public void Chain9_RealScheduler_RegimePressureDefectsOnlyOneHighRiskOfficial()
    {
        FeatureManifest manifest = BuildManifest(includePublicLife: false);
        IReadOnlyList<IModuleRunner> modules = BuildModules(includePublicLife: false);
        Dictionary<string, object> states = BuildStates(modules);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        SeedWorld(worldState);
        worldState.CurrentSeason.Imperial.MandateConfidence = 20;

        OfficeAndCareerState officeState = (OfficeAndCareerState)states[KnownModuleKeys.OfficeAndCareer];
        officeState.People.Add(MakeOfficial(
            1,
            10,
            authorityTier: 2,
            demotion: 90,
            clerk: 60,
            petition: 70,
            reputation: 0));
        officeState.People.Add(MakeOfficial(
            2,
            20,
            authorityTier: 4,
            demotion: 10,
            clerk: 5,
            petition: 10,
            reputation: 70));

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 6),
            manifest,
            new DeterministicRandom(KernelState.Create(992)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.RegimeLegitimacyShifted
                     && e.EntityKey == "regime"));
        IDomainEvent[] defections = events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.OfficeDefected)
            .ToArray();
        Assert.That(defections.Length, Is.EqualTo(1),
            "Regime pressure must resolve through risk allocation, not all-official defection.");
        Assert.That(defections[0].EntityKey, Is.EqualTo("1"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionRisk], Is.EqualTo("100"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionBaselinePressure], Is.EqualTo("35"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionMandateDeficit], Is.EqualTo("5"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionDemotionPressure], Is.EqualTo("45"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionClerkPressure], Is.EqualTo("20"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionPetitionPressure], Is.EqualTo("17"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionReputationStrain], Is.EqualTo("20"));
        Assert.That(defections[0].Metadata[DomainEventMetadataKeys.DefectionAuthorityBuffer], Is.EqualTo("8"));
        Assert.That(officeState.People.Single(static p => p.PersonId == new PersonId(1)).HasAppointment, Is.False);
        Assert.That(officeState.People.Single(static p => p.PersonId == new PersonId(2)).HasAppointment, Is.True);
    }

    private static FeatureManifest BuildManifest(bool includePublicLife)
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        if (includePublicLife)
        {
            manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
        }

        return manifest;
    }

    private static IReadOnlyList<IModuleRunner> BuildModules(bool includePublicLife)
    {
        List<IModuleRunner> modules =
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new EducationAndExamsModule(),
            new SocialMemoryAndRelationsModule(),
            new OfficeAndCareerModule(),
        ];
        if (includePublicLife)
        {
            modules.Add(new PublicLifeAndRumorModule());
        }

        return modules;
    }

    private static Dictionary<string, object> BuildStates(IReadOnlyList<IModuleRunner> modules)
    {
        return modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);
    }

    private static void SeedWorld(WorldSettlementsState state)
    {
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(10),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 60,
            Prosperity = 60,
        });
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(20),
            Name = "North Ford",
            Tier = SettlementTier.MarketTown,
            NodeKind = SettlementNodeKind.MarketTown,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 60,
            Prosperity = 60,
        });
    }

    private static OfficeCareerState MakeOfficial(
        int personId,
        int settlementId,
        int authorityTier,
        int clerk = 0,
        int backlog = 0,
        int leverage = 0,
        int demotion = 0,
        int petition = 0,
        int reputation = 40)
    {
        return new OfficeCareerState
        {
            PersonId = new PersonId(personId),
            ClanId = new ClanId(personId),
            SettlementId = new SettlementId(settlementId),
            DisplayName = $"Official{personId}",
            HasAppointment = true,
            OfficeTitle = "County magistrate",
            AuthorityTier = authorityTier,
            ClerkDependence = clerk,
            PetitionBacklog = backlog,
            JurisdictionLeverage = leverage,
            DemotionPressure = demotion,
            PetitionPressure = petition,
            OfficeReputation = reputation,
        };
    }

    private static SettlementPublicLifeState MakePublicLife(int settlementId)
    {
        return new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(settlementId),
            SettlementName = $"Settlement{settlementId}",
            SettlementTier = SettlementTier.CountySeat,
            StreetTalkHeat = 30,
        };
    }
}
