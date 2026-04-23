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
        // External violent deaths are already identity-owned by PersonRegistry.
        // FamilyCore only translates a clan member's death into lineage pressure.
        IPersonRegistryQueries? registryQueries = null;
        HashSet<PersonId> handledPeople = new();
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != DeathCauseEventNames.DeathByViolence)
            {
                continue;
            }

            if (!TryParsePersonId(domainEvent.EntityKey, out PersonId personId)
                || !handledPeople.Add(personId))
            {
                continue;
            }

            registryQueries ??= scope.GetRequiredQuery<IPersonRegistryQueries>();
            ApplyExternalViolentDeathPressure(scope, personId, registryQueries);
        }
    }

    private static void ApplyExternalViolentDeathPressure(
        ModuleEventHandlingScope<FamilyCoreState> scope,
        PersonId personId,
        IPersonRegistryQueries registryQueries)
    {
        FamilyPersonState? deathTarget = scope.State.People.FirstOrDefault(person => person.Id == personId);
        if (deathTarget is null)
        {
            return;
        }

        ClanStateData? clan = scope.State.Clans.FirstOrDefault(candidate => candidate.Id == deathTarget.ClanId);
        if (clan is null)
        {
            return;
        }

        GameDate currentDate = scope.Context.CurrentDate;
        FamilyMonthSignals signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
        int deathAgeMonths = GetAgeMonths(deathTarget, registryQueries, currentDate);
        bool wasHeir = clan.HeirPersonId == deathTarget.Id;

        if (!registryQueries.TryGetPerson(deathTarget.Id, out _))
        {
            deathTarget.IsAlive = false;
        }

        if (wasHeir)
        {
            clan.HeirPersonId = null;
        }

        FamilyDeathImpactProfile deathImpact = ComputeDeathImpactProfile(
            clan,
            signals,
            deathTarget,
            deathAgeMonths,
            wasHeir,
            registryQueries);
        ApplyDeathImpactProfile(clan, deathImpact);

        FamilyMonthSignals afterDeathSignals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
        clan.HeirSecurity = ComputeHeirSecurity(clan, afterDeathSignals);

        bool isChildDeath = deathAgeMonths < AdultAgeMonths;
        clan.LastLifecycleOutcome = wasHeir
            ? "承祧之人遭暴亡，门内先起举哀与继嗣后议。"
            : isChildDeath
                ? "门内幼者遭暴亡，丧服、惊惧与再育之议一并上肩。"
                : "门内遭暴亡，丧次、祭次与房支声气一时压上来。";
        clan.LastLifecycleTrace = wasHeir
            ? $"{clan.ClanName}按{deathImpact.PressureSummary}承受承祧人暴亡，香火、后议与房支人心一时俱紧。"
            : isChildDeath
                ? $"{clan.ClanName}按{deathImpact.PressureSummary}承受幼者横死，家中丧服、惊惧与再育焦心一并上肩。"
                : $"{clan.ClanName}按{deathImpact.PressureSummary}承受暴亡之事，家中先忙丧次、发引与祭次。";

        scope.RecordDiff(
            wasHeir
                ? $"{clan.ClanName}承祧之人遭暴亡，门内举哀，继嗣之议与房支争执随即翻起（{deathImpact.PressureSummary}）。"
                : isChildDeath
                    ? $"{clan.ClanName}门内幼者遭暴亡，丧服、惊惧与再育之议一并上肩（{deathImpact.PressureSummary}）。"
                    : $"{clan.ClanName}门内遭暴亡，丧服与祭次先压住家中诸事（{deathImpact.PressureSummary}）。",
            deathTarget.Id.Value.ToString());
    }

    private static bool TryParsePersonId(string? entityKey, out PersonId personId)
    {
        personId = default;
        if (string.IsNullOrWhiteSpace(entityKey))
        {
            return false;
        }

        if (!int.TryParse(entityKey, out int value))
        {
            return false;
        }

        personId = new PersonId(value);
        return true;
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
    internal static HashSet<ClanId> TryArrangeCrossClanMarriages(
        ModuleExecutionScope<FamilyCoreState> scope,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        const int MatchPressureThreshold = 55;
        const int MatchMourningCeiling = 18;
        const int PrestigeGapCeiling = 30;
        const int YouthMinAgeMonths = 16 * 12;
        const int YouthMaxAgeMonths = 30 * 12;

        HashSet<ClanId> matchedThisMonth = new();
        ClanStateData[] clans = scope.State.Clans
            .Where(c => c.MarriageAlliancePressure >= MatchPressureThreshold
                        && c.MourningLoad < MatchMourningCeiling)
            .OrderBy(static c => c.Id.Value)
            .ToArray();
        if (clans.Length < 2) return matchedThisMonth;

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

        return matchedThisMonth;
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

    internal static bool TryArrangeAutonomousMarriage(
        ModuleExecutionScope<FamilyCoreState> scope,
        ClanStateData clan,
        FamilyMonthSignals signals,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        if (signals.AdultCount == 0
            || clan.MarriageAllianceValue >= 45
            || clan.MarriageAlliancePressure < 72
            || clan.MourningLoad >= 18
            || clan.SupportReserve < 40)
        {
            return false;
        }

        FamilyPersonState? anchor = FindAutonomousMarriageAnchor(scope.State, clan.Id, registryQueries, currentDate);
        if (anchor is null)
        {
            return false;
        }

        FamilyPersonState? spouse = CreateAutonomousMarriageSpouse(scope, clan, anchor, registryQueries, currentDate);
        if (spouse is null)
        {
            return false;
        }

        anchor.SpouseId = spouse.Id;
        spouse.SpouseId = anchor.Id;

        clan.MarriageAllianceValue = 48;
        clan.MarriageAlliancePressure = Math.Max(0, clan.MarriageAlliancePressure - 18);
        clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 10, 0, 100);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - 3);
        clan.LastLifecycleOutcome = "门内先自议亲，暂把承祧与分房的后议压住。";
        clan.LastLifecycleTrace = $"{clan.ClanName}门内自行议定婚事，{anchor.GivenName}与{spouse.GivenName}已挂为夫妇，先借姻亲稳一稳香火与房支人情。";

        scope.RecordDiff($"{clan.ClanName}门内先自议亲，{anchor.GivenName}与{spouse.GivenName}婚事已定，祠堂与家内都盼其稳住香火。", clan.Id.Value.ToString());
        scope.Emit(FamilyCoreEventNames.MarriageAllianceArranged, $"{clan.ClanName}门内议亲已定。", clan.Id.Value.ToString());
        return true;
    }

    private static FamilyPersonState? FindAutonomousMarriageAnchor(
        FamilyCoreState state,
        ClanId clanId,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        foreach (FamilyPersonState person in state.People
                     .Where(p => p.ClanId == clanId)
                     .OrderByDescending(p => GetAgeMonths(p, registryQueries, currentDate))
                     .ThenBy(p => p.Id.Value))
        {
            if (person.SpouseId is not null) continue;
            if (!IsPersonAlive(person, registryQueries)) continue;

            int ageMonths = GetAgeMonths(person, registryQueries, currentDate);
            if (ageMonths < AdultAgeMonths || ageMonths > 45 * 12) continue;

            return person;
        }

        return null;
    }

    private static FamilyPersonState? CreateAutonomousMarriageSpouse(
        ModuleExecutionScope<FamilyCoreState> scope,
        ClanStateData clan,
        FamilyPersonState anchor,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        PersonId spouseId = AllocateUnusedPersonId(scope, registryQueries);
        bool anchorIsMale = IsHeirEligibleGender(anchor.Id, registryQueries);
        PersonGender spouseGender = anchorIsMale ? PersonGender.Female : PersonGender.Male;
        int anchorAgeMonths = GetAgeMonths(anchor, registryQueries, currentDate);
        int spouseAgeMonths = anchorIsMale
            ? Math.Clamp(anchorAgeMonths - 24, AdultAgeMonths, 32 * 12)
            : Math.Clamp(anchorAgeMonths + 24, AdultAgeMonths, 40 * 12);
        string spouseName = $"{clan.ClanName}姻亲{spouseId.Value}";

        IPersonRegistryCommands registryCommands = scope.GetRequiredQuery<IPersonRegistryCommands>();
        bool registered = registryCommands.Register(
            scope.Context,
            spouseId,
            spouseName,
            SubtractMonths(currentDate, spouseAgeMonths),
            spouseGender,
            FidelityRing.Local);
        if (!registered)
        {
            return null;
        }

        FamilyPersonState spouse = new()
        {
            Id = spouseId,
            ClanId = clan.Id,
            GivenName = spouseName,
            AgeMonths = spouseAgeMonths,
            IsAlive = true,
            BranchPosition = BranchPosition.DependentKin,
            Ambition = 50,
            Prudence = 50,
            Loyalty = 50,
            Sociability = 50,
        };
        scope.State.People.Add(spouse);
        return spouse;
    }

    private static GameDate SubtractMonths(GameDate currentDate, int months)
    {
        int absoluteMonth = (currentDate.Year * 12) + currentDate.Month - 1 - Math.Max(0, months);
        int year = absoluteMonth / 12;
        int month = (absoluteMonth % 12) + 1;
        return new GameDate(year, month);
    }

    private static PersonId AllocateUnusedPersonId(
        ModuleExecutionScope<FamilyCoreState> scope,
        IPersonRegistryQueries registryQueries)
    {
        PersonId id;
        do
        {
            id = KernelIdAllocator.NextPerson(scope.Context.KernelState);
        }
        while (scope.State.People.Any(person => person.Id == id)
               || registryQueries.TryGetPerson(id, out _));

        return id;
    }

    /// <summary>
    /// STEP2A / A6 — 生育链解卡。原 gate 同时要求
    /// <c>MarriageAllianceValue ≥ 55</c> 与 <c>ReproductivePressure ≥ 52</c>，
    /// 而这两个字段 seed 初值均为 0，四重合取一起卡死 → 10 年沙盘零新生儿。
    ///
    /// <para>新形状：<b>已婚成年夫妇存在 × 无在场婴孩 × SupportReserve 可撑 ×
    /// MourningLoad 未塌陷</b>。婚议是"合法可育"的结构锚（由 A4 / A4-kin 真实种
    /// <see cref="FamilyPersonState.SpouseId"/>），不再用 MarriageAllianceValue
    /// 的分值代理；ReproductivePressure 作为"再育焦虑"是死亡/结婚的下游产物，
    /// 不该反过来充当生育的前提。MourningLoad 放宽到 35（原 18 对老死后 12 个月
    /// 窗口过严），让老人去世后仍有合理怀胎窗。</para>
    ///
    /// <para>skill <c>fertility-demography-infant-mortality</c>：生育是"夫妇—家
    /// 内—口粮"的接力，不该塌成一个分值门槛。</para>
    /// </summary>
    internal static bool TryResolveClanBirth(ModuleExecutionScope<FamilyCoreState> scope, ClanStateData clan, FamilyMonthSignals signals, IPersonRegistryQueries registryQueries)
    {
        const int MourningCeiling = 35;
        const int SupportFloor = 25;

        if (signals.AdultCount == 0
            || clan.MourningLoad >= MourningCeiling
            || clan.SupportReserve < SupportFloor
            || signals.InfantCount > 0
            || !HasMarriedAdultCouple(scope.State, clan.Id, registryQueries, scope.Context.CurrentDate))
        {
            return false;
        }

        PersonId newbornId = AllocateUnusedPersonId(scope, registryQueries);
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

        // STEP2A / A4 补遗 — 新生儿挂父母关系。父取本 clan 在世已婚男性中最年长者，
        // 若其 SpouseId 在世则取为母；无已婚父时退而取本 clan 在世男性长者（保
        // 留父系血亲图谱），母留空。挂 ChildrenIds 以便后续过继 / 分房使用。
        FamilyPersonState newborn = scope.State.People[scope.State.People.Count - 1];
        LinkNewbornParents(scope.State, clan, newborn, registryQueries);

        clan.ReproductivePressure = Math.Max(0, clan.ReproductivePressure - 26);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - 4);
        clan.HeirSecurity = Math.Clamp(Math.Max(clan.HeirSecurity, 32) + 4, 0, 100);
        clan.CareLoad = Math.Clamp(clan.CareLoad + 8, 0, 100);
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

        // STEP2A / A5 — 按死者年龄分流死因事件 & 副作用权重。
        // 婴幼儿（<12 岁）走 DeathByIllness + 更重的 ReproductivePressure 与 MourningLoad
        // （skill fertility-demography-infant-mortality：婴儿死不塌成单一 sadness，
        // 应产出 fear/debt/ritual/inheritance anxiety，此处先在 clan 侧加压，
        // SocialMemory 侧再接 child_loss 记忆）；其余仍走 ClanMemberDied（A1 老死链）。
        bool isChildDeath = deathAgeMonths < AdultAgeMonths;

        FamilyDeathImpactProfile deathImpact = ComputeDeathImpactProfile(
            clan,
            signals,
            deathTarget,
            deathAgeMonths,
            wasHeir,
            registryQueries);
        ApplyDeathImpactProfile(clan, deathImpact);

        if (isChildDeath)
        {
            clan.LastLifecycleOutcome = "襁褓未立，堂上添了一分白头人送黑头人的凄惶。";
            clan.LastLifecycleTrace = $"{clan.ClanName}按{deathImpact.PressureSummary}承受幼儿夭折之痛，家中丧服之外又添再育之忧。";
        }
        else
        {
            clan.LastLifecycleOutcome = wasHeir
                ? "承祧之人忽逝，举哀之外，又添后议与房支觊觎。"
                : "门内举哀，堂上先忙丧服与祭次，旁事都得让后。";
            clan.LastLifecycleTrace = wasHeir
                ? $"{clan.ClanName}按{deathImpact.PressureSummary}承受承祧人身故，香火、后议与房支人心一时俱紧。"
                : $"{clan.ClanName}按{deathImpact.PressureSummary}举哀，家中先忙丧服、发引与祭次。";
        }

        scope.RecordDiff(
            isChildDeath
                ? $"{clan.ClanName}门内幼儿夭折，家中丧服与再育之议一并上肩（{deathImpact.PressureSummary}）。"
                : wasHeir
                    ? $"{clan.ClanName}承祧之人身故，门内举哀，继嗣之议与房支争执随即翻起（{deathImpact.PressureSummary}）。"
                    : $"{clan.ClanName}门内举哀，丧服与祭次先压住家中诸事（{deathImpact.PressureSummary}）。",
            clan.Id.Value.ToString());
        // Entity key is PersonId so PersonRegistry can consolidate this into
        // the canonical PersonDeceased. See PERSON_OWNERSHIP_RULES.md.
        if (isChildDeath)
        {
            scope.Emit(DeathCauseEventNames.DeathByIllness, $"{clan.ClanName}门内幼儿病殁。", deathTarget.Id.Value.ToString());
        }
        else
        {
            scope.Emit(FamilyCoreEventNames.ClanMemberDied, $"{clan.ClanName}门内举哀。", deathTarget.Id.Value.ToString());
        }
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
    /// STEP2A / A5 — 婴幼儿病殁风险带累积。走与 A1 老死同一条 <see cref="FamilyPersonState.FragilityLedger"/>
    /// 通道；不重拍新字段。累积到 100 由 <see cref="TryResolveClanDeath"/> 选中，随后
    /// 走 <c>DeathByIllness</c> 分流（见该函数）。
    ///
    /// <para>skill <c>fertility-demography-infant-mortality</c>：婴儿死亡不是单一 sadness，
    /// 应沉淀为 fear / ritual / inheritance anxiety。本函数只负责累压，副作用分布在
    /// 死亡结算（<c>MourningLoad</c>/<c>ReproductivePressure</c>）与 SocialMemory（child_loss 记忆）。</para>
    ///
    /// <para>年龄带（0 – &lt;144 月）：Infant（&lt;24）最高；Young Child（24–59）中；
    /// Child（60–143）只在家内 / 聚落压力真起来时才累。Youth 以上由 A1 接管或不累。</para>
    /// </summary>
    internal static void AccrueChildFragility(
        ClanStateData clan,
        FamilyMonthSignals signals,
        SettlementSnapshot homeSettlement,
        GameDate currentDate)
    {
        foreach (FamilyPersonAge entry in signals.LivingPeople)
        {
            int ageMonths = entry.AgeMonths;
            if (ageMonths >= AdultAgeMonths)
            {
                continue;
            }

            int dose = ComputeChildFragilityDose(ageMonths, clan, homeSettlement, currentDate);
            if (dose <= 0)
            {
                continue;
            }

            entry.Person.FragilityLedger = Math.Clamp(entry.Person.FragilityLedger + dose, 0, 100);
        }
    }

    internal static int ComputeChildFragilityDose(
        int ageMonths,
        ClanStateData clan,
        SettlementSnapshot homeSettlement,
        GameDate currentDate)
    {
        // 年龄带基底（believable band，不拍概率）。Infant 最脆；越大越不依赖基线。
        int dose;
        bool isInfant;
        if (ageMonths < 24)
        {
            dose = 2;
            isInfant = true;
        }
        else if (ageMonths < 60)
        {
            dose = 1;
            isInfant = false;
        }
        else
        {
            dose = 0;
            isInfant = false;
        }

        // 冬寒对襁褓尤甚（农历十一/十二/正）。
        int month = currentDate.Month;
        bool isWinter = month == 11 || month == 12 || month == 1;
        if (isWinter)
        {
            dose += isInfant ? 2 : ageMonths < 60 ? 1 : 0;
        }

        // clan 养护侧：照料已满的户新婴最先崩；支匮喂养不继；哀重家内人心无余力。
        if (clan.CareLoad >= 50) dose += 1;
        if (clan.SupportReserve < 40) dose += 1;
        if (clan.MourningLoad >= 50) dose += 1;

        // 聚落侧：治安差则疫疠多（秩序差 = 水陆 / 街巷腌臜）。
        if (homeSettlement.Security < 40) dose += 1;

        // HealerAccess：对婴儿权重比成人更重。
        switch (homeSettlement.HealerAccess)
        {
            case HealerAccess.None: dose += isInfant ? 2 : 1; break;
            case HealerAccess.Itinerant: dose += 1; break;
            case HealerAccess.Renowned: dose = Math.Max(0, dose - 1); break;
            default: break; // Local 基线
        }

        // 求医信心高略降权重（-1，不归零；skill 铁律：治疗不是魔法）。
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

    /// <summary>
    /// STEP2A / A6 — 生育 gate 前置：本 clan 是否存在一对"已婚成年夫妇"。
    /// 仅要求一方：本 clan 内、在世、成年、<see cref="FamilyPersonState.SpouseId"/>
    /// 挂有目标；配偶可在任一 clan（嫁入 / 嫁出）但需在世。
    /// </summary>
    private static bool HasMarriedAdultCouple(
        FamilyCoreState state,
        ClanId clanId,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        foreach (FamilyPersonState person in state.People)
        {
            if (person.ClanId != clanId) continue;
            if (person.SpouseId is null) continue;
            if (!IsPersonAlive(person, registryQueries)) continue;
            int age = GetAgeMonths(person, registryQueries, currentDate);
            if (age < AdultAgeMonths) continue;

            FamilyPersonState? spouse = state.People.FirstOrDefault(p => p.Id == person.SpouseId.Value);
            if (spouse is null || !IsPersonAlive(spouse, registryQueries)) continue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// STEP2A / A4 补遗 — 新生儿父母挂载。取本 clan 在世已婚男性中最年长者
    /// 为父，其 <c>SpouseId</c>（在任一 clan、在世）为母；若无已婚父，退取本
    /// clan 在世男性长者。同时把新生儿加入父母的 <see cref="FamilyPersonState.ChildrenIds"/>。
    /// </summary>
    private static void LinkNewbornParents(
        FamilyCoreState state,
        ClanStateData clan,
        FamilyPersonState newborn,
        IPersonRegistryQueries registryQueries)
    {
        FamilyPersonState? father = null;
        FamilyPersonState? fatherFallback = null;
        foreach (FamilyPersonState person in state.People
                     .Where(p => p.ClanId == clan.Id && p.Id != newborn.Id)
                     .OrderByDescending(p => p.AgeMonths)
                     .ThenBy(p => p.Id.Value))
        {
            if (!IsPersonAlive(person, registryQueries)) continue;
            if (!IsHeirEligibleGender(person.Id, registryQueries)) continue;
            if (person.SpouseId is not null && father is null)
            {
                father = person;
            }
            fatherFallback ??= person;
        }

        father ??= fatherFallback;
        if (father is null) return;

        newborn.FatherId = father.Id;
        if (!father.ChildrenIds.Contains(newborn.Id))
        {
            father.ChildrenIds.Add(newborn.Id);
        }

        if (father.SpouseId is null) return;

        FamilyPersonState? mother = state.People.FirstOrDefault(p => p.Id == father.SpouseId.Value);
        if (mother is null || !IsPersonAlive(mother, registryQueries)) return;

        newborn.MotherId = mother.Id;
        if (!mother.ChildrenIds.Contains(newborn.Id))
        {
            mother.ChildrenIds.Add(newborn.Id);
        }
    }
}
