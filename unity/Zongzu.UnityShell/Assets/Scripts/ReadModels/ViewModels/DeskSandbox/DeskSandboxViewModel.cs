using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class DeskSandboxViewModel
{
	public IReadOnlyList<SettlementNodeViewModel> Settlements { get; set; } = Array.Empty<SettlementNodeViewModel>();
}
}
