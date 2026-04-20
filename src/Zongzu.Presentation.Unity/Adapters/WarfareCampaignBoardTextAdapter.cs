using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class WarfareCampaignBoardTextAdapter
{
	internal readonly record struct RegionalBoardProfile(string Label, string BackdropSummary);

	internal static string BuildCampaignConditionLabelChinese(CampaignFrontSnapshot campaign)
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
			return campaign.FrontPressure >= 60 ? "粮筹压阵" : "粮驿先行";
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

	internal static string BuildCampaignBoardSurfaceRegionalChinese(
		CampaignFrontSnapshot campaign,
		RegionalBoardProfile regionalProfile)
	{
		string backdropSummary = regionalProfile.BackdropSummary;
		CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
		if (!campaign.IsActive)
		{
			return backdropSummary + " 营旗收束，后营册页、伤损簿与善后批答占住案心。";
		}
		if (string.Equals(campaign.ActiveDirectiveCode, "ProtectSupplyLine", StringComparison.Ordinal))
		{
			return leadRoute == null
				? (backdropSummary + " 粮签、渡口木筹与护运行签压在案心，前锋旗退到边角。")
				: (backdropSummary + " 案心多是" + leadRoute.RouteLabel + "筹签，前锋旗让位于护运与驿递木筹。");
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

	internal static string BuildCampaignBoardAtmosphereRegionalChinese(
		CampaignFrontSnapshot campaign,
		RegionalBoardProfile regionalProfile)
	{
		if (!campaign.IsActive)
		{
			return "此案属" + regionalProfile.Label + "之局，已转为战后覆核：" + campaign.LastAftermathSummary;
		}

		return $"此案属{regionalProfile.Label}之局，呈{BuildCampaignConditionLabelChinese(campaign)}之势：{campaign.FrontLabel}，{campaign.SupplyStateLabel}，{campaign.MoraleStateLabel}；军令为{campaign.ActiveDirectiveLabel}，{DescribeCampaignRouteMixRegionalChinese(campaign)}。";
	}

	internal static string BuildCampaignBoardMarkerRegionalChinese(
		CampaignFrontSnapshot campaign,
		RegionalBoardProfile regionalProfile)
	{
		CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
		if (leadRoute == null)
		{
			return regionalProfile.Label + "案主要凭前锋旗、粮簿与军报批注辨势。";
		}

		CampaignRouteSnapshot? secondRoute = campaign.Routes
			.OrderByDescending(route => route.Pressure)
			.ThenByDescending(route => route.Security)
			.ThenBy(route => route.RouteLabel, StringComparer.Ordinal)
			.Skip(1)
			.FirstOrDefault();
		if (secondRoute != null)
		{
			return $"{regionalProfile.Label}案头以{leadRoute.RouteLabel}为主线，并由{secondRoute.RouteLabel}牵住侧边。";
		}

		return $"{regionalProfile.Label}案头主线落在{leadRoute.RouteLabel}，其势为{leadRoute.FlowStateLabel}。";
	}

	internal static string DescribeCampaignRouteMixRegionalChinese(CampaignFrontSnapshot campaign)
	{
		CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
		if (leadRoute == null)
		{
			return "案上仍以军报和前锋旗为主";
		}

		CampaignRouteSnapshot? secondRoute = campaign.Routes
			.OrderByDescending(route => route.Pressure)
			.ThenByDescending(route => route.Security)
			.ThenBy(route => route.RouteLabel, StringComparer.Ordinal)
			.Skip(1)
			.FirstOrDefault();
		if (secondRoute == null)
		{
			return "主看" + leadRoute.RouteLabel + "，其势" + leadRoute.FlowStateLabel;
		}

		return $"主看{leadRoute.RouteLabel}，并以{secondRoute.RouteLabel}牵住侧边";
	}

	internal static CampaignRouteSnapshot? SelectLeadCampaignRoute(CampaignFrontSnapshot campaign)
	{
		return campaign.Routes
			.OrderByDescending(route => route.Pressure)
			.ThenByDescending(route => route.Security)
			.ThenBy(route => route.RouteLabel, StringComparer.Ordinal)
			.FirstOrDefault();
	}

	internal static RegionalBoardProfile BuildCampaignRegionalProfile(
		CampaignFrontSnapshot campaign,
		SettlementSnapshot? settlement = null,
		IReadOnlyList<ClanTradeRouteSnapshot>? clanTradeRoutes = null)
	{
		List<string> signals =
		[
			campaign.AnchorSettlementName,
			campaign.CampaignName
		];
		signals.AddRange(campaign.Routes.Select(route => route.RouteLabel));
		signals.AddRange(campaign.Routes.Select(route => route.Summary));
		signals.AddRange((clanTradeRoutes ?? Array.Empty<ClanTradeRouteSnapshot>()).Select(route => route.RouteName));

		bool water = ContainsRegionalSignal(signals, "river", "canal", "ferry", "wharf", "water", "河", "江", "渡", "港", "浦", "漕");
		bool mountain = ContainsRegionalSignal(signals, "hill", "mountain", "ridge", "pass", "山", "岭", "关", "隘", "谷");
		int prosperity = settlement?.Prosperity ?? 0;
		int security = settlement?.Security ?? 0;

		if (water)
		{
			return prosperity >= 60
				? new RegionalBoardProfile("水驿商埠", "案旁多铺水线、渡口木牌与舟楫筹。")
				: new RegionalBoardProfile("江渡县口", "案边常见渡津签、河埠木牌与泊船筹。");
		}
		if (mountain)
		{
			return security >= 50
				? new RegionalBoardProfile("山道关路", "案面多画山道折线、岭口木塞与关津旗。")
				: new RegionalBoardProfile("岭道荒隘", "案边竖着山口木牌、险路折签与巡哨火签。");
		}
		if (prosperity >= 65)
		{
			return new RegionalBoardProfile("市镇腹地", "案旁多见仓牌、街市路签与税簿角注。");
		}
		if (security <= 45)
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

			foreach (string marker in markers)
			{
				if (signal.Contains(marker, StringComparison.OrdinalIgnoreCase)
					|| signal.Contains(marker, StringComparison.Ordinal))
				{
					return true;
				}
			}
		}

		return false;
	}
}
