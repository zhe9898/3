using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Presentation.Unity;
using Zongzu.Persistence;

namespace Zongzu.Integration.Tests;

public sealed partial class M2LiteIntegrationTests
{
    private static GameSimulation ConfigureOfficeReach(

        GameSimulation simulation,

        SettlementId settlementId,

        int leverage,

        int clerkDependence,

        int administrativeTaskLoad,

        int petitionPressure,

        int petitionBacklog)

    {

        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();

        ModuleStateEnvelope envelope = saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer];

        OfficeAndCareerState officeState = (OfficeAndCareerState)serializer.Deserialize(

            typeof(OfficeAndCareerState),

            envelope.Payload);

        OfficeCareerState[] careers = officeState.People

            .Where(person => person.HasAppointment && person.SettlementId == settlementId)

            .ToArray();


        Assert.That(careers, Is.Not.Empty);


        foreach (OfficeCareerState career in careers)

        {

            career.JurisdictionLeverage = leverage;

            career.ClerkDependence = clerkDependence;

            career.AdministrativeTaskLoad = administrativeTaskLoad;

            career.PetitionPressure = petitionPressure;

            career.PetitionBacklog = petitionBacklog;

        }


        officeState.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(officeState.People);

        saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer] = new ModuleStateEnvelope

        {

            ModuleKey = envelope.ModuleKey,

            ModuleSchemaVersion = envelope.ModuleSchemaVersion,

            Payload = serializer.Serialize(typeof(OfficeAndCareerState), officeState),

        };


        return SimulationBootstrapper.LoadP1GovernanceLocalConflict(saveRoot);

    }


    private static OfficeCareerState GetLeadOfficeCareer(GameSimulation simulation, SettlementId settlementId)

    {

        MessagePackModuleStateSerializer serializer = new();

        OfficeAndCareerState officeState = (OfficeAndCareerState)serializer.Deserialize(

            typeof(OfficeAndCareerState),

            simulation.ExportSave().ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);


        return officeState.People

            .Where(person => person.HasAppointment && person.SettlementId == settlementId)

            .OrderByDescending(person => person.AuthorityTier)

            .ThenByDescending(person => person.OfficeReputation)

            .ThenBy(person => person.PersonId.Value)

            .First();

    }


    private static OrderAndBanditryState GetOrderState(GameSimulation simulation)

    {

        MessagePackModuleStateSerializer serializer = new();

        return (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            simulation.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

    }
}
