using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed partial class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
{
    private sealed class TradeQueries : ITradeAndIndustryQueries, IBlackRouteLedgerQueries

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


        public IReadOnlyList<ClanTradeRouteSnapshot> GetRoutesForClan(ClanId clanId)

        {

            return _state.Routes

                .Where(route => route.ClanId == clanId)

                .OrderBy(static route => route.RouteId)

                .Select(CloneRoute)

                .ToArray();

        }


        public SettlementBlackRouteLedgerSnapshot GetRequiredSettlementBlackRouteLedger(SettlementId settlementId)

        {

            SettlementBlackRouteLedgerState ledger = _state.BlackRouteLedgers.Single(ledger => ledger.SettlementId == settlementId);

            return CloneLedger(ledger);

        }


        public IReadOnlyList<SettlementBlackRouteLedgerSnapshot> GetSettlementBlackRouteLedgers()

        {

            return _state.BlackRouteLedgers

                .OrderBy(static ledger => ledger.SettlementId.Value)

                .Select(CloneLedger)

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


        private static ClanTradeRouteSnapshot CloneRoute(RouteTradeState route)

        {

            return new ClanTradeRouteSnapshot

            {

                RouteId = route.RouteId,

                ClanId = route.ClanId,

                RouteName = route.RouteName,

                SettlementId = route.SettlementId,

                IsActive = route.IsActive,

                Capacity = route.Capacity,

                Risk = route.Risk,

                LastMargin = route.LastMargin,

                BlockedShipmentCount = route.BlockedShipmentCount,

                SeizureRisk = route.SeizureRisk,

                RouteConstraintLabel = route.RouteConstraintLabel,

                LastRouteTrace = route.LastRouteTrace,

            };

        }


        private static SettlementBlackRouteLedgerSnapshot CloneLedger(SettlementBlackRouteLedgerState ledger)

        {

            return new SettlementBlackRouteLedgerSnapshot

            {

                SettlementId = ledger.SettlementId,

                ShadowPriceIndex = ledger.ShadowPriceIndex,

                DiversionShare = ledger.DiversionShare,

                IllicitMargin = ledger.IllicitMargin,

                BlockedShipmentCount = ledger.BlockedShipmentCount,

                SeizureRisk = ledger.SeizureRisk,

                DiversionBandLabel = ledger.DiversionBandLabel,

                LastLedgerTrace = ledger.LastLedgerTrace,

            };

        }

    }


    private sealed record TradeActivitySnapshot(int ActiveRouteCount, int AverageRouteRisk, int TotalRouteCapacity)

    {

        public static readonly TradeActivitySnapshot Empty = new(0, 0, 0);

    }


    private readonly record struct OrderInterventionCarryoverEffect(

        int LocalRiskDelta,

        int DiversionDelta,

        int BlockedShipmentDelta,

        int SeizureRiskDelta);
}
