namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §2.3 decision H — routes carry legitimacy separately
/// from geometry and function. A smuggling corridor may share a physical
/// valley with an official grain route; the two are distinct
/// <c>RouteState</c> entries with different legitimacy.
/// </summary>
public enum RouteLegitimacy
{
    Unknown = 0,

    /// <summary>State-sanctioned: grain routes, official dispatch, relay paths.</summary>
    Official = 1,

    /// <summary>State-tolerated: most market routes, escort routes.</summary>
    Tolerated = 2,

    /// <summary>Grey-zone: small-scale evasion, fugitive paths across back hills.</summary>
    GrayZone = 3,

    /// <summary>Explicitly forbidden: contraband salt, banned-goods smuggling.</summary>
    Illicit = 4,
}
