namespace Zongzu.Contracts;

public static class FamilyCoreEventNames
{
    public const string ClanPrestigeAdjusted = "ClanPrestigeAdjusted";
    public const string FamilyMembersAged = "FamilyMembersAged";
    public const string LineageDisputeHardened = "LineageDisputeHardened";
    public const string LineageMediationOpened = "LineageMediationOpened";
    public const string BranchSeparationApproved = "BranchSeparationApproved";
    public const string MarriageAllianceArranged = "MarriageAllianceArranged";
    public const string BirthRegistered = "BirthRegistered";

    /// <summary>
    /// FamilyCore reports the death of a clan member. PersonRegistry consumes
    /// this together with other cause-specific death events (DeathByIllness
    /// from PopulationAndHouseholds, DeathByViolence from ConflictAndForce /
    /// OrderAndBanditry / WarfareCampaign) and emits the canonical
    /// <see cref="PersonRegistryEventNames.PersonDeceased"/>.
    /// Points at <see cref="DeathCauseEventNames.ClanMemberDied"/> so that
    /// the event name is defined in exactly one place.
    /// See <c>PERSON_OWNERSHIP_RULES.md</c>.
    /// </summary>
    public const string ClanMemberDied = DeathCauseEventNames.ClanMemberDied;

    public const string HeirSecurityWeakened = "HeirSecurityWeakened";
}
