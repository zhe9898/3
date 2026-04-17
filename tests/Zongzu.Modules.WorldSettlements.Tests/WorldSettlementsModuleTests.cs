using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.WorldSettlements.Tests;

[TestFixture]
public sealed class WorldSettlementsModuleTests
{
    [Test]
    public void RunMonth_UpdatesSettlementPressureWithinBounds()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Security = 50,
            Prosperity = 50,
            BaselineInstitutionCount = 1,
        });

        ModuleExecutionContext context = new(
            new GameDate(1200, 1),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(7)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(state.Settlements[0].Security, Is.InRange(0, 100));
        Assert.That(state.Settlements[0].Prosperity, Is.InRange(0, 100));
        Assert.That(context.Diff.Entries, Has.Count.EqualTo(1));
        Assert.That(context.DomainEvents.Events, Has.Count.EqualTo(1));
    }
}
