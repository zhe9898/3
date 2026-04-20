using System.Collections.Generic;

namespace Zongzu.Contracts;

public sealed class WorldDiffEntry
{
    public string ModuleKey { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? EntityKey { get; set; }
}

public sealed class WorldDiff
{
    public List<WorldDiffEntry> Entries { get; set; } = new();

    public void Record(string moduleKey, string description, string? entityKey = null)
    {
        Entries.Add(new WorldDiffEntry
        {
            ModuleKey = moduleKey,
            Description = description,
            EntityKey = entityKey,
        });
    }
}
