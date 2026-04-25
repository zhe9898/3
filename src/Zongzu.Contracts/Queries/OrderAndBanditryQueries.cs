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

    public int BlackRoutePressure { get; init; }

    public int CoercionRisk { get; init; }

    public int ImplementationDrag { get; init; }

    public int RouteShielding { get; init; }

    public int RetaliationRisk { get; init; }

    public string LastPressureReason { get; init; } = string.Empty;

    public string LastInterventionCommandCode { get; init; } = string.Empty;

    public string LastInterventionCommandLabel { get; init; } = string.Empty;

    public string LastInterventionSummary { get; init; } = string.Empty;

    public string LastInterventionOutcome { get; init; } = string.Empty;

    public string LastInterventionOutcomeCode { get; init; } = string.Empty;

    public string LastInterventionRefusalCode { get; init; } = string.Empty;

    public string LastInterventionPartialCode { get; init; } = string.Empty;

    public string LastInterventionTraceCode { get; init; } = string.Empty;

    public int InterventionCarryoverMonths { get; init; }

    public int RefusalCarryoverMonths { get; init; }

    public string LastRefusalResponseCommandCode { get; init; } = string.Empty;

    public string LastRefusalResponseCommandLabel { get; init; } = string.Empty;

    public string LastRefusalResponseSummary { get; init; } = string.Empty;

    public string LastRefusalResponseOutcomeCode { get; init; } = string.Empty;

    public string LastRefusalResponseTraceCode { get; init; } = string.Empty;

    public int ResponseCarryoverMonths { get; init; }
}

public sealed record OutlawBandSnapshot
{
    public string BandId { get; init; } = string.Empty;

    public string BandName { get; init; } = string.Empty;

    public SettlementId BaseSettlementId { get; init; }

    public int Strength { get; init; }

    public int GrainReserve { get; init; }

    public int Cohesion { get; init; }

    public int Legitimacy { get; init; }

    public BandConcentration Concentration { get; init; } = BandConcentration.Scattered;

    public IReadOnlyList<string> ControlledRoutes { get; init; } = System.Array.Empty<string>();
}

public interface IOrderAndBanditryQueries
{
    SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId);

    IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder();

    IReadOnlyList<OutlawBandSnapshot> GetOutlawBands() => System.Array.Empty<OutlawBandSnapshot>();
}
