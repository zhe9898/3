using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class DebugShellAdapter
{
	internal static DebugPanelViewModel BuildDebugPanel(PresentationDebugSnapshot debug)
	{
		return new DebugPanelViewModel
		{
			DiagnosticsSchemaLabel = $"v{debug.DiagnosticsSchemaVersion}",
			SeedLabel = debug.InitialSeed.ToString(),
			NotificationRetentionLabel = debug.NotificationRetentionLimit.ToString(),
			Scale = BuildScaleGroup(debug),
			Pressure = BuildPressureGroup(debug),
			Hotspots = BuildHotspotsGroup(debug),
			Migration = BuildMigrationGroup(debug),
			Warnings = BuildWarningsGroup(debug)
		};
	}

	private static DebugScaleGroupViewModel BuildScaleGroup(PresentationDebugSnapshot debug)
	{
		return new DebugScaleGroupViewModel
		{
			LatestMetrics = new DebugMetricSummaryViewModel
			{
				DiffEntryCount = debug.LatestMetrics.DiffEntryCount,
				DomainEventCount = debug.LatestMetrics.DomainEventCount,
				NotificationCount = debug.LatestMetrics.NotificationCount,
				SavePayloadBytes = debug.LatestMetrics.SavePayloadBytes,
				RetentionLimitReached = debug.RetentionLimitReached
			},
			CurrentScale = new DebugScaleSummaryViewModel
			{
				EntitySummary = $"{debug.CurrentScale.SettlementCount} settlements, {debug.CurrentScale.ClanCount} clans, {debug.CurrentScale.HouseholdCount} households.",
				FidelityRingSummary = $"Core {debug.CurrentScale.CorePersonCount}, Local {debug.CurrentScale.LocalPersonCount}, Regional {debug.CurrentScale.RegionalPersonCount}.",
				MobilityPoolSummary = $"{debug.CurrentScale.LaborPoolCount} labor pools, {debug.CurrentScale.MarriagePoolCount} marriage pools, {debug.CurrentScale.MigrationPoolCount} migration pools.",
				MovementFocusSummary = $"{debug.CurrentScale.ActiveMigratingHouseholdCount} migrating households, {debug.CurrentScale.ActiveMigratingPersonCount} named migrating persons.",
				InstitutionSummary = $"{debug.CurrentScale.AcademyCount} academies, {debug.CurrentScale.RouteCount} trade routes.",
				ModuleSummary = $"{debug.CurrentScale.EnabledModuleCount} enabled modules mirrored in {debug.CurrentScale.SavedModuleCount} saved envelopes.",
				NotificationUtilizationLabel = $"{debug.CurrentScale.NotificationCount} notices ({debug.CurrentScale.NotificationUtilizationPercent}% of retention).",
				PayloadDensityLabel = $"{debug.CurrentScale.SavePayloadBytesPerSettlement} save bytes per settlement; {debug.CurrentScale.AverageHouseholdsPerSettlement} households per settlement."
			},
			PayloadSummary = new DebugPayloadSummaryViewModel
			{
				TotalPayloadBytes = debug.CurrentPayloadSummary.TotalModulePayloadBytes,
				LargestModuleKey = debug.CurrentPayloadSummary.LargestModuleKey,
				LargestModulePayloadBytes = debug.CurrentPayloadSummary.LargestModulePayloadBytes,
				LargestModuleShareLabel = $"{(double)debug.CurrentPayloadSummary.LargestModuleShareBasisPoints / 100.0:F2}%",
				Summary = string.IsNullOrWhiteSpace(debug.CurrentPayloadSummary.LargestModuleKey)
					? "No module payload data."
					: $"{debug.CurrentPayloadSummary.TotalModulePayloadBytes} total bytes; {debug.CurrentPayloadSummary.LargestModuleKey} leads at {debug.CurrentPayloadSummary.LargestModulePayloadBytes} bytes."
			},
			TopPayloadModules = debug.TopPayloadModules.Select(payload => new DebugPayloadFootprintViewModel
			{
				ModuleKey = payload.ModuleKey,
				PayloadBytes = payload.PayloadBytes,
				ShareLabel = $"{(double)payload.PayloadShareBasisPoints / 100.0:F2}%"
			}).ToArray(),
			EnabledModules = debug.EnabledModules.Select(module => module.ModuleKey + ":" + module.Mode).ToArray(),
			ModuleInspectors = debug.ModuleInspectors.Select(inspector => new DebugModuleInspectorViewModel
			{
				ModuleKey = inspector.ModuleKey,
				SchemaVersion = inspector.ModuleSchemaVersion,
				PayloadBytes = inspector.PayloadBytes
			}).ToArray()
		};
	}

	private static DebugPressureGroupViewModel BuildPressureGroup(PresentationDebugSnapshot debug)
	{
		return new DebugPressureGroupViewModel
		{
			Interaction = new DebugInteractionPressureViewModel
			{
				ActiveConflictSettlements = debug.CurrentInteractionPressure.ActiveConflictSettlements,
				ActivatedResponseSettlements = debug.CurrentInteractionPressure.ActivatedResponseSettlements,
				SupportedOrderSettlements = debug.CurrentInteractionPressure.SupportedOrderSettlements,
				HighSuppressionDemandSettlements = debug.CurrentInteractionPressure.HighSuppressionDemandSettlements,
				AverageSuppressionDemand = debug.CurrentInteractionPressure.AverageSuppressionDemand,
				PeakSuppressionDemand = debug.CurrentInteractionPressure.PeakSuppressionDemand,
				HighBanditThreatSettlements = debug.CurrentInteractionPressure.HighBanditThreatSettlements,
				Summary = $"{debug.CurrentInteractionPressure.ActivatedResponseSettlements} activated, {debug.CurrentInteractionPressure.HighSuppressionDemandSettlements} high suppression, peak demand {debug.CurrentInteractionPressure.PeakSuppressionDemand}."
			},
			Distribution = new DebugPressureDistributionViewModel
			{
				CalmSettlements = debug.CurrentPressureDistribution.CalmSettlements,
				WatchedSettlements = debug.CurrentPressureDistribution.WatchedSettlements,
				StressedSettlements = debug.CurrentPressureDistribution.StressedSettlements,
				CrisisSettlements = debug.CurrentPressureDistribution.CrisisSettlements,
				Summary = $"{debug.CurrentPressureDistribution.CalmSettlements} calm, {debug.CurrentPressureDistribution.WatchedSettlements} watched, {debug.CurrentPressureDistribution.StressedSettlements} stressed, {debug.CurrentPressureDistribution.CrisisSettlements} crisis."
			}
		};
	}

	private static DebugHotspotsGroupViewModel BuildHotspotsGroup(PresentationDebugSnapshot debug)
	{
		return new DebugHotspotsGroupViewModel
		{
			CurrentHotspots = debug.CurrentHotspots.Select(hotspot => new DebugHotspotViewModel
			{
				SettlementName = hotspot.SettlementName,
				HotspotScore = hotspot.HotspotScore,
				PressureSummary = $"Bandit {hotspot.BanditThreat}, route {hotspot.RoutePressure}, suppression {hotspot.SuppressionDemand}.",
				ResponseSummary = hotspot.IsResponseActivated
					? $"Active response {hotspot.ResponseActivationLevel} with support {hotspot.OrderSupportLevel}."
					: "No active response support."
			}).ToArray(),
			DiffTraces = debug.RecentDiffEntries.Select(trace => new DebugTraceItemViewModel
			{
				ModuleKey = trace.ModuleKey,
				Summary = trace.Description,
				EntityKey = trace.EntityKey
			}).ToArray(),
			DomainEvents = debug.RecentDomainEvents.Select(domainEvent => new DebugEventItemViewModel
			{
				ModuleKey = domainEvent.ModuleKey,
				EventType = domainEvent.EventType,
				Summary = domainEvent.Summary
			}).ToArray()
		};
	}

	private static DebugMigrationGroupViewModel BuildMigrationGroup(PresentationDebugSnapshot debug)
	{
		return new DebugMigrationGroupViewModel
		{
			LoadOriginLabel = debug.LoadMigration.LoadOriginLabel,
			MigrationStatusLabel = debug.LoadMigration.ConsistencyPassed ? "Consistency passed" : "Consistency warnings present",
			MigrationSummary = debug.LoadMigration.Summary,
			MigrationConsistencySummary = debug.LoadMigration.ConsistencySummary,
			MigrationStepCountLabel = $"{debug.LoadMigration.StepCount} migration step(s)",
			MigrationSteps = debug.LoadMigration.Steps.Select(step => $"{step.ScopeLabel}:{step.SourceVersion}->{step.TargetVersion}").ToArray()
		};
	}

	private static DebugWarningsGroupViewModel BuildWarningsGroup(PresentationDebugSnapshot debug)
	{
		return new DebugWarningsGroupViewModel
		{
			Messages = debug.Warnings,
			Invariants = debug.Invariants
		};
	}
}
