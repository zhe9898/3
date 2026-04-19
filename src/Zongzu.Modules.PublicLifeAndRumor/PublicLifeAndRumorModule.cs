using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed class PublicLifeAndRumorModule : ModuleRunner<PublicLifeAndRumorState>
{
    private readonly record struct VenueDescriptor(string Code, string Label);

    private static readonly string[] EventNames =
    [
        PublicLifeAndRumorEventNames.StreetTalkSurged,
        PublicLifeAndRumorEventNames.CountyGateCrowded,
        PublicLifeAndRumorEventNames.MarketBuzzRaised,
        PublicLifeAndRumorEventNames.RoadReportDelayed,
        PublicLifeAndRumorEventNames.PrefectureDispatchPressed,
    ];

    public override string ModuleKey => KnownModuleKeys.PublicLifeAndRumor;

    public override int ModuleSchemaVersion => 4;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 760;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override PublicLifeAndRumorState CreateInitialState()
    {
        return new PublicLifeAndRumorState();
    }

    public override void RegisterQueries(PublicLifeAndRumorState state, QueryRegistry queries)
    {
        queries.Register<IPublicLifeAndRumorQueries>(new PublicLifeAndRumorQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        IWorldSettlementsQueries worldQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IReadOnlyList<SettlementSnapshot> settlements = worldQueries.GetSettlements();

        Dictionary<SettlementId, PopulationSettlementSnapshot> populationBySettlement = BuildPopulationBySettlement(scope);
        Dictionary<SettlementId, MarketSnapshot> marketsBySettlement = BuildMarketsBySettlement(scope);
        Dictionary<SettlementId, List<TradeRouteSnapshot>> routesBySettlement = BuildRoutesBySettlement(scope);
        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = BuildDisorderBySettlement(scope);
        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = BuildJurisdictionsBySettlement(scope);
        Dictionary<SettlementId, List<ClanSnapshot>> clansBySettlement = BuildClansBySettlement(scope);
        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = BuildNarrativesByClan(scope);

        Dictionary<SettlementId, SettlementPublicLifeState> stateBySettlement = scope.State.Settlements
            .ToDictionary(static entry => entry.SettlementId, static entry => entry);

        foreach (SettlementSnapshot settlement in settlements.OrderBy(static entry => entry.Id.Value))
        {
            bool isNew = !stateBySettlement.TryGetValue(settlement.Id, out SettlementPublicLifeState? publicLife);
            publicLife ??= new SettlementPublicLifeState
            {
                SettlementId = settlement.Id,
            };

            SettlementPublicLifeState previous = Clone(publicLife);
            RefreshSettlementPulse(
                publicLife,
                scope.Context.CurrentDate,
                settlement,
                populationBySettlement,
                marketsBySettlement,
                routesBySettlement,
                disorderBySettlement,
                jurisdictionsBySettlement,
                clansBySettlement,
                narrativesByClan);

            if (isNew)
            {
                scope.State.Settlements.Add(publicLife);
                stateBySettlement.Add(publicLife.SettlementId, publicLife);
            }

            if (!ShouldReport(previous, publicLife, isNew))
            {
                continue;
            }

            scope.RecordDiff(publicLife.LastPublicTrace, publicLife.SettlementId.Value.ToString());

            string? eventType = DeterminePrimaryEvent(publicLife);
            if (eventType is null)
            {
                continue;
            }

            scope.Emit(
                eventType,
                BuildEventSummary(publicLife, eventType),
                publicLife.SettlementId.Value.ToString());
        }
    }

    private static void RefreshSettlementPulse(
        SettlementPublicLifeState target,
        GameDate currentDate,
        SettlementSnapshot settlement,
        IReadOnlyDictionary<SettlementId, PopulationSettlementSnapshot> populationBySettlement,
        IReadOnlyDictionary<SettlementId, MarketSnapshot> marketsBySettlement,
        IReadOnlyDictionary<SettlementId, List<TradeRouteSnapshot>> routesBySettlement,
        IReadOnlyDictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement,
        IReadOnlyDictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement,
        IReadOnlyDictionary<SettlementId, List<ClanSnapshot>> clansBySettlement,
        IReadOnlyDictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan)
    {
        populationBySettlement.TryGetValue(settlement.Id, out PopulationSettlementSnapshot? population);
        marketsBySettlement.TryGetValue(settlement.Id, out MarketSnapshot? market);
        routesBySettlement.TryGetValue(settlement.Id, out List<TradeRouteSnapshot>? routes);
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

    private static Dictionary<SettlementId, PopulationSettlementSnapshot> BuildPopulationBySettlement(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.PopulationAndHouseholds))
        {
            return new Dictionary<SettlementId, PopulationSettlementSnapshot>();
        }

        return scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>()
            .GetSettlements()
            .ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);
    }

    private static Dictionary<SettlementId, MarketSnapshot> BuildMarketsBySettlement(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry))
        {
            return new Dictionary<SettlementId, MarketSnapshot>();
        }

        return scope.GetRequiredQuery<ITradeAndIndustryQueries>()
            .GetMarkets()
            .ToDictionary(static market => market.SettlementId, static market => market);
    }

    private static Dictionary<SettlementId, List<TradeRouteSnapshot>> BuildRoutesBySettlement(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        Dictionary<SettlementId, List<TradeRouteSnapshot>> routesBySettlement = new();
        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry))
        {
            return routesBySettlement;
        }

        ITradeAndIndustryQueries tradeQueries = scope.GetRequiredQuery<ITradeAndIndustryQueries>();
        foreach (ClanTradeSnapshot trade in tradeQueries.GetClanTrades().OrderBy(static entry => entry.ClanId.Value))
        {
            foreach (TradeRouteSnapshot route in tradeQueries.GetRoutesForClan(trade.ClanId).OrderBy(static entry => entry.RouteId))
            {
                if (!routesBySettlement.TryGetValue(route.SettlementId, out List<TradeRouteSnapshot>? routes))
                {
                    routes = [];
                    routesBySettlement.Add(route.SettlementId, routes);
                }

                routes.Add(route);
            }
        }

        return routesBySettlement;
    }

    private static Dictionary<SettlementId, SettlementDisorderSnapshot> BuildDisorderBySettlement(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry))
        {
            return new Dictionary<SettlementId, SettlementDisorderSnapshot>();
        }

        return scope.GetRequiredQuery<IOrderAndBanditryQueries>()
            .GetSettlementDisorder()
            .ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);
    }

    private static Dictionary<SettlementId, JurisdictionAuthoritySnapshot> BuildJurisdictionsBySettlement(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            return new Dictionary<SettlementId, JurisdictionAuthoritySnapshot>();
        }

        return scope.GetRequiredQuery<IOfficeAndCareerQueries>()
            .GetJurisdictions()
            .ToDictionary(static jurisdiction => jurisdiction.SettlementId, static jurisdiction => jurisdiction);
    }

    private static Dictionary<SettlementId, List<ClanSnapshot>> BuildClansBySettlement(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        Dictionary<SettlementId, List<ClanSnapshot>> clansBySettlement = new();
        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.FamilyCore))
        {
            return clansBySettlement;
        }

        foreach (ClanSnapshot clan in scope.GetRequiredQuery<IFamilyCoreQueries>().GetClans().OrderBy(static entry => entry.Id.Value))
        {
            if (!clansBySettlement.TryGetValue(clan.HomeSettlementId, out List<ClanSnapshot>? clans))
            {
                clans = [];
                clansBySettlement.Add(clan.HomeSettlementId, clans);
            }

            clans.Add(clan);
        }

        return clansBySettlement;
    }

    private static Dictionary<ClanId, ClanNarrativeSnapshot> BuildNarrativesByClan(ModuleExecutionScope<PublicLifeAndRumorState> scope)
    {
        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.SocialMemoryAndRelations))
        {
            return new Dictionary<ClanId, ClanNarrativeSnapshot>();
        }

        return scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>()
            .GetClanNarratives()
            .ToDictionary(static narrative => narrative.ClanId, static narrative => narrative);
    }

    private static int ComputeAverageNarrativePressure(
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyDictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan)
    {
        if (clans.Count == 0)
        {
            return 0;
        }

        int total = 0;
        foreach (ClanSnapshot clan in clans)
        {
            if (!narrativesByClan.TryGetValue(clan.Id, out ClanNarrativeSnapshot? narrative))
            {
                continue;
            }

            total += narrative.GrudgePressure;
            total += narrative.FearPressure;
            total += narrative.ShamePressure / 2;
        }

        return total / clans.Count;
    }

    private static bool ShouldReport(SettlementPublicLifeState previous, SettlementPublicLifeState current, bool isNew)
    {
        return isNew
            || Math.Abs(previous.StreetTalkHeat - current.StreetTalkHeat) >= 5
            || Math.Abs(previous.MarketBuzz - current.MarketBuzz) >= 5
            || Math.Abs(previous.NoticeVisibility - current.NoticeVisibility) >= 5
            || Math.Abs(previous.RoadReportLag - current.RoadReportLag) >= 5
            || Math.Abs(previous.PrefectureDispatchPressure - current.PrefectureDispatchPressure) >= 5
            || Math.Abs(previous.PublicLegitimacy - current.PublicLegitimacy) >= 5
            || Math.Abs(previous.DocumentaryWeight - current.DocumentaryWeight) >= 5
            || Math.Abs(previous.VerificationCost - current.VerificationCost) >= 5
            || Math.Abs(previous.MarketRumorFlow - current.MarketRumorFlow) >= 5
            || Math.Abs(previous.CourierRisk - current.CourierRisk) >= 5
            || !string.Equals(previous.DominantVenueCode, current.DominantVenueCode, StringComparison.Ordinal)
            || !string.Equals(previous.MonthlyCadenceCode, current.MonthlyCadenceCode, StringComparison.Ordinal)
            || !string.Equals(previous.MonthlyCadenceLabel, current.MonthlyCadenceLabel, StringComparison.Ordinal)
            || !string.Equals(previous.CrowdMixLabel, current.CrowdMixLabel, StringComparison.Ordinal)
            || !string.Equals(previous.OfficialNoticeLine, current.OfficialNoticeLine, StringComparison.Ordinal)
            || !string.Equals(previous.StreetTalkLine, current.StreetTalkLine, StringComparison.Ordinal)
            || !string.Equals(previous.RoadReportLine, current.RoadReportLine, StringComparison.Ordinal)
            || !string.Equals(previous.PrefectureDispatchLine, current.PrefectureDispatchLine, StringComparison.Ordinal)
            || !string.Equals(previous.ContentionSummary, current.ContentionSummary, StringComparison.Ordinal)
            || !string.Equals(previous.CadenceSummary, current.CadenceSummary, StringComparison.Ordinal)
            || !string.Equals(previous.PublicSummary, current.PublicSummary, StringComparison.Ordinal)
            || !string.Equals(previous.ChannelSummary, current.ChannelSummary, StringComparison.Ordinal)
            || !string.Equals(previous.RouteReportSummary, current.RouteReportSummary, StringComparison.Ordinal);
    }

    private static string? DeterminePrimaryEvent(SettlementPublicLifeState state)
    {
        if (state.PrefectureDispatchPressure >= 62)
        {
            return PublicLifeAndRumorEventNames.PrefectureDispatchPressed;
        }

        if (state.RoadReportLag >= 56)
        {
            return PublicLifeAndRumorEventNames.RoadReportDelayed;
        }

        if (state.NoticeVisibility >= 58)
        {
            return PublicLifeAndRumorEventNames.CountyGateCrowded;
        }

        if (state.MarketBuzz >= 60)
        {
            return PublicLifeAndRumorEventNames.MarketBuzzRaised;
        }

        if (state.StreetTalkHeat >= 55)
        {
            return PublicLifeAndRumorEventNames.StreetTalkSurged;
        }

        return null;
    }

    private static string BuildEventSummary(SettlementPublicLifeState state, string eventType)
    {
        return eventType switch
        {
            PublicLifeAndRumorEventNames.PrefectureDispatchPressed => $"{state.NodeLabel}{state.MonthlyCadenceLabel}正当眼前，{state.PrefectureDispatchLine}{state.OfficialNoticeLine}{state.ContentionSummary}",
            PublicLifeAndRumorEventNames.RoadReportDelayed => $"{state.NodeLabel}{state.MonthlyCadenceLabel}里，{state.RoadReportLine}{state.ContentionSummary}",
            PublicLifeAndRumorEventNames.CountyGateCrowded => $"{state.NodeLabel}{state.MonthlyCadenceLabel}之际，{state.OfficialNoticeLine}{state.StreetTalkLine}{state.ContentionSummary}",
            PublicLifeAndRumorEventNames.MarketBuzzRaised => $"{state.NodeLabel}{state.MonthlyCadenceLabel}正盛，{state.StreetTalkLine}{state.RoadReportLine}{state.ContentionSummary}",
            _ => $"{state.NodeLabel}{state.MonthlyCadenceLabel}里，{state.StreetTalkLine}{state.OfficialNoticeLine}{state.ContentionSummary}",
        };
    }

    private static string BuildMonthlyCadenceCode(
        GameDate currentDate,
        SettlementSnapshot settlement,
        int activeRouteCount,
        int petitionBacklog,
        int noticeVisibility)
    {
        return currentDate.Month switch
        {
            1 => settlement.Tier is SettlementTier.CountySeat or SettlementTier.PrefectureSeat
                ? "new-year-notice"
                : "new-year-gathering",
            2 or 3 => "spring-fair",
            4 or 5 => activeRouteCount > 0 ? "river-road-bustle" : "market-season",
            6 or 7 => "teahouse-rumor",
            8 or 9 => "autumn-market",
            10 => activeRouteCount > 0 ? "grain-route-busy" : "autumn-market",
            _ => petitionBacklog >= 20 || noticeVisibility >= 50 ? "year-end-docket" : "winter-posting",
        };
    }

    private static string BuildMonthlyCadenceLabel(string cadenceCode, SettlementTier tier)
    {
        return cadenceCode switch
        {
            "new-year-notice" => tier == SettlementTier.PrefectureSeat ? "岁首晓牒" : "岁首晓榜",
            "new-year-gathering" => "岁首乡集",
            "spring-fair" => "春社集日",
            "river-road-bustle" => "春汛行旅",
            "market-season" => "时集正旺",
            "teahouse-rumor" => "茶肆话头",
            "autumn-market" => "秋成集期",
            "grain-route-busy" => "秋粮在路",
            "year-end-docket" => "岁末催科",
            _ => "冬月榜示",
        };
    }

    private static string BuildCrowdMixLabel(
        SettlementSnapshot settlement,
        int activeRouteCount,
        int streetTalkHeat,
        int marketBuzz,
        int noticeVisibility,
        int routeReportLag,
        string dominantVenueCode)
    {
        if (string.Equals(dominantVenueCode, "temple-fair", StringComparison.Ordinal))
        {
            return "多见烧香人、看场乡民、卖食小贩与说书走客";
        }

        if (string.Equals(dominantVenueCode, "wharf-ferry", StringComparison.Ordinal))
        {
            return "多是舟户、脚夫、牙人和守埠歇脚客";
        }

        if (string.Equals(dominantVenueCode, "teahouse-inn", StringComparison.Ordinal))
        {
            return "多见歇脚行旅、茶客、听事闲汉与捎报人";
        }

        if (noticeVisibility >= marketBuzz && noticeVisibility >= streetTalkHeat)
        {
            return settlement.Tier switch
            {
                SettlementTier.PrefectureSeat => "多是承牒差人、书手与候报客商",
                SettlementTier.CountySeat => "多是递状人户、候榜士子与书手差人",
                _ => "多是看榜乡民与候事人户",
            };
        }

        if (marketBuzz >= streetTalkHeat)
        {
            return activeRouteCount > 0
                ? "多见客商、小贩、脚夫与歇脚行旅"
                : "多见镇上店户、小贩与说合人";
        }

        if (routeReportLag >= 48 && activeRouteCount > 0)
        {
            return "多是舟户、脚夫、捎报人和守埠歇脚客";
        }

        return settlement.Tier == SettlementTier.VillageCluster
            ? "多是乡民、佃户、亲邻与媒妁"
            : "多是闲谈看热闹的乡民与过路客";
    }

        private static string BuildNodeLabel(SettlementSnapshot settlement)
    {
        return settlement.Tier switch
        {
            SettlementTier.VillageCluster => $"{settlement.Name}乡里",
            SettlementTier.MarketTown => $"{settlement.Name}镇市",
            SettlementTier.PrefectureSeat => $"{settlement.Name}州门",
            _ => $"{settlement.Name}县门",
        };
    }

    private static VenueDescriptor BuildDominantVenue(
        SettlementSnapshot settlement,
        MarketSnapshot? market,
        IReadOnlyList<TradeRouteSnapshot> routes,
        int streetTalkHeat,
        int marketBuzz,
        int noticeVisibility,
        int routeReportLag,
        string cadenceCode)
    {
        if (string.Equals(cadenceCode, "spring-fair", StringComparison.Ordinal)
            || string.Equals(cadenceCode, "new-year-gathering", StringComparison.Ordinal))
        {
            return new VenueDescriptor("temple-fair", "庙前社场");
        }

        if (noticeVisibility >= marketBuzz && noticeVisibility >= streetTalkHeat)
        {
            return settlement.Tier == SettlementTier.PrefectureSeat
                ? new VenueDescriptor("prefecture-gate", "州门牒房")
                : new VenueDescriptor("county-gate", "县门榜下");
        }

        if (marketBuzz >= streetTalkHeat)
        {
            return market is null
                ? new VenueDescriptor("market-street", settlement.Tier == SettlementTier.MarketTown ? "镇市街口" : "城中行市")
                : new VenueDescriptor("market-street", market.MarketName);
        }

        if (routeReportLag >= 48 && routes.Count > 0)
        {
            return GuessRouteVenue(routes[0].RouteName);
        }

        return settlement.Tier == SettlementTier.VillageCluster
            ? new VenueDescriptor("village-shrine", "乡里社前")
            : new VenueDescriptor("teahouse-inn", "街口茶肆");
    }

    private static string BuildPublicSummary(SettlementPublicLifeState state)
    {
        return $"今月{state.DominantVenueLabel}最见动静：街谈{state.StreetTalkHeat}，市喧{state.MarketBuzz}，榜示{state.NoticeVisibility}，众情向背{state.PublicLegitimacy}。";
    }

    private static string BuildCadenceSummary(SettlementPublicLifeState state)
    {
        return $"值{state.MonthlyCadenceLabel}，{state.DominantVenueLabel}{state.CrowdMixLabel}。";
    }

    private static string BuildChannelSummary(SettlementPublicLifeState state)
    {
        return $"此处榜示分量{state.DocumentaryWeight}，市语流势{state.MarketRumorFlow}，查验周折{state.VerificationCost}，递报险数{state.CourierRisk}。";
    }

    private static string BuildOfficialNoticeLine(SettlementPublicLifeState state)
    {
        if (state.PrefectureDispatchPressure >= 70 && state.NoticeVisibility >= 55)
        {
            return "榜下只说州牒已到，里甲行户皆不得推缓。";
        }

        if (state.DocumentaryWeight >= 60 && state.NoticeVisibility >= 52)
        {
            return "榜文口气甚稳，都说县门已把轻重先后讲明。";
        }

        if (state.NoticeVisibility >= 40)
        {
            return "榜文已经贴出，只说门前词状与差发都按次缓急而行。";
        }

        return "榜文零星传示，门前说法尚未压住众口。";
    }

    private static string BuildStreetTalkLine(SettlementPublicLifeState state)
    {
        if (state.StreetTalkHeat >= 68 && state.MarketRumorFlow >= 58)
        {
            return $"街口都说{state.DominantVenueLabel}另有实情，未必尽如榜上那般轻松。";
        }

        if (state.StreetTalkHeat >= 56)
        {
            return $"街口闲话已多，众人都在拿{state.DominantVenueLabel}的耳闻去量榜上说法。";
        }

        if (state.MarketRumorFlow >= 50)
        {
            return $"市面风声先走一步，{state.DominantVenueLabel}的人大多凭口耳探轻重。";
        }

        return $"街谈尚浅，{state.DominantVenueLabel}的人多半仍在等确实说法。";
    }

    private static string BuildRoadReportLine(SettlementPublicLifeState state, IReadOnlyList<TradeRouteSnapshot> routes)
    {
        if (routes.Count == 0)
        {
            return state.RoadReportLag >= 45
                ? "此地外路稀少，乡里多半只能凭迟来的转口消息揣度虚实。"
                : "此地外路不繁，消息多在近处缓缓传开。";
        }

        TradeRouteSnapshot leadRoute = routes
            .OrderByDescending(static route => route.Risk)
            .ThenByDescending(static route => route.Capacity)
            .First();

        if (state.RoadReportLag >= 56 || state.CourierRisk >= 56)
        {
            return $"由{leadRoute.RouteName}传来的脚信前后不齐，沿路各站说法也不尽相同。";
        }

        if (state.RoadReportLag >= 40)
        {
            return $"由{leadRoute.RouteName}递来的消息略慢半拍，市上往往先有猜度。";
        }

        return $"由{leadRoute.RouteName}递来的消息尚算齐整，还能和门前榜示相互印证。";
    }

    private static string BuildPrefectureDispatchLine(SettlementPublicLifeState state)
    {
        if (state.PrefectureDispatchPressure >= 72)
        {
            return "州牒措辞严急，只认催科与差发，不大理会乡里叫苦。";
        }

        if (state.PrefectureDispatchPressure >= 55)
        {
            return "州牒已经压下来，县里只得在催办与缓冲之间来回斟酌。";
        }

        if (state.PrefectureDispatchPressure >= 36)
        {
            return "州里文移已有催意，但县门还留着几分回旋余地。";
        }

        return "州里此月催迫未甚，县门尚能因地轻重缓办。";
    }

    private static string BuildContentionSummary(SettlementPublicLifeState state)
    {
        if (state.RoadReportLag >= 55 || state.CourierRisk >= 55)
        {
            return "路上传来的话前后不齐，榜文、街谈与脚信多半各执一词。";
        }

        if (state.PrefectureDispatchPressure >= 62 && state.DocumentaryWeight < 50)
        {
            return "上头催得甚急，门前说法却压不住众口，乡里都觉得还有回旋。";
        }

        if (state.MarketRumorFlow >= state.DocumentaryWeight + 12 && state.StreetTalkHeat >= state.NoticeVisibility)
        {
            return "市语快过榜文，镇市与茶肆里的人多凭耳闻，不肯尽信门前晓谕。";
        }

        if (state.DocumentaryWeight >= state.MarketRumorFlow + 12 && state.NoticeVisibility >= state.StreetTalkHeat)
        {
            return "榜文压过街谈，众人纵有狐疑，也多半先看门前印信。";
        }

        return "榜文、街谈、路报彼此牵制，众人多在观望哪一路说法更真。";
    }

    private static string BuildRouteReportSummary(
        SettlementPublicLifeState state,
        IReadOnlyList<TradeRouteSnapshot> routes,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (routes.Count == 0)
        {
            return state.PrefectureDispatchPressure >= 50
                ? $"州牒催迫已至{state.PrefectureDispatchPressure}，虽无大路过境，里甲仍觉差科发急。"
                : $"此地暂无大路牵扰，路报迟滞仅至{state.RoadReportLag}。";
        }

        TradeRouteSnapshot leadRoute = routes
            .OrderByDescending(static route => route.Risk)
            .ThenByDescending(static route => route.Capacity)
            .First();

        string courierSummary = jurisdiction is null || string.IsNullOrWhiteSpace(jurisdiction.LastAdministrativeTrace)
            ? "文移尚未全断"
            : "牒报与词状并行催并";

        return $"{leadRoute.RouteName}眼下迟滞{state.RoadReportLag}，{courierSummary}。";
    }

    private static VenueDescriptor GuessRouteVenue(string routeName)
    {
        if (routeName.Contains("河", StringComparison.Ordinal)
            || routeName.Contains("港", StringComparison.Ordinal)
            || routeName.Contains("埠", StringComparison.Ordinal)
            || routeName.Contains("津", StringComparison.Ordinal)
            || routeName.Contains("渡", StringComparison.Ordinal))
        {
            return new VenueDescriptor("wharf-ferry", "河埠歇脚棚");
        }

        if (routeName.Contains("山", StringComparison.Ordinal)
            || routeName.Contains("岭", StringComparison.Ordinal)
            || routeName.Contains("关", StringComparison.Ordinal)
            || routeName.Contains("隘", StringComparison.Ordinal))
        {
            return new VenueDescriptor("mountain-pass", "关路店舍");
        }

        return new VenueDescriptor("teahouse-inn", "驿路茶棚");
    }

    private static int BuildDocumentaryWeight(
        int authorityTier,
        int leverage,
        int noticeVisibility,
        int prefectureDispatchPressure,
        int streetTalkHeat,
        int routeReportLag)
    {
        return Math.Clamp(
            12
            + (authorityTier * 8)
            + (leverage / 3)
            + (noticeVisibility / 2)
            + (prefectureDispatchPressure / 4)
            - (streetTalkHeat / 5)
            - (routeReportLag / 6),
            0,
            100);
    }

    private static int BuildVerificationCost(
        SettlementSnapshot settlement,
        int activeRouteCount,
        int averageRouteRisk,
        int routeReportLag,
        int noticeVisibility,
        int banditThreat)
    {
        return Math.Clamp(
            8
            + (activeRouteCount * 3)
            + (averageRouteRisk / 3)
            + (routeReportLag / 2)
            + (banditThreat / 5)
            + (settlement.Tier == SettlementTier.VillageCluster ? 10 : settlement.Tier == SettlementTier.MarketTown ? 4 : 0)
            - (noticeVisibility / 5),
            0,
            100);
    }

    private static int BuildMarketRumorFlow(
        int activeRouteCount,
        int streetTalkHeat,
        int marketBuzz,
        int averageNarrativePressure,
        int noticeVisibility,
        int routePressure)
    {
        return Math.Clamp(
            10
            + (activeRouteCount * 4)
            + (streetTalkHeat / 2)
            + (marketBuzz / 2)
            + (averageNarrativePressure / 4)
            + (routePressure / 5)
            - (noticeVisibility / 6),
            0,
            100);
    }

    private static int BuildCourierRisk(
        int activeRouteCount,
        int averageRouteRisk,
        int routeReportLag,
        int routePressure,
        int banditThreat,
        int settlementSecurity)
    {
        return Math.Clamp(
            6
            + (activeRouteCount * 3)
            + (averageRouteRisk / 3)
            + (routeReportLag / 2)
            + (routePressure / 3)
            + (banditThreat / 4)
            - (settlementSecurity / 6),
            0,
            100);
    }
    private static SettlementPublicLifeState Clone(SettlementPublicLifeState state)
    {
        return new SettlementPublicLifeState
        {
            SettlementId = state.SettlementId,
            SettlementName = state.SettlementName,
            SettlementTier = state.SettlementTier,
            NodeLabel = state.NodeLabel,
            DominantVenueLabel = state.DominantVenueLabel,
            DominantVenueCode = state.DominantVenueCode,
            MonthlyCadenceCode = state.MonthlyCadenceCode,
            MonthlyCadenceLabel = state.MonthlyCadenceLabel,
            CrowdMixLabel = state.CrowdMixLabel,
            StreetTalkHeat = state.StreetTalkHeat,
            MarketBuzz = state.MarketBuzz,
            NoticeVisibility = state.NoticeVisibility,
            RoadReportLag = state.RoadReportLag,
            PrefectureDispatchPressure = state.PrefectureDispatchPressure,
            PublicLegitimacy = state.PublicLegitimacy,
            DocumentaryWeight = state.DocumentaryWeight,
            VerificationCost = state.VerificationCost,
            MarketRumorFlow = state.MarketRumorFlow,
            CourierRisk = state.CourierRisk,
            OfficialNoticeLine = state.OfficialNoticeLine,
            StreetTalkLine = state.StreetTalkLine,
            RoadReportLine = state.RoadReportLine,
            PrefectureDispatchLine = state.PrefectureDispatchLine,
            ContentionSummary = state.ContentionSummary,
            CadenceSummary = state.CadenceSummary,
            PublicSummary = state.PublicSummary,
            ChannelSummary = state.ChannelSummary,
            RouteReportSummary = state.RouteReportSummary,
            LastPublicTrace = state.LastPublicTrace,
        };
    }

    private sealed class PublicLifeAndRumorQueries : IPublicLifeAndRumorQueries
    {
        private readonly PublicLifeAndRumorState _state;

        public PublicLifeAndRumorQueries(PublicLifeAndRumorState state)
        {
            _state = state;
        }

        public SettlementPublicLifeSnapshot GetRequiredSettlementPublicLife(SettlementId settlementId)
        {
            SettlementPublicLifeState settlement = _state.Settlements.Single(entry => entry.SettlementId == settlementId);
            return Clone(settlement);
        }

        public IReadOnlyList<SettlementPublicLifeSnapshot> GetSettlementPublicLife()
        {
            return _state.Settlements
                .OrderBy(static entry => entry.SettlementId.Value)
                .Select(Clone)
                .ToArray();
        }

        private static SettlementPublicLifeSnapshot Clone(SettlementPublicLifeState state)
        {
            return new SettlementPublicLifeSnapshot
            {
                SettlementId = state.SettlementId,
                SettlementName = state.SettlementName,
                SettlementTier = state.SettlementTier,
                NodeLabel = state.NodeLabel,
                DominantVenueLabel = state.DominantVenueLabel,
                DominantVenueCode = state.DominantVenueCode,
                MonthlyCadenceCode = state.MonthlyCadenceCode,
                MonthlyCadenceLabel = state.MonthlyCadenceLabel,
                CrowdMixLabel = state.CrowdMixLabel,
                StreetTalkHeat = state.StreetTalkHeat,
                MarketBuzz = state.MarketBuzz,
                NoticeVisibility = state.NoticeVisibility,
                RoadReportLag = state.RoadReportLag,
                PrefectureDispatchPressure = state.PrefectureDispatchPressure,
                PublicLegitimacy = state.PublicLegitimacy,
                DocumentaryWeight = state.DocumentaryWeight,
                VerificationCost = state.VerificationCost,
                MarketRumorFlow = state.MarketRumorFlow,
                CourierRisk = state.CourierRisk,
                OfficialNoticeLine = state.OfficialNoticeLine,
                StreetTalkLine = state.StreetTalkLine,
                RoadReportLine = state.RoadReportLine,
                PrefectureDispatchLine = state.PrefectureDispatchLine,
                ContentionSummary = state.ContentionSummary,
                CadenceSummary = state.CadenceSummary,
                PublicSummary = state.PublicSummary,
                ChannelSummary = state.ChannelSummary,
                RouteReportSummary = state.RouteReportSummary,
                LastPublicTrace = state.LastPublicTrace,
            };
        }
    }
}
