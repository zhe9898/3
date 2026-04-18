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
                LastDirectiveTrace = "Qingshui is protecting supply lines.",
                MobilizationWindowLabel = "Open",
                SupplyLineSummary = "Petitions cluster around grain depots.",
                OfficeCoordinationTrace = "Registrar is coordinating wartime filings.",
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
        Assert.That(career.LastExplanation, Does.Contain("Campaign spillover"));
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
}
