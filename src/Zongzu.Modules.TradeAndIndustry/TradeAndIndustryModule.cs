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

    public override string ModuleKey => KnownModuleKeys.TradeAndIndustry;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 600;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

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

        Dictionary<SettlementId, SettlementMarketState> markets = scope.State.Markets
            .OrderBy(static market => market.SettlementId.Value)
            .ToDictionary(static market => market.SettlementId, static market => market);

        foreach (SettlementMarketState market in scope.State.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(market.SettlementId);
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(market.SettlementId);

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
                + (population.MigrationPressure >= 60 ? 1 : 0),
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
            foreach (RouteTradeState route in routes)
            {
                route.Risk = Math.Clamp(route.Risk + (market.LocalRisk >= 55 ? 1 : -1) + (narrative.GrudgePressure >= 60 ? 1 : 0), 0, 100);
                route.LastMargin = Math.Clamp((route.Capacity / 8) - (route.Risk / 10) + scope.Context.Random.NextInt(-2, 3), -20, 30);
                routeFactor += route.LastMargin;
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
                $"Margin {margin} from demand {market.Demand}, price index {market.PriceIndex}, labor {population.LaborSupply}, route factor {routeFactor}, and grudge penalty {grudgePenalty}.";

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

            if (routes.Any(static route => route.Risk >= 70))
            {
                scope.Emit("RouteBusinessBlocked", $"A trade route for clan {clan.ClanName} was partially blocked.");
            }
        }
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
