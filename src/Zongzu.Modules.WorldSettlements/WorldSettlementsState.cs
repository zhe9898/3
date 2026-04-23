using System.Collections.Generic;
using System.Runtime.Serialization;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// Authoritative state for the <c>WorldSettlements</c> module — the spatial
/// skeleton root: settlement nodes, social-function routes, and the parallel
/// season bands.
///
/// <para>SPATIAL_SKELETON_SPEC Phase 1c (schema v3) adds <see cref="Routes"/>
/// and <see cref="CurrentSeason"/>. See SPEC §1 / §2 / §3.</para>
/// </summary>
public sealed class WorldSettlementsState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.WorldSettlements;

    public List<SettlementStateData> Settlements { get; set; } = new();

    /// <summary>Routes between settlements (SPEC §2). Phase 1c schema v3.</summary>
    public List<RouteStateData> Routes { get; set; } = new();

    /// <summary>Current season bands — natural / government / imperial (SPEC §3). Phase 1c schema v3.</summary>
    public SeasonBandData CurrentSeason { get; set; } = new();

    /// <summary>
    /// Chain 6 declaration watermark. 0 = no active flood disaster, 1 =
    /// moderate declared, 2 = severe declared. Prevents a persistent high
    /// flood band from re-declaring the same disaster every month.
    /// </summary>
    public int LastDeclaredFloodDisasterBand { get; set; }

    /// <summary>
    /// Chain 5 declaration watermark. 0 = no active frontier strain, 1 =
    /// moderate declared, 2 = severe declared. Prevents a persistent high
    /// frontier band from re-triggering supply requisitions every month.
    /// </summary>
    public int LastDeclaredFrontierStrainBand { get; set; }

    /// <summary>
    /// Chain 8 declaration watermark. true = CourtAgendaPressureAccumulated
    /// has been declared while MandateConfidence &lt; 40. Prevents persistent
    /// low mandate confidence from re-emitting every month.
    /// </summary>
    public bool LastCourtAgendaPressureDeclared { get; set; }

    /// <summary>
    /// Chain 9 declaration watermark. true = RegimeLegitimacyShifted has been
    /// declared while MandateConfidence &lt; 25. Prevents persistent very low
    /// mandate confidence from re-emitting every month.
    /// </summary>
    public bool LastRegimeLegitimacyShiftDeclared { get; set; }

    /// <summary>
    /// SPATIAL_SKELETON_SPEC §20.3 — public-surface signals emitted during the
    /// current tick (xun or month). Derived from state transitions by
    /// <see cref="PublicSurfaceSignalEmitter"/>; consumers read via
    /// <see cref="IWorldSettlementsQueries.GetCurrentPulseSignals"/>.
    ///
    /// <para><b>Lifetime = one tick.</b> Cleared at the start of every xun
    /// and month pulse. Not persisted — <see cref="IgnoreDataMemberAttribute"/>
    /// keeps signals out of MessagePack output (regenerated on next tick).</para>
    /// </summary>
    [IgnoreDataMember]
    public List<PublicSurfaceSignal> CurrentPulseSignals { get; set; } = new();
}

/// <summary>
/// Authoritative per-settlement state. SPEC §1 Phase 1c (schema v3) adds
/// <see cref="NodeKind"/> (functional semantics, decision A),
/// <see cref="Visibility"/> (decision H), <see cref="EcoZone"/> (decision I),
/// and graph fields <see cref="NeighborIds"/> / <see cref="ParentAdministrativeId"/>.
/// </summary>
public sealed class SettlementStateData
{
    public SettlementId Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Administrative level. Orthogonal to <see cref="NodeKind"/>.</summary>
    public SettlementTier Tier { get; set; }

    /// <summary>Functional-semantic classification (decision A). Phase 1c schema v3.</summary>
    public SettlementNodeKind NodeKind { get; set; }

    /// <summary>State-visible / local-known / covert (decision H). Phase 1c schema v3.</summary>
    public NodeVisibility Visibility { get; set; }

    /// <summary>Regional ecology (decision I). Phase 1c schema v3.</summary>
    public SettlementEcoZone EcoZone { get; set; }

    public int Security { get; set; }

    public int Prosperity { get; set; }

    public int BaselineInstitutionCount { get; set; }

    /// <summary>
    /// STEP2A / A0a — 家内照料 + 郎中药铺链 band。决定本聚落能否求医，
    /// 以及 A1 老死风险带的降档权重入口。band 而非数字（skill
    /// simulation-calibration）；治疗不保证成功（skill medicine-healing-burial）。
    /// </summary>
    public HealerAccess HealerAccess { get; set; }

    /// <summary>
    /// STEP2A / A0b — 寺观 / 巫祝 / 民间疗法 band。平行通道：信仰救心
    /// 不救命（skill religion-temples-ritual-brokerage）；band 而非数字
    /// （skill simulation-calibration）。A1 老死与 A5 婴幼儿夭折会把
    /// Folk/Lay 档的"延误"反映成风险权重上抬。
    /// </summary>
    public TempleHealingPresence TempleHealingPresence { get; set; }

    /// <summary>
    /// STEP2A / A0c — 官府 / 义仓 / 赈济链。0–100 数值决定 clan <b>是否
    /// 上门求赈</b>（高 = 相信官府、敢去），不是赈米实到率。实到率
    /// 走 <see cref="ReliefReach"/> band（skill
    /// disaster-famine-relief-granaries：赈济是政治）。平时 dormant，
    /// 疫灾 / 饥荒时激活。
    /// </summary>
    public int GranaryTrust { get; set; }

    /// <summary>
    /// STEP2A / A0c — 赈济实到 band（None/Stalled/Selective/OpenHand）。
    /// 与 <see cref="GranaryTrust"/> 配对：信任决定去不去，band 决定到不到。
    /// </summary>
    public ReliefReach ReliefReach { get; set; }

    /// <summary>Adjacency graph — purely geometric neighbors; SPEC §1.1/12.4. Phase 1c schema v3.</summary>
    public List<SettlementId> NeighborIds { get; set; } = new();

    /// <summary>Administrative parent (county seat for a village, prefecture for a county). <c>null</c> at the top. Phase 1c schema v3.</summary>
    public SettlementId? ParentAdministrativeId { get; set; }
}

