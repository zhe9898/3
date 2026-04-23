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

namespace Zongzu.Modules.OrderAndBanditry.Tests;

public sealed partial class OrderAndBanditryModuleTests
{
    public void RunMonth_UsesConflictForcePostureToSlowEscalation()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(2),

            Name = "Lanxi",

            Security = 39,

            Prosperity = 58,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(2),

            CommonerDistress = 64,

            LaborSupply = 92,

            MigrationPressure = 56,

            MilitiaPotential = 70,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(1),

            ClanName = "Zhang",

            HomeSettlementId = new SettlementId(2),

            Prestige = 47,

            SupportReserve = 48,

            HeirPersonId = new PersonId(1),

        });


        SocialMemoryAndRelationsModule socialModule = new();

        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        socialState.ClanNarratives.Add(new ClanNarrativeState

        {

            ClanId = new ClanId(1),

            PublicNarrative = "兰溪乡面压力渐聚。",

            GrudgePressure = 58,

            FearPressure = 57,

            ShamePressure = 26,

            FavorBalance = 4,

        });


        TradeAndIndustryModule tradeModule = new();

        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();

        tradeState.Clans.Add(new ClanTradeState

        {

            ClanId = new ClanId(1),

            PrimarySettlementId = new SettlementId(2),

            CashReserve = 80,

            GrainReserve = 60,

            Debt = 18,

            CommerceReputation = 31,

            ShopCount = 1,

            ManagerSkill = 3,

            LastOutcome = "Stable",

            LastExplanation = "路面之压渐起。",

        });

        tradeState.Routes.Add(new RouteTradeState

        {

            RouteId = 1,

            ClanId = new ClanId(1),

            RouteName = "Lanxi River Wharf",

            SettlementId = new SettlementId(2),

            IsActive = true,

            Capacity = 30,

            Risk = 70,

            LastMargin = 4,

        });


        ConflictAndForceModule conflictModule = new();

        ConflictAndForceState conflictState = conflictModule.CreateInitialState();

        conflictState.Settlements.Add(new SettlementForceState

        {

            SettlementId = new SettlementId(2),

            GuardCount = 18,

            RetainerCount = 7,

            MilitiaCount = 24,

            EscortCount = 10,

            Readiness = 52,

            CommandCapacity = 38,

            ResponseActivationLevel = 9,

            OrderSupportLevel = 12,

            IsResponseActivated = true,

            HasActiveConflict = true,

            LastConflictTrace = "Lanxi is holding a strong watch posture around the wharf.",

        });


        OrderAndBanditryModule module = new();

        OrderAndBanditryState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        OrderAndBanditryModule baselineModule = new();

        OrderAndBanditryState baselineState = baselineModule.CreateInitialState();

        baselineState.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        QueryRegistry baselineQueries = new();

        worldModule.RegisterQueries(worldState, baselineQueries);

        populationModule.RegisterQueries(populationState, baselineQueries);

        familyModule.RegisterQueries(familyState, baselineQueries);

        socialModule.RegisterQueries(socialState, baselineQueries);

        tradeModule.RegisterQueries(tradeState, baselineQueries);

        baselineModule.RegisterQueries(baselineState, baselineQueries);


        ModuleExecutionContext baselineContext = new(

            new GameDate(1200, 6),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(77)),

            baselineQueries,

            new DomainEventBuffer(),

            new WorldDiff());


        baselineModule.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(baselineState, baselineContext));


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        conflictModule.RegisterQueries(conflictState, queries);

        module.RegisterQueries(state, queries);


        ModuleExecutionContext context = new(

            new GameDate(1200, 6),

            CreateConflictEnabledManifest(),

            new DeterministicRandom(KernelState.Create(77)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff());


        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));


        IOrderAndBanditryQueries baselineDisorderQueries = baselineQueries.GetRequired<IOrderAndBanditryQueries>();

        IOrderAndBanditryQueries disorderQueries = queries.GetRequired<IOrderAndBanditryQueries>();

        SettlementDisorderSnapshot baselineSnapshot = baselineDisorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementDisorderSnapshot snapshot = disorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementBlackRoutePressureSnapshot pressureSnapshot = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(snapshot.SuppressionDemand, Is.LessThan(baselineSnapshot.SuppressionDemand));

        Assert.That(snapshot.RoutePressure, Is.LessThanOrEqualTo(baselineSnapshot.RoutePressure));

        Assert.That(pressureSnapshot.RouteShielding, Is.GreaterThan(0));

        Assert.That(pressureSnapshot.RetaliationRisk, Is.GreaterThanOrEqualTo(0));

        Assert.That(snapshot.LastPressureReason, Does.Contain("已激活的守丁"));

        Assert.That(snapshot.LastPressureReason, Does.Contain("整备"));

    }


    [Test]

    public void RunMonth_DoesNotUseCalmForcePostureBeforeResponseActivation()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(2),

            Name = "Lanxi",

            Security = 39,

            Prosperity = 58,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(2),

            CommonerDistress = 64,

            LaborSupply = 92,

            MigrationPressure = 56,

            MilitiaPotential = 70,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(1),

            ClanName = "Zhang",

            HomeSettlementId = new SettlementId(2),

            Prestige = 47,

            SupportReserve = 48,

            HeirPersonId = new PersonId(1),

        });


        SocialMemoryAndRelationsModule socialModule = new();

        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        socialState.ClanNarratives.Add(new ClanNarrativeState

        {

            ClanId = new ClanId(1),

            PublicNarrative = "兰溪乡面压力渐聚。",

            GrudgePressure = 58,

            FearPressure = 57,

            ShamePressure = 26,

            FavorBalance = 4,

        });


        TradeAndIndustryModule tradeModule = new();

        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();

        tradeState.Clans.Add(new ClanTradeState

        {

            ClanId = new ClanId(1),

            PrimarySettlementId = new SettlementId(2),

            CashReserve = 80,

            GrainReserve = 60,

            Debt = 18,

            CommerceReputation = 31,

            ShopCount = 1,

            ManagerSkill = 3,

            LastOutcome = "Stable",

            LastExplanation = "路面之压渐起。",

        });

        tradeState.Routes.Add(new RouteTradeState

        {

            RouteId = 1,

            ClanId = new ClanId(1),

            RouteName = "Lanxi River Wharf",

            SettlementId = new SettlementId(2),

            IsActive = true,

            Capacity = 30,

            Risk = 70,

            LastMargin = 4,

        });


        ConflictAndForceModule conflictModule = new();

        ConflictAndForceState conflictState = conflictModule.CreateInitialState();

        conflictState.Settlements.Add(new SettlementForceState

        {

            SettlementId = new SettlementId(2),

            GuardCount = 6,

            RetainerCount = 2,

            MilitiaCount = 4,

            EscortCount = 1,

            Readiness = 24,

            CommandCapacity = 18,

            ResponseActivationLevel = 2,

            OrderSupportLevel = 0,

            IsResponseActivated = false,

            HasActiveConflict = false,

            LastConflictTrace = "Routine watch rotation in Lanxi.",

        });


        OrderAndBanditryModule module = new();

        OrderAndBanditryState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        OrderAndBanditryModule baselineModule = new();

        OrderAndBanditryState baselineState = baselineModule.CreateInitialState();

        baselineState.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        QueryRegistry baselineQueries = new();

        worldModule.RegisterQueries(worldState, baselineQueries);

        populationModule.RegisterQueries(populationState, baselineQueries);

        familyModule.RegisterQueries(familyState, baselineQueries);

        socialModule.RegisterQueries(socialState, baselineQueries);

        tradeModule.RegisterQueries(tradeState, baselineQueries);

        baselineModule.RegisterQueries(baselineState, baselineQueries);


        ModuleExecutionContext baselineContext = new(

            new GameDate(1200, 6),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(77)),

            baselineQueries,

            new DomainEventBuffer(),

            new WorldDiff());


        baselineModule.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(baselineState, baselineContext));


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        conflictModule.RegisterQueries(conflictState, queries);

        module.RegisterQueries(state, queries);


        ModuleExecutionContext context = new(

            new GameDate(1200, 6),

            CreateConflictEnabledManifest(),

            new DeterministicRandom(KernelState.Create(77)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff());


        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));


        IOrderAndBanditryQueries baselineDisorderQueries = baselineQueries.GetRequired<IOrderAndBanditryQueries>();

        IOrderAndBanditryQueries disorderQueries = queries.GetRequired<IOrderAndBanditryQueries>();

        SettlementDisorderSnapshot baselineSnapshot = baselineDisorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementDisorderSnapshot snapshot = disorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementBlackRoutePressureSnapshot pressureSnapshot = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(snapshot.SuppressionDemand, Is.EqualTo(baselineSnapshot.SuppressionDemand));

        Assert.That(pressureSnapshot.RouteShielding, Is.EqualTo(0));

        Assert.That(pressureSnapshot.RetaliationRisk, Is.EqualTo(0));

        Assert.That(snapshot.LastPressureReason, Does.Not.Contain("support"));

        Assert.That(snapshot.LastPressureReason, Does.Not.Contain("Guards 6"));

    }


    [Test]

    public void RunMonth_DoesNotUseStandingForceWithoutActiveConflict()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(2),

            Name = "Lanxi",

            Security = 39,

            Prosperity = 58,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(2),

            CommonerDistress = 64,

            LaborSupply = 92,

            MigrationPressure = 56,

            MilitiaPotential = 70,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(1),

            ClanName = "Zhang",

            HomeSettlementId = new SettlementId(2),

            Prestige = 47,

            SupportReserve = 48,

            HeirPersonId = new PersonId(1),

        });


        SocialMemoryAndRelationsModule socialModule = new();

        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        socialState.ClanNarratives.Add(new ClanNarrativeState

        {

            ClanId = new ClanId(1),

            PublicNarrative = "兰溪乡面压力渐聚。",

            GrudgePressure = 58,

            FearPressure = 57,

            ShamePressure = 26,

            FavorBalance = 4,

        });


        TradeAndIndustryModule tradeModule = new();

        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();

        tradeState.Clans.Add(new ClanTradeState

        {

            ClanId = new ClanId(1),

            PrimarySettlementId = new SettlementId(2),

            CashReserve = 80,

            GrainReserve = 60,

            Debt = 18,

            CommerceReputation = 31,

            ShopCount = 1,

            ManagerSkill = 3,

            LastOutcome = "Stable",

            LastExplanation = "路面之压渐起。",

        });

        tradeState.Routes.Add(new RouteTradeState

        {

            RouteId = 1,

            ClanId = new ClanId(1),

            RouteName = "Lanxi River Wharf",

            SettlementId = new SettlementId(2),

            IsActive = true,

            Capacity = 30,

            Risk = 70,

            LastMargin = 4,

        });


        ConflictAndForceModule conflictModule = new();

        ConflictAndForceState conflictState = conflictModule.CreateInitialState();

        conflictState.Settlements.Add(new SettlementForceState

        {

            SettlementId = new SettlementId(2),

            GuardCount = 20,

            RetainerCount = 9,

            MilitiaCount = 22,

            EscortCount = 8,

            Readiness = 54,

            CommandCapacity = 41,

            ResponseActivationLevel = 9,

            OrderSupportLevel = 0,

            IsResponseActivated = false,

            HasActiveConflict = false,

            LastConflictTrace = "Routine guard drill and convoy inspection in Lanxi.",

        });


        OrderAndBanditryModule module = new();

        OrderAndBanditryState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        OrderAndBanditryModule baselineModule = new();

        OrderAndBanditryState baselineState = baselineModule.CreateInitialState();

        baselineState.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        QueryRegistry baselineQueries = new();

        worldModule.RegisterQueries(worldState, baselineQueries);

        populationModule.RegisterQueries(populationState, baselineQueries);

        familyModule.RegisterQueries(familyState, baselineQueries);

        socialModule.RegisterQueries(socialState, baselineQueries);

        tradeModule.RegisterQueries(tradeState, baselineQueries);

        baselineModule.RegisterQueries(baselineState, baselineQueries);


        ModuleExecutionContext baselineContext = new(

            new GameDate(1200, 6),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(77)),

            baselineQueries,

            new DomainEventBuffer(),

            new WorldDiff());


        baselineModule.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(baselineState, baselineContext));


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        conflictModule.RegisterQueries(conflictState, queries);

        module.RegisterQueries(state, queries);


        ModuleExecutionContext context = new(

            new GameDate(1200, 6),

            CreateConflictEnabledManifest(),

            new DeterministicRandom(KernelState.Create(77)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff());


        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));


        IOrderAndBanditryQueries baselineDisorderQueries = baselineQueries.GetRequired<IOrderAndBanditryQueries>();

        IOrderAndBanditryQueries disorderQueries = queries.GetRequired<IOrderAndBanditryQueries>();

        SettlementDisorderSnapshot baselineSnapshot = baselineDisorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementDisorderSnapshot snapshot = disorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementBlackRoutePressureSnapshot pressureSnapshot = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(snapshot.RoutePressure, Is.EqualTo(baselineSnapshot.RoutePressure));

        Assert.That(snapshot.DisorderPressure, Is.EqualTo(baselineSnapshot.DisorderPressure));

        Assert.That(snapshot.SuppressionDemand, Is.EqualTo(baselineSnapshot.SuppressionDemand));

        Assert.That(pressureSnapshot.RouteShielding, Is.EqualTo(0));

        Assert.That(pressureSnapshot.RetaliationRisk, Is.EqualTo(0));

        Assert.That(snapshot.LastPressureReason, Does.Not.Contain("已激活的守丁"));

        Assert.That(snapshot.LastPressureReason, Does.Not.Contain("support"));

    }


    [Test]

    public void RunMonth_UsesOfficeLeverageAsBoundedAdministrativeRelief()

    {

        WorldSettlementsModule worldModule = new();

        WorldSettlementsState worldState = worldModule.CreateInitialState();

        worldState.Settlements.Add(new SettlementStateData

        {

            Id = new SettlementId(2),

            Name = "Lanxi",

            Security = 41,

            Prosperity = 58,

            BaselineInstitutionCount = 1,

        });


        PopulationAndHouseholdsModule populationModule = new();

        PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();

        populationState.Settlements.Add(new PopulationSettlementState

        {

            SettlementId = new SettlementId(2),

            CommonerDistress = 64,

            LaborSupply = 92,

            MigrationPressure = 56,

            MilitiaPotential = 52,

        });


        FamilyCoreModule familyModule = new();

        FamilyCoreState familyState = familyModule.CreateInitialState();

        familyState.Clans.Add(new ClanStateData

        {

            Id = new ClanId(1),

            ClanName = "Zhang",

            HomeSettlementId = new SettlementId(2),

            Prestige = 47,

            SupportReserve = 48,

            HeirPersonId = new PersonId(1),

        });


        SocialMemoryAndRelationsModule socialModule = new();

        SocialMemoryAndRelationsState socialState = socialModule.CreateInitialState();

        socialState.ClanNarratives.Add(new ClanNarrativeState

        {

            ClanId = new ClanId(1),

            PublicNarrative = "兰溪乡面压力渐聚。",

            GrudgePressure = 58,

            FearPressure = 57,

            ShamePressure = 26,

            FavorBalance = 4,

        });


        TradeAndIndustryModule tradeModule = new();

        TradeAndIndustryState tradeState = tradeModule.CreateInitialState();

        tradeState.Clans.Add(new ClanTradeState

        {

            ClanId = new ClanId(1),

            PrimarySettlementId = new SettlementId(2),

            CashReserve = 80,

            GrainReserve = 60,

            Debt = 18,

            CommerceReputation = 31,

            ShopCount = 1,

            ManagerSkill = 3,

            LastOutcome = "Stable",

            LastExplanation = "路面之压渐起。",

        });

        tradeState.Routes.Add(new RouteTradeState

        {

            RouteId = 1,

            ClanId = new ClanId(1),

            RouteName = "Lanxi River Wharf",

            SettlementId = new SettlementId(2),

            IsActive = true,

            Capacity = 30,

            Risk = 70,

            LastMargin = 4,

        });


        OrderAndBanditryModule baselineModule = new();

        OrderAndBanditryState baselineState = baselineModule.CreateInitialState();

        baselineState.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        QueryRegistry baselineQueries = new();

        worldModule.RegisterQueries(worldState, baselineQueries);

        populationModule.RegisterQueries(populationState, baselineQueries);

        familyModule.RegisterQueries(familyState, baselineQueries);

        socialModule.RegisterQueries(socialState, baselineQueries);

        tradeModule.RegisterQueries(tradeState, baselineQueries);

        baselineModule.RegisterQueries(baselineState, baselineQueries);


        baselineModule.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(

            baselineState,

            new ModuleExecutionContext(

                new GameDate(1200, 6),

                CreateEnabledManifest(),

                new DeterministicRandom(KernelState.Create(91)),

                baselineQueries,

                new DomainEventBuffer(),

                new WorldDiff())));


        OfficeAndCareerModule officeModule = new();

        OfficeAndCareerState officeState = officeModule.CreateInitialState();

        officeState.Jurisdictions.Add(new JurisdictionAuthorityState

        {

            SettlementId = new SettlementId(2),

            LeadOfficialPersonId = new PersonId(1),

            LeadOfficialName = "Zhang Yuan",

            LeadOfficeTitle = "主簿",

            AuthorityTier = 2,

            JurisdictionLeverage = 62,

            ClerkDependence = 18,

            PetitionPressure = 18,

            PetitionBacklog = 6,

            CurrentAdministrativeTask = "district review",

            AdministrativeTaskLoad = 18,

            LastAdministrativeTrace = "主簿正在调度护运与词状。",

        });


        OrderAndBanditryModule module = new();

        OrderAndBanditryState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 48,

            DisorderPressure = 68,

            LastPressureReason = "Pressure was already building around the wharf.",

        });


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        officeModule.RegisterQueries(officeState, queries);

        module.RegisterQueries(state, queries);


        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(

            state,

            new ModuleExecutionContext(

                new GameDate(1200, 6),

                CreateGovernanceEnabledManifest(),

                new DeterministicRandom(KernelState.Create(91)),

                queries,

                new DomainEventBuffer(),

                new WorldDiff())));


        SettlementDisorderSnapshot baselineSnapshot = baselineQueries.GetRequired<IOrderAndBanditryQueries>().GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementDisorderSnapshot snapshot = queries.GetRequired<IOrderAndBanditryQueries>().GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementBlackRoutePressureSnapshot pressureSnapshot = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(snapshot.SuppressionDemand, Is.LessThan(baselineSnapshot.SuppressionDemand));

        Assert.That(snapshot.DisorderPressure, Is.LessThanOrEqualTo(baselineSnapshot.DisorderPressure));

        Assert.That(pressureSnapshot.PaperCompliance, Is.GreaterThan(0));

        Assert.That(pressureSnapshot.AdministrativeSuppressionWindow, Is.GreaterThan(0));

        Assert.That(snapshot.LastPressureReason, Does.Contain("主簿乡面杠力"));

    }


}
