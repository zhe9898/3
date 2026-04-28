using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildHomeHouseholdLocalResponseAffordances(
        PresentationReadModelBundle bundle)
    {
        HouseholdSocialPressureSnapshot? anchor = SelectPlayerAnchorHouseholdPressure(bundle.HouseholdSocialPressures);
        HouseholdSocialPressureSignalSnapshot? residue = TryGetPublicLifeOrderResidueSignal(anchor);
        if (anchor is null || residue is null || residue.Score <= 0)
        {
            yield break;
        }

        HouseholdPressureSnapshot? household = bundle.Households
            .FirstOrDefault(candidate => candidate.Id == anchor.HouseholdId);
        int migrationRisk = household?.MigrationRisk ?? anchor.PressureScore;
        int distress = household?.Distress ?? anchor.PressureScore;
        int debtPressure = household?.DebtPressure ?? anchor.PressureScore;
        int laborCapacity = household?.LaborCapacity ?? Math.Max(20, 100 - anchor.PressureScore);
        string householdName = anchor.HouseholdName;
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories = household is null
            ? []
            : SelectHomeHouseholdLocalResponseSocialMemories(bundle.SocialMemories, household);
        string socialMemoryHint = JoinHomeHouseholdLocalResponseText(
            BuildHomeHouseholdLocalResponseTextureHint(household),
            BuildHomeHouseholdLocalResponseMemoryHint(localSocialMemories));
        string socialMemoryReadback = BuildOrderSocialMemoryReadbackSummary(localSocialMemories);
        HouseholdLocalResponseAffordanceCapacity nightTravelCapacity =
            BuildHomeHouseholdLocalResponseAffordanceCapacity(PlayerCommandNames.RestrictNightTravel, household, residue.Score);
        HouseholdLocalResponseAffordanceCapacity compensationCapacity =
            BuildHomeHouseholdLocalResponseAffordanceCapacity(PlayerCommandNames.PoolRunnerCompensation, household, residue.Score);
        HouseholdLocalResponseAffordanceCapacity roadMessageCapacity =
            BuildHomeHouseholdLocalResponseAffordanceCapacity(PlayerCommandNames.SendHouseholdRoadMessage, household, residue.Score);
        HouseholdLocalResponseTradeoffForecast nightTravelTradeoff =
            BuildHomeHouseholdLocalResponseTradeoffForecast(PlayerCommandNames.RestrictNightTravel, household, residue.Score);
        HouseholdLocalResponseTradeoffForecast compensationTradeoff =
            BuildHomeHouseholdLocalResponseTradeoffForecast(PlayerCommandNames.PoolRunnerCompensation, household, residue.Score);
        HouseholdLocalResponseTradeoffForecast roadMessageTradeoff =
            BuildHomeHouseholdLocalResponseTradeoffForecast(PlayerCommandNames.SendHouseholdRoadMessage, household, residue.Score);
        HouseholdLocalResponseFollowUpHint nightTravelFollowUp =
            BuildHomeHouseholdLocalResponseFollowUpHint(PlayerCommandNames.RestrictNightTravel, household);
        HouseholdLocalResponseFollowUpHint compensationFollowUp =
            BuildHomeHouseholdLocalResponseFollowUpHint(PlayerCommandNames.PoolRunnerCompensation, household);
        HouseholdLocalResponseFollowUpHint roadMessageFollowUp =
            BuildHomeHouseholdLocalResponseFollowUpHint(PlayerCommandNames.SendHouseholdRoadMessage, household);
        HouseholdExternalOwnerLaneReturnGuidance nightTravelOwnerLane =
            BuildHomeHouseholdExternalOwnerLaneReturnGuidance(PlayerCommandNames.RestrictNightTravel, household);
        HouseholdExternalOwnerLaneReturnGuidance compensationOwnerLane =
            BuildHomeHouseholdExternalOwnerLaneReturnGuidance(PlayerCommandNames.PoolRunnerCompensation, household);
        HouseholdExternalOwnerLaneReturnGuidance roadMessageOwnerLane =
            BuildHomeHouseholdExternalOwnerLaneReturnGuidance(PlayerCommandNames.SendHouseholdRoadMessage, household);
        string nightTravelPersonnelFlowReadiness = BuildHomeHouseholdPersonnelFlowReadinessSummary(
            PlayerCommandNames.RestrictNightTravel,
            household,
            residue.Score);
        string compensationPersonnelFlowReadiness = BuildHomeHouseholdPersonnelFlowReadinessSummary(
            PlayerCommandNames.PoolRunnerCompensation,
            household,
            residue.Score);
        string roadMessagePersonnelFlowReadiness = BuildHomeHouseholdPersonnelFlowReadinessSummary(
            PlayerCommandNames.SendHouseholdRoadMessage,
            household,
            residue.Score);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.RestrictNightTravel,
            anchor.SettlementId,
            $"{householdName}可先暂缩夜行，少走夜路、渡口与黑路，把拒落/半落地后账先从自家脚程上压住。",
            (migrationRisk >= 28 || residue.Score >= 25) && nightTravelCapacity.IsEnabled,
            JoinHomeHouseholdLocalResponseText(
                $"本户迁徙之念{migrationRisk}，{residue.Label}{residue.Score}。",
                socialMemoryHint,
                nightTravelCapacity.AvailabilitySummary,
                nightTravelFollowUp.AvailabilitySummary),
            clanId: anchor.SponsorClanId,
            executionSummary: JoinHomeHouseholdLocalResponseText(
                "由PopulationAndHouseholds处理本户劳力、债压、民困和迁徙险；不替治安、县门或族老修复前案。",
                nightTravelOwnerLane.ExecutionSummary),
            leverageSummary: JoinHomeHouseholdLocalResponseText(
                $"本户只动自家夜行、脚程与临时避险。{residue.Summary}",
                nightTravelTradeoff.BenefitSummary,
                nightTravelTradeoff.BoundarySummary,
                nightTravelOwnerLane.BoundarySummary,
                nightTravelFollowUp.LeverageSummary),
            costSummary: JoinHomeHouseholdLocalResponseText(
                $"代价：丁力会被收紧，债压可能小涨；当前丁力{laborCapacity}，债压{debtPressure}。",
                nightTravelCapacity.CostSummary,
                nightTravelTradeoff.RecoilSummary,
                nightTravelFollowUp.CostSummary),
            readbackSummary: JoinHomeHouseholdLocalResponseText(
                $"下月看{householdName}的迁徙之念、丁力和后账是否转为已缓、暂压或吃紧。",
                nightTravelCapacity.ReadbackSummary,
                nightTravelTradeoff.ReadbackSummary,
                nightTravelTradeoff.BoundarySummary,
                nightTravelOwnerLane.ReadbackSummary,
                nightTravelFollowUp.ReadbackSummary,
                socialMemoryReadback,
                nightTravelPersonnelFlowReadiness),
            personnelFlowReadinessSummary: nightTravelPersonnelFlowReadiness,
            targetLabel: householdName);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.PoolRunnerCompensation,
            anchor.SettlementId,
            $"{householdName}可凑钱赔脚户，把误读和街口话头先压住，免得家户羞面继续外翻。",
            (distress >= 35 || residue.Score >= 20) && compensationCapacity.IsEnabled,
            JoinHomeHouseholdLocalResponseText(
                $"本户民困{distress}，债压{debtPressure}，脚路后账可见。",
                socialMemoryHint,
                compensationCapacity.AvailabilitySummary,
                compensationFollowUp.AvailabilitySummary),
            clanId: anchor.SponsorClanId,
            executionSummary: JoinHomeHouseholdLocalResponseText(
                "由PopulationAndHouseholds处理本户现钱、人情与民困变化；不替OrderAndBanditry补巡丁权威。",
                compensationOwnerLane.ExecutionSummary),
            leverageSummary: JoinHomeHouseholdLocalResponseText(
                "本户拿钱和人情先对脚户解释，只能压住自家牵连的误读。",
                compensationTradeoff.BenefitSummary,
                compensationTradeoff.BoundarySummary,
                compensationOwnerLane.BoundarySummary,
                compensationFollowUp.LeverageSummary),
            costSummary: JoinHomeHouseholdLocalResponseText(
                $"代价：债压会抬升，换取民困和迁徙险暂缓；当前债压{debtPressure}。",
                compensationCapacity.CostSummary,
                compensationTradeoff.RecoilSummary,
                compensationFollowUp.CostSummary),
            readbackSummary: JoinHomeHouseholdLocalResponseText(
                $"下月看{householdName}的民困是否下降，债压是否转成新的欠账。",
                compensationCapacity.ReadbackSummary,
                compensationTradeoff.ReadbackSummary,
                compensationTradeoff.BoundarySummary,
                compensationOwnerLane.ReadbackSummary,
                compensationFollowUp.ReadbackSummary,
                socialMemoryReadback,
                compensationPersonnelFlowReadiness),
            personnelFlowReadinessSummary: compensationPersonnelFlowReadiness,
            targetLabel: householdName);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.SendHouseholdRoadMessage,
            anchor.SettlementId,
            $"{householdName}可遣少丁递信，把路情和脚户说法先问清，再决定是否继续压。",
            laborCapacity >= 18 && residue.Score >= 18 && roadMessageCapacity.IsEnabled,
            JoinHomeHouseholdLocalResponseText(
                $"本户丁力{laborCapacity}，{residue.Label}{residue.Score}。",
                socialMemoryHint,
                roadMessageCapacity.AvailabilitySummary,
                roadMessageFollowUp.AvailabilitySummary),
            clanId: anchor.SponsorClanId,
            executionSummary: JoinHomeHouseholdLocalResponseText(
                "由PopulationAndHouseholds处理自家派丁与劳力抽动；递信不等于官署递报。",
                roadMessageOwnerLane.ExecutionSummary),
            leverageSummary: JoinHomeHouseholdLocalResponseText(
                "本户只能派自家少丁跑一趟，换一点路情清楚和街口解释。",
                roadMessageTradeoff.BenefitSummary,
                roadMessageTradeoff.BoundarySummary,
                roadMessageOwnerLane.BoundarySummary,
                roadMessageFollowUp.LeverageSummary),
            costSummary: JoinHomeHouseholdLocalResponseText(
                $"代价：丁力会下降，若本已薄弱会变成吃紧后账；当前丁力{laborCapacity}。",
                roadMessageCapacity.CostSummary,
                roadMessageTradeoff.RecoilSummary,
                roadMessageFollowUp.CostSummary),
            readbackSummary: JoinHomeHouseholdLocalResponseText(
                $"下月看{householdName}的丁力、迁徙之念和后账读回。",
                roadMessageCapacity.ReadbackSummary,
                roadMessageTradeoff.ReadbackSummary,
                roadMessageTradeoff.BoundarySummary,
                roadMessageOwnerLane.ReadbackSummary,
                roadMessageFollowUp.ReadbackSummary,
                socialMemoryReadback,
                roadMessagePersonnelFlowReadiness),
            personnelFlowReadinessSummary: roadMessagePersonnelFlowReadiness,
            targetLabel: householdName);
    }

    private static IEnumerable<PlayerCommandReceiptSnapshot> BuildHomeHouseholdLocalResponseReceipts(
        PresentationReadModelBundle bundle)
    {
        foreach (HouseholdPressureSnapshot household in bundle.Households
            .Where(static household => !string.IsNullOrWhiteSpace(household.LastLocalResponseCommandCode))
            .OrderBy(static household => household.SettlementId.Value)
            .ThenBy(static household => household.Id.Value))
        {
            IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories =
                SelectHomeHouseholdLocalResponseSocialMemories(bundle.SocialMemories, household);
            HouseholdLocalResponseAffordanceCapacity responseCapacity =
                BuildHomeHouseholdLocalResponseAffordanceCapacity(household.LastLocalResponseCommandCode, household, 0);
            HouseholdLocalResponseTradeoffForecast responseTradeoff =
                BuildHomeHouseholdLocalResponseTradeoffForecast(household.LastLocalResponseCommandCode, household, 0);
            HouseholdLocalResponseShortTermConsequenceReadback responseShortTerm =
                BuildHomeHouseholdLocalResponseShortTermConsequenceReadback(household);
            HouseholdExternalOwnerLaneReturnGuidance ownerLane =
                BuildHomeHouseholdExternalOwnerLaneReturnGuidance(household.LastLocalResponseCommandCode, household);
            string personnelFlowReadiness = BuildHomeHouseholdPersonnelFlowReadinessSummary(
                household.LastLocalResponseCommandCode,
                household,
                0);

            yield return BuildPlayerCommandReceiptSnapshot(
                household.LastLocalResponseCommandCode,
                household.SettlementId,
                household.LastLocalResponseSummary,
                RenderHomeHouseholdLocalResponseOutcome(household.LastLocalResponseOutcomeCode),
                clanId: household.SponsorClanId,
                executionSummary: BuildHomeHouseholdLocalResponseExecutionSummary(household),
                leverageSummary: JoinHomeHouseholdLocalResponseText(
                    "本户回应只结算自家劳力、债压、民困与迁徙险；不改治安、县门、宗房或社会记忆。",
                    responseTradeoff.BenefitSummary,
                    responseTradeoff.BoundarySummary,
                    ownerLane.BoundarySummary,
                    responseShortTerm.ReliefSummary,
                    responseShortTerm.ExternalAfterAccountSummary),
                costSummary: JoinHomeHouseholdLocalResponseText(
                    BuildHomeHouseholdLocalResponseCostSummary(household),
                    responseCapacity.CostSummary,
                    responseTradeoff.RecoilSummary,
                    responseShortTerm.SqueezeSummary),
                readbackSummary: JoinHomeHouseholdLocalResponseText(
                    BuildHomeHouseholdLocalResponseReadbackSummary(household, localSocialMemories),
                    responseCapacity.ReadbackSummary,
                    responseTradeoff.ReadbackSummary,
                    responseTradeoff.BoundarySummary,
                    ownerLane.ReadbackSummary,
                    responseShortTerm.ReliefSummary,
                    responseShortTerm.SqueezeSummary,
                    responseShortTerm.ExternalAfterAccountSummary,
                    personnelFlowReadiness),
                personnelFlowReadinessSummary: personnelFlowReadiness,
                targetLabel: household.HouseholdName,
                labelOverride: household.LastLocalResponseCommandLabel);
        }
    }

    private static IReadOnlyList<SocialMemoryEntrySnapshot> SelectHomeHouseholdLocalResponseSocialMemories(
        IReadOnlyList<SocialMemoryEntrySnapshot> socialMemories,
        HouseholdPressureSnapshot household)
    {
        if (socialMemories.Count == 0 || !household.SponsorClanId.HasValue)
        {
            return [];
        }

        string causePrefix = $"order.public_life.household_response.{household.Id.Value}.";
        return socialMemories
            .Where(memory =>
                memory.State == MemoryLifecycleState.Active
                && memory.SourceClanId == household.SponsorClanId
                && memory.CauseKey.StartsWith(causePrefix, StringComparison.Ordinal))
            .OrderByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .ToArray();
    }

    private static string BuildHomeHouseholdLocalResponseMemoryHint(
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        SocialMemoryEntrySnapshot? residue = localSocialMemories.FirstOrDefault();
        return residue is null
            ? string.Empty
            : $"旧账记忆：{RenderSocialMemoryTypeLabel(residue.Type)}{residue.Weight}。";
    }

    private static string BuildHomeHouseholdLocalResponseTextureHint(HouseholdPressureSnapshot? household)
    {
        if (household is null)
        {
            return string.Empty;
        }

        List<string> notes = [];
        if (household.DebtPressure >= 68)
        {
            notes.Add("债压已高，赔脚户能止口舌但会添欠账");
        }

        if (household.LaborCapacity < 35 || household.DependentCount > household.LaborerCount + 1)
        {
            notes.Add("丁力偏薄，夜禁和递信都会吃人手");
        }

        if (household.Distress >= 62)
        {
            notes.Add("民困偏重，再压容易伤脸面");
        }

        if (household.MigrationRisk >= 55)
        {
            notes.Add("已有迁徙之念，暂缩夜路更有用");
        }

        return notes.Count == 0
            ? string.Empty
            : $"本户底色：{string.Join("；", notes)}。";
    }

    private static string BuildHomeHouseholdPersonnelFlowReadinessSummary(
        string commandName,
        HouseholdPressureSnapshot? household,
        int residueScore)
    {
        if (household is null)
        {
            return string.Empty;
        }

        string commandReadback = commandName switch
        {
            PlayerCommandNames.RestrictNightTravel => "夜行/渡口读法：先压脚程与迁徙之念，不迁人。",
            PlayerCommandNames.PoolRunnerCompensation => "赔脚户读法：先用本户钱债和人情压误读，不调人。",
            PlayerCommandNames.SendHouseholdRoadMessage => "递路信读法：先问路情和脚户说法，不召人。",
            _ => string.Empty,
        };
        if (string.IsNullOrWhiteSpace(commandReadback))
        {
            return string.Empty;
        }

        string nearReadback = household.MigrationRisk >= 60 || residueScore >= 40
            ? "近处细读：本户已被压力选中，只读本户生计、丁力、民困和迁徙之念。"
            : "近处细读：当前只显示本户可承受的小动作，不把县外人逐个拉进来。";

        return JoinHomeHouseholdLocalResponseText(
            "人员流动预备读回：只影响本户生计/丁力/迁徙之念，不是直接调人、迁人、召人命令。",
            commandReadback,
            nearReadback,
            $"PopulationAndHouseholds拥有本户回应；迁徙之念{household.MigrationRisk}，丁力{household.LaborCapacity}，民困{household.Distress}。",
            "PersonRegistry只保身份/FidelityRing；不写 household、office、campaign 或 capability state。",
            "远处汇总：其他人仍留在迁徙池、劳力池和街面压力摘要里，不逐人硬算天下。",
            "UI/Unity只复制投影字段，不计算谁移动或是否调成。");
    }

    private static HouseholdLocalResponseAffordanceCapacity BuildHomeHouseholdLocalResponseAffordanceCapacity(
        string commandName,
        HouseholdPressureSnapshot? household,
        int residueScore)
    {
        if (household is null)
        {
            return HouseholdLocalResponseAffordanceCapacity.Empty;
        }

        bool debtBroken = household.DebtPressure >= 95;
        bool debtRisk = household.DebtPressure >= 88;
        bool laborBroken = household.LaborCapacity < 18
            || household.LaborerCount <= 0
            || (household.LaborCapacity < 30 && household.DependentCount > household.LaborerCount + 2);
        bool laborRisk = household.LaborCapacity < 28
            || household.DependentCount > household.LaborerCount + 1;

        return commandName switch
        {
            PlayerCommandNames.RestrictNightTravel => BuildNightTravelCapacity(household, residueScore, laborBroken, laborRisk),
            PlayerCommandNames.PoolRunnerCompensation => BuildRunnerCompensationCapacity(household, debtBroken, debtRisk),
            PlayerCommandNames.SendHouseholdRoadMessage => BuildRoadMessageCapacity(household, laborBroken, laborRisk),
            _ => HouseholdLocalResponseAffordanceCapacity.Empty,
        };
    }

    private static HouseholdLocalResponseTradeoffForecast BuildHomeHouseholdLocalResponseTradeoffForecast(
        string commandName,
        HouseholdPressureSnapshot? household,
        int residueScore)
    {
        if (household is null)
        {
            return HouseholdLocalResponseTradeoffForecast.Empty;
        }

        return commandName switch
        {
            PlayerCommandNames.RestrictNightTravel => BuildNightTravelTradeoff(household, residueScore),
            PlayerCommandNames.PoolRunnerCompensation => BuildRunnerCompensationTradeoff(household),
            PlayerCommandNames.SendHouseholdRoadMessage => BuildRoadMessageTradeoff(household),
            _ => HouseholdLocalResponseTradeoffForecast.Empty,
        };
    }

    private static HouseholdLocalResponseTradeoffForecast BuildNightTravelTradeoff(
        HouseholdPressureSnapshot household,
        int residueScore)
    {
        string benefit = household.MigrationRisk >= 70 || residueScore >= 45
            ? "取舍预判：预期收益：先压夜路、渡口与迁徙险，迁徙之念高时最有用。"
            : "取舍预判：预期收益：先把自家脚程从夜路和渡口抽回来，给后账留一点缓冲。";
        string recoil = household.LaborCapacity < 30 || household.DependentCount > household.LaborerCount + 1
            ? "反噬尾巴：会挤丁力，薄丁户容易把避险换成口粮吃紧。"
            : "反噬尾巴：丁力小耗，债压可能轻微上浮。";
        string boundary = "外部后账：不补巡丁、不催县门、不替族老解释。";
        string readback = "取舍读回：看迁徙之念是否下降，丁力和债压是否留下新尾巴。";
        return new HouseholdLocalResponseTradeoffForecast(benefit, recoil, boundary, readback);
    }

    private static HouseholdLocalResponseTradeoffForecast BuildRunnerCompensationTradeoff(HouseholdPressureSnapshot household)
    {
        string benefit = household.Distress >= 60
            ? "取舍预判：预期收益：先止脚户误读和街口口舌，民困高时能缓脸面。"
            : "取舍预判：预期收益：用现钱和人情把脚户误读先压成小账。";
        string recoil = household.DebtPressure >= 80
            ? "反噬尾巴：会抬债，债账高时容易坐成新欠账。"
            : "反噬尾巴：用钱和人情换口舌暂缓。";
        string boundary = "外部后账：不补巡丁、不催县门、不替族老解释。";
        string readback = "取舍读回：看口舌是否压住，债压是否坐成新欠账。";
        return new HouseholdLocalResponseTradeoffForecast(benefit, recoil, boundary, readback);
    }

    private static HouseholdLocalResponseTradeoffForecast BuildRoadMessageTradeoff(HouseholdPressureSnapshot household)
    {
        string benefit = household.MigrationRisk >= 55
            ? "取舍预判：预期收益：换回路情和脚户说法，避免继续按误读行事。"
            : "取舍预判：预期收益：先问清路情和脚户说法，再决定是否继续压。";
        string recoil = household.LaborCapacity < 32 || household.DependentCount > household.LaborerCount + 1
            ? "反噬尾巴：抽少丁出门，丁力薄时会压住家计。"
            : "反噬尾巴：耗丁力但债压较轻。";
        string boundary = "外部后账：这只是本户递信，不等于官署递报，也不补巡丁权威。";
        string readback = "取舍读回：看路情是否问清，丁力是否掉到吃紧线。";
        return new HouseholdLocalResponseTradeoffForecast(benefit, recoil, boundary, readback);
    }

    private static HouseholdLocalResponseShortTermConsequenceReadback BuildHomeHouseholdLocalResponseShortTermConsequenceReadback(
        HouseholdPressureSnapshot household)
    {
        return household.LastLocalResponseCommandCode switch
        {
            PlayerCommandNames.RestrictNightTravel => BuildNightTravelShortTermReadback(household),
            PlayerCommandNames.PoolRunnerCompensation => BuildRunnerCompensationShortTermReadback(household),
            PlayerCommandNames.SendHouseholdRoadMessage => BuildRoadMessageShortTermReadback(household),
            _ => HouseholdLocalResponseShortTermConsequenceReadback.Empty,
        };
    }

    private static HouseholdLocalResponseShortTermConsequenceReadback BuildNightTravelShortTermReadback(
        HouseholdPressureSnapshot household)
    {
        string relief = household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Ignored
            ? $"短期后果：缓住项：夜路没有被本户接住，迁徙之念仍是{household.MigrationRisk}。"
            : $"短期后果：缓住项：夜路、渡口和自家脚程先缓住，当前迁徙之念{household.MigrationRisk}。";
        string squeeze = household.LaborCapacity < 30
                         || household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"短期后果：挤压项：少走夜路也会挤丁力，当前丁力{household.LaborCapacity}，债压{household.DebtPressure}。"
            : $"短期后果：挤压项：丁力小耗，当前丁力{household.LaborCapacity}，债压{household.DebtPressure}。";
        string external = "短期后果：仍欠外部后账：巡丁、县门、族老解释和社会记忆仍归各自 owning module。";
        return new HouseholdLocalResponseShortTermConsequenceReadback(relief, squeeze, external);
    }

    private static HouseholdLocalResponseShortTermConsequenceReadback BuildRunnerCompensationShortTermReadback(
        HouseholdPressureSnapshot household)
    {
        string relief = household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Ignored
            ? $"短期后果：缓住项：赔脚户没有接住误读，民困仍是{household.Distress}。"
            : $"短期后果：缓住项：脚户误读和街口口舌先被压下，当前民困{household.Distress}。";
        string squeeze = household.DebtPressure >= 80
                         || household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"短期后果：挤压项：赔付坐成债尾，当前债压{household.DebtPressure}，迁徙之念{household.MigrationRisk}。"
            : $"短期后果：挤压项：用钱和人情换暂缓，当前债压{household.DebtPressure}。";
        string external = "短期后果：仍欠外部后账：赔脚户只压自家误读，不补巡丁、不催县门、不替族老解释。";
        return new HouseholdLocalResponseShortTermConsequenceReadback(relief, squeeze, external);
    }

    private static HouseholdLocalResponseShortTermConsequenceReadback BuildRoadMessageShortTermReadback(
        HouseholdPressureSnapshot household)
    {
        string relief = household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Ignored
            ? $"短期后果：缓住项：路情没有问清，迁徙之念仍是{household.MigrationRisk}。"
            : $"短期后果：缓住项：路情和脚户说法先问清，当前迁徙之念{household.MigrationRisk}。";
        string squeeze = household.LaborCapacity < 32
                         || household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Strained
            ? $"短期后果：挤压项：少丁出门压住家计，当前丁力{household.LaborCapacity}，民困{household.Distress}。"
            : $"短期后果：挤压项：耗一趟丁力换清楚说法，当前丁力{household.LaborCapacity}。";
        string external = "短期后果：仍欠外部后账：本户递信不是官署递报，巡丁权威、县门文移和社会记忆仍未由本户修复。";
        return new HouseholdLocalResponseShortTermConsequenceReadback(relief, squeeze, external);
    }

    private static HouseholdExternalOwnerLaneReturnGuidance BuildHomeHouseholdExternalOwnerLaneReturnGuidance(
        string commandName,
        HouseholdPressureSnapshot? household)
    {
        if (household is null)
        {
            return HouseholdExternalOwnerLaneReturnGuidance.Empty;
        }

        string orderLane = commandName switch
        {
            PlayerCommandNames.RestrictNightTravel =>
                "外部后账归位：该走巡丁/路匪 lane（OrderAndBanditry）：巡丁补保、路匪压制和 route pressure repair 仍归治安路面，本户不能代修。",
            PlayerCommandNames.PoolRunnerCompensation =>
                "外部后账归位：该走巡丁/路匪 lane（OrderAndBanditry）：脚户误读可先在本户压口舌，巡丁权威、路匪压力和 route pressure repair 仍归治安路面，本户不能代修。",
            PlayerCommandNames.SendHouseholdRoadMessage =>
                "外部后账归位：该走巡丁/路匪 lane（OrderAndBanditry）：路情可由本户问清，巡丁权威、路匪压力和 route pressure repair 仍归治安路面，本户不能代修。",
            _ => string.Empty,
        };
        if (string.IsNullOrWhiteSpace(orderLane))
        {
            return HouseholdExternalOwnerLaneReturnGuidance.Empty;
        }

        string officeLane = commandName == PlayerCommandNames.SendHouseholdRoadMessage
            ? "外部后账归位：该走县门/文移 lane（OfficeAndCareer）：本户递信不是官署递报，县门未落地、文移拖延和胥吏续拖仍要 Office lane。"
            : "外部后账归位：该走县门/文移 lane（OfficeAndCareer）：县门未落地、文移拖延和胥吏续拖仍要 Office lane，本户不能代修。";
        string familyLane = "外部后账归位：该走族老/担保 lane（FamilyCore）：族老解释、本户担保和宗房脸面仍要 Family lane，本户不能代修。";
        string memoryLane = "外部后账归位：SocialMemoryAndRelations 只在后续月读取 structured aftermath，沉淀 shame/fear/favor/grudge/obligation residue。";
        string boundary = "外部后账归位：该走巡丁/路匪 lane；该走县门/文移 lane；该走族老/担保 lane；本户不能代修。";

        return new HouseholdExternalOwnerLaneReturnGuidance(
            "外部后账归位：本户回应只交回 owner lane 读回，不补巡丁、不催县门、不替族老解释，本户不能代修。",
            boundary,
            JoinHomeHouseholdLocalResponseText(orderLane, officeLane, familyLane, memoryLane));
    }

    private static HouseholdLocalResponseFollowUpHint BuildHomeHouseholdLocalResponseFollowUpHint(
        string commandName,
        HouseholdPressureSnapshot? household)
    {
        if (household is null || string.IsNullOrWhiteSpace(household.LastLocalResponseCommandCode))
        {
            return HouseholdLocalResponseFollowUpHint.Empty;
        }

        bool repeatsLastCommand = string.Equals(
            commandName,
            household.LastLocalResponseCommandCode,
            StringComparison.Ordinal);
        string priorLabel = string.IsNullOrWhiteSpace(household.LastLocalResponseCommandLabel)
            ? RenderHomeHouseholdLocalResponseCommandLabel(household.LastLocalResponseCommandCode)
            : household.LastLocalResponseCommandLabel;
        string currentLabel = RenderHomeHouseholdLocalResponseCommandLabel(commandName);
        string availability = BuildHomeHouseholdLocalResponseFollowUpAvailability(
            priorLabel,
            household.LastLocalResponseOutcomeCode,
            repeatsLastCommand);
        string leverage = repeatsLastCommand
            ? $"续接边界：重复{priorLabel}仍只处理本户脚边，不修巡丁、县门、族老或社会记忆后账。"
            : $"换招提示：从{priorLabel}转到{currentLabel}只是换一种本户小动作，外部后账仍要原 owning module。";
        string cost = BuildHomeHouseholdLocalResponseFollowUpCost(household, repeatsLastCommand);
        string readback = repeatsLastCommand
            ? $"续接读回：看重复{priorLabel}是否只是把后账在本户内转压，还是能继续缓住一头。"
            : $"续接读回：看换招是否把余波转缓，还是留下新的债压、丁力或迁徙尾巴。";

        return new HouseholdLocalResponseFollowUpHint(availability, leverage, cost, readback);
    }

    private static string BuildHomeHouseholdLocalResponseFollowUpAvailability(
        string priorLabel,
        string outcomeCode,
        bool repeatsLastCommand)
    {
        if (repeatsLastCommand)
        {
            return outcomeCode switch
            {
                HouseholdLocalResponseOutcomeCodes.Relieved => $"续接提示：上次{priorLabel}已缓，重复只宜轻接，不必把本户继续往硬处压。",
                HouseholdLocalResponseOutcomeCodes.Contained => $"续接提示：上次{priorLabel}只是暂压，重复可接一点余波，但外部后账仍未清。",
                HouseholdLocalResponseOutcomeCodes.Strained => $"续接提示：上次{priorLabel}已吃紧，本月重复容易把家计压过线。",
                HouseholdLocalResponseOutcomeCodes.Ignored => $"续接提示：上次{priorLabel}未接住，重复多半仍弱，宜先换招或等外部读回。",
                _ => $"续接提示：上次{priorLabel}后账仍在，重复前先看本户承受线。",
            };
        }

        return outcomeCode switch
        {
            HouseholdLocalResponseOutcomeCodes.Relieved => $"续接提示：上次{priorLabel}已缓，换招只宜轻接余波，不宜把本户当巡丁。",
            HouseholdLocalResponseOutcomeCodes.Contained => $"续接提示：上次{priorLabel}暂压，换招是在补自家脚边，不是修县门和巡防。",
            HouseholdLocalResponseOutcomeCodes.Strained => $"续接提示：上次{priorLabel}已吃紧，换招前先看债压和丁力。",
            HouseholdLocalResponseOutcomeCodes.Ignored => $"续接提示：上次{priorLabel}放置，换招可问路或止口舌，但外部后账仍要另看。",
            _ => $"续接提示：上次{priorLabel}余波未明，换招也只算本户小动作。",
        };
    }

    private static string BuildHomeHouseholdLocalResponseFollowUpCost(
        HouseholdPressureSnapshot household,
        bool repeatsLastCommand)
    {
        string pressure = $"冷却提示：当前民困{household.Distress}，债压{household.DebtPressure}，丁力{household.LaborCapacity}，迁徙之念{household.MigrationRisk}";
        if (household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Strained)
        {
            return repeatsLastCommand
                ? $"{pressure}；本月再压容易叠成新尾巴。"
                : $"{pressure}；换招前先避开继续叠债或叠丁力。";
        }

        if (household.LastLocalResponseOutcomeCode == HouseholdLocalResponseOutcomeCodes.Relieved)
        {
            return $"{pressure}；已缓的一头不宜马上再当主力。";
        }

        return $"{pressure}；仍要防止把外部后账转成自家硬账。";
    }

    private static string RenderHomeHouseholdLocalResponseCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.RestrictNightTravel => "暂缩夜行",
            PlayerCommandNames.PoolRunnerCompensation => "凑钱赔脚户",
            PlayerCommandNames.SendHouseholdRoadMessage => "遣少丁递信",
            _ => commandName,
        };
    }

    private static HouseholdLocalResponseAffordanceCapacity BuildNightTravelCapacity(
        HouseholdPressureSnapshot household,
        int residueScore,
        bool laborBroken,
        bool laborRisk)
    {
        bool isEnabled = !laborBroken || household.MigrationRisk >= 75 || residueScore >= 55;
        string availability = isEnabled
            ? household.MigrationRisk >= 75
                ? "回应承受线：迁徙之念已急，少走夜路仍是本户能先接的一刀。"
                : laborRisk
                    ? "回应承受线：丁力偏薄，暂缉夜行可做，但会挤压口粮人手。"
                    : "回应承受线：本户尚能先从夜路和脚程上避险。"
            : "回应承受线：丁力已贴地，除非迁徙之念已急，不宜再压夜禁。";
        string cost = laborRisk
            ? "承受线代价：会吃丁力，若旧账再硬，容易转成吃紧后账。"
            : string.Empty;
        string readback = isEnabled
            ? "承受线读回：看夜路是否缓住迁徙之念，以及丁力是否被压过线。"
            : "承受线读回：此项暂不宜由本户硬接。";
        return new HouseholdLocalResponseAffordanceCapacity(isEnabled, availability, cost, readback);
    }

    private static HouseholdLocalResponseAffordanceCapacity BuildRunnerCompensationCapacity(
        HouseholdPressureSnapshot household,
        bool debtBroken,
        bool debtRisk)
    {
        bool isEnabled = !debtBroken || household.Distress >= 82;
        string availability = isEnabled
            ? debtRisk
                ? "回应承受线：债账逼线，赔脚户只能止口舌，不能久压。"
                : "回应承受线：债账尚能勉强接住一笔赔付。"
            : "回应承受线：债账已过线，赔付会把新欠账坐实；宜先改走递报或暂缉夜行。";
        string cost = debtRisk
            ? "承受线代价：赔付会抬债，若再叠旧账，多半变成吃紧。"
            : string.Empty;
        string readback = isEnabled
            ? "承受线读回：看口舌是否压住，以及债账是否坐成新尾巴。"
            : "承受线读回：本户当前不宜再用赔付接后账。";
        return new HouseholdLocalResponseAffordanceCapacity(isEnabled, availability, cost, readback);
    }

    private static HouseholdLocalResponseAffordanceCapacity BuildRoadMessageCapacity(
        HouseholdPressureSnapshot household,
        bool laborBroken,
        bool laborRisk)
    {
        bool isEnabled = !laborBroken && household.LaborCapacity >= 22;
        string availability = isEnabled
            ? laborRisk
                ? "回应承受线：丁力偏薄，递信可做，但少丁一出就会压口粮人手。"
                : "回应承受线：本户尚有少丁可跑一趟路信。"
            : "回应承受线：丁力已贴地，遣少丁递信会压断家计；宜先夜路避险或等外路读回。";
        string cost = laborRisk
            ? "承受线代价：递信会抽丁力，容易把路情换成家计吃紧。"
            : string.Empty;
        string readback = isEnabled
            ? "承受线读回：看路情是否问清，以及丁力是否掉到吃紧线。"
            : "承受线读回：此项暂不宜由本户出丁。";
        return new HouseholdLocalResponseAffordanceCapacity(isEnabled, availability, cost, readback);
    }

    private readonly record struct HouseholdLocalResponseAffordanceCapacity(
        bool IsEnabled,
        string AvailabilitySummary,
        string CostSummary,
        string ReadbackSummary)
    {
        public static HouseholdLocalResponseAffordanceCapacity Empty { get; } =
            new(true, string.Empty, string.Empty, string.Empty);
    }

    private readonly record struct HouseholdLocalResponseTradeoffForecast(
        string BenefitSummary,
        string RecoilSummary,
        string BoundarySummary,
        string ReadbackSummary)
    {
        public static HouseholdLocalResponseTradeoffForecast Empty { get; } =
            new(string.Empty, string.Empty, string.Empty, string.Empty);
    }

    private readonly record struct HouseholdLocalResponseShortTermConsequenceReadback(
        string ReliefSummary,
        string SqueezeSummary,
        string ExternalAfterAccountSummary)
    {
        public static HouseholdLocalResponseShortTermConsequenceReadback Empty { get; } =
            new(string.Empty, string.Empty, string.Empty);
    }

    private readonly record struct HouseholdExternalOwnerLaneReturnGuidance(
        string ExecutionSummary,
        string BoundarySummary,
        string ReadbackSummary)
    {
        public static HouseholdExternalOwnerLaneReturnGuidance Empty { get; } =
            new(string.Empty, string.Empty, string.Empty);
    }

    private readonly record struct HouseholdLocalResponseFollowUpHint(
        string AvailabilitySummary,
        string LeverageSummary,
        string CostSummary,
        string ReadbackSummary)
    {
        public static HouseholdLocalResponseFollowUpHint Empty { get; } =
            new(string.Empty, string.Empty, string.Empty, string.Empty);
    }

    private static string JoinHomeHouseholdLocalResponseText(params string[] parts)
    {
        return string.Join(" ", parts.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string RenderHomeHouseholdLocalResponseOutcome(string outcomeCode)
    {
        return outcomeCode switch
        {
            HouseholdLocalResponseOutcomeCodes.Relieved => "本户已缓",
            HouseholdLocalResponseOutcomeCodes.Contained => "本户暂压",
            HouseholdLocalResponseOutcomeCodes.Strained => "本户吃紧",
            HouseholdLocalResponseOutcomeCodes.Ignored => "本户放置",
            _ => outcomeCode,
        };
    }

    private static string BuildHomeHouseholdLocalResponseExecutionSummary(HouseholdPressureSnapshot household)
    {
        string trace = household.LastLocalResponseTraceCode switch
        {
            HouseholdLocalResponseTraceCodes.NightTravelRestricted => "少走夜路与渡口，先从自家脚程避险。",
            HouseholdLocalResponseTraceCodes.RunnerMisreadSettledLocally => "用现钱和人情压住脚户误读。",
            HouseholdLocalResponseTraceCodes.HouseholdRoadMessageSent => "抽少丁递信，换取路情清楚。",
            _ => household.LastLocalResponseTraceCode,
        };
        return $"{household.LastLocalResponseCommandLabel}：{trace}";
    }

    private static string BuildHomeHouseholdLocalResponseCostSummary(HouseholdPressureSnapshot household)
    {
        return $"本户余账：民困{household.Distress}，债压{household.DebtPressure}，丁力{household.LaborCapacity}，迁徙之念{household.MigrationRisk}。";
    }

    private static string BuildHomeHouseholdLocalResponseReadbackSummary(
        HouseholdPressureSnapshot household,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        string tail = household.LastLocalResponseOutcomeCode switch
        {
            HouseholdLocalResponseOutcomeCodes.Relieved => "后账已从本户脚程上缓下，但前案仍归原 owning module。",
            HouseholdLocalResponseOutcomeCodes.Contained => "后账只在本户这边暂压，外部治安/县门/宗房还要各自读回。",
            HouseholdLocalResponseOutcomeCodes.Strained => "后账被压住一头，又转成债压或丁力吃紧。",
            HouseholdLocalResponseOutcomeCodes.Ignored => "本户没有接住后账，压力仍在。",
            _ => household.LastLocalResponseOutcomeCode,
        };
        string householdReadback = $"{household.HouseholdName}：{tail}";
        string socialMemoryReadback = BuildOrderSocialMemoryReadbackSummary(localSocialMemories);
        return string.Join(" ", new[] { householdReadback, socialMemoryReadback }.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }
}
