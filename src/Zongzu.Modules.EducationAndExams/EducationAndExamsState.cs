using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.EducationAndExams;

public sealed class EducationAndExamsState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.EducationAndExams;

    public List<EducationPersonState> People { get; set; } = new();

    public List<AcademyState> Academies { get; set; } = new();
}

public sealed class EducationPersonState
{
    public PersonId PersonId { get; set; }

    public ClanId ClanId { get; set; }

    public InstitutionId AcademyId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsStudying { get; set; }

    public bool HasTutor { get; set; }

    public int TutorQuality { get; set; }

    public int StudyProgress { get; set; }

    public int Stress { get; set; }

    public int ExamAttempts { get; set; }

    public bool HasPassedLocalExam { get; set; }

    public string LastOutcome { get; set; } = "Pending";

    public string LastExplanation { get; set; } = string.Empty;

    public int ScholarlyReputation { get; set; }
}

public sealed class AcademyState
{
    public InstitutionId Id { get; set; }

    public SettlementId SettlementId { get; set; }

    public string AcademyName { get; set; } = string.Empty;

    public bool IsOpen { get; set; }

    public int Capacity { get; set; }

    public int Prestige { get; set; }
}
