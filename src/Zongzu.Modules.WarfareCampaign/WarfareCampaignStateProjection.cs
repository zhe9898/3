using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Modules.WarfareCampaign;

public static class WarfareCampaignStateProjection
{
    public static WarfareCampaignState UpgradeFromSchemaV1(WarfareCampaignState state)
    {
        Dictionary<int, CampaignMobilizationSignalState> signalsBySettlement = UpgradeDescriptors(state);
        return UpgradeFromSchemaV2(state, signalsBySettlement);
    }

    public static WarfareCampaignState UpgradeFromSchemaV2(WarfareCampaignState state)
    {
        Dictionary<int, CampaignMobilizationSignalState> signalsBySettlement = state.MobilizationSignals
            .ToDictionary(static signal => signal.SettlementId.Value, static signal => signal);
        return UpgradeFromSchemaV2(state, signalsBySettlement);
    }

    private static WarfareCampaignState UpgradeFromSchemaV2(
        WarfareCampaignState state,
        IReadOnlyDictionary<int, CampaignMobilizationSignalState> signalsBySettlement)
    {
        foreach (CampaignMobilizationSignalState signal in state.MobilizationSignals)
        {
            if (string.IsNullOrWhiteSpace(signal.ActiveDirectiveCode))
            {
                signal.ActiveDirectiveCode = WarfareCampaignDescriptors.DetermineDirectiveCode(
                    true,
                    Math.Clamp(signal.ResponseActivationLevel + 24, 0, 100),
                    Math.Clamp(40 + signal.AdministrativeLeverage - (signal.PetitionBacklog / 2), 0, 100),
                    signal.PetitionBacklog);
            }

            signal.ActiveDirectiveLabel = string.IsNullOrWhiteSpace(signal.ActiveDirectiveLabel)
                ? WarfareCampaignDescriptors.DetermineDirectiveLabel(signal.ActiveDirectiveCode)
                : signal.ActiveDirectiveLabel;
            signal.ActiveDirectiveSummary = string.IsNullOrWhiteSpace(signal.ActiveDirectiveSummary)
                ? WarfareCampaignDescriptors.BuildDirectiveSummary(signal.ActiveDirectiveCode, signal.SettlementName)
                : signal.ActiveDirectiveSummary;
        }

        foreach (CampaignFrontState campaign in state.Campaigns)
        {
            signalsBySettlement.TryGetValue(campaign.AnchorSettlementId.Value, out CampaignMobilizationSignalState? signal);

            if (string.IsNullOrWhiteSpace(campaign.ActiveDirectiveCode))
            {
                campaign.ActiveDirectiveCode = signal?.ActiveDirectiveCode
                    ?? WarfareCampaignDescriptors.DetermineDirectiveCode(
                        campaign.IsActive,
                        campaign.FrontPressure,
                        campaign.SupplyState,
                        0);
            }

            campaign.ActiveDirectiveLabel = string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)
                ? WarfareCampaignDescriptors.DetermineDirectiveLabel(campaign.ActiveDirectiveCode)
                : campaign.ActiveDirectiveLabel;
            campaign.ActiveDirectiveSummary = string.IsNullOrWhiteSpace(campaign.ActiveDirectiveSummary)
                ? WarfareCampaignDescriptors.BuildDirectiveSummary(campaign.ActiveDirectiveCode, campaign.AnchorSettlementName)
                : campaign.ActiveDirectiveSummary;
            campaign.LastDirectiveTrace = string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace)
                ? $"{campaign.AnchorSettlementName}当前军令为{campaign.ActiveDirectiveLabel}：{campaign.ActiveDirectiveSummary}"
                : campaign.LastDirectiveTrace;
        }

        state.Campaigns = state.Campaigns
            .OrderBy(static campaign => campaign.CampaignId.Value)
            .ToList();
        state.MobilizationSignals = state.MobilizationSignals
            .OrderBy(static signal => signal.SettlementId.Value)
            .ToList();

        return state;
    }

    private static Dictionary<int, CampaignMobilizationSignalState> UpgradeDescriptors(WarfareCampaignState state)
    {
        Dictionary<int, CampaignMobilizationSignalState> signalsBySettlement = new();

        foreach (CampaignMobilizationSignalState signal in state.MobilizationSignals)
        {
            signal.CommandFitLabel = string.IsNullOrWhiteSpace(signal.CommandFitLabel)
                ? WarfareCampaignDescriptors.DetermineCommandFitLabel(
                    signal.CommandCapacity,
                    signal.ResponseActivationLevel,
                    signal.OfficeAuthorityTier,
                    signal.PetitionBacklog)
                : signal.CommandFitLabel;
            signalsBySettlement[signal.SettlementId.Value] = signal;
        }

        foreach (CampaignFrontState campaign in state.Campaigns)
        {
            signalsBySettlement.TryGetValue(campaign.AnchorSettlementId.Value, out CampaignMobilizationSignalState? signal);

            campaign.FrontLabel = string.IsNullOrWhiteSpace(campaign.FrontLabel)
                ? WarfareCampaignDescriptors.DetermineFrontLabel(campaign.FrontPressure)
                : campaign.FrontLabel;
            campaign.SupplyStateLabel = string.IsNullOrWhiteSpace(campaign.SupplyStateLabel)
                ? WarfareCampaignDescriptors.DetermineSupplyStateLabel(campaign.SupplyState)
                : campaign.SupplyStateLabel;
            campaign.MoraleStateLabel = string.IsNullOrWhiteSpace(campaign.MoraleStateLabel)
                ? WarfareCampaignDescriptors.DetermineMoraleStateLabel(campaign.MoraleState)
                : campaign.MoraleStateLabel;
            campaign.CommandFitLabel = string.IsNullOrWhiteSpace(campaign.CommandFitLabel)
                ? signal?.CommandFitLabel ?? WarfareCampaignDescriptors.InferCommandFitFromCampaign(campaign)
                : campaign.CommandFitLabel;
            campaign.CommanderSummary = string.IsNullOrWhiteSpace(campaign.CommanderSummary)
                ? WarfareCampaignDescriptors.BuildLegacyCommanderSummary(campaign, signal)
                : campaign.CommanderSummary;

            if (campaign.Routes.Count == 0)
            {
                campaign.Routes = BuildLegacyRoutes(campaign, signal);
            }
            else
            {
                foreach (CampaignRouteState route in campaign.Routes)
                {
                    route.FlowStateLabel = string.IsNullOrWhiteSpace(route.FlowStateLabel)
                        ? WarfareCampaignDescriptors.DetermineRouteFlowStateLabel(route.Pressure, route.Security)
                        : route.FlowStateLabel;
                    route.Summary = string.IsNullOrWhiteSpace(route.Summary)
                        ? $"{route.RouteLabel}当前为{route.FlowStateLabel}，压力{route.Pressure}，护持{route.Security}。"
                        : route.Summary;
                }
            }
        }

        return signalsBySettlement;
    }

    private static List<CampaignRouteState> BuildLegacyRoutes(
        CampaignFrontState campaign,
        CampaignMobilizationSignalState? signal)
    {
        int escortPressure = Math.Clamp(
            campaign.FrontPressure - (campaign.SupplyState / 4) + ((signal?.PetitionBacklog ?? 0) / 2),
            0,
            100);
        int escortSecurity = Math.Clamp(
            campaign.SupplyState + Math.Max(12, campaign.MobilizedForceCount / 5),
            0,
            100);
        int relayPressure = Math.Clamp(
            campaign.FrontPressure - (campaign.MoraleState / 4) + 10,
            0,
            100);
        int relaySecurity = Math.Clamp(
            campaign.MoraleState + Math.Max(8, campaign.MobilizedForceCount / 6),
            0,
            100);
        int administrativePressure = Math.Clamp(
            campaign.FrontPressure + ((signal?.PetitionBacklog ?? 0) / 2) - Math.Max(0, (signal?.AdministrativeLeverage ?? 0) / 4),
            0,
            100);
        int administrativeSecurity = Math.Clamp(
            campaign.SupplyState + ((signal?.OfficeAuthorityTier ?? 0) * 8),
            0,
            100);

        return
        [
            BuildRoute(
                "粮道",
                "supply",
                escortPressure,
                escortSecurity,
                $"{campaign.AnchorSettlementName}旧粮道转移后仍在维持军粮转运。"),
            BuildRoute(
                "驿报线",
                "command",
                relayPressure,
                relaySecurity,
                $"{campaign.AnchorSettlementName}旧驿报线迁移后仍在传递号令与军报。"),
            BuildRoute(
                signal is not null && signal.OfficeAuthorityTier > 0 ? "文移驿线" : "乡勇集道",
                signal is not null && signal.OfficeAuthorityTier > 0 ? "administrative" : "reserve",
                administrativePressure,
                administrativeSecurity,
                signal is not null && signal.OfficeAuthorityTier > 0
                    ? $"{campaign.AnchorSettlementName}旧文移驿线仍与积案外溢和官署杠杆牵连。"
                    : $"{campaign.AnchorSettlementName}旧乡勇集道仍在承接无官署覆盖的后备流转。"),
        ];
    }

    private static CampaignRouteState BuildRoute(
        string routeLabel,
        string routeRole,
        int pressure,
        int security,
        string summaryPrefix)
    {
        string flowState = WarfareCampaignDescriptors.DetermineRouteFlowStateLabel(pressure, security);
        return new CampaignRouteState
        {
            RouteLabel = routeLabel,
            RouteRole = routeRole,
            Pressure = pressure,
            Security = security,
            FlowStateLabel = flowState,
            Summary = $"{summaryPrefix} 当前为{flowState}，压力{pressure}，护持{security}。",
        };
    }

    public static WarfareCampaignState UpgradeFromSchemaV3(WarfareCampaignState state)
    {
        state.AftermathDockets ??= new List<AftermathDocketState>();
        foreach (CampaignFrontState campaign in state.Campaigns)
        {
            campaign.ContestedRouteIds ??= new List<string>();
        }
        BuildCampaignPhasingAndAftermath(state);
        return state;
    }

    public static void BuildCampaignPhasingAndAftermath(WarfareCampaignState state)
    {
        state.Campaigns ??= new List<CampaignFrontState>();
        state.AftermathDockets ??= new List<AftermathDocketState>();

        Dictionary<int, AftermathDocketState> priorDockets = state.AftermathDockets
            .GroupBy(static docket => docket.CampaignId.Value)
            .ToDictionary(static group => group.Key, static group => group.First());

        List<AftermathDocketState> nextDockets = new();

        foreach (CampaignFrontState campaign in state.Campaigns)
        {
            campaign.Routes ??= new List<CampaignRouteState>();
            campaign.ContestedRouteIds ??= new List<string>();

            campaign.Phase = ClassifyCampaignPhase(campaign);
            campaign.CommittedForces = campaign.MobilizedForceCount;
            campaign.SupplyStretch = System.Math.Clamp(100 - campaign.SupplyState, 0, 100);
            campaign.CommandFit = ProjectCommandFitScore(campaign.CommandFitLabel);
            campaign.CivilianExposure = System.Math.Clamp(
                (campaign.FrontPressure * 2 / 3) + (campaign.SupplyStretch / 3),
                0,
                100);

            campaign.ContestedRouteIds = campaign.Routes
                .Where(static route => route.Pressure >= 55)
                .OrderByDescending(static route => route.Pressure)
                .Select(static route => route.RouteLabel)
                .ToList();

            if (campaign.Phase == Zongzu.Contracts.CampaignPhase.Aftermath)
            {
                priorDockets.TryGetValue(campaign.CampaignId.Value, out AftermathDocketState? prior);
                nextDockets.Add(BuildAftermathDocket(campaign, prior));
            }
        }

        state.AftermathDockets = nextDockets
            .OrderBy(static docket => docket.CampaignId.Value)
            .ToList();
    }

    private static Zongzu.Contracts.CampaignPhase ClassifyCampaignPhase(CampaignFrontState campaign)
    {
        if (!string.IsNullOrWhiteSpace(campaign.LastAftermathSummary))
        {
            return Zongzu.Contracts.CampaignPhase.Aftermath;
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.WithdrawToBarracks, System.StringComparison.Ordinal))
        {
            return Zongzu.Contracts.CampaignPhase.Withdrawing;
        }

        if (!campaign.IsActive && campaign.MobilizedForceCount <= 0)
        {
            return Zongzu.Contracts.CampaignPhase.Proposed;
        }

        if (campaign.MobilizedForceCount > 0 && campaign.FrontPressure < 25)
        {
            return campaign.MobilizedForceCount >= 120
                ? Zongzu.Contracts.CampaignPhase.Marshalled
                : Zongzu.Contracts.CampaignPhase.Mobilizing;
        }

        if (campaign.FrontPressure >= 80 && campaign.MoraleState >= 45)
        {
            return Zongzu.Contracts.CampaignPhase.Decisive;
        }

        if (campaign.FrontPressure >= 55)
        {
            return campaign.MoraleState <= 30
                ? Zongzu.Contracts.CampaignPhase.Stalemate
                : Zongzu.Contracts.CampaignPhase.Engaged;
        }

        return campaign.MobilizedForceCount >= 120
            ? Zongzu.Contracts.CampaignPhase.Marshalled
            : Zongzu.Contracts.CampaignPhase.Mobilizing;
    }

    private static int ProjectCommandFitScore(string commandFitLabel)
    {
        return commandFitLabel switch
        {
            "命出一辙" => 82,
            "军令畅行" => 68,
            "命令迟滞" => 42,
            "案牍梗塞" => 22,
            _ => 50,
        };
    }

    private static AftermathDocketState BuildAftermathDocket(CampaignFrontState campaign, AftermathDocketState? prior)
    {
        List<string> merits = new();
        List<string> blames = new();
        List<string> reliefNeeds = new();
        List<string> routeRepairs = new();

        if (campaign.MoraleState >= 55)
        {
            merits.Add($"{campaign.AnchorSettlementName}行伍士气未堕，押队将佐宜录军功簿。");
        }
        if (campaign.SupplyState >= 55)
        {
            merits.Add($"{campaign.AnchorSettlementName}粮道未绝，转运司吏员合议叙劳。");
        }

        if (campaign.FrontPressure >= 70)
        {
            blames.Add($"{campaign.AnchorSettlementName}前沿压迫过甚，主将号令未能及时收束，宜入案待议。");
        }
        if (campaign.SupplyStretch >= 60)
        {
            blames.Add($"{campaign.AnchorSettlementName}粮道吃紧，转运与护送两处责任未清，宜以文移追究。");
        }

        if (campaign.CivilianExposure >= 55)
        {
            reliefNeeds.Add($"{campaign.AnchorSettlementName}百姓受战事牵连，宜先议免役、赈粮与安置流民。");
        }
        if (campaign.MoraleState <= 30)
        {
            reliefNeeds.Add($"{campaign.AnchorSettlementName}行伍疲敝，宜拨医药、抚恤与归休之资。");
        }

        foreach (string routeLabel in campaign.ContestedRouteIds)
        {
            routeRepairs.Add($"{routeLabel}久受冲压，宜遣工匠巡修桥渡、驿馆与粮仓。");
        }

        string summary = $"{campaign.AnchorSettlementName}战后案卷：功{merits.Count}条、责{blames.Count}条、恤{reliefNeeds.Count}条、修路{routeRepairs.Count}条。";

        return new AftermathDocketState
        {
            CampaignId = campaign.CampaignId,
            Merits = merits,
            Blames = blames,
            ReliefNeeds = reliefNeeds,
            RouteRepairs = routeRepairs,
            DocketSummary = summary,
        };
    }
}
