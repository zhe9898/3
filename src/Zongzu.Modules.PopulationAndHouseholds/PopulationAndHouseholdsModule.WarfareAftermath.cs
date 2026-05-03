using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private static void ApplyWarfareCampaignAftermathEvents(ModuleEventHandlingScope<PopulationAndHouseholdsState> scope)
    {
        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        bool anyHouseholdChanged = false;
        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            PopulationHouseholdState[] households = scope.State.Households
                .Where(household => household.SettlementId == bundle.SettlementId)
                .OrderBy(static household => household.Id.Value)
                .ToArray();

            if (households.Length == 0)
            {
                continue;
            }

            int distressDelta = ComputeCampaignDistressDelta(bundle, campaign);
            int debtDelta = ComputeCampaignDebtDelta(bundle, campaign);
            int migrationDelta = ComputeCampaignMigrationDelta(bundle, campaign);
            int laborDrop = ComputeCampaignLaborDrop(bundle, campaign);

            foreach (PopulationHouseholdState household in households)
            {
                household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);
                household.DebtPressure = Math.Clamp(household.DebtPressure + debtDelta, 0, 100);
                household.MigrationRisk = Math.Clamp(household.MigrationRisk + migrationDelta, 0, 100);
                household.LaborCapacity = Math.Clamp(household.LaborCapacity - laborDrop, 0, 100);
                household.IsMigrating = ResolveMigrationStatus(household);
                anyHouseholdChanged = true;
            }

            scope.RecordDiff(
                $"{campaign.AnchorSettlementName}战后余波所及，民困增{distressDelta}，债压增{debtDelta}，迁徙增{migrationDelta}，丁力减{laborDrop}。{campaign.LastAftermathSummary}",
                bundle.SettlementId.Value.ToString());

            if (households.Any(static household => household.MigrationRisk >= 80))
            {
                PopulationHouseholdState migratingHousehold = households
                    .OrderByDescending(static household => household.MigrationRisk)
                    .ThenBy(static household => household.Id.Value)
                    .First();
                scope.Emit(PopulationEventNames.MigrationStarted, $"{migratingHousehold.HouseholdName}受战后余波所逼，已有远徙之意。", bundle.SettlementId.Value.ToString());
            }

            if (households.Any(static household => household.DebtPressure >= 85 && household.Distress >= 80))
            {
                PopulationHouseholdState collapsedHousehold = households
                    .OrderByDescending(static household => household.DebtPressure + household.Distress)
                    .ThenBy(static household => household.Id.Value)
                    .First();
                scope.Emit(PopulationEventNames.LivelihoodCollapsed, $"{collapsedHousehold.HouseholdName}受{campaign.AnchorSettlementName}战后余波牵压，生计顿敝。", bundle.SettlementId.Value.ToString());
            }
        }

        if (anyHouseholdChanged)
        {
            SynchronizeMembershipLivelihoodsAndActivities(scope.State);
            RebuildSettlementSummaries(scope.State, scope.TryGetQuery<IPersonRegistryQueries>());
        }
    }

    private static int ComputeCampaignDistressDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignMobilized ? 1 : 0;
        delta += bundle.CampaignPressureRaised ? 2 : 0;
        delta += bundle.CampaignSupplyStrained ? 3 : 0;
        delta += bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 60) / 20;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignDebtDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignPressureRaised ? 1 : 0;
        delta += bundle.CampaignSupplyStrained ? 2 : 0;
        delta += bundle.CampaignAftermathRegistered ? 1 : 0;
        delta += Math.Max(0, 50 - campaign.SupplyState) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignMigrationDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = bundle.CampaignAftermathRegistered ? 2 : 0;
        delta += bundle.CampaignSupplyStrained ? 1 : 0;
        delta += Math.Max(0, campaign.FrontPressure - 55) / 18;
        return Math.Max(1, delta);
    }

    private static int ComputeCampaignLaborDrop(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int drop = bundle.CampaignMobilized ? 1 : 0;
        drop += bundle.CampaignPressureRaised ? 1 : 0;
        drop += bundle.CampaignSupplyStrained ? 2 : 0;
        drop += Math.Max(0, campaign.MobilizedForceCount - 24) / 24;
        return Math.Max(1, drop);
    }

}
