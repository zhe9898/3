using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.EducationAndExams;

public sealed class EducationAndExamsModule : ModuleRunner<EducationAndExamsState>
{
    private static readonly string[] CommandNames =
    [
        "FundStudy",
        "HireTutor",
        "RedirectEducationalSupport",
        "WithdrawFromStudy",
    ];

    private static readonly string[] EventNames =
    [
        "ExamPassed",
        "ExamFailed",
        "StudyAbandoned",
        "TutorSecured",
    ];

    public override string ModuleKey => KnownModuleKeys.EducationAndExams;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 500;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override EducationAndExamsState CreateInitialState()
    {
        return new EducationAndExamsState();
    }

    public override void RegisterQueries(EducationAndExamsState state, QueryRegistry queries)
    {
        queries.Register<IEducationAndExamsQueries>(new EducationQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<EducationAndExamsState> scope)
    {
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();

        Dictionary<InstitutionId, AcademyState> academies = scope.State.Academies
            .OrderBy(static academy => academy.Id.Value)
            .ToDictionary(static academy => academy.Id, static academy => academy);

        Dictionary<InstitutionId, int> activeSeats = scope.State.People
            .Where(static person => person.IsStudying)
            .GroupBy(static person => person.AcademyId)
            .ToDictionary(static group => group.Key, static group => group.Count());

        foreach (EducationPersonState student in scope.State.People.OrderBy(static person => person.PersonId.Value))
        {
            if (!academies.TryGetValue(student.AcademyId, out AcademyState? academy))
            {
                continue;
            }

            ClanSnapshot clan = familyQueries.GetRequiredClan(student.ClanId);
            ClanNarrativeSnapshot narrative = socialQueries.GetRequiredClanNarrative(student.ClanId);
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(academy.SettlementId);

            if (!student.IsStudying || student.HasPassedLocalExam)
            {
                continue;
            }

            if (!student.HasTutor && clan.SupportReserve >= 58 && narrative.GrudgePressure < 70)
            {
                student.HasTutor = true;
                student.TutorQuality = 10 + scope.Context.Random.NextInt(0, 8);
                scope.Emit("TutorSecured", $"{student.DisplayName}已得塾师指授。");
            }

            int supportFactor = clan.SupportReserve >= 60 ? 2 : clan.SupportReserve >= 45 ? 1 : -1;
            int narrativeFactor = narrative.FavorBalance >= 10 ? 1 : narrative.ShamePressure >= 55 ? -1 : 0;
            int settlementFactor = settlement.Prosperity >= 58 ? 1 : settlement.Security < 45 ? -1 : 0;
            int tutorFactor = student.HasTutor ? Math.Max(1, student.TutorQuality / 10) : 0;
            int crowdingPenalty = activeSeats.TryGetValue(student.AcademyId, out int seats) && seats > academy.Capacity ? 1 : 0;
            int progressDelta = Math.Max(1, 4 + supportFactor + narrativeFactor + settlementFactor + tutorFactor - crowdingPenalty + scope.Context.Random.NextInt(-1, 2));

            student.StudyProgress = Math.Clamp(student.StudyProgress + progressDelta, 0, 120);
            student.Stress = Math.Clamp(student.Stress + ComputeStressDelta(clan, narrative), 0, 100);

            scope.RecordDiff(
                $"{student.DisplayName}学业进至{student.StudyProgress}，心气劳迫{student.Stress}。",
                student.PersonId.Value.ToString());

            if (!IsExamWindow(scope.Context.CurrentDate) || student.StudyProgress < 60)
            {
                continue;
            }

            student.ExamAttempts += 1;
            int score = student.StudyProgress
                + (academy.Prestige / 2)
                + student.TutorQuality
                + (clan.SupportReserve / 8)
                + (narrative.FavorBalance / 10)
                - (narrative.ShamePressure / 8)
                - student.Stress
                + scope.Context.Random.NextInt(-6, 7);

            if (score >= 75)
            {
                student.HasPassedLocalExam = true;
                student.IsStudying = false;
                student.ScholarlyReputation = Math.Clamp(student.ScholarlyReputation + 15, 0, 100);
                student.LastOutcome = "Passed";
                student.LastExplanation =
                    $"以学业{student.StudyProgress}、塾望{academy.Prestige}、塾师之助{student.TutorQuality}与宗房接济{clan.SupportReserve}，场中得分{score}。";
                student.StudyProgress = 25;

                scope.RecordDiff(
                    $"{student.DisplayName}场屋得捷。{student.LastExplanation}",
                    student.PersonId.Value.ToString());
                scope.Emit("ExamPassed", $"{student.DisplayName}场屋得捷。");
            }
            else
            {
                student.LastOutcome = "Failed";
                student.LastExplanation =
                    $"心气劳迫{student.Stress}、羞压{narrative.ShamePressure}、学业{student.StudyProgress}，场中仅得{score}。";
                student.StudyProgress = Math.Max(20, student.StudyProgress - 25);
                student.Stress = Math.Clamp(student.Stress + 8, 0, 100);

                scope.RecordDiff(
                    $"{student.DisplayName}场屋失利。{student.LastExplanation}",
                    student.PersonId.Value.ToString());
                scope.Emit("ExamFailed", $"{student.DisplayName}场屋失利。");

                if (student.ExamAttempts >= 3 && student.Stress >= 70)
                {
                    student.IsStudying = false;
                    student.LastOutcome = "Abandoned";
                    student.LastExplanation = $"屡试不捷，心气劳迫至{student.Stress}，遂停塾罢读。";
                    scope.Emit("StudyAbandoned", $"{student.DisplayName}停塾罢读。");
                }
            }
        }
    }

    private static int ComputeStressDelta(ClanSnapshot clan, ClanNarrativeSnapshot narrative)
    {
        int delta = 1;
        if (narrative.ShamePressure >= 50)
        {
            delta += 1;
        }

        if (clan.SupportReserve >= 60)
        {
            delta -= 1;
        }

        return delta;
    }

    private static bool IsExamWindow(GameDate currentDate)
    {
        return currentDate.Month is 3 or 9;
    }

    private sealed class EducationQueries : IEducationAndExamsQueries
    {
        private readonly EducationAndExamsState _state;

        public EducationQueries(EducationAndExamsState state)
        {
            _state = state;
        }

        public EducationCandidateSnapshot GetRequiredCandidate(PersonId personId)
        {
            EducationPersonState person = _state.People.Single(person => person.PersonId == personId);
            return ClonePerson(person);
        }

        public AcademySnapshot GetRequiredAcademy(InstitutionId institutionId)
        {
            AcademyState academy = _state.Academies.Single(academy => academy.Id == institutionId);
            return CloneAcademy(academy);
        }

        public IReadOnlyList<EducationCandidateSnapshot> GetCandidates()
        {
            return _state.People
                .OrderBy(static person => person.PersonId.Value)
                .Select(ClonePerson)
                .ToArray();
        }

        public IReadOnlyList<AcademySnapshot> GetAcademies()
        {
            return _state.Academies
                .OrderBy(static academy => academy.Id.Value)
                .Select(CloneAcademy)
                .ToArray();
        }

        private static EducationCandidateSnapshot ClonePerson(EducationPersonState person)
        {
            return new EducationCandidateSnapshot
            {
                PersonId = person.PersonId,
                ClanId = person.ClanId,
                AcademyId = person.AcademyId,
                DisplayName = person.DisplayName,
                IsStudying = person.IsStudying,
                HasTutor = person.HasTutor,
                StudyProgress = person.StudyProgress,
                Stress = person.Stress,
                ExamAttempts = person.ExamAttempts,
                HasPassedLocalExam = person.HasPassedLocalExam,
                LastOutcome = person.LastOutcome,
                LastExplanation = person.LastExplanation,
                ScholarlyReputation = person.ScholarlyReputation,
            };
        }

        private static AcademySnapshot CloneAcademy(AcademyState academy)
        {
            return new AcademySnapshot
            {
                Id = academy.Id,
                SettlementId = academy.SettlementId,
                AcademyName = academy.AcademyName,
                IsOpen = academy.IsOpen,
                Capacity = academy.Capacity,
                Prestige = academy.Prestige,
            };
        }
    }
}
