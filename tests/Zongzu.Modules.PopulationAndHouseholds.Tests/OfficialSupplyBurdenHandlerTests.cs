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
