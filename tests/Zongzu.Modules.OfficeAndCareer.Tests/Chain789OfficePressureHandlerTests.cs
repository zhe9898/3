using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

[TestFixture]
public sealed class Chain789OfficePressureHandlerTests
{
    // ── Chain 7: ClerkCaptureDeepened (RunMonth) ──

    [Test]
    public void PublishedEvents_DeclareClerkCaptureDeepened()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.PublishedEvents,
            Does.Contain(OfficeAndCareerEventNames.ClerkCaptureDeepened),
            "OfficeAndCareer emits ClerkCaptureDeepened, so the event contract must declare it.");
    }

    [Test]
    public void RunMonth_ClerkDependenceAndBacklogAboveThreshold_EmitsClerkCaptureDeepened()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
            ClerkDependence = 35,
            PetitionBacklog = 20,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IEducationAndExamsQueries>(new FakeEducationQueries());
        queries.Register<ISocialMemoryAndRelationsQueries>(new FakeSocialQueries());

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(state, context));

        IDomainEvent[] emitted = buffer.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.ClerkCaptureDeepened)
            .ToArray();

        Assert.That(emitted.Length, Is.EqualTo(1),
            "ClerkDependence >= 30 and PetitionBacklog >= 15 should emit ClerkCaptureDeepened.");
        Assert.That(emitted[0].EntityKey, Is.EqualTo("10"),
            "EntityKey must match the jurisdiction settlement id.");
        Assert.That(
            emitted[0].Metadata[DomainEventMetadataKeys.Cause],
            Is.EqualTo(DomainEventMetadataValues.CauseClerkCapture),
            "Clerk capture receipts must carry structured cause metadata.");
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.ClerkCapturePressure], Is.Not.Empty);
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.ClerkCaptureDependencePressure], Is.EqualTo("35"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.ClerkCaptureBacklogPressure], Is.EqualTo("10"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.ClerkCaptureAuthorityBuffer], Is.EqualTo("9"));
        Assert.That(
            state.ActiveClerkCaptureSettlementIds,
            Does.Contain(new SettlementId(10)),
            "OfficeAndCareer must persist the settlement watermark after emitting the escalation edge.");
    }

    [Test]
    public void RunMonth_ActiveClerkCapture_DoesNotRedeclareUntilConditionClears()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
            ClerkDependence = 35,
            PetitionBacklog = 20,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IEducationAndExamsQueries>(new FakeEducationQueries());
        queries.Register<ISocialMemoryAndRelationsQueries>(new FakeSocialQueries());

        ModuleExecutionContext firstContext = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());
        module.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(state, firstContext));
        Assert.That(
            firstContext.DomainEvents.Events.Select(static e => e.EventType),
            Does.Contain(OfficeAndCareerEventNames.ClerkCaptureDeepened));

        ModuleExecutionContext secondContext = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());
        module.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(state, secondContext));
        Assert.That(
            secondContext.DomainEvents.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.ClerkCaptureDeepened),
            "Persistent clerk capture must not repeat the same escalation event every month.");

        state.People[0].ClerkDependence = 20;
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);
        ModuleExecutionContext clearingContext = new(
            new GameDate(1022, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());
        module.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(state, clearingContext));
        Assert.That(
            state.ActiveClerkCaptureSettlementIds,
            Does.Not.Contain(new SettlementId(10)),
            "Dropping below the capture threshold must clear the settlement watermark.");
    }

    [Test]
    public void RunMonth_ClerkDependenceBelowThreshold_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
            ClerkDependence = 25,
            PetitionBacklog = 20,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IEducationAndExamsQueries>(new FakeEducationQueries());
        queries.Register<ISocialMemoryAndRelationsQueries>(new FakeSocialQueries());

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(state, context));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.ClerkCaptureDeepened),
            "ClerkDependence below 30 must not emit ClerkCaptureDeepened.");
    }

    [Test]
    public void RunMonth_PetitionBacklogBelowThreshold_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
            ClerkDependence = 35,
            PetitionBacklog = 10,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IEducationAndExamsQueries>(new FakeEducationQueries());
        queries.Register<ISocialMemoryAndRelationsQueries>(new FakeSocialQueries());

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(state, context));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.ClerkCaptureDeepened),
            "PetitionBacklog below 15 must not emit ClerkCaptureDeepened.");
    }

    [Test]
    public void RunMonth_NoJurisdiction_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        // No appointed officials -> no jurisdictions.
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IEducationAndExamsQueries>(new FakeEducationQueries());
        queries.Register<ISocialMemoryAndRelationsQueries>(new FakeSocialQueries());

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(state, context));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.ClerkCaptureDeepened),
            "No jurisdiction means no clerk capture event.");
    }

    // ── Chain 8: PolicyWindowOpened (HandleEvents) ──

    [Test]
    public void ConsumedEvents_DeclareCourtAgendaPressureAccumulated()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.ConsumedEvents,
            Does.Contain(WorldSettlementsEventNames.CourtAgendaPressureAccumulated),
            "OfficeAndCareer consumes CourtAgendaPressureAccumulated, so the event contract must declare it.");
    }

    [Test]
    public void CourtAgendaPressureAccumulated_LowMandateConfidence_EmitsPolicyWindowOpened()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 30));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CourtAgendaPressureAccumulated,
            "court agenda pressure",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent[] emitted = buffer.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.PolicyWindowOpened)
            .ToArray();

        Assert.That(emitted.Length, Is.EqualTo(1),
            "Low mandate confidence (< 40) should emit PolicyWindowOpened for the selected court-facing jurisdiction.");
        Assert.That(emitted[0].EntityKey, Is.EqualTo("10"),
            "EntityKey must match the jurisdiction settlement id.");
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseCourt));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.MandateConfidence], Is.EqualTo("30"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowPressure], Is.EqualTo("64"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PressureScore], Is.EqualTo("64"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowMandateDeficit], Is.EqualTo("10"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowAuthoritySignal], Is.EqualTo("54"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowLeverageSignal], Is.EqualTo("0"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowPetitionSignal], Is.EqualTo("0"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowAdministrativeDrag], Is.EqualTo("0"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowClerkDrag], Is.EqualTo("0"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowBacklogDrag], Is.EqualTo("0"));
    }

    [Test]
    public void CourtAgendaPressureAccumulated_MultipleJurisdictions_OpensOnlyBestCourtFacingWindow()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
            JurisdictionLeverage = 60,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(20),
            DisplayName = "Li Mao",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
            JurisdictionLeverage = 10,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 30));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CourtAgendaPressureAccumulated,
            "court agenda pressure",
            "court",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.MandateConfidence] = "30",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent[] emitted = buffer.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.PolicyWindowOpened)
            .ToArray();
        Assert.That(emitted.Length, Is.EqualTo(1),
            "A global court pressure must be allocated to one explicit court-facing jurisdiction in the thin slice.");
        Assert.That(emitted[0].EntityKey, Is.EqualTo("10"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyWindowPressure], Is.EqualTo("84"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.JurisdictionLeverage], Is.EqualTo("60"));
    }

    [Test]
    public void CourtAgendaPressureAccumulated_LocalDragBelowThreshold_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "鍘夸护",
            AuthorityTier = 3,
            ClerkDependence = 80,
            PetitionBacklog = 90,
            AdministrativeTaskLoad = 80,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 30));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CourtAgendaPressureAccumulated,
            "court agenda pressure",
            "court",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.MandateConfidence] = "30",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.PolicyWindowOpened),
            "Court pressure should not open a local policy window when clerk, backlog, and task drag absorb the mandate signal.");
    }

    [Test]
    public void CourtAgendaPressureAccumulated_HighMandateConfidence_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 50));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CourtAgendaPressureAccumulated,
            "court agenda pressure",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.PolicyWindowOpened),
            "High mandate confidence (>= 40) must not open policy window.");
    }

    [Test]
    public void CourtAgendaPressureAccumulated_NoJurisdiction_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 30));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CourtAgendaPressureAccumulated,
            "court agenda pressure",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.PolicyWindowOpened),
            "No jurisdiction means no policy window.");
    }

    // ── Chain 9: OfficeDefected (HandleEvents) ──

    [Test]
    public void ConsumedEvents_DeclareRegimeLegitimacyShifted()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.ConsumedEvents,
            Does.Contain(WorldSettlementsEventNames.RegimeLegitimacyShifted),
            "OfficeAndCareer consumes RegimeLegitimacyShifted, so the event contract must declare it.");
    }

    [Test]
    public void RegimeLegitimacyShifted_LowMandateConfidence_EmitsOfficeDefected()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 2,
            DemotionPressure = 90,
            ClerkDependence = 60,
            PetitionPressure = 70,
            OfficeReputation = 0,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(2),
            SettlementId = new SettlementId(10),
            DisplayName = "Li Mao",
            HasAppointment = false,
            OfficeTitle = "未授官",
            AuthorityTier = 0,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 20));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.RegimeLegitimacyShifted,
            "regime legitimacy shifted",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent[] emitted = buffer.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.OfficeDefected)
            .ToArray();

        Assert.That(emitted.Length, Is.EqualTo(1),
            "Low mandate confidence (< 25) should emit OfficeDefected for an official whose defection risk crosses threshold.");
        Assert.That(emitted[0].EntityKey, Is.EqualTo("1"),
            "EntityKey must match the appointed official person id.");
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseRegime));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionRisk], Is.EqualTo("100"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionBaselinePressure], Is.EqualTo("35"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionMandateDeficit], Is.EqualTo("5"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionDemotionPressure], Is.EqualTo("45"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionClerkPressure], Is.EqualTo("20"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionPetitionPressure], Is.EqualTo("17"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionReputationStrain], Is.EqualTo("20"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.DefectionAuthorityBuffer], Is.EqualTo("8"));
        Assert.That(state.People.Single(static p => p.PersonId == new PersonId(1)).HasAppointment, Is.False,
            "OfficeDefected is a receipt for an actual Office-owned state mutation.");
    }

    [Test]
    public void RegimeLegitimacyShifted_MultipleOfficials_DefectsOnlyHighestRiskOfficial()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 2,
            DemotionPressure = 90,
            ClerkDependence = 60,
            PetitionPressure = 70,
            OfficeReputation = 0,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(2),
            SettlementId = new SettlementId(20),
            DisplayName = "Li Mao",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 4,
            DemotionPressure = 10,
            ClerkDependence = 5,
            PetitionPressure = 10,
            OfficeReputation = 70,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 20));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.RegimeLegitimacyShifted,
            "regime legitimacy shifted",
            "regime",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.MandateConfidence] = "20",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent[] emitted = buffer.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.OfficeDefected)
            .ToArray();
        Assert.That(emitted.Length, Is.EqualTo(1),
            "A regime pressure pulse must not defect every appointed official.");
        Assert.That(emitted[0].EntityKey, Is.EqualTo("1"));
        Assert.That(state.People.Single(static p => p.PersonId == new PersonId(1)).HasAppointment, Is.False);
        Assert.That(state.People.Single(static p => p.PersonId == new PersonId(2)).HasAppointment, Is.True);
    }

    [Test]
    public void RegimeLegitimacyShifted_BufferedOfficialBelowThreshold_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "鍘夸护",
            AuthorityTier = 4,
            DemotionPressure = 10,
            ClerkDependence = 5,
            PetitionPressure = 10,
            OfficeReputation = 80,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 20));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.RegimeLegitimacyShifted,
            "regime legitimacy shifted",
            "regime",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.MandateConfidence] = "20",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.OfficeDefected),
            "Low mandate confidence alone should not defect a well-buffered official below the risk threshold.");
        Assert.That(state.People.Single(static p => p.PersonId == new PersonId(1)).OfficialDefectionRisk, Is.EqualTo(28));
    }

    [Test]
    public void RegimeLegitimacyShifted_HighMandateConfidence_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县令",
            AuthorityTier = 3,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 30));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.RegimeLegitimacyShifted,
            "regime legitimacy shifted",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.OfficeDefected),
            "High mandate confidence (>= 25) must not trigger office defection.");
    }

    [Test]
    public void RegimeLegitimacyShifted_NoAppointedOfficial_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = false,
            OfficeTitle = "未授官",
            AuthorityTier = 0,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(mandateConfidence: 20));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.RegimeLegitimacyShifted,
            "regime legitimacy shifted",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.OfficeDefected),
            "No appointed official means no office defection.");
    }

    // ── Fake query helpers ──

    private sealed class FakeEducationQueries : IEducationAndExamsQueries
    {
        public EducationCandidateSnapshot GetRequiredCandidate(PersonId personId) => throw new NotImplementedException();
        public AcademySnapshot GetRequiredAcademy(InstitutionId institutionId) => throw new NotImplementedException();
        public IReadOnlyList<EducationCandidateSnapshot> GetCandidates() => [];
        public IReadOnlyList<AcademySnapshot> GetAcademies() => [];
    }

    private sealed class FakeSocialQueries : ISocialMemoryAndRelationsQueries
    {
        public ClanNarrativeSnapshot GetRequiredClanNarrative(ClanId clanId) => throw new NotImplementedException();
        public IReadOnlyList<ClanNarrativeSnapshot> GetClanNarratives() => [];
        public IReadOnlyList<SocialMemoryEntrySnapshot> GetMemories() => [];
        public IReadOnlyList<SocialMemoryEntrySnapshot> GetMemoriesByClan(ClanId clanId) => [];
    }

    private sealed class FakeWorldQueries : IWorldSettlementsQueries
    {
        private readonly int _mandateConfidence;

        public FakeWorldQueries(int mandateConfidence)
        {
            _mandateConfidence = mandateConfidence;
        }

        public SeasonBandSnapshot GetCurrentSeason() => new()
        {
            Imperial = new ImperialBandSnapshot
            {
                MandateConfidence = _mandateConfidence,
            },
        };

        public SettlementSnapshot GetRequiredSettlement(SettlementId settlementId) => throw new NotImplementedException();
        public IReadOnlyList<SettlementSnapshot> GetSettlements() => throw new NotImplementedException();
        public IReadOnlyList<SettlementSnapshot> GetSettlementsByNodeKind(SettlementNodeKind kind) => throw new NotImplementedException();
        public IReadOnlyList<SettlementSnapshot> GetSettlementsByVisibility(NodeVisibility visibility) => throw new NotImplementedException();
        public IReadOnlyList<RouteSnapshot> GetRoutes() => throw new NotImplementedException();
        public IReadOnlyList<RouteSnapshot> GetRoutesByKind(RouteKind kind) => throw new NotImplementedException();
        public IReadOnlyList<RouteSnapshot> GetRoutesByLegitimacy(RouteLegitimacy legitimacy) => throw new NotImplementedException();
        public IReadOnlyList<RouteSnapshot> GetRoutesTouching(SettlementId settlementId) => throw new NotImplementedException();
        public LocusSnapshot? GetCurrentLocus() => throw new NotImplementedException();
        public IReadOnlyList<PublicSurfaceSignal> GetCurrentPulseSignals() => throw new NotImplementedException();
    }
}
