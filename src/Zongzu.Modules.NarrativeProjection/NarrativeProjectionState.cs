using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.NarrativeProjection;

public sealed class NarrativeProjectionState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.NarrativeProjection;

    public List<NarrativeNotificationState> Notifications { get; set; } = new();
}

public sealed class NarrativeNotificationState
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

    public List<NarrativeTraceState> Traces { get; set; } = new();
}

public sealed class NarrativeTraceState
{
    public string SourceModuleKey { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string EventSummary { get; set; } = string.Empty;

    public string DiffDescription { get; set; } = string.Empty;

    public string? EntityKey { get; set; }
}
