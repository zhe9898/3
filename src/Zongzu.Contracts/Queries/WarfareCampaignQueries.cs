using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed record CampaignFrontSnapshot
{
    public CampaignId CampaignId { get; init; }

    public SettlementId AnchorSettlementId { get; init; }

    public string AnchorSettlementName { get; init; } = string.Empty;

    public string CampaignName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public string ObjectiveSummary { get; init; } = string.Empty;

    public int MobilizedForceCount { get; init; }

    public int FrontPressure { get; init; }

    public string FrontLabel { get; init; } = string.Empty;

    public int SupplyState { get; init; }

    public string SupplyStateLabel { get; init; } = string.Empty;

    public int MoraleState { get; init; }

    public string MoraleStateLabel { get; init; } = string.Empty;

    public string CommandFitLabel { get; init; } = string.Empty;

    public string CommanderSummary { get; init; } = string.Empty;

    public string ActiveDirectiveCode { get; init; } = string.Empty;

    public string ActiveDirectiveLabel { get; init; } = string.Empty;

    public string ActiveDirectiveSummary { get; init; } = string.Empty;

    public string LastDirectiveTrace { get; init; } = string.Empty;

    public string MobilizationWindowLabel { get; init; } = string.Empty;

    public string SupplyLineSummary { get; init; } = string.Empty;

    public string OfficeCoordinationTrace { get; init; } = string.Empty;

    public string SourceTrace { get; init; } = string.Empty;

    public string LastAftermathSummary { get; init; } = string.Empty;

    public IReadOnlyList<CampaignRouteSnapshot> Routes { get; init; } = [];
}

public sealed record CampaignRouteSnapshot
{
    public string RouteLabel { get; init; } = string.Empty;

    public string RouteRole { get; init; } = string.Empty;

    public int Pressure { get; init; }

    public int Security { get; init; }

    public string FlowStateLabel { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;
}

public sealed record CampaignMobilizationSignalSnapshot
{
    public SettlementId SettlementId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public int ResponseActivationLevel { get; init; }

    public int CommandCapacity { get; init; }

    public int Readiness { get; init; }

    public int AvailableForceCount { get; init; }

    public int OrderSupportLevel { get; init; }

    public int OfficeAuthorityTier { get; init; }

    public int AdministrativeLeverage { get; init; }

    public int PetitionBacklog { get; init; }

    public string CommandFitLabel { get; init; } = string.Empty;

    public string ActiveDirectiveCode { get; init; } = string.Empty;

    public string ActiveDirectiveLabel { get; init; } = string.Empty;

    public string ActiveDirectiveSummary { get; init; } = string.Empty;

    public string MobilizationWindowLabel { get; init; } = string.Empty;

    public string OfficeCoordinationTrace { get; init; } = string.Empty;

    public string SourceTrace { get; init; } = string.Empty;
}

public interface IWarfareCampaignQueries
{
    CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId);

    IReadOnlyList<CampaignFrontSnapshot> GetCampaigns();

    IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals();
}
