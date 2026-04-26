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
public sealed partial class M2LiteIntegrationTests
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

                KnownModuleKeys.PersonRegistry,

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

            KnownModuleKeys.PersonRegistry,

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

                KnownModuleKeys.PersonRegistry,

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

        Assert.That(bundle.HallDocket.LeadItem.LaneKey, Is.EqualTo(HallDocketLaneKeys.Family));

        Assert.That(bundle.HallDocket.LeadItem.Headline, Is.Not.Empty);

        Assert.That(bundle.HallDocket.LeadItem.GuidanceSummary, Is.Not.Empty);

        Assert.That(bundle.HallDocket.LeadItem.OrderingSummary, Is.Not.Empty);

        Assert.That(bundle.HallDocket.LeadItem.SourceProjectionKeys, Does.Contain(HallDocketSourceProjectionKeys.Clans));

        Assert.That(bundle.HallDocket.LeadItem.SourceModuleKeys, Does.Contain(KnownModuleKeys.FamilyCore));

    }


    [Test]

    public void M2Bundle_SurfacesLivingSocietyPressureAndInfluenceFootprint()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260422);

        simulation.AdvanceMonths(1);


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        HouseholdSocialPressureSnapshot sponsoredPressure = bundle.HouseholdSocialPressures.Single(static pressure => pressure.SponsorClanId.HasValue);

        InfluenceReachSnapshot ownHouseholdReach = bundle.InfluenceFootprint.Reaches.Single(static reach => reach.ReachKey == InfluenceReachKeys.OwnHousehold);

        InfluenceReachSnapshot observedHouseholdReach = bundle.InfluenceFootprint.Reaches.Single(static reach => reach.ReachKey == InfluenceReachKeys.ObservedHouseholds);

        InfluenceReachSnapshot lineageReach = bundle.InfluenceFootprint.Reaches.Single(static reach => reach.ReachKey == InfluenceReachKeys.Lineage);

        InfluenceReachSnapshot yamenReach = bundle.InfluenceFootprint.Reaches.Single(static reach => reach.ReachKey == InfluenceReachKeys.Yamen);


        Assert.That(bundle.Households, Has.Count.GreaterThanOrEqualTo(2));

        Assert.That(bundle.HouseholdSocialPressures, Has.Count.EqualTo(bundle.Households.Count));

        Assert.That(sponsoredPressure.SourceModuleKeys, Does.Contain(KnownModuleKeys.PopulationAndHouseholds));

        Assert.That(sponsoredPressure.SourceModuleKeys, Does.Contain(KnownModuleKeys.FamilyCore));

        Assert.That(sponsoredPressure.Signals.Any(static signal => string.Equals(signal.SignalKey, HouseholdSocialPressureSignalKeys.DebtAndSubsistence, StringComparison.Ordinal)), Is.True);

        Assert.That(sponsoredPressure.Signals.Any(static signal => string.Equals(signal.SignalKey, HouseholdSocialPressureSignalKeys.LineageProtection, StringComparison.Ordinal)), Is.True);

        Assert.That(sponsoredPressure.PrimaryDriftKey, Is.Not.Empty);

        Assert.That(sponsoredPressure.IsPlayerAnchor, Is.True);

        Assert.That(sponsoredPressure.AttachmentSummary, Is.Not.Empty);

        Assert.That(sponsoredPressure.VisibleChainSummary, Is.Not.Empty);

        Assert.That(ownHouseholdReach.IsActive, Is.True);

        Assert.That(ownHouseholdReach.IsPlayerAnchor, Is.True);

        Assert.That(ownHouseholdReach.HasLocalAgency, Is.True);

        Assert.That(ownHouseholdReach.HasCommandAffordance, Is.True);
        Assert.That(ownHouseholdReach.CommandSummary, Does.Contain("暂缩夜行"));
        Assert.That(ownHouseholdReach.CommandSummary, Does.Contain("凑钱赔脚户"));
        Assert.That(ownHouseholdReach.CommandSummary, Does.Contain("遣少丁递信"));

        Assert.That(observedHouseholdReach.IsActive, Is.True);

        Assert.That(observedHouseholdReach.HasLocalAgency, Is.False);

        Assert.That(observedHouseholdReach.HasCommandAffordance, Is.False);

        Assert.That(lineageReach.IsActive, Is.True);

        Assert.That(lineageReach.HasCommandAffordance, Is.True);

        Assert.That(yamenReach.IsActive, Is.False);

        Assert.That(yamenReach.HasCommandAffordance, Is.False);

        Assert.That(bundle.InfluenceFootprint.EntryPositionLabel, Is.Not.Empty);

        Assert.That(bundle.InfluenceFootprint.AnchorHouseholdName, Is.EqualTo(sponsoredPressure.HouseholdName));

        Assert.That(bundle.InfluenceFootprint.AnchorHouseholdSummary, Is.Not.Empty);

        Assert.That(bundle.InfluenceFootprint.Summary, Is.Not.Empty);

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

        Assert.That(bundle.Debug.CurrentScale.EnabledModuleCount, Is.EqualTo(9));

        Assert.That(bundle.Debug.CurrentScale.SettlementCount, Is.EqualTo(1));

        Assert.That(bundle.Debug.CurrentScale.ClanCount, Is.EqualTo(1));

        Assert.That(bundle.Debug.CurrentScale.HouseholdCount, Is.GreaterThanOrEqualTo(2));

        Assert.That(bundle.Debug.CurrentScale.AverageHouseholdsPerSettlement, Is.GreaterThanOrEqualTo(2));

        Assert.That(bundle.GovernanceSettlements, Is.Empty);

        Assert.That(bundle.GovernanceFocus.UrgencyScore, Is.EqualTo(0));

        Assert.That(bundle.GovernanceFocus.LeadSummary, Is.Empty);

        Assert.That(bundle.GovernanceDocket.Headline, Is.Empty);

        Assert.That(bundle.GovernanceDocket.PhaseLabel, Is.Empty);

        Assert.That(bundle.GovernanceDocket.PhaseSummary, Is.Empty);

        Assert.That(bundle.GovernanceDocket.HandlingSummary, Is.Empty);

        Assert.That(bundle.GovernanceDocket.GuidanceSummary, Is.Empty);

        Assert.That(bundle.GovernanceDocket.HasRelatedNotification, Is.False);

        Assert.That(bundle.GovernanceDocket.HasRecentReceipt, Is.False);

        Assert.That(bundle.Debug.CurrentHotspots, Is.Empty);

        Assert.That(bundle.Debug.CurrentInteractionPressure.ActivatedResponseSettlements, Is.EqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.PeakSuppressionDemand, Is.EqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.OrderInterventionCarryoverSettlements, Is.EqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.OrderAdministrativeAftermathSettlements, Is.EqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.ShieldingDominantSettlements, Is.EqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.BacklashDominantSettlements, Is.EqualTo(0));

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

        Assert.That(report.PeakScaleMetrics.EnabledModuleCount, Is.EqualTo(9));

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

                // Phase 1c: +~1 KB headroom for WorldSettlements schema v3

                // (Routes + CurrentSeason + per-settlement NodeKind/Visibility/

                // EcoZone/NeighborIds/ParentAdministrativeId). SPEC §13.

                // Step 2-A / A2: +4 kin per clan in FamilyCore + PersonRegistry

                // seeds (elder / spouse / youth / child) raises the peak

                // payload by ~1 KB on this minimal-world slice.

                // Step 2-A / A6: birth gate 解卡后 240 月沙盘真实添丁，新生儿 +

                // 父母 ChildrenIds + PersonRegistry 条目 约再增 ~6 KB 峰值。

                // Pressure tempering kernel: SocialMemory schema v3 persists
                // clan climates and personal tempering ledgers, adding about
                // 4 KB on this minimal long-run slice.
                SavePayloadBytes = 66000,

            },

            GrowthCeiling = new ObservabilityMetricsSnapshot

            {

                DiffEntryCount = 18,

                DomainEventCount = 16,

                NotificationCount = NarrativeProjectionModule.NotificationRetentionLimit,

                SavePayloadBytes = 58000,

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

        Assert.That(report.PeakScaleMetrics.EnabledModuleCount, Is.EqualTo(11));

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

                // Pressure tempering plus current thin owner-lane handoffs add
                // bounded per-clan/per-person payload to the stress slice.
                SavePayloadBytes = 128000,

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

            PeakHotspotScoreCeiling = 460,

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

                // Pressure tempering kernel: schema v3 SocialMemory ledgers add
                // bounded per-clan/per-person payload over 360 months.
                SavePayloadBytes = 140000,

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

            PeakHotspotScoreCeiling = 460,

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


}
