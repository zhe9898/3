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

        ProbeModule worldA = new("WorldProbeA", SimulationPhase.WorldBaseline, 100, SimulationCadencePresets.MonthOnly);
        ProbeModule family = new("FamilyProbe", SimulationPhase.FamilyStructure, 300, SimulationCadencePresets.MonthOnly);
        ProbeModule worldB = new("WorldProbeB", SimulationPhase.WorldBaseline, 200, SimulationCadencePresets.MonthOnly);
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

    [Test]
    public void AdvanceOneMonth_RunsThreeXunPulsesBeforeMonthPass()
    {
        FeatureManifest manifest = new();
        manifest.Set("WorldProbe", FeatureMode.Full);
        manifest.Set("FamilyProbe", FeatureMode.Full);

        ProbeModule world = new("WorldProbe", SimulationPhase.WorldBaseline, 100, SimulationCadencePresets.XunAndMonth);
        ProbeModule family = new("FamilyProbe", SimulationPhase.FamilyStructure, 300, SimulationCadencePresets.XunAndMonth);
        IReadOnlyList<IModuleRunner> modules = [family, world];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1200, 1),
            manifest,
            new DeterministicRandom(KernelState.Create(77)),
            states,
            modules);

        string[] descriptions = result.Diff.Entries.Select(static entry => entry.Description).ToArray();

        Assert.That(
            descriptions,
            Is.EqualTo(
            new[]
            {
                "Executed WorldProbe during Shangxun.",
                "Executed FamilyProbe during Shangxun.",
                "Executed WorldProbe during Zhongxun.",
                "Executed FamilyProbe during Zhongxun.",
                "Executed WorldProbe during Xiaxun.",
                "Executed FamilyProbe during Xiaxun.",
                "Executed WorldProbe during month.",
                "Executed FamilyProbe during month.",
            }));
    }

    [Test]
    public void AdvanceOneMonth_ProcessesHandlerEventsBeforeProjection()
    {
        FeatureManifest manifest = new();
        manifest.Set("EventSource", FeatureMode.Full);
        manifest.Set("EventConsumer", FeatureMode.Full);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        EventSourceModule source = new();
        EventConsumerModule consumer = new();
        ProjectionProbeModule projection = new();
        IReadOnlyList<IModuleRunner> modules = [projection, consumer, source];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1200, 1),
            manifest,
            new DeterministicRandom(KernelState.Create(901)),
            states,
            modules);

        string[] diffModules = result.Diff.Entries.Select(static entry => entry.ModuleKey).ToArray();
        string[] eventTypes = result.DomainEvents.Select(static entry => entry.EventType).ToArray();

        Assert.That(diffModules, Is.EqualTo(new[] { "EventSource", "EventConsumer", KnownModuleKeys.NarrativeProjection }));
        Assert.That(eventTypes, Is.EqualTo(new[] { "SourceRaised", "ConsumerHandled" }));
        Assert.That(result.Diff.Entries[^1].Description, Does.Contain("2 events"));
    }

    private sealed class ProbeModule : ModuleRunner<ProbeState>
    {
        public ProbeModule(string moduleKey, SimulationPhase phase, int executionOrder)
            : this(moduleKey, phase, executionOrder, SimulationCadencePresets.MonthOnly)
        {
        }

        public ProbeModule(
            string moduleKey,
            SimulationPhase phase,
            int executionOrder,
            IReadOnlyCollection<SimulationCadenceBand> cadenceBands)
        {
            ModuleKey = moduleKey;
            Phase = phase;
            ExecutionOrder = executionOrder;
            CadenceBands = cadenceBands;
        }

        public override string ModuleKey { get; }

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase { get; }

        public override int ExecutionOrder { get; }

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands { get; }

        public override ProbeState CreateInitialState()
        {
            return new ProbeState { ModuleKeyValue = ModuleKey };
        }

        public override void RunXun(ModuleExecutionScope<ProbeState> scope)
        {
            scope.RecordDiff($"Executed {ModuleKey} during {scope.Context.CurrentXun}.");
        }

        public override void RunMonth(ModuleExecutionScope<ProbeState> scope)
        {
            scope.RecordDiff($"Executed {ModuleKey} during month.");
        }
    }

    private sealed class ProbeState : IModuleStateDescriptor
    {
        public string ModuleKey => ModuleKeyValue;

        public string ModuleKeyValue { get; set; } = string.Empty;
    }

    private sealed class EventSourceModule : ModuleRunner<ProbeState>
    {
        public override string ModuleKey => "EventSource";

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

        public override int ExecutionOrder => 100;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override IReadOnlyCollection<string> PublishedEvents => [ "SourceRaised" ];

        public override ProbeState CreateInitialState()
        {
            return new ProbeState { ModuleKeyValue = ModuleKey };
        }

        public override void RunMonth(ModuleExecutionScope<ProbeState> scope)
        {
            scope.RecordDiff("Source ran.");
            scope.Emit("SourceRaised", "Source raised a deterministic event.", "1");
        }
    }

    private sealed class EventConsumerModule : ModuleRunner<ProbeState>
    {
        public override string ModuleKey => "EventConsumer";

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

        public override int ExecutionOrder => 200;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override IReadOnlyCollection<string> ConsumedEvents => [ "SourceRaised" ];

        public override IReadOnlyCollection<string> PublishedEvents => [ "ConsumerHandled" ];

        public override ProbeState CreateInitialState()
        {
            return new ProbeState { ModuleKeyValue = ModuleKey };
        }

        public override void RunMonth(ModuleExecutionScope<ProbeState> scope)
        {
        }

        public override void HandleEvents(ModuleEventHandlingScope<ProbeState> scope)
        {
            if (scope.Events.Any(static domainEvent => domainEvent.EventType == "SourceRaised"))
            {
                scope.RecordDiff("Consumer handled source event.");
                scope.Emit("ConsumerHandled", "Consumer handled the source event.", "1");
            }
        }
    }

    private sealed class ProjectionProbeModule : ModuleRunner<ProbeState>
    {
        public override string ModuleKey => KnownModuleKeys.NarrativeProjection;

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase => SimulationPhase.Projection;

        public override int ExecutionOrder => 100;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override ProbeState CreateInitialState()
        {
            return new ProbeState { ModuleKeyValue = ModuleKey };
        }

        public override void RunMonth(ModuleExecutionScope<ProbeState> scope)
        {
            scope.RecordDiff($"Projection saw {scope.Context.DomainEvents.Events.Count} events after handler processing.");
        }
    }

    [Test]
    public void AdvanceOneMonth_BoundedEventDrain_PropagatesIntraMonthFollowOnEvents()
    {
        // Verifies that a follow-on event emitted by a handler in round N
        // is visible to downstream modules in round N+1 within the same
        // month-end pass. See SIMULATION.md Phase 2.
        FeatureManifest manifest = new();
        manifest.Set("CascadeSource", FeatureMode.Full);
        manifest.Set("CascadeMiddle", FeatureMode.Full);
        manifest.Set("CascadeSink", FeatureMode.Full);

        CascadeSourceModule source = new();
        CascadeMiddleModule middle = new();
        CascadeSinkModule sink = new();
        IReadOnlyList<IModuleRunner> modules = [sink, middle, source];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1200, 1),
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            states,
            modules);

        string[] eventTypes = result.DomainEvents.Select(static e => e.EventType).ToArray();

        Assert.That(
            eventTypes,
            Is.EqualTo(new[] { "FirstEvent", "SecondEvent" }),
            "Sink must see the follow-on event emitted by Middle in the same month-end pass.");

        // Verify sink state recorded the follow-on event.
        CascadeSinkState? sinkState = states["CascadeSink"] as CascadeSinkState;
        Assert.That(sinkState, Is.Not.Null);
        Assert.That(sinkState!.SawSecondEvent, Is.True, "Sink should have handled SecondEvent.");
    }

    private sealed class CascadeSourceModule : ModuleRunner<CascadeSourceState>
    {
        public override string ModuleKey => "CascadeSource";

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase => SimulationPhase.WorldBaseline;

        public override int ExecutionOrder => 100;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override IReadOnlyCollection<string> PublishedEvents => ["FirstEvent"];

        public override CascadeSourceState CreateInitialState() => new();

        public override void RunMonth(ModuleExecutionScope<CascadeSourceState> scope)
        {
            scope.Emit("FirstEvent", "Source raised first event.", "1");
        }
    }

    private sealed class CascadeSourceState : IModuleStateDescriptor
    {
        public string ModuleKey => "CascadeSource";
    }

    private sealed class CascadeMiddleModule : ModuleRunner<CascadeMiddleState>
    {
        public override string ModuleKey => "CascadeMiddle";

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase => SimulationPhase.FamilyStructure;

        public override int ExecutionOrder => 200;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override IReadOnlyCollection<string> ConsumedEvents => ["FirstEvent"];

        public override IReadOnlyCollection<string> PublishedEvents => ["SecondEvent"];

        public override CascadeMiddleState CreateInitialState() => new();

        public override void RunMonth(ModuleExecutionScope<CascadeMiddleState> scope)
        {
        }

        public override void HandleEvents(ModuleEventHandlingScope<CascadeMiddleState> scope)
        {
            if (scope.Events.Any(static e => e.EventType == "FirstEvent"))
            {
                scope.Emit("SecondEvent", "Middle raised follow-on event.", "2");
            }
        }
    }

    private sealed class CascadeMiddleState : IModuleStateDescriptor
    {
        public string ModuleKey => "CascadeMiddle";
    }

    private sealed class CascadeSinkModule : ModuleRunner<CascadeSinkState>
    {
        public override string ModuleKey => "CascadeSink";

        public override int ModuleSchemaVersion => 1;

        public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

        public override int ExecutionOrder => 300;

        public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.MonthOnly;

        public override IReadOnlyCollection<string> ConsumedEvents => ["SecondEvent"];

        public override CascadeSinkState CreateInitialState() => new();

        public override void RunMonth(ModuleExecutionScope<CascadeSinkState> scope)
        {
        }

        public override void HandleEvents(ModuleEventHandlingScope<CascadeSinkState> scope)
        {
            if (scope.Events.Any(static e => e.EventType == "SecondEvent"))
            {
                scope.State.SawSecondEvent = true;
            }
        }
    }

    private sealed class CascadeSinkState : IModuleStateDescriptor
    {
        public string ModuleKey => "CascadeSink";

        public bool SawSecondEvent { get; set; }
    }
}
