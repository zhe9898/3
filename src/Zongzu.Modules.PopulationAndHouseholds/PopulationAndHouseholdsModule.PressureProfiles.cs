using System;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private static SettlementId? TryParseSettlementId(string? entityKey)
    {
        return int.TryParse(entityKey, out int settlementId)
            ? new SettlementId(settlementId)
            : null;
    }

    private GrainPriceShockSignal ResolveGrainPriceShockSignal(IDomainEvent domainEvent)
    {
        int currentPriceClampFloor = _householdMobilityRulesData.GetGrainPriceShockCurrentPriceClampFloorOrDefault();
        int currentPriceClampCeiling = _householdMobilityRulesData.GetGrainPriceShockCurrentPriceClampCeilingOrDefault();
        int priceDeltaClampFloor = _householdMobilityRulesData.GetGrainPriceShockPriceDeltaClampFloorOrDefault();
        int priceDeltaClampCeiling = _householdMobilityRulesData.GetGrainPriceShockPriceDeltaClampCeilingOrDefault();
        int supplyClampFloor = _householdMobilityRulesData.GetGrainPriceShockSupplyClampFloorOrDefault();
        int supplyClampCeiling = _householdMobilityRulesData.GetGrainPriceShockSupplyClampCeilingOrDefault();
        int demandClampFloor = _householdMobilityRulesData.GetGrainPriceShockDemandClampFloorOrDefault();
        int demandClampCeiling = _householdMobilityRulesData.GetGrainPriceShockDemandClampCeilingOrDefault();
        int currentPrice = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.GrainCurrentPrice,
            _householdMobilityRulesData.GetGrainPriceShockDefaultCurrentPriceOrDefault());
        int oldPrice = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.GrainOldPrice,
            _householdMobilityRulesData.GetGrainPriceShockDefaultOldPriceOrDefault());
        int priceDelta = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.GrainPriceDelta,
            Math.Max(0, currentPrice - oldPrice));
        int supply = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.GrainSupply,
            _householdMobilityRulesData.GetGrainPriceShockDefaultSupplyOrDefault());
        int demand = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.GrainDemand,
            _householdMobilityRulesData.GetGrainPriceShockDefaultDemandOrDefault());

        return new GrainPriceShockSignal(
            Math.Clamp(currentPrice, currentPriceClampFloor, currentPriceClampCeiling),
            Math.Clamp(Math.Max(0, priceDelta), priceDeltaClampFloor, priceDeltaClampCeiling),
            Math.Clamp(supply, supplyClampFloor, supplyClampCeiling),
            Math.Clamp(demand, demandClampFloor, demandClampCeiling));
    }

    private static int ReadMetadataInt(IDomainEvent domainEvent, string key, int fallback)
    {
        return domainEvent.Metadata.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed)
            ? parsed
            : fallback;
    }

    private SubsistencePressureProfile ComputeSubsistencePressureProfile(
        PopulationHouseholdState household,
        GrainPriceShockSignal signal)
    {
        return new SubsistencePressureProfile(
            ComputePricePressure(signal),
            ComputeGrainBufferPressure(household),
            ComputeMarketDependencyPressure(household),
            ComputeSubsistenceLaborPressure(household),
            ComputeSubsistenceFragilityPressure(household),
            ComputeSubsistenceInteractionPressure(household),
            _householdMobilityRulesData.GetSubsistencePressureDistressDeltaClampFloorOrDefault(),
            _householdMobilityRulesData.GetSubsistencePressureDistressDeltaClampCeilingOrDefault());
    }

    private int ComputePricePressure(GrainPriceShockSignal signal)
    {
        int priceLevel = _householdMobilityRulesData.GetGrainPriceLevelPressureScoreOrDefault(signal.CurrentPrice);
        int priceJump = _householdMobilityRulesData.GetGrainPriceJumpPressureScoreOrDefault(signal.PriceDelta);
        int marketTightness = _householdMobilityRulesData.GetGrainPriceMarketTightnessPressureScoreOrDefault(
            Math.Max(0, signal.Demand - signal.Supply));

        return Math.Clamp(
            priceLevel + priceJump + marketTightness,
            _householdMobilityRulesData.GetGrainPricePressureClampFloorOrDefault(),
            _householdMobilityRulesData.GetGrainPricePressureClampCeilingOrDefault());
    }

    private int ComputeGrainBufferPressure(PopulationHouseholdState household)
    {
        return _householdMobilityRulesData.GetSubsistenceGrainBufferPressureScoreOrDefault(household.GrainStore);
    }

    private int ComputeMarketDependencyPressure(PopulationHouseholdState household)
    {
        return _householdMobilityRulesData.GetSubsistenceMarketDependencyPressureScoreOrDefault(household.Livelihood);
    }

    private int ComputeSubsistenceLaborPressure(PopulationHouseholdState household)
    {
        int laborPressure = _householdMobilityRulesData.GetSubsistenceLaborCapacityPressureScoreOrDefault(
            household.LaborCapacity);

        int dependentPressure = _householdMobilityRulesData.GetSubsistenceDependentCountPressureScoreOrDefault(
            household.DependentCount);

        return Math.Clamp(
            laborPressure + dependentPressure,
            _householdMobilityRulesData.GetSubsistenceLaborPressureClampFloorOrDefault(),
            _householdMobilityRulesData.GetSubsistenceLaborPressureClampCeilingOrDefault());
    }

    private int ComputeSubsistenceFragilityPressure(PopulationHouseholdState household)
    {
        int distressPressure = _householdMobilityRulesData.GetSubsistenceFragilityDistressPressureScoreOrDefault(
            household.Distress);

        int debtPressure = _householdMobilityRulesData.GetSubsistenceFragilityDebtPressureScoreOrDefault(
            household.DebtPressure);

        int migrationPressure = _householdMobilityRulesData.GetSubsistenceFragilityMigrationPressureScoreOrDefault(
            household.IsMigrating,
            household.MigrationRisk);
        return Math.Clamp(
            distressPressure + debtPressure + migrationPressure,
            _householdMobilityRulesData.GetSubsistenceFragilityPressureClampFloorOrDefault(),
            _householdMobilityRulesData.GetSubsistenceFragilityPressureClampCeilingOrDefault());
    }

    private int ComputeSubsistenceInteractionPressure(PopulationHouseholdState household)
    {
        int interaction = 0;
        bool isGrainShortage =
            _householdMobilityRulesData.IsSubsistenceInteractionGrainShortageStoreOrDefault(household.GrainStore);

        if (isGrainShortage && IsCashNeedLivelihood(household.Livelihood))
        {
            interaction += _householdMobilityRulesData.GetSubsistenceInteractionCashNeedBoostScoreOrDefault();
        }

        if (isGrainShortage
            && _householdMobilityRulesData.IsSubsistenceInteractionDebtPressureOrDefault(household.DebtPressure))
        {
            interaction += _householdMobilityRulesData.GetSubsistenceInteractionDebtPressureBoostScoreOrDefault();
        }

        if (_householdMobilityRulesData.IsSubsistenceInteractionResilienceReliefOrDefault(
                household.GrainStore,
                household.LandHolding,
                household.LaborCapacity))
        {
            interaction -= _householdMobilityRulesData.GetSubsistenceInteractionResilienceReliefScoreOrDefault();
        }

        return Math.Clamp(
            interaction,
            _householdMobilityRulesData.GetSubsistenceInteractionPressureClampFloorOrDefault(),
            _householdMobilityRulesData.GetSubsistenceInteractionPressureClampCeilingOrDefault());
    }

    private static SettlementId? ResolveSettlementScope(IDomainEvent domainEvent)
    {
        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.SettlementId, out string? metadataSettlementId)
            && int.TryParse(metadataSettlementId, out int metadataValue))
        {
            return new SettlementId(metadataValue);
        }

        return TryParseSettlementId(domainEvent.EntityKey);
    }

    private TaxSeasonBurdenProfile ComputeTaxSeasonBurdenProfile(PopulationHouseholdState household)
    {
        return new TaxSeasonBurdenProfile(
            ComputeRegistrationVisibilityPressure(household),
            ComputeTaxLiquidityPressure(household),
            ComputeTaxLaborPressure(household),
            ComputeTaxSeasonFragility(household),
            ComputeTaxInteractionPressure(household),
            _householdMobilityRulesData.GetTaxSeasonDebtDeltaClampFloorOrDefault(),
            _householdMobilityRulesData.GetTaxSeasonDebtDeltaClampCeilingOrDefault());
    }

    private static int ComputeRegistrationVisibilityPressure(PopulationHouseholdState household)
    {
        int livelihoodExposure = household.Livelihood switch
        {
            LivelihoodType.Tenant => 4,
            LivelihoodType.Boatman => 3,
            LivelihoodType.PettyTrader => 3,
            LivelihoodType.Artisan => 2,
            LivelihoodType.Smallholder => 3,
            LivelihoodType.HiredLabor => 2,
            LivelihoodType.SeasonalMigrant => 2,
            LivelihoodType.Unknown => 2,
            LivelihoodType.DomesticServant => 1,
            LivelihoodType.YamenRunner => 1,
            LivelihoodType.Vagrant => 1,
            _ => 2,
        };

        int landVisibility = household.LandHolding switch
        {
            >= 80 => 4,
            >= 40 => 3,
            >= 15 => 2,
            > 0 => 1,
            _ => 0,
        };

        return Math.Clamp(livelihoodExposure + landVisibility, 1, 7);
    }

    private static int ComputeTaxLiquidityPressure(PopulationHouseholdState household)
    {
        int grainPressure = household.GrainStore switch
        {
            >= 80 => -3,
            >= 60 => -2,
            >= 40 => -1,
            >= 20 => 1,
            > 0 => 3,
            _ => 0,
        };

        int cashNeed = household.Livelihood switch
        {
            LivelihoodType.PettyTrader => 2,
            LivelihoodType.Boatman => 2,
            LivelihoodType.Artisan => 2,
            LivelihoodType.SeasonalMigrant => 2,
            LivelihoodType.HiredLabor => 1,
            LivelihoodType.Vagrant => 1,
            LivelihoodType.Tenant => 1,
            _ => 0,
        };

        int toolDrag = household.ToolCondition is > 0 and < 35 ? 1 : 0;
        return Math.Clamp(grainPressure + cashNeed + toolDrag, -3, 5);
    }

    private static int ComputeTaxLaborPressure(PopulationHouseholdState household)
    {
        int laborPressure = household.LaborCapacity switch
        {
            >= 80 => -2,
            >= 60 => -1,
            >= 40 => 0,
            >= 30 => 1,
            >= 20 => 2,
            _ => 3,
        };

        int dependencyPressure = household.DependentCount switch
        {
            >= 5 => 2,
            >= 3 => 1,
            _ => 0,
        };

        if (household.DependentCount > 0 && household.LaborerCount > 0 && household.DependentCount > household.LaborerCount * 2)
        {
            dependencyPressure += 1;
        }

        return Math.Clamp(laborPressure + dependencyPressure, -2, 5);
    }

    private static int ComputeTaxSeasonFragility(PopulationHouseholdState household)
    {
        int distressPressure = household.Distress switch
        {
            >= 80 => 3,
            >= 65 => 2,
            >= 50 => 1,
            _ => 0,
        };

        int debtPressure = household.DebtPressure switch
        {
            >= 80 => 3,
            >= 65 => 2,
            >= 55 => 1,
            _ => 0,
        };

        int shelterDrag = household.ShelterQuality is > 0 and < 35 ? 1 : 0;
        int migrationDrag = household.IsMigrating || household.MigrationRisk >= 70 ? 1 : 0;
        return Math.Clamp(distressPressure + debtPressure + shelterDrag + migrationDrag, 0, 7);
    }

    private static int ComputeTaxInteractionPressure(PopulationHouseholdState household)
    {
        int interaction = 0;

        if (household.Livelihood == LivelihoodType.Tenant
            && household.Distress >= 65
            && household.GrainStore is > 0 and < 25)
        {
            interaction += 2;
        }

        if (household.LandHolding >= 40 && household.LaborCapacity < 35)
        {
            interaction += 1;
        }

        if (IsCashNeedLivelihood(household.Livelihood)
            && household.GrainStore is > 0 and < 30
            && household.DebtPressure >= 60)
        {
            interaction += 1;
        }

        if (household.GrainStore >= 70
            && household.LaborCapacity >= 70
            && household.DebtPressure < 55
            && household.Distress < 45)
        {
            interaction -= 2;
        }

        return Math.Clamp(interaction, -2, 4);
    }

    private static bool IsCashNeedLivelihood(LivelihoodType livelihood)
    {
        return livelihood is LivelihoodType.PettyTrader
            or LivelihoodType.Boatman
            or LivelihoodType.Artisan
            or LivelihoodType.SeasonalMigrant
            or LivelihoodType.HiredLabor;
    }

    private OfficialSupplySignal ResolveOfficialSupplySignal(IDomainEvent domainEvent)
    {
        int frontierPressure = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.FrontierPressure,
            _householdMobilityRulesData.GetOfficialSupplyFallbackFrontierPressureOrDefault());
        int quotaPressure = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.OfficialSupplyQuotaPressure,
            _householdMobilityRulesData.GetOfficialSupplyFallbackQuotaPressureOrDefault());
        int docketPressure = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.OfficialSupplyDocketPressure,
            _householdMobilityRulesData.GetOfficialSupplyFallbackDocketPressureOrDefault());
        int clerkDistortionPressure = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure,
            _householdMobilityRulesData.GetOfficialSupplyFallbackClerkDistortionPressureOrDefault());
        int authorityBuffer = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer,
            _householdMobilityRulesData.GetOfficialSupplyFallbackAuthorityBufferOrDefault());
        int supplyPressure = ReadMetadataInt(
            domainEvent,
            DomainEventMetadataKeys.OfficialSupplyPressure,
            Math.Clamp(
                quotaPressure + docketPressure + clerkDistortionPressure - authorityBuffer,
                _householdMobilityRulesData.GetOfficialSupplyFallbackDerivedPressureClampFloorOrDefault(),
                _householdMobilityRulesData.GetOfficialSupplyFallbackDerivedPressureClampCeilingOrDefault()));

        return new OfficialSupplySignal(
            Math.Clamp(
                frontierPressure,
                _householdMobilityRulesData.GetOfficialSupplyFrontierPressureClampFloorOrDefault(),
                _householdMobilityRulesData.GetOfficialSupplyFrontierPressureClampCeilingOrDefault()),
            Math.Clamp(
                supplyPressure,
                _householdMobilityRulesData.GetOfficialSupplyPressureClampFloorOrDefault(),
                _householdMobilityRulesData.GetOfficialSupplyPressureClampCeilingOrDefault()),
            Math.Clamp(
                quotaPressure,
                _householdMobilityRulesData.GetOfficialSupplyQuotaPressureClampFloorOrDefault(),
                _householdMobilityRulesData.GetOfficialSupplyQuotaPressureClampCeilingOrDefault()),
            Math.Clamp(
                docketPressure,
                _householdMobilityRulesData.GetOfficialSupplyDocketPressureClampFloorOrDefault(),
                _householdMobilityRulesData.GetOfficialSupplyDocketPressureClampCeilingOrDefault()),
            Math.Clamp(
                clerkDistortionPressure,
                _householdMobilityRulesData.GetOfficialSupplyClerkDistortionPressureClampFloorOrDefault(),
                _householdMobilityRulesData.GetOfficialSupplyClerkDistortionPressureClampCeilingOrDefault()),
            Math.Clamp(
                authorityBuffer,
                _householdMobilityRulesData.GetOfficialSupplyAuthorityBufferClampFloorOrDefault(),
                _householdMobilityRulesData.GetOfficialSupplyAuthorityBufferClampCeilingOrDefault()));
    }

    private OfficialSupplyBurdenProfile ComputeOfficialSupplyBurdenProfile(
        PopulationHouseholdState household,
        OfficialSupplySignal signal)
    {
        return new OfficialSupplyBurdenProfile(
            signal.SupplyPressure,
            signal.QuotaPressure,
            signal.DocketPressure,
            signal.ClerkDistortionPressure,
            signal.AuthorityBuffer,
            ComputeOfficialSupplyLivelihoodExposurePressure(household),
            ComputeOfficialSupplyResourceBuffer(household),
            ComputeOfficialSupplyLaborPressure(household),
            ComputeOfficialSupplyLiquidityPressure(household),
            ComputeOfficialSupplyFragilityPressure(household),
            ComputeOfficialSupplyInteractionPressure(household, signal),
            _householdMobilityRulesData.GetOfficialSupplyDistressDeltaClampFloorOrDefault(),
            _householdMobilityRulesData.GetOfficialSupplyDistressDeltaClampCeilingOrDefault(),
            _householdMobilityRulesData.GetOfficialSupplyDebtDeltaClampFloorOrDefault(),
            _householdMobilityRulesData.GetOfficialSupplyDebtDeltaClampCeilingOrDefault(),
            _householdMobilityRulesData.GetOfficialSupplyLaborDropClampFloorOrDefault(),
            _householdMobilityRulesData.GetOfficialSupplyLaborDropClampCeilingOrDefault(),
            _householdMobilityRulesData.GetOfficialSupplyMigrationDeltaClampFloorOrDefault(),
            _householdMobilityRulesData.GetOfficialSupplyMigrationDeltaClampCeilingOrDefault());
    }

    private static int ComputeOfficialSupplyLivelihoodExposurePressure(PopulationHouseholdState household)
    {
        int livelihoodExposure = household.Livelihood switch
        {
            LivelihoodType.Boatman => 5,
            LivelihoodType.HiredLabor => 4,
            LivelihoodType.SeasonalMigrant => 4,
            LivelihoodType.Smallholder => 3,
            LivelihoodType.Tenant => 3,
            LivelihoodType.Artisan => 2,
            LivelihoodType.PettyTrader => 2,
            LivelihoodType.YamenRunner => 2,
            LivelihoodType.Unknown => 2,
            LivelihoodType.DomesticServant => 1,
            LivelihoodType.Vagrant => 1,
            _ => 2,
        };

        int landVisibility = household.LandHolding switch
        {
            >= 70 => 2,
            >= 35 => 1,
            _ => 0,
        };

        return Math.Clamp(livelihoodExposure + landVisibility, 1, 7);
    }

    private static int ComputeOfficialSupplyResourceBuffer(PopulationHouseholdState household)
    {
        int grainBuffer = household.GrainStore switch
        {
            >= 85 => 5,
            >= 65 => 4,
            >= 45 => 2,
            >= 25 => 1,
            _ => 0,
        };

        int toolBuffer = household.ToolCondition >= 70 ? 1 : 0;
        int shelterBuffer = household.ShelterQuality >= 60 ? 1 : 0;
        return Math.Clamp(grainBuffer + toolBuffer + shelterBuffer, 0, 7);
    }

    private static int ComputeOfficialSupplyLaborPressure(PopulationHouseholdState household)
    {
        int laborPressure = household.LaborCapacity switch
        {
            >= 80 => -1,
            >= 60 => 0,
            >= 40 => 1,
            >= 25 => 3,
            _ => 4,
        };

        int dependentPressure = household.DependentCount switch
        {
            >= 5 => 2,
            >= 3 => 1,
            _ => 0,
        };

        if (household.LaborerCount > 0 && household.DependentCount > household.LaborerCount * 2)
        {
            dependentPressure += 1;
        }

        return Math.Clamp(laborPressure + dependentPressure, -1, 7);
    }

    private static int ComputeOfficialSupplyLiquidityPressure(PopulationHouseholdState household)
    {
        int grainStrain = household.GrainStore switch
        {
            >= 80 => -2,
            >= 55 => -1,
            >= 25 => 1,
            > 0 => 3,
            _ => 2,
        };

        int cashNeed = IsCashNeedLivelihood(household.Livelihood) ? 2 : 0;
        int toolDrag = household.ToolCondition is > 0 and < 35 ? 1 : 0;
        int debtDrag = household.DebtPressure >= 65 ? 2 : household.DebtPressure >= 50 ? 1 : 0;
        return Math.Clamp(grainStrain + cashNeed + toolDrag + debtDrag, -2, 7);
    }

    private static int ComputeOfficialSupplyFragilityPressure(PopulationHouseholdState household)
    {
        int distressPressure = household.Distress switch
        {
            >= 80 => 3,
            >= 65 => 2,
            >= 50 => 1,
            _ => 0,
        };

        int debtPressure = household.DebtPressure switch
        {
            >= 80 => 3,
            >= 65 => 2,
            >= 50 => 1,
            _ => 0,
        };

        int migrationPressure = household.IsMigrating || household.MigrationRisk >= 70 ? 1 : 0;
        int shelterDrag = household.ShelterQuality is > 0 and < 35 ? 1 : 0;
        return Math.Clamp(distressPressure + debtPressure + migrationPressure + shelterDrag, 0, 8);
    }

    private static int ComputeOfficialSupplyInteractionPressure(
        PopulationHouseholdState household,
        OfficialSupplySignal signal)
    {
        int interaction = 0;

        if (household.Livelihood == LivelihoodType.Boatman && signal.SupplyPressure >= 12)
        {
            interaction += 2;
        }

        if (household.Livelihood is LivelihoodType.HiredLabor or LivelihoodType.SeasonalMigrant
            && household.LaborCapacity < 40)
        {
            interaction += 2;
        }

        if (household.Livelihood == LivelihoodType.Tenant && household.DebtPressure >= 60)
        {
            interaction += 1;
        }

        if (household.GrainStore >= 75
            && household.LaborCapacity >= 75
            && household.DebtPressure < 55
            && household.Distress < 55)
        {
            interaction -= 3;
        }

        return Math.Clamp(interaction, -3, 5);
    }

    private readonly record struct GrainPriceShockSignal(int CurrentPrice, int PriceDelta, int Supply, int Demand);

    private readonly record struct SubsistencePressureProfile(
        int PricePressure,
        int GrainBufferPressure,
        int MarketDependencyPressure,
        int LaborPressure,
        int FragilityPressure,
        int InteractionPressure,
        int DistressDeltaClampFloor,
        int DistressDeltaClampCeiling)
    {
        public int DistressDelta => Math.Clamp(
            PricePressure + GrainBufferPressure + MarketDependencyPressure + LaborPressure + FragilityPressure + InteractionPressure,
            DistressDeltaClampFloor,
            DistressDeltaClampCeiling);
    }

    private readonly record struct TaxSeasonBurdenProfile(
        int VisibilityPressure,
        int LiquidityPressure,
        int LaborPressure,
        int FragilityPressure,
        int InteractionPressure,
        int DebtDeltaClampFloor,
        int DebtDeltaClampCeiling)
    {
        public int DebtDelta => Math.Clamp(
            14 + VisibilityPressure + LiquidityPressure + LaborPressure + FragilityPressure + InteractionPressure,
            DebtDeltaClampFloor,
            DebtDeltaClampCeiling);
    }

    private readonly record struct OfficialSupplySignal(
        int FrontierPressure,
        int SupplyPressure,
        int QuotaPressure,
        int DocketPressure,
        int ClerkDistortionPressure,
        int AuthorityBuffer);

    private readonly record struct OfficialSupplyBurdenProfile(
        int SupplyPressure,
        int QuotaPressure,
        int DocketPressure,
        int ClerkDistortionPressure,
        int AuthorityBuffer,
        int LivelihoodExposurePressure,
        int ResourceBuffer,
        int LaborPressure,
        int LiquidityPressure,
        int FragilityPressure,
        int InteractionPressure,
        int DistressDeltaClampFloor,
        int DistressDeltaClampCeiling,
        int DebtDeltaClampFloor,
        int DebtDeltaClampCeiling,
        int LaborDropClampFloor,
        int LaborDropClampCeiling,
        int MigrationDeltaClampFloor,
        int MigrationDeltaClampCeiling)
    {
        public int DistressDelta => Math.Clamp(
            (SupplyPressure / 4)
            + LivelihoodExposurePressure
            + LaborPressure
            + FragilityPressure
            + (ClerkDistortionPressure / 3)
            + InteractionPressure
            - ResourceBuffer
            - (AuthorityBuffer / 3),
            DistressDeltaClampFloor,
            DistressDeltaClampCeiling);

        public int DebtDelta => Math.Clamp(
            (QuotaPressure / 4)
            + LiquidityPressure
            + (FragilityPressure / 2)
            + Math.Max(0, InteractionPressure)
            + (ClerkDistortionPressure / 4)
            - (ResourceBuffer / 2),
            DebtDeltaClampFloor,
            DebtDeltaClampCeiling);

        public int LaborDrop => Math.Clamp(
            (SupplyPressure / 8)
            + Math.Max(0, LaborPressure)
            + (DocketPressure / 6)
            - (ResourceBuffer / 4),
            LaborDropClampFloor,
            LaborDropClampCeiling);

        public int MigrationDelta => Math.Clamp(
            (DistressDelta / 5)
            + (DebtDelta / 6)
            + (FragilityPressure >= 5 ? 1 : 0),
            MigrationDeltaClampFloor,
            MigrationDeltaClampCeiling);
    }
}
