using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed partial class OrderAndBanditryModule : ModuleRunner<OrderAndBanditryState>
{
    private static void ApplyXunOpeningPulse(

        SettlementDisorderState disorder,

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        int localFear,

        int localGrudge,

        int implementationDrag,

        int routeShielding,

        int forceSuppression)

    {

        int banditDelta = 0;

        banditDelta += settlement.Security < 45 ? 1 : settlement.Security >= 65 ? -1 : 0;

        banditDelta += population.CommonerDistress >= 60 ? 1 : population.CommonerDistress < 35 ? -1 : 0;

        banditDelta += localFear >= 55 ? 1 : 0;

        banditDelta += implementationDrag >= 55 ? 1 : 0;

        banditDelta -= routeShielding >= 60 ? 1 : 0;

        banditDelta -= forceSuppression >= 6 ? 1 : 0;


        int disorderDelta = 0;

        disorderDelta += population.MigrationPressure >= 50 ? 1 : 0;

        disorderDelta += localGrudge >= 55 ? 1 : localGrudge < 30 ? -1 : 0;

        disorderDelta += localFear >= 60 ? 1 : 0;

        disorderDelta -= routeShielding >= 45 ? 1 : 0;

        disorderDelta -= forceSuppression >= 6 ? 1 : 0;


        int coercionDelta = 0;

        coercionDelta += localFear >= 55 ? 1 : 0;

        coercionDelta += localGrudge >= 60 ? 1 : 0;

        coercionDelta += implementationDrag >= 60 ? 1 : 0;

        coercionDelta -= routeShielding >= 45 ? 1 : 0;


        disorder.BanditThreat = Math.Clamp(disorder.BanditThreat + banditDelta, 0, 100);

        disorder.DisorderPressure = Math.Clamp(disorder.DisorderPressure + disorderDelta, 0, 100);

        disorder.CoercionRisk = Math.Clamp(disorder.CoercionRisk + coercionDelta, 0, 100);

    }


    private static void ApplyXunRoadPulse(

        SettlementDisorderState disorder,

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        TradeActivitySnapshot tradeActivity,

        SettlementBlackRouteLedgerSnapshot blackRouteLedger,

        int routeShielding,

        int paperCompliance,

        int implementationDrag,

        int administrativeSuppressionWindow)

    {

        int routeDelta = 0;

        routeDelta += disorder.BanditThreat >= 50 ? 1 : disorder.BanditThreat < 25 ? -1 : 0;

        routeDelta += tradeActivity.ActiveRouteCount > 0 ? 1 : 0;

        routeDelta += tradeActivity.AverageRouteRisk >= 55 ? 1 : 0;

        routeDelta += settlement.Security < 50 ? 1 : 0;

        routeDelta += paperCompliance >= 55 ? 1 : 0;

        routeDelta += implementationDrag >= 45 ? 1 : 0;

        routeDelta -= routeShielding >= 60 ? 1 : 0;

        routeDelta -= administrativeSuppressionWindow >= 4 ? 1 : 0;


        int blackRouteDelta = 0;

        blackRouteDelta += disorder.RoutePressure >= 55 ? 1 : 0;

        blackRouteDelta += tradeActivity.ActiveRouteCount > 0 ? 1 : 0;

        blackRouteDelta += population.CommonerDistress >= 58 ? 1 : 0;

        blackRouteDelta += blackRouteLedger.DiversionShare >= 25 ? 1 : 0;

        blackRouteDelta += blackRouteLedger.ShadowPriceIndex >= 112 ? 1 : blackRouteLedger.ShadowPriceIndex <= 94 ? -1 : 0;

        blackRouteDelta += implementationDrag >= 55 ? 1 : 0;

        blackRouteDelta -= routeShielding >= 60 ? 1 : 0;

        blackRouteDelta -= administrativeSuppressionWindow >= 4 ? 1 : 0;


        disorder.RoutePressure = Math.Clamp(disorder.RoutePressure + routeDelta, 0, 100);

        disorder.BlackRoutePressure = Math.Clamp(disorder.BlackRoutePressure + blackRouteDelta, 0, 100);

    }


    private static void ApplyXunClosingPulse(

        SettlementDisorderState disorder,

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        TradeActivitySnapshot tradeActivity,

        SettlementBlackRouteLedgerSnapshot blackRouteLedger,

        int localFear,

        int localGrudge,

        int routeShielding,

        int administrativeRelief,

        int activeEscortCount,

        int forceSuppression)

    {

        int disorderDelta = 0;

        disorderDelta += disorder.BanditThreat >= 65 ? 1 : 0;

        disorderDelta += localGrudge >= 60 ? 1 : 0;

        disorderDelta += blackRouteLedger.BlockedShipmentCount >= 2 ? 1 : 0;

        disorderDelta -= administrativeRelief >= 2 ? 1 : 0;

        disorderDelta -= forceSuppression >= 6 ? 1 : 0;


        int routeDelta = 0;

        routeDelta += disorder.BlackRoutePressure >= 60 ? 1 : 0;

        routeDelta += tradeActivity.AverageRouteRisk >= 60 ? 1 : 0;

        routeDelta += settlement.Security < 45 ? 1 : 0;

        routeDelta -= routeShielding >= 55 ? 1 : 0;

        routeDelta -= activeEscortCount >= 8 ? 1 : 0;


        int coercionDelta = 0;

        coercionDelta += disorder.BlackRoutePressure >= 55 ? 1 : 0;

        coercionDelta += localFear >= 55 ? 1 : 0;

        coercionDelta += population.MigrationPressure >= 55 ? 1 : 0;

        coercionDelta -= routeShielding >= 45 ? 1 : 0;

        coercionDelta -= administrativeRelief >= 2 ? 1 : 0;


        disorder.DisorderPressure = Math.Clamp(disorder.DisorderPressure + disorderDelta, 0, 100);

        disorder.RoutePressure = Math.Clamp(disorder.RoutePressure + routeDelta, 0, 100);

        disorder.CoercionRisk = Math.Clamp(disorder.CoercionRisk + coercionDelta, 0, 100);

    }


    private static InterventionCarryoverEffect ResolveInterventionCarryover(string commandCode, int carryoverMonths)

    {

        if (carryoverMonths <= 0 || string.IsNullOrWhiteSpace(commandCode))

        {

            return default;

        }


        return commandCode switch

        {

            PlayerCommandNames.EscortRoadReport => new(-1, -2, -1, 10, -3, 1, -2, -1, -1),

            PlayerCommandNames.FundLocalWatch => new(-1, -3, -2, 14, -3, 2, -2, -2, -1),

            PlayerCommandNames.SuppressBanditry => new(-2, -1, 0, 4, -1, 1, 14, 4, 3),

            PlayerCommandNames.NegotiateWithOutlaws => new(-1, -1, 0, 0, -2, 0, -10, 3, -1),

            PlayerCommandNames.TolerateDisorder => new(1, 2, 1, -4, -2, -1, -6, 4, 2),

            _ => default,

        };

    }


}
