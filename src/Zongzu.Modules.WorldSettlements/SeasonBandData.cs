using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC §3.1 — authoritative, mutable container for the
/// three parallel season axes (natural / government / imperial). Owned by
/// <c>WorldSettlements</c>; consumers read <see cref="SeasonBandSnapshot"/>
/// via <see cref="IWorldSettlementsQueries.GetCurrentSeason"/>.
///
/// <para><b>Field-for-field mirror of</b> <see cref="SeasonBandSnapshot"/>,
/// except as a mutable class rather than an immutable record.</para>
///
/// <para><b>Advancement ownership</b> (SPEC §3.2):
/// <list type="bullet">
///   <item><c>RunMonth</c> advances <see cref="AgrarianPhase"/>,
///         <see cref="HarvestWindowProgress"/>, <see cref="CanalWindow"/>,
///         <see cref="CorveeWindow"/>, and decay of imperial mourning /
///         amnesty / succession axes.</item>
///   <item><c>RunXun</c> advances <see cref="LaborPinch"/>,
///         <see cref="MarketCadencePulse"/>, <see cref="MessageDelayBand"/>.</item>
///   <item><see cref="EmbankmentStrain"/> / <see cref="FloodRisk"/> /
///         <see cref="WaterControlConfidence"/> also land in xun but read
///         <c>PopulationAndHouseholds</c> corvée-execution via query (no
///         cross-write).</item>
///   <item><see cref="ImperialBandData.MandateConfidence"/> and
///         <see cref="ImperialBandData.CourtTimeDisruption"/> are
///         <b>externally driven only</b>. WorldSettlements never self-advances
///         them — Phase 1c uses <see cref="IImperialEventTestHarness"/>,
///         Phase 2+ plugs in <c>CourtAndThrone</c> / <c>WorldEvents</c>.</item>
/// </list></para>
/// </summary>
public sealed class SeasonBandData
{
    /// <summary>
    /// GameDate when this band was last advanced. Null until the first tick
    /// runs. Using nullable instead of default value keeps MessagePack
    /// round-trip safe — a default GameDate(0, 0) would fail the ctor
    /// invariant (month must be 1..12).
    /// </summary>
    public GameDate? AsOf { get; set; }

    // ── Agrarian axis ────────────────────────────────────────────
    public AgrarianPhase AgrarianPhase { get; set; } = AgrarianPhase.Slack;

    public int LaborPinch { get; set; }

    public int HarvestWindowProgress { get; set; }

    // ── Water-control axis ───────────────────────────────────────
    public int WaterControlConfidence { get; set; }

    public int EmbankmentStrain { get; set; }

    public int FloodRisk { get; set; }

    // ── Canal axis ───────────────────────────────────────────────
    public CanalWindow CanalWindow { get; set; } = CanalWindow.Limited;

    // ── Market axis ──────────────────────────────────────────────
    public int MarketCadencePulse { get; set; }

    // ── Corvée axis ──────────────────────────────────────────────
    public CorveeWindow CorveeWindow { get; set; } = CorveeWindow.Quiet;

    // ── Information axis ─────────────────────────────────────────
    public int MessageDelayBand { get; set; }

    // ── Imperial axis (decision I; not self-advanced) ────────────
    public ImperialBandData Imperial { get; set; } = new();

    /// <summary>
    /// Persisted baseline used by <c>SeasonBandAdvancer</c> to decide when to
    /// emit <c>ImperialRhythmChanged</c>. Compared against <see cref="Imperial"/>
    /// at the end of each month; updated only when a 10-point band crossing
    /// fires the announcement (prevents decay-drift spam while still catching
    /// between-month injections from <see cref="IImperialEventTestHarness"/>).
    /// </summary>
    public ImperialBandData PreviousAnnouncedImperial { get; set; } = new();
}

/// <summary>
/// SPATIAL_SKELETON_SPEC §3.1 — imperial sub-band nested inside
/// <see cref="SeasonBandData"/>. All values 0..100.
///
/// <para><b>Not self-advanced by</b> <c>WorldSettlements</c>. Channels move
/// only through <see cref="IImperialEventTestHarness"/> (Phase 1c) or
/// <c>CourtAndThrone</c> / <c>WorldEvents</c> (Phase 2+).</para>
/// </summary>
public sealed class ImperialBandData
{
    public int MourningInterruption { get; set; }

    public int AmnestyWave { get; set; }

    public int SuccessionUncertainty { get; set; }

    public int MandateConfidence { get; set; }

    public int CourtTimeDisruption { get; set; }
}
