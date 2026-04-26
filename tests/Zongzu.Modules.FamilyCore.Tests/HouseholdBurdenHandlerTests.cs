using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore.Tests;

[TestFixture]
public sealed class HouseholdBurdenHandlerTests
{
    [Test]
    public void HouseholdDebtSpiked_PressesSponsorClanOnly()
    {
        FamilyCoreModule module = new();
        FamilyCoreState state = BuildFamilyState();
        QueryRegistry queries = BuildQueries(state, new HouseholdPressureSnapshot
        {
            Id = new HouseholdId(1),
            HouseholdName = "Zhang household",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 72,
            DebtPressure = 86,
            LaborCapacity = 30,
            MigrationRisk = 72,
            DependentCount = 3,
            LaborerCount = 1,
        });

        DomainEventBuffer buffer = BuildContextAndBuffer(queries, out ModuleExecutionContext context);
        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.PopulationAndHouseholds,
            PopulationEventNames.HouseholdDebtSpiked,
            "DO_NOT_PARSE_HOUSEHOLD_DEBT_SUMMARY",
            "1",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseTaxSeason,
                [DomainEventMetadataKeys.TaxDebtDelta] = "24",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state, context, buffer.Events.ToList()));

        ClanStateData sponsor = state.Clans.Single(static clan => clan.Id == new ClanId(1));
        ClanStateData offScope = state.Clans.Single(static clan => clan.Id == new ClanId(2));

        Assert.That(sponsor.CharityObligation, Is.GreaterThan(10));
        Assert.That(sponsor.SupportReserve, Is.LessThan(35));
        Assert.That(sponsor.BranchTension, Is.GreaterThan(54));
        Assert.That(sponsor.LastLifecycleTrace, Does.Contain("Zhang household"));
        Assert.That(sponsor.LastLifecycleTrace, Does.Not.Contain("DO_NOT_PARSE"));

        Assert.That(offScope.CharityObligation, Is.EqualTo(3));
        Assert.That(offScope.SupportReserve, Is.EqualTo(50));
        Assert.That(offScope.BranchTension, Is.EqualTo(12));

        IDomainEvent lineageReceipt = buffer.Events.Single(e => e.EventType == FamilyCoreEventNames.LineageDisputeHardened);
        Assert.That(lineageReceipt.Metadata[DomainEventMetadataKeys.SourceEventType], Is.EqualTo(PopulationEventNames.HouseholdDebtSpiked));
        Assert.That(lineageReceipt.Metadata[DomainEventMetadataKeys.HouseholdId], Is.EqualTo("1"));
        Assert.That(lineageReceipt.Metadata[DomainEventMetadataKeys.ClanId], Is.EqualTo("1"));
        Assert.That(lineageReceipt.Metadata[DomainEventMetadataKeys.FamilyCharityObligationDelta], Is.EqualTo("14"));
    }

    [Test]
    public void HouseholdSubsistencePressureChanged_UsesStructuredHouseholdSnapshot()
    {
        FamilyCoreModule module = new();
        FamilyCoreState state = BuildFamilyState();
        QueryRegistry queries = BuildQueries(state, new HouseholdPressureSnapshot
        {
            Id = new HouseholdId(7),
            HouseholdName = "Li household",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 88,
            DebtPressure = 67,
            LaborCapacity = 24,
            MigrationRisk = 64,
            DependentCount = 4,
            LaborerCount = 1,
        });

        DomainEventBuffer buffer = BuildContextAndBuffer(queries, out ModuleExecutionContext context);
        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.PopulationAndHouseholds,
            PopulationEventNames.HouseholdSubsistencePressureChanged,
            "DO_NOT_PARSE_SUBSISTENCE_SUMMARY",
            null,
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.HouseholdId] = "7",
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseGrainPriceSpike,
                [DomainEventMetadataKeys.SubsistenceDistressDelta] = "8",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state, context, buffer.Events.ToList()));

        ClanStateData sponsor = state.Clans.Single(static clan => clan.Id == new ClanId(1));
        Assert.That(sponsor.CharityObligation, Is.GreaterThan(10));
        Assert.That(sponsor.ReliefSanctionPressure, Is.GreaterThan(4));
        Assert.That(sponsor.LastLifecycleTrace, Does.Contain("粮价生计"));
        Assert.That(sponsor.LastLifecycleTrace, Does.Not.Contain("DO_NOT_PARSE"));
    }

    [Test]
    public void HouseholdBurdenIncreased_WithoutSponsorClan_IsNoOp()
    {
        FamilyCoreModule module = new();
        FamilyCoreState state = BuildFamilyState();
        QueryRegistry queries = BuildQueries(state, new HouseholdPressureSnapshot
        {
            Id = new HouseholdId(9),
            HouseholdName = "Unsponsored household",
            SettlementId = new SettlementId(1),
            SponsorClanId = null,
            Distress = 90,
            DebtPressure = 80,
        });

        DomainEventBuffer buffer = BuildContextAndBuffer(queries, out ModuleExecutionContext context);
        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.PopulationAndHouseholds,
            PopulationEventNames.HouseholdBurdenIncreased,
            "DO_NOT_PARSE_OFF_SCOPE_SUMMARY",
            "9",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.OfficialSupplyDistressDelta] = "8",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Clans[0].CharityObligation, Is.EqualTo(10));
        Assert.That(state.Clans[0].SupportReserve, Is.EqualTo(35));
        Assert.That(state.Clans[1].CharityObligation, Is.EqualTo(3));
        Assert.That(state.Clans[1].SupportReserve, Is.EqualTo(50));
        Assert.That(buffer.Events.Count, Is.EqualTo(1));
    }

    private static FamilyCoreState BuildFamilyState()
    {
        FamilyCoreState state = new();
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang clan",
            HomeSettlementId = new SettlementId(1),
            CharityObligation = 10,
            SupportReserve = 35,
            BranchTension = 54,
            ReliefSanctionPressure = 4,
        });
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Li clan",
            HomeSettlementId = new SettlementId(2),
            CharityObligation = 3,
            SupportReserve = 50,
            BranchTension = 12,
            ReliefSanctionPressure = 0,
        });

        return state;
    }

    private static QueryRegistry BuildQueries(FamilyCoreState state, params HouseholdPressureSnapshot[] households)
    {
        QueryRegistry queries = new();
        new FamilyCoreModule().RegisterQueries(state, queries);
        queries.Register<IPopulationAndHouseholdsQueries>(new FakePopulationQueries(households));
        return queries;
    }

    private static DomainEventBuffer BuildContextAndBuffer(QueryRegistry queries, out ModuleExecutionContext context)
    {
        DomainEventBuffer buffer = new();
        context = new ModuleExecutionContext(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());
        return buffer;
    }

    private sealed class FakePopulationQueries : IPopulationAndHouseholdsQueries
    {
        private readonly Dictionary<HouseholdId, HouseholdPressureSnapshot> _households;

        public FakePopulationQueries(IEnumerable<HouseholdPressureSnapshot> households)
        {
            _households = households.ToDictionary(static household => household.Id);
        }

        public HouseholdPressureSnapshot GetRequiredHousehold(HouseholdId householdId)
        {
            return _households[householdId];
        }

        public PopulationSettlementSnapshot GetRequiredSettlement(SettlementId settlementId)
        {
            return new PopulationSettlementSnapshot { SettlementId = settlementId };
        }

        public IReadOnlyList<HouseholdPressureSnapshot> GetHouseholds()
        {
            return _households.Values.OrderBy(static household => household.Id.Value).ToArray();
        }

        public IReadOnlyList<PopulationSettlementSnapshot> GetSettlements()
        {
            return [];
        }
    }
}
