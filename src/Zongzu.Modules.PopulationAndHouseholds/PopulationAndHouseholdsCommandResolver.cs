using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Modules.PopulationAndHouseholds;

public sealed class PopulationAndHouseholdsCommandContext
{
    public PopulationAndHouseholdsState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();

    public ISocialMemoryAndRelationsQueries? SocialMemoryQueries { get; init; }
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

        HouseholdLocalResponseResidueFriction residueFriction =
            ResolveHomeHouseholdLocalResponseResidueFriction(context.SocialMemoryQueries, household);
        HouseholdLocalResponseTextureProfile textureProfile =
            ResolveHomeHouseholdLocalResponseTextureProfile(household);

        return command.CommandName switch
        {
            PlayerCommandNames.RestrictNightTravel => ApplyRestrictNightTravel(command, household, residueFriction, textureProfile),
            PlayerCommandNames.PoolRunnerCompensation => ApplyPoolRunnerCompensation(command, household, residueFriction, textureProfile),
            PlayerCommandNames.SendHouseholdRoadMessage => ApplySendHouseholdRoadMessage(command, household, residueFriction, textureProfile),
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

    private static PlayerCommandResult ApplyRestrictNightTravel(
        PlayerCommandRequest command,
        PopulationHouseholdState household,
        HouseholdLocalResponseResidueFriction residueFriction,
        HouseholdLocalResponseTextureProfile textureProfile)
    {
        int oldMigrationRisk = household.MigrationRisk;
        int migrationRelief = Math.Max(
            1,
            (household.MigrationRisk >= 65 ? 10 : 7)
            + residueFriction.ReliefSupport
            + textureProfile.MigrationReliefBias
            - Math.Min(3, residueFriction.StrainDrag / 2));
        int laborCost = Math.Max(
            1,
            (household.LaborCapacity >= 35 ? 5 : 3)
            + Math.Max(0, residueFriction.StrainDrag - residueFriction.ReliefSupport) / 2
            + textureProfile.LaborDrag);
        int debtCost = Math.Max(
            0,
            (household.DebtPressure >= 70 ? 2 : 1)
            + (residueFriction.ObligationDrag / 2)
            + (residueFriction.StrainDrag / 2)
            + (textureProfile.DebtDrag / 2));
        int distressDrift = (household.Distress >= 75 ? 1 : 0)
            + Math.Max(0, residueFriction.StrainDrag - 3) / 3
            + (textureProfile.DistressDrag / 2);

        household.MigrationRisk = Clamp100(household.MigrationRisk - migrationRelief);
        household.LaborCapacity = Clamp100(household.LaborCapacity - laborCost);
        household.DebtPressure = Clamp100(household.DebtPressure + debtCost);
        household.Distress = Clamp100(household.Distress + distressDrift);
        household.IsMigrating = household.MigrationRisk >= 80 || (household.IsMigrating && household.MigrationRisk >= 65);

        string outcomeCode = household.LaborCapacity < 25
            || household.DebtPressure >= 85
            || (textureProfile.LaborDrag >= 3 && household.LaborCapacity < 32)
            || (residueFriction.StrainDrag >= 5 && household.DebtPressure >= 78)
            ? HouseholdLocalResponseOutcomeCodes.Strained
            : oldMigrationRisk - household.MigrationRisk >= 8
                ? HouseholdLocalResponseOutcomeCodes.Relieved
                : HouseholdLocalResponseOutcomeCodes.Contained;
        string summary = outcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"{household.HouseholdName}暂缩夜行，夜路险头被压住一截，但丁力和债压吃紧。"
            : $"{household.HouseholdName}暂缩夜行，少走夜路、渡口和黑路，迁徙之念缓到{household.MigrationRisk}。";
        summary = AppendHouseholdTextureSummary(summary, textureProfile);
        summary = AppendResidueFrictionSummary(summary, residueFriction);

        ApplyLocalResponseReceipt(
            household,
            command,
            outcomeCode,
            HouseholdLocalResponseTraceCodes.NightTravelRestricted,
            summary);
        return BuildAcceptedResult(command, household);
    }

    private static PlayerCommandResult ApplyPoolRunnerCompensation(
        PlayerCommandRequest command,
        PopulationHouseholdState household,
        HouseholdLocalResponseResidueFriction residueFriction,
        HouseholdLocalResponseTextureProfile textureProfile)
    {
        int debtCost = (household.DebtPressure >= 70 ? 8 : 5)
            + residueFriction.ObligationDrag
            + residueFriction.StrainDrag
            + textureProfile.DebtDrag
            + (textureProfile.DistressDrag / 2);
        int distressRelief = Math.Max(
            1,
            (household.Distress >= 60 ? 8 : 5)
            + (residueFriction.ReliefSupport / 2)
            + Math.Min(2, textureProfile.DistressDrag)
            - Math.Min(2, residueFriction.StrainDrag / 3));
        int migrationRelief = Math.Max(
            0,
            (household.MigrationRisk >= 45 ? 4 : 2)
            + (residueFriction.ReliefSupport / 3)
            + (textureProfile.MigrationReliefBias / 2)
            - Math.Min(2, residueFriction.StrainDrag / 4));

        household.DebtPressure = Clamp100(household.DebtPressure + debtCost);
        household.Distress = Clamp100(household.Distress - distressRelief);
        household.MigrationRisk = Clamp100(household.MigrationRisk - migrationRelief);
        household.LaborCapacity = Clamp100(household.LaborCapacity - 1 - (textureProfile.LaborDrag / 2));
        household.IsMigrating = household.MigrationRisk >= 80 || (household.IsMigrating && household.MigrationRisk >= 65);

        string outcomeCode = household.DebtPressure >= 82
            || (textureProfile.DebtDrag >= 4 && household.DebtPressure >= 78)
            ? HouseholdLocalResponseOutcomeCodes.Strained
            : HouseholdLocalResponseOutcomeCodes.Contained;
        string summary = outcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"{household.HouseholdName}凑钱赔了脚户误读，街口话头暂压，债压却抬到{household.DebtPressure}。"
            : $"{household.HouseholdName}凑钱赔了脚户误读，街口误会先缓，民困降到{household.Distress}。";
        summary = AppendHouseholdTextureSummary(summary, textureProfile);
        summary = AppendResidueFrictionSummary(summary, residueFriction);

        ApplyLocalResponseReceipt(
            household,
            command,
            outcomeCode,
            HouseholdLocalResponseTraceCodes.RunnerMisreadSettledLocally,
            summary);
        return BuildAcceptedResult(command, household);
    }

    private static PlayerCommandResult ApplySendHouseholdRoadMessage(
        PlayerCommandRequest command,
        PopulationHouseholdState household,
        HouseholdLocalResponseResidueFriction residueFriction,
        HouseholdLocalResponseTextureProfile textureProfile)
    {
        bool laborThin = household.LaborCapacity < 30 || textureProfile.LaborDrag >= 3;
        int laborCost = Math.Max(
            1,
            (laborThin ? 4 : 6)
            + Math.Max(0, residueFriction.StrainDrag - residueFriction.ReliefSupport) / 2
            + textureProfile.LaborDrag);
        int distressRelief = Math.Max(
            0,
            (laborThin ? 1 : 4)
            + (residueFriction.ReliefSupport / 2)
            + Math.Min(1, textureProfile.DistressDrag)
            - Math.Min(2, residueFriction.ObligationDrag / 2));
        int migrationDelta = (laborThin ? 2 : -3)
            - (residueFriction.ReliefSupport / 2)
            - (textureProfile.MigrationReliefBias / 2)
            + (residueFriction.StrainDrag / 3)
            + (textureProfile.LaborDrag / 2);

        household.LaborCapacity = Clamp100(household.LaborCapacity - laborCost);
        household.Distress = Clamp100(household.Distress - distressRelief);
        household.MigrationRisk = Clamp100(household.MigrationRisk + migrationDelta);
        household.DebtPressure = Clamp100(household.DebtPressure + 1 + (residueFriction.ObligationDrag / 2) + (textureProfile.DebtDrag / 2));
        household.IsMigrating = household.MigrationRisk >= 80 || (household.IsMigrating && household.MigrationRisk >= 65);

        string outcomeCode = household.LaborCapacity < 25
            || (textureProfile.LaborDrag >= 4 && household.LaborCapacity < 34)
            || (residueFriction.StrainDrag >= 5 && household.LaborCapacity < 32)
            ? HouseholdLocalResponseOutcomeCodes.Strained
            : HouseholdLocalResponseOutcomeCodes.Contained;
        string summary = outcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"{household.HouseholdName}遣少丁递信，路情稍明，却把薄丁力又抽去一截。"
            : $"{household.HouseholdName}遣少丁递信，先把路情和脚户说法递清，迁徙之念调到{household.MigrationRisk}。";
        summary = AppendHouseholdTextureSummary(summary, textureProfile);
        summary = AppendResidueFrictionSummary(summary, residueFriction);

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

    private static HouseholdLocalResponseResidueFriction ResolveHomeHouseholdLocalResponseResidueFriction(
        ISocialMemoryAndRelationsQueries? socialMemoryQueries,
        PopulationHouseholdState household)
    {
        if (socialMemoryQueries is null || !household.SponsorClanId.HasValue)
        {
            return HouseholdLocalResponseResidueFriction.Neutral;
        }

        string causePrefix = $"order.public_life.household_response.{household.Id.Value}.";
        int relieved = 0;
        int contained = 0;
        int strained = 0;
        int ignored = 0;

        foreach (SocialMemoryEntrySnapshot memory in socialMemoryQueries.GetMemoriesByClan(household.SponsorClanId.Value)
                     .Where(static memory => memory.State == MemoryLifecycleState.Active)
                     .Where(memory => memory.CauseKey.StartsWith(causePrefix, StringComparison.Ordinal))
                     .OrderBy(static memory => memory.Id.Value))
        {
            if (memory.CauseKey.Contains($".{HouseholdLocalResponseOutcomeCodes.Relieved}.", StringComparison.Ordinal))
            {
                relieved += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{HouseholdLocalResponseOutcomeCodes.Contained}.", StringComparison.Ordinal))
            {
                contained += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{HouseholdLocalResponseOutcomeCodes.Strained}.", StringComparison.Ordinal))
            {
                strained += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{HouseholdLocalResponseOutcomeCodes.Ignored}.", StringComparison.Ordinal))
            {
                ignored += memory.Weight;
            }
        }

        return HouseholdLocalResponseResidueFriction.FromWeights(relieved, contained, strained, ignored);
    }

    private static HouseholdLocalResponseTextureProfile ResolveHomeHouseholdLocalResponseTextureProfile(
        PopulationHouseholdState household)
    {
        int debtDrag = household.DebtPressure >= 80
            ? 4
            : household.DebtPressure >= 68 ? 2 : 0;
        int laborDrag = household.LaborCapacity < 25
            ? 4
            : household.LaborCapacity < 35 ? 2 : 0;
        int distressDrag = household.Distress >= 75
            ? 3
            : household.Distress >= 62 ? 1 : 0;
        int migrationReliefBias = household.MigrationRisk >= 70
            ? 3
            : household.MigrationRisk >= 55 ? 1 : 0;

        if (household.DependentCount > household.LaborerCount + 1)
        {
            laborDrag = Math.Min(4, laborDrag + 1);
            distressDrag = Math.Min(3, distressDrag + 1);
        }

        if (household.Livelihood is LivelihoodType.Boatman or LivelihoodType.SeasonalMigrant or LivelihoodType.YamenRunner)
        {
            migrationReliefBias = Math.Min(4, migrationReliefBias + 1);
        }

        return new HouseholdLocalResponseTextureProfile(
            debtDrag,
            laborDrag,
            distressDrag,
            migrationReliefBias,
            BuildHouseholdTextureSummaryTail(household));
    }

    private static string AppendHouseholdTextureSummary(
        string summary,
        HouseholdLocalResponseTextureProfile textureProfile)
    {
        return string.IsNullOrWhiteSpace(textureProfile.SummaryTail)
            ? summary
            : $"{summary}{textureProfile.SummaryTail}";
    }

    private static string BuildHouseholdTextureSummaryTail(PopulationHouseholdState household)
    {
        List<string> notes = [];
        if (household.DebtPressure >= 68)
        {
            notes.Add("债压已高，赔付能止口舌但会添欠账");
        }

        if (household.LaborCapacity < 35 || household.DependentCount > household.LaborerCount + 1)
        {
            notes.Add("丁力偏薄，夜禁和递信都会吃家中人手");
        }

        if (household.Distress >= 62)
        {
            notes.Add("民困偏重，再压会先伤脸面和气口");
        }

        if (household.MigrationRisk >= 55)
        {
            notes.Add("已有迁徙之念，少走夜路能先拦一截");
        }

        return notes.Count == 0
            ? string.Empty
            : $" 本户底色：{string.Join("；", notes)}。";
    }

    private static string AppendResidueFrictionSummary(
        string summary,
        HouseholdLocalResponseResidueFriction residueFriction)
    {
        return string.IsNullOrWhiteSpace(residueFriction.SummaryTail)
            ? summary
            : $"{summary}{residueFriction.SummaryTail}";
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

    private readonly record struct HouseholdLocalResponseResidueFriction(
        int ReliefSupport,
        int ObligationDrag,
        int StrainDrag,
        string SummaryTail)
    {
        public static HouseholdLocalResponseResidueFriction Neutral { get; } = new(0, 0, 0, string.Empty);

        public static HouseholdLocalResponseResidueFriction FromWeights(
            int relieved,
            int contained,
            int strained,
            int ignored)
        {
            int reliefSupport = Math.Clamp(relieved / 12, 0, 4);
            int obligationDrag = Math.Clamp(contained / 14, 0, 3);
            int strainDrag = Math.Clamp((strained + ignored) / 10, 0, 6);
            string summaryTail = BuildSummaryTail(relieved, contained, strained, ignored);
            return new HouseholdLocalResponseResidueFriction(reliefSupport, obligationDrag, strainDrag, summaryTail);
        }

        private static string BuildSummaryTail(
            int relieved,
            int contained,
            int strained,
            int ignored)
        {
            int hardWeight = strained + ignored;
            if (hardWeight >= Math.Max(relieved, contained) && hardWeight > 0)
            {
                return $"旧账记忆仍硬，吃紧/放置余重{hardWeight}。";
            }

            if (relieved >= contained && relieved > 0)
            {
                return $"旧账记得曾被缓下，人情余重{relieved}。";
            }

            if (contained > 0)
            {
                return $"旧账暂压未清，担保余重{contained}。";
            }

            return string.Empty;
        }
    }

    private readonly record struct HouseholdLocalResponseTextureProfile(
        int DebtDrag,
        int LaborDrag,
        int DistressDrag,
        int MigrationReliefBias,
        string SummaryTail);
}
