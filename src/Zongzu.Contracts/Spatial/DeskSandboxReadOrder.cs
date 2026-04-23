namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §6.4 decision J — the desk sandbox read order has
/// <b>two layers</b>:
/// <list type="bullet">
///   <item><b>Outer shell</b> (this class): <c>Locus → Action → Consequence → Background</c>,
///         from <c>density-and-focus.md</c> UI grammar.</item>
///   <item><b>Inner causal chain</b> (<see cref="AncientSandboxCausalChain"/>):
///         <c>Node → Route → Household/Clan → Public Surface</c>,
///         expanded <i>inside</i> the Consequence region,
///         from <c>china-ancient-sandbox-structure.md</c>.</item>
/// </list>
///
/// Values are monotonically ordered so a sort by this int renders the
/// correct sequence. Gaps (10, 20, 30, 40) leave room to insert intermediate
/// layers without breaking existing ordering contracts.
///
/// Phase 1c registers the constant only; no projection migration is forced.
/// New notifications written after Phase 1c must honor this ordering.
/// </summary>
public static class DeskSandboxReadOrder
{
    /// <summary>Current locus — the single node or route most worth attention right now.</summary>
    public const int CurrentLocus = 10;

    /// <summary>Immediate action — the 1–3 bounded commands tied to the locus.</summary>
    public const int ImmediateAction = 20;

    /// <summary>Consequence context — what will happen next month if ignored / accepted. The causal chain expands here.</summary>
    public const int ConsequenceContext = 30;

    /// <summary>Background context — other low-priority county pulses.</summary>
    public const int BackgroundContext = 40;
}

/// <summary>
/// SPATIAL_SKELETON_SPEC §6.4 decision J (inner layer) — when expanding the
/// ConsequenceContext region of <see cref="DeskSandboxReadOrder"/>, content
/// is ordered by this causal chain: what pressure sits on which node, how
/// it transmits along which route, which households / branches / offices
/// are implicated, and where it surfaces publicly.
///
/// <c>NarrativeProjection</c> (Phase 1c+1) builds trace diffs in this order.
/// </summary>
public static class AncientSandboxCausalChain
{
    /// <summary>Which node carries the pressure.</summary>
    public const int NodePressure = 10;

    /// <summary>Which route is introducing or carrying it.</summary>
    public const int RouteTransmission = 20;

    /// <summary>Which households, branches, or offices are entangled.</summary>
    public const int HouseholdAndClanImplication = 30;

    /// <summary>Which public surface (OpinionChannel) has made it visible.</summary>
    public const int PublicVisibleSurface = 40;
}
