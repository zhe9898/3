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
        Assert.That(shell.FamilyCouncil.Summary, Does.Contain("眼下最宜先命清河张氏议定承祧"));
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
                CrowdMixLabel = "多见客商与脚夫",
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
        Assert.That(shell.GreatHall.LeadNoticeGuidance, Does.Contain("眼下最宜先命清河张氏拨粮护婴"));
        Assert.That(shell.NotificationCenter.Items, Has.Count.EqualTo(1));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("襁褓"));
        Assert.That(shell.NotificationCenter.Items[0].WhatNext, Does.Contain("眼下最宜先命清河张氏拨粮护婴"));
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
                    LastConflictCommandLabel = "请族老调停",
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
                    Label = "请族老调停",
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
                        Label = "请族老调停",
                        Summary = "族老已在祠堂聚首。",
                        OutcomeSummary = "族老先收两房说辞，缓下当面争口。",
                        TargetLabel = "清河张氏",
                    },
                ],
            },
        };
    }
}
