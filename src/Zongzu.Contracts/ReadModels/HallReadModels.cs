using System.Collections.Generic;
using Zongzu.Kernel;

using System;

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

public sealed record HallDocketItemSnapshot
{
    public string LaneKey { get; init; } = string.Empty;

    public SettlementId SettlementId { get; init; }

    public ClanId? ClanId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public string NodeLabel { get; init; } = string.Empty;

    public string TargetLabel { get; init; } = string.Empty;

    public int UrgencyScore { get; init; }

    public string OrderingSummary { get; init; } = string.Empty;

    public string Headline { get; init; } = string.Empty;

    public string WhyNowSummary { get; init; } = string.Empty;

    public string PhaseLabel { get; init; } = string.Empty;

    public string PhaseSummary { get; init; } = string.Empty;

    public string HandlingSummary { get; init; } = string.Empty;

    public string GuidanceSummary { get; init; } = string.Empty;

    public string SuggestedCommandName { get; init; } = string.Empty;

    public string SuggestedCommandLabel { get; init; } = string.Empty;

    public string SuggestedCommandPrompt { get; init; } = string.Empty;

    public IReadOnlyList<string> SourceProjectionKeys { get; init; } = [];

    public IReadOnlyList<string> SourceModuleKeys { get; init; } = [];
}

public sealed record HallDocketStackSnapshot
{
    public HallDocketItemSnapshot LeadItem { get; init; } = new();

    public IReadOnlyList<HallDocketItemSnapshot> SecondaryItems { get; init; } = [];

    public bool HasLeadItem => !string.IsNullOrWhiteSpace(LeadItem.Headline);

    public IEnumerable<HallDocketItemSnapshot> EnumeratePresentItems()
    {
        if (HasLeadItem)
        {
            yield return LeadItem;
        }

        foreach (HallDocketItemSnapshot item in SecondaryItems)
        {
            if (!string.IsNullOrWhiteSpace(item.Headline))
            {
                yield return item;
            }
        }
    }

    public bool HasLaneItem(string laneKey)
    {
        return TryGetLaneItem(laneKey) is not null;
    }

    public HallDocketItemSnapshot? TryGetLaneItem(string laneKey)
    {
        if (string.IsNullOrWhiteSpace(laneKey))
        {
            return null;
        }

        if (HasLeadItem && string.Equals(LeadItem.LaneKey, laneKey, StringComparison.Ordinal))
        {
            return LeadItem;
        }

        foreach (HallDocketItemSnapshot item in SecondaryItems)
        {
            if (!string.IsNullOrWhiteSpace(item.Headline)
                && string.Equals(item.LaneKey, laneKey, StringComparison.Ordinal))
            {
                return item;
            }
        }

        return null;
    }
}
