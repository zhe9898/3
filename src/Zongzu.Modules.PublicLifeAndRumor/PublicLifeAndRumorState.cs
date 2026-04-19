using System.Collections.Generic;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed class PublicLifeAndRumorState : IModuleStateDescriptor
{
    public string ModuleKey => KnownModuleKeys.PublicLifeAndRumor;

    public List<SettlementPublicLifeState> Settlements { get; set; } = new();
}

public sealed class SettlementPublicLifeState
{
    public SettlementId SettlementId { get; set; }

    public string SettlementName { get; set; } = string.Empty;

    public SettlementTier SettlementTier { get; set; }

    public string NodeLabel { get; set; } = string.Empty;

    public string DominantVenueLabel { get; set; } = string.Empty;

    public string DominantVenueCode { get; set; } = string.Empty;

    public string MonthlyCadenceCode { get; set; } = string.Empty;

    public string MonthlyCadenceLabel { get; set; } = string.Empty;

    public string CrowdMixLabel { get; set; } = string.Empty;

    public int StreetTalkHeat { get; set; }

    public int MarketBuzz { get; set; }

    public int NoticeVisibility { get; set; }

    public int RoadReportLag { get; set; }

    public int PrefectureDispatchPressure { get; set; }

    public int PublicLegitimacy { get; set; }

    public int DocumentaryWeight { get; set; }

    public int VerificationCost { get; set; }

    public int MarketRumorFlow { get; set; }

    public int CourierRisk { get; set; }

    public string OfficialNoticeLine { get; set; } = string.Empty;

    public string StreetTalkLine { get; set; } = string.Empty;

    public string RoadReportLine { get; set; } = string.Empty;

    public string PrefectureDispatchLine { get; set; } = string.Empty;

    public string ContentionSummary { get; set; } = string.Empty;

    public string PublicSummary { get; set; } = string.Empty;

    public string RouteReportSummary { get; set; } = string.Empty;

    public string CadenceSummary { get; set; } = string.Empty;

    public string ChannelSummary { get; set; } = string.Empty;

    public string LastPublicTrace { get; set; } = string.Empty;
}
