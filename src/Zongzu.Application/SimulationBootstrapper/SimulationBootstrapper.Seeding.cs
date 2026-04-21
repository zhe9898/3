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

public static partial class SimulationBootstrapper
{
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
            // STEP2A / A0a — 兰溪县治配坐堂医（非州府名医）。band 而非数字。
            HealerAccess = HealerAccess.Local,
            // STEP2A / A0b — 县治带 Lay 档香火通道（寺观平行通道，不是第二家医院）。
            TempleHealingPresence = TempleHealingPresence.Lay,
        });

        familyState.Clans.Add(new ClanStateData
        {
            Id = clanId,
            ClanName = "张",
            HomeSettlementId = settlementId,
            Prestige = 52,
            SupportReserve = 60,
            HeirPersonId = heirId,
            // STEP2A / A0a — 家内照料链初值：无葬债，照料轻，问医信心中等偏低。
            CareLoad = 6,
            FuneralDebt = 0,
            RemedyConfidence = 40,
        });

        familyState.People.Add(new FamilyPersonState
        {
            Id = heirId,
            ClanId = clanId,
            GivenName = "张远",
            AgeMonths = 32 * 12,
            IsAlive = true,
            BranchPosition = BranchPosition.MainLineHeir,
            Ambition = 55,
            Prudence = 52,
            Loyalty = 60,
            Sociability = 48,
        });

        // Phase 2b.2: every seeded FamilyPersonState must also land in
        // PersonRegistry so IPersonRegistryQueries is authoritative for
        // age / IsAlive instead of FamilyCore's local mirror.
        // See PERSON_OWNERSHIP_RULES.md.
        SeedPersonRecord(
            personRegistryState,
            simulation.CurrentDate,
            heirId,
            "张远",
            ageMonths: 32 * 12,
            gender: PersonGender.Male,
            fidelityRing: FidelityRing.Core);

        // A2: 张宗跨代口——父辈 / 配 / 少 / 幼
        SeedClanKinship(
            familyState,
            personRegistryState,
            simulation,
            clanId,
            elderName: "张德",
            spouseName: "张王氏",
            youthName: "张敬",
            childName: "张晓");

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
            // STEP2A / A0a — 压力片子县 / 镇 / 村按 Tier 推一档，避免同质 0–100。
            HealerAccess = InferStressSettlementTier(slice.SettlementName) switch
            {
                SettlementTier.PrefectureSeat => HealerAccess.Renowned,
                SettlementTier.CountySeat => HealerAccess.Local,
                SettlementTier.MarketTown => HealerAccess.Itinerant,
                _ => HealerAccess.None,
            },
            // STEP2A / A0b — 寺观 band 按 Tier 推断；village 多 Folk，县镇 Lay。
            TempleHealingPresence = InferStressSettlementTier(slice.SettlementName) switch
            {
                SettlementTier.PrefectureSeat => TempleHealingPresence.Institutional,
                SettlementTier.CountySeat => TempleHealingPresence.Lay,
                SettlementTier.MarketTown => TempleHealingPresence.Lay,
                _ => TempleHealingPresence.Folk,
            },
        });

        familyState.Clans.Add(new ClanStateData
        {
            Id = clanId,
            ClanName = slice.ClanName,
            HomeSettlementId = settlementId,
            Prestige = slice.ClanPrestige,
            SupportReserve = slice.ClanSupport,
            HeirPersonId = heirId,
            // STEP2A / A0a — 旁房照料初值：比宗房略紧，葬债尚无。
            CareLoad = 10,
            FuneralDebt = 0,
            RemedyConfidence = Math.Clamp((slice.ClanPrestige / 4) + 25, 0, 60),
        });

        familyState.People.Add(new FamilyPersonState
        {
            Id = heirId,
            ClanId = clanId,
            GivenName = slice.HeirName,
            AgeMonths = 30 * 12,
            IsAlive = true,
            BranchPosition = BranchPosition.MainLineHeir,
            Ambition = 50,
            Prudence = 50,
            Loyalty = 50,
            Sociability = 50,
        });

        // Phase 2b.2: stress-slice heirs also land in PersonRegistry.
        PersonRegistryState stressPersonRegistryState = simulation.GetMutableModuleState<PersonRegistryState>(KnownModuleKeys.PersonRegistry);
        SeedPersonRecord(
            stressPersonRegistryState,
            simulation.CurrentDate,
            heirId,
            slice.HeirName,
            ageMonths: 30 * 12,
            gender: PersonGender.Male,
            fidelityRing: FidelityRing.Local);

        // A2: 旁房跨代口——父辈 / 配 / 少 / 幼
        (string elder, string spouse, string youth, string child) kin = slice.ClanName switch
        {
            "李" => ("李宏", "李范氏", "李承", "李幼"),
            "吴" => ("吴安", "吴沈氏", "吴旭", "吴小"),
            "陈" => ("陈谦", "陈周氏", "陈明", "陈稚"),
            _ => (slice.ClanName + "长", slice.ClanName + "氏", slice.ClanName + "少", slice.ClanName + "幼"),
        };
        SeedClanKinship(
            familyState,
            stressPersonRegistryState,
            simulation,
            clanId,
            elderName: kin.elder,
            spouseName: kin.spouse,
            youthName: kin.youth,
            childName: kin.child);

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

    /// <summary>
    /// Phase 2b.2 — single seam for seeding a <see cref="PersonRecord"/> with
    /// an age-derived <c>BirthDate</c> and matching <see cref="LifeStage"/>.
    /// Every <c>FamilyPersonState</c> seeded into the world must be mirrored
    /// here so <c>IPersonRegistryQueries.GetAgeMonths</c> is authoritative
    /// and FamilyCore's local age mirror is only a transitional fallback.
    /// See <c>PERSON_OWNERSHIP_RULES.md</c>.
    /// </summary>
    private static void SeedPersonRecord(
        PersonRegistryState personRegistryState,
        GameDate currentDate,
        PersonId id,
        string displayName,
        int ageMonths,
        PersonGender gender,
        FidelityRing fidelityRing)
    {
        int years = ageMonths / 12;
        int monthsRemainder = ageMonths % 12;
        int birthYear = currentDate.Year - years;
        int birthMonth = currentDate.Month - monthsRemainder;
        if (birthMonth <= 0)
        {
            birthMonth += 12;
            birthYear -= 1;
        }

        personRegistryState.Persons.Add(new PersonRecord
        {
            Id = id,
            DisplayName = displayName,
            BirthDate = new GameDate(birthYear, birthMonth),
            Gender = gender,
            LifeStage = PersonRegistryModule.ResolveLifeStage(ageMonths),
            IsAlive = true,
            FidelityRing = fidelityRing,
        });
    }

    /// <summary>
    /// Step 2-A / A2 — seed a cross-generation kinship circle around a clan's
    /// main-line heir: one elder (父辈), one spouse (配), one youth (少年次男),
    /// one child (幼). Every seeded FamilyPersonState is mirrored into
    /// PersonRegistry via <see cref="SeedPersonRecord"/>.
    ///
    /// <para>A2 is a pure seed expansion — it does not introduce new fields
    /// or rules. HealerAccess / CareLoad / RemedyConfidence / CharityObligation
    /// etc. are reserved for A0a–A0d. Marriage-in semantics for the spouse
    /// are left at <see cref="BranchPosition.DependentKin"/> until A4 婚议.</para>
    /// </summary>
    private static void SeedClanKinship(
        FamilyCoreState familyState,
        PersonRegistryState personRegistry,
        GameSimulation simulation,
        ClanId clanId,
        string elderName,
        string spouseName,
        string youthName,
        string childName)
    {
        PersonId elderId = KernelIdAllocator.NextPerson(simulation.KernelState);
        PersonId spouseId = KernelIdAllocator.NextPerson(simulation.KernelState);
        PersonId youthId = KernelIdAllocator.NextPerson(simulation.KernelState);
        PersonId childId = KernelIdAllocator.NextPerson(simulation.KernelState);

        // 父辈 — 63 岁，在籍族众之长者
        familyState.People.Add(new FamilyPersonState
        {
            Id = elderId,
            ClanId = clanId,
            GivenName = elderName,
            AgeMonths = 63 * 12,
            IsAlive = true,
            BranchPosition = BranchPosition.BranchHead,
            Ambition = 38,
            Prudence = 62,
            Loyalty = 58,
            Sociability = 44,
        });
        SeedPersonRecord(personRegistry, simulation.CurrentDate, elderId, elderName,
            ageMonths: 63 * 12, gender: PersonGender.Male, fidelityRing: FidelityRing.Core);

        // 配 — 28 岁，heir 之妻
        familyState.People.Add(new FamilyPersonState
        {
            Id = spouseId,
            ClanId = clanId,
            GivenName = spouseName,
            AgeMonths = 28 * 12,
            IsAlive = true,
            BranchPosition = BranchPosition.DependentKin,
            Ambition = 34,
            Prudence = 56,
            Loyalty = 60,
            Sociability = 52,
        });
        SeedPersonRecord(personRegistry, simulation.CurrentDate, spouseId, spouseName,
            ageMonths: 28 * 12, gender: PersonGender.Female, fidelityRing: FidelityRing.Local);

        // 少年 — 17 岁，次房 youth
        familyState.People.Add(new FamilyPersonState
        {
            Id = youthId,
            ClanId = clanId,
            GivenName = youthName,
            AgeMonths = 17 * 12,
            IsAlive = true,
            BranchPosition = BranchPosition.BranchMember,
            Ambition = 48,
            Prudence = 40,
            Loyalty = 52,
            Sociability = 50,
        });
        SeedPersonRecord(personRegistry, simulation.CurrentDate, youthId, youthName,
            ageMonths: 17 * 12, gender: PersonGender.Male, fidelityRing: FidelityRing.Local);

        // 幼 — 8 岁
        familyState.People.Add(new FamilyPersonState
        {
            Id = childId,
            ClanId = clanId,
            GivenName = childName,
            AgeMonths = 8 * 12,
            IsAlive = true,
            BranchPosition = BranchPosition.DependentKin,
            Ambition = 30,
            Prudence = 30,
            Loyalty = 50,
            Sociability = 46,
        });
        SeedPersonRecord(personRegistry, simulation.CurrentDate, childId, childName,
            ageMonths: 8 * 12, gender: PersonGender.Male, fidelityRing: FidelityRing.Local);
    }

}
