using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed record PopulationHouseholdMobilityRulesData(
    int FocusedMemberPromotionCap,
    int MonthlyPressureProsperityDistressThreshold,
    int MonthlyPressureProsperityReliefThreshold,
    int MonthlyPressureSecurityDistressThreshold,
    int MonthlyPressureSecurityReliefThreshold,
    int MonthlyPressureClanSupportReliefThreshold,
    int MonthlyPressureDriftMinInclusive,
    int MonthlyPressureDriftMaxExclusive,
    int GrainPriceShockDefaultCurrentPrice,
    int GrainPriceShockDefaultOldPrice,
    int GrainPriceShockDefaultSupply,
    int GrainPriceShockDefaultDemand,
    int GrainPriceShockCurrentPriceClampFloor,
    int GrainPriceShockCurrentPriceClampCeiling,
    int GrainPriceShockPriceDeltaClampFloor,
    int GrainPriceShockPriceDeltaClampCeiling,
    int GrainPriceShockSupplyClampFloor,
    int GrainPriceShockSupplyClampCeiling,
    int GrainPriceShockDemandClampFloor,
    int GrainPriceShockDemandClampCeiling,
    int GrainPricePressureClampFloor,
    int GrainPricePressureClampCeiling,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> GrainPriceLevelPressureBands,
    int GrainPriceLevelPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> GrainPriceJumpPressureBands,
    int GrainPriceJumpPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> GrainPriceMarketTightnessPressureBands,
    int GrainPriceMarketTightnessPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> SubsistenceGrainBufferPressureBands,
    int SubsistenceGrainBufferPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight> SubsistenceMarketDependencyPressureScoreWeights,
    int SubsistenceMarketDependencyPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> SubsistenceLaborCapacityPressureBands,
    int SubsistenceLaborCapacityPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> SubsistenceDependentCountPressureBands,
    int SubsistenceDependentCountPressureFallbackScore,
    int SubsistenceLaborPressureClampFloor,
    int SubsistenceLaborPressureClampCeiling,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> SubsistenceFragilityDistressPressureBands,
    int SubsistenceFragilityDistressPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> SubsistenceFragilityDebtPressureBands,
    int SubsistenceFragilityDebtPressureFallbackScore,
    int SubsistenceFragilityMigrationRiskThreshold,
    int SubsistenceFragilityMigrationPressureScore,
    int SubsistenceFragilityMigrationPressureFallbackScore,
    int SubsistenceFragilityPressureClampFloor,
    int SubsistenceFragilityPressureClampCeiling,
    int SubsistenceInteractionGrainShortageStoreFloorExclusive,
    int SubsistenceInteractionGrainShortageStoreCeilingExclusive,
    int SubsistenceInteractionCashNeedBoostScore,
    int SubsistenceInteractionDebtPressureThreshold,
    int SubsistenceInteractionDebtPressureBoostScore,
    int SubsistenceInteractionResilienceReliefGrainStoreThreshold,
    int SubsistenceInteractionResilienceReliefLandHoldingThreshold,
    int SubsistenceInteractionResilienceReliefLaborCapacityThreshold,
    int SubsistenceInteractionResilienceReliefScore,
    int SubsistenceInteractionPressureClampFloor,
    int SubsistenceInteractionPressureClampCeiling,
    int SubsistencePressureEventDistressThreshold,
    int SubsistencePressureDistressDeltaClampFloor,
    int SubsistencePressureDistressDeltaClampCeiling,
    int TaxSeasonDebtDeltaClampFloor,
    int TaxSeasonDebtDeltaClampCeiling,
    int TaxSeasonDebtSpikeEventThreshold,
    int OfficialSupplyDistressDeltaClampFloor,
    int OfficialSupplyDistressDeltaClampCeiling,
    int OfficialSupplyDebtDeltaClampFloor,
    int OfficialSupplyDebtDeltaClampCeiling,
    int OfficialSupplyLaborDropClampFloor,
    int OfficialSupplyLaborDropClampCeiling,
    int OfficialSupplyMigrationDeltaClampFloor,
    int OfficialSupplyMigrationDeltaClampCeiling,
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
    int MonthlyRuntimePressureContributionFloor,
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
    public const int DefaultMonthlyPressureProsperityDistressThreshold = 50;
    public const int DefaultMonthlyPressureProsperityReliefThreshold = 60;
    public const int DefaultMonthlyPressureSecurityDistressThreshold = 45;
    public const int DefaultMonthlyPressureSecurityReliefThreshold = 55;
    public const int DefaultMonthlyPressureClanSupportReliefThreshold = 60;
    public const int DefaultMonthlyPressureDriftMinInclusive = -1;
    public const int DefaultMonthlyPressureDriftMaxExclusive = 2;
    public const int MinMonthlyPressureDriftBound = -8;
    public const int MaxMonthlyPressureDriftBound = 9;
    public const int DefaultGrainPriceShockDefaultCurrentPrice = 130;
    public const int DefaultGrainPriceShockDefaultOldPrice = 100;
    public const int DefaultGrainPriceShockDefaultSupply = 50;
    public const int DefaultGrainPriceShockDefaultDemand = 70;
    public const int DefaultGrainPriceShockCurrentPriceClampFloor = 50;
    public const int DefaultGrainPriceShockCurrentPriceClampCeiling = 200;
    public const int DefaultGrainPriceShockPriceDeltaClampFloor = 0;
    public const int DefaultGrainPriceShockPriceDeltaClampCeiling = 150;
    public const int DefaultGrainPriceShockSupplyClampFloor = 0;
    public const int DefaultGrainPriceShockSupplyClampCeiling = 100;
    public const int DefaultGrainPriceShockDemandClampFloor = 0;
    public const int DefaultGrainPriceShockDemandClampCeiling = 100;
    public const int DefaultGrainPricePressureClampFloor = 4;
    public const int DefaultGrainPricePressureClampCeiling = 14;
    public const int DefaultGrainPriceLevelPressureFallbackScore = 1;
    public const int DefaultGrainPriceJumpPressureFallbackScore = 0;
    public const int DefaultGrainPriceMarketTightnessPressureFallbackScore = 0;
    public const int DefaultSubsistenceGrainBufferPressureFallbackScore = 6;
    public const int DefaultSubsistenceMarketDependencyPressureFallbackScore = 2;
    public const int DefaultSubsistenceLaborCapacityPressureFallbackScore = 2;
    public const int DefaultSubsistenceDependentCountPressureFallbackScore = 0;
    public const int DefaultSubsistenceLaborPressureClampFloor = -2;
    public const int DefaultSubsistenceLaborPressureClampCeiling = 4;
    public const int DefaultSubsistenceFragilityDistressPressureFallbackScore = 0;
    public const int DefaultSubsistenceFragilityDebtPressureFallbackScore = 0;
    public const int DefaultSubsistenceFragilityMigrationRiskThreshold = 70;
    public const int DefaultSubsistenceFragilityMigrationPressureScore = 1;
    public const int DefaultSubsistenceFragilityMigrationPressureFallbackScore = 0;
    public const int DefaultSubsistenceFragilityPressureClampFloor = 0;
    public const int DefaultSubsistenceFragilityPressureClampCeiling = 7;
    public const int DefaultSubsistenceInteractionGrainShortageStoreFloorExclusive = 0;
    public const int DefaultSubsistenceInteractionGrainShortageStoreCeilingExclusive = 25;
    public const int DefaultSubsistenceInteractionCashNeedBoostScore = 2;
    public const int DefaultSubsistenceInteractionDebtPressureThreshold = 60;
    public const int DefaultSubsistenceInteractionDebtPressureBoostScore = 1;
    public const int DefaultSubsistenceInteractionResilienceReliefGrainStoreThreshold = 75;
    public const int DefaultSubsistenceInteractionResilienceReliefLandHoldingThreshold = 35;
    public const int DefaultSubsistenceInteractionResilienceReliefLaborCapacityThreshold = 60;
    public const int DefaultSubsistenceInteractionResilienceReliefScore = 2;
    public const int DefaultSubsistenceInteractionPressureClampFloor = -2;
    public const int DefaultSubsistenceInteractionPressureClampCeiling = 4;
    public const int DefaultSubsistencePressureEventDistressThreshold = 60;
    public const int DefaultSubsistencePressureDistressDeltaClampFloor = 4;
    public const int DefaultSubsistencePressureDistressDeltaClampCeiling = 30;
    public const int DefaultTaxSeasonDebtDeltaClampFloor = 8;
    public const int DefaultTaxSeasonDebtDeltaClampCeiling = 28;
    public const int DefaultTaxSeasonDebtSpikeEventThreshold = 70;
    public const int DefaultOfficialSupplyDistressDeltaClampFloor = 0;
    public const int DefaultOfficialSupplyDistressDeltaClampCeiling = 24;
    public const int DefaultOfficialSupplyDebtDeltaClampFloor = 0;
    public const int DefaultOfficialSupplyDebtDeltaClampCeiling = 18;
    public const int DefaultOfficialSupplyLaborDropClampFloor = 0;
    public const int DefaultOfficialSupplyLaborDropClampCeiling = 8;
    public const int DefaultOfficialSupplyMigrationDeltaClampFloor = 0;
    public const int DefaultOfficialSupplyMigrationDeltaClampCeiling = 8;
    public const int MaxGrainPriceShockPrice = 500;
    public const int MaxGrainPriceShockPriceDelta = 500;
    public const int MaxGrainPriceShockPercentage = 100;
    public const int MaxGrainPricePressure = 32;
    public const int MinSubsistenceGrainBufferPressure = -8;
    public const int MaxSubsistenceGrainBufferPressure = 8;
    public const int MaxSubsistenceGrainBufferThreshold = 100;
    public const int MinSubsistenceLaborPressure = -2;
    public const int MaxSubsistenceLaborPressure = 4;
    public const int MaxSubsistenceDependentCountPressure = 4;
    public const int MaxSubsistenceDependentCountThreshold = 32;
    public const int MaxSubsistenceFragilityPressureContribution = 4;
    public const int MinSubsistenceFragilityPressure = 0;
    public const int MaxSubsistenceFragilityPressure = 16;
    public const int MaxSubsistenceInteractionGrainStoreThreshold = 100;
    public const int MinSubsistenceInteractionPressure = -8;
    public const int MaxSubsistenceInteractionPressure = 8;
    public const int MaxSubsistenceInteractionPressureContribution = 8;
    public const int MinSubsistencePressureDistressDelta = 0;
    public const int MaxSubsistencePressureDistressDelta = 64;
    public const int MinTaxSeasonDebtDelta = 0;
    public const int MaxTaxSeasonDebtDelta = 64;
    public const int MinOfficialSupplyDistressDelta = 0;
    public const int MaxOfficialSupplyDistressDelta = 64;
    public const int MinOfficialSupplyDebtDelta = 0;
    public const int MaxOfficialSupplyDebtDelta = 64;
    public const int MinOfficialSupplyLaborDrop = 0;
    public const int MaxOfficialSupplyLaborDrop = 64;
    public const int MinOfficialSupplyMigrationDelta = 0;
    public const int MaxOfficialSupplyMigrationDelta = 64;
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
    public const int DefaultMonthlyRuntimePressureContributionFloor = 0;
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
    public const int MaxMonthlyRuntimePressureContributionFloor = 100;
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

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultGrainPriceLevelPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(170, 7),
            new PopulationHouseholdMobilityThresholdScoreBand(150, 5),
            new PopulationHouseholdMobilityThresholdScoreBand(130, 3),
            new PopulationHouseholdMobilityThresholdScoreBand(120, 2),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultGrainPriceJumpPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(45, 5),
            new PopulationHouseholdMobilityThresholdScoreBand(30, 4),
            new PopulationHouseholdMobilityThresholdScoreBand(18, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(8, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultGrainPriceMarketTightnessPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(60, 4),
            new PopulationHouseholdMobilityThresholdScoreBand(40, 3),
            new PopulationHouseholdMobilityThresholdScoreBand(20, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(8, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultSubsistenceGrainBufferPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(85, -5),
            new PopulationHouseholdMobilityThresholdScoreBand(65, -3),
            new PopulationHouseholdMobilityThresholdScoreBand(45, -1),
            new PopulationHouseholdMobilityThresholdScoreBand(25, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(1, 5),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight>
        DefaultSubsistenceMarketDependencyPressureScoreWeights { get; } =
        new[]
        {
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.PettyTrader, 4),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Boatman, 4),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Artisan, 3),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.HiredLabor, 3),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.SeasonalMigrant, 3),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.DomesticServant, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.YamenRunner, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Vagrant, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Unknown, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Smallholder, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultSubsistenceLaborCapacityPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(80, -2),
            new PopulationHouseholdMobilityThresholdScoreBand(60, -1),
            new PopulationHouseholdMobilityThresholdScoreBand(40, 0),
            new PopulationHouseholdMobilityThresholdScoreBand(25, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultSubsistenceDependentCountPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(5, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(3, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultSubsistenceFragilityDistressPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
            new PopulationHouseholdMobilityThresholdScoreBand(65, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(50, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultSubsistenceFragilityDebtPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
            new PopulationHouseholdMobilityThresholdScoreBand(65, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(50, 1),
        };

    public static PopulationHouseholdMobilityRulesData Default { get; } =
        new(
            DefaultFocusedMemberPromotionCap,
            DefaultMonthlyPressureProsperityDistressThreshold,
            DefaultMonthlyPressureProsperityReliefThreshold,
            DefaultMonthlyPressureSecurityDistressThreshold,
            DefaultMonthlyPressureSecurityReliefThreshold,
            DefaultMonthlyPressureClanSupportReliefThreshold,
            DefaultMonthlyPressureDriftMinInclusive,
            DefaultMonthlyPressureDriftMaxExclusive,
            DefaultGrainPriceShockDefaultCurrentPrice,
            DefaultGrainPriceShockDefaultOldPrice,
            DefaultGrainPriceShockDefaultSupply,
            DefaultGrainPriceShockDefaultDemand,
            DefaultGrainPriceShockCurrentPriceClampFloor,
            DefaultGrainPriceShockCurrentPriceClampCeiling,
            DefaultGrainPriceShockPriceDeltaClampFloor,
            DefaultGrainPriceShockPriceDeltaClampCeiling,
            DefaultGrainPriceShockSupplyClampFloor,
            DefaultGrainPriceShockSupplyClampCeiling,
            DefaultGrainPriceShockDemandClampFloor,
            DefaultGrainPriceShockDemandClampCeiling,
            DefaultGrainPricePressureClampFloor,
            DefaultGrainPricePressureClampCeiling,
            DefaultGrainPriceLevelPressureBands,
            DefaultGrainPriceLevelPressureFallbackScore,
            DefaultGrainPriceJumpPressureBands,
            DefaultGrainPriceJumpPressureFallbackScore,
            DefaultGrainPriceMarketTightnessPressureBands,
            DefaultGrainPriceMarketTightnessPressureFallbackScore,
            DefaultSubsistenceGrainBufferPressureBands,
            DefaultSubsistenceGrainBufferPressureFallbackScore,
            DefaultSubsistenceMarketDependencyPressureScoreWeights,
            DefaultSubsistenceMarketDependencyPressureFallbackScore,
            DefaultSubsistenceLaborCapacityPressureBands,
            DefaultSubsistenceLaborCapacityPressureFallbackScore,
            DefaultSubsistenceDependentCountPressureBands,
            DefaultSubsistenceDependentCountPressureFallbackScore,
            DefaultSubsistenceLaborPressureClampFloor,
            DefaultSubsistenceLaborPressureClampCeiling,
            DefaultSubsistenceFragilityDistressPressureBands,
            DefaultSubsistenceFragilityDistressPressureFallbackScore,
            DefaultSubsistenceFragilityDebtPressureBands,
            DefaultSubsistenceFragilityDebtPressureFallbackScore,
            DefaultSubsistenceFragilityMigrationRiskThreshold,
            DefaultSubsistenceFragilityMigrationPressureScore,
            DefaultSubsistenceFragilityMigrationPressureFallbackScore,
            DefaultSubsistenceFragilityPressureClampFloor,
            DefaultSubsistenceFragilityPressureClampCeiling,
            DefaultSubsistenceInteractionGrainShortageStoreFloorExclusive,
            DefaultSubsistenceInteractionGrainShortageStoreCeilingExclusive,
            DefaultSubsistenceInteractionCashNeedBoostScore,
            DefaultSubsistenceInteractionDebtPressureThreshold,
            DefaultSubsistenceInteractionDebtPressureBoostScore,
            DefaultSubsistenceInteractionResilienceReliefGrainStoreThreshold,
            DefaultSubsistenceInteractionResilienceReliefLandHoldingThreshold,
            DefaultSubsistenceInteractionResilienceReliefLaborCapacityThreshold,
            DefaultSubsistenceInteractionResilienceReliefScore,
            DefaultSubsistenceInteractionPressureClampFloor,
            DefaultSubsistenceInteractionPressureClampCeiling,
            DefaultSubsistencePressureEventDistressThreshold,
            DefaultSubsistencePressureDistressDeltaClampFloor,
            DefaultSubsistencePressureDistressDeltaClampCeiling,
            DefaultTaxSeasonDebtDeltaClampFloor,
            DefaultTaxSeasonDebtDeltaClampCeiling,
            DefaultTaxSeasonDebtSpikeEventThreshold,
            DefaultOfficialSupplyDistressDeltaClampFloor,
            DefaultOfficialSupplyDistressDeltaClampCeiling,
            DefaultOfficialSupplyDebtDeltaClampFloor,
            DefaultOfficialSupplyDebtDeltaClampCeiling,
            DefaultOfficialSupplyLaborDropClampFloor,
            DefaultOfficialSupplyLaborDropClampCeiling,
            DefaultOfficialSupplyMigrationDeltaClampFloor,
            DefaultOfficialSupplyMigrationDeltaClampCeiling,
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
            DefaultMonthlyRuntimePressureContributionFloor,
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
            DefaultMonthlyPressureProsperityDistressThreshold,
            DefaultMonthlyPressureProsperityReliefThreshold,
            DefaultMonthlyPressureSecurityDistressThreshold,
            DefaultMonthlyPressureSecurityReliefThreshold,
            DefaultMonthlyPressureClanSupportReliefThreshold,
            DefaultMonthlyPressureDriftMinInclusive,
            DefaultMonthlyPressureDriftMaxExclusive,
            DefaultGrainPriceShockDefaultCurrentPrice,
            DefaultGrainPriceShockDefaultOldPrice,
            DefaultGrainPriceShockDefaultSupply,
            DefaultGrainPriceShockDefaultDemand,
            DefaultGrainPriceShockCurrentPriceClampFloor,
            DefaultGrainPriceShockCurrentPriceClampCeiling,
            DefaultGrainPriceShockPriceDeltaClampFloor,
            DefaultGrainPriceShockPriceDeltaClampCeiling,
            DefaultGrainPriceShockSupplyClampFloor,
            DefaultGrainPriceShockSupplyClampCeiling,
            DefaultGrainPriceShockDemandClampFloor,
            DefaultGrainPriceShockDemandClampCeiling,
            DefaultGrainPricePressureClampFloor,
            DefaultGrainPricePressureClampCeiling,
            DefaultGrainPriceLevelPressureBands,
            DefaultGrainPriceLevelPressureFallbackScore,
            DefaultGrainPriceJumpPressureBands,
            DefaultGrainPriceJumpPressureFallbackScore,
            DefaultGrainPriceMarketTightnessPressureBands,
            DefaultGrainPriceMarketTightnessPressureFallbackScore,
            DefaultSubsistenceGrainBufferPressureBands,
            DefaultSubsistenceGrainBufferPressureFallbackScore,
            DefaultSubsistenceMarketDependencyPressureScoreWeights,
            DefaultSubsistenceMarketDependencyPressureFallbackScore,
            DefaultSubsistenceLaborCapacityPressureBands,
            DefaultSubsistenceLaborCapacityPressureFallbackScore,
            DefaultSubsistenceDependentCountPressureBands,
            DefaultSubsistenceDependentCountPressureFallbackScore,
            DefaultSubsistenceLaborPressureClampFloor,
            DefaultSubsistenceLaborPressureClampCeiling,
            DefaultSubsistenceFragilityDistressPressureBands,
            DefaultSubsistenceFragilityDistressPressureFallbackScore,
            DefaultSubsistenceFragilityDebtPressureBands,
            DefaultSubsistenceFragilityDebtPressureFallbackScore,
            DefaultSubsistenceFragilityMigrationRiskThreshold,
            DefaultSubsistenceFragilityMigrationPressureScore,
            DefaultSubsistenceFragilityMigrationPressureFallbackScore,
            DefaultSubsistenceFragilityPressureClampFloor,
            DefaultSubsistenceFragilityPressureClampCeiling,
            DefaultSubsistenceInteractionGrainShortageStoreFloorExclusive,
            DefaultSubsistenceInteractionGrainShortageStoreCeilingExclusive,
            DefaultSubsistenceInteractionCashNeedBoostScore,
            DefaultSubsistenceInteractionDebtPressureThreshold,
            DefaultSubsistenceInteractionDebtPressureBoostScore,
            DefaultSubsistenceInteractionResilienceReliefGrainStoreThreshold,
            DefaultSubsistenceInteractionResilienceReliefLandHoldingThreshold,
            DefaultSubsistenceInteractionResilienceReliefLaborCapacityThreshold,
            DefaultSubsistenceInteractionResilienceReliefScore,
            DefaultSubsistenceInteractionPressureClampFloor,
            DefaultSubsistenceInteractionPressureClampCeiling,
            DefaultSubsistencePressureEventDistressThreshold,
            DefaultSubsistencePressureDistressDeltaClampFloor,
            DefaultSubsistencePressureDistressDeltaClampCeiling,
            DefaultTaxSeasonDebtDeltaClampFloor,
            DefaultTaxSeasonDebtDeltaClampCeiling,
            DefaultTaxSeasonDebtSpikeEventThreshold,
            DefaultOfficialSupplyDistressDeltaClampFloor,
            DefaultOfficialSupplyDistressDeltaClampCeiling,
            DefaultOfficialSupplyDebtDeltaClampFloor,
            DefaultOfficialSupplyDebtDeltaClampCeiling,
            DefaultOfficialSupplyLaborDropClampFloor,
            DefaultOfficialSupplyLaborDropClampCeiling,
            DefaultOfficialSupplyMigrationDeltaClampFloor,
            DefaultOfficialSupplyMigrationDeltaClampCeiling,
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
            DefaultMonthlyRuntimePressureContributionFloor,
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

        if (MonthlyPressureProsperityDistressThreshold is < 0 or > 100)
        {
            errors.Add("monthly_pressure_prosperity_distress_threshold must be between 0 and 100.");
        }

        if (MonthlyPressureProsperityReliefThreshold is < 0 or > 100)
        {
            errors.Add("monthly_pressure_prosperity_relief_threshold must be between 0 and 100.");
        }

        if (MonthlyPressureProsperityDistressThreshold is >= 0 and <= 100
            && MonthlyPressureProsperityReliefThreshold is >= 0 and <= 100
            && MonthlyPressureProsperityDistressThreshold > MonthlyPressureProsperityReliefThreshold)
        {
            errors.Add("monthly_pressure_prosperity_distress_threshold must not exceed monthly_pressure_prosperity_relief_threshold.");
        }

        if (MonthlyPressureSecurityDistressThreshold is < 0 or > 100)
        {
            errors.Add("monthly_pressure_security_distress_threshold must be between 0 and 100.");
        }

        if (MonthlyPressureSecurityReliefThreshold is < 0 or > 100)
        {
            errors.Add("monthly_pressure_security_relief_threshold must be between 0 and 100.");
        }

        if (MonthlyPressureSecurityDistressThreshold is >= 0 and <= 100
            && MonthlyPressureSecurityReliefThreshold is >= 0 and <= 100
            && MonthlyPressureSecurityDistressThreshold > MonthlyPressureSecurityReliefThreshold)
        {
            errors.Add("monthly_pressure_security_distress_threshold must not exceed monthly_pressure_security_relief_threshold.");
        }

        if (MonthlyPressureClanSupportReliefThreshold is < 0 or > 100)
        {
            errors.Add("monthly_pressure_clan_support_relief_threshold must be between 0 and 100.");
        }

        if (MonthlyPressureDriftMinInclusive is < MinMonthlyPressureDriftBound or > MaxMonthlyPressureDriftBound)
        {
            errors.Add(
                $"monthly_pressure_drift_min_inclusive must be between {MinMonthlyPressureDriftBound} and {MaxMonthlyPressureDriftBound}.");
        }

        if (MonthlyPressureDriftMaxExclusive is < MinMonthlyPressureDriftBound or > MaxMonthlyPressureDriftBound)
        {
            errors.Add(
                $"monthly_pressure_drift_max_exclusive must be between {MinMonthlyPressureDriftBound} and {MaxMonthlyPressureDriftBound}.");
        }

        if (MonthlyPressureDriftMinInclusive >= MinMonthlyPressureDriftBound
            && MonthlyPressureDriftMinInclusive <= MaxMonthlyPressureDriftBound
            && MonthlyPressureDriftMaxExclusive >= MinMonthlyPressureDriftBound
            && MonthlyPressureDriftMaxExclusive <= MaxMonthlyPressureDriftBound
            && MonthlyPressureDriftMinInclusive >= MonthlyPressureDriftMaxExclusive)
        {
            errors.Add("monthly_pressure_drift_min_inclusive must be less than monthly_pressure_drift_max_exclusive.");
        }

        if (GrainPriceShockDefaultCurrentPrice is < 0 or > MaxGrainPriceShockPrice)
        {
            errors.Add(
                $"grain_price_shock_default_current_price must be between 0 and {MaxGrainPriceShockPrice}.");
        }

        if (GrainPriceShockDefaultOldPrice is < 0 or > MaxGrainPriceShockPrice)
        {
            errors.Add(
                $"grain_price_shock_default_old_price must be between 0 and {MaxGrainPriceShockPrice}.");
        }

        if (GrainPriceShockDefaultSupply is < 0 or > MaxGrainPriceShockPercentage)
        {
            errors.Add(
                $"grain_price_shock_default_supply must be between 0 and {MaxGrainPriceShockPercentage}.");
        }

        if (GrainPriceShockDefaultDemand is < 0 or > MaxGrainPriceShockPercentage)
        {
            errors.Add(
                $"grain_price_shock_default_demand must be between 0 and {MaxGrainPriceShockPercentage}.");
        }

        if (GrainPriceShockCurrentPriceClampFloor is < 0 or > MaxGrainPriceShockPrice)
        {
            errors.Add(
                $"grain_price_shock_current_price_clamp_floor must be between 0 and {MaxGrainPriceShockPrice}.");
        }

        if (GrainPriceShockCurrentPriceClampCeiling is < 0 or > MaxGrainPriceShockPrice)
        {
            errors.Add(
                $"grain_price_shock_current_price_clamp_ceiling must be between 0 and {MaxGrainPriceShockPrice}.");
        }

        if (GrainPriceShockCurrentPriceClampFloor is >= 0 and <= MaxGrainPriceShockPrice
            && GrainPriceShockCurrentPriceClampCeiling is >= 0 and <= MaxGrainPriceShockPrice
            && GrainPriceShockCurrentPriceClampFloor > GrainPriceShockCurrentPriceClampCeiling)
        {
            errors.Add("grain_price_shock_current_price_clamp_floor must not exceed grain_price_shock_current_price_clamp_ceiling.");
        }

        if (GrainPriceShockDefaultCurrentPrice is >= 0 and <= MaxGrainPriceShockPrice
            && GrainPriceShockCurrentPriceClampFloor is >= 0 and <= MaxGrainPriceShockPrice
            && GrainPriceShockCurrentPriceClampCeiling is >= 0 and <= MaxGrainPriceShockPrice
            && GrainPriceShockCurrentPriceClampFloor <= GrainPriceShockCurrentPriceClampCeiling
            && (GrainPriceShockDefaultCurrentPrice < GrainPriceShockCurrentPriceClampFloor
                || GrainPriceShockDefaultCurrentPrice > GrainPriceShockCurrentPriceClampCeiling))
        {
            errors.Add("grain_price_shock_default_current_price must stay within the current price clamp range.");
        }

        if (GrainPriceShockPriceDeltaClampFloor is < 0 or > MaxGrainPriceShockPriceDelta)
        {
            errors.Add(
                $"grain_price_shock_price_delta_clamp_floor must be between 0 and {MaxGrainPriceShockPriceDelta}.");
        }

        if (GrainPriceShockPriceDeltaClampCeiling is < 0 or > MaxGrainPriceShockPriceDelta)
        {
            errors.Add(
                $"grain_price_shock_price_delta_clamp_ceiling must be between 0 and {MaxGrainPriceShockPriceDelta}.");
        }

        if (GrainPriceShockPriceDeltaClampFloor is >= 0 and <= MaxGrainPriceShockPriceDelta
            && GrainPriceShockPriceDeltaClampCeiling is >= 0 and <= MaxGrainPriceShockPriceDelta
            && GrainPriceShockPriceDeltaClampFloor > GrainPriceShockPriceDeltaClampCeiling)
        {
            errors.Add("grain_price_shock_price_delta_clamp_floor must not exceed grain_price_shock_price_delta_clamp_ceiling.");
        }

        if (GrainPriceShockSupplyClampFloor is < 0 or > MaxGrainPriceShockPercentage)
        {
            errors.Add(
                $"grain_price_shock_supply_clamp_floor must be between 0 and {MaxGrainPriceShockPercentage}.");
        }

        if (GrainPriceShockSupplyClampCeiling is < 0 or > MaxGrainPriceShockPercentage)
        {
            errors.Add(
                $"grain_price_shock_supply_clamp_ceiling must be between 0 and {MaxGrainPriceShockPercentage}.");
        }

        if (GrainPriceShockSupplyClampFloor is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockSupplyClampCeiling is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockSupplyClampFloor > GrainPriceShockSupplyClampCeiling)
        {
            errors.Add("grain_price_shock_supply_clamp_floor must not exceed grain_price_shock_supply_clamp_ceiling.");
        }

        if (GrainPriceShockDefaultSupply is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockSupplyClampFloor is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockSupplyClampCeiling is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockSupplyClampFloor <= GrainPriceShockSupplyClampCeiling
            && (GrainPriceShockDefaultSupply < GrainPriceShockSupplyClampFloor
                || GrainPriceShockDefaultSupply > GrainPriceShockSupplyClampCeiling))
        {
            errors.Add("grain_price_shock_default_supply must stay within the supply clamp range.");
        }

        if (GrainPriceShockDemandClampFloor is < 0 or > MaxGrainPriceShockPercentage)
        {
            errors.Add(
                $"grain_price_shock_demand_clamp_floor must be between 0 and {MaxGrainPriceShockPercentage}.");
        }

        if (GrainPriceShockDemandClampCeiling is < 0 or > MaxGrainPriceShockPercentage)
        {
            errors.Add(
                $"grain_price_shock_demand_clamp_ceiling must be between 0 and {MaxGrainPriceShockPercentage}.");
        }

        if (GrainPriceShockDemandClampFloor is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockDemandClampCeiling is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockDemandClampFloor > GrainPriceShockDemandClampCeiling)
        {
            errors.Add("grain_price_shock_demand_clamp_floor must not exceed grain_price_shock_demand_clamp_ceiling.");
        }

        if (GrainPriceShockDefaultDemand is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockDemandClampFloor is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockDemandClampCeiling is >= 0 and <= MaxGrainPriceShockPercentage
            && GrainPriceShockDemandClampFloor <= GrainPriceShockDemandClampCeiling
            && (GrainPriceShockDefaultDemand < GrainPriceShockDemandClampFloor
                || GrainPriceShockDefaultDemand > GrainPriceShockDemandClampCeiling))
        {
            errors.Add("grain_price_shock_default_demand must stay within the demand clamp range.");
        }

        if (GrainPricePressureClampFloor is < 0 or > MaxGrainPricePressure)
        {
            errors.Add(
                $"grain_price_pressure_clamp_floor must be between 0 and {MaxGrainPricePressure}.");
        }

        if (GrainPricePressureClampCeiling is < 0 or > MaxGrainPricePressure)
        {
            errors.Add(
                $"grain_price_pressure_clamp_ceiling must be between 0 and {MaxGrainPricePressure}.");
        }

        if (GrainPricePressureClampFloor is >= 0 and <= MaxGrainPricePressure
            && GrainPricePressureClampCeiling is >= 0 and <= MaxGrainPricePressure
            && GrainPricePressureClampFloor > GrainPricePressureClampCeiling)
        {
            errors.Add("grain_price_pressure_clamp_floor must not exceed grain_price_pressure_clamp_ceiling.");
        }

        if (GrainPriceLevelPressureBands is null
            || GrainPriceLevelPressureBands.Count == 0
            || GrainPriceLevelPressureBands.Any(static band =>
                band.Threshold is < 0 or > MaxGrainPriceShockPrice
                || band.Score is < 0 or > MaxGrainPricePressure)
            || GrainPriceLevelPressureBands.Select(static band => band.Threshold).Distinct().Count() != GrainPriceLevelPressureBands.Count)
        {
            errors.Add(
                $"grain_price_level_pressure_bands must be non-empty, distinct, and between threshold 0..{MaxGrainPriceShockPrice} and score 0..{MaxGrainPricePressure}.");
        }

        if (GrainPriceLevelPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < GrainPriceLevelPressureBands.Count; index++)
            {
                if (GrainPriceLevelPressureBands[index - 1].Threshold <= GrainPriceLevelPressureBands[index].Threshold)
                {
                    errors.Add("grain_price_level_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (GrainPriceLevelPressureFallbackScore is < 0 or > MaxGrainPricePressure)
        {
            errors.Add(
                $"grain_price_level_pressure_fallback_score must be between 0 and {MaxGrainPricePressure}.");
        }

        if (GrainPriceJumpPressureBands is null
            || GrainPriceJumpPressureBands.Count == 0
            || GrainPriceJumpPressureBands.Any(static band =>
                band.Threshold is < 0 or > MaxGrainPriceShockPriceDelta
                || band.Score is < 0 or > MaxGrainPricePressure)
            || GrainPriceJumpPressureBands.Select(static band => band.Threshold).Distinct().Count() != GrainPriceJumpPressureBands.Count)
        {
            errors.Add(
                $"grain_price_jump_pressure_bands must be non-empty, distinct, and between threshold 0..{MaxGrainPriceShockPriceDelta} and score 0..{MaxGrainPricePressure}.");
        }

        if (GrainPriceJumpPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < GrainPriceJumpPressureBands.Count; index++)
            {
                if (GrainPriceJumpPressureBands[index - 1].Threshold <= GrainPriceJumpPressureBands[index].Threshold)
                {
                    errors.Add("grain_price_jump_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (GrainPriceJumpPressureFallbackScore is < 0 or > MaxGrainPricePressure)
        {
            errors.Add(
                $"grain_price_jump_pressure_fallback_score must be between 0 and {MaxGrainPricePressure}.");
        }

        if (GrainPriceMarketTightnessPressureBands is null
            || GrainPriceMarketTightnessPressureBands.Count == 0
            || GrainPriceMarketTightnessPressureBands.Any(static band =>
                band.Threshold is < 0 or > MaxGrainPriceShockPercentage
                || band.Score is < 0 or > MaxGrainPricePressure)
            || GrainPriceMarketTightnessPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != GrainPriceMarketTightnessPressureBands.Count)
        {
            errors.Add(
                $"grain_price_market_tightness_pressure_bands must be non-empty, distinct, and between threshold 0..{MaxGrainPriceShockPercentage} and score 0..{MaxGrainPricePressure}.");
        }

        if (GrainPriceMarketTightnessPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < GrainPriceMarketTightnessPressureBands.Count; index++)
            {
                if (GrainPriceMarketTightnessPressureBands[index - 1].Threshold <= GrainPriceMarketTightnessPressureBands[index].Threshold)
                {
                    errors.Add("grain_price_market_tightness_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (GrainPriceMarketTightnessPressureFallbackScore is < 0 or > MaxGrainPricePressure)
        {
            errors.Add(
                $"grain_price_market_tightness_pressure_fallback_score must be between 0 and {MaxGrainPricePressure}.");
        }

        if (SubsistenceGrainBufferPressureBands is null
            || SubsistenceGrainBufferPressureBands.Count == 0
            || SubsistenceGrainBufferPressureBands.Any(static band =>
                band.Threshold is < 0 or > MaxSubsistenceGrainBufferThreshold
                || band.Score is < MinSubsistenceGrainBufferPressure or > MaxSubsistenceGrainBufferPressure)
            || SubsistenceGrainBufferPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != SubsistenceGrainBufferPressureBands.Count)
        {
            errors.Add(
                $"subsistence_grain_buffer_pressure_bands must be non-empty, distinct, and between threshold 0..{MaxSubsistenceGrainBufferThreshold} and score {MinSubsistenceGrainBufferPressure}..{MaxSubsistenceGrainBufferPressure}.");
        }

        if (SubsistenceGrainBufferPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < SubsistenceGrainBufferPressureBands.Count; index++)
            {
                if (SubsistenceGrainBufferPressureBands[index - 1].Threshold <= SubsistenceGrainBufferPressureBands[index].Threshold)
                {
                    errors.Add("subsistence_grain_buffer_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (SubsistenceGrainBufferPressureFallbackScore is < MinSubsistenceGrainBufferPressure or > MaxSubsistenceGrainBufferPressure)
        {
            errors.Add(
                $"subsistence_grain_buffer_pressure_fallback_score must be between {MinSubsistenceGrainBufferPressure} and {MaxSubsistenceGrainBufferPressure}.");
        }

        if (SubsistenceMarketDependencyPressureScoreWeights is null
            || SubsistenceMarketDependencyPressureScoreWeights.Count == 0
            || SubsistenceMarketDependencyPressureScoreWeights.Any(static entry =>
                !Enum.IsDefined(entry.Livelihood)
                || entry.Weight is < 0 or > MaxGrainPricePressure)
            || SubsistenceMarketDependencyPressureScoreWeights
                .Select(static entry => entry.Livelihood)
                .Distinct()
                .Count() != SubsistenceMarketDependencyPressureScoreWeights.Count)
        {
            errors.Add(
                $"subsistence_market_dependency_pressure_score_weights must be non-empty, distinct, defined, and between 0 and {MaxGrainPricePressure}.");
        }

        if (SubsistenceMarketDependencyPressureFallbackScore is < 0 or > MaxGrainPricePressure)
        {
            errors.Add(
                $"subsistence_market_dependency_pressure_fallback_score must be between 0 and {MaxGrainPricePressure}.");
        }

        if (SubsistenceLaborCapacityPressureBands is null
            || SubsistenceLaborCapacityPressureBands.Count == 0
            || SubsistenceLaborCapacityPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < MinSubsistenceLaborPressure or > MaxSubsistenceLaborPressure)
            || SubsistenceLaborCapacityPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != SubsistenceLaborCapacityPressureBands.Count)
        {
            errors.Add(
                $"subsistence_labor_capacity_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score {MinSubsistenceLaborPressure}..{MaxSubsistenceLaborPressure}.");
        }

        if (SubsistenceLaborCapacityPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < SubsistenceLaborCapacityPressureBands.Count; index++)
            {
                if (SubsistenceLaborCapacityPressureBands[index - 1].Threshold <= SubsistenceLaborCapacityPressureBands[index].Threshold)
                {
                    errors.Add("subsistence_labor_capacity_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (SubsistenceLaborCapacityPressureFallbackScore is < MinSubsistenceLaborPressure or > MaxSubsistenceLaborPressure)
        {
            errors.Add(
                $"subsistence_labor_capacity_pressure_fallback_score must be between {MinSubsistenceLaborPressure} and {MaxSubsistenceLaborPressure}.");
        }

        if (SubsistenceDependentCountPressureBands is null
            || SubsistenceDependentCountPressureBands.Count == 0
            || SubsistenceDependentCountPressureBands.Any(static band =>
                band.Threshold is < 0 or > MaxSubsistenceDependentCountThreshold
                || band.Score is < 0 or > MaxSubsistenceDependentCountPressure)
            || SubsistenceDependentCountPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != SubsistenceDependentCountPressureBands.Count)
        {
            errors.Add(
                $"subsistence_dependent_count_pressure_bands must be non-empty, distinct, and between threshold 0..{MaxSubsistenceDependentCountThreshold} and score 0..{MaxSubsistenceDependentCountPressure}.");
        }

        if (SubsistenceDependentCountPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < SubsistenceDependentCountPressureBands.Count; index++)
            {
                if (SubsistenceDependentCountPressureBands[index - 1].Threshold <= SubsistenceDependentCountPressureBands[index].Threshold)
                {
                    errors.Add("subsistence_dependent_count_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (SubsistenceDependentCountPressureFallbackScore is < 0 or > MaxSubsistenceDependentCountPressure)
        {
            errors.Add(
                $"subsistence_dependent_count_pressure_fallback_score must be between 0 and {MaxSubsistenceDependentCountPressure}.");
        }

        if (SubsistenceLaborPressureClampFloor is < MinSubsistenceLaborPressure or > MaxSubsistenceLaborPressure)
        {
            errors.Add(
                $"subsistence_labor_pressure_clamp_floor must be between {MinSubsistenceLaborPressure} and {MaxSubsistenceLaborPressure}.");
        }

        if (SubsistenceLaborPressureClampCeiling is < MinSubsistenceLaborPressure or > MaxSubsistenceLaborPressure)
        {
            errors.Add(
                $"subsistence_labor_pressure_clamp_ceiling must be between {MinSubsistenceLaborPressure} and {MaxSubsistenceLaborPressure}.");
        }

        if (SubsistenceLaborPressureClampFloor > SubsistenceLaborPressureClampCeiling)
        {
            errors.Add("subsistence_labor_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (SubsistenceFragilityDistressPressureBands is null
            || SubsistenceFragilityDistressPressureBands.Count == 0
            || SubsistenceFragilityDistressPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < 0 or > MaxSubsistenceFragilityPressureContribution)
            || SubsistenceFragilityDistressPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != SubsistenceFragilityDistressPressureBands.Count)
        {
            errors.Add(
                $"subsistence_fragility_distress_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score 0..{MaxSubsistenceFragilityPressureContribution}.");
        }

        if (SubsistenceFragilityDistressPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < SubsistenceFragilityDistressPressureBands.Count; index++)
            {
                if (SubsistenceFragilityDistressPressureBands[index - 1].Threshold <= SubsistenceFragilityDistressPressureBands[index].Threshold)
                {
                    errors.Add("subsistence_fragility_distress_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (SubsistenceFragilityDistressPressureFallbackScore is < 0 or > MaxSubsistenceFragilityPressureContribution)
        {
            errors.Add(
                $"subsistence_fragility_distress_pressure_fallback_score must be between 0 and {MaxSubsistenceFragilityPressureContribution}.");
        }

        if (SubsistenceFragilityDebtPressureBands is null
            || SubsistenceFragilityDebtPressureBands.Count == 0
            || SubsistenceFragilityDebtPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < 0 or > MaxSubsistenceFragilityPressureContribution)
            || SubsistenceFragilityDebtPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != SubsistenceFragilityDebtPressureBands.Count)
        {
            errors.Add(
                $"subsistence_fragility_debt_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score 0..{MaxSubsistenceFragilityPressureContribution}.");
        }

        if (SubsistenceFragilityDebtPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < SubsistenceFragilityDebtPressureBands.Count; index++)
            {
                if (SubsistenceFragilityDebtPressureBands[index - 1].Threshold <= SubsistenceFragilityDebtPressureBands[index].Threshold)
                {
                    errors.Add("subsistence_fragility_debt_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (SubsistenceFragilityDebtPressureFallbackScore is < 0 or > MaxSubsistenceFragilityPressureContribution)
        {
            errors.Add(
                $"subsistence_fragility_debt_pressure_fallback_score must be between 0 and {MaxSubsistenceFragilityPressureContribution}.");
        }

        if (SubsistenceFragilityMigrationRiskThreshold is < 0 or > 100)
        {
            errors.Add("subsistence_fragility_migration_risk_threshold must be between 0 and 100.");
        }

        if (SubsistenceFragilityMigrationPressureScore is < 0 or > MaxSubsistenceFragilityPressureContribution)
        {
            errors.Add(
                $"subsistence_fragility_migration_pressure_score must be between 0 and {MaxSubsistenceFragilityPressureContribution}.");
        }

        if (SubsistenceFragilityMigrationPressureFallbackScore is < 0 or > MaxSubsistenceFragilityPressureContribution)
        {
            errors.Add(
                $"subsistence_fragility_migration_pressure_fallback_score must be between 0 and {MaxSubsistenceFragilityPressureContribution}.");
        }

        if (SubsistenceFragilityPressureClampFloor is < MinSubsistenceFragilityPressure or > MaxSubsistenceFragilityPressure)
        {
            errors.Add(
                $"subsistence_fragility_pressure_clamp_floor must be between {MinSubsistenceFragilityPressure} and {MaxSubsistenceFragilityPressure}.");
        }

        if (SubsistenceFragilityPressureClampCeiling is < MinSubsistenceFragilityPressure or > MaxSubsistenceFragilityPressure)
        {
            errors.Add(
                $"subsistence_fragility_pressure_clamp_ceiling must be between {MinSubsistenceFragilityPressure} and {MaxSubsistenceFragilityPressure}.");
        }

        if (SubsistenceFragilityPressureClampFloor > SubsistenceFragilityPressureClampCeiling)
        {
            errors.Add("subsistence_fragility_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (SubsistenceInteractionGrainShortageStoreFloorExclusive is < 0 or > MaxSubsistenceInteractionGrainStoreThreshold)
        {
            errors.Add(
                $"subsistence_interaction_grain_shortage_store_floor_exclusive must be between 0 and {MaxSubsistenceInteractionGrainStoreThreshold}.");
        }

        if (SubsistenceInteractionGrainShortageStoreCeilingExclusive is < 0 or > MaxSubsistenceInteractionGrainStoreThreshold)
        {
            errors.Add(
                $"subsistence_interaction_grain_shortage_store_ceiling_exclusive must be between 0 and {MaxSubsistenceInteractionGrainStoreThreshold}.");
        }

        if (SubsistenceInteractionGrainShortageStoreFloorExclusive >= SubsistenceInteractionGrainShortageStoreCeilingExclusive)
        {
            errors.Add(
                "subsistence_interaction_grain_shortage_store_floor_exclusive must be less than ceiling_exclusive.");
        }

        if (SubsistenceInteractionCashNeedBoostScore is < 0 or > MaxSubsistenceInteractionPressureContribution)
        {
            errors.Add(
                $"subsistence_interaction_cash_need_boost_score must be between 0 and {MaxSubsistenceInteractionPressureContribution}.");
        }

        if (SubsistenceInteractionDebtPressureThreshold is < 0 or > 100)
        {
            errors.Add("subsistence_interaction_debt_pressure_threshold must be between 0 and 100.");
        }

        if (SubsistenceInteractionDebtPressureBoostScore is < 0 or > MaxSubsistenceInteractionPressureContribution)
        {
            errors.Add(
                $"subsistence_interaction_debt_pressure_boost_score must be between 0 and {MaxSubsistenceInteractionPressureContribution}.");
        }

        if (SubsistenceInteractionResilienceReliefGrainStoreThreshold is < 0 or > 100)
        {
            errors.Add("subsistence_interaction_resilience_relief_grain_store_threshold must be between 0 and 100.");
        }

        if (SubsistenceInteractionResilienceReliefLandHoldingThreshold is < 0 or > 100)
        {
            errors.Add("subsistence_interaction_resilience_relief_land_holding_threshold must be between 0 and 100.");
        }

        if (SubsistenceInteractionResilienceReliefLaborCapacityThreshold is < 0 or > 100)
        {
            errors.Add("subsistence_interaction_resilience_relief_labor_capacity_threshold must be between 0 and 100.");
        }

        if (SubsistenceInteractionResilienceReliefScore is < 0 or > MaxSubsistenceInteractionPressureContribution)
        {
            errors.Add(
                $"subsistence_interaction_resilience_relief_score must be between 0 and {MaxSubsistenceInteractionPressureContribution}.");
        }

        if (SubsistenceInteractionPressureClampFloor is < MinSubsistenceInteractionPressure or > MaxSubsistenceInteractionPressure)
        {
            errors.Add(
                $"subsistence_interaction_pressure_clamp_floor must be between {MinSubsistenceInteractionPressure} and {MaxSubsistenceInteractionPressure}.");
        }

        if (SubsistenceInteractionPressureClampCeiling is < MinSubsistenceInteractionPressure or > MaxSubsistenceInteractionPressure)
        {
            errors.Add(
                $"subsistence_interaction_pressure_clamp_ceiling must be between {MinSubsistenceInteractionPressure} and {MaxSubsistenceInteractionPressure}.");
        }

        if (SubsistenceInteractionPressureClampFloor > SubsistenceInteractionPressureClampCeiling)
        {
            errors.Add("subsistence_interaction_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (SubsistencePressureEventDistressThreshold is < 0 or > 100)
        {
            errors.Add("subsistence_pressure_event_distress_threshold must be between 0 and 100.");
        }

        if (SubsistencePressureDistressDeltaClampFloor is < MinSubsistencePressureDistressDelta or > MaxSubsistencePressureDistressDelta)
        {
            errors.Add(
                $"subsistence_pressure_distress_delta_clamp_floor must be between {MinSubsistencePressureDistressDelta} and {MaxSubsistencePressureDistressDelta}.");
        }

        if (SubsistencePressureDistressDeltaClampCeiling is < MinSubsistencePressureDistressDelta or > MaxSubsistencePressureDistressDelta)
        {
            errors.Add(
                $"subsistence_pressure_distress_delta_clamp_ceiling must be between {MinSubsistencePressureDistressDelta} and {MaxSubsistencePressureDistressDelta}.");
        }

        if (SubsistencePressureDistressDeltaClampFloor > SubsistencePressureDistressDeltaClampCeiling)
        {
            errors.Add("subsistence_pressure_distress_delta_clamp_floor must be less than or equal to ceiling.");
        }

        if (TaxSeasonDebtDeltaClampFloor is < MinTaxSeasonDebtDelta or > MaxTaxSeasonDebtDelta)
        {
            errors.Add(
                $"tax_season_debt_delta_clamp_floor must be between {MinTaxSeasonDebtDelta} and {MaxTaxSeasonDebtDelta}.");
        }

        if (TaxSeasonDebtDeltaClampCeiling is < MinTaxSeasonDebtDelta or > MaxTaxSeasonDebtDelta)
        {
            errors.Add(
                $"tax_season_debt_delta_clamp_ceiling must be between {MinTaxSeasonDebtDelta} and {MaxTaxSeasonDebtDelta}.");
        }

        if (TaxSeasonDebtDeltaClampFloor > TaxSeasonDebtDeltaClampCeiling)
        {
            errors.Add("tax_season_debt_delta_clamp_floor must be less than or equal to ceiling.");
        }

        if (TaxSeasonDebtSpikeEventThreshold is < 0 or > 100)
        {
            errors.Add("tax_season_debt_spike_event_threshold must be between 0 and 100.");
        }

        if (OfficialSupplyDistressDeltaClampFloor is < MinOfficialSupplyDistressDelta or > MaxOfficialSupplyDistressDelta)
        {
            errors.Add(
                $"official_supply_distress_delta_clamp_floor must be between {MinOfficialSupplyDistressDelta} and {MaxOfficialSupplyDistressDelta}.");
        }

        if (OfficialSupplyDistressDeltaClampCeiling is < MinOfficialSupplyDistressDelta or > MaxOfficialSupplyDistressDelta)
        {
            errors.Add(
                $"official_supply_distress_delta_clamp_ceiling must be between {MinOfficialSupplyDistressDelta} and {MaxOfficialSupplyDistressDelta}.");
        }

        if (OfficialSupplyDistressDeltaClampFloor > OfficialSupplyDistressDeltaClampCeiling)
        {
            errors.Add("official_supply_distress_delta_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyDebtDeltaClampFloor is < MinOfficialSupplyDebtDelta or > MaxOfficialSupplyDebtDelta)
        {
            errors.Add(
                $"official_supply_debt_delta_clamp_floor must be between {MinOfficialSupplyDebtDelta} and {MaxOfficialSupplyDebtDelta}.");
        }

        if (OfficialSupplyDebtDeltaClampCeiling is < MinOfficialSupplyDebtDelta or > MaxOfficialSupplyDebtDelta)
        {
            errors.Add(
                $"official_supply_debt_delta_clamp_ceiling must be between {MinOfficialSupplyDebtDelta} and {MaxOfficialSupplyDebtDelta}.");
        }

        if (OfficialSupplyDebtDeltaClampFloor > OfficialSupplyDebtDeltaClampCeiling)
        {
            errors.Add("official_supply_debt_delta_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyLaborDropClampFloor is < MinOfficialSupplyLaborDrop or > MaxOfficialSupplyLaborDrop)
        {
            errors.Add(
                $"official_supply_labor_drop_clamp_floor must be between {MinOfficialSupplyLaborDrop} and {MaxOfficialSupplyLaborDrop}.");
        }

        if (OfficialSupplyLaborDropClampCeiling is < MinOfficialSupplyLaborDrop or > MaxOfficialSupplyLaborDrop)
        {
            errors.Add(
                $"official_supply_labor_drop_clamp_ceiling must be between {MinOfficialSupplyLaborDrop} and {MaxOfficialSupplyLaborDrop}.");
        }

        if (OfficialSupplyLaborDropClampFloor > OfficialSupplyLaborDropClampCeiling)
        {
            errors.Add("official_supply_labor_drop_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyMigrationDeltaClampFloor is < MinOfficialSupplyMigrationDelta or > MaxOfficialSupplyMigrationDelta)
        {
            errors.Add(
                $"official_supply_migration_delta_clamp_floor must be between {MinOfficialSupplyMigrationDelta} and {MaxOfficialSupplyMigrationDelta}.");
        }

        if (OfficialSupplyMigrationDeltaClampCeiling is < MinOfficialSupplyMigrationDelta or > MaxOfficialSupplyMigrationDelta)
        {
            errors.Add(
                $"official_supply_migration_delta_clamp_ceiling must be between {MinOfficialSupplyMigrationDelta} and {MaxOfficialSupplyMigrationDelta}.");
        }

        if (OfficialSupplyMigrationDeltaClampFloor > OfficialSupplyMigrationDeltaClampCeiling)
        {
            errors.Add("official_supply_migration_delta_clamp_floor must be less than or equal to ceiling.");
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

        if (MonthlyRuntimePressureContributionFloor is < 0 or > MaxMonthlyRuntimePressureContributionFloor)
        {
            errors.Add(
                $"monthly_runtime_pressure_contribution_floor must be between 0 and {MaxMonthlyRuntimePressureContributionFloor}.");
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

    public int GetMonthlyPressureProsperityDistressThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyPressureProsperityDistressThreshold
            : DefaultMonthlyPressureProsperityDistressThreshold;
    }

    public int GetMonthlyPressureProsperityReliefThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyPressureProsperityReliefThreshold
            : DefaultMonthlyPressureProsperityReliefThreshold;
    }

    public int GetMonthlyPressureSecurityDistressThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyPressureSecurityDistressThreshold
            : DefaultMonthlyPressureSecurityDistressThreshold;
    }

    public int GetMonthlyPressureSecurityReliefThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyPressureSecurityReliefThreshold
            : DefaultMonthlyPressureSecurityReliefThreshold;
    }

    public int GetMonthlyPressureClanSupportReliefThresholdOrDefault()
    {
        return Validate().IsValid
            ? MonthlyPressureClanSupportReliefThreshold
            : DefaultMonthlyPressureClanSupportReliefThreshold;
    }

    public int GetMonthlyPressureDriftMinInclusiveOrDefault()
    {
        return Validate().IsValid
            ? MonthlyPressureDriftMinInclusive
            : DefaultMonthlyPressureDriftMinInclusive;
    }

    public int GetMonthlyPressureDriftMaxExclusiveOrDefault()
    {
        return Validate().IsValid
            ? MonthlyPressureDriftMaxExclusive
            : DefaultMonthlyPressureDriftMaxExclusive;
    }

    public int GetGrainPriceShockDefaultCurrentPriceOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockDefaultCurrentPrice
            : DefaultGrainPriceShockDefaultCurrentPrice;
    }

    public int GetGrainPriceShockDefaultOldPriceOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockDefaultOldPrice
            : DefaultGrainPriceShockDefaultOldPrice;
    }

    public int GetGrainPriceShockDefaultSupplyOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockDefaultSupply
            : DefaultGrainPriceShockDefaultSupply;
    }

    public int GetGrainPriceShockDefaultDemandOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockDefaultDemand
            : DefaultGrainPriceShockDefaultDemand;
    }

    public int GetGrainPriceShockCurrentPriceClampFloorOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockCurrentPriceClampFloor
            : DefaultGrainPriceShockCurrentPriceClampFloor;
    }

    public int GetGrainPriceShockCurrentPriceClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockCurrentPriceClampCeiling
            : DefaultGrainPriceShockCurrentPriceClampCeiling;
    }

    public int GetGrainPriceShockPriceDeltaClampFloorOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockPriceDeltaClampFloor
            : DefaultGrainPriceShockPriceDeltaClampFloor;
    }

    public int GetGrainPriceShockPriceDeltaClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockPriceDeltaClampCeiling
            : DefaultGrainPriceShockPriceDeltaClampCeiling;
    }

    public int GetGrainPriceShockSupplyClampFloorOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockSupplyClampFloor
            : DefaultGrainPriceShockSupplyClampFloor;
    }

    public int GetGrainPriceShockSupplyClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockSupplyClampCeiling
            : DefaultGrainPriceShockSupplyClampCeiling;
    }

    public int GetGrainPriceShockDemandClampFloorOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockDemandClampFloor
            : DefaultGrainPriceShockDemandClampFloor;
    }

    public int GetGrainPriceShockDemandClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceShockDemandClampCeiling
            : DefaultGrainPriceShockDemandClampCeiling;
    }

    public int GetGrainPricePressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? GrainPricePressureClampFloor
            : DefaultGrainPricePressureClampFloor;
    }

    public int GetGrainPricePressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? GrainPricePressureClampCeiling
            : DefaultGrainPricePressureClampCeiling;
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> GetGrainPriceLevelPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceLevelPressureBands
            : DefaultGrainPriceLevelPressureBands;
    }

    public int GetGrainPriceLevelPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceLevelPressureFallbackScore
            : DefaultGrainPriceLevelPressureFallbackScore;
    }

    public int GetGrainPriceLevelPressureScoreOrDefault(int currentPrice)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in GetGrainPriceLevelPressureBandsOrDefault())
        {
            if (currentPrice >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetGrainPriceLevelPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> GetGrainPriceJumpPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceJumpPressureBands
            : DefaultGrainPriceJumpPressureBands;
    }

    public int GetGrainPriceJumpPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceJumpPressureFallbackScore
            : DefaultGrainPriceJumpPressureFallbackScore;
    }

    public int GetGrainPriceJumpPressureScoreOrDefault(int priceDelta)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in GetGrainPriceJumpPressureBandsOrDefault())
        {
            if (priceDelta >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetGrainPriceJumpPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetGrainPriceMarketTightnessPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceMarketTightnessPressureBands
            : DefaultGrainPriceMarketTightnessPressureBands;
    }

    public int GetGrainPriceMarketTightnessPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? GrainPriceMarketTightnessPressureFallbackScore
            : DefaultGrainPriceMarketTightnessPressureFallbackScore;
    }

    public int GetGrainPriceMarketTightnessPressureScoreOrDefault(int marketTightness)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in GetGrainPriceMarketTightnessPressureBandsOrDefault())
        {
            if (marketTightness >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetGrainPriceMarketTightnessPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetSubsistenceGrainBufferPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceGrainBufferPressureBands
            : DefaultSubsistenceGrainBufferPressureBands;
    }

    public int GetSubsistenceGrainBufferPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceGrainBufferPressureFallbackScore
            : DefaultSubsistenceGrainBufferPressureFallbackScore;
    }

    public int GetSubsistenceGrainBufferPressureScoreOrDefault(int grainStore)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetSubsistenceGrainBufferPressureBandsOrDefault())
        {
            if (grainStore >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetSubsistenceGrainBufferPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight>
        GetSubsistenceMarketDependencyPressureScoreWeightsOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceMarketDependencyPressureScoreWeights
            : DefaultSubsistenceMarketDependencyPressureScoreWeights;
    }

    public int GetSubsistenceMarketDependencyPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceMarketDependencyPressureFallbackScore
            : DefaultSubsistenceMarketDependencyPressureFallbackScore;
    }

    public int GetSubsistenceMarketDependencyPressureScoreOrDefault(LivelihoodType livelihood)
    {
        foreach (PopulationHouseholdMobilityLivelihoodScoreWeight entry in
                 GetSubsistenceMarketDependencyPressureScoreWeightsOrDefault())
        {
            if (entry.Livelihood == livelihood)
            {
                return entry.Weight;
            }
        }

        return GetSubsistenceMarketDependencyPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetSubsistenceLaborCapacityPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceLaborCapacityPressureBands
            : DefaultSubsistenceLaborCapacityPressureBands;
    }

    public int GetSubsistenceLaborCapacityPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceLaborCapacityPressureFallbackScore
            : DefaultSubsistenceLaborCapacityPressureFallbackScore;
    }

    public int GetSubsistenceLaborCapacityPressureScoreOrDefault(int laborCapacity)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetSubsistenceLaborCapacityPressureBandsOrDefault())
        {
            if (laborCapacity >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetSubsistenceLaborCapacityPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetSubsistenceDependentCountPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceDependentCountPressureBands
            : DefaultSubsistenceDependentCountPressureBands;
    }

    public int GetSubsistenceDependentCountPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceDependentCountPressureFallbackScore
            : DefaultSubsistenceDependentCountPressureFallbackScore;
    }

    public int GetSubsistenceDependentCountPressureScoreOrDefault(int dependentCount)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetSubsistenceDependentCountPressureBandsOrDefault())
        {
            if (dependentCount >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetSubsistenceDependentCountPressureFallbackScoreOrDefault();
    }

    public int GetSubsistenceLaborPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceLaborPressureClampFloor
            : DefaultSubsistenceLaborPressureClampFloor;
    }

    public int GetSubsistenceLaborPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceLaborPressureClampCeiling
            : DefaultSubsistenceLaborPressureClampCeiling;
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetSubsistenceFragilityDistressPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityDistressPressureBands
            : DefaultSubsistenceFragilityDistressPressureBands;
    }

    public int GetSubsistenceFragilityDistressPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityDistressPressureFallbackScore
            : DefaultSubsistenceFragilityDistressPressureFallbackScore;
    }

    public int GetSubsistenceFragilityDistressPressureScoreOrDefault(int distress)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetSubsistenceFragilityDistressPressureBandsOrDefault())
        {
            if (distress >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetSubsistenceFragilityDistressPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetSubsistenceFragilityDebtPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityDebtPressureBands
            : DefaultSubsistenceFragilityDebtPressureBands;
    }

    public int GetSubsistenceFragilityDebtPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityDebtPressureFallbackScore
            : DefaultSubsistenceFragilityDebtPressureFallbackScore;
    }

    public int GetSubsistenceFragilityDebtPressureScoreOrDefault(int debtPressure)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetSubsistenceFragilityDebtPressureBandsOrDefault())
        {
            if (debtPressure >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetSubsistenceFragilityDebtPressureFallbackScoreOrDefault();
    }

    public int GetSubsistenceFragilityMigrationRiskThresholdOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityMigrationRiskThreshold
            : DefaultSubsistenceFragilityMigrationRiskThreshold;
    }

    public int GetSubsistenceFragilityMigrationPressureScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityMigrationPressureScore
            : DefaultSubsistenceFragilityMigrationPressureScore;
    }

    public int GetSubsistenceFragilityMigrationPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityMigrationPressureFallbackScore
            : DefaultSubsistenceFragilityMigrationPressureFallbackScore;
    }

    public int GetSubsistenceFragilityMigrationPressureScoreOrDefault(
        bool isMigrating,
        int migrationRisk)
    {
        return isMigrating || migrationRisk >= GetSubsistenceFragilityMigrationRiskThresholdOrDefault()
            ? GetSubsistenceFragilityMigrationPressureScoreOrDefault()
            : GetSubsistenceFragilityMigrationPressureFallbackScoreOrDefault();
    }

    public int GetSubsistenceFragilityPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityPressureClampFloor
            : DefaultSubsistenceFragilityPressureClampFloor;
    }

    public int GetSubsistenceFragilityPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceFragilityPressureClampCeiling
            : DefaultSubsistenceFragilityPressureClampCeiling;
    }

    public int GetSubsistenceInteractionGrainShortageStoreFloorExclusiveOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionGrainShortageStoreFloorExclusive
            : DefaultSubsistenceInteractionGrainShortageStoreFloorExclusive;
    }

    public int GetSubsistenceInteractionGrainShortageStoreCeilingExclusiveOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionGrainShortageStoreCeilingExclusive
            : DefaultSubsistenceInteractionGrainShortageStoreCeilingExclusive;
    }

    public bool IsSubsistenceInteractionGrainShortageStoreOrDefault(int grainStore)
    {
        return grainStore > GetSubsistenceInteractionGrainShortageStoreFloorExclusiveOrDefault()
            && grainStore < GetSubsistenceInteractionGrainShortageStoreCeilingExclusiveOrDefault();
    }

    public int GetSubsistenceInteractionCashNeedBoostScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionCashNeedBoostScore
            : DefaultSubsistenceInteractionCashNeedBoostScore;
    }

    public int GetSubsistenceInteractionDebtPressureThresholdOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionDebtPressureThreshold
            : DefaultSubsistenceInteractionDebtPressureThreshold;
    }

    public bool IsSubsistenceInteractionDebtPressureOrDefault(int debtPressure)
    {
        return debtPressure >= GetSubsistenceInteractionDebtPressureThresholdOrDefault();
    }

    public int GetSubsistenceInteractionDebtPressureBoostScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionDebtPressureBoostScore
            : DefaultSubsistenceInteractionDebtPressureBoostScore;
    }

    public int GetSubsistenceInteractionResilienceReliefGrainStoreThresholdOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionResilienceReliefGrainStoreThreshold
            : DefaultSubsistenceInteractionResilienceReliefGrainStoreThreshold;
    }

    public int GetSubsistenceInteractionResilienceReliefLandHoldingThresholdOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionResilienceReliefLandHoldingThreshold
            : DefaultSubsistenceInteractionResilienceReliefLandHoldingThreshold;
    }

    public int GetSubsistenceInteractionResilienceReliefLaborCapacityThresholdOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionResilienceReliefLaborCapacityThreshold
            : DefaultSubsistenceInteractionResilienceReliefLaborCapacityThreshold;
    }

    public bool IsSubsistenceInteractionResilienceReliefOrDefault(
        int grainStore,
        int landHolding,
        int laborCapacity)
    {
        return grainStore >= GetSubsistenceInteractionResilienceReliefGrainStoreThresholdOrDefault()
            && landHolding >= GetSubsistenceInteractionResilienceReliefLandHoldingThresholdOrDefault()
            && laborCapacity >= GetSubsistenceInteractionResilienceReliefLaborCapacityThresholdOrDefault();
    }

    public int GetSubsistenceInteractionResilienceReliefScoreOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionResilienceReliefScore
            : DefaultSubsistenceInteractionResilienceReliefScore;
    }

    public int GetSubsistenceInteractionPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionPressureClampFloor
            : DefaultSubsistenceInteractionPressureClampFloor;
    }

    public int GetSubsistenceInteractionPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? SubsistenceInteractionPressureClampCeiling
            : DefaultSubsistenceInteractionPressureClampCeiling;
    }

    public int GetSubsistencePressureEventDistressThresholdOrDefault()
    {
        return Validate().IsValid
            ? SubsistencePressureEventDistressThreshold
            : DefaultSubsistencePressureEventDistressThreshold;
    }

    public int GetSubsistencePressureDistressDeltaClampFloorOrDefault()
    {
        return Validate().IsValid
            ? SubsistencePressureDistressDeltaClampFloor
            : DefaultSubsistencePressureDistressDeltaClampFloor;
    }

    public int GetSubsistencePressureDistressDeltaClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? SubsistencePressureDistressDeltaClampCeiling
            : DefaultSubsistencePressureDistressDeltaClampCeiling;
    }

    public int GetTaxSeasonDebtDeltaClampFloorOrDefault()
    {
        return Validate().IsValid
            ? TaxSeasonDebtDeltaClampFloor
            : DefaultTaxSeasonDebtDeltaClampFloor;
    }

    public int GetTaxSeasonDebtDeltaClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? TaxSeasonDebtDeltaClampCeiling
            : DefaultTaxSeasonDebtDeltaClampCeiling;
    }

    public int GetTaxSeasonDebtSpikeEventThresholdOrDefault()
    {
        return Validate().IsValid
            ? TaxSeasonDebtSpikeEventThreshold
            : DefaultTaxSeasonDebtSpikeEventThreshold;
    }

    public int GetOfficialSupplyDistressDeltaClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDistressDeltaClampFloor
            : DefaultOfficialSupplyDistressDeltaClampFloor;
    }

    public int GetOfficialSupplyDistressDeltaClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDistressDeltaClampCeiling
            : DefaultOfficialSupplyDistressDeltaClampCeiling;
    }

    public int GetOfficialSupplyDebtDeltaClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDebtDeltaClampFloor
            : DefaultOfficialSupplyDebtDeltaClampFloor;
    }

    public int GetOfficialSupplyDebtDeltaClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDebtDeltaClampCeiling
            : DefaultOfficialSupplyDebtDeltaClampCeiling;
    }

    public int GetOfficialSupplyLaborDropClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLaborDropClampFloor
            : DefaultOfficialSupplyLaborDropClampFloor;
    }

    public int GetOfficialSupplyLaborDropClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLaborDropClampCeiling
            : DefaultOfficialSupplyLaborDropClampCeiling;
    }

    public int GetOfficialSupplyMigrationDeltaClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyMigrationDeltaClampFloor
            : DefaultOfficialSupplyMigrationDeltaClampFloor;
    }

    public int GetOfficialSupplyMigrationDeltaClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyMigrationDeltaClampCeiling
            : DefaultOfficialSupplyMigrationDeltaClampCeiling;
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

    public int GetMonthlyRuntimePressureContributionFloorOrDefault()
    {
        return Validate().IsValid
            ? MonthlyRuntimePressureContributionFloor
            : DefaultMonthlyRuntimePressureContributionFloor;
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

public readonly record struct PopulationHouseholdMobilityThresholdScoreBand(
    int Threshold,
    int Score);

public enum PopulationHouseholdMobilityPoolTieBreakPriority
{
    SettlementIdAscending = 0,
}

public enum PopulationHouseholdMobilityHouseholdTieBreakPriority
{
    HouseholdIdAscending = 0,
}
