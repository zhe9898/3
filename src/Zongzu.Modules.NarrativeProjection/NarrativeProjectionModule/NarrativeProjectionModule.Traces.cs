using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.NarrativeProjection;

public sealed partial class NarrativeProjectionModule : ModuleRunner<NarrativeProjectionState>
{
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

            case FamilyCoreEventNames.ClanMemberDied:

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

                or FamilyCoreEventNames.ClanMemberDied

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


}
