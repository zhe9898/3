using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Persistence;

public sealed class AppliedRootMigrationStep
{
    public int SourceVersion { get; set; }

    public int TargetVersion { get; set; }
}

public sealed class AppliedModuleMigrationStep
{
    public string ModuleKey { get; set; } = string.Empty;

    public int SourceVersion { get; set; }

    public int TargetVersion { get; set; }
}

public sealed class SaveMigrationReport
{
    public int SourceRootSchemaVersion { get; set; }

    public int PreparedRootSchemaVersion { get; set; }

    public IReadOnlyList<AppliedRootMigrationStep> RootSteps { get; set; } = [];

    public IReadOnlyList<AppliedModuleMigrationStep> ModuleSteps { get; set; } = [];

    public int SourceEnabledModuleCount { get; set; }

    public int PreparedEnabledModuleCount { get; set; }

    public int SourceModuleStateCount { get; set; }

    public int PreparedModuleStateCount { get; set; }

    public bool EnabledModuleKeySetPreserved { get; set; }

    public bool ModuleStateKeySetPreserved { get; set; }

    public IReadOnlyList<string> ConsistencyWarnings { get; set; } = [];

    public bool WasMigrationApplied => RootSteps.Count > 0 || ModuleSteps.Count > 0;

    public int AppliedStepCount => RootSteps.Count + ModuleSteps.Count;

    public bool ConsistencyPassed => EnabledModuleKeySetPreserved && ModuleStateKeySetPreserved && ConsistencyWarnings.Count == 0;
}

public sealed class SavePreparationResult
{
    public SaveRoot SaveRoot { get; set; } = new();

    public SaveMigrationReport Report { get; set; } = new();
}

public sealed class SaveMigrationPipeline
{
    private readonly Dictionary<int, List<RootMigrationRegistration>> _rootMigrations = new();
    private readonly Dictionary<string, Dictionary<int, List<ModuleMigrationRegistration>>> _moduleMigrations = new(StringComparer.Ordinal);

    public SaveMigrationPipeline RegisterRootMigration(int sourceVersion, int targetVersion, Func<SaveRoot, SaveRoot> migration)
    {
        ArgumentNullException.ThrowIfNull(migration);
        ValidateMigrationVersions(sourceVersion, targetVersion, nameof(targetVersion));

        if (!_rootMigrations.TryGetValue(sourceVersion, out List<RootMigrationRegistration>? registrations))
        {
            registrations = [];
            _rootMigrations[sourceVersion] = registrations;
        }

        registrations.Add(new RootMigrationRegistration(sourceVersion, targetVersion, migration));
        registrations.Sort(static (left, right) => left.TargetVersion.CompareTo(right.TargetVersion));
        return this;
    }

    public SaveMigrationPipeline RegisterModuleMigration(
        string moduleKey,
        int sourceVersion,
        int targetVersion,
        Func<ModuleStateEnvelope, ModuleStateEnvelope> migration)
    {
        ArgumentNullException.ThrowIfNull(moduleKey);
        ArgumentNullException.ThrowIfNull(migration);
        ValidateMigrationVersions(sourceVersion, targetVersion, nameof(targetVersion));

        if (!_moduleMigrations.TryGetValue(moduleKey, out Dictionary<int, List<ModuleMigrationRegistration>>? bySource))
        {
            bySource = new Dictionary<int, List<ModuleMigrationRegistration>>();
            _moduleMigrations[moduleKey] = bySource;
        }

        if (!bySource.TryGetValue(sourceVersion, out List<ModuleMigrationRegistration>? registrations))
        {
            registrations = [];
            bySource[sourceVersion] = registrations;
        }

        registrations.Add(new ModuleMigrationRegistration(moduleKey, sourceVersion, targetVersion, migration));
        registrations.Sort(static (left, right) => left.TargetVersion.CompareTo(right.TargetVersion));
        return this;
    }

    public SaveRoot PrepareForLoad(
        SaveRoot saveRoot,
        int targetRootSchemaVersion,
        IReadOnlyList<IModuleRunner> modules)
    {
        return PrepareForLoadWithReport(saveRoot, targetRootSchemaVersion, modules).SaveRoot;
    }

    public SavePreparationResult PrepareForLoadWithReport(
        SaveRoot saveRoot,
        int targetRootSchemaVersion,
        IReadOnlyList<IModuleRunner> modules)
    {
        ArgumentNullException.ThrowIfNull(saveRoot);
        ArgumentNullException.ThrowIfNull(modules);

        List<AppliedRootMigrationStep> appliedRootSteps = [];
        List<AppliedModuleMigrationStep> appliedModuleSteps = [];
        SaveRoot migratedRoot = CloneSaveRoot(saveRoot);
        migratedRoot = ApplyRootMigrations(migratedRoot, targetRootSchemaVersion, appliedRootSteps);

        foreach (IModuleRunner module in modules)
        {
            if (!migratedRoot.FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!migratedRoot.ModuleStates.TryGetValue(module.ModuleKey, out ModuleStateEnvelope? envelope))
            {
                continue;
            }

            migratedRoot.ModuleStates[module.ModuleKey] = ApplyModuleMigrations(
                module.ModuleKey,
                envelope,
                module.ModuleSchemaVersion,
                appliedModuleSteps);
        }

        SaveMigrationReport report = BuildReport(saveRoot, migratedRoot, appliedRootSteps, appliedModuleSteps);

        return new SavePreparationResult
        {
            SaveRoot = migratedRoot,
            Report = report,
        };
    }

    private SaveRoot ApplyRootMigrations(SaveRoot saveRoot, int targetVersion, List<AppliedRootMigrationStep> appliedSteps)
    {
        if (saveRoot.RootSchemaVersion == targetVersion)
        {
            return saveRoot;
        }

        IReadOnlyList<RootMigrationRegistration> path = ResolveRootMigrationPath(saveRoot.RootSchemaVersion, targetVersion);
        SaveRoot migratedRoot = saveRoot;
        foreach (RootMigrationRegistration registration in path)
        {
            migratedRoot = registration.Migration(CloneSaveRoot(migratedRoot))
                ?? throw new SaveMigrationException(
                    $"Root migration {registration.SourceVersion} -> {registration.TargetVersion} returned null.");
            migratedRoot.RootSchemaVersion = registration.TargetVersion;
            appliedSteps.Add(new AppliedRootMigrationStep
            {
                SourceVersion = registration.SourceVersion,
                TargetVersion = registration.TargetVersion,
            });
        }

        return migratedRoot;
    }

    private ModuleStateEnvelope ApplyModuleMigrations(
        string moduleKey,
        ModuleStateEnvelope envelope,
        int targetVersion,
        List<AppliedModuleMigrationStep> appliedSteps)
    {
        if (envelope.ModuleSchemaVersion == targetVersion)
        {
            return envelope;
        }

        IReadOnlyList<ModuleMigrationRegistration> path = ResolveModuleMigrationPath(moduleKey, envelope.ModuleSchemaVersion, targetVersion);
        ModuleStateEnvelope migratedEnvelope = CloneEnvelope(envelope);
        foreach (ModuleMigrationRegistration registration in path)
        {
            migratedEnvelope = registration.Migration(CloneEnvelope(migratedEnvelope))
                ?? throw new SaveMigrationException(
                    $"Module migration {moduleKey} {registration.SourceVersion} -> {registration.TargetVersion} returned null.");
            migratedEnvelope.ModuleKey = moduleKey;
            migratedEnvelope.ModuleSchemaVersion = registration.TargetVersion;
            appliedSteps.Add(new AppliedModuleMigrationStep
            {
                ModuleKey = moduleKey,
                SourceVersion = registration.SourceVersion,
                TargetVersion = registration.TargetVersion,
            });
        }

        return migratedEnvelope;
    }

    private IReadOnlyList<RootMigrationRegistration> ResolveRootMigrationPath(int sourceVersion, int targetVersion)
    {
        Queue<int> frontier = new();
        Dictionary<int, RootMigrationRegistration?> previous = new();
        frontier.Enqueue(sourceVersion);
        previous[sourceVersion] = null;

        while (frontier.Count > 0)
        {
            int currentVersion = frontier.Dequeue();
            if (currentVersion == targetVersion)
            {
                break;
            }

            if (!_rootMigrations.TryGetValue(currentVersion, out List<RootMigrationRegistration>? registrations))
            {
                continue;
            }

            foreach (RootMigrationRegistration registration in registrations)
            {
                if (previous.ContainsKey(registration.TargetVersion))
                {
                    continue;
                }

                previous[registration.TargetVersion] = registration;
                frontier.Enqueue(registration.TargetVersion);
            }
        }

        if (!previous.ContainsKey(targetVersion))
        {
            throw new SaveMigrationException(
                $"No migration path is registered for root schema {sourceVersion} -> {targetVersion}.");
        }

        List<RootMigrationRegistration> path = [];
        int cursor = targetVersion;
        while (cursor != sourceVersion)
        {
            RootMigrationRegistration registration = previous[cursor]
                ?? throw new SaveMigrationException(
                    $"No migration path is registered for root schema {sourceVersion} -> {targetVersion}.");
            path.Add(registration);
            cursor = registration.SourceVersion;
        }

        path.Reverse();
        return path;
    }

    private IReadOnlyList<ModuleMigrationRegistration> ResolveModuleMigrationPath(string moduleKey, int sourceVersion, int targetVersion)
    {
        if (!_moduleMigrations.TryGetValue(moduleKey, out Dictionary<int, List<ModuleMigrationRegistration>>? registrationsBySource))
        {
            throw new SaveMigrationException(
                $"No migration path is registered for module {moduleKey} schema {sourceVersion} -> {targetVersion}.");
        }

        Queue<int> frontier = new();
        Dictionary<int, ModuleMigrationRegistration?> previous = new();
        frontier.Enqueue(sourceVersion);
        previous[sourceVersion] = null;

        while (frontier.Count > 0)
        {
            int currentVersion = frontier.Dequeue();
            if (currentVersion == targetVersion)
            {
                break;
            }

            if (!registrationsBySource.TryGetValue(currentVersion, out List<ModuleMigrationRegistration>? registrations))
            {
                continue;
            }

            foreach (ModuleMigrationRegistration registration in registrations)
            {
                if (previous.ContainsKey(registration.TargetVersion))
                {
                    continue;
                }

                previous[registration.TargetVersion] = registration;
                frontier.Enqueue(registration.TargetVersion);
            }
        }

        if (!previous.ContainsKey(targetVersion))
        {
            throw new SaveMigrationException(
                $"No migration path is registered for module {moduleKey} schema {sourceVersion} -> {targetVersion}.");
        }

        List<ModuleMigrationRegistration> path = [];
        int cursor = targetVersion;
        while (cursor != sourceVersion)
        {
            ModuleMigrationRegistration registration = previous[cursor]
                ?? throw new SaveMigrationException(
                    $"No migration path is registered for module {moduleKey} schema {sourceVersion} -> {targetVersion}.");
            path.Add(registration);
            cursor = registration.SourceVersion;
        }

        path.Reverse();
        return path;
    }

    private static void ValidateMigrationVersions(int sourceVersion, int targetVersion, string parameterName)
    {
        if (sourceVersion == targetVersion)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Migration source and target versions must differ.");
        }
    }

    private static ModuleStateEnvelope CloneEnvelope(ModuleStateEnvelope envelope)
    {
        return new ModuleStateEnvelope
        {
            ModuleKey = envelope.ModuleKey,
            ModuleSchemaVersion = envelope.ModuleSchemaVersion,
            Payload = envelope.Payload.ToArray(),
        };
    }

    private static SaveRoot CloneSaveRoot(SaveRoot saveRoot)
    {
        Dictionary<string, ModuleStateEnvelope> moduleStates = new(StringComparer.Ordinal);
        foreach ((string moduleKey, ModuleStateEnvelope envelope) in saveRoot.ModuleStates)
        {
            moduleStates.Add(moduleKey, new ModuleStateEnvelope
            {
                ModuleKey = envelope.ModuleKey,
                ModuleSchemaVersion = envelope.ModuleSchemaVersion,
                Payload = envelope.Payload.ToArray(),
            });
        }

        return new SaveRoot
        {
            RootSchemaVersion = saveRoot.RootSchemaVersion,
            CurrentDate = saveRoot.CurrentDate,
            FeatureManifest = saveRoot.FeatureManifest.Clone(),
            KernelState = saveRoot.KernelState.Clone(),
            ModuleStates = moduleStates,
        };
    }

    private static SaveMigrationReport BuildReport(
        SaveRoot sourceRoot,
        SaveRoot preparedRoot,
        IReadOnlyList<AppliedRootMigrationStep> appliedRootSteps,
        IReadOnlyList<AppliedModuleMigrationStep> appliedModuleSteps)
    {
        string[] sourceEnabledModuleKeys = sourceRoot.FeatureManifest.GetOrderedEntries()
            .Where(static pair => !string.Equals(pair.Value, "off", StringComparison.Ordinal))
            .Select(static pair => pair.Key)
            .ToArray();
        string[] preparedEnabledModuleKeys = preparedRoot.FeatureManifest.GetOrderedEntries()
            .Where(static pair => !string.Equals(pair.Value, "off", StringComparison.Ordinal))
            .Select(static pair => pair.Key)
            .ToArray();
        string[] sourceModuleStateKeys = sourceRoot.ModuleStates.Keys.OrderBy(static key => key, StringComparer.Ordinal).ToArray();
        string[] preparedModuleStateKeys = preparedRoot.ModuleStates.Keys.OrderBy(static key => key, StringComparer.Ordinal).ToArray();

        bool enabledModuleKeySetPreserved = sourceEnabledModuleKeys.SequenceEqual(preparedEnabledModuleKeys, StringComparer.Ordinal);
        bool moduleStateKeySetPreserved = sourceModuleStateKeys.SequenceEqual(preparedModuleStateKeys, StringComparer.Ordinal);

        List<string> warnings = [];
        if (!enabledModuleKeySetPreserved)
        {
            warnings.Add("Enabled module key set changed during migration preparation.");
        }

        if (!moduleStateKeySetPreserved)
        {
            warnings.Add("Module-state key set changed during migration preparation.");
        }

        return new SaveMigrationReport
        {
            SourceRootSchemaVersion = sourceRoot.RootSchemaVersion,
            PreparedRootSchemaVersion = preparedRoot.RootSchemaVersion,
            RootSteps = appliedRootSteps.ToArray(),
            ModuleSteps = appliedModuleSteps.ToArray(),
            SourceEnabledModuleCount = sourceEnabledModuleKeys.Length,
            PreparedEnabledModuleCount = preparedEnabledModuleKeys.Length,
            SourceModuleStateCount = sourceModuleStateKeys.Length,
            PreparedModuleStateCount = preparedModuleStateKeys.Length,
            EnabledModuleKeySetPreserved = enabledModuleKeySetPreserved,
            ModuleStateKeySetPreserved = moduleStateKeySetPreserved,
            ConsistencyWarnings = warnings.ToArray(),
        };
    }

    private sealed record RootMigrationRegistration(int SourceVersion, int TargetVersion, Func<SaveRoot, SaveRoot> Migration);

    private sealed record ModuleMigrationRegistration(
        string ModuleKey,
        int SourceVersion,
        int TargetVersion,
        Func<ModuleStateEnvelope, ModuleStateEnvelope> Migration);
}
