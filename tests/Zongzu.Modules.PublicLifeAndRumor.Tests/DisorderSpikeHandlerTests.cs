using System.Linq;
using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor.Tests;

[TestFixture]
public sealed class DisorderSpikeHandlerTests
{
    [Test]
    public void DisorderSpike_ForKnownSettlement_RaisesStreetTalkHeat()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = new();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
            StreetTalkHeat = 30,
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
            KnownModuleKeys.OrderAndBanditry,
            OrderAndBanditryEventNames.DisorderSpike,
            "聚落1因徭役加急，失序骤起。",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state, context, buffer.Events.ToList()));

        SettlementPublicLifeState settlement = state.Settlements.Single();
        Assert.That(settlement.StreetTalkHeat, Is.EqualTo(42),
            "DisorderSpike must raise StreetTalkHeat by 12.");
        Assert.That(
            settlement.LastPublicTrace,
            Does.Contain("街谈热度升至42"),
            "Public trace must reflect the heat rise.");
    }

    [Test]
    public void DisorderSpike_ForUnknownSettlement_IsNoOp()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = new();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
            StreetTalkHeat = 30,
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
            KnownModuleKeys.OrderAndBanditry,
            OrderAndBanditryEventNames.DisorderSpike,
            "聚落99因徭役加急，失序骤起。",
            "99"));

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].StreetTalkHeat, Is.EqualTo(30),
            "DisorderSpike for unknown settlement must not mutate any settlement.");
    }

    [Test]
    public void DisorderSpike_ForMatchedSettlement_DoesNotAffectOtherSettlements()
    {
        // Off-scope negative assertion: ensures scoping by settlementId works.
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = new();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
            StreetTalkHeat = 30,
        });
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(2),
            SettlementName = "桐山",
            StreetTalkHeat = 20,
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
            KnownModuleKeys.OrderAndBanditry,
            OrderAndBanditryEventNames.DisorderSpike,
            "聚落1因徭役加急，失序骤起。",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            state.Settlements.Single(s => s.SettlementId == new SettlementId(1)).StreetTalkHeat,
            Is.EqualTo(42),
            "Matched settlement must receive heat rise.");
        Assert.That(
            state.Settlements.Single(s => s.SettlementId == new SettlementId(2)).StreetTalkHeat,
            Is.EqualTo(20),
            "Unmatched settlement must remain untouched (off-scope negative assertion).");
    }

    [Test]
    public void DisorderSpike_CauseHintComesFromMetadata_NotSummary()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = new();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
            StreetTalkHeat = 30,
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
            KnownModuleKeys.OrderAndBanditry,
            OrderAndBanditryEventNames.DisorderSpike,
            "summary mentions 徭役 but metadata says flood disaster",
            "1",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseDisaster,
                [DomainEventMetadataKeys.DisasterKind] = DomainEventMetadataValues.DisasterFlood,
            }));

        module.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Settlements[0].LastPublicTrace, Does.Contain("水患告急"));
        Assert.That(state.Settlements[0].LastPublicTrace, Does.Not.Contain("徭役加急"));
    }
}
