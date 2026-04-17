using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public enum NotificationTier
{
    Urgent = 0,
    Consequential = 1,
    Background = 2,
}

public enum NarrativeSurface
{
    GreatHall = 0,
    AncestralHall = 1,
    DeskSandbox = 2,
    ConflictVignette = 3,
    NotificationCenter = 4,
}

public sealed class NotificationTraceSnapshot
{
    public string SourceModuleKey { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string EventSummary { get; set; } = string.Empty;

    public string DiffDescription { get; set; } = string.Empty;

    public string? EntityKey { get; set; }
}

public sealed class NarrativeNotificationSnapshot
{
    public NotificationId Id { get; set; }

    public GameDate CreatedAt { get; set; }

    public NotificationTier Tier { get; set; }

    public NarrativeSurface Surface { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string WhyItHappened { get; set; } = string.Empty;

    public string WhatNext { get; set; } = string.Empty;

    public string SourceModuleKey { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public IReadOnlyList<NotificationTraceSnapshot> Traces { get; set; } = [];
}

public interface INarrativeProjectionQueries
{
    NarrativeNotificationSnapshot GetRequiredNotification(NotificationId notificationId);

    IReadOnlyList<NarrativeNotificationSnapshot> GetNotifications();
}
