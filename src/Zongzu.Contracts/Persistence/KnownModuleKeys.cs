namespace Zongzu.Contracts;

public static class KnownModuleKeys
{
    /// <summary>
    /// Kernel-layer person identity anchor. Identity-only — see
    /// <c>PERSON_OWNERSHIP_RULES.md</c> and <c>MODULE_BOUNDARIES.md §0</c>.
    /// </summary>
    public const string PersonRegistry = "PersonRegistry";

    public const string WorldSettlements = "WorldSettlements";

    public const string FamilyCore = "FamilyCore";

    public const string PopulationAndHouseholds = "PopulationAndHouseholds";

    public const string SocialMemoryAndRelations = "SocialMemoryAndRelations";

    public const string EducationAndExams = "EducationAndExams";

    public const string TradeAndIndustry = "TradeAndIndustry";

    public const string PublicLifeAndRumor = "PublicLifeAndRumor";

    public const string OfficeAndCareer = "OfficeAndCareer";

    public const string NarrativeProjection = "NarrativeProjection";

    public const string OrderAndBanditry = "OrderAndBanditry";

    public const string ConflictAndForce = "ConflictAndForce";

    public const string WarfareCampaign = "WarfareCampaign";
}
