using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed class FamilyCoreState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.FamilyCore;

    public List<ClanStateData> Clans { get; set; } = new();

    public List<FamilyPersonState> People { get; set; } = new();
}

public sealed class ClanStateData
{
    public ClanId Id { get; set; }

    public string ClanName { get; set; } = string.Empty;

    public SettlementId HomeSettlementId { get; set; }

    public int Prestige { get; set; }

    public int SupportReserve { get; set; }

    public PersonId? HeirPersonId { get; set; }

    public int BranchTension { get; set; }

    public int InheritancePressure { get; set; }

    public int SeparationPressure { get; set; }

    public int MediationMomentum { get; set; }

    public int BranchFavorPressure { get; set; }

    public int ReliefSanctionPressure { get; set; }

    public int MarriageAlliancePressure { get; set; }

    public int MarriageAllianceValue { get; set; }

    public int HeirSecurity { get; set; }

    public int ReproductivePressure { get; set; }

    public int MourningLoad { get; set; }

    public string LastConflictCommandCode { get; set; } = string.Empty;

    public string LastConflictCommandLabel { get; set; } = string.Empty;

    public string LastConflictOutcome { get; set; } = string.Empty;

    public string LastConflictTrace { get; set; } = string.Empty;

    public string LastLifecycleCommandCode { get; set; } = string.Empty;

    public string LastLifecycleCommandLabel { get; set; } = string.Empty;

    public string LastLifecycleOutcome { get; set; } = string.Empty;

    public string LastLifecycleTrace { get; set; } = string.Empty;
}

public sealed class FamilyPersonState
{
    public PersonId Id { get; set; }

    public ClanId ClanId { get; set; }

    public string GivenName { get; set; } = string.Empty;

    public int AgeMonths { get; set; }

    public bool IsAlive { get; set; }

    // ── Phase 2a — clan-scoped kinship (LIVING_WORLD_DESIGN.md §2.2,
    // PERSON_OWNERSHIP_RULES.md). FamilyCore 只持有属于或曾属本族之人的亲属
    // 关系；非族人（流民、外族官、独立匪首）不录此处。

    /// <summary>
    /// 本族房支归位。默认 <see cref="BranchPosition.Unknown"/>；种子与迁移会
    /// 补值。后续 phase 的承祧 / 房长继任逻辑以此为入口。
    /// </summary>
    public BranchPosition BranchPosition { get; set; }

    public PersonId? SpouseId { get; set; }

    public PersonId? FatherId { get; set; }

    public PersonId? MotherId { get; set; }

    /// <summary>族内子女 PersonId 列表。仅限本族可见血亲。</summary>
    public System.Collections.Generic.List<PersonId> ChildrenIds { get; set; } = new();

    // ── Phase 2a — FamilyPersonality（四元组 0..100）。权威归 FamilyCore
    // （PERSON_OWNERSHIP_RULES.md 能力表：性格影响宗族内自主行为）。
    // Phase 2a 仅声明字段与种子默认 50；性格漂移规则留到后续 phase。

    public int Ambition { get; set; }

    public int Prudence { get; set; }

    public int Loyalty { get; set; }

    public int Sociability { get; set; }
}
