using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class CommandSeamIntegrationTests
{
    [Test]
    public void Accepted_office_command_mutates_office_state_and_refreshes_replay_hash()
    {
        GameSimulation simulation = CreateOfficeOnlySimulation(20260423);
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(KnownModuleKeys.OfficeAndCareer);
        SettlementId settlementId = new(12);

        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            DisplayName = "Zhang Wen",
            SettlementId = settlementId,
            HasAppointment = true,
            OfficeTitle = "County Clerk",
            AuthorityTier = 2,
            JurisdictionLeverage = 64,
            PetitionBacklog = 12,
            PetitionPressure = 24,
            AdministrativeTaskLoad = 8,
            OfficeReputation = 32,
            PromotionMomentum = 3,
            DemotionPressure = 7,
            LastOutcome = "Serving",
            LastPetitionOutcome = "Queued",
        });

        string replayHashBefore = simulation.ReplayHash;

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = settlementId,
                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
            });

        OfficeCareerState updatedCareer = officeState.People.Single();

        Assert.That(result.Accepted, Is.True);
        Assert.That(result.ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
        Assert.That(updatedCareer.PetitionBacklog, Is.LessThan(12));
        Assert.That(updatedCareer.PetitionPressure, Is.LessThan(24));
        Assert.That(updatedCareer.CurrentAdministrativeTask, Is.Not.Empty);
        Assert.That(officeState.Jurisdictions, Has.Count.EqualTo(1));
        Assert.That(simulation.ReplayHash, Is.Not.EqualTo(replayHashBefore));
    }

    [Test]
    public void Rejected_office_command_leaves_replay_hash_unchanged()
    {
        GameSimulation simulation = CreateOfficeOnlySimulation(20260424);
        string replayHashBefore = simulation.ReplayHash;

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = new SettlementId(88),
                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
            });

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.ModuleKey, Is.EqualTo(KnownModuleKeys.OfficeAndCareer));
        Assert.That(simulation.ReplayHash, Is.EqualTo(replayHashBefore));
    }

    [Test]
    public void Disabled_command_uses_catalog_metadata_and_preserves_replay_hash()
    {
        GameSimulation simulation = CreateFamilyOnlySimulation(20260425);
        string replayHashBefore = simulation.ReplayHash;
        PlayerCommandRoute route = PlayerCommandCatalog.GetRequired(PlayerCommandNames.PetitionViaOfficeChannels);

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            new PlayerCommandRequest
            {
                SettlementId = new SettlementId(88),
                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
            });

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.ModuleKey, Is.EqualTo(route.ModuleKey));
        Assert.That(result.SurfaceKey, Is.EqualTo(route.SurfaceKey));
        Assert.That(result.Label, Is.EqualTo(route.Label));
        Assert.That(result.Summary, Is.EqualTo(route.DisabledSummary));
        Assert.That(result.TargetLabel, Is.EqualTo(route.BuildDisabledTargetLabel(new PlayerCommandRequest
        {
            SettlementId = new SettlementId(88),
            CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
        })));
        Assert.That(simulation.ReplayHash, Is.EqualTo(replayHashBefore));
    }

    [Test]
    public void Player_command_contract_matches_live_command_owner_modules()
    {
        IReadOnlyList<IModuleRunner> modules = SimulationBootstrapper.CreateP3CampaignSandboxModules();
        HashSet<string> livePlayerCommands = GetPlayerCommandNames();
        HashSet<string> catalogCommands = PlayerCommandCatalog.All
            .Select(static route => route.CommandName)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> currentOwnerModules =
        [
            KnownModuleKeys.FamilyCore,
            KnownModuleKeys.OfficeAndCareer,
            KnownModuleKeys.OrderAndBanditry,
            KnownModuleKeys.WarfareCampaign,
        ];
        IReadOnlyList<string> duplicateCatalogCommands = PlayerCommandCatalog.All
            .GroupBy(static route => route.CommandName, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

        HashSet<string> advertisedCommands = modules
            .Where(module => currentOwnerModules.Contains(module.ModuleKey))
            .SelectMany(static module => module.AcceptedCommands)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> catalogOwnedCommands = PlayerCommandCatalog.All
            .Where(route => currentOwnerModules.Contains(route.ModuleKey))
            .Select(static route => route.CommandName)
            .ToHashSet(StringComparer.Ordinal);

        IReadOnlyList<string> strayAdvertisers = modules
            .Where(module => !currentOwnerModules.Contains(module.ModuleKey))
            .Where(static module => module.AcceptedCommands.Count > 0)
            .Select(static module => module.ModuleKey)
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

        Assert.That(duplicateCatalogCommands, Is.Empty);
        Assert.That(catalogCommands, Is.EquivalentTo(livePlayerCommands));
        Assert.That(catalogOwnedCommands, Is.EquivalentTo(livePlayerCommands));
        Assert.That(advertisedCommands, Is.EquivalentTo(livePlayerCommands));
        Assert.That(strayAdvertisers, Is.Empty,
            "Only current module-owned player-command handlers should advertise AcceptedCommands.");
    }

    [Test]
    public void Presentation_affordances_align_with_catalog_route_metadata()
    {
        PresentationReadModelBuilder builder = new();
        PresentationReadModelBundle governanceBundle = builder.BuildForM2(SimulationBootstrapper.CreateP1GovernanceLocalConflictBootstrap(20260426));
        PresentationReadModelBundle campaignBundle = builder.BuildForM2(SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260427));
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances = governanceBundle.PlayerCommands.Affordances
            .Concat(campaignBundle.PlayerCommands.Affordances)
            .ToArray();

        Assert.That(affordances, Is.Not.Empty);

        foreach (PlayerCommandAffordanceSnapshot affordance in affordances)
        {
            PlayerCommandRoute route = PlayerCommandCatalog.GetRequired(affordance.CommandName);
            Assert.That(affordance.ModuleKey, Is.EqualTo(route.ModuleKey), $"Affordance {affordance.CommandName} drifted module ownership.");
            Assert.That(affordance.SurfaceKey, Is.EqualTo(route.SurfaceKey), $"Affordance {affordance.CommandName} drifted surface ownership.");
            Assert.That(affordance.Label, Is.EqualTo(route.Label), $"Affordance {affordance.CommandName} drifted default label metadata.");
        }
    }

    [Test]
    public void Presentation_receipts_align_with_catalog_route_metadata_after_accepted_commands()
    {
        PresentationReadModelBuilder builder = new();

        GameSimulation familySimulation = SimulationBootstrapper.CreateM2Bootstrap(20260428);
        PlayerCommandReceiptSnapshot familyReceipt = IssueFirstEnabledAffordanceForModuleAndReadReceipt(
            familySimulation,
            builder,
            KnownModuleKeys.FamilyCore);

        GameSimulation officeSimulation = CreateOfficeOnlySimulation(20260429);
        SeedOfficeJurisdiction(officeSimulation, new SettlementId(12));
        PlayerCommandReceiptSnapshot officeReceipt = IssueAcceptedCommandAndReadReceipt(
            officeSimulation,
            builder,
            KnownModuleKeys.OfficeAndCareer,
            new PlayerCommandRequest
            {
                SettlementId = new SettlementId(12),
                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
            });
        GameSimulation orderSimulation = SimulationBootstrapper.CreateM3OrderAndBanditryBootstrap(20260431);
        orderSimulation.AdvanceMonths(2);
        SettlementId orderSettlementId = builder.BuildForM2(orderSimulation).PublicLifeSettlements.Single().SettlementId;
        PlayerCommandReceiptSnapshot orderReceipt = IssueAcceptedCommandAndReadReceipt(
            orderSimulation,
            builder,
            KnownModuleKeys.OrderAndBanditry,
            new PlayerCommandRequest
            {
                SettlementId = orderSettlementId,
                CommandName = PlayerCommandNames.FundLocalWatch,
            });

        GameSimulation warfareSimulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260430);
        warfareSimulation.AdvanceMonths(3);
        SettlementId warfareSettlementId = builder.BuildForM2(warfareSimulation).CampaignMobilizationSignals.Single().SettlementId;
        PlayerCommandReceiptSnapshot warfareReceipt = IssueAcceptedCommandAndReadReceipt(
            warfareSimulation,
            builder,
            KnownModuleKeys.WarfareCampaign,
            new PlayerCommandRequest
            {
                SettlementId = warfareSettlementId,
                CommandName = PlayerCommandNames.ProtectSupplyLine,
            });

        foreach (PlayerCommandReceiptSnapshot receipt in new[] { familyReceipt, officeReceipt, orderReceipt, warfareReceipt })
        {
            PlayerCommandRoute route = PlayerCommandCatalog.GetRequired(receipt.CommandName);
            Assert.That(receipt.ModuleKey, Is.EqualTo(route.ModuleKey), $"Receipt {receipt.CommandName} drifted module ownership.");
            Assert.That(receipt.SurfaceKey, Is.EqualTo(route.SurfaceKey), $"Receipt {receipt.CommandName} drifted surface ownership.");
        }
    }

    private static GameSimulation CreateOfficeOnlySimulation(int seed)
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);

        return GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            new IModuleRunner[]
            {
                new OfficeAndCareerModule(),
            });
    }

    private static GameSimulation CreateFamilyOnlySimulation(int seed)
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Lite);

        return GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            new IModuleRunner[]
            {
                new FamilyCoreModule(),
            });
    }

    private static HashSet<string> GetPlayerCommandNames()
    {
        return typeof(PlayerCommandNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(static field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(static field => (string)field.GetRawConstantValue()!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static void SeedOfficeJurisdiction(GameSimulation simulation, SettlementId settlementId)
    {
        OfficeAndCareerState officeState = simulation.GetModuleStateForTesting<OfficeAndCareerState>(KnownModuleKeys.OfficeAndCareer);
        officeState.People.Add(new OfficeCareerState
        {
            PersonId = new PersonId(1),
            DisplayName = "Zhang Wen",
            SettlementId = settlementId,
            HasAppointment = true,
            OfficeTitle = "County Clerk",
            AuthorityTier = 2,
            JurisdictionLeverage = 64,
            PetitionBacklog = 12,
            PetitionPressure = 24,
            AdministrativeTaskLoad = 8,
            OfficeReputation = 32,
            PromotionMomentum = 3,
            DemotionPressure = 7,
            LastOutcome = "Serving",
            LastPetitionOutcome = "Queued",
        });
    }

    private static PlayerCommandReceiptSnapshot IssueFirstEnabledAffordanceForModuleAndReadReceipt(
        GameSimulation simulation,
        PresentationReadModelBuilder builder,
        string moduleKey)
    {
        PlayerCommandAffordanceSnapshot? affordance = builder.BuildForM2(simulation).PlayerCommands.Affordances
            .Where(affordance => affordance.IsEnabled && string.Equals(affordance.ModuleKey, moduleKey, StringComparison.Ordinal))
            .OrderBy(static affordance => affordance.SurfaceKey, StringComparer.Ordinal)
            .ThenBy(static affordance => affordance.CommandName, StringComparer.Ordinal)
            .FirstOrDefault();

        Assert.That(affordance, Is.Not.Null, $"Expected an enabled affordance for module {moduleKey}.");

        return IssueAcceptedCommandAndReadReceipt(
            simulation,
            builder,
            moduleKey,
            new PlayerCommandRequest
            {
                SettlementId = affordance!.SettlementId,
                ClanId = affordance.ClanId,
                CommandName = affordance.CommandName,
            });
    }

    private static PlayerCommandReceiptSnapshot IssueAcceptedCommandAndReadReceipt(
        GameSimulation simulation,
        PresentationReadModelBuilder builder,
        string moduleKey,
        PlayerCommandRequest command)
    {
        ArgumentNullException.ThrowIfNull(command);

        PlayerCommandResult result = new PlayerCommandService().IssueIntent(
            simulation,
            command);

        Assert.That(result.Accepted, Is.True, $"Expected accepted command {command.CommandName} in {moduleKey}.");

        PresentationReadModelBundle afterBundle = builder.BuildForM2(simulation);
        PlayerCommandReceiptSnapshot? receipt = afterBundle.PlayerCommands.Receipts
            .Where(candidate =>
                string.Equals(candidate.ModuleKey, moduleKey, StringComparison.Ordinal)
                && string.Equals(candidate.CommandName, command.CommandName, StringComparison.Ordinal)
                && candidate.SettlementId.Equals(command.SettlementId)
                && Nullable.Equals(candidate.ClanId, command.ClanId))
            .OrderByDescending(static candidate => candidate.OutcomeSummary.Length)
            .ThenByDescending(static candidate => candidate.Summary.Length)
            .FirstOrDefault();

        Assert.That(receipt, Is.Not.Null, $"Expected a receipt for accepted command {command.CommandName} in {moduleKey}.");
        return receipt!;
    }
}
