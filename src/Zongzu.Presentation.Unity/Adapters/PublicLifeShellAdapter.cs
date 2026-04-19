using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal static class PublicLifeShellAdapter
{
	internal static string BuildGreatHallPublicLifeSummary(
		IReadOnlyList<SettlementPublicLifeSnapshot> publicLifeSettlements,
		SettlementPublicLifeSnapshot leadPublicLife)
	{
		int heatedSettlements = publicLifeSettlements.Count(settlement => settlement.StreetTalkHeat >= 55);
		int crowdedSettlements = publicLifeSettlements.Count(settlement =>
			settlement.NoticeVisibility >= 55 || settlement.PrefectureDispatchPressure >= 55);
		int delayedSettlements = publicLifeSettlements.Count(settlement => settlement.RoadReportLag >= 45);
		int conflictedSettlements = publicLifeSettlements.Count(settlement =>
			settlement.RoadReportLag >= 50
			|| settlement.CourierRisk >= 50
			|| Math.Abs(settlement.DocumentaryWeight - settlement.MarketRumorFlow) >= 12
			|| (settlement.PrefectureDispatchPressure >= 60 && settlement.PublicLegitimacy < 55));

		return $"今月县情起于{leadPublicLife.NodeLabel}，正值{leadPublicLife.MonthlyCadenceLabel}，{leadPublicLife.CrowdMixLabel}，街谈渐热{heatedSettlements}处，门前壅挤{crowdedSettlements}处，路报迟滞{delayedSettlements}处，说法相左{conflictedSettlements}处。{leadPublicLife.ContentionSummary}";
	}

	internal static string BuildSettlementPublicLifeSummary(SettlementPublicLifeSnapshot publicLife)
	{
		return publicLife.NodeLabel
			+ "："
			+ publicLife.CadenceSummary
			+ publicLife.PublicSummary
			+ publicLife.OfficialNoticeLine
			+ publicLife.StreetTalkLine
			+ publicLife.RoadReportLine
			+ publicLife.PrefectureDispatchLine
			+ publicLife.ContentionSummary
			+ publicLife.RouteReportSummary;
	}

	internal static void HydrateDeskSandboxPublicLife(
		PresentationReadModelBundle bundle,
		DeskSandboxViewModel deskSandbox)
	{
		SettlementSnapshot[] orderedSettlements = bundle.Settlements
			.OrderBy(settlement => settlement.Name, StringComparer.Ordinal)
			.ToArray();

		for (int i = 0; i < orderedSettlements.Length && i < deskSandbox.Settlements.Count; i++)
		{
			SettlementSnapshot settlement = orderedSettlements[i];
			SettlementNodeViewModel settlementNode = deskSandbox.Settlements[i];
			settlementNode.PublicLifeCommandAffordances = BuildSettlementPublicLifeAffordances(bundle, settlement.Id);
			settlementNode.PublicLifeRecentReceipts = BuildSettlementPublicLifeReceipts(bundle, settlement.Id);

			SettlementPublicLifeSnapshot? publicLife = bundle.PublicLifeSettlements
				.FirstOrDefault(snapshot => snapshot.SettlementId == settlement.Id);
			if (publicLife != null && !string.IsNullOrWhiteSpace(publicLife.ChannelSummary))
			{
				settlementNode.PublicLifeSummary += publicLife.ChannelSummary;
			}
		}
	}

	private static IReadOnlyList<CommandAffordanceViewModel> BuildSettlementPublicLifeAffordances(
		PresentationReadModelBundle bundle,
		SettlementId settlementId)
	{
		return CommandShellAdapter.BuildAffordances(
			bundle.PlayerCommands.Affordances
				.Where(command =>
					string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)
					&& command.SettlementId == settlementId)
				.OrderBy(command => command.TargetLabel, StringComparer.Ordinal)
				.ThenBy(command => command.CommandName, StringComparer.Ordinal));
	}

	private static IReadOnlyList<CommandReceiptViewModel> BuildSettlementPublicLifeReceipts(
		PresentationReadModelBundle bundle,
		SettlementId settlementId)
	{
		return CommandShellAdapter.BuildReceipts(
			bundle.PlayerCommands.Receipts
				.Where(receipt =>
					string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)
					&& receipt.SettlementId == settlementId)
				.OrderBy(receipt => receipt.TargetLabel, StringComparer.Ordinal)
				.ThenBy(receipt => receipt.CommandName, StringComparer.Ordinal));
	}
}
