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

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.AmnestyApplied,
            "县尉奉赦，1地界在押人犯减等释放。",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(40),
            "Target settlement disorder must rise by 10 after amnesty.");
        Assert.That(state.Settlements[1].DisorderPressure, Is.EqualTo(30),
            "Off-scope settlement must remain unchanged (negative assertion).");
        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OrderAndBanditryEventNames.DisorderSpike),
            "Disorder below 50 should not trigger DisorderSpike.");
    }

    [Test]
    public void AmnestyApplied_CrossesThreshold_EmitsDisorderSpike()
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
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.AmnestyApplied,
            "县尉奉赦，1地界在押人犯减等释放。",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        // Settlement 1: 42 + 10 = 52, crosses 50 → emits DisorderSpike
        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(52));
        Assert.That(
            buffer.Events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "1"),
            "Settlement 1 should emit DisorderSpike when crossing threshold.");

        // Settlement 2: unchanged, already above 50 → no new spike for it
        Assert.That(state.Settlements[1].DisorderPressure, Is.EqualTo(60));
        Assert.That(
            buffer.Events,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "2"),
            "Settlement 2 should not emit DisorderSpike (off-scope and already above threshold).");
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

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.AmnestyApplied,
            "县尉奉赦。",
            "not-a-number"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(40),
            "Invalid entity key must not mutate any settlement.");
        Assert.That(buffer.Events.Count, Is.EqualTo(1),
            "No follow-on events should be emitted for invalid entity key.");
    }
}
