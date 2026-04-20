using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Persistence;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private readonly SaveCodec _saveCodec = new();

    public PresentationReadModelBundle BuildForM2(GameSimulation simulation)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        QueryRegistry queries = BuildQueries(simulation);

        PresentationReadModelBundle bundle = new()
        {
            CurrentDate = simulation.CurrentDate,
            ReplayHash = simulation.ReplayHash,
        };

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.FamilyCore))
        {
            bundle.Clans = queries.GetRequired<IFamilyCoreQueries>().GetClans();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.SocialMemoryAndRelations))
        {
            bundle.ClanNarratives = queries.GetRequired<ISocialMemoryAndRelationsQueries>().GetClanNarratives();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WorldSettlements))
        {
            bundle.Settlements = queries.GetRequired<IWorldSettlementsQueries>().GetSettlements();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PopulationAndHouseholds))
        {
            bundle.PopulationSettlements = queries.GetRequired<IPopulationAndHouseholdsQueries>().GetSettlements();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.EducationAndExams))
        {
            IEducationAndExamsQueries educationQueries = queries.GetRequired<IEducationAndExamsQueries>();
            bundle.EducationCandidates = educationQueries.GetCandidates();
            bundle.Academies = educationQueries.GetAcademies();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.TradeAndIndustry))
        {
            ITradeAndIndustryQueries tradeQueries = queries.GetRequired<ITradeAndIndustryQueries>();
            ClanTradeSnapshot[] clanTrades = tradeQueries.GetClanTrades().ToArray();
            bundle.ClanTrades = clanTrades;
            bundle.Markets = tradeQueries.GetMarkets();
            bundle.ClanTradeRoutes = clanTrades
                .SelectMany(trade => tradeQueries.GetRoutesForClan(trade.ClanId))
                .OrderBy(static route => route.RouteId)
                .ToArray();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PublicLifeAndRumor))
        {
            bundle.PublicLifeSettlements = queries.GetRequired<IPublicLifeAndRumorQueries>().GetSettlementPublicLife();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry))
        {
            bundle.SettlementDisorder = queries.GetRequired<IOrderAndBanditryQueries>().GetSettlementDisorder();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            IOfficeAndCareerQueries officeQueries = queries.GetRequired<IOfficeAndCareerQueries>();
            bundle.OfficeCareers = officeQueries.GetCareers();
            bundle.OfficeJurisdictions = officeQueries.GetJurisdictions();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WarfareCampaign))
        {
            IWarfareCampaignQueries warfareQueries = queries.GetRequired<IWarfareCampaignQueries>();
            bundle.Campaigns = warfareQueries.GetCampaigns();
            bundle.CampaignMobilizationSignals = warfareQueries.GetMobilizationSignals();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.NarrativeProjection))
        {
            bundle.Notifications = queries.GetRequired<INarrativeProjectionQueries>().GetNotifications();
        }

        bundle.PlayerCommands = BuildPlayerCommandSurface(bundle);
        bundle.GovernanceSettlements = BuildGovernanceSettlements(bundle);
        bundle.GovernanceFocus = BuildGovernanceFocus(bundle.GovernanceSettlements);
        bundle.GovernanceDocket = BuildGovernanceDocket(
            bundle.GovernanceFocus,
            bundle.GovernanceSettlements,
            bundle.Notifications,
            bundle.PlayerCommands.Receipts);
        bundle.HallDocket = BuildHallDocketStack(bundle);
        bundle.Debug = BuildDebugSnapshot(simulation, bundle.Notifications);
        return bundle;
    }

    private static QueryRegistry BuildQueries(GameSimulation simulation)
    {
        QueryRegistry queries = new();
        foreach (IModuleRunner module in simulation.Modules
                     .OrderBy(static module => module.Phase)
                     .ThenBy(static module => module.ExecutionOrder)
                     .ThenBy(static module => module.ModuleKey, StringComparer.Ordinal))
        {
            if (!simulation.FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!simulation.TryGetModuleState(module.ModuleKey, out object? state) || state is null)
            {
                throw new InvalidOperationException($"Enabled module {module.ModuleKey} has no state for read-model export.");
            }

            module.RegisterQueries(state, queries);
        }

        return queries;
    }

    private static PlayerCommandSurfaceSnapshot BuildPlayerCommandSurface(PresentationReadModelBundle bundle)
    {
        return new PlayerCommandSurfaceSnapshot
        {
            Affordances = BuildPlayerCommandAffordances(bundle),
            Receipts = BuildPlayerCommandReceipts(bundle),
        };
    }

}
