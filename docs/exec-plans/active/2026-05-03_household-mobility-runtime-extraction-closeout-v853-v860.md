# Household Mobility Runtime Extraction Closeout V853-V860

## Purpose

V853-V860 closes the first household mobility runtime rule hardcoded extraction track.

This is a hardcoded extraction closeout and evidence map only. It records that the first `PopulationAndHouseholds` monthly household mobility runtime rule now reads its rule parameters from owner-consumed in-code `PopulationHouseholdMobilityRulesData`, while the remaining inline controls are structural guards and are not authored rules-data knobs.

Runtime behavior change: none.

## Scope

- Owner: `PopulationAndHouseholds`.
- Cadence: unchanged monthly owner pass.
- Target scope: unchanged active migration pools and eligible local households under existing deterministic caps and ordering.
- Runtime behavior under default rules-data: unchanged.
- Target schema/migration impact: none.

Extracted first-rule parameter families through V852:
- active-pool outflow threshold
- candidate migration-risk floor and ceiling
- distress, debt-pressure, labor-capacity, grain-store, and land-holding trigger thresholds
- trigger livelihoods
- livelihood score weights and unmatched livelihood fallback
- distress, debt-pressure, and migration-risk score weights
- pressure contribution floor
- labor, grain, and land pressure floors and divisors
- settlement and household fanout caps
- pool and household deterministic tie-break priorities
- migration-risk delta
- migration-risk clamp floor and ceiling
- migration status threshold
- migration-started event threshold

Remaining inline controls:
- zero or negative cap/delta no-op guards
- empty household and migration-pool no-op guards
- changed/no-changed return flow
- old-to-new migration-started threshold crossing comparison
- candidate boolean gate composition
- ordinary `return false`, `continue`, and `changed = true` control flow

These remaining inline controls are not authored rules-data knobs. Extracting them would turn rules-data into control-flow authority, widen the future contract beyond parameter data, or imply a runtime rule plugin path.

## Out of scope

- No second household mobility runtime rule.
- No rules-data loader.
- No rules-data file.
- No external authored rules-data file.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No reflection-heavy rule loading.
- No direct route-history.
- No household movement command.
- No migration economy.
- No class/status engine.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, tie-break ledger, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.
- No large-file split in this closeout PR.

## Affected modules

- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No runtime, Application, presentation, Unity, persistence, `PersonRegistry`, or module state source should change.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted hardcoded-extraction-closeout state, ordering ledger, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because V853-V860 changes no runtime code and adds no data input path.

The first runtime rule still uses deterministic active-pool ordering, candidate scoring, tie-break priorities, settlement and household caps, migration-risk delta, clamp, and edge-threshold event emission from owner-consumed rules-data. Remaining no-op guards and boolean flow stay inline as control flow rather than becoming configurable authority.

## Milestones

1. Add V853-V860 closeout ExecPlan.
2. Update required docs with first runtime rule extraction closeout markers.
3. Add architecture guard proving all extracted parameter families are represented in owner-consumed rules-data.
4. Classify remaining inline controls as non-extraction targets.
5. Prove no runtime behavior, schema, `PersonRegistry`, Application, UI, Unity, plugin, loader, movement, migration economy, or class/status drift.
6. Run focused architecture test, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_extraction_closeout_v853_v860_must_classify_remaining_guards_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests closeout commit. No save migration, runtime rollback, content migration, rules-data file rollback, event routing rollback, hardcoded-extraction-closeout state rollback, or production data rollback is required.

## Evidence log

- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_extraction_closeout_v853_v860_must_classify_remaining_guards_without_schema_or_authority_drift"`
  Result: 1 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 129 passed, 0 failed.
- Build passed:
  `dotnet build Zongzu.sln --no-restore`
  Result: 0 warnings, 0 errors.
- Whitespace check passed:
  `git diff --check`
- Encoding check passed:
  touched-file replacement-character scan
  Result: no replacement characters in touched files.
- Full no-build solution tests passed:
  `dotnet test Zongzu.sln --no-build`
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 79 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 129 passed.
- Save/schema impact remained none: no persisted field, schema version, migration, save-manifest membership, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, remaining-guard state, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, serialized projection cache, rules-data file, loader, or content/config namespace was added.
- Authority boundary remained unchanged: `PopulationAndHouseholds` remains the owner-side consumer of first-rule household mobility rules-data; remaining inline controls are classified as non-extraction control flow; Application/UI/Unity do not calculate household mobility outcomes; `PersonRegistry` is unchanged; V853-V860 adds no runtime plugin marketplace, arbitrary script rules, reflection-heavy rule loading, prose parsing, movement command, migration economy, class/status engine, second runtime rule, or large-file split.
