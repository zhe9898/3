using System.Collections.Generic;

namespace Zongzu.Contracts;

public static class PostMvpPreflightSeams
{
    public static IReadOnlyList<string> BlackRouteOwnerModuleKeys { get; } =
    [
        KnownModuleKeys.OrderAndBanditry,
        KnownModuleKeys.TradeAndIndustry,
    ];

    public static IReadOnlyList<string> BlackRoutePressureUpstreamModuleKeys { get; } =
    [
        KnownModuleKeys.OrderAndBanditry,
        KnownModuleKeys.ConflictAndForce,
        KnownModuleKeys.OfficeAndCareer,
    ];

    public static IReadOnlyList<string> BlackRouteLedgerOwnerModuleKeys { get; } =
    [
        KnownModuleKeys.TradeAndIndustry,
    ];

    public static IReadOnlyList<string> WarfareCampaignUpstreamModuleKeys { get; } =
    [
        KnownModuleKeys.ConflictAndForce,
        KnownModuleKeys.WorldSettlements,
        KnownModuleKeys.OfficeAndCareer,
    ];

    public static IReadOnlyList<string> WarfareCampaignMigrationOwnerModuleKeys { get; } =
    [
        KnownModuleKeys.WarfareCampaign,
    ];
}
