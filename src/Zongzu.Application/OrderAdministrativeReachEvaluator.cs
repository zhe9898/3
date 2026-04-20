using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

internal static class OrderAdministrativeReachEvaluator
{
    public static OrderAdministrativeReachProfile Resolve(GameSimulation simulation, SettlementId settlementId)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        QueryRegistry queries = BuildQueries(simulation);
        IOfficeAndCareerQueries officeQueries;
        try
        {
            officeQueries = queries.GetRequired<IOfficeAndCareerQueries>();
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        try
        {
            return Evaluate(officeQueries.GetRequiredJurisdiction(settlementId));
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }
    }

    public static OrderAdministrativeReachProfile Evaluate(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        int supportSignal =
            jurisdiction.JurisdictionLeverage
            + jurisdiction.ClerkDependence
            + (jurisdiction.AuthorityTier * 10);
        int frictionSignal =
            jurisdiction.AdministrativeTaskLoad
            + jurisdiction.PetitionPressure
            + (jurisdiction.PetitionBacklog / 2);
        int netSignal = supportSignal - frictionSignal;

        if (netSignal >= 40)
        {
            return new OrderAdministrativeReachProfile(
                3,
                5,
                -4,
                -3,
                "县署肯出手，文移与差役都跟得上。",
                "县署肯出手，此令多半能照行到底。");
        }

        if (netSignal >= 15)
        {
            return new OrderAdministrativeReachProfile(
                1,
                2,
                -2,
                -1,
                "县署还能接得住，文移差役尚能随令。",
                "县署还能接得住，此令大体跟得上。");
        }

        if (netSignal <= -40)
        {
            return new OrderAdministrativeReachProfile(
                -3,
                -5,
                4,
                3,
                "县署拥案未解，文移不畅，路上只得勉强敷衍。",
                "县署拥案未解，此令多半只落在文面，地面跟得慢。");
        }

        if (netSignal <= -15)
        {
            return new OrderAdministrativeReachProfile(
                -1,
                -2,
                2,
                1,
                "县署案牍偏重，差役跟得慢，只能先做半套。",
                "县署案牍偏重，此令常要拖成半套。");
        }

        return OrderAdministrativeReachProfile.Neutral;
    }

    public static string AppendReceiptSummary(string text, OrderAdministrativeReachProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.SummaryTail))
        {
            return text;
        }

        return $"{text}{profile.SummaryTail}";
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
                throw new InvalidOperationException($"Enabled module {module.ModuleKey} has no state for player-command queries.");
            }

            module.RegisterQueries(state, queries);
        }

        return queries;
    }
}

internal readonly record struct OrderAdministrativeReachProfile(
    int BenefitShift,
    int ShieldingShift,
    int BacklashShift,
    int LeakageShift,
    string SummaryTail,
    string ExecutionSummary = "")
{
    public bool HasModifier => BenefitShift != 0 || ShieldingShift != 0 || BacklashShift != 0 || LeakageShift != 0;

    public static OrderAdministrativeReachProfile Neutral => new(
        0,
        0,
        0,
        0,
        string.Empty,
        "此地眼下多凭本地人手与地面情势，官面帮衬未显。");
}
