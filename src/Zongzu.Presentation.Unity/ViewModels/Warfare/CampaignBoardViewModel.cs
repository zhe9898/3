using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity;

public sealed class CampaignBoardViewModel
{
	public string CampaignName { get; set; } = string.Empty;

	public string SettlementLabel { get; set; } = string.Empty;

	public string StatusLabel { get; set; } = string.Empty;

	public string RegionalProfileLabel { get; set; } = string.Empty;

	public string RegionalBackdropSummary { get; set; } = string.Empty;

	public string EnvironmentLabel { get; set; } = string.Empty;

	public string BoardSurfaceLabel { get; set; } = string.Empty;

	public string BoardAtmosphereSummary { get; set; } = string.Empty;

	public string MarkerSummary { get; set; } = string.Empty;

	public string FrontLabel { get; set; } = string.Empty;

	public string SupplyStateLabel { get; set; } = string.Empty;

	public string MoraleStateLabel { get; set; } = string.Empty;

	public string CommandFitLabel { get; set; } = string.Empty;

	public string DirectiveLabel { get; set; } = string.Empty;

	public string DirectiveSummary { get; set; } = string.Empty;

	public string DirectiveTrace { get; set; } = string.Empty;

	public string ObjectiveSummary { get; set; } = string.Empty;

	public string FrontSummary { get; set; } = string.Empty;

	public string MobilizationSummary { get; set; } = string.Empty;

	public string SupplyLineSummary { get; set; } = string.Empty;

	public string CoordinationSummary { get; set; } = string.Empty;

	public string CommanderSummary { get; set; } = string.Empty;

	public string AftermathSummary { get; set; } = string.Empty;

	public string AftermathDocketSummary { get; set; } = string.Empty;

	public IReadOnlyList<CampaignRouteViewModel> Routes { get; set; } = Array.Empty<CampaignRouteViewModel>();
}
