namespace Zongzu.Contracts;

/// <summary>
/// Phase 9 武力骨骼 — <c>LIVING_WORLD_DESIGN §2.9</c>。
/// 武装聚合的七种族类：家丁 / 镖局 / 乡勇 / 衙门 / 官军分遣 / 反叛 / 戍边驻军。
/// </summary>
public enum ForceFamily
{
    Unknown = 0,
    HouseholdRetainer = 1,
    EscortBand = 2,
    Militia = 3,
    YamenForce = 4,
    OfficialDetachment = 5,
    RebelBand = 6,
    GarrisonForce = 7,
}

/// <summary>
/// 冲突事件四级规模：
/// SocialPressure 社会摩擦 / LocalVignette 乡里小斗 / TacticalLite 局部械斗 / CampaignBoard 上战役盘。
/// </summary>
public enum IncidentScale
{
    Unknown = 0,
    SocialPressure = 1,
    LocalVignette = 2,
    TacticalLite = 3,
    CampaignBoard = 4,
}

/// <summary>
/// 冲突结案去向，与既有字符串 <c>LastConflictTrace</c> 并存以供投影层消费。
/// </summary>
public enum IncidentOutcome
{
    Unknown = 0,
    Pending = 1,
    Suppressed = 2,
    Escalated = 3,
    Negotiated = 4,
    Abandoned = 5,
}
