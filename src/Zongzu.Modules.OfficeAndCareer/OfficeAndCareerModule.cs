using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
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
        "OfficeGranted",
        "OfficeLost",
        "OfficeTransfer",
        "AuthorityChanged",
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.OfficeAndCareer;

    public override int ModuleSchemaVersion => 3;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 625;

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

    public override void RunMonth(ModuleExecutionScope<OfficeAndCareerState> scope)
    {
        IEducationAndExamsQueries educationQueries = scope.GetRequiredQuery<IEducationAndExamsQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();

        Dictionary<InstitutionId, AcademySnapshot> academies = educationQueries.GetAcademies()
            .OrderBy(static academy => academy.Id.Value)
            .ToDictionary(static academy => academy.Id, static academy => academy);

        foreach (EducationCandidateSnapshot candidate in educationQueries.GetCandidates().OrderBy(static candidate => candidate.PersonId.Value))
        {
            if (!academies.TryGetValue(candidate.AcademyId, out AcademySnapshot? academy))
            {
                continue;
            }

            ClanNarrativeSnapshot narrative = socialQueries.GetRequiredClanNarrative(candidate.ClanId);
            OfficeCareerState career = GetOrCreateCareer(scope.State, candidate, academy.SettlementId);
            UpdateCareer(scope, candidate, narrative, career);
        }

        scope.State.People = scope.State.People
            .OrderBy(static person => person.PersonId.Value)
            .ToList();
        scope.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(scope.State.People);
    }

    public override void HandleEvents(ModuleEventHandlingScope<OfficeAndCareerState> scope)
    {
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

    private static void UpdateCareer(
        ModuleExecutionScope<OfficeAndCareerState> scope,
        EducationCandidateSnapshot candidate,
        ClanNarrativeSnapshot narrative,
        OfficeCareerState career)
    {
        career.DisplayName = candidate.DisplayName;
        career.ClanId = candidate.ClanId;

        int scholarlySignal = candidate.ScholarlyReputation
            + (candidate.HasPassedLocalExam ? 16 : 0)
            - Math.Max(0, candidate.ExamAttempts - 1);
        int reputationSignal = scholarlySignal + (narrative.FavorBalance / 2) - (narrative.ShamePressure / 5);
        int leverageBase = 18
            + scholarlySignal
            + narrative.FavorBalance
            - (narrative.ShamePressure / 3)
            - (narrative.GrudgePressure / 6)
            + scope.Context.Random.NextInt(-2, 3);
        int petitionBase = 14
            + (narrative.FearPressure / 2)
            + (narrative.GrudgePressure / 3)
            + (candidate.Stress / 4)
            - (narrative.FavorBalance / 3)
            + scope.Context.Random.NextInt(-1, 2);
        int appointmentBase = 9
            + (candidate.HasPassedLocalExam ? 12 : 0)
            + (scholarlySignal / 4)
            + (narrative.FavorBalance / 3)
            - (narrative.ShamePressure / 5)
            - (candidate.Stress / 6)
            + scope.Context.Random.NextInt(-1, 2);
        int clerkBase = 8
            + (candidate.HasPassedLocalExam ? 5 : 0)
            + (petitionBase / 6)
            + (narrative.FearPressure / 6)
            - (narrative.FavorBalance / 8)
            + scope.Context.Random.NextInt(-1, 2);

        career.IsEligible = candidate.HasPassedLocalExam && reputationSignal >= 16;

        if (!career.HasAppointment)
        {
            UpdatePreAppointmentCareer(
                scope,
                candidate,
                narrative,
                career,
                scholarlySignal,
                reputationSignal,
                leverageBase,
                petitionBase,
                appointmentBase,
                clerkBase);
            return;
        }

        UpdateServingCareer(scope, candidate, narrative, career, reputationSignal, leverageBase, petitionBase);
    }

    private static void GrantInitialAppointment(
        ModuleExecutionScope<OfficeAndCareerState> scope,
        EducationCandidateSnapshot candidate,
        ClanNarrativeSnapshot narrative,
        OfficeCareerState career,
        int scholarlySignal,
        int reputationSignal,
        int leverageBase,
        int petitionBase)
    {
        int authorityTier = Math.Max(1, DetermineAuthorityTier(reputationSignal, narrative, 0, 0, 0));
        AdministrativeResolution resolution = ResolveAdministrativeWork(
            candidate,
            narrative,
            career,
            authorityTier,
            reputationSignal,
            petitionBase);

        career.HasAppointment = true;
        career.OfficeTitle = ResolveNorthernSongOfficeTitle(authorityTier);
        career.AuthorityTier = authorityTier;
        career.AppointmentPressure = Math.Max(career.AppointmentPressure, 48);
        career.ClerkDependence = Math.Clamp(
            Math.Max(career.ClerkDependence, 18 + (resolution.TaskLoad / 2) + (resolution.PetitionBacklog / 5)),
            0,
            100);
        career.ServiceMonths = 1;
        career.PromotionMomentum = Math.Clamp(6 + (scholarlySignal / 3) + (narrative.FavorBalance / 2) - (narrative.ShamePressure / 5), 0, 100);
        career.DemotionPressure = Math.Clamp(Math.Max(0, resolution.PetitionBacklog / 4 + (narrative.ShamePressure / 5) - (narrative.FavorBalance / 6)), 0, 100);
        career.CurrentAdministrativeTask = resolution.TaskName;
        career.AdministrativeTaskLoad = resolution.TaskLoad;
        career.PetitionBacklog = resolution.PetitionBacklog;
        career.LastPetitionOutcome = OfficeAndCareerDescriptors.FormatPetitionOutcome(
            "Queued",
            "新任甫定，词牍先行收案。");
        career.OfficeReputation = Math.Clamp(reputationSignal + 8, 0, 100);
        career.JurisdictionLeverage = Math.Clamp(leverageBase + (authorityTier * 7) + (career.PromotionMomentum / 4), 0, 100);
        career.PetitionPressure = Math.Clamp(petitionBase + (career.PetitionBacklog / 3) - (authorityTier * 2), 0, 100);
        career.LastOutcome = "Granted";
        string taskTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(career.CurrentAdministrativeTask, authorityTier);
        career.LastExplanation =
            $"因场屋得捷、名望{scholarlySignal}与人情{narrative.FavorBalance}，得授{career.OfficeTitle}，首任{taskTier}差遣为“{career.CurrentAdministrativeTask}”。" +
            OfficeAndCareerDescriptors.BuildAuthorityTrajectorySummary(career);

        scope.RecordDiff(
            $"{candidate.DisplayName}得授{career.OfficeTitle}，乡面杖力{career.JurisdictionLeverage}，词牍压{career.PetitionPressure}，积案{career.PetitionBacklog}，首任{taskTier}差遣为{career.CurrentAdministrativeTask}。",
            candidate.PersonId.Value.ToString());
        scope.Emit("OfficeGranted", $"{candidate.DisplayName}得授{career.OfficeTitle}。");
    }

    private static void UpdatePreAppointmentCareer(
        ModuleExecutionScope<OfficeAndCareerState> scope,
        EducationCandidateSnapshot candidate,
        ClanNarrativeSnapshot narrative,
        OfficeCareerState career,
        int scholarlySignal,
        int reputationSignal,
        int leverageBase,
        int petitionBase,
        int appointmentBase,
        int clerkBase)
    {
        if (!career.IsEligible)
        {
            UpdateUnappointedCareer(candidate, career, petitionBase, reputationSignal);
            return;
        }

        career.OfficeTitle = "未授官";
        career.AuthorityTier = 0;
        career.OfficeReputation = Math.Clamp(Math.Max(career.OfficeReputation, reputationSignal), 0, 100);
        career.AppointmentPressure = Math.Clamp(
            Math.Max(career.AppointmentPressure, 10)
            + appointmentBase
            + (string.Equals(career.LastOutcome, "候缺", StringComparison.Ordinal) ? 4 : 0),
            0,
            100);
        career.ClerkDependence = Math.Clamp(
            Math.Max(career.ClerkDependence, 6)
            + clerkBase
            + (career.AppointmentPressure >= 28 ? 3 : 0),
            0,
            100);
        career.JurisdictionLeverage = 0;
        career.PetitionPressure = Math.Clamp(Math.Max(6, petitionBase / 2), 0, 100);
        career.PetitionBacklog = Math.Clamp(Math.Max(0, petitionBase / 4 - 2), 0, 100);

        bool attachedToYamen = career.AppointmentPressure >= 26 || narrative.FavorBalance >= 18;
        bool readyForAppointment =
            career.AppointmentPressure >= 48
            || (career.AppointmentPressure >= 40 && narrative.FavorBalance >= 16)
            || (career.AppointmentPressure >= 34 && attachedToYamen && reputationSignal >= 28);

        career.CurrentAdministrativeTask = ResolvePreAppointmentTask(career.AppointmentPressure, attachedToYamen);
        career.AdministrativeTaskLoad = attachedToYamen
            ? Math.Clamp(6 + (career.ClerkDependence / 4) + (candidate.Stress / 8), 0, 100)
            : 0;
        career.PromotionMomentum = Math.Max(0, career.PromotionMomentum - 1);
        career.DemotionPressure = Math.Max(0, career.DemotionPressure - 1);

        if (readyForAppointment)
        {
            GrantInitialAppointment(scope, candidate, narrative, career, scholarlySignal, reputationSignal, leverageBase, petitionBase);
            return;
        }

        career.LastOutcome = attachedToYamen ? "听差" : "候缺";
        career.LastPetitionOutcome = OfficeAndCareerDescriptors.FormatPetitionOutcome(
            attachedToYamen ? "Queued" : "Unavailable",
            attachedToYamen
                ? "场屋已捷，先在县署随案听差，抄录收牒。"
                : "阙次未至，词状暂未得由本人经手。");
        career.LastExplanation = attachedToYamen
            ? $"场屋已捷，但阙次未到、人情未固，暂在县署随案听差；荐引势{career.AppointmentPressure}，吏案依赖{career.ClerkDependence}。"
            : $"场屋已捷，然阙次未至、人情未定，暂在守选候阙；荐引势{career.AppointmentPressure}，学压{candidate.Stress}。";
    }

    private static void UpdateUnappointedCareer(
        EducationCandidateSnapshot candidate,
        OfficeCareerState career,
        int petitionBase,
        int reputationSignal)
    {
        career.OfficeTitle = "未授官";
        career.AuthorityTier = 0;
        career.OfficeReputation = Math.Clamp(Math.Max(career.OfficeReputation, reputationSignal), 0, 100);
        career.AppointmentPressure = Math.Max(0, career.AppointmentPressure - 2);
        career.ClerkDependence = Math.Max(0, career.ClerkDependence - 2);
        career.JurisdictionLeverage = 0;
        career.PetitionPressure = Math.Clamp(petitionBase, 0, 100);
        career.PetitionBacklog = 0;
        career.CurrentAdministrativeTask = "候补听选";
        career.AdministrativeTaskLoad = 0;
        career.PromotionMomentum = Math.Max(0, career.PromotionMomentum - 1);
        career.DemotionPressure = Math.Max(0, career.DemotionPressure - 1);
        career.LastOutcome = career.IsEligible ? "候缺" : "观望";
        career.LastPetitionOutcome = OfficeAndCareerDescriptors.FormatPetitionOutcome(
            "Unavailable",
            "未得官身，词状不得入案。");
        career.LastExplanation = career.IsEligible
            ? $"已可入途，但人情未定、学压{candidate.Stress}，暂在候缺。"
            : "声望与场屋资历未足，暂未得官。";
    }

    private static void UpdateServingCareer(
        ModuleExecutionScope<OfficeAndCareerState> scope,
        EducationCandidateSnapshot candidate,
        ClanNarrativeSnapshot narrative,
        OfficeCareerState career,
        int reputationSignal,
        int leverageBase,
        int petitionBase)
    {
        int previousTier = career.AuthorityTier;
        string previousTitle = career.OfficeTitle;
        int nextServiceMonth = Math.Max(1, career.ServiceMonths + 1);

        AdministrativeResolution resolution = ResolveAdministrativeWork(
            candidate,
            narrative,
            career,
            Math.Max(1, previousTier),
            reputationSignal,
            petitionBase);

        career.ServiceMonths = nextServiceMonth;
        career.AppointmentPressure = Math.Max(career.AppointmentPressure, 48);
        career.ClerkDependence = Math.Clamp(
            Math.Max(career.ClerkDependence, 12 + (previousTier * 4))
            + Math.Max(0, resolution.TaskLoad - 12) / 3
            + (resolution.PetitionBacklog / 12)
            - (narrative.FavorBalance / 10),
            0,
            100);
        career.CurrentAdministrativeTask = resolution.TaskName;
        career.AdministrativeTaskLoad = resolution.TaskLoad;
        career.PetitionBacklog = resolution.PetitionBacklog;
        career.LastPetitionOutcome = resolution.PetitionOutcome;
        career.PromotionMomentum = Math.Clamp(
            Math.Max(career.PromotionMomentum, previousTier * 6)
            + resolution.PromotionMomentumAdjustment
            + (career.ServiceMonths >= 12 ? 2 : 0),
            0,
            100);
        career.DemotionPressure = Math.Clamp(
            Math.Max(career.DemotionPressure, Math.Max(0, career.PetitionPressure / 4))
            + resolution.DemotionPressureAdjustment
            + Math.Max(0, career.ClerkDependence - 42) / 5
            + (narrative.ShamePressure >= 32 ? 2 : 0),
            0,
            100);

        int updatedTier = DetermineAuthorityTier(
            reputationSignal,
            narrative,
            career.PromotionMomentum,
            career.DemotionPressure,
            career.ServiceMonths);

        if (career.DemotionPressure >= 45 && career.PetitionBacklog >= 40)
        {
            updatedTier = Math.Max(1, updatedTier - 1);
        }

        bool losesAppointment = updatedTier == 0
            || (career.DemotionPressure >= 68
                && career.PetitionBacklog >= 55
                && narrative.FavorBalance <= 6);

        if (losesAppointment)
        {
            career.HasAppointment = false;
            career.OfficeTitle = "未授官";
            career.AuthorityTier = 0;
            career.AppointmentPressure = Math.Max(career.AppointmentPressure, 20);
            career.ClerkDependence = Math.Clamp(career.ClerkDependence + 6, 0, 100);
            career.JurisdictionLeverage = 0;
            career.PetitionPressure = 0;
            career.PetitionBacklog = 0;
            career.CurrentAdministrativeTask = "候补听选";
            career.AdministrativeTaskLoad = 0;
            career.OfficeReputation = Math.Clamp(Math.Max(reputationSignal - 4, 0), 0, 100);
            career.LastOutcome = "Lost";
            career.LastPetitionOutcome = OfficeAndCareerDescriptors.FormatPetitionOutcome(
                "Stalled",
                $"积案壅滞，公信先败于{previousTitle}任上。");
            career.LastExplanation =
                $"积案{resolution.PetitionBacklog}、羞压{narrative.ShamePressure}与人情{narrative.FavorBalance}俱失，地方公信尽散，遂失官身。";

            scope.RecordDiff(
                $"{candidate.DisplayName}因积案壅滞、杖力尽失而罢官；积案{resolution.PetitionBacklog}，羞压{narrative.ShamePressure}，人情{narrative.FavorBalance}。",
                candidate.PersonId.Value.ToString());
            scope.Emit("OfficeLost", $"{candidate.DisplayName}因案牍壅滞而失官。");
            return;
        }

        career.AuthorityTier = updatedTier;
        career.OfficeTitle = ResolveNorthernSongOfficeTitle(updatedTier);
        career.OfficeReputation = Math.Clamp(
            Math.Max(career.OfficeReputation, reputationSignal) + resolution.OfficeReputationAdjustment,
            0,
            100);
        career.JurisdictionLeverage = Math.Clamp(
            leverageBase
            + (updatedTier * 8)
            + (career.PromotionMomentum / 4)
            + resolution.LeverageAdjustment
            - Math.Max(0, career.ClerkDependence - 36) / 4
            - (career.DemotionPressure / 6),
            0,
            100);
        career.PetitionPressure = Math.Clamp(
            petitionBase
            + resolution.PetitionPressureAdjustment
            + (career.PetitionBacklog / 3)
            - updatedTier,
            0,
            100);

        if (updatedTier > previousTier)
        {
            career.LastOutcome = "Promoted";
        }
        else if (updatedTier < previousTier)
        {
            career.LastOutcome = "Demoted";
        }
        else
        {
            career.LastOutcome = "Serving";
        }

        string taskTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(career.CurrentAdministrativeTask, updatedTier);
        string authorityTrace = OfficeAndCareerDescriptors.BuildAuthorityShiftTrace(
            candidate.DisplayName,
            career.LastOutcome,
            career.OfficeTitle,
            taskTier,
            career.PromotionMomentum,
            career.DemotionPressure,
            career.PetitionBacklog);
        career.LastExplanation =
            $"{authorityTrace} {career.LastPetitionOutcome}";

        scope.RecordDiff(
            $"{candidate.DisplayName}今为{career.OfficeTitle}，乡面杖力{career.JurisdictionLeverage}，词牍压{career.PetitionPressure}，积案{career.PetitionBacklog}，所办{taskTier}差遣为{career.CurrentAdministrativeTask}。",
            candidate.PersonId.Value.ToString());

        if (updatedTier != previousTier)
        {
            scope.Emit("AuthorityChanged", $"{candidate.DisplayName}官阶改为第{updatedTier}等。");
        }

        if (!string.Equals(previousTitle, career.OfficeTitle, StringComparison.Ordinal))
        {
            scope.Emit("OfficeTransfer", $"{candidate.DisplayName}由{previousTitle}转为{career.OfficeTitle}。");
        }
    }

    private static OfficeCareerState GetOrCreateCareer(
        OfficeAndCareerState state,
        EducationCandidateSnapshot candidate,
        SettlementId settlementId)
    {
        OfficeCareerState? career = state.People.SingleOrDefault(person => person.PersonId == candidate.PersonId);
        if (career is not null)
        {
            career.SettlementId = settlementId;
            return career;
        }

        career = new OfficeCareerState
        {
            PersonId = candidate.PersonId,
            ClanId = candidate.ClanId,
            SettlementId = settlementId,
            DisplayName = candidate.DisplayName,
        };
        state.People.Add(career);
        return career;
    }

    private static string ResolvePreAppointmentTask(int appointmentPressure, bool attachedToYamen)
    {
        if (attachedToYamen && appointmentPressure >= 38)
        {
            return "随案听差";
        }

        if (appointmentPressure >= 28)
        {
            return "投牒候差";
        }

        return "守选候阙";
    }

    private static string ResolveNorthernSongOfficeTitle(int authorityTier)
    {
        return authorityTier switch
        {
            >= 3 => "县丞",
            2 => "主簿",
            _ => "簿佐",
        };
    }

    private static int DetermineAuthorityTier(
        int reputationSignal,
        ClanNarrativeSnapshot narrative,
        int promotionMomentum,
        int demotionPressure,
        int serviceMonths)
    {
        int signal = reputationSignal
            + (narrative.FavorBalance / 2)
            - (narrative.ShamePressure / 6)
            + (promotionMomentum / 3)
            - (demotionPressure / 4)
            + (Math.Min(serviceMonths, 18) / 3);

        if (signal >= 58)
        {
            return 3;
        }

        if (signal >= 36)
        {
            return 2;
        }

        if (signal >= 18)
        {
            return 1;
        }

        return 0;
    }

    private static int ComputeCampaignBacklogIncrease(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int increase = bundle.CampaignMobilized ? 2 : 0;
        increase += bundle.CampaignPressureRaised ? 3 : 0;
        increase += bundle.CampaignSupplyStrained ? 6 : 0;
        increase += bundle.CampaignAftermathRegistered ? 4 : 0;
        increase += Math.Max(0, campaign.FrontPressure - 55) / 16;
        return Math.Max(1, increase);
    }

    private static int ComputeCampaignPetitionPressureIncrease(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int increase = bundle.CampaignPressureRaised ? 3 : 0;
        increase += bundle.CampaignSupplyStrained ? 4 : 0;
        increase += bundle.CampaignAftermathRegistered ? 2 : 0;
        increase += Math.Max(0, 50 - campaign.SupplyState) / 12;
        increase += Math.Max(0, 45 - campaign.MoraleState) / 15;
        return Math.Max(1, increase);
    }

    private static string ResolveCampaignAdministrativeTask(WarfareCampaignEventBundle bundle)
    {
        if (bundle.CampaignSupplyStrained)
        {
            return "急牍覆核";
        }

        if (bundle.CampaignAftermathRegistered)
        {
            return "勘理词状";
        }

        if (bundle.CampaignPressureRaised)
        {
            return "巡丁清点";
        }

        return "勾检户籍";
    }

    private static string ResolveOfficeTitle(int authorityTier)
    {
        return authorityTier switch
        {
            >= 3 => "县丞",
            2 => "主簿",
            _ => "书吏",
        };
    }

    private static AdministrativeResolution ResolveAdministrativeWork(
        EducationCandidateSnapshot candidate,
        ClanNarrativeSnapshot narrative,
        OfficeCareerState career,
        int authorityTier,
        int reputationSignal,
        int petitionBase)
    {
        AdministrativeTaskPlan taskPlan = DetermineAdministrativeTaskPlan(authorityTier, petitionBase, narrative);
        int taskLoad = Math.Clamp(
            8
            + (petitionBase / 3)
            + (narrative.GrudgePressure / 6)
            + (narrative.FearPressure / 7)
            + (candidate.Stress / 5)
            - (authorityTier * 2),
            0,
            100);
        int petitionBacklog = Math.Clamp(
            (petitionBase / 2)
            + (narrative.FearPressure / 5)
            + (narrative.GrudgePressure / 4)
            + (candidate.Stress / 5)
            - (authorityTier * 3),
            0,
            100);
        int administrativeScore = reputationSignal
            + (career.OfficeReputation / 3)
            + (authorityTier * 8)
            + (narrative.FavorBalance / 2)
            - (narrative.ShamePressure / 5)
            - taskLoad
            - (petitionBacklog / 2)
            - (candidate.Stress / 3);

        if (administrativeScore >= 48)
        {
            return new AdministrativeResolution(
                taskPlan.TaskName,
                taskLoad,
                Math.Max(0, petitionBacklog - 14),
                OfficeAndCareerDescriptors.FormatPetitionOutcome("Cleared", $"今以{taskPlan.TaskName}清理诸状，积案已消。"),
                -10,
                4,
                4,
                -6,
                2);
        }

        if (administrativeScore >= 32)
        {
            return new AdministrativeResolution(
                taskPlan.TaskName,
                taskLoad,
                Math.Max(0, petitionBacklog - 6),
                OfficeAndCareerDescriptors.FormatPetitionOutcome("Triaged", $"正在{taskPlan.TaskName}，诸状分轻重收理。"),
                -4,
                2,
                2,
                -2,
                1);
        }

        if (administrativeScore >= 18)
        {
            return new AdministrativeResolution(
                taskPlan.TaskName,
                taskLoad,
                Math.Clamp(petitionBacklog + 6, 0, 100),
                OfficeAndCareerDescriptors.FormatPetitionOutcome("Delayed", $"正在{taskPlan.TaskName}，其余词牍暂缓。"),
                4,
                0,
                0,
                4,
                0);
        }

        return new AdministrativeResolution(
            taskPlan.TaskName,
            taskLoad,
            Math.Clamp(petitionBacklog + 12, 0, 100),
            OfficeAndCareerDescriptors.FormatPetitionOutcome("Stalled", $"因{taskPlan.TaskName}不济，积案壅滞，怨词渐深。"),
            8,
            -2,
            -2,
            8,
            -1);
    }

    private static AdministrativeTaskPlan DetermineAdministrativeTaskPlan(int authorityTier, int petitionPressure, ClanNarrativeSnapshot narrative)
    {
        if (authorityTier >= 3 && narrative.GrudgePressure >= 45)
        {
            return new AdministrativeTaskPlan("crisis", "勘解乡怨词牍");
        }

        if (authorityTier >= 3 && petitionPressure >= 55)
        {
            return new AdministrativeTaskPlan("crisis", "急牍覆核");
        }

        if (authorityTier >= 2 && narrative.FearPressure >= 45)
        {
            return new AdministrativeTaskPlan("district", "巡丁清点");
        }

        if (authorityTier >= 2 && petitionPressure >= 45)
        {
            return new AdministrativeTaskPlan("district", "勘理词状");
        }

        if (authorityTier >= 2)
        {
            return new AdministrativeTaskPlan("registry", "勾检户籍");
        }

        if (petitionPressure >= 45)
        {
            return new AdministrativeTaskPlan("clerical", "誊录词牍");
        }

        return new AdministrativeTaskPlan("clerical", "誊黄封牍");
    }

    private sealed class OfficeQueries : IOfficeAndCareerQueries
    {
        private readonly OfficeAndCareerState _state;

        public OfficeQueries(OfficeAndCareerState state)
        {
            _state = state;
        }

        public OfficeCareerSnapshot GetRequiredCareer(PersonId personId)
        {
            OfficeCareerState career = _state.People.Single(person => person.PersonId == personId);
            return CloneCareer(career);
        }

        public IReadOnlyList<OfficeCareerSnapshot> GetCareers()
        {
            return _state.People
                .OrderBy(static person => person.PersonId.Value)
                .Select(CloneCareer)
                .ToArray();
        }

        public JurisdictionAuthoritySnapshot GetRequiredJurisdiction(SettlementId settlementId)
        {
            JurisdictionAuthorityState jurisdiction = _state.Jurisdictions.Single(authority => authority.SettlementId == settlementId);
            return CloneJurisdiction(jurisdiction);
        }

        public IReadOnlyList<JurisdictionAuthoritySnapshot> GetJurisdictions()
        {
            return _state.Jurisdictions
                .OrderBy(static authority => authority.SettlementId.Value)
                .Select(CloneJurisdiction)
                .ToArray();
        }

        private static OfficeCareerSnapshot CloneCareer(OfficeCareerState career)
        {
            return new OfficeCareerSnapshot
            {
                PersonId = career.PersonId,
                ClanId = career.ClanId,
                SettlementId = career.SettlementId,
                DisplayName = career.DisplayName,
                IsEligible = career.IsEligible,
                HasAppointment = career.HasAppointment,
                OfficeTitle = career.OfficeTitle,
                AuthorityTier = career.AuthorityTier,
                AppointmentPressure = career.AppointmentPressure,
                ClerkDependence = career.ClerkDependence,
                JurisdictionLeverage = career.JurisdictionLeverage,
                PetitionPressure = career.PetitionPressure,
                PetitionBacklog = career.PetitionBacklog,
                ServiceMonths = career.ServiceMonths,
                PromotionMomentum = career.PromotionMomentum,
                DemotionPressure = career.DemotionPressure,
                CurrentAdministrativeTask = career.CurrentAdministrativeTask,
                AdministrativeTaskTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(career.CurrentAdministrativeTask, career.AuthorityTier),
                AdministrativeTaskLoad = career.AdministrativeTaskLoad,
                OfficeReputation = career.OfficeReputation,
                LastOutcome = career.LastOutcome,
                LastPetitionOutcome = career.LastPetitionOutcome,
                PetitionOutcomeCategory = OfficeAndCareerDescriptors.DeterminePetitionOutcomeCategory(career.LastPetitionOutcome),
                PromotionPressureLabel = OfficeAndCareerDescriptors.DescribePromotionPressure(career.PromotionMomentum),
                DemotionPressureLabel = OfficeAndCareerDescriptors.DescribeDemotionPressure(career.DemotionPressure),
                AuthorityTrajectorySummary = OfficeAndCareerDescriptors.BuildAuthorityTrajectorySummary(career),
                LastExplanation = career.LastExplanation,
            };
        }

        private static JurisdictionAuthoritySnapshot CloneJurisdiction(JurisdictionAuthorityState jurisdiction)
        {
            return new JurisdictionAuthoritySnapshot
            {
                SettlementId = jurisdiction.SettlementId,
                LeadOfficialPersonId = jurisdiction.LeadOfficialPersonId,
                LeadOfficialName = jurisdiction.LeadOfficialName,
                LeadOfficeTitle = jurisdiction.LeadOfficeTitle,
                AuthorityTier = jurisdiction.AuthorityTier,
                JurisdictionLeverage = jurisdiction.JurisdictionLeverage,
                PetitionPressure = jurisdiction.PetitionPressure,
                PetitionBacklog = jurisdiction.PetitionBacklog,
                CurrentAdministrativeTask = jurisdiction.CurrentAdministrativeTask,
                LastPetitionOutcome = jurisdiction.LastPetitionOutcome,
                AdministrativeTaskTier = OfficeAndCareerDescriptors.DetermineAdministrativeTaskTier(jurisdiction.CurrentAdministrativeTask, jurisdiction.AuthorityTier),
                PetitionOutcomeCategory = OfficeAndCareerDescriptors.DeterminePetitionOutcomeCategory(jurisdiction.LastPetitionOutcome),
                LastAdministrativeTrace = jurisdiction.LastAdministrativeTrace,
            };
        }
    }

    private sealed record AdministrativeResolution(
        string TaskName,
        int TaskLoad,
        int PetitionBacklog,
        string PetitionOutcome,
        int PetitionPressureAdjustment,
        int LeverageAdjustment,
        int PromotionMomentumAdjustment,
        int DemotionPressureAdjustment,
        int OfficeReputationAdjustment);

    private sealed record AdministrativeTaskPlan(string TaskTier, string TaskName);
}
