namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §21 — WorldSettlements' "nervous endings". The module
/// emits these events when season bands advance, routes are constrained,
/// nodes breach thresholds, or covert things surface.
///
/// Event name format: <c>WorldSettlements.&lt;past-tense-verb-phrase&gt;</c>.
/// Note this differs from older modules (FamilyCore / WarfareCampaign) that
/// use unprefixed names. SPEC §21 chose the prefixed style to scale cleanly
/// as the event catalog grows across modules.
///
/// The imperial axis (decision I, SPEC §3.1) is <b>not self-advancing</b> in
/// Phase 1c. <see cref="ImperialRhythmChanged"/> fires only when an external
/// driver (test harness in 1c; <c>CourtAndThrone</c> / <c>WorldEvents</c> in
/// later phases) injects a band-threshold crossing.
///
/// Phase 1c ships with WorldSettlements emitting at minimum:
/// <see cref="SeasonPhaseAdvanced"/>, <see cref="CanalWindowChanged"/>,
/// <see cref="CorveeWindowChanged"/>, <see cref="RouteConstraintEmerged"/>,
/// <see cref="FloodRiskThresholdBreached"/>, <see cref="SeasonalFestivalArrived"/>,
/// and <see cref="ImperialRhythmChanged"/> (on injection).
/// Other names are registered so subscribers can compile; their emitters land
/// in later phases.
/// </summary>
public static class WorldSettlementsEventNames
{
    // Natural-axis rhythm
    public const string SeasonPhaseAdvanced = "WorldSettlements.SeasonPhaseAdvanced";
    public const string CanalWindowChanged = "WorldSettlements.CanalWindowChanged";

    // Government-axis rhythm
    public const string CorveeWindowChanged = "WorldSettlements.CorveeWindowChanged";
    public const string ComplianceModeShifted = "WorldSettlements.ComplianceModeShifted";

    // Imperial-axis rhythm (decision I — not self-driven by WorldSettlements)
    public const string ImperialRhythmChanged = "WorldSettlements.ImperialRhythmChanged";

    // Route dynamics (silting, slides, flood, banditry, snow-closure)
    public const string RouteConstraintEmerged = "WorldSettlements.RouteConstraintEmerged";
    public const string RouteConstraintCleared = "WorldSettlements.RouteConstraintCleared";

    // Node alerts
    public const string FloodRiskThresholdBreached = "WorldSettlements.FloodRiskThresholdBreached";
    public const string EmbankmentStressAlert = "WorldSettlements.EmbankmentStressAlert";

    // Visibility / legitimacy dynamics (decision H)
    public const string NodeVisibilityDiscovered = "WorldSettlements.NodeVisibilityDiscovered";
    public const string IllicitRouteExposed = "WorldSettlements.IllicitRouteExposed";

    // Force-family stationing (WorldSettlements re-broadcasts so desk sandbox
    // has a single unified event source; underlying state is owned by
    // ConflictAndForce / FamilyCore / OrderAndBanditry)
    public const string ForceStationChanged = "WorldSettlements.ForceStationChanged";

    // Calendar
    public const string SeasonalFestivalArrived = "WorldSettlements.SeasonalFestivalArrived";
}
