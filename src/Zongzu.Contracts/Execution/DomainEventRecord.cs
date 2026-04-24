using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zongzu.Contracts;

public interface IDomainEvent
{
    string ModuleKey { get; }

    string EventType { get; }

    string Summary { get; }

    string? EntityKey { get; }

    IReadOnlyDictionary<string, string> Metadata { get; }
}

public sealed class DomainEventRecord : IDomainEvent
{
    private static readonly IReadOnlyDictionary<string, string> EmptyMetadata =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal));

    public DomainEventRecord(
        string moduleKey,
        string eventType,
        string summary,
        string? entityKey = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ModuleKey = moduleKey ?? throw new ArgumentNullException(nameof(moduleKey));
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        EntityKey = entityKey;
        Metadata = NormalizeMetadata(metadata);
    }

    public string ModuleKey { get; }

    public string EventType { get; }

    public string Summary { get; }

    public string? EntityKey { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    private static IReadOnlyDictionary<string, string> NormalizeMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return EmptyMetadata;
        }

        Dictionary<string, string> copy = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> pair in metadata)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                continue;
            }

            copy[pair.Key] = pair.Value ?? string.Empty;
        }

        return copy.Count == 0
            ? EmptyMetadata
            : new ReadOnlyDictionary<string, string>(copy);
    }
}

public static class DomainEventMetadataKeys
{
    public const string Cause = "cause";
    public const string SourceEventType = "sourceEventType";
    public const string DisasterKind = "disasterKind";
    public const string Severity = "severity";
    public const string FloodRisk = "floodRisk";
    public const string EmbankmentStrain = "embankmentStrain";
    public const string FrontierPressure = "frontierPressure";
    public const string DisorderDelta = "disorderDelta";
    public const string DisasterDisorderDelta = "disasterDisorderDelta";
    public const string DisasterHazardPressure = "disasterHazardPressure";
    public const string DisasterFloodPressure = "disasterFloodPressure";
    public const string DisasterEmbankmentPressure = "disasterEmbankmentPressure";
    public const string DisasterLocalDisorderSoil = "disasterLocalDisorderSoil";
    public const string DisasterRouteRupturePressure = "disasterRouteRupturePressure";
    public const string DisasterSuppressionBuffer = "disasterSuppressionBuffer";
    public const string CorveeWindow = "corveeWindow";
    public const string SettlementId = "settlementId";
    public const string DistressBefore = "distressBefore";
    public const string DistressAfter = "distressAfter";
    public const string DebtBefore = "debtBefore";
    public const string DebtAfter = "debtAfter";
    public const string TaxDebtDelta = "taxDebtDelta";
    public const string TaxVisibilityPressure = "taxVisibilityPressure";
    public const string TaxLiquidityPressure = "taxLiquidityPressure";
    public const string TaxLaborPressure = "taxLaborPressure";
    public const string TaxFragilityPressure = "taxFragilityPressure";
    public const string TaxInteractionPressure = "taxInteractionPressure";
    public const string GrainOldPrice = "grainOldPrice";
    public const string GrainCurrentPrice = "grainCurrentPrice";
    public const string GrainPriceDelta = "grainPriceDelta";
    public const string GrainSupply = "grainSupply";
    public const string GrainDemand = "grainDemand";
    public const string SubsistenceDistressDelta = "subsistenceDistressDelta";
    public const string SubsistencePricePressure = "subsistencePricePressure";
    public const string SubsistenceGrainBufferPressure = "subsistenceGrainBufferPressure";
    public const string SubsistenceMarketDependencyPressure = "subsistenceMarketDependencyPressure";
    public const string SubsistenceLaborPressure = "subsistenceLaborPressure";
    public const string SubsistenceFragilityPressure = "subsistenceFragilityPressure";
    public const string SubsistenceInteractionPressure = "subsistenceInteractionPressure";
    public const string Livelihood = "livelihood";
    public const string AmnestyWave = "amnestyWave";
    public const string AmnestyReleasePressure = "amnestyReleasePressure";
    public const string AmnestyDocketPressure = "amnestyDocketPressure";
    public const string AmnestyClerkHandlingPressure = "amnestyClerkHandlingPressure";
    public const string AmnestyAuthorityBuffer = "amnestyAuthorityBuffer";
    public const string AmnestyLocalDisorderSoil = "amnestyLocalDisorderSoil";
    public const string AmnestySuppressionBuffer = "amnestySuppressionBuffer";
    public const string JurisdictionLeverage = "jurisdictionLeverage";
    public const string ClerkDependence = "clerkDependence";
    public const string PetitionPressure = "petitionPressure";
    public const string PetitionBacklog = "petitionBacklog";
    public const string AdministrativeTaskLoad = "administrativeTaskLoad";
    public const string ClerkCapturePressure = "clerkCapturePressure";
    public const string ClerkCaptureDependencePressure = "clerkCaptureDependencePressure";
    public const string ClerkCaptureBacklogPressure = "clerkCaptureBacklogPressure";
    public const string ClerkCaptureTaskPressure = "clerkCaptureTaskPressure";
    public const string ClerkCapturePetitionPressure = "clerkCapturePetitionPressure";
    public const string ClerkCaptureAuthorityBuffer = "clerkCaptureAuthorityBuffer";
    public const string OfficialSupplyPressure = "officialSupplyPressure";
    public const string OfficialSupplyQuotaPressure = "officialSupplyQuotaPressure";
    public const string OfficialSupplyDocketPressure = "officialSupplyDocketPressure";
    public const string OfficialSupplyClerkDistortionPressure = "officialSupplyClerkDistortionPressure";
    public const string OfficialSupplyAuthorityBuffer = "officialSupplyAuthorityBuffer";
    public const string OfficialSupplyDistressDelta = "officialSupplyDistressDelta";
    public const string OfficialSupplyDebtDelta = "officialSupplyDebtDelta";
    public const string OfficialSupplyLaborDrop = "officialSupplyLaborDrop";
    public const string OfficialSupplyMigrationDelta = "officialSupplyMigrationDelta";
    public const string OfficialSupplyLivelihoodExposurePressure = "officialSupplyLivelihoodExposurePressure";
    public const string OfficialSupplyResourceBuffer = "officialSupplyResourceBuffer";
    public const string OfficialSupplyLaborPressure = "officialSupplyLaborPressure";
    public const string OfficialSupplyLiquidityPressure = "officialSupplyLiquidityPressure";
    public const string OfficialSupplyFragilityPressure = "officialSupplyFragilityPressure";
    public const string OfficialSupplyInteractionPressure = "officialSupplyInteractionPressure";
    public const string ExamTier = "examTier";
    public const string ExamScore = "examScore";
    public const string ExamStudyProgress = "examStudyProgress";
    public const string ExamAcademyPrestige = "examAcademyPrestige";
    public const string ExamTutorQuality = "examTutorQuality";
    public const string ExamClanSupportReserve = "examClanSupportReserve";
    public const string ExamFavorBalance = "examFavorBalance";
    public const string ExamShamePressure = "examShamePressure";
    public const string ExamStress = "examStress";
    public const string ExamPrestigeDelta = "examPrestigeDelta";
    public const string ExamMarriageAllianceDelta = "examMarriageAllianceDelta";
    public const string ExamTierPrestigePressure = "examTierPrestigePressure";
    public const string ExamDistinctionPressure = "examDistinctionPressure";
    public const string ExamAcademySignal = "examAcademySignal";
    public const string ExamClanStandingPressure = "examClanStandingPressure";
    public const string ExamKinshipRolePressure = "examKinshipRolePressure";
    public const string MandateConfidence = "mandateConfidence";
    public const string PressureScore = "pressureScore";
    public const string PolicyWindowPressure = "policyWindowPressure";
    public const string PolicyWindowMandateDeficit = "policyWindowMandateDeficit";
    public const string PolicyWindowAuthoritySignal = "policyWindowAuthoritySignal";
    public const string PolicyWindowLeverageSignal = "policyWindowLeverageSignal";
    public const string PolicyWindowPetitionSignal = "policyWindowPetitionSignal";
    public const string PolicyWindowAdministrativeDrag = "policyWindowAdministrativeDrag";
    public const string PolicyWindowClerkDrag = "policyWindowClerkDrag";
    public const string PolicyWindowBacklogDrag = "policyWindowBacklogDrag";
    public const string DefectionRisk = "defectionRisk";
    public const string DefectionBaselinePressure = "defectionBaselinePressure";
    public const string DefectionMandateDeficit = "defectionMandateDeficit";
    public const string DefectionDemotionPressure = "defectionDemotionPressure";
    public const string DefectionClerkPressure = "defectionClerkPressure";
    public const string DefectionPetitionPressure = "defectionPetitionPressure";
    public const string DefectionReputationStrain = "defectionReputationStrain";
    public const string DefectionAuthorityBuffer = "defectionAuthorityBuffer";
    public const string AuthorityTier = "authorityTier";
    public const string PersonId = "personId";
    public const string ClanId = "clanId";
    public const string EmotionalAxis = "emotionalAxis";
    public const string SocialPressureScore = "socialPressureScore";
    public const string TemperingScore = "temperingScore";
    public const string PressureBand = "pressureBand";
    public const string TemperingBand = "temperingBand";
}

public static class DomainEventMetadataValues
{
    public const string CauseAmnesty = "amnesty";
    public const string CauseCorvee = "corvee";
    public const string CauseDisaster = "disaster";
    public const string CauseFrontier = "frontier";
    public const string CauseGrainPriceSpike = "grain-price-spike";
    public const string CauseHarvest = "harvest";
    public const string CauseExamPass = "exam-pass";
    public const string CauseOfficialSupply = "official-supply";
    public const string CauseTaxSeason = "tax-season";
    public const string DisasterFlood = "flood";
    public const string SeverityFloodModerate = "flood-moderate";
    public const string SeverityFloodSevere = "flood-severe";
    public const string SeverityFrontierModerate = "frontier-moderate";
    public const string SeverityFrontierSevere = "frontier-severe";
    public const string CauseCourt = "court";
    public const string CauseRegime = "regime";
    public const string CauseClerkCapture = "clerk-capture";
    public const string CauseSocialPressure = "social-pressure";
    public const string CausePressureTempering = "pressure-tempering";
}
