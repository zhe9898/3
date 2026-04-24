using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class ProjectionSelectorAlignmentTests
{
    [Test]
    public void HallDocketStackSnapshot_TryGetLaneItem_ignores_placeholder_items()
    {
        HallDocketStackSnapshot stack = new()
        {
            LeadItem = new HallDocketItemSnapshot
            {
                LaneKey = HallDocketLaneKeys.Family,
            },
            SecondaryItems =
            [
                new HallDocketItemSnapshot
                {
                    LaneKey = HallDocketLaneKeys.Family,
                },
                new HallDocketItemSnapshot
                {
                    LaneKey = HallDocketLaneKeys.Warfare,
                    Headline = "前线有急报",
                },
            ],
        };

        Assert.That(stack.HasLeadItem, Is.False);
        Assert.That(stack.HasLaneItem(HallDocketLaneKeys.Family), Is.False);
        Assert.That(stack.TryGetLaneItem(HallDocketLaneKeys.Warfare)?.Headline, Is.EqualTo("前线有急报"));
        Assert.That(stack.EnumeratePresentItems().Select(static item => item.LaneKey), Is.EquivalentTo(new[] { HallDocketLaneKeys.Warfare }));
    }

    [Test]
    public void PreviewScenario_SelectedFamilyLifecycleAffordance_aligns_with_family_hall_docket_suggestion()
    {
        MvpFamilyLifecyclePreviewResult preview = new MvpFamilyLifecyclePreviewScenario().Build(20260419, 2);
        HallDocketItemSnapshot familyDocket = GetFamilyHallDocketItem(preview.BeforeBundle);

        Assert.That(preview.SelectedAffordance.CommandName, Is.EqualTo(familyDocket.SuggestedCommandName));
        Assert.That(preview.SelectedAffordance.Label, Is.EqualTo(familyDocket.SuggestedCommandLabel));
    }

    [Test]
    public void SharedFamilyLifecycleSelector_matches_preview_affordance_selection()
    {
        MvpFamilyLifecyclePreviewResult preview = new MvpFamilyLifecyclePreviewScenario().Build(20260419, 2);

        PlayerCommandAffordanceSnapshot? affordance = FamilyLifecycleProjectionSelectors.SelectLeadLifecycleAffordance(
            preview.BeforeBundle.Clans,
            preview.BeforeBundle.PlayerCommands.Affordances);

        Assert.That(affordance?.CommandName, Is.EqualTo(preview.SelectedAffordance.CommandName));
        Assert.That(affordance?.Label, Is.EqualTo(preview.SelectedAffordance.Label));
    }

    [Test]
    public void PreviewScenario_AfterAdvanceFamilyGuidance_tracks_family_hall_docket_suggestion()
    {
        MvpFamilyLifecycleTenYearPreviewResult preview = new MvpFamilyLifecyclePreviewScenario().BuildTenYear(20260419, 3);

        foreach (MvpFamilyLifecycleTenYearCheckpoint checkpoint in preview.YearlyCheckpoints)
        {
            Assert.That(checkpoint.AfterAdvanceBundle.PublicLifeSettlements, Is.Empty);

            HallDocketItemSnapshot? familyDocket = TryGetFamilyHallDocketItem(checkpoint.AfterAdvanceBundle);
            if (familyDocket is null || string.IsNullOrWhiteSpace(familyDocket.SuggestedCommandLabel))
            {
                continue;
            }

            PresentationShellViewModel shell = FirstPassPresentationShell.Compose(checkpoint.AfterAdvanceBundle);

            Assert.That(shell.GreatHall.FamilySummary, Does.Contain(familyDocket.SuggestedCommandLabel));
            Assert.That(shell.FamilyCouncil.Summary, Does.Contain(familyDocket.SuggestedCommandLabel));

            FamilyConflictTileViewModel? leadClan = shell.FamilyCouncil.Clans.FirstOrDefault();
            if (leadClan is not null)
            {
                Assert.That(leadClan.LifecycleSummary, Does.Contain(familyDocket.SuggestedCommandLabel));
            }
        }
    }

    [Test]
    public void SelectPrimarySettlementNotification_respects_scope_module_and_priority()
    {
        SettlementId targetSettlementId = new(7);
        NarrativeNotificationSnapshot[] notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(1),
                CreatedAt = new GameDate(1200, 1),
                Tier = NotificationTier.Background,
                SourceModuleKey = KnownModuleKeys.PublicLifeAndRumor,
                Title = "public life",
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        EntityKey = targetSettlementId.Value.ToString(),
                    },
                ],
            },
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(2),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Urgent,
                SourceModuleKey = KnownModuleKeys.WarfareCampaign,
                Title = "other settlement",
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        EntityKey = new SettlementId(8).Value.ToString(),
                    },
                ],
            },
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(3),
                CreatedAt = new GameDate(1200, 3),
                Tier = NotificationTier.Consequential,
                SourceModuleKey = KnownModuleKeys.WarfareCampaign,
                Title = "front report",
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        EntityKey = targetSettlementId.Value.ToString(),
                    },
                ],
            },
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(4),
                CreatedAt = new GameDate(1200, 4),
                Tier = NotificationTier.Background,
                SourceModuleKey = KnownModuleKeys.WarfareCampaign,
                Title = "aftermath",
                Traces =
                [
                    new NotificationTraceSnapshot
                    {
                        EntityKey = targetSettlementId.Value.ToString(),
                    },
                ],
            },
        ];

        NarrativeNotificationSnapshot? warfare = PresentationReadModelBuilder.SelectPrimarySettlementNotification(
            notifications,
            targetSettlementId,
            static notification => string.Equals(notification.Title, "aftermath", StringComparison.Ordinal) ? 0 : 1,
            KnownModuleKeys.WarfareCampaign);
        NarrativeNotificationSnapshot? governance = PresentationReadModelBuilder.SelectPrimarySettlementNotification(
            notifications,
            targetSettlementId,
            static notification => string.Equals(notification.SourceModuleKey, KnownModuleKeys.PublicLifeAndRumor, StringComparison.Ordinal) ? 0 : 1);

        Assert.That(warfare?.Id, Is.EqualTo(new NotificationId(4)));
        Assert.That(governance?.Id, Is.EqualTo(new NotificationId(1)));
    }

    [Test]
    public void PlayerCommandSurfaceSnapshot_enumerators_filter_by_surface_and_settlement()
    {
        PlayerCommandSurfaceSnapshot surface = new()
        {
            Affordances =
            [
                new PlayerCommandAffordanceSnapshot
                {
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    IsEnabled = true,
                },
                new PlayerCommandAffordanceSnapshot
                {
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.DispatchRoadReport,
                    IsEnabled = false,
                },
                new PlayerCommandAffordanceSnapshot
                {
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.ArrangeMarriage,
                    IsEnabled = true,
                },
                new PlayerCommandAffordanceSnapshot
                {
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(2),
                    CommandName = PlayerCommandNames.FundLocalWatch,
                    IsEnabled = true,
                },
            ],
            Receipts =
            [
                new PlayerCommandReceiptSnapshot
                {
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.PostCountyNotice,
                },
                new PlayerCommandReceiptSnapshot
                {
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = new SettlementId(1),
                    CommandName = PlayerCommandNames.ArrangeMarriage,
                },
                new PlayerCommandReceiptSnapshot
                {
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = new SettlementId(2),
                    CommandName = PlayerCommandNames.FundLocalWatch,
                },
            ],
        };

        Assert.That(
            surface.EnumerateAffordances(PlayerCommandSurfaceKeys.PublicLife, new SettlementId(1)).Select(static command => command.CommandName),
            Is.EquivalentTo(new[] { PlayerCommandNames.PostCountyNotice, PlayerCommandNames.DispatchRoadReport }));
        Assert.That(
            surface.EnumerateAffordances(PlayerCommandSurfaceKeys.PublicLife, new SettlementId(1), enabledOnly: true).Select(static command => command.CommandName),
            Is.EquivalentTo(new[] { PlayerCommandNames.PostCountyNotice }));
        Assert.That(
            surface.EnumerateReceipts(PlayerCommandSurfaceKeys.PublicLife, new SettlementId(1)).Select(static receipt => receipt.CommandName),
            Is.EquivalentTo(new[] { PlayerCommandNames.PostCountyNotice }));
    }

    private static HallDocketItemSnapshot GetFamilyHallDocketItem(PresentationReadModelBundle bundle)
    {
        return bundle.HallDocket.TryGetLaneItem(HallDocketLaneKeys.Family)
            ?? throw new AssertionException("Expected a family hall-docket item in the preview bundle.");
    }

    private static HallDocketItemSnapshot? TryGetFamilyHallDocketItem(PresentationReadModelBundle bundle)
    {
        return bundle.HallDocket.TryGetLaneItem(HallDocketLaneKeys.Family);
    }
}
