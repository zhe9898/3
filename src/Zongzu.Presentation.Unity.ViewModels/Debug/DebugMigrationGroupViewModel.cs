using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class DebugMigrationGroupViewModel
{
	public string LoadOriginLabel { get; set; } = string.Empty;

	public string MigrationStatusLabel { get; set; } = string.Empty;

	public string MigrationSummary { get; set; } = string.Empty;

	public string MigrationConsistencySummary { get; set; } = string.Empty;

	public string MigrationStepCountLabel { get; set; } = string.Empty;

	public IReadOnlyList<string> MigrationSteps { get; set; } = Array.Empty<string>();
}
}
