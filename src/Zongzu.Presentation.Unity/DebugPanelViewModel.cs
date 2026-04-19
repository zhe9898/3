namespace Zongzu.Presentation.Unity;

public sealed class DebugPanelViewModel
{
	public string DiagnosticsSchemaLabel { get; set; } = string.Empty;

	public string SeedLabel { get; set; } = string.Empty;

	public string NotificationRetentionLabel { get; set; } = string.Empty;

	public DebugScaleGroupViewModel Scale { get; set; } = new DebugScaleGroupViewModel();

	public DebugPressureGroupViewModel Pressure { get; set; } = new DebugPressureGroupViewModel();

	public DebugHotspotsGroupViewModel Hotspots { get; set; } = new DebugHotspotsGroupViewModel();

	public DebugMigrationGroupViewModel Migration { get; set; } = new DebugMigrationGroupViewModel();

	public DebugWarningsGroupViewModel Warnings { get; set; } = new DebugWarningsGroupViewModel();
}
