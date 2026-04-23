namespace Zongzu.Contracts;

/// <summary>
/// Canonical event name constants emitted by <c>ConflictAndForce</c>.
///
/// New events use prefixed style (<c>ConflictAndForce.EventName</c>) per the
/// Renzong pressure chain contract preflight decision: old unprefixed names
/// remain for compatibility; all new cross-module DomainEvents are prefixed.
/// </summary>
public static class ConflictAndForceEventNames
{
    // ---- Pre-existing (unprefixed, compatibility) ----

    public const string ConflictResolved = "ConflictResolved";

    public const string CommanderWounded = "CommanderWounded";

    public const string ForceReadinessChanged = "ForceReadinessChanged";

    public const string MilitiaMobilized = "MilitiaMobilized";

    // ---- Renzong pressure chain events (prefixed, new) ----

    public const string ForceReadinessDemand = "ConflictAndForce.ForceReadinessDemand";

    public const string ForceFatigueIncreased = "ConflictAndForce.ForceFatigueIncreased";
}
