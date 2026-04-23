using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class PublicLifeAndRumorEventNames
{
    public const string StreetTalkSurged = "StreetTalkSurged";

    public const string CountyGateCrowded = "CountyGateCrowded";

    public const string MarketBuzzRaised = "MarketBuzzRaised";

    public const string RoadReportDelayed = "RoadReportDelayed";

    public const string PrefectureDispatchPressed = "PrefectureDispatchPressed";

    /// <summary>
    /// Fired when public legitimacy shifts due to regime events, ritual claims,
    /// or policy outcomes (P5+). Owned by PublicLifeAndRumor as a downstream
    /// signal that other modules may subscribe to for deterministic responses.
    /// </summary>
    public const string PublicLegitimacyShifted = "PublicLifeAndRumor.PublicLegitimacyShifted";
}

public sealed record SettlementPublicLifeSnapshot
{
    public SettlementId SettlementId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public SettlementTier SettlementTier { get; init; }

    public string NodeLabel { get; init; } = string.Empty;

    public string DominantVenueLabel { get; init; } = string.Empty;

    public string DominantVenueCode { get; init; } = string.Empty;

    public string MonthlyCadenceCode { get; init; } = string.Empty;

    public string MonthlyCadenceLabel { get; init; } = string.Empty;

    public string CrowdMixLabel { get; init; } = string.Empty;

    public int StreetTalkHeat { get; init; }

    public int MarketBuzz { get; init; }

    public int NoticeVisibility { get; init; }

    public int RoadReportLag { get; init; }

    public int PrefectureDispatchPressure { get; init; }

    public int PublicLegitimacy { get; init; }

    public int DocumentaryWeight { get; init; }

    public int VerificationCost { get; init; }

    public int MarketRumorFlow { get; init; }

    public int CourierRisk { get; init; }

    public string OfficialNoticeLine { get; init; } = string.Empty;

    public string StreetTalkLine { get; init; } = string.Empty;

    public string RoadReportLine { get; init; } = string.Empty;

    public string PrefectureDispatchLine { get; init; } = string.Empty;

    public string ContentionSummary { get; init; } = string.Empty;

    public string PublicSummary { get; init; } = string.Empty;

    public string RouteReportSummary { get; init; } = string.Empty;

    public string CadenceSummary { get; init; } = string.Empty;

    public string ChannelSummary { get; init; } = string.Empty;

    public string LastPublicTrace { get; init; } = string.Empty;
}

public interface IPublicLifeAndRumorQueries
{
    SettlementPublicLifeSnapshot GetRequiredSettlementPublicLife(SettlementId settlementId);

    IReadOnlyList<SettlementPublicLifeSnapshot> GetSettlementPublicLife();
}
