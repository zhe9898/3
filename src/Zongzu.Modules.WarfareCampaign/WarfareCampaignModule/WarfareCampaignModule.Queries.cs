using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WarfareCampaign;

public sealed partial class WarfareCampaignModule : ModuleRunner<WarfareCampaignState>
{
    private sealed class WarfareCampaignQueries : IWarfareCampaignQueries

    {

        private readonly WarfareCampaignState _state;


        public WarfareCampaignQueries(WarfareCampaignState state)

        {

            _state = state;

        }


        public CampaignFrontSnapshot GetRequiredCampaign(CampaignId campaignId)

        {

            CampaignFrontState campaign = _state.Campaigns.Single(existing => existing.CampaignId == campaignId);

            return CloneCampaign(campaign);

        }


        public IReadOnlyList<CampaignFrontSnapshot> GetCampaigns()

        {

            return _state.Campaigns

                .OrderBy(static campaign => campaign.CampaignId.Value)

                .Select(CloneCampaign)

                .ToArray();

        }


        public IReadOnlyList<CampaignMobilizationSignalSnapshot> GetMobilizationSignals()

        {

            return _state.MobilizationSignals

                .OrderBy(static signal => signal.SettlementId.Value)

                .Select(CloneSignal)

                .ToArray();

        }


        private static CampaignFrontSnapshot CloneCampaign(CampaignFrontState campaign)

        {

            return new CampaignFrontSnapshot

            {

                CampaignId = campaign.CampaignId,

                AnchorSettlementId = campaign.AnchorSettlementId,

                AnchorSettlementName = campaign.AnchorSettlementName,

                CampaignName = campaign.CampaignName,

                IsActive = campaign.IsActive,

                ObjectiveSummary = campaign.ObjectiveSummary,

                MobilizedForceCount = campaign.MobilizedForceCount,

                FrontPressure = campaign.FrontPressure,

                FrontLabel = campaign.FrontLabel,

                SupplyState = campaign.SupplyState,

                SupplyStateLabel = campaign.SupplyStateLabel,

                MoraleState = campaign.MoraleState,

                MoraleStateLabel = campaign.MoraleStateLabel,

                CommandFitLabel = campaign.CommandFitLabel,

                CommanderSummary = campaign.CommanderSummary,

                ActiveDirectiveCode = campaign.ActiveDirectiveCode,

                ActiveDirectiveLabel = campaign.ActiveDirectiveLabel,

                ActiveDirectiveSummary = campaign.ActiveDirectiveSummary,

                LastDirectiveTrace = campaign.LastDirectiveTrace,

                MobilizationWindowLabel = campaign.MobilizationWindowLabel,

                SupplyLineSummary = campaign.SupplyLineSummary,

                OfficeCoordinationTrace = campaign.OfficeCoordinationTrace,

                SourceTrace = campaign.SourceTrace,

                LastAftermathSummary = campaign.LastAftermathSummary,

                Routes = campaign.Routes

                    .Select(static route => new CampaignRouteSnapshot

                    {

                        RouteLabel = route.RouteLabel,

                        RouteRole = route.RouteRole,

                        Pressure = route.Pressure,

                        Security = route.Security,

                        FlowStateLabel = route.FlowStateLabel,

                        Summary = route.Summary,

                    })

                    .ToArray(),

            };

        }


        private static CampaignMobilizationSignalSnapshot CloneSignal(CampaignMobilizationSignalState signal)

        {

            return new CampaignMobilizationSignalSnapshot

            {

                SettlementId = signal.SettlementId,

                SettlementName = signal.SettlementName,

                ResponseActivationLevel = signal.ResponseActivationLevel,

                CommandCapacity = signal.CommandCapacity,

                Readiness = signal.Readiness,

                AvailableForceCount = signal.AvailableForceCount,

                OrderSupportLevel = signal.OrderSupportLevel,

                OfficeAuthorityTier = signal.OfficeAuthorityTier,

                AdministrativeLeverage = signal.AdministrativeLeverage,

                PetitionBacklog = signal.PetitionBacklog,

                CommandFitLabel = signal.CommandFitLabel,

                ActiveDirectiveCode = signal.ActiveDirectiveCode,

                ActiveDirectiveLabel = signal.ActiveDirectiveLabel,

                ActiveDirectiveSummary = signal.ActiveDirectiveSummary,

                MobilizationWindowLabel = signal.MobilizationWindowLabel,

                OfficeCoordinationTrace = signal.OfficeCoordinationTrace,

                SourceTrace = signal.SourceTrace,

            };

        }

    }
}
