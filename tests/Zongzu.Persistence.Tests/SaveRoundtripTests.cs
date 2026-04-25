using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Persistence;

namespace Zongzu.Persistence.Tests;

[TestFixture]
public sealed class SaveRoundtripTests
{
    [Test]
    public void SaveCodec_RoundtripPreservesSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM0M1Bootstrap(20260418);
        simulation.AdvanceMonths(12);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM0M1(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.PersonRegistry,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.WorldSettlements,
            }));
    }

    [Test]
    public void SaveCodec_RoundtripPreservesM2LiteSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260419);
        simulation.AdvanceMonths(12);
        PresentationReadModelBundle beforeBundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        ClanSnapshot clan = beforeBundle.Clans.Single();
        new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersMediation,
            });

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM2(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.PersonRegistry,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.PublicLifeAndRumor,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));

        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState reloadedState = (FamilyCoreState)serializer.Deserialize(
            typeof(FamilyCoreState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.FamilyCore].Payload);
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.FamilyCore].ModuleSchemaVersion, Is.EqualTo(8));
        Assert.That(reloadedState.Clans.Any(static clanState => clanState.MediationMomentum > 0), Is.True);
        Assert.That(reloadedState.Clans.Any(static clanState => string.Equals(clanState.LastConflictCommandCode, PlayerCommandNames.InviteClanEldersMediation, System.StringComparison.Ordinal)), Is.True);
        Assert.That(reloadedState.Clans.Any(static clanState => !string.IsNullOrWhiteSpace(clanState.LastConflictOutcome)), Is.True);
        Assert.That(reloadedState.Clans.All(static clanState => clanState.MarriageAlliancePressure >= 0), Is.True);
        Assert.That(reloadedState.Clans.All(static clanState => clanState.HeirSecurity >= 0), Is.True);
        Assert.That(reloadedState.Clans.All(static clanState => clanState.LastLifecycleTrace is not null), Is.True);
    }

    [Test]
    public void SaveCodec_RoundtripPreservesOrderAndBanditryLiteSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(20260428);
        simulation.AdvanceMonths(12);
        var settlementId = new PresentationReadModelBuilder().BuildForM2(simulation).SettlementDisorder.Single().SettlementId;
        OrderAndBanditryState preCommandOrderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState preCommandSettlement = preCommandOrderState.Settlements.Single(settlement => settlement.SettlementId == settlementId);
        preCommandSettlement.ImplementationDrag = 0;
        preCommandSettlement.CoercionRisk = 0;
        preCommandSettlement.RetaliationRisk = 0;
        new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.PersonRegistry,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.PublicLifeAndRumor,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));

        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState orderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        TradeAndIndustryState tradeState = (TradeAndIndustryState)serializer.Deserialize(
            typeof(TradeAndIndustryState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.TradeAndIndustry].Payload);
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].ModuleSchemaVersion, Is.EqualTo(9));
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.TradeAndIndustry].ModuleSchemaVersion, Is.EqualTo(4));
        Assert.That(orderState.Settlements.Any(static settlement => settlement.PaperCompliance >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.ImplementationDrag >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.RouteShielding >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.RetaliationRisk >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => string.Equals(settlement.LastInterventionCommandCode, PlayerCommandNames.FundLocalWatch, System.StringComparison.Ordinal)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.LastInterventionSummary)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.LastInterventionOutcome)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.LastInterventionOutcomeCode == OrderInterventionOutcomeCodes.Accepted), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.LastInterventionTraceCode == OrderInterventionTraceCodes.AcceptedFollowThrough), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.InterventionCarryoverMonths == 1), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.RefusalCarryoverMonths == 0), Is.True);
        Assert.That(tradeState.Routes.Any(static route => !string.IsNullOrWhiteSpace(route.RouteConstraintLabel)), Is.True);
        Assert.That(tradeState.Routes.Any(static route => !string.IsNullOrWhiteSpace(route.LastRouteTrace)), Is.True);
    }

    [Test]
    public void SaveCodec_RoundtripPreservesPublicLifeOrderSocialMemoryResidue()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(20260425);
        simulation.AdvanceMonths(2);
        SettlementId settlementId = new PresentationReadModelBuilder().BuildForM2(simulation).SettlementDisorder.Single().SettlementId;

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });
        Assert.That(result.Accepted, Is.True);

        simulation.AdvanceOneMonth();

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(codec.Decode(bytes));

        MessagePackModuleStateSerializer serializer = new();
        SocialMemoryAndRelationsState socialState = (SocialMemoryAndRelationsState)serializer.Deserialize(
            typeof(SocialMemoryAndRelationsState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.SocialMemoryAndRelations].Payload);
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.SocialMemoryAndRelations].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(
            socialState.Memories.Any(static memory =>
                memory.Kind == SocialMemoryKinds.PublicOrderWatchObligation
                && memory.CauseKey == "order.public_life.fund_local_watch"
                && memory.Type == MemoryType.Favor
                && memory.Subtype == MemorySubtype.ProtectionFavor),
            Is.True);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(reloaded);
        Assert.That(bundle.SocialMemories.Any(static memory => memory.CauseKey == "order.public_life.fund_local_watch"), Is.True);
    }

    [Test]
    public void SaveCodec_RoundtripPreservesOrderRefusalTraceAndSocialMemoryResidue()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(20260426);
        simulation.AdvanceMonths(2);
        SettlementId settlementId = new PresentationReadModelBuilder().BuildForM2(simulation).SettlementDisorder.Single().SettlementId;
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.CoercionRisk = 82;
        settlement.RetaliationRisk = 65;
        settlement.ImplementationDrag = 20;

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.SuppressBanditry,
            });
        Assert.That(result.Accepted, Is.False);

        SaveCodec codec = new();
        GameSimulation traceReloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(codec.Decode(codec.Encode(simulation.ExportSave())));
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState reloadedOrderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            traceReloaded.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        SettlementDisorderState reloadedSettlement = reloadedOrderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        Assert.That(traceReloaded.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].ModuleSchemaVersion, Is.EqualTo(9));
        Assert.That(reloadedSettlement.LastInterventionOutcomeCode, Is.EqualTo(OrderInterventionOutcomeCodes.Refused));
        Assert.That(reloadedSettlement.LastInterventionRefusalCode, Is.EqualTo(OrderInterventionRefusalCodes.SuppressionRefused));
        Assert.That(reloadedSettlement.LastInterventionTraceCode, Is.EqualTo(OrderInterventionTraceCodes.SuppressionGroundRefusal));
        Assert.That(reloadedSettlement.RefusalCarryoverMonths, Is.EqualTo(1));
        Assert.That(reloadedSettlement.InterventionCarryoverMonths, Is.EqualTo(0));

        traceReloaded.AdvanceOneMonth();
        GameSimulation residueReloaded = SimulationBootstrapper.LoadM3OrderAndBanditry(codec.Decode(codec.Encode(traceReloaded.ExportSave())));
        SocialMemoryAndRelationsState socialState = (SocialMemoryAndRelationsState)serializer.Deserialize(
            typeof(SocialMemoryAndRelationsState),
            residueReloaded.ExportSave().ModuleStates[KnownModuleKeys.SocialMemoryAndRelations].Payload);
        Assert.That(
            socialState.Memories.Any(static memory =>
                memory.Kind == SocialMemoryKinds.PublicOrderSuppressionRefusalFear
                && memory.CauseKey == "order.public_life.suppress_banditry.refused"
                && memory.Type == MemoryType.Fear
                && memory.Subtype == MemorySubtype.PowerGrudge),
            Is.True);
    }

    [Test]
    public void SaveCodec_RoundtripPreservesPublicLifeOrderResponseTrace()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260427);
        simulation.AdvanceMonths(2);
        SettlementId settlementId = new PresentationReadModelBuilder().BuildForM2(simulation).SettlementDisorder.Single().SettlementId;
        OrderAndBanditryState orderState = simulation.GetModuleStateForTesting<OrderAndBanditryState>(
            KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState settlement = orderState.Settlements.Single(entry => entry.SettlementId == settlementId);
        settlement.ImplementationDrag = 50;
        settlement.RetaliationRisk = 12;
        settlement.CoercionRisk = 10;

        PlayerCommandService commandService = new();
        commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });
        simulation.AdvanceOneMonth();
        commandService.IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.RepairLocalWatchGuarantee,
            });

        SaveCodec codec = new();
        GameSimulation reloaded = SimulationBootstrapper.LoadP1GovernanceLocalConflict(codec.Decode(codec.Encode(simulation.ExportSave())));
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState reloadedOrderState = (OrderAndBanditryState)serializer.Deserialize(
            typeof(OrderAndBanditryState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].Payload);
        SettlementDisorderState reloadedSettlement = reloadedOrderState.Settlements.Single(entry => entry.SettlementId == settlementId);

        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].ModuleSchemaVersion, Is.EqualTo(9));
        Assert.That(reloadedSettlement.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.RepairLocalWatchGuarantee));
        Assert.That(reloadedSettlement.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Repaired));
        Assert.That(reloadedSettlement.LastRefusalResponseTraceCode, Is.EqualTo(PublicLifeOrderResponseTraceCodes.OrderWatchGuaranteeRepaired));
        Assert.That(reloadedSettlement.ResponseCarryoverMonths, Is.EqualTo(1));
    }

    [Test]
    public void SaveCodec_RoundtripPreservesLocalConflictLiteSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM3LocalConflictBootstrap(20260429);
        simulation.AdvanceMonths(12);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadM3LocalConflict(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.ConflictAndForce,
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.PersonRegistry,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.PublicLifeAndRumor,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));

        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState reloadedState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].ModuleSchemaVersion, Is.EqualTo(4));
        Assert.That(reloadedState.Settlements.Any(static settlement => settlement.ResponseActivationLevel > 0), Is.True);
    }

    [Test]
    public void SaveCodec_RoundtripPreservesGovernanceLiteSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260512);
        simulation.AdvanceMonths(12);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadP1GovernanceLocalConflict(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.ConflictAndForce,
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.OfficeAndCareer,
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.PersonRegistry,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.PublicLifeAndRumor,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WorldSettlements,
            }));

        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState reloadedState = (OfficeAndCareerState)serializer.Deserialize(
            typeof(OfficeAndCareerState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.OfficeAndCareer].Payload);
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.OfficeAndCareer].ModuleSchemaVersion, Is.EqualTo(7));
        Assert.That(reloadedState.People.Any(static career => career.HasAppointment), Is.True);
        Assert.That(reloadedState.People.Any(static career => career.ServiceMonths > 0), Is.True);
        Assert.That(reloadedState.People.Any(static career => !string.IsNullOrWhiteSpace(career.LastPetitionOutcome)), Is.True);
        Assert.That(reloadedState.People.Any(static career => career.AppointmentPressure >= 0), Is.True);
        Assert.That(reloadedState.ActiveClerkCaptureSettlementIds, Is.Not.Null);
    }

    [Test]
    public void SaveCodec_RoundtripPreservesCampaignSandboxSimulationState()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260521);
        simulation.AdvanceMonths(12);

        SaveCodec codec = new();
        byte[] bytes = codec.Encode(simulation.ExportSave());
        GameSimulation reloaded = SimulationBootstrapper.LoadP3CampaignSandbox(codec.Decode(bytes));

        Assert.That(reloaded.CurrentDate, Is.EqualTo(simulation.CurrentDate));
        Assert.That(reloaded.ReplayHash, Is.EqualTo(simulation.ReplayHash));
        Assert.That(
            reloaded.ExportSave().ModuleStates.Keys.OrderBy(static key => key).ToArray(),
            Is.EqualTo(new[]
            {
                KnownModuleKeys.ConflictAndForce,
                KnownModuleKeys.EducationAndExams,
                KnownModuleKeys.FamilyCore,
                KnownModuleKeys.NarrativeProjection,
                KnownModuleKeys.OfficeAndCareer,
                KnownModuleKeys.OrderAndBanditry,
                KnownModuleKeys.PersonRegistry,
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.PublicLifeAndRumor,
                KnownModuleKeys.SocialMemoryAndRelations,
                KnownModuleKeys.TradeAndIndustry,
                KnownModuleKeys.WarfareCampaign,
                KnownModuleKeys.WorldSettlements,
            }));

        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState reloadedState = (WarfareCampaignState)serializer.Deserialize(
            typeof(WarfareCampaignState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.WarfareCampaign].Payload);
        ConflictAndForceState conflictState = (ConflictAndForceState)serializer.Deserialize(
            typeof(ConflictAndForceState),
            reloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].Payload);
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].ModuleSchemaVersion, Is.EqualTo(4));
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.WarfareCampaign].ModuleSchemaVersion, Is.EqualTo(4));
        Assert.That(reloadedState.Campaigns, Is.Not.Empty);
        Assert.That(reloadedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.CommandFitLabel)), Is.True);
        Assert.That(reloadedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)), Is.True);
        Assert.That(reloadedState.Campaigns.Any(static campaign => !string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace)), Is.True);
        Assert.That(reloadedState.Campaigns.Any(static campaign => campaign.Routes.Count > 0), Is.True);
        Assert.That(reloadedState.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.MobilizationWindowLabel)), Is.True);
        Assert.That(reloadedState.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.CommandFitLabel)), Is.True);
        Assert.That(reloadedState.MobilizationSignals.Any(static signal => !string.IsNullOrWhiteSpace(signal.ActiveDirectiveLabel)), Is.True);
        Assert.That(conflictState.Settlements, Is.Not.Empty);
        Assert.That(
            conflictState.Settlements.All(static settlement =>
                settlement.CampaignFatigue >= 0
                && settlement.CampaignEscortStrain >= 0
                && settlement.LastCampaignFalloutTrace is not null),
            Is.True);
    }
}
