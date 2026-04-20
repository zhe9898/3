using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed class FamilyCoreState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.FamilyCore;

    public List<ClanStateData> Clans { get; set; } = new();

    public List<FamilyPersonState> People { get; set; } = new();
}

public sealed class ClanStateData
{
    public ClanId Id { get; set; }

    public string ClanName { get; set; } = string.Empty;

    public SettlementId HomeSettlementId { get; set; }

    public int Prestige { get; set; }

    public int SupportReserve { get; set; }

    public PersonId? HeirPersonId { get; set; }

    public int BranchTension { get; set; }

    public int InheritancePressure { get; set; }

    public int SeparationPressure { get; set; }

    public int MediationMomentum { get; set; }

    public int BranchFavorPressure { get; set; }

    public int ReliefSanctionPressure { get; set; }

    public int MarriageAlliancePressure { get; set; }

    public int MarriageAllianceValue { get; set; }

    public int HeirSecurity { get; set; }

    public int ReproductivePressure { get; set; }

    public int MourningLoad { get; set; }

    public string LastConflictCommandCode { get; set; } = string.Empty;

    public string LastConflictCommandLabel { get; set; } = string.Empty;

    public string LastConflictOutcome { get; set; } = string.Empty;

    public string LastConflictTrace { get; set; } = string.Empty;

    public string LastLifecycleCommandCode { get; set; } = string.Empty;

    public string LastLifecycleCommandLabel { get; set; } = string.Empty;

    public string LastLifecycleOutcome { get; set; } = string.Empty;

    public string LastLifecycleTrace { get; set; } = string.Empty;
}

public sealed class FamilyPersonState
{
    public PersonId Id { get; set; }

    public ClanId ClanId { get; set; }

    public string GivenName { get; set; } = string.Empty;

    public int AgeMonths { get; set; }

    public bool IsAlive { get; set; }
}
