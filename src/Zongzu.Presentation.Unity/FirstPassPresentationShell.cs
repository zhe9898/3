using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Presentation.Unity;

public static class FirstPassPresentationShell
{
    public static PresentationShellViewModel Compose(PresentationReadModelBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        NarrativeNotificationSnapshot[] notifications = bundle.Notifications
            .OrderBy(notification => notification.Tier)
            .ThenByDescending(static notification => notification.CreatedAt.Year)
            .ThenByDescending(static notification => notification.CreatedAt.Month)
            .ThenByDescending(static notification => notification.Id.Value)
            .ToArray();

        return new PresentationShellViewModel
        {
            GreatHall = BuildGreatHall(bundle, notifications),
            Lineage = BuildLineage(bundle),
            DeskSandbox = BuildDeskSandbox(bundle, notifications),
            Office = BuildOfficeSurface(bundle),
            Warfare = BuildWarfareSurfaceRegionalChinese(bundle, notifications),
            NotificationCenter = BuildNotificationCenter(notifications),
            Debug = BuildDebugPanel(bundle.Debug),
        };
    }

    private static GreatHallDashboardViewModel BuildGreatHall(PresentationReadModelBundle bundle, IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        ClanSnapshot? leadClan = bundle.Clans
            .OrderByDescending(static clan => clan.Prestige)
            .ThenBy(static clan => clan.ClanName, StringComparer.Ordinal)
            .FirstOrDefault();
        int studyingCount = bundle.EducationCandidates.Count(static candidate => candidate.IsStudying);
        int passedCount = bundle.EducationCandidates.Count(static candidate => candidate.HasPassedLocalExam);
        int profitableClans = bundle.ClanTrades.Count(static trade => string.Equals(trade.LastOutcome, "Profit", StringComparison.Ordinal));
        int appointedCount = bundle.OfficeCareers.Count(static career => career.HasAppointment);
        JurisdictionAuthoritySnapshot? leadJurisdiction = bundle.OfficeJurisdictions
            .OrderByDescending(static jurisdiction => jurisdiction.AuthorityTier)
            .ThenByDescending(static jurisdiction => jurisdiction.JurisdictionLeverage)
            .ThenBy(static jurisdiction => jurisdiction.SettlementId.Value)
            .FirstOrDefault();

        return new GreatHallDashboardViewModel
        {
            CurrentDateLabel = $"{bundle.CurrentDate.Year}-{bundle.CurrentDate.Month:D2}",
            ReplayHash = bundle.ReplayHash,
            UrgentCount = notifications.Count(static notification => notification.Tier == NotificationTier.Urgent),
            ConsequentialCount = notifications.Count(static notification => notification.Tier == NotificationTier.Consequential),
            BackgroundCount = notifications.Count(static notification => notification.Tier == NotificationTier.Background),
            FamilySummary = leadClan is null
                ? "堂中暂无宗房呈报。"
                : $"{leadClan.ClanName}门望{leadClan.Prestige}，可支宗力{leadClan.SupportReserve}。",
            EducationSummary = $"塾馆在读{studyingCount}人，场屋得捷{passedCount}人。",
            TradeSummary = $"市账{bundle.ClanTrades.Count}册，得利{profitableClans}支。",
            GovernanceSummary = leadJurisdiction is null
                ? "案头暂无官署呈报。"
                : $"{appointedCount}人在官途；{leadJurisdiction.LeadOfficialName}以{RenderOfficeTitle(leadJurisdiction.LeadOfficeTitle)}主事，{RenderPetitionOutcomeCategory(leadJurisdiction.PetitionOutcomeCategory)}，积案{leadJurisdiction.PetitionBacklog}。",
            WarfareSummary = BuildGreatHallWarfareSummaryRegionalChinese(bundle),
            AftermathDocketSummary = BuildGreatHallAftermathDocketSummary(bundle, notifications),
            LeadNoticeTitle = notifications.FirstOrDefault()?.Title ?? "堂上暂无急报",
        };
    }

    private static LineageSurfaceViewModel BuildLineage(PresentationReadModelBundle bundle)
    {
        return new LineageSurfaceViewModel
        {
            Clans = bundle.Clans
                .OrderBy(static clan => clan.ClanName, StringComparer.Ordinal)
                .Select(static clan => new ClanTileViewModel
                {
                    ClanName = clan.ClanName,
                    Prestige = clan.Prestige,
                    SupportReserve = clan.SupportReserve,
                    StatusText = clan.HeirPersonId is null
                        ? "宗房暂未举出承祧人。"
                        : "承祧之人已入谱。",
                })
                .ToArray(),
        };
    }

    private static DeskSandboxViewModel BuildDeskSandbox(
        PresentationReadModelBundle bundle,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        Dictionary<int, PopulationSettlementSnapshot> populationBySettlement = bundle.PopulationSettlements
            .ToDictionary(static settlement => settlement.SettlementId.Value, static settlement => settlement);
        ILookup<int, AcademySnapshot> academiesBySettlement = bundle.Academies.ToLookup(static academy => academy.SettlementId.Value);
        Dictionary<int, MarketSnapshot> marketsBySettlement = bundle.Markets.ToDictionary(static market => market.SettlementId.Value, static market => market);
        ILookup<int, TradeRouteSnapshot> tradeRoutesBySettlement = bundle.TradeRoutes.ToLookup(static route => route.SettlementId.Value);
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = bundle.OfficeJurisdictions
            .ToDictionary(static jurisdiction => jurisdiction.SettlementId.Value, static jurisdiction => jurisdiction);
        Dictionary<int, CampaignFrontSnapshot> campaignsBySettlement = bundle.Campaigns
            .ToDictionary(static campaign => campaign.AnchorSettlementId.Value, static campaign => campaign);
        Dictionary<int, CampaignMobilizationSignalSnapshot> mobilizationSignalsBySettlement = bundle.CampaignMobilizationSignals
            .ToDictionary(static signal => signal.SettlementId.Value, static signal => signal);

        return new DeskSandboxViewModel
        {
            Settlements = bundle.Settlements
                .OrderBy(static settlement => settlement.Name, StringComparer.Ordinal)
                .Select(settlement =>
                {
                    PopulationSettlementSnapshot? population = populationBySettlement.TryGetValue(settlement.Id.Value, out PopulationSettlementSnapshot? snapshot)
                        ? snapshot
                        : null;
                    AcademySnapshot[] academies = academiesBySettlement[settlement.Id.Value].OrderBy(static academy => academy.AcademyName, StringComparer.Ordinal).ToArray();
                    bool hasMarket = marketsBySettlement.TryGetValue(settlement.Id.Value, out MarketSnapshot? market);
                    bool hasJurisdiction = jurisdictionsBySettlement.TryGetValue(settlement.Id.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
                    bool hasCampaign = campaignsBySettlement.TryGetValue(settlement.Id.Value, out CampaignFrontSnapshot? campaign);
                    bool hasSignal = mobilizationSignalsBySettlement.TryGetValue(settlement.Id.Value, out CampaignMobilizationSignalSnapshot? mobilizationSignal);
                    TradeRouteSnapshot[] tradeRoutes = tradeRoutesBySettlement[settlement.Id.Value]
                        .OrderBy(static route => route.RouteName, StringComparer.Ordinal)
                        .ToArray();

                    return new SettlementNodeViewModel
                    {
                        SettlementName = settlement.Name,
                        Security = settlement.Security,
                        Prosperity = settlement.Prosperity,
                        AcademySummary = academies.Length == 0
                            ? "塾馆未立。"
                            : string.Join(", ", academies.Select(static academy => academy.AcademyName)),
                        MarketSummary = hasMarket
                            ? $"{market!.MarketName}：市需{market.Demand}，价行{market.PriceIndex}，路险{market.LocalRisk}。"
                            : "市肆未起。",
                        GovernanceSummary = hasJurisdiction
                            ? $"{RenderOfficeTitle(jurisdiction!.LeadOfficeTitle)}{jurisdiction.LeadOfficialName}：乡面杖力{jurisdiction.JurisdictionLeverage}，{RenderAdministrativeTaskTier(jurisdiction.AdministrativeTaskTier)}差遣 {RenderAdministrativeTask(jurisdiction.CurrentAdministrativeTask)}，{RenderPetitionOutcomeCategory(jurisdiction.PetitionOutcomeCategory)}，积案{jurisdiction.PetitionBacklog}。"
                            : "官署未设。",
                        CampaignSummary = BuildSettlementCampaignSummaryRegionalChinese(
                            hasCampaign ? campaign : null,
                            hasSignal ? mobilizationSignal : null,
                            settlement,
                            tradeRoutes),
                        AftermathSummary = BuildSettlementAftermathSummary(
                            settlement,
                            population,
                            hasJurisdiction ? jurisdiction : null,
                            hasCampaign ? campaign : null,
                            notifications),
                        PressureSummary = population is null
                            ? "民户情形未起。"
                            : $"民困{population.CommonerDistress}，丁力{population.LaborSupply}，流徙{population.MigrationPressure}。",
                    };
                })
                .ToArray(),
        };
    }

    private static OfficeSurfaceViewModel BuildOfficeSurface(PresentationReadModelBundle bundle)
    {
        if (bundle.OfficeCareers.Count == 0 && bundle.OfficeJurisdictions.Count == 0)
        {
            return new OfficeSurfaceViewModel
            {
                Summary = "案头暂无官署牍报。",
            };
        }

        int appointedCount = bundle.OfficeCareers.Count(static career => career.HasAppointment);
        int activeJurisdictions = bundle.OfficeJurisdictions.Count;
        int peakBacklog = bundle.OfficeJurisdictions.Count == 0
            ? 0
            : bundle.OfficeJurisdictions.Max(static jurisdiction => jurisdiction.PetitionBacklog);
        Dictionary<int, string> settlementNames = bundle.Settlements.ToDictionary(static settlement => settlement.Id.Value, static settlement => settlement.Name);

        return new OfficeSurfaceViewModel
        {
            Summary = $"现有官人{appointedCount}名，分掌{activeJurisdictions}处，积案最高{peakBacklog}。",
            Appointments = bundle.OfficeCareers
                .OrderByDescending(static career => career.HasAppointment)
                .ThenByDescending(static career => career.AuthorityTier)
                .ThenBy(static career => career.PersonId.Value)
                .Select(static career => new OfficeAppointmentViewModel
                {
                    DisplayName = career.DisplayName,
                    OfficeTitle = RenderOfficeTitle(career.OfficeTitle),
                    HasAppointment = career.HasAppointment,
                    AuthorityTier = career.AuthorityTier,
                    ServiceSummary = career.HasAppointment
                        ? $"供职{career.ServiceMonths}月；升势{RenderPromotionPressureLabel(career.PromotionPressureLabel)}，黜压{RenderDemotionPressureLabel(career.DemotionPressureLabel)}。"
                        : "候补听选。",
                    TaskSummary = $"{RenderAdministrativeTaskTier(career.AdministrativeTaskTier)}差遣：{RenderAdministrativeTask(career.CurrentAdministrativeTask)}",
                    PetitionSummary = $"词牍压{career.PetitionPressure}，积案{career.PetitionBacklog}。",
                    PressureSummary = BuildOfficePressureSummary(career.HasAppointment, career.LastOutcome, career.PetitionBacklog, career.PromotionPressureLabel, career.DemotionPressureLabel),
                    PetitionOutcomeCategory = RenderPetitionOutcomeCategory(career.PetitionOutcomeCategory),
                    LastOutcome = career.LastOutcome,
                    LastPetitionOutcome = RenderPetitionOutcome(career.LastPetitionOutcome),
                })
                .ToArray(),
            Jurisdictions = bundle.OfficeJurisdictions
                .OrderBy(static jurisdiction => jurisdiction.SettlementId.Value)
                .Select(jurisdiction => new OfficeJurisdictionViewModel
                {
                    SettlementLabel = settlementNames.TryGetValue(jurisdiction.SettlementId.Value, out string? settlementName)
                        ? settlementName
                        : $"乡里#{jurisdiction.SettlementId.Value}",
                    LeadSummary = $"{RenderOfficeTitle(jurisdiction.LeadOfficeTitle)} {jurisdiction.LeadOfficialName}",
                    LeverageSummary = $"秩阶{jurisdiction.AuthorityTier}，乡面杖力{jurisdiction.JurisdictionLeverage}。",
                    PetitionSummary = $"词牍压{jurisdiction.PetitionPressure}，积案{jurisdiction.PetitionBacklog}。",
                    TaskSummary = $"{RenderAdministrativeTaskTier(jurisdiction.AdministrativeTaskTier)}差遣：{RenderAdministrativeTask(jurisdiction.CurrentAdministrativeTask)}",
                    PetitionOutcomeCategory = RenderPetitionOutcomeCategory(jurisdiction.PetitionOutcomeCategory),
                    LastPetitionOutcome = RenderPetitionOutcome(jurisdiction.LastPetitionOutcome),
                })
                .ToArray(),
        };
    }

    private static string RenderAdministrativeTask(string taskName)
    {
        return taskName switch
        {
            "Awaiting posting" => "候补听选",
            "emergency petition review" => "急牍覆核",
            "district petition hearings" => "勘理词状",
            "hearing district petitions" => "勘理词状",
            "county register review" => "勾检户籍",
            "petition copying desk" => "誊录词牍",
            "sealed filing copy desk" => "誊黄封牍",
            "copying tax rolls and sealed filings" => "誊录税册与封牍",
            "reviewing memorials after the campaign" => "覆核战后功过文移",
            _ => taskName,
        };
    }

    private static string RenderOfficeTitle(string officeTitle)
    {
        return officeTitle switch
        {
            "Assistant Magistrate" => "县丞",
            "Registrar" => "主簿",
            "District Clerk" => "书吏",
            "Unappointed" => "未授官",
            _ => officeTitle,
        };
    }

    private static string RenderAdministrativeTaskTier(string taskTier)
    {
        return taskTier switch
        {
            "crisis" => "急务",
            "district" => "州县",
            "registry" => "簿册",
            "clerical" => "案牍",
            "inactive" => "候补",
            _ => taskTier,
        };
    }

    private static string RenderPetitionOutcomeCategory(string category)
    {
        return category switch
        {
            "Queued" => "待勘",
            "Unavailable" => "未开案",
            "Delayed" => "稽延",
            "Triaged" => "分轻重",
            "Cleared" => "已清",
            "Granted" => "准行",
            "Surged" => "案牍骤涌",
            "Stalled" => "壅滞",
            "Censured" => "劾责中",
            "Unknown" => "未详",
            _ => category,
        };
    }

    private static string RenderPromotionPressureLabel(string label)
    {
        return label switch
        {
            "promotion-ready" => "可迁",
            "rising" => "渐起",
            "steady" => "持平",
            "thin" => "微弱",
            _ => label,
        };
    }

    private static string RenderDemotionPressureLabel(string label)
    {
        return label switch
        {
            "critical" => "危急",
            "strained" => "吃紧",
            "watched" => "在察",
            "stable" => "平稳",
            _ => label,
        };
    }

    private static string BuildOfficePressureSummary(
        bool hasAppointment,
        string lastOutcome,
        int petitionBacklog,
        string promotionLabel,
        string demotionLabel)
    {
        string promotion = RenderPromotionPressureLabel(promotionLabel);
        string demotion = RenderDemotionPressureLabel(demotionLabel);

        if (!hasAppointment)
        {
            return "官途未开，仍在候缺。";
        }

        return lastOutcome switch
        {
            "Promoted" => $"官途近有迁转之势，升势{promotion}，黜压{demotion}。",
            "Demoted" => $"官途方遭降黜，升势{promotion}，黜压{demotion}。",
            "Lost" => $"官途已失，积案{petitionBacklog}，黜压{demotion}。",
            _ => $"官途暂守，升势{promotion}，黜压{demotion}。",
        };
    }

    private static string RenderPetitionOutcome(string outcome)
    {
        if (string.IsNullOrWhiteSpace(outcome))
        {
            return string.Empty;
        }

        int separatorIndex = outcome.IndexOf(':');
        if (separatorIndex > 0 && separatorIndex < outcome.Length - 1)
        {
            string category = outcome[..separatorIndex].Trim();
            string detail = outcome[(separatorIndex + 1)..].Trim();
            return $"{RenderPetitionOutcomeCategory(category)}：{detail}";
        }

        return outcome switch
        {
            "Petitions cleared while copying tax rolls and sealed filings." => "已清：税册与封牍俱已誊定。",
            "Censured and triaged." => "劾责中：词牍分轻重收理。",
            _ => outcome,
        };
    }

    private static string RenderCampaignSurfaceText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string rendered = RenderEscortRouteSummary(text);
        rendered = rendered.Replace(
            "Registrar couriers are tying docket traffic into the campaign board.",
            "主簿差役正把文移驿线并入军机案头。",
            StringComparison.Ordinal);

        return rendered
            .Replace("Registrar ", "主簿 ", StringComparison.Ordinal)
            .Replace("district层级", "县署层级", StringComparison.Ordinal)
            .Replace(" district ", " 县署 ", StringComparison.OrdinalIgnoreCase)
            .Replace("campaign board", "军机案头", StringComparison.OrdinalIgnoreCase)
            .Replace("docket traffic", "文移驿线", StringComparison.OrdinalIgnoreCase)
            .Replace("backlog", "积案", StringComparison.OrdinalIgnoreCase);
    }

    private static string RenderEscortRouteSummary(string text)
    {
        const string marker = " escorts are keeping stores moving for ";
        int markerIndex = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex <= 0)
        {
            return text;
        }

        string countText = text[..markerIndex].Trim();
        if (!int.TryParse(countText, out int escortCount))
        {
            return text;
        }

        int settlementStart = markerIndex + marker.Length;
        int sentenceEnd = text.IndexOf('.', settlementStart);
        if (sentenceEnd < 0)
        {
            return text;
        }

        string settlementLabel = text[settlementStart..sentenceEnd].Trim();
        string tail = text[(sentenceEnd + 1)..].Trim();
        string rewritten = $"{settlementLabel}粮秣正由护运{escortCount}人维持行转。";
        return string.IsNullOrEmpty(tail)
            ? rewritten
            : $"{rewritten} {tail}";
    }

    private static string BuildCampaignFrontSummaryText(CampaignFrontSnapshot campaign)
    {
        return $"{campaign.FrontLabel}：前线{campaign.FrontPressure}，粮道{campaign.SupplyState}（{campaign.SupplyStateLabel}），军心{campaign.MoraleState}（{campaign.MoraleStateLabel}）。";
    }

    private static string BuildCampaignMobilizationSummaryText(CampaignFrontSnapshot campaign)
    {
        return $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowRegionalChinese(campaign.MobilizationWindowLabel)}。";
    }

    private static string BuildMobilizationSignalForceSummaryText(CampaignMobilizationSignalSnapshot signal)
    {
        return $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。";
    }

    private static string BuildMobilizationSignalOfficeSummaryText(CampaignMobilizationSignalSnapshot signal)
    {
        return signal.OfficeAuthorityTier > 0
            ? $"官署层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。"
            : "暂无官署文移接应。";
    }

    private static WarfareSurfaceViewModel BuildWarfareSurface(PresentationReadModelBundle bundle)
    {
        if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
        {
            return new WarfareSurfaceViewModel
            {
                Summary = "暂无军务沙盘投影。",
            };
        }

        int activeCampaignCount = bundle.Campaigns.Count(static campaign => campaign.IsActive);
        int peakFrontPressure = bundle.Campaigns.Count == 0 ? 0 : bundle.Campaigns.Max(static campaign => campaign.FrontPressure);

        return new WarfareSurfaceViewModel
        {
            Summary = BuildWarfareSurfaceSummary(bundle, activeCampaignCount, peakFrontPressure),
            CampaignBoards = bundle.Campaigns
                .OrderByDescending(static campaign => campaign.IsActive)
                .ThenByDescending(static campaign => campaign.FrontPressure)
                .ThenBy(static campaign => campaign.CampaignId.Value)
                .Select(static campaign => new CampaignBoardViewModel
                {
                    CampaignName = campaign.CampaignName,
                    SettlementLabel = campaign.AnchorSettlementName,
                    StatusLabel = campaign.IsActive ? "行营在案" : "战后覆核",
                    FrontLabel = campaign.FrontLabel,
                    SupplyStateLabel = campaign.SupplyStateLabel,
                    MoraleStateLabel = campaign.MoraleStateLabel,
                    CommandFitLabel = campaign.CommandFitLabel,
                    DirectiveLabel = campaign.ActiveDirectiveLabel,
                    DirectiveSummary = campaign.ActiveDirectiveSummary,
                    DirectiveTrace = campaign.LastDirectiveTrace,
                    ObjectiveSummary = campaign.ObjectiveSummary,
                    FrontSummary = $"{campaign.FrontLabel}：前线{campaign.FrontPressure}，粮道{campaign.SupplyState}（{campaign.SupplyStateLabel}），军心{campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
                    MobilizationSummary = $"应调之众{campaign.MobilizedForceCount}，动员窗{campaign.MobilizationWindowLabel}。",
                    SupplyLineSummary = campaign.SupplyLineSummary,
                    CoordinationSummary = campaign.OfficeCoordinationTrace,
                    CommanderSummary = campaign.CommanderSummary,
                    AftermathSummary = campaign.LastAftermathSummary,
                    Routes = campaign.Routes
                        .Select(static route => new CampaignRouteViewModel
                        {
                            RouteLabel = route.RouteLabel,
                            RouteRole = route.RouteRole,
                            FlowStateLabel = route.FlowStateLabel,
                            Summary = route.Summary,
                        })
                        .ToArray(),
                })
                .ToArray(),
            MobilizationSignals = bundle.CampaignMobilizationSignals
                .OrderByDescending(static signal => signal.ResponseActivationLevel)
                .ThenBy(static signal => signal.SettlementId.Value)
                .Select(static signal => new CampaignMobilizationSignalViewModel
                {
                    SettlementLabel = signal.SettlementName,
                    WindowLabel = signal.MobilizationWindowLabel,
                    ForceSummary = $"应调之众{signal.AvailableForceCount}，整备{signal.Readiness}，统摄{signal.CommandCapacity}。",
                    CommandFitLabel = signal.CommandFitLabel,
                    DirectiveLabel = signal.ActiveDirectiveLabel,
                    DirectiveSummary = signal.ActiveDirectiveSummary,
                    OfficeSummary = signal.OfficeAuthorityTier > 0
                        ? $"官署层级{signal.OfficeAuthorityTier}，杠杆{signal.AdministrativeLeverage}，积案{signal.PetitionBacklog}。"
                        : "暂无官署文移接应。",
                    SourceTrace = signal.SourceTrace,
                })
                .ToArray(),
        };
    }

    private static string BuildGreatHallWarfareSummary(PresentationReadModelBundle bundle)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();

        return
            $"现有{bundle.Campaigns.Count(static campaign => campaign.IsActive)}处在案行营；" +
            $"{leadCampaign.AnchorSettlementName}当前为{leadCampaign.FrontLabel}，{leadCampaign.SupplyStateLabel}，{leadCampaign.CommandFitLabel}。";
    }

    private static string BuildSettlementCampaignSummary(
        CampaignFrontSnapshot? campaign,
        CampaignMobilizationSignalSnapshot? signal)
    {
        if (campaign is not null)
        {
            CampaignRouteSnapshot? leadRoute = campaign.Routes.FirstOrDefault();
            string routeSummary = leadRoute is null
                ? "暂无路况细目"
                : RenderCampaignSurfaceText($"{leadRoute.RouteLabel}{leadRoute.FlowStateLabel}");
            return
                $"{campaign.CampaignName}：{campaign.FrontLabel}，{campaign.CommandFitLabel}，军令{campaign.ActiveDirectiveLabel}，{routeSummary}。";
        }

        if (signal is not null)
        {
            return
                $"{signal.MobilizationWindowLabel}动员窗，应调之众{signal.AvailableForceCount}，整备{signal.Readiness}，统摄{signal.CommandCapacity}，军令{signal.ActiveDirectiveLabel}。";
        }

        return "暂无军务沙盘投影。";
    }

    private static string BuildWarfareSurfaceSummary(
        PresentationReadModelBundle bundle,
        int activeCampaignCount,
        int peakFrontPressure)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();

        return
            $"现有{activeCampaignCount}处在案行营，峰值前线压力{peakFrontPressure}；" +
            $"{leadCampaign.AnchorSettlementName}正在以{leadCampaign.CommandFitLabel}维持{leadCampaign.FrontLabel}。";
    }

    private static NotificationCenterViewModel BuildNotificationCenter(IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        return new NotificationCenterViewModel
        {
            Items = notifications
                .Select(static notification => new NotificationItemViewModel
                {
                    Title = notification.Title,
                    Summary = notification.Summary,
                    WhyItHappened = notification.WhyItHappened,
                    WhatNext = notification.WhatNext,
                    TierLabel = notification.Tier.ToString(),
                    SurfaceLabel = notification.Surface.ToString(),
                    SourceModuleKey = notification.SourceModuleKey,
                    TraceCount = notification.Traces.Count,
                })
                .ToArray(),
        };
    }

    private static WarfareSurfaceViewModel BuildWarfareSurfaceChinese(PresentationReadModelBundle bundle)
    {
        if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
        {
            return new WarfareSurfaceViewModel
            {
                Summary = "暂无军务沙盘投影。",
            };
        }

        int activeCampaignCount = bundle.Campaigns.Count(static campaign => campaign.IsActive);
        int peakFrontPressure = bundle.Campaigns.Count == 0 ? 0 : bundle.Campaigns.Max(static campaign => campaign.FrontPressure);

        return new WarfareSurfaceViewModel
        {
            Summary = BuildWarfareSurfaceSummaryChinese(bundle, activeCampaignCount, peakFrontPressure),
            CampaignBoards = bundle.Campaigns
                .OrderByDescending(static campaign => campaign.IsActive)
                .ThenByDescending(static campaign => campaign.FrontPressure)
                .ThenBy(static campaign => campaign.CampaignId.Value)
                .Select(static campaign => new CampaignBoardViewModel
                {
                    CampaignName = campaign.CampaignName,
                    SettlementLabel = campaign.AnchorSettlementName,
                    StatusLabel = campaign.IsActive ? "行营在案" : "战后覆核",
                    FrontLabel = campaign.FrontLabel,
                    SupplyStateLabel = campaign.SupplyStateLabel,
                    MoraleStateLabel = campaign.MoraleStateLabel,
                    CommandFitLabel = campaign.CommandFitLabel,
                    DirectiveLabel = campaign.ActiveDirectiveLabel,
                    DirectiveSummary = campaign.ActiveDirectiveSummary,
                    DirectiveTrace = campaign.LastDirectiveTrace,
                    ObjectiveSummary = campaign.ObjectiveSummary,
                    FrontSummary = $"{campaign.FrontLabel}：前线 {campaign.FrontPressure}，粮道 {campaign.SupplyState}（{campaign.SupplyStateLabel}），军心 {campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
                    MobilizationSummary = $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowChinese(campaign.MobilizationWindowLabel)}。",
                    SupplyLineSummary = campaign.SupplyLineSummary,
                    CoordinationSummary = campaign.OfficeCoordinationTrace,
                    CommanderSummary = campaign.CommanderSummary,
                    AftermathSummary = campaign.LastAftermathSummary,
                    Routes = campaign.Routes
                        .Select(static route => new CampaignRouteViewModel
                        {
                            RouteLabel = route.RouteLabel,
                            RouteRole = route.RouteRole,
                            FlowStateLabel = route.FlowStateLabel,
                            Summary = route.Summary,
                        })
                        .ToArray(),
                })
                .ToArray(),
            MobilizationSignals = bundle.CampaignMobilizationSignals
                .OrderByDescending(static signal => signal.ResponseActivationLevel)
                .ThenBy(static signal => signal.SettlementId.Value)
                .Select(static signal => new CampaignMobilizationSignalViewModel
                {
                    SettlementLabel = signal.SettlementName,
                    WindowLabel = DescribeMobilizationWindowChinese(signal.MobilizationWindowLabel),
                    ForceSummary = $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。",
                    CommandFitLabel = signal.CommandFitLabel,
                    DirectiveLabel = signal.ActiveDirectiveLabel,
                    DirectiveSummary = signal.ActiveDirectiveSummary,
                    OfficeSummary = signal.OfficeAuthorityTier > 0
                        ? $"官署层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。"
                        : "暂无官署文移接应。",
                    SourceTrace = signal.SourceTrace,
                })
                .ToArray(),
        };
    }

    private static string BuildGreatHallWarfareSummaryChinese(PresentationReadModelBundle bundle)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();

        return
            $"现有 {bundle.Campaigns.Count(static campaign => campaign.IsActive)} 处在案行营；" +
            $"{leadCampaign.AnchorSettlementName}当前为 {leadCampaign.FrontLabel}、{leadCampaign.SupplyStateLabel}、{leadCampaign.CommandFitLabel}。";
    }

    private static string BuildSettlementCampaignSummaryChinese(
        CampaignFrontSnapshot? campaign,
        CampaignMobilizationSignalSnapshot? signal)
    {
        if (campaign is not null)
        {
            CampaignRouteSnapshot? leadRoute = campaign.Routes.FirstOrDefault();
            string routeSummary = leadRoute is null
                ? "暂无路况细目"
                : $"{leadRoute.RouteLabel}{leadRoute.FlowStateLabel}";
            return
                $"{campaign.CampaignName}，{campaign.FrontLabel}，{campaign.CommandFitLabel}，军令 {campaign.ActiveDirectiveLabel}，{routeSummary}。";
        }

        if (signal is not null)
        {
            return
                $"{DescribeMobilizationWindowChinese(signal.MobilizationWindowLabel)}动员窗，应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}，军令 {signal.ActiveDirectiveLabel}。";
        }

        return "暂无军务沙盘投影。";
    }

    private static string BuildWarfareSurfaceSummaryChinese(
        PresentationReadModelBundle bundle,
        int activeCampaignCount,
        int peakFrontPressure)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();

        return
            $"现有 {activeCampaignCount} 处在案行营，峰值前线压力 {peakFrontPressure}；" +
            $"{leadCampaign.AnchorSettlementName}正以 {leadCampaign.CommandFitLabel} 维持 {leadCampaign.FrontLabel}。";
    }

    private static string DescribeMobilizationWindowChinese(string mobilizationWindowLabel)
    {
        return mobilizationWindowLabel switch
        {
            "Open" => "可开",
            "Narrow" => "可守",
            "Preparing" => "待整",
            _ => "已闭",
        };
    }

    private static WarfareSurfaceViewModel BuildWarfareSurfaceReactiveChinese(PresentationReadModelBundle bundle)
    {
        if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
        {
            return new WarfareSurfaceViewModel
            {
                Summary = "暂无军务沙盘投影。",
            };
        }

        int activeCampaignCount = bundle.Campaigns.Count(static campaign => campaign.IsActive);
        int peakFrontPressure = bundle.Campaigns.Count == 0 ? 0 : bundle.Campaigns.Max(static campaign => campaign.FrontPressure);

        return new WarfareSurfaceViewModel
        {
            Summary = BuildWarfareSurfaceSummaryReactiveChinese(bundle, activeCampaignCount, peakFrontPressure),
            CampaignBoards = bundle.Campaigns
                .OrderByDescending(static campaign => campaign.IsActive)
                .ThenByDescending(static campaign => campaign.FrontPressure)
                .ThenBy(static campaign => campaign.CampaignId.Value)
                .Select(static campaign => new CampaignBoardViewModel
                {
                    CampaignName = campaign.CampaignName,
                    SettlementLabel = campaign.AnchorSettlementName,
                    StatusLabel = campaign.IsActive ? "行营在案" : "战后覆核",
                    EnvironmentLabel = BuildCampaignBoardEnvironmentLabel(campaign),
                    BoardSurfaceLabel = BuildCampaignBoardSurfaceLabel(campaign),
                    BoardAtmosphereSummary = BuildCampaignBoardAtmosphereSummary(campaign),
                    MarkerSummary = BuildCampaignBoardMarkerSummary(campaign),
                    FrontLabel = campaign.FrontLabel,
                    SupplyStateLabel = campaign.SupplyStateLabel,
                    MoraleStateLabel = campaign.MoraleStateLabel,
                    CommandFitLabel = campaign.CommandFitLabel,
                    DirectiveLabel = campaign.ActiveDirectiveLabel,
                    DirectiveSummary = campaign.ActiveDirectiveSummary,
                    DirectiveTrace = campaign.LastDirectiveTrace,
                    ObjectiveSummary = campaign.ObjectiveSummary,
                    FrontSummary = $"{campaign.FrontLabel}：前线 {campaign.FrontPressure}，粮道 {campaign.SupplyState}（{campaign.SupplyStateLabel}），军心 {campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
                    MobilizationSummary = $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowReactiveChinese(campaign.MobilizationWindowLabel)}。",
                    SupplyLineSummary = campaign.SupplyLineSummary,
                    CoordinationSummary = campaign.OfficeCoordinationTrace,
                    CommanderSummary = campaign.CommanderSummary,
                    AftermathSummary = campaign.LastAftermathSummary,
                    Routes = campaign.Routes
                        .Select(static route => new CampaignRouteViewModel
                        {
                            RouteLabel = route.RouteLabel,
                            RouteRole = route.RouteRole,
                            FlowStateLabel = route.FlowStateLabel,
                            Summary = route.Summary,
                        })
                        .ToArray(),
                })
                .ToArray(),
            MobilizationSignals = bundle.CampaignMobilizationSignals
                .OrderByDescending(static signal => signal.ResponseActivationLevel)
                .ThenBy(static signal => signal.SettlementId.Value)
                .Select(static signal => new CampaignMobilizationSignalViewModel
                {
                    SettlementLabel = signal.SettlementName,
                    WindowLabel = DescribeMobilizationWindowReactiveChinese(signal.MobilizationWindowLabel),
                    ForceSummary = $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。",
                    CommandFitLabel = signal.CommandFitLabel,
                    DirectiveLabel = signal.ActiveDirectiveLabel,
                    DirectiveSummary = signal.ActiveDirectiveSummary,
                    OfficeSummary = signal.OfficeAuthorityTier > 0
                        ? $"官绅层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。"
                        : "暂无官绅文移接应。",
                    SourceTrace = signal.SourceTrace,
                })
                .ToArray(),
        };
    }

    private static string BuildGreatHallWarfareSummaryReactiveChinese(PresentationReadModelBundle bundle)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();

        return
            $"现有 {bundle.Campaigns.Count(static campaign => campaign.IsActive)} 处在案行营；" +
            $"{leadCampaign.AnchorSettlementName}当前{leadCampaign.FrontLabel}、{leadCampaign.SupplyStateLabel}，案头呈{BuildCampaignBoardEnvironmentLabel(leadCampaign)}。";
    }

    private static string BuildSettlementCampaignSummaryReactiveChinese(
        CampaignFrontSnapshot? campaign,
        CampaignMobilizationSignalSnapshot? signal)
    {
        if (campaign is not null)
        {
            CampaignRouteSnapshot? leadRoute = campaign.Routes.FirstOrDefault();
            string routeSummary = leadRoute is null
                ? "暂无路况细目"
                : $"{leadRoute.RouteLabel}{leadRoute.FlowStateLabel}";
            return $"{campaign.CampaignName}：{BuildCampaignBoardEnvironmentLabel(campaign)}，{campaign.ActiveDirectiveLabel}，{routeSummary}。";
        }

        if (signal is not null)
        {
            return
                $"{DescribeMobilizationWindowReactiveChinese(signal.MobilizationWindowLabel)}动员窗，应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}，军令 {signal.ActiveDirectiveLabel}。";
        }

        return "暂无军务沙盘投影。";
    }

    private static string BuildWarfareSurfaceSummaryReactiveChinese(
        PresentationReadModelBundle bundle,
        int activeCampaignCount,
        int peakFrontPressure)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();

        return
            $"现有 {activeCampaignCount} 处在案行营，峰值前线压力 {peakFrontPressure}；" +
            $"{leadCampaign.AnchorSettlementName}正以 {leadCampaign.CommandFitLabel} 维持 {leadCampaign.FrontLabel}，案头呈{BuildCampaignBoardEnvironmentLabel(leadCampaign)}。";
    }

    private static string DescribeMobilizationWindowReactiveChinese(string mobilizationWindowLabel)
    {
        return mobilizationWindowLabel switch
        {
            "Open" => "可开",
            "Narrow" => "可守",
            "Preparing" => "待整",
            _ => "已闭",
        };
    }

    private static string BuildCampaignBoardEnvironmentLabel(CampaignFrontSnapshot campaign)
    {
        if (!campaign.IsActive)
        {
            return "收卷校核";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.WithdrawToBarracks, StringComparison.Ordinal))
        {
            return "收军归营";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.CommitMobilization, StringComparison.Ordinal))
        {
            return "鼓角催集";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.DraftCampaignPlan, StringComparison.Ordinal))
        {
            return "舆图铺案";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.ProtectSupplyLine, StringComparison.Ordinal))
        {
            return campaign.FrontPressure >= 60 ? "粮筹压阵" : "粮驿先行";
        }

        if (campaign.FrontPressure >= 75 && campaign.MoraleState < 45)
        {
            return "烽尘压案";
        }

        if (campaign.SupplyState < 40)
        {
            return "粮线告急";
        }

        if (campaign.MoraleState < 40)
        {
            return "军心低徊";
        }

        return "行营在案";
    }

    private static string BuildCampaignBoardSurfaceLabel(CampaignFrontSnapshot campaign)
    {
        CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);

        if (!campaign.IsActive)
        {
            return "营旗收束，后营册页、伤损簿与善后批答占住案心。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.ProtectSupplyLine, StringComparison.Ordinal))
        {
            return leadRoute is null
                ? "粮签、渡口木筹与护运行签压在案心，前锋旗退到边角。"
                : $"案心多是{leadRoute.RouteLabel}筹签，前锋旗让位于护运与驿递木筹。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.CommitMobilization, StringComparison.Ordinal))
        {
            return "点军签、乡勇册与营旗铺开，案边尽是催集批注。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.DraftCampaignPlan, StringComparison.Ordinal))
        {
            return "舆图平展，朱笔批注多于军旗，先看路线后议进退。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.WithdrawToBarracks, StringComparison.Ordinal))
        {
            return "营旗后撤，路签收束，案面转向后营与善后册页。";
        }

        if (campaign.FrontPressure >= campaign.SupplyState)
        {
            return "前锋旗与急报纸条堆向案前，粮签紧贴其后。";
        }

        return "粮签、驿筹与渡口木牌并列案心，前锋旗压在边沿。";
    }

    private static string BuildCampaignBoardAtmosphereSummary(CampaignFrontSnapshot campaign)
    {
        if (!campaign.IsActive)
        {
            return $"此案已转为战后覆核：{campaign.LastAftermathSummary}";
        }

        return
            $"此案呈{BuildCampaignBoardEnvironmentLabel(campaign)}之势：{campaign.FrontLabel}，{campaign.SupplyStateLabel}，{campaign.MoraleStateLabel}；" +
            $"军令为{campaign.ActiveDirectiveLabel}，{DescribeCampaignRouteMix(campaign)}。";
    }

    private static string BuildCampaignBoardMarkerSummary(CampaignFrontSnapshot campaign)
    {
        CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
        if (leadRoute is null)
        {
            return "案上主要凭前锋旗、粮簿与军报批注辨势。";
        }

        CampaignRouteSnapshot? secondaryRoute = campaign.Routes
            .OrderByDescending(static route => route.Pressure)
            .ThenByDescending(static route => route.Security)
            .ThenBy(static route => route.RouteLabel, StringComparer.Ordinal)
            .Skip(1)
            .FirstOrDefault();

        if (secondaryRoute is null)
        {
            return $"案头主线落在{leadRoute.RouteLabel}，其势为{leadRoute.FlowStateLabel}。";
        }

        return $"案头以{leadRoute.RouteLabel}为主线，并由{secondaryRoute.RouteLabel}牵住侧边。";
    }

    private static string DescribeCampaignRouteMix(CampaignFrontSnapshot campaign)
    {
        CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
        if (leadRoute is null)
        {
            return "案上仍以军报和前锋旗为主";
        }

        CampaignRouteSnapshot? secondaryRoute = campaign.Routes
            .OrderByDescending(static route => route.Pressure)
            .ThenByDescending(static route => route.Security)
            .ThenBy(static route => route.RouteLabel, StringComparer.Ordinal)
            .Skip(1)
            .FirstOrDefault();

        if (secondaryRoute is null)
        {
            return $"主看{leadRoute.RouteLabel}，其势{leadRoute.FlowStateLabel}";
        }

        return $"主看{leadRoute.RouteLabel}，并以{secondaryRoute.RouteLabel}牵住侧边";
    }

    private static CampaignRouteSnapshot? SelectLeadCampaignRoute(CampaignFrontSnapshot campaign)
    {
        return campaign.Routes
            .OrderByDescending(static route => route.Pressure)
            .ThenByDescending(static route => route.Security)
            .ThenBy(static route => route.RouteLabel, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static WarfareSurfaceViewModel BuildWarfareSurfaceRegionalChinese(
        PresentationReadModelBundle bundle,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        if (bundle.Campaigns.Count == 0 && bundle.CampaignMobilizationSignals.Count == 0)
        {
            return new WarfareSurfaceViewModel
            {
                Summary = "暂无军务沙盘投影。",
            };
        }

        int activeCampaignCount = bundle.Campaigns.Count(static campaign => campaign.IsActive);
        int peakFrontPressure = bundle.Campaigns.Count == 0 ? 0 : bundle.Campaigns.Max(static campaign => campaign.FrontPressure);
        Dictionary<int, SettlementSnapshot> settlementsById = bundle.Settlements
            .ToDictionary(static settlement => settlement.Id.Value, static settlement => settlement);
        ILookup<int, TradeRouteSnapshot> tradeRoutesBySettlement = bundle.TradeRoutes
            .ToLookup(static route => route.SettlementId.Value);

        return new WarfareSurfaceViewModel
        {
            Summary = BuildWarfareSurfaceSummaryRegionalChinese(bundle, activeCampaignCount, peakFrontPressure),
            CampaignBoards = bundle.Campaigns
                .OrderByDescending(static campaign => campaign.IsActive)
                .ThenByDescending(static campaign => campaign.FrontPressure)
                .ThenBy(static campaign => campaign.CampaignId.Value)
                .Select(campaign =>
                {
                    SettlementSnapshot? settlement = settlementsById.TryGetValue(campaign.AnchorSettlementId.Value, out SettlementSnapshot? snapshot)
                        ? snapshot
                        : null;
                    TradeRouteSnapshot[] tradeRoutes = tradeRoutesBySettlement[campaign.AnchorSettlementId.Value]
                        .OrderBy(static route => route.RouteName, StringComparer.Ordinal)
                        .ToArray();
                    RegionalBoardProfile regionalProfile = BuildCampaignRegionalProfile(campaign, settlement, tradeRoutes);

                    return new CampaignBoardViewModel
                    {
                        CampaignName = RenderCampaignSurfaceText(campaign.CampaignName),
                        SettlementLabel = campaign.AnchorSettlementName,
                        StatusLabel = campaign.IsActive ? "行营在案" : "战后覆核",
                        RegionalProfileLabel = regionalProfile.Label,
                        RegionalBackdropSummary = regionalProfile.BackdropSummary,
                        EnvironmentLabel = BuildCampaignConditionLabelChinese(campaign),
                        BoardSurfaceLabel = BuildCampaignBoardSurfaceRegionalChinese(campaign, regionalProfile),
                        BoardAtmosphereSummary = BuildCampaignBoardAtmosphereRegionalChinese(campaign, regionalProfile),
                        MarkerSummary = BuildCampaignBoardMarkerRegionalChinese(campaign, regionalProfile),
                        FrontLabel = campaign.FrontLabel,
                        SupplyStateLabel = campaign.SupplyStateLabel,
                        MoraleStateLabel = campaign.MoraleStateLabel,
                        CommandFitLabel = campaign.CommandFitLabel,
                        DirectiveLabel = campaign.ActiveDirectiveLabel,
                        DirectiveSummary = RenderCampaignSurfaceText(campaign.ActiveDirectiveSummary),
                        DirectiveTrace = RenderCampaignSurfaceText(campaign.LastDirectiveTrace),
                        ObjectiveSummary = RenderCampaignSurfaceText(campaign.ObjectiveSummary),
                        FrontSummary = $"{campaign.FrontLabel}：前线 {campaign.FrontPressure}，粮道 {campaign.SupplyState}（{campaign.SupplyStateLabel}），军心 {campaign.MoraleState}（{campaign.MoraleStateLabel}）。",
                        MobilizationSummary = $"应调之众 {campaign.MobilizedForceCount}，动员窗 {DescribeMobilizationWindowRegionalChinese(campaign.MobilizationWindowLabel)}。",
                        SupplyLineSummary = RenderCampaignSurfaceText(campaign.SupplyLineSummary),
                        CoordinationSummary = RenderCampaignSurfaceText(campaign.OfficeCoordinationTrace),
                        CommanderSummary = RenderCampaignSurfaceText(campaign.CommanderSummary),
                        AftermathSummary = RenderCampaignSurfaceText(campaign.LastAftermathSummary),
                        AftermathDocketSummary = BuildCampaignAftermathDocketSummary(
                            campaign,
                            settlement,
                            notifications),
                        Routes = campaign.Routes
                            .Select(static route => new CampaignRouteViewModel
                            {
                                RouteLabel = route.RouteLabel,
                                RouteRole = route.RouteRole,
                                FlowStateLabel = route.FlowStateLabel,
                                Summary = RenderCampaignSurfaceText(route.Summary),
                            })
                            .ToArray(),
                    };
                })
                .ToArray(),
            MobilizationSignals = bundle.CampaignMobilizationSignals
                .OrderByDescending(static signal => signal.ResponseActivationLevel)
                .ThenBy(static signal => signal.SettlementId.Value)
                .Select(static signal => new CampaignMobilizationSignalViewModel
                {
                    SettlementLabel = signal.SettlementName,
                    WindowLabel = DescribeMobilizationWindowRegionalChinese(signal.MobilizationWindowLabel),
                    ForceSummary = $"应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}。",
                    CommandFitLabel = signal.CommandFitLabel,
                    DirectiveLabel = signal.ActiveDirectiveLabel,
                    DirectiveSummary = RenderCampaignSurfaceText(signal.ActiveDirectiveSummary),
                    OfficeSummary = signal.OfficeAuthorityTier > 0
                        ? $"官绅层级 {signal.OfficeAuthorityTier}，杠杆 {signal.AdministrativeLeverage}，积案 {signal.PetitionBacklog}。"
                        : "暂无官绅文移接应。",
                    SourceTrace = RenderCampaignSurfaceText(signal.SourceTrace),
                })
                .ToArray(),
        };
    }

    private static string BuildGreatHallWarfareSummaryRegionalChinese(PresentationReadModelBundle bundle)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();
        SettlementSnapshot? settlement = bundle.Settlements.FirstOrDefault(candidate => candidate.Id == leadCampaign.AnchorSettlementId);
        TradeRouteSnapshot[] tradeRoutes = bundle.TradeRoutes
            .Where(route => route.SettlementId == leadCampaign.AnchorSettlementId)
            .OrderBy(static route => route.RouteName, StringComparer.Ordinal)
            .ToArray();
        RegionalBoardProfile regionalProfile = BuildCampaignRegionalProfile(leadCampaign, settlement, tradeRoutes);

        return
            $"现有 {bundle.Campaigns.Count(static campaign => campaign.IsActive)} 处在案行营；" +
            $"{leadCampaign.AnchorSettlementName}当前{leadCampaign.FrontLabel}、{leadCampaign.SupplyStateLabel}，属{regionalProfile.Label}之局，案头呈{BuildCampaignConditionLabelChinese(leadCampaign)}。";
    }

    private static string BuildGreatHallAftermathDocketSummary(
        PresentationReadModelBundle bundle,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "堂上尚无战后案牍。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();
        SettlementSnapshot? settlement = bundle.Settlements.FirstOrDefault(candidate => candidate.Id == leadCampaign.AnchorSettlementId);
        PopulationSettlementSnapshot? population = bundle.PopulationSettlements.FirstOrDefault(candidate => candidate.SettlementId == leadCampaign.AnchorSettlementId);
        JurisdictionAuthoritySnapshot? jurisdiction = bundle.OfficeJurisdictions.FirstOrDefault(candidate => candidate.SettlementId == leadCampaign.AnchorSettlementId);
        AftermathDocketSignals signals = BuildAftermathDocketSignals(
            leadCampaign.AnchorSettlementId,
            settlement,
            population,
            jurisdiction,
            leadCampaign,
            notifications);

        if (!signals.HasSignals)
        {
            return $"{leadCampaign.AnchorSettlementName}堂案仍以军报与粮道札记为主。";
        }

        return $"{leadCampaign.AnchorSettlementName}堂案今并载{signals.ComposeClauseText(useArticle: false)}。";
    }

    private static string BuildSettlementCampaignSummaryRegionalChinese(
        CampaignFrontSnapshot? campaign,
        CampaignMobilizationSignalSnapshot? signal,
        SettlementSnapshot settlement,
        IReadOnlyList<TradeRouteSnapshot> tradeRoutes)
    {
        if (campaign is not null)
        {
            RegionalBoardProfile regionalProfile = BuildCampaignRegionalProfile(campaign, settlement, tradeRoutes);
            CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
            string routeSummary = leadRoute is null
                ? "暂无路况细目"
                : $"{leadRoute.RouteLabel}{leadRoute.FlowStateLabel}";
            return $"{campaign.CampaignName}：{regionalProfile.Label}，{BuildCampaignConditionLabelChinese(campaign)}，{routeSummary}。";
        }

        if (signal is not null)
        {
            return
                $"{DescribeMobilizationWindowRegionalChinese(signal.MobilizationWindowLabel)}动员窗，应调之众 {signal.AvailableForceCount}，整备 {signal.Readiness}，统摄 {signal.CommandCapacity}，军令 {signal.ActiveDirectiveLabel}。";
        }

        return "暂无军务沙盘投影。";
    }

    private static string BuildSettlementAftermathSummary(
        SettlementSnapshot settlement,
        PopulationSettlementSnapshot? population,
        JurisdictionAuthoritySnapshot? jurisdiction,
        CampaignFrontSnapshot? campaign,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        AftermathDocketSignals signals = BuildAftermathDocketSignals(
            settlement.Id,
            settlement,
            population,
            jurisdiction,
            campaign,
            notifications);

        if (!signals.HasSignals)
        {
            return "战后案牍未起。";
        }

        return $"战后案牍：{signals.ComposeClauseText(useArticle: true)}。";
    }

    private static string BuildCampaignAftermathDocketSummary(
        CampaignFrontSnapshot campaign,
        SettlementSnapshot? settlement,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        AftermathDocketSignals signals = BuildAftermathDocketSignals(
            campaign.AnchorSettlementId,
            settlement,
            null,
            null,
            campaign,
            notifications);

        if (!signals.HasSignals)
        {
            return campaign.IsActive
                ? "军机案下仍止于军报与路报。"
                : "军机案下尚未并成赏罚抚恤诸册。";
        }

        return $"军机案今并载{signals.ComposeClauseText(useArticle: false)}。";
    }

    private static AftermathDocketSignals BuildAftermathDocketSignals(
        SettlementId settlementId,
        SettlementSnapshot? settlement,
        PopulationSettlementSnapshot? population,
        JurisdictionAuthoritySnapshot? jurisdiction,
        CampaignFrontSnapshot? campaign,
        IReadOnlyList<NarrativeNotificationSnapshot> notifications)
    {
        string settlementKey = settlementId.Value.ToString();
        NarrativeNotificationSnapshot[] relatedNotifications = notifications
            .Where(notification =>
                string.Equals(notification.SourceModuleKey, KnownModuleKeys.WarfareCampaign, StringComparison.Ordinal)
                || notification.Traces.Any(trace => string.Equals(trace.EntityKey, settlementKey, StringComparison.Ordinal)))
            .ToArray();

        bool merit = relatedNotifications.Any(notification => notification.Traces.Any(trace =>
                string.Equals(trace.SourceModuleKey, KnownModuleKeys.FamilyCore, StringComparison.Ordinal)
                || string.Equals(trace.SourceModuleKey, KnownModuleKeys.SocialMemoryAndRelations, StringComparison.Ordinal)))
            || (campaign is not null && campaign.IsActive && campaign.MoraleState >= 55 && campaign.SupplyState >= 50);
        bool blame = relatedNotifications.Any(notification => notification.Traces.Any(trace =>
                string.Equals(trace.SourceModuleKey, KnownModuleKeys.OfficeAndCareer, StringComparison.Ordinal)
                || string.Equals(trace.SourceModuleKey, KnownModuleKeys.ConflictAndForce, StringComparison.Ordinal)))
            || (campaign is not null && (!campaign.IsActive || campaign.FrontPressure >= 60))
            || (jurisdiction?.PetitionBacklog ?? 0) >= 8;
        bool relief = relatedNotifications.Any(notification => notification.Traces.Any(trace =>
                string.Equals(trace.SourceModuleKey, KnownModuleKeys.PopulationAndHouseholds, StringComparison.Ordinal)
                || string.Equals(trace.SourceModuleKey, KnownModuleKeys.WorldSettlements, StringComparison.Ordinal)))
            || (population is not null && (population.CommonerDistress >= 40 || population.MigrationPressure >= 35))
            || (settlement is not null && (settlement.Security <= 55 || settlement.Prosperity <= 58));
        bool disorder = relatedNotifications.Any(notification => notification.Traces.Any(trace =>
                string.Equals(trace.SourceModuleKey, KnownModuleKeys.OrderAndBanditry, StringComparison.Ordinal)
                || string.Equals(trace.SourceModuleKey, KnownModuleKeys.TradeAndIndustry, StringComparison.Ordinal)))
            || (settlement is not null && settlement.Security < 58)
            || (campaign is not null && (campaign.SupplyState < 45 || campaign.Routes.Any(static route => route.Pressure > route.Security)));

        return new AftermathDocketSignals
        {
            Merit = merit,
            Blame = blame,
            Relief = relief,
            Disorder = disorder,
        };
    }

    private static string BuildWarfareSurfaceSummaryRegionalChinese(
        PresentationReadModelBundle bundle,
        int activeCampaignCount,
        int peakFrontPressure)
    {
        if (bundle.Campaigns.Count == 0)
        {
            return "暂无军务沙盘投影。";
        }

        CampaignFrontSnapshot leadCampaign = bundle.Campaigns
            .OrderByDescending(static campaign => campaign.FrontPressure)
            .ThenByDescending(static campaign => campaign.MobilizedForceCount)
            .ThenBy(static campaign => campaign.CampaignId.Value)
            .First();
        SettlementSnapshot? settlement = bundle.Settlements.FirstOrDefault(candidate => candidate.Id == leadCampaign.AnchorSettlementId);
        TradeRouteSnapshot[] tradeRoutes = bundle.TradeRoutes
            .Where(route => route.SettlementId == leadCampaign.AnchorSettlementId)
            .OrderBy(static route => route.RouteName, StringComparer.Ordinal)
            .ToArray();
        RegionalBoardProfile regionalProfile = BuildCampaignRegionalProfile(leadCampaign, settlement, tradeRoutes);

        return
            $"现有 {activeCampaignCount} 处在案行营，峰值前线压力 {peakFrontPressure}；" +
            $"{leadCampaign.AnchorSettlementName}正以 {leadCampaign.CommandFitLabel} 维持 {leadCampaign.FrontLabel}，属{regionalProfile.Label}之局。";
    }

    private static string DescribeMobilizationWindowRegionalChinese(string mobilizationWindowLabel)
    {
        return mobilizationWindowLabel switch
        {
            "Open" => "可开",
            "Narrow" => "可守",
            "Preparing" => "待整",
            _ => "已闭",
        };
    }

    private static string BuildCampaignConditionLabelChinese(CampaignFrontSnapshot campaign)
    {
        if (!campaign.IsActive)
        {
            return "收卷校核";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.WithdrawToBarracks, StringComparison.Ordinal))
        {
            return "收军归营";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.CommitMobilization, StringComparison.Ordinal))
        {
            return "鼓角催集";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.DraftCampaignPlan, StringComparison.Ordinal))
        {
            return "舆图铺案";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.ProtectSupplyLine, StringComparison.Ordinal))
        {
            return campaign.FrontPressure >= 60 ? "粮筹压阵" : "粮驿先行";
        }

        if (campaign.FrontPressure >= 75 && campaign.MoraleState < 45)
        {
            return "烽尘压案";
        }

        if (campaign.SupplyState < 40)
        {
            return "粮线告急";
        }

        if (campaign.MoraleState < 40)
        {
            return "军心低徊";
        }

        return "行营在案";
    }

    private static string BuildCampaignBoardSurfaceRegionalChinese(CampaignFrontSnapshot campaign, RegionalBoardProfile regionalProfile)
    {
        string regionalBase = regionalProfile.BackdropSummary;
        CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);

        if (!campaign.IsActive)
        {
            return $"{regionalBase} 营旗收束，后营册页、伤损簿与善后批答占住案心。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.ProtectSupplyLine, StringComparison.Ordinal))
        {
            return leadRoute is null
                ? $"{regionalBase} 粮签、渡口木筹与护运行签压在案心，前锋旗退到边角。"
                : $"{regionalBase} 案心多是{leadRoute.RouteLabel}筹签，前锋旗让位于护运与驿递木筹。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.CommitMobilization, StringComparison.Ordinal))
        {
            return $"{regionalBase} 点军签、乡勇册与营旗铺开，案边尽是催集批注。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.DraftCampaignPlan, StringComparison.Ordinal))
        {
            return $"{regionalBase} 舆图平展，朱笔批注多于军旗，先看路线后议进退。";
        }

        if (string.Equals(campaign.ActiveDirectiveCode, WarfareCampaignCommandNames.WithdrawToBarracks, StringComparison.Ordinal))
        {
            return $"{regionalBase} 营旗后撤，路签收束，案面转向后营与善后册页。";
        }

        if (campaign.FrontPressure >= campaign.SupplyState)
        {
            return $"{regionalBase} 前锋旗与急报纸条堆向案前，粮签紧贴其后。";
        }

        return $"{regionalBase} 粮签、驿筹与渡口木牌并列案心，前锋旗压在边沿。";
    }

    private static string BuildCampaignBoardAtmosphereRegionalChinese(CampaignFrontSnapshot campaign, RegionalBoardProfile regionalProfile)
    {
        if (!campaign.IsActive)
        {
            return $"此案属{regionalProfile.Label}之局，已转为战后覆核：{campaign.LastAftermathSummary}";
        }

        return
            $"此案属{regionalProfile.Label}之局，呈{BuildCampaignConditionLabelChinese(campaign)}之势：{campaign.FrontLabel}，{campaign.SupplyStateLabel}，{campaign.MoraleStateLabel}；" +
            $"军令为{campaign.ActiveDirectiveLabel}，{DescribeCampaignRouteMixRegionalChinese(campaign)}。";
    }

    private static string BuildCampaignBoardMarkerRegionalChinese(CampaignFrontSnapshot campaign, RegionalBoardProfile regionalProfile)
    {
        CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
        if (leadRoute is null)
        {
            return $"{regionalProfile.Label}案主要凭前锋旗、粮簿与军报批注辨势。";
        }

        CampaignRouteSnapshot? secondaryRoute = campaign.Routes
            .OrderByDescending(static route => route.Pressure)
            .ThenByDescending(static route => route.Security)
            .ThenBy(static route => route.RouteLabel, StringComparer.Ordinal)
            .Skip(1)
            .FirstOrDefault();

        if (secondaryRoute is null)
        {
            return $"{regionalProfile.Label}案头主线落在{leadRoute.RouteLabel}，其势为{leadRoute.FlowStateLabel}。";
        }

        return $"{regionalProfile.Label}案头以{leadRoute.RouteLabel}为主线，并由{secondaryRoute.RouteLabel}牵住侧边。";
    }

    private static string DescribeCampaignRouteMixRegionalChinese(CampaignFrontSnapshot campaign)
    {
        CampaignRouteSnapshot? leadRoute = SelectLeadCampaignRoute(campaign);
        if (leadRoute is null)
        {
            return "案上仍以军报和前锋旗为主";
        }

        CampaignRouteSnapshot? secondaryRoute = campaign.Routes
            .OrderByDescending(static route => route.Pressure)
            .ThenByDescending(static route => route.Security)
            .ThenBy(static route => route.RouteLabel, StringComparer.Ordinal)
            .Skip(1)
            .FirstOrDefault();

        if (secondaryRoute is null)
        {
            return $"主看{leadRoute.RouteLabel}，其势{leadRoute.FlowStateLabel}";
        }

        return $"主看{leadRoute.RouteLabel}，并以{secondaryRoute.RouteLabel}牵住侧边";
    }

    private static RegionalBoardProfile BuildCampaignRegionalProfile(
        CampaignFrontSnapshot campaign,
        SettlementSnapshot? settlement = null,
        IReadOnlyList<TradeRouteSnapshot>? tradeRoutes = null)
    {
        string[] localSignals =
        [
            campaign.AnchorSettlementName,
            campaign.CampaignName,
            .. campaign.Routes.Select(static route => route.RouteLabel),
            .. campaign.Routes.Select(static route => route.Summary),
            .. (tradeRoutes ?? Array.Empty<TradeRouteSnapshot>()).Select(static route => route.RouteName),
        ];

        bool waterLinked = ContainsRegionalSignal(localSignals, "river", "canal", "ferry", "wharf", "water", "河", "江", "渡", "港", "浦", "漕");
        bool hillLinked = ContainsRegionalSignal(localSignals, "hill", "mountain", "ridge", "pass", "山", "岭", "关", "隘", "谷");
        int prosperity = settlement?.Prosperity ?? 0;
        int security = settlement?.Security ?? 0;

        if (waterLinked)
        {
            return prosperity >= 60
                ? new RegionalBoardProfile("水驿商埠", "案旁多铺水线、渡口木牌与舟楫筹。")
                : new RegionalBoardProfile("江渡县口", "案边常见渡津签、河埠木牌与泊船筹。");
        }

        if (hillLinked)
        {
            return security >= 50
                ? new RegionalBoardProfile("山道关路", "案面多画山道折线、岭口木塞与关津旗。")
                : new RegionalBoardProfile("岭道荒隘", "案边竖着山口木牌、险路折签与巡哨火签。");
        }

        if (prosperity >= 65)
        {
            return new RegionalBoardProfile("市镇腹地", "案旁多见仓牌、街市路签与税簿角注。");
        }

        if (security <= 45)
        {
            return new RegionalBoardProfile("边县危垒", "案边竖着塞门木牌、巡哨火签与守望短旗。");
        }

        return new RegionalBoardProfile("县城平畴", "案上以田畴路签、县门木牌与里坊册页为底。");
    }

    private static bool ContainsRegionalSignal(IEnumerable<string> signals, params string[] markers)
    {
        foreach (string signal in signals)
        {
            if (string.IsNullOrWhiteSpace(signal))
            {
                continue;
            }

            foreach (string marker in markers)
            {
                if (signal.Contains(marker, StringComparison.OrdinalIgnoreCase) ||
                    signal.Contains(marker, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private readonly record struct RegionalBoardProfile(string Label, string BackdropSummary);

    private static DebugPanelViewModel BuildDebugPanel(PresentationDebugSnapshot debug)
    {
        return new DebugPanelViewModel
        {
            DiagnosticsSchemaLabel = $"v{debug.DiagnosticsSchemaVersion}",
            SeedLabel = debug.InitialSeed.ToString(),
            NotificationRetentionLabel = debug.NotificationRetentionLimit.ToString(),
            Scale = BuildScaleGroup(debug),
            Pressure = BuildPressureGroup(debug),
            Hotspots = BuildHotspotsGroup(debug),
            Migration = BuildMigrationGroup(debug),
            Warnings = BuildWarningsGroup(debug),
        };
    }

    private static DebugScaleGroupViewModel BuildScaleGroup(PresentationDebugSnapshot debug)
    {
        return new DebugScaleGroupViewModel
        {
            LatestMetrics = new DebugMetricSummaryViewModel
            {
                DiffEntryCount = debug.LatestMetrics.DiffEntryCount,
                DomainEventCount = debug.LatestMetrics.DomainEventCount,
                NotificationCount = debug.LatestMetrics.NotificationCount,
                SavePayloadBytes = debug.LatestMetrics.SavePayloadBytes,
                RetentionLimitReached = debug.RetentionLimitReached,
            },
            CurrentScale = new DebugScaleSummaryViewModel
            {
                EntitySummary = $"{debug.CurrentScale.SettlementCount} settlements, {debug.CurrentScale.ClanCount} clans, {debug.CurrentScale.HouseholdCount} households.",
                InstitutionSummary = $"{debug.CurrentScale.AcademyCount} academies, {debug.CurrentScale.RouteCount} trade routes.",
                ModuleSummary = $"{debug.CurrentScale.EnabledModuleCount} enabled modules mirrored in {debug.CurrentScale.SavedModuleCount} saved envelopes.",
                NotificationUtilizationLabel = $"{debug.CurrentScale.NotificationCount} notices ({debug.CurrentScale.NotificationUtilizationPercent}% of retention).",
                PayloadDensityLabel = $"{debug.CurrentScale.SavePayloadBytesPerSettlement} save bytes per settlement; {debug.CurrentScale.AverageHouseholdsPerSettlement} households per settlement.",
            },
            PayloadSummary = new DebugPayloadSummaryViewModel
            {
                TotalPayloadBytes = debug.CurrentPayloadSummary.TotalModulePayloadBytes,
                LargestModuleKey = debug.CurrentPayloadSummary.LargestModuleKey,
                LargestModulePayloadBytes = debug.CurrentPayloadSummary.LargestModulePayloadBytes,
                LargestModuleShareLabel = $"{debug.CurrentPayloadSummary.LargestModuleShareBasisPoints / 100.0:F2}%",
                Summary = string.IsNullOrWhiteSpace(debug.CurrentPayloadSummary.LargestModuleKey)
                    ? "No module payload data."
                    : $"{debug.CurrentPayloadSummary.TotalModulePayloadBytes} total bytes; {debug.CurrentPayloadSummary.LargestModuleKey} leads at {debug.CurrentPayloadSummary.LargestModulePayloadBytes} bytes.",
            },
            TopPayloadModules = debug.TopPayloadModules
                .Select(static payload => new DebugPayloadFootprintViewModel
                {
                    ModuleKey = payload.ModuleKey,
                    PayloadBytes = payload.PayloadBytes,
                    ShareLabel = $"{payload.PayloadShareBasisPoints / 100.0:F2}%",
                })
                .ToArray(),
            EnabledModules = debug.EnabledModules
                .Select(static module => $"{module.ModuleKey}:{module.Mode}")
                .ToArray(),
            ModuleInspectors = debug.ModuleInspectors
                .Select(static inspector => new DebugModuleInspectorViewModel
                {
                    ModuleKey = inspector.ModuleKey,
                    SchemaVersion = inspector.ModuleSchemaVersion,
                    PayloadBytes = inspector.PayloadBytes,
                })
                .ToArray(),
        };
    }

    private static DebugPressureGroupViewModel BuildPressureGroup(PresentationDebugSnapshot debug)
    {
        return new DebugPressureGroupViewModel
        {
            Interaction = new DebugInteractionPressureViewModel
            {
                ActiveConflictSettlements = debug.CurrentInteractionPressure.ActiveConflictSettlements,
                ActivatedResponseSettlements = debug.CurrentInteractionPressure.ActivatedResponseSettlements,
                SupportedOrderSettlements = debug.CurrentInteractionPressure.SupportedOrderSettlements,
                HighSuppressionDemandSettlements = debug.CurrentInteractionPressure.HighSuppressionDemandSettlements,
                AverageSuppressionDemand = debug.CurrentInteractionPressure.AverageSuppressionDemand,
                PeakSuppressionDemand = debug.CurrentInteractionPressure.PeakSuppressionDemand,
                HighBanditThreatSettlements = debug.CurrentInteractionPressure.HighBanditThreatSettlements,
                Summary = $"{debug.CurrentInteractionPressure.ActivatedResponseSettlements} activated, " +
                    $"{debug.CurrentInteractionPressure.HighSuppressionDemandSettlements} high suppression, " +
                    $"peak demand {debug.CurrentInteractionPressure.PeakSuppressionDemand}.",
            },
            Distribution = new DebugPressureDistributionViewModel
            {
                CalmSettlements = debug.CurrentPressureDistribution.CalmSettlements,
                WatchedSettlements = debug.CurrentPressureDistribution.WatchedSettlements,
                StressedSettlements = debug.CurrentPressureDistribution.StressedSettlements,
                CrisisSettlements = debug.CurrentPressureDistribution.CrisisSettlements,
                Summary = $"{debug.CurrentPressureDistribution.CalmSettlements} calm, " +
                    $"{debug.CurrentPressureDistribution.WatchedSettlements} watched, " +
                    $"{debug.CurrentPressureDistribution.StressedSettlements} stressed, " +
                    $"{debug.CurrentPressureDistribution.CrisisSettlements} crisis.",
            },
        };
    }

    private static DebugHotspotsGroupViewModel BuildHotspotsGroup(PresentationDebugSnapshot debug)
    {
        return new DebugHotspotsGroupViewModel
        {
            CurrentHotspots = debug.CurrentHotspots
                .Select(static hotspot => new DebugHotspotViewModel
                {
                    SettlementName = hotspot.SettlementName,
                    HotspotScore = hotspot.HotspotScore,
                    PressureSummary = $"Bandit {hotspot.BanditThreat}, route {hotspot.RoutePressure}, suppression {hotspot.SuppressionDemand}.",
                    ResponseSummary = hotspot.IsResponseActivated
                        ? $"Active response {hotspot.ResponseActivationLevel} with support {hotspot.OrderSupportLevel}."
                        : "No active response support.",
                })
                .ToArray(),
            DiffTraces = debug.RecentDiffEntries
                .Select(static trace => new DebugTraceItemViewModel
                {
                    ModuleKey = trace.ModuleKey,
                    Summary = trace.Description,
                    EntityKey = trace.EntityKey,
                })
                .ToArray(),
            DomainEvents = debug.RecentDomainEvents
                .Select(static domainEvent => new DebugEventItemViewModel
                {
                    ModuleKey = domainEvent.ModuleKey,
                    EventType = domainEvent.EventType,
                    Summary = domainEvent.Summary,
                })
                .ToArray(),
        };
    }

    private static DebugMigrationGroupViewModel BuildMigrationGroup(PresentationDebugSnapshot debug)
    {
        return new DebugMigrationGroupViewModel
        {
            LoadOriginLabel = debug.LoadMigration.LoadOriginLabel,
            MigrationStatusLabel = debug.LoadMigration.ConsistencyPassed
                ? "Consistency passed"
                : "Consistency warnings present",
            MigrationSummary = debug.LoadMigration.Summary,
            MigrationConsistencySummary = debug.LoadMigration.ConsistencySummary,
            MigrationStepCountLabel = $"{debug.LoadMigration.StepCount} migration step(s)",
            MigrationSteps = debug.LoadMigration.Steps
                .Select(static step => $"{step.ScopeLabel}:{step.SourceVersion}->{step.TargetVersion}")
                .ToArray(),
        };
    }

    private static DebugWarningsGroupViewModel BuildWarningsGroup(PresentationDebugSnapshot debug)
    {
        return new DebugWarningsGroupViewModel
        {
            Messages = debug.Warnings,
            Invariants = debug.Invariants,
        };
    }

    private sealed class AftermathDocketSignals
    {
        public bool Merit { get; init; }

        public bool Blame { get; init; }

        public bool Relief { get; init; }

        public bool Disorder { get; init; }

        public bool HasSignals => Merit || Blame || Relief || Disorder;

        public string ComposeClauseText(bool useArticle)
        {
            List<string> clauses = new();

            if (Merit)
            {
                clauses.Add("记功簿");
            }

            if (Blame)
            {
                clauses.Add("劾责状");
            }

            if (Relief)
            {
                clauses.Add("抚恤簿");
            }

            if (Disorder)
            {
                clauses.Add("清路札");
            }

            return string.Join("、", clauses);
        }
    }
}

public sealed class PresentationShellViewModel
{
    public GreatHallDashboardViewModel GreatHall { get; set; } = new();

    public LineageSurfaceViewModel Lineage { get; set; } = new();

    public DeskSandboxViewModel DeskSandbox { get; set; } = new();

    public OfficeSurfaceViewModel Office { get; set; } = new();

    public WarfareSurfaceViewModel Warfare { get; set; } = new();

    public NotificationCenterViewModel NotificationCenter { get; set; } = new();

    public DebugPanelViewModel Debug { get; set; } = new();
}

public sealed class GreatHallDashboardViewModel
{
    public string CurrentDateLabel { get; set; } = string.Empty;

    public string ReplayHash { get; set; } = string.Empty;

    public int UrgentCount { get; set; }

    public int ConsequentialCount { get; set; }

    public int BackgroundCount { get; set; }

    public string FamilySummary { get; set; } = string.Empty;

    public string EducationSummary { get; set; } = string.Empty;

    public string TradeSummary { get; set; } = string.Empty;

    public string GovernanceSummary { get; set; } = string.Empty;

    public string WarfareSummary { get; set; } = string.Empty;

    public string AftermathDocketSummary { get; set; } = string.Empty;

    public string LeadNoticeTitle { get; set; } = string.Empty;
}

public sealed class LineageSurfaceViewModel
{
    public IReadOnlyList<ClanTileViewModel> Clans { get; set; } = [];
}

public sealed class ClanTileViewModel
{
    public string ClanName { get; set; } = string.Empty;

    public int Prestige { get; set; }

    public int SupportReserve { get; set; }

    public string StatusText { get; set; } = string.Empty;
}

public sealed class DeskSandboxViewModel
{
    public IReadOnlyList<SettlementNodeViewModel> Settlements { get; set; } = [];
}

public sealed class SettlementNodeViewModel
{
    public string SettlementName { get; set; } = string.Empty;

    public int Security { get; set; }

    public int Prosperity { get; set; }

    public string AcademySummary { get; set; } = string.Empty;

    public string MarketSummary { get; set; } = string.Empty;

    public string GovernanceSummary { get; set; } = string.Empty;

    public string CampaignSummary { get; set; } = string.Empty;

    public string AftermathSummary { get; set; } = string.Empty;

    public string PressureSummary { get; set; } = string.Empty;
}

public sealed class OfficeSurfaceViewModel
{
    public string Summary { get; set; } = string.Empty;

    public IReadOnlyList<OfficeAppointmentViewModel> Appointments { get; set; } = [];

    public IReadOnlyList<OfficeJurisdictionViewModel> Jurisdictions { get; set; } = [];
}

public sealed class WarfareSurfaceViewModel
{
    public string Summary { get; set; } = string.Empty;

    public IReadOnlyList<CampaignBoardViewModel> CampaignBoards { get; set; } = [];

    public IReadOnlyList<CampaignMobilizationSignalViewModel> MobilizationSignals { get; set; } = [];
}

public sealed class CampaignBoardViewModel
{
    public string CampaignName { get; set; } = string.Empty;

    public string SettlementLabel { get; set; } = string.Empty;

    public string StatusLabel { get; set; } = string.Empty;

    public string RegionalProfileLabel { get; set; } = string.Empty;

    public string RegionalBackdropSummary { get; set; } = string.Empty;

    public string EnvironmentLabel { get; set; } = string.Empty;

    public string BoardSurfaceLabel { get; set; } = string.Empty;

    public string BoardAtmosphereSummary { get; set; } = string.Empty;

    public string MarkerSummary { get; set; } = string.Empty;

    public string FrontLabel { get; set; } = string.Empty;

    public string SupplyStateLabel { get; set; } = string.Empty;

    public string MoraleStateLabel { get; set; } = string.Empty;

    public string CommandFitLabel { get; set; } = string.Empty;

    public string DirectiveLabel { get; set; } = string.Empty;

    public string DirectiveSummary { get; set; } = string.Empty;

    public string DirectiveTrace { get; set; } = string.Empty;

    public string ObjectiveSummary { get; set; } = string.Empty;

    public string FrontSummary { get; set; } = string.Empty;

    public string MobilizationSummary { get; set; } = string.Empty;

    public string SupplyLineSummary { get; set; } = string.Empty;

    public string CoordinationSummary { get; set; } = string.Empty;

    public string CommanderSummary { get; set; } = string.Empty;

    public string AftermathSummary { get; set; } = string.Empty;

    public string AftermathDocketSummary { get; set; } = string.Empty;

    public IReadOnlyList<CampaignRouteViewModel> Routes { get; set; } = [];
}

public sealed class CampaignRouteViewModel
{
    public string RouteLabel { get; set; } = string.Empty;

    public string RouteRole { get; set; } = string.Empty;

    public string FlowStateLabel { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}

public sealed class CampaignMobilizationSignalViewModel
{
    public string SettlementLabel { get; set; } = string.Empty;

    public string WindowLabel { get; set; } = string.Empty;

    public string ForceSummary { get; set; } = string.Empty;

    public string CommandFitLabel { get; set; } = string.Empty;

    public string DirectiveLabel { get; set; } = string.Empty;

    public string DirectiveSummary { get; set; } = string.Empty;

    public string OfficeSummary { get; set; } = string.Empty;

    public string SourceTrace { get; set; } = string.Empty;
}

public sealed class OfficeAppointmentViewModel
{
    public string DisplayName { get; set; } = string.Empty;

    public string OfficeTitle { get; set; } = string.Empty;

    public bool HasAppointment { get; set; }

    public int AuthorityTier { get; set; }

    public string ServiceSummary { get; set; } = string.Empty;

    public string TaskSummary { get; set; } = string.Empty;

    public string PetitionSummary { get; set; } = string.Empty;

    public string PressureSummary { get; set; } = string.Empty;

    public string PetitionOutcomeCategory { get; set; } = string.Empty;

    public string LastOutcome { get; set; } = string.Empty;

    public string LastPetitionOutcome { get; set; } = string.Empty;
}

public sealed class OfficeJurisdictionViewModel
{
    public string SettlementLabel { get; set; } = string.Empty;

    public string LeadSummary { get; set; } = string.Empty;

    public string LeverageSummary { get; set; } = string.Empty;

    public string PetitionSummary { get; set; } = string.Empty;

    public string TaskSummary { get; set; } = string.Empty;

    public string PetitionOutcomeCategory { get; set; } = string.Empty;

    public string LastPetitionOutcome { get; set; } = string.Empty;
}

public sealed class NotificationCenterViewModel
{
    public IReadOnlyList<NotificationItemViewModel> Items { get; set; } = [];
}

public sealed class NotificationItemViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string WhyItHappened { get; set; } = string.Empty;

    public string WhatNext { get; set; } = string.Empty;

    public string TierLabel { get; set; } = string.Empty;

    public string SurfaceLabel { get; set; } = string.Empty;

    public string SourceModuleKey { get; set; } = string.Empty;

    public int TraceCount { get; set; }
}

public sealed class DebugPanelViewModel
{
    public string DiagnosticsSchemaLabel { get; set; } = string.Empty;

    public string SeedLabel { get; set; } = string.Empty;

    public string NotificationRetentionLabel { get; set; } = string.Empty;

    public DebugScaleGroupViewModel Scale { get; set; } = new();

    public DebugPressureGroupViewModel Pressure { get; set; } = new();

    public DebugHotspotsGroupViewModel Hotspots { get; set; } = new();

    public DebugMigrationGroupViewModel Migration { get; set; } = new();

    public DebugWarningsGroupViewModel Warnings { get; set; } = new();
}

public sealed class DebugScaleGroupViewModel
{
    public DebugMetricSummaryViewModel LatestMetrics { get; set; } = new();

    public DebugScaleSummaryViewModel CurrentScale { get; set; } = new();

    public DebugPayloadSummaryViewModel PayloadSummary { get; set; } = new();

    public IReadOnlyList<DebugPayloadFootprintViewModel> TopPayloadModules { get; set; } = [];

    public IReadOnlyList<string> EnabledModules { get; set; } = [];

    public IReadOnlyList<DebugModuleInspectorViewModel> ModuleInspectors { get; set; } = [];
}

public sealed class DebugPressureGroupViewModel
{
    public DebugInteractionPressureViewModel Interaction { get; set; } = new();

    public DebugPressureDistributionViewModel Distribution { get; set; } = new();
}

public sealed class DebugHotspotsGroupViewModel
{
    public IReadOnlyList<DebugHotspotViewModel> CurrentHotspots { get; set; } = [];

    public IReadOnlyList<DebugTraceItemViewModel> DiffTraces { get; set; } = [];

    public IReadOnlyList<DebugEventItemViewModel> DomainEvents { get; set; } = [];
}

public sealed class DebugMigrationGroupViewModel
{
    public string LoadOriginLabel { get; set; } = string.Empty;

    public string MigrationStatusLabel { get; set; } = string.Empty;

    public string MigrationSummary { get; set; } = string.Empty;

    public string MigrationConsistencySummary { get; set; } = string.Empty;

    public string MigrationStepCountLabel { get; set; } = string.Empty;

    public IReadOnlyList<string> MigrationSteps { get; set; } = [];
}

public sealed class DebugWarningsGroupViewModel
{
    public IReadOnlyList<string> Messages { get; set; } = [];

    public IReadOnlyList<string> Invariants { get; set; } = [];
}

public sealed class DebugMetricSummaryViewModel
{
    public int DiffEntryCount { get; set; }

    public int DomainEventCount { get; set; }

    public int NotificationCount { get; set; }

    public int SavePayloadBytes { get; set; }

    public bool RetentionLimitReached { get; set; }
}

public sealed class DebugInteractionPressureViewModel
{
    public int ActiveConflictSettlements { get; set; }

    public int ActivatedResponseSettlements { get; set; }

    public int SupportedOrderSettlements { get; set; }

    public int HighSuppressionDemandSettlements { get; set; }

    public int AverageSuppressionDemand { get; set; }

    public int PeakSuppressionDemand { get; set; }

    public int HighBanditThreatSettlements { get; set; }

    public string Summary { get; set; } = string.Empty;
}

public sealed class DebugPressureDistributionViewModel
{
    public int CalmSettlements { get; set; }

    public int WatchedSettlements { get; set; }

    public int StressedSettlements { get; set; }

    public int CrisisSettlements { get; set; }

    public string Summary { get; set; } = string.Empty;
}

public sealed class DebugScaleSummaryViewModel
{
    public string EntitySummary { get; set; } = string.Empty;

    public string InstitutionSummary { get; set; } = string.Empty;

    public string ModuleSummary { get; set; } = string.Empty;

    public string NotificationUtilizationLabel { get; set; } = string.Empty;

    public string PayloadDensityLabel { get; set; } = string.Empty;
}

public sealed class DebugHotspotViewModel
{
    public string SettlementName { get; set; } = string.Empty;

    public int HotspotScore { get; set; }

    public string PressureSummary { get; set; } = string.Empty;

    public string ResponseSummary { get; set; } = string.Empty;
}

public sealed class DebugPayloadSummaryViewModel
{
    public int TotalPayloadBytes { get; set; }

    public string LargestModuleKey { get; set; } = string.Empty;

    public int LargestModulePayloadBytes { get; set; }

    public string LargestModuleShareLabel { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}

public sealed class DebugPayloadFootprintViewModel
{
    public string ModuleKey { get; set; } = string.Empty;

    public int PayloadBytes { get; set; }

    public string ShareLabel { get; set; } = string.Empty;
}

public sealed class DebugModuleInspectorViewModel
{
    public string ModuleKey { get; set; } = string.Empty;

    public int SchemaVersion { get; set; }

    public int PayloadBytes { get; set; }
}

public sealed class DebugTraceItemViewModel
{
    public string ModuleKey { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string? EntityKey { get; set; }
}

public sealed class DebugEventItemViewModel
{
    public string ModuleKey { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}
