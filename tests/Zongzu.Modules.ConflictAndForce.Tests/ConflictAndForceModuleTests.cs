using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;

namespace Zongzu.Modules.ConflictAndForce.Tests;

[TestFixture]
public sealed class ConflictAndForceModuleTests
{
    [Test]
    public void RegisterQueries_ExposesStableForceSnapshots_AndRunMonthIsNoOpInPreflight()
    {
        ConflictAndForceModule module = new();
        ConflictAndForceState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementForceState
        {
            SettlementId = new SettlementId(3),
            GuardCount = 12,
            RetainerCount = 5,
            MilitiaCount = 18,
            EscortCount = 4,
            Readiness = 37,
            CommandCapacity = 22,
            LastConflictTrace = "Preflight local conflict placeholder.",
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        IConflictAndForceQueries forceQueries = queries.GetRequired<IConflictAndForceQueries>();

        LocalForcePoolSnapshot snapshot = forceQueries.GetRequiredSettlementForce(new SettlementId(3));
        Assert.That(snapshot.GuardCount, Is.EqualTo(12));
        Assert.That(snapshot.MilitiaCount, Is.EqualTo(18));
        Assert.That(snapshot.LastConflictTrace, Does.Contain("placeholder"));
        Assert.That(module.AcceptedCommands, Does.Contain("MobilizeClanMilitia"));
        Assert.That(module.PublishedEvents, Does.Contain("ConflictResolved"));

        ModuleExecutionContext context = new(
            new GameDate(1200, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(88)),
            queries,
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<ConflictAndForceState>(state, context));

        Assert.That(state.Settlements.Single().Readiness, Is.EqualTo(37));
        Assert.That(context.DomainEvents.Events, Is.Empty);
        Assert.That(context.Diff.Entries, Is.Empty);
    }
}
