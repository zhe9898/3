using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public static partial class FamilyCoreCommandResolver
{
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

}
