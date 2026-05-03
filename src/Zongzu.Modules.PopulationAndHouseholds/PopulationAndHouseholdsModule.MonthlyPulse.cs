using System;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private static void ApplyXunPulseAdjustments(
        SimulationXun currentXun,
        SettlementSnapshot settlement,
        int clanSupport,
        PopulationHouseholdState household)
    {
        int supportBuffer = clanSupport >= 60 ? 1 : 0;

        switch (currentXun)
        {
            case SimulationXun.Shangxun:
            {
                int distressDelta = household.DebtPressure >= 50 ? 1 : 0;
                if (distressDelta > 0 && supportBuffer > 0)
                {
                    distressDelta -= 1;
                }

                household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);

                int debtDelta = household.Distress >= 60 && supportBuffer == 0 ? 1 : 0;
                household.DebtPressure = Math.Clamp(household.DebtPressure + debtDelta, 0, 100);
                break;
            }
            case SimulationXun.Zhongxun:
            {
                int laborDrop = household.Distress >= 60 ? 1 : 0;
                household.LaborCapacity = Math.Clamp(household.LaborCapacity - laborDrop, 0, 100);

                int distressDelta = settlement.Prosperity < 45 && supportBuffer == 0 ? 1 : 0;
                household.Distress = Math.Clamp(household.Distress + distressDelta, 0, 100);
                break;
            }
            case SimulationXun.Xiaxun:
            {
                int migrationDelta = 0;
                if (settlement.Security < 45)
                {
                    migrationDelta += 1;
                }

                if (household.DebtPressure >= 60)
                {
                    migrationDelta += 1;
                }

                if (migrationDelta > 0 && supportBuffer > 0)
                {
                    migrationDelta -= 1;
                }

                household.MigrationRisk = Math.Clamp(household.MigrationRisk + migrationDelta, 0, 100);
                household.IsMigrating = ResolveMigrationStatus(household);
                break;
            }
        }
    }

    private static int GetClanSupportReserve(IFamilyCoreQueries familyQueries, ClanId? sponsorClanId)
    {
        return sponsorClanId is null ? 0 : familyQueries.GetRequiredClan(sponsorClanId.Value).SupportReserve;
    }

    private static int ComputeDebtDelta(int distress)
    {
        if (distress >= 75)
        {
            return 3;
        }

        if (distress >= 55)
        {
            return 2;
        }

        if (distress >= 40)
        {
            return 1;
        }

        return -1;
    }

    private static int ComputeLaborDelta(int prosperity, int distress)
    {
        int delta = prosperity >= 58 ? 1 : prosperity <= 42 ? -1 : 0;
        if (distress >= 70)
        {
            delta -= 1;
        }

        return delta;
    }

    private static int ComputeMigrationDelta(int security, int distress)
    {
        int delta = distress >= 70 ? 2 : distress >= 50 ? 1 : -1;
        if (security < 40)
        {
            delta += 1;
        }

        return delta;
    }

    private static bool ResolveMigrationStatus(PopulationHouseholdState household)
    {
        return ResolveMigrationStatus(
            household,
            PopulationHouseholdMobilityRulesData.DefaultMonthlyRuntimeMigrationStatusThreshold);
    }

    private static bool ResolveMigrationStatus(PopulationHouseholdState household, int migrationStatusThreshold)
    {
        return household.MigrationRisk >= migrationStatusThreshold;
    }

}
