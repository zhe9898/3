using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Persistence;
using Zongzu.Scheduler;

namespace Zongzu.Application;

public sealed class GameSimulation
{
    public const int RootSchemaVersion = 1;

    private readonly IReadOnlyList<IModuleRunner> _modules;
    private readonly MonthlyScheduler _scheduler;
    private readonly MessagePackModuleStateSerializer _moduleStateSerializer;
    private readonly SaveCodec _saveCodec;
    private readonly SimulationStateStore _stateStore;

    private GameSimulation(
        GameDate currentDate,
        KernelState kernelState,
        FeatureManifest featureManifest,
        IReadOnlyList<IModuleRunner> modules,
        SimulationStateStore stateStore,
        SaveMigrationReport? loadMigrationReport)
    {
        CurrentDate = currentDate;
        KernelState = kernelState ?? throw new ArgumentNullException(nameof(kernelState));
        FeatureManifest = featureManifest ?? throw new ArgumentNullException(nameof(featureManifest));
        _modules = modules ?? throw new ArgumentNullException(nameof(modules));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _scheduler = new MonthlyScheduler();
        _moduleStateSerializer = new MessagePackModuleStateSerializer();
        _saveCodec = new SaveCodec();
        LoadMigrationReport = loadMigrationReport;
    }

    public GameDate CurrentDate { get; private set; }

    public KernelState KernelState { get; }

    public FeatureManifest FeatureManifest { get; }

    public string ReplayHash => KernelState.LastReplayHash;

    public IReadOnlyList<IModuleRunner> Modules => _modules;

    public SimulationMonthResult? LastMonthResult { get; private set; }

    public SaveMigrationReport? LoadMigrationReport { get; }

    public static GameSimulation CreateNew(
        GameDate startDate,
        KernelState kernelState,
        FeatureManifest featureManifest,
        IReadOnlyList<IModuleRunner> modules)
    {
        SimulationStateStore stateStore = new();
        foreach (IModuleRunner module in modules)
        {
            if (!featureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            stateStore.Set(module.ModuleKey, module.CreateInitialState());
        }

        GameSimulation simulation = new(startDate, kernelState, featureManifest.Clone(), modules, stateStore, null);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation Load(
        SaveRoot saveRoot,
        IReadOnlyList<IModuleRunner> modules,
        SaveMigrationPipeline? migrationPipeline = null)
    {
        ArgumentNullException.ThrowIfNull(saveRoot);
        ArgumentNullException.ThrowIfNull(modules);

        SavePreparationResult preparation = (migrationPipeline ?? new SaveMigrationPipeline()).PrepareForLoadWithReport(saveRoot, RootSchemaVersion, modules);
        SaveRoot migratedRoot = preparation.SaveRoot;
        MessagePackModuleStateSerializer serializer = new();
        SimulationStateStore stateStore = new();

        foreach (IModuleRunner module in modules)
        {
            if (!migratedRoot.FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!migratedRoot.ModuleStates.TryGetValue(module.ModuleKey, out ModuleStateEnvelope? envelope))
            {
                throw new InvalidOperationException($"Save root is missing module state for {module.ModuleKey}.");
            }

            if (envelope.ModuleSchemaVersion != module.ModuleSchemaVersion)
            {
                throw new InvalidOperationException($"Module schema mismatch for {module.ModuleKey}.");
            }

            stateStore.Set(module.ModuleKey, serializer.Deserialize(module.StateType, envelope.Payload));
        }

        GameSimulation simulation = new(
            migratedRoot.CurrentDate,
            migratedRoot.KernelState.Clone(),
            migratedRoot.FeatureManifest.Clone(),
            modules,
            stateStore,
            preparation.Report);

        ModuleBoundaryValidator.Validate(simulation._modules, simulation.FeatureManifest, migratedRoot);
        return simulation;
    }

    public SimulationMonthResult AdvanceOneMonth()
    {
        DeterministicRandom random = new(KernelState);
        SimulationMonthResult result = _scheduler.AdvanceOneMonth(CurrentDate, FeatureManifest, random, _stateStore.States, _modules, KernelState);
        CurrentDate = result.NextDate;
        LastMonthResult = result;
        RefreshReplayHash();
        return result;
    }

    public void AdvanceMonths(int monthCount)
    {
        if (monthCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthCount));
        }

        for (int index = 0; index < monthCount; index += 1)
        {
            AdvanceOneMonth();
        }
    }

    public SaveRoot ExportSave()
    {
        SaveRoot saveRoot = BuildSaveRoot(includeReplayHash: true);
        ModuleBoundaryValidator.Validate(_modules, FeatureManifest, saveRoot);
        return saveRoot;
    }

    internal TState GetMutableModuleState<TState>(string moduleKey)
        where TState : class
    {
        return _stateStore.GetRequired<TState>(moduleKey);
    }

    internal bool TryGetModuleState(string moduleKey, out object? state)
    {
        return _stateStore.States.TryGetValue(moduleKey, out state);
    }

    internal void RefreshReplayHash()
    {
        SaveRoot hashSnapshot = BuildSaveRoot(includeReplayHash: false);
        byte[] payload = _saveCodec.Encode(hashSnapshot);
        KernelState.LastReplayHash = ReplayHashing.ComputeHex(payload);
    }

    private SaveRoot BuildSaveRoot(bool includeReplayHash)
    {
        Dictionary<string, ModuleStateEnvelope> moduleStates = new(StringComparer.Ordinal);
        foreach (IModuleRunner module in _modules
                     .OrderBy(static module => module.Phase)
                     .ThenBy(static module => module.ExecutionOrder)
                     .ThenBy(static module => module.ModuleKey, StringComparer.Ordinal))
        {
            if (!FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            object state = _stateStore.GetRequired(module.ModuleKey);
            moduleStates.Add(module.ModuleKey, new ModuleStateEnvelope
            {
                ModuleKey = module.ModuleKey,
                ModuleSchemaVersion = module.ModuleSchemaVersion,
                Payload = _moduleStateSerializer.Serialize(module.StateType, state),
            });
        }

        return new SaveRoot
        {
            RootSchemaVersion = RootSchemaVersion,
            CurrentDate = CurrentDate,
            FeatureManifest = FeatureManifest.Clone(),
            KernelState = includeReplayHash ? KernelState.Clone() : KernelState.CloneWithoutReplayHash(),
            ModuleStates = moduleStates,
        };
    }
}
