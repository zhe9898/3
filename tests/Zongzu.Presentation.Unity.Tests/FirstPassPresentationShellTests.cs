using System.IO;
using System.Linq;
using System.Reflection;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

[TestFixture]
public sealed class FirstPassPresentationShellTests
{
    [Test]
    public void Compose_BuildsFirstPassShellFromReadModelBundle()
    {
        PresentationReadModelBundle bundle = new()
        {
            CurrentDate = new GameDate(1200, 4),
            ReplayHash = "abc123",
            Clans =
            [
                new ClanSnapshot
                {
                    Id = new ClanId(1),
                    ClanName = "Zhang",
                    HomeSettlementId = new SettlementId(1),
                    Prestige = 60,
                    SupportReserve = 58,
                    HeirPersonId = new PersonId(1),
                },
            ],
            Settlements =
            [
                new SettlementSnapshot
                {
                    Id = new SettlementId(1),
                    Name = "Lanxi",
                    Security = 62,
                    Prosperity = 64,
                },
            ],
            PopulationSettlements =
            [
                new PopulationSettlementSnapshot
                {
                    SettlementId = new SettlementId(1),
                    CommonerDistress = 36,
                    LaborSupply = 108,
                    MigrationPressure = 19,
                    MilitiaPotential = 72,
                },
            ],
            Academies =
            [
                new AcademySnapshot
                {
                    Id = new InstitutionId(1),
                    SettlementId = new SettlementId(1),
                    AcademyName = "兰溪义塾",
                    IsOpen = true,
                    Capacity = 3,
                    Prestige = 44,
                },
            ],
            EducationCandidates =
            [
                new EducationCandidateSnapshot
                {
                    PersonId = new PersonId(1),
                    ClanId = new ClanId(1),
                    AcademyId = new InstitutionId(1),
                    DisplayName = "Zhang Yuan",
                    IsStudying = true,
                    HasTutor = true,
                    StudyProgress = 74,
                    Stress = 12,
                    ExamAttempts = 1,
                    HasPassedLocalExam = false,
                    LastOutcome = "Preparing",
                    LastExplanation = "本月课业安稳。",
                    ScholarlyReputation = 14,
                },
            ],
            ClanTrades =
            [
                new ClanTradeSnapshot
                {
                    ClanId = new ClanId(1),
                    PrimarySettlementId = new SettlementId(1),
                    CashReserve = 90,
                    GrainReserve = 70,
                    Debt = 12,
                    CommerceReputation = 31,
                    ShopCount = 1,
                    LastOutcome = "Profit",
                    LastExplanation = "本月市利略进。",
                },
            ],
            Markets =
            [
                new MarketSnapshot
                {
                    SettlementId = new SettlementId(1),
                    MarketName = "兰溪早市",
                    PriceIndex = 107,
                    Demand = 69,
                    LocalRisk = 15,
                },
            ],
            TradeRoutes =
            [
                new TradeRouteSnapshot
                {
                    RouteId = 1,
                    ClanId = new ClanId(1),
                    RouteName = "兰溪河埠",
                    SettlementId = new SettlementId(1),
                    IsActive = true,
                    Capacity = 26,
                    Risk = 18,
                    LastMargin = 8,
                },
            ],
            OfficeCareers =
            [
                new OfficeCareerSnapshot
                {
                    PersonId = new PersonId(1),
                    ClanId = new ClanId(1),
                    SettlementId = new SettlementId(1),
                    DisplayName = "Zhang Yuan",
                    IsEligible = true,
                    HasAppointment = true,
                    OfficeTitle = "主簿",
                    AuthorityTier = 2,
                    JurisdictionLeverage = 61,
                    PetitionPressure = 26,
                    PetitionBacklog = 9,
                    ServiceMonths = 14,
                    PromotionMomentum = 33,
                    DemotionPressure = 8,
                    CurrentAdministrativeTask = "勘理词状",
                    AdministrativeTaskTier = "district",
                    AdministrativeTaskLoad = 18,
                    OfficeReputation = 52,
                    LastOutcome = "Serving",
                    LastPetitionOutcome = "分轻重：正在勘理词状，诸状分轻重收理。",
                    PetitionOutcomeCategory = "Triaged",
                    PromotionPressureLabel = "steady",
                    DemotionPressureLabel = "stable",
                    AuthorityTrajectorySummary = "官途暂守，升势持平，黜压平稳。",
                    LastExplanation = "兰溪任上供职安稳。",
                },
            ],
            OfficeJurisdictions =
            [
                new JurisdictionAuthoritySnapshot
                {
                    SettlementId = new SettlementId(1),
                    LeadOfficialPersonId = new PersonId(1),
                    LeadOfficialName = "Zhang Yuan",
                    LeadOfficeTitle = "主簿",
                    AuthorityTier = 2,
                    JurisdictionLeverage = 61,
                    PetitionPressure = 26,
                    PetitionBacklog = 9,
                    CurrentAdministrativeTask = "勘理词状",
                    AdministrativeTaskTier = "district",
                    LastPetitionOutcome = "分轻重：正在勘理词状，诸状分轻重收理。",
                    PetitionOutcomeCategory = "Triaged",
                    LastAdministrativeTrace = "兰溪词状尚能分轻重收理。",
                },
            ],
            Campaigns =
            [
                new CampaignFrontSnapshot
                {
                    CampaignId = new CampaignId(1),
                    AnchorSettlementId = new SettlementId(1),
                    AnchorSettlementName = "Lanxi",
                    CampaignName = "Lanxi军务沙盘",
                    IsActive = true,
                    ObjectiveSummary = "守住 Lanxi 前缘与渡口，护住粮道，不使地方冲突外溢，同时维持可开动员窗口。",
                    MobilizedForceCount = 44,
                    FrontPressure = 63,
                    FrontLabel = "前线吃紧",
                    SupplyState = 57,
                    SupplyStateLabel = "粮道可守",
                    MoraleState = 52,
                    MoraleStateLabel = "军心浮动",
                    CommandFitLabel = "号令相接",
                    CommanderSummary = "Registrar Zhang Yuan正以辖区杠杆61催督文移，使Lanxi守丁14、乡勇18与护运8维持号令相接。",
                    ActiveDirectiveCode = WarfareCampaignCommandNames.ProtectSupplyLine,
                    ActiveDirectiveLabel = "催督粮道",
                    ActiveDirectiveSummary = "先护Lanxi粮道、渡口与文移驿线，不与前锋争一时之气。",
                    LastDirectiveTrace = "Lanxi当前军令为催督粮道：先护Lanxi粮道、渡口与文移驿线，不与前锋争一时之气。 其势表现为前线吃紧、粮道可守、军心浮动。",
                    MobilizationWindowLabel = "Open",
                    SupplyLineSummary = "Lanxi粮道由护运8维持可守之势；繁荣64，并有Registrar杠杆61，积案9。",
                    OfficeCoordinationTrace = "Registrar Zhang Yuan正在协调district层级文移，积案9。",
                    SourceTrace = "Lanxi军务态势取材于可调之众44、整备49、统摄33、治安62、支援7，并维持号令相接。",
                    LastAftermathSummary = "Lanxi军务态势尚能持守，有Registrar接应；前线63，粮道57，军心52。",
                    Routes =
                    [
                        new CampaignRouteSnapshot
                        {
                            RouteLabel = "粮道",
                            RouteRole = "supply",
                            Pressure = 48,
                            Security = 74,
                            FlowStateLabel = "粮运畅通",
                            Summary = "8 escorts are keeping stores moving for Lanxi. 当前为粮运畅通，压力48，护持74。",
                        },
                        new CampaignRouteSnapshot
                        {
                            RouteLabel = "文移驿线",
                            RouteRole = "administrative",
                            Pressure = 39,
                            Security = 65,
                            FlowStateLabel = "粮运畅通",
                            Summary = "Registrar couriers are tying docket traffic into the campaign board. 当前为粮运畅通，压力39，护持65。",
                        },
                    ],
                },
            ],
            CampaignMobilizationSignals =
            [
                new CampaignMobilizationSignalSnapshot
                {
                    SettlementId = new SettlementId(1),
                    SettlementName = "Lanxi",
                    ResponseActivationLevel = 29,
                    CommandCapacity = 33,
                    Readiness = 49,
                    AvailableForceCount = 44,
                    OrderSupportLevel = 7,
                    OfficeAuthorityTier = 2,
                    AdministrativeLeverage = 61,
                    PetitionBacklog = 9,
                    CommandFitLabel = "号令相接",
                    ActiveDirectiveCode = WarfareCampaignCommandNames.ProtectSupplyLine,
                    ActiveDirectiveLabel = "催督粮道",
                    ActiveDirectiveSummary = "先护Lanxi粮道、渡口与文移驿线，不与前锋争一时之气。",
                    MobilizationWindowLabel = "Open",
                    OfficeCoordinationTrace = "Registrar Zhang Yuan正在协调district层级文移，积案9。",
                    SourceTrace = "Lanxi军务态势取材于可调之众44、整备49、统摄33、治安62、支援7，并维持号令相接。",
                },
            ],
            Notifications =
            [
                new NarrativeNotificationSnapshot
                {
                    Id = new NotificationId(1),
                    CreatedAt = new GameDate(1200, 4),
                    Tier = NotificationTier.Consequential,
                    Surface = NarrativeSurface.DeskSandbox,
                    Title = "市利有进",
                    Summary = "张氏商账本月见利。",
                    WhyItHappened = "市需尚旺，路面安稳。",
                    WhatNext = "先看案上商路与市肆风色，再定守利还是添本。",
                    SourceModuleKey = KnownModuleKeys.TradeAndIndustry,
                    Traces =
                    [
                        new NotificationTraceSnapshot
                        {
                            SourceModuleKey = KnownModuleKeys.TradeAndIndustry,
                            EventType = "TradeProspered",
                            EventSummary = "张氏商账本月见利。",
                            DiffDescription = "Margin improved from demand and route stability.",
                            EntityKey = "1",
                        },
                    ],
                },
            ],
            Debug = new PresentationDebugSnapshot
            {
                DiagnosticsSchemaVersion = 1,
                InitialSeed = 12345,
                NotificationRetentionLimit = 40,
                RetentionLimitReached = false,
                LatestMetrics = new ObservabilityMetricsSnapshot
                {
                    DiffEntryCount = 3,
                    DomainEventCount = 2,
                    NotificationCount = 1,
                    SavePayloadBytes = 512,
                },
                CurrentInteractionPressure = new InteractionPressureMetricsSnapshot
                {
                    ActiveConflictSettlements = 1,
                    ActivatedResponseSettlements = 1,
                    SupportedOrderSettlements = 1,
                    HighSuppressionDemandSettlements = 0,
                    AverageSuppressionDemand = 25,
                    PeakSuppressionDemand = 25,
                    HighBanditThreatSettlements = 0,
                },
                CurrentPressureDistribution = new SettlementPressureDistributionSnapshot
                {
                    CalmSettlements = 0,
                    WatchedSettlements = 1,
                    StressedSettlements = 0,
                    CrisisSettlements = 0,
                },
                CurrentScale = new RuntimeScaleMetricsSnapshot
                {
                    EnabledModuleCount = 7,
                    SavedModuleCount = 7,
                    SettlementCount = 1,
                    ClanCount = 1,
                    HouseholdCount = 2,
                    AcademyCount = 1,
                    RouteCount = 1,
                    NotificationCount = 1,
                    NotificationUtilizationPercent = 2,
                    SavePayloadBytesPerSettlement = 512,
                    AverageHouseholdsPerSettlement = 2,
                },
                CurrentHotspots =
                [
                    new SettlementInteractionHotspotSnapshot
                    {
                        SettlementId = new SettlementId(1),
                        SettlementName = "Lanxi",
                        HotspotScore = 121,
                        BanditThreat = 30,
                        RoutePressure = 28,
                        SuppressionDemand = 25,
                        ResponseActivationLevel = 22,
                        OrderSupportLevel = 7,
                        HasActiveConflict = true,
                        IsResponseActivated = true,
                    },
                ],
                CurrentPayloadSummary = new RuntimePayloadSummarySnapshot
                {
                    TotalModulePayloadBytes = 512,
                    LargestModuleKey = KnownModuleKeys.NarrativeProjection,
                    LargestModulePayloadBytes = 256,
                    LargestModuleShareBasisPoints = 5000,
                },
                TopPayloadModules =
                [
                    new ModulePayloadFootprintSnapshot
                    {
                        ModuleKey = KnownModuleKeys.NarrativeProjection,
                        PayloadBytes = 256,
                        PayloadShareBasisPoints = 5000,
                    },
                ],
                LoadMigration = new DebugLoadMigrationSnapshot
                {
                    LoadOriginLabel = "SaveLoad",
                    WasMigrationApplied = true,
                    StepCount = 1,
                    ConsistencyPassed = false,
                    Summary = "Loaded save required 1 migration step(s) before simulation resumed.",
                    ConsistencySummary = "1/1 enabled modules, 1/1 module envelopes preserved.",
                    Steps =
                    [
                        new DebugMigrationStepSnapshot
                        {
                            ScopeLabel = KnownModuleKeys.ConflictAndForce,
                            SourceVersion = 1,
                            TargetVersion = 2,
                        },
                    ],
                    Warnings =
                    [
                        "Enabled module key set changed during migration preparation.",
                    ],
                },
                EnabledModules =
                [
                    new DebugFeatureModeSnapshot
                    {
                        ModuleKey = KnownModuleKeys.NarrativeProjection,
                        Mode = "full",
                    },
                ],
                ModuleInspectors =
                [
                    new DebugModuleInspectorSnapshot
                    {
                        ModuleKey = KnownModuleKeys.NarrativeProjection,
                        ModuleSchemaVersion = 1,
                        PayloadBytes = 256,
                    },
                ],
                RecentDiffEntries =
                [
                    new DebugDiffTraceSnapshot
                    {
                        ModuleKey = KnownModuleKeys.TradeAndIndustry,
                        Description = "Margin improved from demand and route stability.",
                        EntityKey = "1",
                    },
                ],
                RecentDomainEvents =
                [
                    new DebugDomainEventSnapshot
                    {
                        ModuleKey = KnownModuleKeys.TradeAndIndustry,
                        EventType = "TradeProspered",
                        Summary = "Clan Zhang trade prospered.",
                    },
                ],
                Warnings =
                [
                    "Urgent notifications are pending review.",
                ],
                Invariants =
                [
                    "Module boundary validation passed.",
                ],
            },
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.CurrentDateLabel, Is.EqualTo("1200-04"));
        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("Zhang"));
        Assert.That(shell.GreatHall.GovernanceSummary, Does.Contain("人在官途"));
        Assert.That(shell.GreatHall.GovernanceSummary, Does.Contain("分轻重"));
        Assert.That(shell.GreatHall.WarfareSummary, Does.Contain("在案行营"));
        Assert.That(shell.GreatHall.WarfareSummary, Does.Contain("粮筹压阵"));
        Assert.That(shell.Lineage.Clans, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].AcademySummary, Does.Contain("兰溪义塾"));
        Assert.That(shell.DeskSandbox.Settlements[0].GovernanceSummary, Does.Contain("主簿"));
        Assert.That(shell.DeskSandbox.Settlements[0].GovernanceSummary, Does.Contain("州县差遣"));
        Assert.That(shell.DeskSandbox.Settlements[0].CampaignSummary, Does.Contain("Lanxi军务沙盘"));
        Assert.That(shell.DeskSandbox.Settlements[0].CampaignSummary, Does.Contain("粮筹压阵"));
        Assert.That(shell.Office.Summary, Does.Contain("官人"));
        Assert.That(shell.Office.Appointments, Has.Count.EqualTo(1));
        Assert.That(shell.Office.Appointments[0].ServiceSummary, Does.Contain("升势持平"));
        Assert.That(shell.Office.Appointments[0].TaskSummary, Is.EqualTo("州县差遣：勘理词状"));
        Assert.That(shell.Office.Appointments[0].PressureSummary, Does.Contain("官途暂守"));
        Assert.That(shell.Office.Appointments[0].PetitionOutcomeCategory, Is.EqualTo("分轻重"));
        Assert.That(shell.Office.Jurisdictions[0].SettlementLabel, Is.EqualTo("Lanxi"));
        Assert.That(shell.Office.Jurisdictions[0].TaskSummary, Is.EqualTo("州县差遣：勘理词状"));
        Assert.That(shell.Office.Jurisdictions[0].PetitionOutcomeCategory, Is.EqualTo("分轻重"));
        Assert.That(shell.Warfare.Summary, Does.Contain("在案行营"));
        Assert.That(shell.Warfare.CampaignBoards, Has.Count.EqualTo(1));
        Assert.That(shell.Warfare.CampaignBoards[0].RegionalProfileLabel, Is.EqualTo("水驿商埠"));
        Assert.That(shell.Warfare.CampaignBoards[0].RegionalBackdropSummary, Does.Contain("水线"));
        Assert.That(shell.Warfare.CampaignBoards[0].StatusLabel, Is.EqualTo("行营在案"));
        Assert.That(shell.Warfare.CampaignBoards[0].EnvironmentLabel, Is.EqualTo("粮筹压阵"));
        Assert.That(shell.Warfare.CampaignBoards[0].BoardSurfaceLabel, Does.Contain("粮道筹签"));
        Assert.That(shell.Warfare.CampaignBoards[0].BoardAtmosphereSummary, Does.Contain("军令为催督粮道"));
        Assert.That(shell.Warfare.CampaignBoards[0].MarkerSummary, Does.Contain("粮道"));
        Assert.That(shell.Warfare.CampaignBoards[0].MarkerSummary, Does.Contain("文移驿线"));
        Assert.That(shell.Warfare.CampaignBoards[0].FrontLabel, Is.EqualTo("前线吃紧"));
        Assert.That(shell.Warfare.CampaignBoards[0].SupplyStateLabel, Is.EqualTo("粮道可守"));
        Assert.That(shell.Warfare.CampaignBoards[0].MoraleStateLabel, Is.EqualTo("军心浮动"));
        Assert.That(shell.Warfare.CampaignBoards[0].CommandFitLabel, Is.EqualTo("号令相接"));
        Assert.That(shell.Warfare.CampaignBoards[0].DirectiveLabel, Is.EqualTo("催督粮道"));
        Assert.That(shell.Warfare.CampaignBoards[0].DirectiveSummary, Does.Contain("先护Lanxi粮道"));
        Assert.That(shell.Warfare.CampaignBoards[0].DirectiveTrace, Does.Contain("当前军令"));
        Assert.That(shell.Warfare.CampaignBoards[0].MobilizationSummary, Does.Contain("可开"));
        Assert.That(shell.Warfare.CampaignBoards[0].FrontSummary, Does.Contain("前线吃紧"));
        Assert.That(shell.Warfare.CampaignBoards[0].CommanderSummary, Does.Contain("主簿"));
        Assert.That(shell.Warfare.CampaignBoards[0].CommanderSummary, Does.Not.Contain("Registrar"));
        Assert.That(shell.Warfare.CampaignBoards[0].CoordinationSummary, Does.Contain("主簿"));
        Assert.That(shell.Warfare.CampaignBoards[0].CoordinationSummary, Does.Not.Contain("Registrar"));
        Assert.That(shell.Warfare.CampaignBoards[0].Routes, Has.Count.EqualTo(2));
        Assert.That(shell.Warfare.CampaignBoards[0].Routes[0].Summary, Does.Not.Contain("escorts"));
        Assert.That(shell.Warfare.CampaignBoards[0].Routes[1].Summary, Does.Not.Contain("campaign board"));
        Assert.That(shell.Warfare.CampaignBoards[0].Routes[1].Summary, Does.Contain("军机案头"));
        Assert.That(shell.Warfare.CampaignBoards[0].Routes[0].FlowStateLabel, Is.EqualTo("粮运畅通"));
        Assert.That(shell.Warfare.MobilizationSignals[0].WindowLabel, Is.EqualTo("可开"));
        Assert.That(shell.Warfare.MobilizationSignals[0].CommandFitLabel, Is.EqualTo("号令相接"));
        Assert.That(shell.Warfare.MobilizationSignals[0].DirectiveLabel, Is.EqualTo("催督粮道"));
        Assert.That(shell.NotificationCenter.Items[0].TraceCount, Is.EqualTo(1));
        Assert.That(shell.Debug.DiagnosticsSchemaLabel, Is.EqualTo("v1"));
        Assert.That(shell.Debug.SeedLabel, Is.EqualTo("12345"));
        Assert.That(shell.Debug.NotificationRetentionLabel, Is.EqualTo("40"));
        Assert.That(shell.Debug.Scale.LatestMetrics.DiffEntryCount, Is.EqualTo(3));
        Assert.That(shell.Debug.Scale.LatestMetrics.DomainEventCount, Is.EqualTo(2));
        Assert.That(shell.Debug.Scale.LatestMetrics.NotificationCount, Is.EqualTo(1));
        Assert.That(shell.Debug.Scale.LatestMetrics.SavePayloadBytes, Is.EqualTo(512));
        Assert.That(shell.Debug.Scale.LatestMetrics.RetentionLimitReached, Is.False);
        Assert.That(shell.Debug.Pressure.Interaction.ActivatedResponseSettlements, Is.EqualTo(1));
        Assert.That(shell.Debug.Pressure.Interaction.PeakSuppressionDemand, Is.EqualTo(25));
        Assert.That(shell.Debug.Pressure.Interaction.Summary, Does.Contain("peak demand"));
        Assert.That(shell.Debug.Pressure.Distribution.Summary, Does.Contain("watched"));
        Assert.That(shell.Debug.Scale.CurrentScale.EntitySummary, Does.Contain("1 settlements"));
        Assert.That(shell.Debug.Scale.CurrentScale.PayloadDensityLabel, Does.Contain("2 households"));
        Assert.That(shell.Debug.Hotspots.CurrentHotspots[0].SettlementName, Is.EqualTo("Lanxi"));
        Assert.That(shell.Debug.Hotspots.CurrentHotspots[0].ResponseSummary, Does.Contain("Active response"));
        Assert.That(shell.Debug.Scale.PayloadSummary.TotalPayloadBytes, Is.EqualTo(512));
        Assert.That(shell.Debug.Scale.PayloadSummary.LargestModuleKey, Is.EqualTo(KnownModuleKeys.NarrativeProjection));
        Assert.That(shell.Debug.Scale.PayloadSummary.LargestModuleShareLabel, Is.EqualTo("50.00%"));
        Assert.That(shell.Debug.Scale.TopPayloadModules[0].ModuleKey, Is.EqualTo(KnownModuleKeys.NarrativeProjection));
        Assert.That(shell.Debug.Scale.TopPayloadModules[0].ShareLabel, Is.EqualTo("50.00%"));
        Assert.That(shell.Debug.Migration.LoadOriginLabel, Is.EqualTo("SaveLoad"));
        Assert.That(shell.Debug.Migration.MigrationStatusLabel, Is.EqualTo("Consistency warnings present"));
        Assert.That(shell.Debug.Migration.MigrationSummary, Does.Contain("migration step"));
        Assert.That(shell.Debug.Migration.MigrationConsistencySummary, Does.Contain("enabled modules"));
        Assert.That(shell.Debug.Migration.MigrationStepCountLabel, Is.EqualTo("1 migration step(s)"));
        Assert.That(shell.Debug.Migration.MigrationSteps, Does.Contain($"{KnownModuleKeys.ConflictAndForce}:1->2"));
        Assert.That(shell.Debug.Scale.ModuleInspectors[0].ModuleKey, Is.EqualTo(KnownModuleKeys.NarrativeProjection));
        Assert.That(shell.Debug.Warnings.Messages, Does.Contain("Urgent notifications are pending review."));
        Assert.That(shell.Debug.Warnings.Invariants, Does.Contain("Module boundary validation passed."));
    }

    [Test]
    public void Compose_WarfareBoardEnvironmentDescriptors_ChangeWithCampaignConditions()
    {
        PresentationReadModelBundle supplyBundle = CreateWarfareOnlyBundle(
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi军务沙盘",
                IsActive = true,
                ObjectiveSummary = "守住 Lanxi 前缘与渡口。",
                MobilizedForceCount = 44,
                FrontPressure = 63,
                FrontLabel = "前线吃紧",
                SupplyState = 57,
                SupplyStateLabel = "粮道可守",
                MoraleState = 52,
                MoraleStateLabel = "军心浮动",
                CommandFitLabel = "号令相接",
                CommanderSummary = "守丁、乡勇与护运在案。",
                ActiveDirectiveCode = WarfareCampaignCommandNames.ProtectSupplyLine,
                ActiveDirectiveLabel = "催督粮道",
                ActiveDirectiveSummary = "先护粮道与渡口。",
                LastDirectiveTrace = "当前军令为催督粮道。",
                MobilizationWindowLabel = "Open",
                SupplyLineSummary = "粮道仍可维持。",
                OfficeCoordinationTrace = "文移驿线仍在转动。",
                LastAftermathSummary = "暂无战后善后。",
                Routes =
                [
                    new CampaignRouteSnapshot
                    {
                        RouteLabel = "粮道",
                        RouteRole = "supply",
                        Pressure = 48,
                        Security = 74,
                        FlowStateLabel = "粮运畅通",
                        Summary = "粮运仍通。",
                    },
                    new CampaignRouteSnapshot
                    {
                        RouteLabel = "文移驿线",
                        RouteRole = "administrative",
                        Pressure = 39,
                        Security = 65,
                        FlowStateLabel = "驿报不断",
                        Summary = "驿报不断。",
                    },
                ],
            },
            new TradeRouteSnapshot
            {
                RouteId = 1,
                ClanId = new ClanId(1),
                RouteName = "Lanxi River Wharf",
                SettlementId = new SettlementId(1),
                IsActive = true,
                Capacity = 26,
                Risk = 18,
                LastMargin = 7,
            });
        PresentationReadModelBundle withdrawalBundle = CreateWarfareOnlyBundle(
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(2),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi军务沙盘",
                IsActive = true,
                ObjectiveSummary = "收拢兵丁，稳住后营。",
                MobilizedForceCount = 18,
                FrontPressure = 28,
                FrontLabel = "前线回收",
                SupplyState = 46,
                SupplyStateLabel = "粮册回拢",
                MoraleState = 37,
                MoraleStateLabel = "军心低徊",
                CommandFitLabel = "号令迟缓",
                CommanderSummary = "后营正在点退兵册。",
                ActiveDirectiveCode = WarfareCampaignCommandNames.WithdrawToBarracks,
                ActiveDirectiveLabel = "班师归营",
                ActiveDirectiveSummary = "先收旗归营，再议后事。",
                LastDirectiveTrace = "当前军令为班师归营。",
                MobilizationWindowLabel = "Narrow",
                SupplyLineSummary = "后营收束。",
                OfficeCoordinationTrace = "只保留归营文移。",
                LastAftermathSummary = "营旗收束，等待覆核。",
                Routes =
                [
                    new CampaignRouteSnapshot
                    {
                        RouteLabel = "后营归路",
                        RouteRole = "retreat",
                        Pressure = 22,
                        Security = 61,
                        FlowStateLabel = "收束归营",
                        Summary = "兵丁沿后营归路回撤。",
                    },
                ],
            },
            new TradeRouteSnapshot
            {
                RouteId = 2,
                ClanId = new ClanId(1),
                RouteName = "Fushan Hill Route",
                SettlementId = new SettlementId(1),
                IsActive = true,
                Capacity = 18,
                Risk = 27,
                LastMargin = 3,
            });

        CampaignBoardViewModel supplyBoard = FirstPassPresentationShell.Compose(supplyBundle).Warfare.CampaignBoards.Single();
        CampaignBoardViewModel withdrawalBoard = FirstPassPresentationShell.Compose(withdrawalBundle).Warfare.CampaignBoards.Single();

        Assert.That(supplyBoard.RegionalProfileLabel, Is.EqualTo("水驿商埠"));
        Assert.That(withdrawalBoard.RegionalProfileLabel, Is.EqualTo("山道关路"));
        Assert.That(supplyBoard.EnvironmentLabel, Is.EqualTo("粮筹压阵"));
        Assert.That(withdrawalBoard.EnvironmentLabel, Is.EqualTo("收军归营"));
        Assert.That(supplyBoard.BoardSurfaceLabel, Is.Not.EqualTo(withdrawalBoard.BoardSurfaceLabel));
        Assert.That(supplyBoard.BoardSurfaceLabel, Does.Contain("水线"));
        Assert.That(withdrawalBoard.BoardSurfaceLabel, Does.Contain("山道折线"));
        Assert.That(supplyBoard.BoardAtmosphereSummary, Does.Contain("催督粮道"));
        Assert.That(withdrawalBoard.BoardAtmosphereSummary, Does.Contain("班师归营"));
        Assert.That(withdrawalBoard.MarkerSummary, Does.Contain("山道关路"));
    }

    [Test]
    public void Compose_AftermathDocketSummaries_DrawFromWarfareContextWithoutAddingAuthorityUi()
    {
        PresentationReadModelBundle bundle = CreateWarfareOnlyBundle(
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(3),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi鍐涘姟娌欑洏",
                IsActive = false,
                ObjectiveSummary = "鏀舵潫璇稿啗锛屾竻鐞嗗悗浜嬨€?",
                MobilizedForceCount = 24,
                FrontPressure = 54,
                FrontLabel = "鏀跺嵎鏍℃牳",
                SupplyState = 43,
                SupplyStateLabel = "绮亾鍚冪揣",
                MoraleState = 38,
                MoraleStateLabel = "鍐涘績娴姩",
                CommandFitLabel = "鍙蜂护鍕夊己",
                CommanderSummary = "Lanxi 琛屼紞姝ｅ湪鏀舵潫锛屽悗浜嬪凡鍏ユ銆?",
                ActiveDirectiveCode = WarfareCampaignCommandNames.WithdrawToBarracks,
                ActiveDirectiveLabel = "鐝笀褰掕惀",
                ActiveDirectiveSummary = "鍏堟敹閮ㄥ洖钀ワ紝鍐嶅姙鍚庝簨銆?",
                LastDirectiveTrace = "Lanxi 妗堝ご宸查┈鍏ュ悗缁鏍稿強鏀舵潫娴佺▼銆?",
                MobilizationWindowLabel = "Narrow",
                SupplyLineSummary = "杩愮伯绾垮皻寰呬慨鏁淬€?",
                OfficeCoordinationTrace = "Lanxi 褰撳湴瀹樼讲姝ｅ湪鍔炵悊鎶氭仱涓庤拷璐ｆ枃绉汇€?",
                SourceTrace = "Lanxi 妗堝ご鍙栨潗浜庢垬鍚庢敹鍐涗笌鍦版柟浣欐尝銆?",
                LastAftermathSummary = "Lanxi 宸茬敱鎴樹簨杞叆鎶氭仱銆佽拷璐ｄ笌瀹夋皯銆?",
                Routes =
                [
                    new CampaignRouteSnapshot
                    {
                        RouteLabel = "绮亾",
                        RouteRole = "supply",
                        Pressure = 57,
                        Security = 41,
                        FlowStateLabel = "绮繍鑴嗗急",
                        Summary = "绮亾灏氬緟涓€鏍囦慨鏁淬€?",
                    },
                ],
            },
            new TradeRouteSnapshot
            {
                RouteId = 4,
                ClanId = new ClanId(1),
                RouteName = "Lanxi River Wharf",
                SettlementId = new SettlementId(1),
                IsActive = true,
                Capacity = 20,
                Risk = 27,
                LastMargin = 2,
            });
        bundle.PopulationSettlements =
        [
            new PopulationSettlementSnapshot
            {
                SettlementId = new SettlementId(1),
                CommonerDistress = 52,
                LaborSupply = 91,
                MigrationPressure = 43,
                MilitiaPotential = 48,
            },
        ];
        bundle.OfficeJurisdictions =
        [
            new JurisdictionAuthoritySnapshot
            {
                SettlementId = new SettlementId(1),
                LeadOfficialPersonId = new PersonId(1),
                LeadOfficialName = "Zhang Yuan",
                LeadOfficeTitle = "主簿",
                AuthorityTier = 2,
                JurisdictionLeverage = 58,
                PetitionPressure = 32,
                PetitionBacklog = 12,
                CurrentAdministrativeTask = "覆核战后功过文移",
                AdministrativeTaskTier = "district",
                LastPetitionOutcome = "劾责中：词牍分轻重收理。",
                PetitionOutcomeCategory = "Censured",
                LastAdministrativeTrace = "兰溪案头正在权衡劾责与抚恤文移。",
            },
        ];
        bundle.Notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(1),
                CreatedAt = new GameDate(1200, 5),
                Tier = NotificationTier.Consequential,
                Surface = NarrativeSurface.DeskSandbox,
                Title = "战后赏罚与抚恤",
                Summary = "兰溪战后诸案已入覆核。",
                WhyItHappened = "兰溪战后已并入记功、劾责与抚恤诸案。",
                WhatNext = "先看记功请赏、失守追责、抚恤安民诸案，再定班师归营。",
                SourceModuleKey = KnownModuleKeys.WarfareCampaign,
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        SourceModuleKey = KnownModuleKeys.FamilyCore,
                        EventType = WarfareCampaignEventNames.CampaignAftermathRegistered,
                        EventSummary = "兰溪战后诸案已入覆核。",
                        DiffDescription = "兰溪战后记功簿已开。",
                        EntityKey = "1",
                    },
                    new NotificationTraceSnapshot
                    {
                        SourceModuleKey = KnownModuleKeys.OfficeAndCareer,
                        EventType = WarfareCampaignEventNames.CampaignAftermathRegistered,
                        EventSummary = "兰溪战后诸案已入覆核。",
                        DiffDescription = "兰溪案头并入劾责状与抚恤词牍。",
                        EntityKey = "1",
                    },
                    new NotificationTraceSnapshot
                    {
                        SourceModuleKey = KnownModuleKeys.PopulationAndHouseholds,
                        EventType = WarfareCampaignEventNames.CampaignAftermathRegistered,
                        EventSummary = "兰溪战后诸案已入覆核。",
                        DiffDescription = "兰溪民户因战后善后而困压渐重。",
                        EntityKey = "1",
                    },
                    new NotificationTraceSnapshot
                    {
                        SourceModuleKey = KnownModuleKeys.OrderAndBanditry,
                        EventType = WarfareCampaignEventNames.CampaignAftermathRegistered,
                        EventSummary = "兰溪战后诸案已入覆核。",
                        DiffDescription = "兰溪另有清路札与地方不靖之报。",
                        EntityKey = "1",
                    },
                ],
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("记功簿"));
        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("劾责状"));
        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("抚恤簿"));
        Assert.That(shell.DeskSandbox.Settlements.Single().AftermathSummary, Does.Contain("战后案牍"));
        Assert.That(shell.DeskSandbox.Settlements.Single().AftermathSummary, Does.Contain("抚恤簿"));
        Assert.That(shell.Warfare.CampaignBoards.Single().AftermathDocketSummary, Does.Contain("军机案今并载"));
        Assert.That(shell.Warfare.CampaignBoards.Single().AftermathDocketSummary, Does.Contain("劾责状"));
        Assert.That(shell.Warfare.CampaignBoards.Single().AftermathDocketSummary, Does.Contain("清路札"));
    }

    [Test]
    public void Compose_WithoutOfficeReadModels_KeepsOfficeSurfaceReadOnlyAndEmpty()
    {
        PresentationReadModelBundle bundle = new()
        {
            CurrentDate = new GameDate(1200, 4),
            ReplayHash = "abc123",
            Settlements =
            [
                new SettlementSnapshot
                {
                    Id = new SettlementId(1),
                    Name = "Lanxi",
                    Security = 62,
                    Prosperity = 64,
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.GovernanceSummary, Is.EqualTo("案头暂无官署呈报。"));
        Assert.That(shell.GreatHall.WarfareSummary, Is.EqualTo("暂无军务沙盘投影。"));
        Assert.That(shell.DeskSandbox.Settlements[0].GovernanceSummary, Is.EqualTo("官署未设。"));
        Assert.That(shell.DeskSandbox.Settlements[0].CampaignSummary, Is.EqualTo("暂无军务沙盘投影。"));
        Assert.That(shell.Office.Summary, Is.EqualTo("案头暂无官署牍报。"));
        Assert.That(shell.Office.Appointments, Is.Empty);
        Assert.That(shell.Office.Jurisdictions, Is.Empty);
        Assert.That(shell.Warfare.Summary, Is.EqualTo("暂无军务沙盘投影。"));
        Assert.That(shell.Warfare.CampaignBoards, Is.Empty);
        Assert.That(shell.Warfare.MobilizationSignals, Is.Empty);
    }

    [Test]
    public void Compose_PublicApi_ConsumesReadModelBundleOnly()
    {
        MethodInfo composeMethod = typeof(FirstPassPresentationShell).GetMethod("Compose", BindingFlags.Public | BindingFlags.Static)!;

        Assert.That(composeMethod.GetParameters().Single().ParameterType, Is.EqualTo(typeof(PresentationReadModelBundle)));
        Assert.That(composeMethod.ReturnType, Is.EqualTo(typeof(PresentationShellViewModel)));
    }

    [Test]
    public void PresentationProject_ReferencesContractsOnly()
    {
        string projectPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "src", "Zongzu.Presentation.Unity", "Zongzu.Presentation.Unity.csproj"));
        string projectText = File.ReadAllText(projectPath);

        Assert.That(projectText, Does.Contain("Zongzu.Contracts"));
        Assert.That(projectText, Does.Not.Contain("Zongzu.Application"));
        Assert.That(projectText, Does.Not.Contain("Zongzu.Modules."));
        Assert.That(projectText, Does.Not.Contain("Zongzu.Scheduler"));
    }

    private static PresentationReadModelBundle CreateWarfareOnlyBundle(CampaignFrontSnapshot campaign, params TradeRouteSnapshot[] tradeRoutes)
    {
        return new PresentationReadModelBundle
        {
            CurrentDate = new GameDate(1200, 4),
            ReplayHash = "warfare-only",
            Settlements =
            [
                new SettlementSnapshot
                {
                    Id = campaign.AnchorSettlementId,
                    Name = campaign.AnchorSettlementName,
                    Security = 62,
                    Prosperity = 64,
                },
            ],
            TradeRoutes = tradeRoutes,
            Campaigns = [campaign],
        };
    }
}
