using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

[TestFixture]
public sealed class OrderAndBanditryPhase8SkeletonTests
{
    // Phase 8 匪患骨骼 — LIVING_WORLD_DESIGN §2.8
    [Test]
    public void BuildOrEvolveOutlawBands_SeedsFromHighBanditThreat()
    {
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(10),
            BanditThreat = 70,
            BlackRoutePressure = 40,
            CoercionRisk = 30,
            DisorderPressure = 50,
        });
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(11),
            BanditThreat = 20,
        });

        OrderAndBanditryStateProjection.BuildOrEvolveOutlawBands(state);

        Assert.That(state.OutlawBands.Count, Is.EqualTo(1));
        OutlawBandState band = state.OutlawBands[0];
        Assert.That(band.BaseSettlementId, Is.EqualTo(new SettlementId(10)));
        Assert.That(band.BandId, Is.EqualTo("outlaw-band-10"));
        Assert.That(band.Strength, Is.GreaterThan(0));
        Assert.That(band.Concentration, Is.Not.EqualTo(BandConcentration.Unknown));
        Assert.That(band.ControlledRoutes, Is.Empty);
    }

    [Test]
    public void BuildOrEvolveOutlawBands_HighStrengthAndLegitimacy_EscalatesConcentration()
    {
        OrderAndBanditryState state = new();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(20),
            BanditThreat = 95,
            BlackRoutePressure = 90,
            CoercionRisk = 95,
            DisorderPressure = 90,
        });

        OrderAndBanditryStateProjection.BuildOrEvolveOutlawBands(state);

        OutlawBandState band = state.OutlawBands[0];
        Assert.That(band.Concentration, Is.AnyOf(
            BandConcentration.TerritoryHolding,
            BandConcentration.RebelGovernance));
    }
}
