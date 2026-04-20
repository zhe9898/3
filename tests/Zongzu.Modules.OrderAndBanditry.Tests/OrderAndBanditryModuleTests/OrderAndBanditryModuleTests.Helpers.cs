using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.OrderAndBanditry.Tests;

public sealed partial class OrderAndBanditryModuleTests
{
    private static FeatureManifest CreateEnabledManifest()

    {

        FeatureManifest manifest = new();

        manifest.Set(KnownModuleKeys.WorldSettlements, FeatureMode.Full);

        manifest.Set(KnownModuleKeys.FamilyCore, FeatureMode.Full);

        manifest.Set(KnownModuleKeys.PopulationAndHouseholds, FeatureMode.Full);

        manifest.Set(KnownModuleKeys.SocialMemoryAndRelations, FeatureMode.Full);

        manifest.Set(KnownModuleKeys.TradeAndIndustry, FeatureMode.Lite);

        manifest.Set(KnownModuleKeys.OrderAndBanditry, FeatureMode.Lite);

        return manifest;

    }


    private static FeatureManifest CreateConflictEnabledManifest()

    {

        FeatureManifest manifest = CreateEnabledManifest();

        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);

        return manifest;

    }


    private static FeatureManifest CreateGovernanceEnabledManifest()

    {

        FeatureManifest manifest = CreateEnabledManifest();

        manifest.Set(KnownModuleKeys.OfficeAndCareer, FeatureMode.Lite);

        return manifest;

    }


    private static FeatureManifest CreateGovernanceConflictEnabledManifest()

    {

        FeatureManifest manifest = CreateGovernanceEnabledManifest();

        manifest.Set(KnownModuleKeys.ConflictAndForce, FeatureMode.Lite);

        return manifest;

    }


    private sealed class StubWarfareCampaignQueries : IWarfareCampaignQueries

    {

        private readonly IReadOnlyList<CampaignFrontSnapshot> _campaigns;


        public StubWarfareCampaignQueries(IReadOnlyList<CampaignFrontSnapshot> campaigns)

        {

            _campaigns = campaigns;

        }


        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)

        {

            return _campaigns.Single(campaign => campaign.CampaignId == campaignId);

        }


        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()

        {

            return _campaigns;

        }


        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()

        {

            return [];

        }

    }
}
