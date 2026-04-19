namespace Zongzu.Presentation.Unity;

public sealed class DebugPayloadSummaryViewModel
{
	public int TotalPayloadBytes { get; set; }

	public string LargestModuleKey { get; set; } = string.Empty;

	public int LargestModulePayloadBytes { get; set; }

	public string LargestModuleShareLabel { get; set; } = string.Empty;

	public string Summary { get; set; } = string.Empty;
}
