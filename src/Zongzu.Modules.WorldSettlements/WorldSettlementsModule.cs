using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

public sealed class WorldSettlementsModule : ModuleRunner<WorldSettlementsState>
{
    private static readonly string[] EventNames = ["SettlementPressureChanged"];

    public override string ModuleKey => KnownModuleKeys.WorldSettlements;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.WorldBaseline;

    public override int ExecutionOrder => 100;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override WorldSettlementsState CreateInitialState()
    {
        return new WorldSettlementsState();
    }

    public override void RegisterQueries(WorldSettlementsState state, QueryRegistry queries)
    {
        queries.Register<IWorldSettlementsQueries>(new WorldSettlementsQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<WorldSettlementsState> scope)
    {
        foreach (SettlementStateData settlement in scope.State.Settlements.OrderBy(static settlement => settlement.Id.Value))
        {
            int oldSecurity = settlement.Security;
            int oldProsperity = settlement.Prosperity;

            settlement.Security = Math.Clamp(settlement.Security + scope.Context.Random.NextInt(-2, 3), 0, 100);
            settlement.Prosperity = Math.Clamp(settlement.Prosperity + scope.Context.Random.NextInt(-1, 2), 0, 100);

            if (settlement.Security == oldSecurity && settlement.Prosperity == oldProsperity)
            {
                continue;
            }

            scope.RecordDiff(
                $"Settlement {settlement.Name} pressure shifted to security {settlement.Security} and prosperity {settlement.Prosperity}.",
                settlement.Id.Value.ToString());
            scope.Emit("SettlementPressureChanged", $"Settlement {settlement.Name} pressure changed.");
        }
    }

    private sealed class WorldSettlementsQueries : IWorldSettlementsQueries
    {
        private readonly WorldSettlementsState _state;

        public WorldSettlementsQueries(WorldSettlementsState state)
        {
            _state = state;
        }

        public SettlementSnapshot GetRequiredSettlement(SettlementId settlementId)
        {
            SettlementStateData settlement = _state.Settlements
                .Single(settlement => settlement.Id == settlementId);

            return Clone(settlement);
        }

        public IReadOnlyList<SettlementSnapshot> GetSettlements()
        {
            return _state.Settlements
                .OrderBy(static settlement => settlement.Id.Value)
                .Select(Clone)
                .ToArray();
        }

        private static SettlementSnapshot Clone(SettlementStateData settlement)
        {
            return new SettlementSnapshot
            {
                Id = settlement.Id,
                Name = settlement.Name,
                Security = settlement.Security,
                Prosperity = settlement.Prosperity,
            };
        }
    }
}
