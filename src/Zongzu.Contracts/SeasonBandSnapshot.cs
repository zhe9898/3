using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §3.1 / §8 — read-only projection of the
/// <c>SeasonBand</c> container owned by <c>WorldSettlements</c>.
///
/// <para><b>Decision D</b>: season is not a single enum but a set of
/// parallel bands. Three named-state axes (<see cref="AgrarianPhase"/>,
/// <see cref="CanalWindow"/>, <see cref="CorveeWindow"/>) and a handful of
/// 0..100 continuous pulses run in parallel; <see cref="Imperial"/> is the
/// third (imperial) axis per decision I.</para>
///
/// <para><b>Cross-axis hard rules</b> (SPEC §3.1, enforced at authoritative
/// month / xun — mirrored here for read-only consumers):</para>
/// <list type="bullet">
///   <item><c>Imperial.MourningInterruption &gt;= 60</c> forces
///         <see cref="CorveeWindow"/> = <see cref="CorveeWindow.Quiet"/>.</item>
///   <item><c>Imperial.AmnestyWave &gt;= 50</c> downgrades
///         down-flow routes' <see cref="ComplianceMode"/> by one rung.</item>
///   <item><c>Imperial.SuccessionUncertainty &gt;= 70</c> at least doubles
///         <see cref="MessageDelayBand"/>.</item>
/// </list>
/// </summary>
public sealed record SeasonBandSnapshot
{
    /// <summary>GameDate this snapshot was captured at. Null before the first season-advancing tick has run.</summary>
    public GameDate? AsOf { get; init; }

    // ── Agrarian axis ────────────────────────────────────────────
    public AgrarianPhase AgrarianPhase { get; init; }

    /// <summary>0..100 — labor pinch (drives corvée window and labor-borrow availability).</summary>
    public int LaborPinch { get; init; }

    /// <summary>0..100 — autumn-harvest window progress.</summary>
    public int HarvestWindowProgress { get; init; }

    // ── Water-control axis ───────────────────────────────────────
    /// <summary>0..100 — water-works confidence (dike / sluice / canal upkeep).</summary>
    public int WaterControlConfidence { get; init; }

    /// <summary>0..100 — embankment strain.</summary>
    public int EmbankmentStrain { get; init; }

    /// <summary>0..100 — flood risk.</summary>
    public int FloodRisk { get; init; }

    // ── Canal axis ───────────────────────────────────────────────
    public CanalWindow CanalWindow { get; init; }

    // ── Market axis ──────────────────────────────────────────────
    /// <summary>0..100 — this xun's market heat pulse.</summary>
    public int MarketCadencePulse { get; init; }

    // ── Corvée axis ──────────────────────────────────────────────
    public CorveeWindow CorveeWindow { get; init; }

    // ── Information axis ─────────────────────────────────────────
    /// <summary>
    /// Message-delay band (0..4, see <see cref="CalibrationBands"/>
    /// <c>MessageDelay_*</c>). MVP = official-dispatch delay only; private
    /// letter and rumor delay arrive with <c>PublicLifeAndRumor</c> phase 2+.
    /// </summary>
    public int MessageDelayBand { get; init; }

    // ── Imperial axis (decision I) ───────────────────────────────
    /// <summary>Third axis — mostly externally driven (not self-advancing in Phase 1c).</summary>
    public ImperialBandSnapshot Imperial { get; init; } = new();
}

/// <summary>
/// SPATIAL_SKELETON_SPEC §3.1 — read-only projection of the
/// <c>ImperialBand</c> nested inside <c>SeasonBand</c>.
///
/// <para><b>Not self-advanced by</b> <c>WorldSettlements</c>. Values move only
/// when an external driver injects a threshold crossing:</para>
/// <list type="bullet">
///   <item>Phase 1c: <c>IImperialEventTestHarness</c> for tests.</item>
///   <item>Phase 2+: <c>WorldEvents</c> / <c>CourtAndThrone</c> (enthronement,
///         amnesty, succession dispute, regent rotation).</item>
/// </list>
///
/// <para>All values are 0..100.</para>
/// </summary>
public sealed record ImperialBandSnapshot
{
    /// <summary>State-mourning severity — suppresses markets, banquets, corvée.</summary>
    public int MourningInterruption { get; init; }

    /// <summary>Amnesty wave — softens down-flow compliance for a season.</summary>
    public int AmnestyWave { get; init; }

    /// <summary>Succession opacity — heir unclear / disputed / deposed; clerks wait.</summary>
    public int SuccessionUncertainty { get; init; }

    /// <summary>Long-band mandate confidence — slow drift under disaster and war.</summary>
    public int MandateConfidence { get; init; }

    /// <summary>Court-rhythm disruption — chancellor turnover, eunuch faction, deposition.</summary>
    public int CourtTimeDisruption { get; init; }
}
