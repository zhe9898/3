using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record HouseholdPressureSnapshot
{
    public HouseholdId Id { get; init; }

    public string HouseholdName { get; init; } = string.Empty;

    public SettlementId SettlementId { get; init; }

    public ClanId? SponsorClanId { get; init; }

    public LivelihoodType Livelihood { get; init; } = LivelihoodType.Smallholder;

    public int Distress { get; init; }

    public int DebtPressure { get; init; }

    public int LaborCapacity { get; init; }

    public int MigrationRisk { get; init; }

    public bool IsMigrating { get; init; }

    public int LandHolding { get; init; }

    public int GrainStore { get; init; }

    public int ToolCondition { get; init; }

    public int ShelterQuality { get; init; }

    public int DependentCount { get; init; }

    public int LaborerCount { get; init; }

    public string LastLocalResponseCommandCode { get; init; } = string.Empty;

    public string LastLocalResponseCommandLabel { get; init; } = string.Empty;

    public string LastLocalResponseOutcomeCode { get; init; } = string.Empty;

    public string LastLocalResponseTraceCode { get; init; } = string.Empty;

    public string LastLocalResponseSummary { get; init; } = string.Empty;

    public int LocalResponseCarryoverMonths { get; init; }
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

    // Phase 3 生计骨骼 additions. Default implementations keep legacy stubs
    // (e.g. `PublicLifeAndRumor` test doubles) compiling until those stubs
    // need to expose memberships / pools; concrete module overrides them.
    IReadOnlyList<HouseholdMembershipSnapshot> GetMemberships() => [];

    IReadOnlyList<HouseholdMembershipSnapshot> GetMembershipsByHousehold(HouseholdId householdId) => [];

    bool TryGetMembership(PersonId personId, out HouseholdMembershipSnapshot membership)
    {
        membership = new HouseholdMembershipSnapshot();
        return false;
    }

    IReadOnlyList<LaborPoolEntrySnapshot> GetLaborPools() => [];

    IReadOnlyList<MarriagePoolEntrySnapshot> GetMarriagePools() => [];

    IReadOnlyList<MigrationPoolEntrySnapshot> GetMigrationPools() => [];
}
