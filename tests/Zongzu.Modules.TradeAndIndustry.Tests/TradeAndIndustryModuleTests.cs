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
        TradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
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
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("TradeProspered"));
    }

    [Test]
    public void RunMonth_TracksGrayLedgerFromOrderOwnedPressureQueries()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 49,
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
            CommonerDistress = 58,
            LaborSupply = 112,
            MigrationPressure = 36,
            MilitiaPotential = 74,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Lanxi traders are navigating a dirtier market.",
            GrudgePressure = 22,
            FearPressure = 18,
            ShamePressure = 14,
            FavorBalance = 18,
        });

        SettlementDisorderSnapshot disorder = new()
        {
            SettlementId = new SettlementId(1),
            BanditThreat = 48,
            RoutePressure = 54,
            SuppressionDemand = 32,
            DisorderPressure = 41,
            LastPressureReason = "Lanxi order is tightening around the wharf.",
        };
        SettlementBlackRoutePressureSnapshot pressure = new()
        {
            SettlementId = new SettlementId(1),
            BlackRoutePressure = 63,
            CoercionRisk = 55,
            SuppressionRelief = 1,
            ResponseActivationLevel = 4,
            PaperCompliance = 58,
            ImplementationDrag = 44,
            RouteShielding = 46,
            RetaliationRisk = 68,
            AdministrativeSuppressionWindow = 3,
            EscalationBandLabel = "正私并行",
            LastPressureTrace = "Lanxi dark-route pressure is already pressing the market edge.",
        };

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = new SettlementId(1),
            MarketName = "Lanxi Morning Market",
            PriceIndex = 108,
            Demand = 72,
            LocalRisk = 28,
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
        queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries([disorder]));
        queries.Register<IBlackRoutePressureQueries>(new StubBlackRoutePressureQueries([pressure]));
        tradeModule.RegisterQueries(tradeState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            manifest,
            new DeterministicRandom(KernelState.Create(1701)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        tradeModule.RunMonth(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, context));

        SettlementBlackRouteLedgerSnapshot ledger = queries.GetRequired<IBlackRouteLedgerQueries>()
            .GetRequiredSettlementBlackRouteLedger(new SettlementId(1));
        ClanTradeState clanTrade = tradeState.Clans.Single();
        TradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
            .GetRoutesForClan(new ClanId(1))
            .Single();

        Assert.That(ledger.DiversionShare, Is.GreaterThan(0));
        Assert.That(ledger.ShadowPriceIndex, Is.GreaterThan(100));
        Assert.That(ledger.DiversionBandLabel, Is.Not.Empty);
        Assert.That(ledger.LastLedgerTrace, Is.Not.Empty);
        Assert.That(route.BlockedShipmentCount, Is.GreaterThanOrEqualTo(1));
        Assert.That(route.SeizureRisk, Is.GreaterThan(0));
        Assert.That(route.Risk, Is.GreaterThan(15));
        Assert.That(route.RouteConstraintLabel, Is.Not.Empty);
        Assert.That(route.LastRouteTrace, Does.Contain("Lanxi River Wharf"));
        Assert.That(clanTrade.LastExplanation, Is.Not.Empty);
    }

    [Test]
    public void RunMonth_DistinguishesRouteShieldingFromRetaliationRiskWhenReadingOrderPressure()
    {
        static (int RouteRisk, int DiversionShare, int SeizureRisk) RunTradeMonth(int routeShielding, int retaliationRisk)
        {
            WorldSettlementsModule worldModule = new();
            WorldSettlementsState worldState = worldModule.CreateInitialState();
            worldState.Settlements.Add(new SettlementStateData
            {
                Id = new SettlementId(1),
                Name = "Lanxi",
                Security = 52,
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
                CommonerDistress = 52,
                LaborSupply = 112,
                MigrationPressure = 28,
                MilitiaPotential = 74,
            });

            SocialMemoryAndRelationsModule socialModule = new();
            SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
            socialState.ClanNarratives.Add(new ClanNarrativeState
            {
                ClanId = new ClanId(1),
                PublicNarrative = "Lanxi traders are caught between patrols and whispers.",
                GrudgePressure = 22,
                FearPressure = 18,
                ShamePressure = 14,
                FavorBalance = 18,
            });

            SettlementDisorderSnapshot disorder = new()
            {
                SettlementId = new SettlementId(1),
                BanditThreat = 44,
                RoutePressure = 52,
                SuppressionDemand = 30,
                DisorderPressure = 38,
                LastPressureReason = "Lanxi order is tightening around the wharf.",
            };
            SettlementBlackRoutePressureSnapshot pressure = new()
            {
                SettlementId = new SettlementId(1),
                BlackRoutePressure = 56,
                CoercionRisk = 48,
                SuppressionRelief = 2,
                ResponseActivationLevel = 4,
                PaperCompliance = 58,
                ImplementationDrag = 36,
                RouteShielding = routeShielding,
                RetaliationRisk = retaliationRisk,
                AdministrativeSuppressionWindow = 2,
                EscalationBandLabel = "正私并行",
                LastPressureTrace = "Lanxi order pressure is dividing the road.",
            };

            TradeAndIndustryModule tradeModule = new();
            TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
            tradeState.Markets.Add(new SettlementMarketState
            {
                SettlementId = new SettlementId(1),
                MarketName = "Lanxi Morning Market",
                PriceIndex = 108,
                Demand = 72,
                LocalRisk = 32,
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
                Risk = 16,
            });

            QueryRegistry queries = new();
            worldModule.RegisterQueries(worldState, queries);
            familyModule.RegisterQueries(familyState, queries);
            populationModule.RegisterQueries(populationState, queries);
            socialModule.RegisterQueries(socialState, queries);
            queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries([disorder]));
            queries.Register<IBlackRoutePressureQueries>(new StubBlackRoutePressureQueries([pressure]));
            tradeModule.RegisterQueries(tradeState, queries);

            FeatureManifest manifest = new();
            manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);

            ModuleExecutionContext context = new(
                new GameDate(1200, 4),
                manifest,
                new DeterministicRandom(KernelState.Create(1777)),
                queries,
                new DomainEventBuffer(),
                new WorldDiff());

            tradeModule.RunMonth(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, context));

            SettlementBlackRouteLedgerSnapshot ledger = queries.GetRequired<IBlackRouteLedgerQueries>()
                .GetRequiredSettlementBlackRouteLedger(new SettlementId(1));
            TradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
                .GetRoutesForClan(new ClanId(1))
                .Single();

            return (route.Risk, ledger.DiversionShare, route.SeizureRisk);
        }

        (int shieldingRisk, int shieldingDiversion, int shieldingSeizure) = RunTradeMonth(70, 10);
        (int retaliationRisk, int retaliationDiversion, int retaliationSeizure) = RunTradeMonth(10, 70);

        Assert.That(retaliationRisk, Is.GreaterThan(shieldingRisk));
        Assert.That(retaliationDiversion, Is.GreaterThan(shieldingDiversion));
        Assert.That(shieldingSeizure, Is.GreaterThan(retaliationSeizure));
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
            LastExplanation = "Road pressure is still manageable.",
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
                CampaignName = "Lanxi campaign board",
                IsActive = true,
                MobilizedForceCount = 46,
                FrontPressure = 74,
                FrontLabel = "Front tightening",
                SupplyState = 36,
                SupplyStateLabel = "Supply squeezed",
                MoraleState = 43,
                MoraleStateLabel = "Morale shaken",
                CommandFitLabel = "Command still holds",
                CommanderSummary = "Lanxi command still holds together.",
                ActiveDirectiveCode = WarfareCampaignCommandNames.ProtectSupplyLine,
                ActiveDirectiveLabel = "Protect supply line",
                ActiveDirectiveSummary = "Escort the grain route.",
                LastDirectiveTrace = "Lanxi shifted force toward the grain route.",
                MobilizationWindowLabel = "Narrow window",
                SupplyLineSummary = "The grain line is under road pressure.",
                OfficeCoordinationTrace = "The yamen is watching the case desk and grain route.",
                SourceTrace = "Campaign pressure rose from local conflict.",
                LastAftermathSummary = "Aftermath pressure sits on the grain line and trade road.",
            },
        ]));

        DomainEventRecord[] events =
        {
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignPressureRaised, "Lanxi front tightened.", "1"),
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply line tightened.", "1"),
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
        Assert.That(route.BlockedShipmentCount, Is.GreaterThan(0));
        Assert.That(route.SeizureRisk, Is.GreaterThan(0));
        Assert.That(route.RouteConstraintLabel, Is.Not.Empty);
        Assert.That(route.LastRouteTrace, Does.Contain("Lanxi"));
        Assert.That(clanTrade.CashReserve, Is.LessThan(82));
        Assert.That(clanTrade.GrainReserve, Is.LessThan(57));
        Assert.That(clanTrade.Debt, Is.GreaterThan(18));
        Assert.That(clanTrade.LastExplanation, Is.Not.Empty);
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.TradeAndIndustry));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("RouteBusinessBlocked"));
        Assert.That(context.DomainEvents.Events.All(static entry => entry.ModuleKey == KnownModuleKeys.TradeAndIndustry), Is.True);
    }

    [Test]
    public void RunMonth_ReadsInterventionCarryoverAsDistinctTradeFollowThrough()
    {
        static (int RouteRisk, int DiversionShare, int SeizureRisk) RunTradeMonth(string commandName, int carryoverMonths)
        {
            WorldSettlementsModule worldModule = new();
            WorldSettlementsState worldState = worldModule.CreateInitialState();
            worldState.Settlements.Add(new SettlementStateData
            {
                Id = new SettlementId(1),
                Name = "Lanxi",
                Security = 52,
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
                CommonerDistress = 52,
                LaborSupply = 112,
                MigrationPressure = 28,
                MilitiaPotential = 74,
            });

            SocialMemoryAndRelationsModule socialModule = new();
            SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
            socialState.ClanNarratives.Add(new ClanNarrativeState
            {
                ClanId = new ClanId(1),
                PublicNarrative = "Lanxi traders are balancing patrols, whispers, and side traffic.",
                GrudgePressure = 22,
                FearPressure = 18,
                ShamePressure = 14,
                FavorBalance = 18,
            });

            SettlementDisorderSnapshot disorder = new()
            {
                SettlementId = new SettlementId(1),
                BanditThreat = 44,
                RoutePressure = 52,
                SuppressionDemand = 30,
                DisorderPressure = 38,
                LastPressureReason = "Lanxi order is tightening around the wharf.",
                LastInterventionCommandCode = commandName,
                LastInterventionCommandLabel = commandName,
                LastInterventionSummary = "Recent order action is still echoing.",
                LastInterventionOutcome = "The road is still reacting.",
                InterventionCarryoverMonths = carryoverMonths,
            };
            SettlementBlackRoutePressureSnapshot pressure = new()
            {
                SettlementId = new SettlementId(1),
                BlackRoutePressure = 56,
                CoercionRisk = 48,
                SuppressionRelief = 2,
                ResponseActivationLevel = 4,
                PaperCompliance = 58,
                ImplementationDrag = 36,
                RouteShielding = 46,
                RetaliationRisk = 34,
                AdministrativeSuppressionWindow = 2,
                EscalationBandLabel = "正私并行",
                LastPressureTrace = "Lanxi order pressure is dividing the road.",
            };

            TradeAndIndustryModule tradeModule = new();
            TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
            tradeState.Markets.Add(new SettlementMarketState
            {
                SettlementId = new SettlementId(1),
                MarketName = "Lanxi Morning Market",
                PriceIndex = 108,
                Demand = 72,
                LocalRisk = 32,
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
                Risk = 18,
            });

            QueryRegistry queries = new();
            worldModule.RegisterQueries(worldState, queries);
            familyModule.RegisterQueries(familyState, queries);
            populationModule.RegisterQueries(populationState, queries);
            socialModule.RegisterQueries(socialState, queries);
            queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries([disorder]));
            queries.Register<IBlackRoutePressureQueries>(new StubBlackRoutePressureQueries([pressure]));
            tradeModule.RegisterQueries(tradeState, queries);

            FeatureManifest manifest = new();
            manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);

            ModuleExecutionContext context = new(
                new GameDate(1200, 4),
                manifest,
                new DeterministicRandom(KernelState.Create(1778)),
                queries,
                new DomainEventBuffer(),
                new WorldDiff());

            tradeModule.RunMonth(new ModuleExecutionScope<TradeAndIndustryState>(tradeState, context));

            SettlementBlackRouteLedgerSnapshot ledger = queries.GetRequired<IBlackRouteLedgerQueries>()
                .GetRequiredSettlementBlackRouteLedger(new SettlementId(1));
            TradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
                .GetRoutesForClan(new ClanId(1))
                .Single();

            return (route.Risk, ledger.DiversionShare, route.SeizureRisk);
        }

        (int watchBaselineRisk, int watchBaselineDiversion, int watchBaselineSeizure) = RunTradeMonth(PlayerCommandNames.FundLocalWatch, 0);
        (int watchRisk, int watchDiversion, int watchSeizure) = RunTradeMonth(PlayerCommandNames.FundLocalWatch, 1);
        (int negotiateBaselineRisk, int negotiateBaselineDiversion, int negotiateBaselineSeizure) = RunTradeMonth(PlayerCommandNames.NegotiateWithOutlaws, 0);
        (int negotiateRisk, int negotiateDiversion, int negotiateSeizure) = RunTradeMonth(PlayerCommandNames.NegotiateWithOutlaws, 1);

        Assert.That(watchRisk, Is.LessThanOrEqualTo(watchBaselineRisk));
        Assert.That(watchDiversion, Is.LessThanOrEqualTo(watchBaselineDiversion));
        Assert.That(negotiateDiversion, Is.GreaterThan(negotiateBaselineDiversion));
        Assert.That(negotiateSeizure, Is.LessThan(negotiateBaselineSeizure));
        Assert.That(watchSeizure, Is.GreaterThan(negotiateSeizure));
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

    private sealed class StubOrderQueries : IOrderAndBanditryQueries
    {
        private readonly IReadOnlyList<SettlementDisorderSnapshot> _disorders;

        public StubOrderQueries(IReadOnlyList<SettlementDisorderSnapshot> disorders)
        {
            _disorders = disorders;
        }

        public SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId)
        {
            return _disorders.Single(disorder => disorder.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder()
        {
            return _disorders;
        }
    }

    private sealed class StubBlackRoutePressureQueries : IBlackRoutePressureQueries
    {
        private readonly IReadOnlyList<SettlementBlackRoutePressureSnapshot> _pressures;

        public StubBlackRoutePressureQueries(IReadOnlyList<SettlementBlackRoutePressureSnapshot> pressures)
        {
            _pressures = pressures;
        }

        public SettlementBlackRoutePressureSnapshot GetRequiredSettlementBlackRoutePressure(SettlementId settlementId)
        {
            return _pressures.Single(pressure => pressure.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementBlackRoutePressureSnapshot> GetSettlementBlackRoutePressures()
        {
            return _pressures;
        }
    }
}
