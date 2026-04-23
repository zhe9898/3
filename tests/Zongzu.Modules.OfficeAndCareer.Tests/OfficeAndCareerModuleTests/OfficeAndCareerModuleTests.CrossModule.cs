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

}
