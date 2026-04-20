using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WarfareCampaign;

public sealed partial class WarfareCampaignModule : ModuleRunner<WarfareCampaignState>
{
    private static bool ShouldActivateCampaign(LocalForcePoolSnapshot localForce, CampaignMobilizationSignalState signal)

    {

        if (localForce.HasActiveConflict)

        {

            return true;

        }


        if (localForce.IsResponseActivated && localForce.ResponseActivationLevel >= 24)

        {

            return true;

        }


        return localForce.ResponseActivationLevel >= 30

            && signal.AvailableForceCount >= 35

            && localForce.CommandCapacity >= 28;

    }


    private static int ComputeFrontPressure(

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal)

    {

        int officeRelief = Math.Min(12, (signal.AdministrativeLeverage / 8) + (signal.OfficeAuthorityTier * 2));

        int pressure = 22

            + localForce.ResponseActivationLevel

            + (localForce.HasActiveConflict ? 14 : 4)

            + Math.Max(0, 55 - settlement.Security) / 2

            - (localForce.CommandCapacity / 5)

            - officeRelief

            + (signal.PetitionBacklog / 6);


        return Math.Clamp(pressure, 0, 100);

    }


    private static int ComputeSupplyState(

        SettlementSnapshot settlement,

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal,

        int frontPressure)

    {

        int supply = 38

            + (settlement.Prosperity / 2)

            + (localForce.EscortCount * 2)

            + (localForce.CommandCapacity / 4)

            + (signal.AdministrativeLeverage / 5)

            - (signal.PetitionBacklog / 3)

            - Math.Max(0, frontPressure - 55) / 3;


        return Math.Clamp(supply, 0, 100);

    }


    private static int ComputeMoraleState(

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal,

        int frontPressure)

    {

        int morale = 35

            + (localForce.Readiness / 2)

            + (localForce.CommandCapacity / 4)

            + (signal.OrderSupportLevel / 2)

            + (signal.OfficeAuthorityTier * 4)

            - (signal.PetitionBacklog / 4)

            - Math.Max(0, frontPressure - 60) / 3;


        return Math.Clamp(morale, 0, 100);

    }


    private static string DetermineMobilizationWindow(

        LocalForcePoolSnapshot localForce,

        CampaignMobilizationSignalState signal,

        SettlementSnapshot settlement)

    {

        if (localForce.HasActiveConflict && localForce.IsResponseActivated && signal.AdministrativeLeverage >= 40)

        {

            return "Open";

        }


        if (localForce.IsResponseActivated || localForce.ResponseActivationLevel >= 28)

        {

            return "Narrow";

        }


        if (localForce.Readiness >= 35 && settlement.Security < 60)

        {

            return "Preparing";

        }


        return "Closed";

    }


    private static string DescribeMobilizationWindow(string mobilizationWindowLabel)

    {

        return mobilizationWindowLabel switch

        {

            "Open" => "可开",

            "Narrow" => "可守",

            "Preparing" => "待整",

            _ => "已闭",

        };

    }


}
