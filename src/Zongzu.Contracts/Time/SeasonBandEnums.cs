namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §3 decision D — season is a <b>parallel band
/// container</b>, not a single enum. These three enums are the named
/// state-machine axes inside <c>SeasonBand</c>; continuous bands
/// (LaborPinch, FloodRisk, MessageDelayBand, ...) are 0–100 ints held
/// directly on <c>SeasonBand</c>.
/// </summary>
public enum AgrarianPhase
{
    Unknown = 0,
    Slack = 1,
    Sowing = 2,
    Transplant = 3,
    Tending = 4,
    Harvest = 5,
}

/// <summary>
/// Is the canal navigable? SPEC §18.2 drives this with a month /
/// FloodRisk / EmbankmentStrain state machine.
/// </summary>
public enum CanalWindow
{
    Unknown = 0,
    Closed = 1,
    Limited = 2,
    Open = 3,
}

/// <summary>
/// Are corvée levies politically / agronomically practical right now?
/// SPEC §18.3 state machine.
/// </summary>
public enum CorveeWindow
{
    Unknown = 0,
    Quiet = 1,
    Pressed = 2,
    Emergency = 3,
}
