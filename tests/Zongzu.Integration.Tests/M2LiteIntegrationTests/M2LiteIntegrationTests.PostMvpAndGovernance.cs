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
    public void StableM2Bootstrap_DoesNotLeakOfficeOrWarfarePlayerCommands()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260604);

        simulation.AdvanceMonths(2);


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);


        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)), Is.True);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Office, StringComparison.Ordinal)), Is.False);

        Assert.That(bundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal)), Is.False);

        // Step 2-A / A6: birth gate 解卡后 seed 夫妇在头两月内可能自然添丁，进
        // 而产生 Family 表面的生命周期 receipt。契约仍是"Office / Warfare 不泄
        // 漏到 M2 沙盘"，允许 Family receipt 但禁绝非 Family 的 receipt。
        Assert.That(
            bundle.PlayerCommands.Receipts.All(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)),
            Is.True);

        Assert.That(shell.Office.CommandAffordances, Is.Empty);

        Assert.That(shell.Warfare.CommandAffordances, Is.Empty);

        Assert.That(shell.FamilyCouncil.CommandAffordances, Is.Not.Empty);

    }


    [Test]

    public void CampaignSandbox_AftermathHandlers_UpdateOwnedDownstreamModules()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260529);

        simulation.AdvanceMonths(1);


        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();

        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(

            typeof(TradeAndIndustryState),

            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);

        WorldSettlementsState worldState = (WorldSettlementsState)serializer.Deserialize(

            typeof(WorldSettlementsState),

            saveRoot.ModuleStates[KnownModuleKeys.WorldSettlements].Payload);

        PopulationAndHouseholdsState populationState = (PopulationAndHouseholdsState)serializer.Deserialize(

            typeof(PopulationAndHouseholdsState),

            saveRoot.ModuleStates[KnownModuleKeys.PopulationAndHouseholds].Payload);

        FamilyCoreState familyState = (FamilyCoreState)serializer.Deserialize(

            typeof(FamilyCoreState),

            saveRoot.ModuleStates[KnownModuleKeys.FamilyCore].Payload);

        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        OfficeAndCareerState officeState = (OfficeAndCareerState)serializer.Deserialize(

            typeof(OfficeAndCareerState),

            saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);

        SocialMemoryAndRelationsState socialState = (SocialMemoryAndRelationsState)serializer.Deserialize(

            typeof(SocialMemoryAndRelationsState),

            saveRoot.ModuleStates[KnownModuleKeys.SocialMemoryAndRelations].Payload);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);


        Assert.That(simulation.LastMonthResult, Is.Not.Null);

        Assert.That(simulation.LastMonthResult!.DomainEvents.Any(static entry => entry.ModuleKey == KnownModuleKeys.WarfareCampaign && entry.EntityKey == "1"), Is.True);

        Assert.That(worldState.Settlements.Any(static settlement => settlement.Security < 57 || settlement.Prosperity < 61), Is.True);

        Assert.That(populationState.Households.Any(static household => household.Distress > 35 || household.MigrationRisk > 40), Is.True);

        Assert.That(familyState.Clans.Any(static clan => clan.Prestige != 52 || clan.SupportReserve != 60), Is.True);

        Assert.That(tradeState.Clans.Any(static trade => !string.IsNullOrWhiteSpace(trade.LastExplanation)), Is.True);

        Assert.That(orderState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.LastPressureReason)), Is.True);

        Assert.That(officeState.People.Any(static career => !string.IsNullOrWhiteSpace(career.LastPetitionOutcome)), Is.True);

        Assert.That(socialState.Memories.Any(static memory => memory.Kind.StartsWith("campaign-", StringComparison.Ordinal)), Is.True);

        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.WorldSettlements && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.PopulationAndHouseholds && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.FamilyCore && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.TradeAndIndustry && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.OrderAndBanditry && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.OfficeAndCareer && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

        Assert.That(simulation.LastMonthResult.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.SocialMemoryAndRelations && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

        Assert.That(shell.GreatHall.AftermathDocketSummary, Is.Not.Empty);

        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.AftermathSummary)), Is.True);

        Assert.That(shell.Warfare.CampaignBoards.Any(static board => !string.IsNullOrWhiteSpace(board.AftermathDocketSummary)), Is.True);

    }


    [Test]

    public void CampaignSandbox_AftermathHandlers_DragConflictForceThroughOwnedFatigueState()

    {

        GameSimulation governanceSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260531);

        GameSimulation campaignSimulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260531);


        governanceSimulation.AdvanceMonths(1);

        campaignSimulation.AdvanceMonths(1);


        MessagePackModuleStateSerializer serializer = new();

        ConflictAndForceState governanceState = (ConflictAndForceState)serializer.Deserialize(

            typeof(ConflictAndForceState),

            governanceSimulation.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);

        ConflictAndForceState campaignState = (ConflictAndForceState)serializer.Deserialize(

            typeof(ConflictAndForceState),

            campaignSimulation.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);


        SettlementForceState governanceForce = governanceState.Settlements.Single();

        SettlementForceState campaignForce = campaignState.Settlements.Single();


        Assert.That(campaignForce.CampaignFatigue, Is.GreaterThan(0));

        Assert.That(campaignForce.CampaignEscortStrain, Is.GreaterThan(0));

        Assert.That(campaignForce.LastCampaignFalloutTrace, Is.Not.Empty);

        Assert.That(campaignForce.LastConflictTrace, Is.Not.Empty);

        Assert.That(campaignForce.Readiness, Is.LessThan(governanceForce.Readiness));

        Assert.That(campaignForce.CommandCapacity, Is.LessThanOrEqualTo(governanceForce.CommandCapacity));

        Assert.That(campaignSimulation.LastMonthResult!.Diff.Entries.Any(static entry => entry.ModuleKey == KnownModuleKeys.ConflictAndForce && !string.IsNullOrWhiteSpace(entry.Description)), Is.True);

    }


    [Test]

    public void CampaignSandboxBootstrap_RemainsDeterministicAcrossSixtyMonthsForMultipleSeeds()

    {

        int[] seeds = [20260523, 20260524];


        foreach (int seed in seeds)

        {

            GameSimulation firstSimulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(seed);

            GameSimulation secondSimulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(seed);


            firstSimulation.AdvanceMonths(60);

            secondSimulation.AdvanceMonths(60);


            Assert.That(secondSimulation.CurrentDate, Is.EqualTo(firstSimulation.CurrentDate));

            Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));

        }

    }


    [Test]

    public void ActiveM2Bootstrap_RemainsIsolatedFromM3LocalConflictModules()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(27182);

        SaveRoot saveRoot = simulation.ExportSave();


        Assert.That(

            SimulationBootstrapper.CreateM2Modules().Select(static module => module.ModuleKey),

            Does.Not.Contain(KnownModuleKeys.OrderAndBanditry));

        Assert.That(

            SimulationBootstrapper.CreateM2Modules().Select(static module => module.ModuleKey),

            Does.Not.Contain(KnownModuleKeys.ConflictAndForce));

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.False);

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.False);

        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry), Is.False);

        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce), Is.False);

    }


    [Test]

    public void LocalConflictPresentation_RemainsOfficeFreeWithoutGovernanceLite()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(27183);

        simulation.AdvanceMonths(4);


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);


        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer), Is.False);

        Assert.That(bundle.OfficeCareers, Is.Empty);

        Assert.That(bundle.OfficeJurisdictions, Is.Empty);

        Assert.That(shell.GreatHall.GovernanceSummary, Is.Not.Empty);

        Assert.That(shell.Office.Summary, Is.Not.Empty);

        Assert.That(shell.Office.Appointments, Is.Empty);

        Assert.That(shell.Office.Jurisdictions, Is.Empty);

        Assert.That(bundle.Campaigns, Is.Empty);

        Assert.That(bundle.CampaignMobilizationSignals, Is.Empty);

        Assert.That(shell.GreatHall.WarfareSummary, Is.Not.Empty);

        Assert.That(shell.Warfare.Summary, Is.Not.Empty);

    }


    [Test]

    public void PostMvpPreflightSeams_ReserveWarfareCampaignAndKeepBlackRouteInsideOwnedNamespaces()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(314159);


        Assert.That(

            PostMvpPreflightSeams.BlackRouteOwnerModuleKeys,

            Is.EqualTo(new[]

            {

                KnownModuleKeys.OrderAndBanditry,

                KnownModuleKeys.TradeAndIndustry,

            }));

        Assert.That(PostMvpPreflightSeams.BlackRouteOwnerModuleKeys, Does.Not.Contain(KnownModuleKeys.WarfareCampaign));

        Assert.That(

            PostMvpPreflightSeams.BlackRoutePressureUpstreamModuleKeys,

            Is.EqualTo(new[]

            {

                KnownModuleKeys.OrderAndBanditry,

                KnownModuleKeys.ConflictAndForce,

                KnownModuleKeys.OfficeAndCareer,

            }));

        Assert.That(

            PostMvpPreflightSeams.BlackRouteLedgerOwnerModuleKeys,

            Is.EqualTo(new[]

            {

                KnownModuleKeys.TradeAndIndustry,

            }));

        Assert.That(

            PostMvpPreflightSeams.WarfareCampaignUpstreamModuleKeys,

            Is.EqualTo(new[]

            {

                KnownModuleKeys.ConflictAndForce,

                KnownModuleKeys.WorldSettlements,

                KnownModuleKeys.OfficeAndCareer,

            }));

        Assert.That(

            PostMvpPreflightSeams.WarfareCampaignMigrationOwnerModuleKeys,

            Is.EqualTo(new[]

            {

                KnownModuleKeys.WarfareCampaign,

            }));

        Assert.That(

            SimulationBootstrapper.CreateM3LocalConflictModules().Select(static module => module.ModuleKey),

            Does.Not.Contain(KnownModuleKeys.WarfareCampaign));

        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign), Is.False);

        Assert.That(simulation.ExportSave().ModuleStates.ContainsKey(KnownModuleKeys.WarfareCampaign), Is.False);

    }


    [Test]

    public void LocalConflictBootstrap_BlackRouteSlice_StaysInsideOrderAndTradeOwnedState()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(20260623);

        simulation.AdvanceMonths(3);


        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();

        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(

            typeof(TradeAndIndustryState),

            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);


        Assert.That(saveRoot.ModuleStates.ContainsKey("BlackRoute"), Is.False);

        Assert.That(orderState.Settlements, Is.Not.Empty);

        Assert.That(tradeState.BlackRouteLedgers, Is.Not.Empty);

        Assert.That(orderState.Settlements.All(static settlement => settlement.BlackRoutePressure >= 0), Is.True);

        Assert.That(orderState.Settlements.All(static settlement => settlement.PaperCompliance >= 0), Is.True);

        Assert.That(orderState.Settlements.All(static settlement => settlement.ImplementationDrag >= 0), Is.True);

        Assert.That(orderState.Settlements.All(static settlement => settlement.RouteShielding >= 0), Is.True);

        Assert.That(orderState.Settlements.All(static settlement => settlement.RetaliationRisk >= 0), Is.True);

        Assert.That(orderState.Settlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.EscalationBandLabel)), Is.True);

        Assert.That(orderState.Settlements.All(static settlement => !string.IsNullOrWhiteSpace(settlement.LastPressureTrace)), Is.True);

        Assert.That(tradeState.BlackRouteLedgers.All(static ledger => !string.IsNullOrWhiteSpace(ledger.DiversionBandLabel)), Is.True);

        Assert.That(tradeState.BlackRouteLedgers.All(static ledger => !string.IsNullOrWhiteSpace(ledger.LastLedgerTrace)), Is.True);

        Assert.That(tradeState.Routes, Is.Not.Empty);

        Assert.That(tradeState.Routes.All(static route => route.BlockedShipmentCount >= 0), Is.True);

        Assert.That(tradeState.Routes.All(static route => route.SeizureRisk >= 0), Is.True);

        Assert.That(tradeState.Routes.All(static route => !string.IsNullOrWhiteSpace(route.RouteConstraintLabel)), Is.True);

        Assert.That(tradeState.Routes.All(static route => !string.IsNullOrWhiteSpace(route.LastRouteTrace)), Is.True);

        Assert.That(

            orderState.Settlements.Select(static settlement => settlement.SettlementId).OrderBy(static id => id.Value).ToArray(),

            Is.EqualTo(tradeState.BlackRouteLedgers.Select(static ledger => ledger.SettlementId).OrderBy(static id => id.Value).ToArray()));

    }


    [Test]

    public void GovernanceLocalConflict_PublicLifeCommands_ProjectAffordancesAndReceiptsOnDeskNodes()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260619);

        simulation.AdvanceMonths(3);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);


        Assert.That(beforeBundle.PublicLifeSettlements, Is.Not.Empty);

        Assert.That(beforeBundle.PublicLifeSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.ChannelSummary)), Is.True);

        Assert.That(beforeBundle.PublicLifeSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.ContentionSummary)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.PostCountyNotice, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.EscortRoadReport, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.NegotiateWithOutlaws, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.TolerateDisorder, StringComparison.Ordinal)), Is.True);

        Assert.That(beforeBundle.PlayerCommands.Affordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)), Is.True);


        SettlementId settlementId = beforeBundle.PublicLifeSettlements.Single().SettlementId;

        ClanId clanId = beforeBundle.Clans.Single(clan => clan.HomeSettlementId == settlementId).Id;

        PlayerCommandService service = new();


        PlayerCommandResult noticeResult = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.PostCountyNotice,

            });

        PlayerCommandResult roadReportResult = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.DispatchRoadReport,

            });

        PlayerCommandResult escortResult = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.EscortRoadReport,

            });

        PlayerCommandResult watchResult = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.FundLocalWatch,

            });

        PlayerCommandResult elderResult = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                ClanId = clanId,

                CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,

            });


        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(afterBundle);

        SettlementNodeViewModel settlementNode = shell.DeskSandbox.Settlements.Single();


        Assert.That(noticeResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));

        Assert.That(noticeResult.CommandName, Is.EqualTo(PlayerCommandNames.PostCountyNotice));

        Assert.That(roadReportResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));

        Assert.That(escortResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));

        Assert.That(watchResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));

        Assert.That(watchResult.CommandName, Is.EqualTo(PlayerCommandNames.FundLocalWatch));

        Assert.That(elderResult.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)), Is.True);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal)), Is.True);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal)), Is.True);

        Assert.That(afterBundle.PlayerCommands.Receipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)), Is.True);

        Assert.That(

            afterBundle.PlayerCommands.Receipts.Count(receipt =>

                string.Equals(receipt.ModuleKey, KnownModuleKeys.OrderAndBanditry, StringComparison.Ordinal)

                && string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)

                && receipt.SettlementId == settlementId),

            Is.EqualTo(1));

        Assert.That(settlementNode.PublicLifeCommandAffordances, Is.Not.Empty);

        Assert.That(settlementNode.PublicLifeCommandAffordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.PostCountyNotice, StringComparison.Ordinal)), Is.True);

        Assert.That(settlementNode.PublicLifeCommandAffordances.Any(static command => string.Equals(command.CommandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal)), Is.True);

        Assert.That(settlementNode.PublicLifeRecentReceipts, Is.Not.Empty);

        Assert.That(settlementNode.PublicLifeRecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal)), Is.True);

        Assert.That(settlementNode.PublicLifeRecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal)), Is.True);

        Assert.That(settlementNode.PublicLifeRecentReceipts.Any(static receipt => string.Equals(receipt.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)), Is.True);

        Assert.That(

            settlementNode.PublicLifeRecentReceipts.Count(static receipt =>

                string.Equals(receipt.CommandName, PlayerCommandNames.EscortRoadReport, StringComparison.Ordinal)

                || string.Equals(receipt.CommandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal)

                || string.Equals(receipt.CommandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal)

                || string.Equals(receipt.CommandName, PlayerCommandNames.NegotiateWithOutlaws, StringComparison.Ordinal)

                || string.Equals(receipt.CommandName, PlayerCommandNames.TolerateDisorder, StringComparison.Ordinal)),

            Is.EqualTo(1));

        Assert.That(settlementNode.PublicLifeSummary, Is.Not.Empty);

        Assert.That(settlementNode.PublicLifeSummary.Length, Is.GreaterThan(0));

    }


    [Test]

    public void GovernanceLocalConflict_OrderInterventionScalesAgainstOfficeReach()

    {

        GameSimulation supportedSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260624);

        GameSimulation cloggedSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260624);

        supportedSimulation.AdvanceMonths(3);

        cloggedSimulation.AdvanceMonths(3);


        PresentationReadModelBuilder builder = new();

        SettlementId supportedSettlementId = builder.BuildForM2(supportedSimulation).PublicLifeSettlements.Single().SettlementId;

        SettlementId cloggedSettlementId = builder.BuildForM2(cloggedSimulation).PublicLifeSettlements.Single().SettlementId;


        Assert.That(cloggedSettlementId, Is.EqualTo(supportedSettlementId));


        supportedSimulation = ConfigureOfficeReach(supportedSimulation, supportedSettlementId, 78, 66, 12, 16, 10);

        cloggedSimulation = ConfigureOfficeReach(cloggedSimulation, cloggedSettlementId, 14, 18, 88, 74, 62);

        PresentationReadModelBundle supportedBundle = builder.BuildForM2(supportedSimulation);

        PresentationReadModelBundle cloggedBundle = builder.BuildForM2(cloggedSimulation);

        PlayerCommandAffordanceSnapshot supportedAffordance = supportedBundle.PlayerCommands.Affordances.Single(command =>

            command.SettlementId == supportedSettlementId

            && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)

            && string.Equals(command.CommandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal));

        PlayerCommandAffordanceSnapshot cloggedAffordance = cloggedBundle.PlayerCommands.Affordances.Single(command =>

            command.SettlementId == cloggedSettlementId

            && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)

            && string.Equals(command.CommandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal));


        PlayerCommandService service = new();

        PlayerCommandResult supportedResult = service.IssueIntent(

            supportedSimulation,

            new PlayerCommandRequest

            {

                SettlementId = supportedSettlementId,

                CommandName = PlayerCommandNames.SuppressBanditry,

            });

        PlayerCommandResult cloggedResult = service.IssueIntent(

            cloggedSimulation,

            new PlayerCommandRequest

            {

                SettlementId = cloggedSettlementId,

                CommandName = PlayerCommandNames.SuppressBanditry,

            });


        MessagePackModuleStateSerializer serializer = new();

        OrderAndBanditryState supportedOrder = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            supportedSimulation.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        OrderAndBanditryState cloggedOrder = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            cloggedSimulation.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        SettlementDisorderState supportedSettlement = supportedOrder.Settlements.Single(settlement => settlement.SettlementId == supportedSettlementId);

        SettlementDisorderState cloggedSettlement = cloggedOrder.Settlements.Single(settlement => settlement.SettlementId == cloggedSettlementId);


        Assert.That(supportedResult.Accepted, Is.True);

        Assert.That(cloggedResult.Accepted, Is.True);

        Assert.That(supportedSettlement.BanditThreat, Is.LessThan(cloggedSettlement.BanditThreat));

        Assert.That(supportedSettlement.RoutePressure, Is.LessThan(cloggedSettlement.RoutePressure));

        Assert.That(supportedSettlement.RetaliationRisk, Is.LessThan(cloggedSettlement.RetaliationRisk));

        Assert.That(supportedAffordance.ExecutionSummary, Does.Contain("县署肯出手"));

        Assert.That(cloggedAffordance.ExecutionSummary, Does.Contain("县署拥案未解"));

        Assert.That(supportedResult.Summary, Does.Contain("县署肯出手"));

        Assert.That(cloggedResult.Summary, Does.Contain("县署拥案未解"));

    }


    [Test]

    public void GovernanceLocalConflict_RecentOrderCarryoverPressesOfficeOnNextMonth()

    {

        GameSimulation interventionSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260625);

        GameSimulation controlSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260625);

        interventionSimulation.AdvanceMonths(3);

        controlSimulation.AdvanceMonths(3);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(interventionSimulation);

        SettlementId settlementId = beforeBundle.PublicLifeSettlements.Single().SettlementId;

        SettlementId controlSettlementId = builder.BuildForM2(controlSimulation).PublicLifeSettlements.Single().SettlementId;


        Assert.That(controlSettlementId, Is.EqualTo(settlementId));

        PlayerCommandAffordanceSnapshot affordance = beforeBundle.PlayerCommands.Affordances.Single(command =>

            string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)

            && string.Equals(command.CommandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal)

            && command.SettlementId == settlementId);


        Assert.That(affordance.LeverageSummary, Does.Contain("本户"));

        Assert.That(affordance.CostSummary, Does.Contain("代价"));

        Assert.That(affordance.ReadbackSummary, Does.Contain("下月读回"));


        PlayerCommandService service = new();

        PlayerCommandResult result = service.IssueIntent(

            interventionSimulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.SuppressBanditry,

            });


        Assert.That(result.Accepted, Is.True);


        interventionSimulation.AdvanceMonths(1);

        controlSimulation.AdvanceMonths(1);


        PresentationReadModelBundle afterBundle = builder.BuildForM2(interventionSimulation);

        OfficeCareerState interventionCareer = GetLeadOfficeCareer(interventionSimulation, settlementId);

        OfficeCareerState controlCareer = GetLeadOfficeCareer(controlSimulation, settlementId);

        OrderAndBanditryState interventionOrder = GetOrderState(interventionSimulation);

        SettlementDisorderState disorder = interventionOrder.Settlements.Single(entry => entry.SettlementId == settlementId);

        SettlementInteractionHotspotSnapshot hotspot = afterBundle.Debug.CurrentHotspots.Single(entry => entry.SettlementId == settlementId);

        SettlementGovernanceLaneSnapshot governanceLane = afterBundle.GovernanceSettlements.Single(entry => entry.SettlementId == settlementId);

        PlayerCommandReceiptSnapshot receipt = afterBundle.PlayerCommands.Receipts.Single(receipt =>

            string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)

            && string.Equals(receipt.CommandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal)

            && receipt.SettlementId == settlementId);


        Assert.That(interventionCareer.PetitionBacklog, Is.GreaterThan(controlCareer.PetitionBacklog));

        Assert.That(interventionCareer.DemotionPressure, Is.GreaterThan(controlCareer.DemotionPressure));

        Assert.That(interventionCareer.LastPetitionOutcome, Does.Contain("上月严缉路匪"));

        Assert.That(interventionCareer.LastExplanation, Does.Contain("上月严缉路匪"));

        Assert.That(interventionCareer.CurrentAdministrativeTask, Is.AnyOf("勘理词状", "勘解乡怨词牒"));

        Assert.That(afterBundle.Debug.CurrentInteractionPressure.OrderAdministrativeAftermathSettlements, Is.EqualTo(1));

        Assert.That(governanceLane.HasOrderAdministrativeAftermath, Is.True);

        Assert.That(governanceLane.RecentOrderCommandLabel, Is.EqualTo("严缉路匪"));

        Assert.That(governanceLane.GovernanceSummary, Does.Contain("上月严缉路匪"));

        Assert.That(governanceLane.OrderAdministrativeAftermathSummary, Does.Contain("积案"));

        Assert.That(governanceLane.PublicPressureSummary, Does.Contain("路压"));

        Assert.That(governanceLane.SuggestedCommandName, Is.Not.Empty);

        Assert.That(governanceLane.SuggestedCommandLabel, Is.Not.Empty);

        Assert.That(governanceLane.SuggestedCommandPrompt, Does.Contain(governanceLane.SuggestedCommandLabel));

        Assert.That(

            afterBundle.PlayerCommands.Affordances.Any(command =>

                command.IsEnabled

                && command.SettlementId == settlementId

                && string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)

                && string.Equals(command.CommandName, governanceLane.SuggestedCommandName, StringComparison.Ordinal)

                && string.Equals(command.Label, governanceLane.SuggestedCommandLabel, StringComparison.Ordinal)),

            Is.True);

        Assert.That(hotspot.AdministrativeTaskLoad, Is.GreaterThan(0));

        Assert.That(hotspot.PetitionBacklog, Is.GreaterThan(0));

        Assert.That(hotspot.AdministrativeAftermathSummary, Does.Contain("上月严缉路匪"));

        Assert.That(afterBundle.GovernanceFocus.SettlementId, Is.EqualTo(settlementId));

        Assert.That(afterBundle.GovernanceFocus.HasOrderAdministrativeAftermath, Is.True);

        Assert.That(afterBundle.GovernanceFocus.LeadSummary, Does.Contain("上月严缉路匪"));

        Assert.That(afterBundle.GovernanceFocus.PublicMomentumSummary, Is.Not.Empty);

        Assert.That(afterBundle.GovernanceFocus.SuggestedCommandPrompt, Is.Not.Empty);

        Assert.That(afterBundle.GovernanceDocket.SettlementId, Is.EqualTo(settlementId));

        Assert.That(afterBundle.GovernanceDocket.HasOrderAdministrativeAftermath, Is.True);

        Assert.That(afterBundle.GovernanceDocket.HasRecentReceipt, Is.True);

        Assert.That(afterBundle.GovernanceDocket.RecentReceiptCommandName, Is.EqualTo(PlayerCommandNames.SuppressBanditry));

        Assert.That(afterBundle.GovernanceDocket.RecentReceiptLabel, Is.EqualTo("严缉路匪"));

        Assert.That(afterBundle.GovernanceDocket.RecentReceiptLeverageSummary, Does.Contain("本户"));

        Assert.That(afterBundle.GovernanceDocket.RecentReceiptCostSummary, Does.Contain("代价"));

        Assert.That(afterBundle.GovernanceDocket.RecentReceiptReadbackSummary, Does.Contain("下月读回"));

        Assert.That(afterBundle.GovernanceDocket.Headline, Is.Not.Empty);

        Assert.That(afterBundle.GovernanceDocket.PublicMomentumSummary, Is.Not.Empty);

        Assert.That(afterBundle.GovernanceDocket.WhyNowSummary, Does.Contain("积案"));

        Assert.That(afterBundle.GovernanceDocket.PhaseLabel, Is.EqualTo("案后收束"));

        Assert.That(afterBundle.GovernanceDocket.PhaseSummary, Does.Contain("上月严缉路匪"));

        Assert.That(afterBundle.GovernanceDocket.HandlingSummary, Does.Contain("近已按严缉路匪处置"));

        Assert.That(afterBundle.GovernanceDocket.HandlingSummary, Does.Contain("上月严缉路匪"));

        Assert.That(afterBundle.GovernanceDocket.HandlingSummary, Does.Contain("代价"));

        Assert.That(afterBundle.GovernanceDocket.HandlingSummary, Does.Contain("下月读回"));

        Assert.That(afterBundle.GovernanceDocket.GuidanceSummary, Is.Not.Empty);

        Assert.That(receipt.ExecutionSummary, Does.Contain("上月严缉路匪"));

        Assert.That(receipt.ExecutionSummary, Does.Contain("积案"));

        Assert.That(receipt.LeverageSummary, Does.Contain("本户"));

        Assert.That(receipt.CostSummary, Does.Contain("代价"));

        Assert.That(receipt.ReadbackSummary, Does.Contain("下月读回"));

        Assert.That(afterBundle.Debug.Warnings.Any(static warning => warning.Contains("office cleanup tied to recent order follow-through", StringComparison.Ordinal)), Is.True);

        Assert.That(afterBundle.Debug.Warnings.Any(static warning => warning.Contains("office follow-through", StringComparison.Ordinal)), Is.True);

        Assert.That(disorder.InterventionCarryoverMonths, Is.EqualTo(0));

    }


}
