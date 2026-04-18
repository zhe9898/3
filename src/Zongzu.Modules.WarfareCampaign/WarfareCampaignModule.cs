using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WarfareCampaign;

public sealed class WarfareCampaignModule : ModuleRunner<WarfareCampaignState>
{
    private static readonly string[] CommandNames =
    [
        WarfareCampaignCommandNames.DraftCampaignPlan,
        WarfareCampaignCommandNames.CommitMobilization,
        WarfareCampaignCommandNames.ProtectSupplyLine,
        WarfareCampaignCommandNames.WithdrawToBarracks,
    ];

    private static readonly string[] EventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.WarfareCampaign;

    public override int ModuleSchemaVersion => 3;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 750;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override WarfareCampaignState CreateInitialState()
    {
        return new WarfareCampaignState();
    }

    public override void RegisterQueries(WarfareCampaignState state, QueryRegistry queries)
    {
        queries.Register<IWarfareCampaignQueries>(new WarfareCampaignQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<WarfareCampaignState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IConflictAndForceQueries conflictQueries = scope.GetRequiredQuery<IConflictAndForceQueries>();
        IOfficeAndCareerQueries? officeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer)
            ? scope.GetRequiredQuery<IOfficeAndCareerQueries>()
            : null;

        Dictionary<SettlementId, SettlementSnapshot> settlementsById = settlementQueries.GetSettlements()
            .OrderBy(static settlement => settlement.Id.Value)
            .ToDictionary(static settlement => settlement.Id, static settlement => settlement);
        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = officeQueries is null
            ? new Dictionary<SettlementId, JurisdictionAuthoritySnapshot>()
            : officeQueries.GetJurisdictions().ToDictionary(static jurisdiction => jurisdiction.SettlementId, static jurisdiction => jurisdiction);

        HashSet<SettlementId> seenSettlements = new();

        foreach (LocalForcePoolSnapshot localForce in conflictQueries.GetSettlementForces().OrderBy(static force => force.SettlementId.Value))
        {
            if (!settlementsById.TryGetValue(localForce.SettlementId, out SettlementSnapshot? settlement))
            {
                continue;
            }

            seenSettlements.Add(settlement.Id);
            JurisdictionAuthoritySnapshot? jurisdiction = jurisdictionsBySettlement.TryGetValue(settlement.Id, out JurisdictionAuthoritySnapshot? authority)
                ? authority
                : null;

            CampaignMobilizationSignalState signal = GetOrCreateSignal(scope.State, settlement);
            UpdateMobilizationSignal(signal, settlement, localForce, jurisdiction);

            CampaignFrontState? campaign = scope.State.Campaigns.SingleOrDefault(existing => existing.AnchorSettlementId == settlement.Id);
            bool shouldActivateCampaign = ShouldActivateCampaign(localForce, signal);

            if (!shouldActivateCampaign)
            {
                if (campaign is not null && campaign.IsActive)
                {
                    PopulateCampaignBoard(campaign, settlement, localForce, signal, jurisdiction, isActive: false);

                    scope.RecordDiff(
                        $"军务沙盘 {campaign.CampaignName} 已转入战后覆核：{campaign.FrontLabel}、{campaign.CommandFitLabel}，{campaign.SupplyLineSummary}；善后记要为“{campaign.LastAftermathSummary}”。",
                        campaign.CampaignId.Value.ToString());
                    scope.Emit(
                        WarfareCampaignEventNames.CampaignAftermathRegistered,
                        $"{settlement.Name}军务态势已从前线压势退入战后覆核。",
                        settlement.Id.Value.ToString());
                }

                continue;
            }

            bool isNewCampaign = campaign is null;
            int previousFrontPressure = campaign?.FrontPressure ?? 0;
            int previousSupplyState = campaign?.SupplyState ?? 100;
            bool previousActive = campaign?.IsActive ?? false;

            if (campaign is null)
            {
                campaign = new CampaignFrontState
                {
                    CampaignId = NextCampaignId(scope.State),
                    AnchorSettlementId = settlement.Id,
                };
                scope.State.Campaigns.Add(campaign);
            }

            PopulateCampaignBoard(campaign, settlement, localForce, signal, jurisdiction, isActive: true);

            scope.RecordDiff(
                $"军务沙盘 {campaign.CampaignName} 现记 {campaign.FrontLabel}，前线压力 {campaign.FrontPressure}，{campaign.SupplyStateLabel}、{campaign.MoraleStateLabel}、{campaign.CommandFitLabel}，所系之地为 {settlement.Name}。",
                campaign.CampaignId.Value.ToString());

            if (isNewCampaign || !previousActive)
            {
                scope.Emit(
                    WarfareCampaignEventNames.CampaignMobilized,
                    $"{settlement.Name}已立军务沙盘，应调之众 {campaign.MobilizedForceCount} 人。",
                    settlement.Id.Value.ToString());
            }

            if (previousFrontPressure < 60 && campaign.FrontPressure >= 60)
            {
                scope.Emit(
                    WarfareCampaignEventNames.CampaignPressureRaised,
                    $"{settlement.Name}前线压势上扬，军务案头已转紧。",
                    settlement.Id.Value.ToString());
            }

            if (previousSupplyState > 40 && campaign.SupplyState <= 40)
            {
                scope.Emit(
                    WarfareCampaignEventNames.CampaignSupplyStrained,
                    $"{settlement.Name}粮道转紧，军务沙盘已记为供运吃力。",
                    settlement.Id.Value.ToString());
            }
        }

        scope.State.MobilizationSignals = scope.State.MobilizationSignals
            .Where(signal => seenSettlements.Contains(signal.SettlementId))
            .OrderBy(static signal => signal.SettlementId.Value)
            .ToList();
        scope.State.Campaigns = scope.State.Campaigns
            .OrderBy(static campaign => campaign.CampaignId.Value)
            .ToList();
    }

    private static void UpdateMobilizationSignal(
        CampaignMobilizationSignalState signal,
        SettlementSnapshot settlement,
        LocalForcePoolSnapshot localForce,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        signal.SettlementName = settlement.Name;
        signal.ResponseActivationLevel = localForce.ResponseActivationLevel;
        signal.CommandCapacity = localForce.CommandCapacity;
        signal.Readiness = localForce.Readiness;
        signal.AvailableForceCount = localForce.GuardCount + localForce.RetainerCount + localForce.MilitiaCount + localForce.EscortCount;
        signal.OrderSupportLevel = localForce.IsResponseActivated ? localForce.OrderSupportLevel : 0;
        signal.OfficeAuthorityTier = jurisdiction?.AuthorityTier ?? 0;
        signal.AdministrativeLeverage = jurisdiction?.JurisdictionLeverage ?? 0;
        signal.PetitionBacklog = jurisdiction?.PetitionBacklog ?? 0;
        signal.CommandFitLabel = WarfareCampaignDescriptors.DetermineCommandFitLabel(
            localForce.CommandCapacity,
            localForce.ResponseActivationLevel,
            signal.OfficeAuthorityTier,
            signal.PetitionBacklog);
        ApplyDirectiveDefaults(signal, settlement.Name, localForce.HasActiveConflict || localForce.IsResponseActivated, localForce, settlement);
        signal.MobilizationWindowLabel = DetermineMobilizationWindow(localForce, signal, settlement);
        signal.OfficeCoordinationTrace = BuildOfficeCoordinationTrace(jurisdiction);
        signal.SourceTrace = BuildMobilizationSourceTrace(settlement, localForce, signal);
    }

    private static void PopulateCampaignBoard(
        CampaignFrontState campaign,
        SettlementSnapshot settlement,
        LocalForcePoolSnapshot localForce,
        CampaignMobilizationSignalState signal,
        JurisdictionAuthoritySnapshot? jurisdiction,
        bool isActive)
    {
        ApplyDirectiveDefaults(signal, settlement.Name, isActive, localForce, settlement);
        (int adjustedMobilizedForceCount, int frontPressure, int supplyState, int moraleState) = ApplyDirectiveAdjustments(
            signal.ActiveDirectiveCode,
            settlement,
            localForce,
            signal);

        campaign.AnchorSettlementName = settlement.Name;
        campaign.CampaignName = $"{settlement.Name}军务沙盘";
        campaign.IsActive = isActive;
        campaign.MobilizedForceCount = adjustedMobilizedForceCount;
        campaign.FrontPressure = frontPressure;
        campaign.FrontLabel = WarfareCampaignDescriptors.DetermineFrontLabel(frontPressure);
        campaign.SupplyState = supplyState;
        campaign.SupplyStateLabel = WarfareCampaignDescriptors.DetermineSupplyStateLabel(supplyState);
        campaign.MoraleState = moraleState;
        campaign.MoraleStateLabel = WarfareCampaignDescriptors.DetermineMoraleStateLabel(moraleState);
        campaign.CommandFitLabel = signal.CommandFitLabel;
        campaign.CommanderSummary = WarfareCampaignDescriptors.BuildCommanderSummary(settlement.Name, localForce, signal, jurisdiction);
        campaign.ActiveDirectiveCode = signal.ActiveDirectiveCode;
        campaign.ActiveDirectiveLabel = signal.ActiveDirectiveLabel;
        campaign.ActiveDirectiveSummary = signal.ActiveDirectiveSummary;
        campaign.LastDirectiveTrace = BuildDirectiveTrace(campaign, signal, settlement.Name);
        campaign.ObjectiveSummary = BuildObjectiveSummary(settlement, localForce, signal, isActive);
        campaign.MobilizationWindowLabel = signal.MobilizationWindowLabel;
        campaign.SupplyLineSummary = BuildSupplyLineSummary(settlement, localForce, signal, jurisdiction);
        campaign.OfficeCoordinationTrace = signal.OfficeCoordinationTrace;
        campaign.SourceTrace = signal.SourceTrace;
        campaign.Routes = BuildRoutes(settlement, localForce, signal, jurisdiction, frontPressure, supplyState);
        campaign.LastAftermathSummary = BuildAftermathSummary(settlement, localForce, signal, jurisdiction, campaign, isActive);
    }

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

    private sealed class WarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly WarfareCampaignState _state;

        public WarfareCampaignQueries(WarfareCampaignState state)
        {
            _state = state;
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            CampaignFrontState campaign = _state.Campaigns.Single(existing => existing.CampaignId == campaignId);
            return CloneCampaign(campaign);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _state.Campaigns
                .OrderBy(static campaign => campaign.CampaignId.Value)
                .Select(CloneCampaign)
                .ToArray();
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return _state.MobilizationSignals
                .OrderBy(static signal => signal.SettlementId.Value)
                .Select(CloneSignal)
                .ToArray();
        }

        private static CampaignFrontSnapshot CloneCampaign(CampaignFrontState campaign)
        {
            return new CampaignFrontSnapshot
            {
                CampaignId = campaign.CampaignId,
                AnchorSettlementId = campaign.AnchorSettlementId,
                AnchorSettlementName = campaign.AnchorSettlementName,
                CampaignName = campaign.CampaignName,
                IsActive = campaign.IsActive,
                ObjectiveSummary = campaign.ObjectiveSummary,
                MobilizedForceCount = campaign.MobilizedForceCount,
                FrontPressure = campaign.FrontPressure,
                FrontLabel = campaign.FrontLabel,
                SupplyState = campaign.SupplyState,
                SupplyStateLabel = campaign.SupplyStateLabel,
                MoraleState = campaign.MoraleState,
                MoraleStateLabel = campaign.MoraleStateLabel,
                CommandFitLabel = campaign.CommandFitLabel,
                CommanderSummary = campaign.CommanderSummary,
                ActiveDirectiveCode = campaign.ActiveDirectiveCode,
                ActiveDirectiveLabel = campaign.ActiveDirectiveLabel,
                ActiveDirectiveSummary = campaign.ActiveDirectiveSummary,
                LastDirectiveTrace = campaign.LastDirectiveTrace,
                MobilizationWindowLabel = campaign.MobilizationWindowLabel,
                SupplyLineSummary = campaign.SupplyLineSummary,
                OfficeCoordinationTrace = campaign.OfficeCoordinationTrace,
                SourceTrace = campaign.SourceTrace,
                LastAftermathSummary = campaign.LastAftermathSummary,
                Routes = campaign.Routes
                    .Select(static route => new CampaignRouteSnapshot
                    {
                        RouteLabel = route.RouteLabel,
                        RouteRole = route.RouteRole,
                        Pressure = route.Pressure,
                        Security = route.Security,
                        FlowStateLabel = route.FlowStateLabel,
                        Summary = route.Summary,
                    })
                    .ToArray(),
            };
        }

        private static CampaignMobilizationSignalSnapshot CloneSignal(CampaignMobilizationSignalState signal)
        {
            return new CampaignMobilizationSignalSnapshot
            {
                SettlementId = signal.SettlementId,
                SettlementName = signal.SettlementName,
                ResponseActivationLevel = signal.ResponseActivationLevel,
                CommandCapacity = signal.CommandCapacity,
                Readiness = signal.Readiness,
                AvailableForceCount = signal.AvailableForceCount,
                OrderSupportLevel = signal.OrderSupportLevel,
                OfficeAuthorityTier = signal.OfficeAuthorityTier,
                AdministrativeLeverage = signal.AdministrativeLeverage,
                PetitionBacklog = signal.PetitionBacklog,
                CommandFitLabel = signal.CommandFitLabel,
                ActiveDirectiveCode = signal.ActiveDirectiveCode,
                ActiveDirectiveLabel = signal.ActiveDirectiveLabel,
                ActiveDirectiveSummary = signal.ActiveDirectiveSummary,
                MobilizationWindowLabel = signal.MobilizationWindowLabel,
                OfficeCoordinationTrace = signal.OfficeCoordinationTrace,
                SourceTrace = signal.SourceTrace,
            };
        }
    }
}
