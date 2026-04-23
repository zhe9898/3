using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds.Tests;

[TestFixture]
public sealed class OfficialSupplyBurdenHandlerTests
{
    [Test]
    public void PublishedEvents_DeclareHouseholdBurdenIncreased()
    {
        PopulationAndHouseholdsModule module = new();
        Assert.That(
            module.PublishedEvents,
            Does.Contain(PopulationEventNames.HouseholdBurdenIncreased),
            "PopulationAndHouseholds emits HouseholdBurdenIncreased, so the event contract must declare it.");
    }

    [Test]
    public void ConsumedEvents_DeclareOfficialSupplyRequisition()
    {
        PopulationAndHouseholdsModule module = new();
        Assert.That(
            module.ConsumedEvents,
            Does.Contain(OfficeAndCareerEventNames.OfficialSupplyRequisition),
            "PopulationAndHouseholds consumes OfficialSupplyRequisition, so the event contract must declare it.");
    }

    [Test]
    public void OfficialSupplyRequisition_RaisesHouseholdBurden()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            SettlementId = new SettlementId(1),
            Distress = 40,
            DebtPressure = 30,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            SettlementId = new SettlementId(1),
            Distress = 50,
            DebtPressure = 40,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(3),
            SettlementId = new SettlementId(2),
            Distress = 40,
            DebtPressure = 30,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.OfficialSupplyRequisition,
            "县尉奉边报征粮，1地界需筹军需。",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        PopulationHouseholdState household1 = state.Households.Single(h => h.Id == new HouseholdId(1));
        PopulationHouseholdState household2 = state.Households.Single(h => h.Id == new HouseholdId(2));
        PopulationHouseholdState household3 = state.Households.Single(h => h.Id == new HouseholdId(3));

        Assert.That(household1.Distress, Is.EqualTo(45), "Household in affected settlement should gain +5 distress.");
        Assert.That(household1.DebtPressure, Is.EqualTo(33), "Household in affected settlement should gain +3 debt.");
        Assert.That(household2.Distress, Is.EqualTo(55), "Household in affected settlement should gain +5 distress.");
        Assert.That(household2.DebtPressure, Is.EqualTo(43), "Household in affected settlement should gain +3 debt.");
        Assert.That(household3.Distress, Is.EqualTo(40), "Household in off-scope settlement must remain untouched.");
        Assert.That(household3.DebtPressure, Is.EqualTo(30), "Household in off-scope settlement must remain untouched.");
    }

    [Test]
    public void OfficialSupplyRequisition_CrossingDistressThreshold_EmitsHouseholdBurdenIncreased()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Zhang household",
            SettlementId = new SettlementId(1),
            Distress = 78,
            DebtPressure = 30,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Li household",
            SettlementId = new SettlementId(1),
            Distress = 85,
            DebtPressure = 40,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.OfficialSupplyRequisition,
            "supply requisition for settlement 1",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent burden = buffer.Events.Single(
            static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased);
        Assert.That(burden.EntityKey, Is.EqualTo("1"));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseOfficialSupply));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("1"));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.DistressBefore], Is.EqualTo("78"));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.DistressAfter], Is.EqualTo("83"));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidEntityKey_IsNoOp()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            SettlementId = new SettlementId(1),
            Distress = 40,
            DebtPressure = 30,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.OfficialSupplyRequisition,
            "invalid entity key",
            "not-a-number"));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Households[0].Distress, Is.EqualTo(40), "Invalid EntityKey should be a no-op.");
        Assert.That(state.Households[0].DebtPressure, Is.EqualTo(30), "Invalid EntityKey should be a no-op.");
    }
}
