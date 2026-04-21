using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Persistence;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

/// <summary>
/// Phase 1b canary: the Kernel-layer PersonRegistry must consolidate a
/// cause-specific death event from FamilyCore (ClanMemberDied) into the
/// canonical PersonDeceased signal, and must keep the person's identity
/// record in sync (IsAlive=false, LifeStage=Deceased).
///
/// This is the minimum cross-module integration that Phase 1b promised:
/// domain module emits -> Kernel registry consolidates -> projection layer
/// reads canonical PersonDeceased.
/// </summary>
[TestFixture]
public sealed class PersonRegistryIntegrationTests
{
    [Test]
    public void M0M1Bootstrap_IncludesPersonRegistryState_WithHeirSeeded()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(20260512);

        SaveRoot saveRoot = simulation.ExportSave();

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.PersonRegistry), Is.True);
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PersonRegistry), Is.True);
    }

    [Test]
    public void ClanMemberDied_IsConsolidated_Into_PersonDeceased_ByPersonRegistry()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(20260513);

        // Force the heir onto the edge of FamilyCore's death threshold and
        // let the next month-end elder check turn that into ClanMemberDied.
        // FamilyCoreModule uses DeathAgeMonths = 72 * 12 and picks the oldest
        // living clan member. PersonRegistry is authoritative for age since
        // Phase 2b, so we backdate the registry BirthDate (and keep the local
        // FamilyCore mirror aligned as documentation for the test).
        FamilyCoreState familyState = GetModuleState<FamilyCoreState>(simulation, KnownModuleKeys.FamilyCore);
        // Step 2-A / A2: the seed now includes cross-generation kin (elder /
        // spouse / youth / child) around the heir, so pick the main-line heir
        // explicitly rather than relying on a single-person clan.
        FamilyPersonState heir = familyState.People.Single(p => p.BranchPosition == BranchPosition.MainLineHeir);
        heir.AgeMonths = (72 * 12) + 1;

        PersonRegistryState registrySeed = GetModuleState<PersonRegistryState>(simulation, KnownModuleKeys.PersonRegistry);
        PersonRecord heirRecord = registrySeed.Persons.Single(p => p.Id.Equals(heir.Id));
        // Back-date birth so that ComputeAgeMonths(birth, CurrentDate) > DeathAgeMonths
        // at the next month tick.
        heirRecord.BirthDate = new GameDate(simulation.CurrentDate.Year - 73, simulation.CurrentDate.Month);

        simulation.AdvanceOneMonth();

        // Month result should contain both the cause-specific ClanMemberDied
        // AND the consolidated PersonDeceased — that's the whole contract.
        SimulationMonthResult result = simulation.LastMonthResult!;
        IDomainEvent? clanMemberDied = result.DomainEvents.SingleOrDefault(
            e => e.EventType == DeathCauseEventNames.ClanMemberDied);
        IDomainEvent? personDeceased = result.DomainEvents.SingleOrDefault(
            e => e.EventType == PersonRegistryEventNames.PersonDeceased);

        Assert.That(clanMemberDied, Is.Not.Null, "FamilyCore must emit ClanMemberDied");
        Assert.That(personDeceased, Is.Not.Null, "PersonRegistry must consolidate into PersonDeceased");
        Assert.That(clanMemberDied!.EntityKey, Is.EqualTo(heir.Id.Value.ToString()),
            "ClanMemberDied entity key must be PersonId so PersonRegistry can consolidate");
        Assert.That(personDeceased!.EntityKey, Is.EqualTo(heir.Id.Value.ToString()));
        Assert.That(personDeceased.ModuleKey, Is.EqualTo(KnownModuleKeys.PersonRegistry));

        // PersonRegistry's canonical record must now show the person as dead.
        PersonRegistryState registryState = GetModuleState<PersonRegistryState>(simulation, KnownModuleKeys.PersonRegistry);
        PersonRecord registryRecord = registryState.Persons.Single(p => p.Id.Equals(heir.Id));
        Assert.That(registryRecord.IsAlive, Is.False);
        Assert.That(registryRecord.LifeStage, Is.EqualTo(LifeStage.Deceased));
    }

    private static TState GetModuleState<TState>(GameSimulation simulation, string moduleKey)
        where TState : class
    {
        return simulation.GetModuleStateForTesting<TState>(moduleKey);
    }
}
