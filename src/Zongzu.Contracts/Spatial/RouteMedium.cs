namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §2.2 decision C — water and land are parallel
/// topologies. Each <c>RouteState</c> belongs to exactly one medium, but a
/// land road crossing water must list the interface node (Ferry / Bridge /
/// Ford / CanalJunction) in its waypoints.
///
/// Grey-zone routes (<see cref="RouteKind.SmugglingCorridor"/> / <see cref="RouteKind.FugitivePath"/>)
/// may skip the interface check — see SPEC §2.6.
/// </summary>
public enum RouteMedium
{
    Unknown = 0,

    LandRoad = 1,
    WaterRiver = 2,
    WaterCanal = 3,
    MountainPath = 4,
    FerryLink = 5,
    BridgeCrossing = 6,
    PassApproach = 7,

    /// <summary>North-China cart-friction corridor (heavier wheels, winter freeze).</summary>
    CartCorridor = 8,

    /// <summary>Mountain ridge route — exposed, slow, season-sensitive.</summary>
    RidgeTrail = 9,
}
