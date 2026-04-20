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

    private static PlayerCommandAffordanceSnapshot? SelectPrimaryHallFamilyLifecycleAffordance(
        IReadOnlyList<PlayerCommandAffordanceSnapshot> affordances)
    {
        return affordances
            .Where(static entry =>
                entry.IsEnabled
                && string.Equals(entry.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && IsFamilyLifecycleCommand(entry.CommandName))
            .OrderBy(entry => GetFamilyLifecycleCommandPriority(entry.CommandName))
            .ThenBy(static entry => entry.TargetLabel, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static PlayerCommandReceiptSnapshot? SelectFamilyHallDocketReceipt(
        IReadOnlyList<PlayerCommandReceiptSnapshot> receipts,
        ClanId? clanId)
    {
        if (!clanId.HasValue)
        {
            return null;
        }

        return receipts
            .Where(receipt =>
                receipt.ClanId == clanId
                && string.Equals(receipt.SurfaceKey, PlayerCommandSurfaceKeys.Family, StringComparison.Ordinal)
                && IsFamilyLifecycleCommand(receipt.CommandName))
            .OrderBy(receipt => GetFamilyLifecycleCommandPriority(receipt.CommandName))
            .ThenByDescending(static receipt => !string.IsNullOrWhiteSpace(receipt.OutcomeSummary))
            .FirstOrDefault();
    }

    private static bool IsFamilyLifecycleCommand(string commandName)
    {
        return commandName is PlayerCommandNames.SetMourningOrder
            or PlayerCommandNames.SupportNewbornCare
            or PlayerCommandNames.DesignateHeirPolicy
            or PlayerCommandNames.ArrangeMarriage;
    }

    private static int GetFamilyLifecycleCommandPriority(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.SetMourningOrder => 0,
            PlayerCommandNames.SupportNewbornCare => 1,
            PlayerCommandNames.DesignateHeirPolicy => 2,
            PlayerCommandNames.ArrangeMarriage => 3,
            _ => 9,
        };
    }

    private static int ComputeFamilyHallDocketUrgencyScore(ClanSnapshot clan)
    {
        int score =
            clan.MourningLoad * 2
            + clan.InheritancePressure
            + clan.MarriageAlliancePressure
            + clan.ReproductivePressure
            + (clan.InfantCount * 12)
            + Math.Max(0, 60 - clan.HeirSecurity);

        if (!clan.HeirPersonId.HasValue)
        {
            score += 20;
        }

        return score;
    }

    private static string BuildFamilyHallDocketOrderingSummary(ClanSnapshot clan, int urgencyScore)
    {
        if (clan.MourningLoad > 0)
        {
            return $"{clan.ClanName}因门内举哀与承祧后续并压，以 {urgencyScore} 的家门压力排入堂上前列。";
        }

        if (!clan.HeirPersonId.HasValue || clan.HeirSecurity < 40)
        {
            return $"{clan.ClanName}因承祧未定、继嗣稳度偏低，以 {urgencyScore} 的继嗣压力排入堂上前列。";
        }

        if (clan.InfantCount > 0)
        {
            return $"{clan.ClanName}因新婴照护与宗房余力同看，以 {urgencyScore} 的家内压力排入堂上前列。";
        }

        return $"{clan.ClanName}因婚议与添丁两端并压，以 {urgencyScore} 的家门压力排入堂上前列。";
    }

    private static string BuildFamilyHallDocketHeadline(
        ClanSnapshot clan,
        PlayerCommandAffordanceSnapshot? affordance,
        PlayerCommandReceiptSnapshot? receipt)
    {
        if (clan.MourningLoad > 0)
        {
            return $"{clan.ClanName}门内举哀未毕";
        }

        if (!clan.HeirPersonId.HasValue || clan.HeirSecurity < 40)
        {
            return $"{clan.ClanName}承祧未定";
        }

        if (clan.InfantCount > 0)
        {
            return $"{clan.ClanName}襁褓待护";
        }

        if (clan.MarriageAlliancePressure >= 28 || clan.MarriageAllianceValue < 48)
        {
            return $"{clan.ClanName}婚议待定";
        }

        if (!string.IsNullOrWhiteSpace(affordance?.Label))
        {
            return $"{clan.ClanName}{affordance.Label}";
        }

        if (!string.IsNullOrWhiteSpace(receipt?.Label))
        {
            return $"{clan.ClanName}{receipt.Label}";
        }

        return $"{clan.ClanName}门内后计待理";
    }

    private static string BuildFamilyHallDocketWhyNowSummary(
        ClanSnapshot clan,
        PlayerCommandAffordanceSnapshot? affordance)
    {
        string pressureSummary = clan.MourningLoad > 0
            ? $"门内丧服之重{clan.MourningLoad}，后议之压{clan.InheritancePressure}。"
            : !clan.HeirPersonId.HasValue || clan.HeirSecurity < 40
                ? $"承祧稳度{clan.HeirSecurity}，后议之压{clan.InheritancePressure}。"
                : clan.InfantCount > 0
                    ? $"门内现有襁褓{clan.InfantCount}口，宗房余力{clan.SupportReserve}。"
                    : $"婚议之压{clan.MarriageAlliancePressure}，添丁之望{clan.ReproductivePressure}。";

        return CombineGovernanceDocketText(
            pressureSummary,
            affordance?.AvailabilitySummary ?? string.Empty);
    }

    private static string DetermineFamilyHallDocketPhaseLabel(
        PlayerCommandReceiptSnapshot? receipt,
        PlayerCommandAffordanceSnapshot? affordance)
    {
        if (receipt is not null)
        {
            return "已下处置";
        }

        if (affordance is not null)
        {
            return "待先理";
        }

        return "案头在看";
    }

    private static string BuildFamilyHallDocketPhaseSummary(
        ClanSnapshot clan,
        PlayerCommandAffordanceSnapshot? affordance,
        PlayerCommandReceiptSnapshot? receipt,
        string phaseLabel)
    {
        return phaseLabel switch
        {
            "已下处置" => CombineGovernanceDocketText(
                $"{clan.ClanName}前番已按{receipt?.Label ?? receipt?.CommandName}处置。".Trim(),
                receipt?.OutcomeSummary ?? string.Empty),
            "待先理" => CombineGovernanceDocketText(
                $"{clan.ClanName}眼下宜先{affordance?.Label}。".Trim(),
                affordance?.AvailabilitySummary ?? string.Empty),
            _ => $"{clan.ClanName}门内后计仍在案头。".Trim(),
        };
    }

    private static string BuildFamilyHallDocketHandlingSummary(PlayerCommandReceiptSnapshot? receipt)
    {
        if (receipt is null)
        {
            return string.Empty;
        }

        return CombineGovernanceDocketText(
            $"近已按{receipt.Label}处置。",
            receipt.Summary,
            receipt.OutcomeSummary);
    }
}
