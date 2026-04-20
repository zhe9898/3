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

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(31)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(familyState.People.Count(static person => person.IsAlive), Is.EqualTo(1));
        Assert.That(familyState.Clans[0].MourningLoad, Is.GreaterThan(0));
        Assert.That(familyState.Clans[0].HeirSecurity, Is.LessThan(62));
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.ClanMemberDied), Is.True);
        Assert.That(context.DomainEvents.Events.Any(static evt => evt.EventType == FamilyCoreEventNames.HeirSecurityWeakened), Is.True);
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
}
