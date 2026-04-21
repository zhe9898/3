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

        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.OfficeAndCareer].ModuleSchemaVersion, Is.EqualTo(4));
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
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WarfareCampaign
                && step.SourceVersion == 3
                && step.TargetVersion == 4),
            Is.True);
        Assert.That(reloaded.LoadMigrationReport.ConsistencyPassed, Is.True);

        SaveRoot reloadedSave = reloaded.ExportSave();
        WarfareCampaignState migratedState = (WarfareCampaignState)serializer.Deserialize(
            typeof(WarfareCampaignState),
            reloadedSave.ModuleStates[KnownModuleKeys.WarfareCampaign].Payload);

        Assert.That(reloadedSave.ModuleStates[KnownModuleKeys.WarfareCampaign].ModuleSchemaVersion, Is.EqualTo(4));
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
        Assert.That(
            migratedReloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.WarfareCampaign
                && step.SourceVersion == 3
                && step.TargetVersion == 4),
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
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 3, 4, static envelope => envelope);

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(
            saveRoot,
            GameSimulation.RootSchemaVersion,
            [
                new TestNamedModuleRunner(KnownModuleKeys.WarfareCampaign, 4),
                new TestNamedModuleRunner(KnownModuleKeys.OfficeAndCareer, 3),
            ]);

        Assert.That(
            result.SaveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.OfficeAndCareer,
                KnownModuleKeys.WarfareCampaign,
            }));
        Assert.That(result.SaveRoot.ModuleStates[KnownModuleKeys.WarfareCampaign].ModuleSchemaVersion, Is.EqualTo(4));
        Assert.That(result.SaveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(
            result.Report.ModuleSteps.Select(static step => $"{step.ModuleKey}:{step.SourceVersion}->{step.TargetVersion}").ToArray(),
            Is.EqualTo(new[]
            {
                $"{KnownModuleKeys.WarfareCampaign}:1->2",
                $"{KnownModuleKeys.WarfareCampaign}:2->3",
                $"{KnownModuleKeys.WarfareCampaign}:3->4",
            }));
        Assert.That(result.Report.EnabledModuleKeySetPreserved, Is.True);
        Assert.That(result.Report.ModuleStateKeySetPreserved, Is.True);
    }

}
