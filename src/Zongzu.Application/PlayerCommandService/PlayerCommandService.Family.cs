using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;

namespace Zongzu.Application;

public sealed partial class PlayerCommandService
{
    private static PlayerCommandResult IssueFamilyIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.FamilyCore))
        {
            return BuildRejectedFamilyResult(command, "当前存档未启用宗房裁断。");
        }

        FamilyCoreState state = simulation.GetMutableModuleState<FamilyCoreState>(KnownModuleKeys.FamilyCore);
        IPersonRegistryQueries? personRegistryQueries = TryGetPersonRegistryQueries(simulation);
        ClanStateData[] localClans = state.Clans
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

        switch (command.CommandName)
        {
            case PlayerCommandNames.ArrangeMarriage:
            {
                bool hasMarriageableAdult = state.People.Any(person =>
                    person.ClanId == clan.Id
                    && IsFamilyCommandPersonAlive(person, personRegistryQueries)
                    && GetFamilyCommandAgeMonths(person, personRegistryQueries, simulation.CurrentDate) >= 16 * 12
                    && GetFamilyCommandAgeMonths(person, personRegistryQueries, simulation.CurrentDate) < 40 * 12);
                if (!hasMarriageableAdult)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}眼下无可议亲之人，先看家内年龄与服丧轻重。");
                }

                FamilyMarriageResolutionProfile profile = ResolveMarriageProfile(clan);
                clan.MarriageAllianceValue = CommandResolutionMath.Clamp100(Math.Max(clan.MarriageAllianceValue, 28) + profile.MarriageAllianceValueLift);
                clan.MarriageAlliancePressure = Math.Max(0, clan.MarriageAlliancePressure - profile.MarriageAlliancePressureRelief);
                clan.ReproductivePressure = CommandResolutionMath.Clamp100(clan.ReproductivePressure + profile.ReproductivePressureLift);
                clan.HeirSecurity = CommandResolutionMath.Clamp100(clan.HeirSecurity + profile.HeirSecurityLift);
                clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension - profile.BranchTensionRelief + profile.BranchTensionBacklash);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - profile.SupportCost);
                clan.LastLifecycleOutcome = $"婚议已定，先借姻亲稳住香火与门面；婚议之压缓到{clan.MarriageAlliancePressure}，姻亲可资之势起到{clan.MarriageAllianceValue}，承祧稳度起到{clan.HeirSecurity}，宗房余力余{clan.SupportReserve}。{RenderBranchBacklash(profile.BranchTensionBacklash)}";
                clan.LastLifecycleTrace = $"{clan.ClanName}按{profile.ExecutionSummary}裁量婚议，已把媒妁往来、聘财轻重与堂内脸面一并料理。";
                clan.LastLifecycleCommandCode = command.CommandName;
                clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(command.CommandName);
                simulation.RefreshReplayHash();

                return BuildAcceptedFamilyLifecycleResult(command, clan);
            }
            case PlayerCommandNames.DesignateHeirPolicy:
            {
                var candidateEntry = state.People
                    .Where(person => person.ClanId == clan.Id && IsFamilyCommandPersonAlive(person, personRegistryQueries))
                    .Select(person => new
                    {
                        Person = person,
                        AgeMonths = GetFamilyCommandAgeMonths(person, personRegistryQueries, simulation.CurrentDate),
                    })
                    .OrderByDescending(static entry => entry.AgeMonths >= 16 * 12)
                    .ThenByDescending(static entry => entry.AgeMonths)
                    .ThenBy(static entry => entry.Person.Id.Value)
                    .FirstOrDefault();
                if (candidateEntry is null)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}门内暂无人可立嗣，先看婚议与抚育能否续上。");
                }

                FamilyPersonState candidate = candidateEntry.Person;
                int candidateAgeMonths = candidateEntry.AgeMonths;
                FamilyHeirResolutionProfile profile = ResolveHeirPolicyProfile(clan, candidateAgeMonths);
                clan.HeirPersonId = candidate.Id;
                clan.HeirSecurity = CommandResolutionMath.Clamp100(Math.Max(clan.HeirSecurity + profile.HeirSecurityLift, profile.HeirSecurityFloor));
                clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - profile.InheritancePressureRelief);
                clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension + profile.BranchTensionBacklash);
                clan.MediationMomentum = CommandResolutionMath.Clamp100(clan.MediationMomentum + profile.MediationMomentumLift);
                clan.LastLifecycleOutcome = candidateAgeMonths >= 16 * 12
                    ? $"承祧人已定，谱内名分先写稳了；承祧稳度{clan.HeirSecurity}，后议之压暂退到{clan.InheritancePressure}，调停势头到{clan.MediationMomentum}。{RenderBranchBacklash(profile.BranchTensionBacklash)}"
                    : $"嗣苗已记入谱案，香火名分暂有着落；只是人尚年幼，承祧稳度只到{clan.HeirSecurity}，后议之压退到{clan.InheritancePressure}。{RenderBranchBacklash(profile.BranchTensionBacklash)}";
                clan.LastLifecycleTrace = candidateAgeMonths >= 16 * 12
                    ? $"{clan.ClanName}按{profile.ExecutionSummary}裁定承祧次序，先把香火名分写明，免得诸房借机翻后议。"
                    : $"{clan.ClanName}按{profile.ExecutionSummary}先把嗣苗记入谱案，虽未成人，堂上总算先把香火名分定住。";
                clan.LastLifecycleCommandCode = command.CommandName;
                clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(command.CommandName);
                simulation.RefreshReplayHash();

                return BuildAcceptedFamilyLifecycleResult(command, clan);
            }
            case PlayerCommandNames.SupportNewbornCare:
            {
                int infantCount = state.People.Count(person =>
                    person.ClanId == clan.Id
                    && IsFamilyCommandPersonAlive(person, personRegistryQueries)
                    && GetFamilyCommandAgeMonths(person, personRegistryQueries, simulation.CurrentDate) <= FamilyInfantAgeMonths);
                if (infantCount == 0)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}门内眼下并无襁褓待护，先看婚议、承祧与丧服轻重。");
                }

                if (clan.SupportReserve < 4)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}宗房余力过浅，暂难另拨米药与乳养之费。");
                }

                FamilyLifecycleResolutionProfile profile = ResolveNewbornCareProfile(clan, infantCount);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - profile.SupportCost);
                clan.CareLoad = Math.Max(0, clan.CareLoad - profile.CareLoadRelief);
                clan.HeirSecurity = CommandResolutionMath.Clamp100(clan.HeirSecurity + profile.HeirSecurityLift);
                clan.ReproductivePressure = Math.Max(0, clan.ReproductivePressure - profile.ReproductivePressureRelief);
                clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension - profile.BranchTensionRelief + profile.BranchTensionBacklash);
                clan.RemedyConfidence = CommandResolutionMath.Clamp100(clan.RemedyConfidence + profile.RemedyConfidenceLift);
                clan.CharityObligation = CommandResolutionMath.Clamp100(clan.CharityObligation + profile.CharityObligationLift);
                clan.LastLifecycleOutcome = $"已拨米药护住产妇与襁褓；门内襁褓{infantCount}口，照料负担降到{clan.CareLoad}，承祧稳度升到{clan.HeirSecurity}，生育追压退到{clan.ReproductivePressure}，宗房余力余{clan.SupportReserve}。{RenderLifecycleBacklash(profile)}";
                clan.LastLifecycleTrace = $"{clan.ClanName}按{profile.ExecutionSummary}裁量护婴，先把米药、乳养与照看人手拨到近亲身边。";
                clan.LastLifecycleCommandCode = command.CommandName;
                clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(command.CommandName);
                simulation.RefreshReplayHash();

                return BuildAcceptedFamilyLifecycleResult(command, clan);
            }
            case PlayerCommandNames.SetMourningOrder:
            {
                if (clan.MourningLoad <= 0)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}门内暂无丧服之重，眼下不必另议丧次。");
                }

                FamilyLifecycleResolutionProfile profile = ResolveMourningOrderProfile(clan);
                clan.MourningLoad = Math.Max(0, clan.MourningLoad - profile.MourningLoadRelief);
                clan.FuneralDebt = Math.Max(0, clan.FuneralDebt - profile.FuneralDebtRelief);
                clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - profile.InheritancePressureRelief);
                clan.BranchTension = CommandResolutionMath.Clamp100(clan.BranchTension - profile.BranchTensionRelief + profile.BranchTensionBacklash);
                clan.MediationMomentum = CommandResolutionMath.Clamp100(clan.MediationMomentum + profile.MediationMomentumLift);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - profile.SupportCost);
                clan.LastLifecycleOutcome = $"丧次与祭次已定，丧服之重缓到{clan.MourningLoad}，丧葬拖欠降到{clan.FuneralDebt}，后议之压退到{clan.InheritancePressure}，宗房余力余{clan.SupportReserve}。{RenderLifecycleBacklash(profile)}";
                clan.LastLifecycleTrace = $"{clan.ClanName}按{profile.ExecutionSummary}裁定发引、祭次与服序，先让举哀、支用和承祧后议都有规矩可循。";
                clan.LastLifecycleCommandCode = command.CommandName;
                clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(command.CommandName);
                simulation.RefreshReplayHash();

                return BuildAcceptedFamilyLifecycleResult(command, clan);
            }
            case PlayerCommandNames.SupportSeniorBranch:
            {
                FamilyConflictResolutionProfile profile = ResolveSupportSeniorBranchProfile(clan);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "嫡支得护，旁支怨气渐起。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}偏护嫡支，祠堂中的分房与承祧之争随之更紧。";
                break;
            }
            case PlayerCommandNames.OrderFormalApology:
            {
                FamilyConflictResolutionProfile profile = ResolveFormalApologyProfile(clan);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "责成赔礼，祠堂争声暂缓。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}责令赔礼，先压祠堂口角与房支积怨。";
                break;
            }
            case PlayerCommandNames.PermitBranchSeparation:
            {
                FamilyConflictResolutionProfile profile = ResolveBranchSeparationProfile(clan);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "准其分房，旧账改作分门户账。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}准其分房，先将同火之争拆回两房自理。";
                break;
            }
            case PlayerCommandNames.SuspendClanRelief:
            {
                FamilyConflictResolutionProfile profile = ResolveSuspendReliefProfile(clan);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "停其接济，房支怨望转深。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}停其接济，祠下怨望与分房之议都更紧。";
                break;
            }
            case PlayerCommandNames.InviteClanEldersMediation:
            {
                FamilyConflictResolutionProfile profile = ResolveClanEldersMediationProfile(clan, publicBroker: false);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "请族老调停，堂议得以再开。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}请族老调停，先以长辈评断缓开祠堂争执。";
                break;
            }
            case PlayerCommandNames.InviteClanEldersPubliclyBroker:
            {
                FamilyConflictResolutionProfile profile = ResolveClanEldersMediationProfile(clan, publicBroker: true);
                ApplyFamilyConflictProfile(clan, profile);
                clan.LastConflictOutcome = "族老已出面压住街谈，先把堂外口舌与门前围观缓下来。";
                clan.LastConflictTrace = $"{clan.ClanName}按{profile.ExecutionSummary}请族老出面，在县门与街口先行解说，免得堂内争议扩成众口公议。";
                break;
            }
            default:
                return BuildRejectedFamilyResult(command, $"宗房不识此令：{command.CommandName}。");
        }

        clan.LastConflictCommandCode = command.CommandName;
        clan.LastConflictCommandLabel = command.CommandName == PlayerCommandNames.InviteClanEldersPubliclyBroker
            ? DeterminePublicLifeCommandLabel(command.CommandName)
            : DetermineFamilyCommandLabel(command.CommandName);
        simulation.RefreshReplayHash();

        return new PlayerCommandResult
        {
            Accepted = true,
            ModuleKey = KnownModuleKeys.FamilyCore,
            SurfaceKey = command.CommandName == PlayerCommandNames.InviteClanEldersPubliclyBroker
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

    private static IPersonRegistryQueries? TryGetPersonRegistryQueries(GameSimulation simulation)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.PersonRegistry))
        {
            return null;
        }

        try
        {
            return BuildQueries(simulation).GetRequired<IPersonRegistryQueries>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
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

    private static PlayerCommandResult BuildRejectedFamilyResult(PlayerCommandRequest command, string summary)
    {
        bool isPublicLife = string.Equals(command.CommandName, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal);
        return new PlayerCommandResult
        {
            Accepted = false,
            ModuleKey = KnownModuleKeys.FamilyCore,
            SurfaceKey = isPublicLife ? PlayerCommandSurfaceKeys.PublicLife : PlayerCommandSurfaceKeys.Family,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = isPublicLife ? DeterminePublicLifeCommandLabel(command.CommandName) : DetermineFamilyCommandLabel(command.CommandName),
            Summary = summary,
        };
    }

    internal static string DetermineFamilyCommandLabel(string commandName)
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
            _ => commandName,
        };
    }

}
