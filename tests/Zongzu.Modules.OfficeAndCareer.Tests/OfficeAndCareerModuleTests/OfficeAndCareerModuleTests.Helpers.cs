using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.SocialMemoryAndRelations;

namespace Zongzu.Modules.OfficeAndCareer.Tests;

public sealed partial class OfficeAndCareerModuleTests
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

    private sealed class StubConflictAndForceQueries : IConflictAndForceQueries
    {
        private readonly IReadOnlyList<LocalForcePoolSnapshot> _forces;

        public StubConflictAndForceQueries(IReadOnlyList<LocalForcePoolSnapshot> forces)
        {
            _forces = forces;
        }

        public LocalForcePoolSnapshot GetRequiredSettlementForce(SettlementId settlementId)
        {
            return _forces.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<LocalForcePoolSnapshot> GetSettlementForces()
        {
            return _forces;
        }
    }

    private sealed class StubOrderAndBanditryQueries : IOrderAndBanditryQueries, IBlackRoutePressureQueries
    {
        private readonly IReadOnlyList<SettlementDisorderSnapshot> _disorders;
        private readonly IReadOnlyList<SettlementBlackRoutePressureSnapshot> _pressures;

        public StubOrderAndBanditryQueries(
            IReadOnlyList<SettlementDisorderSnapshot> disorders,
            IReadOnlyList<SettlementBlackRoutePressureSnapshot> pressures)
        {
            _disorders = disorders;
            _pressures = pressures;
        }

        public SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId)
        {
            return _disorders.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder()
        {
            return _disorders;
        }

        public SettlementBlackRoutePressureSnapshot GetRequiredSettlementBlackRoutePressure(SettlementId settlementId)
        {
            return _pressures.Single(settlement => settlement.SettlementId == settlementId);
        }

        public IReadOnlyList<SettlementBlackRoutePressureSnapshot> GetSettlementBlackRoutePressures()
        {
            return _pressures;
        }
    }
}
