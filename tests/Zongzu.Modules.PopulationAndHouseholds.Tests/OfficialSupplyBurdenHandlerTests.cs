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
    public void OfficialSupplyRequisition_UsesHouseholdProfileAndKeepsOffScopeUntouched()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Fragile tenant",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Tenant,
            Distress = 40,
            DebtPressure = 60,
            LaborCapacity = 25,
            MigrationRisk = 20,
            LandHolding = 5,
            GrainStore = 10,
            ToolCondition = 20,
            ShelterQuality = 20,
            DependentCount = 5,
            LaborerCount = 1,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Buffered smallholder",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Smallholder,
            Distress = 40,
            DebtPressure = 30,
            LaborCapacity = 90,
            MigrationRisk = 20,
            LandHolding = 50,
            GrainStore = 85,
            ToolCondition = 80,
            ShelterQuality = 80,
            DependentCount = 1,
            LaborerCount = 3,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(3),
            SettlementId = new SettlementId(2),
            Distress = 40,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
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

        buffer.Emit(MakeSupplyEvent(new SettlementId(1), new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        PopulationHouseholdState household1 = state.Households.Single(h => h.Id == new HouseholdId(1));
        PopulationHouseholdState household2 = state.Households.Single(h => h.Id == new HouseholdId(2));
        PopulationHouseholdState household3 = state.Households.Single(h => h.Id == new HouseholdId(3));

        Assert.That(household1.Distress, Is.GreaterThan(household2.Distress),
            "A fragile tenant household should absorb more distress than a buffered household.");
        Assert.That(household1.DebtPressure, Is.GreaterThan(household2.DebtPressure),
            "Debt response should use household liquidity/fragility, not a fixed flat delta.");
        Assert.That(household1.LaborCapacity, Is.LessThan(25),
            "Military supply pressure should consume scarce household labor.");
        Assert.That(household2.Distress, Is.EqualTo(40),
            "Strong grain/labor buffers can absorb the first supply hit without visible distress.");
        Assert.That(household2.DebtPressure, Is.EqualTo(30),
            "Strong grain/labor buffers can absorb the first supply hit without visible debt.");
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
            LaborCapacity = 80,
            MigrationRisk = 20,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Li household",
            SettlementId = new SettlementId(1),
            Distress = 85,
            DebtPressure = 40,
            LaborCapacity = 80,
            MigrationRisk = 20,
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

        buffer.Emit(MakeSupplyEvent(new SettlementId(1), new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "70",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "9",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "2",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "3",
        }));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent burden = buffer.Events.Single(
            static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased);
        Assert.That(burden.EntityKey, Is.EqualTo("1"));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseOfficialSupply));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("1"));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.DistressBefore], Is.EqualTo("78"));
        Assert.That(int.Parse(burden.Metadata[DomainEventMetadataKeys.DistressAfter]), Is.GreaterThanOrEqualTo(80));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo("12"));
        Assert.That(burden.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.Not.Empty);
        Assert.That(burden.Metadata[DomainEventMetadataKeys.Livelihood], Is.EqualTo(LivelihoodType.Smallholder.ToString()));
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

    [Test]
    public void OfficialSupplyRequisition_DefaultDistressDeltaClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDistressDeltaClampFloor = 0,
                OfficialSupplyDistressDeltaClampCeiling = 24,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunCrossingOfficialSupply();
        (PopulationHouseholdState explicitHousehold, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunCrossingOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaClampCeiling, Is.EqualTo(24));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyDistressDeltaClampFloorOrDefault(), Is.EqualTo(0));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyDistressDeltaClampCeilingOrDefault(), Is.EqualTo(24));
        Assert.That(explicitHousehold.Distress, Is.EqualTo(defaultHousehold.Distress));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo("6"));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomDistressDeltaClampFloorRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDistressDeltaClampFloor = 2,
                OfficialSupplyDistressDeltaClampCeiling = 24,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunBufferedOfficialSupply();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunBufferedOfficialSupply(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.Distress, Is.EqualTo(40));
        Assert.That(customHousehold.Distress, Is.EqualTo(42));
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
        Assert.That(customEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomDistressDeltaClampCeilingRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDistressDeltaClampFloor = 0,
                OfficialSupplyDistressDeltaClampCeiling = 4,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunCrossingOfficialSupply();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunCrossingOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo("6"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo("4"));
        Assert.That(customHousehold.Distress, Is.EqualTo(defaultHousehold.Distress - 2));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidDistressDeltaClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDistressDeltaClampFloor = 25,
                OfficialSupplyDistressDeltaClampCeiling = 24,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunCrossingOfficialSupply();
        (PopulationHouseholdState fallbackHousehold, IReadOnlyList<IDomainEvent> fallbackEvents) =
            RunCrossingOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetOfficialSupplyDistressDeltaClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaClampFloor));
        Assert.That(
            malformedRulesData.GetOfficialSupplyDistressDeltaClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaClampCeiling));
        Assert.That(fallbackHousehold.Distress, Is.EqualTo(defaultHousehold.Distress));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo("6"));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultDebtDeltaClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDebtDeltaClampFloor = 0,
                OfficialSupplyDebtDeltaClampCeiling = 18,
            };

        (PopulationHouseholdState defaultHousehold, _) = RunCrossingOfficialSupply();
        (PopulationHouseholdState explicitHousehold, _) = RunCrossingOfficialSupply(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaClampCeiling, Is.EqualTo(18));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyDebtDeltaClampFloorOrDefault(), Is.EqualTo(0));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyDebtDeltaClampCeilingOrDefault(), Is.EqualTo(18));
        Assert.That(explicitHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(defaultHousehold.DebtPressure, Is.EqualTo(35));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomDebtDeltaClampFloorRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDebtDeltaClampFloor = 2,
                OfficialSupplyDebtDeltaClampCeiling = 18,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunBufferedOfficialSupply();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunBufferedOfficialSupply(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.DebtPressure, Is.EqualTo(30));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(32));
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
        Assert.That(customEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomDebtDeltaClampCeilingRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDebtDeltaClampFloor = 0,
                OfficialSupplyDebtDeltaClampCeiling = 4,
            };

        (PopulationHouseholdState defaultHousehold, _) = RunCrossingOfficialSupply();
        (PopulationHouseholdState customHousehold, _) = RunCrossingOfficialSupply(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.DebtPressure, Is.EqualTo(35));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(34));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidDebtDeltaClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDebtDeltaClampFloor = 19,
                OfficialSupplyDebtDeltaClampCeiling = 18,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, _) = RunCrossingOfficialSupply();
        (PopulationHouseholdState fallbackHousehold, _) = RunCrossingOfficialSupply(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetOfficialSupplyDebtDeltaClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaClampFloor));
        Assert.That(
            malformedRulesData.GetOfficialSupplyDebtDeltaClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaClampCeiling));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(35));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultLaborDropClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLaborDropClampFloor = 0,
                OfficialSupplyLaborDropClampCeiling = 8,
            };

        (PopulationHouseholdState defaultHousehold, _) = RunCrossingOfficialSupply();
        (PopulationHouseholdState explicitHousehold, _) = RunCrossingOfficialSupply(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborDropClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborDropClampCeiling, Is.EqualTo(8));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyLaborDropClampFloorOrDefault(), Is.EqualTo(0));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyLaborDropClampCeilingOrDefault(), Is.EqualTo(8));
        Assert.That(explicitHousehold.LaborCapacity, Is.EqualTo(defaultHousehold.LaborCapacity));
        Assert.That(defaultHousehold.LaborCapacity, Is.EqualTo(79));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomLaborDropClampFloorRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLaborDropClampFloor = 4,
                OfficialSupplyLaborDropClampCeiling = 8,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunBufferedOfficialSupply();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunBufferedOfficialSupply(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.LaborCapacity, Is.EqualTo(88));
        Assert.That(customHousehold.LaborCapacity, Is.EqualTo(86));
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
        Assert.That(customEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomLaborDropClampCeilingRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLaborDropClampFloor = 0,
                OfficialSupplyLaborDropClampCeiling = 5,
            };

        (PopulationHouseholdState defaultHousehold, _) = RunLaborHeavyOfficialSupply();
        (PopulationHouseholdState customHousehold, _) = RunLaborHeavyOfficialSupply(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.LaborCapacity, Is.EqualTo(17));
        Assert.That(customHousehold.LaborCapacity, Is.EqualTo(20));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidLaborDropClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLaborDropClampFloor = 9,
                OfficialSupplyLaborDropClampCeiling = 8,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, _) = RunLaborHeavyOfficialSupply();
        (PopulationHouseholdState fallbackHousehold, _) = RunLaborHeavyOfficialSupply(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetOfficialSupplyLaborDropClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborDropClampFloor));
        Assert.That(
            malformedRulesData.GetOfficialSupplyLaborDropClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborDropClampCeiling));
        Assert.That(fallbackHousehold.LaborCapacity, Is.EqualTo(defaultHousehold.LaborCapacity));
        Assert.That(fallbackHousehold.LaborCapacity, Is.EqualTo(17));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultMigrationDeltaClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyMigrationDeltaClampFloor = 0,
                OfficialSupplyMigrationDeltaClampCeiling = 8,
            };

        (PopulationHouseholdState defaultHousehold, _) = RunCrossingOfficialSupply();
        (PopulationHouseholdState explicitHousehold, _) = RunCrossingOfficialSupply(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyMigrationDeltaClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyMigrationDeltaClampCeiling, Is.EqualTo(8));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyMigrationDeltaClampFloorOrDefault(), Is.EqualTo(0));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyMigrationDeltaClampCeilingOrDefault(), Is.EqualTo(8));
        Assert.That(explicitHousehold.MigrationRisk, Is.EqualTo(defaultHousehold.MigrationRisk));
        Assert.That(defaultHousehold.MigrationRisk, Is.EqualTo(21));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomMigrationDeltaClampFloorRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyMigrationDeltaClampFloor = 2,
                OfficialSupplyMigrationDeltaClampCeiling = 8,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunBufferedOfficialSupply();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunBufferedOfficialSupply(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.MigrationRisk, Is.EqualTo(20));
        Assert.That(customHousehold.MigrationRisk, Is.EqualTo(22));
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
        Assert.That(customEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomMigrationDeltaClampCeilingRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyMigrationDeltaClampFloor = 0,
                OfficialSupplyMigrationDeltaClampCeiling = 2,
            };

        (PopulationHouseholdState defaultHousehold, _) = RunLaborHeavyOfficialSupply();
        (PopulationHouseholdState customHousehold, _) = RunLaborHeavyOfficialSupply(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.MigrationRisk, Is.EqualTo(24));
        Assert.That(customHousehold.MigrationRisk, Is.EqualTo(22));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidMigrationDeltaClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyMigrationDeltaClampFloor = 9,
                OfficialSupplyMigrationDeltaClampCeiling = 8,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, _) = RunLaborHeavyOfficialSupply();
        (PopulationHouseholdState fallbackHousehold, _) = RunLaborHeavyOfficialSupply(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetOfficialSupplyMigrationDeltaClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyMigrationDeltaClampFloor));
        Assert.That(
            malformedRulesData.GetOfficialSupplyMigrationDeltaClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyMigrationDeltaClampCeiling));
        Assert.That(fallbackHousehold.MigrationRisk, Is.EqualTo(defaultHousehold.MigrationRisk));
        Assert.That(fallbackHousehold.MigrationRisk, Is.EqualTo(24));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultBurdenEventDistressThresholdRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyBurdenEventDistressThreshold = 80,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunCrossingOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) = RunCrossingOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyBurdenEventDistressThreshold, Is.EqualTo(80));
        Assert.That(explicitPreviousBaseline.GetOfficialSupplyBurdenEventDistressThresholdOrDefault(), Is.EqualTo(80));
        Assert.That(explicitEvent.EntityKey, Is.EqualTo(defaultEvent.EntityKey));
        Assert.That(
            explicitEvent.Metadata[DomainEventMetadataKeys.DistressAfter],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.DistressAfter]));
        Assert.That(
            explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomBurdenEventDistressThresholdRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyBurdenEventDistressThreshold = 58,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLowThresholdOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunLowThresholdOfficialSupply(customRulesData);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
        Assert.That(customEvent.EntityKey, Is.EqualTo("1"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.DistressBefore], Is.EqualTo("54"));
        Assert.That(int.Parse(customEvent.Metadata[DomainEventMetadataKeys.DistressAfter]), Is.GreaterThanOrEqualTo(58));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.Not.Empty);
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidBurdenEventDistressThresholdRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyBurdenEventDistressThreshold = 101,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLowThresholdOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunLowThresholdOfficialSupply(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetOfficialSupplyBurdenEventDistressThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyBurdenEventDistressThreshold));
        Assert.That(
            fallbackEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased),
            Is.EqualTo(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased)));
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased), Is.EqualTo(0));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultSignalFallbackRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFallbackFrontierPressure = 60,
                OfficialSupplyFallbackQuotaPressure = 7,
                OfficialSupplyFallbackDocketPressure = 1,
                OfficialSupplyFallbackClerkDistortionPressure = 0,
                OfficialSupplyFallbackAuthorityBuffer = 4,
                OfficialSupplyFallbackDerivedPressureClampFloor = 4,
                OfficialSupplyFallbackDerivedPressureClampCeiling = 26,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunFallbackOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) = RunFallbackOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackFrontierPressure, Is.EqualTo(60));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackQuotaPressure, Is.EqualTo(7));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackDocketPressure, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackClerkDistortionPressure, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackAuthorityBuffer, Is.EqualTo(4));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackDerivedPressureClampFloor, Is.EqualTo(4));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackDerivedPressureClampCeiling, Is.EqualTo(26));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.FrontierPressure]));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure]));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo("60"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo("4"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure], Is.EqualTo("7"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDocketPressure], Is.EqualTo("1"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure], Is.EqualTo("0"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer], Is.EqualTo("4"));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomSignalFallbackRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFallbackFrontierPressure = 88,
                OfficialSupplyFallbackQuotaPressure = 14,
                OfficialSupplyFallbackDocketPressure = 5,
                OfficialSupplyFallbackClerkDistortionPressure = 3,
                OfficialSupplyFallbackAuthorityBuffer = 2,
                OfficialSupplyFallbackDerivedPressureClampFloor = 4,
                OfficialSupplyFallbackDerivedPressureClampCeiling = 26,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunFallbackOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunFallbackOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo("88"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo("20"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure], Is.EqualTo("14"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDocketPressure], Is.EqualTo("5"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure], Is.EqualTo("3"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer], Is.EqualTo("2"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]),
            Is.GreaterThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidSignalFallbackRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFallbackDerivedPressureClampFloor = 27,
                OfficialSupplyFallbackDerivedPressureClampCeiling = 26,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunFallbackOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunFallbackOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetOfficialSupplyFallbackDerivedPressureClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackDerivedPressureClampFloor));
        Assert.That(
            malformedRulesData.GetOfficialSupplyFallbackDerivedPressureClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFallbackDerivedPressureClampCeiling));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.FrontierPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultSignalNormalizationClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFrontierPressureClampFloor = 0,
                OfficialSupplyFrontierPressureClampCeiling = 100,
                OfficialSupplyPressureClampFloor = 0,
                OfficialSupplyPressureClampCeiling = 30,
                OfficialSupplyQuotaPressureClampFloor = 0,
                OfficialSupplyQuotaPressureClampCeiling = 20,
                OfficialSupplyDocketPressureClampFloor = 0,
                OfficialSupplyDocketPressureClampCeiling = 20,
                OfficialSupplyClerkDistortionPressureClampFloor = 0,
                OfficialSupplyClerkDistortionPressureClampCeiling = 15,
                OfficialSupplyAuthorityBufferClampFloor = 0,
                OfficialSupplyAuthorityBufferClampCeiling = 12,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunOutOfRangeSignalOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) = RunOutOfRangeSignalOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFrontierPressureClampCeiling, Is.EqualTo(100));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyPressureClampCeiling, Is.EqualTo(30));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyQuotaPressureClampCeiling, Is.EqualTo(20));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDocketPressureClampCeiling, Is.EqualTo(20));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyClerkDistortionPressureClampCeiling, Is.EqualTo(15));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyAuthorityBufferClampCeiling, Is.EqualTo(12));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.FrontierPressure]));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo("100"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo("30"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure], Is.EqualTo("20"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDocketPressure], Is.EqualTo("20"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure], Is.EqualTo("15"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer], Is.EqualTo("12"));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomSignalNormalizationClampRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFrontierPressureClampFloor = 11,
                OfficialSupplyFrontierPressureClampCeiling = 50,
                OfficialSupplyPressureClampFloor = 6,
                OfficialSupplyPressureClampCeiling = 10,
                OfficialSupplyQuotaPressureClampFloor = 5,
                OfficialSupplyQuotaPressureClampCeiling = 8,
                OfficialSupplyDocketPressureClampFloor = 4,
                OfficialSupplyDocketPressureClampCeiling = 6,
                OfficialSupplyClerkDistortionPressureClampFloor = 3,
                OfficialSupplyClerkDistortionPressureClampCeiling = 4,
                OfficialSupplyAuthorityBufferClampFloor = 2,
                OfficialSupplyAuthorityBufferClampCeiling = 3,
            };

        (_, IReadOnlyList<IDomainEvent> customEvents) = RunOutOfRangeSignalOfficialSupply(customRulesData);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo("50"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo("10"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure], Is.EqualTo("8"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDocketPressure], Is.EqualTo("6"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure], Is.EqualTo("4"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer], Is.EqualTo("3"));

        (_, IReadOnlyList<IDomainEvent> floorEvents) = RunNegativeSignalOfficialSupply(customRulesData);
        IDomainEvent floorEvent = SingleBurdenEvent(floorEvents);
        Assert.That(floorEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo("11"));
        Assert.That(floorEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo("6"));
        Assert.That(floorEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure], Is.EqualTo("5"));
        Assert.That(floorEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDocketPressure], Is.EqualTo("4"));
        Assert.That(floorEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure], Is.EqualTo("3"));
        Assert.That(floorEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer], Is.EqualTo("2"));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidSignalNormalizationClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyPressureClampFloor = 31,
                OfficialSupplyPressureClampCeiling = 30,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunOutOfRangeSignalOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunOutOfRangeSignalOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetOfficialSupplyPressureClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyPressureClampFloor));
        Assert.That(
            malformedRulesData.GetOfficialSupplyPressureClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyPressureClampCeiling));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.FrontierPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultLivelihoodExposureRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLivelihoodExposureScoreWeights =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLivelihoodExposureScoreWeights,
                OfficialSupplyLivelihoodExposureFallbackScore = 2,
                OfficialSupplyLandVisibilityScoreBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLandVisibilityScoreBands,
                OfficialSupplyLandVisibilityFallbackScore = 0,
                OfficialSupplyLivelihoodExposureClampFloor = 1,
                OfficialSupplyLivelihoodExposureClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLivelihoodExposureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunLivelihoodExposureOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLivelihoodExposureFallbackScore, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLandVisibilityFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLivelihoodExposureClampFloor, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLivelihoodExposureClampCeiling, Is.EqualTo(7));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLivelihoodExposureScoreWeights
                .Single(static entry => entry.Livelihood == LivelihoodType.Boatman).Weight,
            Is.EqualTo(5));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLandVisibilityScoreBands
                .Single(static band => band.Threshold == 70).Score,
            Is.EqualTo(2));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure], Is.EqualTo("7"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomLivelihoodExposureRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLivelihoodExposureScoreWeights =
                    new[] { new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Boatman, 1) },
                OfficialSupplyLivelihoodExposureFallbackScore = 2,
                OfficialSupplyLandVisibilityScoreBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(70, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(35, 0),
                    },
                OfficialSupplyLandVisibilityFallbackScore = 0,
                OfficialSupplyLivelihoodExposureClampFloor = 1,
                OfficialSupplyLivelihoodExposureClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLivelihoodExposureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunLivelihoodExposureOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure], Is.EqualTo("1"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]),
            Is.LessThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidLivelihoodExposureRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLivelihoodExposureScoreWeights =
                    new[]
                    {
                        new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Boatman, 5),
                        new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Boatman, 4),
                    },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLivelihoodExposureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunLivelihoodExposureOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_livelihood_exposure_score_weights must be non-empty, distinct, defined, and between 0 and 8."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLivelihoodExposurePressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultResourceBufferRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyResourceGrainBufferScoreBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceGrainBufferScoreBands,
                OfficialSupplyResourceGrainBufferFallbackScore = 0,
                OfficialSupplyResourceToolConditionThreshold = 70,
                OfficialSupplyResourceToolBufferScore = 1,
                OfficialSupplyResourceToolBufferFallbackScore = 0,
                OfficialSupplyResourceShelterQualityThreshold = 60,
                OfficialSupplyResourceShelterBufferScore = 1,
                OfficialSupplyResourceShelterBufferFallbackScore = 0,
                OfficialSupplyResourceBufferClampFloor = 0,
                OfficialSupplyResourceBufferClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunResourceBufferOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunResourceBufferOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceGrainBufferFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceToolConditionThreshold, Is.EqualTo(70));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceToolBufferScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceShelterQualityThreshold, Is.EqualTo(60));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceShelterBufferScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceBufferClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceBufferClampCeiling, Is.EqualTo(7));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyResourceGrainBufferScoreBands
                .Single(static band => band.Threshold == 85).Score,
            Is.EqualTo(5));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer], Is.EqualTo("7"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomResourceBufferRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyResourceGrainBufferScoreBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(85, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(65, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(45, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(25, 0),
                    },
                OfficialSupplyResourceGrainBufferFallbackScore = 0,
                OfficialSupplyResourceToolConditionThreshold = 70,
                OfficialSupplyResourceToolBufferScore = 0,
                OfficialSupplyResourceToolBufferFallbackScore = 0,
                OfficialSupplyResourceShelterQualityThreshold = 60,
                OfficialSupplyResourceShelterBufferScore = 0,
                OfficialSupplyResourceShelterBufferFallbackScore = 0,
                OfficialSupplyResourceBufferClampFloor = 0,
                OfficialSupplyResourceBufferClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunResourceBufferOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunResourceBufferOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer], Is.EqualTo("0"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]),
            Is.GreaterThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidResourceBufferRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyResourceGrainBufferScoreBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(85, 5),
                        new PopulationHouseholdMobilityThresholdScoreBand(85, 4),
                    },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunResourceBufferOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunResourceBufferOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_resource_grain_buffer_score_bands must be non-empty, distinct, and between threshold 0..100 and score 0..8."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyResourceBuffer]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultLaborPressureRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLaborCapacityPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborCapacityPressureBands,
                OfficialSupplyLaborCapacityPressureFallbackScore = 4,
                OfficialSupplyDependentCountPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDependentCountPressureBands,
                OfficialSupplyDependentCountPressureFallbackScore = 0,
                OfficialSupplyDependentToLaborRatioMultiplier = 2,
                OfficialSupplyDependentToLaborRatioBonusScore = 1,
                OfficialSupplyDependentToLaborRatioFallbackScore = 0,
                OfficialSupplyLaborPressureClampFloor = -1,
                OfficialSupplyLaborPressureClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLaborPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunLaborPressureOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborCapacityPressureFallbackScore, Is.EqualTo(4));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDependentCountPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDependentToLaborRatioMultiplier, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDependentToLaborRatioBonusScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborPressureClampFloor, Is.EqualTo(-1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborPressureClampCeiling, Is.EqualTo(7));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLaborCapacityPressureBands
                .Single(static band => band.Threshold == 80).Score,
            Is.EqualTo(-1));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure], Is.EqualTo("2"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborDrop], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborDrop]));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomLaborPressureRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLaborCapacityPressureBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(80, 4),
                        new PopulationHouseholdMobilityThresholdScoreBand(60, 4),
                        new PopulationHouseholdMobilityThresholdScoreBand(40, 4),
                        new PopulationHouseholdMobilityThresholdScoreBand(25, 4),
                    },
                OfficialSupplyLaborCapacityPressureFallbackScore = 4,
                OfficialSupplyDependentCountPressureBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(5, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(3, 0),
                    },
                OfficialSupplyDependentCountPressureFallbackScore = 0,
                OfficialSupplyDependentToLaborRatioMultiplier = 2,
                OfficialSupplyDependentToLaborRatioBonusScore = 0,
                OfficialSupplyDependentToLaborRatioFallbackScore = 0,
                OfficialSupplyLaborPressureClampFloor = -1,
                OfficialSupplyLaborPressureClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLaborPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunLaborPressureOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure], Is.EqualTo("4"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborDrop]),
            Is.GreaterThanOrEqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborDrop])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidLaborPressureRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLaborCapacityPressureBands =
                    new PopulationHouseholdMobilityThresholdScoreBand[] { },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLaborPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunLaborPressureOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_labor_capacity_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score -4..8."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborDrop], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLaborDrop]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultLiquidityPressureRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLiquidityGrainStrainPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityGrainStrainPressureBands,
                OfficialSupplyLiquidityGrainStrainPressureFallbackScore = 2,
                OfficialSupplyLiquidityCashNeedPressureScore = 2,
                OfficialSupplyLiquidityCashNeedPressureFallbackScore = 0,
                OfficialSupplyLiquidityToolDragConditionThreshold = 35,
                OfficialSupplyLiquidityToolDragPressureScore = 1,
                OfficialSupplyLiquidityToolDragPressureFallbackScore = 0,
                OfficialSupplyLiquidityDebtDragPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityDebtDragPressureBands,
                OfficialSupplyLiquidityDebtDragPressureFallbackScore = 0,
                OfficialSupplyLiquidityPressureClampFloor = -2,
                OfficialSupplyLiquidityPressureClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLiquidityPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunLiquidityPressureOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityGrainStrainPressureFallbackScore, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityCashNeedPressureScore, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityToolDragConditionThreshold, Is.EqualTo(35));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityToolDragPressureScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityDebtDragPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityPressureClampFloor, Is.EqualTo(-2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityPressureClampCeiling, Is.EqualTo(7));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyLiquidityGrainStrainPressureBands
                .Single(static band => band.Threshold == 55).Score,
            Is.EqualTo(-1));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure], Is.EqualTo("4"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomLiquidityPressureRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLiquidityGrainStrainPressureBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(80, -2),
                        new PopulationHouseholdMobilityThresholdScoreBand(55, -2),
                        new PopulationHouseholdMobilityThresholdScoreBand(25, -2),
                        new PopulationHouseholdMobilityThresholdScoreBand(1, -2),
                    },
                OfficialSupplyLiquidityGrainStrainPressureFallbackScore = -2,
                OfficialSupplyLiquidityCashNeedPressureScore = 0,
                OfficialSupplyLiquidityCashNeedPressureFallbackScore = 0,
                OfficialSupplyLiquidityToolDragConditionThreshold = 35,
                OfficialSupplyLiquidityToolDragPressureScore = 0,
                OfficialSupplyLiquidityToolDragPressureFallbackScore = 0,
                OfficialSupplyLiquidityDebtDragPressureBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(65, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(50, 0),
                    },
                OfficialSupplyLiquidityDebtDragPressureFallbackScore = 0,
                OfficialSupplyLiquidityPressureClampFloor = -2,
                OfficialSupplyLiquidityPressureClampCeiling = 7,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLiquidityPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunLiquidityPressureOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure], Is.EqualTo("-2"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta]),
            Is.LessThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidLiquidityPressureRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyLiquidityGrainStrainPressureBands =
                    new PopulationHouseholdMobilityThresholdScoreBand[] { },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunLiquidityPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunLiquidityPressureOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_liquidity_grain_strain_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score -4..8."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyLiquidityPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultFragilityPressureRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFragilityDistressPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityDistressPressureBands,
                OfficialSupplyFragilityDistressPressureFallbackScore = 0,
                OfficialSupplyFragilityDebtPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityDebtPressureBands,
                OfficialSupplyFragilityDebtPressureFallbackScore = 0,
                OfficialSupplyFragilityMigrationRiskThreshold = 70,
                OfficialSupplyFragilityMigrationPressureScore = 1,
                OfficialSupplyFragilityMigrationPressureFallbackScore = 0,
                OfficialSupplyFragilityShelterDragQualityThreshold = 35,
                OfficialSupplyFragilityShelterDragPressureScore = 1,
                OfficialSupplyFragilityShelterDragPressureFallbackScore = 0,
                OfficialSupplyFragilityPressureClampFloor = 0,
                OfficialSupplyFragilityPressureClampCeiling = 8,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunFragilityPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunFragilityPressureOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityDistressPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityDebtPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityMigrationRiskThreshold, Is.EqualTo(70));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityMigrationPressureScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityShelterDragQualityThreshold, Is.EqualTo(35));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityShelterDragPressureScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityPressureClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityPressureClampCeiling, Is.EqualTo(8));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyFragilityDistressPressureBands
                .Single(static band => band.Threshold == 80).Score,
            Is.EqualTo(3));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure], Is.EqualTo("7"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomFragilityPressureRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFragilityDistressPressureBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(80, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(65, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(50, 0),
                    },
                OfficialSupplyFragilityDistressPressureFallbackScore = 0,
                OfficialSupplyFragilityDebtPressureBands =
                    new[]
                    {
                        new PopulationHouseholdMobilityThresholdScoreBand(80, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(65, 0),
                        new PopulationHouseholdMobilityThresholdScoreBand(50, 0),
                    },
                OfficialSupplyFragilityDebtPressureFallbackScore = 0,
                OfficialSupplyFragilityMigrationRiskThreshold = 70,
                OfficialSupplyFragilityMigrationPressureScore = 0,
                OfficialSupplyFragilityMigrationPressureFallbackScore = 0,
                OfficialSupplyFragilityShelterDragQualityThreshold = 35,
                OfficialSupplyFragilityShelterDragPressureScore = 0,
                OfficialSupplyFragilityShelterDragPressureFallbackScore = 0,
                OfficialSupplyFragilityPressureClampFloor = 0,
                OfficialSupplyFragilityPressureClampCeiling = 8,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunFragilityPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunFragilityPressureOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure], Is.EqualTo("0"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]),
            Is.LessThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidFragilityPressureRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyFragilityDistressPressureBands =
                    new PopulationHouseholdMobilityThresholdScoreBand[] { },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunFragilityPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunFragilityPressureOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_fragility_distress_pressure_bands must be non-empty, distinct, and between threshold 0..100 and score 0..8."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyFragilityPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultInteractionPressureRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyInteractionBoatmanLivelihood = LivelihoodType.Boatman,
                OfficialSupplyInteractionBoatmanSupplyPressureThreshold = 12,
                OfficialSupplyInteractionBoatmanBoostScore = 2,
                OfficialSupplyInteractionBoatmanFallbackScore = 0,
                OfficialSupplyInteractionLaborFragilityLivelihoods =
                    PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionLaborFragilityLivelihoods,
                OfficialSupplyInteractionLaborCapacityThreshold = 40,
                OfficialSupplyInteractionLaborFragilityBoostScore = 2,
                OfficialSupplyInteractionLaborFragilityFallbackScore = 0,
                OfficialSupplyInteractionTenantLivelihood = LivelihoodType.Tenant,
                OfficialSupplyInteractionTenantDebtPressureThreshold = 60,
                OfficialSupplyInteractionTenantDebtBoostScore = 1,
                OfficialSupplyInteractionTenantDebtFallbackScore = 0,
                OfficialSupplyInteractionResilienceReliefGrainStoreThreshold = 75,
                OfficialSupplyInteractionResilienceReliefLaborCapacityThreshold = 75,
                OfficialSupplyInteractionResilienceReliefDebtPressureThreshold = 55,
                OfficialSupplyInteractionResilienceReliefDistressThreshold = 55,
                OfficialSupplyInteractionResilienceReliefScore = -3,
                OfficialSupplyInteractionResilienceFallbackScore = 0,
                OfficialSupplyInteractionPressureClampFloor = -3,
                OfficialSupplyInteractionPressureClampCeiling = 5,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunInteractionPressureOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionBoatmanLivelihood, Is.EqualTo(LivelihoodType.Boatman));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionBoatmanSupplyPressureThreshold, Is.EqualTo(12));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionBoatmanBoostScore, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionLaborCapacityThreshold, Is.EqualTo(40));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionTenantDebtPressureThreshold, Is.EqualTo(60));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionResilienceReliefScore, Is.EqualTo(-3));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionPressureClampFloor, Is.EqualTo(-3));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyInteractionPressureClampCeiling, Is.EqualTo(5));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure], Is.EqualTo("2"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomInteractionPressureRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyInteractionBoatmanBoostScore = 0,
                OfficialSupplyInteractionLaborFragilityBoostScore = 0,
                OfficialSupplyInteractionTenantDebtBoostScore = 0,
                OfficialSupplyInteractionResilienceReliefScore = 0,
                OfficialSupplyInteractionPressureClampFloor = -3,
                OfficialSupplyInteractionPressureClampCeiling = 5,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunInteractionPressureOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure], Is.EqualTo("0"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]),
            Is.LessThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidInteractionPressureRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyInteractionLaborFragilityLivelihoods = new LivelihoodType[] { },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunInteractionPressureOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_interaction_labor_fragility_livelihoods must be non-empty, defined, and distinct."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyInteractionPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultDistressDeltaFormulaRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDistressDeltaSupplyPressureDivisor = 4,
                OfficialSupplyDistressDeltaLivelihoodExposureWeight = 1,
                OfficialSupplyDistressDeltaLaborPressureWeight = 1,
                OfficialSupplyDistressDeltaFragilityPressureWeight = 1,
                OfficialSupplyDistressDeltaClerkDistortionPressureDivisor = 3,
                OfficialSupplyDistressDeltaInteractionPressureWeight = 1,
                OfficialSupplyDistressDeltaResourceBufferWeight = 1,
                OfficialSupplyDistressDeltaAuthorityBufferDivisor = 3,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunInteractionPressureOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaSupplyPressureDivisor, Is.EqualTo(4));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaLivelihoodExposureWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaLaborPressureWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaFragilityPressureWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaClerkDistortionPressureDivisor, Is.EqualTo(3));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaInteractionPressureWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaResourceBufferWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDistressDeltaAuthorityBufferDivisor, Is.EqualTo(3));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo("7"));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomDistressDeltaFormulaRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDistressDeltaSupplyPressureDivisor = 1,
                OfficialSupplyDistressDeltaResourceBufferWeight = 0,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunInteractionPressureOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo("24"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]),
            Is.GreaterThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidDistressDeltaFormulaRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDistressDeltaSupplyPressureDivisor = 0,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunInteractionPressureOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_distress_delta_supply_pressure_divisor must be between 1 and 16."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDistressDelta]));
    }

    [Test]
    public void OfficialSupplyRequisition_DefaultDebtDeltaFormulaRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDebtDeltaQuotaPressureDivisor = 4,
                OfficialSupplyDebtDeltaLiquidityPressureWeight = 1,
                OfficialSupplyDebtDeltaFragilityPressureDivisor = 2,
                OfficialSupplyDebtDeltaInteractionPressureFloor = 0,
                OfficialSupplyDebtDeltaInteractionPressureWeight = 1,
                OfficialSupplyDebtDeltaClerkDistortionPressureDivisor = 4,
                OfficialSupplyDebtDeltaResourceBufferDivisor = 2,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunInteractionPressureOfficialSupply(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleBurdenEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaQuotaPressureDivisor, Is.EqualTo(4));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaLiquidityPressureWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaFragilityPressureDivisor, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaInteractionPressureFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaInteractionPressureWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaClerkDistortionPressureDivisor, Is.EqualTo(4));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultOfficialSupplyDebtDeltaResourceBufferDivisor, Is.EqualTo(2));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta], Is.EqualTo("4"));
    }

    [Test]
    public void OfficialSupplyRequisition_CustomDebtDeltaFormulaRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDebtDeltaQuotaPressureDivisor = 1,
                OfficialSupplyDebtDeltaResourceBufferDivisor = 16,
            };

        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> customEvents) = RunInteractionPressureOfficialSupply(customRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent customEvent = SingleBurdenEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta], Is.EqualTo("16"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta]));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta]),
            Is.GreaterThan(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta])));
    }

    [Test]
    public void OfficialSupplyRequisition_InvalidDebtDeltaFormulaRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                OfficialSupplyDebtDeltaQuotaPressureDivisor = 0,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (_, IReadOnlyList<IDomainEvent> defaultEvents) = RunInteractionPressureOfficialSupply();
        (_, IReadOnlyList<IDomainEvent> fallbackEvents) = RunInteractionPressureOfficialSupply(malformedRulesData);
        IDomainEvent defaultEvent = SingleBurdenEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleBurdenEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            validation.Errors,
            Does.Contain("official_supply_debt_delta_quota_pressure_divisor must be between 1 and 16."));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.OfficialSupplyDebtDelta]));
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunCrossingOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Zhang household",
            SettlementId = new SettlementId(1),
            Distress = 78,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "70",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "9",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "2",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "3",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunOutOfRangeSignalOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        return RunSignalNormalizationOfficialSupply(new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "120",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "40",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "30",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "25",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "20",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "15",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunNegativeSignalOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        return RunSignalNormalizationOfficialSupply(new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "-5",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "-5",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "-5",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "-5",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "-5",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "-5",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunSignalNormalizationOfficialSupply(
        IReadOnlyDictionary<string, string> metadata,
        PopulationHouseholdMobilityRulesData? rulesData)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Signal clamp household",
            SettlementId = new SettlementId(1),
            Distress = 78,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
        });

        return RunOfficialSupply(state, metadata, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunLivelihoodExposureOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null,
        LivelihoodType livelihood = LivelihoodType.Boatman,
        int landHolding = 70)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Livelihood exposure household",
            SettlementId = new SettlementId(1),
            Livelihood = livelihood,
            Distress = 78,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
            LandHolding = landHolding,
            GrainStore = 10,
            ToolCondition = 20,
            ShelterQuality = 80,
            DependentCount = 1,
            LaborerCount = 3,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunResourceBufferOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null,
        int grainStore = 85,
        int toolCondition = 80,
        int shelterQuality = 80)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Resource buffer household",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Smallholder,
            Distress = 78,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
            LandHolding = 50,
            GrainStore = grainStore,
            ToolCondition = toolCondition,
            ShelterQuality = shelterQuality,
            DependentCount = 1,
            LaborerCount = 3,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunLaborPressureOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null,
        int laborCapacity = 80,
        int dependentCount = 5,
        int laborerCount = 2)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Labor pressure household",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.HiredLabor,
            Distress = 78,
            DebtPressure = 30,
            LaborCapacity = laborCapacity,
            MigrationRisk = 20,
            LandHolding = 50,
            GrainStore = 85,
            ToolCondition = 80,
            ShelterQuality = 80,
            DependentCount = dependentCount,
            LaborerCount = laborerCount,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunLiquidityPressureOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null,
        int grainStore = 55,
        LivelihoodType livelihood = LivelihoodType.Boatman,
        int toolCondition = 20,
        int debtPressure = 65)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Liquidity pressure household",
            SettlementId = new SettlementId(1),
            Livelihood = livelihood,
            Distress = 78,
            DebtPressure = debtPressure,
            LaborCapacity = 80,
            MigrationRisk = 20,
            LandHolding = 50,
            GrainStore = grainStore,
            ToolCondition = toolCondition,
            ShelterQuality = 80,
            DependentCount = 1,
            LaborerCount = 3,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunFragilityPressureOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null,
        int distress = 79,
        int debtPressure = 80,
        int migrationRisk = 70,
        bool isMigrating = false,
        int shelterQuality = 20)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Fragility pressure household",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.SeasonalMigrant,
            Distress = distress,
            DebtPressure = debtPressure,
            LaborCapacity = 80,
            MigrationRisk = migrationRisk,
            IsMigrating = isMigrating,
            LandHolding = 50,
            GrainStore = 85,
            ToolCondition = 80,
            ShelterQuality = shelterQuality,
            DependentCount = 1,
            LaborerCount = 3,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunInteractionPressureOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null,
        LivelihoodType livelihood = LivelihoodType.Boatman,
        int supplyPressure = 16,
        int laborCapacity = 80,
        int debtPressure = 30,
        int distress = 78,
        int grainStore = 85)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Interaction pressure household",
            SettlementId = new SettlementId(1),
            Livelihood = livelihood,
            Distress = distress,
            DebtPressure = debtPressure,
            LaborCapacity = laborCapacity,
            MigrationRisk = 20,
            LandHolding = 50,
            GrainStore = grainStore,
            ToolCondition = 80,
            ShelterQuality = 80,
            DependentCount = 1,
            LaborerCount = 3,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = supplyPressure.ToString(),
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunFallbackOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Fallback signal household",
            SettlementId = new SettlementId(1),
            Distress = 78,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunLowThresholdOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Threshold smallholder",
            SettlementId = new SettlementId(1),
            Distress = 54,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "70",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "9",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "2",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "3",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunBufferedOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Buffered smallholder",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Smallholder,
            Distress = 40,
            DebtPressure = 30,
            LaborCapacity = 90,
            MigrationRisk = 20,
            LandHolding = 50,
            GrainStore = 85,
            ToolCondition = 80,
            ShelterQuality = 80,
            DependentCount = 1,
            LaborerCount = 3,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunLaborHeavyOfficialSupply(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Labor-scarce tenant",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Tenant,
            Distress = 40,
            DebtPressure = 60,
            LaborCapacity = 25,
            MigrationRisk = 20,
            LandHolding = 5,
            GrainStore = 10,
            ToolCondition = 20,
            ShelterQuality = 20,
            DependentCount = 5,
            LaborerCount = 1,
        });

        return RunOfficialSupply(state, new Dictionary<string, string>
        {
            [DomainEventMetadataKeys.SettlementId] = "1",
            [DomainEventMetadataKeys.FrontierPressure] = "76",
            [DomainEventMetadataKeys.OfficialSupplyPressure] = "16",
            [DomainEventMetadataKeys.OfficialSupplyQuotaPressure] = "12",
            [DomainEventMetadataKeys.OfficialSupplyDocketPressure] = "6",
            [DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure] = "4",
            [DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer] = "2",
        }, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunOfficialSupply(
        PopulationAndHouseholdsState state,
        IReadOnlyDictionary<string, string> metadata,
        PopulationHouseholdMobilityRulesData? rulesData)
    {
        PopulationAndHouseholdsModule module = rulesData is null
            ? new PopulationAndHouseholdsModule()
            : new PopulationAndHouseholdsModule(rulesData);

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

        buffer.Emit(MakeSupplyEvent(new SettlementId(1), metadata));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        return (state.Households.Single(static household => household.Id == new HouseholdId(1)), buffer.Events.ToList());
    }

    private static IDomainEvent SingleBurdenEvent(IReadOnlyList<IDomainEvent> events)
    {
        return events.Single(static e => e.EventType == PopulationEventNames.HouseholdBurdenIncreased);
    }

    private static DomainEventRecord MakeSupplyEvent(
        SettlementId settlementId,
        IReadOnlyDictionary<string, string> metadata)
    {
        return new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.OfficialSupplyRequisition,
            $"supply requisition for settlement {settlementId.Value}",
            settlementId.Value.ToString(),
            metadata);
    }
}
