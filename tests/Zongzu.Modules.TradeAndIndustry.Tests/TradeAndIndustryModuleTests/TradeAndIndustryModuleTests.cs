using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.TradeAndIndustry.Tests;

[TestFixture]
public sealed partial class TradeAndIndustryModuleTests
{
    [Test]
    public void RunXun_ShangxunUpdatesMarketHeatWithoutReadableOutput()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 44,
            Prosperity = 61,
            BaselineInstitutionCount = 2,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 58,
            LaborSupply = 104,
            MigrationPressure = 24,
            MilitiaPotential = 70,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Lanxi traders still hold together.",
            GrudgePressure = 18,
            FearPressure = 16,
            ShamePressure = 12,
            FavorBalance = 14,
        });

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "Lanxi Morning Market",
            PriceIndex = 108,
            Demand = 72,
            LocalRisk = 18,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(171)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Shangxun);

        tradeModule.RunXun(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, context));

        SettlementMarketState market = tradeState.Markets.Single();
        Assert.That(market.PriceIndex, Is.EqualTo(110));
        Assert.That(market.Demand, Is.EqualTo(74));
        Assert.That(market.LocalRisk, Is.EqualTo(19));
        Assert.That(context.Diff.Entries, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunXun_ZhongAndXiaxunRefreshGrayRouteAndRouteConstraintsWithoutReadableOutput()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 47,
            Prosperity = 59,
            BaselineInstitutionCount = 2,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 56,
            LaborSupply = 96,
            MigrationPressure = 24,
            MilitiaPotential = 70,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Lanxi traders are sliding toward side traffic.",
            GrudgePressure = 64,
            FearPressure = 18,
            ShamePressure = 12,
            FavorBalance = 14,
        });

        SettlementDisorderSnapshot disorder = new()
        {
            SettlementId = new SettlementId(1),
            BanditThreat = 67,
            RoutePressure = 63,
            SuppressionDemand = 30,
            DisorderPressure = 40,
            LastPressureReason = "Lanxi roads are tightening.",
        };
        SettlementBlackRoutePressureSnapshot pressure = new()
        {
            SettlementId = new SettlementId(1),
            BlackRoutePressure = 58,
            CoercionRisk = 34,
            SuppressionRelief = 0,
            ResponseActivationLevel = 4,
            PaperCompliance = 56,
            ImplementationDrag = 20,
            RouteShielding = 12,
            RetaliationRisk = 70,
            AdministrativeSuppressionWindow = 4,
            EscalationBandLabel = "正私并行",
            LastPressureTrace = "Lanxi side traffic is tightening against the docks.",
        };

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "Lanxi Morning Market",
            PriceIndex = 112,
            Demand = 73,
            LocalRisk = 61,
        });
        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(1),
            RouteName = "Lanxi River Wharf",
            SettlementId = new SettlementId(1),
            IsActive = true,
            Capacity = 24,
            Risk = 48,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries([disorder]));
        queries.Register<IBlackRoutePressureQueries>(new StubBlackRoutePressureQueries([pressure]));
        tradeModule.RegisterQueries(tradeState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);

        ModuleExecutionContext zhongxunContext = new(
            new GameDate(1200, 4),
            manifest,
            new DeterministicRandom(KernelState.Create(172)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Zhongxun);

        tradeModule.RunXun(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, zhongxunContext));

        ModuleExecutionContext xiaxunContext = new(
            new GameDate(1200, 4),
            manifest,
            new DeterministicRandom(KernelState.Create(172)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Xiaxun);

        tradeModule.RunXun(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, xiaxunContext));

        SettlementBlackRouteLedgerSnapshot ledger = queries.GetRequired<IBlackRouteLedgerQueries>()
            .GetRequiredSettlementBlackRouteLedger(new SettlementId(1));
        ClanTradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
            .GetRoutesForClan(new ClanId(1))
            .Single();
        SettlementMarketState market = tradeState.Markets.Single();

        Assert.That(ledger.ShadowPriceIndex, Is.EqualTo(103));
        Assert.That(ledger.DiversionShare, Is.EqualTo(2));
        Assert.That(ledger.BlockedShipmentCount, Is.EqualTo(2));
        Assert.That(ledger.SeizureRisk, Is.EqualTo(4));
        Assert.That(ledger.DiversionBandLabel, Is.Not.Empty);
        Assert.That(route.Risk, Is.EqualTo(51));
        Assert.That(route.BlockedShipmentCount, Is.EqualTo(6));
        Assert.That(route.SeizureRisk, Is.EqualTo(52));
        Assert.That(route.RouteConstraintLabel, Is.Not.Empty);
        Assert.That(route.LastRouteTrace, Does.Contain("Lanxi River Wharf"));
        Assert.That(market.LocalRisk, Is.EqualTo(64));
        Assert.That(zhongxunContext.Diff.Entries, Is.Empty);
        Assert.That(zhongxunContext.DomainEvents.Events, Is.Empty);
        Assert.That(xiaxunContext.Diff.Entries, Is.Empty);
        Assert.That(xiaxunContext.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_ProducesExplainableTradeProfitForStableLocalMarket()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 61,
            Prosperity = 66,
            BaselineInstitutionCount = 2,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 54,
            SupportReserve = 64,
            HeirPersonId = new PersonId(1),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 39,
            LaborSupply = 112,
            MigrationPressure = 18,
            MilitiaPotential = 74,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Reliable traders",
            GrudgePressure = 12,
            FearPressure = 10,
            ShamePressure = 14,
            FavorBalance = 18,
        });

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "Lanxi Morning Market",
            PriceIndex = 108,
            Demand = 72,
            LocalRisk = 15,
        });
        tradeState.Clans.Add(new ClanTradeState
        {
            ClanId = new ClanId(1),
            PrimarySettlementId = new SettlementId(1),
            CashReserve = 82,
            GrainReserve = 57,
            Debt = 18,
            CommerceReputation = 31,
            ShopCount = 1,
            ManagerSkill = 3,
            LastOutcome = "Stable",
        });
        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(1),
            RouteName = "Lanxi River Wharf",
            SettlementId = new SettlementId(1),
            IsActive = true,
            Capacity = 24,
            Risk = 15,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(17)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        tradeModule.RunMonth(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, context));

        ClanTradeState clanTrade = tradeState.Clans.Single();
        Assert.That(clanTrade.LastOutcome, Is.EqualTo("Profit"));
        Assert.That(clanTrade.LastExplanation, Is.Not.Empty);
        Assert.That(clanTrade.CashReserve, Is.GreaterThan(82));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(TradeAndIndustryEventNames.TradeProspered));
    }

    [Test]
    public void RunMonth_GrainPulse_HighGrainStoreLowDistress_PushesGrainPriceBelowBaseline()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 60,
            Prosperity = 62,
            BaselineInstitutionCount = 2,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 30,
            LaborSupply = 80,
            MigrationPressure = 10,
            MilitiaPotential = 50,
        });
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            Distress = 25,
            DebtPressure = 18,
            LaborCapacity = 55,
            GrainStore = 90,
            LandHolding = 40,
            ToolCondition = 50,
            ShelterQuality = 50,
            LaborerCount = 2,
        });
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Tenant Wang",
            SettlementId = new SettlementId(1),
            Distress = 28,
            DebtPressure = 20,
            LaborCapacity = 50,
            GrainStore = 85,
            LandHolding = 35,
            ToolCondition = 50,
            ShelterQuality = 50,
            LaborerCount = 2,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "Lanxi Morning Market",
            PriceIndex = 100,
            Demand = 50,
            LocalRisk = 15,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(88)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        tradeModule.RunMonth(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, context));

        MarketGoodsEntryState grain = tradeState.MarketGoods
            .Single(entry => entry.SettlementId == new SettlementId(1) && entry.Goods == GoodsCategory.Grain);
        Assert.That(grain.Supply, Is.GreaterThan(grain.Demand), "丰年低困应当供过于求。");
        Assert.That(grain.CurrentPrice, Is.LessThan(grain.BasePrice), "供过于求应当压低粮价。");
        Assert.That(grain.BasePrice, Is.EqualTo(100));

        ITradeAndIndustryQueries tradeQueries = queries.GetRequired<ITradeAndIndustryQueries>();
        IReadOnlyList<MarketGoodsSnapshot> goods = tradeQueries.GetMarketGoodsAt(new SettlementId(1));
        Assert.That(goods, Has.Count.EqualTo(1));
        Assert.That(goods[0].Goods, Is.EqualTo(GoodsCategory.Grain));
    }

}
