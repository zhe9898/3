using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed partial class PublicLifeAndRumorModule : ModuleRunner<PublicLifeAndRumorState>
{
    private static void RefreshSettlementPulse(

        SettlementPublicLifeState target,

        GameDate currentDate,

        SimulationXun currentXun,

        SettlementSnapshot settlement,

        IReadOnlyDictionary<SettlementId, PopulationSettlementSnapshot> populationBySettlement,

        IReadOnlyDictionary<SettlementId, MarketSnapshot> marketsBySettlement,

        IReadOnlyDictionary<SettlementId, List<ClanTradeRouteSnapshot>> routesBySettlement,

        IReadOnlyDictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement,

        IReadOnlyDictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement,

        IReadOnlyDictionary<SettlementId, List<ClanSnapshot>> clansBySettlement,

        IReadOnlyDictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan)

    {

        populationBySettlement.TryGetValue(settlement.Id, out PopulationSettlementSnapshot? population);

        marketsBySettlement.TryGetValue(settlement.Id, out MarketSnapshot? market);

        routesBySettlement.TryGetValue(settlement.Id, out List<ClanTradeRouteSnapshot>? routes);

        disorderBySettlement.TryGetValue(settlement.Id, out SettlementDisorderSnapshot? disorder);

        jurisdictionsBySettlement.TryGetValue(settlement.Id, out JurisdictionAuthoritySnapshot? jurisdiction);

        clansBySettlement.TryGetValue(settlement.Id, out List<ClanSnapshot>? clans);


        routes ??= [];

        clans ??= [];


        int averagePrestige = clans.Count == 0 ? 0 : (int)Math.Round(clans.Average(static clan => clan.Prestige));

        int averageNarrativePressure = ComputeAverageNarrativePressure(clans, narrativesByClan);

        int tierStreetBonus = settlement.Tier switch

        {

            SettlementTier.VillageCluster => 12,

            SettlementTier.MarketTown => 8,

            SettlementTier.CountySeat => 6,

            SettlementTier.PrefectureSeat => 7,

            _ => 6,

        };

        int tierMarketBonus = settlement.Tier switch

        {

            SettlementTier.MarketTown => 14,

            SettlementTier.CountySeat => 10,

            SettlementTier.PrefectureSeat => 12,

            _ => 4,

        };

        int tierNoticeBonus = settlement.Tier switch

        {

            SettlementTier.CountySeat => 18,

            SettlementTier.PrefectureSeat => 26,

            SettlementTier.MarketTown => 9,

            _ => 4,

        };

        int tierDispatchBonus = settlement.Tier switch

        {

            SettlementTier.PrefectureSeat => 26,

            SettlementTier.CountySeat => 14,

            SettlementTier.MarketTown => 8,

            _ => 4,

        };


        int averageRouteRisk = routes.Count == 0 ? 0 : (int)Math.Round(routes.Average(static route => route.Risk));

        int activeRouteCount = routes.Count(static route => route.IsActive);

        int marketDemand = market?.Demand ?? 0;

        int marketRisk = market?.LocalRisk ?? 0;

        int commonerDistress = population?.CommonerDistress ?? 0;

        int migrationPressure = population?.MigrationPressure ?? 0;

        int disorderPressure = disorder?.DisorderPressure ?? 0;

        int routePressure = disorder?.RoutePressure ?? 0;

        int banditThreat = disorder?.BanditThreat ?? 0;

        int petitionPressure = jurisdiction?.PetitionPressure ?? 0;

        int petitionBacklog = jurisdiction?.PetitionBacklog ?? 0;

        int leverage = jurisdiction?.JurisdictionLeverage ?? 0;

        int authorityTier = jurisdiction?.AuthorityTier ?? 0;


        int routeReportLag = Math.Clamp(

            6

            + (activeRouteCount * 4)

            + (averageRouteRisk / 2)

            + (routePressure / 2)

            + (banditThreat / 4)

            + (settlement.Tier == SettlementTier.VillageCluster ? 10 : 0)

            - (settlement.Security / 5),

            0,

            100);

        int streetTalkHeat = Math.Clamp(

            18

            + tierStreetBonus

            + (commonerDistress / 2)

            + (migrationPressure / 3)

            + (averageNarrativePressure / 2)

            + (disorderPressure / 3)

            - (leverage / 6),

            0,

            100);

        int marketBuzz = Math.Clamp(

            10

            + tierMarketBonus

            + (settlement.Prosperity / 3)

            + (marketDemand / 2)

            + (activeRouteCount * 5)

            - (marketRisk / 3)

            - (routePressure / 4),

            0,

            100);

        int noticeVisibility = Math.Clamp(

            6

            + tierNoticeBonus

            + (authorityTier * 7)

            + (leverage / 2)

            + (petitionBacklog / 2)

            - (routeReportLag / 3),

            0,

            100);

        int prefectureDispatchPressure = Math.Clamp(

            tierDispatchBonus

            + petitionPressure

            + (petitionBacklog / 2)

            + (routeReportLag / 2)

            + (averageNarrativePressure / 4),

            0,

            100);

        int publicLegitimacy = Math.Clamp(

            36

            + (settlement.Security / 3)

            + (settlement.Prosperity / 4)

            + (leverage / 5)

            + (averagePrestige / 7)

            - (commonerDistress / 2)

            - (disorderPressure / 3)

            - (streetTalkHeat / 6),

            0,

            100);


        ApplyXunPulseAdjustment(

            currentXun,

            settlement.Tier,

            ref streetTalkHeat,

            ref marketBuzz,

            ref noticeVisibility,

            ref routeReportLag,

            ref prefectureDispatchPressure,

            ref publicLegitimacy);

        ApplyOfficeSurfaceXunAdjustment(

            currentXun,

            jurisdiction,

            ref streetTalkHeat,

            ref noticeVisibility,

            ref routeReportLag,

            ref prefectureDispatchPressure,

            ref publicLegitimacy);


        target.SettlementName = settlement.Name;

        target.SettlementTier = settlement.Tier;

        target.MonthlyCadenceCode = BuildMonthlyCadenceCode(currentDate, settlement, activeRouteCount, petitionBacklog, noticeVisibility);

        target.MonthlyCadenceLabel = BuildMonthlyCadenceLabel(target.MonthlyCadenceCode, settlement.Tier);

        VenueDescriptor dominantVenue = BuildDominantVenue(settlement, market, routes, streetTalkHeat, marketBuzz, noticeVisibility, routeReportLag, target.MonthlyCadenceCode);

        target.NodeLabel = BuildNodeLabel(settlement);

        target.DominantVenueCode = dominantVenue.Code;

        target.DominantVenueLabel = dominantVenue.Label;

        target.CrowdMixLabel = BuildCrowdMixLabel(settlement, activeRouteCount, streetTalkHeat, marketBuzz, noticeVisibility, routeReportLag, dominantVenue.Code);

        target.StreetTalkHeat = streetTalkHeat;

        target.MarketBuzz = marketBuzz;

        target.NoticeVisibility = noticeVisibility;

        target.RoadReportLag = routeReportLag;

        target.PrefectureDispatchPressure = prefectureDispatchPressure;

        target.PublicLegitimacy = publicLegitimacy;

        target.DocumentaryWeight = BuildDocumentaryWeight(authorityTier, leverage, noticeVisibility, prefectureDispatchPressure, streetTalkHeat, routeReportLag);

        target.VerificationCost = BuildVerificationCost(settlement, activeRouteCount, averageRouteRisk, routeReportLag, noticeVisibility, banditThreat);

        target.MarketRumorFlow = BuildMarketRumorFlow(activeRouteCount, streetTalkHeat, marketBuzz, averageNarrativePressure, noticeVisibility, routePressure);

        target.CourierRisk = BuildCourierRisk(activeRouteCount, averageRouteRisk, routeReportLag, routePressure, banditThreat, settlement.Security);

        target.OfficialNoticeLine = BuildOfficialNoticeLine(target);

        target.StreetTalkLine = BuildStreetTalkLine(target);

        target.RoadReportLine = BuildRoadReportLine(target, routes);

        target.PrefectureDispatchLine = BuildPrefectureDispatchLine(target);

        target.ContentionSummary = BuildContentionSummary(target);

        target.PublicSummary = BuildPublicSummary(target);

        target.RouteReportSummary = BuildRouteReportSummary(target, routes, jurisdiction);

        target.CadenceSummary = BuildCadenceSummary(target);

        target.ChannelSummary = BuildChannelSummary(target);

        target.LastPublicTrace = $"{target.NodeLabel}{target.CadenceSummary}{target.PublicSummary}{target.ChannelSummary}{target.ContentionSummary}{target.RouteReportSummary}";

    }


    private static void ApplyXunPulseAdjustment(

        SimulationXun currentXun,

        SettlementTier tier,

        ref int streetTalkHeat,

        ref int marketBuzz,

        ref int noticeVisibility,

        ref int routeReportLag,

        ref int prefectureDispatchPressure,

        ref int publicLegitimacy)

    {

        switch (currentXun)

        {

            case SimulationXun.Shangxun:

                marketBuzz = Math.Clamp(marketBuzz + 4, 0, 100);

                routeReportLag = Math.Clamp(routeReportLag - 2, 0, 100);

                break;

            case SimulationXun.Zhongxun:

                streetTalkHeat = Math.Clamp(streetTalkHeat + 4, 0, 100);

                marketBuzz = Math.Clamp(marketBuzz + 2, 0, 100);

                publicLegitimacy = Math.Clamp(publicLegitimacy - 1, 0, 100);

                break;

            case SimulationXun.Xiaxun:

                noticeVisibility = Math.Clamp(noticeVisibility + 5, 0, 100);

                prefectureDispatchPressure = Math.Clamp(prefectureDispatchPressure + 4, 0, 100);

                routeReportLag = Math.Clamp(routeReportLag + 3, 0, 100);

                publicLegitimacy = Math.Clamp(publicLegitimacy - (tier == SettlementTier.CountySeat || tier == SettlementTier.PrefectureSeat ? 2 : 1), 0, 100);

                break;

        }

    }


    private static void ApplyOfficeSurfaceXunAdjustment(

        SimulationXun currentXun,

        JurisdictionAuthoritySnapshot? jurisdiction,

        ref int streetTalkHeat,

        ref int noticeVisibility,

        ref int routeReportLag,

        ref int prefectureDispatchPressure,

        ref int publicLegitimacy)

    {

        if (jurisdiction is null)

        {

            return;

        }


        int yamenTemperature = ComputeYamenTemperature(jurisdiction);

        int paperQueueDrag = ComputePaperQueueDrag(jurisdiction);

        if (yamenTemperature < 18 && paperQueueDrag < 18)

        {

            return;

        }


        switch (currentXun)

        {

            case SimulationXun.Shangxun:

                noticeVisibility = Math.Clamp(noticeVisibility + Math.Max(1, yamenTemperature / 24), 0, 100);

                break;

            case SimulationXun.Zhongxun:

                streetTalkHeat = Math.Clamp(streetTalkHeat + Math.Max(1, paperQueueDrag / 18), 0, 100);

                noticeVisibility = Math.Clamp(noticeVisibility + Math.Max(1, yamenTemperature / 32), 0, 100);

                publicLegitimacy = Math.Clamp(publicLegitimacy - Math.Max(1, paperQueueDrag / 30), 0, 100);

                break;

            case SimulationXun.Xiaxun:

                noticeVisibility = Math.Clamp(noticeVisibility + Math.Max(1, yamenTemperature / 16), 0, 100);

                prefectureDispatchPressure = Math.Clamp(

                    prefectureDispatchPressure + Math.Max(1, yamenTemperature / 18) + Math.Max(1, paperQueueDrag / 20),

                    0,

                    100);

                routeReportLag = Math.Clamp(routeReportLag + Math.Max(1, paperQueueDrag / 18), 0, 100);

                publicLegitimacy = Math.Clamp(publicLegitimacy - Math.Max(1, paperQueueDrag / 22), 0, 100);

                break;

        }

    }


    private static int ComputeYamenTemperature(JurisdictionAuthoritySnapshot jurisdiction)

    {

        int taskTierWeight = jurisdiction.AdministrativeTaskTier switch

        {

            "prefecture" => 10,

            "county" => 7,

            "district" => 4,

            _ => 0,

        };


        return Math.Clamp(

            (jurisdiction.AdministrativeTaskLoad / 2)

            + (jurisdiction.ClerkDependence / 2)

            + (jurisdiction.PetitionPressure / 8)

            + taskTierWeight

            - (jurisdiction.JurisdictionLeverage / 12),

            0,

            100);

    }


    private static int ComputePaperQueueDrag(JurisdictionAuthoritySnapshot jurisdiction)

    {

        return Math.Clamp(

            (jurisdiction.AdministrativeTaskLoad / 2)

            + (jurisdiction.ClerkDependence / 2)

            + (jurisdiction.PetitionBacklog / 6)

            - (jurisdiction.JurisdictionLeverage / 10),

            0,

            100);

    }


}
