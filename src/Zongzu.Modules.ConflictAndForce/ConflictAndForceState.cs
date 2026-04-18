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

    public int ResponseActivationLevel { get; set; }

    public int OrderSupportLevel { get; set; }

    public bool IsResponseActivated { get; set; }

    public bool HasActiveConflict { get; set; }

    public int CampaignFatigue { get; set; }

    public int CampaignEscortStrain { get; set; }

    public string LastCampaignFalloutTrace { get; set; } = string.Empty;

    public string LastConflictTrace { get; set; } = string.Empty;
}
