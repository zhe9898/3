using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static FidelityScaleSnapshot BuildFidelityScale(PresentationReadModelBundle bundle)
    {
        int core = bundle.PersonDossiers.Count(static dossier => dossier.FidelityRing == FidelityRing.Core);
        int local = bundle.PersonDossiers.Count(static dossier => dossier.FidelityRing == FidelityRing.Local);
        int regional = bundle.PersonDossiers.Count(static dossier => dossier.FidelityRing == FidelityRing.Regional);
        int named = bundle.PersonDossiers.Count;

        string summary = named == 0
            ? "人物精度环尚未投出；远处仍由人口池承接。"
            : $"近处细：Core {core}人；地方半细：Local {local}人；远处汇总：Regional {regional}人。";

        return new FidelityScaleSnapshot
        {
            CorePersonCount = core,
            LocalPersonCount = local,
            RegionalPersonCount = regional,
            NamedPersonCount = named,
            Summary = summary,
            FocusBudgetSummary = "玩家身边活，天下不逐人硬算；压力只把少量远处人物拉进地方读回，其余仍由劳力、婚配、流徙池承接。",
            SourceModuleKeys = [KnownModuleKeys.PersonRegistry],
        };
    }

    private static IReadOnlyList<SettlementMobilitySnapshot> BuildSettlementMobilities(
        FeatureManifest manifest,
        QueryRegistry queries,
        PresentationReadModelBundle bundle)
    {
        if (!manifest.IsEnabled(KnownModuleKeys.PopulationAndHouseholds))
        {
            return [];
        }

        IPopulationAndHouseholdsQueries populationQueries = queries.GetRequired<IPopulationAndHouseholdsQueries>();
        IPersonRegistryQueries? personQueries = manifest.IsEnabled(KnownModuleKeys.PersonRegistry)
            ? queries.GetRequired<IPersonRegistryQueries>()
            : null;

        Dictionary<int, string> settlementNames = bundle.Settlements
            .GroupBy(static settlement => settlement.Id.Value)
            .ToDictionary(static group => group.Key, static group => group.First().Name);
        Dictionary<int, LaborPoolEntrySnapshot> laborPools = populationQueries.GetLaborPools()
            .GroupBy(static entry => entry.SettlementId.Value)
            .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<int, MarriagePoolEntrySnapshot> marriagePools = populationQueries.GetMarriagePools()
            .GroupBy(static entry => entry.SettlementId.Value)
            .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<int, MigrationPoolEntrySnapshot> migrationPools = populationQueries.GetMigrationPools()
            .GroupBy(static entry => entry.SettlementId.Value)
            .ToDictionary(static group => group.Key, static group => group.First());
        Dictionary<int, HouseholdPressureSnapshot[]> householdsBySettlement = bundle.Households
            .GroupBy(static household => household.SettlementId.Value)
            .ToDictionary(static group => group.Key, static group => group.OrderBy(static household => household.Id.Value).ToArray());
        Dictionary<HouseholdId, HouseholdPressureSnapshot> householdsById = bundle.Households
            .GroupBy(static household => household.Id)
            .ToDictionary(static group => group.Key, static group => group.First());

        Dictionary<int, List<HouseholdMembershipSnapshot>> membershipsBySettlement = new();
        foreach (HouseholdMembershipSnapshot membership in populationQueries.GetMemberships()
                     .OrderBy(static membership => membership.PersonId.Value))
        {
            if (!householdsById.TryGetValue(membership.HouseholdId, out HouseholdPressureSnapshot? household))
            {
                continue;
            }

            int settlementKey = household.SettlementId.Value;
            if (!membershipsBySettlement.TryGetValue(settlementKey, out List<HouseholdMembershipSnapshot>? list))
            {
                list = [];
                membershipsBySettlement[settlementKey] = list;
            }

            list.Add(membership);
        }

        int[] settlementIds = bundle.PopulationSettlements
            .Select(static settlement => settlement.SettlementId.Value)
            .Concat(laborPools.Keys)
            .Concat(marriagePools.Keys)
            .Concat(migrationPools.Keys)
            .Concat(householdsBySettlement.Keys)
            .Distinct()
            .Order()
            .ToArray();

        return settlementIds
            .Select(settlementKey =>
            {
                laborPools.TryGetValue(settlementKey, out LaborPoolEntrySnapshot? labor);
                marriagePools.TryGetValue(settlementKey, out MarriagePoolEntrySnapshot? marriage);
                migrationPools.TryGetValue(settlementKey, out MigrationPoolEntrySnapshot? migration);
                householdsBySettlement.TryGetValue(settlementKey, out HouseholdPressureSnapshot[]? households);
                membershipsBySettlement.TryGetValue(settlementKey, out List<HouseholdMembershipSnapshot>? memberships);
                string settlementName = settlementNames.TryGetValue(settlementKey, out string? name)
                    ? name
                    : $"Settlement {settlementKey}";

                int namedLocalPersons = CountNamedLocalPersons(personQueries, memberships);
                int namedMigratingPersons = memberships?.Count(static membership => membership.Activity == PersonActivity.Migrating) ?? 0;
                int migratingHouseholds = households?.Count(static household => household.IsMigrating) ?? 0;

                return new SettlementMobilitySnapshot
                {
                    SettlementId = new SettlementId(settlementKey),
                    SettlementName = settlementName,
                    AvailableLabor = labor?.AvailableLabor ?? 0,
                    LaborDemand = labor?.LaborDemand ?? 0,
                    SeasonalSurplus = labor?.SeasonalSurplus ?? 0,
                    WageLevel = labor?.WageLevel ?? 0,
                    EligibleMales = marriage?.EligibleMales ?? 0,
                    EligibleFemales = marriage?.EligibleFemales ?? 0,
                    MatchDifficulty = marriage?.MatchDifficulty ?? 0,
                    OutflowPressure = migration?.OutflowPressure ?? 0,
                    InflowPressure = migration?.InflowPressure ?? 0,
                    FloatingPopulation = migration?.FloatingPopulation ?? 0,
                    NamedLocalPersons = namedLocalPersons,
                    NamedMigratingPersons = namedMigratingPersons,
                    PoolThicknessSummary = BuildPoolThicknessSummary(settlementName, labor, marriage, migration),
                    MovementReadbackSummary = BuildMovementReadbackSummary(settlementName, migration, migratingHouseholds, namedMigratingPersons),
                    FocusReadbackSummary = BuildFocusReadbackSummary(settlementName, namedLocalPersons, migration),
                    SourceModuleKeys = DistinctNonEmpty(
                        KnownModuleKeys.PopulationAndHouseholds,
                        personQueries is null ? string.Empty : KnownModuleKeys.PersonRegistry,
                        settlementNames.ContainsKey(settlementKey) ? KnownModuleKeys.WorldSettlements : string.Empty),
                };
            })
            .ToArray();
    }

    private static int CountNamedLocalPersons(
        IPersonRegistryQueries? personQueries,
        IReadOnlyList<HouseholdMembershipSnapshot>? memberships)
    {
        if (personQueries is null || memberships is null || memberships.Count == 0)
        {
            return 0;
        }

        int count = 0;
        foreach (HouseholdMembershipSnapshot membership in memberships)
        {
            if (personQueries.TryGetPerson(membership.PersonId, out PersonRecord person)
                && person.IsAlive
                && person.FidelityRing is FidelityRing.Core or FidelityRing.Local)
            {
                count += 1;
            }
        }

        return count;
    }

    private static string BuildPoolThicknessSummary(
        string settlementName,
        LaborPoolEntrySnapshot? labor,
        MarriagePoolEntrySnapshot? marriage,
        MigrationPoolEntrySnapshot? migration)
    {
        if (labor is null && marriage is null && migration is null)
        {
            return $"{settlementName}尚未生成劳力、婚配或流徙池。";
        }

        string laborText = labor is null
            ? "劳力池未起"
            : $"劳力{labor.AvailableLabor}/{labor.LaborDemand}，余缺{labor.SeasonalSurplus}，工价{labor.WageLevel}";
        string marriageText = marriage is null
            ? "婚配池未起"
            : $"婚配男{marriage.EligibleMales}女{marriage.EligibleFemales}，难度{marriage.MatchDifficulty}";
        string migrationText = migration is null
            ? "流徙池未起"
            : $"流出{migration.OutflowPressure}，流入{migration.InflowPressure}，浮动{migration.FloatingPopulation}";

        return $"{settlementName}池化读法：{laborText}；{marriageText}；{migrationText}。";
    }

    private static string BuildMovementReadbackSummary(
        string settlementName,
        MigrationPoolEntrySnapshot? migration,
        int migratingHouseholds,
        int namedMigratingPersons)
    {
        if (migration is null)
        {
            return $"{settlementName}人员流动暂由家户压力读回，尚无区域流徙池。";
        }

        return $"{settlementName}人员流动由PopulationAndHouseholds承接：{migratingHouseholds}户已动迁念，{namedMigratingPersons}名可见人物处在迁徙活动，其余远处人群仍压在流徙池。";
    }

    private static string BuildFocusReadbackSummary(
        string settlementName,
        int namedLocalPersons,
        MigrationPoolEntrySnapshot? migration)
    {
        int outflow = migration?.OutflowPressure ?? 0;
        if (namedLocalPersons > 0 && outflow >= 70)
        {
            return $"{settlementName}压力已把{namedLocalPersons}名人物拉进近处读回；不是全县逐人硬算。";
        }

        if (namedLocalPersons > 0)
        {
            return $"{settlementName}有{namedLocalPersons}名人物保留近处读法，其余仍按区域池汇总。";
        }

        return $"{settlementName}当前以远处汇总为主：玩家可读池厚，不直接操纵每个行人。";
    }
}
