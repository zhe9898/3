using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
{
    private const int ClerkCaptureDependenceThreshold = 30;
    private const int ClerkCaptureBacklogThreshold = 15;
    private const int CourtPolicyWindowAuthorityTierThreshold = 3;
    private const int CourtPolicyWindowScoreThreshold = 60;
    private const int OfficialDefectionRiskThreshold = 80;

    private static readonly string[] CommandNames =

    [

        PlayerCommandNames.PetitionViaOfficeChannels,

        PlayerCommandNames.DeployAdministrativeLeverage,

        PlayerCommandNames.PostCountyNotice,

        PlayerCommandNames.DispatchRoadReport,

        PlayerCommandNames.PressCountyYamenDocument,

        PlayerCommandNames.RedirectRoadReport,

    ];


    private static readonly string[] EventNames =

    [

        OfficeAndCareerEventNames.OfficeGranted,

        OfficeAndCareerEventNames.OfficeLost,

        OfficeAndCareerEventNames.OfficeTransfer,

        OfficeAndCareerEventNames.AuthorityChanged,
        OfficeAndCareerEventNames.YamenOverloaded,
        // Chain 4 thin slice: imperial amnesty → disorder
        OfficeAndCareerEventNames.AmnestyApplied,
        // Chain 5 thin slice: frontier strain → supply requisition
        OfficeAndCareerEventNames.OfficialSupplyRequisition,
        // Chain 7 thin slice: clerk capture deepened
        OfficeAndCareerEventNames.ClerkCaptureDeepened,
        // Chain 8 thin slice: policy window opened
        OfficeAndCareerEventNames.PolicyWindowOpened,
        OfficeAndCareerEventNames.PolicyImplemented,
        // Chain 9 thin slice: office defected
        OfficeAndCareerEventNames.OfficeDefected,

    ];


    private static readonly string[] ConsumedEventNames =

    [

        WarfareCampaignEventNames.CampaignMobilized,

        WarfareCampaignEventNames.CampaignPressureRaised,

        WarfareCampaignEventNames.CampaignSupplyStrained,

        WarfareCampaignEventNames.CampaignAftermathRegistered,

        // Step 1b gap 3: public life → office petition backlog (no-op dispatch)
        PublicLifeAndRumorEventNames.PrefectureDispatchPressed,
        PublicLifeAndRumorEventNames.CountyGateCrowded,
        PublicLifeAndRumorEventNames.StreetTalkSurged,
        // Renzong thin chain: population debt → yamen workload
        PopulationEventNames.HouseholdDebtSpiked,
        // Chain 4 thin slice: imperial rhythm → amnesty → disorder
        WorldSettlementsEventNames.ImperialRhythmChanged,
        // Chain 5 thin slice: frontier strain → supply requisition
        WorldSettlementsEventNames.FrontierStrainEscalated,
        // Chain 8 thin slice: court agenda pressure → policy window
        WorldSettlementsEventNames.CourtAgendaPressureAccumulated,
        OfficeAndCareerEventNames.PolicyWindowOpened,
        // Chain 9 thin slice: regime legitimacy shift → office defection
        WorldSettlementsEventNames.RegimeLegitimacyShifted,

    ];


    public override string ModuleKey => KnownModuleKeys.OfficeAndCareer;


    public override int ModuleSchemaVersion => 7;


    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;


    public override int ExecutionOrder => 625;


    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;


    public override FeatureMode DefaultMode => FeatureMode.Lite;


    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;


    public override IReadOnlyCollection<string> PublishedEvents => EventNames;


    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;


    public override OfficeAndCareerState CreateInitialState()

    {

        return new OfficeAndCareerState();

    }


    public override void RegisterQueries(OfficeAndCareerState state, QueryRegistry queries)

    {

        queries.Register<IOfficeAndCareerQueries>(new OfficeQueries(state));

    }


    public override void RunXun(ModuleExecutionScope<OfficeAndCareerState> scope)

    {

        IEducationAndExamsQueries educationQueries = scope.GetRequiredQuery<IEducationAndExamsQueries>();

        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();

        IOrderAndBanditryQueries? orderQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)

            ? scope.GetRequiredQuery<IOrderAndBanditryQueries>()

            : null;

        IBlackRoutePressureQueries? blackRoutePressureQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)

            ? scope.GetRequiredQuery<IBlackRoutePressureQueries>()

            : null;

        IConflictAndForceQueries? forceQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce)

            ? scope.GetRequiredQuery<IConflictAndForceQueries>()

            : null;


        Dictionary<InstitutionId, AcademySnapshot> academies = educationQueries.GetAcademies()

            .OrderBy(static academy => academy.Id.Value)

            .ToDictionary(static academy => academy.Id, static academy => academy);

        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = orderQueries is null

            ? new Dictionary<SettlementId, SettlementDisorderSnapshot>()

            : orderQueries.GetSettlementDisorder().ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);

        Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot> blackRoutePressureBySettlement = blackRoutePressureQueries is null

            ? new Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot>()

            : blackRoutePressureQueries.GetSettlementBlackRoutePressures().ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);

        Dictionary<SettlementId, LocalForcePoolSnapshot> forceBySettlement = forceQueries is null

            ? new Dictionary<SettlementId, LocalForcePoolSnapshot>()

            : forceQueries.GetSettlementForces().ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);


        foreach (EducationCandidateSnapshot candidate in educationQueries.GetCandidates().OrderBy(static candidate => candidate.PersonId.Value))

        {

            if (!academies.TryGetValue(candidate.AcademyId, out AcademySnapshot? academy))

            {

                continue;

            }


            ClanNarrativeSnapshot narrative = socialQueries.GetRequiredClanNarrative(candidate.ClanId);

            disorderBySettlement.TryGetValue(academy.SettlementId, out SettlementDisorderSnapshot? disorder);

            blackRoutePressureBySettlement.TryGetValue(academy.SettlementId, out SettlementBlackRoutePressureSnapshot? blackRoutePressure);

            forceBySettlement.TryGetValue(academy.SettlementId, out LocalForcePoolSnapshot? force);


            OfficeCareerState career = GetOrCreateCareer(scope.State, candidate, academy.SettlementId);

            ApplyXunCareerDrift(scope.Context.CurrentXun, narrative, career, disorder, blackRoutePressure, force);

        }


        scope.State.People = scope.State.People

            .OrderBy(static person => person.PersonId.Value)

            .ToList();

        scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);

    }


    public override void RunMonth(ModuleExecutionScope<OfficeAndCareerState> scope)

    {

        IEducationAndExamsQueries educationQueries = scope.GetRequiredQuery<IEducationAndExamsQueries>();

        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();

        IFamilyCoreQueries? familyQueries = TryGetFamilyCoreQueries(scope);

        IOrderAndBanditryQueries? orderQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)

            ? scope.GetRequiredQuery<IOrderAndBanditryQueries>()

            : null;

        IBlackRoutePressureQueries? blackRoutePressureQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)

            ? scope.GetRequiredQuery<IBlackRoutePressureQueries>()

            : null;


        Dictionary<InstitutionId, AcademySnapshot> academies = educationQueries.GetAcademies()

            .OrderBy(static academy => academy.Id.Value)

            .ToDictionary(static academy => academy.Id, static academy => academy);

        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = orderQueries is null

            ? new Dictionary<SettlementId, SettlementDisorderSnapshot>()

            : orderQueries.GetSettlementDisorder().ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);

        Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot> blackRoutePressureBySettlement = blackRoutePressureQueries is null

            ? new Dictionary<SettlementId, SettlementBlackRoutePressureSnapshot>()

            : blackRoutePressureQueries.GetSettlementBlackRoutePressures().ToDictionary(static settlement => settlement.SettlementId, static settlement => settlement);


        foreach (EducationCandidateSnapshot candidate in educationQueries.GetCandidates().OrderBy(static candidate => candidate.PersonId.Value))

        {

            if (!academies.TryGetValue(candidate.AcademyId, out AcademySnapshot? academy))

            {

                continue;

            }


            ClanNarrativeSnapshot narrative = socialQueries.GetRequiredClanNarrative(candidate.ClanId);

            disorderBySettlement.TryGetValue(academy.SettlementId, out SettlementDisorderSnapshot? disorder);

            blackRoutePressureBySettlement.TryGetValue(academy.SettlementId, out SettlementBlackRoutePressureSnapshot? blackRoutePressure);

            OrderAdministrativeAftermath orderAftermath = ResolveOrderAdministrativeAftermath(disorder, blackRoutePressure);

            OfficeCareerState career = GetOrCreateCareer(scope.State, candidate, academy.SettlementId);

            UpdateCareer(scope, candidate, narrative, career, orderAftermath);

        }


        scope.State.People = scope.State.People

            .OrderBy(static person => person.PersonId.Value)

            .ToList();

        scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);

        OfficeAndCareerStateProjection.BuildOfficialPostsAndWaitingList(scope.State);

        // Chain 7 thin slice: clerk-capture escalation edge.
        HashSet<SettlementId> activeClerkCapture = scope.State.ActiveClerkCaptureSettlementIds.ToHashSet();
        List<SettlementId> currentCapturedSettlements = new();
        foreach (JurisdictionAuthorityState jurisdiction in scope.State.Jurisdictions
            .OrderBy(static j => j.SettlementId.Value))
        {
            if (jurisdiction.ClerkDependence < ClerkCaptureDependenceThreshold
                || jurisdiction.PetitionBacklog < ClerkCaptureBacklogThreshold)
            {
                continue;
            }

            currentCapturedSettlements.Add(jurisdiction.SettlementId);
            if (activeClerkCapture.Contains(jurisdiction.SettlementId))
            {
                continue;
            }

            ClerkCaptureProfile profile = ComputeClerkCaptureProfile(jurisdiction);
            scope.Emit(
                OfficeAndCareerEventNames.ClerkCaptureDeepened,
                $"{jurisdiction.LeadOfficeTitle}衙门书吏坐大，案牍渐被架空。",
                jurisdiction.SettlementId.Value.ToString(),
                new Dictionary<string, string>
                {
                    [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseClerkCapture,
                    [DomainEventMetadataKeys.SettlementId] = jurisdiction.SettlementId.Value.ToString(),
                    [DomainEventMetadataKeys.PressureScore] = profile.CapturePressure.ToString(),
                    [DomainEventMetadataKeys.ClerkDependence] = jurisdiction.ClerkDependence.ToString(),
                    [DomainEventMetadataKeys.PetitionBacklog] = jurisdiction.PetitionBacklog.ToString(),
                    [DomainEventMetadataKeys.AdministrativeTaskLoad] = jurisdiction.AdministrativeTaskLoad.ToString(),
                    [DomainEventMetadataKeys.PetitionPressure] = jurisdiction.PetitionPressure.ToString(),
                    [DomainEventMetadataKeys.AuthorityTier] = jurisdiction.AuthorityTier.ToString(),
                    [DomainEventMetadataKeys.JurisdictionLeverage] = jurisdiction.JurisdictionLeverage.ToString(),
                    [DomainEventMetadataKeys.ClerkCapturePressure] = profile.CapturePressure.ToString(),
                    [DomainEventMetadataKeys.ClerkCaptureDependencePressure] = profile.DependencePressure.ToString(),
                    [DomainEventMetadataKeys.ClerkCaptureBacklogPressure] = profile.BacklogPressure.ToString(),
                    [DomainEventMetadataKeys.ClerkCaptureTaskPressure] = profile.TaskPressure.ToString(),
                    [DomainEventMetadataKeys.ClerkCapturePetitionPressure] = profile.PetitionPressure.ToString(),
                    [DomainEventMetadataKeys.ClerkCaptureAuthorityBuffer] = profile.AuthorityBuffer.ToString(),
                });
        }

        scope.State.ActiveClerkCaptureSettlementIds = currentCapturedSettlements
            .OrderBy(static settlementId => settlementId.Value)
            .ToList();

        foreach (OfficeCareerState career in scope.State.People)
        {
            if (career.ResponseCarryoverMonths > 0)
            {
                career.ResponseCarryoverMonths = 0;
            }
        }

        ApplyPublicLifeOfficeActorCountermoves(scope, familyQueries, socialQueries);

        scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);
    }


    public override void HandleEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)

    {

        DispatchPublicLifeEvents(scope);
        DispatchPopulationDebtEvents(scope);
        DispatchImperialRhythmEvents(scope);
        DispatchFrontierStrainEvents(scope);
        DispatchCourtAgendaEvents(scope);
        DispatchPolicyWindowImplementationEvents(scope);
        DispatchRegimeEvents(scope);

        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);

        if (warfareEvents.Count == 0)

        {

            return;

        }


        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()

            .GetCampaigns()

            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);


        bool anyCareerChanged = false;

        foreach (WarfareCampaignEventBundle bundle in warfareEvents)

        {

            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))

            {

                continue;

            }


            OfficeCareerState[] careers = scope.State.People

                .Where(person => person.SettlementId == bundle.SettlementId && person.HasAppointment)

                .OrderBy(static person => person.PersonId.Value)

                .ToArray();

            if (careers.Length == 0)

            {

                continue;

            }


            int backlogIncrease = ComputeCampaignBacklogIncrease(bundle, campaign);

            int petitionPressureIncrease = ComputeCampaignPetitionPressureIncrease(bundle, campaign);

            int demotionIncrease = bundle.CampaignSupplyStrained ? 4 : bundle.CampaignAftermathRegistered ? 3 : 2;

            string wartimeTask = ResolveCampaignAdministrativeTask(bundle);


            foreach (OfficeCareerState career in careers)

            {

                career.CurrentAdministrativeTask = wartimeTask;

                career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + petitionPressureIncrease + 2, 0, 100);

                career.PetitionBacklog = Math.Clamp(career.PetitionBacklog + backlogIncrease, 0, 100);

                career.PetitionPressure = Math.Clamp(career.PetitionPressure + petitionPressureIncrease, 0, 100);

                career.DemotionPressure = Math.Clamp(career.DemotionPressure + demotionIncrease, 0, 100);

                career.PromotionMomentum = Math.Clamp(career.PromotionMomentum - (bundle.CampaignSupplyStrained ? 3 : 1), 0, 100);

                career.JurisdictionLeverage = Math.Clamp(career.JurisdictionLeverage - (bundle.CampaignSupplyStrained ? 3 : 1), 0, 100);

                career.LastPetitionOutcome = OfficeAndCareerDescriptors.FormatPetitionOutcome(

                    "Surged",

                    $"{campaign.AnchorSettlementName}战事外溢，案牍骤涌，眼下正以“{wartimeTask}”先行支应。");

                career.LastExplanation =

                    $"{career.LastExplanation} {campaign.AnchorSettlementName}战事外溢，官署人手尽转入“{wartimeTask}”；积案{career.PetitionBacklog}，词牌之压{career.PetitionPressure}，黜压{career.DemotionPressure}。";

                anyCareerChanged = true;

            }


            scope.RecordDiff(

                $"{campaign.AnchorSettlementName}战事外溢，官署积案增{backlogIncrease}，词牌之压增{petitionPressureIncrease}；诸吏先转向“{wartimeTask}”。",

                bundle.SettlementId.Value.ToString());

        }


        if (anyCareerChanged)

        {

            scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);

        }

    }


    private static void DispatchPopulationDebtEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
        // Renzong thin chain: household debt spike → yamen petition backlog.
        // Full formula (Step 3) will consider settlement capacity, official rank, etc.
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != PopulationEventNames.HouseholdDebtSpiked)
            {
                continue;
            }

            SettlementId? settlementId = ResolveSettlementMetadata(domainEvent);
            OfficeCareerState? career = SelectAppointedOfficialForDebtEvent(scope.State, settlementId);
            if (career is null)
            {
                continue;
            }

            int oldBacklog = career.PetitionBacklog;
            career.PetitionBacklog = Math.Clamp(career.PetitionBacklog + 8, 0, 100);
            career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + 5, 0, 100);

            if (oldBacklog < 60 && career.PetitionBacklog >= 60)
            {
                scope.Emit(
                    OfficeAndCareerEventNames.YamenOverloaded,
                    $"{career.OfficeTitle}衙门口因欠税纠纷挤满请减之人，案牍骤增。",
                    career.SettlementId.Value.ToString(),
                    new Dictionary<string, string>
                    {
                        [DomainEventMetadataKeys.Cause] = domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.Cause, out string? cause)
                            ? cause
                            : DomainEventMetadataValues.CauseTaxSeason,
                        [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
                        [DomainEventMetadataKeys.SettlementId] = career.SettlementId.Value.ToString(),
                        [DomainEventMetadataKeys.PersonId] = career.PersonId.Value.ToString(),
                        [DomainEventMetadataKeys.PetitionBacklog] = career.PetitionBacklog.ToString(),
                        [DomainEventMetadataKeys.AdministrativeTaskLoad] = career.AdministrativeTaskLoad.ToString(),
                    });
            }
        }
    }

    private static OfficeCareerState? SelectAppointedOfficialForDebtEvent(
        OfficeAndCareerState state,
        SettlementId? settlementId)
    {
        IEnumerable<OfficeCareerState> appointedOfficials = state.People
            .Where(static p => p.HasAppointment)
            .OrderBy(static p => p.PersonId.Value);

        return settlementId.HasValue
            ? appointedOfficials.FirstOrDefault(p => p.SettlementId == settlementId.Value)
            : appointedOfficials.FirstOrDefault();
    }

    private static SettlementId? ResolveSettlementMetadata(IDomainEvent domainEvent)
    {
        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.SettlementId, out string? rawSettlementId)
            && int.TryParse(rawSettlementId, out int settlementIdValue))
        {
            return new SettlementId(settlementIdValue);
        }

        return null;
    }

    private static void DispatchPublicLifeEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
        // Step 1b gap 3 — thin dispatch only. No state change, no Emit, no diff.
        // 维度入口：州牒 / 县门拥堵 / 街议所在聚落；本地 AuthorityTier 与现任 posts；积案深度；
        // 是否处在徭役窗口或战后恢复期；公议量级。
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case PublicLifeAndRumorEventNames.PrefectureDispatchPressed:
                case PublicLifeAndRumorEventNames.CountyGateCrowded:
                case PublicLifeAndRumorEventNames.StreetTalkSurged:
                    // TODO Step 2: 按维度入口堆积 PetitionBacklog / AuthorityChanged。
                    break;
            }
        }
    }

    private static void DispatchImperialRhythmEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
        // Chain 4 thin slice: imperial amnesty wave → office docket release → disorder reflux.
        IDomainEvent? rhythmEvent = scope.Events.FirstOrDefault(static e =>
            e.EventType == WorldSettlementsEventNames.ImperialRhythmChanged);
        if (rhythmEvent is null)
        {
            return;
        }

        IWorldSettlementsQueries worldQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        SeasonBandSnapshot season = worldQueries.GetCurrentSeason();
        int amnestyWave = season.Imperial.AmnestyWave;

        // Thin-slice threshold: amnesty wave must be strong enough to trigger
        // a tangible docket release. Full chain will use banded thresholds.
        if (amnestyWave < 50)
        {
            scope.State.LastAppliedAmnestyWave = 0;
            return;
        }

        if (amnestyWave <= scope.State.LastAppliedAmnestyWave)
        {
            return;
        }

        // Emit per jurisdiction — OfficeAndCareer owns the yamen touch-points.
        // No jurisdiction = no amnesty command reaches this settlement.
        bool emitted = false;
        foreach (JurisdictionAuthorityState jurisdiction in scope.State.Jurisdictions
            .OrderBy(static j => j.SettlementId.Value))
        {
            scope.Emit(
                OfficeAndCareerEventNames.AmnestyApplied,
                $"{jurisdiction.LeadOfficeTitle}奉赦，{jurisdiction.SettlementId.Value}地界在押人犯减等释放，案牍重理。",
                jurisdiction.SettlementId.Value.ToString(),
                BuildAmnestyAppliedMetadata(amnestyWave, jurisdiction, rhythmEvent));
            emitted = true;
        }

        if (emitted)
        {
            scope.State.LastAppliedAmnestyWave = amnestyWave;
        }
    }

    private static Dictionary<string, string> BuildAmnestyAppliedMetadata(
        int amnestyWave,
        JurisdictionAuthorityState jurisdiction,
        IDomainEvent sourceEvent)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseAmnesty,
            [DomainEventMetadataKeys.SourceEventType] = sourceEvent.EventType,
            [DomainEventMetadataKeys.SettlementId] = jurisdiction.SettlementId.Value.ToString(),
            [DomainEventMetadataKeys.AmnestyWave] = amnestyWave.ToString(),
            [DomainEventMetadataKeys.AuthorityTier] = jurisdiction.AuthorityTier.ToString(),
            [DomainEventMetadataKeys.JurisdictionLeverage] = jurisdiction.JurisdictionLeverage.ToString(),
            [DomainEventMetadataKeys.ClerkDependence] = jurisdiction.ClerkDependence.ToString(),
            [DomainEventMetadataKeys.PetitionBacklog] = jurisdiction.PetitionBacklog.ToString(),
            [DomainEventMetadataKeys.AdministrativeTaskLoad] = jurisdiction.AdministrativeTaskLoad.ToString(),
        };
    }

    private static void DispatchFrontierStrainEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
        // Chain 5 thin slice: frontier strain → official supply requisition.
        foreach (IDomainEvent domainEvent in scope.Events
            .Where(static e => e.EventType == WorldSettlementsEventNames.FrontierStrainEscalated)
            .OrderBy(static e => e.EntityKey, StringComparer.Ordinal))
        {
            if (!int.TryParse(domainEvent.EntityKey, out int settlementIdValue))
            {
                continue;
            }

            SettlementId settlementId = new(settlementIdValue);
            JurisdictionAuthorityState? jurisdiction = scope.State.Jurisdictions
                .Where(j => j.SettlementId == settlementId)
                .OrderBy(static j => j.SettlementId.Value)
                .FirstOrDefault();
            if (jurisdiction is null)
            {
                continue;
            }

            OfficialSupplyRequisitionProfile profile = ComputeOfficialSupplyRequisitionProfile(domainEvent, jurisdiction);
            ApplyOfficialSupplyOfficeLoad(scope.State, settlementId, profile);

            scope.Emit(
                OfficeAndCareerEventNames.OfficialSupplyRequisition,
                $"{jurisdiction.LeadOfficeTitle}奉边报征粮，{jurisdiction.SettlementId.Value}地界需筹军需。",
                jurisdiction.SettlementId.Value.ToString(),
                BuildOfficialSupplyRequisitionMetadata(domainEvent, jurisdiction, profile));
        }

        scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);
    }

    private static void ApplyOfficialSupplyOfficeLoad(
        OfficeAndCareerState state,
        SettlementId settlementId,
        OfficialSupplyRequisitionProfile profile)
    {
        foreach (OfficeCareerState career in state.People
            .Where(person => person.HasAppointment && person.SettlementId == settlementId)
            .OrderBy(static person => person.PersonId.Value))
        {
            career.CurrentAdministrativeTask = "frontier supply requisition";
            career.AdministrativeTaskLoad = Math.Clamp(
                career.AdministrativeTaskLoad + Math.Max(1, profile.DocketPressure / 3),
                0,
                100);
            career.PetitionBacklog = Math.Clamp(
                career.PetitionBacklog + Math.Max(1, profile.DocketPressure / 4),
                0,
                100);
            career.PetitionPressure = Math.Clamp(
                career.PetitionPressure + Math.Max(1, profile.QuotaPressure / 4),
                0,
                100);
            career.DemotionPressure = Math.Clamp(
                career.DemotionPressure + Math.Max(0, profile.ClerkDistortionPressure / 4),
                0,
                100);
            career.JurisdictionLeverage = Math.Clamp(
                career.JurisdictionLeverage - Math.Max(0, profile.SupplyPressure / 12),
                0,
                100);
            career.LastExplanation =
                $"{career.LastExplanation} Frontier supply requisition: pressure {profile.SupplyPressure}, quota {profile.QuotaPressure}, clerk distortion {profile.ClerkDistortionPressure}.";
        }
    }

    private static Dictionary<string, string> BuildOfficialSupplyRequisitionMetadata(
        IDomainEvent sourceEvent,
        JurisdictionAuthorityState jurisdiction,
        OfficialSupplyRequisitionProfile profile)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseOfficialSupply,
            [DomainEventMetadataKeys.SourceEventType] = sourceEvent.EventType,
            [DomainEventMetadataKeys.SettlementId] = jurisdiction.SettlementId.Value.ToString(),
            [DomainEventMetadataKeys.FrontierPressure] = profile.FrontierPressure.ToString(),
            [DomainEventMetadataKeys.Severity] = profile.Severity,
            [DomainEventMetadataKeys.AuthorityTier] = jurisdiction.AuthorityTier.ToString(),
            [DomainEventMetadataKeys.JurisdictionLeverage] = jurisdiction.JurisdictionLeverage.ToString(),
            [DomainEventMetadataKeys.ClerkDependence] = jurisdiction.ClerkDependence.ToString(),
            [DomainEventMetadataKeys.PetitionBacklog] = jurisdiction.PetitionBacklog.ToString(),
            [DomainEventMetadataKeys.AdministrativeTaskLoad] = jurisdiction.AdministrativeTaskLoad.ToString(),
            [DomainEventMetadataKeys.OfficialSupplyPressure] = profile.SupplyPressure.ToString(),
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = profile.QuotaPressure.ToString(),
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = profile.DocketPressure.ToString(),
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = profile.ClerkDistortionPressure.ToString(),
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = profile.AuthorityBuffer.ToString(),
        };
    }

    private static OfficialSupplyRequisitionProfile ComputeOfficialSupplyRequisitionProfile(
        IDomainEvent sourceEvent,
        JurisdictionAuthorityState jurisdiction)
    {
        int frontierPressure = ReadMetadataInt(sourceEvent, DomainEventMetadataKeys.FrontierPressure, 60);
        string severity = sourceEvent.Metadata.TryGetValue(DomainEventMetadataKeys.Severity, out string? rawSeverity)
            ? rawSeverity
            : DomainEventMetadataValues.SeverityFrontierModerate;

        int severityPressure = severity switch
        {
            DomainEventMetadataValues.SeverityFrontierSevere => 3,
            DomainEventMetadataValues.SeverityFrontierModerate => 1,
            _ => frontierPressure >= 70 ? 3 : frontierPressure >= 50 ? 1 : 0,
        };
        int quotaPressure = Math.Clamp(
            (frontierPressure / 10) + severityPressure + (jurisdiction.AdministrativeTaskLoad / 20),
            5,
            16);
        int docketPressure = Math.Clamp(
            severityPressure + (jurisdiction.AdministrativeTaskLoad / 12) + (jurisdiction.PetitionBacklog / 14),
            1,
            14);
        int clerkDistortionPressure = Math.Clamp(
            (jurisdiction.ClerkDependence / 12) + (jurisdiction.PetitionBacklog / 20),
            0,
            10);
        int authorityBuffer = Math.Clamp(
            (jurisdiction.AuthorityTier * 2) + (jurisdiction.JurisdictionLeverage / 20),
            0,
            10);
        int supplyPressure = Math.Clamp(
            quotaPressure + docketPressure + clerkDistortionPressure - authorityBuffer,
            4,
            26);

        return new OfficialSupplyRequisitionProfile(
            Math.Clamp(frontierPressure, 0, 100),
            severity,
            supplyPressure,
            quotaPressure,
            docketPressure,
            clerkDistortionPressure,
            authorityBuffer);
    }

    private static int ReadMetadataInt(IDomainEvent domainEvent, string key, int fallback)
    {
        return domainEvent.Metadata.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed)
            ? parsed
            : fallback;
    }

    private readonly record struct OfficialSupplyRequisitionProfile(
        int FrontierPressure,
        string Severity,
        int SupplyPressure,
        int QuotaPressure,
        int DocketPressure,
        int ClerkDistortionPressure,
        int AuthorityBuffer);

    private static void DispatchCourtAgendaEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
        // Chain 8 thin slice: court agenda pressure → policy window opened.
        IDomainEvent? courtAgendaPressure = scope.Events.FirstOrDefault(static e =>
            e.EventType == WorldSettlementsEventNames.CourtAgendaPressureAccumulated);
        if (courtAgendaPressure is null)
        {
            return;
        }

        int mandateConfidence = GetMandateConfidence(scope, courtAgendaPressure);
        if (mandateConfidence >= 40)
        {
            return;
        }

        JurisdictionAuthorityState? target = SelectPolicyWindowJurisdiction(scope.State.Jurisdictions, mandateConfidence);
        if (target is null)
        {
            return;
        }

        PolicyWindowProfile profile = ComputePolicyWindowProfile(target, mandateConfidence);
        scope.Emit(
            OfficeAndCareerEventNames.PolicyWindowOpened,
            $"{target.LeadOfficeTitle}辖下因朝局紧张，政策窗口忽开。",
            target.SettlementId.Value.ToString(),
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseCourt,
                [DomainEventMetadataKeys.SourceEventType] = courtAgendaPressure.EventType,
                [DomainEventMetadataKeys.SettlementId] = target.SettlementId.Value.ToString(),
                [DomainEventMetadataKeys.MandateConfidence] = mandateConfidence.ToString(),
                [DomainEventMetadataKeys.PressureScore] = profile.WindowPressure.ToString(),
                [DomainEventMetadataKeys.PolicyWindowPressure] = profile.WindowPressure.ToString(),
                [DomainEventMetadataKeys.PolicyWindowMandateDeficit] = profile.MandateDeficit.ToString(),
                [DomainEventMetadataKeys.PolicyWindowAuthoritySignal] = profile.AuthoritySignal.ToString(),
                [DomainEventMetadataKeys.PolicyWindowLeverageSignal] = profile.LeverageSignal.ToString(),
                [DomainEventMetadataKeys.PolicyWindowPetitionSignal] = profile.PetitionSignal.ToString(),
                [DomainEventMetadataKeys.PolicyWindowAdministrativeDrag] = profile.AdministrativeDrag.ToString(),
                [DomainEventMetadataKeys.PolicyWindowClerkDrag] = profile.ClerkDrag.ToString(),
                [DomainEventMetadataKeys.PolicyWindowBacklogDrag] = profile.BacklogDrag.ToString(),
                [DomainEventMetadataKeys.AuthorityTier] = target.AuthorityTier.ToString(),
                [DomainEventMetadataKeys.JurisdictionLeverage] = target.JurisdictionLeverage.ToString(),
                [DomainEventMetadataKeys.ClerkDependence] = target.ClerkDependence.ToString(),
                [DomainEventMetadataKeys.PetitionPressure] = target.PetitionPressure.ToString(),
                [DomainEventMetadataKeys.PetitionBacklog] = target.PetitionBacklog.ToString(),
                [DomainEventMetadataKeys.AdministrativeTaskLoad] = target.AdministrativeTaskLoad.ToString(),
            });
    }

    private static void DispatchRegimeEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
        // Chain 9 thin slice: regime legitimacy shift → office defection.
        IDomainEvent? regimeShift = scope.Events.FirstOrDefault(static e =>
            e.EventType == WorldSettlementsEventNames.RegimeLegitimacyShifted);
        if (regimeShift is null)
        {
            return;
        }

        int mandateConfidence = GetMandateConfidence(scope, regimeShift);
        if (mandateConfidence >= 25)
        {
            return;
        }

        List<OfficeCareerState> appointed = scope.State.People
            .Where(static p => p.HasAppointment)
            .OrderBy(static p => p.PersonId.Value)
            .ToList();
        foreach (OfficeCareerState career in appointed)
        {
            DefectionProfile profile = ComputeDefectionProfile(career, mandateConfidence);
            career.OfficialDefectionRisk = Math.Clamp(
                Math.Max(career.OfficialDefectionRisk, profile.DefectionRisk),
                0,
                100);
        }

        OfficeCareerState? defector = appointed
            .Where(static career => career.OfficialDefectionRisk >= OfficialDefectionRiskThreshold)
            .OrderByDescending(static career => career.OfficialDefectionRisk)
            .ThenByDescending(static career => career.DemotionPressure)
            .ThenBy(static career => career.PersonId.Value)
            .FirstOrDefault();
        if (defector is null)
        {
            return;
        }

        DefectionProfile defectionProfile = ComputeDefectionProfile(defector, mandateConfidence);
        defector.HasAppointment = false;
        defector.IsEligible = false;
        defector.LastOutcome = "Defected";
        defector.DemotionPressure = Math.Max(defector.DemotionPressure, defector.OfficialDefectionRisk);
        defector.LastExplanation = $"Regime legitimacy shift raised defection risk to {defector.OfficialDefectionRisk}.";

        scope.Emit(
            OfficeAndCareerEventNames.OfficeDefected,
            $"{defector.DisplayName}因天命摇动，生去就之心。",
            defector.PersonId.Value.ToString(),
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseRegime,
                [DomainEventMetadataKeys.SourceEventType] = regimeShift.EventType,
                [DomainEventMetadataKeys.PersonId] = defector.PersonId.Value.ToString(),
                [DomainEventMetadataKeys.MandateConfidence] = mandateConfidence.ToString(),
                [DomainEventMetadataKeys.DefectionRisk] = defector.OfficialDefectionRisk.ToString(),
                [DomainEventMetadataKeys.DefectionBaselinePressure] = defectionProfile.BaselinePressure.ToString(),
                [DomainEventMetadataKeys.DefectionMandateDeficit] = defectionProfile.MandateDeficit.ToString(),
                [DomainEventMetadataKeys.DefectionDemotionPressure] = defectionProfile.DemotionPressure.ToString(),
                [DomainEventMetadataKeys.DefectionClerkPressure] = defectionProfile.ClerkPressure.ToString(),
                [DomainEventMetadataKeys.DefectionPetitionPressure] = defectionProfile.PetitionPressure.ToString(),
                [DomainEventMetadataKeys.DefectionReputationStrain] = defectionProfile.ReputationStrain.ToString(),
                [DomainEventMetadataKeys.DefectionAuthorityBuffer] = defectionProfile.AuthorityBuffer.ToString(),
                [DomainEventMetadataKeys.AuthorityTier] = defector.AuthorityTier.ToString(),
                [DomainEventMetadataKeys.ClerkDependence] = defector.ClerkDependence.ToString(),
                [DomainEventMetadataKeys.PetitionPressure] = defector.PetitionPressure.ToString(),
            });

        scope.State.People = scope.State.People
            .OrderBy(static person => person.PersonId.Value)
            .ToList();
        scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);
        OfficeAndCareerStateProjection.BuildOfficialPostsAndWaitingList(scope.State);
    }

    private static int ComputeClerkCapturePressure(JurisdictionAuthorityState jurisdiction)
    {
        return ComputeClerkCaptureProfile(jurisdiction).CapturePressure;
    }

    private static ClerkCaptureProfile ComputeClerkCaptureProfile(JurisdictionAuthorityState jurisdiction)
    {
        int dependencePressure = jurisdiction.ClerkDependence;
        int backlogPressure = jurisdiction.PetitionBacklog / 2;
        int taskPressure = jurisdiction.AdministrativeTaskLoad / 3;
        int petitionPressure = jurisdiction.PetitionPressure / 4;
        int authorityBuffer = (jurisdiction.AuthorityTier * 3) + (jurisdiction.JurisdictionLeverage / 10);
        int capturePressure = Math.Clamp(
            dependencePressure + backlogPressure + taskPressure + petitionPressure - authorityBuffer,
            0,
            100);

        return new ClerkCaptureProfile(
            capturePressure,
            dependencePressure,
            backlogPressure,
            taskPressure,
            petitionPressure,
            authorityBuffer);
    }

    private static JurisdictionAuthorityState? SelectPolicyWindowJurisdiction(
        IReadOnlyList<JurisdictionAuthorityState> jurisdictions,
        int mandateConfidence)
    {
        return jurisdictions
            .Select(jurisdiction => new
            {
                Jurisdiction = jurisdiction,
                Profile = ComputePolicyWindowProfile(jurisdiction, mandateConfidence),
            })
            .Where(candidate => candidate.Jurisdiction.AuthorityTier >= CourtPolicyWindowAuthorityTierThreshold
                                && candidate.Profile.WindowPressure >= CourtPolicyWindowScoreThreshold)
            .OrderByDescending(static candidate => candidate.Profile.WindowPressure)
            .ThenByDescending(static candidate => candidate.Jurisdiction.AuthorityTier)
            .ThenByDescending(static candidate => candidate.Jurisdiction.JurisdictionLeverage)
            .ThenBy(static candidate => candidate.Jurisdiction.SettlementId.Value)
            .Select(static candidate => candidate.Jurisdiction)
            .FirstOrDefault();
    }

    private static int ComputePolicyWindowScore(JurisdictionAuthorityState jurisdiction, int mandateConfidence)
    {
        return ComputePolicyWindowProfile(jurisdiction, mandateConfidence).WindowPressure;
    }

    private static PolicyWindowProfile ComputePolicyWindowProfile(
        JurisdictionAuthorityState jurisdiction,
        int mandateConfidence)
    {
        int mandateDeficit = Math.Max(0, 40 - mandateConfidence);
        int authoritySignal = jurisdiction.AuthorityTier * 18;
        int leverageSignal = jurisdiction.JurisdictionLeverage / 3;
        int petitionSignal = jurisdiction.PetitionPressure / 5;
        int administrativeDrag = jurisdiction.AdministrativeTaskLoad / 4;
        int clerkDrag = jurisdiction.ClerkDependence / 5;
        int backlogDrag = jurisdiction.PetitionBacklog / 6;
        int windowPressure = Math.Clamp(
            authoritySignal
            + leverageSignal
            + petitionSignal
            + mandateDeficit
            - administrativeDrag
            - clerkDrag
            - backlogDrag,
            0,
            100);

        return new PolicyWindowProfile(
            windowPressure,
            mandateDeficit,
            authoritySignal,
            leverageSignal,
            petitionSignal,
            administrativeDrag,
            clerkDrag,
            backlogDrag);
    }

    private static int ComputeDefectionRisk(OfficeCareerState career, int mandateConfidence)
    {
        return ComputeDefectionProfile(career, mandateConfidence).DefectionRisk;
    }

    private static DefectionProfile ComputeDefectionProfile(OfficeCareerState career, int mandateConfidence)
    {
        int mandateDeficit = Math.Max(0, 25 - mandateConfidence);
        const int baselinePressure = 35;
        int demotionPressure = career.DemotionPressure / 2;
        int clerkPressure = career.ClerkDependence / 3;
        int petitionPressure = career.PetitionPressure / 4;
        int reputationStrain = Math.Max(0, 40 - career.OfficeReputation);
        int reputationPressure = reputationStrain / 2;
        int authorityBuffer = (career.AuthorityTier * 4) + (Math.Max(0, career.OfficeReputation - 60) / 5);
        int defectionRisk = Math.Clamp(
            baselinePressure
            + mandateDeficit
            + demotionPressure
            + clerkPressure
            + petitionPressure
            + reputationPressure
            - authorityBuffer,
            0,
            100);

        return new DefectionProfile(
            defectionRisk,
            baselinePressure,
            mandateDeficit,
            demotionPressure,
            clerkPressure,
            petitionPressure,
            reputationPressure,
            authorityBuffer);
    }

    private static int GetMandateConfidence(ModuleEventHandlingScope<OfficeAndCareerState> scope, IDomainEvent domainEvent)
    {
        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.MandateConfidence, out string? raw)
            && int.TryParse(raw, out int mandateConfidence))
        {
            return mandateConfidence;
        }

        IWorldSettlementsQueries worldQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        return worldQueries.GetCurrentSeason().Imperial.MandateConfidence;
    }

    private readonly record struct ClerkCaptureProfile(
        int CapturePressure,
        int DependencePressure,
        int BacklogPressure,
        int TaskPressure,
        int PetitionPressure,
        int AuthorityBuffer);

    private readonly record struct PolicyWindowProfile(
        int WindowPressure,
        int MandateDeficit,
        int AuthoritySignal,
        int LeverageSignal,
        int PetitionSignal,
        int AdministrativeDrag,
        int ClerkDrag,
        int BacklogDrag);

    private readonly record struct DefectionProfile(
        int DefectionRisk,
        int BaselinePressure,
        int MandateDeficit,
        int DemotionPressure,
        int ClerkPressure,
        int PetitionPressure,
        int ReputationStrain,
        int AuthorityBuffer);
}
