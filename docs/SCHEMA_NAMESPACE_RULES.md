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
    "WorldSettlements": { "moduleSchemaVersion": 1, "state": { } },
    "FamilyCore": { "moduleSchemaVersion": 1, "state": { } }
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
- if no migration path is registered for a root or module schema jump, load fails explicitly with a migration error
- runtime debug snapshots, observability summaries, and latest-month diff traces remain outside the persisted compatibility surface

## Versioning rules
- root save uses one root version integer
- every module uses its own schema version integer
- incompatible module changes do not justify hidden root changes

## Current implemented module versions
- `WorldSettlements` uses namespace `WorldSettlements` with schema version `1`
- `FamilyCore` uses namespace `FamilyCore` with schema version `1`
- `PopulationAndHouseholds` uses namespace `PopulationAndHouseholds` with schema version `1`
- `SocialMemoryAndRelations` uses namespace `SocialMemoryAndRelations` with schema version `1`
- `EducationAndExams` uses namespace `EducationAndExams` with schema version `1`
- `TradeAndIndustry` uses namespace `TradeAndIndustry` with schema version `1`
- `NarrativeProjection` uses namespace `NarrativeProjection` with schema version `1`
- `OrderAndBanditry` uses namespace `OrderAndBanditry` with schema version `1` for M3 preflight scaffolding
- `ConflictAndForce` uses namespace `ConflictAndForce` with schema version `1` for M3 preflight scaffolding

## M2-lite default-state policy
- old saves without `EducationAndExams` or `TradeAndIndustry` load cleanly when those modules remain disabled in the feature manifest
- old M0-M1 saves must continue to load through the M2 loader when `EducationAndExams`, `TradeAndIndustry`, and `NarrativeProjection` remain disabled in the manifest
- enabling either M2-lite module requires creating its owned default state inside its own namespace only
- M2-lite explanation strings are module-owned authoritative traces and must roundtrip with the rest of the module state
- `NarrativeProjection` persists derived notification history only inside its own namespace and may be rebuilt later without rewriting foreign module state
- latest-month debug traces, warning lists, and module inspectors are non-persisted read models and must not require a root schema change
- explicit schema mismatches must fail load clearly instead of silently coercing incompatible module envelopes

## M3 preflight namespace policy
- `OrderAndBanditry` and `ConflictAndForce` are schema-reserved and code-scaffolded before active M3 integration
- both modules currently default to empty module-owned state and no-op month execution
- neither module is included in current M2 bootstraps or manifests, so no existing save should gain those envelopes implicitly

## Success criteria
A new module is schema-ready only if:
- it has a namespace
- it has a version
- it has default-state creation
- it has migration notes
- it does not require rewriting unrelated module blobs
