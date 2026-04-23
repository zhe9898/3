using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.FamilyCore.Tests;

[TestFixture]
public sealed class FamilyCoreModuleTests
{
    [Test]
    public void RunXun_UpdatesNearFamilyPressureWithoutReadableOutput()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 44,
            Prosperity = 52,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 60,
            HeirPersonId = new PersonId(1),
            HeirSecurity = 32,
            MarriageAllianceValue = 20,
            MarriageAlliancePressure = 40,
            MourningLoad = 20,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 24 * 12,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<IPersonRegistryCommands>(new RecordingPersonRegistryCommands());

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(5)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Shangxun);

        familyModule.RunXun(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(familyState.People[0].AgeMonths, Is.EqualTo(24 * 12));
        Assert.That(familyState.Clans[0].SupportReserve, Is.EqualTo(59));
        Assert.That(familyState.Clans[0].MarriageAlliancePressure, Is.EqualTo(41));
        Assert.That(context.Diff.Entries, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_AgesPeopleAndRespondsToSettlementPressure()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 30,
            Prosperity = 40,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 60,
            HeirPersonId = new PersonId(1),
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 120,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<IPersonRegistryCommands>(new RecordingPersonRegistryCommands());

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(5)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(familyState.People[0].AgeMonths, Is.EqualTo(121));
        Assert.That(familyState.Clans[0].Prestige, Is.EqualTo(49));
        Assert.That(familyState.Clans[0].SupportReserve, Is.EqualTo(59));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(2));
        Assert.That(context.DomainEvents.Events, Has.Count.EqualTo(2));
    }

    [Test]
    public void HandleEvents_AppliesCampaignReputationAndSupportFalloutInsideClanState()
    {
        FamilyCoreModule module = new();
        FamilyCoreState state = module.CreateInitialState();
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 64,
            HeirPersonId = new PersonId(1),
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "兰溪军务沙盘",
                IsActive = true,
                MobilizedForceCount = 52,
                FrontPressure = 73,
                FrontLabel = "前线转紧",
                SupplyState = 38,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 45,
                MoraleStateLabel = "军心摇动",
                LastAftermathSummary = "兰溪宗房正为车马与丧葬支应所累。",
            },
        ]));

        ModuleExecutionContext context = new(
            new GameDate(1200, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(61)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state,
            context,
            [
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignMobilized, "Lanxi mobilized.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi entered aftermath review.", "1"),
            ]));

        Assert.That(state.Clans[0].Prestige, Is.LessThan(58));
        Assert.That(state.Clans[0].SupportReserve, Is.LessThan(64));
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("战后余波"));
        Assert.That(context.DomainEvents.Events.Single().EventType, Is.EqualTo("ClanPrestigeAdjusted"));
    }

    [Test]
    public void RunMonth_EmitsLineageDisputeEvent_WhenFamilyPressureCrossesThreshold()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 42,
            Prosperity = 48,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 56,
            SupportReserve = 50,
            HeirPersonId = new PersonId(1),
            BranchTension = 54,
            InheritancePressure = 58,
            SeparationPressure = 52,
            BranchFavorPressure = 45,
            ReliefSanctionPressure = 22,
            MediationMomentum = 8,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<IPersonRegistryCommands>(new RecordingPersonRegistryCommands());

        ModuleExecutionContext context = new(
            new GameDate(1200, 2),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(17)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(familyState.Clans[0].BranchTension, Is.GreaterThanOrEqualTo(55));
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.LineageDisputeHardened), Is.True);
        Assert.That(context.Diff.Entries.Any(static entry => entry.Description.Contains("房支争力", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void RunMonth_ArrangesMarriage_WhenAlliancePressureRunsHigh()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 55,
            Prosperity = 62,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 52,
            SupportReserve = 66,
            MarriageAlliancePressure = 78,
            MarriageAllianceValue = 10,
            ReproductivePressure = 28,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 24 * 12,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        RecordingPersonRegistryCommands registryCommands = new();
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries(registryCommands));
        queries.Register<IPersonRegistryCommands>(registryCommands);

        ModuleExecutionContext context = new(
            new GameDate(1200, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(23)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(familyState.Clans[0].MarriageAllianceValue, Is.GreaterThanOrEqualTo(48));
        Assert.That(familyState.Clans[0].LastLifecycleOutcome, Is.Not.Empty);
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.MarriageAllianceArranged), Is.True);
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.BirthRegistered), Is.False);
        Assert.That(registryCommands.RegisteredIds, Has.Count.EqualTo(1));
        PersonId spouseId = registryCommands.RegisteredIds[0];
        FamilyPersonState anchor = familyState.People.Single(static person => person.Id == new PersonId(1));
        FamilyPersonState spouse = familyState.People.Single(person => person.Id == spouseId);
        Assert.That(anchor.SpouseId, Is.EqualTo(spouseId));
        Assert.That(spouse.SpouseId, Is.EqualTo(anchor.Id));
        Assert.That(registryCommands.Records[spouseId].Gender, Is.EqualTo(PersonGender.Female));
    }

    [Test]
    public void RunMonth_RegistersDeath_AndWeakensHeirSecurity_WhenLifecycleThresholdsAreMet()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 52,
            HeirPersonId = new PersonId(1),
            HeirSecurity = 62,
            MarriageAllianceValue = 72,
            ReproductivePressure = 74,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Lao",
            AgeMonths = 72 * 12,
            IsAlive = true,
            // STEP2A / A1：老死走累积 FragilityLedger，不再是 72 岁悬崖。
            // 预置到顶让本月立即老死，保留原测试的语义。
            FragilityLedger = 100,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(2),
            ClanId = new ClanId(1),
            GivenName = "Zhang Er",
            AgeMonths = 23 * 12,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        RecordingPersonRegistryCommands registryCommands = new();
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries(registryCommands));
        queries.Register<IPersonRegistryCommands>(registryCommands);

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(31)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        IFamilyCoreQueries familyQueries = queries.GetRequired<IFamilyCoreQueries>();
        IReadOnlyList<FamilyPersonSnapshot> clanMembers = familyQueries.GetClanMembers(new ClanId(1));
        Assert.That(clanMembers.Count(static person => person.IsAlive), Is.EqualTo(1));
        Assert.That(familyState.Clans[0].MourningLoad, Is.GreaterThan(0));
        Assert.That(familyState.Clans[0].FuneralDebt, Is.EqualTo(18));
        // STEP2A / A3：死一个 heir 立即递补（近 primogeniture），HeirSecurity 不再
        // 人为凹陷——由 Zhang Er 承祧。原断言（HeirSecurity<62 + HeirSecurityWeakened）
        // 预设 heir 空缺一月，与承祧链通电语义冲突，替换为立即补位断言。
        Assert.That(familyState.Clans[0].HeirPersonId, Is.EqualTo(new PersonId(2)));
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.ClanMemberDied), Is.True);
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.HeirSuccessionOccurred), Is.True);
        Assert.That(registryCommands.DeceasedIds, Has.Count.EqualTo(1));
        Assert.That(registryCommands.DeceasedIds[0], Is.EqualTo(new PersonId(1)));
    }

    [Test]
    public void RunMonth_HeirDeathShock_IsHeavier_WhenNoAdultSuccessor()
    {
        (FamilyCoreState withSuccessorState, RecordingPersonRegistryCommands withSuccessorRegistry, FamilyCoreModule withSuccessorModule, ModuleExecutionContext withSuccessorContext) = BuildHeirDeathShockScenario(hasAdultSuccessor: true);
        (FamilyCoreState noSuccessorState, RecordingPersonRegistryCommands noSuccessorRegistry, FamilyCoreModule noSuccessorModule, ModuleExecutionContext noSuccessorContext) = BuildHeirDeathShockScenario(hasAdultSuccessor: false);

        withSuccessorModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(withSuccessorState, withSuccessorContext));
        noSuccessorModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(noSuccessorState, noSuccessorContext));

        ClanStateData withSuccessorClan = withSuccessorState.Clans.Single();
        ClanStateData noSuccessorClan = noSuccessorState.Clans.Single();

        Assert.That(withSuccessorRegistry.DeceasedIds, Is.EqualTo(new[] { new PersonId(1) }));
        Assert.That(noSuccessorRegistry.DeceasedIds, Is.EqualTo(new[] { new PersonId(1) }));
        Assert.That(withSuccessorClan.HeirPersonId, Is.EqualTo(new PersonId(2)));
        Assert.That(noSuccessorClan.HeirPersonId, Is.Null);
        Assert.That(withSuccessorClan.LastLifecycleTrace, Does.Contain("承祧缺口1阶"));
        Assert.That(noSuccessorClan.LastLifecycleTrace, Does.Contain("承祧缺口3阶"));
        Assert.That(noSuccessorClan.InheritancePressure, Is.GreaterThan(withSuccessorClan.InheritancePressure));
        Assert.That(noSuccessorClan.SeparationPressure, Is.GreaterThan(withSuccessorClan.SeparationPressure));
        Assert.That(noSuccessorClan.BranchTension, Is.GreaterThan(withSuccessorClan.BranchTension));
        Assert.That(noSuccessorClan.MarriageAlliancePressure, Is.GreaterThan(withSuccessorClan.MarriageAlliancePressure));
        Assert.That(noSuccessorContext.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.HeirSuccessionOccurred), Is.False);
        Assert.That(withSuccessorContext.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.HeirSuccessionOccurred), Is.True);
    }

    [Test]
    public void HandleEvents_DeathByViolence_ProjectsSameHeirDeathPressureBands()
    {
        (FamilyCoreState withSuccessorState, FamilyCoreModule withSuccessorModule, ModuleExecutionContext withSuccessorContext) = BuildViolentHeirDeathScenario(hasAdultSuccessor: true);
        (FamilyCoreState noSuccessorState, FamilyCoreModule noSuccessorModule, ModuleExecutionContext noSuccessorContext) = BuildViolentHeirDeathScenario(hasAdultSuccessor: false);

        DomainEventRecord violentDeath = new(
            KnownModuleKeys.ConflictAndForce,
            DeathCauseEventNames.DeathByViolence,
            "Zhang heir died in a violent clash.",
            "1");

        withSuccessorModule.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            withSuccessorState,
            withSuccessorContext,
            [violentDeath]));
        noSuccessorModule.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            noSuccessorState,
            noSuccessorContext,
            [violentDeath]));

        ClanStateData withSuccessorClan = withSuccessorState.Clans.Single();
        ClanStateData noSuccessorClan = noSuccessorState.Clans.Single();

        Assert.That(withSuccessorClan.HeirPersonId, Is.Null);
        Assert.That(noSuccessorClan.HeirPersonId, Is.Null);
        Assert.That(withSuccessorClan.LastLifecycleTrace, Does.Contain("承祧缺口1阶"));
        Assert.That(noSuccessorClan.LastLifecycleTrace, Does.Contain("承祧缺口3阶"));
        Assert.That(noSuccessorClan.InheritancePressure, Is.GreaterThan(withSuccessorClan.InheritancePressure));
        Assert.That(noSuccessorClan.SeparationPressure, Is.GreaterThan(withSuccessorClan.SeparationPressure));
        Assert.That(noSuccessorClan.BranchTension, Is.GreaterThan(withSuccessorClan.BranchTension));
        Assert.That(noSuccessorClan.MarriageAlliancePressure, Is.GreaterThan(withSuccessorClan.MarriageAlliancePressure));
        Assert.That(withSuccessorContext.Diff.Entries.Single().EntityKey, Is.EqualTo("1"));
        Assert.That(withSuccessorContext.Diff.Entries.Single().Description, Does.Contain("暴亡"));
        Assert.That(
            withSuccessorContext.DomainEvents.Events.Any(static evt =>
                evt.EventType == FamilyCoreEventNames.ClanMemberDied
                || evt.EventType == DeathCauseEventNames.DeathByIllness),
            Is.False);
    }

    [Test]
    public void RunMonth_InfantDeath_EmitsDeathByIllness_AndRaisesReproductivePressure()
    {
        // STEP2A / A5 — 婴幼儿夭折走 DeathByIllness 分流；MourningLoad 上涨；
        // ReproductivePressure 因再育焦虑同步上涨；成人 heir 不被此事波及。
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
            HealerAccess = HealerAccess.None,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 52,
            HeirPersonId = new PersonId(10),
            HeirSecurity = 62,
            MarriageAllianceValue = 72,
            ReproductivePressure = 30,
            MourningLoad = 0,
        });
        // 成人 heir 不应被新路径影响。
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(10),
            ClanId = new ClanId(1),
            GivenName = "Zhang Lang",
            AgeMonths = 30 * 12,
            IsAlive = true,
        });
        // 婴儿 ledger 预置到顶 → 本月由 TryResolveClanDeath 选中。
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(11),
            ClanId = new ClanId(1),
            GivenName = "Zhang Wa",
            AgeMonths = 6,
            IsAlive = true,
            FragilityLedger = 100,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        RecordingPersonRegistryCommands registryCommands = new();
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries(registryCommands));
        queries.Register<IPersonRegistryCommands>(registryCommands);

        ModuleExecutionContext context = new(
            new GameDate(1200, 12),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(41)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        int reproductiveBefore = familyState.Clans[0].ReproductivePressure;
        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        // 发射的是 DeathByIllness（婴幼儿分流），而不是 ClanMemberDied。
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == DeathCauseEventNames.DeathByIllness), Is.True);
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.ClanMemberDied), Is.False);
        // EntityKey 为 PersonId，便于 PersonRegistry consolidate。
        IDomainEvent illnessEvent = context.DomainEvents.Events.Single(static evt => evt.EventType == DeathCauseEventNames.DeathByIllness);
        Assert.That(illnessEvent.EntityKey, Is.EqualTo("11"));
        // 死亡写回走 PersonRegistry.MarkDeceased（权威链未变）。
        Assert.That(registryCommands.DeceasedIds, Has.Count.EqualTo(1));
        Assert.That(registryCommands.DeceasedIds[0], Is.EqualTo(new PersonId(11)));
        // heir 仍在（成人未被新路径误伤）。
        Assert.That(familyState.Clans[0].HeirPersonId, Is.EqualTo(new PersonId(10)));
        // 副作用：MourningLoad / ReproductivePressure 均涨。
        Assert.That(familyState.Clans[0].MourningLoad, Is.GreaterThan(0));
        Assert.That(familyState.Clans[0].ReproductivePressure, Is.GreaterThan(reproductiveBefore));
        Assert.That(familyState.Clans[0].FuneralDebt, Is.EqualTo(6));
    }

    private static (FamilyCoreState State, RecordingPersonRegistryCommands Registry, FamilyCoreModule Module, ModuleExecutionContext Context) BuildHeirDeathShockScenario(bool hasAdultSuccessor)
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 55,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 35,
            HeirPersonId = new PersonId(1),
            HeirSecurity = 62,
            MarriageAllianceValue = 20,
            MarriageAlliancePressure = 18,
            ReproductivePressure = 20,
            InheritancePressure = 10,
            SeparationPressure = 10,
            BranchTension = 10,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Heir",
            AgeMonths = 40 * 12,
            IsAlive = true,
            FragilityLedger = 100,
        });

        if (hasAdultSuccessor)
        {
            familyState.People.Add(new FamilyPersonState
            {
                Id = new PersonId(2),
                ClanId = new ClanId(1),
                GivenName = "Zhang Successor",
                AgeMonths = 24 * 12,
                IsAlive = true,
            });
        }
        else
        {
            familyState.People.Add(new FamilyPersonState
            {
                Id = new PersonId(3),
                ClanId = new ClanId(1),
                GivenName = "Zhang Child",
                AgeMonths = 8 * 12,
                IsAlive = true,
            });
        }

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        RecordingPersonRegistryCommands registryCommands = new();
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries(registryCommands));
        queries.Register<IPersonRegistryCommands>(registryCommands);

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(hasAdultSuccessor ? 71 : 72)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        return (familyState, registryCommands, familyModule, context);
    }

    private static (FamilyCoreState State, FamilyCoreModule Module, ModuleExecutionContext Context) BuildViolentHeirDeathScenario(bool hasAdultSuccessor)
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 35,
            HeirPersonId = new PersonId(1),
            HeirSecurity = 62,
            MarriageAllianceValue = 20,
            MarriageAlliancePressure = 18,
            ReproductivePressure = 20,
            InheritancePressure = 10,
            SeparationPressure = 10,
            BranchTension = 10,
        });
        FamilyPersonState heir = new()
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Heir",
            AgeMonths = 40 * 12,
            IsAlive = true,
            BranchPosition = BranchPosition.MainLineHeir,
        };
        familyState.People.Add(heir);

        if (hasAdultSuccessor)
        {
            familyState.People.Add(new FamilyPersonState
            {
                Id = new PersonId(2),
                ClanId = new ClanId(1),
                GivenName = "Zhang Successor",
                AgeMonths = 24 * 12,
                IsAlive = true,
            });
        }
        else
        {
            PersonId childId = new(3);
            heir.ChildrenIds.Add(childId);
            familyState.People.Add(new FamilyPersonState
            {
                Id = childId,
                ClanId = new ClanId(1),
                GivenName = "Zhang Child",
                AgeMonths = 8 * 12,
                IsAlive = true,
                FatherId = heir.Id,
            });
        }

        RecordingPersonRegistryCommands registryCommands = new();
        registryCommands.Seed(new PersonRecord
        {
            Id = new PersonId(1),
            DisplayName = "Zhang Heir",
            BirthDate = new GameDate(1160, 1),
            Gender = PersonGender.Male,
            FidelityRing = FidelityRing.Local,
            IsAlive = false,
            LifeStage = LifeStage.Deceased,
        });
        registryCommands.Seed(new PersonRecord
        {
            Id = hasAdultSuccessor ? new PersonId(2) : new PersonId(3),
            DisplayName = hasAdultSuccessor ? "Zhang Successor" : "Zhang Child",
            BirthDate = hasAdultSuccessor ? new GameDate(1176, 1) : new GameDate(1192, 1),
            Gender = PersonGender.Male,
            FidelityRing = FidelityRing.Local,
            IsAlive = true,
            LifeStage = hasAdultSuccessor ? LifeStage.Adult : LifeStage.Child,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries(registryCommands));

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(hasAdultSuccessor ? 81 : 82)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        return (familyState, familyModule, context);
    }

    [Test]
    public void RunMonth_RegistersBirth_WhenMarriageAndReproductivePressureAlign()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 52,
            MarriageAllianceValue = 72,
            ReproductivePressure = 74,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Da",
            AgeMonths = 28 * 12,
            IsAlive = true,
            SpouseId = new PersonId(2),
        });
        // STEP2A / A6 — 生育 gate 需要"已婚成年夫妇"；补一位在世配偶。
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(2),
            ClanId = new ClanId(1),
            GivenName = "Zhang Shi",
            AgeMonths = 26 * 12,
            IsAlive = true,
            SpouseId = new PersonId(1),
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        RecordingPersonRegistryCommands registryCommands = new();
        queries.Register<IPersonRegistryCommands>(registryCommands);

        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(41)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        // 2 名 seed 成年夫妇 + 新生儿。
        Assert.That(familyState.People.Count(static person => person.IsAlive), Is.EqualTo(3));
        Assert.That(familyState.Clans[0].ReproductivePressure, Is.LessThan(74));
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.BirthRegistered), Is.True);
        // Phase 2b.1 收口: FamilyCore births must flow through PersonRegistry.Register
        // so identity ownership stays in one place (PERSON_OWNERSHIP_RULES.md §249).
        Assert.That(registryCommands.RegisteredIds, Has.Count.EqualTo(1));
        PersonId registeredNewbornId = registryCommands.RegisteredIds[0];
        Assert.That(familyState.People.Any(person => person.Id == registeredNewbornId && person.ClanId.Value == 1), Is.True);
        FamilyPersonState newborn = familyState.People.Single(person => person.Id == registeredNewbornId);
        Assert.That(newborn.FatherId, Is.EqualTo(new PersonId(1)));
        Assert.That(newborn.MotherId, Is.EqualTo(new PersonId(2)));
        Assert.That(familyState.People.Single(static person => person.Id == new PersonId(1)).ChildrenIds, Does.Contain(registeredNewbornId));
        Assert.That(familyState.People.Single(static person => person.Id == new PersonId(2)).ChildrenIds, Does.Contain(registeredNewbornId));
        Assert.That(familyState.Clans[0].CareLoad, Is.EqualTo(8));
    }

    [Test]
    public void RunMonth_RegistersBirth_ForMarriedCouple_EvenWhenAllianceValueAndReproductivePressureAreSeedZero()
    {
        // STEP2A / A6 — 生育链解卡：MarriageAllianceValue / ReproductivePressure
        // 在 seed 阶段为 0，原 gate 合取 ≥55 / ≥52 永不触发。新 gate 改以
        // "已婚成年夫妇 × 无近 12 月婴儿 × SupportReserve 可撑 × MourningLoad 未塌陷"
        // 为前提。本测试验证：即使两字段均为 0，只要夫妇成立，就应出新生儿。
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 60,
            MarriageAllianceValue = 0,
            ReproductivePressure = 0,
            MourningLoad = 0,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Da",
            AgeMonths = 28 * 12,
            IsAlive = true,
            SpouseId = new PersonId(2),
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(2),
            ClanId = new ClanId(1),
            GivenName = "Zhang Shi",
            AgeMonths = 26 * 12,
            IsAlive = true,
            SpouseId = new PersonId(1),
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        RecordingPersonRegistryCommands registryCommands = new();
        queries.Register<IPersonRegistryCommands>(registryCommands);

        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(41)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.BirthRegistered), Is.True);
        Assert.That(registryCommands.RegisteredIds, Has.Count.EqualTo(1));
        Assert.That(familyState.People.Count(static person => person.IsAlive), Is.EqualTo(3));
    }

    [Test]
    public void RunMonth_DoesNotRegisterBirth_WhenNoMarriedCouple()
    {
        // STEP2A / A6 — 单个未婚成年不应生子。婚议是生育的结构前置。
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 60,
            MarriageAllianceValue = 72,
            ReproductivePressure = 74,
            MourningLoad = 0,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Du",
            AgeMonths = 28 * 12,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        RecordingPersonRegistryCommands registryCommands = new();
        queries.Register<IPersonRegistryCommands>(registryCommands);

        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(41)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.BirthRegistered), Is.False);
        Assert.That(registryCommands.RegisteredIds, Is.Empty);
    }

    [Test]
    public void RunMonth_EmitsCameOfAge_WhenYouthCrossesAdultThreshold()
    {
        // STEP2A / A7 — Youth → Adult 跨阈发 CameOfAge。本测试让本地镜像
        // 人员 AgeMonths = 16*12 - 1，RunMonth 自增 1 后恰为 AdultAgeMonths。
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 50,
        });
        // 16 周岁前夜，本月将跨阈。
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(42),
            ClanId = new ClanId(1),
            GivenName = "Zhang Guan",
            AgeMonths = (16 * 12) - 1,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<IPersonRegistryCommands>(new RecordingPersonRegistryCommands());

        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        IDomainEvent cameOfAge = context.DomainEvents.Events.Single(static evt => evt.EventType == FamilyCoreEventNames.CameOfAge);
        Assert.That(cameOfAge.EntityKey, Is.EqualTo("42"));
        Assert.That(familyState.People.Single(p => p.Id.Value == 42).AgeMonths, Is.EqualTo(16 * 12));
    }

    [Test]
    public void RunMonth_AdultMaleCrowding_RaisesSeparationPressure_WhenNoMediation()
    {
        // STEP2A / A7 — ≥3 成年男 + MediationMomentum<55 → SeparationPressure 每月 +1。
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 50,
            BranchTension = 0,
            MediationMomentum = 0,
            SeparationPressure = 10,
        });
        for (int i = 1; i <= 3; i++)
        {
            familyState.People.Add(new FamilyPersonState
            {
                Id = new PersonId(i),
                ClanId = new ClanId(1),
                GivenName = $"Zhang Xiong {i}",
                AgeMonths = (20 + i) * 12,
                IsAlive = true,
            });
        }

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<IPersonRegistryCommands>(new RecordingPersonRegistryCommands());

        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        int before = familyState.Clans[0].SeparationPressure;
        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));
        int after = familyState.Clans[0].SeparationPressure;

        // 基础 delta（BranchTension=0, ReliefSanction=0, Mourning=0, Mediation=0）= 0；
        // A7 拥挤加成 +1。
        Assert.That(after - before, Is.EqualTo(1));
    }

    [Test]
    public void RunMonth_AdultMaleCrowding_DoesNotRaiseSeparation_WhenMediationActive()
    {
        // STEP2A / A7 — 若 MediationMomentum ≥ 55（族老已开分房议），拥挤加成
        // 归零。同时基础 delta 中 Mediation>=55 还会再 -1。
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 57,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 50,
            BranchTension = 0,
            MediationMomentum = 60,
            SeparationPressure = 20,
        });
        for (int i = 1; i <= 3; i++)
        {
            familyState.People.Add(new FamilyPersonState
            {
                Id = new PersonId(i),
                ClanId = new ClanId(1),
                GivenName = $"Zhang Xiong {i}",
                AgeMonths = (20 + i) * 12,
                IsAlive = true,
            });
        }

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<IPersonRegistryCommands>(new RecordingPersonRegistryCommands());

        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        int before = familyState.Clans[0].SeparationPressure;
        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));
        int after = familyState.Clans[0].SeparationPressure;

        // 基础 delta -1（Mediation>=55）+ A7 加成 0（Mediation 已在场）= -1。
        Assert.That(after - before, Is.EqualTo(-1));
    }

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }
    }
    private sealed class EmptyPersonRegistryQueries : IPersonRegistryQueries
    {
        private readonly RecordingPersonRegistryCommands? _commands;

        public EmptyPersonRegistryQueries()
        {
        }

        public EmptyPersonRegistryQueries(RecordingPersonRegistryCommands commands)
        {
            _commands = commands;
        }

        public bool TryGetPerson(PersonId id, out PersonRecord person)
        {
            if (_commands is not null && _commands.Records.TryGetValue(id, out PersonRecord? record))
            {
                person = record;
                return true;
            }

            person = null!;
            return false;
        }

        public IReadOnlyList<PersonRecord> GetAllPersons() =>
            _commands is null ? [] : [.. _commands.Records.Values];

        public IReadOnlyList<PersonRecord> GetPersonsByFidelityRing(FidelityRing ring) =>
            _commands is null ? [] : _commands.Records.Values.Where(r => r.FidelityRing == ring).ToArray();

        public IReadOnlyList<PersonRecord> GetLivingPersons() =>
            _commands is null ? [] : _commands.Records.Values.Where(static r => r.IsAlive).ToArray();

        public bool IsAlive(PersonId id) =>
            _commands is not null
            && _commands.Records.TryGetValue(id, out PersonRecord? record)
            && record.IsAlive;

        public int GetAgeMonths(PersonId id, GameDate currentDate) => -1;
    }

    private sealed class RecordingPersonRegistryCommands : IPersonRegistryCommands
    {
        public List<PersonId> RegisteredIds { get; } = new();

        public List<PersonId> DeceasedIds { get; } = new();

        public Dictionary<PersonId, PersonRecord> Records { get; } = new();

        public bool Register(
            ModuleExecutionContext context,
            PersonId id,
            string displayName,
            GameDate birthDate,
            PersonGender gender,
            FidelityRing fidelityRing)
        {
            RegisteredIds.Add(id);
            Records[id] = new PersonRecord
            {
                Id = id,
                DisplayName = displayName,
                BirthDate = birthDate,
                Gender = gender,
                FidelityRing = fidelityRing,
                IsAlive = true,
                LifeStage = LifeStage.Infant,
            };
            return true;
        }

        public bool MarkDeceased(ModuleExecutionContext context, PersonId id)
        {
            DeceasedIds.Add(id);
            if (!Records.TryGetValue(id, out PersonRecord? record))
            {
                record = new PersonRecord { Id = id };
                Records[id] = record;
            }
            record.IsAlive = false;
            record.LifeStage = LifeStage.Deceased;
            return true;
        }

        public void Seed(PersonRecord record)
        {
            Records[record.Id] = record;
        }
    }
}
