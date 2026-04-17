using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class EducationCandidateSnapshot
{
    public PersonId PersonId { get; set; }

    public ClanId ClanId { get; set; }

    public InstitutionId AcademyId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsStudying { get; set; }

    public bool HasTutor { get; set; }

    public int StudyProgress { get; set; }

    public int Stress { get; set; }

    public int ExamAttempts { get; set; }

    public bool HasPassedLocalExam { get; set; }

    public string LastOutcome { get; set; } = string.Empty;

    public string LastExplanation { get; set; } = string.Empty;

    public int ScholarlyReputation { get; set; }
}

public sealed class AcademySnapshot
{
    public InstitutionId Id { get; set; }

    public SettlementId SettlementId { get; set; }

    public string AcademyName { get; set; } = string.Empty;

    public bool IsOpen { get; set; }

    public int Capacity { get; set; }

    public int Prestige { get; set; }
}

public interface IEducationAndExamsQueries
{
    EducationCandidateSnapshot GetRequiredCandidate(PersonId personId);

    AcademySnapshot GetRequiredAcademy(InstitutionId institutionId);

    IReadOnlyList<EducationCandidateSnapshot> GetCandidates();

    IReadOnlyList<AcademySnapshot> GetAcademies();
}
