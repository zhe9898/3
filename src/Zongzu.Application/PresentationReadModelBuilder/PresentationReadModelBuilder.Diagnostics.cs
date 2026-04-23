using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Persistence;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private PresentationDebugSnapshot BuildDebugSnapshot(
        GameSimulation simulation,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        SaveRoot saveRoot = simulation.ExportSave();
        InteractionPressureMetricsSnapshot interactionPressure = RuntimeObservabilityCollector.CollectInteractionPressure(simulation);
        SettlementPressureDistributionSnapshot pressureDistribution = RuntimeObservabilityCollector.CollectPressureDistribution(simulation);
        RuntimeScaleMetricsSnapshot scaleMetrics = RuntimeObservabilityCollector.CollectScaleMetrics(simulation, saveRoot, notifications.Count);
        IReadOnlyList<SettlementInteractionHotspotSnapshot> currentHotspots = RuntimeObservabilityCollector.CollectTopHotspots(simulation);
        RuntimePayloadSummarySnapshot payloadSummary = RuntimeObservabilityCollector.CollectPayloadSummary(saveRoot);
        IReadOnlyList<ModulePayloadFootprintSnapshot> topPayloadModules = RuntimeObservabilityCollector.CollectTopPayloadModules(saveRoot);
        List<string> warnings = new();
        List<string> invariants = new();

        try
        {
            ModuleBoundaryValidator.Validate(simulation.Modules, simulation.FeatureManifest, saveRoot);
            invariants.Add("Module boundary validation passed.");
        }
        catch (Exception exception)
        {
            warnings.Add($"Module boundary validation failed: {exception.Message}");
        }

        if (simulation.LastMonthResult is null)
        {
            warnings.Add("No monthly diff has been recorded yet.");
        }
        else
        {
            if (simulation.LastMonthResult.Diff.Entries.Count == 0)
            {
                warnings.Add("The latest simulated month produced no diff entries.");
            }

            if (simulation.LastMonthResult.DomainEvents.Count == 0)
            {
                warnings.Add("The latest simulated month produced no domain events.");
            }
        }

        if (notifications.Count >= NarrativeProjectionModule.NotificationRetentionLimit)
        {
            warnings.Add("Notification history is at the retention limit; older notices may already be trimmed.");
        }

        if (notifications.Any(static notification => notification.Tier == NotificationTier.Urgent))
        {
            warnings.Add("Urgent notifications are pending review.");
        }

        if (interactionPressure.ActivatedResponseSettlements > 0)
        {
            warnings.Add($"{interactionPressure.ActivatedResponseSettlements} settlement response postures are currently activated.");
        }

        if (interactionPressure.OrderInterventionCarryoverSettlements > 0)
        {
            warnings.Add($"{interactionPressure.OrderInterventionCarryoverSettlements} settlements still carry recent order follow-through into the current month.");
        }

        if (interactionPressure.OrderAdministrativeAftermathSettlements > 0)
        {
            warnings.Add($"{interactionPressure.OrderAdministrativeAftermathSettlements} settlements still have office cleanup tied to recent order follow-through.");
        }

        if (interactionPressure.ShieldingDominantSettlements > 0)
        {
            warnings.Add($"{interactionPressure.ShieldingDominantSettlements} settlements currently lean toward protected traffic rather than backlash.");
        }

        if (interactionPressure.BacklashDominantSettlements > 0)
        {
            warnings.Add($"{interactionPressure.BacklashDominantSettlements} settlements currently lean toward suppression backlash rather than protected traffic.");
        }

        if (pressureDistribution.CrisisSettlements > 0)
        {
            warnings.Add($"{pressureDistribution.CrisisSettlements} settlements are currently in crisis-pressure range.");
        }

        if (currentHotspots.Count > 0)
        {
            warnings.Add($"Current hotspot focus: {currentHotspots[0].SettlementName} at score {currentHotspots[0].HotspotScore}.");

            if (currentHotspots[0].InterventionCarryoverMonths > 0)
            {
                warnings.Add($"Hotspot {currentHotspots[0].SettlementName} still has {currentHotspots[0].InterventionCarryoverMonths} month of order carryover active.");
            }

            if (!string.IsNullOrWhiteSpace(currentHotspots[0].AdministrativeAftermathSummary))
            {
                warnings.Add($"Hotspot {currentHotspots[0].SettlementName} also carries office follow-through: {currentHotspots[0].AdministrativeAftermathSummary}");
            }
        }

        if (scaleMetrics.NotificationUtilizationPercent >= 90)
        {
            warnings.Add($"Notification retention utilization is high at {scaleMetrics.NotificationUtilizationPercent}%.");
        }

        if (topPayloadModules.Count > 0)
        {
            warnings.Add($"Largest payload module: {topPayloadModules[0].ModuleKey} at {topPayloadModules[0].PayloadBytes} bytes.");
        }

        if (payloadSummary.TotalModulePayloadBytes > 0 && scaleMetrics.SettlementCount > 0)
        {
            warnings.Add($"Runtime payload density is {scaleMetrics.SavePayloadBytesPerSettlement} bytes per settlement.");
        }

        DebugLoadMigrationSnapshot loadMigration = BuildLoadMigrationSnapshot(simulation);
        if (loadMigration.WasMigrationApplied)
        {
            warnings.Add(loadMigration.Summary);
        }
        warnings.AddRange(loadMigration.Warnings);

        return new PresentationDebugSnapshot
        {
            DiagnosticsSchemaVersion = 1,
            InitialSeed = simulation.KernelState.InitialSeed,
            NotificationRetentionLimit = NarrativeProjectionModule.NotificationRetentionLimit,
            RetentionLimitReached = notifications.Count >= NarrativeProjectionModule.NotificationRetentionLimit,
            LatestMetrics = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = simulation.LastMonthResult?.Diff.Entries.Count ?? 0,
                DomainEventCount = simulation.LastMonthResult?.DomainEvents.Count ?? 0,
                NotificationCount = notifications.Count,
                SavePayloadBytes = _saveCodec.Encode(saveRoot).Length,
            },
            CurrentInteractionPressure = interactionPressure,
            CurrentPressureDistribution = pressureDistribution,
            CurrentScale = scaleMetrics,
            CurrentHotspots = currentHotspots,
            CurrentPayloadSummary = payloadSummary,
            TopPayloadModules = topPayloadModules,
            LoadMigration = loadMigration,
            EnabledModules = simulation.FeatureManifest.GetOrderedEntries()
                .Where(static pair => !string.Equals(pair.Value, "off", StringComparison.Ordinal))
                .Select(static pair => new DebugFeatureModeSnapshot
                {
                    ModuleKey = pair.Key,
                    Mode = pair.Value,
                })
                .ToArray(),
            ModuleInspectors = saveRoot.ModuleStates
                .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
                .Select(static pair => new DebugModuleInspectorSnapshot
                {
                    ModuleKey = pair.Key,
                    ModuleSchemaVersion = pair.Value.ModuleSchemaVersion,
                    PayloadBytes = pair.Value.Payload.Length,
                })
                .ToArray(),
            RecentDiffEntries = simulation.LastMonthResult?.Diff.Entries
                .Select(static entry => new DebugDiffTraceSnapshot
                {
                    ModuleKey = entry.ModuleKey,
                    Description = entry.Description,
                    EntityKey = entry.EntityKey,
                })
                .ToArray() ?? [],
            RecentDomainEvents = simulation.LastMonthResult?.DomainEvents
                .Select(static domainEvent => new DebugDomainEventSnapshot
                {
                    ModuleKey = domainEvent.ModuleKey,
                    EventType = domainEvent.EventType,
                    Summary = domainEvent.Summary,
                })
                .ToArray() ?? [],
            Warnings = warnings.ToArray(),
            Invariants = invariants.ToArray(),
        };
    }

    private static DebugLoadMigrationSnapshot BuildLoadMigrationSnapshot(GameSimulation simulation)
    {
        SaveMigrationReport? report = simulation.LoadMigrationReport;
        if (report is null)
        {
            return new DebugLoadMigrationSnapshot
            {
                LoadOriginLabel = "Bootstrap",
                WasMigrationApplied = false,
                StepCount = 0,
                ConsistencyPassed = true,
                Summary = "Simulation was created from bootstrap state.",
                ConsistencySummary = "Bootstrap path did not require save preparation.",
                Steps = [],
                Warnings = [],
            };
        }

        DebugMigrationStepSnapshot[] steps =
        [
            .. report.RootSteps.Select(static step => new DebugMigrationStepSnapshot
            {
                ScopeLabel = "root",
                SourceVersion = step.SourceVersion,
                TargetVersion = step.TargetVersion,
            }),
            .. report.ModuleSteps.Select(static step => new DebugMigrationStepSnapshot
            {
                ScopeLabel = step.ModuleKey,
                SourceVersion = step.SourceVersion,
                TargetVersion = step.TargetVersion,
            }),
        ];

        string summary = report.WasMigrationApplied
            ? $"Loaded save required {report.AppliedStepCount} migration step(s) before simulation resumed."
            : "Loaded save matched current schemas without migration.";
        string consistencySummary =
            $"{report.PreparedEnabledModuleCount}/{report.SourceEnabledModuleCount} enabled modules, " +
            $"{report.PreparedModuleStateCount}/{report.SourceModuleStateCount} module envelopes preserved.";

        return new DebugLoadMigrationSnapshot
        {
            LoadOriginLabel = "SaveLoad",
            WasMigrationApplied = report.WasMigrationApplied,
            StepCount = report.AppliedStepCount,
            ConsistencyPassed = report.ConsistencyPassed,
            Summary = summary,
            ConsistencySummary = consistencySummary,
            Steps = steps,
            Warnings = report.ConsistencyWarnings,
        };
    }
}
