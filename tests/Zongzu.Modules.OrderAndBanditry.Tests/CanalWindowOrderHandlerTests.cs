using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

[TestFixture]
public sealed class CanalWindowOrderHandlerTests
{
    [Test]
    public void CanalWindowChanged_Closed_ReturnsRoutePressureToOrderOwnedLane()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        LanxiSeed.LanxiHandles handles = LanxiSeed.Seed(worldState, KernelState.Create(9201));

        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        SettlementId offScope = new(999);

        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = handles.Market,
            RoutePressure = 55,
            BlackRoutePressure = 58,
            DisorderPressure = 40,
            SuppressionDemand = 20,
            RouteShielding = 6,
        });
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = offScope,
            RoutePressure = 20,
            BlackRoutePressure = 20,
            DisorderPressure = 20,
            SuppressionDemand = 10,
            RouteShielding = 8,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        module.RegisterQueries(state, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 12),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(9202)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        DomainEventRecord canalEvent = new(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.CanalWindowChanged,
            "receipt prose says Open; handler must read structured canal window fields.",
            nameof(CanalWindow.Open),
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.CanalWindowBefore] = nameof(CanalWindow.Limited),
                [DomainEventMetadataKeys.CanalWindowAfter] = nameof(CanalWindow.Closed),
            });

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(state, context, [canalEvent]));

        SettlementDisorderState exposed = state.Settlements.Single(settlement => settlement.SettlementId == handles.Market);
        SettlementDisorderState untouched = state.Settlements.Single(settlement => settlement.SettlementId == offScope);

        Assert.That(exposed.RoutePressure, Is.EqualTo(63));
        Assert.That(exposed.BlackRoutePressure, Is.EqualTo(63));
        Assert.That(exposed.DisorderPressure, Is.EqualTo(42));
        Assert.That(exposed.SuppressionDemand, Is.EqualTo(22));
        Assert.That(exposed.RouteShielding, Is.EqualTo(4));
        Assert.That(exposed.LastPressureTrace, Does.Contain(nameof(CanalWindow.Closed)));

        Assert.That(untouched.RoutePressure, Is.EqualTo(20));
        Assert.That(untouched.BlackRoutePressure, Is.EqualTo(20));
        Assert.That(untouched.DisorderPressure, Is.EqualTo(20));
        Assert.That(untouched.SuppressionDemand, Is.EqualTo(10));

        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.OrderAndBanditry));
        Assert.That(
            context.DomainEvents.Events,
            Has.Some.Matches<IDomainEvent>(domainEvent =>
                domainEvent.EventType == OrderAndBanditryEventNames.BlackRoutePressureRaised
                && domainEvent.Metadata[DomainEventMetadataKeys.Cause] == DomainEventMetadataValues.CauseCanalWindow
                && domainEvent.Metadata[DomainEventMetadataKeys.CanalWindow] == nameof(CanalWindow.Closed)));
    }
}
