namespace Zongzu.Contracts;

/// <summary>
/// Phase 8 匪患骨骼 — <c>LIVING_WORLD_DESIGN §2.8</c>。
/// 群盗成形的五级聚合度：
/// Scattered 散匪 / Roaming 流寇 / RouteHolding 据路 / TerritoryHolding 据地 / RebelGovernance 僭号。
/// </summary>
public enum BandConcentration
{
    Unknown = 0,
    Scattered = 1,
    Roaming = 2,
    RouteHolding = 3,
    TerritoryHolding = 4,
    RebelGovernance = 5,
}

public static class OrderInterventionOutcomeCodes
{
    public const string Accepted = "accepted";
    public const string Partial = "partial";
    public const string Refused = "refused";
}

public static class OrderInterventionRefusalCodes
{
    public const string MissingSettlement = "missing_settlement";
    public const string UnknownCommand = "unknown_command";
    public const string WatchmenRefused = "watchmen_refused";
    public const string SuppressionRefused = "suppression_refused";
}

public static class OrderInterventionPartialCodes
{
    public const string CountyDrag = "county_drag";
    public const string WatchMisread = "watch_misread";
    public const string SuppressionBacklash = "suppression_backlash";
}

public static class OrderInterventionTraceCodes
{
    public const string AcceptedFollowThrough = "accepted_follow_through";
    public const string WatchCountyDrag = "watch_county_drag";
    public const string WatchGroundRefusal = "watch_ground_refusal";
    public const string SuppressionCountyDrag = "suppression_county_drag";
    public const string SuppressionBacklash = "suppression_backlash";
    public const string SuppressionGroundRefusal = "suppression_ground_refusal";
    public const string MissingSettlement = "missing_settlement";
    public const string UnknownCommand = "unknown_command";
}
