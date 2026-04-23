namespace Zongzu.Contracts;

/// <summary>
/// Phase 10 战役骨骼 — <c>LIVING_WORLD_DESIGN §2.10</c>。
/// 战役的八步相位：
/// Proposed 议战 / Mobilizing 动员 / Marshalled 集结 / Engaged 接战 /
/// Stalemate 相持 / Decisive 决阵 / Withdrawing 撤收 / Aftermath 善后。
/// </summary>
public enum CampaignPhase
{
    Unknown = 0,
    Proposed = 1,
    Mobilizing = 2,
    Marshalled = 3,
    Engaged = 4,
    Stalemate = 5,
    Decisive = 6,
    Withdrawing = 7,
    Aftermath = 8,
}
