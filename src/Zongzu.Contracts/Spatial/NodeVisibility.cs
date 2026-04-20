namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC decision H — visibility is a first-class attribute.
/// The state map does not assume every node is state-visible.
///
/// This is an <b>instance</b> attribute, not a <see cref="SettlementNodeKind"/>
/// attribute. A <c>LineageHall</c> may be <c>LocalKnown</c> in a strong-clan
/// county and <c>StateVisible</c> in a new-migrant county. Only
/// <c>CovertMeetPoint</c> and <c>SmugglingCache</c> are <b>definitionally</b>
/// covert.
/// </summary>
public enum NodeVisibility
{
    Unknown = 0,

    /// <summary>
    /// Registered with the yamen: county seats, granaries, relay posts,
    /// academies, recognized temples, passes.
    /// </summary>
    StateVisible = 1,

    /// <summary>
    /// Locals know but the yamen does not actively intervene:
    /// ancestral halls in strong-clan counties, private pawnshops, market
    /// side lanes, estate inner rings.
    /// </summary>
    LocalKnown = 2,

    /// <summary>
    /// Covert: sworn-society meeting points, contraband caches, fugitive
    /// hideouts. Discovered via <c>GetSettlementsByVisibility(Covert)</c>,
    /// not via public opinion streams.
    /// </summary>
    Covert = 3,
}
