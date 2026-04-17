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
public sealed record EducationState {
    Dictionary<PersonId, EducationPersonState> People;
    Dictionary<InstitutionId, AcademyState> Academies;
}
```

### TradeAndIndustry state
```csharp
public sealed record TradeState {
    Dictionary<ClanId, ClanTradeState> Clans;
    Dictionary<SettlementId, SettlementMarketState> Markets;
    Dictionary<int, RouteTradeState> Routes; // typed route id recommended in implementation
}
```

### OfficeAndCareer state
```csharp
public sealed record OfficeState {
    Dictionary<PersonId, CareerState> People;
    Dictionary<SettlementId, AuthorityState> Jurisdictions;
}
```

### OrderAndBanditry state
```csharp
public sealed record OrderState {
    Dictionary<SettlementId, LocalOrderState> Settlements;
    Dictionary<int, BanditGroupState> BanditGroups; // typed id recommended
    Dictionary<PersonId, OutlawPathState> People;
}
```

### ConflictAndForce state
```csharp
public sealed record ForceState {
    Dictionary<ClanId, ClanForceState> Clans;
    Dictionary<SettlementId, LocalConflictState> Settlements;
    Dictionary<ForceGroupId, ForceGroupState> ForceGroups;
}
```

### WarfareCampaign state
```csharp
public sealed record CampaignState {
    Dictionary<CampaignId, CampaignData> Campaigns;
}
```

### NarrativeProjection state
Narrative projections may be persisted or rebuilt.
If persisted, keep them clearly marked as derived.
```csharp
public sealed record NarrativeState {
    Dictionary<NotificationId, NotificationData> Notifications;
}
```

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

## 7. Invariants
- dead people cannot hold active pregnancy, study attendance, office duty, or active campaign assignment
- spouse links must be symmetric
- parent/child age relationships must remain plausible
- module state may reference only valid core entity IDs
- module-local references may not point into disabled modules without a documented null/default policy
