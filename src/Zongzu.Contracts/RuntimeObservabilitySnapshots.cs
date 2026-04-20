using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record InteractionPressureMetricsSnapshot
{
    public int ActiveConflictSettlements { get; init; }

    public int ActivatedResponseSettlements { get; init; }

    public int SupportedOrderSettlements { get; init; }

    public int HighSuppressionDemandSettlements { get; init; }

    public int AverageSuppressionDemand { get; init; }

    public int PeakSuppressionDemand { get; init; }

    public int HighBanditThreatSettlements { get; init; }

    public int OrderInterventionCarryoverSettlements { get; init; }

    public int OrderAdministrativeAftermathSettlements { get; init; }

    public int ShieldingDominantSettlements { get; init; }

    public int BacklashDominantSettlements { get; init; }
}

public sealed record SettlementInteractionHotspotSnapshot
{
    public SettlementId SettlementId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public int HotspotScore { get; init; }

    public int BanditThreat { get; init; }

    public int RoutePressure { get; init; }

    public int SuppressionDemand { get; init; }

    public int BlackRoutePressure { get; init; }

    public int RouteShielding { get; init; }

    public int RetaliationRisk { get; init; }

    public int InterventionCarryoverMonths { get; init; }

    public int AdministrativeTaskLoad { get; init; }

    public int PetitionBacklog { get; init; }

    public string AdministrativeAftermathSummary { get; init; } = string.Empty;

    public int ResponseActivationLevel { get; init; }

    public int OrderSupportLevel { get; init; }

    public bool HasActiveConflict { get; init; }

    public bool IsResponseActivated { get; init; }
}

public sealed record RuntimeScaleMetricsSnapshot
{
    public int EnabledModuleCount { get; init; }

    public int SavedModuleCount { get; init; }

    public int SettlementCount { get; init; }

    public int ClanCount { get; init; }

    public int HouseholdCount { get; init; }

    public int AcademyCount { get; init; }

    public int RouteCount { get; init; }

    public int NotificationCount { get; init; }

    public int NotificationUtilizationPercent { get; init; }

    public int SavePayloadBytesPerSettlement { get; init; }

    public int AverageHouseholdsPerSettlement { get; init; }
}

public sealed record ModulePayloadFootprintSnapshot
{
    public string ModuleKey { get; init; } = string.Empty;

    public int PayloadBytes { get; init; }

    public int PayloadShareBasisPoints { get; init; }
}

public sealed record RuntimePayloadSummarySnapshot
{
    public int TotalModulePayloadBytes { get; init; }

    public string LargestModuleKey { get; init; } = string.Empty;

    public int LargestModulePayloadBytes { get; init; }

    public int LargestModuleShareBasisPoints { get; init; }
}

public sealed record SettlementPressureDistributionSnapshot
{
    public int CalmSettlements { get; init; }

    public int WatchedSettlements { get; init; }

    public int StressedSettlements { get; init; }

    public int CrisisSettlements { get; init; }
}
