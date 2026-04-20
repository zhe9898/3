using System;

namespace Zongzu.Presentation.Unity;

internal static class OfficeShellTextAdapter
{
	internal static string RenderAdministrativeTask(string taskName)
	{
		return taskName switch
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
			_ => taskName
		};
	}

	internal static string RenderOfficeTitle(string officeTitle)
	{
		return officeTitle switch
		{
			"Assistant Magistrate" => "县丞",
			"Registrar" => "主簿",
			"District Clerk" => "书吏",
			"Unappointed" => "未授官",
			_ => officeTitle
		};
	}

	internal static string RenderAdministrativeTaskTier(string taskTier)
	{
		if (string.Equals(taskTier, "candidate", StringComparison.Ordinal))
		{
			return "候次";
		}

		return taskTier switch
		{
			"crisis" => "急务",
			"district" => "州县",
			"registry" => "簿册",
			"clerical" => "案牍",
			"inactive" => "候补",
			_ => taskTier
		};
	}

	internal static string RenderPetitionOutcomeCategory(string category)
	{
		return category switch
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
			_ => category
		};
	}

	internal static string RenderPromotionPressureLabel(string label)
	{
		return label switch
		{
			"promotion-ready" => "可迁",
			"rising" => "渐起",
			"steady" => "持平",
			"thin" => "微弱",
			_ => label
		};
	}

	internal static string RenderDemotionPressureLabel(string label)
	{
		return label switch
		{
			"critical" => "危急",
			"strained" => "吃紧",
			"watched" => "在察",
			"stable" => "平稳",
			_ => label
		};
	}

	internal static string BuildOfficePressureSummary(
		bool hasAppointment,
		string lastOutcome,
		int petitionBacklog,
		string promotionLabel,
		string demotionLabel)
	{
		string promotionSummary = RenderPromotionPressureLabel(promotionLabel);
		string demotionSummary = RenderDemotionPressureLabel(demotionLabel);
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

		return lastOutcome switch
		{
			"Promoted" => $"官途近有迁转之势，升势{promotionSummary}，黜压{demotionSummary}。",
			"Demoted" => $"官途方遭降黜，升势{promotionSummary}，黜压{demotionSummary}。",
			"Lost" => $"官途已失，积案{petitionBacklog}，黜压{demotionSummary}。",
			_ => $"官途暂守，升势{promotionSummary}，黜压{demotionSummary}。"
		};
	}

	internal static string RenderPetitionOutcome(string outcome)
	{
		if (string.IsNullOrWhiteSpace(outcome))
		{
			return string.Empty;
		}

		int separatorIndex = outcome.IndexOf(':');
		if (separatorIndex > 0 && separatorIndex < outcome.Length - 1)
		{
			string category = outcome.Substring(0, separatorIndex).Trim();
			string summary = outcome[(separatorIndex + 1)..].Trim();
			return RenderPetitionOutcomeCategory(category) + "：" + summary;
		}

		return outcome switch
		{
			"Petitions cleared while copying tax rolls and sealed filings." => "已清：税册与封牍俱已誊定。",
			"Censured and triaged." => "劾责中：词牍分轻重收理。",
			_ => outcome
		};
	}
}
