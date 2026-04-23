using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Presentation.Unity;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class ProjectionSelectorAlignmentTests
{
    [Test]
    public void PreviewScenario_SelectedFamilyLifecycleAffordance_aligns_with_family_hall_docket_suggestion()
    {
        MvpFamilyLifecyclePreviewResult preview = new MvpFamilyLifecyclePreviewScenario().Build(20260419, 2);
        HallDocketItemSnapshot familyDocket = GetFamilyHallDocketItem(preview.BeforeBundle);

        Assert.That(preview.SelectedAffordance.CommandName, Is.EqualTo(familyDocket.SuggestedCommandName));
        Assert.That(preview.SelectedAffordance.Label, Is.EqualTo(familyDocket.SuggestedCommandLabel));
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

    private static HallDocketItemSnapshot GetFamilyHallDocketItem(PresentationReadModelBundle bundle)
    {
        return TryGetFamilyHallDocketItem(bundle)
            ?? throw new AssertionException("Expected a family hall-docket item in the preview bundle.");
    }

    private static HallDocketItemSnapshot? TryGetFamilyHallDocketItem(PresentationReadModelBundle bundle)
    {
        if (string.Equals(bundle.HallDocket.LeadItem.LaneKey, HallDocketLaneKeys.Family, StringComparison.Ordinal))
        {
            return bundle.HallDocket.LeadItem;
        }

        return bundle.HallDocket.SecondaryItems.FirstOrDefault(item =>
            string.Equals(item.LaneKey, HallDocketLaneKeys.Family, StringComparison.Ordinal));
    }
}
