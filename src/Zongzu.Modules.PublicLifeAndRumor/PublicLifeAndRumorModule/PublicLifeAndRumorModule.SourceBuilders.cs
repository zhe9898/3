using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed partial class PublicLifeAndRumorModule : ModuleRunner<PublicLifeAndRumorState>
{
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


    private static Dictionary<SettlementId, List<ClanTradeRouteSnapshot>> BuildRoutesBySettlement(ModuleExecutionScope<PublicLifeAndRumorState> scope)

    {

        Dictionary<SettlementId, List<ClanTradeRouteSnapshot>> routesBySettlement = new();

        if (!scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry))

        {

            return routesBySettlement;

        }


        ITradeAndIndustryQueries tradeQueries = scope.GetRequiredQuery<ITradeAndIndustryQueries>();

        foreach (ClanTradeSnapshot trade in tradeQueries.GetClanTrades().OrderBy(static entry => entry.ClanId.Value))

        {

            foreach (ClanTradeRouteSnapshot route in tradeQueries.GetRoutesForClan(trade.ClanId).OrderBy(static entry => entry.RouteId))

            {

                if (!routesBySettlement.TryGetValue(route.SettlementId, out List<ClanTradeRouteSnapshot>? routes))

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


}
