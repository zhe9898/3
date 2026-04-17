using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;

namespace Zongzu.Persistence.Tests;

[TestFixture]
public sealed class SaveMigrationPipelineTests
{
    [Test]
    public void PrepareForLoad_SameVersionSave_PassesThroughCurrentSchemas()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260423);
        simulation.AdvanceMonths(4);

        SaveRoot saveRoot = simulation.ExportSave();
        SaveMigrationPipeline pipeline = new();

        SaveRoot migratedRoot = pipeline.PrepareForLoad(saveRoot, GameSimulation.RootSchemaVersion, SimulationBootstrapper.CreateM2Modules());

        Assert.That(migratedRoot.RootSchemaVersion, Is.EqualTo(GameSimulation.RootSchemaVersion));
        Assert.That(migratedRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(), Is.EqualTo(saveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray()));
        Assert.That(
            migratedRoot.ModuleStates.Values.Select(static envelope => envelope.ModuleSchemaVersion).OrderBy(static version => version).ToArray(),
            Is.EqualTo(saveRoot.ModuleStates.Values.Select(static envelope => envelope.ModuleSchemaVersion).OrderBy(static version => version).ToArray()));
    }

    [Test]
    public void PrepareForLoad_RootSchemaMismatch_ThrowsExplicitFailure()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260424);
        SaveRoot saveRoot = simulation.ExportSave();
        saveRoot.RootSchemaVersion = 999;

        SaveMigrationPipeline pipeline = new();

        SaveMigrationException exception = Assert.Throws<SaveMigrationException>(
            () => pipeline.PrepareForLoad(saveRoot, GameSimulation.RootSchemaVersion, SimulationBootstrapper.CreateM2Modules()))!;

        Assert.That(exception.Message, Does.Contain("root schema"));
        Assert.That(exception.Message, Does.Contain("999"));
        Assert.That(exception.Message, Does.Contain(GameSimulation.RootSchemaVersion.ToString()));
    }

    [Test]
    public void PrepareForLoad_ModuleSchemaMismatch_ThrowsExplicitFailure()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260425);
        SaveRoot saveRoot = simulation.ExportSave();
        saveRoot.ModuleStates[KnownModuleKeys.NarrativeProjection].ModuleSchemaVersion = 999;

        SaveMigrationPipeline pipeline = new();

        SaveMigrationException exception = Assert.Throws<SaveMigrationException>(
            () => pipeline.PrepareForLoad(saveRoot, GameSimulation.RootSchemaVersion, SimulationBootstrapper.CreateM2Modules()))!;

        Assert.That(exception.Message, Does.Contain(KnownModuleKeys.NarrativeProjection));
        Assert.That(exception.Message, Does.Contain("999"));
        Assert.That(exception.Message, Does.Contain("schema"));
    }
}
