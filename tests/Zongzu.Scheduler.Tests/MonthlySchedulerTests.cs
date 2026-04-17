using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Scheduler;

namespace Zongzu.Scheduler.Tests;

[TestFixture]
public sealed class MonthlySchedulerTests
{
    [Test]
    public void AdvanceOneMonth_UsesDeterministicModuleOrder()
    {
        FeatureManifest manifest = new();
        manifest.Set("FamilyProbe", FeatureMode.Full);
        manifest.Set("WorldProbeA", FeatureMode.Full);
        manifest.Set("WorldProbeB", FeatureMode.Full);

        ProbeModule worldA = new("WorldProbeA", SimulationPhase.WorldBaseline, 100);
        ProbeModule family = new("FamilyProbe", SimulationPhase.FamilyStructure, 300);
        ProbeModule worldB = new("WorldProbeB", SimulationPhase.WorldBaseline, 200);
        IReadOnlyList<IModuleRunner> modules = [family, worldB, worldA];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1200, 1),
            manifest,
            new DeterministicRandom(KernelState.Create(99)),
            states,
            modules);

        string[] executedModules = result.Diff.Entries.Select(static entry => entry.ModuleKey).ToArray();

        Assert.That(executedModules, Is.EqualTo(new[] { "WorldProbeA", "WorldProbeB", "FamilyProbe" }));
    }

    private sealed class ProbeModule : ModuleRunner<ProbeState>
    {
        public ProbeModule(string moduleKey, SimulationPhase phase, int executionOrder)
        {
            ModuleKey = moduleKey;
            Phase = phase;
            ExecutionOrder = executionOrder;
        }

        public override string ModuleKey { get; }

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase { get; }

        public override int ExecutionOrder { get; }

        public override ProbeState CreateInitialState()
        {
            return new ProbeState { ModuleKeyValue = ModuleKey };
        }

        public override void RunMonth(ModuleExecutionScope<ProbeState> scope)
        {
            scope.RecordDiff($"Executed {ModuleKey}.");
        }
    }

    private sealed class ProbeState : IModuleStateDescriptor
    {
        public string ModuleKey => ModuleKeyValue;

        public string ModuleKeyValue { get; set; } = string.Empty;
    }
}
