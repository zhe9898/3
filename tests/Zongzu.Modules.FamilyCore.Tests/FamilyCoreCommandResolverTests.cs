using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore.Tests;

[TestFixture]
public sealed class FamilyCoreCommandResolverTests
{
    [Test]
    public void IssueIntent_WithoutSocialMemoryQuery_UsesNeutralFamilyResolution()
    {
        FamilyCoreState state = BuildConflictState();
        ClanStateData before = CloneClan(state.Clans.Single());

        PlayerCommandResult result = FamilyCoreCommandResolver.IssueIntent(new FamilyCoreCommandContext
        {
            State = state,
            CurrentDate = new GameDate(1200, 4),
            Command = BuildCommand(PlayerCommandNames.InviteClanEldersMediation),
        });

        ClanStateData after = state.Clans.Single();
        Assert.That(result.Accepted, Is.True);
        Assert.That(result.Summary, Does.Contain("房支争势"));
        Assert.That(result.Summary, Does.Not.Contain("余怨"));
        Assert.That(after.BranchTension, Is.LessThan(before.BranchTension));
        Assert.That(after.MediationMomentum, Is.GreaterThan(before.MediationMomentum));
    }

    [Test]
    public void IssueIntent_HighBitternessAndVolatility_WeakensMediation()
    {
        FamilyCoreState neutralState = BuildConflictState();
        FamilyCoreState bitterState = BuildConflictState();

        FamilyCoreCommandResolver.IssueIntent(new FamilyCoreCommandContext
        {
            State = neutralState,
            CurrentDate = new GameDate(1200, 4),
            Command = BuildCommand(PlayerCommandNames.InviteClanEldersMediation),
        });

        PlayerCommandResult bitterResult = FamilyCoreCommandResolver.IssueIntent(new FamilyCoreCommandContext
        {
            State = bitterState,
            CurrentDate = new GameDate(1200, 4),
            Command = BuildCommand(PlayerCommandNames.InviteClanEldersMediation),
            SocialMemoryQueries = new StubSocialMemoryQueries
            {
                Climate = new ClanEmotionalClimateSnapshot
                {
                    ClanId = new ClanId(1),
                    Bitterness = 86,
                    Volatility = 84,
                    Anger = 74,
                },
                Temperings =
                [
                    new PersonPressureTemperingSnapshot
                    {
                        PersonId = new PersonId(1),
                        ClanId = new ClanId(1),
                        Bitterness = 90,
                        Volatility = 88,
                        Hardening = 70,
                    },
                ],
            },
        });

        ClanStateData neutralAfter = neutralState.Clans.Single();
        ClanStateData bitterAfter = bitterState.Clans.Single();
        Assert.That(bitterResult.Accepted, Is.True);
        Assert.That(bitterResult.Summary, Does.Contain("余怨"));
        Assert.That(bitterAfter.BranchTension, Is.GreaterThan(neutralAfter.BranchTension));
        Assert.That(bitterAfter.MediationMomentum, Is.LessThan(neutralAfter.MediationMomentum));
    }

    [Test]
    public void IssueIntent_HighTrustAndRestraint_StrengthensFormalApology()
    {
        FamilyCoreState neutralState = BuildConflictState();
        FamilyCoreState trustedState = BuildConflictState();

        FamilyCoreCommandResolver.IssueIntent(new FamilyCoreCommandContext
        {
            State = neutralState,
            CurrentDate = new GameDate(1200, 4),
            Command = BuildCommand(PlayerCommandNames.OrderFormalApology),
        });

        PlayerCommandResult trustedResult = FamilyCoreCommandResolver.IssueIntent(new FamilyCoreCommandContext
        {
            State = trustedState,
            CurrentDate = new GameDate(1200, 4),
            Command = BuildCommand(PlayerCommandNames.OrderFormalApology),
            SocialMemoryQueries = new StubSocialMemoryQueries
            {
                Climate = new ClanEmotionalClimateSnapshot
                {
                    ClanId = new ClanId(1),
                    Trust = 88,
                    Restraint = 84,
                    Obligation = 72,
                },
                Temperings =
                [
                    new PersonPressureTemperingSnapshot
                    {
                        PersonId = new PersonId(1),
                        ClanId = new ClanId(1),
                        Trust = 90,
                        Restraint = 86,
                    },
                ],
            },
        });

        ClanStateData neutralAfter = neutralState.Clans.Single();
        ClanStateData trustedAfter = trustedState.Clans.Single();
        Assert.That(trustedResult.Accepted, Is.True);
        Assert.That(trustedResult.Summary, Does.Contain("信义"));
        Assert.That(trustedAfter.BranchTension, Is.LessThan(neutralAfter.BranchTension));
        Assert.That(trustedAfter.MediationMomentum, Is.GreaterThan(neutralAfter.MediationMomentum));
    }

    private static FamilyCoreState BuildConflictState()
    {
        FamilyCoreState state = new();
        state.Clans.Add(new ClanStateData
        {
            Id = new ClanId(1),
            ClanName = "张",
            HomeSettlementId = new SettlementId(1),
            Prestige = 68,
            SupportReserve = 46,
            BranchTension = 68,
            InheritancePressure = 62,
            SeparationPressure = 58,
            BranchFavorPressure = 56,
            ReliefSanctionPressure = 24,
            MediationMomentum = 12,
        });
        state.People.Add(new FamilyPersonState
        {
            Id = new PersonId(1),
            ClanId = new ClanId(1),
            GivenName = "张成",
            AgeMonths = 31 * 12,
            IsAlive = true,
        });

        return state;
    }

    private static PlayerCommandRequest BuildCommand(string commandName)
    {
        return new PlayerCommandRequest
        {
            SettlementId = new SettlementId(1),
            ClanId = new ClanId(1),
            CommandName = commandName,
        };
    }

    private static ClanStateData CloneClan(ClanStateData source)
    {
        return new ClanStateData
        {
            Id = source.Id,
            ClanName = source.ClanName,
            HomeSettlementId = source.HomeSettlementId,
            Prestige = source.Prestige,
            SupportReserve = source.SupportReserve,
            BranchTension = source.BranchTension,
            InheritancePressure = source.InheritancePressure,
            SeparationPressure = source.SeparationPressure,
            MediationMomentum = source.MediationMomentum,
            BranchFavorPressure = source.BranchFavorPressure,
            ReliefSanctionPressure = source.ReliefSanctionPressure,
        };
    }

    private sealed class StubSocialMemoryQueries : ISocialMemoryAndRelationsQueries
    {
        public ClanEmotionalClimateSnapshot Climate { get; init; } = new() { ClanId = new ClanId(1) };

        public IReadOnlyList<PersonPressureTemperingSnapshot> Temperings { get; init; } = [];

        public ClanNarrativeSnapshot GetRequiredClanNarrative(ClanId clanId)
        {
            return new ClanNarrativeSnapshot { ClanId = clanId };
        }

        public IReadOnlyList<ClanNarrativeSnapshot> GetClanNarratives()
        {
            return [new ClanNarrativeSnapshot { ClanId = Climate.ClanId }];
        }

        public ClanEmotionalClimateSnapshot GetClanEmotionalClimate(ClanId clanId)
        {
            return Climate.ClanId == clanId ? Climate : new ClanEmotionalClimateSnapshot { ClanId = clanId };
        }

        public IReadOnlyList<PersonPressureTemperingSnapshot> GetPersonTemperingsByClan(ClanId clanId)
        {
            return Temperings
                .Where(tempering => tempering.ClanId == clanId)
                .OrderBy(static tempering => tempering.PersonId.Value)
                .ToArray();
        }
    }
}
