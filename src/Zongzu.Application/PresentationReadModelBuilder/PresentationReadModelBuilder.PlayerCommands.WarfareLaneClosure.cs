using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static WarfareLaneClosureReadback BuildWarfareLaneClosureReadback(
        CampaignMobilizationSignalSnapshot? signal,
        CampaignFrontSnapshot? campaign,
        JurisdictionAuthoritySnapshot? jurisdiction,
        IReadOnlyList<SocialMemoryEntrySnapshot> localCampaignSocialMemories)
    {
        string entry = BuildWarfareLaneEntryReadbackSummary(signal, campaign);
        string force = BuildForceReadinessReadbackSummary(signal);
        string aftermath = BuildCampaignAftermathReadbackSummary(campaign);
        string closure = BuildWarfareLaneReceiptClosureSummary(campaign);
        string residue = BuildWarfareLaneResidueFollowUpSummary(localCampaignSocialMemories, campaign);
        string noLoop = BuildWarfareLaneNoLoopGuardSummary(signal, campaign, jurisdiction, localCampaignSocialMemories);

        return new WarfareLaneClosureReadback(
            entry,
            force,
            aftermath,
            closure,
            residue,
            noLoop);
    }

    private static string BuildWarfareLaneClosureReadbackText(WarfareLaneClosureReadback readback)
    {
        return JoinOwnerLaneReturnSurfaceText(
            readback.EntryReadbackSummary,
            readback.ForceReadinessReadbackSummary,
            readback.CampaignAftermathReadbackSummary,
            readback.ReceiptClosureSummary,
            readback.ResidueFollowUpSummary,
            readback.NoLoopGuardSummary);
    }

    private static string BuildWarfareLaneEntryReadbackSummary(
        CampaignMobilizationSignalSnapshot? signal,
        CampaignFrontSnapshot? campaign)
    {
        if (signal is null && campaign is null)
        {
            return string.Empty;
        }

        string settlementLabel = campaign?.AnchorSettlementName
            ?? signal?.SettlementName
            ?? string.Empty;
        string campaignLabel = campaign?.CampaignName ?? "军务案头";
        string windowLabel = signal is null
            ? campaign?.MobilizationWindowLabel ?? string.Empty
            : RenderMobilizationWindow(signal.MobilizationWindowLabel);
        string windowTail = string.IsNullOrWhiteSpace(windowLabel)
            ? string.Empty
            : $"动员窗{windowLabel}，";

        return $"军务承接入口：{settlementLabel}{campaignLabel}该回WarfareCampaign/ConflictAndForce读前线、粮道、收军与本地力役；{windowTail}不是普通家户硬扛，也不是把军务后账误读成县门/Order后账。";
    }

    private static string BuildForceReadinessReadbackSummary(CampaignMobilizationSignalSnapshot? signal)
    {
        if (signal is null)
        {
            return string.Empty;
        }

        return $"Force承接读回：可调之众{signal.AvailableForceCount}，战备{signal.Readiness}，指挥容量{signal.CommandCapacity}，响应{signal.ResponseActivationLevel}，秩序支援{signal.OrderSupportLevel}；本地力役、护运余力与动员疲惫归ConflictAndForce读。";
    }

    private static string BuildCampaignAftermathReadbackSummary(CampaignFrontSnapshot? campaign)
    {
        if (campaign is null)
        {
            return string.Empty;
        }

        string aftermath = string.IsNullOrWhiteSpace(campaign.LastAftermathSummary)
            ? "暂未有战后案头，但前线、粮道、士气仍需沿Campaign lane观察。"
            : campaign.LastAftermathSummary;

        return $"战后后账读回：{aftermath} 前线{campaign.FrontPressure}，粮道{campaign.SupplyStateLabel}，士气{campaign.MoraleStateLabel}，民面暴露{campaign.CivilianExposure}；收束仍回WarfareCampaign，不由普通家户或Office单独补账。";
    }

    private static string BuildWarfareLaneReceiptClosureSummary(CampaignFrontSnapshot? campaign)
    {
        if (campaign is null
            || (string.IsNullOrWhiteSpace(campaign.ActiveDirectiveCode)
                && string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace)))
        {
            return string.Empty;
        }

        string directiveLabel = string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)
            ? campaign.ActiveDirectiveCode
            : campaign.ActiveDirectiveLabel;

        return $"军务后手收口读回：{directiveLabel}只说明军务指令已落到WarfareCampaign案头；是否收住要看下月前线、粮道、收军与Force读回，不把战后后账回压普通家户。";
    }

    private static string BuildWarfareLaneResidueFollowUpSummary(
        IReadOnlyList<SocialMemoryEntrySnapshot> localCampaignSocialMemories,
        CampaignFrontSnapshot? campaign)
    {
        SocialMemoryEntrySnapshot? residue = SelectCampaignResidue(localCampaignSocialMemories);
        if (residue is null)
        {
            return string.Empty;
        }

        string campaignLabel = campaign?.CampaignName ?? "军务后账";
        string causeLabel = residue.CauseKey.StartsWith("campaign.aftermath", StringComparison.Ordinal)
            ? "战后余波"
            : "前线压力";

        return $"军务余味续接读回：{campaignLabel}{causeLabel}已沉到{RenderSocialMemoryTypeLabel(residue.Type)}{residue.Weight}；durable residue仍由SocialMemoryAndRelations后续月推进，不读回执散文、不读事件摘要字段。";
    }

    private static string BuildWarfareLaneNoLoopGuardSummary(
        CampaignMobilizationSignalSnapshot? signal,
        CampaignFrontSnapshot? campaign,
        JurisdictionAuthoritySnapshot? jurisdiction,
        IReadOnlyList<SocialMemoryEntrySnapshot> localCampaignSocialMemories)
    {
        bool hasEntry = signal is not null || campaign is not null;
        bool hasResidue = SelectCampaignResidue(localCampaignSocialMemories) is not null;
        bool hasOfficeCoordination = jurisdiction is not null
            && (jurisdiction.PetitionBacklog > 0
                || jurisdiction.AdministrativeTaskLoad > 0
                || jurisdiction.JurisdictionLeverage > 0);
        if (!hasEntry && !hasResidue && !hasOfficeCoordination)
        {
            return string.Empty;
        }

        string officeTail = jurisdiction is null
            ? string.Empty
            : $" 官面协调只读OfficeAndCareer：积案{jurisdiction.PetitionBacklog}，杠杆{jurisdiction.JurisdictionLeverage}，不替Campaign/Force计算收军。";

        return $"军务闭环防回压：前线/粮道/收军留在WarfareCampaign，力役/护运/战备留在ConflictAndForce，官面协调留在OfficeAndCareer，余味留在SocialMemoryAndRelations；不是普通家户硬扛，不是把军务后账误读成县门/Order后账。{officeTail}".Trim();
    }

    private static IReadOnlyList<SocialMemoryEntrySnapshot> SelectLocalCampaignSocialMemories(
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
                && memory.CauseKey.StartsWith("campaign.", StringComparison.Ordinal)
                && memory.SourceClanId.HasValue
                && localClanIds.Contains(memory.SourceClanId.Value))
            .OrderByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .ToArray();
    }

    private static SocialMemoryEntrySnapshot? SelectCampaignResidue(
        IReadOnlyList<SocialMemoryEntrySnapshot> localCampaignSocialMemories)
    {
        return localCampaignSocialMemories
            .Where(static memory => memory.State == MemoryLifecycleState.Active)
            .Where(static memory =>
                memory.CauseKey.StartsWith("campaign.aftermath", StringComparison.Ordinal)
                || memory.CauseKey.StartsWith("campaign.pressure", StringComparison.Ordinal))
            .OrderByDescending(static memory => memory.OriginDate.Year)
            .ThenByDescending(static memory => memory.OriginDate.Month)
            .ThenByDescending(static memory => memory.Weight)
            .ThenBy(static memory => memory.Id.Value)
            .FirstOrDefault();
    }

    private readonly record struct WarfareLaneClosureReadback(
        string EntryReadbackSummary,
        string ForceReadinessReadbackSummary,
        string CampaignAftermathReadbackSummary,
        string ReceiptClosureSummary,
        string ResidueFollowUpSummary,
        string NoLoopGuardSummary);
}
