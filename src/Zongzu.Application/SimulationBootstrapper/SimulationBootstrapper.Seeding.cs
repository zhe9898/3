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

}
