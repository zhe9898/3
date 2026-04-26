using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class SettlementNodeViewModel
{
	public string SettlementName { get; set; } = string.Empty;

	public int Security { get; set; }

	public int Prosperity { get; set; }

	public string AcademySummary { get; set; } = string.Empty;

	public string MarketSummary { get; set; } = string.Empty;

	public string PublicLifeSummary { get; set; } = string.Empty;

	public string GovernanceSummary { get; set; } = string.Empty;

	public string OfficeImplementationReadbackSummary { get; set; } = string.Empty;

	public string OfficeNextStepReadbackSummary { get; set; } = string.Empty;

	public string OfficeLaneEntryReadbackSummary { get; set; } = string.Empty;

	public string OfficeLaneReceiptClosureSummary { get; set; } = string.Empty;

	public string OfficeLaneResidueFollowUpSummary { get; set; } = string.Empty;

	public string OfficeLaneNoLoopGuardSummary { get; set; } = string.Empty;

	public string CourtPolicyEntryReadbackSummary { get; set; } = string.Empty;

	public string CourtPolicyDispatchReadbackSummary { get; set; } = string.Empty;

	public string CourtPolicyPublicReadbackSummary { get; set; } = string.Empty;

	public string CourtPolicyNoLoopGuardSummary { get; set; } = string.Empty;

	public string FamilyLaneEntryReadbackSummary { get; set; } = string.Empty;

	public string FamilyElderExplanationReadbackSummary { get; set; } = string.Empty;

	public string FamilyGuaranteeReadbackSummary { get; set; } = string.Empty;

	public string FamilyHouseFaceReadbackSummary { get; set; } = string.Empty;

	public string FamilyLaneReceiptClosureSummary { get; set; } = string.Empty;

	public string FamilyLaneResidueFollowUpSummary { get; set; } = string.Empty;

	public string FamilyLaneNoLoopGuardSummary { get; set; } = string.Empty;

	public string WarfareLaneEntryReadbackSummary { get; set; } = string.Empty;

	public string ForceReadinessReadbackSummary { get; set; } = string.Empty;

	public string CampaignAftermathReadbackSummary { get; set; } = string.Empty;

	public string WarfareLaneReceiptClosureSummary { get; set; } = string.Empty;

	public string WarfareLaneResidueFollowUpSummary { get; set; } = string.Empty;

	public string WarfareLaneNoLoopGuardSummary { get; set; } = string.Empty;

	public string RegimeOfficeReadbackSummary { get; set; } = string.Empty;

	public string CanalRouteReadbackSummary { get; set; } = string.Empty;

	public string ResidueHealthSummary { get; set; } = string.Empty;

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
}
