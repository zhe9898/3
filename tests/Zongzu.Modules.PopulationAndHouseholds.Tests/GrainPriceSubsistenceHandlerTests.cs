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

    private static (PopulationHouseholdState Household, IDomainEvent SubsistenceEvent) RunMissingMetadataGrainPriceSpike(
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
