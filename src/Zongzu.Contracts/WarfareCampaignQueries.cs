using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class CampaignFrontSnapshot
{
    public CampaignId CampaignId { get; set; }

    public SettlementId AnchorSettlementId { get; set; }

    public string AnchorSettlementName { get; set; } = string.Empty;

    public string CampaignName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string ObjectiveSummary { get; set; } = string.Empty;

    public int MobilizedForceCount { get; set; }

    public int FrontPressure { get; set; }

    public string FrontLabel { get; set; } = string.Empty;

    public int SupplyState { get; set; }

    public string SupplyStateLabel { get; set; } = string.Empty;

    public int MoraleState { get; set; }

    public string MoraleStateLabel { get; set; } = string.Empty;

    public string CommandFitLabel { get; set; } = string.Empty;

    public string CommanderSummary { get; set; } = string.Empty;

    public string ActiveDirectiveCode { get; set; } = string.Empty;

    public string ActiveDirectiveLabel { get; set; } = string.Empty;

    public string ActiveDirectiveSummary { get; set; } = string.Empty;

    public string LastDirectiveTrace { get; set; } = string.Empty;

    public string MobilizationWindowLabel { get; set; } = string.Empty;

    public string SupplyLineSummary { get; set; } = string.Empty;

    public string OfficeCoordinationTrace { get; set; } = string.Empty;

    public string SourceTrace { get; set; } = string.Empty;

    public string LastAftermathSummary { get; set; } = string.Empty;

    public IReadOnlyList<CampaignRouteSnapshot> Routes { get; set; } = [];
}

public sealed class CampaignRouteSnapshot
{
    public string RouteLabel { get; set; } = string.Empty;

    public string RouteRole { get; set; } = string.Empty;

    public int Pressure { get; set; }

    public int Security { get; set; }

    public string FlowStateLabel { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}

public sealed class CampaignMobilizationSignalSnapshot
{
    public SettlementId SettlementId { get; set; }

    public string SettlementName { get; set; } = string.Empty;

    public int ResponseActivationLevel { get; set; }

    public int CommandCapacity { get; set; }

    public int Readiness { get; set; }

    public int AvailableForceCount { get; set; }

    public int OrderSupportLevel { get; set; }

    public int OfficeAuthorityTier { get; set; }

    public int AdministrativeLeverage { get; set; }

    public int PetitionBacklog { get; set; }

    public string CommandFitLabel { get; set; } = string.Empty;

    public string ActiveDirectiveCode { get; set; } = string.Empty;

    public string ActiveDirectiveLabel { get; set; } = string.Empty;

    public string ActiveDirectiveSummary { get; set; } = string.Empty;

    public string MobilizationWindowLabel { get; set; } = string.Empty;

    public string OfficeCoordinationTrace { get; set; } = string.Empty;

    public string SourceTrace { get; set; } = string.Empty;
}

public interface IWarfareCampaignQueries
{
    CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId);

    IReadOnlyList<CampaignFrontSnapshot> GetCampaigns();

    IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals();
}
