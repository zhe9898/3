using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
{
    private static readonly string[] CommandNames =
    [
        "OpenShop",
        "CloseShop",
        "ExpandTradeRoute",
        "BorrowOrInvest",
        "AppointManager",
    ];

    private static readonly string[] EventNames =
    [
        "TradeProspered",
        "TradeLossOccurred",
        "TradeDebtDefaulted",
        "RouteBusinessBlocked",
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.TradeAndIndustry;

    public override int ModuleSchemaVersion => 3;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 600;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override TradeAndIndustryState CreateInitialState()
    {
        return new TradeAndIndustryState();
    }

    public override void RegisterQueries(TradeAndIndustryState state, QueryRegistry queries)
    {
        TradeQueries queryAdapter = new(state);
        queries.Register<ITradeAndIndustryQueries>(queryAdapter);
        queries.Register<IBlackRouteLedgerQueries>(queryAdapter);
    }

    public override void RunXun(ModuleExecutionScope<TradeAndIndustryState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();
        IOrderAndBanditryQueries? orderQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)
            ? scope.GetRequiredQuery<IOrderAndBanditryQueries>()
            : null;
        IBlackRoutePressureQueries? blackRoutePressureQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)
            ? scope.GetRequiredQuery<IBlackRoutePressureQueries>()
            : null;
        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = orderQueries is null
            ? new Dictionary<SettlementId, SettlementDisorderSnapshot>()
            : orderQueries.GetSettlementDisorder().ToDictionary(static disorder => disorder.SettlementId, static disorder => disorder);
        Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot> blackRoutePressureBySettlement = blackRoutePressureQueries is null
            ? new Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot>()
            : blackRoutePressureQueries.GetSettlementBlackRoutePressures().ToDictionary(static pressure => pressure.SettlementId, static pressure => pressure);
        Dictionary<ClanId, ClanNarrativeSnapshot> clanNarrativesByClanId = socialQueries.GetClanNarratives()
            .ToDictionary(static narrative => narrative.ClanId, static narrative => narrative);
        Dictionary<SettlementId, SettlementMarketState> markets = scope.State.Markets
            .OrderBy(static market => market.SettlementId.Value)
            .ToDictionary(static market => market.SettlementId, static market => market);
        Dictionary<SettlementId, TradeActivitySnapshot> tradeActivityBySettlement = BuildTradeActivityBySettlement(scope.State);

        foreach (SettlementMarketState market in scope.State.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(market.SettlementId);
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(market.SettlementId);
            SettlementBlackRoutePressureSnapshot blackRoutePressure = blackRoutePressureBySettlement.TryGetValue(market.SettlementId, out SettlementBlackRoutePressureSnapshot? pressureSnapshot)
                ? pressureSnapshot
                : new SettlementBlackRoutePressureSnapshot
                {
                    SettlementId = market.SettlementId,
                };
            SettlementDisorderSnapshot disorder = disorderBySettlement.TryGetValue(market.SettlementId, out SettlementDisorderSnapshot? disorderSnapshot)
                ? disorderSnapshot
                : new SettlementDisorderSnapshot
                {
                    SettlementId = market.SettlementId,
                };
            TradeActivitySnapshot settlementTradeActivity = tradeActivityBySettlement.TryGetValue(market.SettlementId, out TradeActivitySnapshot? tradeActivity)
                ? tradeActivity
                : TradeActivitySnapshot.Empty;
            SettlementBlackRouteLedgerState ledger = GetOrCreateBlackRouteLedger(scope.State, market.SettlementId);

            switch (scope.Context.CurrentXun)
            {
                case SimulationXun.Shangxun:
                    ApplyXunMarketPulse(market, settlement, population, blackRoutePressure);
                    break;
                case SimulationXun.Zhongxun:
                    ApplyXunLedgerPulse(market, settlementTradeActivity, blackRoutePressure, ledger);
                    break;
                case SimulationXun.Xiaxun:
                    ApplyXunLateMarketRiskPulse(market, disorder, blackRoutePressure);
                    break;
            }
        }

        if (scope.Context.CurrentXun != SimulationXun.Xiaxun)
        {
            return;
        }

        foreach (RouteTradeState route in scope.State.Routes
                     .Where(static route => route.IsActive)
                     .OrderBy(static route => route.RouteId))
        {
            if (!markets.TryGetValue(route.SettlementId, out SettlementMarketState? market))
            {
                continue;
            }

            ClanNarrativeSnapshot narrative = clanNarrativesByClanId.TryGetValue(route.ClanId, out ClanNarrativeSnapshot? narrativeSnapshot)
                ? narrativeSnapshot
                : new ClanNarrativeSnapshot
                {
                    ClanId = route.ClanId,
                };
            SettlementBlackRouteLedgerState ledger = GetOrCreateBlackRouteLedger(scope.State, route.SettlementId);
            SettlementBlackRoutePressureSnapshot pressure = blackRoutePressureBySettlement.TryGetValue(route.SettlementId, out SettlementBlackRoutePressureSnapshot? pressureSnapshot)
                ? pressureSnapshot
                : new SettlementBlackRoutePressureSnapshot
                {
                    SettlementId = route.SettlementId,
                };

            ApplyXunRoutePulse(route, market, narrative, pressure, ledger);
        }
    }

    public override void RunMonth(ModuleExecutionScope<TradeAndIndustryState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();
        IOrderAndBanditryQueries? orderQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)
            ? scope.GetRequiredQuery<IOrderAndBanditryQueries>()
            : null;
        IBlackRoutePressureQueries? blackRoutePressureQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)
            ? scope.GetRequiredQuery<IBlackRoutePressureQueries>()
            : null;
        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = orderQueries is null
            ? new Dictionary<SettlementId, SettlementDisorderSnapshot>()
            : orderQueries.GetSettlementDisorder().ToDictionary(static disorder => disorder.SettlementId, static disorder => disorder);
        Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot> blackRoutePressureBySettlement = blackRoutePressureQueries is null
            ? new Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot>()
            : blackRoutePressureQueries.GetSettlementBlackRoutePressures().ToDictionary(static pressure => pressure.SettlementId, static pressure => pressure);

        Dictionary<SettlementId, SettlementMarketState> markets = scope.State.Markets
            .OrderBy(static market => market.SettlementId.Value)
            .ToDictionary(static market => market.SettlementId, static market => market);
        Dictionary<SettlementId, TradeActivitySnapshot> tradeActivityBySettlement = BuildTradeActivityBySettlement(scope.State);

        foreach (SettlementMarketState market in scope.State.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(market.SettlementId);
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(market.SettlementId);
            int orderMarketPenalty = disorderBySettlement.TryGetValue(market.SettlementId, out SettlementDisorderSnapshot? disorder)
                ? ComputeOrderPenalty(disorder)
                : 0;
            OrderInterventionCarryoverEffect orderCarryover = disorder is null
                ? default
                : ResolveOrderCarryover(disorder);
            SettlementBlackRoutePressureSnapshot blackRoutePressure = blackRoutePressureBySettlement.TryGetValue(market.SettlementId, out SettlementBlackRoutePressureSnapshot? pressureSnapshot)
                ? pressureSnapshot
                : new SettlementBlackRoutePressureSnapshot
                {
                    SettlementId = market.SettlementId,
                };
            TradeActivitySnapshot settlementTradeActivity = tradeActivityBySettlement.TryGetValue(market.SettlementId, out TradeActivitySnapshot? tradeActivity)
                ? tradeActivity
                : TradeActivitySnapshot.Empty;
            SettlementBlackRouteLedgerState ledger = GetOrCreateBlackRouteLedger(scope.State, market.SettlementId);

            market.PriceIndex = Math.Clamp(
                market.PriceIndex
                + (population.CommonerDistress >= 55 ? 1 : -1)
                + (settlement.Prosperity >= 60 ? 1 : 0)
                + (blackRoutePressure.BlackRoutePressure >= 50 ? 1 : 0)
                - (blackRoutePressure.SuppressionRelief >= 5 ? 1 : 0)
                + scope.Context.Random.NextInt(-1, 2),
                50,
                150);
            market.Demand = Math.Clamp(
                market.Demand
                + (population.LaborSupply >= 90 ? 1 : -1)
                + (settlement.Prosperity >= 58 ? 1 : 0)
                - (blackRoutePressure.CoercionRisk >= 60 ? 1 : 0)
                + scope.Context.Random.NextInt(-1, 2),
                0,
                100);
            market.LocalRisk = Math.Clamp(
                market.LocalRisk
                + (settlement.Security < 45 ? 2 : -1)
                + (population.MigrationPressure >= 60 ? 1 : 0)
                + (blackRoutePressure.BlackRoutePressure >= 55 ? 1 : 0)
                + (blackRoutePressure.RetaliationRisk >= 55 ? 1 : 0)
                - (blackRoutePressure.RouteShielding >= 55 ? 2 : blackRoutePressure.RouteShielding >= 30 ? 1 : 0)
                + orderMarketPenalty
                + orderCarryover.LocalRiskDelta,
                0,
                100);

            ledger.ShadowPriceIndex = Math.Clamp(
                Math.Max(ledger.ShadowPriceIndex, 100)
                + (market.PriceIndex >= 110 ? 1 : market.PriceIndex <= 92 ? -1 : 0)
                + (blackRoutePressure.BlackRoutePressure >= 45 ? 1 : 0)
                + (blackRoutePressure.PaperCompliance >= 55 ? 1 : 0)
                + (blackRoutePressure.RetaliationRisk >= 60 ? 1 : 0)
                + (market.LocalRisk >= 55 ? 1 : 0)
                - (blackRoutePressure.AdministrativeSuppressionWindow >= 4 ? 1 : 0),
                70,
                180);
            ledger.DiversionShare = Math.Clamp(
                ledger.DiversionShare
                + (blackRoutePressure.BlackRoutePressure >= 60 ? 2 : blackRoutePressure.BlackRoutePressure >= 35 ? 1 : 0)
                + (settlementTradeActivity.ActiveRouteCount > 0 ? 1 : 0)
                + (market.LocalRisk >= 55 ? 1 : 0)
                + (market.Demand >= 65 ? 1 : 0)
                + (blackRoutePressure.ImplementationDrag >= 45 ? 1 : 0)
                + (blackRoutePressure.ImplementationDrag >= 65 ? 1 : 0)
                + (blackRoutePressure.RetaliationRisk >= 65 ? 2 : blackRoutePressure.RetaliationRisk >= 40 ? 1 : 0)
                - (blackRoutePressure.AdministrativeSuppressionWindow >= 4 ? 2 : blackRoutePressure.AdministrativeSuppressionWindow > 0 ? 1 : 0)
                - (blackRoutePressure.SuppressionRelief >= 4 ? 1 : 0)
                - (blackRoutePressure.RouteShielding >= 60 ? 2 : blackRoutePressure.RouteShielding >= 35 ? 1 : 0)
                + orderCarryover.DiversionDelta
                + scope.Context.Random.NextInt(-1, 2),
                0,
                100);
            ledger.BlockedShipmentCount = Math.Clamp(
                settlementTradeActivity.ActiveRouteCount == 0
                    ? 0
                    : (market.LocalRisk >= 60 ? 1 : 0)
                        + (blackRoutePressure.PaperCompliance >= 45 ? 1 : 0)
                        + (blackRoutePressure.AdministrativeSuppressionWindow >= 4 ? 1 : 0)
                        + (blackRoutePressure.ResponseActivationLevel >= 6 ? 1 : 0)
                        + (blackRoutePressure.RouteShielding >= 45 ? 1 : 0)
                        + (blackRoutePressure.RetaliationRisk >= 70 ? 1 : 0)
                        + (blackRoutePressure.BlackRoutePressure >= 65 ? 1 : 0)
                        + orderCarryover.BlockedShipmentDelta,
                0,
                12);
            ledger.SeizureRisk = Math.Clamp(
                (market.LocalRisk / 3)
                + (blackRoutePressure.PaperCompliance / 4)
                + (blackRoutePressure.AdministrativeSuppressionWindow * 6)
                + (blackRoutePressure.ResponseActivationLevel * 4)
                + (blackRoutePressure.RouteShielding / 5)
                - (blackRoutePressure.ImplementationDrag / 6)
                - (blackRoutePressure.CoercionRisk / 5)
                + orderCarryover.SeizureRiskDelta,
                0,
                100);
            ledger.IllicitMargin = Math.Clamp(
                ((ledger.ShadowPriceIndex - 100) / 4)
                + (ledger.DiversionShare / 10)
                + (settlementTradeActivity.TotalRouteCapacity / 30)
                - (ledger.SeizureRisk / 12)
                - ledger.BlockedShipmentCount,
                -10,
                30);
            ledger.DiversionBandLabel = DetermineDiversionBandLabel(ledger);
            ledger.LastLedgerTrace = BuildBlackRouteLedgerTrace(
                market,
                settlementTradeActivity,
                blackRoutePressure,
                ledger);
        }

        foreach (ClanTradeState clanTrade in scope.State.Clans.OrderBy(static trade => trade.ClanId.Value))
        {
            if (!markets.TryGetValue(clanTrade.PrimarySettlementId, out SettlementMarketState? market))
            {
                continue;
            }

            ClanSnapshot clan = familyQueries.GetRequiredClan(clanTrade.ClanId);
            ClanNarrativeSnapshot narrative = socialQueries.GetRequiredClanNarrative(clanTrade.ClanId);
            SettlementSnapshot settlement = settlementQueries.GetRequiredSettlement(clanTrade.PrimarySettlementId);
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(clanTrade.PrimarySettlementId);
            SettlementBlackRouteLedgerState primaryLedger = GetOrCreateBlackRouteLedger(scope.State, clanTrade.PrimarySettlementId);
            RouteTradeState[] routes = scope.State.Routes
                .Where(route => route.ClanId == clanTrade.ClanId && route.IsActive)
                .OrderBy(static route => route.RouteId)
                .ToArray();

            int routeFactor = 0;
            int orderPressure = 0;
            int grayRouteMargin = 0;
            int seizurePressure = 0;
            foreach (RouteTradeState route in routes)
            {
                int orderPenalty = disorderBySettlement.TryGetValue(route.SettlementId, out SettlementDisorderSnapshot? disorder)
                    ? ComputeOrderPenalty(disorder)
                    : 0;
                SettlementBlackRouteLedgerState routeLedger = GetOrCreateBlackRouteLedger(scope.State, route.SettlementId);
                SettlementBlackRoutePressureSnapshot routePressure = blackRoutePressureBySettlement.TryGetValue(route.SettlementId, out SettlementBlackRoutePressureSnapshot? pressureSnapshot)
                    ? pressureSnapshot
                    : new SettlementBlackRoutePressureSnapshot
                    {
                        SettlementId = route.SettlementId,
                    };
                int illicitOffset = routeLedger.IllicitMargin >= 12 ? 2 : routeLedger.IllicitMargin >= 4 ? 1 : 0;
                int diversionPenalty = routeLedger.DiversionShare >= 35 ? 2 : routeLedger.DiversionShare >= 18 ? 1 : 0;
                int routeLedgerSeizurePenalty = routeLedger.SeizureRisk >= 65 ? 2 : routeLedger.SeizureRisk >= 45 ? 1 : 0;
                int routeLedgerBlockedPenalty = routeLedger.BlockedShipmentCount >= 3 ? 2 : routeLedger.BlockedShipmentCount >= 1 ? 1 : 0;
                int routeShieldRelief = routePressure.RouteShielding >= 60 ? 2 : routePressure.RouteShielding >= 35 ? 1 : 0;
                int retaliationPenalty = routePressure.RetaliationRisk >= 65 ? 2 : routePressure.RetaliationRisk >= 40 ? 1 : 0;

                route.Risk = Math.Clamp(
                    route.Risk
                    + (market.LocalRisk >= 55 ? 1 : -1)
                    + (narrative.GrudgePressure >= 60 ? 1 : 0)
                    + orderPenalty
                    + diversionPenalty
                    + retaliationPenalty
                    + routeLedgerSeizurePenalty
                    + routeLedgerBlockedPenalty
                    - routeShieldRelief,
                    0,
                    100);
                route.BlockedShipmentCount = ComputeRouteBlockedShipmentCount(route, routeLedger, routePressure);
                route.SeizureRisk = ComputeRouteSeizureRisk(route, routeLedger, routePressure);
                route.RouteConstraintLabel = DetermineRouteConstraintLabel(route);
                route.LastRouteTrace = BuildRouteConstraintTrace(route, routePressure, routeLedger);
                int seizurePenalty = route.SeizureRisk >= 70 ? 2 : route.SeizureRisk >= 45 ? 1 : 0;
                int blockedPenalty = route.BlockedShipmentCount >= 3 ? 2 : route.BlockedShipmentCount >= 1 ? 1 : 0;
                route.LastMargin = Math.Clamp(
                    (route.Capacity / 8)
                    - (route.Risk / 10)
                    - orderPenalty
                    - blockedPenalty
                    - seizurePenalty
                    + illicitOffset
                    + scope.Context.Random.NextInt(-2, 3),
                    -20,
                    30);
                routeFactor += route.LastMargin;
                orderPressure += orderPenalty + diversionPenalty;
                grayRouteMargin += illicitOffset - blockedPenalty;
                seizurePressure += seizurePenalty + (routePressure.AdministrativeSuppressionWindow >= 4 ? 1 : 0);
            }

            int laborFactor = population.LaborSupply >= 100 ? 2 : population.LaborSupply >= 60 ? 1 : -1;
            int trustFactor = narrative.FavorBalance >= 10 ? 1 : 0;
            int grudgePenalty = narrative.GrudgePressure >= 60 ? 2 : narrative.GrudgePressure >= 40 ? 1 : 0;
            int clanSupport = clan.SupportReserve >= 55 ? 1 : 0;
            int prosperityFactor = settlement.Prosperity >= 60 ? 2 : settlement.Prosperity < 45 ? -1 : 0;
            int debtPenalty = clanTrade.Debt / 30;
            int primaryIllicitLift = primaryLedger.IllicitMargin >= 10 ? 1 : 0;

            int margin = routeFactor
                + (market.Demand / 10)
                + ((market.PriceIndex - 100) / 10)
                + laborFactor
                + trustFactor
                + clanSupport
                + prosperityFactor
                + clanTrade.ManagerSkill
                + grayRouteMargin
                + primaryIllicitLift
                - orderPressure
                - seizurePressure
                - grudgePenalty
                - debtPenalty
                + scope.Context.Random.NextInt(-3, 4);

            clanTrade.CashReserve = Math.Clamp(clanTrade.CashReserve + (margin * 2), 0, 500);
            clanTrade.GrainReserve = Math.Clamp(clanTrade.GrainReserve + Math.Max(-4, margin / 2), 0, 500);

            if (margin >= 0)
            {
                clanTrade.Debt = Math.Clamp(clanTrade.Debt - Math.Max(1, margin / 3), 0, 300);
                clanTrade.CommerceReputation = Math.Clamp(clanTrade.CommerceReputation + 1, 0, 100);
                clanTrade.LastOutcome = "Profit";
            }
            else
            {
                clanTrade.Debt = Math.Clamp(clanTrade.Debt + Math.Abs(margin), 0, 300);
                clanTrade.CommerceReputation = Math.Clamp(clanTrade.CommerceReputation - 1, 0, 100);
                clanTrade.LastOutcome = "Loss";
            }

            clanTrade.LastExplanation =
                $"市需{market.Demand}、时价{market.PriceIndex}、丁力{population.LaborSupply}、路势{routeFactor}、私下分流得失{grayRouteMargin}、查缉之险{seizurePressure}、不靖之压{orderPressure}与旧怨所损{grudgePenalty}，折成盈亏{margin}。{primaryLedger.LastLedgerTrace}";

            scope.RecordDiff(
                $"{clan.ClanName}商账今存银{clanTrade.CashReserve}，负债{clanTrade.Debt}，本月{RenderTradeOutcome(clanTrade.LastOutcome)}。{clanTrade.LastExplanation}",
                clanTrade.ClanId.Value.ToString());

            if (margin >= 8)
            {
                scope.Emit("TradeProspered", $"{clan.ClanName}本月市利有进。");
            }
            else if (margin < 0)
            {
                scope.Emit("TradeLossOccurred", $"{clan.ClanName}本月商账受亏。");
            }

            if (clanTrade.Debt >= 120 && clanTrade.CashReserve <= 40)
            {
                scope.Emit("TradeDebtDefaulted", $"{clan.ClanName}商债压门。");
            }

            if (routes.Any(static route => route.Risk >= 70) || orderPressure >= 3 || primaryLedger.BlockedShipmentCount >= 2)
            {
                scope.Emit("RouteBusinessBlocked", $"{clan.ClanName}所行商路受阻。");
            }
        }
    }

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

    public override void HandleEvents(ModuleEventHandlingScope<TradeAndIndustryState> scope)
    {
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

    private sealed class TradeQueries : ITradeAndIndustryQueries, IBlackRouteLedgerQueries
    {
        private readonly TradeAndIndustryState _state;

        public TradeQueries(TradeAndIndustryState state)
        {
            _state = state;
        }

        public ClanTradeSnapshot GetRequiredClanTrade(ClanId clanId)
        {
            ClanTradeState trade = _state.Clans.Single(trade => trade.ClanId == clanId);
            return CloneClanTrade(trade);
        }

        public IReadOnlyList<ClanTradeSnapshot> GetClanTrades()
        {
            return _state.Clans
                .OrderBy(static trade => trade.ClanId.Value)
                .Select(CloneClanTrade)
                .ToArray();
        }

        public IReadOnlyList<MarketSnapshot> GetMarkets()
        {
            return _state.Markets
                .OrderBy(static market => market.SettlementId.Value)
                .Select(CloneMarket)
                .ToArray();
        }

        public IReadOnlyList<ClanTradeRouteSnapshot> GetRoutesForClan(ClanId clanId)
        {
            return _state.Routes
                .Where(route => route.ClanId == clanId)
                .OrderBy(static route => route.RouteId)
                .Select(CloneRoute)
                .ToArray();
        }

        public SettlementBlackRouteLedgerSnapshot GetRequiredSettlementBlackRouteLedger(SettlementId settlementId)
        {
            SettlementBlackRouteLedgerState ledger = _state.BlackRouteLedgers.Single(ledger => ledger.SettlementId == settlementId);
            return CloneLedger(ledger);
        }

        public IReadOnlyList<SettlementBlackRouteLedgerSnapshot> GetSettlementBlackRouteLedgers()
        {
            return _state.BlackRouteLedgers
                .OrderBy(static ledger => ledger.SettlementId.Value)
                .Select(CloneLedger)
                .ToArray();
        }

        private static ClanTradeSnapshot CloneClanTrade(ClanTradeState trade)
        {
            return new ClanTradeSnapshot
            {
                ClanId = trade.ClanId,
                PrimarySettlementId = trade.PrimarySettlementId,
                CashReserve = trade.CashReserve,
                GrainReserve = trade.GrainReserve,
                Debt = trade.Debt,
                CommerceReputation = trade.CommerceReputation,
                ShopCount = trade.ShopCount,
                LastOutcome = trade.LastOutcome,
                LastExplanation = trade.LastExplanation,
            };
        }

        private static MarketSnapshot CloneMarket(SettlementMarketState market)
        {
            return new MarketSnapshot
            {
                SettlementId = market.SettlementId,
                MarketName = market.MarketName,
                PriceIndex = market.PriceIndex,
                Demand = market.Demand,
                LocalRisk = market.LocalRisk,
            };
        }

        private static ClanTradeRouteSnapshot CloneRoute(RouteTradeState route)
        {
            return new ClanTradeRouteSnapshot
            {
                RouteId = route.RouteId,
                ClanId = route.ClanId,
                RouteName = route.RouteName,
                SettlementId = route.SettlementId,
                IsActive = route.IsActive,
                Capacity = route.Capacity,
                Risk = route.Risk,
                LastMargin = route.LastMargin,
                BlockedShipmentCount = route.BlockedShipmentCount,
                SeizureRisk = route.SeizureRisk,
                RouteConstraintLabel = route.RouteConstraintLabel,
                LastRouteTrace = route.LastRouteTrace,
            };
        }

        private static SettlementBlackRouteLedgerSnapshot CloneLedger(SettlementBlackRouteLedgerState ledger)
        {
            return new SettlementBlackRouteLedgerSnapshot
            {
                SettlementId = ledger.SettlementId,
                ShadowPriceIndex = ledger.ShadowPriceIndex,
                DiversionShare = ledger.DiversionShare,
                IllicitMargin = ledger.IllicitMargin,
                BlockedShipmentCount = ledger.BlockedShipmentCount,
                SeizureRisk = ledger.SeizureRisk,
                DiversionBandLabel = ledger.DiversionBandLabel,
                LastLedgerTrace = ledger.LastLedgerTrace,
            };
        }
    }

    private sealed record TradeActivitySnapshot(int ActiveRouteCount, int AverageRouteRisk, int TotalRouteCapacity)
    {
        public static readonly TradeActivitySnapshot Empty = new(0, 0, 0);
    }

    private readonly record struct OrderInterventionCarryoverEffect(
        int LocalRiskDelta,
        int DiversionDelta,
        int BlockedShipmentDelta,
        int SeizureRiskDelta);
}
