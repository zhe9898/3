using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
{
    private static void UpdateCareer(

        ModuleExecutionScope<OfficeAndCareerState> scope,

        EducationCandidateSnapshot candidate,

        ClanNarrativeSnapshot narrative,

        OfficeCareerState career,

        OrderAdministrativeAftermath orderAftermath)

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


        UpdateServingCareer(scope, candidate, narrative, career, reputationSignal, leverageBase, petitionBase, orderAftermath);

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

        scope.Emit(OfficeAndCareerEventNames.OfficeGranted, $"{candidate.DisplayName}得授{career.OfficeTitle}。");

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

        int petitionBase,

        OrderAdministrativeAftermath orderAftermath)

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

        career.CurrentAdministrativeTask = SelectAdministrativeTask(

            resolution.TaskName,

            orderAftermath.TaskName,

            previousTier);

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


        if (orderAftermath.HasImpact)

        {

            ApplyOrderAdministrativeAftermath(career, orderAftermath);

        }


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

                orderAftermath.HasImpact

                    ? $"上月{orderAftermath.CommandLabel}余波压来，积案壅滞，公信先败于{previousTitle}任上。"

                    : $"积案壅滞，公信先败于{previousTitle}任上。");

            career.LastExplanation =

                orderAftermath.HasImpact

                    ? $"上月{orderAftermath.CommandLabel}余波未尽，积案{career.PetitionBacklog}、羞压{narrative.ShamePressure}与人情{narrative.FavorBalance}俱失，地方公信尽散，遂失官身。"

                    : $"积案{career.PetitionBacklog}、羞压{narrative.ShamePressure}与人情{narrative.FavorBalance}俱失，地方公信尽散，遂失官身。";


            scope.RecordDiff(

                orderAftermath.HasImpact

                    ? $"{candidate.DisplayName}因上月{orderAftermath.CommandLabel}余波压案、杖力尽失而罢官；积案{career.PetitionBacklog}，羞压{narrative.ShamePressure}，人情{narrative.FavorBalance}。"

                    : $"{candidate.DisplayName}因积案壅滞、杖力尽失而罢官；积案{career.PetitionBacklog}，羞压{narrative.ShamePressure}，人情{narrative.FavorBalance}。",

                candidate.PersonId.Value.ToString());

            scope.Emit(OfficeAndCareerEventNames.OfficeLost, $"{candidate.DisplayName}因案牍壅滞而失官。");

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

            orderAftermath.HasImpact

                ? $"{candidate.DisplayName}今为{career.OfficeTitle}，乡面杖力{career.JurisdictionLeverage}，词牍压{career.PetitionPressure}，积案{career.PetitionBacklog}，所办{taskTier}差遣为{career.CurrentAdministrativeTask}；上月{orderAftermath.CommandLabel}余波仍压在县署。"

                : $"{candidate.DisplayName}今为{career.OfficeTitle}，乡面杖力{career.JurisdictionLeverage}，词牍压{career.PetitionPressure}，积案{career.PetitionBacklog}，所办{taskTier}差遣为{career.CurrentAdministrativeTask}。",

            candidate.PersonId.Value.ToString());


        if (updatedTier != previousTier)

        {

            scope.Emit(OfficeAndCareerEventNames.AuthorityChanged, $"{candidate.DisplayName}官阶改为第{updatedTier}等。");

        }


        if (!string.Equals(previousTitle, career.OfficeTitle, StringComparison.Ordinal))

        {

            scope.Emit(OfficeAndCareerEventNames.OfficeTransfer, $"{candidate.DisplayName}由{previousTitle}转为{career.OfficeTitle}。");

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


}
