using System;

namespace Zongzu.Contracts;

public sealed class ModuleCommandHandlingScope<TState>
    where TState : class
{
    public ModuleCommandHandlingScope(TState state, ModuleExecutionContext context, PlayerCommandRequest command)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Command = command ?? throw new ArgumentNullException(nameof(command));
    }

    public TState State { get; }

    public ModuleExecutionContext Context { get; }

    public PlayerCommandRequest Command { get; }

    public TQuery GetRequiredQuery<TQuery>()
        where TQuery : class
    {
        return Context.Queries.GetRequired<TQuery>();
    }

    public TQuery? TryGetQuery<TQuery>()
        where TQuery : class
    {
        try
        {
            return Context.Queries.GetRequired<TQuery>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
