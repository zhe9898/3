using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Persistence;

public sealed class SaveMigrationPipeline
{
    public SaveRoot PrepareForLoad(
        SaveRoot saveRoot,
        int targetRootSchemaVersion,
        IReadOnlyList<IModuleRunner> modules)
    {
        ArgumentNullException.ThrowIfNull(saveRoot);
        ArgumentNullException.ThrowIfNull(modules);

        SaveRoot migratedRoot = CloneSaveRoot(saveRoot);
        migratedRoot.RootSchemaVersion = MigrateRootSchemaVersion(migratedRoot.RootSchemaVersion, targetRootSchemaVersion);

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

            envelope.ModuleSchemaVersion = MigrateModuleSchemaVersion(
                module.ModuleKey,
                envelope.ModuleSchemaVersion,
                module.ModuleSchemaVersion);
        }

        return migratedRoot;
    }

    private static int MigrateRootSchemaVersion(int sourceVersion, int targetVersion)
    {
        if (sourceVersion == targetVersion)
        {
            return targetVersion;
        }

        throw new SaveMigrationException(
            $"No migration path is registered for root schema {sourceVersion} -> {targetVersion}.");
    }

    private static int MigrateModuleSchemaVersion(string moduleKey, int sourceVersion, int targetVersion)
    {
        if (sourceVersion == targetVersion)
        {
            return targetVersion;
        }

        throw new SaveMigrationException(
            $"No migration path is registered for module {moduleKey} schema {sourceVersion} -> {targetVersion}.");
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
}
