namespace Zongzu.Contracts;

/// <summary>
/// STEP2A — <see cref="MemoryRecordState.Kind"/> 公共常量。Kind 是字符串，
/// 扩常量不改 schema（skill <c>data-schema-migration</c>：只做 additive 增量）。
///
/// <para>A0b 引入 <see cref="VowObligation"/>：寺观疗愈走完后，clan
/// 背负未还之愿（skill <c>religion-temples-ritual-brokerage</c>：
/// 病愈欠还愿债，未还会转回 <c>ShamePressure</c>）。本 step 只立常量，
/// 规则消费在 A1 / A5 阶段。</para>
/// </summary>
public static class SocialMemoryKinds
{
    /// <summary>寺观 / 巫祝疗愈后背负的还愿债。</summary>
    public const string VowObligation = "vow_obligation";

    /// <summary>宗族救济受惠（A0d 预留）。</summary>
    public const string FavorIncurred = "favor_incurred";

    /// <summary>被宗族拒绝救济（A0d 预留）。</summary>
    public const string ShameExclusion = "shame_exclusion";

    /// <summary>Public-life order residue: 添雇巡丁后留下的本户担保 / 护路人情。</summary>
    public const string PublicOrderWatchObligation = "public_order_watch_obligation";

    /// <summary>Public-life order residue: 催护一路后留下的路报担保债。</summary>
    public const string PublicOrderEscortObligation = "public_order_escort_obligation";

    /// <summary>Public-life order residue: 严缉路匪后留下的报复恐惧 / 强压旧怨。</summary>
    public const string PublicOrderSuppressionFear = "public_order_suppression_fear";

    /// <summary>Public-life order residue: 议路或暂缓穷追后留下的公议羞面。</summary>
    public const string PublicOrderPublicShame = "public_order_public_shame";

    /// <summary>Public-life order residue: 添雇巡丁半落地后的担保债和地方拖延。</summary>
    public const string PublicOrderWatchPartialObligation = "public_order_watch_partial_obligation";

    /// <summary>Public-life order residue: 添雇巡丁被拒后的公开担保失败和羞面。</summary>
    public const string PublicOrderWatchRefusalShame = "public_order_watch_refusal_shame";

    /// <summary>Public-life order residue: 严缉路匪半落地后的反噬与怨尾。</summary>
    public const string PublicOrderSuppressionPartialGrudge = "public_order_suppression_partial_grudge";

    /// <summary>Public-life order residue: 严缉路匪被拒后的恐惧、怨尾和后账。</summary>
    public const string PublicOrderSuppressionRefusalFear = "public_order_suppression_refusal_fear";

    /// <summary>Public-life order response residue: 后账被修复后转成的人情与信任。</summary>
    public const string PublicOrderResponseRepaired = "public_order_response_repaired";

    /// <summary>Public-life order response residue: 后账被暂压后留下的担保与人情欠账。</summary>
    public const string PublicOrderResponseContained = "public_order_response_contained";

    /// <summary>Public-life order response residue: 后账恶化后转深的恐惧、羞面与怨尾。</summary>
    public const string PublicOrderResponseEscalated = "public_order_response_escalated";

    /// <summary>Public-life order response residue: 后账被放置后沉下来的羞面与怨尾。</summary>
    public const string PublicOrderResponseIgnored = "public_order_response_ignored";
}
