using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §19 — one contributed pressure reading on a node or
/// a route. Multiple modules may contribute separate readings of the same
/// <see cref="PressureKind"/> on the same entity; <see cref="IPressureFlowQueries"/>
/// returns them as a list, callers sum / max / pick-top as needed.
/// </summary>
/// <param name="Kind">Which of the eight canonical pressures this reading represents.</param>
/// <param name="Intensity">0..100 — this reading's contribution.</param>
/// <param name="SourceModuleKey">
/// Which module contributed this reading (e.g. <c>KnownModuleKeys.OrderAndBanditry</c>).
/// Enables diagnostics / causal trace without coupling consumers to source modules.
/// </param>
/// <param name="SourceEntityKey">
/// Optional per-module identifier for the entity that originated the pressure
/// (a bandit gang id, a household id, etc.). <c>null</c> when the reading is
/// an aggregate with no single owner.
/// </param>
public sealed record PressureReading(
    PressureKind Kind,
    int Intensity,
    string SourceModuleKey,
    string? SourceEntityKey);

/// <summary>
/// SPATIAL_SKELETON_SPEC §19.2 — whole-map pressure snapshot, fed into desk
/// sandbox overlays and the causal-chain projection.
///
/// <para>This is a plain aggregate: per-node and per-route readings, grouped
/// for fast overlay rendering. It is <b>not</b> an authoritative store;
/// state lives in the contributing modules.</para>
///
/// <para>Phase 1c: contract only — implementation of <see cref="IPressureFlowQueries"/>
/// lands in Phase 1c+1. Phase 1c must keep <c>RouteState</c> fields
/// (Reliability / SeasonalVulnerability / Capacity) in shape so the future
/// attenuation formula in SPEC §19.3 can run.</para>
/// </summary>
public sealed record PressureMapSnapshot
{
    /// <summary>When the map was captured.</summary>
    public GameDate AsOf { get; init; }

    /// <summary>Per-node readings. Key: node id. Value: contributed readings in deterministic order.</summary>
    public IReadOnlyDictionary<SettlementId, IReadOnlyList<PressureReading>> NodePressures { get; init; }
        = new Dictionary<SettlementId, IReadOnlyList<PressureReading>>();

    /// <summary>Per-route readings. Key: route id. Value: contributed readings in deterministic order.</summary>
    public IReadOnlyDictionary<RouteId, IReadOnlyList<PressureReading>> RoutePressures { get; init; }
        = new Dictionary<RouteId, IReadOnlyList<PressureReading>>();
}

/// <summary>
/// SPATIAL_SKELETON_SPEC §19.2 — query surface for the pressure-flow pipeline.
///
/// <para><b>Implemented in Phase 1c+1</b>, not Phase 1c. Phase 1c only
/// registers the signature so consuming modules can compile against it.</para>
///
/// <para>Pressure data is <b>query-side aggregation</b>, not stored on
/// <c>WorldSettlementsState</c> (decision E). Each pressure source module
/// contributes readings through a registration seam; the attenuation rule
/// (SPEC §19.3) lives in the implementing projector.</para>
/// </summary>
public interface IPressureFlowQueries
{
    /// <summary>All contributed readings currently sitting on a node.</summary>
    IReadOnlyList<PressureReading> GetPressureAtNode(SettlementId node);

    /// <summary>All readings currently transmitted along a route (post-attenuation).</summary>
    IReadOnlyList<PressureReading> GetPressureOnRoute(RouteId route);

    /// <summary>Whole-map snapshot for overlay / diagnostics / causal trace.</summary>
    PressureMapSnapshot GetPressureMap();
}
