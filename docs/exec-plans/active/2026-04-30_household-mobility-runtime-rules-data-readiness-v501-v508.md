# Household Mobility First Runtime Rule And Rules-Data Readiness V501-V508

## Goal

V501-V508 records the first runtime rule readiness map and hardcoded extraction map for household mobility.

This is a bridge from the v469-v492 owner-lane gate toward a later first real rule. It does not implement that rule. The target future rule remains small, `PopulationAndHouseholds`-owned, monthly-first, deterministic, bounded by fanout, and readable through near detail / far summary.

## Scope In

- Document the owner lane for the future first household mobility runtime rule: `PopulationAndHouseholds`.
- Document the owner-readable existing signals: household livelihood, member activity, distress, debt, labor, grain, land, migration risk, `IsMigrating`, local-response carryover, settlement summaries, labor pools, marriage pools, migration pools, settlement prosperity/security, clan support, grain-price/tax/office-supply/campaign aftermath metadata, and current household mobility projection readbacks.
- Document the cadence preference: monthly authority first; `xun` remains a projection/cadence note unless a later implementation plan proves otherwise.
- Document the target scope: player-near households, pressure-hit local households, active-region pools, and distant summaries.
- Document the no-touch scope: quiet households, off-scope settlements, distant pooled society, `PersonRegistry`, and Application/UI/Unity authority paths.
- Document the future fanout rule: a later runtime pass must set a deterministic monthly cap over household / pool / settlement candidates and must sort by stable pressure score plus stable ID tie-breaks before touching targets.
- Register current `PopulationAndHouseholds` hardcoded rule candidates that should be extracted into owner-consumed authored rules-data over time.
- Add an architecture guard proving this pass is readiness documentation only and adds no movement authority.

## Scope Out

- No full household migration system.
- No relocation or route-history model.
- No movement command.
- No migration economy engine.
- No class/status engine, zhuhu/kehu conversion, office-service route, trade-attachment route, or direct personnel-flow rule.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, or durable movement residue.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application-calculated household mobility outcome.
- No UI/Unity-derived household movement.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, docs text, or household mobility explanation prose.
- No authored runtime plugin marketplace, arbitrary script rule, runtime assembly load, or reflection-heavy rule loading.
- No new persisted state. If a later implementation needs persisted state, stop and write a schema/migration plan first.

## Affected modules

- Runtime authority: none changed.
- Future owner lane: `PopulationAndHouseholds`.
- Code touched in this pass: architecture tests only.
- Docs touched: topology, social strata, fidelity model, design audit, module boundaries, integration rules, data schema, schema namespace rules, simulation, UI/presentation, acceptance tests, skill matrix, and this ExecPlan.

## Save/schema impact

Target schema/migration impact: none.

This pass changes no persisted module state, root envelope, module envelope, feature manifest, migration step, save manifest membership, projection cache, rules-data loader, or serialized read-model cache. `PopulationAndHouseholds` remains schema `3`.

## Determinism risk

No runtime behavior changes in this PR.

The future rule must be deterministic by construction:
- monthly-first owner cadence;
- stable candidate ordering before cap;
- stable tie-breaks such as household ID, settlement ID, pool settlement ID, or declared authored priority;
- no unordered traversal as authority;
- no prose parsing as input;
- bounded fanout before mutation.

## First runtime rule readiness map

The future first rule may read only existing owner-consumable signals:
- Household state: `Livelihood`, `Distress`, `DebtPressure`, `LaborCapacity`, `MigrationRisk`, `IsMigrating`, `LandHolding`, `GrainStore`, `ToolCondition`, `ShelterQuality`, `DependentCount`, `LaborerCount`, and local-response carryover fields.
- Household membership: `Livelihood`, `Health`, `IllnessMonths`, `Activity`, and `HealthResilience`, only through `PopulationAndHouseholds` state/query ownership.
- Settlement summaries: `CommonerDistress`, `LaborSupply`, `MigrationPressure`, and `MilitiaPotential`.
- Pools: `LaborPools`, `MarriagePools`, and `MigrationPools` as settlement-level active-region summaries.
- External read-only signals already used by the owner: settlement prosperity/security, clan support reserve, grain-price metadata, tax/corvee window metadata, official supply metadata, and campaign aftermath metadata.
- Existing projection fields may display output later, but they must not drive authority.

The future first rule should not read or write route history, movement paths, status ledgers, class ledgers, `PersonRegistry` domain facts, Application-derived rankings, UI selections, Unity state, or prose.

## Target scope and no-touch boundary

Target scope:
- player-near households;
- pressure-hit local households;
- active-region labor/marriage/migration pools;
- distant summaries only.

No-touch boundary:
- quiet households with no declared pressure trigger;
- off-scope settlements outside the source pressure locus;
- distant pooled society unless explicitly promoted by a later plan;
- `PersonRegistry` except existing `FidelityRing` behavior already owned by existing code paths;
- Application/UI/Unity, which may route, assemble, and copy projected fields only.

## Fanout rule

V501-V508 does not set the runtime cap value. It records the required shape:
- every monthly rule must declare maximum households, pools, and settlements touched per pass;
- candidate ordering must be deterministic before capping;
- stable tie-breaks must be listed in docs and tests;
- distant summaries must remain aggregate readbacks unless a separate owner-lane plan promotes bounded detail;
- cap overflow must fall back to summary pressure, not hidden per-household state.

## Hardcoded extraction map

Current `PopulationAndHouseholds` hardcoded candidates to extract gradually into owner-consumed authored rules-data:

- Threshold bands: monthly prosperity/security/clan-support thresholds, xun debt/distress/security thresholds, event thresholds for debt/labor/migration/collapse, migration status threshold, livelihood drift thresholds, focus-promotion thresholds, command resolver texture/capacity thresholds, and tax/subsistence/official-supply profile bands.
- Weights: livelihood distress baseline weights, debt/labor/migration deltas, subsistence/tax/official-supply/campaign component weights, and pool formulas for wage level, match difficulty, outflow pressure, inflow pressure, and floating population.
- Caps and limits: `Math.Clamp(..., 0, 100)` pressure caps, labor seasonal surplus `-200..400`, wage level `20..120`, floating population `0..200`, focused member promotion `.Take(2)`, and existing mobility explanation dimension cap `.Take(4)`.
- Recovery / decay: local-response carryover month decrement, negative monthly pressure deltas when settlement/clan conditions recover, livelihood recovery branches, and illness-month decrement/convalescing recovery.
- Deterministic ordering: household ID ordering for monthly/xun/event passes, settlement ID ordering for summary/pool rebuilds, person ID ordering for memberships and hot-member promotion, and pressure score then household ID for local-response target choice.
- Regional assumptions: settlement-scoped metadata as the primary local pressure locus, no cross-settlement household relocation path, and distant society as pooled summary.
- Era/scenario assumptions: tax/corvee windows, grain price shock, official supply requisition, clan support reserve, and campaign aftermath are current Renzong/Song pressure carriers but remain C# constants today.
- Pool limits: labor, marriage, and migration pools aggregate per settlement and must not become hidden household selector or movement ledger state.

## Milestones

- [x] Add ExecPlan.
- [x] Add docs readiness/extraction-map evidence.
- [x] Add focused architecture guard.
- [x] Run focused architecture test.
- [x] Run build, diff, encoding scan, and full test lane.
- [ ] Commit, push, and open PR.

## Tests to add/update

- Add `Household_mobility_runtime_rules_data_readiness_v501_v508_must_remain_readiness_only_without_movement_authority`.
- Run focused architecture test.
- Run `dotnet build Zongzu.sln --no-restore`.
- Run `git diff --check`.
- Run touched-file replacement-character scan.
- Run `dotnet test Zongzu.sln --no-build`.

## Completion evidence

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_rules_data_readiness_v501_v508_must_remain_readiness_only_without_movement_authority"` passed.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Strict UTF-8 touched-file replacement-character scan passed with no U+FFFD in touched files.
- `dotnet test Zongzu.sln --no-build` passed.
- Schema/migration impact remains none; no persisted fields, save manifest entries, loaders, or migrations were added.

## Rollback / fallback plan

Revert the docs/tests commit. No save migration, runtime data rollback, content loader rollback, or schema rollback is required.

## Open questions

- Which single low-risk candidate should become the first extracted rule parameter in V525-V532: fanout cap, threshold band, recovery/decay cap, or tie-break priority?
- Whether the later first runtime rule can remain entirely runtime/read-model visible without persisted state. If not, implementation must stop before code lands and document schema/migration impact.
