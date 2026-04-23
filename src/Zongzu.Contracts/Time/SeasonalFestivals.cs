namespace Zongzu.Contracts;

/// <summary>
/// SPATIAL_SKELETON_SPEC §20.5 — the minimum festival set the Lanxi seed
/// world must recognize. On hit, the matching <see cref="OpinionChannel"/>
/// heat rises and <c>SeasonBand.MarketCadencePulse</c> bumps.
///
/// Keys are short lowercase strings matched against
/// <c>WorldSettlementsEventNames.SeasonalFestivalArrived</c> entity keys.
/// Month values are approximate calendar months; finer timing
/// (xun-of-month) is a module responsibility, not a contract.
/// </summary>
public static class SeasonalFestivals
{
    // Canonical keys (used as event EntityKey per SPEC §21.2)
    public const string Qingming = "qingming";
    public const string Duanwu = "duanwu";
    public const string Zhongyuan = "zhongyuan";
    public const string Qiushe = "qiushe";
    public const string Laba = "laba";

    /// <summary>Qingming (tomb-sweeping) — month 3 xun 3 or month 4 xun 1.</summary>
    public const int QingmingMonth = 3;

    /// <summary>Duanwu (dragon boat) — month 5.</summary>
    public const int DuanwuMonth = 5;

    /// <summary>Zhongyuan (ghost festival) — month 7.</summary>
    public const int ZhongyuanMonth = 7;

    /// <summary>Qiushe (autumn land-god rites before harvest) — month 8.</summary>
    public const int QiusheMonth = 8;

    /// <summary>Laba (twelfth-month porridge rites) — month 12.</summary>
    public const int LabaMonth = 12;
}
