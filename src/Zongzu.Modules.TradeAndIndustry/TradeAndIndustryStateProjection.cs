using System;
using System.Collections.Generic;
using Zongzu.Contracts;

namespace Zongzu.Modules.TradeAndIndustry;

/// <summary>
/// Phase 5 商贸骨骼 schema migration（<c>LIVING_WORLD_DESIGN §2.5</c>）。
/// v3 → v4：初始化 <see cref="TradeAndIndustryState.MarketGoods"/> 容器；
/// 为每个市场补建一条 <see cref="GoodsCategory.Grain"/> 粮价条目；
/// 给既有路线填上 <see cref="RouteTradeState.PrimaryGoods"/>/<see cref="RouteTradeState.Throughput"/>/<see cref="RouteTradeState.RiskPremium"/> 的合理默认。
/// </summary>
public static class TradeAndIndustryStateProjection
{
    public static void UpgradeFromSchemaV3ToV4(TradeAndIndustryState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.MarketGoods ??= new List<MarketGoodsEntryState>();

        foreach (SettlementMarketState market in state.Markets)
        {
            bool hasGrainEntry = false;
            foreach (MarketGoodsEntryState entry in state.MarketGoods)
            {
                if (entry.SettlementId == market.SettlementId && entry.Goods == GoodsCategory.Grain)
                {
                    hasGrainEntry = true;
                    break;
                }
            }

            if (!hasGrainEntry)
            {
                state.MarketGoods.Add(new MarketGoodsEntryState
                {
                    SettlementId = market.SettlementId,
                    Goods = GoodsCategory.Grain,
                    Supply = 50,
                    Demand = 50,
                    BasePrice = 100,
                    CurrentPrice = market.PriceIndex == 0 ? 100 : market.PriceIndex,
                });
            }
        }

        foreach (RouteTradeState route in state.Routes)
        {
            if (route.PrimaryGoods == GoodsCategory.Unknown)
            {
                route.PrimaryGoods = GoodsCategory.Grain;
            }

            if (route.Throughput == 0)
            {
                route.Throughput = Math.Max(0, route.Capacity);
            }

            if (route.RiskPremium == 0)
            {
                route.RiskPremium = Math.Clamp(route.Risk / 4, 0, 25);
            }
        }
    }
}
