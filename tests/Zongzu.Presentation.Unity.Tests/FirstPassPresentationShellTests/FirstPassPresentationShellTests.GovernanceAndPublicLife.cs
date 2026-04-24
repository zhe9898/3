using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

public sealed partial class FirstPassPresentationShellTests
{
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
    public void Compose_UsesPublicLifeFallbackWhenProjectionAndCommandsAreAbsent()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PublicLifeSettlements = [];
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot();

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(shell.GreatHall.PublicLifeSummary, Is.Not.Empty);
        Assert.That(settlementNode.PublicLifeSummary, Is.Not.Empty);
        Assert.That(settlementNode.PublicLifeCommandAffordances, Is.Empty);
        Assert.That(settlementNode.PublicLifeRecentReceipts, Is.Empty);
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

}
