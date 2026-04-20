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
            ClanTradeRouteSnapshot route = queries.GetRequired<ITradeAndIndustryQueries>()
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

}
