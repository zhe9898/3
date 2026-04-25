using System.Collections.Generic;
using Zongzu.Kernel;

namespace Zongzu.Contracts;

public static class HouseholdSocialDriftKeys
{
    public const string HoldFast = "HoldFast";
    public const string SlideDown = "SlideDown";
    public const string MoveOut = "MoveOut";
    public const string EnterMarket = "EnterMarket";
    public const string SponsorStudy = "SponsorStudy";
    public const string SeekPatronage = "SeekPatronage";
    public const string Litigation = "Litigation";
    public const string EnterService = "EnterService";
    public const string ShadowDrift = "ShadowDrift";
    public const string LineageAbsorption = "LineageAbsorption";
    public const string PublicOrderAftermath = "PublicOrderAftermath";
}

public static class HouseholdSocialPressureSignalKeys
{
    public const string DebtAndSubsistence = "DebtAndSubsistence";
    public const string LaborFragility = "LaborFragility";
    public const string Mobility = "Mobility";
    public const string MarketExposure = "MarketExposure";
    public const string EducationPull = "EducationPull";
    public const string LineageProtection = "LineageProtection";
    public const string YamenContact = "YamenContact";
    public const string DisorderOutlet = "DisorderOutlet";
    public const string PublicLifeOrderResidue = "PublicLifeOrderResidue";
}

public static class InfluenceReachKeys
{
    public const string OwnHousehold = "OwnHousehold";
    public const string ObservedHouseholds = "ObservedHouseholds";
    public const string Lineage = "Lineage";
    public const string Market = "Market";
    public const string Education = "Education";
    public const string Yamen = "Yamen";
    public const string PublicLife = "PublicLife";
    public const string Order = "Order";
    public const string Force = "Force";
}

public sealed record HouseholdSocialPressureSignalSnapshot
{
    public string SignalKey { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public int Score { get; init; }

    public string Summary { get; init; } = string.Empty;

    public IReadOnlyList<string> SourceModuleKeys { get; init; } = [];
}

public sealed record HouseholdSocialPressureSnapshot
{
    public HouseholdId HouseholdId { get; init; }

    public string HouseholdName { get; init; } = string.Empty;

    public SettlementId SettlementId { get; init; }

    public string SettlementName { get; init; } = string.Empty;

    public ClanId? SponsorClanId { get; init; }

    public string SponsorClanName { get; init; } = string.Empty;

    public LivelihoodType Livelihood { get; init; } = LivelihoodType.Unknown;

    public string LivelihoodLabel { get; init; } = string.Empty;

    public string PrimaryDriftKey { get; init; } = string.Empty;

    public string PrimaryDriftLabel { get; init; } = string.Empty;

    public int PressureScore { get; init; }

    public string PressureBandLabel { get; init; } = string.Empty;

    public bool IsPlayerAnchor { get; init; }

    public string AttachmentSummary { get; init; } = string.Empty;

    public string PressureSummary { get; init; } = string.Empty;

    public string VisibleChainSummary { get; init; } = string.Empty;

    public IReadOnlyList<HouseholdSocialPressureSignalSnapshot> Signals { get; init; } = [];

    public IReadOnlyList<string> SourceModuleKeys { get; init; } = [];
}

public sealed record InfluenceReachSnapshot
{
    public string ReachKey { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool HasCommandAffordance { get; init; }

    public bool IsPlayerAnchor { get; init; }

    public bool HasLocalAgency { get; init; }

    public int ReachScore { get; init; }

    public string LeverageSummary { get; init; } = string.Empty;

    public string LocalAgencySummary { get; init; } = string.Empty;

    public string ConstraintSummary { get; init; } = string.Empty;

    public string CommandSummary { get; init; } = string.Empty;

    public IReadOnlyList<string> SourceModuleKeys { get; init; } = [];
}

public sealed record PlayerInfluenceFootprintSnapshot
{
    public HouseholdId? AnchorHouseholdId { get; init; }

    public string AnchorHouseholdName { get; init; } = string.Empty;

    public string AnchorHouseholdSummary { get; init; } = string.Empty;

    public string EntryPositionLabel { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public IReadOnlyList<InfluenceReachSnapshot> Reaches { get; init; } = [];
}
