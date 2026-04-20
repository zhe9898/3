using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record ClanTradeSnapshot
{
    public ClanId ClanId { get; init; }

    public SettlementId PrimarySettlementId { get; init; }

    public int CashReserve { get; init; }

    public int GrainReserve { get; init; }

    public int Debt { get; init; }

    public int CommerceReputation { get; init; }

    public int ShopCount { get; init; }

    public string LastOutcome { get; init; } = string.Empty;

    public string LastExplanation { get; init; } = string.Empty;
}

public sealed record MarketSnapshot
{
    public SettlementId SettlementId { get; init; }

    public string MarketName { get; init; } = string.Empty;

    public int PriceIndex { get; init; }

    public int Demand { get; init; }

    public int LocalRisk { get; init; }
}

public sealed record ClanTradeRouteSnapshot
{
    public int RouteId { get; init; }

    public ClanId ClanId { get; init; }

    public string RouteName { get; init; } = string.Empty;

    public SettlementId SettlementId { get; init; }

    public bool IsActive { get; init; }

    public int Capacity { get; init; }

    public int Risk { get; init; }

    public int LastMargin { get; init; }

    public int BlockedShipmentCount { get; init; }

    public int SeizureRisk { get; init; }

    public string RouteConstraintLabel { get; init; } = string.Empty;

    public string LastRouteTrace { get; init; } = string.Empty;
}

public interface ITradeAndIndustryQueries
{
    ClanTradeSnapshot GetRequiredClanTrade(ClanId clanId);

    IReadOnlyList<ClanTradeSnapshot> GetClanTrades();

    IReadOnlyList<MarketSnapshot> GetMarkets();

    IReadOnlyList<ClanTradeRouteSnapshot> GetRoutesForClan(ClanId clanId);

    /// <summary>Phase 5 商贸骨骼：某镇按物品的供需价位。默认空集以兼容旧 stub。</summary>
    IReadOnlyList<MarketGoodsSnapshot> GetMarketGoods() => [];

    /// <summary>Phase 5 商贸骨骼：按镇聚焦某物品。默认空集以兼容旧 stub。</summary>
    IReadOnlyList<MarketGoodsSnapshot> GetMarketGoodsAt(SettlementId settlementId) => [];
}
