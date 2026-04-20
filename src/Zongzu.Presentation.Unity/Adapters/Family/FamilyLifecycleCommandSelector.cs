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
			orderby GetLifecyclePriority(affordance), clan.ClanName
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
			.OrderBy(GetLifecyclePriority)
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

	private static int GetLifecyclePriority(PlayerCommandAffordanceSnapshot command)
	{
		return command.CommandName switch
		{
			PlayerCommandNames.SetMourningOrder => 0,
			PlayerCommandNames.SupportNewbornCare => 1,
			PlayerCommandNames.DesignateHeirPolicy => 2,
			PlayerCommandNames.ArrangeMarriage => 3,
			_ => 10
		};
	}
}
