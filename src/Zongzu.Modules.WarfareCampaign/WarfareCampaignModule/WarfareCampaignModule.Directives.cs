using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WarfareCampaign;

public sealed partial class WarfareCampaignModule : ModuleRunner<WarfareCampaignState>
{
    private static void ApplyDirectiveDefaults(

        CampaignMobilizationSignalState signal,

        string settlementName,

        bool isActive,

        LocalForcePoolSnapshot localForce,

        SettlementSnapshot settlement)

    {

        if (string.IsNullOrWhiteSpace(signal.ActiveDirectiveCode))

        {

            int baselineFront = ComputeFrontPressure(settlement, localForce, signal);

            int baselineSupply = ComputeSupplyState(settlement, localForce, signal, baselineFront);

            signal.ActiveDirectiveCode = WarfareCampaignDescriptors.DetermineDirectiveCode(

                isActive,

                baselineFront,

                baselineSupply,

                signal.PetitionBacklog);

        }


        signal.ActiveDirectiveLabel = WarfareCampaignDescriptors.DetermineDirectiveLabel(signal.ActiveDirectiveCode);

        signal.ActiveDirectiveSummary = WarfareCampaignDescriptors.BuildDirectiveSummary(signal.ActiveDirectiveCode, settlementName);

    }


    private static (int MobilizedForceCount, int FrontPressure, int SupplyState, int MoraleState) ApplyDirectiveAdjustments(

        string directiveCode,

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal)

    {

        int mobilizedForceCount = signal.AvailableForceCount;

        int frontPressure = ComputeFrontPressure(settlement, localForce, signal);

        int supplyState = ComputeSupplyState(settlement, localForce, signal, frontPressure);

        int moraleState = ComputeMoraleState(localForce, signal, frontPressure);


        switch (directiveCode)

        {

            case WarfareCampaignCommandNames.DraftCampaignPlan:

                frontPressure = Math.Clamp(frontPressure - 3, 0, 100);

                moraleState = Math.Clamp(moraleState + 2, 0, 100);

                mobilizedForceCount = Math.Max(0, signal.AvailableForceCount - Math.Max(2, signal.AvailableForceCount / 10));

                break;

            case WarfareCampaignCommandNames.CommitMobilization:

                frontPressure = Math.Clamp(frontPressure + 6, 0, 100);

                supplyState = Math.Clamp(supplyState - 5, 0, 100);

                moraleState = Math.Clamp(moraleState + 4, 0, 100);

                break;

            case WarfareCampaignCommandNames.ProtectSupplyLine:

                frontPressure = Math.Clamp(frontPressure - 4, 0, 100);

                supplyState = Math.Clamp(supplyState + 9, 0, 100);

                moraleState = Math.Clamp(moraleState + 1, 0, 100);

                mobilizedForceCount = Math.Max(0, signal.AvailableForceCount - Math.Max(1, signal.AvailableForceCount / 12));

                break;

            case WarfareCampaignCommandNames.WithdrawToBarracks:

                frontPressure = Math.Clamp(frontPressure - 10, 0, 100);

                supplyState = Math.Clamp(supplyState + 4, 0, 100);

                moraleState = Math.Clamp(moraleState - 3, 0, 100);

                mobilizedForceCount = Math.Max(0, signal.AvailableForceCount - Math.Max(4, signal.AvailableForceCount / 5));

                break;

        }


        return (mobilizedForceCount, frontPressure, supplyState, moraleState);

    }


    private static string BuildObjectiveSummary(

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal,

        bool isActive)

    {

        string windowLabel = DescribeMobilizationWindow(signal.MobilizationWindowLabel);


        if (!isActive)

        {

            return $"先收整{settlement.Name}行伍与辎重，稳住{windowLabel}动员窗口，再议后续檄令。";

        }


        if (localForce.HasActiveConflict)

        {

            return $"守住{settlement.Name}前缘与渡口，护住粮道，不使地方冲突外溢，同时维持{windowLabel}动员窗口。";

        }


        if (localForce.IsResponseActivated)

        {

            return $"整束{settlement.Name}周边应调之众，稳住粮道与驿报，把已启用的响应姿态转成可持续的军务态势。";

        }


        return $"在{settlement.Name}周边先立军务戒备，不急于放大边缘摩擦。";

    }


    private static string BuildSupplyLineSummary(

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal,

        JurisdictionAuthoritySnapshot? jurisdiction)

    {

        string corridorState = signal.MobilizationWindowLabel switch

        {

            "Open" => "可守",

            "Narrow" => "脆弱",

            "Preparing" => "成形",

            _ => "封闭",

        };


        string leverageText = jurisdiction is null

            ? "暂无官署文移接应"

            : $"{jurisdiction.LeadOfficeTitle}杠杆{signal.AdministrativeLeverage}，积案{signal.PetitionBacklog}";


        return $"{settlement.Name}粮道由护运{localForce.EscortCount}维持{corridorState}之势；繁荣{settlement.Prosperity}，并有{leverageText}。";

    }


    private static List<CampaignRouteState> BuildRoutes(

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal,

        JurisdictionAuthoritySnapshot? jurisdiction,

        int frontPressure,

        int supplyState)

    {

        List<CampaignRouteState> routes =

        [

            BuildRoute(

                "粮道",

                "supply",

                Math.Clamp(frontPressure - (localForce.EscortCount * 2) + (signal.PetitionBacklog / 2), 0, 100),

                Math.Clamp(settlement.Security + (localForce.EscortCount * 3) + (signal.AdministrativeLeverage / 6) - 5, 0, 100),

                $"{settlement.Name}护运{localForce.EscortCount}正在维持军粮转运。"),

            BuildRoute(

                "驿报线",

                "command",

                Math.Clamp(frontPressure - (localForce.CommandCapacity / 2) + 10, 0, 100),

                Math.Clamp(settlement.Security + localForce.GuardCount + localForce.RetainerCount - (signal.PetitionBacklog / 4), 0, 100),

                $"{settlement.Name}守丁{localForce.GuardCount}与亲随{localForce.RetainerCount}正在传递号令与驿报。"),

        ];


        if (jurisdiction is not null && jurisdiction.AuthorityTier > 0)

        {

            routes.Add(BuildRoute(

                "文移驿线",

                "administrative",

                Math.Clamp(frontPressure + (signal.PetitionBacklog / 2) - (signal.AdministrativeLeverage / 4), 0, 100),

                Math.Clamp(settlement.Security + (jurisdiction.AuthorityTier * 8) - (signal.PetitionBacklog / 3), 0, 100),

                $"{jurisdiction.LeadOfficeTitle}的文移与军务沙盘正在彼此牵连。"));

        }

        else

        {

            routes.Add(BuildRoute(

                "乡勇集道",

                "reserve",

                Math.Clamp(frontPressure + Math.Max(0, 30 - localForce.MilitiaCount), 0, 100),

                Math.Clamp(settlement.Security + localForce.MilitiaCount - 5, 0, 100),

                $"{settlement.Name}乡勇{localForce.MilitiaCount}正沿集道轮番应点。"));

        }


        if (supplyState <= 45)

        {

            routes.Add(BuildRoute(

                "转运支道",

                "support",

                Math.Clamp(frontPressure + 8 + (signal.PetitionBacklog / 3), 0, 100),

                Math.Clamp(settlement.Prosperity + localForce.EscortCount - 10, 0, 100),

                $"{settlement.Name}正在启用临时转运支道，以纾缓粮道紧张。"));

        }


        return routes;

    }


    private static string BuildOfficeCoordinationTrace(JurisdictionAuthoritySnapshot? jurisdiction)

    {

        if (jurisdiction is null || jurisdiction.AuthorityTier <= 0)

        {

            return WarfareCampaignDescriptors.NoOfficeCoordinationTrace;

        }


        return $"{jurisdiction.LeadOfficeTitle} {jurisdiction.LeadOfficialName}正在协调{jurisdiction.AdministrativeTaskTier}层级文移，积案{jurisdiction.PetitionBacklog}。";

    }


    private static string BuildMobilizationSourceTrace(

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal)

    {

        return $"{settlement.Name}军务态势取材于可调之众{signal.AvailableForceCount}、整备{localForce.Readiness}、统摄{localForce.CommandCapacity}、治安{settlement.Security}、支援{signal.OrderSupportLevel}，并维持{signal.CommandFitLabel}。";

    }


    private static string BuildAftermathSummary(

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal,

        JurisdictionAuthoritySnapshot? jurisdiction,

        CampaignFrontState campaign,

        bool isActive)

    {

        if (!isActive)

        {

            return $"{settlement.Name}正在覆核战后余波：修补护运、消化积案，并围绕{DescribeMobilizationWindow(signal.MobilizationWindowLabel)}窗口稳住军心。";

        }


        if (campaign.SupplyState <= 40)

        {

            return $"{settlement.Name}粮道仍吃紧；护运、积案与地方财力都在压缩久战之力。";

        }


        if (campaign.MoraleState <= 40)

        {

            return $"{settlement.Name}军心浮动；即便仍有乡勇{localForce.MilitiaCount}与守丁{localForce.GuardCount}撑住沙盘，也不宜再躁进。";

        }


        string officeText = jurisdiction is null

            ? "暂无官署接应"

            : $"有{jurisdiction.LeadOfficeTitle}接应";

        return $"{settlement.Name}军务态势尚能持守，{officeText}；前线{campaign.FrontPressure}，粮道{campaign.SupplyState}，军心{campaign.MoraleState}。";

    }


    private static CampaignRouteState BuildRoute(

        string routeLabel,

        string routeRole,

        int pressure,

        int security,

        string summaryPrefix)

    {

        string flowStateLabel = WarfareCampaignDescriptors.DetermineRouteFlowStateLabel(pressure, security);

        return new CampaignRouteState

        {

            RouteLabel = routeLabel,

            RouteRole = routeRole,

            Pressure = pressure,

            Security = security,

            FlowStateLabel = flowStateLabel,

            Summary = $"{summaryPrefix} 当前为{flowStateLabel}，压力{pressure}，护持{security}。",

        };

    }


    private static string BuildDirectiveTrace(

        CampaignFrontState campaign,

        CampaignMobilizationSignalState signal,

        string settlementName)

    {

        return $"{settlementName}当前军令为{signal.ActiveDirectiveLabel}：{signal.ActiveDirectiveSummary} 其势表现为{campaign.FrontLabel}、{campaign.SupplyStateLabel}、{campaign.MoraleStateLabel}。";

    }


    private static CampaignMobilizationSignalState GetOrCreateSignal(WarfareCampaignState state, SettlementSnapshot settlement)

    {

        CampaignMobilizationSignalState? signal = state.MobilizationSignals.SingleOrDefault(existing => existing.SettlementId == settlement.Id);

        if (signal is not null)

        {

            signal.SettlementName = settlement.Name;

            return signal;

        }


        signal = new CampaignMobilizationSignalState

        {

            SettlementId = settlement.Id,

            SettlementName = settlement.Name,

        };

        state.MobilizationSignals.Add(signal);

        return signal;

    }


    private static CampaignId NextCampaignId(WarfareCampaignState state)

    {

        int nextValue = state.Campaigns.Count == 0

            ? 1

            : state.Campaigns.Max(static campaign => campaign.CampaignId.Value) + 1;

        return new CampaignId(nextValue);

    }


}
