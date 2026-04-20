using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// Administrative level of a settlement. <b>Orthogonal to</b>
/// <see cref="SettlementNodeKind"/>: Tier is the settlement's place in the
/// state's administrative hierarchy; NodeKind is its functional-semantic
/// role in the living world. A large <c>MarketTown</c> NodeKind can sit at
/// a county-adjacent Tier. See SPATIAL_SKELETON_SPEC §1.3.
/// </summary>
public enum SettlementTier
{
    Unknown = 0,
    VillageCluster = 1,
    MarketTown = 2,
    CountySeat = 3,
    PrefectureSeat = 4,
}

/// <summary>
/// Read-only projection of a settlement node from <c>WorldSettlementsState</c>.
///
/// <para>Phase 1c (SPATIAL_SKELETON_SPEC §1): <see cref="NodeKind"/>,
/// <see cref="Visibility"/>, and <see cref="EcoZone"/> are added to the
/// snapshot so consumers can filter by functional semantics (decision A),
/// covertness (decision H), and regional ecology (decision I) without
/// reaching into state.</para>
/// </summary>
public sealed record SettlementSnapshot
{
    public SettlementId Id { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>Administrative level (existing field).</summary>
    public SettlementTier Tier { get; init; }

    /// <summary>
    /// Functional-semantic classification (decision A) — "this is the
    /// village's lineage hall" / "this is a ferry" / "this is a covert
    /// meeting point". Orthogonal to <see cref="Tier"/>.
    /// </summary>
    public SettlementNodeKind NodeKind { get; init; }

    /// <summary>
    /// State-visible / local-known / covert (decision H). Instance attribute,
    /// not a NodeKind attribute — the same <see cref="SettlementNodeKind.LineageHall"/>
    /// may be state-visible in one county and local-known in another.
    /// </summary>
    public NodeVisibility Visibility { get; init; }

    /// <summary>
    /// Regional ecology (decision I). Drives season-band advancement
    /// coefficients but not field shape. Lanxi seed = Jiangnan water network.
    /// </summary>
    public SettlementEcoZone EcoZone { get; init; }

    public int Security { get; init; }

    public int Prosperity { get; init; }
}

/// <summary>
/// Query surface for <c>WorldSettlements</c>. SPATIAL_SKELETON_SPEC §8
/// extends the original two-method surface with route queries, node-kind /
/// visibility filters, season-band read, and the desk-sandbox current locus.
///
/// <para>All returned snapshots are detached values — callers must not
/// assume any identity / reference semantics across calls.</para>
/// </summary>
public interface IWorldSettlementsQueries
{
    SettlementSnapshot GetRequiredSettlement(SettlementId settlementId);

    IReadOnlyList<SettlementSnapshot> GetSettlements();

    /// <summary>Filter settlements by functional node kind (decision A).</summary>
    IReadOnlyList<SettlementSnapshot> GetSettlementsByNodeKind(SettlementNodeKind kind);

    /// <summary>Filter settlements by visibility (decision H). Use <see cref="NodeVisibility.Covert"/> to surface covert nodes.</summary>
    IReadOnlyList<SettlementSnapshot> GetSettlementsByVisibility(NodeVisibility visibility);

    /// <summary>All routes in the world (excludes decommissioned if any).</summary>
    IReadOnlyList<RouteSnapshot> GetRoutes();

    /// <summary>Filter routes by social function (decision B).</summary>
    IReadOnlyList<RouteSnapshot> GetRoutesByKind(RouteKind kind);

    /// <summary>Filter routes by legitimacy (decision H). Use <see cref="RouteLegitimacy.Illicit"/> for banned corridors.</summary>
    IReadOnlyList<RouteSnapshot> GetRoutesByLegitimacy(RouteLegitimacy legitimacy);

    /// <summary>All routes that touch a given settlement — as origin, destination, or waypoint.</summary>
    IReadOnlyList<RouteSnapshot> GetRoutesTouching(SettlementId settlementId);

    /// <summary>Current season-band read (SPEC §3.1).</summary>
    SeasonBandSnapshot GetCurrentSeason();

    /// <summary>
    /// Desk-sandbox current locus (SPEC §6.4 decision J). The single node /
    /// route most deserving of player attention this tick, scored
    /// deterministically from season, routes, and settlements. Returns
    /// <c>null</c> only before the first tick has run.
    /// </summary>
    LocusSnapshot? GetCurrentLocus();

    /// <summary>
    /// SPATIAL_SKELETON_SPEC §20.3 — public-surface signals derived during
    /// the most recent xun or month tick. Lifetime is one tick; consumers
    /// must read within the same tick they care about and must not cache.
    /// <c>NarrativeProjection</c> is the canonical consumer, composing these
    /// into the notice tray with stream-column layout.
    /// </summary>
    IReadOnlyList<PublicSurfaceSignal> GetCurrentPulseSignals();
}

