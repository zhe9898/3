namespace Zongzu.Contracts;

/// <summary>
/// Canonical event name constants emitted by <c>OrderAndBanditry</c>.
///
/// New events use prefixed style (<c>OrderAndBanditry.EventName</c>) per the
/// Renzong pressure chain contract preflight decision: old unprefixed names
/// remain for compatibility; all new cross-module DomainEvents are prefixed.
/// </summary>
public static class OrderAndBanditryEventNames
{
    // ---- Pre-existing (unprefixed, compatibility) ----

    public const string BanditThreatRaised = "BanditThreatRaised";

    public const string OutlawGroupFormed = "OutlawGroupFormed";

    public const string SuppressionSucceeded = "SuppressionSucceeded";

    public const string RouteUnsafeDueToBanditry = "RouteUnsafeDueToBanditry";

    public const string BlackRoutePressureRaised = "BlackRoutePressureRaised";

    // ---- Renzong pressure chain events (prefixed, new) ----

    public const string DisorderSpike = "OrderAndBanditry.DisorderSpike";

    public const string BanditRecruitmentOpportunity = "OrderAndBanditry.BanditRecruitmentOpportunity";

    public const string BanditHotspotActivated = "OrderAndBanditry.BanditHotspotActivated";

    /// <summary>
    /// Periodic summary event emitted when the settlement disorder level
    /// changes band. Replaces <c>DisorderRiskAdjusted</c> (which was a
    /// state update masquerading as an event).
    /// </summary>
    public const string DisorderLevelChanged = "OrderAndBanditry.DisorderLevelChanged";

    public const string RouteInsecuritySpike = "OrderAndBanditry.RouteInsecuritySpike";
}
