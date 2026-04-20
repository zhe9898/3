namespace Zongzu.Presentation.Unity;

public sealed class PresentationShellViewModel
{
	public GreatHallDashboardViewModel GreatHall { get; set; } = new GreatHallDashboardViewModel();

	public LineageSurfaceViewModel Lineage { get; set; } = new LineageSurfaceViewModel();

	public FamilyCouncilViewModel FamilyCouncil { get; set; } = new FamilyCouncilViewModel();

	public DeskSandboxViewModel DeskSandbox { get; set; } = new DeskSandboxViewModel();

	public OfficeSurfaceViewModel Office { get; set; } = new OfficeSurfaceViewModel();

	public WarfareSurfaceViewModel Warfare { get; set; } = new WarfareSurfaceViewModel();

	public NotificationCenterViewModel NotificationCenter { get; set; } = new NotificationCenterViewModel();

	public DebugPanelViewModel Debug { get; set; } = new DebugPanelViewModel();
}
