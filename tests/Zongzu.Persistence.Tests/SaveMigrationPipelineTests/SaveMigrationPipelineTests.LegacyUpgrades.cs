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

public sealed partial class SaveMigrationPipelineTests
{
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
        // Phase 1c: WorldSettlements schema was raised 2鈫? (SPATIAL_SKELETON_SPEC
        // 搂13). Legacy v1 saves now chain through v1鈫抳2鈫抳3.
        Assert.That(
            reloaded.LoadMigrationReport!.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WorldSettlements
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.WorldSettlements].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(migratedState.Settlements, Is.Not.Empty);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.Tier == SettlementTier.CountySeat), Is.True);
        // v2鈫抳3 migration must populate the new NodeKind / Visibility / EcoZone
        // fields with sensible defaults (SPEC 搂13.2).
        Assert.That(migratedState.Settlements.All(static settlement => settlement.NodeKind != SettlementNodeKind.Unknown), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.Visibility == NodeVisibility.StateVisible), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.EcoZone == SettlementEcoZone.JiangnanWaterNetwork), Is.True);
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

}
