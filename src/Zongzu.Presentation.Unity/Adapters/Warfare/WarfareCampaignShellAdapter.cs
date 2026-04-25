using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

internal static class WarfareCampaignShellAdapter
{
	internal static WarfareSurfaceViewModel BuildWarfareSurface(
		PresentationReadModelBundle bundle,
		IReadOnlyList<NarrativeNotificationSnapshot> notifications)
	{
		WarfareProjectionContext context = WarfareProjectionContext.Create(bundle);
		if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
		{
			return new WarfareSurfaceViewModel
			{
				Summary = "暂无军务沙盘投影。"
			};
		}

		return new WarfareSurfaceViewModel
		{
			Summary = BuildWarfareSurfaceSummary(context),
			CommandAffordances = CommandShellAdapter.BuildAffordances(context.OrderedAffordances),
			RecentReceipts = context.OrderedReceipts
				.Select(receipt => new CommandReceiptViewModel
				{
					TargetLabel = receipt.TargetLabel,
					CommandName = receipt.CommandName,
					Label = receipt.Label,
					Summary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(receipt.Summary),
					OutcomeSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(receipt.OutcomeSummary),
					LeverageSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(receipt.LeverageSummary),
					CostSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(receipt.CostSummary),
					ReadbackSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(receipt.ReadbackSummary)
				})
				.ToArray(),
			CampaignBoards = context.OrderedCampaigns
				.Select(campaign =>
				{
					context.SettlementsById.TryGetValue(campaign.AnchorSettlementId.Value, out SettlementSnapshot? settlement);
					ClanTradeRouteSnapshot[] clanTradeRoutes = context.ClanTradeRoutesBySettlement[campaign.AnchorSettlementId.Value]
						.OrderBy(route => route.RouteName, StringComparer.Ordinal)
						.ToArray();
					WarfareCampaignBoardTextAdapter.RegionalBoardProfile regionalProfile = WarfareCampaignBoardTextAdapter.BuildCampaignRegionalProfile(campaign, settlement, clanTradeRoutes);

					return new CampaignBoardViewModel
					{
						CampaignName = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.CampaignName),
						SettlementLabel = campaign.AnchorSettlementName,
						StatusLabel = campaign.IsActive ? "行营在案" : "战后覆核",
						RegionalProfileLabel = regionalProfile.Label,
						RegionalBackdropSummary = regionalProfile.BackdropSummary,
						EnvironmentLabel = WarfareCampaignBoardTextAdapter.BuildCampaignConditionLabelChinese(campaign),
						BoardSurfaceLabel = WarfareCampaignBoardTextAdapter.BuildCampaignBoardSurfaceRegionalChinese(campaign, regionalProfile),
						BoardAtmosphereSummary = WarfareCampaignBoardTextAdapter.BuildCampaignBoardAtmosphereRegionalChinese(campaign, regionalProfile),
						MarkerSummary = WarfareCampaignBoardTextAdapter.BuildCampaignBoardMarkerRegionalChinese(campaign, regionalProfile),
						FrontLabel = campaign.FrontLabel,
						SupplyStateLabel = campaign.SupplyStateLabel,
						MoraleStateLabel = campaign.MoraleStateLabel,
						CommandFitLabel = campaign.CommandFitLabel,
						DirectiveLabel = campaign.ActiveDirectiveLabel,
						DirectiveSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.ActiveDirectiveSummary),
						DirectiveTrace = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.LastDirectiveTrace),
						ObjectiveSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.ObjectiveSummary),
						FrontSummary = WarfareCampaignTextAdapter.BuildCampaignFrontSummaryText(campaign),
						MobilizationSummary = WarfareCampaignTextAdapter.BuildCampaignMobilizationSummaryText(campaign),
						SupplyLineSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.SupplyLineSummary),
						CoordinationSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.OfficeCoordinationTrace),
						CommanderSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.CommanderSummary),
						AftermathSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(campaign.LastAftermathSummary),
						AftermathDocketSummary = WarfareAftermathShellAdapter.BuildCampaignAftermathDocketSummary(campaign, settlement, notifications),
						Routes = campaign.Routes.Select(route => new CampaignRouteViewModel
						{
							RouteLabel = route.RouteLabel,
							RouteRole = route.RouteRole,
							FlowStateLabel = route.FlowStateLabel,
							Summary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(route.Summary)
						}).ToArray()
					};
				}).ToArray(),
			MobilizationSignals = context.OrderedMobilizationSignals
				.Select(signal => new CampaignMobilizationSignalViewModel
				{
					SettlementLabel = signal.SettlementName,
					WindowLabel = WarfareCampaignTextAdapter.DescribeMobilizationWindowRegionalChinese(signal.MobilizationWindowLabel),
					ForceSummary = WarfareCampaignTextAdapter.BuildMobilizationSignalForceSummaryText(signal),
					CommandFitLabel = signal.CommandFitLabel,
					DirectiveLabel = signal.ActiveDirectiveLabel,
					DirectiveSummary = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(signal.ActiveDirectiveSummary),
					OfficeSummary = WarfareCampaignTextAdapter.BuildMobilizationSignalOfficeSummaryText(signal),
					SourceTrace = WarfareCampaignTextAdapter.RenderCampaignSurfaceText(signal.SourceTrace)
				}).ToArray()
		};
	}

	internal static string BuildGreatHallWarfareSummary(PresentationReadModelBundle bundle)
	{
		WarfareProjectionContext context = WarfareProjectionContext.Create(bundle);
		if (context.LeadCampaign == null)
		{
			return "暂无军务沙盘投影。";
		}

		CampaignFrontSnapshot leadCampaign = context.LeadCampaign;
		WarfareCampaignBoardTextAdapter.RegionalBoardProfile regionalBoardProfile = WarfareCampaignBoardTextAdapter.BuildCampaignRegionalProfile(
			leadCampaign,
			context.LeadCampaignSettlement,
			context.LeadCampaignClanTradeRoutes);

		return $"现有 {context.ActiveCampaignCount} 处在案行营；{leadCampaign.AnchorSettlementName}当前{leadCampaign.FrontLabel}、{leadCampaign.SupplyStateLabel}，属{regionalBoardProfile.Label}之局，案头呈{WarfareCampaignBoardTextAdapter.BuildCampaignConditionLabelChinese(leadCampaign)}。";
	}

	internal static string BuildSettlementCampaignSummary(
		CampaignFrontSnapshot? campaign,
		CampaignMobilizationSignalSnapshot? signal,
		SettlementSnapshot settlement,
		IReadOnlyList<ClanTradeRouteSnapshot> clanTradeRoutes)
	{
		if (campaign != null)
		{
			WarfareCampaignBoardTextAdapter.RegionalBoardProfile regionalBoardProfile = WarfareCampaignBoardTextAdapter.BuildCampaignRegionalProfile(campaign, settlement, clanTradeRoutes);
			CampaignRouteSnapshot? leadRoute = WarfareCampaignBoardTextAdapter.SelectLeadCampaignRoute(campaign);
			string routeSummary = leadRoute == null
				? "暂无路况细目"
				: leadRoute.RouteLabel + leadRoute.FlowStateLabel;
			return $"{campaign.CampaignName}：{regionalBoardProfile.Label}，{WarfareCampaignBoardTextAdapter.BuildCampaignConditionLabelChinese(campaign)}，{routeSummary}。";
		}

		if (signal != null)
		{
		return $"{WarfareCampaignTextAdapter.DescribeMobilizationWindowRegionalChinese(signal.MobilizationWindowLabel)}动员窗，应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}，军令 {signal.ActiveDirectiveLabel}。";
		}

		return "暂无军务沙盘投影。";
	}

	private static string BuildWarfareSurfaceSummary(WarfareProjectionContext context)
	{
		if (context.LeadCampaign == null)
		{
			return "暂无军务沙盘投影。";
		}

		CampaignFrontSnapshot leadCampaign = context.LeadCampaign;
		WarfareCampaignBoardTextAdapter.RegionalBoardProfile regionalBoardProfile = WarfareCampaignBoardTextAdapter.BuildCampaignRegionalProfile(
			leadCampaign,
			context.LeadCampaignSettlement,
			context.LeadCampaignClanTradeRoutes);

		return $"现有 {context.ActiveCampaignCount} 处在案行营，峰值前线压力 {context.PeakFrontPressure}；{leadCampaign.AnchorSettlementName}正以 {leadCampaign.CommandFitLabel} 维持 {leadCampaign.FrontLabel}，属{regionalBoardProfile.Label}之局。";
	}
}
