using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class SettlementBlackRoutePressureSnapshot
{
    public SettlementId SettlementId { get; set; }

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
}

public sealed class SettlementBlackRouteLedgerSnapshot
{
    public SettlementId SettlementId { get; set; }

    public int ShadowPriceIndex { get; set; }

    public int DiversionShare { get; set; }

    public int IllicitMargin { get; set; }

    public int BlockedShipmentCount { get; set; }

    public int SeizureRisk { get; set; }

    public string DiversionBandLabel { get; set; } = string.Empty;

    public string LastLedgerTrace { get; set; } = string.Empty;
}

public interface IBlackRoutePressureQueries
{
    SettlementBlackRoutePressureSnapshot GetRequiredSettlementBlackRoutePressure(SettlementId settlementId);

    IReadOnlyList<SettlementBlackRoutePressureSnapshot> GetSettlementBlackRoutePressures();
}

public interface IBlackRouteLedgerQueries
{
    SettlementBlackRouteLedgerSnapshot GetRequiredSettlementBlackRouteLedger(SettlementId settlementId);

    IReadOnlyList<SettlementBlackRouteLedgerSnapshot> GetSettlementBlackRouteLedgers();
}
