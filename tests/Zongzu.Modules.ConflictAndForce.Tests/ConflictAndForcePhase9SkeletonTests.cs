using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce.Tests;

[TestFixture]
public sealed class ConflictAndForcePhase9SkeletonTests
{
    // Phase 9 武力骨骼 — LIVING_WORLD_DESIGN §2.9
    [Test]
    public void BuildForceGroupsAndIncidents_SplitsForceHeadcountsIntoTypedGroups()
    {
        ConflictAndForceState state = new();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(10),
            GuardCount = 4,
            RetainerCount = 6,
            MilitiaCount = 0,
            EscortCount = 2,
            Readiness = 55,
            OrderSupportLevel = 50,
        });

        ConflictAndForceStateProjection.BuildForceGroupsAndIncidents(state);

        Assert.That(state.ForceGroups.Count, Is.EqualTo(3));
        Assert.That(state.ForceGroups.Any(static g => g.Family == ForceFamily.HouseholdRetainer));
        Assert.That(state.ForceGroups.Any(static g => g.Family == ForceFamily.YamenForce));
        Assert.That(state.ForceGroups.Any(static g => g.Family == ForceFamily.EscortBand));
        Assert.That(state.ForceGroups.All(static g => g.Strength > 0));
        Assert.That(state.Incidents, Is.Empty);
    }

    [Test]
    public void BuildForceGroupsAndIncidents_ActiveConflict_ProjectsIncidentWithScale()
    {
        ConflictAndForceState state = new();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(20),
            GuardCount = 3,
            HasActiveConflict = true,
            CampaignFatigue = 60,
            CampaignEscortStrain = 45,
            OrderSupportLevel = 20,
            ResponseActivationLevel = 40,
        });

        ConflictAndForceStateProjection.BuildForceGroupsAndIncidents(state);

        ConflictIncidentState incident = state.Incidents.Single();
        Assert.That(incident.Location, Is.EqualTo(new SettlementId(20)));
        Assert.That(incident.IncidentId, Is.EqualTo("incident-20"));
        Assert.That(incident.Scale, Is.EqualTo(IncidentScale.CampaignBoard));
        Assert.That(incident.CauseKey, Is.Not.Empty);
    }
}
