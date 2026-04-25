# SCHEMA_NAMESPACE_RULES

This document defines how save data and state namespaces stay extensible.

## Root save layout
The save root should contain:
- root schema version
- feature manifest
- kernel/common state
- module state dictionary keyed by module name
- replay metadata
- debug metadata if applicable

Example conceptual shape:
```json
{
  "rootSchemaVersion": 1,
  "featureManifest": {
    "FamilyCore": "full",
    "EducationAndExams": "lite",
    "TradeAndIndustry": "lite",
    "OrderAndBanditry": "off"
  },
  "kernelState": { "...": "..." },
  "modules": {
    "WorldSettlements": { "moduleSchemaVersion": 2, "state": { } },
    "FamilyCore": { "moduleSchemaVersion": 3, "state": { } }
  }
}
```

## Namespace rules
- each module owns one top-level save namespace
- module namespaces may contain sub-namespaces
- no module writes fields into another module namespace
- common/core state stays small and stable

## Core vs module state
### Core state should contain only:
- typed ID allocators
- global date
- global RNG/replay metadata
- shared entity identity skeletons if needed
- enabled feature manifest

### Module state should contain:
- all feature-specific authoritative state
- module-local indexes
- module-local projections if persisted deliberately
- module-local version number

## Entity data pattern
Do not create a single fat `Person` or `Clan` save blob with every feature field inside.
Prefer:
- small core identity record
- module-owned per-entity state in module namespace

Example:
- core person identity in kernel/common
- exam progress in `EducationAndExams`
- trade role in `TradeAndIndustry`
- office rank in `OfficeAndCareer`
- outlaw risk in `OrderAndBanditry`
- force role in `ConflictAndForce`

## Migration rules
- root schema migrations are explicit and rare
- module schema migrations are explicit and local
- adding a module requires a default-state migration strategy for old saves
- removing or disabling a module requires a documented retention/cleanup policy

## Current migration seam behavior
- load currently runs through an explicit `SaveMigrationPipeline`
- same-version root and module schemas pass through unchanged
- root and module migrations can now be registered as explicit chained steps
- if no migration path is registered for a root or module schema jump, load fails explicitly with a migration error
- runtime debug snapshots, observability summaries, and latest-month diff traces remain outside the persisted compatibility surface
- runtime load-migration reports and hotspot summaries are explanatory overlays only; they do not extend root or module save namespaces
- runtime scale summaries, payload-footprint summaries, and migration-consistency warnings are also explanatory overlays only
- runtime domain-event targeting and cause metadata used by the event-handling seam is also non-persisted and does not extend save namespaces
- migration preparation must not mutate the caller's source save root; consistency reporting happens on cloned preparation data only

## Versioning rules
- root save uses one root version integer
- every module uses its own schema version integer
- incompatible module changes do not justify hidden root changes

## Current implemented module versions
- `PersonRegistry` uses namespace `PersonRegistry` with schema version `1`
- `WorldSettlements` uses namespace `WorldSettlements` with schema version `8` for the active world-settlement slice plus chain-6 flood-disaster and chain-5 frontier-strain declaration watermark state
- `FamilyCore` uses namespace `FamilyCore` with schema version `8`
- `PopulationAndHouseholds` uses namespace `PopulationAndHouseholds` with schema version `3`
- `SocialMemoryAndRelations` uses namespace `SocialMemoryAndRelations` with schema version `3` for clan emotional climate, person pressure-tempering ledgers, public-life order accepted/refused/partial/response residue, later response-residue decay/hardening, v8 actor-countermove back-pressure inputs, and v13 home-household local response residue stored in existing memory/narrative/climate records
- `EducationAndExams` uses namespace `EducationAndExams` with schema version `2`
- `TradeAndIndustry` uses namespace `TradeAndIndustry` with schema version `4`
- `PublicLifeAndRumor` uses namespace `PublicLifeAndRumor` with schema version `4` for the active county-public-life slice plus monthly-cadence, venue-channel, and channel-contention descriptors
- `OfficeAndCareer` uses namespace `OfficeAndCareer` with schema version `7` for the active governance-lite slice plus chain-4 amnesty de-duplication state, chain-7 clerk-capture edge state, chain-9 official-defection risk state, and office-owned public-life refusal response trace state
- `NarrativeProjection` uses namespace `NarrativeProjection` with schema version `1`
- `OrderAndBanditry` uses namespace `OrderAndBanditry` with schema version `9`
- `ConflictAndForce` uses namespace `ConflictAndForce` with schema version `4` for active M3 local-conflict lite integration plus campaign-fallout persistence
- `WarfareCampaign` uses namespace `WarfareCampaign` with schema version `4` for the active campaign-lite slice (phase + aftermath docket projection)

## M2-lite default-state policy
- old saves without `EducationAndExams` or `TradeAndIndustry` load cleanly when those modules remain disabled in the feature manifest
- old M0-M1 saves must continue to load through the M2 loader when `EducationAndExams`, `TradeAndIndustry`, and `NarrativeProjection` remain disabled in the manifest
- enabling either M2-lite module requires creating its owned default state inside its own namespace only
- built-in default loaders now migrate legacy `FamilyCore` schemas through `8` by first backfilling lineage-conflict defaults, then conservatively backfilling marriage/heir/mourning lifecycle defaults, then adding family-owned public-life refusal response trace fields inside the family namespace only
- built-in default loaders now also migrate legacy `WorldSettlements` schema `1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8` inside the world-settlement namespace only; schema `7` backfills `LastDeclaredFloodDisasterBand = 0`, and schema `8` backfills `LastDeclaredFrontierStrainBand = 0` so old saves do not repeat a past flood or frontier declaration after load
- `PublicLifeAndRumor` now defaults to schema `4` when enabled and owns its public-pulse plus monthly-cadence / venue-channel / channel-contention state inside its own namespace only
- old saves still load cleanly while `PublicLifeAndRumor` remains disabled in the manifest
- active M2 and later bootstrap paths may now enable `PublicLifeAndRumor`; that new envelope is intentional and documented rather than implicit schema drift
- built-in default loaders now migrate legacy `PublicLifeAndRumor` schema `1 -> 2 -> 3 -> 4` by first backfilling cadence code/label, crowd mix, and cadence summary, then by conservatively backfilling dominant-venue code plus channel metrics, then by backfilling official/street/road/prefecture/contention wording inside the public-life namespace only
- built-in default loaders now also migrate legacy `TradeAndIndustry` schema `1 -> 2 -> 3` by first backfilling gray-route ledgers, then by conservatively backfilling per-route blockage / seizure mirrors inside the same trade namespace only
- built-in default loaders now also migrate legacy `SocialMemoryAndRelations` schema `1 -> 2 -> 3` by first classifying legacy memory records, then by backfilling clan emotional climate records from existing clan narratives inside the same social-memory namespace only
- built-in default loaders now also migrate legacy `PopulationAndHouseholds` schema `2 -> 3` by initializing home-household local response traces inside the population namespace only
- public-life order residue v4/v5/v6/v7/v8/v10/v11/v12/v13 does not add a new SocialMemory field or module envelope; it writes, adjusts, reads, or projects records inside existing schema `3` collections and runtime read-models, and therefore requires no `3 -> 4` migration. v6 response aftermath instead adds structured response trace fields to the actual owning modules (`FamilyCore` `7 -> 8`, `OfficeAndCareer` `6 -> 7`, `OrderAndBanditry` `8 -> 9`). v7 response-residue decay and repeat friction reuse existing SocialMemory `Weight`, `MonthlyDecay`, `LifecycleState`, `CauseKey`, narrative, and climate fields. v8 actor countermoves read existing `SocialMemoryEntrySnapshot` cause keys, weights, lifecycle state, source clan, and origin date, then write only existing owner-module response trace fields; they add no fields and require no migration. v10 ordinary-household readback adds only runtime `HouseholdSocialPressure` keys and no module state. v11 ordinary-household play-surface enrichment reuses those runtime projections plus `PlayerCommandAffordanceSnapshot` / `PlayerCommandReceiptSnapshot` strings and adds no module state. v12 home-household local response adds population-owned local response trace fields in `PopulationAndHouseholds` schema `3`, not SocialMemory fields, and requires the `2 -> 3` module migration. v13 reads those structured population fields into existing SocialMemory `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`; it adds no fields and requires no migration. Any later residue work that adds SocialMemory fields, household authority fields outside `PopulationAndHouseholds`, indexes, command target fields, or namespaces must bump the owning module schema and add an explicit migration.
- M2-lite explanation strings are module-owned authoritative traces and must roundtrip with the rest of the module state
- `NarrativeProjection` persists derived notification history only inside its own namespace and may be rebuilt later without rewriting foreign module state
- latest-month debug traces, warning lists, and module inspectors are non-persisted read models and must not require a root schema change
- explicit schema mismatches must fail load clearly instead of silently coercing incompatible module envelopes

## M3 local-conflict namespace policy
- `OrderAndBanditry` now has an order-enabled M3 bridge path and a conflict-enabled M3 local-conflict path; both seed module-owned settlement disorder state only when the feature is enabled
- old M2 saves still load cleanly while `OrderAndBanditry` remains disabled in the manifest
- legacy M3 order saves with `OrderAndBanditry` schema `1` now migrate through built-in `1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8 -> 9` steps that first backfill black-route pressure summaries, then conservatively reconstruct paper-compliance and implementation-drag fields, then backfill route-shielding and retaliation-risk summaries, then backfill empty intervention-receipt fields, then clamp one-month intervention-follow-through state, then backfill structured outcome/refusal/partial trace fields, then add structured refusal-response trace fields inside the same namespace
- public-life order residue v4 adds only query/read-model exposure for existing order aftermath fields; v5 adds `OrderAndBanditry` persisted structured trace fields (`LastInterventionOutcomeCode`, `LastInterventionRefusalCode`, `LastInterventionPartialCode`, `LastInterventionTraceCode`, `RefusalCarryoverMonths`) and a same-namespace `7 -> 8` migration
- public-life order response v6 adds `OrderAndBanditry` persisted response fields (`LastRefusalResponseCommandCode`, `LastRefusalResponseCommandLabel`, `LastRefusalResponseSummary`, `LastRefusalResponseOutcomeCode`, `LastRefusalResponseTraceCode`, `ResponseCarryoverMonths`) and a same-namespace `8 -> 9` migration for order-owned road watch / route-pressure repair responses
- public-life order actor countermove v8 adds trace-code constants only. `OrderAndBanditry` remains schema `9`, `OfficeAndCareer` remains schema `7`, `FamilyCore` remains schema `8`, and `SocialMemoryAndRelations` remains schema `3`; the countermoves reuse the v6 response fields and existing SocialMemory records.
- public-life order ordinary-household readback/play-surface v10/v11 adds runtime read-model constants / projection enrichment only. v12 adds `PopulationAndHouseholds` persisted local response trace fields (`LastLocalResponseCommandCode`, `LastLocalResponseCommandLabel`, `LastLocalResponseOutcomeCode`, `LastLocalResponseTraceCode`, `LastLocalResponseSummary`, `LocalResponseCarryoverMonths`) and a same-namespace `2 -> 3` migration for home-household local commands. `OrderAndBanditry` remains schema `9`, `OfficeAndCareer` remains schema `7`, `FamilyCore` remains schema `8`, and `SocialMemoryAndRelations` remains schema `3`.
- public-life order home-household social-memory readback v13 adds no persisted fields: `SocialMemoryAndRelations` remains schema `3` and reads structured `PopulationAndHouseholds` local response aftermath through queries, while `PopulationAndHouseholds` remains schema `3`.
- `ConflictAndForce` now has a conflict-enabled M3 local-conflict path and seeds module-owned settlement force posture plus explicit response activation/support fields only when the feature is enabled
- legacy M3 local-conflict saves with `ConflictAndForce` schema `1` now migrate through a built-in `1 -> 2` module step during default local-conflict load
- built-in migration now also upgrades legacy `ConflictAndForce` schema `2` saves to schema `3` by backfilling zero campaign-fatigue / escort-strain fields and empty fallout traces inside the same namespace
- no existing save gains `OrderAndBanditry` or `ConflictAndForce` envelopes implicitly unless the feature manifest enables them

## Governance-lite namespace policy
- `OfficeAndCareer` now has a dedicated governance-lite path that seeds its own office-career and jurisdiction-authority state only when the feature is enabled
- legacy governance-lite saves with `OfficeAndCareer` schema `1` now migrate through built-in `1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7` module steps during default governance-lite load; `6 -> 7` adds office-owned public-life refusal response trace fields for yamen/document/clerk-delay handling
- old M2 and M3 saves still load cleanly while `OfficeAndCareer` remains disabled in their manifests
- no existing stable M2/M3 path gains an `OfficeAndCareer` envelope implicitly; only the governance-lite path does

## Campaign-lite namespace policy
- `WarfareCampaign` now has a dedicated campaign-enabled path that seeds its own campaign-board and mobilization-signal state only when the feature is enabled
- old M2, M3, and governance-lite saves still load cleanly while `WarfareCampaign` remains disabled in their manifests
- no existing stable path gains a `WarfareCampaign` envelope implicitly; only the campaign-enabled path does
- built-in migration now upgrades legacy campaign-enabled saves from schema `1` to `2` to `3` by reconstructing board labels, command-fit wording, commander summaries, bounded route descriptors, and directive descriptors inside the same namespace

## Post-MVP preflight namespace policy
- black-route depth must not create a standalone `BlackRoute` namespace; current save data for that slice already stays inside `OrderAndBanditry` and `TradeAndIndustry`
- any future black-route migration steps must therefore preserve the same module-key set unless an explicit module-addition migration introduces `WarfareCampaign` or another documented pack

## Success criteria
A new module is schema-ready only if:
- it has a namespace
- it has a version
- it has default-state creation
- it has migration notes
- it does not require rewriting unrelated module blobs
