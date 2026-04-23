using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
{
    private static readonly string[] CommandNames =

    [

        "PursuePosting",

        "ResignOrRefuse",

        "PetitionViaOfficeChannels",

        "DeployAdministrativeLeverage",

    ];


    private static readonly string[] EventNames =

    [

        OfficeAndCareerEventNames.OfficeGranted,

        OfficeAndCareerEventNames.OfficeLost,

        OfficeAndCareerEventNames.OfficeTransfer,

        OfficeAndCareerEventNames.AuthorityChanged,
        OfficeAndCareerEventNames.YamenOverloaded,

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

    ];


    public override string ModuleKey => KnownModuleKeys.OfficeAndCareer;


    public override int ModuleSchemaVersion => 4;


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

    }


    public override void HandleEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)

    {

        DispatchPublicLifeEvents(scope);
        DispatchPopulationDebtEvents(scope);

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

            // Find the official with jurisdiction over this settlement.
            // For thin-chain, we pick the first appointed official.
            OfficeCareerState? career = scope.State.People.FirstOrDefault(static p => p.HasAppointment);
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
                    career.PersonId.Value.ToString());
            }
        }
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
}
