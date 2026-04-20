using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class WarfareCampaignCommandNames
{
    public const string DraftCampaignPlan = "DraftCampaignPlan";
    public const string CommitMobilization = "CommitMobilization";
    public const string ProtectSupplyLine = "ProtectSupplyLine";
    public const string WithdrawToBarracks = "WithdrawToBarracks";
}

public sealed class WarfareCampaignIntentCommand
{
    public SettlementId SettlementId { get; set; }

    public string CommandName { get; set; } = string.Empty;
}

public sealed class WarfareCampaignIntentResult
{
    public bool Accepted { get; set; }

    public string DirectiveLabel { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}
