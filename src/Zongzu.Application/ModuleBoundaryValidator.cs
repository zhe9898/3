using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Application;

public static class ModuleBoundaryValidator
{
    public static void Validate(IReadOnlyList<IModuleRunner> modules, FeatureManifest featureManifest, SaveRoot saveRoot)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(featureManifest);
        ArgumentNullException.ThrowIfNull(saveRoot);

        Dictionary<string, IModuleRunner> modulesByKey = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module,
            StringComparer.Ordinal);

        if (modulesByKey.Count != modules.Count)
        {
            throw new InvalidOperationException("Module keys must be unique.");
        }

        foreach (IModuleRunner module in modules)
        {
            if (!string.Equals(module.StateNamespace, module.ModuleKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Module {module.ModuleKey} must save only into its own namespace.");
            }
        }

        foreach ((string moduleKey, string _) in featureManifest.GetOrderedEntries())
        {
            if (!featureManifest.IsEnabled(moduleKey))
            {
                continue;
            }

            if (!modulesByKey.ContainsKey(moduleKey))
            {
                throw new InvalidOperationException($"Enabled module {moduleKey} is not registered.");
            }
        }

        foreach ((string moduleKey, ModuleStateEnvelope envelope) in saveRoot.ModuleStates.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            if (!modulesByKey.TryGetValue(moduleKey, out IModuleRunner? module))
            {
                throw new InvalidOperationException($"Save contains unregistered module namespace {moduleKey}.");
            }

            if (!featureManifest.IsEnabled(moduleKey))
            {
                throw new InvalidOperationException($"Save contains disabled module namespace {moduleKey}.");
            }

            if (!string.Equals(envelope.ModuleKey, moduleKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Envelope key mismatch for module {moduleKey}.");
            }

            if (envelope.ModuleSchemaVersion != module.ModuleSchemaVersion)
            {
                throw new InvalidOperationException($"Schema version mismatch for module {moduleKey}.");
            }
        }

        foreach (IModuleRunner module in modules)
        {
            if (featureManifest.IsEnabled(module.ModuleKey) && !saveRoot.ModuleStates.ContainsKey(module.ModuleKey))
            {
                throw new InvalidOperationException($"Enabled module {module.ModuleKey} is missing from the save root.");
            }
        }
    }
}
