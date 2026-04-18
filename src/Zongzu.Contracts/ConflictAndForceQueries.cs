using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class LocalForcePoolSnapshot
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

public interface IConflictAndForceQueries
{
    LocalForcePoolSnapshot GetRequiredSettlementForce(SettlementId settlementId);

    IReadOnlyList<LocalForcePoolSnapshot> GetSettlementForces();
}
