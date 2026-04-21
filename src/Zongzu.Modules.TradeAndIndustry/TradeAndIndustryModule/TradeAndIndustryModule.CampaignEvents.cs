using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed partial class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
{
    public override void HandleEvents(ModuleEventHandlingScope<TradeAndIndustryState> scope)

    {

        DispatchWorldPulseEvents(scope);

        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);

        if (warfareEvents.Count == 0)

        {

            return;

        }


        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()

            .GetCampaigns()

            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);


        foreach (WarfareCampaignEventBundle bundle in warfareEvents)

        {

            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))

            {

                continue;

            }


            int routeRiskDelta = ComputeCampaignTradeRiskDelta(bundle, campaign);

            int marketRiskDelta = Math.Max(1, routeRiskDelta - 1);

            int cashLoss = ComputeCampaignCashLoss(bundle, campaign);

            int grainLoss = ComputeCampaignGrainLoss(bundle, campaign);

            int debtIncrease = ComputeCampaignDebtIncrease(bundle, campaign);

            int reputationLoss = bundle.CampaignSupplyStrained ? 2 : 1;

            SettlementBlackRouteLedgerState ledger = GetOrCreateBlackRouteLedger(scope.State, bundle.SettlementId);


            SettlementMarketState? market = scope.State.Markets.SingleOrDefault(existing => existing.SettlementId == bundle.SettlementId);

            if (market is not null)

            {

                market.LocalRisk = Math.Clamp(market.LocalRisk + marketRiskDelta, 0, 100);

                market.Demand = Math.Clamp(market.Demand - (bundle.CampaignSupplyStrained ? 2 : 1), 0, 100);

                market.PriceIndex = Math.Clamp(market.PriceIndex + Math.Max(1, marketRiskDelta / 2), 50, 150);

            }


            ledger.ShadowPriceIndex = Math.Clamp(Math.Max(ledger.ShadowPriceIndex, 100) + Math.Max(1, routeRiskDelta / 2), 70, 180);

            ledger.DiversionShare = Math.Clamp(ledger.DiversionShare + (bundle.CampaignSupplyStrained ? 3 : 1) + (bundle.CampaignAftermathRegistered ? 1 : 0), 0, 100);

            ledger.IllicitMargin = Math.Clamp(ledger.IllicitMargin + Math.Max(1, routeRiskDelta / 3), -10, 30);

            ledger.BlockedShipmentCount = Math.Clamp(ledger.BlockedShipmentCount + (bundle.CampaignSupplyStrained ? 2 : 1), 0, 12);

            ledger.SeizureRisk = Math.Clamp(ledger.SeizureRisk + (bundle.CampaignPressureRaised ? 3 : 1), 0, 100);

            ledger.DiversionBandLabel = DetermineDiversionBandLabel(ledger);

            ledger.LastLedgerTrace =

                $"{campaign.AnchorSettlementName}战事所及，{campaign.SupplyStateLabel}与{campaign.LastAftermathSummary}把私货逼向暗中转运，阻货{ledger.BlockedShipmentCount}，查缉险{ledger.SeizureRisk}。";


            RouteTradeState[] routes = scope.State.Routes

                .Where(route => route.SettlementId == bundle.SettlementId && route.IsActive)

                .OrderBy(static route => route.RouteId)

                .ToArray();

            HashSet<ClanId> affectedClans = routes

                .Select(static route => route.ClanId)

                .ToHashSet();


            foreach (RouteTradeState route in routes)

            {

                route.Risk = Math.Clamp(route.Risk + routeRiskDelta, 0, 100);

                route.LastMargin = Math.Clamp(route.LastMargin - routeRiskDelta, -20, 30);

                route.BlockedShipmentCount = Math.Clamp(route.BlockedShipmentCount + (bundle.CampaignSupplyStrained ? 1 : 0) + (bundle.CampaignAftermathRegistered ? 1 : 0), 0, 6);

                route.SeizureRisk = Math.Clamp(route.SeizureRisk + (bundle.CampaignPressureRaised ? 4 : 2), 0, 100);

                route.RouteConstraintLabel = DetermineRouteConstraintLabel(route);

                route.LastRouteTrace =

                    $"{route.RouteName}受{campaign.AnchorSettlementName}战事牵累，阻货{route.BlockedShipmentCount}，查缉险{route.SeizureRisk}。";

            }


            ClanTradeState[] clanTrades = scope.State.Clans

                .Where(trade => trade.PrimarySettlementId == bundle.SettlementId || affectedClans.Contains(trade.ClanId))

                .OrderBy(static trade => trade.ClanId.Value)

                .ToArray();


            foreach (ClanTradeState clanTrade in clanTrades)

            {

                clanTrade.CashReserve = Math.Clamp(clanTrade.CashReserve - cashLoss, 0, 500);

                clanTrade.GrainReserve = Math.Clamp(clanTrade.GrainReserve - grainLoss, 0, 500);

                clanTrade.Debt = Math.Clamp(clanTrade.Debt + debtIncrease, 0, 300);

                clanTrade.CommerceReputation = Math.Clamp(clanTrade.CommerceReputation - reputationLoss, 0, 100);

                clanTrade.LastOutcome = string.Equals(clanTrade.LastOutcome, "Loss", StringComparison.Ordinal)

                    ? "Loss"

                    : "War Strain";

                clanTrade.LastExplanation =

                    $"{clanTrade.LastExplanation}{campaign.AnchorSettlementName}战事所及，{campaign.FrontLabel}、{campaign.SupplyStateLabel}与{campaign.LastAftermathSummary}一并压商。";

            }


            if (market is null && routes.Length == 0 && clanTrades.Length == 0)

            {

                continue;

            }


            scope.RecordDiff(

                $"{campaign.AnchorSettlementName}战后余波所及，市险增{marketRiskDelta}，路险增{routeRiskDelta}，商债增{debtIncrease}，私下分流{ledger.DiversionShare}；{campaign.SupplyStateLabel}，{campaign.LastAftermathSummary}",

                bundle.SettlementId.Value.ToString());


            if (routes.Any(static route => route.Risk >= 60) || market?.LocalRisk >= 60)

            {

                scope.Emit("RouteBusinessBlocked", $"{campaign.AnchorSettlementName}战事压得商路难行。", bundle.SettlementId.Value.ToString());

            }


            if (clanTrades.Any(static trade => trade.Debt >= 120 && trade.CashReserve <= 40))

            {

                ClanTradeState defaultingTrade = clanTrades

                    .OrderByDescending(static trade => trade.Debt)

                    .ThenBy(static trade => trade.CashReserve)

                    .First();

                scope.Emit("TradeDebtDefaulted", $"{campaign.AnchorSettlementName}战事所逼，宗房{defaultingTrade.ClanId.Value}商债愈急。", bundle.SettlementId.Value.ToString());

            }

        }

    }


    private static string RenderTradeOutcome(string lastOutcome)

    {

        return lastOutcome switch

        {

            "Profit" => "得利",

            "Loss" => "受亏",

            "War Strain" => "兵事压商",

            "Stable" => "持平",

            _ => lastOutcome,

        };

    }


    private static void DispatchWorldPulseEvents(ModuleEventHandlingScope<TradeAndIndustryState> scope)
    {
        // Step 1b gap 3 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：洪灾 / 路阻 / 徭役窗口落在哪条线 / 哪个聚落；季节带；市面 buzz 与 road-report 延迟；
        // 宗房商路暴露度（ClanTradeSnapshot）；粮价基线。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case WorldSettlementsEventNames.FloodRiskThresholdBreached:
                case WorldSettlementsEventNames.RouteConstraintEmerged:
                case WorldSettlementsEventNames.CorveeWindowChanged:
                case PublicLifeAndRumorEventNames.MarketBuzzRaised:
                case PublicLifeAndRumorEventNames.RoadReportDelayed:
                    // TODO Step 2: 按维度入口调整路况 / 粮价 / 商路风险 / 发 RouteBusinessBlocked。
                    break;
            }
        }
    }
}
