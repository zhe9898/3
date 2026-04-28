using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PersonRegistry;

/// <summary>
/// Default <see cref="IPersonRegistryCommands"/> implementation. Domain
/// modules call this through the QueryRegistry so PersonRegistry stays the
/// sole identity-of-record — see <c>PERSON_OWNERSHIP_RULES.md §249</c>.
/// </summary>
internal sealed class PersonRegistryCommands : IPersonRegistryCommands
{
    private readonly PersonRegistryState _state;

    public PersonRegistryCommands(PersonRegistryState state)
    {
        _state = state;
    }

    public bool Register(
        ModuleExecutionContext context,
        PersonId id,
        string displayName,
        GameDate birthDate,
        PersonGender gender,
        FidelityRing fidelityRing)
    {
        foreach (PersonRecord existing in _state.Persons)
        {
            if (existing.Id.Equals(id))
            {
                return false;
            }
        }

        int ageMonths = PersonRegistryModule.ComputeAgeMonths(birthDate, context.CurrentDate);
        PersonRecord record = new()
        {
            Id = id,
            DisplayName = displayName,
            BirthDate = birthDate,
            Gender = gender,
            FidelityRing = fidelityRing,
            IsAlive = true,
            LifeStage = PersonRegistryModule.ResolveLifeStage(ageMonths),
        };
        _state.Persons.Add(record);

        context.DomainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.PersonRegistry,
            PersonRegistryEventNames.PersonCreated,
            $"{displayName}登入宗房册。",
            id.Value.ToString()));

        return true;
    }

    public bool MarkDeceased(ModuleExecutionContext context, PersonId id)
    {
        PersonRecord? target = null;
        foreach (PersonRecord existing in _state.Persons)
        {
            if (existing.Id.Equals(id))
            {
                target = existing;
                break;
            }
        }

        if (target is null || !target.IsAlive)
        {
            return false;
        }

        target.IsAlive = false;
        target.LifeStage = LifeStage.Deceased;

        context.DomainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.PersonRegistry,
            PersonRegistryEventNames.PersonDeceased,
            $"{target.DisplayName}身故登记。",
            id.Value.ToString()));

        return true;
    }

    public bool ChangeFidelityRing(
        ModuleExecutionContext context,
        PersonId id,
        FidelityRing targetRing,
        string reason)
    {
        PersonRecord? target = null;
        foreach (PersonRecord existing in _state.Persons)
        {
            if (existing.Id.Equals(id))
            {
                target = existing;
                break;
            }
        }

        if (target is null || !target.IsAlive || target.FidelityRing == targetRing)
        {
            return false;
        }

        FidelityRing previousRing = target.FidelityRing;
        target.FidelityRing = targetRing;

        context.DomainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.PersonRegistry,
            PersonRegistryEventNames.FidelityRingChanged,
            $"{target.DisplayName}由{previousRing}环调入{targetRing}环：{reason}。",
            id.Value.ToString(),
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.PersonId] = id.Value.ToString(),
                [DomainEventMetadataKeys.FidelityRingBefore] = previousRing.ToString(),
                [DomainEventMetadataKeys.FidelityRingAfter] = targetRing.ToString(),
                [DomainEventMetadataKeys.FidelityRingReason] = reason,
            }));

        return true;
    }
}
