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
            ISocialMemoryAndRelationsQueries socialQueries = queries.GetRequired<ISocialMemoryAndRelationsQueries>();
            bundle.ClanNarratives = socialQueries.GetClanNarratives();
            bundle.SocialMemories = socialQueries.GetMemories();
        }

        bundle.PersonDossiers = BuildPersonDossiers(simulation.FeatureManifest, queries);

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.WorldSettlements))
        {
            bundle.Settlements = queries.GetRequired<IWorldSettlementsQueries>().GetSettlements();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PopulationAndHouseholds))
        {
            IPopulationAndHouseholdsQueries populationQueries = queries.GetRequired<IPopulationAndHouseholdsQueries>();
            bundle.Households = populationQueries.GetHouseholds();
            bundle.PopulationSettlements = populationQueries.GetSettlements();
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
            bundle.CampaignAftermathDockets = warfareQueries.GetAftermathDockets();
        }

        if (simulation.FeatureManifest.IsEnabled(KnownModuleKeys.NarrativeProjection))
        {
            bundle.Notifications = queries.GetRequired<INarrativeProjectionQueries>().GetNotifications();
        }

        bundle.HouseholdSocialPressures = BuildHouseholdSocialPressures(bundle);
        bundle.FidelityScale = BuildFidelityScale(bundle);
        bundle.SettlementMobilities = BuildSettlementMobilities(simulation.FeatureManifest, queries, bundle);
        bundle.PlayerCommands = BuildPlayerCommandSurface(bundle);
        bundle.GovernanceSettlements = BuildGovernanceSettlements(bundle);
        bundle.GovernanceFocus = BuildGovernanceFocus(bundle.GovernanceSettlements);
        bundle.GovernanceDocket = BuildGovernanceDocket(
            bundle.GovernanceFocus,
            bundle.GovernanceSettlements,
            bundle.Notifications,
            bundle.PlayerCommands.Receipts,
            bundle.Households,
            bundle.OfficeJurisdictions,
            bundle.Clans,
            bundle.SocialMemories);
        bundle.HallDocket = BuildHallDocketStack(bundle);
        bundle.InfluenceFootprint = BuildInfluenceFootprint(bundle);
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
        PlayerCommandAffordanceSnapshot[] affordances = BuildPlayerCommandAffordances(bundle).ToArray();
        PlayerCommandReceiptSnapshot[] receipts = BuildPlayerCommandReceipts(bundle).ToArray();

        return new PlayerCommandSurfaceSnapshot
        {
            Affordances = affordances,
            Receipts = receipts,
            PersonnelFlowReadinessSummary = BuildPlayerCommandSurfacePersonnelFlowReadinessSummary(affordances, receipts),
        };
    }

    private static string BuildPlayerCommandSurfacePersonnelFlowReadinessSummary(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts)
    {
        int readableAffordances = affordances.Count(static affordance =>
            !string.IsNullOrWhiteSpace(affordance.PersonnelFlowReadinessSummary));
        int enabledAffordances = affordances.Count(static affordance =>
            affordance.IsEnabled
            && !string.IsNullOrWhiteSpace(affordance.PersonnelFlowReadinessSummary));
        int readableReceipts = receipts.Count(static receipt =>
            !string.IsNullOrWhiteSpace(receipt.PersonnelFlowReadinessSummary));
        int settlementCount = affordances
            .Where(static affordance => !string.IsNullOrWhiteSpace(affordance.PersonnelFlowReadinessSummary))
            .Select(static affordance => affordance.SettlementId.Value)
            .Concat(receipts
                .Where(static receipt => !string.IsNullOrWhiteSpace(receipt.PersonnelFlowReadinessSummary))
                .Select(static receipt => receipt.SettlementId.Value))
            .Distinct()
            .Count();

        if (readableAffordances == 0 && readableReceipts == 0)
        {
            return string.Empty;
        }

        return
            $"人员流动命令预备汇总：{settlementCount}处地方有本户回应读法，{enabledAffordances}/{readableAffordances}道命令可承接，{readableReceipts}条回执保留读法；" +
            "只汇总已投影的人员流动预备读回，不解析ReadbackSummary、回执文案或事件Summary；" +
            "PopulationAndHouseholds拥有本户回应，PersonRegistry只保身份/FidelityRing，UI/Unity只复制投影字段；不是直接调人、迁人、召人命令。";
    }

}
