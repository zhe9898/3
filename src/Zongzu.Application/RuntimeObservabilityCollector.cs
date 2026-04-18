using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Application;

internal static class RuntimeObservabilityCollector
{
    public static RuntimeScaleMetricsSnapshot CollectScaleMetrics(GameSimulation simulation, SaveRoot saveRoot, int notificationCount)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(saveRoot);

        int settlementCount = 0;
        int clanCount = 0;
        int householdCount = 0;
        int academyCount = 0;
        int routeCount = 0;

        if (simulation.TryGetModuleState(KnownModuleKeys.WorldSettlements, out object? worldStateObject) &&
            worldStateObject is WorldSettlementsState worldState)
        {
            settlementCount = worldState.Settlements.Count;
        }

        if (simulation.TryGetModuleState(KnownModuleKeys.FamilyCore, out object? familyStateObject) &&
            familyStateObject is FamilyCoreState familyState)
        {
            clanCount = familyState.Clans.Count;
        }

        if (simulation.TryGetModuleState(KnownModuleKeys.PopulationAndHouseholds, out object? populationStateObject) &&
            populationStateObject is PopulationAndHouseholdsState populationState)
        {
            householdCount = populationState.Households.Count;
        }

        if (simulation.TryGetModuleState(KnownModuleKeys.EducationAndExams, out object? educationStateObject) &&
            educationStateObject is EducationAndExamsState educationState)
        {
            academyCount = educationState.Academies.Count;
        }

        if (simulation.TryGetModuleState(KnownModuleKeys.TradeAndIndustry, out object? tradeStateObject) &&
            tradeStateObject is TradeAndIndustryState tradeState)
        {
            routeCount = tradeState.Routes.Count;
        }

        int enabledModuleCount = simulation.FeatureManifest.GetOrderedEntries()
            .Count(static pair => !string.Equals(pair.Value, "off", StringComparison.Ordinal));
        int savePayloadBytes = saveRoot.ModuleStates.Values.Sum(static envelope => envelope.Payload.Length);

        return new RuntimeScaleMetricsSnapshot
        {
            EnabledModuleCount = enabledModuleCount,
            SavedModuleCount = saveRoot.ModuleStates.Count,
            SettlementCount = settlementCount,
            ClanCount = clanCount,
            HouseholdCount = householdCount,
            AcademyCount = academyCount,
            RouteCount = routeCount,
            NotificationCount = notificationCount,
            NotificationUtilizationPercent = NarrativeProjectionModule.NotificationRetentionLimit <= 0
                ? 0
                : (notificationCount * 100) / NarrativeProjectionModule.NotificationRetentionLimit,
            SavePayloadBytesPerSettlement = settlementCount == 0 ? savePayloadBytes : savePayloadBytes / settlementCount,
            AverageHouseholdsPerSettlement = settlementCount == 0 ? householdCount : householdCount / settlementCount,
        };
    }

    public static RuntimePayloadSummarySnapshot CollectPayloadSummary(SaveRoot saveRoot)
    {
        ArgumentNullException.ThrowIfNull(saveRoot);

        if (saveRoot.ModuleStates.Count == 0)
        {
            return new RuntimePayloadSummarySnapshot();
        }

        int totalPayloadBytes = saveRoot.ModuleStates.Values.Sum(static envelope => envelope.Payload.Length);
        KeyValuePair<string, ModuleStateEnvelope> largestModule = saveRoot.ModuleStates
            .OrderByDescending(static pair => pair.Value.Payload.Length)
            .ThenBy(static pair => pair.Key, StringComparer.Ordinal)
            .First();

        return new RuntimePayloadSummarySnapshot
        {
            TotalModulePayloadBytes = totalPayloadBytes,
            LargestModuleKey = largestModule.Key,
            LargestModulePayloadBytes = largestModule.Value.Payload.Length,
            LargestModuleShareBasisPoints = totalPayloadBytes <= 0 ? 0 : (largestModule.Value.Payload.Length * 10000) / totalPayloadBytes,
        };
    }

    public static IReadOnlyList<ModulePayloadFootprintSnapshot> CollectTopPayloadModules(SaveRoot saveRoot, int takeCount = 3)
    {
        ArgumentNullException.ThrowIfNull(saveRoot);

        if (takeCount <= 0 || saveRoot.ModuleStates.Count == 0)
        {
            return [];
        }

        int totalPayloadBytes = saveRoot.ModuleStates.Values.Sum(static envelope => envelope.Payload.Length);
        if (totalPayloadBytes <= 0)
        {
            return [];
        }

        return saveRoot.ModuleStates
            .OrderByDescending(static pair => pair.Value.Payload.Length)
            .ThenBy(static pair => pair.Key, StringComparer.Ordinal)
            .Take(takeCount)
            .Select(pair => new ModulePayloadFootprintSnapshot
            {
                ModuleKey = pair.Key,
                PayloadBytes = pair.Value.Payload.Length,
                PayloadShareBasisPoints = (pair.Value.Payload.Length * 10000) / totalPayloadBytes,
            })
            .ToArray();
    }

    public static InteractionPressureMetricsSnapshot CollectInteractionPressure(GameSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        InteractionPressureMetricsSnapshot snapshot = new();

        if (simulation.TryGetModuleState(KnownModuleKeys.ConflictAndForce, out object? conflictStateObject) &&
            conflictStateObject is ConflictAndForceState conflictState)
        {
            snapshot.ActiveConflictSettlements = conflictState.Settlements.Count(static settlement => settlement.HasActiveConflict);
            snapshot.ActivatedResponseSettlements = conflictState.Settlements.Count(static settlement => settlement.IsResponseActivated);
            snapshot.SupportedOrderSettlements = conflictState.Settlements.Count(static settlement => settlement.OrderSupportLevel > 0);
        }

        if (simulation.TryGetModuleState(KnownModuleKeys.OrderAndBanditry, out object? orderStateObject) &&
            orderStateObject is OrderAndBanditryState orderState)
        {
            snapshot.HighSuppressionDemandSettlements = orderState.Settlements.Count(static settlement => settlement.SuppressionDemand >= 55);
            snapshot.AverageSuppressionDemand = orderState.Settlements.Count == 0
                ? 0
                : (int)Math.Round(orderState.Settlements.Average(static settlement => settlement.SuppressionDemand), MidpointRounding.AwayFromZero);
            snapshot.PeakSuppressionDemand = orderState.Settlements.Count == 0
                ? 0
                : orderState.Settlements.Max(static settlement => settlement.SuppressionDemand);
            snapshot.HighBanditThreatSettlements = orderState.Settlements.Count(static settlement => settlement.BanditThreat >= 55);
        }

        return snapshot;
    }

    public static SettlementPressureDistributionSnapshot CollectPressureDistribution(GameSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        SettlementInteractionHotspotSnapshot[] hotspots = BuildHotspotSnapshots(simulation);
        SettlementPressureDistributionSnapshot snapshot = new();
        foreach (SettlementInteractionHotspotSnapshot hotspot in hotspots)
        {
            if (hotspot.HotspotScore >= 220)
            {
                snapshot.CrisisSettlements += 1;
            }
            else if (hotspot.HotspotScore >= 150)
            {
                snapshot.StressedSettlements += 1;
            }
            else if (hotspot.HotspotScore >= 90)
            {
                snapshot.WatchedSettlements += 1;
            }
            else
            {
                snapshot.CalmSettlements += 1;
            }
        }

        return snapshot;
    }

    public static IReadOnlyList<SettlementInteractionHotspotSnapshot> CollectTopHotspots(GameSimulation simulation, int takeCount = 3)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        if (takeCount <= 0)
        {
            return [];
        }

        return BuildHotspotSnapshots(simulation)
            .OrderByDescending(static hotspot => hotspot.HotspotScore)
            .ThenBy(static hotspot => hotspot.SettlementName, StringComparer.Ordinal)
            .Take(takeCount)
            .ToArray();
    }

    private static SettlementInteractionHotspotSnapshot[] BuildHotspotSnapshots(GameSimulation simulation)
    {
        if (!simulation.TryGetModuleState(KnownModuleKeys.OrderAndBanditry, out object? orderStateObject) ||
            orderStateObject is not OrderAndBanditryState orderState)
        {
            return [];
        }

        Dictionary<SettlementId, string> settlementNames = new();
        if (simulation.TryGetModuleState(KnownModuleKeys.WorldSettlements, out object? worldStateObject) &&
            worldStateObject is WorldSettlementsState worldState)
        {
            settlementNames = worldState.Settlements.ToDictionary(static settlement => settlement.Id, static settlement => settlement.Name);
        }

        Dictionary<SettlementId, SettlementForceState> forceBySettlement = new();
        if (simulation.TryGetModuleState(KnownModuleKeys.ConflictAndForce, out object? conflictStateObject) &&
            conflictStateObject is ConflictAndForceState conflictState)
        {
            forceBySettlement = conflictState.Settlements.ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);
        }

        return orderState.Settlements
            .Select(settlement =>
            {
                forceBySettlement.TryGetValue(settlement.SettlementId, out SettlementForceState? force);
                int hotspotScore =
                    settlement.SuppressionDemand
                    + settlement.BanditThreat
                    + settlement.RoutePressure
                    + ((force?.HasActiveConflict ?? false) ? 20 : 0)
                    + ((force?.IsResponseActivated ?? false) ? 12 : 0)
                    + ((force?.OrderSupportLevel ?? 0) * 3);

                return new SettlementInteractionHotspotSnapshot
                {
                    SettlementId = settlement.SettlementId,
                    SettlementName = settlementNames.TryGetValue(settlement.SettlementId, out string? settlementName)
                        ? settlementName
                        : $"Settlement {settlement.SettlementId.Value}",
                    HotspotScore = hotspotScore,
                    BanditThreat = settlement.BanditThreat,
                    RoutePressure = settlement.RoutePressure,
                    SuppressionDemand = settlement.SuppressionDemand,
                    ResponseActivationLevel = force?.ResponseActivationLevel ?? 0,
                    OrderSupportLevel = force?.OrderSupportLevel ?? 0,
                    HasActiveConflict = force?.HasActiveConflict ?? false,
                    IsResponseActivated = force?.IsResponseActivated ?? false,
                };
            })
            .ToArray();
    }
}
