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

        return $"外部后账归位：该走族老/担保 lane（FamilyCore）：{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}，族老解释、本户担保和宗房脸面仍回族中公开说法；本户不能代修。Family承接入口：回到本 lane 先看请族老解释、请族老出面；宗房内面仍看请族老调停。";
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

    private static FamilyLaneClosureReadback BuildFamilyLaneClosureReadback(
        HouseholdPressureSnapshot? household,
        ClanSnapshot? clan,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        string entry = BuildFamilyLaneEntryReadbackSummary(household, clan);
        string elder = BuildFamilyElderExplanationReadbackSummary(clan);
        string guarantee = BuildFamilyGuaranteeReadbackSummary(clan);
        string face = BuildFamilyHouseFaceReadbackSummary(clan);
        string closure = BuildFamilyLaneReceiptClosureSummary(clan);
        string residue = BuildFamilyLaneResidueFollowUpSummary(localSocialMemories, clan);
        string noLoop = BuildFamilyLaneNoLoopGuardSummary(household, clan, localSocialMemories);

        return new FamilyLaneClosureReadback(
            entry,
            elder,
            guarantee,
            face,
            closure,
            residue,
            noLoop);
    }

    private static string BuildFamilyLaneClosureReadbackText(FamilyLaneClosureReadback readback)
    {
        return JoinOwnerLaneReturnSurfaceText(
            readback.EntryReadbackSummary,
            readback.ElderExplanationReadbackSummary,
            readback.GuaranteeReadbackSummary,
            readback.HouseFaceReadbackSummary,
            readback.ReceiptClosureSummary,
            readback.ResidueFollowUpSummary,
            readback.NoLoopGuardSummary);
    }

    private static string BuildFamilyLaneEntryReadbackSummary(HouseholdPressureSnapshot? household, ClanSnapshot? clan)
    {
        if (household is null && clan is null)
        {
            return string.Empty;
        }

        string householdLabel = household is null
            ? "本户后账"
            : $"{household.HouseholdName}本户这头{RenderOwnerLaneReturnResponseState(household)}";
        string clanLabel = clan?.ClanName ?? "宗房";
        string sponsorTail = household?.SponsorClanId == clan?.Id
            ? "SponsorClanId 已指向此宗房，承接口在 FamilyCore。"
            : "承接口仍按 sponsor-clan / 本地宗房结构读，不改成普通家户线。";

        return $"Family承接入口：该回FamilyCore读族老解释、本户担保、宗房脸面；{householdLabel}，{clanLabel}只显示承接入口，不新增担保公式。{sponsorTail} 不是普通家户再扛。";
    }

    private static string BuildFamilyElderExplanationReadbackSummary(ClanSnapshot? clan)
    {
        if (clan is null || !HasStructuredFamilyOwnerLaneResponse(clan))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            clan.LastRefusalResponseCommandLabel,
            clan.LastRefusalResponseCommandCode);
        string elderState = clan.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "族老解释已给前案留出台阶，羞面先缓。",
            PublicLifeOrderResponseOutcomeCodes.Contained => "族老解释只把街口议论暂压住，仍看下月余味。",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "族老解释反被街口误读，余味转硬。",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "族老没有接住前案，解释入口仍空着。",
            _ => "族老解释读法未明。",
        };

        return $"族老解释读回：{commandLabel}留有FamilyCore结构化 owner trace；{elderState} 调停势{clan.MediationMomentum}。";
    }

    private static string BuildFamilyGuaranteeReadbackSummary(ClanSnapshot? clan)
    {
        if (clan is null || !HasStructuredFamilyOwnerLaneResponse(clan))
        {
            return string.Empty;
        }

        string guaranteeState = clan.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "本户担保重新站住，可先停本户加压。",
            PublicLifeOrderResponseOutcomeCodes.Contained => "本户担保只是暂压留账，轻续仍走Family lane。",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "本户担保被议论转硬，先换Family-lane办法。",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "本户担保欠账仍在，等Family承接口或换招。",
            _ => "本户担保读法未明。",
        };

        return $"本户担保读回：{guaranteeState} 宗房余力{clan.SupportReserve}，救济责{clan.CharityObligation}。";
    }

    private static string BuildFamilyHouseFaceReadbackSummary(ClanSnapshot? clan)
    {
        if (clan is null || !HasStructuredFamilyOwnerLaneResponse(clan))
        {
            return string.Empty;
        }

        string faceState = clan.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "宗房脸面已略补，旧账只作读回。",
            PublicLifeOrderResponseOutcomeCodes.Contained => "宗房脸面暂压未平，仍看族中公开说法。",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "宗房脸面被顶起，房支争力需要Family lane收束。",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "宗房脸面没有补回，别让普通家户硬扛。",
            _ => "宗房脸面读法未明。",
        };

        return $"宗房脸面读回：{faceState} 门望{clan.Prestige}，房支争力{clan.BranchTension}。";
    }

    private static string BuildFamilyLaneReceiptClosureSummary(ClanSnapshot? clan)
    {
        if (clan is null || !HasStructuredFamilyOwnerLaneResponse(clan))
        {
            return string.Empty;
        }

        string commandLabel = RenderOwnerLaneResponseCommandLabel(
            clan.LastRefusalResponseCommandLabel,
            clan.LastRefusalResponseCommandCode);
        string closure = clan.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "已收口：族老解释与担保读回已接住，先停本户加压。",
            PublicLifeOrderResponseOutcomeCodes.Contained => "仍留账：族老先压住议论，仍看Family lane下月。",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "转硬待换招：宗房脸面转硬，先换Family-lane办法。",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "未接待承口：族老未接住，仍等Family承接口。",
            _ => string.Empty,
        };

        return string.IsNullOrWhiteSpace(closure)
            ? string.Empty
            : $"Family后手收口读回：{closure} {commandLabel}只读FamilyCore结构化结果，不回压本户。";
    }

    private static string BuildFamilyLaneResidueFollowUpSummary(
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories,
        ClanSnapshot? clan)
    {
        if (clan is null || !HasStructuredFamilyOwnerLaneResponse(clan))
        {
            return string.Empty;
        }

        SocialMemoryEntrySnapshot? residue = SelectOwnerLaneSocialResidue(
            localSocialMemories,
            OwnerLaneReturnSourceFamily,
            clan.LastRefusalResponseCommandCode,
            clan.LastRefusalResponseOutcomeCode);
        if (residue is null)
        {
            return string.Empty;
        }

        string followUp = clan.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "冷却：后账渐平，保留读回，不再追本户。",
            PublicLifeOrderResponseOutcomeCodes.Contained => "轻续：暂压留账，仍走Family lane，不让本户代扛。",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "换招：转硬余味仍在，先换Family-lane办法。",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "等承口：放置发酸，等Family承接口或换招。",
            _ => "仍看Family lane后续月读回。",
        };

        return $"Family余味续接读回：{RenderSocialMemoryTypeLabel(residue.Type)}{residue.Weight}仍在；{followUp} durable residue仍由SocialMemoryAndRelations后续月沉淀。";
    }

    private static string BuildFamilyLaneNoLoopGuardSummary(
        HouseholdPressureSnapshot? household,
        ClanSnapshot? clan,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        bool hasEntry = household is not null;
        bool hasFamilyResponse = clan is not null && HasStructuredFamilyOwnerLaneResponse(clan);
        bool hasFamilyResidue = clan is not null
            && SelectOwnerLaneSocialResidue(
                localSocialMemories,
                OwnerLaneReturnSourceFamily,
                clan.LastRefusalResponseCommandCode,
                clan.LastRefusalResponseOutcomeCode) is not null;
        if (!hasEntry && !hasFamilyResponse && !hasFamilyResidue)
        {
            return string.Empty;
        }

        return "Family闭环防回压：族老解释、本户担保、宗房脸面和SponsorClanId pressure留在FamilyCore lane；不是普通家户再扛，不把Family后手回压PopulationAndHouseholds。";
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

        SocialMemoryEntrySnapshot? residue = SelectOwnerLaneSocialResidue(
            localSocialMemories,
            sourceModuleKey,
            commandCode,
            outcomeCode);
        if (residue is null)
        {
            return string.Empty;
        }

        string residueLabel = RenderOwnerLaneSocialResidueLabel(outcomeCode);
        return JoinOwnerLaneReturnSurfaceText(
            $"社会余味读回：{residueLabel}，余重{residue.Weight}；仍由 SocialMemoryAndRelations 后续沉淀，不是本户再修。",
            BuildOwnerLaneSocialResidueFollowUpGuidance(outcomeCode),
            BuildOwnerLaneAffordanceEcho(outcomeCode),
            BuildOwnerLaneNoLoopGuard(outcomeCode));
    }

    private static SocialMemoryEntrySnapshot? SelectOwnerLaneSocialResidue(
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
            return null;
        }

        return localSocialMemories
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
    }

    private static string BuildOwnerLaneFollowUpReceiptClosure(
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories,
        string sourceModuleKey,
        string commandCode,
        string outcomeCode)
    {
        if (string.IsNullOrWhiteSpace(outcomeCode))
        {
            return string.Empty;
        }

        SocialMemoryEntrySnapshot? residue = SelectOwnerLaneSocialResidue(
            localSocialMemories,
            sourceModuleKey,
            commandCode,
            outcomeCode);
        string residueTail = residue is null
            ? string.Empty
            : $"；社会余味{RenderOwnerLaneSocialResidueLabel(outcomeCode)}，余重{residue.Weight}";
        string closure = outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired => "后手收口读回：已收口：不回压本户",
            PublicLifeOrderResponseOutcomeCodes.Contained => "后手收口读回：仍留账：轻续本 lane",
            PublicLifeOrderResponseOutcomeCodes.Escalated => "后手收口读回：转硬待换招：先换 owner-lane 办法",
            PublicLifeOrderResponseOutcomeCodes.Ignored => "后手收口读回：未接待承口：仍等 owner lane 可承接处",
            _ => string.Empty,
        };

        if (string.IsNullOrWhiteSpace(closure))
        {
            return string.Empty;
        }

        return JoinOwnerLaneReturnSurfaceText(
            $"{closure}{residueTail}。",
            BuildOwnerLaneNoLoopGuard(outcomeCode));
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

    private static string BuildOwnerLaneSocialResidueFollowUpGuidance(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired =>
                "余味冷却提示：后账渐平，先停本户加压；下月只看本 owner lane 与社会记忆是否继续降温。",
            PublicLifeOrderResponseOutcomeCodes.Contained =>
                "余味续接提示：后账暂压留账，轻续仍走本 owner lane；本户这头不再代扛。",
            PublicLifeOrderResponseOutcomeCodes.Escalated =>
                "余味换招提示：后账转硬，先换 owner-lane 办法或等更有力承接入口；别回压本户。",
            PublicLifeOrderResponseOutcomeCodes.Ignored =>
                "余味换招提示：后账放置发酸，先换 owner lane 或等可承接入口；不要从本户硬补。",
            _ => string.Empty,
        };
    }

    private static string BuildOwnerLaneAffordanceEcho(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired =>
                "现有入口读法：建议冷却：先不加压；本 lane 已渐平，只保留读回。",
            PublicLifeOrderResponseOutcomeCodes.Contained =>
                "现有入口读法：可轻续：仍走本 lane；只从 owner-lane 入口续，不让本户代扛。",
            PublicLifeOrderResponseOutcomeCodes.Escalated =>
                "现有入口读法：建议换招：别回压本户；先换 owner-lane 办法或等承接口。",
            PublicLifeOrderResponseOutcomeCodes.Ignored =>
                "现有入口读法：等待承接口：本户不能硬补；先等 owner lane 可承接处或换招。",
            _ => string.Empty,
        };
    }

    private static string BuildOwnerLaneNoLoopGuard(string outcomeCode)
    {
        return outcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired =>
                "闭环防回压：后账已收束，不再回压本户；旧提示仅作读回，不重复追本户。",
            PublicLifeOrderResponseOutcomeCodes.Contained =>
                "闭环防回压：旧提示只指本 owner lane，不重复追本户。",
            PublicLifeOrderResponseOutcomeCodes.Escalated =>
                "闭环防回压：转硬后只换 owner-lane 办法，不回压本户。",
            PublicLifeOrderResponseOutcomeCodes.Ignored =>
                "闭环防回压：未接后等待承接口，不从本户硬补。",
            _ => string.Empty,
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

    private readonly record struct FamilyLaneClosureReadback(
        string EntryReadbackSummary,
        string ElderExplanationReadbackSummary,
        string GuaranteeReadbackSummary,
        string HouseFaceReadbackSummary,
        string ReceiptClosureSummary,
        string ResidueFollowUpSummary,
        string NoLoopGuardSummary);
}
