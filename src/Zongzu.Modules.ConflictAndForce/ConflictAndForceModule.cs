using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed class ConflictAndForceModule : ModuleRunner<ConflictAndForceState>
{
    private static readonly string[] CommandNames =
    [
        "HireGuards",
        "MobilizeClanMilitia",
        "PrepareEscort",
        "RestrainRetaliation",
    ];

    private static readonly string[] EventNames =
    [
        "ConflictResolved",
        "CommanderWounded",
        "ForceReadinessChanged",
        "MilitiaMobilized",
    ];

    public override string ModuleKey => KnownModuleKeys.ConflictAndForce;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.EventHandling;

    public override int ExecutionOrder => 200;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override ConflictAndForceState CreateInitialState()
    {
        return new ConflictAndForceState();
    }

    public override void RegisterQueries(ConflictAndForceState state, QueryRegistry queries)
    {
        queries.Register<IConflictAndForceQueries>(new ConflictAndForceQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<ConflictAndForceState> scope)
    {
        _ = scope;
    }

    private sealed class ConflictAndForceQueries : IConflictAndForceQueries
    {
        private readonly ConflictAndForceState _state;

        public ConflictAndForceQueries(ConflictAndForceState state)
        {
            _state = state;
        }

        public LocalForcePoolSnapshot GetRequiredSettlementForce(SettlementId settlementId)
        {
            SettlementForceState settlement = _state.Settlements.Single(settlement => settlement.SettlementId == settlementId);
            return Clone(settlement);
        }

        public IReadOnlyList<LocalForcePoolSnapshot> GetSettlementForces()
        {
            return _state.Settlements
                .OrderBy(static settlement => settlement.SettlementId.Value)
                .Select(Clone)
                .ToArray();
        }

        private static LocalForcePoolSnapshot Clone(SettlementForceState settlement)
        {
            return new LocalForcePoolSnapshot
            {
                SettlementId = settlement.SettlementId,
                GuardCount = settlement.GuardCount,
                RetainerCount = settlement.RetainerCount,
                MilitiaCount = settlement.MilitiaCount,
                EscortCount = settlement.EscortCount,
                Readiness = settlement.Readiness,
                CommandCapacity = settlement.CommandCapacity,
                LastConflictTrace = settlement.LastConflictTrace,
            };
        }
    }
}
