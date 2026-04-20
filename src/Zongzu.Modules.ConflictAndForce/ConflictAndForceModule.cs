using System;
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

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.ConflictAndForce;

    public override int ModuleSchemaVersion => 3;

    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;

    public override int ExecutionOrder => 650;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;

    public override FeatureMode DefaultMode => FeatureMode.Lite;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override ConflictAndForceState CreateInitialState()
    {
        return new ConflictAndForceState();
    }

    public override void RegisterQueries(ConflictAndForceState state, QueryRegistry queries)
    {
        queries.Register<IConflictAndForceQueries>(new ConflictAndForceQueries(state));
    }

    public override void RunXun(ModuleExecutionScope<ConflictAndForceState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();

        IOrderAndBanditryQueries? orderQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)
            ? scope.GetRequiredQuery<IOrderAndBanditryQueries>()
            : null;
        IOfficeAndCareerQueries? officeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer)
            ? scope.GetRequiredQuery<IOfficeAndCareerQueries>()
            : null;
        ITradeAndIndustryQueries? tradeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)
            ? scope.GetRequiredQuery<ITradeAndIndustryQueries>()
            : null;

        IReadOnlyList<SettlementSnapshot> settlements = settlementQueries.GetSettlements();
        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();
        IReadOnlyList<ClanNarrativeSnapshot> narratives = socialQueries.GetClanNarratives();

        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = narratives.ToDictionary(
            static narrative => narrative.ClanId,
            static narrative => narrative);
        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = orderQueries is null
            ? new Dictionary<SettlementId, SettlementDisorderSnapshot>()
            : orderQueries.GetSettlementDisorder().ToDictionary(static disorder => disorder.SettlementId, static disorder => disorder);
        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionBySettlement = officeQueries is null
            ? new Dictionary<SettlementId, JurisdictionAuthoritySnapshot>()
            : officeQueries.GetJurisdictions().ToDictionary(static jurisdiction => jurisdiction.SettlementId, static jurisdiction => jurisdiction);
        Dictionary<SettlementId, TradeActivitySnapshot> tradeActivityBySettlement = BuildTradeActivityBySettlement(tradeQueries);

        foreach (SettlementSnapshot settlement in settlements.OrderBy(static settlement => settlement.Id.Value))
        {
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(settlement.Id);
            SettlementForceState force = GetOrCreateSettlement(scope.State, settlement.Id);

            ClanSnapshot[] localClans = clans
                .Where(clan => clan.HomeSettlementId == settlement.Id)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();
            int averagePrestige = AverageClanValue(localClans, static clan => clan.Prestige);
            int averageSupport = AverageClanValue(localClans, static clan => clan.SupportReserve);
            int localGrudge = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.GrudgePressure);
            int localFear = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.FearPressure);

            SettlementDisorderSnapshot disorder = disorderBySettlement.TryGetValue(settlement.Id, out SettlementDisorderSnapshot? disorderSnapshot)
                ? disorderSnapshot
                : EmptyDisorderSnapshot.For(settlement.Id);
            JurisdictionAuthoritySnapshot? jurisdiction = jurisdictionBySettlement.TryGetValue(settlement.Id, out JurisdictionAuthoritySnapshot? authority)
                ? authority
                : null;
            int administrativeSupport = jurisdiction is null ? 0 : ComputeAdministrativeSupport(jurisdiction);
            TradeActivitySnapshot tradeActivity = tradeActivityBySettlement.TryGetValue(settlement.Id, out TradeActivitySnapshot? tradeSnapshot)
                ? tradeSnapshot
                : TradeActivitySnapshot.Empty;

            bool previousHasActiveConflict = force.HasActiveConflict;

            switch (scope.Context.CurrentXun)
            {
                case SimulationXun.Shangxun:
                    ApplyXunOpeningPosture(force, settlement, population, disorder, tradeActivity, averageSupport, administrativeSupport);
                    break;
                case SimulationXun.Zhongxun:
                    ApplyXunHotspotPulse(force, settlement, disorder, tradeActivity, localFear, localGrudge, averagePrestige, administrativeSupport);
                    break;
                case SimulationXun.Xiaxun:
                    ApplyXunClosingPosture(force, settlement, population, disorder, tradeActivity, localFear, localGrudge, administrativeSupport);
                    break;
            }

            int conflictPressure =
                disorder.BanditThreat
                + disorder.RoutePressure
                + disorder.DisorderPressure
                + localGrudge
                + (population.CommonerDistress / 2)
                + tradeActivity.AverageRouteRisk;
            int forcePosture = force.Readiness + force.CommandCapacity + force.GuardCount + force.EscortCount;
            int responseActivationLevel = ConflictAndForceResponseStateCalculator.ComputeResponseActivationLevel(force);
            force.HasActiveConflict = scope.Context.CurrentXun switch
            {
                SimulationXun.Shangxun => DetermineXunCarryoverConflict(
                    previousHasActiveConflict,
                    disorder,
                    localGrudge,
                    localFear),
                _ => DetermineXunEscalatingConflict(
                    previousHasActiveConflict,
                    responseActivationLevel,
                    disorder,
                    localGrudge,
                    localFear,
                    conflictPressure,
                    forcePosture),
            };
            ConflictAndForceResponseStateCalculator.Refresh(force);
            force.LastConflictTrace = BuildXunConflictTrace(
                settlement,
                disorder,
                tradeActivity,
                force,
                localFear,
                localGrudge,
                jurisdiction);
        }
    }

    public override void RunMonth(ModuleExecutionScope<ConflictAndForceState> scope)
    {
        IWorldSettlementsQueries settlementQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        IFamilyCoreQueries familyQueries = scope.GetRequiredQuery<IFamilyCoreQueries>();
        ISocialMemoryAndRelationsQueries socialQueries = scope.GetRequiredQuery<ISocialMemoryAndRelationsQueries>();

        IOrderAndBanditryQueries? orderQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry)
            ? scope.GetRequiredQuery<IOrderAndBanditryQueries>()
            : null;
        IOfficeAndCareerQueries? officeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer)
            ? scope.GetRequiredQuery<IOfficeAndCareerQueries>()
            : null;
        ITradeAndIndustryQueries? tradeQueries = scope.Context.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry)
            ? scope.GetRequiredQuery<ITradeAndIndustryQueries>()
            : null;

        IReadOnlyList<SettlementSnapshot> settlements = settlementQueries.GetSettlements();
        IReadOnlyList<ClanSnapshot> clans = familyQueries.GetClans();
        IReadOnlyList<ClanNarrativeSnapshot> narratives = socialQueries.GetClanNarratives();

        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = narratives.ToDictionary(
            static narrative => narrative.ClanId,
            static narrative => narrative);
        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = orderQueries is null
            ? new Dictionary<SettlementId, SettlementDisorderSnapshot>()
            : orderQueries.GetSettlementDisorder().ToDictionary(static disorder => disorder.SettlementId, static disorder => disorder);
        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionBySettlement = officeQueries is null
            ? new Dictionary<SettlementId, JurisdictionAuthoritySnapshot>()
            : officeQueries.GetJurisdictions().ToDictionary(static jurisdiction => jurisdiction.SettlementId, static jurisdiction => jurisdiction);
        Dictionary<SettlementId, TradeActivitySnapshot> tradeActivityBySettlement = BuildTradeActivityBySettlement(tradeQueries);

        foreach (SettlementSnapshot settlement in settlements.OrderBy(static settlement => settlement.Id.Value))
        {
            PopulationSettlementSnapshot population = populationQueries.GetRequiredSettlement(settlement.Id);
            SettlementForceState force = GetOrCreateSettlement(scope.State, settlement.Id);

            ClanSnapshot[] localClans = clans
                .Where(clan => clan.HomeSettlementId == settlement.Id)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();
            int averagePrestige = AverageClanValue(localClans, static clan => clan.Prestige);
            int averageSupport = AverageClanValue(localClans, static clan => clan.SupportReserve);
            int localGrudge = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.GrudgePressure);
            int localFear = AverageNarrative(localClans, narrativesByClan, static narrative => narrative.FearPressure);

            SettlementDisorderSnapshot disorder = disorderBySettlement.TryGetValue(settlement.Id, out SettlementDisorderSnapshot? disorderSnapshot)
                ? disorderSnapshot
                : EmptyDisorderSnapshot.For(settlement.Id);
            JurisdictionAuthoritySnapshot? jurisdiction = jurisdictionBySettlement.TryGetValue(settlement.Id, out JurisdictionAuthoritySnapshot? authority)
                ? authority
                : null;
            int administrativeSupport = jurisdiction is null ? 0 : ComputeAdministrativeSupport(jurisdiction);
            TradeActivitySnapshot tradeActivity = tradeActivityBySettlement.TryGetValue(settlement.Id, out TradeActivitySnapshot? tradeSnapshot)
                ? tradeSnapshot
                : TradeActivitySnapshot.Empty;

            int previousGuardCount = force.GuardCount;
            int previousRetainerCount = force.RetainerCount;
            int previousMilitiaCount = force.MilitiaCount;
            int previousEscortCount = force.EscortCount;
            int previousReadiness = force.Readiness;
            int previousCommandCapacity = force.CommandCapacity;
            int previousResponseActivationLevel = force.ResponseActivationLevel;
            int previousOrderSupportLevel = force.OrderSupportLevel;
            bool previousIsResponseActivated = force.IsResponseActivated;
            bool previousHasActiveConflict = force.HasActiveConflict;
            int previousCampaignFatigue = force.CampaignFatigue;
            int previousCampaignEscortStrain = force.CampaignEscortStrain;
            string previousCampaignFalloutTrace = force.LastCampaignFalloutTrace;

            RecoverCampaignFallout(force, settlement, disorder, administrativeSupport);

            force.GuardCount = Math.Clamp(
                4
                + (settlement.Security / 10)
                + (averageSupport / 15)
                + (disorder.SuppressionDemand / 20)
                - (disorder.BanditThreat / 25)
                + scope.Context.Random.NextInt(-1, 2),
                0,
                60);

            force.RetainerCount = Math.Clamp(
                2
                + (averagePrestige / 14)
                + (averageSupport / 22)
                + (localGrudge / 35)
                + scope.Context.Random.NextInt(-1, 2),
                0,
                40);

            force.MilitiaCount = Math.Clamp(
                (population.MilitiaPotential / 6)
                + (disorder.SuppressionDemand / 10)
                + (localFear / 18)
                - (population.MigrationPressure / 18)
                + scope.Context.Random.NextInt(-2, 3),
                0,
                90);

            force.EscortCount = tradeActivity.ActiveRouteCount == 0
                ? 0
                : Math.Clamp(
                    (tradeActivity.ActiveRouteCount * 2)
                    + (tradeActivity.TotalRouteCapacity / 14)
                    + (disorder.RoutePressure / 18)
                    + (disorder.BanditThreat / 30)
                    + scope.Context.Random.NextInt(-1, 2),
                    0,
                    50);

            ApplyCampaignStrengthPenalties(force);

            force.Readiness = Math.Clamp(
                12
                + force.GuardCount
                + force.RetainerCount
                + (force.MilitiaCount / 2)
                + force.EscortCount
                + (averageSupport / 4)
                + (settlement.Security / 3)
                + (administrativeSupport * 4)
                - (population.CommonerDistress / 3)
                - (disorder.DisorderPressure / 4)
                - (disorder.RoutePressure / 5),
                0,
                100);

            force.Readiness = Math.Clamp(
                force.Readiness
                - (force.CampaignFatigue / 4)
                - (force.CampaignEscortStrain / 8),
                0,
                100);

            force.CommandCapacity = Math.Clamp(
                6
                + (averagePrestige / 3)
                + (averageSupport / 4)
                + force.RetainerCount
                + (force.GuardCount / 2)
                + (administrativeSupport * 6)
                - (localGrudge / 6)
                + scope.Context.Random.NextInt(-2, 3),
                0,
                100);

            force.CommandCapacity = Math.Clamp(
                force.CommandCapacity
                - (force.CampaignFatigue / 5)
                - (force.CampaignEscortStrain / 10),
                0,
                100);

            int conflictPressure =
                disorder.BanditThreat
                + disorder.RoutePressure
                + disorder.DisorderPressure
                + localGrudge
                + (population.CommonerDistress / 2)
                + tradeActivity.AverageRouteRisk;
            int forcePosture = force.Readiness + force.CommandCapacity + force.GuardCount + force.EscortCount;
            bool conflictResolved = (conflictPressure - forcePosture) >= 60
                || (disorder.BanditThreat >= 65 && disorder.RoutePressure >= 60);
            bool commanderWounded = conflictResolved
                && (force.Readiness < 35 || localGrudge >= 60)
                && scope.Context.Random.NextInt(0, 100) < 35;
            int responseActivationLevel = ConflictAndForceResponseStateCalculator.ComputeResponseActivationLevel(force);
            force.HasActiveConflict = DetermineActiveConflict(
                previousHasActiveConflict,
                responseActivationLevel,
                disorder,
                localGrudge,
                localFear,
                conflictPressure,
                forcePosture,
                conflictResolved,
                commanderWounded);
            ConflictAndForceResponseStateCalculator.Refresh(force);

            force.LastConflictTrace = BuildConflictTrace(
                settlement,
                population,
                disorder,
                tradeActivity,
                force,
                localGrudge,
                localFear,
                conflictPressure,
                forcePosture,
                conflictResolved,
                commanderWounded,
                jurisdiction,
                administrativeSupport);

            if (previousGuardCount == force.GuardCount &&
                previousRetainerCount == force.RetainerCount &&
                previousMilitiaCount == force.MilitiaCount &&
                previousEscortCount == force.EscortCount &&
                previousReadiness == force.Readiness &&
                previousCommandCapacity == force.CommandCapacity &&
                previousResponseActivationLevel == force.ResponseActivationLevel &&
                previousOrderSupportLevel == force.OrderSupportLevel &&
                previousIsResponseActivated == force.IsResponseActivated &&
                previousHasActiveConflict == force.HasActiveConflict &&
                previousCampaignFatigue == force.CampaignFatigue &&
                previousCampaignEscortStrain == force.CampaignEscortStrain &&
                string.Equals(previousCampaignFalloutTrace, force.LastCampaignFalloutTrace, StringComparison.Ordinal) &&
                !conflictResolved &&
                !commanderWounded)
            {
                continue;
            }

            scope.RecordDiff(
                $"{settlement.Name}地面守备：守丁{force.GuardCount}，亲兵{force.RetainerCount}，乡勇{force.MilitiaCount}，护运{force.EscortCount}，整备{force.Readiness}，号令{force.CommandCapacity}，应势{force.ResponseActivationLevel}，援压{force.OrderSupportLevel}。{force.LastConflictTrace}",
                settlement.Id.Value.ToString());

            if (previousMilitiaCount < 20 && force.MilitiaCount >= 20)
            {
                scope.Emit("MilitiaMobilized", $"{settlement.Name}乡勇已集。");
            }

            if (Math.Abs(force.Readiness - previousReadiness) >= 8)
            {
                scope.Emit("ForceReadinessChanged", $"{settlement.Name}地面守备整备改为{force.Readiness}。");
            }

            if (conflictResolved)
            {
                scope.Emit("ConflictResolved", $"{settlement.Name}地面冲突暂被按住。");
            }

            if (commanderWounded)
            {
                scope.Emit("CommanderWounded", $"{settlement.Name}弹压之际，有领队负创。");
            }
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<ConflictAndForceState> scope)
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

            SettlementForceState force = GetOrCreateSettlement(scope.State, bundle.SettlementId);
            int previousReadiness = force.Readiness;
            int previousCommandCapacity = force.CommandCapacity;
            int previousMilitiaCount = force.MilitiaCount;
            int previousEscortCount = force.EscortCount;
            int previousResponseActivationLevel = force.ResponseActivationLevel;
            int previousOrderSupportLevel = force.OrderSupportLevel;
            int previousCampaignFatigue = force.CampaignFatigue;
            int previousCampaignEscortStrain = force.CampaignEscortStrain;
            string previousFalloutTrace = force.LastCampaignFalloutTrace;

            int fatigueDelta = ComputeCampaignFatigueDelta(bundle, campaign);
            int escortStrainDelta = ComputeCampaignEscortStrainDelta(bundle, campaign);
            int readinessDrop = ComputeImmediateReadinessDrop(bundle, campaign);
            int commandDrop = ComputeImmediateCommandDrop(bundle, campaign);
            int militiaLoss = Math.Min(force.MilitiaCount, ComputeImmediateMilitiaLoss(bundle, campaign));
            int escortLoss = Math.Min(force.EscortCount, ComputeImmediateEscortLoss(bundle, campaign));

            force.CampaignFatigue = Math.Clamp(force.CampaignFatigue + fatigueDelta, 0, 100);
            force.CampaignEscortStrain = Math.Clamp(force.CampaignEscortStrain + escortStrainDelta, 0, 100);
            force.MilitiaCount = Math.Max(0, force.MilitiaCount - militiaLoss);
            force.EscortCount = Math.Max(0, force.EscortCount - escortLoss);
            force.Readiness = Math.Clamp(force.Readiness - readinessDrop, 0, 100);
            force.CommandCapacity = Math.Clamp(force.CommandCapacity - commandDrop, 0, 100);
            force.LastCampaignFalloutTrace = BuildCampaignFalloutTrace(bundle, campaign, fatigueDelta, escortStrainDelta, readinessDrop, commandDrop);
            force.LastConflictTrace = MergeFalloutIntoConflictTrace(force.LastConflictTrace, force.LastCampaignFalloutTrace);
            ConflictAndForceResponseStateCalculator.Refresh(force);

            if (previousReadiness == force.Readiness
                && previousCommandCapacity == force.CommandCapacity
                && previousMilitiaCount == force.MilitiaCount
                && previousEscortCount == force.EscortCount
                && previousResponseActivationLevel == force.ResponseActivationLevel
                && previousOrderSupportLevel == force.OrderSupportLevel
                && previousCampaignFatigue == force.CampaignFatigue
                && previousCampaignEscortStrain == force.CampaignEscortStrain
                && string.Equals(previousFalloutTrace, force.LastCampaignFalloutTrace, StringComparison.Ordinal))
            {
                continue;
            }

            scope.RecordDiff(
                $"{campaign.AnchorSettlementName}战后余波所留：疲敝{force.CampaignFatigue}，护运困乏{force.CampaignEscortStrain}，整备{force.Readiness}，号令{force.CommandCapacity}。{force.LastCampaignFalloutTrace}",
                bundle.SettlementId.Value.ToString());

            if (previousReadiness - force.Readiness >= 4 || previousCampaignFatigue != force.CampaignFatigue)
            {
                scope.Emit(
                    "ForceReadinessChanged",
                    $"{campaign.AnchorSettlementName}战后余波拖得地面整备降至{force.Readiness}。",
                    bundle.SettlementId.Value.ToString());
            }
        }
    }

    private static Dictionary<SettlementId, TradeActivitySnapshot> BuildTradeActivityBySettlement(ITradeAndIndustryQueries? tradeQueries)
    {
        if (tradeQueries is null)
        {
            return new Dictionary<SettlementId, TradeActivitySnapshot>();
        }

        Dictionary<SettlementId, List<ClanTradeRouteSnapshot>> routesBySettlement = new();
        foreach (ClanTradeSnapshot clanTrade in tradeQueries.GetClanTrades().OrderBy(static trade => trade.ClanId.Value))
        {
            foreach (ClanTradeRouteSnapshot route in tradeQueries.GetRoutesForClan(clanTrade.ClanId)
                         .Where(static route => route.IsActive)
                         .OrderBy(static route => route.RouteId))
            {
                if (!routesBySettlement.TryGetValue(route.SettlementId, out List<ClanTradeRouteSnapshot>? routes))
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

    private static int AverageClanValue(IReadOnlyList<ClanSnapshot> clans, Func<ClanSnapshot, int> selector)
    {
        return clans.Count == 0 ? 0 : clans.Sum(selector) / clans.Count;
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

    private static SettlementForceState GetOrCreateSettlement(ConflictAndForceState state, SettlementId settlementId)
    {
        SettlementForceState? settlement = state.Settlements.SingleOrDefault(existing => existing.SettlementId == settlementId);
        if (settlement is not null)
        {
            return settlement;
        }

        settlement = new SettlementForceState
        {
            SettlementId = settlementId,
        };
        state.Settlements.Add(settlement);
        state.Settlements = state.Settlements.OrderBy(static entry => entry.SettlementId.Value).ToList();
        return settlement;
    }

    private static void ApplyXunOpeningPosture(
        SettlementForceState force,
        SettlementSnapshot settlement,
        PopulationSettlementSnapshot population,
        SettlementDisorderSnapshot disorder,
        TradeActivitySnapshot tradeActivity,
        int averageSupport,
        int administrativeSupport)
    {
        int escortDelta = 0;
        escortDelta += tradeActivity.ActiveRouteCount > 0 ? 1 : 0;
        escortDelta += disorder.RoutePressure >= 55 ? 1 : disorder.RoutePressure < 20 ? -1 : 0;
        escortDelta += tradeActivity.AverageRouteRisk >= 55 ? 1 : 0;
        escortDelta -= force.CampaignEscortStrain >= 40 ? 1 : 0;

        int readinessDelta = 0;
        readinessDelta += disorder.SuppressionDemand >= 55 ? 1 : 0;
        readinessDelta += disorder.RoutePressure >= 55 ? 1 : 0;
        readinessDelta += administrativeSupport;
        readinessDelta += averageSupport >= 60 ? 1 : 0;
        readinessDelta -= settlement.Security < 45 ? 1 : 0;
        readinessDelta -= population.CommonerDistress >= 60 ? 1 : 0;
        readinessDelta -= force.CampaignFatigue >= 35 ? 1 : 0;

        force.EscortCount = Math.Clamp(force.EscortCount + escortDelta, 0, 50);
        force.Readiness = Math.Clamp(force.Readiness + readinessDelta, 0, 100);
    }

    private static void ApplyXunHotspotPulse(
        SettlementForceState force,
        SettlementSnapshot settlement,
        SettlementDisorderSnapshot disorder,
        TradeActivitySnapshot tradeActivity,
        int localFear,
        int localGrudge,
        int averagePrestige,
        int administrativeSupport)
    {
        int commandDelta = 0;
        commandDelta += averagePrestige >= 55 ? 1 : 0;
        commandDelta += administrativeSupport;
        commandDelta += disorder.BanditThreat >= 60 ? 1 : 0;
        commandDelta -= localGrudge >= 60 ? 1 : 0;
        commandDelta -= force.CampaignFatigue >= 40 ? 1 : 0;

        int readinessDelta = 0;
        readinessDelta += disorder.DisorderPressure >= 60 ? 1 : 0;
        readinessDelta += disorder.RoutePressure >= 55 ? 1 : 0;
        readinessDelta -= localFear >= 60 ? 1 : 0;
        readinessDelta -= tradeActivity.AverageRouteRisk >= 70 ? 1 : 0;

        force.CommandCapacity = Math.Clamp(force.CommandCapacity + commandDelta, 0, 100);
        force.Readiness = Math.Clamp(force.Readiness + readinessDelta, 0, 100);

        if (settlement.Security < 45 && disorder.RoutePressure >= 60)
        {
            force.EscortCount = Math.Clamp(force.EscortCount + 1, 0, 50);
        }
    }

    private static void ApplyXunClosingPosture(
        SettlementForceState force,
        SettlementSnapshot settlement,
        PopulationSettlementSnapshot population,
        SettlementDisorderSnapshot disorder,
        TradeActivitySnapshot tradeActivity,
        int localFear,
        int localGrudge,
        int administrativeSupport)
    {
        int readinessDelta = 0;
        readinessDelta += administrativeSupport;
        readinessDelta += disorder.SuppressionDemand >= 50 ? 1 : 0;
        readinessDelta -= population.MigrationPressure >= 55 ? 1 : 0;
        readinessDelta -= localFear >= 55 ? 1 : 0;
        readinessDelta -= force.CampaignFatigue >= 45 ? 1 : 0;

        int escortDelta = 0;
        escortDelta += tradeActivity.ActiveRouteCount > 0 && disorder.RoutePressure >= 45 ? 1 : 0;
        escortDelta -= tradeActivity.ActiveRouteCount == 0 ? 1 : 0;
        escortDelta -= settlement.Security >= 65 && disorder.RoutePressure < 25 ? 1 : 0;

        int commandDelta = 0;
        commandDelta += localGrudge >= 55 ? -1 : 0;
        commandDelta += disorder.DisorderPressure >= 60 ? 1 : 0;
        commandDelta += force.CommandCapacity < 20 && administrativeSupport > 0 ? 1 : 0;

        force.Readiness = Math.Clamp(force.Readiness + readinessDelta, 0, 100);
        force.EscortCount = Math.Clamp(force.EscortCount + escortDelta, 0, 50);
        force.CommandCapacity = Math.Clamp(force.CommandCapacity + commandDelta, 0, 100);
    }

    private static void RecoverCampaignFallout(
        SettlementForceState force,
        SettlementSnapshot settlement,
        SettlementDisorderSnapshot disorder,
        int administrativeSupport)
    {
        int recovery = 2 + (settlement.Security / 30) + administrativeSupport;
        if (disorder.BanditThreat >= 55 || disorder.SuppressionDemand >= 55 || force.HasActiveConflict)
        {
            recovery = Math.Max(1, recovery - 2);
        }

        force.CampaignFatigue = Math.Max(0, force.CampaignFatigue - recovery);
        force.CampaignEscortStrain = Math.Max(0, force.CampaignEscortStrain - (recovery + (settlement.Prosperity >= 60 ? 1 : 0)));

        if (force.CampaignFatigue == 0 && force.CampaignEscortStrain == 0)
        {
            force.LastCampaignFalloutTrace = string.Empty;
        }
    }

    private static void ApplyCampaignStrengthPenalties(SettlementForceState force)
    {
        force.GuardCount = Math.Max(0, force.GuardCount - Math.Max(0, force.CampaignFatigue - 28) / 18);
        force.RetainerCount = Math.Max(0, force.RetainerCount - Math.Max(0, force.CampaignFatigue - 32) / 20);
        force.MilitiaCount = Math.Max(0, force.MilitiaCount - (force.CampaignFatigue / 16));
        force.EscortCount = Math.Max(0, force.EscortCount - Math.Max(force.CampaignFatigue / 24, force.CampaignEscortStrain / 14));
    }

    private static string BuildConflictTrace(
        SettlementSnapshot settlement,
        PopulationSettlementSnapshot population,
        SettlementDisorderSnapshot disorder,
        TradeActivitySnapshot tradeActivity,
        SettlementForceState force,
        int localGrudge,
        int localFear,
        int conflictPressure,
        int forcePosture,
        bool conflictResolved,
        bool commanderWounded,
        JurisdictionAuthoritySnapshot? jurisdiction,
        int administrativeSupport)
    {
        List<string> reasons = [];

        if (disorder.BanditThreat >= 45 || disorder.RoutePressure >= 45)
        {
            reasons.Add($"盗压{disorder.BanditThreat}、路压{disorder.RoutePressure}，逼得地面不得不加紧巡守。");
        }

        if (population.CommonerDistress >= 55 || population.MigrationPressure >= 45)
        {
            reasons.Add($"民困{population.CommonerDistress}、流徙之压{population.MigrationPressure}，都在推高地面斗殴与械争。");
        }

        if (tradeActivity.ActiveRouteCount > 0)
        {
            reasons.Add($"尚有{tradeActivity.ActiveRouteCount}条活路待护，故而护运{force.EscortCount}不能轻撤。");
        }

        if (jurisdiction is not null && administrativeSupport > 0)
        {
            reasons.Add($"{jurisdiction.LeadOfficeTitle}乡面杠力{jurisdiction.JurisdictionLeverage}，替地面守备添了{administrativeSupport}分文移支应。");
        }

        if (force.CampaignFatigue > 0 || force.CampaignEscortStrain > 0)
        {
            string falloutTrace = string.IsNullOrWhiteSpace(force.LastCampaignFalloutTrace)
                ? "前番兵事留下的困乏，仍在耗着守夜与护运。"
                : force.LastCampaignFalloutTrace;
            reasons.Add($"前番兵事仍留疲敝{force.CampaignFatigue}、护运困乏{force.CampaignEscortStrain}。{falloutTrace}");
        }

        if (conflictResolved)
        {
            reasons.Add("这一场地面冲突已先被按住，未曾外漫。");
        }

        reasons.Add($"如今守备之势{forcePosture}，正以整备{force.Readiness}、号令{force.CommandCapacity}去压冲突之压{conflictPressure}。");

        if (localFear >= 50 || localGrudge >= 50)
        {
            reasons.Add($"{settlement.Name}乡里惧意{localFear}、旧怨{localGrudge}，人心仍贴着械斗边缘。");
        }

        if (commanderWounded)
        {
            reasons.Add("虽把局面按住，却也折了一名领队，平添新怨。");
        }

        return string.Join(" ", reasons.Take(5));
    }

    private static int ComputeAdministrativeSupport(JurisdictionAuthoritySnapshot jurisdiction)
    {
        int support = 0;
        if (jurisdiction.AuthorityTier >= 2)
        {
            support += 1;
        }

        if (jurisdiction.JurisdictionLeverage >= 55)
        {
            support += 2;
        }
        else if (jurisdiction.JurisdictionLeverage >= 28)
        {
            support += 1;
        }

        if (jurisdiction.PetitionPressure >= 65)
        {
            support -= 1;
        }

        return Math.Max(0, support);
    }

    private static int ComputeCampaignFatigueDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 2 : 0;
        delta += bundle.CampaignPressureRaised ? 3 : 0;
        delta += bundle.CampaignSupplyStrained ? 4 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;
        delta += Math.Max(0, 55 - campaign.MoraleState) / 20;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignEscortStrainDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignSupplyStrained ? 4 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, 55 - campaign.SupplyState) / 12;
        return Math.Max(1, delta);
    }

    private static int ComputeImmediateReadinessDrop(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int drop = bundle.CampaignPressureRaised ? 2 : 0;
        drop += bundle.CampaignSupplyStrained ? 3 : 0;
        drop += bundle.CampaignAftermathRegistered ? 2 : 0;
        drop += Math.Max(0, campaign.FrontPressure - 60) / 20;
        return Math.Max(1, drop);
    }

    private static int ComputeImmediateCommandDrop(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int drop = bundle.CampaignSupplyStrained ? 2 : 0;
        drop += bundle.CampaignAftermathRegistered ? 1 : 0;
        drop += Math.Max(0, 55 - campaign.MoraleState) / 18;
        return Math.Max(1, drop);
    }

    private static int ComputeImmediateMilitiaLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int loss = bundle.CampaignAftermathRegistered ? 1 : 0;
        loss += campaign.FrontPressure >= 72 ? 1 : 0;
        return loss;
    }

    private static int ComputeImmediateEscortLoss(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int loss = bundle.CampaignSupplyStrained ? 1 : 0;
        loss += campaign.SupplyState <= 35 ? 1 : 0;
        return loss;
    }

    private static string BuildCampaignFalloutTrace(
        WarfareCampaignEventBundle bundle,
        CampaignFrontSnapshot campaign,
        int fatigueDelta,
        int escortStrainDelta,
        int readinessDrop,
        int commandDrop)
    {
        string strainText = bundle.CampaignSupplyStrained
            ? "护粮与驿传这一线回来时已显困敝。"
            : "守夜轮值仍背着前番兵事留下的担子。";

        return $"{campaign.AnchorSettlementName}战后余波留下疲敝+{fatigueDelta}、护运困乏+{escortStrainDelta}、整备-{readinessDrop}、号令-{commandDrop}。{campaign.FrontLabel}、{campaign.SupplyStateLabel}与{campaign.LastAftermathSummary}都还压在地面守备身上。{strainText}";
    }

    private static string MergeFalloutIntoConflictTrace(string currentTrace, string falloutTrace)
    {
        if (string.IsNullOrWhiteSpace(falloutTrace)
            || currentTrace.Contains(falloutTrace, StringComparison.Ordinal))
        {
            return currentTrace;
        }

        if (string.IsNullOrWhiteSpace(currentTrace))
        {
            return falloutTrace;
        }

        return $"{currentTrace} {falloutTrace}";
    }

    private static string BuildXunConflictTrace(
        SettlementSnapshot settlement,
        SettlementDisorderSnapshot disorder,
        TradeActivitySnapshot tradeActivity,
        SettlementForceState force,
        int localFear,
        int localGrudge,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        List<string> reasons = [];

        if (disorder.RoutePressure >= 45 || disorder.BanditThreat >= 45)
        {
            reasons.Add($"{settlement.Name}路面盗压{disorder.BanditThreat}、路压{disorder.RoutePressure}，守夜与护运都被往前推。");
        }

        if (tradeActivity.ActiveRouteCount > 0)
        {
            reasons.Add($"现有{tradeActivity.ActiveRouteCount}条活路，护运{force.EscortCount}正贴着行脚与货脚走。");
        }

        if (jurisdiction is not null && jurisdiction.JurisdictionLeverage >= 28)
        {
            reasons.Add($"{jurisdiction.LeadOfficeTitle}乡面杖力{jurisdiction.JurisdictionLeverage}，还能替地面守备撑一把。");
        }

        reasons.Add($"眼下整备{force.Readiness}、号令{force.CommandCapacity}、应势{force.ResponseActivationLevel}，惧意{localFear}、旧怨{localGrudge}都还在场。");
        return string.Join(" ", reasons.Take(4));
    }

    private static bool DetermineActiveConflict(
        bool previousHasActiveConflict,
        int responseActivationLevel,
        SettlementDisorderSnapshot disorder,
        int localGrudge,
        int localFear,
        int conflictPressure,
        int forcePosture,
        bool conflictResolved,
        bool commanderWounded)
    {
        return conflictResolved
            || commanderWounded
            || disorder.BanditThreat >= 60
            || disorder.RoutePressure >= 55
            || disorder.DisorderPressure >= 60
            || disorder.SuppressionDemand >= 55
            || localGrudge >= 60
            || localFear >= 55
            || (previousHasActiveConflict
                && responseActivationLevel >= ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel
                && (disorder.BanditThreat >= 20
                    || disorder.RoutePressure >= 20
                    || disorder.SuppressionDemand >= 15
                    || localGrudge >= 40
                    || localFear >= 40))
            || (conflictPressure - forcePosture) >= 10;
    }

    private static bool DetermineXunCarryoverConflict(
        bool previousHasActiveConflict,
        SettlementDisorderSnapshot disorder,
        int localGrudge,
        int localFear)
    {
        return previousHasActiveConflict
            && (disorder.BanditThreat >= 20
                || disorder.RoutePressure >= 20
                || disorder.SuppressionDemand >= 15
                || localGrudge >= 35
                || localFear >= 35);
    }

    private static bool DetermineXunEscalatingConflict(
        bool previousHasActiveConflict,
        int responseActivationLevel,
        SettlementDisorderSnapshot disorder,
        int localGrudge,
        int localFear,
        int conflictPressure,
        int forcePosture)
    {
        return disorder.BanditThreat >= 60
            || disorder.RoutePressure >= 55
            || disorder.DisorderPressure >= 60
            || disorder.SuppressionDemand >= 55
            || localGrudge >= 60
            || localFear >= 55
            || DetermineXunCarryoverConflict(
                previousHasActiveConflict,
                disorder,
                localGrudge,
                localFear)
            || (responseActivationLevel >= ConflictAndForceResponseStateCalculator.MinimumResponseActivationLevel
                && (conflictPressure - forcePosture) >= 80);
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
                ResponseActivationLevel = settlement.ResponseActivationLevel,
                OrderSupportLevel = settlement.OrderSupportLevel,
                IsResponseActivated = settlement.IsResponseActivated,
                HasActiveConflict = settlement.HasActiveConflict,
                CampaignFatigue = settlement.CampaignFatigue,
                CampaignEscortStrain = settlement.CampaignEscortStrain,
                LastCampaignFalloutTrace = settlement.LastCampaignFalloutTrace,
                LastConflictTrace = settlement.LastConflictTrace,
            };
        }
    }

    private sealed record TradeActivitySnapshot(int ActiveRouteCount, int AverageRouteRisk, int TotalRouteCapacity)
    {
        public static readonly TradeActivitySnapshot Empty = new(0, 0, 0);
    }

    private static class EmptyDisorderSnapshot
    {
        public static SettlementDisorderSnapshot For(SettlementId settlementId)
        {
            return new SettlementDisorderSnapshot
            {
                SettlementId = settlementId,
            };
        }
    }
}
