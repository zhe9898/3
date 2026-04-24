using System;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryCommandContext
{
    public OrderAndBanditryState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();

    public IOfficeAndCareerQueries? OfficeQueries { get; init; }
}

public static class OrderAndBanditryCommandResolver
{
    public static PlayerCommandResult IssueIntent(OrderAndBanditryCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        PlayerCommandRequest command = context.Command;
        OrderAdministrativeReachProfile administrativeReach = ResolveAdministrativeReach(context.OfficeQueries, command.SettlementId);
        OrderPublicLifeCommandResult resolution = new OrderAndBanditryModule().HandlePublicLifeCommand(
            context.State,
            new OrderPublicLifeCommand
            {
                SettlementId = command.SettlementId,
                CommandName = command.CommandName,
                CommandLabel = DeterminePublicLifeCommandLabel(command.CommandName),
                BenefitShift = administrativeReach.BenefitShift,
                ShieldingShift = administrativeReach.ShieldingShift,
                BacklashShift = administrativeReach.BacklashShift,
                LeakageShift = administrativeReach.LeakageShift,
                ReachSummaryTail = administrativeReach.SummaryTail,
            });

        return new PlayerCommandResult
        {
            Accepted = resolution.Accepted,
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = resolution.Label,
            Summary = resolution.Summary,
            TargetLabel = resolution.Accepted ? $"据点 {command.SettlementId.Value}" : string.Empty,
        };
    }

    public static string DeterminePublicLifeCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.EscortRoadReport => "催护一路",
            PlayerCommandNames.FundLocalWatch => "添雇巡丁",
            PlayerCommandNames.SuppressBanditry => "严缉路匪",
            PlayerCommandNames.NegotiateWithOutlaws => "遣人议路",
            PlayerCommandNames.TolerateDisorder => "暂缓穷追",
            _ => commandName,
        };
    }

    public static string DetermineAdministrativeReachExecutionSummary(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        return EvaluateAdministrativeReach(jurisdiction).ExecutionSummary;
    }

    private static OrderAdministrativeReachProfile ResolveAdministrativeReach(
        IOfficeAndCareerQueries? officeQueries,
        SettlementId settlementId)
    {
        if (officeQueries is null)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        try
        {
            return EvaluateAdministrativeReach(officeQueries.GetRequiredJurisdiction(settlementId));
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }
    }

    private static OrderAdministrativeReachProfile EvaluateAdministrativeReach(JurisdictionAuthoritySnapshot? jurisdiction)
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
                "县署案前偏重，差役跟得慢，只能先做半套。",
                "县署案前偏重，此令常要拖成半套。");
        }

        return OrderAdministrativeReachProfile.Neutral;
    }

    private readonly record struct OrderAdministrativeReachProfile(
        int BenefitShift,
        int ShieldingShift,
        int BacklashShift,
        int LeakageShift,
        string SummaryTail,
        string ExecutionSummary)
    {
        public static OrderAdministrativeReachProfile Neutral => new(
            0,
            0,
            0,
            0,
            string.Empty,
            "此地眼下多凭本地人手与地面情势，官面帮衬未显。");
    }
}
