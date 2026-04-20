using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record HouseholdPressureSnapshot
{
    public HouseholdId Id { get; init; }

    public string HouseholdName { get; init; } = string.Empty;

    public SettlementId SettlementId { get; init; }

    public ClanId? SponsorClanId { get; init; }

    public int Distress { get; init; }

    public int DebtPressure { get; init; }

    public int LaborCapacity { get; init; }

    public int MigrationRisk { get; init; }

    public bool IsMigrating { get; init; }
}

public sealed record PopulationSettlementSnapshot
{
    public SettlementId SettlementId { get; init; }

    public int CommonerDistress { get; init; }

    public int LaborSupply { get; init; }

    public int MigrationPressure { get; init; }

    public int MilitiaPotential { get; init; }
}

public interface IPopulationAndHouseholdsQueries
{
    HouseholdPressureSnapshot GetRequiredHousehold(HouseholdId householdId);

    PopulationSettlementSnapshot GetRequiredSettlement(SettlementId settlementId);

    IReadOnlyList<HouseholdPressureSnapshot> GetHouseholds();

    IReadOnlyList<PopulationSettlementSnapshot> GetSettlements();
}
