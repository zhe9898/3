using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

[TestFixture]
public sealed class PolicyImplementationHandlerTests
{
    [Test]
    public void PublishedEvents_DeclarePolicyImplemented()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.PublishedEvents,
            Does.Contain(OfficeAndCareerEventNames.PolicyImplemented),
            "OfficeAndCareer emits PolicyImplemented, so the event contract must declare it.");
    }

    [Test]
    public void ConsumedEvents_DeclarePolicyWindowOpened()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.ConsumedEvents,
            Does.Contain(OfficeAndCareerEventNames.PolicyWindowOpened),
            "OfficeAndCareer consumes PolicyWindowOpened, so the event contract must declare it.");
    }

    [Test]
    public void PolicyWindowOpened_DraggedImplementation_UpdatesMatchingOfficeStateAndEmitsPolicyImplemented()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(MakeOfficial(
            personId: 1,
            settlementId: 10,
            authorityTier: 3,
            clerk: 35,
            backlog: 40,
            taskLoad: 50,
            leverage: 20,
            petition: 30,
            reputation: 50));
        state.People.Add(MakeOfficial(
            personId: 2,
            settlementId: 20,
            authorityTier: 3,
            clerk: 10,
            backlog: 10,
            taskLoad: 10));
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = BuildContext(module, state, buffer);
        buffer.Emit(MakePolicyWindowEvent(
            settlementId: 10,
            pressure: 64,
            mandateDeficit: 10,
            administrativeDrag: 12,
            clerkDrag: 7,
            backlogDrag: 6));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state,
            context,
            buffer.Events.ToList()));

        IDomainEvent[] emitted = buffer.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.PolicyImplemented)
            .ToArray();
        Assert.That(emitted.Length, Is.EqualTo(1),
            "A policy window should resolve into one Office-owned implementation outcome.");
        Assert.That(emitted[0].EntityKey, Is.EqualTo("10"));
        Assert.That(
            emitted[0].Metadata[DomainEventMetadataKeys.SourceEventType],
            Is.EqualTo(OfficeAndCareerEventNames.PolicyWindowOpened));
        Assert.That(
            emitted[0].Metadata[DomainEventMetadataKeys.PolicyImplementationOutcome],
            Is.EqualTo(DomainEventMetadataValues.PolicyImplementationDragged));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyImplementationScore], Is.EqualTo("20"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyImplementationDocketDrag], Is.EqualTo("63"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyImplementationClerkCapture], Is.EqualTo("38"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyImplementationLocalBuffer], Is.EqualTo("31"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.PolicyImplementationPaperCompliance], Is.EqualTo("37"));

        OfficeCareerState affected = state.People.Single(static p => p.PersonId == new PersonId(1));
        OfficeCareerState offScope = state.People.Single(static p => p.PersonId == new PersonId(2));
        Assert.That(affected.PetitionBacklog, Is.EqualTo(45));
        Assert.That(affected.AdministrativeTaskLoad, Is.EqualTo(56));
        Assert.That(affected.ClerkDependence, Is.EqualTo(38));
        Assert.That(affected.DemotionPressure, Is.EqualTo(2));
        Assert.That(offScope.PetitionBacklog, Is.EqualTo(10),
            "Off-scope jurisdictions must not receive Office-side implementation pressure.");
    }

    [Test]
    public void PolicyWindowOpened_HighClerkCapture_EmitsCapturedImplementation()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(MakeOfficial(
            personId: 1,
            settlementId: 10,
            authorityTier: 3,
            clerk: 80,
            backlog: 60,
            taskLoad: 50,
            leverage: 0,
            petition: 40));
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = BuildContext(module, state, buffer);
        buffer.Emit(MakePolicyWindowEvent(
            settlementId: 10,
            pressure: 70,
            mandateDeficit: 10,
            administrativeDrag: 0,
            clerkDrag: 20,
            backlogDrag: 0));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state,
            context,
            buffer.Events.ToList()));

        IDomainEvent implemented = buffer.Events.Single(static e =>
            e.EventType == OfficeAndCareerEventNames.PolicyImplemented);
        Assert.That(
            implemented.Metadata[DomainEventMetadataKeys.PolicyImplementationOutcome],
            Is.EqualTo(DomainEventMetadataValues.PolicyImplementationCaptured));
        Assert.That(implemented.Metadata[DomainEventMetadataKeys.PolicyImplementationClerkCapture], Is.EqualTo("94"));

        OfficeCareerState affected = state.People.Single();
        Assert.That(affected.ClerkDependence, Is.EqualTo(86));
        Assert.That(affected.PetitionBacklog, Is.EqualTo(67));
        Assert.That(affected.DemotionPressure, Is.EqualTo(4));
    }

    [Test]
    public void PolicyWindowOpened_PaperCompliance_EmitsPaperImplementation()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(MakeOfficial(
            personId: 1,
            settlementId: 10,
            authorityTier: 3,
            clerk: 50,
            backlog: 12,
            taskLoad: 10,
            leverage: 30,
            petition: 10,
            reputation: 50));
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = BuildContext(module, state, buffer);
        buffer.Emit(MakePolicyWindowEvent(
            settlementId: 10,
            pressure: 64,
            mandateDeficit: 10,
            administrativeDrag: 6,
            clerkDrag: 8,
            backlogDrag: 3));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state,
            context,
            buffer.Events.ToList()));

        IDomainEvent implemented = buffer.Events.Single(static e =>
            e.EventType == OfficeAndCareerEventNames.PolicyImplemented);
        Assert.That(
            implemented.Metadata[DomainEventMetadataKeys.PolicyImplementationOutcome],
            Is.EqualTo(DomainEventMetadataValues.PolicyImplementationPaperCompliance));
        Assert.That(implemented.Metadata[DomainEventMetadataKeys.PolicyImplementationScore], Is.EqualTo("59"));
    }

    [Test]
    public void PolicyWindowOpened_OffScopeSettlement_DoesNotMutateOtherJurisdiction()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(MakeOfficial(
            personId: 1,
            settlementId: 10,
            authorityTier: 3,
            clerk: 35,
            backlog: 40,
            taskLoad: 50));
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = BuildContext(module, state, buffer);
        buffer.Emit(MakePolicyWindowEvent(
            settlementId: 20,
            pressure: 64,
            mandateDeficit: 10,
            administrativeDrag: 12,
            clerkDrag: 7,
            backlogDrag: 6));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state,
            context,
            buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.PolicyImplemented));
        OfficeCareerState onlyOfficial = state.People.Single();
        Assert.That(onlyOfficial.PetitionBacklog, Is.EqualTo(40));
        Assert.That(onlyOfficial.AdministrativeTaskLoad, Is.EqualTo(50));
    }

    private static ModuleExecutionContext BuildContext(
        OfficeAndCareerModule module,
        OfficeAndCareerState state,
        DomainEventBuffer buffer)
    {
        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);
        return new ModuleExecutionContext(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());
    }

    private static DomainEventRecord MakePolicyWindowEvent(
        int settlementId,
        int pressure,
        int mandateDeficit,
        int administrativeDrag,
        int clerkDrag,
        int backlogDrag)
    {
        return new DomainEventRecord(
            KnownModuleKeys.OfficeAndCareer,
            OfficeAndCareerEventNames.PolicyWindowOpened,
            $"policy window for settlement {settlementId}",
            settlementId.ToString(),
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseCourt,
                [DomainEventMetadataKeys.SourceEventType] = WorldSettlementsEventNames.CourtAgendaPressureAccumulated,
                [DomainEventMetadataKeys.SettlementId] = settlementId.ToString(),
                [DomainEventMetadataKeys.PressureScore] = pressure.ToString(),
                [DomainEventMetadataKeys.PolicyWindowPressure] = pressure.ToString(),
                [DomainEventMetadataKeys.PolicyWindowMandateDeficit] = mandateDeficit.ToString(),
                [DomainEventMetadataKeys.PolicyWindowAdministrativeDrag] = administrativeDrag.ToString(),
                [DomainEventMetadataKeys.PolicyWindowClerkDrag] = clerkDrag.ToString(),
                [DomainEventMetadataKeys.PolicyWindowBacklogDrag] = backlogDrag.ToString(),
            });
    }

    private static OfficeCareerState MakeOfficial(
        int personId,
        int settlementId,
        int authorityTier,
        int clerk = 0,
        int backlog = 0,
        int taskLoad = 0,
        int leverage = 0,
        int petition = 0,
        int reputation = 40)
    {
        return new OfficeCareerState
        {
            PersonId = new PersonId(personId),
            ClanId = new ClanId(personId),
            SettlementId = new SettlementId(settlementId),
            DisplayName = $"Official{personId}",
            HasAppointment = true,
            OfficeTitle = "County magistrate",
            AuthorityTier = authorityTier,
            ClerkDependence = clerk,
            PetitionBacklog = backlog,
            AdministrativeTaskLoad = taskLoad,
            JurisdictionLeverage = leverage,
            PetitionPressure = petition,
            OfficeReputation = reputation,
        };
    }
}
