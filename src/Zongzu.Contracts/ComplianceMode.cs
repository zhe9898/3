namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §2.4 decision I (government axis) — describes the
/// <i>actual delivery quality</i> of a top-down dispatch chain, not the
/// nominal policy. Only meaningful for down-flow route kinds
/// (<see cref="RouteKind.OfficialDispatchRoute"/>, <see cref="RouteKind.TaxRoute"/>,
/// <see cref="RouteKind.CorveeRoute"/>, <see cref="RouteKind.MilitaryMoveRoute"/>).
/// Other route kinds carry the field but ignore it.
/// </summary>
public enum ComplianceMode
{
    Unknown = 0,

    /// <summary>New magistrate, imperial tour, or high-profile case — orders land hard.</summary>
    StrictEnforcement = 1,

    /// <summary>Default state — paperwork flows, enforcement is partial.</summary>
    PaperCompliance = 2,

    /// <summary>Gentry / clan / broker mediation softens part of the order.</summary>
    LocalBuffering = 3,

    /// <summary>Clerks, runners, brokers, or lineages have captured execution.</summary>
    BrokerCapture = 4,
}
