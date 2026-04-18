using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed class FamilyCoreModule : ModuleRunner<FamilyCoreState>
{
    private static readonly string[] CommandNames =
    [
        "ArrangeMarriage",
        "DesignateHeirPolicy",
        "RedistributeHouseholdSupport",
    ];

    private static readonly string[] EventNames =
    [
        "ClanPrestigeAdjusted",
        "FamilyMembersAged",
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.FamilyCore;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.FamilyStructure;

    public override int ExecutionOrder => 300;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override FamilyCoreState CreateInitialState()
    {
        return new FamilyCoreState();
    }

    public override void RegisterQueries(FamilyCoreState state, QueryRegistry queries)
    {
        queries.Register<IFamilyCoreQueries>(new FamilyCoreQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<FamilyCoreState> scope)
    {
        IWorldSettlementsQueries settlementsQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();

        int peopleAged = 0;
        foreach (FamilyPersonState person in scope.State.People.OrderBy(static person => person.Id.Value))
        {
            if (!person.IsAlive)
            {
                continue;
            }

            person.AgeMonths += 1;
            peopleAged += 1;
        }

        if (peopleAged > 0)
        {
            scope.RecordDiff($"FamilyCore aged {peopleAged} living people by one month.");
            scope.Emit("FamilyMembersAged", $"Aged {peopleAged} living people.");
        }

        foreach (ClanStateData clan in scope.State.Clans.OrderBy(static clan => clan.Id.Value))
        {
            SettlementSnapshot homeSettlement = settlementsQueries.GetRequiredSettlement(clan.HomeSettlementId);
            int oldPrestige = clan.Prestige;
            int oldSupportReserve = clan.SupportReserve;

            int pressureScore = homeSettlement.Security + homeSettlement.Prosperity;
            if (pressureScore >= 130)
            {
                clan.Prestige = Math.Min(100, clan.Prestige + 1);
            }
            else if (pressureScore < 95)
            {
                clan.Prestige = Math.Max(0, clan.Prestige - 1);
            }

            clan.SupportReserve = Math.Clamp(clan.SupportReserve + ((homeSettlement.Prosperity - 50) / 10), 0, 100);

            if (clan.Prestige == oldPrestige && clan.SupportReserve == oldSupportReserve)
            {
                continue;
            }

            scope.RecordDiff(
                $"Clan {clan.ClanName} shifted to prestige {clan.Prestige} with support reserve {clan.SupportReserve}.",
                clan.Id.Value.ToString());
            scope.Emit("ClanPrestigeAdjusted", $"Clan {clan.ClanName} adjusted to local pressure.");
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<FamilyCoreState> scope)
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

            ClanStateData[] clans = scope.State.Clans
                .Where(clan => clan.HomeSettlementId == bundle.SettlementId)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();
            if (clans.Length == 0)
            {
                continue;
            }

            int prestigeDelta = ComputeCampaignPrestigeDelta(bundle, campaign);
            int supportDelta = ComputeCampaignSupportDelta(bundle, campaign);

            foreach (ClanStateData clan in clans)
            {
                clan.Prestige = Math.Clamp(clan.Prestige + prestigeDelta, 0, 100);
                clan.SupportReserve = Math.Clamp(clan.SupportReserve + supportDelta, 0, 100);
            }

            scope.RecordDiff(
                $"Campaign spillover around {campaign.AnchorSettlementName} shifted clan standing by prestige {prestigeDelta:+#;-#;0} and support {supportDelta:+#;-#;0}; {campaign.LastAftermathSummary}",
                bundle.SettlementId.Value.ToString());
            scope.Emit("ClanPrestigeAdjusted", $"Campaign spillover reshaped clan standing around {campaign.AnchorSettlementName}.", bundle.SettlementId.Value.ToString());
        }
    }

    private static int ComputeCampaignPrestigeDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = 0;
        if (campaign.MoraleState >= 62 && campaign.SupplyState >= 50 && !bundle.CampaignSupplyStrained)
        {
            delta += 1;
        }

        delta -= bundle.CampaignPressureRaised ? 1 : 0;
        delta -= bundle.CampaignSupplyStrained ? 2 : 0;
        delta -= bundle.CampaignAftermathRegistered ? 1 : 0;
        return Math.Clamp(delta, -3, 1);
    }

    private static int ComputeCampaignSupportDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = 0;
        delta -= bundle.CampaignMobilized ? 2 : 0;
        delta -= bundle.CampaignPressureRaised ? 1 : 0;
        delta -= bundle.CampaignSupplyStrained ? 2 : 0;
        delta -= bundle.CampaignAftermathRegistered ? 1 : 0;
        delta -= Math.Max(0, campaign.MobilizedForceCount - 24) / 24;
        return Math.Clamp(delta, -8, 0);
    }

    private sealed class FamilyCoreQueries : IFamilyCoreQueries
    {
        private readonly FamilyCoreState _state;

        public FamilyCoreQueries(FamilyCoreState state)
        {
            _state = state;
        }

        public ClanSnapshot GetRequiredClan(ClanId clanId)
        {
            ClanStateData clan = _state.Clans.Single(clan => clan.Id == clanId);
            return Clone(clan);
        }

        public IReadOnlyList<ClanSnapshot> GetClans()
        {
            return _state.Clans
                .OrderBy(static clan => clan.Id.Value)
                .Select(Clone)
                .ToArray();
        }

        private static ClanSnapshot Clone(ClanStateData clan)
        {
            return new ClanSnapshot
            {
                Id = clan.Id,
                ClanName = clan.ClanName,
                HomeSettlementId = clan.HomeSettlementId,
                Prestige = clan.Prestige,
                SupportReserve = clan.SupportReserve,
                HeirPersonId = clan.HeirPersonId,
            };
        }
    }
}
