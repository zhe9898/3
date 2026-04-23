using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal static class HallDocketShellAdapter
{
	internal readonly record struct SettlementHallAgendaProjection(
		string Summary,
		SettlementHallAgendaItemViewModel[] Items,
		string[] LaneKeys,
		bool HasLeadItem,
		string LeadLaneKey)
	{
		public int Count => Items.Length;
	}

	internal static bool TryBuildGreatHallLead(
		HallDocketStackSnapshot hallDocket,
		out string title,
		out string guidance)
	{
		HallDocketProjectionContext context = HallDocketProjectionContext.Create(hallDocket);
		HallDocketItemSnapshot? leadHallDocketItem = context.LeadItem;
		if (leadHallDocketItem is null)
		{
			title = string.Empty;
			guidance = string.Empty;
			return false;
		}

		title = leadHallDocketItem.Headline;
		guidance = HallDocketTextAdapter.BuildLeadGuidance(leadHallDocketItem);
		return true;
	}

	internal static GreatHallSecondaryDocketViewModel[] BuildGreatHallSecondaryDockets(HallDocketStackSnapshot hallDocket)
	{
		HallDocketProjectionContext context = HallDocketProjectionContext.Create(hallDocket);
		return context.SecondaryItems
			.Select(item => new GreatHallSecondaryDocketViewModel
			{
				LaneKey = item.LaneKey,
				TargetLabel = string.IsNullOrWhiteSpace(item.TargetLabel) ? item.NodeLabel : item.TargetLabel,
				Headline = item.Headline,
				PhaseLabel = item.PhaseLabel,
				Summary = HallDocketTextAdapter.BuildGreatHallSecondaryDocketSummary(item)
			})
			.ToArray();
	}

	internal static SettlementHallAgendaProjection BuildSettlementHallAgenda(
		HallDocketStackSnapshot hallDocket,
		SettlementId settlementId)
	{
		HallDocketProjectionContext context = HallDocketProjectionContext.Create(hallDocket);
		SettlementHallAgendaItemViewModel[] items = BuildSettlementHallAgendaItems(context, settlementId);
		return new SettlementHallAgendaProjection(
			HallDocketTextAdapter.BuildSettlementHallAgendaSummary(items),
			items,
			BuildSettlementHallAgendaLaneKeys(items),
			context.HasSettlementLeadItem(settlementId),
			context.GetSettlementLeadLaneKey(settlementId));
	}

	private static SettlementHallAgendaItemViewModel[] BuildSettlementHallAgendaItems(HallDocketProjectionContext context, SettlementId settlementId)
	{
		return context.GetSettlementItems(settlementId)
			.Select(item => new SettlementHallAgendaItemViewModel
			{
				LaneKey = item.LaneKey,
				Headline = item.Headline,
				PhaseLabel = item.PhaseLabel,
				Summary = HallDocketTextAdapter.BuildSettlementHallAgendaItemSummary(item)
			})
			.ToArray();
	}

	private static string[] BuildSettlementHallAgendaLaneKeys(IReadOnlyList<SettlementHallAgendaItemViewModel> hallAgendaItems)
	{
		return hallAgendaItems
			.Select(item => item.LaneKey)
			.Where(static laneKey => !string.IsNullOrWhiteSpace(laneKey))
			.Distinct(StringComparer.Ordinal)
			.ToArray();
	}
}
