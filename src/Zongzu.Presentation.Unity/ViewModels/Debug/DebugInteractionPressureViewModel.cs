namespace Zongzu.Presentation.Unity;

public sealed class DebugInteractionPressureViewModel
{
	public int ActiveConflictSettlements { get; set; }

	public int ActivatedResponseSettlements { get; set; }

	public int SupportedOrderSettlements { get; set; }

	public int HighSuppressionDemandSettlements { get; set; }

	public int AverageSuppressionDemand { get; set; }

	public int PeakSuppressionDemand { get; set; }

	public int HighBanditThreatSettlements { get; set; }

	public string Summary { get; set; } = string.Empty;
}
