using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WarfareCampaign.Tests;

[TestFixture]
public sealed class WarfareCampaignPhase10SkeletonTests
{
    [Test]
    public void BuildCampaignPhasingAndAftermath_HighPressureLowMorale_ClassifiesStalemate()
    {
        WarfareCampaignState state = new();
        state.Campaigns.Add(new CampaignFrontState
        {
            CampaignId = new CampaignId(1),
            AnchorSettlementId = new SettlementId(11),
            AnchorSettlementName = "Lanxi",
            IsActive = true,
            FrontPressure = 72,
            MoraleState = 22,
            SupplyState = 35,
            MobilizedForceCount = 160,
            CommandFitLabel = "命令迟滞",
            ActiveDirectiveCode = WarfareCampaignCommandNames.CommitMobilization,
            Routes =
            {
                new CampaignRouteState { RouteLabel = "粮道", RouteRole = "supply", Pressure = 70, Security = 30 },
                new CampaignRouteState { RouteLabel = "驿报线", RouteRole = "command", Pressure = 40, Security = 55 },
            },
        });

        WarfareCampaignStateProjection.BuildCampaignPhasingAndAftermath(state);

        CampaignFrontState campaign = state.Campaigns.Single();
        Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Stalemate));
        Assert.That(campaign.CommittedForces, Is.EqualTo(160));
        Assert.That(campaign.SupplyStretch, Is.EqualTo(65));
        Assert.That(campaign.CommandFit, Is.EqualTo(42));
        Assert.That(campaign.CivilianExposure, Is.GreaterThan(0));
        Assert.That(campaign.ContestedRouteIds, Does.Contain("粮道"));
        Assert.That(campaign.ContestedRouteIds, Does.Not.Contain("驿报线"));
        Assert.That(state.AftermathDockets, Is.Empty);
    }

    [Test]
    public void BuildCampaignPhasingAndAftermath_AftermathSummaryPresent_EmitsDocket()
    {
        WarfareCampaignState state = new();
        state.Campaigns.Add(new CampaignFrontState
        {
            CampaignId = new CampaignId(2),
            AnchorSettlementId = new SettlementId(12),
            AnchorSettlementName = "Qingshui",
            IsActive = false,
            FrontPressure = 75,
            MoraleState = 28,
            SupplyState = 30,
            MobilizedForceCount = 140,
            CommandFitLabel = "案牍梗塞",
            LastAftermathSummary = "Qingshui战事暂告收束，文移已入案。",
            Routes =
            {
                new CampaignRouteState { RouteLabel = "粮道", RouteRole = "supply", Pressure = 80, Security = 25 },
            },
        });

        WarfareCampaignStateProjection.BuildCampaignPhasingAndAftermath(state);

        CampaignFrontState campaign = state.Campaigns.Single();
        Assert.That(campaign.Phase, Is.EqualTo(CampaignPhase.Aftermath));

        AftermathDocketState docket = state.AftermathDockets.Single();
        Assert.That(docket.CampaignId, Is.EqualTo(new CampaignId(2)));
        Assert.That(docket.Blames, Is.Not.Empty);
        Assert.That(docket.ReliefNeeds, Is.Not.Empty);
        Assert.That(docket.RouteRepairs.Any(static entry => entry.Contains("粮道")), Is.True);
        Assert.That(docket.DocketSummary, Does.Contain("Qingshui"));
    }
}
