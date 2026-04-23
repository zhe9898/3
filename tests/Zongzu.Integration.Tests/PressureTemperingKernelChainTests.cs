using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class PressureTemperingKernelChainTests
{
    [Test]
    public void ExamPass_RealScheduler_DrainsIntoSocialMemoryPressureTempering()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Full);

        IReadOnlyList<IModuleRunner> modules =
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
        ];

        Dictionary<string, object> states = modules.ToDictionary(
            static module => module.ModuleKey,
            static module => module.CreateInitialState(),
            StringComparer.Ordinal);

        WorldSettlementsState worldState = (WorldSettlementsState)states[KnownModuleKeys.WorldSettlements];
        worldState.Settlements.Add(new SettlementStateData
        {
            Id = new SettlementId(1),
            Name = "Lanxi",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 62,
            Prosperity = 66,
        });

        FamilyCoreState familyState = (FamilyCoreState)states[KnownModuleKeys.FamilyCore];
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 52,
            SupportReserve = 75,
            HeirPersonId = new PersonId(1),
            BranchTension = 18,
            InheritancePressure = 12,
            SeparationPressure = 10,
            MediationMomentum = 55,
            HeirSecurity = 70,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 20 * 12,
            IsAlive = true,
            Ambition = 72,
            Prudence = 58,
            Loyalty = 66,
            Sociability = 62,
        });

        PopulationAndHouseholdsState populationState = (PopulationAndHouseholdsState)states[KnownModuleKeys.PopulationAndHouseholds];
        populationState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "Zhang household",
            SettlementId = new SettlementId(1),
            SponsorClanId = new ClanId(1),
            Distress = 28,
            DebtPressure = 22,
            LaborCapacity = 80,
            MigrationRisk = 12,
        });
        populationState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 28,
            LaborSupply = 100,
            MigrationPressure = 12,
            MilitiaPotential = 40,
        });

        SocialMemoryAndRelationsState socialState = (SocialMemoryAndRelationsState)states[KnownModuleKeys.SocialMemoryAndRelations];
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            GrudgePressure = 10,
            FearPressure = 8,
            ShamePressure = 6,
            FavorBalance = 14,
        });

        EducationAndExamsState educationState = (EducationAndExamsState)states[KnownModuleKeys.EducationAndExams];
        educationState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(1),
            AcademyName = "Lanxi county school",
            IsOpen = true,
            Capacity = 10,
            Prestige = 70,
        });
        educationState.People.Add(new EducationPersonState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            AcademyId = new InstitutionId(1),
            DisplayName = "Zhang Yuan",
            IsStudying = true,
            StudyProgress = 90,
            Stress = 10,
            ExamAttempts = 0,
            CurrentTier = ExamTier.CountyExam,
        });

        KernelState kernelState = KernelState.Create(2401);
        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 3),
            manifest,
            new DeterministicRandom(kernelState),
            states,
            modules,
            kernelState);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == EducationAndExamsEventNames.ExamPassed
                     && e.EntityKey == "1"),
            "Real scheduler must emit ExamPassed from EducationAndExams.");
        Assert.That(
            socialState.Memories,
            Has.Some.Matches<MemoryRecordState>(
                memory => memory.Type == MemoryType.Aspiration
                          && memory.Subtype == MemorySubtype.ExamHonor
                          && memory.SubjectClanId == new ClanId(1)),
            "Real scheduler event drain must let SocialMemory record exam aspiration memory in the same month.");

        PersonPressureTemperingState personTempering = socialState.PersonTemperings.Single(static entry => entry.PersonId == new PersonId(1));
        ClanEmotionalClimateState climate = socialState.ClanEmotionalClimates.Single(static entry => entry.ClanId == new ClanId(1));

        Assert.That(personTempering.Hope, Is.GreaterThanOrEqualTo(7));
        Assert.That(personTempering.Obligation, Is.GreaterThanOrEqualTo(2));
        Assert.That(climate.Hope, Is.GreaterThanOrEqualTo(7));
        Assert.That(climate.Trust, Is.GreaterThanOrEqualTo(3));
    }
}
