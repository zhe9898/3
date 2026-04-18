using System;
using System.Collections.Generic;
using System.Linq;

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
}
