using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore.Tests;

[TestFixture]
public sealed class ExamResultHandlerTests
{
    [Test]
    public void ExamPassed_RaisesClanPrestigeAndMarriageValue()
    {
        FamilyCoreModule module = new();
        FamilyCoreState state = new();
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            MarriageAllianceValue = 20,
        });
        state.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 24 * 12,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.EducationAndExams,
            EducationAndExamsEventNames.ExamPassed,
            "Zhang Yuan场屋得捷。",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Clans[0].Prestige, Is.EqualTo(55),
            "ExamPassed must raise clan prestige by 5.");
        Assert.That(state.Clans[0].MarriageAllianceValue, Is.EqualTo(23),
            "ExamPassed must raise marriage alliance value by 3.");
        Assert.That(
            buffer.Events,
            Has.Some.Matches<IDomainEvent>(
                e => e.EventType == FamilyCoreEventNames.ClanPrestigeAdjusted
                     && e.EntityKey == "1"),
            "FamilyCore must emit ClanPrestigeAdjusted after exam pass.");
    }

    [Test]
    public void ExamPassed_OffScopeClan_DoesNotChange()
    {
        // Off-scope negative assertion: ensures only the matched person's clan is affected.
        FamilyCoreModule module = new();
        FamilyCoreState state = new();
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            MarriageAllianceValue = 20,
        });
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(2),
            ClanName = "Li",
            HomeSettlementId = new SettlementId(2),
            Prestige = 40,
            MarriageAllianceValue = 15,
        });
        state.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "Zhang Yuan",
            AgeMonths = 24 * 12,
            IsAlive = true,
        });
        state.People.Add(new FamilyPersonState
        {
            Id = new PersonId(2),
            ClanId = new ClanId(2),
            GivenName = "Li Wei",
            AgeMonths = 24 * 12,
            IsAlive = true,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.EducationAndExams,
            EducationAndExamsEventNames.ExamPassed,
            "Zhang Yuan场屋得捷。",
            "1"));

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Clans[0].Prestige, Is.EqualTo(55),
            "Matched clan must receive prestige rise.");
        Assert.That(state.Clans[1].Prestige, Is.EqualTo(40),
            "Off-scope clan must remain unchanged (negative assertion).");
        Assert.That(state.Clans[1].MarriageAllianceValue, Is.EqualTo(15),
            "Off-scope clan marriage value must remain unchanged.");
    }

    [Test]
    public void ExamPassed_UnknownPerson_IsNoOp()
    {
        FamilyCoreModule module = new();
        FamilyCoreState state = new();
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "Zhang",
            HomeSettlementId = new SettlementId(1),
            Prestige = 50,
            MarriageAllianceValue = 20,
        });

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer buffer = new();
        ModuleExecutionContext context = new(
            new GameDate(1022, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(42)),
            queries,
            buffer,
            new WorldDiff());

        buffer.Emit(new DomainEventRecord(
            KnownModuleKeys.EducationAndExams,
            EducationAndExamsEventNames.ExamPassed,
            "Unknown person场屋得捷。",
            "99"));

        module.HandleEvents(new ModuleEventHandlingScope<FamilyCoreState>(
            state, context, buffer.Events.ToList()));

        Assert.That(state.Clans[0].Prestige, Is.EqualTo(50),
            "ExamPassed for unknown person must not mutate any clan.");
        Assert.That(buffer.Events.Count, Is.EqualTo(1),
            "No follow-on events should be emitted.");
    }
}
