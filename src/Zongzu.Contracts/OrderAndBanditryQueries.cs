using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class SettlementDisorderSnapshot
{
    public SettlementId SettlementId { get; set; }

    public int BanditThreat { get; set; }

    public int RoutePressure { get; set; }

    public int SuppressionDemand { get; set; }

    public int DisorderPressure { get; set; }

    public string LastPressureReason { get; set; } = string.Empty;
}

public interface IOrderAndBanditryQueries
{
    SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId);

    IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder();
}
