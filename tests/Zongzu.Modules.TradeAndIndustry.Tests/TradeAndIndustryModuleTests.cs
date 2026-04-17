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
public sealed class TradeAndIndustryModuleTests
{
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
        Assert.That(clanTrade.LastExplanation, Does.Contain("Margin"));
        Assert.That(clanTrade.LastExplanation, Does.Contain("route factor"));
        Assert.That(clanTrade.CashReserve, Is.GreaterThan(82));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("TradeProspered"));
    }
}
