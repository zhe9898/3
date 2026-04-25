using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OrderAndBanditry;
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

            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.SupportSeniorBranch,
                clan.HomeSettlementId,
                $"{clan.ClanName}可在祠堂先定嫡支体面与承祧次序，但旁支怨气会随之浮起。",
                true,
                "此令可随时下达，但最易牵动房支偏怨。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.OrderFormalApology,
                clan.HomeSettlementId,
                $"{clan.ClanName}可先责成赔礼，以压祠堂口角与旧怨。",
                clan.BranchTension >= 18 || grievancePressure >= 20,
                clan.BranchTension >= 18 || grievancePressure >= 20
                    ? $"当前房支争势{clan.BranchTension}，适宜先压口角。"
                    : "眼下争声尚浅，赔礼之令未必需要。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.PermitBranchSeparation,
                clan.HomeSettlementId,
                $"{clan.ClanName}可准旁支分房，以拆开同灶积怨与承祧旧账。",
                clan.SeparationPressure >= 35 || clan.BranchTension >= 55,
                clan.SeparationPressure >= 35 || clan.BranchTension >= 55
                    ? $"分房之压{clan.SeparationPressure}，已有拆灶立门户之势。"
                    : "分房之议未炽，暂可留待后断。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.SuspendClanRelief,
                clan.HomeSettlementId,
                $"{clan.ClanName}可停其接济，以示宗房威断，但房支怨望会更深。",
                clan.SupportReserve >= 8,
                clan.SupportReserve >= 8
                    ? $"宗房余力{clan.SupportReserve}，足可抽去接济。"
                    : "宗房余力浅薄，再停接济只会自伤。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.InviteClanEldersMediation,
                clan.HomeSettlementId,
                $"{clan.ClanName}可请族老调停，先让堂议有台阶可下。",
                clan.BranchTension >= 20 || clan.SeparationPressure >= 20 || grievancePressure >= 20,
                clan.BranchTension >= 20 || clan.SeparationPressure >= 20 || grievancePressure >= 20
                    ? "争议已起，请族老最能先缓祠堂气口。"
                    : "当前祠堂争议未盛，暂不必惊动族老。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.ArrangeMarriage,
                clan.HomeSettlementId,
                $"{clan.ClanName}可先议亲定婚，借姻亲稳一稳香火、人情与房支后计。",
                clan.MourningLoad < 18 && (clan.MarriageAlliancePressure >= 28 || clan.MarriageAllianceValue < 48),
                clan.MourningLoad >= 18
                    ? $"门内丧服未除，婚议暂宜后缓；丧服之重{clan.MourningLoad}。"
                    : $"婚议之压{clan.MarriageAlliancePressure}，姻亲可资之势{clan.MarriageAllianceValue}。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.SupportNewbornCare,
                clan.HomeSettlementId,
                $"{clan.ClanName}可先拨粮护婴，把产后调护、乳哺与襁褓衣食稳下来。",
                clan.InfantCount > 0 && clan.SupportReserve >= 4,
                clan.InfantCount == 0
                    ? "门内暂无线褓幼儿，眼下无须另拨护婴之费。"
                    : clan.SupportReserve >= 4
                        ? $"门内现有襁褓{clan.InfantCount}口，宗房余力{clan.SupportReserve}。"
                        : $"门内现有襁褓{clan.InfantCount}口，但宗房余力{clan.SupportReserve}，一时难再加拨。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.DesignateHeirPolicy,
                clan.HomeSettlementId,
                $"{clan.ClanName}可先定承祧次序，把香火名分与后议先写稳。",
                !clan.HeirPersonId.HasValue || clan.HeirSecurity < 60,
                !clan.HeirPersonId.HasValue
                    ? "堂上尚未举出承祧之人，宜先定后序。"
                    : $"承祧稳度{clan.HeirSecurity}，名分若虚仍易再起后议。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.SetMourningOrder,
                clan.HomeSettlementId,
                $"{clan.ClanName}可先议定丧次与祭次，别让门内一边举哀一边再翻后议。",
                clan.MourningLoad > 0,
                clan.MourningLoad > 0
                    ? $"门内丧服之重{clan.MourningLoad}，宜先定服序与支用。"
                    : "门内暂无举哀之事，眼下不必另议丧次。",
                clanId: clan.Id,
                targetLabel: clan.ClanName));
        }

        foreach (JurisdictionAuthoritySnapshot jurisdiction in bundle.OfficeJurisdictions.OrderBy(static entry => entry.SettlementId.Value))
        {
            bool canReviewPetitions = jurisdiction.PetitionBacklog > 0 || jurisdiction.PetitionPressure > 0;
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.PetitionViaOfficeChannels,
                jurisdiction.SettlementId,
                $"{jurisdiction.LeadOfficialName}可在{jurisdiction.LeadOfficeTitle}任上先理词状，缓解积案与乡里怨气。",
                canReviewPetitions,
                canReviewPetitions
                    ? $"积案{jurisdiction.PetitionBacklog}，可先批结一轮。"
                    : "本处暂无待批词状。"));
            affordances.Add(BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.DeployAdministrativeLeverage,
                jurisdiction.SettlementId,
                $"{jurisdiction.LeadOfficialName}可凭官箴与印信发签催办，先压急牍与拖延。",
                jurisdiction.JurisdictionLeverage >= 12,
                jurisdiction.JurisdictionLeverage >= 12
                    ? $"乡面杠杆{jurisdiction.JurisdictionLeverage}，足可催动里甲与吏胥。"
                    : "此地官箴未足，不宜强行发签。"));
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
        return BuildPlayerCommandAffordanceSnapshot(
            commandName,
            signal.SettlementId,
            summary,
            isEnabled,
            availabilitySummary);
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildPublicLifeAffordances(PresentationReadModelBundle bundle)
    {
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = IndexFirstBySettlement(
            bundle.OfficeJurisdictions,
            static entry => entry.SettlementId);
        Dictionary<int, SettlementDisorderSnapshot> disorderBySettlement = IndexFirstBySettlement(
            bundle.SettlementDisorder,
            static entry => entry.SettlementId);
        ILookup<int, ClanSnapshot> clansBySettlement = bundle.Clans.ToLookup(static entry => entry.HomeSettlementId.Value);
        Dictionary<int, ClanNarrativeSnapshot> narrativesByClan = bundle.ClanNarratives
            .ToDictionary(static entry => entry.ClanId.Value, static entry => entry);
        ILookup<int, ClanTradeSnapshot> tradesBySettlement = bundle.ClanTrades
            .ToLookup(static entry => entry.PrimarySettlementId.Value);
        ILookup<int, ClanTradeRouteSnapshot> routesBySettlement = bundle.ClanTradeRoutes
            .ToLookup(static entry => entry.SettlementId.Value);

        foreach (SettlementPublicLifeSnapshot publicLife in bundle.PublicLifeSettlements.OrderBy(static entry => entry.SettlementId.Value))
        {
            jurisdictionsBySettlement.TryGetValue(publicLife.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
            ClanSnapshot[] localClans = clansBySettlement[publicLife.SettlementId.Value]
                .OrderByDescending(static entry => entry.Prestige)
                .ThenBy(static entry => entry.ClanName, StringComparer.Ordinal)
                .ToArray();
            ClanNarrativeSnapshot[] localNarratives = localClans
                .Where(clan => narrativesByClan.ContainsKey(clan.Id.Value))
                .Select(clan => narrativesByClan[clan.Id.Value])
                .ToArray();
            ClanTradeSnapshot[] localTrades = tradesBySettlement[publicLife.SettlementId.Value].ToArray();
            ClanTradeRouteSnapshot[] localRoutes = routesBySettlement[publicLife.SettlementId.Value].ToArray();
            IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories =
                SelectLocalPublicLifeOrderSocialMemories(bundle.SocialMemories, localClans);

            if (jurisdiction is not null)
            {
                yield return BuildPlayerCommandAffordanceSnapshot(
                    PlayerCommandNames.PostCountyNotice,
                    publicLife.SettlementId,
                    $"{publicLife.NodeLabel}街谈已热，可先借榜示压住众口。",
                    publicLife.StreetTalkHeat >= 40 || publicLife.PublicLegitimacy < 55,
                    $"榜示分量{publicLife.DocumentaryWeight}，由{jurisdiction.LeadOfficialName}主其晓谕。",
                    targetLabel: publicLife.NodeLabel);

                yield return BuildPlayerCommandAffordanceSnapshot(
                    PlayerCommandNames.DispatchRoadReport,
                    publicLife.SettlementId,
                    $"{publicLife.DominantVenueLabel}消息往来已有迟滞，可先遣吏催报。",
                    publicLife.RoadReportLag >= 36 || publicLife.CourierRisk >= 35,
                    $"递报险数{publicLife.CourierRisk}，查验周折{publicLife.VerificationCost}。",
                    targetLabel: publicLife.DominantVenueLabel);
            }

            if (disorderBySettlement.TryGetValue(publicLife.SettlementId.Value, out SettlementDisorderSnapshot? disorder))
            {
                string administrativeReachSummary = OrderAndBanditryCommandResolver.DetermineAdministrativeReachExecutionSummary(jurisdiction);

                foreach (PlayerCommandAffordanceSnapshot affordance in BuildSupplementalOrderPublicLifeAffordances(
                    publicLife,
                    disorder,
                    jurisdiction,
                    localClans,
                    localNarratives,
                    localTrades,
                    localRoutes,
                    localSocialMemories,
                    administrativeReachSummary))
                {
                    yield return affordance;
                }

                foreach (PlayerCommandAffordanceSnapshot affordance in BuildPublicLifeOrderResponseAffordances(
                    publicLife,
                    disorder,
                    jurisdiction,
                    localClans,
                    localSocialMemories))
                {
                    yield return affordance;
                }

                CommandLeverageProjection escortProjection = BuildOrderPublicLifeLeverageProjection(
                    PlayerCommandNames.EscortRoadReport,
                    publicLife,
                    disorder,
                    jurisdiction,
                    localClans,
                    localNarratives,
                    localTrades,
                    localRoutes,
                    localSocialMemories);
                yield return BuildPlayerCommandAffordanceSnapshot(
                    PlayerCommandNames.EscortRoadReport,
                    publicLife.SettlementId,
                    $"{publicLife.DominantVenueLabel}近来路情不稳，可先催护一路，保住津口与路报。",
                    disorder.RoutePressure >= 28 || publicLife.CourierRisk >= 32,
                    $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                    executionSummary: administrativeReachSummary,
                    leverageSummary: escortProjection.LeverageSummary,
                    costSummary: escortProjection.CostSummary,
                    readbackSummary: escortProjection.ReadbackSummary,
                    targetLabel: publicLife.DominantVenueLabel);
            }

            ClanSnapshot? leadClan = localClans.FirstOrDefault();
            if (leadClan is not null)
            {
                yield return BuildPlayerCommandAffordanceSnapshot(
                    PlayerCommandNames.InviteClanEldersPubliclyBroker,
                    publicLife.SettlementId,
                    $"{leadClan.ClanName}可请族老先出面缓口，免得堂内家事扩成街谈公议。",
                    publicLife.StreetTalkHeat >= 45 || publicLife.MarketRumorFlow >= 45,
                    $"街谈{publicLife.StreetTalkHeat}，市语流势{publicLife.MarketRumorFlow}。",
                    clanId: leadClan.Id,
                    targetLabel: leadClan.ClanName);
            }
        }
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildPublicLifeOrderResponseAffordances(
        SettlementPublicLifeSnapshot publicLife,
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        IReadOnlyList<ClanSnapshot> localClans,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories)
    {
        if (!HasPublicLifeOrderRefusalOrPartialResidue(disorder))
        {
            yield break;
        }

        string responseReadback = CombinePublicLifeResponseText(
            BuildOrderLandingAftermathSummary(disorder),
            BuildOrderResponseAftermathSummary(disorder),
            BuildOrderSocialMemoryReadbackSummary(localSocialMemories));
        string targetLabel = string.IsNullOrWhiteSpace(publicLife.DominantVenueLabel)
            ? publicLife.NodeLabel
            : publicLife.DominantVenueLabel;
        bool watchResidue = string.Equals(disorder.LastInterventionCommandCode, PlayerCommandNames.FundLocalWatch, StringComparison.Ordinal);
        bool suppressionResidue = string.Equals(disorder.LastInterventionCommandCode, PlayerCommandNames.SuppressBanditry, StringComparison.Ordinal);

        if (watchResidue)
        {
            yield return BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.RepairLocalWatchGuarantee,
                publicLife.SettlementId,
                $"{targetLabel}的巡丁后账已露出来，可补保巡丁，把本户担保重新接住。",
                true,
                $"前案为{RenderOrderInterventionLabel(disorder)}；{RenderOrderOutcomeLabelForReadback(disorder.LastInterventionOutcomeCode)}。",
                leverageSummary: "只动路面担保、巡丁口粮与本户信誉；成败要看巡丁、脚户与路面口风。",
                costSummary: "需再押钱粮、人手和公开担保，若前案误读未解，仍可能只压住一半。",
                readbackSummary: responseReadback,
                targetLabel: targetLabel);

            yield return BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.CompensateRunnerMisread,
                publicLife.SettlementId,
                $"{targetLabel}的脚户误读正在传开，可先赔脚户误读，压住脚路口角。",
                true,
                $"误读/拖延来自结构化前案余波：{RenderOrderPartialLabel(disorder.LastInterventionPartialCode)}。",
                leverageSummary: "只触达脚户、巡丁与路面解释，不代替县门补落地。",
                costSummary: "赔付会花现钱和人情，换来脚路传话先缓。",
                readbackSummary: responseReadback,
                targetLabel: targetLabel);
        }

        if (suppressionResidue)
        {
            yield return BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.DeferHardPressure,
                publicLife.SettlementId,
                $"{publicLife.NodeLabel}强压后账仍在，可暂缓强压，先压住地面反噬。",
                true,
                $"前案为{RenderOrderInterventionLabel(disorder)}；报复险{disorder.RetaliationRisk}，胁迫险{disorder.CoercionRisk}。",
                leverageSummary: "只走治安与路面人手收束强压余波，不改官署或宗房账。",
                costSummary: "明面反噬会缓，但路匪尾巴未必马上收净。",
                readbackSummary: responseReadback,
                targetLabel: publicLife.NodeLabel);
        }

        if (jurisdiction is not null)
        {
            string leadLabel = string.IsNullOrWhiteSpace(jurisdiction.LeadOfficialName)
                ? jurisdiction.LeadOfficeTitle
                : jurisdiction.LeadOfficialName;
            yield return BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.PressCountyYamenDocument,
                publicLife.SettlementId,
                $"{leadLabel}可押文催县门，把拖延后账落到案牍上。",
                true,
                $"县门积案{jurisdiction.PetitionBacklog}，胥吏牵制{jurisdiction.ClerkDependence}。",
                executionSummary: $"眼下由{leadLabel}触达县门文移，成败要看文移、胥吏与官面余力。",
                leverageSummary: "官署只处理催办、文移落地与胥吏拖延。",
                costSummary: "若胥吏牵制太重，押文会变成新的积案与恶化后账。",
                readbackSummary: responseReadback,
                targetLabel: leadLabel);

            yield return BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.RedirectRoadReport,
                publicLife.SettlementId,
                $"{leadLabel}可改走递报，绕开县门拖滞，先把路情送出。",
                true,
                $"递报险数{publicLife.CourierRisk}，县门积案{jurisdiction.PetitionBacklog}。",
                executionSummary: $"递报仍归{leadLabel}经手；只是改走文移路径。",
                leverageSummary: "官署处理递报路径，不能直接修复巡丁或宗房羞面。",
                costSummary: "只能暂压后账，正路未补时仍留余波。",
                readbackSummary: responseReadback,
                targetLabel: leadLabel);
        }

        ClanSnapshot? leadClan = localClans.FirstOrDefault();
        if (leadClan is not null)
        {
            yield return BuildPlayerCommandAffordanceSnapshot(
                PlayerCommandNames.AskClanEldersExplain,
                publicLife.SettlementId,
                $"{leadClan.ClanName}可请族老解释，让本户把前案用意说清。",
                true,
                $"宗房门望{leadClan.Prestige}，余力{leadClan.SupportReserve}，房支争力{leadClan.BranchTension}。",
                leverageSummary: "族老只处理公开解释与本户担保，不替治安或县门落命令。",
                costSummary: "解释能缓羞面，也可能留下人情欠账。",
                readbackSummary: responseReadback,
                clanId: leadClan.Id,
                targetLabel: leadClan.ClanName);
        }
    }

    private static bool HasPublicLifeOrderRefusalOrPartialResidue(SettlementDisorderSnapshot disorder)
    {
        return string.Equals(disorder.LastInterventionOutcomeCode, OrderInterventionOutcomeCodes.Refused, StringComparison.Ordinal)
            || string.Equals(disorder.LastInterventionOutcomeCode, OrderInterventionOutcomeCodes.Partial, StringComparison.Ordinal);
    }

    private static string CombinePublicLifeResponseText(params string[] parts)
    {
        return string.Join(" ", parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string RenderOrderOutcomeLabelForReadback(string outcomeCode)
    {
        return outcomeCode switch
        {
            OrderInterventionOutcomeCodes.Refused => "前案被拒",
            OrderInterventionOutcomeCodes.Partial => "前案半落地",
            OrderInterventionOutcomeCodes.Accepted => "前案已落地",
            _ => outcomeCode,
        };
    }

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildSupplementalOrderPublicLifeAffordances(
        SettlementPublicLifeSnapshot publicLife,
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        IReadOnlyList<ClanSnapshot> localClans,
        IReadOnlyList<ClanNarrativeSnapshot> localNarratives,
        IReadOnlyList<ClanTradeSnapshot> localTrades,
        IReadOnlyList<ClanTradeRouteSnapshot> localRoutes,
        IReadOnlyList<SocialMemoryEntrySnapshot> localSocialMemories,
        string administrativeReachSummary)
    {
        CommandLeverageProjection watchProjection = BuildOrderPublicLifeLeverageProjection(
            PlayerCommandNames.FundLocalWatch,
            publicLife,
            disorder,
            jurisdiction,
            localClans,
            localNarratives,
            localTrades,
            localRoutes,
            localSocialMemories);
        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.FundLocalWatch,
            publicLife.SettlementId,
            $"{publicLife.DominantVenueLabel}近来脚路不稳，可先添雇巡丁，把路口与渡头补起来。",
            disorder.RoutePressure >= 22 || disorder.DisorderPressure >= 24,
            $"路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            executionSummary: administrativeReachSummary,
            leverageSummary: watchProjection.LeverageSummary,
            costSummary: watchProjection.CostSummary,
            readbackSummary: watchProjection.ReadbackSummary,
            targetLabel: publicLife.DominantVenueLabel);

        CommandLeverageProjection suppressionProjection = BuildOrderPublicLifeLeverageProjection(
            PlayerCommandNames.SuppressBanditry,
            publicLife,
            disorder,
            jurisdiction,
            localClans,
            localNarratives,
            localTrades,
            localRoutes,
            localSocialMemories);
        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.SuppressBanditry,
            publicLife.SettlementId,
            $"{publicLife.NodeLabel}已见路匪踪迹，可先严缉，但后手报复也会更重。",
            disorder.BanditThreat >= 36 || disorder.SuppressionDemand >= 32,
            $"盗压{disorder.BanditThreat}，镇压之需{disorder.SuppressionDemand}。",
            executionSummary: administrativeReachSummary,
            leverageSummary: suppressionProjection.LeverageSummary,
            costSummary: suppressionProjection.CostSummary,
            readbackSummary: suppressionProjection.ReadbackSummary,
            targetLabel: publicLife.NodeLabel);

        CommandLeverageProjection negotiateProjection = BuildOrderPublicLifeLeverageProjection(
            PlayerCommandNames.NegotiateWithOutlaws,
            publicLife,
            disorder,
            jurisdiction,
            localClans,
            localNarratives,
            localTrades,
            localRoutes,
            localSocialMemories);
        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.NegotiateWithOutlaws,
            publicLife.SettlementId,
            $"{publicLife.DominantVenueLabel}若先求一时通路，可遣人议路，换一段缓和。",
            disorder.BanditThreat >= 24 || disorder.DisorderPressure >= 28,
            $"盗压{disorder.BanditThreat}，地面不靖{disorder.DisorderPressure}。",
            executionSummary: administrativeReachSummary,
            leverageSummary: negotiateProjection.LeverageSummary,
            costSummary: negotiateProjection.CostSummary,
            readbackSummary: negotiateProjection.ReadbackSummary,
            targetLabel: publicLife.DominantVenueLabel);

        CommandLeverageProjection tolerateProjection = BuildOrderPublicLifeLeverageProjection(
            PlayerCommandNames.TolerateDisorder,
            publicLife,
            disorder,
            jurisdiction,
            localClans,
            localNarratives,
            localTrades,
            localRoutes,
            localSocialMemories);
        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.TolerateDisorder,
            publicLife.SettlementId,
            $"{publicLife.NodeLabel}若眼下不宜再逼，也可先缓一缓穷追，把明面风声压住。",
            disorder.BanditThreat >= 18 || disorder.RoutePressure >= 18 || disorder.DisorderPressure >= 18,
            $"盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            executionSummary: administrativeReachSummary,
            leverageSummary: tolerateProjection.LeverageSummary,
            costSummary: tolerateProjection.CostSummary,
            readbackSummary: tolerateProjection.ReadbackSummary,
            targetLabel: publicLife.NodeLabel);
    }
}
