using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class HallDocketTextAdapter
{
	internal static string BuildLeadGuidance(HallDocketItemSnapshot leadHallDocketItem)
	{
		string text = ShellTextAdapter.CombineDistinct(
			leadHallDocketItem.GuidanceSummary,
			leadHallDocketItem.PhaseSummary,
			leadHallDocketItem.HandlingSummary);
		return string.IsNullOrWhiteSpace(text) ? leadHallDocketItem.WhyNowSummary : text;
	}

	internal static string BuildGreatHallSecondaryDocketSummary(HallDocketItemSnapshot item)
	{
		string text = ShellTextAdapter.CombineDistinct(item.WhyNowSummary, item.PhaseSummary);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}

		return ShellTextAdapter.CombineDistinct(item.GuidanceSummary, item.HandlingSummary);
	}

	internal static string BuildSettlementHallAgendaSummary(IReadOnlyList<SettlementHallAgendaItemViewModel> hallAgendaItems)
	{
		if (hallAgendaItems.Count == 0)
		{
			return string.Empty;
		}

		return "堂上并记：" + string.Join("；", hallAgendaItems.Select(BuildSettlementHallAgendaClause));
	}

	internal static string BuildSettlementHallAgendaItemSummary(HallDocketItemSnapshot item)
	{
		string text = ShellTextAdapter.CombineDistinct(item.WhyNowSummary, item.PhaseSummary);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}

		return ShellTextAdapter.CombineDistinct(item.GuidanceSummary, item.HandlingSummary);
	}

	private static string BuildSettlementHallAgendaClause(SettlementHallAgendaItemViewModel item)
	{
		return string.IsNullOrWhiteSpace(item.PhaseLabel)
			? item.Headline
			: $"{item.Headline}（{item.PhaseLabel}）";
	}
}
