using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class PlayerCommandSurfaceKeys
{
    public const string Family = "Family";
    public const string PublicLife = "PublicLife";
    public const string Office = "Office";
    public const string Warfare = "Warfare";
}

public static class PlayerCommandNames
{
    public const string ArrangeMarriage = "ArrangeMarriage";
    public const string DesignateHeirPolicy = "DesignateHeirPolicy";
    public const string SupportNewbornCare = "SupportNewbornCare";
    public const string SetMourningOrder = "SetMourningOrder";
    public const string PetitionViaOfficeChannels = "PetitionViaOfficeChannels";
    public const string DeployAdministrativeLeverage = "DeployAdministrativeLeverage";
    public const string SupportSeniorBranch = "SupportSeniorBranch";
    public const string OrderFormalApology = "OrderFormalApology";
    public const string PermitBranchSeparation = "PermitBranchSeparation";
    public const string SuspendClanRelief = "SuspendClanRelief";
    public const string InviteClanEldersMediation = "InviteClanEldersMediation";
    public const string InviteClanEldersPubliclyBroker = "InviteClanEldersPubliclyBroker";
    public const string PostCountyNotice = "PostCountyNotice";
    public const string DispatchRoadReport = "DispatchRoadReport";
    public const string EscortRoadReport = "EscortRoadReport";
    public const string FundLocalWatch = "FundLocalWatch";
    public const string SuppressBanditry = "SuppressBanditry";
    public const string NegotiateWithOutlaws = "NegotiateWithOutlaws";
    public const string TolerateDisorder = "TolerateDisorder";
    public const string DraftCampaignPlan = WarfareCampaignCommandNames.DraftCampaignPlan;
    public const string CommitMobilization = WarfareCampaignCommandNames.CommitMobilization;
    public const string ProtectSupplyLine = WarfareCampaignCommandNames.ProtectSupplyLine;
    public const string WithdrawToBarracks = WarfareCampaignCommandNames.WithdrawToBarracks;
}

public sealed class PlayerCommandRequest
{
    public SettlementId SettlementId { get; set; }

    public ClanId? ClanId { get; set; }

    public string CommandName { get; set; } = string.Empty;
}

public sealed class PlayerCommandResult
{
    public bool Accepted { get; set; }

    public string ModuleKey { get; set; } = string.Empty;

    public string SurfaceKey { get; set; } = string.Empty;

    public SettlementId SettlementId { get; set; }

    public ClanId? ClanId { get; set; }

    public string CommandName { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string TargetLabel { get; set; } = string.Empty;
}

public sealed class PlayerCommandAffordanceSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public string SurfaceKey { get; set; } = string.Empty;

    public SettlementId SettlementId { get; set; }

    public ClanId? ClanId { get; set; }

    public string CommandName { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public string AvailabilitySummary { get; set; } = string.Empty;

    public string ExecutionSummary { get; set; } = string.Empty;

    public string TargetLabel { get; set; } = string.Empty;
}

public sealed class PlayerCommandReceiptSnapshot
{
    public string ModuleKey { get; set; } = string.Empty;

    public string SurfaceKey { get; set; } = string.Empty;

    public SettlementId SettlementId { get; set; }

    public ClanId? ClanId { get; set; }

    public string CommandName { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string OutcomeSummary { get; set; } = string.Empty;

    public string ExecutionSummary { get; set; } = string.Empty;

    public string TargetLabel { get; set; } = string.Empty;
}

public sealed class PlayerCommandSurfaceSnapshot
{
    public IReadOnlyList<PlayerCommandAffordanceSnapshot> Affordances { get; set; } = [];

    public IReadOnlyList<PlayerCommandReceiptSnapshot> Receipts { get; set; } = [];
}
