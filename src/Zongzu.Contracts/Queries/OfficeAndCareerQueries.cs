using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record OfficeCareerSnapshot
{
    public PersonId PersonId { get; init; }

    public ClanId ClanId { get; init; }

    public SettlementId SettlementId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public bool IsEligible { get; init; }

    public bool HasAppointment { get; init; }

    public string OfficeTitle { get; init; } = string.Empty;

    public int AuthorityTier { get; init; }

    public int AppointmentPressure { get; init; }

    public int ClerkDependence { get; init; }

    public int JurisdictionLeverage { get; init; }

    public int PetitionPressure { get; init; }

    public int PetitionBacklog { get; init; }

    public int ServiceMonths { get; init; }

    public int PromotionMomentum { get; init; }

    public int DemotionPressure { get; init; }

    public string CurrentAdministrativeTask { get; init; } = string.Empty;

    public string AdministrativeTaskTier { get; init; } = string.Empty;

    public int AdministrativeTaskLoad { get; init; }

    public int OfficeReputation { get; init; }

    public string LastOutcome { get; init; } = string.Empty;

    public string LastPetitionOutcome { get; init; } = string.Empty;

    public string PetitionOutcomeCategory { get; init; } = string.Empty;

    public string PromotionPressureLabel { get; init; } = string.Empty;

    public string DemotionPressureLabel { get; init; } = string.Empty;

    public string AuthorityTrajectorySummary { get; init; } = string.Empty;

    public string LastExplanation { get; init; } = string.Empty;
}

public sealed record JurisdictionAuthoritySnapshot
{
    public SettlementId SettlementId { get; init; }

    public PersonId? LeadOfficialPersonId { get; init; }

    public string LeadOfficialName { get; init; } = string.Empty;

    public string LeadOfficeTitle { get; init; } = string.Empty;

    public int AuthorityTier { get; init; }

    public int JurisdictionLeverage { get; init; }

    public int ClerkDependence { get; init; }

    public int PetitionPressure { get; init; }

    public int PetitionBacklog { get; init; }

    public string CurrentAdministrativeTask { get; init; } = string.Empty;

    public string AdministrativeTaskTier { get; init; } = string.Empty;

    public int AdministrativeTaskLoad { get; init; }

    public string LastPetitionOutcome { get; init; } = string.Empty;

    public string PetitionOutcomeCategory { get; init; } = string.Empty;

    public string LastAdministrativeTrace { get; init; } = string.Empty;
}

public interface IOfficeAndCareerQueries
{
    OfficeCareerSnapshot GetRequiredCareer(PersonId personId);

    IReadOnlyList<OfficeCareerSnapshot> GetCareers();

    JurisdictionAuthoritySnapshot GetRequiredJurisdiction(SettlementId settlementId);

    IReadOnlyList<JurisdictionAuthoritySnapshot> GetJurisdictions();

    // Phase 7 衙门骨骼 — LIVING_WORLD_DESIGN §2.7
    IReadOnlyList<OfficialPostSnapshot> GetOfficialPosts() => [];

    IReadOnlyList<WaitingListEntrySnapshot> GetWaitingList() => [];
}

public sealed record OfficialPostSnapshot
{
    public string PostId { get; init; } = string.Empty;

    public SettlementId Location { get; init; }

    public int Rank { get; init; }

    public string PostTitle { get; init; } = string.Empty;

    public PersonId? CurrentHolder { get; init; }

    public int VacancyMonths { get; init; }

    public int PetitionBacklog { get; init; }

    public int ClerkDependence { get; init; }

    public int EvaluationPressure { get; init; }
}

public sealed record WaitingListEntrySnapshot
{
    public PersonId PersonId { get; init; }

    public SettlementId SettlementId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public int QualificationTier { get; init; }

    public int WaitingMonths { get; init; }

    public int PatronageSupport { get; init; }
}
