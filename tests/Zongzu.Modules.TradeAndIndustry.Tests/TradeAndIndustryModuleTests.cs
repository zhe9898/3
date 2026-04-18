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

    [Test]
    public void HandleEvents_AppliesCampaignSpilloverInsideTradeOwnedState()
    {
        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "Lanxi Morning Market",
            PriceIndex = 108,
            Demand = 72,
            LocalRisk = 48,
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
            LastExplanation = "Route pressure is stable.",
        });
        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(1),
            RouteName = "Lanxi River Wharf",
            SettlementId = new SettlementId(1),
            IsActive = true,
            Capacity = 24,
            Risk = 58,
            LastMargin = 6,
        });

        QueryRegistry queries = new();
        tradeModule.RegisterQueries(tradeState, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi军务沙盘",
                IsActive = true,
                MobilizedForceCount = 46,
                FrontPressure = 74,
                FrontLabel = "前线转紧",
                SupplyState = 36,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 43,
                MoraleStateLabel = "军心摇动",
                CommandFitLabel = "号令尚整",
                CommanderSummary = "Lanxi command is holding.",
                ActiveDirectiveCode = WarfareCampaignCommandNames.ProtectSupplyLine,
                ActiveDirectiveLabel = "催督粮道",
                ActiveDirectiveSummary = "督运。",
                LastDirectiveTrace = "Lanxi received a supply-line directive.",
                MobilizationWindowLabel = "Narrow",
                SupplyLineSummary = "Grain caravans are under pressure.",
                OfficeCoordinationTrace = "Registrar is coordinating the docket.",
                SourceTrace = "Campaign pressure rose from local conflict.",
                LastAftermathSummary = "战后覆核压在粮道与商路之上。",
            },
        ]));

        DomainEventRecord[] events =
        {
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignPressureRaised, "Lanxi pressure rose.", "1"),
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
        };

        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7001)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        tradeModule.HandleEvents(new ModuleEventHandlingScope<TradeAndIndustryState>(tradeState, context, events));

        ClanTradeState clanTrade = tradeState.Clans.Single();
        SettlementMarketState market = tradeState.Markets.Single();
        RouteTradeState route = tradeState.Routes.Single();

        Assert.That(market.LocalRisk, Is.GreaterThan(48));
        Assert.That(route.Risk, Is.GreaterThan(58));
        Assert.That(clanTrade.CashReserve, Is.LessThan(82));
        Assert.That(clanTrade.GrainReserve, Is.LessThan(57));
        Assert.That(clanTrade.Debt, Is.GreaterThan(18));
        Assert.That(clanTrade.LastExplanation, Does.Contain("Campaign pressure around Lanxi"));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.TradeAndIndustry));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("RouteBusinessBlocked"));
        Assert.That(context.DomainEvents.Events.All(static entry => entry.ModuleKey == KnownModuleKeys.TradeAndIndustry), Is.True);
    }

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }
    }
}
