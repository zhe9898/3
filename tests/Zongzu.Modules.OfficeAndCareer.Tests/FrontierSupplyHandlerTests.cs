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
    public void FrontierStrainEscalated_EmitsProfiledOfficialSupplyRequisition_ForMatchingJurisdictionOnly()
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
            JurisdictionLeverage = 40,
            ClerkDependence = 36,
            PetitionBacklog = 20,
            AdministrativeTaskLoad = 24,
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

        buffer.Emit(MakeFrontierEvent(new SettlementId(10)));

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
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("10"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.FrontierPressure], Is.EqualTo("76"));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.Severity], Is.EqualTo(DomainEventMetadataValues.SeverityFrontierSevere));
        Assert.That(int.Parse(emitted[0].Metadata[DomainEventMetadataKeys.OfficialSupplyPressure]), Is.GreaterThan(0));
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.OfficialSupplyQuotaPressure], Is.Not.Empty);
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.OfficialSupplyClerkDistortionPressure], Is.Not.Empty);
        Assert.That(emitted[0].Metadata[DomainEventMetadataKeys.OfficialSupplyAuthorityBuffer], Is.Not.Empty);

        OfficeCareerState affectedOfficial = state.People.Single(static p => p.PersonId == new PersonId(1));
        OfficeCareerState offScopeOfficial = state.People.Single(static p => p.PersonId == new PersonId(2));
        Assert.That(affectedOfficial.AdministrativeTaskLoad, Is.GreaterThan(24),
            "Supply requisition should leave office-owned task pressure behind.");
        Assert.That(affectedOfficial.PetitionBacklog, Is.GreaterThan(20),
            "Supply requisition should thicken the docket instead of only emitting a relay event.");
        Assert.That(offScopeOfficial.AdministrativeTaskLoad, Is.EqualTo(0),
            "Off-scope jurisdiction must not receive office-side pressure.");
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

        buffer.Emit(MakeFrontierEvent(new SettlementId(20)));

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

        buffer.Emit(MakeFrontierEvent(new SettlementId(10)));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        Assert.That(
            buffer.Events.Select(static e => e.EventType),
            Does.Not.Contain(OfficeAndCareerEventNames.OfficialSupplyRequisition),
            "No jurisdiction means no supply requisition.");
    }

    private static DomainEventRecord MakeFrontierEvent(SettlementId settlementId)
    {
        return new DomainEventRecord(
            KnownModuleKeys.WorldSettlements,
            WorldSettlementsEventNames.FrontierStrainEscalated,
            $"frontier pressure for settlement {settlementId.Value}",
            settlementId.Value.ToString(),
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.SettlementId] = settlementId.Value.ToString(),
                [DomainEventMetadataKeys.FrontierPressure] = "76",
                [DomainEventMetadataKeys.Severity] = DomainEventMetadataValues.SeverityFrontierSevere,
            });
    }
}
