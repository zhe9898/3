using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

[TestFixture]
public sealed class AmnestyDisorderHandlerTests
{
    [Test]
    public void ConsumedEvents_DeclareAmnestyApplied()
    {
        OrderAndBanditryModule module = new();
        Assert.That(
            module.ConsumedEvents,
            Does.Contain(OfficeAndCareerEventNames.AmnestyApplied),
            "OrderAndBanditry consumes AmnestyApplied, so the event contract must declare it.");
    }

    [Test]
    public void AmnestyApplied_RaisesDisorder_ForTargetSettlement()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 30,
        });
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            DisorderPressure = 30,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(BuildAmnestyAppliedEvent("1"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(40),
            "Target settlement disorder must rise by the computed amnesty profile.");
        Assert.That(state.Settlements[1].DisorderPressure, Is.EqualTo(30),
            "Off-scope settlement must remain unchanged (negative assertion).");
        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OrderAndBanditryEventNames.DisorderSpike),
            "Disorder below 50 should not trigger DisorderSpike.");
    }

    [Test]
    public void AmnestyApplied_CrossesThreshold_EmitsDisorderSpike_WithProfileMetadata()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 42,
        });
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            DisorderPressure = 60,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(BuildAmnestyAppliedEvent("1"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(52));
        IDomainEvent spike = buffer.Events.Single(e =>
            e.EventType == OrderAndBanditryEventNames.DisorderSpike
            && e.EntityKey == "1");
        Assert.That(spike.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseAmnesty));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.SourceEventType], Is.EqualTo(OfficeAndCareerEventNames.AmnestyApplied));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.DisorderDelta], Is.EqualTo("10"));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.AmnestyWave], Is.EqualTo("65"));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.AmnestyReleasePressure], Is.EqualTo("6"));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.AmnestyDocketPressure], Is.EqualTo("3"));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.AmnestyClerkHandlingPressure], Is.EqualTo("2"));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.AmnestyAuthorityBuffer], Is.EqualTo("2"));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.AmnestyLocalDisorderSoil], Is.EqualTo("1"));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.AmnestySuppressionBuffer], Is.EqualTo("0"));

        Assert.That(state.Settlements[1].DisorderPressure, Is.EqualTo(60));
        Assert.That(
            buffer.Events,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "2"),
            "Settlement 2 should not emit DisorderSpike (off-scope and already above threshold).");
    }

    [Test]
    public void AmnestyApplied_MissingAmnestyWaveMetadata_IsNoOp()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 40,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.AmnestyApplied,
            "Summary text says many prisoners were released, but metadata is absent.",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(40),
            "Summary text must not drive the amnesty-disorder rule.");
        Assert.That(buffer.Events.Count, Is.EqualTo(1),
            "No follow-on events should be emitted without structured amnesty metadata.");
    }

    [Test]
    public void AmnestyApplied_StrongLocalSuppression_CanAbsorbDisorder()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 20,
            SuppressionRelief = 70,
            RouteShielding = 70,
            ResponseActivationLevel = 80,
            AdministrativeSuppressionWindow = 70,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.AmnestyApplied,
            "County office applies a limited amnesty under strong local order.",
            "1",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseAmnesty,
                [DomainEventMetadataKeys.SourceEventType] = WorldSettlementsEventNames.ImperialRhythmChanged,
                [DomainEventMetadataKeys.SettlementId] = "1",
                [DomainEventMetadataKeys.AmnestyWave] = "50",
                [DomainEventMetadataKeys.AuthorityTier] = "3",
                [DomainEventMetadataKeys.JurisdictionLeverage] = "70",
                [DomainEventMetadataKeys.ClerkDependence] = "0",
                [DomainEventMetadataKeys.PetitionBacklog] = "20",
                [DomainEventMetadataKeys.AdministrativeTaskLoad] = "0",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(20),
            "Strong local suppression and low disorder soil should absorb a limited amnesty without forcing a bump.");
        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OrderAndBanditryEventNames.DisorderSpike));
    }

    [Test]
    public void AmnestyApplied_InvalidEntityKey_IsNoOp()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 40,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(BuildAmnestyAppliedEvent("not-a-number"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(40),
            "Invalid entity key must not mutate any settlement.");
        Assert.That(buffer.Events.Count, Is.EqualTo(1),
            "No follow-on events should be emitted for invalid entity key.");
    }

    private static DomainEventRecord BuildAmnestyAppliedEvent(string entityKey)
    {
        return new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.AmnestyApplied,
            "County office applies an amnesty release.",
            entityKey,
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseAmnesty,
                [DomainEventMetadataKeys.SourceEventType] = WorldSettlementsEventNames.ImperialRhythmChanged,
                [DomainEventMetadataKeys.SettlementId] = entityKey,
                [DomainEventMetadataKeys.AmnestyWave] = "65",
                [DomainEventMetadataKeys.AuthorityTier] = "2",
                [DomainEventMetadataKeys.JurisdictionLeverage] = "30",
                [DomainEventMetadataKeys.ClerkDependence] = "50",
                [DomainEventMetadataKeys.PetitionBacklog] = "60",
                [DomainEventMetadataKeys.AdministrativeTaskLoad] = "40",
            });
    }
}
