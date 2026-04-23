using System.Linq;
using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

[TestFixture]
public sealed class DisasterDisorderHandlerTests
{
    [Test]
    public void DisasterDeclared_FloodSevere_RaisesDisorder()
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
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.DisasterDeclared,
            "兰溪水患告急：汛险75，堤压40，severity flood-severe。",
            "1",
            FloodMetadata(DomainEventMetadataValues.SeverityFloodSevere)));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(45),
            "Severe flood disaster should raise disorder by 15.");
        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OrderAndBanditryEventNames.DisorderSpike),
            "Disorder below 50 should not trigger DisorderSpike.");
    }

    [Test]
    public void DisasterDeclared_FloodModerate_CrossesThreshold_EmitsDisorderSpike()
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
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.DisasterDeclared,
            "兰溪水患告急：汛险55，堤压30，severity flood-moderate。",
            "1",
            FloodMetadata(DomainEventMetadataValues.SeverityFloodModerate)));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        // Settlement 1: 42 + 8 = 50, crosses 50 → emits DisorderSpike
        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(50));
        Assert.That(
            buffer.Events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "1"),
            "Settlement 1 should emit DisorderSpike when crossing threshold.");

        // Settlement 2: 60, off-scope → no change, no spike
        Assert.That(state.Settlements[1].DisorderPressure, Is.EqualTo(60));
        Assert.That(
            buffer.Events,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "2"),
            "Off-scope settlement should not emit DisorderSpike.");
    }

    [Test]
    public void DisasterDeclared_OffScopeSettlement_DoesNotAffect()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 40,
        });
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            DisorderPressure = 40,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

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
            WorldSettlementsEventNames.DisasterDeclared,
            "severity flood-severe",
            "1",
            FloodMetadata(DomainEventMetadataValues.SeverityFloodSevere)));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(55),
            "Affected settlement should receive +15 from severe flood.");
        Assert.That(state.Settlements[1].DisorderPressure, Is.EqualTo(40),
            "Off-scope settlement must remain untouched.");
    }

    [Test]
    public void DisasterDeclared_TextSeverityWithoutMetadata_IsNoOp()
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
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.DisasterDeclared,
            "summary says flood-severe but carries no structured metadata",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].DisorderPressure, Is.EqualTo(40),
            "Disaster pressure must come from metadata, not parsed Summary text.");
    }

    private static Dictionary<string, string> FloodMetadata(string severity)
    {
        return new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseDisaster,
            [DomainEventMetadataKeys.DisasterKind] = DomainEventMetadataValues.DisasterFlood,
            [DomainEventMetadataKeys.Severity] = severity,
            [DomainEventMetadataKeys.FloodRisk] = "75",
            [DomainEventMetadataKeys.EmbankmentStrain] = "40",
        };
    }
}
