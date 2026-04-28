---
name: zongzu-save-and-schema
description: "Use when changing, reviewing, or documenting Zongzu persisted save data, module state schemas, schema namespace rules, root save versions, module schema versions, migrations, manifests, feature-pack save membership, serialization boundaries, or persistence tests. Do not use for read-only projections, docs, UI copy, or tests with no persisted state."
---

# Zongzu Save and Schema

## Overview

Use this skill to keep Zongzu saves explicit, versioned, deterministic, and module-owned.

The central rule: every module owns its state namespace, and every persisted shape change needs schema documentation, migration behavior, and tests.

Use it only when persistence is real. If a change touches read models, adapters, docs, debug output, or helper methods without changing persisted state or manifest membership, record "no save/schema impact" and stop.

## Current Repo Anchors

Read current schema versions from `docs/SCHEMA_NAMESPACE_RULES.md` and live module `ModuleSchemaVersion`; do not copy remembered version numbers into answers.

Current no-save-impact examples:
- adding read-only methods to snapshot/read-model records
- normalizing notification settlement/module scope traversal over already-built traces
- adding or displaying player-command leverage/cost/readback/follow-up strings when they remain runtime read-model fields
- changing `Zongzu.Presentation.Unity` adapters or Unity scene/binding code without adding persisted data
- updating docs, skills, or projection wording that is not saved as module authority

Current save-impact examples:
- adding module-owned state fields, watermarks, pressure ledgers, command receipts, migration state, or feature-pack manifest membership
- persisting public-life/order command aftermath such as accepted/partial/refused traces, response traces, local household response traces, refusal carryover counters, or durable residue fields
- changing `NarrativeProjection` persisted notification history shape
- changing module defaults in a way that affects save/load or deterministic replay

Current public-life/order schema anchors:
- as of the current public-life order branch, `OrderAndBanditry` schema `9` owns structured accepted/partial/refused command traces, refusal carryover, and owner-side order response trace fields
- `OfficeAndCareer` schema `7` and `FamilyCore` schema `8` own their response trace fields when their lanes resolve public-life/order pressure
- `PopulationAndHouseholds` schema `3` owns home-household local response command/aftermath fields
- `SocialMemoryAndRelations` schema `3` owns durable social residue; later readback paths use existing memory/narrative/climate state unless a new persisted SocialMemory field is added
- current v19-v460 follow-up, owner-lane return/status/outcome/residue/no-loop, Office/Family/Force/Warfare/Court readback, directive/aftermath docket readback, court-policy local-response/SocialMemory/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/public-life-receipt echo guard, social mobility / fidelity / influence readbacks, regime-legitimacy readback, personnel-flow readiness/gate/future-lane preflight wording, commoner/social-position readback, source-key readback, scale-budget summaries, household mobility dynamics explanation, and closeout audit fields are runtime projections or docs/tests unless explicitly backed by an owner module field; they remain no-save/no-schema unless a future cooldown ledger, owner-lane ledger, policy ledger, command queue, receipt ledger, docket ledger, repeat counter, status field, target field, persisted projection cache, or other new persisted field is introduced
- current v35-v460 handoffs/readbacks/audits are no-save/no-schema unless persisted authority is explicitly added: canal-window metadata is runtime-only, v36 family sponsor pressure and v61-v68 relief reuse `FamilyCore` schema `8`, v37-v52 Office/yamen implementation/readback and Office-lane closure reuse `OfficeAndCareer` schema `7`, v69-v92 force/warfare readbacks reuse existing `ConflictAndForce` schema `4` and `WarfareCampaign` schema `4`, v93-v204 court-policy readbacks reuse `OfficeAndCareer` schema `7`, `PublicLifeAndRumor` schema `4`, and `SocialMemoryAndRelations` schema `3`, v213-v292 social mobility uses existing `PopulationAndHouseholds` schema `3` and `PersonRegistry` schema `1`, v253-v268 regime readback adds no schema, v293-v380 personnel-flow readiness/gate/future-lane preflight/closeout adds no schema, v381-v452 social-position/source-key/scale-budget/fidelity preflight adds no schema, and v453-v460 household mobility dynamics explanation adds no schema
- performance caches, projection indexes, Unity pooled objects, debug counters, and read-model traversal helpers are no-save/no-schema unless their data is persisted as module-owned authority
- migration/load cost is a real schema concern: large new histories, denormalized readback ledgers, or content inventories need bounded payload shape, deterministic defaults, and migration tests

## External Calibration Anchors

Use outside engineering guidance to sharpen persistence discipline:
- .NET diagnostics and performance guidance apply to save load, migration, replay hash refresh, and large manifest/content reads when CPU, GC, allocation, or payload size is the risk.
- High-performance logging can help hot migration diagnostics, but logs do not become save facts, player receipts, or migration authority.
- Unity pooling and asset caches are presentation concerns unless data is serialized into a module-owned namespace; pooled objects are never save state.
- Accessibility/ViewModel labels are runtime presentation fields unless explicitly persisted by an owning module with schema, migration, and round-trip tests.

## Use This Skill When

- a diff adds, removes, renames, or retypes persisted module state
- schema versions, root save envelopes, migrations, manifests, or feature-pack save membership change
- authored config becomes rules-data read by code
- a performance cache, projection index, or content inventory might be persisted
- load, migration, replay hash, or save round-trip compatibility is at risk
- a task needs an explicit "no save/schema impact" call

## Fast Lane

For read-only projection, docs, UI copy, adapter, or skill edits, record "no save/schema impact" and do not expand into migration work. Use a full schema pass only when persisted shape, manifest membership, defaults/backfill, or load/migration behavior changes.

## Workflow

1. Decide whether this is actually a save/schema task.

   Trigger this skill when the diff touches:
   - module state shape
   - schema version
   - save envelope or migration code
   - feature-pack manifest membership
   - persisted authored config
   - persistence tests

   Do not expand a read-only projection or presentation change into a schema task unless persisted data changed.

2. Read the save contract needed.

   Start with:
   - `docs/SCHEMA_NAMESPACE_RULES.md`
   - `docs/DATA_SCHEMA.md`
   - `docs/MODULE_BOUNDARIES.md`
   - the active ExecPlan for the touched module or feature pack

   Add architecture, integration, extensibility, or engineering docs only if the change crosses those boundaries.

3. Classify the change.

   Determine whether this is:
   - a new module namespace
   - a module state field addition/removal/rename
   - a module schema version bump
   - a root save version change
   - a manifest/feature-pack membership change
   - a migration pipeline change
   - an authored content/config change
   - a debug/projection-only change with no save impact
   - a performance cache/index that must either be recomputed or explicitly persisted with schema rules

4. Inspect current implementation facts.

   Check:
   - module `StateNamespace` and `ModuleSchemaVersion`
   - module state records/classes and their default initialization
   - `SimulationBootstrapper` manifests and module packs
   - persistence envelope, migration pipeline, and round-trip tests
   - relevant module tests and integration tests

5. Apply the schema rule.

   New module state requires:
   - entry in `docs/DATA_SCHEMA.md`
   - namespace/version rules in `docs/SCHEMA_NAMESPACE_RULES.md`
   - boundary entry in `docs/MODULE_BOUNDARIES.md`
   - integration notes in `docs/MODULE_INTEGRATION_RULES.md`
   - migration or explicit "new saves only" rationale
   - tests for load, save, migration, and deterministic defaults

   Feature-pack save membership requires:
   - manifest update
   - migration/load behavior for absent modules
   - tests for enabled and disabled paths

   Performance/load additions require:
   - reason the data cannot be recomputed from authoritative state
   - payload-size and cardinality expectation
   - deterministic invalidation/backfill behavior
   - migration and round-trip tests when persisted

6. Validate compatibility.

   Use targeted persistence/module tests first, then `dotnet test Zongzu.sln --no-restore`.
   If simulation behavior changes because a migration backfills state, also use `zongzu-simulation-validation`.

## Output Rules

- Do not add state without naming its owning module and namespace.
- Do not mutate another module's state during migration except through explicit persistence-owned envelope migration rules.
- Do not bump schema versions casually; explain what older shape is being upgraded.
- Do not rely on runtime type discovery as the save contract.
- Do not store UI-only presentation shape as authoritative simulation state.
- Do not persist projection caches or performance indexes unless they are explicitly module-owned, versioned, invalidated, and tested.
- Do not persist an optimization just because recomputation is inconvenient; first name recompute cost, cardinality, invalidation point, deterministic backfill, and migration burden.
- Do not persist commoner/social-position/status detail just because a readback names it; first name the owner module, population scope, fidelity ring, schema version, migration/defaults, and no-touch behavior.
- Do not change feature-pack membership without load/manifest tests.
- Prefer conservative default backfills that preserve deterministic behavior.
- Do not add migration work where a simple "no persisted shape changed" note is the correct answer.
