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
        Assert.That(governance.GovernanceSummary, Does.Contain("县门执行读回"));
        Assert.That(afterFirst.GovernanceDocket.OfficeImplementationReadbackSummary, Does.Contain("县门执行读回"));
        Assert.That(afterFirst.GovernanceDocket.GuidanceSummary, Does.Contain("县门/文移后手"));

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
