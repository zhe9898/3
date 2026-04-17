using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed class PopulationAndHouseholdsState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.PopulationAndHouseholds;

    public List<PopulationHouseholdState> Households { get; set; } = new();

    public List<PopulationSettlementState> Settlements { get; set; } = new();
}

public sealed class PopulationHouseholdState
{
    public HouseholdId Id { get; set; }

    public string HouseholdName { get; set; } = string.Empty;

    public SettlementId SettlementId { get; set; }

    public ClanId? SponsorClanId { get; set; }

    public int Distress { get; set; }

    public int DebtPressure { get; set; }

    public int LaborCapacity { get; set; }

    public int MigrationRisk { get; set; }

    public bool IsMigrating { get; set; }
}

public sealed class PopulationSettlementState
{
    public SettlementId SettlementId { get; set; }

    public int CommonerDistress { get; set; }

    public int LaborSupply { get; set; }

    public int MigrationPressure { get; set; }

    public int MilitiaPotential { get; set; }
}
