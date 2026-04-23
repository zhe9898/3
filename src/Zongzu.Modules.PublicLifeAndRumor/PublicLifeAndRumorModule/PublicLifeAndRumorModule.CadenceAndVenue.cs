using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed partial class PublicLifeAndRumorModule : ModuleRunner<PublicLifeAndRumorState>
{
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

        IReadOnlyList<ClanTradeRouteSnapshot> routes,

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


}
