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

    /// <summary>所有结构化记忆条目，按 Id 升序。Phase 4 新增。</summary>
    IReadOnlyList<SocialMemoryEntrySnapshot> GetMemories() => [];

    /// <summary>按宗族聚焦的结构化记忆条目。Phase 4 新增。</summary>
    IReadOnlyList<SocialMemoryEntrySnapshot> GetMemoriesByClan(ClanId clanId) => [];

    /// <summary>降格人物记忆存根列表。Phase 4 新增。</summary>
    IReadOnlyList<DormantStubSnapshot> GetDormantStubs() => [];
}
