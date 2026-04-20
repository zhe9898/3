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

public sealed partial class TradeAndIndustryModuleTests
{
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
        ClanTradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
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
            ClanTradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
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

}
