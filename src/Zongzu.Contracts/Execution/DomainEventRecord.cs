using System;

namespace Zongzu.Contracts;

public interface IDomainEvent
{
    string ModuleKey { get; }

    string EventType { get; }

    string Summary { get; }

    string? EntityKey { get; }
}

public sealed class DomainEventRecord : IDomainEvent
{
    public DomainEventRecord(string moduleKey, string eventType, string summary, string? entityKey = null)
    {
        ModuleKey = moduleKey ?? throw new ArgumentNullException(nameof(moduleKey));
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        EntityKey = entityKey;
    }

    public string ModuleKey { get; }

    public string EventType { get; }

    public string Summary { get; }

    public string? EntityKey { get; }
}
