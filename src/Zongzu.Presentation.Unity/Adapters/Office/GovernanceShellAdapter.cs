using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal static class GovernanceShellAdapter
{
	internal static string BuildGreatHallGovernanceSummary(
		PresentationReadModelBundle bundle,
		string officeFallbackSummary)
	{
		SettlementGovernanceLaneSnapshot? governanceLane = SelectLeadGovernanceLane(bundle);
		GovernanceDocketSnapshot? governanceDocket = HasGovernanceDocket(bundle.GovernanceDocket)
			? bundle.GovernanceDocket
			: null;
		string summary = !string.IsNullOrWhiteSpace(governanceLane?.GovernanceSummary)
			? governanceLane.GovernanceSummary
			: officeFallbackSummary;
		string momentum = !string.IsNullOrWhiteSpace(governanceDocket?.PublicMomentumSummary)
			? governanceDocket.PublicMomentumSummary
			: governanceLane?.PublicMomentumSummary ?? string.Empty;

		return ShellTextAdapter.CombineDistinct(
			summary,
			momentum,
			governanceLane?.CourtPolicyEntryReadbackSummary ?? string.Empty,
			governanceLane?.CourtPolicyDispatchReadbackSummary ?? string.Empty,
			governanceLane?.CourtPolicyPublicReadbackSummary ?? string.Empty,
			governanceLane?.CourtPolicyNoLoopGuardSummary ?? string.Empty,
			governanceLane?.OfficeImplementationReadbackSummary ?? string.Empty,
			governanceLane?.OfficeLaneReceiptClosureSummary ?? string.Empty,
			governanceLane?.OfficeLaneNoLoopGuardSummary ?? string.Empty,
			governanceLane?.FamilyLaneReceiptClosureSummary ?? string.Empty,
			governanceLane?.FamilyLaneNoLoopGuardSummary ?? string.Empty,
			governanceLane?.CampaignAftermathReadbackSummary ?? string.Empty,
			governanceLane?.WarfareLaneNoLoopGuardSummary ?? string.Empty,
			governanceLane?.RegimeOfficeReadbackSummary ?? string.Empty).Trim();
	}

	internal static string BuildSettlementGovernanceSummary(
		string officeFallbackSummary,
		SettlementGovernanceLaneSnapshot? governanceLane)
	{
		if (governanceLane == null)
		{
			return officeFallbackSummary;
		}

		string summary = !string.IsNullOrWhiteSpace(governanceLane.GovernanceSummary)
			? governanceLane.GovernanceSummary
			: officeFallbackSummary;

		return ShellTextAdapter.CombineDistinct(
			summary,
			governanceLane.PublicMomentumSummary,
			governanceLane.OfficeImplementationReadbackSummary,
			governanceLane.OfficeNextStepReadbackSummary,
			governanceLane.OfficeLaneEntryReadbackSummary,
			governanceLane.OfficeLaneReceiptClosureSummary,
			governanceLane.OfficeLaneResidueFollowUpSummary,
			governanceLane.OfficeLaneNoLoopGuardSummary,
			governanceLane.CourtPolicyEntryReadbackSummary,
			governanceLane.CourtPolicyDispatchReadbackSummary,
			governanceLane.CourtPolicyPublicReadbackSummary,
			governanceLane.CourtPolicyNoLoopGuardSummary,
			governanceLane.FamilyLaneEntryReadbackSummary,
			governanceLane.FamilyElderExplanationReadbackSummary,
			governanceLane.FamilyGuaranteeReadbackSummary,
			governanceLane.FamilyHouseFaceReadbackSummary,
			governanceLane.FamilyLaneReceiptClosureSummary,
			governanceLane.FamilyLaneResidueFollowUpSummary,
			governanceLane.FamilyLaneNoLoopGuardSummary,
			governanceLane.WarfareLaneEntryReadbackSummary,
			governanceLane.ForceReadinessReadbackSummary,
			governanceLane.CampaignAftermathReadbackSummary,
			governanceLane.WarfareLaneReceiptClosureSummary,
			governanceLane.WarfareLaneResidueFollowUpSummary,
			governanceLane.WarfareLaneNoLoopGuardSummary,
			governanceLane.RegimeOfficeReadbackSummary,
			governanceLane.CanalRouteReadbackSummary,
			governanceLane.ResidueHealthSummary).Trim();
	}

	private static SettlementGovernanceLaneSnapshot? SelectLeadGovernanceLane(PresentationReadModelBundle bundle)
	{
		if (bundle.GovernanceFocus.SettlementId.Value > 0)
		{
			SettlementGovernanceLaneSnapshot? focusSettlement = bundle.GovernanceSettlements
				.FirstOrDefault(settlement => settlement.SettlementId == bundle.GovernanceFocus.SettlementId);
			if (focusSettlement != null)
			{
				return focusSettlement;
			}
		}

		if (HasGovernanceDocket(bundle.GovernanceDocket) && bundle.GovernanceDocket.SettlementId.Value > 0)
		{
			SettlementGovernanceLaneSnapshot? docketSettlement = bundle.GovernanceSettlements
				.FirstOrDefault(settlement => settlement.SettlementId == bundle.GovernanceDocket.SettlementId);
			if (docketSettlement != null)
			{
				return docketSettlement;
			}
		}

		return bundle.GovernanceSettlements
			.OrderByDescending(settlement =>
				settlement.AdministrativeTaskLoad
				+ settlement.PetitionPressure
				+ settlement.PetitionBacklog
				+ settlement.StreetTalkHeat
				+ settlement.RoutePressure
				+ settlement.SuppressionDemand)
			.ThenBy(settlement => settlement.SettlementName, StringComparer.Ordinal)
			.FirstOrDefault();
	}

	private static bool HasGovernanceDocket(GovernanceDocketSnapshot docket)
	{
		return docket.SettlementId.Value > 0
			|| !string.IsNullOrWhiteSpace(docket.Headline)
			|| !string.IsNullOrWhiteSpace(docket.WhyNowSummary)
			|| !string.IsNullOrWhiteSpace(docket.PublicMomentumSummary);
	}
}
