namespace Zongzu.Contracts;

/// <summary>
/// STEP2A / A0a — 家内照料 + 郎中药铺链（第一条社会化照护链）。
///
/// <para><b>band 而非数字</b>（skill <c>simulation-calibration</c>）：这里只
/// 表达"本聚落有没有郎中、能请到什么档次的郎中"，不拍具体成功率。A1 老死
/// 风险带把 band 翻译成风险权重的"降一档"，但<b>降档不归零</b>——
/// skill <c>medicine-healing-burial</c> 铁律：治疗只改风险权重，不保证成功。</para>
///
/// <para>每个 band 有不同语义，不是同质 0–100 条：</para>
/// <list type="bullet">
///   <item><see cref="None"/> — 花钱也请不到医者，clan.RemedyConfidence 无处可升。</item>
///   <item><see cref="Itinerant"/> — 游方郎中偶至，请医有"时机窗"，不稳。</item>
///   <item><see cref="Local"/> — 坐堂医稳定在本地，常态诊脉抓药。</item>
///   <item><see cref="Renowned"/> — 州府级名医，足以压住重症但成本高。</item>
/// </list>
/// </summary>
public enum HealerAccess
{
    Unknown = 0,
    None = 1,
    Itinerant = 2,
    Local = 3,
    Renowned = 4,
}
