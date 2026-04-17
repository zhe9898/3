using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class SettlementSnapshot
{
    public SettlementId Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Security { get; set; }

    public int Prosperity { get; set; }
}

public interface IWorldSettlementsQueries
{
    SettlementSnapshot GetRequiredSettlement(SettlementId settlementId);

    IReadOnlyList<SettlementSnapshot> GetSettlements();
}
