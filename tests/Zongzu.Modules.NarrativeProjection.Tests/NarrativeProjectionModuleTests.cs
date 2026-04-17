using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;

namespace Zongzu.Modules.NarrativeProjection.Tests;

[TestFixture]
public sealed class NarrativeProjectionModuleTests
{
    [Test]
    public void RunMonth_CreatesTraceableNotificationsFromEventsAndDiffs()
    {
        NarrativeProjectionModule module = new();
        NarrativeProjectionState state = module.CreateInitialState();

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer domainEvents = new();
        domainEvents.Emit(new DomainEventRecord(KnownModuleKeys.EducationAndExams, "ExamPassed", "Scholar Zhang Yuan passed the local exam."));
        domainEvents.Emit(new DomainEventRecord(KnownModuleKeys.TradeAndIndustry, "TradeProspered", "Clan Zhang trade prospered."));

        WorldDiff diff = new();
        diff.Record(KnownModuleKeys.EducationAndExams, "Scholar Zhang Yuan passed with strong study progress and tutor support.", "1");
        diff.Record(KnownModuleKeys.TradeAndIndustry, "Clan trade margin improved from demand and route stability.", "1");

        ModuleExecutionContext context = new(
            new GameDate(1200, 3),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(31)),
            queries,
            domainEvents,
            diff,
            KernelState.Create(31));

        module.RunMonth(new ModuleExecutionScope<NarrativeProjectionState>(state, context));

        Assert.That(state.Notifications, Has.Count.EqualTo(2));
        Assert.That(state.Notifications.All(static notification => notification.Traces.Count > 0), Is.True);

        NarrativeNotificationState examNotice = state.Notifications.Single(notification => notification.SourceModuleKey == KnownModuleKeys.EducationAndExams);
        Assert.That(examNotice.Title, Is.EqualTo("Exam Success"));
        Assert.That(examNotice.Tier, Is.EqualTo(NotificationTier.Consequential));
        Assert.That(examNotice.WhyItHappened, Does.Contain("study progress"));
        Assert.That(examNotice.Traces[0].DiffDescription, Does.Contain("tutor support"));
    }

    [Test]
    public void RunMonth_TrimsNotificationHistoryToRetentionLimit()
    {
        NarrativeProjectionModule module = new();
        NarrativeProjectionState state = module.CreateInitialState();
        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        KernelState kernelState = KernelState.Create(77);

        for (int month = 0; month < NarrativeProjectionModule.NotificationRetentionLimit + 6; month += 1)
        {
            DomainEventBuffer domainEvents = new();
            domainEvents.Emit(new DomainEventRecord(KnownModuleKeys.TradeAndIndustry, "TradeProspered", $"Trade prospered in month {month}."));

            WorldDiff diff = new();
            diff.Record(KnownModuleKeys.TradeAndIndustry, $"Margin improved in month {month}.", month.ToString());

            ModuleExecutionContext context = new(
                new GameDate(1200 + (month / 12), (month % 12) + 1),
                new FeatureManifest(),
                new DeterministicRandom(kernelState),
                queries,
                domainEvents,
                diff,
                kernelState);

            module.RunMonth(new ModuleExecutionScope<NarrativeProjectionState>(state, context));
        }

        Assert.That(state.Notifications, Has.Count.EqualTo(NarrativeProjectionModule.NotificationRetentionLimit));
        Assert.That(state.Notifications[0].Id.Value, Is.EqualTo(7));
        Assert.That(state.Notifications[^1].Id.Value, Is.EqualTo(NarrativeProjectionModule.NotificationRetentionLimit + 6));
    }
}
