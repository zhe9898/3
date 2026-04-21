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
        queries.Register<IPersonRegistryQueries>(new EmptyPersonRegistryQueries());
        queries.Register<IPersonRegistryCommands>(new RecordingPersonRegistryCommands());

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

        Assert.That(familyState.People.Count(static person => person.IsAlive), Is.EqualTo(2));
        Assert.That(familyState.Clans[0].ReproductivePressure, Is.LessThan(74));
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.BirthRegistered), Is.True);
        // Phase 2b.1 收口: FamilyCore births must flow through PersonRegistry.Register
        // so identity ownership stays in one place (PERSON_OWNERSHIP_RULES.md §249).
        Assert.That(registryCommands.RegisteredIds, Has.Count.EqualTo(1));
        PersonId registeredNewbornId = registryCommands.RegisteredIds[0];
        Assert.That(familyState.People.Any(person => person.Id == registeredNewbornId && person.ClanId.Value == 1), Is.True);
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
