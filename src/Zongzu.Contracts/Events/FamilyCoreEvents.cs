namespace Zongzu.Contracts;

public static class FamilyCoreEventNames
{
    /// <summary>
    /// Clan prestige or marriage value changed as a result of an upstream event
    /// (exam, warfare, marriage, etc.). Entity key: <see cref="ClanId"/>.
    /// <para><b>Design debt (P1-non-blocking)</b>: this event currently carries
    /// no structured payload (cause key, delta, prior value). Consumers must infer
    /// cause from <c>Summary</c> + <c>EntityKey</c>. Add typed payload or metadata
    /// bag on <see cref="IDomainEvent"/> once downstream needs to distinguish
    /// "exam prestige" from "war prestige" or "marriage prestige".</para>
    /// </summary>
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

    /// <summary>
    /// STEP2A / A3 — Heir appointed (first-time or after a vacancy). Signals
    /// that a clan that previously had no <c>HeirPersonId</c> now has one.
    /// Different from <see cref="HeirSuccessionOccurred"/>: this is a fill
    /// into an empty slot, not a transfer from a living heir.
    /// Entity key: the newly appointed <c>PersonId</c>.
    /// </summary>
    public const string HeirAppointed = "HeirAppointed";

    /// <summary>
    /// STEP2A / A3 — Heir succession transferred (the previous heir died
    /// and a replacement was picked up the same month). Used by SocialMemory
    /// / NarrativeProjection to distinguish orderly vacancy fills from true
    /// succession turbulence.
    /// Entity key: the newly appointed <c>PersonId</c>.
    /// </summary>
    public const string HeirSuccessionOccurred = "HeirSuccessionOccurred";

    /// <summary>
    /// STEP2A / A7 — 成年仪式（Youth → Adult 跨阈）。FamilyCore 自己消费
    /// 以推动 <c>SeparationPressure</c> / 婚议候选池；SocialMemory 可在后续
    /// step 接入以落轻量 narrative。Entity key: 当事人 <see cref="Zongzu.Contracts.PersonId"/>。
    /// 不由 PersonRegistry 出，因为它是 FamilyCore 对"家内权利—义务"跨阈的
    /// 族内诠释，不是身份登记的账实变化。
    /// </summary>
    public const string CameOfAge = "CameOfAge";

    /// <summary>
    /// Retainers mobilised by a clan for escort, local guard, or campaign-lite.
    /// Owned by FamilyCore because retainers are clan assets; ConflictAndForce
    /// consumes this to update force posture.
    /// </summary>
    public const string RetainerMobilized = "FamilyCore.RetainerMobilized";
}
