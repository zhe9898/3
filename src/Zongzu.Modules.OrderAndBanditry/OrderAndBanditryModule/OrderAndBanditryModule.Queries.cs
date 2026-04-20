using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed partial class OrderAndBanditryModule : ModuleRunner<OrderAndBanditryState>
{
    private sealed class OrderAndBanditryQueries : IOrderAndBanditryQueries, IBlackRoutePressureQueries

    {

        private readonly OrderAndBanditryState _state;


        public OrderAndBanditryQueries(OrderAndBanditryState state)

        {

            _state = state;

        }


        public SettlementDisorderSnapshot GetRequiredSettlementDisorder(SettlementId settlementId)

        {

            SettlementDisorderState settlement = _state.Settlements.Single(settlement => settlement.SettlementId == settlementId);

            return Clone(settlement);

        }


        public IReadOnlyList<SettlementDisorderSnapshot> GetSettlementDisorder()

        {

            return _state.Settlements

                .OrderBy(static settlement => settlement.SettlementId.Value)

                .Select(Clone)

                .ToArray();

        }


        public SettlementBlackRoutePressureSnapshot GetRequiredSettlementBlackRoutePressure(SettlementId settlementId)

        {

            SettlementDisorderState settlement = _state.Settlements.Single(settlement => settlement.SettlementId == settlementId);

            return CloneBlackRoutePressure(settlement);

        }


        public IReadOnlyList<SettlementBlackRoutePressureSnapshot> GetSettlementBlackRoutePressures()

        {

            return _state.Settlements

                .OrderBy(static settlement => settlement.SettlementId.Value)

                .Select(CloneBlackRoutePressure)

                .ToArray();

        }


        private static SettlementDisorderSnapshot Clone(SettlementDisorderState settlement)

        {

            return new SettlementDisorderSnapshot

            {

                SettlementId = settlement.SettlementId,

                BanditThreat = settlement.BanditThreat,

                RoutePressure = settlement.RoutePressure,

                SuppressionDemand = settlement.SuppressionDemand,

                DisorderPressure = settlement.DisorderPressure,

                LastPressureReason = settlement.LastPressureReason,

                LastInterventionCommandCode = settlement.LastInterventionCommandCode,

                LastInterventionCommandLabel = settlement.LastInterventionCommandLabel,

                LastInterventionSummary = settlement.LastInterventionSummary,

                LastInterventionOutcome = settlement.LastInterventionOutcome,

                InterventionCarryoverMonths = settlement.InterventionCarryoverMonths,

            };

        }


        private static SettlementBlackRoutePressureSnapshot CloneBlackRoutePressure(SettlementDisorderState settlement)

        {

            return new SettlementBlackRoutePressureSnapshot

            {

                SettlementId = settlement.SettlementId,

                BlackRoutePressure = settlement.BlackRoutePressure,

                CoercionRisk = settlement.CoercionRisk,

                SuppressionRelief = settlement.SuppressionRelief,

                ResponseActivationLevel = settlement.ResponseActivationLevel,

                PaperCompliance = settlement.PaperCompliance,

                ImplementationDrag = settlement.ImplementationDrag,

                RouteShielding = settlement.RouteShielding,

                RetaliationRisk = settlement.RetaliationRisk,

                AdministrativeSuppressionWindow = settlement.AdministrativeSuppressionWindow,

                EscalationBandLabel = settlement.EscalationBandLabel,

                LastPressureTrace = settlement.LastPressureTrace,

            };

        }

    }


    private sealed record TradeActivitySnapshot(int ActiveRouteCount, int AverageRouteRisk, int TotalRouteCapacity)

    {

        public static readonly TradeActivitySnapshot Empty = new(0, 0, 0);

    }


    private readonly record struct InterventionCarryoverEffect(

        int BanditDelta,

        int RouteDelta,

        int DisorderDelta,

        int RouteShieldingDelta,

        int SuppressionDemandDelta,

        int SuppressionReliefDelta,

        int RetaliationRiskDelta,

        int BlackRoutePressureDelta,

        int CoercionRiskDelta);
}
