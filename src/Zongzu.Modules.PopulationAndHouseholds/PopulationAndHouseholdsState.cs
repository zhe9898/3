using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed class PopulationAndHouseholdsState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.PopulationAndHouseholds;

    public List<PopulationHouseholdState> Households { get; set; } = new();

    public List<PopulationSettlementState> Settlements { get; set; } = new();

    public List<HouseholdMembershipState> Memberships { get; set; } = new();

    public List<LaborPoolEntryState> LaborPools { get; set; } = new();

    public List<MarriagePoolEntryState> MarriagePools { get; set; } = new();

    public List<MigrationPoolEntryState> MigrationPools { get; set; } = new();
}

public sealed class PopulationHouseholdState
{
    public HouseholdId Id { get; set; }

    public string HouseholdName { get; set; } = string.Empty;

    public SettlementId SettlementId { get; set; }

    public ClanId? SponsorClanId { get; set; }

    public LivelihoodType Livelihood { get; set; } = LivelihoodType.Smallholder;

    public int Distress { get; set; }

    public int DebtPressure { get; set; }

    public int LaborCapacity { get; set; }

    public int MigrationRisk { get; set; }

    public bool IsMigrating { get; set; }

    public int LandHolding { get; set; }

    public int GrainStore { get; set; }

    public int ToolCondition { get; set; }

    public int ShelterQuality { get; set; }

    public int DependentCount { get; set; }

    public int LaborerCount { get; set; }

    public string LastLocalResponseCommandCode { get; set; } = string.Empty;

    public string LastLocalResponseCommandLabel { get; set; } = string.Empty;

    public string LastLocalResponseOutcomeCode { get; set; } = string.Empty;

    public string LastLocalResponseTraceCode { get; set; } = string.Empty;

    public string LastLocalResponseSummary { get; set; } = string.Empty;

    public int LocalResponseCarryoverMonths { get; set; }
}

public sealed class PopulationSettlementState
{
    public SettlementId SettlementId { get; set; }

    public int CommonerDistress { get; set; }

    public int LaborSupply { get; set; }

    public int MigrationPressure { get; set; }

    public int MilitiaPotential { get; set; }
}

public sealed class HouseholdMembershipState
{
    public PersonId PersonId { get; set; }

    public HouseholdId HouseholdId { get; set; }

    public LivelihoodType Livelihood { get; set; } = LivelihoodType.Smallholder;

    public int HealthResilience { get; set; } = 50;

    public HealthStatus Health { get; set; } = HealthStatus.Healthy;

    public int IllnessMonths { get; set; }

    public PersonActivity Activity { get; set; } = PersonActivity.Idle;
}

public sealed class LaborPoolEntryState
{
    public SettlementId SettlementId { get; set; }

    public int AvailableLabor { get; set; }

    public int LaborDemand { get; set; }

    public int SeasonalSurplus { get; set; }

    public int WageLevel { get; set; }
}

public sealed class MarriagePoolEntryState
{
    public SettlementId SettlementId { get; set; }

    public int EligibleMales { get; set; }

    public int EligibleFemales { get; set; }

    public int MatchDifficulty { get; set; }
}

public sealed class MigrationPoolEntryState
{
    public SettlementId SettlementId { get; set; }

    public int OutflowPressure { get; set; }

    public int InflowPressure { get; set; }

    public int FloatingPopulation { get; set; }
}
