using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

/// <summary>
/// SPATIAL_SKELETON_SPEC §12 — the Lanxi seed world. Not "example data": this
/// is the minimal self-consistent Jiangnan water-network county used to verify
/// every contract in SPEC §1–§11 can actually be instantiated, and the world
/// the Phase 1c liveness tests (SPEC §22) run against.
///
/// <para>Populates nine named nodes (§12.2), five social-function routes
/// (§12.5), and the initial March-1200 season band (§12.6). Keeps the seed
/// separate from <c>SimulationBootstrapper.SeedMinimalWorld</c> so the
/// M0/M1 single-settlement baseline other module tests depend on stays
/// intact.</para>
/// </summary>
/// <remarks>
/// Adjacency (SPEC §12.4):
/// <code>
/// county ── market ── ferry ═══ south-village
///    │         │
///    │         └─── temple
///    ├─── zhang-hall
///    ├─── east-village ╌╌╌ salt-cache   (gray-zone)
///    └─── granary
/// </code>
/// The gray-zone link is recorded on both sides — the validator allows an
/// illicit route to skip water-crossing interface nodes (SPEC §2.6), but the
/// geometric neighbor list is symmetric.
/// </remarks>
public static class LanxiSeed
{
    /// <summary>Node handles exposed for tests / integrations that need to reference seed ids.</summary>
    public sealed record LanxiHandles(
        SettlementId County,
        SettlementId Market,
        SettlementId ZhangHall,
        SettlementId EastVillage,
        SettlementId SouthVillage,
        SettlementId Ferry,
        SettlementId CanalJunction,
        SettlementId Granary,
        SettlementId QingfengTemple,
        SettlementId SaltCache,
        RouteId GrainEast,
        RouteId GrainSouth,
        RouteId MarketMain,
        RouteId DispatchHub,
        RouteId SaltSmuggle);

    /// <summary>
    /// Populate <paramref name="state"/> with the §12 seed. Allocates ids
    /// from <paramref name="kernelState"/> (deterministic, gap-free).
    /// </summary>
    public static LanxiHandles Seed(WorldSettlementsState state, KernelState kernelState)
    {
        // ── 1. Allocate ids in a stable order ─────────────────────────────
        SettlementId county = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId market = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId zhangHall = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId eastVillage = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId southVillage = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId ferry = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId canalJunction = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId granary = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId temple = KernelIdAllocator.NextSettlement(kernelState);
        SettlementId saltCache = KernelIdAllocator.NextSettlement(kernelState);

        RouteId grainEast = KernelIdAllocator.NextRoute(kernelState);
        RouteId grainSouth = KernelIdAllocator.NextRoute(kernelState);
        RouteId marketMain = KernelIdAllocator.NextRoute(kernelState);
        RouteId dispatchHub = KernelIdAllocator.NextRoute(kernelState);
        RouteId saltSmuggle = KernelIdAllocator.NextRoute(kernelState);

        // ── 2. Nodes (SPEC §12.2) ─────────────────────────────────────────
        state.Settlements.Add(BuildNode(
            county, "兰溪县治", SettlementTier.CountySeat, SettlementNodeKind.CountySeat,
            NodeVisibility.StateVisible, security: 55, prosperity: 50,
            neighbors: new[] { market, zhangHall, eastVillage, granary },
            parent: null));

        state.Settlements.Add(BuildNode(
            market, "埠头镇", SettlementTier.MarketTown, SettlementNodeKind.MarketTown,
            NodeVisibility.StateVisible, security: 45, prosperity: 58,
            neighbors: new[] { county, ferry, temple, canalJunction },
            parent: county));

        state.Settlements.Add(BuildNode(
            zhangHall, "张氏祠堂", SettlementTier.VillageCluster, SettlementNodeKind.LineageHall,
            NodeVisibility.LocalKnown, security: 60, prosperity: 40,
            neighbors: new[] { county },
            parent: county));

        state.Settlements.Add(BuildNode(
            eastVillage, "东溪村", SettlementTier.VillageCluster, SettlementNodeKind.Village,
            NodeVisibility.StateVisible, security: 40, prosperity: 35,
            neighbors: new[] { county, saltCache },
            parent: county));

        state.Settlements.Add(BuildNode(
            southVillage, "南渡村", SettlementTier.VillageCluster, SettlementNodeKind.Village,
            NodeVisibility.StateVisible, security: 38, prosperity: 33,
            neighbors: new[] { ferry },
            parent: county));

        state.Settlements.Add(BuildNode(
            ferry, "南渡津", SettlementTier.VillageCluster, SettlementNodeKind.Ferry,
            NodeVisibility.StateVisible, security: 42, prosperity: 40,
            neighbors: new[] { market, southVillage, canalJunction },
            parent: county));

        // SPEC §12.2 canal-junction anchor — required so FloodRisk breach
        // signals reach all three streams (NoticeBoard via the junction,
        // MarketTalk via the ferry, TempleWhisper via Qingfeng temple),
        // and so §22.1 three-stream competition assertion can hold.
        state.Settlements.Add(BuildNode(
            canalJunction, "兰江闸口", SettlementTier.VillageCluster, SettlementNodeKind.CanalJunction,
            NodeVisibility.StateVisible, security: 40, prosperity: 42,
            neighbors: new[] { market, ferry },
            parent: county));

        state.Settlements.Add(BuildNode(
            granary, "常平仓", SettlementTier.CountySeat, SettlementNodeKind.Granary,
            NodeVisibility.StateVisible, security: 50, prosperity: 45,
            neighbors: new[] { county },
            parent: county));

        state.Settlements.Add(BuildNode(
            temple, "清风庙", SettlementTier.VillageCluster, SettlementNodeKind.Temple,
            NodeVisibility.StateVisible, security: 48, prosperity: 30,
            neighbors: new[] { market },
            parent: county));

        // Covert node (SPEC §12.2 decision H): no admin parent, not state-visible.
        state.Settlements.Add(BuildNode(
            saltCache, "芦滩盐窝", SettlementTier.VillageCluster, SettlementNodeKind.SmugglingCache,
            NodeVisibility.Covert, security: 25, prosperity: 35,
            neighbors: new[] { eastVillage },
            parent: null));

        // ── 3. Routes (SPEC §12.5) ────────────────────────────────────────
        state.Routes.Add(BuildRoute(
            grainEast, RouteKind.GrainRoute, RouteMedium.LandRoad,
            RouteLegitimacy.Official, ComplianceMode.PaperCompliance,
            origin: eastVillage, destination: granary, waypoints: null,
            travelDaysBand: 0, capacity: 50, reliability: 70, seasonalVuln: 30));

        // Grain-south crosses water — SPEC §2.6 requires the ferry waypoint.
        state.Routes.Add(BuildRoute(
            grainSouth, RouteKind.GrainRoute, RouteMedium.LandRoad,
            RouteLegitimacy.Official, ComplianceMode.PaperCompliance,
            origin: southVillage, destination: granary,
            waypoints: new[] { ferry, market },
            travelDaysBand: 1, capacity: 45, reliability: 55, seasonalVuln: 60));

        state.Routes.Add(BuildRoute(
            marketMain, RouteKind.MarketRoute, RouteMedium.WaterRiver,
            RouteLegitimacy.Tolerated, ComplianceMode.PaperCompliance,
            origin: market, destination: county, waypoints: null,
            travelDaysBand: 0, capacity: 70, reliability: 65, seasonalVuln: 45));

        state.Routes.Add(BuildRoute(
            dispatchHub, RouteKind.OfficialDispatchRoute, RouteMedium.LandRoad,
            RouteLegitimacy.Official, ComplianceMode.PaperCompliance,
            origin: county, destination: market, waypoints: null,
            travelDaysBand: 0, capacity: 40, reliability: 75, seasonalVuln: 25));

        // Illicit route (SPEC §2.6) — skips the ferry, validator must exempt it.
        state.Routes.Add(BuildRoute(
            saltSmuggle, RouteKind.SmugglingCorridor, RouteMedium.MountainPath,
            RouteLegitimacy.Illicit, ComplianceMode.PaperCompliance,
            origin: eastVillage, destination: saltCache, waypoints: null,
            travelDaysBand: 0, capacity: 15, reliability: 40, seasonalVuln: 50));

        // ── 4. Initial season band (SPEC §12.6, March 1200) ──────────────
        state.CurrentSeason = new SeasonBandData
        {
            AsOf = new GameDate(1200, 3),
            AgrarianPhase = AgrarianPhase.Sowing,
            LaborPinch = 55,
            HarvestWindowProgress = 0,
            WaterControlConfidence = 60,
            EmbankmentStrain = 30,
            FloodRisk = 35,
            CanalWindow = CanalWindow.Limited,
            MarketCadencePulse = 40,
            CorveeWindow = CorveeWindow.Quiet,
            MessageDelayBand = 1,
            Imperial = new ImperialBandData(),
        };

        return new LanxiHandles(
            county, market, zhangHall, eastVillage, southVillage, ferry,
            canalJunction, granary, temple, saltCache,
            grainEast, grainSouth, marketMain, dispatchHub, saltSmuggle);
    }

    private static SettlementStateData BuildNode(
        SettlementId id,
        string name,
        SettlementTier tier,
        SettlementNodeKind nodeKind,
        NodeVisibility visibility,
        int security,
        int prosperity,
        IReadOnlyList<SettlementId> neighbors,
        SettlementId? parent)
    {
        return new SettlementStateData
        {
            Id = id,
            Name = name,
            Tier = tier,
            NodeKind = nodeKind,
            Visibility = visibility,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = security,
            Prosperity = prosperity,
            BaselineInstitutionCount = 1,
            HealerAccess = DeriveHealerAccess(nodeKind, tier),
            TempleHealingPresence = DeriveTempleHealing(nodeKind, tier),
            GranaryTrust = DeriveGranaryTrust(nodeKind, tier),
            ReliefReach = DeriveReliefReach(nodeKind, tier),
            NeighborIds = new List<SettlementId>(neighbors),
            ParentAdministrativeId = parent,
        };
    }

    /// <summary>
    /// STEP2A / A0a — band 由 NodeKind + Tier 推断，而非同质数字
    /// （skill simulation-calibration）。州府级 MarketTown 才配 Renowned，
    /// 县治 / 本镇有 Local 坐堂医，村落大多 None/Itinerant，祠堂 / 私窝不看病。
    /// </summary>
    private static HealerAccess DeriveHealerAccess(SettlementNodeKind kind, SettlementTier tier)
    {
        return kind switch
        {
            SettlementNodeKind.PrefectureSeat => HealerAccess.Renowned,
            SettlementNodeKind.CountySeat => HealerAccess.Local,
            SettlementNodeKind.MarketTown => HealerAccess.Local,
            SettlementNodeKind.Ferry => HealerAccess.Itinerant,
            SettlementNodeKind.CanalJunction => HealerAccess.Itinerant,
            SettlementNodeKind.Temple => HealerAccess.Itinerant,
            SettlementNodeKind.Granary => HealerAccess.None,
            SettlementNodeKind.LineageHall => HealerAccess.None,
            SettlementNodeKind.Village => HealerAccess.None,
            SettlementNodeKind.SmugglingCache => HealerAccess.None,
            SettlementNodeKind.CovertMeetPoint => HealerAccess.None,
            _ => HealerAccess.None,
        };
    }

    /// <summary>
    /// STEP2A / A0b — 寺观 band 由 NodeKind 推断。寺观本身是
    /// Institutional 档，县治 / 市镇带 Lay 香火通道，村落 / 祠堂 / 私窝
    /// 多是 Folk / None（skill religion-temples-ritual-brokerage：
    /// 保留延误 / 安抚张力，不做第二家医院）。
    /// </summary>
    private static TempleHealingPresence DeriveTempleHealing(SettlementNodeKind kind, SettlementTier tier)
    {
        return kind switch
        {
            SettlementNodeKind.Temple => TempleHealingPresence.Institutional,
            SettlementNodeKind.ShrineCourt => TempleHealingPresence.Lay,
            SettlementNodeKind.HillShrine => TempleHealingPresence.Folk,
            SettlementNodeKind.PrefectureSeat => TempleHealingPresence.Lay,
            SettlementNodeKind.CountySeat => TempleHealingPresence.Lay,
            SettlementNodeKind.MarketTown => TempleHealingPresence.Lay,
            SettlementNodeKind.WalledTown => TempleHealingPresence.Lay,
            SettlementNodeKind.Village => TempleHealingPresence.Folk,
            SettlementNodeKind.EstateCluster => TempleHealingPresence.Folk,
            SettlementNodeKind.Ferry => TempleHealingPresence.Folk,
            SettlementNodeKind.Wharf => TempleHealingPresence.Folk,
            SettlementNodeKind.CanalJunction => TempleHealingPresence.Folk,
            SettlementNodeKind.LineageHall => TempleHealingPresence.None,
            SettlementNodeKind.Granary => TempleHealingPresence.None,
            SettlementNodeKind.SmugglingCache => TempleHealingPresence.None,
            SettlementNodeKind.CovertMeetPoint => TempleHealingPresence.None,
            _ => TempleHealingPresence.None,
        };
    }

    /// <summary>
    /// STEP2A / A0c — GranaryTrust（0–100）初值。仓本身 55（半信）；
    /// 县治与市镇 45（常见失望经验）；村落 30（遥远）；祠堂 / 私窝 20。
    /// 高不是好事——信任 + Stalled band 才是最伤的组合
    /// （skill disaster-famine-relief-granaries：赈济是政治）。
    /// </summary>
    private static int DeriveGranaryTrust(SettlementNodeKind kind, SettlementTier tier)
    {
        return kind switch
        {
            SettlementNodeKind.Granary => 55,
            SettlementNodeKind.PrefectureSeat => 50,
            SettlementNodeKind.CountySeat => 45,
            SettlementNodeKind.MarketTown => 40,
            SettlementNodeKind.WalledTown => 40,
            SettlementNodeKind.Ferry => 35,
            SettlementNodeKind.Wharf => 35,
            SettlementNodeKind.CanalJunction => 38,
            SettlementNodeKind.Temple => 30,
            SettlementNodeKind.Village => 30,
            SettlementNodeKind.EstateCluster => 28,
            SettlementNodeKind.LineageHall => 25,
            SettlementNodeKind.SmugglingCache => 10,
            SettlementNodeKind.CovertMeetPoint => 10,
            _ => 25,
        };
    }

    /// <summary>
    /// STEP2A / A0c — 赈济实到 band。平时 Stalled / Selective 最常见
    /// （skill disaster-famine-relief-granaries：吏胥把持、挑选性是
    /// 常态，OpenHand 是少数名臣窗口）。Granary / PrefectureSeat 略好，
    /// 村落 / 私窝多 None。
    /// </summary>
    private static ReliefReach DeriveReliefReach(SettlementNodeKind kind, SettlementTier tier)
    {
        return kind switch
        {
            SettlementNodeKind.Granary => ReliefReach.Selective,
            SettlementNodeKind.PrefectureSeat => ReliefReach.Selective,
            SettlementNodeKind.CountySeat => ReliefReach.Stalled,
            SettlementNodeKind.MarketTown => ReliefReach.Stalled,
            SettlementNodeKind.WalledTown => ReliefReach.Stalled,
            SettlementNodeKind.Ferry => ReliefReach.None,
            SettlementNodeKind.Wharf => ReliefReach.None,
            SettlementNodeKind.CanalJunction => ReliefReach.None,
            SettlementNodeKind.Temple => ReliefReach.None,
            SettlementNodeKind.ShrineCourt => ReliefReach.None,
            SettlementNodeKind.HillShrine => ReliefReach.None,
            SettlementNodeKind.Village => ReliefReach.None,
            SettlementNodeKind.EstateCluster => ReliefReach.None,
            SettlementNodeKind.LineageHall => ReliefReach.None,
            SettlementNodeKind.SmugglingCache => ReliefReach.None,
            SettlementNodeKind.CovertMeetPoint => ReliefReach.None,
            _ => ReliefReach.None,
        };
    }

    private static RouteStateData BuildRoute(
        RouteId id,
        RouteKind kind,
        RouteMedium medium,
        RouteLegitimacy legitimacy,
        ComplianceMode compliance,
        SettlementId origin,
        SettlementId destination,
        IReadOnlyList<SettlementId>? waypoints,
        int travelDaysBand,
        int capacity,
        int reliability,
        int seasonalVuln)
    {
        return new RouteStateData
        {
            Id = id,
            Kind = kind,
            Medium = medium,
            Legitimacy = legitimacy,
            ComplianceMode = compliance,
            Origin = origin,
            Destination = destination,
            Waypoints = waypoints is null
                ? new List<SettlementId>()
                : new List<SettlementId>(waypoints),
            TravelDaysBand = travelDaysBand,
            Capacity = capacity,
            Reliability = reliability,
            SeasonalVulnerability = seasonalVuln,
            CurrentConstraintLabel = string.Empty,
        };
    }
}
