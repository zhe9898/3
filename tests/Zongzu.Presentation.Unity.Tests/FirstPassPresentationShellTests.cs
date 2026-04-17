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
                    AcademyName = "Lanxi Academy",
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
                    LastExplanation = "Stable study month.",
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
                    LastExplanation = "Margin improved.",
                },
            ],
            Markets =
            [
                new MarketSnapshot
                {
                    SettlementId = new SettlementId(1),
                    MarketName = "Lanxi Morning Market",
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
                    RouteName = "Lanxi River Wharf",
                    SettlementId = new SettlementId(1),
                    IsActive = true,
                    Capacity = 26,
                    Risk = 18,
                    LastMargin = 8,
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
                    Title = "Trade Prospered",
                    Summary = "Clan Zhang trade prospered.",
                    WhyItHappened = "Demand and route stability stayed favorable.",
                    WhatNext = "Review reinvestment.",
                    SourceModuleKey = KnownModuleKeys.TradeAndIndustry,
                    Traces =
                    [
                        new NotificationTraceSnapshot
                        {
                            SourceModuleKey = KnownModuleKeys.TradeAndIndustry,
                            EventType = "TradeProspered",
                            EventSummary = "Clan Zhang trade prospered.",
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
        Assert.That(shell.Lineage.Clans, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].AcademySummary, Does.Contain("Lanxi Academy"));
        Assert.That(shell.NotificationCenter.Items[0].TraceCount, Is.EqualTo(1));
        Assert.That(shell.Debug.DiagnosticsSchemaLabel, Is.EqualTo("v1"));
        Assert.That(shell.Debug.SeedLabel, Is.EqualTo("12345"));
        Assert.That(shell.Debug.NotificationRetentionLabel, Is.EqualTo("40"));
        Assert.That(shell.Debug.LatestMetrics.DiffEntryCount, Is.EqualTo(3));
        Assert.That(shell.Debug.LatestMetrics.DomainEventCount, Is.EqualTo(2));
        Assert.That(shell.Debug.LatestMetrics.NotificationCount, Is.EqualTo(1));
        Assert.That(shell.Debug.LatestMetrics.SavePayloadBytes, Is.EqualTo(512));
        Assert.That(shell.Debug.LatestMetrics.RetentionLimitReached, Is.False);
        Assert.That(shell.Debug.ModuleInspectors[0].ModuleKey, Is.EqualTo(KnownModuleKeys.NarrativeProjection));
        Assert.That(shell.Debug.Invariants, Does.Contain("Module boundary validation passed."));
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
}
