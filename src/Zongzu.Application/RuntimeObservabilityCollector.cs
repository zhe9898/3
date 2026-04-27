using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PersonRegistry;
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
        int corePersonCount = 0;
        int localPersonCount = 0;
        int regionalPersonCount = 0;
        int laborPoolCount = 0;
        int marriagePoolCount = 0;
        int migrationPoolCount = 0;
        int activeMigratingHouseholdCount = 0;
        int activeMigratingPersonCount = 0;
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
            laborPoolCount = populationState.LaborPools.Count;
            marriagePoolCount = populationState.MarriagePools.Count;
            migrationPoolCount = populationState.MigrationPools.Count;
            activeMigratingHouseholdCount = populationState.Households.Count(static household => household.IsMigrating);
            activeMigratingPersonCount = populationState.Memberships.Count(static membership => membership.Activity == PersonActivity.Migrating);
        }

        if (simulation.TryGetModuleState(KnownModuleKeys.PersonRegistry, out object? personStateObject) &&
            personStateObject is PersonRegistryState personState)
        {
            corePersonCount = personState.Persons.Count(static person => person.FidelityRing == FidelityRing.Core);
            localPersonCount = personState.Persons.Count(static person => person.FidelityRing == FidelityRing.Local);
            regionalPersonCount = personState.Persons.Count(static person => person.FidelityRing == FidelityRing.Regional);
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
            CorePersonCount = corePersonCount,
            LocalPersonCount = localPersonCount,
            RegionalPersonCount = regionalPersonCount,
            LaborPoolCount = laborPoolCount,
            MarriagePoolCount = marriagePoolCount,
            MigrationPoolCount = migrationPoolCount,
            ActiveMigratingHouseholdCount = activeMigratingHouseholdCount,
            ActiveMigratingPersonCount = activeMigratingPersonCount,
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

        int activeConflictSettlements = 0;
        int activatedResponseSettlements = 0;
        int supportedOrderSettlements = 0;
        int highSuppressionDemandSettlements = 0;
        int averageSuppressionDemand = 0;
        int peakSuppressionDemand = 0;
        int highBanditThreatSettlements = 0;
        int orderInterventionCarryoverSettlements = 0;
        int shieldingDominantSettlements = 0;
        int backlashDominantSettlements = 0;
        int orderAdministrativeAftermathSettlements = 0;

        if (simulation.TryGetModuleState(KnownModuleKeys.ConflictAndForce, out object? conflictStateObject) &&
            conflictStateObject is ConflictAndForceState conflictState)
        {
            activeConflictSettlements = conflictState.Settlements.Count(static settlement => settlement.HasActiveConflict);
            activatedResponseSettlements = conflictState.Settlements.Count(static settlement => settlement.IsResponseActivated);
            supportedOrderSettlements = conflictState.Settlements.Count(static settlement => settlement.OrderSupportLevel > 0);
        }

        if (simulation.TryGetModuleState(KnownModuleKeys.OrderAndBanditry, out object? orderStateObject) &&
            orderStateObject is OrderAndBanditryState orderState)
        {
            highSuppressionDemandSettlements = orderState.Settlements.Count(static settlement => settlement.SuppressionDemand >= 55);
            averageSuppressionDemand = orderState.Settlements.Count == 0
                ? 0
                : (int)Math.Round(orderState.Settlements.Average(static settlement => settlement.SuppressionDemand), MidpointRounding.AwayFromZero);
            peakSuppressionDemand = orderState.Settlements.Count == 0
                ? 0
                : orderState.Settlements.Max(static settlement => settlement.SuppressionDemand);
            highBanditThreatSettlements = orderState.Settlements.Count(static settlement => settlement.BanditThreat >= 55);
            orderInterventionCarryoverSettlements = orderState.Settlements.Count(static settlement => settlement.InterventionCarryoverMonths > 0);
            shieldingDominantSettlements = orderState.Settlements.Count(static settlement =>
                settlement.RouteShielding >= 35 && settlement.RouteShielding > settlement.RetaliationRisk);
            backlashDominantSettlements = orderState.Settlements.Count(static settlement =>
                settlement.RetaliationRisk >= 35 && settlement.RetaliationRisk > settlement.RouteShielding);

            if (simulation.TryGetModuleState(KnownModuleKeys.OfficeAndCareer, out object? officeStateObject) &&
                officeStateObject is OfficeAndCareerState officeState)
            {
                Dictionary<SettlementId, JurisdictionAuthorityState> jurisdictionsBySettlement = officeState.Jurisdictions
                    .ToDictionary(static jurisdiction => jurisdiction.SettlementId, static jurisdiction => jurisdiction);
                orderAdministrativeAftermathSettlements = orderState.Settlements.Count(settlement =>
                    jurisdictionsBySettlement.TryGetValue(settlement.SettlementId, out JurisdictionAuthorityState? jurisdiction)
                    && HasLinkedAdministrativeAftermath(settlement.LastInterventionCommandLabel, jurisdiction));
            }
        }

        return new InteractionPressureMetricsSnapshot
        {
            ActiveConflictSettlements = activeConflictSettlements,
            ActivatedResponseSettlements = activatedResponseSettlements,
            SupportedOrderSettlements = supportedOrderSettlements,
            HighSuppressionDemandSettlements = highSuppressionDemandSettlements,
            AverageSuppressionDemand = averageSuppressionDemand,
            PeakSuppressionDemand = peakSuppressionDemand,
            HighBanditThreatSettlements = highBanditThreatSettlements,
            OrderInterventionCarryoverSettlements = orderInterventionCarryoverSettlements,
            ShieldingDominantSettlements = shieldingDominantSettlements,
            BacklashDominantSettlements = backlashDominantSettlements,
            OrderAdministrativeAftermathSettlements = orderAdministrativeAftermathSettlements,
        };
    }

    public static SettlementPressureDistributionSnapshot CollectPressureDistribution(GameSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        SettlementInteractionHotspotSnapshot[] hotspots = BuildHotspotSnapshots(simulation);
        int crisisSettlements = 0;
        int stressedSettlements = 0;
        int watchedSettlements = 0;
        int calmSettlements = 0;
        foreach (SettlementInteractionHotspotSnapshot hotspot in hotspots)
        {
            if (hotspot.HotspotScore >= 220)
            {
                crisisSettlements += 1;
            }
            else if (hotspot.HotspotScore >= 150)
            {
                stressedSettlements += 1;
            }
            else if (hotspot.HotspotScore >= 90)
            {
                watchedSettlements += 1;
            }
            else
            {
                calmSettlements += 1;
            }
        }

        return new SettlementPressureDistributionSnapshot
        {
            CrisisSettlements = crisisSettlements,
            StressedSettlements = stressedSettlements,
            WatchedSettlements = watchedSettlements,
            CalmSettlements = calmSettlements,
        };
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

        Dictionary<SettlementId, JurisdictionAuthorityState> jurisdictionsBySettlement = new();
        if (simulation.TryGetModuleState(KnownModuleKeys.OfficeAndCareer, out object? officeStateObject) &&
            officeStateObject is OfficeAndCareerState officeState)
        {
            jurisdictionsBySettlement = officeState.Jurisdictions.ToDictionary(static jurisdiction => jurisdiction.SettlementId, static jurisdiction => jurisdiction);
        }

        return orderState.Settlements
            .Select(settlement =>
            {
                forceBySettlement.TryGetValue(settlement.SettlementId, out SettlementForceState? force);
                jurisdictionsBySettlement.TryGetValue(settlement.SettlementId, out JurisdictionAuthorityState? jurisdiction);
                int hotspotScore =
                    settlement.SuppressionDemand
                    + settlement.BanditThreat
                    + settlement.RoutePressure
                    + (settlement.BlackRoutePressure / 2)
                    + (settlement.RetaliationRisk / 3)
                    + (settlement.InterventionCarryoverMonths > 0 ? 12 : 0)
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
                    BlackRoutePressure = settlement.BlackRoutePressure,
                    RouteShielding = settlement.RouteShielding,
                    RetaliationRisk = settlement.RetaliationRisk,
                    InterventionCarryoverMonths = settlement.InterventionCarryoverMonths,
                    AdministrativeTaskLoad = jurisdiction?.AdministrativeTaskLoad ?? 0,
                    PetitionBacklog = jurisdiction?.PetitionBacklog ?? 0,
                    AdministrativeAftermathSummary = BuildAdministrativeAftermathSummary(settlement, jurisdiction),
                    ResponseActivationLevel = force?.ResponseActivationLevel ?? 0,
                    OrderSupportLevel = force?.OrderSupportLevel ?? 0,
                    HasActiveConflict = force?.HasActiveConflict ?? false,
                    IsResponseActivated = force?.IsResponseActivated ?? false,
                };
            })
            .ToArray();
    }

    private static bool HasLinkedAdministrativeAftermath(string orderCommandLabel, JurisdictionAuthorityState jurisdiction)
    {
        if (string.IsNullOrWhiteSpace(orderCommandLabel))
        {
            return false;
        }

        return jurisdiction.LastPetitionOutcome.Contains(orderCommandLabel, StringComparison.Ordinal)
            || jurisdiction.LastAdministrativeTrace.Contains(orderCommandLabel, StringComparison.Ordinal);
    }

    private static string BuildAdministrativeAftermathSummary(
        SettlementDisorderState settlement,
        JurisdictionAuthorityState? jurisdiction)
    {
        if (jurisdiction is null || !HasLinkedAdministrativeAftermath(settlement.LastInterventionCommandLabel, jurisdiction))
        {
            return string.Empty;
        }

        string leadLabel = string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
            ? jurisdiction.LeadOfficeTitle
            : jurisdiction.LeadOfficialName;
        return $"{leadLabel}仍在{jurisdiction.CurrentAdministrativeTask}；积案{jurisdiction.PetitionBacklog}，{jurisdiction.LastPetitionOutcome}";
    }
}
