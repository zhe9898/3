using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Application;

public static class SimulationBootstrapper
{
    private readonly record struct SeedFixture(SettlementId SettlementId, ClanId ClanId, PersonId HeirId);

    public static IReadOnlyList<IModuleRunner> CreateM0M1Modules()
    {
        return
        [
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM2Modules()
    {
        return
        [
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static GameSimulation CreateM0M1Bootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM0M1Modules());

        SeedMinimalWorld(simulation);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM2Bootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM2Modules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation LoadM0M1(SaveRoot saveRoot)
    {
        return GameSimulation.Load(saveRoot, CreateM0M1Modules());
    }

    public static GameSimulation LoadM2(SaveRoot saveRoot)
    {
        return GameSimulation.Load(saveRoot, CreateM2Modules());
    }

    private static FeatureManifest CreateM0M1Manifest()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        return manifest;
    }

    private static SeedFixture SeedMinimalWorld(GameSimulation simulation)
    {
        WorldSettlementsState worldState = simulation.GetMutableModuleState<WorldSettlementsState>(KnownModuleKeys.WorldSettlements);
        PopulationAndHouseholdsState populationState = simulation.GetMutableModuleState<PopulationAndHouseholdsState>(KnownModuleKeys.PopulationAndHouseholds);
        FamilyCoreState familyState = simulation.GetMutableModuleState<FamilyCoreState>(KnownModuleKeys.FamilyCore);

        SettlementId settlementId = KernelIdAllocator.NextSettlement(simulation.KernelState);
        ClanId clanId = KernelIdAllocator.NextClan(simulation.KernelState);
        PersonId heirId = KernelIdAllocator.NextPerson(simulation.KernelState);
        HouseholdId tenantHouseholdId = KernelIdAllocator.NextHousehold(simulation.KernelState);
        HouseholdId freeHouseholdId = KernelIdAllocator.NextHousehold(simulation.KernelState);

        worldState.Settlements.Add(new SettlementStateData
        {
            Id = settlementId,
            Name = "Lanxi",
            Security = 57,
            Prosperity = 61,
            BaselineInstitutionCount = 1,
        });

        familyState.Clans.Add(new ClanStateData
        {
            Id = clanId,
            ClanName = "Zhang",
            HomeSettlementId = settlementId,
            Prestige = 52,
            SupportReserve = 60,
            HeirPersonId = heirId,
        });

        familyState.People.Add(new FamilyPersonState
        {
            Id = heirId,
            ClanId = clanId,
            GivenName = "Zhang Yuan",
            AgeMonths = 32 * 12,
            IsAlive = true,
        });

        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = tenantHouseholdId,
            HouseholdName = "Tenant Li",
            SettlementId = settlementId,
            SponsorClanId = clanId,
            Distress = 42,
            DebtPressure = 36,
            LaborCapacity = 58,
            MigrationRisk = 18,
            IsMigrating = false,
        });

        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = freeHouseholdId,
            HouseholdName = "Potter Wu",
            SettlementId = settlementId,
            SponsorClanId = null,
            Distress = 47,
            DebtPressure = 44,
            LaborCapacity = 52,
            MigrationRisk = 24,
            IsMigrating = false,
        });

        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = settlementId,
            CommonerDistress = 44,
            LaborSupply = 110,
            MigrationPressure = 21,
            MilitiaPotential = 69,
        });

        return new SeedFixture(settlementId, clanId, heirId);
    }

    private static void SeedM2LiteWorld(GameSimulation simulation, SeedFixture fixture)
    {
        EducationAndExamsState educationState = simulation.GetMutableModuleState<EducationAndExamsState>(KnownModuleKeys.EducationAndExams);
        TradeAndIndustryState tradeState = simulation.GetMutableModuleState<TradeAndIndustryState>(KnownModuleKeys.TradeAndIndustry);

        InstitutionId academyId = KernelIdAllocator.NextInstitution(simulation.KernelState);

        educationState.Academies.Add(new AcademyState
        {
            Id = academyId,
            SettlementId = fixture.SettlementId,
            AcademyName = "Lanxi Academy",
            IsOpen = true,
            Capacity = 3,
            Prestige = 42,
        });

        educationState.People.Add(new EducationPersonState
        {
            PersonId = fixture.HeirId,
            ClanId = fixture.ClanId,
            AcademyId = academyId,
            DisplayName = "Zhang Yuan",
            IsStudying = true,
            HasTutor = false,
            TutorQuality = 0,
            StudyProgress = 54,
            Stress = 16,
            ExamAttempts = 0,
            HasPassedLocalExam = false,
            LastOutcome = "Preparing",
            LastExplanation = "Clan support is funding local study at Lanxi Academy.",
            ScholarlyReputation = 9,
        });

        tradeState.Clans.Add(new ClanTradeState
        {
            ClanId = fixture.ClanId,
            PrimarySettlementId = fixture.SettlementId,
            CashReserve = 78,
            GrainReserve = 66,
            Debt = 22,
            CommerceReputation = 27,
            ShopCount = 1,
            ManagerSkill = 3,
            LastOutcome = "Stable",
            LastExplanation = "The clan maintains a small grain and ceramics trade in Lanxi.",
        });

        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = fixture.SettlementId,
            MarketName = "Lanxi Morning Market",
            PriceIndex = 104,
            Demand = 63,
            LocalRisk = 16,
        });

        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = fixture.ClanId,
            RouteName = "Lanxi River Wharf",
            SettlementId = fixture.SettlementId,
            IsActive = true,
            Capacity = 26,
            Risk = 18,
            LastMargin = 0,
        });
    }
}
