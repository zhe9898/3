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

        return $"外部后账归位：该走巡丁/路匪 lane（OrderAndBanditry）：{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}，巡丁、路匪、路面误读和 route pressure repair 仍回治安路面；本户不能代修。";
    }

    private static string BuildOfficeOwnerLaneReturnSurfaceGuidance(HouseholdPressureSnapshot? household)
    {
        if (household is null)
        {
            return string.Empty;
        }

        return $"外部后账归位：该走县门/文移 lane（OfficeAndCareer）：{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}，县门未落地、文移拖延和胥吏续拖仍回官署案头；本户不能代修。";
    }

    private static string BuildFamilyOwnerLaneReturnSurfaceGuidance(HouseholdPressureSnapshot? household)
    {
        if (household is null)
        {
            return string.Empty;
        }

        return $"外部后账归位：该走族老/担保 lane（FamilyCore）：{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}，族老解释、本户担保和宗房脸面仍回族中公开说法；本户不能代修。";
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
