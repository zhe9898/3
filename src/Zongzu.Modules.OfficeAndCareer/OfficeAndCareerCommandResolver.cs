using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed class OfficeAndCareerCommandContext
{
    public OfficeAndCareerState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();

    public IOrderAndBanditryQueries? OrderQueries { get; init; }

    public IFamilyCoreQueries? FamilyQueries { get; init; }

    public ISocialMemoryAndRelationsQueries? SocialMemoryQueries { get; init; }
}

public static class OfficeAndCareerCommandResolver
{
    public static PlayerCommandResult IssueIntent(OfficeAndCareerCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.State);
        ArgumentNullException.ThrowIfNull(context.Command);

        PlayerCommandRequest command = context.Command;
        OfficeCareerState[] careers = context.State.People
            .Where(person => person.HasAppointment && person.SettlementId == command.SettlementId)
            .OrderByDescending(static person => person.AuthorityTier)
            .ThenByDescending(static person => person.OfficeReputation)
            .ThenBy(static person => person.PersonId.Value)
            .ToArray();

        if (careers.Length == 0)
        {
            return BuildRejectedOfficeResult(command, $"此地暂无可用官署人手：{command.SettlementId.Value}。");
        }

        OfficeCareerState leadCareer = careers[0];
        PublicLifeResponseResidueFriction responseFriction = ResolvePublicLifeResponseResidueFriction(
            context.SocialMemoryQueries,
            context.FamilyQueries,
            command);
        switch (command.CommandName)
        {
            case PlayerCommandNames.PetitionViaOfficeChannels:
                ApplyPetitionReview(careers, leadCareer);
                break;
            case PlayerCommandNames.DeployAdministrativeLeverage:
                ApplyAdministrativeLeverage(careers, leadCareer);
                break;
            case PlayerCommandNames.PostCountyNotice:
                ApplyCountyNoticePosting(careers, leadCareer);
                break;
            case PlayerCommandNames.DispatchRoadReport:
                ApplyRoadReportDispatch(careers, leadCareer);
                break;
            case PlayerCommandNames.PressCountyYamenDocument:
                ApplyCountyYamenDocumentPress(careers, leadCareer, ResolveOrderResidue(context.OrderQueries, command.SettlementId), responseFriction);
                break;
            case PlayerCommandNames.RedirectRoadReport:
                ApplyRoadReportRedirect(careers, leadCareer, ResolveOrderResidue(context.OrderQueries, command.SettlementId), responseFriction);
                break;
            default:
                return BuildRejectedOfficeResult(command, $"官署不识此令：{command.CommandName}。");
        }

        context.State.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(context.State.People);

        bool isPublicLifeCommand = IsPublicLifeOfficeCommand(command.CommandName);
        string label = isPublicLifeCommand
            ? DeterminePublicLifeOfficeCommandLabel(command.CommandName)
            : DetermineOfficeCommandLabel(command.CommandName);

        return new PlayerCommandResult
        {
            Accepted = true,
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            SurfaceKey = isPublicLifeCommand ? PlayerCommandSurfaceKeys.PublicLife : PlayerCommandSurfaceKeys.Office,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = label,
            Summary = $"{leadCareer.DisplayName}已奉行{label}：{leadCareer.LastPetitionOutcome}",
            TargetLabel = leadCareer.DisplayName,
        };
    }

    public static string DetermineOfficeCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.PetitionViaOfficeChannels => "批覆词状",
            PlayerCommandNames.DeployAdministrativeLeverage => "发签催办",
            PlayerCommandNames.PressCountyYamenDocument => "押文催县门",
            PlayerCommandNames.RedirectRoadReport => "改走递报",
            _ => commandName,
        };
    }

    public static string DeterminePublicLifeOfficeCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.PressCountyYamenDocument => "押文催县门",
            PlayerCommandNames.RedirectRoadReport => "改走递报",
            PlayerCommandNames.PostCountyNotice => "张榜晓谕",
            PlayerCommandNames.DispatchRoadReport => "遣吏催报",
            _ => commandName,
        };
    }

    private static void ApplyPetitionReview(OfficeCareerState[] careers, OfficeCareerState leadCareer)
    {
        int backlogReduction = Math.Clamp(4 + leadCareer.AuthorityTier * 2 + (leadCareer.JurisdictionLeverage / 20), 3, 14);
        int pressureReduction = Math.Clamp(2 + leadCareer.AuthorityTier + (leadCareer.JurisdictionLeverage / 30), 2, 8);

        foreach (OfficeCareerState career in careers)
        {
            career.CurrentAdministrativeTask = "勾理词状";
            career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + 3, 0, 100);
            career.PetitionBacklog = Math.Max(0, career.PetitionBacklog - backlogReduction);
            career.PetitionPressure = Math.Max(0, career.PetitionPressure - pressureReduction);
            career.PromotionMomentum = Math.Clamp(career.PromotionMomentum + 2, 0, 100);
            career.DemotionPressure = Math.Max(0, career.DemotionPressure - 3);
            career.OfficeReputation = Math.Clamp(career.OfficeReputation + 1, 0, 100);
            career.LastOutcome = "Serving";
            career.LastPetitionOutcome = career.PetitionBacklog == 0
                ? "已清：本处词状已逐件批结。"
                : $"分轻重：已批一轮词状，尚余积案{career.PetitionBacklog}件。";
            career.LastExplanation = $"{career.DisplayName}奉令批覆词状，先理积案与乡怨牒状；积案{career.PetitionBacklog}，词牌之压{career.PetitionPressure}。";
        }
    }

    private static void ApplyAdministrativeLeverage(OfficeCareerState[] careers, OfficeCareerState leadCareer)
    {
        int leverageSpend = Math.Clamp(Math.Max(leadCareer.JurisdictionLeverage / 10, 3), 3, 10);
        int backlogReduction = Math.Clamp(3 + leadCareer.AuthorityTier + (leadCareer.JurisdictionLeverage / 25), 2, 10);
        int pressureReduction = Math.Clamp(4 + leadCareer.AuthorityTier + (leadCareer.JurisdictionLeverage / 18), 3, 12);

        foreach (OfficeCareerState career in careers)
        {
            career.CurrentAdministrativeTask = "急牒核办";
            career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + 5, 0, 100);
            career.JurisdictionLeverage = Math.Max(0, career.JurisdictionLeverage - leverageSpend);
            career.PetitionBacklog = Math.Max(0, career.PetitionBacklog - backlogReduction);
            career.PetitionPressure = Math.Max(0, career.PetitionPressure - pressureReduction);
            career.PromotionMomentum = Math.Clamp(career.PromotionMomentum + 1, 0, 100);
            career.DemotionPressure = Math.Max(0, career.DemotionPressure - 2);
            career.LastOutcome = "Serving";
            career.LastPetitionOutcome = "准行：已发签催办，先压县门急牒与拖延。";
            career.LastExplanation = $"{career.DisplayName}奉令发签催办，以官符与印信先压急牒；余力所耗{leverageSpend}，积案{career.PetitionBacklog}，词牌之压{career.PetitionPressure}。";
        }
    }

    private static void ApplyCountyNoticePosting(OfficeCareerState[] careers, OfficeCareerState leadCareer)
    {
        int leverageSpend = Math.Clamp(Math.Max(leadCareer.JurisdictionLeverage / 14, 2), 2, 6);

        foreach (OfficeCareerState career in careers)
        {
            career.CurrentAdministrativeTask = "张榜晓谕";
            career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + 2, 0, 100);
            career.JurisdictionLeverage = Math.Max(0, career.JurisdictionLeverage - leverageSpend);
            career.PetitionPressure = Math.Max(0, career.PetitionPressure - 5);
            career.PromotionMomentum = Math.Clamp(career.PromotionMomentum + 1, 0, 100);
            career.OfficeReputation = Math.Clamp(career.OfficeReputation + 1, 0, 100);
            career.LastOutcome = "Serving";
            career.LastPetitionOutcome = "准行：县门已张榜晓谕，先正众议。";
            career.LastExplanation = $"{career.DisplayName}奉令张榜晓谕，先借县门榜示压住街谈；余力所耗{leverageSpend}，词牌之压{career.PetitionPressure}。";
        }
    }

    private static void ApplyRoadReportDispatch(OfficeCareerState[] careers, OfficeCareerState leadCareer)
    {
        int leverageSpend = Math.Clamp(Math.Max(leadCareer.JurisdictionLeverage / 16, 2), 2, 5);

        foreach (OfficeCareerState career in careers)
        {
            career.CurrentAdministrativeTask = "遣吏催报";
            career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + 3, 0, 100);
            career.JurisdictionLeverage = Math.Max(0, career.JurisdictionLeverage - leverageSpend);
            career.PetitionBacklog = Math.Max(0, career.PetitionBacklog - 1);
            career.PetitionPressure = Math.Max(0, career.PetitionPressure - 2);
            career.OfficeReputation = Math.Clamp(career.OfficeReputation + 1, 0, 100);
            career.LastOutcome = "Serving";
            career.LastPetitionOutcome = "准行：已遣吏催报，先通津口路情。";
            career.LastExplanation = $"{career.DisplayName}奉令遣吏催报，先顾津口驿递与来往文移；余力所耗{leverageSpend}，积案{career.PetitionBacklog}。";
        }
    }

    private static void ApplyCountyYamenDocumentPress(
        OfficeCareerState[] careers,
        OfficeCareerState leadCareer,
        SettlementDisorderSnapshot? orderResidue,
        PublicLifeResponseResidueFriction responseFriction)
    {
        bool hasResidue = HasRefusalOrPartialResidue(orderResidue);
        string outcomeCode = PublicLifeOrderResponseOutcomeCodes.Ignored;
        string traceCode = PublicLifeOrderResponseTraceCodes.OfficeResponseIgnored;
        string summary = "押文未接上前案，县门后账仍未补落。";
        int leverageSpend = Math.Clamp(Math.Max(leadCareer.JurisdictionLeverage / 12, 2), 2, 8);
        int effectiveLeverage = leadCareer.JurisdictionLeverage + responseFriction.OfficeLeverageShift;
        int effectiveClerkDependence = leadCareer.ClerkDependence + responseFriction.ClerkDragShift;

        if (hasResidue && effectiveLeverage >= 18 && effectiveClerkDependence <= 68)
        {
            outcomeCode = PublicLifeOrderResponseOutcomeCodes.Repaired;
            traceCode = PublicLifeOrderResponseTraceCodes.OfficeYamenLanded;
            summary = "县门补收文移，前案拖延已落到簿牍上。";
        }
        else if (hasResidue)
        {
            outcomeCode = PublicLifeOrderResponseOutcomeCodes.Escalated;
            traceCode = PublicLifeOrderResponseTraceCodes.OfficeClerkDelayEscalated;
            summary = "押文反被胥吏拖成新积案，县门后账恶化。";
        }

        summary = AppendResponseFrictionSummary(summary, responseFriction);

        foreach (OfficeCareerState career in careers)
        {
            career.CurrentAdministrativeTask = "押文催县门";
            career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + (outcomeCode == PublicLifeOrderResponseOutcomeCodes.Escalated ? 8 : 4) + responseFriction.TaskLoadShift, 0, 100);
            career.JurisdictionLeverage = Math.Max(0, career.JurisdictionLeverage - leverageSpend);
            career.PetitionBacklog = outcomeCode == PublicLifeOrderResponseOutcomeCodes.Escalated
                ? Math.Clamp(career.PetitionBacklog + 3 + responseFriction.BacklogDragShift, 0, 100)
                : Math.Max(0, career.PetitionBacklog - Math.Max(1, 3 + responseFriction.OfficeLeverageShift));
            career.PetitionPressure = outcomeCode == PublicLifeOrderResponseOutcomeCodes.Escalated
                ? Math.Clamp(career.PetitionPressure + 4 + responseFriction.ClerkDragShift, 0, 100)
                : Math.Max(0, career.PetitionPressure - Math.Max(1, 5 + responseFriction.OfficeLeverageShift));
            career.ClerkDependence = outcomeCode == PublicLifeOrderResponseOutcomeCodes.Escalated
                ? Math.Clamp(career.ClerkDependence + 5 + responseFriction.ClerkDragShift, 0, 100)
                : Math.Max(0, career.ClerkDependence - Math.Max(1, 3 - responseFriction.ClerkDragShift));
            career.LastOutcome = "Serving";
            career.LastPetitionOutcome = summary;
            career.LastExplanation = $"{career.DisplayName}奉命押文催县门；积案{career.PetitionBacklog}，胥吏牵制{career.ClerkDependence}。";
            ApplyRefusalResponseReceipt(career, PlayerCommandNames.PressCountyYamenDocument, DeterminePublicLifeOfficeCommandLabel(PlayerCommandNames.PressCountyYamenDocument), summary, outcomeCode, traceCode);
        }
    }

    private static void ApplyRoadReportRedirect(
        OfficeCareerState[] careers,
        OfficeCareerState leadCareer,
        SettlementDisorderSnapshot? orderResidue,
        PublicLifeResponseResidueFriction responseFriction)
    {
        bool hasResidue = HasRefusalOrPartialResidue(orderResidue);
        string outcomeCode = hasResidue
            ? PublicLifeOrderResponseOutcomeCodes.Contained
            : PublicLifeOrderResponseOutcomeCodes.Ignored;
        string traceCode = hasResidue
            ? PublicLifeOrderResponseTraceCodes.OfficeReportRerouted
            : PublicLifeOrderResponseTraceCodes.OfficeResponseIgnored;
        string summary = hasResidue
            ? "已改走递报，县门正道未通，先把路报从旁路递入。"
            : "改走递报未接上前案，只作寻常递报。";
        int leverageSpend = Math.Clamp(Math.Max(leadCareer.JurisdictionLeverage / 18, 1), 1, 5);
        summary = AppendResponseFrictionSummary(summary, responseFriction);

        foreach (OfficeCareerState career in careers)
        {
            career.CurrentAdministrativeTask = "改走递报";
            career.AdministrativeTaskLoad = Math.Clamp(career.AdministrativeTaskLoad + 2 + responseFriction.TaskLoadShift, 0, 100);
            career.JurisdictionLeverage = Math.Max(0, career.JurisdictionLeverage - leverageSpend);
            career.PetitionBacklog = Math.Max(0, career.PetitionBacklog - Math.Max(0, 1 + responseFriction.OfficeLeverageShift));
            career.PetitionPressure = Math.Max(0, career.PetitionPressure - Math.Max(0, 2 + responseFriction.OfficeLeverageShift));
            career.ClerkDependence = Math.Clamp(career.ClerkDependence + Math.Max(0, responseFriction.ClerkDragShift - 2), 0, 100);
            career.LastOutcome = "Serving";
            career.LastPetitionOutcome = summary;
            career.LastExplanation = $"{career.DisplayName}奉命改走递报，先绕开县门拥滞；积案{career.PetitionBacklog}。";
            ApplyRefusalResponseReceipt(career, PlayerCommandNames.RedirectRoadReport, DeterminePublicLifeOfficeCommandLabel(PlayerCommandNames.RedirectRoadReport), summary, outcomeCode, traceCode);
        }
    }

    private static PublicLifeResponseResidueFriction ResolvePublicLifeResponseResidueFriction(
        ISocialMemoryAndRelationsQueries? socialQueries,
        IFamilyCoreQueries? familyQueries,
        PlayerCommandRequest command)
    {
        if (socialQueries is null)
        {
            return PublicLifeResponseResidueFriction.Neutral;
        }

        HashSet<ClanId> localClanIds = ResolveLocalClanIds(familyQueries, command);
        if (localClanIds.Count == 0)
        {
            return PublicLifeResponseResidueFriction.Neutral;
        }

        int repaired = 0;
        int contained = 0;
        int escalated = 0;
        int ignored = 0;

        foreach (SocialMemoryEntrySnapshot memory in socialQueries.GetMemories()
                     .Where(static memory => memory.State == MemoryLifecycleState.Active)
                     .Where(static memory => memory.CauseKey.StartsWith("order.public_life.response.", StringComparison.Ordinal))
                     .Where(memory => memory.SourceClanId.HasValue && localClanIds.Contains(memory.SourceClanId.Value)))
        {
            if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Repaired}.", StringComparison.Ordinal))
            {
                repaired += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Contained}.", StringComparison.Ordinal))
            {
                contained += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Escalated}.", StringComparison.Ordinal))
            {
                escalated += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Ignored}.", StringComparison.Ordinal))
            {
                ignored += memory.Weight;
            }
        }

        return PublicLifeResponseResidueFriction.FromWeights(repaired, contained, escalated, ignored);
    }

    private static HashSet<ClanId> ResolveLocalClanIds(
        IFamilyCoreQueries? familyQueries,
        PlayerCommandRequest command)
    {
        if (command.ClanId.HasValue)
        {
            return [command.ClanId.Value];
        }

        return familyQueries is null
            ? []
            : familyQueries.GetClans()
                .Where(clan => clan.HomeSettlementId == command.SettlementId)
                .Select(static clan => clan.Id)
                .ToHashSet();
    }

    private static string AppendResponseFrictionSummary(
        string summary,
        PublicLifeResponseResidueFriction responseFriction)
    {
        return string.IsNullOrWhiteSpace(responseFriction.SummaryTail)
            ? summary
            : $"{summary}{responseFriction.SummaryTail}";
    }

    private static SettlementDisorderSnapshot? ResolveOrderResidue(
        IOrderAndBanditryQueries? orderQueries,
        SettlementId settlementId)
    {
        if (orderQueries is null)
        {
            return null;
        }

        try
        {
            return orderQueries.GetRequiredSettlementDisorder(settlementId);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static bool HasRefusalOrPartialResidue(SettlementDisorderSnapshot? orderResidue)
    {
        return orderResidue is not null
            && (string.Equals(orderResidue.LastInterventionOutcomeCode, OrderInterventionOutcomeCodes.Refused, StringComparison.Ordinal)
                || string.Equals(orderResidue.LastInterventionOutcomeCode, OrderInterventionOutcomeCodes.Partial, StringComparison.Ordinal));
    }

    private static void ApplyRefusalResponseReceipt(
        OfficeCareerState career,
        string commandName,
        string commandLabel,
        string summary,
        string outcomeCode,
        string traceCode)
    {
        career.LastRefusalResponseCommandCode = commandName;
        career.LastRefusalResponseCommandLabel = commandLabel;
        career.LastRefusalResponseSummary = summary;
        career.LastRefusalResponseOutcomeCode = outcomeCode;
        career.LastRefusalResponseTraceCode = traceCode;
        career.ResponseCarryoverMonths = 1;
    }

    private static PlayerCommandResult BuildRejectedOfficeResult(PlayerCommandRequest command, string summary)
    {
        bool isPublicLife = IsPublicLifeOfficeCommand(command.CommandName);
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            SurfaceKey = isPublicLife ? PlayerCommandSurfaceKeys.PublicLife : PlayerCommandSurfaceKeys.Office,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = isPublicLife ? DeterminePublicLifeOfficeCommandLabel(command.CommandName) : DetermineOfficeCommandLabel(command.CommandName),
            Summary = summary,
        };
    }

    private static bool IsPublicLifeOfficeCommand(string commandName)
    {
        return string.Equals(commandName, PlayerCommandNames.PostCountyNotice, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.PressCountyYamenDocument, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.RedirectRoadReport, StringComparison.Ordinal);
    }

    private readonly record struct PublicLifeResponseResidueFriction(
        int RepairedWeight,
        int ContainedWeight,
        int EscalatedWeight,
        int IgnoredWeight)
    {
        private int SoftSignal => Math.Clamp((RepairedWeight / 20) + (ContainedWeight / 36), 0, 5);

        private int HardSignal => Math.Clamp((EscalatedWeight / 12) + (IgnoredWeight / 16), 0, 8);

        public int OfficeLeverageShift => Math.Clamp(SoftSignal - (HardSignal / 2), -6, 5);

        public int ClerkDragShift => Math.Clamp(HardSignal - (SoftSignal / 2), -4, 8);

        public int BacklogDragShift => Math.Max(0, HardSignal / 2);

        public int TaskLoadShift => Math.Clamp(HardSignal / 3, 0, 3);

        public string SummaryTail => SoftSignal == 0 && HardSignal == 0
            ? string.Empty
            : $" 社会记忆回读：修复余重{RepairedWeight}、暂压余重{ContainedWeight}、恶化余重{EscalatedWeight}、放置余重{IgnoredWeight}。";

        public static PublicLifeResponseResidueFriction Neutral => new(0, 0, 0, 0);

        public static PublicLifeResponseResidueFriction FromWeights(
            int repairedWeight,
            int containedWeight,
            int escalatedWeight,
            int ignoredWeight)
        {
            return new(
                Math.Clamp(repairedWeight, 0, 200),
                Math.Clamp(containedWeight, 0, 200),
                Math.Clamp(escalatedWeight, 0, 200),
                Math.Clamp(ignoredWeight, 0, 200));
        }
    }
}
