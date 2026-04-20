using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class OfficeCareerSnapshot
{
    public PersonId PersonId { get; set; }

    public ClanId ClanId { get; set; }

    public SettlementId SettlementId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsEligible { get; set; }

    public bool HasAppointment { get; set; }

    public string OfficeTitle { get; set; } = string.Empty;

    public int AuthorityTier { get; set; }

    public int AppointmentPressure { get; set; }

    public int ClerkDependence { get; set; }

    public int JurisdictionLeverage { get; set; }

    public int PetitionPressure { get; set; }

    public int PetitionBacklog { get; set; }

    public int ServiceMonths { get; set; }

    public int PromotionMomentum { get; set; }

    public int DemotionPressure { get; set; }

    public string CurrentAdministrativeTask { get; set; } = string.Empty;

    public string AdministrativeTaskTier { get; set; } = string.Empty;

    public int AdministrativeTaskLoad { get; set; }

    public int OfficeReputation { get; set; }

    public string LastOutcome { get; set; } = string.Empty;

    public string LastPetitionOutcome { get; set; } = string.Empty;

    public string PetitionOutcomeCategory { get; set; } = string.Empty;

    public string PromotionPressureLabel { get; set; } = string.Empty;

    public string DemotionPressureLabel { get; set; } = string.Empty;

    public string AuthorityTrajectorySummary { get; set; } = string.Empty;

    public string LastExplanation { get; set; } = string.Empty;
}

public sealed class JurisdictionAuthoritySnapshot
{
    public SettlementId SettlementId { get; set; }

    public PersonId? LeadOfficialPersonId { get; set; }

    public string LeadOfficialName { get; set; } = string.Empty;

    public string LeadOfficeTitle { get; set; } = string.Empty;

    public int AuthorityTier { get; set; }

    public int JurisdictionLeverage { get; set; }

    public int ClerkDependence { get; set; }

    public int PetitionPressure { get; set; }

    public int PetitionBacklog { get; set; }

    public string CurrentAdministrativeTask { get; set; } = string.Empty;

    public string AdministrativeTaskTier { get; set; } = string.Empty;

    public int AdministrativeTaskLoad { get; set; }

    public string LastPetitionOutcome { get; set; } = string.Empty;

    public string PetitionOutcomeCategory { get; set; } = string.Empty;

    public string LastAdministrativeTrace { get; set; } = string.Empty;
}

public interface IOfficeAndCareerQueries
{
    OfficeCareerSnapshot GetRequiredCareer(PersonId personId);

    IReadOnlyList<OfficeCareerSnapshot> GetCareers();

    JurisdictionAuthoritySnapshot GetRequiredJurisdiction(SettlementId settlementId);

    IReadOnlyList<JurisdictionAuthoritySnapshot> GetJurisdictions();
}
