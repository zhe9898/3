using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
{
    private static int ComputeCampaignBacklogIncrease(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int increase = bundle.CampaignMobilized ? 2 : 0;

        increase += bundle.CampaignPressureRaised ? 3 : 0;

        increase += bundle.CampaignSupplyStrained ? 6 : 0;

        increase += bundle.CampaignAftermathRegistered ? 4 : 0;

        increase += Math.Max(0, campaign.FrontPressure - 55) / 16;

        return Math.Max(1, increase);

    }


    private static int ComputeCampaignPetitionPressureIncrease(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int increase = bundle.CampaignPressureRaised ? 3 : 0;

        increase += bundle.CampaignSupplyStrained ? 4 : 0;

        increase += bundle.CampaignAftermathRegistered ? 2 : 0;

        increase += Math.Max(0, 50 - campaign.SupplyState) / 12;

        increase += Math.Max(0, 45 - campaign.MoraleState) / 15;

        return Math.Max(1, increase);

    }


    private static string ResolveCampaignAdministrativeTask(WarfareCampaignEventBundle bundle)

    {

        if (bundle.CampaignSupplyStrained)

        {

            return "急牍覆核";

        }


        if (bundle.CampaignAftermathRegistered)

        {

            return "勘理词状";

        }


        if (bundle.CampaignPressureRaised)

        {

            return "巡丁清点";

        }


        return "勾检户籍";

    }


    private static int ComputePetitionHeat(

        OfficeCareerState career,

        SettlementDisorderSnapshot? disorder,

        SettlementBlackRoutePressureSnapshot? blackRoutePressure,

        LocalForcePoolSnapshot? force)

    {

        int heat = career.PetitionPressure;

        heat = Math.Max(heat, disorder?.DisorderPressure ?? 0);

        heat = Math.Max(heat, disorder?.SuppressionDemand ?? 0);

        heat = Math.Max(heat, disorder?.RoutePressure ?? 0);

        heat = Math.Max(heat, blackRoutePressure?.ImplementationDrag ?? 0);

        heat = Math.Max(heat, blackRoutePressure?.RetaliationRisk ?? 0);


        if (force is not null)

        {

            heat = Math.Max(heat, force.IsResponseActivated ? 58 : force.HasActiveConflict ? 42 : 0);

            heat = Math.Max(heat, force.OrderSupportLevel * 12);

        }


        return Math.Clamp(heat, 0, 100);

    }


    private static bool IsCalmOfficeSurface(

        SettlementDisorderSnapshot? disorder,

        SettlementBlackRoutePressureSnapshot? blackRoutePressure,

        LocalForcePoolSnapshot? force)

    {

        return (disorder?.DisorderPressure ?? 0) < 25

            && (disorder?.RoutePressure ?? 0) < 25

            && (disorder?.SuppressionDemand ?? 0) < 25

            && (blackRoutePressure?.ImplementationDrag ?? 0) < 35

            && (blackRoutePressure?.RetaliationRisk ?? 0) < 35

            && force?.HasActiveConflict != true

            && force?.IsResponseActivated != true;

    }


    private static bool HasAdministrativeCarryover(SettlementDisorderSnapshot? disorder)

    {

        return disorder is not null

            && disorder.InterventionCarryoverMonths > 0

            && !string.IsNullOrWhiteSpace(disorder.LastInterventionCommandCode);

    }


    private static OrderAdministrativeAftermath ResolveOrderAdministrativeAftermath(

        SettlementDisorderSnapshot? disorder,

        SettlementBlackRoutePressureSnapshot? blackRoutePressure)

    {

        if (disorder is null

            || disorder.InterventionCarryoverMonths <= 0

            || string.IsNullOrWhiteSpace(disorder.LastInterventionCommandCode))

        {

            return default;

        }


        int retaliationRisk = blackRoutePressure?.RetaliationRisk ?? 0;

        int routeShielding = blackRoutePressure?.RouteShielding ?? 0;

        int implementationDrag = blackRoutePressure?.ImplementationDrag ?? 0;

        int paperCompliance = blackRoutePressure?.PaperCompliance ?? 0;

        int blackRoutePressureValue = blackRoutePressure?.BlackRoutePressure ?? 0;


        return disorder.LastInterventionCommandCode switch

        {

            PlayerCommandNames.EscortRoadReport => new OrderAdministrativeAftermath(

                disorder.LastInterventionCommandLabel,

                "差丁清点",

                retaliationRisk >= 55 ? "Delayed" : "Triaged",

                retaliationRisk >= 55

                    ? $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在差丁清点，津口路票与沿路词牒一并压来。"

                    : $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在差丁清点，护送人手与驿路递报先分头归整。",

                4 + (implementationDrag / 22) + (disorder.SuppressionDemand / 30),

                Math.Max(0, 2 + (retaliationRisk / 28) + Math.Max(0, disorder.SuppressionDemand - 40) / 22 - (routeShielding >= 45 ? 1 : 0)),

                Math.Max(0, 1 + (retaliationRisk / 30) - (routeShielding >= 55 ? 1 : 0)),

                1 + (implementationDrag / 26),

                Math.Clamp((routeShielding / 28) + (paperCompliance / 40) - (retaliationRisk / 35), -1, 2),

                Math.Clamp((retaliationRisk / 26) - (routeShielding / 42), 0, 3),

                routeShielding >= 60 && retaliationRisk < 45 ? 1 : 0),

            PlayerCommandNames.FundLocalWatch => new OrderAdministrativeAftermath(

                disorder.LastInterventionCommandLabel,

                "差丁清点",

                retaliationRisk >= 60 ? "Delayed" : "Triaged",

                retaliationRisk >= 60

                    ? $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在差丁清点，巡丁名粮与沿路投诉都要重核。"

                    : $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在差丁清点，巡丁名额与护路人手还须逐项点验。",

                5 + (implementationDrag / 20) + (disorder.SuppressionDemand / 26),

                Math.Max(0, 3 + (retaliationRisk / 24) + Math.Max(0, disorder.SuppressionDemand - 35) / 20 - (routeShielding >= 45 ? 1 : 0)),

                Math.Max(0, 2 + (retaliationRisk / 28) - (routeShielding >= 60 ? 1 : 0)),

                2 + (implementationDrag / 22),

                Math.Clamp((routeShielding / 30) + (paperCompliance / 45) - (retaliationRisk / 30), -1, 2),

                Math.Clamp(1 + (retaliationRisk / 24) - (routeShielding / 50), 0, 4),

                routeShielding >= 55 && retaliationRisk < 40 ? 1 : 0),

            PlayerCommandNames.SuppressBanditry => new OrderAdministrativeAftermath(

                disorder.LastInterventionCommandLabel,

                retaliationRisk >= 55 ? "勘解乡怨词牒" : "勘理词状",

                retaliationRisk >= 60 ? "Surged" : "Delayed",

                retaliationRisk >= 60

                    ? $"上月{disorder.LastInterventionCommandLabel}之后，今月转入勘解乡怨词牒，沿路告扰与牵连状牒一并压来。"

                    : $"上月{disorder.LastInterventionCommandLabel}之后，今月转入勘理词状，收审与追比还在路上回荡。",

                6 + (implementationDrag / 18) + (disorder.SuppressionDemand / 24),

                Math.Max(0, 4 + (retaliationRisk / 18) + Math.Max(0, disorder.SuppressionDemand - 30) / 18 - (routeShielding >= 55 ? 1 : 0)),

                Math.Max(0, 3 + (retaliationRisk / 20) - (routeShielding >= 60 ? 1 : 0)),

                1 + (implementationDrag / 24),

                Math.Clamp((routeShielding / 36) - (retaliationRisk / 26), -3, 1),

                Math.Clamp(2 + (retaliationRisk / 16) + (blackRoutePressureValue / 40) - (routeShielding / 55), 0, 6),

                routeShielding >= 60 && retaliationRisk < 35 ? 1 : 0),

            PlayerCommandNames.NegotiateWithOutlaws => new OrderAdministrativeAftermath(

                disorder.LastInterventionCommandLabel,

                retaliationRisk >= 45 ? "勘解乡怨词牒" : "张榜晓谕",

                retaliationRisk >= 45 ? "Delayed" : "Triaged",

                retaliationRisk >= 45

                    ? $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在勘解乡怨词牒，议路之后的猜疑与投诉还未散。"

                    : $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在张榜晓谕，议路约束与安抚话头都要落纸。",

                4 + (implementationDrag / 22) + (disorder.SuppressionDemand / 32),

                Math.Max(0, 2 + (retaliationRisk / 24) + Math.Max(0, disorder.SuppressionDemand - 45) / 26 - (routeShielding >= 50 ? 1 : 0)),

                Math.Max(0, 1 + (retaliationRisk / 28) - (routeShielding >= 60 ? 1 : 0)),

                2 + (implementationDrag / 24),

                Math.Clamp((paperCompliance / 45) + (routeShielding / 40) - (retaliationRisk / 32), -1, 2),

                Math.Clamp(1 + (retaliationRisk / 22) - (routeShielding / 55), 0, 4),

                routeShielding >= 55 && retaliationRisk < 35 ? 1 : 0),

            PlayerCommandNames.TolerateDisorder => new OrderAdministrativeAftermath(

                disorder.LastInterventionCommandLabel,

                "遣吏催报",

                blackRoutePressureValue >= 60 || retaliationRisk >= 45 ? "Surged" : "Delayed",

                blackRoutePressureValue >= 60 || retaliationRisk >= 45

                    ? $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在遣吏催报，失窃与行旅催告先压到了县署。"

                    : $"上月{disorder.LastInterventionCommandLabel}之后，今月仍在遣吏催报，路上失序的回报一件件补来。",

                3 + (implementationDrag / 24) + (disorder.SuppressionDemand / 30),

                5 + (retaliationRisk / 20) + (blackRoutePressureValue / 20),

                Math.Max(0, 5 + (retaliationRisk / 22) + (blackRoutePressureValue / 24) - (routeShielding >= 60 ? 1 : 0)),

                1 + (implementationDrag / 24),

                -2 - Math.Max(0, retaliationRisk - routeShielding) / 28,

                Math.Clamp(2 + (retaliationRisk / 20) + (blackRoutePressureValue / 26), 0, 6),

                -1),

            _ => default,

        };

    }


    private static void ApplyOrderAdministrativeAftermath(OfficeCareerState career, OrderAdministrativeAftermath aftermath)

    {

        career.CurrentAdministrativeTask = SelectAdministrativeTask(

            career.CurrentAdministrativeTask,

            aftermath.TaskName,

            Math.Max(career.AuthorityTier, 1));

        career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + aftermath.TaskLoadDelta, 0, 100);

        career.PetitionBacklog = Math.Clamp(career.PetitionBacklog + aftermath.PetitionBacklogDelta, 0, 100);

        career.PetitionPressure = Math.Clamp(career.PetitionPressure + aftermath.PetitionPressureDelta, 0, 100);

        career.ClerkDependence = Math.Clamp(career.ClerkDependence + aftermath.ClerkDependenceDelta, 0, 100);

        career.JurisdictionLeverage = Math.Clamp(career.JurisdictionLeverage + aftermath.JurisdictionLeverageDelta, 0, 100);

        career.DemotionPressure = Math.Clamp(career.DemotionPressure + aftermath.DemotionPressureDelta, 0, 100);

        career.PromotionMomentum = Math.Clamp(career.PromotionMomentum + aftermath.PromotionMomentumDelta, 0, 100);

        career.LastPetitionOutcome = OfficeAndCareerDescriptors.FormatPetitionOutcome(

            aftermath.PetitionOutcomeCategory,

            aftermath.PetitionOutcomeDetail);

    }


    private static string SelectAdministrativeTask(string currentTask, string incomingTask, int authorityTier)

    {

        if (string.IsNullOrWhiteSpace(incomingTask))

        {

            return currentTask;

        }


        if (string.IsNullOrWhiteSpace(currentTask))

        {

            return incomingTask;

        }


        string currentTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(currentTask, authorityTier);

        string incomingTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(incomingTask, authorityTier);

        return RankAdministrativeTaskTier(incomingTier) > RankAdministrativeTaskTier(currentTier)

            ? incomingTask

            : currentTask;

    }


    private static int RankAdministrativeTaskTier(string taskTier)

    {

        return taskTier switch

        {

            "crisis" => 4,

            "district" => 3,

            "registry" => 2,

            "clerical" => 1,

            _ => 0,

        };

    }


    private static string ResolveOfficeTitle(int authorityTier)

    {

        return authorityTier switch

        {

            >= 3 => "县丞",

            2 => "主簿",

            _ => "书吏",

        };

    }


}
