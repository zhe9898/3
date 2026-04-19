using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

public static class FirstPassPresentationShell
{
	private readonly record struct RegionalBoardProfile(string Label, string BackdropSummary);

	private sealed class AftermathDocketSignals
	{
		public bool Merit { get; init; }

		public bool Blame { get; init; }

		public bool Relief { get; init; }

		public bool Disorder { get; init; }

		public bool HasSignals => Merit || Blame || Relief || Disorder;

		public string ComposeClauseText(bool useArticle)
		{
			List<string> list = new List<string>();
			if (Merit)
			{
				list.Add("记功簿");
			}
			if (Blame)
			{
				list.Add("劾责状");
			}
			if (Relief)
			{
				list.Add("抚恤簿");
			}
			if (Disorder)
			{
				list.Add("清路札");
			}
			return string.Join("、", list);
		}
	}

	public static PresentationShellViewModel Compose(PresentationReadModelBundle bundle)
	{
		ArgumentNullException.ThrowIfNull(bundle, "bundle");
		NarrativeNotificationSnapshot[] notifications = (from notification in bundle.Notifications
			orderby notification.Tier, notification.CreatedAt.Year descending, notification.CreatedAt.Month descending, notification.Id.Value descending
			select notification).ToArray();
		DeskSandboxViewModel deskSandbox = BuildDeskSandbox(bundle, notifications);
		HydrateDeskSandboxPublicLife(bundle, deskSandbox);
		return new PresentationShellViewModel
		{
			GreatHall = BuildGreatHall(bundle, notifications),
			Lineage = BuildLineage(bundle),
			FamilyCouncil = BuildFamilyCouncil(bundle),
			DeskSandbox = deskSandbox,
			Office = BuildOfficeSurface(bundle),
			Warfare = BuildWarfareSurfaceRegionalChinese(bundle, notifications),
			NotificationCenter = BuildNotificationCenter(bundle, notifications),
			Debug = BuildDebugPanel(bundle.Debug)
		};
	}

	private static GreatHallDashboardViewModel BuildGreatHall(PresentationReadModelBundle bundle, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		ClanSnapshot clanSnapshot = bundle.Clans.OrderByDescending((ClanSnapshot clan) => clan.Prestige).ThenBy<ClanSnapshot, string>((ClanSnapshot clan) => clan.ClanName, StringComparer.Ordinal).FirstOrDefault();
		NarrativeNotificationSnapshot? leadNotification = notifications.FirstOrDefault();
		int value = bundle.EducationCandidates.Count((EducationCandidateSnapshot candidate) => candidate.IsStudying);
		int value2 = bundle.EducationCandidates.Count((EducationCandidateSnapshot candidate) => candidate.HasPassedLocalExam);
		int value3 = bundle.ClanTrades.Count((ClanTradeSnapshot trade) => string.Equals(trade.LastOutcome, "Profit", StringComparison.Ordinal));
		int value4 = bundle.OfficeCareers.Count((OfficeCareerSnapshot career) => career.HasAppointment);
		JurisdictionAuthoritySnapshot jurisdictionAuthoritySnapshot = (from jurisdiction in bundle.OfficeJurisdictions
			orderby jurisdiction.AuthorityTier descending, jurisdiction.JurisdictionLeverage descending, jurisdiction.SettlementId.Value
			select jurisdiction).FirstOrDefault();
		SettlementPublicLifeSnapshot settlementPublicLifeSnapshot = bundle.PublicLifeSettlements.OrderByDescending((SettlementPublicLifeSnapshot settlement) => settlement.StreetTalkHeat + settlement.MarketBuzz + settlement.NoticeVisibility + settlement.RoadReportLag + settlement.PrefectureDispatchPressure).ThenBy<SettlementPublicLifeSnapshot, string>((SettlementPublicLifeSnapshot settlement) => settlement.SettlementName, StringComparer.Ordinal).FirstOrDefault();
		return new GreatHallDashboardViewModel
		{
			CurrentDateLabel = $"{bundle.CurrentDate.Year}-{bundle.CurrentDate.Month:D2}",
			ReplayHash = bundle.ReplayHash,
			UrgentCount = notifications.Count((NarrativeNotificationSnapshot notification) => notification.Tier == NotificationTier.Urgent),
			ConsequentialCount = notifications.Count((NarrativeNotificationSnapshot notification) => notification.Tier == NotificationTier.Consequential),
			BackgroundCount = notifications.Count((NarrativeNotificationSnapshot notification) => notification.Tier == NotificationTier.Background),
			FamilySummary = BuildGreatHallFamilySummary(clanSnapshot, bundle.PlayerCommands.Affordances),
			EducationSummary = $"塾馆在读{value}人，场屋得捷{value2}人。",
			TradeSummary = $"市账{bundle.ClanTrades.Count}册，得利{value3}支。",
			PublicLifeSummary = ((settlementPublicLifeSnapshot == null) ? "乡里、镇市与县门今月尚静。" : BuildGreatHallPublicLifeSummary(bundle.PublicLifeSettlements, settlementPublicLifeSnapshot)),
			GovernanceSummary = ((jurisdictionAuthoritySnapshot == null) ? "案头暂无官署呈报。" : $"{value4}人在官途；{jurisdictionAuthoritySnapshot.LeadOfficialName}任{RenderOfficeTitle(jurisdictionAuthoritySnapshot.LeadOfficeTitle)}主事，{RenderPetitionOutcomeCategory(jurisdictionAuthoritySnapshot.PetitionOutcomeCategory)}，积案{jurisdictionAuthoritySnapshot.PetitionBacklog}。"),
			WarfareSummary = BuildGreatHallWarfareSummaryRegionalChinese(bundle),
			AftermathDocketSummary = BuildGreatHallAftermathDocketSummary(bundle, notifications),
			LeadNoticeTitle = (leadNotification?.Title ?? "堂上暂无急报"),
			LeadNoticeGuidance = BuildLeadNoticeGuidance(bundle, leadNotification)
		};
	}

	private static LineageSurfaceViewModel BuildLineage(PresentationReadModelBundle bundle)
	{
		return new LineageSurfaceViewModel
		{
			Clans = (from clan in bundle.Clans.OrderBy<ClanSnapshot, string>((ClanSnapshot clan) => clan.ClanName, StringComparer.Ordinal)
				select new ClanTileViewModel
				{
					ClanName = clan.ClanName,
					Prestige = clan.Prestige,
					SupportReserve = clan.SupportReserve,
					StatusText = ((!clan.HeirPersonId.HasValue) ? "宗房暂未举出承祧人。" : "承祧之人已入谱。")
				}).ToArray()
		};
	}

	private static FamilyCouncilViewModel BuildFamilyCouncil(PresentationReadModelBundle bundle)
	{
		Dictionary<int, ClanNarrativeSnapshot> narrativesByClan = bundle.ClanNarratives.ToDictionary((ClanNarrativeSnapshot narrative) => narrative.ClanId.Value, (ClanNarrativeSnapshot narrative) => narrative);
		PlayerCommandAffordanceSnapshot[] familyAffordances = bundle.PlayerCommands.Affordances
			.Where((PlayerCommandAffordanceSnapshot command) => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal))
			.OrderBy((PlayerCommandAffordanceSnapshot command) => command.TargetLabel, StringComparer.Ordinal)
			.ThenBy((PlayerCommandAffordanceSnapshot command) => command.CommandName, StringComparer.Ordinal)
			.ToArray();
		CommandReceiptViewModel[] familyReceipts = (from receipt in bundle.PlayerCommands.Receipts.Where((PlayerCommandReceiptSnapshot receipt) => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)).OrderBy<PlayerCommandReceiptSnapshot, string>((PlayerCommandReceiptSnapshot receipt) => receipt.TargetLabel, StringComparer.Ordinal).ThenBy<PlayerCommandReceiptSnapshot, string>((PlayerCommandReceiptSnapshot receipt) => receipt.CommandName, StringComparer.Ordinal)
			select new CommandReceiptViewModel
			{
				TargetLabel = receipt.TargetLabel,
				CommandName = receipt.CommandName,
				Label = receipt.Label,
				Summary = receipt.Summary,
				OutcomeSummary = receipt.OutcomeSummary
			}).ToArray();
		int value = bundle.Clans.Count((ClanSnapshot clan) => clan.BranchTension >= 55);
		int value2 = bundle.Clans.Count((ClanSnapshot clan) => clan.SeparationPressure >= 35);
		int value3 = bundle.Clans.Count((ClanSnapshot clan) => clan.MediationMomentum >= 30);
		int value4 = bundle.Clans.Count((ClanSnapshot clan) => clan.MarriageAlliancePressure >= 35);
		int value5 = bundle.Clans.Count((ClanSnapshot clan) => !clan.HeirPersonId.HasValue || clan.HeirSecurity < 40);
		int value6 = bundle.Clans.Count((ClanSnapshot clan) => clan.MourningLoad > 0);
		string lifecyclePrompt = BuildLeadFamilyLifecyclePrompt(bundle.Clans, familyAffordances);
		return new FamilyCouncilViewModel
		{
			Summary = ((bundle.Clans.Count == 0) ? "祠堂暂无线头可议。" : $"祠堂今有{value}宗争势渐炽，{value2}宗起分房之议，{value3}宗可邀族老调停；另有{value4}宗急议婚事，{value5}宗承祧未稳，{value6}宗门内举哀。{lifecyclePrompt}"),
			CommandAffordances = (from command in familyAffordances
				select new CommandAffordanceViewModel
				{
					TargetLabel = command.TargetLabel,
					CommandName = command.CommandName,
					Label = command.Label,
					Summary = command.Summary,
					AvailabilitySummary = command.AvailabilitySummary,
					IsEnabled = command.IsEnabled
				}).ToArray(),
			RecentReceipts = familyReceipts,
			Clans = bundle.Clans.OrderBy<ClanSnapshot, string>((ClanSnapshot clan) => clan.ClanName, StringComparer.Ordinal).Select(delegate(ClanSnapshot clan)
			{
				ClanNarrativeSnapshot value4;
				ClanNarrativeSnapshot narrative2 = (narrativesByClan.TryGetValue(clan.Id.Value, out value4) ? value4 : null);
				return new FamilyConflictTileViewModel
				{
					ClanName = clan.ClanName,
					Prestige = clan.Prestige,
					SupportReserve = clan.SupportReserve,
					ConflictSummary = BuildClanConflictSummary(clan),
					MemorySummary = BuildClanMemorySummary(narrative2),
					LifecycleSummary = BuildClanLifecycleSummary(clan, familyAffordances),
					LastOrderSummary = BuildClanLastOrderSummary(clan)
				};
			}).ToArray()
		};
	}

	private static string BuildClanConflictSummary(ClanSnapshot clan)
	{
		if (clan.SeparationPressure < 60)
		{
			if (clan.BranchTension < 55)
			{
				if (clan.MediationMomentum < 35)
				{
					return $"房支尚可按住；争势{clan.BranchTension}，分房之压{clan.SeparationPressure}。";
				}
				return $"族老已可出面；调停之势{clan.MediationMomentum}。";
			}
			return $"祠堂争声已起；争势{clan.BranchTension}，承祧之压{clan.InheritancePressure}。";
		}
		return $"分房之议已炽；争势{clan.BranchTension}，分房之压{clan.SeparationPressure}。";
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

	private static string BuildGreatHallFamilySummary(ClanSnapshot? clan, IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		if (clan == null)
		{
			return "堂中暂无宗房呈报。";
		}
		string lifecyclePrompt = BuildClanLifecyclePrompt(clan, affordances, includeClanName: false);
		if (clan.MourningLoad > 0)
		{
			return $"{clan.ClanName}门望{clan.Prestige}，门内举哀未毕，丧服之重{clan.MourningLoad}，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
		}
		if (!clan.HeirPersonId.HasValue || clan.HeirSecurity < 40)
		{
			return $"{clan.ClanName}门望{clan.Prestige}，承祧未稳，后议之压在堂，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
		}
		if (clan.MarriageAlliancePressure >= 35 || clan.ReproductivePressure >= 40)
		{
			return $"{clan.ClanName}门望{clan.Prestige}，婚议之压{clan.MarriageAlliancePressure}，添丁之望{clan.ReproductivePressure}，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
		}
		return $"{clan.ClanName}门望{clan.Prestige}，承祧稳度{clan.HeirSecurity}，可支宗务{clan.SupportReserve}。{lifecyclePrompt}";
	}

	private static string BuildClanLifecycleSummary(ClanSnapshot clan, IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		string heirClause = clan.HeirPersonId.HasValue
			? $"承祧稳度{clan.HeirSecurity}"
			: $"堂上尚未定承祧，后议之压{clan.InheritancePressure}";

		string mourningClause = clan.MourningLoad > 0
			? $"门内仍系丧服之重{clan.MourningLoad}"
			: clan.InfantCount > 0
				? $"门内现有襁褓{clan.InfantCount}口，添丁之后仍待护持"
				: $"婚议之压{clan.MarriageAlliancePressure}，添丁之望{clan.ReproductivePressure}";

		if (!string.IsNullOrWhiteSpace(clan.LastLifecycleOutcome))
		{
			string label = string.IsNullOrWhiteSpace(clan.LastLifecycleCommandLabel) ? "门内后计" : clan.LastLifecycleCommandLabel;
			return $"{heirClause}；{mourningClause}。近月{label}：{clan.LastLifecycleOutcome} {BuildClanLifecyclePrompt(clan, affordances, includeClanName: false)}".Trim();
		}

		return $"{heirClause}；{mourningClause}。{BuildClanLifecyclePrompt(clan, affordances, includeClanName: false)}".Trim();
	}

	private static string BuildLeadFamilyLifecyclePrompt(
		IReadOnlyList<ClanSnapshot> clans,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		PlayerCommandAffordanceSnapshot? prompt = clans
			.Select(clan => new
			{
				Clan = clan,
				Affordance = SelectPrimaryLifecycleAffordance(clan, affordances),
			})
			.Where(static entry => entry.Affordance is not null)
			.OrderBy(entry => GetLifecyclePriority(entry.Affordance!))
			.ThenBy(entry => entry.Clan.ClanName, StringComparer.Ordinal)
			.Select(static entry => entry.Affordance)
			.FirstOrDefault();

		if (prompt is null)
		{
			return string.Empty;
		}

		return $"眼下最宜先命{prompt.TargetLabel}{prompt.Label}。";
	}

	private static string BuildLeadNoticeGuidance(
		PresentationReadModelBundle bundle,
		NarrativeNotificationSnapshot? leadNotification)
	{
		if (leadNotification is null)
		{
			return string.Empty;
		}

		string lifecyclePrompt = BuildLeadFamilyLifecyclePrompt(bundle.Clans, bundle.PlayerCommands.Affordances);
		return BuildNotificationWhatNext(leadNotification, lifecyclePrompt);
	}

	private static string BuildNotificationWhatNext(
		NarrativeNotificationSnapshot notification,
		string lifecyclePrompt)
	{
		if (!IsFamilyLifecycleNotification(notification) || string.IsNullOrWhiteSpace(lifecyclePrompt))
		{
			return notification.WhatNext;
		}

		if (string.IsNullOrWhiteSpace(notification.WhatNext))
		{
			return lifecyclePrompt;
		}

		if (notification.WhatNext.Contains(lifecyclePrompt, StringComparison.Ordinal))
		{
			return notification.WhatNext;
		}

		return notification.WhatNext + " " + lifecyclePrompt;
	}

	private static string BuildClanLifecyclePrompt(
		ClanSnapshot clan,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
		bool includeClanName)
	{
		PlayerCommandAffordanceSnapshot? prompt = SelectPrimaryLifecycleAffordance(clan, affordances);
		if (prompt is null)
		{
			return string.Empty;
		}

		string target = includeClanName ? prompt.TargetLabel : string.Empty;
		string prefix = includeClanName
			? $"眼下最宜先命{target}{prompt.Label}"
			: $"眼下宜先{prompt.Label}";
		return $"{prefix}：{prompt.AvailabilitySummary}";
	}

	private static PlayerCommandAffordanceSnapshot? SelectPrimaryLifecycleAffordance(
		ClanSnapshot clan,
		IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
	{
		return affordances
			.Where(command =>
				command.IsEnabled
				&& command.ClanId == clan.Id
				&& string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
				&& IsLifecycleFamilyCommand(command.CommandName))
			.OrderBy(GetLifecyclePriority)
			.ThenBy(command => command.CommandName, StringComparer.Ordinal)
			.FirstOrDefault();
	}

	private static bool IsLifecycleFamilyCommand(string commandName)
	{
		return commandName is PlayerCommandNames.ArrangeMarriage
			or PlayerCommandNames.SupportNewbornCare
			or PlayerCommandNames.DesignateHeirPolicy
			or PlayerCommandNames.SetMourningOrder;
	}

	private static bool IsFamilyLifecycleNotification(NarrativeNotificationSnapshot notification)
	{
		if (!string.Equals(notification.SourceModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal))
		{
			return false;
		}

		return notification.Traces.Any(static trace => trace.EventType is
			FamilyCoreEventNames.MarriageAllianceArranged
			or FamilyCoreEventNames.BirthRegistered
			or FamilyCoreEventNames.DeathRegistered
			or FamilyCoreEventNames.HeirSecurityWeakened);
	}

	private static int GetLifecyclePriority(PlayerCommandAffordanceSnapshot command)
	{
		return command.CommandName switch
		{
			PlayerCommandNames.SetMourningOrder => 0,
			PlayerCommandNames.SupportNewbornCare => 1,
			PlayerCommandNames.DesignateHeirPolicy => 2,
			PlayerCommandNames.ArrangeMarriage => 3,
			_ => 10,
		};
	}

	private static string BuildClanLastOrderSummary(ClanSnapshot clan)
	{
		if (string.IsNullOrWhiteSpace(clan.LastConflictOutcome))
		{
			return "本月尚无新议决。";
		}
		return clan.LastConflictCommandLabel + "：" + clan.LastConflictOutcome;
	}

	private static string BuildGreatHallPublicLifeSummary(IReadOnlyList<SettlementPublicLifeSnapshot> publicLifeSettlements, SettlementPublicLifeSnapshot leadPublicLife)
	{
		int value = publicLifeSettlements.Count((SettlementPublicLifeSnapshot settlement) => settlement.StreetTalkHeat >= 55);
		int value2 = publicLifeSettlements.Count((SettlementPublicLifeSnapshot settlement) => settlement.NoticeVisibility >= 55 || settlement.PrefectureDispatchPressure >= 55);
		int value3 = publicLifeSettlements.Count((SettlementPublicLifeSnapshot settlement) => settlement.RoadReportLag >= 45);
		int value4 = publicLifeSettlements.Count((SettlementPublicLifeSnapshot settlement) =>
			settlement.RoadReportLag >= 50
			|| settlement.CourierRisk >= 50
			|| Math.Abs(settlement.DocumentaryWeight - settlement.MarketRumorFlow) >= 12
			|| settlement.PrefectureDispatchPressure >= 60 && settlement.PublicLegitimacy < 55);
		return $"今月县情起于{leadPublicLife.NodeLabel}，正值{leadPublicLife.MonthlyCadenceLabel}，{leadPublicLife.CrowdMixLabel}，街谈渐热{value}处，门前壅挤{value2}处，路报迟滞{value3}处，说法相左{value4}处。{leadPublicLife.ContentionSummary}";
	}

	private static string BuildSettlementPublicLifeSummary(SettlementPublicLifeSnapshot publicLife)
	{
		return publicLife.NodeLabel + "：" + publicLife.CadenceSummary + publicLife.PublicSummary + publicLife.OfficialNoticeLine + publicLife.StreetTalkLine + publicLife.RoadReportLine + publicLife.PrefectureDispatchLine + publicLife.ContentionSummary + publicLife.RouteReportSummary;
	}

	private static IReadOnlyList<CommandAffordanceViewModel> BuildSettlementPublicLifeAffordances(PresentationReadModelBundle bundle, SettlementId settlementId)
	{
		return (from command in bundle.PlayerCommands.Affordances
			where string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)
				&& command.SettlementId == settlementId
			orderby command.TargetLabel, command.CommandName
			select new CommandAffordanceViewModel
			{
				TargetLabel = command.TargetLabel,
				CommandName = command.CommandName,
				Label = command.Label,
				Summary = command.Summary,
				AvailabilitySummary = command.AvailabilitySummary,
				IsEnabled = command.IsEnabled
			}).ToArray();
	}

	private static IReadOnlyList<CommandReceiptViewModel> BuildSettlementPublicLifeReceipts(PresentationReadModelBundle bundle, SettlementId settlementId)
	{
		return (from receipt in bundle.PlayerCommands.Receipts
			where string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)
				&& receipt.SettlementId == settlementId
			orderby receipt.TargetLabel, receipt.CommandName
			select new CommandReceiptViewModel
			{
				TargetLabel = receipt.TargetLabel,
				CommandName = receipt.CommandName,
				Label = receipt.Label,
				Summary = receipt.Summary,
				OutcomeSummary = receipt.OutcomeSummary
			}).ToArray();
	}

	private static void HydrateDeskSandboxPublicLife(PresentationReadModelBundle bundle, DeskSandboxViewModel deskSandbox)
	{
		SettlementSnapshot[] orderedSettlements = bundle.Settlements
			.OrderBy((SettlementSnapshot settlement) => settlement.Name, StringComparer.Ordinal)
			.ToArray();
		for (int i = 0; i < orderedSettlements.Length && i < deskSandbox.Settlements.Count; i++)
		{
			SettlementSnapshot settlement = orderedSettlements[i];
			SettlementNodeViewModel node = deskSandbox.Settlements[i];
			node.PublicLifeCommandAffordances = BuildSettlementPublicLifeAffordances(bundle, settlement.Id);
			node.PublicLifeRecentReceipts = BuildSettlementPublicLifeReceipts(bundle, settlement.Id);

			SettlementPublicLifeSnapshot? publicLife = bundle.PublicLifeSettlements
				.FirstOrDefault((SettlementPublicLifeSnapshot snapshot) => snapshot.SettlementId == settlement.Id);
			if (publicLife is not null && !string.IsNullOrWhiteSpace(publicLife.ChannelSummary))
			{
				node.PublicLifeSummary += publicLife.ChannelSummary;
			}
		}
	}

	private static DeskSandboxViewModel BuildDeskSandbox(PresentationReadModelBundle bundle, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		Dictionary<int, PopulationSettlementSnapshot> populationBySettlement = bundle.PopulationSettlements.ToDictionary((PopulationSettlementSnapshot settlement) => settlement.SettlementId.Value, (PopulationSettlementSnapshot settlement) => settlement);
		ILookup<int, AcademySnapshot> academiesBySettlement = bundle.Academies.ToLookup((AcademySnapshot academy) => academy.SettlementId.Value);
		Dictionary<int, MarketSnapshot> marketsBySettlement = bundle.Markets.ToDictionary((MarketSnapshot market) => market.SettlementId.Value, (MarketSnapshot market) => market);
		ILookup<int, TradeRouteSnapshot> tradeRoutesBySettlement = bundle.TradeRoutes.ToLookup((TradeRouteSnapshot route) => route.SettlementId.Value);
		Dictionary<int, SettlementPublicLifeSnapshot> publicLifeBySettlement = bundle.PublicLifeSettlements.ToDictionary((SettlementPublicLifeSnapshot settlement) => settlement.SettlementId.Value, (SettlementPublicLifeSnapshot settlement) => settlement);
		Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = bundle.OfficeJurisdictions.ToDictionary((JurisdictionAuthoritySnapshot jurisdiction) => jurisdiction.SettlementId.Value, (JurisdictionAuthoritySnapshot jurisdiction) => jurisdiction);
		Dictionary<int, CampaignFrontSnapshot> campaignsBySettlement = bundle.Campaigns.ToDictionary((CampaignFrontSnapshot campaign) => campaign.AnchorSettlementId.Value, (CampaignFrontSnapshot campaign) => campaign);
		Dictionary<int, CampaignMobilizationSignalSnapshot> mobilizationSignalsBySettlement = bundle.CampaignMobilizationSignals.ToDictionary((CampaignMobilizationSignalSnapshot signal) => signal.SettlementId.Value, (CampaignMobilizationSignalSnapshot signal) => signal);
		return new DeskSandboxViewModel
		{
			Settlements = bundle.Settlements.OrderBy<SettlementSnapshot, string>((SettlementSnapshot settlement) => settlement.Name, StringComparer.Ordinal).Select(delegate(SettlementSnapshot settlement)
			{
				PopulationSettlementSnapshot value;
				PopulationSettlementSnapshot populationSettlementSnapshot = (populationBySettlement.TryGetValue(settlement.Id.Value, out value) ? value : null);
				AcademySnapshot[] array = academiesBySettlement[settlement.Id.Value].OrderBy<AcademySnapshot, string>((AcademySnapshot academy) => academy.AcademyName, StringComparer.Ordinal).ToArray();
				MarketSnapshot value2;
				bool flag = marketsBySettlement.TryGetValue(settlement.Id.Value, out value2);
				SettlementPublicLifeSnapshot value3;
				bool flag2 = publicLifeBySettlement.TryGetValue(settlement.Id.Value, out value3);
				JurisdictionAuthoritySnapshot value4;
				bool flag3 = jurisdictionsBySettlement.TryGetValue(settlement.Id.Value, out value4);
				CampaignFrontSnapshot value5;
				bool flag4 = campaignsBySettlement.TryGetValue(settlement.Id.Value, out value5);
				CampaignMobilizationSignalSnapshot value6;
				bool flag5 = mobilizationSignalsBySettlement.TryGetValue(settlement.Id.Value, out value6);
				TradeRouteSnapshot[] tradeRoutes = tradeRoutesBySettlement[settlement.Id.Value].OrderBy<TradeRouteSnapshot, string>((TradeRouteSnapshot route) => route.RouteName, StringComparer.Ordinal).ToArray();
				return new SettlementNodeViewModel
				{
					SettlementName = settlement.Name,
					Security = settlement.Security,
					Prosperity = settlement.Prosperity,
					AcademySummary = ((array.Length == 0) ? "塾馆未立。" : string.Join(", ", array.Select((AcademySnapshot academy) => academy.AcademyName))),
					MarketSummary = (flag ? $"{value2.MarketName}：市需{value2.Demand}，价行{value2.PriceIndex}，路险{value2.LocalRisk}。" : "市肆未起。"),
					PublicLifeSummary = (flag2 ? BuildSettlementPublicLifeSummary(value3) : "乡里街谈未起，县门榜示亦未壅塞。"),
					GovernanceSummary = (flag3 ? $"{RenderOfficeTitle(value4.LeadOfficeTitle)}{value4.LeadOfficialName}：乡面杠杆{value4.JurisdictionLeverage}，{RenderAdministrativeTaskTier(value4.AdministrativeTaskTier)}差遣 {RenderAdministrativeTask(value4.CurrentAdministrativeTask)}，{RenderPetitionOutcomeCategory(value4.PetitionOutcomeCategory)}，积案{value4.PetitionBacklog}。" : "官署未设。"),
					CampaignSummary = BuildSettlementCampaignSummaryRegionalChinese(flag4 ? value5 : null, flag5 ? value6 : null, settlement, tradeRoutes),
					AftermathSummary = BuildSettlementAftermathSummary(settlement, populationSettlementSnapshot, flag3 ? value4 : null, flag4 ? value5 : null, notifications),
					PressureSummary = ((populationSettlementSnapshot == null) ? "民户情形未起。" : $"民困{populationSettlementSnapshot.CommonerDistress}，丁力{populationSettlementSnapshot.LaborSupply}，流徙{populationSettlementSnapshot.MigrationPressure}。")
				};
			}).ToArray()
		};
	}

	private static OfficeSurfaceViewModel BuildOfficeSurface(PresentationReadModelBundle bundle)
	{
		if (bundle.OfficeCareers.Count == 0 && bundle.OfficeJurisdictions.Count == 0)
		{
			return new OfficeSurfaceViewModel
			{
				Summary = "案头暂无官署牍报。",
				CommandAffordances = Array.Empty<CommandAffordanceViewModel>(),
				RecentReceipts = Array.Empty<CommandReceiptViewModel>()
			};
		}
		int value = bundle.OfficeCareers.Count((OfficeCareerSnapshot career) => career.HasAppointment);
		int count = bundle.OfficeJurisdictions.Count;
		int value2 = ((bundle.OfficeJurisdictions.Count != 0) ? bundle.OfficeJurisdictions.Max((JurisdictionAuthoritySnapshot jurisdiction) => jurisdiction.PetitionBacklog) : 0);
		Dictionary<int, string> settlementNames = bundle.Settlements.ToDictionary((SettlementSnapshot settlement) => settlement.Id.Value, (SettlementSnapshot settlement) => settlement.Name);
		string value3;
		return new OfficeSurfaceViewModel
		{
			Summary = $"现有官人{value}名，分掌{count}处，积案最高{value2}。",
			CommandAffordances = (from command in (from command in bundle.PlayerCommands.Affordances
					where string.Equals(command.SurfaceKey, "Office", StringComparison.Ordinal)
					orderby command.SettlementId.Value
					select command).ThenBy<PlayerCommandAffordanceSnapshot, string>((PlayerCommandAffordanceSnapshot command) => command.CommandName, StringComparer.Ordinal)
				select new CommandAffordanceViewModel
				{
					CommandName = command.CommandName,
					Label = command.Label,
					Summary = command.Summary,
					AvailabilitySummary = command.AvailabilitySummary,
					IsEnabled = command.IsEnabled
				}).ToArray(),
			RecentReceipts = (from receipt in (from receipt in bundle.PlayerCommands.Receipts
					where string.Equals(receipt.SurfaceKey, "Office", StringComparison.Ordinal)
					orderby receipt.SettlementId.Value
					select receipt).ThenBy<PlayerCommandReceiptSnapshot, string>((PlayerCommandReceiptSnapshot receipt) => receipt.CommandName, StringComparer.Ordinal)
				select new CommandReceiptViewModel
				{
					CommandName = receipt.CommandName,
					Label = receipt.Label,
					Summary = receipt.Summary,
					OutcomeSummary = receipt.OutcomeSummary
				}).ToArray(),
			Appointments = (from career in bundle.OfficeCareers
				orderby career.HasAppointment descending, career.AuthorityTier descending, career.PersonId.Value
				select new OfficeAppointmentViewModel
				{
					DisplayName = career.DisplayName,
					OfficeTitle = RenderOfficeTitle(career.OfficeTitle),
					HasAppointment = career.HasAppointment,
					AuthorityTier = career.AuthorityTier,
					ServiceSummary = (career.HasAppointment ? $"供职{career.ServiceMonths}月；升势{RenderPromotionPressureLabel(career.PromotionPressureLabel)}，黜压{RenderDemotionPressureLabel(career.DemotionPressureLabel)}。" : "候补听选。"),
					TaskSummary = RenderAdministrativeTaskTier(career.AdministrativeTaskTier) + "差遣：" + RenderAdministrativeTask(career.CurrentAdministrativeTask),
					PetitionSummary = $"词牍压{career.PetitionPressure}，积案{career.PetitionBacklog}。",
					PressureSummary = BuildOfficePressureSummary(career.HasAppointment, career.LastOutcome, career.PetitionBacklog, career.PromotionPressureLabel, career.DemotionPressureLabel),
					PetitionOutcomeCategory = RenderPetitionOutcomeCategory(career.PetitionOutcomeCategory),
					LastOutcome = career.LastOutcome,
					LastPetitionOutcome = RenderPetitionOutcome(career.LastPetitionOutcome)
				}).ToArray(),
			Jurisdictions = (from jurisdiction in bundle.OfficeJurisdictions
				orderby jurisdiction.SettlementId.Value
				select new OfficeJurisdictionViewModel
				{
					SettlementLabel = (settlementNames.TryGetValue(jurisdiction.SettlementId.Value, out value3) ? value3 : $"乡里#{jurisdiction.SettlementId.Value}"),
					LeadSummary = RenderOfficeTitle(jurisdiction.LeadOfficeTitle) + " " + jurisdiction.LeadOfficialName,
					LeverageSummary = $"秩阶{jurisdiction.AuthorityTier}，乡面杖力{jurisdiction.JurisdictionLeverage}。",
					PetitionSummary = $"词牍压{jurisdiction.PetitionPressure}，积案{jurisdiction.PetitionBacklog}。",
					TaskSummary = RenderAdministrativeTaskTier(jurisdiction.AdministrativeTaskTier) + "差遣：" + RenderAdministrativeTask(jurisdiction.CurrentAdministrativeTask),
					PetitionOutcomeCategory = RenderPetitionOutcomeCategory(jurisdiction.PetitionOutcomeCategory),
					LastPetitionOutcome = RenderPetitionOutcome(jurisdiction.LastPetitionOutcome)
				}).ToArray()
		};
	}

	private static string RenderAdministrativeTask(string taskName)
	{
		if (1 == 0)
		{
		}
		string result = taskName switch
		{
			"Awaiting posting" => "候补听选", 
			"emergency petition review" => "急牍覆核", 
			"district petition hearings" => "勘理词状", 
			"hearing district petitions" => "勘理词状", 
			"county register review" => "勾检户籍", 
			"petition copying desk" => "誊录词牍", 
			"sealed filing copy desk" => "誊黄封牍", 
			"copying tax rolls and sealed filings" => "誊录税册与封牍", 
			"reviewing memorials after the campaign" => "覆核战后功过文移", 
			_ => taskName, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string RenderOfficeTitle(string officeTitle)
	{
		if (1 == 0)
		{
		}
		string result = officeTitle switch
		{
			"Assistant Magistrate" => "县丞", 
			"Registrar" => "主簿", 
			"District Clerk" => "书吏", 
			"Unappointed" => "未授官", 
			_ => officeTitle, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string RenderAdministrativeTaskTier(string taskTier)
	{
		if (string.Equals(taskTier, "candidate", StringComparison.Ordinal))
		{
			return "候次";
		}
		if (1 == 0)
		{
		}
		string result = taskTier switch
		{
			"crisis" => "急务", 
			"district" => "州县", 
			"registry" => "簿册", 
			"clerical" => "案牍", 
			"inactive" => "候补", 
			_ => taskTier, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string RenderPetitionOutcomeCategory(string category)
	{
		if (1 == 0)
		{
		}
		string result = category switch
		{
			"Queued" => "待勘", 
			"Unavailable" => "未开案", 
			"Delayed" => "稽延", 
			"Triaged" => "分轻重", 
			"Cleared" => "已清", 
			"Granted" => "准行", 
			"Surged" => "案牍骤涌", 
			"Stalled" => "壅滞", 
			"Censured" => "劾责中", 
			"Unknown" => "未详", 
			_ => category, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string RenderPromotionPressureLabel(string label)
	{
		if (1 == 0)
		{
		}
		string result = label switch
		{
			"promotion-ready" => "可迁", 
			"rising" => "渐起", 
			"steady" => "持平", 
			"thin" => "微弱", 
			_ => label, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string RenderDemotionPressureLabel(string label)
	{
		if (1 == 0)
		{
		}
		string result = label switch
		{
			"critical" => "危急", 
			"strained" => "吃紧", 
			"watched" => "在察", 
			"stable" => "平稳", 
			_ => label, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string BuildOfficePressureSummary(bool hasAppointment, string lastOutcome, int petitionBacklog, string promotionLabel, string demotionLabel)
	{
		string value = RenderPromotionPressureLabel(promotionLabel);
		string value2 = RenderDemotionPressureLabel(demotionLabel);
		if (!hasAppointment && string.Equals(lastOutcome, "听差", StringComparison.Ordinal))
		{
			return "尚未授官，今先随案听差。";
		}
		if (!hasAppointment && string.Equals(lastOutcome, "候缺", StringComparison.Ordinal))
		{
			return "官途未开，仍在守选候阙。";
		}
		if (!hasAppointment)
		{
			return "官途未开，仍在候缺。";
		}
		if (1 == 0)
		{
		}
		string result = lastOutcome switch
		{
			"Promoted" => $"官途近有迁转之势，升势{value}，黜压{value2}。", 
			"Demoted" => $"官途方遭降黜，升势{value}，黜压{value2}。", 
			"Lost" => $"官途已失，积案{petitionBacklog}，黜压{value2}。", 
			_ => $"官途暂守，升势{value}，黜压{value2}。", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string RenderPetitionOutcome(string outcome)
	{
		if (string.IsNullOrWhiteSpace(outcome))
		{
			return string.Empty;
		}
		int num = outcome.IndexOf(':');
		string text;
		if (num > 0 && num < outcome.Length - 1)
		{
			string category = outcome.Substring(0, num).Trim();
			text = outcome;
			int num2 = num + 1;
			string text2 = text.Substring(num2, text.Length - num2).Trim();
			return RenderPetitionOutcomeCategory(category) + "：" + text2;
		}
		if (1 == 0)
		{
		}
		text = ((outcome == "Petitions cleared while copying tax rolls and sealed filings.") ? "已清：税册与封牍俱已誊定。" : ((!(outcome == "Censured and triaged.")) ? outcome : "劾责中：词牍分轻重收理。"));
		if (1 == 0)
		{
		}
		return text;
	}

	private static string RenderCampaignSurfaceText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}
		string text2 = RenderEscortRouteSummary(text);
		text2 = text2.Replace("Registrar couriers are tying docket traffic into the campaign board.", "主簿差役正把文移驿线并入军机案头。", StringComparison.Ordinal);
		return text2.Replace("Registrar ", "主簿 ", StringComparison.Ordinal).Replace("district层级", "县署层级", StringComparison.Ordinal).Replace(" district ", " 县署 ", StringComparison.OrdinalIgnoreCase)
			.Replace("campaign board", "军机案头", StringComparison.OrdinalIgnoreCase)
			.Replace("docket traffic", "文移驿线", StringComparison.OrdinalIgnoreCase)
			.Replace("backlog", "积案", StringComparison.OrdinalIgnoreCase);
	}

	private static string RenderEscortRouteSummary(string text)
	{
		int num = text.IndexOf(" escorts are keeping stores moving for ", StringComparison.OrdinalIgnoreCase);
		if (num <= 0)
		{
			return text;
		}
		string s = text.Substring(0, num).Trim();
		if (!int.TryParse(s, out var result))
		{
			return text;
		}
		int num2 = num + " escorts are keeping stores moving for ".Length;
		int num3 = text.IndexOf('.', num2);
		if (num3 < 0)
		{
			return text;
		}
		int num4 = num2;
		string value = text.Substring(num4, num3 - num4).Trim();
		num4 = num3 + 1;
		string text2 = text.Substring(num4, text.Length - num4).Trim();
		string text3 = $"{value}粮秣正由护运{result}人维持行转。";
		return string.IsNullOrEmpty(text2) ? text3 : (text3 + " " + text2);
	}

	private static string BuildCampaignFrontSummaryText(CampaignFrontSnapshot campaign)
	{
		return $"{campaign.FrontLabel}：前线{campaign.FrontPressure}，粮道{campaign.SupplyState}（{campaign.SupplyStateLabel}），军心{campaign.MoraleState}（{campaign.MoraleStateLabel}）。";
	}

	private static string BuildCampaignMobilizationSummaryText(CampaignFrontSnapshot campaign)
	{
		return $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowRegionalChinese(campaign.MobilizationWindowLabel)}。";
	}

	private static string BuildMobilizationSignalForceSummaryText(CampaignMobilizationSignalSnapshot signal)
	{
		return $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。";
	}

	private static string BuildMobilizationSignalOfficeSummaryText(CampaignMobilizationSignalSnapshot signal)
	{
		return (signal.OfficeAuthorityTier > 0) ? $"官署层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。" : "暂无官署文移接应。";
	}

	private static WarfareSurfaceViewModel BuildWarfareSurface(PresentationReadModelBundle bundle)
	{
		if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
		{
			return new WarfareSurfaceViewModel
			{
				Summary = "暂无军务沙盘投影。",
				CommandAffordances = Array.Empty<CommandAffordanceViewModel>(),
				RecentReceipts = Array.Empty<CommandReceiptViewModel>()
			};
		}
		int activeCampaignCount = bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive);
		int peakFrontPressure = ((bundle.Campaigns.Count != 0) ? bundle.Campaigns.Max((CampaignFrontSnapshot campaign) => campaign.FrontPressure) : 0);
		return new WarfareSurfaceViewModel
		{
			Summary = BuildWarfareSurfaceSummary(bundle, activeCampaignCount, peakFrontPressure),
			CampaignBoards = (from campaign in bundle.Campaigns
				orderby campaign.IsActive descending, campaign.FrontPressure descending, campaign.CampaignId.Value
				select new CampaignBoardViewModel
				{
					CampaignName = campaign.CampaignName,
					SettlementLabel = campaign.AnchorSettlementName,
					StatusLabel = (campaign.IsActive ? "行营在案" : "战后覆核"),
					FrontLabel = campaign.FrontLabel,
					SupplyStateLabel = campaign.SupplyStateLabel,
					MoraleStateLabel = campaign.MoraleStateLabel,
					CommandFitLabel = campaign.CommandFitLabel,
					DirectiveLabel = campaign.ActiveDirectiveLabel,
					DirectiveSummary = campaign.ActiveDirectiveSummary,
					DirectiveTrace = campaign.LastDirectiveTrace,
					ObjectiveSummary = campaign.ObjectiveSummary,
					FrontSummary = $"{campaign.FrontLabel}：前线{campaign.FrontPressure}，粮道{campaign.SupplyState}（{campaign.SupplyStateLabel}），军心{campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
					MobilizationSummary = $"应调之众{campaign.MobilizedForceCount}，动员窗{campaign.MobilizationWindowLabel}。",
					SupplyLineSummary = campaign.SupplyLineSummary,
					CoordinationSummary = campaign.OfficeCoordinationTrace,
					CommanderSummary = campaign.CommanderSummary,
					AftermathSummary = campaign.LastAftermathSummary,
					Routes = campaign.Routes.Select((CampaignRouteSnapshot route) => new CampaignRouteViewModel
					{
						RouteLabel = route.RouteLabel,
						RouteRole = route.RouteRole,
						FlowStateLabel = route.FlowStateLabel,
						Summary = route.Summary
					}).ToArray()
				}).ToArray(),
			MobilizationSignals = (from signal in bundle.CampaignMobilizationSignals
				orderby signal.ResponseActivationLevel descending, signal.SettlementId.Value
				select new CampaignMobilizationSignalViewModel
				{
					SettlementLabel = signal.SettlementName,
					WindowLabel = signal.MobilizationWindowLabel,
					ForceSummary = $"应调之众{signal.AvailableForceCount}，整备{signal.Readiness}，统摄{signal.CommandCapacity}。",
					CommandFitLabel = signal.CommandFitLabel,
					DirectiveLabel = signal.ActiveDirectiveLabel,
					DirectiveSummary = signal.ActiveDirectiveSummary,
					OfficeSummary = ((signal.OfficeAuthorityTier > 0) ? $"官署层级{signal.OfficeAuthorityTier}，杠杆{signal.AdministrativeLeverage}，积案{signal.PetitionBacklog}。" : "暂无官署文移接应。"),
					SourceTrace = signal.SourceTrace
				}).ToArray()
		};
	}

	private static string BuildGreatHallWarfareSummary(PresentationReadModelBundle bundle)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot campaignFrontSnapshot = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		return $"现有{bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive)}处在案行营；{campaignFrontSnapshot.AnchorSettlementName}当前为{campaignFrontSnapshot.FrontLabel}，{campaignFrontSnapshot.SupplyStateLabel}，{campaignFrontSnapshot.CommandFitLabel}。";
	}

	private static string BuildSettlementCampaignSummary(CampaignFrontSnapshot? campaign, CampaignMobilizationSignalSnapshot? signal)
	{
		if (campaign != null)
		{
			CampaignRouteSnapshot campaignRouteSnapshot = campaign.Routes.FirstOrDefault();
			string value = ((campaignRouteSnapshot == null) ? "暂无路况细目" : RenderCampaignSurfaceText(campaignRouteSnapshot.RouteLabel + campaignRouteSnapshot.FlowStateLabel));
			return $"{campaign.CampaignName}：{campaign.FrontLabel}，{campaign.CommandFitLabel}，军令{campaign.ActiveDirectiveLabel}，{value}。";
		}
		if (signal != null)
		{
			return $"{signal.MobilizationWindowLabel}动员窗，应调之众{signal.AvailableForceCount}，整备{signal.Readiness}，统摄{signal.CommandCapacity}，军令{signal.ActiveDirectiveLabel}。";
		}
		return "暂无军务沙盘投影。";
	}

	private static string BuildWarfareSurfaceSummary(PresentationReadModelBundle bundle, int activeCampaignCount, int peakFrontPressure)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot campaignFrontSnapshot = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		return $"现有{activeCampaignCount}处在案行营，峰值前线压力{peakFrontPressure}；{campaignFrontSnapshot.AnchorSettlementName}正在以{campaignFrontSnapshot.CommandFitLabel}维持{campaignFrontSnapshot.FrontLabel}。";
	}

	private static NotificationCenterViewModel BuildNotificationCenter(PresentationReadModelBundle bundle, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		string lifecyclePrompt = BuildLeadFamilyLifecyclePrompt(bundle.Clans, bundle.PlayerCommands.Affordances);
		return new NotificationCenterViewModel
		{
			Items = notifications.Select((NarrativeNotificationSnapshot notification) => new NotificationItemViewModel
			{
				Title = notification.Title,
				Summary = notification.Summary,
				WhyItHappened = notification.WhyItHappened,
				WhatNext = BuildNotificationWhatNext(notification, lifecyclePrompt),
				TierLabel = notification.Tier.ToString(),
				SurfaceLabel = notification.Surface.ToString(),
				SourceModuleKey = notification.SourceModuleKey,
				TraceCount = notification.Traces.Count
			}).ToArray()
		};
	}

	private static WarfareSurfaceViewModel BuildWarfareSurfaceChinese(PresentationReadModelBundle bundle)
	{
		if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
		{
			return new WarfareSurfaceViewModel
			{
				Summary = "暂无军务沙盘投影。"
			};
		}
		int activeCampaignCount = bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive);
		int peakFrontPressure = ((bundle.Campaigns.Count != 0) ? bundle.Campaigns.Max((CampaignFrontSnapshot campaign) => campaign.FrontPressure) : 0);
		return new WarfareSurfaceViewModel
		{
			Summary = BuildWarfareSurfaceSummaryChinese(bundle, activeCampaignCount, peakFrontPressure),
			CampaignBoards = (from campaign in bundle.Campaigns
				orderby campaign.IsActive descending, campaign.FrontPressure descending, campaign.CampaignId.Value
				select new CampaignBoardViewModel
				{
					CampaignName = campaign.CampaignName,
					SettlementLabel = campaign.AnchorSettlementName,
					StatusLabel = (campaign.IsActive ? "行营在案" : "战后覆核"),
					FrontLabel = campaign.FrontLabel,
					SupplyStateLabel = campaign.SupplyStateLabel,
					MoraleStateLabel = campaign.MoraleStateLabel,
					CommandFitLabel = campaign.CommandFitLabel,
					DirectiveLabel = campaign.ActiveDirectiveLabel,
					DirectiveSummary = campaign.ActiveDirectiveSummary,
					DirectiveTrace = campaign.LastDirectiveTrace,
					ObjectiveSummary = campaign.ObjectiveSummary,
					FrontSummary = $"{campaign.FrontLabel}：前线 {campaign.FrontPressure}，粮道 {campaign.SupplyState}（{campaign.SupplyStateLabel}），军心 {campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
					MobilizationSummary = $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowChinese(campaign.MobilizationWindowLabel)}。",
					SupplyLineSummary = campaign.SupplyLineSummary,
					CoordinationSummary = campaign.OfficeCoordinationTrace,
					CommanderSummary = campaign.CommanderSummary,
					AftermathSummary = campaign.LastAftermathSummary,
					Routes = campaign.Routes.Select((CampaignRouteSnapshot route) => new CampaignRouteViewModel
					{
						RouteLabel = route.RouteLabel,
						RouteRole = route.RouteRole,
						FlowStateLabel = route.FlowStateLabel,
						Summary = route.Summary
					}).ToArray()
				}).ToArray(),
			MobilizationSignals = (from signal in bundle.CampaignMobilizationSignals
				orderby signal.ResponseActivationLevel descending, signal.SettlementId.Value
				select new CampaignMobilizationSignalViewModel
				{
					SettlementLabel = signal.SettlementName,
					WindowLabel = DescribeMobilizationWindowChinese(signal.MobilizationWindowLabel),
					ForceSummary = $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。",
					CommandFitLabel = signal.CommandFitLabel,
					DirectiveLabel = signal.ActiveDirectiveLabel,
					DirectiveSummary = signal.ActiveDirectiveSummary,
					OfficeSummary = ((signal.OfficeAuthorityTier > 0) ? $"官署层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。" : "暂无官署文移接应。"),
					SourceTrace = signal.SourceTrace
				}).ToArray()
		};
	}

	private static string BuildGreatHallWarfareSummaryChinese(PresentationReadModelBundle bundle)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot campaignFrontSnapshot = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		return $"现有 {bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive)} 处在案行营；{campaignFrontSnapshot.AnchorSettlementName}当前为 {campaignFrontSnapshot.FrontLabel}、{campaignFrontSnapshot.SupplyStateLabel}、{campaignFrontSnapshot.CommandFitLabel}。";
	}

	private static string BuildSettlementCampaignSummaryChinese(CampaignFrontSnapshot? campaign, CampaignMobilizationSignalSnapshot? signal)
	{
		if (campaign != null)
		{
			CampaignRouteSnapshot campaignRouteSnapshot = campaign.Routes.FirstOrDefault();
			string value = ((campaignRouteSnapshot == null) ? "暂无路况细目" : (campaignRouteSnapshot.RouteLabel + campaignRouteSnapshot.FlowStateLabel));
			return $"{campaign.CampaignName}，{campaign.FrontLabel}，{campaign.CommandFitLabel}，军令 {campaign.ActiveDirectiveLabel}，{value}。";
		}
		if (signal != null)
		{
			return $"{DescribeMobilizationWindowChinese(signal.MobilizationWindowLabel)}动员窗，应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}，军令 {signal.ActiveDirectiveLabel}。";
		}
		return "暂无军务沙盘投影。";
	}

	private static string BuildWarfareSurfaceSummaryChinese(PresentationReadModelBundle bundle, int activeCampaignCount, int peakFrontPressure)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot campaignFrontSnapshot = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		return $"现有 {activeCampaignCount} 处在案行营，峰值前线压力 {peakFrontPressure}；{campaignFrontSnapshot.AnchorSettlementName}正以 {campaignFrontSnapshot.CommandFitLabel} 维持 {campaignFrontSnapshot.FrontLabel}。";
	}

	private static string DescribeMobilizationWindowChinese(string mobilizationWindowLabel)
	{
		if (1 == 0)
		{
		}
		string result = mobilizationWindowLabel switch
		{
			"Open" => "可开", 
			"Narrow" => "可守", 
			"Preparing" => "待整", 
			_ => "已闭", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static WarfareSurfaceViewModel BuildWarfareSurfaceReactiveChinese(PresentationReadModelBundle bundle)
	{
		if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
		{
			return new WarfareSurfaceViewModel
			{
				Summary = "暂无军务沙盘投影。"
			};
		}
		int activeCampaignCount = bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive);
		int peakFrontPressure = ((bundle.Campaigns.Count != 0) ? bundle.Campaigns.Max((CampaignFrontSnapshot campaign) => campaign.FrontPressure) : 0);
		return new WarfareSurfaceViewModel
		{
			Summary = BuildWarfareSurfaceSummaryReactiveChinese(bundle, activeCampaignCount, peakFrontPressure),
			CampaignBoards = (from campaign in bundle.Campaigns
				orderby campaign.IsActive descending, campaign.FrontPressure descending, campaign.CampaignId.Value
				select new CampaignBoardViewModel
				{
					CampaignName = campaign.CampaignName,
					SettlementLabel = campaign.AnchorSettlementName,
					StatusLabel = (campaign.IsActive ? "行营在案" : "战后覆核"),
					EnvironmentLabel = BuildCampaignBoardEnvironmentLabel(campaign),
					BoardSurfaceLabel = BuildCampaignBoardSurfaceLabel(campaign),
					BoardAtmosphereSummary = BuildCampaignBoardAtmosphereSummary(campaign),
					MarkerSummary = BuildCampaignBoardMarkerSummary(campaign),
					FrontLabel = campaign.FrontLabel,
					SupplyStateLabel = campaign.SupplyStateLabel,
					MoraleStateLabel = campaign.MoraleStateLabel,
					CommandFitLabel = campaign.CommandFitLabel,
					DirectiveLabel = campaign.ActiveDirectiveLabel,
					DirectiveSummary = campaign.ActiveDirectiveSummary,
					DirectiveTrace = campaign.LastDirectiveTrace,
					ObjectiveSummary = campaign.ObjectiveSummary,
					FrontSummary = $"{campaign.FrontLabel}：前线 {campaign.FrontPressure}，粮道 {campaign.SupplyState}（{campaign.SupplyStateLabel}），军心 {campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
					MobilizationSummary = $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowReactiveChinese(campaign.MobilizationWindowLabel)}。",
					SupplyLineSummary = campaign.SupplyLineSummary,
					CoordinationSummary = campaign.OfficeCoordinationTrace,
					CommanderSummary = campaign.CommanderSummary,
					AftermathSummary = campaign.LastAftermathSummary,
					Routes = campaign.Routes.Select((CampaignRouteSnapshot route) => new CampaignRouteViewModel
					{
						RouteLabel = route.RouteLabel,
						RouteRole = route.RouteRole,
						FlowStateLabel = route.FlowStateLabel,
						Summary = route.Summary
					}).ToArray()
				}).ToArray(),
			MobilizationSignals = (from signal in bundle.CampaignMobilizationSignals
				orderby signal.ResponseActivationLevel descending, signal.SettlementId.Value
				select new CampaignMobilizationSignalViewModel
				{
					SettlementLabel = signal.SettlementName,
					WindowLabel = DescribeMobilizationWindowReactiveChinese(signal.MobilizationWindowLabel),
					ForceSummary = $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。",
					CommandFitLabel = signal.CommandFitLabel,
					DirectiveLabel = signal.ActiveDirectiveLabel,
					DirectiveSummary = signal.ActiveDirectiveSummary,
					OfficeSummary = ((signal.OfficeAuthorityTier > 0) ? $"官绅层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。" : "暂无官绅文移接应。"),
					SourceTrace = signal.SourceTrace
				}).ToArray()
		};
	}

	private static string BuildGreatHallWarfareSummaryReactiveChinese(PresentationReadModelBundle bundle)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot campaignFrontSnapshot = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		return $"现有 {bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive)} 处在案行营；{campaignFrontSnapshot.AnchorSettlementName}当前{campaignFrontSnapshot.FrontLabel}、{campaignFrontSnapshot.SupplyStateLabel}，案头呈{BuildCampaignBoardEnvironmentLabel(campaignFrontSnapshot)}。";
	}

	private static string BuildSettlementCampaignSummaryReactiveChinese(CampaignFrontSnapshot? campaign, CampaignMobilizationSignalSnapshot? signal)
	{
		if (campaign != null)
		{
			CampaignRouteSnapshot campaignRouteSnapshot = campaign.Routes.FirstOrDefault();
			string value = ((campaignRouteSnapshot == null) ? "暂无路况细目" : (campaignRouteSnapshot.RouteLabel + campaignRouteSnapshot.FlowStateLabel));
			return $"{campaign.CampaignName}：{BuildCampaignBoardEnvironmentLabel(campaign)}，{campaign.ActiveDirectiveLabel}，{value}。";
		}
		if (signal != null)
		{
			return $"{DescribeMobilizationWindowReactiveChinese(signal.MobilizationWindowLabel)}动员窗，应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}，军令 {signal.ActiveDirectiveLabel}。";
		}
		return "暂无军务沙盘投影。";
	}

	private static string BuildWarfareSurfaceSummaryReactiveChinese(PresentationReadModelBundle bundle, int activeCampaignCount, int peakFrontPressure)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot campaignFrontSnapshot = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		return $"现有 {activeCampaignCount} 处在案行营，峰值前线压力 {peakFrontPressure}；{campaignFrontSnapshot.AnchorSettlementName}正以 {campaignFrontSnapshot.CommandFitLabel} 维持 {campaignFrontSnapshot.FrontLabel}，案头呈{BuildCampaignBoardEnvironmentLabel(campaignFrontSnapshot)}。";
	}

	private static string DescribeMobilizationWindowReactiveChinese(string mobilizationWindowLabel)
	{
		if (1 == 0)
		{
		}
		string result = mobilizationWindowLabel switch
		{
			"Open" => "可开", 
			"Narrow" => "可守", 
			"Preparing" => "待整", 
			_ => "已闭", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string BuildCampaignBoardEnvironmentLabel(CampaignFrontSnapshot campaign)
	{
		if (!campaign.IsActive)
		{
			return "收卷校核";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "WithdrawToBarracks", StringComparison.Ordinal))
		{
			return "收军归营";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "CommitMobilization", StringComparison.Ordinal))
		{
			return "鼓角催集";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "DraftCampaignPlan", StringComparison.Ordinal))
		{
			return "舆图铺案";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "ProtectSupplyLine", StringComparison.Ordinal))
		{
			return (campaign.FrontPressure >= 60) ? "粮筹压阵" : "粮驿先行";
		}
		if (campaign.FrontPressure >= 75 && campaign.MoraleState < 45)
		{
			return "烽尘压案";
		}
		if (campaign.SupplyState < 40)
		{
			return "粮线告急";
		}
		if (campaign.MoraleState < 40)
		{
			return "军心低徊";
		}
		return "行营在案";
	}

	private static string BuildCampaignBoardSurfaceLabel(CampaignFrontSnapshot campaign)
	{
		CampaignRouteSnapshot campaignRouteSnapshot = SelectLeadCampaignRoute(campaign);
		if (!campaign.IsActive)
		{
			return "营旗收束，后营册页、伤损簿与善后批答占住案心。";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "ProtectSupplyLine", StringComparison.Ordinal))
		{
			return (campaignRouteSnapshot == null) ? "粮签、渡口木筹与护运行签压在案心，前锋旗退到边角。" : ("案心多是" + campaignRouteSnapshot.RouteLabel + "筹签，前锋旗让位于护运与驿递木筹。");
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "CommitMobilization", StringComparison.Ordinal))
		{
			return "点军签、乡勇册与营旗铺开，案边尽是催集批注。";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "DraftCampaignPlan", StringComparison.Ordinal))
		{
			return "舆图平展，朱笔批注多于军旗，先看路线后议进退。";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "WithdrawToBarracks", StringComparison.Ordinal))
		{
			return "营旗后撤，路签收束，案面转向后营与善后册页。";
		}
		if (campaign.FrontPressure >= campaign.SupplyState)
		{
			return "前锋旗与急报纸条堆向案前，粮签紧贴其后。";
		}
		return "粮签、驿筹与渡口木牌并列案心，前锋旗压在边沿。";
	}

	private static string BuildCampaignBoardAtmosphereSummary(CampaignFrontSnapshot campaign)
	{
		if (!campaign.IsActive)
		{
			return "此案已转为战后覆核：" + campaign.LastAftermathSummary;
		}
		return $"此案呈{BuildCampaignBoardEnvironmentLabel(campaign)}之势：{campaign.FrontLabel}，{campaign.SupplyStateLabel}，{campaign.MoraleStateLabel}；军令为{campaign.ActiveDirectiveLabel}，{DescribeCampaignRouteMix(campaign)}。";
	}

	private static string BuildCampaignBoardMarkerSummary(CampaignFrontSnapshot campaign)
	{
		CampaignRouteSnapshot campaignRouteSnapshot = SelectLeadCampaignRoute(campaign);
		if (campaignRouteSnapshot == null)
		{
			return "案上主要凭前锋旗、粮簿与军报批注辨势。";
		}
		CampaignRouteSnapshot campaignRouteSnapshot2 = (from route in campaign.Routes
			orderby route.Pressure descending, route.Security descending
			select route).ThenBy<CampaignRouteSnapshot, string>((CampaignRouteSnapshot route) => route.RouteLabel, StringComparer.Ordinal).Skip(1).FirstOrDefault();
		if (campaignRouteSnapshot2 != null)
		{
			return $"案头以{campaignRouteSnapshot.RouteLabel}为主线，并由{campaignRouteSnapshot2.RouteLabel}牵住侧边。";
		}
		return $"案头主线落在{campaignRouteSnapshot.RouteLabel}，其势为{campaignRouteSnapshot.FlowStateLabel}。";
	}

	private static string DescribeCampaignRouteMix(CampaignFrontSnapshot campaign)
	{
		CampaignRouteSnapshot campaignRouteSnapshot = SelectLeadCampaignRoute(campaign);
		if (campaignRouteSnapshot == null)
		{
			return "案上仍以军报和前锋旗为主";
		}
		CampaignRouteSnapshot campaignRouteSnapshot2 = (from route in campaign.Routes
			orderby route.Pressure descending, route.Security descending
			select route).ThenBy<CampaignRouteSnapshot, string>((CampaignRouteSnapshot route) => route.RouteLabel, StringComparer.Ordinal).Skip(1).FirstOrDefault();
		if (campaignRouteSnapshot2 == null)
		{
			return "主看" + campaignRouteSnapshot.RouteLabel + "，其势" + campaignRouteSnapshot.FlowStateLabel;
		}
		return $"主看{campaignRouteSnapshot.RouteLabel}，并以{campaignRouteSnapshot2.RouteLabel}牵住侧边";
	}

	private static CampaignRouteSnapshot? SelectLeadCampaignRoute(CampaignFrontSnapshot campaign)
	{
		return (from route in campaign.Routes
			orderby route.Pressure descending, route.Security descending
			select route).ThenBy<CampaignRouteSnapshot, string>((CampaignRouteSnapshot route) => route.RouteLabel, StringComparer.Ordinal).FirstOrDefault();
	}

	private static WarfareSurfaceViewModel BuildWarfareSurfaceRegionalChinese(PresentationReadModelBundle bundle, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
		{
			return new WarfareSurfaceViewModel
			{
				Summary = "暂无军务沙盘投影。"
			};
		}
		int activeCampaignCount = bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive);
		int peakFrontPressure = ((bundle.Campaigns.Count != 0) ? bundle.Campaigns.Max((CampaignFrontSnapshot campaign) => campaign.FrontPressure) : 0);
		Dictionary<int, SettlementSnapshot> settlementsById = bundle.Settlements.ToDictionary((SettlementSnapshot settlement) => settlement.Id.Value, (SettlementSnapshot settlement) => settlement);
		ILookup<int, TradeRouteSnapshot> tradeRoutesBySettlement = bundle.TradeRoutes.ToLookup((TradeRouteSnapshot route) => route.SettlementId.Value);
		return new WarfareSurfaceViewModel
		{
			Summary = BuildWarfareSurfaceSummaryRegionalChinese(bundle, activeCampaignCount, peakFrontPressure),
			CommandAffordances = (from command in (from command in bundle.PlayerCommands.Affordances
					where string.Equals(command.SurfaceKey, "Warfare", StringComparison.Ordinal)
					orderby command.SettlementId.Value
					select command).ThenBy<PlayerCommandAffordanceSnapshot, string>((PlayerCommandAffordanceSnapshot command) => command.CommandName, StringComparer.Ordinal)
				select new CommandAffordanceViewModel
				{
					CommandName = command.CommandName,
					Label = command.Label,
					Summary = command.Summary,
					AvailabilitySummary = command.AvailabilitySummary,
					IsEnabled = command.IsEnabled
				}).ToArray(),
			RecentReceipts = (from receipt in (from receipt in bundle.PlayerCommands.Receipts
					where string.Equals(receipt.SurfaceKey, "Warfare", StringComparison.Ordinal)
					orderby receipt.SettlementId.Value
					select receipt).ThenBy<PlayerCommandReceiptSnapshot, string>((PlayerCommandReceiptSnapshot receipt) => receipt.CommandName, StringComparer.Ordinal)
				select new CommandReceiptViewModel
				{
					CommandName = receipt.CommandName,
					Label = receipt.Label,
					Summary = RenderCampaignSurfaceText(receipt.Summary),
					OutcomeSummary = RenderCampaignSurfaceText(receipt.OutcomeSummary)
				}).ToArray(),
			CampaignBoards = (from campaign in bundle.Campaigns
				orderby campaign.IsActive descending, campaign.FrontPressure descending, campaign.CampaignId.Value
				select campaign).Select(delegate(CampaignFrontSnapshot campaign)
			{
				SettlementSnapshot value;
				SettlementSnapshot settlement2 = (settlementsById.TryGetValue(campaign.AnchorSettlementId.Value, out value) ? value : null);
				TradeRouteSnapshot[] tradeRoutes = tradeRoutesBySettlement[campaign.AnchorSettlementId.Value].OrderBy<TradeRouteSnapshot, string>((TradeRouteSnapshot route) => route.RouteName, StringComparer.Ordinal).ToArray();
				RegionalBoardProfile regionalProfile = BuildCampaignRegionalProfile(campaign, settlement2, tradeRoutes);
				return new CampaignBoardViewModel
				{
					CampaignName = RenderCampaignSurfaceText(campaign.CampaignName),
					SettlementLabel = campaign.AnchorSettlementName,
					StatusLabel = (campaign.IsActive ? "行营在案" : "战后覆核"),
					RegionalProfileLabel = regionalProfile.Label,
					RegionalBackdropSummary = regionalProfile.BackdropSummary,
					EnvironmentLabel = BuildCampaignConditionLabelChinese(campaign),
					BoardSurfaceLabel = BuildCampaignBoardSurfaceRegionalChinese(campaign, regionalProfile),
					BoardAtmosphereSummary = BuildCampaignBoardAtmosphereRegionalChinese(campaign, regionalProfile),
					MarkerSummary = BuildCampaignBoardMarkerRegionalChinese(campaign, regionalProfile),
					FrontLabel = campaign.FrontLabel,
					SupplyStateLabel = campaign.SupplyStateLabel,
					MoraleStateLabel = campaign.MoraleStateLabel,
					CommandFitLabel = campaign.CommandFitLabel,
					DirectiveLabel = campaign.ActiveDirectiveLabel,
					DirectiveSummary = RenderCampaignSurfaceText(campaign.ActiveDirectiveSummary),
					DirectiveTrace = RenderCampaignSurfaceText(campaign.LastDirectiveTrace),
					ObjectiveSummary = RenderCampaignSurfaceText(campaign.ObjectiveSummary),
					FrontSummary = $"{campaign.FrontLabel}：前线 {campaign.FrontPressure}，粮道 {campaign.SupplyState}（{campaign.SupplyStateLabel}），军心 {campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
					MobilizationSummary = $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowRegionalChinese(campaign.MobilizationWindowLabel)}。",
					SupplyLineSummary = RenderCampaignSurfaceText(campaign.SupplyLineSummary),
					CoordinationSummary = RenderCampaignSurfaceText(campaign.OfficeCoordinationTrace),
					CommanderSummary = RenderCampaignSurfaceText(campaign.CommanderSummary),
					AftermathSummary = RenderCampaignSurfaceText(campaign.LastAftermathSummary),
					AftermathDocketSummary = BuildCampaignAftermathDocketSummary(campaign, settlement2, notifications),
					Routes = campaign.Routes.Select((CampaignRouteSnapshot route) => new CampaignRouteViewModel
					{
						RouteLabel = route.RouteLabel,
						RouteRole = route.RouteRole,
						FlowStateLabel = route.FlowStateLabel,
						Summary = RenderCampaignSurfaceText(route.Summary)
					}).ToArray()
				};
			}).ToArray(),
			MobilizationSignals = (from signal in bundle.CampaignMobilizationSignals
				orderby signal.ResponseActivationLevel descending, signal.SettlementId.Value
				select new CampaignMobilizationSignalViewModel
				{
					SettlementLabel = signal.SettlementName,
					WindowLabel = DescribeMobilizationWindowRegionalChinese(signal.MobilizationWindowLabel),
					ForceSummary = $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。",
					CommandFitLabel = signal.CommandFitLabel,
					DirectiveLabel = signal.ActiveDirectiveLabel,
					DirectiveSummary = RenderCampaignSurfaceText(signal.ActiveDirectiveSummary),
					OfficeSummary = ((signal.OfficeAuthorityTier > 0) ? $"官绅层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。" : "暂无官绅文移接应。"),
					SourceTrace = RenderCampaignSurfaceText(signal.SourceTrace)
				}).ToArray()
		};
	}

	private static string BuildGreatHallWarfareSummaryRegionalChinese(PresentationReadModelBundle bundle)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot leadCampaign = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		SettlementSnapshot settlement = bundle.Settlements.FirstOrDefault((SettlementSnapshot candidate) => candidate.Id == leadCampaign.AnchorSettlementId);
		TradeRouteSnapshot[] tradeRoutes = bundle.TradeRoutes.Where((TradeRouteSnapshot route) => route.SettlementId == leadCampaign.AnchorSettlementId).OrderBy<TradeRouteSnapshot, string>((TradeRouteSnapshot route) => route.RouteName, StringComparer.Ordinal).ToArray();
		RegionalBoardProfile regionalBoardProfile = BuildCampaignRegionalProfile(leadCampaign, settlement, tradeRoutes);
		return $"现有 {bundle.Campaigns.Count((CampaignFrontSnapshot campaign) => campaign.IsActive)} 处在案行营；{leadCampaign.AnchorSettlementName}当前{leadCampaign.FrontLabel}、{leadCampaign.SupplyStateLabel}，属{regionalBoardProfile.Label}之局，案头呈{BuildCampaignConditionLabelChinese(leadCampaign)}。";
	}

	private static string BuildGreatHallAftermathDocketSummary(PresentationReadModelBundle bundle, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "堂上尚无战后案牍。";
		}
		CampaignFrontSnapshot leadCampaign = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		SettlementSnapshot settlement = bundle.Settlements.FirstOrDefault((SettlementSnapshot candidate) => candidate.Id == leadCampaign.AnchorSettlementId);
		PopulationSettlementSnapshot population = bundle.PopulationSettlements.FirstOrDefault((PopulationSettlementSnapshot candidate) => candidate.SettlementId == leadCampaign.AnchorSettlementId);
		JurisdictionAuthoritySnapshot jurisdiction = bundle.OfficeJurisdictions.FirstOrDefault((JurisdictionAuthoritySnapshot candidate) => candidate.SettlementId == leadCampaign.AnchorSettlementId);
		AftermathDocketSignals aftermathDocketSignals = BuildAftermathDocketSignals(leadCampaign.AnchorSettlementId, settlement, population, jurisdiction, leadCampaign, notifications);
		if (!aftermathDocketSignals.HasSignals)
		{
			return leadCampaign.AnchorSettlementName + "堂案仍以军报与粮道札记为主。";
		}
		return leadCampaign.AnchorSettlementName + "堂案今并载" + aftermathDocketSignals.ComposeClauseText(useArticle: false) + "。";
	}

	private static string BuildSettlementCampaignSummaryRegionalChinese(CampaignFrontSnapshot? campaign, CampaignMobilizationSignalSnapshot? signal, SettlementSnapshot settlement, IReadOnlyList<TradeRouteSnapshot> tradeRoutes)
	{
		if (campaign != null)
		{
			RegionalBoardProfile regionalBoardProfile = BuildCampaignRegionalProfile(campaign, settlement, tradeRoutes);
			CampaignRouteSnapshot campaignRouteSnapshot = SelectLeadCampaignRoute(campaign);
			string value = ((campaignRouteSnapshot == null) ? "暂无路况细目" : (campaignRouteSnapshot.RouteLabel + campaignRouteSnapshot.FlowStateLabel));
			return $"{campaign.CampaignName}：{regionalBoardProfile.Label}，{BuildCampaignConditionLabelChinese(campaign)}，{value}。";
		}
		if (signal != null)
		{
			return $"{DescribeMobilizationWindowRegionalChinese(signal.MobilizationWindowLabel)}动员窗，应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}，军令 {signal.ActiveDirectiveLabel}。";
		}
		return "暂无军务沙盘投影。";
	}

	private static string BuildSettlementAftermathSummary(SettlementSnapshot settlement, PopulationSettlementSnapshot? population, JurisdictionAuthoritySnapshot? jurisdiction, CampaignFrontSnapshot? campaign, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		AftermathDocketSignals aftermathDocketSignals = BuildAftermathDocketSignals(settlement.Id, settlement, population, jurisdiction, campaign, notifications);
		if (!aftermathDocketSignals.HasSignals)
		{
			return "战后案牍未起。";
		}
		return "战后案牍：" + aftermathDocketSignals.ComposeClauseText(useArticle: true) + "。";
	}

	private static string BuildCampaignAftermathDocketSummary(CampaignFrontSnapshot campaign, SettlementSnapshot? settlement, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		AftermathDocketSignals aftermathDocketSignals = BuildAftermathDocketSignals(campaign.AnchorSettlementId, settlement, null, null, campaign, notifications);
		if (!aftermathDocketSignals.HasSignals)
		{
			return campaign.IsActive ? "军机案下仍止于军报与路报。" : "军机案下尚未并成赏罚抚恤诸册。";
		}
		return "军机案今并载" + aftermathDocketSignals.ComposeClauseText(useArticle: false) + "。";
	}

	private static AftermathDocketSignals BuildAftermathDocketSignals(SettlementId settlementId, SettlementSnapshot? settlement, PopulationSettlementSnapshot? population, JurisdictionAuthoritySnapshot? jurisdiction, CampaignFrontSnapshot? campaign, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		string settlementKey = settlementId.Value.ToString();
		NarrativeNotificationSnapshot[] source = notifications.Where((NarrativeNotificationSnapshot notification) => string.Equals(notification.SourceModuleKey, "WarfareCampaign", StringComparison.Ordinal) || notification.Traces.Any((NotificationTraceSnapshot trace) => string.Equals(trace.EntityKey, settlementKey, StringComparison.Ordinal))).ToArray();
		bool merit = source.Any((NarrativeNotificationSnapshot notification) => notification.Traces.Any((NotificationTraceSnapshot trace) => string.Equals(trace.SourceModuleKey, "FamilyCore", StringComparison.Ordinal) || string.Equals(trace.SourceModuleKey, "SocialMemoryAndRelations", StringComparison.Ordinal))) || (campaign != null && campaign.IsActive && campaign.MoraleState >= 55 && campaign.SupplyState >= 50);
		bool blame = source.Any((NarrativeNotificationSnapshot notification) => notification.Traces.Any((NotificationTraceSnapshot trace) => string.Equals(trace.SourceModuleKey, "OfficeAndCareer", StringComparison.Ordinal) || string.Equals(trace.SourceModuleKey, "ConflictAndForce", StringComparison.Ordinal))) || (campaign != null && (!campaign.IsActive || campaign.FrontPressure >= 60)) || (jurisdiction?.PetitionBacklog ?? 0) >= 8;
		bool relief = source.Any((NarrativeNotificationSnapshot notification) => notification.Traces.Any((NotificationTraceSnapshot trace) => string.Equals(trace.SourceModuleKey, "PopulationAndHouseholds", StringComparison.Ordinal) || string.Equals(trace.SourceModuleKey, "WorldSettlements", StringComparison.Ordinal))) || (population != null && (population.CommonerDistress >= 40 || population.MigrationPressure >= 35)) || (settlement != null && (settlement.Security <= 55 || settlement.Prosperity <= 58));
		bool disorder = source.Any((NarrativeNotificationSnapshot notification) => notification.Traces.Any((NotificationTraceSnapshot trace) => string.Equals(trace.SourceModuleKey, "OrderAndBanditry", StringComparison.Ordinal) || string.Equals(trace.SourceModuleKey, "TradeAndIndustry", StringComparison.Ordinal))) || (settlement != null && settlement.Security < 58) || (campaign != null && (campaign.SupplyState < 45 || campaign.Routes.Any((CampaignRouteSnapshot route) => route.Pressure > route.Security)));
		return new AftermathDocketSignals
		{
			Merit = merit,
			Blame = blame,
			Relief = relief,
			Disorder = disorder
		};
	}

	private static string BuildWarfareSurfaceSummaryRegionalChinese(PresentationReadModelBundle bundle, int activeCampaignCount, int peakFrontPressure)
	{
		if (bundle.Campaigns.Count == 0)
		{
			return "暂无军务沙盘投影。";
		}
		CampaignFrontSnapshot leadCampaign = (from campaign in bundle.Campaigns
			orderby campaign.FrontPressure descending, campaign.MobilizedForceCount descending, campaign.CampaignId.Value
			select campaign).First();
		SettlementSnapshot settlement = bundle.Settlements.FirstOrDefault((SettlementSnapshot candidate) => candidate.Id == leadCampaign.AnchorSettlementId);
		TradeRouteSnapshot[] tradeRoutes = bundle.TradeRoutes.Where((TradeRouteSnapshot route) => route.SettlementId == leadCampaign.AnchorSettlementId).OrderBy<TradeRouteSnapshot, string>((TradeRouteSnapshot route) => route.RouteName, StringComparer.Ordinal).ToArray();
		RegionalBoardProfile regionalBoardProfile = BuildCampaignRegionalProfile(leadCampaign, settlement, tradeRoutes);
		return $"现有 {activeCampaignCount} 处在案行营，峰值前线压力 {peakFrontPressure}；{leadCampaign.AnchorSettlementName}正以 {leadCampaign.CommandFitLabel} 维持 {leadCampaign.FrontLabel}，属{regionalBoardProfile.Label}之局。";
	}

	private static string DescribeMobilizationWindowRegionalChinese(string mobilizationWindowLabel)
	{
		if (1 == 0)
		{
		}
		string result = mobilizationWindowLabel switch
		{
			"Open" => "可开", 
			"Narrow" => "可守", 
			"Preparing" => "待整", 
			_ => "已闭", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string BuildCampaignConditionLabelChinese(CampaignFrontSnapshot campaign)
	{
		if (!campaign.IsActive)
		{
			return "收卷校核";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "WithdrawToBarracks", StringComparison.Ordinal))
		{
			return "收军归营";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "CommitMobilization", StringComparison.Ordinal))
		{
			return "鼓角催集";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "DraftCampaignPlan", StringComparison.Ordinal))
		{
			return "舆图铺案";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "ProtectSupplyLine", StringComparison.Ordinal))
		{
			return (campaign.FrontPressure >= 60) ? "粮筹压阵" : "粮驿先行";
		}
		if (campaign.FrontPressure >= 75 && campaign.MoraleState < 45)
		{
			return "烽尘压案";
		}
		if (campaign.SupplyState < 40)
		{
			return "粮线告急";
		}
		if (campaign.MoraleState < 40)
		{
			return "军心低徊";
		}
		return "行营在案";
	}

	private static string BuildCampaignBoardSurfaceRegionalChinese(CampaignFrontSnapshot campaign, RegionalBoardProfile regionalProfile)
	{
		string backdropSummary = regionalProfile.BackdropSummary;
		CampaignRouteSnapshot campaignRouteSnapshot = SelectLeadCampaignRoute(campaign);
		if (!campaign.IsActive)
		{
			return backdropSummary + " 营旗收束，后营册页、伤损簿与善后批答占住案心。";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "ProtectSupplyLine", StringComparison.Ordinal))
		{
			return (campaignRouteSnapshot == null) ? (backdropSummary + " 粮签、渡口木筹与护运行签压在案心，前锋旗退到边角。") : (backdropSummary + " 案心多是" + campaignRouteSnapshot.RouteLabel + "筹签，前锋旗让位于护运与驿递木筹。");
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "CommitMobilization", StringComparison.Ordinal))
		{
			return backdropSummary + " 点军签、乡勇册与营旗铺开，案边尽是催集批注。";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "DraftCampaignPlan", StringComparison.Ordinal))
		{
			return backdropSummary + " 舆图平展，朱笔批注多于军旗，先看路线后议进退。";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "WithdrawToBarracks", StringComparison.Ordinal))
		{
			return backdropSummary + " 营旗后撤，路签收束，案面转向后营与善后册页。";
		}
		if (campaign.FrontPressure >= campaign.SupplyState)
		{
			return backdropSummary + " 前锋旗与急报纸条堆向案前，粮签紧贴其后。";
		}
		return backdropSummary + " 粮签、驿筹与渡口木牌并列案心，前锋旗压在边沿。";
	}

	private static string BuildCampaignBoardAtmosphereRegionalChinese(CampaignFrontSnapshot campaign, RegionalBoardProfile regionalProfile)
	{
		if (!campaign.IsActive)
		{
			return "此案属" + regionalProfile.Label + "之局，已转为战后覆核：" + campaign.LastAftermathSummary;
		}
		return $"此案属{regionalProfile.Label}之局，呈{BuildCampaignConditionLabelChinese(campaign)}之势：{campaign.FrontLabel}，{campaign.SupplyStateLabel}，{campaign.MoraleStateLabel}；军令为{campaign.ActiveDirectiveLabel}，{DescribeCampaignRouteMixRegionalChinese(campaign)}。";
	}

	private static string BuildCampaignBoardMarkerRegionalChinese(CampaignFrontSnapshot campaign, RegionalBoardProfile regionalProfile)
	{
		CampaignRouteSnapshot campaignRouteSnapshot = SelectLeadCampaignRoute(campaign);
		if (campaignRouteSnapshot == null)
		{
			return regionalProfile.Label + "案主要凭前锋旗、粮簿与军报批注辨势。";
		}
		CampaignRouteSnapshot campaignRouteSnapshot2 = (from route in campaign.Routes
			orderby route.Pressure descending, route.Security descending
			select route).ThenBy<CampaignRouteSnapshot, string>((CampaignRouteSnapshot route) => route.RouteLabel, StringComparer.Ordinal).Skip(1).FirstOrDefault();
		if (campaignRouteSnapshot2 != null)
		{
			return $"{regionalProfile.Label}案头以{campaignRouteSnapshot.RouteLabel}为主线，并由{campaignRouteSnapshot2.RouteLabel}牵住侧边。";
		}
		return $"{regionalProfile.Label}案头主线落在{campaignRouteSnapshot.RouteLabel}，其势为{campaignRouteSnapshot.FlowStateLabel}。";
	}

	private static string DescribeCampaignRouteMixRegionalChinese(CampaignFrontSnapshot campaign)
	{
		CampaignRouteSnapshot campaignRouteSnapshot = SelectLeadCampaignRoute(campaign);
		if (campaignRouteSnapshot == null)
		{
			return "案上仍以军报和前锋旗为主";
		}
		CampaignRouteSnapshot campaignRouteSnapshot2 = (from route in campaign.Routes
			orderby route.Pressure descending, route.Security descending
			select route).ThenBy<CampaignRouteSnapshot, string>((CampaignRouteSnapshot route) => route.RouteLabel, StringComparer.Ordinal).Skip(1).FirstOrDefault();
		if (campaignRouteSnapshot2 == null)
		{
			return "主看" + campaignRouteSnapshot.RouteLabel + "，其势" + campaignRouteSnapshot.FlowStateLabel;
		}
		return $"主看{campaignRouteSnapshot.RouteLabel}，并以{campaignRouteSnapshot2.RouteLabel}牵住侧边";
	}

	private static RegionalBoardProfile BuildCampaignRegionalProfile(CampaignFrontSnapshot campaign, SettlementSnapshot? settlement = null, IReadOnlyList<TradeRouteSnapshot>? tradeRoutes = null)
	{
		List<string> list = new List<string>();
		list.Add(campaign.AnchorSettlementName);
		list.Add(campaign.CampaignName);
		list.AddRange(campaign.Routes.Select((CampaignRouteSnapshot route) => route.RouteLabel));
		list.AddRange(campaign.Routes.Select((CampaignRouteSnapshot route) => route.Summary));
		list.AddRange((tradeRoutes ?? Array.Empty<TradeRouteSnapshot>()).Select((TradeRouteSnapshot route) => route.RouteName));
		string[] signals = list.ToArray();
		bool flag = ContainsRegionalSignal(signals, "river", "canal", "ferry", "wharf", "water", "河", "江", "渡", "港", "浦", "漕");
		bool flag2 = ContainsRegionalSignal(signals, "hill", "mountain", "ridge", "pass", "山", "岭", "关", "隘", "谷");
		int num = settlement?.Prosperity ?? 0;
		int num2 = settlement?.Security ?? 0;
		if (flag)
		{
			return (num >= 60) ? new RegionalBoardProfile("水驿商埠", "案旁多铺水线、渡口木牌与舟楫筹。") : new RegionalBoardProfile("江渡县口", "案边常见渡津签、河埠木牌与泊船筹。");
		}
		if (flag2)
		{
			return (num2 >= 50) ? new RegionalBoardProfile("山道关路", "案面多画山道折线、岭口木塞与关津旗。") : new RegionalBoardProfile("岭道荒隘", "案边竖着山口木牌、险路折签与巡哨火签。");
		}
		if (num >= 65)
		{
			return new RegionalBoardProfile("市镇腹地", "案旁多见仓牌、街市路签与税簿角注。");
		}
		if (num2 <= 45)
		{
			return new RegionalBoardProfile("边县危垒", "案边竖着塞门木牌、巡哨火签与守望短旗。");
		}
		return new RegionalBoardProfile("县城平畴", "案上以田畴路签、县门木牌与里坊册页为底。");
	}

	private static bool ContainsRegionalSignal(IEnumerable<string> signals, params string[] markers)
	{
		foreach (string signal in signals)
		{
			if (string.IsNullOrWhiteSpace(signal))
			{
				continue;
			}
			foreach (string value in markers)
			{
				if (signal.Contains(value, StringComparison.OrdinalIgnoreCase) || signal.Contains(value, StringComparison.Ordinal))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static DebugPanelViewModel BuildDebugPanel(PresentationDebugSnapshot debug)
	{
		return new DebugPanelViewModel
		{
			DiagnosticsSchemaLabel = $"v{debug.DiagnosticsSchemaVersion}",
			SeedLabel = debug.InitialSeed.ToString(),
			NotificationRetentionLabel = debug.NotificationRetentionLimit.ToString(),
			Scale = BuildScaleGroup(debug),
			Pressure = BuildPressureGroup(debug),
			Hotspots = BuildHotspotsGroup(debug),
			Migration = BuildMigrationGroup(debug),
			Warnings = BuildWarningsGroup(debug)
		};
	}

	private static DebugScaleGroupViewModel BuildScaleGroup(PresentationDebugSnapshot debug)
	{
		return new DebugScaleGroupViewModel
		{
			LatestMetrics = new DebugMetricSummaryViewModel
			{
				DiffEntryCount = debug.LatestMetrics.DiffEntryCount,
				DomainEventCount = debug.LatestMetrics.DomainEventCount,
				NotificationCount = debug.LatestMetrics.NotificationCount,
				SavePayloadBytes = debug.LatestMetrics.SavePayloadBytes,
				RetentionLimitReached = debug.RetentionLimitReached
			},
			CurrentScale = new DebugScaleSummaryViewModel
			{
				EntitySummary = $"{debug.CurrentScale.SettlementCount} settlements, {debug.CurrentScale.ClanCount} clans, {debug.CurrentScale.HouseholdCount} households.",
				InstitutionSummary = $"{debug.CurrentScale.AcademyCount} academies, {debug.CurrentScale.RouteCount} trade routes.",
				ModuleSummary = $"{debug.CurrentScale.EnabledModuleCount} enabled modules mirrored in {debug.CurrentScale.SavedModuleCount} saved envelopes.",
				NotificationUtilizationLabel = $"{debug.CurrentScale.NotificationCount} notices ({debug.CurrentScale.NotificationUtilizationPercent}% of retention).",
				PayloadDensityLabel = $"{debug.CurrentScale.SavePayloadBytesPerSettlement} save bytes per settlement; {debug.CurrentScale.AverageHouseholdsPerSettlement} households per settlement."
			},
			PayloadSummary = new DebugPayloadSummaryViewModel
			{
				TotalPayloadBytes = debug.CurrentPayloadSummary.TotalModulePayloadBytes,
				LargestModuleKey = debug.CurrentPayloadSummary.LargestModuleKey,
				LargestModulePayloadBytes = debug.CurrentPayloadSummary.LargestModulePayloadBytes,
				LargestModuleShareLabel = $"{(double)debug.CurrentPayloadSummary.LargestModuleShareBasisPoints / 100.0:F2}%",
				Summary = (string.IsNullOrWhiteSpace(debug.CurrentPayloadSummary.LargestModuleKey) ? "No module payload data." : $"{debug.CurrentPayloadSummary.TotalModulePayloadBytes} total bytes; {debug.CurrentPayloadSummary.LargestModuleKey} leads at {debug.CurrentPayloadSummary.LargestModulePayloadBytes} bytes.")
			},
			TopPayloadModules = debug.TopPayloadModules.Select((ModulePayloadFootprintSnapshot payload) => new DebugPayloadFootprintViewModel
			{
				ModuleKey = payload.ModuleKey,
				PayloadBytes = payload.PayloadBytes,
				ShareLabel = $"{(double)payload.PayloadShareBasisPoints / 100.0:F2}%"
			}).ToArray(),
			EnabledModules = debug.EnabledModules.Select((DebugFeatureModeSnapshot module) => module.ModuleKey + ":" + module.Mode).ToArray(),
			ModuleInspectors = debug.ModuleInspectors.Select((DebugModuleInspectorSnapshot inspector) => new DebugModuleInspectorViewModel
			{
				ModuleKey = inspector.ModuleKey,
				SchemaVersion = inspector.ModuleSchemaVersion,
				PayloadBytes = inspector.PayloadBytes
			}).ToArray()
		};
	}

	private static DebugPressureGroupViewModel BuildPressureGroup(PresentationDebugSnapshot debug)
	{
		return new DebugPressureGroupViewModel
		{
			Interaction = new DebugInteractionPressureViewModel
			{
				ActiveConflictSettlements = debug.CurrentInteractionPressure.ActiveConflictSettlements,
				ActivatedResponseSettlements = debug.CurrentInteractionPressure.ActivatedResponseSettlements,
				SupportedOrderSettlements = debug.CurrentInteractionPressure.SupportedOrderSettlements,
				HighSuppressionDemandSettlements = debug.CurrentInteractionPressure.HighSuppressionDemandSettlements,
				AverageSuppressionDemand = debug.CurrentInteractionPressure.AverageSuppressionDemand,
				PeakSuppressionDemand = debug.CurrentInteractionPressure.PeakSuppressionDemand,
				HighBanditThreatSettlements = debug.CurrentInteractionPressure.HighBanditThreatSettlements,
				Summary = $"{debug.CurrentInteractionPressure.ActivatedResponseSettlements} activated, {debug.CurrentInteractionPressure.HighSuppressionDemandSettlements} high suppression, peak demand {debug.CurrentInteractionPressure.PeakSuppressionDemand}."
			},
			Distribution = new DebugPressureDistributionViewModel
			{
				CalmSettlements = debug.CurrentPressureDistribution.CalmSettlements,
				WatchedSettlements = debug.CurrentPressureDistribution.WatchedSettlements,
				StressedSettlements = debug.CurrentPressureDistribution.StressedSettlements,
				CrisisSettlements = debug.CurrentPressureDistribution.CrisisSettlements,
				Summary = $"{debug.CurrentPressureDistribution.CalmSettlements} calm, {debug.CurrentPressureDistribution.WatchedSettlements} watched, {debug.CurrentPressureDistribution.StressedSettlements} stressed, {debug.CurrentPressureDistribution.CrisisSettlements} crisis."
			}
		};
	}

	private static DebugHotspotsGroupViewModel BuildHotspotsGroup(PresentationDebugSnapshot debug)
	{
		return new DebugHotspotsGroupViewModel
		{
			CurrentHotspots = debug.CurrentHotspots.Select((SettlementInteractionHotspotSnapshot hotspot) => new DebugHotspotViewModel
			{
				SettlementName = hotspot.SettlementName,
				HotspotScore = hotspot.HotspotScore,
				PressureSummary = $"Bandit {hotspot.BanditThreat}, route {hotspot.RoutePressure}, suppression {hotspot.SuppressionDemand}.",
				ResponseSummary = (hotspot.IsResponseActivated ? $"Active response {hotspot.ResponseActivationLevel} with support {hotspot.OrderSupportLevel}." : "No active response support.")
			}).ToArray(),
			DiffTraces = debug.RecentDiffEntries.Select((DebugDiffTraceSnapshot trace) => new DebugTraceItemViewModel
			{
				ModuleKey = trace.ModuleKey,
				Summary = trace.Description,
				EntityKey = trace.EntityKey
			}).ToArray(),
			DomainEvents = debug.RecentDomainEvents.Select((DebugDomainEventSnapshot domainEvent) => new DebugEventItemViewModel
			{
				ModuleKey = domainEvent.ModuleKey,
				EventType = domainEvent.EventType,
				Summary = domainEvent.Summary
			}).ToArray()
		};
	}

	private static DebugMigrationGroupViewModel BuildMigrationGroup(PresentationDebugSnapshot debug)
	{
		return new DebugMigrationGroupViewModel
		{
			LoadOriginLabel = debug.LoadMigration.LoadOriginLabel,
			MigrationStatusLabel = (debug.LoadMigration.ConsistencyPassed ? "Consistency passed" : "Consistency warnings present"),
			MigrationSummary = debug.LoadMigration.Summary,
			MigrationConsistencySummary = debug.LoadMigration.ConsistencySummary,
			MigrationStepCountLabel = $"{debug.LoadMigration.StepCount} migration step(s)",
			MigrationSteps = debug.LoadMigration.Steps.Select((DebugMigrationStepSnapshot step) => $"{step.ScopeLabel}:{step.SourceVersion}->{step.TargetVersion}").ToArray()
		};
	}

	private static DebugWarningsGroupViewModel BuildWarningsGroup(PresentationDebugSnapshot debug)
	{
		return new DebugWarningsGroupViewModel
		{
			Messages = debug.Warnings,
			Invariants = debug.Invariants
		};
	}
}
