using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds.Tests;

[TestFixture]
public sealed class TaxSeasonBurdenHandlerTests
{
    [Test]
    public void TaxSeasonOpened_UsesHouseholdExposureProfileAndKeepsOffScopeSettlementUntouched()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Buffered smallholder",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Smallholder,
            DebtPressure = 50,
            Distress = 35,
            LaborCapacity = 85,
            LandHolding = 70,
            GrainStore = 90,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Pressed tenant",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Tenant,
            DebtPressure = 50,
            Distress = 76,
            LaborCapacity = 25,
            LandHolding = 0,
            GrainStore = 10,
            DependentCount = 4,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(3),
            HouseholdName = "Offscope tenant",
            SettlementId = new SettlementId(2),
            Livelihood = LivelihoodType.Tenant,
            DebtPressure = 50,
            Distress = 76,
            LaborCapacity = 25,
            GrainStore = 10,
            DependentCount = 4,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.TaxSeasonOpened,
            "Settlement 1 enters tax season.",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        PopulationHouseholdState buffered = state.Households.Single(static h => h.Id == new HouseholdId(1));
        PopulationHouseholdState pressed = state.Households.Single(static h => h.Id == new HouseholdId(2));
        PopulationHouseholdState offscope = state.Households.Single(static h => h.Id == new HouseholdId(3));

        Assert.That(buffered.DebtPressure, Is.EqualTo(63));
        Assert.That(pressed.DebtPressure, Is.EqualTo(78));
        Assert.That(offscope.DebtPressure, Is.EqualTo(50), "Numeric settlement scope must not touch another settlement.");

        IDomainEvent debtEvent = buffer.Events.Single(e => e.EventType == PopulationEventNames.HouseholdDebtSpiked);
        Assert.That(debtEvent.EntityKey, Is.EqualTo("2"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseTaxSeason));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.SourceEventType], Is.EqualTo(WorldSettlementsEventNames.TaxSeasonOpened));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("1"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.DebtBefore], Is.EqualTo("50"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.DebtAfter], Is.EqualTo("78"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure], Is.EqualTo("4"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure], Is.EqualTo("4"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure], Is.EqualTo("3"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure], Is.EqualTo("2"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.TaxInteractionPressure], Is.EqualTo("2"));
        Assert.That(debtEvent.Metadata[DomainEventMetadataKeys.Livelihood], Is.EqualTo(nameof(LivelihoodType.Tenant)));
    }

    [Test]
    public void TaxSeasonOpened_GlobalSymbolicEntityKey_KeepsLegacyGlobalThinChainBehavior()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Zhang household",
            SettlementId = new SettlementId(1),
            DebtPressure = 55,
            Distress = 40,
            LaborCapacity = 50,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Li household",
            SettlementId = new SettlementId(2),
            DebtPressure = 55,
            Distress = 40,
            LaborCapacity = 50,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.TaxSeasonOpened,
            "Tax season opens.",
            "tax-season"));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Households[0].DebtPressure, Is.GreaterThanOrEqualTo(70));
        Assert.That(state.Households[1].DebtPressure, Is.GreaterThanOrEqualTo(70));
        Assert.That(
            buffer.Events.Count(static e => e.EventType == PopulationEventNames.HouseholdDebtSpiked),
            Is.EqualTo(2),
            "Current WorldSettlements emits symbolic tax-season, so this remains a global thin signal until settlement-scoped tax events land.");
    }
}
