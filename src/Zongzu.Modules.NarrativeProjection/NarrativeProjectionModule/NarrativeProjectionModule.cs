using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.NarrativeProjection;

public sealed partial class NarrativeProjectionModule : ModuleRunner<NarrativeProjectionState>
{
    public const int NotificationRetentionLimit = 40;


    public override string ModuleKey => KnownModuleKeys.NarrativeProjection;


    public override int ModuleSchemaVersion => 1;


    public override SimulationPhase Phase => SimulationPhase.Projection;


    public override int ExecutionOrder => 100;


    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;


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


}
