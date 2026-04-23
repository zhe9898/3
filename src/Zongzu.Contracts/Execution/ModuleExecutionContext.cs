using System;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class ModuleExecutionContext
{
    public ModuleExecutionContext(
        GameDate currentDate,
        FeatureManifest featureManifest,
        IDeterministicRandom random,
        QueryRegistry queries,
        DomainEventBuffer domainEvents,
        WorldDiff diff,
        KernelState? kernelState = null,
        SimulationCadenceBand cadenceBand = SimulationCadenceBand.Month,
        SimulationXun currentXun = SimulationXun.None)
    {
        CurrentDate = currentDate;
        FeatureManifest = featureManifest ?? throw new ArgumentNullException(nameof(featureManifest));
        Random = random ?? throw new ArgumentNullException(nameof(random));
        Queries = queries ?? throw new ArgumentNullException(nameof(queries));
        DomainEvents = domainEvents ?? throw new ArgumentNullException(nameof(domainEvents));
        Diff = diff ?? throw new ArgumentNullException(nameof(diff));
        KernelState = kernelState ?? KernelState.Create(1);
        CadenceBand = cadenceBand;
        CurrentXun = cadenceBand == SimulationCadenceBand.Xun ? currentXun : SimulationXun.None;
    }

    public GameDate CurrentDate { get; }

    public FeatureManifest FeatureManifest { get; }

    public IDeterministicRandom Random { get; }

    public QueryRegistry Queries { get; }

    public DomainEventBuffer DomainEvents { get; }

    public WorldDiff Diff { get; }

    public KernelState KernelState { get; }

    public SimulationCadenceBand CadenceBand { get; }

    public SimulationXun CurrentXun { get; }

    public bool IsXunPulse => CadenceBand == SimulationCadenceBand.Xun;
}
