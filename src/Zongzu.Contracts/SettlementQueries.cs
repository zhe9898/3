using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public enum SettlementTier
{
    Unknown = 0,
    VillageCluster = 1,
    MarketTown = 2,
    CountySeat = 3,
    PrefectureSeat = 4,
}

public sealed class SettlementSnapshot
{
    public SettlementId Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public SettlementTier Tier { get; set; }

    public int Security { get; set; }

    public int Prosperity { get; set; }
}

public interface IWorldSettlementsQueries
{
    SettlementSnapshot GetRequiredSettlement(SettlementId settlementId);

    IReadOnlyList<SettlementSnapshot> GetSettlements();
}
