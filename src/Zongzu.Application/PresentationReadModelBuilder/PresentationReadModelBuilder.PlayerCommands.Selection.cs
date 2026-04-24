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
        return FamilyLifecycleProjectionSelectors.SelectLeadLifecycleAffordance(clans, affordances);
    }

    internal static PlayerCommandReceiptSnapshot? SelectPrimaryFamilyLifecycleReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        ClanId? clanId)
    {
        return FamilyLifecycleProjectionSelectors.SelectPrimaryLifecycleReceipt(receipts, clanId);
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
}
