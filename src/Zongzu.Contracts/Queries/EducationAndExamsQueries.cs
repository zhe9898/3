using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record EducationCandidateSnapshot
{
    public PersonId PersonId { get; init; }

    public ClanId ClanId { get; init; }

    public InstitutionId AcademyId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public bool IsStudying { get; init; }

    public bool HasTutor { get; init; }

    public int StudyProgress { get; init; }

    public int Stress { get; init; }

    public int ExamAttempts { get; init; }

    public bool HasPassedLocalExam { get; init; }

    public string LastOutcome { get; init; } = string.Empty;

    public string LastExplanation { get; init; } = string.Empty;

    public int ScholarlyReputation { get; init; }

    // Phase 6 科举骨骼 — LIVING_WORLD_DESIGN §2.6
    public ExamTier CurrentTier { get; init; } = ExamTier.CountyExam;

    public ExamResult LastResult { get; init; } = ExamResult.Pending;

    public FallbackPath FallbackPath { get; init; } = FallbackPath.ContinueStudy;
}

public sealed record AcademySnapshot
{
    public InstitutionId Id { get; init; }

    public SettlementId SettlementId { get; init; }

    public string AcademyName { get; init; } = string.Empty;

    public bool IsOpen { get; init; }

    public int Capacity { get; init; }

    public int Prestige { get; init; }
}

public interface IEducationAndExamsQueries
{
    EducationCandidateSnapshot GetRequiredCandidate(PersonId personId);

    AcademySnapshot GetRequiredAcademy(InstitutionId institutionId);

    IReadOnlyList<EducationCandidateSnapshot> GetCandidates();

    IReadOnlyList<AcademySnapshot> GetAcademies();

    // Phase 6 科举骨骼 — LIVING_WORLD_DESIGN §2.6
    IReadOnlyList<EducationCandidateSnapshot> GetCandidatesByTier(ExamTier tier) => [];
}
