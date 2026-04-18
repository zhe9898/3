using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;

namespace Zongzu.Application;

public static class SimulationBootstrapper
{
    private readonly record struct SeedFixture(SettlementId SettlementId, ClanId ClanId, PersonId HeirId);
    private readonly record struct StressSliceTemplate(
        string SettlementName,
        string ClanName,
        string HeirName,
        string AcademyName,
        string MarketName,
        string RouteName,
        string TenantHouseholdName,
        string FreeHouseholdName,
        int Security,
        int Prosperity,
        int ClanPrestige,
        int ClanSupport,
        int CommonerDistress,
        int LaborSupply,
        int MigrationPressure,
        int MilitiaPotential,
        int GrudgePressure,
        int FearPressure,
        int StudyProgress,
        int ScholarStress,
        int ScholarlyReputation,
        int CashReserve,
        int GrainReserve,
        int Debt,
        int CommerceReputation,
        int ShopCount,
        int ManagerSkill,
        int MarketDemand,
        int MarketRisk,
        int RouteCapacity,
        int RouteRisk,
        int BanditThreat,
        int RoutePressure,
        int SuppressionDemand,
        int DisorderPressure,
        int GuardCount,
        int RetainerCount,
        int MilitiaCount,
        int EscortCount,
        int Readiness,
        int CommandCapacity,
        string InitialConflictTrace);

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

    public static IReadOnlyList<IModuleRunner> CreateM3OrderAndBanditryModules()
    {
        return
        [
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OrderAndBanditryModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM3LocalConflictModules()
    {
        return
        [
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateP1GovernanceLocalConflictModules()
    {
        return
        [
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OfficeAndCareerModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateP3CampaignSandboxModules()
    {
        return
        [
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OfficeAndCareerModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new WarfareCampaignModule(),
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

    public static GameSimulation CreateM3OrderAndBanditryBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3OrderAndBanditryModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM3OrderAndBanditryStressBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3OrderAndBanditryModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3StressWorld(simulation, includeConflict: false);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM3LocalConflictBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3LocalConflictModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM3LocalConflictStressBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3LocalConflictModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        SeedM3StressWorld(simulation, includeConflict: true);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateP1GovernanceLocalConflictBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateP1GovernanceLocalConflictModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedP1GovernanceWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateP3CampaignSandboxBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.WarfareCampaign, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateP3CampaignSandboxModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedP1GovernanceWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation LoadM0M1(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM0M1Modules(), migrationPipeline);
    }

    public static GameSimulation LoadM2(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM2Modules(), migrationPipeline);
    }

    public static GameSimulation LoadM3OrderAndBanditry(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM3OrderAndBanditryModules(), migrationPipeline);
    }

    public static GameSimulation LoadM3LocalConflict(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM3LocalConflictModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadP1GovernanceLocalConflict(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateP1GovernanceLocalConflictModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadP3CampaignSandbox(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateP3CampaignSandboxModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
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

    private static void SeedP1GovernanceWorld(GameSimulation simulation, SeedFixture fixture)
    {
        EducationAndExamsState educationState = simulation.GetMutableModuleState<EducationAndExamsState>(KnownModuleKeys.EducationAndExams);
        EducationPersonState? heir = educationState.People.SingleOrDefault(person => person.PersonId == fixture.HeirId);

        if (heir is null)
        {
            return;
        }

        heir.IsStudying = false;
        heir.HasTutor = true;
        heir.TutorQuality = Math.Max(heir.TutorQuality, 14);
        heir.StudyProgress = 28;
        heir.ExamAttempts = Math.Max(1, heir.ExamAttempts);
        heir.HasPassedLocalExam = true;
        heir.LastOutcome = "Passed";
        heir.LastExplanation = "The heir already passed the local exam before governance-lite appointment review began.";
        heir.ScholarlyReputation = Math.Max(heir.ScholarlyReputation, 26);
    }

    private static void SeedM3OrderWorld(GameSimulation simulation, SeedFixture fixture)
    {
        OrderAndBanditryState orderState = simulation.GetMutableModuleState<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = fixture.SettlementId,
            BanditThreat = 28,
            RoutePressure = 24,
            SuppressionDemand = 19,
            DisorderPressure = 21,
            LastPressureReason = "Lanxi starts with modest road-watch pressure around the river wharf.",
        });
    }

    private static void SeedM3ConflictWorld(GameSimulation simulation, SeedFixture fixture)
    {
        ConflictAndForceState conflictState = simulation.GetMutableModuleState<ConflictAndForceState>(KnownModuleKeys.ConflictAndForce);
        SettlementForceState settlement = new()
        {
            SettlementId = fixture.SettlementId,
            GuardCount = 14,
            RetainerCount = 8,
            MilitiaCount = 22,
            EscortCount = 8,
            Readiness = 46,
            CommandCapacity = 34,
            LastConflictTrace = "Lanxi keeps a disciplined hall guard and escort watch around the river wharf.",
        };
        settlement.HasActiveConflict = true;
        ConflictAndForceResponseStateCalculator.Refresh(settlement);
        conflictState.Settlements.Add(settlement);
    }

    private static void SeedM3StressWorld(GameSimulation simulation, bool includeConflict)
    {
        StressSliceTemplate[] slices =
        [
            new StressSliceTemplate(
                "Qingshui",
                "Li",
                "Li Wen",
                "Qingshui Study Hall",
                "Qingshui Grain Market",
                "Qingshui Canal Convoy",
                "Tenant Han",
                "Weaver Sun",
                56,
                59,
                46,
                49,
                31,
                108,
                16,
                44,
                28,
                26,
                42,
                14,
                7,
                72,
                60,
                14,
                27,
                1,
                3,
                54,
                14,
                20,
                18,
                12,
                10,
                14,
                12,
                8,
                3,
                6,
                2,
                24,
                20,
                "Routine escort rotation keeps the canal market watchful."),
            new StressSliceTemplate(
                "Fushan",
                "Wu",
                "Wu Cheng",
                "Fushan Academy",
                "Fushan Copper Market",
                "Fushan Hill Route",
                "Tenant Gao",
                "Potter Xu",
                44,
                55,
                56,
                61,
                67,
                96,
                53,
                82,
                61,
                58,
                51,
                27,
                11,
                84,
                64,
                31,
                33,
                1,
                4,
                69,
                38,
                29,
                64,
                62,
                54,
                49,
                58,
                15,
                7,
                18,
                6,
                37,
                31,
                "Road escorts in Fushan keep encountering armed coercion around the hills."),
            new StressSliceTemplate(
                "Yonghe",
                "Chen",
                "Chen Rui",
                "Yonghe Hall of Classics",
                "Yonghe Salt Exchange",
                "Yonghe North Ferry",
                "Tenant Deng",
                "Boatman Luo",
                39,
                60,
                62,
                67,
                72,
                101,
                58,
                88,
                66,
                63,
                58,
                31,
                13,
                92,
                69,
                35,
                36,
                2,
                4,
                74,
                46,
                34,
                71,
                68,
                59,
                57,
                63,
                18,
                9,
                24,
                8,
                45,
                38,
                "A bruising dockside clash in Yonghe forced militia and escorts into the same streets."),
        ];

        foreach (StressSliceTemplate slice in slices)
        {
            AppendStressSlice(simulation, slice, includeConflict);
        }
    }

    private static void AppendStressSlice(GameSimulation simulation, StressSliceTemplate slice, bool includeConflict)
    {
        WorldSettlementsState worldState = simulation.GetMutableModuleState<WorldSettlementsState>(KnownModuleKeys.WorldSettlements);
        PopulationAndHouseholdsState populationState = simulation.GetMutableModuleState<PopulationAndHouseholdsState>(KnownModuleKeys.PopulationAndHouseholds);
        FamilyCoreState familyState = simulation.GetMutableModuleState<FamilyCoreState>(KnownModuleKeys.FamilyCore);
        SocialMemoryAndRelationsState socialState = simulation.GetMutableModuleState<SocialMemoryAndRelationsState>(KnownModuleKeys.SocialMemoryAndRelations);
        EducationAndExamsState educationState = simulation.GetMutableModuleState<EducationAndExamsState>(KnownModuleKeys.EducationAndExams);
        TradeAndIndustryState tradeState = simulation.GetMutableModuleState<TradeAndIndustryState>(KnownModuleKeys.TradeAndIndustry);
        OrderAndBanditryState orderState = simulation.GetMutableModuleState<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        ConflictAndForceState? conflictState = includeConflict
            ? simulation.GetMutableModuleState<ConflictAndForceState>(KnownModuleKeys.ConflictAndForce)
            : null;

        SettlementId settlementId = KernelIdAllocator.NextSettlement(simulation.KernelState);
        ClanId clanId = KernelIdAllocator.NextClan(simulation.KernelState);
        PersonId heirId = KernelIdAllocator.NextPerson(simulation.KernelState);
        HouseholdId tenantHouseholdId = KernelIdAllocator.NextHousehold(simulation.KernelState);
        HouseholdId freeHouseholdId = KernelIdAllocator.NextHousehold(simulation.KernelState);
        InstitutionId academyId = KernelIdAllocator.NextInstitution(simulation.KernelState);
        int routeId = tradeState.Routes.Count + 1;

        worldState.Settlements.Add(new SettlementStateData
        {
            Id = settlementId,
            Name = slice.SettlementName,
            Security = slice.Security,
            Prosperity = slice.Prosperity,
            BaselineInstitutionCount = 1,
        });

        familyState.Clans.Add(new ClanStateData
        {
            Id = clanId,
            ClanName = slice.ClanName,
            HomeSettlementId = settlementId,
            Prestige = slice.ClanPrestige,
            SupportReserve = slice.ClanSupport,
            HeirPersonId = heirId,
        });

        familyState.People.Add(new FamilyPersonState
        {
            Id = heirId,
            ClanId = clanId,
            GivenName = slice.HeirName,
            AgeMonths = 30 * 12,
            IsAlive = true,
        });

        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = tenantHouseholdId,
            HouseholdName = slice.TenantHouseholdName,
            SettlementId = settlementId,
            SponsorClanId = clanId,
            Distress = Math.Clamp(slice.CommonerDistress - 5, 0, 100),
            DebtPressure = Math.Clamp(slice.CommonerDistress - 9, 0, 100),
            LaborCapacity = Math.Clamp(slice.LaborSupply / 2, 0, 100),
            MigrationRisk = Math.Clamp(slice.MigrationPressure - 6, 0, 100),
            IsMigrating = false,
        });

        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = freeHouseholdId,
            HouseholdName = slice.FreeHouseholdName,
            SettlementId = settlementId,
            SponsorClanId = null,
            Distress = Math.Clamp(slice.CommonerDistress - 1, 0, 100),
            DebtPressure = Math.Clamp(slice.CommonerDistress - 4, 0, 100),
            LaborCapacity = Math.Clamp((slice.LaborSupply / 2) - 4, 0, 100),
            MigrationRisk = slice.MigrationPressure,
            IsMigrating = false,
        });

        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = settlementId,
            CommonerDistress = slice.CommonerDistress,
            LaborSupply = slice.LaborSupply,
            MigrationPressure = slice.MigrationPressure,
            MilitiaPotential = slice.MilitiaPotential,
        });

        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = clanId,
            PublicNarrative = $"{slice.SettlementName} balances study, trade, and rising coercion.",
            GrudgePressure = slice.GrudgePressure,
            FearPressure = slice.FearPressure,
            ShamePressure = Math.Clamp(slice.GrudgePressure / 2, 0, 100),
            FavorBalance = Math.Clamp(slice.ClanSupport / 10, 0, 100),
        });

        educationState.Academies.Add(new AcademyState
        {
            Id = academyId,
            SettlementId = settlementId,
            AcademyName = slice.AcademyName,
            IsOpen = true,
            Capacity = 4,
            Prestige = Math.Clamp(slice.ClanPrestige - 8, 0, 100),
        });

        educationState.People.Add(new EducationPersonState
        {
            PersonId = heirId,
            ClanId = clanId,
            AcademyId = academyId,
            DisplayName = slice.HeirName,
            IsStudying = true,
            HasTutor = slice.ClanSupport >= 60,
            TutorQuality = slice.ClanSupport >= 60 ? 2 : 1,
            StudyProgress = slice.StudyProgress,
            Stress = slice.ScholarStress,
            ExamAttempts = 0,
            HasPassedLocalExam = false,
            LastOutcome = "Preparing",
            LastExplanation = $"{slice.ClanName} is financing study while managing unrest around {slice.SettlementName}.",
            ScholarlyReputation = slice.ScholarlyReputation,
        });

        tradeState.Clans.Add(new ClanTradeState
        {
            ClanId = clanId,
            PrimarySettlementId = settlementId,
            CashReserve = slice.CashReserve,
            GrainReserve = slice.GrainReserve,
            Debt = slice.Debt,
            CommerceReputation = slice.CommerceReputation,
            ShopCount = slice.ShopCount,
            ManagerSkill = slice.ManagerSkill,
            LastOutcome = "Stable",
            LastExplanation = $"{slice.SettlementName} trade is balancing convoy risk against local pressure.",
        });

        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = settlementId,
            MarketName = slice.MarketName,
            PriceIndex = 100 + (slice.RouteRisk / 10),
            Demand = slice.MarketDemand,
            LocalRisk = slice.MarketRisk,
        });

        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = routeId,
            ClanId = clanId,
            RouteName = slice.RouteName,
            SettlementId = settlementId,
            IsActive = true,
            Capacity = slice.RouteCapacity,
            Risk = slice.RouteRisk,
            LastMargin = 0,
        });

        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = settlementId,
            BanditThreat = slice.BanditThreat,
            RoutePressure = slice.RoutePressure,
            SuppressionDemand = slice.SuppressionDemand,
            DisorderPressure = slice.DisorderPressure,
            LastPressureReason = $"{slice.SettlementName} starts with a guarded but fragile order balance.",
        });

        if (conflictState is null)
        {
            return;
        }

        SettlementForceState forceState = new()
        {
            SettlementId = settlementId,
            GuardCount = slice.GuardCount,
            RetainerCount = slice.RetainerCount,
            MilitiaCount = slice.MilitiaCount,
            EscortCount = slice.EscortCount,
            Readiness = slice.Readiness,
            CommandCapacity = slice.CommandCapacity,
            LastConflictTrace = slice.InitialConflictTrace,
            HasActiveConflict = ConflictAndForceResponseStateCalculator.InferLegacyHasActiveConflict(slice.InitialConflictTrace),
        };
        ConflictAndForceResponseStateCalculator.Refresh(forceState);
        conflictState.Settlements.Add(forceState);
    }

    private static SaveMigrationPipeline CreateDefaultMigrationPipeline()
    {
        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterModuleMigration(KnownModuleKeys.OfficeAndCareer, 1, 2, MigrateOfficeAndCareerStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.ConflictAndForce, 1, 2, MigrateConflictAndForceStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.ConflictAndForce, 2, 3, MigrateConflictAndForceStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 1, 2, MigrateWarfareCampaignStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 2, 3, MigrateWarfareCampaignStateV2ToV3);
        return pipeline;
    }

    private static ModuleStateEnvelope MigrateOfficeAndCareerStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState migratedState = (OfficeAndCareerState)serializer.Deserialize(typeof(OfficeAndCareerState), envelope.Payload);
        OfficeAndCareerStateProjection.UpgradeFromSchemaV1(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(OfficeAndCareerState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateConflictAndForceStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState migratedState = (ConflictAndForceState)serializer.Deserialize(typeof(ConflictAndForceState), envelope.Payload);

        foreach (SettlementForceState migratedSettlement in migratedState.Settlements)
        {
            migratedSettlement.HasActiveConflict = ConflictAndForceResponseStateCalculator.InferLegacyHasActiveConflict(migratedSettlement.LastConflictTrace);
            ConflictAndForceResponseStateCalculator.Refresh(migratedSettlement);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(ConflictAndForceState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateConflictAndForceStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState migratedState = (ConflictAndForceState)serializer.Deserialize(typeof(ConflictAndForceState), envelope.Payload);

        foreach (SettlementForceState migratedSettlement in migratedState.Settlements)
        {
            migratedSettlement.CampaignFatigue = Math.Clamp(migratedSettlement.CampaignFatigue, 0, 100);
            migratedSettlement.CampaignEscortStrain = Math.Clamp(migratedSettlement.CampaignEscortStrain, 0, 100);
            migratedSettlement.LastCampaignFalloutTrace ??= string.Empty;
            ConflictAndForceResponseStateCalculator.Refresh(migratedSettlement);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(ConflictAndForceState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateWarfareCampaignStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState migratedState = (WarfareCampaignState)serializer.Deserialize(typeof(WarfareCampaignState), envelope.Payload);
        WarfareCampaignStateProjection.UpgradeFromSchemaV1(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(WarfareCampaignState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateWarfareCampaignStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState migratedState = (WarfareCampaignState)serializer.Deserialize(typeof(WarfareCampaignState), envelope.Payload);
        WarfareCampaignStateProjection.UpgradeFromSchemaV2(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(WarfareCampaignState), migratedState),
        };
    }
}
