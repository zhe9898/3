using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record ClanSnapshot
{
    public ClanId Id { get; init; }

    public string ClanName { get; init; } = string.Empty;

    public SettlementId HomeSettlementId { get; init; }

    public int Prestige { get; init; }

    public int SupportReserve { get; init; }

    public PersonId? HeirPersonId { get; init; }

    public int BranchTension { get; init; }

    public int InheritancePressure { get; init; }

    public int SeparationPressure { get; init; }

    public int MediationMomentum { get; init; }

    public int BranchFavorPressure { get; init; }

    public int ReliefSanctionPressure { get; init; }

    public int MarriageAlliancePressure { get; init; }

    public int MarriageAllianceValue { get; init; }

    public int HeirSecurity { get; init; }

    public int ReproductivePressure { get; init; }

    public int MourningLoad { get; init; }

    public int InfantCount { get; init; }

    public string LastConflictCommandCode { get; init; } = string.Empty;

    public string LastConflictCommandLabel { get; init; } = string.Empty;

    public string LastConflictOutcome { get; init; } = string.Empty;

    public string LastConflictTrace { get; init; } = string.Empty;

    public string LastLifecycleCommandCode { get; init; } = string.Empty;

    public string LastLifecycleCommandLabel { get; init; } = string.Empty;

    public string LastLifecycleOutcome { get; init; } = string.Empty;

    public string LastLifecycleTrace { get; init; } = string.Empty;
}

public interface IFamilyCoreQueries
{
    ClanSnapshot GetRequiredClan(ClanId clanId);

    IReadOnlyList<ClanSnapshot> GetClans();
}
