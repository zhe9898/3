using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed partial class PublicLifeAndRumorModule : ModuleRunner<PublicLifeAndRumorState>
{
    private sealed class PublicLifeAndRumorQueries : IPublicLifeAndRumorQueries

    {

        private readonly PublicLifeAndRumorState _state;


        public PublicLifeAndRumorQueries(PublicLifeAndRumorState state)

        {

            _state = state;

        }


        public SettlementPublicLifeSnapshot GetRequiredSettlementPublicLife(SettlementId settlementId)

        {

            SettlementPublicLifeState settlement = _state.Settlements.Single(entry => entry.SettlementId == settlementId);

            return Clone(settlement);

        }


        public IReadOnlyList<SettlementPublicLifeSnapshot> GetSettlementPublicLife()

        {

            return _state.Settlements

                .OrderBy(static entry => entry.SettlementId.Value)

                .Select(Clone)

                .ToArray();

        }


        private static SettlementPublicLifeSnapshot Clone(SettlementPublicLifeState state)

        {

            return new SettlementPublicLifeSnapshot

            {

                SettlementId = state.SettlementId,

                SettlementName = state.SettlementName,

                SettlementTier = state.SettlementTier,

                NodeLabel = state.NodeLabel,

                DominantVenueLabel = state.DominantVenueLabel,

                DominantVenueCode = state.DominantVenueCode,

                MonthlyCadenceCode = state.MonthlyCadenceCode,

                MonthlyCadenceLabel = state.MonthlyCadenceLabel,

                CrowdMixLabel = state.CrowdMixLabel,

                StreetTalkHeat = state.StreetTalkHeat,

                MarketBuzz = state.MarketBuzz,

                NoticeVisibility = state.NoticeVisibility,

                RoadReportLag = state.RoadReportLag,

                PrefectureDispatchPressure = state.PrefectureDispatchPressure,

                PublicLegitimacy = state.PublicLegitimacy,

                DocumentaryWeight = state.DocumentaryWeight,

                VerificationCost = state.VerificationCost,

                MarketRumorFlow = state.MarketRumorFlow,

                CourierRisk = state.CourierRisk,

                OfficialNoticeLine = state.OfficialNoticeLine,

                StreetTalkLine = state.StreetTalkLine,

                RoadReportLine = state.RoadReportLine,

                PrefectureDispatchLine = state.PrefectureDispatchLine,

                ContentionSummary = state.ContentionSummary,

                CadenceSummary = state.CadenceSummary,

                PublicSummary = state.PublicSummary,

                ChannelSummary = state.ChannelSummary,

                RouteReportSummary = state.RouteReportSummary,

                LastPublicTrace = state.LastPublicTrace,

            };

        }

    }
}
