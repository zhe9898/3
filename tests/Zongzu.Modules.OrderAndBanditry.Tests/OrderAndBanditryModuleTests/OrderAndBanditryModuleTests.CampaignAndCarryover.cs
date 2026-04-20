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


}
