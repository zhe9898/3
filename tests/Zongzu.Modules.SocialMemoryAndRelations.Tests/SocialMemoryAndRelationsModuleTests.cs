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
}
