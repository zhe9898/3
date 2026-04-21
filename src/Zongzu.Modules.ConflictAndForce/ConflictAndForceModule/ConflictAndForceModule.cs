using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

public sealed partial class ConflictAndForceModule : ModuleRunner<ConflictAndForceState>
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

        // Step 1b gap 2: declared only; source trigger 留给 Step 2。
        DeathCauseEventNames.DeathByViolence,

    ];


    private static readonly string[] ConsumedEventNames =

    [

        WarfareCampaignEventNames.CampaignMobilized,

        WarfareCampaignEventNames.CampaignPressureRaised,

        WarfareCampaignEventNames.CampaignSupplyStrained,

        WarfareCampaignEventNames.CampaignAftermathRegistered,

    ];


    public override string ModuleKey => KnownModuleKeys.ConflictAndForce;


    public override int ModuleSchemaVersion => 4;


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

        ConflictAndForceStateProjection.BuildForceGroupsAndIncidents(scope.State);

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


}
