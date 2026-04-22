using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Presentation.Unity;

namespace Zongzu.Host;

// Owns the single authoritative simulation instance for this host process.
// UI layers never mutate it directly; they read projected view models and post commands.
public sealed class SimulationHostService
{
    private readonly object _gate = new();
    private readonly PresentationReadModelBuilder _builder = new();
    private GameSimulation _simulation;

    public SimulationHostService()
    {
        _simulation = SimulationBootstrapper.CreateMvpBootstrap(seed: 20260419);
    }

    public PresentationShellViewModel SnapshotShell()
    {
        lock (_gate)
        {
            PresentationReadModelBundle bundle = _builder.BuildForM2(_simulation);
            return FirstPassPresentationShell.Compose(bundle);
        }
    }

    public PresentationShellViewModel AdvanceOneMonth()
    {
        lock (_gate)
        {
            _simulation.AdvanceOneMonth();
            PresentationReadModelBundle bundle = _builder.BuildForM2(_simulation);
            return FirstPassPresentationShell.Compose(bundle);
        }
    }
}
