using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed class FamilyCoreCommandContext
{
    public FamilyCoreState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();

    public GameDate CurrentDate { get; init; }

    public IPersonRegistryQueries? PersonRegistryQueries { get; init; }

    public ISocialMemoryAndRelationsQueries? SocialMemoryQueries { get; init; }
}

public static partial class FamilyCoreCommandResolver
{
    public static PlayerCommandResult IssueIntent(FamilyCoreCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.State);
        ArgumentNullException.ThrowIfNull(context.Command);

        PlayerCommandRequest command = context.Command;
        ClanStateData[] localClans = context.State.Clans
            .Where(clan => clan.HomeSettlementId == command.SettlementId)
            .OrderByDescending(static clan => clan.Prestige)
            .ThenBy(static clan => clan.Id.Value)
            .ToArray();
        ClanStateData? clan = command.ClanId is null
            ? localClans.FirstOrDefault()
            : localClans.SingleOrDefault(candidate => candidate.Id == command.ClanId.Value);

        if (clan is null)
        {
            return BuildRejectedFamilyResult(command, $"此地暂无可裁断的宗房：{command.SettlementId.Value}。");
        }

        FamilyCommandSocialModifier social = BuildSocialModifier(context, clan);

        switch (command.CommandName)
        {
            case PlayerCommandNames.ArrangeMarriage:
                return IssueArrangeMarriage(context, clan, social);
            case PlayerCommandNames.DesignateHeirPolicy:
                return IssueDesignateHeirPolicy(context, clan, social);
            case PlayerCommandNames.SupportNewbornCare:
                return IssueSupportNewbornCare(context, clan, social);
            case PlayerCommandNames.SetMourningOrder:
                return IssueSetMourningOrder(context, clan, social);
            case PlayerCommandNames.SupportSeniorBranch:
            {
                FamilyConflictResolutionProfile profile = ApplyConflictSocialModifier(
                    command.CommandName,
                    ResolveSupportSeniorBranchProfile(clan),
                    social);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "嫡支得护，旁支怨气渐起。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}偏护嫡支，祠堂中的分房与承祧之争随之更紧。";
                break;
            }
            case PlayerCommandNames.OrderFormalApology:
            {
                FamilyConflictResolutionProfile profile = ApplyConflictSocialModifier(
                    command.CommandName,
                    ResolveFormalApologyProfile(clan),
                    social);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "责成赔礼，祠堂争声暂缓。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}责令赔礼，先压祠堂口角与房支积怨。";
                break;
            }
            case PlayerCommandNames.PermitBranchSeparation:
            {
                FamilyConflictResolutionProfile profile = ApplyConflictSocialModifier(
                    command.CommandName,
                    ResolveBranchSeparationProfile(clan),
                    social);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "准其分房，旧账改作分门户账。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}准其分房，先将同火之争拆回两房自理。";
                break;
            }
            case PlayerCommandNames.SuspendClanRelief:
            {
                FamilyConflictResolutionProfile profile = ApplyConflictSocialModifier(
                    command.CommandName,
                    ResolveSuspendReliefProfile(clan),
                    social);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "停其接济，房支怨望转深。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}停其接济，祠下怨望与分房之议都更紧。";
                break;
            }
            case PlayerCommandNames.InviteClanEldersMediation:
            {
                FamilyConflictResolutionProfile profile = ApplyConflictSocialModifier(
                    command.CommandName,
                    ResolveClanEldersMediationProfile(clan, publicBroker: false),
                    social);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "请族老调停，堂议得以再开。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}请族老调停，先以长辈评断缓开祠堂争执。";
                break;
            }
            case PlayerCommandNames.InviteClanEldersPubliclyBroker:
            {
                FamilyConflictResolutionProfile profile = ApplyConflictSocialModifier(
                    command.CommandName,
                    ResolveClanEldersMediationProfile(clan, publicBroker: true),
                    social);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "族老已出面压住街谈，先把堂外口舌与门前围观缓下来。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}请族老出面，在县门与街口先行解说，免得堂内争议扩成众口公议。";
                break;
            }
            default:
                return BuildRejectedFamilyResult(command, $"宗房不识此令：{command.CommandName}。");
        }

        clan.LastConflictCommandCode = command.CommandName;
        clan.LastConflictCommandLabel = IsPublicLifeFamilyCommand(command.CommandName)
            ? DeterminePublicLifeFamilyCommandLabel(command.CommandName)
            : DetermineFamilyCommandLabel(command.CommandName);

        return new PlayerCommandResult
        {
            Accepted = true,
            ModuleKey = KnownModuleKeys.FamilyCore,
            SurfaceKey = IsPublicLifeFamilyCommand(command.CommandName)
                ? PlayerCommandSurfaceKeys.PublicLife
                : PlayerCommandSurfaceKeys.Family,
            SettlementId = command.SettlementId,
            ClanId = clan.Id,
            CommandName = command.CommandName,
            Label = clan.LastConflictCommandLabel,
            Summary = clan.LastConflictTrace,
            TargetLabel = clan.ClanName,
        };
    }

    public static string DetermineFamilyCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.ArrangeMarriage => "议亲定婚",
            PlayerCommandNames.DesignateHeirPolicy => "议定承祧",
            PlayerCommandNames.SupportNewbornCare => "拨粮护婴",
            PlayerCommandNames.SetMourningOrder => "议定丧次",
            PlayerCommandNames.SupportSeniorBranch => "偏护嫡支",
            PlayerCommandNames.OrderFormalApology => "责令赔礼",
            PlayerCommandNames.PermitBranchSeparation => "准其分房",
            PlayerCommandNames.SuspendClanRelief => "停其接济",
            PlayerCommandNames.InviteClanEldersMediation => "请族老调停",
            PlayerCommandNames.InviteClanEldersPubliclyBroker => "请族老出面",
            _ => commandName,
        };
    }

    private static PlayerCommandResult BuildAcceptedFamilyLifecycleResult(PlayerCommandRequest command, ClanStateData clan)
    {
        return new PlayerCommandResult
        {
            Accepted = true,
            ModuleKey = KnownModuleKeys.FamilyCore,
            SurfaceKey = PlayerCommandSurfaceKeys.Family,
            SettlementId = command.SettlementId,
            ClanId = clan.Id,
            CommandName = command.CommandName,
            Label = clan.LastLifecycleCommandLabel,
            Summary = $"{clan.LastLifecycleTrace} {clan.LastLifecycleOutcome}",
            TargetLabel = clan.ClanName,
        };
    }

    private static PlayerCommandResult BuildRejectedFamilyResult(PlayerCommandRequest command, string summary)
    {
        bool isPublicLife = IsPublicLifeFamilyCommand(command.CommandName);
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.FamilyCore,
            SurfaceKey = isPublicLife ? PlayerCommandSurfaceKeys.PublicLife : PlayerCommandSurfaceKeys.Family,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = isPublicLife ? DeterminePublicLifeFamilyCommandLabel(command.CommandName) : DetermineFamilyCommandLabel(command.CommandName),
            Summary = summary,
        };
    }

    private static bool IsPublicLifeFamilyCommand(string commandName)
    {
        return string.Equals(commandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal);
    }

    private static string DeterminePublicLifeFamilyCommandLabel(string commandName)
    {
        return string.Equals(commandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)
            ? "请族老出面"
            : DetermineFamilyCommandLabel(commandName);
    }

    private static bool IsFamilyCommandPersonAlive(FamilyPersonState person, IPersonRegistryQueries? registryQueries)
    {
        if (registryQueries is not null && registryQueries.TryGetPerson(person.Id, out PersonRecord record))
        {
            return record.IsAlive;
        }

        return person.IsAlive;
    }

    private static int GetFamilyCommandAgeMonths(
        FamilyPersonState person,
        IPersonRegistryQueries? registryQueries,
        GameDate currentDate)
    {
        int registryAgeMonths = registryQueries?.GetAgeMonths(person.Id, currentDate) ?? -1;
        return registryAgeMonths >= 0 ? registryAgeMonths : person.AgeMonths;
    }

}
