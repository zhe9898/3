using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed partial class OrderAndBanditryModule : ModuleRunner<OrderAndBanditryState>
{
    private static readonly string[] CommandNames =

    [

        "FundLocalWatch",

        "SuppressBanditry",

        "NegotiateWithOutlaws",

        "TolerateDisorder",

    ];


    private static readonly string[] EventNames =

    [

        OrderAndBanditryEventNames.BanditThreatRaised,

        OrderAndBanditryEventNames.OutlawGroupFormed,

        OrderAndBanditryEventNames.SuppressionSucceeded,

        OrderAndBanditryEventNames.RouteUnsafeDueToBanditry,

        OrderAndBanditryEventNames.BlackRoutePressureRaised,

    ];


    private static readonly string[] ConsumedEventNames =

    [

        WarfareCampaignEventNames.CampaignMobilized,

        WarfareCampaignEventNames.CampaignPressureRaised,

        WarfareCampaignEventNames.CampaignSupplyStrained,

        WarfareCampaignEventNames.CampaignAftermathRegistered,

        // Step 1b gap 3: world pulse / public life → order baseline (no-op dispatch)
        WorldSettlementsEventNames.FloodRiskThresholdBreached,
        WorldSettlementsEventNames.RouteConstraintEmerged,
        WorldSettlementsEventNames.CorveeWindowChanged,
        PublicLifeAndRumorEventNames.PrefectureDispatchPressed,
        PublicLifeAndRumorEventNames.CountyGateCrowded,
        PublicLifeAndRumorEventNames.StreetTalkSurged,
        PublicLifeAndRumorEventNames.MarketBuzzRaised,
        PublicLifeAndRumorEventNames.RoadReportDelayed,

    ];


    public override string ModuleKey => KnownModuleKeys.OrderAndBanditry;


    public override int ModuleSchemaVersion => 7;


    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;


    public override int ExecutionOrder => 700;


    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;


    public override FeatureMode DefaultMode => FeatureMode.Lite;


    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;


    public override IReadOnlyCollection<string> PublishedEvents => EventNames;


    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;


    public override OrderAndBanditryState CreateInitialState()

    {

        return new OrderAndBanditryState();

    }


    public override void RegisterQueries(OrderAndBanditryState state, QueryRegistry queries)

    {

        OrderAndBanditryQueries queryAdapter = new(state);

        queries.Register<IOrderAndBanditryQueries>(queryAdapter);

        queries.Register<IBlackRoutePressureQueries>(queryAdapter);

    }


    public override void RunXun(ModuleExecutionScope<OrderAndBanditryState> scope)

    {

        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();

        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();

        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();

        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();


        ITradeAndIndustryQueries? tradeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)

            ? scope.GetRequiredQuery<ITradeAndIndustryQueries>()

            : null;

        IBlackRouteLedgerQueries? blackRouteLedgerQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)

            ? scope.GetRequiredQuery<IBlackRouteLedgerQueries>()

            : null;

        IOfficeAndCareerQueries? officeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer)

            ? scope.GetRequiredQuery<IOfficeAndCareerQueries>()

            : null;

        IConflictAndForceQueries? conflictQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce)

            ? scope.GetRequiredQuery<IConflictAndForceQueries>()

            : null;


        IReadOnlyList<SettlementSnapshot> settlements = settlementQueries.GetSettlements();

        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();

        IReadOnlyList<ClanNarrativeSnapshot> narratives = socialQueries.GetClanNarratives();

        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = narratives.ToDictionary(static narrative => narrative.ClanId, static narrative => narrative);

        Dictionary<SettlementId, TradeActivitySnapshot> tradeActivityBySettlement = BuildTradeActivityBySettlement(tradeQueries);

        Dictionary<SettlementId, SettlementBlackRouteLedgerSnapshot> blackRouteLedgerBySettlement = blackRouteLedgerQueries is null

            ? new Dictionary<SettlementId, SettlementBlackRouteLedgerSnapshot>()

            : blackRouteLedgerQueries.GetSettlementBlackRouteLedgers().ToDictionary(static ledger => ledger.SettlementId, static ledger => ledger);

        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionBySettlement = officeQueries is null

            ? new Dictionary<SettlementId, JurisdictionAuthoritySnapshot>()

            : officeQueries.GetJurisdictions().ToDictionary(static authority => authority.SettlementId, static authority => authority);

        Dictionary<SettlementId, LocalForcePoolSnapshot> forceBySettlement = conflictQueries is null

            ? new Dictionary<SettlementId, LocalForcePoolSnapshot>()

            : conflictQueries.GetSettlementForces().ToDictionary(static force => force.SettlementId, static force => force);


        foreach (SettlementSnapshot settlement in settlements.OrderBy(static settlement => settlement.Id.Value))

        {

            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(settlement.Id);

            SettlementDisorderState disorder = GetOrCreateSettlement(scope.State, settlement.Id);


            ClanSnapshot[] localClans = clans

                .Where(clan => clan.HomeSettlementId == settlement.Id)

                .OrderBy(static clan => clan.Id.Value)

                .ToArray();

            int localFear = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.FearPressure);

            int localGrudge = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.GrudgePressure);


            TradeActivitySnapshot tradeActivity = tradeActivityBySettlement.TryGetValue(settlement.Id, out TradeActivitySnapshot? snapshot)

                ? snapshot

                : TradeActivitySnapshot.Empty;

            SettlementBlackRouteLedgerSnapshot blackRouteLedger = blackRouteLedgerBySettlement.TryGetValue(settlement.Id, out SettlementBlackRouteLedgerSnapshot? ledgerSnapshot)

                ? ledgerSnapshot

                : new SettlementBlackRouteLedgerSnapshot

                {

                    SettlementId = settlement.Id,

                };

            JurisdictionAuthoritySnapshot? jurisdiction = jurisdictionBySettlement.TryGetValue(settlement.Id, out JurisdictionAuthoritySnapshot? authority)

                ? authority

                : null;

            LocalForcePoolSnapshot? localForce = forceBySettlement.TryGetValue(settlement.Id, out LocalForcePoolSnapshot? forceSnapshot)

                ? forceSnapshot

                : null;

            bool hasActivatedResponse = localForce is not null && localForce.HasActiveConflict && localForce.IsResponseActivated;

            int forceSuppression = localForce is null ? 0 : ComputeForceSuppression(localForce);

            int paperCompliance = ComputePaperCompliance(jurisdiction);

            int implementationDrag = ComputeImplementationDrag(jurisdiction, paperCompliance);

            int administrativeRelief = jurisdiction is null ? 0 : ComputeAdministrativeRelief(jurisdiction, paperCompliance, implementationDrag);

            int responseActivationLevel = hasActivatedResponse ? localForce!.ResponseActivationLevel : 0;

            int administrativeSuppressionWindow = ComputeAdministrativeSuppressionWindow(paperCompliance, implementationDrag, jurisdiction);

            int activeEscortCount = hasActivatedResponse ? localForce!.EscortCount : 0;

            int routeShielding = ComputeRouteShielding(localForce, tradeActivity, administrativeRelief);


            switch (scope.Context.CurrentXun)

            {

                case SimulationXun.Shangxun:

                    ApplyXunOpeningPulse(

                        disorder,

                        settlement,

                        population,

                        localFear,

                        localGrudge,

                        implementationDrag,

                        routeShielding,

                        forceSuppression);

                    break;

                case SimulationXun.Zhongxun:

                    ApplyXunRoadPulse(

                        disorder,

                        settlement,

                        population,

                        tradeActivity,

                        blackRouteLedger,

                        routeShielding,

                        paperCompliance,

                        implementationDrag,

                        administrativeSuppressionWindow);

                    break;

                case SimulationXun.Xiaxun:

                    ApplyXunClosingPulse(

                        disorder,

                        settlement,

                        population,

                        tradeActivity,

                        blackRouteLedger,

                        localFear,

                        localGrudge,

                        routeShielding,

                        administrativeRelief,

                        activeEscortCount,

                        forceSuppression);

                    break;

            }


            disorder.ResponseActivationLevel = responseActivationLevel;

            disorder.PaperCompliance = paperCompliance;

            disorder.ImplementationDrag = implementationDrag;

            disorder.RouteShielding = routeShielding;

            disorder.AdministrativeSuppressionWindow = administrativeSuppressionWindow;

            disorder.SuppressionDemand = Math.Clamp(

                ((disorder.BanditThreat * 2) + disorder.RoutePressure + disorder.DisorderPressure + localFear + localGrudge) / 5

                - (population.MilitiaPotential / 25)

                - forceSuppression

                - (routeShielding >= 55 ? 2 : routeShielding >= 30 ? 1 : 0)

                - administrativeRelief,

                0,

                100);

            disorder.SuppressionRelief = Math.Clamp(

                forceSuppression

                + administrativeRelief

                + (routeShielding >= 60 ? 2 : routeShielding >= 35 ? 1 : 0)

                + (paperCompliance >= 55 ? 1 : 0)

                + (blackRouteLedger.SeizureRisk >= 55 ? 1 : 0)

                + (blackRouteLedger.BlockedShipmentCount >= 2 ? 1 : 0),

                0,

                12);

            disorder.RetaliationRisk = ComputeRetaliationRisk(

                disorder,

                localForce,

                tradeActivity,

                blackRouteLedger,

                localFear,

                localGrudge,

                routeShielding);

            disorder.EscalationBandLabel = DetermineEscalationBandLabel(disorder.BlackRoutePressure, disorder.CoercionRisk);

            disorder.LastPressureReason = BuildPressureReason(

                settlement,

                population,

                tradeActivity,

                localFear,

                localGrudge,

                localForce,

                forceSuppression,

                jurisdiction,

                administrativeRelief);

            disorder.LastPressureTrace = BuildBlackRoutePressureTrace(

                settlement,

                tradeActivity,

                blackRouteLedger,

                disorder,

                population,

                localFear,

                localGrudge);

        }

    }


    public override void RunMonth(ModuleExecutionScope<OrderAndBanditryState> scope)

    {

        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();

        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();

        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();

        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();


        ITradeAndIndustryQueries? tradeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)

            ? scope.GetRequiredQuery<ITradeAndIndustryQueries>()

            : null;

        IBlackRouteLedgerQueries? blackRouteLedgerQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)

            ? scope.GetRequiredQuery<IBlackRouteLedgerQueries>()

            : null;

        IOfficeAndCareerQueries? officeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer)

            ? scope.GetRequiredQuery<IOfficeAndCareerQueries>()

            : null;

        IConflictAndForceQueries? conflictQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce)

            ? scope.GetRequiredQuery<IConflictAndForceQueries>()

            : null;


        IReadOnlyList<SettlementSnapshot> settlements = settlementQueries.GetSettlements();

        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();

        IReadOnlyList<ClanNarrativeSnapshot> narratives = socialQueries.GetClanNarratives();

        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = narratives.ToDictionary(static narrative => narrative.ClanId, static narrative => narrative);

        Dictionary<SettlementId, TradeActivitySnapshot> tradeActivityBySettlement = BuildTradeActivityBySettlement(tradeQueries);

        Dictionary<SettlementId, SettlementBlackRouteLedgerSnapshot> blackRouteLedgerBySettlement = blackRouteLedgerQueries is null

            ? new Dictionary<SettlementId, SettlementBlackRouteLedgerSnapshot>()

            : blackRouteLedgerQueries.GetSettlementBlackRouteLedgers().ToDictionary(static ledger => ledger.SettlementId, static ledger => ledger);

        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionBySettlement = officeQueries is null

            ? new Dictionary<SettlementId, JurisdictionAuthoritySnapshot>()

            : officeQueries.GetJurisdictions().ToDictionary(static authority => authority.SettlementId, static authority => authority);

        Dictionary<SettlementId, LocalForcePoolSnapshot> forceBySettlement = conflictQueries is null

            ? new Dictionary<SettlementId, LocalForcePoolSnapshot>()

            : conflictQueries.GetSettlementForces().ToDictionary(static force => force.SettlementId, static force => force);


        foreach (SettlementSnapshot settlement in settlements.OrderBy(static settlement => settlement.Id.Value))

        {

            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(settlement.Id);

            SettlementDisorderState disorder = GetOrCreateSettlement(scope.State, settlement.Id);


            ClanSnapshot[] localClans = clans

                .Where(clan => clan.HomeSettlementId == settlement.Id)

                .OrderBy(static clan => clan.Id.Value)

                .ToArray();

            int localFear = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.FearPressure);

            int localGrudge = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.GrudgePressure);


            TradeActivitySnapshot tradeActivity = tradeActivityBySettlement.TryGetValue(settlement.Id, out TradeActivitySnapshot? snapshot)

                ? snapshot

                : TradeActivitySnapshot.Empty;

            SettlementBlackRouteLedgerSnapshot blackRouteLedger = blackRouteLedgerBySettlement.TryGetValue(settlement.Id, out SettlementBlackRouteLedgerSnapshot? ledgerSnapshot)

                ? ledgerSnapshot

                : new SettlementBlackRouteLedgerSnapshot

                {

                    SettlementId = settlement.Id,

                };

            JurisdictionAuthoritySnapshot? jurisdiction = jurisdictionBySettlement.TryGetValue(settlement.Id, out JurisdictionAuthoritySnapshot? authority)

                ? authority

                : null;

            LocalForcePoolSnapshot? localForce = forceBySettlement.TryGetValue(settlement.Id, out LocalForcePoolSnapshot? forceSnapshot)

                ? forceSnapshot

                : null;

            bool hasActivatedResponse = localForce is not null && localForce.HasActiveConflict && localForce.IsResponseActivated;

            int forceSuppression = localForce is null ? 0 : ComputeForceSuppression(localForce);

            int paperCompliance = ComputePaperCompliance(jurisdiction);

            int implementationDrag = ComputeImplementationDrag(jurisdiction, paperCompliance);

            int administrativeRelief = jurisdiction is null ? 0 : ComputeAdministrativeRelief(jurisdiction, paperCompliance, implementationDrag);

            int responseActivationLevel = hasActivatedResponse ? localForce!.ResponseActivationLevel : 0;

            int administrativeSuppressionWindow = ComputeAdministrativeSuppressionWindow(paperCompliance, implementationDrag, jurisdiction);

            int activeEscortCount = hasActivatedResponse ? localForce!.EscortCount : 0;

            int activeMilitiaCount = hasActivatedResponse ? localForce!.MilitiaCount : 0;

            int routeShielding = ComputeRouteShielding(localForce, tradeActivity, administrativeRelief);

            InterventionCarryoverEffect interventionCarryover = ResolveInterventionCarryover(

                disorder.LastInterventionCommandCode,

                disorder.InterventionCarryoverMonths);

            routeShielding = Math.Clamp(routeShielding + interventionCarryover.RouteShieldingDelta, 0, 100);


            int previousBanditThreat = disorder.BanditThreat;

            int previousRoutePressure = disorder.RoutePressure;

            int previousDisorderPressure = disorder.DisorderPressure;

            int previousSuppressionDemand = disorder.SuppressionDemand;

            int previousBlackRoutePressure = disorder.BlackRoutePressure;

            int previousCoercionRisk = disorder.CoercionRisk;

            int previousSuppressionRelief = disorder.SuppressionRelief;

            int previousResponseActivationLevel = disorder.ResponseActivationLevel;

            int previousPaperCompliance = disorder.PaperCompliance;

            int previousImplementationDrag = disorder.ImplementationDrag;

            int previousRouteShielding = disorder.RouteShielding;

            int previousRetaliationRisk = disorder.RetaliationRisk;

            int previousAdministrativeSuppressionWindow = disorder.AdministrativeSuppressionWindow;

            string previousEscalationBandLabel = disorder.EscalationBandLabel;

            string previousPressureTrace = disorder.LastPressureTrace;


            int banditDelta =

                (settlement.Security < 45 ? 2 : settlement.Security < 58 ? 1 : -1)

                + (population.CommonerDistress >= 60 ? 1 : population.CommonerDistress < 35 ? -1 : 0)

                + (population.MigrationPressure >= 50 ? 1 : 0)

                + (tradeActivity.AverageRouteRisk >= 55 ? 1 : 0)

                + (localFear >= 55 ? 1 : 0)

                + (implementationDrag >= 60 ? 1 : 0)

                + (routeShielding >= 55 ? -1 : 0)

                + (forceSuppression >= 8 ? -2 : forceSuppression >= 4 ? -1 : 0)

                + (administrativeRelief >= 2 ? -1 : 0)

                + interventionCarryover.BanditDelta

                + scope.Context.Random.NextInt(-1, 2);


            disorder.BanditThreat = Math.Clamp(disorder.BanditThreat + banditDelta, 0, 100);


            int routeDelta =

                (disorder.BanditThreat >= 50 ? 2 : disorder.BanditThreat >= 30 ? 1 : 0)

                + (tradeActivity.ActiveRouteCount > 0 ? 1 : 0)

                + (tradeActivity.TotalRouteCapacity >= 25 ? 1 : 0)

                + (tradeActivity.AverageRouteRisk >= 50 ? 1 : tradeActivity.AverageRouteRisk < 25 ? -1 : 0)

                + (settlement.Security < 50 ? 1 : 0)

                + (paperCompliance >= 55 ? 1 : 0)

                + (implementationDrag >= 45 ? 1 : 0)

                + (routeShielding >= 60 ? -2 : routeShielding >= 35 ? -1 : 0)

                + (activeEscortCount >= 8 ? -2 : activeEscortCount >= 4 ? -1 : 0)

                + (administrativeRelief >= 2 ? -1 : 0)

                + interventionCarryover.RouteDelta

                + scope.Context.Random.NextInt(-1, 2);


            disorder.RoutePressure = Math.Clamp(disorder.RoutePressure + routeDelta, 0, 100);


            int disorderDelta =

                (population.CommonerDistress >= 55 ? 1 : population.CommonerDistress < 35 ? -1 : 0)

                + (population.MigrationPressure >= 50 ? 1 : 0)

                + (localGrudge >= 55 ? 1 : 0)

                + (disorder.BanditThreat >= 60 ? 1 : 0)

                + (implementationDrag >= 55 ? 1 : 0)

                + (settlement.Security >= 65 ? -1 : 0)

                + (activeMilitiaCount >= 20 ? -1 : 0)

                + (routeShielding >= 45 ? -1 : 0)

                - administrativeRelief

                + interventionCarryover.DisorderDelta

                + scope.Context.Random.NextInt(-1, 2);


            disorder.DisorderPressure = Math.Clamp(disorder.DisorderPressure + disorderDelta, 0, 100);

            disorder.SuppressionDemand = Math.Clamp(

                ((disorder.BanditThreat * 2) + disorder.RoutePressure + disorder.DisorderPressure + localFear + localGrudge) / 5

                - (population.MilitiaPotential / 20)

                - forceSuppression

                - (routeShielding >= 55 ? 2 : routeShielding >= 30 ? 1 : 0)

                - administrativeRelief

                + interventionCarryover.SuppressionDemandDelta,

                0,

                100);

            disorder.ResponseActivationLevel = responseActivationLevel;

            disorder.PaperCompliance = paperCompliance;

            disorder.ImplementationDrag = implementationDrag;

            disorder.RouteShielding = routeShielding;

            disorder.AdministrativeSuppressionWindow = administrativeSuppressionWindow;

            disorder.SuppressionRelief = Math.Clamp(

                forceSuppression

                + administrativeRelief

                + (routeShielding >= 60 ? 2 : routeShielding >= 35 ? 1 : 0)

                + (paperCompliance >= 55 ? 1 : 0)

                + (blackRouteLedger.SeizureRisk >= 55 ? 1 : 0)

                + (blackRouteLedger.BlockedShipmentCount >= 2 ? 1 : 0)

                + interventionCarryover.SuppressionReliefDelta,

                0,

                12);

            int retaliationRisk = ComputeRetaliationRisk(

                disorder,

                localForce,

                tradeActivity,

                blackRouteLedger,

                localFear,

                localGrudge,

                routeShielding);

            retaliationRisk = Math.Clamp(retaliationRisk + interventionCarryover.RetaliationRiskDelta, 0, 100);

            disorder.RetaliationRisk = retaliationRisk;

            disorder.BlackRoutePressure = Math.Clamp(

                disorder.BlackRoutePressure

                + (disorder.RoutePressure >= 60 ? 2 : disorder.RoutePressure >= 35 ? 1 : 0)

                + (tradeActivity.ActiveRouteCount > 0 ? 1 : 0)

                + (tradeActivity.TotalRouteCapacity >= 25 ? 1 : 0)

                + (population.CommonerDistress >= 58 ? 1 : 0)

                + (population.MigrationPressure >= 50 ? 1 : 0)

                + (localFear >= 50 ? 1 : 0)

                + (localGrudge >= 55 ? 1 : 0)

                + (blackRouteLedger.DiversionShare >= 18 ? 1 : 0)

                + (blackRouteLedger.DiversionShare >= 35 ? 1 : 0)

                + (blackRouteLedger.ShadowPriceIndex >= 112 ? 1 : blackRouteLedger.ShadowPriceIndex <= 94 ? -1 : 0)

                + (blackRouteLedger.BlockedShipmentCount >= 2 ? 1 : 0)

                + (paperCompliance >= 45 && implementationDrag >= 35 ? 1 : 0)

                + (implementationDrag >= 55 ? 1 : 0)

                + (retaliationRisk >= 65 ? 2 : retaliationRisk >= 40 ? 1 : 0)

                - disorder.SuppressionRelief

                - (routeShielding >= 60 ? 2 : routeShielding >= 35 ? 1 : 0)

                - administrativeSuppressionWindow

                + interventionCarryover.BlackRoutePressureDelta

                + scope.Context.Random.NextInt(-1, 2),

                0,

                100);

            disorder.CoercionRisk = Math.Clamp(

                (disorder.BlackRoutePressure / 2)

                + (population.CommonerDistress / 5)

                + (localFear / 4)

                + (blackRouteLedger.DiversionShare / 4)

                + (blackRouteLedger.IllicitMargin / 4)

                + (implementationDrag / 6)

                + (retaliationRisk / 3)

                - (paperCompliance / 10)

                - (routeShielding / 6)

                - (disorder.SuppressionRelief * 2)

                - (administrativeSuppressionWindow * 2)

                + interventionCarryover.CoercionRiskDelta,

                0,

                100);

            disorder.EscalationBandLabel = DetermineEscalationBandLabel(disorder.BlackRoutePressure, disorder.CoercionRisk);


            disorder.LastPressureReason = BuildPressureReason(

                settlement,

                population,

                tradeActivity,

                localFear,

                localGrudge,

                localForce,

                forceSuppression,

                jurisdiction,

                administrativeRelief);

            disorder.LastPressureTrace = BuildBlackRoutePressureTrace(

                settlement,

                tradeActivity,

                blackRouteLedger,

                disorder,

                population,

                localFear,

                localGrudge);


            if (disorder.InterventionCarryoverMonths > 0)

            {

                disorder.InterventionCarryoverMonths = 0;

            }


            if (previousBanditThreat == disorder.BanditThreat &&

                previousRoutePressure == disorder.RoutePressure &&

                previousDisorderPressure == disorder.DisorderPressure &&

                previousSuppressionDemand == disorder.SuppressionDemand &&

                previousBlackRoutePressure == disorder.BlackRoutePressure &&

                previousCoercionRisk == disorder.CoercionRisk &&

                previousSuppressionRelief == disorder.SuppressionRelief &&

                previousResponseActivationLevel == disorder.ResponseActivationLevel &&

                previousPaperCompliance == disorder.PaperCompliance &&

                previousImplementationDrag == disorder.ImplementationDrag &&

                previousRouteShielding == disorder.RouteShielding &&

                previousRetaliationRisk == disorder.RetaliationRisk &&

                previousAdministrativeSuppressionWindow == disorder.AdministrativeSuppressionWindow &&

                string.Equals(previousEscalationBandLabel, disorder.EscalationBandLabel, StringComparison.Ordinal) &&

                string.Equals(previousPressureTrace, disorder.LastPressureTrace, StringComparison.Ordinal))

            {

                continue;

            }


            scope.RecordDiff(

                $"{settlement.Name}地面不靖：盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}，私路压{disorder.BlackRoutePressure}，胁迫险{disorder.CoercionRisk}。{disorder.LastPressureReason} {disorder.LastPressureTrace}",

                settlement.Id.Value.ToString());


            if (previousBanditThreat < 60 && disorder.BanditThreat >= 60)

            {

                scope.Emit(OrderAndBanditryEventNames.BanditThreatRaised, $"{settlement.Name}盗警骤起。");

            }


            if (previousDisorderPressure < 70 && disorder.DisorderPressure >= 70)

            {

                scope.Emit(OrderAndBanditryEventNames.OutlawGroupFormed, $"{settlement.Name}啸聚之势渐成。");

            }


            if (previousRoutePressure < 60 && disorder.RoutePressure >= 60)

            {

                scope.Emit(OrderAndBanditryEventNames.RouteUnsafeDueToBanditry, $"{settlement.Name}行路已不稳。");

            }


            if (previousSuppressionDemand >= 55 && disorder.SuppressionDemand <= 40)

            {

                scope.Emit(OrderAndBanditryEventNames.SuppressionSucceeded, $"{settlement.Name}镇压之需稍缓。");

            }


            if (previousBlackRoutePressure < 60 && disorder.BlackRoutePressure >= 60)

            {

                scope.Emit(OrderAndBanditryEventNames.BlackRoutePressureRaised, $"{settlement.Name}私路压力已起。", settlement.Id.Value.ToString());

            }

        }

        IReadOnlyList<string> priorBandIds = scope.State.OutlawBands
            .Select(static band => band.BandId)
            .ToArray();

        OrderAndBanditryStateProjection.BuildOrEvolveOutlawBands(scope.State);

        foreach (OutlawBandState band in scope.State.OutlawBands)
        {
            if (!priorBandIds.Contains(band.BandId, StringComparer.Ordinal))
            {
                scope.Emit(OrderAndBanditryEventNames.OutlawGroupFormed, $"{band.BandName}聚众成势。", band.BaseSettlementId.Value.ToString());
            }
        }

    }


    public override void HandleEvents(ModuleEventHandlingScope<OrderAndBanditryState> scope)

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


            SettlementDisorderState disorder = GetOrCreateSettlement(scope.State, bundle.SettlementId);

            int previousBanditThreat = disorder.BanditThreat;

            int previousRoutePressure = disorder.RoutePressure;

            int previousDisorderPressure = disorder.DisorderPressure;

            int previousSuppressionDemand = disorder.SuppressionDemand;

            int previousBlackRoutePressure = disorder.BlackRoutePressure;

            int previousCoercionRisk = disorder.CoercionRisk;

            int previousRouteShielding = disorder.RouteShielding;

            int previousRetaliationRisk = disorder.RetaliationRisk;

            string previousEscalationBandLabel = disorder.EscalationBandLabel;

            string previousPressureTrace = disorder.LastPressureTrace;


            int banditDelta = bundle.CampaignAftermathRegistered ? 2 : 0;

            banditDelta += bundle.CampaignSupplyStrained ? 1 : 0;

            banditDelta += Math.Max(0, campaign.FrontPressure - 60) / 20;


            int routeDelta = bundle.CampaignMobilized ? 1 : 0;

            routeDelta += bundle.CampaignPressureRaised ? 2 : 0;

            routeDelta += bundle.CampaignSupplyStrained ? 4 : 0;

            routeDelta += bundle.CampaignAftermathRegistered ? 2 : 0;

            routeDelta += Math.Max(0, 50 - campaign.SupplyState) / 10;


            int disorderDelta = bundle.CampaignAftermathRegistered ? 3 : 0;

            disorderDelta += bundle.CampaignPressureRaised ? 1 : 0;

            disorderDelta += Math.Max(0, 50 - campaign.MoraleState) / 12;


            int suppressionDelta = bundle.CampaignMobilized ? 1 : 0;

            suppressionDelta += bundle.CampaignPressureRaised ? 2 : 0;

            suppressionDelta += bundle.CampaignSupplyStrained ? 3 : 0;

            suppressionDelta += bundle.CampaignAftermathRegistered ? 2 : 0;


            disorder.BanditThreat = Math.Clamp(disorder.BanditThreat + banditDelta, 0, 100);

            disorder.RoutePressure = Math.Clamp(disorder.RoutePressure + routeDelta, 0, 100);

            disorder.DisorderPressure = Math.Clamp(disorder.DisorderPressure + disorderDelta, 0, 100);

            disorder.SuppressionDemand = Math.Clamp(disorder.SuppressionDemand + suppressionDelta, 0, 100);

            disorder.RouteShielding = Math.Clamp(

                disorder.RouteShielding

                - (bundle.CampaignMobilized ? 3 : 0)

                - (bundle.CampaignSupplyStrained ? 8 : 0)

                - (bundle.CampaignAftermathRegistered ? 4 : 0),

                0,

                100);

            disorder.RetaliationRisk = Math.Clamp(

                disorder.RetaliationRisk

                + (bundle.CampaignPressureRaised ? 6 : 0)

                + (bundle.CampaignSupplyStrained ? 5 : 0)

                + (bundle.CampaignAftermathRegistered ? 4 : 0)

                + Math.Max(0, campaign.FrontPressure - 60) / 6,

                0,

                100);

            disorder.BlackRoutePressure = Math.Clamp(disorder.BlackRoutePressure + routeDelta + (bundle.CampaignAftermathRegistered ? 2 : 0), 0, 100);

            disorder.CoercionRisk = Math.Clamp(

                disorder.CoercionRisk

                + disorderDelta

                + (bundle.CampaignSupplyStrained ? 3 : 1)

                + (disorder.RetaliationRisk / 10)

                - (disorder.RouteShielding / 12),

                0,

                100);

            disorder.EscalationBandLabel = DetermineEscalationBandLabel(disorder.BlackRoutePressure, disorder.CoercionRisk);

            disorder.LastPressureReason =

                $"{campaign.AnchorSettlementName}战事外溢，{campaign.FrontLabel}、{campaign.SupplyStateLabel}与{campaign.LastAftermathSummary}都压进了乡路巡哨。";

            disorder.LastPressureTrace =

                $"{campaign.AnchorSettlementName}战后余波压进私路，前线压{campaign.FrontPressure}、粮道态势{campaign.SupplyState}与善后余波一并逼高私货潜行。";


            if (previousBanditThreat == disorder.BanditThreat

                && previousRoutePressure == disorder.RoutePressure

                && previousDisorderPressure == disorder.DisorderPressure

                && previousSuppressionDemand == disorder.SuppressionDemand

                && previousBlackRoutePressure == disorder.BlackRoutePressure

                && previousCoercionRisk == disorder.CoercionRisk

                && previousRouteShielding == disorder.RouteShielding

                && previousRetaliationRisk == disorder.RetaliationRisk

                && string.Equals(previousEscalationBandLabel, disorder.EscalationBandLabel, StringComparison.Ordinal)

                && string.Equals(previousPressureTrace, disorder.LastPressureTrace, StringComparison.Ordinal))

            {

                continue;

            }


            scope.RecordDiff(

                $"{campaign.AnchorSettlementName}战事外溢：盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}，私路压{disorder.BlackRoutePressure}。{disorder.LastPressureReason} {disorder.LastPressureTrace}",

                bundle.SettlementId.Value.ToString());


            if (previousBanditThreat < 60 && disorder.BanditThreat >= 60)

            {

                scope.Emit(OrderAndBanditryEventNames.BanditThreatRaised, $"战事外溢使{campaign.AnchorSettlementName}盗警更紧。", bundle.SettlementId.Value.ToString());

            }


            if (previousDisorderPressure < 70 && disorder.DisorderPressure >= 70)

            {

                scope.Emit(OrderAndBanditryEventNames.OutlawGroupFormed, $"战事外溢使{campaign.AnchorSettlementName}啸聚更成形。", bundle.SettlementId.Value.ToString());

            }


            if (previousRoutePressure < 60 && disorder.RoutePressure >= 60)

            {

                scope.Emit(OrderAndBanditryEventNames.RouteUnsafeDueToBanditry, $"战事外溢使{campaign.AnchorSettlementName}行路更险。", bundle.SettlementId.Value.ToString());

            }


            if (previousBlackRoutePressure < 60 && disorder.BlackRoutePressure >= 60)

            {

                scope.Emit(OrderAndBanditryEventNames.BlackRoutePressureRaised, $"战事外溢使{campaign.AnchorSettlementName}私路更炽。", bundle.SettlementId.Value.ToString());

            }

        }

    }


    private static void DispatchWorldPulseEvents(ModuleEventHandlingScope<OrderAndBanditryState> scope)
    {
        // Step 1b gap 3 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：洪灾 / 徭役 / 州牒 / 县门拥堵落在哪个聚落；该聚落治安基线 / 地形 / 繁荣度；
        // 是否战时；是否粮荒窗口；既有匪患饱和度。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case WorldSettlementsEventNames.FloodRiskThresholdBreached:
                case WorldSettlementsEventNames.RouteConstraintEmerged:
                case WorldSettlementsEventNames.CorveeWindowChanged:
                case PublicLifeAndRumorEventNames.PrefectureDispatchPressed:
                case PublicLifeAndRumorEventNames.CountyGateCrowded:
                case PublicLifeAndRumorEventNames.StreetTalkSurged:
                case PublicLifeAndRumorEventNames.MarketBuzzRaised:
                case PublicLifeAndRumorEventNames.RoadReportDelayed:
                    // TODO Step 2: 按维度入口抬升匪患基线 / RouteUnsafe / BlackRoutePressure。
                    break;
            }
        }
    }
}
