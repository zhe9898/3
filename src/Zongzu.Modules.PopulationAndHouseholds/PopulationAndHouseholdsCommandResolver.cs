using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed class PopulationAndHouseholdsCommandContext
{
    public PopulationAndHouseholdsState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();
}

public static class PopulationAndHouseholdsCommandResolver
{
    public static PlayerCommandResult IssueIntent(PopulationAndHouseholdsCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.State);
        ArgumentNullException.ThrowIfNull(context.Command);

        PlayerCommandRequest command = context.Command;
        if (!IsLocalResponseCommand(command.CommandName))
        {
            return BuildRejectedResult(command, $"PopulationAndHouseholds does not handle player command {command.CommandName}.");
        }

        PopulationHouseholdState? household = ResolveTargetHousehold(context.State, command);
        if (household is null)
        {
            return BuildRejectedResult(command, $"此地暂无可由本户先处理的家户压力：{command.SettlementId.Value}。");
        }

        return command.CommandName switch
        {
            PlayerCommandNames.RestrictNightTravel => ApplyRestrictNightTravel(command, household),
            PlayerCommandNames.PoolRunnerCompensation => ApplyPoolRunnerCompensation(command, household),
            PlayerCommandNames.SendHouseholdRoadMessage => ApplySendHouseholdRoadMessage(command, household),
            _ => BuildRejectedResult(command, $"PopulationAndHouseholds does not handle player command {command.CommandName}."),
        };
    }

    public static string DetermineLocalResponseCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.RestrictNightTravel => "暂缩夜行",
            PlayerCommandNames.PoolRunnerCompensation => "凑钱赔脚户",
            PlayerCommandNames.SendHouseholdRoadMessage => "遣少丁递信",
            _ => commandName,
        };
    }

    private static PlayerCommandResult ApplyRestrictNightTravel(PlayerCommandRequest command, PopulationHouseholdState household)
    {
        int oldMigrationRisk = household.MigrationRisk;
        int migrationRelief = household.MigrationRisk >= 65 ? 10 : 7;
        int laborCost = household.LaborCapacity >= 35 ? 5 : 3;
        int debtCost = household.DebtPressure >= 70 ? 2 : 1;
        int distressDrift = household.Distress >= 75 ? 1 : 0;

        household.MigrationRisk = Clamp100(household.MigrationRisk - migrationRelief);
        household.LaborCapacity = Clamp100(household.LaborCapacity - laborCost);
        household.DebtPressure = Clamp100(household.DebtPressure + debtCost);
        household.Distress = Clamp100(household.Distress + distressDrift);
        household.IsMigrating = household.MigrationRisk >= 80 || (household.IsMigrating && household.MigrationRisk >= 65);

        string outcomeCode = household.LaborCapacity < 25 || household.DebtPressure >= 85
            ? HouseholdLocalResponseOutcomeCodes.Strained
            : oldMigrationRisk - household.MigrationRisk >= 8
                ? HouseholdLocalResponseOutcomeCodes.Relieved
                : HouseholdLocalResponseOutcomeCodes.Contained;
        string summary = outcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"{household.HouseholdName}暂缩夜行，夜路险头被压住一截，但丁力和债压吃紧。"
            : $"{household.HouseholdName}暂缩夜行，少走夜路、渡口和黑路，迁徙之念缓到{household.MigrationRisk}。";

        ApplyLocalResponseReceipt(
            household,
            command,
            outcomeCode,
            HouseholdLocalResponseTraceCodes.NightTravelRestricted,
            summary);
        return BuildAcceptedResult(command, household);
    }

    private static PlayerCommandResult ApplyPoolRunnerCompensation(PlayerCommandRequest command, PopulationHouseholdState household)
    {
        int debtCost = household.DebtPressure >= 70 ? 8 : 5;
        int distressRelief = household.Distress >= 60 ? 8 : 5;
        int migrationRelief = household.MigrationRisk >= 45 ? 4 : 2;

        household.DebtPressure = Clamp100(household.DebtPressure + debtCost);
        household.Distress = Clamp100(household.Distress - distressRelief);
        household.MigrationRisk = Clamp100(household.MigrationRisk - migrationRelief);
        household.LaborCapacity = Clamp100(household.LaborCapacity - 1);
        household.IsMigrating = household.MigrationRisk >= 80 || (household.IsMigrating && household.MigrationRisk >= 65);

        string outcomeCode = household.DebtPressure >= 82
            ? HouseholdLocalResponseOutcomeCodes.Strained
            : HouseholdLocalResponseOutcomeCodes.Contained;
        string summary = outcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"{household.HouseholdName}凑钱赔了脚户误读，街口话头暂压，债压却抬到{household.DebtPressure}。"
            : $"{household.HouseholdName}凑钱赔了脚户误读，街口误会先缓，民困降到{household.Distress}。";

        ApplyLocalResponseReceipt(
            household,
            command,
            outcomeCode,
            HouseholdLocalResponseTraceCodes.RunnerMisreadSettledLocally,
            summary);
        return BuildAcceptedResult(command, household);
    }

    private static PlayerCommandResult ApplySendHouseholdRoadMessage(PlayerCommandRequest command, PopulationHouseholdState household)
    {
        bool laborThin = household.LaborCapacity < 30;
        int laborCost = laborThin ? 4 : 6;
        int distressRelief = laborThin ? 1 : 4;
        int migrationDelta = laborThin ? 2 : -3;

        household.LaborCapacity = Clamp100(household.LaborCapacity - laborCost);
        household.Distress = Clamp100(household.Distress - distressRelief);
        household.MigrationRisk = Clamp100(household.MigrationRisk + migrationDelta);
        household.DebtPressure = Clamp100(household.DebtPressure + 1);
        household.IsMigrating = household.MigrationRisk >= 80 || (household.IsMigrating && household.MigrationRisk >= 65);

        string outcomeCode = household.LaborCapacity < 25
            ? HouseholdLocalResponseOutcomeCodes.Strained
            : HouseholdLocalResponseOutcomeCodes.Contained;
        string summary = outcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"{household.HouseholdName}遣少丁递信，路情稍明，却把薄丁力又抽去一截。"
            : $"{household.HouseholdName}遣少丁递信，先把路情和脚户说法递清，迁徙之念调到{household.MigrationRisk}。";

        ApplyLocalResponseReceipt(
            household,
            command,
            outcomeCode,
            HouseholdLocalResponseTraceCodes.HouseholdRoadMessageSent,
            summary);
        return BuildAcceptedResult(command, household);
    }

    private static PopulationHouseholdState? ResolveTargetHousehold(
        PopulationAndHouseholdsState state,
        PlayerCommandRequest command)
    {
        IEnumerable<PopulationHouseholdState> localHouseholds = state.Households
            .Where(household => household.SettlementId == command.SettlementId);

        if (command.ClanId.HasValue)
        {
            localHouseholds = localHouseholds.Where(household => household.SponsorClanId == command.ClanId.Value);
        }

        return localHouseholds
            .OrderByDescending(static household => household.SponsorClanId.HasValue)
            .ThenByDescending(ComputeHouseholdExposureScore)
            .ThenBy(static household => household.Id.Value)
            .FirstOrDefault();
    }

    private static int ComputeHouseholdExposureScore(PopulationHouseholdState household)
    {
        return Math.Clamp(
            (household.Distress + household.DebtPressure + household.MigrationRisk + Math.Max(0, 100 - household.LaborCapacity)) / 4,
            0,
            100);
    }

    private static void ApplyLocalResponseReceipt(
        PopulationHouseholdState household,
        PlayerCommandRequest command,
        string outcomeCode,
        string traceCode,
        string summary)
    {
        household.LastLocalResponseCommandCode = command.CommandName;
        household.LastLocalResponseCommandLabel = DetermineLocalResponseCommandLabel(command.CommandName);
        household.LastLocalResponseOutcomeCode = outcomeCode;
        household.LastLocalResponseTraceCode = traceCode;
        household.LastLocalResponseSummary = summary;
        household.LocalResponseCarryoverMonths = 1;
    }

    private static PlayerCommandResult BuildAcceptedResult(PlayerCommandRequest command, PopulationHouseholdState household)
    {
        return new PlayerCommandResult
        {
            Accepted = true,
            ModuleKey = KnownModuleKeys.PopulationAndHouseholds,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId ?? household.SponsorClanId,
            CommandName = command.CommandName,
            Label = DetermineLocalResponseCommandLabel(command.CommandName),
            Summary = household.LastLocalResponseSummary,
            TargetLabel = household.HouseholdName,
        };
    }

    private static PlayerCommandResult BuildRejectedResult(PlayerCommandRequest command, string summary)
    {
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.PopulationAndHouseholds,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = DetermineLocalResponseCommandLabel(command.CommandName),
            Summary = summary,
        };
    }

    private static bool IsLocalResponseCommand(string commandName)
    {
        return string.Equals(commandName, PlayerCommandNames.RestrictNightTravel, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.PoolRunnerCompensation, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.SendHouseholdRoadMessage, StringComparison.Ordinal);
    }

    private static int Clamp100(int value)
    {
        return Math.Clamp(value, 0, 100);
    }
}
