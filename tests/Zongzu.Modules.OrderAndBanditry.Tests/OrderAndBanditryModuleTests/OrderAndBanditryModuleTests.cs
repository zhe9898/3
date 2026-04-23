using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

[TestFixture]
public sealed partial class OrderAndBanditryModuleTests
{
    [Test]

    public void RunMonth_RaisesDisorderAndEmitsTraceableEvents()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(2),

            Name = "Lanxi",

            Security = 39,

            Prosperity = 58,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(2),

            CommonerDistress = 68,

            LaborSupply = 92,

            MigrationPressure = 56,

            MilitiaPotential = 44,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(1),

            ClanName = "Zhang",

            HomeSettlementId = new SettlementId(2),

            Prestige = 47,

            SupportReserve = 48,

            HeirPersonId = new PersonId(1),

        });


        SocialMemoryAndRelationsModule socialModule = new();

        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        socialState.ClanNarratives.Add(new ClanNarrativeState

        {

            ClanId = new ClanId(1),

            PublicNarrative = "兰溪乡面压力渐聚。",

            GrudgePressure = 58,

            FearPressure = 57,

            ShamePressure = 26,

            FavorBalance = 4,

        });


        TradeAndIndustryModule tradeModule = new();

        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();

        tradeState.Clans.Add(new ClanTradeState

        {

            ClanId = new ClanId(1),

            PrimarySettlementId = new SettlementId(2),

            CashReserve = 80,

            GrainReserve = 60,

            Debt = 18,

            CommerceReputation = 31,

            ShopCount = 1,

            ManagerSkill = 3,

            LastOutcome = "Stable",

            LastExplanation = "路面之压渐起。",

        });

        tradeState.Routes.Add(new RouteTradeState

        {

            RouteId = 1,

            ClanId = new ClanId(1),

            RouteName = "Lanxi River Wharf",

            SettlementId = new SettlementId(2),

            IsActive = true,

            Capacity = 30,

            Risk = 70,

            LastMargin = 4,

        });


        OrderAndBanditryModule module = new();

        OrderAndBanditryState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 13,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        module.RegisterQueries(state, queries);

        IOrderAndBanditryQueries disorderQueries = queries.GetRequired<IOrderAndBanditryQueries>();


        ModuleExecutionContext context = new(

            new GameDate(1200, 6),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(77)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff());


        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));


        SettlementDisorderSnapshot snapshot = disorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));

        Assert.That(snapshot.BanditThreat, Is.GreaterThanOrEqualTo(60));

        Assert.That(snapshot.RoutePressure, Is.GreaterThanOrEqualTo(60));

        Assert.That(snapshot.DisorderPressure, Is.GreaterThanOrEqualTo(70));

        Assert.That(snapshot.LastPressureReason, Does.Contain("乡面安宁"));

        Assert.That(snapshot.LastPressureReason, Does.Contain("民困"));

        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(OrderAndBanditryEventNames.BanditThreatRaised));

        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(OrderAndBanditryEventNames.OutlawGroupFormed));

        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(OrderAndBanditryEventNames.RouteUnsafeDueToBanditry));

        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.OrderAndBanditry));

        Assert.That(module.AcceptedCommands, Does.Contain("FundLocalWatch"));

        Assert.That(module.PublishedEvents, Does.Contain(OrderAndBanditryEventNames.RouteUnsafeDueToBanditry));

    }


    [Test]

    public void RunMonth_ProjectsBlackRoutePressureThroughOwnedQuery()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(2),

            Name = "Lanxi",

            Security = 39,

            Prosperity = 58,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(2),

            CommonerDistress = 68,

            LaborSupply = 92,

            MigrationPressure = 56,

            MilitiaPotential = 44,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(1),

            ClanName = "Zhang",

            HomeSettlementId = new SettlementId(2),

            Prestige = 47,

            SupportReserve = 48,

            HeirPersonId = new PersonId(1),

        });


        SocialMemoryAndRelationsModule socialModule = new();

        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        socialState.ClanNarratives.Add(new ClanNarrativeState

        {

            ClanId = new ClanId(1),

            PublicNarrative = "Lanxi wharf pressure is thickening.",

            GrudgePressure = 58,

            FearPressure = 57,

            ShamePressure = 26,

            FavorBalance = 4,

        });


        TradeAndIndustryModule tradeModule = new();

        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();

        tradeState.Clans.Add(new ClanTradeState

        {

            ClanId = new ClanId(1),

            PrimarySettlementId = new SettlementId(2),

            CashReserve = 80,

            GrainReserve = 60,

            Debt = 18,

            CommerceReputation = 31,

            ShopCount = 1,

            ManagerSkill = 3,

            LastOutcome = "Stable",

            LastExplanation = "Wharf traffic is still moving.",

        });

        tradeState.Routes.Add(new RouteTradeState

        {

            RouteId = 1,

            ClanId = new ClanId(1),

            RouteName = "Lanxi River Wharf",

            SettlementId = new SettlementId(2),

            IsActive = true,

            Capacity = 30,

            Risk = 70,

            LastMargin = 4,

        });

        tradeState.BlackRouteLedgers.Add(new SettlementBlackRouteLedgerState

        {

            SettlementId = new SettlementId(2),

            ShadowPriceIndex = 122,

            DiversionShare = 42,

            IllicitMargin = 16,

            BlockedShipmentCount = 2,

            SeizureRisk = 24,

            DiversionBandLabel = "正私并行",

            LastLedgerTrace = "Lanxi already has shadow cargo peeling off from the main wharf.",

        });


        OrderAndBanditryModule module = new();

        OrderAndBanditryState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 13,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

            BlackRoutePressure = 58,

            CoercionRisk = 22,

        });


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        module.RegisterQueries(state, queries);


        ModuleExecutionContext context = new(

            new GameDate(1200, 6),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(771)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff());


        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));


        SettlementBlackRoutePressureSnapshot snapshot = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(snapshot.BlackRoutePressure, Is.GreaterThanOrEqualTo(60));

        Assert.That(snapshot.CoercionRisk, Is.GreaterThan(22));

        Assert.That(snapshot.EscalationBandLabel, Is.Not.Empty);

        Assert.That(snapshot.LastPressureTrace, Does.Contain("私下分流"));

        Assert.That(snapshot.LastPressureTrace, Does.Contain("民困"));

        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(OrderAndBanditryEventNames.BlackRoutePressureRaised));

    }


}
