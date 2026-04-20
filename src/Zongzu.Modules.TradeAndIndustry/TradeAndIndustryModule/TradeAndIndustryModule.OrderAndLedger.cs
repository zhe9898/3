using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed partial class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
{
    private static int ComputeOrderPenalty(SettlementDisorderSnapshot disorder)

    {

        int penalty = 0;

        if (disorder.RoutePressure >= 60)

        {

            penalty += 2;

        }

        else if (disorder.RoutePressure >= 35)

        {

            penalty += 1;

        }


        if (disorder.BanditThreat >= 65)

        {

            penalty += 1;

        }


        return penalty;

    }


    private static OrderInterventionCarryoverEffect ResolveOrderCarryover(SettlementDisorderSnapshot disorder)

    {

        if (disorder.InterventionCarryoverMonths <= 0 || string.IsNullOrWhiteSpace(disorder.LastInterventionCommandCode))

        {

            return default;

        }


        return disorder.LastInterventionCommandCode switch

        {

            PlayerCommandNames.EscortRoadReport => new(-1, -1, 0, 1),

            PlayerCommandNames.FundLocalWatch => new(-2, -2, 0, 1),

            PlayerCommandNames.SuppressBanditry => new(0, 2, 1, 2),

            PlayerCommandNames.NegotiateWithOutlaws => new(-1, 2, -1, -2),

            PlayerCommandNames.TolerateDisorder => new(2, 2, -1, -2),

            _ => default,

        };

    }


    private static int ComputeCampaignTradeRiskDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int delta = bundle.CampaignMobilized ? 1 : 0;

        delta += bundle.CampaignPressureRaised ? 2 : 0;

        delta += bundle.CampaignSupplyStrained ? 4 : 0;

        delta += bundle.CampaignAftermathRegistered ? 2 : 0;

        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;

        delta += campaign.SupplyState <= 40 ? 1 : 0;

        return Math.Max(1, delta);

    }


    private static int ComputeCampaignCashLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int loss = bundle.CampaignMobilized ? 1 : 0;

        loss += bundle.CampaignPressureRaised ? 2 : 0;

        loss += bundle.CampaignSupplyStrained ? 5 : 0;

        loss += bundle.CampaignAftermathRegistered ? 2 : 0;

        loss += Math.Max(0, campaign.MobilizedForceCount - 24) / 20;

        return Math.Max(1, loss);

    }


    private static int ComputeCampaignGrainLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int loss = bundle.CampaignMobilized ? 2 : 0;

        loss += bundle.CampaignPressureRaised ? 2 : 0;

        loss += bundle.CampaignSupplyStrained ? 6 : 0;

        loss += bundle.CampaignAftermathRegistered ? 2 : 0;

        loss += Math.Max(0, 55 - campaign.SupplyState) / 10;

        return Math.Max(1, loss);

    }


    private static int ComputeCampaignDebtIncrease(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int increase = bundle.CampaignPressureRaised ? 1 : 0;

        increase += bundle.CampaignSupplyStrained ? 3 : 0;

        increase += bundle.CampaignAftermathRegistered ? 1 : 0;

        increase += Math.Max(0, campaign.FrontPressure - 60) / 20;

        return Math.Max(1, increase);

    }


    private static SettlementBlackRouteLedgerState GetOrCreateBlackRouteLedger(TradeAndIndustryState state, SettlementId settlementId)

    {

        SettlementBlackRouteLedgerState? ledger = state.BlackRouteLedgers.SingleOrDefault(existing => existing.SettlementId == settlementId);

        if (ledger is not null)

        {

            return ledger;

        }


        ledger = new SettlementBlackRouteLedgerState

        {

            SettlementId = settlementId,

            ShadowPriceIndex = 100,

            DiversionBandLabel = "尚未分流",

        };

        state.BlackRouteLedgers.Add(ledger);

        state.BlackRouteLedgers = state.BlackRouteLedgers.OrderBy(static entry => entry.SettlementId.Value).ToList();

        return ledger;

    }


    private static Dictionary<SettlementId, TradeActivitySnapshot> BuildTradeActivityBySettlement(TradeAndIndustryState state)

    {

        Dictionary<SettlementId, List<RouteTradeState>> routesBySettlement = new();

        foreach (RouteTradeState route in state.Routes

                     .Where(static route => route.IsActive)

                     .OrderBy(static route => route.RouteId))

        {

            if (!routesBySettlement.TryGetValue(route.SettlementId, out List<RouteTradeState>? routes))

            {

                routes = [];

                routesBySettlement[route.SettlementId] = routes;

            }


            routes.Add(route);

        }


        return routesBySettlement.ToDictionary(

            static pair => pair.Key,

            static pair =>

            {

                int averageRouteRisk = pair.Value.Count == 0 ? 0 : pair.Value.Sum(static route => route.Risk) / pair.Value.Count;

                int totalRouteCapacity = pair.Value.Sum(static route => route.Capacity);

                return new TradeActivitySnapshot(pair.Value.Count, averageRouteRisk, totalRouteCapacity);

            });

    }


    private static string DetermineDiversionBandLabel(SettlementBlackRouteLedgerState ledger)

    {

        int combined = ledger.DiversionShare + ledger.SeizureRisk + Math.Max(0, ledger.IllicitMargin);

        return combined switch

        {

            >= 120 => "私货成路",

            >= 85 => "正私并行",

            >= 55 => "夹带渐增",

            >= 25 => "零星夹带",

            _ => "尚未分流",

        };

    }


    private static int ComputeRouteBlockedShipmentCount(

        RouteTradeState route,

        SettlementBlackRouteLedgerState ledger,

        SettlementBlackRoutePressureSnapshot pressure)

    {

        return Math.Clamp(

            (ledger.BlockedShipmentCount > 0 ? 1 : 0)

            + (ledger.BlockedShipmentCount >= 3 ? 1 : 0)

            + (pressure.BlackRoutePressure >= 55 ? 1 : 0)

            + (pressure.PaperCompliance >= 45 ? 1 : 0)

            + (pressure.ResponseActivationLevel >= 4 ? 1 : 0)

            + (pressure.RouteShielding >= 45 ? 1 : 0)

            + (pressure.RetaliationRisk >= 70 ? 1 : 0)

            + (pressure.AdministrativeSuppressionWindow >= 2 ? 1 : 0)

            + (route.Risk >= 70 ? 1 : 0),

            0,

            6);

    }


    private static int ComputeRouteSeizureRisk(

        RouteTradeState route,

        SettlementBlackRouteLedgerState ledger,

        SettlementBlackRoutePressureSnapshot pressure)

    {

        return Math.Clamp(

            (ledger.SeizureRisk / 2)

            + (pressure.PaperCompliance / 4)

            + (pressure.AdministrativeSuppressionWindow * 5)

            + (pressure.ResponseActivationLevel * 3)

            + (pressure.RouteShielding / 5)

            + (route.Risk / 5)

            - (pressure.ImplementationDrag / 6)

            - (pressure.CoercionRisk / 6),

            0,

            100);

    }


    private static string DetermineRouteConstraintLabel(RouteTradeState route)

    {

        int combined = (route.BlockedShipmentCount * 12) + route.SeizureRisk + (route.Risk / 2);

        return combined switch

        {

            >= 120 => "盘查封路",

            >= 85 => "卡口渐密",

            >= 50 => "时有阻滞",

            >= 20 => "尚可通行",

            _ => "行路平稳",

        };

    }


    private static string BuildRouteConstraintTrace(

        RouteTradeState route,

        SettlementBlackRoutePressureSnapshot pressure,

        SettlementBlackRouteLedgerState ledger)

    {

        List<string> reasons = [];


        if (route.BlockedShipmentCount > 0)

        {

            reasons.Add($"{route.RouteName}眼下阻货{route.BlockedShipmentCount}，埠口与关卡都更难过。");

        }


        if (route.SeizureRisk > 0)

        {

            reasons.Add($"查缉险{route.SeizureRisk}，盘验多落在{route.RouteName}。");

        }


        if (pressure.BlackRoutePressure > 0 || pressure.AdministrativeSuppressionWindow > 0 || ledger.DiversionShare > 0)

        {

            reasons.Add($"私路压{pressure.BlackRoutePressure}、官面查缉窗{pressure.AdministrativeSuppressionWindow}、分流{ledger.DiversionShare}，把正私两路都牵紧了。");

        }


        if (reasons.Count == 0)

        {

            reasons.Add($"{route.RouteName}眼下尚可通行，未见明显阻滞。");

        }


        return string.Join(" ", reasons.Take(3));

    }


    private static string BuildBlackRouteLedgerTrace(

        SettlementMarketState market,

        TradeActivitySnapshot tradeActivity,

        SettlementBlackRoutePressureSnapshot pressure,

        SettlementBlackRouteLedgerState ledger)

    {

        List<string> reasons = [];


        if (tradeActivity.ActiveRouteCount > 0)

        {

            reasons.Add($"本地仍有{tradeActivity.ActiveRouteCount}条活路，正货与私货同挤在{market.MarketName}。");

        }


        if (pressure.BlackRoutePressure > 0 || pressure.CoercionRisk > 0)

        {

            reasons.Add($"上月私路压{pressure.BlackRoutePressure}、胁迫险{pressure.CoercionRisk}，逼得分流占比升到{ledger.DiversionShare}。");

        }


        if (ledger.BlockedShipmentCount > 0 || ledger.SeizureRisk > 0)

        {

            reasons.Add($"眼下阻货{ledger.BlockedShipmentCount}，查缉险{ledger.SeizureRisk}，私货只敢挑缝走。");

        }


        if (reasons.Count == 0)

        {

            reasons.Add($"{market.MarketName}私下分流尚浅，仍未压过正市主路。");

        }


        return string.Join(" ", reasons.Take(3));

    }


}
