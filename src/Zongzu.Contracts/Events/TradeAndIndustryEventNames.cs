namespace Zongzu.Contracts;

/// <summary>
/// Canonical event name constants emitted by <c>TradeAndIndustry</c>.
///
/// New events use prefixed style (<c>TradeAndIndustry.EventName</c>) per the
/// Renzong pressure chain contract preflight decision: old unprefixed names
/// remain for compatibility; all new cross-module DomainEvents are prefixed.
/// </summary>
public static class TradeAndIndustryEventNames
{
    // ---- Pre-existing (unprefixed, compatibility) ----

    public const string TradeProspered = "TradeProspered";

    public const string TradeLossOccurred = "TradeLossOccurred";

    public const string TradeDebtDefaulted = "TradeDebtDefaulted";

    public const string RouteBusinessBlocked = "RouteBusinessBlocked";

    // ---- Renzong pressure chain events (prefixed, new) ----

    public const string GrainPriceSpike = "TradeAndIndustry.GrainPriceSpike";

    public const string GrainPriceSpikeRisk = "TradeAndIndustry.GrainPriceSpikeRisk";

    public const string MarketPanicRisk = "TradeAndIndustry.MarketPanicRisk";

    public const string OfficialPurchasingPressure = "TradeAndIndustry.OfficialPurchasingPressure";

    public const string MarketDiversion = "TradeAndIndustry.MarketDiversion";

    public const string GrainRouteBlocked = "TradeAndIndustry.GrainRouteBlocked";

    public const string GrainRouteReopened = "TradeAndIndustry.GrainRouteReopened";
}
