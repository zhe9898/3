using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed partial class ConflictAndForceModule : ModuleRunner<ConflictAndForceState>
{
    private static void RecoverCampaignFallout(

        SettlementForceState force,

        SettlementSnapshot settlement,

        SettlementDisorderSnapshot disorder,

        int administrativeSupport)

    {

        int recovery = 2 + (settlement.Security / 30) + administrativeSupport;

        if (disorder.BanditThreat >= 55 || disorder.SuppressionDemand >= 55 || force.HasActiveConflict)

        {

            recovery = Math.Max(1, recovery - 2);

        }


        force.CampaignFatigue = Math.Max(0, force.CampaignFatigue - recovery);

        force.CampaignEscortStrain = Math.Max(0, force.CampaignEscortStrain - (recovery + (settlement.Prosperity >= 60 ? 1 : 0)));


        if (force.CampaignFatigue == 0 && force.CampaignEscortStrain == 0)

        {

            force.LastCampaignFalloutTrace = string.Empty;

        }

    }


    private static void ApplyCampaignStrengthPenalties(SettlementForceState force)

    {

        force.GuardCount = Math.Max(0, force.GuardCount - Math.Max(0, force.CampaignFatigue - 28) / 18);

        force.RetainerCount = Math.Max(0, force.RetainerCount - Math.Max(0, force.CampaignFatigue - 32) / 20);

        force.MilitiaCount = Math.Max(0, force.MilitiaCount - (force.CampaignFatigue / 16));

        force.EscortCount = Math.Max(0, force.EscortCount - Math.Max(force.CampaignFatigue / 24, force.CampaignEscortStrain / 14));

    }


    private static string BuildConflictTrace(

        SettlementSnapshot settlement,

        PopulationSettlementSnapshot population,

        SettlementDisorderSnapshot disorder,

        TradeActivitySnapshot tradeActivity,

        SettlementForceState force,

        int localGrudge,

        int localFear,

        int conflictPressure,

        int forcePosture,

        bool conflictResolved,

        bool commanderWounded,

        JurisdictionAuthoritySnapshot? jurisdiction,

        int administrativeSupport)

    {

        List<string> reasons = [];


        if (disorder.BanditThreat >= 45 || disorder.RoutePressure >= 45)

        {

            reasons.Add($"盗压{disorder.BanditThreat}、路压{disorder.RoutePressure}，逼得地面不得不加紧巡守。");

        }


        if (population.CommonerDistress >= 55 || population.MigrationPressure >= 45)

        {

            reasons.Add($"民困{population.CommonerDistress}、流徙之压{population.MigrationPressure}，都在推高地面斗殴与械争。");

        }


        if (tradeActivity.ActiveRouteCount > 0)

        {

            reasons.Add($"尚有{tradeActivity.ActiveRouteCount}条活路待护，故而护运{force.EscortCount}不能轻撤。");

        }


        if (jurisdiction is not null && administrativeSupport > 0)

        {

            reasons.Add($"{jurisdiction.LeadOfficeTitle}乡面杠力{jurisdiction.JurisdictionLeverage}，替地面守备添了{administrativeSupport}分文移支应。");

        }


        if (force.CampaignFatigue > 0 || force.CampaignEscortStrain > 0)

        {

            string falloutTrace = string.IsNullOrWhiteSpace(force.LastCampaignFalloutTrace)

                ? "前番兵事留下的困乏，仍在耗着守夜与护运。"

                : force.LastCampaignFalloutTrace;

            reasons.Add($"前番兵事仍留疲敝{force.CampaignFatigue}、护运困乏{force.CampaignEscortStrain}。{falloutTrace}");

        }


        if (conflictResolved)

        {

            reasons.Add("这一场地面冲突已先被按住，未曾外漫。");

        }


        reasons.Add($"如今守备之势{forcePosture}，正以整备{force.Readiness}、号令{force.CommandCapacity}去压冲突之压{conflictPressure}。");


        if (localFear >= 50 || localGrudge >= 50)

        {

            reasons.Add($"{settlement.Name}乡里惧意{localFear}、旧怨{localGrudge}，人心仍贴着械斗边缘。");

        }


        if (commanderWounded)

        {

            reasons.Add("虽把局面按住，却也折了一名领队，平添新怨。");

        }


        return string.Join(" ", reasons.Take(5));

    }


    private static int ComputeAdministrativeSupport(JurisdictionAuthoritySnapshot jurisdiction)

    {

        int support = 0;

        if (jurisdiction.AuthorityTier >= 2)

        {

            support += 1;

        }


        if (jurisdiction.JurisdictionLeverage >= 55)

        {

            support += 2;

        }

        else if (jurisdiction.JurisdictionLeverage >= 28)

        {

            support += 1;

        }


        if (jurisdiction.PetitionPressure >= 65)

        {

            support -= 1;

        }


        return Math.Max(0, support);

    }


    private static int ComputeCampaignFatigueDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int delta = bundle.CampaignMobilized ? 2 : 0;

        delta += bundle.CampaignPressureRaised ? 3 : 0;

        delta += bundle.CampaignSupplyStrained ? 4 : 0;

        delta += bundle.CampaignAftermathRegistered ? 2 : 0;

        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;

        delta += Math.Max(0, 55 - campaign.MoraleState) / 20;

        return Math.Max(1, delta);

    }


    private static int ComputeCampaignEscortStrainDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int delta = bundle.CampaignMobilized ? 1 : 0;

        delta += bundle.CampaignSupplyStrained ? 4 : 0;

        delta += bundle.CampaignAftermathRegistered ? 2 : 0;

        delta += Math.Max(0, 55 - campaign.SupplyState) / 12;

        return Math.Max(1, delta);

    }


    private static int ComputeImmediateReadinessDrop(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int drop = bundle.CampaignPressureRaised ? 2 : 0;

        drop += bundle.CampaignSupplyStrained ? 3 : 0;

        drop += bundle.CampaignAftermathRegistered ? 2 : 0;

        drop += Math.Max(0, campaign.FrontPressure - 60) / 20;

        return Math.Max(1, drop);

    }


    private static int ComputeImmediateCommandDrop(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int drop = bundle.CampaignSupplyStrained ? 2 : 0;

        drop += bundle.CampaignAftermathRegistered ? 1 : 0;

        drop += Math.Max(0, 55 - campaign.MoraleState) / 18;

        return Math.Max(1, drop);

    }


    private static int ComputeImmediateMilitiaLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int loss = bundle.CampaignAftermathRegistered ? 1 : 0;

        loss += campaign.FrontPressure >= 72 ? 1 : 0;

        return loss;

    }


    private static int ComputeImmediateEscortLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)

    {

        int loss = bundle.CampaignSupplyStrained ? 1 : 0;

        loss += campaign.SupplyState <= 35 ? 1 : 0;

        return loss;

    }


    private static string BuildCampaignFalloutTrace(

        WarfareCampaignEventBundle bundle,

        CampaignFrontSnapshot campaign,

        int fatigueDelta,

        int escortStrainDelta,

        int readinessDrop,

        int commandDrop)

    {

        string strainText = bundle.CampaignSupplyStrained

            ? "护粮与驿传这一线回来时已显困敝。"

            : "守夜轮值仍背着前番兵事留下的担子。";


        return $"{campaign.AnchorSettlementName}战后余波留下疲敝+{fatigueDelta}、护运困乏+{escortStrainDelta}、整备-{readinessDrop}、号令-{commandDrop}。{campaign.FrontLabel}、{campaign.SupplyStateLabel}与{campaign.LastAftermathSummary}都还压在地面守备身上。{strainText}";

    }


    private static string MergeFalloutIntoConflictTrace(string currentTrace, string falloutTrace)

    {

        if (string.IsNullOrWhiteSpace(falloutTrace)

            || currentTrace.Contains(falloutTrace, StringComparison.Ordinal))

        {

            return currentTrace;

        }


        if (string.IsNullOrWhiteSpace(currentTrace))

        {

            return falloutTrace;

        }


        return $"{currentTrace} {falloutTrace}";

    }


    private static string BuildXunConflictTrace(

        SettlementSnapshot settlement,

        SettlementDisorderSnapshot disorder,

        TradeActivitySnapshot tradeActivity,

        SettlementForceState force,

        int localFear,

        int localGrudge,

        JurisdictionAuthoritySnapshot? jurisdiction)

    {

        List<string> reasons = [];


        if (disorder.RoutePressure >= 45 || disorder.BanditThreat >= 45)

        {

            reasons.Add($"{settlement.Name}路面盗压{disorder.BanditThreat}、路压{disorder.RoutePressure}，守夜与护运都被往前推。");

        }


        if (tradeActivity.ActiveRouteCount > 0)

        {

            reasons.Add($"现有{tradeActivity.ActiveRouteCount}条活路，护运{force.EscortCount}正贴着行脚与货脚走。");

        }


        if (jurisdiction is not null && jurisdiction.JurisdictionLeverage >= 28)

        {

            reasons.Add($"{jurisdiction.LeadOfficeTitle}乡面杖力{jurisdiction.JurisdictionLeverage}，还能替地面守备撑一把。");

        }


        reasons.Add($"眼下整备{force.Readiness}、号令{force.CommandCapacity}、应势{force.ResponseActivationLevel}，惧意{localFear}、旧怨{localGrudge}都还在场。");

        return string.Join(" ", reasons.Take(4));

    }


    private static bool DetermineActiveConflict(

        bool previousHasActiveConflict,

        int responseActivationLevel,

        SettlementDisorderSnapshot disorder,

        int localGrudge,

        int localFear,

        int conflictPressure,

        int forcePosture,

        bool conflictResolved,

        bool commanderWounded)

    {

        return conflictResolved

            || commanderWounded

            || disorder.BanditThreat >= 60

            || disorder.RoutePressure >= 55

            || disorder.DisorderPressure >= 60

            || disorder.SuppressionDemand >= 55

            || localGrudge >= 60

            || localFear >= 55

            || (previousHasActiveConflict

                && responseActivationLevel >= ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel

                && (disorder.BanditThreat >= 20

                    || disorder.RoutePressure >= 20

                    || disorder.SuppressionDemand >= 15

                    || localGrudge >= 40

                    || localFear >= 40))

            || (conflictPressure - forcePosture) >= 10;

    }


    private static bool DetermineXunCarryoverConflict(

        bool previousHasActiveConflict,

        SettlementDisorderSnapshot disorder,

        int localGrudge,

        int localFear)

    {

        return previousHasActiveConflict

            && (disorder.BanditThreat >= 20

                || disorder.RoutePressure >= 20

                || disorder.SuppressionDemand >= 15

                || localGrudge >= 35

                || localFear >= 35);

    }


    private static bool DetermineXunEscalatingConflict(

        bool previousHasActiveConflict,

        int responseActivationLevel,

        SettlementDisorderSnapshot disorder,

        int localGrudge,

        int localFear,

        int conflictPressure,

        int forcePosture)

    {

        return disorder.BanditThreat >= 60

            || disorder.RoutePressure >= 55

            || disorder.DisorderPressure >= 60

            || disorder.SuppressionDemand >= 55

            || localGrudge >= 60

            || localFear >= 55

            || DetermineXunCarryoverConflict(

                previousHasActiveConflict,

                disorder,

                localGrudge,

                localFear)

            || (responseActivationLevel >= ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel

                && (conflictPressure - forcePosture) >= 80);

    }


}
