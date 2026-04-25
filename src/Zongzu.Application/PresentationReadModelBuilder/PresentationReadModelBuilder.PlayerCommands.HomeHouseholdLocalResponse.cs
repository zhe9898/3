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

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.RestrictNightTravel,
            anchor.SettlementId,
            $"{householdName}可先暂缩夜行，少走夜路、渡口与黑路，把拒落/半落地后账先从自家脚程上压住。",
            migrationRisk >= 28 || residue.Score >= 25,
            $"本户迁徙之念{migrationRisk}，{residue.Label}{residue.Score}。",
            clanId: anchor.SponsorClanId,
            executionSummary: "由PopulationAndHouseholds处理本户劳力、债压、民困和迁徙险；不替治安、县门或族老修复前案。",
            leverageSummary: $"本户只动自家夜行、脚程与临时避险。{residue.Summary}",
            costSummary: $"代价：丁力会被收紧，债压可能小涨；当前丁力{laborCapacity}，债压{debtPressure}。",
            readbackSummary: $"下月看{householdName}的迁徙之念、丁力和后账是否转为已缓、暂压或吃紧。",
            targetLabel: householdName);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.PoolRunnerCompensation,
            anchor.SettlementId,
            $"{householdName}可凑钱赔脚户，把误读和街口话头先压住，免得家户羞面继续外翻。",
            distress >= 35 || residue.Score >= 20,
            $"本户民困{distress}，债压{debtPressure}，脚路后账可见。",
            clanId: anchor.SponsorClanId,
            executionSummary: "由PopulationAndHouseholds处理本户现钱、人情与民困变化；不替OrderAndBanditry补巡丁权威。",
            leverageSummary: "本户拿钱和人情先对脚户解释，只能压住自家牵连的误读。",
            costSummary: $"代价：债压会抬升，换取民困和迁徙险暂缓；当前债压{debtPressure}。",
            readbackSummary: $"下月看{householdName}的民困是否下降，债压是否转成新的欠账。",
            targetLabel: householdName);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.SendHouseholdRoadMessage,
            anchor.SettlementId,
            $"{householdName}可遣少丁递信，把路情和脚户说法先问清，再决定是否继续压。",
            laborCapacity >= 18 && residue.Score >= 18,
            $"本户丁力{laborCapacity}，{residue.Label}{residue.Score}。",
            clanId: anchor.SponsorClanId,
            executionSummary: "由PopulationAndHouseholds处理自家派丁与劳力抽动；递信不等于官署递报。",
            leverageSummary: "本户只能派自家少丁跑一趟，换一点路情清楚和街口解释。",
            costSummary: $"代价：丁力会下降，若本已薄弱会变成吃紧后账；当前丁力{laborCapacity}。",
            readbackSummary: $"下月看{householdName}的丁力、迁徙之念和后账读回。",
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
            yield return BuildPlayerCommandReceiptSnapshot(
                household.LastLocalResponseCommandCode,
                household.SettlementId,
                household.LastLocalResponseSummary,
                RenderHomeHouseholdLocalResponseOutcome(household.LastLocalResponseOutcomeCode),
                clanId: household.SponsorClanId,
                executionSummary: BuildHomeHouseholdLocalResponseExecutionSummary(household),
                leverageSummary: "本户回应只结算自家劳力、债压、民困与迁徙险；不改治安、县门、宗房或社会记忆。",
                costSummary: BuildHomeHouseholdLocalResponseCostSummary(household),
                readbackSummary: BuildHomeHouseholdLocalResponseReadbackSummary(household),
                targetLabel: household.HouseholdName,
                labelOverride: household.LastLocalResponseCommandLabel);
        }
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

    private static string BuildHomeHouseholdLocalResponseReadbackSummary(HouseholdPressureSnapshot household)
    {
        string tail = household.LastLocalResponseOutcomeCode switch
        {
            HouseholdLocalResponseOutcomeCodes.Relieved => "后账已从本户脚程上缓下，但前案仍归原 owning module。",
            HouseholdLocalResponseOutcomeCodes.Contained => "后账只在本户这边暂压，外部治安/县门/宗房还要各自读回。",
            HouseholdLocalResponseOutcomeCodes.Strained => "后账被压住一头，又转成债压或丁力吃紧。",
            HouseholdLocalResponseOutcomeCodes.Ignored => "本户没有接住后账，压力仍在。",
            _ => household.LastLocalResponseOutcomeCode,
        };
        return $"{household.HouseholdName}：{tail}";
    }
}
