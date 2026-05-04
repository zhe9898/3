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
