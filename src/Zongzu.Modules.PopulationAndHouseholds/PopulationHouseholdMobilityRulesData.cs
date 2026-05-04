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
    int OfficialSupplyFallbackFrontierPressure,
    int OfficialSupplyFallbackQuotaPressure,
    int OfficialSupplyFallbackDocketPressure,
    int OfficialSupplyFallbackClerkDistortionPressure,
    int OfficialSupplyFallbackAuthorityBuffer,
    int OfficialSupplyFallbackDerivedPressureClampFloor,
    int OfficialSupplyFallbackDerivedPressureClampCeiling,
    int OfficialSupplyFrontierPressureClampFloor,
    int OfficialSupplyFrontierPressureClampCeiling,
    int OfficialSupplyPressureClampFloor,
    int OfficialSupplyPressureClampCeiling,
    int OfficialSupplyQuotaPressureClampFloor,
    int OfficialSupplyQuotaPressureClampCeiling,
    int OfficialSupplyDocketPressureClampFloor,
    int OfficialSupplyDocketPressureClampCeiling,
    int OfficialSupplyClerkDistortionPressureClampFloor,
    int OfficialSupplyClerkDistortionPressureClampCeiling,
    int OfficialSupplyAuthorityBufferClampFloor,
    int OfficialSupplyAuthorityBufferClampCeiling,
    IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight> OfficialSupplyLivelihoodExposureScoreWeights,
    int OfficialSupplyLivelihoodExposureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyLandVisibilityScoreBands,
    int OfficialSupplyLandVisibilityFallbackScore,
    int OfficialSupplyLivelihoodExposureClampFloor,
    int OfficialSupplyLivelihoodExposureClampCeiling,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyResourceGrainBufferScoreBands,
    int OfficialSupplyResourceGrainBufferFallbackScore,
    int OfficialSupplyResourceToolConditionThreshold,
    int OfficialSupplyResourceToolBufferScore,
    int OfficialSupplyResourceToolBufferFallbackScore,
    int OfficialSupplyResourceShelterQualityThreshold,
    int OfficialSupplyResourceShelterBufferScore,
    int OfficialSupplyResourceShelterBufferFallbackScore,
    int OfficialSupplyResourceBufferClampFloor,
    int OfficialSupplyResourceBufferClampCeiling,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyLaborCapacityPressureBands,
    int OfficialSupplyLaborCapacityPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyDependentCountPressureBands,
    int OfficialSupplyDependentCountPressureFallbackScore,
    int OfficialSupplyDependentToLaborRatioMultiplier,
    int OfficialSupplyDependentToLaborRatioBonusScore,
    int OfficialSupplyDependentToLaborRatioFallbackScore,
    int OfficialSupplyLaborPressureClampFloor,
    int OfficialSupplyLaborPressureClampCeiling,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyLiquidityGrainStrainPressureBands,
    int OfficialSupplyLiquidityGrainStrainPressureFallbackScore,
    int OfficialSupplyLiquidityCashNeedPressureScore,
    int OfficialSupplyLiquidityCashNeedPressureFallbackScore,
    int OfficialSupplyLiquidityToolDragConditionThreshold,
    int OfficialSupplyLiquidityToolDragPressureScore,
    int OfficialSupplyLiquidityToolDragPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyLiquidityDebtDragPressureBands,
    int OfficialSupplyLiquidityDebtDragPressureFallbackScore,
    int OfficialSupplyLiquidityPressureClampFloor,
    int OfficialSupplyLiquidityPressureClampCeiling,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyFragilityDistressPressureBands,
    int OfficialSupplyFragilityDistressPressureFallbackScore,
    IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand> OfficialSupplyFragilityDebtPressureBands,
    int OfficialSupplyFragilityDebtPressureFallbackScore,
    int OfficialSupplyFragilityMigrationRiskThreshold,
    int OfficialSupplyFragilityMigrationPressureScore,
    int OfficialSupplyFragilityMigrationPressureFallbackScore,
    int OfficialSupplyFragilityShelterDragQualityThreshold,
    int OfficialSupplyFragilityShelterDragPressureScore,
    int OfficialSupplyFragilityShelterDragPressureFallbackScore,
    int OfficialSupplyFragilityPressureClampFloor,
    int OfficialSupplyFragilityPressureClampCeiling,
    int OfficialSupplyDistressDeltaClampFloor,
    int OfficialSupplyDistressDeltaClampCeiling,
    int OfficialSupplyDebtDeltaClampFloor,
    int OfficialSupplyDebtDeltaClampCeiling,
    int OfficialSupplyLaborDropClampFloor,
    int OfficialSupplyLaborDropClampCeiling,
    int OfficialSupplyMigrationDeltaClampFloor,
    int OfficialSupplyMigrationDeltaClampCeiling,
    int OfficialSupplyBurdenEventDistressThreshold,
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
    public const int DefaultOfficialSupplyFallbackFrontierPressure = 60;
    public const int DefaultOfficialSupplyFallbackQuotaPressure = 7;
    public const int DefaultOfficialSupplyFallbackDocketPressure = 1;
    public const int DefaultOfficialSupplyFallbackClerkDistortionPressure = 0;
    public const int DefaultOfficialSupplyFallbackAuthorityBuffer = 4;
    public const int DefaultOfficialSupplyFallbackDerivedPressureClampFloor = 4;
    public const int DefaultOfficialSupplyFallbackDerivedPressureClampCeiling = 26;
    public const int DefaultOfficialSupplyFrontierPressureClampFloor = 0;
    public const int DefaultOfficialSupplyFrontierPressureClampCeiling = 100;
    public const int DefaultOfficialSupplyPressureClampFloor = 0;
    public const int DefaultOfficialSupplyPressureClampCeiling = 30;
    public const int DefaultOfficialSupplyQuotaPressureClampFloor = 0;
    public const int DefaultOfficialSupplyQuotaPressureClampCeiling = 20;
    public const int DefaultOfficialSupplyDocketPressureClampFloor = 0;
    public const int DefaultOfficialSupplyDocketPressureClampCeiling = 20;
    public const int DefaultOfficialSupplyClerkDistortionPressureClampFloor = 0;
    public const int DefaultOfficialSupplyClerkDistortionPressureClampCeiling = 15;
    public const int DefaultOfficialSupplyAuthorityBufferClampFloor = 0;
    public const int DefaultOfficialSupplyAuthorityBufferClampCeiling = 12;
    public const int DefaultOfficialSupplyLivelihoodExposureFallbackScore = 2;
    public const int DefaultOfficialSupplyLandVisibilityFallbackScore = 0;
    public const int DefaultOfficialSupplyLivelihoodExposureClampFloor = 1;
    public const int DefaultOfficialSupplyLivelihoodExposureClampCeiling = 7;
    public const int DefaultOfficialSupplyResourceGrainBufferFallbackScore = 0;
    public const int DefaultOfficialSupplyResourceToolConditionThreshold = 70;
    public const int DefaultOfficialSupplyResourceToolBufferScore = 1;
    public const int DefaultOfficialSupplyResourceToolBufferFallbackScore = 0;
    public const int DefaultOfficialSupplyResourceShelterQualityThreshold = 60;
    public const int DefaultOfficialSupplyResourceShelterBufferScore = 1;
    public const int DefaultOfficialSupplyResourceShelterBufferFallbackScore = 0;
    public const int DefaultOfficialSupplyResourceBufferClampFloor = 0;
    public const int DefaultOfficialSupplyResourceBufferClampCeiling = 7;
    public const int DefaultOfficialSupplyLaborCapacityPressureFallbackScore = 4;
    public const int DefaultOfficialSupplyDependentCountPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyDependentToLaborRatioMultiplier = 2;
    public const int DefaultOfficialSupplyDependentToLaborRatioBonusScore = 1;
    public const int DefaultOfficialSupplyDependentToLaborRatioFallbackScore = 0;
    public const int DefaultOfficialSupplyLaborPressureClampFloor = -1;
    public const int DefaultOfficialSupplyLaborPressureClampCeiling = 7;
    public const int DefaultOfficialSupplyLiquidityGrainStrainPressureFallbackScore = 2;
    public const int DefaultOfficialSupplyLiquidityCashNeedPressureScore = 2;
    public const int DefaultOfficialSupplyLiquidityCashNeedPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyLiquidityToolDragConditionThreshold = 35;
    public const int DefaultOfficialSupplyLiquidityToolDragPressureScore = 1;
    public const int DefaultOfficialSupplyLiquidityToolDragPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyLiquidityDebtDragPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyLiquidityPressureClampFloor = -2;
    public const int DefaultOfficialSupplyLiquidityPressureClampCeiling = 7;
    public const int DefaultOfficialSupplyFragilityDistressPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyFragilityDebtPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyFragilityMigrationRiskThreshold = 70;
    public const int DefaultOfficialSupplyFragilityMigrationPressureScore = 1;
    public const int DefaultOfficialSupplyFragilityMigrationPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyFragilityShelterDragQualityThreshold = 35;
    public const int DefaultOfficialSupplyFragilityShelterDragPressureScore = 1;
    public const int DefaultOfficialSupplyFragilityShelterDragPressureFallbackScore = 0;
    public const int DefaultOfficialSupplyFragilityPressureClampFloor = 0;
    public const int DefaultOfficialSupplyFragilityPressureClampCeiling = 8;
    public const int DefaultOfficialSupplyDistressDeltaClampFloor = 0;
    public const int DefaultOfficialSupplyDistressDeltaClampCeiling = 24;
    public const int DefaultOfficialSupplyDebtDeltaClampFloor = 0;
    public const int DefaultOfficialSupplyDebtDeltaClampCeiling = 18;
    public const int DefaultOfficialSupplyLaborDropClampFloor = 0;
    public const int DefaultOfficialSupplyLaborDropClampCeiling = 8;
    public const int DefaultOfficialSupplyMigrationDeltaClampFloor = 0;
    public const int DefaultOfficialSupplyMigrationDeltaClampCeiling = 8;
    public const int DefaultOfficialSupplyBurdenEventDistressThreshold = 80;
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
    public const int MaxOfficialSupplyFallbackFrontierPressure = 100;
    public const int MaxOfficialSupplyFallbackSupplyPressure = 30;
    public const int MaxOfficialSupplyFallbackQuotaPressure = 20;
    public const int MaxOfficialSupplyFallbackDocketPressure = 20;
    public const int MaxOfficialSupplyFallbackClerkDistortionPressure = 15;
    public const int MaxOfficialSupplyFallbackAuthorityBuffer = 12;
    public const int MaxOfficialSupplyLivelihoodExposureContribution = 8;
    public const int MaxOfficialSupplyLandVisibilityScore = 4;
    public const int MinOfficialSupplyLivelihoodExposurePressure = 0;
    public const int MaxOfficialSupplyLivelihoodExposurePressure = 16;
    public const int MaxOfficialSupplyResourceBufferContribution = 8;
    public const int MinOfficialSupplyResourceBufferPressure = 0;
    public const int MaxOfficialSupplyResourceBufferPressure = 16;
    public const int MinOfficialSupplyLaborPressureContribution = -4;
    public const int MaxOfficialSupplyLaborPressureContribution = 8;
    public const int MaxOfficialSupplyDependentCountThreshold = 32;
    public const int MinOfficialSupplyLaborPressure = -4;
    public const int MaxOfficialSupplyLaborPressure = 16;
    public const int MinOfficialSupplyLiquidityPressureContribution = -4;
    public const int MaxOfficialSupplyLiquidityPressureContribution = 8;
    public const int MinOfficialSupplyLiquidityPressure = -8;
    public const int MaxOfficialSupplyLiquidityPressure = 16;
    public const int MaxOfficialSupplyFragilityPressureContribution = 8;
    public const int MinOfficialSupplyFragilityPressure = 0;
    public const int MaxOfficialSupplyFragilityPressure = 16;
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

    public static IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight>
        DefaultOfficialSupplyLivelihoodExposureScoreWeights { get; } =
        new[]
        {
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Boatman, 5),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.HiredLabor, 4),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.SeasonalMigrant, 4),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Smallholder, 3),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 3),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Artisan, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.PettyTrader, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.YamenRunner, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Unknown, 2),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.DomesticServant, 1),
            new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Vagrant, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyLandVisibilityScoreBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(70, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(35, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyResourceGrainBufferScoreBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(85, 5),
            new PopulationHouseholdMobilityThresholdScoreBand(65, 4),
            new PopulationHouseholdMobilityThresholdScoreBand(45, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(25, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyLaborCapacityPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(80, -1),
            new PopulationHouseholdMobilityThresholdScoreBand(60, 0),
            new PopulationHouseholdMobilityThresholdScoreBand(40, 1),
            new PopulationHouseholdMobilityThresholdScoreBand(25, 3),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyDependentCountPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(5, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(3, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyLiquidityGrainStrainPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(80, -2),
            new PopulationHouseholdMobilityThresholdScoreBand(55, -1),
            new PopulationHouseholdMobilityThresholdScoreBand(25, 1),
            new PopulationHouseholdMobilityThresholdScoreBand(1, 3),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyLiquidityDebtDragPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(65, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(50, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyFragilityDistressPressureBands { get; } =
        new[]
        {
            new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
            new PopulationHouseholdMobilityThresholdScoreBand(65, 2),
            new PopulationHouseholdMobilityThresholdScoreBand(50, 1),
        };

    public static IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        DefaultOfficialSupplyFragilityDebtPressureBands { get; } =
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
            DefaultOfficialSupplyFallbackFrontierPressure,
            DefaultOfficialSupplyFallbackQuotaPressure,
            DefaultOfficialSupplyFallbackDocketPressure,
            DefaultOfficialSupplyFallbackClerkDistortionPressure,
            DefaultOfficialSupplyFallbackAuthorityBuffer,
            DefaultOfficialSupplyFallbackDerivedPressureClampFloor,
            DefaultOfficialSupplyFallbackDerivedPressureClampCeiling,
            DefaultOfficialSupplyFrontierPressureClampFloor,
            DefaultOfficialSupplyFrontierPressureClampCeiling,
            DefaultOfficialSupplyPressureClampFloor,
            DefaultOfficialSupplyPressureClampCeiling,
            DefaultOfficialSupplyQuotaPressureClampFloor,
            DefaultOfficialSupplyQuotaPressureClampCeiling,
            DefaultOfficialSupplyDocketPressureClampFloor,
            DefaultOfficialSupplyDocketPressureClampCeiling,
            DefaultOfficialSupplyClerkDistortionPressureClampFloor,
            DefaultOfficialSupplyClerkDistortionPressureClampCeiling,
            DefaultOfficialSupplyAuthorityBufferClampFloor,
            DefaultOfficialSupplyAuthorityBufferClampCeiling,
            DefaultOfficialSupplyLivelihoodExposureScoreWeights,
            DefaultOfficialSupplyLivelihoodExposureFallbackScore,
            DefaultOfficialSupplyLandVisibilityScoreBands,
            DefaultOfficialSupplyLandVisibilityFallbackScore,
            DefaultOfficialSupplyLivelihoodExposureClampFloor,
            DefaultOfficialSupplyLivelihoodExposureClampCeiling,
            DefaultOfficialSupplyResourceGrainBufferScoreBands,
            DefaultOfficialSupplyResourceGrainBufferFallbackScore,
            DefaultOfficialSupplyResourceToolConditionThreshold,
            DefaultOfficialSupplyResourceToolBufferScore,
            DefaultOfficialSupplyResourceToolBufferFallbackScore,
            DefaultOfficialSupplyResourceShelterQualityThreshold,
            DefaultOfficialSupplyResourceShelterBufferScore,
            DefaultOfficialSupplyResourceShelterBufferFallbackScore,
            DefaultOfficialSupplyResourceBufferClampFloor,
            DefaultOfficialSupplyResourceBufferClampCeiling,
            DefaultOfficialSupplyLaborCapacityPressureBands,
            DefaultOfficialSupplyLaborCapacityPressureFallbackScore,
            DefaultOfficialSupplyDependentCountPressureBands,
            DefaultOfficialSupplyDependentCountPressureFallbackScore,
            DefaultOfficialSupplyDependentToLaborRatioMultiplier,
            DefaultOfficialSupplyDependentToLaborRatioBonusScore,
            DefaultOfficialSupplyDependentToLaborRatioFallbackScore,
            DefaultOfficialSupplyLaborPressureClampFloor,
            DefaultOfficialSupplyLaborPressureClampCeiling,
            DefaultOfficialSupplyLiquidityGrainStrainPressureBands,
            DefaultOfficialSupplyLiquidityGrainStrainPressureFallbackScore,
            DefaultOfficialSupplyLiquidityCashNeedPressureScore,
            DefaultOfficialSupplyLiquidityCashNeedPressureFallbackScore,
            DefaultOfficialSupplyLiquidityToolDragConditionThreshold,
            DefaultOfficialSupplyLiquidityToolDragPressureScore,
            DefaultOfficialSupplyLiquidityToolDragPressureFallbackScore,
            DefaultOfficialSupplyLiquidityDebtDragPressureBands,
            DefaultOfficialSupplyLiquidityDebtDragPressureFallbackScore,
            DefaultOfficialSupplyLiquidityPressureClampFloor,
            DefaultOfficialSupplyLiquidityPressureClampCeiling,
            DefaultOfficialSupplyFragilityDistressPressureBands,
            DefaultOfficialSupplyFragilityDistressPressureFallbackScore,
            DefaultOfficialSupplyFragilityDebtPressureBands,
            DefaultOfficialSupplyFragilityDebtPressureFallbackScore,
            DefaultOfficialSupplyFragilityMigrationRiskThreshold,
            DefaultOfficialSupplyFragilityMigrationPressureScore,
            DefaultOfficialSupplyFragilityMigrationPressureFallbackScore,
            DefaultOfficialSupplyFragilityShelterDragQualityThreshold,
            DefaultOfficialSupplyFragilityShelterDragPressureScore,
            DefaultOfficialSupplyFragilityShelterDragPressureFallbackScore,
            DefaultOfficialSupplyFragilityPressureClampFloor,
            DefaultOfficialSupplyFragilityPressureClampCeiling,
            DefaultOfficialSupplyDistressDeltaClampFloor,
            DefaultOfficialSupplyDistressDeltaClampCeiling,
            DefaultOfficialSupplyDebtDeltaClampFloor,
            DefaultOfficialSupplyDebtDeltaClampCeiling,
            DefaultOfficialSupplyLaborDropClampFloor,
            DefaultOfficialSupplyLaborDropClampCeiling,
            DefaultOfficialSupplyMigrationDeltaClampFloor,
            DefaultOfficialSupplyMigrationDeltaClampCeiling,
            DefaultOfficialSupplyBurdenEventDistressThreshold,
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
            DefaultOfficialSupplyFallbackFrontierPressure,
            DefaultOfficialSupplyFallbackQuotaPressure,
            DefaultOfficialSupplyFallbackDocketPressure,
            DefaultOfficialSupplyFallbackClerkDistortionPressure,
            DefaultOfficialSupplyFallbackAuthorityBuffer,
            DefaultOfficialSupplyFallbackDerivedPressureClampFloor,
            DefaultOfficialSupplyFallbackDerivedPressureClampCeiling,
            DefaultOfficialSupplyFrontierPressureClampFloor,
            DefaultOfficialSupplyFrontierPressureClampCeiling,
            DefaultOfficialSupplyPressureClampFloor,
            DefaultOfficialSupplyPressureClampCeiling,
            DefaultOfficialSupplyQuotaPressureClampFloor,
            DefaultOfficialSupplyQuotaPressureClampCeiling,
            DefaultOfficialSupplyDocketPressureClampFloor,
            DefaultOfficialSupplyDocketPressureClampCeiling,
            DefaultOfficialSupplyClerkDistortionPressureClampFloor,
            DefaultOfficialSupplyClerkDistortionPressureClampCeiling,
            DefaultOfficialSupplyAuthorityBufferClampFloor,
            DefaultOfficialSupplyAuthorityBufferClampCeiling,
            DefaultOfficialSupplyLivelihoodExposureScoreWeights,
            DefaultOfficialSupplyLivelihoodExposureFallbackScore,
            DefaultOfficialSupplyLandVisibilityScoreBands,
            DefaultOfficialSupplyLandVisibilityFallbackScore,
            DefaultOfficialSupplyLivelihoodExposureClampFloor,
            DefaultOfficialSupplyLivelihoodExposureClampCeiling,
            DefaultOfficialSupplyResourceGrainBufferScoreBands,
            DefaultOfficialSupplyResourceGrainBufferFallbackScore,
            DefaultOfficialSupplyResourceToolConditionThreshold,
            DefaultOfficialSupplyResourceToolBufferScore,
            DefaultOfficialSupplyResourceToolBufferFallbackScore,
            DefaultOfficialSupplyResourceShelterQualityThreshold,
            DefaultOfficialSupplyResourceShelterBufferScore,
            DefaultOfficialSupplyResourceShelterBufferFallbackScore,
            DefaultOfficialSupplyResourceBufferClampFloor,
            DefaultOfficialSupplyResourceBufferClampCeiling,
            DefaultOfficialSupplyLaborCapacityPressureBands,
            DefaultOfficialSupplyLaborCapacityPressureFallbackScore,
            DefaultOfficialSupplyDependentCountPressureBands,
            DefaultOfficialSupplyDependentCountPressureFallbackScore,
            DefaultOfficialSupplyDependentToLaborRatioMultiplier,
            DefaultOfficialSupplyDependentToLaborRatioBonusScore,
            DefaultOfficialSupplyDependentToLaborRatioFallbackScore,
            DefaultOfficialSupplyLaborPressureClampFloor,
            DefaultOfficialSupplyLaborPressureClampCeiling,
            DefaultOfficialSupplyLiquidityGrainStrainPressureBands,
            DefaultOfficialSupplyLiquidityGrainStrainPressureFallbackScore,
            DefaultOfficialSupplyLiquidityCashNeedPressureScore,
            DefaultOfficialSupplyLiquidityCashNeedPressureFallbackScore,
            DefaultOfficialSupplyLiquidityToolDragConditionThreshold,
            DefaultOfficialSupplyLiquidityToolDragPressureScore,
            DefaultOfficialSupplyLiquidityToolDragPressureFallbackScore,
            DefaultOfficialSupplyLiquidityDebtDragPressureBands,
            DefaultOfficialSupplyLiquidityDebtDragPressureFallbackScore,
            DefaultOfficialSupplyLiquidityPressureClampFloor,
            DefaultOfficialSupplyLiquidityPressureClampCeiling,
            DefaultOfficialSupplyFragilityDistressPressureBands,
            DefaultOfficialSupplyFragilityDistressPressureFallbackScore,
            DefaultOfficialSupplyFragilityDebtPressureBands,
            DefaultOfficialSupplyFragilityDebtPressureFallbackScore,
            DefaultOfficialSupplyFragilityMigrationRiskThreshold,
            DefaultOfficialSupplyFragilityMigrationPressureScore,
            DefaultOfficialSupplyFragilityMigrationPressureFallbackScore,
            DefaultOfficialSupplyFragilityShelterDragQualityThreshold,
            DefaultOfficialSupplyFragilityShelterDragPressureScore,
            DefaultOfficialSupplyFragilityShelterDragPressureFallbackScore,
            DefaultOfficialSupplyFragilityPressureClampFloor,
            DefaultOfficialSupplyFragilityPressureClampCeiling,
            DefaultOfficialSupplyDistressDeltaClampFloor,
            DefaultOfficialSupplyDistressDeltaClampCeiling,
            DefaultOfficialSupplyDebtDeltaClampFloor,
            DefaultOfficialSupplyDebtDeltaClampCeiling,
            DefaultOfficialSupplyLaborDropClampFloor,
            DefaultOfficialSupplyLaborDropClampCeiling,
            DefaultOfficialSupplyMigrationDeltaClampFloor,
            DefaultOfficialSupplyMigrationDeltaClampCeiling,
            DefaultOfficialSupplyBurdenEventDistressThreshold,
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

        if (OfficialSupplyFallbackFrontierPressure is < 0 or > MaxOfficialSupplyFallbackFrontierPressure)
        {
            errors.Add($"official_supply_fallback_frontier_pressure must be between 0 and {MaxOfficialSupplyFallbackFrontierPressure}.");
        }

        if (OfficialSupplyFallbackQuotaPressure is < 0 or > MaxOfficialSupplyFallbackQuotaPressure)
        {
            errors.Add($"official_supply_fallback_quota_pressure must be between 0 and {MaxOfficialSupplyFallbackQuotaPressure}.");
        }

        if (OfficialSupplyFallbackDocketPressure is < 0 or > MaxOfficialSupplyFallbackDocketPressure)
        {
            errors.Add($"official_supply_fallback_docket_pressure must be between 0 and {MaxOfficialSupplyFallbackDocketPressure}.");
        }

        if (OfficialSupplyFallbackClerkDistortionPressure is < 0 or > MaxOfficialSupplyFallbackClerkDistortionPressure)
        {
            errors.Add($"official_supply_fallback_clerk_distortion_pressure must be between 0 and {MaxOfficialSupplyFallbackClerkDistortionPressure}.");
        }

        if (OfficialSupplyFallbackAuthorityBuffer is < 0 or > MaxOfficialSupplyFallbackAuthorityBuffer)
        {
            errors.Add($"official_supply_fallback_authority_buffer must be between 0 and {MaxOfficialSupplyFallbackAuthorityBuffer}.");
        }

        if (OfficialSupplyFallbackDerivedPressureClampFloor is < 0 or > MaxOfficialSupplyFallbackSupplyPressure)
        {
            errors.Add($"official_supply_fallback_derived_pressure_clamp_floor must be between 0 and {MaxOfficialSupplyFallbackSupplyPressure}.");
        }

        if (OfficialSupplyFallbackDerivedPressureClampCeiling is < 0 or > MaxOfficialSupplyFallbackSupplyPressure)
        {
            errors.Add($"official_supply_fallback_derived_pressure_clamp_ceiling must be between 0 and {MaxOfficialSupplyFallbackSupplyPressure}.");
        }

        if (OfficialSupplyFallbackDerivedPressureClampFloor > OfficialSupplyFallbackDerivedPressureClampCeiling)
        {
            errors.Add("official_supply_fallback_derived_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyFrontierPressureClampFloor is < 0 or > MaxOfficialSupplyFallbackFrontierPressure)
        {
            errors.Add($"official_supply_frontier_pressure_clamp_floor must be between 0 and {MaxOfficialSupplyFallbackFrontierPressure}.");
        }

        if (OfficialSupplyFrontierPressureClampCeiling is < 0 or > MaxOfficialSupplyFallbackFrontierPressure)
        {
            errors.Add($"official_supply_frontier_pressure_clamp_ceiling must be between 0 and {MaxOfficialSupplyFallbackFrontierPressure}.");
        }

        if (OfficialSupplyFrontierPressureClampFloor > OfficialSupplyFrontierPressureClampCeiling)
        {
            errors.Add("official_supply_frontier_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyPressureClampFloor is < 0 or > MaxOfficialSupplyFallbackSupplyPressure)
        {
            errors.Add($"official_supply_pressure_clamp_floor must be between 0 and {MaxOfficialSupplyFallbackSupplyPressure}.");
        }

        if (OfficialSupplyPressureClampCeiling is < 0 or > MaxOfficialSupplyFallbackSupplyPressure)
        {
            errors.Add($"official_supply_pressure_clamp_ceiling must be between 0 and {MaxOfficialSupplyFallbackSupplyPressure}.");
        }

        if (OfficialSupplyPressureClampFloor > OfficialSupplyPressureClampCeiling)
        {
            errors.Add("official_supply_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyQuotaPressureClampFloor is < 0 or > MaxOfficialSupplyFallbackQuotaPressure)
        {
            errors.Add($"official_supply_quota_pressure_clamp_floor must be between 0 and {MaxOfficialSupplyFallbackQuotaPressure}.");
        }

        if (OfficialSupplyQuotaPressureClampCeiling is < 0 or > MaxOfficialSupplyFallbackQuotaPressure)
        {
            errors.Add($"official_supply_quota_pressure_clamp_ceiling must be between 0 and {MaxOfficialSupplyFallbackQuotaPressure}.");
        }

        if (OfficialSupplyQuotaPressureClampFloor > OfficialSupplyQuotaPressureClampCeiling)
        {
            errors.Add("official_supply_quota_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyDocketPressureClampFloor is < 0 or > MaxOfficialSupplyFallbackDocketPressure)
        {
            errors.Add($"official_supply_docket_pressure_clamp_floor must be between 0 and {MaxOfficialSupplyFallbackDocketPressure}.");
        }

        if (OfficialSupplyDocketPressureClampCeiling is < 0 or > MaxOfficialSupplyFallbackDocketPressure)
        {
            errors.Add($"official_supply_docket_pressure_clamp_ceiling must be between 0 and {MaxOfficialSupplyFallbackDocketPressure}.");
        }

        if (OfficialSupplyDocketPressureClampFloor > OfficialSupplyDocketPressureClampCeiling)
        {
            errors.Add("official_supply_docket_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyClerkDistortionPressureClampFloor is < 0 or > MaxOfficialSupplyFallbackClerkDistortionPressure)
        {
            errors.Add($"official_supply_clerk_distortion_pressure_clamp_floor must be between 0 and {MaxOfficialSupplyFallbackClerkDistortionPressure}.");
        }

        if (OfficialSupplyClerkDistortionPressureClampCeiling is < 0 or > MaxOfficialSupplyFallbackClerkDistortionPressure)
        {
            errors.Add($"official_supply_clerk_distortion_pressure_clamp_ceiling must be between 0 and {MaxOfficialSupplyFallbackClerkDistortionPressure}.");
        }

        if (OfficialSupplyClerkDistortionPressureClampFloor > OfficialSupplyClerkDistortionPressureClampCeiling)
        {
            errors.Add("official_supply_clerk_distortion_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyAuthorityBufferClampFloor is < 0 or > MaxOfficialSupplyFallbackAuthorityBuffer)
        {
            errors.Add($"official_supply_authority_buffer_clamp_floor must be between 0 and {MaxOfficialSupplyFallbackAuthorityBuffer}.");
        }

        if (OfficialSupplyAuthorityBufferClampCeiling is < 0 or > MaxOfficialSupplyFallbackAuthorityBuffer)
        {
            errors.Add($"official_supply_authority_buffer_clamp_ceiling must be between 0 and {MaxOfficialSupplyFallbackAuthorityBuffer}.");
        }

        if (OfficialSupplyAuthorityBufferClampFloor > OfficialSupplyAuthorityBufferClampCeiling)
        {
            errors.Add("official_supply_authority_buffer_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyLivelihoodExposureScoreWeights is null
            || OfficialSupplyLivelihoodExposureScoreWeights.Count == 0
            || OfficialSupplyLivelihoodExposureScoreWeights.Any(static entry =>
                !Enum.IsDefined(entry.Livelihood)
                || entry.Weight is < 0 or > MaxOfficialSupplyLivelihoodExposureContribution)
            || OfficialSupplyLivelihoodExposureScoreWeights
                .Select(static entry => entry.Livelihood)
                .Distinct()
                .Count() != OfficialSupplyLivelihoodExposureScoreWeights.Count)
        {
            errors.Add(
                $"official_supply_livelihood_exposure_score_weights must be non-empty, distinct, defined, and between 0 and {MaxOfficialSupplyLivelihoodExposureContribution}.");
        }

        if (OfficialSupplyLivelihoodExposureFallbackScore is < 0 or > MaxOfficialSupplyLivelihoodExposureContribution)
        {
            errors.Add(
                $"official_supply_livelihood_exposure_fallback_score must be between 0 and {MaxOfficialSupplyLivelihoodExposureContribution}.");
        }

        if (OfficialSupplyLandVisibilityScoreBands is null
            || OfficialSupplyLandVisibilityScoreBands.Count == 0
            || OfficialSupplyLandVisibilityScoreBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < 0 or > MaxOfficialSupplyLandVisibilityScore)
            || OfficialSupplyLandVisibilityScoreBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyLandVisibilityScoreBands.Count)
        {
            errors.Add(
                $"official_supply_land_visibility_score_bands must be non-empty, distinct, and between threshold 0..100 and score 0..{MaxOfficialSupplyLandVisibilityScore}.");
        }

        if (OfficialSupplyLandVisibilityScoreBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyLandVisibilityScoreBands.Count; index++)
            {
                if (OfficialSupplyLandVisibilityScoreBands[index - 1].Threshold <= OfficialSupplyLandVisibilityScoreBands[index].Threshold)
                {
                    errors.Add("official_supply_land_visibility_score_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyLandVisibilityFallbackScore is < 0 or > MaxOfficialSupplyLandVisibilityScore)
        {
            errors.Add(
                $"official_supply_land_visibility_fallback_score must be between 0 and {MaxOfficialSupplyLandVisibilityScore}.");
        }

        if (OfficialSupplyLivelihoodExposureClampFloor is < MinOfficialSupplyLivelihoodExposurePressure or > MaxOfficialSupplyLivelihoodExposurePressure)
        {
            errors.Add(
                $"official_supply_livelihood_exposure_clamp_floor must be between {MinOfficialSupplyLivelihoodExposurePressure} and {MaxOfficialSupplyLivelihoodExposurePressure}.");
        }

        if (OfficialSupplyLivelihoodExposureClampCeiling is < MinOfficialSupplyLivelihoodExposurePressure or > MaxOfficialSupplyLivelihoodExposurePressure)
        {
            errors.Add(
                $"official_supply_livelihood_exposure_clamp_ceiling must be between {MinOfficialSupplyLivelihoodExposurePressure} and {MaxOfficialSupplyLivelihoodExposurePressure}.");
        }

        if (OfficialSupplyLivelihoodExposureClampFloor > OfficialSupplyLivelihoodExposureClampCeiling)
        {
            errors.Add("official_supply_livelihood_exposure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyResourceGrainBufferScoreBands is null
            || OfficialSupplyResourceGrainBufferScoreBands.Count == 0
            || OfficialSupplyResourceGrainBufferScoreBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < 0 or > MaxOfficialSupplyResourceBufferContribution)
            || OfficialSupplyResourceGrainBufferScoreBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyResourceGrainBufferScoreBands.Count)
        {
            errors.Add(
                $"official_supply_resource_grain_buffer_score_bands must be non-empty, distinct, and between threshold 0..100 and score 0..{MaxOfficialSupplyResourceBufferContribution}.");
        }

        if (OfficialSupplyResourceGrainBufferScoreBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyResourceGrainBufferScoreBands.Count; index++)
            {
                if (OfficialSupplyResourceGrainBufferScoreBands[index - 1].Threshold <= OfficialSupplyResourceGrainBufferScoreBands[index].Threshold)
                {
                    errors.Add("official_supply_resource_grain_buffer_score_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyResourceGrainBufferFallbackScore is < 0 or > MaxOfficialSupplyResourceBufferContribution)
        {
            errors.Add(
                $"official_supply_resource_grain_buffer_fallback_score must be between 0 and {MaxOfficialSupplyResourceBufferContribution}.");
        }

        if (OfficialSupplyResourceToolConditionThreshold is < 0 or > 100)
        {
            errors.Add("official_supply_resource_tool_condition_threshold must be between 0 and 100.");
        }

        if (OfficialSupplyResourceToolBufferScore is < 0 or > MaxOfficialSupplyResourceBufferContribution)
        {
            errors.Add(
                $"official_supply_resource_tool_buffer_score must be between 0 and {MaxOfficialSupplyResourceBufferContribution}.");
        }

        if (OfficialSupplyResourceToolBufferFallbackScore is < 0 or > MaxOfficialSupplyResourceBufferContribution)
        {
            errors.Add(
                $"official_supply_resource_tool_buffer_fallback_score must be between 0 and {MaxOfficialSupplyResourceBufferContribution}.");
        }

        if (OfficialSupplyResourceShelterQualityThreshold is < 0 or > 100)
        {
            errors.Add("official_supply_resource_shelter_quality_threshold must be between 0 and 100.");
        }

        if (OfficialSupplyResourceShelterBufferScore is < 0 or > MaxOfficialSupplyResourceBufferContribution)
        {
            errors.Add(
                $"official_supply_resource_shelter_buffer_score must be between 0 and {MaxOfficialSupplyResourceBufferContribution}.");
        }

        if (OfficialSupplyResourceShelterBufferFallbackScore is < 0 or > MaxOfficialSupplyResourceBufferContribution)
        {
            errors.Add(
                $"official_supply_resource_shelter_buffer_fallback_score must be between 0 and {MaxOfficialSupplyResourceBufferContribution}.");
        }

        if (OfficialSupplyResourceBufferClampFloor is < MinOfficialSupplyResourceBufferPressure or > MaxOfficialSupplyResourceBufferPressure)
        {
            errors.Add(
                $"official_supply_resource_buffer_clamp_floor must be between {MinOfficialSupplyResourceBufferPressure} and {MaxOfficialSupplyResourceBufferPressure}.");
        }

        if (OfficialSupplyResourceBufferClampCeiling is < MinOfficialSupplyResourceBufferPressure or > MaxOfficialSupplyResourceBufferPressure)
        {
            errors.Add(
                $"official_supply_resource_buffer_clamp_ceiling must be between {MinOfficialSupplyResourceBufferPressure} and {MaxOfficialSupplyResourceBufferPressure}.");
        }

        if (OfficialSupplyResourceBufferClampFloor > OfficialSupplyResourceBufferClampCeiling)
        {
            errors.Add("official_supply_resource_buffer_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyLaborCapacityPressureBands is null
            || OfficialSupplyLaborCapacityPressureBands.Count == 0
            || OfficialSupplyLaborCapacityPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < MinOfficialSupplyLaborPressureContribution or > MaxOfficialSupplyLaborPressureContribution)
            || OfficialSupplyLaborCapacityPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyLaborCapacityPressureBands.Count)
        {
            errors.Add(
                $"official_supply_labor_capacity_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score {MinOfficialSupplyLaborPressureContribution}..{MaxOfficialSupplyLaborPressureContribution}.");
        }

        if (OfficialSupplyLaborCapacityPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyLaborCapacityPressureBands.Count; index++)
            {
                if (OfficialSupplyLaborCapacityPressureBands[index - 1].Threshold <= OfficialSupplyLaborCapacityPressureBands[index].Threshold)
                {
                    errors.Add("official_supply_labor_capacity_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyLaborCapacityPressureFallbackScore is < MinOfficialSupplyLaborPressureContribution or > MaxOfficialSupplyLaborPressureContribution)
        {
            errors.Add(
                $"official_supply_labor_capacity_pressure_fallback_score must be between {MinOfficialSupplyLaborPressureContribution} and {MaxOfficialSupplyLaborPressureContribution}.");
        }

        if (OfficialSupplyDependentCountPressureBands is null
            || OfficialSupplyDependentCountPressureBands.Count == 0
            || OfficialSupplyDependentCountPressureBands.Any(static band =>
                band.Threshold is < 0 or > MaxOfficialSupplyDependentCountThreshold
                || band.Score is < 0 or > MaxOfficialSupplyLaborPressureContribution)
            || OfficialSupplyDependentCountPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyDependentCountPressureBands.Count)
        {
            errors.Add(
                $"official_supply_dependent_count_pressure_bands must be non-empty, distinct, and between threshold 0..{MaxOfficialSupplyDependentCountThreshold} and score 0..{MaxOfficialSupplyLaborPressureContribution}.");
        }

        if (OfficialSupplyDependentCountPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyDependentCountPressureBands.Count; index++)
            {
                if (OfficialSupplyDependentCountPressureBands[index - 1].Threshold <= OfficialSupplyDependentCountPressureBands[index].Threshold)
                {
                    errors.Add("official_supply_dependent_count_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyDependentCountPressureFallbackScore is < 0 or > MaxOfficialSupplyLaborPressureContribution)
        {
            errors.Add(
                $"official_supply_dependent_count_pressure_fallback_score must be between 0 and {MaxOfficialSupplyLaborPressureContribution}.");
        }

        if (OfficialSupplyDependentToLaborRatioMultiplier is < 1 or > MaxOfficialSupplyDependentCountThreshold)
        {
            errors.Add(
                $"official_supply_dependent_to_labor_ratio_multiplier must be between 1 and {MaxOfficialSupplyDependentCountThreshold}.");
        }

        if (OfficialSupplyDependentToLaborRatioBonusScore is < 0 or > MaxOfficialSupplyLaborPressureContribution)
        {
            errors.Add(
                $"official_supply_dependent_to_labor_ratio_bonus_score must be between 0 and {MaxOfficialSupplyLaborPressureContribution}.");
        }

        if (OfficialSupplyDependentToLaborRatioFallbackScore is < 0 or > MaxOfficialSupplyLaborPressureContribution)
        {
            errors.Add(
                $"official_supply_dependent_to_labor_ratio_fallback_score must be between 0 and {MaxOfficialSupplyLaborPressureContribution}.");
        }

        if (OfficialSupplyLaborPressureClampFloor is < MinOfficialSupplyLaborPressure or > MaxOfficialSupplyLaborPressure)
        {
            errors.Add(
                $"official_supply_labor_pressure_clamp_floor must be between {MinOfficialSupplyLaborPressure} and {MaxOfficialSupplyLaborPressure}.");
        }

        if (OfficialSupplyLaborPressureClampCeiling is < MinOfficialSupplyLaborPressure or > MaxOfficialSupplyLaborPressure)
        {
            errors.Add(
                $"official_supply_labor_pressure_clamp_ceiling must be between {MinOfficialSupplyLaborPressure} and {MaxOfficialSupplyLaborPressure}.");
        }

        if (OfficialSupplyLaborPressureClampFloor > OfficialSupplyLaborPressureClampCeiling)
        {
            errors.Add("official_supply_labor_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyLiquidityGrainStrainPressureBands is null
            || OfficialSupplyLiquidityGrainStrainPressureBands.Count == 0
            || OfficialSupplyLiquidityGrainStrainPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < MinOfficialSupplyLiquidityPressureContribution or > MaxOfficialSupplyLiquidityPressureContribution)
            || OfficialSupplyLiquidityGrainStrainPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyLiquidityGrainStrainPressureBands.Count)
        {
            errors.Add(
                $"official_supply_liquidity_grain_strain_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score {MinOfficialSupplyLiquidityPressureContribution}..{MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityGrainStrainPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyLiquidityGrainStrainPressureBands.Count; index++)
            {
                if (OfficialSupplyLiquidityGrainStrainPressureBands[index - 1].Threshold <= OfficialSupplyLiquidityGrainStrainPressureBands[index].Threshold)
                {
                    errors.Add("official_supply_liquidity_grain_strain_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyLiquidityGrainStrainPressureFallbackScore is < MinOfficialSupplyLiquidityPressureContribution or > MaxOfficialSupplyLiquidityPressureContribution)
        {
            errors.Add(
                $"official_supply_liquidity_grain_strain_pressure_fallback_score must be between {MinOfficialSupplyLiquidityPressureContribution} and {MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityCashNeedPressureScore is < 0 or > MaxOfficialSupplyLiquidityPressureContribution)
        {
            errors.Add(
                $"official_supply_liquidity_cash_need_pressure_score must be between 0 and {MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityCashNeedPressureFallbackScore is < 0 or > MaxOfficialSupplyLiquidityPressureContribution)
        {
            errors.Add(
                $"official_supply_liquidity_cash_need_pressure_fallback_score must be between 0 and {MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityToolDragConditionThreshold is < 0 or > 100)
        {
            errors.Add("official_supply_liquidity_tool_drag_condition_threshold must be between 0 and 100.");
        }

        if (OfficialSupplyLiquidityToolDragPressureScore is < 0 or > MaxOfficialSupplyLiquidityPressureContribution)
        {
            errors.Add(
                $"official_supply_liquidity_tool_drag_pressure_score must be between 0 and {MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityToolDragPressureFallbackScore is < 0 or > MaxOfficialSupplyLiquidityPressureContribution)
        {
            errors.Add(
                $"official_supply_liquidity_tool_drag_pressure_fallback_score must be between 0 and {MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityDebtDragPressureBands is null
            || OfficialSupplyLiquidityDebtDragPressureBands.Count == 0
            || OfficialSupplyLiquidityDebtDragPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < 0 or > MaxOfficialSupplyLiquidityPressureContribution)
            || OfficialSupplyLiquidityDebtDragPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyLiquidityDebtDragPressureBands.Count)
        {
            errors.Add(
                $"official_supply_liquidity_debt_drag_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score 0..{MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityDebtDragPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyLiquidityDebtDragPressureBands.Count; index++)
            {
                if (OfficialSupplyLiquidityDebtDragPressureBands[index - 1].Threshold <= OfficialSupplyLiquidityDebtDragPressureBands[index].Threshold)
                {
                    errors.Add("official_supply_liquidity_debt_drag_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyLiquidityDebtDragPressureFallbackScore is < 0 or > MaxOfficialSupplyLiquidityPressureContribution)
        {
            errors.Add(
                $"official_supply_liquidity_debt_drag_pressure_fallback_score must be between 0 and {MaxOfficialSupplyLiquidityPressureContribution}.");
        }

        if (OfficialSupplyLiquidityPressureClampFloor is < MinOfficialSupplyLiquidityPressure or > MaxOfficialSupplyLiquidityPressure)
        {
            errors.Add(
                $"official_supply_liquidity_pressure_clamp_floor must be between {MinOfficialSupplyLiquidityPressure} and {MaxOfficialSupplyLiquidityPressure}.");
        }

        if (OfficialSupplyLiquidityPressureClampCeiling is < MinOfficialSupplyLiquidityPressure or > MaxOfficialSupplyLiquidityPressure)
        {
            errors.Add(
                $"official_supply_liquidity_pressure_clamp_ceiling must be between {MinOfficialSupplyLiquidityPressure} and {MaxOfficialSupplyLiquidityPressure}.");
        }

        if (OfficialSupplyLiquidityPressureClampFloor > OfficialSupplyLiquidityPressureClampCeiling)
        {
            errors.Add("official_supply_liquidity_pressure_clamp_floor must be less than or equal to ceiling.");
        }

        if (OfficialSupplyFragilityDistressPressureBands is null
            || OfficialSupplyFragilityDistressPressureBands.Count == 0
            || OfficialSupplyFragilityDistressPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
            || OfficialSupplyFragilityDistressPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyFragilityDistressPressureBands.Count)
        {
            errors.Add(
                $"official_supply_fragility_distress_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score 0..{MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityDistressPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyFragilityDistressPressureBands.Count; index++)
            {
                if (OfficialSupplyFragilityDistressPressureBands[index - 1].Threshold <= OfficialSupplyFragilityDistressPressureBands[index].Threshold)
                {
                    errors.Add("official_supply_fragility_distress_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyFragilityDistressPressureFallbackScore is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
        {
            errors.Add(
                $"official_supply_fragility_distress_pressure_fallback_score must be between 0 and {MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityDebtPressureBands is null
            || OfficialSupplyFragilityDebtPressureBands.Count == 0
            || OfficialSupplyFragilityDebtPressureBands.Any(static band =>
                band.Threshold is < 0 or > 100
                || band.Score is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
            || OfficialSupplyFragilityDebtPressureBands.Select(static band => band.Threshold).Distinct().Count()
                != OfficialSupplyFragilityDebtPressureBands.Count)
        {
            errors.Add(
                $"official_supply_fragility_debt_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score 0..{MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityDebtPressureBands is { Count: > 1 })
        {
            for (int index = 1; index < OfficialSupplyFragilityDebtPressureBands.Count; index++)
            {
                if (OfficialSupplyFragilityDebtPressureBands[index - 1].Threshold <= OfficialSupplyFragilityDebtPressureBands[index].Threshold)
                {
                    errors.Add("official_supply_fragility_debt_pressure_bands must be ordered by descending threshold.");
                    break;
                }
            }
        }

        if (OfficialSupplyFragilityDebtPressureFallbackScore is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
        {
            errors.Add(
                $"official_supply_fragility_debt_pressure_fallback_score must be between 0 and {MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityMigrationRiskThreshold is < 0 or > 100)
        {
            errors.Add("official_supply_fragility_migration_risk_threshold must be between 0 and 100.");
        }

        if (OfficialSupplyFragilityMigrationPressureScore is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
        {
            errors.Add(
                $"official_supply_fragility_migration_pressure_score must be between 0 and {MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityMigrationPressureFallbackScore is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
        {
            errors.Add(
                $"official_supply_fragility_migration_pressure_fallback_score must be between 0 and {MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityShelterDragQualityThreshold is < 0 or > 100)
        {
            errors.Add("official_supply_fragility_shelter_drag_quality_threshold must be between 0 and 100.");
        }

        if (OfficialSupplyFragilityShelterDragPressureScore is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
        {
            errors.Add(
                $"official_supply_fragility_shelter_drag_pressure_score must be between 0 and {MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityShelterDragPressureFallbackScore is < 0 or > MaxOfficialSupplyFragilityPressureContribution)
        {
            errors.Add(
                $"official_supply_fragility_shelter_drag_pressure_fallback_score must be between 0 and {MaxOfficialSupplyFragilityPressureContribution}.");
        }

        if (OfficialSupplyFragilityPressureClampFloor is < MinOfficialSupplyFragilityPressure or > MaxOfficialSupplyFragilityPressure)
        {
            errors.Add(
                $"official_supply_fragility_pressure_clamp_floor must be between {MinOfficialSupplyFragilityPressure} and {MaxOfficialSupplyFragilityPressure}.");
        }

        if (OfficialSupplyFragilityPressureClampCeiling is < MinOfficialSupplyFragilityPressure or > MaxOfficialSupplyFragilityPressure)
        {
            errors.Add(
                $"official_supply_fragility_pressure_clamp_ceiling must be between {MinOfficialSupplyFragilityPressure} and {MaxOfficialSupplyFragilityPressure}.");
        }

        if (OfficialSupplyFragilityPressureClampFloor > OfficialSupplyFragilityPressureClampCeiling)
        {
            errors.Add("official_supply_fragility_pressure_clamp_floor must be less than or equal to ceiling.");
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

        if (OfficialSupplyBurdenEventDistressThreshold is < 0 or > 100)
        {
            errors.Add("official_supply_burden_event_distress_threshold must be between 0 and 100.");
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

    public int GetOfficialSupplyFallbackFrontierPressureOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFallbackFrontierPressure
            : DefaultOfficialSupplyFallbackFrontierPressure;
    }

    public int GetOfficialSupplyFallbackQuotaPressureOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFallbackQuotaPressure
            : DefaultOfficialSupplyFallbackQuotaPressure;
    }

    public int GetOfficialSupplyFallbackDocketPressureOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFallbackDocketPressure
            : DefaultOfficialSupplyFallbackDocketPressure;
    }

    public int GetOfficialSupplyFallbackClerkDistortionPressureOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFallbackClerkDistortionPressure
            : DefaultOfficialSupplyFallbackClerkDistortionPressure;
    }

    public int GetOfficialSupplyFallbackAuthorityBufferOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFallbackAuthorityBuffer
            : DefaultOfficialSupplyFallbackAuthorityBuffer;
    }

    public int GetOfficialSupplyFallbackDerivedPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFallbackDerivedPressureClampFloor
            : DefaultOfficialSupplyFallbackDerivedPressureClampFloor;
    }

    public int GetOfficialSupplyFallbackDerivedPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFallbackDerivedPressureClampCeiling
            : DefaultOfficialSupplyFallbackDerivedPressureClampCeiling;
    }

    public int GetOfficialSupplyFrontierPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFrontierPressureClampFloor
            : DefaultOfficialSupplyFrontierPressureClampFloor;
    }

    public int GetOfficialSupplyFrontierPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFrontierPressureClampCeiling
            : DefaultOfficialSupplyFrontierPressureClampCeiling;
    }

    public int GetOfficialSupplyPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyPressureClampFloor
            : DefaultOfficialSupplyPressureClampFloor;
    }

    public int GetOfficialSupplyPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyPressureClampCeiling
            : DefaultOfficialSupplyPressureClampCeiling;
    }

    public int GetOfficialSupplyQuotaPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyQuotaPressureClampFloor
            : DefaultOfficialSupplyQuotaPressureClampFloor;
    }

    public int GetOfficialSupplyQuotaPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyQuotaPressureClampCeiling
            : DefaultOfficialSupplyQuotaPressureClampCeiling;
    }

    public int GetOfficialSupplyDocketPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDocketPressureClampFloor
            : DefaultOfficialSupplyDocketPressureClampFloor;
    }

    public int GetOfficialSupplyDocketPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDocketPressureClampCeiling
            : DefaultOfficialSupplyDocketPressureClampCeiling;
    }

    public int GetOfficialSupplyClerkDistortionPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyClerkDistortionPressureClampFloor
            : DefaultOfficialSupplyClerkDistortionPressureClampFloor;
    }

    public int GetOfficialSupplyClerkDistortionPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyClerkDistortionPressureClampCeiling
            : DefaultOfficialSupplyClerkDistortionPressureClampCeiling;
    }

    public int GetOfficialSupplyAuthorityBufferClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyAuthorityBufferClampFloor
            : DefaultOfficialSupplyAuthorityBufferClampFloor;
    }

    public int GetOfficialSupplyAuthorityBufferClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyAuthorityBufferClampCeiling
            : DefaultOfficialSupplyAuthorityBufferClampCeiling;
    }

    public IReadOnlyList<PopulationHouseholdMobilityLivelihoodScoreWeight>
        GetOfficialSupplyLivelihoodExposureScoreWeightsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLivelihoodExposureScoreWeights
            : DefaultOfficialSupplyLivelihoodExposureScoreWeights;
    }

    public int GetOfficialSupplyLivelihoodExposureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLivelihoodExposureFallbackScore
            : DefaultOfficialSupplyLivelihoodExposureFallbackScore;
    }

    public int GetOfficialSupplyLivelihoodExposureScoreOrDefault(LivelihoodType livelihood)
    {
        foreach (PopulationHouseholdMobilityLivelihoodScoreWeight entry in
                 GetOfficialSupplyLivelihoodExposureScoreWeightsOrDefault())
        {
            if (entry.Livelihood == livelihood)
            {
                return entry.Weight;
            }
        }

        return GetOfficialSupplyLivelihoodExposureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyLandVisibilityScoreBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLandVisibilityScoreBands
            : DefaultOfficialSupplyLandVisibilityScoreBands;
    }

    public int GetOfficialSupplyLandVisibilityFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLandVisibilityFallbackScore
            : DefaultOfficialSupplyLandVisibilityFallbackScore;
    }

    public int GetOfficialSupplyLandVisibilityScoreOrDefault(int landHolding)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyLandVisibilityScoreBandsOrDefault())
        {
            if (landHolding >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyLandVisibilityFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyLivelihoodExposureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLivelihoodExposureClampFloor
            : DefaultOfficialSupplyLivelihoodExposureClampFloor;
    }

    public int GetOfficialSupplyLivelihoodExposureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLivelihoodExposureClampCeiling
            : DefaultOfficialSupplyLivelihoodExposureClampCeiling;
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyResourceGrainBufferScoreBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceGrainBufferScoreBands
            : DefaultOfficialSupplyResourceGrainBufferScoreBands;
    }

    public int GetOfficialSupplyResourceGrainBufferFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceGrainBufferFallbackScore
            : DefaultOfficialSupplyResourceGrainBufferFallbackScore;
    }

    public int GetOfficialSupplyResourceGrainBufferScoreOrDefault(int grainStore)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyResourceGrainBufferScoreBandsOrDefault())
        {
            if (grainStore >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyResourceGrainBufferFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyResourceToolConditionThresholdOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceToolConditionThreshold
            : DefaultOfficialSupplyResourceToolConditionThreshold;
    }

    public int GetOfficialSupplyResourceToolBufferScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceToolBufferScore
            : DefaultOfficialSupplyResourceToolBufferScore;
    }

    public int GetOfficialSupplyResourceToolBufferFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceToolBufferFallbackScore
            : DefaultOfficialSupplyResourceToolBufferFallbackScore;
    }

    public int GetOfficialSupplyResourceToolBufferScoreOrDefault(int toolCondition)
    {
        return toolCondition >= GetOfficialSupplyResourceToolConditionThresholdOrDefault()
            ? GetOfficialSupplyResourceToolBufferScoreOrDefault()
            : GetOfficialSupplyResourceToolBufferFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyResourceShelterQualityThresholdOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceShelterQualityThreshold
            : DefaultOfficialSupplyResourceShelterQualityThreshold;
    }

    public int GetOfficialSupplyResourceShelterBufferScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceShelterBufferScore
            : DefaultOfficialSupplyResourceShelterBufferScore;
    }

    public int GetOfficialSupplyResourceShelterBufferFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceShelterBufferFallbackScore
            : DefaultOfficialSupplyResourceShelterBufferFallbackScore;
    }

    public int GetOfficialSupplyResourceShelterBufferScoreOrDefault(int shelterQuality)
    {
        return shelterQuality >= GetOfficialSupplyResourceShelterQualityThresholdOrDefault()
            ? GetOfficialSupplyResourceShelterBufferScoreOrDefault()
            : GetOfficialSupplyResourceShelterBufferFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyResourceBufferClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceBufferClampFloor
            : DefaultOfficialSupplyResourceBufferClampFloor;
    }

    public int GetOfficialSupplyResourceBufferClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyResourceBufferClampCeiling
            : DefaultOfficialSupplyResourceBufferClampCeiling;
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyLaborCapacityPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLaborCapacityPressureBands
            : DefaultOfficialSupplyLaborCapacityPressureBands;
    }

    public int GetOfficialSupplyLaborCapacityPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLaborCapacityPressureFallbackScore
            : DefaultOfficialSupplyLaborCapacityPressureFallbackScore;
    }

    public int GetOfficialSupplyLaborCapacityPressureScoreOrDefault(int laborCapacity)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyLaborCapacityPressureBandsOrDefault())
        {
            if (laborCapacity >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyLaborCapacityPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyDependentCountPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDependentCountPressureBands
            : DefaultOfficialSupplyDependentCountPressureBands;
    }

    public int GetOfficialSupplyDependentCountPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDependentCountPressureFallbackScore
            : DefaultOfficialSupplyDependentCountPressureFallbackScore;
    }

    public int GetOfficialSupplyDependentCountPressureScoreOrDefault(int dependentCount)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyDependentCountPressureBandsOrDefault())
        {
            if (dependentCount >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyDependentCountPressureFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyDependentToLaborRatioMultiplierOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDependentToLaborRatioMultiplier
            : DefaultOfficialSupplyDependentToLaborRatioMultiplier;
    }

    public int GetOfficialSupplyDependentToLaborRatioBonusScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDependentToLaborRatioBonusScore
            : DefaultOfficialSupplyDependentToLaborRatioBonusScore;
    }

    public int GetOfficialSupplyDependentToLaborRatioFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyDependentToLaborRatioFallbackScore
            : DefaultOfficialSupplyDependentToLaborRatioFallbackScore;
    }

    public int GetOfficialSupplyDependentToLaborRatioScoreOrDefault(int laborerCount, int dependentCount)
    {
        return laborerCount > 0
            && dependentCount > laborerCount * GetOfficialSupplyDependentToLaborRatioMultiplierOrDefault()
                ? GetOfficialSupplyDependentToLaborRatioBonusScoreOrDefault()
                : GetOfficialSupplyDependentToLaborRatioFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyLaborPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLaborPressureClampFloor
            : DefaultOfficialSupplyLaborPressureClampFloor;
    }

    public int GetOfficialSupplyLaborPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLaborPressureClampCeiling
            : DefaultOfficialSupplyLaborPressureClampCeiling;
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyLiquidityGrainStrainPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityGrainStrainPressureBands
            : DefaultOfficialSupplyLiquidityGrainStrainPressureBands;
    }

    public int GetOfficialSupplyLiquidityGrainStrainPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityGrainStrainPressureFallbackScore
            : DefaultOfficialSupplyLiquidityGrainStrainPressureFallbackScore;
    }

    public int GetOfficialSupplyLiquidityGrainStrainPressureScoreOrDefault(int grainStore)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyLiquidityGrainStrainPressureBandsOrDefault())
        {
            if (grainStore >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyLiquidityGrainStrainPressureFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyLiquidityCashNeedPressureScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityCashNeedPressureScore
            : DefaultOfficialSupplyLiquidityCashNeedPressureScore;
    }

    public int GetOfficialSupplyLiquidityCashNeedPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityCashNeedPressureFallbackScore
            : DefaultOfficialSupplyLiquidityCashNeedPressureFallbackScore;
    }

    public int GetOfficialSupplyLiquidityToolDragConditionThresholdOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityToolDragConditionThreshold
            : DefaultOfficialSupplyLiquidityToolDragConditionThreshold;
    }

    public int GetOfficialSupplyLiquidityToolDragPressureScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityToolDragPressureScore
            : DefaultOfficialSupplyLiquidityToolDragPressureScore;
    }

    public int GetOfficialSupplyLiquidityToolDragPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityToolDragPressureFallbackScore
            : DefaultOfficialSupplyLiquidityToolDragPressureFallbackScore;
    }

    public int GetOfficialSupplyLiquidityToolDragPressureScoreOrDefault(int toolCondition)
    {
        return toolCondition is > 0
            && toolCondition < GetOfficialSupplyLiquidityToolDragConditionThresholdOrDefault()
                ? GetOfficialSupplyLiquidityToolDragPressureScoreOrDefault()
                : GetOfficialSupplyLiquidityToolDragPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyLiquidityDebtDragPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityDebtDragPressureBands
            : DefaultOfficialSupplyLiquidityDebtDragPressureBands;
    }

    public int GetOfficialSupplyLiquidityDebtDragPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityDebtDragPressureFallbackScore
            : DefaultOfficialSupplyLiquidityDebtDragPressureFallbackScore;
    }

    public int GetOfficialSupplyLiquidityDebtDragPressureScoreOrDefault(int debtPressure)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyLiquidityDebtDragPressureBandsOrDefault())
        {
            if (debtPressure >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyLiquidityDebtDragPressureFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyLiquidityPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityPressureClampFloor
            : DefaultOfficialSupplyLiquidityPressureClampFloor;
    }

    public int GetOfficialSupplyLiquidityPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyLiquidityPressureClampCeiling
            : DefaultOfficialSupplyLiquidityPressureClampCeiling;
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyFragilityDistressPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityDistressPressureBands
            : DefaultOfficialSupplyFragilityDistressPressureBands;
    }

    public int GetOfficialSupplyFragilityDistressPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityDistressPressureFallbackScore
            : DefaultOfficialSupplyFragilityDistressPressureFallbackScore;
    }

    public int GetOfficialSupplyFragilityDistressPressureScoreOrDefault(int distress)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyFragilityDistressPressureBandsOrDefault())
        {
            if (distress >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyFragilityDistressPressureFallbackScoreOrDefault();
    }

    public IReadOnlyList<PopulationHouseholdMobilityThresholdScoreBand>
        GetOfficialSupplyFragilityDebtPressureBandsOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityDebtPressureBands
            : DefaultOfficialSupplyFragilityDebtPressureBands;
    }

    public int GetOfficialSupplyFragilityDebtPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityDebtPressureFallbackScore
            : DefaultOfficialSupplyFragilityDebtPressureFallbackScore;
    }

    public int GetOfficialSupplyFragilityDebtPressureScoreOrDefault(int debtPressure)
    {
        foreach (PopulationHouseholdMobilityThresholdScoreBand band in
                 GetOfficialSupplyFragilityDebtPressureBandsOrDefault())
        {
            if (debtPressure >= band.Threshold)
            {
                return band.Score;
            }
        }

        return GetOfficialSupplyFragilityDebtPressureFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyFragilityMigrationRiskThresholdOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityMigrationRiskThreshold
            : DefaultOfficialSupplyFragilityMigrationRiskThreshold;
    }

    public int GetOfficialSupplyFragilityMigrationPressureScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityMigrationPressureScore
            : DefaultOfficialSupplyFragilityMigrationPressureScore;
    }

    public int GetOfficialSupplyFragilityMigrationPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityMigrationPressureFallbackScore
            : DefaultOfficialSupplyFragilityMigrationPressureFallbackScore;
    }

    public int GetOfficialSupplyFragilityMigrationPressureScoreOrDefault(
        bool isMigrating,
        int migrationRisk)
    {
        return isMigrating || migrationRisk >= GetOfficialSupplyFragilityMigrationRiskThresholdOrDefault()
            ? GetOfficialSupplyFragilityMigrationPressureScoreOrDefault()
            : GetOfficialSupplyFragilityMigrationPressureFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyFragilityShelterDragQualityThresholdOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityShelterDragQualityThreshold
            : DefaultOfficialSupplyFragilityShelterDragQualityThreshold;
    }

    public int GetOfficialSupplyFragilityShelterDragPressureScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityShelterDragPressureScore
            : DefaultOfficialSupplyFragilityShelterDragPressureScore;
    }

    public int GetOfficialSupplyFragilityShelterDragPressureFallbackScoreOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityShelterDragPressureFallbackScore
            : DefaultOfficialSupplyFragilityShelterDragPressureFallbackScore;
    }

    public int GetOfficialSupplyFragilityShelterDragPressureScoreOrDefault(int shelterQuality)
    {
        return shelterQuality is > 0
            && shelterQuality < GetOfficialSupplyFragilityShelterDragQualityThresholdOrDefault()
                ? GetOfficialSupplyFragilityShelterDragPressureScoreOrDefault()
                : GetOfficialSupplyFragilityShelterDragPressureFallbackScoreOrDefault();
    }

    public int GetOfficialSupplyFragilityPressureClampFloorOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityPressureClampFloor
            : DefaultOfficialSupplyFragilityPressureClampFloor;
    }

    public int GetOfficialSupplyFragilityPressureClampCeilingOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyFragilityPressureClampCeiling
            : DefaultOfficialSupplyFragilityPressureClampCeiling;
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

    public int GetOfficialSupplyBurdenEventDistressThresholdOrDefault()
    {
        return Validate().IsValid
            ? OfficialSupplyBurdenEventDistressThreshold
            : DefaultOfficialSupplyBurdenEventDistressThreshold;
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
