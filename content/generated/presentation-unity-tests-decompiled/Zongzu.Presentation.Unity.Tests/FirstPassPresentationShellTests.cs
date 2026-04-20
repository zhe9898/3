using System;
using System.Linq;
using NUnit.Framework;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity.Tests;

[TestFixture]
public sealed class FirstPassPresentationShellTests
{
	[Test]
	public void Compose_ProjectsMonthlyPublicLifeCadenceIntoGreatHallAndDeskSandbox()
	{
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(CreateBundle());
		Assert.That(presentationShellViewModel.GreatHall.PublicLifeSummary, Does.Contain("春社集日"));
		Assert.That(presentationShellViewModel.GreatHall.PublicLifeSummary, Does.Contain("客商"));
		Assert.That(presentationShellViewModel.GreatHall.PublicLifeSummary, Does.Contain("说法相左").Or.Contain("榜文").Or.Contain("街谈"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements, Has.Count.EqualTo(1));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("春社集日"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("街口茶肆"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("榜文").Or.Contain("街谈").Or.Contain("路报"));
	}

	[Test]
	public void Compose_ProjectsFamilyCouncilCommandsAndReceipts()
	{
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(CreateBundle());
		Assert.That(presentationShellViewModel.FamilyCouncil.Clans, Has.Count.EqualTo(1));
		Assert.That(presentationShellViewModel.GreatHall.FamilySummary, Does.Contain("承祧").Or.Contain("婚议").Or.Contain("举哀"));
		Assert.That(presentationShellViewModel.GreatHall.FamilySummary, Does.Contain("宜先议定承祧"));
		Assert.That(presentationShellViewModel.FamilyCouncil.Clans[0].LifecycleSummary, Does.Contain("议亲定婚").And.Contain("承祧"));
		Assert.That(presentationShellViewModel.FamilyCouncil.Clans[0].LifecycleSummary, Does.Contain("眼下宜先议定承祧"));
		Assert.That(presentationShellViewModel.FamilyCouncil.Clans[0].ClanName, Is.EqualTo("清河张氏"));
		Assert.That(presentationShellViewModel.FamilyCouncil.CommandAffordances, Has.Count.EqualTo(2));
		Assert.That(presentationShellViewModel.FamilyCouncil.CommandAffordances.Any((CommandAffordanceViewModel command) => command.CommandName == "InviteClanEldersMediation"), Is.True);
		Assert.That(presentationShellViewModel.FamilyCouncil.CommandAffordances.Any((CommandAffordanceViewModel command) => command.CommandName == "DesignateHeirPolicy"), Is.True);
		Assert.That(presentationShellViewModel.FamilyCouncil.RecentReceipts, Has.Count.EqualTo(1));
		Assert.That(presentationShellViewModel.FamilyCouncil.RecentReceipts[0].OutcomeSummary, Does.Contain("族老"));
		Assert.That(presentationShellViewModel.FamilyCouncil.Summary, Does.Contain("婚事").And.Contain("承祧"));
		Assert.That(presentationShellViewModel.FamilyCouncil.Summary, Does.Contain("眼下最宜先命清河张氏议定承祧"));
	}

	[Test]
	public void Compose_UsesOfficeFallbackWhenGovernanceProjectionIsAbsent()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.OfficeCareers = Array.Empty<OfficeCareerSnapshot>();
		presentationReadModelBundle.OfficeJurisdictions = Array.Empty<JurisdictionAuthoritySnapshot>();
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		Assert.That(presentationShellViewModel.Office.Summary, Does.Contain("暂无官署"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].GovernanceSummary, Does.Contain("官署未设"));
	}

	[Test]
	public void Compose_ProjectsPublicLifeAffordancesAndReceiptsIntoSettlementNodes()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
		{
			Affordances = new global::_003C_003Ez__ReadOnlySingleElementList<PlayerCommandAffordanceSnapshot>(new PlayerCommandAffordanceSnapshot
			{
				ModuleKey = "OfficeAndCareer",
				SurfaceKey = "PublicLife",
				SettlementId = new SettlementId(1),
				CommandName = "PostCountyNotice",
				Label = "张榜晓谕",
				Summary = "先在县门榜下压住街谈。",
				AvailabilitySummary = "榜示与街谈正在相争。",
				TargetLabel = "县门榜下",
				IsEnabled = true
			}),
			Receipts = new global::_003C_003Ez__ReadOnlySingleElementList<PlayerCommandReceiptSnapshot>(new PlayerCommandReceiptSnapshot
			{
				ModuleKey = "OfficeAndCareer",
				SurfaceKey = "PublicLife",
				SettlementId = new SettlementId(1),
				CommandName = "PostCountyNotice",
				Label = "张榜晓谕",
				Summary = "榜文已出，县门口风先压下去。",
				OutcomeSummary = "街谈略收，榜示可见。",
				TargetLabel = "县门榜下"
			})
		};
		presentationReadModelBundle.PublicLifeSettlements = new global::_003C_003Ez__ReadOnlySingleElementList<SettlementPublicLifeSnapshot>(new SettlementPublicLifeSnapshot
		{
			SettlementId = new SettlementId(1),
			SettlementName = "兰溪",
			SettlementTier = SettlementTier.CountySeat,
			NodeLabel = "县门榜下",
			DominantVenueCode = "county-gate",
			DominantVenueLabel = "县门榜下",
			MonthlyCadenceCode = "river-road-bustle",
			MonthlyCadenceLabel = "春汛行旅",
			CrowdMixLabel = "多见客商与脚夫",
			StreetTalkHeat = 58,
			MarketBuzz = 47,
			NoticeVisibility = 61,
			RoadReportLag = 29,
			PrefectureDispatchPressure = 44,
			PublicLegitimacy = 53,
			DocumentaryWeight = 67,
			VerificationCost = 24,
			MarketRumorFlow = 55,
			CourierRisk = 31,
			OfficialNoticeLine = "榜下只说县门已经晓谕轻重。",
			StreetTalkLine = "街口都说埠上传来的话比榜文更紧。",
			RoadReportLine = "路上传来的脚信尚能递到县门。",
			PrefectureDispatchLine = "州牒催意已到，县里仍想缓出几分。",
			ContentionSummary = "榜文、街谈与脚信彼此牵扯，众人还在观望。",
			CadenceSummary = "值春汛行旅，县门榜下多见客商与脚夫。",
			PublicSummary = "街谈渐热，榜示亦重。",
			RouteReportSummary = "路报尚能递到县门。",
			ChannelSummary = "榜示分量稳住场面，市语仍在暗流。",
			LastPublicTrace = "县门榜下人语未散。"
		});
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		SettlementNodeViewModel settlementNodeViewModel = presentationShellViewModel.DeskSandbox.Settlements.Single();
		Assert.That(settlementNodeViewModel.PublicLifeCommandAffordances, Has.Count.EqualTo(1));
		Assert.That(settlementNodeViewModel.PublicLifeCommandAffordances[0].CommandName, Is.EqualTo("PostCountyNotice"));
		Assert.That(settlementNodeViewModel.PublicLifeRecentReceipts, Has.Count.EqualTo(1));
		Assert.That(settlementNodeViewModel.PublicLifeRecentReceipts[0].CommandName, Is.EqualTo("PostCountyNotice"));
		Assert.That(settlementNodeViewModel.PublicLifeSummary, Does.Contain("榜示"));
		Assert.That(settlementNodeViewModel.PublicLifeSummary, Does.Contain("街谈").Or.Contain("观望").Or.Contain("路报"));
	}

	[Test]
	public void Compose_ProjectsGovernanceMomentumIntoGreatHallAndDeskSandbox()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.GovernanceSettlements = new global::_003C_003Ez__ReadOnlySingleElementList<SettlementGovernanceLaneSnapshot>(new SettlementGovernanceLaneSnapshot
		{
			SettlementId = new SettlementId(1),
			SettlementName = "Lanxi",
			NodeLabel = "county-gate",
			LeadOfficialName = "Zhang Yuan",
			LeadOfficeTitle = "Registrar",
			CurrentAdministrativeTask = "district petition hearings",
			AdministrativeTaskLoad = 62,
			PetitionPressure = 51,
			PetitionBacklog = 9,
			PublicLegitimacy = 46,
			StreetTalkHeat = 64,
			RoutePressure = 37,
			SuppressionDemand = 22,
			PublicPressureSummary = "county gate pressure is not yet cleared",
			PublicMomentumSummary = "county gate momentum is tightening",
			GovernanceSummary = "registrar is still triaging petitions"
		});
		presentationReadModelBundle.GovernanceFocus = new GovernanceFocusSnapshot
		{
			SettlementId = new SettlementId(1),
			SettlementName = "Lanxi",
			NodeLabel = "county-gate",
			UrgencyScore = 73,
			LeadSummary = "county gate petitions remain unsettled",
			PublicPressureSummary = "notice and street talk are both rising",
			PublicMomentumSummary = "county gate momentum is tightening",
			SuggestedCommandName = "PostCountyNotice",
			SuggestedCommandLabel = "post notice",
			SuggestedCommandPrompt = "stabilize the county gate surface first"
		};
		presentationReadModelBundle.GovernanceDocket = new GovernanceDocketSnapshot
		{
			SettlementId = new SettlementId(1),
			SettlementName = "Lanxi",
			NodeLabel = "county-gate",
			UrgencyScore = 73,
			Headline = "county gate petitions remain unsettled",
			WhyNowSummary = "notice and street talk are both pushing pressure toward the gate",
			PublicMomentumSummary = "county gate momentum is tightening",
			PhaseLabel = "needs response",
			PhaseSummary = "stabilize the gate before the queue worsens",
			HandlingSummary = "registrar is still triaging petitions",
			GuidanceSummary = "stabilize the county gate surface first"
		};
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		Assert.That(presentationShellViewModel.GreatHall.GovernanceSummary, Does.Contain("county gate momentum is tightening"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements, Has.Count.EqualTo(1));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].GovernanceSummary, Does.Contain("county gate momentum is tightening"));
	}

	[Test]
	public void Compose_PrefersHallDocketLeadItemOverNotificationForGreatHallLeadNotice()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.Notifications = new global::_003C_003Ez__ReadOnlySingleElementList<NarrativeNotificationSnapshot>(new NarrativeNotificationSnapshot
		{
			Id = new NotificationId(11),
			CreatedAt = new GameDate(1200, 2),
			Tier = NotificationTier.Urgent,
			Surface = NarrativeSurface.GreatHall,
			Title = "raw notification title",
			Summary = "raw notification summary",
			WhatNext = "raw notification guidance"
		});
		presentationReadModelBundle.HallDocket = new HallDocketStackSnapshot
		{
			LeadItem = new HallDocketItemSnapshot
			{
				LaneKey = "Governance",
				SettlementId = new SettlementId(1),
				SettlementName = "Lanxi",
				NodeLabel = "county-gate",
				Headline = "hall docket lead title",
				WhyNowSummary = "hall docket why-now",
				PhaseLabel = "needs response",
				PhaseSummary = "hall docket phase",
				HandlingSummary = "hall docket handling",
				GuidanceSummary = "hall docket guidance"
			},
			SecondaryItems = Array.Empty<HallDocketItemSnapshot>()
		};
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		Assert.That(presentationShellViewModel.GreatHall.LeadNoticeTitle, Is.EqualTo("hall docket lead title"));
		Assert.That(presentationShellViewModel.GreatHall.LeadNoticeGuidance, Does.Contain("hall docket guidance"));
		Assert.That(presentationShellViewModel.GreatHall.LeadNoticeGuidance, Does.Contain("hall docket phase"));
		Assert.That(presentationShellViewModel.NotificationCenter.Items, Has.Count.EqualTo(1));
		Assert.That(presentationShellViewModel.NotificationCenter.Items[0].Title, Is.EqualTo("raw notification title"));
	}

	[Test]
	public void Compose_ProjectsSecondaryHallDocketItemsIntoGreatHall()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.HallDocket = new HallDocketStackSnapshot
		{
			LeadItem = new HallDocketItemSnapshot
			{
				LaneKey = "Family",
				SettlementId = new SettlementId(1),
				Headline = "lead family matter",
				GuidanceSummary = "lead family guidance"
			},
			SecondaryItems = new global::_003C_003Ez__ReadOnlyArray<HallDocketItemSnapshot>(new HallDocketItemSnapshot[2]
			{
				new HallDocketItemSnapshot
				{
					LaneKey = "Governance",
					SettlementId = new SettlementId(1),
					SettlementName = "Lanxi",
					NodeLabel = "county-gate",
					TargetLabel = "county-gate",
					Headline = "governance secondary matter",
					PhaseLabel = "needs response",
					WhyNowSummary = "county paperwork is tightening",
					PhaseSummary = "stabilize the gate before backlog rises"
				},
				new HallDocketItemSnapshot
				{
					LaneKey = "Warfare",
					SettlementId = new SettlementId(1),
					SettlementName = "Lanxi",
					NodeLabel = "river-route",
					TargetLabel = "river-route",
					Headline = "warfare secondary matter",
					PhaseLabel = "aftercare",
					GuidanceSummary = "escort strain still needs follow-through",
					HandlingSummary = "recent mobilization has not fully settled"
				}
			})
		};
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		Assert.That(presentationShellViewModel.GreatHall.SecondaryDockets, Has.Count.EqualTo(2));
		Assert.That(presentationShellViewModel.GreatHall.SecondaryDockets[0].Headline, Is.EqualTo("governance secondary matter"));
		Assert.That(presentationShellViewModel.GreatHall.SecondaryDockets[0].Summary, Does.Contain("county paperwork is tightening"));
		Assert.That(presentationShellViewModel.GreatHall.SecondaryDockets[1].Headline, Is.EqualTo("warfare secondary matter"));
		Assert.That(presentationShellViewModel.GreatHall.SecondaryDockets[1].Summary, Does.Contain("escort strain still needs follow-through"));
	}

	[Test]
	public void Compose_ProjectsHallDocketAgendaIntoDeskSettlementNodes()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.HallDocket = new HallDocketStackSnapshot
		{
			LeadItem = new HallDocketItemSnapshot
			{
				LaneKey = "Family",
				SettlementId = new SettlementId(1),
				SettlementName = "Lanxi",
				NodeLabel = "ancestral-hall",
				Headline = "lead family matter",
				PhaseLabel = "family review"
			},
			SecondaryItems = new global::_003C_003Ez__ReadOnlyArray<HallDocketItemSnapshot>(new HallDocketItemSnapshot[2]
			{
				new HallDocketItemSnapshot
				{
					LaneKey = "Governance",
					SettlementId = new SettlementId(1),
					SettlementName = "Lanxi",
					NodeLabel = "county-gate",
					Headline = "governance secondary matter",
					PhaseLabel = "needs response",
					WhyNowSummary = "county paperwork is still tightening"
				},
				new HallDocketItemSnapshot
				{
					LaneKey = "Warfare",
					SettlementId = new SettlementId(2),
					SettlementName = "Elsewhere",
					NodeLabel = "river-route",
					Headline = "off-node warfare matter",
					PhaseLabel = "aftercare"
				}
			})
		};
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		SettlementNodeViewModel settlementNodeViewModel = presentationShellViewModel.DeskSandbox.Settlements.Single();
		Assert.That(settlementNodeViewModel.HallAgendaSummary, Does.Contain("lead family matter"));
		Assert.That(settlementNodeViewModel.HallAgendaSummary, Does.Contain("governance secondary matter"));
		Assert.That(settlementNodeViewModel.HallAgendaSummary, Does.Not.Contain("off-node warfare matter"));
		Assert.That(settlementNodeViewModel.HallAgendaCount, Is.EqualTo(2));
		Assert.That(settlementNodeViewModel.HallAgendaItems, Has.Count.EqualTo(2));
		Assert.That(settlementNodeViewModel.HallAgendaLaneKeys, Is.EqualTo(new string[2] { "Family", "Governance" }));
		Assert.That(settlementNodeViewModel.HasLeadHallAgendaItem, Is.True);
		Assert.That(settlementNodeViewModel.LeadHallAgendaLaneKey, Is.EqualTo("Family"));
		Assert.That(settlementNodeViewModel.HallAgendaItems[0].Headline, Is.EqualTo("lead family matter"));
		Assert.That(settlementNodeViewModel.HallAgendaItems[1].LaneKey, Is.EqualTo("Governance"));
		Assert.That(settlementNodeViewModel.HallAgendaItems[1].Summary, Does.Contain("county paperwork is still tightening"));
	}

	[Test]
	public void Compose_LeavesDeskLeadHallAgendaMarkerEmptyWhenLeadItemTargetsElsewhere()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.HallDocket = new HallDocketStackSnapshot
		{
			LeadItem = new HallDocketItemSnapshot
			{
				LaneKey = "Warfare",
				SettlementId = new SettlementId(2),
				SettlementName = "Elsewhere",
				NodeLabel = "river-route",
				Headline = "off-node lead matter",
				PhaseLabel = "aftercare"
			},
			SecondaryItems = new global::_003C_003Ez__ReadOnlySingleElementList<HallDocketItemSnapshot>(new HallDocketItemSnapshot
			{
				LaneKey = "Governance",
				SettlementId = new SettlementId(1),
				SettlementName = "Lanxi",
				NodeLabel = "county-gate",
				Headline = "local secondary matter",
				PhaseLabel = "needs response"
			})
		};
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		SettlementNodeViewModel settlementNodeViewModel = presentationShellViewModel.DeskSandbox.Settlements.Single();
		Assert.That(settlementNodeViewModel.HallAgendaCount, Is.EqualTo(1));
		Assert.That(settlementNodeViewModel.HasLeadHallAgendaItem, Is.False);
		Assert.That(settlementNodeViewModel.LeadHallAgendaLaneKey, Is.Empty);
		Assert.That(settlementNodeViewModel.HallAgendaLaneKeys, Is.EqualTo(new string[1] { "Governance" }));
	}

	[Test]
	public void Compose_AlignsFamilyLifecycleLeadNoticeAndNotificationCenterGuidance()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
		{
			Affordances = new global::_003C_003Ez__ReadOnlySingleElementList<PlayerCommandAffordanceSnapshot>(new PlayerCommandAffordanceSnapshot
			{
				ModuleKey = "FamilyCore",
				SurfaceKey = "Family",
				SettlementId = new SettlementId(1),
				ClanId = new ClanId(1),
				CommandName = "SupportNewbornCare",
				Label = "拨粮护婴",
				Summary = "先把产后调护、乳哺与襁褓衣食稳下来。",
				AvailabilitySummary = "门内现有襁褓1口，宗房余力尚可拨用。",
				TargetLabel = "清河张氏",
				IsEnabled = true
			}),
			Receipts = Array.Empty<PlayerCommandReceiptSnapshot>()
		};
		presentationReadModelBundle.Notifications = new global::_003C_003Ez__ReadOnlySingleElementList<NarrativeNotificationSnapshot>(new NarrativeNotificationSnapshot
		{
			Id = new NotificationId(9),
			CreatedAt = new GameDate(1200, 2),
			Tier = NotificationTier.Consequential,
			Surface = NarrativeSurface.AncestralHall,
			Title = "门内添丁",
			Summary = "张氏门内新添一口。",
			WhyItHappened = "门内添丁之后，乳哺与抚养之费一并压上肩头。",
			WhatNext = "先把产妇与襁褓照看住，再看口粮、乳哺与看护之费该由谁分担。",
			SourceModuleKey = "FamilyCore",
			Traces = new global::_003C_003Ez__ReadOnlySingleElementList<NotificationTraceSnapshot>(new NotificationTraceSnapshot
			{
				SourceModuleKey = "FamilyCore",
				EventType = "BirthRegistered",
				EventSummary = "张氏门内添丁。",
				DiffDescription = "张氏添丁之后，香火暂缓焦心。",
				EntityKey = "1"
			})
		});
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		Assert.That(presentationShellViewModel.GreatHall.LeadNoticeTitle, Is.EqualTo("门内添丁"));
		Assert.That(presentationShellViewModel.GreatHall.LeadNoticeGuidance, Does.Contain("襁褓"));
		Assert.That(presentationShellViewModel.GreatHall.LeadNoticeGuidance, Does.Contain("眼下最宜先命清河张氏拨粮护婴"));
		Assert.That(presentationShellViewModel.NotificationCenter.Items, Has.Count.EqualTo(1));
		Assert.That(presentationShellViewModel.NotificationCenter.Items[0].WhatNext, Does.Contain("襁褓"));
		Assert.That(presentationShellViewModel.NotificationCenter.Items[0].WhatNext, Does.Contain("眼下最宜先命清河张氏拨粮护婴"));
	}

	[Test]
	public void Compose_ProjectsRegionalWarfareAndAftermathIntoHallDeskAndCampaignBoard_V4()
	{
		PresentationReadModelBundle presentationReadModelBundle = CreateBundle();
		presentationReadModelBundle.Settlements[0].Security = 54;
		presentationReadModelBundle.Settlements[0].Prosperity = 62;
		presentationReadModelBundle.PopulationSettlements[0].CommonerDistress = 45;
		presentationReadModelBundle.PopulationSettlements[0].MigrationPressure = 36;
		presentationReadModelBundle.OfficeJurisdictions[0].PetitionBacklog = 9;
		presentationReadModelBundle.TradeRoutes = new global::_003C_003Ez__ReadOnlySingleElementList<TradeRouteSnapshot>(new TradeRouteSnapshot
		{
			RouteId = 1,
			ClanId = new ClanId(1),
			SettlementId = new SettlementId(1),
			RouteName = "兰溪渡口粮线",
			Capacity = 41,
			Risk = 62,
			IsActive = true,
			LastMargin = 14,
			BlockedShipmentCount = 2,
			SeizureRisk = 38,
			RouteConstraintLabel = "渡口脚力吃紧",
			LastRouteTrace = "渡口脚力被军需牵住。"
		});
		presentationReadModelBundle.Campaigns = new global::_003C_003Ez__ReadOnlySingleElementList<CampaignFrontSnapshot>(new CampaignFrontSnapshot
		{
			CampaignId = new CampaignId(1),
			AnchorSettlementId = new SettlementId(1),
			AnchorSettlementName = "兰溪",
			CampaignName = "兰溪河渡行营",
			IsActive = true,
			ObjectiveSummary = "先稳河渡，再护粮线。",
			MobilizedForceCount = 180,
			FrontPressure = 62,
			FrontLabel = "前锋相持",
			SupplyState = 52,
			SupplyStateLabel = "粮道尚稳",
			MoraleState = 60,
			MoraleStateLabel = "军心可用",
			CommandFitLabel = "以护运守务",
			CommanderSummary = "张元暂摄转运与护路节次。",
			ActiveDirectiveCode = "ProtectSupplyLine",
			ActiveDirectiveLabel = "护粮稳线",
			ActiveDirectiveSummary = "先保护口粮线，再缓前锋耗损。",
			LastDirectiveTrace = "主簿差弁已把渡口签牌并入军机案头。",
			MobilizationWindowLabel = "Narrow",
			SupplyLineSummary = "渡口粮线仍能转运，只是脚力吃紧。",
			OfficeCoordinationTrace = "县署差弁与护运脚夫并线而行。",
			SourceTrace = "河渡一线还在收束。",
			LastAftermathSummary = "渡口村落仍待安辑与抚恤。",
			Routes = new global::_003C_003Ez__ReadOnlyArray<CampaignRouteSnapshot>(new CampaignRouteSnapshot[2]
			{
				new CampaignRouteSnapshot
				{
					RouteLabel = "渡口粮线",
					RouteRole = "supply",
					Pressure = 80,
					Security = 40,
					FlowStateLabel = "受阻",
					Summary = "渡口粮签与脚夫簿正被军需挤压。"
				},
				new CampaignRouteSnapshot
				{
					RouteLabel = "县门驿路",
					RouteRole = "dispatch",
					Pressure = 48,
					Security = 55,
					FlowStateLabel = "可通",
					Summary = "县门驿路仍可递送军报。"
				}
			})
		});
		PresentationShellViewModel presentationShellViewModel = FirstPassPresentationShell.Compose(presentationReadModelBundle);
		Assert.That(presentationShellViewModel.GreatHall.WarfareSummary, Does.Contain("水驿商埠"));
		Assert.That(presentationShellViewModel.GreatHall.AftermathDocketSummary, Does.Contain("记功簿"));
		Assert.That(presentationShellViewModel.GreatHall.AftermathDocketSummary, Does.Contain("清路札"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].CampaignSummary, Does.Contain("兰溪河渡行营"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].CampaignSummary, Does.Contain("水驿商埠"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].AftermathSummary, Does.Contain("战后案牍"));
		Assert.That(presentationShellViewModel.DeskSandbox.Settlements[0].AftermathSummary, Does.Contain("抚恤簿"));
		Assert.That(presentationShellViewModel.Warfare.CampaignBoards, Has.Count.EqualTo(1));
		Assert.That(presentationShellViewModel.Warfare.CampaignBoards[0].RegionalProfileLabel, Is.EqualTo("水驿商埠"));
		Assert.That(presentationShellViewModel.Warfare.CampaignBoards[0].AftermathDocketSummary, Does.Contain("军机案今并载"));
		Assert.That(presentationShellViewModel.Warfare.CampaignBoards[0].AftermathDocketSummary, Does.Contain("清路札"));
	}

	private static PresentationReadModelBundle CreateBundle()
	{
		PresentationReadModelBundle presentationReadModelBundle = new PresentationReadModelBundle();
		presentationReadModelBundle.CurrentDate = new GameDate(1200, 2);
		presentationReadModelBundle.ReplayHash = "cadence-hash";
		presentationReadModelBundle.Clans = new global::_003C_003Ez__ReadOnlySingleElementList<ClanSnapshot>(new ClanSnapshot
		{
			Id = new ClanId(1),
			ClanName = "清河张氏",
			HomeSettlementId = new SettlementId(1),
			Prestige = 62,
			SupportReserve = 55,
			HeirPersonId = new PersonId(1),
			BranchTension = 61,
			InheritancePressure = 44,
			SeparationPressure = 38,
			MediationMomentum = 36,
			MarriageAlliancePressure = 42,
			MarriageAllianceValue = 26,
			HeirSecurity = 31,
			ReproductivePressure = 48,
			MourningLoad = 0,
			LastLifecycleCommandLabel = "议亲定婚",
			LastLifecycleOutcome = "婚事已议，门内暂可缓一缓承祧后议。",
			LastConflictCommandLabel = "请族老调停",
			LastConflictOutcome = "族老已入祠堂议事。"
		});
		presentationReadModelBundle.ClanNarratives = new global::_003C_003Ez__ReadOnlySingleElementList<ClanNarrativeSnapshot>(new ClanNarrativeSnapshot
		{
			ClanId = new ClanId(1),
			PublicNarrative = "祠堂里外都在议张氏分房。",
			GrudgePressure = 34,
			ShamePressure = 28,
			FavorBalance = 12
		});
		presentationReadModelBundle.Settlements = new global::_003C_003Ez__ReadOnlySingleElementList<SettlementSnapshot>(new SettlementSnapshot
		{
			Id = new SettlementId(1),
			Name = "兰溪",
			Tier = SettlementTier.CountySeat,
			Security = 63,
			Prosperity = 66
		});
		presentationReadModelBundle.PopulationSettlements = new global::_003C_003Ez__ReadOnlySingleElementList<PopulationSettlementSnapshot>(new PopulationSettlementSnapshot
		{
			SettlementId = new SettlementId(1),
			CommonerDistress = 35,
			LaborSupply = 112,
			MigrationPressure = 18,
			MilitiaPotential = 70
		});
		presentationReadModelBundle.ClanTrades = new global::_003C_003Ez__ReadOnlySingleElementList<ClanTradeSnapshot>(new ClanTradeSnapshot
		{
			ClanId = new ClanId(1),
			PrimarySettlementId = new SettlementId(1),
			CashReserve = 92,
			GrainReserve = 71,
			Debt = 9,
			CommerceReputation = 29,
			ShopCount = 1,
			LastOutcome = "Profit",
			LastExplanation = "春社集期，河埠买卖略有盈余。"
		});
		presentationReadModelBundle.PublicLifeSettlements = new global::_003C_003Ez__ReadOnlySingleElementList<SettlementPublicLifeSnapshot>(new SettlementPublicLifeSnapshot
		{
			SettlementId = new SettlementId(1),
			SettlementName = "兰溪",
			SettlementTier = SettlementTier.CountySeat,
			NodeLabel = "县门榜下",
			DominantVenueLabel = "街口茶肆",
			DominantVenueCode = "teahouse-inn",
			MonthlyCadenceCode = "spring-fair",
			MonthlyCadenceLabel = "春社集日",
			CrowdMixLabel = "多见客商、小贩与脚夫",
			StreetTalkHeat = 63,
			MarketBuzz = 58,
			NoticeVisibility = 55,
			RoadReportLag = 29,
			PrefectureDispatchPressure = 47,
			PublicLegitimacy = 52,
			DocumentaryWeight = 59,
			VerificationCost = 22,
			MarketRumorFlow = 57,
			CourierRisk = 24,
			OfficialNoticeLine = "榜下只说县门已先晓谕轻重。",
			StreetTalkLine = "街口都说茶肆听来的话更近实情。",
			RoadReportLine = "路上传来的脚信尚能和门前榜示相互印证。",
			PrefectureDispatchLine = "州牒催意已到，县里还想缓出几分。",
			ContentionSummary = "榜文、街谈与脚信彼此牵扯，众人仍在观望。",
			CadenceSummary = "值春社集日，街口茶肆多见客商、小贩与脚夫。",
			PublicSummary = "街谈渐热，镇市喧起。",
			RouteReportSummary = "路报尚能递到县门。",
			LastPublicTrace = "县门榜下街谈渐热。"
		});
		presentationReadModelBundle.OfficeCareers = new global::_003C_003Ez__ReadOnlySingleElementList<OfficeCareerSnapshot>(new OfficeCareerSnapshot
		{
			PersonId = new PersonId(1),
			ClanId = new ClanId(1),
			SettlementId = new SettlementId(1),
			DisplayName = "张元",
			HasAppointment = true,
			OfficeTitle = "主簿",
			AuthorityTier = 2,
			JurisdictionLeverage = 58,
			PetitionPressure = 24,
			PetitionBacklog = 7,
			CurrentAdministrativeTask = "勾理词状",
			AdministrativeTaskTier = "district",
			PetitionOutcomeCategory = "Triaged",
			LastPetitionOutcome = "分轻重，先收县门词状。"
		});
		presentationReadModelBundle.OfficeJurisdictions = new global::_003C_003Ez__ReadOnlySingleElementList<JurisdictionAuthoritySnapshot>(new JurisdictionAuthoritySnapshot
		{
			SettlementId = new SettlementId(1),
			LeadOfficialPersonId = new PersonId(1),
			LeadOfficialName = "张元",
			LeadOfficeTitle = "主簿",
			AuthorityTier = 2,
			JurisdictionLeverage = 58,
			PetitionPressure = 24,
			PetitionBacklog = 7,
			CurrentAdministrativeTask = "勾理词状",
			AdministrativeTaskTier = "district",
			PetitionOutcomeCategory = "Triaged",
			LastPetitionOutcome = "分轻重，先收县门词状。"
		});
		presentationReadModelBundle.Notifications = new global::_003C_003Ez__ReadOnlySingleElementList<NarrativeNotificationSnapshot>(new NarrativeNotificationSnapshot
		{
			Id = new NotificationId(1),
			CreatedAt = new GameDate(1200, 2),
			Tier = NotificationTier.Consequential,
			Surface = NarrativeSurface.GreatHall,
			Title = "县门榜示",
			Summary = "春社集日前后，县门议论渐起。"
		});
		presentationReadModelBundle.PlayerCommands = new PlayerCommandSurfaceSnapshot
		{
			Affordances = new global::_003C_003Ez__ReadOnlyArray<PlayerCommandAffordanceSnapshot>(new PlayerCommandAffordanceSnapshot[2]
			{
				new PlayerCommandAffordanceSnapshot
				{
					ModuleKey = "FamilyCore",
					SurfaceKey = "Family",
					SettlementId = new SettlementId(1),
					ClanId = new ClanId(1),
					CommandName = "DesignateHeirPolicy",
					Label = "议定承祧",
					Summary = "先把承祧次序与谱内名分写稳。",
					AvailabilitySummary = "承祧稳度31，名分若虚仍易再起后议。",
					TargetLabel = "清河张氏",
					IsEnabled = true
				},
				new PlayerCommandAffordanceSnapshot
				{
					ModuleKey = "FamilyCore",
					SurfaceKey = "Family",
					SettlementId = new SettlementId(1),
					ClanId = new ClanId(1),
					CommandName = "InviteClanEldersMediation",
					Label = "请族老调停",
					Summary = "请族老入祠堂缓争，先压分房之议。",
					AvailabilitySummary = "族老与房亲都在县城，可即刻议事。",
					TargetLabel = "清河张氏",
					IsEnabled = true
				}
			}),
			Receipts = new global::_003C_003Ez__ReadOnlySingleElementList<PlayerCommandReceiptSnapshot>(new PlayerCommandReceiptSnapshot
			{
				ModuleKey = "FamilyCore",
				SurfaceKey = "Family",
				SettlementId = new SettlementId(1),
				ClanId = new ClanId(1),
				CommandName = "InviteClanEldersMediation",
				Label = "请族老调停",
				Summary = "族老已在祠堂聚首。",
				OutcomeSummary = "族老先收两房说辞，缓下当面争口。",
				TargetLabel = "清河张氏"
			})
		};
		return presentationReadModelBundle;
	}
}
