using System;
using System.Collections.Generic;

namespace Zongzu.Application;

internal sealed class SimulationStateStore
{
    private readonly Dictionary<string, object> _states = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, object> States => _states;

    public void Set(string moduleKey, object state)
    {
        _states[moduleKey] = state ?? throw new ArgumentNullException(nameof(state));
    }

    public object GetRequired(string moduleKey)
    {
        if (_states.TryGetValue(moduleKey, out object? state))
        {
            return state;
        }

        throw new InvalidOperationException($"State for module {moduleKey} is not present.");
    }

    public TState GetRequired<TState>(string moduleKey)
        where TState : class
    {
        return GetRequired(moduleKey) as TState
            ?? throw new InvalidOperationException($"State for module {moduleKey} is not of type {typeof(TState).Name}.");
    }
}
