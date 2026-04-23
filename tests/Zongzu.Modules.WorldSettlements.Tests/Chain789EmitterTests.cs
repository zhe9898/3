using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.WorldSettlements.Tests;

[TestFixture]
public sealed class Chain789EmitterTests
{
    [Test]
    public void RunMonth_DefaultMandateConfidence_DoesNotEmitCourtOrRegimePressure()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.CourtAgendaPressureAccumulated),
            "Unseeded imperial confidence must be neutral, not treated as a court crisis.");
        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.RegimeLegitimacyShifted),
            "Unseeded imperial confidence must not defect officials by default.");
    }

    [Test]
    public void RunMonth_CourtAgendaPressureAccumulated_LowMandateConfidence_EmitsEvent()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 30;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        IDomainEvent pressure = context.DomainEvents.Events.Single(
            static e => e.EventType == WorldSettlementsEventNames.CourtAgendaPressureAccumulated);
        Assert.That(pressure.EntityKey, Is.EqualTo("court"));
        Assert.That(pressure.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseCourt));
        Assert.That(pressure.Metadata[DomainEventMetadataKeys.MandateConfidence], Is.EqualTo("30"));
        Assert.That(state.LastCourtAgendaPressureDeclared, Is.True);
    }

    [Test]
    public void RunMonth_CourtAgendaPressureAccumulated_HighMandateConfidence_DoesNotEmit()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 50;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.CourtAgendaPressureAccumulated),
            "MandateConfidence above threshold must not emit court agenda pressure.");
        Assert.That(state.LastCourtAgendaPressureDeclared, Is.False);
    }

    [Test]
    public void RunMonth_CourtAgendaPressureAccumulated_ActivePressure_DoesNotRedeclare()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 30;
        state.LastCourtAgendaPressureDeclared = true;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.CourtAgendaPressureAccumulated),
            "Persistent low mandate confidence must not re-emit court agenda pressure every month.");
        Assert.That(state.LastCourtAgendaPressureDeclared, Is.True);
    }

    [Test]
    public void RunMonth_CourtAgendaPressureAccumulated_RisesAboveThreshold_ClearsWatermark()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 50;
        state.LastCourtAgendaPressureDeclared = true;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(state.LastCourtAgendaPressureDeclared, Is.False);
    }

    [Test]
    public void RunMonth_RegimeLegitimacyShifted_LowMandateConfidence_EmitsEvent()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 20;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        IDomainEvent shift = context.DomainEvents.Events.Single(
            static e => e.EventType == WorldSettlementsEventNames.RegimeLegitimacyShifted);
        Assert.That(shift.EntityKey, Is.EqualTo("regime"));
        Assert.That(shift.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseRegime));
        Assert.That(shift.Metadata[DomainEventMetadataKeys.MandateConfidence], Is.EqualTo("20"));
        Assert.That(state.LastRegimeLegitimacyShiftDeclared, Is.True);
    }

    [Test]
    public void RunMonth_RegimeLegitimacyShifted_HighMandateConfidence_DoesNotEmit()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 30;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.RegimeLegitimacyShifted),
            "MandateConfidence above regime threshold must not emit regime legitimacy shift.");
        Assert.That(state.LastRegimeLegitimacyShiftDeclared, Is.False);
    }

    [Test]
    public void RunMonth_RegimeLegitimacyShifted_ActiveShift_DoesNotRedeclare()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 20;
        state.LastRegimeLegitimacyShiftDeclared = true;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(
            context.DomainEvents.Events,
            Has.None.Matches<IDomainEvent>(static e => e.EventType == WorldSettlementsEventNames.RegimeLegitimacyShifted),
            "Persistent very low mandate confidence must not re-emit regime legitimacy shift every month.");
        Assert.That(state.LastRegimeLegitimacyShiftDeclared, Is.True);
    }

    [Test]
    public void RunMonth_RegimeLegitimacyShifted_RisesAboveThreshold_ClearsWatermark()
    {
        WorldSettlementsModule module = new();
        WorldSettlementsState state = module.CreateInitialState();
        state.CurrentSeason.Imperial.MandateConfidence = 30;
        state.LastRegimeLegitimacyShiftDeclared = true;

        ModuleExecutionContext context = new(
            new GameDate(1022, 6),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            new DomainEventBuffer(),
            new WorldDiff());

        module.RunMonth(new ModuleExecutionScope<WorldSettlementsState>(state, context));

        Assert.That(state.LastRegimeLegitimacyShiftDeclared, Is.False);
    }
}
