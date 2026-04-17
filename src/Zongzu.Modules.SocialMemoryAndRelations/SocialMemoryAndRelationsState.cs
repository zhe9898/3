using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed class SocialMemoryAndRelationsState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.SocialMemoryAndRelations;

    public List<ClanNarrativeState> ClanNarratives { get; set; } = new();

    public List<MemoryRecordState> Memories { get; set; } = new();
}

public sealed class ClanNarrativeState
{
    public ClanId ClanId { get; set; }

    public string PublicNarrative { get; set; } = "Quiet watchfulness";

    public int GrudgePressure { get; set; }

    public int FearPressure { get; set; }

    public int ShamePressure { get; set; }

    public int FavorBalance { get; set; }
}

public sealed class MemoryRecordState
{
    public MemoryId Id { get; set; }

    public ClanId SubjectClanId { get; set; }

    public string Kind { get; set; } = string.Empty;

    public int Intensity { get; set; }

    public bool IsPublic { get; set; }

    public GameDate CreatedAt { get; set; }

    public string Summary { get; set; } = string.Empty;
}
