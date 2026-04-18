using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Scheduler;

public sealed class MonthlyScheduler
{
    public IReadOnlyList<IModuleRunner> OrderModules(IEnumerable<IModuleRunner> modules)
    {
        return modules
            .OrderBy(static module => module.Phase)
            .ThenBy(static module => module.ExecutionOrder)
            .ThenBy(static module => module.ModuleKey, StringComparer.Ordinal)
            .ToArray();
    }

    public SimulationMonthResult AdvanceOneMonth(
        GameDate currentDate,
        FeatureManifest featureManifest,
        IDeterministicRandom random,
        IReadOnlyDictionary<string, object> moduleStates,
        IReadOnlyList<IModuleRunner> modules,
        KernelState? kernelState = null)
    {
        IReadOnlyList<IModuleRunner> orderedModules = OrderModules(modules);
        IModuleRunner[] authorityModules = orderedModules
            .Where(static module => module.Phase != SimulationPhase.Projection)
            .ToArray();
        IModuleRunner[] projectionModules = orderedModules
            .Where(static module => module.Phase == SimulationPhase.Projection)
            .ToArray();
        QueryRegistry queryRegistry = new();

        foreach (IModuleRunner module in orderedModules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!moduleStates.TryGetValue(module.ModuleKey, out object? state))
            {
                throw new InvalidOperationException($"Missing state for enabled module {module.ModuleKey}.");
            }

            module.RegisterQueries(state, queryRegistry);
        }

        WorldDiff diff = new();
        DomainEventBuffer domainEvents = new();
        ModuleExecutionContext context = new(currentDate, featureManifest, random, queryRegistry, domainEvents, diff, kernelState);

        foreach (IModuleRunner module in authorityModules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            module.RunMonth(context, moduleStates[module.ModuleKey]);
        }

        IDomainEvent[] eventSnapshot = domainEvents.Events.ToArray();
        foreach (IModuleRunner module in authorityModules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey) || module.ConsumedEvents.Count == 0)
            {
                continue;
            }

            module.HandleEvents(context, moduleStates[module.ModuleKey], eventSnapshot);
        }

        foreach (IModuleRunner module in projectionModules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            module.RunMonth(context, moduleStates[module.ModuleKey]);
        }

        return new SimulationMonthResult
        {
            SimulatedDate = currentDate,
            NextDate = currentDate.NextMonth(),
            Diff = diff,
            DomainEvents = domainEvents.Events.ToArray(),
        };
    }
}
