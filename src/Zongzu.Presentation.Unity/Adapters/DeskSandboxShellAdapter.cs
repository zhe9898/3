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
					CampaignMobilizationSignalSnapshot? mobilizationSignal = context.GetMobilizationSignal(settlement.Id);
					TradeRouteSnapshot[] tradeRoutes = context.GetTradeRoutes(settlement.Id);
					HallDocketShellAdapter.SettlementHallAgendaProjection hallAgenda = HallDocketShellAdapter.BuildSettlementHallAgenda(bundle.HallDocket, settlement.Id);

					SettlementNodeViewModel settlementNode = new SettlementNodeViewModel
					{
						SettlementName = settlement.Name,
						Security = settlement.Security,
						Prosperity = settlement.Prosperity,
						AcademySummary = academies.Length == 0 ? "塾馆未立。" : string.Join(", ", academies.Select(academy => academy.AcademyName)),
						MarketSummary = market == null ? "市肆未起。" : $"{market.MarketName}：市需{market.Demand}，价行{market.PriceIndex}，路险{market.LocalRisk}。",
						PublicLifeSummary = publicLife == null ? "乡里街谈未起，县门榜示亦未壅塞。" : PublicLifeShellAdapter.BuildSettlementPublicLifeSummary(publicLife),
						GovernanceSummary = OfficeShellAdapter.BuildSettlementGovernanceFallbackSummary(jurisdiction),
						CampaignSummary = WarfareCampaignShellAdapter.BuildSettlementCampaignSummary(campaign, mobilizationSignal, settlement, tradeRoutes),
						AftermathSummary = WarfareAftermathShellAdapter.BuildSettlementAftermathSummary(settlement, populationSettlement, jurisdiction, campaign, notifications),
						PressureSummary = populationSettlement == null ? "民户情形未起。" : $"民困{populationSettlement.CommonerDistress}，丁力{populationSettlement.LaborSupply}，流徙{populationSettlement.MigrationPressure}。",
						HallAgendaSummary = hallAgenda.Summary,
						HallAgendaItems = hallAgenda.Items,
						HallAgendaCount = hallAgenda.Count,
						HallAgendaLaneKeys = hallAgenda.LaneKeys,
						HasLeadHallAgendaItem = hallAgenda.HasLeadItem,
						LeadHallAgendaLaneKey = hallAgenda.LeadLaneKey
					};

					settlementNode.GovernanceSummary = GovernanceShellAdapter.BuildSettlementGovernanceSummary(settlementNode.GovernanceSummary, governance);
					return settlementNode;
				})
				.ToArray()
		};

		PublicLifeShellAdapter.HydrateDeskSandboxPublicLife(bundle, deskSandbox);
		return deskSandbox;
	}
}
