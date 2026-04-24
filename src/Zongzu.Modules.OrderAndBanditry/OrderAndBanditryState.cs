using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.OrderAndBanditry;

    public List<SettlementDisorderState> Settlements { get; set; } = new();

    public List<OutlawBandState> OutlawBands { get; set; } = new();
}

public sealed class OutlawBandState
{
    public string BandId { get; set; } = string.Empty;

    public string BandName { get; set; } = string.Empty;

    public SettlementId BaseSettlementId { get; set; }

    public int Strength { get; set; }

    public int GrainReserve { get; set; }

    public int Cohesion { get; set; }

    public int Legitimacy { get; set; }

    public BandConcentration Concentration { get; set; } = BandConcentration.Scattered;

    public List<string> ControlledRoutes { get; set; } = new();
}

public sealed class SettlementDisorderState
{
    public SettlementId SettlementId { get; set; }

    public int BanditThreat { get; set; }

    public int RoutePressure { get; set; }

    public int SuppressionDemand { get; set; }

    public int DisorderPressure { get; set; }

    public string LastPressureReason { get; set; } = string.Empty;

    public int BlackRoutePressure { get; set; }

    public int CoercionRisk { get; set; }

    public int SuppressionRelief { get; set; }

    public int ResponseActivationLevel { get; set; }

    public int PaperCompliance { get; set; }

    public int ImplementationDrag { get; set; }

    public int RouteShielding { get; set; }

    public int RetaliationRisk { get; set; }

    public int AdministrativeSuppressionWindow { get; set; }

    public string EscalationBandLabel { get; set; } = string.Empty;

    public string LastPressureTrace { get; set; } = string.Empty;

    public string LastInterventionCommandCode { get; set; } = string.Empty;

    public string LastInterventionCommandLabel { get; set; } = string.Empty;

    public string LastInterventionSummary { get; set; } = string.Empty;

    public string LastInterventionOutcome { get; set; } = string.Empty;

    public string LastInterventionOutcomeCode { get; set; } = string.Empty;

    public string LastInterventionRefusalCode { get; set; } = string.Empty;

    public string LastInterventionPartialCode { get; set; } = string.Empty;

    public string LastInterventionTraceCode { get; set; } = string.Empty;

    public int InterventionCarryoverMonths { get; set; }

    public int RefusalCarryoverMonths { get; set; }
}
