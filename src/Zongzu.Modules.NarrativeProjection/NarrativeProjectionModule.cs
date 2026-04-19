using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.NarrativeProjection;

public sealed class NarrativeProjectionModule : ModuleRunner<NarrativeProjectionState>
{
    public const int NotificationRetentionLimit = 40;

    public override string ModuleKey => KnownModuleKeys.NarrativeProjection;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.Projection;

    public override int ExecutionOrder => 100;

    public override NarrativeProjectionState CreateInitialState()
    {
        return new NarrativeProjectionState();
    }

    public override void RegisterQueries(NarrativeProjectionState state, QueryRegistry queries)
    {
        queries.Register<INarrativeProjectionQueries>(new NarrativeProjectionQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<NarrativeProjectionState> scope)
    {
        IReadOnlyList<WorldDiffEntry> diffEntries = scope.Context.Diff.Entries;
        IReadOnlyList<IDomainEvent> domainEvents = scope.Context.DomainEvents.Events;

        Dictionary<string, WorldDiffEntry[]> diffsByModule = diffEntries
            .GroupBy(static entry => entry.ModuleKey, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.ToArray(), StringComparer.Ordinal);

        Dictionary<string, IDomainEvent[]> eventsByModule = domainEvents
            .GroupBy(static domainEvent => domainEvent.ModuleKey, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.ToArray(), StringComparer.Ordinal);

        string[] projectionKeys = diffsByModule.Keys
            .Concat(eventsByModule.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

        foreach (string moduleKey in projectionKeys)
        {
            WorldDiffEntry[] moduleDiffs = diffsByModule.TryGetValue(moduleKey, out WorldDiffEntry[]? diffGroup)
                ? diffGroup
                : [];
            IDomainEvent[] moduleEvents = eventsByModule.TryGetValue(moduleKey, out IDomainEvent[]? eventGroup)
                ? eventGroup
                : [];

            if (moduleEvents.Length > 0)
            {
                foreach (IDomainEvent domainEvent in moduleEvents)
                {
                    scope.State.Notifications.Add(
                        CreateEventNotification(
                            scope.Context.CurrentDate,
                            scope.Context.KernelState,
                            domainEvent,
                            moduleDiffs,
                            diffEntries));
                }

                continue;
            }

            if (moduleDiffs.Length > 0)
            {
                scope.State.Notifications.Add(CreateDiffNotification(scope.Context.CurrentDate, scope.Context.KernelState, moduleKey, moduleDiffs));
            }
        }

        TrimHistory(scope.State);
    }

    private static NarrativeNotificationState CreateEventNotification(
        GameDate currentDate,
        KernelState kernelState,
        IDomainEvent domainEvent,
        IReadOnlyList<WorldDiffEntry> moduleDiffs,
        IReadOnlyList<WorldDiffEntry> allDiffs)
    {
        NarrativeTraceState[] traces = BuildTraces(domainEvent, moduleDiffs, allDiffs);
        return new NarrativeNotificationState
        {
            Id = KernelIdAllocator.NextNotification(kernelState),
            CreatedAt = currentDate,
            Tier = DetermineTier(domainEvent.EventType),
            Surface = DetermineSurface(domainEvent.ModuleKey),
            Title = BuildTitle(domainEvent.ModuleKey, domainEvent.EventType, traces),
            Summary = domainEvent.Summary,
            WhyItHappened = BuildWhyText(traces),
            WhatNext = BuildWhatNext(domainEvent.ModuleKey, domainEvent.EventType, traces),
            SourceModuleKey = domainEvent.ModuleKey,
            IsRead = false,
            Traces = traces.ToList(),
        };
    }

    private static NarrativeNotificationState CreateDiffNotification(
        GameDate currentDate,
        KernelState kernelState,
        string moduleKey,
        IReadOnlyList<WorldDiffEntry> moduleDiffs)
    {
        NarrativeTraceState[] traces = moduleDiffs
            .Take(2)
            .Select(diff => new NarrativeTraceState
            {
                SourceModuleKey = moduleKey,
                EventType = string.Empty,
                EventSummary = string.Empty,
                DiffDescription = diff.Description,
                EntityKey = diff.EntityKey,
            })
            .ToArray();

        return new NarrativeNotificationState
        {
            Id = KernelIdAllocator.NextNotification(kernelState),
            CreatedAt = currentDate,
            Tier = NotificationTier.Background,
            Surface = DetermineSurface(moduleKey),
            Title = $"{GetModuleLabel(moduleKey)}月报",
            Summary = moduleDiffs[0].Description,
            WhyItHappened = BuildWhyText(traces),
            WhatNext = BuildWhatNext(moduleKey, string.Empty, traces),
            SourceModuleKey = moduleKey,
            IsRead = false,
            Traces = traces.ToList(),
        };
    }

    private static NarrativeTraceState[] BuildTraces(
        IDomainEvent domainEvent,
        IReadOnlyList<WorldDiffEntry> moduleDiffs,
        IReadOnlyList<WorldDiffEntry> allDiffs)
    {
        List<NarrativeTraceState> traces = moduleDiffs
            .Take(2)
            .Select(diff => new NarrativeTraceState
            {
                SourceModuleKey = domainEvent.ModuleKey,
                EventType = domainEvent.EventType,
                EventSummary = domainEvent.Summary,
                DiffDescription = diff.Description,
                EntityKey = diff.EntityKey,
            })
            .ToList();

        if (ShouldPullWarfareContext(domainEvent))
        {
            traces.AddRange(SelectWarfareContextDiffs(domainEvent, allDiffs)
                .Select(diff => new NarrativeTraceState
                {
                    SourceModuleKey = diff.ModuleKey,
                    EventType = domainEvent.EventType,
                    EventSummary = domainEvent.Summary,
                    DiffDescription = diff.Description,
                    EntityKey = diff.EntityKey,
                }));
        }

        if (ShouldPullFamilyLifecycleContext(domainEvent))
        {
            traces.AddRange(SelectFamilyLifecycleContextDiffs(domainEvent, allDiffs)
                .Select(diff => new NarrativeTraceState
                {
                    SourceModuleKey = diff.ModuleKey,
                    EventType = domainEvent.EventType,
                    EventSummary = domainEvent.Summary,
                    DiffDescription = diff.Description,
                    EntityKey = diff.EntityKey,
                }));
        }

        if (traces.Count == 0)
        {
            traces.Add(new NarrativeTraceState
            {
                SourceModuleKey = domainEvent.ModuleKey,
                EventType = domainEvent.EventType,
                EventSummary = domainEvent.Summary,
                DiffDescription = "此案暂无旁证可录。",
                EntityKey = domainEvent.EntityKey,
            });
        }

        return traces.ToArray();
    }

    private static IReadOnlyList<WorldDiffEntry> SelectWarfareContextDiffs(
        IDomainEvent domainEvent,
        IReadOnlyList<WorldDiffEntry> allDiffs)
    {
        List<WorldDiffEntry> selected = new();
        TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.FamilyCore, KnownModuleKeys.SocialMemoryAndRelations);
        TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.OfficeAndCareer, KnownModuleKeys.ConflictAndForce);
        TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.PopulationAndHouseholds, KnownModuleKeys.WorldSettlements);
        TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.OrderAndBanditry, KnownModuleKeys.TradeAndIndustry);
        return selected;
    }

    private static IReadOnlyList<WorldDiffEntry> SelectFamilyLifecycleContextDiffs(
        IDomainEvent domainEvent,
        IReadOnlyList<WorldDiffEntry> allDiffs)
    {
        List<WorldDiffEntry> selected = new();

        switch (domainEvent.EventType)
        {
            case FamilyCoreEventNames.BirthRegistered:
                TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.PopulationAndHouseholds, KnownModuleKeys.WorldSettlements);
                TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.SocialMemoryAndRelations);
                break;
            case FamilyCoreEventNames.DeathRegistered:
                TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.SocialMemoryAndRelations);
                TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.PopulationAndHouseholds, KnownModuleKeys.WorldSettlements);
                break;
            case FamilyCoreEventNames.HeirSecurityWeakened:
                TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.PopulationAndHouseholds);
                TryAddContextDiff(selected, domainEvent, allDiffs, KnownModuleKeys.SocialMemoryAndRelations);
                break;
        }

        return selected;
    }

    private static void TryAddContextDiff(
        ICollection<WorldDiffEntry> selected,
        IDomainEvent domainEvent,
        IReadOnlyList<WorldDiffEntry> allDiffs,
        params string[] moduleKeys)
    {
        foreach (string moduleKey in moduleKeys)
        {
            WorldDiffEntry? candidate = allDiffs
                .Where(diff =>
                    !string.Equals(diff.ModuleKey, domainEvent.ModuleKey, StringComparison.Ordinal)
                    && string.Equals(diff.EntityKey, domainEvent.EntityKey, StringComparison.Ordinal)
                    && string.Equals(diff.ModuleKey, moduleKey, StringComparison.Ordinal)
                    && IsWarfareContextModule(diff.ModuleKey))
                .OrderBy(static diff => diff.Description, StringComparer.Ordinal)
                .FirstOrDefault();

            if (candidate is not null)
            {
                selected.Add(candidate);
                return;
            }
        }
    }

    private static bool ShouldPullWarfareContext(IDomainEvent domainEvent)
    {
        return string.Equals(domainEvent.ModuleKey, KnownModuleKeys.WarfareCampaign, StringComparison.Ordinal)
            && domainEvent.EventType is WarfareCampaignEventNames.CampaignPressureRaised
                or WarfareCampaignEventNames.CampaignSupplyStrained
                or WarfareCampaignEventNames.CampaignAftermathRegistered;
    }

    private static bool ShouldPullFamilyLifecycleContext(IDomainEvent domainEvent)
    {
        return string.Equals(domainEvent.ModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal)
            && domainEvent.EventType is FamilyCoreEventNames.BirthRegistered
                or FamilyCoreEventNames.DeathRegistered
                or FamilyCoreEventNames.HeirSecurityWeakened;
    }

    private static bool IsWarfareContextModule(string moduleKey)
    {
        return moduleKey is
            KnownModuleKeys.FamilyCore or
            KnownModuleKeys.WorldSettlements or
            KnownModuleKeys.PopulationAndHouseholds or
            KnownModuleKeys.TradeAndIndustry or
            KnownModuleKeys.OrderAndBanditry or
            KnownModuleKeys.OfficeAndCareer or
            KnownModuleKeys.SocialMemoryAndRelations or
            KnownModuleKeys.ConflictAndForce;
    }

    private static NotificationTier DetermineTier(string eventType)
    {
        return eventType switch
        {
            "TradeDebtDefaulted" => NotificationTier.Urgent,
            "LivelihoodCollapsed" => NotificationTier.Urgent,
            "ExamPassed" => NotificationTier.Consequential,
            "ExamFailed" => NotificationTier.Consequential,
            "StudyAbandoned" => NotificationTier.Consequential,
            "TradeProspered" => NotificationTier.Consequential,
            "TradeLossOccurred" => NotificationTier.Consequential,
            "RouteBusinessBlocked" => NotificationTier.Consequential,
            "BanditThreatRaised" => NotificationTier.Consequential,
            "OutlawGroupFormed" => NotificationTier.Consequential,
            "RouteUnsafeDueToBanditry" => NotificationTier.Consequential,
            "ConflictResolved" => NotificationTier.Consequential,
            "CommanderWounded" => NotificationTier.Urgent,
            "ForceReadinessChanged" => NotificationTier.Consequential,
            "MilitiaMobilized" => NotificationTier.Consequential,
            PublicLifeAndRumorEventNames.StreetTalkSurged => NotificationTier.Consequential,
            PublicLifeAndRumorEventNames.CountyGateCrowded => NotificationTier.Consequential,
            PublicLifeAndRumorEventNames.MarketBuzzRaised => NotificationTier.Background,
            PublicLifeAndRumorEventNames.RoadReportDelayed => NotificationTier.Urgent,
            PublicLifeAndRumorEventNames.PrefectureDispatchPressed => NotificationTier.Urgent,
            WarfareCampaignEventNames.CampaignMobilized => NotificationTier.Consequential,
            WarfareCampaignEventNames.CampaignPressureRaised => NotificationTier.Urgent,
            WarfareCampaignEventNames.CampaignSupplyStrained => NotificationTier.Urgent,
            WarfareCampaignEventNames.CampaignAftermathRegistered => NotificationTier.Consequential,
            "MigrationStarted" => NotificationTier.Consequential,
            "GrudgeEscalated" => NotificationTier.Consequential,
            FamilyCoreEventNames.MarriageAllianceArranged => NotificationTier.Consequential,
            FamilyCoreEventNames.BirthRegistered => NotificationTier.Consequential,
            FamilyCoreEventNames.DeathRegistered => NotificationTier.Urgent,
            FamilyCoreEventNames.HeirSecurityWeakened => NotificationTier.Urgent,
            FamilyCoreEventNames.LineageDisputeHardened => NotificationTier.Consequential,
            FamilyCoreEventNames.LineageMediationOpened => NotificationTier.Consequential,
            FamilyCoreEventNames.BranchSeparationApproved => NotificationTier.Consequential,
            _ => NotificationTier.Background,
        };
    }

    private static NarrativeSurface DetermineSurface(string moduleKey)
    {
        return moduleKey switch
        {
            KnownModuleKeys.FamilyCore => NarrativeSurface.AncestralHall,
            KnownModuleKeys.SocialMemoryAndRelations => NarrativeSurface.AncestralHall,
            KnownModuleKeys.WorldSettlements => NarrativeSurface.DeskSandbox,
            KnownModuleKeys.PopulationAndHouseholds => NarrativeSurface.DeskSandbox,
            KnownModuleKeys.EducationAndExams => NarrativeSurface.DeskSandbox,
            KnownModuleKeys.TradeAndIndustry => NarrativeSurface.DeskSandbox,
            KnownModuleKeys.PublicLifeAndRumor => NarrativeSurface.DeskSandbox,
            KnownModuleKeys.OrderAndBanditry => NarrativeSurface.DeskSandbox,
            KnownModuleKeys.ConflictAndForce => NarrativeSurface.ConflictVignette,
            KnownModuleKeys.WarfareCampaign => NarrativeSurface.DeskSandbox,
            _ => NarrativeSurface.GreatHall,
        };
    }

    private static string BuildTitle(string moduleKey, string eventType, IReadOnlyList<NarrativeTraceState> traces)
    {
        return eventType switch
        {
            "ExamPassed" => "场屋得捷",
            "ExamFailed" => "场屋失利",
            "StudyAbandoned" => "停馆罢读",
            "TradeProspered" => "市利有进",
            "TradeLossOccurred" => "商账受亏",
            "TradeDebtDefaulted" => "债主压门",
            "RouteBusinessBlocked" => "行路受阻",
            "BanditThreatRaised" => "盗警渐起",
            "OutlawGroupFormed" => "啸聚成股",
            "RouteUnsafeDueToBanditry" => "商路不靖",
            "ConflictResolved" => "乡斗暂息",
            "CommanderWounded" => "领队负创",
            "ForceReadinessChanged" => "营伍更张",
            "MilitiaMobilized" => "乡勇应募",
            PublicLifeAndRumorEventNames.StreetTalkSurged => "街谈渐热",
            PublicLifeAndRumorEventNames.CountyGateCrowded => "县门壅挤",
            PublicLifeAndRumorEventNames.MarketBuzzRaised => "镇市喧起",
            PublicLifeAndRumorEventNames.RoadReportDelayed => "路报迟滞",
            PublicLifeAndRumorEventNames.PrefectureDispatchPressed => "州牒催迫",
            WarfareCampaignEventNames.CampaignMobilized => "军檄立案",
            WarfareCampaignEventNames.CampaignPressureRaised => "前线告急",
            WarfareCampaignEventNames.CampaignSupplyStrained => "粮道告急",
            WarfareCampaignEventNames.CampaignAftermathRegistered => BuildCampaignAftermathTitle(traces),
            "MigrationStarted" => "流徙启行",
            "LivelihoodCollapsed" => "生计顿敝",
            "GrudgeEscalated" => "旧怨益深",
            FamilyCoreEventNames.LineageDisputeHardened => "祠堂争议渐炽",
            FamilyCoreEventNames.LineageMediationOpened => "族老调停开议",
            FamilyCoreEventNames.BranchSeparationApproved => "分房议定",
            FamilyCoreEventNames.MarriageAllianceArranged => "门内议亲",
            FamilyCoreEventNames.BirthRegistered => "门内添丁",
            FamilyCoreEventNames.DeathRegistered => "门内举哀",
            FamilyCoreEventNames.HeirSecurityWeakened => "承祧未稳",
            _ => $"{GetModuleLabel(moduleKey)}告示",
        };
    }

    private static string BuildCampaignAftermathTitle(IReadOnlyList<NarrativeTraceState> traces)
    {
        bool hasMerit = traces.Any(static trace =>
            string.Equals(trace.SourceModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal)
            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.SocialMemoryAndRelations, StringComparison.Ordinal));
        bool hasBlame = traces.Any(static trace =>
            string.Equals(trace.SourceModuleKey, KnownModuleKeys.OfficeAndCareer, StringComparison.Ordinal)
            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.ConflictAndForce, StringComparison.Ordinal));
        bool hasRelief = traces.Any(static trace =>
            string.Equals(trace.SourceModuleKey, KnownModuleKeys.PopulationAndHouseholds, StringComparison.Ordinal)
            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.WorldSettlements, StringComparison.Ordinal));

        if (hasMerit && hasBlame && hasRelief)
        {
            return "战后赏罚与抚恤";
        }

        if (hasBlame && hasRelief)
        {
            return "战后追责与抚恤";
        }

        if (hasMerit && hasRelief)
        {
            return "战后记功与抚恤";
        }

        return "战后覆核入案";
    }

    private static string BuildWhyText(IReadOnlyList<NarrativeTraceState> traces)
    {
        string[] reasons = traces
            .Select(static trace => trace.DiffDescription)
            .Where(static text => !string.IsNullOrWhiteSpace(text))
            .Distinct(StringComparer.Ordinal)
            .Take(4)
            .ToArray();

        if (reasons.Length == 0)
        {
            return "案下暂无可征旁证。";
        }

        return string.Join(" ", reasons);
    }

    private static string BuildWhatNext(string moduleKey, string eventType, IReadOnlyList<NarrativeTraceState> traces)
    {
        return eventType switch
        {
            "ExamPassed" => "先回宗房看谁可续供书资，再议是否借此经营官途。",
            "ExamFailed" => "先定是否续供馆谷、缓其心火，还是转向别的出路。",
            "StudyAbandoned" => "先看宗房是否担得住声望挫折，再议改走别途。",
            "TradeProspered" => "先看案上商路与市肆风色，再定守利还是添本。",
            "TradeLossOccurred" => "先看路险、市价与宗房余力，再定是否继续下注。",
            "TradeDebtDefaulted" => "先查民户与宗房余力，别让债压继续外溢。",
            "RouteBusinessBlocked" => "先看路险落在何处，再定暂避、缓行还是硬着头皮通路。",
            "BanditThreatRaised" => "先看此地盗压与商路，再定添巡、缓行还是暂且容忍市面迟滞。",
            "OutlawGroupFormed" => "先看人心、旧怨与路口外露，再定是镇、抚还是先稳宗房。",
            "RouteUnsafeDueToBanditry" => "先看商账与乡里情势，再定观望、镇压还是认下行路迟滞。",
            "ConflictResolved" => "先看冲突案卷，再定添护运、缓报复还是认下镇息之费。",
            "CommanderWounded" => "此事须立刻过目，负创最易牵动营伍与追责。",
            "ForceReadinessChanged" => "先看守丁、乡勇与护运气力，再定下月偏重何处。",
            "MilitiaMobilized" => "先定乡勇是暂留声势，还是尽早收回，以免地方不静久拖。",
            PublicLifeAndRumorEventNames.StreetTalkSurged => "先看街谈所起是因民困、失礼还是市面波动，再定该压、该抚还是该缓。",
            PublicLifeAndRumorEventNames.CountyGateCrowded => "先看榜示、词状与差人催并压在何处，再定先理积案还是先安门前人情。",
            PublicLifeAndRumorEventNames.MarketBuzzRaised => "先看哪处镇市最热、哪一路客货最盛，再定添本守利还是暂借喧势行货。",
            PublicLifeAndRumorEventNames.RoadReportDelayed => "先看驿路、河埠与津口何处迟滞，再定是护一路、缓一路，还是先稳县门消息。",
            PublicLifeAndRumorEventNames.PrefectureDispatchPressed => "先看州牒、差发与县门榜示压在何处，再定先消积案、先稳乡里，还是先应上催。",
            WarfareCampaignEventNames.CampaignMobilized => "展开军务沙盘，先看前线、粮道与文移接应，再定是否继续发檄点兵。",
            WarfareCampaignEventNames.CampaignPressureRaised => "先看前线告急落在何处，再定是增援、固守关津，还是缩短战线。",
            WarfareCampaignEventNames.CampaignSupplyStrained => "先看护运、仓储与渡口，再定是催督粮道、修转运支路，还是暂缓推进。",
            WarfareCampaignEventNames.CampaignAftermathRegistered => BuildCampaignAftermathNextStep(traces),
            "MigrationStarted" => "先看民困与佣作去路，再定赈恤、缓征还是另觅安顿。",
            "LivelihoodCollapsed" => "此事须立刻过堂，民困最易化成怨气与流徙。",
            FamilyCoreEventNames.LineageDisputeHardened => "先回祠堂看房支争端落在承祧、接济还是分房，再定偏护、责礼还是请族老出面。",
            FamilyCoreEventNames.LineageMediationOpened => "先看族老调停是否压住旧怨，再定是否赔礼、续济还是改议分房。",
            FamilyCoreEventNames.BranchSeparationApproved => "先看分房是否稳住祠堂气口，再定后续接济、祭田与香火如何分开。",
            FamilyCoreEventNames.MarriageAllianceArranged => "先看这门婚议是缓了承祧之急，还是又添人情与聘财之压，再定后手接济轻重。",
            FamilyCoreEventNames.BirthRegistered => BuildBirthNextStep(traces),
            FamilyCoreEventNames.DeathRegistered => BuildDeathNextStep(traces),
            FamilyCoreEventNames.HeirSecurityWeakened => BuildHeirSecurityNextStep(traces),
            _ => moduleKey switch
            {
                KnownModuleKeys.WorldSettlements => "先看案头乡里风色，辨清安危与丰等如何消长。",
                KnownModuleKeys.PopulationAndHouseholds => "先看民户情势，别让困顿外溢成流徙与怨气。",
                KnownModuleKeys.FamilyCore => "回宗房看房支与承祧情形，再定下一步。",
                KnownModuleKeys.SocialMemoryAndRelations => "回宗房看情分、羞责与旧怨，再定是否先行安抚。",
                KnownModuleKeys.EducationAndExams => "去顾馆案前看本月学业与场屋动静。",
                KnownModuleKeys.TradeAndIndustry => "翻市账与路报，先看商路压在何处。",
                KnownModuleKeys.PublicLifeAndRumor => "先看乡里、镇市、县门与路报哪处先热，再定应榜示、应抚恤，还是应压住门前喧议。",
                KnownModuleKeys.OrderAndBanditry => "看案头哪处不静、哪条商路将坏，再定镇抚轻重。",
                KnownModuleKeys.ConflictAndForce => "看冲突案卷，先辨守丁、乡勇与护运吃力在何处。",
                KnownModuleKeys.WarfareCampaign => "查看军务沙盘，确认前线、粮道与善后态势如何演变。",
                _ => "先看案上情势，再发后令。",
            },
        };
    }

    private static string BuildBirthNextStep(IReadOnlyList<NarrativeTraceState> traces)
    {
        bool hasHouseholdCare = traces.Any(static trace =>
            string.Equals(trace.SourceModuleKey, KnownModuleKeys.PopulationAndHouseholds, StringComparison.Ordinal)
            || trace.DiffDescription.Contains("襁褓", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("抚养", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("口粮", StringComparison.Ordinal));
        bool hasHeirPressure = traces.Any(static trace =>
            trace.DiffDescription.Contains("承祧", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("继嗣", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("香火", StringComparison.Ordinal));

        if (hasHouseholdCare && hasHeirPressure)
        {
            return "先护产妇与襁褓，再把口粮、乳哺与看护之费拨稳；若门内本就为承祧焦心，也要趁势缓一缓诸房催逼。";
        }

        if (hasHouseholdCare)
        {
            return "先把产妇、襁褓与口粮护住，别让添丁喜事转成抚养与债压的新缺口。";
        }

        return "先把产妇与襁褓照看住，再看口粮、乳哺与看护之费该由谁分担；若门内原有承祧焦心，也要趁势缓一缓诸房催逼。";
    }

    private static string BuildDeathNextStep(IReadOnlyList<NarrativeTraceState> traces)
    {
        bool hasSuccessionShock = traces.Any(static trace =>
            trace.DiffDescription.Contains("承祧", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("继嗣", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("后议", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("房支", StringComparison.Ordinal));
        bool hasMourningLoad = traces.Any(static trace =>
            trace.DiffDescription.Contains("丧服", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("祭次", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("举哀", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("支用", StringComparison.Ordinal));

        if (hasSuccessionShock && hasMourningLoad)
        {
            return "先把丧次、祭次与支用议定，别让门内一边举哀一边翻出承祧旧账；若继嗣未稳，就要尽早把谱内名分先按住。";
        }

        if (hasSuccessionShock)
        {
            return "先看谁来接住香火名分，再定后议与接济，不要让举哀之时变成诸房争先之机。";
        }

        return "先把丧次、发引与祭次理顺，再把承祧名分暂按住，随后看门内缺口落在口粮、劳力还是堂上支用。";
    }

    private static string BuildHeirSecurityNextStep(IReadOnlyList<NarrativeTraceState> traces)
    {
        bool hasChildCare = traces.Any(static trace =>
            trace.DiffDescription.Contains("年幼", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("襁褓", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("护持", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("抚养", StringComparison.Ordinal));
        bool hasBranchPressure = traces.Any(static trace =>
            trace.DiffDescription.Contains("房支", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("后议", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("争", StringComparison.Ordinal)
            || trace.DiffDescription.Contains("分房", StringComparison.Ordinal));

        if (hasChildCare && hasBranchPressure)
        {
            return "先把嗣苗护住、名分写稳，再看要不要请族老压住诸房后议；若人尚年幼，就别让承祧名分离了抚养与接济。";
        }

        if (hasChildCare)
        {
            return "先护住嗣苗与抚养之资，再议何时把名分写入谱案，免得香火虽有着落，人却无人照拂。";
        }

        return "先把承祧次序与谱内名分重新按定，再把诸房声气压住，别让名分未稳拖成新的门内争议。";
    }

    private static string BuildCampaignAftermathNextStep(IReadOnlyList<NarrativeTraceState> traces)
    {
        bool hasMerit = traces.Any(static trace =>
            string.Equals(trace.SourceModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal)
            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.SocialMemoryAndRelations, StringComparison.Ordinal));
        bool hasBlame = traces.Any(static trace =>
            string.Equals(trace.SourceModuleKey, KnownModuleKeys.OfficeAndCareer, StringComparison.Ordinal)
            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.ConflictAndForce, StringComparison.Ordinal));
        bool hasRelief = traces.Any(static trace =>
            string.Equals(trace.SourceModuleKey, KnownModuleKeys.PopulationAndHouseholds, StringComparison.Ordinal)
            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.WorldSettlements, StringComparison.Ordinal));

        List<string> tasks = new();
        if (hasMerit)
        {
            tasks.Add("记功请赏");
        }

        if (hasBlame)
        {
            tasks.Add("失守追责");
        }

        if (hasRelief)
        {
            tasks.Add("抚恤安民");
        }

        if (tasks.Count == 0)
        {
            return "先看战后覆核，再定班师、修粮道还是继续戒备。";
        }

        return $"先看{string.Join("、", tasks)}诸案，再定班师归营、修补粮道，还是继续镇抚地方。";
    }

    private static string GetModuleLabel(string moduleKey)
    {
        return moduleKey switch
        {
            KnownModuleKeys.WorldSettlements => "乡里",
            KnownModuleKeys.PopulationAndHouseholds => "民户",
            KnownModuleKeys.FamilyCore => "宗房",
            KnownModuleKeys.SocialMemoryAndRelations => "情分旧怨",
            KnownModuleKeys.EducationAndExams => "塾馆科途",
            KnownModuleKeys.TradeAndIndustry => "市易",
            KnownModuleKeys.PublicLifeAndRumor => "县情",
            KnownModuleKeys.OrderAndBanditry => "巷缉",
            KnownModuleKeys.ConflictAndForce => "兵防",
            KnownModuleKeys.OfficeAndCareer => "官署",
            KnownModuleKeys.WarfareCampaign => "军务",
            _ => moduleKey,
        };
    }

    private static void TrimHistory(NarrativeProjectionState state)
    {
        if (state.Notifications.Count <= NotificationRetentionLimit)
        {
            return;
        }

        int removeCount = state.Notifications.Count - NotificationRetentionLimit;
        state.Notifications.RemoveRange(0, removeCount);
    }

    private sealed class NarrativeProjectionQueries : INarrativeProjectionQueries
    {
        private readonly NarrativeProjectionState _state;

        public NarrativeProjectionQueries(NarrativeProjectionState state)
        {
            _state = state;
        }

        public NarrativeNotificationSnapshot GetRequiredNotification(NotificationId notificationId)
        {
            NarrativeNotificationState notification = _state.Notifications.Single(notification => notification.Id == notificationId);
            return Clone(notification);
        }

        public IReadOnlyList<NarrativeNotificationSnapshot> GetNotifications()
        {
            return _state.Notifications
                .OrderByDescending(static notification => notification.CreatedAt.Year)
                .ThenByDescending(static notification => notification.CreatedAt.Month)
                .ThenByDescending(static notification => notification.Id.Value)
                .Select(Clone)
                .ToArray();
        }

        private static NarrativeNotificationSnapshot Clone(NarrativeNotificationState notification)
        {
            return new NarrativeNotificationSnapshot
            {
                Id = notification.Id,
                CreatedAt = notification.CreatedAt,
                Tier = notification.Tier,
                Surface = notification.Surface,
                Title = notification.Title,
                Summary = notification.Summary,
                WhyItHappened = notification.WhyItHappened,
                WhatNext = notification.WhatNext,
                SourceModuleKey = notification.SourceModuleKey,
                IsRead = notification.IsRead,
                Traces = notification.Traces.Select(CloneTrace).ToArray(),
            };
        }

        private static NotificationTraceSnapshot CloneTrace(NarrativeTraceState trace)
        {
            return new NotificationTraceSnapshot
            {
                SourceModuleKey = trace.SourceModuleKey,
                EventType = trace.EventType,
                EventSummary = trace.EventSummary,
                DiffDescription = trace.DiffDescription,
                EntityKey = trace.EntityKey,
            };
        }
    }
}
