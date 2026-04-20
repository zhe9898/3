using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.PublicLifeAndRumor;

namespace Zongzu.Modules.PublicLifeAndRumor.Tests;

[TestFixture]
public sealed class PublicLifeAndRumorModuleTests
{
    [Test]
    public void RunXun_RefreshesSettlementPulseWithoutReadableOutputAndAppliesShangxunBias()
    {
        PublicLifeAndRumorModule module = new();
        QueryRegistry queries = CreateSharedQueries();
        FeatureManifest manifest = CreatePublicLifeManifest();

        PublicLifeAndRumorState xunState = module.CreateInitialState();
        ModuleExecutionContext xunContext = CreateContext(
            new GameDate(1200, 4),
            manifest,
            queries,
            seed: 19,
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Shangxun);

        module.RunXun(new ModuleExecutionScope<PublicLifeAndRumorState>(xunState, xunContext));

        Assert.That(xunState.Settlements, Has.Count.EqualTo(1));
        SettlementPublicLifeState xunSettlement = xunState.Settlements.Single();
        Assert.That(xunSettlement.SettlementName, Is.EqualTo("Lanxi"));
        Assert.That(xunSettlement.NodeLabel, Is.EqualTo("Lanxi县门"));
        Assert.That(xunSettlement.MonthlyCadenceCode, Is.EqualTo("river-road-bustle"));
        Assert.That(xunSettlement.MonthlyCadenceLabel, Is.EqualTo("春汛行旅"));
        Assert.That(xunSettlement.DominantVenueCode, Is.Not.Empty);
        Assert.That(xunSettlement.DominantVenueLabel, Is.Not.Empty);
        Assert.That(xunSettlement.CrowdMixLabel, Is.Not.Empty);
        Assert.That(xunSettlement.CadenceSummary, Does.Contain("春汛行旅"));
        Assert.That(xunSettlement.PublicSummary, Does.Contain("街谈"));
        Assert.That(xunSettlement.RouteReportSummary, Does.Contain("Lanxi Ferry Run"));
        Assert.That(xunSettlement.ChannelSummary, Does.Contain("榜示分量"));
        Assert.That(xunSettlement.LastPublicTrace, Does.Contain("Lanxi县门"));
        Assert.That(xunContext.Diff.Entries, Is.Empty);
        Assert.That(xunContext.DomainEvents.Events, Is.Empty);

        PublicLifeAndRumorState monthState = module.CreateInitialState();
        ModuleExecutionContext monthContext = CreateContext(
            new GameDate(1200, 4),
            manifest,
            queries,
            seed: 19);

        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(monthState, monthContext));

        SettlementPublicLifeState monthSettlement = monthState.Settlements.Single();
        Assert.That(xunSettlement.MarketBuzz, Is.GreaterThan(monthSettlement.MarketBuzz));
        Assert.That(xunSettlement.RoadReportLag, Is.LessThan(monthSettlement.RoadReportLag));
    }

    [Test]
    public void RunXun_XiaxunAmplifiesHotYamenSurfaceFromOfficeTaskLoadAndClerkDependence()
    {
        PublicLifeAndRumorModule module = new();
        FeatureManifest manifest = CreatePublicLifeManifest();

        QueryRegistry hotQueries = CreateSharedQueries();
        hotQueries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [],
        [
            CreateJurisdictionSnapshot(
                clerkDependence: 68,
                administrativeTaskLoad: 76,
                petitionPressure: 24,
                petitionBacklog: 18,
                jurisdictionLeverage: 42),
        ]));

        QueryRegistry calmQueries = CreateSharedQueries();
        calmQueries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [],
        [
            CreateJurisdictionSnapshot(
                clerkDependence: 10,
                administrativeTaskLoad: 12,
                petitionPressure: 24,
                petitionBacklog: 18,
                jurisdictionLeverage: 42),
        ]));

        PublicLifeAndRumorState hotState = module.CreateInitialState();
        PublicLifeAndRumorState calmState = module.CreateInitialState();
        ModuleExecutionContext hotContext = CreateContext(
            new GameDate(1200, 4),
            manifest,
            hotQueries,
            seed: 31,
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Xiaxun);
        ModuleExecutionContext calmContext = CreateContext(
            new GameDate(1200, 4),
            manifest,
            calmQueries,
            seed: 31,
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Xiaxun);

        module.RunXun(new ModuleExecutionScope<PublicLifeAndRumorState>(hotState, hotContext));
        module.RunXun(new ModuleExecutionScope<PublicLifeAndRumorState>(calmState, calmContext));

        SettlementPublicLifeState hotSettlement = hotState.Settlements.Single();
        SettlementPublicLifeState calmSettlement = calmState.Settlements.Single();

        Assert.That(hotSettlement.NoticeVisibility, Is.GreaterThan(calmSettlement.NoticeVisibility));
        Assert.That(hotSettlement.PrefectureDispatchPressure, Is.GreaterThan(calmSettlement.PrefectureDispatchPressure));
        Assert.That(hotSettlement.RoadReportLag, Is.GreaterThan(calmSettlement.RoadReportLag));
        Assert.That(hotSettlement.DocumentaryWeight, Is.GreaterThan(calmSettlement.DocumentaryWeight));
        Assert.That(hotSettlement.PublicLegitimacy, Is.LessThan(calmSettlement.PublicLegitimacy));
        Assert.That(hotContext.Diff.Entries, Is.Empty);
        Assert.That(calmContext.Diff.Entries, Is.Empty);
        Assert.That(hotContext.DomainEvents.Events, Is.Empty);
        Assert.That(calmContext.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_BuildsCountyPulseAndEmitsPublicEvent()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        QueryRegistry queries = CreateSharedQueries();
        FeatureManifest manifest = CreatePublicLifeManifest();
        ModuleExecutionContext context = CreateContext(
            new GameDate(1200, 4),
            manifest,
            queries,
            seed: 19);

        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(state, context));

        Assert.That(state.Settlements, Has.Count.EqualTo(1));
        SettlementPublicLifeState settlement = state.Settlements.Single();
        Assert.That(settlement.SettlementName, Is.EqualTo("Lanxi"));
        Assert.That(settlement.NodeLabel, Is.EqualTo("Lanxi县门"));
        Assert.That(settlement.MonthlyCadenceCode, Is.EqualTo("river-road-bustle"));
        Assert.That(settlement.MonthlyCadenceLabel, Is.EqualTo("春汛行旅"));
        Assert.That(settlement.CrowdMixLabel, Is.Not.Empty);
        Assert.That(settlement.DominantVenueCode, Is.Not.Empty);
        Assert.That(settlement.DominantVenueLabel, Is.Not.Empty);
        Assert.That(settlement.DocumentaryWeight, Is.InRange(0, 100));
        Assert.That(settlement.VerificationCost, Is.InRange(0, 100));
        Assert.That(settlement.MarketRumorFlow, Is.InRange(0, 100));
        Assert.That(settlement.CourierRisk, Is.InRange(0, 100));
        Assert.That(settlement.OfficialNoticeLine, Is.Not.Empty);
        Assert.That(settlement.StreetTalkLine, Is.Not.Empty);
        Assert.That(settlement.RoadReportLine, Does.Contain("Lanxi Ferry Run"));
        Assert.That(settlement.PrefectureDispatchLine, Is.Not.Empty);
        Assert.That(settlement.ContentionSummary, Is.Not.Empty);
        Assert.That(settlement.CadenceSummary, Does.Contain("春汛行旅"));
        Assert.That(settlement.PublicSummary, Does.Contain("街谈"));
        Assert.That(settlement.RouteReportSummary, Does.Contain("Lanxi Ferry Run"));
        Assert.That(settlement.ChannelSummary, Does.Contain("榜示分量"));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(1));
        Assert.That(context.Diff.Entries[0].Description, Does.Contain("Lanxi县门"));
        Assert.That(context.DomainEvents.Events, Has.Count.EqualTo(1));
        Assert.That(context.DomainEvents.Events[0].EventType, Is.EqualTo(PublicLifeAndRumorEventNames.PrefectureDispatchPressed));
        Assert.That(context.DomainEvents.Events[0].Summary, Does.Contain("Lanxi"));
        Assert.That(context.DomainEvents.Events[0].Summary, Does.Contain("春汛行旅"));
    }

    [Test]
    public void RunMonth_ChangesMonthlyCadenceAcrossSeasons()
    {
        PublicLifeAndRumorModule module = new();
        QueryRegistry queries = CreateSharedQueries();
        FeatureManifest manifest = CreatePublicLifeManifest();

        PublicLifeAndRumorState springState = module.CreateInitialState();
        PublicLifeAndRumorState winterState = module.CreateInitialState();
        ModuleExecutionContext springContext = CreateContext(
            new GameDate(1200, 2),
            manifest,
            queries,
            seed: 77);
        ModuleExecutionContext winterContext = CreateContext(
            new GameDate(1200, 11),
            manifest,
            queries,
            seed: 77);

        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(springState, springContext));
        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(winterState, winterContext));

        SettlementPublicLifeState spring = springState.Settlements.Single();
        SettlementPublicLifeState winter = winterState.Settlements.Single();

        Assert.That(spring.MonthlyCadenceCode, Is.EqualTo("spring-fair"));
        Assert.That(winter.MonthlyCadenceCode, Is.EqualTo("year-end-docket"));
        Assert.That(spring.MonthlyCadenceLabel, Is.EqualTo("春社集日"));
        Assert.That(winter.MonthlyCadenceLabel, Is.EqualTo("岁末催科"));
        Assert.That(spring.CadenceSummary, Is.Not.EqualTo(winter.CadenceSummary));
    }

    [Test]
    public void RunMonth_KeepsOfficeTaskLoadAndClerkDependenceOutOfMonthPublicPulse()
    {
        PublicLifeAndRumorModule module = new();
        FeatureManifest manifest = CreatePublicLifeManifest();

        QueryRegistry hotQueries = CreateSharedQueries();
        hotQueries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [],
        [
            CreateJurisdictionSnapshot(
                clerkDependence: 68,
                administrativeTaskLoad: 76,
                petitionPressure: 24,
                petitionBacklog: 18,
                jurisdictionLeverage: 42),
        ]));

        QueryRegistry calmQueries = CreateSharedQueries();
        calmQueries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [],
        [
            CreateJurisdictionSnapshot(
                clerkDependence: 10,
                administrativeTaskLoad: 12,
                petitionPressure: 24,
                petitionBacklog: 18,
                jurisdictionLeverage: 42),
        ]));

        PublicLifeAndRumorState hotState = module.CreateInitialState();
        PublicLifeAndRumorState calmState = module.CreateInitialState();
        ModuleExecutionContext hotContext = CreateContext(
            new GameDate(1200, 4),
            manifest,
            hotQueries,
            seed: 31);
        ModuleExecutionContext calmContext = CreateContext(
            new GameDate(1200, 4),
            manifest,
            calmQueries,
            seed: 31);

        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(hotState, hotContext));
        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(calmState, calmContext));

        SettlementPublicLifeState hotSettlement = hotState.Settlements.Single();
        SettlementPublicLifeState calmSettlement = calmState.Settlements.Single();

        Assert.That(hotSettlement.NoticeVisibility, Is.EqualTo(calmSettlement.NoticeVisibility));
        Assert.That(hotSettlement.PrefectureDispatchPressure, Is.EqualTo(calmSettlement.PrefectureDispatchPressure));
        Assert.That(hotSettlement.RoadReportLag, Is.EqualTo(calmSettlement.RoadReportLag));
        Assert.That(hotSettlement.DocumentaryWeight, Is.EqualTo(calmSettlement.DocumentaryWeight));
        Assert.That(hotSettlement.PublicLegitimacy, Is.EqualTo(calmSettlement.PublicLegitimacy));
    }

    [Test]
    public void RegisterQueries_ExposesSettlementPublicPulseForReadModels()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(3),
            SettlementName = "North Ford",
            SettlementTier = SettlementTier.MarketTown,
            DominantVenueCode = "wharf-ferry",
            NodeLabel = "North Ford镇市",
            DominantVenueLabel = "河埠歇脚棚",
            MonthlyCadenceCode = "spring-fair",
            MonthlyCadenceLabel = "春社集日",
            CrowdMixLabel = "多见客商、小贩与脚夫",
            StreetTalkHeat = 52,
            MarketBuzz = 61,
            NoticeVisibility = 33,
            RoadReportLag = 44,
            PrefectureDispatchPressure = 28,
            PublicLegitimacy = 49,
            DocumentaryWeight = 61,
            VerificationCost = 26,
            MarketRumorFlow = 57,
            CourierRisk = 33,
            OfficialNoticeLine = "榜下已说县门会先分轻重。",
            StreetTalkLine = "街口都在议论北津的新消息。",
            RoadReportLine = "由North Ford Ferry递来的消息尚能相互印证。",
            PrefectureDispatchLine = "州里已有催意，但县门还留着回旋。",
            ContentionSummary = "榜文、街谈与路报彼此牵制，众人仍在观望。",
            CadenceSummary = "值春社集日，河埠歇脚棚多见客商、小贩与脚夫。",
            PublicSummary = "今月河埠歇脚棚最见动静：街谈52，市喧61，榜示33，众情向背49。",
            RouteReportSummary = "North Ford Ferry眼下迟滞44，牒报与词状并行催并。",
            LastPublicTrace = "North Ford镇市值春社集日，河埠歇脚棚多见客商、小贩与脚夫。",
            ChannelSummary = "此处榜示分量61，市语流势57，查验周折26，递报险数33。",
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        IPublicLifeAndRumorQueries publicLifeQueries = queries.GetRequired<IPublicLifeAndRumorQueries>();

        SettlementPublicLifeSnapshot snapshot = publicLifeQueries.GetRequiredSettlementPublicLife(new SettlementId(3));

        Assert.That(snapshot.NodeLabel, Is.EqualTo("North Ford镇市"));
        Assert.That(snapshot.DominantVenueLabel, Is.EqualTo("河埠歇脚棚"));
        Assert.That(snapshot.MonthlyCadenceLabel, Is.EqualTo("春社集日"));
        Assert.That(snapshot.CadenceSummary, Does.Contain("春社"));
        Assert.That(snapshot.PublicSummary, Does.Contain("街谈52"));
        Assert.That(snapshot.OfficialNoticeLine, Does.Contain("县门"));
        Assert.That(snapshot.StreetTalkLine, Does.Contain("北津"));
        Assert.That(snapshot.RoadReportLine, Does.Contain("North Ford Ferry"));
        Assert.That(snapshot.PrefectureDispatchLine, Does.Contain("州里"));
        Assert.That(snapshot.ContentionSummary, Does.Contain("观望"));
        Assert.That(publicLifeQueries.GetSettlementPublicLife(), Has.Count.EqualTo(1));
    }

    private static ModuleExecutionContext CreateContext(
        GameDate date,
        FeatureManifest manifest,
        QueryRegistry queries,
        int seed,
        SimulationCadenceBand cadenceBand = SimulationCadenceBand.Month,
        SimulationXun currentXun = SimulationXun.None)
    {
        return new ModuleExecutionContext(
            date,
            manifest,
            new DeterministicRandom(KernelState.Create(seed)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: cadenceBand,
            currentXun: currentXun);
    }

    private static FeatureManifest CreatePublicLifeManifest()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Lite);
        return manifest;
    }

    private static QueryRegistry CreateSharedQueries()
    {
        QueryRegistry queries = new();
        queries.Register<IWorldSettlementsQueries>(new StubWorldQueries(
        [
            new SettlementSnapshot
            {
                Id = new SettlementId(1),
                Name = "Lanxi",
                Tier = SettlementTier.CountySeat,
                Security = 58,
                Prosperity = 66,
            },
        ]));
        queries.Register<IPopulationAndHouseholdsQueries>(new StubPopulationQueries(
        [
            new PopulationSettlementSnapshot
            {
                SettlementId = new SettlementId(1),
                CommonerDistress = 48,
                LaborSupply = 106,
                MigrationPressure = 27,
                MilitiaPotential = 71,
            },
        ]));
        queries.Register<ITradeAndIndustryQueries>(new StubTradeQueries(
        [
            new ClanTradeSnapshot
            {
                ClanId = new ClanId(1),
                PrimarySettlementId = new SettlementId(1),
                CashReserve = 84,
                GrainReserve = 62,
                Debt = 16,
                CommerceReputation = 34,
                ShopCount = 1,
                LastOutcome = "Profit",
                LastExplanation = "Lanxi market stayed active.",
            },
        ],
        [
            new MarketSnapshot
            {
                SettlementId = new SettlementId(1),
                MarketName = "Lanxi Market Street",
                PriceIndex = 107,
                Demand = 73,
                LocalRisk = 18,
            },
        ],
        [
            new ClanTradeRouteSnapshot
            {
                RouteId = 1,
                ClanId = new ClanId(1),
                RouteName = "Lanxi Ferry Run",
                SettlementId = new SettlementId(1),
                IsActive = true,
                Capacity = 24,
                Risk = 36,
                LastMargin = 7,
            },
        ]));
        queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries(
        [
            new SettlementDisorderSnapshot
            {
                SettlementId = new SettlementId(1),
                BanditThreat = 26,
                RoutePressure = 31,
                SuppressionDemand = 18,
                DisorderPressure = 29,
                LastPressureReason = "Ferry whispers have started to spread.",
            },
        ]));
        queries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [],
        [
            new JurisdictionAuthoritySnapshot
            {
                SettlementId = new SettlementId(1),
                LeadOfficialPersonId = new PersonId(1),
                LeadOfficialName = "Zhang Yuan",
                LeadOfficeTitle = "县主簿",
                AuthorityTier = 2,
                JurisdictionLeverage = 59,
                PetitionPressure = 44,
                PetitionBacklog = 28,
                CurrentAdministrativeTask = "勘理词状",
                AdministrativeTaskTier = "district",
                LastPetitionOutcome = "门前已先分轻重缓急。",
                PetitionOutcomeCategory = "Triaged",
                LastAdministrativeTrace = "县门词状与榜示并压。",
            },
        ]));
        queries.Register<IFamilyCoreQueries>(new StubFamilyQueries(
        [
            new ClanSnapshot
            {
                Id = new ClanId(1),
                ClanName = "Zhang",
                HomeSettlementId = new SettlementId(1),
                Prestige = 57,
                SupportReserve = 61,
                HeirPersonId = new PersonId(2),
            },
        ]));
        queries.Register<ISocialMemoryAndRelationsQueries>(new StubSocialQueries(
        [
            new ClanNarrativeSnapshot
            {
                ClanId = new ClanId(1),
                PublicNarrative = "People in town are still discussing the Zhang household.",
                GrudgePressure = 24,
                FearPressure = 18,
                ShamePressure = 16,
                FavorBalance = 12,
            },
        ]));
        return queries;
    }

    private static JurisdictionAuthoritySnapshot CreateJurisdictionSnapshot(
        int clerkDependence,
        int administrativeTaskLoad,
        int petitionPressure,
        int petitionBacklog,
        int jurisdictionLeverage,
        int authorityTier = 2,
        string administrativeTaskTier = "district")
    {
        return new JurisdictionAuthoritySnapshot
        {
            SettlementId = new SettlementId(1),
            LeadOfficialPersonId = new PersonId(1),
            LeadOfficialName = "Zhang Yuan",
            LeadOfficeTitle = "County Clerk",
            AuthorityTier = authorityTier,
            JurisdictionLeverage = jurisdictionLeverage,
            ClerkDependence = clerkDependence,
            PetitionPressure = petitionPressure,
            PetitionBacklog = petitionBacklog,
            CurrentAdministrativeTask = "Review petitions",
            AdministrativeTaskTier = administrativeTaskTier,
            AdministrativeTaskLoad = administrativeTaskLoad,
            LastPetitionOutcome = "Triaged",
            PetitionOutcomeCategory = "Triaged",
            LastAdministrativeTrace = "County gate petitions remain in motion.",
        };
    }

    private sealed class StubWorldQueries : IWorldSettlementsQueries
    {
        private readonly IReadOnlyList<SettlementSnapshot> _settlements;

        public StubWorldQueries(IReadOnlyList<SettlementSnapshot> settlements)
        {
            _settlements = settlements;
        }

        public SettlementSnapshot GetRequiredSettlement(SettlementId settlementId)
        {
            return _settlements.Single(settlement => settlement.Id == settlementId);
        }

        public IReadOnlyList<SettlementSnapshot> GetSettlements()
        {
            return _settlements;
        }

        // SPATIAL_SKELETON_SPEC §8 extensions — not exercised by these tests;
        // any test that touches them should override in its own stub.
        public IReadOnlyList<SettlementSnapshot> GetSettlementsByNodeKind(SettlementNodeKind kind) => [];
        public IReadOnlyList<SettlementSnapshot> GetSettlementsByVisibility(NodeVisibility visibility) => [];
        public IReadOnlyList<RouteSnapshot> GetRoutes() => [];
        public IReadOnlyList<RouteSnapshot> GetRoutesByKind(RouteKind kind) => [];
        public IReadOnlyList<RouteSnapshot> GetRoutesByLegitimacy(RouteLegitimacy legitimacy) => [];
        public IReadOnlyList<RouteSnapshot> GetRoutesTouching(SettlementId settlementId) => [];
        public SeasonBandSnapshot GetCurrentSeason() => new();
        public LocusSnapshot? GetCurrentLocus() => null;
        public IReadOnlyList<PublicSurfaceSignal> GetCurrentPulseSignals() => [];
    }

    private sealed class StubPopulationQueries : IPopulationAndHouseholdsQueries
    {
        private readonly IReadOnlyList<PopulationSettlementSnapshot> _settlements;

        public StubPopulationQueries(IReadOnlyList<PopulationSettlementSnapshot> settlements)
        {
            _settlements = settlements;
        }

        public HouseholdPressureSnapshot GetRequiredHousehold(HouseholdId householdId)
        {
            throw new AssertionException("Household lookup should not be used in this test.");
        }

        public PopulationSettlementSnapshot GetRequiredSettlement(SettlementId settlementId)
        {
            return _settlements.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<HouseholdPressureSnapshot> GetHouseholds()
        {
            return [];
        }

        public IReadOnlyList<PopulationSettlementSnapshot> GetSettlements()
        {
            return _settlements;
        }
    }

    private sealed class StubTradeQueries : ITradeAndIndustryQueries
    {
        private readonly IReadOnlyList<ClanTradeSnapshot> _clans;
        private readonly IReadOnlyList<MarketSnapshot> _markets;
        private readonly IReadOnlyList<ClanTradeRouteSnapshot> _routes;

        public StubTradeQueries(
            IReadOnlyList<ClanTradeSnapshot> clans,
            IReadOnlyList<MarketSnapshot> markets,
            IReadOnlyList<ClanTradeRouteSnapshot> routes)
        {
            _clans = clans;
            _markets = markets;
            _routes = routes;
        }

        public ClanTradeSnapshot GetRequiredClanTrade(ClanId clanId)
        {
            return _clans.Single(clan => clan.ClanId == clanId);
        }

        public IReadOnlyList<ClanTradeSnapshot> GetClanTrades()
        {
            return _clans;
        }

        public IReadOnlyList<MarketSnapshot> GetMarkets()
        {
            return _markets;
        }

        public IReadOnlyList<ClanTradeRouteSnapshot> GetRoutesForClan(ClanId clanId)
        {
            return _routes.Where(route => route.ClanId == clanId).ToArray();
        }
    }

    private sealed class StubOrderQueries : IOrderAndBanditryQueries
    {
        private readonly IReadOnlyList<SettlementDisorderSnapshot> _settlements;

        public StubOrderQueries(IReadOnlyList<SettlementDisorderSnapshot> settlements)
        {
            _settlements = settlements;
        }

        public SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId)
        {
            return _settlements.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder()
        {
            return _settlements;
        }
    }

    private sealed class StubOfficeQueries : IOfficeAndCareerQueries
    {
        private readonly IReadOnlyList<OfficeCareerSnapshot> _careers;
        private readonly IReadOnlyList<JurisdictionAuthoritySnapshot> _jurisdictions;

        public StubOfficeQueries(
            IReadOnlyList<OfficeCareerSnapshot> careers,
            IReadOnlyList<JurisdictionAuthoritySnapshot> jurisdictions)
        {
            _careers = careers;
            _jurisdictions = jurisdictions;
        }

        public OfficeCareerSnapshot GetRequiredCareer(PersonId personId)
        {
            return _careers.Single(career => career.PersonId == personId);
        }

        public IReadOnlyList<OfficeCareerSnapshot> GetCareers()
        {
            return _careers;
        }

        public JurisdictionAuthoritySnapshot GetRequiredJurisdiction(SettlementId settlementId)
        {
            return _jurisdictions.Single(jurisdiction => jurisdiction.SettlementId == settlementId);
        }

        public IReadOnlyList<JurisdictionAuthoritySnapshot> GetJurisdictions()
        {
            return _jurisdictions;
        }
    }

    private sealed class StubFamilyQueries : IFamilyCoreQueries
    {
        private readonly IReadOnlyList<ClanSnapshot> _clans;

        public StubFamilyQueries(IReadOnlyList<ClanSnapshot> clans)
        {
            _clans = clans;
        }

        public ClanSnapshot GetRequiredClan(ClanId clanId)
        {
            return _clans.Single(clan => clan.Id == clanId);
        }

        public IReadOnlyList<ClanSnapshot> GetClans()
        {
            return _clans;
        }
    }

    private sealed class StubSocialQueries : ISocialMemoryAndRelationsQueries
    {
        private readonly IReadOnlyList<ClanNarrativeSnapshot> _narratives;

        public StubSocialQueries(IReadOnlyList<ClanNarrativeSnapshot> narratives)
        {
            _narratives = narratives;
        }

        public ClanNarrativeSnapshot GetRequiredClanNarrative(ClanId clanId)
        {
            return _narratives.Single(narrative => narrative.ClanId == clanId);
        }

        public IReadOnlyList<ClanNarrativeSnapshot> GetClanNarratives()
        {
            return _narratives;
        }
    }
}
