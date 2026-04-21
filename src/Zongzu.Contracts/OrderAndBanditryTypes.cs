namespace Zongzu.Contracts;

/// <summary>
/// Phase 8 匪患骨骼 — <c>LIVING_WORLD_DESIGN §2.8</c>。
/// 群盗成形的五级聚合度：
/// Scattered 散匪 / Roaming 流寇 / RouteHolding 据路 / TerritoryHolding 据地 / RebelGovernance 僭号。
/// </summary>
public enum BandConcentration
{
    Unknown = 0,
    Scattered = 1,
    Roaming = 2,
    RouteHolding = 3,
    TerritoryHolding = 4,
    RebelGovernance = 5,
}
