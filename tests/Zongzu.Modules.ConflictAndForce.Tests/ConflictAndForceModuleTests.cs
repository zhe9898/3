using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.ConflictAndForce.Tests;

[TestFixture]
public sealed class ConflictAndForceModuleTests
{
    [Test]
    public void RunMonth_BuildsForcePostureAndResolvesTraceableLocalConflict()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(5),
            Name = "Lanxi",
            Security = 42,
            Prosperity = 59,
            BaselineInstitutionCount = 1,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(5),
            CommonerDistress = 67,
            LaborSupply = 95,
            MigrationPressure = 52,
            MilitiaPotential = 84,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(5),
            Prestige = 58,
            SupportReserve = 62,
            HeirPersonId = new PersonId(1),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(2),
            PublicNarrative = "Lanxi is sitting on a knife-edge.",
            GrudgePressure = 63,
            FearPressure = 59,
            ShamePressure = 24,
            FavorBalance = 2,
        });

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Clans.Add(new ClanTradeState
        {
            ClanId = new ClanId(2),
            PrimarySettlementId = new SettlementId(5),
            CashReserve = 76,
            GrainReserve = 63,
            Debt = 36,
            CommerceReputation = 24,
            ShopCount = 1,
            ManagerSkill = 3,
            LastOutcome = "Stable",
            LastExplanation = "Lanxi is still trading under escort pressure.",
        });
        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(2),
            RouteName = "Lanxi River Wharf",
            SettlementId = new SettlementId(5),
            IsActive = true,
            Capacity = 32,
            Risk = 72,
            LastMargin = -3,
        });

        OrderAndBanditryModule orderModule = new();
        OrderAndBanditryState orderState = orderModule.CreateInitialState();
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(5),
            BanditThreat = 81,
            RoutePressure = 72,
            SuppressionDemand = 68,
            DisorderPressure = 76,
            LastPressureReason = "Road raids and coercion are already widespread.",
        });

        ConflictAndForceModule module = new();
        ConflictAndForceState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(5),
            GuardCount = 4,
            RetainerCount = 2,
            MilitiaCount = 8,
            EscortCount = 1,
            Readiness = 18,
            CommandCapacity = 14,
            LastConflictTrace = "Forces are thin and tired.",
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);
        orderModule.RegisterQueries(orderState, queries);
        module.RegisterQueries(state, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            CreateEnabledManifest(),
            new DeterministicRandom(KernelState.Create(99)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<ConflictAndForceState>(state, context));

        IConflictAndForceQueries forceQueries = queries.GetRequired<IConflictAndForceQueries>();
        LocalForcePoolSnapshot snapshot = forceQueries.GetRequiredSettlementForce(new SettlementId(5));

        Assert.That(snapshot.GuardCount, Is.GreaterThan(4));
        Assert.That(snapshot.MilitiaCount, Is.GreaterThanOrEqualTo(20));
        Assert.That(snapshot.EscortCount, Is.GreaterThan(1));
        Assert.That(snapshot.Readiness, Is.GreaterThan(18));
        Assert.That(snapshot.CommandCapacity, Is.GreaterThan(14));
        Assert.That(snapshot.ResponseActivationLevel, Is.GreaterThanOrEqualTo(ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel));
        Assert.That(snapshot.OrderSupportLevel, Is.GreaterThan(0));
        Assert.That(snapshot.IsResponseActivated, Is.True);
        Assert.That(snapshot.HasActiveConflict, Is.True);
        Assert.That(snapshot.LastConflictTrace, Does.Contain("Force posture"));
        Assert.That(snapshot.LastConflictTrace, Does.Contain("contained"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("MilitiaMobilized"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("ForceReadinessChanged"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("ConflictResolved"));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.ConflictAndForce));
        Assert.That(module.AcceptedCommands, Does.Contain("PrepareEscort"));
        Assert.That(module.PublishedEvents, Does.Contain("CommanderWounded"));
    }

    [Test]
    public void RegisterQueries_CalmPostureReportsNoActivatedSupport()
    {
        ConflictAndForceModule module = new();
        ConflictAndForceState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(9),
            GuardCount = 6,
            RetainerCount = 2,
            MilitiaCount = 4,
            EscortCount = 1,
            Readiness = 24,
            CommandCapacity = 18,
            LastConflictTrace = "Routine watch rotation in Lanxi.",
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        IConflictAndForceQueries forceQueries = queries.GetRequired<IConflictAndForceQueries>();
        LocalForcePoolSnapshot snapshot = forceQueries.GetRequiredSettlementForce(new SettlementId(9));

        Assert.That(snapshot.ResponseActivationLevel, Is.LessThan(ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel));
        Assert.That(snapshot.OrderSupportLevel, Is.EqualTo(0));
        Assert.That(snapshot.IsResponseActivated, Is.False);
        Assert.That(snapshot.HasActiveConflict, Is.False);
    }

    [Test]
    public void RunMonth_HighStandingForceWithoutActiveConflict_DoesNotActivateOrderSupport()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(6),
            Name = "Qingshui",
            Security = 68,
            Prosperity = 63,
            BaselineInstitutionCount = 1,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(6),
            CommonerDistress = 29,
            LaborSupply = 118,
            MigrationPressure = 18,
            MilitiaPotential = 82,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(4),
            ClanName = "Li",
            HomeSettlementId = new SettlementId(6),
            Prestige = 61,
            SupportReserve = 70,
            HeirPersonId = new PersonId(4),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(4),
            PublicNarrative = "Qingshui is guarded but calm.",
            GrudgePressure = 21,
            FearPressure = 23,
            ShamePressure = 10,
            FavorBalance = 8,
        });

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Clans.Add(new ClanTradeState
        {
            ClanId = new ClanId(4),
            PrimarySettlementId = new SettlementId(6),
            CashReserve = 82,
            GrainReserve = 71,
            Debt = 14,
            CommerceReputation = 36,
            ShopCount = 1,
            ManagerSkill = 4,
            LastOutcome = "Stable",
            LastExplanation = "The canal convoy is moving on schedule.",
        });
        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(4),
            RouteName = "Qingshui Canal Convoy",
            SettlementId = new SettlementId(6),
            IsActive = true,
            Capacity = 24,
            Risk = 14,
            LastMargin = 6,
        });

        OrderAndBanditryModule orderModule = new();
        OrderAndBanditryState orderState = orderModule.CreateInitialState();
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(6),
            BanditThreat = 12,
            RoutePressure = 10,
            SuppressionDemand = 16,
            DisorderPressure = 14,
            LastPressureReason = "Routine patrols are enough this month.",
        });

        ConflictAndForceModule module = new();
        ConflictAndForceState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(6),
            GuardCount = 20,
            RetainerCount = 9,
            MilitiaCount = 22,
            EscortCount = 8,
            Readiness = 54,
            CommandCapacity = 41,
            LastConflictTrace = "Routine guard drill and convoy inspection in Qingshui.",
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);
        orderModule.RegisterQueries(orderState, queries);
        module.RegisterQueries(state, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            CreateEnabledManifest(),
            new DeterministicRandom(KernelState.Create(199)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<ConflictAndForceState>(state, context));

        IConflictAndForceQueries forceQueries = queries.GetRequired<IConflictAndForceQueries>();
        LocalForcePoolSnapshot snapshot = forceQueries.GetRequiredSettlementForce(new SettlementId(6));

        Assert.That(snapshot.ResponseActivationLevel, Is.GreaterThanOrEqualTo(ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel));
        Assert.That(snapshot.HasActiveConflict, Is.False);
        Assert.That(snapshot.IsResponseActivated, Is.False);
        Assert.That(snapshot.OrderSupportLevel, Is.EqualTo(0));
    }

    [Test]
    public void RunMonth_UsesOfficeLeverageToIncreaseAdministrativeSupport()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(5),
            Name = "Lanxi",
            Security = 42,
            Prosperity = 59,
            BaselineInstitutionCount = 1,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(5),
            CommonerDistress = 67,
            LaborSupply = 95,
            MigrationPressure = 52,
            MilitiaPotential = 84,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(5),
            Prestige = 58,
            SupportReserve = 62,
            HeirPersonId = new PersonId(1),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(2),
            PublicNarrative = "Lanxi is sitting on a knife-edge.",
            GrudgePressure = 63,
            FearPressure = 59,
            ShamePressure = 24,
            FavorBalance = 2,
        });

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Clans.Add(new ClanTradeState
        {
            ClanId = new ClanId(2),
            PrimarySettlementId = new SettlementId(5),
            CashReserve = 76,
            GrainReserve = 63,
            Debt = 36,
            CommerceReputation = 24,
            ShopCount = 1,
            ManagerSkill = 3,
            LastOutcome = "Stable",
            LastExplanation = "Lanxi is still trading under escort pressure.",
        });
        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(2),
            RouteName = "Lanxi River Wharf",
            SettlementId = new SettlementId(5),
            IsActive = true,
            Capacity = 32,
            Risk = 72,
            LastMargin = -3,
        });

        OrderAndBanditryModule orderModule = new();
        OrderAndBanditryState orderState = orderModule.CreateInitialState();
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(5),
            BanditThreat = 81,
            RoutePressure = 72,
            SuppressionDemand = 68,
            DisorderPressure = 76,
            LastPressureReason = "Road raids and coercion are already widespread.",
        });

        ConflictAndForceModule baselineModule = new();
        ConflictAndForceState baselineState = baselineModule.CreateInitialState();
        baselineState.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(5),
            GuardCount = 4,
            RetainerCount = 2,
            MilitiaCount = 8,
            EscortCount = 1,
            Readiness = 18,
            CommandCapacity = 14,
            LastConflictTrace = "Forces are thin and tired.",
        });

        QueryRegistry baselineQueries = new();
        worldModule.RegisterQueries(worldState, baselineQueries);
        populationModule.RegisterQueries(populationState, baselineQueries);
        familyModule.RegisterQueries(familyState, baselineQueries);
        socialModule.RegisterQueries(socialState, baselineQueries);
        tradeModule.RegisterQueries(tradeState, baselineQueries);
        orderModule.RegisterQueries(orderState, baselineQueries);
        baselineModule.RegisterQueries(baselineState, baselineQueries);

        baselineModule.RunMonth(new ModuleExecutionScope<ConflictAndForceState>(
            baselineState,
            new ModuleExecutionContext(
                new GameDate(1200, 7),
                CreateEnabledManifest(),
                new DeterministicRandom(KernelState.Create(109)),
                baselineQueries,
                new DomainEventBuffer(),
                new WorldDiff())));

        OfficeAndCareerModule officeModule = new();
        OfficeAndCareerState officeState = officeModule.CreateInitialState();
        officeState.Jurisdictions.Add(new JurisdictionAuthorityState
        {
            SettlementId = new SettlementId(5),
            LeadOfficialPersonId = new PersonId(1),
            LeadOfficialName = "Zhang Yuan",
            LeadOfficeTitle = "Assistant Magistrate",
            AuthorityTier = 3,
            JurisdictionLeverage = 68,
            PetitionPressure = 22,
            LastAdministrativeTrace = "The assistant magistrate is coordinating retainers and petitions.",
        });

        ConflictAndForceModule module = new();
        ConflictAndForceState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(5),
            GuardCount = 4,
            RetainerCount = 2,
            MilitiaCount = 8,
            EscortCount = 1,
            Readiness = 18,
            CommandCapacity = 14,
            LastConflictTrace = "Forces are thin and tired.",
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);
        orderModule.RegisterQueries(orderState, queries);
        officeModule.RegisterQueries(officeState, queries);
        module.RegisterQueries(state, queries);

        module.RunMonth(new ModuleExecutionScope<ConflictAndForceState>(
            state,
            new ModuleExecutionContext(
                new GameDate(1200, 7),
                CreateGovernanceEnabledManifest(),
                new DeterministicRandom(KernelState.Create(109)),
                queries,
                new DomainEventBuffer(),
                new WorldDiff())));

        LocalForcePoolSnapshot baselineSnapshot = baselineQueries.GetRequired<IConflictAndForceQueries>().GetRequiredSettlementForce(new SettlementId(5));
        LocalForcePoolSnapshot snapshot = queries.GetRequired<IConflictAndForceQueries>().GetRequiredSettlementForce(new SettlementId(5));

        Assert.That(snapshot.Readiness, Is.GreaterThan(baselineSnapshot.Readiness));
        Assert.That(snapshot.CommandCapacity, Is.GreaterThan(baselineSnapshot.CommandCapacity));
        Assert.That(snapshot.LastConflictTrace, Does.Contain("Assistant Magistrate leverage"));
    }

    [Test]
    public void RunMonth_PersistentCampaignFatigue_DragsForcePostureWhileRecovering()
    {
        WorldSettlementsModule worldModule = new();
        WorldSettlementsState worldState = worldModule.CreateInitialState();
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(7),
            Name = "Hengshan",
            Security = 51,
            Prosperity = 58,
            BaselineInstitutionCount = 1,
        });

        PopulationAndHouseholdsModule populationModule = new();
        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(7),
            CommonerDistress = 41,
            LaborSupply = 102,
            MigrationPressure = 22,
            MilitiaPotential = 88,
        });

        FamilyCoreModule familyModule = new();
        FamilyCoreState familyState = familyModule.CreateInitialState();
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(7),
            ClanName = "Shen",
            HomeSettlementId = new SettlementId(7),
            Prestige = 55,
            SupportReserve = 64,
            HeirPersonId = new PersonId(7),
        });

        SocialMemoryAndRelationsModule socialModule = new();
        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(7),
            PublicNarrative = "Hengshan is tense but holding.",
            GrudgePressure = 39,
            FearPressure = 31,
            ShamePressure = 18,
            FavorBalance = 5,
        });

        TradeAndIndustryModule tradeModule = new();
        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();
        tradeState.Clans.Add(new ClanTradeState
        {
            ClanId = new ClanId(7),
            PrimarySettlementId = new SettlementId(7),
            CashReserve = 82,
            GrainReserve = 70,
            Debt = 18,
            CommerceReputation = 34,
            ShopCount = 1,
            ManagerSkill = 4,
            LastOutcome = "Stable",
            LastExplanation = "Convoys are moving without major interruption.",
        });
        tradeState.Routes.Add(new RouteTradeState
        {
            RouteId = 1,
            ClanId = new ClanId(7),
            RouteName = "Hengshan Pass Escort",
            SettlementId = new SettlementId(7),
            IsActive = true,
            Capacity = 28,
            Risk = 26,
            LastMargin = 4,
        });

        OrderAndBanditryModule orderModule = new();
        OrderAndBanditryState orderState = orderModule.CreateInitialState();
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(7),
            BanditThreat = 33,
            RoutePressure = 28,
            SuppressionDemand = 24,
            DisorderPressure = 30,
            LastPressureReason = "Routine patrols are holding the roads.",
        });

        ConflictAndForceModule baselineModule = new();
        ConflictAndForceState baselineState = baselineModule.CreateInitialState();
        baselineState.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(7),
            GuardCount = 16,
            RetainerCount = 6,
            MilitiaCount = 24,
            EscortCount = 7,
            Readiness = 48,
            CommandCapacity = 38,
            LastConflictTrace = "The watch is settled into routine patrols.",
        });

        QueryRegistry baselineQueries = new();
        worldModule.RegisterQueries(worldState, baselineQueries);
        populationModule.RegisterQueries(populationState, baselineQueries);
        familyModule.RegisterQueries(familyState, baselineQueries);
        socialModule.RegisterQueries(socialState, baselineQueries);
        tradeModule.RegisterQueries(tradeState, baselineQueries);
        orderModule.RegisterQueries(orderState, baselineQueries);
        baselineModule.RegisterQueries(baselineState, baselineQueries);

        baselineModule.RunMonth(new ModuleExecutionScope<ConflictAndForceState>(
            baselineState,
            new ModuleExecutionContext(
                new GameDate(1200, 8),
                CreateEnabledManifest(),
                new DeterministicRandom(KernelState.Create(333)),
                baselineQueries,
                new DomainEventBuffer(),
                new WorldDiff())));

        ConflictAndForceModule fatiguedModule = new();
        ConflictAndForceState fatiguedState = fatiguedModule.CreateInitialState();
        fatiguedState.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(7),
            GuardCount = 16,
            RetainerCount = 6,
            MilitiaCount = 24,
            EscortCount = 7,
            Readiness = 48,
            CommandCapacity = 38,
            CampaignFatigue = 24,
            CampaignEscortStrain = 18,
            LastCampaignFalloutTrace = "Campaign fallout from Hengshan left escorts frayed and watches short of sleep.",
            LastConflictTrace = "The watch is settled into routine patrols.",
        });

        QueryRegistry fatiguedQueries = new();
        worldModule.RegisterQueries(worldState, fatiguedQueries);
        populationModule.RegisterQueries(populationState, fatiguedQueries);
        familyModule.RegisterQueries(familyState, fatiguedQueries);
        socialModule.RegisterQueries(socialState, fatiguedQueries);
        tradeModule.RegisterQueries(tradeState, fatiguedQueries);
        orderModule.RegisterQueries(orderState, fatiguedQueries);
        fatiguedModule.RegisterQueries(fatiguedState, fatiguedQueries);

        fatiguedModule.RunMonth(new ModuleExecutionScope<ConflictAndForceState>(
            fatiguedState,
            new ModuleExecutionContext(
                new GameDate(1200, 8),
                CreateEnabledManifest(),
                new DeterministicRandom(KernelState.Create(333)),
                fatiguedQueries,
                new DomainEventBuffer(),
                new WorldDiff())));

        LocalForcePoolSnapshot baselineSnapshot = baselineQueries.GetRequired<IConflictAndForceQueries>().GetRequiredSettlementForce(new SettlementId(7));
        LocalForcePoolSnapshot fatiguedSnapshot = fatiguedQueries.GetRequired<IConflictAndForceQueries>().GetRequiredSettlementForce(new SettlementId(7));

        Assert.That(fatiguedSnapshot.CampaignFatigue, Is.LessThan(24));
        Assert.That(fatiguedSnapshot.CampaignEscortStrain, Is.LessThan(18));
        Assert.That(fatiguedSnapshot.Readiness, Is.LessThan(baselineSnapshot.Readiness));
        Assert.That(fatiguedSnapshot.CommandCapacity, Is.LessThan(baselineSnapshot.CommandCapacity));
        Assert.That(fatiguedSnapshot.EscortCount, Is.LessThan(baselineSnapshot.EscortCount));
        Assert.That(fatiguedSnapshot.LastConflictTrace, Does.Contain("Earlier campaigning still leaves fatigue"));
    }

    [Test]
    public void HandleEvents_AppliesCampaignFalloutInsideConflictOwnedState()
    {
        ConflictAndForceModule module = new();
        ConflictAndForceState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(8),
            GuardCount = 18,
            RetainerCount = 7,
            MilitiaCount = 26,
            EscortCount = 9,
            Readiness = 56,
            CommandCapacity = 44,
            ResponseActivationLevel = 5,
            OrderSupportLevel = 7,
            IsResponseActivated = true,
            HasActiveConflict = true,
            LastConflictTrace = "The watch is pressing hard but holding the roads.",
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(8),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi Campaign Board",
                IsActive = true,
                MobilizedForceCount = 52,
                FrontPressure = 76,
                FrontLabel = "Front under strain",
                SupplyState = 34,
                SupplyStateLabel = "Supply strained",
                MoraleState = 43,
                MoraleStateLabel = "Morale unsettled",
                CommandFitLabel = "Orders stretching thin",
                ActiveDirectiveCode = WarfareCampaignCommandNames.CommitMobilization,
                ActiveDirectiveLabel = "Commit mobilization",
                ActiveDirectiveSummary = "Push men and grain forward.",
                LastDirectiveTrace = "The board is pushing harder into the front.",
                MobilizationWindowLabel = "Open",
                ObjectiveSummary = "Hold the pass and keep the route open.",
                SupplyLineSummary = "Escort lines are burning effort to hold the grain route.",
                OfficeCoordinationTrace = "County clerks are relaying transport orders.",
                LastAftermathSummary = "Exhausted escorts and militia are filtering back into town.",
            },
        ]));

        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            CreateCampaignEnabledManifest(),
            new DeterministicRandom(KernelState.Create(808)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<ConflictAndForceState>(
            state,
            context,
            [
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignPressureRaised, "Lanxi front pressure rose.", "8"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply lines strained.", "8"),
                new DomainEventRecord(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi entered aftermath review.", "8"),
            ]));

        LocalForcePoolSnapshot snapshot = queries.GetRequired<IConflictAndForceQueries>().GetRequiredSettlementForce(new SettlementId(8));

        Assert.That(snapshot.CampaignFatigue, Is.GreaterThan(0));
        Assert.That(snapshot.CampaignEscortStrain, Is.GreaterThan(0));
        Assert.That(snapshot.Readiness, Is.LessThan(56));
        Assert.That(snapshot.CommandCapacity, Is.LessThan(44));
        Assert.That(snapshot.EscortCount, Is.LessThan(9));
        Assert.That(snapshot.LastCampaignFalloutTrace, Does.Contain("Campaign fallout from Lanxi"));
        Assert.That(snapshot.LastConflictTrace, Does.Contain("Campaign fallout from Lanxi"));
        Assert.That(context.Diff.Entries.Single().Description, Does.Contain("fatigue"));
        Assert.That(context.DomainEvents.Events.Any(static entry => entry.EventType == "ForceReadinessChanged"), Is.True);
    }

    private static FeatureManifest CreateEnabledManifest()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        return manifest;
    }

    private static FeatureManifest CreateCampaignEnabledManifest()
    {
        FeatureManifest manifest = CreateGovernanceEnabledManifest();
        manifest.Set(KnownModuleKeys.WarfareCampaign, FeatureMode.Lite);
        return manifest;
    }

    private static FeatureManifest CreateGovernanceEnabledManifest()
    {
        FeatureManifest manifest = CreateEnabledManifest();
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        return manifest;
    }

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
