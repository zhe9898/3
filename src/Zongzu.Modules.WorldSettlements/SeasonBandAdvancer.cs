using System;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC §3 — advances the three parallel season-band axes
/// (natural / government / imperial) owned by <c>WorldSettlements</c>.
///
/// <para><b>Cadence split (SPEC §3.2)</b>:</para>
/// <list type="bullet">
///   <item><see cref="AdvanceMonth"/> — agrarian phase / harvest progress /
///         canal window / corvée window / imperial channel decay.</item>
///   <item><see cref="AdvanceXun"/> — labor pinch / market cadence /
///         message delay / water-control pulse.</item>
/// </list>
///
/// <para><b>Cross-axis hard rules (SPEC §3.1, applied after each advance)</b>:</para>
/// <list type="bullet">
///   <item>Mourning ≥ 60 forces CorveeWindow = Quiet (state mourning stops corvée).</item>
///   <item>Amnesty ≥ 50 softens down-flow compliance (handled in route layer, signal emitted here).</item>
///   <item>Succession ≥ 70 at least doubles MessageDelayBand (court backlog).</item>
/// </list>
///
/// <para><b>Determinism</b>: all pulses driven by the scope's
/// <see cref="IDeterministicRandom"/> — never wall-clock or
/// <c>System.Random</c>. Event emission order is deterministic (fixed
/// channel sequence; no hash/dict iteration).</para>
///
/// <para><b>Imperial axis is not self-advanced here</b> — it only decays
/// toward 0. Positive intensity must be injected via
/// <see cref="IImperialEventTestHarness"/> (Phase 1c) or future
/// <c>CourtAndThrone</c> / <c>WorldEvents</c> modules (Phase 2+).</para>
/// </summary>
internal static class SeasonBandAdvancer
{
    // ── Monthly advancement ──────────────────────────────────────────────

    public static MonthAdvanceReport AdvanceMonth(SeasonBandData season, ModuleExecutionScope<WorldSettlementsState> scope)
    {
        GameDate date = scope.Context.CurrentDate;
        season.AsOf = date;

        // Snapshot imperial before decay + cross-axis apply so we can detect
        // whether an external injection (IImperialEventTestHarness) or the
        // monthly decay shifted the rhythm enough to announce. Compared
        // against the persisted "last-announced" baseline — not the start
        // of this method — so an inject-then-decay in a single month still
        // fires the announcement instead of cancelling itself out.
        ImperialBandData previousImperial = CloneImperial(season.PreviousAnnouncedImperial);

        AgrarianPhase previousPhase = season.AgrarianPhase;
        season.AgrarianPhase = ResolveAgrarianPhase(date.Month);
        season.HarvestWindowProgress = ResolveHarvestProgress(date.Month);

        CanalWindow previousCanal = season.CanalWindow;
        season.CanalWindow = ResolveCanalWindow(date.Month);

        CorveeWindow previousCorvee = season.CorveeWindow;
        season.CorveeWindow = ResolveCorveeWindow(season.AgrarianPhase, season.LaborPinch);

        AdvanceWaterControl(season, date.Month, scope);

        // Imperial decay — all externally driven channels tick toward 0 every month.
        DecayImperial(season.Imperial);

        ApplyCrossAxisRules(season);

        // ── Emit events for meaningful transitions ─────────────────────
        if (season.AgrarianPhase != previousPhase)
        {
            scope.Emit(
                WorldSettlementsEventNames.SeasonPhaseAdvanced,
                $"农时入{DescribeAgrarianPhase(season.AgrarianPhase)}。",
                season.AgrarianPhase.ToString());
        }

        bool canalChanged = season.CanalWindow != previousCanal;
        if (canalChanged)
        {
            scope.Emit(
                WorldSettlementsEventNames.CanalWindowChanged,
                $"漕渠转{DescribeCanalWindow(season.CanalWindow)}。",
                season.CanalWindow.ToString());
        }

        if (season.CorveeWindow != previousCorvee)
        {
            scope.Emit(
                WorldSettlementsEventNames.CorveeWindowChanged,
                $"徭役窗口转{DescribeCorveeWindow(season.CorveeWindow)}。",
                season.CorveeWindow.ToString());
        }

        if (IsImperialRhythmChanged(previousImperial, season.Imperial))
        {
            scope.Emit(
                WorldSettlementsEventNames.ImperialRhythmChanged,
                $"皇权节律有变：哀 {season.Imperial.MourningInterruption}、赦 {season.Imperial.AmnestyWave}、储 {season.Imperial.SuccessionUncertainty}。",
                "imperial");
            // Latch the baseline only when we actually announced — avoids
            // silently drifting the reference point during quiet months.
            season.PreviousAnnouncedImperial = CloneImperial(season.Imperial);
        }

        bool floodBreached = season.FloodRisk >= FloodRiskUrgentThreshold;
        if (floodBreached)
        {
            scope.Emit(
                WorldSettlementsEventNames.FloodRiskThresholdBreached,
                $"水险破阈：汛险至 {season.FloodRisk}，堤压 {season.EmbankmentStrain}。",
                "season");
        }

        EmitSeasonalFestivalIfDue(date.Month, scope);

        return new MonthAdvanceReport(
            CanalWindowChanged: canalChanged,
            CanalFrom: previousCanal,
            CanalTo: season.CanalWindow,
            FloodRiskBreached: floodBreached,
            FloodRisk: season.FloodRisk);
    }

    /// <summary>
    /// Report returned by <see cref="AdvanceMonth"/> so the module layer
    /// can derive <see cref="PublicSurfaceSignal"/>s that need access to
    /// settlement nodes (which the advancer does not hold).
    /// </summary>
    internal readonly record struct MonthAdvanceReport(
        bool CanalWindowChanged,
        CanalWindow CanalFrom,
        CanalWindow CanalTo,
        bool FloodRiskBreached,
        int FloodRisk);

    private static ImperialBandData CloneImperial(ImperialBandData source) => new()
    {
        MourningInterruption = source.MourningInterruption,
        AmnestyWave = source.AmnestyWave,
        SuccessionUncertainty = source.SuccessionUncertainty,
        MandateConfidence = source.MandateConfidence,
        CourtTimeDisruption = source.CourtTimeDisruption,
    };

    private static bool IsImperialRhythmChanged(ImperialBandData before, ImperialBandData after)
    {
        // Cross a 10-point band to signal — avoids event spam on decay drift.
        return CrossesBand(before.MourningInterruption, after.MourningInterruption)
            || CrossesBand(before.AmnestyWave, after.AmnestyWave)
            || CrossesBand(before.SuccessionUncertainty, after.SuccessionUncertainty)
            || CrossesBand(before.CourtTimeDisruption, after.CourtTimeDisruption);

        static bool CrossesBand(int before, int after) => Math.Abs(after - before) >= 10;
    }

    // ── Xun-level advancement ────────────────────────────────────────────

    public static void AdvanceXun(SeasonBandData season, ModuleExecutionScope<WorldSettlementsState> scope)
    {
        IDeterministicRandom random = scope.Context.Random;

        // LaborPinch follows agrarian phase: busy phases stack, slack drains.
        int laborTarget = season.AgrarianPhase switch
        {
            AgrarianPhase.Transplant => 80,
            AgrarianPhase.Harvest => 85,
            AgrarianPhase.Tending => 50,
            AgrarianPhase.Sowing => 55,
            _ => 20,
        };
        season.LaborPinch = DriftToward(season.LaborPinch, laborTarget, random.NextInt(8, 15), clampMax: 100);

        // Market heat pulses per xun with baseline drift + festival bumps.
        int marketDelta = random.NextInt(-12, 13);
        if (IsFestivalMonth(scope.Context.CurrentDate.Month))
        {
            marketDelta += 8;
        }
        season.MarketCadencePulse = Math.Clamp(season.MarketCadencePulse + marketDelta, 0, 100);

        // Message delay drifts with succession band (court backlog).
        int messageBase = season.Imperial.SuccessionUncertainty >= SuccessionDelayThreshold
            ? CalibrationBands.MessageDelay_Slow
            : CalibrationBands.MessageDelay_Normal;
        season.MessageDelayBand = Math.Clamp(messageBase + random.NextInt(0, 2), 0, 4);

        // Re-apply cross-axis rules (succession doubles delay; mourning may have latched).
        ApplyCrossAxisRules(season);
    }

    // ── Imperial injection (test harness path) ──────────────────────────

    /// <summary>
    /// Raises an imperial channel to <paramref name="intensity"/> without
    /// exceeding 100. Used by <see cref="WorldSettlementsImperialTestHarness"/>
    /// for SPEC §22 liveness assertions.
    /// </summary>
    public static void InjectImperialPulse(ImperialBandData imperial, ImperialEventKind kind, int intensity)
    {
        int clamped = Math.Clamp(intensity, 0, 100);
        switch (kind)
        {
            case ImperialEventKind.EmperorMourning:
                imperial.MourningInterruption = Math.Max(imperial.MourningInterruption, clamped);
                break;
            case ImperialEventKind.GrandAmnesty:
                imperial.AmnestyWave = Math.Max(imperial.AmnestyWave, clamped);
                break;
            case ImperialEventKind.SuccessionCrisis:
                imperial.SuccessionUncertainty = Math.Max(imperial.SuccessionUncertainty, clamped);
                break;
            case ImperialEventKind.CourtFactionOverturn:
                imperial.CourtTimeDisruption = Math.Max(imperial.CourtTimeDisruption, clamped);
                break;
        }
    }

    // ── Internal helpers ─────────────────────────────────────────────────

    private const int FloodRiskUrgentThreshold = 70;
    private const int MourningQuietThreshold = 60;
    private const int AmnestyDowngradeThreshold = 50;
    private const int SuccessionDelayThreshold = 70;

    private static AgrarianPhase ResolveAgrarianPhase(int month) => month switch
    {
        2 or 3 => AgrarianPhase.Sowing,
        4 or 5 => AgrarianPhase.Transplant,
        6 or 7 => AgrarianPhase.Tending,
        8 or 9 or 10 => AgrarianPhase.Harvest,
        _ => AgrarianPhase.Slack,
    };

    private static int ResolveHarvestProgress(int month) => month switch
    {
        8 => 30,
        9 => 70,
        10 => 100,
        11 => 100,
        12 => 0,
        _ => 0,
    };

    private static CanalWindow ResolveCanalWindow(int month) => month switch
    {
        12 or 1 or 2 => CanalWindow.Closed,      // Winter freeze
        3 or 11 => CanalWindow.Limited,           // Shoulder seasons
        _ => CanalWindow.Open,                    // April through October
    };

    private static CorveeWindow ResolveCorveeWindow(AgrarianPhase phase, int laborPinch)
    {
        // High pinch + tending/harvest phases means mustering is a crisis move.
        if (laborPinch >= 85)
        {
            return CorveeWindow.Emergency;
        }
        if (phase is AgrarianPhase.Transplant or AgrarianPhase.Harvest)
        {
            return CorveeWindow.Pressed;
        }
        return CorveeWindow.Quiet;
    }

    private static void AdvanceWaterControl(
        SeasonBandData season,
        int month,
        ModuleExecutionScope<WorldSettlementsState> scope)
    {
        IDeterministicRandom random = scope.Context.Random;

        // Flood season (June–August): strain and risk climb; other months
        // drain slowly.
        bool isFloodSeason = month is >= 5 and <= 8;
        int strainDelta = isFloodSeason ? random.NextInt(10, 25) : random.NextInt(-8, 2);
        int riskDelta = isFloodSeason ? random.NextInt(8, 22) : random.NextInt(-10, 2);

        season.EmbankmentStrain = Math.Clamp(season.EmbankmentStrain + strainDelta, 0, 100);
        season.FloodRisk = Math.Clamp(season.FloodRisk + riskDelta, 0, 100);

        // Confidence is the inverse pressure — climbs during slack months.
        int confidenceDelta = isFloodSeason ? -random.NextInt(5, 12) : random.NextInt(2, 8);
        season.WaterControlConfidence = Math.Clamp(season.WaterControlConfidence + confidenceDelta, 0, 100);
    }

    private static void DecayImperial(ImperialBandData imperial)
    {
        imperial.MourningInterruption = DecayToward(imperial.MourningInterruption, 0, step: 8);
        imperial.AmnestyWave = DecayToward(imperial.AmnestyWave, 0, step: 6);
        imperial.SuccessionUncertainty = DecayToward(imperial.SuccessionUncertainty, 0, step: 5);
        imperial.CourtTimeDisruption = DecayToward(imperial.CourtTimeDisruption, 0, step: 6);
        // MandateConfidence does not self-decay — externally driven only.
    }

    private static void ApplyCrossAxisRules(SeasonBandData season)
    {
        if (season.Imperial.MourningInterruption >= MourningQuietThreshold)
        {
            season.CorveeWindow = CorveeWindow.Quiet;
        }

        if (season.Imperial.SuccessionUncertainty >= SuccessionDelayThreshold)
        {
            // At minimum double the message delay band.
            season.MessageDelayBand = Math.Clamp(Math.Max(season.MessageDelayBand, CalibrationBands.MessageDelay_Slow * 2), 0, 4);
        }
    }

    private static void EmitSeasonalFestivalIfDue(int month, ModuleExecutionScope<WorldSettlementsState> scope)
    {
        string? festivalKey = month switch
        {
            SeasonalFestivals.QingmingMonth => SeasonalFestivals.Qingming,
            SeasonalFestivals.DuanwuMonth => SeasonalFestivals.Duanwu,
            SeasonalFestivals.ZhongyuanMonth => SeasonalFestivals.Zhongyuan,
            SeasonalFestivals.QiusheMonth => SeasonalFestivals.Qiushe,
            SeasonalFestivals.LabaMonth => SeasonalFestivals.Laba,
            _ => null,
        };

        if (festivalKey is null)
        {
            return;
        }

        scope.Emit(
            WorldSettlementsEventNames.SeasonalFestivalArrived,
            $"时至{DescribeFestival(festivalKey)}。",
            festivalKey);
    }

    private static bool IsFestivalMonth(int month) => month is
        SeasonalFestivals.QingmingMonth
        or SeasonalFestivals.DuanwuMonth
        or SeasonalFestivals.ZhongyuanMonth
        or SeasonalFestivals.QiusheMonth
        or SeasonalFestivals.LabaMonth;

    private static int DriftToward(int current, int target, int step, int clampMax)
    {
        int delta = target - current;
        int move = Math.Sign(delta) * Math.Min(Math.Abs(delta), step);
        return Math.Clamp(current + move, 0, clampMax);
    }

    private static int DecayToward(int current, int target, int step)
    {
        int delta = target - current;
        int move = Math.Sign(delta) * Math.Min(Math.Abs(delta), step);
        return Math.Clamp(current + move, 0, 100);
    }

    private static string DescribeAgrarianPhase(AgrarianPhase phase) => phase switch
    {
        AgrarianPhase.Sowing => "春种",
        AgrarianPhase.Transplant => "插秧",
        AgrarianPhase.Tending => "田间",
        AgrarianPhase.Harvest => "秋收",
        _ => "农闲",
    };

    private static string DescribeCanalWindow(CanalWindow window) => window switch
    {
        CanalWindow.Closed => "冰封",
        CanalWindow.Limited => "半通",
        _ => "畅通",
    };

    private static string DescribeCorveeWindow(CorveeWindow window) => window switch
    {
        CorveeWindow.Emergency => "紧征",
        CorveeWindow.Pressed => "紧张",
        _ => "暂歇",
    };

    private static string DescribeFestival(string key) => key switch
    {
        SeasonalFestivals.Qingming => "清明",
        SeasonalFestivals.Duanwu => "端午",
        SeasonalFestivals.Zhongyuan => "中元",
        SeasonalFestivals.Qiushe => "秋社",
        SeasonalFestivals.Laba => "腊八",
        _ => key,
    };
}
