using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;

namespace Zongzu.Persistence.Tests;

[TestFixture]
public sealed partial class SaveMigrationPipelineTests
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
    public void PrepareForLoad_RootMigrationChain_AppliesInRegisteredOrder()
    {
        SaveRoot saveRoot = new()
        {
            RootSchemaVersion = 0,
            CurrentDate = new GameDate(1200, 1),
            FeatureManifest = new FeatureManifest(),
            KernelState = KernelState.Create(1),
        };

        int[] appliedSteps = [];
        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterRootMigration(0, 1, root =>
        {
            appliedSteps = [.. appliedSteps, 1];
            return root;
        });
        pipeline.RegisterRootMigration(1, 3, root =>
        {
            appliedSteps = [.. appliedSteps, 3];
            return root;
        });

        SaveRoot migratedRoot = pipeline.PrepareForLoad(saveRoot, 3, Array.Empty<IModuleRunner>());

        Assert.That(appliedSteps, Is.EqualTo(new[] { 1, 3 }));
        Assert.That(migratedRoot.RootSchemaVersion, Is.EqualTo(3));
    }

    [Test]
    public void PrepareForLoadWithReport_RootAndModuleMigrations_ReportCombinedStepsAndSchemas()
    {
        const string testModuleKey = "Test.Migration";
        SaveRoot saveRoot = new()
        {
            RootSchemaVersion = 0,
            CurrentDate = new GameDate(1200, 1),
            FeatureManifest = new FeatureManifest(),
            KernelState = KernelState.Create(88),
            ModuleStates = new Dictionary<string, ModuleStateEnvelope>(StringComparer.Ordinal)
            {
                [testModuleKey] = new ModuleStateEnvelope
                {
                    ModuleKey = testModuleKey,
                    ModuleSchemaVersion = 0,
                    Payload = [4, 4, 4],
                },
            },
        };
        saveRoot.FeatureManifest.Set(testModuleKey, FeatureMode.Full);

        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterRootMigration(0, 1, static root => root);
        pipeline.RegisterModuleMigration(testModuleKey, 0, 2, static envelope => envelope);

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(saveRoot, 1, [new TestMigrationModuleRunner()]);

        Assert.That(result.Report.SourceRootSchemaVersion, Is.EqualTo(0));
        Assert.That(result.Report.PreparedRootSchemaVersion, Is.EqualTo(1));
        Assert.That(result.Report.AppliedStepCount, Is.EqualTo(2));
        Assert.That(result.Report.RootSteps, Has.Count.EqualTo(1));
        Assert.That(result.Report.ModuleSteps, Has.Count.EqualTo(1));
        Assert.That(result.Report.ConsistencyPassed, Is.True);
    }

    [Test]
    public void PrepareForLoadWithReport_ModuleMigrationChain_ReportsAppliedStepsInRegisteredOrder()
    {
        const string testModuleKey = "Test.Migration";
        SaveRoot saveRoot = new()
        {
            RootSchemaVersion = GameSimulation.RootSchemaVersion,
            CurrentDate = new GameDate(1200, 1),
            FeatureManifest = new FeatureManifest(),
            KernelState = KernelState.Create(44),
            ModuleStates = new Dictionary<string, ModuleStateEnvelope>(StringComparer.Ordinal)
            {
                [testModuleKey] = new ModuleStateEnvelope
                {
                    ModuleKey = testModuleKey,
                    ModuleSchemaVersion = 0,
                    Payload = [1, 2, 3],
                },
            },
        };
        saveRoot.FeatureManifest.Set(testModuleKey, FeatureMode.Full);

        int[] appliedSteps = [];
        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterModuleMigration(testModuleKey, 0, 1, envelope =>
        {
            appliedSteps = [.. appliedSteps, 1];
            return envelope;
        });
        pipeline.RegisterModuleMigration(testModuleKey, 1, 2, envelope =>
        {
            appliedSteps = [.. appliedSteps, 2];
            return envelope;
        });

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(
            saveRoot,
            GameSimulation.RootSchemaVersion,
            [new TestMigrationModuleRunner()]);

        Assert.That(appliedSteps, Is.EqualTo(new[] { 1, 2 }));
        Assert.That(result.SaveRoot.ModuleStates[testModuleKey].ModuleSchemaVersion, Is.EqualTo(2));
        Assert.That(result.Report.WasMigrationApplied, Is.True);
        Assert.That(result.Report.SourceEnabledModuleCount, Is.EqualTo(1));
        Assert.That(result.Report.PreparedEnabledModuleCount, Is.EqualTo(1));
        Assert.That(result.Report.EnabledModuleKeySetPreserved, Is.True);
        Assert.That(result.Report.ModuleStateKeySetPreserved, Is.True);
        Assert.That(result.Report.ConsistencyWarnings, Is.Empty);
        Assert.That(result.Report.AppliedStepCount, Is.EqualTo(2));
        Assert.That(result.Report.ConsistencyPassed, Is.True);
        Assert.That(
            result.Report.ModuleSteps.Select(static step => $"{step.ModuleKey}:{step.SourceVersion}->{step.TargetVersion}").ToArray(),
            Is.EqualTo(new[] { "Test.Migration:0->1", "Test.Migration:1->2" }));
    }

    [Test]
    public void PrepareForLoadWithReport_DoesNotMutateSourceSaveRoot()
    {
        const string testModuleKey = "Test.Migration";
        SaveRoot saveRoot = new()
        {
            RootSchemaVersion = GameSimulation.RootSchemaVersion,
            CurrentDate = new GameDate(1200, 1),
            FeatureManifest = new FeatureManifest(),
            KernelState = KernelState.Create(77),
            ModuleStates = new Dictionary<string, ModuleStateEnvelope>(StringComparer.Ordinal)
            {
                [testModuleKey] = new ModuleStateEnvelope
                {
                    ModuleKey = testModuleKey,
                    ModuleSchemaVersion = 0,
                    Payload = [5, 6, 7],
                },
            },
        };
        saveRoot.FeatureManifest.Set(testModuleKey, FeatureMode.Full);

        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterModuleMigration(testModuleKey, 0, 2, envelope =>
        {
            envelope.Payload = [9, 9, 9];
            return envelope;
        });

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(
            saveRoot,
            GameSimulation.RootSchemaVersion,
            [new TestMigrationModuleRunner()]);

        Assert.That(saveRoot.ModuleStates[testModuleKey].ModuleSchemaVersion, Is.EqualTo(0));
        Assert.That(saveRoot.ModuleStates[testModuleKey].Payload, Is.EqualTo(new byte[] { 5, 6, 7 }));
        Assert.That(result.SaveRoot.ModuleStates[testModuleKey].ModuleSchemaVersion, Is.EqualTo(2));
        Assert.That(result.SaveRoot.ModuleStates[testModuleKey].Payload, Is.EqualTo(new byte[] { 9, 9, 9 }));
    }

    [Test]
    public void PrepareForLoadWithReport_KeySetChangingMigration_EmitsConsistencyWarning()
    {
        const string testModuleKey = "Test.Migration";
        SaveRoot saveRoot = new()
        {
            RootSchemaVersion = 0,
            CurrentDate = new GameDate(1200, 1),
            FeatureManifest = new FeatureManifest(),
            KernelState = KernelState.Create(99),
            ModuleStates = new Dictionary<string, ModuleStateEnvelope>(StringComparer.Ordinal)
            {
                [testModuleKey] = new ModuleStateEnvelope
                {
                    ModuleKey = testModuleKey,
                    ModuleSchemaVersion = 2,
                    Payload = [1],
                },
            },
        };
        saveRoot.FeatureManifest.Set(testModuleKey, FeatureMode.Full);

        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterRootMigration(0, 1, root =>
        {
            root.FeatureManifest = new FeatureManifest();
            root.ModuleStates.Remove(testModuleKey);
            return root;
        });

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(saveRoot, 1, [new TestMigrationModuleRunner()]);

        Assert.That(result.Report.EnabledModuleKeySetPreserved, Is.False);
        Assert.That(result.Report.ModuleStateKeySetPreserved, Is.False);
        Assert.That(result.Report.ConsistencyPassed, Is.False);
        Assert.That(result.Report.ConsistencyWarnings, Has.Count.EqualTo(2));
    }

}

