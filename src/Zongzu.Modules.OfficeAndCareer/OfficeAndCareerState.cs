using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed class OfficeAndCareerState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.OfficeAndCareer;

    public List<OfficeCareerState> People { get; set; } = new();

    public List<JurisdictionAuthorityState> Jurisdictions { get; set; } = new();

    // Phase 7 衙门骨骼 — LIVING_WORLD_DESIGN §2.7
    public List<OfficialPostState> OfficialPosts { get; set; } = new();

    public List<WaitingListEntryState> WaitingList { get; set; } = new();
}

// Phase 7 衙门骨骼 — LIVING_WORLD_DESIGN §2.7。
// 官署一缺：由 postId 锚定，载 currentHolder / vacancyMonths 等空缺脉络。
public sealed class OfficialPostState
{
    public string PostId { get; set; } = string.Empty;

    public SettlementId Location { get; set; }

    public int Rank { get; set; }

    public string PostTitle { get; set; } = string.Empty;

    public PersonId? CurrentHolder { get; set; }

    public int VacancyMonths { get; set; }

    public int PetitionBacklog { get; set; }

    public int ClerkDependence { get; set; }

    public int EvaluationPressure { get; set; }
}

// Phase 7 候补：合格/可选而未授官者，按等次等候。
public sealed class WaitingListEntryState
{
    public PersonId PersonId { get; set; }

    public SettlementId SettlementId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public int QualificationTier { get; set; }

    public int WaitingMonths { get; set; }

    public int PatronageSupport { get; set; }
}

public sealed class OfficeCareerState
{
    public PersonId PersonId { get; set; }

    public ClanId ClanId { get; set; }

    public SettlementId SettlementId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsEligible { get; set; }

    public bool HasAppointment { get; set; }

    public string OfficeTitle { get; set; } = "未授官";

    public int AuthorityTier { get; set; }

    public int AppointmentPressure { get; set; }

    public int ClerkDependence { get; set; }

    public int JurisdictionLeverage { get; set; }

    public int PetitionPressure { get; set; }

    public int PetitionBacklog { get; set; }

    public int ServiceMonths { get; set; }

    public int PromotionMomentum { get; set; }

    public int DemotionPressure { get; set; }

    public string CurrentAdministrativeTask { get; set; } = "候补听选";

    public int AdministrativeTaskLoad { get; set; }

    public int OfficeReputation { get; set; }

    public string LastOutcome { get; set; } = "观望";

    public string LastPetitionOutcome { get; set; } = "未开案：暂无词牍。";

    public string LastExplanation { get; set; } = string.Empty;
}

public sealed class JurisdictionAuthorityState
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

    public int AdministrativeTaskLoad { get; set; }

    public string LastPetitionOutcome { get; set; } = string.Empty;

    public string LastAdministrativeTrace { get; set; } = string.Empty;
}
