using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// Identity-only life stage bucket maintained by <c>PersonRegistry</c>.
/// Domain-specific age semantics (e.g. exam eligibility, military age) stay
/// inside their owning modules and are NOT derived from this enum.
/// </summary>
public enum LifeStage
{
    Infant = 0,
    Child = 1,
    Youth = 2,
    Adult = 3,
    Elder = 4,
    Deceased = 5,
}

/// <summary>
/// Simulation fidelity ring assignment. See <c>SIMULATION_FIDELITY_MODEL.md</c>.
/// Fidelity ring drives how much simulation precision is spent on a person;
/// it is not a social status ranking.
/// </summary>
public enum FidelityRing
{
    Core = 0,
    Local = 1,
    Regional = 2,
}

public enum PersonGender
{
    Unspecified = 0,
    Male = 1,
    Female = 2,
}

/// <summary>
/// Identity-only record owned by <c>PersonRegistry</c>.
/// Answers only: does this person exist, are they alive, how old are they,
/// and at what simulation precision ring. Any field answering
/// "what are they doing / feeling / capable of / related to" belongs in a
/// domain module, not here. See <c>PERSON_OWNERSHIP_RULES.md</c>.
/// </summary>
public sealed class PersonRecord
{
    public PersonId Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public GameDate BirthDate { get; set; }

    public PersonGender Gender { get; set; } = PersonGender.Unspecified;

    public LifeStage LifeStage { get; set; } = LifeStage.Infant;

    public bool IsAlive { get; set; } = true;

    public FidelityRing FidelityRing { get; set; } = FidelityRing.Local;
}

/// <summary>
/// Read-only view over the identity anchor table. Domain modules use this to
/// confirm a person exists / is alive / life stage bucket without reaching
/// into any domain module's private state.
/// </summary>
public interface IPersonRegistryQueries
{
    bool TryGetPerson(PersonId id, out PersonRecord person);

    IReadOnlyList<PersonRecord> GetAllPersons();

    IReadOnlyList<PersonRecord> GetPersonsByFidelityRing(FidelityRing ring);

    IReadOnlyList<PersonRecord> GetLivingPersons();

    /// <summary>
    /// Returns <see langword="true"/> only when the person exists in the
    /// registry and <see cref="PersonRecord.IsAlive"/> is set. Returns
    /// <see langword="false"/> for unknown persons so callers can distinguish
    /// "not registered" from "alive" via <see cref="TryGetPerson"/>.
    /// </summary>
    bool IsAlive(PersonId id);

    /// <summary>
    /// Computes the person's current age in months using their registered
    /// <see cref="PersonRecord.BirthDate"/>. Returns <c>-1</c> when the person
    /// is not registered so domain modules can fall back to their own
    /// local age mirror during the transitional period.
    /// </summary>
    int GetAgeMonths(PersonId id, GameDate currentDate);
}

/// <summary>
/// Identity-creation write surface. Domain modules that create new persons
/// (FamilyCore on birth, PopulationAndHouseholds on immigration, etc.) call
/// <see cref="Register"/> synchronously so PersonRegistry remains the single
/// identity-of-record — see <c>PERSON_OWNERSHIP_RULES.md §249</c>.
///
/// This is deliberately narrow: only identity-shaped fields are accepted.
/// Domain state (clan membership, health, skills) is still the caller's
/// responsibility and must be written to the caller's own module state.
/// </summary>
public interface IPersonRegistryCommands
{
    /// <summary>
    /// Registers a new person. Returns <see langword="false"/> when a person
    /// with the same <paramref name="id"/> is already registered — callers
    /// should treat that as a programmer error, not as a retry signal.
    /// Emits <c>PersonCreated</c> on the execution scope on success.
    /// </summary>
    bool Register(
        ModuleExecutionContext context,
        PersonId id,
        string displayName,
        GameDate birthDate,
        PersonGender gender,
        FidelityRing fidelityRing);

    /// <summary>
    /// Marks an existing person as deceased. Returns <see langword="false"/>
    /// when the person is not registered or was already dead — both are
    /// idempotent no-ops so domain modules can call this without first
    /// checking registry state. Emits <c>PersonDeceased</c> on success.
    ///
    /// Domain modules (FamilyCore on clan death, ConflictAndForce on
    /// violence, etc.) call this synchronously as the authoritative death
    /// write; cause-specific events like <c>ClanMemberDied</c> still flow
    /// for downstream flavor consumers but no longer drive the death state
    /// in the registry.
    /// </summary>
    bool MarkDeceased(ModuleExecutionContext context, PersonId id);
}

public static class PersonRegistryEventNames
{
    /// <summary>
    /// Raised by PersonRegistry after it has consolidated a cause-specific
    /// death event from a domain module (e.g. <c>ClanMemberDied</c>,
    /// <c>DeathByIllness</c>, <c>DeathByViolence</c>).
    /// This is the single canonical "person is now deceased" signal that
    /// downstream projections (NarrativeProjection) should read.
    /// </summary>
    public const string PersonDeceased = "PersonDeceased";

    /// <summary>
    /// Raised when a new PersonRecord is registered.
    /// </summary>
    public const string PersonCreated = "PersonCreated";

    /// <summary>
    /// Raised when a person's simulation fidelity ring changes.
    /// </summary>
    public const string FidelityRingChanged = "FidelityRingChanged";
}

/// <summary>
/// Domain-specific death cause event names.
/// Each domain module emits its own cause-specific event; <c>PersonRegistry</c>
/// consumes them and consolidates into <c>PersonDeceased</c>.
/// See <c>PERSON_OWNERSHIP_RULES.md</c>.
/// </summary>
public static class DeathCauseEventNames
{
    /// <summary>
    /// Emitted by FamilyCore when a clan member dies. Entity key is PersonId.
    /// </summary>
    public const string ClanMemberDied = "ClanMemberDied";

    /// <summary>
    /// Emitted by PopulationAndHouseholds when a person dies of illness or
    /// subsistence collapse. Reserved for Phase 2+ when person-level health is
    /// introduced in that module. Entity key is PersonId.
    /// </summary>
    public const string DeathByIllness = "DeathByIllness";

    /// <summary>
    /// Emitted by ConflictAndForce / OrderAndBanditry / WarfareCampaign when a
    /// person dies from violent conflict. Reserved for Phase 2+ when
    /// person-level martial state is introduced. Entity key is PersonId.
    /// </summary>
    public const string DeathByViolence = "DeathByViolence";
}
