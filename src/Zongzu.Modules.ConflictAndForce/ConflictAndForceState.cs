using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed class ConflictAndForceState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.ConflictAndForce;

    public List<SettlementForceState> Settlements { get; set; } = new();

    public List<ForceGroupState> ForceGroups { get; set; } = new();

    public List<ConflictIncidentState> Incidents { get; set; } = new();
}

public sealed class ForceGroupState
{
    public string ForceId { get; set; } = string.Empty;

    public ForceFamily Family { get; set; } = ForceFamily.Unknown;

    public string OwnerKey { get; set; } = string.Empty;

    public SettlementId Location { get; set; }

    public int Strength { get; set; }

    public int Readiness { get; set; }

    public int Morale { get; set; }

    public int Discipline { get; set; }

    public int Fatigue { get; set; }
}

public sealed class ConflictIncidentState
{
    public string IncidentId { get; set; } = string.Empty;

    public IncidentScale Scale { get; set; } = IncidentScale.Unknown;

    public SettlementId Location { get; set; }

    public string RouteId { get; set; } = string.Empty;

    public List<string> Attackers { get; set; } = new();

    public List<string> Defenders { get; set; } = new();

    public IncidentOutcome Outcome { get; set; } = IncidentOutcome.Pending;

    public string CauseKey { get; set; } = string.Empty;

    public int OccurredYear { get; set; }

    public int OccurredMonth { get; set; }
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
