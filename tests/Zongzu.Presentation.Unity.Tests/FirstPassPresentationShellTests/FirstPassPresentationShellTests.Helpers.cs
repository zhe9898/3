using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

public sealed partial class FirstPassPresentationShellTests
{
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
            PersonDossiers =
            [
                new PersonDossierSnapshot
                {
                    PersonId = new PersonId(1),
                    DisplayName = "Zhang Yuan",
                    LifeStage = LifeStage.Adult,
                    Gender = PersonGender.Male,
                    IsAlive = true,
                    FidelityRing = FidelityRing.Core,
                    ClanId = new ClanId(1),
                    ClanName = "娓呮渤寮犳皬",
                    BranchPositionLabel = "Main-line heir",
                    KinshipSummary = "spouse Li; children 1",
                    TemperamentSummary = "ambition 64, prudence 53, loyalty 71, sociability 47",
                    MemoryPressureSummary = "pressure 38; fear 22, obligation 16",
                    CurrentStatusSummary = "Living Adult; Core ring; clan Qinghe Zhang; Main-line heir; pressure 38.",
                    SourceModuleKeys =
                    [
                        KnownModuleKeys.PersonRegistry,
                        KnownModuleKeys.FamilyCore,
                        KnownModuleKeys.SocialMemoryAndRelations,
                    ],
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
