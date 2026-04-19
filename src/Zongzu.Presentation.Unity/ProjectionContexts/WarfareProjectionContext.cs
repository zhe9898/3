using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal sealed class WarfareProjectionContext
{
	private WarfareProjectionContext()
	{
	}

	internal int ActiveCampaignCount { get; private init; }

	internal int PeakFrontPressure { get; private init; }

	internal IReadOnlyDictionary<int, SettlementSnapshot> SettlementsById { get; private init; } = new Dictionary<int, SettlementSnapshot>();

	internal ILookup<int, TradeRouteSnapshot> TradeRoutesBySettlement { get; private init; } = EmptyTradeRouteLookup.Instance;

	internal CampaignFrontSnapshot[] OrderedCampaigns { get; private init; } = [];

	internal CampaignMobilizationSignalSnapshot[] OrderedMobilizationSignals { get; private init; } = [];

	internal PlayerCommandAffordanceSnapshot[] OrderedAffordances { get; private init; } = [];

	internal PlayerCommandReceiptSnapshot[] OrderedReceipts { get; private init; } = [];

	internal CampaignFrontSnapshot? LeadCampaign { get; private init; }

	internal SettlementSnapshot? LeadCampaignSettlement { get; private init; }

	internal TradeRouteSnapshot[] LeadCampaignTradeRoutes { get; private init; } = [];

	internal static WarfareProjectionContext Create(PresentationReadModelBundle bundle)
	{
		ArgumentNullException.ThrowIfNull(bundle);

		CampaignFrontSnapshot[] orderedCampaigns = bundle.Campaigns
			.OrderByDescending(campaign => campaign.IsActive)
			.ThenByDescending(campaign => campaign.FrontPressure)
			.ThenBy(campaign => campaign.CampaignId.Value)
			.ToArray();
		ILookup<int, TradeRouteSnapshot> tradeRoutesBySettlement = bundle.TradeRoutes
			.ToLookup(route => route.SettlementId.Value);
		IReadOnlyDictionary<int, SettlementSnapshot> settlementsById = bundle.Settlements
			.ToDictionary(settlement => settlement.Id.Value, settlement => settlement);
		CampaignFrontSnapshot? leadCampaign = bundle.Campaigns
			.OrderByDescending(campaign => campaign.FrontPressure)
			.ThenByDescending(campaign => campaign.MobilizedForceCount)
			.ThenBy(campaign => campaign.CampaignId.Value)
			.FirstOrDefault();
		SettlementSnapshot? leadCampaignSettlement = null;
		TradeRouteSnapshot[] leadCampaignTradeRoutes = [];
		if (leadCampaign != null)
		{
			settlementsById.TryGetValue(leadCampaign.AnchorSettlementId.Value, out leadCampaignSettlement);
			leadCampaignTradeRoutes = tradeRoutesBySettlement[leadCampaign.AnchorSettlementId.Value]
				.OrderBy(route => route.RouteName, StringComparer.Ordinal)
				.ToArray();
		}

		return new WarfareProjectionContext
		{
			ActiveCampaignCount = bundle.Campaigns.Count(campaign => campaign.IsActive),
			PeakFrontPressure = bundle.Campaigns.Count == 0
				? 0
				: bundle.Campaigns.Max(campaign => campaign.FrontPressure),
			SettlementsById = settlementsById,
			TradeRoutesBySettlement = tradeRoutesBySettlement,
			OrderedCampaigns = orderedCampaigns,
			OrderedMobilizationSignals = bundle.CampaignMobilizationSignals
				.OrderByDescending(signal => signal.ResponseActivationLevel)
				.ThenBy(signal => signal.SettlementId.Value)
				.ToArray(),
			OrderedAffordances = bundle.PlayerCommands.Affordances
				.Where(command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal))
				.OrderBy(command => command.SettlementId.Value)
				.ThenBy(command => command.CommandName, StringComparer.Ordinal)
				.ToArray(),
			OrderedReceipts = bundle.PlayerCommands.Receipts
				.Where(receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal))
				.OrderBy(receipt => receipt.SettlementId.Value)
				.ThenBy(receipt => receipt.CommandName, StringComparer.Ordinal)
				.ToArray(),
			LeadCampaign = leadCampaign,
			LeadCampaignSettlement = leadCampaignSettlement,
			LeadCampaignTradeRoutes = leadCampaignTradeRoutes
		};
	}

	private sealed class EmptyTradeRouteLookup : ILookup<int, TradeRouteSnapshot>
	{
		internal static readonly EmptyTradeRouteLookup Instance = new();

		public IEnumerable<TradeRouteSnapshot> this[int key] => Array.Empty<TradeRouteSnapshot>();

		public int Count => 0;

		public bool Contains(int key) => false;

		public IEnumerator<IGrouping<int, TradeRouteSnapshot>> GetEnumerator() => Enumerable.Empty<IGrouping<int, TradeRouteSnapshot>>().GetEnumerator();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
