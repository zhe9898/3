using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity;

public sealed class DebugHotspotsGroupViewModel
{
	public IReadOnlyList<DebugHotspotViewModel> CurrentHotspots { get; set; } = Array.Empty<DebugHotspotViewModel>();

	public IReadOnlyList<DebugTraceItemViewModel> DiffTraces { get; set; } = Array.Empty<DebugTraceItemViewModel>();

	public IReadOnlyList<DebugEventItemViewModel> DomainEvents { get; set; } = Array.Empty<DebugEventItemViewModel>();
}
