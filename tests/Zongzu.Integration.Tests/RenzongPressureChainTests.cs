using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

/// <summary>
/// Renzong pressure chain thin-tests.
///
/// Principle: one chain at a time, minimal handlers, event-only cross-module
/// communication, no UI or NarrativeProjection writes authoritative state.
/// </summary>
[TestFixture]
public sealed class RenzongPressureChainTests
{
    // ── Chain 1: TaxSeasonOpened → HouseholdDebtSpiked → YamenOverloaded ──

    [Test]
    public void Chain1_TaxSeasonOpens_DebtsSpike_YamenOverloads_PublicLifeReacts()
    {
        QueryRegistry queries = new();
        DomainEventBuffer buffer = new();
        WorldDiff diff = new();
        FeatureManifest manifest = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            diff);

        // Step 1: WorldSettlements emits TaxSeasonOpened
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldModule.RegisterQueries(worldState, queries);
        worldModule.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(worldState, context));

        IDomainEvent[] taxEvents = buffer.Events.ToArray();
        Assert.That(
            taxEvents,
            Has.Some.Matches<IDomainEvent>(e => e.EventType == WorldSettlementsEventNames.TaxSeasonOpened),
            "WorldSettlements must emit TaxSeasonOpened in month 5");

        // Step 2: PopulationAndHouseholds consumes TaxSeasonOpened,
        //            increases debt pressure, emits HouseholdDebtSpiked
        PopulationAndHouseholdsModule popModule = new();
        PopulationAndHouseholdsState popState = popModule.CreateInitialState();
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "张家",
            SettlementId = new SettlementId(1),
            DebtPressure = 55,
            Distress = 50,
            LaborCapacity = 50,
            MigrationRisk = 20,
        });
        popModule.RegisterQueries(popState, queries);
        popModule.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            popState, context, taxEvents));

        int eventsAfterPop = buffer.Events.Count;
        IDomainEvent[] debtEvents = buffer.Events.Skip(taxEvents.Length).ToArray();
        Assert.That(
            debtEvents,
            Has.Some.Matches<IDomainEvent>(e => e.EventType == PopulationEventNames.HouseholdDebtSpiked),
            "PopulationAndHouseholds must emit HouseholdDebtSpiked when tax season pushes debt over threshold");
        Assert.That(
            popState.Households[0].DebtPressure,
            Is.GreaterThanOrEqualTo(70),
            "Household debt pressure must cross the 70 threshold");

        // Step 3: OfficeAndCareer consumes HouseholdDebtSpiked,
        //            increases backlog, emits YamenOverloaded
        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(1),
            DisplayName = "张县令",
            HasAppointment = true,
            OfficeTitle = "县令",
            PetitionBacklog = 52,
            AdministrativeTaskLoad = 30,
        });
        officeModule.RegisterQueries(officeState, queries);
        officeModule.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            officeState, context, buffer.Events.ToList()));

        IDomainEvent[] yamenEvents = buffer.Events.Skip(eventsAfterPop).ToArray();
        Assert.That(
            yamenEvents,
            Has.Some.Matches<IDomainEvent>(e => e.EventType == OfficeAndCareerEventNames.YamenOverloaded),
            "OfficeAndCareer must emit YamenOverloaded when debt spike increases backlog");
        Assert.That(
            officeState.People[0].PetitionBacklog,
            Is.GreaterThanOrEqualTo(60),
            "Official petition backlog must cross the 60 threshold");

        // Step 4: PublicLifeAndRumor consumes YamenOverloaded,
        //            updates street-talk heat
        PublicLifeAndRumorModule publicLifeModule = new();
        PublicLifeAndRumorState publicLifeState = publicLifeModule.CreateInitialState();
        publicLifeState.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
        });
        publicLifeModule.RegisterQueries(publicLifeState, queries);
        publicLifeModule.HandleEvents(new ModuleEventHandlingScope<PublicLifeAndRumorState>(
            publicLifeState, context, buffer.Events.ToList()));

        SettlementPublicLifeState? settlement = publicLifeState.Settlements
            .SingleOrDefault(s => s.SettlementId == new SettlementId(1));
        Assert.That(settlement, Is.Not.Null, "PublicLife must contain the settlement");
        Assert.That(
            settlement!.StreetTalkHeat,
            Is.GreaterThan(0),
            "PublicLife street-talk heat must rise after YamenOverloaded");
    }

    // ── Chain 2: SeasonPhaseAdvanced(Harvest) → GrainPriceSpike → HouseholdSubsistencePressureChanged ──

    [Test]
    public void Chain1_RealMonthlyScheduler_DrainsTaxSeasonIntoYamenAndPublicLife()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);

        IReadOnlyList<IModuleRunner> modules =
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new OfficeAndCareerModule(),
            new PublicLifeAndRumorModule(),
        ];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
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

        PopulationAndHouseholdsState popState = (PopulationAndHouseholdsState)states[KnownModuleKeys.PopulationAndHouseholds];
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "张家",
            SettlementId = new SettlementId(1),
            DebtPressure = 55,
            Distress = 48,
            LaborCapacity = 80,
            MigrationRisk = 20,
            GrainStore = 50,
        });

        OfficeAndCareerState officeState = (OfficeAndCareerState)states[KnownModuleKeys.OfficeAndCareer];
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(1),
            DisplayName = "张县令",
            HasAppointment = true,
            OfficeTitle = "县令",
            PetitionBacklog = 52,
            AdministrativeTaskLoad = 30,
        });

        PublicLifeAndRumorState publicLifeState = (PublicLifeAndRumorState)states[KnownModuleKeys.PublicLifeAndRumor];
        publicLifeState.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(1),
            SettlementName = "兰溪",
        });

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 5),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        int taxIndex = Array.FindIndex(events, static e => e.EventType == WorldSettlementsEventNames.TaxSeasonOpened);
        int debtIndex = Array.FindIndex(events, static e => e.EventType == PopulationEventNames.HouseholdDebtSpiked);
        int yamenIndex = Array.FindIndex(events, static e => e.EventType == OfficeAndCareerEventNames.YamenOverloaded);

        Assert.That(taxIndex, Is.GreaterThanOrEqualTo(0), "Real scheduler must emit TaxSeasonOpened in month 5.");
        Assert.That(debtIndex, Is.GreaterThan(taxIndex), "Real scheduler must drain tax season into household debt pressure.");
        Assert.That(yamenIndex, Is.GreaterThan(debtIndex), "Real scheduler must drain household debt into yamen overload.");

        PopulationHouseholdState household = popState.Households.Single(static entry => entry.Id == new HouseholdId(1));
        OfficeCareerState official = officeState.People.Single(static entry => entry.PersonId == new PersonId(1));
        SettlementPublicLifeState settlement = publicLifeState.Settlements.Single(static entry => entry.SettlementId == new SettlementId(1));

        Assert.That(household.DebtPressure, Is.GreaterThanOrEqualTo(70));
        Assert.That(official.PetitionBacklog, Is.GreaterThanOrEqualTo(60));
        Assert.That(settlement.StreetTalkHeat, Is.GreaterThanOrEqualTo(15));
        Assert.That(
            settlement.LastPublicTrace,
            Does.Contain("衙门口"),
            "PublicLife must react to the yamen overload emitted by the real scheduler drain.");
    }

    [Test]
    public void Chain2_HarvestPhase_GrainPriceSpike_SubsistencePressureChanged()
    {
        QueryRegistry queries = new();
        DomainEventBuffer buffer = new();
        WorldDiff diff = new();
        FeatureManifest manifest = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 9),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            diff);

        // Step 1: WorldSettlements emits SeasonPhaseAdvanced(Harvest)
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.CurrentSeason.AgrarianPhase = AgrarianPhase.Tending;
        worldModule.RegisterQueries(worldState, queries);
        worldModule.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(worldState, context));

        IDomainEvent[] seasonEvents = buffer.Events.ToArray();
        IDomainEvent? harvestEvent = seasonEvents.SingleOrDefault(
            e => e.EventType == WorldSettlementsEventNames.SeasonPhaseAdvanced
                 && e.EntityKey == nameof(AgrarianPhase.Harvest));
        Assert.That(harvestEvent, Is.Not.Null,
            "WorldSettlements must emit SeasonPhaseAdvanced with Harvest entityKey");

        // Step 2: TradeAndIndustry consumes Harvest phase,
        //            tightens grain supply, emits GrainPriceSpike
        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "兰溪市集",
        });
        tradeState.MarketGoods.Add(new MarketGoodsEntryState
        {
            SettlementId = new SettlementId(1),
            Goods = GoodsCategory.Grain,
            Supply = 60,
            Demand = 80,
            BasePrice = 100,
            CurrentPrice = 110,
        });
        tradeModule.RegisterQueries(tradeState, queries);
        tradeModule.HandleEvents(new ModuleEventHandlingScope<TradeAndIndustryState>(
            tradeState, context, seasonEvents));

        IDomainEvent[] priceEvents = buffer.Events.Skip(seasonEvents.Length).ToArray();
        Assert.That(
            priceEvents,
            Has.Some.Matches<IDomainEvent>(e => e.EventType == TradeAndIndustryEventNames.GrainPriceSpike),
            "TradeAndIndustry must emit GrainPriceSpike when harvest tightens supply above threshold");

        MarketGoodsEntryState grainEntry = tradeState.MarketGoods.Single(g => g.Goods == GoodsCategory.Grain);
        Assert.That(grainEntry.CurrentPrice, Is.GreaterThanOrEqualTo(120),
            "Grain price must cross the 120 threshold");

        // Step 3: PopulationAndHouseholds consumes GrainPriceSpike,
        //            increases distress, emits HouseholdSubsistencePressureChanged
        PopulationAndHouseholdsModule popModule = new();
        PopulationAndHouseholdsState popState = popModule.CreateInitialState();
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "张家",
            SettlementId = new SettlementId(1),
            Distress = 50,
            DebtPressure = 40,
            LaborCapacity = 50,
            MigrationRisk = 20,
        });
        popModule.RegisterQueries(popState, queries);
        popModule.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            popState, context, buffer.Events.ToList()));

        IDomainEvent[] subsistenceEvents = buffer.Events.Skip(seasonEvents.Length + priceEvents.Length).ToArray();
        Assert.That(
            subsistenceEvents,
            Has.Some.Matches<IDomainEvent>(e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged),
            "PopulationAndHouseholds must emit HouseholdSubsistencePressureChanged when grain price pushes distress over threshold");
        Assert.That(
            popState.Households[0].Distress,
            Is.GreaterThanOrEqualTo(60),
            "Household distress must cross the 60 threshold");
    }

    [Test]
    public void Chain2_RealMonthlyScheduler_DrainsHarvestPriceIntoLocalHouseholdPressure()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);

        IReadOnlyList<IModuleRunner> modules =
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new TradeAndIndustryModule(),
        ];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        worldState.CurrentSeason.AgrarianPhase = AgrarianPhase.Tending;
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "兰溪",
            Tier = SettlementTier.MarketTown,
            NodeKind = SettlementNodeKind.MarketTown,
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
            Livelihood = LivelihoodType.PettyTrader,
            Distress = 55,
            DebtPressure = 65,
            LaborCapacity = 25,
            MigrationRisk = 75,
            GrainStore = 10,
            DependentCount = 4,
        });
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "周家",
            SettlementId = new SettlementId(1),
            Distress = 90,
            DebtPressure = 40,
            LaborCapacity = 100,
            MigrationRisk = 20,
            GrainStore = 50,
        });
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(3),
            HouseholdName = "李家",
            SettlementId = new SettlementId(2),
            Livelihood = LivelihoodType.PettyTrader,
            Distress = 55,
            DebtPressure = 65,
            LaborCapacity = 25,
            MigrationRisk = 75,
            GrainStore = 10,
            DependentCount = 4,
        });

        TradeAndIndustryState tradeState = (TradeAndIndustryState)states[KnownModuleKeys.TradeAndIndustry];
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "兰溪市集",
        });

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 9),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.SeasonPhaseAdvanced
                     && e.EntityKey == nameof(AgrarianPhase.Harvest)),
            "Real scheduler must emit the harvest phase event from WorldSettlements.");
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == TradeAndIndustryEventNames.GrainPriceSpike
                     && e.EntityKey == "1"),
            "Real scheduler must drain the harvest event into a local grain price spike.");
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged
                     && e.EntityKey == "1"),
            "Real scheduler must drain the follow-on grain event into household pressure in the same month.");
        Assert.That(
            events,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged
                     && e.EntityKey == "3"),
            "A grain price spike in one settlement must not raise household pressure in another settlement.");

        PopulationHouseholdState affectedHousehold = popState.Households.Single(static household => household.Id == new HouseholdId(1));
        PopulationHouseholdState unaffectedHousehold = popState.Households.Single(static household => household.Id == new HouseholdId(3));
        Assert.That(affectedHousehold.Distress, Is.GreaterThanOrEqualTo(60));
        Assert.That(unaffectedHousehold.Distress, Is.LessThan(60));
    }
}
