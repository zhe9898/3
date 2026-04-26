using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class OfficeShellAdapter
{
	internal static string BuildGreatHallGovernanceFallbackSummary(
		int appointedCareerCount,
		JurisdictionAuthoritySnapshot? jurisdictionAuthority)
	{
		if (jurisdictionAuthority == null)
		{
			return "案头暂无官署呈报。";
		}

		return $"{appointedCareerCount}人在官途；{jurisdictionAuthority.LeadOfficialName}任{OfficeShellTextAdapter.RenderOfficeTitle(jurisdictionAuthority.LeadOfficeTitle)}主事，{OfficeShellTextAdapter.RenderPetitionOutcomeCategory(jurisdictionAuthority.PetitionOutcomeCategory)}，积案{jurisdictionAuthority.PetitionBacklog}。";
	}

	internal static string BuildSettlementGovernanceFallbackSummary(JurisdictionAuthoritySnapshot? jurisdictionAuthority)
	{
		if (jurisdictionAuthority == null)
		{
			return "官署未设。";
		}

		return $"{OfficeShellTextAdapter.RenderOfficeTitle(jurisdictionAuthority.LeadOfficeTitle)}{jurisdictionAuthority.LeadOfficialName}：乡面杠力{jurisdictionAuthority.JurisdictionLeverage}，{OfficeShellTextAdapter.RenderAdministrativeTaskTier(jurisdictionAuthority.AdministrativeTaskTier)}差遣 {OfficeShellTextAdapter.RenderAdministrativeTask(jurisdictionAuthority.CurrentAdministrativeTask)}，{OfficeShellTextAdapter.RenderPetitionOutcomeCategory(jurisdictionAuthority.PetitionOutcomeCategory)}，积案{jurisdictionAuthority.PetitionBacklog}。";
	}

	internal static OfficeSurfaceViewModel BuildOfficeSurface(PresentationReadModelBundle bundle)
	{
		if (bundle.OfficeCareers.Count == 0 && bundle.OfficeJurisdictions.Count == 0)
		{
			return new OfficeSurfaceViewModel
			{
				Summary = "案头暂无官署牍报。",
				CommandAffordances = Array.Empty<CommandAffordanceViewModel>(),
				RecentReceipts = Array.Empty<CommandReceiptViewModel>()
			};
		}

		OfficeProjectionContext context = OfficeProjectionContext.Create(bundle);
		int appointedCareers = context.AppointedCareerCount;
		int jurisdictionCount = context.JurisdictionCount;
		int highestBacklog = context.HighestBacklog;
		IReadOnlyDictionary<int, string> settlementNames = context.SettlementNames;

		return new OfficeSurfaceViewModel
		{
			Summary = $"现有官人{appointedCareers}名，分掌{jurisdictionCount}处，积案最高{highestBacklog}。",
			CommandAffordances = CommandShellAdapter.BuildAffordances(context.OrderedAffordances),
			RecentReceipts = CommandShellAdapter.BuildReceipts(context.OrderedReceipts),
			Appointments = context.OrderedCareers
				.Select(career => new OfficeAppointmentViewModel
				{
					DisplayName = career.DisplayName,
					OfficeTitle = OfficeShellTextAdapter.RenderOfficeTitle(career.OfficeTitle),
					HasAppointment = career.HasAppointment,
					AuthorityTier = career.AuthorityTier,
					ServiceSummary = career.HasAppointment
						? $"供职{career.ServiceMonths}月；升势{OfficeShellTextAdapter.RenderPromotionPressureLabel(career.PromotionPressureLabel)}，黜压{OfficeShellTextAdapter.RenderDemotionPressureLabel(career.DemotionPressureLabel)}。"
						: "候补听选。",
					TaskSummary = OfficeShellTextAdapter.RenderAdministrativeTaskTier(career.AdministrativeTaskTier) + "差遣：" + OfficeShellTextAdapter.RenderAdministrativeTask(career.CurrentAdministrativeTask),
					PetitionSummary = $"词牍压{career.PetitionPressure}，积案{career.PetitionBacklog}。",
					PressureSummary = OfficeShellTextAdapter.BuildOfficePressureSummary(career.HasAppointment, career.LastOutcome, career.PetitionBacklog, career.PromotionPressureLabel, career.DemotionPressureLabel),
					PetitionOutcomeCategory = OfficeShellTextAdapter.RenderPetitionOutcomeCategory(career.PetitionOutcomeCategory),
					LastOutcome = career.LastOutcome,
					LastPetitionOutcome = OfficeShellTextAdapter.RenderPetitionOutcome(career.LastPetitionOutcome)
				})
				.ToArray(),
			Jurisdictions = context.OrderedJurisdictions
				.Select(jurisdiction =>
				{
					context.GovernanceBySettlement.TryGetValue(
						jurisdiction.SettlementId.Value,
						out SettlementGovernanceLaneSnapshot? governance);
					return new OfficeJurisdictionViewModel
					{
						SettlementLabel = settlementNames.TryGetValue(jurisdiction.SettlementId.Value, out string? settlementName)
							? settlementName
							: $"乡里#{jurisdiction.SettlementId.Value}",
						LeadSummary = OfficeShellTextAdapter.RenderOfficeTitle(jurisdiction.LeadOfficeTitle) + " " + jurisdiction.LeadOfficialName,
						LeverageSummary = $"秩阶{jurisdiction.AuthorityTier}，乡面杠力{jurisdiction.JurisdictionLeverage}。",
						PetitionSummary = $"词牍压{jurisdiction.PetitionPressure}，积案{jurisdiction.PetitionBacklog}。",
						TaskSummary = OfficeShellTextAdapter.RenderAdministrativeTaskTier(jurisdiction.AdministrativeTaskTier) + "差遣：" + OfficeShellTextAdapter.RenderAdministrativeTask(jurisdiction.CurrentAdministrativeTask),
						PetitionOutcomeCategory = OfficeShellTextAdapter.RenderPetitionOutcomeCategory(jurisdiction.PetitionOutcomeCategory),
						LastPetitionOutcome = OfficeShellTextAdapter.RenderPetitionOutcome(jurisdiction.LastPetitionOutcome),
						OfficeImplementationReadbackSummary = governance?.OfficeImplementationReadbackSummary ?? string.Empty,
						OfficeNextStepReadbackSummary = governance?.OfficeNextStepReadbackSummary ?? string.Empty,
						OfficeLaneEntryReadbackSummary = governance?.OfficeLaneEntryReadbackSummary ?? string.Empty,
						OfficeLaneReceiptClosureSummary = governance?.OfficeLaneReceiptClosureSummary ?? string.Empty,
						OfficeLaneResidueFollowUpSummary = governance?.OfficeLaneResidueFollowUpSummary ?? string.Empty,
						OfficeLaneNoLoopGuardSummary = governance?.OfficeLaneNoLoopGuardSummary ?? string.Empty,
						CourtPolicyEntryReadbackSummary = governance?.CourtPolicyEntryReadbackSummary ?? string.Empty,
						CourtPolicyDispatchReadbackSummary = governance?.CourtPolicyDispatchReadbackSummary ?? string.Empty,
						CourtPolicyPublicReadbackSummary = governance?.CourtPolicyPublicReadbackSummary ?? string.Empty,
						CourtPolicyNoLoopGuardSummary = governance?.CourtPolicyNoLoopGuardSummary ?? string.Empty,
						WarfareLaneEntryReadbackSummary = governance?.WarfareLaneEntryReadbackSummary ?? string.Empty,
						ForceReadinessReadbackSummary = governance?.ForceReadinessReadbackSummary ?? string.Empty,
						CampaignAftermathReadbackSummary = governance?.CampaignAftermathReadbackSummary ?? string.Empty,
						WarfareLaneReceiptClosureSummary = governance?.WarfareLaneReceiptClosureSummary ?? string.Empty,
						WarfareLaneResidueFollowUpSummary = governance?.WarfareLaneResidueFollowUpSummary ?? string.Empty,
						WarfareLaneNoLoopGuardSummary = governance?.WarfareLaneNoLoopGuardSummary ?? string.Empty,
						RegimeOfficeReadbackSummary = governance?.RegimeOfficeReadbackSummary ?? string.Empty,
						CanalRouteReadbackSummary = governance?.CanalRouteReadbackSummary ?? string.Empty,
						ResidueHealthSummary = governance?.ResidueHealthSummary ?? string.Empty,
					};
				})
				.ToArray()
		};
	}
}
