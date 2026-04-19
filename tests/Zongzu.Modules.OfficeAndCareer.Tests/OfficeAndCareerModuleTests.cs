using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

[TestFixture]
public sealed class OfficeAndCareerModuleTests
{
    [Test]
    public void RunMonth_GrantsExplainableAppointmentAndBuildsJurisdiction()
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
            StudyProgress = 28,
            Stress = 12,
            ExamAttempts = 1,
            HasPassedLocalExam = true,
            LastOutcome = "Passed",
            LastExplanation = "Passed the local exam last season.",
            ScholarlyReputation = 28,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(5),
            PublicNarrative = "The clan is trusted in local scholarship.",
            GrudgePressure = 18,
            FearPressure = 14,
            ShamePressure = 10,
            FavorBalance = 16,
        });

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();

        QueryRegistry queries = new();
        educationModule.RegisterQueries(educationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        officeModule.RegisterQueries(officeState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            manifest,
            new DeterministicRandom(KernelState.Create(401)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        officeModule.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));

        OfficeCareerState career = officeState.People.Single();
        JurisdictionAuthorityState jurisdiction = officeState.Jurisdictions.Single();
        IOfficeAndCareerQueries officeQueries = queries.GetRequired<IOfficeAndCareerQueries>();
        OfficeCareerSnapshot snapshot = officeQueries.GetRequiredCareer(new PersonId(7));

        Assert.That(career.HasAppointment, Is.True);
        Assert.That(career.OfficeTitle, Is.Not.EqualTo("未授官"));
        Assert.That(career.AuthorityTier, Is.GreaterThan(0));
        Assert.That(career.JurisdictionLeverage, Is.GreaterThan(0));
        Assert.That(career.ServiceMonths, Is.EqualTo(1));
        Assert.That(career.CurrentAdministrativeTask, Is.Not.EqualTo("候补听选"));
        Assert.That(career.PetitionBacklog, Is.GreaterThanOrEqualTo(0));
        Assert.That(career.LastOutcome, Is.EqualTo("Granted"));
        Assert.That(career.LastPetitionOutcome, Does.Contain("新任甫定"));
        Assert.That(career.LastExplanation, Does.Contain("场屋得捷"));
        Assert.That(career.LastExplanation, Does.Contain("首任"));
        Assert.That(jurisdiction.SettlementId, Is.EqualTo(new SettlementId(2)));
        Assert.That(jurisdiction.LeadOfficialPersonId, Is.EqualTo(new PersonId(7)));
        Assert.That(jurisdiction.JurisdictionLeverage, Is.EqualTo(career.JurisdictionLeverage));
        Assert.That(jurisdiction.CurrentAdministrativeTask, Is.EqualTo(career.CurrentAdministrativeTask));
        Assert.That(jurisdiction.LastPetitionOutcome, Is.EqualTo(career.LastPetitionOutcome));
        Assert.That(snapshot.OfficeTitle, Is.EqualTo(career.OfficeTitle));
        Assert.That(snapshot.HasAppointment, Is.True);
        Assert.That(snapshot.ServiceMonths, Is.EqualTo(1));
        Assert.That(snapshot.CurrentAdministrativeTask, Is.EqualTo(career.CurrentAdministrativeTask));
        Assert.That(snapshot.AdministrativeTaskTier, Is.Not.Empty);
        Assert.That(snapshot.PetitionOutcomeCategory, Is.EqualTo("Queued"));
        Assert.That(snapshot.AuthorityTrajectorySummary, Is.Not.Empty);
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("OfficeGranted"));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
        Assert.That(officeModule.AcceptedCommands, Does.Contain("PursuePosting"));
        Assert.That(officeModule.PublishedEvents, Does.Contain("AuthorityChanged"));
    }

    [Test]
    public void RunMonth_UnpassedCandidate_RemainsUnappointed()
    {
        EducationAndExamsModule educationModule = new();
        EducationAndExamsState educationState = educationModule.CreateInitialState();
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(3),
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
            IsStudying = true,
            HasTutor = false,
            TutorQuality = 0,
            StudyProgress = 42,
            Stress = 28,
            ExamAttempts = 0,
            HasPassedLocalExam = false,
            LastOutcome = "Preparing",
            LastExplanation = "Still studying.",
            ScholarlyReputation = 11,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(6),
            PublicNarrative = "The clan is still waiting for recognition.",
            GrudgePressure = 22,
            FearPressure = 17,
            ShamePressure = 20,
            FavorBalance = 4,
        });

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();

        QueryRegistry queries = new();
        educationModule.RegisterQueries(educationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        officeModule.RegisterQueries(officeState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(402)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        officeModule.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));

        OfficeCareerState career = officeState.People.Single();

        Assert.That(career.HasAppointment, Is.False);
        Assert.That(career.IsEligible, Is.False);
        Assert.That(career.AuthorityTier, Is.EqualTo(0));
        Assert.That(career.LastOutcome, Is.EqualTo("观望"));
        Assert.That(career.CurrentAdministrativeTask, Is.EqualTo("候补听选"));
        Assert.That(career.ServiceMonths, Is.EqualTo(0));
        Assert.That(career.LastPetitionOutcome, Does.Contain("未得官身"));
        Assert.That(career.LastExplanation, Does.Contain("暂未得官"));
        Assert.That(officeState.Jurisdictions, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_PassedCandidateWithoutStrongBacking_EntersQueuedOfficePathBeforeAppointment()
    {
        EducationAndExamsModule educationModule = new();
        EducationAndExamsState educationState = educationModule.CreateInitialState();
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(4),
            AcademyName = "Bianzhou County School",
            IsOpen = true,
            Capacity = 2,
            Prestige = 32,
        });
        educationState.People.Add(new EducationPersonState
        {
            PersonId = new PersonId(9),
            ClanId = new ClanId(7),
            AcademyId = new InstitutionId(1),
            DisplayName = "Sun Qing",
            IsStudying = false,
            HasTutor = true,
            TutorQuality = 10,
            StudyProgress = 26,
            Stress = 18,
            ExamAttempts = 1,
            HasPassedLocalExam = true,
            LastOutcome = "Passed",
            LastExplanation = "Passed the local exam but lacks strong backing.",
            ScholarlyReputation = 17,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(7),
            PublicNarrative = "The clan has some learning but thin patronage.",
            GrudgePressure = 14,
            FearPressure = 12,
            ShamePressure = 18,
            FavorBalance = 2,
        });

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();

        QueryRegistry queries = new();
        educationModule.RegisterQueries(educationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        officeModule.RegisterQueries(officeState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(452)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        officeModule.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));

        OfficeCareerState career = officeState.People.Single();

        Assert.That(career.IsEligible, Is.True);
        Assert.That(career.HasAppointment, Is.False);
        Assert.That(career.AppointmentPressure, Is.GreaterThan(0));
        Assert.That(career.ClerkDependence, Is.GreaterThanOrEqualTo(0));
        Assert.That(career.LastOutcome, Is.AnyOf("候缺", "听差"));
        Assert.That(career.CurrentAdministrativeTask, Is.Not.Empty);
        Assert.That(career.LastExplanation, Does.Contain("场屋已捷"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Not.Contain("OfficeGranted"));
    }

    [Test]
    public void RunMonth_StrongServingOfficial_PromotesThroughAdministrativeService()
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
            GrudgePressure = 18,
            FearPressure = 14,
            ShamePressure = 8,
            FavorBalance = 20,
        });

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
            OfficeTitle = "书吏",
            AuthorityTier = 1,
            JurisdictionLeverage = 28,
            PetitionPressure = 24,
            PetitionBacklog = 10,
            ServiceMonths = 11,
            PromotionMomentum = 24,
            DemotionPressure = 6,
            CurrentAdministrativeTask = "誊黄封牍",
            AdministrativeTaskLoad = 16,
            OfficeReputation = 42,
            LastOutcome = "Serving",
            LastPetitionOutcome = "分轻重：正在誊黄封牍，诸状分轻重收理。",
            LastExplanation = "Steady clerkship in Lanxi.",
        });

        QueryRegistry queries = new();
        educationModule.RegisterQueries(educationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        officeModule.RegisterQueries(officeState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1201, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(403)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        officeModule.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));

        OfficeCareerState career = officeState.People.Single();

        Assert.That(career.HasAppointment, Is.True);
        Assert.That(career.AuthorityTier, Is.GreaterThan(1));
        Assert.That(career.OfficeTitle, Is.Not.EqualTo("书吏"));
        Assert.That(career.ServiceMonths, Is.EqualTo(12));
        Assert.That(career.LastOutcome, Is.EqualTo("Promoted"));
        Assert.That(career.LastPetitionOutcome, Does.Contain("已清"));
        Assert.That(career.PromotionMomentum, Is.GreaterThanOrEqualTo(24));
        Assert.That(career.DemotionPressure, Is.LessThan(10));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("AuthorityChanged"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("OfficeTransfer"));
        Assert.That(officeState.Jurisdictions.Single().LeadOfficeTitle, Is.EqualTo(career.OfficeTitle));
    }

    [Test]
    public void RunMonth_OverwhelmedOfficial_LosesAppointmentWhenPetitionsCollapse()
    {
        EducationAndExamsModule educationModule = new();
        EducationAndExamsState educationState = educationModule.CreateInitialState();
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(3),
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
            HasTutor = false,
            TutorQuality = 0,
            StudyProgress = 42,
            Stress = 44,
            ExamAttempts = 3,
            HasPassedLocalExam = true,
            LastOutcome = "Passed",
            LastExplanation = "Won a posting but is overextended.",
            ScholarlyReputation = 12,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(6),
            PublicNarrative = "The clan is losing public confidence.",
            GrudgePressure = 48,
            FearPressure = 40,
            ShamePressure = 36,
            FavorBalance = 2,
        });

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(8),
            ClanId = new ClanId(6),
            SettlementId = new SettlementId(3),
            DisplayName = "Li Wen",
            IsEligible = true,
            HasAppointment = true,
            OfficeTitle = "主簿",
            AuthorityTier = 2,
            JurisdictionLeverage = 26,
            PetitionPressure = 72,
            PetitionBacklog = 60,
            ServiceMonths = 14,
            PromotionMomentum = 12,
            DemotionPressure = 65,
            CurrentAdministrativeTask = "勘理词状",
            AdministrativeTaskLoad = 38,
            OfficeReputation = 18,
            LastOutcome = "Serving",
            LastPetitionOutcome = "稽延：正在勘理词状，其余词牍暂缓。",
            LastExplanation = "The docket is slipping out of control.",
        });

        QueryRegistry queries = new();
        educationModule.RegisterQueries(educationState, queries);
        socialModule.RegisterQueries(socialState, queries);
        officeModule.RegisterQueries(officeState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1201, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(404)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        officeModule.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));

        OfficeCareerState career = officeState.People.Single();

        Assert.That(career.HasAppointment, Is.False);
        Assert.That(career.AuthorityTier, Is.EqualTo(0));
        Assert.That(career.OfficeTitle, Is.EqualTo("未授官"));
        Assert.That(career.LastOutcome, Is.EqualTo("Lost"));
        Assert.That(career.CurrentAdministrativeTask, Is.EqualTo("候补听选"));
        Assert.That(career.LastExplanation, Does.Contain("遂失官身"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("OfficeLost"));
        Assert.That(officeState.Jurisdictions, Is.Empty);
    }

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

    [Test]
    public void RunMonth_RecentOrderInterventionCarryoverPressesServingOfficeThroughQueries()
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
            GrudgePressure = 18,
            FearPressure = 16,
            ShamePressure = 8,
            FavorBalance = 20,
        });

        SettlementDisorderSnapshot disorder = new()
        {
            SettlementId = new SettlementId(2),
            BanditThreat = 46,
            RoutePressure = 58,
            SuppressionDemand = 62,
            DisorderPressure = 44,
            LastInterventionCommandCode = PlayerCommandNames.SuppressBanditry,
            LastInterventionCommandLabel = "严缉路匪",
            LastInterventionSummary = "已遣差缉拿路上匪徒。",
            LastInterventionOutcome = "本月先压住路上惊扰。",
            InterventionCarryoverMonths = 1,
        };
        SettlementBlackRoutePressureSnapshot blackRoutePressure = new()
        {
            SettlementId = new SettlementId(2),
            BlackRoutePressure = 66,
            CoercionRisk = 52,
            PaperCompliance = 40,
            ImplementationDrag = 54,
            RouteShielding = 18,
            RetaliationRisk = 78,
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
            OfficeTitle = "主簿",
            AuthorityTier = 2,
            JurisdictionLeverage = 46,
            PetitionPressure = 24,
            PetitionBacklog = 10,
            ServiceMonths = 12,
            PromotionMomentum = 30,
            DemotionPressure = 10,
            ClerkDependence = 20,
            CurrentAdministrativeTask = "勾检户籍",
            AdministrativeTaskLoad = 16,
            OfficeReputation = 42,
            LastOutcome = "Serving",
            LastPetitionOutcome = "Triaged: 正在勾检户籍，诸状分轻重收理。",
            LastExplanation = "Office is stable.",
        });
        OfficeAndCareerState controlOfficeState = officeModule.CreateInitialState();
        controlOfficeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(7),
            ClanId = new ClanId(5),
            SettlementId = new SettlementId(2),
            DisplayName = "Zhang Yuan",
            IsEligible = true,
            HasAppointment = true,
            OfficeTitle = "主簿",
            AuthorityTier = 2,
            JurisdictionLeverage = 46,
            PetitionPressure = 24,
            PetitionBacklog = 10,
            ServiceMonths = 12,
            PromotionMomentum = 30,
            DemotionPressure = 10,
            ClerkDependence = 20,
            CurrentAdministrativeTask = "勾检户籍",
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
        officeModule.RegisterQueries(officeState, queries);

        QueryRegistry controlQueries = new();
        educationModule.RegisterQueries(educationState, controlQueries);
        socialModule.RegisterQueries(socialState, controlQueries);
        officeModule.RegisterQueries(controlOfficeState, controlQueries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        FeatureManifest controlManifest = new();
        controlManifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        controlManifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        controlManifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);

        ModuleExecutionContext context = new(
            new GameDate(1201, 2),
            manifest,
            new DeterministicRandom(KernelState.Create(405)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());
        ModuleExecutionContext controlContext = new(
            new GameDate(1201, 2),
            controlManifest,
            new DeterministicRandom(KernelState.Create(405)),
            controlQueries,
            new DomainEventBuffer(),
            new WorldDiff());

        officeModule.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(officeState, context));
        officeModule.RunMonth(new ModuleExecutionScope<OfficeAndCareerState>(controlOfficeState, controlContext));

        OfficeCareerState career = officeState.People.Single();
        OfficeCareerState controlCareer = controlOfficeState.People.Single();
        JurisdictionAuthorityState jurisdiction = officeState.Jurisdictions.Single();

        Assert.That(career.HasAppointment, Is.True);
        Assert.That(career.CurrentAdministrativeTask, Is.EqualTo("勘解乡怨词牒"));
        Assert.That(career.PetitionBacklog, Is.GreaterThan(controlCareer.PetitionBacklog));
        Assert.That(career.PetitionPressure, Is.GreaterThan(controlCareer.PetitionPressure));
        Assert.That(career.DemotionPressure, Is.GreaterThan(controlCareer.DemotionPressure));
        Assert.That(career.LastPetitionOutcome, Does.Contain("上月严缉路匪"));
        Assert.That(career.LastExplanation, Does.Contain("上月严缉路匪"));
        Assert.That(jurisdiction.CurrentAdministrativeTask, Is.EqualTo(career.CurrentAdministrativeTask));
        Assert.That(jurisdiction.PetitionBacklog, Is.EqualTo(career.PetitionBacklog));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
    }

    [Test]
    public void HandleEvents_PushesOfficeBacklogAndRebuildsJurisdictionFromCampaignSpillover()
    {
        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(8),
            ClanId = new ClanId(6),
            SettlementId = new SettlementId(3),
            DisplayName = "Li Wen",
            IsEligible = true,
            HasAppointment = true,
            OfficeTitle = "主簿",
            AuthorityTier = 2,
            JurisdictionLeverage = 44,
            PetitionPressure = 26,
            PetitionBacklog = 12,
            ServiceMonths = 14,
            PromotionMomentum = 28,
            DemotionPressure = 14,
            CurrentAdministrativeTask = "勾检户籍",
            AdministrativeTaskLoad = 16,
            OfficeReputation = 38,
            LastOutcome = "Serving",
            LastPetitionOutcome = "Triaged: 正在勾检户籍，诸状分轻重收理。",
            LastExplanation = "Office is stable.",
        });
        officeState.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(officeState.People);

        QueryRegistry queries = new();
        officeModule.RegisterQueries(officeState, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(3),
                AnchorSettlementName = "Qingshui",
                CampaignName = "Qingshui军务沙盘",
                IsActive = true,
                MobilizedForceCount = 42,
                FrontPressure = 70,
                FrontLabel = "前线转紧",
                SupplyState = 33,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 44,
                MoraleStateLabel = "军心浮动",
                CommandFitLabel = "号令尚整",
                CommanderSummary = "Qingshui command is holding.",
                ActiveDirectiveCode = WarfareCampaignCommandNames.ProtectSupplyLine,
                ActiveDirectiveLabel = "催督粮道",
                ActiveDirectiveSummary = "先护粮道。",
                LastDirectiveTrace = "清水已受催督粮道之令。",
                MobilizationWindowLabel = "可发",
                SupplyLineSummary = "粮站四周词状与催运俱聚。",
                OfficeCoordinationTrace = "主簿正在看顾军务文移。",
                SourceTrace = "Campaign pressure rose from local conflict.",
                LastAftermathSummary = "战后覆核把文移与粮运一并压紧。",
            },
        ]));

        DomainEventRecord[] events =
        {
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignPressureRaised, "Qingshui pressure rose.", "3"),
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Qingshui supply strained.", "3"),
        };

        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7203)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        officeModule.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(officeState, context, events));

        OfficeCareerState career = officeState.People.Single();
        JurisdictionAuthorityState jurisdiction = officeState.Jurisdictions.Single();

        Assert.That(career.PetitionBacklog, Is.GreaterThan(12));
        Assert.That(career.PetitionPressure, Is.GreaterThan(26));
        Assert.That(career.CurrentAdministrativeTask, Is.EqualTo("急牍覆核"));
        Assert.That(career.LastPetitionOutcome, Does.Contain("案牍骤涌"));
        Assert.That(career.LastExplanation, Does.Contain("战事外溢"));
        Assert.That(jurisdiction.CurrentAdministrativeTask, Is.EqualTo(career.CurrentAdministrativeTask));
        Assert.That(jurisdiction.PetitionBacklog, Is.EqualTo(career.PetitionBacklog));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }
    }

    private sealed class StubConflictAndForceQueries : IConflictAndForceQueries
    {
        private readonly IReadOnlyList<LocalForcePoolSnapshot> _forces;

        public StubConflictAndForceQueries(IReadOnlyList<LocalForcePoolSnapshot> forces)
        {
            _forces = forces;
        }

        public LocalForcePoolSnapshot GetRequiredSettlementForce(SettlementId settlementId)
        {
            return _forces.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<LocalForcePoolSnapshot> GetSettlementForces()
        {
            return _forces;
        }
    }

    private sealed class StubOrderAndBanditryQueries : IOrderAndBanditryQueries, IBlackRoutePressureQueries
    {
        private readonly IReadOnlyList<SettlementDisorderSnapshot> _disorders;
        private readonly IReadOnlyList<SettlementBlackRoutePressureSnapshot> _pressures;

        public StubOrderAndBanditryQueries(
            IReadOnlyList<SettlementDisorderSnapshot> disorders,
            IReadOnlyList<SettlementBlackRoutePressureSnapshot> pressures)
        {
            _disorders = disorders;
            _pressures = pressures;
        }

        public SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId)
        {
            return _disorders.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder()
        {
            return _disorders;
        }

        public SettlementBlackRoutePressureSnapshot GetRequiredSettlementBlackRoutePressure(SettlementId settlementId)
        {
            return _pressures.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementBlackRoutePressureSnapshot> GetSettlementBlackRoutePressures()
        {
            return _pressures;
        }
    }
}
