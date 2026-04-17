using System;
using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Persistence;
using Zongzu.Scheduler;

namespace Zongzu.Application;

public sealed class M2DiagnosticsMonthSample
{
    public int MonthIndex { get; set; }

    public int SimulatedYear { get; set; }

    public int SimulatedMonth { get; set; }

    public ObservabilityMetricsSnapshot Metrics { get; set; } = new();

    public string ReplayHash { get; set; } = string.Empty;
}

public sealed class M2DiagnosticsReport
{
    public int DiagnosticsSchemaVersion { get; set; }

    public int Seed { get; set; }

    public int MonthsSimulated { get; set; }

    public ObservabilityMetricsSnapshot PeakMetrics { get; set; } = new();

    public ObservabilityMetricsSnapshot FinalMetrics { get; set; } = new();

    public bool RetentionLimitReached { get; set; }

    public IReadOnlyList<M2DiagnosticsMonthSample> Samples { get; set; } = [];
}

public sealed class M2DiagnosticsHarness
{
    private readonly PresentationReadModelBuilder _readModelBuilder = new();
    private readonly SaveCodec _saveCodec = new();

    public M2DiagnosticsReport Run(int seed, int monthCount)
    {
        if (monthCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthCount), "monthCount must be positive.");
        }

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(seed);
        List<M2DiagnosticsMonthSample> samples = new(monthCount);

        ObservabilityMetricsSnapshot peakMetrics = new();
        ObservabilityMetricsSnapshot finalMetrics = new();
        bool retentionLimitReached = false;

        for (int monthIndex = 1; monthIndex <= monthCount; monthIndex += 1)
        {
            SimulationMonthResult result = simulation.AdvanceOneMonth();
            int savePayloadBytes = _saveCodec.Encode(simulation.ExportSave()).Length;
            int notificationCount = _readModelBuilder.BuildForM2(simulation).Notifications.Count;

            M2DiagnosticsMonthSample sample = new()
            {
                MonthIndex = monthIndex,
                SimulatedYear = result.SimulatedDate.Year,
                SimulatedMonth = result.SimulatedDate.Month,
                Metrics = new ObservabilityMetricsSnapshot
                {
                    DiffEntryCount = result.Diff.Entries.Count,
                    DomainEventCount = result.DomainEvents.Count,
                    NotificationCount = notificationCount,
                    SavePayloadBytes = savePayloadBytes,
                },
                ReplayHash = simulation.ReplayHash,
            };

            samples.Add(sample);
            peakMetrics.DiffEntryCount = Math.Max(peakMetrics.DiffEntryCount, sample.Metrics.DiffEntryCount);
            peakMetrics.DomainEventCount = Math.Max(peakMetrics.DomainEventCount, sample.Metrics.DomainEventCount);
            peakMetrics.NotificationCount = Math.Max(peakMetrics.NotificationCount, sample.Metrics.NotificationCount);
            peakMetrics.SavePayloadBytes = Math.Max(peakMetrics.SavePayloadBytes, sample.Metrics.SavePayloadBytes);
            retentionLimitReached |= sample.Metrics.NotificationCount >= NarrativeProjectionModule.NotificationRetentionLimit;
            finalMetrics = new ObservabilityMetricsSnapshot
            {
                DiffEntryCount = sample.Metrics.DiffEntryCount,
                DomainEventCount = sample.Metrics.DomainEventCount,
                NotificationCount = sample.Metrics.NotificationCount,
                SavePayloadBytes = sample.Metrics.SavePayloadBytes,
            };
        }

        return new M2DiagnosticsReport
        {
            DiagnosticsSchemaVersion = 1,
            Seed = seed,
            MonthsSimulated = monthCount,
            PeakMetrics = peakMetrics,
            FinalMetrics = finalMetrics,
            RetentionLimitReached = retentionLimitReached,
            Samples = samples,
        };
    }
}
