using System.Collections.Generic;
using System;
using System.Linq;
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

public sealed record NotificationTraceSnapshot
{
    public string SourceModuleKey { get; init; } = string.Empty;

    public string EventType { get; init; } = string.Empty;

    public string EventSummary { get; init; } = string.Empty;

    public string DiffDescription { get; init; } = string.Empty;

    public string? EntityKey { get; init; }

    public bool MatchesSettlementScope(SettlementId settlementId)
    {
        return string.Equals(EntityKey, settlementId.Value.ToString(), StringComparison.Ordinal);
    }

    public bool MatchesSourceModule(string sourceModuleKey)
    {
        return !string.IsNullOrWhiteSpace(sourceModuleKey)
            && string.Equals(SourceModuleKey, sourceModuleKey, StringComparison.Ordinal);
    }
}

public sealed record NarrativeNotificationSnapshot
{
    public NotificationId Id { get; init; }

    public GameDate CreatedAt { get; init; }

    public NotificationTier Tier { get; init; }

    public NarrativeSurface Surface { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string WhyItHappened { get; init; } = string.Empty;

    public string WhatNext { get; init; } = string.Empty;

    public string SourceModuleKey { get; init; } = string.Empty;

    public bool IsRead { get; init; }

    public IReadOnlyList<NotificationTraceSnapshot> Traces { get; init; } = [];

    public bool MatchesSourceModule(string sourceModuleKey)
    {
        return !string.IsNullOrWhiteSpace(sourceModuleKey)
            && string.Equals(SourceModuleKey, sourceModuleKey, StringComparison.Ordinal);
    }

    public bool HasTraceFromModule(string sourceModuleKey)
    {
        return Traces.Any(trace => trace.MatchesSourceModule(sourceModuleKey));
    }

    public bool MatchesSettlementScope(SettlementId settlementId)
    {
        return Traces.Any(trace => trace.MatchesSettlementScope(settlementId));
    }

    public bool MatchesScope(SettlementId settlementId, string? sourceModuleKey = null)
    {
        return (string.IsNullOrWhiteSpace(sourceModuleKey) || MatchesSourceModule(sourceModuleKey))
            && MatchesSettlementScope(settlementId);
    }
}

public interface INarrativeProjectionQueries
{
    NarrativeNotificationSnapshot GetRequiredNotification(NotificationId notificationId);

    IReadOnlyList<NarrativeNotificationSnapshot> GetNotifications();
}
