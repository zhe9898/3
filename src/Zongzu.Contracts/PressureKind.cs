namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §19.1 — cross-module shared concept. Nodes are
/// pressure sources, routes are pressure media, other modules are pressure
/// consumers.
///
/// <c>WorldSettlements</c> only <i>registers the enum</i>; it does not own
/// pressure values. Each <see cref="PressureKind"/> is contributed by one or
/// more domain modules (grain by <c>TradeAndIndustry</c> / <c>WorldSettlements</c>,
/// tax by <c>OfficeAndCareer</c>, etc.) and consumed via the future
/// <c>IPressureFlowQueries</c> (SPEC §19.2).
/// </summary>
public enum PressureKind
{
    Unknown = 0,

    GrainPressure = 10,
    TaxPressure = 20,
    CorveePressure = 30,
    BanditPressure = 40,
    FloodPressure = 50,
    InformationLag = 60,
    MarriagePressure = 70,
    EscortPressure = 80,
}
