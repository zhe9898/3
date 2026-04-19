namespace Zongzu.Presentation.Unity;

public sealed class OfficeAppointmentViewModel
{
	public string DisplayName { get; set; } = string.Empty;

	public string OfficeTitle { get; set; } = string.Empty;

	public bool HasAppointment { get; set; }

	public int AuthorityTier { get; set; }

	public string ServiceSummary { get; set; } = string.Empty;

	public string TaskSummary { get; set; } = string.Empty;

	public string PetitionSummary { get; set; } = string.Empty;

	public string PressureSummary { get; set; } = string.Empty;

	public string PetitionOutcomeCategory { get; set; } = string.Empty;

	public string LastOutcome { get; set; } = string.Empty;

	public string LastPetitionOutcome { get; set; } = string.Empty;
}
