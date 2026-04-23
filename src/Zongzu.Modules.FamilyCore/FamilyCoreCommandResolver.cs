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

public static class FamilyCoreCommandResolver
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

    private static PlayerCommandResult IssueArrangeMarriage(
        FamilyCoreCommandContext context,
        ClanStateData clan,
        FamilyCommandSocialModifier social)
    {
        bool hasMarriageableAdult = context.State.People.Any(person =>
            person.ClanId == clan.Id
            && IsFamilyCommandPersonAlive(person, context.PersonRegistryQueries)
            && GetFamilyCommandAgeMonths(person, context.PersonRegistryQueries, context.CurrentDate) >= FamilyCoreModule.AdultAgeMonths
            && GetFamilyCommandAgeMonths(person, context.PersonRegistryQueries, context.CurrentDate) < 40 * 12);
        if (!hasMarriageableAdult)
        {
            return BuildRejectedFamilyResult(context.Command, $"{clan.ClanName}眼下无可议亲之人，先看家内年龄与服丧轻重。");
        }

        FamilyMarriageResolutionProfile profile = ApplyMarriageSocialModifier(ResolveMarriageProfile(clan), social);
        clan.MarriageAllianceValue = CommandResolutionMath.Clamp100(Math.Max(clan.MarriageAllianceValue, 28) + profile.MarriageAllianceValueLift);
        clan.MarriageAlliancePressure = Math.Max(0, clan.MarriageAlliancePressure - profile.MarriageAlliancePressureRelief);
        clan.ReproductivePressure = CommandResolutionMath.Clamp100(clan.ReproductivePressure + profile.ReproductivePressureLift);
        clan.HeirSecurity = CommandResolutionMath.Clamp100(clan.HeirSecurity + profile.HeirSecurityLift);
        clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension - profile.BranchTensionRelief + profile.BranchTensionBacklash);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - profile.SupportCost);
        clan.LastLifecycleOutcome = $"婚议已定，先借姻亲稳住香火与门面；婚议之压缓到{clan.MarriageAlliancePressure}，姻亲可资之势起到{clan.MarriageAllianceValue}，承祧稳度起到{clan.HeirSecurity}，宗房余力余{clan.SupportReserve}。{RenderBranchBacklash(profile.BranchTensionBacklash)}";
        clan.LastLifecycleTrace = $"{clan.ClanName}按{profile.ExecutionSummary}裁量婚议，已把媒妁往来、聘财轻重与堂内脸面一并料理。";
        clan.LastLifecycleCommandCode = context.Command.CommandName;
        clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(context.Command.CommandName);

        return BuildAcceptedFamilyLifecycleResult(context.Command, clan);
    }

    private static PlayerCommandResult IssueDesignateHeirPolicy(
        FamilyCoreCommandContext context,
        ClanStateData clan,
        FamilyCommandSocialModifier social)
    {
        var candidateEntry = context.State.People
            .Where(person => person.ClanId == clan.Id && IsFamilyCommandPersonAlive(person, context.PersonRegistryQueries))
            .Select(person => new
            {
                Person = person,
                AgeMonths = GetFamilyCommandAgeMonths(person, context.PersonRegistryQueries, context.CurrentDate),
            })
            .OrderByDescending(static entry => entry.AgeMonths >= FamilyCoreModule.AdultAgeMonths)
            .ThenByDescending(static entry => entry.AgeMonths)
            .ThenBy(static entry => entry.Person.Id.Value)
            .FirstOrDefault();
        if (candidateEntry is null)
        {
            return BuildRejectedFamilyResult(context.Command, $"{clan.ClanName}门内暂无人可立嗣，先看婚议与抚育能否续上。");
        }

        FamilyPersonState candidate = candidateEntry.Person;
        int candidateAgeMonths = candidateEntry.AgeMonths;
        FamilyHeirResolutionProfile profile = ApplyHeirSocialModifier(ResolveHeirPolicyProfile(clan, candidateAgeMonths), social);
        clan.HeirPersonId = candidate.Id;
        clan.HeirSecurity = CommandResolutionMath.Clamp100(Math.Max(clan.HeirSecurity + profile.HeirSecurityLift, profile.HeirSecurityFloor));
        clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - profile.InheritancePressureRelief);
        clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension + profile.BranchTensionBacklash);
        clan.MediationMomentum = CommandResolutionMath.Clamp100(clan.MediationMomentum + profile.MediationMomentumLift);
        clan.LastLifecycleOutcome = candidateAgeMonths >= FamilyCoreModule.AdultAgeMonths
            ? $"承祧人已定，谱内名分先写稳了；承祧稳度{clan.HeirSecurity}，后议之压暂退到{clan.InheritancePressure}，调停势头到{clan.MediationMomentum}。{RenderBranchBacklash(profile.BranchTensionBacklash)}"
            : $"嗣苗已记入谱案，香火名分暂有着落；只是人尚年幼，承祧稳度只到{clan.HeirSecurity}，后议之压退到{clan.InheritancePressure}。{RenderBranchBacklash(profile.BranchTensionBacklash)}";
        clan.LastLifecycleTrace = candidateAgeMonths >= FamilyCoreModule.AdultAgeMonths
            ? $"{clan.ClanName}按{profile.ExecutionSummary}裁定承祧次序，先把香火名分写明，免得诸房借机翻后议。"
            : $"{clan.ClanName}按{profile.ExecutionSummary}先把嗣苗记入谱案，虽未成人，堂上总算先把香火名分定住。";
        clan.LastLifecycleCommandCode = context.Command.CommandName;
        clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(context.Command.CommandName);

        return BuildAcceptedFamilyLifecycleResult(context.Command, clan);
    }

    private static PlayerCommandResult IssueSupportNewbornCare(
        FamilyCoreCommandContext context,
        ClanStateData clan,
        FamilyCommandSocialModifier social)
    {
        int infantCount = context.State.People.Count(person =>
            person.ClanId == clan.Id
            && IsFamilyCommandPersonAlive(person, context.PersonRegistryQueries)
            && GetFamilyCommandAgeMonths(person, context.PersonRegistryQueries, context.CurrentDate) <= FamilyCoreModule.InfantAgeMonths);
        if (infantCount == 0)
        {
            return BuildRejectedFamilyResult(context.Command, $"{clan.ClanName}门内眼下并无襁褓待护，先看婚议、承祧与丧服轻重。");
        }

        if (clan.SupportReserve < 4)
        {
            return BuildRejectedFamilyResult(context.Command, $"{clan.ClanName}宗房余力过浅，暂难另拨米药与乳养之费。");
        }

        FamilyLifecycleResolutionProfile profile = ApplyLifecycleSocialModifier(
            context.Command.CommandName,
            ResolveNewbornCareProfile(clan, infantCount),
            social);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - profile.SupportCost);
        clan.CareLoad = Math.Max(0, clan.CareLoad - profile.CareLoadRelief);
        clan.HeirSecurity = CommandResolutionMath.Clamp100(clan.HeirSecurity + profile.HeirSecurityLift);
        clan.ReproductivePressure = Math.Max(0, clan.ReproductivePressure - profile.ReproductivePressureRelief);
        clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension - profile.BranchTensionRelief + profile.BranchTensionBacklash);
        clan.RemedyConfidence = CommandResolutionMath.Clamp100(clan.RemedyConfidence + profile.RemedyConfidenceLift);
        clan.CharityObligation = CommandResolutionMath.Clamp100(clan.CharityObligation + profile.CharityObligationLift);
        clan.LastLifecycleOutcome = $"已拨米药护住产妇与襁褓；门内襁褓{infantCount}口，照料负担降到{clan.CareLoad}，承祧稳度升到{clan.HeirSecurity}，生育追压退到{clan.ReproductivePressure}，宗房余力余{clan.SupportReserve}。{RenderLifecycleBacklash(profile)}";
        clan.LastLifecycleTrace = $"{clan.ClanName}按{profile.ExecutionSummary}裁量护婴，先把米药、乳养与照看人手拨到近亲身边。";
        clan.LastLifecycleCommandCode = context.Command.CommandName;
        clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(context.Command.CommandName);

        return BuildAcceptedFamilyLifecycleResult(context.Command, clan);
    }

    private static PlayerCommandResult IssueSetMourningOrder(
        FamilyCoreCommandContext context,
        ClanStateData clan,
        FamilyCommandSocialModifier social)
    {
        if (clan.MourningLoad <= 0)
        {
            return BuildRejectedFamilyResult(context.Command, $"{clan.ClanName}门内暂无丧服之重，眼下不必另议丧次。");
        }

        FamilyLifecycleResolutionProfile profile = ApplyLifecycleSocialModifier(
            context.Command.CommandName,
            ResolveMourningOrderProfile(clan),
            social);
        clan.MourningLoad = Math.Max(0, clan.MourningLoad - profile.MourningLoadRelief);
        clan.FuneralDebt = Math.Max(0, clan.FuneralDebt - profile.FuneralDebtRelief);
        clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - profile.InheritancePressureRelief);
        clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension - profile.BranchTensionRelief + profile.BranchTensionBacklash);
        clan.MediationMomentum = CommandResolutionMath.Clamp100(clan.MediationMomentum + profile.MediationMomentumLift);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - profile.SupportCost);
        clan.LastLifecycleOutcome = $"丧次与祭次已定，丧服之重缓到{clan.MourningLoad}，丧葬拖欠降到{clan.FuneralDebt}，后议之压退到{clan.InheritancePressure}，宗房余力余{clan.SupportReserve}。{RenderLifecycleBacklash(profile)}";
        clan.LastLifecycleTrace = $"{clan.ClanName}按{profile.ExecutionSummary}裁定发引、祭次与服序，先让举哀、支用和承祧后议都有规矩可循。";
        clan.LastLifecycleCommandCode = context.Command.CommandName;
        clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(context.Command.CommandName);

        return BuildAcceptedFamilyLifecycleResult(context.Command, clan);
    }

    private static FamilyCommandSocialModifier BuildSocialModifier(FamilyCoreCommandContext context, ClanStateData clan)
    {
        if (context.SocialMemoryQueries is null)
        {
            return FamilyCommandSocialModifier.Neutral;
        }

        ClanEmotionalClimateSnapshot climate;
        try
        {
            climate = context.SocialMemoryQueries.GetClanEmotionalClimate(clan.Id);
        }
        catch (InvalidOperationException)
        {
            return FamilyCommandSocialModifier.Neutral;
        }

        PersonPressureTemperingSnapshot[] adultTemperings = context.SocialMemoryQueries
            .GetPersonTemperingsByClan(clan.Id)
            .Where(tempering => IsAdultAlive(context, tempering.PersonId))
            .OrderBy(static tempering => tempering.PersonId.Value)
            .ToArray();

        int adultCount = Math.Max(1, adultTemperings.Length);
        int avgRestraint = adultTemperings.Sum(static tempering => tempering.Restraint) / adultCount;
        int avgHardening = adultTemperings.Sum(static tempering => tempering.Hardening) / adultCount;
        int avgBitterness = adultTemperings.Sum(static tempering => tempering.Bitterness) / adultCount;
        int avgVolatility = adultTemperings.Sum(static tempering => tempering.Volatility) / adultCount;
        int avgTrust = adultTemperings.Sum(static tempering => tempering.Trust) / adultCount;

        int volatilityBand = CommandResolutionBands.Score(Math.Max(climate.Volatility, avgVolatility), 45, 65, 82);
        int bitternessBand = CommandResolutionBands.Score(Math.Max(climate.Bitterness, avgBitterness), 45, 65, 82);
        int hardeningBand = CommandResolutionBands.Score(Math.Max(climate.Hardening, avgHardening), 45, 65, 82);
        int trustBand = CommandResolutionBands.Score(Math.Max(climate.Trust, avgTrust), 45, 65, 82);
        int restraintBand = CommandResolutionBands.Score(Math.Max(climate.Restraint, avgRestraint), 45, 65, 82);
        int obligationBand = CommandResolutionBands.Score(climate.Obligation, 45, 65, 82);
        int griefBand = CommandResolutionBands.Score(climate.Grief, 45, 65, 82);
        int shameBand = CommandResolutionBands.Score(climate.Shame, 45, 65, 82);
        int angerBand = CommandResolutionBands.Score(climate.Anger, 45, 65, 82);

        int resistanceBand = Math.Clamp(volatilityBand + bitternessBand + (angerBand / 2) - restraintBand - trustBand, 0, 6);
        int cohesionBand = Math.Clamp(trustBand + restraintBand + obligationBand - bitternessBand, 0, 6);
        int backlashDelta = Math.Clamp(resistanceBand + griefBand - trustBand, 0, 5);
        int reliefBonus = Math.Clamp(cohesionBand - resistanceBand, 0, 4);
        int reliefPenalty = Math.Clamp(resistanceBand - cohesionBand, 0, 4);
        int supportCostDelta = Math.Clamp(griefBand + shameBand + volatilityBand - trustBand - restraintBand, -2, 4);
        int harshCommandPressure = Math.Clamp(hardeningBand + angerBand - trustBand, 0, 4);
        bool hasSignal = volatilityBand + bitternessBand + hardeningBand + trustBand + restraintBand + obligationBand + griefBand + shameBand + angerBand > 0;

        string trace = hasSignal
            ? CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("余怨", bitternessBand),
                new CommandResolutionFactor("克制", restraintBand),
                new CommandResolutionFactor("信义", trustBand),
                new CommandResolutionFactor("躁动", volatilityBand),
                new CommandResolutionFactor("硬气", hardeningBand))
            : string.Empty;

        return new FamilyCommandSocialModifier(
            hasSignal,
            reliefBonus,
            reliefPenalty,
            backlashDelta,
            supportCostDelta,
            harshCommandPressure,
            Math.Clamp(cohesionBand - resistanceBand, -3, 5),
            trace);
    }

    private static bool IsAdultAlive(FamilyCoreCommandContext context, PersonId personId)
    {
        FamilyPersonState? person = context.State.People.FirstOrDefault(person => person.Id == personId);
        return person is not null
            && IsFamilyCommandPersonAlive(person, context.PersonRegistryQueries)
            && GetFamilyCommandAgeMonths(person, context.PersonRegistryQueries, context.CurrentDate) >= FamilyCoreModule.AdultAgeMonths;
    }

    private static FamilyConflictResolutionProfile ApplyConflictSocialModifier(
        string commandName,
        FamilyConflictResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        bool isReconciliation = commandName is PlayerCommandNames.OrderFormalApology
            or PlayerCommandNames.InviteClanEldersMediation
            or PlayerCommandNames.InviteClanEldersPubliclyBroker;
        bool isHarsh = commandName is PlayerCommandNames.SupportSeniorBranch
            or PlayerCommandNames.SuspendClanRelief;

        int reliefShift = isReconciliation ? social.ReliefBonus - social.ReliefPenalty : 0;
        int harshShift = isHarsh ? social.HarshCommandPressure : 0;

        return profile with
        {
            BranchFavorPressureDelta = ApplySignedReliefShift(profile.BranchFavorPressureDelta, reliefShift),
            BranchTensionDelta = ApplySignedReliefShift(profile.BranchTensionDelta, reliefShift) + harshShift + (isReconciliation ? 0 : social.BacklashDelta / 2),
            InheritancePressureDelta = ApplySignedReliefShift(profile.InheritancePressureDelta, reliefShift / 2),
            SeparationPressureDelta = ApplySignedReliefShift(profile.SeparationPressureDelta, reliefShift) + (isHarsh ? social.BacklashDelta / 2 : 0),
            MediationMomentumDelta = profile.MediationMomentumDelta + (isReconciliation ? social.MediationMomentumDelta : -social.ReliefPenalty),
            SupportReserveDelta = profile.SupportReserveDelta - Math.Max(0, social.SupportCostDelta),
            ReliefSanctionPressureDelta = profile.ReliefSanctionPressureDelta + (isHarsh ? social.HarshCommandPressure : 0),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static FamilyMarriageResolutionProfile ApplyMarriageSocialModifier(
        FamilyMarriageResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        return profile with
        {
            MarriageAllianceValueLift = Math.Max(1, profile.MarriageAllianceValueLift + social.ReliefBonus - (social.ReliefPenalty / 2)),
            MarriageAlliancePressureRelief = Math.Max(1, profile.MarriageAlliancePressureRelief + social.ReliefBonus - social.ReliefPenalty),
            HeirSecurityLift = Math.Max(0, profile.HeirSecurityLift + (social.ReliefBonus / 2) - (social.BacklashDelta / 2)),
            BranchTensionRelief = Math.Max(0, profile.BranchTensionRelief + social.ReliefBonus - social.ReliefPenalty),
            BranchTensionBacklash = Math.Max(0, profile.BranchTensionBacklash + social.BacklashDelta - social.ReliefBonus),
            SupportCost = Math.Max(1, profile.SupportCost + Math.Max(0, social.SupportCostDelta)),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static FamilyHeirResolutionProfile ApplyHeirSocialModifier(
        FamilyHeirResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        return profile with
        {
            HeirSecurityFloor = Math.Clamp(profile.HeirSecurityFloor + social.ReliefBonus - social.ReliefPenalty, 25, 78),
            HeirSecurityLift = Math.Max(0, profile.HeirSecurityLift + social.ReliefBonus - (social.ReliefPenalty / 2)),
            InheritancePressureRelief = Math.Max(1, profile.InheritancePressureRelief + social.ReliefBonus - social.ReliefPenalty),
            BranchTensionBacklash = Math.Max(0, profile.BranchTensionBacklash + social.BacklashDelta - social.ReliefBonus),
            MediationMomentumLift = Math.Max(0, profile.MediationMomentumLift + social.MediationMomentumDelta),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static FamilyLifecycleResolutionProfile ApplyLifecycleSocialModifier(
        string commandName,
        FamilyLifecycleResolutionProfile profile,
        FamilyCommandSocialModifier social)
    {
        if (!social.HasSignal)
        {
            return profile;
        }

        bool isCare = commandName == PlayerCommandNames.SupportNewbornCare;
        bool isMourning = commandName == PlayerCommandNames.SetMourningOrder;
        int reliefBonus = isCare || isMourning ? social.ReliefBonus : 0;

        return profile with
        {
            SupportCost = Math.Max(0, profile.SupportCost + social.SupportCostDelta),
            CareLoadRelief = Math.Max(0, profile.CareLoadRelief + (isCare ? reliefBonus - social.ReliefPenalty : 0)),
            FuneralDebtRelief = Math.Max(0, profile.FuneralDebtRelief + (isMourning ? reliefBonus - social.ReliefPenalty : 0)),
            MourningLoadRelief = Math.Max(0, profile.MourningLoadRelief + (isMourning ? reliefBonus - social.ReliefPenalty : 0)),
            HeirSecurityLift = Math.Max(0, profile.HeirSecurityLift + (reliefBonus / 2) - (social.BacklashDelta / 2)),
            ReproductivePressureRelief = Math.Max(0, profile.ReproductivePressureRelief + (isCare ? reliefBonus / 2 : 0)),
            InheritancePressureRelief = Math.Max(0, profile.InheritancePressureRelief + (isMourning ? reliefBonus / 2 : 0)),
            BranchTensionRelief = Math.Max(0, profile.BranchTensionRelief + reliefBonus - social.ReliefPenalty),
            BranchTensionBacklash = Math.Max(0, profile.BranchTensionBacklash + social.BacklashDelta - reliefBonus),
            CharityObligationLift = Math.Max(0, profile.CharityObligationLift + (isCare ? reliefBonus / 2 : 0)),
            RemedyConfidenceLift = Math.Max(0, profile.RemedyConfidenceLift + (isCare ? reliefBonus / 2 : 0)),
            MediationMomentumLift = Math.Max(0, profile.MediationMomentumLift + (isMourning ? social.MediationMomentumDelta : 0)),
            ExecutionSummary = AppendSocialTrace(profile.ExecutionSummary, social),
        };
    }

    private static int ApplySignedReliefShift(int delta, int reliefShift)
    {
        if (delta < 0)
        {
            return delta - reliefShift;
        }

        if (delta > 0)
        {
            return Math.Max(0, delta - reliefShift);
        }

        return delta;
    }

    private static string AppendSocialTrace(string executionSummary, FamilyCommandSocialModifier social)
    {
        return social.HasSignal ? $"{executionSummary}、{social.Trace}" : executionSummary;
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

    private static FamilyConflictResolutionProfile ResolveSupportSeniorBranchProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int inheritanceBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 8, 25, 45);
        int mediationBand = CommandResolutionBands.Score(clan.MediationMomentum, 10, 30, 55);
        int prestigeBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: Math.Clamp(12 + (inheritanceBand * 2) + prestigeBand, 12, 22),
            BranchTensionDelta: Math.Clamp(5 + branchPressureBand + inheritanceBand - (mediationBand / 2), 3, 14),
            InheritancePressureDelta: Math.Clamp(3 + inheritanceBand + (branchPressureBand / 2), 2, 8),
            SeparationPressureDelta: 0,
            MediationMomentumDelta: -Math.Clamp(2 + (branchPressureBand / 2), 1, 5),
            SupportReserveDelta: -Math.Clamp(1 + (supportBand >= 2 ? 1 : 0), 1, 3),
            ReliefSanctionPressureDelta: 0,
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("后议压力", inheritanceBand),
                new CommandResolutionFactor("宗房余力", supportBand),
                new CommandResolutionFactor("调停余势", mediationBand),
                new CommandResolutionFactor("门第声势", prestigeBand)));
    }

    private static FamilyConflictResolutionProfile ResolveFormalApologyProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int favorBand = CommandResolutionBands.Score(clan.BranchFavorPressure, 15, 35, 60);
        int prestigeBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: -Math.Clamp(2 + favorBand + (prestigeBand / 2), 2, 8),
            BranchTensionDelta: -Math.Clamp(8 + (branchPressureBand * 2) + prestigeBand, 6, 18),
            InheritancePressureDelta: 0,
            SeparationPressureDelta: -Math.Clamp(3 + separationBand + (prestigeBand / 2), 2, 9),
            MediationMomentumDelta: Math.Clamp(5 + prestigeBand + (branchPressureBand / 2), 5, 12),
            SupportReserveDelta: 0,
            ReliefSanctionPressureDelta: 0,
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("偏怨旧账", favorBand),
                new CommandResolutionFactor("门第声势", prestigeBand)));
    }

    private static FamilyConflictResolutionProfile ResolveBranchSeparationProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int inheritanceBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 8, 25, 45);
        int reliefBand = CommandResolutionBands.Score(clan.ReliefSanctionPressure, 15, 35, 60);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: 0,
            BranchTensionDelta: -Math.Clamp(3 + branchPressureBand + (supportBand / 2), 3, 9),
            InheritancePressureDelta: -Math.Clamp(4 + inheritanceBand + separationBand, 3, 12),
            SeparationPressureDelta: -Math.Clamp(12 + (separationBand * 4) + branchPressureBand, 10, 28),
            MediationMomentumDelta: Math.Clamp(2 + (supportBand / 2), 2, 5),
            SupportReserveDelta: -Math.Clamp(4 + separationBand - (supportBand / 2), 4, 10),
            ReliefSanctionPressureDelta: -Math.Clamp(2 + reliefBand, 1, 6),
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("后议压力", inheritanceBand),
                new CommandResolutionFactor("宗房余力", supportBand),
                new CommandResolutionFactor("救济制裁", reliefBand)));
    }

    private static FamilyConflictResolutionProfile ResolveSuspendReliefProfile(ClanStateData clan)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int reliefBand = CommandResolutionBands.Score(clan.ReliefSanctionPressure, 15, 35, 60);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 8, 25, 45);

        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: 0,
            BranchTensionDelta: Math.Clamp(5 + branchPressureBand + reliefBand, 5, 14),
            InheritancePressureDelta: 0,
            SeparationPressureDelta: Math.Clamp(3 + separationBand + reliefBand, 3, 10),
            MediationMomentumDelta: -Math.Clamp(2 + branchPressureBand, 2, 6),
            SupportReserveDelta: Math.Clamp(2 + reliefBand + (supportBand == 0 ? 1 : 0), 2, 6),
            ReliefSanctionPressureDelta: Math.Clamp(12 + (reliefBand * 2), 12, 22),
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("救济制裁", reliefBand),
                new CommandResolutionFactor("宗房余力", supportBand)));
    }

    private static FamilyConflictResolutionProfile ResolveClanEldersMediationProfile(ClanStateData clan, bool publicBroker)
    {
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int separationBand = CommandResolutionBands.Score(clan.SeparationPressure, 15, 35, 60);
        int favorBand = CommandResolutionBands.Score(clan.BranchFavorPressure, 15, 35, 60);
        int inheritanceBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int prestigeBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);

        int publicDiscount = publicBroker ? 2 : 0;
        return new FamilyConflictResolutionProfile(
            BranchFavorPressureDelta: -Math.Clamp(3 + favorBand + (prestigeBand / 2) - publicDiscount, 2, 8),
            BranchTensionDelta: -Math.Clamp(5 + branchPressureBand + prestigeBand - publicDiscount, 3, 12),
            InheritancePressureDelta: publicBroker ? 0 : -Math.Clamp(1 + (inheritanceBand / 2), 1, 4),
            SeparationPressureDelta: -Math.Clamp(3 + separationBand + (prestigeBand / 2) - publicDiscount, 2, 8),
            MediationMomentumDelta: Math.Clamp((publicBroker ? 8 : 10) + prestigeBand + branchPressureBand + separationBand, publicBroker ? 8 : 10, publicBroker ? 18 : 22),
            SupportReserveDelta: -Math.Clamp(publicBroker ? 2 + (prestigeBand / 2) : 1 + prestigeBand, publicBroker ? 2 : 1, publicBroker ? 5 : 4),
            ReliefSanctionPressureDelta: 0,
            ExecutionSummary: CommandResolutionProfileText.RenderFactors(
                new CommandResolutionFactor("房支争势", branchPressureBand),
                new CommandResolutionFactor("分房之压", separationBand),
                new CommandResolutionFactor("偏怨旧账", favorBand),
                new CommandResolutionFactor("后议压力", inheritanceBand),
                new CommandResolutionFactor("门第声势", prestigeBand)));
    }

    private static void ApplyFamilyConflictProfile(ClanStateData clan, FamilyConflictResolutionProfile profile)
    {
        clan.BranchFavorPressure = ApplyPressureDelta(clan.BranchFavorPressure, profile.BranchFavorPressureDelta);
        clan.BranchTension = ApplyPressureDelta(clan.BranchTension, profile.BranchTensionDelta);
        clan.InheritancePressure = ApplyPressureDelta(clan.InheritancePressure, profile.InheritancePressureDelta);
        clan.SeparationPressure = ApplyPressureDelta(clan.SeparationPressure, profile.SeparationPressureDelta);
        clan.MediationMomentum = ApplyPressureDelta(clan.MediationMomentum, profile.MediationMomentumDelta);
        clan.SupportReserve = ApplyPressureDelta(clan.SupportReserve, profile.SupportReserveDelta);
        clan.ReliefSanctionPressure = ApplyPressureDelta(clan.ReliefSanctionPressure, profile.ReliefSanctionPressureDelta);
    }

    private static int ApplyPressureDelta(int value, int delta)
    {
        return CommandResolutionMath.Clamp100(value + delta);
    }

    private static FamilyMarriageResolutionProfile ResolveMarriageProfile(ClanStateData clan)
    {
        int alliancePressureBand = CommandResolutionBands.Score(clan.MarriageAlliancePressure, 20, 40, 65);
        int allianceValueBand = CommandResolutionBands.Score(clan.MarriageAllianceValue, 28, 48, 70);
        int affinalNeedBand = Math.Clamp(3 - allianceValueBand, 0, 3);
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 12, 30, 50);
        int standingBand = CommandResolutionBands.Score(clan.Prestige, 35, 55, 75);
        int heirFragility = clan.HeirSecurity < 35 ? 2 : clan.HeirSecurity < 60 ? 1 : 0;
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int mourningDrag = clan.MourningLoad >= 24 ? 2 : clan.MourningLoad >= 12 ? 1 : 0;

        int valueLift = Math.Clamp(18 + (alliancePressureBand * 4) + (standingBand * 2) - (mourningDrag * 3), 12, 36);
        int pressureRelief = Math.Clamp(12 + (alliancePressureBand * 4) + (supportBand * 2) - (mourningDrag * 3), 6, 30);
        int reproductiveLift = Math.Clamp(4 + (heirFragility * 2) + alliancePressureBand - mourningDrag, 2, 12);
        int heirSecurityLift = Math.Clamp(3 + (heirFragility * 2) + standingBand - mourningDrag, 1, 10);
        int branchTensionRelief = clan.BranchTension >= 20 ? Math.Clamp(1 + standingBand + (supportBand / 2), 1, 5) : 0;
        int branchBacklash = supportBand == 0 && clan.BranchTension >= 35 ? 2 : mourningDrag > 0 && branchPressureBand >= 2 ? 1 : 0;
        int supportCost = Math.Clamp(2 + affinalNeedBand + (standingBand >= 2 ? 1 : 0) - (supportBand / 2), 2, 7);
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("婚议之压", alliancePressureBand),
            new CommandResolutionFactor("姻亲短处", affinalNeedBand),
            new CommandResolutionFactor("宗房余力", supportBand),
            new CommandResolutionFactor("门第声势", standingBand),
            new CommandResolutionFactor("丧服拖累", mourningDrag),
            new CommandResolutionFactor("承祧虚处", heirFragility),
            new CommandResolutionFactor("房支争势", branchPressureBand));

        return new FamilyMarriageResolutionProfile(
            valueLift,
            pressureRelief,
            reproductiveLift,
            heirSecurityLift,
            branchTensionRelief,
            branchBacklash,
            supportCost,
            executionSummary);
    }

    private static FamilyHeirResolutionProfile ResolveHeirPolicyProfile(ClanStateData clan, int candidateAgeMonths)
    {
        bool isAdultCandidate = candidateAgeMonths >= FamilyCoreModule.AdultAgeMonths;
        int candidateStabilityBand = isAdultCandidate
            ? CommandResolutionBands.Score(candidateAgeMonths, FamilyCoreModule.AdultAgeMonths, 20 * 12, 28 * 12)
            : CommandResolutionBands.Score(candidateAgeMonths, 4 * 12, 8 * 12, 12 * 12);
        int heirFragility = clan.HeirSecurity < 35 ? 2 : clan.HeirSecurity < 60 ? 1 : 0;
        int inheritancePressureBand = CommandResolutionBands.Score(clan.InheritancePressure, 20, 45, 70);
        int branchPressureBand = CommandResolutionBands.Score(clan.BranchTension, 20, 45, 70);
        int mediationBand = CommandResolutionBands.Score(clan.MediationMomentum, 10, 30, 55);

        int floorBase = isAdultCandidate ? 58 : 34;
        int heirSecurityFloor = Math.Clamp(floorBase + (candidateStabilityBand * 3) + (isAdultCandidate ? mediationBand : 0), 30, 72);
        int heirSecurityLift = Math.Clamp(2 + (heirFragility * 3) + candidateStabilityBand + (mediationBand / 2), 1, 12);
        int inheritanceRelief = Math.Clamp(5 + (candidateStabilityBand * 2) + mediationBand + inheritancePressureBand + (isAdultCandidate ? 2 : 0) - (branchPressureBand / 2), 3, 18);
        int branchBacklash = Math.Clamp(1 + branchPressureBand + heirFragility - (mediationBand / 2) - (isAdultCandidate ? 1 : 0), 0, 6);
        int mediationMomentumLift = Math.Clamp(1 + (mediationBand / 2) + (isAdultCandidate ? 1 : 0), 1, 4);
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("候选稳度", candidateStabilityBand),
            new CommandResolutionFactor("承祧虚处", heirFragility),
            new CommandResolutionFactor("后议压力", inheritancePressureBand),
            new CommandResolutionFactor("房支争势", branchPressureBand),
            new CommandResolutionFactor("调停余势", mediationBand));

        return new FamilyHeirResolutionProfile(
            heirSecurityFloor,
            heirSecurityLift,
            inheritanceRelief,
            branchBacklash,
            mediationMomentumLift,
            executionSummary);
    }

    private static FamilyLifecycleResolutionProfile ResolveNewbornCareProfile(ClanStateData clan, int infantCount)
    {
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 18, 36, 58);
        int careBand = CommandResolutionBands.Score(clan.CareLoad, 12, 30, 55);
        int remedyBand = CommandResolutionBands.Score(clan.RemedyConfidence, 25, 50, 72);
        int mourningDrag = clan.MourningLoad >= 24 ? 2 : clan.MourningLoad >= 12 ? 1 : 0;
        int infantBand = Math.Clamp(infantCount, 1, 3);

        int supportCost = Math.Clamp(3 + infantBand + careBand - supportBand, 4, 9);
        int careRelief = Math.Clamp(3 + supportBand + remedyBand + infantBand - mourningDrag, 2, 10);
        int heirSecurityLift = Math.Clamp(2 + infantBand + remedyBand - mourningDrag, 1, 8);
        int reproductivePressureRelief = Math.Clamp(2 + careBand + (remedyBand / 2), 1, 7);
        int branchTensionRelief = clan.BranchTension >= 25 ? Math.Clamp(1 + supportBand, 1, 4) : 0;
        int branchBacklash = clan.SupportReserve <= supportCost + 4 && clan.BranchTension >= 35 ? 2 : 0;
        int charityObligationLift = Math.Clamp(1 + infantBand, 1, 4);
        int remedyConfidenceLift = clan.RemedyConfidence >= 80 ? 0 : supportBand >= 1 ? 2 : 1;
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("宗房余力", supportBand),
            new CommandResolutionFactor("照料负担", careBand),
            new CommandResolutionFactor("药护信心", remedyBand),
            new CommandResolutionFactor("丧服拖累", mourningDrag),
            new CommandResolutionFactor("襁褓口数", infantBand));

        return new FamilyLifecycleResolutionProfile(
            supportCost,
            careRelief,
            0,
            0,
            heirSecurityLift,
            reproductivePressureRelief,
            0,
            branchTensionRelief,
            branchBacklash,
            charityObligationLift,
            remedyConfidenceLift,
            0,
            executionSummary);
    }

    private static FamilyLifecycleResolutionProfile ResolveMourningOrderProfile(ClanStateData clan)
    {
        int supportBand = CommandResolutionBands.Score(clan.SupportReserve, 12, 28, 48);
        int mourningBand = CommandResolutionBands.Score(clan.MourningLoad, 12, 28, 50);
        int funeralDebtBand = CommandResolutionBands.Score(clan.FuneralDebt, 8, 20, 40);
        int ritualStandingBand = CommandResolutionBands.Score(clan.Prestige + clan.MediationMomentum, 55, 85, 115);
        int heirFragility = clan.HeirSecurity < 35 ? 2 : clan.HeirSecurity < 55 ? 1 : 0;

        int supportCost = Math.Clamp(2 + funeralDebtBand + Math.Max(0, ritualStandingBand - 1), 2, 8);
        int mourningLoadRelief = Math.Clamp(5 + ritualStandingBand + supportBand - heirFragility, 4, 14);
        int funeralDebtRelief = Math.Clamp(2 + supportBand + ritualStandingBand - (clan.SupportReserve < 8 ? 1 : 0), 1, 9);
        int inheritancePressureRelief = Math.Clamp(2 + ritualStandingBand + mourningBand - heirFragility, 0, 9);
        int branchTensionRelief = clan.BranchTension > 0 ? Math.Clamp(1 + ritualStandingBand + (mourningBand / 2), 1, 6) : 0;
        int branchBacklash = heirFragility > 0 && clan.InheritancePressure >= 35 ? heirFragility : 0;
        int mediationMomentumLift = ritualStandingBand > 0 ? 1 : 0;
        string executionSummary = CommandResolutionProfileText.RenderFactors(
            new CommandResolutionFactor("礼法威望", ritualStandingBand),
            new CommandResolutionFactor("宗房余力", supportBand),
            new CommandResolutionFactor("丧服之重", mourningBand),
            new CommandResolutionFactor("丧葬债压", funeralDebtBand),
            new CommandResolutionFactor("承祧虚处", heirFragility));

        return new FamilyLifecycleResolutionProfile(
            supportCost,
            0,
            funeralDebtRelief,
            mourningLoadRelief,
            0,
            0,
            inheritancePressureRelief,
            branchTensionRelief,
            branchBacklash,
            0,
            0,
            mediationMomentumLift,
            executionSummary);
    }

    private static string RenderLifecycleBacklash(FamilyLifecycleResolutionProfile profile)
    {
        return RenderBranchBacklash(profile.BranchTensionBacklash);
    }

    private static string RenderBranchBacklash(int branchTensionBacklash)
    {
        return branchTensionBacklash > 0
            ? $"诸房仍有{branchTensionBacklash}点反弹留在堂内。"
            : string.Empty;
    }

    private readonly record struct FamilyCommandSocialModifier(
        bool HasSignal,
        int ReliefBonus,
        int ReliefPenalty,
        int BacklashDelta,
        int SupportCostDelta,
        int HarshCommandPressure,
        int MediationMomentumDelta,
        string Trace)
    {
        public static FamilyCommandSocialModifier Neutral { get; } = new(false, 0, 0, 0, 0, 0, 0, string.Empty);
    }

    private readonly record struct FamilyConflictResolutionProfile(
        int BranchFavorPressureDelta,
        int BranchTensionDelta,
        int InheritancePressureDelta,
        int SeparationPressureDelta,
        int MediationMomentumDelta,
        int SupportReserveDelta,
        int ReliefSanctionPressureDelta,
        string ExecutionSummary);

    private readonly record struct FamilyMarriageResolutionProfile(
        int MarriageAllianceValueLift,
        int MarriageAlliancePressureRelief,
        int ReproductivePressureLift,
        int HeirSecurityLift,
        int BranchTensionRelief,
        int BranchTensionBacklash,
        int SupportCost,
        string ExecutionSummary);

    private readonly record struct FamilyHeirResolutionProfile(
        int HeirSecurityFloor,
        int HeirSecurityLift,
        int InheritancePressureRelief,
        int BranchTensionBacklash,
        int MediationMomentumLift,
        string ExecutionSummary);

    private readonly record struct FamilyLifecycleResolutionProfile(
        int SupportCost,
        int CareLoadRelief,
        int FuneralDebtRelief,
        int MourningLoadRelief,
        int HeirSecurityLift,
        int ReproductivePressureRelief,
        int InheritancePressureRelief,
        int BranchTensionRelief,
        int BranchTensionBacklash,
        int CharityObligationLift,
        int RemedyConfidenceLift,
        int MediationMomentumLift,
        string ExecutionSummary);
}

internal static class CommandResolutionBands
{
    public static int Score(int value, int low, int medium, int high)
    {
        if (value >= high)
        {
            return 3;
        }

        if (value >= medium)
        {
            return 2;
        }

        return value >= low ? 1 : 0;
    }
}

internal readonly record struct CommandResolutionFactor(string Label, int Band)
{
    public string Render()
    {
        return $"{Label}{Band}阶";
    }
}

internal static class CommandResolutionProfileText
{
    public static string RenderFactors(params CommandResolutionFactor[] factors)
    {
        return string.Join("、", factors.Select(static factor => factor.Render()));
    }
}

internal static class CommandResolutionMath
{
    public static int Clamp100(int value)
    {
        return Math.Clamp(value, 0, 100);
    }
}
