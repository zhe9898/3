using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class OfficeSurfaceViewModel
{
	public string Summary { get; set; } = string.Empty;

	public IReadOnlyList<CommandAffordanceViewModel> CommandAffordances { get; set; } = Array.Empty<CommandAffordanceViewModel>();

	public IReadOnlyList<CommandReceiptViewModel> RecentReceipts { get; set; } = Array.Empty<CommandReceiptViewModel>();

	public IReadOnlyList<OfficeAppointmentViewModel> Appointments { get; set; } = Array.Empty<OfficeAppointmentViewModel>();

	public IReadOnlyList<OfficeJurisdictionViewModel> Jurisdictions { get; set; } = Array.Empty<OfficeJurisdictionViewModel>();
}
}
