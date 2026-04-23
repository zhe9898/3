using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class FamilyLifecycleCommandSelector
{
	internal static PlayerCommandAffordanceSnapshot? SelectLeadLifecycleAffordance(
		IReadOnlyList<ClanSnapshot> clans,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		return (from clan in clans
			let affordance = SelectPrimaryLifecycleAffordance(clan, affordances)
			where affordance != null
			orderby GetLifecyclePriority(clan, affordance), clan.ClanName
			select affordance)
			.FirstOrDefault();
	}

	internal static PlayerCommandAffordanceSnapshot? SelectPrimaryLifecycleAffordance(
		ClanSnapshot clan,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
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

	private static bool IsLifecycleFamilyCommand(string commandName)
	{
		return commandName is
			PlayerCommandNames.ArrangeMarriage or
			PlayerCommandNames.SupportNewbornCare or
			PlayerCommandNames.DesignateHeirPolicy or
			PlayerCommandNames.SetMourningOrder;
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
			_ => 10
		};
	}
}
