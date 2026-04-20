using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record ClanNarrativeSnapshot
{
    public ClanId ClanId { get; init; }

    public string PublicNarrative { get; init; } = string.Empty;

    public int GrudgePressure { get; init; }

    public int FearPressure { get; init; }

    public int ShamePressure { get; init; }

    public int FavorBalance { get; init; }

    public int MemoryCount { get; init; }
}

public interface ISocialMemoryAndRelationsQueries
{
    ClanNarrativeSnapshot GetRequiredClanNarrative(ClanId clanId);

    IReadOnlyList<ClanNarrativeSnapshot> GetClanNarratives();
}
