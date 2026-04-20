using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Persistence;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static IReadOnlyList<PlayerCommandAffordanceSnapshot> BuildPlayerCommandAffordances(PresentationReadModelBundle bundle)
    {
        List<PlayerCommandAffordanceSnapshot> affordances = new();
        Dictionary<int, ClanNarrativeSnapshot> narrativesByClan = bundle.ClanNarratives
            .ToDictionary(static narrative => narrative.ClanId.Value, static narrative => narrative);

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.ClanName, StringComparer.Ordinal))
        {
            ClanNarrativeSnapshot? narrative = narrativesByClan.TryGetValue(clan.Id.Value, out ClanNarrativeSnapshot? snapshot)
                ? snapshot
                : null;
            int grievancePressure = narrative?.GrudgePressure ?? 0;

            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SupportSeniorBranch,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SupportSeniorBranch),
                Summary = $"{clan.ClanName}可在祠堂先定嫡支体面与承祧次序，但旁支怨气会随之浮起。",
                IsEnabled = true,
                AvailabilitySummary = "此令可随时下达，但最易牵动房支偏怨。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.OrderFormalApology,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.OrderFormalApology),
                Summary = $"{clan.ClanName}可先责成赔礼，以压祠堂口角与旧怨。",
                IsEnabled = clan.BranchTension >= 18 || grievancePressure >= 20,
                AvailabilitySummary = clan.BranchTension >= 18 || grievancePressure >= 20
                    ? $"当前房支争势{clan.BranchTension}，适宜先压口角。"
                    : "眼下争声尚浅，赔礼之令未必需要。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.PermitBranchSeparation,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.PermitBranchSeparation),
                Summary = $"{clan.ClanName}可准旁支分房，以拆开同灶积怨与承祧旧账。",
                IsEnabled = clan.SeparationPressure >= 35 || clan.BranchTension >= 55,
                AvailabilitySummary = clan.SeparationPressure >= 35 || clan.BranchTension >= 55
                    ? $"分房之压{clan.SeparationPressure}，已有拆灶立门户之势。"
                    : "分房之议未炽，暂可留待后断。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SuspendClanRelief,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SuspendClanRelief),
                Summary = $"{clan.ClanName}可停其接济，以示宗房威断，但房支怨望会更深。",
                IsEnabled = clan.SupportReserve >= 8,
                AvailabilitySummary = clan.SupportReserve >= 8
                    ? $"宗房余力{clan.SupportReserve}，足可抽去接济。"
                    : "宗房余力浅薄，再停接济只会自伤。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersMediation,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.InviteClanEldersMediation),
                Summary = $"{clan.ClanName}可请族老调停，先让堂议有台阶可下。",
                IsEnabled = clan.BranchTension >= 20 || clan.SeparationPressure >= 20 || grievancePressure >= 20,
                AvailabilitySummary = clan.BranchTension >= 20 || clan.SeparationPressure >= 20 || grievancePressure >= 20
                    ? "争议已起，请族老最能先缓祠堂气口。"
                    : "当前祠堂争议未盛，暂不必惊动族老。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.ArrangeMarriage,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.ArrangeMarriage),
                Summary = $"{clan.ClanName}可先议亲定婚，借姻亲稳一稳香火、人情与房支后计。",
                IsEnabled = clan.MourningLoad < 18 && (clan.MarriageAlliancePressure >= 28 || clan.MarriageAllianceValue < 48),
                AvailabilitySummary = clan.MourningLoad >= 18
                    ? $"门内丧服未除，婚议暂宜后缓；丧服之重{clan.MourningLoad}。"
                    : $"婚议之压{clan.MarriageAlliancePressure}，姻亲可资之势{clan.MarriageAllianceValue}。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SupportNewbornCare,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SupportNewbornCare),
                Summary = $"{clan.ClanName}可先拨粮护婴，把产后调护、乳哺与襁褓衣食稳下来。",
                IsEnabled = clan.InfantCount > 0 && clan.SupportReserve >= 4,
                AvailabilitySummary = clan.InfantCount == 0
                    ? "门内暂无线褓幼儿，眼下无须另拨护婴之费。"
                    : clan.SupportReserve >= 4
                        ? $"门内现有襁褓{clan.InfantCount}口，宗房余力{clan.SupportReserve}。"
                        : $"门内现有襁褓{clan.InfantCount}口，但宗房余力{clan.SupportReserve}，一时难再加拨。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.DesignateHeirPolicy,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.DesignateHeirPolicy),
                Summary = $"{clan.ClanName}可先定承祧次序，把香火名分与后议先写稳。",
                IsEnabled = !clan.HeirPersonId.HasValue || clan.HeirSecurity < 60,
                AvailabilitySummary = !clan.HeirPersonId.HasValue
                    ? "堂上尚未举出承祧之人，宜先定后序。"
                    : $"承祧稳度{clan.HeirSecurity}，名分若虚仍易再起后议。",
                TargetLabel = clan.ClanName,
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.SetMourningOrder,
                Label = PlayerCommandService.DetermineFamilyCommandLabel(PlayerCommandNames.SetMourningOrder),
                Summary = $"{clan.ClanName}可先议定丧次与祭次，别让门内一边举哀一边再翻后议。",
                IsEnabled = clan.MourningLoad > 0,
                AvailabilitySummary = clan.MourningLoad > 0
                    ? $"门内丧服之重{clan.MourningLoad}，宜先定服序与支用。"
                    : "门内暂无举哀之事，眼下不必另议丧次。",
                TargetLabel = clan.ClanName,
            });
        }

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            bool canReviewPetitions = jurisdiction.PetitionBacklog > 0 || jurisdiction.PetitionPressure > 0;
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.OfficeAndCareer,
                SurfaceKey = PlayerCommandSurfaceKeys.Office,
                SettlementId = jurisdiction.SettlementId,
                CommandName = PlayerCommandNames.PetitionViaOfficeChannels,
                Label = PlayerCommandService.DetermineOfficeCommandLabel(PlayerCommandNames.PetitionViaOfficeChannels),
                Summary = $"{jurisdiction.LeadOfficialName}可在{jurisdiction.LeadOfficeTitle}任上先理词状，缓解积案与乡里怨气。",
                IsEnabled = canReviewPetitions,
                AvailabilitySummary = canReviewPetitions
                    ? $"积案{jurisdiction.PetitionBacklog}，可先批结一轮。"
                    : "本处暂无待批词状。",
            });
            affordances.Add(new PlayerCommandAffordanceSnapshot
            {
                ModuleKey = KnownModuleKeys.OfficeAndCareer,
                SurfaceKey = PlayerCommandSurfaceKeys.Office,
                SettlementId = jurisdiction.SettlementId,
                CommandName = PlayerCommandNames.DeployAdministrativeLeverage,
                Label = PlayerCommandService.DetermineOfficeCommandLabel(PlayerCommandNames.DeployAdministrativeLeverage),
                Summary = $"{jurisdiction.LeadOfficialName}可凭官箴与印信发签催办，先压急牍与拖延。",
                IsEnabled = jurisdiction.JurisdictionLeverage >= 12,
                AvailabilitySummary = jurisdiction.JurisdictionLeverage >= 12
                    ? $"乡面杠杆{jurisdiction.JurisdictionLeverage}，足可催动里甲与吏胥。"
                    : "此地官箴未足，不宜强行发签。",
            });
        }

        foreach (CampaignMobilizationSignalSnapshot signal in bundle.CampaignMobilizationSignals.OrderBy(static entry => entry.SettlementId.Value))
        {
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.DraftCampaignPlan,
                "先在案头筹议关津、驿报与前后队次，不急于放大边缘摩擦。",
                true,
                "此令偏向先定方略，不急发众。"));
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.CommitMobilization,
                "发檄点兵，先聚守丁、乡勇与护运之众，再定前压与驻防。",
                !string.Equals(signal.MobilizationWindowLabel, "Closed", StringComparison.Ordinal),
                string.Equals(signal.MobilizationWindowLabel, "Closed", StringComparison.Ordinal)
                    ? "当前动员窗已闭，不宜强起兵众。"
                    : $"当前动员窗为{RenderMobilizationWindow(signal.MobilizationWindowLabel)}。"));
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.ProtectSupplyLine,
                "催督粮道与渡口，优先保住转运、驿报与军前补给。",
                signal.AvailableForceCount > 0,
                signal.AvailableForceCount > 0
                    ? $"可调之众{signal.AvailableForceCount}。"
                    : "当前无可调之众。"));
            affordances.Add(BuildWarfareAffordance(
                signal,
                PlayerCommandNames.WithdrawToBarracks,
                "暂收行伍归营，整顿伤员、粮册与营中号令。",
                signal.AvailableForceCount > 0,
                signal.AvailableForceCount > 0
                    ? "可行收军归营之令。"
                    : "当前无营伍可收。"));
        }

        affordances.AddRange(BuildPublicLifeAffordances(bundle));
        return affordances;
    }

    private static PlayerCommandAffordanceSnapshot BuildWarfareAffordance(
        CampaignMobilizationSignalSnapshot signal,
        string commandName,
        string summary,
        bool isEnabled,
        string availabilitySummary)
    {
        return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
            SettlementId = signal.SettlementId,
            CommandName = commandName,
            Label = WarfareCampaignDescriptors.DetermineDirectiveLabel(commandName),
            Summary = summary,
            IsEnabled = isEnabled,
            AvailabilitySummary = availabilitySummary,
        };
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildPublicLifeAffordances(PresentationReadModelBundle bundle)
    {
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = bundle.OfficeJurisdictions
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);
        Dictionary<int, SettlementDisorderSnapshot> disorderBySettlement = bundle.SettlementDisorder
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);
        ILookup<int, ClanSnapshot> clansBySettlement = bundle.Clans.ToLookup(static entry => entry.HomeSettlementId.Value);

        foreach (SettlementPublicLifeSnapshot publicLife in bundle.PublicLifeSettlements.OrderBy(static entry => entry.SettlementId.Value))
        {
            jurisdictionsBySettlement.TryGetValue(publicLife.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);

            if (jurisdiction is not null)
            {
                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.PostCountyNotice),
                    Summary = $"{publicLife.NodeLabel}街谈已热，可先借榜示压住众口。",
                    IsEnabled = publicLife.StreetTalkHeat >= 40 || publicLife.PublicLegitimacy < 55,
                    AvailabilitySummary = $"榜示分量{publicLife.DocumentaryWeight}，由{jurisdiction.LeadOfficialName}主其晓谕。",
                    TargetLabel = publicLife.NodeLabel,
                };

                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    CommandName = PlayerCommandNames.DispatchRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.DispatchRoadReport),
                    Summary = $"{publicLife.DominantVenueLabel}消息往来已有迟滞，可先遣吏催报。",
                    IsEnabled = publicLife.RoadReportLag >= 36 || publicLife.CourierRisk >= 35,
                    AvailabilitySummary = $"递报险数{publicLife.CourierRisk}，查验周折{publicLife.VerificationCost}。",
                    TargetLabel = publicLife.DominantVenueLabel,
                };

            }

            if (disorderBySettlement.TryGetValue(publicLife.SettlementId.Value, out SettlementDisorderSnapshot? disorder))
            {
                OrderAdministrativeReachProfile administrativeReach = OrderAdministrativeReachEvaluator.Evaluate(jurisdiction);

                foreach (PlayerCommandAffordanceSnapshot affordance in BuildSupplementalOrderPublicLifeAffordances(publicLife, disorder, administrativeReach))
                {
                    yield return affordance;
                }

                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.OrderAndBanditry,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    CommandName = PlayerCommandNames.EscortRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.EscortRoadReport),
                    Summary = $"{publicLife.DominantVenueLabel}近来路情不稳，可先催护一路，保住津口与路报。",
                    IsEnabled = disorder.RoutePressure >= 28 || publicLife.CourierRisk >= 32,
                    AvailabilitySummary = $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                    ExecutionSummary = administrativeReach.ExecutionSummary,
                    TargetLabel = publicLife.DominantVenueLabel,
                };
            }

            ClanSnapshot? leadClan = clansBySettlement[publicLife.SettlementId.Value]
                .OrderByDescending(static entry => entry.Prestige)
                .ThenBy(static entry => entry.ClanName, StringComparer.Ordinal)
                .FirstOrDefault();
            if (leadClan is not null)
            {
                yield return new PlayerCommandAffordanceSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = publicLife.SettlementId,
                    ClanId = leadClan.Id,
                    CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.InviteClanEldersPubliclyBroker),
                    Summary = $"{leadClan.ClanName}可请族老先出面缓口，免得堂内家事扩成街谈公议。",
                    IsEnabled = publicLife.StreetTalkHeat >= 45 || publicLife.MarketRumorFlow >= 45,
                    AvailabilitySummary = $"街谈{publicLife.StreetTalkHeat}，市语流势{publicLife.MarketRumorFlow}。",
                    TargetLabel = leadClan.ClanName,
                };
            }
        }
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildSupplementalOrderPublicLifeAffordances(
        SettlementPublicLifeSnapshot publicLife,
        SettlementDisorderSnapshot disorder,
        OrderAdministrativeReachProfile administrativeReach)
    {
        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.FundLocalWatch,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.FundLocalWatch),
            Summary = $"{publicLife.DominantVenueLabel}近来脚路不稳，可先添雇巡丁，把路口与渡头补起来。",
            IsEnabled = disorder.RoutePressure >= 22 || disorder.DisorderPressure >= 24,
            AvailabilitySummary = $"路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.DominantVenueLabel,
        };

        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.SuppressBanditry,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.SuppressBanditry),
            Summary = $"{publicLife.NodeLabel}已见路匪踪迹，可先严缉，但后手报复也会更重。",
            IsEnabled = disorder.BanditThreat >= 36 || disorder.SuppressionDemand >= 32,
            AvailabilitySummary = $"盗压{disorder.BanditThreat}，镇压之需{disorder.SuppressionDemand}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.NodeLabel,
        };

        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.NegotiateWithOutlaws,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.NegotiateWithOutlaws),
            Summary = $"{publicLife.DominantVenueLabel}若先求一时通路，可遣人议路，换一段缓和。",
            IsEnabled = disorder.BanditThreat >= 24 || disorder.DisorderPressure >= 28,
            AvailabilitySummary = $"盗压{disorder.BanditThreat}，地面不靖{disorder.DisorderPressure}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.DominantVenueLabel,
        };

        yield return new PlayerCommandAffordanceSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = publicLife.SettlementId,
            CommandName = PlayerCommandNames.TolerateDisorder,
            Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.TolerateDisorder),
            Summary = $"{publicLife.NodeLabel}若眼下不宜再逼，也可先缓一缓穷追，把明面风声压住。",
            IsEnabled = disorder.BanditThreat >= 18 || disorder.RoutePressure >= 18 || disorder.DisorderPressure >= 18,
            AvailabilitySummary = $"盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            ExecutionSummary = administrativeReach.ExecutionSummary,
            TargetLabel = publicLife.NodeLabel,
        };
    }

    private static IEnumerable<PlayerCommandReceiptSnapshot> BuildPublicLifeReceipts(PresentationReadModelBundle bundle)
    {
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = bundle.OfficeJurisdictions
            .ToDictionary(static entry => entry.SettlementId.Value, static entry => entry);

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "张榜晓谕", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.PostCountyNotice),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }

            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "遣吏催报", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.DispatchRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.DispatchRoadReport),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            jurisdictionsBySettlement.TryGetValue(disorder.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
            PlayerCommandReceiptSnapshot? receipt = BuildOrderPublicLifeReceipt(disorder, jurisdiction);
            if (receipt is not null)
            {
                yield return receipt;
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (!disorder.LastPressureReason.Contains("催护一路", StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = disorder.SettlementId,
                CommandName = PlayerCommandNames.EscortRoadReport,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.EscortRoadReport),
                Summary = disorder.LastPressureReason,
                OutcomeSummary = $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                TargetLabel = disorder.SettlementId.Value.ToString(),
            };
        }

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.HomeSettlementId.Value))
        {
            if (!string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.InviteClanEldersPubliclyBroker),
                Summary = clan.LastConflictTrace,
                OutcomeSummary = clan.LastConflictOutcome,
                TargetLabel = clan.ClanName,
            };
        }
    }

    private static IEnumerable<PlayerCommandReceiptSnapshot> BuildPublicLifeReceiptsNormalized(PresentationReadModelBundle bundle)
    {
        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "张榜晓谕", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.PostCountyNotice,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.PostCountyNotice),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }

            if (string.Equals(jurisdiction.CurrentAdministrativeTask, "遣吏催报", StringComparison.Ordinal))
            {
                yield return new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.OfficeAndCareer,
                    SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                    SettlementId = jurisdiction.SettlementId,
                    CommandName = PlayerCommandNames.DispatchRoadReport,
                    Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.DispatchRoadReport),
                    Summary = jurisdiction.LastAdministrativeTrace,
                    OutcomeSummary = jurisdiction.LastPetitionOutcome,
                    TargetLabel = jurisdiction.LeadOfficialName,
                };
            }
        }

        foreach (SettlementDisorderSnapshot disorder in bundle.SettlementDisorder.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (!disorder.LastPressureReason.Contains("催护一路", StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.OrderAndBanditry,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = disorder.SettlementId,
                CommandName = PlayerCommandNames.EscortRoadReport,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.EscortRoadReport),
                Summary = disorder.LastPressureReason,
                OutcomeSummary = $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                TargetLabel = disorder.SettlementId.Value.ToString(),
            };
        }

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.HomeSettlementId.Value))
        {
            if (!string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.InviteClanEldersPubliclyBroker, StringComparison.Ordinal))
            {
                continue;
            }

            yield return new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = PlayerCommandNames.InviteClanEldersPubliclyBroker,
                Label = PlayerCommandService.DeterminePublicLifeCommandLabel(PlayerCommandNames.InviteClanEldersPubliclyBroker),
                Summary = clan.LastConflictTrace,
                OutcomeSummary = clan.LastConflictOutcome,
                TargetLabel = clan.ClanName,
            };
        }
    }

    private static PlayerCommandReceiptSnapshot? BuildOrderPublicLifeReceipt(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (string.IsNullOrWhiteSpace(disorder.LastInterventionCommandCode))
        {
            return null;
        }

        return new PlayerCommandReceiptSnapshot
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = disorder.SettlementId,
            CommandName = disorder.LastInterventionCommandCode,
            Label = string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
                ? PlayerCommandService.DeterminePublicLifeCommandLabel(disorder.LastInterventionCommandCode)
                : disorder.LastInterventionCommandLabel,
            Summary = disorder.LastInterventionSummary,
            OutcomeSummary = disorder.LastInterventionOutcome,
            ExecutionSummary = BuildOrderAdministrativeAftermathExecutionSummary(disorder, jurisdiction),
            TargetLabel = disorder.SettlementId.Value.ToString(),
        };
    }

    private static string BuildOrderAdministrativeAftermathExecutionSummary(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null
            || string.IsNullOrWhiteSpace(disorder.LastInterventionCommandLabel)
            || (string.IsNullOrWhiteSpace(jurisdiction.LastPetitionOutcome)
                && string.IsNullOrWhiteSpace(jurisdiction.LastAdministrativeTrace)))
        {
            return string.Empty;
        }

        bool linkedToOrderAftermath =
            jurisdiction.LastPetitionOutcome.Contains(disorder.LastInterventionCommandLabel, StringComparison.Ordinal)
            || jurisdiction.LastAdministrativeTrace.Contains(disorder.LastInterventionCommandLabel, StringComparison.Ordinal);
        if (!linkedToOrderAftermath)
        {
            return string.Empty;
        }

        string leadLabel = string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
            ? jurisdiction.LeadOfficeTitle
            : jurisdiction.LeadOfficialName;
        return $"{leadLabel}眼下仍在{jurisdiction.CurrentAdministrativeTask}；积案{jurisdiction.PetitionBacklog}，{jurisdiction.LastPetitionOutcome}";
    }


    private static IReadOnlyList<PlayerCommandReceiptSnapshot> BuildPlayerCommandReceipts(PresentationReadModelBundle bundle)
    {
        List<PlayerCommandReceiptSnapshot> receipts = new();

        foreach (ClanSnapshot clan in bundle.Clans.OrderBy(static entry => entry.ClanName, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(clan.LastConflictTrace)
                && string.IsNullOrWhiteSpace(clan.LastConflictOutcome)
                && string.IsNullOrWhiteSpace(clan.LastLifecycleTrace)
                && string.IsNullOrWhiteSpace(clan.LastLifecycleOutcome))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(clan.LastConflictTrace) || !string.IsNullOrWhiteSpace(clan.LastConflictOutcome))
            {
                receipts.Add(new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.FamilyCore,
                SurfaceKey = PlayerCommandSurfaceKeys.Family,
                SettlementId = clan.HomeSettlementId,
                ClanId = clan.Id,
                CommandName = string.IsNullOrWhiteSpace(clan.LastConflictCommandCode)
                    ? PlayerCommandNames.InviteClanEldersMediation
                    : clan.LastConflictCommandCode,
                Label = string.IsNullOrWhiteSpace(clan.LastConflictCommandLabel)
                    ? "祠堂议决"
                    : clan.LastConflictCommandLabel,
                Summary = clan.LastConflictTrace,
                OutcomeSummary = clan.LastConflictOutcome,
                TargetLabel = clan.ClanName,
            });
            }

            if (!string.IsNullOrWhiteSpace(clan.LastLifecycleTrace) || !string.IsNullOrWhiteSpace(clan.LastLifecycleOutcome))
            {
                receipts.Add(new PlayerCommandReceiptSnapshot
                {
                    ModuleKey = KnownModuleKeys.FamilyCore,
                    SurfaceKey = PlayerCommandSurfaceKeys.Family,
                    SettlementId = clan.HomeSettlementId,
                    ClanId = clan.Id,
                    CommandName = string.IsNullOrWhiteSpace(clan.LastLifecycleCommandCode)
                        ? PlayerCommandNames.ArrangeMarriage
                        : clan.LastLifecycleCommandCode,
                    Label = string.IsNullOrWhiteSpace(clan.LastLifecycleCommandLabel)
                        ? "门内后计"
                        : clan.LastLifecycleCommandLabel,
                    Summary = clan.LastLifecycleTrace,
                    OutcomeSummary = clan.LastLifecycleOutcome,
                    TargetLabel = clan.ClanName,
                });
            }
        }

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            if (string.IsNullOrWhiteSpace(jurisdiction.LastAdministrativeTrace) && string.IsNullOrWhiteSpace(jurisdiction.LastPetitionOutcome))
            {
                continue;
            }

            string commandName = string.Equals(jurisdiction.PetitionOutcomeCategory, "Granted", StringComparison.Ordinal)
                ? PlayerCommandNames.DeployAdministrativeLeverage
                : PlayerCommandNames.PetitionViaOfficeChannels;

            receipts.Add(new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.OfficeAndCareer,
                SurfaceKey = PlayerCommandSurfaceKeys.Office,
                SettlementId = jurisdiction.SettlementId,
                CommandName = commandName,
                Label = PlayerCommandService.DetermineOfficeCommandLabel(commandName),
                Summary = jurisdiction.LastAdministrativeTrace,
                OutcomeSummary = jurisdiction.LastPetitionOutcome,
            });
        }

        foreach (CampaignFrontSnapshot campaign in bundle.Campaigns.OrderBy(static entry => entry.CampaignId.Value))
        {
            if (string.IsNullOrWhiteSpace(campaign.ActiveDirectiveLabel)
                && string.IsNullOrWhiteSpace(campaign.LastDirectiveTrace))
            {
                continue;
            }

            receipts.Add(new PlayerCommandReceiptSnapshot
            {
                ModuleKey = KnownModuleKeys.WarfareCampaign,
                SurfaceKey = PlayerCommandSurfaceKeys.Warfare,
                SettlementId = campaign.AnchorSettlementId,
                CommandName = campaign.ActiveDirectiveCode,
                Label = campaign.ActiveDirectiveLabel,
                Summary = campaign.LastDirectiveTrace,
                OutcomeSummary = campaign.ActiveDirectiveSummary,
            });
        }

        receipts.AddRange(BuildPublicLifeReceipts(bundle));
        return receipts
            .GroupBy(static receipt => (
                receipt.ModuleKey,
                receipt.SurfaceKey,
                receipt.SettlementId,
                receipt.ClanId,
                receipt.CommandName,
                receipt.TargetLabel))
            .Select(static group => group.First())
            .ToList();
    }

    private static string RenderMobilizationWindow(string windowLabel)
    {
        return windowLabel switch
        {
            "Open" => "可开",
            "Narrow" => "可守",
            "Preparing" => "待整",
            _ => "已闭",
        };
    }

}
