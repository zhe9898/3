using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

public static class FirstPassPresentationShell
{
	// Composer only: shared projection setup belongs here, surface-specific mapping stays in adapters.
	public static PresentationShellViewModel Compose(PresentationReadModelBundle bundle)
	{
		return Compose(bundle, selection: null);
	}

	// Selection is transient shell state: it chooses from existing read models and never mutates authority.
	public static PresentationShellViewModel Compose(
		PresentationReadModelBundle bundle,
		PresentationShellSelectionViewModel? selection)
	{
		ArgumentNullException.ThrowIfNull(bundle, "bundle");
		NarrativeNotificationSnapshot[] notifications = (from notification in bundle.Notifications
			orderby notification.Tier, notification.CreatedAt.Year descending, notification.CreatedAt.Month descending, notification.Id.Value descending
			select notification).ToArray();
		string lifecyclePrompt = FamilyShellAdapter.BuildLeadFamilyLifecyclePrompt(bundle.Clans, bundle.PlayerCommands.Affordances);
		NotificationProjectionContext notificationContext = NotificationProjectionContext.Create(notifications, lifecyclePrompt);
		return new PresentationShellViewModel
		{
			GreatHall = GreatHallShellAdapter.BuildGreatHall(bundle, notifications, notificationContext),
			Lineage = LineageShellAdapter.BuildLineage(bundle, selection?.FocusedPersonId),
			FamilyCouncil = FamilyShellAdapter.BuildFamilyCouncil(bundle),
			DeskSandbox = DeskSandboxShellAdapter.BuildDeskSandbox(bundle, notifications),
			Office = OfficeShellAdapter.BuildOfficeSurface(bundle),
			Warfare = WarfareCampaignShellAdapter.BuildWarfareSurface(bundle, notifications),
			NotificationCenter = NotificationShellAdapter.BuildNotificationCenter(notificationContext),
			Debug = DebugShellAdapter.BuildDebugPanel(bundle.Debug)
		};
	}

}
