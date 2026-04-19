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
            "急牒核办" => "crisis",
            "急牒覆核" => "crisis",
            "勘解乡怨词牒" => "crisis",
            "差丁清点" => "district",
            "勘理词状" => "district",
            "张榜晓谕" => "district",
            "遣吏催报" => "district",
            "勾检户籍" => "registry",
            "誊录词牒" => "clerical",
            "誊黄封牒" => "clerical",
            "随案听差" => "candidate",
            "投牒候差" => "candidate",
            "守选候阙" => "inactive",
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

        int separator = outcomeText.IndexOf('：');
        if (separator > 0)
        {
            return ParsePetitionOutcomeCategory(outcomeText[..separator].Trim());
        }

        separator = outcomeText.IndexOf(':');
        if (separator > 0)
        {
            return ParsePetitionOutcomeCategory(outcomeText[..separator].Trim());
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

    public static string DescribeAppointmentPressure(int appointmentPressure)
    {
        return appointmentPressure switch
        {
            >= 48 => "posting-near",
            >= 32 => "queue-forming",
            >= 18 => "entry-open",
            _ => "thin",
        };
    }

    public static string DescribeClerkDependence(int clerkDependence)
    {
        return clerkDependence switch
        {
            >= 60 => "captured",
            >= 42 => "dependent",
            >= 24 => "shared",
            _ => "light",
        };
    }

    public static string BuildAuthorityTrajectorySummary(OfficeCareerState career)
    {
        string promotion = DescribePromotionPressure(career.PromotionMomentum);
        string demotion = DescribeDemotionPressure(career.DemotionPressure);

        if (!career.HasAppointment)
        {
            if (!career.IsEligible)
            {
                return "场屋与声望未足，官途尚未开启。";
            }

            string appointment = DescribeAppointmentPressure(career.AppointmentPressure);
            string clerk = DescribeClerkDependence(career.ClerkDependence);
            return career.LastOutcome switch
            {
                "听差" => $"场屋已捷，正随案听差；荐引势{appointment}，吏案依赖{clerk}。",
                _ => $"已入守选候阙之途；荐引势{appointment}，升势{promotion}，黜压{demotion}。",
            };
        }

        return career.LastOutcome switch
        {
            "Promoted" => $"官阶新进，升势{promotion}，黜压{demotion}。",
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
        return outcome switch
        {
            "Promoted" => $"{displayName}今以{officeTitle}供职，所主{taskTier}差遣，升势{promotion}，黜压{demotion}，积案{petitionBacklog}。",
            "Demoted" => $"{displayName}今仍在{officeTitle}任上，所主{taskTier}差遣，升势{promotion}，黜压{demotion}，积案{petitionBacklog}。",
            _ => $"{displayName}今以{officeTitle}供职，所主{taskTier}差遣，升势{promotion}，黜压{demotion}，积案{petitionBacklog}。",
        };
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
            "案前骇涌" or "Surged" => "Surged",
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
            "Surged" => "案前骇涌",
            "Stalled" => "壅滞",
            "Censured" => "劾责中",
            _ => category,
        };
    }
}
