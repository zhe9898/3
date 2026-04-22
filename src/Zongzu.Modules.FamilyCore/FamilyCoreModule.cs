using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed partial class FamilyCoreModule : ModuleRunner<FamilyCoreState>
{
    internal const int AdultAgeMonths = 16 * 12;
    internal const int SecureHeirAgeMonths = 20 * 12;
    internal const int ElderAgeMonths = 55 * 12;
    // STEP2A / A1 之后老死走累积风险带（FragilityLedger），不再有固定悬崖。
    // DeathAgeMonths 常量废止，保留 AccrueElderFragility 的 90 岁天花板。
    internal const int InfantAgeMonths = 2 * 12;

    private static readonly string[] CommandNames =
    [
        PlayerCommandNames.ArrangeMarriage,
        PlayerCommandNames.DesignateHeirPolicy,
        PlayerCommandNames.SupportSeniorBranch,
        PlayerCommandNames.OrderFormalApology,
        PlayerCommandNames.PermitBranchSeparation,
        PlayerCommandNames.SuspendClanRelief,
        PlayerCommandNames.InviteClanEldersMediation,
        PlayerCommandNames.InviteClanEldersPubliclyBroker,
    ];

    private static readonly string[] EventNames =
    [
        FamilyCoreEventNames.ClanPrestigeAdjusted,
        FamilyCoreEventNames.FamilyMembersAged,
        FamilyCoreEventNames.LineageDisputeHardened,
        FamilyCoreEventNames.LineageMediationOpened,
        FamilyCoreEventNames.BranchSeparationApproved,
        FamilyCoreEventNames.MarriageAllianceArranged,
        FamilyCoreEventNames.BirthRegistered,
        FamilyCoreEventNames.ClanMemberDied,
        FamilyCoreEventNames.HeirSecurityWeakened,
        FamilyCoreEventNames.HeirAppointed,
        FamilyCoreEventNames.HeirSuccessionOccurred,
        // STEP2A / A5 — 婴幼儿夭折走 DeathByIllness 分流（= DeathCauseEventNames.DeathByIllness）。
        DeathCauseEventNames.DeathByIllness,
        // STEP2A / A7 — Youth → Adult 跨阈。FamilyCore 自用：推 SeparationPressure / 开启婚议候选。
        FamilyCoreEventNames.CameOfAge,
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
        // Step 1b gap 1: trade shock → clan pressure (no-op dispatch, rule density 留给 Step 2)
        TradeShockEventTypes.RouteBusinessBlocked,
        TradeShockEventTypes.TradeLossOccurred,
        TradeShockEventTypes.TradeDebtDefaulted,
        TradeShockEventTypes.TradeProspered,
        // Step 1b gap 2: violent death → clan mourning / heir security / grudge (no-op dispatch)
        DeathCauseEventNames.DeathByViolence,
    ];

    public override string ModuleKey => KnownModuleKeys.FamilyCore;

    public override int ModuleSchemaVersion => 7;

    public override SimulationPhase Phase => SimulationPhase.FamilyStructure;

    public override int ExecutionOrder => 300;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override FamilyCoreState CreateInitialState()
    {
        return new FamilyCoreState();
    }

    public override void RegisterQueries(FamilyCoreState state, QueryRegistry queries)
    {
        queries.Register<IFamilyCoreQueries>(new FamilyCoreQueries(state, queries));
    }

    public override void RunXun(ModuleExecutionScope<FamilyCoreState> scope)
    {
        IWorldSettlementsQueries settlementsQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPersonRegistryQueries registryQueries = scope.GetRequiredQuery<IPersonRegistryQueries>();
        GameDate currentDate = scope.Context.CurrentDate;

        foreach (ClanStateData clan in scope.State.Clans.OrderBy(static clan => clan.Id.Value))
        {
            SettlementSnapshot homeSettlement = settlementsQueries.GetRequiredSettlement(clan.HomeSettlementId);
            FamilyMonthSignals signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
            ApplyXunClanPulse(scope.Context.CurrentXun, clan, signals, homeSettlement);
        }
    }

    public override void RunMonth(ModuleExecutionScope<FamilyCoreState> scope)
    {
        IWorldSettlementsQueries settlementsQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPersonRegistryQueries registryQueries = scope.GetRequiredQuery<IPersonRegistryQueries>();
        GameDate currentDate = scope.Context.CurrentDate;

        // Age progression for persons not yet registered in PersonRegistry
        // (clan-internal mirror). Registered persons are aged by PersonRegistry
        // in Phase 0 and FamilyCore reads via IPersonRegistryQueries.GetAgeMonths.
        // See PERSON_OWNERSHIP_RULES.md — authority flows from PersonRegistry.
        int peopleAged = 0;
        foreach (FamilyPersonState person in scope.State.People.OrderBy(static person => person.Id.Value))
        {
            if (!IsPersonAlive(person, registryQueries))
            {
                continue;
            }

            int ageBefore = GetAgeMonths(person, registryQueries, currentDate);

            if (registryQueries.TryGetPerson(person.Id, out _))
            {
                // Registry is authoritative for this person; local mirror is unused for age.
                peopleAged += 1;
            }
            else
            {
                person.AgeMonths += 1;
                peopleAged += 1;
            }

            // STEP2A / A7 — Youth → Adult 跨阈：发 CameOfAge。两路径均已推进
            // 到本月龄：registry 权威者由 Phase 0 PersonRegistry 自增，本地镜像
            // 由上一句 += 1。故本月龄 == AdultAgeMonths 即为跨阈首帧。
            int ageAfter = registryQueries.TryGetPerson(person.Id, out _)
                ? ageBefore
                : person.AgeMonths;
            if (ageAfter == AdultAgeMonths)
            {
                scope.Emit(
                    FamilyCoreEventNames.CameOfAge,
                    $"{person.GivenName}行冠礼，堂上以成丁视之。",
                    person.Id.Value.ToString());
            }
        }

        if (peopleAged > 0)
        {
            scope.RecordDiff($"宗房册中在世之人本月俱各长一月，共{peopleAged}人。");
            scope.Emit(FamilyCoreEventNames.FamilyMembersAged, $"宗房人口本月俱各长一月，共{peopleAged}人。");
        }

        // STEP2A / A4 — 跨 clan 婚议通电。先做一轮跨族配对（同聚落 + 异性 +
        // 未婚 + 适婚窗 + 双方压力阈值达到），配成则双边各写一次
        // MarriageAllianceArranged；未配上的走本族自议 fallback（下方循环）。
        // skill marriage-alliance-politics：联姻由两族合意而成，不是单方
        // 意愿；本 step 不做聘礼 / 债务 / 政治操盘。
        TryArrangeCrossClanMarriages(scope, registryQueries, currentDate);

        foreach (ClanStateData clan in scope.State.Clans.OrderBy(static clan => clan.Id.Value))
        {
            SettlementSnapshot homeSettlement = settlementsQueries.GetRequiredSettlement(clan.HomeSettlementId);
            FamilyMonthSignals signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);

            if (TryArrangeAutonomousMarriage(scope, clan, signals))
            {
                signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
            }

            // STEP2A / A1 — 老死风险带累积（先累账本，再判身亡）。
            AccrueElderFragility(clan, signals, homeSettlement, currentDate);
            // STEP2A / A5 — 婴幼儿病殁风险带累积（与 A1 同通道；累到 100 走
            // DeathByIllness 分流，由 TryResolveClanDeath 按年龄分流发射）。
            AccrueChildFragility(clan, signals, homeSettlement, currentDate);
            bool hadDeathThisMonth = TryResolveClanDeath(scope, clan, signals, registryQueries);
            signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);

            // STEP2A / A3 — Heir 自动指派 / 递补。死亡之后立即补位，避免
            // HeirSecurity 的人为凹陷；初次沙盘里 HeirPersonId 为 null 的 clan
            // 也会被填上。skill lineage-inheritance：近 primogeniture，但本
            // step 不做过继 / 旁支孀 / 收养。
            TryReappointHeir(scope, clan, signals, registryQueries, hadDeathThisMonth);
            signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);

            bool hadBirthThisMonth = TryResolveClanBirth(scope, clan, signals, registryQueries);
            if (hadBirthThisMonth)
            {
                signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
            }

            int oldPrestige = clan.Prestige;
            int oldSupportReserve = clan.SupportReserve;
            int oldBranchTension = clan.BranchTension;
            int oldInheritancePressure = clan.InheritancePressure;
            int oldSeparationPressure = clan.SeparationPressure;
            int oldMediationMomentum = clan.MediationMomentum;
            int oldMarriageAlliancePressure = clan.MarriageAlliancePressure;
            int oldMarriageAllianceValue = clan.MarriageAllianceValue;
            int oldHeirSecurity = clan.HeirSecurity;
            int oldReproductivePressure = clan.ReproductivePressure;
            int oldMourningLoad = clan.MourningLoad;

            int pressureScore = homeSettlement.Security + homeSettlement.Prosperity;
            if (pressureScore >= 130)
            {
                clan.Prestige = Math.Min(100, clan.Prestige + 1);
            }
            else if (pressureScore < 95)
            {
                clan.Prestige = Math.Max(0, clan.Prestige - 1);
            }

            clan.SupportReserve = Math.Clamp(clan.SupportReserve + ((homeSettlement.Prosperity - 50) / 10), 0, 100);

            clan.MarriageAlliancePressure = Math.Clamp(
                clan.MarriageAlliancePressure + ComputeMarriageAlliancePressureDelta(clan, signals),
                0,
                100);
            clan.ReproductivePressure = Math.Clamp(
                clan.ReproductivePressure + ComputeReproductivePressureDelta(clan, signals),
                0,
                100);
            clan.HeirSecurity = ComputeHeirSecurity(clan, signals);
            clan.MourningLoad = Math.Max(0, clan.MourningLoad - (hadDeathThisMonth ? 0 : 1));

            clan.BranchTension = Math.Clamp(
                clan.BranchTension + ComputeBranchTensionDelta(clan, homeSettlement),
                0,
                100);
            clan.InheritancePressure = Math.Clamp(
                clan.InheritancePressure + ComputeInheritancePressureDelta(clan, signals),
                0,
                100);
            clan.SeparationPressure = Math.Clamp(
                clan.SeparationPressure
                    + ComputeSeparationPressureDelta(clan)
                    + ComputeAdultMaleCrowdingSeparationDelta(scope.State, clan, registryQueries, currentDate),
                0,
                100);
            clan.MediationMomentum = Math.Max(0, clan.MediationMomentum - 1);
            clan.BranchFavorPressure = Math.Max(0, clan.BranchFavorPressure - 1);
            clan.ReliefSanctionPressure = Math.Max(0, clan.ReliefSanctionPressure - 1);

            if (ClanStateUnchanged(
                    clan,
                    oldPrestige,
                    oldSupportReserve,
                    oldBranchTension,
                    oldInheritancePressure,
                    oldSeparationPressure,
                    oldMediationMomentum,
                    oldMarriageAlliancePressure,
                    oldMarriageAllianceValue,
                    oldHeirSecurity,
                    oldReproductivePressure,
                    oldMourningLoad))
            {
                continue;
            }

            scope.RecordDiff(
                $"{clan.ClanName}门望{clan.Prestige}，可支宗力{clan.SupportReserve}，婚议之压{clan.MarriageAlliancePressure}，承祧稳度{clan.HeirSecurity}，添丁之望{clan.ReproductivePressure}，丧服之重{clan.MourningLoad}，房支争力{clan.BranchTension}，分房之议{clan.SeparationPressure}。",
                clan.Id.Value.ToString());
            scope.Emit(FamilyCoreEventNames.ClanPrestigeAdjusted, $"{clan.ClanName}因门内盛衰与乡里形势而声势消长。", clan.Id.Value.ToString());

            if (oldHeirSecurity >= 40 && clan.HeirSecurity < 40)
            {
                scope.Emit(FamilyCoreEventNames.HeirSecurityWeakened, $"{clan.ClanName}承祧未稳，祠堂已起后议。", clan.Id.Value.ToString());
            }

            if (oldBranchTension < 55 && clan.BranchTension >= 55)
            {
                scope.Emit(FamilyCoreEventNames.LineageDisputeHardened, $"{clan.ClanName}祠堂争端渐炽。", clan.Id.Value.ToString());
            }

            if (oldMediationMomentum < 45 && clan.MediationMomentum >= 45)
            {
                scope.Emit(FamilyCoreEventNames.LineageMediationOpened, $"{clan.ClanName}已请族老开议调停。", clan.Id.Value.ToString());
            }

            if (oldSeparationPressure >= 55
                && clan.SeparationPressure <= 35
                && string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.PermitBranchSeparation, StringComparison.Ordinal))
            {
                scope.Emit(FamilyCoreEventNames.BranchSeparationApproved, $"{clan.ClanName}分房之议已定。", clan.Id.Value.ToString());
            }
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<FamilyCoreState> scope)
    {
        DispatchTradeShockEvents(scope);
        DispatchViolentDeathEvents(scope);

        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            ClanStateData[] clans = scope.State.Clans
                .Where(clan => clan.HomeSettlementId == bundle.SettlementId)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();
            if (clans.Length == 0)
            {
                continue;
            }

            int prestigeDelta = ComputeCampaignPrestigeDelta(bundle, campaign);
            int supportDelta = ComputeCampaignSupportDelta(bundle, campaign);

            foreach (ClanStateData clan in clans)
            {
                clan.Prestige = Math.Clamp(clan.Prestige + prestigeDelta, 0, 100);
                clan.SupportReserve = Math.Clamp(clan.SupportReserve + supportDelta, 0, 100);
            }

            scope.RecordDiff(
                $"{campaign.AnchorSettlementName}战后余波牵动宗房声势：门望{prestigeDelta:+#;-#;0}，宗力{supportDelta:+#;-#;0}。{campaign.LastAftermathSummary}",
                bundle.SettlementId.Value.ToString());
            scope.Emit(FamilyCoreEventNames.ClanPrestigeAdjusted, $"{campaign.AnchorSettlementName}战后余波改动了宗房声势。", bundle.SettlementId.Value.ToString());
        }
    }
}
