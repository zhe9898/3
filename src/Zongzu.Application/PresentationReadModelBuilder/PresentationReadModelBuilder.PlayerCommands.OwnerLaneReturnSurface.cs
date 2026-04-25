using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private const string OwnerLaneReturnSourceOrder = "OrderAndBanditry";
    private const string OwnerLaneReturnSourceOffice = "OfficeAndCareer";
    private const string OwnerLaneReturnSourceFamily = "FamilyCore";
    private const string OwnerLaneReturnSocialResiduePrefix = "order.public_life.response.";

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
        SettlementDisorderSnapshot? disorder,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        if (household is null || disorder is null || !HasStructuredOrderOwnerLaneResponse(disorder))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            disorder.LastRefusalResponseCommandLabel,
            disorder.LastRefusalResponseCommandCode);
        string outcomeLabel = RenderPublicLifeResponseOutcome(disorder.LastRefusalResponseOutcomeCode);
        return JoinOwnerLaneReturnSurfaceText(
            $"归口状态：已归口到巡丁/路匪 lane（OrderAndBanditry）：{household.HouseholdName}本户后账已归到治安路面，{commandLabel}留有结构化 owner trace；归口不等于修好，当前 owner lane 读回：{outcomeLabel}，仍看 owner lane 下月读回。",
            BuildOwnerLaneOutcomeReading(disorder.LastRefusalResponseOutcomeCode),
            BuildOwnerLaneSocialResidueReadback(
                localSocialMemories,
                OwnerLaneReturnSourceOrder,
                disorder.LastRefusalResponseCommandCode,
                disorder.LastRefusalResponseOutcomeCode));
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
        JurisdictionAuthoritySnapshot? jurisdiction,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        if (household is null || jurisdiction is null || !HasStructuredOfficeOwnerLaneResponse(jurisdiction))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            jurisdiction.LastRefusalResponseCommandLabel,
            jurisdiction.LastRefusalResponseCommandCode);
        string outcomeLabel = RenderPublicLifeResponseOutcome(jurisdiction.LastRefusalResponseOutcomeCode);
        return JoinOwnerLaneReturnSurfaceText(
            $"归口状态：已归口到县门/文移 lane（OfficeAndCareer）：{household.HouseholdName}本户后账已归到官署案头，{commandLabel}留有结构化 owner trace；归口不等于修好，当前 owner lane 读回：{outcomeLabel}，仍看 owner lane 下月读回。",
            BuildOwnerLaneOutcomeReading(jurisdiction.LastRefusalResponseOutcomeCode),
            BuildOwnerLaneSocialResidueReadback(
                localSocialMemories,
                OwnerLaneReturnSourceOffice,
                jurisdiction.LastRefusalResponseCommandCode,
                jurisdiction.LastRefusalResponseOutcomeCode));
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
        ClanSnapshot? clan,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        if (household is null || clan is null || !HasStructuredFamilyOwnerLaneResponse(clan))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            clan.LastRefusalResponseCommandLabel,
            clan.LastRefusalResponseCommandCode);
        string outcomeLabel = RenderPublicLifeResponseOutcome(clan.LastRefusalResponseOutcomeCode);
        return JoinOwnerLaneReturnSurfaceText(
            $"归口状态：已归口到族老/担保 lane（FamilyCore）：{household.HouseholdName}本户后账已归到族中公开说法，{commandLabel}留有结构化 owner trace；归口不等于修好，当前 owner lane 读回：{outcomeLabel}，仍看 owner lane 下月读回。",
            BuildOwnerLaneOutcomeReading(clan.LastRefusalResponseOutcomeCode),
            BuildOwnerLaneSocialResidueReadback(
                localSocialMemories,
                OwnerLaneReturnSourceFamily,
                clan.LastRefusalResponseCommandCode,
                clan.LastRefusalResponseOutcomeCode));
    }

    private static string BuildOwnerLaneOutcomeReading(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired =>
                "归口后读法：已修复：先停本户加压；下月只看 owner lane 与后续记忆是否渐平。",
            PublicLifeOrderResponseOutcomeCodes.Contained =>
                "归口后读法：暂压留账：仍看本 lane 下月；本户这头不宜继续代扛。",
            PublicLifeOrderResponseOutcomeCodes.Escalated =>
                "归口后读法：恶化转硬：别让本户代扛；应回到 owner lane 继续读回。",
            PublicLifeOrderResponseOutcomeCodes.Ignored =>
                "归口后读法：放置未接：仍回 owner lane；本户不能替巡丁、县门或族老修后账。",
            _ => string.Empty,
        };
    }

    private static string BuildOwnerLaneSocialResidueReadback(
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories,
        string sourceModuleKey,
        string commandCode,
        string outcomeCode)
    {
        if (localSocialMemories.Count == 0
            || string.IsNullOrWhiteSpace(sourceModuleKey)
            || string.IsNullOrWhiteSpace(commandCode)
            || string.IsNullOrWhiteSpace(outcomeCode))
        {
            return string.Empty;
        }

        SocialMemoryEntrySnapshot? residue = localSocialMemories
            .Where(memory => memory.State == MemoryLifecycleState.Active)
            .Where(memory => TryReadOwnerLaneSocialResidueCause(memory.CauseKey, out OwnerLaneSocialResidueCause cause)
                             && string.Equals(cause.SourceModuleKey, sourceModuleKey, StringComparison.Ordinal)
                             && string.Equals(cause.CommandCode, commandCode, StringComparison.Ordinal)
                             && string.Equals(cause.OutcomeCode, outcomeCode, StringComparison.Ordinal))
            .OrderByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .FirstOrDefault();
        if (residue is null)
        {
            return string.Empty;
        }

        string residueLabel = RenderOwnerLaneSocialResidueLabel(outcomeCode);
        return $"社会余味读回：{residueLabel}，余重{residue.Weight}；仍由 SocialMemoryAndRelations 后续沉淀，不是本户再修。";
    }

    private static bool TryReadOwnerLaneSocialResidueCause(
        string causeKey,
        out OwnerLaneSocialResidueCause cause)
    {
        cause = default;
        if (!causeKey.StartsWith(OwnerLaneReturnSocialResiduePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        string[] parts = causeKey.Split('.');
        if (parts.Length < 6)
        {
            return false;
        }

        cause = new OwnerLaneSocialResidueCause(
            SourceModuleKey: parts[3],
            CommandCode: parts[4],
            OutcomeCode: parts[5]);
        return true;
    }

    private static string RenderOwnerLaneSocialResidueLabel(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "后账渐平",
            PublicLifeOrderResponseOutcomeCodes.Contained => "后账暂压留账",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "后账转硬",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "后账放置发酸",
            _ => "后账余味未明",
        };
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

    private readonly record struct OwnerLaneSocialResidueCause(
        string SourceModuleKey,
        string CommandCode,
        string OutcomeCode);
}
