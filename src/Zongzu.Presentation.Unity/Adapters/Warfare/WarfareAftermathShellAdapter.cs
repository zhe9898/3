using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal static class WarfareAftermathShellAdapter
{
	internal static string BuildGreatHallAftermathDocketSummary(
		PresentationReadModelBundle bundle,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(bundle);
		ArgumentNullException.ThrowIfNull(notifications);

		if (bundle.Campaigns.Count == 0 || bundle.CampaignAftermathDockets.Count == 0)
		{
			return "堂上尚无战后案牍。";
		}

		CampaignFrontSnapshot? leadCampaign = bundle.Campaigns
			.Join(
				bundle.CampaignAftermathDockets,
				campaign => campaign.CampaignId,
				docket => docket.CampaignId,
				(campaign, docket) => new { campaign, docket })
			.OrderByDescending(entry => CountDocketEntries(entry.docket))
			.ThenByDescending(entry => entry.campaign.FrontPressure)
			.ThenBy(entry => entry.campaign.CampaignId.Value)
			.Select(entry => entry.campaign)
			.FirstOrDefault();
		if (leadCampaign is null)
		{
			return "堂上尚无战后案牍。";
		}

		AftermathDocketSnapshot docket = bundle.CampaignAftermathDockets.First(entry => entry.CampaignId == leadCampaign.CampaignId);
		return leadCampaign.AnchorSettlementName + "堂案今并载" + ComposeDocketClauseText(docket) + "。";
	}

	internal static string BuildSettlementAftermathSummary(
		SettlementSnapshot settlement,
		PopulationSettlementSnapshot? population,
		JurisdictionAuthoritySnapshot? jurisdiction,
		CampaignFrontSnapshot? campaign,
		AftermathDocketSnapshot? aftermathDocket,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(settlement);
		ArgumentNullException.ThrowIfNull(notifications);

		if (campaign is null || aftermathDocket is null || CountDocketEntries(aftermathDocket) == 0)
		{
			return "战后案牍未起。";
		}

		return "战后案牍：" + ComposeDocketClauseText(aftermathDocket) + "。";
	}

	internal static string BuildCampaignAftermathDocketSummary(
		CampaignFrontSnapshot campaign,
		AftermathDocketSnapshot? aftermathDocket,
		SettlementSnapshot? settlement,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(campaign);
		ArgumentNullException.ThrowIfNull(notifications);

		if (aftermathDocket is null || CountDocketEntries(aftermathDocket) == 0)
		{
			return campaign.IsActive ? "军机案下仍止于军报与路报。" : "军机案下尚未并成赏罚抚恤诸册。";
		}

		return "军机案今并载" + ComposeDocketClauseText(aftermathDocket) + "。";
	}

	private static int CountDocketEntries(AftermathDocketSnapshot docket)
	{
		return docket.Merits.Count + docket.Blames.Count + docket.ReliefNeeds.Count + docket.RouteRepairs.Count;
	}

	private static string ComposeDocketClauseText(AftermathDocketSnapshot docket)
	{
		List<string> clauses = new List<string>();
		AppendDocketClause(clauses, docket.Merits, "记功簿");
		AppendDocketClause(clauses, docket.Blames, "劾责状");
		AppendDocketClause(clauses, docket.ReliefNeeds, "抚恤簿");
		AppendDocketClause(clauses, docket.RouteRepairs, "清路札");
		return clauses.Count == 0 ? "军报与路报" : string.Join("、", clauses);
	}

	private static void AppendDocketClause(List<string> clauses, IReadOnlyList<string> entries, string label)
	{
		if (entries.Count > 0)
		{
			clauses.Add(label);
		}
	}
}
