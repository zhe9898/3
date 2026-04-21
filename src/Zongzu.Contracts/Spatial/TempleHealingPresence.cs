namespace Zongzu.Contracts;

/// <summary>
/// STEP2A / A0b — 链 ③ 寺观 / 巫祝 / 民间疗法 band。band 而非数字
/// （skill <c>simulation-calibration</c>：每 band 语义各不相同）。
///
/// <para><b>铁律</b>（skill <c>religion-temples-ritual-brokerage</c>）：
/// 寺观不是"第二家医院"。本字段只入 <b>平行通道</b>——信仰救心不救命：
/// 信寺观的家走这条路会"延误就医"（A1/A5 风险权重反涨），但
/// <c>ShamePressure</c> / <c>FearPressure</c> 下沉（安抚）。病愈欠
/// 还愿债（<see cref="SocialMemoryKinds.VowObligation"/> memory），
/// 未还会转回 <c>ShamePressure</c>。</para>
///
/// <para>band 语义：</para>
/// <list type="bullet">
///   <item><see cref="None"/> — 无寺无庙无巫，走不了这条通道。</item>
///   <item><see cref="Folk"/> — 民间巫祝 / 游方僧道，安抚弱、延误强。</item>
///   <item><see cref="Lay"/> — 本地寺观香火稳定，安抚中、延误中。</item>
///   <item><see cref="Institutional"/> — 州府敕建名刹，安抚强、延误弱（有义诊）。</item>
/// </list>
/// </summary>
public enum TempleHealingPresence
{
    Unknown = 0,
    None = 1,
    Folk = 2,
    Lay = 3,
    Institutional = 4,
}
