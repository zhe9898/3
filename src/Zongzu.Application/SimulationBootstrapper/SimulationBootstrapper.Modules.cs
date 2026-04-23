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
    public static IReadOnlyList<IModuleRunner> CreateM0M1Modules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateMvpModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM2Modules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM3OrderAndBanditryModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OrderAndBanditryModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateM3LocalConflictModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateP1GovernanceLocalConflictModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OfficeAndCareerModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }

    public static IReadOnlyList<IModuleRunner> CreateP3CampaignSandboxModules()
    {
        return
        [
            new PersonRegistryModule(),
            new WorldSettlementsModule(),
            new PopulationAndHouseholdsModule(),
            new FamilyCoreModule(),
            new SocialMemoryAndRelationsModule(),
            new EducationAndExamsModule(),
            new TradeAndIndustryModule(),
            new OfficeAndCareerModule(),
            new OrderAndBanditryModule(),
            new ConflictAndForceModule(),
            new WarfareCampaignModule(),
            new PublicLifeAndRumorModule(),
            new NarrativeProjectionModule(),
        ];
    }


    private static FeatureManifest CreateM0M1Manifest()
    {
        FeatureManifest manifest = new();
        manifest.Set(KnownModuleKeys.PersonRegistry, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);
        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);
        return manifest;
    }

}
