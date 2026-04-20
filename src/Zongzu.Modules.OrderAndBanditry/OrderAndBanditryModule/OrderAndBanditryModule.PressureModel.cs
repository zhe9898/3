using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed partial class OrderAndBanditryModule : ModuleRunner<OrderAndBanditryState>
{
    private static string BuildPressureReason(

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        TradeActivitySnapshot tradeActivity,

        int localFear,

        int localGrudge,

        LocalForcePoolSnapshot? localForce,

        int forceSuppression,

        JurisdictionAuthoritySnapshot? jurisdiction,

        int administrativeRelief)

    {

        List<string> reasons = [];


        if (settlement.Security < 50)

        {

            reasons.Add($"乡面安宁仅{settlement.Security}，仓路与铺户更易外露。");

        }


        if (population.CommonerDistress >= 55)

        {

            reasons.Add($"民困{population.CommonerDistress}，乡里窘急渐聚。");

        }


        if (localForce is not null && forceSuppression > 0)

        {

            reasons.Add($"已激活的守丁{localForce.GuardCount}、护运{localForce.EscortCount}、整备{localForce.Readiness}与应援{localForce.OrderSupportLevel}，正缓住事势。");

        }


        if (jurisdiction is not null && administrativeRelief > 0)

        {

            reasons.Add($"{jurisdiction.LeadOfficeTitle}乡面杠力{jurisdiction.JurisdictionLeverage}，可替镇压之需卸去{administrativeRelief}分。");

        }


        if (population.MigrationPressure >= 45)

        {

            reasons.Add($"流徙之压{population.MigrationPressure}，乡面人气渐散。");

        }


        if (tradeActivity.ActiveRouteCount > 0)

        {

            reasons.Add($"现有{tradeActivity.ActiveRouteCount}条活路、载力{tradeActivity.TotalRouteCapacity}，行旅财货尽在外露。");

        }


        if (tradeActivity.AverageRouteRisk >= 45)

        {

            reasons.Add($"路险均值{tradeActivity.AverageRouteRisk}，最招乘隙劫掠。");

        }


        if (localFear >= 50 || localGrudge >= 55)

        {

            reasons.Add($"乡里惧意{localFear}、旧怨{localGrudge}，渐化为不靖之势。");

        }


        if (reasons.Count == 0)

        {

            reasons.Add("本月乡里大势尚能按住。");

        }


        return string.Join(" ", reasons.Take(3));

    }


    private static int ComputeForceSuppression(LocalForcePoolSnapshot localForce)

    {

        if (!localForce.HasActiveConflict || !localForce.IsResponseActivated || localForce.OrderSupportLevel <= 0)

        {

            return 0;

        }


        return localForce.OrderSupportLevel;

    }


    private static int ComputePaperCompliance(JurisdictionAuthoritySnapshot? jurisdiction)

    {

        if (jurisdiction is null)

        {

            return 0;

        }


        int taskSignal = jurisdiction.AdministrativeTaskTier switch

        {

            "crisis" => 12,

            "district" => 10,

            "registry" => 6,

            "clerical" => 4,

            _ => 3,

        };


        return Math.Clamp(

            (jurisdiction.JurisdictionLeverage / 2)

            + (jurisdiction.AuthorityTier * 10)

            + taskSignal

            + (jurisdiction.AdministrativeTaskLoad / 3)

            - (jurisdiction.PetitionPressure / 8),

            0,

            100);

    }


    private static int ComputeImplementationDrag(JurisdictionAuthoritySnapshot? jurisdiction, int paperCompliance)

    {

        if (jurisdiction is null)

        {

            return 0;

        }


        return Math.Clamp(

            (jurisdiction.ClerkDependence / 2)

            + (jurisdiction.PetitionPressure / 2)

            + (jurisdiction.PetitionBacklog / 2)

            + (jurisdiction.AdministrativeTaskLoad / 3)

            - (paperCompliance / 5)

            - (jurisdiction.AuthorityTier * 4),

            0,

            100);

    }


    private static int ComputeAdministrativeRelief(

        JurisdictionAuthoritySnapshot jurisdiction,

        int paperCompliance,

        int implementationDrag)

    {

        int effectiveReach = paperCompliance - implementationDrag;

        int relief = effectiveReach >= 30

            ? 2

            : effectiveReach >= 12

                ? 1

                : 0;


        if (jurisdiction.PetitionPressure >= 60)

        {

            relief -= 1;

        }


        return Math.Max(0, relief);

    }


    private static int ComputeAdministrativeSuppressionWindow(

        int paperCompliance,

        int implementationDrag,

        JurisdictionAuthoritySnapshot? jurisdiction)

    {

        if (jurisdiction is null)

        {

            return 0;

        }


        return Math.Clamp(

            (paperCompliance / 16)

            + (jurisdiction.AuthorityTier >= 2 ? 1 : 0)

            - (implementationDrag / 18),

            0,

            8);

    }


    private static int ComputeRouteShielding(

        LocalForcePoolSnapshot? localForce,

        TradeActivitySnapshot tradeActivity,

        int administrativeRelief)

    {

        if (localForce is null

            || !localForce.HasActiveConflict

            || !localForce.IsResponseActivated

            || tradeActivity.ActiveRouteCount == 0)

        {

            return 0;

        }


        return Math.Clamp(

            (localForce.OrderSupportLevel * 5)

            + (localForce.EscortCount * 2)

            + (localForce.GuardCount / 2)

            + (localForce.Readiness / 3)

            + (localForce.CommandCapacity / 4)

            + (administrativeRelief * 8)

            - (localForce.CampaignEscortStrain / 2)

            - (localForce.CampaignFatigue / 3)

            - Math.Max(0, tradeActivity.AverageRouteRisk - 45) / 2,

            0,

            100);

    }


    private static int ComputeRetaliationRisk(

        SettlementDisorderState disorder,

        LocalForcePoolSnapshot? localForce,

        TradeActivitySnapshot tradeActivity,

        SettlementBlackRouteLedgerSnapshot blackRouteLedger,

        int localFear,

        int localGrudge,

        int routeShielding)

    {

        if (localForce is null || !localForce.IsResponseActivated)

        {

            return 0;

        }


        return Math.Clamp(

            (disorder.BlackRoutePressure / 3)

            + (disorder.RoutePressure / 4)

            + (localFear / 4)

            + (localGrudge / 4)

            + (blackRouteLedger.DiversionShare / 5)

            + (blackRouteLedger.IllicitMargin / 4)

            + (tradeActivity.ActiveRouteCount > 0 ? 8 : 0)

            + (tradeActivity.AverageRouteRisk / 8)

            + (localForce.CampaignEscortStrain / 2)

            + (localForce.CampaignFatigue / 3)

            - (routeShielding / 2)

            - (localForce.OrderSupportLevel * 2)

            - (localForce.Readiness / 6),

            0,

            100);

    }


    private static string DetermineEscalationBandLabel(int blackRoutePressure, int coercionRisk)

    {

        int combined = blackRoutePressure + coercionRisk;

        return combined switch

        {

            >= 130 => "私路成势",

            >= 100 => "暗运成线",

            >= 70 => "夹带渐多",

            >= 40 => "私贩试探",

            _ => "尚未成势",

        };

    }


    private static string BuildBlackRoutePressureTrace(

        SettlementSnapshot settlement,

        TradeActivitySnapshot tradeActivity,

        SettlementBlackRouteLedgerSnapshot blackRouteLedger,

        SettlementDisorderState disorder,

        PopulationSettlementSnapshot population,

        int localFear,

        int localGrudge)

    {

        List<string> reasons = [];


        if (tradeActivity.ActiveRouteCount > 0)

        {

            reasons.Add($"正路尚有{tradeActivity.ActiveRouteCount}条，私货便在河埠与街市间夹带试行。");

        }


        if (blackRouteLedger.DiversionShare > 0 || blackRouteLedger.IllicitMargin > 0)

        {

            reasons.Add($"私下分流{blackRouteLedger.DiversionShare}，浮利约{blackRouteLedger.IllicitMargin}，私下转运正在挤压正市。");

        }


        if (population.CommonerDistress >= 55 || localFear >= 50 || localGrudge >= 50)

        {

            reasons.Add($"民困{population.CommonerDistress}、惧意{localFear}、旧怨{localGrudge}，更易逼人投向私运与胁从。");

        }


        if (disorder.SuppressionRelief > 0 || disorder.AdministrativeSuppressionWindow > 0)

        {

            reasons.Add($"眼下可调护力{disorder.SuppressionRelief}，官面查缉窗口{disorder.AdministrativeSuppressionWindow}，还能暂压一程。");

        }


        if (reasons.Count == 0)

        {

            reasons.Add($"{settlement.Name}私路尚浅，仍以试探为主。");

        }


        return string.Join(" ", reasons.Take(3));

    }


}
