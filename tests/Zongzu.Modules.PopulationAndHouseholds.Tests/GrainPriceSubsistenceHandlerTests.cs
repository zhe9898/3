using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds.Tests;

[TestFixture]
public sealed class GrainPriceSubsistenceHandlerTests
{
    [Test]
    public void GrainPriceSpike_UsesHouseholdSubsistenceProfileAndKeepsOffScopeSettlementUntouched()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Buffered smallholder",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Smallholder,
            Distress = 45,
            DebtPressure = 35,
            LaborCapacity = 85,
            LandHolding = 70,
            GrainStore = 90,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(2),
            HouseholdName = "Pressed petty trader",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.PettyTrader,
            Distress = 50,
            DebtPressure = 65,
            LaborCapacity = 25,
            GrainStore = 10,
            DependentCount = 4,
            MigrationRisk = 75,
        });
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(3),
            HouseholdName = "Offscope petty trader",
            SettlementId = new SettlementId(2),
            Livelihood = LivelihoodType.PettyTrader,
            Distress = 50,
            DebtPressure = 65,
            LaborCapacity = 25,
            GrainStore = 10,
            DependentCount = 4,
            MigrationRisk = 75,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 9),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.TradeAndIndustry,
            TradeAndIndustryEventNames.GrainPriceSpike,
            "Settlement 1 grain price spike.",
            "1",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.SettlementId] = "1",
                [DomainEventMetadataKeys.GrainOldPrice] = "110",
                [DomainEventMetadataKeys.GrainCurrentPrice] = "140",
                [DomainEventMetadataKeys.GrainPriceDelta] = "30",
                [DomainEventMetadataKeys.GrainSupply] = "20",
                [DomainEventMetadataKeys.GrainDemand] = "85",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        PopulationHouseholdState buffered = state.Households.Single(static h => h.Id == new HouseholdId(1));
        PopulationHouseholdState pressed = state.Households.Single(static h => h.Id == new HouseholdId(2));
        PopulationHouseholdState offscope = state.Households.Single(static h => h.Id == new HouseholdId(3));

        Assert.That(buffered.Distress, Is.EqualTo(49));
        Assert.That(pressed.Distress, Is.EqualTo(79));
        Assert.That(offscope.Distress, Is.EqualTo(50), "A local grain price spike must not touch another settlement.");

        IDomainEvent subsistenceEvent = buffer.Events.Single(e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged);
        Assert.That(subsistenceEvent.EntityKey, Is.EqualTo("2"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseGrainPriceSpike));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SourceEventType], Is.EqualTo(TradeAndIndustryEventNames.GrainPriceSpike));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("1"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.DistressBefore], Is.EqualTo("50"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.DistressAfter], Is.EqualTo("79"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta], Is.EqualTo("29"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("11"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceGrainBufferPressure], Is.EqualTo("5"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceMarketDependencyPressure], Is.EqualTo("4"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("2"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainCurrentPrice], Is.EqualTo("140"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainPriceDelta], Is.EqualTo("30"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainSupply], Is.EqualTo("20"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainDemand], Is.EqualTo("85"));
        Assert.That(subsistenceEvent.Metadata[DomainEventMetadataKeys.Livelihood], Is.EqualTo(nameof(LivelihoodType.PettyTrader)));
    }

    [Test]
    public void GrainPriceSpike_DefaultSignalRulesDataMatchesExplicitPreviousFallbackBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceShockDefaultCurrentPrice = 130,
                GrainPriceShockDefaultOldPrice = 100,
                GrainPriceShockDefaultSupply = 50,
                GrainPriceShockDefaultDemand = 70,
                GrainPriceShockCurrentPriceClampFloor = 50,
                GrainPriceShockCurrentPriceClampCeiling = 200,
                GrainPriceShockPriceDeltaClampFloor = 0,
                GrainPriceShockPriceDeltaClampCeiling = 150,
                GrainPriceShockSupplyClampFloor = 0,
                GrainPriceShockSupplyClampCeiling = 100,
                GrainPriceShockDemandClampFloor = 0,
                GrainPriceShockDemandClampCeiling = 100,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockDefaultCurrentPrice, Is.EqualTo(130));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockDefaultOldPrice, Is.EqualTo(100));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockDefaultSupply, Is.EqualTo(50));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockDefaultDemand, Is.EqualTo(70));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockCurrentPriceClampFloor, Is.EqualTo(50));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockCurrentPriceClampCeiling, Is.EqualTo(200));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockPriceDeltaClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockPriceDeltaClampCeiling, Is.EqualTo(150));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockSupplyClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockSupplyClampCeiling, Is.EqualTo(100));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockDemandClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockDemandClampCeiling, Is.EqualTo(100));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.GrainCurrentPrice], Is.EqualTo("130"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.GrainPriceDelta], Is.EqualTo("30"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.GrainSupply], Is.EqualTo("50"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.GrainDemand], Is.EqualTo("70"));
    }

    [Test]
    public void GrainPriceSpike_InvalidSignalRulesDataFallsBackToPreviousFallbackBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceShockCurrentPriceClampFloor = 250,
                GrainPriceShockCurrentPriceClampCeiling = 100,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetGrainPriceShockCurrentPriceClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockCurrentPriceClampFloor));
        Assert.That(
            malformedRulesData.GetGrainPriceShockCurrentPriceClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultGrainPriceShockCurrentPriceClampCeiling));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.GrainCurrentPrice], Is.EqualTo("130"));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.GrainPriceDelta], Is.EqualTo("30"));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.GrainSupply], Is.EqualTo("50"));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.GrainDemand], Is.EqualTo("70"));
    }

    [Test]
    public void GrainPriceSpike_DefaultPricePressureClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPricePressureClampFloor = 4,
                GrainPricePressureClampCeiling = 14,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPricePressureClampFloor, Is.EqualTo(4));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPricePressureClampCeiling, Is.EqualTo(14));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_InvalidPricePressureClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPricePressureClampFloor = 20,
                GrainPricePressureClampCeiling = 5,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetGrainPricePressureClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultGrainPricePressureClampFloor));
        Assert.That(
            malformedRulesData.GetGrainPricePressureClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultGrainPricePressureClampCeiling));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_DefaultPriceLevelBandRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceLevelPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(170, 7),
                    new PopulationHouseholdMobilityThresholdScoreBand(150, 5),
                    new PopulationHouseholdMobilityThresholdScoreBand(130, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(120, 2),
                },
                GrainPriceLevelPressureFallbackScore = 1,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceLevelPressureFallbackScore, Is.EqualTo(1));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultGrainPriceLevelPressureBands,
            Is.EqualTo(new[]
            {
                new PopulationHouseholdMobilityThresholdScoreBand(170, 7),
                new PopulationHouseholdMobilityThresholdScoreBand(150, 5),
                new PopulationHouseholdMobilityThresholdScoreBand(130, 3),
                new PopulationHouseholdMobilityThresholdScoreBand(120, 2),
            }));
        Assert.That(explicitPreviousBaseline.GetGrainPriceLevelPressureScoreOrDefault(130), Is.EqualTo(3));
        Assert.That(explicitPreviousBaseline.GetGrainPriceLevelPressureScoreOrDefault(119), Is.EqualTo(1));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_InvalidPriceLevelBandRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceLevelPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(130, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(170, 7),
                },
                GrainPriceLevelPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetGrainPriceLevelPressureScoreOrDefault(130), Is.EqualTo(3));
        Assert.That(malformedRulesData.GetGrainPriceLevelPressureFallbackScoreOrDefault(), Is.EqualTo(1));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_DefaultPriceJumpBandRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceJumpPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(45, 5),
                    new PopulationHouseholdMobilityThresholdScoreBand(30, 4),
                    new PopulationHouseholdMobilityThresholdScoreBand(18, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(8, 1),
                },
                GrainPriceJumpPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceJumpPressureFallbackScore, Is.EqualTo(0));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultGrainPriceJumpPressureBands,
            Is.EqualTo(new[]
            {
                new PopulationHouseholdMobilityThresholdScoreBand(45, 5),
                new PopulationHouseholdMobilityThresholdScoreBand(30, 4),
                new PopulationHouseholdMobilityThresholdScoreBand(18, 2),
                new PopulationHouseholdMobilityThresholdScoreBand(8, 1),
            }));
        Assert.That(explicitPreviousBaseline.GetGrainPriceJumpPressureScoreOrDefault(30), Is.EqualTo(4));
        Assert.That(explicitPreviousBaseline.GetGrainPriceJumpPressureScoreOrDefault(7), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_InvalidPriceJumpBandRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceJumpPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(18, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(45, 5),
                },
                GrainPriceJumpPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetGrainPriceJumpPressureScoreOrDefault(30), Is.EqualTo(4));
        Assert.That(malformedRulesData.GetGrainPriceJumpPressureFallbackScoreOrDefault(), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_DefaultMarketTightnessBandRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceMarketTightnessPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(60, 4),
                    new PopulationHouseholdMobilityThresholdScoreBand(40, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(20, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(8, 1),
                },
                GrainPriceMarketTightnessPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultGrainPriceMarketTightnessPressureFallbackScore, Is.EqualTo(0));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultGrainPriceMarketTightnessPressureBands,
            Is.EqualTo(new[]
            {
                new PopulationHouseholdMobilityThresholdScoreBand(60, 4),
                new PopulationHouseholdMobilityThresholdScoreBand(40, 3),
                new PopulationHouseholdMobilityThresholdScoreBand(20, 2),
                new PopulationHouseholdMobilityThresholdScoreBand(8, 1),
            }));
        Assert.That(explicitPreviousBaseline.GetGrainPriceMarketTightnessPressureScoreOrDefault(20), Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetGrainPriceMarketTightnessPressureScoreOrDefault(7), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_InvalidMarketTightnessBandRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                GrainPriceMarketTightnessPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(20, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(60, 4),
                },
                GrainPriceMarketTightnessPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetGrainPriceMarketTightnessPressureScoreOrDefault(20), Is.EqualTo(2));
        Assert.That(malformedRulesData.GetGrainPriceMarketTightnessPressureFallbackScoreOrDefault(), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure], Is.EqualTo("9"));
    }

    [Test]
    public void GrainPriceSpike_DefaultGrainBufferRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceGrainBufferPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(85, -5),
                    new PopulationHouseholdMobilityThresholdScoreBand(65, -3),
                    new PopulationHouseholdMobilityThresholdScoreBand(45, -1),
                    new PopulationHouseholdMobilityThresholdScoreBand(25, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(1, 5),
                },
                SubsistenceGrainBufferPressureFallbackScore = 6,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceGrainBufferPressureFallbackScore, Is.EqualTo(6));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceGrainBufferPressureBands, Is.EqualTo(explicitPreviousBaseline.SubsistenceGrainBufferPressureBands));
        Assert.That(explicitPreviousBaseline.GetSubsistenceGrainBufferPressureScoreOrDefault(90), Is.EqualTo(-5));
        Assert.That(explicitPreviousBaseline.GetSubsistenceGrainBufferPressureScoreOrDefault(70), Is.EqualTo(-3));
        Assert.That(explicitPreviousBaseline.GetSubsistenceGrainBufferPressureScoreOrDefault(50), Is.EqualTo(-1));
        Assert.That(explicitPreviousBaseline.GetSubsistenceGrainBufferPressureScoreOrDefault(30), Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceGrainBufferPressureScoreOrDefault(10), Is.EqualTo(5));
        Assert.That(explicitPreviousBaseline.GetSubsistenceGrainBufferPressureScoreOrDefault(0), Is.EqualTo(6));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceGrainBufferPressure], Is.EqualTo("5"));
    }

    [Test]
    public void GrainPriceSpike_CustomGrainBufferRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceGrainBufferPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(85, -5),
                    new PopulationHouseholdMobilityThresholdScoreBand(65, -3),
                    new PopulationHouseholdMobilityThresholdScoreBand(45, -1),
                    new PopulationHouseholdMobilityThresholdScoreBand(25, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(1, 4),
                },
                SubsistenceGrainBufferPressureFallbackScore = 6,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceGrainBufferPressure], Is.EqualTo("4"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 1));
        Assert.That(customHousehold.Distress, Is.EqualTo(defaultHousehold.Distress - 1));
    }

    [Test]
    public void GrainPriceSpike_InvalidGrainBufferRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceGrainBufferPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(85, -5),
                    new PopulationHouseholdMobilityThresholdScoreBand(85, -3),
                },
                SubsistenceGrainBufferPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetSubsistenceGrainBufferPressureScoreOrDefault(10), Is.EqualTo(5));
        Assert.That(malformedRulesData.GetSubsistenceGrainBufferPressureFallbackScoreOrDefault(), Is.EqualTo(6));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceGrainBufferPressure], Is.EqualTo("5"));
    }

    [Test]
    public void GrainPriceSpike_DefaultMarketDependencyRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceMarketDependencyPressureScoreWeights = new[]
                {
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.PettyTrader, 4),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Boatman, 4),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Artisan, 3),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.HiredLabor, 3),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.SeasonalMigrant, 3),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.DomesticServant, 2),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.YamenRunner, 2),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Vagrant, 2),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 2),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Unknown, 2),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Smallholder, 1),
                },
                SubsistenceMarketDependencyPressureFallbackScore = 2,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceMarketDependencyPressureFallbackScore, Is.EqualTo(2));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultSubsistenceMarketDependencyPressureScoreWeights,
            Is.EqualTo(explicitPreviousBaseline.SubsistenceMarketDependencyPressureScoreWeights));
        Assert.That(explicitPreviousBaseline.GetSubsistenceMarketDependencyPressureScoreOrDefault(LivelihoodType.PettyTrader), Is.EqualTo(4));
        Assert.That(explicitPreviousBaseline.GetSubsistenceMarketDependencyPressureScoreOrDefault(LivelihoodType.Smallholder), Is.EqualTo(1));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceMarketDependencyPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_InvalidMarketDependencyRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceMarketDependencyPressureScoreWeights = new[]
                {
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.PettyTrader, 4),
                    new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.PettyTrader, 3),
                },
                SubsistenceMarketDependencyPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetSubsistenceMarketDependencyPressureScoreOrDefault(LivelihoodType.PettyTrader), Is.EqualTo(4));
        Assert.That(malformedRulesData.GetSubsistenceMarketDependencyPressureFallbackScoreOrDefault(), Is.EqualTo(2));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceMarketDependencyPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_DefaultLaborCapacityRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceLaborCapacityPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, -2),
                    new PopulationHouseholdMobilityThresholdScoreBand(60, -1),
                    new PopulationHouseholdMobilityThresholdScoreBand(40, 0),
                    new PopulationHouseholdMobilityThresholdScoreBand(25, 1),
                },
                SubsistenceLaborCapacityPressureFallbackScore = 2,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceLaborCapacityPressureFallbackScore, Is.EqualTo(2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceLaborCapacityPressureBands, Is.EqualTo(explicitPreviousBaseline.SubsistenceLaborCapacityPressureBands));
        Assert.That(explicitPreviousBaseline.GetSubsistenceLaborCapacityPressureScoreOrDefault(85), Is.EqualTo(-2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceLaborCapacityPressureScoreOrDefault(60), Is.EqualTo(-1));
        Assert.That(explicitPreviousBaseline.GetSubsistenceLaborCapacityPressureScoreOrDefault(40), Is.EqualTo(0));
        Assert.That(explicitPreviousBaseline.GetSubsistenceLaborCapacityPressureScoreOrDefault(25), Is.EqualTo(1));
        Assert.That(explicitPreviousBaseline.GetSubsistenceLaborCapacityPressureScoreOrDefault(10), Is.EqualTo(2));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("2"));
    }

    [Test]
    public void GrainPriceSpike_InvalidLaborCapacityRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceLaborCapacityPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, -2),
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 0),
                },
                SubsistenceLaborCapacityPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetSubsistenceLaborCapacityPressureScoreOrDefault(25), Is.EqualTo(1));
        Assert.That(malformedRulesData.GetSubsistenceLaborCapacityPressureFallbackScoreOrDefault(), Is.EqualTo(2));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("2"));
    }

    [Test]
    public void GrainPriceSpike_DefaultDependentCountRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceDependentCountPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(5, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(3, 1),
                },
                SubsistenceDependentCountPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceDependentCountPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceDependentCountPressureBands, Is.EqualTo(explicitPreviousBaseline.SubsistenceDependentCountPressureBands));
        Assert.That(explicitPreviousBaseline.GetSubsistenceDependentCountPressureScoreOrDefault(5), Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceDependentCountPressureScoreOrDefault(3), Is.EqualTo(1));
        Assert.That(explicitPreviousBaseline.GetSubsistenceDependentCountPressureScoreOrDefault(0), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("2"));
    }

    [Test]
    public void GrainPriceSpike_InvalidDependentCountRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceDependentCountPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(5, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(5, 1),
                },
                SubsistenceDependentCountPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetSubsistenceDependentCountPressureScoreOrDefault(4), Is.EqualTo(1));
        Assert.That(malformedRulesData.GetSubsistenceDependentCountPressureFallbackScoreOrDefault(), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("2"));
    }

    [Test]
    public void GrainPriceSpike_DefaultLaborClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceLaborPressureClampFloor = -2,
                SubsistenceLaborPressureClampCeiling = 4,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceLaborPressureClampFloor, Is.EqualTo(-2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceLaborPressureClampCeiling, Is.EqualTo(4));
        Assert.That(explicitPreviousBaseline.GetSubsistenceLaborPressureClampFloorOrDefault(), Is.EqualTo(-2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceLaborPressureClampCeilingOrDefault(), Is.EqualTo(4));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("2"));
    }

    [Test]
    public void GrainPriceSpike_CustomLaborClampRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceLaborPressureClampFloor = 0,
                SubsistenceLaborPressureClampCeiling = 1,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("1"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 1));
        Assert.That(customHousehold.Distress, Is.EqualTo(defaultHousehold.Distress - 1));
    }

    [Test]
    public void GrainPriceSpike_InvalidLaborClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceLaborPressureClampFloor = 3,
                SubsistenceLaborPressureClampCeiling = 2,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetSubsistenceLaborPressureClampFloorOrDefault(), Is.EqualTo(-2));
        Assert.That(malformedRulesData.GetSubsistenceLaborPressureClampCeilingOrDefault(), Is.EqualTo(4));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure], Is.EqualTo("2"));
    }

    [Test]
    public void GrainPriceSpike_DefaultFragilityDistressRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityDistressPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(65, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(50, 1),
                },
                SubsistenceFragilityDistressPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityDistressPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityDistressPressureBands, Is.EqualTo(explicitPreviousBaseline.SubsistenceFragilityDistressPressureBands));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDistressPressureScoreOrDefault(80), Is.EqualTo(3));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDistressPressureScoreOrDefault(65), Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDistressPressureScoreOrDefault(50), Is.EqualTo(1));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDistressPressureScoreOrDefault(49), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_CustomFragilityDistressRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityDistressPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(65, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(50, 2),
                },
                SubsistenceFragilityDistressPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("5"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) + 1));
        Assert.That(customHousehold.Distress, Is.EqualTo(defaultHousehold.Distress + 1));
    }

    [Test]
    public void GrainPriceSpike_InvalidFragilityDistressRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityDistressPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 2),
                },
                SubsistenceFragilityDistressPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetSubsistenceFragilityDistressPressureScoreOrDefault(50), Is.EqualTo(1));
        Assert.That(malformedRulesData.GetSubsistenceFragilityDistressPressureFallbackScoreOrDefault(), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_DefaultFragilityDebtRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityDebtPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(65, 2),
                    new PopulationHouseholdMobilityThresholdScoreBand(50, 1),
                },
                SubsistenceFragilityDebtPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityDebtPressureFallbackScore, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityDebtPressureBands, Is.EqualTo(explicitPreviousBaseline.SubsistenceFragilityDebtPressureBands));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDebtPressureScoreOrDefault(80), Is.EqualTo(3));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDebtPressureScoreOrDefault(65), Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDebtPressureScoreOrDefault(50), Is.EqualTo(1));
        Assert.That(explicitPreviousBaseline.GetSubsistenceFragilityDebtPressureScoreOrDefault(49), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_CustomFragilityDebtRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityDebtPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(65, 1),
                    new PopulationHouseholdMobilityThresholdScoreBand(50, 1),
                },
                SubsistenceFragilityDebtPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("3"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 1));
        Assert.That(customHousehold.Distress, Is.EqualTo(defaultHousehold.Distress - 1));
    }

    [Test]
    public void GrainPriceSpike_InvalidFragilityDebtRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityDebtPressureBands = new[]
                {
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 3),
                    new PopulationHouseholdMobilityThresholdScoreBand(80, 2),
                },
                SubsistenceFragilityDebtPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(malformedRulesData.GetSubsistenceFragilityDebtPressureScoreOrDefault(65), Is.EqualTo(2));
        Assert.That(malformedRulesData.GetSubsistenceFragilityDebtPressureFallbackScoreOrDefault(), Is.EqualTo(0));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_DefaultFragilityMigrationRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityMigrationRiskThreshold = 70,
                SubsistenceFragilityMigrationPressureScore = 1,
                SubsistenceFragilityMigrationPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityMigrationRiskThreshold, Is.EqualTo(70));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityMigrationPressureScore, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityMigrationPressureFallbackScore, Is.EqualTo(0));
        Assert.That(
            explicitPreviousBaseline.GetSubsistenceFragilityMigrationPressureScoreOrDefault(false, 70),
            Is.EqualTo(1));
        Assert.That(
            explicitPreviousBaseline.GetSubsistenceFragilityMigrationPressureScoreOrDefault(false, 69),
            Is.EqualTo(0));
        Assert.That(
            explicitPreviousBaseline.GetSubsistenceFragilityMigrationPressureScoreOrDefault(true, 0),
            Is.EqualTo(1));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_CustomFragilityMigrationRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityMigrationRiskThreshold = 80,
                SubsistenceFragilityMigrationPressureScore = 1,
                SubsistenceFragilityMigrationPressureFallbackScore = 0,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customRulesData.GetSubsistenceFragilityMigrationPressureScoreOrDefault(false, 75), Is.EqualTo(0));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("3"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 1));
        Assert.That(customHousehold.MigrationRisk, Is.EqualTo(defaultHousehold.MigrationRisk));
        Assert.That(customHousehold.IsMigrating, Is.EqualTo(defaultHousehold.IsMigrating));
    }

    [Test]
    public void GrainPriceSpike_InvalidFragilityMigrationRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityMigrationRiskThreshold = 101,
                SubsistenceFragilityMigrationPressureScore = 99,
                SubsistenceFragilityMigrationPressureFallbackScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistenceFragilityMigrationRiskThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityMigrationRiskThreshold));
        Assert.That(malformedRulesData.GetSubsistenceFragilityMigrationPressureScoreOrDefault(false, 75), Is.EqualTo(1));
        Assert.That(
            malformedRulesData.GetSubsistenceFragilityMigrationPressureFallbackScoreOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityMigrationPressureFallbackScore));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_DefaultFragilityClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityPressureClampFloor = 0,
                SubsistenceFragilityPressureClampCeiling = 7,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityPressureClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityPressureClampCeiling, Is.EqualTo(7));
        Assert.That(
            explicitPreviousBaseline.GetSubsistenceFragilityPressureClampFloorOrDefault(),
            Is.EqualTo(0));
        Assert.That(
            explicitPreviousBaseline.GetSubsistenceFragilityPressureClampCeilingOrDefault(),
            Is.EqualTo(7));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_CustomFragilityClampRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityPressureClampFloor = 0,
                SubsistenceFragilityPressureClampCeiling = 3,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("3"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 1));
        Assert.That(customHousehold.MigrationRisk, Is.EqualTo(defaultHousehold.MigrationRisk));
    }

    [Test]
    public void GrainPriceSpike_InvalidFragilityClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceFragilityPressureClampFloor = 8,
                SubsistenceFragilityPressureClampCeiling = 7,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistenceFragilityPressureClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityPressureClampFloor));
        Assert.That(
            malformedRulesData.GetSubsistenceFragilityPressureClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceFragilityPressureClampCeiling));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure], Is.EqualTo("4"));
    }

    [Test]
    public void GrainPriceSpike_DefaultInteractionGrainShortageRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionGrainShortageStoreFloorExclusive = 0,
                SubsistenceInteractionGrainShortageStoreCeilingExclusive = 25,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionGrainShortageStoreFloorExclusive,
            Is.EqualTo(0));
        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionGrainShortageStoreCeilingExclusive,
            Is.EqualTo(25));
        Assert.That(explicitPreviousBaseline.IsSubsistenceInteractionGrainShortageStoreOrDefault(10), Is.True);
        Assert.That(explicitPreviousBaseline.IsSubsistenceInteractionGrainShortageStoreOrDefault(0), Is.False);
        Assert.That(explicitPreviousBaseline.IsSubsistenceInteractionGrainShortageStoreOrDefault(25), Is.False);
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionGrainShortageRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionGrainShortageStoreFloorExclusive = 0,
                SubsistenceInteractionGrainShortageStoreCeilingExclusive = 5,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customRulesData.IsSubsistenceInteractionGrainShortageStoreOrDefault(10), Is.False);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("0"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 3));
        Assert.That(customHousehold.GrainStore, Is.EqualTo(defaultHousehold.GrainStore));
    }

    [Test]
    public void GrainPriceSpike_InvalidInteractionGrainShortageRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionGrainShortageStoreFloorExclusive = 25,
                SubsistenceInteractionGrainShortageStoreCeilingExclusive = 25,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionGrainShortageStoreFloorExclusiveOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionGrainShortageStoreFloorExclusive));
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionGrainShortageStoreCeilingExclusiveOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionGrainShortageStoreCeilingExclusive));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_DefaultInteractionCashNeedRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionCashNeedBoostScore = 2,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionCashNeedBoostScore, Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceInteractionCashNeedBoostScoreOrDefault(), Is.EqualTo(2));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionCashNeedRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionCashNeedBoostScore = 1,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("2"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 1));
        Assert.That(customHousehold.Livelihood, Is.EqualTo(defaultHousehold.Livelihood));
    }

    [Test]
    public void GrainPriceSpike_InvalidInteractionCashNeedRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionCashNeedBoostScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionCashNeedBoostScoreOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionCashNeedBoostScore));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_DefaultInteractionDebtPressureRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionDebtPressureThreshold = 60,
                SubsistenceInteractionDebtPressureBoostScore = 1,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionDebtPressureThreshold, Is.EqualTo(60));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionDebtPressureBoostScore, Is.EqualTo(1));
        Assert.That(explicitPreviousBaseline.GetSubsistenceInteractionDebtPressureThresholdOrDefault(), Is.EqualTo(60));
        Assert.That(explicitPreviousBaseline.GetSubsistenceInteractionDebtPressureBoostScoreOrDefault(), Is.EqualTo(1));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionDebtPressureThresholdRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionDebtPressureThreshold = 70,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customRulesData.IsSubsistenceInteractionDebtPressureOrDefault(65), Is.False);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("2"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) - 1));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionDebtPressureBoostRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionDebtPressureBoostScore = 2,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customRulesData.IsSubsistenceInteractionDebtPressureOrDefault(65), Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("4"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) + 1));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
    }

    [Test]
    public void GrainPriceSpike_InvalidInteractionDebtPressureRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionDebtPressureThreshold = 101,
                SubsistenceInteractionDebtPressureBoostScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionDebtPressureThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionDebtPressureThreshold));
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionDebtPressureBoostScoreOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionDebtPressureBoostScore));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_DefaultInteractionResilienceReliefRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionResilienceReliefGrainStoreThreshold = 75,
                SubsistenceInteractionResilienceReliefLandHoldingThreshold = 35,
                SubsistenceInteractionResilienceReliefLaborCapacityThreshold = 60,
                SubsistenceInteractionResilienceReliefScore = 2,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunResilientGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunResilientGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionResilienceReliefGrainStoreThreshold, Is.EqualTo(75));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionResilienceReliefLandHoldingThreshold, Is.EqualTo(35));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionResilienceReliefLaborCapacityThreshold, Is.EqualTo(60));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionResilienceReliefScore, Is.EqualTo(2));
        Assert.That(explicitPreviousBaseline.IsSubsistenceInteractionResilienceReliefOrDefault(90, 40, 65), Is.True);
        Assert.That(explicitPreviousBaseline.GetSubsistenceInteractionResilienceReliefScoreOrDefault(), Is.EqualTo(2));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("-2"));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionResilienceThresholdRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionResilienceReliefGrainStoreThreshold = 95,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunResilientGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunResilientGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customRulesData.IsSubsistenceInteractionResilienceReliefOrDefault(90, 40, 65), Is.False);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("0"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]) + 1));
        Assert.That(customHousehold.GrainStore, Is.EqualTo(defaultHousehold.GrainStore));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionResilienceReliefScoreRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionResilienceReliefScore = 1,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunResilientGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunResilientGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customRulesData.IsSubsistenceInteractionResilienceReliefOrDefault(90, 40, 65), Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("-1"));
        Assert.That(
            int.Parse(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta]),
            Is.EqualTo(int.Parse(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta])));
        Assert.That(customHousehold.LaborCapacity, Is.EqualTo(defaultHousehold.LaborCapacity));
    }

    [Test]
    public void GrainPriceSpike_InvalidInteractionResilienceReliefRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionResilienceReliefGrainStoreThreshold = 101,
                SubsistenceInteractionResilienceReliefScore = 99,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunResilientGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunResilientGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionResilienceReliefGrainStoreThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionResilienceReliefGrainStoreThreshold));
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionResilienceReliefScoreOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionResilienceReliefScore));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("-2"));
    }

    [Test]
    public void GrainPriceSpike_DefaultInteractionClampRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionPressureClampFloor = -2,
                SubsistenceInteractionPressureClampCeiling = 4,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionPressureClampFloor, Is.EqualTo(-2));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionPressureClampCeiling, Is.EqualTo(4));
        Assert.That(explicitPreviousBaseline.GetSubsistenceInteractionPressureClampFloorOrDefault(), Is.EqualTo(-2));
        Assert.That(explicitPreviousBaseline.GetSubsistenceInteractionPressureClampCeilingOrDefault(), Is.EqualTo(4));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionClampFloorRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionPressureClampFloor = 0,
                SubsistenceInteractionPressureClampCeiling = 4,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunResilientGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunResilientGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("0"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure]));
        Assert.That(customHousehold.GrainStore, Is.EqualTo(defaultHousehold.GrainStore));
    }

    [Test]
    public void GrainPriceSpike_CustomInteractionClampCeilingRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionPressureClampFloor = -2,
                SubsistenceInteractionPressureClampCeiling = 2,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState customHousehold, IDomainEvent customEvent) =
            RunMissingMetadataGrainPriceSpike(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("2"));
        Assert.That(customEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.Not.EqualTo(defaultEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure]));
        Assert.That(customHousehold.DebtPressure, Is.EqualTo(defaultHousehold.DebtPressure));
    }

    [Test]
    public void GrainPriceSpike_InvalidInteractionClampRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistenceInteractionPressureClampFloor = 5,
                SubsistenceInteractionPressureClampCeiling = 4,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionPressureClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionPressureClampFloor));
        Assert.That(
            malformedRulesData.GetSubsistenceInteractionPressureClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistenceInteractionPressureClampCeiling));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure], Is.EqualTo("3"));
    }

    [Test]
    public void GrainPriceSpike_DefaultSubsistenceEventThresholdRulesDataMatchesPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData explicitPreviousBaseline =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistencePressureEventDistressThreshold = 60,
            };

        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState explicitHousehold, IDomainEvent explicitEvent) =
            RunMissingMetadataGrainPriceSpike(explicitPreviousBaseline);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultSubsistencePressureEventDistressThreshold, Is.EqualTo(60));
        Assert.That(explicitPreviousBaseline.GetSubsistencePressureEventDistressThresholdOrDefault(), Is.EqualTo(60));
        Assert.That(BuildGrainShockSignature(explicitHousehold, explicitEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.DistressBefore], Is.EqualTo("50"));
        Assert.That(defaultEvent.Metadata[DomainEventMetadataKeys.DistressAfter], Is.EqualTo("77"));
    }

    [Test]
    public void GrainPriceSpike_CustomSubsistenceEventThresholdRulesDataIsOwnerConsumed()
    {
        PopulationHouseholdMobilityRulesData customRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistencePressureEventDistressThreshold = 80,
            };

        (PopulationHouseholdState defaultHousehold, IReadOnlyList<IDomainEvent> defaultEvents) =
            RunMissingMetadataGrainPriceSpikeWithEvents();
        (PopulationHouseholdState customHousehold, IReadOnlyList<IDomainEvent> customEvents) =
            RunMissingMetadataGrainPriceSpikeWithEvents(customRulesData);

        Assert.That(customRulesData.Validate().IsValid, Is.True);
        Assert.That(customRulesData.GetSubsistencePressureEventDistressThresholdOrDefault(), Is.EqualTo(80));
        Assert.That(defaultEvents.Count(e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged), Is.EqualTo(1));
        Assert.That(customEvents.Count(e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged), Is.Zero);
        Assert.That(customHousehold.Distress, Is.EqualTo(defaultHousehold.Distress));
    }

    [Test]
    public void GrainPriceSpike_InvalidSubsistenceEventThresholdRulesDataFallsBackToPreviousBaseline()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                SubsistencePressureEventDistressThreshold = 101,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = malformedRulesData.Validate();
        (PopulationHouseholdState defaultHousehold, IDomainEvent defaultEvent) = RunMissingMetadataGrainPriceSpike();
        (PopulationHouseholdState fallbackHousehold, IDomainEvent fallbackEvent) =
            RunMissingMetadataGrainPriceSpike(malformedRulesData);

        Assert.That(validation.IsValid, Is.False);
        Assert.That(
            malformedRulesData.GetSubsistencePressureEventDistressThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultSubsistencePressureEventDistressThreshold));
        Assert.That(BuildGrainShockSignature(fallbackHousehold, fallbackEvent), Is.EqualTo(BuildGrainShockSignature(defaultHousehold, defaultEvent)));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.DistressBefore], Is.EqualTo("50"));
        Assert.That(fallbackEvent.Metadata[DomainEventMetadataKeys.DistressAfter], Is.EqualTo("77"));
    }

    private static (PopulationHouseholdState Household, IDomainEvent SubsistenceEvent) RunMissingMetadataGrainPriceSpike(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        (PopulationHouseholdState household, IReadOnlyList<IDomainEvent> events) =
            RunMissingMetadataGrainPriceSpikeWithEvents(rulesData);
        IDomainEvent subsistenceEvent = events.Single(e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged);
        return (household, subsistenceEvent);
    }

    private static (PopulationHouseholdState Household, IReadOnlyList<IDomainEvent> Events) RunMissingMetadataGrainPriceSpikeWithEvents(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsModule module = rulesData is null
            ? new PopulationAndHouseholdsModule()
            : new PopulationAndHouseholdsModule(rulesData);
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(10),
            HouseholdName = "Fallback petty trader",
            SettlementId = new SettlementId(7),
            Livelihood = LivelihoodType.PettyTrader,
            Distress = 50,
            DebtPressure = 65,
            LaborCapacity = 25,
            GrainStore = 10,
            DependentCount = 4,
            MigrationRisk = 75,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 9),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.TradeAndIndustry,
            TradeAndIndustryEventNames.GrainPriceSpike,
            "Settlement 7 grain price spike with missing grain metadata.",
            "7",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.SettlementId] = "7",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        PopulationHouseholdState household = state.Households.Single(static h => h.Id == new HouseholdId(10));
        return (household, buffer.Events.ToList());
    }

    private static (PopulationHouseholdState Household, IDomainEvent SubsistenceEvent) RunResilientGrainPriceSpike(
        PopulationHouseholdMobilityRulesData? rulesData = null)
    {
        PopulationAndHouseholdsModule module = rulesData is null
            ? new PopulationAndHouseholdsModule()
            : new PopulationAndHouseholdsModule(rulesData);
        PopulationAndHouseholdsState state = new();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(11),
            HouseholdName = "Resilient smallholder",
            SettlementId = new SettlementId(8),
            Livelihood = LivelihoodType.Smallholder,
            Distress = 58,
            DebtPressure = 20,
            LaborCapacity = 65,
            LandHolding = 40,
            GrainStore = 90,
            DependentCount = 1,
            MigrationRisk = 10,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 9),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.TradeAndIndustry,
            TradeAndIndustryEventNames.GrainPriceSpike,
            "Settlement 8 resilient grain price spike with missing grain metadata.",
            "8",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.SettlementId] = "8",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state, context, buffer.Events.ToList()));

        PopulationHouseholdState household = state.Households.Single(static h => h.Id == new HouseholdId(11));
        IDomainEvent subsistenceEvent = buffer.Events.Single(e => e.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged);
        return (household, subsistenceEvent);
    }

    private static string BuildGrainShockSignature(PopulationHouseholdState household, IDomainEvent subsistenceEvent)
    {
        return string.Join(
            "|",
            household.Distress,
            household.DebtPressure,
            household.LaborCapacity,
            household.MigrationRisk,
            household.IsMigrating,
            subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceDistressDelta],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistencePricePressure],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceGrainBufferPressure],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceMarketDependencyPressure],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceLaborPressure],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceFragilityPressure],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.SubsistenceInteractionPressure],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainCurrentPrice],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainPriceDelta],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainSupply],
            subsistenceEvent.Metadata[DomainEventMetadataKeys.GrainDemand]);
    }
}
