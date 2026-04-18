using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Modules.SocialMemoryAndRelations.Tests;

[TestFixture]
public sealed class SocialMemoryAndRelationsModuleTests
{
    [Test]
    public void RunMonth_PreservesAndEscalatesGrudgesUnderRepeatedStrain()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 42,
            SupportReserve = 35,
            HeirPersonId = new PersonId(1),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 78,
            DebtPressure = 82,
            LaborCapacity = 24,
            MigrationRisk = 71,
            IsMigrating = false,
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);
        socialModule.RegisterQueries(socialState, queries);

        KernelState kernelState = KernelState.Create(5);
        for (int month = 0; month < 36; month += 1)
        {
            ModuleExecutionContext context = new(
                new GameDate(1200 + (month / 12), (month % 12) + 1),
                new FeatureManifest(),
                new DeterministicRandom(kernelState),
                queries,
                new DomainEventBuffer(),
                new WorldDiff(),
                kernelState);

            socialModule.RunMonth(new ModuleExecutionScope<SocialMemoryAndRelationsState>(socialState, context));
        }

        Assert.That(socialState.ClanNarratives, Has.Count.EqualTo(1));
        Assert.That(socialState.ClanNarratives[0].GrudgePressure, Is.GreaterThanOrEqualTo(60));
        Assert.That(socialState.Memories.Count, Is.GreaterThan(0));
    }

    [Test]
    public void HandleEvents_AddsCampaignMemoryAndRaisesFearInsideSocialState()
    {
        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 42,
            SupportReserve = 35,
            HeirPersonId = new PersonId(1),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            PublicNarrative = "Quiet watchfulness",
            GrudgePressure = 34,
            FearPressure = 28,
            ShamePressure = 19,
            FavorBalance = 10,
        });

        QueryRegistry queries = new();
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi军务沙盘",
                IsActive = true,
                MobilizedForceCount = 40,
                FrontPressure = 72,
                FrontLabel = "前线转紧",
                SupplyState = 35,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 39,
                MoraleStateLabel = "军心摇动",
                CommandFitLabel = "号令尚整",
                CommanderSummary = "Lanxi command is strained.",
                ActiveDirectiveCode = WarfareCampaignCommandNames.CommitMobilization,
                ActiveDirectiveLabel = "发檄点兵",
                ActiveDirectiveSummary = "整众应调。",
                LastDirectiveTrace = "Lanxi has committed mobilization.",
                MobilizationWindowLabel = "Open",
                SupplyLineSummary = "Granaries and roads are under watch.",
                OfficeCoordinationTrace = "Registrar is forwarding wartime filings.",
                SourceTrace = "Campaign pressure rose from local conflict.",
                LastAftermathSummary = "战后覆核仍在地方记忆里发酵。",
            },
        ]));

        DomainEventRecord[] events =
        {
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi aftermath registered.", "1"),
        };

        KernelState kernelState = KernelState.Create(7304);
        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            new FeatureManifest(),
            new DeterministicRandom(kernelState),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            kernelState);

        socialModule.HandleEvents(new ModuleEventHandlingScope<SocialMemoryAndRelationsState>(socialState, context, events));

        ClanNarrativeState narrative = socialState.ClanNarratives.Single();

        Assert.That(narrative.FearPressure, Is.GreaterThan(28));
        Assert.That(narrative.GrudgePressure, Is.GreaterThan(34));
        Assert.That(narrative.PublicNarrative, Does.Contain("Lanxi"));
        Assert.That(socialState.Memories, Has.Count.EqualTo(1));
        Assert.That(socialState.Memories[0].Kind, Is.EqualTo("campaign-aftermath"));
        Assert.That(socialState.Memories[0].Summary, Does.Contain("Lanxi"));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.SocialMemoryAndRelations));
    }

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }
    }
}
