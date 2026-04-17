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
}
