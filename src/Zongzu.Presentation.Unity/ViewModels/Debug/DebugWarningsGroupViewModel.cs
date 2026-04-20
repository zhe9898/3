using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity;

public sealed class DebugWarningsGroupViewModel
{
	public IReadOnlyList<string> Messages { get; set; } = Array.Empty<string>();

	public IReadOnlyList<string> Invariants { get; set; } = Array.Empty<string>();
}
