using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static HouseholdPressureSnapshot? SelectRecentLocalResponseHouseholdForSettlement(
        IReadOnlyList<HouseholdPressureSnapshot> households,
        SettlementId settlementId)
    {
        return households
            .Where(household =>
                household.SettlementId == settlementId
                && HasStructuredHomeHouseholdLocalResponse(household))
            .OrderByDescending(static household => household.LocalResponseCarryoverMonths)
            .ThenBy(static household => household.Id.Value)
            .FirstOrDefault();
    }

    private static HouseholdPressureSnapshot? SelectRecentLocalResponseHouseholdForClan(
        IReadOnlyList<HouseholdPressureSnapshot> households,
        ClanSnapshot clan)
    {
        HouseholdPressureSnapshot? sponsored = households
            .Where(household =>
                household.SponsorClanId == clan.Id
                && HasStructuredHomeHouseholdLocalResponse(household))
            .OrderByDescending(static household => household.LocalResponseCarryoverMonths)
            .ThenBy(static household => household.Id.Value)
            .FirstOrDefault();
        if (sponsored is not null)
        {
            return sponsored;
        }

        return SelectRecentLocalResponseHouseholdForSettlement(households, clan.HomeSettlementId);
    }

    private static bool HasStructuredHomeHouseholdLocalResponse(HouseholdPressureSnapshot household)
    {
        return household.LastLocalResponseCommandCode switch
        {
            PlayerCommandNames.RestrictNightTravel => true,
            PlayerCommandNames.PoolRunnerCompensation => true,
            PlayerCommandNames.SendHouseholdRoadMessage => true,
            _ => false,
        };
    }

    private static string BuildOrderOwnerLaneReturnSurfaceGuidance(HouseholdPressureSnapshot? household)
    {
        if (household is null)
        {
            return string.Empty;
        }

        return $"外部后账归位：该走巡丁/路匪 lane（OrderAndBanditry）：{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}，巡丁、路匪、路面误读和 route pressure repair 仍回治安路面；本户不能代修。承接入口：回到本 lane 先看添雇巡丁、严缉路匪、催护一路；若是前案误读，再看补保巡丁或赔脚户误读。";
    }

    private static string BuildOrderOwnerLaneReturnStatusGuidance(
        HouseholdPressureSnapshot? household,
        SettlementDisorderSnapshot? disorder)
    {
        if (household is null || disorder is null || !HasStructuredOrderOwnerLaneResponse(disorder))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            disorder.LastRefusalResponseCommandLabel,
            disorder.LastRefusalResponseCommandCode);
        string outcomeLabel = RenderPublicLifeResponseOutcome(disorder.LastRefusalResponseOutcomeCode);
        return $"归口状态：已归口到巡丁/路匪 lane（OrderAndBanditry）：{household.HouseholdName}本户后账已归到治安路面，{commandLabel}留有结构化 owner trace；归口不等于修好，当前 owner lane 读回：{outcomeLabel}，仍看 owner lane 下月读回。";
    }

    private static string BuildOfficeOwnerLaneReturnSurfaceGuidance(HouseholdPressureSnapshot? household)
    {
        if (household is null)
        {
            return string.Empty;
        }

        return $"外部后账归位：该走县门/文移 lane（OfficeAndCareer）：{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}，县门未落地、文移拖延和胥吏续拖仍回官署案头；本户不能代修。承接入口：回到本 lane 先看押文催县门、改走递报；常规官署仍看批覆词状或发签催办。";
    }

    private static string BuildOfficeOwnerLaneReturnStatusGuidance(
        HouseholdPressureSnapshot? household,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (household is null || jurisdiction is null || !HasStructuredOfficeOwnerLaneResponse(jurisdiction))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            jurisdiction.LastRefusalResponseCommandLabel,
            jurisdiction.LastRefusalResponseCommandCode);
        string outcomeLabel = RenderPublicLifeResponseOutcome(jurisdiction.LastRefusalResponseOutcomeCode);
        return $"归口状态：已归口到县门/文移 lane（OfficeAndCareer）：{household.HouseholdName}本户后账已归到官署案头，{commandLabel}留有结构化 owner trace；归口不等于修好，当前 owner lane 读回：{outcomeLabel}，仍看 owner lane 下月读回。";
    }

    private static string BuildFamilyOwnerLaneReturnSurfaceGuidance(HouseholdPressureSnapshot? household)
    {
        if (household is null)
        {
            return string.Empty;
        }

        return $"外部后账归位：该走族老/担保 lane（FamilyCore）：{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}，族老解释、本户担保和宗房脸面仍回族中公开说法；本户不能代修。承接入口：回到本 lane 先看请族老解释、请族老出面；宗房内面仍看请族老调停。";
    }

    private static string BuildFamilyOwnerLaneReturnStatusGuidance(
        HouseholdPressureSnapshot? household,
        ClanSnapshot? clan)
    {
        if (household is null || clan is null || !HasStructuredFamilyOwnerLaneResponse(clan))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            clan.LastRefusalResponseCommandLabel,
            clan.LastRefusalResponseCommandCode);
        string outcomeLabel = RenderPublicLifeResponseOutcome(clan.LastRefusalResponseOutcomeCode);
        return $"归口状态：已归口到族老/担保 lane（FamilyCore）：{household.HouseholdName}本户后账已归到族中公开说法，{commandLabel}留有结构化 owner trace；归口不等于修好，当前 owner lane 读回：{outcomeLabel}，仍看 owner lane 下月读回。";
    }

    private static bool HasStructuredOrderOwnerLaneResponse(SettlementDisorderSnapshot disorder)
    {
        if (string.IsNullOrWhiteSpace(disorder.LastRefusalResponseOutcomeCode))
        {
            return false;
        }

        return disorder.LastRefusalResponseCommandCode switch
        {
            PlayerCommandNames.RepairLocalWatchGuarantee => true,
            PlayerCommandNames.CompensateRunnerMisread => true,
            PlayerCommandNames.DeferHardPressure => true,
            _ => false,
        };
    }

    private static bool HasStructuredOfficeOwnerLaneResponse(JurisdictionAuthoritySnapshot jurisdiction)
    {
        if (string.IsNullOrWhiteSpace(jurisdiction.LastRefusalResponseOutcomeCode))
        {
            return false;
        }

        return jurisdiction.LastRefusalResponseCommandCode switch
        {
            PlayerCommandNames.PressCountyYamenDocument => true,
            PlayerCommandNames.RedirectRoadReport => true,
            _ => false,
        };
    }

    private static bool HasStructuredFamilyOwnerLaneResponse(ClanSnapshot clan)
    {
        if (string.IsNullOrWhiteSpace(clan.LastRefusalResponseOutcomeCode))
        {
            return false;
        }

        return clan.LastRefusalResponseCommandCode switch
        {
            PlayerCommandNames.AskClanEldersExplain => true,
            _ => false,
        };
    }

    private static string RenderOwnerLaneResponseCommandLabel(string commandLabel, string commandCode)
    {
        return string.IsNullOrWhiteSpace(commandLabel)
            ? commandCode
            : commandLabel;
    }

    private static string RenderOwnerLaneReturnResponseState(HouseholdPressureSnapshot household)
    {
        return household.LastLocalResponseOutcomeCode switch
        {
            HouseholdLocalResponseOutcomeCodes.Relieved => "已缓",
            HouseholdLocalResponseOutcomeCodes.Contained => "暂压",
            HouseholdLocalResponseOutcomeCodes.Strained => "吃紧",
            HouseholdLocalResponseOutcomeCodes.Ignored => "放置",
            _ => "已回应",
        };
    }

    private static string JoinOwnerLaneReturnSurfaceText(params string[] parts)
    {
        return string.Join(" ", parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
    }
}
