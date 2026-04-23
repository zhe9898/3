namespace Zongzu.Contracts;

public sealed record ObservabilityMetricsSnapshot
{
    public int DiffEntryCount { get; init; }

    public int DomainEventCount { get; init; }

    public int NotificationCount { get; init; }

    public int SavePayloadBytes { get; init; }
}
