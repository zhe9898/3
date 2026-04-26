using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record SettlementGovernanceLaneSnapshot
{
    public SettlementId SettlementId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public string NodeLabel { get; init; } = string.Empty;

    public string LeadOfficialName { get; init; } = string.Empty;

    public string LeadOfficeTitle { get; init; } = string.Empty;

    public string CurrentAdministrativeTask { get; init; } = string.Empty;

    public int AdministrativeTaskLoad { get; init; }

    public int PetitionPressure { get; init; }

    public int PetitionBacklog { get; init; }

    public int PublicLegitimacy { get; init; }

    public int StreetTalkHeat { get; init; }

    public int RoutePressure { get; init; }

    public int SuppressionDemand { get; init; }

    public string RecentOrderCommandName { get; init; } = string.Empty;

    public string RecentOrderCommandLabel { get; init; } = string.Empty;

    public bool HasOrderAdministrativeAftermath { get; init; }

    public string SuggestedCommandName { get; init; } = string.Empty;

    public string SuggestedCommandLabel { get; init; } = string.Empty;

    public string SuggestedCommandPrompt { get; init; } = string.Empty;

    public string PublicPressureSummary { get; init; } = string.Empty;

    public string PublicMomentumSummary { get; init; } = string.Empty;

    public string OrderAdministrativeAftermathSummary { get; init; } = string.Empty;

    public string OfficeImplementationReadbackSummary { get; init; } = string.Empty;

    public string OfficeNextStepReadbackSummary { get; init; } = string.Empty;

    public string OfficeLaneEntryReadbackSummary { get; init; } = string.Empty;

    public string OfficeLaneReceiptClosureSummary { get; init; } = string.Empty;

    public string OfficeLaneResidueFollowUpSummary { get; init; } = string.Empty;

    public string OfficeLaneNoLoopGuardSummary { get; init; } = string.Empty;

    public string RegimeOfficeReadbackSummary { get; init; } = string.Empty;

    public string CanalRouteReadbackSummary { get; init; } = string.Empty;

    public string ResidueHealthSummary { get; init; } = string.Empty;

    public string GovernanceSummary { get; init; } = string.Empty;
}

public sealed record GovernanceFocusSnapshot
{
    public SettlementId SettlementId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public string NodeLabel { get; init; } = string.Empty;

    public int UrgencyScore { get; init; }

    public bool HasOrderAdministrativeAftermath { get; init; }

    public string LeadSummary { get; init; } = string.Empty;

    public string PublicPressureSummary { get; init; } = string.Empty;

    public string PublicMomentumSummary { get; init; } = string.Empty;

    public string OfficeImplementationReadbackSummary { get; init; } = string.Empty;

    public string OfficeNextStepReadbackSummary { get; init; } = string.Empty;

    public string OfficeLaneEntryReadbackSummary { get; init; } = string.Empty;

    public string OfficeLaneReceiptClosureSummary { get; init; } = string.Empty;

    public string OfficeLaneResidueFollowUpSummary { get; init; } = string.Empty;

    public string OfficeLaneNoLoopGuardSummary { get; init; } = string.Empty;

    public string RegimeOfficeReadbackSummary { get; init; } = string.Empty;

    public string CanalRouteReadbackSummary { get; init; } = string.Empty;

    public string ResidueHealthSummary { get; init; } = string.Empty;

    public string SuggestedCommandName { get; init; } = string.Empty;

    public string SuggestedCommandLabel { get; init; } = string.Empty;

    public string SuggestedCommandPrompt { get; init; } = string.Empty;
}

public sealed record GovernanceDocketSnapshot
{
    public SettlementId SettlementId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public string NodeLabel { get; init; } = string.Empty;

    public int UrgencyScore { get; init; }

    public bool HasOrderAdministrativeAftermath { get; init; }

    public bool HasRelatedNotification { get; init; }

    public NotificationTier RelatedNotificationTier { get; init; }

    public NarrativeSurface RelatedNotificationSurface { get; init; }

    public string RelatedNotificationTitle { get; init; } = string.Empty;

    public string RelatedNotificationWhyItHappened { get; init; } = string.Empty;

    public string RelatedNotificationWhatNext { get; init; } = string.Empty;

    public string RelatedNotificationSourceModuleKey { get; init; } = string.Empty;

    public string LeadOfficialName { get; init; } = string.Empty;

    public string LeadOfficeTitle { get; init; } = string.Empty;

    public string CurrentAdministrativeTask { get; init; } = string.Empty;

    public bool HasRecentReceipt { get; init; }

    public string RecentReceiptSurfaceKey { get; init; } = string.Empty;

    public string RecentReceiptCommandName { get; init; } = string.Empty;

    public string RecentReceiptLabel { get; init; } = string.Empty;

    public string RecentReceiptSummary { get; init; } = string.Empty;

    public string RecentReceiptOutcomeSummary { get; init; } = string.Empty;

    public string RecentReceiptExecutionSummary { get; init; } = string.Empty;

    public string RecentReceiptLeverageSummary { get; init; } = string.Empty;

    public string RecentReceiptCostSummary { get; init; } = string.Empty;

    public string RecentReceiptReadbackSummary { get; init; } = string.Empty;

    public string Headline { get; init; } = string.Empty;

    public string WhyNowSummary { get; init; } = string.Empty;

    public string PublicMomentumSummary { get; init; } = string.Empty;

    public string OfficeImplementationReadbackSummary { get; init; } = string.Empty;

    public string OfficeNextStepReadbackSummary { get; init; } = string.Empty;

    public string OfficeLaneEntryReadbackSummary { get; init; } = string.Empty;

    public string OfficeLaneReceiptClosureSummary { get; init; } = string.Empty;

    public string OfficeLaneResidueFollowUpSummary { get; init; } = string.Empty;

    public string OfficeLaneNoLoopGuardSummary { get; init; } = string.Empty;

    public string RegimeOfficeReadbackSummary { get; init; } = string.Empty;

    public string CanalRouteReadbackSummary { get; init; } = string.Empty;

    public string ResidueHealthSummary { get; init; } = string.Empty;

    public string PhaseLabel { get; init; } = string.Empty;

    public string PhaseSummary { get; init; } = string.Empty;

    public string HandlingSummary { get; init; } = string.Empty;

    public string GuidanceSummary { get; init; } = string.Empty;

    public string SuggestedCommandName { get; init; } = string.Empty;

    public string SuggestedCommandLabel { get; init; } = string.Empty;

    public string SuggestedCommandPrompt { get; init; } = string.Empty;
}
