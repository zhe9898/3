using System;

namespace Zongzu.Contracts;

public interface IDomainEvent
{
    string ModuleKey { get; }

    string EventType { get; }

    string Summary { get; }
}

public sealed class DomainEventRecord : IDomainEvent
{
    public DomainEventRecord(string moduleKey, string eventType, string summary)
    {
        ModuleKey = moduleKey ?? throw new ArgumentNullException(nameof(moduleKey));
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
    }

    public string ModuleKey { get; }

    public string EventType { get; }

    public string Summary { get; }
}
