using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.SocialMemoryAndRelations;

public sealed class SocialMemoryAndRelationsModule : ModuleRunner<SocialMemoryAndRelationsState>
{
    private static readonly string[] CommandNames =
    [
        "Apologize",
        "Compensate",
        "RestrainRetaliation",
        "PubliclyHonorOrShame",
    ];

    private static readonly string[] EventNames =
    [
        "GrudgeEscalated",
        "GrudgeSoftened",
        "FavorIncurred",
        "ClanNarrativeUpdated",
    ];

    public override string ModuleKey => KnownModuleKeys.SocialMemoryAndRelations;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.SocialMemory;

    public override int ExecutionOrder => 400;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override SocialMemoryAndRelationsState CreateInitialState()
    {
        return new SocialMemoryAndRelationsState();
    }

    public override void RegisterQueries(SocialMemoryAndRelationsState state, QueryRegistry queries)
    {
        queries.Register<ISocialMemoryAndRelationsQueries>(new SocialMemoryQueries(state));
    }

    public override void RunMonth(ModuleExecutionScope<SocialMemoryAndRelationsState> scope)
    {
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();

        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();
        IReadOnlyList<HouseholdPressureSnapshot> households = populationQueries.GetHouseholds();

        foreach (ClanSnapshot clan in clans.OrderBy(static clan => clan.Id.Value))
        {
            ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clan.Id);
            HouseholdPressureSnapshot[] sponsoredHouseholds = households
                .Where(household => household.SponsorClanId == clan.Id)
                .OrderBy(household => household.Id.Value)
                .ToArray();

            int previousGrudge = narrative.GrudgePressure;
            int previousFavor = narrative.FavorBalance;

            int averageDistress = sponsoredHouseholds.Length == 0 ? 0 : sponsoredHouseholds.Sum(static household => household.Distress) / sponsoredHouseholds.Length;

            narrative.GrudgePressure = Math.Clamp(narrative.GrudgePressure + ComputeGrudgeDelta(clan, averageDistress), 0, 100);
            narrative.FearPressure = Math.Clamp(narrative.FearPressure + (averageDistress >= 70 ? 1 : averageDistress < 45 ? -1 : 0), 0, 100);
            narrative.ShamePressure = Math.Clamp(narrative.ShamePressure + (clan.Prestige < 45 ? 1 : clan.Prestige > 58 ? -1 : 0), 0, 100);
            narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + (clan.SupportReserve >= 65 ? 1 : clan.SupportReserve < 40 ? -1 : 0), -100, 100);
            narrative.PublicNarrative = BuildNarrativeText(clan, averageDistress, narrative.GrudgePressure);

            scope.RecordDiff(
                $"Clan {clan.ClanName} narrative shifted to grudge {narrative.GrudgePressure}, fear {narrative.FearPressure}, shame {narrative.ShamePressure}.",
                clan.Id.Value.ToString());

            if (previousGrudge < 60 && narrative.GrudgePressure >= 60)
            {
                scope.Emit("GrudgeEscalated", $"Clan {clan.ClanName} grievance pressure escalated.");
                AddMemory(scope.State, clan.Id, "hardship", narrative.GrudgePressure, true, $"Household hardship hardened grievances around clan {clan.ClanName}.", scope.Context);
            }

            if (previousGrudge >= 45 && narrative.GrudgePressure < 45)
            {
                scope.Emit("GrudgeSoftened", $"Clan {clan.ClanName} grievance pressure softened.");
                AddMemory(scope.State, clan.Id, "conciliation", Math.Max(10, narrative.FavorBalance), true, $"Material relief softened resentment around clan {clan.ClanName}.", scope.Context);
            }

            if (previousFavor < 10 && narrative.FavorBalance >= 10)
            {
                scope.Emit("FavorIncurred", $"Clan {clan.ClanName} accrued visible obligations.");
            }

            scope.Emit("ClanNarrativeUpdated", $"Clan {clan.ClanName} narrative updated.");
        }
    }

    private static ClanNarrativeState GetOrCreateNarrative(SocialMemoryAndRelationsState state, ClanId clanId)
    {
        ClanNarrativeState? narrative = state.ClanNarratives.SingleOrDefault(existing => existing.ClanId == clanId);
        if (narrative is not null)
        {
            return narrative;
        }

        narrative = new ClanNarrativeState { ClanId = clanId };
        state.ClanNarratives.Add(narrative);
        state.ClanNarratives = state.ClanNarratives.OrderBy(static entry => entry.ClanId.Value).ToList();
        return narrative;
    }

    private static int ComputeGrudgeDelta(ClanSnapshot clan, int averageDistress)
    {
        int delta = 0;
        if (averageDistress >= 75)
        {
            delta += 2;
        }
        else if (averageDistress >= 55)
        {
            delta += 1;
        }
        else if (averageDistress < 40)
        {
            delta -= 1;
        }

        if (clan.Prestige < 45)
        {
            delta += 1;
        }

        if (clan.SupportReserve >= 70)
        {
            delta -= 1;
        }

        return delta;
    }

    private static string BuildNarrativeText(ClanSnapshot clan, int averageDistress, int grudgePressure)
    {
        if (grudgePressure >= 70)
        {
            return $"Resentment is hardening around clan {clan.ClanName}.";
        }

        if (averageDistress >= 60)
        {
            return $"Household strain is testing clan {clan.ClanName}.";
        }

        if (clan.SupportReserve >= 65)
        {
            return $"Clan {clan.ClanName} is seen as holding the line through relief.";
        }

        return $"Clan {clan.ClanName} remains under watchful local memory.";
    }

    private static void AddMemory(
        SocialMemoryAndRelationsState state,
        ClanId clanId,
        string kind,
        int intensity,
        bool isPublic,
        string summary,
        ModuleExecutionContext context)
    {
        bool alreadyRecordedThisMonth = state.Memories.Any(memory =>
            memory.SubjectClanId == clanId &&
            string.Equals(memory.Kind, kind, StringComparison.Ordinal) &&
            memory.CreatedAt.Year == context.CurrentDate.Year &&
            memory.CreatedAt.Month == context.CurrentDate.Month);

        if (alreadyRecordedThisMonth)
        {
            return;
        }

        state.Memories.Add(new MemoryRecordState
        {
            Id = KernelIdAllocator.NextMemory(context.KernelState),
            SubjectClanId = clanId,
            Kind = kind,
            Intensity = Math.Clamp(intensity, 0, 100),
            IsPublic = isPublic,
            CreatedAt = context.CurrentDate,
            Summary = summary,
        });
    }

    private sealed class SocialMemoryQueries : ISocialMemoryAndRelationsQueries
    {
        private readonly SocialMemoryAndRelationsState _state;

        public SocialMemoryQueries(SocialMemoryAndRelationsState state)
        {
            _state = state;
        }

        public ClanNarrativeSnapshot GetRequiredClanNarrative(ClanId clanId)
        {
            ClanNarrativeState narrative = _state.ClanNarratives.Single(narrative => narrative.ClanId == clanId);
            return CloneNarrative(narrative, _state.Memories);
        }

        public IReadOnlyList<ClanNarrativeSnapshot> GetClanNarratives()
        {
            return _state.ClanNarratives
                .OrderBy(static narrative => narrative.ClanId.Value)
                .Select(narrative => CloneNarrative(narrative, _state.Memories))
                .ToArray();
        }

        private static ClanNarrativeSnapshot CloneNarrative(ClanNarrativeState narrative, IReadOnlyCollection<MemoryRecordState> memories)
        {
            int memoryCount = memories.Count(memory => memory.SubjectClanId == narrative.ClanId);
            return new ClanNarrativeSnapshot
            {
                ClanId = narrative.ClanId,
                PublicNarrative = narrative.PublicNarrative,
                GrudgePressure = narrative.GrudgePressure,
                FearPressure = narrative.FearPressure,
                ShamePressure = narrative.ShamePressure,
                FavorBalance = narrative.FavorBalance,
                MemoryCount = memoryCount,
            };
        }
    }
}
