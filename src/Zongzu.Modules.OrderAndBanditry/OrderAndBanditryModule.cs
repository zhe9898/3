using System;
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

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.OrderAndBanditry;

    public override int ModuleSchemaVersion => 1;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 700;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

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
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();

        ITradeAndIndustryQueries? tradeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)
            ? scope.GetRequiredQuery<ITradeAndIndustryQueries>()
            : null;
        IOfficeAndCareerQueries? officeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer)
            ? scope.GetRequiredQuery<IOfficeAndCareerQueries>()
            : null;
        IConflictAndForceQueries? conflictQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.ConflictAndForce)
            ? scope.GetRequiredQuery<IConflictAndForceQueries>()
            : null;

        IReadOnlyList<SettlementSnapshot> settlements = settlementQueries.GetSettlements();
        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();
        IReadOnlyList<ClanNarrativeSnapshot> narratives = socialQueries.GetClanNarratives();
        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = narratives.ToDictionary(static narrative => narrative.ClanId, static narrative => narrative);
        Dictionary<SettlementId, TradeActivitySnapshot> tradeActivityBySettlement = BuildTradeActivityBySettlement(tradeQueries);
        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionBySettlement = officeQueries is null
            ? new Dictionary<SettlementId, JurisdictionAuthoritySnapshot>()
            : officeQueries.GetJurisdictions().ToDictionary(static authority => authority.SettlementId, static authority => authority);
        Dictionary<SettlementId, LocalForcePoolSnapshot> forceBySettlement = conflictQueries is null
            ? new Dictionary<SettlementId, LocalForcePoolSnapshot>()
            : conflictQueries.GetSettlementForces().ToDictionary(static force => force.SettlementId, static force => force);

        foreach (SettlementSnapshot settlement in settlements.OrderBy(static settlement => settlement.Id.Value))
        {
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(settlement.Id);
            SettlementDisorderState disorder = GetOrCreateSettlement(scope.State, settlement.Id);

            ClanSnapshot[] localClans = clans
                .Where(clan => clan.HomeSettlementId == settlement.Id)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();
            int localFear = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.FearPressure);
            int localGrudge = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.GrudgePressure);

            TradeActivitySnapshot tradeActivity = tradeActivityBySettlement.TryGetValue(settlement.Id, out TradeActivitySnapshot? snapshot)
                ? snapshot
                : TradeActivitySnapshot.Empty;
            JurisdictionAuthoritySnapshot? jurisdiction = jurisdictionBySettlement.TryGetValue(settlement.Id, out JurisdictionAuthoritySnapshot? authority)
                ? authority
                : null;
            LocalForcePoolSnapshot? localForce = forceBySettlement.TryGetValue(settlement.Id, out LocalForcePoolSnapshot? forceSnapshot)
                ? forceSnapshot
                : null;
            bool hasActivatedResponse = localForce is not null && localForce.HasActiveConflict && localForce.IsResponseActivated;
            int forceSuppression = localForce is null ? 0 : ComputeForceSuppression(localForce);
            int administrativeRelief = jurisdiction is null ? 0 : ComputeAdministrativeRelief(jurisdiction);
            int activeEscortCount = hasActivatedResponse ? localForce!.EscortCount : 0;
            int activeMilitiaCount = hasActivatedResponse ? localForce!.MilitiaCount : 0;

            int previousBanditThreat = disorder.BanditThreat;
            int previousRoutePressure = disorder.RoutePressure;
            int previousDisorderPressure = disorder.DisorderPressure;
            int previousSuppressionDemand = disorder.SuppressionDemand;

            int banditDelta =
                (settlement.Security < 45 ? 2 : settlement.Security < 58 ? 1 : -1)
                + (population.CommonerDistress >= 60 ? 1 : population.CommonerDistress < 35 ? -1 : 0)
                + (population.MigrationPressure >= 50 ? 1 : 0)
                + (tradeActivity.AverageRouteRisk >= 55 ? 1 : 0)
                + (localFear >= 55 ? 1 : 0)
                + (forceSuppression >= 8 ? -2 : forceSuppression >= 4 ? -1 : 0)
                + (administrativeRelief >= 2 ? -1 : 0)
                + scope.Context.Random.NextInt(-1, 2);

            disorder.BanditThreat = Math.Clamp(disorder.BanditThreat + banditDelta, 0, 100);

            int routeDelta =
                (disorder.BanditThreat >= 50 ? 2 : disorder.BanditThreat >= 30 ? 1 : 0)
                + (tradeActivity.ActiveRouteCount > 0 ? 1 : 0)
                + (tradeActivity.TotalRouteCapacity >= 25 ? 1 : 0)
                + (tradeActivity.AverageRouteRisk >= 50 ? 1 : tradeActivity.AverageRouteRisk < 25 ? -1 : 0)
                + (settlement.Security < 50 ? 1 : 0)
                + (activeEscortCount >= 8 ? -2 : activeEscortCount >= 4 ? -1 : 0)
                + (administrativeRelief >= 2 ? -1 : 0)
                + scope.Context.Random.NextInt(-1, 2);

            disorder.RoutePressure = Math.Clamp(disorder.RoutePressure + routeDelta, 0, 100);

            int disorderDelta =
                (population.CommonerDistress >= 55 ? 1 : population.CommonerDistress < 35 ? -1 : 0)
                + (population.MigrationPressure >= 50 ? 1 : 0)
                + (localGrudge >= 55 ? 1 : 0)
                + (disorder.BanditThreat >= 60 ? 1 : 0)
                + (settlement.Security >= 65 ? -1 : 0)
                + (activeMilitiaCount >= 20 ? -1 : 0)
                - administrativeRelief
                + scope.Context.Random.NextInt(-1, 2);

            disorder.DisorderPressure = Math.Clamp(disorder.DisorderPressure + disorderDelta, 0, 100);
            disorder.SuppressionDemand = Math.Clamp(
                ((disorder.BanditThreat * 2) + disorder.RoutePressure + disorder.DisorderPressure + localFear + localGrudge) / 5
                - (population.MilitiaPotential / 20)
                - forceSuppression
                - administrativeRelief,
                0,
                100);

            disorder.LastPressureReason = BuildPressureReason(
                settlement,
                population,
                tradeActivity,
                localFear,
                localGrudge,
                localForce,
                forceSuppression,
                jurisdiction,
                administrativeRelief);

            if (previousBanditThreat == disorder.BanditThreat &&
                previousRoutePressure == disorder.RoutePressure &&
                previousDisorderPressure == disorder.DisorderPressure &&
                previousSuppressionDemand == disorder.SuppressionDemand)
            {
                continue;
            }

            scope.RecordDiff(
                $"{settlement.Name}地面不靖：盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。{disorder.LastPressureReason}",
                settlement.Id.Value.ToString());

            if (previousBanditThreat < 60 && disorder.BanditThreat >= 60)
            {
                scope.Emit("BanditThreatRaised", $"{settlement.Name}盗警骤起。");
            }

            if (previousDisorderPressure < 70 && disorder.DisorderPressure >= 70)
            {
                scope.Emit("OutlawGroupFormed", $"{settlement.Name}啸聚之势渐成。");
            }

            if (previousRoutePressure < 60 && disorder.RoutePressure >= 60)
            {
                scope.Emit("RouteUnsafeDueToBanditry", $"{settlement.Name}行路已不稳。");
            }

            if (previousSuppressionDemand >= 55 && disorder.SuppressionDemand <= 40)
            {
                scope.Emit("SuppressionSucceeded", $"{settlement.Name}镇压之需稍缓。");
            }
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<OrderAndBanditryState> scope)
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

            SettlementDisorderState disorder = GetOrCreateSettlement(scope.State, bundle.SettlementId);
            int previousBanditThreat = disorder.BanditThreat;
            int previousRoutePressure = disorder.RoutePressure;
            int previousDisorderPressure = disorder.DisorderPressure;
            int previousSuppressionDemand = disorder.SuppressionDemand;

            int banditDelta = bundle.CampaignAftermathRegistered ? 2 : 0;
            banditDelta += bundle.CampaignSupplyStrained ? 1 : 0;
            banditDelta += Math.Max(0, campaign.FrontPressure - 60) / 20;

            int routeDelta = bundle.CampaignMobilized ? 1 : 0;
            routeDelta += bundle.CampaignPressureRaised ? 2 : 0;
            routeDelta += bundle.CampaignSupplyStrained ? 4 : 0;
            routeDelta += bundle.CampaignAftermathRegistered ? 2 : 0;
            routeDelta += Math.Max(0, 50 - campaign.SupplyState) / 10;

            int disorderDelta = bundle.CampaignAftermathRegistered ? 3 : 0;
            disorderDelta += bundle.CampaignPressureRaised ? 1 : 0;
            disorderDelta += Math.Max(0, 50 - campaign.MoraleState) / 12;

            int suppressionDelta = bundle.CampaignMobilized ? 1 : 0;
            suppressionDelta += bundle.CampaignPressureRaised ? 2 : 0;
            suppressionDelta += bundle.CampaignSupplyStrained ? 3 : 0;
            suppressionDelta += bundle.CampaignAftermathRegistered ? 2 : 0;

            disorder.BanditThreat = Math.Clamp(disorder.BanditThreat + banditDelta, 0, 100);
            disorder.RoutePressure = Math.Clamp(disorder.RoutePressure + routeDelta, 0, 100);
            disorder.DisorderPressure = Math.Clamp(disorder.DisorderPressure + disorderDelta, 0, 100);
            disorder.SuppressionDemand = Math.Clamp(disorder.SuppressionDemand + suppressionDelta, 0, 100);
            disorder.LastPressureReason =
                $"{campaign.AnchorSettlementName}战事外溢，{campaign.FrontLabel}、{campaign.SupplyStateLabel}与{campaign.LastAftermathSummary}都压进了乡路巡哨。";

            if (previousBanditThreat == disorder.BanditThreat
                && previousRoutePressure == disorder.RoutePressure
                && previousDisorderPressure == disorder.DisorderPressure
                && previousSuppressionDemand == disorder.SuppressionDemand)
            {
                continue;
            }

            scope.RecordDiff(
                $"{campaign.AnchorSettlementName}战事外溢：盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。{disorder.LastPressureReason}",
                bundle.SettlementId.Value.ToString());

            if (previousBanditThreat < 60 && disorder.BanditThreat >= 60)
            {
                scope.Emit("BanditThreatRaised", $"战事外溢使{campaign.AnchorSettlementName}盗警更紧。", bundle.SettlementId.Value.ToString());
            }

            if (previousDisorderPressure < 70 && disorder.DisorderPressure >= 70)
            {
                scope.Emit("OutlawGroupFormed", $"战事外溢使{campaign.AnchorSettlementName}啸聚更成形。", bundle.SettlementId.Value.ToString());
            }

            if (previousRoutePressure < 60 && disorder.RoutePressure >= 60)
            {
                scope.Emit("RouteUnsafeDueToBanditry", $"战事外溢使{campaign.AnchorSettlementName}行路更险。", bundle.SettlementId.Value.ToString());
            }
        }
    }

    private static Dictionary<SettlementId, TradeActivitySnapshot> BuildTradeActivityBySettlement(ITradeAndIndustryQueries? tradeQueries)
    {
        if (tradeQueries is null)
        {
            return new Dictionary<SettlementId, TradeActivitySnapshot>();
        }

        Dictionary<SettlementId, List<TradeRouteSnapshot>> routesBySettlement = new();
        foreach (ClanTradeSnapshot clanTrade in tradeQueries.GetClanTrades().OrderBy(static trade => trade.ClanId.Value))
        {
            foreach (TradeRouteSnapshot route in tradeQueries.GetRoutesForClan(clanTrade.ClanId)
                         .Where(static route => route.IsActive)
                         .OrderBy(static route => route.RouteId))
            {
                if (!routesBySettlement.TryGetValue(route.SettlementId, out List<TradeRouteSnapshot>? routes))
                {
                    routes = [];
                    routesBySettlement[route.SettlementId] = routes;
                }

                routes.Add(route);
            }
        }

        return routesBySettlement.ToDictionary(
            static pair => pair.Key,
            static pair =>
            {
                int averageRouteRisk = pair.Value.Count == 0 ? 0 : pair.Value.Sum(static route => route.Risk) / pair.Value.Count;
                int totalRouteCapacity = pair.Value.Sum(static route => route.Capacity);
                return new TradeActivitySnapshot(pair.Value.Count, averageRouteRisk, totalRouteCapacity);
            });
    }

    private static int AverageNarrative(
        IReadOnlyList<ClanSnapshot> clans,
        IReadOnlyDictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan,
        Func<ClanNarrativeSnapshot, int> selector)
    {
        int total = 0;
        int count = 0;
        foreach (ClanSnapshot clan in clans)
        {
            if (!narrativesByClan.TryGetValue(clan.Id, out ClanNarrativeSnapshot? narrative))
            {
                continue;
            }

            total += selector(narrative);
            count += 1;
        }

        return count == 0 ? 0 : total / count;
    }

    private static SettlementDisorderState GetOrCreateSettlement(OrderAndBanditryState state, SettlementId settlementId)
    {
        SettlementDisorderState? settlement = state.Settlements.SingleOrDefault(existing => existing.SettlementId == settlementId);
        if (settlement is not null)
        {
            return settlement;
        }

        settlement = new SettlementDisorderState
        {
            SettlementId = settlementId,
        };
        state.Settlements.Add(settlement);
        state.Settlements = state.Settlements.OrderBy(static entry => entry.SettlementId.Value).ToList();
        return settlement;
    }

    private static string BuildPressureReason(
        SettlementSnapshot settlement,
        PopulationSettlementSnapshot population,
        TradeActivitySnapshot tradeActivity,
        int localFear,
        int localGrudge,
        LocalForcePoolSnapshot? localForce,
        int forceSuppression,
        JurisdictionAuthoritySnapshot? jurisdiction,
        int administrativeRelief)
    {
        List<string> reasons = [];

        if (settlement.Security < 50)
        {
            reasons.Add($"乡面安宁仅{settlement.Security}，仓路与铺户更易外露。");
        }

        if (population.CommonerDistress >= 55)
        {
            reasons.Add($"民困{population.CommonerDistress}，乡里窘急渐聚。");
        }

        if (localForce is not null && forceSuppression > 0)
        {
            reasons.Add($"已激活的守丁{localForce.GuardCount}、护运{localForce.EscortCount}、整备{localForce.Readiness}与应援{localForce.OrderSupportLevel}，正缓住事势。");
        }

        if (jurisdiction is not null && administrativeRelief > 0)
        {
            reasons.Add($"{jurisdiction.LeadOfficeTitle}乡面杠力{jurisdiction.JurisdictionLeverage}，可替镇压之需卸去{administrativeRelief}分。");
        }

        if (population.MigrationPressure >= 45)
        {
            reasons.Add($"流徙之压{population.MigrationPressure}，乡面人气渐散。");
        }

        if (tradeActivity.ActiveRouteCount > 0)
        {
            reasons.Add($"现有{tradeActivity.ActiveRouteCount}条活路、载力{tradeActivity.TotalRouteCapacity}，行旅财货尽在外露。");
        }

        if (tradeActivity.AverageRouteRisk >= 45)
        {
            reasons.Add($"路险均值{tradeActivity.AverageRouteRisk}，最招乘隙劫掠。");
        }

        if (localFear >= 50 || localGrudge >= 55)
        {
            reasons.Add($"乡里惧意{localFear}、旧怨{localGrudge}，渐化为不靖之势。");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("本月乡里大势尚能按住。");
        }

        return string.Join(" ", reasons.Take(3));
    }

    private static int ComputeForceSuppression(LocalForcePoolSnapshot localForce)
    {
        if (!localForce.HasActiveConflict || !localForce.IsResponseActivated || localForce.OrderSupportLevel <= 0)
        {
            return 0;
        }

        return localForce.OrderSupportLevel;
    }

    private static int ComputeAdministrativeRelief(JurisdictionAuthoritySnapshot jurisdiction)
    {
        int relief = 0;
        if (jurisdiction.JurisdictionLeverage >= 55)
        {
            relief += 2;
        }
        else if (jurisdiction.JurisdictionLeverage >= 28)
        {
            relief += 1;
        }

        if (jurisdiction.PetitionPressure >= 60)
        {
            relief -= 1;
        }

        return Math.Max(0, relief);
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

    private sealed record TradeActivitySnapshot(int ActiveRouteCount, int AverageRouteRisk, int TotalRouteCapacity)
    {
        public static readonly TradeActivitySnapshot Empty = new(0, 0, 0);
    }
}
