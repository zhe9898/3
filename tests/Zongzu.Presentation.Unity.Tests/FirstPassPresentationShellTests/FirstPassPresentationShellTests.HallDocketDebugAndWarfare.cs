using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

public sealed partial class FirstPassPresentationShellTests
{
    [Test]
    public void Compose_PrefersHallDocketLeadItemOverNotificationForGreatHallLeadNotice()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(11),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Urgent,
                Surface = NarrativeSurface.GreatHall,
                Title = "raw notification title",
                Summary = "raw notification summary",
                WhatNext = "raw notification guidance",
            },
        ];
        bundle.HallDocket = new HallDocketStackSnapshot
        {
            LeadItem = new HallDocketItemSnapshot
            {
                LaneKey = HallDocketLaneKeys.Governance,
                SettlementId = new SettlementId(1),
                SettlementName = "Lanxi",
                NodeLabel = "county-gate",
                Headline = "hall docket lead title",
                WhyNowSummary = "hall docket why-now",
                PhaseLabel = "needs response",
                PhaseSummary = "hall docket phase",
                HandlingSummary = "hall docket handling",
                GuidanceSummary = "hall docket guidance",
            },
            SecondaryItems = [],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.LeadNoticeTitle, Is.EqualTo("hall docket lead title"));
        Assert.That(shell.GreatHall.LeadNoticeGuidance, Does.Contain("hall docket guidance"));
        Assert.That(shell.GreatHall.LeadNoticeGuidance, Does.Contain("hall docket phase"));
        Assert.That(shell.NotificationCenter.Items, Has.Count.EqualTo(1));
        Assert.That(shell.NotificationCenter.Items[0].Title, Is.EqualTo("raw notification title"));
    }

    [Test]
    public void Compose_ProjectsSecondaryHallDocketItemsIntoGreatHall()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.HallDocket = new HallDocketStackSnapshot
        {
            LeadItem = new HallDocketItemSnapshot
            {
                LaneKey = HallDocketLaneKeys.Family,
                SettlementId = new SettlementId(1),
                Headline = "lead family matter",
                GuidanceSummary = "lead family guidance",
            },
            SecondaryItems =
            [
                new HallDocketItemSnapshot
                {
                    LaneKey = HallDocketLaneKeys.Governance,
                    SettlementId = new SettlementId(1),
                    SettlementName = "Lanxi",
                    NodeLabel = "county-gate",
                    TargetLabel = "county-gate",
                    Headline = "governance secondary matter",
                    PhaseLabel = "needs response",
                    WhyNowSummary = "county paperwork is tightening",
                    PhaseSummary = "stabilize the gate before backlog rises",
                },
                new HallDocketItemSnapshot
                {
                    LaneKey = HallDocketLaneKeys.Warfare,
                    SettlementId = new SettlementId(1),
                    SettlementName = "Lanxi",
                    NodeLabel = "river-route",
                    TargetLabel = "river-route",
                    Headline = "warfare secondary matter",
                    PhaseLabel = "aftercare",
                    GuidanceSummary = "escort strain still needs follow-through",
                    HandlingSummary = "recent mobilization has not fully settled",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.SecondaryDockets, Has.Count.EqualTo(2));
        Assert.That(shell.GreatHall.SecondaryDockets[0].Headline, Is.EqualTo("governance secondary matter"));
        Assert.That(shell.GreatHall.SecondaryDockets[0].Summary, Does.Contain("county paperwork is tightening"));
        Assert.That(shell.GreatHall.SecondaryDockets[1].Headline, Is.EqualTo("warfare secondary matter"));
        Assert.That(shell.GreatHall.SecondaryDockets[1].Summary, Does.Contain("escort strain still needs follow-through"));
    }

    [Test]
    public void Compose_ProjectsHallDocketAgendaIntoDeskSettlementNodes()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.HallDocket = new HallDocketStackSnapshot
        {
            LeadItem = new HallDocketItemSnapshot
            {
                LaneKey = HallDocketLaneKeys.Family,
                SettlementId = new SettlementId(1),
                SettlementName = "Lanxi",
                NodeLabel = "ancestral-hall",
                Headline = "lead family matter",
                PhaseLabel = "family review",
            },
            SecondaryItems =
            [
                new HallDocketItemSnapshot
                {
                    LaneKey = HallDocketLaneKeys.Governance,
                    SettlementId = new SettlementId(1),
                    SettlementName = "Lanxi",
                    NodeLabel = "county-gate",
                    Headline = "governance secondary matter",
                    PhaseLabel = "needs response",
                    WhyNowSummary = "county paperwork is still tightening",
                },
                new HallDocketItemSnapshot
                {
                    LaneKey = HallDocketLaneKeys.Warfare,
                    SettlementId = new SettlementId(2),
                    SettlementName = "Elsewhere",
                    NodeLabel = "river-route",
                    Headline = "off-node warfare matter",
                    PhaseLabel = "aftercare",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(settlementNode.HallAgendaSummary, Does.Contain("lead family matter"));
        Assert.That(settlementNode.HallAgendaSummary, Does.Contain("governance secondary matter"));
        Assert.That(settlementNode.HallAgendaSummary, Does.Not.Contain("off-node warfare matter"));
        Assert.That(settlementNode.HallAgendaCount, Is.EqualTo(2));
        Assert.That(settlementNode.HallAgendaItems, Has.Count.EqualTo(2));
        Assert.That(settlementNode.HallAgendaLaneKeys, Is.EqualTo(new[] { HallDocketLaneKeys.Family, HallDocketLaneKeys.Governance }));
        Assert.That(settlementNode.HasLeadHallAgendaItem, Is.True);
        Assert.That(settlementNode.LeadHallAgendaLaneKey, Is.EqualTo(HallDocketLaneKeys.Family));
        Assert.That(settlementNode.HallAgendaItems[0].Headline, Is.EqualTo("lead family matter"));
        Assert.That(settlementNode.HallAgendaItems[1].LaneKey, Is.EqualTo(HallDocketLaneKeys.Governance));
        Assert.That(settlementNode.HallAgendaItems[1].Summary, Does.Contain("county paperwork is still tightening"));
    }

    [Test]
    public void Compose_LeavesDeskLeadHallAgendaMarkerEmptyWhenLeadItemTargetsElsewhere()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.HallDocket = new HallDocketStackSnapshot
        {
            LeadItem = new HallDocketItemSnapshot
            {
                LaneKey = HallDocketLaneKeys.Warfare,
                SettlementId = new SettlementId(2),
                SettlementName = "Elsewhere",
                NodeLabel = "river-route",
                Headline = "off-node lead matter",
                PhaseLabel = "aftercare",
            },
            SecondaryItems =
            [
                new HallDocketItemSnapshot
                {
                    LaneKey = HallDocketLaneKeys.Governance,
                    SettlementId = new SettlementId(1),
                    SettlementName = "Lanxi",
                    NodeLabel = "county-gate",
                    Headline = "local secondary matter",
                    PhaseLabel = "needs response",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(settlementNode.HallAgendaCount, Is.EqualTo(1));
        Assert.That(settlementNode.HasLeadHallAgendaItem, Is.False);
        Assert.That(settlementNode.LeadHallAgendaLaneKey, Is.Empty);
        Assert.That(settlementNode.HallAgendaLaneKeys, Is.EqualTo(new[] { HallDocketLaneKeys.Governance }));
    }

    [Test]
    public void Compose_AlignsFamilyLifecycleLeadNoticeAndNotificationCenterGuidance()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.SupportNewbornCare,
                    Label = "拨粮护婴",
                    Summary = "先把产后调护、乳哺与襁褓衣食稳下来。",
                    AvailabilitySummary = "门内现有襁褓1口，宗房余力尚可拨用。",
                    TargetLabel = "清河张氏",
                    IsEnabled = true,
                },
            ],
            Receipts = [],
        };
        bundle.Notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(9),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Consequential,
                Surface = NarrativeSurface.AncestralHall,
                Title = "门内添丁",
                Summary = "张氏门内新添一口。",
                WhyItHappened = "门内添丁之后，乳哺与抚养之费一并压上肩头。",
                WhatNext = "先把产妇与襁褓照看住，再看口粮、乳哺与看护之费该由谁分担。",
                SourceModuleKey = KnownModuleKeys.FamilyCore,
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        SourceModuleKey = KnownModuleKeys.FamilyCore,
                        EventType = FamilyCoreEventNames.BirthRegistered,
                        EventSummary = "张氏门内添丁。",
                        DiffDescription = "张氏添丁之后，香火暂缓焦心。",
                        EntityKey = "1",
                    },
                ],
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.LeadNoticeTitle, Is.EqualTo("门内添丁"));
        Assert.That(shell.GreatHall.LeadNoticeGuidance, Does.Contain("襁褓"));
        Assert.That(shell.GreatHall.LeadNoticeGuidance, Does.Contain("眼下最宜先命清河张氏拨粮护婴。"));
        Assert.That(shell.NotificationCenter.Items, Has.Count.EqualTo(1));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("襁褓"));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("眼下最宜先命清河张氏拨粮护婴。"));
    }

    [Test]
    public void Compose_DeathNoticeWithSevereSuccessionGap_PromptsHeirDesignationBeforeMourningOrder()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Clans =
        [
            bundle.Clans[0] with
            {
                HeirPersonId = null,
                HeirSecurity = 8,
                InheritancePressure = 76,
                BranchTension = 58,
                MourningLoad = 32,
                LastLifecycleTrace = "清河张氏按死者名分3阶、承祧缺口3阶、丧葬拖累2阶承受承祧人身故。",
            },
        ];
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.SetMourningOrder,
                    Label = "议定丧次",
                    AvailabilitySummary = "门内丧服之重32，宜先定服序与支用。",
                    TargetLabel = "清河张氏",
                    IsEnabled = true,
                },
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.DesignateHeirPolicy,
                    Label = "议定承祧",
                    AvailabilitySummary = "堂上尚未举出承祧之人，宜先定后序。",
                    TargetLabel = "清河张氏",
                    IsEnabled = true,
                },
            ],
            Receipts = [],
        };
        bundle.Notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(10),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Urgent,
                Surface = NarrativeSurface.AncestralHall,
                Title = "门内举哀",
                WhatNext = "先议定承祧名分，再理丧次、祭次与支用。",
                SourceModuleKey = KnownModuleKeys.FamilyCore,
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        SourceModuleKey = KnownModuleKeys.FamilyCore,
                        EventType = FamilyCoreEventNames.ClanMemberDied,
                        EventSummary = "清河张氏门内举哀。",
                        DiffDescription = "清河张氏承祧之人身故（承祧缺口3阶）。",
                        EntityKey = "1",
                    },
                ],
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("承祧未稳"));
        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("宜先议定承祧"));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("先议定承祧名分"));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("眼下最宜先命清河张氏议定承祧。"));
    }

    [Test]
    public void Compose_DeathNoticeWithAdultSuccessor_PromptsMourningOrderBeforeHeirFollowup()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Clans =
        [
            bundle.Clans[0] with
            {
                HeirPersonId = new PersonId(2),
                HeirSecurity = 34,
                InheritancePressure = 44,
                BranchTension = 42,
                MourningLoad = 26,
                LastLifecycleTrace = "清河张氏按死者名分3阶、承祧缺口1阶、丧葬拖累2阶承受承祧人身故。",
            },
        ];
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.SetMourningOrder,
                    Label = "议定丧次",
                    AvailabilitySummary = "门内丧服之重26，宜先定服序与支用。",
                    TargetLabel = "清河张氏",
                    IsEnabled = true,
                },
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.DesignateHeirPolicy,
                    Label = "议定承祧",
                    AvailabilitySummary = "承祧稳度34，名分若虚仍易再起后议。",
                    TargetLabel = "清河张氏",
                    IsEnabled = true,
                },
            ],
            Receipts = [],
        };
        bundle.Notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(11),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Urgent,
                Surface = NarrativeSurface.AncestralHall,
                Title = "门内举哀",
                WhatNext = "先把丧次、祭次与支用议定，再把新承祧名分写稳。",
                SourceModuleKey = KnownModuleKeys.FamilyCore,
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        SourceModuleKey = KnownModuleKeys.FamilyCore,
                        EventType = FamilyCoreEventNames.ClanMemberDied,
                        EventSummary = "清河张氏门内举哀。",
                        DiffDescription = "清河张氏承祧之人身故（承祧缺口1阶）。",
                        EntityKey = "1",
                    },
                ],
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("门内举哀未毕"));
        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("宜先议定丧次"));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("新承祧名分"));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("眼下最宜先命清河张氏议定丧次。"));
    }

    [Test]
    public void Compose_ProjectsDebugSnapshotIntoDebugPanel()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Debug = new PresentationDebugSnapshot
        {
            DiagnosticsSchemaVersion = 3,
            InitialSeed = 4242,
            NotificationRetentionLimit = 9,
            RetentionLimitReached = true,
            LatestMetrics = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = 5,
                DomainEventCount = 2,
                NotificationCount = 4,
                SavePayloadBytes = 1024,
            },
            CurrentScale = new RuntimeScaleMetricsSnapshot
            {
                SettlementCount = 1,
                ClanCount = 1,
                HouseholdCount = 12,
                AcademyCount = 1,
                RouteCount = 2,
                EnabledModuleCount = 6,
                SavedModuleCount = 6,
                NotificationCount = 4,
                NotificationUtilizationPercent = 44,
                SavePayloadBytesPerSettlement = 1024,
                AverageHouseholdsPerSettlement = 12,
            },
            CurrentPayloadSummary = new RuntimePayloadSummarySnapshot
            {
                TotalModulePayloadBytes = 4096,
                LargestModuleKey = KnownModuleKeys.FamilyCore,
                LargestModulePayloadBytes = 1024,
                LargestModuleShareBasisPoints = 2500,
            },
            TopPayloadModules =
            [
                new ModulePayloadFootprintSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    PayloadBytes = 1024,
                    PayloadShareBasisPoints = 2500,
                },
            ],
            EnabledModules =
            [
                new DebugFeatureModeSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    Mode = "active",
                },
            ],
            ModuleInspectors =
            [
                new DebugModuleInspectorSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    ModuleSchemaVersion = 2,
                    PayloadBytes = 1024,
                },
            ],
            LoadMigration = new DebugLoadMigrationSnapshot
            {
                LoadOriginLabel = "save-slot",
                ConsistencyPassed = false,
                Summary = "migration applied",
                ConsistencySummary = "lineage fixup pending review",
                StepCount = 1,
                Steps =
                [
                    new DebugMigrationStepSnapshot
                    {
                        ScopeLabel = "root",
                        SourceVersion = 1,
                        TargetVersion = 2,
                    },
                ],
            },
            Warnings = ["payload nearing ceiling"],
            Invariants = ["hall docket stack stable"],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.Debug.DiagnosticsSchemaLabel, Is.EqualTo("v3"));
        Assert.That(shell.Debug.SeedLabel, Is.EqualTo("4242"));
        Assert.That(shell.Debug.Scale.CurrentScale.EntitySummary, Does.Contain("1 settlements, 1 clans, 12 households."));
        Assert.That(shell.Debug.Scale.PayloadSummary.Summary, Does.Contain(KnownModuleKeys.FamilyCore));
        Assert.That(shell.Debug.Migration.LoadOriginLabel, Is.EqualTo("save-slot"));
        Assert.That(shell.Debug.Migration.MigrationStatusLabel, Is.EqualTo("Consistency warnings present"));
        Assert.That(shell.Debug.Migration.MigrationSteps, Is.EqualTo(new[] { "root:1->2" }));
        Assert.That(shell.Debug.Warnings.Messages, Is.EqualTo(new[] { "payload nearing ceiling" }));
        Assert.That(shell.Debug.Warnings.Invariants, Is.EqualTo(new[] { "hall docket stack stable" }));
    }


    [Test]
    public void Compose_ProjectsRegionalWarfareAndAftermathIntoHallDeskAndCampaignBoard()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Settlements = [bundle.Settlements[0] with { Security = 54, Prosperity = 62 }];
        bundle.PopulationSettlements = [bundle.PopulationSettlements[0] with { CommonerDistress = 45, MigrationPressure = 36 }];
        bundle.OfficeJurisdictions = [bundle.OfficeJurisdictions[0] with { PetitionBacklog = 9 }];
        bundle.ClanTradeRoutes =
        [
            new ClanTradeRouteSnapshot
            {
                RouteId = 1,
                ClanId = new ClanId(1),
                SettlementId = new SettlementId(1),
                RouteName = "兰溪渡口粮线",
                Capacity = 41,
                Risk = 62,
                IsActive = true,
                LastMargin = 14,
                BlockedShipmentCount = 2,
                SeizureRisk = 38,
                RouteConstraintLabel = "渡口脚力吃紧",
                LastRouteTrace = "渡口脚力被军需牵住。",
            },
        ];
        bundle.Campaigns =
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "兰溪",
                CampaignName = "兰溪河渡行营",
                IsActive = true,
                ObjectiveSummary = "先稳河渡，再护粮线。",
                MobilizedForceCount = 180,
                FrontPressure = 62,
                FrontLabel = "前锋相持",
                SupplyState = 52,
                SupplyStateLabel = "粮道尚稳",
                MoraleState = 60,
                MoraleStateLabel = "军心可用",
                CommandFitLabel = "以护运守。",
                CommanderSummary = "张元暂摄转运与护路节次。",
                ActiveDirectiveCode = "ProtectSupplyLine",
                ActiveDirectiveLabel = "护粮稳线",
                ActiveDirectiveSummary = "先保护口粮线，再缓前锋耗损。",
                LastDirectiveTrace = "主簿差弁已把渡口签牌并入军机案头。",
                MobilizationWindowLabel = "Narrow",
                SupplyLineSummary = "渡口粮线仍能转运，只是脚力吃紧。",
                OfficeCoordinationTrace = "县署差弁与护运脚夫并线而行。",
                SourceTrace = "河渡一线还在收束。",
                LastAftermathSummary = "渡口村落仍待安辑与抚恤。",
                Routes =
                [
                    new CampaignRouteSnapshot
                    {
                        RouteLabel = "渡口粮线",
                        RouteRole = "supply",
                        Pressure = 80,
                        Security = 40,
                        FlowStateLabel = "受阻",
                        Summary = "渡口粮签与脚夫簿正被军需挤压。",
                    },
                    new CampaignRouteSnapshot
                    {
                        RouteLabel = "县门驿路",
                        RouteRole = "dispatch",
                        Pressure = 48,
                        Security = 55,
                        FlowStateLabel = "可。",
                        Summary = "县门驿路仍可递送军报。",
                    },
                ],
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.WarfareSummary, Does.Contain("水驿商埠"));
        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("记功簿"));
        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("清路札"));
        Assert.That(shell.DeskSandbox.Settlements[0].CampaignSummary, Does.Contain("兰溪河渡行营"));
        Assert.That(shell.DeskSandbox.Settlements[0].CampaignSummary, Does.Contain("水驿商埠"));
        Assert.That(shell.DeskSandbox.Settlements[0].AftermathSummary, Does.Contain("战后案牍"));
        Assert.That(shell.DeskSandbox.Settlements[0].AftermathSummary, Does.Contain("抚恤簿"));
        Assert.That(shell.Warfare.CampaignBoards, Has.Count.EqualTo(1));
        Assert.That(shell.Warfare.CampaignBoards[0].RegionalProfileLabel, Is.EqualTo("水驿商埠"));
        Assert.That(shell.Warfare.CampaignBoards[0].AftermathDocketSummary, Does.Contain("军机案今并载"));
        Assert.That(shell.Warfare.CampaignBoards[0].AftermathDocketSummary, Does.Contain("清路札"));
    }

    [Test]
    public void Compose_CopiesProjectedWarfareLaneClosureFieldsOnly()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.CampaignMobilizationSignals =
        [
            new CampaignMobilizationSignalSnapshot
            {
                SettlementId = new SettlementId(1),
                SettlementName = "兰溪",
                AvailableForceCount = 180,
                Readiness = 58,
                CommandCapacity = 62,
                ResponseActivationLevel = 44,
                OrderSupportLevel = 36,
                MobilizationWindowLabel = "Narrow",
            },
        ];
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.WarfareCampaign,
                    SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.ProtectSupplyLine,
                    Label = "护粮稳线",
                    IsEnabled = true,
                    ReadbackSummary = "军令选择读回：粮道护持选择，WarfareCampaign拥有军令；军务选择不是县门文移代打，不是普通家户硬扛。",
                    WarfareLaneEntryReadbackSummary = "军务承接入口：回到WarfareCampaign/ConflictAndForce。",
                    ForceReadinessReadbackSummary = "Force承接读回：可调之众180。",
                    CampaignAftermathReadbackSummary = "战后后账读回：渡口村落待安辑。",
                    WarfareLaneReceiptClosureSummary = "军务后手收口读回：军令只说明已落案头。",
                    WarfareLaneResidueFollowUpSummary = "军务余味续接读回：恐惧12仍在。",
                    WarfareLaneNoLoopGuardSummary = "军务闭环防回压：不是普通家户硬扛。",
                },
            ],
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.WarfareCampaign,
                    SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.ProtectSupplyLine,
                    Label = "护粮稳线",
                    Summary = "军令已落案头。",
                    OutcomeSummary = "先保渡口粮线。",
                    ReadbackSummary = "军令选择读回：粮道护持选择，WarfareCampaign拥有军令；军务选择不是县门文移代打，不是普通家户硬扛。",
                    WarfareLaneEntryReadbackSummary = "军务承接入口：回到WarfareCampaign/ConflictAndForce。",
                    ForceReadinessReadbackSummary = "Force承接读回：可调之众180。",
                    CampaignAftermathReadbackSummary = "战后后账读回：渡口村落待安辑。",
                    WarfareLaneReceiptClosureSummary = "军务后手收口读回：军令只说明已落案头。",
                    WarfareLaneResidueFollowUpSummary = "军务余味续接读回：恐惧12仍在。",
                    WarfareLaneNoLoopGuardSummary = "军务闭环防回压：不是普通家户硬扛。",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        CommandAffordanceViewModel affordance = shell.Warfare.CommandAffordances.Single();
        CommandReceiptViewModel receipt = shell.Warfare.RecentReceipts.Single();

        Assert.That(affordance.ReadbackSummary, Does.Contain("军令选择读回"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("WarfareCampaign拥有军令"));
        Assert.That(affordance.WarfareLaneEntryReadbackSummary, Does.Contain("军务承接入口"));
        Assert.That(affordance.ForceReadinessReadbackSummary, Does.Contain("Force承接读回"));
        Assert.That(affordance.WarfareLaneNoLoopGuardSummary, Does.Contain("军务闭环防回压"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("军令选择读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("军务选择不是县门文移代打"));
        Assert.That(receipt.CampaignAftermathReadbackSummary, Does.Contain("战后后账读回"));
        Assert.That(receipt.WarfareLaneReceiptClosureSummary, Does.Contain("军务后手收口读回"));
    }

}
