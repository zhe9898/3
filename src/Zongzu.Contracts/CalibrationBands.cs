namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §4 — time is expressed as <b>named bands</b>, not
/// fixed day counts, per <c>simulation-calibration.md</c>. Bands resist
/// pseudo-precision ("3.7 days"), survive region re-tuning, and let
/// narrative translation layers render appropriate prose.
///
/// Values are integers so they fit directly in <c>RouteState.TravelDaysBand</c>
/// and <c>SeasonBand.MessageDelayBand</c>.
/// </summary>
public static class CalibrationBands
{
    /// <summary>Intra-settlement or neighboring village — same day to within hours.</summary>
    public const int TravelDays_SameDay = 0;

    /// <summary>Within the county — 1 to 3 days.</summary>
    public const int TravelDays_Short = 1;

    /// <summary>County to adjacent county or county-to-prefecture — 3 to 7 days.</summary>
    public const int TravelDays_Medium = 2;

    /// <summary>Prefecture to adjacent prefecture — 7 to 15 days.</summary>
    public const int TravelDays_Long = 3;

    /// <summary>Cross-circuit, cross-region — 15 to 40 days.</summary>
    public const int TravelDays_Regional = 4;

    /// <summary>Remote, season-closed, or disaster-broken — indefinite.</summary>
    public const int TravelDays_Extreme = 5;

    /// <summary>Fast — current xun.</summary>
    public const int MessageDelay_Fast = 0;

    /// <summary>Normal — 1 xun (~10 days).</summary>
    public const int MessageDelay_Normal = 1;

    /// <summary>Slow — 2 xun (~20 days).</summary>
    public const int MessageDelay_Slow = 2;

    /// <summary>Backed-up — multi-month pileup (flood season, succession crisis, etc.).</summary>
    public const int MessageDelay_Jammed = 3;

    /// <summary>Broken — routes cut, rumor outruns official dispatch.</summary>
    public const int MessageDelay_Broken = 4;
}
