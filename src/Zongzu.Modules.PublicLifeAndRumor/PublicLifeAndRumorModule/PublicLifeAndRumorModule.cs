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
        // Renzong thin chain: yamen overload → public life heat.
        foreach (IDomainEvent domainEvent in scope.Events)
        {
            if (domainEvent.EventType != OfficeAndCareerEventNames.YamenOverloaded)
            {
                continue;
            }

            // Find or create settlement public-life state for the affected settlement.
            // For thin-chain we attach heat to all settlements (no settlement id in payload yet).
            foreach (SettlementPublicLifeState publicLife in scope.State.Settlements)
            {
                publicLife.StreetTalkHeat = Math.Clamp(publicLife.StreetTalkHeat + 15, 0, 100);
                publicLife.LastPublicTrace = $"衙门口因税役挤满请减之人，街谈热度升至{publicLife.StreetTalkHeat}。";
            }
        }
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
