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
- runtime domain-event targeting metadata used by the event-handling seam is also non-persisted and does not extend save namespaces
- migration preparation must not mutate the caller's source save root; consistency reporting happens on cloned preparation data only

## Versioning rules
- root save uses one root version integer
- every module uses its own schema version integer
- incompatible module changes do not justify hidden root changes

## Current implemented module versions
- `PersonRegistry` uses namespace `PersonRegistry` with schema version `1`
- `WorldSettlements` uses namespace `WorldSettlements` with schema version `6`
- `FamilyCore` uses namespace `FamilyCore` with schema version `7`
- `PopulationAndHouseholds` uses namespace `PopulationAndHouseholds` with schema version `2`
- `SocialMemoryAndRelations` uses namespace `SocialMemoryAndRelations` with schema version `2`
- `EducationAndExams` uses namespace `EducationAndExams` with schema version `2`
- `TradeAndIndustry` uses namespace `TradeAndIndustry` with schema version `4`
- `PublicLifeAndRumor` uses namespace `PublicLifeAndRumor` with schema version `4` for the active county-public-life slice plus monthly-cadence, venue-channel, and channel-contention descriptors
- `OfficeAndCareer` uses namespace `OfficeAndCareer` with schema version `4` for the active governance-lite slice
- `NarrativeProjection` uses namespace `NarrativeProjection` with schema version `1`
- `OrderAndBanditry` uses namespace `OrderAndBanditry` with schema version `7`
- `ConflictAndForce` uses namespace `ConflictAndForce` with schema version `4` for active M3 local-conflict lite integration plus campaign-fallout persistence
- `WarfareCampaign` uses namespace `WarfareCampaign` with schema version `4` for the active campaign-lite slice (phase + aftermath docket projection)

## M2-lite default-state policy
- old saves without `EducationAndExams` or `TradeAndIndustry` load cleanly when those modules remain disabled in the feature manifest
- old M0-M1 saves must continue to load through the M2 loader when `EducationAndExams`, `TradeAndIndustry`, and `NarrativeProjection` remain disabled in the manifest
- enabling either M2-lite module requires creating its owned default state inside its own namespace only
- built-in default loaders now migrate legacy `FamilyCore` schema `1 -> 2 -> 3` by first backfilling lineage-conflict defaults, then by conservatively backfilling marriage/heir/mourning lifecycle defaults inside the family namespace only
- built-in default loaders now also migrate legacy `WorldSettlements` schema `1` saves to schema `2` by backfilling conservative settlement tiers inside the world-settlement namespace only
- `PublicLifeAndRumor` now defaults to schema `4` when enabled and owns its public-pulse plus monthly-cadence / venue-channel / channel-contention state inside its own namespace only
- old saves still load cleanly while `PublicLifeAndRumor` remains disabled in the manifest
- active M2 and later bootstrap paths may now enable `PublicLifeAndRumor`; that new envelope is intentional and documented rather than implicit schema drift
- built-in default loaders now migrate legacy `PublicLifeAndRumor` schema `1 -> 2 -> 3 -> 4` by first backfilling cadence code/label, crowd mix, and cadence summary, then by conservatively backfilling dominant-venue code plus channel metrics, then by backfilling official/street/road/prefecture/contention wording inside the public-life namespace only
- built-in default loaders now also migrate legacy `TradeAndIndustry` schema `1 -> 2 -> 3` by first backfilling gray-route ledgers, then by conservatively backfilling per-route blockage / seizure mirrors inside the same trade namespace only
- M2-lite explanation strings are module-owned authoritative traces and must roundtrip with the rest of the module state
- `NarrativeProjection` persists derived notification history only inside its own namespace and may be rebuilt later without rewriting foreign module state
- latest-month debug traces, warning lists, and module inspectors are non-persisted read models and must not require a root schema change
- explicit schema mismatches must fail load clearly instead of silently coercing incompatible module envelopes

## M3 local-conflict namespace policy
- `OrderAndBanditry` now has an order-enabled M3 bridge path and a conflict-enabled M3 local-conflict path; both seed module-owned settlement disorder state only when the feature is enabled
- old M2 saves still load cleanly while `OrderAndBanditry` remains disabled in the manifest
- legacy M3 order saves with `OrderAndBanditry` schema `1` now migrate through built-in `1 -> 2 -> 3 -> 4 -> 5 -> 6` steps that first backfill black-route pressure summaries, then conservatively reconstruct paper-compliance and implementation-drag fields, then backfill route-shielding and retaliation-risk summaries, then backfill empty intervention-receipt fields, then clamp one-month intervention-follow-through state inside the same namespace
- `ConflictAndForce` now has a conflict-enabled M3 local-conflict path and seeds module-owned settlement force posture plus explicit response activation/support fields only when the feature is enabled
- legacy M3 local-conflict saves with `ConflictAndForce` schema `1` now migrate through a built-in `1 -> 2` module step during default local-conflict load
- built-in migration now also upgrades legacy `ConflictAndForce` schema `2` saves to schema `3` by backfilling zero campaign-fatigue / escort-strain fields and empty fallout traces inside the same namespace
- no existing save gains `OrderAndBanditry` or `ConflictAndForce` envelopes implicitly unless the feature manifest enables them

## Governance-lite namespace policy
- `OfficeAndCareer` now has a dedicated governance-lite path that seeds its own office-career and jurisdiction-authority state only when the feature is enabled
- legacy governance-lite saves with `OfficeAndCareer` schema `1` now migrate through built-in `1 -> 2 -> 3` module steps during default governance-lite load
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
