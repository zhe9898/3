using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class SocialMobilityFidelityRingIntegrationTests
{
    [Test]
    public void MonthlyPressure_DrivesLivelihoodDriftFocusRingAndProjectedMobilityReadback()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260428);
        PersonRegistryState registryState = simulation.GetModuleStateForTesting<PersonRegistryState>(
            KnownModuleKeys.PersonRegistry);
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        PopulationHouseholdState household = populationState.Households
            .OrderBy(static entry => entry.Id.Value)
            .First();

        PersonId travelerId = new(9301);
        registryState.Persons.Add(new PersonRecord
        {
            Id = travelerId,
            DisplayName = "Li Xing",
            BirthDate = new GameDate(1181, 1),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Regional,
        });

        household.Livelihood = LivelihoodType.Tenant;
        household.Distress = 86;
        household.DebtPressure = 86;
        household.LaborCapacity = 24;
        household.MigrationRisk = 81;
        household.LandHolding = 6;
        household.GrainStore = 8;
        household.DependentCount = 2;
        household.LaborerCount = 1;
        populationState.Memberships.Add(new HouseholdMembershipState
        {
            PersonId = travelerId,
            HouseholdId = household.Id,
            Livelihood = LivelihoodType.Tenant,
            HealthResilience = 90,
            Health = HealthStatus.Healthy,
            Activity = PersonActivity.Farming,
        });

        simulation.AdvanceOneMonth();

        PersonRecord traveler = registryState.Persons.Single(person => person.Id == travelerId);
        HouseholdMembershipState membership = populationState.Memberships.Single(entry => entry.PersonId == travelerId);
        PresentationReadModelBundle bundle = new PresentationReadModelBuilder().BuildForM2(simulation);
        SettlementMobilitySnapshot mobility = bundle.SettlementMobilities.Single(entry =>
            entry.SettlementId == household.SettlementId);
        PersonDossierSnapshot dossier = bundle.PersonDossiers.Single(entry => entry.PersonId == travelerId);

        Assert.That(household.Livelihood, Is.EqualTo(LivelihoodType.Vagrant).Or.EqualTo(LivelihoodType.SeasonalMigrant));
        Assert.That(membership.Activity, Is.EqualTo(PersonActivity.Migrating));
        Assert.That(traveler.FidelityRing, Is.EqualTo(FidelityRing.Local));
        Assert.That(simulation.LastMonthResult!.DomainEvents.Any(evt =>
            evt.EventType == PersonRegistryEventNames.FidelityRingChanged), Is.True);
        Assert.That(bundle.FidelityScale.LocalPersonCount, Is.GreaterThanOrEqualTo(1));
        Assert.That(mobility.OutflowPressure, Is.GreaterThanOrEqualTo(60));
        Assert.That(mobility.SourceModuleKeys, Does.Contain(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(mobility.SourceModuleKeys, Does.Contain(KnownModuleKeys.PersonRegistry));
        Assert.That(mobility.PoolThicknessSummary, Does.Contain("PopulationAndHouseholds").Or.Contain("池").Or.Contain("姹"));
        Assert.That(mobility.MovementReadbackSummary, Does.Contain("PopulationAndHouseholds"));
        Assert.That(mobility.FocusReadbackSummary, Does.Contain("不是").Or.Contain("涓嶆槸").Or.Contain("区域池"));
        Assert.That(dossier.MovementReadbackSummary, Does.Contain("流徙池").Or.Contain(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(dossier.FidelityRingReadbackSummary, Does.Contain("近处").Or.Contain(KnownModuleKeys.PersonRegistry));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.PersonRegistry));
    }
}
