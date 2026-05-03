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

Current save/schema rule:
- persisted root time still records year and month in the current codebase
- the design target for short-band authority is day-level scheduler motion inside the month
- `xun` is a calendar/projection grouping, not a schema-owned authority unit
- if exact day must become persisted authoritative state later, that is a schema/version/migration change and must update this document, `SCHEMA_NAMESPACE_RULES.md`, and save/load tests

### Kernel person identity
`PersonRegistry` (Kernel layer) owns the thin identity anchor:
```csharp
public sealed class PersonRecord {
    public PersonId Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public GameDate BirthDate { get; set; }
    public Gender Gender { get; set; }
    public LifeStage LifeStage { get; set; }
    public bool IsAlive { get; set; }
    public FidelityRing FidelityRing { get; set; }
}
```

> No fat `PersonCoreData` exists in the codebase.  Module-owned per-person state lives inside each module's state namespace (e.g. `FamilyPersonState` in `FamilyCore`, `HouseholdMembership` in `PopulationAndHouseholds`, `OfficeCareerState` in `OfficeAndCareer`).

### Module-owned identity fragments
Modules expose read-only *snapshots* via query interfaces, not shared core records:
- `ClanSnapshot` — projection from `FamilyCore`
- `SettlementSnapshot` — projection from `WorldSettlements`
- `PopulationSettlementSnapshot` — projection from `PopulationAndHouseholds`

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
    IReadOnlyDictionary<string, string> Metadata;
}
```

Current note:
- `EntityKey` is optional runtime-only targeting metadata used by deterministic handler passes and projection traces
- `Metadata` is optional runtime-only structured cause / severity / source metadata used when a generic event type needs rule-readable context; modules must not parse `Summary` for authoritative formulas
- runtime domain-event metadata is copied into a read-only dictionary on creation and is not a save namespace
- it does not change root or module save schema

Current regime legitimacy readback v253-v260 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, full regime engine, faction AI, Court module, regime-recognition ledger, legitimacy ledger, defection ledger, owner-lane ledger, cooldown ledger, or same-month SocialMemory residue
- `WorldSettlements.RegimeLegitimacyShifted` metadata, `OfficeAndCareer.OfficeDefected` metadata, `JurisdictionAuthoritySnapshot`, and `SettlementPublicLifeSnapshot` are runtime/query/projection carriers only
- `天命摇动读回`, `去就风险读回`, `官身承压姿态`, `公议向背读法`, `仍由Office/PublicLife分读`, `不是本户替朝廷修合法性`, and `不是UI判定归附成败` are projected readback text, not persisted authority

Current regime legitimacy readback closeout v261-v268 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, full regime engine, faction AI, Court module, regime-recognition ledger, legitimacy ledger, defection ledger, public-allegiance ledger, owner-lane ledger, cooldown ledger, scheduler ledger, or same-month SocialMemory residue
- the v253-v260 Chain 9 first readback branch is closed only as docs/test evidence; it does not persist regime process state, public allegiance state, dynasty-cycle state, ritual legitimacy state, force-backing state, or durable regime memory state
- any future regime-recognition or durable-regime-memory work must declare an owning module namespace, schema impact, migration plan, and tests before adding state

Current court-policy first rule-density closeout audit v197-v204 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, docket ledger, receipt ledger, receipt-docket ledger, public-life receipt echo ledger, or Court module
- the v109-v196 first rule-density closeout audit v197-v204 is documentation/test governance only; it does not persist court process state, appointment slate, dispatch arrival, or downstream household/market/public consequence state
- existing owner namespaces remain unchanged: `WorldSettlements` for court agenda pressure source, `OfficeAndCareer` for policy/local-response posture, `PublicLifeAndRumor` for public interpretation, and `SocialMemoryAndRelations` for durable `office.policy_local_response...` residue

Current court-policy public-life receipt echo v189-v196 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, public-life receipt echo ledger, receipt-docket ledger, docket-consistency ledger, receipt ledger, suggested-receipt ledger, suggested-action ledger, docket ledger, public-follow-up ledger, cooldown ledger, public-reading ledger, or memory-pressure ledger
- the new `公议回执回声防误读` / `街面只读已投影的政策公议后手` / `公议不把回执读成新政令` text is runtime projection over existing `SocialMemoryEntrySnapshot` cause data and `SettlementPublicLifeSnapshot` values
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a public-life receipt echo cache, cooldown marker, repeat-pressure field, command-ranking override, or court-policy ledger

Current court-policy receipt-docket consistency guard v181-v188 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, receipt-docket ledger, docket-consistency ledger, receipt ledger, suggested-receipt ledger, suggested-action ledger, docket ledger, public-follow-up ledger, cooldown ledger, public-reading ledger, or memory-pressure ledger
- the new `回执案牍一致防误读` / `回执只回收已投影的政策公议后手` / `案牍不把回执读成新政策结果` text is runtime projection over existing `SocialMemoryEntrySnapshot` cause data and `SettlementPublicLifeSnapshot` values
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a receipt-docket guard cache, cooldown marker, repeat-pressure field, command-ranking override, or court-policy ledger

Current court-policy suggested receipt guard v173-v180 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, receipt ledger, suggested-receipt ledger, suggested-action ledger, docket ledger, public-follow-up ledger, cooldown ledger, public-reading ledger, or memory-pressure ledger
- the new `建议回执防误读` / `只回收已投影的政策公议后手` / `回执不是新政策结果` text is runtime projection over existing `SocialMemoryEntrySnapshot` cause data and `SettlementPublicLifeSnapshot` values
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a suggested receipt guard cache, cooldown marker, repeat-pressure field, command-ranking override, or court-policy ledger

Current court-policy suggested action guard v165-v172 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, suggested-action ledger, docket ledger, public-follow-up ledger, cooldown ledger, public-reading ledger, or memory-pressure ledger
- the new `建议动作防误读` / `只承接已投影的政策公议后手` text is runtime projection over existing `SocialMemoryEntrySnapshot` cause data, `SettlementPublicLifeSnapshot` values, and the already-selected `PlayerCommandAffordanceSnapshot`
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a suggested-action guard cache, cooldown marker, repeat-pressure field, command-ranking override, or court-policy ledger

Current court-policy public follow-up cue v149-v156 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, public-follow-up ledger, cooldown ledger, public-reading ledger, or memory-pressure ledger
- the new `政策公议后手提示` / `公议轻续提示` text is runtime projection over existing `SocialMemoryEntrySnapshot` cause data and `SettlementPublicLifeSnapshot` values
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a public follow-up cache, cooldown marker, repeat-pressure field, or court-policy ledger

Current court-policy follow-up docket guard v157-v164 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, docket ledger, public-follow-up ledger, cooldown ledger, public-reading ledger, or memory-pressure ledger
- the new `政策后手案牍防误读` / `公议后手只作案牍提示` text is runtime projection over existing `SocialMemoryEntrySnapshot` cause data and `SettlementPublicLifeSnapshot` values
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a docket guard cache, cooldown marker, repeat-pressure field, or court-policy ledger

Current court-policy public-reading echo v141-v148 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, public-reading ledger, or memory-pressure ledger
- the new `政策公议旧读回` / `公议旧账回声` text is runtime projection over existing `SocialMemoryEntrySnapshot`, `JurisdictionAuthoritySnapshot`, and `SettlementPublicLifeSnapshot` values
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a public-reading cache, repeat-pressure field, or court-policy ledger

Current court-policy memory-pressure readback v133-v140 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, projection cache, or ledger
- the new `政策旧账回压读回` text is runtime projection over existing `SocialMemoryEntrySnapshot`, `JurisdictionAuthoritySnapshot`, and `SettlementPublicLifeSnapshot` values
- existing `MemoryRecordState` records remain the only durable source; Application does not persist a memory-pressure cache or repeat-pressure field

Current court-policy social-memory echo v125-v132 note:
- no root or module schema version changes
- no new persisted field, namespace, migration, save manifest entry, feature-pack save membership, or ledger
- `SocialMemoryKinds.OfficePolicyLocalResponseResidue` is a code constant only; durable records use existing `MemoryRecordState` fields (`Kind`, `CauseKey`, `Type`, `Subtype`, `Weight`, dates, summary, lifecycle)
- the structured cause prefix is `office.policy_local_response...`; it distinguishes delayed court-policy local-response residue from `order.public_life.response...` without adding schema
- if future work needs a new persisted discriminator beyond existing memory fields, it must stop and add schema/migration documentation and tests first

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

### WorldSettlements state
Owns:
- settlement baselines
- route conditions
- prosperity / security
- settlement tier / node rank

```csharp
public sealed class WorldSettlementsState {
    List<SettlementStateData> Settlements;
    List<RouteStateData> Routes;
    SeasonBandData CurrentSeason;
    int LastDeclaredFloodDisasterBand;
    int LastDeclaredFrontierStrainBand;
}

public sealed class SettlementStateData {
    SettlementId SettlementId;
    string DisplayName;
    SettlementTier Tier;
    int Security;
    int Prosperity;
    string LastOutcome;
    string LastExplanation;
}
```

Current note:
- `WorldSettlements` schema `2` now persists settlement tier so county seat, market town, village cluster, and prefecture-facing nodes stay module-owned rather than UI-invented
- the built-in `1 -> 2` migration backfills conservative tiers for legacy saves inside the same namespace only
- `WorldSettlements` schema `7` persists `LastDeclaredFloodDisasterBand` for the chain-6 thin slice so a persistent flood band does not re-emit `WorldSettlements.DisasterDeclared` every month
- the built-in `6 -> 7` migration initializes that declaration watermark to `0`; this is module-local compatibility state, not a root-schema change
- `WorldSettlements` schema `8` persists `LastDeclaredFrontierStrainBand` for the chain-5 thin slice so a persistent frontier pressure band does not re-emit `WorldSettlements.FrontierStrainEscalated` and re-trigger supply requisitions every month
- the built-in `7 -> 8` migration initializes that frontier watermark to `0`; current thin allocation emits one settlement-scoped frontier fact, not a whole-realm demand
- `SeasonBandData.Imperial.MandateConfidence` defaults to a neutral `70`; chain-8 / chain-9 pressure must be explicitly seeded or moved by an imperial/court owner, not inferred from an uninitialized zero

### FamilyCore state
Owns:
- lineage links
- marriage states
- inheritance positions
- branch metadata
- clan policies

```csharp
public sealed class FamilyCoreState {
    List<ClanStateData> Clans;
    List<FamilyPersonState> People;
}

public sealed class ClanStateData {
    ClanId Id;
    string ClanName;
    SettlementId HomeSettlementId;
    int Prestige;
    int SupportReserve;
    PersonId? HeirPersonId;
    int BranchTension;
    int InheritancePressure;
    int SeparationPressure;
    int MediationMomentum;
    int BranchFavorPressure;
    int ReliefSanctionPressure;
    int MarriageAlliancePressure;
    int MarriageAllianceValue;
    int HeirSecurity;
    int ReproductivePressure;
    int MourningLoad;
    int CareLoad;
    int FuneralDebt;
    int RemedyConfidence;
    int CharityObligation;
    string LastConflictCommandCode;
    string LastConflictCommandLabel;
    string LastConflictOutcome;
    string LastConflictTrace;
    string LastRefusalResponseCommandCode;
    string LastRefusalResponseCommandLabel;
    string LastRefusalResponseSummary;
    string LastRefusalResponseOutcomeCode;
    string LastRefusalResponseTraceCode;
    int ResponseCarryoverMonths;
    string LastLifecycleCommandCode;
    string LastLifecycleCommandLabel;
    string LastLifecycleOutcome;
    string LastLifecycleTrace;
}

public sealed class FamilyPersonState {
    PersonId Id;
    ClanId ClanId;
    string GivenName;
    int AgeMonths;
    bool IsAlive;
    BranchPosition BranchPosition;
    PersonId? SpouseId;
    PersonId? FatherId;
    PersonId? MotherId;
    List<PersonId> ChildrenIds;
    int Ambition;
    int Prudence;
    int Loyalty;
    int Sociability;
    int FragilityLedger;
}
```

Current note:
- `FamilyCore` schema `8` owns lineage-conflict, lifecycle pressure, clan-scoped kinship, care burden, funeral debt, remedy confidence, charity-obligation state, and family-owned public-life refusal response trace fields inside the family namespace
- marriage/heir/mourning/care/funeral pressures remain authoritative family state even when projected through the hall, family council, or social-memory read models
- births and marriage-in spouses create identity anchors through `PersonRegistry` command surfaces, but `FamilyCore` owns the clan-scoped facts: `SpouseId`, `FatherId`, `MotherId`, and `ChildrenIds`
- `LastRefusalResponse*` and `ResponseCarryoverMonths` record `FamilyCore`-owned bounded responses such as `请族老解释`; v8 family actor countermoves such as `族老自解释` or `族老避羞` reuse those same trace fields. They are structured aftermath fields for later readback and SocialMemory reads, not receipt text that other modules may parse

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

public sealed class PopulationHouseholdState {
    HouseholdId Id;
    SettlementId SettlementId;
    ClanId? SponsorClanId;
    string HouseholdName;
    int LaborCapacity;
    int DebtPressure;
    int Distress;
    int MigrationRisk;
    string LastLocalResponseCommandCode;
    string LastLocalResponseCommandLabel;
    string LastLocalResponseOutcomeCode;
    string LastLocalResponseTraceCode;
    string LastLocalResponseSummary;
    int LocalResponseCarryoverMonths;
}
```

Current note:
- `PopulationAndHouseholds` schema `3` owns home-household local response traces for public-life/order after-accounts. These fields record bounded household-seat responses such as `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.
- `LastLocalResponse*` and `LocalResponseCarryoverMonths` mutate only inside the population namespace and describe household labor, debt, distress, and migration-risk aftermath. They do not repair `OrderAndBanditry` refusal authority, county-yamen/document landing, family explanation, or SocialMemory residue.
- v13 `SocialMemoryAndRelations` may read `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode` through `HouseholdPressureSnapshot` on the later monthly pass, but this does not make population own durable memory residue and does not add population fields.
- v14 `PopulationAndHouseholds` may read existing SocialMemory snapshots for the same household as local repeat friction, but this reads `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, and `State` only and adds no population state fields.
- v16 `回应承受线` capacity affordance adds no new state fields: projected capacity and command-time capacity summary are derived from existing debt, labor, distress, migration, dependent, laborer, and livelihood fields.
- v17 `取舍预判` tradeoff forecast adds no new state fields: projected benefit / recoil / external-boundary text and command-time tradeoff summary are derived from existing debt, labor, distress, migration, dependent, laborer, and livelihood fields.
- v19 `续接提示` / `换招提示` / `冷却提示` / `续接读回` follow-up affordance adds no new state fields: projected follow-up hints are derived from existing household fields and structured `LastLocalResponse*` codes only.
- v20 `外部后账归位` owner-lane return guidance adds no new state fields: projected `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` are derived from existing household fields and structured `LastLocalResponse*` codes only.
- v21 owner-lane surface readback adds no new state fields: Office/Governance and Family-facing copies of `外部后账归位` are runtime projections derived from existing household response structure, settlement scope, and sponsor-clan scope only.
- v22 owner-lane handoff entry readback adds no new state fields: projected `承接入口` labels name existing Order / Office / Family affordances from the same structured household response projection and do not create a command queue, owner-lane ledger, or household target.
- v23 owner-lane receipt status readback adds no new state fields: projected `归口状态` / `已归口到... lane` strings are derived from existing owner-module response trace fields plus existing household local-response structure and do not create a receipt-status ledger, owner-lane ledger, or household target.
- v24 owner-lane outcome reading guidance adds no new state fields: projected `归口后读法` strings are derived from existing owner-module `LastRefusalResponseOutcomeCode` values and do not create an outcome ledger, receipt-status ledger, owner-lane ledger, household target, schema bump, or migration.
- v25 owner-lane social-residue readback adds no new state fields: projected `社会余味读回` strings are derived from existing owner-module response traces plus existing `SocialMemoryEntrySnapshot` structured fields and do not create a SocialMemory ledger, outcome ledger, owner-lane ledger, household target, schema bump, or migration.
- v26 owner-lane social-residue follow-up guidance adds no new state fields: projected `余味冷却提示`, `余味续接提示`, and `余味换招提示` strings are derived from existing owner-module response traces plus existing `SocialMemoryEntrySnapshot` structured fields and do not create a follow-up ledger, SocialMemory ledger, outcome ledger, owner-lane ledger, household target, schema bump, or migration.
- v27-v30 owner-lane affordance echo, receipt closure, no-loop guard, and closure audit add no new state fields: projected `现有入口读法`, `后手收口读回`, and `闭环防回压` strings are derived from the same existing owner-module response traces plus existing `SocialMemoryEntrySnapshot` structured fields and do not create a stale-guidance ledger, follow-up ledger, SocialMemory ledger, outcome ledger, owner-lane ledger, household target, schema bump, or migration.
- v35 backend canal-window Trade/Order hookup adds no new state fields: `WorldSettlements.CanalWindowChanged` now carries runtime before/after metadata and is consumed by `TradeAndIndustry` / `OrderAndBanditry`, but all mutations land in existing module fields. It creates no canal ledger, owner-lane ledger, save envelope, schema bump, or migration.
- Migration `2 -> 3` initializes local-response strings to empty values and clamps `LocalResponseCarryoverMonths` to `0..1`.

### SocialMemoryAndRelations state
Owns:
- relationship edges
- memory records
- grudge tracks
- clan narrative records
- clan emotional climate records
- person pressure-tempering records

```csharp
public sealed class SocialMemoryAndRelationsState {
    List<ClanNarrativeState> ClanNarratives;
    List<MemoryRecordState> Memories;
    List<DormantStubState> DormantStubs;
    List<ClanEmotionalClimateState> ClanEmotionalClimates;
    List<PersonPressureTemperingState> PersonTemperings;
}

public sealed class ClanEmotionalClimateState {
    ClanId ClanId;
    int Fear;
    int Shame;
    int Grief;
    int Anger;
    int Obligation;
    int Hope;
    int Trust;
    int Restraint;
    int Hardening;
    int Bitterness;
    int Volatility;
    int LastPressureScore;
    int LastPressureBand;
    int LastTemperingBand;
    GameDate LastUpdated;
    string LastTrace;
}

public sealed class PersonPressureTemperingState {
    PersonId PersonId;
    ClanId ClanId;
    int Fear;
    int Shame;
    int Grief;
    int Anger;
    int Obligation;
    int Hope;
    int Trust;
    int Restraint;
    int Hardening;
    int Bitterness;
    int Volatility;
    int LastPressureScore;
    GameDate LastUpdated;
    string LastTrace;
}
```

Current note:
- `SocialMemoryAndRelations` schema `3` persists pressure-tempering state. Clan climates and person tempering ledgers are module-owned residue from household distress, lineage conflict, trade pressure, exam outcomes, death, marriage, and warfare aftermath.
- `SocialMemoryAndRelations` reads foreign pressure through queries or scoped domain events only. It does not write family, population, trade, education, office, order, force, or campaign state.
- public-life order residue v4 uses existing schema `3` fields rather than adding a new persisted shape: `Memories` carry public-order cause/kind entries, `ClanNarratives` carry the lasting social interpretation, and `ClanEmotionalClimates` carry obligation, fear, shame, anger, trust, bitterness, or volatility changes
- public-life order refusal residue v5 still uses SocialMemory schema `3`: refused or partial `添雇巡丁` / `严缉路匪` read structured Order outcome/refusal/partial codes and then write only existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates` records
- public-life order response residue v6 still uses SocialMemory schema `3`: Month N+2 response aftermath reads structured `LastRefusalResponseCommandCode`, `LastRefusalResponseOutcomeCode`, `LastRefusalResponseTraceCode`, and `ResponseCarryoverMonths` fields from Order / Office / Family query snapshots, then writes only existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates` records
- public-life order residue decay / repeat friction v7 still uses SocialMemory schema `3`: later-month softening, hardening, and command-friction signals reuse existing `MemoryRecordState.Weight`, `MonthlyDecay`, `LifecycleState`, `CauseKey`, `ClanNarratives`, and `ClanEmotionalClimates`; no SocialMemory `3 -> 4` migration is required
- public-life order actor countermove / passive back-pressure v8 still uses SocialMemory schema `3`: `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` read existing `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, `State`, `SourceClanId`, and `OriginDate` values, skip current-month response memories, and then write only their own existing v6 response trace fields. No SocialMemory field, index, namespace, migration, or save-envelope change is introduced
- home-household local response readback v13 still uses SocialMemory schema `3`: Month N+2 SocialMemory reads structured `PopulationAndHouseholds` local response aftermath (`LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, `LastLocalResponseTraceCode`) and writes only existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates` records. It does not parse `LastLocalResponseSummary` and does not add a new persisted field, index, namespace, or migration
- home-household repeat friction v14 still uses SocialMemory schema `3`: `PopulationAndHouseholds` reads existing `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, `State`, and clan/household scope as command-time friction inputs, but it does not write SocialMemory state and does not require a SocialMemory `3 -> 4` migration
- home-household response capacity v16 still uses SocialMemory schema `3`: SocialMemory does not read `回应承受线`, `承受线代价`, `承受线读回`, or `LastLocalResponseSummary`; later residue still comes only from structured local response aftermath fields
- home-household response tradeoff forecast v17 still uses SocialMemory schema `3`: SocialMemory does not read `取舍预判`, `预期收益`, `反噬尾巴`, `外部后账`, or `LastLocalResponseSummary`; later residue still comes only from structured local response aftermath fields
- home-household short-term consequence readback v18 still uses SocialMemory schema `3`: SocialMemory does not read `短期后果`, `缓住项`, `挤压项`, `仍欠外部后账`, or `LastLocalResponseSummary`; later residue still comes only from structured local response aftermath fields
- home-household follow-up affordance v19 still uses SocialMemory schema `3`: SocialMemory does not read `续接提示`, `换招提示`, `冷却提示`, `续接读回`, or `LastLocalResponseSummary`; later residue still comes only from structured local response aftermath fields
- home-household owner-lane return guidance v20 still uses SocialMemory schema `3`: SocialMemory does not read `外部后账归位`, `该走巡丁`, `该走县门`, `该走族老`, `本户不能代修`, or `LastLocalResponseSummary`; later residue still comes only from structured local response aftermath fields
- home-household owner-lane surface readback v21 still uses SocialMemory schema `3`: SocialMemory does not read Office/Governance or Family-facing copies of `外部后账归位`; later residue still comes only from structured local response aftermath fields
- home-household owner-lane handoff entry readback v22 still uses SocialMemory schema `3`: SocialMemory does not read `承接入口`, owner-lane command-entry labels, or receipt prose; later residue still comes only from structured local response aftermath fields
- home-household owner-lane receipt status readback v23 still uses SocialMemory schema `3`: SocialMemory does not read `归口状态`, `已归口到巡丁`, `已归口到县门`, `已归口到族老`, `归口不等于修好`, `仍看 owner lane 下月读回`, `LastRefusalResponseSummary`, or receipt prose; later residue still comes only from structured aftermath fields
- home-household owner-lane outcome reading v24 still uses SocialMemory schema `3`: SocialMemory does not read `归口后读法`, `已修复：先停本户加压`, `暂压留账`, `恶化转硬`, `放置未接`, `LastRefusalResponseSummary`, or receipt prose; later residue still comes only from structured aftermath fields
- owner-lane social-residue readback v25 still uses SocialMemory schema `3`: Application projections may read existing `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, and `OriginDate` for `社会余味读回`, and same-month SocialMemory de-duplication preserves distinct owner-lane response records by `CauseKey` when the memory kind is otherwise the same. No new SocialMemory field, index, namespace, ledger, or migration is introduced
- owner-lane social-residue follow-up guidance v26 still uses SocialMemory schema `3`: Application projections may read existing `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane outcome codes for `余味冷却提示`, `余味续接提示`, and `余味换招提示`. SocialMemory does not parse these cues, owner-lane guidance prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`; no new SocialMemory field, index, namespace, ledger, or migration is introduced
- owner-lane affordance echo / receipt closure / no-loop guard v27-v30 still use SocialMemory schema `3`: Application projections may read existing `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane outcome codes for `现有入口读法`, `后手收口读回`, and `闭环防回压`. SocialMemory does not parse these cues, owner-lane guidance prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`; no new SocialMemory field, index, namespace, stale-guidance ledger, follow-up ledger, or migration is introduced
- current public-order residue cause keys include `order.public_life.escort_road_report`, `order.public_life.fund_local_watch`, `order.public_life.suppress_banditry`, `order.public_life.negotiate_with_outlaws`, and `order.public_life.tolerate_disorder`
- v5 refusal / partial cause keys include `order.public_life.fund_local_watch.refused`, `order.public_life.fund_local_watch.partial`, `order.public_life.suppress_banditry.refused`, and `order.public_life.suppress_banditry.partial`
- v6 response cause keys use `order.public_life.response`; durable meaning comes from structured outcome codes `Repaired`, `Contained`, `Escalated`, and `Ignored`, never from `DomainEvent.Summary`, receipt summary, or `LastInterventionSummary`
- v13 home-household local response cause keys use `order.public_life.household_response.{HouseholdId}.{CommandCode}.{OutcomeCode}.{TraceCode}`; durable meaning comes from structured outcome codes `Relieved`, `Contained`, `Strained`, and `Ignored`, never from `LastLocalResponseSummary` or receipt prose
- v14 repeat friction reads the same `order.public_life.household_response.{HouseholdId}` cause-key family and weights; it does not add a new key family or persisted ledger
- v7 repeat-friction readers in `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` read projected `SocialMemoryEntrySnapshot` cause keys and weights only; they do not add persisted fields and do not parse social-memory summary prose
- v8 actor countermove readers use structured SocialMemory cause keys, outcome markers, weights, source clan, lifecycle state, and origin date only; they do not parse `DomainEvent.Summary`, memory summaries, receipt summaries, `LastInterventionSummary`, or `LastRefusalResponseSummary`
- `LastUpdated` on climate / tempering records must be a valid `GameDate` even for default or migrated state; default `0000-00` dates are invalid save data.
- `SocialMemoryAndRelations.PressureTempered` and `SocialMemoryAndRelations.EmotionalPressureShifted` are runtime receipts after owned state mutation; their metadata does not extend save schema.

## Current household mobility owner-lane preflight v469-v476 note

- V469-V476 changes no persisted module state, root envelope, module envelope, feature manifest, migration step, or serialized read-model cache.
- `PopulationAndHouseholds` remains the default first owner lane for future household mobility depth because household livelihood, activity, distress, debt, labor, grain, land, migration pressure, and pool carriers already live in that module state.
- Future route history, selector state, target-cardinality state, durable SocialMemory movement residue, commoner status drift, zhuhu/kehu conversion, projection cache, or new owner state is a schema/migration decision and requires a separate plan before implementation.
- `PersonRegistry` remains identity / `FidelityRing` only and gains no household mobility, route, class, status, or durable-residue fields.

## Current household mobility preflight closeout v485-v492 note

- V485-V492 changes no persisted module state, root envelope, module envelope, feature manifest, migration step, or serialized read-model cache.
- The closeout does not add owner-lane state, route-history state, selector state, target-cardinality state, durable SocialMemory movement residue, commoner status drift, zhuhu/kehu conversion, projection cache, or any new module state.
- Future household mobility depth still requires a separate owner-module schema/migration plan before storing mobility histories, movement routes, selector watermarks, target-cardinality state, residue, status drift, or projection caches.
- `PersonRegistry` remains identity / `FidelityRing` only and gains no household mobility, route, class, status, selector, or durable-residue fields.

## Current household mobility runtime rules-data readiness v501-v508 note

- V501-V508 changes no persisted module state, root envelope, module envelope, feature manifest, migration step, serialized read-model cache, rules-data file, rules-data loader, or save manifest.
- The readiness map documents existing `PopulationAndHouseholds` state and snapshot carriers only: household livelihood/activity/distress/debt/labor/grain/land/migration pressure, local-response carryover, settlement summaries, and labor/marriage/migration pools.
- The hardcoded extraction map is documentation and architecture-test evidence. It does not add rules-data authority, persisted config, owner-lane state, selector state, target-cardinality state, route-history state, durable movement residue, class/status state, movement ledger, household-mobility ledger, projection cache, or `PersonRegistry` domain fields.
- Future authored rules-data must still prove whether it is persisted content/config, how it is validated, how defaults/fallbacks work, and whether save/schema docs need a separate update.

## Current household mobility rules-data contract preflight v509-v516 note

- V509-V516 changes no persisted module state, root envelope, module envelope, feature manifest, migration step, serialized read-model cache, rules-data file, rules-data loader, validator, or save manifest.
- The future contract is documented only. It requires stable ids, schema/version, deterministic ordering, default fallback, readable validation errors, owner-consumed use only, no UI/Application authority, and no arbitrary script/plugin execution.
- The tracked `content/` tree has no reusable runtime rules-data/content/config pattern for household mobility, so this pass adds no default skeleton, loader, validator, persisted config, or schema namespace.
- Future rules-data may later describe threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and deterministic tie-break priorities, but only through a separate owner-consumed validation plan.
- `PopulationAndHouseholds` remains schema `3`; `PersonRegistry` gains no household mobility, route, class, status, selector, config, validation, or durable-residue fields.

## Current household mobility default rules-data skeleton v517-v524 note

- V517-V524 changes no persisted module state, root envelope, module envelope, feature manifest, migration step, serialized read-model cache, rules-data file, rules-data loader, validator, default skeleton file, or save manifest.
- The future skeleton shape is documented only: `ruleSetId`, `schemaVersion`, `ownerModule`, `defaultFallbackPolicy`, `parameterGroups`, `validationResult`, and deterministic declaration order.
- The tracked `content/` tree has no reusable runtime rules-data/content/config pattern for household mobility, so this pass adds no `content/rules-data`, default file, loader, validator, persisted config, or schema namespace.
- The skeleton does not enter save and does not change current hardcoded behavior.
- `PopulationAndHouseholds` remains schema `3`; `PersonRegistry` gains no household mobility, route, class, status, selector, config, validation, or durable-residue fields.

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
    List<SettlementBlackRouteLedgerState> BlackRouteLedgers;
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
    int BlockedShipmentCount;
    int SeizureRisk;
    string RouteConstraintLabel;
    string LastRouteTrace;
}

public sealed class SettlementBlackRouteLedgerState {
    SettlementId SettlementId;
    int ShadowPriceIndex;
    int DiversionShare;
    int IllicitMargin;
    int BlockedShipmentCount;
    int SeizureRisk;
    string DiversionBandLabel;
    string LastLedgerTrace;
}
```

Current post-MVP slice note:
- active route records now also carry bounded blockage / seizure mirrors so route-level read models can explain which road is being squeezed without moving ownership out of trade
- gray-route / illicit ledgers now stay in `TradeAndIndustry`; they do not justify a detached `BlackRoute` root namespace

### PublicLifeAndRumor state
Owns:
- street-talk heat
- market bustle
- notice visibility
- road-report lag
- prefecture-dispatch pressure
- public legitimacy
- dominant venue / public trace wording
- monthly cadence / crowd-mix wording for each settlement node
- documentary weight / verification cost / market-rumor flow / courier risk
- channel-summary wording for each settlement node
- official-notice / street-talk / road-report / prefecture-dispatch wording
- contention-summary wording for each settlement node

```csharp
public sealed class PublicLifeAndRumorState {
    List<SettlementPublicLifeState> Settlements;
}

public sealed class SettlementPublicLifeState {
    SettlementId SettlementId;
    int StreetTalkHeat;
    int MarketBuzz;
    int NoticeVisibility;
    int RoadReportLag;
    int PrefectureDispatchPressure;
    int PublicLegitimacy;
    string NodeLabel;
    string DominantVenueLabel;
    string DominantVenueCode;
    string MonthlyCadenceCode;
    string MonthlyCadenceLabel;
    string CrowdMixLabel;
    int DocumentaryWeight;
    int VerificationCost;
    int MarketRumorFlow;
    int CourierRisk;
    string OfficialNoticeLine;
    string StreetTalkLine;
    string RoadReportLine;
    string PrefectureDispatchLine;
    string ContentionSummary;
    string CadenceSummary;
    string PublicSummary;
    string RouteReportSummary;
    string ChannelSummary;
    string LastPublicTrace;
}
```

Current note:
- `PublicLifeAndRumor.Lite` is a thin county-public-life slice: it owns only public pulse and venue wording
- schema `2` extends the slice with monthly-cadence descriptors so the same county / town / road node can read differently across months without inventing a separate calendar module
- schema `3` extends the slice with venue-channel competition descriptors so county gates, market streets, ferries, inns, and prefecture dispatch pressure can read differently without moving command ownership into the public-life module
- schema `4` extends the slice with explicit channel lines so `榜文`、`街谈`、`路报`、`州牒` can visibly diverge without introducing a detached information manager
- it derives from stable query inputs across settlement, household, trade, order, office, family, and social-memory layers without taking ownership away from them

### OfficeAndCareer state
```csharp
public sealed class OfficeAndCareerState {
    List<OfficeCareerState> People;
    List<JurisdictionAuthorityState> Jurisdictions;
    List<OfficialPostState> OfficialPosts;
    List<WaitingListEntryState> WaitingList;
    int LastAppliedAmnestyWave;
    List<SettlementId> ActiveClerkCaptureSettlementIds;
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
    int AppointmentPressure;
    int ClerkDependence;
    int JurisdictionLeverage;
    int PetitionPressure;
    int PetitionBacklog;
    int ServiceMonths;
    int PromotionMomentum;
    int DemotionPressure;
    int OfficialDefectionRisk;
    string CurrentAdministrativeTask;
    int AdministrativeTaskLoad;
    int OfficeReputation;
    string LastOutcome;
    string LastPetitionOutcome;
    string LastExplanation;
    string LastRefusalResponseCommandCode;
    string LastRefusalResponseCommandLabel;
    string LastRefusalResponseSummary;
    string LastRefusalResponseOutcomeCode;
    string LastRefusalResponseTraceCode;
    int ResponseCarryoverMonths;
}

public sealed class JurisdictionAuthorityState {
    SettlementId SettlementId;
    PersonId? LeadOfficialPersonId;
    string LeadOfficialName;
    string LeadOfficeTitle;
    int AuthorityTier;
    int JurisdictionLeverage;
    int ClerkDependence;
    int PetitionPressure;
    int PetitionBacklog;
    string CurrentAdministrativeTask;
    int AdministrativeTaskLoad;
    string LastPetitionOutcome;
    string LastAdministrativeTrace;
    string LastRefusalResponseCommandCode;
    string LastRefusalResponseCommandLabel;
    string LastRefusalResponseSummary;
    string LastRefusalResponseOutcomeCode;
    string LastRefusalResponseTraceCode;
    int ResponseCarryoverMonths;
}
```

Current lite note:
- v37 policy implementation drag adds no new saved fields: `PolicyImplemented` is a runtime `DomainEvent` emitted after `OfficeAndCareer` mutates existing task, backlog, clerk, leverage, promotion/demotion, petition-outcome, and explanation fields. New `PolicyImplementation*` metadata keys are runtime payload only and do not require an `OfficeAndCareer` schema bump, migration, policy ledger, yamen workflow object, owner-lane ledger, or save roundtrip change.
- the active governance-lite v7 slice persists office careers, candidate waiting pressure, clerk dependence, service progression, petition handling, settlement jurisdiction leverage, jurisdiction-level administrative task load, official post/waiting-list skeleton state, `LastAppliedAmnestyWave` for chain-4 amnesty de-duplication, `ActiveClerkCaptureSettlementIds` for chain-7 edge de-duplication, `OfficialDefectionRisk` for chain-9 risk-before-receipt resolution, and office-owned public-life refusal response trace fields
- office leverage now remains owned by `OfficeAndCareer` while downstream order/force modules may read it through queries only
- the lighter office v2.1 slice adds only derived query/read-model labels such as administrative-task tier, petition-outcome category, and authority-trajectory wording; it does not add new saved fields
- `LastRefusalResponse*` and `ResponseCarryoverMonths` record county-yamen and document-routing responses such as `押文催县门` and `改走递报`; v8 office actor countermoves such as `县门自补落地` or `胥吏续拖` reuse those same trace fields. They are office-owned structured aftermath, not a new workflow manager

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
    int BlackRoutePressure;
    int CoercionRisk;
    int SuppressionRelief;
    int ResponseActivationLevel;
    int PaperCompliance;
    int ImplementationDrag;
    int RouteShielding;
    int RetaliationRisk;
    int AdministrativeSuppressionWindow;
    string EscalationBandLabel;
    string LastPressureTrace;
    string LastInterventionCommandCode;
    string LastInterventionCommandLabel;
    string LastInterventionSummary;
    string LastInterventionOutcome;
    string LastInterventionOutcomeCode;
    string LastInterventionRefusalCode;
    string LastInterventionPartialCode;
    string LastInterventionTraceCode;
    int InterventionCarryoverMonths;
    int RefusalCarryoverMonths;
    string LastRefusalResponseCommandCode;
    string LastRefusalResponseCommandLabel;
    string LastRefusalResponseSummary;
    string LastRefusalResponseOutcomeCode;
    string LastRefusalResponseTraceCode;
    int ResponseCarryoverMonths;
}
```

Current lite note:
- the current M3 slice persists settlement-level disorder plus black-route pressure summaries, paper-compliance visibility, implementation-drag friction, route-shielding relief, retaliation-risk backlash, and public-life order trace fields
- `OrderAndBanditry` schema `8` adds structured public-life order outcome/refusal/partial trace fields plus `RefusalCarryoverMonths`; migration `7 -> 8` backfills legacy intervention receipts as accepted follow-through and clamps both carryover windows
- `OrderAndBanditry` schema `9` adds order-owned public-life refusal response trace fields plus `ResponseCarryoverMonths`; migration `8 -> 9` initializes those fields conservatively so `添雇巡丁` / `严缉路匪` post-refusal repair can survive save/load before SocialMemory reads it
- bounded public-life order interventions now persist one-month follow-through or refusal carryover windows so recent road-watch / crackdown / negotiation choices can echo into the next monthly pass without creating a second authority surface
- `SettlementDisorderSnapshot` exposes the same structured aftermath fields, including black-route pressure, coercion risk, implementation drag, route shielding, and retaliation risk, so downstream modules can read order aftermath without parsing receipt text
- `LastRefusalResponse*` and `ResponseCarryoverMonths` record order-owned response commands such as `补保巡丁`, `赔脚户误读`, and `暂缓强压`; v8 order actor countermoves such as `巡丁自补保` or `脚户误读反噬` reuse those same trace fields. The response outcome code is one of `Repaired`, `Contained`, `Escalated`, or `Ignored`
- outlaw actors/camps remain deferred to a later deeper slice
- black-route pressure snapshots stay inside `OrderAndBanditry` even when `TradeAndIndustry` reads them through query seams

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
- v77-v84 warfare directive choice depth reuses those existing directive fields. `军令选择读回`, `案头筹议选择`, `点兵加压选择`, `粮道护持选择`, `归营止损选择`, `WarfareCampaign拥有军令`, and `军务选择不是县门文移代打` are command/readback text over existing fields, not new persisted state.
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
    IReadOnlyList<ClanNarrativeSnapshot> ClanNarratives;
    IReadOnlyList<SocialMemoryEntrySnapshot> SocialMemories;
    IReadOnlyList<PersonDossierSnapshot> PersonDossiers;
    IReadOnlyList<SettlementSnapshot> Settlements;
    IReadOnlyList<PopulationSettlementSnapshot> PopulationSettlements;
    IReadOnlyList<HouseholdPressureSnapshot> Households;
    IReadOnlyList<HouseholdSocialPressureSnapshot> HouseholdSocialPressures;
    IReadOnlyList<EducationCandidateSnapshot> EducationCandidates;
    IReadOnlyList<AcademySnapshot> Academies;
    IReadOnlyList<ClanTradeSnapshot> ClanTrades;
    IReadOnlyList<MarketSnapshot> Markets;
    IReadOnlyList<ClanTradeRouteSnapshot> ClanTradeRoutes;
    IReadOnlyList<SettlementPublicLifeSnapshot> PublicLifeSettlements;
    IReadOnlyList<SettlementDisorderSnapshot> SettlementDisorder;
    IReadOnlyList<OfficeCareerSnapshot> OfficeCareers;
    IReadOnlyList<JurisdictionAuthoritySnapshot> OfficeJurisdictions;
    IReadOnlyList<SettlementGovernanceLaneSnapshot> GovernanceSettlements;
    GovernanceFocusSnapshot GovernanceFocus;
    GovernanceDocketSnapshot GovernanceDocket;
    HallDocketStackSnapshot HallDocket;
    PlayerInfluenceFootprintSnapshot InfluenceFootprint;
    IReadOnlyList<CampaignFrontSnapshot> Campaigns;
    IReadOnlyList<CampaignMobilizationSignalSnapshot> CampaignMobilizationSignals;
    IReadOnlyList<NarrativeNotificationSnapshot> Notifications;
    PlayerCommandSurfaceSnapshot PlayerCommands;
    PresentationDebugSnapshot Debug;
}

public sealed class HouseholdSocialPressureSnapshot {
    HouseholdId HouseholdId;
    string HouseholdName;
    SettlementId SettlementId;
    ClanId? SponsorClanId;
    LivelihoodType Livelihood;
    string PrimaryDriftKey;
    string PrimaryDriftLabel;
    int PressureScore;
    bool IsPlayerAnchor;
    string AttachmentSummary;
    string VisibleChainSummary;
    IReadOnlyList<HouseholdSocialPressureSignalSnapshot> Signals;
}

public sealed class PersonDossierSnapshot {
    PersonId PersonId;
    string DisplayName;
    LifeStage LifeStage;
    PersonGender Gender;
    bool IsAlive;
    FidelityRing FidelityRing;
    ClanId? ClanId;
    string ClanName;
    string BranchPositionLabel;
    string KinshipSummary;
    string TemperamentSummary;
    HouseholdId? HouseholdId;
    string HouseholdName;
    string LivelihoodSummary;
    string HealthSummary;
    string ActivitySummary;
    string EducationSummary;
    string TradeSummary;
    string OfficeSummary;
    string MemoryPressureSummary;
    string DormantMemorySummary;
    string SocialPositionLabel;
    string CurrentStatusSummary;
    IReadOnlyList<string> SourceModuleKeys;
}

public sealed class PlayerInfluenceFootprintSnapshot {
    HouseholdId? AnchorHouseholdId;
    string AnchorHouseholdName;
    string AnchorHouseholdSummary;
    string EntryPositionLabel;
    string Summary;
    IReadOnlyList<InfluenceReachSnapshot> Reaches;
}

public sealed class InfluenceReachSnapshot {
    string ReachKey;
    string Label;
    bool IsActive;
    bool HasCommandAffordance;
    bool IsPlayerAnchor;
    bool HasLocalAgency;
    int ReachScore;
    string LeverageSummary;
    string LocalAgencySummary;
    string ConstraintSummary;
    string CommandSummary;
}

public sealed class PlayerCommandSurfaceSnapshot {
    IReadOnlyList<PlayerCommandAffordanceSnapshot> Affordances;
    IReadOnlyList<PlayerCommandReceiptSnapshot> Receipts;
}

public sealed class PlayerCommandAffordanceSnapshot {
    string ModuleKey;
    string SurfaceKey;
    SettlementId SettlementId;
    ClanId? ClanId;
    string CommandName;
    string Label;
    string Summary;
    bool IsEnabled;
    string AvailabilitySummary;
    string ExecutionSummary;
    string LeverageSummary;
    string CostSummary;
    string ReadbackSummary;
    string TargetLabel;
}

public sealed class PlayerCommandReceiptSnapshot {
    string ModuleKey;
    string SurfaceKey;
    SettlementId SettlementId;
    ClanId? ClanId;
    string CommandName;
    string Label;
    string Summary;
    string OutcomeSummary;
    string ExecutionSummary;
    string LeverageSummary;
    string CostSummary;
    string ReadbackSummary;
    string TargetLabel;
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

Current note:
- the read-model bundle now carries `ClanNarratives` so lineage conflict, shame, and favor pressure can be shown in the family council without reading module state directly
- the read-model bundle now also carries `SocialMemories` so family, public-life order receipts, governance lanes, and shell adapters can show durable SocialMemory-owned residue without querying module state from UI
- the read-model bundle now also carries runtime-only `PersonDossiers` composed from existing `PersonRegistry`, `FamilyCore`, `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, and optional `SocialMemoryAndRelations` queries; this does not add a root schema, module schema, save namespace, migration, or authoritative person table
- the read-model bundle now also carries `Households`, `HouseholdSocialPressures`, and `InfluenceFootprint` as runtime-only joins across household, lineage, market, education, yamen, public-life, order, and force projections; these fields are not saved and do not create a player route system
- `HouseholdSocialPressures` may include runtime-only v10 keys such as `HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue` and `HouseholdSocialDriftKeys.PublicOrderAftermath` to show ordinary-household public-life/order after-account readback. These keys are read-model constants, not persisted module state, and they do not require a schema bump or migration.
- v11 may use that projected household pressure to enrich `PlayerCommandAffordanceSnapshot` and `PlayerCommandReceiptSnapshot` leverage / cost / execution / readback strings for public-life refusal responses. This remains runtime presentation data; it does not add `HouseholdId` to `PlayerCommandRequest`, does not add module state, and does not require a schema bump or migration.
- v12 adds persisted `PopulationAndHouseholds` local response traces for home-household commands. The projected affordances / receipts still come from read models, and `PlayerCommandRequest` still stays settlement / optional clan scoped; the persisted authority change is limited to population-owned household pressure and local response trace fields.
- v15 common-household response texture adds no new schema shape: command-time texture is derived from existing `HouseholdPressureSnapshot` / `PopulationHouseholdState` fields (`DebtPressure`, `LaborCapacity`, `Distress`, `MigrationRisk`, `DependentCount`, `LaborerCount`, `Livelihood`) and projected as runtime `本户底色` text only.
- v16 home-household response capacity adds no new schema shape: projected `回应承受线` / `承受线代价` / `承受线读回` strings are runtime read-model data, and command-time capacity summaries reuse existing `LastLocalResponseSummary` rather than adding a capacity ledger.
- v17 home-household response tradeoff forecast adds no new schema shape: projected `取舍预判` / `预期收益` / `反噬尾巴` / `外部后账` strings are runtime read-model data, and command-time tradeoff summaries reuse existing `LastLocalResponseSummary` rather than adding a tradeoff ledger.
- v19 home-household follow-up affordance adds no new schema shape: projected `续接提示` / `换招提示` / `冷却提示` / `续接读回` strings are runtime read-model data and do not add a cooldown ledger or repeated-response counter.
- v20 home-household owner-lane return guidance adds no new schema shape: projected `外部后账归位`, `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` strings are runtime read-model data and do not add an owner-lane ledger, household target field, or module state.
- v21 owner-lane surface readback adds no new schema shape: projected Office/Governance and Family-facing owner-lane strings reuse existing `PlayerCommandAffordanceSnapshot` / `GovernanceDocketSnapshot` fields and do not add an owner-lane ledger, household target field, module state, or migration.
- v23 owner-lane receipt status readback adds no new schema shape: projected `归口状态` strings reuse existing `PlayerCommandAffordanceSnapshot` / `GovernanceDocketSnapshot` fields and existing owner-module response traces; they do not add a receipt-status ledger, owner-lane ledger, household target field, module state, or migration.
- `InfluenceFootprint` distinguishes the player's anchor household (`OwnHousehold`, local agency) from observed household pressure (`ObservedHouseholds`, indirect influence only)
- `PlayerCommands` now spans family, office, order, and warfare affordances/receipts as read-only presentation data only
- public-life order command affordances/receipts may include runtime-only `LeverageSummary`, `CostSummary`, and `ReadbackSummary` strings; v5 readback may include structured `县门未落地`, `地方拖延`, and `后账仍在` text plus SocialMemory refusal residue, v6/v7 readback may include repaired / contained / escalated / ignored response residue and later `后账渐平` / `后账转硬` SocialMemory summaries, v8 readback may include owner-module actor countermove labels such as `巡丁自补保`, `胥吏续拖`, `县门自补落地`, `族老自解释`, or `族老避羞`, and governance docket may copy the projected receipt/gateway text for next-month readback. v12 home-household local response receipts may also show `本户已缓`, `本户暂压`, `本户吃紧`, or `本户放置`; v16-v30 may add runtime-only `回应承受线`, `取舍预判`, `短期后果`, `续接提示`, `外部后账归位`, owner-lane surface readback, `承接入口`, `归口状态`, `归口后读法`, `社会余味读回`, `余味冷却提示` / `余味续接提示` / `余味换招提示`, `现有入口读法`, `后手收口读回`, and `闭环防回压` strings; only the v12 `PopulationAndHouseholds` local response trace fields are saved, while projection strings remain non-authoritative.
- family command targeting is expressed through optional `ClanId` plus `TargetLabel`; it does not create a new save namespace
- v77-v84 warfare directive-choice readback may appear in runtime `PlayerCommandAffordanceSnapshot.ReadbackSummary` and `PlayerCommandReceiptSnapshot.ReadbackSummary` as `军令选择读回`, `粮道护持选择`, or `WarfareCampaign拥有军令`. These strings are not saved as authoritative projection state and do not add a schema namespace, migration, directive ledger, owner-lane ledger, or save-manifest entry.

Diagnostics harness note:
- longer multi-seed sweep reports and budget evaluations are runtime-only application diagnostics
- per-module diff/event activity peaks are also runtime-only diagnostics
- local-conflict interaction-pressure summaries are also runtime-only diagnostics
- settlement pressure distributions are also runtime-only diagnostics
- named settlement hotspot summaries for local-conflict stress runs are also runtime-only diagnostics
- load-migration summaries shown in debug/presentation are runtime-only diagnostics derived from the active load path
- scale summaries and top module payload footprints are also runtime-only diagnostics
- event-contract health classifications, the v33 no-unclassified gate, and the v34 owner/evidence backlinks for emitted-but-unconsumed or declared-but-not-emitted `DomainEvent` names are also runtime-only diagnostics
- payload-summary headlines and migration-consistency status are also runtime-only diagnostics
- player-command affordances and receipts in the presentation bundle are also runtime-only read models
- household social-pressure and influence-footprint snapshots in the presentation bundle are also runtime-only read models
- person dossiers in the presentation bundle are also runtime-only read models
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

Current backend household-family burden v36 note:
- v36 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, relief ledger, sponsor-lane ledger, household target field, or projection cache.
- `PopulationAndHouseholds` remains schema `3`; `FamilyCore` remains schema `8`; `SocialMemoryAndRelations` remains schema `3`.
- New event metadata keys for household-family burden are runtime `DomainEvent` payload only. They are not saved fields and do not require a migration.
- The v36 handler reuses existing family-owned fields such as charity obligation, support reserve, branch tension, relief sanction pressure, and lifecycle trace/outcome readback.

Current backend office/yamen readback v38-v45 note:
- v38-v45 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, policy ledger, yamen workflow state, owner-lane ledger, cooldown ledger, household target field, or projection cache.
- `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New governance read-model fields such as `OfficeImplementationReadbackSummary`, `OfficeNextStepReadbackSummary`, `RegimeOfficeReadbackSummary`, `CanalRouteReadbackSummary`, and `ResidueHealthSummary` are runtime projections only and are not saved.
- The later SocialMemory office/yamen residue reuses existing memory/narrative/climate records and cause keys. It does not add a SocialMemory field, memory namespace, relationship table, or migration.

Current backend office-lane closure v46-v52 note:
- v46-v52 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, policy ledger, yamen workflow state, owner-lane ledger, receipt-status ledger, outcome ledger, cooldown ledger, follow-up ledger, household target field, or projection cache.
- `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New governance / Unity read-model fields `OfficeLaneEntryReadbackSummary`, `OfficeLaneReceiptClosureSummary`, `OfficeLaneResidueFollowUpSummary`, and `OfficeLaneNoLoopGuardSummary` are runtime projections only and are not saved.
- These fields are derived from existing structured `JurisdictionAuthoritySnapshot` fields, office response trace codes, and `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State`; they are not derived from `DomainEvent.Summary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- Ordinary home-household response remains a low-power `PopulationAndHouseholds` lane. It can explain local relief/strain, but it is not a universal repair lane for yamen documents, clerk delay, route pressure, clan guarantee face, or durable social residue.

Current backend Family-lane closure v53-v60 note:
- v53-v60 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, Family closure ledger, guarantee ledger, owner-lane ledger, receipt-status ledger, outcome ledger, cooldown ledger, follow-up ledger, household target field, or projection cache.
- `FamilyCore` remains schema `8`; `PopulationAndHouseholds` remains schema `3`; `SocialMemoryAndRelations` remains schema `3`.
- New read-model / Unity fields `FamilyLaneEntryReadbackSummary`, `FamilyElderExplanationReadbackSummary`, `FamilyGuaranteeReadbackSummary`, `FamilyHouseFaceReadbackSummary`, `FamilyLaneReceiptClosureSummary`, `FamilyLaneResidueFollowUpSummary`, and `FamilyLaneNoLoopGuardSummary` are runtime projections only and are not saved.
- These fields are derived from existing structured `ClanSnapshot`, `HouseholdPressureSnapshot`, `SponsorClanId`, Family response trace codes, and `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State`; they are not derived from `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or projected Family prose.
- Ordinary home-household response remains a low-power `PopulationAndHouseholds` lane. It can explain local relief/strain, but it is not a universal repair lane for clan elder explanation, household guarantee, lineage-house face, sponsor-clan pressure, or durable social residue.

Current backend Family relief choice v61-v68 note:
- v61-v68 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, relief ledger, charity ledger, guarantee ledger, Family closure ledger, owner-lane ledger, cooldown ledger, household target field, or projection cache.
- `FamilyCore` remains schema `8`; `PopulationAndHouseholds` remains schema `3`; `SocialMemoryAndRelations` remains schema `3`.
- `GrantClanRelief` reuses existing `FamilyCore` persisted fields: `CharityObligation`, `SupportReserve`, `BranchTension`, `BranchFavorPressure`, `ReliefSanctionPressure`, `MediationMomentum`, and existing conflict receipt fields.
- New readback text such as `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, and `不是普通家户再扛` is runtime projection only and is not saved.
- Ordinary home-household response remains a low-power `PopulationAndHouseholds` lane. It is not a universal repair lane for Family relief, lineage-house face, sponsor-clan pressure, or durable social residue.

Current backend Force/Campaign/Regime owner-lane readback v69-v76 note:
- v69-v76 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, or projection cache.
- `ConflictAndForce`, `WarfareCampaign`, `OfficeAndCareer`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` schema versions remain unchanged.
- New read-model / Unity fields `WarfareLaneEntryReadbackSummary`, `ForceReadinessReadbackSummary`, `CampaignAftermathReadbackSummary`, `WarfareLaneReceiptClosureSummary`, `WarfareLaneResidueFollowUpSummary`, and `WarfareLaneNoLoopGuardSummary` are runtime projections only and are not saved.
- These fields are derived from existing structured campaign/force/office/clan snapshots and `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State` / `OriginDate`; they are not derived from `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or projected military prose.
- Ordinary home-household response remains a low-power `PopulationAndHouseholds` lane. It is not a universal repair lane for campaign aftermath, force readiness, military order, regime coordination, or durable social residue.

Current backend Warfare aftermath docket readback v85-v92 note:
- v85-v92 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, aftermath ledger, relief ledger, route-repair ledger, owner-lane ledger, cooldown ledger, household target field, or projection cache.
- `WarfareCampaign` remains schema `4`; existing persisted `AftermathDocketState` fields (`Merits`, `Blames`, `ReliefNeeds`, `RouteRepairs`, and `DocketSummary`) already carry the campaign aftermath docket.
- New runtime read-model exposure `PresentationReadModelBundle.CampaignAftermathDockets` and readback text such as `战后案卷读回`, `记功簿读回`, `劾责状读回`, `抚恤簿读回`, `清路札读回`, `WarfareCampaign拥有战后案卷`, `战后案卷不是县门/Order代算`, and `不是普通家户补战后` are projections only and are not saved.
- Application/Unity read structured docket lists and counts only; they do not parse `DocketSummary`, receipt prose, `LastDirectiveTrace`, `DomainEvent.Summary`, or SocialMemory summary prose.

Current backend court-policy process readback v93-v100 note:
- v93-v100 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, court module, dispatch ledger, policy closure ledger, owner-lane ledger, cooldown ledger, household target field, or projection cache.
- `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `WorldSettlements`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` schema versions remain unchanged.
- New read-model / Unity fields `CourtPolicyEntryReadbackSummary`, `CourtPolicyDispatchReadbackSummary`, `CourtPolicyPublicReadbackSummary`, and `CourtPolicyNoLoopGuardSummary` are runtime projections only and are not saved.
- These fields are derived from structured `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` values; they are not derived from `DomainEvent.Summary`, receipt prose, `LastAdministrativeTrace`, `LastPetitionOutcome`, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

Current thin-chain closeout audit v101-v108 note:
- v101-v108 adds no data shape. It records that v3-v100 thin-chain topology/readback evidence is closed while full-chain rule density remains future work.
- No module envelope, root schema version, module schema version, save-manifest membership, migration, persisted projection cache, owner-lane ledger, cooldown ledger, dispatch ledger, relief ledger, aftermath ledger, household target field, or serialized payload shape changes.
- Because this is docs/test governance only, existing module schema versions remain unchanged.

Current court-policy process thickening v109-v116 note:
- v109-v116 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, household target field, or projection cache.
- `WorldSettlements` remains schema `8`; `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New court-policy thickening text such as `政策语气读回`, `文移指向读回`, `县门承接姿态`, `公议承压读法`, `朝廷后手仍不直写地方`, and `不是本户硬扛朝廷后账` is runtime projection / owner-lane prose over existing structured metadata and snapshots. It is not saved and does not require migration.

Current court-policy local response v117-v124 note:
- v117-v124 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, household target field, or projection cache.
- `WorldSettlements` remains schema `8`; `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New local-response guidance such as `政策回应入口`, `文移续接选择`, `县门轻催`, `递报改道`, `公议降温只读回`, and `不是本户硬扛朝廷后账` is runtime command/readback projection over existing Office/PublicLife fields and existing Office response traces. It is not saved and does not require migration.

Current court-policy memory-pressure readback v133-v140 note:
- v133-v140 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, memory-pressure ledger, household target field, or projection cache.
- `WorldSettlements` remains schema `8`; `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New memory-pressure guidance such as `政策旧账回压读回`, `旧文移余味`, `下一次政策窗口读法`, `公议旧读法续压`, and `不是本户硬扛朝廷旧账` is runtime projection over existing SocialMemory and Office/PublicLife snapshots. It is not saved and does not require migration.

Current court-policy public-reading echo v141-v148 note:
- v141-v148 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, memory-pressure ledger, public-reading ledger, household target field, or projection cache.
- `WorldSettlements` remains schema `8`; `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New public-reading guidance such as `政策公议旧读回`, `公议旧账回声`, `下一次榜示/递报旧读法`, `PublicLife只读街面解释`, and `县门承接仍归OfficeAndCareer` is runtime projection over existing SocialMemory and Office/PublicLife snapshots. It is not saved and does not require migration.

Current court-policy public follow-up cue v149-v156 note:
- v149-v156 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, memory-pressure ledger, public-reading ledger, public-follow-up ledger, household target field, or projection cache.
- `WorldSettlements` remains schema `8`; `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New public follow-up guidance such as `政策公议后手提示`, `公议冷却提示`, `公议轻续提示`, `公议换招提示`, `下一步仍看榜示/递报承口`, and `不是冷却账本` is runtime projection over existing SocialMemory cause data and PublicLife snapshots. It is not saved and does not require migration.

Current court-policy follow-up docket guard v157-v164 note:
- v157-v164 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, Court module, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, memory-pressure ledger, public-reading ledger, public-follow-up ledger, docket ledger, household target field, or projection cache.
- `WorldSettlements` remains schema `8`; `OfficeAndCareer` remains schema `7`; `PublicLifeAndRumor` remains schema `4`; `SocialMemoryAndRelations` remains schema `3`.
- New docket guard guidance such as `政策后手案牍防误读`, `公议后手只作案牍提示`, `不是Order后账`, `不是Office成败`, and `仍等Office/PublicLife/SocialMemory分读` is runtime projection over existing SocialMemory cause data and PublicLife snapshots. It is not saved and does not require migration.

Current social mobility fidelity ring v213-v244 note:
- v213-v244 adds no new persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, social-mobility ledger, movement ledger, focus ledger, projection cache, or `PersonRegistry` domain expansion.
- `PopulationAndHouseholds` remains schema `3`; it now actively rebuilds already-persisted household livelihood, membership activity, labor-pool, marriage-pool, and migration-pool carriers during deterministic monthly/xun passes.
- `PersonRegistry` remains schema `1`; `ChangeFidelityRing` mutates only existing identity/fidelity-ring state and emits a structured receipt event. Household, livelihood, activity, relation, office, and memory facts stay outside `PersonRegistry`.
- New `FidelityScaleSnapshot`, `SettlementMobilitySnapshot`, person dossier movement/fidelity readbacks, runtime observability counters, and Unity shell mobility strings are runtime read models or diagnostics only and are not saved.

Current social mobility fidelity ring closeout v245-v252 note:
- v245-v252 is docs/test governance only and adds no persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, projection cache, or serialized payload change.
- The closeout does not authorize a movement ledger, social-mobility ledger, focus ledger, owner-lane ledger, scheduler ledger, dormant-stub store, or `PersonRegistry` domain expansion.
- Future durable movement residue, demotion/dormant-state, or richer migration/class mobility state must arrive with an explicit schema/migration plan before any field is added.

Current social mobility scale budget guard v269-v276 note:
- v269-v276 is docs/test governance only and adds no persisted fields, module envelope, root schema version, module schema version, save-manifest membership, migration, projection cache, or serialized payload change.
- The four precision bands are runtime design/architecture guidance over existing state: close-orbit named detail, influence/pressure selective detail, active-region structured pools, and distant-world pressure summaries.
- The guard does not authorize a movement ledger, social-mobility ledger, focus ledger, scheduler ledger, global person simulation manager, dormant-stub store, or `PersonRegistry` domain expansion.

Current social mobility influence readback v277-v284 note:
- v277-v284 adds runtime read-model / Unity ViewModel fields only: `InfluenceFootprintReadbackSummary` and `ScaleBudgetReadbackSummary`.
- These fields are not module state and are not saved. They require no module schema version, root save version, migration, manifest change, projection cache, or serialized module payload change.
- The pass does not authorize a movement ledger, social-mobility ledger, focus ledger, scheduler ledger, global person simulation manager, dormant-stub store, SocialMemory movement residue, or `PersonRegistry` domain expansion.

Current social mobility boundary closeout v285-v292 note:
- v285-v292 is docs/tests governance only. It adds no persisted fields, module envelope, root save version, module schema version, migration, manifest membership, projection cache, or serialized module payload change.
- The closeout does not authorize a movement ledger, social-mobility ledger, focus ledger, scheduler ledger, command ledger, personnel ledger, global person simulation manager, dormant-stub store, SocialMemory movement residue, or `PersonRegistry` domain expansion.
- Any future durable movement residue, dormant-stub state, migration economy, personnel command state, or cross-region flow cache must open a schema/migration plan before implementation.

Current personnel command preflight v293-v300 note:
- v293-v300 is docs/tests governance only. It adds no persisted fields, module envelope, root save version, module schema version, migration, manifest membership, projection cache, command receipt shape, or serialized module payload change.
- The preflight does not authorize direct move/transfer/summon/assign-person state, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, global person simulation manager, dormant-stub store, durable movement residue, or `PersonRegistry` domain expansion.
- Any future persisted personnel-command receipt, assignment state, durable movement residue, dormant-stub store, migration economy, or cross-region flow cache must open a schema/migration plan before implementation.

Current personnel flow command readiness v301-v308 note:
- `PersonnelFlowReadinessSummary` is a runtime player-command read-model/ViewModel field only. It is not saved in a module namespace, save envelope, manifest, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, durable residue store, or serialized module payload.
- `PopulationAndHouseholds` remains schema `3`; `PersonRegistry` remains identity/fidelity only. No root save version, module schema version, migration, or manifest membership changes are required.
- Any future persisted personnel-flow receipt, movement state, assignment state, durable residue, migration economy, dormant-stub store, or cross-region flow cache must open a separate schema/migration plan before implementation.

Current personnel flow surface echo v309-v316 note:
- `PlayerCommandSurfaceSnapshot.PersonnelFlowReadinessSummary` is runtime read-model text only. It is assembled from existing runtime affordance/receipt `PersonnelFlowReadinessSummary` fields and is not persisted.
- It adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, durable residue, or serialized module payload.
- Any future persisted personnel-flow command history, movement receipt, surface echo cache, or durable social residue must open a separate schema/migration plan before implementation.

Current personnel flow readiness closeout v317-v324 note:
- V317-V324 is docs/tests only and adds no runtime save shape.
- The v293-v316 layer remains runtime read-model/ViewModel/projection work only. It adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, closeout ledger, durable residue, or serialized module payload.
- Any future persisted personnel-flow command history, migration state, movement receipt, or durable social residue still requires a separate schema/migration plan before implementation.

Current personnel flow owner-lane gate v325-v332 note:
- `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary` is runtime read-model text only. It is assembled from existing runtime command affordance/receipt metadata and is not persisted.
- It adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, owner-lane-gate ledger, durable residue, or serialized module payload.
- Any future persisted owner-lane selection, personnel-flow command history, migration state, movement receipt, assignment state, or durable social residue still requires a separate schema/migration plan before implementation.

Current personnel flow desk gate echo v333-v340 note:
- Desk Sandbox use of `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary` is runtime presentation only. It is not persisted.
- It adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, desk-gate ledger, durable residue, or serialized module payload.
- Any future persisted local gate history, owner-lane selection, personnel-flow command history, migration state, movement receipt, assignment state, or durable social residue still requires a separate schema/migration plan before implementation.

Current personnel flow desk gate containment v341-v348 note:
- V341-V348 is tests/docs plus presentation containment over existing runtime fields. It adds no persisted fields.
- The negative desk echo proof does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, desk-gate containment ledger, durable residue, or serialized module payload.
- Any future persisted settlement-local owner-lane gate history, movement receipt, assignment state, migration state, or durable social residue still requires a separate schema/migration plan before implementation.

Current personnel flow gate closeout v349-v356 note:
- V349-V356 is docs/tests closeout only. It adds no persisted fields.
- The closeout does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, owner-lane-gate ledger, durable residue, or serialized module payload.
- Any future persisted personnel-flow command history, owner-lane selection, movement receipt, assignment state, migration state, or durable social residue still requires a separate schema/migration plan before implementation.

Current personnel flow future owner-lane preflight v357-v364 note:
- V357-V364 is docs/tests preflight only. It adds no persisted fields.
- The preflight does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, future-owner-lane ledger, durable residue, or serialized module payload.
- Any future Family/Office/Warfare personnel-flow owner lane with persisted state, command history, movement receipt, assignment state, migration state, or durable social residue still requires a separate schema/migration plan before implementation.

Current personnel flow future lane surface v365-v372 note:
- `PlayerCommandSurfaceSnapshot.PersonnelFlowFutureOwnerLanePreflightSummary` is runtime read-model text only and is not persisted.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, future-lane-surface ledger, durable residue, or serialized module payload.
- Any future persisted Family/Office/Warfare personnel-flow lane, command history, movement receipt, assignment state, migration state, projection cache, or durable social residue still requires a separate schema/migration plan before implementation.

Current personnel flow future lane closeout v373-v380 note:
- V373-V380 is docs/tests closeout only and adds no persisted fields.
- The closeout does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, future-lane-closeout ledger, durable residue, or serialized module payload.
- Any future Family/Office/Warfare personnel-flow owner lane with persisted state, command history, movement receipt, assignment state, migration state, projection cache, or durable social residue still requires a separate schema/migration plan before implementation.

Current commoner social position preflight v381-v388 note:
- V381-V388 is docs/tests preflight only and adds no persisted fields.
- The preflight does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, class ledger, social-position ledger, personnel ledger, movement ledger, focus ledger, scheduler ledger, durable social-position residue, or serialized module payload.
- Any future commoner status drift, tenant/landholding conversion, office-service status, trade attachment, clerk path, artisan/merchant route, or durable social-position residue still requires a separate owner-module schema/migration plan before implementation.

Current commoner social position readback v389-v396 note:
- `PersonDossierSnapshot.SocialPositionReadbackSummary` and `PersonDossierViewModel.SocialPositionReadbackSummary` are runtime read-model/ViewModel fields only and are not persisted.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, class ledger, social-position ledger, personnel ledger, movement ledger, focus ledger, scheduler ledger, readback ledger, durable social-position residue, or serialized module payload.
- Any future persisted status drift, conversion state, route history, durable residue, or projection cache still requires a separate owner-module schema/migration plan before implementation.

Current social position owner-lane keys v397-v404 note:
- `PersonDossierSnapshot.SocialPositionSourceModuleKeys` and `PersonDossierViewModel.SocialPositionSourceModuleKeys` are runtime read-model/ViewModel fields only and are not persisted.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, class ledger, social-position ledger, personnel ledger, movement ledger, source-key ledger, durable social-position residue, or serialized module payload.
- Any future persisted source provenance, status drift, conversion state, route history, durable residue, or projection cache still requires a separate owner-module schema/migration plan before implementation.

Current social position readback closeout v405-v412 note:
- V405-V412 is docs/tests closeout only and adds no persisted fields.
- The closeout does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, class ledger, social-position ledger, personnel ledger, movement ledger, closeout ledger, durable social-position residue, or serialized module payload.
- Any future persisted commoner status drift, conversion state, route history, durable residue, source provenance, or projection cache still requires a separate owner-module schema/migration plan before implementation.

Current social position scale budget v413-v420 note:
- `PersonDossierSnapshot.SocialPositionScaleBudgetReadbackSummary` and `PersonDossierViewModel.SocialPositionScaleBudgetReadbackSummary` are runtime read-model/ViewModel fields only and are not persisted.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, precision-band state, class ledger, social-position ledger, personnel ledger, movement ledger, scale-budget ledger, durable social-position residue, or serialized module payload.
- Any future persisted precision-band state, commoner status drift, conversion state, route history, durable residue, source provenance, or projection cache still requires a separate owner-module schema/migration plan before implementation.

Current social position regional scale guard v421-v428 note:
- V421-V428 is tests/docs guard only and adds no persisted fields.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, precision-band state, class ledger, social-position ledger, personnel ledger, movement ledger, scale-budget ledger, durable social-position residue, or serialized module payload.
- Any future persisted regional precision, selection state, commoner status drift, conversion state, route history, durable residue, source provenance, or projection cache still requires a separate owner-module schema/migration plan before implementation.

Current social position scale closeout v429-v436 note:
- V429-V436 is docs/tests closeout only and adds no persisted fields.
- The closeout does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, precision-band state, class ledger, social-position ledger, personnel ledger, movement ledger, source-key ledger, scale-budget ledger, closeout ledger, durable social-position residue, or serialized module payload.
- Any future persisted commoner status drift, conversion state, route history, precision state, source provenance, durable residue, or projection cache still requires a separate owner-module schema/migration plan before implementation.

Current commoner status owner-lane preflight v437-v444 note:
- V437-V444 is docs/tests preflight only and adds no persisted fields.
- The preflight does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, promote/demote state, zhuhu/kehu conversion state, precision-band state, class ledger, social-position ledger, commoner-status ledger, personnel ledger, movement ledger, owner-lane ledger, preflight ledger, durable social-position residue, or serialized module payload.
- Any future persisted commoner status drift in `PopulationAndHouseholds` still requires a separate schema/migration plan before implementation.

Current fidelity scale budget preflight v445-v452 note:
- V445-V452 is docs/tests preflight only and adds no persisted fields.
- The preflight does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, precision-band state, selector state, target-cardinality state, class ledger, social-position ledger, commoner-status ledger, personnel ledger, movement ledger, scale-budget ledger, fidelity-budget ledger, durable residue, or serialized module payload.
- Any future persisted precision band, selector watermark, commoner status drift, movement route, durable residue, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility dynamics explanation v453-v460 note:
- `HouseholdSocialPressureSnapshot.MobilityDynamicsExplanationSummary`, `HouseholdSocialPressureSnapshot.MobilityDynamicsDimensionKeys`, and `SettlementNodeViewModel.HouseholdMobilityDynamicsSummary` are runtime read-model/ViewModel fields only.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, selector state, target-cardinality state, class ledger, commoner-status ledger, household-mobility ledger, movement ledger, durable residue, or serialized module payload.
- Any future persisted mobility explanation history, route history, selector watermark, commoner status drift, durable residue, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility dynamics closeout v461-v468 note:
- v461-v468 is docs/tests closeout only. It does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, projection cache, selector state, target-cardinality state, route-history state, class ledger, commoner-status ledger, household-mobility ledger, movement ledger, closeout ledger, durable residue, or serialized module payload.
- Any future persisted household mobility history, movement route, selector watermark, commoner status drift, durable residue, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility first hardcoded rule extraction v525-v532 note:
- `PopulationHouseholdMobilityRulesData` is runtime owner-consumed code shape only inside `PopulationAndHouseholds`.
- The focused member promotion cap default remains 2 and is not persisted.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, rules-data file, content/rules-data namespace, projection cache, selector state, target-cardinality state, route-history state, class ledger, commoner-status ledger, household-mobility ledger, movement ledger, extraction ledger, durable residue, or serialized module payload.
- Any future persisted rules-data file, route history, selector watermark, commoner status drift, movement residue, durable residue, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility first runtime rule v533-v540 note:
- Existing state is sufficient for the first runtime rule. The rule uses `MigrationPools`, `MigrationRisk`, `IsMigrating`, livelihood, distress, debt, labor, grain, and land fields already present in schema 3.
- Monthly threshold/cap/delta values live in runtime owner code shape only and are not persisted or loaded from a save/content namespace.
- The pass does not add a module namespace, save envelope field, manifest membership, root/module schema version, migration, rules-data file, content/rules-data namespace, projection cache, selector state, target-cardinality state, route-history state, movement ledger, cooldown ledger, owner-lane ledger, class ledger, commoner-status ledger, household-mobility ledger, durable residue, or serialized module payload.
- Any future persisted movement history, route history, cooldown, selector watermark, commoner status drift, movement residue, durable residue, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility first runtime rule closeout v541-v548 note:
- V541-V548 is docs/tests closeout and does not change `PopulationAndHouseholds` schema 3.
- The closeout adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, rules-data file, content/rules-data namespace, projection cache, selector state, target-cardinality state, route-history state, movement ledger, cooldown ledger, owner-lane ledger, class ledger, commoner-status ledger, household-mobility ledger, durable residue, or serialized module payload.
- The V533-V540 first runtime rule continues to use only existing `MigrationPools`, `MigrationRisk`, `IsMigrating`, livelihood, distress, debt, labor, grain, and land fields.
- Any future persisted movement history, route history, cooldown, selector watermark, commoner status drift, movement residue, durable residue, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility runtime rule health evidence v549-v556 note:
- V549-V556 is diagnostics/readiness evidence and does not change `PopulationAndHouseholds` schema 3.
- The pass adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, rules-data file, content/rules-data namespace, projection cache, selector state, target-cardinality state, route-history state, movement ledger, cooldown ledger, owner-lane ledger, class ledger, commoner-status ledger, household-mobility ledger, diagnostic state, durable residue, or serialized module payload.
- The V533-V540 first runtime rule continues to use only existing `MigrationPools`, `MigrationRisk`, `IsMigrating`, livelihood, distress, debt, labor, grain, and land fields.
- Any future persisted movement history, route history, cooldown, selector watermark, long-run health marker, commoner status drift, movement residue, durable residue, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility runtime widening gate v557-v564 note:
- V557-V564 is docs/tests preflight and does not change `PopulationAndHouseholds` schema 3.
- The pass adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, rules-data file, content/rules-data namespace, projection cache, selector state, target-cardinality state, route-history state, movement ledger, cooldown ledger, owner-lane ledger, class ledger, commoner-status ledger, household-mobility ledger, touch-count state, diagnostic state, performance cache, durable residue, or serialized module payload.
- The V533-V540 first runtime rule continues to use only existing `MigrationPools`, `MigrationRisk`, `IsMigrating`, livelihood, distress, debt, labor, grain, and land fields.
- Any future persisted touched counts, movement history, route history, cooldown, selector watermark, long-run health marker, commoner status drift, movement residue, durable residue, performance cache, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility runtime touch-count proof v565-v572 note:
- V565-V572 is focused test evidence and does not change `PopulationAndHouseholds` schema 3.
- The pass adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, rules-data file, content/rules-data namespace, projection cache, selector state, target-cardinality state, route-history state, movement ledger, cooldown ledger, owner-lane ledger, class ledger, commoner-status ledger, household-mobility ledger, persisted touch-count state, diagnostic state, performance cache, durable residue, or serialized module payload.
- The proof counts touched households only in tests by comparing existing risk deltas against a zero-risk-delta baseline; no runtime counter, diagnostic state, projection field, or persisted payload is introduced.
- Any future persisted touched counts, movement history, route history, cooldown, selector watermark, long-run health marker, commoner status drift, movement residue, durable residue, performance cache, or projection cache still requires a separate schema/migration plan before implementation.

Current household mobility rules-data fallback matrix v573-v580 note:
- V573-V580 is focused fallback evidence and does not change `PopulationAndHouseholds` schema 3.
- The pass adds no module namespace, save envelope field, manifest membership, root/module schema version, migration, rules-data file, content/rules-data namespace, projection cache, selector state, target-cardinality state, route-history state, movement ledger, cooldown ledger, owner-lane ledger, class ledger, commoner-status ledger, household-mobility ledger, persisted touch-count state, diagnostic state, performance cache, durable residue, serialized validation ledger, or serialized module payload.
- Malformed runtime rules-data fallback is test evidence over existing defaults; no runtime loader, default file, config namespace, plugin system, or persisted validation state is introduced.
- Any future persisted rules-data inventory, validation ledger, touched counts, movement history, route history, cooldown, selector watermark, long-run health marker, commoner status drift, movement residue, durable residue, performance cache, or projection cache still requires a separate schema/migration plan before implementation.

## 7. Invariants
- dead people cannot hold active pregnancy, study attendance, office duty, or active campaign assignment
- spouse links must be symmetric
- parent/child age relationships must remain plausible
- module state may reference only valid core entity IDs
- module-local references may not point into disabled modules without a documented null/default policy
Current household mobility runtime threshold no-touch v581-v588 note:

V581-V588 adds focused tests/docs proving active-pool threshold blocking for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime zero-cap no-touch v589-v596 note:

V589-V596 adds focused tests/docs proving zero-cap blocking for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime zero-risk-delta no-touch v597-v604 note:

V597-V604 adds focused tests/docs proving zero-risk-delta blocking for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime candidate-filter no-touch v605-v612 note:

V605-V612 adds focused tests/docs proving candidate-filter no-touch behavior for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime tie-break no-touch v613-v620 note:

V613-V620 adds focused tests/docs proving deterministic tie-break no-touch behavior for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, tie-break ledger, ordering ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime pool tie-break no-touch v621-v628 note:

V621-V628 adds focused tests/docs proving deterministic active-pool tie-break no-touch behavior for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pool tie-break ledger, settlement ordering ledger, active-pool ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime score-ordering no-touch v629-v636 note:

V629-V636 adds focused tests/docs proving deterministic candidate score-ordering no-touch behavior for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, score-order ledger, candidate-rank ledger, ordering ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime pool-priority no-touch v637-v644 note:

V637-V644 adds focused tests/docs proving active-pool priority before cross-pool household score no-touch behavior for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pool-priority ledger, cross-pool score ledger, active-pool ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.
Current household mobility runtime per-pool cap no-touch v645-v652 note:

V645-V652 adds focused tests/docs proving household cap application is per selected active pool for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, per-pool cap ledger, global household cap ledger, active-pool ledger, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.

Current household mobility runtime threshold-event no-touch v653-v660 note:

V653-V660 adds focused tests/docs proving threshold-event no-touch behavior for the first `PopulationAndHouseholds` household mobility runtime rule. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, threshold-event ledger, event-routing ledger, migration-started selector state, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.

Current household mobility runtime event-metadata no-prose v661-v668 note:

V661-V668 adds focused tests/docs proving structured event metadata, not event prose, carries cause, settlement id, and household id for the first runtime rule's threshold event. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, event-metadata ledger, prose-parsing ledger, event-routing ledger, migration-started selector state, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.

Current household mobility runtime event-metadata replay v669-v676 note:

V669-V676 adds focused tests/docs proving same-seed replay stability for the first runtime rule's selected threshold-event metadata signature. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, event-replay state, event-metadata ledger, event-routing ledger, migration-started selector state, diagnostic state, performance cache, rules-data file, loader, or save-manifest change is added.

Current household mobility runtime threshold extraction v677-v684 note:

V677-V684 extracts the selected-household runtime `MigrationStarted` event threshold into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted threshold field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, threshold-extraction state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime candidate-floor extraction v685-v692 note:

V685-V692 extracts the runtime candidate migration-risk floor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted candidate-floor field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, candidate-floor state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime score-weight extraction v693-v700 note:

V693-V700 extracts the runtime migration-risk score weight into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted score-weight field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, score-weight state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime labor-floor extraction v701-v708 note:

V701-V708 extracts the runtime labor-capacity pressure floor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted labor-floor field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, labor-floor state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime grain-floor extraction v709-v716 note:

V709-V716 extracts the runtime grain-store pressure floor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-floor field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, grain-floor state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime land-floor extraction v717-v724 note:

V717-V724 extracts the runtime land-holding pressure floor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted land-floor field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, land-floor state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime grain-divisor extraction v725-v732 note:

V725-V732 extracts the runtime grain-store pressure divisor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-divisor field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, grain-divisor state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime land-divisor extraction v733-v740 note:

V733-V740 extracts the runtime land-holding pressure divisor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted land-divisor field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, land-divisor state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime candidate-ceiling extraction v741-v748 note:

V741-V748 extracts the runtime high-risk candidate ceiling into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted candidate-ceiling field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, candidate-ceiling state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime distress-trigger extraction v749-v756 note:

V749-V756 extracts the runtime distress trigger threshold into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted distress-trigger field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, distress-trigger state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime debt-trigger extraction v757-v764 note:

V757-V764 extracts the runtime debt-pressure trigger threshold into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted debt-trigger field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, debt-trigger state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime labor-trigger extraction v765-v772 note:

V765-V772 extracts the runtime labor-capacity trigger ceiling into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted labor-trigger field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, labor-trigger state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime grain-trigger extraction v773-v780 note:

V773-V780 extracts the runtime grain-store trigger floor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-trigger field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, grain-trigger state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime land-trigger extraction v781-v788 note:

V781-V788 extracts the runtime land-holding trigger floor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted land-trigger field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, land-trigger state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime livelihood-trigger extraction v789-v796 note:

V789-V796 extracts the runtime trigger livelihood list into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted livelihood-trigger field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, livelihood-trigger state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime livelihood-score extraction v797-v804 note:

V797-V804 extracts the runtime livelihood score weights into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted livelihood-score field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, livelihood-score state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime pressure-score extraction v805-v812 note:

V805-V812 extracts the runtime distress/debt pressure score weights into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted pressure-score field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-score state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime migration-status extraction v813-v820 note:

V813-V820 extracts the runtime migration status threshold into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted migration-status field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, migration-status state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime migration-risk clamp extraction v821-v828 note:

V821-V828 extracts the runtime migration-risk clamp bounds into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted migration-risk clamp field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, risk-clamp state, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime tie-break priority extraction v829-v836 note:

V829-V836 extracts the runtime active-pool and household tie-break priorities into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted tie-break priority field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, tie-break state, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime unmatched livelihood score extraction v837-v844 note:

V837-V844 extracts the runtime unmatched-livelihood score fallback into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted unmatched-livelihood score field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, unmatched-livelihood state, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime pressure contribution floor extraction v845-v852 note:

V845-V852 extracts the runtime pressure contribution floor into owner-consumed in-code rules-data. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted pressure-contribution floor field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-contribution state, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current household mobility runtime extraction closeout v853-v860 note:

V853-V860 closes the first household mobility runtime rule hardcoded extraction track with docs/tests evidence only. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted closeout field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, remaining-guard state, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, file-split schema, or save-manifest change is added.

Current population households runtime rule file split v861-v868 note:

V861-V868 moves private owner-rule methods into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted file-split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, split-state marker, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households membership focus file split v869-v876 note:

V869-V876 moves private membership synchronization and focus-promotion helpers into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted membership-focus split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, focus ledger, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households pool rebuild file split v877-v884 note:

V877-V884 moves private settlement summary and labor/marriage/migration pool rebuild helpers into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted pool-rebuild split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pool ledger, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households query surface file split v885-v892 note:

V885-V892 moves the private query implementation and clone helpers into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted query-surface split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, query ledger, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households pressure profile file split v893-v900 note:

V893-V900 moves private pressure profile computation helpers into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted pressure-profile split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, event-routing ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households event dispatch file split v901-v908 note:

V901-V908 moves private event-dispatch/application helpers into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted event-dispatch split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, event-dispatch ledger, event-routing ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households livelihood drift file split v909-v916 note:

V909-V916 moves private monthly livelihood drift helpers into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted livelihood-drift split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, livelihood-drift ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households monthly pulse file split v917-v924 note:

V917-V924 moves private xun/month pulse helpers into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted monthly-pulse split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, monthly-pulse ledger, migration-status ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households warfare aftermath file split v925-v932 note:

V925-V932 moves private warfare aftermath handling into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted warfare-aftermath split field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, warfare-aftermath ledger, campaign-aftermath ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households health lifecycle file split v933-v940 note:

V933-V940 moves private health lifecycle handling into a partial file. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted health-lifecycle split field, illness state beyond existing fields, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, health-lifecycle ledger, illness ledger, mortality ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households monthly pressure rules-data extraction v941-v948 note:

V941-V948 extracts monthly prosperity/security/clan-relief/drift thresholds into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted monthly-pressure field, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, monthly-pressure ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households grain price signal rules-data extraction v949-v956 note:

V949-V956 extracts grain-price shock signal fallback and clamp values into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-shock field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households grain price pressure clamp extraction v957-v964 note:

V957-V964 extracts grain-price price-pressure clamp bounds into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-price-pressure field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households grain price level band extraction v965-v972 note:

V965-V972 extracts grain-price level threshold/score bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-price-level field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households grain price jump band extraction v973-v980 note:

V973-V980 extracts grain-price jump threshold/score bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-price-jump field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households grain price market tightness band extraction v981-v988 note:

V981-V988 extracts grain-price market tightness threshold/score bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted market-tightness field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households subsistence market dependency extraction v989-v996 note:

V989-V996 extracts subsistence market dependency livelihood score weights into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted market-dependency field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.
Current population households subsistence labor capacity extraction v997-v1004 note:

V997-V1004 extracts subsistence labor-capacity pressure bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted subsistence-labor field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households subsistence dependent count extraction v1005-v1012 note:

V1005-V1012 extracts subsistence dependent-count pressure bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted dependent-count pressure field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households subsistence labor clamp extraction v1013-v1020 note:

V1013-V1020 extracts the final subsistence labor pressure clamp into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted subsistence-labor pressure field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households subsistence grain buffer extraction v1021-v1028 note:

V1021-V1028 extracts subsistence grain-buffer pressure bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted grain-buffer pressure field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households subsistence fragility distress extraction v1029-v1036 note:

V1029-V1036 extracts subsistence fragility distress pressure bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted fragility-distress pressure field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.

Current population households subsistence fragility debt extraction v1037-v1044 note:

V1037-V1044 extracts subsistence fragility debt pressure bands into in-code owner-consumed rules-data defaults and validation. Schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`; no persisted fragility-debt pressure field, pressure-profile state, household mobility state, route history, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or save-manifest change is added.
