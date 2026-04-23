using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PersonRegistry;

/// <summary>
/// Kernel-layer person identity anchor module.
///
/// Responsibilities:
///  1. Advance age / life stage at the start of each month (Phase 0).
///  2. Consolidate cause-specific death events from domain modules
///     (ClanMemberDied / DeathByIllness / DeathByViolence) into the canonical
///     <c>PersonDeceased</c> signal.
///  3. Publish <see cref="IPersonRegistryQueries"/> so domain modules can
///     reference each other's persons by PersonId without any module becoming
///     the "person master table".
///
/// This module is identity-only. See <c>PERSON_OWNERSHIP_RULES.md</c> and
/// <c>MODULE_BOUNDARIES.md §0</c>. Do not add domain fields here.
/// </summary>
public sealed class PersonRegistryModule : ModuleRunner<PersonRegistryState>
{
    // Age buckets in months. These are IDENTITY-ONLY life stages and are
    // intentionally coarse. Domain modules maintain their own age-derived
    // gates (exam-eligible, adult-male labor, military age, etc.).
    private const int InfantMaxAgeMonths = 2 * 12;
    private const int ChildMaxAgeMonths = 12 * 12;
    private const int YouthMaxAgeMonths = 18 * 12;
    private const int AdultMaxAgeMonths = 55 * 12;

    private static readonly string[] ConsumedEventNames =
    [
        DeathCauseEventNames.ClanMemberDied,
        DeathCauseEventNames.DeathByIllness,
        DeathCauseEventNames.DeathByViolence,
        // Step 1b gap 4: branch separation → person settlement re-anchor (no-op dispatch)
        FamilyCoreEventNames.BranchSeparationApproved,
    ];

    private static readonly string[] PublishedEventNames =
    [
        PersonRegistryEventNames.PersonCreated,
        PersonRegistryEventNames.PersonDeceased,
        PersonRegistryEventNames.FidelityRingChanged,
    ];

    public override string ModuleKey => KnownModuleKeys.PersonRegistry;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.Prepare;

    public override int ExecutionOrder => 0;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override IReadOnlyCollection<string> PublishedEvents => PublishedEventNames;

    public override PersonRegistryState CreateInitialState()
    {
        return new PersonRegistryState();
    }

    public override void RegisterQueries(PersonRegistryState state, QueryRegistry queries)
    {
        queries.Register<IPersonRegistryQueries>(new PersonRegistryQueries(state));
        queries.Register<IPersonRegistryCommands>(new PersonRegistryCommands(state));
    }

    public override void RunMonth(ModuleExecutionScope<PersonRegistryState> scope)
    {
        // Phase 0: age progression and life-stage re-check, so every domain
        // module that runs afterwards sees a current life stage.
        GameDate current = scope.Context.CurrentDate;
        foreach (PersonRecord person in scope.State.Persons)
        {
            if (!person.IsAlive)
            {
                continue;
            }

            int ageMonths = ComputeAgeMonths(person.BirthDate, current);
            LifeStage previous = person.LifeStage;
            LifeStage resolved = ResolveLifeStage(ageMonths);
            if (resolved != previous)
            {
                person.LifeStage = resolved;
            }
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<PersonRegistryState> scope)
    {
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (!IsCauseSpecificDeath(domainEvent.EventType))
            {
                continue;
            }

            if (!TryParsePersonId(domainEvent.EntityKey, out PersonId personId))
            {
                // Some death events (e.g. historical ClanMemberDied records
                // keyed by ClanId) do not carry a PersonId. Those are
                // legacy-shaped and will be tightened in Phase 2.
                continue;
            }

            PersonRecord? target = scope.State.Persons.FirstOrDefault(p => p.Id.Equals(personId));
            if (target is null || !target.IsAlive)
            {
                continue;
            }

            target.IsAlive = false;
            target.LifeStage = LifeStage.Deceased;
            scope.Emit(
                PersonRegistryEventNames.PersonDeceased,
                $"{target.DisplayName}身故登记。",
                target.Id.Value.ToString());
        }
    }

    public static LifeStage ResolveLifeStage(int ageMonths)
    {
        if (ageMonths < InfantMaxAgeMonths)
        {
            return LifeStage.Infant;
        }

        if (ageMonths < ChildMaxAgeMonths)
        {
            return LifeStage.Child;
        }

        if (ageMonths < YouthMaxAgeMonths)
        {
            return LifeStage.Youth;
        }

        if (ageMonths < AdultMaxAgeMonths)
        {
            return LifeStage.Adult;
        }

        return LifeStage.Elder;
    }

    public static int ComputeAgeMonths(GameDate birth, GameDate current)
    {
        int months = ((current.Year - birth.Year) * 12) + (current.Month - birth.Month);
        return Math.Max(0, months);
    }

    private static bool IsCauseSpecificDeath(string eventType)
    {
        return eventType == DeathCauseEventNames.ClanMemberDied
            || eventType == DeathCauseEventNames.DeathByIllness
            || eventType == DeathCauseEventNames.DeathByViolence;
    }

    private static bool TryParsePersonId(string? entityKey, out PersonId personId)
    {
        personId = default;
        if (string.IsNullOrEmpty(entityKey))
        {
            return false;
        }

        if (int.TryParse(entityKey, out int value))
        {
            personId = new PersonId(value);
            return true;
        }

        return false;
    }
}
