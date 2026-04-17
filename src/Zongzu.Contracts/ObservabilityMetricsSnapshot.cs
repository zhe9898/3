namespace Zongzu.Contracts;

public sealed class ObservabilityMetricsSnapshot
{
    public int DiffEntryCount { get; set; }

    public int DomainEventCount { get; set; }

    public int NotificationCount { get; set; }

    public int SavePayloadBytes { get; set; }
}
