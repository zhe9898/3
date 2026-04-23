using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OfficeAndCareer;

public sealed class OfficeAndCareerCommandContext
{
    public OfficeAndCareerState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();
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
            _ => commandName,
        };
    }

    public static string DeterminePublicLifeOfficeCommandLabel(string commandName)
    {
        return commandName switch
        {
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
            || string.Equals(commandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal);
    }
}
