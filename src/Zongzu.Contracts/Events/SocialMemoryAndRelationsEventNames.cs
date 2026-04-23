namespace Zongzu.Contracts;

/// <summary>
/// Canonical event name constants emitted by <c>SocialMemoryAndRelations</c>.
///
/// New events use prefixed style (<c>SocialMemoryAndRelations.EventName</c>)
/// per the Renzong pressure chain contract preflight decision: old unprefixed
/// names remain for compatibility; all new cross-module DomainEvents are
/// prefixed.
/// </summary>
public static class SocialMemoryAndRelationsEventNames
{
    // ---- Pre-existing (unprefixed, compatibility) ----

    public const string GrudgeEscalated = "GrudgeEscalated";

    public const string GrudgeSoftened = "GrudgeSoftened";

    public const string FavorIncurred = "FavorIncurred";

    public const string ClanNarrativeUpdated = "ClanNarrativeUpdated";

    // ---- Pressure-tempering kernel receipts (prefixed, new) ----

    public const string PressureTempered = "SocialMemoryAndRelations.PressureTempered";

    public const string EmotionalPressureShifted = "SocialMemoryAndRelations.EmotionalPressureShifted";
}
