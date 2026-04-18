using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.FamilyCore.Tests;

[TestFixture]
public sealed class FamilyCoreModuleTests
{
    [Test]
    public void RunMonth_AgesPeopleAndRespondsToSettlementPressure()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 30,
            Prosperity = 40,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 60,
            HeirPersonId = new PersonId(1),
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 120,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(5)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        familyModule.RunMonth(new ModuleExecutionScope<FamilyCoreState>(familyState, context));

        Assert.That(familyState.People[0].AgeMonths, Is.EqualTo(121));
        Assert.That(familyState.Clans[0].Prestige, Is.EqualTo(49));
        Assert.That(familyState.Clans[0].SupportReserve, Is.EqualTo(59));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(2));
        Assert.That(context.DomainEvents.Events, Has.Count.EqualTo(2));
    }

    [Test]
    public void HandleEvents_AppliesCampaignReputationAndSupportFalloutInsideClanState()
    {
        FamilyCoreModule module = new();
        FamilyCoreState state = module.CreateInitialState();
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 58,
            SupportReserve = 64,
            HeirPersonId = new PersonId(1),
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
                MobilizedForceCount = 52,
                FrontPressure = 73,
                FrontLabel = "Front tightening",
                SupplyState = 38,
                SupplyStateLabel = "Supply strained",
                MoraleState = 45,
                MoraleStateLabel = "Morale wavering",
                LastAftermathSummary = "Lanxi clans are paying for wagons and funerals.",
            },
        ]));

        ModuleExecutionContext context = new(
            new GameDate(1200, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(61)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state,
            context,
            [
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignMobilized, "Lanxi mobilized.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi entered aftermath review.", "1"),
            ]));

        Assert.That(state.Clans[0].Prestige, Is.LessThan(58));
        Assert.That(state.Clans[0].SupportReserve, Is.LessThan(64));
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("Campaign spillover"));
        Assert.That(context.DomainEvents.Events.Single().EventType, Is.EqualTo("ClanPrestigeAdjusted"));
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
