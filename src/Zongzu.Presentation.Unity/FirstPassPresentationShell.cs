using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

public static class FirstPassPresentationShell
{
    public static PresentationShellViewModel Compose(PresentationReadModelBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        NarrativeNotificationSnapshot[] notifications = bundle.Notifications
            .OrderBy(notification => notification.Tier)
            .ThenByDescending(static notification => notification.CreatedAt.Year)
            .ThenByDescending(static notification => notification.CreatedAt.Month)
            .ThenByDescending(static notification => notification.Id.Value)
            .ToArray();

        return new PresentationShellViewModel
        {
            GreatHall = BuildGreatHall(bundle, notifications),
            Lineage = BuildLineage(bundle),
            DeskSandbox = BuildDeskSandbox(bundle),
            NotificationCenter = BuildNotificationCenter(notifications),
            Debug = BuildDebugPanel(bundle.Debug),
        };
    }

    private static GreatHallDashboardViewModel BuildGreatHall(PresentationReadModelBundle bundle, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        ClanSnapshot? leadClan = bundle.Clans
            .OrderByDescending(static clan => clan.Prestige)
            .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
            .FirstOrDefault();
        int studyingCount = bundle.EducationCandidates.Count(static candidate => candidate.IsStudying);
        int passedCount = bundle.EducationCandidates.Count(static candidate => candidate.HasPassedLocalExam);
        int profitableClans = bundle.ClanTrades.Count(static trade => string.Equals(trade.LastOutcome, "Profit", StringComparison.Ordinal));

        return new GreatHallDashboardViewModel
        {
            CurrentDateLabel = $"{bundle.CurrentDate.Year}-{bundle.CurrentDate.Month:D2}",
            ReplayHash = bundle.ReplayHash,
            UrgentCount = notifications.Count(static notification => notification.Tier == NotificationTier.Urgent),
            ConsequentialCount = notifications.Count(static notification => notification.Tier == NotificationTier.Consequential),
            BackgroundCount = notifications.Count(static notification => notification.Tier == NotificationTier.Background),
            FamilySummary = leadClan is null
                ? "No clan projection available."
                : $"{leadClan.ClanName} prestige {leadClan.Prestige}, support reserve {leadClan.SupportReserve}.",
            EducationSummary = $"{studyingCount} studying, {passedCount} with local exam success.",
            TradeSummary = $"{bundle.ClanTrades.Count} trade ledgers tracked, {profitableClans} currently profitable.",
            LeadNoticeTitle = notifications.FirstOrDefault()?.Title ?? "Quiet month in the hall",
        };
    }

    private static LineageSurfaceViewModel BuildLineage(PresentationReadModelBundle bundle)
    {
        return new LineageSurfaceViewModel
        {
            Clans = bundle.Clans
                .OrderBy(static clan => clan.ClanName, StringComparer.Ordinal)
                .Select(static clan => new ClanTileViewModel
                {
                    ClanName = clan.ClanName,
                    Prestige = clan.Prestige,
                    SupportReserve = clan.SupportReserve,
                    StatusText = clan.HeirPersonId is null
                        ? "No heir projection currently surfaced."
                        : $"Heir tracked as person #{clan.HeirPersonId.Value.Value}.",
                })
                .ToArray(),
        };
    }

    private static DeskSandboxViewModel BuildDeskSandbox(PresentationReadModelBundle bundle)
    {
        Dictionary<int, PopulationSettlementSnapshot> populationBySettlement = bundle.PopulationSettlements
            .ToDictionary(static settlement => settlement.SettlementId.Value, static settlement => settlement);
        ILookup<int, AcademySnapshot> academiesBySettlement = bundle.Academies.ToLookup(static academy => academy.SettlementId.Value);
        Dictionary<int, MarketSnapshot> marketsBySettlement = bundle.Markets.ToDictionary(static market => market.SettlementId.Value, static market => market);

        return new DeskSandboxViewModel
        {
            Settlements = bundle.Settlements
                .OrderBy(static settlement => settlement.Name, StringComparer.Ordinal)
                .Select(settlement =>
                {
                    PopulationSettlementSnapshot? population = populationBySettlement.TryGetValue(settlement.Id.Value, out PopulationSettlementSnapshot? snapshot)
                        ? snapshot
                        : null;
                    AcademySnapshot[] academies = academiesBySettlement[settlement.Id.Value].OrderBy(static academy => academy.AcademyName, StringComparer.Ordinal).ToArray();
                    bool hasMarket = marketsBySettlement.TryGetValue(settlement.Id.Value, out MarketSnapshot? market);

                    return new SettlementNodeViewModel
                    {
                        SettlementName = settlement.Name,
                        Security = settlement.Security,
                        Prosperity = settlement.Prosperity,
                        AcademySummary = academies.Length == 0
                            ? "No academy projection."
                            : string.Join(", ", academies.Select(static academy => academy.AcademyName)),
                        MarketSummary = hasMarket
                            ? $"{market!.MarketName}: demand {market.Demand}, price {market.PriceIndex}, risk {market.LocalRisk}."
                            : "No market projection.",
                        PressureSummary = population is null
                            ? "No household pressure projection."
                            : $"Distress {population.CommonerDistress}, labor {population.LaborSupply}, migration {population.MigrationPressure}.",
                    };
                })
                .ToArray(),
        };
    }

    private static NotificationCenterViewModel BuildNotificationCenter(IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        return new NotificationCenterViewModel
        {
            Items = notifications
                .Select(static notification => new NotificationItemViewModel
                {
                    Title = notification.Title,
                    Summary = notification.Summary,
                    WhyItHappened = notification.WhyItHappened,
                    WhatNext = notification.WhatNext,
                    TierLabel = notification.Tier.ToString(),
                    SurfaceLabel = notification.Surface.ToString(),
                    SourceModuleKey = notification.SourceModuleKey,
                    TraceCount = notification.Traces.Count,
                })
                .ToArray(),
        };
    }

    private static DebugPanelViewModel BuildDebugPanel(PresentationDebugSnapshot debug)
    {
        return new DebugPanelViewModel
        {
            DiagnosticsSchemaLabel = $"v{debug.DiagnosticsSchemaVersion}",
            SeedLabel = debug.InitialSeed.ToString(),
            NotificationRetentionLabel = debug.NotificationRetentionLimit.ToString(),
            LatestMetrics = new DebugMetricSummaryViewModel
            {
                DiffEntryCount = debug.LatestMetrics.DiffEntryCount,
                DomainEventCount = debug.LatestMetrics.DomainEventCount,
                NotificationCount = debug.LatestMetrics.NotificationCount,
                SavePayloadBytes = debug.LatestMetrics.SavePayloadBytes,
                RetentionLimitReached = debug.RetentionLimitReached,
            },
            EnabledModules = debug.EnabledModules
                .Select(static module => $"{module.ModuleKey}:{module.Mode}")
                .ToArray(),
            ModuleInspectors = debug.ModuleInspectors
                .Select(static inspector => new DebugModuleInspectorViewModel
                {
                    ModuleKey = inspector.ModuleKey,
                    SchemaVersion = inspector.ModuleSchemaVersion,
                    PayloadBytes = inspector.PayloadBytes,
                })
                .ToArray(),
            DiffTraces = debug.RecentDiffEntries
                .Select(static trace => new DebugTraceItemViewModel
                {
                    ModuleKey = trace.ModuleKey,
                    Summary = trace.Description,
                    EntityKey = trace.EntityKey,
                })
                .ToArray(),
            DomainEvents = debug.RecentDomainEvents
                .Select(static domainEvent => new DebugEventItemViewModel
                {
                    ModuleKey = domainEvent.ModuleKey,
                    EventType = domainEvent.EventType,
                    Summary = domainEvent.Summary,
                })
                .ToArray(),
            Warnings = debug.Warnings,
            Invariants = debug.Invariants,
        };
    }
}

public sealed class PresentationShellViewModel
{
    public GreatHallDashboardViewModel GreatHall { get; set; } = new();

    public LineageSurfaceViewModel Lineage { get; set; } = new();

    public DeskSandboxViewModel DeskSandbox { get; set; } = new();

    public NotificationCenterViewModel NotificationCenter { get; set; } = new();

    public DebugPanelViewModel Debug { get; set; } = new();
}

public sealed class GreatHallDashboardViewModel
{
    public string CurrentDateLabel { get; set; } = string.Empty;

    public string ReplayHash { get; set; } = string.Empty;

    public int UrgentCount { get; set; }

    public int ConsequentialCount { get; set; }

    public int BackgroundCount { get; set; }

    public string FamilySummary { get; set; } = string.Empty;

    public string EducationSummary { get; set; } = string.Empty;

    public string TradeSummary { get; set; } = string.Empty;

    public string LeadNoticeTitle { get; set; } = string.Empty;
}

public sealed class LineageSurfaceViewModel
{
    public IReadOnlyList<ClanTileViewModel> Clans { get; set; } = [];
}

public sealed class ClanTileViewModel
{
    public string ClanName { get; set; } = string.Empty;

    public int Prestige { get; set; }

    public int SupportReserve { get; set; }

    public string StatusText { get; set; } = string.Empty;
}

public sealed class DeskSandboxViewModel
{
    public IReadOnlyList<SettlementNodeViewModel> Settlements { get; set; } = [];
}

public sealed class SettlementNodeViewModel
{
    public string SettlementName { get; set; } = string.Empty;

    public int Security { get; set; }

    public int Prosperity { get; set; }

    public string AcademySummary { get; set; } = string.Empty;

    public string MarketSummary { get; set; } = string.Empty;

    public string PressureSummary { get; set; } = string.Empty;
}

public sealed class NotificationCenterViewModel
{
    public IReadOnlyList<NotificationItemViewModel> Items { get; set; } = [];
}

public sealed class NotificationItemViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string WhyItHappened { get; set; } = string.Empty;

    public string WhatNext { get; set; } = string.Empty;

    public string TierLabel { get; set; } = string.Empty;

    public string SurfaceLabel { get; set; } = string.Empty;

    public string SourceModuleKey { get; set; } = string.Empty;

    public int TraceCount { get; set; }
}

public sealed class DebugPanelViewModel
{
    public string DiagnosticsSchemaLabel { get; set; } = string.Empty;

    public string SeedLabel { get; set; } = string.Empty;

    public string NotificationRetentionLabel { get; set; } = string.Empty;

    public DebugMetricSummaryViewModel LatestMetrics { get; set; } = new();

    public IReadOnlyList<string> EnabledModules { get; set; } = [];

    public IReadOnlyList<DebugModuleInspectorViewModel> ModuleInspectors { get; set; } = [];

    public IReadOnlyList<DebugTraceItemViewModel> DiffTraces { get; set; } = [];

    public IReadOnlyList<DebugEventItemViewModel> DomainEvents { get; set; } = [];

    public IReadOnlyList<string> Warnings { get; set; } = [];

    public IReadOnlyList<string> Invariants { get; set; } = [];
}

public sealed class DebugMetricSummaryViewModel
{
    public int DiffEntryCount { get; set; }

    public int DomainEventCount { get; set; }

    public int NotificationCount { get; set; }

    public int SavePayloadBytes { get; set; }

    public bool RetentionLimitReached { get; set; }
}

public sealed class DebugModuleInspectorViewModel
{
    public string ModuleKey { get; set; } = string.Empty;

    public int SchemaVersion { get; set; }

    public int PayloadBytes { get; set; }
}

public sealed class DebugTraceItemViewModel
{
    public string ModuleKey { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string? EntityKey { get; set; }
}

public sealed class DebugEventItemViewModel
{
    public string ModuleKey { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}
