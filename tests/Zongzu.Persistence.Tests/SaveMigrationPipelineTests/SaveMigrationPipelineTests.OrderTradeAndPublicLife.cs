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
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 2, 3, static envelope => envelope);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 3, 4, static envelope => envelope);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 4, 5, static envelope => envelope);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 5, 6, static envelope => envelope);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 1, 2, static envelope => envelope);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 2, 3, static envelope => envelope);

        SavePreparationResult result = pipeline.PrepareForLoadWithReport(
            saveRoot,
            GameSimulation.RootSchemaVersion,
            [
                new TestNamedModuleRunner(KnownModuleKeys.OrderAndBanditry, 6),
                new TestNamedModuleRunner(KnownModuleKeys.TradeAndIndustry, 3),
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
            result.Report.ModuleSteps.Select(static step => step.ModuleKey).Distinct().OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.TradeAndIndustry,
            }));
        Assert.That(result.Report.EnabledModuleKeySetPreserved, Is.True);
        Assert.That(result.Report.ModuleStateKeySetPreserved, Is.True);
    }

    [Test]
    public void LoadM3OrderAndBanditry_DefaultMigrationPipeline_UpgradesLegacyOrderSchemaIntoBlackRoutePressureFields()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(20260621);
        simulation.AdvanceMonths(3);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState currentState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        foreach (SettlementDisorderState settlement in currentState.Settlements)
        {
            settlement.BlackRoutePressure = 0;
            settlement.CoercionRisk = 0;
            settlement.SuppressionRelief = 0;
            settlement.ResponseActivationLevel = 0;
            settlement.PaperCompliance = 0;
            settlement.ImplementationDrag = 0;
            settlement.RouteShielding = 0;
            settlement.RetaliationRisk = 0;
            settlement.AdministrativeSuppressionWindow = 0;
            settlement.EscalationBandLabel = string.Empty;
            settlement.LastPressureTrace = string.Empty;
            settlement.LastInterventionCommandCode = null!;
            settlement.LastInterventionCommandLabel = null!;
            settlement.LastInterventionSummary = null!;
            settlement.LastInterventionOutcome = null!;
            settlement.InterventionCarryoverMonths = 4;
        }

        saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), currentState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(saveRoot);
        SaveRoot migratedSave = reloaded.ExportSave();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            migratedSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(
            reloaded.LoadMigrationReport!.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OrderAndBanditry
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OrderAndBanditry
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OrderAndBanditry
                && step.SourceVersion == 3
                && step.TargetVersion == 4),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OrderAndBanditry
                && step.SourceVersion == 4
                && step.TargetVersion == 5),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.OrderAndBanditry
                && step.SourceVersion == 5
                && step.TargetVersion == 6),
            Is.True);
        Assert.That(migratedSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].ModuleSchemaVersion, Is.EqualTo(6));
        Assert.That(migratedState.Settlements, Is.Not.Empty);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.BlackRoutePressure > 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.CoercionRisk >= 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.PaperCompliance >= 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.ImplementationDrag >= 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.RouteShielding >= 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.RetaliationRisk >= 0), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.InterventionCarryoverMonths is >= 0 and <= 1), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.EscalationBandLabel)), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.LastPressureTrace)), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.LastInterventionCommandCode is not null), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.LastInterventionCommandLabel is not null), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.LastInterventionSummary is not null), Is.True);
        Assert.That(migratedState.Settlements.All(static settlement => settlement.LastInterventionOutcome is not null), Is.True);
    }

    [Test]
    public void LoadM3OrderAndBanditry_DefaultMigrationPipeline_BackfillsLegacyTradeGrayLedger()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(20260622);
        simulation.AdvanceMonths(2);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        TradeAndIndustryState currentState = (TradeAndIndustryState)serializer.Deserialize(
            typeof(TradeAndIndustryState),
            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);

        currentState.BlackRouteLedgers = [];
        foreach (RouteTradeState route in currentState.Routes)
        {
            route.BlockedShipmentCount = 0;
            route.SeizureRisk = 0;
            route.RouteConstraintLabel = string.Empty;
            route.LastRouteTrace = string.Empty;
        }

        saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.TradeAndIndustry,
            ModuleSchemaVersion = 1,
            Payload = serializer.Serialize(typeof(TradeAndIndustryState), currentState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(saveRoot);
        SaveRoot migratedSave = reloaded.ExportSave();
        TradeAndIndustryState migratedState = (TradeAndIndustryState)serializer.Deserialize(
            typeof(TradeAndIndustryState),
            migratedSave.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);

        Assert.That(reloaded.LoadMigrationReport, Is.Not.Null);
        Assert.That(
            reloaded.LoadMigrationReport!.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.TradeAndIndustry
                && step.SourceVersion == 1
                && step.TargetVersion == 2),
            Is.True);
        Assert.That(
            reloaded.LoadMigrationReport.ModuleSteps.Any(static step =>
                step.ModuleKey == KnownModuleKeys.TradeAndIndustry
                && step.SourceVersion == 2
                && step.TargetVersion == 3),
            Is.True);
        Assert.That(migratedSave.ModuleStates[KnownModuleKeys.TradeAndIndustry].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(migratedState.BlackRouteLedgers.Count, Is.EqualTo(migratedState.Markets.Count));
        Assert.That(migratedState.BlackRouteLedgers.All(static ledger => ledger.ShadowPriceIndex >= 70), Is.True);
        Assert.That(migratedState.BlackRouteLedgers.All(static ledger => !string.IsNullOrWhiteSpace(ledger.DiversionBandLabel)), Is.True);
        Assert.That(migratedState.BlackRouteLedgers.All(static ledger => !string.IsNullOrWhiteSpace(ledger.LastLedgerTrace)), Is.True);
        Assert.That(migratedState.Routes.All(static route => route.BlockedShipmentCount >= 0), Is.True);
        Assert.That(migratedState.Routes.All(static route => route.SeizureRisk >= 0), Is.True);
        Assert.That(migratedState.Routes.All(static route => !string.IsNullOrWhiteSpace(route.RouteConstraintLabel)), Is.True);
        Assert.That(migratedState.Routes.All(static route => !string.IsNullOrWhiteSpace(route.LastRouteTrace)), Is.True);
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

}
