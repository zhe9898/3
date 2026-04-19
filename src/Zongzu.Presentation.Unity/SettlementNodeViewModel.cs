using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity;

public sealed class SettlementNodeViewModel
{
	public string SettlementName { get; set; } = string.Empty;

	public int Security { get; set; }

	public int Prosperity { get; set; }

	public string AcademySummary { get; set; } = string.Empty;

	public string MarketSummary { get; set; } = string.Empty;

	public string PublicLifeSummary { get; set; } = string.Empty;

	public string GovernanceSummary { get; set; } = string.Empty;

    public string CampaignSummary { get; set; } = string.Empty;

    public string AftermathSummary { get; set; } = string.Empty;

    public string PressureSummary { get; set; } = string.Empty;

    public IReadOnlyList<CommandAffordanceViewModel> PublicLifeCommandAffordances { get; set; } = Array.Empty<CommandAffordanceViewModel>();

    public IReadOnlyList<CommandReceiptViewModel> PublicLifeRecentReceipts { get; set; } = Array.Empty<CommandReceiptViewModel>();
}
