using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class FamilyCouncilViewModel
{
	public string Summary { get; set; } = string.Empty;

	public IReadOnlyList<CommandAffordanceViewModel> CommandAffordances { get; set; } = Array.Empty<CommandAffordanceViewModel>();

	public IReadOnlyList<CommandReceiptViewModel> RecentReceipts { get; set; } = Array.Empty<CommandReceiptViewModel>();

	public IReadOnlyList<FamilyConflictTileViewModel> Clans { get; set; } = Array.Empty<FamilyConflictTileViewModel>();
}
}
