using System;
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
    public const string GrantClanRelief = "GrantClanRelief";
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
    public const string RepairLocalWatchGuarantee = "RepairLocalWatchGuarantee";
    public const string AskClanEldersExplain = "AskClanEldersExplain";
    public const string PressCountyYamenDocument = "PressCountyYamenDocument";
    public const string CompensateRunnerMisread = "CompensateRunnerMisread";
    public const string DeferHardPressure = "DeferHardPressure";
    public const string RedirectRoadReport = "RedirectRoadReport";
    public const string RestrictNightTravel = "RestrictNightTravel";
    public const string PoolRunnerCompensation = "PoolRunnerCompensation";
    public const string SendHouseholdRoadMessage = "SendHouseholdRoadMessage";
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

public sealed record PlayerCommandAffordanceSnapshot
{
    public string ModuleKey { get; init; } = string.Empty;

    public string SurfaceKey { get; init; } = string.Empty;

    public SettlementId SettlementId { get; init; }

    public ClanId? ClanId { get; init; }

    public string CommandName { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }

    public string AvailabilitySummary { get; init; } = string.Empty;

    public string ExecutionSummary { get; init; } = string.Empty;

    public string LeverageSummary { get; init; } = string.Empty;

    public string CostSummary { get; init; } = string.Empty;

    public string ReadbackSummary { get; init; } = string.Empty;

    public string PersonnelFlowReadinessSummary { get; init; } = string.Empty;

    public string FamilyLaneEntryReadbackSummary { get; init; } = string.Empty;

    public string FamilyElderExplanationReadbackSummary { get; init; } = string.Empty;

    public string FamilyGuaranteeReadbackSummary { get; init; } = string.Empty;

    public string FamilyHouseFaceReadbackSummary { get; init; } = string.Empty;

    public string FamilyLaneReceiptClosureSummary { get; init; } = string.Empty;

    public string FamilyLaneResidueFollowUpSummary { get; init; } = string.Empty;

    public string FamilyLaneNoLoopGuardSummary { get; init; } = string.Empty;

    public string WarfareLaneEntryReadbackSummary { get; init; } = string.Empty;

    public string ForceReadinessReadbackSummary { get; init; } = string.Empty;

    public string CampaignAftermathReadbackSummary { get; init; } = string.Empty;

    public string WarfareLaneReceiptClosureSummary { get; init; } = string.Empty;

    public string WarfareLaneResidueFollowUpSummary { get; init; } = string.Empty;

    public string WarfareLaneNoLoopGuardSummary { get; init; } = string.Empty;

    public string TargetLabel { get; init; } = string.Empty;
}

public sealed record PlayerCommandReceiptSnapshot
{
    public string ModuleKey { get; init; } = string.Empty;

    public string SurfaceKey { get; init; } = string.Empty;

    public SettlementId SettlementId { get; init; }

    public ClanId? ClanId { get; init; }

    public string CommandName { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string OutcomeSummary { get; init; } = string.Empty;

    public string ExecutionSummary { get; init; } = string.Empty;

    public string LeverageSummary { get; init; } = string.Empty;

    public string CostSummary { get; init; } = string.Empty;

    public string ReadbackSummary { get; init; } = string.Empty;

    public string PersonnelFlowReadinessSummary { get; init; } = string.Empty;

    public string FamilyLaneEntryReadbackSummary { get; init; } = string.Empty;

    public string FamilyElderExplanationReadbackSummary { get; init; } = string.Empty;

    public string FamilyGuaranteeReadbackSummary { get; init; } = string.Empty;

    public string FamilyHouseFaceReadbackSummary { get; init; } = string.Empty;

    public string FamilyLaneReceiptClosureSummary { get; init; } = string.Empty;

    public string FamilyLaneResidueFollowUpSummary { get; init; } = string.Empty;

    public string FamilyLaneNoLoopGuardSummary { get; init; } = string.Empty;

    public string WarfareLaneEntryReadbackSummary { get; init; } = string.Empty;

    public string ForceReadinessReadbackSummary { get; init; } = string.Empty;

    public string CampaignAftermathReadbackSummary { get; init; } = string.Empty;

    public string WarfareLaneReceiptClosureSummary { get; init; } = string.Empty;

    public string WarfareLaneResidueFollowUpSummary { get; init; } = string.Empty;

    public string WarfareLaneNoLoopGuardSummary { get; init; } = string.Empty;

    public string TargetLabel { get; init; } = string.Empty;
}

public sealed record PlayerCommandSurfaceSnapshot
{
    public IReadOnlyList<PlayerCommandAffordanceSnapshot> Affordances { get; init; } = [];

    public IReadOnlyList<PlayerCommandReceiptSnapshot> Receipts { get; init; } = [];

    public string PersonnelFlowReadinessSummary { get; init; } = string.Empty;

    public string PersonnelFlowOwnerLaneGateSummary { get; init; } = string.Empty;

    public IEnumerable<PlayerCommandAffordanceSnapshot> EnumerateAffordances(
        string surfaceKey,
        SettlementId? settlementId = null,
        bool enabledOnly = false)
    {
        if (string.IsNullOrWhiteSpace(surfaceKey))
        {
            yield break;
        }

        foreach (PlayerCommandAffordanceSnapshot affordance in Affordances)
        {
            if (!string.Equals(affordance.SurfaceKey, surfaceKey, StringComparison.Ordinal))
            {
                continue;
            }

            if (settlementId.HasValue && affordance.SettlementId != settlementId.Value)
            {
                continue;
            }

            if (enabledOnly && !affordance.IsEnabled)
            {
                continue;
            }

            yield return affordance;
        }
    }

    public IEnumerable<PlayerCommandReceiptSnapshot> EnumerateReceipts(
        string surfaceKey,
        SettlementId? settlementId = null)
    {
        if (string.IsNullOrWhiteSpace(surfaceKey))
        {
            yield break;
        }

        foreach (PlayerCommandReceiptSnapshot receipt in Receipts)
        {
            if (!string.Equals(receipt.SurfaceKey, surfaceKey, StringComparison.Ordinal))
            {
                continue;
            }

            if (settlementId.HasValue && receipt.SettlementId != settlementId.Value)
            {
                continue;
            }

            yield return receipt;
        }
    }
}
