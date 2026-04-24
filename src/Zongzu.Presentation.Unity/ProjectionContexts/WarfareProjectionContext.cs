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

	internal ILookup<int, ClanTradeRouteSnapshot> ClanTradeRoutesBySettlement { get; private init; } = EmptyClanTradeRouteLookup.Instance;

	internal CampaignFrontSnapshot[] OrderedCampaigns { get; private init; } = [];

	internal CampaignMobilizationSignalSnapshot[] OrderedMobilizationSignals { get; private init; } = [];

	internal PlayerCommandAffordanceSnapshot[] OrderedAffordances { get; private init; } = [];

	internal PlayerCommandReceiptSnapshot[] OrderedReceipts { get; private init; } = [];

	internal CampaignFrontSnapshot? LeadCampaign { get; private init; }

	internal SettlementSnapshot? LeadCampaignSettlement { get; private init; }

	internal ClanTradeRouteSnapshot[] LeadCampaignClanTradeRoutes { get; private init; } = [];

	internal static WarfareProjectionContext Create(PresentationReadModelBundle bundle)
	{
		ArgumentNullException.ThrowIfNull(bundle);

		CampaignFrontSnapshot[] orderedCampaigns = bundle.Campaigns
			.OrderByDescending(campaign => campaign.IsActive)
			.ThenByDescending(campaign => campaign.FrontPressure)
			.ThenBy(campaign => campaign.CampaignId.Value)
			.ToArray();
		ILookup<int, ClanTradeRouteSnapshot> clanTradeRoutesBySettlement = bundle.ClanTradeRoutes
			.ToLookup(route => route.SettlementId.Value);
		IReadOnlyDictionary<int, SettlementSnapshot> settlementsById = bundle.Settlements
			.ToDictionary(settlement => settlement.Id.Value, settlement => settlement);
		CampaignFrontSnapshot? leadCampaign = bundle.Campaigns
			.OrderByDescending(campaign => campaign.FrontPressure)
			.ThenByDescending(campaign => campaign.MobilizedForceCount)
			.ThenBy(campaign => campaign.CampaignId.Value)
			.FirstOrDefault();
		SettlementSnapshot? leadCampaignSettlement = null;
		ClanTradeRouteSnapshot[] leadCampaignTradeRoutes = [];
		if (leadCampaign != null)
		{
			settlementsById.TryGetValue(leadCampaign.AnchorSettlementId.Value, out leadCampaignSettlement);
			leadCampaignTradeRoutes = clanTradeRoutesBySettlement[leadCampaign.AnchorSettlementId.Value]
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
			ClanTradeRoutesBySettlement = clanTradeRoutesBySettlement,
			OrderedCampaigns = orderedCampaigns,
			OrderedMobilizationSignals = bundle.CampaignMobilizationSignals
				.OrderByDescending(signal => signal.ResponseActivationLevel)
				.ThenBy(signal => signal.SettlementId.Value)
				.ToArray(),
			OrderedAffordances = bundle.PlayerCommands
				.EnumerateAffordances(PlayerCommandSurfaceKeys.Warfare)
				.OrderBy(command => command.SettlementId.Value)
				.ThenBy(command => command.CommandName, StringComparer.Ordinal)
				.ToArray(),
			OrderedReceipts = bundle.PlayerCommands
				.EnumerateReceipts(PlayerCommandSurfaceKeys.Warfare)
				.OrderBy(receipt => receipt.SettlementId.Value)
				.ThenBy(receipt => receipt.CommandName, StringComparer.Ordinal)
				.ToArray(),
			LeadCampaign = leadCampaign,
			LeadCampaignSettlement = leadCampaignSettlement,
			LeadCampaignClanTradeRoutes = leadCampaignTradeRoutes
		};
	}

	private sealed class EmptyClanTradeRouteLookup : ILookup<int, ClanTradeRouteSnapshot>
	{
		internal static readonly EmptyClanTradeRouteLookup Instance = new();

		public IEnumerable<ClanTradeRouteSnapshot> this[int key] => Array.Empty<ClanTradeRouteSnapshot>();

		public int Count => 0;

		public bool Contains(int key) => false;

		public IEnumerator<IGrouping<int, ClanTradeRouteSnapshot>> GetEnumerator() => Enumerable.Empty<IGrouping<int, ClanTradeRouteSnapshot>>().GetEnumerator();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
