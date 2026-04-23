using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class DebugScaleGroupViewModel
{
	public DebugMetricSummaryViewModel LatestMetrics { get; set; } = new DebugMetricSummaryViewModel();

	public DebugScaleSummaryViewModel CurrentScale { get; set; } = new DebugScaleSummaryViewModel();

	public DebugPayloadSummaryViewModel PayloadSummary { get; set; } = new DebugPayloadSummaryViewModel();

	public IReadOnlyList<DebugPayloadFootprintViewModel> TopPayloadModules { get; set; } = Array.Empty<DebugPayloadFootprintViewModel>();

	public IReadOnlyList<string> EnabledModules { get; set; } = Array.Empty<string>();

	public IReadOnlyList<DebugModuleInspectorViewModel> ModuleInspectors { get; set; } = Array.Empty<DebugModuleInspectorViewModel>();
}
}
