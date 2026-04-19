using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal sealed class HallDocketProjectionContext
{
	private readonly IReadOnlyDictionary<int, HallDocketItemSnapshot[]> _settlementItems;

	private HallDocketProjectionContext(
		HallDocketItemSnapshot? leadItem,
		HallDocketItemSnapshot[] secondaryItems,
		IReadOnlyDictionary<int, HallDocketItemSnapshot[]> settlementItems)
	{
		LeadItem = leadItem;
		SecondaryItems = secondaryItems;
		_settlementItems = settlementItems;
	}

	internal HallDocketItemSnapshot? LeadItem { get; }

	internal HallDocketItemSnapshot[] SecondaryItems { get; }

	internal static HallDocketProjectionContext Create(HallDocketStackSnapshot hallDocket)
	{
		ArgumentNullException.ThrowIfNull(hallDocket);

		HallDocketItemSnapshot? leadItem = string.IsNullOrWhiteSpace(hallDocket.LeadItem.Headline)
			? null
			: hallDocket.LeadItem;
		HallDocketItemSnapshot[] secondaryItems = hallDocket.SecondaryItems
			.Where(item => !string.IsNullOrWhiteSpace(item.Headline))
			.ToArray();

		HallDocketItemSnapshot[] allItems = leadItem is null
			? secondaryItems
			: [leadItem, .. secondaryItems];

		Dictionary<int, HallDocketItemSnapshot[]> settlementItems = allItems
			.GroupBy(item => item.SettlementId.Value)
			.ToDictionary(group => group.Key, group => group.ToArray());

		return new HallDocketProjectionContext(leadItem, secondaryItems, settlementItems);
	}

	internal HallDocketItemSnapshot[] GetSettlementItems(SettlementId settlementId)
	{
		return _settlementItems.TryGetValue(settlementId.Value, out HallDocketItemSnapshot[]? items)
			? items
			: [];
	}

	internal bool HasSettlementLeadItem(SettlementId settlementId)
	{
		return LeadItem?.SettlementId == settlementId;
	}

	internal string GetSettlementLeadLaneKey(SettlementId settlementId)
	{
		return HasSettlementLeadItem(settlementId) && LeadItem is not null
			? LeadItem.LaneKey
			: string.Empty;
	}
}
