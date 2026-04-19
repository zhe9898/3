using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;

namespace Zongzu.Application;

public sealed class PlayerCommandService
{
    private const int FamilyInfantAgeMonths = 2 * 12;

    private readonly WarfareCampaignCommandService _warfareCampaignCommandService = new();

    public PlayerCommandResult IssueIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(command);

        return command.CommandName switch
        {
            PlayerCommandNames.ArrangeMarriage
                or PlayerCommandNames.DesignateHeirPolicy
                or PlayerCommandNames.SupportNewbornCare
                or PlayerCommandNames.SetMourningOrder
                or PlayerCommandNames.SupportSeniorBranch
                or PlayerCommandNames.OrderFormalApology
                or PlayerCommandNames.PermitBranchSeparation
                or PlayerCommandNames.SuspendClanRelief
                or PlayerCommandNames.InviteClanEldersMediation
                or PlayerCommandNames.InviteClanEldersPubliclyBroker
                => IssueFamilyIntent(simulation, command),
            PlayerCommandNames.PetitionViaOfficeChannels
                or PlayerCommandNames.DeployAdministrativeLeverage
                or PlayerCommandNames.PostCountyNotice
                or PlayerCommandNames.DispatchRoadReport
                => IssueOfficeIntent(simulation, command),
            PlayerCommandNames.EscortRoadReport
                or PlayerCommandNames.FundLocalWatch
                or PlayerCommandNames.SuppressBanditry
                or PlayerCommandNames.NegotiateWithOutlaws
                or PlayerCommandNames.TolerateDisorder
                => IssueExpandedOrderIntent(simulation, command),
            PlayerCommandNames.DraftCampaignPlan
                or PlayerCommandNames.CommitMobilization
                or PlayerCommandNames.ProtectSupplyLine
                or PlayerCommandNames.WithdrawToBarracks
                => IssueWarfareIntent(simulation, command),
            _ => new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = string.Empty,
                SurfaceKey = string.Empty,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = command.CommandName,
                Summary = $"Unknown player command: {command.CommandName}.",
            },
        };
    }

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

    private static PlayerCommandResult IssueOfficeIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            return BuildRejectedOfficeResult(command, "当前存档未启用官署权柄。");
        }

        OfficeAndCareerState state = simulation.GetMutableModuleState<OfficeAndCareerState>(KnownModuleKeys.OfficeAndCareer);
        OfficeCareerState[] careers = state.People
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

        state.Jurisdictions = OfficeAndCareerStateProjection.BuildJurisdictions(state.People);
        simulation.RefreshReplayHash();

        bool isPublicLifeCommand = IsPublicLifeOfficeCommand(command.CommandName);
        string label = isPublicLifeCommand
            ? DeterminePublicLifeCommandLabel(command.CommandName)
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

    private static PlayerCommandResult IssueExpandedOrderIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry))
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = "当前存档未启用地方治安与护路。",
            };
        }

        OrderAndBanditryState state = simulation.GetMutableModuleState<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState? settlement = state.Settlements.SingleOrDefault(entry => entry.SettlementId == command.SettlementId);
        if (settlement is null)
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = $"此地暂无可调度的路面与治安节制：{command.SettlementId.Value}。",
            };
        }

        OrderAdministrativeReachProfile administrativeReach = OrderAdministrativeReachEvaluator.Resolve(simulation, command.SettlementId);

        if (!TryApplyOrderPublicLifeCommand(settlement, command.CommandName, administrativeReach))
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = $"地方治安不识此令：{command.CommandName}。",
            };
        }

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

    private static PlayerCommandResult IssueOrderIntentLegacy(GameSimulation simulation, PlayerCommandRequest command)
    {
        return IssueExpandedOrderIntent(simulation, command);

#if false
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OrderAndBanditry))
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = "当前存档未启用地方巡缉与护路。",
            };
        }

        OrderAndBanditryState state = simulation.GetMutableModuleState<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        SettlementDisorderState? settlement = state.Settlements.SingleOrDefault(entry => entry.SettlementId == command.SettlementId);
        if (settlement is null)
        {
            return new PlayerCommandResult
            {
                Accepted = false,
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = command.SettlementId,
                ClanId = command.ClanId,
                CommandName = command.CommandName,
                Label = DeterminePublicLifeCommandLabel(command.CommandName),
                Summary = $"此地暂无线路可护：{command.SettlementId.Value}。",
            };
        }

        switch (command.CommandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 8);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 5);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.LastPressureReason = "已催护一路，先顾津口驿报与往来行货。";
                break;
            default:
                return new PlayerCommandResult
                {
                    Accepted = false,
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = command.SettlementId,
                    ClanId = command.ClanId,
                    CommandName = command.CommandName,
                    Label = DeterminePublicLifeCommandLabel(command.CommandName),
                    Summary = $"地方巡级不识此令：{command.CommandName}。",
                };
        }

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
            Summary = settlement.LastPressureReason,
            TargetLabel = $"据点 {command.SettlementId.Value}",
        };
    }

#endif
    }

    private static OrderAdministrativeReachProfile ResolveOrderAdministrativeReach(GameSimulation simulation, SettlementId settlementId)
    {
        if (!simulation.FeatureManifest.IsEnabled(KnownModuleKeys.OfficeAndCareer))
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        QueryRegistry queries = BuildQueries(simulation);
        IOfficeAndCareerQueries officeQueries;
        try
        {
            officeQueries = queries.GetRequired<IOfficeAndCareerQueries>();
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        JurisdictionAuthoritySnapshot jurisdiction;
        try
        {
            jurisdiction = officeQueries.GetRequiredJurisdiction(settlementId);
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        int supportSignal =
            jurisdiction.JurisdictionLeverage
            + jurisdiction.ClerkDependence
            + (jurisdiction.AuthorityTier * 10);
        int frictionSignal =
            jurisdiction.AdministrativeTaskLoad
            + jurisdiction.PetitionPressure
            + (jurisdiction.PetitionBacklog / 2);
        int netSignal = supportSignal - frictionSignal;

        if (netSignal >= 40)
        {
            return new OrderAdministrativeReachProfile(3, 5, -4, -3, "县署肯出手，文移与差役都跟得上。");
        }

        if (netSignal >= 15)
        {
            return new OrderAdministrativeReachProfile(1, 2, -2, -1, "县署还能接得住，文移差役尚能随令。");
        }

        if (netSignal <= -40)
        {
            return new OrderAdministrativeReachProfile(-3, -5, 4, 3, "县署拥案未解，文移不畅，路上只得勉强敷衍。");
        }

        if (netSignal <= -15)
        {
            return new OrderAdministrativeReachProfile(-1, -2, 2, 1, "县署案牍偏重，差役跟得慢，只能先做半套。");
        }

        return OrderAdministrativeReachProfile.Neutral;
    }

    private static QueryRegistry BuildQueries(GameSimulation simulation)
    {
        QueryRegistry queries = new();
        foreach (IModuleRunner module in simulation.Modules
                     .OrderBy(static module => module.Phase)
                     .ThenBy(static module => module.ExecutionOrder)
                     .ThenBy(static module => module.ModuleKey, StringComparer.Ordinal))
        {
            if (!simulation.FeatureManifest.IsEnabled(module.ModuleKey))
            {
                continue;
            }

            if (!simulation.TryGetModuleState(module.ModuleKey, out object? state) || state is null)
            {
                throw new InvalidOperationException($"Enabled module {module.ModuleKey} has no state for player-command queries.");
            }

            module.RegisterQueries(state, queries);
        }

        return queries;
    }

    private static bool TryApplyOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        string commandName,
        OrderAdministrativeReachProfile administrativeReach)
    {
        if (administrativeReach.HasModifier)
        {
            return TryApplyOfficeAwareOrderPublicLifeCommand(settlement, commandName, administrativeReach);
        }

        switch (commandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 8);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 5);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + 12, 0, 100);
                settlement.ImplementationDrag = Math.Max(0, settlement.ImplementationDrag - 4);
                settlement.LastPressureReason = "已先护住路报往来，河埠脚路暂得照看。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已催护一路，先把沿途报信与货脚照看起来。",
                    $"路压缓到{settlement.RoutePressure}，镇压之需减到{settlement.SuppressionDemand}，护路得力升到{settlement.RouteShielding}。");
                return true;
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 3);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 10);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 4);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 5);
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + 16, 0, 100);
                settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel + 1, 0, 12);
                settlement.LastPressureReason = "已添雇巡丁，先把路口、渡头与夜巡补起来。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已添雇巡丁，先补路口、渡头与夜巡人手。",
                    $"路压缓到{settlement.RoutePressure}，地面不靖退到{settlement.DisorderPressure}，护路得力升到{settlement.RouteShielding}。");
                return true;
            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 12);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 6);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 8);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 4);
                settlement.CoercionRisk = Math.Clamp(settlement.CoercionRisk + 8, 0, 100);
                settlement.RetaliationRisk = Math.Clamp(settlement.RetaliationRisk + 14, 0, 100);
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + 2, 0, 12);
                settlement.LastPressureReason = "已严缉路匪，先压住明面扰动，但后手报复也会跟着抬头。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已严缉路匪，先压明面匪踪与拦路生事。",
                    $"盗压降到{settlement.BanditThreat}，路压降到{settlement.RoutePressure}，但反噬险已升到{settlement.RetaliationRisk}。");
                return true;
            case PlayerCommandNames.NegotiateWithOutlaws:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - 4);
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - 3);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 10);
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - 2);
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + 6, 0, 100);
                settlement.CoercionRisk = Math.Max(0, settlement.CoercionRisk - 6);
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - 12);
                settlement.LastPressureReason = "已遣人议路，先换一路暂安，但私下分流会更容易坐大。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已遣人议路，先换渡头与路口一时安静。",
                    $"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但私路压抬到{settlement.BlackRoutePressure}。");
                return true;
            case PlayerCommandNames.TolerateDisorder:
                settlement.BanditThreat = Math.Clamp(settlement.BanditThreat + 4, 0, 100);
                settlement.RoutePressure = Math.Clamp(settlement.RoutePressure + 6, 0, 100);
                settlement.DisorderPressure = Math.Clamp(settlement.DisorderPressure + 5, 0, 100);
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + 8, 0, 100);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - 6);
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - 8);
                settlement.RouteShielding = Math.Max(0, settlement.RouteShielding - 4);
                settlement.LastPressureReason = "已暂缓穷追，先不把地面逼到立刻翻脸，但路面与私下手脚会继续滋长。";
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    "已暂缓穷追，先把差役与地面都收一收。",
                    $"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但路压涨到{settlement.RoutePressure}，私路压涨到{settlement.BlackRoutePressure}。");
                return true;
            default:
                return false;
        }
    }

    private static bool TryApplyOfficeAwareOrderPublicLifeCommand(
        SettlementDisorderState settlement,
        string commandName,
        OrderAdministrativeReachProfile administrativeReach)
    {
        switch (commandName)
        {
            case PlayerCommandNames.EscortRoadReport:
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(5, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(2, administrativeReach.BenefitShift, 0));
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + AdjustIncrease(12, administrativeReach.ShieldingShift), 0, 100);
                settlement.ImplementationDrag = Math.Max(0, settlement.ImplementationDrag - AdjustReduction(4, Math.Max(0, administrativeReach.BenefitShift)));
                settlement.LastPressureReason = AppendReachSummary("已先护住路报往来，河埠脚路暂得照看。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已催护一路，先把沿途报信与货脚照看起来。", administrativeReach),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，镇压之需减到{settlement.SuppressionDemand}，护路得力升到{settlement.RouteShielding}。", administrativeReach));
                return true;
            case PlayerCommandNames.FundLocalWatch:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(3, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(10, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(5, administrativeReach.BenefitShift));
                settlement.RouteShielding = Math.Clamp(settlement.RouteShielding + AdjustIncrease(16, administrativeReach.ShieldingShift), 0, 100);
                settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel + 1, 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已添雇巡丁，先把路口、渡头与夜巡补起来。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已添雇巡丁，先补路口、渡头与夜巡人手。", administrativeReach),
                    AppendReachSummary($"路压缓到{settlement.RoutePressure}，地面不靖退到{settlement.DisorderPressure}，护路得力升到{settlement.RouteShielding}。", administrativeReach));
                return true;
            case PlayerCommandNames.SuppressBanditry:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(12, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.CoercionRisk = Math.Clamp(settlement.CoercionRisk + AdjustIncrease(8, administrativeReach.BacklashShift), 0, 100);
                settlement.RetaliationRisk = Math.Clamp(settlement.RetaliationRisk + AdjustIncrease(14, administrativeReach.BacklashShift), 0, 100);
                settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief + AdjustIncrease(2, Math.Max(0, administrativeReach.BenefitShift)), 0, 12);
                settlement.LastPressureReason = AppendReachSummary("已严缉路匪，先压住明面扰动，但后手报复也会跟着抬头。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已严缉路匪，先压明面匪踪与拦路生事。", administrativeReach),
                    AppendReachSummary($"盗压降到{settlement.BanditThreat}，路压降到{settlement.RoutePressure}，但反噬险已升到{settlement.RetaliationRisk}。", administrativeReach));
                return true;
            case PlayerCommandNames.NegotiateWithOutlaws:
                settlement.BanditThreat = Math.Max(0, settlement.BanditThreat - AdjustReduction(4, administrativeReach.BenefitShift));
                settlement.RoutePressure = Math.Max(0, settlement.RoutePressure - AdjustReduction(3, administrativeReach.BenefitShift));
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(10, administrativeReach.BenefitShift));
                settlement.DisorderPressure = Math.Max(0, settlement.DisorderPressure - AdjustReduction(2, administrativeReach.BenefitShift, 0));
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + AdjustIncrease(6, administrativeReach.LeakageShift), 0, 100);
                settlement.CoercionRisk = Math.Max(0, settlement.CoercionRisk - AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - AdjustReduction(12, administrativeReach.BenefitShift));
                settlement.LastPressureReason = AppendReachSummary("已遣人议路，先换一路暂安，但私下分流会更容易坐大。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已遣人议路，先换渡头与路口一时安静。", administrativeReach),
                    AppendReachSummary($"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但私路压抬到{settlement.BlackRoutePressure}。", administrativeReach));
                return true;
            case PlayerCommandNames.TolerateDisorder:
                settlement.BanditThreat = Math.Clamp(settlement.BanditThreat + AdjustIncrease(4, administrativeReach.LeakageShift), 0, 100);
                settlement.RoutePressure = Math.Clamp(settlement.RoutePressure + AdjustIncrease(6, administrativeReach.LeakageShift), 0, 100);
                settlement.DisorderPressure = Math.Clamp(settlement.DisorderPressure + AdjustIncrease(5, administrativeReach.LeakageShift), 0, 100);
                settlement.BlackRoutePressure = Math.Clamp(settlement.BlackRoutePressure + AdjustIncrease(8, administrativeReach.LeakageShift), 0, 100);
                settlement.SuppressionDemand = Math.Max(0, settlement.SuppressionDemand - AdjustReduction(6, administrativeReach.BenefitShift));
                settlement.RetaliationRisk = Math.Max(0, settlement.RetaliationRisk - AdjustReduction(8, administrativeReach.BenefitShift));
                settlement.RouteShielding = Math.Max(0, settlement.RouteShielding - Math.Max(0, 4 - Math.Max(0, administrativeReach.BenefitShift)));
                settlement.LastPressureReason = AppendReachSummary("已暂缓穷追，先不把地面逼到立刻翻脸，但路面与私下手脚会继续滋长。", administrativeReach);
                ApplyOrderInterventionReceipt(
                    settlement,
                    commandName,
                    AppendReachSummary("已暂缓穷追，先把差役与地面都收一收。", administrativeReach),
                    AppendReachSummary($"镇压之需退到{settlement.SuppressionDemand}，反噬险降到{settlement.RetaliationRisk}，但路压涨到{settlement.RoutePressure}，私路压涨到{settlement.BlackRoutePressure}。", administrativeReach));
                return true;
            default:
                return false;
        }
    }

    private static int AdjustReduction(int baseValue, int shift, int minimum = 1)
    {
        return Math.Max(minimum, baseValue + shift);
    }

    private static int AdjustIncrease(int baseValue, int shift)
    {
        return Math.Max(0, baseValue + shift);
    }

    private static string AppendReachSummary(string text, OrderAdministrativeReachProfile administrativeReach)
    {
        if (string.IsNullOrWhiteSpace(administrativeReach.SummaryTail))
        {
            return text;
        }

        return $"{text}{administrativeReach.SummaryTail}";
    }

    private static void ApplyOrderInterventionReceipt(
        SettlementDisorderState settlement,
        string commandName,
        string summary,
        string outcome)
    {
        settlement.LastInterventionCommandCode = commandName;
        settlement.LastInterventionCommandLabel = DeterminePublicLifeCommandLabel(commandName);
        settlement.LastInterventionSummary = summary;
        settlement.LastInterventionOutcome = outcome;
        settlement.InterventionCarryoverMonths = 1;
    }

    private PlayerCommandResult IssueWarfareIntent(GameSimulation simulation, PlayerCommandRequest command)
    {
        WarfareCampaignIntentResult warfareResult = _warfareCampaignCommandService.IssueIntent(
            simulation,
            new WarfareCampaignIntentCommand
            {
                SettlementId = command.SettlementId,
                CommandName = command.CommandName,
            });

        return new PlayerCommandResult
        {
            Accepted = warfareResult.Accepted,
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = warfareResult.DirectiveLabel,
            Summary = warfareResult.Summary,
            TargetLabel = $"据点 {command.SettlementId.Value}",
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
            Label = isPublicLife ? DeterminePublicLifeCommandLabel(command.CommandName) : DetermineOfficeCommandLabel(command.CommandName),
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

    internal static string DetermineOfficeCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.PetitionViaOfficeChannels => "批覆词状",
            PlayerCommandNames.DeployAdministrativeLeverage => "发签催办",
            _ => commandName,
        };
    }

    internal static string DeterminePublicLifeCommandLabel(string commandName)
    {
        if (string.Equals(commandName, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal))
        {
            return "添雇巡丁";
        }

        if (string.Equals(commandName, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal))
        {
            return "严缉路匪";
        }

        if (string.Equals(commandName, PlayerCommandNames.NegotiateWithOutlaws, StringComparison.Ordinal))
        {
            return "遣人议路";
        }

        if (string.Equals(commandName, PlayerCommandNames.TolerateDisorder, StringComparison.Ordinal))
        {
            return "暂缓穷追";
        }

        return commandName switch
        {
            PlayerCommandNames.PostCountyNotice => "张榜晓谕",
            PlayerCommandNames.DispatchRoadReport => "遣吏催报",
            PlayerCommandNames.EscortRoadReport => "催护一路",
            PlayerCommandNames.InviteClanEldersPubliclyBroker => "请族老出面",
            _ => commandName,
        };
    }

    private static bool IsPublicLifeOfficeCommand(string commandName)
    {
        return string.Equals(commandName, PlayerCommandNames.PostCountyNotice, StringComparison.Ordinal)
            || string.Equals(commandName, PlayerCommandNames.DispatchRoadReport, StringComparison.Ordinal);
    }
}
