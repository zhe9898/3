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
                    ApplyYamenOverloadHeat(scope);
                    break;

                case OrderAndBanditryEventNames.DisorderSpike:
                    ApplyDisorderSpikeHeat(scope, domainEvent);
                    break;

                case OfficeAndCareerEventNames.ClerkCaptureDeepened:
                    ApplyClerkCaptureHeat(scope, domainEvent);
                    break;
            }
        }
    }

    private static void ApplyYamenOverloadHeat(ModuleEventHandlingScope<PublicLifeAndRumorState> scope)
    {
        // Renzong thin chain: yamen overload → public life heat.
        // For thin-chain we attach heat to all settlements (no settlement id in payload yet).
        foreach (SettlementPublicLifeState publicLife in scope.State.Settlements)
        {
            publicLife.StreetTalkHeat = Math.Clamp(publicLife.StreetTalkHeat + 15, 0, 100);
            publicLife.LastPublicTrace = $"衙门口因税役挤满请减之人，街谈热度升至{publicLife.StreetTalkHeat}。";
        }
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
