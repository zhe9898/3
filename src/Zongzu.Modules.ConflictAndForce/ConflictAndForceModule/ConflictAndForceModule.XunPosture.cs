using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed partial class ConflictAndForceModule : ModuleRunner<ConflictAndForceState>
{
    private static void ApplyXunOpeningPosture(

        SettlementForceState force,

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        SettlementDisorderSnapshot disorder,

        TradeActivitySnapshot tradeActivity,

        int averageSupport,

        int administrativeSupport)

    {

        int escortDelta = 0;

        escortDelta += tradeActivity.ActiveRouteCount > 0 ? 1 : 0;

        escortDelta += disorder.RoutePressure >= 55 ? 1 : disorder.RoutePressure < 20 ? -1 : 0;

        escortDelta += tradeActivity.AverageRouteRisk >= 55 ? 1 : 0;

        escortDelta -= force.CampaignEscortStrain >= 40 ? 1 : 0;


        int readinessDelta = 0;

        readinessDelta += disorder.SuppressionDemand >= 55 ? 1 : 0;

        readinessDelta += disorder.RoutePressure >= 55 ? 1 : 0;

        readinessDelta += administrativeSupport;

        readinessDelta += averageSupport >= 60 ? 1 : 0;

        readinessDelta -= settlement.Security < 45 ? 1 : 0;

        readinessDelta -= population.CommonerDistress >= 60 ? 1 : 0;

        readinessDelta -= force.CampaignFatigue >= 35 ? 1 : 0;


        force.EscortCount = Math.Clamp(force.EscortCount + escortDelta, 0, 50);

        force.Readiness = Math.Clamp(force.Readiness + readinessDelta, 0, 100);

    }


    private static void ApplyXunHotspotPulse(

        SettlementForceState force,

        SettlementSnapshot settlement,

        SettlementDisorderSnapshot disorder,

        TradeActivitySnapshot tradeActivity,

        int localFear,

        int localGrudge,

        int averagePrestige,

        int administrativeSupport)

    {

        int commandDelta = 0;

        commandDelta += averagePrestige >= 55 ? 1 : 0;

        commandDelta += administrativeSupport;

        commandDelta += disorder.BanditThreat >= 60 ? 1 : 0;

        commandDelta -= localGrudge >= 60 ? 1 : 0;

        commandDelta -= force.CampaignFatigue >= 40 ? 1 : 0;


        int readinessDelta = 0;

        readinessDelta += disorder.DisorderPressure >= 60 ? 1 : 0;

        readinessDelta += disorder.RoutePressure >= 55 ? 1 : 0;

        readinessDelta -= localFear >= 60 ? 1 : 0;

        readinessDelta -= tradeActivity.AverageRouteRisk >= 70 ? 1 : 0;


        force.CommandCapacity = Math.Clamp(force.CommandCapacity + commandDelta, 0, 100);

        force.Readiness = Math.Clamp(force.Readiness + readinessDelta, 0, 100);


        if (settlement.Security < 45 && disorder.RoutePressure >= 60)

        {

            force.EscortCount = Math.Clamp(force.EscortCount + 1, 0, 50);

        }

    }


    private static void ApplyXunClosingPosture(

        SettlementForceState force,

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        SettlementDisorderSnapshot disorder,

        TradeActivitySnapshot tradeActivity,

        int localFear,

        int localGrudge,

        int administrativeSupport)

    {

        int readinessDelta = 0;

        readinessDelta += administrativeSupport;

        readinessDelta += disorder.SuppressionDemand >= 50 ? 1 : 0;

        readinessDelta -= population.MigrationPressure >= 55 ? 1 : 0;

        readinessDelta -= localFear >= 55 ? 1 : 0;

        readinessDelta -= force.CampaignFatigue >= 45 ? 1 : 0;


        int escortDelta = 0;

        escortDelta += tradeActivity.ActiveRouteCount > 0 && disorder.RoutePressure >= 45 ? 1 : 0;

        escortDelta -= tradeActivity.ActiveRouteCount == 0 ? 1 : 0;

        escortDelta -= settlement.Security >= 65 && disorder.RoutePressure < 25 ? 1 : 0;


        int commandDelta = 0;

        commandDelta += localGrudge >= 55 ? -1 : 0;

        commandDelta += disorder.DisorderPressure >= 60 ? 1 : 0;

        commandDelta += force.CommandCapacity < 20 && administrativeSupport > 0 ? 1 : 0;


        force.Readiness = Math.Clamp(force.Readiness + readinessDelta, 0, 100);

        force.EscortCount = Math.Clamp(force.EscortCount + escortDelta, 0, 50);

        force.CommandCapacity = Math.Clamp(force.CommandCapacity + commandDelta, 0, 100);

    }


}
