using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WarfareCampaign;

public sealed partial class WarfareCampaignModule : ModuleRunner<WarfareCampaignState>
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


    public override int ModuleSchemaVersion => 4;


    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;


    public override int ExecutionOrder => 750;


    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthAndSeasonal;


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

        WarfareCampaignStateProjection.BuildCampaignPhasingAndAftermath(scope.State);

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


}
