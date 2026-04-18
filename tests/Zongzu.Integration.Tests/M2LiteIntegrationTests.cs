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
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));
        Assert.That(saveRoot.ModuleStates.All(static pair => string.Equals(pair.Key, pair.Value.ModuleKey, StringComparison.Ordinal)), Is.True);
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
        Assert.That(bundle.Debug.CurrentScale.EnabledModuleCount, Is.EqualTo(7));
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
        Assert.That(report.PeakScaleMetrics.EnabledModuleCount, Is.EqualTo(7));
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
        Assert.That(report.PeakScaleMetrics.EnabledModuleCount, Is.EqualTo(9));
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
            Assert.That(disorder.LastPressureReason, Does.Contain("Activated guards"));
        }

        foreach (LocalForcePoolSnapshot calm in calmResponses)
        {
            SettlementDisorderState disorder = orderState.Settlements.Single(settlement => settlement.SettlementId == calm.SettlementId);
            Assert.That(calm.OrderSupportLevel, Is.EqualTo(0));
            Assert.That(disorder.LastPressureReason, Does.Not.Contain("Activated guards"));
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
        Assert.That(tradeState.Clans.Single().LastExplanation, Does.Contain("order pressure"));
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
        Assert.That(orderState.Settlements.Single().LastPressureReason, Does.Contain("Activated guards"));
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
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(bundle.Debug.LoadMigration.LoadOriginLabel, Is.EqualTo("SaveLoad"));
        Assert.That(bundle.Debug.LoadMigration.WasMigrationApplied, Is.True);
        Assert.That(bundle.Debug.LoadMigration.Steps.Any(static step => step.ScopeLabel == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 1 && step.TargetVersion == 2), Is.True);
        Assert.That(bundle.Debug.LoadMigration.Steps.Any(static step => step.ScopeLabel == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 2 && step.TargetVersion == 3), Is.True);
        Assert.That(bundle.Debug.LoadMigration.StepCount, Is.EqualTo(2));
        Assert.That(bundle.Debug.LoadMigration.ConsistencyPassed, Is.True);
        Assert.That(bundle.Debug.LoadMigration.ConsistencySummary, Does.Contain("module envelopes preserved"));
        Assert.That(bundle.Debug.LoadMigration.Warnings, Is.Empty);
        Assert.That(bundle.Debug.CurrentInteractionPressure.ActivatedResponseSettlements, Is.GreaterThan(0));
        Assert.That(bundle.Debug.CurrentPressureDistribution.StressedSettlements + bundle.Debug.CurrentPressureDistribution.CrisisSettlements, Is.GreaterThanOrEqualTo(0));
        Assert.That(bundle.Debug.CurrentScale.SettlementCount, Is.EqualTo(1));
        Assert.That(bundle.Debug.CurrentHotspots, Is.Not.Empty);
        Assert.That(bundle.Debug.CurrentHotspots[0].SettlementName, Is.Not.Empty);
        Assert.That(bundle.Debug.CurrentPayloadSummary.LargestModulePayloadBytes, Is.GreaterThan(0));
        Assert.That(bundle.Debug.TopPayloadModules, Is.Not.Empty);
        Assert.That(shell.Debug.Migration.LoadOriginLabel, Is.EqualTo("SaveLoad"));
        Assert.That(shell.Debug.Migration.MigrationStatusLabel, Is.EqualTo("Consistency passed"));
        Assert.That(shell.Debug.Migration.MigrationConsistencySummary, Does.Contain("module envelopes preserved"));
        Assert.That(shell.Debug.Migration.MigrationSteps, Does.Contain($"{KnownModuleKeys.ConflictAndForce}:1->2"));
        Assert.That(shell.Debug.Migration.MigrationSteps, Does.Contain($"{KnownModuleKeys.ConflictAndForce}:2->3"));
        Assert.That(shell.Debug.Hotspots.CurrentHotspots.Count, Is.EqualTo(bundle.Debug.CurrentHotspots.Count));
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
        Assert.That(officeState.People.Any(static career => !string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask) && !string.Equals(career.CurrentAdministrativeTask, "候补听选", StringComparison.Ordinal)), Is.True);
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
        Assert.That(shell.GreatHall.GovernanceSummary, Does.Contain("人在官途"));
        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => settlement.GovernanceSummary.Contains("差遣", StringComparison.Ordinal)), Is.True);
        Assert.That(shell.Office.Appointments.Any(static appointment => appointment.OfficeTitle != "未授官"), Is.True);
        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.PressureSummary)), Is.True);
        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.PetitionOutcomeCategory)), Is.True);
        Assert.That(shell.Office.Jurisdictions.Any(static jurisdiction => jurisdiction.PetitionSummary.Contains("积案", StringComparison.Ordinal)), Is.True);
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
        Assert.That(bundle.Campaigns.Any(static campaign => campaign.AnchorSettlementName == "Lanxi"), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommanderSummary)), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => campaign.Routes.Count > 0), Is.True);
        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.MobilizationWindowLabel)), Is.True);
        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);
        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)), Is.True);
        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.ActiveDirectiveLabel)), Is.True);
        Assert.That(bundle.Notifications.Any(static notification => notification.SourceModuleKey == KnownModuleKeys.WarfareCampaign), Is.True);
        Assert.That(bundle.Notifications.Any(static notification => notification.Surface == NarrativeSurface.DeskSandbox), Is.True);
        Assert.That(shell.GreatHall.WarfareSummary, Does.Contain("在案行营"));
        Assert.That(shell.GreatHall.WarfareSummary, Does.Contain("案头呈"));
        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => settlement.CampaignSummary.Contains("军务沙盘", StringComparison.Ordinal)), Is.True);
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
        Assert.That(result.DirectiveLabel, Is.EqualTo("催督粮道"));
        Assert.That(result.Summary, Does.Contain("催督粮道"));
        Assert.That(campaign.ActiveDirectiveLabel, Is.EqualTo("催督粮道"));
        Assert.That(campaign.ActiveDirectiveSummary, Does.Contain("粮道"));
        Assert.That(campaign.LastDirectiveTrace, Does.Contain("已收到军令"));
        Assert.That(signal.ActiveDirectiveLabel, Is.EqualTo("催督粮道"));
        Assert.That(signal.ActiveDirectiveSummary, Does.Contain("粮道"));
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
        Assert.That(tradeState.Clans.Any(static trade => trade.LastExplanation.Contains("Campaign pressure around Lanxi", StringComparison.Ordinal)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.LastPressureReason.Contains("Campaign spillover from Lanxi", StringComparison.Ordinal)), Is.True);
        Assert.That(officeState.People.Any(static career => career.LastPetitionOutcome.Contains("案牍骤涌", StringComparison.Ordinal)), Is.True);
        Assert.That(socialState.Memories.Any(static memory => memory.Kind.StartsWith("campaign-", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.WorldSettlements && entry.Description.Contains("Campaign spillover", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.PopulationAndHouseholds && entry.Description.Contains("Campaign spillover", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.FamilyCore && entry.Description.Contains("Campaign spillover", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.TradeAndIndustry && entry.Description.Contains("Campaign aftermath", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.OrderAndBanditry && entry.Description.Contains("Campaign spillover", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.OfficeAndCareer && entry.Description.Contains("Campaign spillover", StringComparison.Ordinal)), Is.True);
        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations && entry.Description.Contains("Campaign spillover", StringComparison.Ordinal)), Is.True);
        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("Lanxi"));
        Assert.That(shell.DeskSandbox.Settlements.Single(static settlement => string.Equals(settlement.SettlementName, "Lanxi", StringComparison.Ordinal)).AftermathSummary, Does.Contain("战后案牍"));
        Assert.That(shell.Warfare.CampaignBoards.Single(static board => string.Equals(board.SettlementLabel, "Lanxi", StringComparison.Ordinal)).AftermathDocketSummary, Does.Contain("军机案今并载"));
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
        Assert.That(campaignForce.LastCampaignFalloutTrace, Does.Contain("Campaign fallout from Lanxi"));
        Assert.That(campaignForce.LastConflictTrace, Does.Contain("Campaign fallout from Lanxi"));
        Assert.That(campaignForce.Readiness, Is.LessThan(governanceForce.Readiness));
        Assert.That(campaignForce.CommandCapacity, Is.LessThanOrEqualTo(governanceForce.CommandCapacity));
        Assert.That(campaignSimulation.LastMonthResult!.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.ConflictAndForce && entry.Description.Contains("Campaign aftermath", StringComparison.Ordinal)), Is.True);
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
        Assert.That(shell.GreatHall.GovernanceSummary, Is.EqualTo("案头暂无官署呈报。"));
        Assert.That(shell.Office.Summary, Is.EqualTo("案头暂无官署牍报。"));
        Assert.That(shell.Office.Appointments, Is.Empty);
        Assert.That(shell.Office.Jurisdictions, Is.Empty);
        Assert.That(bundle.Campaigns, Is.Empty);
        Assert.That(bundle.CampaignMobilizationSignals, Is.Empty);
        Assert.That(shell.GreatHall.WarfareSummary, Is.EqualTo("暂无军务沙盘投影。"));
        Assert.That(shell.Warfare.Summary, Is.EqualTo("暂无军务沙盘投影。"));
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
}
