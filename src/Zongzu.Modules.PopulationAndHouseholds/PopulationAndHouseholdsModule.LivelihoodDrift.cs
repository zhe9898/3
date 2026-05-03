using Zongzu.Contracts;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed partial class PopulationAndHouseholdsModule
{
    private readonly record struct LivelihoodDriftResult(
        LivelihoodType Previous,
        LivelihoodType Current,
        string Reason);

    private static bool TryApplyMonthlyLivelihoodDrift(
        PopulationHouseholdState household,
        SettlementSnapshot settlement,
        out LivelihoodDriftResult result)
    {
        LivelihoodType previous = household.Livelihood;
        LivelihoodType current = ResolveMonthlyLivelihood(household, settlement);
        if (current == previous)
        {
            result = default;
            return false;
        }

        household.Livelihood = current;
        result = new LivelihoodDriftResult(previous, current, BuildLivelihoodDriftReason(household, settlement, current));
        return true;
    }

    private static LivelihoodType ResolveMonthlyLivelihood(PopulationHouseholdState household, SettlementSnapshot settlement)
    {
        if (household.Distress >= 85 && household.DebtPressure >= 80 && household.LaborCapacity < 35)
        {
            return LivelihoodType.Vagrant;
        }

        if (household.IsMigrating || household.MigrationRisk >= 80)
        {
            return household.Livelihood == LivelihoodType.Vagrant
                ? LivelihoodType.Vagrant
                : LivelihoodType.SeasonalMigrant;
        }

        if (household.Livelihood == LivelihoodType.Smallholder
            && household.LandHolding is > 0 and < 20
            && household.DebtPressure >= 65)
        {
            return LivelihoodType.Tenant;
        }

        if (household.Livelihood == LivelihoodType.Tenant
            && household.Distress >= 70
            && household.LaborCapacity < 45)
        {
            return LivelihoodType.HiredLabor;
        }

        if (household.Livelihood is LivelihoodType.PettyTrader or LivelihoodType.Artisan or LivelihoodType.Boatman
            && household.DebtPressure >= 80
            && household.Distress >= 70)
        {
            return LivelihoodType.HiredLabor;
        }

        if (household.Livelihood is LivelihoodType.HiredLabor or LivelihoodType.SeasonalMigrant
            && settlement.Prosperity >= 58
            && settlement.Security >= 55
            && household.Distress <= 40
            && household.DebtPressure <= 40
            && household.LandHolding >= 25
            && household.GrainStore >= 45)
        {
            return LivelihoodType.Smallholder;
        }

        if (household.Livelihood == LivelihoodType.Vagrant
            && household.Distress <= 45
            && household.DebtPressure <= 50
            && household.LaborCapacity >= 45
            && settlement.Security >= 50)
        {
            return LivelihoodType.HiredLabor;
        }

        return household.Livelihood;
    }

    private static string BuildLivelihoodDriftReason(
        PopulationHouseholdState household,
        SettlementSnapshot settlement,
        LivelihoodType current)
    {
        return current switch
        {
            LivelihoodType.Vagrant => "债压、民困和丁力一齐断裂，先从家计滑成游食",
            LivelihoodType.SeasonalMigrant => "迁徙风险已过阈值，远路成为压力出口",
            LivelihoodType.Tenant => "地少债重，先从自耕滑向佃作",
            LivelihoodType.HiredLabor => "家计不稳，只能把劳力先卖到短工、脚役或佣作处",
            LivelihoodType.Smallholder => $"{settlement.Name}市况和治安稍稳，存粮与薄地足以把家户拉回小农轨道",
            _ => $"在{settlement.Name}的压力面中改换生计",
        };
    }

    private static bool IsLivelihoodCollapseDrift(LivelihoodDriftResult result)
    {
        return result.Current == LivelihoodType.Vagrant
            || (result.Previous is LivelihoodType.Smallholder or LivelihoodType.Tenant
                && result.Current == LivelihoodType.HiredLabor);
    }

    private static string RenderLivelihoodForDiff(LivelihoodType livelihood)
    {
        return livelihood switch
        {
            LivelihoodType.Smallholder => "小农",
            LivelihoodType.Tenant => "佃作",
            LivelihoodType.HiredLabor => "雇工",
            LivelihoodType.Artisan => "手艺",
            LivelihoodType.PettyTrader => "小贩",
            LivelihoodType.Boatman => "船脚",
            LivelihoodType.DomesticServant => "仆役",
            LivelihoodType.YamenRunner => "衙前差使",
            LivelihoodType.SeasonalMigrant => "季节外出",
            LivelihoodType.Vagrant => "游食",
            _ => "生计未明",
        };
    }

    private static int ComputeLivelihoodDistressBaseline(LivelihoodType livelihood)
    {
        return livelihood switch
        {
            LivelihoodType.Vagrant => 3,
            LivelihoodType.SeasonalMigrant => 2,
            LivelihoodType.Tenant => 1,
            LivelihoodType.HiredLabor => 1,
            LivelihoodType.Boatman => 1,
            LivelihoodType.DomesticServant => 0,
            LivelihoodType.YamenRunner => 0,
            LivelihoodType.Smallholder => 0,
            LivelihoodType.Artisan => -1,
            LivelihoodType.PettyTrader => -1,
            _ => 0,
        };
    }

}