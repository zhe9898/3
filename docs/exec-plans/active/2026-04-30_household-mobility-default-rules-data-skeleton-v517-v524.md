# Household Mobility Default Rules-Data Skeleton V517-V524

## Goal

V517-V524 defines the default rules-data skeleton contract for future household mobility extraction.

Because the current repo has no reusable owner-consumed runtime rules-data/content/config pattern, this PR remains a docs/tests-only skeleton contract. It does not add a default rules-data file, loader, validator implementation, runtime behavior, or save schema change.

## Scope In

- Record the pattern decision: no suitable reusable runtime rules-data pattern exists today.
- Define the future default skeleton shape without adding a file:
  - `ruleSetId`;
  - `schemaVersion`;
  - `ownerModule`;
  - `defaultFallbackPolicy`;
  - `parameterGroups`;
  - `validationResult`;
  - deterministic declaration order.
- Define future parameter groups:
  - `thresholdBands`;
  - `pressureWeights`;
  - `regionalModifiers`;
  - `eraScenarioModifiers`;
  - `recoveryDecayRates`;
  - `fanoutCaps`;
  - `tieBreakPriorities`.
- Keep fallback explicit: absent optional groups fall back to declared defaults; malformed required groups fail validation deterministically before runtime use.
- Add an architecture guard proving this pass adds no runtime plugin marketplace, no Application/UI/Unity config calculation, no schema/migration drift, and no `PersonRegistry` expansion.
- Update docs required by the household mobility track.

## Scope Out

- No runtime behavior change.
- No default rules-data file.
- No rules-data loader.
- No validator implementation.
- No config-backed runtime rule.
- No movement command.
- No household relocation, migration economy, route history, selector, target-cardinality state, owner-lane ledger, cooldown ledger, or durable movement residue.
- No class/status engine, zhuhu/kehu conversion, office-service route, or trade-attachment route.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application-calculated household mobility outcome.
- No UI/Unity-derived household movement or config calculation.
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

This pass changes no persisted module state, root envelope, module envelope, feature manifest, migration step, save manifest membership, projection cache, rules-data file, rules-data loader, validator, default skeleton file, or serialized read-model cache. `PopulationAndHouseholds` remains schema `3`.

The skeleton does not enter save. It is contract text only until a later owner-consumed implementation proves validation, fallback, runtime equivalence, and schema neutrality.

## Pattern decision

The tracked `content/` tree contains `content/authoring` and `content/generated`. It does not contain a reusable owner-consumed runtime rules-data contract, rules-data manifest, loader, deterministic validator, or household mobility default config pattern.

In short: no reusable runtime rules-data/content/config pattern exists for this pass.

Therefore V517-V524 does not create `content/rules-data`, does not introduce a loader, and does not add a validator. The future implementation must either first establish a small owner-consumed pattern with explicit validation, or continue docs/tests-only.

## Future default skeleton shape

The first actual default skeleton should be data-only and owner-consumed:

- `ruleSetId`: stable identifier such as a module-owned household mobility default set, not display text.
- `schemaVersion`: rules-data contract version, separate from save schema.
- `ownerModule`: `PopulationAndHouseholds`.
- `defaultFallbackPolicy`: declared behavior for absent optional values and malformed required values.
- `parameterGroups`: ordered groups for `thresholdBands`, `pressureWeights`, `regionalModifiers`, `eraScenarioModifiers`, `recoveryDecayRates`, `fanoutCaps`, and `tieBreakPriorities`.
- `validationResult`: deterministic, readable validation output that can report missing ids, duplicate ids, out-of-range numeric values, invalid ordering, unsupported schema versions, and illegal executable/plugin fields.
- deterministic declaration order: every list or priority table must define stable order before runtime use.

## Milestones

- [x] Add ExecPlan.
- [x] Add docs skeleton-contract evidence.
- [x] Add focused architecture guard.
- [x] Run focused architecture test.
- [x] Run build, diff, encoding scan, and full test lane.
- [ ] Commit, publish branch, open PR, wait for CI, and merge before V525-V532.

## Tests to add/update

- Add `Household_mobility_default_rules_data_skeleton_v517_v524_must_remain_docs_tests_only_without_loader_when_no_pattern_exists`.
- Run focused architecture test.
- Run `dotnet build Zongzu.sln --no-restore`.
- Run `git diff --check`.
- Run touched-file replacement-character scan.
- Run `dotnet test Zongzu.sln --no-build`.

## Completion evidence

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_default_rules_data_skeleton_v517_v524_must_remain_docs_tests_only_without_loader_when_no_pattern_exists"` passed.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Strict UTF-8 touched-file replacement-character scan passed with no U+FFFD in touched files.
- `dotnet test Zongzu.sln --no-build` passed.
- Schema/migration impact remains none; no persisted fields, save manifest entries, rules-data files, loaders, validators, default skeleton files, or migrations were added.

## Rollback / fallback plan

Revert the docs/tests commit. No save migration, runtime data rollback, content loader rollback, rules-data rollback, validator rollback, or schema rollback is required.

## Open questions

- V525-V532 should choose the first extraction only after an owner-consumed validation path exists; fanout cap remains the lowest-risk candidate.
- If no loader pattern is added before extraction, the extraction PR must either introduce a small owner-consumed validator or explicitly stop before runtime behavior changes.
