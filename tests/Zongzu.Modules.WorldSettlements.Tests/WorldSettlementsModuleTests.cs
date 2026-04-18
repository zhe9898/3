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
    public void RunMonth_UpdatesSettlementPressureWithinBounds()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
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

        Assert.That(state.Settlements[0].Security, Is.InRange(0, 100));
        Assert.That(state.Settlements[0].Prosperity, Is.InRange(0, 100));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(1));
        Assert.That(context.DomainEvents.Events, Has.Count.EqualTo(1));
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
                CampaignName = "Lanxi Campaign Board",
                IsActive = true,
                MobilizedForceCount = 54,
                FrontPressure = 78,
                FrontLabel = "Front under strain",
                SupplyState = 34,
                SupplyStateLabel = "Supply strained",
                MoraleState = 41,
                MoraleStateLabel = "Morale unsettled",
                LastAftermathSummary = "Lanxi is carrying burned stores and shaken roads.",
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
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("Campaign spillover"));
        Assert.That(context.DomainEvents.Events.Single().EventType, Is.EqualTo("SettlementPressureChanged"));
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
