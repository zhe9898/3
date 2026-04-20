namespace Zongzu.Presentation.Unity;

public sealed class DebugPressureGroupViewModel
{
	public DebugInteractionPressureViewModel Interaction { get; set; } = new DebugInteractionPressureViewModel();

	public DebugPressureDistributionViewModel Distribution { get; set; } = new DebugPressureDistributionViewModel();
}
