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
}
