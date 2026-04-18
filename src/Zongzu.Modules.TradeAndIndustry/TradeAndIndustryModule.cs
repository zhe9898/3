using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
{
    private static readonly string[] CommandNames =
    [
        "OpenShop",
        "CloseShop",
        "ExpandTradeRoute",
        "BorrowOrInvest",
        "AppointManager",
    ];

    private static readonly string[] EventNames =
    [
        "TradeProspered",
        "TradeLossOccurred",
        "TradeDebtDefaulted",
        "RouteBusinessBlocked",
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.TradeAndIndustry;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 600;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override TradeAndIndustryState CreateInitialState()
    {
        return new TradeAndIndustryState();
    }

    public override void RegisterQueries(TradeAndIndustryState state, QueryRegistry queries)
    {
        queries.Register<ITradeAndIndustryQueries>(new TradeQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<TradeAndIndustryState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();
        IOrderAndBanditryQueries? orderQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)
            ? scope.GetRequiredQuery<IOrderAndBanditryQueries>()
            : null;
        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = orderQueries is null
            ? new Dictionary<SettlementId, SettlementDisorderSnapshot>()
            : orderQueries.GetSettlementDisorder().ToDictionary(static disorder => disorder.SettlementId, static disorder => disorder);

        Dictionary<SettlementId, SettlementMarketState> markets = scope.State.Markets
            .OrderBy(static market => market.SettlementId.Value)
            .ToDictionary(static market => market.SettlementId, static market => market);

        foreach (SettlementMarketState market in scope.State.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(market.SettlementId);
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(market.SettlementId);
            int orderMarketPenalty = disorderBySettlement.TryGetValue(market.SettlementId, out SettlementDisorderSnapshot? disorder)
                ? ComputeOrderPenalty(disorder)
                : 0;

            market.PriceIndex = Math.Clamp(
                market.PriceIndex
                + (population.CommonerDistress >= 55 ? 1 : -1)
                + (settlement.Prosperity >= 60 ? 1 : 0)
                + scope.Context.Random.NextInt(-1, 2),
                50,
                150);
            market.Demand = Math.Clamp(
                market.Demand
                + (population.LaborSupply >= 90 ? 1 : -1)
                + (settlement.Prosperity >= 58 ? 1 : 0)
                + scope.Context.Random.NextInt(-1, 2),
                0,
                100);
            market.LocalRisk = Math.Clamp(
                market.LocalRisk
                + (settlement.Security < 45 ? 2 : -1)
                + (population.MigrationPressure >= 60 ? 1 : 0)
                + orderMarketPenalty,
                0,
                100);
        }

        foreach (ClanTradeState clanTrade in scope.State.Clans.OrderBy(static trade => trade.ClanId.Value))
        {
            if (!markets.TryGetValue(clanTrade.PrimarySettlementId, out SettlementMarketState? market))
            {
                continue;
            }

            ClanSnapshot clan = familyQueries.GetRequiredClan(clanTrade.ClanId);
            ClanNarrativeSnapshot narrative = socialQueries.GetRequiredClanNarrative(clanTrade.ClanId);
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(clanTrade.PrimarySettlementId);
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(clanTrade.PrimarySettlementId);
            RouteTradeState[] routes = scope.State.Routes
                .Where(route => route.ClanId == clanTrade.ClanId && route.IsActive)
                .OrderBy(static route => route.RouteId)
                .ToArray();

            int routeFactor = 0;
            int orderPressure = 0;
            foreach (RouteTradeState route in routes)
            {
                int orderPenalty = disorderBySettlement.TryGetValue(route.SettlementId, out SettlementDisorderSnapshot? disorder)
                    ? ComputeOrderPenalty(disorder)
                    : 0;

                route.Risk = Math.Clamp(
                    route.Risk
                    + (market.LocalRisk >= 55 ? 1 : -1)
                    + (narrative.GrudgePressure >= 60 ? 1 : 0)
                    + orderPenalty,
                    0,
                    100);
                route.LastMargin = Math.Clamp((route.Capacity / 8) - (route.Risk / 10) - orderPenalty + scope.Context.Random.NextInt(-2, 3), -20, 30);
                routeFactor += route.LastMargin;
                orderPressure += orderPenalty;
            }

            int laborFactor = population.LaborSupply >= 100 ? 2 : population.LaborSupply >= 60 ? 1 : -1;
            int trustFactor = narrative.FavorBalance >= 10 ? 1 : 0;
            int grudgePenalty = narrative.GrudgePressure >= 60 ? 2 : narrative.GrudgePressure >= 40 ? 1 : 0;
            int clanSupport = clan.SupportReserve >= 55 ? 1 : 0;
            int prosperityFactor = settlement.Prosperity >= 60 ? 2 : settlement.Prosperity < 45 ? -1 : 0;
            int debtPenalty = clanTrade.Debt / 30;

            int margin = routeFactor
                + (market.Demand / 10)
                + ((market.PriceIndex - 100) / 10)
                + laborFactor
                + trustFactor
                + clanSupport
                + prosperityFactor
                + clanTrade.ManagerSkill
                - orderPressure
                - grudgePenalty
                - debtPenalty
                + scope.Context.Random.NextInt(-3, 4);

            clanTrade.CashReserve = Math.Clamp(clanTrade.CashReserve + (margin * 2), 0, 500);
            clanTrade.GrainReserve = Math.Clamp(clanTrade.GrainReserve + Math.Max(-4, margin / 2), 0, 500);

            if (margin >= 0)
            {
                clanTrade.Debt = Math.Clamp(clanTrade.Debt - Math.Max(1, margin / 3), 0, 300);
                clanTrade.CommerceReputation = Math.Clamp(clanTrade.CommerceReputation + 1, 0, 100);
                clanTrade.LastOutcome = "Profit";
            }
            else
            {
                clanTrade.Debt = Math.Clamp(clanTrade.Debt + Math.Abs(margin), 0, 300);
                clanTrade.CommerceReputation = Math.Clamp(clanTrade.CommerceReputation - 1, 0, 100);
                clanTrade.LastOutcome = "Loss";
            }

            clanTrade.LastExplanation =
                $"Margin {margin} from demand {market.Demand}, price index {market.PriceIndex}, labor {population.LaborSupply}, route factor {routeFactor}, order pressure {orderPressure}, and grudge penalty {grudgePenalty}.";

            scope.RecordDiff(
                $"Clan trade for {clan.ClanName} moved to cash {clanTrade.CashReserve}, debt {clanTrade.Debt}, outcome {clanTrade.LastOutcome}. {clanTrade.LastExplanation}",
                clanTrade.ClanId.Value.ToString());

            if (margin >= 8)
            {
                scope.Emit("TradeProspered", $"Clan {clan.ClanName} trade prospered.");
            }
            else if (margin < 0)
            {
                scope.Emit("TradeLossOccurred", $"Clan {clan.ClanName} trade suffered losses.");
            }

            if (clanTrade.Debt >= 120 && clanTrade.CashReserve <= 40)
            {
                scope.Emit("TradeDebtDefaulted", $"Clan {clan.ClanName} trade debt defaulted.");
            }

            if (routes.Any(static route => route.Risk >= 70) || orderPressure >= 3)
            {
                scope.Emit("RouteBusinessBlocked", $"A trade route for clan {clan.ClanName} was partially blocked.");
            }
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<TradeAndIndustryState> scope)
    {
        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            int routeRiskDelta = ComputeCampaignTradeRiskDelta(bundle, campaign);
            int marketRiskDelta = Math.Max(1, routeRiskDelta - 1);
            int cashLoss = ComputeCampaignCashLoss(bundle, campaign);
            int grainLoss = ComputeCampaignGrainLoss(bundle, campaign);
            int debtIncrease = ComputeCampaignDebtIncrease(bundle, campaign);
            int reputationLoss = bundle.CampaignSupplyStrained ? 2 : 1;

            SettlementMarketState? market = scope.State.Markets.SingleOrDefault(existing => existing.SettlementId == bundle.SettlementId);
            if (market is not null)
            {
                market.LocalRisk = Math.Clamp(market.LocalRisk + marketRiskDelta, 0, 100);
                market.Demand = Math.Clamp(market.Demand - (bundle.CampaignSupplyStrained ? 2 : 1), 0, 100);
                market.PriceIndex = Math.Clamp(market.PriceIndex + Math.Max(1, marketRiskDelta / 2), 50, 150);
            }

            RouteTradeState[] routes = scope.State.Routes
                .Where(route => route.SettlementId == bundle.SettlementId && route.IsActive)
                .OrderBy(static route => route.RouteId)
                .ToArray();
            HashSet<ClanId> affectedClans = routes
                .Select(static route => route.ClanId)
                .ToHashSet();

            foreach (RouteTradeState route in routes)
            {
                route.Risk = Math.Clamp(route.Risk + routeRiskDelta, 0, 100);
                route.LastMargin = Math.Clamp(route.LastMargin - routeRiskDelta, -20, 30);
            }

            ClanTradeState[] clanTrades = scope.State.Clans
                .Where(trade => trade.PrimarySettlementId == bundle.SettlementId || affectedClans.Contains(trade.ClanId))
                .OrderBy(static trade => trade.ClanId.Value)
                .ToArray();

            foreach (ClanTradeState clanTrade in clanTrades)
            {
                clanTrade.CashReserve = Math.Clamp(clanTrade.CashReserve - cashLoss, 0, 500);
                clanTrade.GrainReserve = Math.Clamp(clanTrade.GrainReserve - grainLoss, 0, 500);
                clanTrade.Debt = Math.Clamp(clanTrade.Debt + debtIncrease, 0, 300);
                clanTrade.CommerceReputation = Math.Clamp(clanTrade.CommerceReputation - reputationLoss, 0, 100);
                clanTrade.LastOutcome = string.Equals(clanTrade.LastOutcome, "Loss", StringComparison.Ordinal)
                    ? "Loss"
                    : "War Strain";
                clanTrade.LastExplanation =
                    $"{clanTrade.LastExplanation} Campaign pressure around {campaign.AnchorSettlementName} strained trade: {campaign.FrontLabel}, {campaign.SupplyStateLabel}, {campaign.LastAftermathSummary}";
            }

            if (market is null && routes.Length == 0 && clanTrades.Length == 0)
            {
                continue;
            }

            scope.RecordDiff(
                $"Campaign aftermath around {campaign.AnchorSettlementName} pushed market risk by {marketRiskDelta}, route risk by {routeRiskDelta}, and trade debt by {debtIncrease}; {campaign.SupplyStateLabel} and {campaign.LastAftermathSummary}",
                bundle.SettlementId.Value.ToString());

            if (routes.Any(static route => route.Risk >= 60) || market?.LocalRisk >= 60)
            {
                scope.Emit("RouteBusinessBlocked", $"Campaign pressure partially blocked trade routes around {campaign.AnchorSettlementName}.", bundle.SettlementId.Value.ToString());
            }

            if (clanTrades.Any(static trade => trade.Debt >= 120 && trade.CashReserve <= 40))
            {
                ClanTradeState defaultingTrade = clanTrades
                    .OrderByDescending(static trade => trade.Debt)
                    .ThenBy(static trade => trade.CashReserve)
                    .First();
                scope.Emit("TradeDebtDefaulted", $"Campaign strain pushed clan {defaultingTrade.ClanId.Value} toward debt default around {campaign.AnchorSettlementName}.", bundle.SettlementId.Value.ToString());
            }
        }
    }

    private static int ComputeOrderPenalty(SettlementDisorderSnapshot disorder)
    {
        int penalty = 0;
        if (disorder.RoutePressure >= 60)
        {
            penalty += 2;
        }
        else if (disorder.RoutePressure >= 35)
        {
            penalty += 1;
        }

        if (disorder.BanditThreat >= 65)
        {
            penalty += 1;
        }

        return penalty;
    }

    private static int ComputeCampaignTradeRiskDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignPressureRaised ? 2 : 0;
        delta += bundle.CampaignSupplyStrained ? 4 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;
        delta += campaign.SupplyState <= 40 ? 1 : 0;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignCashLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int loss = bundle.CampaignMobilized ? 1 : 0;
        loss += bundle.CampaignPressureRaised ? 2 : 0;
        loss += bundle.CampaignSupplyStrained ? 5 : 0;
        loss += bundle.CampaignAftermathRegistered ? 2 : 0;
        loss += Math.Max(0, campaign.MobilizedForceCount - 24) / 20;
        return Math.Max(1, loss);
    }

    private static int ComputeCampaignGrainLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int loss = bundle.CampaignMobilized ? 2 : 0;
        loss += bundle.CampaignPressureRaised ? 2 : 0;
        loss += bundle.CampaignSupplyStrained ? 6 : 0;
        loss += bundle.CampaignAftermathRegistered ? 2 : 0;
        loss += Math.Max(0, 55 - campaign.SupplyState) / 10;
        return Math.Max(1, loss);
    }

    private static int ComputeCampaignDebtIncrease(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int increase = bundle.CampaignPressureRaised ? 1 : 0;
        increase += bundle.CampaignSupplyStrained ? 3 : 0;
        increase += bundle.CampaignAftermathRegistered ? 1 : 0;
        increase += Math.Max(0, campaign.FrontPressure - 60) / 20;
        return Math.Max(1, increase);
    }

    private sealed class TradeQueries : ITradeAndIndustryQueries
    {
        private readonly TradeAndIndustryState _state;

        public TradeQueries(TradeAndIndustryState state)
        {
            _state = state;
        }

        public ClanTradeSnapshot GetRequiredClanTrade(ClanId clanId)
        {
            ClanTradeState trade = _state.Clans.Single(trade => trade.ClanId == clanId);
            return CloneClanTrade(trade);
        }

        public IReadOnlyList<ClanTradeSnapshot> GetClanTrades()
        {
            return _state.Clans
                .OrderBy(static trade => trade.ClanId.Value)
                .Select(CloneClanTrade)
                .ToArray();
        }

        public IReadOnlyList<MarketSnapshot> GetMarkets()
        {
            return _state.Markets
                .OrderBy(static market => market.SettlementId.Value)
                .Select(CloneMarket)
                .ToArray();
        }

        public IReadOnlyList<TradeRouteSnapshot> GetRoutesForClan(ClanId clanId)
        {
            return _state.Routes
                .Where(route => route.ClanId == clanId)
                .OrderBy(static route => route.RouteId)
                .Select(CloneRoute)
                .ToArray();
        }

        private static ClanTradeSnapshot CloneClanTrade(ClanTradeState trade)
        {
            return new ClanTradeSnapshot
            {
                ClanId = trade.ClanId,
                PrimarySettlementId = trade.PrimarySettlementId,
                CashReserve = trade.CashReserve,
                GrainReserve = trade.GrainReserve,
                Debt = trade.Debt,
                CommerceReputation = trade.CommerceReputation,
                ShopCount = trade.ShopCount,
                LastOutcome = trade.LastOutcome,
                LastExplanation = trade.LastExplanation,
            };
        }

        private static MarketSnapshot CloneMarket(SettlementMarketState market)
        {
            return new MarketSnapshot
            {
                SettlementId = market.SettlementId,
                MarketName = market.MarketName,
                PriceIndex = market.PriceIndex,
                Demand = market.Demand,
                LocalRisk = market.LocalRisk,
            };
        }

        private static TradeRouteSnapshot CloneRoute(RouteTradeState route)
        {
            return new TradeRouteSnapshot
            {
                RouteId = route.RouteId,
                ClanId = route.ClanId,
                RouteName = route.RouteName,
                SettlementId = route.SettlementId,
                IsActive = route.IsActive,
                Capacity = route.Capacity,
                Risk = route.Risk,
                LastMargin = route.LastMargin,
            };
        }
    }
}
