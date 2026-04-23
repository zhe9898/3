using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

public sealed partial class OfficeAndCareerModuleTests
{
    [Test]
    public void RunXun_ZhongxunServingOfficeAbsorbsQueuePressureWithoutReadableOutput()
    {
        EducationAndExamsModule educationModule = new();
        EducationAndExamsState educationState = educationModule.CreateInitialState();
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(2),
            AcademyName = "Lanxi Academy",
            IsOpen = true,
            Capacity = 3,
            Prestige = 46,
        });
        educationState.People.Add(new EducationPersonState
        {
            PersonId = new PersonId(7),
            ClanId = new ClanId(5),
            AcademyId = new InstitutionId(1),
            DisplayName = "Zhang Yuan",
            IsStudying = false,
            HasTutor = true,
            TutorQuality = 14,
            StudyProgress = 36,
            Stress = 10,
            ExamAttempts = 1,
            HasPassedLocalExam = true,
            LastOutcome = "Passed",
            LastExplanation = "Passed the local exam and entered advanced study circles.",
            ScholarlyReputation = 38,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(5),
            PublicNarrative = "The clan is trusted in local scholarship.",
            GrudgePressure = 24,
            FearPressure = 48,
            ShamePressure = 8,
            FavorBalance = 20,
        });

        SettlementDisorderSnapshot disorder = new()
        {
            SettlementId = new SettlementId(2),
            BanditThreat = 52,
            RoutePressure = 60,
            SuppressionDemand = 62,
            DisorderPressure = 48,
            LastInterventionCommandCode = PlayerCommandNames.FundLocalWatch,
            LastInterventionCommandLabel = "添雇巡丁",
            LastInterventionSummary = "已雇巡丁看住津口与路牌。",
            LastInterventionOutcome = "本月先稳住沿路惊散。",
            InterventionCarryoverMonths = 1,
        };
        SettlementBlackRoutePressureSnapshot blackRoutePressure = new()
        {
            SettlementId = new SettlementId(2),
            BlackRoutePressure = 58,
            CoercionRisk = 44,
            PaperCompliance = 32,
            ImplementationDrag = 58,
            RouteShielding = 24,
            RetaliationRisk = 64,
        };
        LocalForcePoolSnapshot force = new()
        {
            SettlementId = new SettlementId(2),
            GuardCount = 14,
            RetainerCount = 4,
            MilitiaCount = 12,
            EscortCount = 6,
            Readiness = 42,
            CommandCapacity = 40,
            ResponseActivationLevel = 3,
            OrderSupportLevel = 5,
            IsResponseActivated = true,
            HasActiveConflict = true,
            LastConflictTrace = "Lanxi is still holding the road line.",
        };

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(7),
            ClanId = new ClanId(5),
            SettlementId = new SettlementId(2),
            DisplayName = "Zhang Yuan",
            IsEligible = true,
            HasAppointment = true,
            OfficeTitle = "涓荤翱",
            AuthorityTier = 2,
            JurisdictionLeverage = 46,
            PetitionPressure = 28,
            PetitionBacklog = 14,
            ServiceMonths = 12,
            PromotionMomentum = 30,
            DemotionPressure = 10,
            ClerkDependence = 18,
            CurrentAdministrativeTask = "鍕炬鎴风睄",
            AdministrativeTaskLoad = 16,
            OfficeReputation = 42,
            LastOutcome = "Serving",
            LastPetitionOutcome = "Triaged: 正在勾检户籍，诸状分轻重收理。",
            LastExplanation = "Office is stable.",
        });
        QueryRegistry queries = new();
        educationModule.RegisterQueries(educationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        StubOrderAndBanditryQueries orderQueries = new([disorder], [blackRoutePressure]);
        queries.Register<IOrderAndBanditryQueries>(orderQueries);
        queries.Register<IBlackRoutePressureQueries>(orderQueries);
        queries.Register<IConflictAndForceQueries>(new StubConflictAndForceQueries([force]));
        officeModule.RegisterQueries(officeState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);

        ModuleExecutionContext context = new(
            new GameDate(1201, 2),
            manifest,
            new DeterministicRandom(KernelState.Create(406)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Zhongxun);

        officeModule.RunXun(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));

        OfficeCareerState career = officeState.People.Single();
        JurisdictionAuthorityState jurisdiction = officeState.Jurisdictions.Single();

        Assert.That(career.HasAppointment, Is.True);
        Assert.That(career.AuthorityTier, Is.EqualTo(2));
        Assert.That(career.ServiceMonths, Is.EqualTo(12));
        Assert.That(career.PetitionBacklog, Is.GreaterThan(14));
        Assert.That(career.PetitionPressure, Is.GreaterThan(28));
        Assert.That(career.ClerkDependence, Is.GreaterThan(18));
        Assert.That(career.CurrentAdministrativeTask, Is.Not.Empty);
        Assert.That(career.LastPetitionOutcome, Does.Contain("Triaged"));
        Assert.That(career.LastExplanation, Is.EqualTo("Office is stable."));
        Assert.That(jurisdiction.CurrentAdministrativeTask, Is.EqualTo(career.CurrentAdministrativeTask));
        Assert.That(jurisdiction.PetitionBacklog, Is.EqualTo(career.PetitionBacklog));
        Assert.That(context.Diff.Entries, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunXun_XiaxunQueuedCandidateStaysUnappointedWhilePaperSurfaceCalms()
    {
        EducationAndExamsModule educationModule = new();
        EducationAndExamsState educationState = educationModule.CreateInitialState();
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(4),
            AcademyName = "Qingshui Academy",
            IsOpen = true,
            Capacity = 2,
            Prestige = 35,
        });
        educationState.People.Add(new EducationPersonState
        {
            PersonId = new PersonId(8),
            ClanId = new ClanId(6),
            AcademyId = new InstitutionId(1),
            DisplayName = "Li Wen",
            IsStudying = false,
            HasTutor = true,
            TutorQuality = 10,
            StudyProgress = 26,
            Stress = 16,
            ExamAttempts = 1,
            HasPassedLocalExam = true,
            LastOutcome = "Passed",
            LastExplanation = "Passed the local exam and is waiting for a path in.",
            ScholarlyReputation = 17,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(6),
            PublicNarrative = "The clan has some learning but thin patronage.",
            GrudgePressure = 14,
            FearPressure = 12,
            ShamePressure = 16,
            FavorBalance = 4,
        });

        SettlementDisorderSnapshot disorder = new()
        {
            SettlementId = new SettlementId(4),
            BanditThreat = 10,
            RoutePressure = 12,
            SuppressionDemand = 8,
            DisorderPressure = 12,
            LastPressureReason = "The county gate is calm this xiaxun.",
        };
        SettlementBlackRoutePressureSnapshot blackRoutePressure = new()
        {
            SettlementId = new SettlementId(4),
            BlackRoutePressure = 18,
            CoercionRisk = 12,
            PaperCompliance = 68,
            ImplementationDrag = 20,
            RouteShielding = 16,
            RetaliationRisk = 14,
        };

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(8),
            ClanId = new ClanId(6),
            SettlementId = new SettlementId(4),
            DisplayName = "Li Wen",
            IsEligible = true,
            HasAppointment = false,
            OfficeTitle = "鏈巿瀹?",
            AuthorityTier = 0,
            AppointmentPressure = 26,
            ClerkDependence = 14,
            PetitionPressure = 8,
            PetitionBacklog = 0,
            ServiceMonths = 0,
            PromotionMomentum = 0,
            DemotionPressure = 0,
            CurrentAdministrativeTask = "瀹堥€夊€欓槞",
            AdministrativeTaskLoad = 3,
            OfficeReputation = 20,
            LastOutcome = "鍊欑己",
            LastPetitionOutcome = "Unavailable: 未得官身，词状不得入案。",
            LastExplanation = "Still waiting in the queue.",
        });
        QueryRegistry queries = new();
        educationModule.RegisterQueries(educationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        StubOrderAndBanditryQueries orderQueries = new([disorder], [blackRoutePressure]);
        queries.Register<IOrderAndBanditryQueries>(orderQueries);
        queries.Register<IBlackRoutePressureQueries>(orderQueries);
        officeModule.RegisterQueries(officeState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);

        ModuleExecutionContext context = new(
            new GameDate(1201, 2),
            manifest,
            new DeterministicRandom(KernelState.Create(407)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Xiaxun);

        officeModule.RunXun(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));

        OfficeCareerState career = officeState.People.Single();

        Assert.That(career.HasAppointment, Is.False);
        Assert.That(career.AuthorityTier, Is.EqualTo(0));
        Assert.That(career.ServiceMonths, Is.EqualTo(0));
        Assert.That(career.AppointmentPressure, Is.GreaterThan(26));
        Assert.That(career.ClerkDependence, Is.LessThan(14));
        Assert.That(career.PetitionPressure, Is.LessThan(8));
        Assert.That(career.CurrentAdministrativeTask, Is.Not.Empty);
        Assert.That(officeState.Jurisdictions, Is.Empty);
        Assert.That(context.Diff.Entries, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

}
