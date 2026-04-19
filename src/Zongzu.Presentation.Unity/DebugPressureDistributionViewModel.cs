namespace Zongzu.Presentation.Unity;

public sealed class DebugPressureDistributionViewModel
{
	public int CalmSettlements { get; set; }

	public int WatchedSettlements { get; set; }

	public int StressedSettlements { get; set; }

	public int CrisisSettlements { get; set; }

	public string Summary { get; set; } = string.Empty;
}
