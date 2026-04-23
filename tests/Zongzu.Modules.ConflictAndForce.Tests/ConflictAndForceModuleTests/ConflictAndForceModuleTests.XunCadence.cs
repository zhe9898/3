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

public sealed partial class ConflictAndForceModuleTests
{
    [Test]
    public void RunXun_ShangAndZhongxunRaiseEscortPostureIntoActivatedResponseWithoutReadableOutput()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(11),

            Name = "Lanxi",

            Security = 43,

            Prosperity = 58,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(11),

            CommonerDistress = 62,

            LaborSupply = 94,

            MigrationPressure = 41,

            MilitiaPotential = 52,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(2),

            ClanName = "Zhang",

            HomeSettlementId = new SettlementId(11),

            Prestige = 58,

            SupportReserve = 65,

            HeirPersonId = new PersonId(1),

        });


        SocialMemoryAndRelationsModule socialModule = new();

        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        socialState.ClanNarratives.Add(new ClanNarrativeState

        {

            ClanId = new ClanId(2),

            PublicNarrative = "Lanxi is tightening around the road line.",

            GrudgePressure = 58,

            FearPressure = 54,

            ShamePressure = 22,

            FavorBalance = 3,

        });


        TradeAndIndustryModule tradeModule = new();

        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();

        tradeState.Clans.Add(new ClanTradeState

        {

            ClanId = new ClanId(2),

            PrimarySettlementId = new SettlementId(11),

            CashReserve = 76,

            GrainReserve = 63,

            Debt = 24,

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

            SettlementId = new SettlementId(11),

            IsActive = true,

            Capacity = 28,

            Risk = 72,

            LastMargin = -2,

        });


        OrderAndBanditryModule orderModule = new();

        OrderAndBanditryState orderState = orderModule.CreateInitialState();

        orderState.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(11),

            BanditThreat = 60,

            RoutePressure = 58,

            SuppressionDemand = 56,

            DisorderPressure = 52,

            LastPressureReason = "Road raids are forcing a stronger response.",

        });


        ConflictAndForceModule module = new();

        ConflictAndForceState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementForceState

        {

            SettlementId = new SettlementId(11),

            GuardCount = 16,

            RetainerCount = 4,

            MilitiaCount = 10,

            EscortCount = 3,

            Readiness = 29,

            CommandCapacity = 34,

            LastConflictTrace = "Watch posture is still thin.",

        });


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        orderModule.RegisterQueries(orderState, queries);

        module.RegisterQueries(state, queries);


        ModuleExecutionContext shangxunContext = new(

            new GameDate(1200, 7),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(701)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff(),

            cadenceBand: SimulationCadenceBand.Xun,

            currentXun: SimulationXun.Shangxun);


        module.RunXun(new ModuleExecutionScope<ConflictAndForceState>(state, shangxunContext));


        LocalForcePoolSnapshot shangxunSnapshot = queries.GetRequired<IConflictAndForceQueries>()

            .GetRequiredSettlementForce(new SettlementId(11));


        Assert.That(shangxunSnapshot.EscortCount, Is.GreaterThan(3));

        Assert.That(shangxunSnapshot.Readiness, Is.GreaterThan(29));

        Assert.That(shangxunSnapshot.IsResponseActivated, Is.False);

        Assert.That(shangxunContext.Diff.Entries, Is.Empty);

        Assert.That(shangxunContext.DomainEvents.Events, Is.Empty);


        ModuleExecutionContext zhongxunContext = new(

            new GameDate(1200, 7),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(702)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff(),

            cadenceBand: SimulationCadenceBand.Xun,

            currentXun: SimulationXun.Zhongxun);


        module.RunXun(new ModuleExecutionScope<ConflictAndForceState>(state, zhongxunContext));


        LocalForcePoolSnapshot zhongxunSnapshot = queries.GetRequired<IConflictAndForceQueries>()

            .GetRequiredSettlementForce(new SettlementId(11));


        Assert.That(zhongxunSnapshot.CommandCapacity, Is.GreaterThan(34));

        Assert.That(zhongxunSnapshot.ResponseActivationLevel, Is.GreaterThanOrEqualTo(ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel));

        Assert.That(zhongxunSnapshot.HasActiveConflict, Is.True);

        Assert.That(zhongxunSnapshot.IsResponseActivated, Is.True);

        Assert.That(zhongxunSnapshot.OrderSupportLevel, Is.GreaterThan(0));

        Assert.That(zhongxunSnapshot.LastConflictTrace, Is.Not.Empty);

        Assert.That(zhongxunContext.Diff.Entries, Is.Empty);

        Assert.That(zhongxunContext.DomainEvents.Events, Is.Empty);

    }


    [Test]

    public void RunXun_XiaxunKeepsCalmSurfaceFromActivatingOrderSupport()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(12),

            Name = "Qingshui",

            Security = 68,

            Prosperity = 63,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(12),

            CommonerDistress = 28,

            LaborSupply = 116,

            MigrationPressure = 18,

            MilitiaPotential = 44,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(4),

            ClanName = "Li",

            HomeSettlementId = new SettlementId(12),

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

            PrimarySettlementId = new SettlementId(12),

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

            SettlementId = new SettlementId(12),

            IsActive = true,

            Capacity = 24,

            Risk = 14,

            LastMargin = 6,

        });


        OrderAndBanditryModule orderModule = new();

        OrderAndBanditryState orderState = orderModule.CreateInitialState();

        orderState.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(12),

            BanditThreat = 12,

            RoutePressure = 10,

            SuppressionDemand = 16,

            DisorderPressure = 14,

            LastPressureReason = "The county road is calm this month.",

        });


        ConflictAndForceModule module = new();

        ConflictAndForceState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementForceState

        {

            SettlementId = new SettlementId(12),

            GuardCount = 14,

            RetainerCount = 5,

            MilitiaCount = 8,

            EscortCount = 4,

            Readiness = 30,

            CommandCapacity = 34,

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


        ModuleExecutionContext xiaxunContext = new(

            new GameDate(1200, 7),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(703)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff(),

            cadenceBand: SimulationCadenceBand.Xun,

            currentXun: SimulationXun.Xiaxun);


        module.RunXun(new ModuleExecutionScope<ConflictAndForceState>(state, xiaxunContext));


        LocalForcePoolSnapshot snapshot = queries.GetRequired<IConflictAndForceQueries>()

            .GetRequiredSettlementForce(new SettlementId(12));


        Assert.That(snapshot.EscortCount, Is.LessThanOrEqualTo(4));

        Assert.That(snapshot.ResponseActivationLevel, Is.LessThan(ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel));

        Assert.That(snapshot.HasActiveConflict, Is.False);

        Assert.That(snapshot.IsResponseActivated, Is.False);

        Assert.That(snapshot.OrderSupportLevel, Is.EqualTo(0));

        Assert.That(snapshot.LastConflictTrace, Is.Not.Empty);

        Assert.That(xiaxunContext.Diff.Entries, Is.Empty);

        Assert.That(xiaxunContext.DomainEvents.Events, Is.Empty);

    }


}
