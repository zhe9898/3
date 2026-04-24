using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class FamilyShellAdapter
{
	internal static string BuildLeadFamilyLifecyclePrompt(
		IReadOnlyList<ClanSnapshot> clans,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		PlayerCommandAffordanceSnapshot? affordance = FamilyLifecycleProjectionSelectors.SelectLeadLifecycleAffordance(clans, affordances);

		if (affordance == null)
		{
			return string.Empty;
		}

		return "眼下最宜先命" + affordance.TargetLabel + affordance.Label + "。";
	}

	internal static string BuildGreatHallFamilySummary(
		ClanSnapshot? clan,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		if (clan == null)
		{
			return "堂中暂无宗房呈报。";
		}

		string lifecyclePrompt = BuildClanLifecyclePrompt(clan, affordances, includeClanName: false);
		if (clan.MourningLoad > 0 && !clan.HeirPersonId.HasValue)
		{
			return $"{clan.ClanName}门望{clan.Prestige}，门内举哀未毕，承祧未稳，丧服之重{clan.MourningLoad}，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
		}

		if (clan.MourningLoad > 0)
		{
			return $"{clan.ClanName}门望{clan.Prestige}，门内举哀未毕，丧服之重{clan.MourningLoad}，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
		}

		if (clan.HeirPersonId.HasValue && clan.HeirSecurity >= 40)
		{
			if (clan.MarriageAlliancePressure < 35 && clan.ReproductivePressure < 40)
			{
				return $"{clan.ClanName}门望{clan.Prestige}，承祧稳度{clan.HeirSecurity}，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
			}

			return $"{clan.ClanName}门望{clan.Prestige}，婚议之压{clan.MarriageAlliancePressure}，添丁之望{clan.ReproductivePressure}，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
		}

		return $"{clan.ClanName}门望{clan.Prestige}，承祧未稳，后议之压在堂，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
	}

	internal static FamilyCouncilViewModel BuildFamilyCouncil(PresentationReadModelBundle bundle)
	{
		FamilyProjectionContext context = FamilyProjectionContext.Create(bundle);
		string lifecyclePrompt = BuildLeadFamilyLifecyclePrompt(context.OrderedClans, context.OrderedAffordances);

		return new FamilyCouncilViewModel
		{
			Summary = context.OrderedClans.Length == 0
				? "祠堂暂无线头可议。"
				: $"祠堂今有{context.TenseClanCount}宗争势渐炽，{context.SeparationClanCount}宗起分房之议，{context.MediationClanCount}宗可邀族老调停；另有{context.MarriageClanCount}宗急议婚事，{context.HeirRiskClanCount}宗承祧未稳，{context.MourningClanCount}宗门内举哀。{lifecyclePrompt}",
			CommandAffordances = CommandShellAdapter.BuildAffordances(context.OrderedAffordances),
			RecentReceipts = CommandShellAdapter.BuildReceipts(context.OrderedReceipts),
			Clans = context.OrderedClans
				.Select(clan =>
				{
					context.NarrativesByClan.TryGetValue(clan.Id.Value, out ClanNarrativeSnapshot? narrative);
					return new FamilyConflictTileViewModel
					{
						ClanName = clan.ClanName,
						Prestige = clan.Prestige,
						SupportReserve = clan.SupportReserve,
						ConflictSummary = BuildClanConflictSummary(clan),
						MemorySummary = BuildClanMemorySummary(narrative),
						LifecycleSummary = BuildClanLifecycleSummary(clan, context.OrderedAffordances),
						LastOrderSummary = BuildClanLastOrderSummary(clan)
					};
				})
				.ToArray()
		};
	}

	private static string BuildClanConflictSummary(ClanSnapshot clan)
	{
		if (clan.SeparationPressure >= 60)
		{
			return $"分房之议已炽；争势{clan.BranchTension}，分房之压{clan.SeparationPressure}。";
		}

		if (clan.BranchTension >= 55)
		{
			return $"祠堂争声已起；争势{clan.BranchTension}，承祧之压{clan.InheritancePressure}。";
		}

		if (clan.MediationMomentum >= 35)
		{
			return $"族老已可出面；调停之势{clan.MediationMomentum}。";
		}

		return $"房支尚可按住；争势{clan.BranchTension}，分房之压{clan.SeparationPressure}。";
	}

	private static string BuildClanMemorySummary(ClanNarrativeSnapshot? narrative)
	{
		if (narrative == null)
		{
			return "堂上暂无线性旧怨。";
		}

		if (!string.IsNullOrWhiteSpace(narrative.PublicNarrative))
		{
			return narrative.PublicNarrative;
		}

		return $"旧怨{narrative.GrudgePressure}，羞责{narrative.ShamePressure}，人情往还{narrative.FavorBalance}。";
	}

	private static string BuildClanLifecycleSummary(
		ClanSnapshot clan,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		string inheritanceSummary = clan.HeirPersonId.HasValue
			? $"承祧稳度{clan.HeirSecurity}"
			: $"堂上尚未定承祧，后议之压{clan.InheritancePressure}";
		string householdSummary = clan.MourningLoad > 0
			? $"门内仍系丧服之重{clan.MourningLoad}"
			: clan.InfantCount > 0
				? $"门内现有襁褓{clan.InfantCount}口，添丁之后仍待护持"
				: $"婚议之压{clan.MarriageAlliancePressure}，添丁之望{clan.ReproductivePressure}";
		string lifecyclePrompt = BuildClanLifecyclePrompt(clan, affordances, includeClanName: false);

		if (!string.IsNullOrWhiteSpace(clan.LastLifecycleOutcome))
		{
			string recentLabel = string.IsNullOrWhiteSpace(clan.LastLifecycleCommandLabel) ? "门内后计" : clan.LastLifecycleCommandLabel;
			return $"{inheritanceSummary}；{householdSummary}。近月{recentLabel}：{clan.LastLifecycleOutcome} {lifecyclePrompt}".Trim();
		}

		return $"{inheritanceSummary}；{householdSummary}。{lifecyclePrompt}".Trim();
	}

	private static string BuildClanLifecyclePrompt(
		ClanSnapshot clan,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
		bool includeClanName)
	{
		PlayerCommandAffordanceSnapshot? affordance = FamilyLifecycleProjectionSelectors.SelectPrimaryLifecycleAffordance(clan, affordances);
		if (affordance == null)
		{
			return string.Empty;
		}

		string prefix = includeClanName
			? "眼下最宜先命" + affordance.TargetLabel + affordance.Label
			: "眼下宜先" + affordance.Label;
		return prefix + "：" + affordance.AvailabilitySummary;
	}

	private static string BuildClanLastOrderSummary(ClanSnapshot clan)
	{
		if (string.IsNullOrWhiteSpace(clan.LastConflictOutcome))
		{
			return "本月尚无新议决。";
		}

		return clan.LastConflictCommandLabel + "：" + clan.LastConflictOutcome;
	}
}
