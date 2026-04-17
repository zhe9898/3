using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.OrderAndBanditry;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

[TestFixture]
public sealed class OrderAndBanditryModuleTests
{
    [Test]
    public void RegisterQueries_ExposesStableSnapshots_AndRunMonthIsNoOpInPreflight()
    {
        OrderAndBanditryModule module = new();
        OrderAndBanditryState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = new SettlementId(2),
            BanditThreat = 18,
            RoutePressure = 24,
            SuppressionDemand = 13,
            DisorderPressure = 21,
            LastPressureReason = "Preflight route pressure placeholder.",
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        IOrderAndBanditryQueries disorderQueries = queries.GetRequired<IOrderAndBanditryQueries>();

        SettlementDisorderSnapshot snapshot = disorderQueries.GetRequiredSettlementDisorder(new SettlementId(2));
        Assert.That(snapshot.BanditThreat, Is.EqualTo(18));
        Assert.That(snapshot.RoutePressure, Is.EqualTo(24));
        Assert.That(snapshot.LastPressureReason, Does.Contain("placeholder"));
        Assert.That(module.AcceptedCommands, Does.Contain("FundLocalWatch"));
        Assert.That(module.PublishedEvents, Does.Contain("RouteUnsafeDueToBanditry"));

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(77)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<OrderAndBanditryState>(state, context));

        Assert.That(state.Settlements.Single().DisorderPressure, Is.EqualTo(21));
        Assert.That(context.DomainEvents.Events, Is.Empty);
        Assert.That(context.Diff.Entries, Is.Empty);
    }
}
