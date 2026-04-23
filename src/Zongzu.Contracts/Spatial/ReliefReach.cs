namespace Zongzu.Contracts;

/// <summary>
/// STEP2A / A0c — 链 ④ 官府 / 义仓 / 赈济 band。band 而非数字
/// （skill <c>simulation-calibration</c>：每 band 语义各不相同）。
/// 与 <see cref="SettlementStateData.GranaryTrust"/>（0–100，是否求赈）
/// 配对：<c>GranaryTrust</c> 决定 clan <b>是否上门求赈</b>，
/// <see cref="ReliefReach"/> 决定"即便上门，赈米实到不实到"。
///
/// <para><b>铁律</b>（skill <c>disaster-famine-relief-granaries</c>）：
/// 赈济是政治，不是纯人道——高 <c>GranaryTrust</c> 遇 <see cref="Stalled"/>
/// 档恰恰最伤：去求了，被推搪，记 <c>shame_exclusion</c>。</para>
///
/// <para>band 语义：</para>
/// <list type="bullet">
///   <item><see cref="None"/> — 无仓无赈，疫灾时根本没这条路。</item>
///   <item><see cref="Stalled"/> — 有仓但卡住（吏胥把持 / 印押不通 / 路阻）。</item>
///   <item><see cref="Selective"/> — 通但挑选性（有门路的房支能到，无门路的不到）。</item>
///   <item><see cref="OpenHand"/> — 真的放赈（少见，多在州府级或名臣任上）。</item>
/// </list>
/// </summary>
public enum ReliefReach
{
    Unknown = 0,
    None = 1,
    Stalled = 2,
    Selective = 3,
    OpenHand = 4,
}
