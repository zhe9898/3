namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §23.3 — query surface for validating that a
/// <see cref="ConflictAnchor"/> fits the spatial skeleton (right node kind
/// for the engagement, at least one route for tactical-lite+ scales, scale
/// within the scale-vs-board red line of SPEC §23.4).
///
/// <para><b>Phase 1c: contract only</b>. Implementation lands in Phase 5/6
/// with <c>ConflictAndForce</c> and <c>WarfareCampaign</c>. Phase 1c
/// validates nothing — it just registers the seam so reviewers can reject
/// scale 1-3 anchors mounted on the campaign board later.</para>
/// </summary>
public interface IConflictAnchorQueries
{
    /// <summary>
    /// Validates a conflict anchor against the spatial skeleton: the listed
    /// nodes exist and match the <see cref="ConflictAnchor.KindKey"/>'s
    /// expected node kinds (e.g. "pass-defense" needs a <c>Pass</c>); the
    /// listed routes exist and touch the nodes; scale is within the kind's
    /// permitted range.
    /// </summary>
    bool IsAnchorValid(ConflictAnchor anchor);
}
