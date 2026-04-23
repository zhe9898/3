using System.Collections.Generic;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Integration.Tests;

/// <summary>
/// SPATIAL_SKELETON_SPEC §22 — Phase 1c 活世界收官测试（liveness test）。
///
/// <para>本文件是 SPEC §22.4 指定名 <c>LivingWorldLivenessTests.cs</c> 的最小
/// 实装，覆盖 §22.1 断言矩阵：</para>
/// <list type="bullet">
///   <item>§22.1 自然轴脉动：SeasonPhaseAdvanced / CanalWindowChanged /
///         FloodRiskThresholdBreached / SeasonalFestivalArrived。</item>
///   <item>§22.1 政府轴脉动：CorveeWindowChanged。</item>
///   <item>§22.1 路轴：RouteConstraintEmerged ≥ 1；OfficialDispatchRoute.Reliability
///         一年内方差 &gt; 0（不是静态摆设）。</item>
///   <item>§22.1 皇权轴：注入 Mourning ≥ 60 强制 CorveeWindow = Quiet，
///         并发 ImperialRhythmChanged。</item>
///   <item>§22.1 公域脉动 / 壳可见：Locus ReasonKey ≥ 3；PublicSurfaceSignal
///         至少覆盖三条 OpinionChannel；一年内至少一条 Urgent 通知（汛险破阈）。</item>
///   <item>§22.1 静态骨骼：10 节点 / 5 路 / covert + illicit 层必存。</item>
///   <item>Determinism：同 seed 的 replay hash 与 domain event 序列必等。</item>
/// </list>
///
/// <para>SPEC §22.3 死世界反面症状（脉动消失 / 隐节点显形 / 灰路消失 / Locus
/// 锁死 / 节点永不告警）全部以 <b>正向断言</b> 的方式在本文件被钉住。</para>
/// </summary>
[TestFixture]
public sealed class LivingWorldLivenessTests
{
    private const long LivenessSeed = 20250601L;

    // ── §12 静态骨骼形状 ──────────────────────────────────────────────────

    [Test]
    public void LanxiSeed_ProducesTenNodes_WithCanonicalNodeKindDistribution()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);

        IWorldSettlementsQueries queries = simulation.GetQueryForTesting<IWorldSettlementsQueries>();
        IReadOnlyList<SettlementSnapshot> settlements = queries.GetSettlements();

        Assert.That(settlements.Count, Is.EqualTo(10), "SPEC §12.2 seed nodes: 9 core + canal-junction anchor.");

        Dictionary<SettlementNodeKind, int> countsByKind = settlements
            .GroupBy(static node => node.NodeKind)
            .ToDictionary(static group => group.Key, static group => group.Count());

        Assert.Multiple(() =>
        {
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.CountySeat), Is.EqualTo(1));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.MarketTown), Is.EqualTo(1));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.LineageHall), Is.EqualTo(1));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.Village), Is.EqualTo(2));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.Ferry), Is.EqualTo(1));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.Granary), Is.EqualTo(1));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.Temple), Is.EqualTo(1));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.CanalJunction), Is.EqualTo(1));
            Assert.That(countsByKind.GetValueOrDefault(SettlementNodeKind.SmugglingCache), Is.EqualTo(1));
        });
    }

    [Test]
    public void LanxiSeed_PreservesCovertAndIllicitLayers()
    {
        // SPEC §22.3 死世界反面症状："隐节点显形 / 灰色路线消失" 的正向断言。
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        IWorldSettlementsQueries queries = simulation.GetQueryForTesting<IWorldSettlementsQueries>();

        Assert.Multiple(() =>
        {
            Assert.That(queries.GetSettlementsByVisibility(NodeVisibility.Covert).Count, Is.EqualTo(1),
                "Covert 盐窝必须存在 —— 否则兰溪种子退化到公共层。");
            Assert.That(queries.GetRoutesByLegitimacy(RouteLegitimacy.Illicit).Count, Is.EqualTo(1),
                "灰色盐路必须存在 —— 否则合法化世界坍塌为单一秩序。");
        });
    }

    [Test]
    public void LanxiSeed_ProducesFiveRoutes_AcrossAllSpecifiedKinds()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        IWorldSettlementsQueries queries = simulation.GetQueryForTesting<IWorldSettlementsQueries>();

        IReadOnlyList<RouteSnapshot> routes = queries.GetRoutes();
        Assert.That(routes.Count, Is.EqualTo(5), "SPEC §12.5 specifies five seed routes.");

        Assert.Multiple(() =>
        {
            Assert.That(queries.GetRoutesByKind(RouteKind.GrainRoute).Count, Is.EqualTo(2));
            Assert.That(queries.GetRoutesByKind(RouteKind.MarketRoute).Count, Is.EqualTo(1));
            Assert.That(queries.GetRoutesByKind(RouteKind.OfficialDispatchRoute).Count, Is.EqualTo(1));
            Assert.That(queries.GetRoutesByKind(RouteKind.SmugglingCorridor).Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void InitialSeason_Matches_March1200_Baseline()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        SeasonBandSnapshot season = simulation.GetQueryForTesting<IWorldSettlementsQueries>().GetCurrentSeason();

        Assert.Multiple(() =>
        {
            Assert.That(season.AsOf, Is.EqualTo(new GameDate(1200, 3)));
            Assert.That(season.AgrarianPhase, Is.EqualTo(AgrarianPhase.Sowing));
            Assert.That(season.CanalWindow, Is.EqualTo(CanalWindow.Limited),
                "三月漕渠过渡期 —— SPEC §12.6 seed baseline.");
            Assert.That(season.CorveeWindow, Is.EqualTo(CorveeWindow.Quiet));
        });
    }

    // ── §3 季节脉动（全部 §22.1 指定事件） ──────────────────────────────

    [Test]
    public void TwelveMonthRun_EmitsFullSeasonAndGovernmentRhythm()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        LivenessTrace trace = CollectTwelveMonthTrace(simulation);

        Assert.Multiple(() =>
        {
            // SPEC §22.1 自然轴
            Assert.That(
                trace.EventCounts.GetValueOrDefault(WorldSettlementsEventNames.SeasonPhaseAdvanced),
                Is.GreaterThanOrEqualTo(4),
                "五相位 / 年必须轮完一圈。");
            Assert.That(
                trace.EventCounts.GetValueOrDefault(WorldSettlementsEventNames.CanalWindowChanged),
                Is.GreaterThanOrEqualTo(2),
                "春开 + 秋闭最少两次漕窗切换。");
            Assert.That(
                trace.EventCounts.GetValueOrDefault(WorldSettlementsEventNames.FloodRiskThresholdBreached),
                Is.GreaterThanOrEqualTo(1),
                "兰溪水网县汛季必破阈 —— 否则退化为 '节点永不告警'。");
            Assert.That(
                trace.EventCounts.GetValueOrDefault(WorldSettlementsEventNames.SeasonalFestivalArrived),
                Is.GreaterThanOrEqualTo(3),
                "清明 / 端午 / 中元 / 秋社 / 腊八 五选三。");

            // SPEC §22.1 政府轴 —— 插秧 / 秋收两度走到 Pressed 以上。
            Assert.That(
                trace.EventCounts.GetValueOrDefault(WorldSettlementsEventNames.CorveeWindowChanged),
                Is.GreaterThanOrEqualTo(2),
                "SPEC §22.1：徭役窗口一年至少两次切换（Quiet ↔ Pressed/Emergency）。");
        });
    }

    [Test]
    public void TwelveMonthRun_ShowsLaborAndMarketPulsation()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        List<int> laborSamples = new(12);
        List<int> marketSamples = new(12);

        for (int month = 0; month < 12; month += 1)
        {
            simulation.AdvanceOneMonth();
            SeasonBandSnapshot season = simulation.GetQueryForTesting<IWorldSettlementsQueries>().GetCurrentSeason();
            laborSamples.Add(season.LaborPinch);
            marketSamples.Add(season.MarketCadencePulse);
        }

        int laborRange = laborSamples.Max() - laborSamples.Min();
        Assert.That(laborRange, Is.GreaterThanOrEqualTo(40),
            "SPEC §22.1：一年忙闲极差 ≥ 40。");

        double mean = marketSamples.Average();
        double variance = marketSamples.Select(value => (value - mean) * (value - mean)).Sum() / marketSamples.Count;
        double stddev = System.Math.Sqrt(variance);
        Assert.That(stddev, Is.GreaterThan(8.0),
            "SPEC §22.3 死世界症状 '脉动消失' 的反面：市脉必须波动。");
    }

    // ── §22.1 路轴：RouteAdvancer 产生的 live pressure chain ─────────────

    [Test]
    public void TwelveMonthRun_EmitsRouteConstraintEmerged_AtLeastOnce()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        LivenessTrace trace = CollectTwelveMonthTrace(simulation);

        Assert.That(
            trace.EventCounts.GetValueOrDefault(WorldSettlementsEventNames.RouteConstraintEmerged),
            Is.GreaterThanOrEqualTo(1),
            "SPEC §22.1 / STATIC_BACKEND_FIRST.md 最小活规则：一年内至少一次路阻向下穿带。");
    }

    [Test]
    public void OfficialDispatchRoute_Reliability_VariesAcrossYear()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        IWorldSettlementsQueries queries = simulation.GetQueryForTesting<IWorldSettlementsQueries>();

        HashSet<int> distinct = new();
        for (int month = 0; month < 12; month += 1)
        {
            simulation.AdvanceOneMonth();
            RouteSnapshot dispatch = queries.GetRoutesByKind(RouteKind.OfficialDispatchRoute).Single();
            distinct.Add(dispatch.Reliability);
        }

        Assert.That(distinct.Count, Is.GreaterThan(1),
            "SPEC §22.1：驿传路 Reliability 一年内必须变 —— 否则退化为 '数字摆设'。");
    }

    // ── §6.4 locus + §20.3 signal ───────────────────────────────────────

    [Test]
    public void Locus_IsAlwaysPresent_AfterFirstMonth_AndShiftsAcrossYear()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        HashSet<string> seenReasonKeys = new(System.StringComparer.Ordinal);
        int nonNullTicks = 0;

        for (int month = 0; month < 12; month += 1)
        {
            simulation.AdvanceOneMonth();
            LocusSnapshot? locus = simulation.GetQueryForTesting<IWorldSettlementsQueries>().GetCurrentLocus();
            if (locus is not null)
            {
                nonNullTicks += 1;
                seenReasonKeys.Add(locus.ReasonKey);
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(nonNullTicks, Is.EqualTo(12),
                "SPEC §22.1：12 月内 GetCurrentLocus 任意时刻非空（100%）。");
            Assert.That(seenReasonKeys.Count, Is.GreaterThanOrEqualTo(3),
                $"SPEC §22.1 / §22.3：ReasonKey 至少 3 种 —— 实得 [{string.Join(",", seenReasonKeys)}]。");
        });
    }

    [Test]
    public void FloodBreachMonth_EmitsMultipleStreamSignals_IncludingUrgentTier()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        HashSet<OpinionChannel> streamsSeenOverYear = new();
        bool foundFloodBreachWithMultipleStreams = false;
        bool sawUrgentSignalThisYear = false;

        for (int month = 0; month < 12; month += 1)
        {
            simulation.AdvanceOneMonth();

            IReadOnlyList<IDomainEvent> events = simulation.LastMonthResult!.DomainEvents;
            bool floodBreachedThisMonth = events.Any(evt => evt.EventType == WorldSettlementsEventNames.FloodRiskThresholdBreached);

            IReadOnlyList<PublicSurfaceSignal> signals =
                simulation.GetQueryForTesting<IWorldSettlementsQueries>().GetCurrentPulseSignals();

            foreach (PublicSurfaceSignal signal in signals)
            {
                streamsSeenOverYear.Add(signal.Stream);
                if (signal.Tier == NotificationTier.Urgent)
                {
                    sawUrgentSignalThisYear = true;
                }
            }

            if (floodBreachedThisMonth && signals.Count >= 2)
            {
                int distinctStreams = signals.Select(static signal => signal.Stream).Distinct().Count();
                if (distinctStreams >= 2)
                {
                    foundFloodBreachWithMultipleStreams = true;
                }
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(streamsSeenOverYear.Count, Is.GreaterThanOrEqualTo(3),
                "SPEC §22.1：PublicSurfaceSignal 必须覆盖至少三条 OpinionChannel。");
            Assert.That(foundFloodBreachWithMultipleStreams, Is.True,
                "SPEC §22.1：同一次汛险破阈至少产出两条不同 stream 的 signal（流竞争的最小证据）。");
            Assert.That(sawUrgentSignalThisYear, Is.True,
                "SPEC §22.1：一年内必须至少一条 Urgent 级 PublicSurfaceSignal（通知托盘紧报）。");
        });
    }

    // ── §3.1 皇权轴 Month 6 注入 ────────────────────────────────────────

    [Test]
    public void ImperialInjection_AtMonthSix_ForcesCorveeQuiet_AndEmitsRhythmChange()
    {
        GameSimulation simulation = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);

        simulation.AdvanceMonths(5);

        IImperialEventTestHarness harness = simulation.GetQueryForTesting<IImperialEventTestHarness>();
        harness.Inject(ImperialEventKind.EmperorMourning, 80);

        simulation.AdvanceOneMonth();

        SeasonBandSnapshot seasonAfter = simulation.GetQueryForTesting<IWorldSettlementsQueries>().GetCurrentSeason();
        int rhythmChangedCount = simulation.LastMonthResult!.DomainEvents
            .Count(evt => evt.EventType == WorldSettlementsEventNames.ImperialRhythmChanged);

        Assert.Multiple(() =>
        {
            Assert.That(seasonAfter.Imperial.MourningInterruption, Is.GreaterThanOrEqualTo(60),
                "注入后 Mourning 必须仍高于跨轴阈值 60。");
            Assert.That(seasonAfter.CorveeWindow, Is.EqualTo(CorveeWindow.Quiet),
                "SPEC §3.1 cross-axis：Mourning ≥ 60 → CorveeWindow 强制 Quiet。");
            Assert.That(rhythmChangedCount, Is.GreaterThanOrEqualTo(1),
                "SPEC §22.1：注入后必须发出至少一次 ImperialRhythmChanged。");
        });
    }

    // ── Determinism ──────────────────────────────────────────────────────

    [Test]
    public void TwelveMonthRun_IsDeterministicAcrossRuns()
    {
        GameSimulation a = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);
        GameSimulation b = SimulationBootstrapper.CreatePhase1cLivenessBootstrap(LivenessSeed);

        List<string> eventsA = [];
        List<string> eventsB = [];

        for (int month = 0; month < 12; month += 1)
        {
            a.AdvanceOneMonth();
            b.AdvanceOneMonth();
            foreach (IDomainEvent evt in a.LastMonthResult!.DomainEvents)
            {
                eventsA.Add($"{evt.EventType}|{evt.EntityKey}");
            }
            foreach (IDomainEvent evt in b.LastMonthResult!.DomainEvents)
            {
                eventsB.Add($"{evt.EventType}|{evt.EntityKey}");
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(b.ReplayHash, Is.EqualTo(a.ReplayHash),
                "同 seed 同步长 replay hash 必须相等。");
            Assert.That(eventsB, Is.EqualTo(eventsA),
                "同 seed 同步长 domain event 序列必须逐条相等（emit order 确定性）。");
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static LivenessTrace CollectTwelveMonthTrace(GameSimulation simulation)
    {
        Dictionary<string, int> eventCounts = new(System.StringComparer.Ordinal);

        for (int month = 0; month < 12; month += 1)
        {
            simulation.AdvanceOneMonth();
            foreach (IDomainEvent evt in simulation.LastMonthResult!.DomainEvents)
            {
                eventCounts.TryGetValue(evt.EventType, out int current);
                eventCounts[evt.EventType] = current + 1;
            }
        }

        return new LivenessTrace(eventCounts);
    }

    private sealed record LivenessTrace(IReadOnlyDictionary<string, int> EventCounts);
}
