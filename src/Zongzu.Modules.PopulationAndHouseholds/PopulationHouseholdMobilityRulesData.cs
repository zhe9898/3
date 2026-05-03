using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed record PopulationHouseholdMobilityRulesData(
    int FocusedMemberPromotionCap,
    int MonthlyRuntimeActivePoolOutflowThreshold,
    int MonthlyRuntimeCandidateMigrationRiskFloor,
    int MonthlyRuntimeCandidateMigrationRiskCeiling,
    int MonthlyRuntimeDistressTriggerThreshold,
    int MonthlyRuntimeDebtPressureTriggerThreshold,
    int MonthlyRuntimeLaborCapacityTriggerCeiling,
    int MonthlyRuntimeGrainStoreTriggerFloor,
    int MonthlyRuntimeLandHoldingTriggerFloor,
    IReadOnlyList<LivelihoodType> MonthlyRuntimeTriggerLivelihoods,
    IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight> MonthlyRuntimeLivelihoodScoreWeights,
    int MonthlyRuntimeUnmatchedLivelihoodScoreWeight,
    int MonthlyRuntimeDistressScoreWeight,
    int MonthlyRuntimeDebtPressureScoreWeight,
    int MonthlyRuntimeMigrationRiskScoreWeight,
    int MonthlyRuntimeLaborCapacityPressureFloor,
    int MonthlyRuntimeGrainStorePressureFloor,
    int MonthlyRuntimeGrainStorePressureDivisor,
    int MonthlyRuntimeLandHoldingPressureFloor,
    int MonthlyRuntimeLandHoldingPressureDivisor,
    int MonthlyRuntimeSettlementCap,
    int MonthlyRuntimeHouseholdCap,
    PopulationHouseholdMobilityPoolTieBreakPriority MonthlyRuntimePoolTieBreakPriority,
    PopulationHouseholdMobilityHouseholdTieBreakPriority MonthlyRuntimeHouseholdTieBreakPriority,
    int MonthlyRuntimeRiskDelta,
    int MonthlyRuntimeMigrationRiskClampFloor,
    int MonthlyRuntimeMigrationRiskClampCeiling,
    int MonthlyRuntimeMigrationStatusThreshold,
    int MonthlyRuntimeMigrationStartedEventThreshold)
{
    public const int DefaultFocusedMemberPromotionCap = 2;
    public const int MaxFocusedMemberPromotionCap = 8;
    public const int DefaultMonthlyRuntimeActivePoolOutflowThreshold = 60;
    public const int DefaultMonthlyRuntimeCandidateMigrationRiskFloor = 55;
    public const int DefaultMonthlyRuntimeCandidateMigrationRiskCeiling = 80;
    public const int DefaultMonthlyRuntimeDistressTriggerThreshold = 60;
    public const int DefaultMonthlyRuntimeDebtPressureTriggerThreshold = 60;
    public const int DefaultMonthlyRuntimeLaborCapacityTriggerCeiling = 45;
    public const int DefaultMonthlyRuntimeGrainStoreTriggerFloor = 25;
    public const int DefaultMonthlyRuntimeLandHoldingTriggerFloor = 15;
    public const int DefaultMonthlyRuntimeUnmatchedLivelihoodScoreWeight = 0;
    public const int DefaultMonthlyRuntimeDistressScoreWeight = 1;
    public const int DefaultMonthlyRuntimeDebtPressureScoreWeight = 1;
    public const int DefaultMonthlyRuntimeMigrationRiskScoreWeight = 4;
    public const int DefaultMonthlyRuntimeLaborCapacityPressureFloor = 60;
    public const int DefaultMonthlyRuntimeGrainStorePressureFloor = 25;
    public const int DefaultMonthlyRuntimeGrainStorePressureDivisor = 2;
    public const int DefaultMonthlyRuntimeLandHoldingPressureFloor = 20;
    public const int DefaultMonthlyRuntimeLandHoldingPressureDivisor = 2;
    public const int DefaultMonthlyRuntimeSettlementCap = 1;
    public const int DefaultMonthlyRuntimeHouseholdCap = 2;
    public const PopulationHouseholdMobilityPoolTieBreakPriority DefaultMonthlyRuntimePoolTieBreakPriority =
        PopulationHouseholdMobilityPoolTieBreakPriority.SettlementIdAscending;
    public const PopulationHouseholdMobilityHouseholdTieBreakPriority DefaultMonthlyRuntimeHouseholdTieBreakPriority =
        PopulationHouseholdMobilityHouseholdTieBreakPriority.HouseholdIdAscending;
    public const int DefaultMonthlyRuntimeRiskDelta = 1;
    public const int DefaultMonthlyRuntimeMigrationRiskClampFloor = 0;
    public const int DefaultMonthlyRuntimeMigrationRiskClampCeiling = 100;
    public const int DefaultMonthlyRuntimeMigrationStatusThreshold = 80;
    public const int DefaultMonthlyRuntimeMigrationStartedEventThreshold = 80;
    public const int MaxMonthlyRuntimeSettlementCap = 8;
    public const int MaxMonthlyRuntimeHouseholdCap = 16;
    public const int MaxMonthlyRuntimeRiskDelta = 8;
    public const int MaxMonthlyRuntimeCandidateMigrationRiskCeiling = 100;
    public const int MaxMonthlyRuntimeLivelihoodScoreWeight = 32;
    public const int MaxMonthlyRuntimePressureScoreWeight = 8;
    public const int MaxMonthlyRuntimeMigrationRiskScoreWeight = 16;
    public const int MaxMonthlyRuntimeGrainStorePressureDivisor = 16;
    public const int MaxMonthlyRuntimeLandHoldingPressureDivisor = 16;
    public const int MaxMonthlyRuntimeMigrationRiskClampFloor = 100;
    public const int MaxMonthlyRuntimeMigrationRiskClampCeiling = 100;
    public const int MaxMonthlyRuntimeMigrationStatusThreshold = 100;
    public const int MaxMonthlyRuntimeMigrationStartedEventThreshold = 100;

    public static IReadOnlyList<LivelihoodType> DefaultMonthlyRuntimeTriggerLivelihoods { get; } =
        new[] { LivelihoodType.SeasonalMigrant, LivelihoodType.HiredLabor };

    public static IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight>
        DefaultMonthlyRuntimeLivelihoodScoreWeights { get; } =
        new[]
        {
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.SeasonalMigrant, 18),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.HiredLabor, 10),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 6),
        };

    public static PopulationHouseholdMobilityRulesData Default { get; } =
        new(
            DefaultFocusedMemberPromotionCap,
            DefaultMonthlyRuntimeActivePoolOutflowThreshold,
            DefaultMonthlyRuntimeCandidateMigrationRiskFloor,
            DefaultMonthlyRuntimeCandidateMigrationRiskCeiling,
            DefaultMonthlyRuntimeDistressTriggerThreshold,
            DefaultMonthlyRuntimeDebtPressureTriggerThreshold,
            DefaultMonthlyRuntimeLaborCapacityTriggerCeiling,
            DefaultMonthlyRuntimeGrainStoreTriggerFloor,
            DefaultMonthlyRuntimeLandHoldingTriggerFloor,
            DefaultMonthlyRuntimeTriggerLivelihoods,
            DefaultMonthlyRuntimeLivelihoodScoreWeights,
            DefaultMonthlyRuntimeUnmatchedLivelihoodScoreWeight,
            DefaultMonthlyRuntimeDistressScoreWeight,
            DefaultMonthlyRuntimeDebtPressureScoreWeight,
            DefaultMonthlyRuntimeMigrationRiskScoreWeight,
            DefaultMonthlyRuntimeLaborCapacityPressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureDivisor,
            DefaultMonthlyRuntimeLandHoldingPressureFloor,
            DefaultMonthlyRuntimeLandHoldingPressureDivisor,
            DefaultMonthlyRuntimeSettlementCap,
            DefaultMonthlyRuntimeHouseholdCap,
            DefaultMonthlyRuntimePoolTieBreakPriority,
            DefaultMonthlyRuntimeHouseholdTieBreakPriority,
            DefaultMonthlyRuntimeRiskDelta,
            DefaultMonthlyRuntimeMigrationRiskClampFloor,
            DefaultMonthlyRuntimeMigrationRiskClampCeiling,
            DefaultMonthlyRuntimeMigrationStatusThreshold,
            DefaultMonthlyRuntimeMigrationStartedEventThreshold);

    public PopulationHouseholdMobilityRulesData(int focusedMemberPromotionCap)
        : this(
            focusedMemberPromotionCap,
            DefaultMonthlyRuntimeActivePoolOutflowThreshold,
            DefaultMonthlyRuntimeCandidateMigrationRiskFloor,
            DefaultMonthlyRuntimeCandidateMigrationRiskCeiling,
            DefaultMonthlyRuntimeDistressTriggerThreshold,
            DefaultMonthlyRuntimeDebtPressureTriggerThreshold,
            DefaultMonthlyRuntimeLaborCapacityTriggerCeiling,
            DefaultMonthlyRuntimeGrainStoreTriggerFloor,
            DefaultMonthlyRuntimeLandHoldingTriggerFloor,
            DefaultMonthlyRuntimeTriggerLivelihoods,
            DefaultMonthlyRuntimeLivelihoodScoreWeights,
            DefaultMonthlyRuntimeUnmatchedLivelihoodScoreWeight,
            DefaultMonthlyRuntimeDistressScoreWeight,
            DefaultMonthlyRuntimeDebtPressureScoreWeight,
            DefaultMonthlyRuntimeMigrationRiskScoreWeight,
            DefaultMonthlyRuntimeLaborCapacityPressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureDivisor,
            DefaultMonthlyRuntimeLandHoldingPressureFloor,
            DefaultMonthlyRuntimeLandHoldingPressureDivisor,
            DefaultMonthlyRuntimeSettlementCap,
            DefaultMonthlyRuntimeHouseholdCap,
            DefaultMonthlyRuntimePoolTieBreakPriority,
            DefaultMonthlyRuntimeHouseholdTieBreakPriority,
            DefaultMonthlyRuntimeRiskDelta,
            DefaultMonthlyRuntimeMigrationRiskClampFloor,
            DefaultMonthlyRuntimeMigrationRiskClampCeiling,
            DefaultMonthlyRuntimeMigrationStatusThreshold,
            DefaultMonthlyRuntimeMigrationStartedEventThreshold)
    {
    }

    public PopulationHouseholdMobilityRulesValidationResult Validate()
    {
        List<string> errors = new();

        if (FocusedMemberPromotionCap is < 0 or > MaxFocusedMemberPromotionCap)
        {
            errors.Add(
                $"focused_member_promotion_cap must be between 0 and {MaxFocusedMemberPromotionCap}.");
        }

        if (MonthlyRuntimeActivePoolOutflowThreshold is < 0 or > 100)
        {
            errors.Add("monthly_runtime_active_pool_outflow_threshold must be between 0 and 100.");
        }

        if (MonthlyRuntimeCandidateMigrationRiskFloor is < 0 or > 100)
        {
            errors.Add("monthly_runtime_candidate_migration_risk_floor must be between 0 and 100.");
        }

        if (MonthlyRuntimeCandidateMigrationRiskCeiling is < 1 or > MaxMonthlyRuntimeCandidateMigrationRiskCeiling)
        {
            errors.Add(
                $"monthly_runtime_candidate_migration_risk_ceiling must be between 1 and {MaxMonthlyRuntimeCandidateMigrationRiskCeiling}.");
        }

        if (MonthlyRuntimeDistressTriggerThreshold is < 0 or > 100)
        {
            errors.Add("monthly_runtime_distress_trigger_threshold must be between 0 and 100.");
        }

        if (MonthlyRuntimeDebtPressureTriggerThreshold is < 0 or > 100)
        {
            errors.Add("monthly_runtime_debt_pressure_trigger_threshold must be between 0 and 100.");
        }

        if (MonthlyRuntimeLaborCapacityTriggerCeiling is < 0 or > 100)
        {
            errors.Add("monthly_runtime_labor_capacity_trigger_ceiling must be between 0 and 100.");
        }

        if (MonthlyRuntimeGrainStoreTriggerFloor is < 0 or > 100)
        {
            errors.Add("monthly_runtime_grain_store_trigger_floor must be between 0 and 100.");
        }

        if (MonthlyRuntimeLandHoldingTriggerFloor is < 0 or > 100)
        {
            errors.Add("monthly_runtime_land_holding_trigger_floor must be between 0 and 100.");
        }

        if (MonthlyRuntimeTriggerLivelihoods is null
            || MonthlyRuntimeTriggerLivelihoods.Count == 0
            || MonthlyRuntimeTriggerLivelihoods.Any(static livelihood => !Enum.IsDefined(livelihood))
            || MonthlyRuntimeTriggerLivelihoods.Distinct().Count() != MonthlyRuntimeTriggerLivelihoods.Count)
        {
            errors.Add("monthly_runtime_trigger_livelihoods must be non-empty, distinct, and defined.");
        }

        if (MonthlyRuntimeLivelihoodScoreWeights is null
            || MonthlyRuntimeLivelihoodScoreWeights.Count == 0
            || MonthlyRuntimeLivelihoodScoreWeights.Any(static entry =>
                !Enum.IsDefined(entry.Livelihood)
                || entry.Weight is < 0 or > MaxMonthlyRuntimeLivelihoodScoreWeight)
            || MonthlyRuntimeLivelihoodScoreWeights
                .Select(static entry => entry.Livelihood)
                .Distinct()
                .Count() != MonthlyRuntimeLivelihoodScoreWeights.Count)
        {
            errors.Add(
                $"monthly_runtime_livelihood_score_weights must be non-empty, distinct, defined, and between 0 and {MaxMonthlyRuntimeLivelihoodScoreWeight}.");
        }

        if (MonthlyRuntimeUnmatchedLivelihoodScoreWeight is < 0 or > MaxMonthlyRuntimeLivelihoodScoreWeight)
        {
            errors.Add(
                $"monthly_runtime_unmatched_livelihood_score_weight must be between 0 and {MaxMonthlyRuntimeLivelihoodScoreWeight}.");
        }

        if (MonthlyRuntimeDistressScoreWeight is < 0 or > MaxMonthlyRuntimePressureScoreWeight)
        {
            errors.Add(
                $"monthly_runtime_distress_score_weight must be between 0 and {MaxMonthlyRuntimePressureScoreWeight}.");
        }

        if (MonthlyRuntimeDebtPressureScoreWeight is < 0 or > MaxMonthlyRuntimePressureScoreWeight)
        {
            errors.Add(
                $"monthly_runtime_debt_pressure_score_weight must be between 0 and {MaxMonthlyRuntimePressureScoreWeight}.");
        }

        if (MonthlyRuntimeMigrationRiskScoreWeight is < 0 or > MaxMonthlyRuntimeMigrationRiskScoreWeight)
        {
            errors.Add(
                $"monthly_runtime_migration_risk_score_weight must be between 0 and {MaxMonthlyRuntimeMigrationRiskScoreWeight}.");
        }

        if (MonthlyRuntimeLaborCapacityPressureFloor is < 0 or > 100)
        {
            errors.Add("monthly_runtime_labor_capacity_pressure_floor must be between 0 and 100.");
        }

        if (MonthlyRuntimeGrainStorePressureFloor is < 0 or > 100)
        {
            errors.Add("monthly_runtime_grain_store_pressure_floor must be between 0 and 100.");
        }

        if (MonthlyRuntimeGrainStorePressureDivisor is < 1 or > MaxMonthlyRuntimeGrainStorePressureDivisor)
        {
            errors.Add(
                $"monthly_runtime_grain_store_pressure_divisor must be between 1 and {MaxMonthlyRuntimeGrainStorePressureDivisor}.");
        }

        if (MonthlyRuntimeLandHoldingPressureFloor is < 0 or > 100)
        {
            errors.Add("monthly_runtime_land_holding_pressure_floor must be between 0 and 100.");
        }

        if (MonthlyRuntimeLandHoldingPressureDivisor is < 1 or > MaxMonthlyRuntimeLandHoldingPressureDivisor)
        {
            errors.Add(
                $"monthly_runtime_land_holding_pressure_divisor must be between 1 and {MaxMonthlyRuntimeLandHoldingPressureDivisor}.");
        }

        if (MonthlyRuntimeSettlementCap is < 0 or > MaxMonthlyRuntimeSettlementCap)
        {
            errors.Add(
                $"monthly_runtime_settlement_cap must be between 0 and {MaxMonthlyRuntimeSettlementCap}.");
        }

        if (MonthlyRuntimeHouseholdCap is < 0 or > MaxMonthlyRuntimeHouseholdCap)
        {
            errors.Add(
                $"monthly_runtime_household_cap must be between 0 and {MaxMonthlyRuntimeHouseholdCap}.");
        }

        if (!Enum.IsDefined(MonthlyRuntimePoolTieBreakPriority))
        {
            errors.Add("monthly_runtime_pool_tie_break_priority must be defined.");
        }

        if (!Enum.IsDefined(MonthlyRuntimeHouseholdTieBreakPriority))
        {
            errors.Add("monthly_runtime_household_tie_break_priority must be defined.");
        }

        if (MonthlyRuntimeRiskDelta is < 0 or > MaxMonthlyRuntimeRiskDelta)
        {
            errors.Add(
                $"monthly_runtime_risk_delta must be between 0 and {MaxMonthlyRuntimeRiskDelta}.");
        }

        if (MonthlyRuntimeMigrationRiskClampFloor is < 0 or > MaxMonthlyRuntimeMigrationRiskClampFloor)
        {
            errors.Add(
                $"monthly_runtime_migration_risk_clamp_floor must be between 0 and {MaxMonthlyRuntimeMigrationRiskClampFloor}.");
        }

        if (MonthlyRuntimeMigrationRiskClampCeiling is < 1 or > MaxMonthlyRuntimeMigrationRiskClampCeiling)
        {
            errors.Add(
                $"monthly_runtime_migration_risk_clamp_ceiling must be between 1 and {MaxMonthlyRuntimeMigrationRiskClampCeiling}.");
        }

        if (MonthlyRuntimeMigrationRiskClampFloor <= MaxMonthlyRuntimeMigrationRiskClampFloor
            && MonthlyRuntimeMigrationRiskClampCeiling is >= 1 and <= MaxMonthlyRuntimeMigrationRiskClampCeiling
            && MonthlyRuntimeMigrationRiskClampFloor > MonthlyRuntimeMigrationRiskClampCeiling)
        {
            errors.Add(
                "monthly_runtime_migration_risk_clamp_floor must not exceed monthly_runtime_migration_risk_clamp_ceiling.");
        }

        if (MonthlyRuntimeMigrationStatusThreshold is < 1 or > MaxMonthlyRuntimeMigrationStatusThreshold)
        {
            errors.Add(
                $"monthly_runtime_migration_status_threshold must be between 1 and {MaxMonthlyRuntimeMigrationStatusThreshold}.");
        }

        if (MonthlyRuntimeMigrationStartedEventThreshold is < 1 or > MaxMonthlyRuntimeMigrationStartedEventThreshold)
        {
            errors.Add(
                $"monthly_runtime_migration_started_event_threshold must be between 1 and {MaxMonthlyRuntimeMigrationStartedEventThreshold}.");
        }

        return errors.Count == 0
            ? PopulationHouseholdMobilityRulesValidationResult.Valid
            : new PopulationHouseholdMobilityRulesValidationResult(false, errors);
    }

    public int GetFocusedMemberPromotionCapOrDefault()
    {
        return Validate().IsValid
            ? FocusedMemberPromotionCap
            : DefaultFocusedMemberPromotionCap;
    }

    public int GetMonthlyRuntimeActivePoolOutflowThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeActivePoolOutflowThreshold
            : DefaultMonthlyRuntimeActivePoolOutflowThreshold;
    }

    public int GetMonthlyRuntimeCandidateMigrationRiskFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeCandidateMigrationRiskFloor
            : DefaultMonthlyRuntimeCandidateMigrationRiskFloor;
    }

    public int GetMonthlyRuntimeCandidateMigrationRiskCeilingOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeCandidateMigrationRiskCeiling
            : DefaultMonthlyRuntimeCandidateMigrationRiskCeiling;
    }

    public int GetMonthlyRuntimeDistressTriggerThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeDistressTriggerThreshold
            : DefaultMonthlyRuntimeDistressTriggerThreshold;
    }

    public int GetMonthlyRuntimeDebtPressureTriggerThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeDebtPressureTriggerThreshold
            : DefaultMonthlyRuntimeDebtPressureTriggerThreshold;
    }

    public int GetMonthlyRuntimeLaborCapacityTriggerCeilingOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeLaborCapacityTriggerCeiling
            : DefaultMonthlyRuntimeLaborCapacityTriggerCeiling;
    }

    public int GetMonthlyRuntimeGrainStoreTriggerFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeGrainStoreTriggerFloor
            : DefaultMonthlyRuntimeGrainStoreTriggerFloor;
    }

    public int GetMonthlyRuntimeLandHoldingTriggerFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeLandHoldingTriggerFloor
            : DefaultMonthlyRuntimeLandHoldingTriggerFloor;
    }

    public IReadOnlyList<LivelihoodType> GetMonthlyRuntimeTriggerLivelihoodsOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeTriggerLivelihoods
            : DefaultMonthlyRuntimeTriggerLivelihoods;
    }

    public IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight> GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeLivelihoodScoreWeights
            : DefaultMonthlyRuntimeLivelihoodScoreWeights;
    }

    public int GetMonthlyRuntimeUnmatchedLivelihoodScoreWeightOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeUnmatchedLivelihoodScoreWeight
            : DefaultMonthlyRuntimeUnmatchedLivelihoodScoreWeight;
    }

    public int GetMonthlyRuntimeLivelihoodScoreWeightOrDefault(LivelihoodType livelihood)
    {
        foreach (PopulationHouseholdMobilityLivelihoodScoreWeight entry in
                 GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault())
        {
            if (entry.Livelihood == livelihood)
            {
                return entry.Weight;
            }
        }

        return GetMonthlyRuntimeUnmatchedLivelihoodScoreWeightOrDefault();
    }

    public int GetMonthlyRuntimeDistressScoreWeightOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeDistressScoreWeight
            : DefaultMonthlyRuntimeDistressScoreWeight;
    }

    public int GetMonthlyRuntimeDebtPressureScoreWeightOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeDebtPressureScoreWeight
            : DefaultMonthlyRuntimeDebtPressureScoreWeight;
    }

    public int GetMonthlyRuntimeMigrationRiskScoreWeightOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeMigrationRiskScoreWeight
            : DefaultMonthlyRuntimeMigrationRiskScoreWeight;
    }

    public int GetMonthlyRuntimeLaborCapacityPressureFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeLaborCapacityPressureFloor
            : DefaultMonthlyRuntimeLaborCapacityPressureFloor;
    }

    public int GetMonthlyRuntimeGrainStorePressureFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeGrainStorePressureFloor
            : DefaultMonthlyRuntimeGrainStorePressureFloor;
    }

    public int GetMonthlyRuntimeGrainStorePressureDivisorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeGrainStorePressureDivisor
            : DefaultMonthlyRuntimeGrainStorePressureDivisor;
    }

    public int GetMonthlyRuntimeLandHoldingPressureFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeLandHoldingPressureFloor
            : DefaultMonthlyRuntimeLandHoldingPressureFloor;
    }

    public int GetMonthlyRuntimeLandHoldingPressureDivisorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeLandHoldingPressureDivisor
            : DefaultMonthlyRuntimeLandHoldingPressureDivisor;
    }

    public int GetMonthlyRuntimeSettlementCapOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeSettlementCap
            : DefaultMonthlyRuntimeSettlementCap;
    }

    public int GetMonthlyRuntimeHouseholdCapOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeHouseholdCap
            : DefaultMonthlyRuntimeHouseholdCap;
    }

    public PopulationHouseholdMobilityPoolTieBreakPriority GetMonthlyRuntimePoolTieBreakPriorityOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimePoolTieBreakPriority
            : DefaultMonthlyRuntimePoolTieBreakPriority;
    }

    public PopulationHouseholdMobilityHouseholdTieBreakPriority GetMonthlyRuntimeHouseholdTieBreakPriorityOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeHouseholdTieBreakPriority
            : DefaultMonthlyRuntimeHouseholdTieBreakPriority;
    }

    public int GetMonthlyRuntimeRiskDeltaOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeRiskDelta
            : DefaultMonthlyRuntimeRiskDelta;
    }

    public int GetMonthlyRuntimeMigrationRiskClampFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeMigrationRiskClampFloor
            : DefaultMonthlyRuntimeMigrationRiskClampFloor;
    }

    public int GetMonthlyRuntimeMigrationRiskClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeMigrationRiskClampCeiling
            : DefaultMonthlyRuntimeMigrationRiskClampCeiling;
    }

    public int GetMonthlyRuntimeMigrationStatusThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeMigrationStatusThreshold
            : DefaultMonthlyRuntimeMigrationStatusThreshold;
    }

    public int GetMonthlyRuntimeMigrationStartedEventThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeMigrationStartedEventThreshold
            : DefaultMonthlyRuntimeMigrationStartedEventThreshold;
    }
}

public sealed record PopulationHouseholdMobilityRulesValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors)
{
    public static PopulationHouseholdMobilityRulesValidationResult Valid { get; } =
        new(true, Array.Empty<string>());
}

public readonly record struct PopulationHouseholdMobilityLivelihoodScoreWeight(
    LivelihoodType Livelihood,
    int Weight);

public enum PopulationHouseholdMobilityPoolTieBreakPriority
{
    SettlementIdAscending = 0,
}

public enum PopulationHouseholdMobilityHouseholdTieBreakPriority
{
    HouseholdIdAscending = 0,
}
