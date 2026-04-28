using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PublicLifeAndRumor;

public sealed partial class PublicLifeAndRumorModule : ModuleRunner<PublicLifeAndRumorState>
{
    private readonly record struct VenueDescriptor(string Code, string Label);


    private static readonly string[] EventNames =

    [

        PublicLifeAndRumorEventNames.StreetTalkSurged,

        PublicLifeAndRumorEventNames.CountyGateCrowded,

        PublicLifeAndRumorEventNames.MarketBuzzRaised,

        PublicLifeAndRumorEventNames.RoadReportDelayed,

        PublicLifeAndRumorEventNames.PrefectureDispatchPressed,

    ];


    public override string ModuleKey => KnownModuleKeys.PublicLifeAndRumor;


    public override int ModuleSchemaVersion => 4;


    public override SimulationPhase Phase => SimulationPhase.UpwardMobilityAndEconomy;


    public override int ExecutionOrder => 760;


    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;


    public override FeatureMode DefaultMode => FeatureMode.Lite;


    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    private static readonly string[] ConsumedEventNames =
    [
        OfficeAndCareerEventNames.YamenOverloaded,
        OrderAndBanditryEventNames.DisorderSpike,
        OfficeAndCareerEventNames.ClerkCaptureDeepened,
        OfficeAndCareerEventNames.PolicyImplemented,
        OfficeAndCareerEventNames.OfficeDefected,
    ];

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override PublicLifeAndRumorState CreateInitialState()

    {

        return new PublicLifeAndRumorState();

    }


    public override void RegisterQueries(PublicLifeAndRumorState state, QueryRegistry queries)

    {

        queries.Register<IPublicLifeAndRumorQueries>(new PublicLifeAndRumorQueries(state));

    }


    public override void RunXun(ModuleExecutionScope<PublicLifeAndRumorState> scope)

    {

        RunSettlementPulse(scope, emitReadableOutput: false);

    }


    public override void RunMonth(ModuleExecutionScope<PublicLifeAndRumorState> scope)

    {

        RunSettlementPulse(scope, emitReadableOutput: true);

    }

    public override void HandleEvents(ModuleEventHandlingScope<PublicLifeAndRumorState> scope)
    {
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            switch (domainEvent.EventType)
            {
                case OfficeAndCareerEventNames.YamenOverloaded:
                    ApplyYamenOverloadHeat(scope, domainEvent);
                    break;

                case OrderAndBanditryEventNames.DisorderSpike:
                    ApplyDisorderSpikeHeat(scope, domainEvent);
                    break;

                case OfficeAndCareerEventNames.ClerkCaptureDeepened:
                    ApplyClerkCaptureHeat(scope, domainEvent);
                    break;

                case OfficeAndCareerEventNames.PolicyImplemented:
                    ApplyPolicyImplementationHeat(scope, domainEvent);
                    break;

                case OfficeAndCareerEventNames.OfficeDefected:
                    ApplyOfficeDefectionHeat(scope, domainEvent);
                    break;
            }
        }
    }

    private static void ApplyYamenOverloadHeat(
        ModuleEventHandlingScope<PublicLifeAndRumorState> scope,
        IDomainEvent domainEvent)
    {
        // Renzong thin chain: yamen overload → public life heat.
        if (!int.TryParse(domainEvent.EntityKey, out int settlementIdValue))
        {
            return;
        }

        SettlementId settlementId = new(settlementIdValue);
        SettlementPublicLifeState? publicLife = scope.State.Settlements
            .FirstOrDefault(s => s.SettlementId == settlementId);

        if (publicLife is null)
        {
            return;
        }

        publicLife.StreetTalkHeat = Math.Clamp(publicLife.StreetTalkHeat + 15, 0, 100);
        publicLife.LastPublicTrace = $"衙门口因税役挤满请减之人，街谈热度升至{publicLife.StreetTalkHeat}。";
    }

    private static void ApplyDisorderSpikeHeat(
        ModuleEventHandlingScope<PublicLifeAndRumorState> scope,
        IDomainEvent domainEvent)
    {
        if (!int.TryParse(domainEvent.EntityKey, out int settlementIdValue))
        {
            return;
        }

        SettlementId settlementId = new(settlementIdValue);
        SettlementPublicLifeState? publicLife = scope.State.Settlements
            .FirstOrDefault(s => s.SettlementId == settlementId);

        if (publicLife is null)
        {
            return;
        }

        publicLife.StreetTalkHeat = Math.Clamp(publicLife.StreetTalkHeat + 12, 0, 100);

        string causeHint = ResolveDisorderCauseHint(domainEvent);

        publicLife.LastPublicTrace = $"{causeHint}，街面不安，街谈热度升至{publicLife.StreetTalkHeat}。";
    }

    private static void ApplyClerkCaptureHeat(
        ModuleEventHandlingScope<PublicLifeAndRumorState> scope,
        IDomainEvent domainEvent)
    {
        if (!int.TryParse(domainEvent.EntityKey, out int settlementIdValue))
        {
            return;
        }

        SettlementId settlementId = new(settlementIdValue);
        SettlementPublicLifeState? publicLife = scope.State.Settlements
            .FirstOrDefault(s => s.SettlementId == settlementId);

        if (publicLife is null)
        {
            return;
        }

        int heatDelta = ResolveClerkCaptureHeatDelta(domainEvent);
        publicLife.StreetTalkHeat = Math.Clamp(publicLife.StreetTalkHeat + heatDelta, 0, 100);
        string pressureTrace = ResolveClerkCaptureTrace(domainEvent, heatDelta);
        publicLife.LastPublicTrace = $"{pressureTrace}，街谈热度升至{publicLife.StreetTalkHeat}。";
    }

    private static void ApplyPolicyImplementationHeat(
        ModuleEventHandlingScope<PublicLifeAndRumorState> scope,
        IDomainEvent domainEvent)
    {
        if (!TryResolveEventSettlementId(domainEvent, out SettlementId settlementId))
        {
            return;
        }

        SettlementPublicLifeState? publicLife = scope.State.Settlements
            .FirstOrDefault(s => s.SettlementId == settlementId);
        if (publicLife is null)
        {
            return;
        }

        string outcome = domainEvent.Metadata.TryGetValue(
            DomainEventMetadataKeys.PolicyImplementationOutcome,
            out string? rawOutcome)
            ? rawOutcome
            : DomainEventMetadataValues.PolicyImplementationDragged;
        int score = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.PolicyImplementationScore);
        int docketDrag = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.PolicyImplementationDocketDrag);
        int clerkCapture = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.PolicyImplementationClerkCapture);
        int paperCompliance = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.PolicyImplementationPaperCompliance);
        int windowPressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.PolicyImplementationWindowPressure);

        PolicyImplementationPublicReadback readback = ResolvePolicyImplementationPublicReadback(
            outcome,
            score,
            docketDrag,
            clerkCapture,
            paperCompliance,
            windowPressure);

        publicLife.StreetTalkHeat = Math.Clamp(publicLife.StreetTalkHeat + readback.HeatDelta, 0, 100);
        publicLife.NoticeVisibility = Math.Clamp(publicLife.NoticeVisibility + readback.NoticeDelta, 0, 100);
        publicLife.PrefectureDispatchPressure = Math.Clamp(publicLife.PrefectureDispatchPressure + readback.DispatchDelta, 0, 100);
        publicLife.RoadReportLag = Math.Clamp(publicLife.RoadReportLag + readback.RoadLagDelta, 0, 100);
        publicLife.PublicLegitimacy = Math.Clamp(publicLife.PublicLegitimacy + readback.LegitimacyDelta, 0, 100);

        publicLife.OfficialNoticeLine = readback.NoticeLine;
        publicLife.PrefectureDispatchLine = readback.DispatchLine;
        publicLife.ContentionSummary = readback.ContentionSummary;
        publicLife.ChannelSummary = readback.ChannelSummary;
        publicLife.LastPublicTrace = readback.Trace;
    }

    private static void ApplyOfficeDefectionHeat(
        ModuleEventHandlingScope<PublicLifeAndRumorState> scope,
        IDomainEvent domainEvent)
    {
        if (!TryResolveEventSettlementId(domainEvent, out SettlementId settlementId))
        {
            return;
        }

        SettlementPublicLifeState? publicLife = scope.State.Settlements
            .FirstOrDefault(s => s.SettlementId == settlementId);
        if (publicLife is null)
        {
            return;
        }

        int risk = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.DefectionRisk);
        int mandateDeficit = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.DefectionMandateDeficit);
        int clerkPressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.DefectionClerkPressure);
        int petitionPressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.DefectionPetitionPressure);
        int heatDelta = Math.Clamp(8 + (risk / 12) + (mandateDeficit / 8), 8, 22);
        int dispatchDelta = Math.Clamp(4 + (petitionPressure / 8), 4, 14);
        int legitimacyDelta = -Math.Clamp(2 + (risk / 20), 2, 8);

        publicLife.StreetTalkHeat = Math.Clamp(publicLife.StreetTalkHeat + heatDelta, 0, 100);
        publicLife.PrefectureDispatchPressure = Math.Clamp(publicLife.PrefectureDispatchPressure + dispatchDelta, 0, 100);
        publicLife.RoadReportLag = Math.Clamp(publicLife.RoadReportLag + (clerkPressure / 4), 0, 100);
        publicLife.PublicLegitimacy = Math.Clamp(publicLife.PublicLegitimacy + legitimacyDelta, 0, 100);
        publicLife.ContentionSummary =
            $"天命摇动读回：去就风险读回{risk}，天命缺口{mandateDeficit}；公议向背读法随街谈与榜示承压，仍由Office/PublicLife分读，不是本户替朝廷修合法性。";
        publicLife.PrefectureDispatchLine =
            $"官身承压姿态：州县之间已见去就摇摆，递报压力{publicLife.PrefectureDispatchPressure}；仍由Office/PublicLife分读，不是UI判定归附成败。";
        publicLife.ChannelSummary =
            $"公议向背读法：胥吏压{clerkPressure}、词牍压{petitionPressure}推高街面读法；不是本户替朝廷修合法性。";
        publicLife.LastPublicTrace =
            $"去就风险读回：风险{risk}，胥吏压{clerkPressure}，词牍压{petitionPressure}，街谈升温{heatDelta}。";
    }

    private static bool TryResolveEventSettlementId(IDomainEvent domainEvent, out SettlementId settlementId)
    {
        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.SettlementId, out string? rawSettlementId)
            && int.TryParse(rawSettlementId, out int metadataSettlementId))
        {
            settlementId = new SettlementId(metadataSettlementId);
            return true;
        }

        if (int.TryParse(domainEvent.EntityKey, out int entitySettlementId))
        {
            settlementId = new SettlementId(entitySettlementId);
            return true;
        }

        settlementId = default;
        return false;
    }

    private static PolicyImplementationPublicReadback ResolvePolicyImplementationPublicReadback(
        string outcome,
        int score,
        int docketDrag,
        int clerkCapture,
        int paperCompliance,
        int windowPressure)
    {
        return outcome switch
        {
            DomainEventMetadataValues.PolicyImplementationCaptured => new PolicyImplementationPublicReadback(
                18,
                6,
                8,
                5,
                -8,
                $"政策语气读回：催牒偏硬；县门执行读回：胥吏把持；县门承接姿态：榜示可见但案牍未真落地；分数{score}，胥吏捕获{clerkCapture}，仍回OfficeAndCareer lane，本户不能代修。",
                $"文移指向读回：州牒已催，县门仍被胥吏与积案拖住；朝廷后手仍不直写地方，外部后账该回OfficeAndCareer lane。",
                $"公议承压读法：街面把它读成县门被胥吏截留，押文催县门或改走递报仍需OfficeAndCareer。",
                $"县门承接姿态：胥吏把持，纸面{paperCompliance}，案牍拖{docketDrag}；本户不能代修，不是本户硬扛朝廷后账。",
                $"公议承压读法：captured，窗口压{windowPressure}，案牍拖{docketDrag}，胥吏捕获{clerkCapture}。"),

            DomainEventMetadataValues.PolicyImplementationPaperCompliance => new PolicyImplementationPublicReadback(
                8,
                10,
                3,
                2,
                -2,
                $"政策语气读回：榜文先行；县门执行读回：纸面落地；县门承接姿态：榜示先过，实办仍薄；分数{score}，纸面{paperCompliance}，仍看OfficeAndCareer lane，本户不能代修。",
                $"文移指向读回：州牒暂见回声，但文移实办仍需OfficeAndCareer lane读回；朝廷后手仍不直写地方。",
                $"公议承压读法：榜示先被看见，纸面落地仍欠实办后账，可先冷却观察或轻催县门。",
                $"县门承接姿态：纸面落地不等于修好，案牍拖{docketDrag}；本户不能代修，不是本户硬扛朝廷后账。",
                $"公议承压读法：paper-compliance，窗口压{windowPressure}，纸面{paperCompliance}。"),

            DomainEventMetadataValues.PolicyImplementationRapid => new PolicyImplementationPublicReadback(
                3,
                8,
                -2,
                -2,
                4,
                $"政策语气读回：急牍先过；县门执行读回：急牍先过；县门承接姿态：榜示与案牍暂能接住；分数{score}，仍看OfficeAndCareer lane，本户不能代修其他 lane。",
                $"文移指向读回：州县文移已有回声，眼下宜冷却观察；朝廷后手仍不直写地方，后续仍看OfficeAndCareer lane。",
                $"公议承压读法：急牍先过，街面暂缓；若路面后账未消，仍回各 owner lane。",
                $"县门承接姿态：急牍先过，仍只说明官署这头暂缓；本户不能代修，不是本户硬扛朝廷后账。",
                $"公议承压读法：rapid，窗口压{windowPressure}，分数{score}。"),

            _ => new PolicyImplementationPublicReadback(
                14,
                5,
                6,
                4,
                -5,
                $"政策语气读回：催意已到县门；县门执行读回：文移拖在案牍；县门承接姿态：榜示虽动，实办仍慢；分数{score}，案牍拖{docketDrag}，仍回OfficeAndCareer lane，本户不能代修。",
                $"文移指向读回：州牒催意已到，县门仍拖；朝廷后手仍不直写地方，外部后账该回OfficeAndCareer lane。",
                $"公议承压读法：街面看见文移拖在案牍，押文催县门或改走递报仍需OfficeAndCareer。",
                $"县门承接姿态：文移拖延，胥吏捕获{clerkCapture}；本户不能代修，不是本户硬扛朝廷后账。",
                $"公议承压读法：dragged，窗口压{windowPressure}，案牍拖{docketDrag}。"),
        };
    }

    private static int ResolveClerkCaptureHeatDelta(IDomainEvent domainEvent)
    {
        if (!domainEvent.Metadata.ContainsKey(DomainEventMetadataKeys.ClerkCapturePressure))
        {
            return 12;
        }

        int capturePressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCapturePressure);
        int backlogPressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCaptureBacklogPressure);
        int taskPressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCaptureTaskPressure);
        int authorityBuffer = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCaptureAuthorityBuffer);

        return Math.Clamp(
            8 + (capturePressure / 10) + (backlogPressure / 8) + (taskPressure / 8) - (authorityBuffer / 12),
            6,
            20);
    }

    private static string ResolveClerkCaptureTrace(IDomainEvent domainEvent, int heatDelta)
    {
        if (!domainEvent.Metadata.ContainsKey(DomainEventMetadataKeys.ClerkCapturePressure))
        {
            return "书吏坐大，街谈渐热";
        }

        int capturePressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCapturePressure);
        int backlogPressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCaptureBacklogPressure);
        int taskPressure = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCaptureTaskPressure);
        int authorityBuffer = ReadMetadataInt(domainEvent, DomainEventMetadataKeys.ClerkCaptureAuthorityBuffer);
        return $"书吏坐大，案牍压{backlogPressure}，差遣压{taskPressure}，捕获势{capturePressure}，官威缓冲{authorityBuffer}，街谈升温{heatDelta}";
    }

    private static string ResolveDisorderCauseHint(IDomainEvent domainEvent)
    {
        if (!domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.Cause, out string? cause))
        {
            return "街面失序";
        }

        return cause switch
        {
            DomainEventMetadataValues.CauseCorvee => "徭役加急",
            DomainEventMetadataValues.CauseAmnesty => "大赦释囚",
            DomainEventMetadataValues.CauseDisaster => ResolveDisasterCauseHint(domainEvent),
            _ => "街面失序",
        };
    }

    private static string ResolveDisasterCauseHint(IDomainEvent domainEvent)
    {
        if (!domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.DisasterKind, out string? disasterKind))
        {
            return "灾荒告急";
        }

        return disasterKind switch
        {
            DomainEventMetadataValues.DisasterFlood => "水患告急",
            _ => "灾荒告急",
        };
    }

    private static int ReadMetadataInt(IDomainEvent domainEvent, string key)
    {
        return domainEvent.Metadata.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed)
            ? parsed
            : 0;
    }

    private readonly record struct PolicyImplementationPublicReadback(
        int HeatDelta,
        int NoticeDelta,
        int DispatchDelta,
        int RoadLagDelta,
        int LegitimacyDelta,
        string NoticeLine,
        string DispatchLine,
        string ContentionSummary,
        string ChannelSummary,
        string Trace);

    private static void RunSettlementPulse(ModuleExecutionScope<PublicLifeAndRumorState> scope, bool emitReadableOutput)

    {

        IWorldSettlementsQueries worldQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();

        IReadOnlyList<SettlementSnapshot> settlements = worldQueries.GetSettlements();


        Dictionary<SettlementId, PopulationSettlementSnapshot> populationBySettlement = BuildPopulationBySettlement(scope);

        Dictionary<SettlementId, MarketSnapshot> marketsBySettlement = BuildMarketsBySettlement(scope);

        Dictionary<SettlementId, List<ClanTradeRouteSnapshot>> routesBySettlement = BuildRoutesBySettlement(scope);

        Dictionary<SettlementId, SettlementDisorderSnapshot> disorderBySettlement = BuildDisorderBySettlement(scope);

        Dictionary<SettlementId, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = BuildJurisdictionsBySettlement(scope);

        Dictionary<SettlementId, List<ClanSnapshot>> clansBySettlement = BuildClansBySettlement(scope);

        Dictionary<ClanId, ClanNarrativeSnapshot> narrativesByClan = BuildNarrativesByClan(scope);


        Dictionary<SettlementId, SettlementPublicLifeState> stateBySettlement = scope.State.Settlements

            .ToDictionary(static entry => entry.SettlementId, static entry => entry);


        foreach (SettlementSnapshot settlement in settlements.OrderBy(static entry => entry.Id.Value))

        {

            bool isNew = !stateBySettlement.TryGetValue(settlement.Id, out SettlementPublicLifeState? publicLife);

            publicLife ??= new SettlementPublicLifeState

            {

                SettlementId = settlement.Id,

            };


            SettlementPublicLifeState previous = Clone(publicLife);

            RefreshSettlementPulse(

                publicLife,

                scope.Context.CurrentDate,

                scope.Context.CurrentXun,

                settlement,

                populationBySettlement,

                marketsBySettlement,

                routesBySettlement,

                disorderBySettlement,

                jurisdictionsBySettlement,

                clansBySettlement,

                narrativesByClan);


            if (isNew)

            {

                scope.State.Settlements.Add(publicLife);

                stateBySettlement.Add(publicLife.SettlementId, publicLife);

            }


            if (!emitReadableOutput || !ShouldReport(previous, publicLife, isNew))

            {

                continue;

            }


            scope.RecordDiff(publicLife.LastPublicTrace, publicLife.SettlementId.Value.ToString());


            string? eventType = DeterminePrimaryEvent(publicLife);

            if (eventType is null)

            {

                continue;

            }


            scope.Emit(

                eventType,

                BuildEventSummary(publicLife, eventType),

                publicLife.SettlementId.Value.ToString());

        }

    }


}
