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
}
