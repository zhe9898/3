using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.EducationAndExams.Tests;

[TestFixture]
public sealed class EducationAndExamsModuleTests
{
    [Test]
    public void RunMonth_ProducesExplainableExamPassDuringExamWindow()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 62,
            Prosperity = 64,
            BaselineInstitutionCount = 2,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 72,
            HeirPersonId = new PersonId(1),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Scholarly ambition",
            GrudgePressure = 18,
            FearPressure = 12,
            ShamePressure = 10,
            FavorBalance = 16,
        });

        EducationAndExamsModule educationModule = new();
        EducationAndExamsState educationState = educationModule.CreateInitialState();
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(1),
            AcademyName = "Lanxi Academy",
            IsOpen = true,
            Capacity = 2,
            Prestige = 44,
        });
        educationState.People.Add(new EducationPersonState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            AcademyId = new InstitutionId(1),
            DisplayName = "Zhang Yuan",
            IsStudying = true,
            HasTutor = false,
            TutorQuality = 0,
            StudyProgress = 68,
            Stress = 8,
            ExamAttempts = 0,
            HasPassedLocalExam = false,
            LastOutcome = "Preparing",
            ScholarlyReputation = 12,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        educationModule.RegisterQueries(educationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(11)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        educationModule.RunMonth(new ModuleExecutionScope<EducationAndExamsState>(educationState, context));

        EducationPersonState student = educationState.People.Single();
        Assert.That(student.HasPassedLocalExam, Is.True);
        Assert.That(student.LastOutcome, Is.EqualTo("Passed"));
        Assert.That(student.LastExplanation, Does.Contain("得分"));
        Assert.That(student.LastExplanation, Does.Contain("塾望"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(EducationAndExamsEventNames.TutorSecured));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(EducationAndExamsEventNames.ExamPassed));
        Assert.That(student.LastResult, Is.EqualTo(ExamResult.Passed));
        Assert.That(student.CurrentTier, Is.EqualTo(ExamTier.PrefecturalExam));
        Assert.That(student.FallbackPath, Is.EqualTo(FallbackPath.ContinueStudy));
    }

    [Test]
    public void RunMonth_FailedExam_RoutesFallbackPathByClanAndShame()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Shaowu",
            Security = 48,
            Prosperity = 42,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Li",
            HomeSettlementId = new SettlementId(1),
            Prestige = 30,
            SupportReserve = 42,
            HeirPersonId = new PersonId(1),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Modest clerks",
            GrudgePressure = 40,
            FearPressure = 30,
            ShamePressure = 60,
            FavorBalance = 0,
        });

        EducationAndExamsModule educationModule = new();
        EducationAndExamsState educationState = educationModule.CreateInitialState();
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(1),
            AcademyName = "Shaowu Academy",
            IsOpen = true,
            Capacity = 2,
            Prestige = 20,
        });
        educationState.People.Add(new EducationPersonState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            AcademyId = new InstitutionId(1),
            DisplayName = "Li Rong",
            IsStudying = true,
            HasTutor = false,
            StudyProgress = 60,
            Stress = 40,
            ExamAttempts = 1,
            HasPassedLocalExam = false,
            LastOutcome = "Preparing",
            ScholarlyReputation = 6,
            CurrentTier = ExamTier.CountyExam,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        educationModule.RegisterQueries(educationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(3)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        educationModule.RunMonth(new ModuleExecutionScope<EducationAndExamsState>(educationState, context));

        EducationPersonState student = educationState.People.Single();
        Assert.That(student.HasPassedLocalExam, Is.False);
        Assert.That(student.LastResult, Is.AnyOf(ExamResult.Failed, ExamResult.Abandoned));
        Assert.That(student.FallbackPath, Is.Not.EqualTo(FallbackPath.Unknown));
        Assert.That(student.CurrentTier, Is.EqualTo(ExamTier.CountyExam));
    }
}
