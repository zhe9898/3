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

    public string HallAgendaSummary { get; set; } = string.Empty;

    public IReadOnlyList<SettlementHallAgendaItemViewModel> HallAgendaItems { get; set; } = Array.Empty<SettlementHallAgendaItemViewModel>();

    public int HallAgendaCount { get; set; }

    public IReadOnlyList<string> HallAgendaLaneKeys { get; set; } = Array.Empty<string>();

    public bool HasLeadHallAgendaItem { get; set; }

    public string LeadHallAgendaLaneKey { get; set; } = string.Empty;

    public IReadOnlyList<CommandAffordanceViewModel> PublicLifeCommandAffordances { get; set; } = Array.Empty<CommandAffordanceViewModel>();

    public IReadOnlyList<CommandReceiptViewModel> PublicLifeRecentReceipts { get; set; } = Array.Empty<CommandReceiptViewModel>();
}
