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

    public IOrderAndBanditryQueries? OrderQueries { get; init; }
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
            case PlayerCommandNames.AskClanEldersExplain:
                ApplyClanEldersPublicLifeExplanation(
                    clan,
                    ResolveOrderResidue(context.OrderQueries, command.SettlementId),
                    ResolvePublicLifeResponseResidueFriction(context.SocialMemoryQueries, clan.Id));
                break;
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
            PlayerCommandNames.AskClanEldersExplain => "请族老解释",
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
        return string.Equals(commandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.AskClanEldersExplain, StringComparison.Ordinal);
    }

    private static string DeterminePublicLifeFamilyCommandLabel(string commandName)
    {
        if (string.Equals(commandName, PlayerCommandNames.AskClanEldersExplain, StringComparison.Ordinal))
        {
            return "请族老解释";
        }

        return string.Equals(commandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal)
            ? "请族老出面"
            : DetermineFamilyCommandLabel(commandName);
    }

    private static void ApplyClanEldersPublicLifeExplanation(
        ClanStateData clan,
        SettlementDisorderSnapshot? orderResidue,
        PublicLifeResponseResidueFriction responseFriction)
    {
        bool hasResidue = HasRefusalOrPartialResidue(orderResidue);
        string outcomeCode = PublicLifeOrderResponseOutcomeCodes.Ignored;
        string traceCode = PublicLifeOrderResponseTraceCodes.FamilyResponseIgnored;
        string summary = "族老未接上前案，后账仍放在原处。";

        if (hasResidue)
        {
            int standing =
                clan.Prestige
                + (clan.SupportReserve * 2)
                + clan.MediationMomentum
                + responseFriction.StandingShift;
            bool hardeningBlocksExplanation = responseFriction.HardeningDrag >= 6 && standing < 24;
            bool hasEnoughStanding = standing >= 12 && !hardeningBlocksExplanation;

            if (hardeningBlocksExplanation)
            {
                outcomeCode = PublicLifeOrderResponseOutcomeCodes.Ignored;
                traceCode = PublicLifeOrderResponseTraceCodes.FamilyResponseIgnored;
                summary = "族老出面太迟，旧后账已经转硬，街口只当本户又来遮羞。";
                clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + 2, 0, 100);
                clan.BranchTension = Math.Clamp(clan.BranchTension + 2, 0, 100);
                clan.BranchFavorPressure = Math.Clamp(clan.BranchFavorPressure + 1, 0, 100);
            }
            else
            {
                outcomeCode = hasEnoughStanding
                    ? PublicLifeOrderResponseOutcomeCodes.Repaired
                    : PublicLifeOrderResponseOutcomeCodes.Contained;
                traceCode = PublicLifeOrderResponseTraceCodes.FamilyElderExplained;
                summary = hasEnoughStanding
                    ? "族老在县门与街口解释本户前意，公开羞面被缓下。"
                    : "族老先把街口议论压住，但本户担保仍欠人情。";

                clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + (hasEnoughStanding ? 9 : 6) + responseFriction.MediationShift, 0, 100);
                clan.BranchTension = Math.Max(0, clan.BranchTension - (hasEnoughStanding ? 5 : 3));
                clan.BranchFavorPressure = Math.Max(0, clan.BranchFavorPressure - (hasEnoughStanding ? 3 : 1));
                clan.ReliefSanctionPressure = Math.Max(0, clan.ReliefSanctionPressure - 1);
                if (hasEnoughStanding)
                {
                    clan.Prestige = Math.Clamp(clan.Prestige + 1, 0, 100);
                }
                else if (clan.SupportReserve > 0)
                {
                    clan.SupportReserve -= 1;
                }
            }
        }

        summary = AppendResponseFrictionSummary(summary, responseFriction);
        ApplyRefusalResponseReceipt(
            clan,
            PlayerCommandNames.AskClanEldersExplain,
            DeterminePublicLifeFamilyCommandLabel(PlayerCommandNames.AskClanEldersExplain),
            summary,
            outcomeCode,
            traceCode);
        clan.LastConflictOutcome = summary;
        clan.LastConflictTrace = $"{clan.ClanName}{summary}";
    }

    private static PublicLifeResponseResidueFriction ResolvePublicLifeResponseResidueFriction(
        ISocialMemoryAndRelationsQueries? socialQueries,
        ClanId clanId)
    {
        if (socialQueries is null)
        {
            return PublicLifeResponseResidueFriction.Neutral;
        }

        int repaired = 0;
        int contained = 0;
        int escalated = 0;
        int ignored = 0;

        foreach (SocialMemoryEntrySnapshot memory in socialQueries.GetMemoriesByClan(clanId)
                     .Where(static memory => memory.State == MemoryLifecycleState.Active)
                     .Where(static memory => memory.CauseKey.StartsWith("order.public_life.response.", StringComparison.Ordinal)))
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
        ClanStateData clan,
        string commandName,
        string commandLabel,
        string summary,
        string outcomeCode,
        string traceCode)
    {
        clan.LastRefusalResponseCommandCode = commandName;
        clan.LastRefusalResponseCommandLabel = commandLabel;
        clan.LastRefusalResponseSummary = summary;
        clan.LastRefusalResponseOutcomeCode = outcomeCode;
        clan.LastRefusalResponseTraceCode = traceCode;
        clan.ResponseCarryoverMonths = 2;
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

    private readonly record struct PublicLifeResponseResidueFriction(
        int RepairedWeight,
        int ContainedWeight,
        int EscalatedWeight,
        int IgnoredWeight)
    {
        private int SoftSignal => Math.Clamp((RepairedWeight / 18) + (ContainedWeight / 32), 0, 6);

        private int HardSignal => Math.Clamp((EscalatedWeight / 12) + (IgnoredWeight / 16), 0, 8);

        public int StandingShift => Math.Clamp(SoftSignal - HardSignal, -8, 6);

        public int MediationShift => Math.Clamp(SoftSignal / 2, 0, 3);

        public int HardeningDrag => HardSignal;

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
