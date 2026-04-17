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
  "rootSchemaVersion": 3,
  "featureManifest": {
    "FamilyCore": "full",
    "EducationAndExams": "lite",
    "TradeAndIndustry": "lite",
    "OrderAndBanditry": "off"
  },
  "kernelState": { "...": "..." },
  "modules": {
    "WorldSettlements": { "moduleSchemaVersion": 2, "state": { } },
    "FamilyCore": { "moduleSchemaVersion": 4, "state": { } }
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

## Versioning rules
- root save uses one root version integer
- every module uses its own schema version integer
- incompatible module changes do not justify hidden root changes

## Success criteria
A new module is schema-ready only if:
- it has a namespace
- it has a version
- it has default-state creation
- it has migration notes
- it does not require rewriting unrelated module blobs
