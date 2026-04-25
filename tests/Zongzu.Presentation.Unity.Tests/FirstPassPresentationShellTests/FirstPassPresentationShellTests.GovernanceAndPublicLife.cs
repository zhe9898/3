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
    public void Compose_ProjectsOwnerLaneReturnGuidanceInOfficeAndFamilySurfacesWithoutShellAuthority()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        const string officeGuidance = "外部后账归位：该走县门/文移 lane（OfficeAndCareer）：张户本户这头吃紧，县门未落地、文移拖延和胥吏续拖仍回官署案头；本户不能代修。承接入口：回到本 lane 先看押文催县门、改走递报；常规官署仍看批覆词状或发签催办。";
        const string familyGuidance = "外部后账归位：该走族老/担保 lane（FamilyCore）：张户本户这头吃紧，族老解释、本户担保和宗房脸面仍回族中公开说法；本户不能代修。承接入口：回到本 lane 先看请族老解释、请族老出面；宗房内面仍看请族老调停。";
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
                    Label = "批阅词状",
                    Summary = "先把县门词状分轻重批下去。",
                    AvailabilitySummary = "县门积案未清，可先行发落。",
                    LeverageSummary = officeGuidance,
                    ReadbackSummary = officeGuidance,
                    TargetLabel = "兰溪县门",
                    IsEnabled = true,
                },
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.InviteClanEldersMediation,
                    Label = "请族老调停",
                    Summary = "请族老入祠堂缓争，先压担保后账。",
                    AvailabilitySummary = "族老与房亲都在县城，可即刻议事。",
                    LeverageSummary = familyGuidance,
                    ReadbackSummary = familyGuidance,
                    TargetLabel = "清河张氏",
                    IsEnabled = true,
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        CommandAffordanceViewModel office = shell.Office.CommandAffordances
            .Single(command => command.CommandName == PlayerCommandNames.PetitionViaOfficeChannels);
        Assert.That(office.LeverageSummary, Does.Contain("外部后账归位"));
        Assert.That(office.LeverageSummary, Does.Contain("该走县门/文移 lane"));
        Assert.That(office.LeverageSummary, Does.Contain("本户不能代修"));
        Assert.That(office.LeverageSummary, Does.Contain("承接入口"));
        Assert.That(office.LeverageSummary, Does.Contain("押文催县门"));
        Assert.That(office.ReadbackSummary, Is.EqualTo(officeGuidance));

        CommandAffordanceViewModel family = shell.FamilyCouncil.CommandAffordances
            .Single(command => command.CommandName == PlayerCommandNames.InviteClanEldersMediation);
        Assert.That(family.LeverageSummary, Does.Contain("外部后账归位"));
        Assert.That(family.LeverageSummary, Does.Contain("该走族老/担保 lane"));
        Assert.That(family.LeverageSummary, Does.Contain("本户不能代修"));
        Assert.That(family.LeverageSummary, Does.Contain("承接入口"));
        Assert.That(family.LeverageSummary, Does.Contain("请族老解释"));
        Assert.That(family.ReadbackSummary, Is.EqualTo(familyGuidance));
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
                    LeverageSummary = "本户可借宗族体面与官面触达。",
                    CostSummary = "代价是钱粮与人情先垫出去。",
                    ReadbackSummary = "下月读回看街谈与路压。",
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
                    LeverageSummary = "本户已动用榜下熟人。",
                    CostSummary = "代价留在人情与担保里。",
                    ReadbackSummary = "下月读回看榜下街谈。",
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
        CommandAffordanceViewModel affordance = settlementNode.PublicLifeCommandAffordances[0];
        Assert.That(affordance.CommandName, Is.EqualTo(PlayerCommandNames.PostCountyNotice));
        Assert.That(affordance.LeverageSummary, Does.Contain("宗族体面"));
        Assert.That(affordance.CostSummary, Does.Contain("代价"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("下月读回"));
        Assert.That(settlementNode.PublicLifeRecentReceipts, Has.Count.EqualTo(1));
        CommandReceiptViewModel receipt = settlementNode.PublicLifeRecentReceipts[0];
        Assert.That(receipt.CommandName, Is.EqualTo(PlayerCommandNames.PostCountyNotice));
        Assert.That(receipt.LeverageSummary, Does.Contain("本户"));
        Assert.That(receipt.CostSummary, Does.Contain("代价"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("下月读回"));
        Assert.That(settlementNode.PublicLifeSummary, Does.Contain("榜示"));
        Assert.That(settlementNode.PublicLifeSummary, Does.Contain("街谈").Or.Contain("观望").Or.Contain("路报"));
    }

    [Test]
    public void Compose_ProjectsSocialMemoryOrderReadbackWithoutShellAuthority()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.FundLocalWatch,
                    Label = "添雇巡丁",
                    Summary = "巡丁已经补到路口。",
                    OutcomeSummary = "路口暂稳。",
                    ReadbackSummary = "县门未落地：添雇巡丁被拒；巡丁不应、脚户误读，本户公开担保失手，后账仍在。社会记忆读回：羞面46，张氏因上月添雇巡丁被地方拒住留下巡丁不应后的公开担保失败。",
                    TargetLabel = "县门榜下",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        CommandReceiptViewModel receipt = shell.DeskSandbox.Settlements.Single().PublicLifeRecentReceipts.Single();

        Assert.That(receipt.CommandName, Is.EqualTo(PlayerCommandNames.FundLocalWatch));
        Assert.That(receipt.ReadbackSummary, Does.Contain("县门未落地"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("后账仍在"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("社会记忆读回"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("添雇巡丁"));
    }

    [Test]
    public void Compose_ProjectsPublicLifeResponseReadbackWithoutShellAuthority()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
                    Label = "补保巡丁",
                    Summary = "巡丁后账已露出来，可补保巡丁。",
                    IsEnabled = true,
                    AvailabilitySummary = "前案半落地。",
                    ReadbackSummary = "后账已修复：护路担保重新接住。",
                    TargetLabel = "县门榜下",
                },
            ],
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
                    Label = "补保巡丁",
                    Summary = "本户补出担保与口粮。",
                    OutcomeSummary = "后账已修复",
                    ReadbackSummary = "后账已修复：护路担保重新接住。社会记忆读回：人情27。",
                    TargetLabel = "县门榜下",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(settlementNode.PublicLifeCommandAffordances.Single().ReadbackSummary, Does.Contain("后账已修复"));
        Assert.That(settlementNode.PublicLifeRecentReceipts.Single().ReadbackSummary, Does.Contain("社会记忆读回"));
    }

    [Test]
    public void Compose_ProjectsActorCountermoveReadbackWithoutShellAuthority()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.CompensateRunnerMisread,
                    Label = "脚户误读反噬",
                    Summary = "路上后账被脚户误读，怨尾再起。",
                    OutcomeSummary = "后账恶化",
                    ReadbackSummary = "脚户误读反噬：后账恶化，地面反噬与恐惧继续加重。",
                    TargetLabel = "县门榜下",
                },
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.PressCountyYamenDocument,
                    Label = "县门自补落地",
                    Summary = "县门自行把前案补入正道。",
                    OutcomeSummary = "后账已修复",
                    ReadbackSummary = "县门自补落地：县门已补落地，文移进入案牍正道。",
                    TargetLabel = "主簿",
                },
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    ClanId = new ClanId(1),
                    CommandName = PlayerCommandNames.AskClanEldersExplain,
                    Label = "族老自解释",
                    Summary = "族老私下解释前案。",
                    OutcomeSummary = "后账已修复",
                    ReadbackSummary = "族老自解释：族老解释缓下羞面，本户担保重新站住。",
                    TargetLabel = "张宗",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(settlementNode.PublicLifeRecentReceipts.Select(static receipt => receipt.Label), Does.Contain("脚户误读反噬"));
        Assert.That(settlementNode.PublicLifeRecentReceipts.Select(static receipt => receipt.Label), Does.Contain("县门自补落地"));
        Assert.That(settlementNode.PublicLifeRecentReceipts.Select(static receipt => receipt.Label), Does.Contain("族老自解释"));
        Assert.That(
            settlementNode.PublicLifeRecentReceipts.Select(static receipt => receipt.ReadbackSummary),
            Has.Some.Contains("后账恶化"));
        Assert.That(
            settlementNode.PublicLifeRecentReceipts.Select(static receipt => receipt.ReadbackSummary),
            Has.Some.Contains("县门已补落地"));
        Assert.That(
            settlementNode.PublicLifeRecentReceipts.Select(static receipt => receipt.ReadbackSummary),
            Has.Some.Contains("族老解释缓下羞面"));
    }

    [Test]
    public void Compose_ProjectsOrdinaryHouseholdOrderResiduePressureWithoutShellAuthority()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.HouseholdSocialPressures =
        [
            new HouseholdSocialPressureSnapshot
            {
                HouseholdId = new HouseholdId(44),
                HouseholdName = "Kiln Wu household",
                SettlementId = new SettlementId(1),
                SettlementName = "Lanxi",
                Livelihood = LivelihoodType.HiredLabor,
                LivelihoodLabel = "hired labor",
                PrimaryDriftKey = HouseholdSocialDriftKeys.PublicOrderAftermath,
                PrimaryDriftLabel = "road-watch after-account",
                PressureScore = 78,
                PressureBandLabel = "urgent",
                IsPlayerAnchor = false,
                AttachmentSummary = "ordinary household near the road mouth",
                PressureSummary = "Kiln Wu household still reads the road-watch after-account.",
                VisibleChainSummary = "Kiln Wu household sees route pressure and runner misread.",
                Signals =
                [
                    new HouseholdSocialPressureSignalSnapshot
                    {
                        SignalKey = HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue,
                        Label = "order residue",
                        Score = 82,
                        Summary = "Kiln Wu household still reads the road-watch after-account from runner misread.",
                        SourceModuleKeys =
                        [
                            KnownModuleKeys.PopulationAndHouseholds,
                            KnownModuleKeys.OrderAndBanditry,
                        ],
                    },
                ],
                SourceModuleKeys =
                [
                    KnownModuleKeys.PopulationAndHouseholds,
                    KnownModuleKeys.OrderAndBanditry,
                ],
            },
        ];
        bundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
                    Label = "repair watch guarantee",
                    Summary = "costed response choice",
                    IsEnabled = true,
                    AvailabilitySummary = "Kiln Wu household is the visible ordinary-household stake.",
                    ExecutionSummary = "OrderAndBanditry resolves the response.",
                    LeverageSummary = "Kiln Wu household carries the road-watch pressure.",
                    CostSummary = "Spend guarantee on Kiln Wu household first.",
                    ReadbackSummary = "Next readback watches Kiln Wu household.",
                    TargetLabel = "road mouth / Kiln Wu household",
                },
            ],
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
                    Label = "repair watch guarantee",
                    Summary = "response resolved",
                    OutcomeSummary = "contained",
                    LeverageSummary = "Kiln Wu household remains on the receipt.",
                    CostSummary = "Kiln Wu household cost is visible.",
                    ReadbackSummary = "Kiln Wu household readback is projected only.",
                    TargetLabel = "road mouth / Kiln Wu household",
                },
            ],
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(settlementNode.PressureSummary, Does.Contain("Kiln Wu household"));
        Assert.That(settlementNode.PressureSummary, Does.Contain("road-watch after-account"));
        Assert.That(settlementNode.PressureSummary, Does.Contain("runner misread"));
        Assert.That(settlementNode.PublicLifeCommandAffordances.Single().TargetLabel, Does.Contain("Kiln Wu household"));
        Assert.That(settlementNode.PublicLifeCommandAffordances.Single().LeverageSummary, Does.Contain("Kiln Wu household"));
        Assert.That(settlementNode.PublicLifeCommandAffordances.Single().CostSummary, Does.Contain("Kiln Wu household"));
        Assert.That(settlementNode.PublicLifeCommandAffordances.Single().ReadbackSummary, Does.Contain("Kiln Wu household"));
        Assert.That(settlementNode.PublicLifeRecentReceipts.Single().TargetLabel, Does.Contain("Kiln Wu household"));
        Assert.That(settlementNode.PublicLifeRecentReceipts.Single().ReadbackSummary, Does.Contain("projected only"));
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
