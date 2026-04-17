using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class M0M1IntegrationTests
{
    [Test]
    public void BootstrapWorld_IsDeterministicAcrossTwelveMonths()
    {
        GameSimulation firstSimulation = SimulationBootstrapper.CreateM0M1Bootstrap(88);
        GameSimulation secondSimulation = SimulationBootstrapper.CreateM0M1Bootstrap(88);

        firstSimulation.AdvanceMonths(12);
        secondSimulation.AdvanceMonths(12);

        Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));
        Assert.That(secondSimulation.ExportSave().CurrentDate, Is.EqualTo(firstSimulation.ExportSave().CurrentDate));
    }

    [Test]
    public void ExportedSave_PassesModuleBoundaryInvariantChecks()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(99);
        simulation.AdvanceMonths(3);

        SaveRoot saveRoot = simulation.ExportSave();

        Assert.That(
            () => ModuleBoundaryValidator.Validate(simulation.Modules, simulation.FeatureManifest, saveRoot),
            Throws.Nothing);
        Assert.That(
            saveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.WorldSettlements,
            }));
        Assert.That(saveRoot.ModuleStates.All(static pair => string.Equals(pair.Key, pair.Value.ModuleKey, StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void DisabledModules_RemainAbsentFromSaveState()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(77),
            manifest,
            SimulationBootstrapper.CreateM0M1Modules());

        SaveRoot saveRoot = simulation.ExportSave();

        Assert.That(saveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(), Is.EqualTo(new[] { KnownModuleKeys.FamilyCore, KnownModuleKeys.WorldSettlements }));
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.PopulationAndHouseholds), Is.False);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.SocialMemoryAndRelations), Is.False);
    }
}
