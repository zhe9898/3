using System;
using System.Collections.Generic;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed record PopulationHouseholdMobilityRulesData(
    int FocusedMemberPromotionCap,
    int MonthlyRuntimeActivePoolOutflowThreshold,
    int MonthlyRuntimeCandidateMigrationRiskFloor,
    int MonthlyRuntimeMigrationRiskScoreWeight,
    int MonthlyRuntimeLaborCapacityPressureFloor,
    int MonthlyRuntimeGrainStorePressureFloor,
    int MonthlyRuntimeGrainStorePressureDivisor,
    int MonthlyRuntimeLandHoldingPressureFloor,
    int MonthlyRuntimeLandHoldingPressureDivisor,
    int MonthlyRuntimeSettlementCap,
    int MonthlyRuntimeHouseholdCap,
    int MonthlyRuntimeRiskDelta,
    int MonthlyRuntimeMigrationStartedEventThreshold)
{
    public const int DefaultFocusedMemberPromotionCap = 2;
    public const int MaxFocusedMemberPromotionCap = 8;
    public const int DefaultMonthlyRuntimeActivePoolOutflowThreshold = 60;
    public const int DefaultMonthlyRuntimeCandidateMigrationRiskFloor = 55;
    public const int DefaultMonthlyRuntimeMigrationRiskScoreWeight = 4;
    public const int DefaultMonthlyRuntimeLaborCapacityPressureFloor = 60;
    public const int DefaultMonthlyRuntimeGrainStorePressureFloor = 25;
    public const int DefaultMonthlyRuntimeGrainStorePressureDivisor = 2;
    public const int DefaultMonthlyRuntimeLandHoldingPressureFloor = 20;
    public const int DefaultMonthlyRuntimeLandHoldingPressureDivisor = 2;
    public const int DefaultMonthlyRuntimeSettlementCap = 1;
    public const int DefaultMonthlyRuntimeHouseholdCap = 2;
    public const int DefaultMonthlyRuntimeRiskDelta = 1;
    public const int DefaultMonthlyRuntimeMigrationStartedEventThreshold = 80;
    public const int MaxMonthlyRuntimeSettlementCap = 8;
    public const int MaxMonthlyRuntimeHouseholdCap = 16;
    public const int MaxMonthlyRuntimeRiskDelta = 8;
    public const int MaxMonthlyRuntimeMigrationRiskScoreWeight = 16;
    public const int MaxMonthlyRuntimeGrainStorePressureDivisor = 16;
    public const int MaxMonthlyRuntimeLandHoldingPressureDivisor = 16;
    public const int MaxMonthlyRuntimeMigrationStartedEventThreshold = 100;

    public static PopulationHouseholdMobilityRulesData Default { get; } =
        new(
            DefaultFocusedMemberPromotionCap,
            DefaultMonthlyRuntimeActivePoolOutflowThreshold,
            DefaultMonthlyRuntimeCandidateMigrationRiskFloor,
            DefaultMonthlyRuntimeMigrationRiskScoreWeight,
            DefaultMonthlyRuntimeLaborCapacityPressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureDivisor,
            DefaultMonthlyRuntimeLandHoldingPressureFloor,
            DefaultMonthlyRuntimeLandHoldingPressureDivisor,
            DefaultMonthlyRuntimeSettlementCap,
            DefaultMonthlyRuntimeHouseholdCap,
            DefaultMonthlyRuntimeRiskDelta,
            DefaultMonthlyRuntimeMigrationStartedEventThreshold);

    public PopulationHouseholdMobilityRulesData(int focusedMemberPromotionCap)
        : this(
            focusedMemberPromotionCap,
            DefaultMonthlyRuntimeActivePoolOutflowThreshold,
            DefaultMonthlyRuntimeCandidateMigrationRiskFloor,
            DefaultMonthlyRuntimeMigrationRiskScoreWeight,
            DefaultMonthlyRuntimeLaborCapacityPressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureFloor,
            DefaultMonthlyRuntimeGrainStorePressureDivisor,
            DefaultMonthlyRuntimeLandHoldingPressureFloor,
            DefaultMonthlyRuntimeLandHoldingPressureDivisor,
            DefaultMonthlyRuntimeSettlementCap,
            DefaultMonthlyRuntimeHouseholdCap,
            DefaultMonthlyRuntimeRiskDelta,
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

        if (MonthlyRuntimeRiskDelta is < 0 or > MaxMonthlyRuntimeRiskDelta)
        {
            errors.Add(
                $"monthly_runtime_risk_delta must be between 0 and {MaxMonthlyRuntimeRiskDelta}.");
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

    public int GetMonthlyRuntimeRiskDeltaOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimeRiskDelta
            : DefaultMonthlyRuntimeRiskDelta;
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
