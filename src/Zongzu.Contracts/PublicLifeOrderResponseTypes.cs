namespace Zongzu.Contracts;

public static class PublicLifeOrderResponseOutcomeCodes
{
    public const string Repaired = "Repaired";
    public const string Contained = "Contained";
    public const string Escalated = "Escalated";
    public const string Ignored = "Ignored";
}

public static class PublicLifeOrderResponseTraceCodes
{
    public const string OrderWatchGuaranteeRepaired = "order.watch_guarantee_repaired";
    public const string OrderRunnerMisreadRepaired = "order.runner_misread_repaired";
    public const string OrderPressureContained = "order.pressure_contained";
    public const string OrderResponseIgnored = "order.response_ignored";
    public const string OfficeYamenLanded = "office.yamen_landed";
    public const string OfficeClerkDelayEscalated = "office.clerk_delay_escalated";
    public const string OfficeReportRerouted = "office.report_rerouted";
    public const string OfficeResponseIgnored = "office.response_ignored";
    public const string FamilyElderExplained = "family.elder_explained";
    public const string FamilyGuaranteeContained = "family.guarantee_contained";
    public const string FamilyResponseIgnored = "family.response_ignored";
    public const string OrderActorLocalWatchSelfSettled = "order.actor_local_watch_self_settled";
    public const string OrderActorRunnerMisreadHardened = "order.actor_runner_misread_hardened";
    public const string OfficeActorYamenQuietLanding = "office.actor_yamen_quiet_landing";
    public const string OfficeActorClerkDelayContinued = "office.actor_clerk_delay_continued";
    public const string FamilyActorElderQuietExplanation = "family.actor_elder_quiet_explanation";
    public const string FamilyActorGuaranteeAvoided = "family.actor_guarantee_avoided";
}
