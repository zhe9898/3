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
}
