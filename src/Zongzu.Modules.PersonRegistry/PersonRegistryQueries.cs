using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PersonRegistry;

internal sealed class PersonRegistryQueries : IPersonRegistryQueries
{
    private readonly PersonRegistryState _state;

    public PersonRegistryQueries(PersonRegistryState state)
    {
        _state = state;
    }

    public bool TryGetPerson(PersonId id, out PersonRecord person)
    {
        foreach (PersonRecord candidate in _state.Persons)
        {
            if (candidate.Id.Equals(id))
            {
                person = candidate;
                return true;
            }
        }

        person = null!;
        return false;
    }

    public IReadOnlyList<PersonRecord> GetAllPersons()
    {
        return _state.Persons
            .OrderBy(static p => p.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<PersonRecord> GetPersonsByFidelityRing(FidelityRing ring)
    {
        return _state.Persons
            .Where(p => p.FidelityRing == ring)
            .OrderBy(static p => p.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<PersonRecord> GetLivingPersons()
    {
        return _state.Persons
            .Where(static p => p.IsAlive)
            .OrderBy(static p => p.Id.Value)
            .ToArray();
    }

    public bool IsAlive(PersonId id)
    {
        foreach (PersonRecord candidate in _state.Persons)
        {
            if (candidate.Id.Equals(id))
            {
                return candidate.IsAlive;
            }
        }

        return false;
    }

    public int GetAgeMonths(PersonId id, GameDate currentDate)
    {
        foreach (PersonRecord candidate in _state.Persons)
        {
            if (candidate.Id.Equals(id))
            {
                return PersonRegistryModule.ComputeAgeMonths(candidate.BirthDate, currentDate);
            }
        }

        return -1;
    }
}
