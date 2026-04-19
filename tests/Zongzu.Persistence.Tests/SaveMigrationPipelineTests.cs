using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;

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
    public void LoadM2_UsesRegisteredRootAndModuleMigrationsToLoadLegacySchemas()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260425);
        SaveRoot saveRoot = simulation.ExportSave();
        saveRoot.RootSchemaVersion = 0;
        saveRoot.ModuleStates[KnownModuleKeys.NarrativeProjection].ModuleSchemaVersion = 0;

        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterRootMigration(0, GameSimulation.RootSchemaVersion, static root => root);
        pipeline.RegisterModuleMigration(KnownModuleKeys.NarrativeProjection, 0, 1, static envelope => envelope);

        GameSimulation reloaded = SimulationBootstrapper.LoadM2(saveRoot, pipeline);

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(reloaded.ExportSave().RootSchemaVersion, Is.EqualTo(GameSimulation.RootSchemaVersion));
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.NarrativeProjection].ModuleSchemaVersion, Is.EqualTo(1));
    }

    [Test]
    public void LoadM2_DefaultMigrationPipeline_UpgradesLegacyFamilyCoreSchema()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260606);
        simulation.AdvanceMonths(4);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState currentState = (FamilyCoreState)serializer.Deserialize(
            typeof(FamilyCoreState),
            saveRoot.ModuleStates[KnownModuleKeys.FamilyCore].Payload);

        LegacyFamilyCoreStateV1 legacyState = new()
        {
            Clans = currentState.Clans.Select(static clan => new LegacyClanStateDataV1
            {
                Id = clan.Id,
                ClanName = clan.ClanName,
                HomeSettlementId = clan.HomeSettlementId,
                Prestige = clan.Prestige,
                SupportReserve = clan.SupportReserve,
                HeirPersonId = clan.HeirPersonId,
            }).ToList(),
            People = currentState.People.Select(static person => new LegacyFamilyPersonStateV1
            {
                Id = person.Id,
                ClanId = person.ClanId,
                GivenName = person.GivenName,
                AgeMonths = person.AgeMonths,
                IsAlive = person.IsAlive,
            }).ToList(),
        };

        saveRoot.ModuleStates[KnownModuleKeys.FamilyCore] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(LegacyFamilyCoreStateV1), legacyState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadM2(saveRoot);
        SaveRoot reloadedSave = reloaded.ExportSave();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(
            typeof(FamilyCoreState),
            reloadedSave.ModuleStates[KnownModuleKeys.FamilyCore].Payload);

        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(reloaded.LoadMigrationReport!.ConsistencyPassed, Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.FamilyCore
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.FamilyCore
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.FamilyCore].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(migratedState.Clans, Has.Count.EqualTo(currentState.Clans.Count));
        Assert.That(
            migratedState.Clans.All(static clan =>
                clan.BranchTension >= 0
                && clan.InheritancePressure >= 0
                && clan.SeparationPressure >= 0
                && clan.MediationMomentum >= 0
                && clan.BranchFavorPressure >= 0
                && clan.ReliefSanctionPressure >= 0
                && clan.MarriageAlliancePressure >= 0
                && clan.HeirSecurity >= 0
                && clan.ReproductivePressure >= 0
                && clan.MourningLoad >= 0),
            Is.True);
        Assert.That(migratedState.Clans.All(static clan => clan.LastConflictCommandCode is not null), Is.True);
        Assert.That(migratedState.Clans.All(static clan => clan.LastConflictTrace is not null), Is.True);
        Assert.That(migratedState.Clans.All(static clan => clan.LastLifecycleCommandCode is not null), Is.True);
        Assert.That(migratedState.Clans.All(static clan => clan.LastLifecycleTrace is not null), Is.True);
    }

    [Test]
    public void LoadM2_DefaultMigrationPipeline_UpgradesLegacyWorldSettlementsSchema()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260607);
        simulation.AdvanceMonths(2);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState currentState = (WorldSettlementsState)serializer.Deserialize(
            typeof(WorldSettlementsState),
            saveRoot.ModuleStates[KnownModuleKeys.WorldSettlements].Payload);

        foreach (SettlementStateData settlement in currentState.Settlements)
        {
            settlement.Tier = SettlementTier.Unknown;
        }

        saveRoot.ModuleStates[KnownModuleKeys.WorldSettlements] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), currentState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadM2(saveRoot);
        SaveRoot reloadedSave = reloaded.ExportSave();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(
            typeof(WorldSettlementsState),
            reloadedSave.ModuleStates[KnownModuleKeys.WorldSettlements].Payload);

        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(
            reloaded.LoadMigrationReport!.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WorldSettlements
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.WorldSettlements].ModuleSchemaVersion, Is.EqualTo(2));
        Assert.That(migratedState.Settlements, Is.Not.Empty);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.Tier == SettlementTier.CountySeat), Is.True);
    }

    [Test]
    public void PrepareForLoad_ModuleSchemaMismatch_ThrowsExplicitFailure()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260427);
        SaveRoot saveRoot = simulation.ExportSave();
        saveRoot.ModuleStates[KnownModuleKeys.NarrativeProjection].ModuleSchemaVersion = 999;

        SaveMigrationPipeline pipeline = new();

        SaveMigrationException exception = Assert.Throws<SaveMigrationException>(
            () => pipeline.PrepareForLoad(saveRoot, GameSimulation.RootSchemaVersion, SimulationBootstrapper.CreateM2Modules()))!;

        Assert.That(exception.Message, Does.Contain(KnownModuleKeys.NarrativeProjection));
        Assert.That(exception.Message, Does.Contain("999"));
        Assert.That(exception.Message, Does.Contain("schema"));
    }

    [Test]
    public void LoadM3LocalConflict_DefaultMigrationPipeline_UpgradesLegacyConflictForceSchema()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(20260430);
        simulation.AdvanceMonths(3);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState currentState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        foreach (SettlementForceState settlement in currentState.Settlements)
        {
            settlement.ResponseActivationLevel = 0;
            settlement.OrderSupportLevel = 0;
            settlement.IsResponseActivated = false;
            settlement.HasActiveConflict = false;
        }

        saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(ConflictAndForceState), currentState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadM3LocalConflict(saveRoot);
        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(reloaded.LoadMigrationReport!.ConsistencyWarnings, Is.Empty);
        Assert.That(reloaded.LoadMigrationReport.ModuleStateKeySetPreserved, Is.True);
        SaveRoot reloadedSave = reloaded.ExportSave();
        ConflictAndForceState migratedState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            reloadedSave.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.ConflictAndForce].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(migratedState.Settlements, Has.Count.EqualTo(currentState.Settlements.Count));
        Assert.That(migratedState.Settlements.Any(static settlement => settlement.ResponseActivationLevel > 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.CampaignFatigue == 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.CampaignEscortStrain == 0), Is.True);
    }

    [Test]
    public void LoadM3LocalConflict_MigratedLegacyStressSave_PreservesStructuralConflictStateAfterRun()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictStressBootstrap(20260501);
        simulation.AdvanceMonths(6);

        SaveCodec codec = new();
        MessagePackModuleStateSerializer serializer = new();
        SaveRoot currentSave = codec.Decode(codec.Encode(simulation.ExportSave()));
        SaveRoot legacySave = codec.Decode(codec.Encode(simulation.ExportSave()));

        ConflictAndForceState legacyConflictState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            legacySave.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);
        foreach (SettlementForceState settlement in legacyConflictState.Settlements)
        {
            settlement.ResponseActivationLevel = 0;
            settlement.OrderSupportLevel = 0;
            settlement.IsResponseActivated = false;
            settlement.HasActiveConflict = false;
        }

        legacySave.ModuleStates[KnownModuleKeys.ConflictAndForce] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(ConflictAndForceState), legacyConflictState),
        };

        GameSimulation currentReloaded = SimulationBootstrapper.LoadM3LocalConflict(currentSave);
        GameSimulation migratedReloaded = SimulationBootstrapper.LoadM3LocalConflict(legacySave);

        currentReloaded.AdvanceMonths(24);
        migratedReloaded.AdvanceMonths(24);

        ConflictAndForceState currentReloadedState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            currentReloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);
        ConflictAndForceState migratedReloadedState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            migratedReloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        Assert.That(migratedReloaded.CurrentDate, Is.EqualTo(currentReloaded.CurrentDate));
        Assert.That(
            migratedReloadedState.Settlements
                .OrderBy(static settlement => settlement.SettlementId.Value)
                .Select(static settlement => settlement.SettlementId.Value)
                .ToArray(),
            Is.EqualTo(
                currentReloadedState.Settlements
                    .OrderBy(static settlement => settlement.SettlementId.Value)
                    .Select(static settlement => settlement.SettlementId.Value)
                    .ToArray()));
        Assert.That(migratedReloadedState.Settlements, Has.Count.EqualTo(currentReloadedState.Settlements.Count));
        Assert.That(migratedReloadedState.Settlements.All(static settlement =>
            settlement.GuardCount >= 0
            && settlement.RetainerCount >= 0
            && settlement.MilitiaCount >= 0
            && settlement.EscortCount >= 0
            && settlement.Readiness is >= 0 and <= 100
            && settlement.CommandCapacity is >= 0 and <= 100
            && settlement.ResponseActivationLevel >= 0
            && settlement.OrderSupportLevel >= 0
            && settlement.CampaignFatigue >= 0
            && settlement.CampaignEscortStrain >= 0), Is.True);
        Assert.That(migratedReloadedState.Settlements.Any(static settlement =>
            settlement.ResponseActivationLevel > 0
            || settlement.OrderSupportLevel > 0
            || settlement.HasActiveConflict), Is.True);
        Assert.That(migratedReloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(migratedReloaded.LoadMigrationReport!.ConsistencyPassed, Is.True);
        Assert.That(migratedReloaded.LoadMigrationReport.ModuleSteps.Any(static step => step.ModuleKey == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 2 && step.TargetVersion == 3), Is.True);
    }

    [Test]
    public void LoadP3CampaignSandbox_DefaultMigrationPipeline_UpgradesLegacyConflictForceSchemaV2()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260518);
        simulation.AdvanceMonths(4);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState currentState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        LegacyConflictAndForceStateV2 legacyState = new()
        {
            Settlements = currentState.Settlements.Select(static settlement => new LegacySettlementForceStateV2
            {
                SettlementId = settlement.SettlementId,
                GuardCount = settlement.GuardCount,
                RetainerCount = settlement.RetainerCount,
                MilitiaCount = settlement.MilitiaCount,
                EscortCount = settlement.EscortCount,
                Readiness = settlement.Readiness,
                CommandCapacity = settlement.CommandCapacity,
                ResponseActivationLevel = settlement.ResponseActivationLevel,
                OrderSupportLevel = settlement.OrderSupportLevel,
                IsResponseActivated = settlement.IsResponseActivated,
                HasActiveConflict = settlement.HasActiveConflict,
                LastConflictTrace = settlement.LastConflictTrace,
            }).ToList(),
        };

        saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(LegacyConflictAndForceStateV2), legacyState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadP3CampaignSandbox(saveRoot);
        SaveRoot reloadedSave = reloaded.ExportSave();
        ConflictAndForceState migratedState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            reloadedSave.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(reloaded.LoadMigrationReport!.ModuleSteps.Any(static step => step.ModuleKey == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 2 && step.TargetVersion == 3), Is.True);
        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.ConflictAndForce].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(migratedState.Settlements, Has.Count.EqualTo(currentState.Settlements.Count));
        Assert.That(migratedState.Settlements.All(static settlement => settlement.CampaignFatigue == 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.CampaignEscortStrain == 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.LastCampaignFalloutTrace == string.Empty), Is.True);
    }

    [Test]
    public void LoadP1GovernanceLocalConflict_DefaultMigrationPipeline_UpgradesLegacyOfficeSchema()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260519);
        simulation.AdvanceMonths(6);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState currentState = (OfficeAndCareerState)serializer.Deserialize(
            typeof(OfficeAndCareerState),
            saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);

        LegacyOfficeAndCareerStateV1 legacyState = new()
        {
            People = currentState.People.Select(static career => new LegacyOfficeCareerStateV1
            {
                PersonId = career.PersonId,
                ClanId = career.ClanId,
                SettlementId = career.SettlementId,
                DisplayName = career.DisplayName,
                IsEligible = career.IsEligible,
                HasAppointment = career.HasAppointment,
                OfficeTitle = career.OfficeTitle,
                AuthorityTier = career.AuthorityTier,
                JurisdictionLeverage = career.JurisdictionLeverage,
                PetitionPressure = career.PetitionPressure,
                OfficeReputation = career.OfficeReputation,
                LastOutcome = career.LastOutcome,
                LastExplanation = career.LastExplanation,
            }).ToList(),
            Jurisdictions = currentState.Jurisdictions.Select(static jurisdiction => new LegacyJurisdictionAuthorityStateV1
            {
                SettlementId = jurisdiction.SettlementId,
                LeadOfficialPersonId = jurisdiction.LeadOfficialPersonId,
                LeadOfficialName = jurisdiction.LeadOfficialName,
                LeadOfficeTitle = jurisdiction.LeadOfficeTitle,
                AuthorityTier = jurisdiction.AuthorityTier,
                JurisdictionLeverage = jurisdiction.JurisdictionLeverage,
                PetitionPressure = jurisdiction.PetitionPressure,
                LastAdministrativeTrace = jurisdiction.LastAdministrativeTrace,
            }).ToList(),
        };

        saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(LegacyOfficeAndCareerStateV1), legacyState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadP1GovernanceLocalConflict(saveRoot);

        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(
            reloaded.LoadMigrationReport!.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OfficeAndCareer
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OfficeAndCareer
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
        Assert.That(reloaded.LoadMigrationReport.ConsistencyPassed, Is.True);

        SaveRoot reloadedSave = reloaded.ExportSave();
        OfficeAndCareerState migratedState = (OfficeAndCareerState)serializer.Deserialize(
            typeof(OfficeAndCareerState),
            reloadedSave.ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);

        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.OfficeAndCareer].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(migratedState.People.Any(static career => career.ServiceMonths > 0), Is.True);
        Assert.That(migratedState.People.Any(static career => !string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask)), Is.True);
        Assert.That(migratedState.People.Any(static career => !string.IsNullOrWhiteSpace(career.LastPetitionOutcome)), Is.True);
        Assert.That(migratedState.People.Any(static career => career.AppointmentPressure >= 0), Is.True);
    }

    [Test]
    public void LoadP1GovernanceLocalConflict_MigratedLegacyOfficeSave_MatchesReplayOfCurrentSchema()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260520);
        simulation.AdvanceMonths(6);

        SaveCodec codec = new();
        MessagePackModuleStateSerializer serializer = new();
        SaveRoot currentSave = codec.Decode(codec.Encode(simulation.ExportSave()));
        SaveRoot legacySave = codec.Decode(codec.Encode(simulation.ExportSave()));

        OfficeAndCareerState currentState = (OfficeAndCareerState)serializer.Deserialize(
            typeof(OfficeAndCareerState),
            legacySave.ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);
        LegacyOfficeAndCareerStateV1 legacyState = new()
        {
            People = currentState.People.Select(static career => new LegacyOfficeCareerStateV1
            {
                PersonId = career.PersonId,
                ClanId = career.ClanId,
                SettlementId = career.SettlementId,
                DisplayName = career.DisplayName,
                IsEligible = career.IsEligible,
                HasAppointment = career.HasAppointment,
                OfficeTitle = career.OfficeTitle,
                AuthorityTier = career.AuthorityTier,
                JurisdictionLeverage = career.JurisdictionLeverage,
                PetitionPressure = career.PetitionPressure,
                OfficeReputation = career.OfficeReputation,
                LastOutcome = career.LastOutcome,
                LastExplanation = career.LastExplanation,
            }).ToList(),
            Jurisdictions = currentState.Jurisdictions.Select(static jurisdiction => new LegacyJurisdictionAuthorityStateV1
            {
                SettlementId = jurisdiction.SettlementId,
                LeadOfficialPersonId = jurisdiction.LeadOfficialPersonId,
                LeadOfficialName = jurisdiction.LeadOfficialName,
                LeadOfficeTitle = jurisdiction.LeadOfficeTitle,
                AuthorityTier = jurisdiction.AuthorityTier,
                JurisdictionLeverage = jurisdiction.JurisdictionLeverage,
                PetitionPressure = jurisdiction.PetitionPressure,
                LastAdministrativeTrace = jurisdiction.LastAdministrativeTrace,
            }).ToList(),
        };

        legacySave.ModuleStates[KnownModuleKeys.OfficeAndCareer] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(LegacyOfficeAndCareerStateV1), legacyState),
        };

        GameSimulation currentReloaded = SimulationBootstrapper.LoadP1GovernanceLocalConflict(currentSave);
        GameSimulation migratedReloaded = SimulationBootstrapper.LoadP1GovernanceLocalConflict(legacySave);

        currentReloaded.AdvanceMonths(24);
        migratedReloaded.AdvanceMonths(24);

        Assert.That(migratedReloaded.CurrentDate, Is.EqualTo(currentReloaded.CurrentDate));
        Assert.That(migratedReloaded.ReplayHash, Is.Not.Empty);
        Assert.That(currentReloaded.ReplayHash, Is.Not.Empty);
        Assert.That(migratedReloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(migratedReloaded.LoadMigrationReport!.ConsistencyPassed, Is.True);
        Assert.That(
            migratedReloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OfficeAndCareer
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(
            migratedReloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OfficeAndCareer
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
        MessagePackModuleStateSerializer migratedSerializer = new();
        OfficeAndCareerState currentOfficeState = (OfficeAndCareerState)migratedSerializer.Deserialize(
            typeof(OfficeAndCareerState),
            currentReloaded.ExportSave().ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);
        OfficeAndCareerState migratedOfficeState = (OfficeAndCareerState)migratedSerializer.Deserialize(
            typeof(OfficeAndCareerState),
            migratedReloaded.ExportSave().ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);
        Assert.That(migratedOfficeState.People.Count, Is.EqualTo(currentOfficeState.People.Count));
        Assert.That(migratedOfficeState.People.Count(static career => career.HasAppointment), Is.EqualTo(currentOfficeState.People.Count(static career => career.HasAppointment)));
        Assert.That(migratedOfficeState.People.Any(static career => career.AppointmentPressure >= 0), Is.True);
    }

    [Test]
    public void LoadP3CampaignSandbox_DefaultMigrationPipeline_UpgradesLegacyWarfareSchema()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260525);
        simulation.AdvanceMonths(6);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState currentState = (WarfareCampaignState)serializer.Deserialize(
            typeof(WarfareCampaignState),
            saveRoot.ModuleStates[KnownModuleKeys.WarfareCampaign].Payload);

        LegacyWarfareCampaignStateV1 legacyState = CreateLegacyWarfareCampaignStateV1(currentState);
        saveRoot.ModuleStates[KnownModuleKeys.WarfareCampaign] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(LegacyWarfareCampaignStateV1), legacyState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadP3CampaignSandbox(saveRoot);

        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(
            reloaded.LoadMigrationReport!.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WarfareCampaign
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WarfareCampaign
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
        Assert.That(reloaded.LoadMigrationReport.ConsistencyPassed, Is.True);

        SaveRoot reloadedSave = reloaded.ExportSave();
        WarfareCampaignState migratedState = (WarfareCampaignState)serializer.Deserialize(
            typeof(WarfareCampaignState),
            reloadedSave.ModuleStates[KnownModuleKeys.WarfareCampaign].Payload);

        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.WarfareCampaign].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(migratedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.FrontLabel)), Is.True);
        Assert.That(migratedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommandFitLabel)), Is.True);
        Assert.That(migratedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommanderSummary)), Is.True);
        Assert.That(migratedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)), Is.True);
        Assert.That(migratedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace)), Is.True);
        Assert.That(migratedState.Campaigns.Any(static campaign => campaign.Routes.Count > 0), Is.True);
        Assert.That(migratedState.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);
        Assert.That(migratedState.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.ActiveDirectiveLabel)), Is.True);
    }

    [Test]
    public void LoadP3CampaignSandbox_MigratedLegacyWarfareSave_MatchesReplayOfCurrentSchema()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260526);
        simulation.AdvanceMonths(6);

        SaveCodec codec = new();
        MessagePackModuleStateSerializer serializer = new();
        SaveRoot currentSave = codec.Decode(codec.Encode(simulation.ExportSave()));
        SaveRoot legacySave = codec.Decode(codec.Encode(simulation.ExportSave()));

        WarfareCampaignState currentState = (WarfareCampaignState)serializer.Deserialize(
            typeof(WarfareCampaignState),
            legacySave.ModuleStates[KnownModuleKeys.WarfareCampaign].Payload);
        LegacyWarfareCampaignStateV1 legacyState = CreateLegacyWarfareCampaignStateV1(currentState);
        legacySave.ModuleStates[KnownModuleKeys.WarfareCampaign] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(LegacyWarfareCampaignStateV1), legacyState),
        };

        GameSimulation currentReloaded = SimulationBootstrapper.LoadP3CampaignSandbox(currentSave);
        GameSimulation migratedReloaded = SimulationBootstrapper.LoadP3CampaignSandbox(legacySave);

        currentReloaded.AdvanceMonths(24);
        migratedReloaded.AdvanceMonths(24);

        Assert.That(migratedReloaded.CurrentDate, Is.EqualTo(currentReloaded.CurrentDate));
        Assert.That(migratedReloaded.ReplayHash, Is.EqualTo(currentReloaded.ReplayHash));
        Assert.That(migratedReloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(migratedReloaded.LoadMigrationReport!.ConsistencyPassed, Is.True);
        Assert.That(
            migratedReloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WarfareCampaign
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(
            migratedReloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WarfareCampaign
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
    }

    [Test]
    public void PrepareForLoadWithReport_WarfareCampaignMigration_UsesOwnedModuleKey()
    {
        SaveRoot saveRoot = new()
        {
            RootSchemaVersion = GameSimulation.RootSchemaVersion,
            CurrentDate = new GameDate(1200, 1),
            FeatureManifest = new FeatureManifest(),
            KernelState = KernelState.Create(20260502),
            ModuleStates = new Dictionary<string, ModuleStateEnvelope>(StringComparer.Ordinal)
            {
                [KnownModuleKeys.WarfareCampaign] = new ModuleStateEnvelope
                {
                    ModuleKey = KnownModuleKeys.WarfareCampaign,
                    ModuleSchemaVersion = 1,
                    Payload = [7, 7, 7],
                },
                [KnownModuleKeys.OfficeAndCareer] = new ModuleStateEnvelope
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    ModuleSchemaVersion = 3,
                    Payload = [8, 8, 8],
                },
            },
        };
        saveRoot.FeatureManifest.Set(KnownModuleKeys.WarfareCampaign, FeatureMode.Lite);
        saveRoot.FeatureManifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);

        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 1, 2, static envelope => envelope);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 2, 3, static envelope => envelope);

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(
            saveRoot,
            GameSimulation.RootSchemaVersion,
            [
                new TestNamedModuleRunner(KnownModuleKeys.WarfareCampaign, 3),
                new TestNamedModuleRunner(KnownModuleKeys.OfficeAndCareer, 3),
            ]);

        Assert.That(
            result.SaveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.OfficeAndCareer,
                KnownModuleKeys.WarfareCampaign,
            }));
        Assert.That(result.SaveRoot.ModuleStates[KnownModuleKeys.WarfareCampaign].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(result.SaveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(
            result.Report.ModuleSteps.Select(static step => $"{step.ModuleKey}:{step.SourceVersion}->{step.TargetVersion}").ToArray(),
            Is.EqualTo(new[]
            {
                $"{KnownModuleKeys.WarfareCampaign}:1->2",
                $"{KnownModuleKeys.WarfareCampaign}:2->3",
            }));
        Assert.That(result.Report.EnabledModuleKeySetPreserved, Is.True);
        Assert.That(result.Report.ModuleStateKeySetPreserved, Is.True);
    }

    [Test]
    public void PrepareForLoadWithReport_BlackRoutePreflightMigrations_StayInsideOrderAndTradeNamespaces()
    {
        SaveRoot saveRoot = new()
        {
            RootSchemaVersion = GameSimulation.RootSchemaVersion,
            CurrentDate = new GameDate(1200, 1),
            FeatureManifest = new FeatureManifest(),
            KernelState = KernelState.Create(20260503),
            ModuleStates = new Dictionary<string, ModuleStateEnvelope>(StringComparer.Ordinal)
            {
                [KnownModuleKeys.OrderAndBanditry] = new ModuleStateEnvelope
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    ModuleSchemaVersion = 1,
                    Payload = [1, 2, 3],
                },
                [KnownModuleKeys.TradeAndIndustry] = new ModuleStateEnvelope
                {
                    ModuleKey = KnownModuleKeys.TradeAndIndustry,
                    ModuleSchemaVersion = 1,
                    Payload = [4, 5, 6],
                },
            },
        };
        saveRoot.FeatureManifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Full);
        saveRoot.FeatureManifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Full);

        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 1, 2, static envelope => envelope);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 1, 2, static envelope => envelope);

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(
            saveRoot,
            GameSimulation.RootSchemaVersion,
            [
                new TestNamedModuleRunner(KnownModuleKeys.OrderAndBanditry, 2),
                new TestNamedModuleRunner(KnownModuleKeys.TradeAndIndustry, 2),
            ]);

        Assert.That(
            result.SaveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.TradeAndIndustry,
            }));
        Assert.That(result.SaveRoot.ModuleStates.ContainsKey(KnownModuleKeys.WarfareCampaign), Is.False);
        Assert.That(
            result.Report.ModuleSteps.Select(static step => step.ModuleKey).OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.TradeAndIndustry,
            }));
        Assert.That(result.Report.EnabledModuleKeySetPreserved, Is.True);
        Assert.That(result.Report.ModuleStateKeySetPreserved, Is.True);
    }

    [Test]
    public void LoadM2_MigratesPublicLifeSchemaThreeToFour_AndKeepsCountyPublicLifeLoadable()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260620);
        simulation.AdvanceMonths(2);

        SaveRoot saveRoot = simulation.ExportSave();
        saveRoot.ModuleStates[KnownModuleKeys.PublicLifeAndRumor].ModuleSchemaVersion = 3;

        GameSimulation loaded = SimulationBootstrapper.LoadM2(saveRoot);
        loaded.AdvanceMonths(1);

        SaveRoot migratedSave = loaded.ExportSave();
        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(loaded);

        Assert.That(migratedSave.ModuleStates[KnownModuleKeys.PublicLifeAndRumor].ModuleSchemaVersion, Is.EqualTo(4));
        Assert.That(bundle.PublicLifeSettlements, Is.Not.Empty);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.ChannelSummary)), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.DominantVenueCode)), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => settlement.DocumentaryWeight is >= 0 and <= 100), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => settlement.VerificationCost is >= 0 and <= 100), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => settlement.MarketRumorFlow is >= 0 and <= 100), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => settlement.CourierRisk is >= 0 and <= 100), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.OfficialNoticeLine)), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.StreetTalkLine)), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.RoadReportLine)), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.PrefectureDispatchLine)), Is.True);
        Assert.That(bundle.PublicLifeSettlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.ContentionSummary)), Is.True);
    }

    private sealed class TestMigrationModuleState : IModuleStateDescriptor
    {
        public string ModuleKey => "Test.Migration";
    }

    private static LegacyWarfareCampaignStateV1 CreateLegacyWarfareCampaignStateV1(WarfareCampaignState currentState)
    {
        return new LegacyWarfareCampaignStateV1
        {
            Campaigns = currentState.Campaigns.Select(static campaign => new LegacyCampaignFrontStateV1
            {
                CampaignId = campaign.CampaignId,
                AnchorSettlementId = campaign.AnchorSettlementId,
                AnchorSettlementName = campaign.AnchorSettlementName,
                CampaignName = campaign.CampaignName,
                IsActive = campaign.IsActive,
                ObjectiveSummary = campaign.ObjectiveSummary,
                MobilizedForceCount = campaign.MobilizedForceCount,
                FrontPressure = campaign.FrontPressure,
                SupplyState = campaign.SupplyState,
                MoraleState = campaign.MoraleState,
                MobilizationWindowLabel = campaign.MobilizationWindowLabel,
                SupplyLineSummary = campaign.SupplyLineSummary,
                OfficeCoordinationTrace = campaign.OfficeCoordinationTrace,
                SourceTrace = campaign.SourceTrace,
                LastAftermathSummary = campaign.LastAftermathSummary,
            }).ToList(),
            MobilizationSignals = currentState.MobilizationSignals.Select(static signal => new LegacyCampaignMobilizationSignalStateV1
            {
                SettlementId = signal.SettlementId,
                SettlementName = signal.SettlementName,
                ResponseActivationLevel = signal.ResponseActivationLevel,
                CommandCapacity = signal.CommandCapacity,
                Readiness = signal.Readiness,
                AvailableForceCount = signal.AvailableForceCount,
                OrderSupportLevel = signal.OrderSupportLevel,
                OfficeAuthorityTier = signal.OfficeAuthorityTier,
                AdministrativeLeverage = signal.AdministrativeLeverage,
                PetitionBacklog = signal.PetitionBacklog,
                MobilizationWindowLabel = signal.MobilizationWindowLabel,
                OfficeCoordinationTrace = signal.OfficeCoordinationTrace,
                SourceTrace = signal.SourceTrace,
            }).ToList(),
        };
    }

    public sealed class LegacyOfficeAndCareerStateV1 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.OfficeAndCareer;

        public List<LegacyOfficeCareerStateV1> People { get; set; } = new();

        public List<LegacyJurisdictionAuthorityStateV1> Jurisdictions { get; set; } = new();
    }

    public sealed class LegacyFamilyCoreStateV1 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.FamilyCore;

        public List<LegacyClanStateDataV1> Clans { get; set; } = new();

        public List<LegacyFamilyPersonStateV1> People { get; set; } = new();
    }

    public sealed class LegacyConflictAndForceStateV2 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.ConflictAndForce;

        public List<LegacySettlementForceStateV2> Settlements { get; set; } = new();
    }

    public sealed class LegacyWarfareCampaignStateV1 : IModuleStateDescriptor
    {
        public string ModuleKey => KnownModuleKeys.WarfareCampaign;

        public List<LegacyCampaignFrontStateV1> Campaigns { get; set; } = new();

        public List<LegacyCampaignMobilizationSignalStateV1> MobilizationSignals { get; set; } = new();
    }

    public sealed class LegacyOfficeCareerStateV1
    {
        public PersonId PersonId { get; set; }

        public ClanId ClanId { get; set; }

        public SettlementId SettlementId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public bool IsEligible { get; set; }

        public bool HasAppointment { get; set; }

        public string OfficeTitle { get; set; } = "Unappointed";

        public int AuthorityTier { get; set; }

        public int JurisdictionLeverage { get; set; }

        public int PetitionPressure { get; set; }

        public int OfficeReputation { get; set; }

        public string LastOutcome { get; set; } = string.Empty;

        public string LastExplanation { get; set; } = string.Empty;
    }

    public sealed class LegacyClanStateDataV1
    {
        public ClanId Id { get; set; }

        public string ClanName { get; set; } = string.Empty;

        public SettlementId HomeSettlementId { get; set; }

        public int Prestige { get; set; }

        public int SupportReserve { get; set; }

        public PersonId? HeirPersonId { get; set; }
    }

    public sealed class LegacyFamilyPersonStateV1
    {
        public PersonId Id { get; set; }

        public ClanId ClanId { get; set; }

        public string GivenName { get; set; } = string.Empty;

        public int AgeMonths { get; set; }

        public bool IsAlive { get; set; }
    }

    public sealed class LegacyJurisdictionAuthorityStateV1
    {
        public SettlementId SettlementId { get; set; }

        public PersonId? LeadOfficialPersonId { get; set; }

        public string LeadOfficialName { get; set; } = string.Empty;

        public string LeadOfficeTitle { get; set; } = string.Empty;

        public int AuthorityTier { get; set; }

        public int JurisdictionLeverage { get; set; }

        public int PetitionPressure { get; set; }

        public string LastAdministrativeTrace { get; set; } = string.Empty;
    }

    public sealed class LegacySettlementForceStateV2
    {
        public SettlementId SettlementId { get; set; }

        public int GuardCount { get; set; }

        public int RetainerCount { get; set; }

        public int MilitiaCount { get; set; }

        public int EscortCount { get; set; }

        public int Readiness { get; set; }

        public int CommandCapacity { get; set; }

        public int ResponseActivationLevel { get; set; }

        public int OrderSupportLevel { get; set; }

        public bool IsResponseActivated { get; set; }

        public bool HasActiveConflict { get; set; }

        public string LastConflictTrace { get; set; } = string.Empty;
    }

    public sealed class LegacyCampaignFrontStateV1
    {
        public CampaignId CampaignId { get; set; }

        public SettlementId AnchorSettlementId { get; set; }

        public string AnchorSettlementName { get; set; } = string.Empty;

        public string CampaignName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string ObjectiveSummary { get; set; } = string.Empty;

        public int MobilizedForceCount { get; set; }

        public int FrontPressure { get; set; }

        public int SupplyState { get; set; }

        public int MoraleState { get; set; }

        public string MobilizationWindowLabel { get; set; } = string.Empty;

        public string SupplyLineSummary { get; set; } = string.Empty;

        public string OfficeCoordinationTrace { get; set; } = string.Empty;

        public string SourceTrace { get; set; } = string.Empty;

        public string LastAftermathSummary { get; set; } = string.Empty;
    }

    public sealed class LegacyCampaignMobilizationSignalStateV1
    {
        public SettlementId SettlementId { get; set; }

        public string SettlementName { get; set; } = string.Empty;

        public int ResponseActivationLevel { get; set; }

        public int CommandCapacity { get; set; }

        public int Readiness { get; set; }

        public int AvailableForceCount { get; set; }

        public int OrderSupportLevel { get; set; }

        public int OfficeAuthorityTier { get; set; }

        public int AdministrativeLeverage { get; set; }

        public int PetitionBacklog { get; set; }

        public string MobilizationWindowLabel { get; set; } = string.Empty;

        public string OfficeCoordinationTrace { get; set; } = string.Empty;

        public string SourceTrace { get; set; } = string.Empty;
    }

    private sealed class TestMigrationModuleRunner : ModuleRunner<TestMigrationModuleState>
    {
        public override string ModuleKey => "Test.Migration";

        public override int ModuleSchemaVersion => 2;

        public override SimulationPhase Phase => SimulationPhase.Prepare;

        public override int ExecutionOrder => 1;

        public override TestMigrationModuleState CreateInitialState()
        {
            return new TestMigrationModuleState();
        }

        public override void RunMonth(ModuleExecutionScope<TestMigrationModuleState> scope)
        {
        }
    }

    private sealed class TestNamedModuleState : IModuleStateDescriptor
    {
        public string ModuleKey { get; init; } = string.Empty;
    }

    private sealed class TestNamedModuleRunner : ModuleRunner<TestNamedModuleState>
    {
        private readonly string _moduleKey;
        private readonly int _moduleSchemaVersion;

        public TestNamedModuleRunner(string moduleKey, int moduleSchemaVersion)
        {
            _moduleKey = moduleKey;
            _moduleSchemaVersion = moduleSchemaVersion;
        }

        public override string ModuleKey => _moduleKey;

        public override int ModuleSchemaVersion => _moduleSchemaVersion;

        public override SimulationPhase Phase => SimulationPhase.Prepare;

        public override int ExecutionOrder => 1;

        public override TestNamedModuleState CreateInitialState()
        {
            return new TestNamedModuleState
            {
                ModuleKey = _moduleKey,
            };
        }

        public override void RunMonth(ModuleExecutionScope<TestNamedModuleState> scope)
        {
        }
    }
}
