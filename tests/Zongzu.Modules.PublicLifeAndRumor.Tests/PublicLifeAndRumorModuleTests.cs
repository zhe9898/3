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
    public void RunMonth_BuildsCountyPulseAndEmitsPublicEvent()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        QueryRegistry queries = CreateSharedQueries();

        FeatureManifest manifest = CreatePublicLifeManifest();
        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            manifest,
            new DeterministicRandom(KernelState.Create(19)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(state, context));

        Assert.That(state.Settlements, Has.Count.EqualTo(1));
        SettlementPublicLifeState settlement = state.Settlements.Single();
        Assert.That(settlement.NodeLabel, Is.EqualTo("兰溪县门"));
        Assert.That(settlement.DominantVenueLabel, Does.Contain("榜下").Or.Contain("镇市").Or.Contain("茶肆"));
        Assert.That(settlement.MonthlyCadenceCode, Is.EqualTo("river-road-bustle"));
        Assert.That(settlement.MonthlyCadenceLabel, Is.EqualTo("春汛行旅"));
        Assert.That(settlement.CrowdMixLabel, Is.Not.Empty);
        Assert.That(settlement.DominantVenueCode, Is.Not.Empty);
        Assert.That(settlement.DocumentaryWeight, Is.InRange(0, 100));
        Assert.That(settlement.VerificationCost, Is.InRange(0, 100));
        Assert.That(settlement.MarketRumorFlow, Is.InRange(0, 100));
        Assert.That(settlement.CourierRisk, Is.InRange(0, 100));
        Assert.That(settlement.OfficialNoticeLine, Is.Not.Empty);
        Assert.That(settlement.StreetTalkLine, Is.Not.Empty);
        Assert.That(settlement.RoadReportLine, Is.Not.Empty);
        Assert.That(settlement.PrefectureDispatchLine, Is.Not.Empty);
        Assert.That(settlement.ContentionSummary, Is.Not.Empty);
        Assert.That(settlement.CadenceSummary, Does.Contain("春汛行旅"));
        Assert.That(settlement.PublicSummary, Does.Contain("街谈"));
        Assert.That(settlement.RouteReportSummary, Does.Contain("清水河埠").Or.Contain("州牒").Or.Contain("路报"));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(1));
        Assert.That(settlement.ChannelSummary, Is.Not.Empty);
        Assert.That(context.DomainEvents.Events, Has.Count.EqualTo(1));
        Assert.That(context.DomainEvents.Events[0].EventType, Is.EqualTo(PublicLifeAndRumorEventNames.PrefectureDispatchPressed));
        Assert.That(context.DomainEvents.Events[0].Summary, Does.Contain("春汛行旅"));
    }

    [Test]
    public void RunMonth_ChangesMonthlyCadenceAcrossSeasons()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState springState = module.CreateInitialState();
        PublicLifeAndRumorState winterState = module.CreateInitialState();
        QueryRegistry queries = CreateSharedQueries();
        FeatureManifest manifest = CreatePublicLifeManifest();

        ModuleExecutionContext springContext = new(
            new GameDate(1200, 2),
            manifest,
            new DeterministicRandom(KernelState.Create(77)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());
        ModuleExecutionContext winterContext = new(
            new GameDate(1200, 11),
            manifest,
            new DeterministicRandom(KernelState.Create(77)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(springState, springContext));
        module.RunMonth(new ModuleExecutionScope<PublicLifeAndRumorState>(winterState, winterContext));

        SettlementPublicLifeState spring = springState.Settlements.Single();
        SettlementPublicLifeState winter = winterState.Settlements.Single();

        Assert.That(spring.MonthlyCadenceCode, Is.EqualTo("spring-fair"));
        Assert.That(winter.MonthlyCadenceCode, Is.EqualTo("year-end-docket"));
        Assert.That(spring.MonthlyCadenceLabel, Is.Not.EqualTo(winter.MonthlyCadenceLabel));
        Assert.That(spring.CadenceSummary, Is.Not.EqualTo(winter.CadenceSummary));
    }

    [Test]
    public void RegisterQueries_ExposesSettlementPublicPulseForReadModels()
    {
        PublicLifeAndRumorModule module = new();
        PublicLifeAndRumorState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementPublicLifeState
        {
            SettlementId = new SettlementId(3),
            SettlementName = "永河北津",
            SettlementTier = SettlementTier.MarketTown,
            DominantVenueCode = "wharf-ferry",
            NodeLabel = "永河北津镇市",
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
            OfficialNoticeLine = "榜下只说县门已将轻重先后讲定。",
            StreetTalkLine = "街口都说北津来船另有话头。",
            RoadReportLine = "由北津来的脚信尚能相互印证。",
            PrefectureDispatchLine = "州牒已有催意，升斗尚留回旋余地。",
            ContentionSummary = "榜文、街谈与路报互相牵扯，人心尚在观望。",
            CadenceSummary = "值春社集日，河埠歇脚棚多见客商、小贩与脚夫。",
            PublicSummary = "河埠与镇市都在喧起。",
            RouteReportSummary = "北津来船略有迟滞。",
            LastPublicTrace = "值春社集日，河埠歇脚棚多见客商、小贩与脚夫。北津来船略有迟滞。",
            ChannelSummary = "榜示分量已稳，市语流势犹盛。",
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        IPublicLifeAndRumorQueries publicLifeQueries = queries.GetRequired<IPublicLifeAndRumorQueries>();

        SettlementPublicLifeSnapshot snapshot = publicLifeQueries.GetRequiredSettlementPublicLife(new SettlementId(3));

        Assert.That(snapshot.NodeLabel, Is.EqualTo("永河北津镇市"));
        Assert.That(snapshot.DominantVenueLabel, Is.EqualTo("河埠歇脚棚"));
        Assert.That(snapshot.MonthlyCadenceLabel, Is.EqualTo("春社集日"));
        Assert.That(snapshot.CadenceSummary, Does.Contain("春社"));
        Assert.That(snapshot.PublicSummary, Does.Contain("喧起"));
        Assert.That(snapshot.OfficialNoticeLine, Does.Contain("县门"));
        Assert.That(snapshot.StreetTalkLine, Does.Contain("北津"));
        Assert.That(snapshot.RoadReportLine, Does.Contain("脚信"));
        Assert.That(snapshot.PrefectureDispatchLine, Does.Contain("州牒"));
        Assert.That(snapshot.ContentionSummary, Does.Contain("观望"));
        Assert.That(publicLifeQueries.GetSettlementPublicLife(), Has.Count.EqualTo(1));
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
                Name = "兰溪",
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
                LastExplanation = "兰溪市面尚旺。",
            },
        ],
        [
            new MarketSnapshot
            {
                SettlementId = new SettlementId(1),
                MarketName = "兰溪镇市",
                PriceIndex = 107,
                Demand = 73,
                LocalRisk = 18,
            },
        ],
        [
            new TradeRouteSnapshot
            {
                RouteId = 1,
                ClanId = new ClanId(1),
                RouteName = "清水河埠",
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
                LastPressureReason = "河埠传闻渐盛。",
            },
        ]));
        queries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [],
        [
            new JurisdictionAuthoritySnapshot
            {
                SettlementId = new SettlementId(1),
                LeadOfficialPersonId = new PersonId(1),
                LeadOfficialName = "张元",
                LeadOfficeTitle = "主簿",
                AuthorityTier = 2,
                JurisdictionLeverage = 59,
                PetitionPressure = 44,
                PetitionBacklog = 28,
                CurrentAdministrativeTask = "勘理词状",
                AdministrativeTaskTier = "district",
                LastPetitionOutcome = "分轻重：先理门前争状。",
                PetitionOutcomeCategory = "Triaged",
                LastAdministrativeTrace = "县门词状与榜示并压。",
            },
        ]));
        queries.Register<IFamilyCoreQueries>(new StubFamilyQueries(
        [
            new ClanSnapshot
            {
                Id = new ClanId(1),
                ClanName = "张氏",
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
                PublicNarrative = "乡里都在议论张氏近来的脸面。",
                GrudgePressure = 24,
                FearPressure = 18,
                ShamePressure = 16,
                FavorBalance = 12,
            },
        ]));
        return queries;
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
        private readonly IReadOnlyList<TradeRouteSnapshot> _routes;

        public StubTradeQueries(
            IReadOnlyList<ClanTradeSnapshot> clans,
            IReadOnlyList<MarketSnapshot> markets,
            IReadOnlyList<TradeRouteSnapshot> routes)
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

        public IReadOnlyList<TradeRouteSnapshot> GetRoutesForClan(ClanId clanId)
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
