using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryModule : ModuleRunner<OrderAndBanditryState>
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
        "BanditThreatRaised",
        "OutlawGroupFormed",
        "SuppressionSucceeded",
        "RouteUnsafeDueToBanditry",
        "BlackRoutePressureRaised",
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.OrderAndBanditry;

    public override int ModuleSchemaVersion => 6;

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
                scope.Emit("BanditThreatRaised", $"{settlement.Name}盗警骤起。");
            }

            if (previousDisorderPressure < 70 && disorder.DisorderPressure >= 70)
            {
                scope.Emit("OutlawGroupFormed", $"{settlement.Name}啸聚之势渐成。");
            }

            if (previousRoutePressure < 60 && disorder.RoutePressure >= 60)
            {
                scope.Emit("RouteUnsafeDueToBanditry", $"{settlement.Name}行路已不稳。");
            }

            if (previousSuppressionDemand >= 55 && disorder.SuppressionDemand <= 40)
            {
                scope.Emit("SuppressionSucceeded", $"{settlement.Name}镇压之需稍缓。");
            }

            if (previousBlackRoutePressure < 60 && disorder.BlackRoutePressure >= 60)
            {
                scope.Emit("BlackRoutePressureRaised", $"{settlement.Name}私路压力已起。", settlement.Id.Value.ToString());
            }
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<OrderAndBanditryState> scope)
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
                scope.Emit("BanditThreatRaised", $"战事外溢使{campaign.AnchorSettlementName}盗警更紧。", bundle.SettlementId.Value.ToString());
            }

            if (previousDisorderPressure < 70 && disorder.DisorderPressure >= 70)
            {
                scope.Emit("OutlawGroupFormed", $"战事外溢使{campaign.AnchorSettlementName}啸聚更成形。", bundle.SettlementId.Value.ToString());
            }

            if (previousRoutePressure < 60 && disorder.RoutePressure >= 60)
            {
                scope.Emit("RouteUnsafeDueToBanditry", $"战事外溢使{campaign.AnchorSettlementName}行路更险。", bundle.SettlementId.Value.ToString());
            }

            if (previousBlackRoutePressure < 60 && disorder.BlackRoutePressure >= 60)
            {
                scope.Emit("BlackRoutePressureRaised", $"战事外溢使{campaign.AnchorSettlementName}私路更炽。", bundle.SettlementId.Value.ToString());
            }
        }
    }

    private static Dictionary<SettlementId, TradeActivitySnapshot> BuildTradeActivityBySettlement(ITradeAndIndustryQueries? tradeQueries)
    {
        if (tradeQueries is null)
        {
            return new Dictionary<SettlementId, TradeActivitySnapshot>();
        }

        Dictionary<SettlementId, List<TradeRouteSnapshot>> routesBySettlement = new();
        foreach (ClanTradeSnapshot clanTrade in tradeQueries.GetClanTrades().OrderBy(static trade => trade.ClanId.Value))
        {
            foreach (TradeRouteSnapshot route in tradeQueries.GetRoutesForClan(clanTrade.ClanId)
                         .Where(static route => route.IsActive)
                         .OrderBy(static route => route.RouteId))
            {
                if (!routesBySettlement.TryGetValue(route.SettlementId, out List<TradeRouteSnapshot>? routes))
                {
                    routes = [];
                    routesBySettlement[route.SettlementId] = routes;
                }

                routes.Add(route);
            }
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

    private static int AverageNarrative(
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyDictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan,
        Func<ClanNarrativeSnapshot, int> selector)
    {
        int total = 0;
        int count = 0;
        foreach (ClanSnapshot clan in clans)
        {
            if (!narrativesByClan.TryGetValue(clan.Id, out ClanNarrativeSnapshot? narrative))
            {
                continue;
            }

            total += selector(narrative);
            count += 1;
        }

        return count == 0 ? 0 : total / count;
    }

    private static SettlementDisorderState GetOrCreateSettlement(OrderAndBanditryState state, SettlementId settlementId)
    {
        SettlementDisorderState? settlement = state.Settlements.SingleOrDefault(existing => existing.SettlementId == settlementId);
        if (settlement is not null)
        {
            return settlement;
        }

        settlement = new SettlementDisorderState
        {
            SettlementId = settlementId,
        };
        state.Settlements.Add(settlement);
        state.Settlements = state.Settlements.OrderBy(static entry => entry.SettlementId.Value).ToList();
        return settlement;
    }

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

    private static string BuildPressureReason(
        SettlementSnapshot settlement,
        PopulationSettlementSnapshot population,
        TradeActivitySnapshot tradeActivity,
        int localFear,
        int localGrudge,
        LocalForcePoolSnapshot? localForce,
        int forceSuppression,
        JurisdictionAuthoritySnapshot? jurisdiction,
        int administrativeRelief)
    {
        List<string> reasons = [];

        if (settlement.Security < 50)
        {
            reasons.Add($"乡面安宁仅{settlement.Security}，仓路与铺户更易外露。");
        }

        if (population.CommonerDistress >= 55)
        {
            reasons.Add($"民困{population.CommonerDistress}，乡里窘急渐聚。");
        }

        if (localForce is not null && forceSuppression > 0)
        {
            reasons.Add($"已激活的守丁{localForce.GuardCount}、护运{localForce.EscortCount}、整备{localForce.Readiness}与应援{localForce.OrderSupportLevel}，正缓住事势。");
        }

        if (jurisdiction is not null && administrativeRelief > 0)
        {
            reasons.Add($"{jurisdiction.LeadOfficeTitle}乡面杠力{jurisdiction.JurisdictionLeverage}，可替镇压之需卸去{administrativeRelief}分。");
        }

        if (population.MigrationPressure >= 45)
        {
            reasons.Add($"流徙之压{population.MigrationPressure}，乡面人气渐散。");
        }

        if (tradeActivity.ActiveRouteCount > 0)
        {
            reasons.Add($"现有{tradeActivity.ActiveRouteCount}条活路、载力{tradeActivity.TotalRouteCapacity}，行旅财货尽在外露。");
        }

        if (tradeActivity.AverageRouteRisk >= 45)
        {
            reasons.Add($"路险均值{tradeActivity.AverageRouteRisk}，最招乘隙劫掠。");
        }

        if (localFear >= 50 || localGrudge >= 55)
        {
            reasons.Add($"乡里惧意{localFear}、旧怨{localGrudge}，渐化为不靖之势。");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("本月乡里大势尚能按住。");
        }

        return string.Join(" ", reasons.Take(3));
    }

    private static int ComputeForceSuppression(LocalForcePoolSnapshot localForce)
    {
        if (!localForce.HasActiveConflict || !localForce.IsResponseActivated || localForce.OrderSupportLevel <= 0)
        {
            return 0;
        }

        return localForce.OrderSupportLevel;
    }

    private static int ComputePaperCompliance(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null)
        {
            return 0;
        }

        int taskSignal = jurisdiction.AdministrativeTaskTier switch
        {
            "crisis" => 12,
            "district" => 10,
            "registry" => 6,
            "clerical" => 4,
            _ => 3,
        };

        return Math.Clamp(
            (jurisdiction.JurisdictionLeverage / 2)
            + (jurisdiction.AuthorityTier * 10)
            + taskSignal
            + (jurisdiction.AdministrativeTaskLoad / 3)
            - (jurisdiction.PetitionPressure / 8),
            0,
            100);
    }

    private static int ComputeImplementationDrag(JurisdictionAuthoritySnapshot? jurisdiction, int paperCompliance)
    {
        if (jurisdiction is null)
        {
            return 0;
        }

        return Math.Clamp(
            (jurisdiction.ClerkDependence / 2)
            + (jurisdiction.PetitionPressure / 2)
            + (jurisdiction.PetitionBacklog / 2)
            + (jurisdiction.AdministrativeTaskLoad / 3)
            - (paperCompliance / 5)
            - (jurisdiction.AuthorityTier * 4),
            0,
            100);
    }

    private static int ComputeAdministrativeRelief(
        JurisdictionAuthoritySnapshot jurisdiction,
        int paperCompliance,
        int implementationDrag)
    {
        int effectiveReach = paperCompliance - implementationDrag;
        int relief = effectiveReach >= 30
            ? 2
            : effectiveReach >= 12
                ? 1
                : 0;

        if (jurisdiction.PetitionPressure >= 60)
        {
            relief -= 1;
        }

        return Math.Max(0, relief);
    }

    private static int ComputeAdministrativeSuppressionWindow(
        int paperCompliance,
        int implementationDrag,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null)
        {
            return 0;
        }

        return Math.Clamp(
            (paperCompliance / 16)
            + (jurisdiction.AuthorityTier >= 2 ? 1 : 0)
            - (implementationDrag / 18),
            0,
            8);
    }

    private static int ComputeRouteShielding(
        LocalForcePoolSnapshot? localForce,
        TradeActivitySnapshot tradeActivity,
        int administrativeRelief)
    {
        if (localForce is null
            || !localForce.HasActiveConflict
            || !localForce.IsResponseActivated
            || tradeActivity.ActiveRouteCount == 0)
        {
            return 0;
        }

        return Math.Clamp(
            (localForce.OrderSupportLevel * 5)
            + (localForce.EscortCount * 2)
            + (localForce.GuardCount / 2)
            + (localForce.Readiness / 3)
            + (localForce.CommandCapacity / 4)
            + (administrativeRelief * 8)
            - (localForce.CampaignEscortStrain / 2)
            - (localForce.CampaignFatigue / 3)
            - Math.Max(0, tradeActivity.AverageRouteRisk - 45) / 2,
            0,
            100);
    }

    private static int ComputeRetaliationRisk(
        SettlementDisorderState disorder,
        LocalForcePoolSnapshot? localForce,
        TradeActivitySnapshot tradeActivity,
        SettlementBlackRouteLedgerSnapshot blackRouteLedger,
        int localFear,
        int localGrudge,
        int routeShielding)
    {
        if (localForce is null || !localForce.IsResponseActivated)
        {
            return 0;
        }

        return Math.Clamp(
            (disorder.BlackRoutePressure / 3)
            + (disorder.RoutePressure / 4)
            + (localFear / 4)
            + (localGrudge / 4)
            + (blackRouteLedger.DiversionShare / 5)
            + (blackRouteLedger.IllicitMargin / 4)
            + (tradeActivity.ActiveRouteCount > 0 ? 8 : 0)
            + (tradeActivity.AverageRouteRisk / 8)
            + (localForce.CampaignEscortStrain / 2)
            + (localForce.CampaignFatigue / 3)
            - (routeShielding / 2)
            - (localForce.OrderSupportLevel * 2)
            - (localForce.Readiness / 6),
            0,
            100);
    }

    private static string DetermineEscalationBandLabel(int blackRoutePressure, int coercionRisk)
    {
        int combined = blackRoutePressure + coercionRisk;
        return combined switch
        {
            >= 130 => "私路成势",
            >= 100 => "暗运成线",
            >= 70 => "夹带渐多",
            >= 40 => "私贩试探",
            _ => "尚未成势",
        };
    }

    private static string BuildBlackRoutePressureTrace(
        SettlementSnapshot settlement,
        TradeActivitySnapshot tradeActivity,
        SettlementBlackRouteLedgerSnapshot blackRouteLedger,
        SettlementDisorderState disorder,
        PopulationSettlementSnapshot population,
        int localFear,
        int localGrudge)
    {
        List<string> reasons = [];

        if (tradeActivity.ActiveRouteCount > 0)
        {
            reasons.Add($"正路尚有{tradeActivity.ActiveRouteCount}条，私货便在河埠与街市间夹带试行。");
        }

        if (blackRouteLedger.DiversionShare > 0 || blackRouteLedger.IllicitMargin > 0)
        {
            reasons.Add($"私下分流{blackRouteLedger.DiversionShare}，浮利约{blackRouteLedger.IllicitMargin}，私下转运正在挤压正市。");
        }

        if (population.CommonerDistress >= 55 || localFear >= 50 || localGrudge >= 50)
        {
            reasons.Add($"民困{population.CommonerDistress}、惧意{localFear}、旧怨{localGrudge}，更易逼人投向私运与胁从。");
        }

        if (disorder.SuppressionRelief > 0 || disorder.AdministrativeSuppressionWindow > 0)
        {
            reasons.Add($"眼下可调护力{disorder.SuppressionRelief}，官面查缉窗口{disorder.AdministrativeSuppressionWindow}，还能暂压一程。");
        }

        if (reasons.Count == 0)
        {
            reasons.Add($"{settlement.Name}私路尚浅，仍以试探为主。");
        }

        return string.Join(" ", reasons.Take(3));
    }

    private sealed class OrderAndBanditryQueries : IOrderAndBanditryQueries, IBlackRoutePressureQueries
    {
        private readonly OrderAndBanditryState _state;

        public OrderAndBanditryQueries(OrderAndBanditryState state)
        {
            _state = state;
        }

        public SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId)
        {
            SettlementDisorderState settlement = _state.Settlements.Single(settlement => settlement.SettlementId == settlementId);
            return Clone(settlement);
        }

        public IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder()
        {
            return _state.Settlements
                .OrderBy(static settlement => settlement.SettlementId.Value)
                .Select(Clone)
                .ToArray();
        }

        public SettlementBlackRoutePressureSnapshot GetRequiredSettlementBlackRoutePressure(SettlementId settlementId)
        {
            SettlementDisorderState settlement = _state.Settlements.Single(settlement => settlement.SettlementId == settlementId);
            return CloneBlackRoutePressure(settlement);
        }

        public IReadOnlyList<SettlementBlackRoutePressureSnapshot> GetSettlementBlackRoutePressures()
        {
            return _state.Settlements
                .OrderBy(static settlement => settlement.SettlementId.Value)
                .Select(CloneBlackRoutePressure)
                .ToArray();
        }

        private static SettlementDisorderSnapshot Clone(SettlementDisorderState settlement)
        {
            return new SettlementDisorderSnapshot
            {
                SettlementId = settlement.SettlementId,
                BanditThreat = settlement.BanditThreat,
                RoutePressure = settlement.RoutePressure,
                SuppressionDemand = settlement.SuppressionDemand,
                DisorderPressure = settlement.DisorderPressure,
                LastPressureReason = settlement.LastPressureReason,
                LastInterventionCommandCode = settlement.LastInterventionCommandCode,
                LastInterventionCommandLabel = settlement.LastInterventionCommandLabel,
                LastInterventionSummary = settlement.LastInterventionSummary,
                LastInterventionOutcome = settlement.LastInterventionOutcome,
                InterventionCarryoverMonths = settlement.InterventionCarryoverMonths,
            };
        }

        private static SettlementBlackRoutePressureSnapshot CloneBlackRoutePressure(SettlementDisorderState settlement)
        {
            return new SettlementBlackRoutePressureSnapshot
            {
                SettlementId = settlement.SettlementId,
                BlackRoutePressure = settlement.BlackRoutePressure,
                CoercionRisk = settlement.CoercionRisk,
                SuppressionRelief = settlement.SuppressionRelief,
                ResponseActivationLevel = settlement.ResponseActivationLevel,
                PaperCompliance = settlement.PaperCompliance,
                ImplementationDrag = settlement.ImplementationDrag,
                RouteShielding = settlement.RouteShielding,
                RetaliationRisk = settlement.RetaliationRisk,
                AdministrativeSuppressionWindow = settlement.AdministrativeSuppressionWindow,
                EscalationBandLabel = settlement.EscalationBandLabel,
                LastPressureTrace = settlement.LastPressureTrace,
            };
        }
    }

    private sealed record TradeActivitySnapshot(int ActiveRouteCount, int AverageRouteRisk, int TotalRouteCapacity)
    {
        public static readonly TradeActivitySnapshot Empty = new(0, 0, 0);
    }

    private readonly record struct InterventionCarryoverEffect(
        int BanditDelta,
        int RouteDelta,
        int DisorderDelta,
        int RouteShieldingDelta,
        int SuppressionDemandDelta,
        int SuppressionReliefDelta,
        int RetaliationRiskDelta,
        int BlackRoutePressureDelta,
        int CoercionRiskDelta);
}
