using Zongzu.Kernel;

namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §20.4 — cross-month opinion residue on a settlement
/// node, grounded in <c>public-opinion-reputation-public-spaces.md</c>:
/// "public humiliation and public praise should matter because they change
/// later compliance, revenge, and alliance choices".
///
/// <para><b>Ownership</b>: state lives in <c>SocialMemoryAndRelations</c>
/// (Phase 4), not in <c>WorldSettlements</c>. The spatial skeleton
/// <b>registers only the contract</b>; consumers (e.g. future
/// <c>GetCurrentLocus</c> scoring, Phase 2+) read through this interface and
/// must not reach into the owning module's state.</para>
///
/// <para><b>Phase 1c</b>: contract only. A stub / null-object implementation
/// returning <c>0</c> is acceptable until <c>SocialMemoryAndRelations</c>
/// lands the residue book.</para>
///
/// <para><b>Red line (SPEC §20.6)</b>: covert nodes
/// (<see cref="SettlementNodeKind.CovertMeetPoint"/>,
/// <see cref="SettlementNodeKind.SmugglingCache"/>) host no opinion streams —
/// all residue queries on them must return 0.</para>
/// </summary>
public interface IOpinionResidueQueries
{
    /// <summary>
    /// 0..100 — carried-over heat of one specific <see cref="OpinionStream"/>
    /// on a node, after monthly decay. Independent per stream — a shamed
    /// notice board may coexist with a calm teahouse.
    /// </summary>
    int GetStreamHeat(SettlementId node, OpinionStream stream);

    /// <summary>
    /// 0..100 — composite negative residue across all streams on a node.
    /// Used by downstream scoring (locus selection, compliance modulation)
    /// that wants a single "shame load" handle.
    /// </summary>
    int GetShameResidue(SettlementId node);

    /// <summary>
    /// 0..100 — composite positive residue across all streams on a node.
    /// Mirror of <see cref="GetShameResidue(SettlementId)"/>.
    /// </summary>
    int GetPraiseResidue(SettlementId node);
}
