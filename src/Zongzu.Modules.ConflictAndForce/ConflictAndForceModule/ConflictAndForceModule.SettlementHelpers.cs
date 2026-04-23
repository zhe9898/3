using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed partial class ConflictAndForceModule : ModuleRunner<ConflictAndForceState>
{
    private static Dictionary<SettlementId, TradeActivitySnapshot> BuildTradeActivityBySettlement(ITradeAndIndustryQueries? tradeQueries)

    {

        if (tradeQueries is null)

        {

            return new Dictionary<SettlementId, TradeActivitySnapshot>();

        }


        Dictionary<SettlementId, List<ClanTradeRouteSnapshot>> routesBySettlement = new();

        foreach (ClanTradeSnapshot clanTrade in tradeQueries.GetClanTrades().OrderBy(static trade => trade.ClanId.Value))

        {

            foreach (ClanTradeRouteSnapshot route in tradeQueries.GetRoutesForClan(clanTrade.ClanId)

                         .Where(static route => route.IsActive)

                         .OrderBy(static route => route.RouteId))

            {

                if (!routesBySettlement.TryGetValue(route.SettlementId, out List<ClanTradeRouteSnapshot>? routes))

                {

                    routes = [];

                    routesBySettlement[route.SettlementId] = routes;

                }


                routes.Add(route);

            }

        }


        return routesBySettlement.ToDictionary(

            static pair => pair.Key,

            static pair =>

            {

                int averageRouteRisk = pair.Value.Count == 0 ? 0 : pair.Value.Sum(static route => route.Risk) / pair.Value.Count;

                int totalRouteCapacity = pair.Value.Sum(static route => route.Capacity);

                return new TradeActivitySnapshot(pair.Value.Count, averageRouteRisk, totalRouteCapacity);

            });

    }


    private static int AverageClanValue(IReadOnlyList<ClanSnapshot> clans, Func<ClanSnapshot, int> selector)

    {

        return clans.Count == 0 ? 0 : clans.Sum(selector) / clans.Count;

    }


    private static int AverageNarrative(

        IReadOnlyList<ClanSnapshot> clans,

        IReadOnlyDictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan,

        Func<ClanNarrativeSnapshot, int> selector)

    {

        int total = 0;

        int count = 0;

        foreach (ClanSnapshot clan in clans)

        {

            if (!narrativesByClan.TryGetValue(clan.Id, out ClanNarrativeSnapshot? narrative))

            {

                continue;

            }


            total += selector(narrative);

            count += 1;

        }


        return count == 0 ? 0 : total / count;

    }


    private static SettlementForceState GetOrCreateSettlement(ConflictAndForceState state, SettlementId settlementId)

    {

        SettlementForceState? settlement = state.Settlements.SingleOrDefault(existing => existing.SettlementId == settlementId);

        if (settlement is not null)

        {

            return settlement;

        }


        settlement = new SettlementForceState

        {

            SettlementId = settlementId,

        };

        state.Settlements.Add(settlement);

        state.Settlements = state.Settlements.OrderBy(static entry => entry.SettlementId.Value).ToList();

        return settlement;

    }


}
