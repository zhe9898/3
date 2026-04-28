using System;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class LineageShellAdapter
{
	internal static LineageSurfaceViewModel BuildLineage(PresentationReadModelBundle bundle, int? focusedPersonId = null)
	{
		ArgumentNullException.ThrowIfNull(bundle);

		ClanTileViewModel[] clans = bundle.Clans
			.OrderBy(clan => clan.ClanName, StringComparer.Ordinal)
			.Select(clan => new ClanTileViewModel
			{
				ClanName = clan.ClanName,
				Prestige = clan.Prestige,
				SupportReserve = clan.SupportReserve,
				StatusText = clan.HeirPersonId.HasValue ? "承祧之人已入谱。" : "宗房暂未举出承祧人。"
			})
			.ToArray();
		PersonDossierViewModel[] personDossiers = bundle.PersonDossiers
			.Select(MapPersonDossier)
			.ToArray();

		return new LineageSurfaceViewModel
		{
			Clans = clans,
			PersonDossiers = personDossiers,
			FocusedPerson = BuildFocusedPerson(SelectFocusedPerson(personDossiers, focusedPersonId))
		};
	}

	private static PersonDossierViewModel MapPersonDossier(PersonDossierSnapshot dossier)
	{
		return new PersonDossierViewModel
		{
			PersonId = dossier.PersonId.Value,
			DisplayName = dossier.DisplayName,
			LifeStage = dossier.LifeStage.ToString(),
			Gender = dossier.Gender.ToString(),
			IsAlive = dossier.IsAlive,
			FidelityRing = dossier.FidelityRing.ToString(),
			ClanId = dossier.ClanId?.Value,
			ClanName = dossier.ClanName,
			BranchPositionLabel = dossier.BranchPositionLabel,
			KinshipSummary = dossier.KinshipSummary,
			TemperamentSummary = dossier.TemperamentSummary,
			HouseholdId = dossier.HouseholdId?.Value,
			HouseholdName = dossier.HouseholdName,
			LivelihoodSummary = dossier.LivelihoodSummary,
			HealthSummary = dossier.HealthSummary,
			ActivitySummary = dossier.ActivitySummary,
			MovementReadbackSummary = dossier.MovementReadbackSummary,
			FidelityRingReadbackSummary = dossier.FidelityRingReadbackSummary,
			InfluenceFootprintReadbackSummary = dossier.InfluenceFootprintReadbackSummary,
			EducationSummary = dossier.EducationSummary,
			TradeSummary = dossier.TradeSummary,
			OfficeSummary = dossier.OfficeSummary,
			MemoryPressureSummary = dossier.MemoryPressureSummary,
			DormantMemorySummary = dossier.DormantMemorySummary,
			SocialPositionLabel = dossier.SocialPositionLabel,
			SocialPositionReadbackSummary = dossier.SocialPositionReadbackSummary,
			SocialPositionSourceModuleKeys = dossier.SocialPositionSourceModuleKeys.ToArray(),
			SocialPositionScaleBudgetReadbackSummary = dossier.SocialPositionScaleBudgetReadbackSummary,
			CurrentStatusSummary = dossier.CurrentStatusSummary,
			SourceModuleKeys = dossier.SourceModuleKeys.ToArray()
		};
	}

	private static PersonInspectorViewModel? BuildFocusedPerson(PersonDossierViewModel? dossier)
	{
		if (dossier is null)
		{
			return null;
		}

		return new PersonInspectorViewModel
		{
			ObjectAnchorLabel = "画像卷轴",
			TabletLabel = string.IsNullOrWhiteSpace(dossier.ClanName)
				? dossier.DisplayName
				: $"{dossier.ClanName} · {dossier.DisplayName}",
			PortraitScrollLine = $"{dossier.DisplayName} · {dossier.SocialPositionLabel}",
			KinshipThreadLine = dossier.KinshipSummary,
			LivelihoodThreadLine = dossier.LivelihoodSummary,
			EducationThreadLine = dossier.EducationSummary,
			OfficeThreadLine = dossier.OfficeSummary,
			MemoryThreadLine = BuildMemoryThreadLine(dossier),
			StatusLedgerLine = BuildStatusLedgerLine(dossier),
			Dossier = dossier,
			SourceModuleKeys = dossier.SourceModuleKeys.ToArray()
		};
	}

	private static string BuildStatusLedgerLine(PersonDossierViewModel dossier)
	{
		return string.Join(
			" ",
			new[]
			{
				dossier.CurrentStatusSummary,
				dossier.SocialPositionReadbackSummary,
				dossier.SocialPositionScaleBudgetReadbackSummary,
			}.Where(static line => !string.IsNullOrWhiteSpace(line)));
	}

	private static string BuildMemoryThreadLine(PersonDossierViewModel dossier)
	{
		return string.IsNullOrWhiteSpace(dossier.DormantMemorySummary) ||
			dossier.DormantMemorySummary == "No dormant social-memory stub."
				? dossier.MemoryPressureSummary
				: $"{dossier.MemoryPressureSummary}; {dossier.DormantMemorySummary}";
	}

	private static PersonDossierViewModel? SelectFocusedPerson(
		PersonDossierViewModel[] personDossiers,
		int? focusedPersonId)
	{
		if (personDossiers.Length == 0)
		{
			return null;
		}

		if (focusedPersonId.HasValue)
		{
			PersonDossierViewModel? requested = personDossiers
				.FirstOrDefault(dossier => dossier.PersonId == focusedPersonId.Value);
			if (requested is not null)
			{
				return requested;
			}
		}

		return personDossiers[0];
	}
}
