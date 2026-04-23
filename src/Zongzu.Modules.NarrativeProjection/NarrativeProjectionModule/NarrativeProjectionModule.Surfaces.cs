using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.NarrativeProjection;

public sealed partial class NarrativeProjectionModule : ModuleRunner<NarrativeProjectionState>
{
    private static NotificationTier DetermineTier(string eventType)

    {

        return eventType switch

        {

            TradeAndIndustryEventNames.TradeDebtDefaulted => NotificationTier.Urgent,

            PopulationEventNames.LivelihoodCollapsed => NotificationTier.Urgent,

            EducationAndExamsEventNames.ExamPassed => NotificationTier.Consequential,

            EducationAndExamsEventNames.ExamFailed => NotificationTier.Consequential,

            EducationAndExamsEventNames.StudyAbandoned => NotificationTier.Consequential,

            TradeAndIndustryEventNames.TradeProspered => NotificationTier.Consequential,

            TradeAndIndustryEventNames.TradeLossOccurred => NotificationTier.Consequential,

            TradeAndIndustryEventNames.RouteBusinessBlocked => NotificationTier.Consequential,

            OrderAndBanditryEventNames.DisorderSpike => NotificationTier.Urgent,

            OrderAndBanditryEventNames.BanditThreatRaised => NotificationTier.Consequential,

            OrderAndBanditryEventNames.OutlawGroupFormed => NotificationTier.Consequential,

            OrderAndBanditryEventNames.RouteUnsafeDueToBanditry => NotificationTier.Consequential,

            ConflictAndForceEventNames.ConflictResolved => NotificationTier.Consequential,

            ConflictAndForceEventNames.CommanderWounded => NotificationTier.Urgent,

            ConflictAndForceEventNames.ForceReadinessChanged => NotificationTier.Consequential,

            ConflictAndForceEventNames.MilitiaMobilized => NotificationTier.Consequential,

            PublicLifeAndRumorEventNames.StreetTalkSurged => NotificationTier.Consequential,

            PublicLifeAndRumorEventNames.CountyGateCrowded => NotificationTier.Consequential,

            PublicLifeAndRumorEventNames.MarketBuzzRaised => NotificationTier.Background,

            PublicLifeAndRumorEventNames.RoadReportDelayed => NotificationTier.Urgent,

            PublicLifeAndRumorEventNames.PrefectureDispatchPressed => NotificationTier.Urgent,

            WarfareCampaignEventNames.CampaignMobilized => NotificationTier.Consequential,

            WarfareCampaignEventNames.CampaignPressureRaised => NotificationTier.Urgent,

            WarfareCampaignEventNames.CampaignSupplyStrained => NotificationTier.Urgent,

            WarfareCampaignEventNames.CampaignAftermathRegistered => NotificationTier.Consequential,

            PopulationEventNames.MigrationStarted => NotificationTier.Consequential,

            SocialMemoryAndRelationsEventNames.GrudgeEscalated => NotificationTier.Consequential,

            FamilyCoreEventNames.ClanPrestigeAdjusted => NotificationTier.Consequential,

            FamilyCoreEventNames.MarriageAllianceArranged => NotificationTier.Consequential,

            FamilyCoreEventNames.BirthRegistered => NotificationTier.Consequential,

            DeathCauseEventNames.DeathByIllness => NotificationTier.Urgent,

            DeathCauseEventNames.DeathByViolence => NotificationTier.Urgent,

            FamilyCoreEventNames.ClanMemberDied => NotificationTier.Urgent,

            FamilyCoreEventNames.HeirAppointed => NotificationTier.Consequential,

            FamilyCoreEventNames.HeirSuccessionOccurred => NotificationTier.Consequential,

            FamilyCoreEventNames.HeirSecurityWeakened => NotificationTier.Urgent,

            FamilyCoreEventNames.LineageDisputeHardened => NotificationTier.Consequential,

            FamilyCoreEventNames.LineageMediationOpened => NotificationTier.Consequential,

            FamilyCoreEventNames.BranchSeparationApproved => NotificationTier.Consequential,

            _ => NotificationTier.Background,

        };

    }


    private static NarrativeSurface DetermineSurface(string moduleKey)

    {

        return moduleKey switch

        {

            KnownModuleKeys.FamilyCore => NarrativeSurface.AncestralHall,

            KnownModuleKeys.SocialMemoryAndRelations => NarrativeSurface.AncestralHall,

            KnownModuleKeys.WorldSettlements => NarrativeSurface.DeskSandbox,

            KnownModuleKeys.PopulationAndHouseholds => NarrativeSurface.DeskSandbox,

            KnownModuleKeys.EducationAndExams => NarrativeSurface.DeskSandbox,

            KnownModuleKeys.TradeAndIndustry => NarrativeSurface.DeskSandbox,

            KnownModuleKeys.PublicLifeAndRumor => NarrativeSurface.DeskSandbox,

            KnownModuleKeys.OrderAndBanditry => NarrativeSurface.DeskSandbox,

            KnownModuleKeys.ConflictAndForce => NarrativeSurface.ConflictVignette,

            KnownModuleKeys.WarfareCampaign => NarrativeSurface.DeskSandbox,

            _ => NarrativeSurface.GreatHall,

        };

    }


    private static string BuildTitle(string moduleKey, string eventType, IReadOnlyList<NarrativeTraceState> traces)

    {

        return eventType switch

        {

            EducationAndExamsEventNames.ExamPassed => "场屋得捷",

            EducationAndExamsEventNames.ExamFailed => "场屋失利",

            EducationAndExamsEventNames.StudyAbandoned => "停馆罢读",

            TradeAndIndustryEventNames.TradeProspered => "市利有进",

            TradeAndIndustryEventNames.TradeLossOccurred => "商账受亏",

            TradeAndIndustryEventNames.TradeDebtDefaulted => "债主压门",

            TradeAndIndustryEventNames.RouteBusinessBlocked => "行路受阻",

            OrderAndBanditryEventNames.DisorderSpike => "失序骤起",

            OrderAndBanditryEventNames.BanditThreatRaised => "盗警渐起",

            OrderAndBanditryEventNames.OutlawGroupFormed => "啸聚成股",

            OrderAndBanditryEventNames.RouteUnsafeDueToBanditry => "商路不靖",

            ConflictAndForceEventNames.ConflictResolved => "乡斗暂息",

            ConflictAndForceEventNames.CommanderWounded => "领队负创",

            ConflictAndForceEventNames.ForceReadinessChanged => "营伍更张",

            ConflictAndForceEventNames.MilitiaMobilized => "乡勇应募",

            PublicLifeAndRumorEventNames.StreetTalkSurged => "街谈渐热",

            PublicLifeAndRumorEventNames.CountyGateCrowded => "县门壅挤",

            PublicLifeAndRumorEventNames.MarketBuzzRaised => "镇市喧起",

            PublicLifeAndRumorEventNames.RoadReportDelayed => "路报迟滞",

            PublicLifeAndRumorEventNames.PrefectureDispatchPressed => "州牒催迫",

            WarfareCampaignEventNames.CampaignMobilized => "军檄立案",

            WarfareCampaignEventNames.CampaignPressureRaised => "前线告急",

            WarfareCampaignEventNames.CampaignSupplyStrained => "粮道告急",

            WarfareCampaignEventNames.CampaignAftermathRegistered => BuildCampaignAftermathTitle(traces),

            "MigrationStarted" => "流徙启行",

            "LivelihoodCollapsed" => "生计顿敝",

            "GrudgeEscalated" => "旧怨益深",

            FamilyCoreEventNames.ClanPrestigeAdjusted => "门望有变",

            FamilyCoreEventNames.LineageDisputeHardened => "祠堂争议渐炽",

            FamilyCoreEventNames.LineageMediationOpened => "族老调停开议",

            FamilyCoreEventNames.BranchSeparationApproved => "分房议定",

            FamilyCoreEventNames.MarriageAllianceArranged => "门内议亲",

            FamilyCoreEventNames.BirthRegistered => "门内添丁",

            DeathCauseEventNames.DeathByIllness => "门内伤幼",

            DeathCauseEventNames.DeathByViolence => "暴亡入案",

            FamilyCoreEventNames.ClanMemberDied => "门内举哀",

            FamilyCoreEventNames.HeirAppointed => "承祧立名",

            FamilyCoreEventNames.HeirSuccessionOccurred => "承祧转房",

            FamilyCoreEventNames.HeirSecurityWeakened => "承祧未稳",

            _ => $"{GetModuleLabel(moduleKey)}告示",

        };

    }


    private static string BuildCampaignAftermathTitle(IReadOnlyList<NarrativeTraceState> traces)

    {

        bool hasMerit = traces.Any(static trace =>

            string.Equals(trace.SourceModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal)

            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.SocialMemoryAndRelations, StringComparison.Ordinal));

        bool hasBlame = traces.Any(static trace =>

            string.Equals(trace.SourceModuleKey, KnownModuleKeys.OfficeAndCareer, StringComparison.Ordinal)

            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.ConflictAndForce, StringComparison.Ordinal));

        bool hasRelief = traces.Any(static trace =>

            string.Equals(trace.SourceModuleKey, KnownModuleKeys.PopulationAndHouseholds, StringComparison.Ordinal)

            || string.Equals(trace.SourceModuleKey, KnownModuleKeys.WorldSettlements, StringComparison.Ordinal));


        if (hasMerit && hasBlame && hasRelief)

        {

            return "战后赏罚与抚恤";

        }


        if (hasBlame && hasRelief)

        {

            return "战后追责与抚恤";

        }


        if (hasMerit && hasRelief)

        {

            return "战后记功与抚恤";

        }


        return "战后覆核入案";

    }


    private static string BuildWhyText(IReadOnlyList<NarrativeTraceState> traces)

    {

        string[] reasons = traces

            .Select(static trace => trace.DiffDescription)

            .Where(static text => !string.IsNullOrWhiteSpace(text))

            .Distinct(StringComparer.Ordinal)

            .Take(4)

            .ToArray();


        if (reasons.Length == 0)

        {

            return "案下暂无可征旁证。";

        }


        return string.Join(" ", reasons);

    }


}
