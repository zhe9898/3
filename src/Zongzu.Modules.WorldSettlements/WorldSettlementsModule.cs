using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

public sealed class WorldSettlementsModule : ModuleRunner<WorldSettlementsState>
{
    private static readonly string[] EventNames =
    [
        WorldSettlementsEventNames.SettlementPressureChanged,
        WorldSettlementsEventNames.SeasonPhaseAdvanced,
        WorldSettlementsEventNames.CanalWindowChanged,
        WorldSettlementsEventNames.CorveeWindowChanged,
        WorldSettlementsEventNames.ImperialRhythmChanged,
        WorldSettlementsEventNames.ComplianceModeShifted,
        WorldSettlementsEventNames.RouteConstraintEmerged,
        WorldSettlementsEventNames.IllicitRouteExposed,
        WorldSettlementsEventNames.NodeVisibilityDiscovered,
        WorldSettlementsEventNames.FloodRiskThresholdBreached,
        WorldSettlementsEventNames.ForceStationChanged,
        WorldSettlementsEventNames.SeasonalFestivalArrived,
        // STEP2A / A0c — 契约登记，规则发射留给后续 step。
        WorldSettlementsEventNames.EpidemicOutbreak,
        WorldSettlementsEventNames.ReliefDelivered,
        WorldSettlementsEventNames.ReliefWithheld,
        WorldSettlementsEventNames.TaxSeasonOpened,
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.WorldSettlements;

    public override int ModuleSchemaVersion => 6;

    public override SimulationPhase Phase => SimulationPhase.WorldBaseline;

    public override int ExecutionOrder => 100;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunMonthAndSeasonal;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override WorldSettlementsState CreateInitialState()
    {
        return new WorldSettlementsState();
    }

    public override void RegisterQueries(WorldSettlementsState state, QueryRegistry queries)
    {
        queries.Register<IWorldSettlementsQueries>(new WorldSettlementsQueries(state));
        queries.Register<IImperialEventTestHarness>(new WorldSettlementsImperialTestHarness(state));
    }

    public override void RunXun(ModuleExecutionScope<WorldSettlementsState> scope)
    {
        // SPEC §20.3: pulse signals have tick lifetime — reset before this
        // tick's emissions.
        scope.State.CurrentPulseSignals.Clear();

        // SPATIAL_SKELETON_SPEC §3.2 — xun advances labor/market/message axes.
        SeasonBandAdvancer.AdvanceXun(scope.State.CurrentSeason, scope);

        foreach (SettlementStateData settlement in scope.State.Settlements.OrderBy(static settlement => settlement.Id.Value))
        {
            int oldSecurity = settlement.Security;
            int oldProsperity = settlement.Prosperity;

            int securityDelta = scope.Context.CurrentXun switch
            {
                SimulationXun.Shangxun => scope.Context.Random.NextInt(-1, 2),
                SimulationXun.Zhongxun => scope.Context.Random.NextInt(-1, 2),
                SimulationXun.Xiaxun => scope.Context.Random.NextInt(-2, 1),
                _ => 0,
            };
            int prosperityDelta = scope.Context.CurrentXun switch
            {
                SimulationXun.Shangxun => scope.Context.Random.NextInt(0, 2),
                SimulationXun.Zhongxun => scope.Context.Random.NextInt(-1, 2),
                SimulationXun.Xiaxun => scope.Context.Random.NextInt(-1, 1),
                _ => 0,
            };

            settlement.Security = Math.Clamp(settlement.Security + securityDelta, 0, 100);
            settlement.Prosperity = Math.Clamp(settlement.Prosperity + prosperityDelta, 0, 100);

            if (settlement.Security == oldSecurity && settlement.Prosperity == oldProsperity)
            {
                continue;
            }

            scope.RecordDiff(
                $"{settlement.Name}{DescribeXun(scope.Context.CurrentXun)}乡面短波有变，安宁至{settlement.Security}，丰实至{settlement.Prosperity}。",
                settlement.Id.Value.ToString());
        }
    }

    public override void RunMonth(ModuleExecutionScope<WorldSettlementsState> scope)
    {
        // SPEC §20.3: pulse signals have tick lifetime — reset before this
        // tick's emissions.
        scope.State.CurrentPulseSignals.Clear();

        // SPATIAL_SKELETON_SPEC §3.2 — month advances the slow axes and
        // returns a transition report so we can derive public-surface
        // signals that need settlement-graph context (canal junction, ferry,
        // temple nodes) which the advancer does not hold.
        SeasonBandAdvancer.MonthAdvanceReport report =
            SeasonBandAdvancer.AdvanceMonth(scope.State.CurrentSeason, scope);

        // SPEC §22.1 / STATIC_BACKEND_FIRST.md — one live pressure chain,
        // in-module only: routes react to the season band their own module
        // just advanced. Must run after SeasonBandAdvancer so routes see the
        // new CanalWindow / FloodRisk.
        RouteAdvancer.AdvanceMonth(scope.State, scope);

        EmitCanalAndFloodSignals(scope.State, report);
    }

    /// <summary>
    /// SPATIAL_SKELETON_SPEC §20.3 Phase 1c minimum signal emission.
    /// State transitions on the season band have already fired their
    /// authoritative domain events inside <see cref="SeasonBandAdvancer"/>;
    /// here we project them into one or more <see cref="PublicSurfaceSignal"/>s
    /// by selecting the relevant settlement nodes from <see cref="WorldSettlementsState.Settlements"/>.
    ///
    /// <para>Canal transition → one <see cref="OpinionChannel.NoticeBoard"/>
    /// signal on any <see cref="SettlementNodeKind.CanalJunction"/>.</para>
    ///
    /// <para>Flood breach → three concurrent signals (NoticeBoard / MarketTalk
    /// / TempleWhisper) — the canonical stream-competition demonstration.</para>
    /// </summary>
    private static void EmitCanalAndFloodSignals(WorldSettlementsState state, SeasonBandAdvancer.MonthAdvanceReport report)
    {
        if (report.CanalWindowChanged)
        {
            foreach (SettlementStateData canalJunction in state.Settlements
                .Where(static node => node.NodeKind == SettlementNodeKind.CanalJunction)
                .OrderBy(static node => node.Id.Value))
            {
                PublicSurfaceSignalEmitter.EmitCanalWindowChanged(
                    state.CurrentPulseSignals,
                    canalJunction,
                    report.CanalFrom,
                    report.CanalTo);
            }
        }

        if (report.FloodRiskBreached)
        {
            SettlementStateData? canalJunction = state.Settlements
                .Where(static node => node.NodeKind == SettlementNodeKind.CanalJunction)
                .OrderBy(static node => node.Id.Value)
                .FirstOrDefault();
            SettlementStateData? ferry = state.Settlements
                .Where(static node => node.NodeKind == SettlementNodeKind.Ferry)
                .OrderBy(static node => node.Id.Value)
                .FirstOrDefault();
            SettlementStateData? temple = state.Settlements
                .Where(static node => node.NodeKind == SettlementNodeKind.Temple)
                .OrderBy(static node => node.Id.Value)
                .FirstOrDefault();

            PublicSurfaceSignalEmitter.EmitFloodRiskBreach(
                state.CurrentPulseSignals,
                report.FloodRisk,
                canalJunction,
                ferry,
                temple);
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<WorldSettlementsState> scope)
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

            SettlementStateData settlement = scope.State.Settlements.SingleOrDefault(existing => existing.Id == bundle.SettlementId)
                ?? throw new InvalidOperationException($"Settlement {bundle.SettlementId.Value} was not found for warfare fallout.");
            int previousSecurity = settlement.Security;
            int previousProsperity = settlement.Prosperity;

            int securityDelta = ComputeCampaignSecurityDelta(bundle, campaign);
            int prosperityDelta = ComputeCampaignProsperityDelta(bundle, campaign);

            settlement.Security = Math.Clamp(settlement.Security - securityDelta, 0, 100);
            settlement.Prosperity = Math.Clamp(settlement.Prosperity - prosperityDelta, 0, 100);

            if (previousSecurity == settlement.Security && previousProsperity == settlement.Prosperity)
            {
                continue;
            }

            scope.RecordDiff(
                $"{settlement.Name}受战后余波所压，安宁减{securityDelta}，丰实减{prosperityDelta}；{campaign.FrontLabel}、{campaign.SupplyStateLabel}，{campaign.LastAftermathSummary}",
                settlement.Id.Value.ToString());
            scope.Emit(WorldSettlementsEventNames.SettlementPressureChanged, $"{settlement.Name}受战后余波，乡面气象有变。", settlement.Id.Value.ToString());
        }
    }

    private static int ComputeCampaignSecurityDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignPressureRaised ? 2 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 60) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignProsperityDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignSupplyStrained ? 3 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, 55 - campaign.SupplyState) / 16;
        return Math.Max(1, delta);
    }

    private static string DescribeXun(SimulationXun xun)
    {
        return xun switch
        {
            SimulationXun.Shangxun => "上旬",
            SimulationXun.Zhongxun => "中旬",
            SimulationXun.Xiaxun => "下旬",
            _ => "月内",
        };
    }

    private sealed class WorldSettlementsQueries : IWorldSettlementsQueries
    {
        private readonly WorldSettlementsState _state;

        public WorldSettlementsQueries(WorldSettlementsState state)
        {
            _state = state;
        }

        public SettlementSnapshot GetRequiredSettlement(SettlementId settlementId)
        {
            SettlementStateData settlement = _state.Settlements
                .Single(settlement => settlement.Id == settlementId);

            return Clone(settlement);
        }

        public IReadOnlyList<SettlementSnapshot> GetSettlements()
        {
            return _state.Settlements
                .OrderBy(static settlement => settlement.Id.Value)
                .Select(Clone)
                .ToArray();
        }

        public IReadOnlyList<SettlementSnapshot> GetSettlementsByNodeKind(SettlementNodeKind kind)
        {
            return _state.Settlements
                .Where(settlement => settlement.NodeKind == kind)
                .OrderBy(static settlement => settlement.Id.Value)
                .Select(Clone)
                .ToArray();
        }

        public IReadOnlyList<SettlementSnapshot> GetSettlementsByVisibility(NodeVisibility visibility)
        {
            return _state.Settlements
                .Where(settlement => settlement.Visibility == visibility)
                .OrderBy(static settlement => settlement.Id.Value)
                .Select(Clone)
                .ToArray();
        }

        public IReadOnlyList<RouteSnapshot> GetRoutes()
        {
            return _state.Routes
                .OrderBy(static route => route.Id.Value)
                .Select(CloneRoute)
                .ToArray();
        }

        public IReadOnlyList<RouteSnapshot> GetRoutesByKind(RouteKind kind)
        {
            return _state.Routes
                .Where(route => route.Kind == kind)
                .OrderBy(static route => route.Id.Value)
                .Select(CloneRoute)
                .ToArray();
        }

        public IReadOnlyList<RouteSnapshot> GetRoutesByLegitimacy(RouteLegitimacy legitimacy)
        {
            return _state.Routes
                .Where(route => route.Legitimacy == legitimacy)
                .OrderBy(static route => route.Id.Value)
                .Select(CloneRoute)
                .ToArray();
        }

        public IReadOnlyList<RouteSnapshot> GetRoutesTouching(SettlementId settlementId)
        {
            return _state.Routes
                .Where(route =>
                    route.Origin == settlementId
                    || route.Destination == settlementId
                    || route.Waypoints.Contains(settlementId))
                .OrderBy(static route => route.Id.Value)
                .Select(CloneRoute)
                .ToArray();
        }

        public SeasonBandSnapshot GetCurrentSeason() => CloneSeason(_state.CurrentSeason);

        // SPATIAL_SKELETON_SPEC §6.4 / §8 — deterministic locus cascade.
        // Pure function of current state; see LocusScorer for the priority
        // ladder. Null only when the seed has produced no settlements yet.
        public LocusSnapshot? GetCurrentLocus() => LocusScorer.Score(_state);

        public IReadOnlyList<PublicSurfaceSignal> GetCurrentPulseSignals()
        {
            // Snapshot-copy the tick-lifetime buffer so callers cannot mutate
            // the authoritative list, and so a mid-tick consumer reads a
            // stable value. List is already in deterministic emission order.
            return _state.CurrentPulseSignals.ToArray();
        }

        private static SettlementSnapshot Clone(SettlementStateData settlement)
        {
            return new SettlementSnapshot
            {
                Id = settlement.Id,
                Name = settlement.Name,
                Tier = settlement.Tier,
                NodeKind = settlement.NodeKind,
                Visibility = settlement.Visibility,
                EcoZone = settlement.EcoZone,
                Security = settlement.Security,
                Prosperity = settlement.Prosperity,
                HealerAccess = settlement.HealerAccess,
                TempleHealingPresence = settlement.TempleHealingPresence,
                GranaryTrust = settlement.GranaryTrust,
                ReliefReach = settlement.ReliefReach,
            };
        }

        private static RouteSnapshot CloneRoute(RouteStateData route)
        {
            return new RouteSnapshot
            {
                Id = route.Id,
                Kind = route.Kind,
                Medium = route.Medium,
                Legitimacy = route.Legitimacy,
                ComplianceMode = route.ComplianceMode,
                Origin = route.Origin,
                Destination = route.Destination,
                // Defensive copy: consumers must not see internal list.
                Waypoints = route.Waypoints.ToArray(),
                TravelDaysBand = route.TravelDaysBand,
                Capacity = route.Capacity,
                Reliability = route.Reliability,
                SeasonalVulnerability = route.SeasonalVulnerability,
                CurrentConstraintLabel = route.CurrentConstraintLabel,
            };
        }

        private static SeasonBandSnapshot CloneSeason(SeasonBandData season)
        {
            return new SeasonBandSnapshot
            {
                AsOf = season.AsOf,
                AgrarianPhase = season.AgrarianPhase,
                LaborPinch = season.LaborPinch,
                HarvestWindowProgress = season.HarvestWindowProgress,
                WaterControlConfidence = season.WaterControlConfidence,
                EmbankmentStrain = season.EmbankmentStrain,
                FloodRisk = season.FloodRisk,
                CanalWindow = season.CanalWindow,
                MarketCadencePulse = season.MarketCadencePulse,
                CorveeWindow = season.CorveeWindow,
                MessageDelayBand = season.MessageDelayBand,
                Imperial = new ImperialBandSnapshot
                {
                    MourningInterruption = season.Imperial.MourningInterruption,
                    AmnestyWave = season.Imperial.AmnestyWave,
                    SuccessionUncertainty = season.Imperial.SuccessionUncertainty,
                    MandateConfidence = season.Imperial.MandateConfidence,
                    CourtTimeDisruption = season.Imperial.CourtTimeDisruption,
                },
            };
        }
    }
}
