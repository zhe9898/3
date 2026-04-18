using System.Collections.Generic;

namespace Zongzu.Contracts;

public sealed class DebugFeatureModeSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;
}

public sealed class DebugModuleInspectorSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public int ModuleSchemaVersion { get; set; }

    public int PayloadBytes { get; set; }
}

public sealed class DebugDiffTraceSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? EntityKey { get; set; }
}

public sealed class DebugDomainEventSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}

public sealed class DebugMigrationStepSnapshot
{
    public string ScopeLabel { get; set; } = string.Empty;

    public int SourceVersion { get; set; }

    public int TargetVersion { get; set; }
}

public sealed class DebugLoadMigrationSnapshot
{
    public string LoadOriginLabel { get; set; } = string.Empty;

    public bool WasMigrationApplied { get; set; }

    public int StepCount { get; set; }

    public bool ConsistencyPassed { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string ConsistencySummary { get; set; } = string.Empty;

    public IReadOnlyList<DebugMigrationStepSnapshot> Steps { get; set; } = [];

    public IReadOnlyList<string> Warnings { get; set; } = [];
}

public sealed class PresentationDebugSnapshot
{
    public int DiagnosticsSchemaVersion { get; set; }

    public long InitialSeed { get; set; }

    public int NotificationRetentionLimit { get; set; }

    public bool RetentionLimitReached { get; set; }

    public ObservabilityMetricsSnapshot LatestMetrics { get; set; } = new();

    public InteractionPressureMetricsSnapshot CurrentInteractionPressure { get; set; } = new();

    public SettlementPressureDistributionSnapshot CurrentPressureDistribution { get; set; } = new();

    public RuntimeScaleMetricsSnapshot CurrentScale { get; set; } = new();

    public IReadOnlyList<SettlementInteractionHotspotSnapshot> CurrentHotspots { get; set; } = [];

    public RuntimePayloadSummarySnapshot CurrentPayloadSummary { get; set; } = new();

    public IReadOnlyList<ModulePayloadFootprintSnapshot> TopPayloadModules { get; set; } = [];

    public DebugLoadMigrationSnapshot LoadMigration { get; set; } = new();

    public IReadOnlyList<DebugFeatureModeSnapshot> EnabledModules { get; set; } = [];

    public IReadOnlyList<DebugModuleInspectorSnapshot> ModuleInspectors { get; set; } = [];

    public IReadOnlyList<DebugDiffTraceSnapshot> RecentDiffEntries { get; set; } = [];

    public IReadOnlyList<DebugDomainEventSnapshot> RecentDomainEvents { get; set; } = [];

    public IReadOnlyList<string> Warnings { get; set; } = [];

    public IReadOnlyList<string> Invariants { get; set; } = [];
}
