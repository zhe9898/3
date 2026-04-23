using System;
using System.Collections.Generic;

namespace Zongzu.Contracts;

public sealed class QueryRegistry
{
    private readonly Dictionary<Type, object> _queries = new();

    public void Register<TQuery>(TQuery query)
        where TQuery : class
    {
        _queries[typeof(TQuery)] = query ?? throw new ArgumentNullException(nameof(query));
    }

    public TQuery GetRequired<TQuery>()
        where TQuery : class
    {
        if (_queries.TryGetValue(typeof(TQuery), out object? query) && query is TQuery typedQuery)
        {
            return typedQuery;
        }

        throw new InvalidOperationException($"Query service {typeof(TQuery).Name} is not registered.");
    }
}
