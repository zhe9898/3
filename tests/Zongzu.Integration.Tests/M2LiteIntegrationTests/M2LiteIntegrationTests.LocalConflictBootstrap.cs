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
    public void StressBootstraps_PreserveSettlementParityAcrossOrderOnlyAndLocalConflictPaths()

    {

        GameSimulation orderStressSimulation = SimulationBootstrapper.CreateM3OrderAndBanditryStressBootstrap(6060);

        GameSimulation localConflictStressSimulation = SimulationBootstrapper.CreateM3LocalConflictStressBootstrap(6060);


        SaveRoot orderStressSave = orderStressSimulation.ExportSave();

        SaveRoot localConflictStressSave = localConflictStressSimulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();


        OrderAndBanditryState orderStressState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            orderStressSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        OrderAndBanditryState localConflictOrderState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            localConflictStressSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);


        Assert.That(orderStressState.Settlements.Select(static settlement => settlement.SettlementId).OrderBy(static id => id.Value).ToArray(),

            Is.EqualTo(localConflictOrderState.Settlements.Select(static settlement => settlement.SettlementId).OrderBy(static id => id.Value).ToArray()));

        Assert.That(orderStressState.Settlements, Has.Count.EqualTo(4));

        Assert.That(localConflictStressSave.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.True);

        Assert.That(orderStressSave.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.False);

    }


    [Test]

    public void LocalConflictStressBootstrap_SeparatesActivatedAndCalmSettlementOrderReasons()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictStressBootstrap(7070);

        simulation.AdvanceMonths(1);


        MessagePackModuleStateSerializer serializer = new();

        SaveRoot saveRoot = simulation.ExportSave();

        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        ConflictAndForceState conflictState = (ConflictAndForceState)serializer.Deserialize(

            typeof(ConflictAndForceState),

            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);


        ConflictAndForceModule conflictModule = new();

        QueryRegistry queries = new();

        conflictModule.RegisterQueries(conflictState, queries);

        IConflictAndForceQueries conflictQueries = queries.GetRequired<IConflictAndForceQueries>();


        LocalForcePoolSnapshot[] activeResponses = conflictQueries.GetSettlementForces()

            .Where(static snapshot => snapshot.HasActiveConflict && snapshot.IsResponseActivated)

            .ToArray();

        LocalForcePoolSnapshot[] calmResponses = conflictQueries.GetSettlementForces()

            .Where(static snapshot => !snapshot.HasActiveConflict || !snapshot.IsResponseActivated)

            .ToArray();


        Assert.That(activeResponses, Is.Not.Empty);

        Assert.That(calmResponses, Is.Not.Empty);


        foreach (LocalForcePoolSnapshot active in activeResponses)

        {

            SettlementDisorderState disorder = orderState.Settlements.Single(settlement => settlement.SettlementId == active.SettlementId);

            Assert.That(active.OrderSupportLevel, Is.GreaterThan(0));

            Assert.That(disorder.LastPressureReason, Is.Not.Empty);

        }


        foreach (LocalForcePoolSnapshot calm in calmResponses)

        {

            SettlementDisorderState disorder = orderState.Settlements.Single(settlement => settlement.SettlementId == calm.SettlementId);

            Assert.That(calm.OrderSupportLevel, Is.EqualTo(0));

            Assert.That(disorder.LastPressureReason, Is.Not.Empty);

        }

    }


    [Test]

    public void OrderAndBanditryLite_ProducesTraceableDisorderAndInfluencesTrade()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(5150);

        simulation.AdvanceMonths(2);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle bundle = builder.BuildForM2(simulation);

        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();


        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(

            typeof(TradeAndIndustryState),

            saveRoot.ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);


        Assert.That(orderState.Settlements, Has.Count.EqualTo(1));

        Assert.That(orderState.Settlements.Single().BanditThreat, Is.GreaterThan(0));

        Assert.That(orderState.Settlements.Single().RoutePressure, Is.GreaterThan(0));

        Assert.That(orderState.Settlements.Single().LastPressureReason, Is.Not.Empty);

        Assert.That(tradeState.Clans.Single().LastExplanation, Is.Not.Empty);

        Assert.That(bundle.Notifications.Any(static notification => notification.SourceModuleKey == KnownModuleKeys.OrderAndBanditry), Is.True);

        Assert.That(bundle.Debug.RecentDiffEntries.Any(static trace => trace.ModuleKey == KnownModuleKeys.OrderAndBanditry), Is.True);

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.True);

    }


    [Test]

    public void OrderInterventionCarryover_ProjectsIntoRuntimeDebugAndHotspots()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(5153);

        simulation.AdvanceMonths(2);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);

        SettlementId settlementId = beforeBundle.PublicLifeSettlements.Single().SettlementId;

        PlayerCommandService service = new();


        PlayerCommandResult result = service.IssueIntent(

            simulation,

            new PlayerCommandRequest

            {

                SettlementId = settlementId,

                CommandName = PlayerCommandNames.FundLocalWatch,

            });


        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);

        SettlementInteractionHotspotSnapshot hotspot = afterBundle.Debug.CurrentHotspots.Single();


        Assert.That(result.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));

        Assert.That(result.CommandName, Is.EqualTo(PlayerCommandNames.FundLocalWatch));

        Assert.That(afterBundle.Debug.CurrentInteractionPressure.OrderInterventionCarryoverSettlements, Is.EqualTo(1));

        Assert.That(afterBundle.Debug.CurrentInteractionPressure.OrderAdministrativeAftermathSettlements, Is.EqualTo(0));

        Assert.That(afterBundle.Debug.CurrentInteractionPressure.ShieldingDominantSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(afterBundle.Debug.CurrentInteractionPressure.BacklashDominantSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(afterBundle.Debug.CurrentHotspots, Has.Count.EqualTo(1));

        Assert.That(hotspot.SettlementId, Is.EqualTo(settlementId));

        Assert.That(hotspot.RouteShielding, Is.GreaterThan(0));

        Assert.That(hotspot.RetaliationRisk, Is.GreaterThanOrEqualTo(0));

        Assert.That(hotspot.InterventionCarryoverMonths, Is.EqualTo(1));

        Assert.That(hotspot.AdministrativeTaskLoad, Is.EqualTo(0));

        Assert.That(hotspot.PetitionBacklog, Is.EqualTo(0));

        Assert.That(hotspot.AdministrativeAftermathSummary, Is.Empty);

        Assert.That(afterBundle.Debug.Warnings.Any(static warning => warning.Contains("order carryover active", StringComparison.Ordinal)), Is.True);

        Assert.That(

            afterBundle.PlayerCommands.Receipts.Any(static receipt =>

                string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.PublicLife, StringComparison.Ordinal)

                && string.Equals(receipt.CommandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal)),

            Is.True);

    }


    [Test]

    public void OrderEnabledM3Bootstrap_LoadsOrderWithoutActivatingConflict()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(1234);

        simulation.AdvanceMonths(6);


        SaveCodec codec = new();

        GameSimulation reloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(codec.Decode(codec.Encode(simulation.ExportSave())));


        Assert.That(

            SimulationBootstrapper.CreateM3OrderAndBanditryModules().Select(static module => module.ModuleKey),

            Does.Contain(KnownModuleKeys.OrderAndBanditry));

        Assert.That(

            SimulationBootstrapper.CreateM3OrderAndBanditryModules().Select(static module => module.ModuleKey),

            Does.Not.Contain(KnownModuleKeys.ConflictAndForce));

        Assert.That(reloaded.ExportSave().ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.True);

        Assert.That(reloaded.ExportSave().ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.False);

        Assert.That(reloaded.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry), Is.True);

        Assert.That(reloaded.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce), Is.False);

    }


    [Test]

    public void LocalConflictM3Bootstrap_ActivatesConflictAndFeedsBackIntoOrder()

    {

        GameSimulation orderOnlySimulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(5151);

        GameSimulation localConflictSimulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(5151);


        orderOnlySimulation.AdvanceMonths(2);

        localConflictSimulation.AdvanceMonths(2);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle bundle = builder.BuildForM2(localConflictSimulation);

        SaveRoot saveRoot = localConflictSimulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();


        OrderAndBanditryState orderOnlyState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            orderOnlySimulation.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            saveRoot.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        ConflictAndForceState conflictState = (ConflictAndForceState)serializer.Deserialize(

            typeof(ConflictAndForceState),

            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);


        Assert.That(

            SimulationBootstrapper.CreateM3LocalConflictModules().Select(static module => module.ModuleKey),

            Does.Contain(KnownModuleKeys.OrderAndBanditry));

        Assert.That(

            SimulationBootstrapper.CreateM3LocalConflictModules().Select(static module => module.ModuleKey),

            Does.Contain(KnownModuleKeys.ConflictAndForce));

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OrderAndBanditry), Is.True);

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.ConflictAndForce), Is.True);

        Assert.That(localConflictSimulation.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce), Is.True);

        Assert.That(conflictState.Settlements, Has.Count.EqualTo(1));

        Assert.That(conflictState.Settlements.Single().Readiness, Is.GreaterThan(0));

        Assert.That(conflictState.Settlements.Single().LastConflictTrace, Is.Not.Empty);

        Assert.That(orderState.Settlements.Single().SuppressionDemand, Is.LessThan(orderOnlyState.Settlements.Single().SuppressionDemand));

        Assert.That(orderState.Settlements.Single().LastPressureReason, Is.Not.Empty);

        Assert.That(bundle.Notifications.Any(static notification => notification.SourceModuleKey == KnownModuleKeys.ConflictAndForce), Is.True);

        Assert.That(bundle.Notifications.Any(static notification => notification.Surface == NarrativeSurface.ConflictVignette), Is.True);

        Assert.That(bundle.Debug.ModuleInspectors.Any(static inspector => string.Equals(inspector.ModuleKey, KnownModuleKeys.ConflictAndForce, StringComparison.Ordinal)), Is.True);

        Assert.That(bundle.Debug.RecentDiffEntries.Any(static trace => trace.ModuleKey == KnownModuleKeys.ConflictAndForce), Is.True);

    }


    [Test]

    public void LocalConflictM3Bootstrap_SynchronizesForceSupportIntoOrderWithinFirstMonth()

    {

        GameSimulation orderOnlySimulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(5152);

        GameSimulation localConflictSimulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(5152);


        orderOnlySimulation.AdvanceMonths(1);

        localConflictSimulation.AdvanceMonths(1);


        MessagePackModuleStateSerializer serializer = new();

        SaveRoot orderOnlySave = orderOnlySimulation.ExportSave();

        SaveRoot localConflictSave = localConflictSimulation.ExportSave();


        OrderAndBanditryState orderOnlyState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            orderOnlySave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        OrderAndBanditryState localConflictOrderState = (OrderAndBanditryState)serializer.Deserialize(

            typeof(OrderAndBanditryState),

            localConflictSave.ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);

        ConflictAndForceState conflictState = (ConflictAndForceState)serializer.Deserialize(

            typeof(ConflictAndForceState),

            localConflictSave.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);


        ConflictAndForceModule conflictModule = new();

        QueryRegistry queries = new();

        conflictModule.RegisterQueries(conflictState, queries);

        LocalForcePoolSnapshot forceSnapshot = queries.GetRequired<IConflictAndForceQueries>().GetRequiredSettlementForce(conflictState.Settlements.Single().SettlementId);


        Assert.That(forceSnapshot.IsResponseActivated, Is.True);

        Assert.That(forceSnapshot.OrderSupportLevel, Is.GreaterThan(0));

        Assert.That(localConflictOrderState.Settlements.Single().SuppressionDemand, Is.LessThan(orderOnlyState.Settlements.Single().SuppressionDemand));

    }


    [Test]

    public void MigratedLocalConflictLoad_SurfacesRuntimeMigrationAndHotspotDebugInfo()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(8181);

        simulation.AdvanceMonths(3);


        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();

        ConflictAndForceState legacyConflictState = (ConflictAndForceState)serializer.Deserialize(

            typeof(ConflictAndForceState),

            saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);


        foreach (SettlementForceState settlement in legacyConflictState.Settlements)

        {

            settlement.ResponseActivationLevel = 0;

            settlement.OrderSupportLevel = 0;

            settlement.IsResponseActivated = false;

            settlement.HasActiveConflict = false;

        }


        saveRoot.ModuleStates[KnownModuleKeys.ConflictAndForce] = new ModuleStateEnvelope

        {

            ModuleKey = KnownModuleKeys.ConflictAndForce,

            ModuleSchemaVersion = 1,

            Payload = serializer.Serialize(typeof(ConflictAndForceState), legacyConflictState),

        };


        GameSimulation reloaded = SimulationBootstrapper.LoadM3LocalConflict(saveRoot);

        reloaded.AdvanceMonths(1);


        PresentationReadModelBuilder builder = new();

        PresentationReadModelBundle bundle = builder.BuildForM2(reloaded);


        Assert.That(bundle.Debug.LoadMigration.LoadOriginLabel, Is.EqualTo("SaveLoad"));

        Assert.That(bundle.Debug.LoadMigration.WasMigrationApplied, Is.True);

        Assert.That(bundle.Debug.LoadMigration.Steps.Any(static step => step.ScopeLabel == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 1 && step.TargetVersion == 2), Is.True);

        Assert.That(bundle.Debug.LoadMigration.Steps.Any(static step => step.ScopeLabel == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 2 && step.TargetVersion == 3), Is.True);

        Assert.That(bundle.Debug.LoadMigration.Steps.Any(static step => step.ScopeLabel == KnownModuleKeys.ConflictAndForce && step.SourceVersion == 3 && step.TargetVersion == 4), Is.True);

        Assert.That(bundle.Debug.LoadMigration.StepCount, Is.EqualTo(3));

        Assert.That(bundle.Debug.LoadMigration.ConsistencyPassed, Is.True);

        Assert.That(bundle.Debug.LoadMigration.ConsistencySummary, Does.Contain("module envelopes preserved"));

        Assert.That(bundle.Debug.LoadMigration.Warnings, Is.Empty);

        Assert.That(bundle.Debug.CurrentInteractionPressure.ActiveConflictSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.PeakSuppressionDemand, Is.GreaterThanOrEqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.OrderInterventionCarryoverSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.OrderAdministrativeAftermathSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.ShieldingDominantSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(bundle.Debug.CurrentInteractionPressure.BacklashDominantSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(bundle.Debug.CurrentPressureDistribution.StressedSettlements + bundle.Debug.CurrentPressureDistribution.CrisisSettlements, Is.GreaterThanOrEqualTo(0));

        Assert.That(bundle.Debug.CurrentScale.SettlementCount, Is.EqualTo(1));

        Assert.That(bundle.Debug.CurrentHotspots, Is.Not.Null);

        Assert.That(bundle.Debug.CurrentHotspots.All(static hotspot => !string.IsNullOrWhiteSpace(hotspot.SettlementName)), Is.True);

        Assert.That(bundle.Debug.CurrentHotspots.All(static hotspot => hotspot.RouteShielding >= 0), Is.True);

        Assert.That(bundle.Debug.CurrentHotspots.All(static hotspot => hotspot.RetaliationRisk >= 0), Is.True);

        Assert.That(bundle.Debug.CurrentHotspots.All(static hotspot => hotspot.InterventionCarryoverMonths >= 0), Is.True);

        Assert.That(bundle.Debug.CurrentHotspots.All(static hotspot => hotspot.AdministrativeTaskLoad >= 0), Is.True);

        Assert.That(bundle.Debug.CurrentHotspots.All(static hotspot => hotspot.PetitionBacklog >= 0), Is.True);

        Assert.That(bundle.Debug.CurrentPayloadSummary.LargestModulePayloadBytes, Is.GreaterThan(0));

        Assert.That(bundle.Debug.TopPayloadModules, Is.Not.Empty);

    }


    [Test]

    public void GovernanceBootstrap_ActivatesOfficeModuleAndProducesAppointedCareers()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260511);

        simulation.AdvanceMonths(12);


        SaveRoot saveRoot = simulation.ExportSave();

        MessagePackModuleStateSerializer serializer = new();

        OfficeAndCareerState officeState = (OfficeAndCareerState)serializer.Deserialize(

            typeof(OfficeAndCareerState),

            saveRoot.ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);


        Assert.That(simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer), Is.True);

        Assert.That(saveRoot.ModuleStates.ContainsKey(KnownModuleKeys.OfficeAndCareer), Is.True);

        Assert.That(officeState.People.Any(static career => career.HasAppointment), Is.True);

        Assert.That(officeState.Jurisdictions.Any(static jurisdiction => jurisdiction.JurisdictionLeverage > 0), Is.True);

        Assert.That(officeState.People.Any(static career => career.ServiceMonths > 0), Is.True);

        Assert.That(officeState.People.Any(static career => !string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask)), Is.True);

        Assert.That(officeState.People.Any(static career => !string.IsNullOrWhiteSpace(career.LastPetitionOutcome)), Is.True);

        Assert.That(officeState.Jurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.CurrentAdministrativeTask)), Is.True);

        Assert.That(bundle.Debug.EnabledModules.Any(static module => module.ModuleKey == KnownModuleKeys.OfficeAndCareer), Is.True);

        Assert.That(bundle.Debug.ModuleInspectors.Any(static inspector => inspector.ModuleKey == KnownModuleKeys.OfficeAndCareer), Is.True);

    }


    [Test]

    public void GovernanceBootstrap_PresentationBundle_ExposesReadOnlyOfficeSurface()

    {

        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260517);

        simulation.AdvanceMonths(8);


        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);


        Assert.That(bundle.OfficeCareers, Is.Not.Empty);

        Assert.That(bundle.OfficeJurisdictions, Is.Not.Empty);

        Assert.That(bundle.OfficeCareers.Any(static career => career.HasAppointment), Is.True);

        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.CurrentAdministrativeTask)), Is.True);

        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.AdministrativeTaskTier)), Is.True);

        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.PetitionOutcomeCategory)), Is.True);

        Assert.That(bundle.OfficeCareers.Any(static career => !string.IsNullOrWhiteSpace(career.AuthorityTrajectorySummary)), Is.True);

        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => jurisdiction.PetitionBacklog >= 0), Is.True);

        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.AdministrativeTaskTier)), Is.True);

        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.PetitionOutcomeCategory)), Is.True);

        Assert.That(bundle.OfficeJurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.LastAdministrativeTrace)), Is.True);

        Assert.That(bundle.GovernanceSettlements, Is.Not.Empty);

        Assert.That(bundle.GovernanceSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.GovernanceSummary)), Is.True);

        Assert.That(bundle.GovernanceSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.PublicPressureSummary)), Is.True);

        Assert.That(bundle.GovernanceSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.PublicMomentumSummary)), Is.True);

        Assert.That(bundle.GovernanceSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.SuggestedCommandName)), Is.True);

        Assert.That(bundle.GovernanceSettlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.SuggestedCommandPrompt)), Is.True);

        Assert.That(bundle.GovernanceFocus.UrgencyScore, Is.GreaterThan(0));

        Assert.That(bundle.GovernanceFocus.LeadSummary, Is.Not.Empty);

        Assert.That(bundle.GovernanceFocus.PublicPressureSummary, Is.Not.Empty);

        Assert.That(bundle.GovernanceFocus.PublicMomentumSummary, Is.Not.Empty);

        Assert.That(bundle.GovernanceDocket.SettlementId, Is.EqualTo(bundle.GovernanceFocus.SettlementId));

        Assert.That(bundle.GovernanceDocket.UrgencyScore, Is.EqualTo(bundle.GovernanceFocus.UrgencyScore));

        Assert.That(bundle.GovernanceDocket.Headline, Is.Not.Empty);

        Assert.That(bundle.GovernanceDocket.WhyNowSummary, Is.Not.Empty);

        Assert.That(bundle.GovernanceDocket.PublicMomentumSummary, Is.Not.Empty);

        Assert.That(bundle.GovernanceDocket.PhaseLabel, Is.Not.Empty);

        Assert.That(bundle.GovernanceDocket.PhaseSummary, Is.Not.Empty);

        Assert.That(bundle.GovernanceDocket.GuidanceSummary, Is.Not.Empty);

        Assert.That(

            string.IsNullOrWhiteSpace(bundle.GovernanceDocket.HandlingSummary)

            || bundle.GovernanceDocket.HasRecentReceipt,

            Is.True);

        Assert.That(bundle.HallDocket.LeadItem.LaneKey, Is.EqualTo(HallDocketLaneKeys.Governance));

        Assert.That(bundle.HallDocket.LeadItem.Headline, Is.EqualTo(bundle.GovernanceDocket.Headline));

        Assert.That(bundle.HallDocket.LeadItem.OrderingSummary, Is.Not.Empty);

        Assert.That(bundle.HallDocket.LeadItem.SourceProjectionKeys, Does.Contain(HallDocketSourceProjectionKeys.GovernanceDocket));

        Assert.That(bundle.HallDocket.LeadItem.SourceModuleKeys, Does.Contain(KnownModuleKeys.OfficeAndCareer));

        Assert.That(shell.GreatHall.GovernanceSummary, Is.Not.Empty);

        Assert.That(shell.DeskSandbox.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.GovernanceSummary)), Is.True);

        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.OfficeTitle)), Is.True);

        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.PressureSummary)), Is.True);

        Assert.That(shell.Office.Appointments.Any(static appointment => !string.IsNullOrWhiteSpace(appointment.PetitionOutcomeCategory)), Is.True);

        Assert.That(shell.Office.Jurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.PetitionSummary)), Is.True);

        Assert.That(shell.Office.Jurisdictions.Any(static jurisdiction => !string.IsNullOrWhiteSpace(jurisdiction.PetitionOutcomeCategory)), Is.True);

    }


    [Test]

    public void GovernanceBootstrap_RemainsDeterministicAcrossSixtyMonthsForMultipleSeeds()

    {

        int[] seeds = [20260518, 20260521];


        foreach (int seed in seeds)

        {

            GameSimulation firstSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(seed);

            GameSimulation secondSimulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(seed);


            firstSimulation.AdvanceMonths(60);

            secondSimulation.AdvanceMonths(60);


            Assert.That(secondSimulation.CurrentDate, Is.EqualTo(firstSimulation.CurrentDate));

            Assert.That(secondSimulation.ReplayHash, Is.EqualTo(firstSimulation.ReplayHash));

        }

    }


}
