using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Persistence;

namespace Zongzu.Application;

public sealed class PresentationReadModelBuilder
{
    private readonly SaveCodec _saveCodec = new();

    public PresentationReadModelBundle BuildForM2(GameSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        QueryRegistry queries = BuildQueries(simulation);

        PresentationReadModelBundle bundle = new()
        {
            CurrentDate = simulation.CurrentDate,
            ReplayHash = simulation.ReplayHash,
        };

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.FamilyCore))
        {
            bundle.Clans = queries.GetRequired<IFamilyCoreQueries>().GetClans();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WorldSettlements))
        {
            bundle.Settlements = queries.GetRequired<IWorldSettlementsQueries>().GetSettlements();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PopulationAndHouseholds))
        {
            bundle.PopulationSettlements = queries.GetRequired<IPopulationAndHouseholdsQueries>().GetSettlements();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.EducationAndExams))
        {
            IEducationAndExamsQueries educationQueries = queries.GetRequired<IEducationAndExamsQueries>();
            bundle.EducationCandidates = educationQueries.GetCandidates();
            bundle.Academies = educationQueries.GetAcademies();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry))
        {
            ITradeAndIndustryQueries tradeQueries = queries.GetRequired<ITradeAndIndustryQueries>();
            ClanTradeSnapshot[] clanTrades = tradeQueries.GetClanTrades().ToArray();
            bundle.ClanTrades = clanTrades;
            bundle.Markets = tradeQueries.GetMarkets();
            bundle.TradeRoutes = clanTrades
                .SelectMany(trade => tradeQueries.GetRoutesForClan(trade.ClanId))
                .OrderBy(static route => route.RouteId)
                .ToArray();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.NarrativeProjection))
        {
            bundle.Notifications = queries.GetRequired<INarrativeProjectionQueries>().GetNotifications();
        }

        bundle.Debug = BuildDebugSnapshot(simulation, bundle.Notifications);
        return bundle;
    }

    private static QueryRegistry BuildQueries(GameSimulation simulation)
    {
        QueryRegistry queries = new();
        foreach (IModuleRunner module in simulation.Modules
                     .OrderBy(static module => module.Phase)
                     .ThenBy(static module => module.ExecutionOrder)
                     .ThenBy(static module => module.ModuleKey, StringComparer.Ordinal))
        {
            if (!simulation.FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!simulation.TryGetModuleState(module.ModuleKey, out object? state) || state is null)
            {
                throw new InvalidOperationException($"Enabled module {module.ModuleKey} has no state for read-model export.");
            }

            module.RegisterQueries(state, queries);
        }

        return queries;
    }

    private PresentationDebugSnapshot BuildDebugSnapshot(
        GameSimulation simulation,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        SaveRoot saveRoot = simulation.ExportSave();
        List<string> warnings = new();
        List<string> invariants = new();

        try
        {
            ModuleBoundaryValidator.Validate(simulation.Modules, simulation.FeatureManifest, saveRoot);
            invariants.Add("Module boundary validation passed.");
        }
        catch (Exception exception)
        {
            warnings.Add($"Module boundary validation failed: {exception.Message}");
        }

        if (simulation.LastMonthResult is null)
        {
            warnings.Add("No monthly diff has been recorded yet.");
        }
        else
        {
            if (simulation.LastMonthResult.Diff.Entries.Count == 0)
            {
                warnings.Add("The latest simulated month produced no diff entries.");
            }

            if (simulation.LastMonthResult.DomainEvents.Count == 0)
            {
                warnings.Add("The latest simulated month produced no domain events.");
            }
        }

        if (notifications.Count >= NarrativeProjectionModule.NotificationRetentionLimit)
        {
            warnings.Add("Notification history is at the retention limit; older notices may already be trimmed.");
        }

        if (notifications.Any(static notification => notification.Tier == NotificationTier.Urgent))
        {
            warnings.Add("Urgent notifications are pending review.");
        }

        return new PresentationDebugSnapshot
        {
            DiagnosticsSchemaVersion = 1,
            InitialSeed = simulation.KernelState.InitialSeed,
            NotificationRetentionLimit = NarrativeProjectionModule.NotificationRetentionLimit,
            RetentionLimitReached = notifications.Count >= NarrativeProjectionModule.NotificationRetentionLimit,
            LatestMetrics = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = simulation.LastMonthResult?.Diff.Entries.Count ?? 0,
                DomainEventCount = simulation.LastMonthResult?.DomainEvents.Count ?? 0,
                NotificationCount = notifications.Count,
                SavePayloadBytes = _saveCodec.Encode(saveRoot).Length,
            },
            EnabledModules = simulation.FeatureManifest.GetOrderedEntries()
                .Where(static pair => !string.Equals(pair.Value, "off", StringComparison.Ordinal))
                .Select(static pair => new DebugFeatureModeSnapshot
                {
                    ModuleKey = pair.Key,
                    Mode = pair.Value,
                })
                .ToArray(),
            ModuleInspectors = saveRoot.ModuleStates
                .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
                .Select(static pair => new DebugModuleInspectorSnapshot
                {
                    ModuleKey = pair.Key,
                    ModuleSchemaVersion = pair.Value.ModuleSchemaVersion,
                    PayloadBytes = pair.Value.Payload.Length,
                })
                .ToArray(),
            RecentDiffEntries = simulation.LastMonthResult?.Diff.Entries
                .Select(static entry => new DebugDiffTraceSnapshot
                {
                    ModuleKey = entry.ModuleKey,
                    Description = entry.Description,
                    EntityKey = entry.EntityKey,
                })
                .ToArray() ?? [],
            RecentDomainEvents = simulation.LastMonthResult?.DomainEvents
                .Select(static domainEvent => new DebugDomainEventSnapshot
                {
                    ModuleKey = domainEvent.ModuleKey,
                    EventType = domainEvent.EventType,
                    Summary = domainEvent.Summary,
                })
                .ToArray() ?? [],
            Warnings = warnings.ToArray(),
            Invariants = invariants.ToArray(),
        };
    }
}
