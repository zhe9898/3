using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Persistence;
using Zongzu.Scheduler;

namespace Zongzu.Application;

public sealed class SimulationDiagnosticsBudget
{
    public ObservabilityMetricsSnapshot PeakCeiling { get; set; } = new();

    public ObservabilityMetricsSnapshot GrowthCeiling { get; set; } = new();

    public InteractionPressureMetricsSnapshot InteractionPressureCeiling { get; set; } = new();

    public int PeakHotspotScoreCeiling { get; set; }
}

public sealed class SimulationDiagnosticsBudgetEvaluation
{
    public bool WithinBudget { get; set; }

    public IReadOnlyList<string> Violations { get; set; } = [];
}

public sealed class M2DiagnosticsMonthSample
{
    public int MonthIndex { get; set; }

    public int SimulatedYear { get; set; }

    public int SimulatedMonth { get; set; }

    public ObservabilityMetricsSnapshot Metrics { get; set; } = new();

    public IReadOnlyList<ModuleActivityMetricsSnapshot> ModuleActivity { get; set; } = [];

    public InteractionPressureMetricsSnapshot InteractionPressure { get; set; } = new();

    public SettlementPressureDistributionSnapshot PressureDistribution { get; set; } = new();

    public RuntimeScaleMetricsSnapshot ScaleMetrics { get; set; } = new();

    public IReadOnlyList<SettlementInteractionHotspotSnapshot> TopHotspots { get; set; } = [];

    public RuntimePayloadSummarySnapshot PayloadSummary { get; set; } = new();

    public IReadOnlyList<ModulePayloadFootprintSnapshot> TopPayloadModules { get; set; } = [];

    public string ReplayHash { get; set; } = string.Empty;
}

public sealed class ModuleActivityMetricsSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public int DiffEntryCount { get; set; }

    public int DomainEventCount { get; set; }
}

public sealed class M2DiagnosticsReport
{
    public int DiagnosticsSchemaVersion { get; set; }

    public int Seed { get; set; }

    public int MonthsSimulated { get; set; }

    public ObservabilityMetricsSnapshot InitialMetrics { get; set; } = new();

    public ObservabilityMetricsSnapshot PeakMetrics { get; set; } = new();

    public ObservabilityMetricsSnapshot FinalMetrics { get; set; } = new();

    public ObservabilityMetricsSnapshot GrowthMetrics { get; set; } = new();

    public IReadOnlyList<ModuleActivityMetricsSnapshot> PeakModuleActivity { get; set; } = [];

    public InteractionPressureMetricsSnapshot PeakInteractionPressure { get; set; } = new();

    public SettlementPressureDistributionSnapshot PeakPressureDistribution { get; set; } = new();

    public RuntimeScaleMetricsSnapshot PeakScaleMetrics { get; set; } = new();

    public RuntimePayloadSummarySnapshot PeakPayloadSummary { get; set; } = new();

    public IReadOnlyList<SettlementInteractionHotspotSnapshot> PeakHotspots { get; set; } = [];

    public IReadOnlyList<ModulePayloadFootprintSnapshot> PeakPayloadModules { get; set; } = [];

    public int PeakHotspotScore { get; set; }

    public bool RetentionLimitReached { get; set; }

    public IReadOnlyList<M2DiagnosticsMonthSample> Samples { get; set; } = [];
}

public sealed class M2DiagnosticsSweepReport
{
    public int DiagnosticsSchemaVersion { get; set; }

    public int MonthsSimulated { get; set; }

    public ObservabilityMetricsSnapshot PeakMetrics { get; set; } = new();

    public ObservabilityMetricsSnapshot PeakGrowthMetrics { get; set; } = new();

    public InteractionPressureMetricsSnapshot PeakInteractionPressure { get; set; } = new();

    public SettlementPressureDistributionSnapshot PeakPressureDistribution { get; set; } = new();

    public RuntimeScaleMetricsSnapshot PeakScaleMetrics { get; set; } = new();

    public RuntimePayloadSummarySnapshot PeakPayloadSummary { get; set; } = new();

    public int PeakHotspotScore { get; set; }

    public bool RetentionLimitReached { get; set; }

    public SimulationDiagnosticsBudgetEvaluation BudgetEvaluation { get; set; } = new();

    public IReadOnlyList<M2DiagnosticsReport> Runs { get; set; } = [];
}

public sealed class M2DiagnosticsHarness
{
    private readonly PresentationReadModelBuilder _readModelBuilder = new();
    private readonly SaveCodec _saveCodec = new();

    public M2DiagnosticsReport Run(int seed, int monthCount)
    {
        return Run(SimulationBootstrapper.CreateM2Bootstrap, seed, monthCount);
    }

    public M2DiagnosticsReport Run(Func<long, GameSimulation> bootstrapFactory, int seed, int monthCount)
    {
        if (monthCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthCount), "monthCount must be positive.");
        }

        ArgumentNullException.ThrowIfNull(bootstrapFactory);

        GameSimulation simulation = bootstrapFactory(seed);
        List<M2DiagnosticsMonthSample> samples = new(monthCount);

        ObservabilityMetricsSnapshot initialMetrics = CollectMetrics(simulation);
        ObservabilityMetricsSnapshot peakMetrics = new();
        ObservabilityMetricsSnapshot finalMetrics = new();
        Dictionary<string, ModuleActivityMetricsSnapshot> peakModuleActivity = new(StringComparer.Ordinal);
        Dictionary<SettlementId, SettlementInteractionHotspotSnapshot> peakHotspots = new();
        Dictionary<string, ModulePayloadFootprintSnapshot> peakPayloadModules = new(StringComparer.Ordinal);
        InteractionPressureMetricsSnapshot peakInteractionPressure = new();
        SettlementPressureDistributionSnapshot peakPressureDistribution = new();
        RuntimeScaleMetricsSnapshot peakScaleMetrics = new();
        RuntimePayloadSummarySnapshot peakPayloadSummary = new();
        bool retentionLimitReached = false;

        for (int monthIndex = 1; monthIndex <= monthCount; monthIndex += 1)
        {
            SimulationMonthResult result = simulation.AdvanceOneMonth();
            SaveRoot saveRoot = simulation.ExportSave();
            int savePayloadBytes = _saveCodec.Encode(saveRoot).Length;
            int notificationCount = _readModelBuilder.BuildForM2(simulation).Notifications.Count;
            InteractionPressureMetricsSnapshot interactionPressure = RuntimeObservabilityCollector.CollectInteractionPressure(simulation);
            SettlementPressureDistributionSnapshot pressureDistribution = RuntimeObservabilityCollector.CollectPressureDistribution(simulation);
            RuntimeScaleMetricsSnapshot scaleMetrics = RuntimeObservabilityCollector.CollectScaleMetrics(simulation, saveRoot, notificationCount);
            RuntimePayloadSummarySnapshot payloadSummary = RuntimeObservabilityCollector.CollectPayloadSummary(saveRoot);

            M2DiagnosticsMonthSample sample = new()
            {
                MonthIndex = monthIndex,
                SimulatedYear = result.SimulatedDate.Year,
                SimulatedMonth = result.SimulatedDate.Month,
                Metrics = new ObservabilityMetricsSnapshot
                {
                    DiffEntryCount = result.Diff.Entries.Count,
                    DomainEventCount = result.DomainEvents.Count,
                    NotificationCount = notificationCount,
                    SavePayloadBytes = savePayloadBytes,
                },
                ModuleActivity = BuildModuleActivity(result),
                InteractionPressure = interactionPressure,
                PressureDistribution = pressureDistribution,
                ScaleMetrics = scaleMetrics,
                TopHotspots = RuntimeObservabilityCollector.CollectTopHotspots(simulation),
                PayloadSummary = payloadSummary,
                TopPayloadModules = RuntimeObservabilityCollector.CollectTopPayloadModules(saveRoot),
                ReplayHash = simulation.ReplayHash,
            };

            samples.Add(sample);
            peakMetrics.DiffEntryCount = Math.Max(peakMetrics.DiffEntryCount, sample.Metrics.DiffEntryCount);
            peakMetrics.DomainEventCount = Math.Max(peakMetrics.DomainEventCount, sample.Metrics.DomainEventCount);
            peakMetrics.NotificationCount = Math.Max(peakMetrics.NotificationCount, sample.Metrics.NotificationCount);
            peakMetrics.SavePayloadBytes = Math.Max(peakMetrics.SavePayloadBytes, sample.Metrics.SavePayloadBytes);
            UpdatePeakModuleActivity(peakModuleActivity, sample.ModuleActivity);
            peakInteractionPressure = MaxInteractionPressure(peakInteractionPressure, sample.InteractionPressure);
            peakPressureDistribution = MaxPressureDistribution(peakPressureDistribution, sample.PressureDistribution);
            peakScaleMetrics = MaxScaleMetrics(peakScaleMetrics, sample.ScaleMetrics);
            peakPayloadSummary = MaxPayloadSummary(peakPayloadSummary, sample.PayloadSummary);
            UpdatePeakHotspots(peakHotspots, sample.TopHotspots);
            UpdatePeakPayloadModules(peakPayloadModules, sample.TopPayloadModules);
            retentionLimitReached |= sample.Metrics.NotificationCount >= NarrativeProjectionModule.NotificationRetentionLimit;
            finalMetrics = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = sample.Metrics.DiffEntryCount,
                DomainEventCount = sample.Metrics.DomainEventCount,
                NotificationCount = sample.Metrics.NotificationCount,
                SavePayloadBytes = sample.Metrics.SavePayloadBytes,
            };
        }

        return new M2DiagnosticsReport
        {
            DiagnosticsSchemaVersion = 1,
            Seed = seed,
            MonthsSimulated = monthCount,
            InitialMetrics = initialMetrics,
            PeakMetrics = peakMetrics,
            FinalMetrics = finalMetrics,
            GrowthMetrics = ComputeGrowth(initialMetrics, finalMetrics),
            PeakModuleActivity = peakModuleActivity.Values.OrderBy(static metric => metric.ModuleKey, StringComparer.Ordinal).ToArray(),
            PeakInteractionPressure = peakInteractionPressure,
            PeakPressureDistribution = peakPressureDistribution,
            PeakScaleMetrics = peakScaleMetrics,
            PeakPayloadSummary = peakPayloadSummary,
            PeakHotspots = peakHotspots.Values
                .OrderByDescending(static hotspot => hotspot.HotspotScore)
                .ThenBy(static hotspot => hotspot.SettlementName, StringComparer.Ordinal)
                .ToArray(),
            PeakPayloadModules = peakPayloadModules.Values
                .OrderByDescending(static footprint => footprint.PayloadBytes)
                .ThenBy(static footprint => footprint.ModuleKey, StringComparer.Ordinal)
                .ToArray(),
            PeakHotspotScore = peakHotspots.Count == 0 ? 0 : peakHotspots.Values.Max(static hotspot => hotspot.HotspotScore),
            RetentionLimitReached = retentionLimitReached,
            Samples = samples,
        };
    }

    public M2DiagnosticsSweepReport RunMany(
        IReadOnlyList<int> seeds,
        int monthCount,
        SimulationDiagnosticsBudget? budget = null,
        Func<long, GameSimulation>? bootstrapFactory = null)
    {
        ArgumentNullException.ThrowIfNull(seeds);
        if (seeds.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seeds), "At least one seed is required.");
        }

        Func<long, GameSimulation> resolvedBootstrapFactory = bootstrapFactory ?? SimulationBootstrapper.CreateM2Bootstrap;
        M2DiagnosticsReport[] runs = seeds
            .Select(seed => Run(resolvedBootstrapFactory, seed, monthCount))
            .ToArray();

        ObservabilityMetricsSnapshot peakMetrics = new();
        ObservabilityMetricsSnapshot peakGrowthMetrics = new();
        InteractionPressureMetricsSnapshot peakInteractionPressure = new();
        SettlementPressureDistributionSnapshot peakPressureDistribution = new();
        RuntimeScaleMetricsSnapshot peakScaleMetrics = new();
        RuntimePayloadSummarySnapshot peakPayloadSummary = new();
        int peakHotspotScore = 0;
        foreach (M2DiagnosticsReport run in runs)
        {
            peakMetrics = MaxMetrics(peakMetrics, run.PeakMetrics);
            peakGrowthMetrics = MaxMetrics(peakGrowthMetrics, run.GrowthMetrics);
            peakInteractionPressure = MaxInteractionPressure(peakInteractionPressure, run.PeakInteractionPressure);
            peakPressureDistribution = MaxPressureDistribution(peakPressureDistribution, run.PeakPressureDistribution);
            peakScaleMetrics = MaxScaleMetrics(peakScaleMetrics, run.PeakScaleMetrics);
            peakPayloadSummary = MaxPayloadSummary(peakPayloadSummary, run.PeakPayloadSummary);
            peakHotspotScore = Math.Max(peakHotspotScore, run.PeakHotspotScore);
        }

        return new M2DiagnosticsSweepReport
        {
            DiagnosticsSchemaVersion = 1,
            MonthsSimulated = monthCount,
            PeakMetrics = peakMetrics,
            PeakGrowthMetrics = peakGrowthMetrics,
            PeakInteractionPressure = peakInteractionPressure,
            PeakPressureDistribution = peakPressureDistribution,
            PeakScaleMetrics = peakScaleMetrics,
            PeakPayloadSummary = peakPayloadSummary,
            PeakHotspotScore = peakHotspotScore,
            RetentionLimitReached = runs.Any(static run => run.RetentionLimitReached),
            BudgetEvaluation = EvaluateBudget(peakMetrics, peakGrowthMetrics, peakInteractionPressure, peakHotspotScore, budget),
            Runs = runs,
        };
    }

    private ObservabilityMetricsSnapshot CollectMetrics(GameSimulation simulation)
    {
        int savePayloadBytes = _saveCodec.Encode(simulation.ExportSave()).Length;
        int notificationCount = _readModelBuilder.BuildForM2(simulation).Notifications.Count;

        return new ObservabilityMetricsSnapshot
        {
            DiffEntryCount = simulation.LastMonthResult?.Diff.Entries.Count ?? 0,
            DomainEventCount = simulation.LastMonthResult?.DomainEvents.Count ?? 0,
            NotificationCount = notificationCount,
            SavePayloadBytes = savePayloadBytes,
        };
    }

    private static ObservabilityMetricsSnapshot ComputeGrowth(ObservabilityMetricsSnapshot initialMetrics, ObservabilityMetricsSnapshot finalMetrics)
    {
        return new ObservabilityMetricsSnapshot
        {
            DiffEntryCount = Math.Max(0, finalMetrics.DiffEntryCount - initialMetrics.DiffEntryCount),
            DomainEventCount = Math.Max(0, finalMetrics.DomainEventCount - initialMetrics.DomainEventCount),
            NotificationCount = Math.Max(0, finalMetrics.NotificationCount - initialMetrics.NotificationCount),
            SavePayloadBytes = Math.Max(0, finalMetrics.SavePayloadBytes - initialMetrics.SavePayloadBytes),
        };
    }

    private static ObservabilityMetricsSnapshot MaxMetrics(ObservabilityMetricsSnapshot current, ObservabilityMetricsSnapshot candidate)
    {
        return new ObservabilityMetricsSnapshot
        {
            DiffEntryCount = Math.Max(current.DiffEntryCount, candidate.DiffEntryCount),
            DomainEventCount = Math.Max(current.DomainEventCount, candidate.DomainEventCount),
            NotificationCount = Math.Max(current.NotificationCount, candidate.NotificationCount),
            SavePayloadBytes = Math.Max(current.SavePayloadBytes, candidate.SavePayloadBytes),
        };
    }

    private static InteractionPressureMetricsSnapshot MaxInteractionPressure(
        InteractionPressureMetricsSnapshot current,
        InteractionPressureMetricsSnapshot candidate)
    {
        return new InteractionPressureMetricsSnapshot
        {
            ActiveConflictSettlements = Math.Max(current.ActiveConflictSettlements, candidate.ActiveConflictSettlements),
            ActivatedResponseSettlements = Math.Max(current.ActivatedResponseSettlements, candidate.ActivatedResponseSettlements),
            SupportedOrderSettlements = Math.Max(current.SupportedOrderSettlements, candidate.SupportedOrderSettlements),
            HighSuppressionDemandSettlements = Math.Max(current.HighSuppressionDemandSettlements, candidate.HighSuppressionDemandSettlements),
            AverageSuppressionDemand = Math.Max(current.AverageSuppressionDemand, candidate.AverageSuppressionDemand),
            PeakSuppressionDemand = Math.Max(current.PeakSuppressionDemand, candidate.PeakSuppressionDemand),
            HighBanditThreatSettlements = Math.Max(current.HighBanditThreatSettlements, candidate.HighBanditThreatSettlements),
            OrderInterventionCarryoverSettlements = Math.Max(current.OrderInterventionCarryoverSettlements, candidate.OrderInterventionCarryoverSettlements),
            ShieldingDominantSettlements = Math.Max(current.ShieldingDominantSettlements, candidate.ShieldingDominantSettlements),
            BacklashDominantSettlements = Math.Max(current.BacklashDominantSettlements, candidate.BacklashDominantSettlements),
        };
    }

    private static SettlementPressureDistributionSnapshot MaxPressureDistribution(
        SettlementPressureDistributionSnapshot current,
        SettlementPressureDistributionSnapshot candidate)
    {
        return new SettlementPressureDistributionSnapshot
        {
            CalmSettlements = Math.Max(current.CalmSettlements, candidate.CalmSettlements),
            WatchedSettlements = Math.Max(current.WatchedSettlements, candidate.WatchedSettlements),
            StressedSettlements = Math.Max(current.StressedSettlements, candidate.StressedSettlements),
            CrisisSettlements = Math.Max(current.CrisisSettlements, candidate.CrisisSettlements),
        };
    }

    private static RuntimeScaleMetricsSnapshot MaxScaleMetrics(RuntimeScaleMetricsSnapshot current, RuntimeScaleMetricsSnapshot candidate)
    {
        return new RuntimeScaleMetricsSnapshot
        {
            EnabledModuleCount = Math.Max(current.EnabledModuleCount, candidate.EnabledModuleCount),
            SavedModuleCount = Math.Max(current.SavedModuleCount, candidate.SavedModuleCount),
            SettlementCount = Math.Max(current.SettlementCount, candidate.SettlementCount),
            ClanCount = Math.Max(current.ClanCount, candidate.ClanCount),
            HouseholdCount = Math.Max(current.HouseholdCount, candidate.HouseholdCount),
            AcademyCount = Math.Max(current.AcademyCount, candidate.AcademyCount),
            RouteCount = Math.Max(current.RouteCount, candidate.RouteCount),
            NotificationCount = Math.Max(current.NotificationCount, candidate.NotificationCount),
            NotificationUtilizationPercent = Math.Max(current.NotificationUtilizationPercent, candidate.NotificationUtilizationPercent),
            SavePayloadBytesPerSettlement = Math.Max(current.SavePayloadBytesPerSettlement, candidate.SavePayloadBytesPerSettlement),
            AverageHouseholdsPerSettlement = Math.Max(current.AverageHouseholdsPerSettlement, candidate.AverageHouseholdsPerSettlement),
        };
    }

    private static RuntimePayloadSummarySnapshot MaxPayloadSummary(
        RuntimePayloadSummarySnapshot current,
        RuntimePayloadSummarySnapshot candidate)
    {
        return new RuntimePayloadSummarySnapshot
        {
            TotalModulePayloadBytes = Math.Max(current.TotalModulePayloadBytes, candidate.TotalModulePayloadBytes),
            LargestModuleKey = candidate.LargestModulePayloadBytes >= current.LargestModulePayloadBytes
                ? candidate.LargestModuleKey
                : current.LargestModuleKey,
            LargestModulePayloadBytes = Math.Max(current.LargestModulePayloadBytes, candidate.LargestModulePayloadBytes),
            LargestModuleShareBasisPoints = Math.Max(current.LargestModuleShareBasisPoints, candidate.LargestModuleShareBasisPoints),
        };
    }

    private static SimulationDiagnosticsBudgetEvaluation EvaluateBudget(
        ObservabilityMetricsSnapshot peakMetrics,
        ObservabilityMetricsSnapshot peakGrowthMetrics,
        InteractionPressureMetricsSnapshot peakInteractionPressure,
        int peakHotspotScore,
        SimulationDiagnosticsBudget? budget)
    {
        if (budget is null)
        {
            return new SimulationDiagnosticsBudgetEvaluation
            {
                WithinBudget = true,
                Violations = [],
            };
        }

        List<string> violations = new();
        AddViolationIfExceeded(violations, "peak diff entries", peakMetrics.DiffEntryCount, budget.PeakCeiling.DiffEntryCount);
        AddViolationIfExceeded(violations, "peak domain events", peakMetrics.DomainEventCount, budget.PeakCeiling.DomainEventCount);
        AddViolationIfExceeded(violations, "peak notifications", peakMetrics.NotificationCount, budget.PeakCeiling.NotificationCount);
        AddViolationIfExceeded(violations, "peak save payload bytes", peakMetrics.SavePayloadBytes, budget.PeakCeiling.SavePayloadBytes);
        AddViolationIfExceeded(violations, "growth diff entries", peakGrowthMetrics.DiffEntryCount, budget.GrowthCeiling.DiffEntryCount);
        AddViolationIfExceeded(violations, "growth domain events", peakGrowthMetrics.DomainEventCount, budget.GrowthCeiling.DomainEventCount);
        AddViolationIfExceeded(violations, "growth notifications", peakGrowthMetrics.NotificationCount, budget.GrowthCeiling.NotificationCount);
        AddViolationIfExceeded(violations, "growth save payload bytes", peakGrowthMetrics.SavePayloadBytes, budget.GrowthCeiling.SavePayloadBytes);
        AddViolationIfExceeded(
            violations,
            "peak active conflict settlements",
            peakInteractionPressure.ActiveConflictSettlements,
            budget.InteractionPressureCeiling.ActiveConflictSettlements);
        AddViolationIfExceeded(
            violations,
            "peak activated response settlements",
            peakInteractionPressure.ActivatedResponseSettlements,
            budget.InteractionPressureCeiling.ActivatedResponseSettlements);
        AddViolationIfExceeded(
            violations,
            "peak supported order settlements",
            peakInteractionPressure.SupportedOrderSettlements,
            budget.InteractionPressureCeiling.SupportedOrderSettlements);
        AddViolationIfExceeded(
            violations,
            "peak high suppression demand settlements",
            peakInteractionPressure.HighSuppressionDemandSettlements,
            budget.InteractionPressureCeiling.HighSuppressionDemandSettlements);
        AddViolationIfExceeded(
            violations,
            "peak average suppression demand",
            peakInteractionPressure.AverageSuppressionDemand,
            budget.InteractionPressureCeiling.AverageSuppressionDemand);
        AddViolationIfExceeded(
            violations,
            "peak suppression demand",
            peakInteractionPressure.PeakSuppressionDemand,
            budget.InteractionPressureCeiling.PeakSuppressionDemand);
        AddViolationIfExceeded(
            violations,
            "peak high bandit threat settlements",
            peakInteractionPressure.HighBanditThreatSettlements,
            budget.InteractionPressureCeiling.HighBanditThreatSettlements);
        AddViolationIfExceeded(
            violations,
            "peak order carryover settlements",
            peakInteractionPressure.OrderInterventionCarryoverSettlements,
            budget.InteractionPressureCeiling.OrderInterventionCarryoverSettlements);
        AddViolationIfExceeded(
            violations,
            "peak shielding-dominant settlements",
            peakInteractionPressure.ShieldingDominantSettlements,
            budget.InteractionPressureCeiling.ShieldingDominantSettlements);
        AddViolationIfExceeded(
            violations,
            "peak backlash-dominant settlements",
            peakInteractionPressure.BacklashDominantSettlements,
            budget.InteractionPressureCeiling.BacklashDominantSettlements);
        AddViolationIfExceeded(violations, "peak hotspot score", peakHotspotScore, budget.PeakHotspotScoreCeiling);

        return new SimulationDiagnosticsBudgetEvaluation
        {
            WithinBudget = violations.Count == 0,
            Violations = violations,
        };
    }

    private static void AddViolationIfExceeded(List<string> violations, string label, int actual, int ceiling)
    {
        if (ceiling <= 0 || actual <= ceiling)
        {
            return;
        }

        violations.Add($"{label} {actual} exceeded ceiling {ceiling}.");
    }

    private static IReadOnlyList<ModuleActivityMetricsSnapshot> BuildModuleActivity(SimulationMonthResult result)
    {
        Dictionary<string, ModuleActivityMetricsSnapshot> activity = new(StringComparer.Ordinal);

        foreach (IGrouping<string, WorldDiffEntry> diffGroup in result.Diff.Entries.GroupBy(static entry => entry.ModuleKey, StringComparer.Ordinal))
        {
            if (!activity.TryGetValue(diffGroup.Key, out ModuleActivityMetricsSnapshot? metrics))
            {
                metrics = new ModuleActivityMetricsSnapshot
                {
                    ModuleKey = diffGroup.Key,
                };
                activity[diffGroup.Key] = metrics;
            }

            metrics.DiffEntryCount = diffGroup.Count();
        }

        foreach (IGrouping<string, IDomainEvent> eventGroup in result.DomainEvents.GroupBy(static entry => entry.ModuleKey, StringComparer.Ordinal))
        {
            if (!activity.TryGetValue(eventGroup.Key, out ModuleActivityMetricsSnapshot? metrics))
            {
                metrics = new ModuleActivityMetricsSnapshot
                {
                    ModuleKey = eventGroup.Key,
                };
                activity[eventGroup.Key] = metrics;
            }

            metrics.DomainEventCount = eventGroup.Count();
        }

        return activity.Values.OrderBy(static metric => metric.ModuleKey, StringComparer.Ordinal).ToArray();
    }

    private static void UpdatePeakModuleActivity(
        Dictionary<string, ModuleActivityMetricsSnapshot> peakModuleActivity,
        IReadOnlyList<ModuleActivityMetricsSnapshot> sampleActivity)
    {
        foreach (ModuleActivityMetricsSnapshot sample in sampleActivity)
        {
            if (!peakModuleActivity.TryGetValue(sample.ModuleKey, out ModuleActivityMetricsSnapshot? peak))
            {
                peakModuleActivity[sample.ModuleKey] = new ModuleActivityMetricsSnapshot
                {
                    ModuleKey = sample.ModuleKey,
                    DiffEntryCount = sample.DiffEntryCount,
                    DomainEventCount = sample.DomainEventCount,
                };
                continue;
            }

            peak.DiffEntryCount = Math.Max(peak.DiffEntryCount, sample.DiffEntryCount);
            peak.DomainEventCount = Math.Max(peak.DomainEventCount, sample.DomainEventCount);
        }
    }

    private static void UpdatePeakHotspots(
        Dictionary<SettlementId, SettlementInteractionHotspotSnapshot> peakHotspots,
        IReadOnlyList<SettlementInteractionHotspotSnapshot> sampleHotspots)
    {
        foreach (SettlementInteractionHotspotSnapshot sample in sampleHotspots)
        {
            if (!peakHotspots.TryGetValue(sample.SettlementId, out SettlementInteractionHotspotSnapshot? existing) ||
                sample.HotspotScore > existing.HotspotScore)
            {
                peakHotspots[sample.SettlementId] = sample;
            }
        }
    }

    private static void UpdatePeakPayloadModules(
        Dictionary<string, ModulePayloadFootprintSnapshot> peakPayloadModules,
        IReadOnlyList<ModulePayloadFootprintSnapshot> samplePayloadModules)
    {
        foreach (ModulePayloadFootprintSnapshot sample in samplePayloadModules)
        {
            if (!peakPayloadModules.TryGetValue(sample.ModuleKey, out ModulePayloadFootprintSnapshot? existing) ||
                sample.PayloadBytes > existing.PayloadBytes)
            {
                peakPayloadModules[sample.ModuleKey] = sample;
            }
        }
    }
}
