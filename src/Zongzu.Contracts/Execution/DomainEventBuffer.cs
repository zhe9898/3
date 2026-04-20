using System;
using System.Collections.Generic;

namespace Zongzu.Contracts;

public sealed class DomainEventBuffer
{
    private readonly List<IDomainEvent> _events = new();

    public IReadOnlyList<IDomainEvent> Events => _events;

    public void Emit(IDomainEvent domainEvent)
    {
        _events.Add(domainEvent ?? throw new ArgumentNullException(nameof(domainEvent)));
    }
}
