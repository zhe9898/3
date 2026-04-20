using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;

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

#if false
        if (false && false && TryApplyOrderPublicLifeCommand(default!, command.CommandName))
        {
            simulation.RefreshReplayHash();
            return new PlayerCommandResult
            {
                Accepted = true,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = settlement.LastInterventionSummary,
                TargetLabel = $"据点 {command.SettlementId.Value}",
            };
        }

        if (false && TryApplyOrderPublicLifeCommand(default!, command.CommandName))
        {
            simulation.RefreshReplayHash();
            return new PlayerCommandResult
            {
                Accepted = true,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = settlement.LastInterventionSummary,
                TargetLabel = $"据点 {command.SettlementId.Value}",
            };
        }

#endif
        switch (command.CommandName)
        {
            case PlayerCommandNames.ArrangeMarriage:
            {
                bool hasMarriageableAdult = state.People.Any(person =>
                    person.ClanId == clan.Id
                    && person.IsAlive
                    && person.AgeMonths >= 16 * 12
                    && person.AgeMonths < 40 * 12);
                if (!hasMarriageableAdult)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}眼下无可议亲之人，先看家内年龄与服丧轻重。");
                }

                clan.MarriageAllianceValue = Math.Clamp(Math.Max(clan.MarriageAllianceValue, 28) + 30, 0, 100);
                clan.MarriageAlliancePressure = Math.Max(0, clan.MarriageAlliancePressure - 22);
                clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 10, 0, 100);
                clan.HeirSecurity = Math.Clamp(clan.HeirSecurity + 8, 0, 100);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - 3);
                clan.LastLifecycleOutcome = $"婚议已定，先借姻亲稳住香火与门面；婚议之压缓到{clan.MarriageAlliancePressure}，承祧稳度起到{clan.HeirSecurity}，只是聘财与往来之费已压上案头。";
                clan.LastLifecycleTrace = $"{clan.ClanName}奉命议亲，已把媒妁往来、聘财轻重与堂内脸面一并料理，先借姻亲缓一缓承祧与分房后议。";
                clan.LastLifecycleCommandCode = command.CommandName;
                clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(command.CommandName);
                simulation.RefreshReplayHash();

                return BuildAcceptedFamilyLifecycleResult(command, clan);
            }
            case PlayerCommandNames.DesignateHeirPolicy:
            {
                FamilyPersonState? candidate = state.People
                    .Where(person => person.ClanId == clan.Id && person.IsAlive)
                    .OrderByDescending(static person => person.AgeMonths >= 16 * 12)
                    .ThenByDescending(static person => person.AgeMonths)
                    .ThenBy(static person => person.Id.Value)
                    .FirstOrDefault();
                if (candidate is null)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}门内暂无人可立嗣，先看婚议与抚育能否续上。");
                }

                clan.HeirPersonId = candidate.Id;
                clan.HeirSecurity = Math.Clamp(Math.Max(clan.HeirSecurity, candidate.AgeMonths >= 16 * 12 ? 62 : 36), 0, 100);
                clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - 12);
                clan.BranchTension = Math.Clamp(clan.BranchTension + 3, 0, 100);
                clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + 2, 0, 100);
                clan.LastLifecycleOutcome = candidate.AgeMonths >= 16 * 12
                    ? $"承祧人已定，谱内名分先写稳了；承祧稳度{clan.HeirSecurity}，后议之压暂退到{clan.InheritancePressure}，只是诸房眼色未必尽平。"
                    : $"嗣苗已记入谱案，香火名分暂有着落；只是人尚年幼，承祧稳度只到{clan.HeirSecurity}，堂上仍得分心护持。";
                clan.LastLifecycleTrace = candidate.AgeMonths >= 16 * 12
                    ? $"{clan.ClanName}已将承祧次序议定入谱，先把香火名分写明，免得诸房借机翻后议。"
                    : $"{clan.ClanName}已先把嗣苗记入谱案，虽未成人，堂上总算先把香火名分定住。";
                clan.LastLifecycleCommandCode = command.CommandName;
                clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(command.CommandName);
                simulation.RefreshReplayHash();

                return BuildAcceptedFamilyLifecycleResult(command, clan);
            }
            case PlayerCommandNames.SupportNewbornCare:
            {
                int infantCount = state.People.Count(person =>
                    person.ClanId == clan.Id
                    && person.IsAlive
                    && person.AgeMonths <= FamilyInfantAgeMonths);
                if (infantCount == 0)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}门内眼下并无襁褓待护，先看婚议、承祧与丧服轻重。");
                }

                if (clan.SupportReserve < 4)
                {
                    return BuildRejectedFamilyResult(command, $"{clan.ClanName}宗房余力过浅，暂难另拨米药与乳养之费。");
                }

                clan.SupportReserve = Math.Max(0, clan.SupportReserve - 4);
                clan.HeirSecurity = Math.Clamp(clan.HeirSecurity + 6, 0, 100);
                clan.ReproductivePressure = Math.Max(0, clan.ReproductivePressure - 5);
                clan.BranchTension = Math.Max(0, clan.BranchTension - 2);
                clan.LastLifecycleOutcome = $"已拨米药护住产妇与襁褓；门内现有襁褓{infantCount}口，承祧稳度升到{clan.HeirSecurity}，只是宗房余力也减到{clan.SupportReserve}。";
                clan.LastLifecycleTrace = $"{clan.ClanName}已先从宗房拨出养口与看护之资，把产后调护、乳哺与襁褓衣食先稳下来。";
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

                clan.MourningLoad = Math.Max(0, clan.MourningLoad - 12);
                clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - 6);
                clan.BranchTension = Math.Max(0, clan.BranchTension - 3);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - 2);
                clan.LastLifecycleOutcome = $"丧次与祭次已定，门内丧服之重缓到{clan.MourningLoad}，后议之压退到{clan.InheritancePressure}，只是祭需与支用仍在消磨宗房余力。";
                clan.LastLifecycleTrace = $"{clan.ClanName}已把发引、祭次与服序议定，先让门内举哀有了规矩，不致一边办丧一边再起争执。";
                clan.LastLifecycleCommandCode = command.CommandName;
                clan.LastLifecycleCommandLabel = DetermineFamilyCommandLabel(command.CommandName);
                simulation.RefreshReplayHash();

                return BuildAcceptedFamilyLifecycleResult(command, clan);
            }
            case PlayerCommandNames.SupportSeniorBranch:
                clan.BranchFavorPressure = Math.Clamp(clan.BranchFavorPressure + 18, 0, 100);
                clan.BranchTension = Math.Clamp(clan.BranchTension + 10, 0, 100);
                clan.InheritancePressure = Math.Clamp(clan.InheritancePressure + 6, 0, 100);
                clan.MediationMomentum = Math.Max(0, clan.MediationMomentum - 4);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - 2);
                clan.LastConflictOutcome = "嫡支得护，旁支怨气渐起。";
                clan.LastConflictTrace = $"{clan.ClanName}奉命偏护嫡支，祠堂中的分房与承祧之争随之更紧。";
                break;
            case PlayerCommandNames.OrderFormalApology:
                clan.BranchTension = Math.Max(0, clan.BranchTension - 12);
                clan.SeparationPressure = Math.Max(0, clan.SeparationPressure - 6);
                clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + 10, 0, 100);
                clan.BranchFavorPressure = Math.Max(0, clan.BranchFavorPressure - 4);
                clan.LastConflictOutcome = "责成赔礼，祠堂争声暂缓。";
                clan.LastConflictTrace = $"{clan.ClanName}已责令赔礼，先压祠堂口角与房支积怨。";
                break;
            case PlayerCommandNames.PermitBranchSeparation:
                clan.SeparationPressure = Math.Max(0, clan.SeparationPressure - 20);
                clan.BranchTension = Math.Max(0, clan.BranchTension - 6);
                clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - 8);
                clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + 4, 0, 100);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - 8);
                clan.ReliefSanctionPressure = Math.Max(0, clan.ReliefSanctionPressure - 6);
                clan.LastConflictOutcome = "准其分房，旧账改作分门户账。";
                clan.LastConflictTrace = $"{clan.ClanName}已准分房，先将同火之争拆回两房自理。";
                break;
            case PlayerCommandNames.SuspendClanRelief:
                clan.ReliefSanctionPressure = Math.Clamp(clan.ReliefSanctionPressure + 18, 0, 100);
                clan.BranchTension = Math.Clamp(clan.BranchTension + 9, 0, 100);
                clan.SeparationPressure = Math.Clamp(clan.SeparationPressure + 7, 0, 100);
                clan.MediationMomentum = Math.Max(0, clan.MediationMomentum - 4);
                clan.SupportReserve = Math.Clamp(clan.SupportReserve + 4, 0, 100);
                clan.LastConflictOutcome = "停其接济，房支怨望转深。";
                clan.LastConflictTrace = $"{clan.ClanName}已停其接济，祠下怨望与分房之议都更紧。";
                break;
            case PlayerCommandNames.InviteClanEldersMediation:
                clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + 18, 0, 100);
                clan.BranchTension = Math.Max(0, clan.BranchTension - 8);
                clan.SeparationPressure = Math.Max(0, clan.SeparationPressure - 5);
                clan.BranchFavorPressure = Math.Max(0, clan.BranchFavorPressure - 6);
                clan.InheritancePressure = Math.Max(0, clan.InheritancePressure - 2);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - 2);
                clan.LastConflictOutcome = "请族老调停，堂议得以再开。";
                clan.LastConflictTrace = $"{clan.ClanName}已请族老调停，先以长辈评断缓开祠堂争执。";
                break;
            case PlayerCommandNames.InviteClanEldersPubliclyBroker:
                clan.MediationMomentum = Math.Clamp(clan.MediationMomentum + 14, 0, 100);
                clan.BranchTension = Math.Max(0, clan.BranchTension - 6);
                clan.SeparationPressure = Math.Max(0, clan.SeparationPressure - 4);
                clan.BranchFavorPressure = Math.Max(0, clan.BranchFavorPressure - 4);
                clan.SupportReserve = Math.Max(0, clan.SupportReserve - 3);
                clan.LastConflictOutcome = "族老已出面压住街谈，先把堂外口舌与门前围观缓下来。";
                clan.LastConflictTrace = $"{clan.ClanName}已请族老出面，在县门与街口先行解说，免得堂内争议扩成众口公议。";
                break;
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
