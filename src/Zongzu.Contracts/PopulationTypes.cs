using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// Household livelihood archetype. Owned by <c>PopulationAndHouseholds</c>.
/// See <c>LIVING_WORLD_DESIGN.md §2.3</c> — drives household distress baseline
/// and (Phase 3+) downstream labor / migration / debt propensity.
/// </summary>
public enum LivelihoodType
{
    Unknown = 0,
    Smallholder = 1,
    Tenant = 2,
    HiredLabor = 3,
    Artisan = 4,
    PettyTrader = 5,
    Boatman = 6,
    DomesticServant = 7,
    YamenRunner = 8,
    SeasonalMigrant = 9,
    Vagrant = 10,
}

/// <summary>
/// Person-level health bucket. Owned by <c>PopulationAndHouseholds</c> via
/// <see cref="HouseholdMembership"/>. See <c>PERSON_OWNERSHIP_RULES.md §2.2</c>.
/// Illness progression: Healthy → Ailing → Ill → Bedridden → Moribund → Deceased.
/// Deceased is signalled via <c>PersonRegistry.MarkDeceased</c>; this enum does
/// not carry a Deceased bucket.
/// </summary>
public enum HealthStatus
{
    Unknown = 0,
    Healthy = 1,
    Ailing = 2,
    Ill = 3,
    Bedridden = 4,
    Moribund = 5,
}

/// <summary>
/// Coarse-grained person activity bucket. Owned by
/// <c>PopulationAndHouseholds</c>. Drives fidelity-ring routing for wider-ring
/// summaries in later phases.
/// </summary>
public enum PersonActivity
{
    Unknown = 0,
    Idle = 1,
    Farming = 2,
    Laboring = 3,
    Trading = 4,
    Migrating = 5,
    Studying = 6,
    Serving = 7,
    Convalescing = 8,
}

/// <summary>
/// Read-only snapshot of a household membership record.
/// Authored by <c>PopulationAndHouseholds</c>. Person identity itself stays in
/// <c>PersonRegistry</c>; this is the population-domain projection of that
/// person's domestic placement, livelihood participation, and health.
/// </summary>
public sealed record HouseholdMembershipSnapshot
{
    public PersonId PersonId { get; init; }

    public HouseholdId HouseholdId { get; init; }

    public LivelihoodType Livelihood { get; init; } = LivelihoodType.Smallholder;

    public int HealthResilience { get; init; }

    public HealthStatus Health { get; init; } = HealthStatus.Healthy;

    public int IllnessMonths { get; init; }

    public PersonActivity Activity { get; init; } = PersonActivity.Idle;
}

/// <summary>
/// Regional-ring labor pool entry. See <c>SIMULATION_FIDELITY_MODEL.md</c>.
/// Summary carrier, not an individual projection.
/// </summary>
public sealed record LaborPoolEntrySnapshot
{
    public SettlementId SettlementId { get; init; }

    public int AvailableLabor { get; init; }

    public int LaborDemand { get; init; }

    public int SeasonalSurplus { get; init; }

    public int WageLevel { get; init; }
}

/// <summary>
/// Regional-ring marriage pool entry. Summary carrier.
/// </summary>
public sealed record MarriagePoolEntrySnapshot
{
    public SettlementId SettlementId { get; init; }

    public int EligibleMales { get; init; }

    public int EligibleFemales { get; init; }

    public int MatchDifficulty { get; init; }
}

/// <summary>
/// Regional-ring migration pool entry. Summary carrier.
/// </summary>
public sealed record MigrationPoolEntrySnapshot
{
    public SettlementId SettlementId { get; init; }

    public int OutflowPressure { get; init; }

    public int InflowPressure { get; init; }

    public int FloatingPopulation { get; init; }
}

/// <summary>
/// Canonical event name constants emitted by <c>PopulationAndHouseholds</c>.
/// Mirrors <c>DeathCauseEventNames.DeathByIllness</c> by value so the
/// PersonRegistry event-consolidator continues to work.
/// </summary>
public static class PopulationEventNames
{
    public const string HouseholdDebtSpiked = "HouseholdDebtSpiked";

    public const string MigrationStarted = "MigrationStarted";

    public const string LaborShortage = "LaborShortage";

    public const string LivelihoodCollapsed = "LivelihoodCollapsed";

    /// <summary>
    /// Same string value as <see cref="DeathCauseEventNames.DeathByIllness"/>.
    /// Emitted alongside <c>IPersonRegistryCommands.MarkDeceased</c>: the
    /// registry consolidates this into <c>PersonDeceased</c>, and downstream
    /// flavor consumers can subscribe to this cause-specific event directly.
    /// </summary>
    public const string DeathByIllness = DeathCauseEventNames.DeathByIllness;
}
