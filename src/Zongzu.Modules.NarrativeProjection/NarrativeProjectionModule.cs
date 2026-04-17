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
                    scope.State.Notifications.Add(CreateEventNotification(scope.Context.CurrentDate, scope.Context.KernelState, domainEvent, moduleDiffs));
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
        IReadOnlyList<WorldDiffEntry> moduleDiffs)
    {
        NarrativeTraceState[] traces = BuildTraces(domainEvent, moduleDiffs);
        return new NarrativeNotificationState
        {
            Id = KernelIdAllocator.NextNotification(kernelState),
            CreatedAt = currentDate,
            Tier = DetermineTier(domainEvent.EventType),
            Surface = DetermineSurface(domainEvent.ModuleKey),
            Title = BuildTitle(domainEvent.ModuleKey, domainEvent.EventType),
            Summary = domainEvent.Summary,
            WhyItHappened = BuildWhyText(moduleDiffs),
            WhatNext = BuildWhatNext(domainEvent.ModuleKey, domainEvent.EventType),
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
            Title = $"{GetModuleLabel(moduleKey)} Monthly Update",
            Summary = moduleDiffs[0].Description,
            WhyItHappened = BuildWhyText(moduleDiffs),
            WhatNext = BuildWhatNext(moduleKey, string.Empty),
            SourceModuleKey = moduleKey,
            IsRead = false,
            Traces = traces.ToList(),
        };
    }

    private static NarrativeTraceState[] BuildTraces(IDomainEvent domainEvent, IReadOnlyList<WorldDiffEntry> moduleDiffs)
    {
        if (moduleDiffs.Count == 0)
        {
            return
            [
                new NarrativeTraceState
                {
                    SourceModuleKey = domainEvent.ModuleKey,
                    EventType = domainEvent.EventType,
                    EventSummary = domainEvent.Summary,
                    DiffDescription = "No module diff text was recorded for this notice.",
                    EntityKey = null,
                },
            ];
        }

        return moduleDiffs
            .Take(2)
            .Select(diff => new NarrativeTraceState
            {
                SourceModuleKey = domainEvent.ModuleKey,
                EventType = domainEvent.EventType,
                EventSummary = domainEvent.Summary,
                DiffDescription = diff.Description,
                EntityKey = diff.EntityKey,
            })
            .ToArray();
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
            "MigrationStarted" => NotificationTier.Consequential,
            "GrudgeEscalated" => NotificationTier.Consequential,
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
            _ => NarrativeSurface.GreatHall,
        };
    }

    private static string BuildTitle(string moduleKey, string eventType)
    {
        return eventType switch
        {
            "ExamPassed" => "Exam Success",
            "ExamFailed" => "Exam Failure",
            "StudyAbandoned" => "Study Abandoned",
            "TradeProspered" => "Trade Prospered",
            "TradeLossOccurred" => "Trade Loss",
            "TradeDebtDefaulted" => "Trade Debt Defaulted",
            "RouteBusinessBlocked" => "Route Business Blocked",
            "MigrationStarted" => "Migration Began",
            "LivelihoodCollapsed" => "Livelihood Collapsed",
            "GrudgeEscalated" => "Grudge Escalated",
            _ => $"{GetModuleLabel(moduleKey)} Notice",
        };
    }

    private static string BuildWhyText(IReadOnlyList<WorldDiffEntry> moduleDiffs)
    {
        if (moduleDiffs.Count == 0)
        {
            return "This notice has no extra structural diff text yet.";
        }

        return string.Join(" ", moduleDiffs.Take(2).Select(static diff => diff.Description));
    }

    private static string BuildWhatNext(string moduleKey, string eventType)
    {
        return eventType switch
        {
            "ExamPassed" => "Review the lineage surface and decide whether to convert scholarly momentum into future office ambitions.",
            "ExamFailed" => "Decide whether to keep funding study, lower pressure, or redirect support to other pathways.",
            "StudyAbandoned" => "Choose whether to absorb the reputational hit or redirect the household into another social pathway.",
            "TradeProspered" => "Inspect the desk sandbox before reinvesting and decide whether to protect gains or expand cautiously.",
            "TradeLossOccurred" => "Check route risk, market demand, and clan reserves before committing more capital.",
            "TradeDebtDefaulted" => "Review household pressure and clan reserves immediately before debt stress spreads further.",
            "RouteBusinessBlocked" => "Inspect route risk in the desk sandbox and consider whether to cut exposure or wait out disruption.",
            "MigrationStarted" => "Review household distress and decide whether relief or tenancy changes can stabilize the settlement.",
            "LivelihoodCollapsed" => "This requires immediate attention in the hall because distress may cascade into grudges or migration.",
            _ => moduleKey switch
            {
                KnownModuleKeys.WorldSettlements => "Inspect the desk sandbox to see how prosperity and security are drifting.",
                KnownModuleKeys.PopulationAndHouseholds => "Review commoner pressure before it spills into migration or grievance.",
                KnownModuleKeys.FamilyCore => "Open the lineage surface to review the branch and heir context.",
                KnownModuleKeys.SocialMemoryAndRelations => "Check the lineage surface for favors, shame, and grudge pressure before tensions harden.",
                KnownModuleKeys.EducationAndExams => "Open the academy and study panel to review this month's scholarly movement.",
                KnownModuleKeys.TradeAndIndustry => "Open the market ledger and route overview to inspect commercial pressure.",
                _ => "Review the relevant surface before issuing further commands.",
            },
        };
    }

    private static string GetModuleLabel(string moduleKey)
    {
        return moduleKey switch
        {
            KnownModuleKeys.WorldSettlements => "Settlement",
            KnownModuleKeys.PopulationAndHouseholds => "Household",
            KnownModuleKeys.FamilyCore => "Family",
            KnownModuleKeys.SocialMemoryAndRelations => "Social Memory",
            KnownModuleKeys.EducationAndExams => "Education",
            KnownModuleKeys.TradeAndIndustry => "Trade",
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
