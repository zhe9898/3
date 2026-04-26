using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Integration.Tests;

public sealed partial class M2LiteIntegrationTests
{
    [Test]
    public void CampaignAftermathDocketReadback_ProjectsWarfareOwnedDocketWithoutOfficeOrderBackfill()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260627);
        simulation.AdvanceMonths(8);

        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(bundle.CampaignAftermathDockets, Is.Not.Empty);
        Assert.That(bundle.CampaignAftermathDockets.Any(static docket =>
            docket.Merits.Count + docket.Blames.Count + docket.ReliefNeeds.Count + docket.RouteRepairs.Count > 0), Is.True);

        PlayerCommandAffordanceSnapshot warfareAffordance = bundle.PlayerCommands.Affordances.First(command =>
            string.Equals(command.SurfaceKey, PlayerCommandSurfaceKeys.Warfare, StringComparison.Ordinal)
            && string.Equals(command.CommandName, PlayerCommandNames.ProtectSupplyLine, StringComparison.Ordinal));

        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("战后案卷读回"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("记功簿读回"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("劾责状读回"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("抚恤簿读回"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("清路札读回"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("WarfareCampaign拥有战后案卷"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("战后案卷不是县门/Order代算"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("不是普通家户补战后"));
        Assert.That(warfareAffordance.CampaignAftermathReadbackSummary, Does.Contain("军务案卷防回压"));

        Assert.That(bundle.GovernanceSettlements.Any(static governance =>
            governance.CampaignAftermathReadbackSummary.Contains("战后案卷读回", StringComparison.Ordinal)
            && governance.CampaignAftermathReadbackSummary.Contains("战后案卷不是县门/Order代算", StringComparison.Ordinal)), Is.True);
        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("堂案今并载"));
        Assert.That(shell.Warfare.CampaignBoards.Any(static board =>
            board.AftermathDocketSummary.Contains("军机案今并载", StringComparison.Ordinal)), Is.True);
    }
}
