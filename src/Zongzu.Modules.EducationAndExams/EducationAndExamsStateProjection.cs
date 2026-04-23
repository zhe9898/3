using System;

namespace Zongzu.Modules.EducationAndExams;

/// <summary>
/// Phase 6 科举骨骼 schema migration（<c>LIVING_WORLD_DESIGN §2.6</c>）。
/// v1 → v2：为既有 <see cref="EducationPersonState"/> 补上 CurrentTier / LastResult / FallbackPath 的合理默认。
/// </summary>
public static class EducationAndExamsStateProjection
{
    public static void UpgradeFromSchemaV1ToV2(EducationAndExamsState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (EducationPersonState person in state.People)
        {
            if (person.CurrentTier == Contracts.ExamTier.Unknown)
            {
                person.CurrentTier = person.HasPassedLocalExam
                    ? Contracts.ExamTier.PrefecturalExam
                    : Contracts.ExamTier.CountyExam;
            }

            if (person.LastResult == Contracts.ExamResult.Unknown)
            {
                person.LastResult = person.LastOutcome switch
                {
                    "Passed" => Contracts.ExamResult.Passed,
                    "Failed" => Contracts.ExamResult.Failed,
                    "Abandoned" => Contracts.ExamResult.Abandoned,
                    _ => Contracts.ExamResult.Pending,
                };
            }

            if (person.FallbackPath == Contracts.FallbackPath.Unknown)
            {
                person.FallbackPath = Contracts.FallbackPath.ContinueStudy;
            }
        }
    }
}
