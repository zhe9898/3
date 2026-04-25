using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Application;

public sealed partial class PresentationReadModelBuilder
{
    private static IReadOnlyList<HouseholdSocialPressureSnapshot> BuildHouseholdSocialPressures(PresentationReadModelBundle bundle)
    {
        if (bundle.Households.Count == 0)
        {
            return [];
        }

        HouseholdId? anchorHouseholdId = SelectPlayerAnchorHouseholdId(bundle.Households);
        Dictionary<int, SettlementSnapshot> settlementsById = bundle.Settlements
            .ToDictionary(static settlement => settlement.Id.Value, static settlement => settlement);
        Dictionary<int, PopulationSettlementSnapshot> populationBySettlement = IndexFirstBySettlement(
            bundle.PopulationSettlements,
            static settlement => settlement.SettlementId);
        Dictionary<int, ClanSnapshot> clansById = bundle.Clans
            .ToDictionary(static clan => clan.Id.Value, static clan => clan);
        Dictionary<int, SettlementPublicLifeSnapshot> publicLifeBySettlement = IndexFirstBySettlement(
            bundle.PublicLifeSettlements,
            static entry => entry.SettlementId);
        Dictionary<int, SettlementDisorderSnapshot> disorderBySettlement = IndexFirstBySettlement(
            bundle.SettlementDisorder,
            static entry => entry.SettlementId);
        Dictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement = IndexFirstBySettlement(
            bundle.OfficeJurisdictions,
            static entry => entry.SettlementId);
        Dictionary<int, MarketSnapshot> marketsBySettlement = IndexFirstBySettlement(
            bundle.Markets,
            static market => market.SettlementId);

        return bundle.Households
            .OrderBy(static household => household.SettlementId.Value)
            .ThenBy(static household => household.Id.Value)
            .Select(household => BuildHouseholdSocialPressure(
                household,
                anchorHouseholdId,
                settlementsById,
                populationBySettlement,
                clansById,
                publicLifeBySettlement,
                disorderBySettlement,
                jurisdictionsBySettlement,
                marketsBySettlement,
                bundle))
            .ToArray();
    }

    private static HouseholdSocialPressureSnapshot BuildHouseholdSocialPressure(
        HouseholdPressureSnapshot household,
        HouseholdId? anchorHouseholdId,
        IReadOnlyDictionary<int, SettlementSnapshot> settlementsById,
        IReadOnlyDictionary<int, PopulationSettlementSnapshot> populationBySettlement,
        IReadOnlyDictionary<int, ClanSnapshot> clansById,
        IReadOnlyDictionary<int, SettlementPublicLifeSnapshot> publicLifeBySettlement,
        IReadOnlyDictionary<int, SettlementDisorderSnapshot> disorderBySettlement,
        IReadOnlyDictionary<int, JurisdictionAuthoritySnapshot> jurisdictionsBySettlement,
        IReadOnlyDictionary<int, MarketSnapshot> marketsBySettlement,
        PresentationReadModelBundle bundle)
    {
        settlementsById.TryGetValue(household.SettlementId.Value, out SettlementSnapshot? settlement);
        populationBySettlement.TryGetValue(household.SettlementId.Value, out PopulationSettlementSnapshot? population);
        publicLifeBySettlement.TryGetValue(household.SettlementId.Value, out SettlementPublicLifeSnapshot? publicLife);
        disorderBySettlement.TryGetValue(household.SettlementId.Value, out SettlementDisorderSnapshot? disorder);
        jurisdictionsBySettlement.TryGetValue(household.SettlementId.Value, out JurisdictionAuthoritySnapshot? jurisdiction);
        marketsBySettlement.TryGetValue(household.SettlementId.Value, out MarketSnapshot? market);

        ClanSnapshot? sponsorClan = household.SponsorClanId is { } sponsorClanId
            && clansById.TryGetValue(sponsorClanId.Value, out ClanSnapshot? clan)
                ? clan
                : null;

        HouseholdSocialPressureSignalSnapshot debtSignal = BuildDebtSignal(household, settlement);
        HouseholdSocialPressureSignalSnapshot laborSignal = BuildLaborSignal(household, population);
        HouseholdSocialPressureSignalSnapshot mobilitySignal = BuildMobilitySignal(household, settlement, population);
        HouseholdSocialPressureSignalSnapshot marketSignal = BuildMarketSignal(household, market, bundle);
        HouseholdSocialPressureSignalSnapshot educationSignal = BuildEducationSignal(sponsorClan, bundle);
        HouseholdSocialPressureSignalSnapshot lineageSignal = BuildLineageProtectionSignal(household, sponsorClan);
        HouseholdSocialPressureSignalSnapshot yamenSignal = BuildYamenSignal(publicLife, jurisdiction);
        HouseholdSocialPressureSignalSnapshot publicLifeOrderResidueSignal = BuildPublicLifeOrderResidueSignal(
            household,
            publicLife,
            disorder,
            jurisdiction,
            sponsorClan);
        HouseholdSocialPressureSignalSnapshot disorderSignal = BuildDisorderSignal(household, disorder);

        HouseholdSocialPressureSignalSnapshot[] signals =
        [
            debtSignal,
            laborSignal,
            mobilitySignal,
            marketSignal,
            educationSignal,
            lineageSignal,
            yamenSignal,
            publicLifeOrderResidueSignal,
            disorderSignal,
        ];

        (string driftKey, string driftLabel, HouseholdSocialPressureSignalSnapshot leadSignal) =
            SelectPrimaryHouseholdDrift(household, sponsorClan, signals);
        int pressureScore = ComputeHouseholdPressureScore(household, leadSignal);
        string livelihoodLabel = RenderLivelihoodLabel(household.Livelihood);
        string settlementName = settlement?.Name ?? $"Settlement {household.SettlementId.Value}";

        return new HouseholdSocialPressureSnapshot
        {
            HouseholdId = household.Id,
            HouseholdName = household.HouseholdName,
            SettlementId = household.SettlementId,
            SettlementName = settlementName,
            SponsorClanId = household.SponsorClanId,
            SponsorClanName = sponsorClan?.ClanName ?? string.Empty,
            Livelihood = household.Livelihood,
            LivelihoodLabel = livelihoodLabel,
            PrimaryDriftKey = driftKey,
            PrimaryDriftLabel = driftLabel,
            PressureScore = pressureScore,
            PressureBandLabel = RenderPressureBand(pressureScore),
            IsPlayerAnchor = anchorHouseholdId == household.Id,
            AttachmentSummary = BuildHouseholdAttachmentSummary(household, sponsorClan, market, publicLife, jurisdiction),
            PressureSummary = leadSignal.Summary,
            VisibleChainSummary = BuildHouseholdVisibleChainSummary(
                household,
                settlementName,
                livelihoodLabel,
                driftLabel,
                leadSignal,
                sponsorClan,
                market),
            Signals = signals,
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.PopulationAndHouseholds,
                settlement is null ? string.Empty : KnownModuleKeys.WorldSettlements,
                sponsorClan is null ? string.Empty : KnownModuleKeys.FamilyCore,
                market is null ? string.Empty : KnownModuleKeys.TradeAndIndustry,
                publicLife is null ? string.Empty : KnownModuleKeys.PublicLifeAndRumor,
                jurisdiction is null ? string.Empty : KnownModuleKeys.OfficeAndCareer,
                disorder is null ? string.Empty : KnownModuleKeys.OrderAndBanditry),
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildDebtSignal(HouseholdPressureSnapshot household, SettlementSnapshot? settlement)
    {
        int settlementDrag = settlement is null ? 0 : Math.Max(0, 50 - settlement.Prosperity) / 2;
        int score = Math.Clamp((household.Distress + household.DebtPressure + settlementDrag) / 2, 0, 100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.DebtAndSubsistence,
            Label = "债与口粮",
            Score = score,
            Summary = $"{household.HouseholdName}民困{household.Distress}，债压{household.DebtPressure}，先看口粮、借贷与支用能否续住。",
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.PopulationAndHouseholds,
                settlement is null ? string.Empty : KnownModuleKeys.WorldSettlements),
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildLaborSignal(HouseholdPressureSnapshot household, PopulationSettlementSnapshot? population)
    {
        int settlementLaborDrag = population is null ? 0 : Math.Max(0, 55 - population.LaborSupply / 2);
        int score = Math.Clamp(100 - household.LaborCapacity + household.DependentCount * 4 + settlementLaborDrag, 0, 100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.LaborFragility,
            Label = "劳力脆弱",
            Score = score,
            Summary = $"{household.HouseholdName}劳力{household.LaborCapacity}，依口{household.DependentCount}，家内能不能撑住取决于劳力与照料负担。",
            SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildMobilitySignal(
        HouseholdPressureSnapshot household,
        SettlementSnapshot? settlement,
        PopulationSettlementSnapshot? population)
    {
        int securityDrag = settlement is null ? 0 : Math.Max(0, 48 - settlement.Security);
        int settlementOutflow = population?.MigrationPressure ?? 0;
        int score = Math.Clamp(household.MigrationRisk + (household.IsMigrating ? 18 : 0) + securityDrag + settlementOutflow / 5, 0, 100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.Mobility,
            Label = "外出迁徙",
            Score = score,
            Summary = household.IsMigrating
                ? $"{household.HouseholdName}已经动了外出之念，迁徙风险{household.MigrationRisk}，乡里留力正在变薄。"
                : $"{household.HouseholdName}迁徙风险{household.MigrationRisk}；若债压和路况再坏，外出会先成为压力出口。",
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.PopulationAndHouseholds,
                settlement is null ? string.Empty : KnownModuleKeys.WorldSettlements),
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildMarketSignal(
        HouseholdPressureSnapshot household,
        MarketSnapshot? market,
        PresentationReadModelBundle bundle)
    {
        int livelihoodPull = household.Livelihood switch
        {
            LivelihoodType.PettyTrader => 42,
            LivelihoodType.Artisan => 34,
            LivelihoodType.Boatman => 32,
            LivelihoodType.HiredLabor => 20,
            LivelihoodType.SeasonalMigrant => 18,
            _ => 8,
        };
        int marketPull = market is null
            ? 0
            : Math.Clamp((market.Demand / 2) + Math.Max(0, 115 - market.PriceIndex) / 5 - market.LocalRisk / 3, 0, 45);
        int marketNetworkPull = bundle.ClanTradeRoutes.Any(entry => entry.SettlementId == household.SettlementId && entry.IsActive)
            ? 12
            : 0;
        int score = Math.Clamp(livelihoodPull + marketPull + marketNetworkPull, 0, 100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.MarketExposure,
            Label = "入市机会",
            Score = score,
            Summary = market is null
                ? $"{household.HouseholdName}眼下仍以{RenderLivelihoodLabel(household.Livelihood)}为主，市镇机会尚未形成明确牵引。"
                : $"{household.HouseholdName}贴着{market.MarketName}，市需{market.Demand}，本地风险{market.LocalRisk}，可见小买卖、工役或脚路机会。",
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.PopulationAndHouseholds,
                market is null ? string.Empty : KnownModuleKeys.TradeAndIndustry),
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildEducationSignal(ClanSnapshot? sponsorClan, PresentationReadModelBundle bundle)
    {
        if (sponsorClan is null)
        {
            return new HouseholdSocialPressureSignalSnapshot
            {
                SignalKey = HouseholdSocialPressureSignalKeys.EducationPull,
                Label = "供读牵引",
                Score = 0,
                Summary = "未见明确供读牵引。",
                SourceModuleKeys = [],
            };
        }

        EducationCandidateSnapshot[] clanCandidates = bundle.EducationCandidates
            .Where(candidate => candidate.ClanId == sponsorClan.Id)
            .ToArray();
        int studyingCount = clanCandidates.Count(static candidate => candidate.IsStudying);
        int stress = clanCandidates.Length == 0
            ? 0
            : Math.Clamp((int)clanCandidates.Average(static candidate => candidate.Stress), 0, 100);
        int score = Math.Clamp(studyingCount * 18 + stress / 3 + sponsorClan.Prestige / 5, 0, 100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.EducationPull,
            Label = "供读牵引",
            Score = score,
            Summary = studyingCount == 0
                ? $"{sponsorClan.ClanName}尚未把这一带家户明显卷入供读压力。"
                : $"{sponsorClan.ClanName}有{studyingCount}名子弟在读，供读、账房、塾师与落第后路都会牵动附着家户。",
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.FamilyCore,
                clanCandidates.Length == 0 ? string.Empty : KnownModuleKeys.EducationAndExams),
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildLineageProtectionSignal(HouseholdPressureSnapshot household, ClanSnapshot? sponsorClan)
    {
        if (sponsorClan is null)
        {
            return new HouseholdSocialPressureSignalSnapshot
            {
                SignalKey = HouseholdSocialPressureSignalKeys.LineageProtection,
                Label = "宗族吸附",
                Score = 0,
                Summary = $"{household.HouseholdName}未挂在可见宗族庇护下，遇事更多靠家户与邻里自己消化。",
                SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
            };
        }

        int need = Math.Clamp((household.Distress + household.DebtPressure) / 2, 0, 100);
        int support = Math.Clamp((sponsorClan.SupportReserve + sponsorClan.Prestige) / 2, 0, 100);
        int score = Math.Clamp((need + support) / 2 + sponsorClan.CharityObligation / 4, 0, 100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.LineageProtection,
            Label = "宗族吸附",
            Score = score,
            Summary = $"{household.HouseholdName}挂在{sponsorClan.ClanName}名下；族望{sponsorClan.Prestige}、支用{sponsorClan.SupportReserve}会决定它是得救济，还是被压房支。",
            SourceModuleKeys = DistinctNonEmpty(KnownModuleKeys.PopulationAndHouseholds, KnownModuleKeys.FamilyCore),
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildYamenSignal(
        SettlementPublicLifeSnapshot? publicLife,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (publicLife is null && jurisdiction is null)
        {
            return new HouseholdSocialPressureSignalSnapshot
            {
                SignalKey = HouseholdSocialPressureSignalKeys.YamenContact,
                Label = "文书接触",
                Score = 0,
                Summary = "未见明确衙门、契据或诉状接触面。",
                SourceModuleKeys = [],
            };
        }

        int publicDocument = publicLife is null ? 0 : publicLife.DocumentaryWeight / 2 + publicLife.NoticeVisibility / 3;
        int officeLoad = jurisdiction is null ? 0 : jurisdiction.PetitionPressure + jurisdiction.ClerkDependence / 2 + jurisdiction.PetitionBacklog / 2;
        int score = Math.Clamp(publicDocument + officeLoad / 2, 0, 100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.YamenContact,
            Label = "文书接触",
            Score = score,
            Summary = jurisdiction is null
                ? $"{publicLife?.NodeLabel ?? "地方公共面"}文书可见度升高，契据、告示与街谈会改变家户判断。"
                : $"{jurisdiction.LeadOfficeTitle}案牍负荷牵住本地，诉状、税契、保人和吏胥中介会进入家户压力链。",
            SourceModuleKeys = DistinctNonEmpty(
                jurisdiction is null ? string.Empty : KnownModuleKeys.OfficeAndCareer,
                publicLife is null ? string.Empty : KnownModuleKeys.PublicLifeAndRumor),
        };
    }

    private static HouseholdSocialPressureSignalSnapshot BuildPublicLifeOrderResidueSignal(
        HouseholdPressureSnapshot household,
        SettlementPublicLifeSnapshot? publicLife,
        SettlementDisorderSnapshot? disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        ClanSnapshot? sponsorClan)
    {
        if (disorder is null
            || !HasHouseholdPublicLifeOrderAftermath(disorder, jurisdiction, sponsorClan))
        {
            return new HouseholdSocialPressureSignalSnapshot
            {
                SignalKey = HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue,
                Label = "巡防后账",
                Score = 0,
                Summary = $"{household.HouseholdName}尚未读到明确的巡防后账；夜路、脚户误读、巡丁担保与县门文移暂未压到此户。",
                SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
            };
        }

        int publicLifeDrag = publicLife is null
            ? 0
            : publicLife.StreetTalkHeat / 4
              + publicLife.MarketRumorFlow / 5
              + publicLife.RoadReportLag / 4
              + publicLife.CourierRisk / 5;
        int officeDrag = jurisdiction is null
            ? 0
            : jurisdiction.PetitionBacklog / 3
              + jurisdiction.ClerkDependence / 4
              + jurisdiction.PetitionPressure / 5;
        int householdExposure = household.Distress / 5
            + household.DebtPressure / 6
            + household.MigrationRisk / 6
            + Math.Max(0, 65 - household.LaborCapacity) / 5;
        int landingResidue = HasPublicLifeOrderRefusalOrPartialResidue(disorder) || disorder.RefusalCarryoverMonths > 0
            ? 30
            : 0;
        int responseResidue = HasAnyPublicLifeOrderResponseAftermath(disorder, jurisdiction, sponsorClan)
            ? 18
            : 0;
        int score = Math.Clamp(
            landingResidue
            + responseResidue
            + disorder.ImplementationDrag / 3
            + disorder.RetaliationRisk / 4
            + disorder.CoercionRisk / 5
            + disorder.RoutePressure / 4
            + publicLifeDrag
            + officeDrag
            + householdExposure,
            0,
            100);

        string orderLabel = RenderOrderInterventionLabel(disorder);
        string landingTail = BuildOrderLandingAftermathSummary(disorder);
        string responseTail = CombinePublicLifeResponseText(
            BuildOrderResponseAftermathSummary(disorder),
            jurisdiction is null ? string.Empty : BuildOfficeResponseAftermathSummary(jurisdiction),
            sponsorClan is null ? string.Empty : BuildFamilyResponseAftermathSummary(sponsorClan));
        string outcomeTail = string.IsNullOrWhiteSpace(responseTail)
            ? "后账尚未被回应，普通户先用夜路、脚路口角与丁力耗费读到余波。"
            : responseTail;

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue,
            Label = "巡防后账",
            Score = score,
            Summary = CombinePublicLifeResponseText(
                $"{household.HouseholdName}读到{orderLabel}之后的普通户后账：夜路更怯、脚户误读更易传开、巡丁担保与口粮会压回本户丁力。",
                landingTail,
                outcomeTail,
                $"民困{household.Distress}，债压{household.DebtPressure}，流徙险{household.MigrationRisk}；路压{disorder.RoutePressure}，县门积案{jurisdiction?.PetitionBacklog ?? 0}。"),
            SourceModuleKeys = DistinctNonEmpty(
                KnownModuleKeys.PopulationAndHouseholds,
                KnownModuleKeys.OrderAndBanditry,
                publicLife is null ? string.Empty : KnownModuleKeys.PublicLifeAndRumor,
                jurisdiction is null ? string.Empty : KnownModuleKeys.OfficeAndCareer,
                sponsorClan is null ? string.Empty : KnownModuleKeys.FamilyCore),
        };
    }

    private static bool HasHouseholdPublicLifeOrderAftermath(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        ClanSnapshot? sponsorClan)
    {
        return HasPublicLifeOrderRefusalOrPartialResidue(disorder)
            || disorder.RefusalCarryoverMonths > 0
            || disorder.ResponseCarryoverMonths > 0
            || !string.IsNullOrWhiteSpace(disorder.LastRefusalResponseTraceCode)
            || HasAnyPublicLifeOrderResponseAftermath(disorder, jurisdiction, sponsorClan);
    }

    private static bool HasAnyPublicLifeOrderResponseAftermath(
        SettlementDisorderSnapshot disorder,
        JurisdictionAuthoritySnapshot? jurisdiction,
        ClanSnapshot? sponsorClan)
    {
        return HasPublicLifeOrderResponseReceipt(disorder)
            || (jurisdiction is not null && HasPublicLifeOrderResponseReceipt(jurisdiction))
            || (sponsorClan is not null && HasPublicLifeOrderResponseReceipt(sponsorClan));
    }

    private static HouseholdSocialPressureSignalSnapshot BuildDisorderSignal(HouseholdPressureSnapshot household, SettlementDisorderSnapshot? disorder)
    {
        if (disorder is null)
        {
            int fallbackScore = Math.Clamp((household.DebtPressure + household.MigrationRisk - household.LaborCapacity) / 3, 0, 60);
            return new HouseholdSocialPressureSignalSnapshot
            {
                SignalKey = HouseholdSocialPressureSignalKeys.DisorderOutlet,
                Label = "失序出口",
                Score = fallbackScore,
                Summary = $"{household.HouseholdName}若债压与迁徙风险恶化，逃散、投靠或灰色生计会成为远端风险出口。",
                SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
            };
        }

        int score = Math.Clamp(
            disorder.BanditThreat / 2
            + disorder.DisorderPressure / 2
            + disorder.RoutePressure / 3
            + household.DebtPressure / 4
            + household.MigrationRisk / 4,
            0,
            100);

        return new HouseholdSocialPressureSignalSnapshot
        {
            SignalKey = HouseholdSocialPressureSignalKeys.DisorderOutlet,
            Label = "失序出口",
            Score = score,
            Summary = $"{household.HouseholdName}所在处盗警{disorder.BanditThreat}、失序压{disorder.DisorderPressure}；若家计撑不住，逃散和私下投靠会更近。",
            SourceModuleKeys = DistinctNonEmpty(KnownModuleKeys.PopulationAndHouseholds, KnownModuleKeys.OrderAndBanditry),
        };
    }

    private static (string DriftKey, string DriftLabel, HouseholdSocialPressureSignalSnapshot LeadSignal) SelectPrimaryHouseholdDrift(
        HouseholdPressureSnapshot household,
        ClanSnapshot? sponsorClan,
        IReadOnlyList<HouseholdSocialPressureSignalSnapshot> signals)
    {
        HouseholdSocialPressureSignalSnapshot debt = GetSignal(signals, HouseholdSocialPressureSignalKeys.DebtAndSubsistence);
        HouseholdSocialPressureSignalSnapshot labor = GetSignal(signals, HouseholdSocialPressureSignalKeys.LaborFragility);
        HouseholdSocialPressureSignalSnapshot mobility = GetSignal(signals, HouseholdSocialPressureSignalKeys.Mobility);
        HouseholdSocialPressureSignalSnapshot market = GetSignal(signals, HouseholdSocialPressureSignalKeys.MarketExposure);
        HouseholdSocialPressureSignalSnapshot education = GetSignal(signals, HouseholdSocialPressureSignalKeys.EducationPull);
        HouseholdSocialPressureSignalSnapshot lineage = GetSignal(signals, HouseholdSocialPressureSignalKeys.LineageProtection);
        HouseholdSocialPressureSignalSnapshot yamen = GetSignal(signals, HouseholdSocialPressureSignalKeys.YamenContact);
        HouseholdSocialPressureSignalSnapshot publicOrder = GetSignal(signals, HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue);
        HouseholdSocialPressureSignalSnapshot disorder = GetSignal(signals, HouseholdSocialPressureSignalKeys.DisorderOutlet);

        if (publicOrder.Score >= 58 && (debt.Score >= 35 || yamen.Score >= 40 || disorder.Score >= 45))
        {
            return (HouseholdSocialDriftKeys.PublicOrderAftermath, "巡防后账牵动", publicOrder);
        }

        if (disorder.Score >= 70 && debt.Score >= 45)
        {
            return (HouseholdSocialDriftKeys.ShadowDrift, "滑向失序", disorder);
        }

        if (mobility.Score >= 65 && (debt.Score >= 40 || household.IsMigrating))
        {
            return (HouseholdSocialDriftKeys.MoveOut, "外出迁徙", mobility);
        }

        if (sponsorClan is not null && lineage.Score >= 58)
        {
            return (HouseholdSocialDriftKeys.LineageAbsorption, "投靠宗族", lineage);
        }

        if (education.Score >= 55)
        {
            return (HouseholdSocialDriftKeys.SponsorStudy, "供读牵引", education);
        }

        if (yamen.Score >= 58)
        {
            return (HouseholdSocialDriftKeys.Litigation, "诉讼文书", yamen);
        }

        if (market.Score >= 52)
        {
            return (HouseholdSocialDriftKeys.EnterMarket, "入市谋生", market);
        }

        if (labor.Score >= 66 && (household.Livelihood == LivelihoodType.DomesticServant || household.Livelihood == LivelihoodType.YamenRunner))
        {
            return (HouseholdSocialDriftKeys.EnterService, "差役依附", labor);
        }

        if (debt.Score >= 55 || household.Distress >= 60)
        {
            return (HouseholdSocialDriftKeys.SlideDown, "下滑求贷", debt);
        }

        return (HouseholdSocialDriftKeys.HoldFast, "暂且稳住", debt);
    }

    private static HouseholdSocialPressureSignalSnapshot GetSignal(IReadOnlyList<HouseholdSocialPressureSignalSnapshot> signals, string key)
    {
        return signals.First(signal => string.Equals(signal.SignalKey, key, StringComparison.Ordinal));
    }

    private static int ComputeHouseholdPressureScore(HouseholdPressureSnapshot household, HouseholdSocialPressureSignalSnapshot leadSignal)
    {
        return Math.Clamp(
            (household.Distress
             + household.DebtPressure
             + household.MigrationRisk
             + Math.Max(0, 100 - household.LaborCapacity)
             + leadSignal.Score) / 5,
            0,
            100);
    }

    private static string BuildHouseholdAttachmentSummary(
        HouseholdPressureSnapshot household,
        ClanSnapshot? sponsorClan,
        MarketSnapshot? market,
        SettlementPublicLifeSnapshot? publicLife,
        JurisdictionAuthoritySnapshot? jurisdiction)
    {
        List<string> parts = new() { RenderLivelihoodLabel(household.Livelihood) };

        if (sponsorClan is not null)
        {
            parts.Add($"附着{sponsorClan.ClanName}");
        }

        if (market is not null)
        {
            parts.Add($"贴近{market.MarketName}");
        }

        if (publicLife is not null && !string.IsNullOrWhiteSpace(publicLife.DominantVenueLabel))
        {
            parts.Add($"常见于{publicLife.DominantVenueLabel}");
        }

        if (jurisdiction is not null && !string.IsNullOrWhiteSpace(jurisdiction.LeadOfficeTitle))
        {
            parts.Add($"受{jurisdiction.LeadOfficeTitle}文书面牵动");
        }

        return string.Join("；", parts);
    }

    private static string BuildHouseholdVisibleChainSummary(
        HouseholdPressureSnapshot household,
        string settlementName,
        string livelihoodLabel,
        string driftLabel,
        HouseholdSocialPressureSignalSnapshot leadSignal,
        ClanSnapshot? sponsorClan,
        MarketSnapshot? market)
    {
        string attachment = sponsorClan is not null
            ? $"挂在{sponsorClan.ClanName}名下"
            : market is not null
                ? $"贴着{market.MarketName}"
                : "只在乡里家计中消化";

        return $"{settlementName}{household.HouseholdName}以{livelihoodLabel}吃饭，{attachment}；眼下主要漂向「{driftLabel}」，因为{leadSignal.Label}分数{leadSignal.Score}。";
    }

    private static PlayerInfluenceFootprintSnapshot BuildInfluenceFootprint(PresentationReadModelBundle bundle)
    {
        HouseholdSocialPressureSnapshot? anchorHousehold = SelectPlayerAnchorHouseholdPressure(bundle.HouseholdSocialPressures);
        HashSet<string> enabledCommandSurfaces = IndexEnabledAffordanceSurfaces(bundle.PlayerCommands.Affordances);
        InfluenceReachSnapshot[] reaches =
        [
            BuildOwnHouseholdReach(anchorHousehold),
            BuildObservedHouseholdReach(bundle, anchorHousehold),
            BuildLineageReach(bundle, enabledCommandSurfaces),
            BuildMarketReach(bundle),
            BuildEducationReach(bundle),
            BuildYamenReach(bundle, enabledCommandSurfaces),
            BuildPublicLifeReach(bundle, enabledCommandSurfaces),
            BuildOrderReach(bundle, enabledCommandSurfaces),
            BuildForceReach(bundle, enabledCommandSurfaces),
        ];

        InfluenceReachSnapshot? lead = reaches
            .Where(static reach => reach.IsActive)
            .OrderByDescending(static reach => reach.ReachScore)
            .FirstOrDefault();

        return new PlayerInfluenceFootprintSnapshot
        {
            AnchorHouseholdId = anchorHousehold?.HouseholdId,
            AnchorHouseholdName = anchorHousehold?.HouseholdName ?? string.Empty,
            AnchorHouseholdSummary = BuildAnchorHouseholdSummary(anchorHousehold),
            EntryPositionLabel = BuildEntryPositionLabel(reaches),
            Summary = BuildInfluenceFootprintSummary(reaches, lead),
            Reaches = reaches,
        };
    }

    private static InfluenceReachSnapshot BuildOwnHouseholdReach(HouseholdSocialPressureSnapshot? anchorHousehold)
    {
        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.OwnHousehold,
            Label = "本户",
            IsActive = anchorHousehold is not null,
            HasCommandAffordance = false,
            IsPlayerAnchor = anchorHousehold is not null,
            HasLocalAgency = anchorHousehold is not null,
            ReachScore = anchorHousehold?.PressureScore ?? 0,
            LeverageSummary = anchorHousehold is null
                ? "尚未锚定玩家本户。"
                : $"{anchorHousehold.HouseholdName}是当前立足本户，可先处理劳力、口粮、借贷、供读、议婚、迁徙等本户事项。",
            LocalAgencySummary = anchorHousehold is null
                ? string.Empty
                : $"本户眼下漂向「{anchorHousehold.PrimaryDriftLabel}」，压力{anchorHousehold.PressureScore}；能动性先限于自家劳力、储粮、债务、亲事和求助选择。",
            ConstraintSummary = "本户不是天下按钮；成年人仍会按债、亲缘、劳力和风险重新解释命令。",
            CommandSummary = "本户有本地能动性；正式 command surface 未展开时，不可把它外推成操控全县家户。",
            SourceModuleKeys = anchorHousehold?.SourceModuleKeys ?? [KnownModuleKeys.PopulationAndHouseholds],
        };
    }

    private static InfluenceReachSnapshot BuildObservedHouseholdReach(
        PresentationReadModelBundle bundle,
        HouseholdSocialPressureSnapshot? anchorHousehold)
    {
        HouseholdSocialPressureSnapshot[] observedHouseholds = bundle.HouseholdSocialPressures
            .Where(pressure => anchorHousehold is null || pressure.HouseholdId != anchorHousehold.HouseholdId)
            .ToArray();
        int pressure = observedHouseholds.Length == 0
            ? 0
            : Math.Clamp((int)observedHouseholds.Average(static entry => entry.PressureScore), 0, 100);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.ObservedHouseholds,
            Label = "外户压力",
            IsActive = observedHouseholds.Length > 0,
            HasCommandAffordance = false,
            IsPlayerAnchor = false,
            HasLocalAgency = false,
            ReachScore = pressure,
            LeverageSummary = observedHouseholds.Length == 0
                ? "尚未投出外户压力面。"
                : $"可读{observedHouseholds.Length}户的债、劳力、迁徙与生计压力，但不能直接替他们下决定。",
            LocalAgencySummary = "外户没有玩家本地能动性；只能通过雇佣、借贷、保人、族中救济、告示、诉状、市场机会等触点间接影响。",
            ConstraintSummary = "外户不是棋子，成年人会按自家债、亲缘、劳力和风险回应外来压力。",
            CommandSummary = "外户压力当前是观察面；后续若有命令，也应经宗族、市镇、公共生活或衙门触面传导。",
            SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
        };
    }

    private static InfluenceReachSnapshot BuildLineageReach(
        PresentationReadModelBundle bundle,
        IReadOnlySet<string> enabledCommandSurfaces)
    {
        int score = bundle.Clans.Count == 0
            ? 0
            : Math.Clamp(bundle.Clans.Max(static clan => (clan.Prestige + clan.SupportReserve + Math.Max(0, 100 - clan.BranchTension)) / 3), 0, 100);
        bool hasCommandAffordance = HasEnabledAffordanceForSurface(enabledCommandSurfaces, PlayerCommandSurfaceKeys.Family);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.Lineage,
            Label = "宗族",
            IsActive = bundle.Clans.Count > 0,
            HasCommandAffordance = hasCommandAffordance,
            ReachScore = score,
            LeverageSummary = bundle.Clans.Count == 0
                ? "未见可用宗族节点。"
                : $"可见{bundle.Clans.Count}个宗族节点，族望、支用、房支张力会影响庇护与压制。",
            ConstraintSummary = "宗族只是社会强节点，不是世界本体；它能吸附家户，也会被市场、官府和舆论反压。",
            CommandSummary = hasCommandAffordance
                ? "可借族老、婚配、救济、丧次与承祧议程施力。"
                : "当前宗族面只读。",
            SourceModuleKeys = [KnownModuleKeys.FamilyCore],
        };
    }

    private static InfluenceReachSnapshot BuildMarketReach(PresentationReadModelBundle bundle)
    {
        int marketScore = bundle.Markets.Count == 0
            ? 0
            : Math.Clamp((int)bundle.Markets.Average(static market => market.Demand + Math.Max(0, 120 - market.PriceIndex) / 3 - market.LocalRisk / 2), 0, 100);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.Market,
            Label = "市镇",
            IsActive = bundle.Markets.Count > 0 || bundle.ClanTradeRoutes.Count > 0,
            HasCommandAffordance = false,
            ReachScore = Math.Clamp(marketScore + Math.Min(20, bundle.ClanTradeRoutes.Count * 3), 0, 100),
            LeverageSummary = bundle.Markets.Count == 0
                ? "市镇投影尚未展开。"
                : $"可见{bundle.Markets.Count}处市镇，价格、需求、风险牵动店铺、作坊、脚夫和小贩。",
            ConstraintSummary = "市场不是职业表；同一家户可同时种田、欠债、记账、跑腿或临时入市。",
            CommandSummary = "MVP先作为可见压力面，后续再接商贸与信用命令。",
            SourceModuleKeys = [KnownModuleKeys.TradeAndIndustry],
        };
    }

    private static InfluenceReachSnapshot BuildEducationReach(PresentationReadModelBundle bundle)
    {
        int studyingCount = bundle.EducationCandidates.Count(static candidate => candidate.IsStudying);
        int score = bundle.EducationCandidates.Count == 0
            ? 0
            : Math.Clamp(studyingCount * 16 + (int)bundle.EducationCandidates.Average(static candidate => candidate.Stress) / 2, 0, 100);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.Education,
            Label = "读书",
            IsActive = bundle.EducationCandidates.Count > 0 || bundle.Academies.Count > 0,
            HasCommandAffordance = false,
            ReachScore = score,
            LeverageSummary = bundle.EducationCandidates.Count == 0
                ? "尚未投出读书压力。"
                : $"可见{studyingCount}名在读者，供读、师承、落第和账房文书后路会牵动家户。",
            ConstraintSummary = "读书不是单独职业线，它消耗家计，也可能把人带向文书、教书、账房或官场边缘。",
            CommandSummary = "MVP先读压力，暂不把读书做成按钮链。",
            SourceModuleKeys = [KnownModuleKeys.EducationAndExams],
        };
    }

    private static InfluenceReachSnapshot BuildYamenReach(
        PresentationReadModelBundle bundle,
        IReadOnlySet<string> enabledCommandSurfaces)
    {
        int score = bundle.OfficeJurisdictions.Count == 0
            ? 0
            : Math.Clamp(bundle.OfficeJurisdictions.Max(static jurisdiction =>
                jurisdiction.JurisdictionLeverage
                + jurisdiction.PetitionPressure / 2
                + jurisdiction.ClerkDependence / 2), 0, 100);
        bool hasCommandAffordance = HasEnabledAffordanceForSurface(enabledCommandSurfaces, PlayerCommandSurfaceKeys.Office);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.Yamen,
            Label = "衙门文书",
            IsActive = bundle.OfficeJurisdictions.Count > 0,
            HasCommandAffordance = hasCommandAffordance,
            ReachScore = score,
            LeverageSummary = bundle.OfficeJurisdictions.Count == 0
                ? "未见县署文书触面。"
                : $"可见{bundle.OfficeJurisdictions.Count}个文书触面，案牍、税契、诉状和吏胥中介会改变地方判断。",
            ConstraintSummary = "仁宗朝开局不默认王安石以后那套保甲压法；地方接触先从案牍、税契和人情中介走。",
            CommandSummary = hasCommandAffordance
                ? "可通过请托、呈文或行政杠杆间接施力。"
                : "当前衙门面只读。",
            SourceModuleKeys = [KnownModuleKeys.OfficeAndCareer],
        };
    }

    private static InfluenceReachSnapshot BuildPublicLifeReach(
        PresentationReadModelBundle bundle,
        IReadOnlySet<string> enabledCommandSurfaces)
    {
        int score = bundle.PublicLifeSettlements.Count == 0
            ? 0
            : Math.Clamp(bundle.PublicLifeSettlements.Max(static entry =>
                entry.StreetTalkHeat + entry.PublicLegitimacy / 2 + entry.MarketRumorFlow / 2), 0, 100);
        bool hasCommandAffordance = HasEnabledAffordanceForSurface(enabledCommandSurfaces, PlayerCommandSurfaceKeys.PublicLife);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.PublicLife,
            Label = "公共生活",
            IsActive = bundle.PublicLifeSettlements.Count > 0,
            HasCommandAffordance = hasCommandAffordance,
            ReachScore = score,
            LeverageSummary = bundle.PublicLifeSettlements.Count == 0
                ? "寺院、茶肆、告示与街谈尚未投出。"
                : $"可见{bundle.PublicLifeSettlements.Count}处公共面，街谈、告示、寺院义举和市声会放大或缓冲压力。",
            ConstraintSummary = "公共生活是社会信任节点，不是魔法舆论池。",
            CommandSummary = hasCommandAffordance
                ? "可借告示、调停、路报或地方义举影响可见面。"
                : "当前公共生活面只读。",
            SourceModuleKeys = [KnownModuleKeys.PublicLifeAndRumor],
        };
    }

    private static InfluenceReachSnapshot BuildOrderReach(
        PresentationReadModelBundle bundle,
        IReadOnlySet<string> enabledCommandSurfaces)
    {
        int score = bundle.SettlementDisorder.Count == 0
            ? 0
            : Math.Clamp(bundle.SettlementDisorder.Max(static entry =>
                entry.BanditThreat + entry.RoutePressure + entry.SuppressionDemand) / 2, 0, 100);
        bool hasCommandAffordance = HasEnabledAffordanceForSurface(enabledCommandSurfaces, PlayerCommandSurfaceKeys.PublicLife);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.Order,
            Label = "灰色失序",
            IsActive = bundle.SettlementDisorder.Count > 0,
            HasCommandAffordance = hasCommandAffordance,
            ReachScore = score,
            LeverageSummary = bundle.SettlementDisorder.Count == 0
                ? "尚未投出治安与灰色压力。"
                : $"可见{bundle.SettlementDisorder.Count}处失序压力，逃散、小盗、私贩和保护关系会成为压力出口。",
            ConstraintSummary = "灰色层不是开局匪帮玩法，而是债、饥、路况、官府迟滞共同挤出的出口。",
            CommandSummary = hasCommandAffordance
                ? "可借地方看守、告示、调停或镇压间接处理。"
                : "当前失序面只读。",
            SourceModuleKeys = [KnownModuleKeys.OrderAndBanditry],
        };
    }

    private static InfluenceReachSnapshot BuildForceReach(
        PresentationReadModelBundle bundle,
        IReadOnlySet<string> enabledCommandSurfaces)
    {
        int score = bundle.Campaigns.Count == 0
            ? 0
            : Math.Clamp(bundle.Campaigns.Max(static campaign =>
                campaign.FrontPressure
                + campaign.SupplyStretch / 2
                + campaign.CivilianExposure / 2), 0, 100);
        bool hasCommandAffordance = HasEnabledAffordanceForSurface(enabledCommandSurfaces, PlayerCommandSurfaceKeys.Warfare);

        return new InfluenceReachSnapshot
        {
            ReachKey = InfluenceReachKeys.Force,
            Label = "军伍边防",
            IsActive = bundle.Campaigns.Count > 0,
            HasCommandAffordance = hasCommandAffordance,
            ReachScore = score,
            LeverageSummary = bundle.Campaigns.Count == 0
                ? "暂无军伍边防触面。"
                : $"可见{bundle.Campaigns.Count}个军务面，军费、招募、伤亡回流和边讯会压回地方社会。",
            ConstraintSummary = "仁宗朝可有西夏压力，但不变成全战；军务是家户、财政、路况和士气的外压。",
            CommandSummary = hasCommandAffordance
                ? "可通过军议、护运、动员或撤防做有限处置。"
                : "当前军务面只读。",
            SourceModuleKeys = [KnownModuleKeys.WarfareCampaign],
        };
    }

    private static string BuildEntryPositionLabel(IReadOnlyList<InfluenceReachSnapshot> reaches)
    {
        bool ownHousehold = reaches.Any(static reach => reach.ReachKey == InfluenceReachKeys.OwnHousehold && reach.IsActive);
        bool lineage = reaches.Any(static reach => reach.ReachKey == InfluenceReachKeys.Lineage && reach.IsActive);
        bool yamen = reaches.Any(static reach => reach.ReachKey == InfluenceReachKeys.Yamen && reach.IsActive);
        bool market = reaches.Any(static reach => reach.ReachKey == InfluenceReachKeys.Market && reach.IsActive);

        if (ownHousehold && lineage && yamen && market)
        {
            return "本户-宗族-市镇-文书交界";
        }

        if (ownHousehold && lineage && market)
        {
            return "本户-宗族-市镇交界";
        }

        if (ownHousehold && lineage)
        {
            return "本户-宗族交界";
        }

        if (ownHousehold)
        {
            return "地方本户视角";
        }

        return "地方家户视角";
    }

    private static string BuildInfluenceFootprintSummary(
        IReadOnlyList<InfluenceReachSnapshot> reaches,
        InfluenceReachSnapshot? lead)
    {
        int activeCount = reaches.Count(static reach => reach.IsActive);
        int commandCount = reaches.Count(static reach => reach.HasCommandAffordance);
        int localAgencyCount = reaches.Count(static reach => reach.HasLocalAgency);

        if (lead is null)
        {
            return "活社会投影尚未展开，玩家影响圈暂不可读。";
        }

        return $"当前可见{activeCount}层社会触面，其中{localAgencyCount}层是本地能动性、{commandCount}层已有正式命令杠杆；最强压力在「{lead.Label}」，分数{lead.ReachScore}。";
    }

    private static HouseholdId? SelectPlayerAnchorHouseholdId(IReadOnlyList<HouseholdPressureSnapshot> households)
    {
        if (households.Count == 0)
        {
            return null;
        }

        return households
            .OrderByDescending(static household => household.SponsorClanId.HasValue)
            .ThenBy(static household => household.SettlementId.Value)
            .ThenBy(static household => household.Id.Value)
            .First()
            .Id;
    }

    private static HouseholdSocialPressureSnapshot? SelectPlayerAnchorHouseholdPressure(
        IReadOnlyList<HouseholdSocialPressureSnapshot> pressures)
    {
        return pressures.FirstOrDefault(static pressure => pressure.IsPlayerAnchor)
               ?? pressures
                   .OrderByDescending(static pressure => pressure.SponsorClanId.HasValue)
                   .ThenBy(static pressure => pressure.SettlementId.Value)
                   .ThenBy(static pressure => pressure.HouseholdId.Value)
                   .FirstOrDefault();
    }

    private static string BuildAnchorHouseholdSummary(HouseholdSocialPressureSnapshot? anchorHousehold)
    {
        return anchorHousehold is null
            ? string.Empty
            : $"{anchorHousehold.HouseholdName}以{anchorHousehold.LivelihoodLabel}吃饭，眼下{anchorHousehold.PrimaryDriftLabel}；这是玩家能先从自家劳力、口粮、债务和亲缘处下手的立足点。";
    }

    private static string RenderLivelihoodLabel(LivelihoodType livelihood)
    {
        return livelihood switch
        {
            LivelihoodType.Smallholder => "小农",
            LivelihoodType.Tenant => "佃作",
            LivelihoodType.HiredLabor => "雇工",
            LivelihoodType.Artisan => "手艺",
            LivelihoodType.PettyTrader => "小贩",
            LivelihoodType.Boatman => "船脚",
            LivelihoodType.DomesticServant => "仆役",
            LivelihoodType.YamenRunner => "衙前差使",
            LivelihoodType.SeasonalMigrant => "季节外出",
            LivelihoodType.Vagrant => "游食",
            _ => "生计未明",
        };
    }

    private static string RenderPressureBand(int score)
    {
        return score switch
        {
            >= 75 => "急压",
            >= 55 => "偏紧",
            >= 35 => "可撑",
            _ => "暂稳",
        };
    }
}
