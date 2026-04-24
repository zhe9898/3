using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

/// <summary>
/// Chain 4 thin slice: ImperialRhythmChanged → AmnestyApplied → DisorderSpike
/// scheduler end-to-end tests.
/// </summary>
[TestFixture]
public sealed class ImperialAmnestyDisorderChainTests
{
    private const long ChainSeed = 20250604L;

    [Test]
    public void ImperialAmnesty_ThinChain_RealScheduler_DrainsIntoDisorderSpike()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);

        IReadOnlyList<IModuleRunner> modules =
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new OfficeAndCareerModule(),
            new OrderAndBanditryModule(),
        ];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        // Seed WorldSettlements (Lanxi) and extract county seat id.
        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        LanxiSeed.Seed(worldState, KernelState.Create(ChainSeed));
        SettlementId countyId = worldState.Settlements
            .Single(static s => s.NodeKind == SettlementNodeKind.CountySeat).Id;

        // Seed PopulationAndHouseholds.
        // OrderAndBanditry.RunXun queries population for every settlement in WorldSettlements,
        // and RebuildSettlementSummaries rebuilds from households. Every settlement needs at
        // least one household so the summary exists.
        PopulationAndHouseholdsState popState = (PopulationAndHouseholdsState)states[KnownModuleKeys.PopulationAndHouseholds];
        foreach (SettlementStateData worldSettlement in worldState.Settlements)
        {
            popState.Households.Add(new PopulationHouseholdState
            {
                Id = new HouseholdId(worldSettlement.Id.Value),
                HouseholdName = $"Household{worldSettlement.Id.Value}",
                SettlementId = worldSettlement.Id,
                Distress = 40,
                DebtPressure = 30,
                LaborCapacity = 80,
                MigrationRisk = 20,
            });
        }

        // Seed FamilyCore
        FamilyCoreState familyState = (FamilyCoreState)states[KnownModuleKeys.FamilyCore];
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "张家",
            HomeSettlementId = countyId,
            Prestige = 50,
            SupportReserve = 60,
            MarriageAllianceValue = 20,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "张远",
            AgeMonths = 24 * 12,
            IsAlive = true,
        });

        // Seed SocialMemoryAndRelations
        SocialMemoryAndRelationsState socialState = (SocialMemoryAndRelationsState)states[KnownModuleKeys.SocialMemoryAndRelations];
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            GrudgePressure = 20,
            FearPressure = 15,
            ShamePressure = 10,
            FavorBalance = 10,
        });

        // Seed EducationAndExams
        EducationAndExamsState eduState = (EducationAndExamsState)states[KnownModuleKeys.EducationAndExams];
        eduState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = countyId,
            AcademyName = "兰溪县学",
            IsOpen = true,
            Capacity = 10,
            Prestige = 50,
        });
        eduState.People.Add(new EducationPersonState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            AcademyId = new InstitutionId(1),
            DisplayName = "张远",
            IsStudying = true,
            StudyProgress = 80,
            Stress = 30,
            ExamAttempts = 0,
            CurrentTier = ExamTier.CountyExam,
        });

        // Seed OfficeAndCareer: one appointed official at the county seat.
        OfficeAndCareerState officeState = (OfficeAndCareerState)states[KnownModuleKeys.OfficeAndCareer];
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(1),
            SettlementId = countyId,
            DisplayName = "张县尉",
            HasAppointment = true,
            OfficeTitle = "县尉",
            AuthorityTier = 2,
            PetitionPressure = 20,
            JurisdictionLeverage = 30,
            PetitionBacklog = 60,
            ClerkDependence = 50,
            AdministrativeTaskLoad = 40,
        });
        officeState.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(officeState.People);

        // Seed OrderAndBanditry: one settlement disorder record at the county seat.
        // Real scheduler tests include xun/month drift before HandleEvents, so leave
        // enough headroom for the amnesty pulse to prove the threshold crossing.
        OrderAndBanditryState orderState = (OrderAndBanditryState)states[KnownModuleKeys.OrderAndBanditry];
        orderState.Settlements.Add(new SettlementDisorderState
        {
            SettlementId = countyId,
            BanditThreat = 35,
            RoutePressure = 24,
            SuppressionDemand = 19,
            DisorderPressure = 48,
            BlackRoutePressure = 34,
            CoercionRisk = 25,
        });

        // Inject grand amnesty before the tick.
        worldState.CurrentSeason.Imperial.AmnestyWave = 80;
        // Ensure PreviousAnnouncedImperial is low so the crossing is detected.
        worldState.CurrentSeason.PreviousAnnouncedImperial = new ImperialBandData();

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1200, 3),
            manifest,
            new DeterministicRandom(KernelState.Create(ChainSeed)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();

        // Diagnostic: print all events for debugging
        foreach (IDomainEvent evt in events)
        {
            Console.WriteLine($"EVENT: {evt.EventType} | EntityKey={evt.EntityKey}");
        }

        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == WorldSettlementsEventNames.ImperialRhythmChanged
                     && e.EntityKey == "imperial"),
            "Real scheduler must emit ImperialRhythmChanged after amnesty injection.");

        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OfficeAndCareerEventNames.AmnestyApplied
                     && e.EntityKey == countyId.Value.ToString()),
            "Real scheduler must drain ImperialRhythmChanged into AmnestyApplied.");

        // State assertion: disorder must end above the public spike threshold under
        // the real scheduler, after normal xun/month drift and the amnesty pulse.
        SettlementDisorderState targetSettlement = orderState.Settlements.Single(
            s => s.SettlementId == countyId);
        Console.WriteLine($"DISORDER: initial=48 final={targetSettlement.DisorderPressure}");

        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == OrderAndBanditryEventNames.DisorderSpike
                     && e.EntityKey == countyId.Value.ToString()),
            "Real scheduler must drain AmnestyApplied into DisorderSpike when threshold crossed.");
        IDomainEvent spike = events.Single(e =>
            e.EventType == OrderAndBanditryEventNames.DisorderSpike
            && e.EntityKey == countyId.Value.ToString());
        Assert.That(spike.Metadata[DomainEventMetadataKeys.Cause], Is.EqualTo(DomainEventMetadataValues.CauseAmnesty));
        Assert.That(int.Parse(spike.Metadata[DomainEventMetadataKeys.AmnestyWave]), Is.GreaterThanOrEqualTo(50));
        Assert.That(spike.Metadata[DomainEventMetadataKeys.DisorderDelta], Is.Not.Empty);

        Assert.That(targetSettlement.DisorderPressure, Is.GreaterThanOrEqualTo(50),
            "Target settlement disorder must cross the public spike threshold after real scheduler drain.");
    }
}
