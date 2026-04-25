using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class CommandShellAdapter
{
	internal static CommandAffordanceViewModel[] BuildAffordances(IEnumerable<PlayerCommandAffordanceSnapshot> affordances)
	{
		return affordances
			.Select(command => new CommandAffordanceViewModel
			{
				TargetLabel = command.TargetLabel,
				CommandName = command.CommandName,
				Label = command.Label,
				Summary = command.Summary,
				AvailabilitySummary = command.AvailabilitySummary,
				LeverageSummary = command.LeverageSummary,
				CostSummary = command.CostSummary,
				ReadbackSummary = command.ReadbackSummary,
				IsEnabled = command.IsEnabled
			})
			.ToArray();
	}

	internal static CommandReceiptViewModel[] BuildReceipts(IEnumerable<PlayerCommandReceiptSnapshot> receipts)
	{
		return receipts
			.Select(receipt => new CommandReceiptViewModel
			{
				TargetLabel = receipt.TargetLabel,
				CommandName = receipt.CommandName,
				Label = receipt.Label,
				Summary = receipt.Summary,
				OutcomeSummary = receipt.OutcomeSummary,
				LeverageSummary = receipt.LeverageSummary,
				CostSummary = receipt.CostSummary,
				ReadbackSummary = receipt.ReadbackSummary
			})
			.ToArray();
	}
}
