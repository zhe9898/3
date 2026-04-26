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
				FamilyLaneEntryReadbackSummary = command.FamilyLaneEntryReadbackSummary,
				FamilyElderExplanationReadbackSummary = command.FamilyElderExplanationReadbackSummary,
				FamilyGuaranteeReadbackSummary = command.FamilyGuaranteeReadbackSummary,
				FamilyHouseFaceReadbackSummary = command.FamilyHouseFaceReadbackSummary,
				FamilyLaneReceiptClosureSummary = command.FamilyLaneReceiptClosureSummary,
				FamilyLaneResidueFollowUpSummary = command.FamilyLaneResidueFollowUpSummary,
				FamilyLaneNoLoopGuardSummary = command.FamilyLaneNoLoopGuardSummary,
				WarfareLaneEntryReadbackSummary = command.WarfareLaneEntryReadbackSummary,
				ForceReadinessReadbackSummary = command.ForceReadinessReadbackSummary,
				CampaignAftermathReadbackSummary = command.CampaignAftermathReadbackSummary,
				WarfareLaneReceiptClosureSummary = command.WarfareLaneReceiptClosureSummary,
				WarfareLaneResidueFollowUpSummary = command.WarfareLaneResidueFollowUpSummary,
				WarfareLaneNoLoopGuardSummary = command.WarfareLaneNoLoopGuardSummary,
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
				ReadbackSummary = receipt.ReadbackSummary,
				FamilyLaneEntryReadbackSummary = receipt.FamilyLaneEntryReadbackSummary,
				FamilyElderExplanationReadbackSummary = receipt.FamilyElderExplanationReadbackSummary,
				FamilyGuaranteeReadbackSummary = receipt.FamilyGuaranteeReadbackSummary,
				FamilyHouseFaceReadbackSummary = receipt.FamilyHouseFaceReadbackSummary,
				FamilyLaneReceiptClosureSummary = receipt.FamilyLaneReceiptClosureSummary,
				FamilyLaneResidueFollowUpSummary = receipt.FamilyLaneResidueFollowUpSummary,
				FamilyLaneNoLoopGuardSummary = receipt.FamilyLaneNoLoopGuardSummary,
				WarfareLaneEntryReadbackSummary = receipt.WarfareLaneEntryReadbackSummary,
				ForceReadinessReadbackSummary = receipt.ForceReadinessReadbackSummary,
				CampaignAftermathReadbackSummary = receipt.CampaignAftermathReadbackSummary,
				WarfareLaneReceiptClosureSummary = receipt.WarfareLaneReceiptClosureSummary,
				WarfareLaneResidueFollowUpSummary = receipt.WarfareLaneResidueFollowUpSummary,
				WarfareLaneNoLoopGuardSummary = receipt.WarfareLaneNoLoopGuardSummary
			})
			.ToArray();
	}
}
