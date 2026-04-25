using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static CommandLeverageProjection BuildOrderPublicLifeLeverageProjection(
        string commandName,
        SettlementPublicLifeSnapshot? publicLife,
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        IReadOnlyList<ClanSnapshot> localClans,
        IReadOnlyList<ClanNarrativeSnapshot> localNarratives,
        IReadOnlyList<ClanTradeSnapshot> localTrades,
        IReadOnlyList<ClanTradeRouteSnapshot> localRoutes,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        string nodeLabel = ResolvePublicLifeNodeLabel(publicLife, disorder);
        string lineage = BuildOrderLineageLeverageSummary(localClans, localNarratives);
        string office = BuildOrderOfficeLeverageSummary(jurisdiction);
        string trade = BuildOrderTradeLeverageSummary(localTrades, localRoutes);
        string pressure = BuildOrderGroundPressureSummary(publicLife, disorder);
        string readback = BuildOrderPressureReadbackSummary(disorder, jurisdiction, localSocialMemories);

        return commandName switch
        {
            PlayerCommandNames.EscortRoadReport => new(
                $"本户可先借路报脚力、熟人递信与{office}护住{nodeLabel}的来往；{trade}",
                "代价是差脚、担保与沿路人情先垫出去；若官面拖滞，护送只落在半路，仍会让本户背上催报之责。",
                $"下月读回看递报险数、路压与县衙案牍是否仍牵着这一脉脚信。{readback}"),

            PlayerCommandNames.FundLocalWatch => new(
                $"本户可动用{lineage}，再添现钱、粮饭与巡丁去补{nodeLabel}的路口、渡头和夜巡；{trade}",
                "代价是钱粮、人手和公开担保被押到地面上；巡丁、脚户与邻里会记得是谁出面养这一班人。",
                $"下月读回看护路得力、地面不靖、商路风险和县门积案是否仍围着这笔巡丁账回响。{readback}"),

            PlayerCommandNames.SuppressBanditry => new(
                $"本户可把{office}与{lineage}合在一起，请差役、乡勇或强手先压住{nodeLabel}的明面路匪。",
                "代价是反噬、胁迫与仇怨容易抬高；被压的人、被误伤的脚户，以及县门积案都会把这次强压记成后账。",
                $"下月读回看报复风险、胁迫风险、词状积压和街谈是否仍沿着上月严缉翻上来。{readback}"),

            PlayerCommandNames.NegotiateWithOutlaws => new(
                $"本户可凭{lineage}与熟人中保去{nodeLabel}议一段通路，先换路口、渡头和脚信暂安。",
                "代价是私下承诺会长出新的义务；黑路、分润和被议和者的面子会留下更难公开说清的尾巴。",
                $"下月读回看私路压力、街谈观望和官面是否把这次议路当成默许。{readback}"),

            PlayerCommandNames.TolerateDisorder => new(
                $"本户也可选择暂不再追，花的是忍耐、风险承受和对{nodeLabel}地面灰色秩序的默认空间。",
                "代价是路压、黑路和邻里不安会继续滋长；眼下省下的人情，可能变成日后更难收拾的口实。",
                $"下月读回看路压、私路压力和公共观望是否因暂缓穷追而继续上浮。{readback}"),

            _ => new(
                $"本户只能按{nodeLabel}当前压力寻找可达关系链；{pressure}",
                "代价取决于实际承接人、钱粮、人情与官面阻力。",
                readback),
        };
    }

    private static bool IsOrderPublicLifeCommand(string commandName)
    {
        return string.Equals(commandName, PlayerCommandNames.EscortRoadReport, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.NegotiateWithOutlaws, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.TolerateDisorder, StringComparison.Ordinal);
    }

    private static string ResolvePublicLifeNodeLabel(
        SettlementPublicLifeSnapshot? publicLife,
        SettlementDisorderSnapshot disorder)
    {
        if (!string.IsNullOrWhiteSpace(publicLife?.DominantVenueLabel))
        {
            return publicLife.DominantVenueLabel;
        }

        if (!string.IsNullOrWhiteSpace(publicLife?.NodeLabel))
        {
            return publicLife.NodeLabel;
        }

        return $"据点{disorder.SettlementId.Value}";
    }

    private static string BuildOrderLineageLeverageSummary(
        IReadOnlyList<ClanSnapshot> localClans,
        IReadOnlyList<ClanNarrativeSnapshot> localNarratives)
    {
        ClanSnapshot? leadClan = localClans
            .OrderByDescending(static clan => clan.Prestige)
            .ThenByDescending(static clan => clan.SupportReserve)
            .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
            .FirstOrDefault();
        if (leadClan is null)
        {
            return "本户在此地暂无可见宗族背书，只能靠现钱、熟人和地面人手";
        }

        ClanNarrativeSnapshot? narrative = localNarratives.FirstOrDefault(n => n.ClanId == leadClan.Id);
        string memoryTail = narrative is null
            ? string.Empty
            : $"，旧怨{narrative.GrudgePressure}、羞面{narrative.ShamePressure}、人情余账{narrative.FavorBalance}";

        return $"{leadClan.ClanName}的宗族体面{leadClan.Prestige}、余力{leadClan.SupportReserve}和房支承压{leadClan.BranchTension}{memoryTail}";
    }

    private static string BuildOrderOfficeLeverageSummary(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null)
        {
            return "本地官面触达未显，主要靠本地人手与地面情势";
        }

        string lead = string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
            ? jurisdiction.LeadOfficeTitle
            : jurisdiction.LeadOfficialName;
        return $"{lead}手中的官面触达：乡面杠杆{jurisdiction.JurisdictionLeverage}、吏胥牵制{jurisdiction.ClerkDependence}、积案{jurisdiction.PetitionBacklog}";
    }

    private static string BuildOrderTradeLeverageSummary(
        IReadOnlyList<ClanTradeSnapshot> localTrades,
        IReadOnlyList<ClanTradeRouteSnapshot> localRoutes)
    {
        int activeRoutes = localRoutes.Count(static route => route.IsActive);
        int peakRisk = localRoutes.Count == 0
            ? 0
            : localRoutes.Max(static route => Math.Max(route.Risk, route.SeizureRisk));
        int cashReserve = localTrades.Count == 0
            ? 0
            : localTrades.Max(static trade => trade.CashReserve);
        int debt = localTrades.Count == 0
            ? 0
            : localTrades.Max(static trade => trade.Debt);

        if (activeRoutes <= 0 && cashReserve <= 0)
        {
            return "商路与现钱只作旁证，不能凭空变成治安权柄。";
        }

        return $"本地可见商路{activeRoutes}条、路险{peakRisk}、现金余力{cashReserve}、债压{debt}，商路会读出护路或反噬。";
    }

    private static string BuildOrderGroundPressureSummary(
        SettlementPublicLifeSnapshot? publicLife,
        SettlementDisorderSnapshot disorder)
    {
        string publicPressure = publicLife is null
            ? string.Empty
            : $"街谈{publicLife.StreetTalkHeat}，公议{publicLife.PublicLegitimacy}，递报险数{publicLife.CourierRisk}。";
        return $"{publicPressure}路压{disorder.RoutePressure}，盗压{disorder.BanditThreat}，镇压之需{disorder.SuppressionDemand}，地面不靖{disorder.DisorderPressure}。";
    }

    private static string BuildOrderPressureReadbackSummary(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        string landingTail = BuildOrderLandingAftermathSummary(disorder);
        string responseTail = BuildOrderResponseAftermathSummary(disorder);
        string officeTail = jurisdiction is null
            ? "官面读回暂缺。"
            : $"官面读回看{jurisdiction.CurrentAdministrativeTask}、积案{jurisdiction.PetitionBacklog}、词牒之压{jurisdiction.PetitionPressure}。";
        string baseReadback = $"当前读回锚点：路压{disorder.RoutePressure}、镇压之需{disorder.SuppressionDemand}、护路得力{disorder.InterventionCarryoverMonths}月余波；{officeTail}";

        return string.Join(" ", new[] { landingTail, responseTail, baseReadback }.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string BuildOrderLandingAftermathSummary(SettlementDisorderSnapshot disorder)
    {
        string orderLabel = RenderOrderInterventionLabel(disorder);
        return disorder.LastInterventionOutcomeCode switch
        {
            OrderInterventionOutcomeCodes.Refused =>
                $"县门未落地：{orderLabel}被拒；{RenderOrderRefusalLabel(disorder.LastInterventionRefusalCode)}，后账仍在。",
            OrderInterventionOutcomeCodes.Partial =>
                $"地方拖延：{orderLabel}半落地；{RenderOrderPartialLabel(disorder.LastInterventionPartialCode)}，后账仍在。",
            _ => string.Empty,
        };
    }

    private static string RenderOrderInterventionLabel(SettlementDisorderSnapshot disorder)
    {
        return string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
            ? disorder.LastInterventionCommandCode
            : disorder.LastInterventionCommandLabel;
    }

    private static string RenderOrderRefusalLabel(string refusalCode)
    {
        return refusalCode switch
        {
            OrderInterventionRefusalCodes.WatchmenRefused => "巡丁不应、脚户误读，本户公开担保失手",
            OrderInterventionRefusalCodes.SuppressionRefused => "胥吏拖延、地方不肯真动，镇压不成",
            OrderInterventionRefusalCodes.MissingSettlement => "地方接点缺失，命令无处落脚",
            OrderInterventionRefusalCodes.UnknownCommand => "地方不识此令，无法承接",
            _ => "地方未接住，本户担保没有转成实令",
        };
    }

    private static string RenderOrderPartialLabel(string partialCode)
    {
        return partialCode switch
        {
            OrderInterventionPartialCodes.CountyDrag => "县门拖延，只落成半套",
            OrderInterventionPartialCodes.WatchMisread => "巡丁与脚户误读，只护住半路",
            OrderInterventionPartialCodes.SuppressionBacklash => "强压引出反噬，明面暂退、怨尾仍在",
            _ => "地方只接住半截，余波留给本户背账",
        };
    }

    private static string BuildOrderResponseAftermathSummary(SettlementDisorderSnapshot disorder)
    {
        if (string.IsNullOrWhiteSpace(disorder.LastRefusalResponseOutcomeCode)
            || string.IsNullOrWhiteSpace(disorder.LastRefusalResponseCommandCode))
        {
            return string.Empty;
        }

        string commandLabel = string.IsNullOrWhiteSpace(disorder.LastRefusalResponseCommandLabel)
            ? disorder.LastRefusalResponseCommandCode
            : disorder.LastRefusalResponseCommandLabel;
        return disorder.LastRefusalResponseOutcomeCode switch
        {
            PublicLifeOrderResponseOutcomeCodes.Repaired =>
                $"{commandLabel}后账已修复：护路担保重新接住，本户羞面与人情开始回缓。",
            PublicLifeOrderResponseOutcomeCodes.Contained =>
                $"{commandLabel}后账暂压：明面反噬被压住，但担保与欠账仍在。",
            PublicLifeOrderResponseOutcomeCodes.Escalated =>
                $"{commandLabel}后账恶化：地面反噬、恐惧与怨尾继续加重。",
            PublicLifeOrderResponseOutcomeCodes.Ignored =>
                $"{commandLabel}后账放置：前案余波仍无人接住。",
            _ => $"{commandLabel}后账{disorder.LastRefusalResponseOutcomeCode}。",
        };
    }

    private static IReadOnlyList<SocialMemoryEntrySnapshot> SelectLocalPublicLifeOrderSocialMemories(
        IReadOnlyList<SocialMemoryEntrySnapshot> socialMemories,
        IReadOnlyList<ClanSnapshot> localClans)
    {
        if (socialMemories.Count == 0 || localClans.Count == 0)
        {
            return [];
        }

        HashSet<ClanId> localClanIds = localClans.Select(static clan => clan.Id).ToHashSet();
        return socialMemories
            .Where(memory =>
                memory.State == MemoryLifecycleState.Active
                && IsPublicLifeOrderSocialMemory(memory)
                && memory.SourceClanId.HasValue
                && localClanIds.Contains(memory.SourceClanId.Value))
            .OrderByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .ToArray();
    }

    private static string BuildOrderSocialMemoryReadbackSummary(IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        SocialMemoryEntrySnapshot? residue = localSocialMemories.FirstOrDefault();
        if (residue is null)
        {
            return string.Empty;
        }

        return $"社会记忆读回：{RenderSocialMemoryTypeLabel(residue.Type)}{residue.Weight}，{residue.Summary}";
    }

    private static bool IsPublicLifeOrderSocialMemory(SocialMemoryEntrySnapshot memory)
    {
        return memory.CauseKey.StartsWith("order.public_life.escort_road_report", StringComparison.Ordinal)
            || memory.CauseKey.StartsWith("order.public_life.fund_local_watch", StringComparison.Ordinal)
            || memory.CauseKey.StartsWith("order.public_life.suppress_banditry", StringComparison.Ordinal)
            || memory.CauseKey.StartsWith("order.public_life.response", StringComparison.Ordinal)
            || memory.CauseKey.StartsWith("order.public_life.household_response", StringComparison.Ordinal)
            || memory.CauseKey.StartsWith("order.public_life.negotiate_with_outlaws", StringComparison.Ordinal)
            || memory.CauseKey.StartsWith("order.public_life.tolerate_disorder", StringComparison.Ordinal);
    }

    private static string RenderSocialMemoryTypeLabel(MemoryType type)
    {
        return type switch
        {
            MemoryType.Favor => "人情",
            MemoryType.Shame => "羞面",
            MemoryType.Fear => "恐惧",
            MemoryType.Grudge => "旧怨",
            MemoryType.Debt => "担保债",
            MemoryType.Trust => "信任",
            _ => "余痕",
        };
    }

    private readonly record struct CommandLeverageProjection(
        string LeverageSummary,
        string CostSummary,
        string ReadbackSummary);
}
