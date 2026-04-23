using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

/// <summary>
/// FrontierStrainEscalated -> OfficialSupplyRequisition -> HouseholdBurden thin chain tests.
/// </summary>
[TestFixture]
public sealed class FrontierSupplyHouseholdChainTests
{
    [Test]
    public void ThinFrontierChain_FrontierStrain_SupplyRequisition_HouseholdBurden()
    {
        QueryRegistry queries = new();
        DomainEventBuffer buffer = new();
        WorldDiff diff = new();
        FeatureManifest manifest = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 10),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            diff);

        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.CurrentSeason.FrontierPressure = 60;
        worldState.Settlements.Add(MakeSettlement(1, SettlementNodeKind.CountySeat));
        worldState.Settlements.Add(MakeSettlement(2, SettlementNodeKind.MarketTown));
        worldModule.RegisterQueries(worldState, queries);
        worldModule.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(worldState, context));

        IDomainEvent[] frontierEvents = buffer.Events.ToArray();
        Assert.That(
            frontierEvents,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.FrontierStrainEscalated
                     && e.EntityKey == "1"),
            "WorldSettlements must emit FrontierStrainEscalated with a settlement EntityKey.");

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.People.Add(MakeOfficial(1, 1));
        officeState.People.Add(MakeOfficial(2, 2));
        officeState.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(officeState.People);
        officeModule.RegisterQueries(officeState, queries);
        officeModule.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            officeState, context, frontierEvents));

        IDomainEvent[] supplyEvents = buffer.Events.Skip(frontierEvents.Length).ToArray();
        Assert.That(
            supplyEvents,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.OfficialSupplyRequisition
                     && e.EntityKey == "1"),
            "OfficeAndCareer must emit OfficialSupplyRequisition for the affected settlement.");
        Assert.That(
            supplyEvents,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.OfficialSupplyRequisition
                     && e.EntityKey == "2"),
            "OfficeAndCareer must not fan one frontier signal out to every jurisdiction.");

        PopulationAndHouseholdsModule popModule = new();
        PopulationAndHouseholdsState popState = popModule.CreateInitialState();
        popState.Households.Add(MakeHousehold(1, 1, distress: 78, debt: 30));
        popState.Households.Add(MakeHousehold(2, 2, distress: 40, debt: 30));
        popModule.RegisterQueries(popState, queries);
        popModule.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            popState, context, buffer.Events.ToList()));

        PopulationHouseholdState household1 = popState.Households.Single(h => h.Id == new HouseholdId(1));
        PopulationHouseholdState household2 = popState.Households.Single(h => h.Id == new HouseholdId(2));

        Assert.That(household1.Distress, Is.EqualTo(83),
            "Household in settlement 1 must gain +5 distress from supply requisition.");
        Assert.That(household1.DebtPressure, Is.EqualTo(33),
            "Household in settlement 1 must gain +3 debt from supply requisition.");
        Assert.That(household2.Distress, Is.EqualTo(40),
            "Household in settlement 2 must remain untouched.");
        Assert.That(household2.DebtPressure, Is.EqualTo(30),
            "Household in settlement 2 must remain untouched.");
        Assert.That(
            buffer.Events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased
                     && e.EntityKey == "1"),
            "PopulationAndHouseholds must emit the threshold-crossing burden receipt.");
    }

    [Test]
    public void ThinFrontierChain_RealScheduler_FrontierDrainsIntoScopedHouseholdBurden()
    {
        FeatureManifest manifest = BuildManifest();
        IReadOnlyList<IModuleRunner> modules = BuildModules();
        Dictionary<string, object> states = BuildStates(modules);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        worldState.CurrentSeason.FrontierPressure = 60;
        worldState.Settlements.Add(MakeSettlement(1, SettlementNodeKind.CountySeat));
        worldState.Settlements.Add(MakeSettlement(2, SettlementNodeKind.MarketTown));

        PopulationAndHouseholdsState popState = (PopulationAndHouseholdsState)states[KnownModuleKeys.PopulationAndHouseholds];
        popState.Households.Add(MakeHousehold(1, 1, distress: 78, debt: 30));
        popState.Households.Add(MakeHousehold(2, 2, distress: 40, debt: 30));

        OfficeAndCareerState officeState = (OfficeAndCareerState)states[KnownModuleKeys.OfficeAndCareer];
        officeState.People.Add(MakeOfficial(1, 1));
        officeState.People.Add(MakeOfficial(2, 2));
        officeState.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(officeState.People);

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 10),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.FrontierStrainEscalated
                     && e.EntityKey == "1"),
            "Real scheduler must emit settlement-scoped FrontierStrainEscalated.");
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.OfficialSupplyRequisition
                     && e.EntityKey == "1"),
            "Real scheduler must drain FrontierStrainEscalated into scoped OfficialSupplyRequisition.");
        Assert.That(
            events,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.OfficialSupplyRequisition
                     && e.EntityKey == "2"),
            "Real scheduler must not spread a frontier signal to every jurisdiction.");

        PopulationHouseholdState affected = popState.Households.Single(static h => h.Id == new HouseholdId(1));
        PopulationHouseholdState unaffected = popState.Households.Single(static h => h.Id == new HouseholdId(2));
        Assert.That(affected.Distress, Is.GreaterThanOrEqualTo(80),
            "Affected household distress must cross the burden receipt threshold after scheduler drain.");
        Assert.That(affected.DebtPressure, Is.GreaterThan(30),
            "Affected household debt must rise after scheduler drain.");
        Assert.That(unaffected.DebtPressure, Is.LessThan(affected.DebtPressure),
            "Off-scope household must not receive the supply debt delta.");
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased
                     && e.EntityKey == "1"),
            "Real scheduler must emit the household burden receipt for the affected household.");
    }

    [Test]
    public void ThinFrontierChain_RealScheduler_ActiveFrontierBandDoesNotRepeat()
    {
        FeatureManifest manifest = BuildManifest();
        IReadOnlyList<IModuleRunner> modules = BuildModules();
        Dictionary<string, object> states = BuildStates(modules);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        worldState.CurrentSeason.FrontierPressure = 75;
        worldState.LastDeclaredFrontierStrainBand = 2;
        worldState.Settlements.Add(MakeSettlement(1, SettlementNodeKind.CountySeat));

        PopulationAndHouseholdsState popState = (PopulationAndHouseholdsState)states[KnownModuleKeys.PopulationAndHouseholds];
        popState.Households.Add(MakeHousehold(1, 1, distress: 40, debt: 30));

        OfficeAndCareerState officeState = (OfficeAndCareerState)states[KnownModuleKeys.OfficeAndCareer];
        officeState.People.Add(MakeOfficial(1, 1));
        officeState.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(officeState.People);

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 10),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            states,
            modules);

        Assert.That(
            result.DomainEvents,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.FrontierStrainEscalated),
            "A persistent active frontier band must not repeat the declaration.");
        Assert.That(
            result.DomainEvents,
            Has.None.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.OfficialSupplyRequisition),
            "No repeated frontier declaration means no repeated supply requisition.");
    }

    private static FeatureManifest BuildManifest()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        return manifest;
    }

    private static IReadOnlyList<IModuleRunner> BuildModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new OfficeAndCareerModule(),
        ];
    }

    private static Dictionary<string, object> BuildStates(IReadOnlyList<IModuleRunner> modules)
    {
        return modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);
    }

    private static SettlementStateData MakeSettlement(int id, SettlementNodeKind nodeKind)
    {
        return new SettlementStateData
        {
            Id = new SettlementId(id),
            Name = $"Settlement {id}",
            Tier = nodeKind == SettlementNodeKind.CountySeat
                ? SettlementTier.CountySeat
                : SettlementTier.MarketTown,
            NodeKind = nodeKind,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 60,
            Prosperity = 60,
        };
    }

    private static OfficeCareerState MakeOfficial(int personId, int settlementId)
    {
        return new OfficeCareerState
        {
            PersonId = new PersonId(personId),
            ClanId = new ClanId(personId),
            SettlementId = new SettlementId(settlementId),
            DisplayName = $"Official {personId}",
            HasAppointment = true,
            OfficeTitle = $"County Office {settlementId}",
            AuthorityTier = 2,
        };
    }

    private static PopulationHouseholdState MakeHousehold(int householdId, int settlementId, int distress, int debt)
    {
        return new PopulationHouseholdState
        {
            Id = new HouseholdId(householdId),
            HouseholdName = $"Household {householdId}",
            SettlementId = new SettlementId(settlementId),
            Distress = distress,
            DebtPressure = debt,
            LaborCapacity = 80,
            MigrationRisk = 20,
        };
    }
}
