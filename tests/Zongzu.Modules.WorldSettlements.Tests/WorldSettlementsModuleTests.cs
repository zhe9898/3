using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.WorldSettlements.Tests;

[TestFixture]
public sealed class WorldSettlementsModuleTests
{
    [Test]
    public void RunXun_UpdatesSettlementPressureWithinBounds()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            Security = 50,
            Prosperity = 50,
            BaselineInstitutionCount = 1,
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Shangxun);

        module.RunXun(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(state.Settlements[0].Security, Is.InRange(0, 100));
        Assert.That(state.Settlements[0].Prosperity, Is.InRange(0, 100));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(1));
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_DoesNotApplyAdditionalSettlementDrift()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            Security = 50,
            Prosperity = 50,
            BaselineInstitutionCount = 1,
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        // Phase 1c: RunMonth now advances the three season-band axes
        // (SPATIAL_SKELETON_SPEC §3.2). Settlement security / prosperity
        // must still stay put — that drift lives in xun, not month.
        Assert.That(state.Settlements[0].Security, Is.EqualTo(50));
        Assert.That(state.Settlements[0].Prosperity, Is.EqualTo(50));
        // Settlement-level diffs must still be empty — season events carry no
        // settlement entity key and live on the module channel, not the
        // per-settlement one tested here.
        Assert.That(
            context.Diff.Entries.Any(entry => entry.EntityKey == "1"),
            Is.False,
            "RunMonth must not drift individual settlements.");
    }

    [Test]
    public void RunMonth_FloodDisasterDeclared_CarriesMetadataAndLatchesBand()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.FloodRisk = 75;
        state.CurrentSeason.EmbankmentStrain = 40;
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 50,
            Prosperity = 50,
        });

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        IDomainEvent disaster = context.DomainEvents.Events.Single(
            static e => e.EventType == WorldSettlementsEventNames.DisasterDeclared);
        Assert.That(disaster.EntityKey, Is.EqualTo("1"));
        Assert.That(disaster.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseDisaster));
        Assert.That(disaster.Metadata[DomainEventMetadataKeys.DisasterKind], Is.EqualTo(DomainEventMetadataValues.DisasterFlood));
        Assert.That(disaster.Metadata[DomainEventMetadataKeys.Severity], Is.EqualTo(DomainEventMetadataValues.SeverityFloodSevere));
        Assert.That(state.LastDeclaredFloodDisasterBand, Is.EqualTo(2));
    }

    [Test]
    public void RunMonth_ActiveFloodDisasterBand_DoesNotRedeclare()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.FloodRisk = 75;
        state.CurrentSeason.EmbankmentStrain = 40;
        state.LastDeclaredFloodDisasterBand = 2;
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 50,
            Prosperity = 50,
        });

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.DisasterDeclared),
            "A persistent severe flood band must not re-declare the same disaster every month.");
        Assert.That(state.LastDeclaredFloodDisasterBand, Is.EqualTo(2));
    }

    [Test]
    public void RunMonth_FloodRiskFallsBelowDisasterBand_ClearsDisasterWatermark()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.FloodRisk = 20;
        state.CurrentSeason.EmbankmentStrain = 10;
        state.LastDeclaredFloodDisasterBand = 2;

        ModuleExecutionContext context = new(
            new GameDate(1022, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(state.LastDeclaredFloodDisasterBand, Is.EqualTo(0));
    }

    [Test]
    public void RunMonth_FrontierStrainEscalated_CarriesSettlementEntityKeyAndLatchesBand()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.FrontierPressure = 60;
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 50,
            Prosperity = 50,
        });
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(2),
            Name = "Market",
            Tier = SettlementTier.MarketTown,
            NodeKind = SettlementNodeKind.MarketTown,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 50,
            Prosperity = 50,
        });

        ModuleExecutionContext context = new(
            new GameDate(1022, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        IDomainEvent frontier = context.DomainEvents.Events.Single(
            static e => e.EventType == WorldSettlementsEventNames.FrontierStrainEscalated);
        Assert.That(frontier.EntityKey, Is.EqualTo("1"));
        Assert.That(frontier.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseFrontier));
        Assert.That(frontier.Metadata[DomainEventMetadataKeys.Severity], Is.EqualTo(DomainEventMetadataValues.SeverityFrontierModerate));
        Assert.That(state.LastDeclaredFrontierStrainBand, Is.EqualTo(1));
    }

    [Test]
    public void RunMonth_ActiveFrontierStrainBand_DoesNotRedeclare()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.FrontierPressure = 60;
        state.LastDeclaredFrontierStrainBand = 1;
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 50,
            Prosperity = 50,
        });

        ModuleExecutionContext context = new(
            new GameDate(1022, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.FrontierStrainEscalated),
            "A persistent frontier band must not re-trigger supply requisitions every month.");
        Assert.That(state.LastDeclaredFrontierStrainBand, Is.EqualTo(1));
    }

    [Test]
    public void RunMonth_FrontierPressureFallsBelowBand_ClearsFrontierWatermark()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.FrontierPressure = 20;
        state.LastDeclaredFrontierStrainBand = 2;

        ModuleExecutionContext context = new(
            new GameDate(1022, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(state.LastDeclaredFrontierStrainBand, Is.EqualTo(0));
    }

    [Test]
    public void HandleEvents_AppliesCampaignScarsToSettlementBaseline()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            Security = 62,
            Prosperity = 68,
            BaselineInstitutionCount = 1,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "兰溪军务沙盘",
                IsActive = true,
                MobilizedForceCount = 54,
                FrontPressure = 78,
                FrontLabel = "前线吃紧",
                SupplyState = 34,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 41,
                MoraleStateLabel = "军心未定",
                LastAftermathSummary = "兰溪仓路受焚余与惊扰所压。",
            },
        ]));

        ModuleExecutionContext context = new(
            new GameDate(1200, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(52)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<WorldSettlementsState>(
            state,
            context,
            [
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignPressureRaised, "Lanxi pressure rose.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi entered aftermath review.", "1"),
            ]));

        Assert.That(state.Settlements[0].Security, Is.LessThan(62));
        Assert.That(state.Settlements[0].Prosperity, Is.LessThan(68));
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("战后余波"));
        Assert.That(context.DomainEvents.Events.Single().EventType, Is.EqualTo(WorldSettlementsEventNames.SettlementPressureChanged));
    }

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }
    }
}
