using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Presentation.Unity;
using Zongzu.Persistence;

namespace Zongzu.Integration.Tests;

public sealed partial class M2LiteIntegrationTests
{
    [Test]
    public void CampaignSandboxBootstrap_ActivatesWarfareCampaignAndSurfacesReadOnlyBoard()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260522);

        simulation.AdvanceMonths(8);


        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();

        WarfareCampaignState warfareState = (WarfareCampaignState)serializer.Deserialize(

            typeof(WarfareCampaignState),

            saveRoot.ModuleStates[KnownModuleKeys.WarfareCampaign].Payload);


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);


        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign), Is.True);

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.WarfareCampaign), Is.True);

        Assert.That(warfareState.Campaigns, Is.Not.Empty);

        Assert.That(

            warfareState.Campaigns.Any(static campaign => campaign.IsActive || !string.IsNullOrWhiteSpace(campaign.LastAftermathSummary)),

            Is.True);

        Assert.That(warfareState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.FrontLabel)), Is.True);

        Assert.That(warfareState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommandFitLabel)), Is.True);

        Assert.That(warfareState.Campaigns.Any(static campaign => campaign.Routes.Count > 0), Is.True);

        Assert.That(warfareState.MobilizationSignals, Is.Not.Empty);

        Assert.That(warfareState.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);

        Assert.That(bundle.Campaigns, Is.Not.Empty);

        Assert.That(bundle.CampaignMobilizationSignals, Is.Not.Empty);

        Assert.That(

            bundle.Campaigns.Any(static campaign => campaign.IsActive || !string.IsNullOrWhiteSpace(campaign.LastAftermathSummary)),

            Is.True);

        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.AnchorSettlementName)), Is.True);

        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommanderSummary)), Is.True);

        Assert.That(bundle.Campaigns.Any(static campaign => campaign.Routes.Count > 0), Is.True);

        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.MobilizationWindowLabel)), Is.True);

        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);

        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)), Is.True);

        Assert.That(bundle.CampaignMobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.ActiveDirectiveLabel)), Is.True);

        Assert.That(bundle.Notifications.Any(static notification => notification.SourceModuleKey == KnownModuleKeys.WarfareCampaign), Is.True);

        Assert.That(bundle.Notifications.Any(static notification => notification.Surface == NarrativeSurface.DeskSandbox), Is.True);

        Assert.That(bundle.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.LastAftermathSummary)), Is.True);

        Assert.That(

            string.Equals(bundle.HallDocket.LeadItem.LaneKey, HallDocketLaneKeys.Warfare, StringComparison.Ordinal)

            || bundle.HallDocket.SecondaryItems.Any(static item => string.Equals(item.LaneKey, HallDocketLaneKeys.Warfare, StringComparison.Ordinal)),

            Is.True);

        HallDocketItemSnapshot warfareItem = string.Equals(bundle.HallDocket.LeadItem.LaneKey, HallDocketLaneKeys.Warfare, StringComparison.Ordinal)

            ? bundle.HallDocket.LeadItem

            : bundle.HallDocket.SecondaryItems.Single(item => string.Equals(item.LaneKey, HallDocketLaneKeys.Warfare, StringComparison.Ordinal));

        Assert.That(warfareItem.OrderingSummary, Is.Not.Empty);

        Assert.That(warfareItem.SourceProjectionKeys, Does.Contain(HallDocketSourceProjectionKeys.Campaigns));

        Assert.That(warfareItem.SourceModuleKeys, Does.Contain(KnownModuleKeys.WarfareCampaign));

        Assert.That(shell.GreatHall.WarfareSummary, Is.Not.Empty);

        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.CampaignSummary)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards, Is.Not.Empty);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.RegionalProfileLabel)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.RegionalBackdropSummary)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.EnvironmentLabel)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.BoardSurfaceLabel)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.BoardAtmosphereSummary)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.MarkerSummary)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.CommandFitLabel)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.CommanderSummary)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.DirectiveLabel)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => board.Routes.Count > 0), Is.True);

        Assert.That(shell.Warfare.MobilizationSignals, Is.Not.Empty);

        Assert.That(shell.Warfare.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);

        Assert.That(shell.Warfare.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.DirectiveLabel)), Is.True);

    }


    [Test]

    public void CampaignSandboxBootstrap_UsesNorthernSongGroundedSeedLabels()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictStressBootstrap(20260523);

        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();


        WorldSettlementsState worldState = (WorldSettlementsState)serializer.Deserialize(

            typeof(WorldSettlementsState),

            saveRoot.ModuleStates[KnownModuleKeys.WorldSettlements].Payload);

        PopulationAndHouseholdsState populationState = (PopulationAndHouseholdsState)serializer.Deserialize(

            typeof(PopulationAndHouseholdsState),

            saveRoot.ModuleStates[KnownModuleKeys.PopulationAndHouseholds].Payload);

        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(

            typeof(TradeAndIndustryState),

            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);


        Assert.That(worldState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.Name)), Is.True);

        Assert.That(worldState.Settlements.Select(static settlement => settlement.Name).Distinct(StringComparer.Ordinal).Count(), Is.GreaterThanOrEqualTo(2));

        Assert.That(tradeState.Markets.Any(static market => !string.IsNullOrWhiteSpace(market.MarketName)), Is.True);

        Assert.That(tradeState.Routes.Any(static route => !string.IsNullOrWhiteSpace(route.RouteName)), Is.True);

        Assert.That(tradeState.Routes.Select(static route => route.RouteName).Distinct(StringComparer.Ordinal).Count(), Is.GreaterThanOrEqualTo(2));

        Assert.That(populationState.Households.Any(static household => !string.IsNullOrWhiteSpace(household.HouseholdName)), Is.True);

        Assert.That(populationState.Households.Select(static household => household.HouseholdName).Distinct(StringComparer.Ordinal).Count(), Is.GreaterThanOrEqualTo(2));

        Assert.That(

            tradeState.Routes.All(static route => !route.RouteName.Contains("Wharf", StringComparison.Ordinal) && !route.RouteName.Contains("Canal", StringComparison.Ordinal) && !route.RouteName.Contains("Ferry", StringComparison.Ordinal)),

            Is.True);

        Assert.That(

            populationState.Households.All(static household => !household.HouseholdName.Contains("Tenant", StringComparison.Ordinal) && !household.HouseholdName.Contains("Boatman", StringComparison.Ordinal)),

            Is.True);

    }


    [Test]

    public void CampaignSandboxCommandService_AppliesAncientDirectiveToCampaignBoard()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260527);

        simulation.AdvanceMonths(3);


        WarfareCampaignCommandService service = new();

        SettlementId anchorSettlementId = new PresentationReadModelBuilder()

            .BuildForM2(simulation)

            .CampaignMobilizationSignals

            .Single()

            .SettlementId;


        WarfareCampaignIntentResult result = service.IssueIntent(

            simulation,

            new WarfareCampaignIntentCommand

            {

                SettlementId = anchorSettlementId,

                CommandName = WarfareCampaignCommandNames.ProtectSupplyLine,

            });


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        CampaignFrontSnapshot campaign = bundle.Campaigns.Single();

        CampaignMobilizationSignalSnapshot signal = bundle.CampaignMobilizationSignals.Single();


        Assert.That(result.Accepted, Is.True);

        Assert.That(result.DirectiveLabel, Is.Not.Empty);

        Assert.That(result.Summary, Is.Not.Empty);

        Assert.That(campaign.ActiveDirectiveLabel, Is.EqualTo(result.DirectiveLabel));

        Assert.That(campaign.ActiveDirectiveSummary, Is.Not.Empty);

        Assert.That(campaign.LastDirectiveTrace, Is.Not.Empty);

        Assert.That(signal.ActiveDirectiveLabel, Is.EqualTo(result.DirectiveLabel));

        Assert.That(signal.ActiveDirectiveSummary, Is.Not.Empty);

    }


    [Test]

    public void CampaignBundle_ExportsReadOnlyPlayerCommandAffordances()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260601);

        simulation.AdvanceMonths(2);


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);


        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal)), Is.True);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal)), Is.True);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.PetitionViaOfficeChannels, StringComparison.Ordinal)), Is.True);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.DeployAdministrativeLeverage, StringComparison.Ordinal)), Is.True);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.CommitMobilization, StringComparison.Ordinal)), Is.True);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal) && command.IsEnabled), Is.True);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal) && command.IsEnabled), Is.True);

        Assert.That(shell.Office.CommandAffordances, Is.Not.Empty);

        Assert.That(shell.Office.CommandAffordances.Any(static command => command.IsEnabled), Is.True);

        Assert.That(shell.Warfare.CommandAffordances, Is.Not.Empty);

        Assert.That(shell.Warfare.CommandAffordances.Any(static command => command.IsEnabled), Is.True);

    }


    [Test]

    public void PlayerCommandService_RoutesOfficeAndWarfareIntents_AndUpdatesReadModels()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260602);

        simulation.AdvanceMonths(3);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);

        SettlementId officeSettlementId = beforeBundle.OfficeJurisdictions.Single().SettlementId;

        SettlementId warfareSettlementId = beforeBundle.CampaignMobilizationSignals.Single().SettlementId;

        PlayerCommandService service = new();


        PlayerCommandResult officeResult = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = officeSettlementId,

                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,

            });

        PlayerCommandResult warfareResult = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = warfareSettlementId,

                CommandName = PlayerCommandNames.ProtectSupplyLine,

            });


        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);

        JurisdictionAuthoritySnapshot jurisdiction = afterBundle.OfficeJurisdictions.Single();

        CampaignFrontSnapshot campaign = afterBundle.Campaigns.Single();

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);


        Assert.That(officeResult.Accepted, Is.True);

        Assert.That(officeResult.Label, Is.Not.Empty);

        Assert.That(officeResult.Summary, Is.Not.Empty);

        Assert.That(warfareResult.Accepted, Is.True);

        Assert.That(warfareResult.Label, Is.Not.Empty);

        Assert.That(jurisdiction.LastAdministrativeTrace, Is.Not.Empty);

        Assert.That(jurisdiction.LastPetitionOutcome, Is.Not.Empty);

        Assert.That(campaign.ActiveDirectiveLabel, Is.Not.Empty);

        Assert.That(campaign.LastDirectiveTrace, Is.Not.Empty);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal)), Is.True);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal)), Is.True);

        Assert.That(shell.Office.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.PetitionViaOfficeChannels, StringComparison.Ordinal)), Is.True);

        Assert.That(shell.Warfare.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.ProtectSupplyLine, StringComparison.Ordinal)), Is.True);

    }


    [Test]

    public void PlayerCommandService_RemainsDeterministicForSameOfficeIntent()

    {

        GameSimulation firstSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260603);

        GameSimulation secondSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260603);

        firstSimulation.AdvanceMonths(2);

        secondSimulation.AdvanceMonths(2);


        SettlementId settlementId = new PresentationReadModelBuilder()

            .BuildForM2(firstSimulation)

            .OfficeJurisdictions

            .Single()

            .SettlementId;

        PlayerCommandService service = new();


        PlayerCommandResult firstResult = service.IssueIntent(

            firstSimulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.DeployAdministrativeLeverage,

            });

        PlayerCommandResult secondResult = service.IssueIntent(

            secondSimulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.DeployAdministrativeLeverage,

            });


        Assert.That(firstResult.Accepted, Is.True);

        Assert.That(secondResult.Accepted, Is.True);

        Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));

        Assert.That(secondSimulation.ExportSave().CurrentDate, Is.EqualTo(firstSimulation.ExportSave().CurrentDate));

        Assert.That(new PresentationReadModelBuilder().BuildForM2(secondSimulation).OfficeJurisdictions.Single().LastPetitionOutcome,

            Is.EqualTo(new PresentationReadModelBuilder().BuildForM2(firstSimulation).OfficeJurisdictions.Single().LastPetitionOutcome));

    }


    [Test]

    public void PlayerCommandService_RoutesFamilyIntent_AndSurfacesFamilyReceipts()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260605);

        simulation.AdvanceMonths(2);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);

        ClanSnapshot clan = beforeBundle.Clans.Single();

        PlayerCommandService service = new();


        PlayerCommandResult result = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = clan.HomeSettlementId,

                ClanId = clan.Id,

                CommandName = PlayerCommandNames.InviteClanEldersMediation,

            });


        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);

        ClanSnapshot updatedClan = afterBundle.Clans.Single(updated => updated.Id == clan.Id);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);


        Assert.That(result.Accepted, Is.True);

        Assert.That(result.CommandName, Is.EqualTo(PlayerCommandNames.InviteClanEldersMediation));

        Assert.That(result.Label, Is.Not.Empty);

        Assert.That(result.TargetLabel, Is.EqualTo(clan.ClanName));

        Assert.That(updatedClan.MediationMomentum, Is.GreaterThan(clan.MediationMomentum));

        Assert.That(updatedClan.LastConflictOutcome, Is.Not.Empty);

        Assert.That(updatedClan.LastConflictCommandLabel, Is.EqualTo(result.Label));

        Assert.That(afterBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)), Is.True);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(receipt =>

            string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)

            && string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersMediation, StringComparison.Ordinal)

            && string.Equals(receipt.TargetLabel, clan.ClanName, StringComparison.Ordinal)), Is.True);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal) && string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersMediation, StringComparison.Ordinal)), Is.True);

        Assert.That(shell.FamilyCouncil.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersMediation, StringComparison.Ordinal)), Is.True);

        Assert.That(shell.FamilyCouncil.Clans.Any(static entry => !string.IsNullOrWhiteSpace(entry.LastOrderSummary)), Is.True);

    }


    [Test]

    public void PlayerCommandService_RoutesFamilyLifecycleIntent_AndSurfacesRicherLifecycleReceipts()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260619);

        simulation.AdvanceMonths(2);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);

        ClanSnapshot clan = beforeBundle.Clans.Single();

        PlayerCommandService service = new();


        PlayerCommandResult result = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = clan.HomeSettlementId,

                ClanId = clan.Id,

                CommandName = PlayerCommandNames.ArrangeMarriage,

            });


        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);

        ClanSnapshot updatedClan = afterBundle.Clans.Single(updated => updated.Id == clan.Id);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);


        Assert.That(result.Accepted, Is.True);

        Assert.That(result.CommandName, Is.EqualTo(PlayerCommandNames.ArrangeMarriage));

        Assert.That(result.Label, Is.Not.Empty);

        Assert.That(result.TargetLabel, Is.EqualTo(clan.ClanName));

        Assert.That(result.Summary, Is.Not.Empty);

        Assert.That(updatedClan.LastLifecycleOutcome, Is.Not.Empty);

        Assert.That(updatedClan.LastLifecycleCommandLabel, Is.EqualTo(result.Label));

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt =>

            string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)

            && string.Equals(receipt.CommandName, PlayerCommandNames.ArrangeMarriage, StringComparison.Ordinal)

            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);

        Assert.That(shell.FamilyCouncil.Clans.Any(entry => entry.LifecycleSummary.Contains(result.Label, StringComparison.Ordinal)), Is.True);

    }


    [Test]

    public void PlayerCommandService_RoutesNewbornCareIntent_AndSurfacesInfantFollowUp()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260620);

        simulation.AdvanceMonths(2);


        MessagePackModuleStateSerializer serializer = new();

        SaveRoot saveRoot = simulation.ExportSave();

        FamilyCoreState familyState = (FamilyCoreState)serializer.Deserialize(

            typeof(FamilyCoreState),

            saveRoot.ModuleStates[KnownModuleKeys.FamilyCore].Payload);

        ClanStateData clanState = familyState.Clans.Single();

        familyState.People.Add(new FamilyPersonState

        {

            Id = new PersonId(9001),

            ClanId = clanState.Id,

            GivenName = "新婴",

            AgeMonths = 8,

            IsAlive = true,

        });

        clanState.SupportReserve = Math.Max(clanState.SupportReserve, 12);

        saveRoot.ModuleStates[KnownModuleKeys.FamilyCore] = new ModuleStateEnvelope

        {

            ModuleKey = KnownModuleKeys.FamilyCore,

            ModuleSchemaVersion = 3,

            Payload = serializer.Serialize(typeof(FamilyCoreState), familyState),

        };

        simulation = SimulationBootstrapper.LoadM2(saveRoot);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);

        ClanSnapshot clan = beforeBundle.Clans.Single();

        PlayerCommandService service = new();


        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static affordance =>

            string.Equals(affordance.CommandName, PlayerCommandNames.SupportNewbornCare, StringComparison.Ordinal)

            && affordance.IsEnabled), Is.True);


        PlayerCommandResult result = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = clan.HomeSettlementId,

                ClanId = clan.Id,

                CommandName = PlayerCommandNames.SupportNewbornCare,

            });


        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);

        ClanSnapshot updatedClan = afterBundle.Clans.Single(updated => updated.Id == clan.Id);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);


        Assert.That(result.Accepted, Is.True);

        Assert.That(result.Label, Is.Not.Empty);

        Assert.That(result.Summary, Is.Not.Empty);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt =>

            string.Equals(receipt.CommandName, PlayerCommandNames.SupportNewbornCare, StringComparison.Ordinal)

            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);

        Assert.That(updatedClan.LastLifecycleCommandLabel, Is.EqualTo(result.Label));

        Assert.That(updatedClan.LastLifecycleOutcome, Is.Not.Empty);

        Assert.That(updatedClan.InfantCount, Is.GreaterThan(0));

        Assert.That(shell.FamilyCouncil.RecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.SupportNewbornCare, StringComparison.Ordinal)), Is.True);

        Assert.That(shell.FamilyCouncil.Clans.Any(entry => entry.LifecycleSummary.Contains(result.Label, StringComparison.Ordinal)), Is.True);

    }


    [Test]

    public void PlayerCommandService_RoutesMourningOrderIntent_AndSurfacesMourningFollowUp()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260621);

        simulation.AdvanceMonths(2);


        MessagePackModuleStateSerializer serializer = new();

        SaveRoot saveRoot = simulation.ExportSave();

        FamilyCoreState familyState = (FamilyCoreState)serializer.Deserialize(

            typeof(FamilyCoreState),

            saveRoot.ModuleStates[KnownModuleKeys.FamilyCore].Payload);

        ClanStateData clanState = familyState.Clans.Single();

        clanState.MourningLoad = 24;

        clanState.InheritancePressure = Math.Max(clanState.InheritancePressure, 28);

        clanState.SupportReserve = Math.Max(clanState.SupportReserve, 12);

        saveRoot.ModuleStates[KnownModuleKeys.FamilyCore] = new ModuleStateEnvelope

        {

            ModuleKey = KnownModuleKeys.FamilyCore,

            ModuleSchemaVersion = 3,

            Payload = serializer.Serialize(typeof(FamilyCoreState), familyState),

        };

        simulation = SimulationBootstrapper.LoadM2(saveRoot);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);

        ClanSnapshot clan = beforeBundle.Clans.Single();

        PlayerCommandService service = new();

        PresentationShellViewModel shellBefore = FirstPassPresentationShell.Compose(beforeBundle);


        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static affordance =>

            string.Equals(affordance.CommandName, PlayerCommandNames.SetMourningOrder, StringComparison.Ordinal)

            && affordance.IsEnabled), Is.True);

        Assert.That(shellBefore.FamilyCouncil.Summary, Is.Not.Empty);


        PlayerCommandResult result = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = clan.HomeSettlementId,

                ClanId = clan.Id,

                CommandName = PlayerCommandNames.SetMourningOrder,

            });


        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);


        Assert.That(result.Accepted, Is.True);

        Assert.That(result.Label, Is.Not.Empty);

        Assert.That(result.Summary, Is.Not.Empty);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt =>

            string.Equals(receipt.CommandName, PlayerCommandNames.SetMourningOrder, StringComparison.Ordinal)

            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);

        Assert.That(shell.FamilyCouncil.RecentReceipts.Any(static receipt =>

            string.Equals(receipt.CommandName, PlayerCommandNames.SetMourningOrder, StringComparison.Ordinal)

            && !string.IsNullOrWhiteSpace(receipt.OutcomeSummary)), Is.True);

        Assert.That(shell.FamilyCouncil.Clans.Single().LifecycleSummary, Is.Not.Empty);

    }


    [Test]

    public void MvpFamilyLifecyclePreviewScenario_BuildsViewableBeforeAndAfterBundles()

    {

        MvpFamilyLifecyclePreviewResult preview = new MvpFamilyLifecyclePreviewScenario().Build(20260419, 2);


        PresentationShellViewModel beforeShell = FirstPassPresentationShell.Compose(preview.BeforeBundle);

        PresentationShellViewModel afterCommandShell = FirstPassPresentationShell.Compose(preview.AfterCommandBundle);

        PresentationShellViewModel afterAdvanceShell = FirstPassPresentationShell.Compose(preview.AfterAdvanceBundle);


        Assert.That(preview.SelectedAffordance.IsEnabled, Is.True);

        Assert.That(preview.CommandResult.Accepted, Is.True);

        Assert.That(preview.BeforeBundle.Clans, Is.Not.Empty);

        Assert.That(preview.BeforeBundle.PublicLifeSettlements, Is.Empty);

        Assert.That(preview.AfterCommandBundle.PublicLifeSettlements, Is.Empty);

        Assert.That(preview.AfterAdvanceBundle.PublicLifeSettlements, Is.Empty);

        Assert.That(preview.AfterCommandBundle.PlayerCommands.Receipts.Any(receipt =>

            string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)

            && string.Equals(receipt.CommandName, preview.CommandResult.CommandName, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeShell.GreatHall.FamilySummary, Is.Not.Empty);

        Assert.That(beforeShell.GreatHall.PublicLifeSummary, Is.Not.Empty);

        Assert.That(afterCommandShell.FamilyCouncil.RecentReceipts.Any(receipt =>

            string.Equals(receipt.CommandName, preview.CommandResult.CommandName, StringComparison.Ordinal)), Is.True);

        Assert.That(afterAdvanceShell.NotificationCenter.Items, Is.Not.Empty);

        Assert.That(afterAdvanceShell.GreatHall.LeadNoticeTitle, Is.Not.Empty);

    }


    [Test]

    public void MvpFamilyLifecyclePreviewScenario_TenYearRun_KeepsLifecycleGuidanceAligned()

    {

        MvpFamilyLifecycleTenYearPreviewResult preview = new MvpFamilyLifecyclePreviewScenario().BuildTenYear(20260419, 10);


        Assert.That(preview.YearlyCheckpoints, Has.Count.EqualTo(10));

        Assert.That(preview.IssuedCommands, Is.Not.Empty);


        foreach (MvpFamilyLifecycleTenYearCheckpoint checkpoint in preview.YearlyCheckpoints)

        {

            PresentationShellViewModel shell = FirstPassPresentationShell.Compose(checkpoint.AfterAdvanceBundle);

            PlayerCommandAffordanceSnapshot? primary = checkpoint.AfterAdvanceBundle.PlayerCommands.Affordances

                .Where(static command =>

                    command.IsEnabled

                    && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)

                    && command.CommandName is

                        PlayerCommandNames.SetMourningOrder

                        or PlayerCommandNames.SupportNewbornCare

                        or PlayerCommandNames.DesignateHeirPolicy

                        or PlayerCommandNames.ArrangeMarriage)

                .OrderBy(static command => command.CommandName switch

                {

                    PlayerCommandNames.SetMourningOrder => 0,

                    PlayerCommandNames.SupportNewbornCare => 1,

                    PlayerCommandNames.DesignateHeirPolicy => 2,

                    PlayerCommandNames.ArrangeMarriage => 3,

                    _ => 10,

                })

                .ThenBy(static command => command.TargetLabel, StringComparer.Ordinal)

                .FirstOrDefault();


            Assert.That(checkpoint.AfterAdvanceBundle.PublicLifeSettlements, Is.Empty);


            if (primary is null)

            {

                continue;

            }


            Assert.That(shell.GreatHall.FamilySummary, Does.Contain(primary.Label));

            Assert.That(shell.FamilyCouncil.Summary, Does.Contain(primary.Label));


            FamilyConflictTileViewModel? clan = shell.FamilyCouncil.Clans.FirstOrDefault();

            if (clan is not null)

            {

                Assert.That(clan.LifecycleSummary, Does.Contain(primary.Label));

            }


            NotificationItemViewModel? familyNotice = shell.NotificationCenter.Items

                .FirstOrDefault(static item => item.SourceModuleKey == KnownModuleKeys.FamilyCore);

            if (familyNotice is not null)

            {

                Assert.That(familyNotice.WhatNext, Is.Not.Empty);

            }

        }

    }


}
