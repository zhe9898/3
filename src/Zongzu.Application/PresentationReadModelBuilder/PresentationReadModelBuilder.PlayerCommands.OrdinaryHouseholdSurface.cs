using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static IReadOnlyList<PlayerCommandAffordanceSnapshot> AddOrdinaryHouseholdResponseChoiceSurface(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances,
        IReadOnlyList<HouseholdSocialPressureSnapshot> householdPressures)
    {
        if (affordances.Count == 0 || householdPressures.Count == 0)
        {
            return affordances;
        }

        ILookup<int, HouseholdSocialPressureSnapshot> pressuresBySettlement =
            householdPressures.ToLookup(static pressure => pressure.SettlementId.Value);
        return affordances
            .Select(affordance => EnrichOrdinaryHouseholdResponseAffordance(
                affordance,
                SelectOrdinaryHouseholdPublicLifeOrderPressure(pressuresBySettlement[affordance.SettlementId.Value])))
            .ToArray();
    }

    private static IReadOnlyList<PlayerCommandReceiptSnapshot> AddOrdinaryHouseholdResponseReceiptSurface(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        IReadOnlyList<HouseholdSocialPressureSnapshot> householdPressures)
    {
        if (receipts.Count == 0 || householdPressures.Count == 0)
        {
            return receipts;
        }

        ILookup<int, HouseholdSocialPressureSnapshot> pressuresBySettlement =
            householdPressures.ToLookup(static pressure => pressure.SettlementId.Value);
        return receipts
            .Select(receipt => EnrichOrdinaryHouseholdResponseReceipt(
                receipt,
                SelectOrdinaryHouseholdPublicLifeOrderPressure(pressuresBySettlement[receipt.SettlementId.Value])))
            .ToArray();
    }

    private static PlayerCommandAffordanceSnapshot EnrichOrdinaryHouseholdResponseAffordance(
        PlayerCommandAffordanceSnapshot affordance,
        HouseholdSocialPressureSnapshot? household)
    {
        HouseholdSocialPressureSignalSnapshot? signal = TryGetPublicLifeOrderResidueSignal(household);
        if (!IsPublicLifeOrderResponseCommand(affordance.CommandName) || household is null || signal is null || signal.Score <= 0)
        {
            return affordance;
        }

        string householdStake = BuildOrdinaryHouseholdStakeSummary(household, signal);
        string ownerSummary = BuildOrdinaryHouseholdOwnerSummary(affordance.ModuleKey);

        return affordance with
        {
            TargetLabel = AppendOrdinaryHouseholdTarget(affordance.TargetLabel, household),
            AvailabilitySummary = CombinePublicLifeResponseText(
                affordance.AvailabilitySummary,
                householdStake),
            ExecutionSummary = CombinePublicLifeResponseText(
                affordance.ExecutionSummary,
                ownerSummary),
            LeverageSummary = CombinePublicLifeResponseText(
                affordance.LeverageSummary,
                BuildOrdinaryHouseholdLeverageSummary(household, signal)),
            CostSummary = CombinePublicLifeResponseText(
                affordance.CostSummary,
                BuildOrdinaryHouseholdChoiceCostSummary(affordance.CommandName, household)),
            ReadbackSummary = CombinePublicLifeResponseText(
                affordance.ReadbackSummary,
                BuildOrdinaryHouseholdNextReadbackSummary(household)),
        };
    }

    private static PlayerCommandReceiptSnapshot EnrichOrdinaryHouseholdResponseReceipt(
        PlayerCommandReceiptSnapshot receipt,
        HouseholdSocialPressureSnapshot? household)
    {
        HouseholdSocialPressureSignalSnapshot? signal = TryGetPublicLifeOrderResidueSignal(household);
        if (!IsPublicLifeOrderResponseCommand(receipt.CommandName) || household is null || signal is null || signal.Score <= 0)
        {
            return receipt;
        }

        return receipt with
        {
            TargetLabel = AppendOrdinaryHouseholdTarget(receipt.TargetLabel, household),
            LeverageSummary = CombinePublicLifeResponseText(
                receipt.LeverageSummary,
                BuildOrdinaryHouseholdLeverageSummary(household, signal)),
            CostSummary = CombinePublicLifeResponseText(
                receipt.CostSummary,
                BuildOrdinaryHouseholdChoiceCostSummary(receipt.CommandName, household)),
            ReadbackSummary = CombinePublicLifeResponseText(
                receipt.ReadbackSummary,
                BuildOrdinaryHouseholdNextReadbackSummary(household)),
        };
    }

    private static HouseholdSocialPressureSnapshot? SelectOrdinaryHouseholdPublicLifeOrderPressure(
        IEnumerable<HouseholdSocialPressureSnapshot> pressures)
    {
        return pressures
            .Select(static pressure => new
            {
                Pressure = pressure,
                Signal = TryGetPublicLifeOrderResidueSignal(pressure),
            })
            .Where(static entry => entry.Signal is { Score: > 0 })
            .OrderByDescending(static entry => !entry.Pressure.SponsorClanId.HasValue)
            .ThenByDescending(static entry => entry.Signal!.Score)
            .ThenByDescending(static entry => entry.Pressure.PressureScore)
            .ThenBy(static entry => entry.Pressure.HouseholdName, StringComparer.Ordinal)
            .Select(static entry => entry.Pressure)
            .FirstOrDefault();
    }

    private static HouseholdSocialPressureSignalSnapshot? TryGetPublicLifeOrderResidueSignal(
        HouseholdSocialPressureSnapshot? household)
    {
        return household?.Signals.FirstOrDefault(static signal =>
            string.Equals(signal.SignalKey, HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue, StringComparison.Ordinal));
    }

    private static bool IsPublicLifeOrderResponseCommand(string commandName)
    {
        return string.Equals(commandName, PlayerCommandNames.RepairLocalWatchGuarantee, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.CompensateRunnerMisread, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.DeferHardPressure, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.PressCountyYamenDocument, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.RedirectRoadReport, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.AskClanEldersExplain, StringComparison.Ordinal);
    }

    private static string BuildOrdinaryHouseholdStakeSummary(
        HouseholdSocialPressureSnapshot household,
        HouseholdSocialPressureSignalSnapshot signal)
    {
        return $"牵连民户：{household.HouseholdName}；{signal.Label}{signal.Score}，{household.PressureBandLabel}。";
    }

    private static string BuildOrdinaryHouseholdOwnerSummary(string moduleKey)
    {
        return moduleKey switch
        {
            KnownModuleKeys.OrderAndBanditry => "这手只把普通家户后账交回治安 / 路面规则结算；家户本身不是命令所有者。",
            KnownModuleKeys.OfficeAndCareer => "这手只把普通家户后账交回县门文移规则结算；家户本身不是命令所有者。",
            KnownModuleKeys.FamilyCore => "这手只把普通家户后账交回族老解释 / 本户担保规则结算；家户本身不是命令所有者。",
            _ => "这手只显示普通家户牵连，不把投影变成新的命令所有权。",
        };
    }

    private static string BuildOrdinaryHouseholdLeverageSummary(
        HouseholdSocialPressureSnapshot household,
        HouseholdSocialPressureSignalSnapshot signal)
    {
        return $"普通家户读回：{signal.Summary}";
    }

    private static string BuildOrdinaryHouseholdChoiceCostSummary(
        string commandName,
        HouseholdSocialPressureSnapshot household)
    {
        string householdName = household.HouseholdName;
        return commandName switch
        {
            PlayerCommandNames.RepairLocalWatchGuarantee =>
                $"取舍：先把钱粮、担保与巡丁解释压到{householdName}这笔夜路后账上，别处路压可能仍留。",
            PlayerCommandNames.CompensateRunnerMisread =>
                $"取舍：先用现钱和人情替{householdName}压脚户误读，县门文移未必同时补齐。",
            PlayerCommandNames.DeferHardPressure =>
                $"取舍：先少伤{householdName}的地面恐惧与劳力，盗尾和黑路余压仍可能拖住。",
            PlayerCommandNames.PressCountyYamenDocument =>
                $"取舍：把{householdName}牵连的后账推入案牍，若胥吏续拖会换成新的县门积案。",
            PlayerCommandNames.RedirectRoadReport =>
                $"取舍：先替{householdName}把路情绕出拖滞，正路未补时仍不是全清。",
            PlayerCommandNames.AskClanEldersExplain =>
                $"取舍：族老解释能替{householdName}缓羞面，但本户担保和人情欠账会继续被看见。",
            _ =>
                $"取舍：先回应{householdName}这一户的可见后账，其他牵连仍要下月读回。",
        };
    }

    private static string BuildOrdinaryHouseholdNextReadbackSummary(HouseholdSocialPressureSnapshot household)
    {
        return $"下月看{household.HouseholdName}的巡防后账是否随回应变成已修、暂压、恶化或放置；这是投影读回，不是UI判定结果。";
    }

    private static string AppendOrdinaryHouseholdTarget(string targetLabel, HouseholdSocialPressureSnapshot household)
    {
        if (string.IsNullOrWhiteSpace(targetLabel))
        {
            return household.HouseholdName;
        }

        return targetLabel.Contains(household.HouseholdName, StringComparison.Ordinal)
            ? targetLabel
            : $"{targetLabel} / {household.HouseholdName}";
    }
}
