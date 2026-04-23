using System;
using System.Collections.Generic;
using System.Linq;

namespace Zongzu.Contracts;

public enum FeatureMode
{
    Off = 0,
    Lite = 1,
    Full = 2,
}

public sealed class FeatureManifest
{
    public Dictionary<string, string> Modules { get; set; } = new();

    public FeatureManifest Clone()
    {
        FeatureManifest clone = new();
        foreach ((string key, string value) in Modules.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            clone.Modules.Add(key, value);
        }

        return clone;
    }

    public void Set(string moduleKey, FeatureMode mode)
    {
        Modules[moduleKey] = ToWireValue(mode);
    }

    public FeatureMode GetMode(string moduleKey)
    {
        if (!Modules.TryGetValue(moduleKey, out string? wireValue))
        {
            return FeatureMode.Off;
        }

        return wireValue switch
        {
            "full" => FeatureMode.Full,
            "lite" => FeatureMode.Lite,
            _ => FeatureMode.Off,
        };
    }

    public bool IsEnabled(string moduleKey)
    {
        return GetMode(moduleKey) != FeatureMode.Off;
    }

    public IReadOnlyList<KeyValuePair<string, string>> GetOrderedEntries()
    {
        return Modules
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .ToArray();
    }

    private static string ToWireValue(FeatureMode mode)
    {
        return mode switch
        {
            FeatureMode.Full => "full",
            FeatureMode.Lite => "lite",
            _ => "off",
        };
    }
}
