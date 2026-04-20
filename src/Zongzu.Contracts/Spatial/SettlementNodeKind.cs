namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §1.1 decision A — node classification is
/// <b>functional semantics</b>, not geographic geometry. Each node earns its
/// place by owning a pressure / queue / report / conflict / public signal.
///
/// Independent from <see cref="SettlementTier"/>: a <c>MarketTown</c> NodeKind
/// may sit at Tier <c>MarketTown</c> or be <c>county-adjacent</c>.
/// </summary>
public enum SettlementNodeKind
{
    // Unknown sentinel — default for migrated-from-v1 entries.
    Unknown = 0,

    // Administrative / government
    PrefectureSeat = 10,
    CountySeat = 20,
    YamenPost = 30,
    RelayPost = 35,

    // Settlements
    MarketTown = 40,
    WalledTown = 45,
    Village = 50,
    EstateCluster = 55,
    FrontierCamp = 58,

    // Lineage / education
    LineageHall = 60,
    Academy = 65,
    VillageSchool = 66,

    // Religion
    Temple = 70,
    ShrineCourt = 71,
    HillShrine = 72,

    // Economy / logistics
    Granary = 80,
    Depot = 81,
    WellPost = 82,
    Ferry = 85,
    Wharf = 86,
    CanalJunction = 87,
    Bridge = 88,
    Ford = 89,

    // Military
    Garrison = 90,
    Pass = 95,
    BorderWatch = 96,

    // Covert (SPEC decision H — not every node is state-visible)
    CovertMeetPoint = 200,
    SmugglingCache = 210,
}
