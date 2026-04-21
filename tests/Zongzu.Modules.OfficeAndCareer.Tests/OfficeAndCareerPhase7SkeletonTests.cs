using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

[TestFixture]
public sealed class OfficeAndCareerPhase7SkeletonTests
{
    // Phase 7 衙门骨骼 — LIVING_WORLD_DESIGN §2.7
    [Test]
    public void BuildOfficialPostsAndWaitingList_SplitsHoldersAndCandidates()
    {
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            IsEligible = true,
            HasAppointment = true,
            OfficeTitle = "县尉",
            AuthorityTier = 2,
            PetitionBacklog = 14,
            ClerkDependence = 22,
            DemotionPressure = 18,
            PetitionPressure = 30,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Li Mao",
            IsEligible = true,
            HasAppointment = false,
            PromotionMomentum = 12,
        });

        OfficeAndCareerStateProjection.BuildOfficialPostsAndWaitingList(state);

        OfficialPostState post = state.OfficialPosts.Single();
        Assert.That(post.CurrentHolder, Is.EqualTo(new PersonId(1)));
        Assert.That(post.VacancyMonths, Is.EqualTo(0));
        Assert.That(post.Rank, Is.EqualTo(2));
        Assert.That(post.PostTitle, Is.EqualTo("县尉"));
        Assert.That(post.EvaluationPressure, Is.GreaterThan(0));

        WaitingListEntryState waiter = state.WaitingList.Single();
        Assert.That(waiter.PersonId, Is.EqualTo(new PersonId(2)));
        Assert.That(waiter.WaitingMonths, Is.EqualTo(1));
        Assert.That(waiter.PatronageSupport, Is.EqualTo(12));
    }

    [Test]
    public void BuildOfficialPostsAndWaitingList_OnVacation_AccruesVacancyAndWaitingMonths()
    {
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Zhang Heng",
            IsEligible = true,
            HasAppointment = true,
            OfficeTitle = "县尉",
            AuthorityTier = 2,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            SettlementId = new SettlementId(10),
            DisplayName = "Li Mao",
            IsEligible = true,
            HasAppointment = false,
        });

        OfficeAndCareerStateProjection.BuildOfficialPostsAndWaitingList(state);
        string originalPostId = state.OfficialPosts.Single().PostId;

        // 第二月：在任者去职 → 缺空。
        state.People[0].HasAppointment = false;
        state.People[0].IsEligible = false;

        OfficeAndCareerStateProjection.BuildOfficialPostsAndWaitingList(state);

        OfficialPostState vacant = state.OfficialPosts.Single(post => post.PostId == originalPostId);
        Assert.That(vacant.CurrentHolder, Is.Null);
        Assert.That(vacant.VacancyMonths, Is.EqualTo(1));

        WaitingListEntryState waiter = state.WaitingList.Single();
        Assert.That(waiter.WaitingMonths, Is.EqualTo(2));
    }
}
