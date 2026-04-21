using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public static class FamilyCoreStateProjection
{
    public static void UpgradeFromSchemaV1(FamilyCoreState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (ClanStateData clan in state.Clans)
        {
            clan.BranchTension = Math.Clamp(clan.BranchTension, 0, 100);
            clan.InheritancePressure = Math.Clamp(clan.InheritancePressure, 0, 100);
            clan.SeparationPressure = Math.Clamp(clan.SeparationPressure, 0, 100);
            clan.MediationMomentum = Math.Clamp(clan.MediationMomentum, 0, 100);
            clan.BranchFavorPressure = Math.Clamp(clan.BranchFavorPressure, 0, 100);
            clan.ReliefSanctionPressure = Math.Clamp(clan.ReliefSanctionPressure, 0, 100);
            clan.LastConflictCommandCode ??= string.Empty;
            clan.LastConflictCommandLabel ??= string.Empty;
            clan.LastConflictOutcome ??= string.Empty;
            clan.LastConflictTrace ??= string.Empty;
        }
    }

    public static void UpgradeFromSchemaV2ToV3(FamilyCoreState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (ClanStateData clan in state.Clans)
        {
            FamilyPersonState[] livingPeople = state.People
                .Where(person => person.ClanId == clan.Id && person.IsAlive)
                .OrderByDescending(static person => person.AgeMonths)
                .ThenBy(static person => person.Id.Value)
                .ToArray();

            FamilyPersonState? livingHeir = clan.HeirPersonId is null
                ? null
                : livingPeople.SingleOrDefault(person => person.Id == clan.HeirPersonId.Value);

            int adultCount = livingPeople.Count(static person => person.AgeMonths >= 16 * 12 && person.AgeMonths < 55 * 12);
            int childCount = livingPeople.Count(static person => person.AgeMonths < 16 * 12);
            int elderCount = livingPeople.Count(static person => person.AgeMonths >= 55 * 12);

            clan.MarriageAlliancePressure = Math.Clamp(
                adultCount <= 1
                    ? 54
                    : 34,
                0,
                100);
            clan.MarriageAllianceValue = 0;
            clan.HeirSecurity = InferHeirSecurity(livingHeir);
            clan.ReproductivePressure = Math.Clamp(
                childCount == 0
                    ? 42 + Math.Max(0, 1 - adultCount) * 8
                    : 22,
                0,
                100);
            clan.MourningLoad = elderCount > 0 ? 6 : 0;
            clan.LastLifecycleCommandCode ??= string.Empty;
            clan.LastLifecycleCommandLabel ??= string.Empty;
            clan.LastLifecycleOutcome ??= string.Empty;
            clan.LastLifecycleTrace ??= string.Empty;
        }
    }

    private static int InferHeirSecurity(FamilyPersonState? livingHeir)
    {
        if (livingHeir is null)
        {
            return 18;
        }

        if (livingHeir.AgeMonths >= 20 * 12)
        {
            return 68;
        }

        if (livingHeir.AgeMonths >= 12 * 12)
        {
            return 48;
        }

        return 28;
    }

    /// <summary>
    /// Phase 2a schema v3→v4：为旧存档补齐 FamilyPersonState 新字段。
    /// <list type="bullet">
    ///   <item>Heir → <see cref="BranchPosition.MainLineHeir"/>；其余在世成人 →
    ///         <see cref="BranchPosition.BranchMember"/>；在世幼童 →
    ///         <see cref="BranchPosition.DependentKin"/>。</item>
    ///   <item>性格四元组 Ambition/Prudence/Loyalty/Sociability 缺省 50。</item>
    ///   <item>ChildrenIds 保持空（血亲图谱在后续 phase 补）。</item>
    /// </list>
    /// </summary>
    public static void UpgradeFromSchemaV3ToV4(FamilyCoreState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        System.Collections.Generic.HashSet<PersonId> heirIds = state.Clans
            .Where(clan => clan.HeirPersonId is not null)
            .Select(clan => clan.HeirPersonId!.Value)
            .ToHashSet();

        foreach (FamilyPersonState person in state.People)
        {
            if (person.BranchPosition == BranchPosition.Unknown)
            {
                person.BranchPosition = heirIds.Contains(person.Id)
                    ? BranchPosition.MainLineHeir
                    : person.AgeMonths >= 16 * 12
                        ? BranchPosition.BranchMember
                        : BranchPosition.DependentKin;
            }

            if (person.Ambition == 0) person.Ambition = 50;
            if (person.Prudence == 0) person.Prudence = 50;
            if (person.Loyalty == 0) person.Loyalty = 50;
            if (person.Sociability == 0) person.Sociability = 50;

            person.ChildrenIds ??= new System.Collections.Generic.List<PersonId>();
        }
    }

    /// <summary>
    /// STEP2A / A0a — v4 → v5：家内照料 + 郎中药铺链字段入场。
    /// <list type="bullet">
    ///   <item><c>CareLoad</c>, <c>FuneralDebt</c> 默认 0（旧档没有葬债记账）。</item>
    ///   <item><c>RemedyConfidence</c> 按 <c>Prestige/4 + 30</c> 推断并夹 0–60；
    ///         旧档没有问医信心，但门望高的人家对请医更自信，给一点启动值。</item>
    /// </list>
    /// 本 step 不写规则；A1 老死风险带才会读 CareLoad/RemedyConfidence。
    /// </summary>
    public static void UpgradeFromSchemaV4ToV5(FamilyCoreState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (ClanStateData clan in state.Clans)
        {
            clan.CareLoad = Math.Clamp(clan.CareLoad, 0, 100);
            clan.FuneralDebt = Math.Clamp(clan.FuneralDebt, 0, 100);
            if (clan.RemedyConfidence <= 0)
            {
                clan.RemedyConfidence = Math.Clamp((clan.Prestige / 4) + 30, 0, 60);
            }
            else
            {
                clan.RemedyConfidence = Math.Clamp(clan.RemedyConfidence, 0, 100);
            }
        }
    }

    /// <summary>
    /// STEP2A / A0d — v5 → v6：宗族救济链字段入场（skill
    /// lineage-institutions-corporate-power）。救济是挑选性的，不是普惠，
    /// 债主是宗（clan），不是官。<c>CharityObligation</c> 0–100：宗房越殷实，
    /// 对"债主感"认知越重。旧档按 <c>SupportReserve/3 + 10</c> clamp 0–60
    /// 推断，避免同质化。本 step 不写规则。
    /// </summary>
    public static void UpgradeFromSchemaV5ToV6(FamilyCoreState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (ClanStateData clan in state.Clans)
        {
            if (clan.CharityObligation <= 0)
            {
                clan.CharityObligation = Math.Clamp((clan.SupportReserve / 3) + 10, 0, 60);
            }
            else
            {
                clan.CharityObligation = Math.Clamp(clan.CharityObligation, 0, 100);
            }
        }
    }
}
