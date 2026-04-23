namespace Zongzu.Contracts;

/// <summary>
/// Phase 6 科举骨骼 — <c>LIVING_WORLD_DESIGN §2.6</c>。
/// 三级考阶：县试 / 府试 / 省试（Metropolitan=礼部省试）。
/// </summary>
public enum ExamTier
{
    Unknown = 0,
    CountyExam = 1,
    PrefecturalExam = 2,
    MetropolitanExam = 3,
}

/// <summary>
/// 考试结果的结构化投影，与既有字符串 <c>LastOutcome</c> 并存以便投影层消费。
/// </summary>
public enum ExamResult
{
    Unknown = 0,
    Pending = 1,
    Passed = 2,
    Failed = 3,
    Abandoned = 4,
}

/// <summary>
/// 场屋失利后的退路分流：继续苦读 / 村塾课徒 / 转商贾 / 入吏胥 / 漂泊无依。
/// </summary>
public enum FallbackPath
{
    Unknown = 0,
    ContinueStudy = 1,
    TeachVillage = 2,
    TurnToTrade = 3,
    BecomeClerk = 4,
    Drift = 5,
}
