using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

[TestFixture]
public sealed class CorveeDisorderHandlerTests
{
    [Test]
    public void PublishedEvents_DeclareDisorderSpike()
    {
        OrderAndBanditryModule module = new();

        Assert.That(
            module.PublishedEvents,
            Does.Contain(OrderAndBanditryEventNames.DisorderSpike),
            "OrderAndBanditry emits DisorderSpike, so the event contract must declare it.");
    }

    [Test]
    public void CorveeWindowChanged_Pressed_RaisesDisorder()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
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

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CorveeWindowChanged,
            "徭役窗口转Pressed。",
            nameof(CorveeWindow.Pressed)));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(38),
            "Pressed corvee window should raise disorder by 8.");
        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OrderAndBanditryEventNames.DisorderSpike),
            "Disorder below 50 should not trigger DisorderSpike.");
    }

    [Test]
    public void CorveeWindowChanged_Emergency_CrossesThreshold_EmitsDisorderSpike()
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

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CorveeWindowChanged,
            "徭役窗口转Emergency。",
            nameof(CorveeWindow.Emergency)));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        // Settlement 1: 42 + 15 = 57, crosses 50 → emits DisorderSpike
        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(57));
        Assert.That(
            buffer.Events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "1"),
            "Settlement 1 should emit DisorderSpike when crossing threshold.");

        // Settlement 2: 60 + 15 = 75, already above 50 → no new spike
        Assert.That(state.Settlements[1].DisorderPressure, Is.EqualTo(75));
        Assert.That(
            buffer.Events,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "2"),
            "Settlement 2 should not emit DisorderSpike when already above threshold.");
    }

    [Test]
    public void CorveeWindowChanged_Quiet_DoesNothing()
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
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CorveeWindowChanged,
            "徭役窗口转Quiet。",
            nameof(CorveeWindow.Quiet)));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(40),
            "Quiet corvee window should not raise disorder.");
        Assert.That(buffer.Events.Count, Is.EqualTo(1),
            "No follow-on events should be emitted.");
    }
}
