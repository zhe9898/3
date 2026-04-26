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

                scope.Emit(TradeAndIndustryEventNames.RouteBusinessBlocked, $"{campaign.AnchorSettlementName}战事压得商路难行。", bundle.SettlementId.Value.ToString());

            }


            if (clanTrades.Any(static trade => trade.Debt >= 120 && trade.CashReserve <= 40))

            {

                ClanTradeState defaultingTrade = clanTrades

                    .OrderByDescending(static trade => trade.Debt)

                    .ThenBy(static trade => trade.CashReserve)

                    .First();

                scope.Emit(TradeAndIndustryEventNames.TradeDebtDefaulted, $"{campaign.AnchorSettlementName}战事所逼，宗房{defaultingTrade.ClanId.Value}商债愈急。", bundle.SettlementId.Value.ToString());

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

                case WorldSettlementsEventNames.CanalWindowChanged:
                    ApplyCanalWindowTradePressure(scope, domainEvent);
                    break;

                case WorldSettlementsEventNames.SeasonPhaseAdvanced:
                    ApplyHarvestPricePulse(scope, domainEvent);
                    break;
            }
        }
    }

    private static void ApplyCanalWindowTradePressure(
        ModuleEventHandlingScope<TradeAndIndustryState> scope,
        IDomainEvent domainEvent)
    {
        string canalWindow = ResolveCanalWindow(domainEvent);
        CanalTradePressureProfile profile = canalWindow switch
        {
            nameof(CanalWindow.Closed) => new(8, 6, -3, 2, 1, 4, 3, 2),
            nameof(CanalWindow.Limited) => new(4, 3, -1, 1, 1, 2, 1, 1),
            nameof(CanalWindow.Open) => new(-4, -3, 2, -1, -1, -2, -2, -1),
            _ => default,
        };

        if (profile == default)
        {
            return;
        }

        IWorldSettlementsQueries worldQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        HashSet<SettlementId> exposedSettlements = BuildCanalExposedSettlementIds(worldQueries);
        if (exposedSettlements.Count == 0)
        {
            return;
        }

        HashSet<SettlementId> changedSettlements = [];
        foreach (SettlementMarketState market in scope.State.Markets
            .Where(market => exposedSettlements.Contains(market.SettlementId))
            .OrderBy(static market => market.SettlementId.Value))
        {
            int oldLocalRisk = market.LocalRisk;
            int oldDemand = market.Demand;
            int oldPriceIndex = market.PriceIndex;

            market.LocalRisk = Math.Clamp(market.LocalRisk + profile.MarketRiskDelta, 0, 100);
            market.Demand = Math.Clamp(market.Demand + profile.DemandDelta, 0, 100);
            market.PriceIndex = Math.Clamp(market.PriceIndex + profile.PriceIndexDelta, 50, 150);

            SettlementBlackRouteLedgerState ledger = GetOrCreateBlackRouteLedger(scope.State, market.SettlementId);
            ledger.ShadowPriceIndex = Math.Clamp(Math.Max(ledger.ShadowPriceIndex, 100) + profile.ShadowPriceDelta, 70, 180);
            ledger.DiversionShare = Math.Clamp(ledger.DiversionShare + profile.DiversionDelta, 0, 100);
            ledger.BlockedShipmentCount = Math.Clamp(ledger.BlockedShipmentCount + profile.BlockedShipmentDelta, 0, 12);
            ledger.SeizureRisk = Math.Clamp(ledger.SeizureRisk + profile.SeizureRiskDelta, 0, 100);
            ledger.DiversionBandLabel = DetermineDiversionBandLabel(ledger);
            ledger.LastLedgerTrace =
                $"{market.MarketName}受漕渠窗口{canalWindow}牵动，正货周转与私下分流一并重排。";

            if (oldLocalRisk != market.LocalRisk
                || oldDemand != market.Demand
                || oldPriceIndex != market.PriceIndex)
            {
                changedSettlements.Add(market.SettlementId);
            }
        }

        foreach (RouteTradeState route in scope.State.Routes
            .Where(route => route.IsActive && exposedSettlements.Contains(route.SettlementId))
            .OrderBy(static route => route.RouteId))
        {
            int oldRisk = route.Risk;
            int oldBlocked = route.BlockedShipmentCount;
            int oldSeizure = route.SeizureRisk;

            route.Risk = Math.Clamp(route.Risk + profile.RouteRiskDelta, 0, 100);
            route.LastMargin = Math.Clamp(route.LastMargin - profile.RouteRiskDelta, -20, 30);
            route.BlockedShipmentCount = Math.Clamp(route.BlockedShipmentCount + profile.BlockedShipmentDelta, 0, 6);
            route.SeizureRisk = Math.Clamp(route.SeizureRisk + profile.SeizureRiskDelta, 0, 100);
            route.RouteConstraintLabel = DetermineRouteConstraintLabel(route);
            route.LastRouteTrace =
                $"{route.RouteName}受漕渠窗口{canalWindow}牵动，路险{route.Risk}、阻货{route.BlockedShipmentCount}、查缉险{route.SeizureRisk}。";

            if (oldRisk != route.Risk || oldBlocked != route.BlockedShipmentCount || oldSeizure != route.SeizureRisk)
            {
                changedSettlements.Add(route.SettlementId);
            }

            if (oldRisk < 60 && route.Risk >= 60)
            {
                scope.Emit(
                    TradeAndIndustryEventNames.RouteBusinessBlocked,
                    $"{route.RouteName}因漕渠窗口{canalWindow}而商路受阻。",
                    route.SettlementId.Value.ToString(),
                    BuildCanalTradeMetadata(domainEvent, profile, route.SettlementId));
            }
        }

        foreach (SettlementId settlementId in changedSettlements.OrderBy(static id => id.Value))
        {
            scope.RecordDiff(
                $"{settlementId.Value}地漕渠窗口{canalWindow}落入商路读数：市险变{profile.MarketRiskDelta}，路险变{profile.RouteRiskDelta}。",
                settlementId.Value.ToString());
        }
    }

    private static string ResolveCanalWindow(IDomainEvent domainEvent)
    {
        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.CanalWindowAfter, out string? metadataWindow)
            && !string.IsNullOrWhiteSpace(metadataWindow))
        {
            return metadataWindow;
        }

        return domainEvent.EntityKey ?? string.Empty;
    }

    private static Dictionary<string, string> BuildCanalTradeMetadata(
        IDomainEvent domainEvent,
        CanalTradePressureProfile profile,
        SettlementId settlementId)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseCanalWindow,
            [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
            [DomainEventMetadataKeys.SettlementId] = settlementId.Value.ToString(),
            [DomainEventMetadataKeys.CanalWindow] = ResolveCanalWindow(domainEvent),
            [DomainEventMetadataKeys.RouteRiskDelta] = profile.RouteRiskDelta.ToString(),
            [DomainEventMetadataKeys.MarketRiskDelta] = profile.MarketRiskDelta.ToString(),
        };
    }

    private static HashSet<SettlementId> BuildCanalExposedSettlementIds(IWorldSettlementsQueries worldQueries)
    {
        HashSet<SettlementId> interfaceNodes = worldQueries.GetSettlements()
            .Where(static settlement => settlement.NodeKind is SettlementNodeKind.CanalJunction
                or SettlementNodeKind.Wharf
                or SettlementNodeKind.Ferry)
            .Select(static settlement => settlement.Id)
            .ToHashSet();

        HashSet<SettlementId> exposed = new(interfaceNodes);
        foreach (RouteSnapshot route in worldQueries.GetRoutes().OrderBy(static route => route.Id.Value))
        {
            bool waterRoute = route.Medium is RouteMedium.WaterCanal
                or RouteMedium.WaterRiver
                or RouteMedium.FerryLink;
            bool touchesInterface = interfaceNodes.Contains(route.Origin)
                || interfaceNodes.Contains(route.Destination)
                || route.Waypoints.Any(interfaceNodes.Contains);

            if (!waterRoute && !touchesInterface)
            {
                continue;
            }

            exposed.Add(route.Origin);
            exposed.Add(route.Destination);
            foreach (SettlementId waypoint in route.Waypoints)
            {
                exposed.Add(waypoint);
            }
        }

        return exposed;
    }

    private static void ApplyHarvestPricePulse(ModuleEventHandlingScope<TradeAndIndustryState> scope, IDomainEvent domainEvent)
    {
        // Thin chain: harvest phase reduces grain supply and may trigger price spike.
        // Full formula (Step 3) will consider yield ratio, granary security, route risk.
        if (domainEvent.EntityKey != nameof(AgrarianPhase.Harvest))
        {
            return;
        }

        foreach (SettlementMarketState market in scope.State.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            MarketGoodsEntryState entry = GetOrCreateGrainEntry(scope.State, market.SettlementId);
            int oldPrice = entry.CurrentPrice;

            // Harvest tightens supply (hoarding, tribute, export pressure).
            entry.Supply = Math.Clamp(entry.Supply - 25, 0, 100);
            entry.CurrentPrice = Math.Clamp(entry.BasePrice + ((entry.Demand - entry.Supply) / 2), 50, 200);

            if (oldPrice < 120 && entry.CurrentPrice >= 120)
            {
                scope.Emit(
                    TradeAndIndustryEventNames.GrainPriceSpike,
                    $"{market.MarketName}秋收后粮价陡起，现每石{entry.CurrentPrice}文。",
                    market.SettlementId.Value.ToString(),
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseHarvest,
                        [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                        [DomainEventMetadataKeys.SettlementId] = market.SettlementId.Value.ToString(),
                        [DomainEventMetadataKeys.GrainOldPrice] = oldPrice.ToString(),
                        [DomainEventMetadataKeys.GrainCurrentPrice] = entry.CurrentPrice.ToString(),
                        [DomainEventMetadataKeys.GrainPriceDelta] = (entry.CurrentPrice - oldPrice).ToString(),
                        [DomainEventMetadataKeys.GrainSupply] = entry.Supply.ToString(),
                        [DomainEventMetadataKeys.GrainDemand] = entry.Demand.ToString(),
                    });
            }
        }
    }

    private readonly record struct CanalTradePressureProfile(
        int RouteRiskDelta,
        int MarketRiskDelta,
        int DemandDelta,
        int PriceIndexDelta,
        int BlockedShipmentDelta,
        int SeizureRiskDelta,
        int DiversionDelta,
        int ShadowPriceDelta);
}
