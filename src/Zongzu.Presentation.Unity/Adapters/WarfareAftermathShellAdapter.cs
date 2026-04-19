using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal static class WarfareAftermathShellAdapter
{
	private sealed class AftermathDocketSignals
	{
		public bool Merit { get; init; }

		public bool Blame { get; init; }

		public bool Relief { get; init; }

		public bool Disorder { get; init; }

		public bool HasSignals => Merit || Blame || Relief || Disorder;

		public string ComposeClauseText()
		{
			List<string> clauses = new List<string>();
			if (Merit)
			{
				clauses.Add("记功簿");
			}

			if (Blame)
			{
				clauses.Add("劾责状");
			}

			if (Relief)
			{
				clauses.Add("抚恤簿");
			}

			if (Disorder)
			{
				clauses.Add("清路札");
			}

			return string.Join("、", clauses);
		}
	}

	internal static string BuildGreatHallAftermathDocketSummary(
		PresentationReadModelBundle bundle,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(bundle);
		ArgumentNullException.ThrowIfNull(notifications);

		if (bundle.Campaigns.Count == 0)
		{
			return "堂上尚无战后案牍。";
		}

		CampaignFrontSnapshot leadCampaign = bundle.Campaigns
			.OrderByDescending(campaign => campaign.FrontPressure)
			.ThenByDescending(campaign => campaign.MobilizedForceCount)
			.ThenBy(campaign => campaign.CampaignId.Value)
			.First();
		SettlementSnapshot? settlement = bundle.Settlements
			.FirstOrDefault(candidate => candidate.Id == leadCampaign.AnchorSettlementId);
		PopulationSettlementSnapshot? population = bundle.PopulationSettlements
			.FirstOrDefault(candidate => candidate.SettlementId == leadCampaign.AnchorSettlementId);
		JurisdictionAuthoritySnapshot? jurisdiction = bundle.OfficeJurisdictions
			.FirstOrDefault(candidate => candidate.SettlementId == leadCampaign.AnchorSettlementId);
		AftermathDocketSignals signals = BuildAftermathDocketSignals(
			leadCampaign.AnchorSettlementId,
			settlement,
			population,
			jurisdiction,
			leadCampaign,
			notifications);

		if (!signals.HasSignals)
		{
			return leadCampaign.AnchorSettlementName + "堂案仍以军报与粮道札记为主。";
		}

		return leadCampaign.AnchorSettlementName + "堂案今并载" + signals.ComposeClauseText() + "。";
	}

	internal static string BuildSettlementAftermathSummary(
		SettlementSnapshot settlement,
		PopulationSettlementSnapshot? population,
		JurisdictionAuthoritySnapshot? jurisdiction,
		CampaignFrontSnapshot? campaign,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(settlement);
		ArgumentNullException.ThrowIfNull(notifications);

		AftermathDocketSignals signals = BuildAftermathDocketSignals(
			settlement.Id,
			settlement,
			population,
			jurisdiction,
			campaign,
			notifications);
		if (!signals.HasSignals)
		{
			return "战后案牍未起。";
		}

		return "战后案牍：" + signals.ComposeClauseText() + "。";
	}

	internal static string BuildCampaignAftermathDocketSummary(
		CampaignFrontSnapshot campaign,
		SettlementSnapshot? settlement,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ArgumentNullException.ThrowIfNull(campaign);
		ArgumentNullException.ThrowIfNull(notifications);

		AftermathDocketSignals signals = BuildAftermathDocketSignals(
			campaign.AnchorSettlementId,
			settlement,
			null,
			null,
			campaign,
			notifications);
		if (!signals.HasSignals)
		{
			return campaign.IsActive ? "军机案下仍止于军报与路报。" : "军机案下尚未并成赏罚抚恤诸册。";
		}

		return "军机案今并载" + signals.ComposeClauseText() + "。";
	}

	private static AftermathDocketSignals BuildAftermathDocketSignals(
		SettlementId settlementId,
		SettlementSnapshot? settlement,
		PopulationSettlementSnapshot? population,
		JurisdictionAuthoritySnapshot? jurisdiction,
		CampaignFrontSnapshot? campaign,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		string settlementKey = settlementId.Value.ToString();
		NarrativeNotificationSnapshot[] source = notifications
			.Where(notification =>
				string.Equals(notification.SourceModuleKey, KnownModuleKeys.WarfareCampaign, StringComparison.Ordinal)
				|| notification.Traces.Any(trace => string.Equals(trace.EntityKey, settlementKey, StringComparison.Ordinal)))
			.ToArray();

		bool merit = source.Any(notification => notification.Traces.Any(trace =>
				string.Equals(trace.SourceModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal)
				|| string.Equals(trace.SourceModuleKey, KnownModuleKeys.SocialMemoryAndRelations, StringComparison.Ordinal)))
			|| campaign != null && campaign.IsActive && campaign.MoraleState >= 55 && campaign.SupplyState >= 50;
		bool blame = source.Any(notification => notification.Traces.Any(trace =>
				string.Equals(trace.SourceModuleKey, KnownModuleKeys.OfficeAndCareer, StringComparison.Ordinal)
				|| string.Equals(trace.SourceModuleKey, KnownModuleKeys.ConflictAndForce, StringComparison.Ordinal)))
			|| campaign != null && (!campaign.IsActive || campaign.FrontPressure >= 60)
			|| (jurisdiction?.PetitionBacklog ?? 0) >= 8;
		bool relief = source.Any(notification => notification.Traces.Any(trace =>
				string.Equals(trace.SourceModuleKey, KnownModuleKeys.PopulationAndHouseholds, StringComparison.Ordinal)
				|| string.Equals(trace.SourceModuleKey, KnownModuleKeys.WorldSettlements, StringComparison.Ordinal)))
			|| population != null && (population.CommonerDistress >= 40 || population.MigrationPressure >= 35)
			|| settlement != null && (settlement.Security <= 55 || settlement.Prosperity <= 58);
		bool disorder = source.Any(notification => notification.Traces.Any(trace =>
				string.Equals(trace.SourceModuleKey, KnownModuleKeys.OrderAndBanditry, StringComparison.Ordinal)
				|| string.Equals(trace.SourceModuleKey, KnownModuleKeys.TradeAndIndustry, StringComparison.Ordinal)))
			|| settlement != null && settlement.Security < 58
			|| campaign != null && (campaign.SupplyState < 45 || campaign.Routes.Any(route => route.Pressure > route.Security));

		return new AftermathDocketSignals
		{
			Merit = merit,
			Blame = blame,
			Relief = relief,
			Disorder = disorder
		};
	}
}
