using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

public sealed partial class OfficeAndCareerModuleTests
{
    [Test]
    public void HandleEvents_HouseholdDebtSpiked_UsesSettlementMetadataForYamenScope()
    {
        OfficeAndCareerModule module = new();
        OfficeAndCareerState state = module.CreateInitialState();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(10),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Lanxi official",
            HasAppointment = true,
            OfficeTitle = "县令",
            PetitionBacklog = 52,
            AdministrativeTaskLoad = 30,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(20),
            ClanId = new ClanId(2),
            SettlementId = new SettlementId(20),
            DisplayName = "North Ford official",
            HasAppointment = true,
            OfficeTitle = "县丞",
            PetitionBacklog = 52,
            AdministrativeTaskLoad = 30,
        });

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            new QueryRegistry(),
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.PopulationAndHouseholds,
            PopulationEventNames.HouseholdDebtSpiked,
            "Household debt crossed the tax threshold.",
            "101",
            new Dictionary<string, string>
            {
                [DomainEventMetadataKeys.Cause] = DomainEventMetadataValues.CauseTaxSeason,
                [DomainEventMetadataKeys.SettlementId] = "20",
            }));

        module.HandleEvents(new ModuleEventHandlingScope<OfficeAndCareerState>(
            state, context, buffer.Events.ToList()));

        OfficeCareerState lanxi = state.People.Single(static p => p.PersonId == new PersonId(10));
        OfficeCareerState northFord = state.People.Single(static p => p.PersonId == new PersonId(20));
        IDomainEvent yamenEvent = buffer.Events.Single(static e =>
            e.EventType == OfficeAndCareerEventNames.YamenOverloaded);

        Assert.That(lanxi.PetitionBacklog, Is.EqualTo(52),
            "Debt pressure for another settlement must not mutate this official.");
        Assert.That(northFord.PetitionBacklog, Is.EqualTo(60));
        Assert.That(yamenEvent.EntityKey, Is.EqualTo("20"));
        Assert.That(yamenEvent.Metadata[DomainEventMetadataKeys.SettlementId], Is.EqualTo("20"));
        Assert.That(yamenEvent.Metadata[DomainEventMetadataKeys.PersonId], Is.EqualTo("20"));
        Assert.That(yamenEvent.Metadata[DomainEventMetadataKeys.SourceEventType], Is.EqualTo(PopulationEventNames.HouseholdDebtSpiked));
        Assert.That(yamenEvent.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseTaxSeason));
    }
}
