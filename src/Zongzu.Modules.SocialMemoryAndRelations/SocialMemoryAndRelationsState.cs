using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed class SocialMemoryAndRelationsState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.SocialMemoryAndRelations;

    public List<ClanNarrativeState> ClanNarratives { get; set; } = new();

    public List<MemoryRecordState> Memories { get; set; } = new();

    public List<DormantStubState> DormantStubs { get; set; } = new();

    public List<ClanEmotionalClimateState> ClanEmotionalClimates { get; set; } = new();

    public List<PersonPressureTemperingState> PersonTemperings { get; set; } = new();
}

public sealed class ClanNarrativeState
{
    public ClanId ClanId { get; set; }

    public string PublicNarrative { get; set; } = "乡里尚在静看。";

    public int GrudgePressure { get; set; }

    public int FearPressure { get; set; }

    public int ShamePressure { get; set; }

    public int FavorBalance { get; set; }
}

/// <summary>
/// Phase 4 记忆条目（<c>LIVING_WORLD_DESIGN §2.4</c>）。保留旧字段
/// <see cref="Kind"/>/<see cref="Intensity"/>/<see cref="Summary"/> 以维系旧读者；
/// 新字段承载结构化大类/小类、来源/指向、原因键与生命周期。
/// </summary>
public sealed class MemoryRecordState
{
    public MemoryId Id { get; set; }

    public ClanId SubjectClanId { get; set; }

    public string Kind { get; set; } = string.Empty;

    public int Intensity { get; set; }

    public bool IsPublic { get; set; }

    public GameDate CreatedAt { get; set; }

    public string Summary { get; set; } = string.Empty;

    public MemoryType Type { get; set; } = MemoryType.Grudge;

    public MemorySubtype Subtype { get; set; } = MemorySubtype.Unknown;

    public MemorySubjectKind SourceKind { get; set; } = MemorySubjectKind.Clan;

    public PersonId? SourcePersonId { get; set; }

    public ClanId? SourceClanId { get; set; }

    public MemorySubjectKind TargetKind { get; set; } = MemorySubjectKind.Clan;

    public PersonId? TargetPersonId { get; set; }

    public ClanId? TargetClanId { get; set; }

    public GameDate OriginDate { get; set; }

    public string CauseKey { get; set; } = string.Empty;

    public int Weight { get; set; }

    public int MonthlyDecay { get; set; } = 2;

    public MemoryLifecycleState LifecycleState { get; set; } = MemoryLifecycleState.Active;
}

/// <summary>
/// 重度降格人物的记忆存根（<c>LIVING_WORLD_DESIGN §2.4</c>）。
/// </summary>
public sealed class DormantStubState
{
    public PersonId PersonId { get; set; }

    public SettlementId? LastKnownSettlementId { get; set; }

    public string LastKnownRole { get; set; } = string.Empty;

    public List<MemoryId> ActiveMemoryIds { get; set; } = new();

    public GameDate LastSeen { get; set; }

    public bool IsEligibleForReemergence { get; set; }
}

public sealed class ClanEmotionalClimateState
{
    public ClanId ClanId { get; set; }

    public int Fear { get; set; }

    public int Shame { get; set; }

    public int Grief { get; set; }

    public int Anger { get; set; }

    public int Obligation { get; set; }

    public int Hope { get; set; }

    public int Trust { get; set; }

    public int Restraint { get; set; }

    public int Hardening { get; set; }

    public int Bitterness { get; set; }

    public int Volatility { get; set; }

    public int LastPressureScore { get; set; }

    public int LastPressureBand { get; set; }

    public int LastTemperingBand { get; set; }

    public GameDate LastUpdated { get; set; } = new(1, 1);

    public string LastTrace { get; set; } = string.Empty;
}

public sealed class PersonPressureTemperingState
{
    public PersonId PersonId { get; set; }

    public ClanId ClanId { get; set; }

    public int Fear { get; set; }

    public int Shame { get; set; }

    public int Grief { get; set; }

    public int Anger { get; set; }

    public int Obligation { get; set; }

    public int Hope { get; set; }

    public int Trust { get; set; }

    public int Restraint { get; set; }

    public int Hardening { get; set; }

    public int Bitterness { get; set; }

    public int Volatility { get; set; }

    public int LastPressureScore { get; set; }

    public GameDate LastUpdated { get; set; } = new(1, 1);

    public string LastTrace { get; set; } = string.Empty;
}
