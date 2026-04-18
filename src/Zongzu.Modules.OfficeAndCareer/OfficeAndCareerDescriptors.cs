using System;

namespace Zongzu.Modules.OfficeAndCareer;

internal static class OfficeAndCareerDescriptors
{
    public static string DetermineAdministrativeTaskTier(string taskName, int authorityTier)
    {
        if (string.IsNullOrWhiteSpace(taskName))
        {
            return authorityTier <= 0 ? "inactive" : "clerical";
        }

        return taskName switch
        {
            "急牍覆核" => "crisis",
            "勘解乡怨词牍" => "crisis",
            "巡丁清点" => "district",
            "勘理词状" => "district",
            "勾检户籍" => "registry",
            "誊录词牍" => "clerical",
            "誊黄封牍" => "clerical",
            "候补听选" => "inactive",
            _ => authorityTier >= 2 ? "district" : "clerical",
        };
    }

    public static string FormatPetitionOutcome(string category, string detail)
    {
        return $"{RenderPetitionOutcomeCategory(category)}：{detail}";
    }

    public static string DeterminePetitionOutcomeCategory(string outcomeText)
    {
        if (string.IsNullOrWhiteSpace(outcomeText))
        {
            return "Unknown";
        }

        if (outcomeText.Contains('：'))
        {
            string category = outcomeText[..outcomeText.IndexOf('：')].Trim();
            return ParsePetitionOutcomeCategory(category);
        }

        int separator = outcomeText.IndexOf(':');
        if (separator > 0)
        {
            string category = outcomeText[..separator].Trim();
            return ParsePetitionOutcomeCategory(category);
        }

        return "Unknown";
    }

    public static string DescribePromotionPressure(int promotionMomentum)
    {
        return promotionMomentum switch
        {
            >= 55 => "promotion-ready",
            >= 35 => "rising",
            >= 18 => "steady",
            _ => "thin",
        };
    }

    public static string DescribeDemotionPressure(int demotionPressure)
    {
        return demotionPressure switch
        {
            >= 60 => "critical",
            >= 40 => "strained",
            >= 20 => "watched",
            _ => "stable",
        };
    }

    public static string BuildAuthorityTrajectorySummary(OfficeCareerState career)
    {
        string promotion = DescribePromotionPressure(career.PromotionMomentum);
        string demotion = DescribeDemotionPressure(career.DemotionPressure);

        if (!career.HasAppointment)
        {
            return career.IsEligible
                ? $"已入选途，升势{promotion}，黜压{demotion}，仍在候缺。"
                : "未入官途，暂无仕路起伏。";
        }

        return career.LastOutcome switch
        {
            "Promoted" => $"官阶新迁，升势{promotion}，黜压{demotion}。",
            "Demoted" => $"官阶受抑，升势{promotion}，黜压{demotion}。",
            "Lost" => $"官身已失，黜压{demotion}，积案{career.PetitionBacklog}。",
            _ => $"官途暂守，升势{promotion}，黜压{demotion}。",
        };
    }

    public static string BuildAuthorityShiftTrace(
        string displayName,
        string outcome,
        string officeTitle,
        string taskTier,
        int promotionMomentum,
        int demotionPressure,
        int petitionBacklog)
    {
        string promotion = DescribePromotionPressure(promotionMomentum);
        string demotion = DescribeDemotionPressure(demotionPressure);
        return $"{displayName}今以{officeTitle}供职，事类{taskTier}，升势{promotion}，黜压{demotion}，积案{petitionBacklog}。";
    }

    private static string ParsePetitionOutcomeCategory(string category)
    {
        return category switch
        {
            "待勘" or "Queued" => "Queued",
            "未开案" or "Unavailable" => "Unavailable",
            "稽延" or "Delayed" => "Delayed",
            "分轻重" or "Triaged" => "Triaged",
            "已清" or "Cleared" => "Cleared",
            "准行" or "Granted" => "Granted",
            "案牍骤涌" or "Surged" => "Surged",
            "壅滞" or "Stalled" => "Stalled",
            "劾责中" or "Censured" => "Censured",
            _ => "Unknown",
        };
    }

    private static string RenderPetitionOutcomeCategory(string category)
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
            _ => category,
        };
    }
}
