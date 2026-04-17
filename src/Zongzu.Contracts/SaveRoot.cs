using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public sealed class SaveRoot
{
    public int RootSchemaVersion { get; set; }

    public GameDate CurrentDate { get; set; }

    public FeatureManifest FeatureManifest { get; set; } = new();

    public KernelState KernelState { get; set; } = KernelState.Create(1);

    public Dictionary<string, ModuleStateEnvelope> ModuleStates { get; set; } = new();
}
