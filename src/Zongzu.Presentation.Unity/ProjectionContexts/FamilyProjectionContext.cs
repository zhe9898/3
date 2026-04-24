using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal sealed class FamilyProjectionContext
{
	private FamilyProjectionContext()
	{
	}

	internal IReadOnlyDictionary<int, ClanNarrativeSnapshot> NarrativesByClan { get; private init; } = new Dictionary<int, ClanNarrativeSnapshot>();

	internal ClanSnapshot[] OrderedClans { get; private init; } = [];

	internal PlayerCommandAffordanceSnapshot[] OrderedAffordances { get; private init; } = [];

	internal PlayerCommandReceiptSnapshot[] OrderedReceipts { get; private init; } = [];

	internal int TenseClanCount { get; private init; }

	internal int SeparationClanCount { get; private init; }

	internal int MediationClanCount { get; private init; }

	internal int MarriageClanCount { get; private init; }

	internal int HeirRiskClanCount { get; private init; }

	internal int MourningClanCount { get; private init; }

	internal static FamilyProjectionContext Create(PresentationReadModelBundle bundle)
	{
		ArgumentNullException.ThrowIfNull(bundle);

		return new FamilyProjectionContext
		{
			NarrativesByClan = bundle.ClanNarratives.ToDictionary(narrative => narrative.ClanId.Value, narrative => narrative),
			OrderedClans = bundle.Clans
				.OrderBy(clan => clan.ClanName, StringComparer.Ordinal)
				.ToArray(),
			OrderedAffordances = bundle.PlayerCommands
				.EnumerateAffordances(PlayerCommandSurfaceKeys.Family)
				.OrderBy(command => command.TargetLabel, StringComparer.Ordinal)
				.ThenBy(command => command.CommandName, StringComparer.Ordinal)
				.ToArray(),
			OrderedReceipts = bundle.PlayerCommands
				.EnumerateReceipts(PlayerCommandSurfaceKeys.Family)
				.OrderBy(receipt => receipt.TargetLabel, StringComparer.Ordinal)
				.ThenBy(receipt => receipt.CommandName, StringComparer.Ordinal)
				.ToArray(),
			TenseClanCount = bundle.Clans.Count(clan => clan.BranchTension >= 55),
			SeparationClanCount = bundle.Clans.Count(clan => clan.SeparationPressure >= 35),
			MediationClanCount = bundle.Clans.Count(clan => clan.MediationMomentum >= 30),
			MarriageClanCount = bundle.Clans.Count(clan => clan.MarriageAlliancePressure >= 35),
			HeirRiskClanCount = bundle.Clans.Count(clan => !clan.HeirPersonId.HasValue || clan.HeirSecurity < 40),
			MourningClanCount = bundle.Clans.Count(clan => clan.MourningLoad > 0)
		};
	}
}
