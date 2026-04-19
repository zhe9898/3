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
        Assert.That(examNotice.Title, Is.Not.Empty);
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

    [Test]
    public void RunMonth_WarfareAftermathNotice_PullsCrossModuleContextForHonorsBlameAndRelief()
    {
        NarrativeProjectionModule module = new();
        NarrativeProjectionState state = module.CreateInitialState();

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer domainEvents = new();
        domainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.WarfareCampaign,
            WarfareCampaignEventNames.CampaignAftermathRegistered,
            "Lanxi campaign moved into aftermath review.",
            "1"));

        WorldDiff diff = new();
        diff.Record(KnownModuleKeys.WarfareCampaign, "Lanxi campaign aftermath is now under formal review.", "1");
        diff.Record(KnownModuleKeys.FamilyCore, "Campaign spillover opened clan merit claims and prestige review in Lanxi.", "1");
        diff.Record(KnownModuleKeys.OfficeAndCareer, "Campaign spillover pushed censure memorials and relief petitions into the Lanxi docket.", "1");
        diff.Record(KnownModuleKeys.PopulationAndHouseholds, "Campaign spillover raised household distress and aftercare pressure in Lanxi.", "1");

        ModuleExecutionContext context = new(
            new GameDate(1200, 5),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(91)),
            queries,
            domainEvents,
            diff,
            KernelState.Create(91));

        module.RunMonth(new ModuleExecutionScope<NarrativeProjectionState>(state, context));

        NarrativeNotificationState aftermathNotice = state.Notifications.Single(notification =>
            notification.SourceModuleKey == KnownModuleKeys.WarfareCampaign);

        Assert.That(aftermathNotice.Title, Is.Not.Empty);
        Assert.That(aftermathNotice.Tier, Is.EqualTo(NotificationTier.Consequential));
        Assert.That(aftermathNotice.WhyItHappened, Does.Contain("merit claims"));
        Assert.That(aftermathNotice.WhyItHappened, Does.Contain("censure memorials"));
        Assert.That(aftermathNotice.WhatNext, Is.Not.Empty);
        Assert.That(aftermathNotice.Traces.Any(static trace => trace.SourceModuleKey == KnownModuleKeys.FamilyCore), Is.True);
        Assert.That(aftermathNotice.Traces.Any(static trace => trace.SourceModuleKey == KnownModuleKeys.OfficeAndCareer), Is.True);
        Assert.That(aftermathNotice.Traces.Any(static trace => trace.SourceModuleKey == KnownModuleKeys.PopulationAndHouseholds), Is.True);
    }

    [Test]
    public void RunMonth_FamilyConflictEvent_ProjectAncestralHallGuidance()
    {
        NarrativeProjectionModule module = new();
        NarrativeProjectionState state = module.CreateInitialState();

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer domainEvents = new();
        domainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.FamilyCore,
            FamilyCoreEventNames.LineageDisputeHardened,
            "Clan Zhang ancestral-hall dispute hardened.",
            "1"));

        WorldDiff diff = new();
        diff.Record(KnownModuleKeys.FamilyCore, "Clan Zhang shifted to branch tension 62 and separation pressure 57.", "1");
        diff.Record(KnownModuleKeys.SocialMemoryAndRelations, "Clan Zhang ancestral-hall quarrel deepened and old resentment rose with it.", "1");

        ModuleExecutionContext context = new(
            new GameDate(1200, 7),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(119)),
            queries,
            domainEvents,
            diff,
            KernelState.Create(119));

        module.RunMonth(new ModuleExecutionScope<NarrativeProjectionState>(state, context));

        NarrativeNotificationState notice = state.Notifications.First(
            static notification => notification.Surface == NarrativeSurface.AncestralHall);
        Assert.That(notice.Surface, Is.EqualTo(NarrativeSurface.AncestralHall));
        Assert.That(notice.Title, Is.Not.Empty);
        Assert.That(notice.WhatNext, Is.Not.Empty);
        Assert.That(notice.WhyItHappened, Does.Contain("branch tension"));
    }

    [Test]
    public void RunMonth_FamilyLifecycleEvent_ProjectsDedicatedTitleAndGuidance()
    {
        NarrativeProjectionModule module = new();
        NarrativeProjectionState state = module.CreateInitialState();

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer domainEvents = new();
        domainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.FamilyCore,
            FamilyCoreEventNames.MarriageAllianceArranged,
            "Clan Zhang arranged a marriage alliance.",
            "1"));

        WorldDiff diff = new();
        diff.Record(KnownModuleKeys.FamilyCore, "Clan Zhang settled a marriage tie to ease heir pressure and branch unease.", "1");
        diff.Record(KnownModuleKeys.SocialMemoryAndRelations, "Clan Zhang's affinal talk briefly softened ancestral-hall resentment.", "1");

        ModuleExecutionContext context = new(
            new GameDate(1200, 8),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(151)),
            queries,
            domainEvents,
            diff,
            KernelState.Create(151));

        module.RunMonth(new ModuleExecutionScope<NarrativeProjectionState>(state, context));

        NarrativeNotificationState notice = state.Notifications.Single(static notification =>
            notification.SourceModuleKey == KnownModuleKeys.FamilyCore
            && notification.Title == "门内议亲");

        Assert.That(notice.Surface, Is.EqualTo(NarrativeSurface.AncestralHall));
        Assert.That(notice.Title, Is.EqualTo("门内议亲"));
        Assert.That(notice.WhatNext, Does.Contain("承祧").And.Contain("聘财"));
        Assert.That(notice.WhyItHappened, Does.Contain("marriage tie"));
    }

    [Test]
    public void RunMonth_BirthLifecycleEvent_ProjectsInfantCareGuidance()
    {
        NarrativeProjectionModule module = new();
        NarrativeProjectionState state = module.CreateInitialState();

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer domainEvents = new();
        domainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.FamilyCore,
            FamilyCoreEventNames.BirthRegistered,
            "Clan Zhang registered a newborn.",
            "1"));

        WorldDiff diff = new();
        diff.Record(KnownModuleKeys.FamilyCore, "Clan Zhang门内添丁，香火暂缓焦心。", "1");
        diff.Record(KnownModuleKeys.PopulationAndHouseholds, "张家襁褓初定，口粮与抚养之费一并上肩。", "1");

        ModuleExecutionContext context = new(
            new GameDate(1200, 9),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(173)),
            queries,
            domainEvents,
            diff,
            KernelState.Create(173));

        module.RunMonth(new ModuleExecutionScope<NarrativeProjectionState>(state, context));

        NarrativeNotificationState notice = state.Notifications.Single(static notification =>
            notification.SourceModuleKey == KnownModuleKeys.FamilyCore
            && notification.Title == "门内添丁");

        Assert.That(notice.Surface, Is.EqualTo(NarrativeSurface.AncestralHall));
        Assert.That(notice.WhatNext, Does.Contain("襁褓").And.Contain("口粮"));
        Assert.That(notice.WhatNext, Does.Contain("承祧"));
        Assert.That(notice.WhyItHappened, Does.Contain("襁褓"));
    }

    [Test]
    public void RunMonth_DeathLifecycleEvent_ProjectsMourningAndSuccessionGuidance()
    {
        NarrativeProjectionModule module = new();
        NarrativeProjectionState state = module.CreateInitialState();

        QueryRegistry queries = new();
        module.RegisterQueries(state, queries);

        DomainEventBuffer domainEvents = new();
        domainEvents.Emit(new DomainEventRecord(
            KnownModuleKeys.FamilyCore,
            FamilyCoreEventNames.DeathRegistered,
            "Clan Zhang entered mourning.",
            "1"));

        WorldDiff diff = new();
        diff.Record(KnownModuleKeys.FamilyCore, "张氏门内举哀，承祧与房支后议一时俱紧。", "1");
        diff.Record(KnownModuleKeys.SocialMemoryAndRelations, "丧次未定，诸房都在看谁先开口争后议。", "1");

        ModuleExecutionContext context = new(
            new GameDate(1200, 10),
            new FeatureManifest(),
            new DeterministicRandom(KernelState.Create(181)),
            queries,
            domainEvents,
            diff,
            KernelState.Create(181));

        module.RunMonth(new ModuleExecutionScope<NarrativeProjectionState>(state, context));

        NarrativeNotificationState notice = state.Notifications.Single(static notification =>
            notification.SourceModuleKey == KnownModuleKeys.FamilyCore
            && notification.Title == "门内举哀");

        Assert.That(notice.Surface, Is.EqualTo(NarrativeSurface.AncestralHall));
        Assert.That(notice.WhatNext, Does.Contain("丧次").And.Contain("承祧"));
        Assert.That(notice.WhatNext, Does.Contain("谱内名分").Or.Contain("继嗣"));
        Assert.That(notice.WhyItHappened, Does.Contain("后议"));
    }
}
