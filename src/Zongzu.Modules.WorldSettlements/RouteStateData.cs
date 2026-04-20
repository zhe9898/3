using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC §2.5 — authoritative, mutable state for one social-
/// function route. Owned by <c>WorldSettlements</c>; other modules read only
/// through <see cref="RouteSnapshot"/> via <see cref="IWorldSettlementsQueries"/>.
///
/// <para>Mirrors <see cref="RouteSnapshot"/> field-for-field. State is a
/// mutable class (tick-driven); the snapshot is an immutable <c>record</c>
/// (decoupled value).</para>
///
/// <para>Not stored here (decision E, SPEC §2.5):
/// <list type="bullet">
///   <item>Derived overlays (TaxReach, GrainMovement, BanditRisk) — query-time
///         composition from multiple modules.</item>
///   <item>Real-time traffic volume — held by <c>TradeAndIndustry</c> /
///         <c>PopulationAndHouseholds</c> in their own state.</item>
/// </list></para>
/// </summary>
public sealed class RouteStateData
{
    public RouteId Id { get; set; }

    public RouteKind Kind { get; set; }

    public RouteMedium Medium { get; set; }

    public RouteLegitimacy Legitimacy { get; set; }

    /// <summary>Meaningful only for down-flow <see cref="RouteKind"/>s; others ignore.</summary>
    public ComplianceMode ComplianceMode { get; set; }

    public SettlementId Origin { get; set; }

    public SettlementId Destination { get; set; }

    /// <summary>Ordered intermediate node path. Must include a water-land interface node when crossing water (SPEC §2.6; gray-zone routes exempt).</summary>
    public List<SettlementId> Waypoints { get; set; } = new();

    /// <summary>0..5 band from <see cref="CalibrationBands"/> <c>TravelDays_*</c>.</summary>
    public int TravelDaysBand { get; set; }

    /// <summary>0..100 — throughput capacity.</summary>
    public int Capacity { get; set; }

    /// <summary>0..100 — reliability (silting, slides, raids, delays reduce it).</summary>
    public int Reliability { get; set; }

    /// <summary>0..100 — sensitivity to seasonal hazards.</summary>
    public int SeasonalVulnerability { get; set; }

    /// <summary>Translation key for current blocking reason; empty when clear.</summary>
    public string CurrentConstraintLabel { get; set; } = string.Empty;
}
