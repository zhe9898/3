using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class HouseholdPressureSnapshot
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

public sealed class PopulationSettlementSnapshot
{
    public SettlementId SettlementId { get; set; }

    public int CommonerDistress { get; set; }

    public int LaborSupply { get; set; }

    public int MigrationPressure { get; set; }

    public int MilitiaPotential { get; set; }
}

public interface IPopulationAndHouseholdsQueries
{
    HouseholdPressureSnapshot GetRequiredHousehold(HouseholdId householdId);

    PopulationSettlementSnapshot GetRequiredSettlement(SettlementId settlementId);

    IReadOnlyList<HouseholdPressureSnapshot> GetHouseholds();

    IReadOnlyList<PopulationSettlementSnapshot> GetSettlements();
}
