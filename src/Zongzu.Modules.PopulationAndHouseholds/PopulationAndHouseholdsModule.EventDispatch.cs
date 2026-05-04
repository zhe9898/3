using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private void DispatchTradeShockEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Step 1b gap 1 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口（Step 2 可吃）：违约方所属聚落的 debt/distress/livelihood；家户 sponsor clan 救济余力；
        // 当地粮价波动；徭役窗口；灾害窗口；季节带。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case TradeAndIndustryEventNames.RouteBusinessBlocked:
                case TradeAndIndustryEventNames.TradeLossOccurred:
                case TradeAndIndustryEventNames.TradeDebtDefaulted:
                case TradeAndIndustryEventNames.TradeProspered:
                    // TODO Step 2: 按维度入口调整相关 household 的 DebtLoad / LivelihoodPressure / MigrationRisk。
                    break;

                case TradeAndIndustryEventNames.GrainPriceSpike:
                    ApplyGrainPriceSubsistencePressure(scope, domainEvent);
                    break;
            }
        }
    }

    private void ApplyGrainPriceSubsistencePressure(
        ModuleEventHandlingScope<PopulationAndHouseholdsState> scope,
        IDomainEvent domainEvent)
    {
        SettlementId? affectedSettlementId = ResolveSettlementScope(domainEvent);
        GrainPriceShockSignal signal = ResolveGrainPriceShockSignal(domainEvent);
        bool anyHouseholdChanged = false;

        foreach (PopulationHouseholdState household in scope.State.Households.OrderBy(static household => household.Id.Value))
        {
            if (affectedSettlementId.HasValue && household.SettlementId != affectedSettlementId.Value)
            {
                continue;
            }

            int oldDistress = household.Distress;
            SubsistencePressureProfile subsistenceProfile = ComputeSubsistencePressureProfile(household, signal);
            int distressDelta = subsistenceProfile.DistressDelta;
            int eventDistressThreshold = _householdMobilityRulesData.GetSubsistencePressureEventDistressThresholdOrDefault();
            household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);
            anyHouseholdChanged = true;

            if (oldDistress < eventDistressThreshold && household.Distress >= eventDistressThreshold)
            {
                scope.Emit(
                    PopulationEventNames.HouseholdSubsistencePressureChanged,
                    $"{household.HouseholdName}粮价陡起，生计吃紧。",
                    household.Id.Value.ToString(),
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseGrainPriceSpike,
                        [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                        [DomainEventMetadataKeys.SettlementId] = household.SettlementId.Value.ToString(),
                        [DomainEventMetadataKeys.DistressBefore] = oldDistress.ToString(),
                        [DomainEventMetadataKeys.DistressAfter] = household.Distress.ToString(),
                        [DomainEventMetadataKeys.SubsistenceDistressDelta] = distressDelta.ToString(),
                        [DomainEventMetadataKeys.SubsistencePricePressure] = subsistenceProfile.PricePressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceGrainBufferPressure] = subsistenceProfile.GrainBufferPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceMarketDependencyPressure] = subsistenceProfile.MarketDependencyPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceLaborPressure] = subsistenceProfile.LaborPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceFragilityPressure] = subsistenceProfile.FragilityPressure.ToString(),
                        [DomainEventMetadataKeys.SubsistenceInteractionPressure] = subsistenceProfile.InteractionPressure.ToString(),
                        [DomainEventMetadataKeys.GrainCurrentPrice] = signal.CurrentPrice.ToString(),
                        [DomainEventMetadataKeys.GrainPriceDelta] = signal.PriceDelta.ToString(),
                        [DomainEventMetadataKeys.GrainSupply] = signal.Supply.ToString(),
                        [DomainEventMetadataKeys.GrainDemand] = signal.Demand.ToString(),
                        [DomainEventMetadataKeys.Livelihood] = household.Livelihood.ToString(),
                    });
            }
        }

        if (anyHouseholdChanged)
        {
            SynchronizeMembershipLivelihoodsAndActivities(scope.State);
            RebuildSettlementSummaries(scope.State, scope.TryGetQuery<IPersonRegistryQueries>());
        }
    }

    private void DispatchWorldPulseEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Step 1b gap 3 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：洪灾 / 徭役窗口落在哪个聚落；家户 sponsor clan 支撑力；家底 ratio；季节带。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case WorldSettlementsEventNames.FloodRiskThresholdBreached:
                case WorldSettlementsEventNames.CorveeWindowChanged:
                    // TODO Step 2: 按维度入口调整 LaborPressure / LivelihoodPressure / MigrationRisk。
                    break;

                case WorldSettlementsEventNames.TaxSeasonOpened:
                    ApplyTaxSeasonPressure(scope, domainEvent);
                    break;
            }
        }
    }

    private void ApplyTaxSeasonPressure(
        ModuleEventHandlingScope<PopulationAndHouseholdsState> scope,
        IDomainEvent domainEvent)
    {
        SettlementId? scopedSettlementId = ResolveSettlementScope(domainEvent);

        foreach (PopulationHouseholdState household in scope.State.Households.OrderBy(static household => household.Id.Value))
        {
            if (scopedSettlementId is not null && household.SettlementId != scopedSettlementId.Value)
            {
                continue;
            }

            int oldDebt = household.DebtPressure;
            TaxSeasonBurdenProfile taxProfile = ComputeTaxSeasonBurdenProfile(household);
            int taxDebtDelta = taxProfile.DebtDelta;
            household.DebtPressure = Math.Clamp(household.DebtPressure + taxDebtDelta, 0, 100);

            if (oldDebt < 70 && household.DebtPressure >= 70)
            {
                scope.Emit(
                    PopulationEventNames.HouseholdDebtSpiked,
                    $"{household.HouseholdName}税役加急，债压陡起。",
                    household.Id.Value.ToString(),
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseTaxSeason,
                        [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                        [DomainEventMetadataKeys.SettlementId] = household.SettlementId.Value.ToString(),
                        [DomainEventMetadataKeys.DebtBefore] = oldDebt.ToString(),
                        [DomainEventMetadataKeys.DebtAfter] = household.DebtPressure.ToString(),
                        [DomainEventMetadataKeys.TaxDebtDelta] = taxDebtDelta.ToString(),
                        [DomainEventMetadataKeys.TaxVisibilityPressure] = taxProfile.VisibilityPressure.ToString(),
                        [DomainEventMetadataKeys.TaxLiquidityPressure] = taxProfile.LiquidityPressure.ToString(),
                        [DomainEventMetadataKeys.TaxLaborPressure] = taxProfile.LaborPressure.ToString(),
                        [DomainEventMetadataKeys.TaxFragilityPressure] = taxProfile.FragilityPressure.ToString(),
                        [DomainEventMetadataKeys.TaxInteractionPressure] = taxProfile.InteractionPressure.ToString(),
                        [DomainEventMetadataKeys.Livelihood] = household.Livelihood.ToString(),
                    });
            }
        }
    }

    private static void DispatchFamilyBranchEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Step 1b gap 4 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：分房 clan 家底 / 聚落容纳；旧家 sponsor 关系；是否可迁移；新分出支系与原聚落距离；
        // 四季带 / 战后恢复期。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case FamilyCoreEventNames.BranchSeparationApproved:
                case FamilyCoreEventNames.HeirSecurityWeakened:
                    // TODO Step 2: 按维度入口调整 household sponsor / 新增户 / MigrationRisk。
                    break;
            }
        }
    }

    private static void DispatchOfficeSupplyEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        // Chain 5: official supply requisition becomes household-owned burden.
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != OfficeAndCareerEventNames.OfficialSupplyRequisition)
            {
                continue;
            }

            SettlementId? scopedSettlementId = ResolveSettlementScope(domainEvent);
            if (scopedSettlementId is null)
            {
                continue;
            }

            SettlementId settlementId = scopedSettlementId.Value;
            OfficialSupplySignal signal = ResolveOfficialSupplySignal(domainEvent);
            bool anyHouseholdChanged = false;

            foreach (PopulationHouseholdState household in scope.State.Households
                .Where(h => h.SettlementId == settlementId)
                .OrderBy(static h => h.Id.Value))
            {
                int oldDistress = household.Distress;
                int oldDebt = household.DebtPressure;
                int oldLabor = household.LaborCapacity;
                int oldMigration = household.MigrationRisk;
                OfficialSupplyBurdenProfile burdenProfile = ComputeOfficialSupplyBurdenProfile(household, signal);

                household.Distress = Math.Clamp(household.Distress + burdenProfile.DistressDelta, 0, 100);
                household.DebtPressure = Math.Clamp(household.DebtPressure + burdenProfile.DebtDelta, 0, 100);
                household.LaborCapacity = Math.Clamp(household.LaborCapacity - burdenProfile.LaborDrop, 0, 100);
                household.MigrationRisk = Math.Clamp(household.MigrationRisk + burdenProfile.MigrationDelta, 0, 100);
                household.IsMigrating = ResolveMigrationStatus(household);
                anyHouseholdChanged = anyHouseholdChanged
                    || oldDistress != household.Distress
                    || oldDebt != household.DebtPressure
                    || oldLabor != household.LaborCapacity
                    || oldMigration != household.MigrationRisk;

                if (oldDistress < 80 && household.Distress >= 80)
                {
                    scope.Emit(
                        PopulationEventNames.HouseholdBurdenIncreased,
                        $"{household.HouseholdName}军需催迫，家户负担加重。",
                        household.Id.Value.ToString(),
                        new Dictionary<string, string>
                        {
                            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseOfficialSupply,
                            [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                            [DomainEventMetadataKeys.SettlementId] = settlementId.Value.ToString(),
                            [DomainEventMetadataKeys.DistressBefore] = oldDistress.ToString(),
                            [DomainEventMetadataKeys.DistressAfter] = household.Distress.ToString(),
                            [DomainEventMetadataKeys.DebtBefore] = oldDebt.ToString(),
                            [DomainEventMetadataKeys.DebtAfter] = household.DebtPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyPressure] = signal.SupplyPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = signal.QuotaPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = signal.DocketPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = signal.ClerkDistortionPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = signal.AuthorityBuffer.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyDistressDelta] = burdenProfile.DistressDelta.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyDebtDelta] = burdenProfile.DebtDelta.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLaborDrop] = burdenProfile.LaborDrop.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyMigrationDelta] = burdenProfile.MigrationDelta.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure] = burdenProfile.LivelihoodExposurePressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyResourceBuffer] = burdenProfile.ResourceBuffer.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLaborPressure] = burdenProfile.LaborPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyLiquidityPressure] = burdenProfile.LiquidityPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyFragilityPressure] = burdenProfile.FragilityPressure.ToString(),
                            [DomainEventMetadataKeys.OfficialSupplyInteractionPressure] = burdenProfile.InteractionPressure.ToString(),
                            [DomainEventMetadataKeys.FrontierPressure] = signal.FrontierPressure.ToString(),
                            [DomainEventMetadataKeys.Livelihood] = household.Livelihood.ToString(),
                        });
                }
            }

            if (anyHouseholdChanged)
            {
                SynchronizeMembershipLivelihoodsAndActivities(scope.State);
                RebuildSettlementSummaries(scope.State, scope.TryGetQuery<IPersonRegistryQueries>());
            }
        }
    }

}
