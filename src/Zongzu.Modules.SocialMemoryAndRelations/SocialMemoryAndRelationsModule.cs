using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed class SocialMemoryAndRelationsModule : ModuleRunner<SocialMemoryAndRelationsState>
{
    private static readonly string[] CommandNames =
    [
        "Apologize",
        "Compensate",
        "RestrainRetaliation",
        "PubliclyHonorOrShame",
    ];

    private static readonly string[] EventNames =
    [
        "GrudgeEscalated",
        "GrudgeSoftened",
        "FavorIncurred",
        "ClanNarrativeUpdated",
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
        // Step 1b gap 1: trade shock → clan narrative memory (no-op dispatch)
        TradeShockEventTypes.RouteBusinessBlocked,
        TradeShockEventTypes.TradeLossOccurred,
        TradeShockEventTypes.TradeDebtDefaulted,
        TradeShockEventTypes.TradeProspered,
        // Step 1b gap 2: violent death → cross-generation blood-feud memory (no-op dispatch)
        DeathCauseEventNames.DeathByViolence,
        // Step 1b gap 4: branch split / heir weakened → clan narrative (no-op dispatch)
        FamilyCoreEventNames.BranchSeparationApproved,
        FamilyCoreEventNames.HeirSecurityWeakened,
    ];

    private static class TradeShockEventTypes
    {
        public const string RouteBusinessBlocked = "RouteBusinessBlocked";
        public const string TradeLossOccurred = "TradeLossOccurred";
        public const string TradeDebtDefaulted = "TradeDebtDefaulted";
        public const string TradeProspered = "TradeProspered";
    }

    public override string ModuleKey => KnownModuleKeys.SocialMemoryAndRelations;

    public override int ModuleSchemaVersion => 2;

    public override SimulationPhase Phase => SimulationPhase.SocialMemory;

    public override int ExecutionOrder => 400;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override SocialMemoryAndRelationsState CreateInitialState()
    {
        return new SocialMemoryAndRelationsState();
    }

    public override void RegisterQueries(SocialMemoryAndRelationsState state, QueryRegistry queries)
    {
        queries.Register<ISocialMemoryAndRelationsQueries>(new SocialMemoryQueries(state));
    }

    public override void RunXun(ModuleExecutionScope<SocialMemoryAndRelationsState> scope)
    {
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        ITradeAndIndustryQueries? tradeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)
            ? scope.GetRequiredQuery<ITradeAndIndustryQueries>()
            : null;

        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();
        IReadOnlyList<HouseholdPressureSnapshot> households = populationQueries.GetHouseholds();
        Dictionary<ClanId, ClanTradeSnapshot> tradeByClan = tradeQueries is null
            ? new Dictionary<ClanId, ClanTradeSnapshot>()
            : tradeQueries.GetClanTrades().ToDictionary(static trade => trade.ClanId, static trade => trade);

        foreach (ClanSnapshot clan in clans.OrderBy(static clan => clan.Id.Value))
        {
            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clan.Id);
            HouseholdPressureSnapshot[] sponsoredHouseholds = households
                .Where(household => household.SponsorClanId == clan.Id)
                .OrderBy(household => household.Id.Value)
                .ToArray();
            int averageDistress = sponsoredHouseholds.Length == 0 ? 0 : sponsoredHouseholds.Sum(static household => household.Distress) / sponsoredHouseholds.Length;
            int averageMigrationRisk = sponsoredHouseholds.Length == 0 ? 0 : sponsoredHouseholds.Sum(static household => household.MigrationRisk) / sponsoredHouseholds.Length;
            bool hasMigratingHousehold = sponsoredHouseholds.Any(static household => household.IsMigrating);
            ClanTradeSnapshot? trade = tradeByClan.TryGetValue(clan.Id, out ClanTradeSnapshot? snapshot)
                ? snapshot
                : null;

            ApplyXunSocialPulse(
                scope.Context.CurrentXun,
                clan,
                averageDistress,
                averageMigrationRisk,
                hasMigratingHousehold,
                trade,
                narrative);
        }
    }

    public override void RunMonth(ModuleExecutionScope<SocialMemoryAndRelationsState> scope)
    {
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();

        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();
        IReadOnlyList<HouseholdPressureSnapshot> households = populationQueries.GetHouseholds();

        AdvanceMemoryLifecycle(scope.State);

        foreach (ClanSnapshot clan in clans.OrderBy(static clan => clan.Id.Value))
        {
            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clan.Id);
            HouseholdPressureSnapshot[] sponsoredHouseholds = households
                .Where(household => household.SponsorClanId == clan.Id)
                .OrderBy(household => household.Id.Value)
                .ToArray();

            int previousGrudge = narrative.GrudgePressure;
            int previousFavor = narrative.FavorBalance;

            int averageDistress = sponsoredHouseholds.Length == 0 ? 0 : sponsoredHouseholds.Sum(static household => household.Distress) / sponsoredHouseholds.Length;

            narrative.GrudgePressure = Math.Clamp(narrative.GrudgePressure + ComputeGrudgeDelta(clan, averageDistress), 0, 100);
            narrative.FearPressure = Math.Clamp(narrative.FearPressure + (averageDistress >= 70 ? 1 : averageDistress < 45 ? -1 : 0), 0, 100);
            narrative.ShamePressure = Math.Clamp(narrative.ShamePressure + ComputeShameDelta(clan), 0, 100);
            narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + ComputeFavorDelta(clan), -100, 100);
            narrative.PublicNarrative = BuildNarrativeText(clan, averageDistress, narrative.GrudgePressure);

            scope.RecordDiff(
                $"{clan.ClanName}宗中旧怨{narrative.GrudgePressure}，惧意{narrative.FearPressure}，羞压{narrative.ShamePressure}。",
                clan.Id.Value.ToString());

            if (previousGrudge < 60 && narrative.GrudgePressure >= 60)
            {
                scope.Emit("GrudgeEscalated", $"{clan.ClanName}旧怨更深。");
                AddMemory(scope.State, clan.Id, "hardship", MemoryType.Grudge, MemorySubtype.WealthGrudge, "clan.hardship", 2, narrative.GrudgePressure, true, $"{clan.ClanName}因民困与家计艰难，旧怨更深。", scope.Context);
            }

            if (previousGrudge >= 45 && narrative.GrudgePressure < 45)
            {
                scope.Emit("GrudgeSoftened", $"{clan.ClanName}旧怨稍缓。");
                AddMemory(scope.State, clan.Id, "conciliation", MemoryType.Favor, MemorySubtype.ReliefFavor, "clan.relief", 3, Math.Max(10, narrative.FavorBalance), true, $"{clan.ClanName}因接济与调处，怨气稍缓。", scope.Context);
            }

            if (previousFavor < 10 && narrative.FavorBalance >= 10)
            {
                scope.Emit("FavorIncurred", $"{clan.ClanName}人情债渐著。");
            }

            scope.Emit("ClanNarrativeUpdated", $"{clan.ClanName}乡议口气有变。");
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        DispatchTradeShockEvents(scope);
        DispatchViolentDeathEvents(scope);
        DispatchFamilyBranchEvents(scope);

        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        ClanSnapshot[] clans = familyQueries.GetClans()
            .OrderBy(static clan => clan.Id.Value)
            .ToArray();

        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            ClanSnapshot[] localClans = clans
                .Where(clan => clan.HomeSettlementId == bundle.SettlementId)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();

            foreach (ClanSnapshot clan in localClans)
            {
                ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clan.Id);
                int previousGrudge = narrative.GrudgePressure;

                narrative.FearPressure = Math.Clamp(narrative.FearPressure + ComputeCampaignFearDelta(bundle, campaign), 0, 100);
                narrative.GrudgePressure = Math.Clamp(narrative.GrudgePressure + ComputeCampaignGrudgeDelta(bundle, campaign), 0, 100);
                narrative.ShamePressure = Math.Clamp(narrative.ShamePressure + (bundle.CampaignAftermathRegistered ? 2 : 1), 0, 100);
                narrative.FavorBalance = Math.Clamp(narrative.FavorBalance - (bundle.CampaignSupplyStrained ? 2 : 1), -100, 100);
                narrative.PublicNarrative = BuildCampaignNarrativeText(clan, campaign, narrative);

                scope.RecordDiff(
                    $"{campaign.AnchorSettlementName}战后余波牵得{clan.ClanName}惧意{narrative.FearPressure}，旧怨{narrative.GrudgePressure}，羞压{narrative.ShamePressure}。",
                    clan.Id.Value.ToString());

                AddMemory(
                    scope.State,
                    clan.Id,
                    bundle.CampaignAftermathRegistered ? "campaign-aftermath" : "campaign-pressure",
                    MemoryType.Fear,
                    MemorySubtype.WarDread,
                    bundle.CampaignAftermathRegistered ? "campaign.aftermath" : "campaign.pressure",
                    bundle.CampaignAftermathRegistered ? 1 : 2,
                    Math.Max(narrative.FearPressure, narrative.GrudgePressure),
                    true,
                    $"{campaign.AnchorSettlementName}战后余波把{campaign.FrontLabel}与{campaign.LastAftermathSummary}一并压进了{clan.ClanName}的旧忆里。",
                    scope.Context);

                if (previousGrudge < 60 && narrative.GrudgePressure >= 60)
                {
                    scope.Emit("GrudgeEscalated", $"战后余波使{clan.ClanName}旧怨更炽。", clan.Id.Value.ToString());
                }
            }
        }
    }

    private static void DispatchViolentDeathEvents(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        // Step 1b gap 2 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：加害方 / 受害方所属 clan；两家既有 grudge / favor；事件规模（群殴/单伤/命案）；
        // 是否战时 / 战后余波；地方权威是否介入（Office AuthorityTier）。暴力致死在族叙事里应当比病死更
        // 容易沉淀为跨代血仇——Step 2 填函数形状。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType == DeathCauseEventNames.DeathByViolence)
            {
                // TODO Step 2: 更新 ClanNarrativeState / 跨代 grudge / FeudMemory。
            }
        }
    }

    private static void DispatchFamilyBranchEvents(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        // Step 1b gap 4 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：分家规模（主房 / 旁支）；分出方与原房既有 grudge / favor；分家是否伴随 shame；
        // 邻族是否围观；当地公议（PublicLife）热度。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case FamilyCoreEventNames.BranchSeparationApproved:
                case FamilyCoreEventNames.HeirSecurityWeakened:
                    // TODO Step 2: 更新 ClanNarrativeState / EventMemoryState。
                    break;
            }
        }
    }

    /// <summary>
    /// STEP2A / A4 — 跨 clan 婚议 → 双方 clan 的 <c>FavorBalance</c> 各加一跳；
    /// 若越过 10 的阈值发 <c>FavorIncurred</c>。婚议本身即人情债的生成事件。
    /// EntityKey 即本 clan 的 ClanId（FamilyCore 发 MarriageAllianceArranged 时
    /// 双方 clan 各发一条）。
    /// </summary>
    private static void DispatchMarriageAllianceEvents(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != FamilyCoreEventNames.MarriageAllianceArranged) continue;
            if (string.IsNullOrEmpty(domainEvent.EntityKey)) continue;
            if (!int.TryParse(domainEvent.EntityKey, out int clanIdValue)) continue;

            ClanId clanId = new(clanIdValue);
            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clanId);
            int previousFavor = narrative.FavorBalance;
            narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + 3, -100, 100);

            if (previousFavor < 10 && narrative.FavorBalance >= 10)
            {
                scope.Emit("FavorIncurred", $"{clanId.Value}族因议婚人情渐著。");
            }
        }
    }

    private static void DispatchTradeShockEvents(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        // Step 1b gap 1 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口（Step 2 可吃）：违约方与被违约方既有 grudge/favor；事件规模；当地粮价与治安；
        // 是否发生在灾害窗口；两家是否既有姻亲/世交。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case TradeShockEventTypes.RouteBusinessBlocked:
                case TradeShockEventTypes.TradeLossOccurred:
                case TradeShockEventTypes.TradeDebtDefaulted:
                case TradeShockEventTypes.TradeProspered:
                    // TODO Step 2: 按维度入口更新 ClanNarrativeState / EventMemoryState。
                    break;
            }
        }
    }

    private static void ApplyXunSocialPulse(
        SimulationXun currentXun,
        ClanSnapshot clan,
        int averageDistress,
        int averageMigrationRisk,
        bool hasMigratingHousehold,
        ClanTradeSnapshot? trade,
        ClanNarrativeState narrative)
    {
        switch (currentXun)
        {
            case SimulationXun.Shangxun:
            {
                int fearDelta = 0;
                fearDelta += averageDistress >= 70 ? 1 : averageDistress < 45 ? -1 : 0;
                fearDelta += hasMigratingHousehold ? 1 : 0;
                fearDelta -= clan.SupportReserve >= 65 ? 1 : 0;

                int grudgeDelta = 0;
                grudgeDelta += clan.BranchTension >= 60 ? 1 : clan.BranchTension < 35 ? -1 : 0;
                grudgeDelta += averageDistress >= 75 ? 1 : 0;
                grudgeDelta -= clan.MediationMomentum >= 55 ? 1 : 0;

                narrative.FearPressure = Math.Clamp(narrative.FearPressure + fearDelta, 0, 100);
                narrative.GrudgePressure = Math.Clamp(narrative.GrudgePressure + grudgeDelta, 0, 100);
                break;
            }
            case SimulationXun.Zhongxun:
            {
                int shameDelta = clan.Prestige < 45 ? 1 : clan.Prestige > 58 ? -1 : 0;
                shameDelta += clan.BranchFavorPressure >= 40 ? 1 : 0;
                shameDelta += trade is not null && trade.CommerceReputation < 25 ? 1 : 0;
                shameDelta += trade is not null && trade.Debt >= 90 ? 1 : 0;

                int favorDelta = clan.SupportReserve >= 65 ? 1 : clan.SupportReserve < 40 ? -1 : 0;
                favorDelta += clan.MediationMomentum >= 45 ? 1 : 0;
                favorDelta -= clan.ReliefSanctionPressure >= 40 ? 1 : 0;
                favorDelta += trade is not null && trade.CashReserve >= 100 && trade.CommerceReputation >= 35 ? 1 : 0;
                favorDelta -= averageDistress >= 70 && clan.SupportReserve < 40 ? 1 : 0;

                narrative.ShamePressure = Math.Clamp(narrative.ShamePressure + shameDelta, 0, 100);
                narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + favorDelta, -100, 100);
                break;
            }
            case SimulationXun.Xiaxun:
            {
                int grudgeDelta = 0;
                grudgeDelta += clan.SeparationPressure >= 60 ? 1 : 0;
                grudgeDelta += clan.InheritancePressure >= 55 ? 1 : 0;
                grudgeDelta -= clan.MediationMomentum >= 50 ? 1 : 0;

                int fearDelta = 0;
                fearDelta += averageMigrationRisk >= 60 ? 1 : 0;
                fearDelta += hasMigratingHousehold ? 1 : 0;
                fearDelta -= clan.SupportReserve >= 65 ? 1 : 0;

                int shameDelta = 0;
                shameDelta += clan.SeparationPressure >= 60 ? 1 : 0;
                shameDelta -= clan.MediationMomentum >= 50 ? 1 : 0;

                int favorDelta = 0;
                favorDelta += averageDistress >= 60 && clan.SupportReserve >= 65 ? 1 : 0;
                favorDelta -= averageMigrationRisk >= 60 && clan.SupportReserve < 40 ? 1 : 0;

                narrative.GrudgePressure = Math.Clamp(narrative.GrudgePressure + grudgeDelta, 0, 100);
                narrative.FearPressure = Math.Clamp(narrative.FearPressure + fearDelta, 0, 100);
                narrative.ShamePressure = Math.Clamp(narrative.ShamePressure + shameDelta, 0, 100);
                narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + favorDelta, -100, 100);
                break;
            }
        }
    }

    private static ClanNarrativeState GetOrCreateNarrative(SocialMemoryAndRelationsState state, ClanId clanId)
    {
        ClanNarrativeState? narrative = state.ClanNarratives.SingleOrDefault(existing => existing.ClanId == clanId);
        if (narrative is not null)
        {
            return narrative;
        }

        narrative = new ClanNarrativeState { ClanId = clanId };
        state.ClanNarratives.Add(narrative);
        state.ClanNarratives = state.ClanNarratives.OrderBy(static entry => entry.ClanId.Value).ToList();
        return narrative;
    }

    private static int ComputeGrudgeDelta(ClanSnapshot clan, int averageDistress)
    {
        int delta = 0;
        if (averageDistress >= 75)
        {
            delta += 2;
        }
        else if (averageDistress >= 55)
        {
            delta += 1;
        }
        else if (averageDistress < 40)
        {
            delta -= 1;
        }

        if (clan.Prestige < 45)
        {
            delta += 1;
        }

        if (clan.SupportReserve >= 70)
        {
            delta -= 1;
        }

        if (clan.BranchTension >= 60)
        {
            delta += 2;
        }
        else if (clan.BranchTension >= 40)
        {
            delta += 1;
        }

        if (clan.ReliefSanctionPressure >= 45)
        {
            delta += 1;
        }

        if (clan.MediationMomentum >= 55)
        {
            delta -= 2;
        }
        else if (clan.MediationMomentum >= 28)
        {
            delta -= 1;
        }

        return delta;
    }

    private static int ComputeShameDelta(ClanSnapshot clan)
    {
        int delta = clan.Prestige < 45 ? 1 : clan.Prestige > 58 ? -1 : 0;
        delta += clan.BranchFavorPressure >= 40 ? 1 : 0;
        delta += clan.SeparationPressure >= 60 ? 1 : 0;
        delta -= clan.MediationMomentum >= 50 ? 1 : 0;
        return delta;
    }

    private static int ComputeFavorDelta(ClanSnapshot clan)
    {
        int delta = clan.SupportReserve >= 65 ? 1 : clan.SupportReserve < 40 ? -1 : 0;
        delta += clan.MediationMomentum >= 45 ? 1 : 0;
        delta -= clan.ReliefSanctionPressure >= 40 ? 2 : 0;
        return delta;
    }

    private static int ComputeCampaignFearDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignPressureRaised ? 2 : 0;
        delta += bundle.CampaignSupplyStrained ? 3 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignGrudgeDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignPressureRaised ? 1 : 0;
        delta += bundle.CampaignSupplyStrained ? 2 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, 45 - campaign.MoraleState) / 14;
        return Math.Max(1, delta);
    }

    private static string BuildCampaignNarrativeText(ClanSnapshot clan, CampaignFrontSnapshot campaign, ClanNarrativeState narrative)
    {
        if (narrative.GrudgePressure >= 70)
        {
            return $"{campaign.AnchorSettlementName}兵后余波未歇，{clan.ClanName}宗中怨望愈深。";
        }

        if (narrative.FearPressure >= 60)
        {
            return $"{campaign.AnchorSettlementName}前线余压未退，{clan.ClanName}宗中人心惴惴。";
        }

        return $"{clan.ClanName}仍记得{campaign.AnchorSettlementName}战后诸案与里中劳瘁。";
    }

    private static string BuildNarrativeText(ClanSnapshot clan, int averageDistress, int grudgePressure)
    {
        if (clan.SeparationPressure >= 65)
        {
            return $"{clan.ClanName}宗中分房之议渐炽。";
        }

        if (clan.BranchTension >= 60)
        {
            return $"{clan.ClanName}祠堂争声渐炽。";
        }

        if (clan.MediationMomentum >= 45)
        {
            return $"{clan.ClanName}正请族老压住争端。";
        }

        if (grudgePressure >= 70)
        {
            return $"{clan.ClanName}宗中旧怨益深。";
        }

        if (averageDistress >= 60)
        {
            return $"{clan.ClanName}正受民户困顿所逼。";
        }

        if (clan.SupportReserve >= 65)
        {
            return $"{clan.ClanName}尚能以接济稳住门里人心。";
        }

        return $"{clan.ClanName}仍在乡里目光之下。";
    }

    private static void AddMemory(
        SocialMemoryAndRelationsState state,
        ClanId clanId,
        string kind,
        MemoryType type,
        MemorySubtype subtype,
        string causeKey,
        int monthlyDecay,
        int intensity,
        bool isPublic,
        string summary,
        ModuleExecutionContext context)
    {
        bool alreadyRecordedThisMonth = state.Memories.Any(memory =>
            memory.SubjectClanId == clanId &&
            string.Equals(memory.Kind, kind, StringComparison.Ordinal) &&
            memory.CreatedAt.Year == context.CurrentDate.Year &&
            memory.CreatedAt.Month == context.CurrentDate.Month);

        if (alreadyRecordedThisMonth)
        {
            return;
        }

        int clampedIntensity = Math.Clamp(intensity, 0, 100);
        state.Memories.Add(new MemoryRecordState
        {
            Id = KernelIdAllocator.NextMemory(context.KernelState),
            SubjectClanId = clanId,
            Kind = kind,
            Intensity = clampedIntensity,
            IsPublic = isPublic,
            CreatedAt = context.CurrentDate,
            Summary = summary,
            Type = type,
            Subtype = subtype,
            SourceKind = MemorySubjectKind.Clan,
            SourceClanId = clanId,
            TargetKind = MemorySubjectKind.Clan,
            TargetClanId = clanId,
            OriginDate = context.CurrentDate,
            CauseKey = causeKey,
            Weight = clampedIntensity,
            MonthlyDecay = Math.Max(1, monthlyDecay),
            LifecycleState = MemoryLifecycleState.Active,
        });
    }

    private static void AdvanceMemoryLifecycle(SocialMemoryAndRelationsState state)
    {
        foreach (MemoryRecordState memory in state.Memories)
        {
            if (memory.LifecycleState != MemoryLifecycleState.Active)
            {
                continue;
            }

            memory.Weight = Math.Max(0, memory.Weight - Math.Max(1, memory.MonthlyDecay));
            if (memory.Weight == 0)
            {
                memory.LifecycleState = MemoryLifecycleState.Dormant;
            }
        }
    }

    private sealed class SocialMemoryQueries : ISocialMemoryAndRelationsQueries
    {
        private readonly SocialMemoryAndRelationsState _state;

        public SocialMemoryQueries(SocialMemoryAndRelationsState state)
        {
            _state = state;
        }

        public ClanNarrativeSnapshot GetRequiredClanNarrative(ClanId clanId)
        {
            ClanNarrativeState narrative = _state.ClanNarratives.Single(narrative => narrative.ClanId == clanId);
            return CloneNarrative(narrative, _state.Memories);
        }

        public IReadOnlyList<ClanNarrativeSnapshot> GetClanNarratives()
        {
            return _state.ClanNarratives
                .OrderBy(static narrative => narrative.ClanId.Value)
                .Select(narrative => CloneNarrative(narrative, _state.Memories))
                .ToArray();
        }

        public IReadOnlyList<SocialMemoryEntrySnapshot> GetMemories()
        {
            return _state.Memories
                .OrderBy(static memory => memory.Id.Value)
                .Select(CloneMemory)
                .ToArray();
        }

        public IReadOnlyList<SocialMemoryEntrySnapshot> GetMemoriesByClan(ClanId clanId)
        {
            return _state.Memories
                .Where(memory => memory.SubjectClanId == clanId)
                .OrderBy(static memory => memory.Id.Value)
                .Select(CloneMemory)
                .ToArray();
        }

        public IReadOnlyList<DormantStubSnapshot> GetDormantStubs()
        {
            return _state.DormantStubs
                .OrderBy(static stub => stub.PersonId.Value)
                .Select(static stub => new DormantStubSnapshot
                {
                    PersonId = stub.PersonId,
                    LastKnownSettlementId = stub.LastKnownSettlementId,
                    LastKnownRole = stub.LastKnownRole,
                    ActiveMemoryIds = stub.ActiveMemoryIds.ToArray(),
                    LastSeen = stub.LastSeen,
                    IsEligibleForReemergence = stub.IsEligibleForReemergence,
                })
                .ToArray();
        }

        private static SocialMemoryEntrySnapshot CloneMemory(MemoryRecordState memory)
        {
            return new SocialMemoryEntrySnapshot
            {
                Id = memory.Id,
                Type = memory.Type,
                Subtype = memory.Subtype,
                SourceKind = memory.SourceKind,
                SourcePersonId = memory.SourcePersonId,
                SourceClanId = memory.SourceClanId,
                TargetKind = memory.TargetKind,
                TargetPersonId = memory.TargetPersonId,
                TargetClanId = memory.TargetClanId,
                OriginDate = memory.OriginDate,
                CauseKey = memory.CauseKey,
                Weight = memory.Weight,
                MonthlyDecay = memory.MonthlyDecay,
                IsPublic = memory.IsPublic,
                State = memory.LifecycleState,
                Summary = memory.Summary,
            };
        }

        private static ClanNarrativeSnapshot CloneNarrative(ClanNarrativeState narrative, IReadOnlyCollection<MemoryRecordState> memories)
        {
            int memoryCount = memories.Count(memory => memory.SubjectClanId == narrative.ClanId);
            return new ClanNarrativeSnapshot
            {
                ClanId = narrative.ClanId,
                PublicNarrative = narrative.PublicNarrative,
                GrudgePressure = narrative.GrudgePressure,
                FearPressure = narrative.FearPressure,
                ShamePressure = narrative.ShamePressure,
                FavorBalance = narrative.FavorBalance,
                MemoryCount = memoryCount,
            };
        }
    }
}
