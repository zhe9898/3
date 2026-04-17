using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Persistence;

namespace Zongzu.Persistence.Tests;

[TestFixture]
public sealed class SaveCompatibilityGuardrailTests
{
    [Test]
    public void LoadM2_CanLoadLegacyM0M1SaveWhenNewModulesAreDisabled()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(20260420);
        simulation.AdvanceMonths(12);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM2(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.WorldSettlements,
            }));
    }

    [Test]
    public void LoadM2_ResetsRuntimeOnlyDebugTracesAfterSaveRoundtrip()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260421);
        simulation.AdvanceMonths(6);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeSave = builder.BuildForM2(simulation);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM2(codec.Decode(bytes));
        PresentationReadModelBundle afterLoad = builder.BuildForM2(reloaded);

        Assert.That(simulation.LastMonthResult, Is.Not.Null);
        Assert.That(beforeSave.Debug.RecentDiffEntries, Is.Not.Empty);
        Assert.That(beforeSave.Debug.RecentDomainEvents, Is.Not.Empty);
        Assert.That(reloaded.LastMonthResult, Is.Null);
        Assert.That(afterLoad.Debug.RecentDiffEntries, Is.Empty);
        Assert.That(afterLoad.Debug.RecentDomainEvents, Is.Empty);
        Assert.That(afterLoad.Debug.Warnings, Does.Contain("No monthly diff has been recorded yet."));
        Assert.That(afterLoad.Notifications, Is.Not.Empty);
    }

    [Test]
    public void LoadM2_RejectsNarrativeProjectionSchemaMismatch()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260422);
        simulation.AdvanceMonths(3);

        SaveRoot saveRoot = simulation.ExportSave();
        saveRoot.ModuleStates[KnownModuleKeys.NarrativeProjection].ModuleSchemaVersion = 999;

        SaveMigrationException exception = Assert.Throws<SaveMigrationException>(() => SimulationBootstrapper.LoadM2(saveRoot))!;
        Assert.That(exception.Message, Does.Contain(KnownModuleKeys.NarrativeProjection));
        Assert.That(exception.Message, Does.Contain("migration path").IgnoreCase);
        Assert.That(exception.Message, Does.Contain("schema").IgnoreCase);
    }

    [Test]
    public void LoadM2_RejectsRootSchemaMismatch()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260426);
        SaveRoot saveRoot = simulation.ExportSave();
        saveRoot.RootSchemaVersion = 999;

        SaveMigrationException exception = Assert.Throws<SaveMigrationException>(() => SimulationBootstrapper.LoadM2(saveRoot))!;
        Assert.That(exception.Message, Does.Contain("root schema"));
        Assert.That(exception.Message, Does.Contain("999"));
    }
}
