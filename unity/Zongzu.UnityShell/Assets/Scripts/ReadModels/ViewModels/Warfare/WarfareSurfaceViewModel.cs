using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class WarfareSurfaceViewModel
{
	public string Summary { get; set; } = string.Empty;

	public IReadOnlyList<CommandAffordanceViewModel> CommandAffordances { get; set; } = Array.Empty<CommandAffordanceViewModel>();

	public IReadOnlyList<CommandReceiptViewModel> RecentReceipts { get; set; } = Array.Empty<CommandReceiptViewModel>();

	public IReadOnlyList<CampaignBoardViewModel> CampaignBoards { get; set; } = Array.Empty<CampaignBoardViewModel>();

	public IReadOnlyList<CampaignMobilizationSignalViewModel> MobilizationSignals { get; set; } = Array.Empty<CampaignMobilizationSignalViewModel>();
}
}
