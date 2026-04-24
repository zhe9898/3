using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// 记忆条目的大类。拥有模块：<c>SocialMemoryAndRelations</c>。
/// 参见 <c>LIVING_WORLD_DESIGN.md §2.4</c>。
/// </summary>
public enum MemoryType
{
    Unknown = 0,
    Favor = 1,
    Grudge = 2,
    Shame = 3,
    Fear = 4,
    Debt = 5,
    Patronage = 6,
    Aspiration = 7,
    Grief = 8,
    Trust = 9,
}

/// <summary>
/// 记忆小类。用于区分同一大类下的成因（血仇、财怨、荣辱、权势等）。
/// </summary>
public enum MemorySubtype
{
    Unknown = 0,
    BloodGrudge = 1,
    WealthGrudge = 2,
    HonorGrudge = 3,
    PowerGrudge = 4,
    ReliefFavor = 5,
    ProtectionFavor = 6,
    PatronageTie = 7,
    PublicShame = 8,
    WarDread = 9,
    MarketDebt = 10,
    MourningDebt = 11,
    ExamHonor = 12,
    ExamFailure = 13,
    MarriageObligation = 14,
    TradeBreach = 15,
    BranchRift = 16,
    ViolentDeath = 17,
    TemperingResidue = 18,
}

public enum EmotionalPressureAxis
{
    Unknown = 0,
    Fear = 1,
    Shame = 2,
    Grief = 3,
    Anger = 4,
    Obligation = 5,
    Hope = 6,
    Trust = 7,
}

/// <summary>
/// 记忆条目生命状态。<c>Active</c> 正在影响乡议；<c>Dormant</c> 已降格压入
/// 存根只保留钩子；<c>Resolved</c> 事由化解不再漂移。
/// </summary>
public enum MemoryLifecycleState
{
    Unknown = 0,
    Active = 1,
    Dormant = 2,
    Resolved = 3,
}

/// <summary>
/// 记忆的 source / target 可能是人物或宗族。
/// </summary>
public enum MemorySubjectKind
{
    Unknown = 0,
    Person = 1,
    Clan = 2,
}

/// <summary>
/// Phase 4 记忆骨骼读投影。参见 <c>LIVING_WORLD_DESIGN.md §2.4</c>。
/// source / target 通过 <see cref="MemorySubjectKind"/> + 对应 Id 字段表达，
/// 以保持契约不依赖 union 类型。
/// </summary>
public sealed record SocialMemoryEntrySnapshot
{
    public MemoryId Id { get; init; }

    public MemoryType Type { get; init; } = MemoryType.Grudge;

    public MemorySubtype Subtype { get; init; } = MemorySubtype.Unknown;

    public MemorySubjectKind SourceKind { get; init; } = MemorySubjectKind.Clan;

    public PersonId? SourcePersonId { get; init; }

    public ClanId? SourceClanId { get; init; }

    public MemorySubjectKind TargetKind { get; init; } = MemorySubjectKind.Clan;

    public PersonId? TargetPersonId { get; init; }

    public ClanId? TargetClanId { get; init; }

    public GameDate OriginDate { get; init; }

    public string CauseKey { get; init; } = string.Empty;

    public int Weight { get; init; }

    public int MonthlyDecay { get; init; }

    public bool IsPublic { get; init; }

    public MemoryLifecycleState State { get; init; } = MemoryLifecycleState.Active;

    public string Summary { get; init; } = string.Empty;
}

/// <summary>
/// 重度降格人物的记忆存根。参见 <c>LIVING_WORLD_DESIGN.md §2.4</c>。
/// </summary>
public sealed record DormantStubSnapshot
{
    public PersonId PersonId { get; init; }

    public SettlementId? LastKnownSettlementId { get; init; }

    public string LastKnownRole { get; init; } = string.Empty;

    public IReadOnlyList<MemoryId> ActiveMemoryIds { get; init; } = [];

    public GameDate LastSeen { get; init; }

    public bool IsEligibleForReemergence { get; init; }
}

public sealed record ClanEmotionalClimateSnapshot
{
    public ClanId ClanId { get; init; }

    public int Fear { get; init; }

    public int Shame { get; init; }

    public int Grief { get; init; }

    public int Anger { get; init; }

    public int Obligation { get; init; }

    public int Hope { get; init; }

    public int Trust { get; init; }

    public int Restraint { get; init; }

    public int Hardening { get; init; }

    public int Bitterness { get; init; }

    public int Volatility { get; init; }

    public int LastPressureScore { get; init; }

    public int LastPressureBand { get; init; }

    public int LastTemperingBand { get; init; }

    public GameDate LastUpdated { get; init; }

    public string LastTrace { get; init; } = string.Empty;
}

public sealed record PersonPressureTemperingSnapshot
{
    public PersonId PersonId { get; init; }

    public ClanId ClanId { get; init; }

    public int Fear { get; init; }

    public int Shame { get; init; }

    public int Grief { get; init; }

    public int Anger { get; init; }

    public int Obligation { get; init; }

    public int Hope { get; init; }

    public int Trust { get; init; }

    public int Restraint { get; init; }

    public int Hardening { get; init; }

    public int Bitterness { get; init; }

    public int Volatility { get; init; }

    public int LastPressureScore { get; init; }

    public GameDate LastUpdated { get; init; }

    public string LastTrace { get; init; } = string.Empty;
}
