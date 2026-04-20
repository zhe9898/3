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
public sealed partial class ConflictAndForceModuleTests
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

        Assert.That(snapshot.LastConflictTrace, Does.Contain("守备之势"));

        Assert.That(snapshot.LastConflictTrace, Does.Contain("已先被按住"));

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


}
