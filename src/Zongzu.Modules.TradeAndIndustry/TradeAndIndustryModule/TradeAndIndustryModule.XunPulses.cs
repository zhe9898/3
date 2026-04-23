using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed partial class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
{
    private static void ApplyXunMarketPulse(

        SettlementMarketState market,

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        SettlementBlackRoutePressureSnapshot pressure)

    {

        int priceDelta =

            (settlement.Prosperity >= 60 ? 1 : settlement.Prosperity < 45 ? -1 : 0)

            + (population.CommonerDistress >= 55 ? 1 : 0)

            + (pressure.BlackRoutePressure >= 60 ? 1 : 0)

            - (pressure.SuppressionRelief >= 6 ? 1 : 0);

        int demandDelta =

            (population.LaborSupply >= 100 ? 1 : population.LaborSupply < 70 ? -1 : 0)

            + (settlement.Prosperity >= 58 ? 1 : 0)

            - (market.LocalRisk >= 60 ? 1 : 0);

        int riskDelta =

            (settlement.Security < 45 ? 1 : settlement.Security >= 60 ? -1 : 0)

            + (pressure.BlackRoutePressure >= 55 ? 1 : 0)

            + (pressure.RetaliationRisk >= 60 ? 1 : 0)

            - (pressure.RouteShielding >= 60 ? 1 : 0)

            - (pressure.SuppressionRelief >= 6 ? 1 : 0);


        market.PriceIndex = Math.Clamp(market.PriceIndex + priceDelta, 50, 150);

        market.Demand = Math.Clamp(market.Demand + demandDelta, 0, 100);

        market.LocalRisk = Math.Clamp(market.LocalRisk + riskDelta, 0, 100);

    }


    private static void ApplyXunLedgerPulse(

        SettlementMarketState market,

        TradeActivitySnapshot tradeActivity,

        SettlementBlackRoutePressureSnapshot pressure,

        SettlementBlackRouteLedgerState ledger)

    {

        int shadowDelta =

            (market.PriceIndex >= 110 ? 1 : market.PriceIndex <= 92 ? -1 : 0)

            + (pressure.BlackRoutePressure >= 55 ? 1 : 0)

            + (market.LocalRisk >= 55 ? 1 : 0);

        int diversionDelta =

            (pressure.BlackRoutePressure >= 55 ? 1 : 0)

            + (market.LocalRisk >= 55 ? 1 : 0)

            + (tradeActivity.ActiveRouteCount > 0 ? 1 : 0)

            - (pressure.RouteShielding >= 60 ? 1 : 0)

            - (pressure.AdministrativeSuppressionWindow >= 4 ? 1 : 0);

        int blockedDelta =

            (market.LocalRisk >= 60 ? 1 : 0)

            + (pressure.PaperCompliance >= 50 ? 1 : 0)

            - (pressure.ImplementationDrag >= 60 ? 1 : 0);

        int seizureDelta =

            (pressure.PaperCompliance >= 55 ? 2 : pressure.PaperCompliance >= 35 ? 1 : 0)

            + (pressure.AdministrativeSuppressionWindow >= 4 ? 1 : 0)

            + (market.LocalRisk >= 60 ? 1 : 0)

            - (pressure.CoercionRisk >= 60 ? 1 : 0);


        ledger.ShadowPriceIndex = Math.Clamp(Math.Max(ledger.ShadowPriceIndex, 100) + shadowDelta, 70, 180);

        ledger.DiversionShare = Math.Clamp(ledger.DiversionShare + diversionDelta, 0, 100);

        ledger.BlockedShipmentCount = Math.Clamp(ledger.BlockedShipmentCount + blockedDelta, 0, 12);

        ledger.SeizureRisk = Math.Clamp(ledger.SeizureRisk + seizureDelta, 0, 100);

        ledger.IllicitMargin = Math.Clamp(

            ((ledger.ShadowPriceIndex - 100) / 4)

            + (ledger.DiversionShare / 10)

            + (tradeActivity.TotalRouteCapacity / 30)

            - (ledger.SeizureRisk / 12)

            - ledger.BlockedShipmentCount,

            -10,

            30);

        ledger.DiversionBandLabel = DetermineDiversionBandLabel(ledger);

        ledger.LastLedgerTrace = BuildBlackRouteLedgerTrace(market, tradeActivity, pressure, ledger);

    }


    private static void ApplyXunLateMarketRiskPulse(

        SettlementMarketState market,

        SettlementDisorderSnapshot disorder,

        SettlementBlackRoutePressureSnapshot pressure)

    {

        int lateRiskDelta =

            (disorder.RoutePressure >= 60 ? 1 : 0)

            + (disorder.BanditThreat >= 65 ? 1 : 0)

            + (pressure.RetaliationRisk >= 65 ? 1 : 0)

            - (pressure.RouteShielding >= 65 ? 1 : 0);


        market.LocalRisk = Math.Clamp(market.LocalRisk + lateRiskDelta, 0, 100);

    }


    private static void ApplyXunRoutePulse(

        RouteTradeState route,

        SettlementMarketState market,

        ClanNarrativeSnapshot narrative,

        SettlementBlackRoutePressureSnapshot pressure,

        SettlementBlackRouteLedgerState ledger)

    {

        int routeRiskDelta =

            (market.LocalRisk >= 55 ? 1 : 0)

            + (narrative.GrudgePressure >= 60 ? 1 : 0)

            + (pressure.RetaliationRisk >= 65 ? 1 : 0)

            - (pressure.RouteShielding >= 60 ? 1 : 0)

            - (pressure.SuppressionRelief >= 6 ? 1 : 0);


        route.Risk = Math.Clamp(route.Risk + routeRiskDelta, 0, 100);

        route.BlockedShipmentCount = ComputeRouteBlockedShipmentCount(route, ledger, pressure);

        route.SeizureRisk = ComputeRouteSeizureRisk(route, ledger, pressure);

        route.RouteConstraintLabel = DetermineRouteConstraintLabel(route);

        route.LastRouteTrace = BuildRouteConstraintTrace(route, pressure, ledger);

    }


}
