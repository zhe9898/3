using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Scheduler;

public sealed class SimulationMonthResult
{
    public GameDate SimulatedDate { get; set; }

    public GameDate NextDate { get; set; }

    public WorldDiff Diff { get; set; } = new();

    public IReadOnlyList<IDomainEvent> DomainEvents { get; set; } = [];
}
