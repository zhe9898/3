using System;
using Zongzu.Contracts;

namespace Zongzu.Modules.WarfareCampaign;

public static class WarfareCampaignDescriptors
{
    public const string NoOfficeCoordinationTrace = "当前军务信号暂无官署文移接应。";

    public static string DetermineFrontLabel(int frontPressure)
    {
        return frontPressure switch
        {
            >= 75 => "前线鏖持",
            >= 55 => "前线吃紧",
            >= 35 => "前线持守",
            _ => "前线暂静",
        };
    }

    public static string DetermineSupplyStateLabel(int supplyState)
    {
        return supplyState switch
        {
            >= 75 => "粮道畅行",
            >= 55 => "粮道可守",
            >= 35 => "粮道吃紧",
            _ => "粮道断绝",
        };
    }

    public static string DetermineMoraleStateLabel(int moraleState)
    {
        return moraleState switch
        {
            >= 75 => "军心振作",
            >= 55 => "军心尚稳",
            >= 35 => "军心浮动",
            _ => "军心惶惧",
        };
    }

    public static string DetermineCommandFitLabel(
        int commandCapacity,
        int responseActivationLevel,
        int officeAuthorityTier,
        int petitionBacklog)
    {
        int fitScore = commandCapacity
            + (responseActivationLevel / 2)
            + (officeAuthorityTier * 5)
            - (petitionBacklog / 3);

        return fitScore switch
        {
            >= 70 => "号令严整",
            >= 50 => "号令相接",
            >= 30 => "号令勉强",
            _ => "号令离散",
        };
    }

    public static string DetermineRouteFlowStateLabel(int routePressure, int routeSecurity)
    {
        int margin = routeSecurity - routePressure;
        return margin switch
        {
            >= 25 => "粮运畅通",
            >= 5 => "粮运可守",
            >= -10 => "粮运脆弱",
            _ => "粮运断裂",
        };
    }

    public static string DetermineDirectiveCode(
        bool isActive,
        int frontPressure,
        int supplyState,
        int petitionBacklog)
    {
        if (!isActive)
        {
            return WarfareCampaignCommandNames.WithdrawToBarracks;
        }

        if (supplyState <= 42)
        {
            return WarfareCampaignCommandNames.ProtectSupplyLine;
        }

        if (frontPressure >= 68 || petitionBacklog >= 18)
        {
            return WarfareCampaignCommandNames.CommitMobilization;
        }

        return WarfareCampaignCommandNames.DraftCampaignPlan;
    }

    public static string DetermineDirectiveLabel(string directiveCode)
    {
        return directiveCode switch
        {
            WarfareCampaignCommandNames.DraftCampaignPlan => "筹议方略",
            WarfareCampaignCommandNames.CommitMobilization => "发檄点兵",
            WarfareCampaignCommandNames.ProtectSupplyLine => "催督粮道",
            WarfareCampaignCommandNames.WithdrawToBarracks => "班师归营",
            _ => "案上观势",
        };
    }

    public static string BuildDirectiveSummary(string directiveCode, string settlementName)
    {
        return directiveCode switch
        {
            WarfareCampaignCommandNames.DraftCampaignPlan => $"先在{settlementName}案上筹定关津、驿报与前后队次，不轻启身进之势。",
            WarfareCampaignCommandNames.CommitMobilization => $"向{settlementName}周边亲兵、乡勇与护运队发檄点集，先稳阵脚，再图外压。",
            WarfareCampaignCommandNames.ProtectSupplyLine => $"先护{settlementName}粮道、渡口与文移驿线，不与前锋争一时之气。",
            WarfareCampaignCommandNames.WithdrawToBarracks => $"暂收{settlementName}行伍归营，整饷糗粮与伤员，留余地再议。",
            _ => $"先观{settlementName}军务态势，再定檄令。",
        };
    }

    public static string BuildDirectiveChoiceReadbackSummary(string directiveCode, string settlementName)
    {
        string choiceLabel = directiveCode switch
        {
            WarfareCampaignCommandNames.DraftCampaignPlan => "案头筹议选择",
            WarfareCampaignCommandNames.CommitMobilization => "点兵加压选择",
            WarfareCampaignCommandNames.ProtectSupplyLine => "粮道护持选择",
            WarfareCampaignCommandNames.WithdrawToBarracks => "归营止损选择",
            _ => "案头观势选择",
        };

        return $"军令选择读回：{choiceLabel}已落在{settlementName}的WarfareCampaign案头；WarfareCampaign拥有军令，Force只读力役/战备，Office只读官面协调，军务选择不是县门文移代打，也不是普通家户硬扛。";
    }

    public static string BuildCommanderSummary(
        string settlementName,
        LocalForcePoolSnapshot localForce,
        CampaignMobilizationSignalState signal,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is not null && jurisdiction.AuthorityTier > 0)
        {
            return
                $"{jurisdiction.LeadOfficeTitle}{jurisdiction.LeadOfficialName}正以辖区杠杆{signal.AdministrativeLeverage}催督文移，" +
                $"使{settlementName}守丁{localForce.GuardCount}、乡勇{localForce.MilitiaCount}与护运{localForce.EscortCount}维持{signal.CommandFitLabel}。";
        }

        return
            $"{settlementName}本地统兵只得勉力维持{signal.CommandFitLabel}，" +
            $"现有整备{localForce.Readiness}、统摄{localForce.CommandCapacity}、应调之众{signal.AvailableForceCount}。";
    }

    public static string BuildLegacyCommanderSummary(CampaignFrontState campaign, CampaignMobilizationSignalState? signal)
    {
        string? commandFit = signal?.CommandFitLabel;
        if (string.IsNullOrWhiteSpace(commandFit))
        {
            commandFit = InferCommandFitFromCampaign(campaign);
        }

        if (!string.IsNullOrWhiteSpace(campaign.OfficeCoordinationTrace)
            && !string.Equals(campaign.OfficeCoordinationTrace, NoOfficeCoordinationTrace, StringComparison.Ordinal))
        {
            return $"{campaign.OfficeCoordinationTrace} {campaign.AnchorSettlementName}军务仍维持{commandFit}。";
        }

        return
            $"{campaign.AnchorSettlementName}本地统兵正以{commandFit}维持{campaign.MobilizedForceCount}人应调之众，粮道刻度为{campaign.SupplyState}。";
    }

    public static string InferCommandFitFromCampaign(CampaignFrontState campaign)
    {
        int inferredCommandCapacity = Math.Clamp(
            (campaign.MobilizedForceCount / 2)
            + (campaign.MoraleState / 4)
            + Math.Max(0, campaign.SupplyState - campaign.FrontPressure) / 5,
            0,
            100);

        int inferredResponseActivation = Math.Clamp(campaign.FrontPressure - 15, 0, 100);
        int inferredOfficeTier = string.IsNullOrWhiteSpace(campaign.OfficeCoordinationTrace)
            || string.Equals(campaign.OfficeCoordinationTrace, NoOfficeCoordinationTrace, StringComparison.Ordinal)
            ? 0
            : 2;
        int inferredBacklog = campaign.SupplyState < 45 ? 18 : 8;

        return DetermineCommandFitLabel(
            inferredCommandCapacity,
            inferredResponseActivation,
            inferredOfficeTier,
            inferredBacklog);
    }
}
