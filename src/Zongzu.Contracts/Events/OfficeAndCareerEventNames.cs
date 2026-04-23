namespace Zongzu.Contracts;

/// <summary>
/// Canonical event name constants emitted by <c>OfficeAndCareer</c>.
///
/// New events use prefixed style (<c>OfficeAndCareer.EventName</c>) per the
/// Renzong pressure chain contract preflight decision: old unprefixed names
/// remain for compatibility; all new cross-module DomainEvents are prefixed.
///
/// Design note: pressure is state; the burst point is the event.
/// <c>EvaluationPressure</c>, <c>ClerkCaptureRisk</c>,
/// <c>WaitingListFrustration</c> and <c>OfficialDefectionRisk</c> are NOT
/// events — they are internal state / projection fields.
/// </summary>
public static class OfficeAndCareerEventNames
{
    // ---- Pre-existing (unprefixed, compatibility) ----

    public const string OfficeGranted = "OfficeGranted";

    public const string OfficeLost = "OfficeLost";

    public const string OfficeTransfer = "OfficeTransfer";

    public const string AuthorityChanged = "AuthorityChanged";

    // ---- Renzong pressure chain events (prefixed, new) ----

    public const string YamenOverloaded = "OfficeAndCareer.YamenOverloaded";

    public const string AppointmentQueuePressure = "OfficeAndCareer.AppointmentQueuePressure";

    public const string AmnestyApplied = "OfficeAndCareer.AmnestyApplied";

    public const string OfficialSupplyRequisition = "OfficeAndCareer.OfficialSupplyRequisition";

    public const string MemorialAttackReceived = "OfficeAndCareer.MemorialAttackReceived";

    public const string ClerkCaptureDeepened = "OfficeAndCareer.ClerkCaptureDeepened";

    /// <summary>
    /// Unified implementation outcome event. Payload carries
    /// <c>outcome</c> enum: Rapid, Dragged, Captured, PaperCompliance.
    /// Replaces the three separate events <c>RapidImplementation</c>,
    /// <c>ImplementationDrag</c> and <c>ImplementationCaptured</c>.
    /// </summary>
    public const string PolicyImplemented = "OfficeAndCareer.PolicyImplemented";

    public const string PolicyWindowOpened = "OfficeAndCareer.PolicyWindowOpened";

    public const string AppointmentSlateProposed = "OfficeAndCareer.AppointmentSlateProposed";

    public const string PolicyWordingDrafted = "OfficeAndCareer.PolicyWordingDrafted";

    public const string DispatchArrived = "OfficeAndCareer.DispatchArrived";

    /// <summary>
    /// Fired when an evaluation cycle completes and the score is available.
    /// Replaces <c>EvaluationCycleTriggered</c> (which was an internal cadence
    /// signal, not a cross-module event).
    /// </summary>
    public const string EvaluationCompleted = "OfficeAndCareer.EvaluationCompleted";

    /// <summary>
    /// Fired when an official defects to a new regime (P5+).
    /// </summary>
    public const string OfficeDefected = "OfficeAndCareer.OfficeDefected";
}
