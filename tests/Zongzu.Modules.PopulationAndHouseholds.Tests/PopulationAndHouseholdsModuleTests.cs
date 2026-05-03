using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.PopulationAndHouseholds.Tests;

[TestFixture]
public sealed class PopulationAndHouseholdsModuleTests
{
    [Test]
    public void RunXun_UpdatesLivelihoodPressureWithoutReadableOutput()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 38,
            Prosperity = 43,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 40,
            HeirPersonId = new PersonId(1),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 58,
            DebtPressure = 60,
            LaborCapacity = 55,
            MigrationRisk = 20,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(11)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff(),
            cadenceBand: SimulationCadenceBand.Xun,
            currentXun: SimulationXun.Shangxun);

        populationModule.RunXun(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        PopulationHouseholdState household = populationState.Households[0];
        Assert.That(household.Distress, Is.EqualTo(59));
        Assert.That(household.DebtPressure, Is.EqualTo(60));
        Assert.That(populationState.Settlements, Has.Count.EqualTo(1));
        Assert.That(populationState.Settlements[0].CommonerDistress, Is.EqualTo(59));
        Assert.That(context.Diff.Entries, Is.Empty);
        Assert.That(context.DomainEvents.Events, Is.Empty);
    }

    [Test]
    public void RunMonth_KeepsHouseholdPressuresInBoundsAndRebuildsSettlementSummary()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 38,
            Prosperity = 43,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 65,
            HeirPersonId = new PersonId(1),
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 50,
            DebtPressure = 49,
            LaborCapacity = 55,
            MigrationRisk = 20,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(11)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        PopulationHouseholdState household = populationState.Households[0];
        Assert.That(household.Distress, Is.InRange(0, 100));
        Assert.That(household.DebtPressure, Is.InRange(0, 100));
        Assert.That(household.LaborCapacity, Is.InRange(0, 100));
        Assert.That(household.MigrationRisk, Is.InRange(0, 100));
        Assert.That(populationState.Settlements, Has.Count.EqualTo(1));
        Assert.That(populationState.Settlements[0].LaborSupply, Is.EqualTo(household.LaborCapacity));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void HandleEvents_AppliesCampaignBurdenToHouseholdLivelihood()
    {
        PopulationAndHouseholdsModule module = new();
        PopulationAndHouseholdsState state = module.CreateInitialState();
        state.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 52,
            DebtPressure = 48,
            LaborCapacity = 60,
            MigrationRisk = 42,
        });
        state.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 52,
            LaborSupply = 60,
            MigrationPressure = 42,
            MilitiaPotential = 34,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(1),
                AnchorSettlementName = "Lanxi",
                CampaignName = "兰溪军务沙盘",
                IsActive = true,
                MobilizedForceCount = 48,
                FrontPressure = 75,
                FrontLabel = "前线转紧",
                SupplyState = 36,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 44,
                MoraleStateLabel = "军心摇动",
                LastAftermathSummary = "还乡兵丁与败车残辎壅在路上。",
            },
        ]));

        ModuleExecutionContext context = new(
            new GameDate(1200, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(41)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<PopulationAndHouseholdsState>(
            state,
            context,
            [
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignPressureRaised, "Lanxi pressure rose.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "1"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi entered aftermath review.", "1"),
            ]));

        PopulationHouseholdState household = state.Households[0];
        PopulationSettlementState settlement = state.Settlements[0];

        Assert.That(household.Distress, Is.GreaterThan(52));
        Assert.That(household.DebtPressure, Is.GreaterThan(48));
        Assert.That(household.MigrationRisk, Is.GreaterThan(42));
        Assert.That(household.LaborCapacity, Is.LessThan(60));
        Assert.That(settlement.CommonerDistress, Is.EqualTo(household.Distress));
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("战后余波"));
    }

    [Test]
    public void RunMonth_VagrantLivelihood_RaisesDistressAboveSmallholderBaseline()
    {
        PopulationHouseholdState smallholder = RunSingleHouseholdMonth(LivelihoodType.Smallholder);
        PopulationHouseholdState vagrant = RunSingleHouseholdMonth(LivelihoodType.Vagrant);

        Assert.That(vagrant.Distress, Is.GreaterThan(smallholder.Distress));
    }

    [Test]
    public void RunMonth_MoribundMembership_MarksPersonDeceasedAndEmitsDeathByIllness()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 38,
            Prosperity = 43,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();

        PersonRegistryModule registryModule = new();
        PersonRegistryState registryState = registryModule.CreateInitialState();
        registryState.Persons.Add(new PersonRecord
        {
            Id = new PersonId(9001),
            DisplayName = "李病",
            BirthDate = new GameDate(1170, 1),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Local,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Tenant,
            Distress = 85,
            DebtPressure = 60,
            LaborCapacity = 40,
            MigrationRisk = 20,
            DependentCount = 1,
        });
        populationState.Memberships.Add(new HouseholdMembershipState
        {
            PersonId = new PersonId(9001),
            HouseholdId = new HouseholdId(1),
            Livelihood = LivelihoodType.Tenant,
            HealthResilience = 20,
            Health = HealthStatus.Moribund,
            IllnessMonths = 3,
            Activity = PersonActivity.Convalescing,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        registryModule.RegisterQueries(registryState, queries);
        populationModule.RegisterQueries(populationState, queries);

        DomainEventBuffer eventBuffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(73)),
            queries,
            eventBuffer,
            new WorldDiff());

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        PersonRecord registered = registryState.Persons.Single(person => person.Id == new PersonId(9001));
        Assert.That(registered.IsAlive, Is.False);
        Assert.That(populationState.Memberships, Is.Empty);
        Assert.That(
            eventBuffer.Events.Any(record =>
                record.EventType == PopulationEventNames.DeathByIllness
                && record.EntityKey == "9001"),
            Is.True);
    }

    [Test]
    public void RunMonth_HighPressureHousehold_DriftsLivelihoodPromotesRegionalPersonAndRebuildsPools()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 34,
            Prosperity = 36,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();

        PersonRegistryModule registryModule = new();
        PersonRegistryState registryState = registryModule.CreateInitialState();
        registryState.Persons.Add(new PersonRecord
        {
            Id = new PersonId(9101),
            DisplayName = "李行",
            BirthDate = new GameDate(1180, 1),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Regional,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Tenant,
            Distress = 86,
            DebtPressure = 86,
            LaborCapacity = 24,
            MigrationRisk = 81,
            LandHolding = 6,
            GrainStore = 8,
            DependentCount = 2,
            LaborerCount = 1,
        });
        populationState.Memberships.Add(new HouseholdMembershipState
        {
            PersonId = new PersonId(9101),
            HouseholdId = new HouseholdId(1),
            Livelihood = LivelihoodType.Tenant,
            HealthResilience = 90,
            Health = HealthStatus.Healthy,
            Activity = PersonActivity.Farming,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        registryModule.RegisterQueries(registryState, queries);
        populationModule.RegisterQueries(populationState, queries);

        DomainEventBuffer eventBuffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(137)),
            queries,
            eventBuffer,
            new WorldDiff());

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        PopulationHouseholdState household = populationState.Households.Single();
        HouseholdMembershipState membership = populationState.Memberships.Single();
        PersonRecord person = registryState.Persons.Single();

        Assert.That(household.Livelihood, Is.EqualTo(LivelihoodType.Vagrant));
        Assert.That(household.IsMigrating, Is.True);
        Assert.That(membership.Livelihood, Is.EqualTo(LivelihoodType.Vagrant));
        Assert.That(membership.Activity, Is.EqualTo(PersonActivity.Migrating));
        Assert.That(person.FidelityRing, Is.EqualTo(FidelityRing.Local));
        Assert.That(populationState.LaborPools, Has.Count.EqualTo(1));
        Assert.That(populationState.MarriagePools, Has.Count.EqualTo(1));
        Assert.That(populationState.MigrationPools, Has.Count.EqualTo(1));
        Assert.That(populationState.MigrationPools[0].OutflowPressure, Is.GreaterThanOrEqualTo(80));
        Assert.That(eventBuffer.Events.Any(evt => evt.EventType == PersonRegistryEventNames.FidelityRingChanged), Is.True);
    }

    [Test]
    public void RunMonth_HighPressureHousehold_DefaultMobilityRulesDataPromotesTwoRegionalPeopleInPersonIdOrder()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 34,
            Prosperity = 36,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();

        PersonRegistryModule registryModule = new();
        PersonRegistryState registryState = registryModule.CreateInitialState();
        registryState.Persons.Add(new PersonRecord
        {
            Id = new PersonId(9301),
            DisplayName = "Regional A",
            BirthDate = new GameDate(1180, 1),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Regional,
        });
        registryState.Persons.Add(new PersonRecord
        {
            Id = new PersonId(9302),
            DisplayName = "Regional B",
            BirthDate = new GameDate(1181, 1),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Regional,
        });
        registryState.Persons.Add(new PersonRecord
        {
            Id = new PersonId(9303),
            DisplayName = "Regional C",
            BirthDate = new GameDate(1182, 1),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Regional,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Tenant Li",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.Tenant,
            Distress = 86,
            DebtPressure = 86,
            LaborCapacity = 24,
            MigrationRisk = 81,
            LandHolding = 6,
            GrainStore = 8,
            DependentCount = 2,
            LaborerCount = 1,
        });
        foreach (PersonRecord person in registryState.Persons)
        {
            populationState.Memberships.Add(new HouseholdMembershipState
            {
                PersonId = person.Id,
                HouseholdId = new HouseholdId(1),
                Livelihood = LivelihoodType.Tenant,
                HealthResilience = 90,
                Health = HealthStatus.Healthy,
                Activity = PersonActivity.Farming,
            });
        }

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        registryModule.RegisterQueries(registryState, queries);
        populationModule.RegisterQueries(populationState, queries);

        DomainEventBuffer eventBuffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(139)),
            queries,
            eventBuffer,
            new WorldDiff());

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        IReadOnlyDictionary<PersonId, PersonRecord> peopleById = registryState.Persons
            .ToDictionary(static person => person.Id, static person => person);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultFocusedMemberPromotionCap,
            Is.EqualTo(2));
        Assert.That(peopleById[new PersonId(9301)].FidelityRing, Is.EqualTo(FidelityRing.Local));
        Assert.That(peopleById[new PersonId(9302)].FidelityRing, Is.EqualTo(FidelityRing.Local));
        Assert.That(peopleById[new PersonId(9303)].FidelityRing, Is.EqualTo(FidelityRing.Regional));
        Assert.That(
            eventBuffer.Events.Count(evt => evt.EventType == PersonRegistryEventNames.FidelityRingChanged),
            Is.EqualTo(2));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidFocusedMemberPromotionCapFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData = new(-1);

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Count.EqualTo(1));
        Assert.That(validation.Errors[0], Does.Contain("focused_member_promotion_cap"));
        Assert.That(
            rulesData.GetFocusedMemberPromotionCapOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultFocusedMemberPromotionCap));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleTouchesOnlyCappedEligibleHouseholdsInActivePool()
    {
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 });
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(PopulationHouseholdMobilityRulesData.Default);

        Assert.That(
            GetHousehold(actual.State, 1).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 1).MigrationRisk + 1));
        Assert.That(
            GetHousehold(actual.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk + 1));

        Assert.That(
            GetHousehold(actual.State, 3).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 3).MigrationRisk),
            "The household cap leaves the lower-priority pressure-hit household untouched.");
        Assert.That(
            GetHousehold(actual.State, 4).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 4).MigrationRisk),
            "Quiet households below the candidate floor remain untouched.");
        Assert.That(
            GetHousehold(actual.State, 5).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 5).MigrationRisk),
            "The settlement cap leaves the lower-priority active pool untouched.");
        Assert.That(
            GetHousehold(actual.State, 6).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 6).MigrationRisk),
            "The settlement cap leaves the lower-priority active pool untouched.");

        Assert.That(actual.State.MigrationPools, Has.Count.EqualTo(baseline.State.MigrationPools.Count));
        Assert.That(actual.Diff.Entries.Count, Is.EqualTo(baseline.Diff.Entries.Count + 2));
        Assert.That(
            actual.Diff.Entries
                .Where(entry => entry.Description.Contains("Household mobility pressure"))
                .Select(entry => entry.EntityKey),
            Is.EquivalentTo(new[] { "1", "2" }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultCapsTouchOnlyOnePoolAndTwoHouseholds()
    {
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 });
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(PopulationHouseholdMobilityRulesData.Default);

        int[] selectedPoolTouchedHouseholds = actual.State.Households
            .Where(household =>
                GetHousehold(actual.State, household.Id.Value).MigrationRisk
                == GetHousehold(baseline.State, household.Id.Value).MigrationRisk + 1)
            .Select(static household => household.Id.Value)
            .OrderBy(static householdId => householdId)
            .ToArray();
        int[] selectedPoolTouchedSettlements = selectedPoolTouchedHouseholds
            .Select(householdId => GetHousehold(actual.State, householdId).SettlementId.Value)
            .Distinct()
            .OrderBy(static settlementId => settlementId)
            .ToArray();
        int[] monthlyRuntimeDiffKeys = actual.Diff.Entries
            .Where(static entry => entry.Description.Contains("Household mobility pressure"))
            .Select(static entry => int.Parse(entry.EntityKey!))
            .OrderBy(static householdId => householdId)
            .ToArray();

        Assert.That(selectedPoolTouchedHouseholds, Is.EqualTo(new[] { 1, 2 }));
        Assert.That(selectedPoolTouchedSettlements, Is.EqualTo(new[] { 1 }));
        Assert.That(monthlyRuntimeDiffKeys, Is.EqualTo(selectedPoolTouchedHouseholds));

        Assert.That(GetHousehold(actual.State, 3).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 3).MigrationRisk));
        Assert.That(GetHousehold(actual.State, 4).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 4).MigrationRisk));
        Assert.That(GetHousehold(actual.State, 5).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 5).MigrationRisk));
        Assert.That(GetHousehold(actual.State, 6).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 6).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleReplaySameSeedStable()
    {
        PopulationMobilityRunResult first = RunFirstMobilityRuntimeScenario(PopulationHouseholdMobilityRulesData.Default);
        PopulationMobilityRunResult second = RunFirstMobilityRuntimeScenario(PopulationHouseholdMobilityRulesData.Default);

        Assert.That(BuildFirstMobilityRuntimeSignature(second), Is.EqualTo(BuildFirstMobilityRuntimeSignature(first)));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleActivePoolThresholdNoTouchesHouseholdsOrPools()
    {
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 });
        PopulationMobilityRunResult thresholdBlocked = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeActivePoolOutflowThreshold = 100 });

        Assert.That(
            baseline.State.MigrationPools.Select(static pool => pool.OutflowPressure),
            Is.All.LessThan(100),
            "The fixture must keep every pool below the maximum threshold for this no-touch proof.");
        Assert.That(
            BuildFirstMobilityRuntimeSignature(thresholdBlocked),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(baseline)));
        Assert.That(
            thresholdBlocked.Diff.Entries.Where(static entry => entry.Description.Contains("Household mobility pressure")),
            Is.Empty);
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleZeroCapsNoTouchHouseholdsOrPools()
    {
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 });
        PopulationMobilityRunResult settlementCapBlocked = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeSettlementCap = 0 });
        PopulationMobilityRunResult householdCapBlocked = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 0 });

        Assert.That(
            BuildFirstMobilityRuntimeSignature(settlementCapBlocked),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(baseline)));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(householdCapBlocked),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(baseline)));
        Assert.That(
            settlementCapBlocked.Diff.Entries.Where(static entry => entry.Description.Contains("Household mobility pressure")),
            Is.Empty);
        Assert.That(
            householdCapBlocked.Diff.Entries.Where(static entry => entry.Description.Contains("Household mobility pressure")),
            Is.Empty);
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleZeroRiskDeltaNoTouchHouseholdsOrPools()
    {
        PopulationMobilityRunResult settlementCapBlocked = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeSettlementCap = 0 });
        PopulationMobilityRunResult riskDeltaBlocked = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 });

        Assert.That(
            BuildFirstMobilityRuntimeSignature(riskDeltaBlocked),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(settlementCapBlocked)));
        Assert.That(
            riskDeltaBlocked.Diff.Entries.Where(static entry => entry.Description.Contains("Household mobility pressure")),
            Is.Empty);
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleCandidateFiltersNoTouchMigratingHighRiskOrBelowFloorHouseholds()
    {
        static void ConfigureCandidateFilters(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState alreadyMigrating = GetHousehold(state, 1);
            alreadyMigrating.MigrationRisk = 82;
            alreadyMigrating.IsMigrating = true;

            PopulationHouseholdState belowFloor = GetHousehold(state, 2);
            belowFloor.MigrationRisk = 52;
        }

        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureCandidateFilters);
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureCandidateFilters);

        Assert.That(GetHousehold(actual.State, 1).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 1).MigrationRisk));
        Assert.That(GetHousehold(actual.State, 2).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(GetHousehold(actual.State, 3).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 3).MigrationRisk + 1));
        Assert.That(
            actual.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 3 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskFloorPreservesPreviousNoTouchBehavior()
    {
        static void ConfigureCandidateFloorFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState belowFloor = GetHousehold(state, 2);
            belowFloor.MigrationRisk = 52;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultFloorRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeCandidateMigrationRiskFloor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskFloor,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureCandidateFloorFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureCandidateFloorFixture);
        PopulationMobilityRunResult explicitDefaultFloorResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultFloorRules,
            ConfigureCandidateFloorFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskFloor,
            Is.EqualTo(55));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultFloorResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultFloorResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskCeilingPreservesPreviousNoTouchBehavior()
    {
        static void ConfigureCandidateCeilingFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState atCeiling = GetHousehold(state, 2);
            atCeiling.MigrationRisk = 80;
            atCeiling.IsMigrating = false;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultCeilingRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeCandidateMigrationRiskCeiling =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskCeiling,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureCandidateCeilingFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureCandidateCeilingFixture);
        PopulationMobilityRunResult explicitDefaultCeilingResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultCeilingRules,
            ConfigureCandidateCeilingFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskCeiling,
            Is.EqualTo(80));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultCeilingResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultCeilingResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultDistressTriggerThresholdPreservesPreviousNoTouchBehavior()
    {
        static void ConfigureDistressTriggerFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState belowDistressTrigger = GetHousehold(state, 2);
            belowDistressTrigger.Livelihood = LivelihoodType.Smallholder;
            belowDistressTrigger.MigrationRisk = 70;
            belowDistressTrigger.Distress = 59;
            belowDistressTrigger.DebtPressure = 55;
            belowDistressTrigger.LaborCapacity = 50;
            belowDistressTrigger.GrainStore = 30;
            belowDistressTrigger.LandHolding = 20;
            belowDistressTrigger.IsMigrating = false;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultDistressTriggerRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeDistressTriggerThreshold =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressTriggerThreshold,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureDistressTriggerFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureDistressTriggerFixture);
        PopulationMobilityRunResult explicitDefaultDistressTriggerResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultDistressTriggerRules,
            ConfigureDistressTriggerFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressTriggerThreshold,
            Is.EqualTo(60));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultDistressTriggerResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultDistressTriggerResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultDebtPressureTriggerThresholdPreservesPreviousNoTouchBehavior()
    {
        static void ConfigureDebtTriggerFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState belowDebtTrigger = GetHousehold(state, 2);
            belowDebtTrigger.Livelihood = LivelihoodType.Smallholder;
            belowDebtTrigger.MigrationRisk = 70;
            belowDebtTrigger.Distress = 55;
            belowDebtTrigger.DebtPressure = 59;
            belowDebtTrigger.LaborCapacity = 50;
            belowDebtTrigger.GrainStore = 30;
            belowDebtTrigger.LandHolding = 20;
            belowDebtTrigger.IsMigrating = false;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultDebtTriggerRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeDebtPressureTriggerThreshold =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureTriggerThreshold,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureDebtTriggerFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureDebtTriggerFixture);
        PopulationMobilityRunResult explicitDefaultDebtTriggerResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultDebtTriggerRules,
            ConfigureDebtTriggerFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureTriggerThreshold,
            Is.EqualTo(60));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultDebtTriggerResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultDebtTriggerResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultLaborCapacityTriggerCeilingPreservesPreviousNoTouchBehavior()
    {
        static void ConfigureLaborTriggerFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState atLaborTriggerCeiling = GetHousehold(state, 2);
            atLaborTriggerCeiling.Livelihood = LivelihoodType.Smallholder;
            atLaborTriggerCeiling.MigrationRisk = 70;
            atLaborTriggerCeiling.Distress = 55;
            atLaborTriggerCeiling.DebtPressure = 55;
            atLaborTriggerCeiling.LaborCapacity = 45;
            atLaborTriggerCeiling.GrainStore = 30;
            atLaborTriggerCeiling.LandHolding = 20;
            atLaborTriggerCeiling.IsMigrating = false;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultLaborTriggerRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeLaborCapacityTriggerCeiling =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityTriggerCeiling,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureLaborTriggerFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureLaborTriggerFixture);
        PopulationMobilityRunResult explicitDefaultLaborTriggerResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultLaborTriggerRules,
            ConfigureLaborTriggerFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityTriggerCeiling,
            Is.EqualTo(45));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultLaborTriggerResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultLaborTriggerResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultGrainStoreTriggerFloorPreservesPreviousNoTouchBehavior()
    {
        static void ConfigureGrainTriggerFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState atGrainTriggerFloor = GetHousehold(state, 2);
            atGrainTriggerFloor.Livelihood = LivelihoodType.Smallholder;
            atGrainTriggerFloor.MigrationRisk = 70;
            atGrainTriggerFloor.Distress = 55;
            atGrainTriggerFloor.DebtPressure = 55;
            atGrainTriggerFloor.LaborCapacity = 50;
            atGrainTriggerFloor.GrainStore = 25;
            atGrainTriggerFloor.LandHolding = 20;
            atGrainTriggerFloor.IsMigrating = false;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultGrainTriggerRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeGrainStoreTriggerFloor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStoreTriggerFloor,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureGrainTriggerFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureGrainTriggerFixture);
        PopulationMobilityRunResult explicitDefaultGrainTriggerResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultGrainTriggerRules,
            ConfigureGrainTriggerFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStoreTriggerFloor,
            Is.EqualTo(25));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultGrainTriggerResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultGrainTriggerResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultLandHoldingTriggerFloorPreservesPreviousNoTouchBehavior()
    {
        static void ConfigureLandTriggerFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState atLandTriggerFloor = GetHousehold(state, 2);
            atLandTriggerFloor.Livelihood = LivelihoodType.Smallholder;
            atLandTriggerFloor.MigrationRisk = 70;
            atLandTriggerFloor.Distress = 55;
            atLandTriggerFloor.DebtPressure = 55;
            atLandTriggerFloor.LaborCapacity = 50;
            atLandTriggerFloor.GrainStore = 30;
            atLandTriggerFloor.LandHolding = 15;
            atLandTriggerFloor.IsMigrating = false;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultLandTriggerRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeLandHoldingTriggerFloor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingTriggerFloor,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureLandTriggerFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureLandTriggerFixture);
        PopulationMobilityRunResult explicitDefaultLandTriggerResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultLandTriggerRules,
            ConfigureLandTriggerFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingTriggerFloor,
            Is.EqualTo(15));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultLandTriggerResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultLandTriggerResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultTriggerLivelihoodsPreservePreviousCandidateBehavior()
    {
        static void ConfigureLivelihoodTriggerFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState triggerLivelihood = GetHousehold(state, 2);
            triggerLivelihood.Livelihood = LivelihoodType.HiredLabor;
            triggerLivelihood.MigrationRisk = 70;
            triggerLivelihood.Distress = 55;
            triggerLivelihood.DebtPressure = 55;
            triggerLivelihood.LaborCapacity = 50;
            triggerLivelihood.GrainStore = 30;
            triggerLivelihood.LandHolding = 20;
            triggerLivelihood.IsMigrating = false;
        }

        PopulationHouseholdMobilityRulesData explicitDefaultLivelihoodTriggerRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeTriggerLivelihoods =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeTriggerLivelihoods,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureLivelihoodTriggerFixture);
        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigureLivelihoodTriggerFixture);
        PopulationMobilityRunResult explicitDefaultLivelihoodTriggerResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultLivelihoodTriggerRules,
            ConfigureLivelihoodTriggerFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeTriggerLivelihoods,
            Is.EqualTo(new[] { LivelihoodType.SeasonalMigrant, LivelihoodType.HiredLabor }));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultLivelihoodTriggerResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            GetHousehold(defaultResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
        Assert.That(
            GetHousehold(explicitDefaultLivelihoodTriggerResult.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleTieBreakTouchesLowerHouseholdIdWhenScoresMatch()
    {
        static void ConfigureTieBreakFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState lowerId = GetHousehold(state, 1);
            lowerId.Livelihood = LivelihoodType.Tenant;
            lowerId.Distress = 62;
            lowerId.DebtPressure = 64;
            lowerId.LaborCapacity = 42;
            lowerId.MigrationRisk = 76;
            lowerId.LandHolding = 12;
            lowerId.GrainStore = 20;

            PopulationHouseholdState higherId = GetHousehold(state, 2);
            higherId.Livelihood = LivelihoodType.Tenant;
            higherId.Distress = 61;
            higherId.DebtPressure = 64;
            higherId.LaborCapacity = 42;
            higherId.MigrationRisk = 76;
            higherId.LandHolding = 12;
            higherId.GrainStore = 20;
        }

        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            cappedRules with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureTieBreakFixture);
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(
            cappedRules,
            ConfigureTieBreakFixture);

        Assert.That(
            ComputeFirstMobilityRuntimeScoreForTest(GetHousehold(baseline.State, 1)),
            Is.EqualTo(ComputeFirstMobilityRuntimeScoreForTest(GetHousehold(baseline.State, 2))),
            "The fixture must reach the owner-rule ordering step with tied runtime scores.");
        Assert.That(GetHousehold(actual.State, 1).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 1).MigrationRisk + 1));
        Assert.That(
            GetHousehold(actual.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk),
            "The higher household id remains no-touch when cap one is consumed by the lower-id tie-break winner.");
        Assert.That(
            actual.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRulePoolTieBreakTouchesLowerSettlementIdWhenOutflowsMatch()
    {
        static void ConfigurePoolTieBreakFixture(PopulationAndHouseholdsState state)
        {
            GetHousehold(state, 5).MigrationRisk = 64;
            GetHousehold(state, 6).MigrationRisk = 64;
        }

        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeRiskDelta = 0 },
            ConfigurePoolTieBreakFixture);
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(
            PopulationHouseholdMobilityRulesData.Default,
            ConfigurePoolTieBreakFixture);

        Assert.That(
            GetMigrationPool(baseline.State, 1).OutflowPressure,
            Is.EqualTo(GetMigrationPool(baseline.State, 2).OutflowPressure),
            "The fixture must reach the active-pool ordering step with tied outflow pressure.");
        Assert.That(GetHousehold(actual.State, 1).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 1).MigrationRisk + 1));
        Assert.That(GetHousehold(actual.State, 2).MigrationRisk, Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk + 1));
        Assert.That(
            GetHousehold(actual.State, 5).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 5).MigrationRisk),
            "The higher settlement id remains no-touch when cap one is consumed by the lower-settlement-id pool.");
        Assert.That(
            GetHousehold(actual.State, 6).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 6).MigrationRisk),
            "The higher settlement id remains no-touch when cap one is consumed by the lower-settlement-id pool.");
        Assert.That(
            actual.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!))
                .OrderBy(static householdId => householdId),
            Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleScoreOrderingTouchesHigherScoreBeforeLowerHouseholdId()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            cappedRules with { MonthlyRuntimeRiskDelta = 0 });
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(cappedRules);

        Assert.That(
            ComputeFirstMobilityRuntimeScoreForTest(GetHousehold(baseline.State, 2)),
            Is.GreaterThan(ComputeFirstMobilityRuntimeScoreForTest(GetHousehold(baseline.State, 1))),
            "The fixture must reach candidate ordering with household 2 scoring above the lower household id.");
        Assert.That(
            GetHousehold(actual.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk + 1));
        Assert.That(
            GetHousehold(actual.State, 1).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 1).MigrationRisk),
            "The lower household id remains no-touch when cap one is consumed by the higher-score candidate.");
        Assert.That(
            GetHousehold(actual.State, 3).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 3).MigrationRisk));
        Assert.That(
            actual.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultMigrationRiskScoreWeightPreservesPreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultWeightRules =
            cappedRules with
            {
                MonthlyRuntimeMigrationRiskScoreWeight =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskScoreWeight,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultWeightResult = RunFirstMobilityRuntimeScenario(explicitDefaultWeightRules);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskScoreWeight,
            Is.EqualTo(4));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultWeightResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultWeightResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultPressureScoreWeightsPreservePreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultPressureScoreRules =
            cappedRules with
            {
                MonthlyRuntimeDistressScoreWeight =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressScoreWeight,
                MonthlyRuntimeDebtPressureScoreWeight =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureScoreWeight,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultPressureScoreResult =
            RunFirstMobilityRuntimeScenario(explicitDefaultPressureScoreRules);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressScoreWeight, Is.EqualTo(1));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureScoreWeight, Is.EqualTo(1));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultPressureScoreResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultPressureScoreResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultLivelihoodScoreWeightsPreservePreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultLivelihoodScoreRules =
            cappedRules with
            {
                MonthlyRuntimeLivelihoodScoreWeights =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLivelihoodScoreWeights,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultLivelihoodScoreResult =
            RunFirstMobilityRuntimeScenario(explicitDefaultLivelihoodScoreRules);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLivelihoodScoreWeights,
            Is.EqualTo(new[]
            {
                new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.SeasonalMigrant, 18),
                new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.HiredLabor, 10),
                new PopulationHouseholdMobilityLivelihoodScoreWeight(LivelihoodType.Tenant, 6),
            }));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultLivelihoodScoreResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultLivelihoodScoreResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultLaborCapacityPressureFloorPreservesPreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultLaborFloorRules =
            cappedRules with
            {
                MonthlyRuntimeLaborCapacityPressureFloor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityPressureFloor,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultLaborFloorResult =
            RunFirstMobilityRuntimeScenario(explicitDefaultLaborFloorRules);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityPressureFloor,
            Is.EqualTo(60));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultLaborFloorResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultLaborFloorResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultGrainStorePressureFloorPreservesPreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultGrainFloorRules =
            cappedRules with
            {
                MonthlyRuntimeGrainStorePressureFloor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureFloor,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultGrainFloorResult =
            RunFirstMobilityRuntimeScenario(explicitDefaultGrainFloorRules);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureFloor,
            Is.EqualTo(25));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultGrainFloorResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultGrainFloorResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultGrainStorePressureDivisorPreservesPreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultGrainDivisorRules =
            cappedRules with
            {
                MonthlyRuntimeGrainStorePressureDivisor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureDivisor,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultGrainDivisorResult =
            RunFirstMobilityRuntimeScenario(explicitDefaultGrainDivisorRules);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureDivisor,
            Is.EqualTo(2));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultGrainDivisorResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultGrainDivisorResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultLandHoldingPressureFloorPreservesPreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultLandFloorRules =
            cappedRules with
            {
                MonthlyRuntimeLandHoldingPressureFloor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureFloor,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultLandFloorResult =
            RunFirstMobilityRuntimeScenario(explicitDefaultLandFloorRules);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureFloor,
            Is.EqualTo(20));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultLandFloorResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultLandFloorResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultLandHoldingPressureDivisorPreservesPreviousScoreOrdering()
    {
        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultLandDivisorRules =
            cappedRules with
            {
                MonthlyRuntimeLandHoldingPressureDivisor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureDivisor,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(cappedRules);
        PopulationMobilityRunResult explicitDefaultLandDivisorResult =
            RunFirstMobilityRuntimeScenario(explicitDefaultLandDivisorRules);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureDivisor,
            Is.EqualTo(2));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultLandDivisorResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(
            explicitDefaultLandDivisorResult.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRulePoolPriorityPrecedesCrossPoolHouseholdScore()
    {
        static void ConfigurePoolPriorityFixture(PopulationAndHouseholdsState state)
        {
            PopulationHouseholdState selectedPoolOutflowCarrier = GetHousehold(state, 4);
            selectedPoolOutflowCarrier.MigrationRisk = 79;

            PopulationHouseholdState offPoolHighScore = GetHousehold(state, 5);
            offPoolHighScore.Livelihood = LivelihoodType.HiredLabor;
            offPoolHighScore.Distress = 75;
            offPoolHighScore.DebtPressure = 78;
            offPoolHighScore.LaborCapacity = 35;
            offPoolHighScore.MigrationRisk = 78;
            offPoolHighScore.LandHolding = 8;
            offPoolHighScore.GrainStore = 20;

            PopulationHouseholdState offPoolLowRiskBalance = GetHousehold(state, 6);
            offPoolLowRiskBalance.MigrationRisk = 35;
        }

        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeSettlementCap = 1,
                MonthlyRuntimeHouseholdCap = 1,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            cappedRules with { MonthlyRuntimeRiskDelta = 0 },
            ConfigurePoolPriorityFixture);
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(
            cappedRules,
            ConfigurePoolPriorityFixture);

        int selectedPoolBestScore = new[] { 1, 2, 3, 4 }
            .Select(householdId => ComputeFirstMobilityRuntimeScoreForTest(GetHousehold(baseline.State, householdId)))
            .Max();

        Assert.That(
            GetMigrationPool(baseline.State, 1).OutflowPressure,
            Is.GreaterThan(GetMigrationPool(baseline.State, 2).OutflowPressure),
            "The fixture must prove active-pool priority before any cross-pool household score comparison.");
        Assert.That(
            ComputeFirstMobilityRuntimeScoreForTest(GetHousehold(baseline.State, 5)),
            Is.GreaterThan(selectedPoolBestScore),
            "The off-pool household must have the stronger runtime score while remaining outside the selected pool.");
        Assert.That(
            GetHousehold(actual.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk + 1));
        Assert.That(
            GetHousehold(actual.State, 5).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 5).MigrationRisk),
            "The higher-scoring off-pool household remains no-touch because active-pool priority is applied before household score.");
        Assert.That(
            GetHousehold(actual.State, 6).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 6).MigrationRisk),
            "The lower-priority pool remains no-touch when settlement cap one is consumed by the higher-outflow pool.");
        Assert.That(
            actual.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleHouseholdCapAppliesPerSelectedPool()
    {
        PopulationHouseholdMobilityRulesData perPoolCapRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeSettlementCap = 2,
                MonthlyRuntimeHouseholdCap = 1,
            };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            perPoolCapRules with { MonthlyRuntimeRiskDelta = 0 });
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(perPoolCapRules);

        Assert.That(
            GetMigrationPool(baseline.State, 1).OutflowPressure,
            Is.GreaterThanOrEqualTo(perPoolCapRules.GetMonthlyRuntimeActivePoolOutflowThresholdOrDefault()));
        Assert.That(
            GetMigrationPool(baseline.State, 2).OutflowPressure,
            Is.GreaterThanOrEqualTo(perPoolCapRules.GetMonthlyRuntimeActivePoolOutflowThresholdOrDefault()));
        Assert.That(
            GetHousehold(actual.State, 2).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 2).MigrationRisk + 1),
            "The first selected pool receives its own cap-one household touch.");
        Assert.That(
            GetHousehold(actual.State, 6).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 6).MigrationRisk + 1),
            "The second selected pool also receives its own cap-one household touch; the household cap is not global.");
        Assert.That(
            GetHousehold(actual.State, 1).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 1).MigrationRisk),
            "The lower-score household in the first selected pool remains no-touch under per-pool cap one.");
        Assert.That(
            GetHousehold(actual.State, 5).MigrationRisk,
            Is.EqualTo(GetHousehold(baseline.State, 5).MigrationRisk),
            "The lower-score household in the second selected pool remains no-touch under per-pool cap one.");
        Assert.That(
            actual.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!))
                .OrderBy(static householdId => householdId),
            Is.EqualTo(new[] { 2, 6 }));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleEmitsThresholdEventOnlyForSelectedCrossingHousehold()
    {
        static void ConfigureThresholdEventFixture(PopulationAndHouseholdsState state)
        {
            GetHousehold(state, 2).MigrationRisk = 78;
        }

        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationMobilityRunResult baseline = RunFirstMobilityRuntimeScenario(
            cappedRules with { MonthlyRuntimeRiskDelta = 0 },
            ConfigureThresholdEventFixture);
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(
            cappedRules,
            ConfigureThresholdEventFixture);

        IDomainEvent thresholdEvent = actual.EventBuffer.Events.Single(evt =>
            evt.EventType == PopulationEventNames.MigrationStarted
            && evt.EntityKey == "2");

        Assert.That(GetHousehold(baseline.State, 2).MigrationRisk, Is.EqualTo(79));
        Assert.That(GetHousehold(actual.State, 2).MigrationRisk, Is.EqualTo(80));
        Assert.That(GetHousehold(actual.State, 2).IsMigrating, Is.True);
        Assert.That(
            actual.EventBuffer.Events
                .Where(static evt => evt.EventType == PopulationEventNames.MigrationStarted)
                .Select(static evt => evt.EntityKey),
            Is.EqualTo(new[] { "2" }));
        Assert.That(
            actual.Diff.Entries
                .Where(static entry => entry.Description.Contains("Household mobility pressure"))
                .Select(static entry => int.Parse(entry.EntityKey!)),
            Is.EqualTo(new[] { 2 }));
        Assert.That(thresholdEvent.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseSocialPressure));
        Assert.That(thresholdEvent.Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("1"));
        Assert.That(thresholdEvent.Metadata[DomainEventMetadataKeys.HouseholdId], Is.EqualTo("2"));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultMigrationStartedEventThresholdPreservesPreviousEventBehavior()
    {
        static void ConfigureThresholdEventFixture(PopulationAndHouseholdsState state)
        {
            GetHousehold(state, 2).MigrationRisk = 78;
        }

        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultThresholdRules =
            cappedRules with
            {
                MonthlyRuntimeMigrationStartedEventThreshold =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStartedEventThreshold,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            cappedRules,
            ConfigureThresholdEventFixture);
        PopulationMobilityRunResult explicitDefaultThresholdResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultThresholdRules,
            ConfigureThresholdEventFixture);

        Assert.That(
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStartedEventThreshold,
            Is.EqualTo(80));
        Assert.That(
            BuildThresholdEventSignature(explicitDefaultThresholdResult),
            Is.EqualTo(BuildThresholdEventSignature(defaultResult)));
        Assert.That(GetHousehold(defaultResult.State, 2).MigrationRisk, Is.EqualTo(80));
        Assert.That(GetHousehold(explicitDefaultThresholdResult.State, 2).MigrationRisk, Is.EqualTo(80));

        static string BuildThresholdEventSignature(PopulationMobilityRunResult result)
        {
            IDomainEvent thresholdEvent = result.EventBuffer.Events.Single(evt =>
                evt.EventType == PopulationEventNames.MigrationStarted
                && evt.EntityKey == "2");

            return string.Join(
                "|",
                thresholdEvent.EventType,
                thresholdEvent.EntityKey,
                thresholdEvent.Metadata[DomainEventMetadataKeys.Cause],
                thresholdEvent.Metadata[DomainEventMetadataKeys.SettlementId],
                thresholdEvent.Metadata[DomainEventMetadataKeys.HouseholdId],
                thresholdEvent.Summary);
        }
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultMigrationStatusThresholdPreservesPreviousStatusBehavior()
    {
        static void ConfigureThresholdStatusFixture(PopulationAndHouseholdsState state)
        {
            GetHousehold(state, 2).MigrationRisk = 78;
        }

        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationHouseholdMobilityRulesData explicitDefaultStatusThresholdRules =
            cappedRules with
            {
                MonthlyRuntimeMigrationStatusThreshold =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStatusThreshold,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            cappedRules,
            ConfigureThresholdStatusFixture);
        PopulationMobilityRunResult explicitDefaultStatusThresholdResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultStatusThresholdRules,
            ConfigureThresholdStatusFixture);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStatusThreshold, Is.EqualTo(80));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultStatusThresholdResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(GetHousehold(defaultResult.State, 2).MigrationRisk, Is.EqualTo(80));
        Assert.That(GetHousehold(defaultResult.State, 2).IsMigrating, Is.True);
        Assert.That(GetHousehold(explicitDefaultStatusThresholdResult.State, 2).IsMigrating, Is.True);
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleDefaultMigrationRiskClampPreservesPreviousClampBehavior()
    {
        static void ConfigureClampFixture(PopulationAndHouseholdsState state)
        {
            GetHousehold(state, 2).MigrationRisk = 78;
        }

        PopulationHouseholdMobilityRulesData clampRules =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeCandidateMigrationRiskCeiling = 100,
                MonthlyRuntimeHouseholdCap = 1,
                MonthlyRuntimeRiskDelta = 8,
            };
        PopulationHouseholdMobilityRulesData explicitDefaultClampRules =
            clampRules with
            {
                MonthlyRuntimeMigrationRiskClampFloor =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampFloor,
                MonthlyRuntimeMigrationRiskClampCeiling =
                    PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampCeiling,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(
            clampRules,
            ConfigureClampFixture);
        PopulationMobilityRunResult explicitDefaultClampResult = RunFirstMobilityRuntimeScenario(
            explicitDefaultClampRules,
            ConfigureClampFixture);

        Assert.That(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampFloor, Is.EqualTo(0));
        Assert.That(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampCeiling, Is.EqualTo(100));
        Assert.That(
            BuildFirstMobilityRuntimeSignature(explicitDefaultClampResult),
            Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
        Assert.That(GetHousehold(defaultResult.State, 2).MigrationRisk, Is.EqualTo(87));
        Assert.That(GetHousehold(explicitDefaultClampResult.State, 2).MigrationRisk, Is.EqualTo(87));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleThresholdEventCarriesMetadataWithoutSummaryParsing()
    {
        static void ConfigureThresholdMetadataFixture(PopulationAndHouseholdsState state)
        {
            GetHousehold(state, 2).MigrationRisk = 78;
        }

        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationMobilityRunResult actual = RunFirstMobilityRuntimeScenario(cappedRules, ConfigureThresholdMetadataFixture);

        IDomainEvent thresholdEvent = actual.EventBuffer.Events.Single(evt =>
            evt.EventType == PopulationEventNames.MigrationStarted
            && evt.EntityKey == "2");

        string metadataTuple = string.Join(
            ":",
            thresholdEvent.Metadata[DomainEventMetadataKeys.Cause],
            thresholdEvent.Metadata[DomainEventMetadataKeys.SettlementId],
            thresholdEvent.Metadata[DomainEventMetadataKeys.HouseholdId]);

        Assert.That(metadataTuple, Is.EqualTo("social-pressure:1:2"));
        Assert.That(thresholdEvent.Summary, Does.Not.Contain(DomainEventMetadataKeys.Cause));
        Assert.That(thresholdEvent.Summary, Does.Not.Contain(DomainEventMetadataKeys.SettlementId));
        Assert.That(thresholdEvent.Summary, Does.Not.Contain(DomainEventMetadataKeys.HouseholdId));
        Assert.That(thresholdEvent.Summary, Does.Not.Contain(DomainEventMetadataValues.CauseSocialPressure));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleThresholdEventMetadataReplayStable()
    {
        static void ConfigureThresholdReplayFixture(PopulationAndHouseholdsState state)
        {
            GetHousehold(state, 2).MigrationRisk = 78;
        }

        PopulationHouseholdMobilityRulesData cappedRules =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = 1 };
        PopulationMobilityRunResult first = RunFirstMobilityRuntimeScenario(cappedRules, ConfigureThresholdReplayFixture);
        PopulationMobilityRunResult second = RunFirstMobilityRuntimeScenario(cappedRules, ConfigureThresholdReplayFixture);

        Assert.That(BuildThresholdEventMetadataSignature(second), Is.EqualTo(BuildThresholdEventMetadataSignature(first)));

        static string BuildThresholdEventMetadataSignature(PopulationMobilityRunResult result)
        {
            IDomainEvent thresholdEvent = result.EventBuffer.Events.Single(evt =>
                evt.EventType == PopulationEventNames.MigrationStarted
                && evt.EntityKey == "2");

            return string.Join(
                "|",
                thresholdEvent.EventType,
                thresholdEvent.EntityKey,
                thresholdEvent.Metadata[DomainEventMetadataKeys.Cause],
                thresholdEvent.Metadata[DomainEventMetadataKeys.SettlementId],
                thresholdEvent.Metadata[DomainEventMetadataKeys.HouseholdId],
                thresholdEvent.Summary);
        }
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCapFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeHouseholdCap = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_household_cap"));
        Assert.That(
            rulesData.GetMonthlyRuntimeHouseholdCapOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeHouseholdCap));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeActivePoolOutflowThreshold = -1,
                MonthlyRuntimeCandidateMigrationRiskFloor = -1,
                MonthlyRuntimeCandidateMigrationRiskCeiling =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeCandidateMigrationRiskCeiling + 1,
                MonthlyRuntimeDistressTriggerThreshold = -1,
                MonthlyRuntimeDebtPressureTriggerThreshold = -1,
                MonthlyRuntimeLaborCapacityTriggerCeiling = -1,
                MonthlyRuntimeGrainStoreTriggerFloor = -1,
                MonthlyRuntimeLandHoldingTriggerFloor = -1,
                MonthlyRuntimeTriggerLivelihoods = new[] { (LivelihoodType)999 },
                MonthlyRuntimeLivelihoodScoreWeights =
                    new[]
                    {
                        new PopulationHouseholdMobilityLivelihoodScoreWeight(
                            (LivelihoodType)999,
                            PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeLivelihoodScoreWeight + 1),
                    },
                MonthlyRuntimeDistressScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimePressureScoreWeight + 1,
                MonthlyRuntimeDebtPressureScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimePressureScoreWeight + 1,
                MonthlyRuntimeMigrationRiskScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationRiskScoreWeight + 1,
                MonthlyRuntimeLaborCapacityPressureFloor = -1,
                MonthlyRuntimeGrainStorePressureFloor = -1,
                MonthlyRuntimeGrainStorePressureDivisor =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeGrainStorePressureDivisor + 1,
                MonthlyRuntimeLandHoldingPressureFloor = -1,
                MonthlyRuntimeLandHoldingPressureDivisor =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeLandHoldingPressureDivisor + 1,
                MonthlyRuntimeSettlementCap = PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeSettlementCap + 1,
                MonthlyRuntimeHouseholdCap = PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeHouseholdCap + 1,
                MonthlyRuntimeRiskDelta = PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeRiskDelta + 1,
                MonthlyRuntimeMigrationRiskClampFloor =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationRiskClampFloor + 1,
                MonthlyRuntimeMigrationRiskClampCeiling = 0,
                MonthlyRuntimeMigrationStatusThreshold =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationStatusThreshold + 1,
                MonthlyRuntimeMigrationStartedEventThreshold =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationStartedEventThreshold + 1,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Count.EqualTo(25));
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_active_pool_outflow_threshold")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_candidate_migration_risk_floor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_candidate_migration_risk_ceiling")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_distress_trigger_threshold")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_debt_pressure_trigger_threshold")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_labor_capacity_trigger_ceiling")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_grain_store_trigger_floor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_land_holding_trigger_floor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_trigger_livelihoods")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_livelihood_score_weights")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_distress_score_weight")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_debt_pressure_score_weight")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_migration_risk_score_weight")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_labor_capacity_pressure_floor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_grain_store_pressure_floor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_grain_store_pressure_divisor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_land_holding_pressure_floor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_land_holding_pressure_divisor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_settlement_cap")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_household_cap")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_risk_delta")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_migration_risk_clamp_floor")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_migration_risk_clamp_ceiling")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_migration_status_threshold")), Is.True);
        Assert.That(
            validation.Errors.Any(static error => error.Contains("monthly_runtime_migration_started_event_threshold")),
            Is.True);
        Assert.That(
            rulesData.GetMonthlyRuntimeActivePoolOutflowThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeActivePoolOutflowThreshold));
        Assert.That(
            rulesData.GetMonthlyRuntimeCandidateMigrationRiskFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeCandidateMigrationRiskCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskCeiling));
        Assert.That(
            rulesData.GetMonthlyRuntimeDistressTriggerThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressTriggerThreshold));
        Assert.That(
            rulesData.GetMonthlyRuntimeDebtPressureTriggerThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureTriggerThreshold));
        Assert.That(
            rulesData.GetMonthlyRuntimeLaborCapacityTriggerCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityTriggerCeiling));
        Assert.That(
            rulesData.GetMonthlyRuntimeGrainStoreTriggerFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStoreTriggerFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeLandHoldingTriggerFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingTriggerFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeTriggerLivelihoodsOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeTriggerLivelihoods));
        Assert.That(
            rulesData.GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLivelihoodScoreWeights));
        Assert.That(
            rulesData.GetMonthlyRuntimeDistressScoreWeightOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressScoreWeight));
        Assert.That(
            rulesData.GetMonthlyRuntimeDebtPressureScoreWeightOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureScoreWeight));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationRiskScoreWeightOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskScoreWeight));
        Assert.That(
            rulesData.GetMonthlyRuntimeLaborCapacityPressureFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityPressureFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeGrainStorePressureFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeGrainStorePressureDivisorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureDivisor));
        Assert.That(
            rulesData.GetMonthlyRuntimeLandHoldingPressureFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeLandHoldingPressureDivisorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureDivisor));
        Assert.That(
            rulesData.GetMonthlyRuntimeSettlementCapOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeSettlementCap));
        Assert.That(
            rulesData.GetMonthlyRuntimeHouseholdCapOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeHouseholdCap));
        Assert.That(
            rulesData.GetMonthlyRuntimeRiskDeltaOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeRiskDelta));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationRiskClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationRiskClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampCeiling));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationStatusThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStatusThreshold));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationStartedEventThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStartedEventThreshold));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskFloorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeCandidateMigrationRiskFloor = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_candidate_migration_risk_floor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeCandidateMigrationRiskFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskFloor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskCeilingFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeCandidateMigrationRiskCeiling = 0 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_candidate_migration_risk_ceiling"));
        Assert.That(
            rulesData.GetMonthlyRuntimeCandidateMigrationRiskCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeCandidateMigrationRiskCeiling));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDistressTriggerThresholdFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeDistressTriggerThreshold = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_distress_trigger_threshold"));
        Assert.That(
            rulesData.GetMonthlyRuntimeDistressTriggerThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressTriggerThreshold));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDebtPressureTriggerThresholdFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeDebtPressureTriggerThreshold = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_debt_pressure_trigger_threshold"));
        Assert.That(
            rulesData.GetMonthlyRuntimeDebtPressureTriggerThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureTriggerThreshold));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLaborCapacityTriggerCeilingFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeLaborCapacityTriggerCeiling = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_labor_capacity_trigger_ceiling"));
        Assert.That(
            rulesData.GetMonthlyRuntimeLaborCapacityTriggerCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityTriggerCeiling));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStoreTriggerFloorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeGrainStoreTriggerFloor = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_grain_store_trigger_floor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeGrainStoreTriggerFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStoreTriggerFloor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLandHoldingTriggerFloorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeLandHoldingTriggerFloor = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_land_holding_trigger_floor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeLandHoldingTriggerFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingTriggerFloor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeTriggerLivelihoodsFallBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeTriggerLivelihoods = new[] { (LivelihoodType)999 },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_trigger_livelihoods"));
        Assert.That(
            rulesData.GetMonthlyRuntimeTriggerLivelihoodsOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeTriggerLivelihoods));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLivelihoodScoreWeightsFallBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeLivelihoodScoreWeights =
                    new[]
                    {
                        new PopulationHouseholdMobilityLivelihoodScoreWeight(
                            (LivelihoodType)999,
                            PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeLivelihoodScoreWeight + 1),
                    },
            };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_livelihood_score_weights"));
        Assert.That(
            rulesData.GetMonthlyRuntimeLivelihoodScoreWeightsOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLivelihoodScoreWeights));
        Assert.That(
            rulesData.GetMonthlyRuntimeLivelihoodScoreWeightOrDefault(LivelihoodType.SeasonalMigrant),
            Is.EqualTo(18));
        Assert.That(
            rulesData.GetMonthlyRuntimeLivelihoodScoreWeightOrDefault(LivelihoodType.HiredLabor),
            Is.EqualTo(10));
        Assert.That(
            rulesData.GetMonthlyRuntimeLivelihoodScoreWeightOrDefault(LivelihoodType.Tenant),
            Is.EqualTo(6));
        Assert.That(
            rulesData.GetMonthlyRuntimeLivelihoodScoreWeightOrDefault(LivelihoodType.Smallholder),
            Is.EqualTo(0));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimePressureScoreWeightsFallBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeDistressScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimePressureScoreWeight + 1,
                MonthlyRuntimeDebtPressureScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimePressureScoreWeight + 1,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors, Has.Count.EqualTo(2));
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_distress_score_weight")), Is.True);
        Assert.That(validation.Errors.Any(static error => error.Contains("monthly_runtime_debt_pressure_score_weight")), Is.True);
        Assert.That(
            rulesData.GetMonthlyRuntimeDistressScoreWeightOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressScoreWeight));
        Assert.That(
            rulesData.GetMonthlyRuntimeDebtPressureScoreWeightOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureScoreWeight));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationRiskScoreWeightFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeMigrationRiskScoreWeight = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_migration_risk_score_weight"));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationRiskScoreWeightOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskScoreWeight));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLaborCapacityPressureFloorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeLaborCapacityPressureFloor = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_labor_capacity_pressure_floor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeLaborCapacityPressureFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityPressureFloor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStorePressureFloorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeGrainStorePressureFloor = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_grain_store_pressure_floor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeGrainStorePressureFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureFloor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStorePressureDivisorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeGrainStorePressureDivisor = 0 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_grain_store_pressure_divisor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeGrainStorePressureDivisorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureDivisor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLandHoldingPressureFloorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeLandHoldingPressureFloor = -1 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_land_holding_pressure_floor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeLandHoldingPressureFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureFloor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLandHoldingPressureDivisorFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeLandHoldingPressureDivisor = 0 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_land_holding_pressure_divisor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeLandHoldingPressureDivisorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureDivisor));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationRiskClampFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeMigrationRiskClampFloor = 90,
                MonthlyRuntimeMigrationRiskClampCeiling = 10,
            };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_migration_risk_clamp_floor"));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationRiskClampFloorOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampFloor));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationRiskClampCeilingOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskClampCeiling));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationStartedEventThresholdFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeMigrationStartedEventThreshold = 0 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_migration_started_event_threshold"));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationStartedEventThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStartedEventThreshold));
    }

    [Test]
    public void PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationStatusThresholdFallsBackToDefault()
    {
        PopulationHouseholdMobilityRulesData rulesData =
            PopulationHouseholdMobilityRulesData.Default with { MonthlyRuntimeMigrationStatusThreshold = 0 };

        PopulationHouseholdMobilityRulesValidationResult validation = rulesData.Validate();

        Assert.That(validation.IsValid, Is.False);
        Assert.That(validation.Errors.Single(), Does.Contain("monthly_runtime_migration_status_threshold"));
        Assert.That(
            rulesData.GetMonthlyRuntimeMigrationStatusThresholdOrDefault(),
            Is.EqualTo(PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStatusThreshold));
    }

    [Test]
    public void RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome()
    {
        PopulationHouseholdMobilityRulesData malformedRulesData =
            PopulationHouseholdMobilityRulesData.Default with
            {
                MonthlyRuntimeActivePoolOutflowThreshold = -1,
                MonthlyRuntimeCandidateMigrationRiskFloor = -1,
                MonthlyRuntimeCandidateMigrationRiskCeiling =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeCandidateMigrationRiskCeiling + 1,
                MonthlyRuntimeDistressTriggerThreshold = -1,
                MonthlyRuntimeDebtPressureTriggerThreshold = -1,
                MonthlyRuntimeLaborCapacityTriggerCeiling = -1,
                MonthlyRuntimeGrainStoreTriggerFloor = -1,
                MonthlyRuntimeLandHoldingTriggerFloor = -1,
                MonthlyRuntimeTriggerLivelihoods = new[] { (LivelihoodType)999 },
                MonthlyRuntimeLivelihoodScoreWeights =
                    new[]
                    {
                        new PopulationHouseholdMobilityLivelihoodScoreWeight(
                            (LivelihoodType)999,
                            PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeLivelihoodScoreWeight + 1),
                    },
                MonthlyRuntimeDistressScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimePressureScoreWeight + 1,
                MonthlyRuntimeDebtPressureScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimePressureScoreWeight + 1,
                MonthlyRuntimeMigrationRiskScoreWeight =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationRiskScoreWeight + 1,
                MonthlyRuntimeLaborCapacityPressureFloor = -1,
                MonthlyRuntimeGrainStorePressureFloor = -1,
                MonthlyRuntimeGrainStorePressureDivisor =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeGrainStorePressureDivisor + 1,
                MonthlyRuntimeLandHoldingPressureFloor = -1,
                MonthlyRuntimeLandHoldingPressureDivisor =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeLandHoldingPressureDivisor + 1,
                MonthlyRuntimeSettlementCap = PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeSettlementCap + 1,
                MonthlyRuntimeHouseholdCap = PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeHouseholdCap + 1,
                MonthlyRuntimeRiskDelta = PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeRiskDelta + 1,
                MonthlyRuntimeMigrationRiskClampFloor =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationRiskClampFloor + 1,
                MonthlyRuntimeMigrationRiskClampCeiling = 0,
                MonthlyRuntimeMigrationStatusThreshold =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationStatusThreshold + 1,
                MonthlyRuntimeMigrationStartedEventThreshold =
                    PopulationHouseholdMobilityRulesData.MaxMonthlyRuntimeMigrationStartedEventThreshold + 1,
            };

        PopulationMobilityRunResult defaultResult = RunFirstMobilityRuntimeScenario(PopulationHouseholdMobilityRulesData.Default);
        PopulationMobilityRunResult malformedResult = RunFirstMobilityRuntimeScenario(malformedRulesData);

        Assert.That(BuildFirstMobilityRuntimeSignature(malformedResult), Is.EqualTo(BuildFirstMobilityRuntimeSignature(defaultResult)));
    }

    [Test]
    public void RunMonth_StableHiredLaborCanDriftBackToSmallholder()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 66,
            Prosperity = 64,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();

        PersonRegistryModule registryModule = new();
        PersonRegistryState registryState = registryModule.CreateInitialState();
        registryState.Persons.Add(new PersonRecord
        {
            Id = new PersonId(9201),
            DisplayName = "Labor Li",
            BirthDate = new GameDate(1180, 1),
            Gender = PersonGender.Male,
            LifeStage = LifeStage.Adult,
            IsAlive = true,
            FidelityRing = FidelityRing.Local,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Labor Li",
            SettlementId = new SettlementId(1),
            Livelihood = LivelihoodType.HiredLabor,
            Distress = 20,
            DebtPressure = 20,
            LaborCapacity = 82,
            MigrationRisk = 12,
            LandHolding = 45,
            GrainStore = 70,
            DependentCount = 1,
            LaborerCount = 2,
        });
        populationState.Memberships.Add(new HouseholdMembershipState
        {
            PersonId = new PersonId(9201),
            HouseholdId = new HouseholdId(1),
            Livelihood = LivelihoodType.HiredLabor,
            HealthResilience = 70,
            Health = HealthStatus.Healthy,
            Activity = PersonActivity.Laboring,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        registryModule.RegisterQueries(registryState, queries);
        populationModule.RegisterQueries(populationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(139)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));

        Assert.That(populationState.Households.Single().Livelihood, Is.EqualTo(LivelihoodType.Smallholder));
        Assert.That(populationState.Memberships.Single().Livelihood, Is.EqualTo(LivelihoodType.Smallholder));
        Assert.That(populationState.Memberships.Single().Activity, Is.EqualTo(PersonActivity.Farming));
        Assert.That(populationState.LaborPools.Single().AvailableLabor, Is.GreaterThan(0));
        Assert.That(populationState.MigrationPools.Single().OutflowPressure, Is.LessThan(40));
    }

    private static PopulationHouseholdState RunSingleHouseholdMonth(LivelihoodType livelihood)
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 50,
            Prosperity = 50,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Test House",
            SettlementId = new SettlementId(1),
            Livelihood = livelihood,
            Distress = 50,
            DebtPressure = 40,
            LaborCapacity = 50,
            MigrationRisk = 20,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(17)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));
        return populationState.Households[0];
    }

    private static PopulationMobilityRunResult RunFirstMobilityRuntimeScenario(
        PopulationHouseholdMobilityRulesData rulesData,
        Action<PopulationAndHouseholdsState>? configureState = null)
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 50,
            Prosperity = 50,
            BaselineInstitutionCount = 1,
        });
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(2),
            Name = "Yanguan",
            Security = 50,
            Prosperity = 50,
            BaselineInstitutionCount = 1,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();

        PopulationAndHouseholdsModule populationModule = new(rulesData);
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Households.AddRange(
        [
            new PopulationHouseholdState
            {
                Id = new HouseholdId(1),
                HouseholdName = "Tenant Li",
                SettlementId = new SettlementId(1),
                Livelihood = LivelihoodType.Tenant,
                Distress = 61,
                DebtPressure = 64,
                LaborCapacity = 42,
                MigrationRisk = 77,
                LandHolding = 12,
                GrainStore = 20,
                DependentCount = 2,
                LaborerCount = 1,
            },
            new PopulationHouseholdState
            {
                Id = new HouseholdId(2),
                HouseholdName = "Labor Wu",
                SettlementId = new SettlementId(1),
                Livelihood = LivelihoodType.HiredLabor,
                Distress = 60,
                DebtPressure = 70,
                LaborCapacity = 44,
                MigrationRisk = 76,
                LandHolding = 8,
                GrainStore = 22,
                DependentCount = 2,
                LaborerCount = 1,
            },
            new PopulationHouseholdState
            {
                Id = new HouseholdId(3),
                HouseholdName = "Migrant Sun",
                SettlementId = new SettlementId(1),
                Livelihood = LivelihoodType.SeasonalMigrant,
                Distress = 60,
                DebtPressure = 60,
                LaborCapacity = 48,
                MigrationRisk = 65,
                LandHolding = 18,
                GrainStore = 28,
                DependentCount = 1,
                LaborerCount = 1,
            },
            new PopulationHouseholdState
            {
                Id = new HouseholdId(4),
                HouseholdName = "Quiet Zheng",
                SettlementId = new SettlementId(1),
                Livelihood = LivelihoodType.Smallholder,
                Distress = 30,
                DebtPressure = 20,
                LaborCapacity = 70,
                MigrationRisk = 40,
                LandHolding = 35,
                GrainStore = 70,
                DependentCount = 1,
                LaborerCount = 2,
            },
            new PopulationHouseholdState
            {
                Id = new HouseholdId(5),
                HouseholdName = "Tenant He",
                SettlementId = new SettlementId(2),
                Livelihood = LivelihoodType.Tenant,
                Distress = 60,
                DebtPressure = 62,
                LaborCapacity = 44,
                MigrationRisk = 62,
                LandHolding = 11,
                GrainStore = 25,
                DependentCount = 2,
                LaborerCount = 1,
            },
            new PopulationHouseholdState
            {
                Id = new HouseholdId(6),
                HouseholdName = "Labor Qian",
                SettlementId = new SettlementId(2),
                Livelihood = LivelihoodType.HiredLabor,
                Distress = 60,
                DebtPressure = 62,
                LaborCapacity = 44,
                MigrationRisk = 62,
                LandHolding = 11,
                GrainStore = 25,
                DependentCount = 2,
                LaborerCount = 1,
            },
        ]);

        configureState?.Invoke(populationState);

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        familyModule.RegisterQueries(familyState, queries);
        populationModule.RegisterQueries(populationState, queries);

        DomainEventBuffer eventBuffer = new();
        WorldDiff diff = new();
        ModuleExecutionContext context = new(
            new GameDate(1200, 8),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(211)),
            queries,
            eventBuffer,
            diff);

        populationModule.RunMonth(new ModuleExecutionScope<PopulationAndHouseholdsState>(populationState, context));
        return new PopulationMobilityRunResult(populationState, eventBuffer, diff);
    }

    private static PopulationHouseholdState GetHousehold(PopulationAndHouseholdsState state, int householdId)
    {
        return state.Households.Single(household => household.Id == new HouseholdId(householdId));
    }

    private static MigrationPoolEntryState GetMigrationPool(PopulationAndHouseholdsState state, int settlementId)
    {
        return state.MigrationPools.Single(pool => pool.SettlementId == new SettlementId(settlementId));
    }

    private static string BuildFirstMobilityRuntimeSignature(PopulationMobilityRunResult result)
    {
        string households = string.Join(
            "|",
            result.State.Households
                .OrderBy(static household => household.Id.Value)
                .Select(static household =>
                    $"{household.Id.Value}:{household.SettlementId.Value}:{household.MigrationRisk}:{household.IsMigrating}:{household.Livelihood}"));
        string pools = string.Join(
            "|",
            result.State.MigrationPools
                .OrderBy(static pool => pool.SettlementId.Value)
                .Select(static pool =>
                    $"{pool.SettlementId.Value}:{pool.OutflowPressure}:{pool.InflowPressure}:{pool.FloatingPopulation}"));
        string events = string.Join(
            "|",
            result.EventBuffer.Events.Select(static evt =>
                $"{evt.ModuleKey}:{evt.EventType}:{evt.EntityKey}"));

        return $"{households}::{pools}::{events}";
    }

    private static int ComputeFirstMobilityRuntimeScoreForTest(PopulationHouseholdState household)
    {
        int laborPressure = Math.Max(
            0,
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLaborCapacityPressureFloor - household.LaborCapacity);
        int grainPressure = Math.Max(
            0,
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureFloor - household.GrainStore)
            / PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeGrainStorePressureDivisor;
        int landPressure = Math.Max(
            0,
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureFloor - household.LandHolding)
            / PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeLandHoldingPressureDivisor;
        int livelihoodPressure = PopulationHouseholdMobilityRulesData.Default
            .GetMonthlyRuntimeLivelihoodScoreWeightOrDefault(household.Livelihood);

        return (household.MigrationRisk * PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationRiskScoreWeight)
            + (household.Distress * PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDistressScoreWeight)
            + (household.DebtPressure * PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeDebtPressureScoreWeight)
            + laborPressure
            + grainPressure
            + landPressure
            + livelihoodPressure;
    }

    private sealed record PopulationMobilityRunResult(
        PopulationAndHouseholdsState State,
        DomainEventBuffer EventBuffer,
        WorldDiff Diff);

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }
    }
}
