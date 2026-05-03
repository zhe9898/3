using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private bool ApplyMonthlyHouseholdMobilityRuntimeRule(
        ModuleExecutionScope<PopulationAndHouseholdsState> scope)
    {
        int activePoolThreshold = _householdMobilityRulesData
            .GetMonthlyRuntimeActivePoolOutflowThresholdOrDefault();
        int candidateMigrationRiskFloor = _householdMobilityRulesData
            .GetMonthlyRuntimeCandidateMigrationRiskFloorOrDefault();
        int candidateMigrationRiskCeiling = _householdMobilityRulesData
            .GetMonthlyRuntimeCandidateMigrationRiskCeilingOrDefault();
        int distressTriggerThreshold = _householdMobilityRulesData
            .GetMonthlyRuntimeDistressTriggerThresholdOrDefault();
        int debtPressureTriggerThreshold = _householdMobilityRulesData
            .GetMonthlyRuntimeDebtPressureTriggerThresholdOrDefault();
        int laborCapacityTriggerCeiling = _householdMobilityRulesData
            .GetMonthlyRuntimeLaborCapacityTriggerCeilingOrDefault();
        int grainStoreTriggerFloor = _householdMobilityRulesData
            .GetMonthlyRuntimeGrainStoreTriggerFloorOrDefault();
        int landHoldingTriggerFloor = _householdMobilityRulesData
            .GetMonthlyRuntimeLandHoldingTriggerFloorOrDefault();
        IReadOnlyList<LivelihoodType> triggerLivelihoods = _householdMobilityRulesData
            .GetMonthlyRuntimeTriggerLivelihoodsOrDefault();
        IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight> livelihoodScoreWeights =
            _householdMobilityRulesData.GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault();
        int unmatchedLivelihoodScoreWeight = _householdMobilityRulesData
            .GetMonthlyRuntimeUnmatchedLivelihoodScoreWeightOrDefault();
        int distressScoreWeight = _householdMobilityRulesData.GetMonthlyRuntimeDistressScoreWeightOrDefault();
        int debtPressureScoreWeight = _householdMobilityRulesData.GetMonthlyRuntimeDebtPressureScoreWeightOrDefault();
        int migrationRiskScoreWeight = _householdMobilityRulesData
            .GetMonthlyRuntimeMigrationRiskScoreWeightOrDefault();
        int pressureContributionFloor = _householdMobilityRulesData
            .GetMonthlyRuntimePressureContributionFloorOrDefault();
        int laborCapacityPressureFloor = _householdMobilityRulesData
            .GetMonthlyRuntimeLaborCapacityPressureFloorOrDefault();
        int grainStorePressureFloor = _householdMobilityRulesData
            .GetMonthlyRuntimeGrainStorePressureFloorOrDefault();
        int grainStorePressureDivisor = _householdMobilityRulesData
            .GetMonthlyRuntimeGrainStorePressureDivisorOrDefault();
        int landHoldingPressureFloor = _householdMobilityRulesData
            .GetMonthlyRuntimeLandHoldingPressureFloorOrDefault();
        int landHoldingPressureDivisor = _householdMobilityRulesData
            .GetMonthlyRuntimeLandHoldingPressureDivisorOrDefault();
        int settlementCap = _householdMobilityRulesData.GetMonthlyRuntimeSettlementCapOrDefault();
        int householdCap = _householdMobilityRulesData.GetMonthlyRuntimeHouseholdCapOrDefault();
        PopulationHouseholdMobilityPoolTieBreakPriority poolTieBreakPriority = _householdMobilityRulesData
            .GetMonthlyRuntimePoolTieBreakPriorityOrDefault();
        PopulationHouseholdMobilityHouseholdTieBreakPriority householdTieBreakPriority = _householdMobilityRulesData
            .GetMonthlyRuntimeHouseholdTieBreakPriorityOrDefault();
        int riskDelta = _householdMobilityRulesData.GetMonthlyRuntimeRiskDeltaOrDefault();
        int migrationRiskClampFloor = _householdMobilityRulesData
            .GetMonthlyRuntimeMigrationRiskClampFloorOrDefault();
        int migrationRiskClampCeiling = _householdMobilityRulesData
            .GetMonthlyRuntimeMigrationRiskClampCeilingOrDefault();
        int migrationStatusThreshold = _householdMobilityRulesData
            .GetMonthlyRuntimeMigrationStatusThresholdOrDefault();
        int migrationStartedEventThreshold = _householdMobilityRulesData
            .GetMonthlyRuntimeMigrationStartedEventThresholdOrDefault();

        if (settlementCap <= 0
            || householdCap <= 0
            || riskDelta <= 0
            || scope.State.Households.Count == 0
            || scope.State.MigrationPools.Count == 0)
        {
            return false;
        }

        IOrderedEnumerable<MigrationPoolEntryState> orderedPools = scope.State.MigrationPools
            .Where(pool => pool.OutflowPressure >= activePoolThreshold)
            .OrderByDescending(static pool => pool.OutflowPressure);
        MigrationPoolEntryState[] activePools = ApplyMonthlyRuntimePoolTieBreak(
                orderedPools,
                poolTieBreakPriority)
            .Take(settlementCap)
            .ToArray();

        bool changed = false;
        foreach (MigrationPoolEntryState pool in activePools)
        {
            IOrderedEnumerable<PopulationHouseholdState> orderedCandidates = scope.State.Households
                .Where(household => household.SettlementId == pool.SettlementId
                    && IsMonthlyHouseholdMobilityRuntimeCandidate(
                        household,
                        candidateMigrationRiskFloor,
                        candidateMigrationRiskCeiling,
                        distressTriggerThreshold,
                        debtPressureTriggerThreshold,
                        laborCapacityTriggerCeiling,
                        grainStoreTriggerFloor,
                        landHoldingTriggerFloor,
                        triggerLivelihoods))
                .OrderByDescending(household =>
                    ComputeMonthlyHouseholdMobilityRuntimeScore(
                        household,
                        migrationRiskScoreWeight,
                        distressScoreWeight,
                        debtPressureScoreWeight,
                        pressureContributionFloor,
                        laborCapacityPressureFloor,
                        grainStorePressureFloor,
                        grainStorePressureDivisor,
                        landHoldingPressureFloor,
                        landHoldingPressureDivisor,
                        livelihoodScoreWeights,
                        unmatchedLivelihoodScoreWeight));
            PopulationHouseholdState[] candidates = ApplyMonthlyRuntimeHouseholdTieBreak(
                    orderedCandidates,
                    householdTieBreakPriority)
                .Take(householdCap)
                .ToArray();

            foreach (PopulationHouseholdState household in candidates)
            {
                int oldMigrationRisk = household.MigrationRisk;
                household.MigrationRisk = Math.Clamp(
                    household.MigrationRisk + riskDelta,
                    migrationRiskClampFloor,
                    migrationRiskClampCeiling);
                household.IsMigrating = ResolveMigrationStatus(household, migrationStatusThreshold);
                if (household.MigrationRisk == oldMigrationRisk)
                {
                    continue;
                }

                changed = true;
                scope.RecordDiff(
                    $"Household mobility pressure raised {household.HouseholdName} migration risk from {oldMigrationRisk} to {household.MigrationRisk}.",
                    household.Id.Value.ToString());

                if (oldMigrationRisk < migrationStartedEventThreshold
                    && household.MigrationRisk >= migrationStartedEventThreshold)
                {
                    scope.Emit(
                        PopulationEventNames.MigrationStarted,
                        $"{household.HouseholdName} reached the migration pressure threshold.",
                        household.Id.Value.ToString(),
                        new Dictionary<string, string>
                        {
                            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseSocialPressure,
                            [DomainEventMetadataKeys.SettlementId] = household.SettlementId.Value.ToString(),
                            [DomainEventMetadataKeys.HouseholdId] = household.Id.Value.ToString(),
                        });
                }
            }
        }

        return changed;
    }

    private static IOrderedEnumerable<MigrationPoolEntryState> ApplyMonthlyRuntimePoolTieBreak(
        IOrderedEnumerable<MigrationPoolEntryState> orderedPools,
        PopulationHouseholdMobilityPoolTieBreakPriority tieBreakPriority)
    {
        return tieBreakPriority switch
        {
            PopulationHouseholdMobilityPoolTieBreakPriority.SettlementIdAscending =>
                orderedPools.ThenBy(static pool => pool.SettlementId.Value),
            _ => orderedPools.ThenBy(static pool => pool.SettlementId.Value),
        };
    }

    private static IOrderedEnumerable<PopulationHouseholdState> ApplyMonthlyRuntimeHouseholdTieBreak(
        IOrderedEnumerable<PopulationHouseholdState> orderedCandidates,
        PopulationHouseholdMobilityHouseholdTieBreakPriority tieBreakPriority)
    {
        return tieBreakPriority switch
        {
            PopulationHouseholdMobilityHouseholdTieBreakPriority.HouseholdIdAscending =>
                orderedCandidates.ThenBy(static household => household.Id.Value),
            _ => orderedCandidates.ThenBy(static household => household.Id.Value),
        };
    }

    private static bool IsMonthlyHouseholdMobilityRuntimeCandidate(
        PopulationHouseholdState household,
        int candidateMigrationRiskFloor,
        int candidateMigrationRiskCeiling,
        int distressTriggerThreshold,
        int debtPressureTriggerThreshold,
        int laborCapacityTriggerCeiling,
        int grainStoreTriggerFloor,
        int landHoldingTriggerFloor,
        IReadOnlyList<LivelihoodType> triggerLivelihoods)
    {
        if (household.IsMigrating
            || household.MigrationRisk >= candidateMigrationRiskCeiling
            || household.MigrationRisk < candidateMigrationRiskFloor)
        {
            return false;
        }

        return household.Distress >= distressTriggerThreshold
            || household.DebtPressure >= debtPressureTriggerThreshold
            || household.LaborCapacity < laborCapacityTriggerCeiling
            || household.GrainStore < grainStoreTriggerFloor
            || household.LandHolding < landHoldingTriggerFloor
            || triggerLivelihoods.Contains(household.Livelihood);
    }

    private static int ComputeMonthlyHouseholdMobilityRuntimeScore(
        PopulationHouseholdState household,
        int migrationRiskScoreWeight,
        int distressScoreWeight,
        int debtPressureScoreWeight,
        int pressureContributionFloor,
        int laborCapacityPressureFloor,
        int grainStorePressureFloor,
        int grainStorePressureDivisor,
        int landHoldingPressureFloor,
        int landHoldingPressureDivisor,
        IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight> livelihoodScoreWeights,
        int unmatchedLivelihoodScoreWeight)
    {
        int laborPressure = Math.Max(pressureContributionFloor, laborCapacityPressureFloor - household.LaborCapacity);
        int grainPressure =
            Math.Max(pressureContributionFloor, grainStorePressureFloor - household.GrainStore) / grainStorePressureDivisor;
        int landPressure =
            Math.Max(pressureContributionFloor, landHoldingPressureFloor - household.LandHolding) / landHoldingPressureDivisor;
        int livelihoodPressure = ResolveMonthlyHouseholdMobilityLivelihoodScoreWeight(
            household.Livelihood,
            livelihoodScoreWeights,
            unmatchedLivelihoodScoreWeight);

        return (household.MigrationRisk * migrationRiskScoreWeight)
            + (household.Distress * distressScoreWeight)
            + (household.DebtPressure * debtPressureScoreWeight)
            + laborPressure
            + grainPressure
            + landPressure
            + livelihoodPressure;
    }

    private static int ResolveMonthlyHouseholdMobilityLivelihoodScoreWeight(
        LivelihoodType livelihood,
        IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight> livelihoodScoreWeights,
        int unmatchedLivelihoodScoreWeight)
    {
        foreach (PopulationHouseholdMobilityLivelihoodScoreWeight entry in livelihoodScoreWeights)
        {
            if (entry.Livelihood == livelihood)
            {
                return entry.Weight;
            }
        }

        return unmatchedLivelihoodScoreWeight;
    }
}
