using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

[TestFixture]
public sealed class FrontierSupplyHandlerTests
{
    [Test]
    public void PublishedEvents_DeclareOfficialSupplyRequisition()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.PublishedEvents,
            Does.Contain(OfficeAndCareerEventNames.OfficialSupplyRequisition),
            "OfficeAndCareer emits OfficialSupplyRequisition, so the event contract must declare it.");
    }

    [Test]
    public void ConsumedEvents_DeclareFrontierStrainEscalated()
    {
        OfficeAndCareerModule module = new();
        Assert.That(
            module.ConsumedEvents,
            Does.Contain(WorldSettlementsEventNames.FrontierStrainEscalated),
            "OfficeAndCareer consumes FrontierStrainEscalated, so the event contract must declare it.");
    }

    [Test]
    public void FrontierStrainEscalated_EmitsOfficialSupplyRequisition_ForMatchingJurisdictionOnly()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "County Magistrate",
            AuthorityTier = 2,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(2),
            SettlementId = new SettlementId(20),
            DisplayName = "Li Yue",
            HasAppointment = true,
            OfficeTitle = "County Clerk",
            AuthorityTier = 1,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.FrontierStrainEscalated,
            "frontier pressure for settlement 10",
            "10"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        IDomainEvent[] emitted = buffer.Events
            .Where(static e => e.EventType == OfficeAndCareerEventNames.OfficialSupplyRequisition)
            .ToArray();

        Assert.That(emitted.Length, Is.EqualTo(1),
            "Only the matching jurisdiction should receive OfficialSupplyRequisition.");
        Assert.That(emitted[0].EntityKey, Is.EqualTo("10"),
            "EntityKey must match the scoped frontier settlement id.");
        Assert.That(
            emitted[0].Metadata[DomainEventMetadataKeys.SourceEventType],
            Is.EqualTo(WorldSettlementsEventNames.FrontierStrainEscalated));
    }

    [Test]
    public void FrontierStrainEscalated_OffScopeSettlement_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            HasAppointment = true,
            OfficeTitle = "County Magistrate",
            AuthorityTier = 2,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.FrontierStrainEscalated,
            "frontier pressure for settlement 20",
            "20"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.OfficialSupplyRequisition),
            "A frontier event for another settlement must not fan out to unrelated jurisdictions.");
    }

    [Test]
    public void FrontierStrainEscalated_NoJurisdiction_DoesNotEmit()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = new();

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.FrontierStrainEscalated,
            "frontier pressure for settlement 10",
            "10"));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.OfficialSupplyRequisition),
            "No jurisdiction means no supply requisition.");
    }
}
