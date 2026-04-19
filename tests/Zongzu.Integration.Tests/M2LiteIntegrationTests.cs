using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Presentation.Unity;
using Zongzu.Persistence;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class M2LiteIntegrationTests
{
    [Test]
    public void MvpBootstrap_EnablesOnlyMandatoryMvpModules()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateMvpBootstrap(20260419);
        SaveRoot saveRoot = simulation.ExportSave();

        Assert.That(
            SimulationBootstrapper.CreateMvpModules().Select(static module => module.ModuleKey).OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));
        Assert.That(saveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(), Is.EqualTo(new[]
        {
            KnownModuleKeys.EducationAndExams,
            KnownModuleKeys.FamilyCore,
            KnownModuleKeys.NarrativeProjection,
            KnownModuleKeys.PopulationAndHouseholds,
            KnownModuleKeys.SocialMemoryAndRelations,
            KnownModuleKeys.TradeAndIndustry,
            KnownModuleKeys.WorldSettlements,
        }));
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PublicLifeAndRumor), Is.False);
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry), Is.False);
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce), Is.False);
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer), Is.False);
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign), Is.False);
    }

    [Test]
    public void MvpBootstrap_RemainsDeterministicAcrossTwentyYears()
    {
        GameSimulation firstSimulation = SimulationBootstrapper.CreateMvpBootstrap(20260420);
        GameSimulation secondSimulation = SimulationBootstrapper.CreateMvpBootstrap(20260420);

        firstSimulation.AdvanceMonths(240);
        secondSimulation.AdvanceMonths(240);

        Assert.That(secondSimulation.CurrentDate, Is.EqualTo(firstSimulation.CurrentDate));
        Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));
        Assert.That(secondSimulation.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(), Is.EqualTo(firstSimulation.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray()));
    }

    [Test]
    public void BootstrapWorld_IsDeterministicAcrossTwelveMonths()
    {
        GameSimulation firstSimulation = SimulationBootstrapper.CreateM2Bootstrap(188);
        GameSimulation secondSimulation = SimulationBootstrapper.CreateM2Bootstrap(188);

        firstSimulation.AdvanceMonths(12);
        secondSimulation.AdvanceMonths(12);

        Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));
        Assert.That(secondSimulation.ExportSave().CurrentDate, Is.EqualTo(firstSimulation.ExportSave().CurrentDate));
    }

    [Test]
    public void BootstrapWorld_RemainsDeterministicAcrossSixtyMonthsForMultipleSeeds()
    {
        int[] seeds = [11, 37, 20260418];

        foreach (int seed in seeds)
        {
            GameSimulation firstSimulation = SimulationBootstrapper.CreateM2Bootstrap(seed);
            GameSimulation secondSimulation = SimulationBootstrapper.CreateM2Bootstrap(seed);

            firstSimulation.AdvanceMonths(60);
            secondSimulation.AdvanceMonths(60);

            Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash), $"Replay hash mismatch for seed {seed}.");
            Assert.That(secondSimulation.CurrentDate, Is.EqualTo(firstSimulation.CurrentDate), $"Current date mismatch for seed {seed}.");
        }
    }

    [Test]
    public void ExportedSave_PassesModuleBoundaryInvariantChecks()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(199);
        simulation.AdvanceMonths(6);

        SaveRoot saveRoot = simulation.ExportSave();

        Assert.That(
            () => ModuleBoundaryValidator.Validate(simulation.Modules, simulation.FeatureManifest, saveRoot),
            Throws.Nothing);
        Assert.That(
            saveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.PublicLifeAndRumor,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));
        Assert.That(saveRoot.ModuleStates.All(static pair => string.Equals(pair.Key, pair.Value.ModuleKey, StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void M2Bundle_SurfacesFamilyLifecycleAffordances_AndBundleShowsLifecyclePressure()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260419);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        ClanSnapshot clan = bundle.Clans.Single();

        Assert.That(
            bundle.PlayerCommands.Affordances.Any(static affordance =>
                string.Equals(affordance.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && string.Equals(affordance.CommandName, PlayerCommandNames.ArrangeMarriage, StringComparison.Ordinal)),
            Is.True);
        Assert.That(
            bundle.PlayerCommands.Affordances.Any(static affordance =>
                string.Equals(affordance.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && string.Equals(affordance.CommandName, PlayerCommandNames.DesignateHeirPolicy, StringComparison.Ordinal)),
            Is.True);
        Assert.That(bundle.Clans, Is.Not.Empty);
        Assert.That(clan.MarriageAlliancePressure + clan.InheritancePressure + clan.ReproductivePressure + clan.MourningLoad, Is.GreaterThan(0));
    }

    [Test]
    public void DisabledM2LiteModules_RemainAbsentFromSaveState()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(177),
            manifest,
            SimulationBootstrapper.CreateM2Modules());

        SaveRoot saveRoot = simulation.ExportSave();

        Assert.That(
            saveRoot.ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.WorldSettlements,
            }));
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.EducationAndExams), Is.False);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.NarrativeProjection), Is.False);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.TradeAndIndustry), Is.False);
    }

    [Test]
    public void PresentationBundle_AndShell_ExposeNotificationsWithDiffTraceback()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(211);
        simulation.AdvanceMonths(4);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(bundle.Notifications, Is.Not.Empty);
        Assert.That(bundle.PublicLifeSettlements, Is.Not.Empty);
        Assert.That(bundle.Notifications.All(static notification => notification.Traces.Count > 0), Is.True);
        Assert.That(
            bundle.Notifications.Any(static notification => notification.Traces.Any(static trace => !string.IsNullOrWhiteSpace(trace.DiffDescription))),
            Is.True);
        Assert.That(bundle.Notifications.All(static notification => !string.Equals(notification.SourceModuleKey, KnownModuleKeys.NarrativeProjection, StringComparison.Ordinal)), Is.True);
        Assert.That(shell.NotificationCenter.Items.Count, Is.EqualTo(bundle.Notifications.Count));
        Assert.That(bundle.Debug.InitialSeed, Is.EqualTo(211));
        Assert.That(bundle.Debug.DiagnosticsSchemaVersion, Is.EqualTo(1));
        Assert.That(bundle.Debug.LoadMigration.LoadOriginLabel, Is.EqualTo("Bootstrap"));
        Assert.That(bundle.Debug.LoadMigration.WasMigrationApplied, Is.False);
        Assert.That(bundle.Debug.EnabledModules.Any(static module => string.Equals(module.ModuleKey, KnownModuleKeys.NarrativeProjection, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.Debug.ModuleInspectors.Any(static inspector => string.Equals(inspector.ModuleKey, KnownModuleKeys.NarrativeProjection, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.Debug.RecentDiffEntries, Is.Not.Empty);
        Assert.That(bundle.Debug.RecentDomainEvents, Is.Not.Empty);
        Assert.That(bundle.Debug.LatestMetrics.DiffEntryCount, Is.GreaterThan(0));
        Assert.That(bundle.Debug.LatestMetrics.DomainEventCount, Is.GreaterThan(0));
        Assert.That(bundle.Debug.LatestMetrics.NotificationCount, Is.EqualTo(bundle.Notifications.Count));
        Assert.That(bundle.Debug.LatestMetrics.SavePayloadBytes, Is.GreaterThan(0));
        Assert.That(bundle.Debug.CurrentScale.EnabledModuleCount, Is.EqualTo(8));
        Assert.That(bundle.Debug.CurrentScale.SettlementCount, Is.EqualTo(1));
        Assert.That(bundle.Debug.CurrentScale.ClanCount, Is.EqualTo(1));
        Assert.That(bundle.Debug.CurrentScale.HouseholdCount, Is.GreaterThanOrEqualTo(2));
        Assert.That(bundle.Debug.CurrentScale.AverageHouseholdsPerSettlement, Is.GreaterThanOrEqualTo(2));
        Assert.That(bundle.Debug.CurrentHotspots, Is.Empty);
        Assert.That(bundle.Debug.CurrentInteractionPressure.ActivatedResponseSettlements, Is.EqualTo(0));
        Assert.That(bundle.Debug.CurrentInteractionPressure.PeakSuppressionDemand, Is.EqualTo(0));
        Assert.That(bundle.Debug.CurrentPressureDistribution.CalmSettlements, Is.EqualTo(0));
        Assert.That(bundle.Debug.CurrentPayloadSummary.TotalModulePayloadBytes, Is.GreaterThan(0));
        Assert.That(bundle.Debug.TopPayloadModules, Is.Not.Empty);
        Assert.That(bundle.Debug.Invariants, Does.Contain("Module boundary validation passed."));
        Assert.That(shell.Debug.DiagnosticsSchemaLabel, Is.EqualTo("v1"));
        Assert.That(shell.Debug.Scale.LatestMetrics.NotificationCount, Is.EqualTo(bundle.Notifications.Count));
        Assert.That(shell.Debug.Migration.LoadOriginLabel, Is.EqualTo("Bootstrap"));
        Assert.That(shell.Debug.Scale.CurrentScale.ModuleSummary, Does.Contain("enabled modules"));
        Assert.That(shell.Debug.Scale.PayloadSummary.TotalPayloadBytes, Is.GreaterThan(0));
        Assert.That(shell.Debug.Migration.MigrationStatusLabel, Is.EqualTo("Consistency passed"));
        Assert.That(shell.Debug.Scale.ModuleInspectors.Count, Is.EqualTo(bundle.Debug.ModuleInspectors.Count));
        Assert.That(shell.Debug.Hotspots.DiffTraces.Count, Is.EqualTo(bundle.Debug.RecentDiffEntries.Count));
        Assert.That(bundle.PublicLifeSettlements.Single().PublicSummary, Is.Not.Empty);
        Assert.That(bundle.PublicLifeSettlements.Single().CadenceSummary, Is.Not.Empty);
    }

    [Test]
    public void DiagnosticsHarness_ReportsBoundedGrowthAcrossOneHundredTwentyMonths()
    {
        M2DiagnosticsHarness harness = new();
        M2DiagnosticsReport report = harness.Run(31415, 120);

        Assert.That(report.DiagnosticsSchemaVersion, Is.EqualTo(1));
        Assert.That(report.Seed, Is.EqualTo(31415));
        Assert.That(report.MonthsSimulated, Is.EqualTo(120));
        Assert.That(report.Samples, Has.Count.EqualTo(120));
        Assert.That(report.PeakMetrics.DiffEntryCount, Is.GreaterThan(0));
        Assert.That(report.PeakMetrics.DomainEventCount, Is.GreaterThan(0));
        Assert.That(report.PeakMetrics.NotificationCount, Is.LessThanOrEqualTo(NarrativeProjectionModule.NotificationRetentionLimit));
        Assert.That(report.PeakMetrics.SavePayloadBytes, Is.GreaterThan(0));
        Assert.That(report.FinalMetrics.NotificationCount, Is.LessThanOrEqualTo(NarrativeProjectionModule.NotificationRetentionLimit));
        Assert.That(report.RetentionLimitReached, Is.True);
        Assert.That(report.PeakScaleMetrics.EnabledModuleCount, Is.EqualTo(8));
        Assert.That(report.PeakScaleMetrics.SettlementCount, Is.EqualTo(1));
        Assert.That(report.PeakPayloadSummary.TotalModulePayloadBytes, Is.GreaterThan(0));
        Assert.That(report.Samples.All(static sample => !string.IsNullOrWhiteSpace(sample.ReplayHash)), Is.True);
        Assert.That(report.Samples.All(static sample => sample.TopPayloadModules.Count > 0), Is.True);
        Assert.That(report.Samples.All(static sample => sample.PayloadSummary.TotalModulePayloadBytes > 0), Is.True);
        Assert.That(
            report.Samples.Any(static sample => sample.Metrics.NotificationCount == NarrativeProjectionModule.NotificationRetentionLimit),
            Is.True);
    }

    [Test]
    public void DiagnosticsHarness_RunMany_TracksTwoHundredFortyMonthBudgetForLocalConflictSlice()
    {
        M2DiagnosticsHarness harness = new();
        SimulationDiagnosticsBudget budget = new()
        {
            PeakCeiling = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = 18,
                DomainEventCount = 16,
                NotificationCount = NarrativeProjectionModule.NotificationRetentionLimit,
                SavePayloadBytes = 50000,
            },
            GrowthCeiling = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = 18,
                DomainEventCount = 16,
                NotificationCount = NarrativeProjectionModule.NotificationRetentionLimit,
                SavePayloadBytes = 45000,
            },
        };

        M2DiagnosticsSweepReport report = harness.RunMany(
            [11, 37, 20260418],
            240,
            budget,
            SimulationBootstrapper.CreateM3LocalConflictBootstrap);

        Assert.That(report.DiagnosticsSchemaVersion, Is.EqualTo(1));
        Assert.That(report.MonthsSimulated, Is.EqualTo(240));
        Assert.That(report.Runs, Has.Count.EqualTo(3));
        Assert.That(report.PeakMetrics.NotificationCount, Is.LessThanOrEqualTo(NarrativeProjectionModule.NotificationRetentionLimit));
        Assert.That(report.PeakGrowthMetrics.SavePayloadBytes, Is.GreaterThan(0));
        Assert.That(report.RetentionLimitReached, Is.True);
        Assert.That(
            report.Runs.All(static run => run.PeakModuleActivity.Any(static module => module.ModuleKey == KnownModuleKeys.ConflictAndForce && module.DiffEntryCount > 0)),
            Is.True);
        Assert.That(
            report.Runs.All(static run => run.PeakModuleActivity.Any(static module => module.ModuleKey == KnownModuleKeys.OrderAndBanditry && module.DiffEntryCount > 0)),
            Is.True);
        Assert.That(report.PeakInteractionPressure.ActivatedResponseSettlements, Is.GreaterThanOrEqualTo(1));
        Assert.That(report.PeakInteractionPressure.SupportedOrderSettlements, Is.GreaterThanOrEqualTo(1));
        Assert.That(report.PeakInteractionPressure.PeakSuppressionDemand, Is.GreaterThan(0));
        Assert.That(report.PeakScaleMetrics.EnabledModuleCount, Is.EqualTo(10));
        Assert.That(report.PeakScaleMetrics.SettlementCount, Is.EqualTo(1));
        Assert.That(report.PeakPayloadSummary.TotalModulePayloadBytes, Is.GreaterThan(0));
        Assert.That(
            report.BudgetEvaluation.WithinBudget,
            Is.True,
            report.BudgetEvaluation.Violations.Count == 0 ? string.Empty : string.Join(" ", report.BudgetEvaluation.Violations));
    }

    [Test]
    public void DiagnosticsHarness_StressBootstrap_TracksHeavierLocalConflictInteractionBudget()
    {
        M2DiagnosticsHarness harness = new();
        SimulationDiagnosticsBudget budget = new()
        {
            PeakCeiling = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = 64,
                DomainEventCount = 40,
                NotificationCount = NarrativeProjectionModule.NotificationRetentionLimit,
                SavePayloadBytes = 120000,
            },
            GrowthCeiling = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = 64,
                DomainEventCount = 40,
                NotificationCount = NarrativeProjectionModule.NotificationRetentionLimit,
                SavePayloadBytes = 110000,
            },
            InteractionPressureCeiling = new InteractionPressureMetricsSnapshot
            {
                ActiveConflictSettlements = 4,
                ActivatedResponseSettlements = 4,
                SupportedOrderSettlements = 4,
                HighSuppressionDemandSettlements = 4,
            },
            PeakHotspotScoreCeiling = 400,
        };

        M2DiagnosticsSweepReport report = harness.RunMany(
            [7, 17, 29],
            180,
            budget,
            SimulationBootstrapper.CreateM3LocalConflictStressBootstrap);

        Assert.That(report.DiagnosticsSchemaVersion, Is.EqualTo(1));
        Assert.That(report.MonthsSimulated, Is.EqualTo(180));
        Assert.That(report.Runs, Has.Count.EqualTo(3));
        Assert.That(report.PeakMetrics.NotificationCount, Is.LessThanOrEqualTo(NarrativeProjectionModule.NotificationRetentionLimit));
        Assert.That(report.PeakInteractionPressure.ActiveConflictSettlements, Is.GreaterThanOrEqualTo(2));
        Assert.That(report.PeakInteractionPressure.ActivatedResponseSettlements, Is.GreaterThanOrEqualTo(2));
        Assert.That(report.PeakInteractionPressure.SupportedOrderSettlements, Is.GreaterThanOrEqualTo(2));
        Assert.That(report.PeakInteractionPressure.HighSuppressionDemandSettlements, Is.GreaterThanOrEqualTo(1));
        Assert.That(report.PeakInteractionPressure.HighBanditThreatSettlements, Is.GreaterThanOrEqualTo(1));
        Assert.That(report.PeakPressureDistribution.StressedSettlements + report.PeakPressureDistribution.CrisisSettlements, Is.GreaterThanOrEqualTo(1));
        Assert.That(report.PeakScaleMetrics.SettlementCount, Is.EqualTo(4));
        Assert.That(report.PeakScaleMetrics.RouteCount, Is.EqualTo(4));
        Assert.That(report.PeakPayloadSummary.TotalModulePayloadBytes, Is.GreaterThan(0));
        Assert.That(report.Runs.All(static run => run.PeakHotspots.Count > 0), Is.True);
        Assert.That(report.Runs.All(static run => !string.IsNullOrWhiteSpace(run.PeakHotspots[0].SettlementName)), Is.True);
        Assert.That(report.Runs.All(static run => run.PeakHotspots[0].HotspotScore >= run.PeakHotspots[^1].HotspotScore), Is.True);
        Assert.That(report.PeakHotspotScore, Is.GreaterThan(0));
        Assert.That(report.Runs.All(static run => run.PeakPayloadModules.Count > 0), Is.True);
        Assert.That(report.Runs.All(static run => run.Samples.Any(static sample => sample.PressureDistribution.StressedSettlements + sample.PressureDistribution.CrisisSettlements > 0)), Is.True);
        Assert.That(
            report.Runs.All(static run => run.PeakModuleActivity.Any(static module => module.ModuleKey == KnownModuleKeys.ConflictAndForce && module.DiffEntryCount >= 2)),
            Is.True);
        Assert.That(
            report.Runs.All(static run => run.Samples.Any(static sample => sample.InteractionPressure.ActivatedResponseSettlements > 0)),
            Is.True);
        Assert.That(
            report.Runs.All(static run => run.Samples.All(static sample => sample.TopHotspots.Count <= 3)),
            Is.True);
        Assert.That(
            report.BudgetEvaluation.WithinBudget,
            Is.True,
            report.BudgetEvaluation.Violations.Count == 0 ? string.Empty : string.Join(" ", report.BudgetEvaluation.Violations));
    }

    [Test]
    public void DiagnosticsHarness_StressBootstrap_TracksThreeHundredSixtyMonthTrendAndPressureBudgets()
    {
        M2DiagnosticsHarness harness = new();
        SimulationDiagnosticsBudget budget = new()
        {
            PeakCeiling = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = 70,
                DomainEventCount = 45,
                NotificationCount = NarrativeProjectionModule.NotificationRetentionLimit,
                SavePayloadBytes = 130000,
            },
            GrowthCeiling = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = 70,
                DomainEventCount = 45,
                NotificationCount = NarrativeProjectionModule.NotificationRetentionLimit,
                SavePayloadBytes = 120000,
            },
            InteractionPressureCeiling = new InteractionPressureMetricsSnapshot
            {
                ActiveConflictSettlements = 4,
                ActivatedResponseSettlements = 4,
                SupportedOrderSettlements = 4,
                HighSuppressionDemandSettlements = 4,
            },
            PeakHotspotScoreCeiling = 400,
        };

        M2DiagnosticsSweepReport report = harness.RunMany(
            [5, 23],
            360,
            budget,
            SimulationBootstrapper.CreateM3LocalConflictStressBootstrap);

        Assert.That(report.MonthsSimulated, Is.EqualTo(360));
        Assert.That(report.Runs, Has.Count.EqualTo(2));
        Assert.That(report.PeakMetrics.SavePayloadBytes, Is.GreaterThan(0));
        Assert.That(report.PeakInteractionPressure.ActiveConflictSettlements, Is.GreaterThanOrEqualTo(2));
        Assert.That(report.PeakScaleMetrics.SettlementCount, Is.EqualTo(4));
        Assert.That(report.PeakScaleMetrics.SavePayloadBytesPerSettlement, Is.GreaterThan(0));
        Assert.That(report.PeakPayloadSummary.LargestModulePayloadBytes, Is.GreaterThan(0));
        Assert.That(report.PeakHotspotScore, Is.GreaterThan(0));
        Assert.That(report.Runs.All(static run => run.Samples.Count == 360), Is.True);
        Assert.That(report.Runs.All(static run => run.PeakHotspots.Count > 0), Is.True);
        Assert.That(
            report.BudgetEvaluation.WithinBudget,
            Is.True,
            report.BudgetEvaluation.Violations.Count == 0 ? string.Empty : string.Join(" ", report.BudgetEvaluation.Violations));
    }

    [Test]
    public void StressBootstraps_PreserveSettlementParityAcrossOrderOnlyAndLocalConflictPaths()
    {
        GameSimulation orderStressSimulation = SimulationBootstrapper.CreateM3OrderAndBanditryStressBootstrap(6060);
        GameSimulation localConflictStressSimulation = SimulationBootstrapper.CreateM3LocalConflictStressBootstrap(6060);

        SaveRoot orderStressSave = orderStressSimulation.ExportSave();
        SaveRoot localConflictStressSave = localConflictStressSimulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();

        OrderAndBanditryState orderStressState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            orderStressSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        OrderAndBanditryState localConflictOrderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            localConflictStressSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        Assert.That(orderStressState.Settlements.Select(static settlement => settlement.SettlementId).OrderBy(static id => id.Value).ToArray(),
            Is.EqualTo(localConflictOrderState.Settlements.Select(static settlement => settlement.SettlementId).OrderBy(static id => id.Value).ToArray()));
        Assert.That(orderStressState.Settlements, Has.Count.EqualTo(4));
        Assert.That(localConflictStressSave.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.True);
        Assert.That(orderStressSave.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.False);
    }

    [Test]
    public void LocalConflictStressBootstrap_SeparatesActivatedAndCalmSettlementOrderReasons()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictStressBootstrap(7070);
        simulation.AdvanceMonths(1);

        MessagePackModuleStateSerializer serializer = new();
        SaveRoot saveRoot = simulation.ExportSave();
        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        ConflictAndForceState conflictState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        ConflictAndForceModule conflictModule = new();
        QueryRegistry queries = new();
        conflictModule.RegisterQueries(conflictState, queries);
        IConflictAndForceQueries conflictQueries = queries.GetRequired<IConflictAndForceQueries>();

        LocalForcePoolSnapshot[] activeResponses = conflictQueries.GetSettlementForces()
            .Where(static snapshot => snapshot.HasActiveConflict && snapshot.IsResponseActivated)
            .ToArray();
        LocalForcePoolSnapshot[] calmResponses = conflictQueries.GetSettlementForces()
            .Where(static snapshot => !snapshot.HasActiveConflict || !snapshot.IsResponseActivated)
            .ToArray();

        Assert.That(activeResponses, Is.Not.Empty);
        Assert.That(calmResponses, Is.Not.Empty);

        foreach (LocalForcePoolSnapshot active in activeResponses)
        {
            SettlementDisorderState disorder = orderState.Settlements.Single(settlement => settlement.SettlementId == active.SettlementId);
            Assert.That(active.OrderSupportLevel, Is.GreaterThan(0));
            Assert.That(disorder.LastPressureReason, Is.Not.Empty);
        }

        foreach (LocalForcePoolSnapshot calm in calmResponses)
        {
            SettlementDisorderState disorder = orderState.Settlements.Single(settlement => settlement.SettlementId == calm.SettlementId);
            Assert.That(calm.OrderSupportLevel, Is.EqualTo(0));
            Assert.That(disorder.LastPressureReason, Is.Not.Empty);
        }
    }

    [Test]
    public void OrderAndBanditryLite_ProducesTraceableDisorderAndInfluencesTrade()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(5150);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);
        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();

        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(
            typeof(TradeAndIndustryState),
            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);

        Assert.That(orderState.Settlements, Has.Count.EqualTo(1));
        Assert.That(orderState.Settlements.Single().BanditThreat, Is.GreaterThan(0));
        Assert.That(orderState.Settlements.Single().RoutePressure, Is.GreaterThan(0));
        Assert.That(orderState.Settlements.Single().LastPressureReason, Is.Not.Empty);
        Assert.That(tradeState.Clans.Single().LastExplanation, Is.Not.Empty);
        Assert.That(bundle.Notifications.Any(static notification => notification.SourceModuleKey == KnownModuleKeys.OrderAndBanditry), Is.True);
        Assert.That(bundle.Debug.RecentDiffEntries.Any(static trace => trace.ModuleKey == KnownModuleKeys.OrderAndBanditry), Is.True);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.True);
    }

    [Test]
    public void OrderEnabledM3Bootstrap_LoadsOrderWithoutActivatingConflict()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(1234);
        simulation.AdvanceMonths(6);

        SaveCodec codec = new();
        GameSimulation reloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(codec.Decode(codec.Encode(simulation.ExportSave())));

        Assert.That(
            SimulationBootstrapper.CreateM3OrderAndBanditryModules().Select(static module => module.ModuleKey),
            Does.Contain(KnownModuleKeys.OrderAndBanditry));
        Assert.That(
            SimulationBootstrapper.CreateM3OrderAndBanditryModules().Select(static module => module.ModuleKey),
            Does.Not.Contain(KnownModuleKeys.ConflictAndForce));
        Assert.That(reloaded.ExportSave().ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.True);
        Assert.That(reloaded.ExportSave().ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.False);
        Assert.That(reloaded.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry), Is.True);
        Assert.That(reloaded.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce), Is.False);
    }

    [Test]
    public void LocalConflictM3Bootstrap_ActivatesConflictAndFeedsBackIntoOrder()
    {
        GameSimulation orderOnlySimulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(5151);
        GameSimulation localConflictSimulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(5151);

        orderOnlySimulation.AdvanceMonths(2);
        localConflictSimulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle bundle = builder.BuildForM2(localConflictSimulation);
        SaveRoot saveRoot = localConflictSimulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();

        OrderAndBanditryState orderOnlyState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            orderOnlySimulation.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        ConflictAndForceState conflictState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        Assert.That(
            SimulationBootstrapper.CreateM3LocalConflictModules().Select(static module => module.ModuleKey),
            Does.Contain(KnownModuleKeys.OrderAndBanditry));
        Assert.That(
            SimulationBootstrapper.CreateM3LocalConflictModules().Select(static module => module.ModuleKey),
            Does.Contain(KnownModuleKeys.ConflictAndForce));
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.True);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.True);
        Assert.That(localConflictSimulation.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce), Is.True);
        Assert.That(conflictState.Settlements, Has.Count.EqualTo(1));
        Assert.That(conflictState.Settlements.Single().Readiness, Is.GreaterThan(0));
        Assert.That(conflictState.Settlements.Single().LastConflictTrace, Is.Not.Empty);
        Assert.That(orderState.Settlements.Single().SuppressionDemand, Is.LessThan(orderOnlyState.Settlements.Single().SuppressionDemand));
        Assert.That(orderState.Settlements.Single().LastPressureReason, Is.Not.Empty);
        Assert.That(bundle.Notifications.Any(static notification => notification.SourceModuleKey == KnownModuleKeys.ConflictAndForce), Is.True);
        Assert.That(bundle.Notifications.Any(static notification => notification.Surface == NarrativeSurface.ConflictVignette), Is.True);
        Assert.That(bundle.Debug.ModuleInspectors.Any(static inspector => string.Equals(inspector.ModuleKey, KnownModuleKeys.ConflictAndForce, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.Debug.RecentDiffEntries.Any(static trace => trace.ModuleKey == KnownModuleKeys.ConflictAndForce), Is.True);
    }

    [Test]
    public void LocalConflictM3Bootstrap_SynchronizesForceSupportIntoOrderWithinFirstMonth()
    {
        GameSimulation orderOnlySimulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(5152);
        GameSimulation localConflictSimulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(5152);

        orderOnlySimulation.AdvanceMonths(1);
        localConflictSimulation.AdvanceMonths(1);

        MessagePackModuleStateSerializer serializer = new();
        SaveRoot orderOnlySave = orderOnlySimulation.ExportSave();
        SaveRoot localConflictSave = localConflictSimulation.ExportSave();

        OrderAndBanditryState orderOnlyState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            orderOnlySave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        OrderAndBanditryState localConflictOrderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            localConflictSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        ConflictAndForceState conflictState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            localConflictSave.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        ConflictAndForceModule conflictModule = new();
        QueryRegistry queries = new();
        conflictModule.RegisterQueries(conflictState, queries);
        LocalForcePoolSnapshot forceSnapshot = queries.GetRequired<IConflictAndForceQueries>().GetRequiredSettlementForce(conflictState.Settlements.Single().SettlementId);

        Assert.That(forceSnapshot.IsResponseActivated, Is.True);
        Assert.That(forceSnapshot.OrderSupportLevel, Is.GreaterThan(0));
        Assert.That(localConflictOrderState.Settlements.Single().SuppressionDemand, Is.LessThan(orderOnlyState.Settlements.Single().SuppressionDemand));
    }

    [Test]
    public void MigratedLocalConflictLoad_SurfacesRuntimeMigrationAndHotspotDebugInfo()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(8181);
        simulation.AdvanceMonths(3);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState legacyConflictState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        foreach (SettlementForceState settlement in legacyConflictState.Settlements)
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
            Payload = serializer.Serialize(typeof(ConflictAndForceState), legacyConflictState),
        };

        GameSimulation reloaded = SimulationBootstrapper.LoadM3LocalConflict(saveRoot);
        reloaded.AdvanceMonths(1);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle bundle = builder.BuildForM2(reloaded);

        Assert.That(bundle.Debug.LoadMigration.LoadOriginLabel, Is.EqualTo("SaveLoad"));
        Assert.That(bundle.Debug.LoadMigration.WasMigrationApplied, Is.True);
        Assert.That(bundle.Debug.LoadMigration.Steps.Any(static step => step.ScopeLabel == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 1 && step.TargetVersion == 2), Is.True);
        Assert.That(bundle.Debug.LoadMigration.Steps.Any(static step => step.ScopeLabel == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 2 && step.TargetVersion == 3), Is.True);
        Assert.That(bundle.Debug.LoadMigration.StepCount, Is.EqualTo(2));
        Assert.That(bundle.Debug.LoadMigration.ConsistencyPassed, Is.True);
        Assert.That(bundle.Debug.LoadMigration.ConsistencySummary, Does.Contain("module envelopes preserved"));
        Assert.That(bundle.Debug.LoadMigration.Warnings, Is.Empty);
        Assert.That(bundle.Debug.CurrentInteractionPressure.ActiveConflictSettlements, Is.GreaterThanOrEqualTo(0));
        Assert.That(bundle.Debug.CurrentInteractionPressure.PeakSuppressionDemand, Is.GreaterThanOrEqualTo(0));
        Assert.That(bundle.Debug.CurrentPressureDistribution.StressedSettlements + bundle.Debug.CurrentPressureDistribution.CrisisSettlements, Is.GreaterThanOrEqualTo(0));
        Assert.That(bundle.Debug.CurrentScale.SettlementCount, Is.EqualTo(1));
        Assert.That(bundle.Debug.CurrentHotspots, Is.Not.Null);
        Assert.That(bundle.Debug.CurrentHotspots.All(static hotspot => !string.IsNullOrWhiteSpace(hotspot.SettlementName)), Is.True);
        Assert.That(bundle.Debug.CurrentPayloadSummary.LargestModulePayloadBytes, Is.GreaterThan(0));
        Assert.That(bundle.Debug.TopPayloadModules, Is.Not.Empty);
    }

    [Test]
    public void GovernanceBootstrap_ActivatesOfficeModuleAndProducesAppointedCareers()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260511);
        simulation.AdvanceMonths(12);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState officeState = (OfficeAndCareerState)serializer.Deserialize(
            typeof(OfficeAndCareerState),
            saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer), Is.True);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OfficeAndCareer), Is.True);
        Assert.That(officeState.People.Any(static career => career.HasAppointment), Is.True);
        Assert.That(officeState.Jurisdictions.Any(static jurisdiction => jurisdiction.JurisdictionLeverage > 0), Is.True);
        Assert.That(officeState.People.Any(static career => career.ServiceMonths > 0), Is.True);
        Assert.That(officeState.People.Any(static career => !string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask)), Is.True);
        Assert.That(officeState.People.Any(static career => !string.IsNullOrWhiteSpace(career.LastPetitionOutcome)), Is.True);
        Assert.That(officeState.Jurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.CurrentAdministrativeTask)), Is.True);
        Assert.That(bundle.Debug.EnabledModules.Any(static module => module.ModuleKey == KnownModuleKeys.OfficeAndCareer), Is.True);
        Assert.That(bundle.Debug.ModuleInspectors.Any(static inspector => inspector.ModuleKey == KnownModuleKeys.OfficeAndCareer), Is.True);
    }

    [Test]
    public void GovernanceBootstrap_PresentationBundle_ExposesReadOnlyOfficeSurface()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260517);
        simulation.AdvanceMonths(8);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(bundle.OfficeCareers, Is.Not.Empty);
        Assert.That(bundle.OfficeJurisdictions, Is.Not.Empty);
        Assert.That(bundle.OfficeCareers.Any(static career => career.HasAppointment), Is.True);
        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask)), Is.True);
        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.AdministrativeTaskTier)), Is.True);
        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.PetitionOutcomeCategory)), Is.True);
        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.AuthorityTrajectorySummary)), Is.True);
        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => jurisdiction.PetitionBacklog >= 0), Is.True);
        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.AdministrativeTaskTier)), Is.True);
        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.PetitionOutcomeCategory)), Is.True);
        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.LastAdministrativeTrace)), Is.True);
        Assert.That(shell.GreatHall.GovernanceSummary, Is.Not.Empty);
        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.GovernanceSummary)), Is.True);
        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.OfficeTitle)), Is.True);
        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.PressureSummary)), Is.True);
        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.PetitionOutcomeCategory)), Is.True);
        Assert.That(shell.Office.Jurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.PetitionSummary)), Is.True);
        Assert.That(shell.Office.Jurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.PetitionOutcomeCategory)), Is.True);
    }

    [Test]
    public void GovernanceBootstrap_RemainsDeterministicAcrossSixtyMonthsForMultipleSeeds()
    {
        int[] seeds = [20260518, 20260521];

        foreach (int seed in seeds)
        {
            GameSimulation firstSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(seed);
            GameSimulation secondSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(seed);

            firstSimulation.AdvanceMonths(60);
            secondSimulation.AdvanceMonths(60);

            Assert.That(secondSimulation.CurrentDate, Is.EqualTo(firstSimulation.CurrentDate));
            Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));
        }
    }

    [Test]
    public void CampaignSandboxBootstrap_ActivatesWarfareCampaignAndSurfacesReadOnlyBoard()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260522);
        simulation.AdvanceMonths(8);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState warfareState = (WarfareCampaignState)serializer.Deserialize(
            typeof(WarfareCampaignState),
            saveRoot.ModuleStates[KnownModuleKeys.WarfareCampaign].Payload);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign), Is.True);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.WarfareCampaign), Is.True);
        Assert.That(warfareState.Campaigns, Is.Not.Empty);
        Assert.That(warfareState.Campaigns.Any(static campaign => campaign.IsActive), Is.True);
        Assert.That(warfareState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.FrontLabel)), Is.True);
        Assert.That(warfareState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommandFitLabel)), Is.True);
        Assert.That(warfareState.Campaigns.Any(static campaign => campaign.Routes.Count > 0), Is.True);
        Assert.That(warfareState.MobilizationSignals, Is.Not.Empty);
        Assert.That(warfareState.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);
        Assert.That(bundle.Campaigns, Is.Not.Empty);
        Assert.That(bundle.CampaignMobilizationSignals, Is.Not.Empty);
        Assert.That(bundle.Campaigns.Any(static campaign => campaign.IsActive), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.AnchorSettlementName)), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommanderSummary)), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => campaign.Routes.Count > 0), Is.True);
        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.MobilizationWindowLabel)), Is.True);
        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)), Is.True);
        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.ActiveDirectiveLabel)), Is.True);
        Assert.That(bundle.Notifications.Any(static notification => notification.SourceModuleKey == KnownModuleKeys.WarfareCampaign), Is.True);
        Assert.That(bundle.Notifications.Any(static notification => notification.Surface == NarrativeSurface.DeskSandbox), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.LastAftermathSummary)), Is.True);
        Assert.That(shell.GreatHall.WarfareSummary, Is.Not.Empty);
        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.CampaignSummary)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards, Is.Not.Empty);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.RegionalProfileLabel)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.RegionalBackdropSummary)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.EnvironmentLabel)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.BoardSurfaceLabel)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.BoardAtmosphereSummary)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.MarkerSummary)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.CommandFitLabel)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.CommanderSummary)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.DirectiveLabel)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => board.Routes.Count > 0), Is.True);
        Assert.That(shell.Warfare.MobilizationSignals, Is.Not.Empty);
        Assert.That(shell.Warfare.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);
        Assert.That(shell.Warfare.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.DirectiveLabel)), Is.True);
    }

    [Test]
    public void CampaignSandboxBootstrap_UsesNorthernSongGroundedSeedLabels()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictStressBootstrap(20260523);
        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();

        WorldSettlementsState worldState = (WorldSettlementsState)serializer.Deserialize(
            typeof(WorldSettlementsState),
            saveRoot.ModuleStates[KnownModuleKeys.WorldSettlements].Payload);
        PopulationAndHouseholdsState populationState = (PopulationAndHouseholdsState)serializer.Deserialize(
            typeof(PopulationAndHouseholdsState),
            saveRoot.ModuleStates[KnownModuleKeys.PopulationAndHouseholds].Payload);
        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(
            typeof(TradeAndIndustryState),
            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);

        Assert.That(worldState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.Name)), Is.True);
        Assert.That(worldState.Settlements.Select(static settlement => settlement.Name).Distinct(StringComparer.Ordinal).Count(), Is.GreaterThanOrEqualTo(2));
        Assert.That(tradeState.Markets.Any(static market => !string.IsNullOrWhiteSpace(market.MarketName)), Is.True);
        Assert.That(tradeState.Routes.Any(static route => !string.IsNullOrWhiteSpace(route.RouteName)), Is.True);
        Assert.That(tradeState.Routes.Select(static route => route.RouteName).Distinct(StringComparer.Ordinal).Count(), Is.GreaterThanOrEqualTo(2));
        Assert.That(populationState.Households.Any(static household => !string.IsNullOrWhiteSpace(household.HouseholdName)), Is.True);
        Assert.That(populationState.Households.Select(static household => household.HouseholdName).Distinct(StringComparer.Ordinal).Count(), Is.GreaterThanOrEqualTo(2));
        Assert.That(
            tradeState.Routes.All(static route => !route.RouteName.Contains("Wharf", StringComparison.Ordinal) && !route.RouteName.Contains("Canal", StringComparison.Ordinal) && !route.RouteName.Contains("Ferry", StringComparison.Ordinal)),
            Is.True);
        Assert.That(
            populationState.Households.All(static household => !household.HouseholdName.Contains("Tenant", StringComparison.Ordinal) && !household.HouseholdName.Contains("Boatman", StringComparison.Ordinal)),
            Is.True);
    }

    [Test]
    public void CampaignSandboxCommandService_AppliesAncientDirectiveToCampaignBoard()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260527);
        simulation.AdvanceMonths(3);

        WarfareCampaignCommandService service = new();
        SettlementId anchorSettlementId = new PresentationReadModelBuilder()
            .BuildForM2(simulation)
            .CampaignMobilizationSignals
            .Single()
            .SettlementId;

        WarfareCampaignIntentResult result = service.IssueIntent(
            simulation,
            new WarfareCampaignIntentCommand
            {
                SettlementId = anchorSettlementId,
                CommandName = WarfareCampaignCommandNames.ProtectSupplyLine,
            });

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        CampaignFrontSnapshot campaign = bundle.Campaigns.Single();
        CampaignMobilizationSignalSnapshot signal = bundle.CampaignMobilizationSignals.Single();

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.DirectiveLabel, Is.Not.Empty);
        Assert.That(result.Summary, Is.Not.Empty);
        Assert.That(campaign.ActiveDirectiveLabel, Is.EqualTo(result.DirectiveLabel));
        Assert.That(campaign.ActiveDirectiveSummary, Is.Not.Empty);
        Assert.That(campaign.LastDirectiveTrace, Is.Not.Empty);
        Assert.That(signal.ActiveDirectiveLabel, Is.EqualTo(result.DirectiveLabel));
        Assert.That(signal.ActiveDirectiveSummary, Is.Not.Empty);
    }

    [Test]
    public void CampaignBundle_ExportsReadOnlyPlayerCommandAffordances()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260601);
        simulation.AdvanceMonths(2);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.PetitionViaOfficeChannels, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.DeployAdministrativeLeverage, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.CommitMobilization, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal) && command.IsEnabled), Is.True);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal) && command.IsEnabled), Is.True);
        Assert.That(shell.Office.CommandAffordances, Is.Not.Empty);
        Assert.That(shell.Office.CommandAffordances.Any(static command => command.IsEnabled), Is.True);
        Assert.That(shell.Warfare.CommandAffordances, Is.Not.Empty);
        Assert.That(shell.Warfare.CommandAffordances.Any(static command => command.IsEnabled), Is.True);
    }

    [Test]
    public void PlayerCommandService_RoutesOfficeAndWarfareIntents_AndUpdatesReadModels()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260602);
        simulation.AdvanceMonths(3);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
        SettlementId officeSettlementId = beforeBundle.OfficeJurisdictions.Single().SettlementId;
        SettlementId warfareSettlementId = beforeBundle.CampaignMobilizationSignals.Single().SettlementId;
        PlayerCommandService service = new();

        PlayerCommandResult officeResult = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = officeSettlementId,
                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
            });
        PlayerCommandResult warfareResult = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = warfareSettlementId,
                CommandName = PlayerCommandNames.ProtectSupplyLine,
            });

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        JurisdictionAuthoritySnapshot jurisdiction = afterBundle.OfficeJurisdictions.Single();
        CampaignFrontSnapshot campaign = afterBundle.Campaigns.Single();
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);

        Assert.That(officeResult.Accepted, Is.True);
        Assert.That(officeResult.Label, Is.Not.Empty);
        Assert.That(officeResult.Summary, Is.Not.Empty);
        Assert.That(warfareResult.Accepted, Is.True);
        Assert.That(warfareResult.Label, Is.Not.Empty);
        Assert.That(jurisdiction.LastAdministrativeTrace, Is.Not.Empty);
        Assert.That(jurisdiction.LastPetitionOutcome, Is.Not.Empty);
        Assert.That(campaign.ActiveDirectiveLabel, Is.Not.Empty);
        Assert.That(campaign.LastDirectiveTrace, Is.Not.Empty);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal)), Is.True);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal)), Is.True);
        Assert.That(shell.Office.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.PetitionViaOfficeChannels, StringComparison.Ordinal)), Is.True);
        Assert.That(shell.Warfare.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.ProtectSupplyLine, StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void PlayerCommandService_RemainsDeterministicForSameOfficeIntent()
    {
        GameSimulation firstSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260603);
        GameSimulation secondSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260603);
        firstSimulation.AdvanceMonths(2);
        secondSimulation.AdvanceMonths(2);

        SettlementId settlementId = new PresentationReadModelBuilder()
            .BuildForM2(firstSimulation)
            .OfficeJurisdictions
            .Single()
            .SettlementId;
        PlayerCommandService service = new();

        PlayerCommandResult firstResult = service.IssueIntent(
            firstSimulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.DeployAdministrativeLeverage,
            });
        PlayerCommandResult secondResult = service.IssueIntent(
            secondSimulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.DeployAdministrativeLeverage,
            });

        Assert.That(firstResult.Accepted, Is.True);
        Assert.That(secondResult.Accepted, Is.True);
        Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));
        Assert.That(secondSimulation.ExportSave().CurrentDate, Is.EqualTo(firstSimulation.ExportSave().CurrentDate));
        Assert.That(new PresentationReadModelBuilder().BuildForM2(secondSimulation).OfficeJurisdictions.Single().LastPetitionOutcome,
            Is.EqualTo(new PresentationReadModelBuilder().BuildForM2(firstSimulation).OfficeJurisdictions.Single().LastPetitionOutcome));
    }

    [Test]
    public void PlayerCommandService_RoutesFamilyIntent_AndSurfacesFamilyReceipts()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260605);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
        ClanSnapshot clan = beforeBundle.Clans.Single();
        PlayerCommandService service = new();

        PlayerCommandResult result = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersMediation,
            });

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        ClanSnapshot updatedClan = afterBundle.Clans.Single(updated => updated.Id == clan.Id);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.CommandName, Is.EqualTo(PlayerCommandNames.InviteClanEldersMediation));
        Assert.That(result.Label, Is.Not.Empty);
        Assert.That(result.TargetLabel, Is.EqualTo(clan.ClanName));
        Assert.That(updatedClan.MediationMomentum, Is.GreaterThan(clan.MediationMomentum));
        Assert.That(updatedClan.LastConflictOutcome, Is.Not.Empty);
        Assert.That(updatedClan.LastConflictCommandLabel, Is.EqualTo(result.Label));
        Assert.That(afterBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)), Is.True);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(receipt =>
            string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
            && string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersMediation, StringComparison.Ordinal)
            && string.Equals(receipt.TargetLabel, clan.ClanName, StringComparison.Ordinal)), Is.True);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal) && string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersMediation, StringComparison.Ordinal)), Is.True);
        Assert.That(shell.FamilyCouncil.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersMediation, StringComparison.Ordinal)), Is.True);
        Assert.That(shell.FamilyCouncil.Clans.Any(static entry => !string.IsNullOrWhiteSpace(entry.LastOrderSummary)), Is.True);
    }

    [Test]
    public void PlayerCommandService_RoutesFamilyLifecycleIntent_AndSurfacesRicherLifecycleReceipts()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260619);
        simulation.AdvanceMonths(2);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
        ClanSnapshot clan = beforeBundle.Clans.Single();
        PlayerCommandService service = new();

        PlayerCommandResult result = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.ArrangeMarriage,
            });

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        ClanSnapshot updatedClan = afterBundle.Clans.Single(updated => updated.Id == clan.Id);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.CommandName, Is.EqualTo(PlayerCommandNames.ArrangeMarriage));
        Assert.That(result.Label, Is.Not.Empty);
        Assert.That(result.TargetLabel, Is.EqualTo(clan.ClanName));
        Assert.That(result.Summary, Is.Not.Empty);
        Assert.That(updatedClan.LastLifecycleOutcome, Is.Not.Empty);
        Assert.That(updatedClan.LastLifecycleCommandLabel, Is.EqualTo(result.Label));
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt =>
            string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
            && string.Equals(receipt.CommandName, PlayerCommandNames.ArrangeMarriage, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);
        Assert.That(shell.FamilyCouncil.Clans.Any(entry => entry.LifecycleSummary.Contains(result.Label, StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void PlayerCommandService_RoutesNewbornCareIntent_AndSurfacesInfantFollowUp()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260620);
        simulation.AdvanceMonths(2);

        MessagePackModuleStateSerializer serializer = new();
        SaveRoot saveRoot = simulation.ExportSave();
        FamilyCoreState familyState = (FamilyCoreState)serializer.Deserialize(
            typeof(FamilyCoreState),
            saveRoot.ModuleStates[KnownModuleKeys.FamilyCore].Payload);
        ClanStateData clanState = familyState.Clans.Single();
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(9001),
            ClanId = clanState.Id,
            GivenName = "新婴",
            AgeMonths = 8,
            IsAlive = true,
        });
        clanState.SupportReserve = Math.Max(clanState.SupportReserve, 12);
        saveRoot.ModuleStates[KnownModuleKeys.FamilyCore] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(FamilyCoreState), familyState),
        };
        simulation = SimulationBootstrapper.LoadM2(saveRoot);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
        ClanSnapshot clan = beforeBundle.Clans.Single();
        PlayerCommandService service = new();

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static affordance =>
            string.Equals(affordance.CommandName, PlayerCommandNames.SupportNewbornCare, StringComparison.Ordinal)
            && affordance.IsEnabled), Is.True);

        PlayerCommandResult result = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SupportNewbornCare,
            });

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        ClanSnapshot updatedClan = afterBundle.Clans.Single(updated => updated.Id == clan.Id);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Label, Is.Not.Empty);
        Assert.That(result.Summary, Is.Not.Empty);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt =>
            string.Equals(receipt.CommandName, PlayerCommandNames.SupportNewbornCare, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);
        Assert.That(updatedClan.LastLifecycleCommandLabel, Is.EqualTo(result.Label));
        Assert.That(updatedClan.LastLifecycleOutcome, Is.Not.Empty);
        Assert.That(updatedClan.InfantCount, Is.GreaterThan(0));
        Assert.That(shell.FamilyCouncil.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.SupportNewbornCare, StringComparison.Ordinal)), Is.True);
        Assert.That(shell.FamilyCouncil.Clans.Any(entry => entry.LifecycleSummary.Contains(result.Label, StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void PlayerCommandService_RoutesMourningOrderIntent_AndSurfacesMourningFollowUp()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260621);
        simulation.AdvanceMonths(2);

        MessagePackModuleStateSerializer serializer = new();
        SaveRoot saveRoot = simulation.ExportSave();
        FamilyCoreState familyState = (FamilyCoreState)serializer.Deserialize(
            typeof(FamilyCoreState),
            saveRoot.ModuleStates[KnownModuleKeys.FamilyCore].Payload);
        ClanStateData clanState = familyState.Clans.Single();
        clanState.MourningLoad = 24;
        clanState.InheritancePressure = Math.Max(clanState.InheritancePressure, 28);
        clanState.SupportReserve = Math.Max(clanState.SupportReserve, 12);
        saveRoot.ModuleStates[KnownModuleKeys.FamilyCore] = new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(FamilyCoreState), familyState),
        };
        simulation = SimulationBootstrapper.LoadM2(saveRoot);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
        ClanSnapshot clan = beforeBundle.Clans.Single();
        PlayerCommandService service = new();
        PresentationShellViewModel shellBefore = FirstPassPresentationShell.Compose(beforeBundle);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static affordance =>
            string.Equals(affordance.CommandName, PlayerCommandNames.SetMourningOrder, StringComparison.Ordinal)
            && affordance.IsEnabled), Is.True);
        Assert.That(shellBefore.FamilyCouncil.Summary, Is.Not.Empty);

        PlayerCommandResult result = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SetMourningOrder,
            });

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Label, Is.Not.Empty);
        Assert.That(result.Summary, Is.Not.Empty);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt =>
            string.Equals(receipt.CommandName, PlayerCommandNames.SetMourningOrder, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);
        Assert.That(shell.FamilyCouncil.RecentReceipts.Any(static receipt =>
            string.Equals(receipt.CommandName, PlayerCommandNames.SetMourningOrder, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);
        Assert.That(shell.FamilyCouncil.Clans.Single().LifecycleSummary, Is.Not.Empty);
    }

    [Test]
    public void MvpFamilyLifecyclePreviewScenario_BuildsViewableBeforeAndAfterBundles()
    {
        MvpFamilyLifecyclePreviewResult preview = new MvpFamilyLifecyclePreviewScenario().Build(20260419, 2);

        PresentationShellViewModel beforeShell = FirstPassPresentationShell.Compose(preview.BeforeBundle);
        PresentationShellViewModel afterCommandShell = FirstPassPresentationShell.Compose(preview.AfterCommandBundle);
        PresentationShellViewModel afterAdvanceShell = FirstPassPresentationShell.Compose(preview.AfterAdvanceBundle);

        Assert.That(preview.SelectedAffordance.IsEnabled, Is.True);
        Assert.That(preview.CommandResult.Accepted, Is.True);
        Assert.That(preview.BeforeBundle.Clans, Is.Not.Empty);
        Assert.That(preview.BeforeBundle.PublicLifeSettlements, Is.Empty);
        Assert.That(preview.AfterCommandBundle.PublicLifeSettlements, Is.Empty);
        Assert.That(preview.AfterAdvanceBundle.PublicLifeSettlements, Is.Empty);
        Assert.That(preview.AfterCommandBundle.PlayerCommands.Receipts.Any(receipt =>
            string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
            && string.Equals(receipt.CommandName, preview.CommandResult.CommandName, StringComparison.Ordinal)), Is.True);
        Assert.That(beforeShell.GreatHall.FamilySummary, Is.Not.Empty);
        Assert.That(beforeShell.GreatHall.PublicLifeSummary, Is.Not.Empty);
        Assert.That(afterCommandShell.FamilyCouncil.RecentReceipts.Any(receipt =>
            string.Equals(receipt.CommandName, preview.CommandResult.CommandName, StringComparison.Ordinal)), Is.True);
        Assert.That(afterAdvanceShell.NotificationCenter.Items, Is.Not.Empty);
        Assert.That(afterAdvanceShell.GreatHall.LeadNoticeTitle, Is.Not.Empty);
    }

    [Test]
    public void MvpFamilyLifecyclePreviewScenario_TenYearRun_KeepsLifecycleGuidanceAligned()
    {
        MvpFamilyLifecycleTenYearPreviewResult preview = new MvpFamilyLifecyclePreviewScenario().BuildTenYear(20260419, 10);

        Assert.That(preview.YearlyCheckpoints, Has.Count.EqualTo(10));
        Assert.That(preview.IssuedCommands, Is.Not.Empty);

        foreach (MvpFamilyLifecycleTenYearCheckpoint checkpoint in preview.YearlyCheckpoints)
        {
            PresentationShellViewModel shell = FirstPassPresentationShell.Compose(checkpoint.AfterAdvanceBundle);
            PlayerCommandAffordanceSnapshot? primary = checkpoint.AfterAdvanceBundle.PlayerCommands.Affordances
                .Where(static command =>
                    command.IsEnabled
                    && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                    && command.CommandName is
                        PlayerCommandNames.SetMourningOrder
                        or PlayerCommandNames.SupportNewbornCare
                        or PlayerCommandNames.DesignateHeirPolicy
                        or PlayerCommandNames.ArrangeMarriage)
                .OrderBy(static command => command.CommandName switch
                {
                    PlayerCommandNames.SetMourningOrder => 0,
                    PlayerCommandNames.SupportNewbornCare => 1,
                    PlayerCommandNames.DesignateHeirPolicy => 2,
                    PlayerCommandNames.ArrangeMarriage => 3,
                    _ => 10,
                })
                .ThenBy(static command => command.TargetLabel, StringComparer.Ordinal)
                .FirstOrDefault();

            Assert.That(checkpoint.AfterAdvanceBundle.PublicLifeSettlements, Is.Empty);

            if (primary is null)
            {
                continue;
            }

            Assert.That(shell.GreatHall.FamilySummary, Does.Contain(primary.Label));
            Assert.That(shell.FamilyCouncil.Summary, Does.Contain(primary.Label));

            FamilyConflictTileViewModel? clan = shell.FamilyCouncil.Clans.FirstOrDefault();
            if (clan is not null)
            {
                Assert.That(clan.LifecycleSummary, Does.Contain(primary.Label));
            }

            NotificationItemViewModel? familyNotice = shell.NotificationCenter.Items
                .FirstOrDefault(static item => item.SourceModuleKey == KnownModuleKeys.FamilyCore);
            if (familyNotice is not null)
            {
                Assert.That(familyNotice.WhatNext, Is.Not.Empty);
            }
        }
    }

    [Test]
    public void StableM2Bootstrap_DoesNotLeakOfficeOrWarfarePlayerCommands()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260604);
        simulation.AdvanceMonths(2);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal)), Is.False);
        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal)), Is.False);
        Assert.That(bundle.PlayerCommands.Receipts, Is.Empty);
        Assert.That(shell.Office.CommandAffordances, Is.Empty);
        Assert.That(shell.Warfare.CommandAffordances, Is.Empty);
        Assert.That(shell.FamilyCouncil.CommandAffordances, Is.Not.Empty);
    }

    [Test]
    public void CampaignSandbox_AftermathHandlers_UpdateOwnedDownstreamModules()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260529);
        simulation.AdvanceMonths(1);

        SaveRoot saveRoot = simulation.ExportSave();
        MessagePackModuleStateSerializer serializer = new();
        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(
            typeof(TradeAndIndustryState),
            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);
        WorldSettlementsState worldState = (WorldSettlementsState)serializer.Deserialize(
            typeof(WorldSettlementsState),
            saveRoot.ModuleStates[KnownModuleKeys.WorldSettlements].Payload);
        PopulationAndHouseholdsState populationState = (PopulationAndHouseholdsState)serializer.Deserialize(
            typeof(PopulationAndHouseholdsState),
            saveRoot.ModuleStates[KnownModuleKeys.PopulationAndHouseholds].Payload);
        FamilyCoreState familyState = (FamilyCoreState)serializer.Deserialize(
            typeof(FamilyCoreState),
            saveRoot.ModuleStates[KnownModuleKeys.FamilyCore].Payload);
        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        OfficeAndCareerState officeState = (OfficeAndCareerState)serializer.Deserialize(
            typeof(OfficeAndCareerState),
            saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);
        SocialMemoryAndRelationsState socialState = (SocialMemoryAndRelationsState)serializer.Deserialize(
            typeof(SocialMemoryAndRelationsState),
            saveRoot.ModuleStates[KnownModuleKeys.SocialMemoryAndRelations].Payload);
        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(simulation.LastMonthResult, Is.Not.Null);
        Assert.That(simulation.LastMonthResult!.DomainEvents.Any(static entry => entry.ModuleKey == KnownModuleKeys.WarfareCampaign && entry.EntityKey == "1"), Is.True);
        Assert.That(worldState.Settlements.Any(static settlement => settlement.Security < 57 || settlement.Prosperity < 61), Is.True);
        Assert.That(populationState.Households.Any(static household => household.Distress > 35 || household.MigrationRisk > 40), Is.True);
        Assert.That(familyState.Clans.Any(static clan => clan.Prestige != 52 || clan.SupportReserve != 60), Is.True);
        Assert.That(tradeState.Clans.Any(static trade => !string.IsNullOrWhiteSpace(trade.LastExplanation)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.LastPressureReason)), Is.True);
        Assert.That(officeState.People.Any(static career => !string.IsNullOrWhiteSpace(career.LastPetitionOutcome)), Is.True);
        Assert.That(socialState.Memories.Any(static memory => memory.Kind.StartsWith("campaign-", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.WorldSettlements && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.PopulationAndHouseholds && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.FamilyCore && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.TradeAndIndustry && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.OrderAndBanditry && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.OfficeAndCareer && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
        Assert.That(shell.GreatHall.AftermathDocketSummary, Is.Not.Empty);
        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.AftermathSummary)), Is.True);
        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.AftermathDocketSummary)), Is.True);
    }

    [Test]
    public void CampaignSandbox_AftermathHandlers_DragConflictForceThroughOwnedFatigueState()
    {
        GameSimulation governanceSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260531);
        GameSimulation campaignSimulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260531);

        governanceSimulation.AdvanceMonths(1);
        campaignSimulation.AdvanceMonths(1);

        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState governanceState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            governanceSimulation.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);
        ConflictAndForceState campaignState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            campaignSimulation.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        SettlementForceState governanceForce = governanceState.Settlements.Single();
        SettlementForceState campaignForce = campaignState.Settlements.Single();

        Assert.That(campaignForce.CampaignFatigue, Is.GreaterThan(0));
        Assert.That(campaignForce.CampaignEscortStrain, Is.GreaterThan(0));
        Assert.That(campaignForce.LastCampaignFalloutTrace, Is.Not.Empty);
        Assert.That(campaignForce.LastConflictTrace, Is.Not.Empty);
        Assert.That(campaignForce.Readiness, Is.LessThan(governanceForce.Readiness));
        Assert.That(campaignForce.CommandCapacity, Is.LessThanOrEqualTo(governanceForce.CommandCapacity));
        Assert.That(campaignSimulation.LastMonthResult!.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.ConflictAndForce && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);
    }

    [Test]
    public void CampaignSandboxBootstrap_RemainsDeterministicAcrossSixtyMonthsForMultipleSeeds()
    {
        int[] seeds = [20260523, 20260524];

        foreach (int seed in seeds)
        {
            GameSimulation firstSimulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(seed);
            GameSimulation secondSimulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(seed);

            firstSimulation.AdvanceMonths(60);
            secondSimulation.AdvanceMonths(60);

            Assert.That(secondSimulation.CurrentDate, Is.EqualTo(firstSimulation.CurrentDate));
            Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));
        }
    }

    [Test]
    public void ActiveM2Bootstrap_RemainsIsolatedFromM3LocalConflictModules()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(27182);
        SaveRoot saveRoot = simulation.ExportSave();

        Assert.That(
            SimulationBootstrapper.CreateM2Modules().Select(static module => module.ModuleKey),
            Does.Not.Contain(KnownModuleKeys.OrderAndBanditry));
        Assert.That(
            SimulationBootstrapper.CreateM2Modules().Select(static module => module.ModuleKey),
            Does.Not.Contain(KnownModuleKeys.ConflictAndForce));
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.False);
        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.False);
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry), Is.False);
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce), Is.False);
    }

    [Test]
    public void LocalConflictPresentation_RemainsOfficeFreeWithoutGovernanceLite()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(27183);
        simulation.AdvanceMonths(4);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer), Is.False);
        Assert.That(bundle.OfficeCareers, Is.Empty);
        Assert.That(bundle.OfficeJurisdictions, Is.Empty);
        Assert.That(shell.GreatHall.GovernanceSummary, Is.Not.Empty);
        Assert.That(shell.Office.Summary, Is.Not.Empty);
        Assert.That(shell.Office.Appointments, Is.Empty);
        Assert.That(shell.Office.Jurisdictions, Is.Empty);
        Assert.That(bundle.Campaigns, Is.Empty);
        Assert.That(bundle.CampaignMobilizationSignals, Is.Empty);
        Assert.That(shell.GreatHall.WarfareSummary, Is.Not.Empty);
        Assert.That(shell.Warfare.Summary, Is.Not.Empty);
    }

    [Test]
    public void PostMvpPreflightSeams_ReserveWarfareCampaignAndKeepBlackRouteInsideOwnedNamespaces()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(314159);

        Assert.That(
            PostMvpPreflightSeams.BlackRouteOwnerModuleKeys,
            Is.EqualTo(new[]
            {
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.TradeAndIndustry,
            }));
        Assert.That(PostMvpPreflightSeams.BlackRouteOwnerModuleKeys, Does.Not.Contain(KnownModuleKeys.WarfareCampaign));
        Assert.That(
            PostMvpPreflightSeams.BlackRoutePressureUpstreamModuleKeys,
            Is.EqualTo(new[]
            {
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.ConflictAndForce,
                KnownModuleKeys.OfficeAndCareer,
            }));
        Assert.That(
            PostMvpPreflightSeams.BlackRouteLedgerOwnerModuleKeys,
            Is.EqualTo(new[]
            {
                KnownModuleKeys.TradeAndIndustry,
            }));
        Assert.That(
            PostMvpPreflightSeams.WarfareCampaignUpstreamModuleKeys,
            Is.EqualTo(new[]
            {
                KnownModuleKeys.ConflictAndForce,
                KnownModuleKeys.WorldSettlements,
                KnownModuleKeys.OfficeAndCareer,
            }));
        Assert.That(
            PostMvpPreflightSeams.WarfareCampaignMigrationOwnerModuleKeys,
            Is.EqualTo(new[]
            {
                KnownModuleKeys.WarfareCampaign,
            }));
        Assert.That(
            SimulationBootstrapper.CreateM3LocalConflictModules().Select(static module => module.ModuleKey),
            Does.Not.Contain(KnownModuleKeys.WarfareCampaign));
        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign), Is.False);
        Assert.That(simulation.ExportSave().ModuleStates.ContainsKey(KnownModuleKeys.WarfareCampaign), Is.False);
    }

    [Test]
    public void GovernanceLocalConflict_PublicLifeCommands_ProjectAffordancesAndReceiptsOnDeskNodes()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260619);
        simulation.AdvanceMonths(3);

        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);

        Assert.That(beforeBundle.PublicLifeSettlements, Is.Not.Empty);
        Assert.That(beforeBundle.PublicLifeSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.ChannelSummary)), Is.True);
        Assert.That(beforeBundle.PublicLifeSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.ContentionSummary)), Is.True);
        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)), Is.True);
        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.PostCountyNotice, StringComparison.Ordinal)), Is.True);
        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal)), Is.True);
        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.EscortRoadReport, StringComparison.Ordinal)), Is.True);
        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)), Is.True);

        SettlementId settlementId = beforeBundle.PublicLifeSettlements.Single().SettlementId;
        ClanId clanId = beforeBundle.Clans.Single(clan => clan.HomeSettlementId == settlementId).Id;
        PlayerCommandService service = new();

        PlayerCommandResult noticeResult = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.PostCountyNotice,
            });
        PlayerCommandResult roadReportResult = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.DispatchRoadReport,
            });
        PlayerCommandResult escortResult = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.EscortRoadReport,
            });
        PlayerCommandResult elderResult = service.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                ClanId = clanId,
                CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,
            });

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);
        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();

        Assert.That(noticeResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));
        Assert.That(noticeResult.CommandName, Is.EqualTo(PlayerCommandNames.PostCountyNotice));
        Assert.That(roadReportResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));
        Assert.That(escortResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));
        Assert.That(elderResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)), Is.True);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal)), Is.True);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.EscortRoadReport, StringComparison.Ordinal)), Is.True);
        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)), Is.True);
        Assert.That(settlementNode.PublicLifeCommandAffordances, Is.Not.Empty);
        Assert.That(settlementNode.PublicLifeCommandAffordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.PostCountyNotice, StringComparison.Ordinal)), Is.True);
        Assert.That(settlementNode.PublicLifeRecentReceipts, Is.Not.Empty);
        Assert.That(settlementNode.PublicLifeRecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal)), Is.True);
        Assert.That(settlementNode.PublicLifeRecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.EscortRoadReport, StringComparison.Ordinal)), Is.True);
        Assert.That(settlementNode.PublicLifeRecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)), Is.True);
        Assert.That(settlementNode.PublicLifeSummary, Is.Not.Empty);
        Assert.That(settlementNode.PublicLifeSummary.Length, Is.GreaterThan(0));
    }
}
