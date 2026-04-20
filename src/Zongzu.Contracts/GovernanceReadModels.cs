using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class SettlementGovernanceLaneSnapshot
{
    public SettlementId SettlementId { get; set; }

    public string SettlementName { get; set; } = string.Empty;

    public string NodeLabel { get; set; } = string.Empty;

    public string LeadOfficialName { get; set; } = string.Empty;

    public string LeadOfficeTitle { get; set; } = string.Empty;

    public string CurrentAdministrativeTask { get; set; } = string.Empty;

    public int AdministrativeTaskLoad { get; set; }

    public int PetitionPressure { get; set; }

    public int PetitionBacklog { get; set; }

    public int PublicLegitimacy { get; set; }

    public int StreetTalkHeat { get; set; }

    public int RoutePressure { get; set; }

    public int SuppressionDemand { get; set; }

    public string RecentOrderCommandName { get; set; } = string.Empty;

    public string RecentOrderCommandLabel { get; set; } = string.Empty;

    public bool HasOrderAdministrativeAftermath { get; set; }

    public string SuggestedCommandName { get; set; } = string.Empty;

    public string SuggestedCommandLabel { get; set; } = string.Empty;

    public string SuggestedCommandPrompt { get; set; } = string.Empty;

    public string PublicPressureSummary { get; set; } = string.Empty;

    public string PublicMomentumSummary { get; set; } = string.Empty;

    public string OrderAdministrativeAftermathSummary { get; set; } = string.Empty;

    public string GovernanceSummary { get; set; } = string.Empty;
}

public sealed class GovernanceFocusSnapshot
{
    public SettlementId SettlementId { get; set; }

    public string SettlementName { get; set; } = string.Empty;

    public string NodeLabel { get; set; } = string.Empty;

    public int UrgencyScore { get; set; }

    public bool HasOrderAdministrativeAftermath { get; set; }

    public string LeadSummary { get; set; } = string.Empty;

    public string PublicPressureSummary { get; set; } = string.Empty;

    public string PublicMomentumSummary { get; set; } = string.Empty;

    public string SuggestedCommandName { get; set; } = string.Empty;

    public string SuggestedCommandLabel { get; set; } = string.Empty;

    public string SuggestedCommandPrompt { get; set; } = string.Empty;
}

public sealed class GovernanceDocketSnapshot
{
    public SettlementId SettlementId { get; set; }

    public string SettlementName { get; set; } = string.Empty;

    public string NodeLabel { get; set; } = string.Empty;

    public int UrgencyScore { get; set; }

    public bool HasOrderAdministrativeAftermath { get; set; }

    public bool HasRelatedNotification { get; set; }

    public NotificationTier RelatedNotificationTier { get; set; }

    public NarrativeSurface RelatedNotificationSurface { get; set; }

    public string RelatedNotificationTitle { get; set; } = string.Empty;

    public string RelatedNotificationWhyItHappened { get; set; } = string.Empty;

    public string RelatedNotificationWhatNext { get; set; } = string.Empty;

    public string RelatedNotificationSourceModuleKey { get; set; } = string.Empty;

    public string LeadOfficialName { get; set; } = string.Empty;

    public string LeadOfficeTitle { get; set; } = string.Empty;

    public string CurrentAdministrativeTask { get; set; } = string.Empty;

    public bool HasRecentReceipt { get; set; }

    public string RecentReceiptSurfaceKey { get; set; } = string.Empty;

    public string RecentReceiptCommandName { get; set; } = string.Empty;

    public string RecentReceiptLabel { get; set; } = string.Empty;

    public string RecentReceiptSummary { get; set; } = string.Empty;

    public string RecentReceiptOutcomeSummary { get; set; } = string.Empty;

    public string RecentReceiptExecutionSummary { get; set; } = string.Empty;

    public string Headline { get; set; } = string.Empty;

    public string WhyNowSummary { get; set; } = string.Empty;

    public string PublicMomentumSummary { get; set; } = string.Empty;

    public string PhaseLabel { get; set; } = string.Empty;

    public string PhaseSummary { get; set; } = string.Empty;

    public string HandlingSummary { get; set; } = string.Empty;

    public string GuidanceSummary { get; set; } = string.Empty;

    public string SuggestedCommandName { get; set; } = string.Empty;

    public string SuggestedCommandLabel { get; set; } = string.Empty;

    public string SuggestedCommandPrompt { get; set; } = string.Empty;
}
