using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class HallDocketLaneKeys
{
    public const string Family = "Family";
    public const string Governance = "Governance";
    public const string Warfare = "Warfare";
}

public static class HallDocketSourceProjectionKeys
{
    public const string Clans = "Clans";
    public const string GovernanceDocket = "GovernanceDocket";
    public const string Campaigns = "Campaigns";
    public const string Notifications = "Notifications";
    public const string PlayerCommandAffordances = "PlayerCommandAffordances";
    public const string PlayerCommandReceipts = "PlayerCommandReceipts";
}

public sealed class HallDocketItemSnapshot
{
    public string LaneKey { get; set; } = string.Empty;

    public SettlementId SettlementId { get; set; }

    public ClanId? ClanId { get; set; }

    public string SettlementName { get; set; } = string.Empty;

    public string NodeLabel { get; set; } = string.Empty;

    public string TargetLabel { get; set; } = string.Empty;

    public int UrgencyScore { get; set; }

    public string OrderingSummary { get; set; } = string.Empty;

    public string Headline { get; set; } = string.Empty;

    public string WhyNowSummary { get; set; } = string.Empty;

    public string PhaseLabel { get; set; } = string.Empty;

    public string PhaseSummary { get; set; } = string.Empty;

    public string HandlingSummary { get; set; } = string.Empty;

    public string GuidanceSummary { get; set; } = string.Empty;

    public string SuggestedCommandName { get; set; } = string.Empty;

    public string SuggestedCommandLabel { get; set; } = string.Empty;

    public string SuggestedCommandPrompt { get; set; } = string.Empty;

    public IReadOnlyList<string> SourceProjectionKeys { get; set; } = [];

    public IReadOnlyList<string> SourceModuleKeys { get; set; } = [];
}

public sealed class HallDocketStackSnapshot
{
    public HallDocketItemSnapshot LeadItem { get; set; } = new();

    public IReadOnlyList<HallDocketItemSnapshot> SecondaryItems { get; set; } = [];
}
