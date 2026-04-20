using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;

namespace Zongzu.Application;

public static partial class SimulationBootstrapper
{
    public static GameSimulation CreateM0M1Bootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM0M1Modules());

        SeedMinimalWorld(simulation);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateMvpBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateMvpModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM2Bootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM2Modules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    /// <summary>
    /// SPATIAL_SKELETON_SPEC §22 — dedicated bootstrap for the Phase 1c
    /// living-world liveness tests. Uses <see cref="LanxiSeed"/> to build
    /// the full 9-node / 5-route Jiangnan water-network county with the
    /// March-1200 initial season band, instead of the single-settlement
    /// <c>SeedMinimalWorld</c> fixture the rest of the test corpus depends on.
    ///
    /// <para>Only <c>WorldSettlements</c> is enabled: liveness is a property
    /// of the spatial skeleton itself, independent of Family / Population /
    /// Trade layers. Keeping the surface narrow also lets the test assert
    /// that no other module mutated <c>WorldSettlementsState</c>.</para>
    /// </summary>
    public static GameSimulation CreatePhase1cLivenessBootstrap(long seed)
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 3),
            KernelState.Create(seed),
            manifest,
            [new WorldSettlementsModule()]);

        WorldSettlementsState worldState = simulation.GetMutableModuleState<WorldSettlementsState>(KnownModuleKeys.WorldSettlements);
        LanxiSeed.Seed(worldState, simulation.KernelState);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM3OrderAndBanditryBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3OrderAndBanditryModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM3OrderAndBanditryStressBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3OrderAndBanditryModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3StressWorld(simulation, includeConflict: false);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM3LocalConflictBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3LocalConflictModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateM3LocalConflictStressBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateM3LocalConflictModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        SeedM3StressWorld(simulation, includeConflict: true);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateP1GovernanceLocalConflictBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateP1GovernanceLocalConflictModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedP1GovernanceWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation CreateP3CampaignSandboxBootstrap(long seed)
    {
        FeatureManifest manifest = CreateM0M1Manifest();
        manifest.Set(KnownModuleKeys.EducationAndExams, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.WarfareCampaign, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.PublicLifeAndRumor, FeatureMode.Lite);
        manifest.Set(KnownModuleKeys.NarrativeProjection, FeatureMode.Full);

        GameSimulation simulation = GameSimulation.CreateNew(
            new GameDate(1200, 1),
            KernelState.Create(seed),
            manifest,
            CreateP3CampaignSandboxModules());

        SeedFixture fixture = SeedMinimalWorld(simulation);
        SeedM2LiteWorld(simulation, fixture);
        SeedP1GovernanceWorld(simulation, fixture);
        SeedM3OrderWorld(simulation, fixture);
        SeedM3ConflictWorld(simulation, fixture);
        simulation.RefreshReplayHash();
        return simulation;
    }

    public static GameSimulation LoadM0M1(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM0M1Modules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadMvp(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateMvpModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadM2(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM2Modules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadM3OrderAndBanditry(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM3OrderAndBanditryModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadM3LocalConflict(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateM3LocalConflictModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadP1GovernanceLocalConflict(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateP1GovernanceLocalConflictModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

    public static GameSimulation LoadP3CampaignSandbox(SaveRoot saveRoot, SaveMigrationPipeline? migrationPipeline = null)
    {
        return GameSimulation.Load(saveRoot, CreateP3CampaignSandboxModules(), migrationPipeline ?? CreateDefaultMigrationPipeline());
    }

}
