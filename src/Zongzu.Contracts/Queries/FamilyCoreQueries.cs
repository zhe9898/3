using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record ClanSnapshot
{
    public ClanId Id { get; init; }

    public string ClanName { get; init; } = string.Empty;

    public SettlementId HomeSettlementId { get; init; }

    public int Prestige { get; init; }

    public int SupportReserve { get; init; }

    public PersonId? HeirPersonId { get; init; }

    public int BranchTension { get; init; }

    public int InheritancePressure { get; init; }

    public int SeparationPressure { get; init; }

    public int MediationMomentum { get; init; }

    public int BranchFavorPressure { get; init; }

    public int ReliefSanctionPressure { get; init; }

    public int MarriageAlliancePressure { get; init; }

    public int MarriageAllianceValue { get; init; }

    public int HeirSecurity { get; init; }

    public int ReproductivePressure { get; init; }

    public int MourningLoad { get; init; }

    // STEP2A / A0a — 家内照料 + 郎中药铺链投影。
    public int CareLoad { get; init; }

    public int FuneralDebt { get; init; }

    public int RemedyConfidence { get; init; }

    // STEP2A / A0d — 宗族救济挑选性投影（skill lineage-institutions-corporate-power）。
    public int CharityObligation { get; init; }

    public int InfantCount { get; init; }

    public string LastConflictCommandCode { get; init; } = string.Empty;

    public string LastConflictCommandLabel { get; init; } = string.Empty;

    public string LastConflictOutcome { get; init; } = string.Empty;

    public string LastConflictTrace { get; init; } = string.Empty;

    public string LastLifecycleCommandCode { get; init; } = string.Empty;

    public string LastLifecycleCommandLabel { get; init; } = string.Empty;

    public string LastLifecycleOutcome { get; init; } = string.Empty;

    public string LastLifecycleTrace { get; init; } = string.Empty;
}

public interface IFamilyCoreQueries
{
    ClanSnapshot GetRequiredClan(ClanId clanId);

    IReadOnlyList<ClanSnapshot> GetClans();

    /// <summary>
    /// PERSON_OWNERSHIP_RULES.md — FamilyCore 视角下某人的结构性归位与性格
    /// 四元组。其他模块需要学业 / 商贸 / 武力等能力时，走各自模块的 Query，
    /// 不得从此处取。
    /// </summary>
    FamilyPersonSnapshot? FindPerson(PersonId personId);

    IReadOnlyList<FamilyPersonSnapshot> GetClanMembers(ClanId clanId);
}

/// <summary>
/// LIVING_WORLD_DESIGN.md §2.2 + PERSON_OWNERSHIP_RULES.md — clan-scoped
/// kinship view. Age / IsAlive 仍由 FamilyCore 暂存（Phase 2b 会迁出到
/// PersonRegistry），故此快照同时投这两类字段。
/// </summary>
public sealed record FamilyPersonSnapshot
{
    public PersonId Id { get; init; }

    public ClanId ClanId { get; init; }

    public string GivenName { get; init; } = string.Empty;

    public int AgeMonths { get; init; }

    public bool IsAlive { get; init; }

    public BranchPosition BranchPosition { get; init; }

    public PersonId? SpouseId { get; init; }

    public PersonId? FatherId { get; init; }

    public PersonId? MotherId { get; init; }

    public IReadOnlyList<PersonId> ChildrenIds { get; init; } = System.Array.Empty<PersonId>();

    public int Ambition { get; init; }

    public int Prudence { get; init; }

    public int Loyalty { get; init; }

    public int Sociability { get; init; }
}
