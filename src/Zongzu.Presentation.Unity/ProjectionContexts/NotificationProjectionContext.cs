using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal sealed class NotificationProjectionContext
{
	private readonly IReadOnlyDictionary<int, string> _whatNextByNotificationId;

	private NotificationProjectionContext(
		NotificationItemViewModel[] items,
		IReadOnlyDictionary<int, string> whatNextByNotificationId)
	{
		Items = items;
		_whatNextByNotificationId = whatNextByNotificationId;
	}

	internal NotificationItemViewModel[] Items { get; }

	internal static NotificationProjectionContext Create(
		IReadOnlyList<NarrativeNotificationSnapshot> notifications,
		string lifecyclePrompt)
	{
		ArgumentNullException.ThrowIfNull(notifications);
		ArgumentNullException.ThrowIfNull(lifecyclePrompt);

		Dictionary<int, string> whatNextByNotificationId = notifications.ToDictionary(
			notification => notification.Id.Value,
			notification => BuildNotificationWhatNext(notification, lifecyclePrompt));

		NotificationItemViewModel[] items = notifications
			.Select(notification => new NotificationItemViewModel
			{
				Title = notification.Title,
				Summary = notification.Summary,
				WhyItHappened = notification.WhyItHappened,
				WhatNext = whatNextByNotificationId[notification.Id.Value],
				TierLabel = notification.Tier.ToString(),
				SurfaceLabel = notification.Surface.ToString(),
				SourceModuleKey = notification.SourceModuleKey,
				TraceCount = notification.Traces.Count
			})
			.ToArray();

		return new NotificationProjectionContext(items, whatNextByNotificationId);
	}

	internal string GetLeadNoticeGuidance(NarrativeNotificationSnapshot? leadNotification)
	{
		if (leadNotification == null)
		{
			return string.Empty;
		}

		return _whatNextByNotificationId.TryGetValue(leadNotification.Id.Value, out string? guidance)
			? guidance
			: string.Empty;
	}

	private static string BuildNotificationWhatNext(
		NarrativeNotificationSnapshot notification,
		string lifecyclePrompt)
	{
		if (!IsFamilyLifecycleNotification(notification) || string.IsNullOrWhiteSpace(lifecyclePrompt))
		{
			return notification.WhatNext;
		}

		if (string.IsNullOrWhiteSpace(notification.WhatNext))
		{
			return lifecyclePrompt;
		}

		return ShellTextAdapter.AppendDistinct(notification.WhatNext, lifecyclePrompt);
	}

	private static bool IsFamilyLifecycleNotification(NarrativeNotificationSnapshot notification)
	{
		if (!string.Equals(notification.SourceModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal))
		{
			return false;
		}

		return notification.Traces.Any(trace => trace.EventType is
			FamilyCoreEventNames.MarriageAllianceArranged or
			FamilyCoreEventNames.BirthRegistered or
			FamilyCoreEventNames.DeathRegistered or
			FamilyCoreEventNames.HeirSecurityWeakened);
	}
}
