using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Presentation.Unity;

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
        Assert.That(bundle.Debug.EnabledModules.Any(static module => string.Equals(module.ModuleKey, KnownModuleKeys.NarrativeProjection, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.Debug.ModuleInspectors.Any(static inspector => string.Equals(inspector.ModuleKey, KnownModuleKeys.NarrativeProjection, StringComparison.Ordinal)), Is.True);
        Assert.That(bundle.Debug.RecentDiffEntries, Is.Not.Empty);
        Assert.That(bundle.Debug.RecentDomainEvents, Is.Not.Empty);
        Assert.That(bundle.Debug.LatestMetrics.DiffEntryCount, Is.GreaterThan(0));
        Assert.That(bundle.Debug.LatestMetrics.DomainEventCount, Is.GreaterThan(0));
        Assert.That(bundle.Debug.LatestMetrics.NotificationCount, Is.EqualTo(bundle.Notifications.Count));
        Assert.That(bundle.Debug.LatestMetrics.SavePayloadBytes, Is.GreaterThan(0));
        Assert.That(bundle.Debug.Invariants, Does.Contain("Module boundary validation passed."));
        Assert.That(shell.Debug.DiagnosticsSchemaLabel, Is.EqualTo("v1"));
        Assert.That(shell.Debug.LatestMetrics.NotificationCount, Is.EqualTo(bundle.Notifications.Count));
        Assert.That(shell.Debug.ModuleInspectors.Count, Is.EqualTo(bundle.Debug.ModuleInspectors.Count));
        Assert.That(shell.Debug.DiffTraces.Count, Is.EqualTo(bundle.Debug.RecentDiffEntries.Count));
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
        Assert.That(report.Samples.All(static sample => !string.IsNullOrWhiteSpace(sample.ReplayHash)), Is.True);
        Assert.That(
            report.Samples.Any(static sample => sample.Metrics.NotificationCount == NarrativeProjectionModule.NotificationRetentionLimit),
            Is.True);
    }

    [Test]
    public void ActiveM2Bootstrap_RemainsIsolatedFromM3PreflightModules()
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
}
