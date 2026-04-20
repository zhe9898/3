using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

[TestFixture]
public sealed class FirstPassPresentationShellTests
{
    [Test]
    public void Compose_ProjectsMonthlyPublicLifeCadenceIntoGreatHallAndDeskSandbox()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(CreateBundle());

        Assert.That(shell.GreatHall.PublicLifeSummary, Does.Contain("春社集日"));
        Assert.That(shell.GreatHall.PublicLifeSummary, Does.Contain("客商"));
        Assert.That(shell.GreatHall.PublicLifeSummary, Does.Contain("说法相左").Or.Contain("榜文").Or.Contain("街谈"));
        Assert.That(shell.DeskSandbox.Settlements, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("春社集日"));
        Assert.That(shell.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("街口茶肆"));
        Assert.That(shell.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("榜文").Or.Contain("街谈").Or.Contain("路报"));
    }

    [Test]
    public void Compose_ProjectsGreatHallCountsDateHashAndCoreSummaries()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(1),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Urgent,
                Surface = NarrativeSurface.GreatHall,
                Title = "急报一",
                Summary = "急事一件。",
            },
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(2),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Consequential,
                Surface = NarrativeSurface.GreatHall,
                Title = "缓报二",
                Summary = "缓事一件。",
            },
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(3),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Background,
                Surface = NarrativeSurface.DeskSandbox,
                Title = "杂讯三",
                Summary = "杂讯一件。",
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.CurrentDateLabel, Is.EqualTo("1200-02"));
        Assert.That(shell.GreatHall.ReplayHash, Is.EqualTo("cadence-hash"));
        Assert.That(shell.GreatHall.UrgentCount, Is.EqualTo(1));
        Assert.That(shell.GreatHall.ConsequentialCount, Is.EqualTo(1));
        Assert.That(shell.GreatHall.BackgroundCount, Is.EqualTo(1));
        Assert.That(shell.GreatHall.EducationSummary, Does.Contain("塾馆在读0人"));
        Assert.That(shell.GreatHall.TradeSummary, Does.Contain("市账1册"));
        Assert.That(shell.GreatHall.TradeSummary, Does.Contain("得利1支"));
        Assert.That(shell.GreatHall.LeadNoticeTitle, Is.EqualTo("急报一"));
    }

    [Test]
    public void Compose_ProjectsWarfareAftermathFallbacksWhenNoCampaignAftermathExists()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(CreateBundle());

        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("尚无战后案牍"));
        Assert.That(shell.DeskSandbox.Settlements, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].AftermathSummary, Is.EqualTo("战后案牍未起。"));
        Assert.That(shell.Warfare.Summary, Does.Contain("暂无军务"));
    }

    [Test]
    public void Compose_ProjectsFamilyCouncilCommandsAndReceipts()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(CreateBundle());

        Assert.That(shell.FamilyCouncil.Clans, Has.Count.EqualTo(1));
        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("承祧").Or.Contain("婚议").Or.Contain("举哀"));
        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("宜先议定承祧"));
        Assert.That(shell.FamilyCouncil.Clans[0].LifecycleSummary, Does.Contain("议亲定婚").And.Contain("承祧"));
        Assert.That(shell.FamilyCouncil.Clans[0].LifecycleSummary, Does.Contain("眼下宜先议定承祧"));
        Assert.That(shell.FamilyCouncil.Clans[0].ClanName, Is.EqualTo("清河张氏"));
        Assert.That(shell.FamilyCouncil.CommandAffordances, Has.Count.EqualTo(2));
        Assert.That(shell.FamilyCouncil.CommandAffordances.Any(static command => command.CommandName == PlayerCommandNames.InviteClanEldersMediation), Is.True);
        Assert.That(shell.FamilyCouncil.CommandAffordances.Any(static command => command.CommandName == PlayerCommandNames.DesignateHeirPolicy), Is.True);
        Assert.That(shell.FamilyCouncil.RecentReceipts, Has.Count.EqualTo(1));
        Assert.That(shell.FamilyCouncil.RecentReceipts[0].OutcomeSummary, Does.Contain("族老"));
        Assert.That(shell.FamilyCouncil.Summary, Does.Contain("婚事").And.Contain("承祧"));
        Assert.That(shell.FamilyCouncil.Summary, Does.Contain("眼下最宜先命清河张氏议定承祧。"));
    }

    [Test]
    public void Compose_ProjectsLineageTilesForHeirAndNonHeirClans()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Clans =
        [
            bundle.Clans[0],
            new ClanSnapshot
            {
                Id = new ClanId(2),
                ClanName = "B房李氏",
                HomeSettlementId = new SettlementId(1),
                Prestige = 41,
                SupportReserve = 27,
                BranchTension = 18,
                InheritancePressure = 22,
                SeparationPressure = 14,
                MediationMomentum = 11,
                MarriageAlliancePressure = 19,
                MarriageAllianceValue = 12,
                HeirSecurity = 9,
                ReproductivePressure = 23,
                MourningLoad = 0,
                LastLifecycleCommandLabel = "缓议婚帖",
                LastLifecycleOutcome = "暂缓一月再议。",
                LastConflictCommandLabel = string.Empty,
                LastConflictOutcome = string.Empty,
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.Lineage.Clans, Has.Count.EqualTo(2));
        ClanTileViewModel withHeir = shell.Lineage.Clans.Single(clan => clan.ClanName == "清河张氏");
        ClanTileViewModel withoutHeir = shell.Lineage.Clans.Single(clan => clan.ClanName == "B房李氏");
        Assert.That(withHeir.StatusText, Is.EqualTo("承祧之人已入谱。"));
        Assert.That(withoutHeir.StatusText, Is.EqualTo("宗房暂未举出承祧人。"));
        Assert.That(withoutHeir.SupportReserve, Is.EqualTo(27));
    }

    [Test]
    public void Compose_UsesOfficeFallbackWhenGovernanceProjectionIsAbsent()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.OfficeCareers = [];
        bundle.OfficeJurisdictions = [];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.Office.Summary, Does.Contain("暂无官署"));
        Assert.That(shell.DeskSandbox.Settlements[0].GovernanceSummary, Does.Contain("官署未设"));
    }

    [Test]
    public void Compose_ProjectsOfficeAppointmentsJurisdictionsAndCommands()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.OfficeCareers = [bundle.OfficeCareers[0] with
        {
            ServiceMonths = 6,
            PromotionPressureLabel = "rising",
            DemotionPressureLabel = "watched",
            LastOutcome = "Promoted",
        }];
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.Office,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
                    Label = "批覆词状",
                    Summary = "先把县门词状分轻重批下去。",
                    AvailabilitySummary = "兰溪县门积案未清，可先行发落。",
                    TargetLabel = "兰溪县门",
                    IsEnabled = true,
                },
            ],
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.Office,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
                    Label = "批覆词状",
                    Summary = "县门词状已按轻重批覆。",
                    OutcomeSummary = "先收急件，余案随后清理。",
                    TargetLabel = "兰溪县门",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.Office.Summary, Does.Contain("现有官人1名"));
        Assert.That(shell.Office.Summary, Does.Contain("积案最高7"));
        Assert.That(shell.Office.CommandAffordances, Has.Count.EqualTo(1));
        Assert.That(shell.Office.CommandAffordances[0].CommandName, Is.EqualTo(PlayerCommandNames.PetitionViaOfficeChannels));
        Assert.That(shell.Office.RecentReceipts, Has.Count.EqualTo(1));
        Assert.That(shell.Office.RecentReceipts[0].OutcomeSummary, Does.Contain("先收急件"));
        Assert.That(shell.Office.Appointments, Has.Count.EqualTo(1));
        Assert.That(shell.Office.Appointments[0].DisplayName, Is.EqualTo("张元"));
        Assert.That(shell.Office.Appointments[0].OfficeTitle, Is.EqualTo("主簿"));
        Assert.That(shell.Office.Jurisdictions, Has.Count.EqualTo(1));
        Assert.That(shell.Office.Jurisdictions[0].LeadSummary, Does.Contain("主簿"));
        Assert.That(shell.Office.Jurisdictions[0].LeadSummary, Does.Contain("张元"));
        Assert.That(shell.Office.Jurisdictions[0].TaskSummary, Does.Contain("勾理词状").Or.Contain("勘理词状"));
    }

    [Test]
    public void Compose_ProjectsPublicLifeAffordancesAndReceiptsIntoSettlementNodes()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = "张榜晓谕",
                    Summary = "先在县门榜下压住街谈。",
                    AvailabilitySummary = "榜示与街谈正在相争。",
                    TargetLabel = "县门榜下",
                    IsEnabled = true,
                },
            ],
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = "张榜晓谕",
                    Summary = "榜文已出，县门口风先压下去。",
                    OutcomeSummary = "街谈略收，榜示可见。",
                    TargetLabel = "县门榜下",
                },
            ],
        };
        bundle.PublicLifeSettlements =
        [
            new SettlementPublicLifeSnapshot
            {
                SettlementId = new SettlementId(1),
                SettlementName = "兰溪",
                SettlementTier = SettlementTier.CountySeat,
                NodeLabel = "县门榜下",
                DominantVenueCode = "county-gate",
                DominantVenueLabel = "县门榜下",
                MonthlyCadenceCode = "river-road-bustle",
                MonthlyCadenceLabel = "春汛行旅",
                CrowdMixLabel = "多见客商与脚。",
                StreetTalkHeat = 58,
                MarketBuzz = 47,
                NoticeVisibility = 61,
                RoadReportLag = 29,
                PrefectureDispatchPressure = 44,
                PublicLegitimacy = 53,
                DocumentaryWeight = 67,
                VerificationCost = 24,
                MarketRumorFlow = 55,
                CourierRisk = 31,
                OfficialNoticeLine = "榜下只说县门已经晓谕轻重。",
                StreetTalkLine = "街口都说埠上传来的话比榜文更紧。",
                RoadReportLine = "路上传来的脚信尚能递到县门。",
                PrefectureDispatchLine = "州牒催意已到，县里仍想缓出几分。",
                ContentionSummary = "榜文、街谈与脚信彼此牵扯，众人还在观望。",
                CadenceSummary = "值春汛行旅，县门榜下多见客商与脚夫。",
                PublicSummary = "街谈渐热，榜示亦重。",
                RouteReportSummary = "路报尚能递到县门。",
                ChannelSummary = "榜示分量稳住场面，市语仍在暗流。",
                LastPublicTrace = "县门榜下人语未散。",
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(settlementNode.PublicLifeCommandAffordances, Has.Count.EqualTo(1));
        Assert.That(settlementNode.PublicLifeCommandAffordances[0].CommandName, Is.EqualTo(PlayerCommandNames.PostCountyNotice));
        Assert.That(settlementNode.PublicLifeRecentReceipts, Has.Count.EqualTo(1));
        Assert.That(settlementNode.PublicLifeRecentReceipts[0].CommandName, Is.EqualTo(PlayerCommandNames.PostCountyNotice));
        Assert.That(settlementNode.PublicLifeSummary, Does.Contain("榜示"));
        Assert.That(settlementNode.PublicLifeSummary, Does.Contain("街谈").Or.Contain("观望").Or.Contain("路报"));
    }

    [Test]
    public void Compose_ProjectsGovernanceMomentumIntoGreatHallAndDeskSandbox()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.GovernanceSettlements =
        [
            new SettlementGovernanceLaneSnapshot
            {
                SettlementId = new SettlementId(1),
                SettlementName = "Lanxi",
                NodeLabel = "county-gate",
                LeadOfficialName = "Zhang Yuan",
                LeadOfficeTitle = "Registrar",
                CurrentAdministrativeTask = "district petition hearings",
                AdministrativeTaskLoad = 62,
                PetitionPressure = 51,
                PetitionBacklog = 9,
                PublicLegitimacy = 46,
                StreetTalkHeat = 64,
                RoutePressure = 37,
                SuppressionDemand = 22,
                PublicPressureSummary = "county gate pressure is not yet cleared",
                PublicMomentumSummary = "county gate momentum is tightening",
                GovernanceSummary = "registrar is still triaging petitions",
            },
        ];
        bundle.GovernanceFocus = new GovernanceFocusSnapshot
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            NodeLabel = "county-gate",
            UrgencyScore = 73,
            LeadSummary = "county gate petitions remain unsettled",
            PublicPressureSummary = "notice and street talk are both rising",
            PublicMomentumSummary = "county gate momentum is tightening",
            SuggestedCommandName = PlayerCommandNames.PostCountyNotice,
            SuggestedCommandLabel = "post notice",
            SuggestedCommandPrompt = "stabilize the county gate surface first",
        };
        bundle.GovernanceDocket = new GovernanceDocketSnapshot
        {
            SettlementId = new SettlementId(1),
            SettlementName = "Lanxi",
            NodeLabel = "county-gate",
            UrgencyScore = 73,
            Headline = "county gate petitions remain unsettled",
            WhyNowSummary = "notice and street talk are both pushing pressure toward the gate",
            PublicMomentumSummary = "county gate momentum is tightening",
            PhaseLabel = "needs response",
            PhaseSummary = "stabilize the gate before the queue worsens",
            HandlingSummary = "registrar is still triaging petitions",
            GuidanceSummary = "stabilize the county gate surface first",
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.GovernanceSummary, Does.Contain("county gate momentum is tightening"));
        Assert.That(shell.DeskSandbox.Settlements, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].GovernanceSummary, Does.Contain("county gate momentum is tightening"));
    }

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

    private static PresentationReadModelBundle CreateBundle()
    {
        return new PresentationReadModelBundle
        {
            CurrentDate = new GameDate(1200, 2),
            ReplayHash = "cadence-hash",
            Clans =
            [
                new ClanSnapshot
                {
                    Id = new ClanId(1),
                    ClanName = "清河张氏",
                    HomeSettlementId = new SettlementId(1),
                    Prestige = 62,
                    SupportReserve = 55,
                    HeirPersonId = new PersonId(1),
                    BranchTension = 61,
                    InheritancePressure = 44,
                    SeparationPressure = 38,
                    MediationMomentum = 36,
                    MarriageAlliancePressure = 42,
                    MarriageAllianceValue = 26,
                    HeirSecurity = 31,
                    ReproductivePressure = 48,
                    MourningLoad = 0,
                    LastLifecycleCommandLabel = "议亲定婚",
                    LastLifecycleOutcome = "婚事已议，门内暂可缓一缓承祧后议。",
                    LastConflictCommandLabel = "请族老调。",
                    LastConflictOutcome = "族老已入祠堂议事。",
                },
            ],
            ClanNarratives =
            [
                new ClanNarrativeSnapshot
                {
                    ClanId = new ClanId(1),
                    PublicNarrative = "祠堂里外都在议张氏分房。",
                    GrudgePressure = 34,
                    ShamePressure = 28,
                    FavorBalance = 12,
                },
            ],
            Settlements =
            [
                new SettlementSnapshot
                {
                    Id = new SettlementId(1),
                    Name = "兰溪",
                    Tier = SettlementTier.CountySeat,
                    Security = 63,
                    Prosperity = 66,
                },
            ],
            PopulationSettlements =
            [
                new PopulationSettlementSnapshot
                {
                    SettlementId = new SettlementId(1),
                    CommonerDistress = 35,
                    LaborSupply = 112,
                    MigrationPressure = 18,
                    MilitiaPotential = 70,
                },
            ],
            ClanTrades =
            [
                new ClanTradeSnapshot
                {
                    ClanId = new ClanId(1),
                    PrimarySettlementId = new SettlementId(1),
                    CashReserve = 92,
                    GrainReserve = 71,
                    Debt = 9,
                    CommerceReputation = 29,
                    ShopCount = 1,
                    LastOutcome = "Profit",
                    LastExplanation = "春社集期，河埠买卖略有盈余。",
                },
            ],
            PublicLifeSettlements =
            [
                new SettlementPublicLifeSnapshot
                {
                    SettlementId = new SettlementId(1),
                    SettlementName = "兰溪",
                    SettlementTier = SettlementTier.CountySeat,
                    NodeLabel = "县门榜下",
                    DominantVenueLabel = "街口茶肆",
                    DominantVenueCode = "teahouse-inn",
                    MonthlyCadenceCode = "spring-fair",
                    MonthlyCadenceLabel = "春社集日",
                    CrowdMixLabel = "多见客商、小贩与脚夫",
                    StreetTalkHeat = 63,
                    MarketBuzz = 58,
                    NoticeVisibility = 55,
                    RoadReportLag = 29,
                    PrefectureDispatchPressure = 47,
                    PublicLegitimacy = 52,
                    DocumentaryWeight = 59,
                    VerificationCost = 22,
                    MarketRumorFlow = 57,
                    CourierRisk = 24,
                    OfficialNoticeLine = "榜下只说县门已先晓谕轻重。",
                    StreetTalkLine = "街口都说茶肆听来的话更近实情。",
                    RoadReportLine = "路上传来的脚信尚能和门前榜示相互印证。",
                    PrefectureDispatchLine = "州牒催意已到，县里还想缓出几分。",
                    ContentionSummary = "榜文、街谈与脚信彼此牵扯，众人仍在观望。",
                    CadenceSummary = "值春社集日，街口茶肆多见客商、小贩与脚夫。",
                    PublicSummary = "街谈渐热，镇市喧起。",
                    RouteReportSummary = "路报尚能递到县门。",
                    LastPublicTrace = "县门榜下街谈渐热。",
                },
            ],
            OfficeCareers =
            [
                new OfficeCareerSnapshot
                {
                    PersonId = new PersonId(1),
                    ClanId = new ClanId(1),
                    SettlementId = new SettlementId(1),
                    DisplayName = "张元",
                    HasAppointment = true,
                    OfficeTitle = "主簿",
                    AuthorityTier = 2,
                    JurisdictionLeverage = 58,
                    PetitionPressure = 24,
                    PetitionBacklog = 7,
                    CurrentAdministrativeTask = "勾理词状",
                    AdministrativeTaskTier = "district",
                    PetitionOutcomeCategory = "Triaged",
                    LastPetitionOutcome = "分轻重，先收县门词状。",
                },
            ],
            OfficeJurisdictions =
            [
                new JurisdictionAuthoritySnapshot
                {
                    SettlementId = new SettlementId(1),
                    LeadOfficialPersonId = new PersonId(1),
                    LeadOfficialName = "张元",
                    LeadOfficeTitle = "主簿",
                    AuthorityTier = 2,
                    JurisdictionLeverage = 58,
                    PetitionPressure = 24,
                    PetitionBacklog = 7,
                    CurrentAdministrativeTask = "勾理词状",
                    AdministrativeTaskTier = "district",
                    PetitionOutcomeCategory = "Triaged",
                    LastPetitionOutcome = "分轻重，先收县门词状。",
                },
            ],
            Notifications =
            [
                new NarrativeNotificationSnapshot
                {
                    Id = new NotificationId(1),
                    CreatedAt = new GameDate(1200, 2),
                    Tier = NotificationTier.Consequential,
                    Surface = NarrativeSurface.GreatHall,
                    Title = "县门榜示",
                    Summary = "春社集日前后，县门议论渐起。",
                },
            ],
            PlayerCommands = new PlayerCommandSurfaceSnapshot
            {
                Affordances =
                [
                    new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.DesignateHeirPolicy,
                    Label = "议定承祧",
                    Summary = "先把承祧次序与谱内名分写稳。",
                    AvailabilitySummary = "承祧稳度31，名分若虚仍易再起后议。",
                    TargetLabel = "清河张氏",
                    IsEnabled = true,
                },
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.InviteClanEldersMediation,
                    Label = "请族老调。",
                    Summary = "请族老入祠堂缓争，先压分房之议。",
                    AvailabilitySummary = "族老与房亲都在县城，可即刻议事。",
                    TargetLabel = "清河张氏",
                        IsEnabled = true,
                    },
                ],
                Receipts =
                [
                    new PlayerCommandReceiptSnapshot
                    {
                        ModuleKey = KnownModuleKeys.FamilyCore,
                        SurfaceKey = PlayerCommandSurfaceKeys.Family,
                        SettlementId = new SettlementId(1),
                        ClanId = new ClanId(1),
                        CommandName = PlayerCommandNames.InviteClanEldersMediation,
                        Label = "请族老调。",
                        Summary = "族老已在祠堂聚首。",
                        OutcomeSummary = "族老先收两房说辞，缓下当面争口。",
                        TargetLabel = "清河张氏",
                    },
                ],
            },
        };
    }
}

