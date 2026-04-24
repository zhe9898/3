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
    public void M0M1Bootstrap_BuildsPersonDossiers_WithSeededHeirFamilyContext()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(20260514);
        FamilyCoreState familyState = GetModuleState<FamilyCoreState>(simulation, KnownModuleKeys.FamilyCore);
        FamilyPersonState heir = familyState.People.Single(p => p.BranchPosition == BranchPosition.MainLineHeir);
        PersonRegistryState registryState = GetModuleState<PersonRegistryState>(simulation, KnownModuleKeys.PersonRegistry);
        PersonRecord heirRecord = registryState.Persons.Single(p => p.Id == heir.Id);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PersonDossierSnapshot dossier = bundle.PersonDossiers.Single(dossier => dossier.PersonId == heir.Id);

        Assert.That(dossier.DisplayName, Is.EqualTo(heirRecord.DisplayName));
        Assert.That(dossier.ClanId, Is.EqualTo(heir.ClanId));
        Assert.That(dossier.BranchPositionLabel, Is.EqualTo("Main-line heir"));
        Assert.That(dossier.KinshipSummary, Does.Contain("children"));
        Assert.That(dossier.TemperamentSummary, Does.Contain("ambition"));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.PersonRegistry));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.FamilyCore));
    }

    [Test]
    public void M0M1Bootstrap_AfterMonth_IncludesSocialMemoryTemperingInPersonDossier()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(20260515);
        FamilyCoreState familyState = GetModuleState<FamilyCoreState>(simulation, KnownModuleKeys.FamilyCore);
        PersonId heirId = familyState.People.Single(p => p.BranchPosition == BranchPosition.MainLineHeir).Id;

        simulation.AdvanceOneMonth();
        PersonPressureTemperingSnapshot? tempering = simulation
            .GetQueryForTesting<ISocialMemoryAndRelationsQueries>()
            .FindPersonTempering(heirId);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PersonDossierSnapshot dossier = bundle.PersonDossiers.Single(dossier => dossier.PersonId == heirId);

        Assert.That(tempering, Is.Not.Null);
        Assert.That(dossier.MemoryPressureSummary, Does.Contain("pressure"));
        Assert.That(dossier.MemoryPressureSummary, Does.Contain(tempering!.LastPressureScore.ToString()));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.SocialMemoryAndRelations));
    }

    [Test]
    public void RegistryOnlyBootstrap_BuildsDossier_WhenOptionalFamilyAndSocialMemoryModulesAreMissing()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(20260516),
            manifest,
            [new PersonRegistryModule()]);
        PersonRegistryState registryState = GetModuleState<PersonRegistryState>(simulation, KnownModuleKeys.PersonRegistry);
        registryState.Persons.Add(new PersonRecord
        {
            Id = new PersonId(99),
            DisplayName = "Registry Only",
            BirthDate = new GameDate(1180, 1),
            Gender = PersonGender.Unspecified,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Local,
        });

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PersonDossierSnapshot dossier = bundle.PersonDossiers.Single();

        Assert.That(dossier.PersonId, Is.EqualTo(new PersonId(99)));
        Assert.That(dossier.DisplayName, Is.EqualTo("Registry Only"));
        Assert.That(dossier.ClanId, Is.Null);
        Assert.That(dossier.KinshipSummary, Is.EqualTo("No clan kinship projection."));
        Assert.That(dossier.MemoryPressureSummary, Is.EqualTo("No social-memory pressure projection."));
        Assert.That(dossier.SourceModuleKeys, Is.EqualTo(new[] { KnownModuleKeys.PersonRegistry }));
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
        // STEP2A / A1：老死走累积 FragilityLedger，不再是 72 岁悬崖。
        // 预置到顶让本月立即老死，保留原"死后发 ClanMemberDied→PersonDeceased"契约测试。
        heir.FragilityLedger = 100;

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
