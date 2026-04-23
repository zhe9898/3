using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    internal static PlayerCommandAffordanceSnapshot? SelectPrimaryFamilyLifecycleAffordance(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
        IReadOnlyList<ClanSnapshot> clans)
    {
        Dictionary<int, ClanSnapshot> clansById = clans
            .ToDictionary(static entry => entry.Id.Value, static entry => entry);

        return EnumerateAffordancesForSurface(affordances, PlayerCommandSurfaceKeys.Family)
            .Select(entry => new
            {
                Affordance = entry,
                Clan = entry.ClanId.HasValue && clansById.TryGetValue(entry.ClanId.Value.Value, out ClanSnapshot? clan)
                    ? clan
                    : null,
            })
            .Where(static entry =>
                entry.Clan is not null
                && IsFamilyLifecycleCommand(entry.Affordance.CommandName))
            .OrderBy(entry => GetFamilyLifecycleCommandPriority(entry.Clan!, entry.Affordance.CommandName))
            .ThenBy(static entry => entry.Affordance.TargetLabel, StringComparer.Ordinal)
            .Select(static entry => entry.Affordance)
            .FirstOrDefault();
    }

    internal static PlayerCommandReceiptSnapshot? SelectPrimaryFamilyLifecycleReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        ClanId? clanId)
    {
        if (!clanId.HasValue)
        {
            return null;
        }

        return EnumerateReceiptsForSurface(receipts, PlayerCommandSurfaceKeys.Family)
            .Where(receipt =>
                receipt.ClanId == clanId
                && IsFamilyLifecycleCommand(receipt.CommandName))
            .OrderBy(receipt => GetFamilyLifecycleCommandPriority(receipt.CommandName))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.OutcomeSummary))
            .FirstOrDefault();
    }

    private static Dictionary<int, T> IndexFirstBySettlement<T>(
        IEnumerable<T> values,
        Func<T, SettlementId> getSettlementId)
    {
        return values
            .GroupBy(value => getSettlementId(value).Value)
            .ToDictionary(static group => group.Key, static group => group.First());
    }

    private static HashSet<string> IndexEnabledAffordanceSurfaces(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
    {
        return affordances
            .Where(static affordance => affordance.IsEnabled && !string.IsNullOrWhiteSpace(affordance.SurfaceKey))
            .Select(static affordance => affordance.SurfaceKey)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool HasEnabledAffordanceForSurface(
        IReadOnlySet<string> enabledSurfaceKeys,
        string surfaceKey)
    {
        return !string.IsNullOrWhiteSpace(surfaceKey)
            && enabledSurfaceKeys.Contains(surfaceKey);
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> EnumerateAffordancesForSurface(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
        string surfaceKey,
        SettlementId? settlementId = null)
    {
        return affordances.Where(command =>
            command.IsEnabled
            && string.Equals(command.SurfaceKey, surfaceKey, StringComparison.Ordinal)
            && (!settlementId.HasValue || command.SettlementId == settlementId.Value));
    }

    private static IEnumerable<PlayerCommandReceiptSnapshot> EnumerateReceiptsForSurface(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        string surfaceKey,
        SettlementId? settlementId = null)
    {
        return receipts.Where(receipt =>
            string.Equals(receipt.SurfaceKey, surfaceKey, StringComparison.Ordinal)
            && (!settlementId.HasValue || receipt.SettlementId == settlementId.Value));
    }

    internal static NarrativeNotificationSnapshot? SelectPrimarySettlementNotification(
        IReadOnlyList<NarrativeNotificationSnapshot> notifications,
        SettlementId settlementId,
        Func<NarrativeNotificationSnapshot, int> priority,
        string? sourceModuleKey = null)
    {
        ArgumentNullException.ThrowIfNull(priority);

        string settlementKey = settlementId.Value.ToString();

        return notifications
            .Where(notification =>
                (string.IsNullOrWhiteSpace(sourceModuleKey)
                 || string.Equals(notification.SourceModuleKey, sourceModuleKey, StringComparison.Ordinal))
                && notification.Traces.Any(trace => string.Equals(trace.EntityKey, settlementKey, StringComparison.Ordinal)))
            .OrderBy(priority)
            .ThenBy(static notification => notification.Tier)
            .ThenByDescending(static notification => notification.CreatedAt.Year)
            .ThenByDescending(static notification => notification.CreatedAt.Month)
            .ThenByDescending(static notification => notification.Id.Value)
            .FirstOrDefault();
    }

    private static bool IsFamilyLifecycleCommand(string commandName)
    {
        return commandName is PlayerCommandNames.SetMourningOrder
            or PlayerCommandNames.SupportNewbornCare
            or PlayerCommandNames.DesignateHeirPolicy
            or PlayerCommandNames.ArrangeMarriage;
    }

    private static int GetFamilyLifecycleCommandPriority(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.SetMourningOrder => 0,
            PlayerCommandNames.SupportNewbornCare => 1,
            PlayerCommandNames.DesignateHeirPolicy => 2,
            PlayerCommandNames.ArrangeMarriage => 3,
            _ => 9,
        };
    }

    private static int GetFamilyLifecycleCommandPriority(ClanSnapshot clan, string commandName)
    {
        bool hasSuccessionGap = !clan.HeirPersonId.HasValue
            || clan.LastLifecycleTrace.Contains("承祧缺口3阶", StringComparison.Ordinal);

        if (commandName == PlayerCommandNames.DesignateHeirPolicy && hasSuccessionGap)
        {
            return 0;
        }

        return commandName switch
        {
            PlayerCommandNames.SetMourningOrder => hasSuccessionGap ? 1 : 0,
            PlayerCommandNames.SupportNewbornCare => 2,
            PlayerCommandNames.DesignateHeirPolicy => 3,
            PlayerCommandNames.ArrangeMarriage => 4,
            _ => 9,
        };
    }
}
