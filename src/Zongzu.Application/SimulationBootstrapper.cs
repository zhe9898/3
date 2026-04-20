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
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.PublicLifeAndRumor;
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
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateMvpModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM2Modules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM3OrderAndBanditryModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OrderAndBanditryModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM3LocalConflictModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateP1GovernanceLocalConflictModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OfficeAndCareerModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateP3CampaignSandboxModules()
    {
        return
        [
            new PersonRegistryModule(),
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
            new PublicLifeAndRumorModule(),
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

    public static GameSimulation CreateMvpBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateMvpModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM2Bootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
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
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
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
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
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
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
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
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
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
        return GameSimulation.Load(saveRoot, CreateM0M1Modules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadMvp(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateMvpModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadM2(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM2Modules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadM3OrderAndBanditry(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM3OrderAndBanditryModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
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
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
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
        PersonRegistryState personRegistryState = simulation.GetMutableModuleState<PersonRegistryState>(KnownModuleKeys.PersonRegistry);

        SettlementId settlementId = KernelIdAllocator.NextSettlement(simulation.KernelState);
        ClanId clanId = KernelIdAllocator.NextClan(simulation.KernelState);
        PersonId heirId = KernelIdAllocator.NextPerson(simulation.KernelState);
        HouseholdId tenantHouseholdId = KernelIdAllocator.NextHousehold(simulation.KernelState);
        HouseholdId freeHouseholdId = KernelIdAllocator.NextHousehold(simulation.KernelState);

        worldState.Settlements.Add(new SettlementStateData
        {
            Id = settlementId,
            Name = "兰溪",
            Tier = SettlementTier.CountySeat,
            Security = 57,
            Prosperity = 61,
            BaselineInstitutionCount = 1,
        });

        familyState.Clans.Add(new ClanStateData
        {
            Id = clanId,
            ClanName = "张",
            HomeSettlementId = settlementId,
            Prestige = 52,
            SupportReserve = 60,
            HeirPersonId = heirId,
        });

        familyState.People.Add(new FamilyPersonState
        {
            Id = heirId,
            ClanId = clanId,
            GivenName = "张远",
            AgeMonths = 32 * 12,
            IsAlive = true,
        });

        // Phase 1b: register the clan heir in the Kernel-layer PersonRegistry
        // as the canonical identity anchor. FamilyCore's FamilyPersonState is
        // kept in sync for now (Phase 2 will retire the redundant age/alive
        // fields). See PERSON_OWNERSHIP_RULES.md.
        personRegistryState.Persons.Add(new PersonRecord
        {
            Id = heirId,
            DisplayName = "张远",
            BirthDate = new GameDate(simulation.CurrentDate.Year - 32, simulation.CurrentDate.Month),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Core,
        });

        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = tenantHouseholdId,
            HouseholdName = "佃户李家",
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
            HouseholdName = "陶户吴家",
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
            AcademyName = "兰溪书院",
            IsOpen = true,
            Capacity = 3,
            Prestige = 42,
        });

        educationState.People.Add(new EducationPersonState
        {
            PersonId = fixture.HeirId,
            ClanId = fixture.ClanId,
            AcademyId = academyId,
            DisplayName = "张远",
            IsStudying = true,
            HasTutor = false,
            TutorQuality = 0,
            StudyProgress = 54,
            Stress = 16,
            ExamAttempts = 0,
            HasPassedLocalExam = false,
            LastOutcome = "备试",
            LastExplanation = "宗房仍在出资供张远入兰溪书院。",
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
            LastOutcome = "持平",
            LastExplanation = "宗房在兰溪河埠守着小本行货。",
        });

        tradeState.Markets.Add(new SettlementMarketState
        {
            SettlementId = fixture.SettlementId,
            MarketName = "兰溪早市",
            PriceIndex = 104,
            Demand = 63,
            LocalRisk = 16,
        });

        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = fixture.ClanId,
            RouteName = "兰溪河埠",
            SettlementId = fixture.SettlementId,
            IsActive = true,
            Capacity = 26,
            Risk = 18,
            LastMargin = 0,
            BlockedShipmentCount = 0,
            SeizureRisk = 10,
            RouteConstraintLabel = "尚可通行",
            LastRouteTrace = "兰溪河埠眼下尚可通行，未见明显阻滞。",
        });
        tradeState.BlackRouteLedgers.Add(new SettlementBlackRouteLedgerState
        {
            SettlementId = fixture.SettlementId,
            ShadowPriceIndex = 99,
            DiversionShare = 6,
            IllicitMargin = 1,
            BlockedShipmentCount = 0,
            SeizureRisk = 12,
            DiversionBandLabel = "零星夹带",
            LastLedgerTrace = "兰溪早市偶有零星夹带，尚未压过正市。",
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
        heir.LastOutcome = "得解";
        heir.LastExplanation = "张远场屋得捷，已可依门路守选候阙。";
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
            LastPressureReason = "兰溪河埠虽有巡丁照看，市面与水路仍多暗涌。",
            BlackRoutePressure = 18,
            CoercionRisk = 12,
            SuppressionRelief = 0,
            ResponseActivationLevel = 0,
            PaperCompliance = 0,
            ImplementationDrag = 0,
            RouteShielding = 0,
            RetaliationRisk = 0,
            AdministrativeSuppressionWindow = 0,
            EscalationBandLabel = "尚未成势",
            LastPressureTrace = "兰溪河埠私路尚浅，只在边角试探。",
            LastInterventionCommandCode = string.Empty,
            LastInterventionCommandLabel = string.Empty,
            LastInterventionSummary = string.Empty,
            LastInterventionOutcome = string.Empty,
            InterventionCarryoverMonths = 0,
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
            LastConflictTrace = "兰溪宗房护院与护运丁役守着河埠，昼夜轮看。",
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
                "清水",
                "李",
                "李文",
                "清水书塾",
                "清水粮市",
                "清水漕埠",
                "佃户韩家",
                "织户孙家",
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
                "清水埠头护运按班轮值，漕埠上下仍算警醒。"),
            new StressSliceTemplate(
                "浮山",
                "吴",
                "吴成",
                "浮山书院",
                "浮山铜市",
                "浮山山路",
                "佃户高家",
                "窑户徐家",
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
                "浮山山路护送丁役屡逢挟胁，往来商脚多有惊惧。"),
            new StressSliceTemplate(
                "永和",
                "陈",
                "陈睿",
                "永和经馆",
                "永和盐肆",
                "永河北津",
                "佃户邓家",
                "舟户罗家",
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
                "永河北津码头一度群斗，乡勇与护运同巷并守。"),
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
            Tier = InferStressSettlementTier(slice.SettlementName),
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
            PublicNarrative = $"{slice.SettlementName}一地书塾、行旅与乡里催迫并起。",
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
            LastOutcome = "备试",
            LastExplanation = $"{slice.ClanName}家一面供读，一面应付{slice.SettlementName}乡里的催迫。",
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
            LastOutcome = "持平",
            LastExplanation = $"{slice.SettlementName}市易往来夹在护运风险与街巷催迫之间。",
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
            BlockedShipmentCount = Math.Clamp(slice.RoutePressure / 28, 0, 6),
            SeizureRisk = Math.Clamp((slice.RouteRisk / 3) + (slice.MarketRisk / 4), 0, 100),
            RouteConstraintLabel = DetermineMigratedRouteConstraintLabel(
                Math.Clamp(slice.RoutePressure / 28, 0, 6),
                Math.Clamp((slice.RouteRisk / 3) + (slice.MarketRisk / 4), 0, 100),
                slice.RouteRisk),
            LastRouteTrace = $"{slice.RouteName}眼下已有零星盘查，行路不如旧时顺手。",
        });
        tradeState.BlackRouteLedgers.Add(new SettlementBlackRouteLedgerState
        {
            SettlementId = settlementId,
            ShadowPriceIndex = 100 + (slice.RouteRisk / 12),
            DiversionShare = Math.Clamp((slice.RoutePressure / 2) + (slice.MarketRisk / 3), 0, 100),
            IllicitMargin = Math.Clamp((slice.RouteRisk / 10) + (slice.MarketRisk / 12), -10, 30),
            BlockedShipmentCount = Math.Clamp(slice.RoutePressure / 18, 0, 12),
            SeizureRisk = Math.Clamp((slice.RoutePressure / 2) + (slice.MarketRisk / 2), 0, 100),
            DiversionBandLabel = DetermineMigratedDiversionBandLabel(
                Math.Clamp((slice.RoutePressure / 2) + (slice.MarketRisk / 3), 0, 100),
                Math.Clamp((slice.RoutePressure / 2) + (slice.MarketRisk / 2), 0, 100),
                Math.Clamp((slice.RouteRisk / 10) + (slice.MarketRisk / 12), -10, 30)),
            LastLedgerTrace = $"{slice.SettlementName}河埠与市集之间已有私货夹带。",
        });

        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = settlementId,
            BanditThreat = slice.BanditThreat,
            RoutePressure = slice.RoutePressure,
            SuppressionDemand = slice.SuppressionDemand,
            DisorderPressure = slice.DisorderPressure,
            LastPressureReason = $"{slice.SettlementName}现有巡防支应，然街市与道路之间仍觉绷紧。",
            BlackRoutePressure = Math.Clamp((slice.RoutePressure + slice.MarketRisk) / 2, 0, 100),
            CoercionRisk = Math.Clamp((slice.CommonerDistress + slice.GrudgePressure) / 2, 0, 100),
            SuppressionRelief = 0,
            ResponseActivationLevel = 0,
            PaperCompliance = 0,
            ImplementationDrag = 0,
            RouteShielding = 0,
            RetaliationRisk = 0,
            AdministrativeSuppressionWindow = 0,
            EscalationBandLabel = DetermineMigratedEscalationBandLabel(
                Math.Clamp((slice.RoutePressure + slice.MarketRisk) / 2, 0, 100),
                Math.Clamp((slice.CommonerDistress + slice.GrudgePressure) / 2, 0, 100)),
            LastPressureTrace = $"{slice.SettlementName}私路与暗手正在街市边角试探。",
            LastInterventionCommandCode = string.Empty,
            LastInterventionCommandLabel = string.Empty,
            LastInterventionSummary = string.Empty,
            LastInterventionOutcome = string.Empty,
            InterventionCarryoverMonths = 0,
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

    private static SettlementTier InferStressSettlementTier(string settlementName)
    {
        return settlementName switch
        {
            "姘稿拰" => SettlementTier.PrefectureSeat,
            "娓呮按" or "娴北" => SettlementTier.MarketTown,
            _ => SettlementTier.CountySeat,
        };
    }

    private static string DetermineMigratedEscalationBandLabel(int blackRoutePressure, int coercionRisk)
    {
        int combined = blackRoutePressure + coercionRisk;
        return combined switch
        {
            >= 130 => "私路成势",
            >= 100 => "暗运成线",
            >= 70 => "夹带渐多",
            >= 40 => "私贩试探",
            _ => "尚未成势",
        };
    }

    private static string DetermineMigratedDiversionBandLabel(int diversionShare, int seizureRisk, int illicitMargin)
    {
        int combined = diversionShare + seizureRisk + Math.Max(0, illicitMargin);
        return combined switch
        {
            >= 120 => "私货成路",
            >= 85 => "正私并行",
            >= 55 => "夹带渐增",
            >= 25 => "零星夹带",
            _ => "尚未分流",
        };
    }

    private static string DetermineMigratedRouteConstraintLabel(int blockedShipmentCount, int seizureRisk, int routeRisk)
    {
        int combined = (blockedShipmentCount * 12) + seizureRisk + (routeRisk / 2);
        return combined switch
        {
            >= 120 => "盘查封路",
            >= 85 => "卡口渐密",
            >= 50 => "时有阻滞",
            >= 20 => "尚可通行",
            _ => "行路平稳",
        };
    }

    private static SaveMigrationPipeline CreateDefaultMigrationPipeline()
    {
        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterModuleMigration(KnownModuleKeys.WorldSettlements, 1, 2, MigrateWorldSettlementsStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WorldSettlements, 2, 3, MigrateWorldSettlementsStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 1, 2, MigrateFamilyCoreStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 2, 3, MigrateFamilyCoreStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.PublicLifeAndRumor, 1, 2, MigratePublicLifeAndRumorStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.PublicLifeAndRumor, 2, 3, MigratePublicLifeAndRumorStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.PublicLifeAndRumor, 3, 4, MigratePublicLifeAndRumorStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OfficeAndCareer, 1, 2, MigrateOfficeAndCareerStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OfficeAndCareer, 2, 3, MigrateOfficeAndCareerStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 1, 2, MigrateTradeAndIndustryStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 2, 3, MigrateTradeAndIndustryStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 1, 2, MigrateOrderAndBanditryStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 2, 3, MigrateOrderAndBanditryStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 3, 4, MigrateOrderAndBanditryStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 4, 5, MigrateOrderAndBanditryStateV4ToV5);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 5, 6, MigrateOrderAndBanditryStateV5ToV6);
        pipeline.RegisterModuleMigration(KnownModuleKeys.ConflictAndForce, 1, 2, MigrateConflictAndForceStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.ConflictAndForce, 2, 3, MigrateConflictAndForceStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 1, 2, MigrateWarfareCampaignStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 2, 3, MigrateWarfareCampaignStateV2ToV3);
        return pipeline;
    }

    private static ModuleStateEnvelope MigrateWorldSettlementsStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(typeof(WorldSettlementsState), envelope.Payload);

        foreach (SettlementStateData settlement in migratedState.Settlements)
        {
            if (settlement.Tier == SettlementTier.Unknown)
            {
                settlement.Tier = SettlementTier.CountySeat;
            }
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), migratedState),
        };
    }

    /// <summary>
    /// SPATIAL_SKELETON_SPEC §13 — Phase 1c v2→v3 upgrade:
    /// <list type="bullet">
    ///   <item>Seed <see cref="SettlementStateData.NodeKind"/> by inference from
    ///         <see cref="SettlementStateData.Tier"/> (SPEC §13.2 table).</item>
    ///   <item>Seed <see cref="SettlementStateData.Visibility"/> to
    ///         <see cref="NodeVisibility.StateVisible"/> (safe default: all
    ///         pre-1c nodes were officially registered).</item>
    ///   <item>Seed <see cref="SettlementStateData.EcoZone"/> to
    ///         <see cref="SettlementEcoZone.JiangnanWaterNetwork"/> (Lanxi
    ///         seed — the only live world in Phase 1c).</item>
    ///   <item>Leave <see cref="WorldSettlementsState.Routes"/> empty (SPEC
    ///         §13.3: old saves carry no routes; seed bootstrap rebuilds them).</item>
    ///   <item>Leave <see cref="WorldSettlementsState.CurrentSeason"/> at
    ///         constructor defaults (neutral Slack / Limited / Quiet).</item>
    /// </list>
    /// </summary>
    private static ModuleStateEnvelope MigrateWorldSettlementsStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(typeof(WorldSettlementsState), envelope.Payload);

        foreach (SettlementStateData settlement in migratedState.Settlements)
        {
            if (settlement.NodeKind == SettlementNodeKind.Unknown)
            {
                settlement.NodeKind = settlement.Tier switch
                {
                    SettlementTier.PrefectureSeat => SettlementNodeKind.PrefectureSeat,
                    SettlementTier.CountySeat => SettlementNodeKind.CountySeat,
                    SettlementTier.MarketTown => SettlementNodeKind.MarketTown,
                    _ => SettlementNodeKind.Village,
                };
            }

            if (settlement.Visibility == NodeVisibility.Unknown)
            {
                settlement.Visibility = NodeVisibility.StateVisible;
            }

            if (settlement.EcoZone == SettlementEcoZone.Unknown)
            {
                settlement.EcoZone = SettlementEcoZone.JiangnanWaterNetwork;
            }

            // NeighborIds and ParentAdministrativeId default to empty/null;
            // seed bootstrap (SPEC §12) rebuilds them when the Lanxi world
            // is re-seeded. Phase 1c does not force pre-existing saves to
            // adopt the seed graph.
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateFamilyCoreStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV1(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateFamilyCoreStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV2ToV3(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigratePublicLifeAndRumorStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        PublicLifeAndRumorState migratedState = (PublicLifeAndRumorState)serializer.Deserialize(typeof(PublicLifeAndRumorState), envelope.Payload);

        foreach (SettlementPublicLifeState settlement in migratedState.Settlements)
        {
            settlement.MonthlyCadenceCode ??= "legacy-cadence";
            settlement.MonthlyCadenceLabel ??= "旧档续脉";
            settlement.CrowdMixLabel ??= string.Empty;
            settlement.CadenceSummary ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.PublicLifeAndRumor,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(PublicLifeAndRumorState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigratePublicLifeAndRumorStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        PublicLifeAndRumorState migratedState = (PublicLifeAndRumorState)serializer.Deserialize(typeof(PublicLifeAndRumorState), envelope.Payload);

        foreach (SettlementPublicLifeState settlement in migratedState.Settlements)
        {
            settlement.DominantVenueCode ??= string.Empty;
            settlement.DocumentaryWeight = Math.Clamp(settlement.DocumentaryWeight, 0, 100);
            settlement.VerificationCost = Math.Clamp(settlement.VerificationCost, 0, 100);
            settlement.MarketRumorFlow = Math.Clamp(settlement.MarketRumorFlow, 0, 100);
            settlement.CourierRisk = Math.Clamp(settlement.CourierRisk, 0, 100);
            settlement.ChannelSummary ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.PublicLifeAndRumor,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(PublicLifeAndRumorState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigratePublicLifeAndRumorStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        PublicLifeAndRumorState migratedState = (PublicLifeAndRumorState)serializer.Deserialize(typeof(PublicLifeAndRumorState), envelope.Payload);

        foreach (SettlementPublicLifeState settlement in migratedState.Settlements)
        {
            settlement.OfficialNoticeLine ??= string.Empty;
            settlement.StreetTalkLine ??= string.Empty;
            settlement.RoadReportLine ??= string.Empty;
            settlement.PrefectureDispatchLine ??= string.Empty;
            settlement.ContentionSummary ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.PublicLifeAndRumor,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(PublicLifeAndRumorState), migratedState),
        };
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

    private static ModuleStateEnvelope MigrateOfficeAndCareerStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState migratedState = (OfficeAndCareerState)serializer.Deserialize(typeof(OfficeAndCareerState), envelope.Payload);
        OfficeAndCareerStateProjection.UpgradeFromSchemaV2ToV3(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(OfficeAndCareerState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateTradeAndIndustryStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        TradeAndIndustryState migratedState = (TradeAndIndustryState)serializer.Deserialize(typeof(TradeAndIndustryState), envelope.Payload);

        migratedState.Clans ??= [];
        migratedState.Markets ??= [];
        migratedState.Routes ??= [];
        migratedState.BlackRouteLedgers ??= [];

        foreach (SettlementMarketState market in migratedState.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            SettlementBlackRouteLedgerState ledger = migratedState.BlackRouteLedgers.SingleOrDefault(existing => existing.SettlementId == market.SettlementId)
                ?? new SettlementBlackRouteLedgerState
                {
                    SettlementId = market.SettlementId,
                    ShadowPriceIndex = 100,
                };

            if (!migratedState.BlackRouteLedgers.Contains(ledger))
            {
                migratedState.BlackRouteLedgers.Add(ledger);
            }

            int activeRouteCount = migratedState.Routes.Count(route => route.IsActive && route.SettlementId == market.SettlementId);
            int routeCapacity = migratedState.Routes
                .Where(route => route.IsActive && route.SettlementId == market.SettlementId)
                .Sum(static route => route.Capacity);

            ledger.ShadowPriceIndex = Math.Clamp(
                Math.Max(ledger.ShadowPriceIndex, 100)
                + ((market.PriceIndex - 100) / 2)
                + (market.LocalRisk / 5),
                70,
                180);
            ledger.DiversionShare = Math.Clamp(
                Math.Max(ledger.DiversionShare, (market.LocalRisk / 8) + (activeRouteCount * 4)),
                0,
                100);
            ledger.BlockedShipmentCount = Math.Clamp(
                Math.Max(ledger.BlockedShipmentCount, (market.LocalRisk >= 55 ? 1 : 0) + (activeRouteCount >= 2 ? 1 : 0)),
                0,
                12);
            ledger.SeizureRisk = Math.Clamp(
                Math.Max(ledger.SeizureRisk, (market.LocalRisk / 3) + (activeRouteCount * 2)),
                0,
                100);
            ledger.IllicitMargin = Math.Clamp(
                Math.Max(ledger.IllicitMargin, ((ledger.ShadowPriceIndex - 100) / 5) + (routeCapacity / 40) - ledger.BlockedShipmentCount),
                -10,
                30);
            ledger.DiversionBandLabel = string.IsNullOrWhiteSpace(ledger.DiversionBandLabel)
                ? DetermineMigratedDiversionBandLabel(ledger.DiversionShare, ledger.SeizureRisk, ledger.IllicitMargin)
                : ledger.DiversionBandLabel;
            ledger.LastLedgerTrace = string.IsNullOrWhiteSpace(ledger.LastLedgerTrace)
                ? $"{market.MarketName}的私下分流由旧档补出，先按市险与活路回填。"
                : ledger.LastLedgerTrace;
        }

        migratedState.BlackRouteLedgers = migratedState.BlackRouteLedgers
            .OrderBy(static ledger => ledger.SettlementId.Value)
            .ToList();

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.TradeAndIndustry,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(TradeAndIndustryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateTradeAndIndustryStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        TradeAndIndustryState migratedState = (TradeAndIndustryState)serializer.Deserialize(typeof(TradeAndIndustryState), envelope.Payload);

        foreach (RouteTradeState route in migratedState.Routes)
        {
            SettlementBlackRouteLedgerState? ledger = migratedState.BlackRouteLedgers
                .SingleOrDefault(existing => existing.SettlementId == route.SettlementId);
            int blockedShipmentCount = Math.Clamp(
                Math.Max(route.BlockedShipmentCount, ledger is null ? 0 : (ledger.BlockedShipmentCount > 0 ? 1 : 0) + (ledger.BlockedShipmentCount >= 3 ? 1 : 0)),
                0,
                6);
            int seizureRisk = Math.Clamp(
                Math.Max(route.SeizureRisk, (route.Risk / 5) + (ledger?.SeizureRisk ?? 0) / 2),
                0,
                100);

            route.BlockedShipmentCount = blockedShipmentCount;
            route.SeizureRisk = seizureRisk;
            route.RouteConstraintLabel = string.IsNullOrWhiteSpace(route.RouteConstraintLabel)
                ? DetermineMigratedRouteConstraintLabel(blockedShipmentCount, seizureRisk, route.Risk)
                : route.RouteConstraintLabel;
            route.LastRouteTrace = string.IsNullOrWhiteSpace(route.LastRouteTrace)
                ? $"{route.RouteName}旧档已按阻货与查缉势回填。"
                : route.LastRouteTrace;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.TradeAndIndustry,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(TradeAndIndustryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.BlackRoutePressure = Math.Clamp(
                Math.Max(settlement.BlackRoutePressure, (settlement.BanditThreat + settlement.RoutePressure + settlement.DisorderPressure) / 3),
                0,
                100);
            settlement.CoercionRisk = Math.Clamp(
                Math.Max(settlement.CoercionRisk, (settlement.BlackRoutePressure / 2) + (settlement.DisorderPressure / 3)),
                0,
                100);
            settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief, 0, 12);
            settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel, 0, 12);
            settlement.AdministrativeSuppressionWindow = Math.Clamp(settlement.AdministrativeSuppressionWindow, 0, 8);
            settlement.EscalationBandLabel = string.IsNullOrWhiteSpace(settlement.EscalationBandLabel)
                ? DetermineMigratedEscalationBandLabel(settlement.BlackRoutePressure, settlement.CoercionRisk)
                : settlement.EscalationBandLabel;
            settlement.LastPressureTrace = string.IsNullOrWhiteSpace(settlement.LastPressureTrace)
                ? (string.IsNullOrWhiteSpace(settlement.LastPressureReason)
                    ? "旧档私路压力已按地面不靖回填。"
                    : settlement.LastPressureReason)
                : settlement.LastPressureTrace;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.PaperCompliance = Math.Clamp(
                Math.Max(settlement.PaperCompliance, (settlement.SuppressionRelief * 18) + (settlement.AdministrativeSuppressionWindow * 12)),
                0,
                100);
            settlement.ImplementationDrag = Math.Clamp(
                Math.Max(settlement.ImplementationDrag, settlement.BlackRoutePressure - (settlement.SuppressionRelief * 6) - (settlement.AdministrativeSuppressionWindow * 8)),
                0,
                100);
            settlement.AdministrativeSuppressionWindow = Math.Clamp(settlement.AdministrativeSuppressionWindow, 0, 8);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.RouteShielding = Math.Clamp(
                Math.Max(
                    settlement.RouteShielding,
                    (settlement.ResponseActivationLevel * 8)
                    + (settlement.SuppressionRelief * 6)
                    - (settlement.RoutePressure / 4)),
                0,
                100);
            settlement.RetaliationRisk = Math.Clamp(
                Math.Max(
                    settlement.RetaliationRisk,
                    (settlement.CoercionRisk / 2)
                    + Math.Max(0, settlement.BlackRoutePressure - (settlement.RouteShielding / 2))
                    - (settlement.SuppressionRelief * 4)
                    - (settlement.AdministrativeSuppressionWindow * 5)),
                0,
                100);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV4ToV5(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.LastInterventionCommandCode ??= string.Empty;
            settlement.LastInterventionCommandLabel ??= string.Empty;
            settlement.LastInterventionSummary ??= string.Empty;
            settlement.LastInterventionOutcome ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 5,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV5ToV6(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.InterventionCarryoverMonths = Math.Clamp(settlement.InterventionCarryoverMonths, 0, 1);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 6,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
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
