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

/// <summary>
/// Chain 3 thin slice: ExamPassed → ClanPrestigeAdjusted scheduler end-to-end tests.
/// </summary>
[TestFixture]
public sealed class ExamPrestigeChainTests
{
    [Test]
    public void ExamPass_ThinChain_RealScheduler_DrainsIntoClanPrestige()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);

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
            Name = "兰溪",
            Tier = SettlementTier.CountySeat,
            NodeKind = SettlementNodeKind.CountySeat,
            Visibility = NodeVisibility.StateVisible,
            EcoZone = SettlementEcoZone.JiangnanWaterNetwork,
            Security = 60,
            Prosperity = 60,
        });

        FamilyCoreState familyState = (FamilyCoreState)states[KnownModuleKeys.FamilyCore];
        familyState.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "张家",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            SupportReserve = 60,
            MarriageAllianceValue = 20,
        });
        familyState.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "张远",
            AgeMonths = 20 * 12,
            IsAlive = true,
        });

        PopulationAndHouseholdsState popState = (PopulationAndHouseholdsState)states[KnownModuleKeys.PopulationAndHouseholds];
        popState.Households.Add(new PopulationHouseholdState
        {
            Id = new HouseholdId(1),
            HouseholdName = "张家",
            SettlementId = new SettlementId(1),
            Distress = 40,
            DebtPressure = 30,
            LaborCapacity = 80,
            MigrationRisk = 20,
        });
        popState.Settlements.Add(new PopulationSettlementState
        {
            SettlementId = new SettlementId(1),
            CommonerDistress = 40,
            LaborSupply = 100,
            MigrationPressure = 20,
            MilitiaPotential = 60,
        });

        SocialMemoryAndRelationsState socialState = (SocialMemoryAndRelationsState)states[KnownModuleKeys.SocialMemoryAndRelations];
        socialState.ClanNarratives.Add(new ClanNarrativeState
        {
            ClanId = new ClanId(1),
            GrudgePressure = 20,
            FearPressure = 15,
            ShamePressure = 10,
            FavorBalance = 10,
        });

        EducationAndExamsState eduState = (EducationAndExamsState)states[KnownModuleKeys.EducationAndExams];
        eduState.Academies.Add(new AcademyState
        {
            Id = new InstitutionId(1),
            SettlementId = new SettlementId(1),
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

        MonthlyScheduler scheduler = new();
        SimulationMonthResult result = scheduler.AdvanceOneMonth(
            new GameDate(1022, 3), // Month 3 is exam window
            manifest,
            new DeterministicRandom(KernelState.Create(42)),
            states,
            modules);

        IDomainEvent[] events = result.DomainEvents.ToArray();
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == EducationAndExamsEventNames.ExamPassed
                     && e.EntityKey == "1"),
            "Real scheduler must emit ExamPassed for the student in month 3.");
        Assert.That(
            events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == FamilyCoreEventNames.ClanPrestigeAdjusted
                     && e.EntityKey == "1"),
            "Real scheduler must drain ExamPassed into ClanPrestigeAdjusted.");

        ClanStateData clan = familyState.Clans.Single(static c => c.Id == new ClanId(1));
        Assert.That(clan.Prestige, Is.EqualTo(55),
            "Clan prestige must rise by 5 after real scheduler drain.");
        Assert.That(clan.MarriageAllianceValue, Is.EqualTo(23),
            "Clan marriage alliance value must rise by 3.");
    }
}
