using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class InteractionPressureMetricsSnapshot
{
    public int ActiveConflictSettlements { get; set; }

    public int ActivatedResponseSettlements { get; set; }

    public int SupportedOrderSettlements { get; set; }

    public int HighSuppressionDemandSettlements { get; set; }

    public int AverageSuppressionDemand { get; set; }

    public int PeakSuppressionDemand { get; set; }

    public int HighBanditThreatSettlements { get; set; }
}

public sealed class SettlementInteractionHotspotSnapshot
{
    public SettlementId SettlementId { get; set; }

    public string SettlementName { get; set; } = string.Empty;

    public int HotspotScore { get; set; }

    public int BanditThreat { get; set; }

    public int RoutePressure { get; set; }

    public int SuppressionDemand { get; set; }

    public int ResponseActivationLevel { get; set; }

    public int OrderSupportLevel { get; set; }

    public bool HasActiveConflict { get; set; }

    public bool IsResponseActivated { get; set; }
}

public sealed class RuntimeScaleMetricsSnapshot
{
    public int EnabledModuleCount { get; set; }

    public int SavedModuleCount { get; set; }

    public int SettlementCount { get; set; }

    public int ClanCount { get; set; }

    public int HouseholdCount { get; set; }

    public int AcademyCount { get; set; }

    public int RouteCount { get; set; }

    public int NotificationCount { get; set; }

    public int NotificationUtilizationPercent { get; set; }

    public int SavePayloadBytesPerSettlement { get; set; }

    public int AverageHouseholdsPerSettlement { get; set; }
}

public sealed class ModulePayloadFootprintSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public int PayloadBytes { get; set; }

    public int PayloadShareBasisPoints { get; set; }
}

public sealed class RuntimePayloadSummarySnapshot
{
    public int TotalModulePayloadBytes { get; set; }

    public string LargestModuleKey { get; set; } = string.Empty;

    public int LargestModulePayloadBytes { get; set; }

    public int LargestModuleShareBasisPoints { get; set; }
}

public sealed class SettlementPressureDistributionSnapshot
{
    public int CalmSettlements { get; set; }

    public int WatchedSettlements { get; set; }

    public int StressedSettlements { get; set; }

    public int CrisisSettlements { get; set; }
}
