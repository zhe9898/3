using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.TradeAndIndustry.Tests;

[TestFixture]
public sealed class CanalWindowTradeHandlerTests
{
    [Test]
    public void CanalWindowChanged_Closed_AdjustsTradeOwnedWaterRouteStateOnly()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        LanxiSeed.LanxiHandles handles = LanxiSeed.Seed(worldState, KernelState.Create(9101));

        TradeAndIndustryModule module = new();
        TradeAndIndustryState state = module.CreateInitialState();
        SettlementId offScope = new(999);

        state.Markets.Add(new SettlementMarketState
        {
            SettlementId = handles.Market,
            MarketName = "Lanxi Market",
            PriceIndex = 100,
            Demand = 70,
            LocalRisk = 48,
        });
        state.Markets.Add(new SettlementMarketState
        {
            SettlementId = offScope,
            MarketName = "Hill Market",
            PriceIndex = 100,
            Demand = 70,
            LocalRisk = 20,
        });
        state.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(1),
            RouteName = "Lanxi Water Wharf",
            SettlementId = handles.Market,
            IsActive = true,
            Capacity = 28,
            Risk = 58,
            LastMargin = 5,
            SeizureRisk = 10,
        });
        state.Routes.Add(new RouteTradeState
        {
            RouteId = 2,
            ClanId = new ClanId(2),
            RouteName = "Hill Track",
            SettlementId = offScope,
            IsActive = true,
            Capacity = 16,
            Risk = 20,
            LastMargin = 5,
            SeizureRisk = 4,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        module.RegisterQueries(state, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 12),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(9102)),
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

        module.HandleEvents(new ModuleEventHandlingScope<TradeAndIndustryState>(state, context, [canalEvent]));

        SettlementMarketState exposedMarket = state.Markets.Single(market => market.SettlementId == handles.Market);
        SettlementMarketState offScopeMarket = state.Markets.Single(market => market.SettlementId == offScope);
        RouteTradeState exposedRoute = state.Routes.Single(route => route.SettlementId == handles.Market);
        RouteTradeState offScopeRoute = state.Routes.Single(route => route.SettlementId == offScope);

        Assert.That(exposedMarket.LocalRisk, Is.EqualTo(54));
        Assert.That(exposedMarket.Demand, Is.EqualTo(67));
        Assert.That(exposedRoute.Risk, Is.EqualTo(66));
        Assert.That(exposedRoute.BlockedShipmentCount, Is.EqualTo(1));
        Assert.That(exposedRoute.SeizureRisk, Is.EqualTo(14));
        Assert.That(exposedRoute.RouteConstraintLabel, Is.Not.Empty);
        Assert.That(exposedRoute.LastRouteTrace, Does.Contain(nameof(CanalWindow.Closed)));

        Assert.That(offScopeMarket.LocalRisk, Is.EqualTo(20));
        Assert.That(offScopeRoute.Risk, Is.EqualTo(20));
        Assert.That(offScopeRoute.BlockedShipmentCount, Is.EqualTo(0));

        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.TradeAndIndustry));
        Assert.That(
            context.DomainEvents.Events,
            Has.Some.Matches<IDomainEvent>(domainEvent =>
                domainEvent.EventType == TradeAndIndustryEventNames.RouteBusinessBlocked
                && domainEvent.Metadata[DomainEventMetadataKeys.Cause] == DomainEventMetadataValues.CauseCanalWindow
                && domainEvent.Metadata[DomainEventMetadataKeys.CanalWindow] == nameof(CanalWindow.Closed)));
    }
}
