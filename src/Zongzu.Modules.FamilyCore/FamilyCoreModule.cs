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

    public override string ModuleKey => KnownModuleKeys.FamilyCore;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.FamilyStructure;

    public override int ExecutionOrder => 300;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

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
