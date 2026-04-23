using System;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC §2.5 + §22.1 — the minimum live pressure chain
/// Phase 1c promises (STATIC_BACKEND_FIRST.md): routes react to the
/// <see cref="SeasonBandData"/> their own module already owns. Scope is
/// deliberately narrow — no cross-module reads, no banditry, no warfare.
///
/// <para><b>Inputs (same-module only)</b>: <see cref="SeasonBandData.CanalWindow"/>,
/// <see cref="SeasonBandData.FloodRisk"/>, <see cref="RouteStateData.Medium"/>,
/// <see cref="RouteStateData.SeasonalVulnerability"/>.</para>
///
/// <para><b>Outputs</b>: drifted <see cref="RouteStateData.Reliability"/>,
/// updated <see cref="RouteStateData.CurrentConstraintLabel"/>, and
/// <see cref="WorldSettlementsEventNames.RouteConstraintEmerged"/> /
/// <see cref="WorldSettlementsEventNames.RouteConstraintCleared"/> events on
/// downward / upward band crossings.</para>
///
/// <para><b>Determinism</b>: all drift uses <see cref="IDeterministicRandom"/>;
/// routes are iterated by <see cref="RouteId.Value"/> order so emit order is
/// stable across runs.</para>
///
/// <para><b>Cadence</b>: month only. Routes do not pulse per xun — xun-level
/// behavior belongs to trade / population traffic, which are outside this
/// module.</para>
/// </summary>
internal static class RouteAdvancer
{
    private const int FloodBreachThreshold = 70;
    private const int ConstrainedBand = 60;
    private const int BrokenBand = 40;

    public static void AdvanceMonth(WorldSettlementsState state, ModuleExecutionScope<WorldSettlementsState> scope)
    {
        SeasonBandData season = state.CurrentSeason;
        IDeterministicRandom random = scope.Context.Random;

        foreach (RouteStateData route in SortById(state))
        {
            int before = route.Reliability;
            int delta = ComputeDelta(route, season, random);
            int after = Math.Clamp(before + delta, 0, 100);
            route.Reliability = after;

            string? crossedDownLabel = DetectDownwardCrossing(before, after, route, season);
            if (crossedDownLabel is not null)
            {
                route.CurrentConstraintLabel = crossedDownLabel;
                scope.Emit(
                    WorldSettlementsEventNames.RouteConstraintEmerged,
                    $"路阻现形：{DescribeRouteKind(route.Kind)}受{DescribeConstraint(crossedDownLabel)}所迫，靠谱降至 {after}。",
                    route.Id.Value.ToString());
                continue;
            }

            if (DetectUpwardCrossing(before, after))
            {
                route.CurrentConstraintLabel = string.Empty;
                scope.Emit(
                    WorldSettlementsEventNames.RouteConstraintCleared,
                    $"路阻稍解：{DescribeRouteKind(route.Kind)}靠谱回至 {after}。",
                    route.Id.Value.ToString());
            }
        }
    }

    private static System.Collections.Generic.IEnumerable<RouteStateData> SortById(WorldSettlementsState state)
    {
        // Explicit sort — no LINQ allocation dependency on hash order.
        RouteStateData[] buffer = state.Routes.ToArray();
        Array.Sort(buffer, static (a, b) => a.Id.Value.CompareTo(b.Id.Value));
        return buffer;
    }

    private static int ComputeDelta(RouteStateData route, SeasonBandData season, IDeterministicRandom random)
    {
        // Baseline fluctuation — guarantees non-zero variance even for sheltered routes
        // (SPEC §22.1 requires OfficialDispatchRoute.Reliability to vary across 12 months).
        int delta = random.NextInt(-3, 4);

        bool isWaterMedium = route.Medium is RouteMedium.WaterRiver
            or RouteMedium.WaterCanal
            or RouteMedium.FerryLink;
        bool crossesWater = isWaterMedium || route.SeasonalVulnerability >= 50;

        if (isWaterMedium && season.CanalWindow == CanalWindow.Closed)
        {
            delta -= random.NextInt(8, 19);
        }
        else if (isWaterMedium && season.CanalWindow == CanalWindow.Limited)
        {
            delta -= random.NextInt(2, 7);
        }

        if (season.FloodRisk >= FloodBreachThreshold && crossesWater)
        {
            delta -= random.NextInt(5, 13);
        }

        // Slow recovery when the world is quiet — open canal, low flood risk.
        if (season.CanalWindow == CanalWindow.Open && season.FloodRisk < 40)
        {
            delta += random.NextInt(1, 5);
        }

        return delta;
    }

    private static string? DetectDownwardCrossing(int before, int after, RouteStateData route, SeasonBandData season)
    {
        if (before >= BrokenBand && after < BrokenBand)
        {
            return ResolveConstraintLabel(route, season, severe: true);
        }
        if (before >= ConstrainedBand && after < ConstrainedBand)
        {
            return ResolveConstraintLabel(route, season, severe: false);
        }
        return null;
    }

    private static bool DetectUpwardCrossing(int before, int after)
    {
        return (before < ConstrainedBand && after >= ConstrainedBand)
            || (before < BrokenBand && after >= BrokenBand);
    }

    private static string ResolveConstraintLabel(RouteStateData route, SeasonBandData season, bool severe)
    {
        if (season.CanalWindow == CanalWindow.Closed
            && route.Medium is RouteMedium.WaterRiver or RouteMedium.WaterCanal or RouteMedium.FerryLink)
        {
            return "route-canal-closed";
        }
        if (season.FloodRisk >= FloodBreachThreshold)
        {
            return severe ? "route-flood-broken" : "route-flood-silting";
        }
        return severe ? "route-broken" : "route-low-reliability";
    }

    private static string DescribeRouteKind(RouteKind kind) => kind switch
    {
        RouteKind.GrainRoute => "漕粮路",
        RouteKind.MarketRoute => "市道",
        RouteKind.OfficialDispatchRoute => "驿传路",
        RouteKind.SmugglingCorridor => "私盐径",
        RouteKind.FugitivePath => "亡命径",
        _ => "路",
    };

    private static string DescribeConstraint(string label) => label switch
    {
        "route-canal-closed" => "冰封漕渠",
        "route-flood-silting" => "汛后淤塞",
        "route-flood-broken" => "汛水冲断",
        "route-low-reliability" => "路况起伏",
        "route-broken" => "路体崩坏",
        _ => "事故",
    };
}
