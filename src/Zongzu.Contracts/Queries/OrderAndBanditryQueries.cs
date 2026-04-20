using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record SettlementDisorderSnapshot
{
    public SettlementId SettlementId { get; init; }

    public int BanditThreat { get; init; }

    public int RoutePressure { get; init; }

    public int SuppressionDemand { get; init; }

    public int DisorderPressure { get; init; }

    public string LastPressureReason { get; init; } = string.Empty;

    public string LastInterventionCommandCode { get; init; } = string.Empty;

    public string LastInterventionCommandLabel { get; init; } = string.Empty;

    public string LastInterventionSummary { get; init; } = string.Empty;

    public string LastInterventionOutcome { get; init; } = string.Empty;

    public int InterventionCarryoverMonths { get; init; }
}

public interface IOrderAndBanditryQueries
{
    SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId);

    IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder();
}
