namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC decision I / §3.3 — regional ecology changes the
/// <i>coefficients</i> of season-band advancement, not the shape of the bands.
///
/// Lanxi seed is <see cref="JiangnanWaterNetwork"/>. The other zones are
/// registered so that <c>SettlementStateData</c> can serialize / round-trip
/// them; full world seeds for non-Jiangnan zones arrive in later phases.
/// </summary>
public enum SettlementEcoZone
{
    Unknown = 0,

    /// <summary>Canals, dikes, ferries, paddy. Jiangnan water-network county.</summary>
    JiangnanWaterNetwork = 1,

    /// <summary>Dry roads, wells, cart corridors, winter freeze. North-China road county.</summary>
    NorthChinaDryRoad = 2,

    /// <summary>Ridge trails, valley markets, hybrid shrines. Southwest mountain borderland.</summary>
    MountainBorderland = 3,

    /// <summary>Fort lines, relay posts, watchtowers. Border garrison belt.</summary>
    BorderGarrisonBelt = 4,
}
