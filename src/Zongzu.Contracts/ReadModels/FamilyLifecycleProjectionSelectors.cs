using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class FamilyLifecycleProjectionSelectors
{
    public static PlayerCommandAffordanceSnapshot? SelectLeadLifecycleAffordance(
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
    {
        return (from clan in clans
                let affordance = SelectPrimaryLifecycleAffordance(clan, affordances)
                where affordance is not null
                orderby GetLifecyclePriority(clan, affordance), clan.ClanName
                select affordance)
            .FirstOrDefault();
    }

    public static PlayerCommandAffordanceSnapshot? SelectPrimaryLifecycleAffordance(
        ClanSnapshot clan,
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
    {
        ArgumentNullException.ThrowIfNull(clan);
        ArgumentNullException.ThrowIfNull(affordances);

        return affordances
            .Where(command =>
                command.IsEnabled
                && command.ClanId.HasValue
                && command.ClanId.Value == clan.Id
                && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && IsLifecycleFamilyCommand(command.CommandName))
            .OrderBy(command => GetLifecyclePriority(clan, command))
            .ThenBy(command => command.CommandName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static PlayerCommandReceiptSnapshot? SelectPrimaryLifecycleReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        ClanId? clanId)
    {
        if (!clanId.HasValue)
        {
            return null;
        }

        return receipts
            .Where(receipt =>
                receipt.ClanId == clanId
                && string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && IsLifecycleFamilyCommand(receipt.CommandName))
            .OrderBy(receipt => GetLifecyclePriority(receipt.CommandName))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.OutcomeSummary))
            .FirstOrDefault();
    }

    private static bool IsLifecycleFamilyCommand(string commandName)
    {
        return commandName is
            PlayerCommandNames.ArrangeMarriage or
            PlayerCommandNames.SupportNewbornCare or
            PlayerCommandNames.DesignateHeirPolicy or
            PlayerCommandNames.SetMourningOrder;
    }

    private static int GetLifecyclePriority(string commandName)
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

    private static int GetLifecyclePriority(ClanSnapshot clan, PlayerCommandAffordanceSnapshot command)
    {
        bool hasSuccessionGap = !clan.HeirPersonId.HasValue
            || clan.LastLifecycleTrace.Contains("承祧缺口3阶", StringComparison.Ordinal);

        if (command.CommandName == PlayerCommandNames.DesignateHeirPolicy && hasSuccessionGap)
        {
            return 0;
        }

        return command.CommandName switch
        {
            PlayerCommandNames.SetMourningOrder => hasSuccessionGap ? 1 : 0,
            PlayerCommandNames.SupportNewbornCare => 2,
            PlayerCommandNames.DesignateHeirPolicy => 3,
            PlayerCommandNames.ArrangeMarriage => 4,
            _ => 10,
        };
    }
}
