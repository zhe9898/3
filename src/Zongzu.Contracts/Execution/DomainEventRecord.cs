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
    public const string Livelihood = "livelihood";
    public const string MandateConfidence = "mandateConfidence";
    public const string PressureScore = "pressureScore";
    public const string DefectionRisk = "defectionRisk";
    public const string AuthorityTier = "authorityTier";
    public const string PersonId = "personId";
}

public static class DomainEventMetadataValues
{
    public const string CauseAmnesty = "amnesty";
    public const string CauseCorvee = "corvee";
    public const string CauseDisaster = "disaster";
    public const string CauseFrontier = "frontier";
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
}
