using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.PersonRegistry;

namespace Zongzu.Modules.PersonRegistryTests;

[TestFixture]
public sealed class PersonRegistryModuleTests
{
    [Test]
    public void LifeStage_Resolves_By_AgeMonths()
    {
        // Infant <2y, Child <12y, Youth <18y, Adult <55y, Elder >=55y
        Assert.That(PersonRegistryModule.ResolveLifeStage(0), Is.EqualTo(LifeStage.Infant));
        Assert.That(PersonRegistryModule.ResolveLifeStage(23), Is.EqualTo(LifeStage.Infant));
        Assert.That(PersonRegistryModule.ResolveLifeStage(24), Is.EqualTo(LifeStage.Child));
        Assert.That(PersonRegistryModule.ResolveLifeStage(11 * 12 + 11), Is.EqualTo(LifeStage.Child));
        Assert.That(PersonRegistryModule.ResolveLifeStage(12 * 12), Is.EqualTo(LifeStage.Youth));
        Assert.That(PersonRegistryModule.ResolveLifeStage(17 * 12 + 11), Is.EqualTo(LifeStage.Youth));
        Assert.That(PersonRegistryModule.ResolveLifeStage(18 * 12), Is.EqualTo(LifeStage.Adult));
        Assert.That(PersonRegistryModule.ResolveLifeStage(54 * 12 + 11), Is.EqualTo(LifeStage.Adult));
        Assert.That(PersonRegistryModule.ResolveLifeStage(55 * 12), Is.EqualTo(LifeStage.Elder));
    }

    [Test]
    public void ComputeAgeMonths_Is_Monotonic_And_Nonnegative()
    {
        GameDate birth = new(1200, 3);
        Assert.That(PersonRegistryModule.ComputeAgeMonths(birth, new GameDate(1200, 3)), Is.EqualTo(0));
        Assert.That(PersonRegistryModule.ComputeAgeMonths(birth, new GameDate(1200, 4)), Is.EqualTo(1));
        Assert.That(PersonRegistryModule.ComputeAgeMonths(birth, new GameDate(1201, 3)), Is.EqualTo(12));
        Assert.That(PersonRegistryModule.ComputeAgeMonths(birth, new GameDate(1199, 1)), Is.EqualTo(0));
    }

    [Test]
    public void RunMonth_Advances_LifeStage_For_Living_Persons_Only()
    {
        PersonRegistryModule module = new();
        PersonRegistryState state = module.CreateInitialState();
        state.Persons.Add(new PersonRecord
        {
            Id = new PersonId(1),
            DisplayName = "张远",
            BirthDate = new GameDate(1188, 1),
            LifeStage = LifeStage.Youth,
            IsAlive = true,
        });
        state.Persons.Add(new PersonRecord
        {
            Id = new PersonId(2),
            DisplayName = "张老",
            BirthDate = new GameDate(1100, 1),
            LifeStage = LifeStage.Deceased,
            IsAlive = false,
        });

        ModuleExecutionContext context = NewContext(new GameDate(1220, 1));
        module.RunMonth(new ModuleExecutionScope<PersonRegistryState>(state, context));

        Assert.That(state.Persons[0].LifeStage, Is.EqualTo(LifeStage.Adult), "living person age-progressed");
        Assert.That(state.Persons[1].LifeStage, Is.EqualTo(LifeStage.Deceased), "dead person untouched");
    }

    [Test]
    public void HandleEvents_Consolidates_ClanMemberDied_Into_PersonDeceased()
    {
        PersonRegistryModule module = new();
        PersonRegistryState state = module.CreateInitialState();
        state.Persons.Add(new PersonRecord
        {
            Id = new PersonId(7),
            DisplayName = "张远",
            BirthDate = new GameDate(1150, 1),
            LifeStage = LifeStage.Elder,
            IsAlive = true,
        });

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = NewContext(new GameDate(1222, 6), buffer);
        IDomainEvent[] incoming =
        [
            new DomainEventRecord(
                KnownModuleKeys.FamilyCore,
                DeathCauseEventNames.ClanMemberDied,
                "张门内举哀。",
                entityKey: "7"),
        ];

        module.HandleEvents(new ModuleEventHandlingScope<PersonRegistryState>(state, context, incoming));

        PersonRecord target = state.Persons.Single();
        Assert.That(target.IsAlive, Is.False);
        Assert.That(target.LifeStage, Is.EqualTo(LifeStage.Deceased));
        Assert.That(
            buffer.Events.Any(e => e.EventType == PersonRegistryEventNames.PersonDeceased && e.EntityKey == "7"),
            Is.True,
            "PersonRegistry must emit canonical PersonDeceased");
    }

    [Test]
    public void HandleEvents_Ignores_Unrelated_Events_And_Missing_Persons()
    {
        PersonRegistryModule module = new();
        PersonRegistryState state = module.CreateInitialState();
        state.Persons.Add(new PersonRecord { Id = new PersonId(1), IsAlive = true });

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = NewContext(new GameDate(1220, 1), buffer);
        IDomainEvent[] incoming =
        [
            new DomainEventRecord(KnownModuleKeys.FamilyCore, FamilyCoreEventNames.BirthRegistered, "添丁", "1"),
            new DomainEventRecord(KnownModuleKeys.FamilyCore, DeathCauseEventNames.ClanMemberDied, "不明人身故", "999"),
            new DomainEventRecord(KnownModuleKeys.FamilyCore, DeathCauseEventNames.ClanMemberDied, "无键死", null),
        ];

        module.HandleEvents(new ModuleEventHandlingScope<PersonRegistryState>(state, context, incoming));

        Assert.That(state.Persons[0].IsAlive, Is.True);
        Assert.That(buffer.Events.Any(e => e.EventType == PersonRegistryEventNames.PersonDeceased), Is.False);
    }

    [Test]
    public void Queries_Expose_Living_And_Fidelity_Views()
    {
        PersonRegistryModule module = new();
        PersonRegistryState state = module.CreateInitialState();
        state.Persons.AddRange(new[]
        {
            new PersonRecord { Id = new PersonId(1), IsAlive = true, FidelityRing = FidelityRing.Core },
            new PersonRecord { Id = new PersonId(2), IsAlive = true, FidelityRing = FidelityRing.Local },
            new PersonRecord { Id = new PersonId(3), IsAlive = false, FidelityRing = FidelityRing.Local },
        });

        QueryRegistry registry = new();
        module.RegisterQueries(state, registry);
        IPersonRegistryQueries queries = registry.GetRequired<IPersonRegistryQueries>();

        Assert.That(queries.GetAllPersons().Count, Is.EqualTo(3));
        Assert.That(queries.GetLivingPersons().Count, Is.EqualTo(2));
        Assert.That(queries.GetPersonsByFidelityRing(FidelityRing.Local).Count, Is.EqualTo(2));
        Assert.That(queries.TryGetPerson(new PersonId(1), out PersonRecord found), Is.True);
        Assert.That(found.FidelityRing, Is.EqualTo(FidelityRing.Core));
    }

    private static ModuleExecutionContext NewContext(GameDate date, DomainEventBuffer? buffer = null)
    {
        KernelState kernelState = KernelState.Create(42L);
        return new ModuleExecutionContext(
            date,
            new FeatureManifest(),
            new DeterministicRandom(kernelState),
            new QueryRegistry(),
            buffer ?? new DomainEventBuffer(),
            new WorldDiff(),
            kernelState: kernelState,
            SimulationCadenceBand.Month,
            SimulationXun.None);
    }
}
