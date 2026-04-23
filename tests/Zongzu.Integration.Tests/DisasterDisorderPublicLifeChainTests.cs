using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

/// <summary>
/// DisasterDeclared → DisorderSpike → PublicLife thin chain tests.
/// </summary>
[TestFixture]
public sealed class DisasterDisorderPublicLifeChainTests
{
    [Test]
    public void ThinDisasterChain_FloodSevere_DisorderSpike_PublicLifeReacts()
    {
        QueryRegistry queries = new();
        DomainEventBuffer buffer = new();
        WorldDiff diff = new();
        FeatureManifest manifest = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            diff);

        // Step 1: WorldSettlements emits DisasterDeclared (flood season, high FloodRisk)
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.CurrentSeason.FloodRisk = 75;
        worldState.CurrentSeason.EmbankmentStrain = 40;
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "兰溪",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 60,
            Prosperity = 60,
        });
        worldModule.RegisterQueries(worldState, queries);
        worldModule.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(worldState, context));

        IDomainEvent[] disasterEvents = buffer.Events.ToArray();
        Assert.That(
            disasterEvents,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.DisasterDeclared
                     && e.EntityKey == "1"),
            "WorldSettlements must emit DisasterDeclared for settlement 1 when FloodRisk >= 50.");

        // Step 2: OrderAndBanditry consumes DisasterDeclared,
        //            raises disorder, emits DisorderSpike if crossing 50
        OrderAndBanditryModule orderModule = new();
        OrderAndBanditryState orderState = orderModule.CreateInitialState();
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 42, // 42 + 15 = 57, crosses threshold
        });
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            DisorderPressure = 30, // 30, off-scope
        });
        orderModule.RegisterQueries(orderState, queries);
        orderModule.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(
            orderState, context, disasterEvents));

        IDomainEvent[] disorderEvents = buffer.Events.Skip(disasterEvents.Length).ToArray();
        Assert.That(
            disorderEvents,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "1"),
            "OrderAndBanditry must emit DisorderSpike when settlement 1 crosses threshold.");
        Assert.That(
            disorderEvents,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "2"),
            "Settlement 2 must not emit DisorderSpike (off-scope negative assertion).");

        // Step 3: PublicLifeAndRumor consumes DisorderSpike,
        //            raises street-talk heat for the matched settlement only
        PublicLifeAndRumorModule publicLifeModule = new();
        PublicLifeAndRumorState publicLifeState = publicLifeModule.CreateInitialState();
        publicLifeState.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
            StreetTalkHeat = 30,
        });
        publicLifeState.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(2),
            SettlementName = "桐山",
            StreetTalkHeat = 20,
        });
        publicLifeModule.RegisterQueries(publicLifeState, queries);
        publicLifeModule.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            publicLifeState, context, buffer.Events.ToList()));

        SettlementPublicLifeState? lanxi = publicLifeState.Settlements
            .SingleOrDefault(s => s.SettlementId == new SettlementId(1));
        SettlementPublicLifeState? tongshan = publicLifeState.Settlements
            .SingleOrDefault(s => s.SettlementId == new SettlementId(2));

        Assert.That(lanxi, Is.Not.Null);
        Assert.That(tongshan, Is.Not.Null);
        Assert.That(lanxi!.StreetTalkHeat, Is.EqualTo(42),
            "PublicLife must raise street-talk heat in settlement 1 after DisorderSpike.");
        Assert.That(tongshan!.StreetTalkHeat, Is.EqualTo(20),
            "PublicLife must not touch settlement 2 (off-scope negative assertion).");
        Assert.That(
            lanxi.LastPublicTrace,
            Does.Contain("水患告急"),
            "PublicLife trace must reflect disaster cause, not default to corvee text.");
    }

    [Test]
    public void ThinDisasterChain_RealScheduler_FloodRiskDrainsIntoDisorderSpike()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);

        IReadOnlyList<IModuleRunner> modules =
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new OrderAndBanditryModule(),
            new PublicLifeAndRumorModule(),
        ];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        worldState.CurrentSeason.FloodRisk = 75;
        worldState.CurrentSeason.EmbankmentStrain = 40;
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "兰溪",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 60,
            Prosperity = 60,
        });
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(2),
            Name = "桐山",
            Tier = SettlementTier.VillageCluster,
            NodeKind = SettlementNodeKind.Village,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 60,
            Prosperity = 60,
        });

        PopulationAndHouseholdsState popState = (PopulationAndHouseholdsState)states[KnownModuleKeys.PopulationAndHouseholds];
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "张家",
            SettlementId = new SettlementId(1),
            Distress = 48,
            DebtPressure = 40,
            LaborCapacity = 80,
            MigrationRisk = 20,
        });
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "周家",
            SettlementId = new SettlementId(2),
            Distress = 40,
            DebtPressure = 30,
            LaborCapacity = 70,
            MigrationRisk = 20,
        });

        // Seed population settlements so OrderAndBanditry.RunXun can query them
        popState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 40,
            MigrationPressure = 20,
            MilitiaPotential = 30,
        });
        popState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(2),
            CommonerDistress = 35,
            MigrationPressure = 15,
            MilitiaPotential = 25,
        });

        OrderAndBanditryState orderState = (OrderAndBanditryState)states[KnownModuleKeys.OrderAndBanditry];
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(1),
            DisorderPressure = 46, // 46 + 15 = 61, crosses 50 after real scheduler xun drift
        });
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            DisorderPressure = 30,
        });

        PublicLifeAndRumorState publicLifeState = (PublicLifeAndRumorState)states[KnownModuleKeys.PublicLifeAndRumor];
        publicLifeState.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
            StreetTalkHeat = 30,
        });
        publicLifeState.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(2),
            SettlementName = "桐山",
            StreetTalkHeat = 20,
        });

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 6),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.DisasterDeclared
                     && e.EntityKey == "1"),
            "Real scheduler must emit DisasterDeclared for settlement 1 when FloodRisk >= 50.");
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "1"),
            "Real scheduler must drain DisasterDeclared into DisorderSpike for settlement 1.");
        Assert.That(
            events,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == "2"),
            "Settlement 2 must not emit DisorderSpike (off-scope negative assertion).");

        SettlementDisorderState lanxiDisorder = orderState.Settlements
            .Single(static s => s.SettlementId == new SettlementId(1));
        SettlementDisorderState tongshanDisorder = orderState.Settlements
            .Single(static s => s.SettlementId == new SettlementId(2));
        Assert.That(lanxiDisorder.DisorderPressure, Is.GreaterThanOrEqualTo(50),
            "Settlement 1 disorder pressure must cross 50 after disaster delta + xun drift.");
        Assert.That(tongshanDisorder.DisorderPressure, Is.LessThan(50),
            "Settlement 2 disorder pressure must stay below 50 (off-scope).");

        SettlementPublicLifeState lanxiLife = publicLifeState.Settlements
            .Single(static s => s.SettlementId == new SettlementId(1));
        SettlementPublicLifeState tongshanLife = publicLifeState.Settlements
            .Single(static s => s.SettlementId == new SettlementId(2));
        Assert.That(lanxiLife.StreetTalkHeat, Is.GreaterThan(30),
            "PublicLife must raise street-talk heat in settlement 1 after real scheduler drain.");
        Assert.That(
            lanxiLife.LastPublicTrace,
            Does.Contain("水患告急"),
            "PublicLife trace must reflect disaster cause.");
        Assert.That(
            tongshanLife.LastPublicTrace,
            Does.Not.Contain("水患告急"),
            "PublicLife must not touch settlement 2 after real scheduler drain (off-scope negative assertion).");
    }
}
