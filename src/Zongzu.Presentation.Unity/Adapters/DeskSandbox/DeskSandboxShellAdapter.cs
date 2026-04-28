using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class DeskSandboxShellAdapter
{
	internal static DeskSandboxViewModel BuildDeskSandbox(
		PresentationReadModelBundle bundle,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(bundle);
		ArgumentNullException.ThrowIfNull(notifications);
		DeskSandboxProjectionContext context = DeskSandboxProjectionContext.Create(bundle);

		DeskSandboxViewModel deskSandbox = new DeskSandboxViewModel
		{
			Settlements = context.OrderedSettlements
				.Select(settlement =>
				{
					PopulationSettlementSnapshot? populationSettlement = context.GetPopulation(settlement.Id);
					AcademySnapshot[] academies = context.GetAcademies(settlement.Id);
					MarketSnapshot? market = context.GetMarket(settlement.Id);
					SettlementPublicLifeSnapshot? publicLife = context.GetPublicLife(settlement.Id);
					JurisdictionAuthoritySnapshot? jurisdiction = context.GetJurisdiction(settlement.Id);
					SettlementGovernanceLaneSnapshot? governance = context.GetGovernance(settlement.Id);
					CampaignFrontSnapshot? campaign = context.GetCampaign(settlement.Id);
					AftermathDocketSnapshot? aftermathDocket = context.GetAftermathDocket(campaign);
					CampaignMobilizationSignalSnapshot? mobilizationSignal = context.GetMobilizationSignal(settlement.Id);
					ClanTradeRouteSnapshot[] clanTradeRoutes = context.GetClanTradeRoutes(settlement.Id);
					HouseholdSocialPressureSnapshot[] householdPressures = context.GetHouseholdPressures(settlement.Id);
					SettlementMobilitySnapshot? mobility = context.GetMobility(settlement.Id);
					HallDocketShellAdapter.SettlementHallAgendaProjection hallAgenda = HallDocketShellAdapter.BuildSettlementHallAgenda(bundle.HallDocket, settlement.Id);

					SettlementNodeViewModel settlementNode = new()
					{
						SettlementName = settlement.Name,
						Security = settlement.Security,
						Prosperity = settlement.Prosperity,
						AcademySummary = academies.Length == 0 ? "塾馆未立。" : string.Join(", ", academies.Select(academy => academy.AcademyName)),
						MarketSummary = market == null ? "市肆未起。" : $"{market.MarketName}：市需{market.Demand}，价行{market.PriceIndex}，路险{market.LocalRisk}。",
						PublicLifeSummary = publicLife == null ? "乡里街谈未起，县门榜示亦未壅塞。" : PublicLifeShellAdapter.BuildSettlementPublicLifeSummary(publicLife),
						GovernanceSummary = OfficeShellAdapter.BuildSettlementGovernanceFallbackSummary(jurisdiction),
						CampaignSummary = WarfareCampaignShellAdapter.BuildSettlementCampaignSummary(campaign, mobilizationSignal, settlement, clanTradeRoutes),
						AftermathSummary = WarfareAftermathShellAdapter.BuildSettlementAftermathSummary(settlement, populationSettlement, jurisdiction, campaign, aftermathDocket, notifications),
						PressureSummary = BuildSettlementHouseholdPressureSummary(populationSettlement, householdPressures),
						MobilitySummary = BuildSettlementMobilitySummary(mobility),
						HallAgendaSummary = hallAgenda.Summary,
						HallAgendaItems = hallAgenda.Items,
						HallAgendaCount = hallAgenda.Count,
						HallAgendaLaneKeys = hallAgenda.LaneKeys,
						HasLeadHallAgendaItem = hallAgenda.HasLeadItem,
						LeadHallAgendaLaneKey = hallAgenda.LeadLaneKey
					};

					settlementNode.GovernanceSummary = GovernanceShellAdapter.BuildSettlementGovernanceSummary(settlementNode.GovernanceSummary, governance);
					if (governance != null)
					{
						settlementNode.OfficeImplementationReadbackSummary = governance.OfficeImplementationReadbackSummary;
						settlementNode.OfficeNextStepReadbackSummary = governance.OfficeNextStepReadbackSummary;
						settlementNode.OfficeLaneEntryReadbackSummary = governance.OfficeLaneEntryReadbackSummary;
						settlementNode.OfficeLaneReceiptClosureSummary = governance.OfficeLaneReceiptClosureSummary;
						settlementNode.OfficeLaneResidueFollowUpSummary = governance.OfficeLaneResidueFollowUpSummary;
						settlementNode.OfficeLaneNoLoopGuardSummary = governance.OfficeLaneNoLoopGuardSummary;
						settlementNode.CourtPolicyEntryReadbackSummary = governance.CourtPolicyEntryReadbackSummary;
						settlementNode.CourtPolicyDispatchReadbackSummary = governance.CourtPolicyDispatchReadbackSummary;
						settlementNode.CourtPolicyPublicReadbackSummary = governance.CourtPolicyPublicReadbackSummary;
						settlementNode.CourtPolicyNoLoopGuardSummary = governance.CourtPolicyNoLoopGuardSummary;
						settlementNode.FamilyLaneEntryReadbackSummary = governance.FamilyLaneEntryReadbackSummary;
						settlementNode.FamilyElderExplanationReadbackSummary = governance.FamilyElderExplanationReadbackSummary;
						settlementNode.FamilyGuaranteeReadbackSummary = governance.FamilyGuaranteeReadbackSummary;
						settlementNode.FamilyHouseFaceReadbackSummary = governance.FamilyHouseFaceReadbackSummary;
						settlementNode.FamilyLaneReceiptClosureSummary = governance.FamilyLaneReceiptClosureSummary;
						settlementNode.FamilyLaneResidueFollowUpSummary = governance.FamilyLaneResidueFollowUpSummary;
						settlementNode.FamilyLaneNoLoopGuardSummary = governance.FamilyLaneNoLoopGuardSummary;
						settlementNode.WarfareLaneEntryReadbackSummary = governance.WarfareLaneEntryReadbackSummary;
						settlementNode.ForceReadinessReadbackSummary = governance.ForceReadinessReadbackSummary;
						settlementNode.CampaignAftermathReadbackSummary = governance.CampaignAftermathReadbackSummary;
						settlementNode.WarfareLaneReceiptClosureSummary = governance.WarfareLaneReceiptClosureSummary;
						settlementNode.WarfareLaneResidueFollowUpSummary = governance.WarfareLaneResidueFollowUpSummary;
						settlementNode.WarfareLaneNoLoopGuardSummary = governance.WarfareLaneNoLoopGuardSummary;
						settlementNode.RegimeOfficeReadbackSummary = governance.RegimeOfficeReadbackSummary;
						settlementNode.CanalRouteReadbackSummary = governance.CanalRouteReadbackSummary;
						settlementNode.ResidueHealthSummary = governance.ResidueHealthSummary;
					}

					return settlementNode;
				})
				.ToArray()
		};

		PublicLifeShellAdapter.HydrateDeskSandboxPublicLife(bundle, deskSandbox);
		return deskSandbox;
	}

	private static string BuildSettlementHouseholdPressureSummary(
		PopulationSettlementSnapshot? populationSettlement,
		IReadOnlyList<HouseholdSocialPressureSnapshot> householdPressures)
	{
		string populationSummary = populationSettlement == null
			? "民户情形未起。"
			: $"民困{populationSettlement.CommonerDistress}，丁力{populationSettlement.LaborSupply}，流徙{populationSettlement.MigrationPressure}。";
		HouseholdSocialPressureSnapshot? leadHousehold = householdPressures.FirstOrDefault();
		if (leadHousehold is null)
		{
			return populationSummary;
		}

		HouseholdSocialPressureSignalSnapshot? orderResidueSignal = leadHousehold.Signals
			.FirstOrDefault(static signal =>
				string.Equals(signal.SignalKey, HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue, StringComparison.Ordinal)
				&& signal.Score > 0);
		string householdSummary = orderResidueSignal?.Summary
			?? leadHousehold.VisibleChainSummary
			?? leadHousehold.PressureSummary;

		return string.IsNullOrWhiteSpace(householdSummary)
			? populationSummary
			: $"{populationSummary} {householdSummary}";
	}

	private static string BuildSettlementMobilitySummary(SettlementMobilitySnapshot? mobility)
	{
		if (mobility is null)
		{
			return "人员流动暂未投出。";
		}

		return $"{mobility.PoolThicknessSummary} {mobility.FocusReadbackSummary}";
	}
}
