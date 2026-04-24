using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal sealed class OfficeProjectionContext
{
	private OfficeProjectionContext()
	{
	}

	internal int AppointedCareerCount { get; private init; }

	internal int JurisdictionCount { get; private init; }

	internal int HighestBacklog { get; private init; }

	internal IReadOnlyDictionary<int, string> SettlementNames { get; private init; } = new Dictionary<int, string>();

	internal OfficeCareerSnapshot[] OrderedCareers { get; private init; } = [];

	internal JurisdictionAuthoritySnapshot[] OrderedJurisdictions { get; private init; } = [];

	internal PlayerCommandAffordanceSnapshot[] OrderedAffordances { get; private init; } = [];

	internal PlayerCommandReceiptSnapshot[] OrderedReceipts { get; private init; } = [];

	internal static OfficeProjectionContext Create(PresentationReadModelBundle bundle)
	{
		ArgumentNullException.ThrowIfNull(bundle);

		return new OfficeProjectionContext
		{
			AppointedCareerCount = bundle.OfficeCareers.Count(career => career.HasAppointment),
			JurisdictionCount = bundle.OfficeJurisdictions.Count,
			HighestBacklog = bundle.OfficeJurisdictions.Count == 0
				? 0
				: bundle.OfficeJurisdictions.Max(jurisdiction => jurisdiction.PetitionBacklog),
			SettlementNames = bundle.Settlements.ToDictionary(settlement => settlement.Id.Value, settlement => settlement.Name),
			OrderedCareers = bundle.OfficeCareers
				.OrderByDescending(career => career.HasAppointment)
				.ThenByDescending(career => career.AuthorityTier)
				.ThenBy(career => career.PersonId.Value)
				.ToArray(),
			OrderedJurisdictions = bundle.OfficeJurisdictions
				.OrderBy(jurisdiction => jurisdiction.SettlementId.Value)
				.ToArray(),
			OrderedAffordances = bundle.PlayerCommands
				.EnumerateAffordances(PlayerCommandSurfaceKeys.Office)
				.OrderBy(command => command.SettlementId.Value)
				.ThenBy(command => command.CommandName, StringComparer.Ordinal)
				.ToArray(),
			OrderedReceipts = bundle.PlayerCommands
				.EnumerateReceipts(PlayerCommandSurfaceKeys.Office)
				.OrderBy(receipt => receipt.SettlementId.Value)
				.ThenBy(receipt => receipt.CommandName, StringComparer.Ordinal)
				.ToArray()
		};
	}
}
