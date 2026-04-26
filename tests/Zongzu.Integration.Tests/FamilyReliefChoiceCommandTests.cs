using System;
using System.Linq;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class FamilyReliefChoiceCommandTests
{
    [Test]
    public void GrantClanRelief_MutatesOnlyFamilyCoreAndProjectsFamilyReadback()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateM2Bootstrap(20260426);
        PresentationReadModelBuilder builder = new();
        FamilyCoreState familyState = simulation.GetModuleStateForTesting<FamilyCoreState>(
            KnownModuleKeys.FamilyCore);
        PopulationAndHouseholdsState populationState = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(
            KnownModuleKeys.PopulationAndHouseholds);
        SocialMemoryAndRelationsState socialState = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(
            KnownModuleKeys.SocialMemoryAndRelations);

        ClanStateData clan = familyState.Clans
            .OrderBy(static candidate => candidate.Id.Value)
            .First();
        clan.CharityObligation = 46;
        clan.SupportReserve = 38;
        clan.BranchTension = 52;
        clan.ReliefSanctionPressure = 34;
        clan.BranchFavorPressure = 12;
        clan.MediationMomentum = 6;

        int supportReserveBefore = clan.SupportReserve;
        int charityObligationBefore = clan.CharityObligation;
        int reliefSanctionPressureBefore = clan.ReliefSanctionPressure;
        int branchTensionBefore = clan.BranchTension;
        string populationBefore = SnapshotPopulation(populationState);
        string socialBefore = SnapshotSocialMemory(socialState);

        PresentationReadModelBundle beforeBundle = builder.BuildForM2(simulation);
        PlayerCommandAffordanceSnapshot affordance = beforeBundle.PlayerCommands.Affordances.Single(candidate =>
            candidate.CommandName == PlayerCommandNames.GrantClanRelief
            && candidate.ClanId == clan.Id);
        Assert.That(affordance.IsEnabled, Is.True);
        Assert.That(affordance.ReadbackSummary, Does.Contain("Family\u6551\u6d4e\u9009\u62e9\u8bfb\u56de"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("\u63a5\u6d4e\u4e49\u52a1\u8bfb\u56de"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("\u5b97\u623f\u4f59\u529b\u8bfb\u56de"));
        Assert.That(affordance.ReadbackSummary, Does.Contain("\u4e0d\u662f\u666e\u901a\u5bb6\u6237\u518d\u625b"));

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.GrantClanRelief,
            });

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.ModuleKey, Is.EqualTo(KnownModuleKeys.FamilyCore));
        Assert.That(result.SurfaceKey, Is.EqualTo(PlayerCommandSurfaceKeys.Family));
        Assert.That(clan.SupportReserve, Is.LessThan(supportReserveBefore));
        Assert.That(clan.CharityObligation, Is.LessThan(charityObligationBefore));
        Assert.That(clan.ReliefSanctionPressure, Is.LessThan(reliefSanctionPressureBefore));
        Assert.That(clan.BranchTension, Is.LessThan(branchTensionBefore));
        Assert.That(SnapshotPopulation(populationState), Is.EqualTo(populationBefore));
        Assert.That(SnapshotSocialMemory(socialState), Is.EqualTo(socialBefore));

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot receipt = afterBundle.PlayerCommands.Receipts.Single(candidate =>
            candidate.CommandName == PlayerCommandNames.GrantClanRelief
            && candidate.ClanId == clan.Id);
        Assert.That(receipt.ReadbackSummary, Does.Contain("Family\u6551\u6d4e\u9009\u62e9\u8bfb\u56de"));
        Assert.That(receipt.ReadbackSummary, Does.Contain("\u4e0d\u662f\u666e\u901a\u5bb6\u6237\u518d\u625b"));
    }

    private static string SnapshotPopulation(PopulationAndHouseholdsState state)
    {
        return string.Join(
            "|",
            state.Households
                .OrderBy(static household => household.Id.Value)
                .Select(static household =>
                    string.Join(
                        ":",
                        household.Id.Value,
                        household.SettlementId.Value,
                        household.SponsorClanId?.Value.ToString() ?? string.Empty,
                        household.Distress,
                        household.DebtPressure,
                        household.LaborCapacity,
                        household.MigrationRisk,
                        household.LastLocalResponseCommandCode,
                        household.LastLocalResponseOutcomeCode,
                        household.LastLocalResponseTraceCode,
                        household.LocalResponseCarryoverMonths)));
    }

    private static string SnapshotSocialMemory(SocialMemoryAndRelationsState state)
    {
        return string.Join("||", new[]
        {
            string.Join(
                "|",
                state.Memories
                    .OrderBy(static memory => memory.Id.Value)
                    .Select(static memory =>
                        string.Join(
                            ":",
                            memory.Id.Value,
                            memory.SubjectClanId.Value,
                            memory.Kind,
                            memory.Intensity,
                            memory.Type,
                            memory.Subtype,
                            memory.CauseKey,
                            memory.Weight,
                            memory.MonthlyDecay,
                            memory.LifecycleState))),
            string.Join(
                "|",
                state.ClanNarratives
                    .OrderBy(static narrative => narrative.ClanId.Value)
                    .Select(static narrative =>
                        string.Join(
                            ":",
                            narrative.ClanId.Value,
                            narrative.GrudgePressure,
                            narrative.FearPressure,
                            narrative.ShamePressure,
                            narrative.FavorBalance))),
            string.Join(
                "|",
                state.ClanEmotionalClimates
                    .OrderBy(static climate => climate.ClanId.Value)
                    .Select(static climate =>
                        string.Join(
                            ":",
                            climate.ClanId.Value,
                            climate.Fear,
                            climate.Shame,
                            climate.Grief,
                            climate.Anger,
                            climate.Obligation,
                            climate.Hope,
                            climate.Trust,
                            climate.Restraint,
                            climate.Hardening,
                            climate.Bitterness,
                            climate.Volatility,
                            climate.LastTrace))),
            string.Join(
                "|",
                state.PersonTemperings
                    .OrderBy(static tempering => tempering.PersonId.Value)
                    .Select(static tempering =>
                        string.Join(
                            ":",
                            tempering.PersonId.Value,
                            tempering.ClanId.Value,
                            tempering.Fear,
                            tempering.Shame,
                            tempering.Grief,
                            tempering.Anger,
                            tempering.Obligation,
                            tempering.Hope,
                            tempering.Trust,
                            tempering.Restraint,
                            tempering.Hardening,
                            tempering.Bitterness,
                            tempering.Volatility,
                            tempering.LastTrace))),
        });
    }
}
