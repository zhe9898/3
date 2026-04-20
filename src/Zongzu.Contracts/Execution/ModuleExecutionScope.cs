using System;

namespace Zongzu.Contracts;

public sealed class ModuleExecutionScope<TState>
    where TState : class
{
    public ModuleExecutionScope(TState state, ModuleExecutionContext context)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public TState State { get; }

    public ModuleExecutionContext Context { get; }

    public TQuery GetRequiredQuery<TQuery>()
        where TQuery : class
    {
        return Context.Queries.GetRequired<TQuery>();
    }

    public void Emit(string eventType, string summary, string? entityKey = null)
    {
        Context.DomainEvents.Emit(new DomainEventRecord(ContextDiffModuleKey(), eventType, summary, entityKey));
    }

    public void RecordDiff(string description, string? entityKey = null)
    {
        Context.Diff.Record(ContextDiffModuleKey(), description, entityKey);
    }

    private string ContextDiffModuleKey()
    {
        if (State is IModuleStateDescriptor stateDescriptor)
        {
            return stateDescriptor.ModuleKey;
        }

        throw new InvalidOperationException($"State type {typeof(TState).Name} does not expose a module key.");
    }
}
