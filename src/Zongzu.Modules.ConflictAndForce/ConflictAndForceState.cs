using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed class ConflictAndForceState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.ConflictAndForce;

    public List<SettlementForceState> Settlements { get; set; } = new();
}

public sealed class SettlementForceState
{
    public SettlementId SettlementId { get; set; }

    public int GuardCount { get; set; }

    public int RetainerCount { get; set; }

    public int MilitiaCount { get; set; }

    public int EscortCount { get; set; }

    public int Readiness { get; set; }

    public int CommandCapacity { get; set; }

    public string LastConflictTrace { get; set; } = string.Empty;
}
