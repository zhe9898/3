using System;
using System.Collections.Generic;

namespace Zongzu.Presentation.Unity
{
public sealed class PersonDossierViewModel
{
	public int PersonId { get; set; }

	public string DisplayName { get; set; } = string.Empty;

	public string LifeStage { get; set; } = string.Empty;

	public string Gender { get; set; } = string.Empty;

	public bool IsAlive { get; set; }

	public string FidelityRing { get; set; } = string.Empty;

	public int? ClanId { get; set; }

	public string ClanName { get; set; } = string.Empty;

	public string BranchPositionLabel { get; set; } = string.Empty;

	public string KinshipSummary { get; set; } = string.Empty;

	public string TemperamentSummary { get; set; } = string.Empty;

	public string MemoryPressureSummary { get; set; } = string.Empty;

	public string CurrentStatusSummary { get; set; } = string.Empty;

	public IReadOnlyList<string> SourceModuleKeys { get; set; } = Array.Empty<string>();
}
}
