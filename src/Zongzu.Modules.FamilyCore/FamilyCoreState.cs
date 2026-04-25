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

    // ── STEP2A / A0a — 家内照料 + 郎中药铺链。A1 老死风险带、A4 婚议、
    //    A5 婴幼儿夭折的共同维度入口；本 step 只种字段 + seed，不写规则。
    //    skill medicine-healing-burial：治疗只改风险权重，不保证成功。

    /// <summary>长期照料负担（0–100）。慢病老人、重症婴儿累积。</summary>
    public int CareLoad { get; set; }

    /// <summary>葬事拖累（0–100）。葬事一跳，3–6 月衰。</summary>
    public int FuneralDebt { get; set; }

    /// <summary>求医信心（0–100）。与聚落 HealerAccess band 联动。</summary>
    public int RemedyConfidence { get; set; }

    /// <summary>
    /// STEP2A / A0d — 宗族救济链（<b>挑选性，非普惠</b>，skill
    /// <c>lineage-institutions-corporate-power</c>）。0–100 表本 clan
    /// 对下属房支、贫亲、外戚的"救济义务积压"——债主是 clan 自己。
    ///
    /// <para>不是温情值：救一人 → 降本字段 + 涨 <c>Prestige</c>（公信）；
    /// 弃一人 → 降本字段 + 涨远支 <c>BranchTension</c>、给被弃者记
    /// <see cref="SocialMemoryKinds.ShameExclusion"/>（跨代 grudge）。
    /// 本 step 只种字段 + seed，规则（挑选逻辑）走后续 step。</para>
    /// </summary>
    public int CharityObligation { get; set; }

    public string LastConflictCommandCode { get; set; } = string.Empty;

    public string LastConflictCommandLabel { get; set; } = string.Empty;

    public string LastConflictOutcome { get; set; } = string.Empty;

    public string LastConflictTrace { get; set; } = string.Empty;

    public string LastRefusalResponseCommandCode { get; set; } = string.Empty;

    public string LastRefusalResponseCommandLabel { get; set; } = string.Empty;

    public string LastRefusalResponseSummary { get; set; } = string.Empty;

    public string LastRefusalResponseOutcomeCode { get; set; } = string.Empty;

    public string LastRefusalResponseTraceCode { get; set; } = string.Empty;

    public int ResponseCarryoverMonths { get; set; }

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

    // ── STEP2A / A1 — 老死风险带（累积脆弱度账本）。年龄 ≥55 者按月
    //    加 dose，由年龄带 × clan 照料 × 聚落 HealerAccess × 季节 × 战后
    //    综合决定。ledger ≥ 100 → 候选本月老死。skill 铁律
    //    disease-lifespan-death：不拍概率，不引新 RNG，复合维度累积。
    /// <summary>Fragility ledger (0–100). 仅对 ≥55 岁在世者有意义；
    /// 每月在 FamilyCore RunMonth 累积 dose，到顶则本月身亡。</summary>
    public int FragilityLedger { get; set; }
}
