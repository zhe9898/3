using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record SettlementBlackRoutePressureSnapshot
{
    public SettlementId SettlementId { get; init; }

    public int BlackRoutePressure { get; init; }

    public int CoercionRisk { get; init; }

    public int SuppressionRelief { get; init; }

    public int ResponseActivationLevel { get; init; }

    public int PaperCompliance { get; init; }

    public int ImplementationDrag { get; init; }

    public int RouteShielding { get; init; }

    public int RetaliationRisk { get; init; }

    public int AdministrativeSuppressionWindow { get; init; }

    public string EscalationBandLabel { get; init; } = string.Empty;

    public string LastPressureTrace { get; init; } = string.Empty;
}

public sealed record SettlementBlackRouteLedgerSnapshot
{
    public SettlementId SettlementId { get; init; }

    public int ShadowPriceIndex { get; init; }

    public int DiversionShare { get; init; }

    public int IllicitMargin { get; init; }

    public int BlockedShipmentCount { get; init; }

    public int SeizureRisk { get; init; }

    public string DiversionBandLabel { get; init; } = string.Empty;

    public string LastLedgerTrace { get; init; } = string.Empty;
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
