using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class NotificationShellAdapter
{
	internal static NotificationCenterViewModel BuildNotificationCenter(
		NotificationProjectionContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		return new NotificationCenterViewModel
		{
			Items = context.Items
		};
	}

	internal static string BuildLeadNoticeGuidance(
		NotificationProjectionContext context,
		NarrativeNotificationSnapshot? leadNotification)
	{
		ArgumentNullException.ThrowIfNull(context);
		return context.GetLeadNoticeGuidance(leadNotification);
	}
}
