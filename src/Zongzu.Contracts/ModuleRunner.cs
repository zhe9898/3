using System;
using System.Collections.Generic;

namespace Zongzu.Contracts;

public interface IModuleRunner
{
    string ModuleKey { get; }

    string StateNamespace { get; }

    int ModuleSchemaVersion { get; }

    SimulationPhase Phase { get; }

    int ExecutionOrder { get; }

    FeatureMode DefaultMode { get; }

    Type StateType { get; }

    IReadOnlyCollection<string> AcceptedCommands { get; }

    IReadOnlyCollection<string> PublishedEvents { get; }

    IReadOnlyCollection<string> ConsumedEvents { get; }

    object CreateInitialState();

    void RegisterQueries(object state, QueryRegistry queries);

    void RunMonth(ModuleExecutionContext context, object state);

    void HandleEvents(ModuleExecutionContext context, object state, IReadOnlyList<IDomainEvent> events);
}

public interface IModuleStateDescriptor
{
    string ModuleKey { get; }
}

public abstract class ModuleRunner<TState> : IModuleRunner
    where TState : class, IModuleStateDescriptor
{
    public abstract string ModuleKey { get; }

    public virtual string StateNamespace => ModuleKey;

    public abstract int ModuleSchemaVersion { get; }

    public abstract SimulationPhase Phase { get; }

    public abstract int ExecutionOrder { get; }

    public virtual FeatureMode DefaultMode => FeatureMode.Full;

    public Type StateType => typeof(TState);

    public virtual IReadOnlyCollection<string> AcceptedCommands => Array.Empty<string>();

    public virtual IReadOnlyCollection<string> PublishedEvents => Array.Empty<string>();

    public virtual IReadOnlyCollection<string> ConsumedEvents => Array.Empty<string>();

    public abstract TState CreateInitialState();

    public virtual void RegisterQueries(TState state, QueryRegistry queries)
    {
    }

    public abstract void RunMonth(ModuleExecutionScope<TState> scope);

    public virtual void HandleEvents(ModuleEventHandlingScope<TState> scope)
    {
    }

    object IModuleRunner.CreateInitialState()
    {
        return CreateInitialState();
    }

    void IModuleRunner.RegisterQueries(object state, QueryRegistry queries)
    {
        RegisterQueries(CastState(state), queries);
    }

    void IModuleRunner.RunMonth(ModuleExecutionContext context, object state)
    {
        RunMonth(new ModuleExecutionScope<TState>(CastState(state), context));
    }

    void IModuleRunner.HandleEvents(ModuleExecutionContext context, object state, IReadOnlyList<IDomainEvent> events)
    {
        HandleEvents(new ModuleEventHandlingScope<TState>(CastState(state), context, events));
    }

    private static TState CastState(object state)
    {
        return state as TState
            ?? throw new InvalidOperationException($"Expected state of type {typeof(TState).Name}, but got {state.GetType().Name}.");
    }
}
