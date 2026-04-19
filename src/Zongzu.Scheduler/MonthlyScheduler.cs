using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Scheduler;

public sealed class MonthlyScheduler
{
    private static readonly SimulationXun[] XunOrder =
    [
        SimulationXun.Shangxun,
        SimulationXun.Zhongxun,
        SimulationXun.Xiaxun,
    ];

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
        WorldDiff diff = new();
        DomainEventBuffer domainEvents = new();

        foreach (SimulationXun xun in XunOrder)
        {
            RunCadencePass(
                currentDate,
                featureManifest,
                random,
                moduleStates,
                orderedModules,
                authorityModules,
                kernelState,
                diff,
                domainEvents,
                SimulationCadenceBand.Xun,
                xun);
        }

        RunCadencePass(
            currentDate,
            featureManifest,
            random,
            moduleStates,
            orderedModules,
            authorityModules,
            kernelState,
            diff,
            domainEvents,
            SimulationCadenceBand.Month,
            SimulationXun.None);

        HandleMonthEndEvents(
            currentDate,
            featureManifest,
            random,
            moduleStates,
            orderedModules,
            authorityModules,
            kernelState,
            diff,
            domainEvents);

        RunProjectionPass(
            currentDate,
            featureManifest,
            random,
            moduleStates,
            orderedModules,
            projectionModules,
            kernelState,
            diff,
            domainEvents);

        return new SimulationMonthResult
        {
            SimulatedDate = currentDate,
            NextDate = currentDate.NextMonth(),
            Diff = diff,
            DomainEvents = domainEvents.Events.ToArray(),
        };
    }

    private static void RunCadencePass(
        GameDate currentDate,
        FeatureManifest featureManifest,
        IDeterministicRandom random,
        IReadOnlyDictionary<string, object> moduleStates,
        IReadOnlyList<IModuleRunner> orderedModules,
        IReadOnlyList<IModuleRunner> targetModules,
        KernelState? kernelState,
        WorldDiff diff,
        DomainEventBuffer domainEvents,
        SimulationCadenceBand cadenceBand,
        SimulationXun currentXun)
    {
        QueryRegistry queryRegistry = BuildQueryRegistry(featureManifest, moduleStates, orderedModules);
        ModuleExecutionContext context = new(
            currentDate,
            featureManifest,
            random,
            queryRegistry,
            domainEvents,
            diff,
            kernelState,
            cadenceBand,
            currentXun);

        foreach (IModuleRunner module in targetModules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey) ||
                !module.CadenceBands.Contains(cadenceBand))
            {
                continue;
            }

            if (cadenceBand == SimulationCadenceBand.Xun)
            {
                module.RunXun(context, moduleStates[module.ModuleKey]);
                continue;
            }

            module.RunMonth(context, moduleStates[module.ModuleKey]);
        }
    }

    private static void HandleMonthEndEvents(
        GameDate currentDate,
        FeatureManifest featureManifest,
        IDeterministicRandom random,
        IReadOnlyDictionary<string, object> moduleStates,
        IReadOnlyList<IModuleRunner> orderedModules,
        IReadOnlyList<IModuleRunner> authorityModules,
        KernelState? kernelState,
        WorldDiff diff,
        DomainEventBuffer domainEvents)
    {
        QueryRegistry queryRegistry = BuildQueryRegistry(featureManifest, moduleStates, orderedModules);
        ModuleExecutionContext context = new(
            currentDate,
            featureManifest,
            random,
            queryRegistry,
            domainEvents,
            diff,
            kernelState,
            SimulationCadenceBand.Month,
            SimulationXun.None);
        IDomainEvent[] eventSnapshot = domainEvents.Events.ToArray();

        foreach (IModuleRunner module in authorityModules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey) || module.ConsumedEvents.Count == 0)
            {
                continue;
            }

            module.HandleEvents(context, moduleStates[module.ModuleKey], eventSnapshot);
        }
    }

    private static void RunProjectionPass(
        GameDate currentDate,
        FeatureManifest featureManifest,
        IDeterministicRandom random,
        IReadOnlyDictionary<string, object> moduleStates,
        IReadOnlyList<IModuleRunner> orderedModules,
        IReadOnlyList<IModuleRunner> projectionModules,
        KernelState? kernelState,
        WorldDiff diff,
        DomainEventBuffer domainEvents)
    {
        QueryRegistry queryRegistry = BuildQueryRegistry(featureManifest, moduleStates, orderedModules);
        ModuleExecutionContext context = new(
            currentDate,
            featureManifest,
            random,
            queryRegistry,
            domainEvents,
            diff,
            kernelState,
            SimulationCadenceBand.Month,
            SimulationXun.None);

        foreach (IModuleRunner module in projectionModules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey) ||
                !module.CadenceBands.Contains(SimulationCadenceBand.Month))
            {
                continue;
            }

            module.RunMonth(context, moduleStates[module.ModuleKey]);
        }
    }

    private static QueryRegistry BuildQueryRegistry(
        FeatureManifest featureManifest,
        IReadOnlyDictionary<string, object> moduleStates,
        IReadOnlyList<IModuleRunner> orderedModules)
    {
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

        return queryRegistry;
    }
}
