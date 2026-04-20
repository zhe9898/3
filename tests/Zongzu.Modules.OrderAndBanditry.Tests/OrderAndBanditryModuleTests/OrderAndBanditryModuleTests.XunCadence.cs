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
    public void RunXun_ShangxunAndZhongxunAdvanceOrderPressureWithoutReadableOutput()

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

            CommonerDistress = 68,

            LaborSupply = 92,

            MigrationPressure = 56,

            MilitiaPotential = 44,

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

            PublicNarrative = "Lanxi road pressure is thickening.",

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

            LastExplanation = "Wharf traffic is still moving.",

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

        tradeState.BlackRouteLedgers.Add(new SettlementBlackRouteLedgerState

        {

            SettlementId = new SettlementId(2),

            ShadowPriceIndex = 122,

            DiversionShare = 42,

            IllicitMargin = 16,

            BlockedShipmentCount = 2,

            SeizureRisk = 24,

            DiversionBandLabel = "parallel",

            LastLedgerTrace = "Lanxi already has shadow cargo peeling off from the main wharf.",

        });


        OrderAndBanditryModule module = new();

        OrderAndBanditryState state = module.CreateInitialState();

        state.Settlements.Add(new SettlementDisorderState

        {

            SettlementId = new SettlementId(2),

            BanditThreat = 55,

            RoutePressure = 55,

            SuppressionDemand = 44,

            DisorderPressure = 48,

            LastPressureReason = "Road pressure is already tightening.",

            BlackRoutePressure = 52,

            CoercionRisk = 18,

        });


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        module.RegisterQueries(state, queries);


        ModuleExecutionContext shangxunContext = new(

            new GameDate(1200, 6),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(177)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff(),

            cadenceBand: SimulationCadenceBand.Xun,

            currentXun: SimulationXun.Shangxun);


        module.RunXun(new ModuleExecutionScope<OrderAndBanditryState>(state, shangxunContext));


        SettlementDisorderSnapshot shangxunSnapshot = queries.GetRequired<IOrderAndBanditryQueries>()

            .GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementBlackRoutePressureSnapshot shangxunPressure = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(shangxunSnapshot.BanditThreat, Is.GreaterThan(55));

        Assert.That(shangxunSnapshot.DisorderPressure, Is.GreaterThan(48));

        Assert.That(shangxunPressure.CoercionRisk, Is.GreaterThan(18));

        Assert.That(shangxunContext.Diff.Entries, Is.Empty);

        Assert.That(shangxunContext.DomainEvents.Events, Is.Empty);


        ModuleExecutionContext zhongxunContext = new(

            new GameDate(1200, 6),

            CreateEnabledManifest(),

            new DeterministicRandom(KernelState.Create(178)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff(),

            cadenceBand: SimulationCadenceBand.Xun,

            currentXun: SimulationXun.Zhongxun);


        module.RunXun(new ModuleExecutionScope<OrderAndBanditryState>(state, zhongxunContext));


        SettlementDisorderSnapshot zhongxunSnapshot = queries.GetRequired<IOrderAndBanditryQueries>()

            .GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementBlackRoutePressureSnapshot zhongxunPressure = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(zhongxunSnapshot.RoutePressure, Is.GreaterThan(55));

        Assert.That(zhongxunPressure.BlackRoutePressure, Is.GreaterThan(52));

        Assert.That(zhongxunSnapshot.SuppressionDemand, Is.GreaterThan(44));

        Assert.That(zhongxunPressure.LastPressureTrace, Is.Not.Empty);

        Assert.That(zhongxunContext.Diff.Entries, Is.Empty);

        Assert.That(zhongxunContext.DomainEvents.Events, Is.Empty);

    }


    [Test]

    public void RunXun_XiaxunRefreshesOfficeAndForceMirrorsWithoutReadableOutput()

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

            CommonerDistress = 62,

            LaborSupply = 90,

            MigrationPressure = 54,

            MilitiaPotential = 72,

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

            PublicNarrative = "Lanxi pressure is gathering around the county road.",

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

            LastExplanation = "Wharf traffic is still moving.",

        });

        tradeState.Routes.Add(new RouteTradeState

        {

            RouteId = 1,

            ClanId = new ClanId(1),

            RouteName = "Lanxi River Wharf",

            SettlementId = new SettlementId(2),

            IsActive = true,

            Capacity = 30,

            Risk = 58,

            LastMargin = 4,

        });

        tradeState.BlackRouteLedgers.Add(new SettlementBlackRouteLedgerState

        {

            SettlementId = new SettlementId(2),

            ShadowPriceIndex = 118,

            DiversionShare = 30,

            IllicitMargin = 12,

            BlockedShipmentCount = 1,

            SeizureRisk = 28,

            DiversionBandLabel = "parallel",

            LastLedgerTrace = "Some shadow cargo already bends around the county road.",

        });


        OfficeAndCareerModule officeModule = new();

        OfficeAndCareerState officeState = officeModule.CreateInitialState();

        officeState.Jurisdictions.Add(new JurisdictionAuthorityState

        {

            SettlementId = new SettlementId(2),

            LeadOfficialPersonId = new PersonId(1),

            LeadOfficialName = "Zhang Yuan",

            LeadOfficeTitle = "Registrar",

            AuthorityTier = 2,

            JurisdictionLeverage = 62,

            ClerkDependence = 18,

            PetitionPressure = 18,

            PetitionBacklog = 6,

            CurrentAdministrativeTask = "district review",

            AdministrativeTaskLoad = 18,

            LastPetitionOutcome = "County paperwork is moving.",

            LastAdministrativeTrace = "Lanxi yamen still has room to press the road line.",

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

            SuppressionDemand = 44,

            DisorderPressure = 48,

            LastPressureReason = "Road pressure is already tightening.",

            BlackRoutePressure = 52,

            CoercionRisk = 18,

        });


        QueryRegistry queries = new();

        worldModule.RegisterQueries(worldState, queries);

        populationModule.RegisterQueries(populationState, queries);

        familyModule.RegisterQueries(familyState, queries);

        socialModule.RegisterQueries(socialState, queries);

        tradeModule.RegisterQueries(tradeState, queries);

        officeModule.RegisterQueries(officeState, queries);

        conflictModule.RegisterQueries(conflictState, queries);

        module.RegisterQueries(state, queries);


        ModuleExecutionContext xiaxunContext = new(

            new GameDate(1200, 6),

            CreateGovernanceConflictEnabledManifest(),

            new DeterministicRandom(KernelState.Create(179)),

            queries,

            new DomainEventBuffer(),

            new WorldDiff(),

            cadenceBand: SimulationCadenceBand.Xun,

            currentXun: SimulationXun.Xiaxun);


        module.RunXun(new ModuleExecutionScope<OrderAndBanditryState>(state, xiaxunContext));


        SettlementDisorderSnapshot disorder = queries.GetRequired<IOrderAndBanditryQueries>()

            .GetRequiredSettlementDisorder(new SettlementId(2));

        SettlementBlackRoutePressureSnapshot pressure = queries.GetRequired<IBlackRoutePressureQueries>()

            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));


        Assert.That(pressure.ResponseActivationLevel, Is.EqualTo(9));

        Assert.That(pressure.PaperCompliance, Is.GreaterThan(0));

        Assert.That(pressure.ImplementationDrag, Is.GreaterThanOrEqualTo(0));

        Assert.That(pressure.RouteShielding, Is.GreaterThan(0));

        Assert.That(pressure.AdministrativeSuppressionWindow, Is.GreaterThan(0));

        Assert.That(pressure.SuppressionRelief, Is.GreaterThan(0));

        Assert.That(pressure.RetaliationRisk, Is.GreaterThanOrEqualTo(0));

        Assert.That(pressure.LastPressureTrace, Is.Not.Empty);

        Assert.That(disorder.SuppressionDemand, Is.GreaterThanOrEqualTo(0));

        Assert.That(xiaxunContext.Diff.Entries, Is.Empty);

        Assert.That(xiaxunContext.DomainEvents.Events, Is.Empty);

    }


}
