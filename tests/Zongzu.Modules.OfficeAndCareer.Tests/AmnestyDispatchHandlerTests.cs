using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

[TestFixture]
public sealed class AmnestyDispatchHandlerTests
{
    [Test]
    public void PublishedEvents_DeclareAmnestyApplied()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.PublishedEvents,
            Does.Contain(OfficeAndCareerEventNames.AmnestyApplied),
            "OfficeAndCareer emits AmnestyApplied, so the event contract must declare it.");
    }

    [Test]
    public void ConsumedEvents_DeclareImperialRhythmChanged()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.ConsumedEvents,
            Does.Contain(WorldSettlementsEventNames.ImperialRhythmChanged),
            "OfficeAndCareer consumes ImperialRhythmChanged, so the event contract must declare it.");
    }

    [Test]
    public void ImperialRhythmChanged_HighAmnestyWave_EmitsAmnestyApplied_PerJurisdiction()
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
            OfficeTitle = "县尉",
            AuthorityTier = 2,
            JurisdictionLeverage = 45,
            ClerkDependence = 55,
            PetitionBacklog = 40,
            AdministrativeTaskLoad = 30,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(2),
            SettlementId = new SettlementId(20),
            DisplayName = "Li Mao",
            HasAppointment = true,
            OfficeTitle = "主簿",
            AuthorityTier = 1,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(amnestyWave: 60));

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
            WorldSettlementsEventNames.ImperialRhythmChanged,
            "皇权节律有变：赦 60。",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent[] emitted = buffer.Events
            .Where(e => e.EventType == OfficeAndCareerEventNames.AmnestyApplied)
            .ToArray();

        Assert.That(emitted.Length, Is.EqualTo(2),
            "High amnesty wave should emit one AmnestyApplied per jurisdiction.");
        Assert.That(emitted, Has.Some.Matches<IDomainEvent>(e => e.EntityKey == "10"),
            "Jurisdiction at settlement 10 must receive amnesty event.");
        Assert.That(emitted, Has.Some.Matches<IDomainEvent>(e => e.EntityKey == "20"),
            "Jurisdiction at settlement 20 must receive amnesty event.");
        IDomainEvent settlement10Event = emitted.Single(static e => e.EntityKey == "10");
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseAmnesty));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.SourceEventType], Is.EqualTo(WorldSettlementsEventNames.ImperialRhythmChanged));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("10"));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.AmnestyWave], Is.EqualTo("60"));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.AuthorityTier], Is.EqualTo("2"));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.JurisdictionLeverage], Is.EqualTo("45"));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.ClerkDependence], Is.EqualTo("55"));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.PetitionBacklog], Is.EqualTo("40"));
        Assert.That(settlement10Event.Metadata[DomainEventMetadataKeys.AdministrativeTaskLoad], Is.EqualTo("30"));
        Assert.That(state.LastAppliedAmnestyWave, Is.EqualTo(60),
            "Handled amnesty wave should be latched so unrelated imperial rhythm changes do not repeat it.");
    }

    [Test]
    public void ImperialRhythmChanged_LowAmnestyWave_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.LastAppliedAmnestyWave = 70;
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "县尉",
            AuthorityTier = 2,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(amnestyWave: 30));

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
            WorldSettlementsEventNames.ImperialRhythmChanged,
            "皇权节律有变：赦 30。",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.AmnestyApplied),
            "Low amnesty wave should not trigger AmnestyApplied.");
        Assert.That(state.LastAppliedAmnestyWave, Is.EqualTo(0),
            "Dropping below the amnesty threshold should reset the de-duplication latch.");
    }

    [Test]
    public void ImperialRhythmChanged_AlreadyAppliedAmnestyWave_DoesNotEmitAgain()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new()
        {
            LastAppliedAmnestyWave = 70,
        };
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "CountyMagistrate",
            AuthorityTier = 2,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(amnestyWave: 64));

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.ImperialRhythmChanged,
            "Imperial rhythm changed for a non-amnesty axis.",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.AmnestyApplied),
            "A non-amnesty imperial rhythm change must not repeat an already applied amnesty wave.");
        Assert.That(state.LastAppliedAmnestyWave, Is.EqualTo(70));
    }

    [Test]
    public void ImperialRhythmChanged_NoJurisdiction_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        // No appointed officials → no jurisdictions.
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWorldSettlementsQueries>(new FakeWorldQueries(amnestyWave: 80));

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
            WorldSettlementsEventNames.ImperialRhythmChanged,
            "皇权节律有变：赦 80。",
            "imperial"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.AmnestyApplied),
            "No jurisdiction means no amnesty command reaches any settlement.");
        Assert.That(state.LastAppliedAmnestyWave, Is.EqualTo(0),
            "No jurisdiction means no settlement actually applied the amnesty, so the latch stays open.");
    }

    private sealed class FakeWorldQueries : IWorldSettlementsQueries
    {
        private readonly int _amnestyWave;

        public FakeWorldQueries(int amnestyWave)
        {
            _amnestyWave = amnestyWave;
        }

        public SeasonBandSnapshot GetCurrentSeason() => new()
        {
            Imperial = new ImperialBandSnapshot
            {
                AmnestyWave = _amnestyWave,
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
