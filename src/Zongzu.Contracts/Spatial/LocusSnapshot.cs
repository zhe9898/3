using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §6.4 / §8 (decision J) — the "current locus" read
/// by the desk sandbox: the single node-or-route that most deserves player
/// attention right now, derived deterministically from the current
/// <see cref="SeasonBandSnapshot"/>, routes, and settlements.
///
/// <para>One of <see cref="PrimaryNode"/> / <see cref="PrimaryRoute"/> is
/// set; both may be set when the locus is "this ferry on this silted
/// route". Neither being set is legal only for the bootstrap tick.</para>
///
/// <para><b>Determinism rule</b> (SPEC §8 locus-score block): the
/// intensity scoring function is pure, deterministic, and advances strictly
/// with month / xun ticks. No wall-clock input, no random tie-breaking —
/// ties resolve by settlement / route Id order.</para>
///
/// <para><see cref="ReasonKey"/> is a projection-layer translation key
/// ("flood-risk-breached" / "canal-opening" / "corvee-peak" / ...); shell
/// renders prose, authority stores the key (ENGINEERING_RULES §10).</para>
/// </summary>
/// <param name="PrimaryNode">Node this locus anchors on, if any.</param>
/// <param name="PrimaryRoute">Route this locus anchors on, if any.</param>
/// <param name="ReasonKey">Translation key for why this locus wins right now.</param>
/// <param name="Intensity">0..100 — how strongly this locus is claiming focus.</param>
public sealed record LocusSnapshot(
    SettlementId? PrimaryNode,
    RouteId? PrimaryRoute,
    string ReasonKey,
    int Intensity);
