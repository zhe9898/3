using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

[TestFixture]
public sealed class CourtPolicyLocalResponseCommandTests
{
    [Test]
    public void PressCountyYamenDocument_UsesCourtPolicyOfficeScalarsWithoutOrderResidue()
    {
        OfficeAndCareerState state = BuildState();
        OfficeCareerState affected = state.People.Single(static career => career.PersonId == new PersonId(1));
        int beforePressure = affected.PetitionPressure;
        int beforeBacklog = affected.PetitionBacklog;

        PlayerCommandResult result = OfficeAndCareerCommandResolver.IssueIntent(new OfficeAndCareerCommandContext
        {
            State = state,
            Command = new PlayerCommandRequest
            {
                SettlementId = new SettlementId(10),
                CommandName = PlayerCommandNames.PressCountyYamenDocument,
            },
        });

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
        Assert.That(result.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));
        Assert.That(result.Summary, Does.Contain("政策文移续接"));

        Assert.That(affected.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.PressCountyYamenDocument));
        Assert.That(affected.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Contained));
        Assert.That(affected.LastRefusalResponseTraceCode, Is.EqualTo(PublicLifeOrderResponseTraceCodes.OfficeYamenLanded));
        Assert.That(affected.LastRefusalResponseSummary, Does.Contain("不是本户硬扛朝廷后账"));
        Assert.That(affected.PetitionPressure, Is.LessThan(beforePressure));
        Assert.That(affected.PetitionBacklog, Is.LessThan(beforeBacklog));

        OfficeCareerState offScope = state.People.Single(static career => career.PersonId == new PersonId(2));
        Assert.That(offScope.LastRefusalResponseCommandCode, Is.Empty);
        Assert.That(offScope.PetitionPressure, Is.EqualTo(5));
    }

    [Test]
    public void RedirectRoadReport_UsesCourtPolicyOfficeScalarsWithoutOrderResidue()
    {
        OfficeAndCareerState state = BuildState();
        OfficeCareerState affected = state.People.Single(static career => career.PersonId == new PersonId(1));
        int beforePressure = affected.PetitionPressure;

        PlayerCommandResult result = OfficeAndCareerCommandResolver.IssueIntent(new OfficeAndCareerCommandContext
        {
            State = state,
            Command = new PlayerCommandRequest
            {
                SettlementId = new SettlementId(10),
                CommandName = PlayerCommandNames.RedirectRoadReport,
            },
        });

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
        Assert.That(result.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.PublicLife));
        Assert.That(result.Summary, Does.Contain("政策递报改道"));

        Assert.That(affected.LastRefusalResponseCommandCode, Is.EqualTo(PlayerCommandNames.RedirectRoadReport));
        Assert.That(affected.LastRefusalResponseOutcomeCode, Is.EqualTo(PublicLifeOrderResponseOutcomeCodes.Contained));
        Assert.That(affected.LastRefusalResponseTraceCode, Is.EqualTo(PublicLifeOrderResponseTraceCodes.OfficeReportRerouted));
        Assert.That(affected.LastRefusalResponseSummary, Does.Contain("不计算政策成败"));
        Assert.That(affected.PetitionPressure, Is.LessThan(beforePressure));

        OfficeCareerState offScope = state.People.Single(static career => career.PersonId == new PersonId(2));
        Assert.That(offScope.LastRefusalResponseCommandCode, Is.Empty);
        Assert.That(offScope.PetitionPressure, Is.EqualTo(5));
    }

    private static OfficeAndCareerState BuildState()
    {
        OfficeAndCareerState state = new();
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            ClanId = new ClanId(1),
            SettlementId = new SettlementId(10),
            DisplayName = "Official1",
            OfficeTitle = "County magistrate",
            HasAppointment = true,
            AuthorityTier = 3,
            JurisdictionLeverage = 32,
            PetitionPressure = 58,
            PetitionBacklog = 12,
            AdministrativeTaskLoad = 60,
            ClerkDependence = 42,
            OfficeReputation = 50,
        });
        state.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(2),
            ClanId = new ClanId(2),
            SettlementId = new SettlementId(20),
            DisplayName = "Official2",
            OfficeTitle = "County magistrate",
            HasAppointment = true,
            AuthorityTier = 3,
            JurisdictionLeverage = 10,
            PetitionPressure = 5,
            PetitionBacklog = 1,
            AdministrativeTaskLoad = 8,
            ClerkDependence = 8,
            OfficeReputation = 40,
        });
        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);
        return state;
    }
}
