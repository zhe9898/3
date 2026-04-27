using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Modules.SocialMemoryAndRelations.Tests;

[TestFixture]
public sealed class SocialMemoryAndRelationsModuleTests
{
    [Test]
    public void RunXun_ShangxunUpdatesFearAndGrudgeWithoutReadableOutput()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 48,
            SupportReserve = 38,
            HeirPersonId = new PersonId(1),
            BranchTension = 64,
            MediationMomentum = 10,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 76,
            DebtPressure = 72,
            LaborCapacity = 30,
            MigrationRisk = 68,
            IsMigrating = true,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Quiet watchfulness",
            GrudgePressure = 24,
            FearPressure = 18,
            ShamePressure = 10,
            FavorBalance = 3,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 2),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(71)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Shangxun);

        socialModule.RunXun(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        ClanNarrativeState narrative = socialState.ClanNarratives.Single();
        Assert.That(narrative.FearPressure, Is.EqualTo(20));
        Assert.That(narrative.GrudgePressure, Is.EqualTo(26));
        Assert.That(narrative.ShamePressure, Is.EqualTo(10));
        Assert.That(narrative.FavorBalance, Is.EqualTo(3));
        Assert.That(socialState.Memories, Is.Empty);
        Assert.That(context.Diff.Entries, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunXun_ZhongAndXiaxunUpdateShameFavorAndLateMonthFeudWithoutReadableOutput()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 42,
            SupportReserve = 68,
            HeirPersonId = new PersonId(1),
            MediationMomentum = 48,
            ReliefSanctionPressure = 41,
            BranchFavorPressure = 45,
            SeparationPressure = 62,
            InheritancePressure = 57,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 32,
            DebtPressure = 28,
            LaborCapacity = 48,
            MigrationRisk = 64,
            IsMigrating = true,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Quiet watchfulness",
            GrudgePressure = 30,
            FearPressure = 22,
            ShamePressure = 14,
            FavorBalance = 5,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<ITradeAndIndustryQueries>(new StubTradeQueries(
        [
            new ClanTradeSnapshot
            {
                ClanId = new ClanId(1),
                PrimarySettlementId = new SettlementId(1),
                CashReserve = 110,
                GrainReserve = 54,
                Debt = 92,
                CommerceReputation = 36,
                ShopCount = 1,
                LastOutcome = "Stable",
                LastExplanation = string.Empty,
            },
        ]));
        socialModule.RegisterQueries(socialState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);

        ModuleExecutionContext zhongxunContext = new(
            new GameDate(1200, 2),
            manifest,
            new DeterministicRandom(KernelState.Create(72)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Zhongxun);

        socialModule.RunXun(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, zhongxunContext));

        ModuleExecutionContext xiaxunContext = new(
            new GameDate(1200, 2),
            manifest,
            new DeterministicRandom(KernelState.Create(72)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Xiaxun);

        socialModule.RunXun(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, xiaxunContext));

        ClanNarrativeState narrative = socialState.ClanNarratives.Single();
        Assert.That(narrative.GrudgePressure, Is.EqualTo(32));
        Assert.That(narrative.FearPressure, Is.EqualTo(23));
        Assert.That(narrative.ShamePressure, Is.EqualTo(18));
        Assert.That(narrative.FavorBalance, Is.EqualTo(7));
        Assert.That(socialState.Memories, Is.Empty);
        Assert.That(zhongxunContext.Diff.Entries, Is.Empty);
        Assert.That(zhongxunContext.DomainEvents.Events, Is.Empty);
        Assert.That(xiaxunContext.Diff.Entries, Is.Empty);
        Assert.That(xiaxunContext.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_PreservesAndEscalatesGrudgesUnderRepeatedStrain()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 42,
            SupportReserve = 35,
            HeirPersonId = new PersonId(1),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 78,
            DebtPressure = 82,
            LaborCapacity = 24,
            MigrationRisk = 71,
            IsMigrating = false,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);

        KernelState kernelState = KernelState.Create(5);
        for (int month = 0; month < 36; month += 1)
        {
            ModuleExecutionContext context = new(
                new GameDate(1200 + (month / 12), (month % 12) + 1),
                new FeatureManifest(),
                new DeterministicRandom(kernelState),
                queries,
                new DomainEventBuffer(),
                new WorldDiff(),
                kernelState);

            socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));
        }

        Assert.That(socialState.ClanNarratives, Has.Count.EqualTo(1));
        Assert.That(socialState.ClanNarratives[0].GrudgePressure, Is.GreaterThanOrEqualTo(60));
        Assert.That(socialState.Memories.Count, Is.GreaterThan(0));
    }

    [Test]
    public void HandleEvents_AddsCampaignMemoryAndRaisesFearInsideSocialState()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 42,
            SupportReserve = 35,
            HeirPersonId = new PersonId(1),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Quiet watchfulness",
            GrudgePressure = 34,
            FearPressure = 28,
            ShamePressure = 19,
            FavorBalance = 10,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi军务沙盘",
                IsActive = true,
                MobilizedForceCount = 40,
                FrontPressure = 72,
                FrontLabel = "前线转紧",
                SupplyState = 35,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 39,
                MoraleStateLabel = "军心摇动",
                CommandFitLabel = "号令尚整",
                CommanderSummary = "Lanxi command is strained.",
                ActiveDirectiveCode = WarfareCampaignCommandNames.CommitMobilization,
                ActiveDirectiveLabel = "发檄点兵",
                ActiveDirectiveSummary = "整众应调。",
                LastDirectiveTrace = "兰溪已发檄点兵。",
                MobilizationWindowLabel = "可发",
                SupplyLineSummary = "仓路与渡口都在看守之下。",
                OfficeCoordinationTrace = "主簿正在转递军务文移。",
                SourceTrace = "Campaign pressure rose from local conflict.",
                LastAftermathSummary = "战后覆核仍在地方记忆里发酵。",
            },
        ]));

        DomainEventRecord[] events =
        {
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi aftermath registered.", "1"),
        };

        KernelState kernelState = KernelState.Create(7304);
        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            new FeatureManifest(),
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.HandleEvents(new ModuleEventHandlingScope<SocialMemoryAndRelationsState>(socialState, context, events));

        ClanNarrativeState narrative = socialState.ClanNarratives.Single();

        Assert.That(narrative.FearPressure, Is.GreaterThan(28));
        Assert.That(narrative.GrudgePressure, Is.GreaterThan(34));
        Assert.That(narrative.PublicNarrative, Does.Contain("Lanxi"));
        Assert.That(socialState.Memories, Has.Count.EqualTo(1));
        Assert.That(socialState.Memories[0].Kind, Is.EqualTo("campaign-aftermath"));
        Assert.That(socialState.Memories[0].Summary, Does.Contain("Lanxi"));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.SocialMemoryAndRelations));
    }

    [Test]
    public void RunMonth_FamilyConflictPressure_ShapesClanNarrativeTowardAncestralHallDispute()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 51,
            SupportReserve = 48,
            HeirPersonId = new PersonId(1),
            BranchTension = 64,
            InheritancePressure = 58,
            SeparationPressure = 68,
            MediationMomentum = 4,
            BranchFavorPressure = 33,
            ReliefSanctionPressure = 42,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 44,
            DebtPressure = 41,
            LaborCapacity = 52,
            MigrationRisk = 28,
            IsMigrating = false,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(29)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            KernelState.Create(29));

        socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        Assert.That(socialState.ClanNarratives, Has.Count.EqualTo(1));
        Assert.That(socialState.ClanNarratives[0].PublicNarrative, Does.Contain("分房"));
        Assert.That(socialState.ClanNarratives[0].GrudgePressure, Is.GreaterThan(0));
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("旧怨"));
    }

    [Test]
    public void RunMonth_ReliefPath_AddsStructuredFavorMemoryWithReliefFavorSubtype()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 52,
            SupportReserve = 72,
            HeirPersonId = new PersonId(1),
            MediationMomentum = 58,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 30,
            DebtPressure = 20,
            LaborCapacity = 55,
            MigrationRisk = 15,
            IsMigrating = false,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            GrudgePressure = 46,
            FearPressure = 10,
            ShamePressure = 10,
            FavorBalance = 6,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);

        KernelState kernelState = KernelState.Create(9001);
        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        MemoryRecordState reliefMemory = socialState.Memories.Single(memory => memory.CauseKey == "clan.relief");
        Assert.That(reliefMemory.Type, Is.EqualTo(MemoryType.Favor));
        Assert.That(reliefMemory.Subtype, Is.EqualTo(MemorySubtype.ReliefFavor));
        Assert.That(reliefMemory.LifecycleState, Is.EqualTo(MemoryLifecycleState.Active));
        Assert.That(reliefMemory.SourceKind, Is.EqualTo(MemorySubjectKind.Clan));
        Assert.That(reliefMemory.SourceClanId, Is.EqualTo(new ClanId(1)));
        Assert.That(reliefMemory.Weight, Is.GreaterThan(0));
        Assert.That(reliefMemory.MonthlyDecay, Is.GreaterThanOrEqualTo(1));

        ISocialMemoryAndRelationsQueries socialQueries = queries.GetRequired<ISocialMemoryAndRelationsQueries>();
        IReadOnlyList<SocialMemoryEntrySnapshot> structured = socialQueries.GetMemoriesByClan(new ClanId(1));
        Assert.That(structured, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(structured.Any(entry => entry.Subtype == MemorySubtype.ReliefFavor), Is.True);
    }

    [Test]
    public void RunMonth_MemoryLifecycle_DecaysActiveToDormantOverMonths()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 50,
            HeirPersonId = new PersonId(1),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.Memories.Add(new MemoryRecordState
        {
            Id = new MemoryId(1),
            SubjectClanId = new ClanId(1),
            Kind = "seed",
            Intensity = 20,
            IsPublic = true,
            CreatedAt = new GameDate(1200, 1),
            Summary = "seeded",
            Type = MemoryType.Grudge,
            Subtype = MemorySubtype.WealthGrudge,
            SourceKind = MemorySubjectKind.Clan,
            SourceClanId = new ClanId(1),
            TargetKind = MemorySubjectKind.Clan,
            TargetClanId = new ClanId(1),
            OriginDate = new GameDate(1200, 1),
            CauseKey = "seed",
            Weight = 6,
            MonthlyDecay = 2,
            LifecycleState = MemoryLifecycleState.Active,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);

        KernelState kernelState = KernelState.Create(42);
        for (int month = 0; month < 4; month += 1)
        {
            ModuleExecutionContext context = new(
                new GameDate(1200, month + 2),
                new FeatureManifest(),
                new DeterministicRandom(kernelState),
                queries,
                new DomainEventBuffer(),
                new WorldDiff(),
                kernelState);
            socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));
        }

        MemoryRecordState seed = socialState.Memories.Single(memory => memory.CauseKey == "seed");
        Assert.That(seed.Weight, Is.EqualTo(0));
        Assert.That(seed.LifecycleState, Is.EqualTo(MemoryLifecycleState.Dormant));
    }

    [Test]
    public void HandleEvents_ChildLossFromFamilyCore_RaisesFearAndAddsChildLossMemory()
    {
        // STEP2A / A5 — FamilyCore 发的 DeathByIllness（婴幼儿分流）应在
        // 死者所属 clan 提 FearPressure 并添一条 Fear/MourningDebt 记忆。
        // Population 的 DeathByIllness（族外佃户 / 成人）不走此通道。
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(11),
            ClanId = new ClanId(1),
            GivenName = "Zhang Wa",
            AgeMonths = 6,
            IsAlive = false,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());

        DomainEventRecord[] events =
        {
            // FamilyCore child death
            new(KnownModuleKeys.FamilyCore, DeathCauseEventNames.DeathByIllness, "Zhang门内幼儿病殁。", "11"),
            // Population adult illness — 不应触发 child_loss
            new(KnownModuleKeys.PopulationAndHouseholds, DeathCauseEventNames.DeathByIllness, "佃户病殁。", "999"),
        };

        KernelState kernelState = KernelState.Create(915);
        ModuleExecutionContext context = new(
            new GameDate(1200, 12),
            new FeatureManifest(),
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.HandleEvents(new ModuleEventHandlingScope<SocialMemoryAndRelationsState>(socialState, context, events));

        ClanNarrativeState narrative = socialState.ClanNarratives.Single();
        Assert.That(narrative.FearPressure, Is.GreaterThan(0));

        MemoryRecordState[] childLoss = socialState.Memories.Where(m => m.Kind == "child_loss").ToArray();
        Assert.That(childLoss, Has.Length.EqualTo(1));
        Assert.That(childLoss[0].Type, Is.EqualTo(MemoryType.Fear));
        Assert.That(childLoss[0].Subtype, Is.EqualTo(MemorySubtype.MourningDebt));
        Assert.That(childLoss[0].SubjectClanId, Is.EqualTo(new ClanId(1)));
    }

    [Test]
    public void RunMonth_MultidimensionalPressure_BuildsClanClimateAndPersonalTempering()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 34,
            SupportReserve = 24,
            HeirPersonId = new PersonId(1),
            BranchTension = 75,
            InheritancePressure = 70,
            SeparationPressure = 65,
            MediationMomentum = 8,
            ReliefSanctionPressure = 64,
            BranchFavorPressure = 62,
            MourningLoad = 56,
            CareLoad = 72,
            FuneralDebt = 50,
            CharityObligation = 58,
            HeirSecurity = 30,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Jin",
            AgeMonths = 30 * 12,
            IsAlive = true,
            Ambition = 82,
            Prudence = 25,
            Loyalty = 20,
            Sociability = 35,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(2),
            ClanId = new ClanId(1),
            GivenName = "Zhang Shen",
            AgeMonths = 42 * 12,
            IsAlive = true,
            Ambition = 35,
            Prudence = 80,
            Loyalty = 75,
            Sociability = 65,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 84,
            DebtPressure = 88,
            LaborCapacity = 22,
            MigrationRisk = 78,
            IsMigrating = true,
        });
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Tenant Zhou",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 78,
            DebtPressure = 76,
            LaborCapacity = 35,
            MigrationRisk = 74,
            IsMigrating = false,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            GrudgePressure = 38,
            FearPressure = 24,
            ShamePressure = 18,
            FavorBalance = 2,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<ITradeAndIndustryQueries>(new StubTradeQueries(
        [
            new ClanTradeSnapshot
            {
                ClanId = new ClanId(1),
                PrimarySettlementId = new SettlementId(1),
                CashReserve = 18,
                GrainReserve = 20,
                Debt = 96,
                CommerceReputation = 30,
                ShopCount = 1,
                LastOutcome = "Strained",
            },
        ]));
        socialModule.RegisterQueries(socialState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);

        KernelState kernelState = KernelState.Create(1401);
        ModuleExecutionContext context = new(
            new GameDate(1200, 4),
            manifest,
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        ISocialMemoryAndRelationsQueries socialQueries = queries.GetRequired<ISocialMemoryAndRelationsQueries>();
        ClanEmotionalClimateSnapshot climate = socialQueries.GetClanEmotionalClimate(new ClanId(1));
        PersonPressureTemperingSnapshot ambitious = socialQueries.FindPersonTempering(new PersonId(1))!;
        PersonPressureTemperingSnapshot cautious = socialQueries.FindPersonTempering(new PersonId(2))!;

        Assert.That(climate.LastPressureScore, Is.EqualTo(100));
        Assert.That(climate.LastPressureBand, Is.EqualTo(3));
        Assert.That(climate.Fear, Is.GreaterThan(0));
        Assert.That(climate.Shame, Is.GreaterThan(0));
        Assert.That(climate.Anger, Is.GreaterThan(0));
        Assert.That(climate.Hardening, Is.GreaterThan(0));
        Assert.That(socialState.PersonTemperings, Has.Count.EqualTo(2));
        Assert.That(cautious.Restraint, Is.GreaterThan(ambitious.Restraint));
        Assert.That(ambitious.Volatility, Is.GreaterThan(cautious.Volatility));
        Assert.That(
            context.DomainEvents.Events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == SocialMemoryAndRelationsEventNames.EmotionalPressureShifted
                     && e.EntityKey == "1"
                     && e.Metadata[DomainEventMetadataKeys.SocialPressureScore] == "100"));
    }

    [Test]
    public void HandleEvents_ScopedTradeShock_UpdatesOnlyTargetClanClimate()
    {
        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState { ClanId = new ClanId(1) });
        socialState.ClanNarratives.Add(new ClanNarrativeState { ClanId = new ClanId(2) });
        socialState.ClanEmotionalClimates.Add(new ClanEmotionalClimateState
        {
            ClanId = new ClanId(1),
            Fear = 35,
            Shame = 10,
        });
        socialState.ClanEmotionalClimates.Add(new ClanEmotionalClimateState
        {
            ClanId = new ClanId(2),
            Fear = 35,
            Shame = 10,
        });

        QueryRegistry queries = new();
        socialModule.RegisterQueries(socialState, queries);
        DomainEventRecord[] events =
        {
            new(
                KnownModuleKeys.TradeAndIndustry,
                TradeAndIndustryEventNames.TradeDebtDefaulted,
                "Zhang trade debt defaulted.",
                "1"),
        };

        KernelState kernelState = KernelState.Create(1402);
        ModuleExecutionContext context = new(
            new GameDate(1200, 5),
            new FeatureManifest(),
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.HandleEvents(new ModuleEventHandlingScope<SocialMemoryAndRelationsState>(socialState, context, events));

        ClanEmotionalClimateState target = socialState.ClanEmotionalClimates.Single(static climate => climate.ClanId == new ClanId(1));
        ClanEmotionalClimateState untouched = socialState.ClanEmotionalClimates.Single(static climate => climate.ClanId == new ClanId(2));
        Assert.That(target.Fear, Is.EqualTo(39));
        Assert.That(target.Shame, Is.EqualTo(15));
        Assert.That(target.Anger, Is.EqualTo(3));
        Assert.That(untouched.Fear, Is.EqualTo(35));
        Assert.That(untouched.Shame, Is.EqualTo(10));
        Assert.That(untouched.Anger, Is.EqualTo(0));
        Assert.That(socialState.Memories.Single().Subtype, Is.EqualTo(MemorySubtype.TradeBreach));
        Assert.That(
            context.DomainEvents.Events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == SocialMemoryAndRelationsEventNames.EmotionalPressureShifted
                     && e.EntityKey == "1"));
    }

    [Test]
    public void HandleEvents_ExamPassed_ShapesPersonTemperingAndAspirationMemory()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(11),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 20 * 12,
            IsAlive = true,
            Ambition = 72,
            Prudence = 58,
            Loyalty = 65,
            Sociability = 60,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        DomainEventRecord[] events =
        {
            new(
                KnownModuleKeys.EducationAndExams,
                EducationAndExamsEventNames.ExamPassed,
                "Zhang Yuan passed the county exam.",
                "11"),
        };

        KernelState kernelState = KernelState.Create(1403);
        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.HandleEvents(new ModuleEventHandlingScope<SocialMemoryAndRelationsState>(socialState, context, events));

        PersonPressureTemperingState person = socialState.PersonTemperings.Single();
        ClanEmotionalClimateState climate = socialState.ClanEmotionalClimates.Single();
        MemoryRecordState memory = socialState.Memories.Single();

        Assert.That(person.PersonId, Is.EqualTo(new PersonId(11)));
        Assert.That(person.Hope, Is.EqualTo(7));
        Assert.That(person.Obligation, Is.EqualTo(2));
        Assert.That(climate.Hope, Is.EqualTo(7));
        Assert.That(climate.Trust, Is.EqualTo(3));
        Assert.That(memory.Type, Is.EqualTo(MemoryType.Aspiration));
        Assert.That(memory.Subtype, Is.EqualTo(MemorySubtype.ExamHonor));
        Assert.That(memory.CauseKey, Is.EqualTo("exam.pass"));
    }

    [Test]
    public void RunMonth_PublicLifeOrderAftermath_WritesOwnerOwnedResidueFromOrderQuery()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(7),
            Prestige = 55,
            SupportReserve = 44,
            HeirPersonId = new PersonId(1),
        });
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Li",
            HomeSettlementId = new SettlementId(8),
            Prestige = 80,
            SupportReserve = 44,
            HeirPersonId = new PersonId(2),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Zhang waits on the road matter.",
            FearPressure = 12,
            ShamePressure = 8,
            GrudgePressure = 10,
            FavorBalance = 2,
        });
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(2),
            PublicNarrative = "Li is off-scope.",
            FearPressure = 12,
            ShamePressure = 8,
            GrudgePressure = 10,
            FavorBalance = 2,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries(
        [
            new SettlementDisorderSnapshot
            {
                SettlementId = new SettlementId(7),
                RoutePressure = 35,
                SuppressionDemand = 26,
                DisorderPressure = 28,
                RouteShielding = 18,
                RetaliationRisk = 12,
                LastInterventionCommandCode = PlayerCommandNames.FundLocalWatch,
                LastInterventionCommandLabel = "添雇巡丁",
                InterventionCarryoverMonths = 1,
            },
        ]));
        socialModule.RegisterQueries(socialState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        KernelState kernelState = KernelState.Create(1410);
        ModuleExecutionContext context = new(
            new GameDate(1200, 8),
            manifest,
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        MemoryRecordState residue = socialState.Memories.Single(memory => memory.CauseKey == "order.public_life.fund_local_watch");
        Assert.That(residue.SubjectClanId, Is.EqualTo(new ClanId(1)));
        Assert.That(residue.Kind, Is.EqualTo(SocialMemoryKinds.PublicOrderWatchObligation));
        Assert.That(residue.Type, Is.EqualTo(MemoryType.Favor));
        Assert.That(residue.Subtype, Is.EqualTo(MemorySubtype.ProtectionFavor));
        Assert.That(residue.Summary, Does.Contain("添雇巡丁"));
        Assert.That(residue.Weight, Is.GreaterThan(0));

        ClanNarrativeState targetNarrative = socialState.ClanNarratives.Single(static narrative => narrative.ClanId == new ClanId(1));
        Assert.That(targetNarrative.FavorBalance, Is.GreaterThan(2));
        Assert.That(socialState.Memories.Any(static memory => memory.SubjectClanId == new ClanId(2)), Is.False);
        Assert.That(socialState.ClanEmotionalClimates.Single(static climate => climate.ClanId == new ClanId(1)).Obligation, Is.GreaterThan(0));
        Assert.That(context.Diff.Entries.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
        Assert.That(context.DomainEvents.Events.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
    }

    [Test]
    public void RunMonth_PublicLifeOrderRefusalAndPartialAftermath_ConsumesStructuredOrderTraceOnly()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(7),
            Prestige = 55,
            SupportReserve = 44,
            HeirPersonId = new PersonId(1),
        });
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Li",
            HomeSettlementId = new SettlementId(8),
            Prestige = 80,
            SupportReserve = 44,
            HeirPersonId = new PersonId(2),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries(
        [
            new SettlementDisorderSnapshot
            {
                SettlementId = new SettlementId(7),
                RoutePressure = 42,
                SuppressionDemand = 30,
                DisorderPressure = 34,
                RouteShielding = 8,
                RetaliationRisk = 36,
                ImplementationDrag = 62,
                LastInterventionCommandCode = PlayerCommandNames.FundLocalWatch,
                LastInterventionCommandLabel = "添雇巡丁",
                LastInterventionSummary = "summary text must not drive residue",
                LastInterventionOutcomeCode = OrderInterventionOutcomeCodes.Refused,
                LastInterventionRefusalCode = OrderInterventionRefusalCodes.WatchmenRefused,
                LastInterventionTraceCode = OrderInterventionTraceCodes.WatchGroundRefusal,
                RefusalCarryoverMonths = 1,
            },
            new SettlementDisorderSnapshot
            {
                SettlementId = new SettlementId(8),
                BanditThreat = 50,
                RoutePressure = 44,
                SuppressionDemand = 40,
                DisorderPressure = 36,
                BlackRoutePressure = 24,
                CoercionRisk = 58,
                RouteShielding = 10,
                RetaliationRisk = 52,
                LastInterventionCommandCode = PlayerCommandNames.SuppressBanditry,
                LastInterventionCommandLabel = "严缉路匪",
                LastInterventionSummary = "summary text must not drive residue",
                LastInterventionOutcomeCode = OrderInterventionOutcomeCodes.Partial,
                LastInterventionPartialCode = OrderInterventionPartialCodes.SuppressionBacklash,
                LastInterventionTraceCode = OrderInterventionTraceCodes.SuppressionBacklash,
                InterventionCarryoverMonths = 1,
            },
        ]));
        socialModule.RegisterQueries(socialState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        KernelState kernelState = KernelState.Create(1411);
        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            manifest,
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        MemoryRecordState watchRefusal = socialState.Memories.Single(memory => memory.CauseKey == "order.public_life.fund_local_watch.refused");
        Assert.That(watchRefusal.SubjectClanId, Is.EqualTo(new ClanId(1)));
        Assert.That(watchRefusal.Kind, Is.EqualTo(SocialMemoryKinds.PublicOrderWatchRefusalShame));
        Assert.That(watchRefusal.Type, Is.EqualTo(MemoryType.Shame));
        Assert.That(watchRefusal.Subtype, Is.EqualTo(MemorySubtype.PublicShame));
        Assert.That(watchRefusal.Summary, Does.Contain("添雇巡丁"));
        Assert.That(watchRefusal.Summary, Does.Not.Contain("summary text must not drive residue"));

        MemoryRecordState suppressionPartial = socialState.Memories.Single(memory => memory.CauseKey == "order.public_life.suppress_banditry.partial");
        Assert.That(suppressionPartial.SubjectClanId, Is.EqualTo(new ClanId(2)));
        Assert.That(suppressionPartial.Kind, Is.EqualTo(SocialMemoryKinds.PublicOrderSuppressionPartialGrudge));
        Assert.That(suppressionPartial.Type, Is.EqualTo(MemoryType.Grudge));
        Assert.That(suppressionPartial.Subtype, Is.EqualTo(MemorySubtype.PowerGrudge));
        Assert.That(suppressionPartial.Summary, Does.Contain("严缉路匪"));
        Assert.That(suppressionPartial.Summary, Does.Not.Contain("summary text must not drive residue"));

        Assert.That(socialState.ClanNarratives.Single(static narrative => narrative.ClanId == new ClanId(1)).ShamePressure, Is.GreaterThan(0));
        Assert.That(socialState.ClanEmotionalClimates.Single(static climate => climate.ClanId == new ClanId(2)).Anger, Is.GreaterThan(0));
        Assert.That(context.Diff.Entries.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
        Assert.That(context.DomainEvents.Events.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
    }

    [Test]
    public void RunMonth_OfficePolicyImplementationResidue_ReadsStructuredOfficeSnapshotOnly()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(7),
            Prestige = 55,
            SupportReserve = 44,
            HeirPersonId = new PersonId(1),
        });
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Li",
            HomeSettlementId = new SettlementId(8),
            Prestige = 80,
            SupportReserve = 44,
            HeirPersonId = new PersonId(2),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        queries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [
                new JurisdictionAuthoritySnapshot
                {
                    SettlementId = new SettlementId(7),
                    LeadOfficialName = "Zhang Yuan",
                    LeadOfficeTitle = "Registrar",
                    AuthorityTier = 3,
                    JurisdictionLeverage = 20,
                    ClerkDependence = 80,
                    PetitionPressure = 30,
                    PetitionBacklog = 20,
                    AdministrativeTaskLoad = 62,
                    PetitionOutcomeCategory = "Stalled",
                    LastPetitionOutcome = "projection prose must not drive residue",
                    LastAdministrativeTrace = "政策语气读回 文移指向读回 公议承压读法",
                },
                new JurisdictionAuthoritySnapshot
                {
                    SettlementId = new SettlementId(8),
                    LeadOfficialName = "Li Wen",
                    LeadOfficeTitle = "Registrar",
                    AuthorityTier = 1,
                    JurisdictionLeverage = 80,
                    ClerkDependence = 4,
                    PetitionPressure = 4,
                    PetitionBacklog = 2,
                    AdministrativeTaskLoad = 6,
                    PetitionOutcomeCategory = "Queued",
                },
            ]));
        socialModule.RegisterQueries(socialState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        KernelState kernelState = KernelState.Create(1412);
        ModuleExecutionContext context = new(
            new GameDate(1200, 10),
            manifest,
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        MemoryRecordState residue = socialState.Memories.Single(memory => memory.CauseKey == "office.policy_implementation.7.Stalled");
        Assert.That(residue.SubjectClanId, Is.EqualTo(new ClanId(1)));
        Assert.That(residue.Kind, Is.EqualTo(SocialMemoryKinds.OfficePolicyImplementationResidue));
        Assert.That(residue.Type, Is.EqualTo(MemoryType.Fear));
        Assert.That(residue.Subtype, Is.EqualTo(MemorySubtype.PowerGrudge));
        Assert.That(residue.Summary, Does.Contain("县门执行读回"));
        Assert.That(residue.Summary, Does.Contain("OfficeAndCareer"));
        Assert.That(residue.Summary, Does.Not.Contain("receipt prose"));
        Assert.That(residue.Summary, Does.Not.Contain("projection prose must not drive residue"));
        Assert.That(residue.Summary, Does.Not.Contain("政策语气读回"));
        Assert.That(residue.Summary, Does.Not.Contain("文移指向读回"));
        Assert.That(residue.Summary, Does.Not.Contain("公议承压读法"));
        Assert.That(socialState.Memories.Any(static memory => memory.SubjectClanId == new ClanId(2)), Is.False);
        Assert.That(socialState.ClanNarratives.Single(static narrative => narrative.ClanId == new ClanId(1)).FearPressure, Is.GreaterThan(0));
        Assert.That(context.Diff.Entries.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
        Assert.That(context.DomainEvents.Events.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
    }

    [Test]
    public void RunMonth_CourtPolicyLocalResponseResidue_ReadsStructuredOfficeResponseWithoutOrderMislabel()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(7),
            Prestige = 55,
            SupportReserve = 44,
            HeirPersonId = new PersonId(1),
        });
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Li",
            HomeSettlementId = new SettlementId(8),
            Prestige = 80,
            SupportReserve = 44,
            HeirPersonId = new PersonId(2),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        queries.Register<IOrderAndBanditryQueries>(new StubOrderQueries([]));
        queries.Register<IOfficeAndCareerQueries>(new StubOfficeQueries(
            [
                new JurisdictionAuthoritySnapshot
                {
                    SettlementId = new SettlementId(7),
                    LeadOfficialName = "Zhang Yuan",
                    LeadOfficeTitle = "Registrar",
                    AuthorityTier = 3,
                    JurisdictionLeverage = 32,
                    ClerkDependence = 42,
                    PetitionPressure = 52,
                    PetitionBacklog = 10,
                    AdministrativeTaskLoad = 58,
                    PetitionOutcomeCategory = "Triaged",
                    LastPetitionOutcome = "projection prose must not drive residue",
                    LastAdministrativeTrace = "政策回应入口 文移续接选择 公议降温只读回",
                    LastRefusalResponseCommandCode = PlayerCommandNames.PressCountyYamenDocument,
                    LastRefusalResponseCommandLabel = "押文催县门",
                    LastRefusalResponseSummary = "receipt prose must not drive residue",
                    LastRefusalResponseOutcomeCode = PublicLifeOrderResponseOutcomeCodes.Contained,
                    LastRefusalResponseTraceCode = PublicLifeOrderResponseTraceCodes.OfficeYamenLanded,
                    ResponseCarryoverMonths = 1,
                },
                new JurisdictionAuthoritySnapshot
                {
                    SettlementId = new SettlementId(8),
                    LeadOfficialName = "Li Wen",
                    LeadOfficeTitle = "Registrar",
                    AuthorityTier = 1,
                    JurisdictionLeverage = 80,
                    ClerkDependence = 4,
                    PetitionPressure = 4,
                    PetitionBacklog = 2,
                    AdministrativeTaskLoad = 6,
                    PetitionOutcomeCategory = "Queued",
                },
            ]));
        socialModule.RegisterQueries(socialState, queries);

        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        KernelState kernelState = KernelState.Create(1413);
        ModuleExecutionContext context = new(
            new GameDate(1200, 11),
            manifest,
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));

        const string expectedCauseKey = "office.policy_local_response.7.PressCountyYamenDocument.Contained.office.yamen_landed";
        Assert.That(
            socialState.Memories.Select(static memory => memory.CauseKey),
            Does.Contain(expectedCauseKey),
            string.Join(" | ", socialState.Memories.Select(static memory => memory.CauseKey)));
        MemoryRecordState residue = socialState.Memories.Single(memory => memory.CauseKey == expectedCauseKey);
        Assert.That(residue.SubjectClanId, Is.EqualTo(new ClanId(1)));
        Assert.That(residue.Kind, Is.EqualTo(SocialMemoryKinds.OfficePolicyLocalResponseResidue));
        Assert.That(residue.Type, Is.EqualTo(MemoryType.Debt));
        Assert.That(residue.Subtype, Is.EqualTo(MemorySubtype.ProtectionFavor));
        Assert.That(residue.Summary, Does.Contain("政策回应读回"));
        Assert.That(residue.Summary, Does.Contain("县门轻催"));
        Assert.That(residue.Summary, Does.Contain("OfficeAndCareer/PublicLifeAndRumor"));
        Assert.That(residue.Summary, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(residue.Summary, Does.Not.Contain("receipt prose must not drive residue"));
        Assert.That(residue.Summary, Does.Not.Contain("projection prose must not drive residue"));
        Assert.That(residue.Summary, Does.Not.Contain("政策回应入口"));
        Assert.That(residue.Summary, Does.Not.Contain("文移续接选择"));
        Assert.That(residue.Summary, Does.Not.Contain("公议降温只读回"));
        Assert.That(socialState.Memories.Any(static memory =>
            memory.CauseKey.StartsWith("order.public_life.response.OfficeAndCareer", StringComparison.Ordinal)), Is.False);
        Assert.That(socialState.Memories.Any(static memory =>
            memory.CauseKey.StartsWith("office.policy_local_response.8.", StringComparison.Ordinal)), Is.False);
        Assert.That(socialState.ClanNarratives.Single(static narrative => narrative.ClanId == new ClanId(1)).PublicNarrative, Does.Contain("政策回应余味"));
        Assert.That(context.Diff.Entries.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
        Assert.That(context.DomainEvents.Events.All(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations), Is.True);
    }

    [Test]
    public void PublishedEvents_ContainsPressureTemperingReceipts()
    {
        SocialMemoryAndRelationsModule socialModule = new();

        Assert.That(socialModule.PublishedEvents, Does.Contain(SocialMemoryAndRelationsEventNames.PressureTempered));
        Assert.That(socialModule.PublishedEvents, Does.Contain(SocialMemoryAndRelationsEventNames.EmotionalPressureShifted));
    }

    private sealed class EmptyPersonRegistryQueries : IPersonRegistryQueries
    {
        public bool TryGetPerson(PersonId id, out PersonRecord person)
        {
            person = null!;
            return false;
        }

        public IReadOnlyList<PersonRecord> GetAllPersons() => [];

        public IReadOnlyList<PersonRecord> GetPersonsByFidelityRing(FidelityRing ring) => [];

        public IReadOnlyList<PersonRecord> GetLivingPersons() => [];

        public bool IsAlive(PersonId id) => false;

        public int GetAgeMonths(PersonId id, GameDate currentDate) => -1;
    }

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }
    }

    private sealed class StubTradeQueries : ITradeAndIndustryQueries
    {
        private readonly IReadOnlyList<ClanTradeSnapshot> _clanTrades;

        public StubTradeQueries(IReadOnlyList<ClanTradeSnapshot> clanTrades)
        {
            _clanTrades = clanTrades;
        }

        public ClanTradeSnapshot GetRequiredClanTrade(ClanId clanId)
        {
            return _clanTrades.Single(trade => trade.ClanId == clanId);
        }

        public IReadOnlyList<ClanTradeSnapshot> GetClanTrades()
        {
            return _clanTrades;
        }

        public IReadOnlyList<MarketSnapshot> GetMarkets()
        {
            return [];
        }

        public IReadOnlyList<ClanTradeRouteSnapshot> GetRoutesForClan(ClanId clanId)
        {
            return [];
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
        private readonly IReadOnlyList<JurisdictionAuthoritySnapshot> _jurisdictions;

        public StubOfficeQueries(IReadOnlyList<JurisdictionAuthoritySnapshot> jurisdictions)
        {
            _jurisdictions = jurisdictions;
        }

        public OfficeCareerSnapshot GetRequiredCareer(PersonId personId)
        {
            throw new AssertionException("Career lookup should not be used in this test.");
        }

        public IReadOnlyList<OfficeCareerSnapshot> GetCareers()
        {
            return [];
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
}
