using System.Collections.Generic;
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

    [Test]
    public void TaxSeasonOpened_DefaultRegistrationVisibilityRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonRegistrationVisibilityLivelihoodExposureScoreWeights =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonRegistrationVisibilityLivelihoodExposureScoreWeights,
                TaxSeasonRegistrationVisibilityLivelihoodExposureFallbackScore = 2,
                TaxSeasonRegistrationVisibilityLandVisibilityScoreBands =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonRegistrationVisibilityLandVisibilityScoreBands,
                TaxSeasonRegistrationVisibilityLandVisibilityFallbackScore = 0,
                TaxSeasonRegistrationVisibilityClampFloor = 1,
                TaxSeasonRegistrationVisibilityClampCeiling = 7,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState explicitHousehold, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunPressedTaxSeason(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleDebtSpikeEvent(explicitEvents);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultTaxSeasonRegistrationVisibilityLivelihoodExposureFallbackScore,
            Is.EqualTo(2));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultTaxSeasonRegistrationVisibilityLandVisibilityFallbackScore,
            Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonRegistrationVisibilityClampFloor, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonRegistrationVisibilityClampCeiling, Is.EqualTo(7));
        Assert.That(
            explicitPreviousBaseline.GetTaxSeasonRegistrationVisibilityLivelihoodExposureScoreOrDefault(LivelihoodType.Tenant),
            Is.EqualTo(4));
        Assert.That(
            explicitPreviousBaseline.GetTaxSeasonRegistrationVisibilityLandVisibilityScoreOrDefault(80),
            Is.EqualTo(4));
        Assert.That(explicitHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            explicitEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure], Is.EqualTo("4"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_CustomRegistrationVisibilityRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonRegistrationVisibilityLivelihoodExposureScoreWeights =
                    new[] { new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 1) },
                TaxSeasonRegistrationVisibilityLivelihoodExposureFallbackScore = 0,
                TaxSeasonRegistrationVisibilityLandVisibilityScoreBands =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonRegistrationVisibilityLandVisibilityScoreBands,
                TaxSeasonRegistrationVisibilityClampFloor = 0,
                TaxSeasonRegistrationVisibilityClampCeiling = 7,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunPressedTaxSeason(customRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent customEvent = SingleDebtSpikeEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure], Is.EqualTo("4"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure], Is.EqualTo("1"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("26"));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure - 2));
    }

    [Test]
    public void TaxSeasonOpened_InvalidRegistrationVisibilityRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonRegistrationVisibilityLivelihoodExposureScoreWeights =
                    new[] { new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 9) },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState fallbackHousehold, IReadOnlyList<IDomainEvent> fallbackEvents) =
            RunPressedTaxSeason(malformedRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleDebtSpikeEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetTaxSeasonRegistrationVisibilityLivelihoodExposureScoreOrDefault(LivelihoodType.Tenant),
            Is.EqualTo(4));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            fallbackEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxVisibilityPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_DefaultLiquidityRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonLiquidityGrainPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityGrainPressureBands,
                TaxSeasonLiquidityGrainPressureFallbackScore = 0,
                TaxSeasonLiquidityCashNeedScoreWeights =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityCashNeedScoreWeights,
                TaxSeasonLiquidityCashNeedFallbackScore = 0,
                TaxSeasonLiquidityToolDragConditionThreshold = 35,
                TaxSeasonLiquidityToolDragScore = 1,
                TaxSeasonLiquidityToolDragFallbackScore = 0,
                TaxSeasonLiquidityPressureClampFloor = -3,
                TaxSeasonLiquidityPressureClampCeiling = 5,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState explicitHousehold, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunPressedTaxSeason(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleDebtSpikeEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityGrainPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityCashNeedFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityToolDragConditionThreshold, Is.EqualTo(35));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityToolDragScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityPressureClampFloor, Is.EqualTo(-3));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLiquidityPressureClampCeiling, Is.EqualTo(5));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonLiquidityGrainPressureScoreOrDefault(10), Is.EqualTo(3));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonLiquidityCashNeedScoreOrDefault(LivelihoodType.Tenant), Is.EqualTo(1));
        Assert.That(explicitHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            explicitEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure], Is.EqualTo("4"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_CustomLiquidityRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonLiquidityGrainPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, 1) },
                TaxSeasonLiquidityCashNeedScoreWeights =
                    new[] { new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 0) },
                TaxSeasonLiquidityToolDragConditionThreshold = 35,
                TaxSeasonLiquidityPressureClampFloor = -3,
                TaxSeasonLiquidityPressureClampCeiling = 5,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunPressedTaxSeason(customRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent customEvent = SingleDebtSpikeEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure], Is.EqualTo("4"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure], Is.EqualTo("1"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("26"));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure - 2));
    }

    [Test]
    public void TaxSeasonOpened_InvalidLiquidityRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonLiquidityGrainPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, -9) },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState fallbackHousehold, IReadOnlyList<IDomainEvent> fallbackEvents) =
            RunPressedTaxSeason(malformedRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleDebtSpikeEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetTaxSeasonLiquidityGrainPressureScoreOrDefault(10), Is.EqualTo(3));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            fallbackEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLiquidityPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_DefaultLaborRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonLaborCapacityPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLaborCapacityPressureBands,
                TaxSeasonLaborCapacityPressureFallbackScore = 3,
                TaxSeasonDependentCountPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDependentCountPressureBands,
                TaxSeasonDependentCountPressureFallbackScore = 0,
                TaxSeasonDependentToLaborRatioMultiplier = 2,
                TaxSeasonDependentToLaborRatioBonusScore = 1,
                TaxSeasonDependentToLaborRatioFallbackScore = 0,
                TaxSeasonLaborPressureClampFloor = -2,
                TaxSeasonLaborPressureClampCeiling = 5,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState explicitHousehold, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunPressedTaxSeason(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleDebtSpikeEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLaborCapacityPressureFallbackScore, Is.EqualTo(3));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDependentCountPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDependentToLaborRatioMultiplier, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDependentToLaborRatioBonusScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLaborPressureClampFloor, Is.EqualTo(-2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonLaborPressureClampCeiling, Is.EqualTo(5));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonLaborCapacityPressureScoreOrDefault(25), Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonDependentCountPressureScoreOrDefault(4), Is.EqualTo(1));
        Assert.That(explicitHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            explicitEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure], Is.EqualTo("3"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_CustomLaborRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonLaborCapacityPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, 0) },
                TaxSeasonLaborCapacityPressureFallbackScore = 0,
                TaxSeasonDependentCountPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, 0) },
                TaxSeasonDependentCountPressureFallbackScore = 0,
                TaxSeasonDependentToLaborRatioMultiplier = 2,
                TaxSeasonDependentToLaborRatioBonusScore = 0,
                TaxSeasonDependentToLaborRatioFallbackScore = 0,
                TaxSeasonLaborPressureClampFloor = -2,
                TaxSeasonLaborPressureClampCeiling = 5,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunPressedTaxSeason(customRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent customEvent = SingleDebtSpikeEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure], Is.EqualTo("3"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure], Is.EqualTo("0"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("26"));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure - 2));
    }

    [Test]
    public void TaxSeasonOpened_InvalidLaborRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonLaborCapacityPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, -9) },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState fallbackHousehold, IReadOnlyList<IDomainEvent> fallbackEvents) =
            RunPressedTaxSeason(malformedRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleDebtSpikeEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetTaxSeasonLaborCapacityPressureScoreOrDefault(25), Is.EqualTo(2));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            fallbackEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxLaborPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_DefaultFragilityRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonFragilityDistressPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityDistressPressureBands,
                TaxSeasonFragilityDistressPressureFallbackScore = 0,
                TaxSeasonFragilityDebtPressureBands =
                    PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityDebtPressureBands,
                TaxSeasonFragilityDebtPressureFallbackScore = 0,
                TaxSeasonFragilityShelterDragQualityThreshold = 35,
                TaxSeasonFragilityShelterDragPressureScore = 1,
                TaxSeasonFragilityShelterDragPressureFallbackScore = 0,
                TaxSeasonFragilityMigrationRiskThreshold = 70,
                TaxSeasonFragilityMigrationPressureScore = 1,
                TaxSeasonFragilityMigrationPressureFallbackScore = 0,
                TaxSeasonFragilityPressureClampFloor = 0,
                TaxSeasonFragilityPressureClampCeiling = 7,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState explicitHousehold, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunPressedTaxSeason(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleDebtSpikeEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityDistressPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityDebtPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityShelterDragQualityThreshold, Is.EqualTo(35));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityShelterDragPressureScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityMigrationRiskThreshold, Is.EqualTo(70));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityMigrationPressureScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityPressureClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonFragilityPressureClampCeiling, Is.EqualTo(7));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonFragilityDistressPressureScoreOrDefault(76), Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonFragilityDebtPressureScoreOrDefault(55), Is.EqualTo(1));
        Assert.That(explicitHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            explicitEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure], Is.EqualTo("2"));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_CustomFragilityRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonFragilityDistressPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, 0) },
                TaxSeasonFragilityDebtPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, 0) },
                TaxSeasonFragilityShelterDragQualityThreshold = 35,
                TaxSeasonFragilityShelterDragPressureScore = 0,
                TaxSeasonFragilityMigrationRiskThreshold = 70,
                TaxSeasonFragilityMigrationPressureScore = 0,
                TaxSeasonFragilityPressureClampFloor = 0,
                TaxSeasonFragilityPressureClampCeiling = 7,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunPressedTaxSeason(customRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent customEvent = SingleDebtSpikeEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure], Is.EqualTo("2"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure], Is.EqualTo("0"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("27"));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure - 1));
    }

    [Test]
    public void TaxSeasonOpened_InvalidFragilityRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonFragilityDistressPressureBands =
                    new[] { new PopulationHouseholdMobilityThresholdScoreBand(1, 9) },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState fallbackHousehold, IReadOnlyList<IDomainEvent> fallbackEvents) =
            RunPressedTaxSeason(malformedRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleDebtSpikeEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetTaxSeasonFragilityDistressPressureScoreOrDefault(76), Is.EqualTo(2));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(
            fallbackEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure],
            Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxFragilityPressure]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_DefaultTaxDebtDeltaClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonDebtDeltaClampFloor = 8,
                TaxSeasonDebtDeltaClampCeiling = 28,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState explicitHousehold, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunPressedTaxSeason(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleDebtSpikeEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDebtDeltaClampFloor, Is.EqualTo(8));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDebtDeltaClampCeiling, Is.EqualTo(28));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonDebtDeltaClampFloorOrDefault(), Is.EqualTo(8));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonDebtDeltaClampCeilingOrDefault(), Is.EqualTo(28));
        Assert.That(explicitHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_CustomTaxDebtDeltaClampFloorRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonDebtDeltaClampFloor = 20,
                TaxSeasonDebtDeltaClampCeiling = 28,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunResilientTaxSeason();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunResilientTaxSeason(customRulesData);
        IDomainEvent customEvent = SingleDebtSpikeEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.DebtPressure, Is.EqualTo(65));
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdDebtSpiked), Is.EqualTo(0));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(72));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("20"));
    }

    [Test]
    public void TaxSeasonOpened_CustomTaxDebtDeltaClampCeilingRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonDebtDeltaClampFloor = 8,
                TaxSeasonDebtDeltaClampCeiling = 20,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunPressedTaxSeason(customRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent customEvent = SingleDebtSpikeEvent(customEvents);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("20"));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure - 8));
    }

    [Test]
    public void TaxSeasonOpened_InvalidTaxDebtDeltaClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonDebtDeltaClampFloor = 29,
                TaxSeasonDebtDeltaClampCeiling = 28,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState fallbackHousehold, IReadOnlyList<IDomainEvent> fallbackEvents) =
            RunPressedTaxSeason(malformedRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleDebtSpikeEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetTaxSeasonDebtDeltaClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDebtDeltaClampFloor));
        Assert.That(
            malformedRulesData.GetTaxSeasonDebtDeltaClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDebtDeltaClampCeiling));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.TaxDebtDelta], Is.EqualTo("28"));
    }

    [Test]
    public void TaxSeasonOpened_DefaultDebtSpikeEventThresholdRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonDebtSpikeEventThreshold = 70,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState explicitHousehold, IReadOnlyList<IDomainEvent> explicitEvents) =
            RunPressedTaxSeason(explicitPreviousBaseline);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent explicitEvent = SingleDebtSpikeEvent(explicitEvents);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDebtSpikeEventThreshold, Is.EqualTo(70));
        Assert.That(explicitPreviousBaseline.GetTaxSeasonDebtSpikeEventThresholdOrDefault(), Is.EqualTo(70));
        Assert.That(explicitHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(explicitEvent.Metadata[DomainEventMetadataKeys.DebtAfter], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.DebtAfter]));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.DebtAfter], Is.EqualTo("78"));
    }

    [Test]
    public void TaxSeasonOpened_CustomDebtSpikeEventThresholdRulesDataIsOwnerConsumedWithoutChangingDebt()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonDebtSpikeEventThreshold = 80,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunPressedTaxSeason(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(defaultHousehold.DebtPressure, Is.EqualTo(78));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(defaultEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdDebtSpiked), Is.EqualTo(1));
        Assert.That(customEvents.Count(static e => e.EventType == PopulationEventNames.HouseholdDebtSpiked), Is.EqualTo(0));
    }

    [Test]
    public void TaxSeasonOpened_InvalidDebtSpikeEventThresholdRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                TaxSeasonDebtSpikeEventThreshold = 101,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) = RunPressedTaxSeason();
        (PopulationHouseholdState fallbackHousehold, IReadOnlyList<IDomainEvent> fallbackEvents) =
            RunPressedTaxSeason(malformedRulesData);
        IDomainEvent defaultEvent = SingleDebtSpikeEvent(defaultEvents);
        IDomainEvent fallbackEvent = SingleDebtSpikeEvent(fallbackEvents);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetTaxSeasonDebtSpikeEventThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultTaxSeasonDebtSpikeEventThreshold));
        Assert.That(fallbackHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.DebtAfter], Is.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.DebtAfter]));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.DebtAfter], Is.EqualTo("78"));
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunPressedTaxSeason(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
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

        return RunTaxSeason(state, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunResilientTaxSeason(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Buffered smallholder",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Smallholder,
            DebtPressure = 52,
            Distress = 35,
            LaborCapacity = 85,
            LandHolding = 70,
            GrainStore = 90,
        });

        return RunTaxSeason(state, rulesData);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunTaxSeason(
        PopulationAndHouseholdsState state,
        PopulationHouseholdMobilityRulesData? rulesData)
    {
        PopulationAndHouseholdsModule module = rulesData is null
            ? new PopulationAndHouseholdsModule()
            : new PopulationAndHouseholdsModule(rulesData);

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

        return (state.Households.Single(static household => household.Id == new HouseholdId(1)), buffer.Events.ToList());
    }

    private static IDomainEvent SingleDebtSpikeEvent(IReadOnlyList<IDomainEvent> events)
    {
        return events.Single(static e => e.EventType == PopulationEventNames.HouseholdDebtSpiked);
    }
}
