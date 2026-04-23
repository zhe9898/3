using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed class TradeAndIndustryState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.TradeAndIndustry;

    public List<ClanTradeState> Clans { get; set; } = new();

    public List<SettlementMarketState> Markets { get; set; } = new();

    public List<MarketGoodsEntryState> MarketGoods { get; set; } = new();

    public List<RouteTradeState> Routes { get; set; } = new();

    public List<SettlementBlackRouteLedgerState> BlackRouteLedgers { get; set; } = new();
}

public sealed class ClanTradeState
{
    public ClanId ClanId { get; set; }

    public SettlementId PrimarySettlementId { get; set; }

    public int CashReserve { get; set; }

    public int GrainReserve { get; set; }

    public int Debt { get; set; }

    public int CommerceReputation { get; set; }

    public int ShopCount { get; set; }

    public int ManagerSkill { get; set; }

    public string LastOutcome { get; set; } = "Stable";

    public string LastExplanation { get; set; } = string.Empty;
}

public sealed class SettlementMarketState
{
    public SettlementId SettlementId { get; set; }

    public string MarketName { get; set; } = string.Empty;

    public int PriceIndex { get; set; }

    public int Demand { get; set; }

    public int LocalRisk { get; set; }
}

public sealed class RouteTradeState
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

    public GoodsCategory PrimaryGoods { get; set; } = GoodsCategory.Grain;

    public int Throughput { get; set; }

    public int RiskPremium { get; set; }
}

/// <summary>
/// Phase 5 商贸骨骼 — 某镇场上某类物品的供需价位。
/// 当前仅 <see cref="GoodsCategory.Grain"/> 由收成链路真实驱动。
/// </summary>
public sealed class MarketGoodsEntryState
{
    public SettlementId SettlementId { get; set; }

    public GoodsCategory Goods { get; set; } = GoodsCategory.Unknown;

    public int Supply { get; set; }

    public int Demand { get; set; }

    public int BasePrice { get; set; } = 100;

    public int CurrentPrice { get; set; } = 100;
}

public sealed class SettlementBlackRouteLedgerState
{
    public SettlementId SettlementId { get; set; }

    public int ShadowPriceIndex { get; set; }

    public int DiversionShare { get; set; }

    public int IllicitMargin { get; set; }

    public int BlockedShipmentCount { get; set; }

    public int SeizureRisk { get; set; }

    public string DiversionBandLabel { get; set; } = string.Empty;

    public string LastLedgerTrace { get; set; } = string.Empty;
}
