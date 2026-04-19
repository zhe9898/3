using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.PopulationAndHouseholds.Tests;

[TestFixture]
public sealed class PopulationAndHouseholdsModuleTests
{
    [Test]
    public void RunXun_UpdatesLivelihoodPressureWithoutReadableOutput()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 38,
            Prosperity = 43,
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
            SupportReserve = 40,
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
            Distress = 58,
            DebtPressure = 60,
            LaborCapacity = 55,
            MigrationRisk = 20,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(11)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Shangxun);

        populationModule.RunXun(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        PopulationHouseholdState household = populationState.Households[0];
        Assert.That(household.Distress, Is.EqualTo(59));
        Assert.That(household.DebtPressure, Is.EqualTo(60));
        Assert.That(populationState.Settlements, Has.Count.EqualTo(1));
        Assert.That(populationState.Settlements[0].CommonerDistress, Is.EqualTo(59));
        Assert.That(context.Diff.Entries, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_KeepsHouseholdPressuresInBoundsAndRebuildsSettlementSummary()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 38,
            Prosperity = 43,
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
            SupportReserve = 65,
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
            Distress = 50,
            DebtPressure = 49,
            LaborCapacity = 55,
            MigrationRisk = 20,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(11)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        PopulationHouseholdState household = populationState.Households[0];
        Assert.That(household.Distress, Is.InRange(0, 100));
        Assert.That(household.DebtPressure, Is.InRange(0, 100));
        Assert.That(household.LaborCapacity, Is.InRange(0, 100));
        Assert.That(household.MigrationRisk, Is.InRange(0, 100));
        Assert.That(populationState.Settlements, Has.Count.EqualTo(1));
        Assert.That(populationState.Settlements[0].LaborSupply, Is.EqualTo(household.LaborCapacity));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void HandleEvents_AppliesCampaignBurdenToHouseholdLivelihood()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = module.CreateInitialState();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 52,
            DebtPressure = 48,
            LaborCapacity = 60,
            MigrationRisk = 42,
        });
        state.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 52,
            LaborSupply = 60,
            MigrationPressure = 42,
            MilitiaPotential = 34,
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
                MobilizedForceCount = 48,
                FrontPressure = 75,
                FrontLabel = "前线转紧",
                SupplyState = 36,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 44,
                MoraleStateLabel = "军心摇动",
                LastAftermathSummary = "还乡兵丁与败车残辎壅在路上。",
            },
        ]));

        ModuleExecutionContext context = new(
            new GameDate(1200, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(41)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state,
            context,
            [
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignPressureRaised, "Lanxi pressure rose.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi entered aftermath review.", "1"),
            ]));

        PopulationHouseholdState household = state.Households[0];
        PopulationSettlementState settlement = state.Settlements[0];

        Assert.That(household.Distress, Is.GreaterThan(52));
        Assert.That(household.DebtPressure, Is.GreaterThan(48));
        Assert.That(household.MigrationRisk, Is.GreaterThan(42));
        Assert.That(household.LaborCapacity, Is.LessThan(60));
        Assert.That(settlement.CommonerDistress, Is.EqualTo(household.Distress));
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("战后余波"));
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
