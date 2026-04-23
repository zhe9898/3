using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record LocalForcePoolSnapshot
{
    public SettlementId SettlementId { get; init; }

    public int GuardCount { get; init; }

    public int RetainerCount { get; init; }

    public int MilitiaCount { get; init; }

    public int EscortCount { get; init; }

    public int Readiness { get; init; }

    public int CommandCapacity { get; init; }

    public int ResponseActivationLevel { get; init; }

    public int OrderSupportLevel { get; init; }

    public bool IsResponseActivated { get; init; }

    public bool HasActiveConflict { get; init; }

    public int CampaignFatigue { get; init; }

    public int CampaignEscortStrain { get; init; }

    public string LastCampaignFalloutTrace { get; init; } = string.Empty;

    public string LastConflictTrace { get; init; } = string.Empty;
}

public sealed record ForceGroupSnapshot
{
    public string ForceId { get; init; } = string.Empty;

    public ForceFamily Family { get; init; } = ForceFamily.Unknown;

    public string OwnerKey { get; init; } = string.Empty;

    public SettlementId Location { get; init; }

    public int Strength { get; init; }

    public int Readiness { get; init; }

    public int Morale { get; init; }

    public int Discipline { get; init; }

    public int Fatigue { get; init; }
}

public sealed record ConflictIncidentSnapshot
{
    public string IncidentId { get; init; } = string.Empty;

    public IncidentScale Scale { get; init; } = IncidentScale.Unknown;

    public SettlementId Location { get; init; }

    public string RouteId { get; init; } = string.Empty;

    public IReadOnlyList<string> Attackers { get; init; } = System.Array.Empty<string>();

    public IReadOnlyList<string> Defenders { get; init; } = System.Array.Empty<string>();

    public IncidentOutcome Outcome { get; init; } = IncidentOutcome.Pending;

    public string CauseKey { get; init; } = string.Empty;

    public int OccurredYear { get; init; }

    public int OccurredMonth { get; init; }
}

public interface IConflictAndForceQueries
{
    LocalForcePoolSnapshot GetRequiredSettlementForce(SettlementId settlementId);

    IReadOnlyList<LocalForcePoolSnapshot> GetSettlementForces();

    IReadOnlyList<ForceGroupSnapshot> GetForceGroups() => System.Array.Empty<ForceGroupSnapshot>();

    IReadOnlyList<ConflictIncidentSnapshot> GetConflictIncidents() => System.Array.Empty<ConflictIncidentSnapshot>();
}
