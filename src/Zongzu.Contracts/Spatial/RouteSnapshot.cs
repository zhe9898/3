using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §8 — read-only projection of a
/// <c>RouteState</c> owned by <c>WorldSettlements</c>.
///
/// <para>One-to-one field mirror of SPEC §2.5 <c>RouteState</c>. Other
/// modules read routes through this snapshot; they never hold a reference
/// to the module's internal state (ENGINEERING_RULES §4, §5).</para>
///
/// <para><b>Not to be confused with</b> <see cref="ClanTradeRouteSnapshot"/>:
/// that one is a clan's trade operation on a corridor (capacity, risk,
/// clan owner); this one is the corridor itself as a piece of the living
/// spatial skeleton (kind, medium, legitimacy, compliance, waypoints).
/// The same physical corridor can host multiple <see cref="RouteSnapshot"/>
/// entries (grain route + market route + exam-travel route) and any number
/// of <see cref="ClanTradeRouteSnapshot"/> entries (different clans operating
/// along it).</para>
///
/// <para>Derived layers (TaxReach, GrainMovement, BanditRisk, ...) are
/// <b>not</b> fields here — SPEC §2.5 decision E keeps them as query-time
/// overlays computed from multiple modules.</para>
/// </summary>
public sealed record RouteSnapshot
{
    /// <summary>Stable route identity — never recycled within a save.</summary>
    public RouteId Id { get; init; }

    /// <summary>Social function — grain / tax / market / smuggling / etc.</summary>
    public RouteKind Kind { get; init; }

    /// <summary>Physical medium — land road / canal / ridge trail / ferry link / etc.</summary>
    public RouteMedium Medium { get; init; }

    /// <summary>Official / Tolerated / GrayZone / Illicit (decision H).</summary>
    public RouteLegitimacy Legitimacy { get; init; }

    /// <summary>
    /// Delivery-quality of top-down dispatch (decision I). Meaningful only
    /// for down-flow kinds (OfficialDispatch / Tax / Corvee / MilitaryMove);
    /// other kinds carry but ignore it.
    /// </summary>
    public ComplianceMode ComplianceMode { get; init; }

    public SettlementId Origin { get; init; }

    public SettlementId Destination { get; init; }

    /// <summary>
    /// Ordered node path (excludes Origin and Destination). For
    /// LandRoad/WaterRiver/CartCorridor routes crossing water, must include
    /// the interface node (Ferry / Bridge / Ford / CanalJunction) unless
    /// <see cref="Legitimacy"/> &gt;= GrayZone — see SPEC §2.6.
    /// </summary>
    public IReadOnlyList<SettlementId> Waypoints { get; init; } = [];

    /// <summary>
    /// Travel-time band (0..5, see <see cref="CalibrationBands"/>
    /// <c>TravelDays_*</c>). Named band, not a day count.
    /// </summary>
    public int TravelDaysBand { get; init; }

    /// <summary>0..100 — throughput / carrying capacity.</summary>
    public int Capacity { get; init; }

    /// <summary>0..100 — reliability (low = silted, slipped, raided, delayed).</summary>
    public int Reliability { get; init; }

    /// <summary>0..100 — sensitivity to season. High = flood / snow / drought breaks it early.</summary>
    public int SeasonalVulnerability { get; init; }

    /// <summary>
    /// Current blocking reason translation key — empty when clear. Shell
    /// renders prose; authority only stores the key
    /// (ENGINEERING_RULES §10: "shell prose belongs downstream").
    /// </summary>
    public string CurrentConstraintLabel { get; init; } = string.Empty;
}
