namespace Zongzu.Presentation.Unity;

public sealed class FamilyConflictTileViewModel
{
	public string ClanName { get; set; } = string.Empty;

	public int Prestige { get; set; }

	public int SupportReserve { get; set; }

	public string ConflictSummary { get; set; } = string.Empty;

	public string MemorySummary { get; set; } = string.Empty;

	public string LifecycleSummary { get; set; } = string.Empty;

	public string LastOrderSummary { get; set; } = string.Empty;
}
