## Goal
- harden M3 local-conflict interaction so only explicitly activated response posture can feed suppression support into `OrderAndBanditry.Lite`
- remove brittle trace-only inference from the active response boundary
- extend runtime-only diagnostics with a heavier M3 stress slice
- refine post-MVP seam notes for warfare and black-route expansion without implementing those packs yet

## Scope in
- explicit `ConflictAndForce` response-state fields if needed
- built-in migration support for any `ConflictAndForce` schema change
- calm / standing-force non-leak tests
- heavier multi-settlement M3 diagnostics bootstrap and performance coverage
- docs updates for schema, boundaries, acceptance, and post-MVP seams

## Scope out
- no new player commands
- no new authority modules beyond the current M3 local-conflict slice
- no campaign board, warfare rules, black-market ledgers, or diplomacy implementation
- no Unity interaction expansion

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Modules.ConflictAndForce`
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Persistence`
- integration, persistence, and module tests
- schema and post-MVP planning docs

## Save/schema impact
- likely `ConflictAndForce` module schema bump if response activation/support becomes explicit owned state
- if schema changes, ship a built-in migration path for legacy M3 local-conflict saves
- keep root schema unchanged

## Determinism risk
- medium
- support gating and stress bootstrap changes can alter replay outputs
- mitigate with:
  - deterministic response-state calculation only
  - calm / non-leak module tests
  - local-conflict replay and diagnostics sweeps
  - full build/test verification

## Milestones
1. capture the response-boundary hardening slice in an ExecPlan
2. make conflict response activation explicit and tighten support gating
3. add migration coverage and boundary regression tests
4. add heavier stress diagnostics for M3 local-conflict interaction
5. update docs and verify

## Tests to add/update
- `ConflictAndForceModuleTests`
- `OrderAndBanditryModuleTests`
- `M2LiteIntegrationTests`
- `SaveMigrationPipelineTests`
- `SaveRoundtripTests`

## Rollback / fallback plan
- if explicit persisted response-state creates too much schema churn, keep the new semantics but compute them deterministically from current-month force/disorder state only
- if the stress bootstrap adds too much maintenance burden, keep it test-only and runtime-only rather than exposing it through presentation or save docs

## Open questions
- should activated response require both sufficient force posture and active local-conflict pressure, or is one enough
- how much post-MVP black-route / warfare planning should live in `POST_MVP_SCOPE.md` versus a dedicated preplan note

## Completion notes
- `ConflictAndForce` now owns explicit response activation/support state, so `OrderAndBanditry` no longer depends on trace-text inference to decide whether local force posture can relieve disorder.
- same-month order relief is now gated on active local-conflict response only; calm or standing-but-untriggered guards, escorts, and militia stay visible but do not leak route, disorder, or suppression relief.
- a heavier M3 stress bootstrap now seeds multiple settlement slices so runtime-only diagnostics can track larger local-conflict interaction pressure without expanding save compatibility surface.
- default `LoadM3LocalConflict` now carries a built-in `ConflictAndForce` `1 -> 2` migration path so legacy local-conflict saves upgrade cleanly into explicit response-state persistence.
- post-MVP seam notes now more clearly state that black-route systems extend `OrderAndBanditry` plus `TradeAndIndustry`, while warfare promotes `ConflictAndForce` posture into owned `WarfareCampaign` state rather than inventing a parallel force model.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`

## Save compatibility notes
- `ConflictAndForce` schema advanced from `1` to `2` to persist explicit response activation/support state.
- legacy local-conflict saves now migrate through the default local-conflict loader; root schema stayed unchanged.
