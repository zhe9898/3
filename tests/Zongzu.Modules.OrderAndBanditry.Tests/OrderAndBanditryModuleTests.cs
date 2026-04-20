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

[TestFixture]
public sealed class OrderAndBanditryModuleTests
{
    [Test]
    public void RunMonth_RaisesDisorderAndEmitsTraceableEvents()
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

        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            BanditThreat = 55,
            RoutePressure = 55,
            SuppressionDemand = 13,
            DisorderPressure = 68,
            LastPressureReason = "Pressure was already building around the wharf.",
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);
        module.RegisterQueries(state, queries);
        IOrderAndBanditryQueries disorderQueries = queries.GetRequired<IOrderAndBanditryQueries>();

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            CreateEnabledManifest(),
            new DeterministicRandom(KernelState.Create(77)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));

        SettlementDisorderSnapshot snapshot = disorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));
        Assert.That(snapshot.BanditThreat, Is.GreaterThanOrEqualTo(60));
        Assert.That(snapshot.RoutePressure, Is.GreaterThanOrEqualTo(60));
        Assert.That(snapshot.DisorderPressure, Is.GreaterThanOrEqualTo(70));
        Assert.That(snapshot.LastPressureReason, Does.Contain("乡面安宁"));
        Assert.That(snapshot.LastPressureReason, Does.Contain("民困"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("BanditThreatRaised"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("OutlawGroupFormed"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("RouteUnsafeDueToBanditry"));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.OrderAndBanditry));
        Assert.That(module.AcceptedCommands, Does.Contain("FundLocalWatch"));
        Assert.That(module.PublishedEvents, Does.Contain("RouteUnsafeDueToBanditry"));
    }

    [Test]
    public void RunMonth_ProjectsBlackRoutePressureThroughOwnedQuery()
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
            PublicNarrative = "Lanxi wharf pressure is thickening.",
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
            DiversionBandLabel = "正私并行",
            LastLedgerTrace = "Lanxi already has shadow cargo peeling off from the main wharf.",
        });

        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            BanditThreat = 55,
            RoutePressure = 55,
            SuppressionDemand = 13,
            DisorderPressure = 68,
            LastPressureReason = "Pressure was already building around the wharf.",
            BlackRoutePressure = 58,
            CoercionRisk = 22,
        });

        QueryRegistry queries = new();
        worldModule.RegisterQueries(worldState, queries);
        populationModule.RegisterQueries(populationState, queries);
        familyModule.RegisterQueries(familyState, queries);
        socialModule.RegisterQueries(socialState, queries);
        tradeModule.RegisterQueries(tradeState, queries);
        module.RegisterQueries(state, queries);

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            CreateEnabledManifest(),
            new DeterministicRandom(KernelState.Create(771)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));

        SettlementBlackRoutePressureSnapshot snapshot = queries.GetRequired<IBlackRoutePressureQueries>()
            .GetRequiredSettlementBlackRoutePressure(new SettlementId(2));

        Assert.That(snapshot.BlackRoutePressure, Is.GreaterThanOrEqualTo(60));
        Assert.That(snapshot.CoercionRisk, Is.GreaterThan(22));
        Assert.That(snapshot.EscalationBandLabel, Is.Not.Empty);
        Assert.That(snapshot.LastPressureTrace, Does.Contain("私下分流"));
        Assert.That(snapshot.LastPressureTrace, Does.Contain("民困"));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("BlackRoutePressureRaised"));
    }

    [Test]
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

    [Test]
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

    [Test]
    public void HandleEvents_RaisesDisorderFromCampaignSpilloverWithoutForeignMutation()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            BanditThreat = 54,
            RoutePressure = 57,
            SuppressionDemand = 42,
            DisorderPressure = 61,
            LastPressureReason = "Order is fragile but contained.",
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        queries.Register<IWarfareCampaignQueries>(new StubWarfareCampaignQueries(
        [
            new CampaignFrontSnapshot
            {
                CampaignId = new CampaignId(1),
                AnchorSettlementId = new SettlementId(2),
                AnchorSettlementName = "Lanxi",
                CampaignName = "Lanxi军务沙盘",
                IsActive = true,
                MobilizedForceCount = 44,
                FrontPressure = 76,
                FrontLabel = "前线转紧",
                SupplyState = 34,
                SupplyStateLabel = "粮道吃紧",
                MoraleState = 40,
                MoraleStateLabel = "军心摇动",
                CommandFitLabel = "号令尚整",
                CommanderSummary = "Lanxi command is strained.",
                ActiveDirectiveCode = WarfareCampaignCommandNames.CommitMobilization,
                ActiveDirectiveLabel = "发檄点兵",
                ActiveDirectiveSummary = "点集行伍。",
                LastDirectiveTrace = "兰溪已发檄点兵。",
                MobilizationWindowLabel = "可发",
                SupplyLineSummary = "运粮车队已显吃紧。",
                OfficeCoordinationTrace = "主簿正在转递军务文移。",
                SourceTrace = "Campaign pressure rose from local conflict.",
                LastAftermathSummary = "战后覆核与败粮余波压在路面。",
            },
        ]));

        DomainEventRecord[] events =
        {
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignSupplyStrained, "Lanxi supply strained.", "2"),
            new(KnownModuleKeys.WarfareCampaign, WarfareCampaignEventNames.CampaignAftermathRegistered, "Lanxi entered aftermath review.", "2"),
        };

        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            CreateConflictEnabledManifest(),
            new DeterministicRandom(KernelState.Create(7102)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.HandleEvents(new ModuleEventHandlingScope<OrderAndBanditryState>(state, context, events));

        SettlementDisorderState disorder = state.Settlements.Single();

        Assert.That(disorder.BanditThreat, Is.GreaterThan(54));
        Assert.That(disorder.RoutePressure, Is.GreaterThan(57));
        Assert.That(disorder.DisorderPressure, Is.GreaterThan(61));
        Assert.That(disorder.SuppressionDemand, Is.GreaterThan(42));
        Assert.That(disorder.LastPressureReason, Does.Contain("Lanxi战事外溢"));
        Assert.That(context.Diff.Entries.Single().ModuleKey, Is.EqualTo(KnownModuleKeys.OrderAndBanditry));
        Assert.That(context.DomainEvents.Events.Select(static entry => entry.EventType), Does.Contain("RouteUnsafeDueToBanditry"));
        Assert.That(context.DomainEvents.Events.All(static entry => entry.ModuleKey == KnownModuleKeys.OrderAndBanditry), Is.True);
    }

    [Test]
    public void RunMonth_ConsumesProtectiveCarryoverFromRecentWatchCommand()
    {
        static (SettlementDisorderSnapshot Disorder, SettlementBlackRoutePressureSnapshot Pressure) RunMonth(int carryoverMonths)
        {
            WorldSettlementsModule worldModule = new();
            WorldSettlementsState worldState = worldModule.CreateInitialState();
            worldState.Settlements.Add(new SettlementStateData
            {
                Id = new SettlementId(2),
                Name = "Lanxi",
                Security = 46,
                Prosperity = 58,
                BaselineInstitutionCount = 1,
            });

            PopulationAndHouseholdsModule populationModule = new();
            PopulationAndHouseholdsState populationState = populationModule.CreateInitialState();
            populationState.Settlements.Add(new PopulationSettlementState
            {
                SettlementId = new SettlementId(2),
                CommonerDistress = 61,
                LaborSupply = 92,
                MigrationPressure = 48,
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
                PublicNarrative = "Lanxi roads are uneasy but still open.",
                GrudgePressure = 44,
                FearPressure = 46,
                ShamePressure = 20,
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
                LastExplanation = "Road traffic is still moving.",
            });
            tradeState.Routes.Add(new RouteTradeState
            {
                RouteId = 1,
                ClanId = new ClanId(1),
                RouteName = "Lanxi River Wharf",
                SettlementId = new SettlementId(2),
                IsActive = true,
                Capacity = 28,
                Risk = 54,
                LastMargin = 3,
            });
            tradeState.BlackRouteLedgers.Add(new SettlementBlackRouteLedgerState
            {
                SettlementId = new SettlementId(2),
                ShadowPriceIndex = 116,
                DiversionShare = 22,
                IllicitMargin = 9,
                BlockedShipmentCount = 1,
                SeizureRisk = 18,
                DiversionBandLabel = "夹带渐多",
                LastLedgerTrace = "Shadow cargo is starting to peel off from the wharf.",
            });

            OrderAndBanditryModule module = new();
            OrderAndBanditryState state = module.CreateInitialState();
            state.Settlements.Add(new SettlementDisorderState
            {
                SettlementId = new SettlementId(2),
                BanditThreat = 49,
                RoutePressure = 55,
                SuppressionDemand = 37,
                DisorderPressure = 47,
                LastPressureReason = "Road pressure is tightening.",
                BlackRoutePressure = 34,
                CoercionRisk = 18,
                LastInterventionCommandCode = PlayerCommandNames.FundLocalWatch,
                LastInterventionCommandLabel = "添雇巡丁",
                LastInterventionSummary = "Added extra watchers around the road mouth.",
                LastInterventionOutcome = "The first response reached the road mouth.",
                InterventionCarryoverMonths = carryoverMonths,
            });

            QueryRegistry queries = new();
            worldModule.RegisterQueries(worldState, queries);
            populationModule.RegisterQueries(populationState, queries);
            familyModule.RegisterQueries(familyState, queries);
            socialModule.RegisterQueries(socialState, queries);
            tradeModule.RegisterQueries(tradeState, queries);
            module.RegisterQueries(state, queries);

            ModuleExecutionContext context = new(
                new GameDate(1200, 7),
                CreateEnabledManifest(),
                new DeterministicRandom(KernelState.Create(9201)),
                queries,
                new DomainEventBuffer(),
                new WorldDiff());

            module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));

            return (
                queries.GetRequired<IOrderAndBanditryQueries>().GetRequiredSettlementDisorder(new SettlementId(2)),
                queries.GetRequired<IBlackRoutePressureQueries>().GetRequiredSettlementBlackRoutePressure(new SettlementId(2)));
        }

        (SettlementDisorderSnapshot baselineDisorder, SettlementBlackRoutePressureSnapshot baselinePressure) = RunMonth(0);
        (SettlementDisorderSnapshot carriedDisorder, SettlementBlackRoutePressureSnapshot carriedPressure) = RunMonth(1);

        Assert.That(carriedDisorder.InterventionCarryoverMonths, Is.EqualTo(0));
        Assert.That(carriedDisorder.RoutePressure, Is.LessThan(baselineDisorder.RoutePressure));
        Assert.That(carriedDisorder.DisorderPressure, Is.LessThan(baselineDisorder.DisorderPressure));
        Assert.That(carriedPressure.RouteShielding, Is.GreaterThan(baselinePressure.RouteShielding));
        Assert.That(carriedPressure.BlackRoutePressure, Is.LessThan(baselinePressure.BlackRoutePressure));
    }

    [Test]
    public void RunMonth_CarriesSuppressionBacklashIntoNextMonthPressure()
    {
        static (SettlementDisorderSnapshot Disorder, SettlementBlackRoutePressureSnapshot Pressure) RunMonth(int carryoverMonths)
        {
            WorldSettlementsModule worldModule = new();
            WorldSettlementsState worldState = worldModule.CreateInitialState();
            worldState.Settlements.Add(new SettlementStateData
            {
                Id = new SettlementId(2),
                Name = "Lanxi",
                Security = 44,
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
                MigrationPressure = 52,
                MilitiaPotential = 48,
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
                PublicNarrative = "Lanxi is trying to quiet the roads by force.",
                GrudgePressure = 56,
                FearPressure = 54,
                ShamePressure = 20,
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
                LastExplanation = "Road traffic is still moving.",
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
                LastMargin = 3,
            });
            tradeState.BlackRouteLedgers.Add(new SettlementBlackRouteLedgerState
            {
                SettlementId = new SettlementId(2),
                ShadowPriceIndex = 119,
                DiversionShare = 28,
                IllicitMargin = 12,
                BlockedShipmentCount = 1,
                SeizureRisk = 22,
                DiversionBandLabel = "正私并行",
                LastLedgerTrace = "Some shadow cargo already keeps moving around the main road.",
            });

            OrderAndBanditryModule module = new();
            OrderAndBanditryState state = module.CreateInitialState();
            state.Settlements.Add(new SettlementDisorderState
            {
                SettlementId = new SettlementId(2),
                BanditThreat = 58,
                RoutePressure = 52,
                SuppressionDemand = 44,
                DisorderPressure = 49,
                LastPressureReason = "Road pressure is tightening.",
                BlackRoutePressure = 38,
                CoercionRisk = 20,
                LastInterventionCommandCode = PlayerCommandNames.SuppressBanditry,
                LastInterventionCommandLabel = "严缉路匪",
                LastInterventionSummary = "Crackdown has started around the road line.",
                LastInterventionOutcome = "Open banditry is pressed down for the moment.",
                InterventionCarryoverMonths = carryoverMonths,
            });

            QueryRegistry queries = new();
            worldModule.RegisterQueries(worldState, queries);
            populationModule.RegisterQueries(populationState, queries);
            familyModule.RegisterQueries(familyState, queries);
            socialModule.RegisterQueries(socialState, queries);
            tradeModule.RegisterQueries(tradeState, queries);
            module.RegisterQueries(state, queries);

            ModuleExecutionContext context = new(
                new GameDate(1200, 7),
                CreateEnabledManifest(),
                new DeterministicRandom(KernelState.Create(9202)),
                queries,
                new DomainEventBuffer(),
                new WorldDiff());

            module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));

            return (
                queries.GetRequired<IOrderAndBanditryQueries>().GetRequiredSettlementDisorder(new SettlementId(2)),
                queries.GetRequired<IBlackRoutePressureQueries>().GetRequiredSettlementBlackRoutePressure(new SettlementId(2)));
        }

        (SettlementDisorderSnapshot baselineDisorder, SettlementBlackRoutePressureSnapshot baselinePressure) = RunMonth(0);
        (SettlementDisorderSnapshot carriedDisorder, SettlementBlackRoutePressureSnapshot carriedPressure) = RunMonth(1);

        Assert.That(carriedDisorder.InterventionCarryoverMonths, Is.EqualTo(0));
        Assert.That(carriedDisorder.BanditThreat, Is.LessThanOrEqualTo(baselineDisorder.BanditThreat));
        Assert.That(carriedPressure.RetaliationRisk, Is.GreaterThan(baselinePressure.RetaliationRisk));
        Assert.That(carriedPressure.BlackRoutePressure, Is.GreaterThan(baselinePressure.BlackRoutePressure));
        Assert.That(carriedPressure.CoercionRisk, Is.GreaterThan(baselinePressure.CoercionRisk));
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
        return manifest;
    }

    private static FeatureManifest CreateConflictEnabledManifest()
    {
        FeatureManifest manifest = CreateEnabledManifest();
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        return manifest;
    }

    private static FeatureManifest CreateGovernanceEnabledManifest()
    {
        FeatureManifest manifest = CreateEnabledManifest();
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        return manifest;
    }

    private static FeatureManifest CreateGovernanceConflictEnabledManifest()
    {
        FeatureManifest manifest = CreateGovernanceEnabledManifest();
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        return manifest;
    }

    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries
    {
        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;

        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)
        {
            _campaigns = campaigns;
        }

        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)
        {
            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);
        }

        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()
        {
            return _campaigns;
        }

        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()
        {
            return [];
        }
    }
}
