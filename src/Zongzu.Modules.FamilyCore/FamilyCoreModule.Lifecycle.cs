using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed partial class FamilyCoreModule
{
    internal static void DispatchViolentDeathEvents(ModuleEventHandlingScope<FamilyCoreState> scope)
    {
        // Step 1b gap 2 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：死者生前 LifeStage / 家族身份（宗子 / 分支 / 旁系）；家族 prestige / prosperity 支撑力；
        // 战后恢复期（Warfare Phase == Aftermath）；既有 grudge 网络；季节带 / 治安。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType == DeathCauseEventNames.DeathByViolence)
            {
                // TODO Step 2: 按维度入口更新 MourningLoad / HeirSecurity 与发 ClanMemberDied / HeirSecurityWeakened。
            }
        }
    }

    internal static void DispatchTradeShockEvents(ModuleEventHandlingScope<FamilyCoreState> scope)
    {
        // Step 1b gap 1 — thin dispatch only. Intentionally no state change, no Emit, no diff.
        // 维度入口（Step 2 填规则时可吃）：违约方 clan prestige / prosperity / shame / branchTension；
        // 债务规模与家底比；两家 SocialMemory grudge；当地粮价 / 治安 / 季节带 / 徭役窗口。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case TradeShockEventTypes.RouteBusinessBlocked:
                case TradeShockEventTypes.TradeLossOccurred:
                case TradeShockEventTypes.TradeDebtDefaulted:
                case TradeShockEventTypes.TradeProspered:
                    // TODO Step 2: 按维度入口调整 clan Prestige / Shame / SupportReserve / BranchTension。
                    break;
            }
        }
    }

    /// <summary>
    /// STEP2A / A4 — 跨 clan 婚议通电。每月一次扫描所有 clan，按以下合意配对：
    /// 同聚落、双方 <c>MarriageAlliancePressure ≥ 55</c>、双方 <c>MourningLoad &lt; 18</c>、
    /// <c>|ΔPrestige| ≤ 30</c>、候选：未婚（<c>SpouseId == null</c>）+ 在世 +
    /// 16–30 岁 + 异性（依 PersonRegistry gender；未登记默认男），每对只配一次。
    ///
    /// <para>双边：各自 <c>MarriageAllianceValue</c> 至少拉到 55，<c>MarriageAlliancePressure -= 22</c>，
    /// <c>ReproductivePressure += 8</c>，<c>SupportReserve -= 2</c>（聘仪所费）；
    /// 写双向 <c>SpouseId</c>；发两次 <c>MarriageAllianceArranged</c>（以各自
    /// clan.Id 为 entity key）。未配上的 clan 下方循环走本族自议 fallback。</para>
    ///
    /// <para>skill <c>marriage-alliance-politics</c>：婚议是两族合意的联络，
    /// 本 step 不做聘礼 / 债务 / 政治操盘（后续 step 接）。</para>
    /// </summary>
    internal static void TryArrangeCrossClanMarriages(
        ModuleExecutionScope<FamilyCoreState> scope,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        const int MatchPressureThreshold = 55;
        const int MatchMourningCeiling = 18;
        const int PrestigeGapCeiling = 30;
        const int YouthMinAgeMonths = 16 * 12;
        const int YouthMaxAgeMonths = 30 * 12;

        ClanStateData[] clans = scope.State.Clans
            .Where(c => c.MarriageAlliancePressure >= MatchPressureThreshold
                        && c.MourningLoad < MatchMourningCeiling)
            .OrderBy(static c => c.Id.Value)
            .ToArray();
        if (clans.Length < 2) return;

        // 收集每个合格 clan 的未婚候选（按性别分桶）。
        Dictionary<ClanId, List<FamilyPersonState>> maleByClan = new();
        Dictionary<ClanId, List<FamilyPersonState>> femaleByClan = new();
        foreach (ClanStateData clan in clans)
        {
            List<FamilyPersonState> males = new();
            List<FamilyPersonState> females = new();
            foreach (FamilyPersonState person in scope.State.People.Where(p => p.ClanId == clan.Id))
            {
                if (person.SpouseId is not null) continue;
                if (!IsPersonAlive(person, registryQueries)) continue;
                int age = GetAgeMonths(person, registryQueries, currentDate);
                if (age < YouthMinAgeMonths || age > YouthMaxAgeMonths) continue;
                if (IsHeirEligibleGender(person.Id, registryQueries))
                {
                    males.Add(person);
                }
                else
                {
                    females.Add(person);
                }
            }
            if (males.Count > 0) maleByClan[clan.Id] = males;
            if (females.Count > 0) femaleByClan[clan.Id] = females;
        }

        // 配对：按 clan 对有序遍历（i<j），先吃一对合格男女则写定；已写入
        // 的 clan 本月不再配第二对（避免一月多婚与候选被重复选中）。
        HashSet<ClanId> matchedThisMonth = new();
        for (int i = 0; i < clans.Length; i++)
        {
            ClanStateData a = clans[i];
            if (matchedThisMonth.Contains(a.Id)) continue;
            for (int j = i + 1; j < clans.Length; j++)
            {
                ClanStateData b = clans[j];
                if (matchedThisMonth.Contains(b.Id)) continue;
                if (a.HomeSettlementId != b.HomeSettlementId) continue;
                if (Math.Abs(a.Prestige - b.Prestige) > PrestigeGapCeiling) continue;

                // 找一对：A-男 × B-女，或 A-女 × B-男。优先取最年长。
                FamilyPersonState? groom = null;
                FamilyPersonState? bride = null;
                if (maleByClan.TryGetValue(a.Id, out List<FamilyPersonState>? aMales)
                    && femaleByClan.TryGetValue(b.Id, out List<FamilyPersonState>? bFemales))
                {
                    groom = aMales.OrderByDescending(p => GetAgeMonths(p, registryQueries, currentDate)).First();
                    bride = bFemales.OrderByDescending(p => GetAgeMonths(p, registryQueries, currentDate)).First();
                }
                else if (femaleByClan.TryGetValue(a.Id, out List<FamilyPersonState>? aFemales)
                         && maleByClan.TryGetValue(b.Id, out List<FamilyPersonState>? bMales))
                {
                    bride = aFemales.OrderByDescending(p => GetAgeMonths(p, registryQueries, currentDate)).First();
                    groom = bMales.OrderByDescending(p => GetAgeMonths(p, registryQueries, currentDate)).First();
                }

                if (groom is null || bride is null) continue;

                // 写 spouse 双向 + 两边 clan 状态。
                groom.SpouseId = bride.Id;
                bride.SpouseId = groom.Id;

                ApplyCrossClanMarriageEffects(scope, a, groom, bride);
                ApplyCrossClanMarriageEffects(scope, b, bride, groom);

                matchedThisMonth.Add(a.Id);
                matchedThisMonth.Add(b.Id);
                break;
            }
        }
    }

    private static void ApplyCrossClanMarriageEffects(
        ModuleExecutionScope<FamilyCoreState> scope,
        ClanStateData clan,
        FamilyPersonState ownParty,
        FamilyPersonState otherParty)
    {
        clan.MarriageAllianceValue = Math.Max(clan.MarriageAllianceValue, 55);
        clan.MarriageAlliancePressure = Math.Max(0, clan.MarriageAlliancePressure - 22);
        clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 8, 0, 100);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - 2);
        clan.LastLifecycleOutcome = $"{clan.ClanName}与邻族结亲，婚议已定。";
        clan.LastLifecycleTrace = $"{ownParty.GivenName}与{otherParty.GivenName}议定姻亲，门墙之间先借此一络。";

        string message = $"{clan.ClanName}与邻族议定{ownParty.GivenName}之姻。";
        scope.RecordDiff(message, clan.Id.Value.ToString());
        scope.Emit(FamilyCoreEventNames.MarriageAllianceArranged, message, clan.Id.Value.ToString());
    }

    internal static bool TryArrangeAutonomousMarriage(ModuleExecutionScope<FamilyCoreState> scope, ClanStateData clan, FamilyMonthSignals signals)
    {
        if (signals.AdultCount == 0
            || clan.MarriageAllianceValue >= 45
            || clan.MarriageAlliancePressure < 72
            || clan.MourningLoad >= 18
            || clan.SupportReserve < 40)
        {
            return false;
        }

        clan.MarriageAllianceValue = 48;
        clan.MarriageAlliancePressure = Math.Max(0, clan.MarriageAlliancePressure - 18);
        clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 10, 0, 100);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - 3);
        clan.LastLifecycleOutcome = "门内先自议亲，暂把承祧与分房的后议压住。";
        clan.LastLifecycleTrace = $"{clan.ClanName}门内自行议定婚事，先借姻亲稳一稳香火与房支人情。";

        scope.RecordDiff($"{clan.ClanName}门内先自议亲，婚事已定，祠堂与家内都盼其稳住香火。", clan.Id.Value.ToString());
        scope.Emit(FamilyCoreEventNames.MarriageAllianceArranged, $"{clan.ClanName}门内议亲已定。", clan.Id.Value.ToString());
        return true;
    }

    internal static bool TryResolveClanBirth(ModuleExecutionScope<FamilyCoreState> scope, ClanStateData clan, FamilyMonthSignals signals)
    {
        if (signals.AdultCount == 0
            || clan.MarriageAllianceValue < 55
            || clan.ReproductivePressure < 52
            || clan.MourningLoad >= 18
            || signals.InfantCount > 0)
        {
            return false;
        }

        PersonId newbornId = KernelIdAllocator.NextPerson(scope.Context.KernelState);
        string newbornName = $"{clan.ClanName}新丁{newbornId.Value}";
        // Register identity first (PersonRegistry is authoritative for age and
        // IsAlive since Phase 2b). FamilyCore still owns clan-scoped kinship
        // and personality.
        IPersonRegistryCommands registryCommands = scope.GetRequiredQuery<IPersonRegistryCommands>();
        registryCommands.Register(
            scope.Context,
            newbornId,
            newbornName,
            scope.Context.CurrentDate,
            PersonGender.Unspecified,
            FidelityRing.Local);

        scope.State.People.Add(new FamilyPersonState
        {
            Id = newbornId,
            ClanId = clan.Id,
            GivenName = newbornName,
            AgeMonths = 0,
            IsAlive = true,
            BranchPosition = BranchPosition.DependentKin,
            Ambition = 50,
            Prudence = 50,
            Loyalty = 50,
            Sociability = 50,
        });

        clan.ReproductivePressure = Math.Max(0, clan.ReproductivePressure - 26);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - 4);
        clan.HeirSecurity = Math.Clamp(Math.Max(clan.HeirSecurity, 32) + 4, 0, 100);
        clan.LastLifecycleOutcome = "门内添丁，香火暂得续望，但家中口粮与抚养之费也随之加重。";
        clan.LastLifecycleTrace = $"{clan.ClanName}门内新添襁褓之儿，堂上暂缓继嗣焦心，家内却更添抚养之累。";

        scope.RecordDiff($"{clan.ClanName}门内添丁，襁褓初定，家中口粮、抚养与香火之望一并上肩。", clan.Id.Value.ToString());
        scope.Emit(FamilyCoreEventNames.BirthRegistered, $"{clan.ClanName}门内添丁。", clan.Id.Value.ToString());
        return true;
    }

    internal static bool TryResolveClanDeath(
        ModuleExecutionScope<FamilyCoreState> scope,
        ClanStateData clan,
        FamilyMonthSignals signals,
        IPersonRegistryQueries registryQueries)
    {
        // STEP2A / A1 — 按 FragilityLedger 选身亡者。ledger ≥ 100 表该人
        // 已过脆弱度阈。多人同时过阈时选 ledger 最高 → 最老 → 最小 Id
        // （纯确定性，不引 RNG）。skill disease-lifespan-death：老死是
        // 累积，不是悬崖；复合维度权重在 AccrueElderFragility 里计算。
        FamilyPersonAge? deathEntry = null;
        int bestLedger = -1;
        int bestAge = -1;
        foreach (FamilyPersonAge entry in signals.LivingPeople)
        {
            int ledger = entry.Person.FragilityLedger;
            if (ledger < 100)
            {
                continue;
            }

            if (ledger > bestLedger
                || (ledger == bestLedger && entry.AgeMonths > bestAge)
                || (ledger == bestLedger && entry.AgeMonths == bestAge && deathEntry is not null
                    && entry.Person.Id.Value < deathEntry.Value.Person.Id.Value))
            {
                deathEntry = entry;
                bestLedger = ledger;
                bestAge = entry.AgeMonths;
            }
        }

        if (deathEntry is null)
        {
            return false;
        }

        FamilyPersonState deathTarget = deathEntry.Value.Person;
        int deathAgeMonths = deathEntry.Value.AgeMonths;
        // PersonRegistry is the authoritative death write since Phase 2c/2d.
        // FamilyCore no longer writes the local IsAlive mirror; all readers
        // (simulation loop, snapshots, projections) consult registryQueries
        // first and fall back to the local flag only for unregistered persons.
        // See PERSON_OWNERSHIP_RULES.md.
        IPersonRegistryCommands registryCommands = scope.GetRequiredQuery<IPersonRegistryCommands>();
        registryCommands.MarkDeceased(scope.Context, deathTarget.Id);
        bool wasHeir = clan.HeirPersonId == deathTarget.Id;
        if (wasHeir)
        {
            clan.HeirPersonId = null;
        }

        clan.MourningLoad = Math.Clamp(clan.MourningLoad + 24, 0, 100);
        clan.InheritancePressure = Math.Clamp(clan.InheritancePressure + (wasHeir ? 18 : 8), 0, 100);
        clan.SeparationPressure = Math.Clamp(clan.SeparationPressure + (wasHeir ? 8 : 2), 0, 100);
        if (deathAgeMonths < AdultAgeMonths)
        {
            clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 10, 0, 100);
        }

        clan.LastLifecycleOutcome = wasHeir
            ? "承祧之人忽逝，举哀之外，又添后议与房支觊觎。"
            : "门内举哀，堂上先忙丧服与祭次，旁事都得让后。";
        clan.LastLifecycleTrace = wasHeir
            ? $"{clan.ClanName}失了承祧之人，香火、后议与房支人心一时俱紧。"
            : $"{clan.ClanName}门内有长者亡故，家中先忙丧服、发引与祭次。";

        scope.RecordDiff(
            wasHeir
                ? $"{clan.ClanName}承祧之人身故，门内举哀，继嗣之议与房支争执随即翻起。"
                : $"{clan.ClanName}门内举哀，丧服与祭次先压住家中诸事。",
            clan.Id.Value.ToString());
        // Entity key is PersonId so PersonRegistry can consolidate this into
        // the canonical PersonDeceased. See PERSON_OWNERSHIP_RULES.md.
        scope.Emit(FamilyCoreEventNames.ClanMemberDied, $"{clan.ClanName}门内举哀。", deathTarget.Id.Value.ToString());
        return true;
    }

    /// <summary>
    /// STEP2A / A1 — 每月给 clan 内 ≥55 岁在世者累积
    /// <see cref="FamilyPersonState.FragilityLedger"/>。复合维度（年龄带 × 养护 ×
    /// 聚落 HealerAccess × 季节 × 战后）纯确定性相加，不引新 RNG。ledger 到顶
    /// 不清零（保持已过阈的状态，由 <see cref="TryResolveClanDeath"/> 消费）。
    /// skill 铁律：治疗只降权重、不归零——<c>RemedyConfidence</c> 高时 -1，
    /// 不能一笔勾销老龄 dose。
    /// </summary>
    internal static void AccrueElderFragility(
        ClanStateData clan,
        FamilyMonthSignals signals,
        SettlementSnapshot homeSettlement,
        GameDate currentDate)
    {
        foreach (FamilyPersonAge entry in signals.LivingPeople)
        {
            int ageMonths = entry.AgeMonths;
            if (ageMonths < ElderAgeMonths)
            {
                continue;
            }

            int dose = ComputeFragilityDose(ageMonths, clan, homeSettlement, currentDate);
            if (dose <= 0)
            {
                continue;
            }

            entry.Person.FragilityLedger = Math.Clamp(entry.Person.FragilityLedger + dose, 0, 100);
        }
    }

    internal static int ComputeFragilityDose(
        int ageMonths,
        ClanStateData clan,
        SettlementSnapshot homeSettlement,
        GameDate currentDate)
    {
        // 年龄带基底（believable band，不拍概率）。
        int dose;
        if (ageMonths >= 90 * 12)
        {
            // 期颐之年，本月必到顶——仍走 ledger 通道以便诊断可见。
            return 100;
        }
        else if (ageMonths >= 85 * 12)
        {
            dose = 20;
        }
        else if (ageMonths >= 75 * 12)
        {
            dose = 8;
        }
        else if (ageMonths >= 65 * 12)
        {
            dose = 3;
        }
        else
        {
            dose = 1; // 55–64 岁
        }

        // clan 养护侧：支匮、哀重、照料已满的户，老人更脆。
        if (clan.SupportReserve < 30) dose += 2;
        else if (clan.SupportReserve < 50) dose += 1;

        if (clan.MourningLoad >= 50) dose += 2;
        else if (clan.MourningLoad >= 25) dose += 1;

        if (clan.CareLoad >= 60) dose += 2;
        else if (clan.CareLoad >= 30) dose += 1;

        // 聚落侧：秩序与医者可及性。
        if (homeSettlement.Security < 40) dose += 2;

        switch (homeSettlement.HealerAccess)
        {
            case HealerAccess.None: dose += 2; break;
            case HealerAccess.Itinerant: dose += 1; break;
            case HealerAccess.Renowned: dose = Math.Max(0, dose - 1); break;
            default: break; // Local 基线
        }

        // 季节：寒冬对 ≥65 岁尤甚（农历十一/十二/正）。
        if (ageMonths >= 65 * 12)
        {
            int m = currentDate.Month;
            if (m == 11 || m == 12 || m == 1) dose += 2;
        }

        // 求医信心高则略降权重（-1，不归零）。
        if (clan.RemedyConfidence >= 60) dose = Math.Max(0, dose - 1);

        return dose;
    }

    /// <summary>
    /// STEP2A / A3 — Heir 自动指派 / 递补。每月 RunMonth 在死亡结算后调用：
    /// 若 <c>HeirPersonId</c> 空缺或指向已亡者，从在世族人中按 "male → 最年长
    /// → 最小 Id" 选位（近 primogeniture，不做过继 / 旁支 / 收养——那是后
    /// 续 step）。选中者标为 <see cref="BranchPosition.MainLineHeir"/>，
    /// 原在世旧 heir 降为 <c>BranchMember</c>。根据本月是否刚死 heir
    /// 发 <c>HeirSuccessionOccurred</c> 或 <c>HeirAppointed</c>。
    /// </summary>
    internal static void TryReappointHeir(
        ModuleExecutionScope<FamilyCoreState> scope,
        ClanStateData clan,
        FamilyMonthSignals signals,
        IPersonRegistryQueries registryQueries,
        bool previousHeirDiedThisMonth)
    {
        PersonId? oldHeirId = clan.HeirPersonId;
        bool oldHeirAlive = oldHeirId is not null
            && signals.LivingHeir is not null
            && signals.LivingHeir.Value.Person.Id == oldHeirId.Value;

        if (oldHeirId is not null && oldHeirAlive)
        {
            return;
        }

        FamilyPersonAge? pick = null;
        foreach (FamilyPersonAge entry in signals.LivingPeople)
        {
            if (entry.AgeMonths < AdultAgeMonths) continue;
            if (!IsHeirEligibleGender(entry.Person.Id, registryQueries)) continue;
            if (pick is null
                || entry.AgeMonths > pick.Value.AgeMonths
                || (entry.AgeMonths == pick.Value.AgeMonths && entry.Person.Id.Value < pick.Value.Person.Id.Value))
            {
                pick = entry;
            }
        }

        if (pick is null)
        {
            clan.HeirPersonId = null;
            return;
        }

        PersonId newHeirId = pick.Value.Person.Id;
        if (oldHeirId is not null && oldHeirId.Value == newHeirId)
        {
            return;
        }

        if (oldHeirId is not null && oldHeirAlive)
        {
            FamilyPersonState? oldHeir = scope.State.People.FirstOrDefault(p => p.Id == oldHeirId.Value);
            if (oldHeir is not null && oldHeir.BranchPosition == BranchPosition.MainLineHeir)
            {
                oldHeir.BranchPosition = BranchPosition.BranchMember;
            }
        }

        clan.HeirPersonId = newHeirId;
        pick.Value.Person.BranchPosition = BranchPosition.MainLineHeir;

        string eventName = previousHeirDiedThisMonth
            ? FamilyCoreEventNames.HeirSuccessionOccurred
            : FamilyCoreEventNames.HeirAppointed;
        string message = previousHeirDiedThisMonth
            ? $"{clan.ClanName}承祧转房，{pick.Value.Person.GivenName}承继宗祧。"
            : $"{clan.ClanName}立{pick.Value.Person.GivenName}为承祧之人。";
        scope.RecordDiff(message, clan.Id.Value.ToString());
        scope.Emit(eventName, message, newHeirId.Value.ToString());
    }

    private static bool IsHeirEligibleGender(PersonId id, IPersonRegistryQueries registryQueries)
    {
        if (!registryQueries.TryGetPerson(id, out PersonRecord record))
        {
            // 未登记者按传统默认（seed 的本族成员为男性）；A7+ 若加收养需重审。
            return true;
        }
        return record.Gender != PersonGender.Female;
    }
}
