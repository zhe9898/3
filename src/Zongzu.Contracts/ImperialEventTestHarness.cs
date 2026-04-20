namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §24.4 — imperial-axis event kinds a test harness
/// can inject to drive <c>ImperialBand</c> assertions in SPEC §22 liveness
/// tests.
///
/// <para>These are the four externally-triggered throne pulses the spatial
/// skeleton must cope with in Phase 1c. They do not exhaustively enumerate
/// all imperial events the future <c>CourtAndThrone</c> module will
/// emit — only the ones the Phase 1c liveness harness drives.</para>
/// </summary>
public enum ImperialEventKind
{
    Unknown = 0,

    /// <summary>State mourning — drives <c>MourningInterruption</c> up; suppresses corvée and markets.</summary>
    EmperorMourning = 1,

    /// <summary>Grand amnesty — drives <c>AmnestyWave</c> up; softens down-flow compliance.</summary>
    GrandAmnesty = 2,

    /// <summary>Succession unclear or disputed — drives <c>SuccessionUncertainty</c> up; doubles message delay.</summary>
    SuccessionCrisis = 3,

    /// <summary>Chancellor / eunuch / regent upheaval — drives <c>CourtTimeDisruption</c> up.</summary>
    CourtFactionOverturn = 4,
}

/// <summary>
/// SPATIAL_SKELETON_SPEC §3.2 / §24.4 — test-only seam for driving
/// <c>ImperialBand</c> from outside <c>WorldSettlements</c>.
///
/// <para><b>Phase 1c is the only phase where this is the sole driver.</b>
/// In later phases, <c>CourtAndThrone</c> / <c>WorldEvents</c> (Phase 2+)
/// become the authoritative source; this harness stays as a test vehicle.</para>
///
/// <para><b>Visibility rule (SPEC §24.4)</b>: this interface must not be
/// exposed in production builds. It lives in contracts so test projects can
/// type against it, but the concrete implementation must be registered only
/// in test bootstraps — never in the normal application startup path.</para>
/// </summary>
public interface IImperialEventTestHarness
{
    /// <summary>
    /// Inject an imperial pulse of a given kind at 0..100 intensity. The
    /// implementation maps the kind to the matching <c>ImperialBand</c>
    /// channel, applies the intensity, and lets the next xun/month tick
    /// propagate the cross-axis consequences defined in SPEC §3.1.
    /// </summary>
    void Inject(ImperialEventKind kind, int intensity);
}
