using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryModule : ModuleRunner<OrderAndBanditryState>
{
    private static readonly string[] CommandNames =
    [
        "FundLocalWatch",
        "SuppressBanditry",
        "NegotiateWithOutlaws",
        "TolerateDisorder",
    ];

    private static readonly string[] EventNames =
    [
        "BanditThreatRaised",
        "OutlawGroupFormed",
        "SuppressionSucceeded",
        "RouteUnsafeDueToBanditry",
    ];

    public override string ModuleKey => KnownModuleKeys.OrderAndBanditry;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.EventHandling;

    public override int ExecutionOrder => 100;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override OrderAndBanditryState CreateInitialState()
    {
        return new OrderAndBanditryState();
    }

    public override void RegisterQueries(OrderAndBanditryState state, QueryRegistry queries)
    {
        queries.Register<IOrderAndBanditryQueries>(new OrderAndBanditryQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<OrderAndBanditryState> scope)
    {
        _ = scope;
    }

    private sealed class OrderAndBanditryQueries : IOrderAndBanditryQueries
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
            };
        }
    }
}
