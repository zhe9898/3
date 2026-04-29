# Household Mobility Rules-Data Contract And Validator Preflight V509-V516

## Goal

V509-V516 defines the future household mobility rules-data contract and validator preflight.

This is docs/tests-only contract preflight. It changes no runtime behavior, adds no loader, and changes no save schema. The contract exists so a later `PopulationAndHouseholds` owner-consumed rule can extract hardcoded thresholds, weights, modifiers, recovery/decay rates, fanout caps, and tie-break priorities without becoming a runtime plugin marketplace or an Application/UI/Unity rule path.

## Scope In

- Define the future rules-data contract requirements:
  - stable ids;
  - schema/version;
  - deterministic ordering;
  - default fallback;
  - validation errors;
  - owner-consumed only;
  - no UI/Application authority;
  - no arbitrary script/plugin execution.
- Document future parameter categories:
  - threshold bands;
  - pressure weights;
  - regional modifiers;
  - era/scenario modifiers;
  - recovery/decay rates;
  - fanout caps;
  - deterministic tie-break priorities.
- Record that the current repository has `content/authoring` and `content/generated`, but no reusable runtime rules-data/content/config pattern for this owner-consumed contract. Therefore V509-V516 stops at docs/tests-only contract preflight and does not add a loader.
- Add an architecture guard proving future rules-data is not a runtime plugin system and this PR adds no runtime authority.
- Update docs required by the household mobility track.

## Scope Out

- No runtime behavior change.
- No rules-data loader.
- No default rules-data file.
- No validator implementation.
- No movement command.
- No household relocation, migration economy, route history, selector, target-cardinality state, owner-lane ledger, cooldown ledger, or durable movement residue.
- No class/status engine, zhuhu/kehu conversion, office-service route, or trade-attachment route.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application-calculated household mobility outcome.
- No UI/Unity-derived household movement or rules-data calculation.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, docs text, or household mobility explanation prose.
- No authored runtime plugin marketplace, arbitrary script rule, runtime assembly load, dynamic assembly scan, reflection-heavy rule loading, or arbitrary code execution.
- No new persisted state. If a later implementation needs persisted state, stop and write a schema/migration plan first.

## Affected modules

- Runtime authority: none changed.
- Future owner lane: `PopulationAndHouseholds`.
- Code touched in this pass: architecture tests only.
- Docs touched: topology, social strata, fidelity model, design audit, module boundaries, integration rules, data schema, schema namespace rules, simulation, UI/presentation, acceptance tests, skill matrix, and this ExecPlan.

## Save/schema impact

Target schema/migration impact: none.

This pass changes no persisted module state, root envelope, module envelope, feature manifest, migration step, save manifest membership, projection cache, rules-data file, rules-data loader, validator, or serialized read-model cache. `PopulationAndHouseholds` remains schema `3`.

## Existing content/config pattern scan

The current tracked content surface contains `content/authoring` and `content/generated`. It does not contain an owner-consumed runtime rules-data contract, rules-data manifest, rules-data loader, deterministic validator, or household mobility default config pattern.

Because no reusable runtime rules-data/content/config pattern exists, V509-V516 does not invent one. The next implementation step must either stay docs/tests-only or first add a small, owner-consumed pattern with explicit validation, deterministic fallback, and no save/schema drift.

## Future contract

The future household mobility rules-data contract must require:

- stable ids: each rule set, parameter group, threshold band, modifier, and tie-break priority must have a stable identifier that does not depend on display text or ordering in prose;
- schema/version: rules-data must declare a schema and version so validation can reject or default unsupported shapes deterministically;
- deterministic ordering: arrays, maps, modifiers, and tie-breaks must have a declared stable order before use;
- default fallback: absent optional values must fall back to declared defaults, not ambient C# constants hidden in call sites;
- validation errors: malformed ids, duplicate ids, out-of-range values, unsupported schema versions, missing required defaults, unordered priorities, and illegal script/plugin fields must produce readable deterministic validation results;
- owner-consumed only: `PopulationAndHouseholds` may consume validated data for owner-owned rules; Application, UI, Unity, and other modules must not calculate household mobility outcomes from it;
- no UI/Application authority: presentation can display projected fields only and cannot read config to infer movement, status drift, route eligibility, or target selection;
- no arbitrary script/plugin execution: rules-data may contain values and declared priorities only, not executable scripts, runtime assemblies, reflection-loaded types, dynamic expressions, or plugin marketplace entries.

## Future parameter categories

- threshold bands: migration risk, distress, debt, labor, settlement prosperity/security, support, collapse, and focus-promotion bands;
- pressure weights: livelihood distress, debt/labor/migration deltas, subsistence/tax/official-supply/campaign components, and pool formula weights;
- regional modifiers: settlement/region-local multipliers or clamps that remain explicit and bounded;
- era/scenario modifiers: Renzong/Song tax, corvee, grain-price, official-supply, campaign aftermath, and climate/flood pressure calibration;
- recovery/decay rates: local response carryover, pressure recovery, illness/convalescence, livelihood recovery, and shock decay;
- fanout caps: monthly household, pool, and settlement candidate caps with deterministic overflow behavior;
- deterministic tie-break priorities: stable priority order by pressure score, household ID, settlement ID, pool ID, authored priority, and any later explicitly declared owner key.

## Milestones

- [x] Add ExecPlan.
- [x] Add docs rules-data contract/preflight evidence.
- [x] Add focused architecture guard.
- [x] Run focused architecture test.
- [x] Run build, diff, encoding scan, and full test lane.
- [ ] Commit, push, open PR, wait for CI, and merge before V517-V524.

## Tests to add/update

- Add `Household_mobility_rules_data_contract_v509_v516_must_stay_contract_preflight_not_runtime_plugin_system`.
- Run focused architecture test.
- Run `dotnet build Zongzu.sln --no-restore`.
- Run `git diff --check`.
- Run touched-file replacement-character scan.
- Run `dotnet test Zongzu.sln --no-build`.

## Completion evidence

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_rules_data_contract_v509_v516_must_stay_contract_preflight_not_runtime_plugin_system"` passed.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Strict UTF-8 touched-file replacement-character scan passed with no U+FFFD in touched files.
- `dotnet test Zongzu.sln --no-build` passed.
- Schema/migration impact remains none; no persisted fields, save manifest entries, rules-data files, loaders, validators, or migrations were added.

## Rollback / fallback plan

Revert the docs/tests commit. No save migration, runtime data rollback, content loader rollback, rules-data rollback, or schema rollback is required.

## Open questions

- Whether V517-V524 should remain docs/tests-only skeleton contract because no reusable runtime rules-data pattern exists today.
- If a later PR introduces a first owner-consumed validator, which minimal parameter should it validate first: fanout cap, threshold band, recovery/decay cap, or deterministic tie-break priority?
