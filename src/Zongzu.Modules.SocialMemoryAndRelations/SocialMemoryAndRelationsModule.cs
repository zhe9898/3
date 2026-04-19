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

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.SocialMemoryAndRelations;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.SocialMemory;

    public override int ExecutionOrder => 400;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

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
            narrative.ShamePressure = Math.Clamp(narrative.ShamePressure + ComputeShameDelta(clan), 0, 100);
            narrative.FavorBalance = Math.Clamp(narrative.FavorBalance + ComputeFavorDelta(clan), -100, 100);
            narrative.PublicNarrative = BuildNarrativeText(clan, averageDistress, narrative.GrudgePressure);

            scope.RecordDiff(
                $"{clan.ClanName}宗中旧怨{narrative.GrudgePressure}，惧意{narrative.FearPressure}，羞压{narrative.ShamePressure}。",
                clan.Id.Value.ToString());

            if (previousGrudge < 60 && narrative.GrudgePressure >= 60)
            {
                scope.Emit("GrudgeEscalated", $"{clan.ClanName}旧怨更深。");
                AddMemory(scope.State, clan.Id, "hardship", narrative.GrudgePressure, true, $"{clan.ClanName}因民困与家计艰难，旧怨更深。", scope.Context);
            }

            if (previousGrudge >= 45 && narrative.GrudgePressure < 45)
            {
                scope.Emit("GrudgeSoftened", $"{clan.ClanName}旧怨稍缓。");
                AddMemory(scope.State, clan.Id, "conciliation", Math.Max(10, narrative.FavorBalance), true, $"{clan.ClanName}因接济与调处，怨气稍缓。", scope.Context);
            }

            if (previousFavor < 10 && narrative.FavorBalance >= 10)
            {
                scope.Emit("FavorIncurred", $"{clan.ClanName}人情债渐著。");
            }

            scope.Emit("ClanNarrativeUpdated", $"{clan.ClanName}乡议口气有变。");
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<SocialMemoryAndRelationsState> scope)
    {
        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        ClanSnapshot[] clans = familyQueries.GetClans()
            .OrderBy(static clan => clan.Id.Value)
            .ToArray();

        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            ClanSnapshot[] localClans = clans
                .Where(clan => clan.HomeSettlementId == bundle.SettlementId)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();

            foreach (ClanSnapshot clan in localClans)
            {
                ClanNarrativeState narrative = GetOrCreateNarrative(scope.State, clan.Id);
                int previousGrudge = narrative.GrudgePressure;

                narrative.FearPressure = Math.Clamp(narrative.FearPressure + ComputeCampaignFearDelta(bundle, campaign), 0, 100);
                narrative.GrudgePressure = Math.Clamp(narrative.GrudgePressure + ComputeCampaignGrudgeDelta(bundle, campaign), 0, 100);
                narrative.ShamePressure = Math.Clamp(narrative.ShamePressure + (bundle.CampaignAftermathRegistered ? 2 : 1), 0, 100);
                narrative.FavorBalance = Math.Clamp(narrative.FavorBalance - (bundle.CampaignSupplyStrained ? 2 : 1), -100, 100);
                narrative.PublicNarrative = BuildCampaignNarrativeText(clan, campaign, narrative);

                scope.RecordDiff(
                    $"{campaign.AnchorSettlementName}战后余波牵得{clan.ClanName}惧意{narrative.FearPressure}，旧怨{narrative.GrudgePressure}，羞压{narrative.ShamePressure}。",
                    clan.Id.Value.ToString());

                AddMemory(
                    scope.State,
                    clan.Id,
                    bundle.CampaignAftermathRegistered ? "campaign-aftermath" : "campaign-pressure",
                    Math.Max(narrative.FearPressure, narrative.GrudgePressure),
                    true,
                    $"{campaign.AnchorSettlementName}战后余波把{campaign.FrontLabel}与{campaign.LastAftermathSummary}一并压进了{clan.ClanName}的旧忆里。",
                    scope.Context);

                if (previousGrudge < 60 && narrative.GrudgePressure >= 60)
                {
                    scope.Emit("GrudgeEscalated", $"战后余波使{clan.ClanName}旧怨更炽。", clan.Id.Value.ToString());
                }
            }
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

        if (clan.BranchTension >= 60)
        {
            delta += 2;
        }
        else if (clan.BranchTension >= 40)
        {
            delta += 1;
        }

        if (clan.ReliefSanctionPressure >= 45)
        {
            delta += 1;
        }

        if (clan.MediationMomentum >= 55)
        {
            delta -= 2;
        }
        else if (clan.MediationMomentum >= 28)
        {
            delta -= 1;
        }

        return delta;
    }

    private static int ComputeShameDelta(ClanSnapshot clan)
    {
        int delta = clan.Prestige < 45 ? 1 : clan.Prestige > 58 ? -1 : 0;
        delta += clan.BranchFavorPressure >= 40 ? 1 : 0;
        delta += clan.SeparationPressure >= 60 ? 1 : 0;
        delta -= clan.MediationMomentum >= 50 ? 1 : 0;
        return delta;
    }

    private static int ComputeFavorDelta(ClanSnapshot clan)
    {
        int delta = clan.SupportReserve >= 65 ? 1 : clan.SupportReserve < 40 ? -1 : 0;
        delta += clan.MediationMomentum >= 45 ? 1 : 0;
        delta -= clan.ReliefSanctionPressure >= 40 ? 2 : 0;
        return delta;
    }

    private static int ComputeCampaignFearDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignPressureRaised ? 2 : 0;
        delta += bundle.CampaignSupplyStrained ? 3 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignGrudgeDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignPressureRaised ? 1 : 0;
        delta += bundle.CampaignSupplyStrained ? 2 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, 45 - campaign.MoraleState) / 14;
        return Math.Max(1, delta);
    }

    private static string BuildCampaignNarrativeText(ClanSnapshot clan, CampaignFrontSnapshot campaign, ClanNarrativeState narrative)
    {
        if (narrative.GrudgePressure >= 70)
        {
            return $"{campaign.AnchorSettlementName}兵后余波未歇，{clan.ClanName}宗中怨望愈深。";
        }

        if (narrative.FearPressure >= 60)
        {
            return $"{campaign.AnchorSettlementName}前线余压未退，{clan.ClanName}宗中人心惴惴。";
        }

        return $"{clan.ClanName}仍记得{campaign.AnchorSettlementName}战后诸案与里中劳瘁。";
    }

    private static string BuildNarrativeText(ClanSnapshot clan, int averageDistress, int grudgePressure)
    {
        if (clan.SeparationPressure >= 65)
        {
            return $"{clan.ClanName}宗中分房之议渐炽。";
        }

        if (clan.BranchTension >= 60)
        {
            return $"{clan.ClanName}祠堂争声渐炽。";
        }

        if (clan.MediationMomentum >= 45)
        {
            return $"{clan.ClanName}正请族老压住争端。";
        }

        if (grudgePressure >= 70)
        {
            return $"{clan.ClanName}宗中旧怨益深。";
        }

        if (averageDistress >= 60)
        {
            return $"{clan.ClanName}正受民户困顿所逼。";
        }

        if (clan.SupportReserve >= 65)
        {
            return $"{clan.ClanName}尚能以接济稳住门里人心。";
        }

        return $"{clan.ClanName}仍在乡里目光之下。";
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
