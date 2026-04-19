using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
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
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.FamilyCore].ModuleSchemaVersion, Is.EqualTo(3));
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
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.OrderAndBanditry].ModuleSchemaVersion, Is.EqualTo(6));
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.TradeAndIndustry].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(orderState.Settlements.Any(static settlement => settlement.PaperCompliance >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.ImplementationDrag >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.RouteShielding >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.RetaliationRisk >= 0), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => string.Equals(settlement.LastInterventionCommandCode, PlayerCommandNames.FundLocalWatch, System.StringComparison.Ordinal)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.LastInterventionSummary)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => !string.IsNullOrWhiteSpace(settlement.LastInterventionOutcome)), Is.True);
        Assert.That(orderState.Settlements.Any(static settlement => settlement.InterventionCarryoverMonths == 1), Is.True);
        Assert.That(tradeState.Routes.Any(static route => !string.IsNullOrWhiteSpace(route.RouteConstraintLabel)), Is.True);
        Assert.That(tradeState.Routes.Any(static route => !string.IsNullOrWhiteSpace(route.LastRouteTrace)), Is.True);
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
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].ModuleSchemaVersion, Is.EqualTo(3));
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
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.OfficeAndCareer].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(reloadedState.People.Any(static career => career.HasAppointment), Is.True);
        Assert.That(reloadedState.People.Any(static career => career.ServiceMonths > 0), Is.True);
        Assert.That(reloadedState.People.Any(static career => !string.IsNullOrWhiteSpace(career.LastPetitionOutcome)), Is.True);
        Assert.That(reloadedState.People.Any(static career => career.AppointmentPressure >= 0), Is.True);
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
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.ConflictAndForce].ModuleSchemaVersion, Is.EqualTo(3));
        Assert.That(reloaded.ExportSave().ModuleStates[KnownModuleKeys.WarfareCampaign].ModuleSchemaVersion, Is.EqualTo(3));
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
