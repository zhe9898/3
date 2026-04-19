using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.WorldSettlements;

public sealed class WorldSettlementsModule : ModuleRunner<WorldSettlementsState>
{
    private static readonly string[] EventNames = ["SettlementPressureChanged"];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.WorldSettlements;

    public override int ModuleSchemaVersion => 2;

    public override SimulationPhase Phase => SimulationPhase.WorldBaseline;

    public override int ExecutionOrder => 100;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

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
                $"{settlement.Name}乡面安宁{settlement.Security}，丰实{settlement.Prosperity}。",
                settlement.Id.Value.ToString());
            scope.Emit("SettlementPressureChanged", $"{settlement.Name}乡面安宁与丰实有变。");
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<WorldSettlementsState> scope)
    {
        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            SettlementStateData settlement = scope.State.Settlements.SingleOrDefault(existing => existing.Id == bundle.SettlementId)
                ?? throw new InvalidOperationException($"Settlement {bundle.SettlementId.Value} was not found for warfare fallout.");
            int previousSecurity = settlement.Security;
            int previousProsperity = settlement.Prosperity;

            int securityDelta = ComputeCampaignSecurityDelta(bundle, campaign);
            int prosperityDelta = ComputeCampaignProsperityDelta(bundle, campaign);

            settlement.Security = Math.Clamp(settlement.Security - securityDelta, 0, 100);
            settlement.Prosperity = Math.Clamp(settlement.Prosperity - prosperityDelta, 0, 100);

            if (previousSecurity == settlement.Security && previousProsperity == settlement.Prosperity)
            {
                continue;
            }

            scope.RecordDiff(
                $"{settlement.Name}受战后余波所压，安宁减{securityDelta}，丰实减{prosperityDelta}；{campaign.FrontLabel}、{campaign.SupplyStateLabel}，{campaign.LastAftermathSummary}",
                settlement.Id.Value.ToString());
            scope.Emit("SettlementPressureChanged", $"{settlement.Name}受战后余波，乡面气象有变。", settlement.Id.Value.ToString());
        }
    }

    private static int ComputeCampaignSecurityDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignPressureRaised ? 2 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 60) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignProsperityDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignSupplyStrained ? 3 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, 55 - campaign.SupplyState) / 16;
        return Math.Max(1, delta);
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
                Tier = settlement.Tier,
                Security = settlement.Security,
                Prosperity = settlement.Prosperity,
            };
        }
    }
}
