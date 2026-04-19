using System.Collections.Generic;

namespace Zongzu.Presentation.Unity;

public sealed class GreatHallDashboardViewModel
{
	public string CurrentDateLabel { get; set; } = string.Empty;

	public string ReplayHash { get; set; } = string.Empty;

	public int UrgentCount { get; set; }

	public int ConsequentialCount { get; set; }

	public int BackgroundCount { get; set; }

	public string FamilySummary { get; set; } = string.Empty;

	public string EducationSummary { get; set; } = string.Empty;

	public string TradeSummary { get; set; } = string.Empty;

	public string PublicLifeSummary { get; set; } = string.Empty;

	public string GovernanceSummary { get; set; } = string.Empty;

	public string WarfareSummary { get; set; } = string.Empty;

	public string AftermathDocketSummary { get; set; } = string.Empty;

	public string LeadNoticeTitle { get; set; } = string.Empty;

	public string LeadNoticeGuidance { get; set; } = string.Empty;

	public IReadOnlyList<GreatHallSecondaryDocketViewModel> SecondaryDockets { get; set; } = [];
}
