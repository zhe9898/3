using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

/// <summary>
/// Phase 5 商贸骨骼 —— 粮食供需价位薄链（<c>LIVING_WORLD_DESIGN §2.5</c>）。
/// 收成与户口粮储 → 某镇粮食供给（<see cref="MarketGoodsEntryState.Supply"/>）；
/// 人口丁口与困蹙 → 粮食需求（<see cref="MarketGoodsEntryState.Demand"/>）；
/// 供需差 → 基准价附近的月度粮价（<see cref="MarketGoodsEntryState.CurrentPrice"/>）。
/// </summary>
public sealed partial class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
{
    private static void ApplyMonthlyGrainPulse(
        TradeAndIndustryState state,
        IPopulationAndHouseholdsQueries populationQueries,
        IWorldSettlementsQueries settlementQueries)
    {
        IReadOnlyList<HouseholdPressureSnapshot> households = populationQueries.GetHouseholds();
        Dictionary<SettlementId, int> grainBySettlement = households
            .GroupBy(static household => household.SettlementId)
            .ToDictionary(
                static group => group.Key,
                static group => group.Sum(static household => household.GrainStore));
        Dictionary<SettlementId, int> householdCountBySettlement = households
            .GroupBy(static household => household.SettlementId)
            .ToDictionary(
                static group => group.Key,
                static group => group.Count());

        foreach (SettlementMarketState market in state.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            MarketGoodsEntryState entry = GetOrCreateGrainEntry(state, market.SettlementId);

            int grainStore = grainBySettlement.TryGetValue(market.SettlementId, out int stored) ? stored : 0;
            int householdCount = householdCountBySettlement.TryGetValue(market.SettlementId, out int count) ? count : 0;
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(market.SettlementId);

            // Supply：以户均粮储为核心（每户标线 50），再叠加本镇生产力微调。
            int supplyBaseline = householdCount == 0 ? 50 : Math.Clamp(grainStore / Math.Max(1, householdCount), 0, 100);
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(market.SettlementId);
            int supplyDelta = (settlement.Prosperity >= 60 ? 2 : settlement.Prosperity < 45 ? -2 : 0);
            entry.Supply = Math.Clamp(supplyBaseline + supplyDelta, 0, 100);

            // Demand：丁口越多、困蹙越重，粮食需求越紧。
            int demandBaseline = Math.Clamp(50 + (population.CommonerDistress - 50) / 2 + (population.LaborSupply - 80) / 4, 0, 100);
            entry.Demand = demandBaseline;

            // CurrentPrice：基准价上浮动；供给不足抬价、需求疲软压价。
            int pressure = entry.Demand - entry.Supply;
            entry.BasePrice = entry.BasePrice == 0 ? 100 : entry.BasePrice;
            entry.CurrentPrice = Math.Clamp(entry.BasePrice + (pressure / 2), 50, 200);
        }
    }

    private static MarketGoodsEntryState GetOrCreateGrainEntry(TradeAndIndustryState state, SettlementId settlementId)
    {
        MarketGoodsEntryState? existing = state.MarketGoods
            .FirstOrDefault(entry => entry.SettlementId == settlementId && entry.Goods == GoodsCategory.Grain);
        if (existing is not null)
        {
            return existing;
        }

        MarketGoodsEntryState created = new()
        {
            SettlementId = settlementId,
            Goods = GoodsCategory.Grain,
            Supply = 50,
            Demand = 50,
            BasePrice = 100,
            CurrentPrice = 100,
        };
        state.MarketGoods.Add(created);
        state.MarketGoods = state.MarketGoods
            .OrderBy(static entry => entry.SettlementId.Value)
            .ThenBy(static entry => (int)entry.Goods)
            .ToList();
        return created;
    }
}
