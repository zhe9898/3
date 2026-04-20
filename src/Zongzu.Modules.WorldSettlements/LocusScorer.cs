using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC §6.4 / §8 (decision J) — deterministic "what is
/// the desk sandbox pointing at right now?" scorer.
///
/// <para>Phase 1c scope: a rules-driven cascade of <i>most-pressing</i>
/// season-band readings, each resolving to a single node + <c>ReasonKey</c>
/// + intensity. No randomness, no wall-clock, no event-pool sampling — the
/// same state always yields the same locus, per SPEC §8's determinism rule.</para>
///
/// <para>Cascade priority (first matching wins):
/// <list type="number">
///   <item>Flood breach (<c>FloodRisk &gt;= 70</c>) → ferry / canal junction.</item>
///   <item>Mourning-driven corvée latch → county seat.</item>
///   <item>Canal window at a non-Open extreme during shoulder season → ferry / canal junction.</item>
///   <item>Corvée pressure (<c>CorveeWindow != Quiet</c>) → county seat.</item>
///   <item>Harvest peak (<c>AgrarianPhase == Harvest</c>) → granary.</item>
///   <item>Market peak (<c>MarketCadencePulse &gt;= 60</c>) → market town.</item>
///   <item>Otherwise → county seat routine.</item>
/// </list></para>
///
/// <para>Ties within a matching class resolve by <see cref="SettlementId"/>
/// order, never by dict iteration.</para>
/// </summary>
internal static class LocusScorer
{
    public static LocusSnapshot? Score(WorldSettlementsState state)
    {
        if (state.Settlements.Count == 0)
        {
            return null;
        }

        SeasonBandData season = state.CurrentSeason;
        IReadOnlyList<SettlementStateData> nodes = state.Settlements
            .OrderBy(static node => node.Id.Value)
            .ToArray();

        // 1. Flood breach dominates everything else.
        if (season.FloodRisk >= 70)
        {
            SettlementStateData? anchor = FirstByKind(nodes, SettlementNodeKind.Ferry)
                ?? FirstByKind(nodes, SettlementNodeKind.CanalJunction)
                ?? FirstByKind(nodes, SettlementNodeKind.CountySeat);
            if (anchor is not null)
            {
                return new LocusSnapshot(anchor.Id, null, "flood-risk-breached", season.FloodRisk);
            }
        }

        // 2. Mourning-driven corvée latch (cross-axis rule).
        if (season.Imperial.MourningInterruption >= 60)
        {
            SettlementStateData? anchor = FirstByKind(nodes, SettlementNodeKind.CountySeat);
            if (anchor is not null)
            {
                int intensity = System.Math.Clamp(season.Imperial.MourningInterruption, 0, 100);
                return new LocusSnapshot(anchor.Id, null, "imperial-mourning", intensity);
            }
        }

        // 3. Shoulder-season canal transitions — ferry takes focus.
        if (season.CanalWindow == CanalWindow.Closed)
        {
            SettlementStateData? anchor = FirstByKind(nodes, SettlementNodeKind.Ferry)
                ?? FirstByKind(nodes, SettlementNodeKind.CanalJunction);
            if (anchor is not null)
            {
                return new LocusSnapshot(anchor.Id, null, "canal-closed", 65);
            }
        }
        if (season.CanalWindow == CanalWindow.Limited)
        {
            SettlementStateData? anchor = FirstByKind(nodes, SettlementNodeKind.Ferry)
                ?? FirstByKind(nodes, SettlementNodeKind.CanalJunction);
            if (anchor is not null)
            {
                return new LocusSnapshot(anchor.Id, null, "canal-limited", 55);
            }
        }

        // 4. Corvée pressure.
        if (season.CorveeWindow != CorveeWindow.Quiet)
        {
            SettlementStateData? anchor = FirstByKind(nodes, SettlementNodeKind.CountySeat);
            if (anchor is not null)
            {
                int intensity = season.CorveeWindow == CorveeWindow.Emergency
                    ? System.Math.Max(75, season.LaborPinch)
                    : System.Math.Max(55, season.LaborPinch);
                return new LocusSnapshot(anchor.Id, null, "corvee-peak", intensity);
            }
        }

        // 5. Harvest peak.
        if (season.AgrarianPhase == AgrarianPhase.Harvest)
        {
            SettlementStateData? anchor = FirstByKind(nodes, SettlementNodeKind.Granary)
                ?? FirstByKind(nodes, SettlementNodeKind.CountySeat);
            if (anchor is not null)
            {
                int intensity = System.Math.Max(season.HarvestWindowProgress, 40);
                return new LocusSnapshot(anchor.Id, null, "harvest-peak", intensity);
            }
        }

        // 6. Market peak.
        if (season.MarketCadencePulse >= 60)
        {
            SettlementStateData? anchor = FirstByKind(nodes, SettlementNodeKind.MarketTown)
                ?? FirstByKind(nodes, SettlementNodeKind.CountySeat);
            if (anchor is not null)
            {
                return new LocusSnapshot(anchor.Id, null, "market-peak", season.MarketCadencePulse);
            }
        }

        // 7. Routine fallback — never null once any node exists.
        SettlementStateData routine = FirstByKind(nodes, SettlementNodeKind.CountySeat)
            ?? nodes[0];
        return new LocusSnapshot(routine.Id, null, "county-routine", 30);
    }

    private static SettlementStateData? FirstByKind(
        IReadOnlyList<SettlementStateData> nodes,
        SettlementNodeKind kind)
    {
        foreach (SettlementStateData node in nodes)
        {
            if (node.NodeKind == kind && node.Visibility != NodeVisibility.Covert)
            {
                return node;
            }
        }
        return null;
    }
}
