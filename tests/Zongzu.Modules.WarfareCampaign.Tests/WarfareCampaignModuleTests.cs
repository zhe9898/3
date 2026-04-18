using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.WarfareCampaign.Tests;

[TestFixture]
public sealed class WarfareCampaignModuleTests
{
    [Test]
    public void RunMonth_ActiveConflictCreatesCampaignBoardAndMobilizationSignal()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(3),
            Name = "Lanxi",
            Security = 43,
            Prosperity = 58,
            BaselineInstitutionCount = 1,
        });

        ConflictAndForceModule conflictModule = new();
        ConflictAndForceState conflictState = conflictModule.CreateInitialState();
        conflictState.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(3),
            GuardCount = 14,
            RetainerCount = 6,
            MilitiaCount = 24,
            EscortCount = 8,
            Readiness = 49,
            CommandCapacity = 36,
            ResponseActivationLevel = 31,
            OrderSupportLevel = 9,
            IsResponseActivated = true,
            HasActiveConflict = true,
            LastConflictTrace = "Lanxi has entered a wider escort-and-militia response posture.",
        });

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.Jurisdictions.Add(new JurisdictionAuthorityState
        {
            SettlementId = new SettlementId(3),
            LeadOfficialPersonId = new PersonId(8),
            LeadOfficialName = "Zhang Yuan",
            LeadOfficeTitle = "Assistant Magistrate",
            AuthorityTier = 3,
            JurisdictionLeverage = 68,
            PetitionPressure = 22,
            PetitionBacklog = 11,
            CurrentAdministrativeTask = "emergency petition review",
            LastPetitionOutcome = "Triaged: Petitions triaged while emergency petition review.",
            LastAdministrativeTrace = "The assistant magistrate has opened a rapid docket for transport complaints.",
        });

        WarfareCampaignModule module = new();
        WarfareCampaignState state = module.CreateInitialState();

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        conflictModule.RegisterQueries(conflictState, queries);
        officeModule.RegisterQueries(officeState, queries);
        module.RegisterQueries(state, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            CreateManifest(includeOffice: true),
            new DeterministicRandom(KernelState.Create(7070)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WarfareCampaignState>(state, context));

        IWarfareCampaignQueries warfareQueries = queries.GetRequired<IWarfareCampaignQueries>();
        CampaignFrontSnapshot campaign = warfareQueries.GetCampaigns().Single();
        CampaignMobilizationSignalSnapshot signal = warfareQueries.GetMobilizationSignals().Single();

        Assert.That(campaign.AnchorSettlementName, Is.EqualTo("Lanxi"));
        Assert.That(campaign.IsActive, Is.True);
        Assert.That(campaign.MobilizedForceCount, Is.EqualTo(47));
        Assert.That(campaign.FrontPressure, Is.GreaterThan(0));
        Assert.That(campaign.CampaignName, Is.EqualTo("Lanxi军务沙盘"));
        Assert.That(campaign.FrontLabel, Is.EqualTo("前线持守"));
        Assert.That(campaign.SupplyState, Is.GreaterThan(0));
        Assert.That(campaign.SupplyStateLabel, Does.Contain("粮道"));
        Assert.That(campaign.MoraleState, Is.GreaterThan(0));
        Assert.That(campaign.MoraleStateLabel, Does.Contain("军心"));
        Assert.That(campaign.CommandFitLabel, Does.Contain("号令"));
        Assert.That(campaign.CommanderSummary, Does.Contain("Assistant Magistrate"));
        Assert.That(campaign.ActiveDirectiveLabel, Is.EqualTo("筹议方略"));
        Assert.That(campaign.ActiveDirectiveSummary, Does.Contain("案上筹定"));
        Assert.That(campaign.LastDirectiveTrace, Does.Contain("当前军令"));
        Assert.That(campaign.MobilizationWindowLabel, Is.EqualTo("Open"));
        Assert.That(campaign.SupplyLineSummary, Does.Contain("粮道"));
        Assert.That(campaign.OfficeCoordinationTrace, Does.Contain("Assistant Magistrate"));
        Assert.That(campaign.SourceTrace, Does.Contain("Lanxi军务态势"));
        Assert.That(campaign.LastAftermathSummary, Does.Contain("军务态势"));
        Assert.That(campaign.Routes, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(campaign.Routes.Any(static route => route.RouteRole == "supply"), Is.True);
        Assert.That(campaign.Routes[0].RouteLabel, Is.EqualTo("粮道"));
        Assert.That(campaign.Routes.All(static route => !string.IsNullOrWhiteSpace(route.FlowStateLabel)), Is.True);
        Assert.That(campaign.Routes.All(static route => !string.IsNullOrWhiteSpace(route.Summary)), Is.True);

        Assert.That(signal.SettlementName, Is.EqualTo("Lanxi"));
        Assert.That(signal.AvailableForceCount, Is.EqualTo(52));
        Assert.That(signal.CommandFitLabel, Does.Contain("号令"));
        Assert.That(signal.ActiveDirectiveLabel, Is.EqualTo("筹议方略"));
        Assert.That(signal.ActiveDirectiveSummary, Does.Contain("案上筹定"));
        Assert.That(signal.MobilizationWindowLabel, Is.EqualTo("Open"));
        Assert.That(signal.OfficeCoordinationTrace, Does.Contain("Assistant Magistrate"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain(WarfareCampaignEventNames.CampaignMobilized));
        Assert.That(context.DomainEvents.Events.Any(static entry => entry.EventType == WarfareCampaignEventNames.CampaignMobilized && entry.EntityKey == "3"), Is.True);
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.WarfareCampaign));
        Assert.That(module.AcceptedCommands, Does.Contain(WarfareCampaignCommandNames.ProtectSupplyLine));
        Assert.That(module.PublishedEvents, Does.Contain(WarfareCampaignEventNames.CampaignSupplyStrained));
    }

    [Test]
    public void RunMonth_CalmReadinessCreatesClosedSignalWithoutCampaignBoard()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(4),
            Name = "Qingshui",
            Security = 67,
            Prosperity = 62,
            BaselineInstitutionCount = 1,
        });

        ConflictAndForceModule conflictModule = new();
        ConflictAndForceState conflictState = conflictModule.CreateInitialState();
        conflictState.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(4),
            GuardCount = 8,
            RetainerCount = 3,
            MilitiaCount = 6,
            EscortCount = 2,
            Readiness = 26,
            CommandCapacity = 19,
            ResponseActivationLevel = 12,
            OrderSupportLevel = 0,
            IsResponseActivated = false,
            HasActiveConflict = false,
            LastConflictTrace = "Qingshui remains under routine watch rotation.",
        });

        WarfareCampaignModule module = new();
        WarfareCampaignState state = module.CreateInitialState();

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        conflictModule.RegisterQueries(conflictState, queries);
        module.RegisterQueries(state, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            CreateManifest(includeOffice: false),
            new DeterministicRandom(KernelState.Create(8080)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WarfareCampaignState>(state, context));

        IWarfareCampaignQueries warfareQueries = queries.GetRequired<IWarfareCampaignQueries>();
        CampaignMobilizationSignalSnapshot signal = warfareQueries.GetMobilizationSignals().Single();

        Assert.That(warfareQueries.GetCampaigns(), Is.Empty);
        Assert.That(signal.MobilizationWindowLabel, Is.EqualTo("Closed"));
        Assert.That(signal.AvailableForceCount, Is.EqualTo(19));
        Assert.That(signal.CommandFitLabel, Does.Contain("号令"));
        Assert.That(signal.ActiveDirectiveLabel, Is.EqualTo("班师归营"));
        Assert.That(signal.OfficeCoordinationTrace, Does.Contain("暂无官署"));
        Assert.That(context.DomainEvents.Events, Is.Empty);
        Assert.That(context.Diff.Entries, Is.Empty);
    }

    private static FeatureManifest CreateManifest(bool includeOffice)
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.WarfareCampaign, FeatureMode.Lite);
        if (includeOffice)
        {
            manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        }

        return manifest;
    }
}
