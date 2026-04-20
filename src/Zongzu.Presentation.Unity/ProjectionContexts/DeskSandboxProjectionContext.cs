using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal sealed class DeskSandboxProjectionContext
{
	private readonly Dictionary<int, PopulationSettlementSnapshot> _populationBySettlement;
	private readonly ILookup<int, AcademySnapshot> _academiesBySettlement;
	private readonly Dictionary<int, MarketSnapshot> _marketsBySettlement;
	private readonly ILookup<int, TradeRouteSnapshot> _tradeRoutesBySettlement;
	private readonly Dictionary<int, SettlementPublicLifeSnapshot> _publicLifeBySettlement;
	private readonly Dictionary<int, JurisdictionAuthoritySnapshot> _jurisdictionsBySettlement;
	private readonly Dictionary<int, SettlementGovernanceLaneSnapshot> _governanceBySettlement;
	private readonly Dictionary<int, CampaignFrontSnapshot> _campaignsBySettlement;
	private readonly Dictionary<int, CampaignMobilizationSignalSnapshot> _mobilizationSignalsBySettlement;

	private DeskSandboxProjectionContext(
		SettlementSnapshot[] orderedSettlements,
		Dictionary<int, PopulationSettlementSnapshot> populationBySettlement,
		ILookup<int, AcademySnapshot> academiesBySettlement,
		Dictionary<int, MarketSnapshot> marketsBySettlement,
		ILookup<int, TradeRouteSnapshot> tradeRoutesBySettlement,
		Dictionary<int, SettlementPublicLifeSnapshot> publicLifeBySettlement,
		Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement,
		Dictionary<int, SettlementGovernanceLaneSnapshot> governanceBySettlement,
		Dictionary<int, CampaignFrontSnapshot> campaignsBySettlement,
		Dictionary<int, CampaignMobilizationSignalSnapshot> mobilizationSignalsBySettlement)
	{
		OrderedSettlements = orderedSettlements;
		_populationBySettlement = populationBySettlement;
		_academiesBySettlement = academiesBySettlement;
		_marketsBySettlement = marketsBySettlement;
		_tradeRoutesBySettlement = tradeRoutesBySettlement;
		_publicLifeBySettlement = publicLifeBySettlement;
		_jurisdictionsBySettlement = jurisdictionsBySettlement;
		_governanceBySettlement = governanceBySettlement;
		_campaignsBySettlement = campaignsBySettlement;
		_mobilizationSignalsBySettlement = mobilizationSignalsBySettlement;
	}

	internal SettlementSnapshot[] OrderedSettlements { get; }

	internal static DeskSandboxProjectionContext Create(PresentationReadModelBundle bundle)
	{
		return new DeskSandboxProjectionContext(
			bundle.Settlements
				.OrderBy(settlement => settlement.Name, StringComparer.Ordinal)
				.ToArray(),
			bundle.PopulationSettlements.ToDictionary(settlement => settlement.SettlementId.Value, settlement => settlement),
			bundle.Academies.ToLookup(academy => academy.SettlementId.Value),
			bundle.Markets.ToDictionary(market => market.SettlementId.Value, market => market),
			bundle.TradeRoutes.ToLookup(route => route.SettlementId.Value),
			bundle.PublicLifeSettlements.ToDictionary(settlement => settlement.SettlementId.Value, settlement => settlement),
			bundle.OfficeJurisdictions.ToDictionary(jurisdiction => jurisdiction.SettlementId.Value, jurisdiction => jurisdiction),
			bundle.GovernanceSettlements.ToDictionary(settlement => settlement.SettlementId.Value, settlement => settlement),
			bundle.Campaigns.ToDictionary(campaign => campaign.AnchorSettlementId.Value, campaign => campaign),
			bundle.CampaignMobilizationSignals.ToDictionary(signal => signal.SettlementId.Value, signal => signal));
	}

	internal PopulationSettlementSnapshot? GetPopulation(SettlementId settlementId)
	{
		_populationBySettlement.TryGetValue(settlementId.Value, out PopulationSettlementSnapshot? population);
		return population;
	}

	internal AcademySnapshot[] GetAcademies(SettlementId settlementId)
	{
		return _academiesBySettlement[settlementId.Value]
			.OrderBy(academy => academy.AcademyName, StringComparer.Ordinal)
			.ToArray();
	}

	internal MarketSnapshot? GetMarket(SettlementId settlementId)
	{
		_marketsBySettlement.TryGetValue(settlementId.Value, out MarketSnapshot? market);
		return market;
	}

	internal SettlementPublicLifeSnapshot? GetPublicLife(SettlementId settlementId)
	{
		_publicLifeBySettlement.TryGetValue(settlementId.Value, out SettlementPublicLifeSnapshot? publicLife);
		return publicLife;
	}

	internal JurisdictionAuthoritySnapshot? GetJurisdiction(SettlementId settlementId)
	{
		_jurisdictionsBySettlement.TryGetValue(settlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
		return jurisdiction;
	}

	internal SettlementGovernanceLaneSnapshot? GetGovernance(SettlementId settlementId)
	{
		_governanceBySettlement.TryGetValue(settlementId.Value, out SettlementGovernanceLaneSnapshot? governance);
		return governance;
	}

	internal CampaignFrontSnapshot? GetCampaign(SettlementId settlementId)
	{
		_campaignsBySettlement.TryGetValue(settlementId.Value, out CampaignFrontSnapshot? campaign);
		return campaign;
	}

	internal CampaignMobilizationSignalSnapshot? GetMobilizationSignal(SettlementId settlementId)
	{
		_mobilizationSignalsBySettlement.TryGetValue(settlementId.Value, out CampaignMobilizationSignalSnapshot? signal);
		return signal;
	}

	internal TradeRouteSnapshot[] GetTradeRoutes(SettlementId settlementId)
	{
		return _tradeRoutesBySettlement[settlementId.Value]
			.OrderBy(route => route.RouteName, StringComparer.Ordinal)
			.ToArray();
	}
}
