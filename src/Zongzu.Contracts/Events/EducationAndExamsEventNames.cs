namespace Zongzu.Contracts;

/// <summary>
/// Canonical event name constants emitted by <c>EducationAndExams</c>.
///
/// New events use prefixed style (<c>EducationAndExams.EventName</c>) per the
/// Renzong pressure chain contract preflight decision: old unprefixed names
/// remain for compatibility; all new cross-module DomainEvents are prefixed.
/// </summary>
public static class EducationAndExamsEventNames
{
    // ---- Pre-existing (unprefixed, compatibility) ----

    public const string ExamPassed = "ExamPassed";

    public const string ExamFailed = "ExamFailed";

    public const string StudyAbandoned = "StudyAbandoned";

    public const string TutorSecured = "TutorSecured";

    // ---- Renzong pressure chain events (prefixed, new) ----

    public const string ExamAttemptResolved = "EducationAndExams.ExamAttemptResolved";
}
