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

public interface IConflictAndForceQueries
{
    LocalForcePoolSnapshot GetRequiredSettlementForce(SettlementId settlementId);

    IReadOnlyList<LocalForcePoolSnapshot> GetSettlementForces();
}
