using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.FamilyCore.Tests;

[TestFixture]
public sealed class FamilyCoreModuleTests
{
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
}
