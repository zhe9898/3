namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §2.1 decision B — routes are classified by
/// <b>social function</b>, not by road-segment geometry. The same physical
/// corridor can host multiple <see cref="RouteKind"/> entries (grain + market
/// + exam-travel sharing one road), each with its own <c>RouteState</c>.
///
/// Grey-zone entries (<see cref="SmugglingCorridor"/>, <see cref="FugitivePath"/>)
/// implement decision H — illicit traffic is part of the skeleton, not an
/// afterthought.
/// </summary>
public enum RouteKind
{
    Unknown = 0,

    // Legitimate social functions
    GrainRoute = 10,
    TaxRoute = 11,
    PetitionRoute = 12,
    OfficialDispatchRoute = 13,
    ExamTravelRoute = 14,
    MarketRoute = 15,
    MilitaryMoveRoute = 16,
    EscortRoute = 17,
    CorveeRoute = 18,

    // Grey / illicit (decision H)
    SmugglingCorridor = 200,
    FugitivePath = 210,
}
