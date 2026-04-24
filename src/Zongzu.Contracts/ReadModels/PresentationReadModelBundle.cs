using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class PresentationReadModelBundle
{
    public GameDate CurrentDate { get; set; }

    public string ReplayHash { get; set; } = string.Empty;

    public IReadOnlyList<ClanSnapshot> Clans { get; set; } = [];

    public IReadOnlyList<ClanNarrativeSnapshot> ClanNarratives { get; set; } = [];

    public IReadOnlyList<PersonDossierSnapshot> PersonDossiers { get; set; } = [];

    public IReadOnlyList<SettlementSnapshot> Settlements { get; set; } = [];

    public IReadOnlyList<PopulationSettlementSnapshot> PopulationSettlements { get; set; } = [];

    public IReadOnlyList<HouseholdPressureSnapshot> Households { get; set; } = [];

    public IReadOnlyList<HouseholdSocialPressureSnapshot> HouseholdSocialPressures { get; set; } = [];

    public IReadOnlyList<EducationCandidateSnapshot> EducationCandidates { get; set; } = [];

    public IReadOnlyList<AcademySnapshot> Academies { get; set; } = [];

    public IReadOnlyList<ClanTradeSnapshot> ClanTrades { get; set; } = [];

    public IReadOnlyList<MarketSnapshot> Markets { get; set; } = [];

    public IReadOnlyList<ClanTradeRouteSnapshot> ClanTradeRoutes { get; set; } = [];

    public IReadOnlyList<SettlementPublicLifeSnapshot> PublicLifeSettlements { get; set; } = [];

    public IReadOnlyList<SettlementDisorderSnapshot> SettlementDisorder { get; set; } = [];

    public IReadOnlyList<OfficeCareerSnapshot> OfficeCareers { get; set; } = [];

    public IReadOnlyList<JurisdictionAuthoritySnapshot> OfficeJurisdictions { get; set; } = [];

    public IReadOnlyList<SettlementGovernanceLaneSnapshot> GovernanceSettlements { get; set; } = [];

    public GovernanceFocusSnapshot GovernanceFocus { get; set; } = new();

    public GovernanceDocketSnapshot GovernanceDocket { get; set; } = new();

    public HallDocketStackSnapshot HallDocket { get; set; } = new();

    public PlayerInfluenceFootprintSnapshot InfluenceFootprint { get; set; } = new();

    public IReadOnlyList<CampaignFrontSnapshot> Campaigns { get; set; } = [];

    public IReadOnlyList<CampaignMobilizationSignalSnapshot> CampaignMobilizationSignals { get; set; } = [];

    public IReadOnlyList<NarrativeNotificationSnapshot> Notifications { get; set; } = [];

    public PlayerCommandSurfaceSnapshot PlayerCommands { get; set; } = new();

    public PresentationDebugSnapshot Debug { get; set; } = new();
}
