using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class ClanTradeSnapshot
{
    public ClanId ClanId { get; set; }

    public SettlementId PrimarySettlementId { get; set; }

    public int CashReserve { get; set; }

    public int GrainReserve { get; set; }

    public int Debt { get; set; }

    public int CommerceReputation { get; set; }

    public int ShopCount { get; set; }

    public string LastOutcome { get; set; } = string.Empty;

    public string LastExplanation { get; set; } = string.Empty;
}

public sealed class MarketSnapshot
{
    public SettlementId SettlementId { get; set; }

    public string MarketName { get; set; } = string.Empty;

    public int PriceIndex { get; set; }

    public int Demand { get; set; }

    public int LocalRisk { get; set; }
}

public sealed class TradeRouteSnapshot
{
    public int RouteId { get; set; }

    public ClanId ClanId { get; set; }

    public string RouteName { get; set; } = string.Empty;

    public SettlementId SettlementId { get; set; }

    public bool IsActive { get; set; }

    public int Capacity { get; set; }

    public int Risk { get; set; }

    public int LastMargin { get; set; }

    public int BlockedShipmentCount { get; set; }

    public int SeizureRisk { get; set; }

    public string RouteConstraintLabel { get; set; } = string.Empty;

    public string LastRouteTrace { get; set; } = string.Empty;
}

public interface ITradeAndIndustryQueries
{
    ClanTradeSnapshot GetRequiredClanTrade(ClanId clanId);

    IReadOnlyList<ClanTradeSnapshot> GetClanTrades();

    IReadOnlyList<MarketSnapshot> GetMarkets();

    IReadOnlyList<TradeRouteSnapshot> GetRoutesForClan(ClanId clanId);
}
