using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WorldSettlements;

namespace Zongzu.Modules.TradeAndIndustry.Tests;

public sealed partial class TradeAndIndustryModuleTests
{
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

    private sealed class StubOrderQueries : IOrderAndBanditryQueries
    {
        private readonly IReadOnlyList<SettlementDisorderSnapshot> _disorders;

        public StubOrderQueries(IReadOnlyList<SettlementDisorderSnapshot> disorders)
        {
            _disorders = disorders;
        }

        public SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId)
        {
            return _disorders.Single(disorder => disorder.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder()
        {
            return _disorders;
        }
    }

    private sealed class StubBlackRoutePressureQueries : IBlackRoutePressureQueries
    {
        private readonly IReadOnlyList<SettlementBlackRoutePressureSnapshot> _pressures;

        public StubBlackRoutePressureQueries(IReadOnlyList<SettlementBlackRoutePressureSnapshot> pressures)
        {
            _pressures = pressures;
        }

        public SettlementBlackRoutePressureSnapshot GetRequiredSettlementBlackRoutePressure(SettlementId settlementId)
        {
            return _pressures.Single(pressure => pressure.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementBlackRoutePressureSnapshot> GetSettlementBlackRoutePressures()
        {
            return _pressures;
        }
    }
}
