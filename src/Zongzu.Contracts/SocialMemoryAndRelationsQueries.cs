using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class ClanNarrativeSnapshot
{
    public ClanId ClanId { get; set; }

    public string PublicNarrative { get; set; } = string.Empty;

    public int GrudgePressure { get; set; }

    public int FearPressure { get; set; }

    public int ShamePressure { get; set; }

    public int FavorBalance { get; set; }

    public int MemoryCount { get; set; }
}

public interface ISocialMemoryAndRelationsQueries
{
    ClanNarrativeSnapshot GetRequiredClanNarrative(ClanId clanId);

    IReadOnlyList<ClanNarrativeSnapshot> GetClanNarratives();
}
