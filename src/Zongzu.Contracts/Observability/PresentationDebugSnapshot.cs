using System.Collections.Generic;

namespace Zongzu.Contracts;

public sealed record DebugFeatureModeSnapshot
{
    public string ModuleKey { get; init; } = string.Empty;

    public string Mode { get; init; } = string.Empty;
}

public sealed record DebugModuleInspectorSnapshot
{
    public string ModuleKey { get; init; } = string.Empty;

    public int ModuleSchemaVersion { get; init; }

    public int PayloadBytes { get; init; }
}

public sealed record DebugDiffTraceSnapshot
{
    public string ModuleKey { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? EntityKey { get; init; }
}

public sealed record DebugDomainEventSnapshot
{
    public string ModuleKey { get; init; } = string.Empty;

    public string EventType { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;
}

public sealed record DebugMigrationStepSnapshot
{
    public string ScopeLabel { get; init; } = string.Empty;

    public int SourceVersion { get; init; }

    public int TargetVersion { get; init; }
}

public sealed record DebugLoadMigrationSnapshot
{
    public string LoadOriginLabel { get; init; } = string.Empty;

    public bool WasMigrationApplied { get; init; }

    public int StepCount { get; init; }

    public bool ConsistencyPassed { get; init; }

    public string Summary { get; init; } = string.Empty;

    public string ConsistencySummary { get; init; } = string.Empty;

    public IReadOnlyList<DebugMigrationStepSnapshot> Steps { get; init; } = [];

    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record PresentationDebugSnapshot
{
    public int DiagnosticsSchemaVersion { get; init; }

    public long InitialSeed { get; init; }

    public int NotificationRetentionLimit { get; init; }

    public bool RetentionLimitReached { get; init; }

    public ObservabilityMetricsSnapshot LatestMetrics { get; init; } = new();

    public InteractionPressureMetricsSnapshot CurrentInteractionPressure { get; init; } = new();

    public SettlementPressureDistributionSnapshot CurrentPressureDistribution { get; init; } = new();

    public RuntimeScaleMetricsSnapshot CurrentScale { get; init; } = new();

    public IReadOnlyList<SettlementInteractionHotspotSnapshot> CurrentHotspots { get; init; } = [];

    public RuntimePayloadSummarySnapshot CurrentPayloadSummary { get; init; } = new();

    public IReadOnlyList<ModulePayloadFootprintSnapshot> TopPayloadModules { get; init; } = [];

    public DebugLoadMigrationSnapshot LoadMigration { get; init; } = new();

    public IReadOnlyList<DebugFeatureModeSnapshot> EnabledModules { get; init; } = [];

    public IReadOnlyList<DebugModuleInspectorSnapshot> ModuleInspectors { get; init; } = [];

    public IReadOnlyList<DebugDiffTraceSnapshot> RecentDiffEntries { get; init; } = [];

    public IReadOnlyList<DebugDomainEventSnapshot> RecentDomainEvents { get; init; } = [];

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public IReadOnlyList<string> Invariants { get; init; } = [];
}
