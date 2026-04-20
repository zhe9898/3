using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed partial class OfficeAndCareerModule : ModuleRunner<OfficeAndCareerState>
{
    private static void ApplyXunCareerDrift(

        SimulationXun currentXun,

        ClanNarrativeSnapshot narrative,

        OfficeCareerState career,

        SettlementDisorderSnapshot? disorder,

        SettlementBlackRoutePressureSnapshot? blackRoutePressure,

        LocalForcePoolSnapshot? force)

    {

        if (career.HasAppointment)

        {

            ApplyServingXunDrift(currentXun, narrative, career, disorder, blackRoutePressure, force);

            return;

        }


        ApplyPreAppointmentXunDrift(currentXun, career, disorder, blackRoutePressure, force);

    }


    private static void ApplyServingXunDrift(

        SimulationXun currentXun,

        ClanNarrativeSnapshot narrative,

        OfficeCareerState career,

        SettlementDisorderSnapshot? disorder,

        SettlementBlackRoutePressureSnapshot? blackRoutePressure,

        LocalForcePoolSnapshot? force)

    {

        int authorityTier = Math.Max(career.AuthorityTier, 1);

        int petitionHeat = ComputePetitionHeat(career, disorder, blackRoutePressure, force);

        AdministrativeTaskPlan taskPlan = DetermineAdministrativeTaskPlan(authorityTier, petitionHeat, narrative);

        bool hasCarryover = HasAdministrativeCarryover(disorder);

        bool calmSurface = IsCalmOfficeSurface(disorder, blackRoutePressure, force);


        career.CurrentAdministrativeTask = SelectAdministrativeTask(career.CurrentAdministrativeTask, taskPlan.TaskName, authorityTier);


        switch (currentXun)

        {

            case SimulationXun.Shangxun:

                career.AdministrativeTaskLoad = Math.Clamp(

                    career.AdministrativeTaskLoad

                    + (disorder?.SuppressionDemand >= 45 ? 1 : 0)

                    + (blackRoutePressure?.ImplementationDrag >= 50 ? 1 : 0)

                    + (force?.HasActiveConflict == true ? 1 : 0)

                    - (calmSurface && (blackRoutePressure?.PaperCompliance ?? 0) >= 60 ? 1 : 0),

                    0,

                    100);

                career.PetitionPressure = Math.Clamp(

                    career.PetitionPressure

                    + (disorder?.RoutePressure >= 50 ? 1 : 0)

                    + (blackRoutePressure?.RetaliationRisk >= 55 ? 1 : 0)

                    + (hasCarryover ? 1 : 0)

                    - (calmSurface && (blackRoutePressure?.PaperCompliance ?? 0) >= 65 ? 1 : 0),

                    0,

                    100);

                career.JurisdictionLeverage = Math.Clamp(

                    career.JurisdictionLeverage

                    + ((blackRoutePressure?.PaperCompliance ?? 0) >= 55 ? 1 : 0)

                    - (blackRoutePressure?.ImplementationDrag >= 60 ? 1 : 0)

                    - (force?.IsResponseActivated == true ? 1 : 0),

                    0,

                    100);

                break;


            case SimulationXun.Zhongxun:

                career.PetitionBacklog = Math.Clamp(

                    career.PetitionBacklog

                    + (disorder?.DisorderPressure >= 45 ? 1 : 0)

                    + (blackRoutePressure?.ImplementationDrag >= 45 ? 1 : 0)

                    + (force?.HasActiveConflict == true ? 1 : 0)

                    + (hasCarryover ? 1 : 0)

                    - (calmSurface && (blackRoutePressure?.PaperCompliance ?? 0) >= 65 ? 1 : 0),

                    0,

                    100);

                career.PetitionPressure = Math.Clamp(

                    career.PetitionPressure

                    + (career.PetitionBacklog >= 24 ? 1 : 0)

                    + (blackRoutePressure?.RetaliationRisk >= 60 ? 1 : 0)

                    - (calmSurface && career.PetitionBacklog <= 10 ? 1 : 0),

                    0,

                    100);

                career.ClerkDependence = Math.Clamp(

                    career.ClerkDependence

                    + (career.PetitionBacklog >= 24 ? 1 : 0)

                    + (blackRoutePressure?.ImplementationDrag >= 55 ? 1 : 0)

                    - (calmSurface && (blackRoutePressure?.PaperCompliance ?? 0) >= 70 ? 1 : 0),

                    0,

                    100);

                break;


            case SimulationXun.Xiaxun:

                career.AdministrativeTaskLoad = Math.Clamp(

                    career.AdministrativeTaskLoad

                    + (calmSurface ? -1 : 0)

                    + (hasCarryover ? 1 : 0)

                    + (force?.IsResponseActivated == true ? 1 : 0),

                    0,

                    100);

                career.PetitionBacklog = Math.Clamp(

                    career.PetitionBacklog

                    + (calmSurface ? -1 : 0)

                    + (hasCarryover ? 1 : 0)

                    + (disorder?.DisorderPressure >= 55 ? 1 : 0),

                    0,

                    100);

                career.PetitionPressure = Math.Clamp(

                    career.PetitionPressure

                    + (calmSurface ? -1 : 0)

                    + (hasCarryover ? 1 : 0)

                    + (blackRoutePressure?.RetaliationRisk >= 55 ? 1 : 0),

                    0,

                    100);

                career.ClerkDependence = Math.Clamp(

                    career.ClerkDependence

                    + (calmSurface && career.PetitionBacklog <= 12 ? -1 : 0)

                    + (hasCarryover ? 1 : 0)

                    + (force?.IsResponseActivated == true ? 1 : 0),

                    0,

                    100);

                career.JurisdictionLeverage = Math.Clamp(

                    career.JurisdictionLeverage

                    + (calmSurface && (blackRoutePressure?.PaperCompliance ?? 0) >= 50 ? 1 : 0)

                    - (blackRoutePressure?.ImplementationDrag >= 60 ? 1 : 0)

                    - (force?.IsResponseActivated == true ? 1 : 0),

                    0,

                    100);

                break;

        }

    }


    private static void ApplyPreAppointmentXunDrift(

        SimulationXun currentXun,

        OfficeCareerState career,

        SettlementDisorderSnapshot? disorder,

        SettlementBlackRoutePressureSnapshot? blackRoutePressure,

        LocalForcePoolSnapshot? force)

    {

        bool attachedToYamen = string.Equals(career.LastOutcome, "鍚樊", StringComparison.Ordinal)

            || career.AppointmentPressure >= 28

            || career.ClerkDependence >= 18;

        bool calmSurface = IsCalmOfficeSurface(disorder, blackRoutePressure, force);

        int petitionHeat = ComputePetitionHeat(career, disorder, blackRoutePressure, force);


        switch (currentXun)

        {

            case SimulationXun.Shangxun:

                career.AppointmentPressure = Math.Clamp(

                    career.AppointmentPressure

                    + (career.IsEligible ? 1 : 0)

                    + (attachedToYamen && petitionHeat < 45 ? 1 : 0)

                    - (petitionHeat >= 60 ? 1 : 0),

                    0,

                    100);

                career.ClerkDependence = Math.Clamp(

                    career.ClerkDependence

                    + (attachedToYamen ? 1 : 0)

                    + (blackRoutePressure?.ImplementationDrag >= 50 ? 1 : 0),

                    0,

                    100);

                break;


            case SimulationXun.Zhongxun:

                career.AdministrativeTaskLoad = Math.Clamp(

                    career.AdministrativeTaskLoad

                    + (attachedToYamen ? 1 : 0)

                    + (petitionHeat >= 55 ? 1 : 0)

                    - (calmSurface && !attachedToYamen ? 1 : 0),

                    0,

                    100);

                career.PetitionPressure = Math.Clamp(

                    career.PetitionPressure

                    + (petitionHeat >= 50 ? 1 : 0)

                    + (force?.HasActiveConflict == true ? 1 : 0)

                    - (calmSurface ? 1 : 0),

                    0,

                    100);

                career.ClerkDependence = Math.Clamp(

                    career.ClerkDependence

                    + (attachedToYamen && petitionHeat >= 45 ? 1 : 0)

                    - (calmSurface && career.ClerkDependence >= 12 ? 1 : 0),

                    0,

                    100);

                break;


            case SimulationXun.Xiaxun:

                career.AppointmentPressure = Math.Clamp(

                    career.AppointmentPressure

                    + (career.IsEligible && calmSurface ? 1 : 0)

                    - (petitionHeat >= 65 ? 1 : 0),

                    0,

                    100);

                career.ClerkDependence = Math.Clamp(

                    career.ClerkDependence

                    + (attachedToYamen && petitionHeat >= 50 ? 1 : 0)

                    - (calmSurface && career.ClerkDependence >= 10 ? 1 : 0),

                    0,

                    100);

                career.PetitionPressure = Math.Clamp(

                    career.PetitionPressure

                    + (petitionHeat >= 55 ? 1 : 0)

                    - (calmSurface ? 1 : 0),

                    0,

                    100);

                break;

        }


        bool refreshedAttachment = attachedToYamen || career.AppointmentPressure >= 30 || career.ClerkDependence >= 18;

        career.CurrentAdministrativeTask = ResolvePreAppointmentTask(career.AppointmentPressure, refreshedAttachment);

    }


}
