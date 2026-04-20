using System;
using Zongzu.Contracts;

namespace Zongzu.Presentation.Unity;

internal static class WarfareCampaignTextAdapter
{
	internal static string RenderCampaignSurfaceText(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}

		string normalized = RenderEscortRouteSummary(text);
		normalized = normalized.Replace(
			"Registrar couriers are tying docket traffic into the campaign board.",
			"主簿差役正把文移驿线并入军机案头。",
			StringComparison.Ordinal);

		return normalized
			.Replace("Registrar ", "主簿 ", StringComparison.Ordinal)
			.Replace("district层级", "县署层级", StringComparison.Ordinal)
			.Replace(" district ", " 县署 ", StringComparison.OrdinalIgnoreCase)
			.Replace("campaign board", "军机案头", StringComparison.OrdinalIgnoreCase)
			.Replace("docket traffic", "文移驿线", StringComparison.OrdinalIgnoreCase)
			.Replace("backlog", "积案", StringComparison.OrdinalIgnoreCase);
	}

	internal static string BuildCampaignFrontSummaryText(CampaignFrontSnapshot campaign)
	{
		return $"{campaign.FrontLabel}：前线{campaign.FrontPressure}，粮道{campaign.SupplyState}（{campaign.SupplyStateLabel}），军心{campaign.MoraleState}（{campaign.MoraleStateLabel}）。";
	}

	internal static string BuildCampaignMobilizationSummaryText(CampaignFrontSnapshot campaign)
	{
		return $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowRegionalChinese(campaign.MobilizationWindowLabel)}。";
	}

	internal static string BuildMobilizationSignalForceSummaryText(CampaignMobilizationSignalSnapshot signal)
	{
		return $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。";
	}

	internal static string BuildMobilizationSignalOfficeSummaryText(CampaignMobilizationSignalSnapshot signal)
	{
		return signal.OfficeAuthorityTier > 0
			? $"官绅层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。"
			: "暂无官绅文移接应。";
	}

	internal static string DescribeMobilizationWindowRegionalChinese(string mobilizationWindowLabel)
	{
		return mobilizationWindowLabel switch
		{
			"Open" => "可开",
			"Narrow" => "可守",
			"Preparing" => "待整",
			_ => "已闭"
		};
	}

	private static string RenderEscortRouteSummary(string text)
	{
		int markerIndex = text.IndexOf(" escorts are keeping stores moving for ", StringComparison.OrdinalIgnoreCase);
		if (markerIndex <= 0)
		{
			return text;
		}

		string countText = text.Substring(0, markerIndex).Trim();
		if (!int.TryParse(countText, out int escortCount))
		{
			return text;
		}

		int labelStart = markerIndex + " escorts are keeping stores moving for ".Length;
		int sentenceEnd = text.IndexOf('.', labelStart);
		if (sentenceEnd < 0)
		{
			return text;
		}

		string routeLabel = text.Substring(labelStart, sentenceEnd - labelStart).Trim();
		string tail = text.Substring(sentenceEnd + 1).Trim();
		string summary = $"{routeLabel}粮秣正由护运{escortCount}人维持行转。";
		return string.IsNullOrEmpty(tail) ? summary : (summary + " " + tail);
	}
}
