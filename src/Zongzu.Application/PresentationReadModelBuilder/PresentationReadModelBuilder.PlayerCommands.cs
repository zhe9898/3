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

        foreach (SettlementPublicLifeSnapshot publicLife in bundle.PublicLifeSettlements.OrderBy(static entry => entry.SettlementId.Value))
        {
            jurisdictionsBySettlement.TryGetValue(publicLife.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);

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

                foreach (PlayerCommandAffordanceSnapshot affordance in BuildSupplementalOrderPublicLifeAffordances(publicLife, disorder, administrativeReachSummary))
                {
                    yield return affordance;
                }

                yield return BuildPlayerCommandAffordanceSnapshot(
                    PlayerCommandNames.EscortRoadReport,
                    publicLife.SettlementId,
                    $"{publicLife.DominantVenueLabel}近来路情不稳，可先催护一路，保住津口与路报。",
                    disorder.RoutePressure >= 28 || publicLife.CourierRisk >= 32,
                    $"路压{disorder.RoutePressure}，镇压之需{disorder.SuppressionDemand}。",
                    executionSummary: administrativeReachSummary,
                    targetLabel: publicLife.DominantVenueLabel);
            }

            ClanSnapshot? leadClan = clansBySettlement[publicLife.SettlementId.Value]
                .OrderByDescending(static entry => entry.Prestige)
                .ThenBy(static entry => entry.ClanName, StringComparer.Ordinal)
                .FirstOrDefault();
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

    private static IEnumerable<PlayerCommandAffordanceSnapshot> BuildSupplementalOrderPublicLifeAffordances(
        SettlementPublicLifeSnapshot publicLife,
        SettlementDisorderSnapshot disorder,
        string administrativeReachSummary)
    {
        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.FundLocalWatch,
            publicLife.SettlementId,
            $"{publicLife.DominantVenueLabel}近来脚路不稳，可先添雇巡丁，把路口与渡头补起来。",
            disorder.RoutePressure >= 22 || disorder.DisorderPressure >= 24,
            $"路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            executionSummary: administrativeReachSummary,
            targetLabel: publicLife.DominantVenueLabel);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.SuppressBanditry,
            publicLife.SettlementId,
            $"{publicLife.NodeLabel}已见路匪踪迹，可先严缉，但后手报复也会更重。",
            disorder.BanditThreat >= 36 || disorder.SuppressionDemand >= 32,
            $"盗压{disorder.BanditThreat}，镇压之需{disorder.SuppressionDemand}。",
            executionSummary: administrativeReachSummary,
            targetLabel: publicLife.NodeLabel);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.NegotiateWithOutlaws,
            publicLife.SettlementId,
            $"{publicLife.DominantVenueLabel}若先求一时通路，可遣人议路，换一段缓和。",
            disorder.BanditThreat >= 24 || disorder.DisorderPressure >= 28,
            $"盗压{disorder.BanditThreat}，地面不靖{disorder.DisorderPressure}。",
            executionSummary: administrativeReachSummary,
            targetLabel: publicLife.DominantVenueLabel);

        yield return BuildPlayerCommandAffordanceSnapshot(
            PlayerCommandNames.TolerateDisorder,
            publicLife.SettlementId,
            $"{publicLife.NodeLabel}若眼下不宜再逼，也可先缓一缓穷追，把明面风声压住。",
            disorder.BanditThreat >= 18 || disorder.RoutePressure >= 18 || disorder.DisorderPressure >= 18,
            $"盗压{disorder.BanditThreat}，路压{disorder.RoutePressure}，地面不靖{disorder.DisorderPressure}。",
            executionSummary: administrativeReachSummary,
            targetLabel: publicLife.NodeLabel);
    }
}
