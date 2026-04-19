namespace Zongzu.Presentation.Unity;

public sealed class DebugMetricSummaryViewModel
{
	public int DiffEntryCount { get; set; }

	public int DomainEventCount { get; set; }

	public int NotificationCount { get; set; }

	public int SavePayloadBytes { get; set; }

	public bool RetentionLimitReached { get; set; }
}
