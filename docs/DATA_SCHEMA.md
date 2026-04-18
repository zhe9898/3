# DATA_SCHEMA

This file defines the canonical data model at the modular level.

## 1. Typed IDs
Use stable, monotonic typed IDs.

```csharp
public readonly record struct PersonId(int Value);
public readonly record struct HouseholdId(int Value);
public readonly record struct ClanId(int Value);
public readonly record struct SettlementId(int Value);
public readonly record struct InstitutionId(int Value);
public readonly record struct MemoryId(int Value);
public readonly record struct RelationshipEdgeId(int Value);
public readonly record struct ForceGroupId(int Value);
public readonly record struct CampaignId(int Value);
public readonly record struct NotificationId(int Value);
```

Rules:
- IDs are unique within save scope
- IDs are never recycled
- IDs are serialized as primitive integers

## 2. Core shared data

### GameDate
```csharp
public readonly record struct GameDate(int Year, int Month);
```

### Core person identity
```csharp
public sealed record PersonCoreData {
    PersonId Id;
    string GivenName;
    string? CourtesyName;
    Sex Sex;
    int BirthAgeMonths;   // optional alternative to BirthDate depending on implementation
    bool IsAlive;
    ClanId ClanId;
    HouseholdId HouseholdId;
    SettlementId SettlementId;
    List<string> CoreTraits;
}
```

### Core household identity
```csharp
public sealed record HouseholdCoreData {
    HouseholdId Id;
    ClanId? ClanId;
    SettlementId SettlementId;
    PersonId? HeadId;
    List<PersonId> MemberIds;
}
```

### Core clan identity
```csharp
public sealed record ClanCoreData {
    ClanId Id;
    string ClanName;
    SettlementId HomeSettlementId;
}
```

### Core settlement identity
```csharp
public sealed record SettlementCoreData {
    SettlementId Id;
    string Name;
    SettlementTier Tier;
    SettlementId? ParentSettlementId;
}
```

## 3. Module state model

### Module state envelope
```csharp
public sealed record ModuleStateEnvelope<TState> {
    string ModuleKey;
    int ModuleSchemaVersion;
    TState State;
}
```

### Runtime domain event record
Not persisted as a save namespace, but part of the canonical deterministic runtime contract.
```csharp
public interface IDomainEvent {
    string ModuleKey;
    string EventType;
    string Summary;
    string? EntityKey;
}
```

Current note:
- `EntityKey` is optional runtime-only targeting metadata used by deterministic handler passes and projection traces
- it does not change root or module save schema

### Save root
```csharp
public sealed record SaveRoot {
    int RootSchemaVersion;
    GameDate CurrentDate;
    FeatureManifestData FeatureManifest;
    KernelStateData KernelState;
    Dictionary<string, object> ModuleStates;
}
```

## 4. Example module states

### FamilyCore state
Owns:
- lineage links
- marriage states
- inheritance positions
- branch metadata
- clan policies

```csharp
public sealed record FamilyCoreState {
    Dictionary<PersonId, FamilyPersonState> People;
    Dictionary<ClanId, FamilyClanState> Clans;
}
```

### PopulationAndHouseholds state
Owns:
- commoner household pressure
- labor roles
- migration pressure
- tenant/worker conditions

```csharp
public sealed record PopulationState {
    Dictionary<HouseholdId, PopulationHouseholdState> Households;
    Dictionary<PersonId, PopulationPersonState> People;
    Dictionary<SettlementId, PopulationSettlementState> Settlements;
}
```

### SocialMemoryAndRelations state
Owns:
- relationship edges
- memory records
- grudge tracks
- clan narrative records

```csharp
public sealed record SocialMemoryState {
    Dictionary<RelationshipEdgeId, RelationshipEdgeData> Relationships;
    Dictionary<MemoryId, MemoryData> Memories;
    Dictionary<ClanId, ClanNarrativeState> ClanNarratives;
}
```

### EducationAndExams state
```csharp
public sealed class EducationAndExamsState {
    List<EducationPersonState> People;
    List<AcademyState> Academies;
}

public sealed class EducationPersonState {
    PersonId PersonId;
    ClanId ClanId;
    InstitutionId AcademyId;
    string DisplayName;
    bool IsStudying;
    bool HasTutor;
    int TutorQuality;
    int StudyProgress;
    int Stress;
    int ExamAttempts;
    bool HasPassedLocalExam;
    string LastOutcome;
    string LastExplanation;
    int ScholarlyReputation;
}

public sealed class AcademyState {
    InstitutionId Id;
    SettlementId SettlementId;
    string AcademyName;
    bool IsOpen;
    int Capacity;
    int Prestige;
}
```

### TradeAndIndustry state
```csharp
public sealed class TradeAndIndustryState {
    List<ClanTradeState> Clans;
    List<SettlementMarketState> Markets;
    List<RouteTradeState> Routes;
}

public sealed class ClanTradeState {
    ClanId ClanId;
    SettlementId PrimarySettlementId;
    int CashReserve;
    int GrainReserve;
    int Debt;
    int CommerceReputation;
    int ShopCount;
    int ManagerSkill;
    string LastOutcome;
    string LastExplanation;
}

public sealed class SettlementMarketState {
    SettlementId SettlementId;
    string MarketName;
    int PriceIndex;
    int Demand;
    int LocalRisk;
}

public sealed class RouteTradeState {
    int RouteId;   // typed route id can replace this later if needed
    ClanId ClanId;
    string RouteName;
    SettlementId SettlementId;
    bool IsActive;
    int Capacity;
    int Risk;
    int LastMargin;
}
```

Post-MVP seam note:
- future black-route / gray-route ledgers stay in `TradeAndIndustry`; they do not justify a detached `BlackRoute` root namespace

### OfficeAndCareer state
```csharp
public sealed class OfficeAndCareerState {
    List<OfficeCareerState> People;
    List<JurisdictionAuthorityState> Jurisdictions;
}

public sealed class OfficeCareerState {
    PersonId PersonId;
    ClanId ClanId;
    SettlementId SettlementId;
    string DisplayName;
    bool IsEligible;
    bool HasAppointment;
    string OfficeTitle;
    int AuthorityTier;
    int JurisdictionLeverage;
    int PetitionPressure;
    int PetitionBacklog;
    int ServiceMonths;
    int PromotionMomentum;
    int DemotionPressure;
    string CurrentAdministrativeTask;
    int AdministrativeTaskLoad;
    int OfficeReputation;
    string LastOutcome;
    string LastPetitionOutcome;
    string LastExplanation;
}

public sealed class JurisdictionAuthorityState {
    SettlementId SettlementId;
    PersonId? LeadOfficialPersonId;
    string LeadOfficialName;
    string LeadOfficeTitle;
    int AuthorityTier;
    int JurisdictionLeverage;
    int PetitionPressure;
    int PetitionBacklog;
    string CurrentAdministrativeTask;
    string LastPetitionOutcome;
    string LastAdministrativeTrace;
}
```

Current lite note:
- the active governance-lite v2 slice persists office careers, service progression, petition handling, and settlement jurisdiction leverage
- office leverage now remains owned by `OfficeAndCareer` while downstream order/force modules may read it through queries only
- the lighter office v2.1 slice adds only derived query/read-model labels such as administrative-task tier, petition-outcome category, and authority-trajectory wording; it does not add new saved fields

### OrderAndBanditry state
```csharp
public sealed class OrderAndBanditryState {
    List<SettlementDisorderState> Settlements;
}

public sealed class SettlementDisorderState {
    SettlementId SettlementId;
    int BanditThreat;
    int RoutePressure;
    int SuppressionDemand;
    int DisorderPressure;
    string LastPressureReason;
}
```

Current lite note:
- the first active M3 slice persists settlement-level disorder only
- outlaw actors/camps remain deferred to a later deeper slice
- future black-route pressure snapshots stay conceptually inside `OrderAndBanditry` even when `TradeAndIndustry` later reads them through preflight query seams

### ConflictAndForce state
```csharp
public sealed class ConflictAndForceState {
    List<SettlementForceState> Settlements;
}

public sealed class SettlementForceState {
    SettlementId SettlementId;
    int GuardCount;
    int RetainerCount;
    int MilitiaCount;
    int EscortCount;
    int Readiness;
    int CommandCapacity;
    int ResponseActivationLevel;
    int OrderSupportLevel;
    bool IsResponseActivated;
    bool HasActiveConflict;
    int CampaignFatigue;
    int CampaignEscortStrain;
    string LastCampaignFalloutTrace;
    string LastConflictTrace;
}
```

Current lite note:
- the active M3 local-conflict slice now persists settlement-level force posture plus campaign-fatigue / escort-strain fallout and readable local conflict traces
- explicit response activation/support state is now persisted with the force posture so cross-module order support does not depend on trace-text inference alone
- campaign aftermath may now increase `CampaignFatigue` and `CampaignEscortStrain`, which later reduce readiness / command / escort posture only inside `ConflictAndForce`
- explicit casualty ledgers, named commanders, and deeper supply fields remain deferred to later force-depth work

### WarfareCampaign state
```csharp
public sealed class WarfareCampaignState {
    List<CampaignFrontState> Campaigns;
    List<CampaignMobilizationSignalState> MobilizationSignals;
}

public sealed class CampaignFrontState {
    CampaignId CampaignId;
    SettlementId AnchorSettlementId;
    string AnchorSettlementName;
    string CampaignName;
    bool IsActive;
    string ObjectiveSummary;
    int MobilizedForceCount;
    int FrontPressure;
    string FrontLabel;
    int SupplyState;
    string SupplyStateLabel;
    int MoraleState;
    string MoraleStateLabel;
    string CommandFitLabel;
    string CommanderSummary;
    string ActiveDirectiveCode;
    string ActiveDirectiveLabel;
    string ActiveDirectiveSummary;
    string LastDirectiveTrace;
    string MobilizationWindowLabel;
    string SupplyLineSummary;
    string OfficeCoordinationTrace;
    string SourceTrace;
    string LastAftermathSummary;
    List<CampaignRouteState> Routes;
}

public sealed class CampaignRouteState {
    string RouteLabel;
    string RouteRole;
    int Pressure;
    int Security;
    string FlowStateLabel;
    string Summary;
}

public sealed class CampaignMobilizationSignalState {
    SettlementId SettlementId;
    string SettlementName;
    int ResponseActivationLevel;
    int CommandCapacity;
    int Readiness;
    int AvailableForceCount;
    int OrderSupportLevel;
    int OfficeAuthorityTier;
    int AdministrativeLeverage;
    int PetitionBacklog;
    string CommandFitLabel;
    string ActiveDirectiveCode;
    string ActiveDirectiveLabel;
    string ActiveDirectiveSummary;
    string MobilizationWindowLabel;
    string OfficeCoordinationTrace;
    string SourceTrace;
}
```

Current lite note:
- `WarfareCampaign.Lite` is now active only through a dedicated campaign-enabled bootstrap/load path
- the active lite slice owns bounded campaign boards plus mobilization signals derived from `ConflictAndForce`, `WorldSettlements`, and optional `OfficeAndCareer` queries only
- schema `3` now also persists module-owned campaign-intent descriptors: directive code/label/summary plus last directive trace
- built-in `1 -> 2 -> 3` migration backfills labels, route descriptors, and directive descriptors deterministically for legacy campaign-enabled saves
- current lite presentation deliberately phrases campaign boards in Chinese-ancient desk-sandbox language while keeping the authoritative rules campaign-level only
- current lite aftermath now drives trade/order/office/social consequences through runtime-only warfare events plus module-owned handler updates, without adding new save fields to those downstream modules
- this slice remains campaign-level: no unit micro, no detached battlefield map, and no separate force-ownership model

### NarrativeProjection state
Narrative projections may be persisted or rebuilt.
If persisted, keep them clearly marked as derived.
```csharp
public sealed class NarrativeProjectionState {
    List<NarrativeNotificationState> Notifications;
}

public sealed class NarrativeNotificationState {
    NotificationId Id;
    GameDate CreatedAt;
    NotificationTier Tier;
    NarrativeSurface Surface;
    string Title;
    string Summary;
    string WhyItHappened;
    string WhatNext;
    string SourceModuleKey;
    bool IsRead;
    List<NarrativeTraceState> Traces;
}

public sealed class NarrativeTraceState {
    string SourceModuleKey;
    string EventType;
    string EventSummary;
    string DiffDescription;
    string? EntityKey;
}
```

### Presentation read-model bundle
Not authoritative and not saved as a separate namespace.
```csharp
public sealed class PresentationReadModelBundle {
    GameDate CurrentDate;
    string ReplayHash;
    IReadOnlyList<ClanSnapshot> Clans;
    IReadOnlyList<SettlementSnapshot> Settlements;
    IReadOnlyList<PopulationSettlementSnapshot> PopulationSettlements;
    IReadOnlyList<EducationCandidateSnapshot> EducationCandidates;
    IReadOnlyList<AcademySnapshot> Academies;
    IReadOnlyList<ClanTradeSnapshot> ClanTrades;
    IReadOnlyList<MarketSnapshot> Markets;
    IReadOnlyList<TradeRouteSnapshot> TradeRoutes;
    IReadOnlyList<OfficeCareerSnapshot> OfficeCareers;
    IReadOnlyList<JurisdictionAuthoritySnapshot> OfficeJurisdictions;
    IReadOnlyList<CampaignFrontSnapshot> Campaigns;
    IReadOnlyList<CampaignMobilizationSignalSnapshot> CampaignMobilizationSignals;
    IReadOnlyList<NarrativeNotificationSnapshot> Notifications;
    PresentationDebugSnapshot Debug;
}

public sealed class PresentationDebugSnapshot {
    int DiagnosticsSchemaVersion;
    long InitialSeed;
    int NotificationRetentionLimit;
    bool RetentionLimitReached;
    ObservabilityMetricsSnapshot LatestMetrics;
    InteractionPressureMetricsSnapshot CurrentInteractionPressure;
    SettlementPressureDistributionSnapshot CurrentPressureDistribution;
    RuntimeScaleMetricsSnapshot CurrentScale;
    IReadOnlyList<SettlementInteractionHotspotSnapshot> CurrentHotspots;
    RuntimePayloadSummarySnapshot CurrentPayloadSummary;
    IReadOnlyList<ModulePayloadFootprintSnapshot> TopPayloadModules;
    DebugLoadMigrationSnapshot LoadMigration;
    IReadOnlyList<DebugFeatureModeSnapshot> EnabledModules;
    IReadOnlyList<DebugModuleInspectorSnapshot> ModuleInspectors;
    IReadOnlyList<DebugDiffTraceSnapshot> RecentDiffEntries;
    IReadOnlyList<DebugDomainEventSnapshot> RecentDomainEvents;
    IReadOnlyList<string> Warnings;
    IReadOnlyList<string> Invariants;
}

public sealed class ObservabilityMetricsSnapshot {
    int DiffEntryCount;
    int DomainEventCount;
    int NotificationCount;
    int SavePayloadBytes;
}

public sealed class DebugLoadMigrationSnapshot {
    string LoadOriginLabel;
    bool WasMigrationApplied;
    int StepCount;
    bool ConsistencyPassed;
    string Summary;
    string ConsistencySummary;
    IReadOnlyList<DebugMigrationStepSnapshot> Steps;
    IReadOnlyList<string> Warnings;
}

public sealed class InteractionPressureMetricsSnapshot {
    int ActiveConflictSettlements;
    int ActivatedResponseSettlements;
    int SupportedOrderSettlements;
    int HighSuppressionDemandSettlements;
    int AverageSuppressionDemand;
    int PeakSuppressionDemand;
    int HighBanditThreatSettlements;
}

public sealed class SettlementPressureDistributionSnapshot {
    int CalmSettlements;
    int WatchedSettlements;
    int StressedSettlements;
    int CrisisSettlements;
}

public sealed class RuntimeScaleMetricsSnapshot {
    int EnabledModuleCount;
    int SavedModuleCount;
    int SettlementCount;
    int ClanCount;
    int HouseholdCount;
    int AcademyCount;
    int RouteCount;
    int NotificationCount;
    int NotificationUtilizationPercent;
    int SavePayloadBytesPerSettlement;
    int AverageHouseholdsPerSettlement;
}

public sealed class RuntimePayloadSummarySnapshot {
    int TotalModulePayloadBytes;
    string LargestModuleKey;
    int LargestModulePayloadBytes;
    int LargestModuleShareBasisPoints;
}

public sealed class ModulePayloadFootprintSnapshot {
    string ModuleKey;
    int PayloadBytes;
    int PayloadShareBasisPoints;
}
```

Diagnostics harness note:
- longer multi-seed sweep reports and budget evaluations are runtime-only application diagnostics
- per-module diff/event activity peaks are also runtime-only diagnostics
- local-conflict interaction-pressure summaries are also runtime-only diagnostics
- settlement pressure distributions are also runtime-only diagnostics
- named settlement hotspot summaries for local-conflict stress runs are also runtime-only diagnostics
- load-migration summaries shown in debug/presentation are runtime-only diagnostics derived from the active load path
- scale summaries and top module payload footprints are also runtime-only diagnostics
- payload-summary headlines and migration-consistency status are also runtime-only diagnostics
- they are not saved in authoritative module namespaces

## 5. Relationship and grudge data
```csharp
public sealed record RelationshipEdgeData {
    RelationshipEdgeId Id;
    PersonId SourcePersonId;
    PersonId TargetPersonId;
    RelationshipKind Kind;
    int Affection;
    int Trust;
    int Fear;
    int Obligation;
    int Shame;
}
```

```csharp
public sealed record MemoryData {
    MemoryId Id;
    MemoryKind Kind;
    PersonId? SubjectPersonId;
    ClanId? SubjectClanId;
    PersonId? TargetPersonId;
    ClanId? TargetClanId;
    int Intensity;
    bool IsPublic;
    GameDate CreatedAt;
}
```

## 6. Force and campaign essentials
```csharp
public sealed record ClanForceState {
    int AuthorityTier;
    int CommandCapacity;
    int SupplyCapacity;
    int OfficialTroops;
    int PrivateRetainers;
    int ClanMilitia;
    int AlliedContingent;
    int Mercenaries;
}
```

```csharp
public sealed record CampaignData {
    CampaignId Id;
    GameDate StartedAt;
    int Objective;
    PersonId? CommanderId;
    int ExecutionQuality;
    int DeviationRisk;
    int MoraleState;
    int SupplyRatio;
}
```

Current lite note:
- the active lite implementation persists one bounded campaign board per participating settlement rather than a free-growing battle log
- mobilization signals stay campaign-owned once the warfare path is enabled, but they are still derived from upstream read-only force and office projections

## 7. Invariants
- dead people cannot hold active pregnancy, study attendance, office duty, or active campaign assignment
- spouse links must be symmetric
- parent/child age relationships must remain plausible
- module state may reference only valid core entity IDs
- module-local references may not point into disabled modules without a documented null/default policy
