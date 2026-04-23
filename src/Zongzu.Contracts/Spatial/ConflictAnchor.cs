using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §23.3 — parameter object describing a conflict's
/// spatial anchor: which nodes it touches, which routes it runs along, how
/// big it is, what kind it is.
///
/// Used by the future <see cref="IConflictAnchorQueries"/> to validate that
/// a <c>ConflictAndForce</c> / <c>WarfareCampaign</c> engagement actually
/// fits the spatial skeleton — a "pass-defense" anchor must touch at least
/// one <see cref="SettlementNodeKind.Pass"/> node; an "escort-ambush" anchor
/// must list at least one route, etc.
///
/// <para><b>Red line (SPEC §23.4)</b>: scale 1-3 clashes must not be mounted
/// on the campaign board. This record's <see cref="Scale"/> field is the
/// anchor that lets reviewers enforce that rule.</para>
///
/// <para>Phase 1c only <b>registers</b> the type and <see cref="IConflictAnchorQueries"/>
/// interface; the validation implementation lands in Phase 5/6 with
/// <c>ConflictAndForce</c> and <c>WarfareCampaign</c>.</para>
/// </summary>
/// <param name="Scale">
/// 1..5 severity ladder from <c>conflict-scale-ladder.md</c>. Scale 1-2 =
/// family/market clash, 3 = escort fight or raid, 4-5 = campaign-board.
/// </param>
/// <param name="Nodes">
/// Settlements this conflict touches. At least one required; a pure route
/// ambush still anchors to the nearest node for shell context.
/// </param>
/// <param name="Routes">
/// Routes the conflict runs along. For tactical-lite and above (scale >= 3)
/// at least one route is expected; pure-node scuffles may leave this empty.
/// </param>
/// <param name="KindKey">
/// Translation key for conflict kind — "escort-ambush" / "pass-defense" /
/// "market-brawl" / etc. Shell maps to prose; authority keeps the key.
/// </param>
public sealed record ConflictAnchor(
    int Scale,
    IReadOnlyList<SettlementId> Nodes,
    IReadOnlyList<RouteId> Routes,
    string KindKey);
