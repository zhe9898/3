using System;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static PlayerCommandAffordanceSnapshot BuildPlayerCommandAffordanceSnapshot(
        string commandName,
        SettlementId settlementId,
        string summary,
        bool isEnabled,
        string availabilitySummary,
        ClanId? clanId = null,
        string executionSummary = "",
        string targetLabel = "",
        string? labelOverride = null)
    {
        PlayerCommandRoute route = PlayerCommandCatalog.GetRequired(commandName);

        return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = route.ModuleKey,
            SurfaceKey = route.SurfaceKey,
            SettlementId = settlementId,
            ClanId = clanId,
            CommandName = commandName,
            Label = string.IsNullOrWhiteSpace(labelOverride) ? route.Label : labelOverride,
            Summary = summary,
            IsEnabled = isEnabled,
            AvailabilitySummary = availabilitySummary,
            ExecutionSummary = executionSummary,
            TargetLabel = targetLabel,
        };
    }

    private static PlayerCommandReceiptSnapshot BuildPlayerCommandReceiptSnapshot(
        string commandName,
        SettlementId settlementId,
        string summary,
        string outcomeSummary,
        ClanId? clanId = null,
        string executionSummary = "",
        string targetLabel = "",
        string? labelOverride = null)
    {
        PlayerCommandRoute route = PlayerCommandCatalog.GetRequired(commandName);

        return new PlayerCommandReceiptSnapshot
        {
            ModuleKey = route.ModuleKey,
            SurfaceKey = route.SurfaceKey,
            SettlementId = settlementId,
            ClanId = clanId,
            CommandName = commandName,
            Label = string.IsNullOrWhiteSpace(labelOverride) ? route.Label : labelOverride,
            Summary = summary,
            OutcomeSummary = outcomeSummary,
            ExecutionSummary = executionSummary,
            TargetLabel = targetLabel,
        };
    }
}
