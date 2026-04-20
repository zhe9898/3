using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed partial class PublicLifeAndRumorModule : ModuleRunner<PublicLifeAndRumorState>
{
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


    private static string BuildRoadReportLine(SettlementPublicLifeState state, IReadOnlyList<ClanTradeRouteSnapshot> routes)

    {

        if (routes.Count == 0)

        {

            return state.RoadReportLag >= 45

                ? "此地外路稀少，乡里多半只能凭迟来的转口消息揣度虚实。"

                : "此地外路不繁，消息多在近处缓缓传开。";

        }


        ClanTradeRouteSnapshot leadRoute = routes

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

        IReadOnlyList<ClanTradeRouteSnapshot> routes,

        JurisdictionAuthoritySnapshot? jurisdiction)

    {

        if (routes.Count == 0)

        {

            return state.PrefectureDispatchPressure >= 50

                ? $"州牒催迫已至{state.PrefectureDispatchPressure}，虽无大路过境，里甲仍觉差科发急。"

                : $"此地暂无大路牵扰，路报迟滞仅至{state.RoadReportLag}。";

        }


        ClanTradeRouteSnapshot leadRoute = routes

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


}
