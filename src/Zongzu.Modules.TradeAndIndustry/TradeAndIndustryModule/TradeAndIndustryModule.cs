using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.TradeAndIndustry;

public sealed partial class TradeAndIndustryModule : ModuleRunner<TradeAndIndustryState>
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

        TradeAndIndustryEventNames.TradeProspered,

        TradeAndIndustryEventNames.TradeLossOccurred,

        TradeAndIndustryEventNames.TradeDebtDefaulted,

        TradeAndIndustryEventNames.RouteBusinessBlocked,

        TradeAndIndustryEventNames.GrainPriceSpike,

    ];


    private static readonly string[] ConsumedEventNames =

    [

        WarfareCampaignEventNames.CampaignMobilized,

        WarfareCampaignEventNames.CampaignPressureRaised,

        WarfareCampaignEventNames.CampaignSupplyStrained,

        WarfareCampaignEventNames.CampaignAftermathRegistered,

        // Step 1b gap 3: world pulse / public life → trade routes & prices (no-op dispatch)
        WorldSettlementsEventNames.FloodRiskThresholdBreached,
        WorldSettlementsEventNames.RouteConstraintEmerged,
        WorldSettlementsEventNames.CorveeWindowChanged,
        WorldSettlementsEventNames.SeasonPhaseAdvanced,
        PublicLifeAndRumorEventNames.MarketBuzzRaised,
        PublicLifeAndRumorEventNames.RoadReportDelayed,

    ];


    public override string ModuleKey => KnownModuleKeys.TradeAndIndustry;


    public override int ModuleSchemaVersion => 4;


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


        // Phase 5 商贸骨骼 —— 收成 → 粮食供给 → 粮价（薄链）。
        ApplyMonthlyGrainPulse(scope.State, populationQueries, settlementQueries);


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

                scope.Emit(TradeAndIndustryEventNames.TradeProspered, $"{clan.ClanName}本月市利有进。");

            }

            else if (margin < 0)

            {

                scope.Emit(TradeAndIndustryEventNames.TradeLossOccurred, $"{clan.ClanName}本月商账受亏。");

            }


            if (clanTrade.Debt >= 120 && clanTrade.CashReserve <= 40)

            {

                scope.Emit(TradeAndIndustryEventNames.TradeDebtDefaulted, $"{clan.ClanName}商债压门。");

            }


            if (routes.Any(static route => route.Risk >= 70) || orderPressure >= 3 || primaryLedger.BlockedShipmentCount >= 2)

            {

                scope.Emit(TradeAndIndustryEventNames.RouteBusinessBlocked, $"{clan.ClanName}所行商路受阻。");

            }

        }

    }


}
